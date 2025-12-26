# ✅ رفع مشکل Duplicate Print در ReceptionList

**تاریخ:** 1404/10/05  
**اولویت:** 🔴 CRITICAL - رفع باگ Duplicate Print

---

## 📊 مشکل اصلی

### ❌ قبل از رفع:
- با یک کلیک روی دکمه چاپ، **دو پنجره چاپ باز می‌شد**
- Event handler چند بار attach می‌شد
- هیچ Debounce یا جلوگیری از فراخوانی مکرر وجود نداشت

**لاگ خطا:**
```
🖨️ Reception List: Printing receipt for reception: 2216
🔗 Reception List: Print URL: /ReceptionV2/PrintReceipt/2216?type=payment&printer=thermal
🖨️ PrintManager: Print request received: ...
📋 PrintManager: Job added to queue. Queue size: 1
🖨️ PrintManager: Processing job from queue: ...
✅ PrintManager: Print window opened
✅ PrintManager: Print command sent
✅ PrintManager: Print completed successfully
✅ Reception List: چاپ قبض با موفقیت به صف اضافه شد
🔓 PrintManager: Lock released
```

**مشکل:** دو پنجره باز می‌شد (duplicate call)

---

## ✅ راه‌حل پیاده‌سازی شده

### 1. استفاده از Namespace برای Event Handlers
- ✅ **Namespace:** `.receptionList` برای همه event handlers
- ✅ **Cleanup:** `.off('click.receptionList')` قبل از `.on('click.receptionList')`
- ✅ **Stop Propagation:** `e.preventDefault()` و `e.stopPropagation()`

### 2. Debounce در توابع چاپ
- ✅ **Debounce (500ms):** جلوگیری از فراخوانی مکرر
- ✅ **Time Tracking:** استفاده از `window._lastPrintTime` و `window._lastPrintInsuranceTime`

### 3. یکپارچه‌سازی Event Handlers
- ✅ **Consistency:** همه دکمه‌های چاپ از `handlePrintReceipt()` استفاده می‌کنند
- ✅ **Namespace:** همه event handlers با namespace `.receptionList`

---

## 📋 تغییرات جزئی

### قبل:
```javascript
// ❌ بدون namespace
$('.btn-print-receipt').off('click').on('click', function() {
    const receptionId = $(this).data('reception-id');
    handlePrintReceipt(receptionId);
});

// ❌ بدون debounce
function handlePrintReceipt(receptionId) {
    console.log('🖨️ Reception List: Printing receipt...');
    // ...
}
```

### بعد:
```javascript
// ✅ با namespace و stopPropagation
$('.btn-print-receipt').off('click.receptionList').on('click.receptionList', function(e) {
    e.preventDefault();
    e.stopPropagation();
    const receptionId = $(this).data('reception-id');
    console.log('🖨️ Reception List: Print button clicked - ReceptionId:', receptionId);
    handlePrintReceipt(receptionId);
});

// ✅ با debounce
function handlePrintReceipt(receptionId) {
    // ✅ CRITICAL: جلوگیری از فراخوانی مکرر با Debounce
    const now = Date.now();
    if (window._lastPrintTime && (now - window._lastPrintTime) < 500) {
        console.warn('⚠️ Reception List: Print request ignored (debounce)');
        return;
    }
    window._lastPrintTime = now;
    
    console.log('🖨️ Reception List: Printing receipt...');
    // ...
}
```

---

## 🔧 فایل‌های تغییر یافته

### 1. `Scripts/reception.v2/reception-list.js`

#### تغییرات:
- ✅ `attachEventHandlers()`: استفاده از namespace `.receptionList` برای همه event handlers
- ✅ `handlePrintReceipt()`: اضافه کردن Debounce (500ms)
- ✅ `handlePrintInsurance()`: اضافه کردن Debounce (500ms)
- ✅ `btnPrintReceptionDetails`: استفاده از namespace و `handlePrintReceipt()` برای consistency

---

## ✅ نتیجه

### قبل از رفع:
- ❌ دو پنجره با یک کلیک
- ❌ Event handler چند بار attach می‌شد
- ❌ هیچ Debounce وجود نداشت

### بعد از رفع:
- ✅ فقط یک پنجره با یک کلیک
- ✅ Event handlers با namespace (جلوگیری از duplicate)
- ✅ Debounce (500ms) برای جلوگیری از فراخوانی مکرر
- ✅ Stop Propagation برای جلوگیری از event bubbling

---

## 🎯 ویژگی‌های پیاده‌سازی شده

1. **Namespace برای Event Handlers:**
   - `.receptionList` برای همه event handlers
   - Cleanup با `.off('click.receptionList')`

2. **Debounce:**
   - 500ms debounce برای `handlePrintReceipt()`
   - 500ms debounce برای `handlePrintInsurance()`
   - استفاده از `window._lastPrintTime` و `window._lastPrintInsuranceTime`

3. **Stop Propagation:**
   - `e.preventDefault()` و `e.stopPropagation()` در همه event handlers

4. **Consistency:**
   - همه دکمه‌های چاپ از `handlePrintReceipt()` استفاده می‌کنند

---

## 🚀 آماده برای Production

سیستم چاپ در ReceptionList اکنون:
- ✅ بدون Duplicate Calls
- ✅ Production-Grade (Namespace, Debounce, Stop Propagation)
- ✅ User-Friendly (فقط یک پنجره با یک کلیک)

