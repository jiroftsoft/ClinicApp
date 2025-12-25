# ⚡ Quick Reference - دسترسی سریع به Helpers/Extensions

**نسخه:** 1.0.0  
**تعداد کل:** 56 Helper/Extension

---

## 🔍 جستجو سریع

### **استفاده:**
1. `Ctrl+F` را بزن
2. Use Case خودت را جستجو کن
3. Helper/Extension مناسب را پیدا کن
4. به مستند کامل برو (اگر نیاز داشتی)

---

## 📅 تاریخ و زمان

| نیاز | Helper/Extension | مثال | فایل |
|------|-----------------|------|------|
| تبدیل میلادی → شمسی | `PersianDateHelper.ToPersianDate()` | `"1404/10/05"` | [01](01-Helpers-DateTime.md) |
| تبدیل شمسی → میلادی | `PersianDateHelper.ParsePersianDate()` | `DateTime` | [01](01-Helpers-DateTime.md) |
| Extension تبدیل | `DateTime.ToPersianDate()` | `DateTime.Now.ToPersianDate()` | [01](01-Helpers-DateTime.md) |
| DatePicker در View | `@Html.Partial("_PersianDatePicker")` | Partial View | [01](01-Helpers-DateTime.md) |
| Parse در Controller | `this.ParseDateFromHiddenInput("Date", _logger)` | `DateTime?` | [01](01-Helpers-DateTime.md) |
| محاسبه سن | `AgeCalculationHelper.CalculateAge()` | `35` | [01](01-Helpers-DateTime.md) |
| سن فارسی | `AgeCalculationHelper.CalculateAgeString()` | `"۳۵ سال"` | [01](01-Helpers-DateTime.md) |
| فرمت زمان | `TimeFormatHelper.FormatTime()` | `"15:40:22"` | [01](01-Helpers-DateTime.md) |

---

## ✅ اعتبارسنجی

| نیاز | Helper | مثال | فایل |
|------|--------|------|------|
| کد ملی | `IranianNationalCodeValidator.IsValid()` | `true/false` | [02](02-Helpers-Validation.md) |
| موبایل | `PhoneNumberValidator.IsValidMobile()` | `true/false` | [02](02-Helpers-Validation.md) |
| تلفن ثابت | `PhoneNumberValidator.IsValidPhone()` | `true/false` | [02](02-Helpers-Validation.md) |
| پاکسازی شماره | `PhoneNumberHelper.CleanPhoneNumber()` | `"09123456789"` | [02](02-Helpers-Validation.md) |
| فرمت نمایشی | `PhoneNumberHelper.FormatPhoneNumber()` | `"0912-345-6789"` | [02](02-Helpers-Validation.md) |

---

## 🔐 امنیت

| نیاز | Helper | مثال |
|------|--------|------|
| لاگ امنیتی | `SecurityLogger.LogSecurityEvent()` | void |
| پوشاندن کد ملی | `SensitiveDataMaskingHelper.MaskNationalCode()` | `"012****789"` |
| پوشاندن موبایل | `SensitiveDataMaskingHelper.MaskPhoneNumber()` | `"0912***6789"` |
| SQL امن | `SafeSqlBuilder.Build()` | SQL Query |

---

## 📝 String و متن

| نیاز | Helper | مثال |
|------|--------|------|
| حذف HTML | `StringHelper.StripHtml()` | Plain Text |
| Truncate | `StringHelper.Truncate()` | کوتاه شده |
| Truncate HTML | `StringHelper.StripHtmlAndTruncate()` | Plain Text کوتاه |
| اعداد فارسی | `PersianNumberHelper.ToPersianNumber()` | `"۱۲۳۴"` |
| اعداد انگلیسی | `PersianNumberHelper.ToEnglishNumber()` | `"1234"` |

---

## 🔔 Notification

| نیاز | Helper | مثال |
|------|--------|------|
| موفقیت (Backend) | `NotificationHelper.SetSuccess(TempData, "...")` | Toastr |
| خطا (Backend) | `NotificationHelper.SetError(TempData, "...")` | Toastr |
| موفقیت (Frontend Admin) | `AdminNotification.success("...")` | Toastr |
| خطا (Frontend Admin) | `AdminNotification.error("...")` | Toastr |
| موفقیت (Frontend Public) | `Notify.success("...")` | Toastr |
| تأیید (Frontend) | `Notify.confirm("...", "...", callback)` | SweetAlert2 |

