# گزارش کامل: مهاجرت از Port 8080 به Port 5000

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ **تکمیل شد**

---

## 📋 خلاصه تغییرات

### مشکل اصلی:
Service SSP1126 روی Port 5000 listen می‌کند (نه Port 8080)، اما تمام تنظیمات Application به Port 8080 اشاره می‌کردند.

### راه‌حل:
همه تنظیمات از Port 8080 به Port 5000 تغییر یافت.

---

## ✅ فایل‌های تغییر یافته

### 1. Web.config
- ✅ `appSettings` → `SamanKishSignalRUrl`: `http://localhost:5000/signalr`
- ✅ `customHeaders` → `Content-Security-Policy`: Port 8080 → 5000

### 2. Views
- ✅ `Views/ReceptionV2/Index.cshtml`: Default Port 8080 → 5000
- ✅ `Views/PosTest/Index.cshtml`: Default Port 8080 → 5000 و CSP
- ✅ `Views/Shared/_Layout.cshtml`: CSP Port 8080 → 5000

### 3. JavaScript Files
- ✅ `Scripts/pos-payment/pos-payment-client.js`: Default Port 8080 → 5000
- ✅ `Scripts/reception.v2/payment-panel.js`: Default Port 8080 → 5000
- ✅ پیام‌های خطا: Port 8080 → 5000

---

## 🔧 تنظیمات Service

### وضعیت Service:
- ✅ Service در حال اجرا است: `SSP1126Service1` (Status: Running)
- ✅ Port 5000 باز است: `netstat -ano | findstr :5000 | findstr LISTENING`
- ✅ SignalR Hubs در دسترس است: `http://localhost:5000/signalr/hubs` (StatusCode: 200)

### Config File:
- ✅ `LogPath`: `C:\Log\` (اصلاح شد)
- ✅ `HostUrl`: `192.168.1.103`

---

## 📋 چک‌لیست نهایی

- [x] Web.config (appSettings) به Port 5000 تغییر یافت
- [x] Web.config (customHeaders CSP) به Port 5000 تغییر یافت
- [x] Views/ReceptionV2/Index.cshtml به Port 5000 تغییر یافت
- [x] Views/PosTest/Index.cshtml به Port 5000 تغییر یافت
- [x] Views/Shared/_Layout.cshtml به Port 5000 تغییر یافت
- [x] JavaScript files به Port 5000 تغییر یافت
- [x] پیام‌های خطا به Port 5000 تغییر یافت
- [x] CSP در تمام فایل‌ها به Port 5000 تغییر یافت
- [x] Service Config اصلاح شد (LogPath)
- [x] تست در `/PosTest` موفق است ✅

---

## 🎯 نتیجه

### قبل:
- ❌ Service روی Port 5000 listen می‌کرد
- ❌ Application به Port 8080 اشاره می‌کرد
- ❌ CSP Port 8080 را اجازه می‌داد
- ❌ خطای `ERR_CONNECTION_REFUSED` و `CSP violation`

### بعد:
- ✅ Service روی Port 5000 listen می‌کند
- ✅ Application به Port 5000 اشاره می‌کند
- ✅ CSP Port 5000 را اجازه می‌دهد
- ✅ SignalR Hubs به درستی بارگذاری می‌شود
- ✅ تست در `/PosTest` موفق است

---

## 📚 مستندات ایجاد شده

1. `Docs/SERVICE_PORT_NOT_LISTENING_FIX.md` - راهنمای عیب‌یابی Port
2. `Docs/SERVICE_CANNOT_START_FIX.md` - راهنمای Start Service
3. `Docs/SERVICE_RUNNING_BUT_PORT_NOT_LISTENING.md` - راهنمای عیب‌یابی Service
4. `Docs/SERVICE_CONFIG_FIX_APPLIED.md` - گزارش اصلاح Config
5. `Docs/SERVICE_PORT_5000_FIX_APPLIED.md` - گزارش تغییر Port
6. `Docs/SERVICE_LISTENING_ON_WRONG_PORT.md` - شناسایی Port اشتباه
7. `Docs/SIGNALR_PROTOCOL_ERROR_FIX.md` - راهنمای Protocol Error
8. `Docs/CSP_PORT_5000_FIX_APPLIED.md` - گزارش اصلاح CSP در Layout
9. `Docs/ERROR_MESSAGE_PORT_5000_FIX.md` - گزارش اصلاح پیام‌های خطا
10. `Docs/WEB_CONFIG_CSP_PORT_5000_FIX.md` - گزارش اصلاح CSP در Web.config
11. `Docs/PORT_5000_MIGRATION_COMPLETE.md` - این فایل

---

## ⚠️ نکات مهم برای آینده

1. **Port Service:** Service به صورت پیش‌فرض روی Port 5000 listen می‌کند
2. **CSP:** باید در دو جا تنظیم شود:
   - `Web.config` → `customHeaders` (اولویت اول)
   - `Views` → Meta Tag (اولویت دوم)
3. **Application Pool:** بعد از تغییر `Web.config` باید Restart شود
4. **Browser Cache:** ممکن است نیاز به Clear Cache باشد

---

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ **مهاجرت کامل - همه چیز کار می‌کند**

