# 📋 راهنمای سریع قرارداد توسعه
## ClinicApp - Medical Production Environment

**نسخه:** 1.0  
**آخرین به‌روزرسانی:** دی ۱۴۰۴  
**مرجع کامل:** `Docs/DEVELOPMENT_CONTRACT.md`

---

## 🎯 اصول اساسی (Non-Negotiable)

### 1. محیط درمانی رسمی (Medical Production)
```
✅ رسمی | رسمی | رسمی
❌ جیق | جلف | فانتزی
```

- طراحی مناسب محیط بیمارستانی/کلینیکی
- رنگ‌بندی رسمی و حرفه‌ای
- تمرکز بر کارایی و دقت

---

## 🎨 پالت رنگ استاندارد

### ✅ رنگ‌های مجاز (MUST USE)

```css
:root {
    /* رنگ‌های اصلی */
    --medical-primary: #2c5aa0;      /* آبی درمانی */
    --medical-secondary: #6c757d;    /* خاکستری */
    --medical-success: #28a745;      /* سبز */
    --medical-danger: #dc3545;       /* قرمز */
    --medical-warning: #ffc107;      /* زرد */
    --medical-info: #17a2b8;         /* آبی روشن */
    
    /* پس‌زمینه */
    --medical-light: #f8f9fa;
    --medical-bg: #ffffff;
    
    /* متن */
    --medical-text: #212529;
    --medical-text-muted: #6c757d;
    
    /* Border */
    --medical-border: #dee2e6;
}
```

### ❌ رنگ‌های ممنوع (FORBIDDEN)

```css
/* ❌ ممنوع - بنفش جیغ */
#9b59b6, #8e44ad

/* ❌ ممنوع - صورتی */
#e91e63, #f06292

/* ❌ ممنوع - نارنجی تند */
#ff5722, #ff9800

/* ❌ ممنوع - گرادینت‌های فانتزی */
background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
```

### ✅ قواعد استایل

```css
/* ✅ درست - رنگ ساده */
.card-header {
    background-color: var(--medical-primary);
    color: white;
    border-radius: 12px 12px 0 0;
}

/* ❌ اشتباه - گرادینت */
.card-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}
```

---

## 💪 Strongly-Typed Development

### ✅ الزامی: ViewModel برای همه چیز

```csharp
// ✅ درست
@model ClinicApp.ViewModels.CMS.HealthTipCreateEditViewModel

public async Task<ActionResult> Index()
{
    var viewModel = new InsuranceInfoIndexPageViewModel
    {
        InsuranceInfos = insurances,
        InsuranceTypes = types,
        SelectedType = selectedType
    };
    return View(viewModel);
}
```

```csharp
// ❌ اشتباه
@model dynamic
ViewBag.InsuranceTypes = types;  // ❌ ممنوع برای داده‌های اصلی
ViewData["SelectedType"] = selectedType;  // ❌ ممنوع
```

### ✅ استثنای ViewBag (فقط موارد زیر مجاز است)

```csharp
// ✅ مجاز - فقط برای UI
ViewBag.Title = "عنوان صفحه";
ViewBag.MetaDescription = "توضیحات";
ViewBag.ShowHelp = true;
```

### ✅ View Resolution در Admin Area

```csharp
// ✅ الزامی - استفاده از GetViewPath()
public ActionResult Create()
{
    return View(GetViewPath("Create"), model);
}

public async Task<ActionResult> Index()
{
    return View(GetViewPath("Index"), viewModel);
}
```

---

## 🛡️ Bulletproof Coding

### ✅ Error Handling الزامی

```csharp
// ✅ درست
try
{
    var result = await _service.DoSomethingAsync();
    if (!result.Success)
    {
        _logger.Warning("خطا: {ErrorMessage}", result.Message);
        NotificationHelper.SetError(TempData, result.Message);
        return View(model);
    }
}
catch (Exception ex)
{
    _logger.Error(ex, "خطا در انجام عملیات");
    NotificationHelper.SetError(TempData, "خطا در انجام عملیات");
    return View(model);
}
```

