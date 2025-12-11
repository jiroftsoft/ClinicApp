# 📋 قرارداد توسعه و استانداردهای کدنویسی
## ClinicApp - Medical Production Environment

**نسخه:** 1.0.0  
**تاریخ ایجاد:** 2024  
**وضعیت:** فعال و الزامی

---

## 📌 مقدمه

این سند به عنوان قرارداد الزامی برای تمام توسعه‌های پروژه ClinicApp تعریف شده است. تمام کدهای جدید و تغییرات موجود باید طبق این استانداردها پیاده‌سازی شوند.

---

## 1️⃣ بهینه‌سازی UI/UX برای محیط Production درمانی

### 1.1 اصول طراحی

#### ✅ الزامات UI/UX:
- **رسمی و حرفه‌ای**: تمام رابط‌های کاربری باید برای محیط درمانی رسمی طراحی شوند
- **RTL Support**: پشتیبانی کامل از راست‌به‌چپ برای زبان فارسی
- **Accessibility**: رعایت استانداردهای دسترسی‌پذیری (WCAG 2.1 Level AA)
- **Responsive Design**: سازگاری کامل با تمام دستگاه‌ها (Mobile, Tablet, Desktop)
- **Performance**: زمان بارگذاری صفحه کمتر از 3 ثانیه
- **Font Consistency**: استفاده از فونت Vazir برای تمام ماژول‌های CMS

#### ✅ استانداردهای طراحی:
```css
/* رنگ‌های استاندارد برای محیط درمانی */
--primary-color: #007bff;      /* آبی - اعتماد */
--success-color: #28a745;      /* سبز - موفقیت */
--danger-color: #dc3545;       /* قرمز - هشدار */
--warning-color: #ffc107;      /* زرد - توجه */
--info-color: #17a2b8;        /* آبی روشن - اطلاعات */
--medical-red: #c82333;        /* قرمز درمانی */
--medical-blue: #0056b3;       /* آبی درمانی */
```

#### ✅ Component Standards:
- **Cards**: استفاده از `card shadow` برای تمام کارت‌ها
- **Buttons**: استفاده از `btn btn-{type}` با آیکون مناسب
- **Forms**: استفاده از `form-control` با validation messages
- **Tables**: استفاده از `table table-bordered` با responsive wrapper
- **Modals**: استفاده از Bootstrap Modal یا SweetAlert2

### 1.2 Checklist UI/UX:

- [ ] تمام عناصر با فونت Vazir نمایش داده می‌شوند
- [ ] تمام فرم‌ها دارای placeholder و help text هستند
- [ ] تمام دکمه‌ها دارای آیکون مناسب هستند
- [ ] تمام جداول responsive هستند
- [ ] تمام تصاویر دارای alt text هستند
- [ ] تمام لینک‌ها دارای title attribute هستند
- [ ] تمام فرم‌ها دارای validation client-side و server-side هستند
- [ ] تمام صفحات دارای loading state هستند
- [ ] تمام عملیات دارای feedback مناسب هستند

---

## 2️⃣ Strongly-Typed Development

### 2.1 الزامات Strongly-Typed:

#### ✅ ViewModels:
```csharp
// ✅ درست
@model ClinicApp.ViewModels.CMS.HealthTipCreateEditViewModel

// ❌ اشتباه
@model dynamic
@ViewBag.HealthTip
```

#### ✅ View Helpers:
```csharp
// ✅ درست
@Html.TextBoxFor(m => m.Title, new { @class = "form-control" })
@Html.ValidationMessageFor(m => m.Title)

// ❌ اشتباه
@Html.TextBox("Title", ViewBag.Title)
@ViewData["Title"]
```

#### ✅ Controller Actions:
```csharp
// ✅ درست
public async Task<ActionResult> Create(HealthTipCreateEditViewModel model)
{
    // استفاده از model
}

// ❌ اشتباه
public async Task<ActionResult> Create()
{
    var title = Request.Form["Title"];
    ViewBag.Title = title;
}
```

### 2.2 ممنوعیت استفاده از ViewBag و ViewData:

#### ✅ قوانین استفاده:
- **ممنوع**: استفاده از `ViewBag` و `ViewData` برای داده‌های اصلی و ضروری
- **مجاز**: استفاده از `ViewBag` و `ViewData` فقط برای موارد غیر حساس و غیر ضروری:
  - `ViewBag.Title` - برای عنوان صفحه (استثنا مجاز)
  - `ViewBag.MetaDescription` - برای توضیحات متا (استثنا مجاز)
  - تنظیمات UI کوچک و غیر ضروری (مثلاً `ViewBag.ShowHelp = true`)

#### ✅ مثال‌های درست و اشتباه:
```csharp
// ❌ اشتباه - استفاده از ViewBag برای داده‌های اصلی
public async Task<ActionResult> Index()
{
    var insurances = await _service.GetInsurancesAsync();
    var types = await _service.GetTypesAsync();
    
    ViewBag.InsuranceTypes = types;  // ❌ اشتباه!
    ViewBag.SelectedType = selectedType;  // ❌ اشتباه!
    
    return View(insurances);
}

// ✅ درست - استفاده از ViewModel
public async Task<ActionResult> Index()
{
    var insurances = await _service.GetInsurancesAsync();
    var types = await _service.GetTypesAsync();
    
    var viewModel = new InsuranceInfoIndexPageViewModel
    {
        InsuranceInfos = insurances,
        InsuranceTypes = types,
        SelectedType = selectedType
    };
    
    return View(viewModel);
}

// ✅ مجاز - استفاده از ViewBag برای تنظیمات UI کوچک
public ActionResult Create()
{
    ViewBag.PageTitle = "ایجاد جدید";  // ✅ مجاز - فقط برای UI
    ViewBag.ShowHelp = true;  // ✅ مجاز - فقط برای UI
    return View(model);
}
```

### 2.3 View Resolution (آدرس‌دهی View):

