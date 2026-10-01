# 📡 راهنمای فرمت کانفیگ‌ها

## پروتکل‌های پشتیبانی شده

| پروتکل | پیشوند | مثال |
|--------|--------|------|
| VMess | `vmess://` | `vmess://eyJ2Ijoi...` |
| VLess | `vless://` | `vless://uuid@host:port?...` |
| Trojan | `trojan://` | `trojan://password@host:port?...` |
| Shadowsocks | `ss://` | `ss://base64@host:port#name` |
| Hysteria2 | `hy2://` یا `hysteria://` | `hy2://password@host:port?...` |

## مثال‌های عملی

### 1. VMess
```
vmess://eyJ2IjoiMiIsInBzIjoiTXlTZXJ2ZXIiLCJhZGQiOiJzZXJ2ZXIuZXhhbXBsZS5jb20iLCJwb3J0Ijo0NDMsImlkIjoiMTIzNDU2NzgtMTIzNC0xMjM0LTEyMzQtMTIzNDU2Nzg5YWJjIiwiYWlkIjowLCJuZXQiOiJ3cyIsInR5cGUiOiJub25lIiwiaG9zdCI6IiIsInBhdGgiOiIvIiwidGxzIjoidGxzIn0=
```
(محتوای base64 یک JSON شامل اطلاعات سرور)

### 2. VLess
```
vless://12345678-1234-1234-1234-123456789abc@server.com:443?security=tls&type=ws&path=/ws&host=cdn.com#MyServer
```

### 3. Trojan
```
trojan://myStrongPassword@server.com:443?security=tls&sni=server.com#MyServer
```

### 4. Shadowsocks
```
ss://Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTpwYXNzd29yZA==@server.com:8388#MyServer
```

### 5. Hysteria2
```
hy2://myPassword@server.com:443?sni=server.com&insecure=0#MyServer
```

## ساختار فایل configs.txt

```text
# خطوط شروع شده با # کامنت هستند و نادیده گرفته می‌شوند
# هر خط یک کانفیگ کامل است

vless://uuid1@server1.com:443#Server-1
vmess://base64json...#Server-2
trojan://pass@server3.com:443#Server-3

# خطوط خالی هم نادیده گرفته می‌شوند
```

## نکات مهم

1. **Whitespace**: فاصله‌های اضافی قبل/بعد خط نادیده گرفته می‌شوند
2. **Comments**: خطوط شروع شده با `#` نادیده گرفته می‌شوند
3. **Protocol Detection**: برنامه به طور خودکار پروتکل را از پیشوند URL تشخیص می‌دهد
4. **Multiple Lines**: هر کانفیگ باید در یک خط جداگانه باشد

## تست کانفیگ‌ها

بعد از اضافه کردن کانفیگ‌ها:
1. در برنامه دکمه **"Fetch New"** را بزنید
2. لیست سرورها نمایش داده می‌شود
3. دکمه **"Test Ping"** را بزنید تا پینگ همه سرورها اندازه‌گیری شود
4. دکمه **"Auto Best"** بهترین سرور (کمترین پینگ) را انتخاب می‌کند
5. دکمه بزرگ **"CONNECT"** را بزنید تا وصل شوید

## عیب‌یابی

### کانفیگ لود نمی‌شود
- مطمئن شوید URL کامل است و شامل `#name` در انتها باشد
- فرمت base64 را برای vmess بررسی کنید
- یک space یا کاراکتر اضافی در خط نباشد

### پینگ بالا یا timeout
- سرور ممکن است فیلتر یا down باشد
- فایروال ویندوز را بررسی کنید
- آنتی‌ویروس ممکن است مانع شود

### اتصال برقرار نمی‌شود
- لاگ‌های پایین صفحه را بررسی کنید
- مطمئن شوید sing-box.exe در کنار فایل exe اصلی است
- در خط فرمان تست کنید: `sing-box.exe check -c config.json`
