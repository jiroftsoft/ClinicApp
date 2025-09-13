# 📋 قرارداد Persian DatePicker - استانداردهای پیاده‌سازی

## 🎯 هدف قرارداد

این قرارداد استانداردهای کامل پیاده‌سازی Persian DatePicker در تمام فرم‌های سیستم را تعریف می‌کند تا از بروز خطاهای مشابه جلوگیری شود.

---

## 🚨 مشکلات رایج و راه‌حل‌ها

### ❌ مشکل اصلی: "The field ValidFrom must be a date."

**علت:** عدم تطابق بین View binding و ViewModel properties

**راه‌حل:** استفاده از `string` properties برای تاریخ شمسی و `DateTime` properties برای ذخیره در دیتابیس

---

## 📊 ساختار استاندارد ViewModel

### ✅ ViewModel صحیح:

```csharp
public class ExampleCreateEditViewModel
{
    // Properties برای نمایش به کاربر (تاریخ شمسی)
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

    // Properties برای ذخیره در دیتابیس (تاریخ میلادی)
    [HiddenInput(DisplayValue = false)]
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime ValidFrom { get; set; }

    [HiddenInput(DisplayValue = false)]
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateTime? ValidTo { get; set; }

    // متدهای تبدیل
    public void ConvertPersianDatesToGregorian()
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

    public void ConvertGregorianDatesToPersian()
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

### ❌ ViewModel اشتباه:

```csharp
// اشتباه: استفاده مستقیم از DateTime
public DateTime ValidFrom { get; set; }
public DateTime? ValidTo { get; set; }
```

---

## 🎨 ساختار استاندارد View

### ✅ View صحیح:

```html
<!-- فرم ایجاد و ویرایش -->
<div class="form-group">
    @Html.LabelFor(m => m.ValidFromShamsi, new { @class = "form-label" })
    @Html.TextBoxFor(m => m.ValidFromShamsi, new { 
        @class = "form-control persian-datepicker", 
        placeholder = "تاریخ شروع اعتبار", 
        required = "required" 
    })
    @Html.ValidationMessageFor(m => m.ValidFromShamsi, "", new { @class = "text-danger" })
</div>

<div class="form-group">
    @Html.LabelFor(m => m.ValidToShamsi, new { @class = "form-label" })
    @Html.TextBoxFor(m => m.ValidToShamsi, new { 
        @class = "form-control persian-datepicker", 
        placeholder = "تاریخ پایان اعتبار", 
        required = "required" 
    })
    @Html.ValidationMessageFor(m => m.ValidToShamsi, "", new { @class = "text-danger" })
</div>
```

### ❌ View اشتباه:

```html
<!-- اشتباه: استفاده از DateTime properties -->
@Html.TextBoxFor(m => m.ValidFrom, ...)
@Html.TextBoxFor(m => m.ValidTo, ...)
```

---

## ⚙️ ساختار استاندارد Controller

### ✅ Controller صحیح:

```csharp
public class ExampleController : Controller
{
    // GET: Create - فرم خالی
    public async Task<ActionResult> Create()
    {
        var model = new ExampleCreateEditViewModel();
        
        // بارگیری داده‌های مورد نیاز
        // ...
        
        // در فرم ایجاد، TextBox ها باید خالی باشند
        // model.ConvertGregorianDatesToPersian(); // Comment شده
        
        return View(model);
    }

    // GET: Edit - نمایش مقادیر موجود
    public async Task<ActionResult> Edit(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.Success)
        {
            return HttpNotFound();
        }

        // تبدیل تاریخ‌های میلادی به شمسی برای نمایش در فرم
        result.Data.ConvertGregorianDatesToPersian();
        
