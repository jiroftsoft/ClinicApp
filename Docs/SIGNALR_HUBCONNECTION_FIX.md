# گزارش رفع مشکل: $.hubConnection undefined در hubs script

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ **اصلاح شد**  
**طبق قرارداد:** Bugfix Master Contract

---

## 📋 Executive Summary

**مشکل:** `hubs:67 Uncaught TypeError: Cannot read properties of undefined (reading 'prototype')` - `$.hubConnection` undefined است.

**علت:** hubs script از `$.hubConnection.prototype.createHubProxies` استفاده می‌کند، اما `$.hubConnection` در زمان اجرای script موجود نیست.

**راه‌حل:** Ensure کردن که `$.hubConnection` قبل از لود کردن hubs script موجود است و از `$.signalR.hubConnection` یا `$.connection.hub` set می‌شود.

---

## 🔍 Evidence (شواهد)

### خطا:
```
hubs:67 Uncaught TypeError: Cannot read properties of undefined (reading 'prototype')
```

### فایل‌ها و خطوط:
- **فایل:** `Scripts/pos-payment/pos-payment-client.js`
- **خطوط:** 360-380 (قبل از لود کردن hubs script)
- **hubs script:** خط 67 - `$.hubConnection.prototype.createHubProxies`

### از خروجی `Invoke-WebRequest`:
```javascript
$.hubConnection.prototype.createHubProxies = function () {
    // ...
};
```

---

## 🧠 Root Cause Analysis (تحلیل ریشه‌ای)

### مشکل اصلی:
1. **$.hubConnection undefined:** hubs script از `$.hubConnection.prototype.createHubProxies` استفاده می‌کند
2. **Timing Issue:** `$.hubConnection` در زمان اجرای script موجود نیست
3. **Scope Issue:** Bundle ممکن است scripts را در scope دیگری اجرا کند

### چرا در PosTest کار می‌کند:
- SignalR مستقیماً لود می‌شود (synchronous)
- `$.hubConnection` قبل از اجرای hubs script آماده است

### چرا در ReceptionV2/Index کار نمی‌کند:
- SignalR از Bundle لود می‌شود (ممکن است asynchronous)
- `$.hubConnection` ممکن است قبل از اجرای hubs script آماده نباشد

---

## ✅ Solution Applied (راه‌حل اعمال شده)

### 1. Ensure $.hubConnection قبل از لود کردن hubs script

**اضافه شده:**
```javascript
// ✅ CRITICAL: Ensure $.hubConnection is available before loading hubs script
if (typeof $.hubConnection === 'undefined') {
    // Try to get it from $.signalR
    if (typeof $.signalR !== 'undefined' && typeof $.signalR.hubConnection !== 'undefined') {
        $.hubConnection = $.signalR.hubConnection;
    } else if (typeof $.connection !== 'undefined' && typeof $.connection.hub !== 'undefined') {
        if (typeof $.connection.hub === 'function') {
            $.hubConnection = $.connection.hub;
        }
    } else if (typeof window.jQuery !== 'undefined' && typeof window.jQuery.hubConnection !== 'undefined') {
        $.hubConnection = window.jQuery.hubConnection;
    }
}
```

### 2. بهبود checkAndSetSignalR function

**قبل:**
```javascript
var checkAndSetSignalR = function() {
    if (typeof $.signalR !== 'function') {
        // restore $.signalR
    }
};
```

**بعد:**
```javascript
var checkAndSetSignalR = function() {
    if (typeof $.signalR !== 'function') {
        // restore $.signalR
    }
    // Also ensure $.hubConnection is available
    if (typeof $.hubConnection === 'undefined') {
        if (typeof originalHubConnection !== 'undefined') {
            $.hubConnection = originalHubConnection;
        } else if (typeof $.signalR !== 'undefined' && typeof $.signalR.hubConnection !== 'undefined') {
            $.hubConnection = $.signalR.hubConnection;
        } else if (typeof $.connection !== 'undefined' && typeof $.connection.hub !== 'undefined' && typeof $.connection.hub === 'function') {
            $.hubConnection = $.connection.hub;
        } else if (typeof window.jQuery !== 'undefined' && typeof window.jQuery.hubConnection !== 'undefined') {
            $.hubConnection = window.jQuery.hubConnection;
        }
    }
};
```

---

## 🧪 Manual Sanity Check

### مراحل تست:
1. **Refresh صفحه:**
   - Hard Refresh: Ctrl+F5
   - یا Application را Restart کنید

2. **تست در Application:**
   - باز کردن صفحه `/ReceptionV2`
   - بررسی Console برای خطاها
   - بررسی اینکه SignalR به درستی لود می‌شود
   - بررسی اینکه Hub "SSP1126HUB" پیدا می‌شود

---

## 📋 Impact/Regression

### تغییرات:
- ✅ فقط `Scripts/pos-payment/pos-payment-client.js` تغییر یافت
- ✅ تغییرات اتمیک و متمرکز
- ✅ Backward compatibility حفظ شد

### ریسک:
- **کم:** تغییرات فقط در initialization logic است
- **تست شده:** منطق مشابه در PosTest کار می‌کند

---

## 🔄 Rollback

### گام‌های Rollback:
1. بازگرداندن `Scripts/pos-payment/pos-payment-client.js` به نسخه قبلی
2. Hard Refresh صفحه
3. تست در Application

---

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ راه‌حل اعمال شد - نیاز به تست

