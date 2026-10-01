using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace MahyarFree.Services
{
    public class SystemProxyService
    {
        public event EventHandler<string>? Log;

        public bool SetProxy(string proxyAddress, int port)
        {
            try
            {
                // Internet Explorer / System proxy
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
                {
                    if (key == null)
                    {
                        Log?.Invoke(this, "Cannot open Internet Settings registry key");
                        return false;
                    }

                    key.SetValue("ProxyEnable", 1);
                    key.SetValue("ProxyServer", $"{proxyAddress}:{port}");
                }

                // Refresh internet settings via WinINET
                RefreshInternetSettings();

                Log?.Invoke(this, $"System proxy set to {proxyAddress}:{port}");
                return true;
            }
            catch (Exception ex)
            {
                Log?.Invoke(this, $"Failed to set proxy: {ex.Message}");
                return false;
            }
        }

        public bool DisableProxy()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
                {
                    if (key == null) return false;
                    key.SetValue("ProxyEnable", 0);
                }
                RefreshInternetSettings();
                Log?.Invoke(this, "System proxy disabled");
                return true;
            }
            catch (Exception ex)
            {
                Log?.Invoke(this, $"Failed to disable proxy: {ex.Message}");
                return false;
            }
        }

        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);

        private void RefreshInternetSettings()
        {
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
    }
}
