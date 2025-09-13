# 📋 راهنمای استفاده از ماژول Persian DatePicker

## 🎯 هدف ماژول

این ماژول طبق اصول **DRY** و **SRP** طراحی شده است تا:
- ✅ کدهای تکراری حذف شوند
- ✅ هر کلاس یک مسئولیت داشته باشد
- ✅ قابلیت استفاده مجدد در کل پروژه
- ✅ تنظیمات مرکزی و یکپارچه

---

## 🛠️ نحوه استفاده

### **1. در ViewModel:**

```csharp
// استفاده از Base Class
public class MyViewModel : PersianDateViewModelWithValidation
{
    [Required(ErrorMessage = "تاریخ شروع الزامی است")]
    [PersianDate(IsRequired = true, MustBeFutureDate = false, MinYear = 700, MaxYear = 1500,
        InvalidFormatMessage = "فرمت تاریخ شروع نامعتبر است. (مثال: 1404/06/23)",
        YearRangeMessage = "سال تاریخ شروع باید بین 700 تا 1500 باشد.")]
    [Display(Name = "تاریخ شروع")]
    public string ValidFromShamsi { get; set; }

    [PersianDate(IsRequired = false, MustBeFutureDate = false, MinYear = 700, MaxYear = 1500,
        InvalidFormatMessage = "فرمت تاریخ پایان نامعتبر است. (مثال: 1404/06/23)",
        YearRangeMessage = "سال تاریخ پایان باید بین 700 تا 1500 باشد.")]
    [Display(Name = "تاریخ پایان")]
    public string ValidToShamsi { get; set; }

    [HiddenInput(DisplayValue = false)]
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime ValidFrom { get; set; }

    [HiddenInput(DisplayValue = false)]
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? ValidTo { get; set; }

    // Override متدهای تبدیل
    public override void ConvertPersianDatesToGregorian()
    {
        if (!string.IsNullOrEmpty(ValidFromShamsi))
        {
            ValidFrom = ValidFromShamsi.ToDateTime();
        }

        if (!string.IsNullOrEmpty(ValidToShamsi))
        {
            ValidTo = ValidToShamsi.ToDateTime();
        }
    }

    public override void ConvertGregorianDatesToPersian()
    {
        if (ValidFrom != DateTime.MinValue)
        {
            ValidFromShamsi = ValidFrom.ToPersianDate();
        }

        if (ValidTo.HasValue)
        {
            ValidToShamsi = ValidTo.Value.ToPersianDate();
        }
    }
}
```

### **2. در Controller:**

```csharp
// استفاده از Base Controller
public class MyController : PersianDateCrudController<MyViewModel, MyEntity>
{
    protected override MyViewModel CreateNewModel()
    {
        return new MyViewModel();
    }

    protected override async Task<MyViewModel> GetModelByIdAsync(int id)
    {
        // منطق دریافت مدل
        return await _service.GetByIdAsync(id);
    }

    protected override async Task<bool> SaveModelAsync(MyViewModel model)
    {
        // منطق ذخیره
        return await _service.CreateAsync(model);
    }

    protected override async Task<bool> UpdateModelAsync(MyViewModel model)
    {
        // منطق به‌روزرسانی
        return await _service.UpdateAsync(model);
    }
}
```

### **3. در View:**

#### **روش 1: استفاده از HtmlHelper**

```html
@using ClinicApp.Helpers

<!-- تاریخ شروع -->
<div class="form-group">
    @Html.LabelFor(m => m.ValidFromShamsi, new { @class = "form-label" })
    @Html.PersianDatePickerFor(m => m.ValidFromShamsi, new { 
        @class = "form-control", 
        placeholder = "تاریخ شروع اعتبار", 
        required = "required" 
    })
    @Html.ValidationMessageFor(m => m.ValidFromShamsi, "", new { @class = "text-danger" })
</div>

<!-- تاریخ پایان با مقایسه -->
<div class="form-group">
    @Html.LabelFor(m => m.ValidToShamsi, new { @class = "form-label" })
    @Html.PersianDatePickerFor(m => m.ValidToShamsi, m => m.ValidFromShamsi, new { 
        @class = "form-control", 
        placeholder = "تاریخ پایان اعتبار", 
        required = "required" 
    })
    @Html.ValidationMessageFor(m => m.ValidToShamsi, "", new { @class = "text-danger" })
</div>
```

#### **روش 2: استفاده از Extension Methods**

```html
@using ClinicApp.Extensions

<!-- تاریخ شروع -->
<div class="form-group">
    @Html.LabelFor(m => m.ValidFromShamsi, new { @class = "form-label" })
    @Html.PersianStartDatePicker(new { 
        @class = "form-control", 
        placeholder = "تاریخ شروع اعتبار", 
        required = "required" 
    })
    @Html.ValidationMessageFor(m => m.ValidFromShamsi, "", new { @class = "text-danger" })
</div>

<!-- تاریخ پایان -->
<div class="form-group">
    @Html.LabelFor(m => m.ValidToShamsi, new { @class = "form-label" })
    @Html.PersianEndDatePicker(new { 
        @class = "form-control", 
        placeholder = "تاریخ پایان اعتبار", 
        required = "required" 
    })
    @Html.ValidationMessageFor(m => m.ValidToShamsi, "", new { @class = "text-danger" })
</div>
```