### ✅ Null Checking

```csharp
// ✅ درست
if (model == null)
{
    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
}

var title = model.Title ?? string.Empty;
```

### ✅ ModelState Validation

```csharp
// ✅ درست
if (!ModelState.IsValid)
{
    return View(model);
}
```

---

## 🏗️ معماری SRP (Single Responsibility Principle)

### ✅ Controller → فقط Routing و Orchestration

```csharp
// ✅ درست
public class HealthTipController : BaseCMSController
{
    private readonly IHealthTipService _healthTipService;
    
    public async Task<ActionResult> Create(HealthTipCreateEditViewModel model)
    {
        // فقط orchestration
        var result = await _healthTipService.CreateHealthTipAsync(model);
        
        if (!result.Success)
        {
            NotificationHelper.SetError(TempData, result.Message);
            return View(GetViewPath("Create"), model);
        }
        
        NotificationHelper.SetSuccess(TempData, "عملیات موفق");
        return RedirectToAction("Index");
    }
}
```

```csharp
// ❌ اشتباه - Business Logic در Controller
public async Task<ActionResult> Create(HealthTipCreateEditViewModel model)
{
    var healthTip = new HealthTip();
    healthTip.Title = model.Title;  // ❌ Business Logic در Controller
    _context.HealthTips.Add(healthTip);
    await _context.SaveChangesAsync();
}
```

### ✅ Service → فقط Business Logic

```csharp
// ✅ درست
public class HealthTipService : IHealthTipService
{
    private readonly IHealthTipRepository _repository;
    
    public async Task<ServiceResult<HealthTip>> CreateHealthTipAsync(
        HealthTipCreateEditViewModel model)
    {
        // Business logic
        var healthTip = new HealthTip { /* ... */ };
        _repository.Add(healthTip);
        await _context.SaveChangesAsync();
        return ServiceResult<HealthTip>.Successful(healthTip);
    }
}
```

### ✅ Repository → فقط Data Access

```csharp
// ✅ درست
public class HealthTipRepository : IHealthTipRepository
{
    public async Task<HealthTip> GetByIdAsync(int id)
    {
        return await _context.HealthTips.FindAsync(id);
    }
}
```

---

## 🔔 سیستم پیام‌ها و هشدارها

### ✅ Toastr Notifications (الزامی)

```csharp
// ✅ درست
NotificationHelper.SetSuccess(TempData, "عملیات با موفقیت انجام شد");
NotificationHelper.SetError(TempData, "خطا در انجام عملیات");
NotificationHelper.SetWarning(TempData, "هشدار");
NotificationHelper.SetInfo(TempData, "اطلاعات");
```

```csharp
// ❌ اشتباه
TempData["Success"] = "عملیات موفق";  // ❌ ممنوع
```

### ✅ SweetAlert2 Confirmations

```javascript
// ✅ درست
Swal.fire({
    title: 'آیا از انجام این عملیات اطمینان دارید؟',
    text: 'این عملیات قابل بازگشت نیست',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#dc3545',
    cancelButtonColor: '#6c757d',
    confirmButtonText: 'بله، انجام بده',
    cancelButtonText: 'خیر، انصراف',
    reverseButtons: true
}).then(function(result) {
    if (result.isConfirmed) {
        form.submit();
    }
});
```

```javascript
// ❌ اشتباه
if (confirm('مطمئنید؟')) {  // ❌ ممنوع
    form.submit();
}
```

### ❌ حذف Alert های Bootstrap

```html
<!-- ❌ اشتباه - حذف شود -->
@if (TempData["Success"] != null)
{
    <div class="alert alert-success">@TempData["Success"]</div>
}
```

---

## 📅 تقویم شمسی (JalaliDatePicker Enterprise) - Enterprise-Grade

