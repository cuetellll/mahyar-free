using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MahyarFree.Models;
using MahyarFree.Services;

namespace MahyarFree.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly UriParserService _uriParser;
        private readonly ConfigFetcherService _configFetcher;
        private readonly PingService _pingService;
        private readonly SingBoxService _singBoxService;
        private readonly SystemProxyService _proxyService;

        private ProxyConfig? _selectedConfig;
        private bool _isConnected;
        private bool _isBusy;
        private string _statusText = "Disconnected";
        private string _statusColor = "#7C4DFF";
        private string _logText = string.Empty;
        private CancellationTokenSource? _pingCts;

        public ObservableCollection<ProxyConfig> Configs { get; } = new();
        public TrafficStats TrafficStats => _singBoxService.TrafficStats;

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConnectButtonText));
                OnPropertyChanged(nameof(ConnectButtonColor));
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotBusy)); }
        }

        public bool IsNotBusy => !_isBusy;

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public string StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        public string ConnectButtonText => IsConnected ? "DISCONNECT" : "CONNECT";

        public string ConnectButtonColor => IsConnected ? "#FF5252" : "#7C4DFF";

        public ProxyConfig? SelectedConfig
        {
            get => _selectedConfig;
            set
            {
                if (_selectedConfig != null) _selectedConfig.IsSelected = false;
                _selectedConfig = value;
                if (_selectedConfig != null) _selectedConfig.IsSelected = true;
                OnPropertyChanged();
            }
        }

        public string LogText
        {
            get => _logText;
            set { _logText = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            _uriParser = new UriParserService();
            _configFetcher = new ConfigFetcherService();
            _pingService = new PingService();
            _singBoxService = new SingBoxService();
            _proxyService = new SystemProxyService();

            _configFetcher.Log += (s, m) => AppendLog($"[Fetch] {m}");
            _pingService.Log += (s, m) => AppendLog($"[Ping] {m}");
            _singBoxService.Log += (s, m) => AppendLog($"[Core] {m}");
            _proxyService.Log += (s, m) => AppendLog($"[Proxy] {m}");
            _singBoxService.ConnectionStateChanged += (s, running) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    IsConnected = running;
                    StatusText = running ? "Connected" : "Disconnected";
                    StatusColor = running ? "#00E676" : "#7C4DFF";
                });
            };
            _singBoxService.TrafficUpdated += (s, stats) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(TrafficStats));
                });
            };
        }

        public async Task LoadInitialConfigsAsync()
        {
            IsBusy = true;
            AppendLog("Loading cached configs...");
            try
            {
                var content = _configFetcher.GetCachedContent();
                if (!string.IsNullOrEmpty(content))
                {
                    LoadConfigsFromContent(content);
                    AppendLog($"Loaded {Configs.Count} cached configs");
                }
            }
            catch (Exception ex) { AppendLog($"Error loading cache: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        public async Task FetchConfigsAsync()
        {
            IsBusy = true;
            AppendLog("Fetching latest configs from server...");
            try
            {
                var content = await _configFetcher.FetchOrCacheAsync();
                LoadConfigsFromContent(content);
                AppendLog($"✓ {Configs.Count} configs loaded");
            }
            catch (Exception ex)
            {
                AppendLog($"✗ Fetch failed: {ex.Message}");
            }
            finally { IsBusy = false; }
        }

        private void LoadConfigsFromContent(string content)
        {
            Configs.Clear();
            var list = _uriParser.ParseConfigFile(content);
            foreach (var c in list) Configs.Add(c);
        }

        public async Task TestAllPingsAsync()
        {
            if (Configs.Count == 0)
            {
                AppendLog("No configs to test");
                return;
            }

            _pingCts?.Cancel();
            _pingCts = new CancellationTokenSource();
            IsBusy = true;
            AppendLog($"Testing ping for {Configs.Count} servers...");

            await _pingService.PingAllAsync(
                Configs.ToList(),
                config => Application.Current?.Dispatcher.Invoke(() =>
                {
                    var idx = Configs.IndexOf(config);
                    if (idx >= 0) Configs[idx] = config;
                }),
                maxParallel: 20,
                pingTimeoutMs: 3000,
                _pingCts.Token);

            var tested = Configs.Count(c => c.Ping > 0);
            AppendLog($"✓ Tested {tested}/{Configs.Count} servers successfully");

            // اگر کاربر قبلاً کانفیگی انتخاب نکرده، بهترین را انتخاب کن
            if (SelectedConfig == null || SelectedConfig.Ping < 0)
            {
                var best = _pingService.GetBestConfig(Configs.ToList());
                if (best != null)
                {
                    SelectedConfig = best;
                    AppendLog($"Auto-selected best server: {best.Name} ({best.Ping}ms)");
                }
            }
            IsBusy = false;
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
            {
                await DisconnectAsync();
                return;
            }

            if (SelectedConfig == null)
            {
                // اگر انتخاب نشده، بهترین را پیدا کن
                var best = _pingService.GetBestConfig(Configs.ToList());
                if (best != null)
                {
                    SelectedConfig = best;
                    AppendLog($"Auto-selected best server: {best.Name} ({best.Ping}ms)");
                }
                else if (Configs.Count > 0)
                {
                    SelectedConfig = Configs[0];
                    AppendLog($"Using first config: {SelectedConfig.Name}");
                }
                else
                {
                    AppendLog("✗ No configs available. Please fetch configs first.");
                    return;
                }
            }

            // اگر پینگ گرفته نشده، سرور انتخاب‌شده را تست کن
            if (SelectedConfig.Ping < 0)
            {
                AppendLog($"Testing selected config: {SelectedConfig.Name}");
                SelectedConfig.IsTesting = true;
                var ping = await _pingService.TcpPingAsync(SelectedConfig.Address, SelectedConfig.Port);
                SelectedConfig.Ping = ping;
                SelectedConfig.IsTesting = false;
                if (ping < 0)
                {
                    AppendLog($"✗ Config {SelectedConfig.Name} is unreachable");
                    return;
                }
            }

            IsBusy = true;
            StatusText = "Connecting...";
            StatusColor = "#FFB300";

            var endpoint = _singBoxService.GetProxyEndpoint().Replace("http://", "");
            var parts = endpoint.Split(':');
            var started = await _singBoxService.StartAsync(SelectedConfig);

            if (started)
            {
                _proxyService.SetProxy(parts[0], int.Parse(parts[1]));
                AppendLog($"✓ Connected to {SelectedConfig.Name} ({SelectedConfig.Ping}ms)");
            }
            else
            {
                AppendLog("✗ Failed to start VPN core");
                StatusText = "Connection Failed";
                StatusColor = "#FF5252";
            }
            IsBusy = false;
        }

        public async Task DisconnectAsync()
        {
            IsBusy = true;
            AppendLog("Disconnecting...");
            _proxyService.DisableProxy();
            await _singBoxService.StopAsync();
            IsConnected = false;
            StatusText = "Disconnected";
            StatusColor = "#7C4DFF";
            AppendLog("✓ Disconnected");
            IsBusy = false;
        }

        private void AppendLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var line = $"[{timestamp}] {message}\n";
            Application.Current?.Dispatcher.Invoke(() =>
            {
                LogText += line;
                OnPropertyChanged(nameof(LogText));
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