#### **روش 3: استفاده از Partial View**

```html
<!-- استفاده از EditorTemplate -->
@Html.EditorFor(m => m.ValidFromShamsi, "PersianDatePicker", new { 
    htmlAttributes = new { @class = "form-control", placeholder = "تاریخ شروع اعتبار" },
    datePickerOptions = new PersianDatePickerOptions { MinYear = 700, MaxYear = 1500 }
})

<!-- استفاده از Partial View برای محدوده تاریخ -->
@Html.Partial("_PersianDateRange", Model, new ViewDataDictionary {
    { "startDateProperty", "ValidFromShamsi" },
    { "endDateProperty", "ValidToShamsi" },
    { "startDateLabel", "تاریخ شروع" },
    { "endDateLabel", "تاریخ پایان" },
    { "cssClass", "col-md-6" }
})
```

---

## ⚙️ تنظیمات پیشرفته

### **1. تنظیمات DatePicker:**

```csharp
var options = new PersianDatePickerOptions
{
    Format = "YYYY/MM/DD",
    MinYear = 700,
    MaxYear = 1500,
    ComparisonErrorMessage = "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.",
    EnableTimePicker = false
};

@Html.PersianDatePickerFor(m => m.ValidFromShamsi, new { @class = "form-control" }, options)
```

### **2. تنظیمات سفارشی:**

```csharp
// در ViewModel
[PersianDate(
    IsRequired = true,
    MustBeFutureDate = false,
    MinYear = 1300,  // محدوده سفارشی
    MaxYear = 1500,
    InvalidFormatMessage = "فرمت تاریخ نامعتبر است.",
    YearRangeMessage = "سال باید بین 1300 تا 1500 باشد."
)]
public string CustomDateShamsi { get; set; }
```

---

## 📋 چک‌لیست پیاده‌سازی

### **✅ قبل از شروع:**

- [ ] ViewModel از `PersianDateViewModelWithValidation` ارث‌بری می‌کند
- [ ] Controller از `PersianDateCrudController` ارث‌بری می‌کند
- [ ] View از HtmlHelper یا Extension Methods استفاده می‌کند
- [ ] تنظیمات DatePicker مناسب است

### **✅ بعد از پیاده‌سازی:**

- [ ] DatePicker به درستی کار می‌کند
- [ ] Validation سمت کلاینت و سرور درست عمل می‌کند
- [ ] مقایسه تاریخ‌ها کار می‌کند
- [ ] خطای "The field ValidFrom must be a date." رفع شده است
- [ ] کدهای تکراری حذف شده‌اند

---

## 🚨 نکات مهم

### **1. ترتیب عملیات:**
```csharp
// در Controller:
model = PrepareModelForPost(model);  // 1. تبدیل تاریخ‌ها
if (ModelState.IsValid)              // 2. بررسی validation
{
    // 3. ذخیره در دیتابیس
}
```

### **2. Error Handling:**
```javascript
// در JavaScript:
try {
    var fromDate = new persianDate(validFrom.split('/')).toDate();
} catch (e) {
    console.log('Date conversion error:', e);
}
```

### **3. تنظیمات مرکزی:**
```csharp
// در PersianDatePickerOptions:
MinYear = 700   // پشتیبانی از تاریخ‌های قدیمی
MaxYear = 1500  // محدوده معتبر
```

---

## 📁 فایل‌های ماژول

### **فایل‌های اصلی:**
- `Helpers/PersianDatePickerHelper.cs` - HtmlHelper مرکزی
- `ViewModels/Base/PersianDateViewModel.cs` - Base ViewModel
- `Controllers/Base/PersianDateController.cs` - Base Controller
- `Extensions/PersianDateExtensions.cs` - Extension Methods
- `Views/Shared/EditorTemplates/PersianDatePicker.cshtml` - EditorTemplate
- `Views/Shared/EditorTemplates/PersianDateRange.cshtml` - Partial View

### **فایل‌های پشتیبانی:**
- `Filters/PersianDateAttribute.cs` - Validation Attribute
- `Extensions/DateTimeExtensions.cs` - تبدیل تاریخ
- `Helpers/PersianDateHelper.cs` - Helper مرکزی

---

## 🎯 مزایای ماژول

### **✅ DRY (Don't Repeat Yourself):**
- کدهای DatePicker در یک مکان
- تنظیمات مرکزی
- منطق تبدیل تاریخ یکپارچه

### **✅ SRP (Single Responsibility Principle):**
- هر کلاس یک مسئولیت
- HtmlHelper فقط مسئول UI
- Controller فقط مسئول منطق business
- ViewModel فقط مسئول data

### **✅ قابلیت استفاده مجدد:**
- در تمام فرم‌های پروژه
- تنظیمات قابل سفارشی‌سازی
- Extension Methods برای سهولت استفاده

### **✅ نگهداری آسان:**
- تغییرات در یک مکان
- تست‌پذیری بالا
- مستندسازی کامل

---

## 🚀 نتیجه

**این ماژول:**
- ✅ کدهای تکراری را حذف می‌کند
- ✅ اصول DRY و SRP را رعایت می‌کند
- ✅ در کل پروژه قابل استفاده است
- ✅ تنظیمات مرکزی و یکپارچه دارد
- ✅ نگهداری و توسعه آسان است

**🎯 ماژول آماده استفاده است!**