**⚠️ CRITICAL:** فقط از **JalaliDatePicker Enterprise** استفاده کنید. الگوی قدیمی (Persian DatePicker - babakhani) حذف شده است.

**مرجع:** `Docs/Jalili/JALALIDATEPICKER_ENTERPRISE_GUIDE.md`

**🔴 الزامی:** طبق `Docs/ENTERPRISE_DATE_MIGRATION_GUIDE.md`

### ✅ در View (استفاده از Partial - الزامی)

```razor
@* ✅ ENTERPRISE-GRADE: استفاده از JalaliDatePicker Enterprise (الزامی) *@
@* ✅ طبق Docs/Jalili/JALALIDATEPICKER_ENTERPRISE_GUIDE.md *@
@{
    ViewBag.PersianDatePickerId = "startDatePicker";
    ViewBag.PersianDatePickerName = "StartDate";
    ViewBag.PersianDatePickerValue = Model.StartDate; // DateTime? (UTC از دیتابیس)
    ViewBag.PersianDatePickerLabel = "تاریخ شروع";
    ViewBag.PersianDatePickerPlaceholder = "تاریخ شروع را انتخاب کنید";
    ViewBag.PersianDatePickerHelpText = "";
    ViewBag.PersianDatePickerRequired = true;
    ViewBag.PersianDatePickerCssClass = "form-control";
}
@Html.Partial("_PersianDatePicker")

@section Scripts {
    @* ✅ ENTERPRISE-GRADE: استفاده از JalaliDatePicker Enterprise *@
    @* ❌ ممنوع: persian-datepicker.min.js (الگوی قدیمی حذف شده) *@
    @Html.Partial("_PersianDatePickerScript")
}
```

### ❌ ممنوع در View:

```html
<!-- ❌ ممنوع - استفاده از datetime-local -->
<input type="datetime-local" name="StartDate" />

<!-- ❌ ممنوع - استفاده از date -->
<input type="date" name="StartDate" />
```

### ✅ در Controller (Parse از Hidden Input - Enterprise-Grade)

```csharp
// ✅ ENTERPRISE-GRADE: Parse تاریخ از hidden input
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(MyViewModel model)
{
    // ✅ Parse تاریخ از hidden input (تبدیل شمسی → میلادی)
    model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
    model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
    
    // ✅ تبدیل به UTC قبل از ذخیره در دیتابیس
    if (model.StartDate.HasValue)
    {
        model.StartDate = model.StartDate.Value.ToUniversalTime();
    }
    
    // ادامه عملیات...
}
```

### ✅ در Services (استفاده از ITimeProvider - Enterprise-Grade)

```csharp
// ✅ ENTERPRISE-GRADE: استفاده از ITimeProvider
public class AppointmentBookingService
{
    private readonly ITimeProvider _timeProvider;
    
    public AppointmentBookingService(ITimeProvider timeProvider, ...)
    {
        _timeProvider = timeProvider;
    }
    
    public async Task<ServiceResult> ReserveAppointmentAsync(...)
    {
        // ✅ استفاده از UTC
        var utcNow = _timeProvider.UtcNow;
        var iranToday = _timeProvider.GetIranToday(); // برای Validation
        
        // ✅ Validation بر اساس timezone ایران
        if (request.AppointmentDate.Date < iranToday)
        {
            return ServiceResult.Failed("...");
        }
        
        // ✅ ذخیره در دیتابیس به صورت UTC
        var appointment = new Appointment
        {
            AppointmentDate = request.AppointmentDate.ToUniversalTime(),
            CreatedAt = _timeProvider.UtcNow // ✅ UTC
        };
    }
}
```

### ✅ نمایش تاریخ شمسی

```razor
@* ✅ نمایش تاریخ شمسی (از UTC دیتابیس) *@
@PersianDateHelper.ToPersianDate(item.Date)

@* ✅ نمایش با فرمت سفارشی *@
@PersianDateHelper.ToPersianDateString(item.Date, "yyyy/MM/dd - HH:mm")
```

