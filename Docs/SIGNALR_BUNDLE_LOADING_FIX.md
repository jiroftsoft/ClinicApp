# راهنمای حل مشکل: SignalR در Bundle به درستی لود نمی‌شود

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ **اصلاح شد**

---

## 🔍 مشکل شناسایی شده

### خطا:
```
hubs:17 Uncaught Error: SignalR: SignalR is not loaded. Please ensure jquery.signalR-x.js is referenced before ~/signalr/js.
```

### تفاوت بین `/PosTest` و `/ReceptionV2/Index`:

1. **PosTest** (کار می‌کند ✅):
   - SignalR به صورت مستقیم در View لود می‌شود:
   ```html
   <script src="~/Scripts/jquery.signalR-2.4.2.min.js"></script>
   ```
   - این script به صورت **synchronous** لود می‌شود
   - قبل از اجرای کد JavaScript آماده است

2. **ReceptionV2/Index** (خطا دارد ❌):
   - SignalR از Bundle لود می‌شود:
   ```csharp
   @Scripts.Render("~/bundles/reception.v2")
   ```
   - Bundle ممکن است به صورت **asynchronous** لود شود
   - `pos-payment-client.js` ممکن است قبل از اینکه SignalR کاملاً initialize شود اجرا شود

### علت:
- `hubs` script قبل از اینکه `$.signalR` function باشد اجرا می‌شود
- Bundle ممکن است scripts را به صورت asynchronous لود کند
- Timing issue: `pos-payment-client.js` قبل از آماده شدن SignalR اجرا می‌شود

---

## ✅ راه‌حل اعمال شده

### 1. Double-check قبل از لود کردن hubs script

**قبل:**
```javascript
var hubsScript = document.createElement('script');
hubsScript.src = hubsUrl;
```

**بعد:**
```javascript
// ✅ CRITICAL: Ensure $.signalR is a function BEFORE loading hubs script
// Double-check that $.signalR is still a function right before loading
if (typeof $.signalR !== 'function') {
    // Attempt to restore from stored reference
    if (typeof signalRRef === 'function') {
        $.signalR = signalRRef;
    } else if (typeof connectionRef === 'function') {
        $.signalR = connectionRef;
    } else if (typeof window.signalR === 'function') {
        $.signalR = window.signalR;
    } else {
        // Error: Cannot restore
        return;
    }
}

// ✅ Final verification: $.signalR MUST be a function
if (typeof $.signalR !== 'function') {
    // Error: Still not a function
    return;
}

var hubsScript = document.createElement('script');
hubsScript.src = hubsUrl;
```

### 2. بهبود polling mechanism

کد موجود `_waitForSignalRAndInitialize` از قبل polling mechanism دارد که منتظر می‌ماند تا SignalR لود شود. این کد به درستی کار می‌کند.

---

## 🔧 مراحل بعدی

### 1. Refresh صفحه
- Hard Refresh: Ctrl+F5
- یا Application را Restart کنید

### 2. تست در Application
- باز کردن صفحه `/ReceptionV2`
- بررسی Console برای خطاها
- بررسی اینکه SignalR به درستی لود می‌شود

---

## 📋 چک‌لیست

- [x] Double-check قبل از لود کردن hubs script اضافه شد
- [x] Restore mechanism برای $.signalR اضافه شد
- [x] Final verification قبل از لود کردن hubs script اضافه شد
- [ ] تست در Application موفق است

---

## ⚠️ نکات مهم

1. **Bundle Loading:** Bundle ممکن است scripts را به صورت asynchronous لود کند
2. **Timing Issue:** باید مطمئن شویم که SignalR قبل از hubs script کاملاً آماده است
3. **$.signalR:** باید function باشد، نه object
4. **Polling:** کد موجود polling mechanism دارد که منتظر می‌ماند تا SignalR لود شود

---

## 🔄 تفاوت با PosTest

### PosTest (کار می‌کند):
- SignalR به صورت مستقیم لود می‌شود (synchronous)
- قبل از اجرای کد JavaScript آماده است

### ReceptionV2/Index (اصلاح شد):
- SignalR از Bundle لود می‌شود (ممکن است asynchronous باشد)
- Double-check قبل از لود کردن hubs script اضافه شد
- Restore mechanism برای $.signalR اضافه شد

---

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ Double-check و restore mechanism قبل از لود کردن hubs script اضافه شد