#### ✅ الزامات View Resolution:
- **الزامی**: تمام Controller Actions در Admin Area باید از `GetViewPath()` استفاده کنند
- **هدف**: جلوگیری از تداخل View resolution بین Areas و Views اصلی
- **استفاده**: فقط برای Controllers که از `BaseCMSController` ارث‌بری می‌کنند

#### ✅ مثال‌های درست و اشتباه:
```csharp
// ❌ اشتباه - بدون GetViewPath
public ActionResult Create()
{
    var model = new InsuranceInfoCreateEditViewModel();
    return View(model);  // ❌ ممکن است View اشتباه پیدا شود
}

// ✅ درست - با GetViewPath
public ActionResult Create()
{
    var model = new InsuranceInfoCreateEditViewModel();
    return View(GetViewPath("Create"), model);  // ✅ مسیر کامل View
}

// ✅ درست - برای تمام Actions
public async Task<ActionResult> Index()
{
    var result = await _service.GetItemsAsync();
    return View(GetViewPath("Index"), result.Data);
}

public async Task<ActionResult> Details(int id)
{
    var result = await _service.GetDetailsAsync(id);
    return View(GetViewPath("Details"), result.Data);
}

public ActionResult Create()
{
    return View(GetViewPath("Create"), model);
}

public async Task<ActionResult> Edit(int id)
{
    var result = await _service.GetForEditAsync(id);
    return View(GetViewPath("Edit"), result.Data);
}
```

#### ✅ نحوه پیاده‌سازی:
```csharp
// BaseCMSController.cs
public abstract class BaseCMSController : Controller
{
    protected string GetViewPath(string viewName)
    {
        string controllerName = GetType().Name.Replace("Controller", "");
        return $"~/Areas/Admin/Views/CMS/{controllerName}/{viewName}.cshtml";
    }
}
```

### 2.4 Checklist Strongly-Typed:

- [ ] تمام View ها دارای `@model` هستند
- [ ] هیچ استفاده از `ViewBag` برای داده‌های اصلی وجود ندارد
- [ ] هیچ استفاده از `ViewData` برای داده‌های اصلی وجود ندارد
- [ ] تمام ViewModels دارای Data Annotations هستند
- [ ] تمام Controller Actions دارای ViewModel parameter هستند
- [ ] تمام Partial Views دارای `@model` هستند
- [ ] تمام Enum ها دارای `DisplayAttribute` برای نام‌های فارسی هستند
- [ ] تمام داده‌های ضروری از طریق ViewModel منتقل می‌شوند
- [ ] تمام Controller Actions در Admin Area از `GetViewPath()` استفاده می‌کنند
- [ ] تمام `return View()` calls شامل `GetViewPath()` هستند

---

## 3️⃣ Bulletproof Coding (ضد گلوله)

### 3.1 اصول Bulletproof:

#### ✅ Error Handling:
```csharp
// ✅ درست
try
{
    var result = await _service.DoSomethingAsync();
    if (!result.Success)
    {
        _logger.Warning("خطا در انجام عملیات: {ErrorMessage}", result.Message);
        NotificationHelper.SetError(TempData, result.Message);
        return View(model);
    }
    // ...
}
catch (Exception ex)
{
    _logger.Error(ex, "خطا در انجام عملیات");
    NotificationHelper.SetError(TempData, "خطا در انجام عملیات");
    return View(model);
}

// ❌ اشتباه
var result = await _service.DoSomethingAsync();
// بدون بررسی result.Success
```

#### ✅ Null Checking:
```csharp
// ✅ درست
if (model == null)
{
    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
}

var title = model.Title ?? string.Empty;

// ❌ اشتباه
var title = model.Title; // ممکن است null باشد
```

#### ✅ Validation:
```csharp
// ✅ درست
if (!ModelState.IsValid)
{
    return View(model);
}

// ❌ اشتباه
// بدون بررسی ModelState
```

### 3.2 Checklist Bulletproof:

- [ ] تمام متدهای async دارای try-catch هستند
- [ ] تمام null reference ها بررسی شده‌اند
- [ ] تمام ModelState ها بررسی شده‌اند
- [ ] تمام Service Results بررسی شده‌اند
- [ ] تمام عملیات I/O دارای error handling هستند
- [ ] تمام لاگ‌ها با Serilog ثبت می‌شوند
- [ ] تمام خطاها به کاربر نمایش داده می‌شوند
- [ ] تمام عملیات حساس دارای authorization check هستند

---

## 4️⃣ رعایت اصول SRP (Single Responsibility Principle)

### 4.1 اصول SRP:

#### ✅ Controller:
```csharp
// ✅ درست - Controller فقط routing و orchestration
public class HealthTipController : BaseCMSController
{
    private readonly IHealthTipService _healthTipService;
    
    public async Task<ActionResult> Create(HealthTipCreateEditViewModel model)
    {
        // فقط orchestration
        var result = await _healthTipService.CreateHealthTipAsync(model);
        // ...
    }
}

// ❌ اشتباه - Controller شامل business logic
public class HealthTipController : Controller
{
    public async Task<ActionResult> Create(HealthTipCreateEditViewModel model)
    {
        // business logic در Controller - اشتباه!
        var healthTip = new HealthTip();
        healthTip.Title = model.Title;
        // ...
        _context.HealthTips.Add(healthTip);
        await _context.SaveChangesAsync();
    }
}
```

#### ✅ Service:
```csharp
// ✅ درست - Service فقط business logic
public class HealthTipService : IHealthTipService
{
    private readonly IHealthTipRepository _repository;
    
    public async Task<ServiceResult<HealthTip>> CreateHealthTipAsync(HealthTipCreateEditViewModel model)
    {
        // business logic
        var healthTip = new HealthTip { /* ... */ };
        _repository.Add(healthTip);
        await _context.SaveChangesAsync();
        return ServiceResult<HealthTip>.Successful(healthTip);
    }
}
```

