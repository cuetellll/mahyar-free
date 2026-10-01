# 📤 راهنمای آپلود دستی به GitHub

## 🚀 مرحله ۱: ساخت ریپازیتوری

1. به [https://github.com/new](https://github.com/new) بروید
2. **Repository name**: `mahyar-free`
3. **Description**: `Modern VPN client powered by sing-box`
4. **Public** را انتخاب کنید ✅
5. ❌ تیک "Add a README file" را بردارید (ما داریم)
6. ❌ بقیه تیک‌ها را هم بردارید
7. روی **Create repository** کلیک کنید

## 📁 مرحله ۲: آپلود فایل‌ها

### روش آسان (آپلود ZIP):

GitHub مستقیماً ZIP قبول نمی‌کند، پس باید فایل‌ها را یکی یکی یا گروهی آپلود کنید.

### روش ۱: آپلود همه فایل‌ها به یکباره

1. در صفحه ریپازیتوری، روی **uploading an existing file** کلیک کنید
   (یا دکمه **Add file** → **Upload files**)
2. فایل `mahyar-free-upload.zip` را drag & drop کنید
3. صبر کنید تا آپلود شود
4. روی **Commit changes** کلیک کنید

⚠️ **توجه:** فایل zip به صورت یک فایل binary آپلود می‌شود. سپس شما باید آن را extract کنید:

1. به [https://extract.me/](https://extract.me/) بروید (یک extract آنلاین)
2. URL فایل zip از GitHub را وارد کنید
3. فایل‌ها را extract کنید

❌ **این روش پیچیده است.** روش بهتر:

### روش ۲: آپلود پوشه‌ها (توصیه شده)

1. به صفحه اصلی ریپازیتوری بروید
2. روی **Add file** → **Upload files** کلیک کنید
3. فایل `mahyar-free-upload.zip` را بکشید و در صفحه رها کنید
4. **صبر نکنید!** GitHub خودش unzip می‌کند اگر zip باشد

⚠️ در واقع GitHub فایل zip را unzip نمی‌کند.

### ✅ روش ۳: استفاده از GitHub Desktop (ساده‌ترین)

1. **GitHub Desktop** را از [desktop.github.com](https://desktop.github.com/) دانلود و نصب کنید
2. وارد اکانت GitHub خود شوید
3. **File** → **Clone repository** → تب **URL**
4. URL: `https://github.com/YOUR-USERNAME/mahyar-free.git`
5. محل ذخیره: `C:\Projects\`
6. کلیک **Clone**
7. فایل‌های موجود در `C:\Projects\mahyar-free\` (غیر از پوشه mahyar-free) را در `C:\Projects\mahyar-free\` (که GitHub ساخته) کپی کنید
8. در GitHub Desktop می‌بینید همه فایل‌ها اضافه شده‌اند
9. یک commit message بنویسید: `Initial commit`
10. **Commit to main** → **Push origin**

این روش **۱۰۰٪ کار می‌کند** ✅

## 🎯 روش ۴: استفاده از Git CLI (حرفه‌ای)

اگر Git CLI نصب دارید، در PowerShell:

```powershell
cd C:\Projects\mahyar-free
git init
git add .
git commit -m "Initial commit - Mahyar Free VPN v1.0"
git branch -M main
git remote add origin https://github.com/YOUR-USERNAME/mahyar-free.git
git push -u origin main
```

## 🔄 بعد از آپلود

### فعال‌سازی GitHub Actions:

1. به ریپازیتوری بروید
2. تب **Actions** را بزنید
3. روی **"I understand my workflows, go ahead and enable them"** کلیک کنید
4. اگر workflow خودکار اجرا نشد، روی **"Run workflow"** کلیک کنید

### دانلود فایل exe:

1. به تب **Actions** بروید
2. روی آخرین workflow موفق کلیک کنید
3. در پایین صفحه بخش **Artifacts** فایل `MahyarFree-main.zip` را دانلود کنید
4. Extract کنید → `MahyarFree.exe` و `sing-box.exe` آماده‌اند!

### ساخت Release (نسخه رسمی):

1. در ریپازیتوری روی **Releases** (سمت راست) کلیک کنید
2. **Create a new release** کلیک کنید
3. **Choose a tag**: `v1.0.0` تایپ کنید → **Create new tag**
4. **Release title**: `Mahyar Free v1.0.0`
5. توضیحات اضافه کنید
6. **Publish release**

اگر workflow با تگ `v*` اجرا شود، GitHub Actions خودکار فایل‌ها را به Release اضافه می‌کند.

## 🆘 مشکل رایج: فایل ZIP بزرگ

اگر GitHub خطای "File too large" داد:
- فایل `mahyar-free-upload.zip` را از پوشه پاک کنید
- فایل‌ها را مستقیم آپلود کنید (بدون zip)

## 📞 کمک

اگر در هر مرحله مشکلی داشتید، بپرسید تا راهنمایی کنم.
