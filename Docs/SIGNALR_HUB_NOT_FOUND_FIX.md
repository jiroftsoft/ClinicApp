# راهنمای حل مشکل: Hub "SSP1126HUB" یافت نشد

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ **اصلاح شد**

---

## 🔍 مشکل شناسایی شده

### خطا:
```
❌ Hub "SSP1126HUB" not found in $.connection
🔍 $.connection keys: _, events, resources, ajaxDefaults, changeState, isDisconnecting, connectionState, hub, fn, noConflict, transports, version
```

### علت:
hubs script Hub ها را به `signalR` اضافه می‌کند (`$.extend(signalR, signalR.hub.createHubProxies())`)، نه مستقیماً به `$.connection`. در SignalR 2.x، Hub ها باید از `$.signalR` به `$.connection` کپی شوند.

---

## ✅ راه‌حل اعمال شده

### 1. بررسی Hub در چند مکان

**قبل:**
```javascript
// فقط $.connection را بررسی می‌کرد
this.posHub = $.connection[this.config.hubName];
```

**بعد:**
```javascript
// بررسی $.connection، $.signalR، و window.signalR
var hubFound = false;
if ($.connection[this.config.hubName]) {
    this.posHub = $.connection[this.config.hubName];
    hubFound = true;
} else if ($.signalR[this.config.hubName]) {
    this.posHub = $.signalR[this.config.hubName];
    hubFound = true;
}
```

### 2. کپی Hub از $.signalR به $.connection

**در `hubsScript.onload`:**
```javascript
// ✅ CRITICAL: hubs script Hub ها را به signalR اضافه می‌کند، نه $.connection
// از خروجی hubs script: $.extend(signalR, signalR.hub.createHubProxies())
// پس باید Hub ها را از $.signalR به $.connection کپی کنیم
if (typeof $.signalR === 'object' && $.signalR[self.config.hubName]) {
    if (!$.connection[self.config.hubName]) {
        $.connection[self.config.hubName] = $.signalR[self.config.hubName];
        self._log('info', '✅ Hub "' + self.config.hubName + '" copied from $.signalR to $.connection');
    }
}
```

---

## 🔧 مراحل بعدی

### 1. Refresh صفحه
- Hard Refresh: Ctrl+F5
- یا Application را Restart کنید

### 2. تست در Application
- باز کردن صفحه `/ReceptionV2`
- بررسی Console برای خطاها
- بررسی اینکه Hub به درستی پیدا می‌شود

---

## 📋 چک‌لیست

- [x] Hub در hubs script تعریف شده است (SSP1126HUB)
- [x] کد برای بررسی Hub در چند مکان اضافه شد
- [x] کد برای کپی Hub از $.signalR به $.connection اضافه شد
- [ ] تست در Application موفق است

---

## ⚠️ نکات مهم

1. **hubs script:** Hub ها را به `signalR` اضافه می‌کند (`$.extend(signalR, ...)`)
2. **$.connection:** باید Hub ها را از `$.signalR` کپی کنیم
3. **SignalR 2.x:** در این نسخه، `$.signalR` و `$.connection` ممکن است متفاوت باشند

---

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ کد برای کپی Hub از $.signalR به $.connection اضافه شد