#### ✅ Repository:
```csharp
// ✅ درست - Repository فقط data access
public class HealthTipRepository : IHealthTipRepository
{
    public async Task<HealthTip> GetByIdAsync(int id)
    {
        return await _context.HealthTips.FindAsync(id);
    }
}
```

### 4.2 Checklist SRP:

- [ ] Controller ها فقط routing و orchestration انجام می‌دهند
- [ ] Service ها فقط business logic دارند
- [ ] Repository ها فقط data access دارند
- [ ] ViewModels فقط data transfer انجام می‌دهند
- [ ] Helpers فقط utility functions دارند
- [ ] هر کلاس فقط یک مسئولیت دارد
- [ ] هیچ business logic در Controller وجود ندارد
- [ ] هیچ data access در Service وجود ندارد

---

## 5️⃣ سیستم پیام‌ها و هشدارها

### 5.1 Toastr Notifications:

#### ✅ استفاده از NotificationHelper:
```csharp
// ✅ درست
using static ClinicApp.Helpers.NotificationHelper;

NotificationHelper.SetSuccess(TempData, "عملیات با موفقیت انجام شد");
NotificationHelper.SetError(TempData, "خطا در انجام عملیات");
NotificationHelper.SetWarning(TempData, "هشدار");
NotificationHelper.SetInfo(TempData, "اطلاعات");

// ❌ اشتباه
TempData["Success"] = "عملیات با موفقیت انجام شد";
TempData["Error"] = "خطا در انجام عملیات";
```

#### ✅ View Implementation:
```razor
@* ✅ درست - پیام‌ها خودکار از _AdminLayout.cshtml نمایش داده می‌شوند *@
@* پیام‌ها با استفاده از Toastr نمایش داده می‌شوند - در _AdminLayout.cshtml *@

@* ❌ اشتباه - نمایش دستی alert *@
@if (TempData["Success"] != null)
{
    <div class="alert alert-success">@TempData["Success"]</div>
}
```

### 5.2 SweetAlert2 Confirmations:

#### ✅ JavaScript Implementation:
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

// ❌ اشتباه
if (confirm('آیا از انجام این عملیات اطمینان دارید؟')) {
    form.submit();
}
```

### 5.3 Checklist Notifications:

- [ ] تمام پیام‌های موفقیت با `NotificationHelper.SetSuccess()` هستند
- [ ] تمام پیام‌های خطا با `NotificationHelper.SetError()` هستند
- [ ] تمام پیام‌های هشدار با `NotificationHelper.SetWarning()` هستند
- [ ] تمام پیام‌های اطلاعات با `NotificationHelper.SetInfo()` هستند
- [ ] هیچ استفاده مستقیم از `TempData` وجود ندارد
- [ ] تمام confirmations با SweetAlert2 هستند
- [ ] هیچ استفاده از `confirm()` وجود ندارد
- [ ] تمام View ها alert های Bootstrap را حذف کرده‌اند

---

## 6️⃣ سیستم تقویم شمسی (Persian DatePicker)

### 6.1 الزامات تقویم شمسی:

#### ✅ استفاده از Persian DatePicker:
```razor
@* ✅ درست - استفاده از Partial View *@
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
@Html.Partial("_PersianDatePickerScript")

@* ❌ اشتباه - استفاده از datetime-local *@
@Html.TextBoxFor(m => m.StartDate, new { @class = "form-control", type = "datetime-local" })
```

#### ✅ Controller Implementation:
```csharp
// ✅ درست - Parse کردن تاریخ از hidden input
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(MyViewModel model)
{
    // Parse تاریخ از hidden input
    model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
    model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
    
    // ادامه عملیات...
}

// ❌ اشتباه - استفاده مستقیم از Model Binding
[HttpPost]
public async Task<ActionResult> Create(MyViewModel model)
{
    // تاریخ به صورت میلادی از Model Binding می‌آید - اشتباه!
    // ...
}
```

#### ✅ Helper Methods:
```csharp
// ✅ درست - استفاده از ControllerExtensions
model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);

// ✅ درست - استفاده از PersianDateHelper برای نمایش
var persianDate = PersianDateHelper.ToPersianDate(model.StartDate);

// ❌ اشتباه - تبدیل دستی تاریخ
var persianDate = model.StartDate.ToString("yyyy/MM/dd");
```

### 6.2 استانداردهای تقویم شمسی:

#### ✅ فیلدهای تاریخ:
- تمام فیلدهای تاریخ باید از `_PersianDatePicker` partial view استفاده کنند
- تمام فرم‌ها باید `_PersianDatePickerScript` را در بخش Scripts داشته باشند
- تمام Controller ها باید از `ParseDateFromHiddenInput` برای parse کردن تاریخ استفاده کنند

#### ✅ تبدیل تاریخ:
- تبدیل تاریخ میلادی به شمسی: `PersianDateHelper.ToPersianDate(DateTime)`
- تبدیل تاریخ شمسی به میلادی: `PersianDateHelper.ParsePersianDate(string)`
- نمایش تاریخ در Index: `PersianDateHelper.ToPersianDate(item.Date)`

#### ✅ مدیریت Timezone:
- تمام تاریخ‌ها باید به صورت Local ذخیره شوند
- استفاده از `DateTime.SpecifyKind(date, DateTimeKind.Local)` برای اطمینان از Local بودن
- استفاده از `.Date` برای حذف زمان و فقط نگه داشتن تاریخ

### 6.3 Checklist تقویم شمسی:

- [ ] تمام فیلدهای تاریخ از `_PersianDatePicker` استفاده می‌کنند
- [ ] تمام فرم‌ها `_PersianDatePickerScript` را در Scripts دارند
- [ ] تمام Controller ها از `ParseDateFromHiddenInput` استفاده می‌کنند
- [ ] تمام نمایش تاریخ‌ها از `PersianDateHelper.ToPersianDate` استفاده می‌کنند
- [ ] هیچ استفاده از `datetime-local` وجود ندارد
- [ ] هیچ استفاده از `ToString("yyyy/MM/dd")` برای تاریخ شمسی وجود ندارد
- [ ] تمام تاریخ‌ها به صورت Local ذخیره می‌شوند
- [ ] تمام تاریخ‌ها فقط تاریخ دارند (بدون زمان) - استفاده از `.Date`

### 6.4 مراجع:

- `Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md` - راهنمای کامل ماژول
- `Helpers/PersianDateHelper.cs` - Helper Methods برای تبدیل تاریخ
- `Helpers/ControllerExtensions.cs` - Extension Methods برای Controller
- `Areas/Admin/Views/Shared/_PersianDatePicker.cshtml` - Partial View
- `Areas/Admin/Views/Shared/_PersianDatePickerScript.cshtml` - JavaScript Script
- `Content/js/persian-datepicker-manager.js` - Manager Module

---

## 7️⃣ سیستم آپلود تصویر (Image Upload System)

### 7.1 الزامات آپلود تصویر:

#### ✅ استفاده از IImageUploadService:
```csharp
// ✅ درست - تزریق IImageUploadService
public class HealthTipController : BaseCMSController
{
    private readonly IImageUploadService _imageUploadService;
    
