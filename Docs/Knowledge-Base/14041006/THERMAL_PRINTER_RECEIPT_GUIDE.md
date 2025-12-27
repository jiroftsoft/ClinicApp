# 🖨️ راهنمای کامل سیستم چاپ قبض فیش پرینتر

**تاریخ ایجاد:** 1404/10/06  
**نسخه:** 1.0.0  
**وضعیت:** ✅ Production Ready

---

## 📋 فهرست مطالب

1. [معرفی](#معرفی)
2. [معماری سیستم](#معماری-سیستم)
3. [نحوه استفاده](#نحوه-استفاده)
4. [فایل‌های کلیدی](#فایل‌های-کلیدی)
5. [تنظیمات و پیکربندی](#تنظیمات-و-پیکربندی)
6. [بهینه‌سازی‌ها](#بهینه‌سازی‌ها)
7. [نکات مهم](#نکات-مهم)
8. [مثال‌های کاربردی](#مثال‌های-کاربردی)
9. [عیب‌یابی](#عیب‌یابی)

---

## 🎯 معرفی

سیستم چاپ قبض برای **فیش پرینترهای حرارتی** (Thermal Printers) طراحی شده است که به صورت خودکار و بهینه برای دو اندازه کاغذ کار می‌کند:

- **80mm** (Bixolon SRP-350III) - پیش‌فرض
- **58mm** (Bixolon SRP-330II)

### ویژگی‌های کلیدی

✅ **چاپ خودکار** - بدون نیاز به تنظیمات دستی  
✅ **بهینه برای RTL** - ترازبندی کامل برای فارسی  
✅ **فونت‌های محلی** - Vazir, Shabnam, Yekan  
✅ **Print Manager** - مدیریت حرفه‌ای چاپ (Queue, Debounce, Single Window)  
✅ **مینیمال و کاربرپسند** - طراحی حرفه‌ای برای محیط واقعی  
✅ **کاهش مصرف کاغذ** - بهینه‌سازی فاصله‌ها و اندازه فونت  

---

## 🏗️ معماری سیستم

### 1. Controller Action

```csharp
// Controllers/ReceptionV2/ReceptionControllerV2.cs
[HttpGet]
[Route("PrintReceipt/{id:int}", Name = "ReceptionV2_PrintReceipt")]
public async Task<ActionResult> PrintReceipt(int id, string type = "payment", string printer = "thermal")
{
    // دریافت اطلاعات پذیرش
    var receptionResult = await _receptionFacade.GetReceptionDetailsFullAsync(id);
    
    ViewBag.ReceptionId = id;
    ViewBag.ReceiptType = type;      // "payment" یا "insurance"
    ViewBag.PrinterType = printer;   // "thermal" (80mm) یا "normal" (58mm)
    
    return View("~/Views/ReceptionV2/PrintReceipt.cshtml", receptionResult.Data);
}
```

**پارامترها:**
- `id`: شناسه پذیرش (ReceptionId)
- `type`: نوع قبض (`payment` یا `insurance`)
- `printer`: نوع پرینتر (`thermal` = 80mm, `normal` = 58mm)

### 2. View Structure

```
Views/ReceptionV2/PrintReceipt.cshtml
├── Layout: _ThermalPrintLayout.cshtml
├── Model: ReceptionDetailsFullDto
└── Sections:
    ├── Header (کلینیک شفا + نوع رسید)
    ├── Reception Info (شماره پذیرش، تاریخ)
    ├── Patient Info (نام بیمار، کد ملی)
    ├── Payment Info (مبالغ، بیمه، پرداخت)
    └── Footer (تشکر، تماس، تاریخ)
```

### 3. Print Manager

```javascript
// Scripts/reception.v2/print-manager.js
window.PrintManager.print(url)
    .then(() => console.log('✅ چاپ موفق'))
    .catch(err => console.error('❌ خطا:', err));
```

**ویژگی‌های Print Manager:**
- ✅ Single Window Reuse
- ✅ Print Queue (FIFO)
- ✅ Debounce (1500ms)
- ✅ Lock Manager
- ✅ Auto Cleanup

---

## 🚀 نحوه استفاده

### 1. چاپ قبض پرداخت (ساده)

```javascript
// استفاده از Print Manager (توصیه می‌شود)
const printUrl = `/ReceptionV2/PrintReceipt/${receptionId}?type=payment&printer=thermal`;
window.PrintManager.print(printUrl);
```

### 2. چاپ قبض بیمه تکمیلی

```javascript
const printUrl = `/ReceptionV2/PrintReceipt/${receptionId}?type=insurance&printer=thermal`;
window.PrintManager.print(printUrl);
```

### 3. چاپ با پرینتر 58mm

```javascript
const printUrl = `/ReceptionV2/PrintReceipt/${receptionId}?type=payment&printer=normal`;
window.PrintManager.print(printUrl);
```

### 4. استفاده مستقیم (بدون Print Manager)

```javascript
// Fallback - فقط در صورت عدم دسترسی به Print Manager
const url = `/ReceptionV2/PrintReceipt/${receptionId}?type=payment&printer=thermal`;
window.open(url, '_blank');
```

---

## 📁 فایل‌های کلیدی

### 1. View: `Views/ReceptionV2/PrintReceipt.cshtml`

**مسئولیت:**
- نمایش اطلاعات قبض
- بهینه‌سازی برای فیش پرینتر
- CSS داینامیک بر اساس نوع پرینتر
- RTL Support

**نکات مهم:**
- استفاده از `@model ReceptionDetailsFullDto`
- تشخیص خودکار عرض پرینتر (`printerType`)
- محاسبه مبالغ از Model
- فونت‌های محلی فارسی

### 2. Layout: `Views/Shared/_ThermalPrintLayout.cshtml`

**مسئولیت:**
- Layout مینیمال برای چاپ
- تنظیمات `@page` برای فیش پرینتر
- بارگذاری فونت‌های محلی

### 3. JavaScript: `Scripts/reception.v2/print-manager.js`

**مسئولیت:**
- مدیریت صف چاپ
- جلوگیری از چاپ همزمان
- Debounce و Lock Management
- Auto Cleanup

**Config:**
```javascript
config: {
    debounceDelay: 1500,      // 1.5s
    windowCloseDelay: 1000,    // 1s
    printDelay: 300,           // 300ms
    maxQueueSize: 10
}
```

### 4. Controller: `Controllers/ReceptionV2/ReceptionControllerV2.cs`

**Action:** `PrintReceipt(int id, string type, string printer)`

---

## ⚙️ تنظیمات و پیکربندی

### 1. تشخیص نوع پرینتر

```csharp
// در PrintReceipt.cshtml
var printerWidth = printerType == "thermal" ? "80mm" : "58mm";
var is80mm = printerWidth == "80mm";
```

### 2. تنظیمات CSS داینامیک

```css
/* 80mm (Bixolon SRP-350III) */
body {
    font-size: 12px;
    padding: 3mm 4mm 3mm 2mm;  /* top right bottom left (RTL) */
}

/* 58mm (SRP-330II) */
body {
    font-size: 10px;
    padding: 2mm 3mm 2mm 1.5mm;
}
```

### 3. فونت‌های محلی

```css
font-family: 'Vazir', 'Shabnam', 'Yekan', 'Tahoma', 'Arial', sans-serif;
```

**فایل:** `Content/css/local-fonts.css` (بارگذاری خودکار در Layout)

### 4. Print Media Settings

```css
@media print {
    @page {
        size: 80mm auto;  /* یا 58mm */
        margin: 0;
    }
}
```

---

## 🎨 بهینه‌سازی‌ها

### 1. ترازبندی RTL

✅ **Labels از حاشیه راست فاصله دارند:**
```css
.receipt-label {
    min-width: 75px;  /* 80mm */
    max-width: 85px;
    padding-right: 2px;
}
```

✅ **Values در مقابل Labels:**
```css
.receipt-value {
    flex: 1;
    text-align: right;
    padding-left: 4px;
}
```

### 2. Header مرکزی

```css
.receipt-header {
    text-align: center;
    width: 100%;
}

.receipt-title {
    font-size: 18px;  /* 80mm */
    font-weight: bold;
    text-align: center;
}
```

### 3. کاهش مصرف کاغذ

- ✅ فاصله‌های بهینه (`margin-bottom: 4px`)
- ✅ `line-height: 1.5` (به جای 1.8)
- ✅ حذف عناصر غیرضروری
- ✅ فونت‌های فشرده

### 4. Print Manager بهینه‌سازی‌ها

- ✅ **Debounce:** جلوگیری از کلیک‌های مکرر (1500ms)
- ✅ **Queue:** مدیریت درخواست‌های متوالی
- ✅ **Single Window:** استفاده مجدد از یک پنجره
- ✅ **Auto Close:** بستن خودکار بعد از چاپ (1000ms)

---

## ⚠️ نکات مهم

### 1. ترتیب بارگذاری Scripts

```html
<!-- ✅ درست -->
<script src="~/Scripts/reception.v2/print-manager.js"></script>
<script src="~/Scripts/reception.v2/reception-list.js"></script>
```

**⚠️ Print Manager باید قبل از reception-list.js بارگذاری شود.**

### 2. Event Delegation

```javascript
// ✅ درست - Event Delegation
$(document).on('click', '.btn-print-receipt', function() {
    const receptionId = $(this).data('reception-id');
    handlePrintReceipt(receptionId);
});

// ❌ اشتباه - Multiple Event Handlers
$('.btn-print-receipt').click(function() { ... });
```

### 3. جلوگیری از چاپ همزمان

```javascript
let isPrintingInProgress = false;

function handlePrintReceipt(receptionId) {
    if (isPrintingInProgress) {
        console.warn('⏳ چاپ در حال انجام است...');
        return;
    }
    
    isPrintingInProgress = true;
    // ... چاپ
}
```

### 4. Model Validation

```csharp
// ✅ همیشه Model را بررسی کنید
if (!receptionResult.Success || receptionResult.Data == null)
{
    return View("Error");
}
```

### 5. Fallback برای Print Manager

```javascript
if (window.PrintManager && typeof window.PrintManager.print === 'function') {
    // استفاده از Print Manager
    window.PrintManager.print(url);
} else {
    // Fallback
    window.open(url, '_blank');
}
```

---

## 💡 مثال‌های کاربردی

### مثال 1: چاپ از Reception List

```javascript
// Scripts/reception.v2/reception-list.js
function handlePrintReceipt(receptionId) {
    if (isPrintingInProgress) return;
    
    isPrintingInProgress = true;
    
    const printUrl = `/ReceptionV2/PrintReceipt/${receptionId}?type=payment&printer=thermal`;
    
    window.PrintManager.print(printUrl)
        .then(() => {
            console.log('✅ چاپ موفق');
            setTimeout(() => {
                isPrintingInProgress = false;
            }, 2500);
        })
        .catch(err => {
            console.error('❌ خطا:', err);
            isPrintingInProgress = false;
        });
}
```

### مثال 2: چاپ از Payment Panel

```javascript
// Scripts/reception.v2/payment-panel.js
function printReceiptAfterPayment(receptionId) {
    const printUrl = `/ReceptionV2/PrintReceipt/${receptionId}?type=payment&printer=thermal`;
    window.PrintManager.print(printUrl);
}
```

### مثال 3: چاپ با پرینتر 58mm

```javascript
// برای پرینترهای کوچکتر
const printUrl = `/ReceptionV2/PrintReceipt/${receptionId}?type=payment&printer=normal`;
window.PrintManager.print(printUrl);
```

### مثال 4: چاپ بیمه تکمیلی

```javascript
const printUrl = `/ReceptionV2/PrintReceipt/${receptionId}?type=insurance&printer=thermal`;
window.PrintManager.print(printUrl);
```

---

## 🔧 عیب‌یابی

### مشکل 1: چاپ دو بار انجام می‌شود

**علت:** Event Handler چند بار attach شده است.

**راه‌حل:**
```javascript
// ✅ استفاده از Event Delegation
$(document).on('click', '.btn-print-receipt', function() { ... });

// ❌ نه این روش
$('.btn-print-receipt').click(function() { ... });
```

### مشکل 2: Labels از حاشیه بریده می‌شوند

**علت:** Padding راست کافی نیست.

**راه‌حل:**
```css
body {
    padding: 3mm 4mm 3mm 2mm;  /* افزایش padding-right */
}
```

### مشکل 3: Print Manager کار نمی‌کند

**بررسی:**
1. آیا Script بارگذاری شده است؟
2. آیا `window.PrintManager` وجود دارد؟
3. Console Errors را بررسی کنید.

**Fallback:**
```javascript
if (!window.PrintManager) {
    window.open(url, '_blank');
}
```

### مشکل 4: فونت‌های فارسی نمایش داده نمی‌شوند

**بررسی:**
1. آیا `local-fonts.css` بارگذاری شده است؟
2. آیا فونت‌ها در `Content/fonts/` موجود هستند؟

**راه‌حل:**
```html
<!-- در _ThermalPrintLayout.cshtml -->
<link href="~/Content/css/local-fonts.css" rel="stylesheet" />
```

### مشکل 5: پنجره چاپ بسته نمی‌شود

**بررسی:**
- `windowCloseDelay` در Print Manager
- `afterprint` event listener

**راه‌حل:**
```javascript
// در PrintReceipt.cshtml
window.addEventListener('afterprint', function() {
    setTimeout(() => window.close(), 500);
});
```

---

## 📊 خلاصه Quick Reference

### URL Pattern

```
/ReceptionV2/PrintReceipt/{id}?type={payment|insurance}&printer={thermal|normal}
```

### Print Manager API

```javascript
// چاپ
window.PrintManager.print(url).then(...).catch(...);

// بررسی وضعیت
window.PrintManager.isPrinting;  // boolean
window.PrintManager.printQueue.length;  // number
```

### CSS Classes

- `.thermal-receipt` - Container اصلی
- `.receipt-header` - Header (مرکزی)
- `.receipt-title` - عنوان کلینیک
- `.receipt-subtitle` - نوع رسید
- `.receipt-row` - هر ردیف اطلاعات
- `.receipt-label` - Label (سمت راست)
- `.receipt-value` - Value (سمت چپ)
- `.receipt-total` - مبلغ کل (با border-top)
- `.receipt-footer` - Footer (مرکزی)

### ViewBag Properties

- `ViewBag.ReceptionId` - شناسه پذیرش
- `ViewBag.ReceiptType` - "payment" یا "insurance"
- `ViewBag.PrinterType` - "thermal" (80mm) یا "normal" (58mm)

---

## 📝 تغییرات آینده

### پیشنهادات بهبود:

1. **پشتیبانی از پرینترهای بیشتر:**
   - 112mm (پرینترهای بزرگ)
   - A4 (چاپ معمولی)

2. **Template System:**
   - قالب‌های مختلف برای قبض
   - امکان سفارشی‌سازی Header/Footer

3. **Print Preview:**
   - پیش‌نمایش قبل از چاپ
   - امکان ویرایش قبل از چاپ

4. **Batch Printing:**
   - چاپ چند قبض به صورت یکجا
   - Export به PDF

---

## 📚 منابع مرتبط

- `Docs/RECEPTION_LIST_PRINT_OPTIMIZATION.md` - بهینه‌سازی چاپ
- `Docs/PRINT_SYSTEM_OPTIMIZATION_COMPLETE.md` - بهینه‌سازی سیستم چاپ
- `Scripts/reception.v2/print-manager.js` - کد منبع Print Manager
- `Views/ReceptionV2/PrintReceipt.cshtml` - کد منبع View

---

## ✅ Checklist برای توسعه‌دهندگان

قبل از استفاده از سیستم چاپ:

- [ ] Print Manager بارگذاری شده است
- [ ] Event Delegation استفاده شده است
- [ ] Flag `isPrintingInProgress` پیاده‌سازی شده است
- [ ] Fallback برای Print Manager وجود دارد
- [ ] URL Pattern درست است
- [ ] Model Validation انجام شده است
- [ ] Error Handling پیاده‌سازی شده است
- [ ] Console Logging برای Debug اضافه شده است

---

**نویسنده:** ClinicApp Development Team  
**آخرین به‌روزرسانی:** 1404/10/06  
**وضعیت:** ✅ Production Ready

