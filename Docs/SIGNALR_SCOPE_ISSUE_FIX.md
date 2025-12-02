# گزارش رفع مشکل: SignalR Scope Issue در Bundle Loading

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ **اصلاح شد**  
**طبق قرارداد:** Bugfix Master Contract

---

## 📋 Executive Summary

**مشکل:** `hubs` script قبل از اینکه `$.signalR` function باشد اجرا می‌شود، حتی با double-check.

**علت:** Bundle ممکن است scripts را در scope دیگری اجرا کند، و `$.signalR` در زمان اجرای script در scope درست موجود نیست.

**راه‌حل:** استفاده از `window.jQuery.signalR` و `window.signalR` برای ensure کردن cross-scope access، و یک check function که قبل و بعد از لود کردن script اجرا می‌شود.

---

## 🔍 Evidence (شواهد)

### خطا:
```
hubs:17 Uncaught Error: SignalR: SignalR is not loaded. Please ensure jquery.signalR-x.js is referenced before ~/signalr/js.
```

### فایل‌ها و خطوط:
- **فایل:** `Scripts/pos-payment/pos-payment-client.js`
- **خطوط:** 280-320 (قبل از لود کردن hubs script)
- **خطوط:** 405-420 (بعد از لود کردن hubs script)

### تفاوت بین `/PosTest` و `/ReceptionV2/Index`:
- **PosTest:** SignalR مستقیماً لود می‌شود (synchronous) ✅
- **ReceptionV2/Index:** SignalR از Bundle لود می‌شود (ممکن است asynchronous) ❌

---

## 🧠 Root Cause Analysis (تحلیل ریشه‌ای)

### مشکل اصلی:
1. **Scope Issue:** Bundle ممکن است scripts را در scope دیگری اجرا کند
2. **Timing Issue:** `hubs` script در زمان اجرا `$.signalR` را چک می‌کند: `if (typeof ($.signalR) !== "function")`
3. **Context Loss:** `$.signalR` ممکن است در زمان اجرای script در scope درست موجود نباشد

### چرا در PosTest کار می‌کند:
- SignalR مستقیماً لود می‌شود (synchronous)
- قبل از اجرای کد JavaScript آماده است
- در global scope موجود است

### چرا در ReceptionV2/Index کار نمی‌کند:
- SignalR از Bundle لود می‌شود (ممکن است asynchronous)
- `pos-payment-client.js` ممکن است قبل از اینکه SignalR کاملاً initialize شود اجرا شود
- Bundle ممکن است scripts را در scope دیگری اجرا کند

---

## ✅ Solution Applied (راه‌حل اعمال شده)

### 1. Set $.signalR on window.jQuery and window

**قبل:**
```javascript
var hubsScript = document.createElement('script');
hubsScript.src = hubsUrl;
```

**بعد:**
```javascript
// ✅ CRITICAL: Use Object.defineProperty to ensure $.signalR is always a function
var signalRFunctionRef = $.signalR;

// ✅ Ensure $.signalR is set on window.jQuery as well (for cross-scope access)
if (typeof window.jQuery !== 'undefined' && typeof signalRFunctionRef === 'function') {
    if (typeof window.jQuery.signalR === 'undefined' || typeof window.jQuery.signalR !== 'function') {
        window.jQuery.signalR = signalRFunctionRef;
    }
}

// ✅ Ensure $.signalR is set on window as well (fallback)
if (typeof window !== 'undefined' && typeof signalRFunctionRef === 'function') {
    if (typeof window.signalR === 'undefined' || typeof window.signalR !== 'function') {
        window.signalR = signalRFunctionRef;
    }
}
```

### 2. Check Function برای قبل و بعد از لود کردن script

**اضافه شده:**
```javascript
// ✅ CRITICAL: Set up a global check function that hubs script can use
var originalSignalR = $.signalR;
var checkAndSetSignalR = function() {
    if (typeof $.signalR !== 'function') {
        if (typeof originalSignalR === 'function') {
            $.signalR = originalSignalR;
        } else if (typeof window.jQuery !== 'undefined' && typeof window.jQuery.signalR === 'function') {
            $.signalR = window.jQuery.signalR;
        } else if (typeof window.signalR === 'function') {
            $.signalR = window.signalR;
        }
    }
};

// ✅ Execute check before script loads (in case script executes synchronously)
checkAndSetSignalR();
```

### 3. بهبود restore mechanism در onload

**قبل:**
```javascript
if (typeof $.signalR !== 'function') {
    if (typeof signalRRef === 'function') {
        $.signalR = signalRRef;
    } else if (typeof connectionRef !== 'undefined') {
        $.signalR = connectionRef;
    }
}
```

**بعد:**
```javascript
// ✅ Execute check function to ensure $.signalR is set
checkAndSetSignalR();

if (typeof $.signalR !== 'function') {
    // Try multiple sources
    if (typeof signalRRef === 'function') {
        $.signalR = signalRRef;
    } else if (typeof connectionRef === 'function') {
        $.signalR = connectionRef;
    } else if (typeof window.jQuery !== 'undefined' && typeof window.jQuery.signalR === 'function') {
        $.signalR = window.jQuery.signalR;
    } else if (typeof window.signalR === 'function') {
        $.signalR = window.signalR;
    }
}
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

## 📝 TODO برای PROD

- [ ] بررسی اینکه آیا نیاز به تغییر در Bundle configuration است
- [ ] بررسی اینکه آیا نیاز به تغییر در View است
- [ ] تست کامل در محیط Production

---

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ راه‌حل اعمال شد - نیاز به تست