    public HealthTipController(
        IHealthTipService healthTipService,
        ICurrentUserService currentUserService,
        IImageUploadService imageUploadService)
    {
        _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
    }
}

// ❌ اشتباه - آپلود دستی تصویر
public class HealthTipController : Controller
{
    public async Task<ActionResult> Create(HealthTipCreateEditViewModel model)
    {
        var file = Request.Files["ImageFile"];
        // آپلود دستی - اشتباه!
        file.SaveAs(Server.MapPath("~/Content/Images/..." + file.FileName));
    }
}
```

#### ✅ Controller Implementation:
```csharp
// ✅ درست - استفاده از ProcessImageUpload
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

// ✅ درست - متد ProcessImageUpload
private async Task ProcessImageUpload(HealthTipCreateEditViewModel model)
{
    try
    {
        var imageFile = Request.Files["ImageFile"];
        var thumbnailFile = Request.Files["ThumbnailFile"];
        
        // اگر تصویر اصلی آپلود شده
        if (imageFile != null && imageFile.ContentLength > 0)
        {
            var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                imageFile,
                HealthTipImageUploadPath,        // ~/Content/Images/health-tips
                HealthTipThumbnailUploadPath,   // ~/Content/Images/health-tips/thumbnails
                ThumbnailWidth,                  // 300
                ThumbnailHeight,                 // 300
                MaxImageWidth,                   // 1920
                MaxImageHeight);                 // 1080
            
            if (!uploadResult.Success)
            {
                _logger.Warning("خطا در آپلود تصویر: {ErrorMessage}", uploadResult.Message);
                NotificationHelper.SetError(TempData, uploadResult.Message);
                ModelState.AddModelError("ImageFile", uploadResult.Message);
                return;
            }
            
            model.ImageUrl = uploadResult.Data.ImageUrl;
            
            // اگر thumbnail جداگانه آپلود نشده، از thumbnail خودکار استفاده کن
            if (thumbnailFile == null || thumbnailFile.ContentLength == 0)
            {
                model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
            }
        }
        
        // اگر thumbnail جداگانه آپلود شده
        if (thumbnailFile != null && thumbnailFile.ContentLength > 0)
        {
            var thumbnailResult = _imageUploadService.UploadImageWithThumbnail(
                thumbnailFile,
                HealthTipThumbnailUploadPath,
                HealthTipThumbnailUploadPath,
                ThumbnailWidth,
                ThumbnailHeight,
                ThumbnailWidth,
                ThumbnailHeight);
            
            if (!thumbnailResult.Success)
            {
                _logger.Warning("خطا در آپلود thumbnail: {ErrorMessage}", thumbnailResult.Message);
                NotificationHelper.SetError(TempData, thumbnailResult.Message);
                ModelState.AddModelError("ThumbnailFile", thumbnailResult.Message);
                return;
            }
            
            model.ThumbnailUrl = thumbnailResult.Data.ImageUrl;
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

#### ✅ View Implementation:
```razor
@* ✅ درست - استفاده از File Input با Preview *@
@using (Html.BeginForm("Create", "HealthTip", FormMethod.Post, new { enctype = "multipart/form-data" }))
{
    <div class="form-group">
        @Html.LabelFor(m => m.ImageUrl, "تصویر اصلی")
        <div class="custom-file">
            <input type="file" class="custom-file-input" id="ImageFile" name="ImageFile" accept="image/jpeg,image/jpg,image/png,image/gif,image/webp">
            <label class="custom-file-label" for="ImageFile">انتخاب تصویر اصلی...</label>
        </div>
        <small class="form-text text-muted">
            <i class="fas fa-info-circle"></i> فرمت‌های مجاز: JPG, PNG, GIF, WEBP | حداکثر حجم: 5 مگابایت | ابعاد توصیه شده: 1920x1080
        </small>
        @Html.HiddenFor(m => m.ImageUrl)
        @Html.ValidationMessageFor(m => m.ImageUrl)
        <div id="imagePreview" class="mt-2" style="display: none;">
            <img id="imagePreviewImg" src="" alt="پیش‌نمایش تصویر" class="img-thumbnail" style="max-width: 200px; max-height: 200px;">
        </div>
        @if (!string.IsNullOrEmpty(Model.ImageUrl))
        {
            <div class="mt-2">
                <img src="@Model.ImageUrl" alt="تصویر فعلی" class="img-thumbnail" style="max-width: 200px; max-height: 200px;">
                <br />
                <small class="text-muted">تصویر فعلی</small>
            </div>
        }
    </div>
}

@* ❌ اشتباه - استفاده از TextBox برای مسیر تصویر *@
@Html.TextBoxFor(m => m.ImageUrl, new { @class = "form-control", placeholder = "/Content/Images/..." })
```

#### ✅ JavaScript Implementation:
```javascript
// ✅ درست - Preview و Validation
(function() {
    'use strict';
    
    function domReady(fn) {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', fn);
        } else {
            fn();
        }
    }
    
    domReady(function() {
        var imageFileInput = document.getElementById('ImageFile');
        var imagePreview = document.getElementById('imagePreview');
        var imagePreviewImg = document.getElementById('imagePreviewImg');
        var imageFileLabel = document.querySelector('label[for="ImageFile"]');
        
        if (imageFileInput) {
            imageFileInput.addEventListener('change', function(e) {
                var file = e.target.files[0];
                if (file) {
                    // بررسی نوع فایل
                    if (!file.type.match('image.*')) {
                        if (typeof AdminNotification !== 'undefined') {
                            AdminNotification.error('لطفاً یک فایل تصویری انتخاب کنید.');
                        }
                        e.target.value = '';
                        return;
                    }
                    
                    // بررسی حجم فایل (5 مگابایت)
                    if (file.size > 5 * 1024 * 1024) {
                        if (typeof AdminNotification !== 'undefined') {
                            AdminNotification.error('حجم فایل نباید بیشتر از 5 مگابایت باشد.');
                        }
                        e.target.value = '';
                        return;
                    }
                    
                    // نمایش پیش‌نمایش
                    var reader = new FileReader();
                    reader.onload = function(e) {
                        imagePreviewImg.src = e.target.result;
                        imagePreview.style.display = 'block';
                    };
                    reader.readAsDataURL(file);
                    
                    // به‌روزرسانی label
                    if (imageFileLabel) {
                        imageFileLabel.textContent = file.name;
                    }
                }
            });
        }
    });
})();
```

### 7.2 استانداردهای آپلود تصویر:

#### ✅ Configuration:
```csharp
// ✅ درست - تعریف Constants در Controller
private const string HealthTipImageUploadPath = "~/Content/Images/health-tips";
private const string HealthTipThumbnailUploadPath = "~/Content/Images/health-tips/thumbnails";
private const int ThumbnailWidth = 300;
private const int ThumbnailHeight = 300;
private const int MaxImageWidth = 1920;  // Full HD
private const int MaxImageHeight = 1080; // Full HD
```

#### ✅ Security Standards:
- **File Type Validation**: بررسی ContentType و Extension
- **File Signature Validation**: بررسی header فایل (امنیت بالا)
- **File Size Validation**: حداکثر 5 مگابایت
- **Dimension Validation**: حداقل 100x100، حداکثر 4000x4000
- **Filename Sanitization**: پاکسازی نام فایل برای جلوگیری از Path Traversal
- **Unique Filename**: استفاده از GUID برای جلوگیری از Overwrite

#### ✅ Performance Standards:
- **Auto Resize**: Resize خودکار برای کاهش حجم
- **Thumbnail Generation**: ایجاد thumbnail خودکار با کیفیت بالا
- **Quality Optimization**: بهینه‌سازی کیفیت تصویر (90% برای JPEG)

### 7.3 Checklist آپلود تصویر:

- [ ] `IImageUploadService` در Controller تزریق شده است
- [ ] Constants برای مسیرها و ابعاد تعریف شده‌اند
- [ ] متد `ProcessImageUpload` پیاده‌سازی شده است
- [ ] `ProcessImageUpload` در Create Action فراخوانی می‌شود
- [ ] `ProcessImageUpload` در Edit Action فراخوانی می‌شود
- [ ] Form دارای `enctype="multipart/form-data"` است
- [ ] File Input با `accept` attribute تعریف شده است
- [ ] Image Preview پیاده‌سازی شده است
- [ ] JavaScript Validation برای نوع و حجم فایل پیاده‌سازی شده است
- [ ] نمایش تصویر فعلی در Edit View پیاده‌سازی شده است
- [ ] تمام خطاها با `NotificationHelper` نمایش داده می‌شوند
- [ ] تمام خطاها در `ModelState` ثبت می‌شوند
- [ ] تمام عملیات لاگ می‌شوند

### 7.4 مراجع:

- `Docs/IMAGE_UPLOAD_SYSTEM_GUIDE.md` - راهنمای کامل سیستم آپلود تصویر
- `Interfaces/IImageUploadService.cs` - Interface سرویس
- `Services/ImageUploadService.cs` - پیاده‌سازی سرویس
- `Areas/Admin/Controllers/CMS/BlogPostController.cs` - مثال پیاده‌سازی
- `Areas/Admin/Controllers/CMS/HealthTipController.cs` - مثال پیاده‌سازی

---

## 8️⃣ سیستم ویرایشگر متن (CKEditor)

### 8.1 الزامات CKEditor:

#### ✅ استفاده از CKEditor:

**CKEditor برای فیلدهای متنی طولانی استفاده می‌شود:**
- محتوای مقالات (BlogPost.Content)
- محتوای نکات سلامتی (HealthTip.Content)
- محتوای اطلاعیه‌ها (Announcement.Content)
- مشخصات فنی تجهیزات (MedicalEquipment.TechnicalSpecifications)
- هر فیلد متنی که نیاز به فرمت‌بندی دارد

#### ✅ View Implementation:

**1. بارگذاری Scripts:**

```html
@* در @section Scripts *@
@Html.Partial("_CKEditorScript")
```

**2. ایجاد TextArea با ID مشخص:**

```html
@Html.TextAreaFor(m => m.Content, new { 
    @class = "form-control", 
    id = "contentEditor",
    rows = "10"
})
@Html.ValidationMessageFor(m => m.Content, "", new { @class = "text-danger" })
```

**3. Initialize کردن CKEditor:**

```html
@* در @section Scripts، بعد از _CKEditorScript *@
@{
    ViewBag.CKEditorSelector = "#contentEditor";
    ViewBag.CKEditorHeight = 400; // ارتفاع دلخواه
}
@Html.Partial("_CKEditorInit")
```

**4. مثال کامل:**

```html
@model ClinicApp.ViewModels.CMS.BlogPostCreateEditViewModel

@using (Html.BeginForm("Create", "BlogPost", FormMethod.Post, 
    new { @class = "form-horizontal", role = "form" }))
{
    @Html.AntiForgeryToken()
    
    <div class="form-group">
        @Html.LabelFor(m => m.Content)
        @Html.TextAreaFor(m => m.Content, new { 
            @class = "form-control", 
            id = "contentEditor",
            rows = "10"
        })
        @Html.ValidationMessageFor(m => m.Content, "", new { @class = "text-danger" })
        <small class="form-text text-muted">
            محتوای مقاله را با استفاده از ویرایشگر وارد کنید.
        </small>
    </div>
    
    <button type="submit" class="btn btn-primary">ذخیره</button>
}

@section Scripts {
    @Scripts.Render("~/bundles/jqueryval")
    
    @* بارگذاری CKEditor Scripts *@
    @Html.Partial("_CKEditorScript")
    
    @* Initialize CKEditor *@
    @{
        ViewBag.CKEditorSelector = "#contentEditor";
        ViewBag.CKEditorHeight = 400;
    }
    @Html.Partial("_CKEditorInit")
}
```

#### ✅ ViewModel Implementation:

**اضافه کردن `[AllowHtml]` به فیلدهای HTML:**

```csharp
public class BlogPostCreateEditViewModel
{
    [Required(ErrorMessage = "محتوای مقاله الزامی است.")]
    [AllowHtml] // ✅ الزامی برای فیلدهای CKEditor
    [Display(Name = "محتوای مقاله")]
    public string Content { get; set; }
}
```

#### ✅ Controller Implementation:

**1. اضافه کردن `[ValidateInput(false)]` به POST Actions:**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[ValidateInput(false)] // ✅ الزامی برای فیلدهای CKEditor
public async Task<ActionResult> Create(BlogPostCreateEditViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }
    
    // پردازش model.Content که شامل HTML است
    // ...
}
```

**2. مثال کامل Controller:**

```csharp
public class BlogPostController : Controller
{
    [HttpGet]
    public ActionResult Create()
    {
        var model = new BlogPostCreateEditViewModel();
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateInput(false)] // ✅ الزامی
    public async Task<ActionResult> Create(BlogPostCreateEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // model.Content شامل HTML از CKEditor است
            // می‌توانید آن را مستقیماً ذخیره کنید یا پردازش کنید
            
            // ذخیره در دیتابیس
            // ...
            
            NotificationHelper.SetSuccess(TempData, "مقاله با موفقیت ایجاد شد.");
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ایجاد مقاله");
            NotificationHelper.SetError(TempData, "خطا در ایجاد مقاله.");
            return View(model);
        }
    }
}
```

### 8.2 استانداردهای CKEditor:

#### ✅ Configuration:

**1. Web.config:**

```xml
<appSettings>
    <!-- استفاده از نسخه محلی (پیش‌فرض) -->
    <add key="CKEditor:UseCDN" value="false" />