### 🚨 قانون طلایی Enterprise-Grade:

> **"همیشه UTC در دیتابیس، تبدیل به timezone محلی فقط برای نمایش"**

### 📚 مراجع:

- `Docs/ENTERPRISE_DATE_MIGRATION_GUIDE.md` - راهنمای کامل (الزامی)
- `Contracts/Knowledge-Base/AI/Master/01-Helpers-DateTime.md` - راهنمای Helpers

### ❌ ممنوع

```html
<!-- ❌ اشتباه - استفاده از datetime-local -->
@Html.TextBoxFor(m => m.StartDate, new { type = "datetime-local" })
```

---

## 🖼️ سیستم آپلود تصویر

### ✅ Controller Implementation

```csharp
// ✅ درست - تزریق IImageUploadService
private readonly IImageUploadService _imageUploadService;

// Constants
private const string ImageUploadPath = "~/Content/Images/health-tips";
private const string ThumbnailUploadPath = "~/Content/Images/health-tips/thumbnails";
private const int ThumbnailWidth = 300;
private const int ThumbnailHeight = 300;
private const int MaxImageWidth = 1920;
private const int MaxImageHeight = 1080;

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(HealthTipCreateEditViewModel model)
{
    // Parse تاریخ‌ها
    model.PublishedAt = this.ParseDateFromHiddenInput("PublishedAt", _logger);
    
    // پردازش آپلود تصویر
    await ProcessImageUpload(model);
    
    if (!ModelState.IsValid)
    {
        return View(GetViewPath("Create"), model);
    }
    
    // ادامه عملیات...
}

private async Task ProcessImageUpload(HealthTipCreateEditViewModel model)
{
    try
    {
        var imageFile = Request.Files["ImageFile"];
        if (imageFile != null && imageFile.ContentLength > 0)
        {
            var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                imageFile,
                ImageUploadPath,
                ThumbnailUploadPath,
                ThumbnailWidth,
                ThumbnailHeight,
                MaxImageWidth,
                MaxImageHeight);
            
            if (!uploadResult.Success)
            {
                _logger.Warning("خطا در آپلود تصویر: {ErrorMessage}", uploadResult.Message);
                NotificationHelper.SetError(TempData, uploadResult.Message);
                ModelState.AddModelError("ImageFile", uploadResult.Message);
                return;
            }
            
            model.ImageUrl = uploadResult.Data.ImageUrl;
            model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
        }
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در پردازش آپلود تصویر");
        NotificationHelper.SetError(TempData, "خطا در آپلود تصویر");
        ModelState.AddModelError("", "خطا در آپلود تصویر");
    }
}
```

### ✅ View Implementation

```html
<!-- ✅ درست -->
@using (Html.BeginForm("Create", "HealthTip", FormMethod.Post, 
    new { enctype = "multipart/form-data" }))
{
    <div class="form-group">
        <label>تصویر اصلی</label>
        <div class="custom-file">
            <input type="file" class="custom-file-input" id="ImageFile" 
                   name="ImageFile" 
                   accept="image/jpeg,image/jpg,image/png,image/gif,image/webp">
            <label class="custom-file-label" for="ImageFile">
                انتخاب تصویر...
            </label>
        </div>
        <small class="form-text text-muted">
            فرمت‌های مجاز: JPG, PNG, GIF, WEBP | حداکثر حجم: 5 مگابایت
        </small>
        @Html.HiddenFor(m => m.ImageUrl)
        @Html.ValidationMessageFor(m => m.ImageUrl)
    </div>
}
```

---

## 📝 CKEditor (ویرایشگر متن)

### ✅ ViewModel

```csharp
// ✅ درست
public class BlogPostCreateEditViewModel
{
    [Required(ErrorMessage = "محتوای مقاله الزامی است.")]
    [AllowHtml] // ✅ الزامی برای فیلدهای CKEditor
    [Display(Name = "محتوای مقاله")]
    public string Content { get; set; }
}
```