---

## 🔧 Controller Extensions

| نیاز | Extension | مثال |
|------|-----------|------|
| Parse تاریخ | `this.ParseDateFromHiddenInput("Date", _logger)` | `DateTime?` |
| نمایش خطا | `this.ShowError("...")` | void |

---

## 📊 Service Result

| نیاز | Helper | مثال |
|------|--------|------|
| نتیجه موفق | `ServiceResult.Successful()` | `{ Success = true }` |
| نتیجه ناموفق | `ServiceResult.Failed("...")` | `{ Success = false, Message = "..." }` |
| نتیجه با داده | `ServiceResult<T>.Successful(data)` | `{ Success = true, Data = data }` |

---

## 🎨 Enum Extensions

| نیاز | Extension | مثال |
|------|-----------|------|
| DisplayName | `MyEnum.Value.GetDisplayName()` | `"نام فارسی"` |
| لیست Enum | `EnumExtensions.GetEnumList<MyEnum>()` | `List<SelectListItem>` |

---

## 🖼️ تصاویر

| نیاز | Helper/Service | مثال |
|------|----------------|------|
| مسیر تصویر | `ImagePathHelper.GetFullImagePath()` | Full Path |
| آپلود تصویر | `IImageUploadService.UploadImageWithThumbnail()` | Upload Result |

---

## 📄 گزارش‌ها

| نیاز | Helper | استفاده |
|------|--------|---------|
| Excel پزشکی | `MedicalReportExcelGenerator.Generate()` | Excel File |

---

## 📋 قالب‌ها (Templates)

| نیاز | Helper | استفاده |
|------|--------|---------|
| Parse قالب | `SmartTemplateParser.Parse()` | Parsed Template |
| Render قالب | `SmartTemplateRenderer.Render()` | Rendered HTML |
| متغیرهای قالب | `SmartTemplateVariableHelper.GetVariables()` | Variables |

---

## 🔐 Identity

| نیاز | Extension | مثال |
|------|-----------|------|
| User ID | `User.GetUserId()` | `int` |
| Username | `User.GetUserName()` | `string` |
| Roles | `User.GetRoles()` | `List<string>` |

---

## 🗄️ Database

| نیاز | Helper | استفاده |
|------|--------|---------|
| SQL پویا | `DynamicSqlHelper.Build()` | SQL Query |
| SQL امن | `SafeSqlBuilder.Build()` | Safe SQL |

---

## 🌍 Culture

| نیاز | Extension | مثال |
|------|-----------|------|
| تنظیم فارسی | `CultureHelper.SetPersianCulture()` | void |
| Culture جاری | `CultureExtensions.GetCurrentCulture()` | `CultureInfo` |

---

## ⚙️ App Settings

| نیاز | Helper | مثال |
|------|--------|------|
| خواندن تنظیمات | `AppSettings.Get("Key")` | `string` |
| ورژن App | `ApplicationVersion.GetVersion()` | `"1.0.0"` |

---

## 📊 Logging

| نیاز | Helper | مثال |
|------|--------|------|
| لاگ معمولی | `LoggingHelper.Log("...")` | void |
| لاگ ساختاریافته | `StructuredLogger.Log("...", data)` | void |
| لاگ امنیتی | `SecurityLogger.LogSecurityEvent("...")` | void |

---

## 🎯 Use Case → Helper

### **سناریو 1: ثبت بیمار**
```csharp
// Validation کد ملی
if (!IranianNationalCodeValidator.IsValid(model.NationalCode)) { ... }

// Validation موبایل
if (!PhoneNumberValidator.IsValidMobile(model.PhoneNumber)) { ... }

// Parse تاریخ تولد
model.BirthDate = this.ParseDateFromHiddenInput("BirthDate", _logger);

// محاسبه سن
var age = AgeCalculationHelper.CalculateAge(model.BirthDate);

// نرمال‌سازی شماره
model.PhoneNumber = PhoneNumberHelper.CleanPhoneNumber(model.PhoneNumber);

// Notification
NotificationHelper.SetSuccess(TempData, "بیمار با موفقیت ثبت شد");
```

