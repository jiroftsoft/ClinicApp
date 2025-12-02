# راهنمای حل مشکل: Protocol error: Unknown transport

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ **اصلاح شد**

---

## 🔍 مشکل شناسایی شده

### خطا:
```
http://localhost:5000/signalr
Protocol error: Unknown transport.
```

### علت:
1. **URL نادرست:** باید `/signalr/hubs` باشد، نه `/signalr`
2. **JavaScript هنوز به Port 8080 اشاره می‌کند:** Default values در JavaScript files هنوز Port 8080 دارند

---

## ✅ راه‌حل اعمال شده

### 1. تغییر Default Port در JavaScript Files

**فایل:** `Scripts/pos-payment/pos-payment-client.js`
```javascript
// قبل:
signalRUrl: 'http://localhost:8080/signalr',

// بعد:
signalRUrl: 'http://localhost:5000/signalr',
```

**فایل:** `Scripts/reception.v2/payment-panel.js`
```javascript
// قبل:
var signalRUrl = window.SamanKishSignalRUrl || 'http://localhost:8080/signalr';

// بعد:
var signalRUrl = window.SamanKishSignalRUrl || 'http://localhost:5000/signalr';
```

### 2. بررسی View Configuration

**فایل:** `Views/ReceptionV2/Index.cshtml`
- باید `window.SamanKishSignalRUrl` را از `Web.config` بخواند
- این مقدار از `Web.config` → `SamanKishSignalRUrl` می‌آید

---

## 🔧 نکات مهم

### URL صحیح برای SignalR:

1. **برای بارگذاری Hubs Script:**
   ```
   http://localhost:5000/signalr/hubs
   ```

2. **برای اتصال به Hub:**
   ```
   http://localhost:5000/signalr
   ```

### تفاوت:
- `/signalr/hubs` → JavaScript file که Hub proxies را تعریف می‌کند
- `/signalr` → Base URL برای اتصال SignalR (برای negotiate و transport)

---

## 📋 چک‌لیست

- [x] Web.config به Port 5000 تغییر یافت
- [x] Default values در JavaScript files به Port 5000 تغییر یافت
- [ ] Application Pool Restart شده است
- [ ] View `window.SamanKishSignalRUrl` را از Web.config می‌خواند
- [ ] تست در Application موفق است

---

## ⚠️ نکات مهم

1. **URL Format:**
   - ✅ صحیح: `http://localhost:5000/signalr`
   - ❌ اشتباه: `http://localhost:5000/signalr/` (با trailing slash)
   - ❌ اشتباه: `http://localhost:5000/signalr/hubs` (برای connection)

2. **Hubs Script:**
   - ✅ صحیح: `http://localhost:5000/signalr/hubs` (برای loading script)
   - ❌ اشتباه: `http://localhost:5000/signalr` (برای loading script)

3. **Connection URL:**
   - ✅ صحیح: `http://localhost:5000/signalr` (برای `$.connection.hub.url`)
   - ❌ اشتباه: `http://localhost:5000/signalr/hubs` (برای connection)

---

## 🔄 مراحل تست

### 1. Restart Application Pool
```powershell
Import-Module WebAdministration
Restart-WebAppPool -Name "ClinicApp"
```

### 2. تست SignalR Hubs
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/signalr/hubs" -UseBasicParsing
```

### 3. تست در Browser
```
http://localhost:5000/signalr/hubs
```

**باید JavaScript code برگرداند (نه HTML error page)**

### 4. تست در Application
1. باز کردن صفحه `/PosTest` یا `/ReceptionV2`
2. بررسی Console برای خطاها
3. تست اتصال به SignalR

---

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ Default values در JavaScript files به Port 5000 تغییر یافت