### ✅ Controller

```csharp
// ✅ درست
[HttpPost]
[ValidateAntiForgeryToken]
[ValidateInput(false)] // ✅ الزامی برای فیلدهای CKEditor
public async Task<ActionResult> Create(BlogPostCreateEditViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }
    
    // model.Content شامل HTML از CKEditor است
    // ...
}
```

### ✅ View

```html
<!-- ✅ درست -->
@Html.TextAreaFor(m => m.Content, new { 
    @class = "form-control", 
    id = "contentEditor",
    rows = "10"
})

@section Scripts {
    @Html.Partial("_CKEditorScript")
    
    @{
        ViewBag.CKEditorSelector = "#contentEditor";
        ViewBag.CKEditorHeight = 400;
    }
    @Html.Partial("_CKEditorInit")
}
```

---

## 🏥 فرم‌های درمانی (Medical Forms)

### ✅ اصول پایه

1. **سادگی مطلق** - حداقل رنگ، حداکثر خوانایی
2. **رسمی و حرفه‌ای** - مناسب محیط بیمارستانی
3. **حذف عناصر غیرضروری**
4. **تمرکز بر ورود سریع اطلاعات**

### ✅ ساختار فرم

```html
<!-- ✅ درست - تقسیم‌بندی با Fieldset -->
<form>
    <fieldset class="form-section">
        <legend class="form-section-header">اطلاعات هویتی</legend>
        <!-- فیلدها -->
    </fieldset>
    
    <fieldset class="form-section">
        <legend class="form-section-header">اطلاعات تماس</legend>
        <!-- فیلدها -->
    </fieldset>
    
    <fieldset class="form-section">
        <legend class="form-section-header">اطلاعات پزشکی</legend>
        <!-- فیلدها -->
    </fieldset>
</form>
```

### ✅ Input Design

```css
/* ✅ درست */
.medical-form input {
    border: 1px solid var(--medical-form-border);
    border-radius: 4px;  /* Radius کم */
    padding: 0.75rem;
    font-size: 16px;
}

.medical-form input:focus {
    border-color: var(--medical-form-primary);
    box-shadow: 0 0 0 0.2rem rgba(44, 90, 160, 0.25);
}
```

### ✅ فونت‌های مجاز

```css
/* ✅ درست */
.medical-form {
    font-family: 'IRANSansX', 'Vazirmatn', 'Dana', 'Shabnam', sans-serif;
    font-size: 16px;
    line-height: 1.6;
}
```

### ✅ Validation

```javascript
// ✅ درست - Real-time validation
document.querySelectorAll('.medical-form input').forEach(input => {
    input.addEventListener('blur', function() {
        validateField(this);
    });
    
    input.addEventListener('input', function() {
        if (this.classList.contains('is-invalid')) {
            validateField(this);
        }
    });
});
```

### ✅ دکمه‌ها

```html
<!-- ✅ درست -->
<div class="form-actions">
    <button type="submit" class="btn btn-primary">
        ثبت اطلاعات
    </button>
    <button type="button" class="btn btn-secondary" onclick="history.back()">
        انصراف
    </button>
</div>
```

### ❌ انیمیشن‌های ممنوع

```css
/* ❌ ممنوع - Bounce, Shake, Slide اغراق‌آمیز */
@keyframes bounce { /* ... */ }
@keyframes shake { /* ... */ }
```

### ✅ انیمیشن‌های مجاز

```css
/* ✅ مجاز - Fade-in ملایم */
.form-section {
    animation: fadeIn 0.25s ease-in-out;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
}
```

---

## ✅ Checklist نهایی قبل از Commit

### UI/UX
- [ ] فونت Vazir یا IRANSansX استفاده شده است
- [ ] رنگ‌های استاندارد `--medical-*` استفاده شده‌اند
- [ ] هیچ رنگ جیق و جلف وجود ندارد
- [ ] هیچ گرادینت فانتزی وجود ندارد
- [ ] Border-radius مناسب است (4px-12px)
- [ ] Responsive Design تست شده است