### **سناریو 2: نمایش لیست بیماران**
```csharp
// در ViewModel
public class PatientListItemViewModel
{
    public string BirthDate => patient.BirthDate.ToPersianDate();
    public string Age => AgeCalculationHelper.CalculateAgeString(patient.BirthDate);
    public string PhoneNumber => PhoneNumberHelper.FormatPhoneNumber(patient.PhoneNumber);
}

// در View
@foreach (var patient in Model)
{
    <tr>
        <td>@patient.BirthDate</td> <!-- تاریخ شمسی -->
        <td>@patient.Age</td> <!-- سن فارسی -->
        <td>@patient.PhoneNumber</td> <!-- فرمت نمایشی -->
    </tr>
}
```

### **سناریو 3: گزارش Excel**
```csharp
// تولید گزارش
var excel = MedicalReportExcelGenerator.Generate(data);

// تبدیل تاریخ‌ها در گزارش
var persianDate = PersianDateHelper.ToPersianDate(item.Date);

// فرمت اعداد فارسی
var persianNumber = PersianNumberHelper.ToPersianNumber(item.Amount.ToString());
```

### **سناریو 4: لاگ امنیتی**
```csharp
// لاگ ورود کاربر
SecurityLogger.LogSecurityEvent("Login", userId);

// پوشاندن داده حساس
var maskedNationalCode = SensitiveDataMaskingHelper.MaskNationalCode(nationalCode);
_logger.Information("کاربر با کد ملی {NationalCode} وارد شد", maskedNationalCode);
```

---

## 🚀 دستور یادگیری سریع

### **روز 1: تاریخ و زمان**
- ✅ `PersianDateHelper.ToPersianDate()`
- ✅ `this.ParseDateFromHiddenInput()`
- ✅ `AgeCalculationHelper.CalculateAge()`

### **روز 2: Validation**
- ✅ `IranianNationalCodeValidator.IsValid()`
- ✅ `PhoneNumberValidator.IsValidMobile()`
- ✅ `PhoneNumberHelper.CleanPhoneNumber()`

### **روز 3: Notification**
- ✅ `NotificationHelper.SetSuccess()`
- ✅ `NotificationHelper.SetError()`
- ✅ `Notify.success()` (Frontend)

### **روز 4: String و عمومی**
- ✅ `StringHelper.StripHtml()`
- ✅ `PersianNumberHelper.ToPersianNumber()`
- ✅ `ServiceResult.Successful()`

### **روز 5: Extensions**
- ✅ `DateTime.ToPersianDate()`
- ✅ `MyEnum.GetDisplayName()`
- ✅ `User.GetUserId()`

---

## 📚 لینک‌های مفید

- **[01-Helpers-DateTime.md](01-Helpers-DateTime.md)** - راهنمای کامل تاریخ
- **[02-Helpers-Validation.md](02-Helpers-Validation.md)** - راهنمای کامل Validation
- **[README.md](README.md)** - فهرست اصلی

---

## ⚡ نکات سریع

### ✅ **همیشه:**
1. از `PersianDateHelper` برای تاریخ استفاده کن
2. از `NotificationHelper` برای پیام استفاده کن
3. از `ServiceResult` برای نتیجه استفاده کن
4. از `IranianNationalCodeValidator` برای کد ملی استفاده کن

### 🚫 **هرگز:**
1. `DateTime.ToString("yyyy/MM/dd")` برای تاریخ شمسی
2. `TempData["Success"]` مستقیم برای پیام
3. `return true/false` مستقیم از سرویس (از `ServiceResult` استفاده کن)
4. کد ملی بدون Validation ذخیره کن

---

**نسخه:** 1.0.0  
**آخرین به‌روزرسانی:** 1404/10/05

🎉 **Quick Reference آماده است!** 🎉

**این فایل را Bookmark کن برای دسترسی سریع!** 📌

