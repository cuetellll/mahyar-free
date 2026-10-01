using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MahyarFree.Models;
using Newtonsoft.Json;

namespace MahyarFree.Services
{
    public class UriParserService
    {
        public List<ProxyConfig> ParseConfigFile(string content)
        {
            var configs = new List<ProxyConfig>();
            if (string.IsNullOrWhiteSpace(content)) return configs;

            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int index = 1;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                try
                {
                    var config = ParseUri(line);
                    if (config != null)
                    {
                        config.Name = $"{config.Protocol.ToUpper()} #{index}";
                        configs.Add(config);
                        index++;
                    }
                }
                catch
                {
                    // Skip invalid line silently
                }
            }
            return configs;
        }

        public ProxyConfig? ParseUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) return null;

            uri = uri.Trim();
            if (uri.StartsWith("vmess://", StringComparison.OrdinalIgnoreCase))
                return ParseVmess(uri);
            if (uri.StartsWith("vless://", StringComparison.OrdinalIgnoreCase))
                return ParseVless(uri);
            if (uri.StartsWith("trojan://", StringComparison.OrdinalIgnoreCase))
                return ParseTrojan(uri);
            if (uri.StartsWith("ss://", StringComparison.OrdinalIgnoreCase))
                return ParseShadowsocks(uri);
            if (uri.StartsWith("hysteria://", StringComparison.OrdinalIgnoreCase) ||
                uri.StartsWith("hy2://", StringComparison.OrdinalIgnoreCase))
                return ParseHysteria(uri);

            return null;
        }

        private ProxyConfig? ParseVmess(string uri)
        {
            try
            {
                var b64 = uri.Substring(8);
                // Pad if needed
                int pad = 4 - (b64.Length % 4);
                if (pad != 4) b64 = b64.PadRight(b64.Length + pad, '=');
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
                dynamic obj = JsonConvert.DeserializeObject(json)!;

                var config = new ProxyConfig
                {
                    Protocol = "vmess",
                    RawUri = uri,
                    Address = obj.add ?? obj.address ?? "",
                    Port = Convert.ToInt32(obj.port ?? "0")
                };
                return config;
            }
            catch { return null; }
        }

        private ProxyConfig? ParseVless(string uri)
        {
            try
            {
                // vless://uuid@host:port?params#name
                var match = Regex.Match(uri, @"^vless://([^@]+)@([^:]+):(\d+)(?:\?([^#]*))?(?:#(.*))?$");
                if (!match.Success) return null;

                return new ProxyConfig
                {
                    Protocol = "vless",
                    RawUri = uri,
                    Address = match.Groups[2].Value,
                    Port = int.Parse(match.Groups[3].Value)
                };
            }
            catch { return null; }
        }

        private ProxyConfig? ParseTrojan(string uri)
        {
            try
            {
                var match = Regex.Match(uri, @"^trojan://([^@]+)@([^:]+):(\d+)(?:\?([^#]*))?(?:#(.*))?$");
                if (!match.Success) return null;

                return new ProxyConfig
                {
                    Protocol = "trojan",
                    RawUri = uri,
                    Address = match.Groups[2].Value,
                    Port = int.Parse(match.Groups[3].Value)
                };
            }
            catch { return null; }
        }

        private ProxyConfig? ParseShadowsocks(string uri)
        {
            try
            {
                // ss://base64(method:password)@host:port#name
                // or ss://base64(method:password@host:port)#name
                var match = Regex.Match(uri, @"^ss://([^@]+)@([^:]+):(\d+)(?:#(.*))?$");
                if (match.Success)
                {
                    return new ProxyConfig
                    {
                        Protocol = "ss",
                        RawUri = uri,
                        Address = match.Groups[2].Value,
                        Port = int.Parse(match.Groups[3].Value)
                    };
                }
                // Try full base64 format
                var b64 = uri.Substring(5);
                int hashIdx = b64.IndexOf('#');
                if (hashIdx > 0) b64 = b64.Substring(0, hashIdx);
                int pad = 4 - (b64.Length % 4);
                if (pad != 4) b64 = b64.PadRight(b64.Length + pad, '=');
                try
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
                    var parts = decoded.Split('@');
                    if (parts.Length == 2)
                    {
                        var hp = parts[1].Split(':');
                        return new ProxyConfig
                        {
                            Protocol = "ss",
                            RawUri = uri,
                            Address = hp[0],
                            Port = int.Parse(hp[1])
                        };
                    }
                }
                catch { }
                return null;
            }
            catch { return null; }
        }

        private ProxyConfig? ParseHysteria(string uri)
        {
            try
            {
                var match = Regex.Match(uri, @"^(?:hysteria|hy2)://([^@]+)@?([^:]+):(\d+)(?:\?([^#]*))?(?:#(.*))?$");
                if (!match.Success) return null;

                return new ProxyConfig
                {
                    Protocol = "hysteria2",
                    RawUri = uri,
                    Address = match.Groups[2].Value,
                    Port = int.Parse(match.Groups[3].Value)
                };
            }
            catch { return null; }
        }
    }
}