### Strongly-Typed
- [ ] تمام View ها دارای `@model` هستند
- [ ] هیچ `ViewBag`/`ViewData` برای داده‌های اصلی وجود ندارد
- [ ] تمام Controller Actions در Admin Area از `GetViewPath()` استفاده می‌کنند

### Bulletproof
- [ ] تمام متدهای async دارای try-catch هستند
- [ ] تمام null reference ها بررسی شده‌اند
- [ ] تمام `ModelState` ها بررسی شده‌اند
- [ ] تمام `ServiceResult` ها بررسی شده‌اند

### SRP
- [ ] Controller ها فقط routing و orchestration دارند
- [ ] Service ها فقط business logic دارند
- [ ] Repository ها فقط data access دارند

### Notifications
- [ ] تمام پیام‌ها با `NotificationHelper` هستند
- [ ] تمام confirmations با SweetAlert2 هستند
- [ ] هیچ `alert()` یا `confirm()` وجود ندارد
- [ ] هیچ Alert Bootstrap وجود ندارد

### JalaliDatePicker Enterprise
- [ ] تمام فیلدهای تاریخ از `_PersianDatePicker` استفاده می‌کنند
- [ ] فقط از JalaliDatePicker Enterprise استفاده می‌شود (الگوی قدیمی حذف شده)
- [ ] `persian-datepicker.min.js` حذف شده و از `_PersianDatePickerScript` استفاده می‌شود
- [ ] تمام Controller ها از `ParseDateFromHiddenInput` استفاده می‌کنند
- [ ] هیچ `datetime-local` وجود ندارد

### Image Upload
- [ ] `IImageUploadService` تزریق شده است
- [ ] متد `ProcessImageUpload` پیاده‌سازی شده است
- [ ] Form دارای `enctype="multipart/form-data"` است
- [ ] Image Preview پیاده‌سازی شده است

### CKEditor (در صورت نیاز)
- [ ] `[AllowHtml]` به ViewModel اضافه شده است
- [ ] `[ValidateInput(false)]` به POST Action اضافه شده است
- [ ] `_CKEditorScript` و `_CKEditorInit` بارگذاری شده‌اند

### Medical Forms
- [ ] ساختار فرم با Section/Fieldset تقسیم‌بندی شده است
- [ ] استایل Input حرفه‌ای است (Border ساده، Radius کم)
- [ ] Real-time Validation پیاده‌سازی شده است
- [ ] انیمیشن‌های مینیمال استفاده شده‌اند
- [ ] دسترس‌پذیری رعایت شده است

### Security
- [ ] تمام inputs validated هستند
- [ ] تمام forms دارای CSRF protection هستند
- [ ] تمام SQL queries parameterized هستند

---

## 📚 مراجع

### Documents
- `Docs/DEVELOPMENT_CONTRACT.md` - قرارداد توسعه کامل
- `Docs/TODO_TEMPLATE.md` - Template TODO
- `Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md` - راهنمای تقویم شمسی
- `Docs/IMAGE_UPLOAD_SYSTEM_GUIDE.md` - راهنمای آپلود تصویر
- `Docs/CKEDITOR_USAGE_GUIDE.md` - راهنمای CKEditor

### Knowledge Base
- `Docs/Knowledge-Base/01-Helpers-DateTime.md` - Helper های تاریخ و زمان
- `Docs/Knowledge-Base/02-Helpers-Validation.md` - Helper های اعتبارسنجی
- `Docs/Knowledge-Base/06-Quick-Reference.md` - مرجع سریع

---

## 💡 نکته مهم

**این راهنما باید همیشه در دسترس باشد و قبل از شروع هر کاری مطالعه شود!**

✅ = الزامی و غیرقابل مذاکره  
❌ = ممنوع و غیرمجاز  
📋 = توصیه شده

