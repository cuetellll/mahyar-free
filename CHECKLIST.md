# ✅ چک‌لیست نهایی - قبل از Build

## ۱. پیش‌نیازها
- [ ] نصب [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [ ] نصب Visual Studio 2022 (اختیاری، برای راحتی بیشتر)
- [ ] دانلود [sing-box v1.14](https://github.com/SagerNet/sing-box/releases/tag/v1.14.0)

## ۲. ساختار فایل‌ها
پروژه باید این ساختار را داشته باشد:
```
C:\Projects\mahyar-free\
├── configs.txt                    # لیست کانفیگ‌ها
├── README.md
├── SETUP.md
├── CONFIG_FORMATS.md
├── build.ps1
├── .gitignore
└── src\
    ├── MahyarFree.sln
    └── MahyarFree\
        ├── MahyarFree.csproj
        ├── App.xaml
        ├── App.xaml.cs
        ├── bin\
        │   └── sing-box.exe       # ⬅️ اینجا کپی کنید
        ├── Models\
        │   ├── ProxyConfig.cs
        │   └── TrafficStats.cs
        ├── Services\
        │   ├── UriParserService.cs
        │   ├── ConfigFetcherService.cs
        │   ├── PingService.cs
        │   ├── SingBoxService.cs
        │   └── SystemProxyService.cs
        ├── ViewModels\
        │   └── MainViewModel.cs
        ├── Views\
        │   ├── MainWindow.xaml
        │   └── MainWindow.xaml.cs
        ├── Converters\
        │   └── Converters.cs
        └── Resources\
            ├── Colors.xaml
            ├── Styles.xaml
            └── Animations.xaml
```

## ۳. مراحل Build

### در PowerShell:
```powershell
cd C:\Projects\mahyar-free
.\build.ps1
```

### یا دستی:
```powershell
cd C:\Projects\mahyar-free\src
dotnet restore
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## ۴. تنظیمات ضروری بعد از Build

### ۴.۱. تغییر URL ریپازیتوری
فایل `src\MahyarFree\Services\ConfigFetcherService.cs` را باز کنید و خط ۱۱ را ویرایش کنید:
```csharp
private const string DefaultConfigUrl = "https://raw.githubusercontent.com/YOUR-USERNAME/mahyar-free/main/configs.txt";
```

### ۴.۲. اضافه کردن کانفیگ‌های واقعی
فایل `configs.txt` را در ریپازیتوری GitHub ویرایش کنید و کانفیگ‌های واقعی خود را اضافه کنید.

### ۴.۳. Push به GitHub
```powershell
cd C:\Projects\mahyar-free
git init
git add .
git commit -m "Initial commit - Mahyar Free VPN v1.0"
git remote add origin https://github.com/YOUR-USERNAME/mahyar-free.git
git branch -M main
git push -u origin main
```

## ۵. تست

### ۵.۱. اجرای برنامه
```powershell
cd C:\Projects\mahyar-free\src\MahyarFree\bin\Release\net6.0-windows\win-x64\publish
.\MahyarFree.exe
```

### ۵.۲. چک کردن عملکرد
- [ ] برنامه باز می‌شود
- [ ] دکمه "Fetch New" کار می‌کند و کانفیگ‌ها لود می‌شوند
- [ ] دکمه "Test Ping" پینگ همه سرورها را اندازه می‌گیرد
- [ ] دکمه "Auto Best" بهترین سرور را انتخاب می‌کند
- [ ] دکمه بزرگ "CONNECT" اتصال را برقرار می‌کند
- [ ] System proxy تنظیم می‌شود
- [ ] اینترنت از طریق VPN کار می‌کند
- [ ] ترافیک مصرفی نمایش داده می‌شود
- [ ] System tray icon کار می‌کند
- [ ] انیمیشن‌ها و glow effect نمایش داده می‌شوند

## ۶. انتشار نهایی

### در GitHub یک Release بسازید:
1. به ریپازیتوری بروید
2. روی "Releases" کلیک کنید
3. "Create a new release" را بزنید
4. تگ نسخه (مثلاً v1.0.0) و عنوان بگذارید
5. فایل‌های زیر را آپلود کنید:
   - `MahyarFree.exe`
   - `sing-box.exe`
   - `configs.txt` (اختیاری)
6. انتشار دهید

## ۷. عیب‌یابی رایج

### خطای "sing-box.exe not found"
- مطمئن شوید sing-box.exe در کنار MahyarFree.exe است

### خطای "Cannot connect to GitHub"
- اتصال اینترنت خود را بررسی کنید
- URL ریپازیتوری را در `ConfigFetcherService.cs` بررسی کنید
- اگر فیلتر است، VPN دیگری استفاده کنید یا از cache استفاده کنید

### دکمه CONNECT کار نمی‌کند
- ابتدا "Fetch New" و "Test Ping" را بزنید
- یک کانفیگ انتخاب کنید
- لاگ‌های پایین صفحه را بررسی کنید

### پنجره بسته می‌شود ولی برنامه در پس‌زمینه است
- این طبیعی است! آیکون کنار ساعت (System Tray) را نگاه کنید
- برای خروج کامل: راست‌کلیک روی آیکون → Exit
