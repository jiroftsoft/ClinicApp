# 📅 راهنمای کامپوننت Persian DatePicker

## 🎯 مقدمه

کامپوننت اصولی و قابل استفاده مجدد برای Persian DatePicker که طبق استانداردهای فرم‌های درمانی سطح سازمانی طراحی شده است.

### ✨ ویژگی‌ها

- ✅ **Component-Based**: کامپوننت محور و قابل استفاده مجدد
- ✅ **Server-Side Today**: دریافت تاریخ امروز از سرور برای اطمینان از صحت
- ✅ **Medical Form Standards**: طبق استانداردهای فرم‌های درمانی سطح سازمانی
- ✅ **Bulletproof**: مقاوم و ضد گلوله
- ✅ **Tested**: تست شده و قابل اعتماد

### 🔧 رفع مشکل تاریخ امروز

**مشکل:** تاریخ امروز به اشتباه نمایش داده می‌شد (مثلاً 3 به جای 4)

**راه حل:**
- دریافت تاریخ امروز از سرور (C#) برای اطمینان از صحت
- Fallback به محاسبه client-side در صورت عدم دسترسی به سرور
- Cache برای بهینه‌سازی عملکرد

---

## 🚀 استفاده

### 1. اضافه کردن Script

در `@section Scripts`:

```csharp
@section Scripts {
    @Html.Partial("_PersianDatePickerScript")
    // سایر script ها...
}
```

### 2. استفاده در View

```csharp
@{
    ViewBag.PersianDatePickerId = "startDatePicker";
    ViewBag.PersianDatePickerName = "StartDate";
    ViewBag.PersianDatePickerValue = Model.StartDate;
    ViewBag.PersianDatePickerLabel = "تاریخ شروع";
    ViewBag.PersianDatePickerPlaceholder = "تاریخ شروع (اختیاری)";
    ViewBag.PersianDatePickerHelpText = "اگر خالی باشد، اطلاعیه از همین الان فعال می‌شود";
    ViewBag.PersianDatePickerRequired = false;
}
@Html.Partial("_PersianDatePicker")
```

### 3. استفاده در Controller

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(MyViewModel model)
{
    // تبدیل خودکار تاریخ‌ها از hidden inputs
    model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
    model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
    
    // حذف خطاهای validation
    ModelState.Remove("StartDate");
    ModelState.Remove("EndDate");
    
    // ادامه...
}
```

---

## 📚 API Reference

### JavaScript API

#### `PersianDatePickerComponent.getTodayFromServer()`

دریافت تاریخ امروز شمسی از سرور.

**Returns:**
- `Promise<string>`: Promise که تاریخ امروز شمسی را برمی‌گرداند

**Example:**
```javascript
PersianDatePickerComponent.getTodayFromServer().then(function(today) {
    console.log('تاریخ امروز:', today); // "1404/10/04"
});
```

#### `PersianDatePickerComponent.initializeAll()`

Initialize کردن تمام DatePicker ها.

**Returns:**
- `boolean`: true اگر موفق باشد

**Example:**
```javascript
PersianDatePickerComponent.initializeAll();
```

### C# API

#### `PersianDateApiController.GetToday()`

API endpoint برای دریافت تاریخ امروز شمسی از سرور.

**Route:** `GET /api/persian-date/today`

**Response:**
```json
{
    "success": true,
    "persianDate": "1404/10/04",
    "gregorianDate": "2025-12-25",
    "timestamp": 1735084800
}
```

#### `Controller.ParseDateFromHiddenInput(fieldName, logger)`

تبدیل تاریخ شمسی به میلادی از hidden input.

**Parameters:**
- `fieldName` (string): نام فیلد (مثلاً "StartDate")
- `logger` (ILogger): Logger برای لاگ‌گذاری (اختیاری)

**Returns:**
- `DateTime?`: تاریخ میلادی یا null

**Example:**
```csharp
model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
```

---

## 🎨 استایل‌های فرم‌های درمانی

کامپوننت طبق استانداردهای فرم‌های درمانی سطح سازمانی طراحی شده است:

### رنگ‌بندی

```css
:root {
    --medical-form-primary: #2c5aa0;        /* آبی تیره */
    --medical-form-secondary: #28a745;      /* سبز ملایم */
    --medical-form-bg: #ffffff;             /* سفید */
    --medical-form-error: #dc3545;          /* قرمز ملایم */
    --medical-form-success: #28a745;        /* سبز خنثی */
    --medical-form-border: #dee2e6;        /* خاکستری ملایم */
}
```

### تایپوگرافی

- فونت: `IRANSansX`, `Vazirmatn`, `Dana`, `Shabnam`
- سایز متن: 14px – 16px
- Line-height: حداقل 1.6

---

## 🔧 تنظیمات

### Configuration

```javascript
PersianDatePickerComponent.config = {
    selector: 'input[data-persian-datepicker="true"]',
    hiddenInputSuffix: '_Hidden',
    apiEndpoint: '/api/persian-date/today',
    logPrefix: '📅 [PersianDatePicker]',
    enableLogging: true,
    cacheTodayFor: 60000, // 1 دقیقه (میلی‌ثانیه)
    retryDelay: 100,
    maxRetries: 3
};
```

### تغییر تنظیمات

```javascript
// تغییر endpoint
PersianDatePickerComponent.config.apiEndpoint = '/custom/api/today';

// غیرفعال کردن logging
PersianDatePickerComponent.config.enableLogging = false;

// تغییر زمان cache
PersianDatePickerComponent.config.cacheTodayFor = 300000; // 5 دقیقه
```

---

## 🐛 Troubleshooting

### مشکل: تاریخ امروز به اشتباه نمایش داده می‌شود

**راه حل:**
1. بررسی اتصال به سرور: `GET /api/persian-date/today`
2. بررسی Console برای خطاها
3. بررسی Cache: `PersianDatePickerComponent.cache.today`

### مشکل: DatePicker initialize نمی‌شود

**راه حل:**
1. بررسی jQuery: `typeof jQuery !== 'undefined'`
2. بررسی pDatepicker: `typeof $.fn.pDatepicker !== 'undefined'`
3. بررسی Selector: `input[data-persian-datepicker="true"]`

### مشکل: تاریخ در submit تبدیل نمی‌شود

**راه حل:**
1. بررسی hidden input: `fieldName_Hidden`
2. بررسی jalaali library
3. بررسی Console برای خطاها

---

## 📝 Checklist

### قبل از استفاده

- [ ] Script ها لود شده‌اند (`_PersianDatePickerScript`)
- [ ] jQuery و pDatepicker موجود هستند
- [ ] API endpoint در دسترس است (`/api/persian-date/today`)
- [ ] ViewBag تنظیم شده است

### بعد از استفاده

- [ ] DatePicker initialize شده است
- [ ] تاریخ امروز به درستی نمایش داده می‌شود
- [ ] انتخاب تاریخ کار می‌کند
- [ ] تبدیل تاریخ در submit کار می‌کند
- [ ] Hidden input ایجاد شده است

---

## 🔗 مراجع

- `Docs/DEVELOPMENT_CONTRACT.md` - قرارداد توسعه
- `Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md` - راهنمای ماژول قدیمی
- `Helpers/PersianDateHelper.cs` - Helper Methods
- `Helpers/ControllerExtensions.cs` - Extension Methods
- `Controllers/Api/PersianDateApiController.cs` - API Controller
- `Content/js/persian-datepicker-component.js` - کامپوننت JavaScript

---

## ✅ تایید

این کامپوننت طبق استانداردهای فرم‌های درمانی سطح سازمانی طراحی شده و آماده استفاده در Production است.

**نسخه:** 2.0.0  
**تاریخ:** 1404/10/04  
**وضعیت:** فعال و تست شده
