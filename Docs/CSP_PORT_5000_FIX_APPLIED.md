# گزارش اصلاح CSP: تغییر Port از 8080 به 5000

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
CSP (Content Security Policy) در `Views/Shared/_Layout.cshtml` هنوز `http://localhost:8080` را اجازه می‌دهد، اما Service روی Port 5000 listen می‌کند.

---

## ✅ راه‌حل اعمال شده

### تغییر CSP در _Layout.cshtml

**قبل:**
```html
<meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:8080; script-src-elem 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:8080; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; img-src 'self' data:; font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; connect-src 'self' http://localhost:8080 ws://localhost:8080; frame-src 'none'; object-src 'none'; base-uri 'self'; form-action 'self';">
```

**بعد:**
```html
<meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:5000; script-src-elem 'self' 'unsafe-inline' 'unsafe-eval' http://localhost:5000; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; img-src 'self' data:; font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; connect-src 'self' http://localhost:5000 ws://localhost:5000; frame-src 'none'; object-src 'none'; base-uri 'self'; form-action 'self';">
```

### تغییرات:
- `script-src`: `http://localhost:8080` → `http://localhost:5000`
- `script-src-elem`: `http://localhost:8080` → `http://localhost:5000`
- `connect-src`: `http://localhost:8080 ws://localhost:8080` → `http://localhost:5000 ws://localhost:5000`

---

## 🔧 مراحل بعدی

### 1. Restart Application Pool
```powershell
Import-Module WebAdministration
Restart-WebAppPool -Name "ClinicApp"
```

### 2. Refresh صفحه
- صفحه را Refresh کنید (Ctrl+F5)
- یا Application را Restart کنید

### 3. تست در Application
- باز کردن صفحه `/ReceptionV2`
- بررسی Console برای خطاها
- بررسی اینکه SignalR Hubs به درستی بارگذاری می‌شود

---

## 📋 چک‌لیست

- [x] Web.config به Port 5000 تغییر یافت
- [x] Views به Port 5000 تغییر یافت
- [x] JavaScript files به Port 5000 تغییر یافت
- [x] CSP به Port 5000 تغییر یافت
- [ ] Application Pool Restart شده است
- [ ] تست در Application موفق است

---

## ⚠️ نکات مهم

1. **CSP Directives:**
   - `script-src`: برای اجرای JavaScript از منابع خارجی
   - `script-src-elem`: برای `<script>` elements
   - `connect-src`: برای AJAX, WebSocket, و SignalR connections

2. **Port 5000:**
   - Service به صورت پیش‌فرض روی Port 5000 listen می‌کند
   - همه تنظیمات باید به Port 5000 اشاره کنند

3. **Restart:**
   - بعد از تغییر CSP، Application Pool باید Restart شود
   - یا صفحه را Hard Refresh کنید (Ctrl+F5)

---

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ CSP به Port 5000 تغییر یافت - نیاز به Restart Application Pool

