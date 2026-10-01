# 🔑 راهنمای ساخت GitHub Token و آپلود

## 📋 مرحله ۱: ساخت GitHub Personal Access Token

### ۱.۱. به صفحه Token بروید
👉 [https://github.com/settings/tokens](https://github.com/settings/tokens)

### ۱.۲. یک Token جدید بسازید
1. روی **"Generate new token"** کلیک کنید
2. **"Generate new token (classic)"** را انتخاب کنید
3. تنظیمات:
   - **Note**: `Mahyar Free Upload`
   - **Expiration**: `No expiration` یا `90 days`
   - **Scopes**: فقط **`repo`** را تیک بزنید ✅
4. روی **"Generate token"** کلیک کنید
5. **مهم:** Token را کپی کنید و در جای امن ذخیره کنید! (بعداً نمی‌بینیدش)

Token چیزی شبیه این است:
```
ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

## 📤 مرحله ۲: اجرای اسکریپت آپلود

PowerShell را باز کنید (به عنوان Administrator) و:

```powershell
cd C:\Projects\mahyar-free
.\upload-to-github.ps1 -GitHubUsername "cuetellll" -GitHubToken "ghp_xxxxxxxxxxxxxxxxxx"
```

**مثال واقعی:**
```powershell
.\upload-to-github.ps1 -GitHubUsername "cuetellll" -GitHubToken "ghp_abc123def456ghi789jkl012mno345pqr678"
```

## ⏱️ مرحله ۳: منتظر Build بمانید

1. به [https://github.com/cuetellll/mahyar-free/actions](https://github.com/cuetellll/mahyar-free/actions) بروید
2. workflow در حال اجرا را می‌بینید
3. صبر کنید تا ✅ سبز شود (معمولاً ۲-۵ دقیقه)

## 📥 مرحله ۴: دانلود فایل EXE

1. روی workflow موفق کلیک کنید
2. به پایین اسکرول کنید → بخش **Artifacts**
3. روی **`MahyarFree-main`** کلیک کنید
4. فایل ZIP دانلود می‌شود
5. Extract کنید → `MahyarFree.exe` و `sing-box.exe` آماده هستند!

## 🎉 مرحله ۵: ساخت Release (اختیاری)

اگر می‌خواهید یک نسخه رسمی (Release) داشته باشید:

1. به [https://github.com/cuetellll/mahyar-free/releases/new](https://github.com/cuetellll/mahyar-free/releases/new) بروید
2. **Choose a tag**: `v1.0.0` تایپ کنید → **Create new tag: v1.0.0 on publish**
3. **Release title**: `Mahyar Free v1.0.0 - First Release`
4. توضیحات:
   ```markdown
   ## 🌐 Mahyar Free VPN v1.0.0
   
   First official release of Mahyar Free VPN!
   
   ### Features
   - Modern dark UI with glassmorphism and glow effects
   - Powered by sing-box v1.14
   - Real TCP ping testing
   - Auto-fetch configs from this repository
   - System tray support
   - Multi-protocol: VMess, VLess, Trojan, Shadowsocks, Hysteria2
   
   ### Download
   Download `MahyarFree-1.0.0.zip` below, extract, and run `MahyarFree.exe`.
   ```
5. **Publish release**

⚠️ **توجه:** فایل‌های Release بعد از انتشار workflow بعدی که با تگ v شروع می‌شود، اضافه می‌شوند. یا می‌توانید فایل ZIP را دستی آپلود کنید.

## 🆘 مشکل رایج: Token کار نمی‌کند

اگر خطا گرفتید:
- مطمئن شوید Token را درست کپی کردید (بدون فاصله)
- Token منقضی نشده باشد
- Scope `repo` تیک خورده باشد
- Repository `mahyar-free` وجود داشته باشد (شما ساختید ✅)

## 🔒 امنیت Token

⚠️ **هرگز Token را در جای عمومی قرار ندهید!**

بعد از آپلود، می‌توانید:
- Token را در GitHub Settings حذف کنید
- یا expiration آن را کوتاه کنید