</appSettings>
```

**2. نسخه CKEditor:**
- **نسخه محلی:** `Content/plugins/ckeditor/` (4.22.1 Standard - رایگان)
- **نسخه CDN:** `https://cdn.ckeditor.com/4.22.1/standard/ckeditor.js` (رایگان)

**3. تنظیمات پیش‌فرض:**
- زبان: فارسی (`language: 'fa'`)
- جهت: راست‌به‌چپ (`contentsLangDirection: 'rtl'`)
- فونت: Tahoma
- ارتفاع: قابل تنظیم (پیش‌فرض: 300px)

#### ✅ Security Standards:

**1. HTML Sanitization:**
- CKEditor به صورت پیش‌فرض HTML را sanitize می‌کند
- برای محتوای پزشکی، HTML کامل مجاز است (`allowedContent: true`)
- در صورت نیاز، می‌توانید HTML را قبل از ذخیره sanitize کنید

**2. XSS Protection:**
- استفاده از `[AllowHtml]` فقط برای فیلدهای مورد اعتماد
- اعتبارسنجی سمت سرور برای محتوای HTML
- استفاده از `Html.Raw()` فقط برای نمایش محتوای ذخیره‌شده

#### ✅ Performance Standards:

**1. Lazy Loading:**
- CKEditor فقط در صفحاتی که نیاز است بارگذاری می‌شود
- استفاده از Partial Views برای مدیریت بهتر