        return View(result.Data);
    }

    // POST: Create
    [HttpPost]
    public async Task<ActionResult> Create(ExampleCreateEditViewModel model)
    {
        // تبدیل تاریخ‌های شمسی به میلادی قبل از validation
        model.ConvertPersianDatesToGregorian();
        
        if (ModelState.IsValid)
        {
            // ذخیره در دیتابیس
            var result = await _service.CreateAsync(model);
            if (result.Success)
            {
                return RedirectToAction("Index");
            }
        }

        // در صورت خطا، بارگیری مجدد داده‌ها
        // ...
        return View(model);
    }

    // POST: Edit
    [HttpPost]
    public async Task<ActionResult> Edit(ExampleCreateEditViewModel model)
    {
        // تبدیل تاریخ‌های شمسی به میلادی قبل از validation
        model.ConvertPersianDatesToGregorian();
        
        if (ModelState.IsValid)
        {
            // به‌روزرسانی در دیتابیس
            var result = await _service.UpdateAsync(model);
            if (result.Success)
            {
                return RedirectToAction("Index");
            }
        }

        // در صورت خطا، بارگیری مجدد داده‌ها
        // ...
        return View(model);
    }
}
```

---

## 🎯 ساختار استاندارد JavaScript

### ✅ JavaScript صحیح:

```javascript
$(document).ready(function() {
    // Initialize Persian DatePicker
    $('.persian-datepicker').persianDatepicker({
        format: 'YYYY/MM/DD',
        altField: '.observer-example-alt',
        altFormat: 'YYYY/MM/DD',
        observer: true,
        timePicker: {
            enabled: false
        }
    });

    // Date validation
    $('.persian-datepicker').on('change', function() {
        var validFrom = $('#ValidFromShamsi').val();
        var validTo = $('#ValidToShamsi').val();
        
        if (validFrom && validTo) {
            // Convert Persian dates to Gregorian for comparison
            try {
                var fromDate = new persianDate(validFrom.split('/')).toDate();
                var toDate = new persianDate(validTo.split('/')).toDate();
                
                if (fromDate >= toDate) {
                    $('#ValidToShamsi').addClass('is-invalid');
                    $('#ValidToShamsi').next('.invalid-feedback').remove();
                    $('#ValidToShamsi').after('<div class="invalid-feedback">تاریخ پایان اعتبار نمی‌تواند قبل از تاریخ شروع اعتبار باشد.</div>');
                } else {
                    $('#ValidToShamsi').removeClass('is-invalid');
                    $('#ValidToShamsi').next('.invalid-feedback').remove();
                }
            } catch (e) {
                console.log('Date conversion error:', e);
            }
        }
    });

    // Real-time validation feedback
    $('.form-control').on('blur', function() {
        if ($(this).val().trim() === '' && $(this).prop('required')) {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
});
```

### ❌ JavaScript اشتباه:

```javascript
// اشتباه: استفاده از DateTime properties
var validFrom = $('#ValidFrom').val();
var validTo = $('#ValidTo').val();
```

---

## 🔧 تنظیمات PersianDateAttribute

### ✅ تنظیمات صحیح:

```csharp
// Filters/PersianDateAttribute.cs
public class PersianDateAttribute : ValidationAttribute
{
    /// <summary>
    /// محدوده سال‌های معتبر برای سیستم‌های پزشکی
    /// </summary>
    public int MinYear { get; set; } = 700;  // پشتیبانی از تاریخ‌های قدیمی
    public int MaxYear { get; set; } = 1500;

    public string YearRangeMessage { get; set; } = "سال باید بین {0} تا {1} باشد.";
    public string InvalidFormatMessage { get; set; } = "فرمت تاریخ نامعتبر است. (مثال: 1403/05/12)";
    public string InvalidDateMessage { get; set; } = "تاریخ وارد شده معتبر نیست.";
}
```

---

## 📋 چک‌لیست پیاده‌سازی

### ✅ قبل از شروع کار:

- [ ] ViewModel دارای `string` properties برای تاریخ شمسی است
- [ ] ViewModel دارای `DateTime` properties با `[NotMapped]` است
- [ ] ViewModel دارای متدهای تبدیل است
- [ ] View به `string` properties متصل است
- [ ] JavaScript به `string` properties اشاره می‌کند
- [ ] Controller ترتیب صحیح عملیات را دارد

### ✅ بعد از پیاده‌سازی:

- [ ] فرم ایجاد TextBox های خالی دارد
- [ ] فرم ویرایش مقادیر موجود را نمایش می‌دهد
- [ ] DatePicker به درستی کار می‌کند
- [ ] Validation سمت کلاینت و سرور درست عمل می‌کند
- [ ] خطای "The field ValidFrom must be a date." رفع شده است

---

## 🚨 نکات حیاتی

### 1. ترتیب عملیات در Controller:
```csharp
// ترتیب صحیح:
model.ConvertPersianDatesToGregorian(); // 1. تبدیل تاریخ‌ها
if (ModelState.IsValid)                  // 2. بررسی validation
{
    // 3. ذخیره در دیتابیس
}
```

### 2. محدوده سال:
```csharp
// برای پشتیبانی از تاریخ‌های قدیمی:
MinYear = 700  // سال‌های 700-999
MaxYear = 1500 // سال‌های 1000-1500
```

### 3. Error Handling:
```javascript
// همیشه try-catch استفاده کنید:
try {
    var fromDate = new persianDate(validFrom.split('/')).toDate();
} catch (e) {
    console.log('Date conversion error:', e);
}
```

---

## 📁 فایل‌های مرجع

### فایل‌های اصلی:
- `ViewModels/Insurance/InsurancePlan/InsurancePlanCreateEditViewModel.cs`
- `Areas/Admin/Views/InsurancePlan/Create.cshtml`
- `Areas/Admin/Views/InsurancePlan/Edit.cshtml`
- `Areas/Admin/Controllers/Insurance/InsurancePlanController.cs`
- `Filters/PersianDateAttribute.cs`
- `Extensions/DateTimeExtensions.cs`

### فایل‌های مشابه (نیاز به بررسی):
- تمام ViewModel های دارای تاریخ
- تمام View های دارای DatePicker
- تمام Controller های دارای عملیات تاریخ

---

## 🎯 نتیجه

**پس از اعمال این قرارداد:**
- ✅ خطای "The field ValidFrom must be a date." رفع می‌شود
- ✅ DatePicker به درستی کار می‌کند
- ✅ Validation سمت کلاینت و سرور درست عمل می‌کند
- ✅ مقادیر پیش‌فرض در فرم ایجاد نمایش داده نمی‌شود
- ✅ مقادیر موجود در فرم ویرایش نمایش داده می‌شود

**این قرارداد برای تمام فرم‌های دارای DatePicker قابل استفاده است.**

---

## 📞 پشتیبانی

در صورت بروز مشکل، به این قرارداد مراجعه کنید و چک‌لیست را بررسی نمایید.

**تاریخ ایجاد:** 2024/12/19  
**نسخه:** 1.0  
**وضعیت:** فعال
