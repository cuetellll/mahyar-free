using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MahyarFree.Models;
using Newtonsoft.Json;

namespace MahyarFree.Services
{
    public class SingBoxService
    {
        private Process? _singBoxProcess;
        private readonly string _singBoxPath;
        private readonly string _configPath;
        private TrafficStats _trafficStats = new();
        private CancellationTokenSource? _trafficCts;

        public event EventHandler<string>? Log;
        public event EventHandler<TrafficStats>? TrafficUpdated;
        public event EventHandler<bool>? ConnectionStateChanged;

        public bool IsRunning => _singBoxProcess != null && !_singBoxProcess.HasExited;
        public TrafficStats TrafficStats => _trafficStats;

        public SingBoxService()
        {
            var baseDir = AppContext.BaseDirectory;
            _singBoxPath = Path.Combine(baseDir, "bin", "sing-box.exe");
            _configPath = Path.Combine(baseDir, "bin", "config.json");
        }

        public async Task<bool> StartAsync(ProxyConfig config)
        {
            if (IsRunning)
            {
                Log?.Invoke(this, "sing-box is already running, stopping first...");
                await StopAsync();
            }

            if (!File.Exists(_singBoxPath))
            {
                Log?.Invoke(this, $"sing-box.exe not found at: {_singBoxPath}");
                return false;
            }

            try
            {
                var singBoxConfig = BuildSingBoxConfig(config);
                await File.WriteAllTextAsync(_configPath, singBoxConfig);
                Log?.Invoke(this, "Config written, starting sing-box...");

                var psi = new ProcessStartInfo
                {
                    FileName = _singBoxPath,
                    Arguments = $"run -c \"{_configPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _singBoxProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
                _singBoxProcess.OutputDataReceived += (s, e) => { if (e.Data != null) Log?.Invoke(this, $"[stdout] {e.Data}"); };
                _singBoxProcess.ErrorDataReceived += (s, e) => { if (e.Data != null) Log?.Invoke(this, $"[stderr] {e.Data}"); };
                _singBoxProcess.Exited += (s, e) =>
                {
                    Log?.Invoke(this, "sing-box process exited");
                    ConnectionStateChanged?.Invoke(this, false);
                };

                _singBoxProcess.Start();
                _singBoxProcess.BeginOutputReadLine();
                _singBoxProcess.BeginErrorReadLine();

                _trafficStats = new TrafficStats();
                _trafficCts = new CancellationTokenSource();
                _ = Task.Run(() => MonitorTrafficAsync(_trafficCts.Token));

                Log?.Invoke(this, $"sing-box started (PID: {_singBoxProcess.Id})");
                ConnectionStateChanged?.Invoke(this, true);
                return true;
            }
            catch (Exception ex)
            {
                Log?.Invoke(this, $"Failed to start sing-box: {ex.Message}");
                ConnectionStateChanged?.Invoke(this, false);
                return false;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                _trafficCts?.Cancel();
                if (_singBoxProcess != null && !_singBoxProcess.HasExited)
                {
                    Log?.Invoke(this, "Stopping sing-box...");
                    _singBoxProcess.Kill(true);
                    await _singBoxProcess.WaitForExitAsync();
                }
                _singBoxProcess?.Dispose();
                _singBoxProcess = null;
                Log?.Invoke(this, "sing-box stopped");
                ConnectionStateChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                Log?.Invoke(this, $"Error stopping sing-box: {ex.Message}");
            }
        }

        private string BuildSingBoxConfig(ProxyConfig config)
        {
            // ساخت outbound بر اساس نوع پروتکل
            dynamic outbound;
            switch (config.Protocol.ToLower())
            {
                case "vmess":
                    outbound = BuildVmessOutbound(config);
                    break;
                case "vless":
                    outbound = BuildVlessOutbound(config);
                    break;
                case "trojan":
                    outbound = BuildTrojanOutbound(config);
                    break;
                case "ss":
                case "shadowsocks":
                    outbound = BuildShadowsocksOutbound(config);
                    break;
                case "hysteria":
                case "hysteria2":
                    outbound = BuildHysteriaOutbound(config);
                    break;
                default:
                    throw new NotSupportedException($"Protocol {config.Protocol} not supported");
            }

            var sbConfig = new
            {
                log = new { level = "info", timestamp = true },
                inbounds = new object[]
                {
                    new
                    {
                        type = "mixed",
                        tag = "mixed-in",
                        listen = "127.0.0.1",
                        listen_port = 2080
                    }
                },
                outbounds = new object[] { outbound, new { type = "direct", tag = "direct" } },
                route = new { final = outbound.tag }
            };

            return JsonConvert.SerializeObject(sbConfig, Formatting.Indented);
        }

        private object BuildVmessOutbound(ProxyConfig config)
        {
            return new
            {
                type = "vmess",
                tag = "proxy",
                server = config.Address,
                server_port = config.Port,
                uuid = ExtractFromUri(config.RawUri, "uuid") ?? "",
                security = "auto",
                alter_id = 0
            };
        }

        private object BuildVlessOutbound(ProxyConfig config)
        {
            return new
            {
                type = "vless",
                tag = "proxy",
                server = config.Address,
                server_port = config.Port,
                uuid = ExtractFromUri(config.RawUri, "uuid") ?? "",
                flow = "",
                tls = new { enabled = false }
            };
        }

        private object BuildTrojanOutbound(ProxyConfig config)
        {
            return new
            {
                type = "trojan",
                tag = "proxy",
                server = config.Address,
                server_port = config.Port,
                password = ExtractFromUri(config.RawUri, "password") ?? ""
            };
        }

        private object BuildShadowsocksOutbound(ProxyConfig config)
        {
            return new
            {
                type = "shadowsocks",
                tag = "proxy",
                server = config.Address,
                server_port = config.Port,
                method = "chacha20-ietf-poly1305",
                password = ""
            };
        }

        private object BuildHysteriaOutbound(ProxyConfig config)
        {
            return new
            {
                type = "hysteria2",
                tag = "proxy",
                server = config.Address,
                server_port = config.Port,
                password = ExtractFromUri(config.RawUri, "password") ?? ""
            };
        }

        private string? ExtractFromUri(string uri, string key)
        {
            try
            {
                var queryIdx = uri.IndexOf('?');
                if (queryIdx < 0) return null;
                var query = uri.Substring(queryIdx + 1);
                foreach (var part in query.Split('&'))
                {
                    var kv = part.Split('=');
                    if (kv.Length == 2 && kv[0] == key)
                        return Uri.UnescapeDataString(kv[1]);
                }
            }
            catch { }
            return null;
        }

        private async Task MonitorTrafficAsync(CancellationToken token)
        {
            // این تابع در نسخه کامل از API sing-box استفاده می‌کند
            // فعلاً یک نمایش ساده داریم
            var random = new Random();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, token);
                    // شبیه‌سازی - در نسخه واقعی از stats API استفاده می‌شود
                    _trafficStats.CurrentDownloadSpeed = random.Next(50000, 5000000);
                    _trafficStats.CurrentUploadSpeed = random.Next(10000, 500000);
                    _trafficStats.TotalDownload += (long)_trafficStats.CurrentDownloadSpeed;
                    _trafficStats.TotalUpload += (long)_trafficStats.CurrentUploadSpeed;
                    TrafficUpdated?.Invoke(this, _trafficStats);
                }
                catch (OperationCanceledException) { break; }
                catch { }
            }
        }

        public string GetProxyEndpoint() => "http://127.0.0.1:2080";
    }
}