**2. Caching:**
- فایل‌های CKEditor در مرورگر cache می‌شوند
- استفاده از CDN برای بارگذاری سریع‌تر (در صورت نیاز)

### 8.3 Checklist CKEditor:

**قبل از استفاده از CKEditor:**

- [ ] فیلد متنی طولانی است و نیاز به فرمت‌بندی دارد
- [ ] `[AllowHtml]` به ViewModel اضافه شده است
- [ ] `[ValidateInput(false)]` به POST Action اضافه شده است
- [ ] `_CKEditorScript` در `@section Scripts` بارگذاری شده است
- [ ] `_CKEditorInit` با selector و height مناسب اضافه شده است
- [ ] TextArea دارای ID منحصر به فرد است
- [ ] Validation Messages برای فیلد اضافه شده است
- [ ] Help Text برای راهنمایی کاربر اضافه شده است

**بعد از پیاده‌سازی:**

- [ ] CKEditor به درستی بارگذاری می‌شود
- [ ] محتوای فارسی به درستی نمایش داده می‌شود
- [ ] جهت راست‌به‌چپ به درستی اعمال می‌شود
- [ ] محتوا به درستی ذخیره می‌شود
- [ ] محتوا به درستی در Edit نمایش داده می‌شود
- [ ] HTML به درستی در نمایش (Index/Details) render می‌شود
- [ ] خطاهای JavaScript در Console وجود ندارد

### 8.4 مراجع:

