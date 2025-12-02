# گزارش اصلاح CSP در Web.config: تغییر Port از 8080 به 5000

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ **اصلاح شد**

---

## 🔍 مشکل شناسایی شده

### خطا:
```
Loading the script 'http://localhost:5000/signalr/hubs' violates the following Content Security Policy directive: 
"script-src-elem 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:8080". The action has been blocked.
```

### علت:
CSP در `Web.config` در بخش `customHeaders` هنوز `http://localhost:8080` را اجازه می‌دهد. این CSP به عنوان HTTP Response Header ارسال می‌شود و بر Meta Tag در View اولویت دارد.

---

## ✅ راه‌حل اعمال شده

### 1. اصلاح CSP در Web.config (customHeaders)

**فایل:** `Web.config` (خط 104)

**قبل:**
```xml
<add name="Content-Security-Policy" value="default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:8080; script-src-elem 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:8080; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; img-src 'self' data:; font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; connect-src 'self' http://localhost:8080 ws://localhost:8080; frame-ancestors 'none';" />
```

**بعد:**
```xml
<add name="Content-Security-Policy" value="default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:5000; script-src-elem 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:5000; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; img-src 'self' data:; font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; connect-src 'self' http://localhost:5000 ws://localhost:5000; frame-ancestors 'none';" />
```

### 2. اصلاح CSP در Views/PosTest/Index.cshtml

**فایل:** `Views/PosTest/Index.cshtml` (خط 7-8)

**قبل:**
```html
<!-- Override CSP for SignalR (این صفحه نیاز به اتصال به localhost:8080 دارد) -->
<meta http-equiv="Content-Security-Policy" content="... http://localhost:8080 ...">
```

**بعد:**
```html
<!-- Override CSP for SignalR (این صفحه نیاز به اتصال به localhost:5000 دارد) -->
<meta http-equiv="Content-Security-Policy" content="... http://localhost:5000 ...">
```

---

## 🔧 مراحل بعدی

### 1. Restart Application Pool (ضروری)

**⚠️ نیاز به دسترسی Administrator:**

```powershell
Import-Module WebAdministration
Restart-WebAppPool -Name "ClinicApp"
```

**یا از IIS Manager:**
1. باز کردن IIS Manager
2. انتخاب Application Pool
3. راست کلیک → Recycle

### 2. Clear Browser Cache

**برای اطمینان از اعمال تغییرات:**
- Hard Refresh: Ctrl+F5
- یا Clear Cache: Ctrl+Shift+Delete

### 3. تست در Application

1. باز کردن صفحه `/ReceptionV2`
2. بررسی Console برای خطاها
3. بررسی اینکه SignalR Hubs به درستی بارگذاری می‌شود

---

## 📋 چک‌لیست

- [x] Web.config (appSettings) به Port 5000 تغییر یافت
- [x] Web.config (customHeaders CSP) به Port 5000 تغییر یافت
- [x] Views/ReceptionV2/Index.cshtml به Port 5000 تغییر یافت
- [x] Views/PosTest/Index.cshtml به Port 5000 تغییر یافت
- [x] Views/Shared/_Layout.cshtml به Port 5000 تغییر یافت
- [x] JavaScript files به Port 5000 تغییر یافت
- [x] پیام‌های خطا به Port 5000 تغییر یافت
- [ ] Application Pool Restart شده است (ضروری!)
- [ ] Browser Cache Clear شده است
- [ ] تست در Application موفق است

---

## ⚠️ نکات مهم

1. **Web.config customHeaders:** این CSP به عنوان HTTP Response Header ارسال می‌شود و بر Meta Tag اولویت دارد
2. **Application Pool Restart:** بعد از تغییر Web.config، Application Pool باید Restart شود
3. **Browser Cache:** ممکن است Browser CSP قدیمی را cache کرده باشد
4. **Port 5000:** Service به صورت پیش‌فرض روی Port 5000 listen می‌کند

---

## 🔄 اولویت CSP

1. **HTTP Response Header** (از Web.config customHeaders) - بالاترین اولویت
2. **Meta Tag** (از View) - اولویت دوم
3. **Browser Default** - اگر هیچ کدام تنظیم نشده باشد

---

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ CSP در Web.config و Views به Port 5000 تغییر یافت - نیاز به Restart Application Pool

