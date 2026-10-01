using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MahyarFree.Models;
using MahyarFree.ViewModels;

namespace MahyarFree.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private bool _reallyExit = false;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            StateChanged += MainWindow_StateChanged;

            // Connect text binding to ViewModel
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Start pulse animation
            StartPulseAnimation();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.ConnectButtonText))
            {
                ConnectText.Text = _viewModel.ConnectButtonText;
            }
            if (e.PropertyName == nameof(MainViewModel.IsBusy))
            {
                LoadingOverlay.Visibility = _viewModel.IsBusy ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void StartPulseAnimation()
        {
            // Pulse glow on the main button
            var pulseAnimation = new DoubleAnimation
            {
                From = 25,
                To = 50,
                Duration = TimeSpan.FromSeconds(1.8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(pulseAnimation, ConnectButton);
            Storyboard.SetTargetProperty(pulseAnimation, new PropertyPath("(Button.Effect).(DropShadowEffect.BlurRadius)"));
            var sb = new Storyboard();
            sb.Children.Add(pulseAnimation);
            sb.Begin();

            // Pulse ring
            var ringAnimation = new DoubleAnimation
            {
                From = 180,
                To = 240,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever
            };
            var opacityAnimation = new DoubleAnimation
            {
                From = 0.6,
                To = 0,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(ringAnimation, PulseRing);
            Storyboard.SetTargetProperty(ringAnimation, new PropertyPath("(Border.Width)"));
            Storyboard.SetTarget(opacityAnimation, PulseRing);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(Border.Opacity)"));
            var ringSb = new Storyboard();
            ringSb.Children.Add(ringAnimation);
            ringSb.Children.Add(opacityAnimation);
            ringSb.Begin();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadInitialConfigsAsync();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!_reallyExit)
            {
                e.Cancel = true;
                Hide();
                TrayIcon.ShowBalloonTip("Mahyar Free", "Running in background. Click the icon to reopen.", BalloonIcon.Info);
            }
            else
            {
                TrayIcon.Visibility = Visibility.Collapsed;
                TrayIcon.Dispose();
                if (_viewModel.IsConnected)
                {
                    _ = _viewModel.DisconnectAsync();
                }
            }
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                TrayIcon.ShowBalloonTip("Mahyar Free", "Minimized to system tray.", BalloonIcon.Info);
            }
        }

        // Title bar drag
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        // Server item click
        private void ServerItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProxyConfig config)
            {
                _viewModel.SelectedConfig = config;
            }
        }

        // Connect button
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.ConnectAsync();
        }

        // Fetch button
        private async void FetchButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.FetchConfigsAsync();
        }

        // Test ping button
        private async void TestPingButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.TestAllPingsAsync();
        }

        // Auto-select best server
        private async void AutoSelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Configs.Count == 0)
            {
                MessageBox.Show("Please fetch configs first.", "Mahyar Free",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // If no ping has been tested, test all first
            if (!_viewModel.Configs.Any(c => c.Ping > 0))
            {
                await _viewModel.TestAllPingsAsync();
            }

            // Find best (lowest ping)
            ProxyConfig? best = null;
            foreach (var c in _viewModel.Configs)
            {
                if (c.Ping > 0 && (best == null || c.Ping < best.Ping))
                    best = c;
            }
            if (best != null)
            {
                _viewModel.SelectedConfig = best;
            }
            else
            {
                _viewModel.SelectedConfig = _viewModel.Configs[0];
            }
        }

        // Tray icon handlers
        private void TrayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ShowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private async void ConnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.ConnectAsync();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _reallyExit = true;
            Close();
        }
    }
}