- `Docs/CKEDITOR_USAGE_GUIDE.md` - راهنمای کامل استفاده از CKEditor
- `Docs/CKEDITOR_QUICK_START.md` - راهنمای سریع شروع
- `Areas/Admin/Views/Shared/_CKEditorScript.cshtml` - Script بارگذاری
- `Areas/Admin/Views/Shared/_CKEditorInit.cshtml` - Initialize Editor
- `Content/plugins/ckeditor/` - فایل‌های CKEditor
- `Areas/Admin/Controllers/CMS/BlogPostController.cs` - مثال پیاده‌سازی
- `Areas/Admin/Controllers/CMS/HealthTipController.cs` - مثال پیاده‌سازی
- `Areas/Admin/Controllers/CMS/MedicalEquipmentController.cs` - مثال پیاده‌سازی

---

## 9️⃣ استانداردهای رنگ‌بندی برای محیط Production درمانی

### 9.1 الزامات رنگ‌بندی:

#### ✅ ممنوعیت استفاده از رنگ‌های جیق و جلف:

**❌ ممنوع:**
- استفاده از Gradient های رنگی پیچیده (مثل `linear-gradient(135deg, #667eea 0%, #764ba2 100%)`)
- استفاده از رنگ‌های روشن و جیق (مثل `#f093fb`, `#f5576c`)
- استفاده از رنگ‌های نئون و درخشان
- استفاده از رنگ‌های متضاد و چشم‌آزار

**✅ مجاز:**
- استفاده از رنگ‌های رسمی و اداری
- استفاده از رنگ‌های ساده و یکنواخت
- استفاده از رنگ‌های مناسب محیط درمانی

#### ✅ پالت رنگ استاندارد:

```css
:root {
    /* رنگ‌های اصلی */
    --medical-primary: #2c5aa0;      /* آبی درمانی - رنگ اصلی */
    --medical-secondary: #6c757d;    /* خاکستری - رنگ ثانویه */
    --medical-success: #28a745;      /* سبز - موفقیت */
    --medical-danger: #dc3545;       /* قرمز - خطا/هشدار */
    --medical-warning: #ffc107;       /* زرد - توجه */
    --medical-info: #17a2b8;         /* آبی روشن - اطلاعات */
    
    /* رنگ‌های پس‌زمینه */
    --medical-light: #f8f9fa;        /* خاکستری روشن */
    --medical-bg: #ffffff;           /* سفید */
    
    /* رنگ‌های متن */
    --medical-dark: #212529;         /* تیره - متن اصلی */
    --medical-text: #212529;         /* متن اصلی */
    --medical-text-muted: #6c757d;   /* متن ثانویه */
    
    /* رنگ‌های Border */
    --medical-border: #dee2e6;       /* حاشیه */
}
```

### 9.2 استانداردهای استفاده:

#### ✅ Header و Card Header:

```css
/* ✅ درست */
.card-header {
    background-color: var(--medical-primary);
    color: white;
    border-radius: 12px 12px 0 0;
}

/* ❌ اشتباه */
.card-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}
```

#### ✅ Badge و Label:

```css
/* ✅ درست */
.badge-primary {
    background-color: var(--medical-primary);
    color: white;
    border-radius: 6px;
}

/* ❌ اشتباه */
.feature-badge {
    background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
    border-radius: 25px;
}
```

#### ✅ Button:

```css
/* ✅ درست */
.btn-primary {
    background-color: var(--medical-primary);
    border-color: var(--medical-primary);
    border-radius: 6px;
}

/* ❌ اشتباه */
.btn-primary {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-radius: 30px;
}
```

#### ✅ Card و Container:

```css
/* ✅ درست */
.card {
    background-color: var(--medical-bg);
    border: 1px solid var(--medical-border);
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

/* ❌ اشتباه */
.card {
    background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
    border-radius: 20px;
}
```

### 9.3 Checklist رنگ‌بندی:

**قبل از Commit:**

- [ ] تمام Gradient های رنگی حذف شده‌اند
- [ ] از رنگ‌های استاندارد `--medical-*` استفاده شده است
- [ ] رنگ‌های جیق و جلف استفاده نشده است
- [ ] رنگ‌بندی مناسب محیط Production درمانی است
- [ ] تمام Badge ها از رنگ‌های ساده استفاده می‌کنند
- [ ] تمام Button ها از رنگ‌های رسمی استفاده می‌کنند
- [ ] تمام Card Header ها از `--medical-primary` استفاده می‌کنند
- [ ] Border-radius مناسب است (6px تا 12px، نه 20px+)
- [ ] رنگ متن قابل خواندن است (contrast ratio مناسب)

**بعد از پیاده‌سازی:**

- [ ] صفحه در محیط Production رسمی به نظر می‌رسد
- [ ] رنگ‌بندی یکنواخت و حرفه‌ای است
- [ ] هیچ رنگ جیق و جلفی وجود ندارد
- [ ] تمام عناصر از پالت رنگ استاندارد استفاده می‌کنند

### 9.4 مراجع:

- `Areas/Admin/Views/CMS/MedicalEquipment/Index.cshtml` - مثال پیاده‌سازی صحیح
- `Views/MedicalEquipment/Index.cshtml` - مثال پیاده‌سازی صحیح
- `Views/MedicalEquipment/Details.cshtml` - مثال پیاده‌سازی صحیح
- `Views/Home/Sections/_MedicalEquipmentSection.cshtml` - مثال پیاده‌سازی صحیح

---

## 🔟 بهینه‌سازی و بازطراحی با دقت 100%

### 8.1 Code Review Checklist:

#### ✅ قبل از Commit:
- [ ] تمام کدها طبق استانداردهای این سند هستند
- [ ] تمام تست‌ها پاس شده‌اند
- [ ] تمام linter errors برطرف شده‌اند
- [ ] تمام warnings برطرف شده‌اند
- [ ] تمام TODO ها بررسی شده‌اند
- [ ] تمام comments به‌روزرسانی شده‌اند
- [ ] تمام لاگ‌ها اضافه شده‌اند
- [ ] تمام error handling ها پیاده‌سازی شده‌اند

#### ✅ بررسی همه‌جانبه:
- [ ] بررسی Security (SQL Injection, XSS, CSRF)
- [ ] بررسی Performance (N+1 queries, caching)
- [ ] بررسی Scalability (async/await, connection pooling)
- [ ] بررسی Maintainability (code organization, naming)
- [ ] بررسی Testability (dependency injection, mocking)

