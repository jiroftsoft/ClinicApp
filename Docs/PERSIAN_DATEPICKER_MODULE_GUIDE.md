# 📅 راهنمای ماژول Persian DatePicker Manager

**نسخه:** 1.0.0  
**تاریخ:** 1404/09/20  
**نویسنده:** تیم توسعه کلینیک شفا

---

## 📋 فهرست مطالب

1. [معرفی](#معرفی)
2. [معماری](#معماری)
3. [استفاده](#استفاده)
4. [API Reference](#api-reference)
5. [لاگ‌گذاری](#لاگ‌گذاری)
6. [عیب‌یابی](#عیب‌یابی)

---

## 🎯 معرفی

**Persian DatePicker Manager** یک ماژول JavaScript حرفه‌ای برای مدیریت Persian DatePicker در تمام پروژه است.

### ویژگی‌ها:

- ✅ **ماژولار** - طراحی بر اساس اصول SRP و DRY
- ✅ **لاگ‌گذاری قوی** - لاگ‌گذاری کامل در Console مرورگر
- ✅ **خودکار** - Initialize خودکار تمام DatePicker ها
- ✅ **قابل استفاده مجدد** - استفاده در تمام فرم‌ها
- ✅ **مدیریت Edit Forms** - تبدیل خودکار تاریخ‌های موجود

---

## 🏗️ معماری

### ساختار ماژول:

```
PersianDatePickerManager
├── config          // تنظیمات
├── logger          // سیستم لاگ‌گذاری
├── convertPersianToGregorian()  // تبدیل تاریخ
├── getOrCreateHiddenInput()     // مدیریت hidden inputs
├── initializeDatePicker()       // Initialize یک DatePicker
├── handleDateSelect()           // مدیریت انتخاب تاریخ
├── prepareFormForSubmit()       // آماده‌سازی فرم
└── initializeAll()              // Initialize همه
```

### اصول طراحی:

1. **SRP (Single Responsibility Principle)**: هر تابع یک مسئولیت دارد
2. **DRY (Don't Repeat Yourself)**: بدون تکرار کد
3. **Logging**: لاگ‌گذاری کامل برای دیباگ
4. **Modular**: قابل استفاده مجدد

---

## 🚀 استفاده

### 1. اضافه کردن Script

در `@section Scripts`:

```csharp
@section Scripts {
    @Html.Partial("_PersianDatePickerScript")
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
public ActionResult Create(MyModel model)
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

#### `PersianDatePickerManager.convertPersianToGregorian(persianDate)`

تبدیل تاریخ شمسی به میلادی.

**Parameters:**
- `persianDate` (string): تاریخ شمسی (مثلاً "1404/09/19")

**Returns:**
- `string|null`: تاریخ میلادی ISO format یا null

**Example:**
```javascript
var gregorian = PersianDatePickerManager.convertPersianToGregorian("1404/09/19");
// Returns: "2025-12-10T00:00:00"
```

#### `PersianDatePickerManager.initializeAll()`

Initialize کردن تمام DatePicker ها.

**Returns:**
- `boolean`: true اگر موفق باشد

---

### C# API

#### `Controller.ParseDateFromHiddenInput(fieldName, logger)`

تبدیل تاریخ از hidden input به DateTime.

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

## 📊 لاگ‌گذاری

### Console Logs:

ماژول لاگ‌های زیر را در Console مرورگر نمایش می‌دهد:

- `📅 [PersianDatePicker] ✅` - عملیات موفق
- `📅 [PersianDatePicker] ⚠️` - هشدار
- `📅 [PersianDatePicker] ❌` - خطا
- `📅 [PersianDatePicker]` - اطلاعات عمومی

### مثال لاگ‌ها:

```
📅 [PersianDatePicker] شروع initialize تمام DatePicker ها...
📅 [PersianDatePicker] ✅ DatePicker initialize شد: StartDate
📅 [PersianDatePicker] ✅ تاریخ موجود تبدیل شد: {field: "StartDate", persian: "1404/09/19", gregorian: "2025-12-10T00:00:00"}
📅 [PersianDatePicker] ✅ تاریخ انتخاب و تبدیل شد: {field: "StartDate", persian: "1404/09/20", gregorian: "2025-12-11T00:00:00"}
📅 [PersianDatePicker] آماده‌سازی فرم برای submit...
📅 [PersianDatePicker] ✅ فرم آماده submit است
```

---

## 🔍 عیب‌یابی

### مشکل 1: تاریخ ذخیره نمی‌شود

**بررسی:**
1. Console را باز کنید (F12)
2. لاگ‌های `📅 [PersianDatePicker]` را بررسی کنید
3. بررسی کنید که hidden input ایجاد شده است

**راه‌حل:**
- مطمئن شوید که `_PersianDatePickerScript` در `@section Scripts` اضافه شده است
- بررسی کنید که jalaali library لود شده است

### مشکل 2: تاریخ در Edit form تبدیل نمی‌شود

**بررسی:**
1. Console را باز کنید
2. بررسی کنید که لاگ `✅ تاریخ موجود تبدیل شد` نمایش داده می‌شود

**راه‌حل:**
- ماژول به صورت خودکار تاریخ‌های موجود را تبدیل می‌کند
- اگر لاگ نمایش داده نمی‌شود، بررسی کنید که input مقدار دارد

### مشکل 3: لاگ‌ها نمایش داده نمی‌شوند

**راه‌حل:**
```javascript
// فعال کردن لاگ‌گذاری
PersianDatePickerManager.config.enableLogging = true;
```

---

## ✅ بهترین روش‌ها

1. **همیشه از Component استفاده کنید** - `_PersianDatePicker.cshtml`
2. **از Extension Method استفاده کنید** - `ParseDateFromHiddenInput`
3. **لاگ‌ها را بررسی کنید** - برای دیباگ
4. **Validation را مدیریت کنید** - `ModelState.Remove("FieldName")`

---

## 📚 منابع بیشتر

- `Content/js/persian-datepicker-manager.js` - ماژول اصلی
- `Helpers/ControllerExtensions.cs` - Extension Methods
- `Areas/Admin/Views/Shared/_PersianDatePicker.cshtml` - Component
- `Areas/Admin/Views/Shared/_PersianDatePickerScript.cshtml` - Scripts

---

**آخرین به‌روزرسانی:** 1404/09/20

