# 🌐 Mahyar Free VPN

A modern, beautiful Windows VPN client powered by [sing-box](https://sing-box.sagernet.org/) v1.14.

![Mahyar Free VPN](https://img.shields.io/badge/version-1.0.0-blueviolet)
![.NET](https://img.shields.io/badge/.NET-6.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

## ✨ Features

- 🎨 **Modern UI** with glassmorphism, gradient, and glow effects
- 🚀 **Powered by sing-box v1.14** - high performance proxy core
- 📡 **Real TCP ping** testing for accurate latency measurement
- ⚡ **Auto-select best server** based on lowest ping
- 🔄 **Auto-fetch configs** from this GitHub repository
- 🎯 **System tray** support with quick connect
- 📊 **Live traffic statistics** (download/upload speed and total)
- 🌈 **Animated UI** with pulse, glow, and rotation effects
- 🔌 **System proxy** auto-configuration
- 📦 **Multi-protocol**: VMess, VLess, Trojan, Shadowsocks, Hysteria2

## 🖼️ Screenshots

_(Add your app screenshots here)_

## 📥 Download

Download the latest release from the [Releases](../../releases) page.

## 🛠️ Building from Source

### Prerequisites
- Windows 10/11
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- sing-box v1.14 binary (place `sing-box.exe` in `src/MahyarFree/bin/`)

### Build
```powershell
cd src
dotnet restore
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained false
```

The output will be in `src/MahyarFree/bin/Release/net6.0-windows/win-x64/publish/`.

## 📡 Adding/Updating Configs

To add or update VPN server configs:

1. Edit [`configs.txt`](./configs.txt) in this repository
2. Add your config URLs (one per line), e.g.:
   ```
   vmess://base64encoded...
   vless://uuid@server.com:443?security=tls#MyServer
   trojan://password@server.com:443#MyServer
   ```
3. Commit and push
4. Users can click **"Fetch New"** in the app to get the updated list

## 🎯 How It Works

- App fetches `configs.txt` from this repository on demand
- Parses each line as a proxy URI (vmess, vless, trojan, etc.)
- Tests real TCP ping to each server's host:port
- Builds a sing-box config JSON for the selected server
- Runs `sing-box.exe` with local SOCKS/HTTP proxy on `127.0.0.1:2080`
- Sets Windows system proxy to use this local endpoint

## ⚙️ Configuration

The default config URL is hardcoded in `ConfigFetcherService.cs`:
```csharp
private const string DefaultConfigUrl = "https://raw.githubusercontent.com/mahyar-free/mahyar-free/main/configs.txt";
```

Change it to your repository URL.

## 🔒 Privacy

- No data is sent anywhere except the configured config URL
- All traffic goes through the selected proxy server
- No analytics, no telemetry

## 📝 License

MIT License - feel free to use and modify.

## 🙏 Credits

- [sing-box](https://sing-box.sagernet.org/) - Universal proxy platform
- [Material Design In XAML](http://materialdesigninxaml.com/) - UI components
- [Hardcodet.NotifyIcon.Wpf](https://github.com/H-Joker/NotifyIcon.Wpf) - System tray

---

Made with ❤️ by Mahyar
