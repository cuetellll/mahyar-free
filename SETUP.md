# 🔧 Setup Guide - Mahyar Free VPN

## Quick Start (3 Steps)

### Step 1: دانلود sing-box v1.14
از یکی از این لینک‌ها فایل `sing-box-windows-amd64.zip` را دانلود کنید:
- https://github.com/SagerNet/sing-box/releases/tag/v1.14.0
- https://github.com/SagerNet/sing-box/releases/latest

سپس فایل `sing-box.exe` را در مسیر زیر قرار دهید:
```
C:\Projects\mahyar-free\src\MahyarFree\bin\sing-box.exe
```

### Step 2: Build پروژه
PowerShell را باز کنید و دستورات زیر را اجرا کنید:
```powershell
cd C:\Projects\mahyar-free\src
dotnet restore
dotnet build -c Release
```

### Step 3: اجرا
```powershell
dotnet run --project MahyarFree
```

## اضافه کردن کانفیگ سرورها

### روش ۱: ویرایش مستقیم فایل configs.txt در GitHub
1. به ریپازیتوری `mahyar-free` در GitHub بروید
2. فایل `configs.txt` را ویرایش کنید
3. کانفیگ‌های v2ray/vmess/vless/trojan/ss را خط به خط اضافه کنید
4. Commit کنید
5. کاربران در برنامه دکمه "Fetch New" را بزنند

### روش ۲: ویرایش محلی و push
```powershell
cd C:\Projects\mahyar-free
notepad configs.txt
git add configs.txt
git commit -m "Update configs"
git push
```

## مثال فرمت کانفیگ‌ها

```text
# VMess
vmess://eyJ2IjoiMiIsInBzIjoiU2VydmVyIiw... (base64 encoded JSON)

# VLess
vless://uuid-here@server.com:443?security=tls&type=ws#ServerName

# Trojan
trojan://password@server.com:443#ServerName

# Shadowsocks
ss://Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTpwYXNzd29yZA==@server.com:8388#ServerName

# Hysteria2
hy2://password@server.com:443#ServerName
```

## تغییر URL کانفیگ‌ها

اگر ریپازیتوری شما نام متفاوتی دارد، در فایل زیر URL را تغییر دهید:
```
C:\Projects\mahyar-free\src\MahyarFree\Services\ConfigFetcherService.cs
```

خط ۱۱:
```csharp
private const string DefaultConfigUrl = "https://raw.githubusercontent.com/YOUR-USERNAME/mahyar-free/main/configs.txt";
```

## انتشار نسخه نهایی (EXE)

```powershell
cd C:\Projects\mahyar-free\src
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

فایل exe در این مسیر ساخته می‌شود:
```
C:\Projects\mahyar-free\src\MahyarFree\bin\Release\net6.0-windows\win-x64\publish\MahyarFree.exe
```

⚠️ **نکته مهم:** حتماً `sing-box.exe` را کنار فایل exe خروجی کپی کنید!
