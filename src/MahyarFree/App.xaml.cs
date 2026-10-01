using System.Windows;
using System.Windows.Threading;

namespace MahyarFree
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Global exception handler
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will continue running.",
                "Mahyar Free - Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            e.Handled = true;
        }
    }
}