### 8.2 Performance Checklist:

- [ ] تمام database queries بهینه شده‌اند
- [ ] تمام N+1 queries برطرف شده‌اند
- [ ] تمام عملیات I/O async هستند
- [ ] تمام عملیات سنگین در background انجام می‌شوند
- [ ] تمام static resources cached هستند
- [ ] تمام JavaScript minified هستند
- [ ] تمام CSS minified هستند
- [ ] تمام images optimized هستند

---

## 1️⃣1️⃣ نقشه راه و TODO List

### 11.1 Template TODO List:

```markdown
## 📋 TODO List

### Phase 1: Analysis & Design
- [ ] تحلیل نیازمندی‌ها
- [ ] طراحی Entity و Configuration
- [ ] طراحی ViewModels
- [ ] طراحی Repository Interface
- [ ] طراحی Service Interface

### Phase 2: Implementation
- [ ] پیاده‌سازی Repository
- [ ] پیاده‌سازی Service
- [ ] پیاده‌سازی Controller
- [ ] پیاده‌سازی Views
- [ ] پیاده‌سازی Dependency Injection

### Phase 3: UI/UX
- [ ] بهینه‌سازی UI/UX
- [ ] پیاده‌سازی Toastr Notifications
- [ ] پیاده‌سازی SweetAlert Confirmations
- [ ] تست Responsive Design
- [ ] تست Accessibility

### Phase 4: Testing & Optimization
- [ ] تست Unit Tests
- [ ] تست Integration Tests
- [ ] بهینه‌سازی Performance
- [ ] بررسی Security
- [ ] Code Review

### Phase 5: Documentation
- [ ] مستندسازی API
- [ ] مستندسازی UI/UX
- [ ] به‌روزرسانی README
- [ ] ایجاد User Guide
```

### 11.2 Roadmap Template:

```markdown
## 🗺️ Roadmap

### Sprint 1: Foundation (Week 1-2)
- Entity Design
- Repository Implementation
- Service Implementation
- Basic CRUD Operations

### Sprint 2: UI/UX (Week 3-4)
- View Implementation
- UI/UX Optimization
- Notification System
- Responsive Design

### Sprint 3: Advanced Features (Week 5-6)
- Advanced Search
- Filtering
- Pagination
- Export/Import

### Sprint 4: Testing & Deployment (Week 7-8)
- Testing
- Bug Fixing
- Performance Optimization
- Deployment
```

---

## 1️⃣2️⃣ استانداردهای کدنویسی

### 12.1 Naming Conventions:

```csharp
// ✅ درست
public class HealthTipService : IHealthTipService
{
    private readonly IHealthTipRepository _healthTipRepository;
    
    public async Task<ServiceResult<HealthTip>> CreateHealthTipAsync(HealthTipCreateEditViewModel model)
    {
        // ...
    }
}

// ❌ اشتباه
public class HealthTipSvc : IHealthTipSvc
{
    private IHealthTipRepo repo;
    
    public async Task<Result> Create(ViewModel m)
    {
        // ...
    }
}
```

### 12.2 Code Organization:

```
Areas/
  Admin/
    Controllers/
      CMS/
        HealthTipController.cs
    Views/
      CMS/
        HealthTip/
          Index.cshtml
          Create.cshtml
          Edit.cshtml
          Details.cshtml
Services/
  CMS/
    HealthTipService.cs
Repositories/
  CMS/
    HealthTipRepository.cs
ViewModels/
  CMS/
    HealthTipViewModels.cs
Interfaces/
  CMS/
    IHealthTipService.cs
    IHealthTipRepository.cs
```

---

## 1️⃣3️⃣ چک‌لیست نهایی قبل از Production

### 13.1 Security Checklist:
- [ ] تمام inputs validated هستند
- [ ] تمام outputs encoded هستند
- [ ] تمام SQL queries parameterized هستند
- [ ] تمام forms دارای CSRF protection هستند
- [ ] تمام sensitive data encrypted هستند
- [ ] تمام authentication checks انجام شده‌اند
- [ ] تمام authorization checks انجام شده‌اند

### 13.2 Quality Checklist:
- [ ] تمام کدها طبق این استانداردها هستند
- [ ] تمام tests پاس شده‌اند
- [ ] تمام documentation به‌روز است
- [ ] تمام performance benchmarks برآورده شده‌اند
- [ ] تمام accessibility requirements برآورده شده‌اند

---

## 1️⃣4️⃣ منابع و مراجع

### 14.1 Documentation:
- [ASP.NET MVC Best Practices](https://docs.microsoft.com/en-us/aspnet/mvc/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [Toastr Documentation](https://github.com/CodeSeven/toastr)
- [SweetAlert2 Documentation](https://sweetalert2.github.io/)

### 14.2 Internal References:
- `Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md` - راهنمای کامل تقویم شمسی
- `Docs/NOTIFICATION_SYSTEM_GUIDE.md` - راهنمای سیستم پیام‌ها
- `Docs/IMAGE_UPLOAD_GUIDE.md` - راهنمای آپلود تصویر
- `Docs/BLOG_COMMENT_LIKE_SYSTEM_GUIDE.md` - راهنمای سیستم کامنت و لایک

---

## ✅ تایید و امضا

این قرارداد باید توسط تمام توسعه‌دهندگان پروژه رعایت شود. هر تغییری در این استانداردها باید با تایید تیم فنی انجام شود.

**تاریخ آخرین به‌روزرسانی:** 2024  
**نسخه:** 1.0.0  
**وضعیت:** فعال

---

## 📞 پشتیبانی

در صورت هرگونه سوال یا ابهام، لطفاً با تیم فنی تماس بگیرید.

**نکته:** این سند به صورت مداوم به‌روزرسانی می‌شود. لطفاً همیشه از آخرین نسخه استفاده کنید.

