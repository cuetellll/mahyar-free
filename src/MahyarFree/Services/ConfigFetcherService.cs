using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MahyarFree.Services
{
    public class ConfigFetcherService
    {
        // آدرس raw فایل کانفیگ‌ها در ریپازیتوری GitHub
        // شما می‌توانید محتوای فایل configs.txt را در ریپازیتوری خود به‌روزرسانی کنید
        private const string DefaultConfigUrl = "https://raw.githubusercontent.com/mahyar-free/mahyar-free/main/configs.txt";

        private readonly HttpClient _httpClient;
        private readonly string _localCacheFile;

        public event EventHandler<string>? Log;

        public ConfigFetcherService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MahyarFree/1.0");

            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MahyarFree");
            Directory.CreateDirectory(appData);
            _localCacheFile = Path.Combine(appData, "configs_cache.txt");
        }

        public string GetCachedContent()
        {
            try
            {
                if (File.Exists(_localCacheFile))
                    return File.ReadAllText(_localCacheFile);
            }
            catch (Exception ex) { Log?.Invoke(this, $"Cache read error: {ex.Message}"); }
            return string.Empty;
        }

        public async Task<string> FetchAsync(string? url = null)
        {
            var targetUrl = string.IsNullOrWhiteSpace(url) ? DefaultConfigUrl : url;
            Log?.Invoke(this, $"Fetching configs from: {targetUrl}");

            try
            {
                var response = await _httpClient.GetStringAsync(targetUrl);
                await SaveCacheAsync(response);
                Log?.Invoke(this, $"Configs fetched successfully ({response.Length} bytes)");
                return response;
            }
            catch (HttpRequestException ex)
            {
                Log?.Invoke(this, $"Network error: {ex.Message}");
                // اگر دانلود نشد، کش قبلی را برگردان
                var cached = GetCachedContent();
                if (!string.IsNullOrEmpty(cached))
                {
                    Log?.Invoke(this, "Using cached configs");
                    return cached;
                }
                throw;
            }
        }

        public async Task<string> FetchOrCacheAsync(string? url = null)
        {
            try
            {
                return await FetchAsync(url);
            }
            catch
            {
                var cached = GetCachedContent();
                if (string.IsNullOrEmpty(cached))
                    throw new Exception("Could not fetch configs and no cache available.");
                return cached;
            }
        }

        private async Task SaveCacheAsync(string content)
        {
            try
            {
                await File.WriteAllTextAsync(_localCacheFile, content);
            }
            catch (Exception ex)
            {
                Log?.Invoke(this, $"Cache save error: {ex.Message}");
            }
        }
    }
}
