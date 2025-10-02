# 🗓️ **تحلیل کامل PersianDateController و کاربردهای آن**

> **پروژه**: کلینیک شفا  
> **موضوع**: مدیریت تاریخ شمسی در Controllers  
> **تاریخ**: 1404/07/11  
> **اهمیت**: 🔥 بسیار بالا (برای تمام ماژول‌های دارای تاریخ)

---

## 📑 **فهرست**

1. [هدف و کاربرد](#هدف-و-کاربرد)
2. [معماری و طراحی](#معماری-و-طراحی)
3. [دو نوع Base Controller](#دو-نوع-base-controller)
4. [نحوه استفاده در پروژه](#نحوه-استفاده-در-پروژه)
5. [مثال‌های کاربردی](#مثالهای-کاربردی)
6. [Best Practices](#best-practices)
7. [مشکلات رایج و راه‌حل](#مشکلات-رایج-و-راهحل)
8. [پیشنهادات بهینه‌سازی](#پیشنهادات-بهینهسازی)

---

## 🎯 **هدف و کاربرد**

### **مشکل اصلی که حل می‌کند:**

در پروژه‌های ایرانی، تبدیل تاریخ شمسی به میلادی و برعکس یک **چالش تکراری** است:

```csharp
// ❌ مشکل: کد تکراری در هر Controller

public ActionResult Create(MyViewModel model)
{
    // تبدیل تاریخ شمسی به میلادی
    if (!string.IsNullOrEmpty(model.StartDateShamsi))
    {
        model.StartDate = ConvertPersianToGregorian(model.StartDateShamsi);
    }
    
    if (ModelState.IsValid)
    {
        // ذخیره...
    }
    return View(model);
}

// همین کد در 50+ Controller تکرار می‌شود! 😓
```

### **راه‌حل: PersianDateController:**

```csharp
// ✅ راه‌حل: Base Controller با مسئولیت واحد

public class MyController : PersianDateCrudController<MyViewModel, MyEntity>
{
    // فقط منطق اصلی کسب‌وکار!
    // تبدیل تاریخ‌ها به صورت خودکار انجام می‌شود
}
```

---

## 🏗️ **معماری و طراحی**

### **اصول SOLID:**

#### ✅ **1. Single Responsibility Principle (SRP):**
```
PersianDateController = فقط مسئول مدیریت تاریخ‌های شمسی
```

#### ✅ **2. Open/Closed Principle (OCP):**
```
باز برای گسترش (Inheritance) + بسته برای تغییر (Base Behavior)
```

#### ✅ **3. Liskov Substitution Principle (LSP):**
```
هر Controller که از PersianDateController ارث می‌برد، 
همان رفتار پیش‌بینی شده را دارد
```

#### ✅ **4. Dependency Inversion Principle (DIP):**
```
به ViewModel های Abstract وابسته است (PersianDateViewModel)
```

---

## 📦 **دو نوع Base Controller**

### **1️⃣ PersianDateController (ساده):**

```csharp
public abstract class PersianDateController : Controller
{
    // ✅ 4 متد محافظت شده (Protected)
    protected virtual T PrepareModelForCreate<T>(T model) where T : PersianDateViewModel
    protected virtual T PrepareModelForEdit<T>(T model) where T : PersianDateViewModel
    protected virtual T PrepareModelForPost<T>(T model) where T : PersianDateViewModel
    protected virtual bool ValidateModelWithPersianDates<T>(T model) where T : PersianDateViewModel
    
    // ✅ 2 متد Helper
    protected virtual void AddDateValidationError(string fieldName, string errorMessage)
    protected virtual void AddDateComparisonError(string fieldName, string errorMessage)
}
```

**کاربرد:**
- برای Controller های ساده که فقط نیاز به Helper Methods دارند
- زمانی که می‌خواهید خودتان CRUD را پیاده‌سازی کنید
- برای Controller های API

**مثال:**
```csharp
public class ReportController : PersianDateController
{
    public async Task<ActionResult> GenerateReport(ReportViewModel model)
    {
        // آماده‌سازی Model قبل از پردازش
        model = PrepareModelForPost(model);
        
        if (ModelState.IsValid && ValidateModelWithPersianDates(model))
        {
            // تولید گزارش...
        }
        
        return View(model);
    }
}
```

---

### **2️⃣ PersianDateCrudController<TViewModel, TEntity> (کامل):**

```csharp
public abstract class PersianDateCrudController<TViewModel, TEntity> : PersianDateController
    where TViewModel : PersianDateViewModel
    where TEntity : class
{
    // ✅ متدهای Abstract (باید پیاده‌سازی شوند)
    protected abstract TViewModel CreateNewModel();
    protected abstract Task<TViewModel> GetModelByIdAsync(int id);
    protected abstract Task<bool> SaveModelAsync(TViewModel model);
    protected abstract Task<bool> UpdateModelAsync(TViewModel model);
    
    // ✅ متدهای Virtual (پیاده‌سازی شده، قابل Override)
    public virtual async Task<ActionResult> Create()        // GET
    public virtual async Task<ActionResult> Create(TViewModel model)  // POST
    public virtual async Task<ActionResult> Edit(int id)    // GET
    public virtual async Task<ActionResult> Edit(TViewModel model)    // POST
}
```

**کاربرد:**
- برای Controller های CRUD استاندارد
- زمانی که می‌خواهید از پیاده‌سازی آماده استفاده کنید
- برای کاهش کد تکراری در CRUD

**مثال:**
```csharp
public class InsurancePlanController : PersianDateCrudController<InsurancePlanViewModel, InsurancePlan>
{
    private readonly IInsurancePlanService _service;

    // فقط 4 متد را پیاده‌سازی کنید
    protected override InsurancePlanViewModel CreateNewModel()
    {
        return new InsurancePlanViewModel();
    }

    protected override async Task<InsurancePlanViewModel> GetModelByIdAsync(int id)
    {
        var result = await _service.GetForEditAsync(id);
        return result.Data;
    }

    protected override async Task<bool> SaveModelAsync(InsurancePlanViewModel model)
    {
        var result = await _service.CreateAsync(model);
        return result.Success;
    }

    protected override async Task<bool> UpdateModelAsync(InsurancePlanViewModel model)
    {
        var result = await _service.UpdateAsync(model);
        return result.Success;
    }
    
    // ✨ تمام! Create/Edit (GET/POST) به صورت خودکار کار می‌کنند
}
```

---

## 🔄 **جریان کار (Workflow)**

### **فرآیند کامل ایجاد/ویرایش با تاریخ شمسی:**

```
┌─────────────────────────────────────────────────────────────────┐
│                    📝 GET: Create/Edit                          │
└─────────────────────────────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 1. Controller.Create() یا Edit(id)       │
        │    - CreateNewModel()                     │
        │    - GetModelByIdAsync(id)                │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 2. PrepareModelForCreate/Edit             │
        │    - برای Create: فیلدها خالی            │
        │    - برای Edit: تبدیل میلادی به شمسی    │
        │      model.ConvertGregorianDatesToPersian()│
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 3. Return View(model)                     │
        │    - DatePicker فیلدهای شمسی را نمایش   │
        │      می‌دهد                               │
        └───────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    💾 POST: Create/Edit                         │
└─────────────────────────────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 1. کاربر فرم را پر کرده و Submit         │
        │    - تاریخ‌ها به صورت شمسی               │
        │      (مثال: "1404/06/23")                │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 2. Controller.Create/Edit(model) [POST]   │
        │    - دریافت Model با تاریخ‌های شمسی      │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 3. PrepareModelForPost(model)             │
        │    - تبدیل تاریخ‌های شمسی به میلادی      │
        │      model.ConvertPersianDatesToGregorian()│
        │    - "1404/06/23" → DateTime(2025,9,14)  │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 4. ModelState.IsValid Check               │
        │    - Data Annotations Validation          │
        │    - [Required], [Range], etc.            │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 5. ValidateModelWithPersianDates(model)   │
        │    - اعتبارسنجی خاص تاریخ‌های شمسی       │
        │    - بررسی تاریخ شروع < تاریخ پایان      │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 6. SaveModelAsync / UpdateModelAsync      │
        │    - فراخوانی Service Layer              │
        │    - ذخیره در Database (تاریخ میلادی)   │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 7. Redirect به Index                      │
        │    - نمایش پیام موفقیت                   │
        └───────────────────────────────────────────┘
```

---

## 💡 **نحوه استفاده در پروژه**

### **قدم 1: ViewModel را از Base ارث ببرید:**

```csharp
using ClinicApp.ViewModels.Base;
using ClinicApp.Extensions;
using ClinicApp.Filters;

public class PatientInsuranceCreateEditViewModel : PersianDateViewModelWithValidation
{
    // 🔹 فیلدهای شمسی (نمایش در UI)
    [Required(ErrorMessage = "تاریخ شروع بیمه الزامی است")]
    [PersianDate(IsRequired = true, MinYear = 700, MaxYear = 1500,
        InvalidFormatMessage = "فرمت تاریخ نامعتبر است. (مثال: 1404/06/23)")]
    [Display(Name = "تاریخ شروع")]
    public string StartDateShamsi { get; set; }

    [PersianDate(IsRequired = false, MinYear = 700, MaxYear = 1500)]
    [Display(Name = "تاریخ پایان")]
    public string EndDateShamsi { get; set; }

    // 🔹 فیلدهای میلادی (ذخیره در Database)
    [HiddenInput(DisplayValue = false)]
    [NotMapped]
    public DateTime StartDate { get; set; }

    [HiddenInput(DisplayValue = false)]
    [NotMapped]
    public DateTime? EndDate { get; set; }

    // 🔹 Override متدهای تبدیل
    public override void ConvertPersianDatesToGregorian()
    {
        // شمسی → میلادی (برای POST)
        if (!string.IsNullOrEmpty(StartDateShamsi))
        {
            StartDate = StartDateShamsi.ToDateTime();
        }

        if (!string.IsNullOrEmpty(EndDateShamsi))
        {
            EndDate = EndDateShamsi.ToDateTime();
        }
    }

    public override void ConvertGregorianDatesToPersian()
    {
        // میلادی → شمسی (برای Edit GET)
        if (StartDate != DateTime.MinValue)
        {
            StartDateShamsi = StartDate.ToPersianDate();
        }

        if (EndDate.HasValue)
        {
            EndDateShamsi = EndDate.Value.ToPersianDate();
        }
    }
}
```

---

### **قدم 2: Controller را از Base ارث ببرید:**

```csharp
using ClinicApp.Controllers.Base;

public class PatientInsuranceController : PersianDateCrudController<PatientInsuranceCreateEditViewModel, PatientInsurance>
{
    private readonly IPatientInsuranceService _service;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _log;

    public PatientInsuranceController(
        IPatientInsuranceService service,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _service = service;
        _currentUserService = currentUserService;
        _log = logger.ForContext<PatientInsuranceController>();
    }

    // 🔹 1. ایجاد Model خالی
    protected override PatientInsuranceCreateEditViewModel CreateNewModel()
    {
        return new PatientInsuranceCreateEditViewModel
        {
            PatientId = 0,
            IsPrimary = false,
            IsActive = true
        };
    }

    // 🔹 2. دریافت Model برای Edit
    protected override async Task<PatientInsuranceCreateEditViewModel> GetModelByIdAsync(int id)
    {
        _log.Information("دریافت بیمه برای ویرایش. Id: {Id}", id);
        
        var result = await _service.GetPatientInsuranceForEditAsync(id);
        if (!result.Success)
        {
            _log.Warning("بیمه یافت نشد. Id: {Id}", id);
            return null;
        }
        
        return result.Data;
    }

    // 🔹 3. ذخیره Model جدید
    protected override async Task<bool> SaveModelAsync(PatientInsuranceCreateEditViewModel model)
    {
        _log.Information("ایجاد بیمه جدید. PatientId: {PatientId}", model.PatientId);
        
        var result = await _service.CreatePatientInsuranceAsync(model);
        
        if (result.Success)
        {
            TempData["SuccessMessage"] = "بیمه با موفقیت ایجاد شد.";
            _log.Information("بیمه ایجاد شد. Id: {Id}", result.Data);
            return true;
        }
        
        _log.Warning("خطا در ایجاد بیمه. Message: {Message}", result.Message);
        TempData["ErrorMessage"] = result.Message;
        return false;
    }

    // 🔹 4. به‌روزرسانی Model
    protected override async Task<bool> UpdateModelAsync(PatientInsuranceCreateEditViewModel model)
    {
        _log.Information("به‌روزرسانی بیمه. Id: {Id}", model.PatientInsuranceId);
        
        var result = await _service.UpdatePatientInsuranceAsync(model);
        
        if (result.Success)
        {
            TempData["SuccessMessage"] = "بیمه با موفقیت به‌روزرسانی شد.";
            _log.Information("بیمه به‌روزرسانی شد. Id: {Id}", model.PatientInsuranceId);
            return true;
        }
        
        _log.Warning("خطا در به‌روزرسانی بیمه. Message: {Message}", result.Message);
        TempData["ErrorMessage"] = result.Message;
        return false;
    }

    // ✨ تمام! Create/Edit (GET/POST) خودکار کار می‌کنند
    
    // ⚡ اگر نیاز به سفارشی‌سازی داشته باشید، Override کنید:
    public override async Task<ActionResult> Create()
    {
        // اضافه کردن منطق خاص قبل از نمایش فرم
        ViewBag.Patients = await GetPatientsForDropdown();
        ViewBag.InsurancePlans = await GetInsurancePlansForDropdown();
        
        return await base.Create();
    }
}
```

---

## 🎨 **مثال‌های کاربردی از پروژه**

### **1. ماژول Insurance (با تاریخ شروع/پایان):**

```csharp
// ViewModel
public class InsurancePlanCreateEditViewModel : PersianDateViewModelWithValidation
{
    public string ValidFromShamsi { get; set; }
    public string ValidToShamsi { get; set; }
    
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    public override void ConvertPersianDatesToGregorian()
    {
        if (!string.IsNullOrEmpty(ValidFromShamsi))
            ValidFrom = ValidFromShamsi.ToDateTime();
        
        if (!string.IsNullOrEmpty(ValidToShamsi))
            ValidTo = ValidToShamsi.ToDateTime();
    }
    
    public override void ConvertGregorianDatesToPersian()
    {
        if (ValidFrom != DateTime.MinValue)
            ValidFromShamsi = ValidFrom.ToPersianDate();
        
        if (ValidTo.HasValue)
            ValidToShamsi = ValidTo.Value.ToPersianDate();
    }
}

// Controller
public class InsurancePlanController : PersianDateCrudController<InsurancePlanCreateEditViewModel, InsurancePlan>
{
    // پیاده‌سازی 4 متد Abstract...
}
```

---

### **2. ماژول Appointment (با تاریخ قرار):**

```csharp
// ViewModel
public class AppointmentCreateEditViewModel : PersianDateViewModelWithValidation
{
    public string AppointmentDateShamsi { get; set; }
    public DateTime AppointmentDate { get; set; }
    
    public override void ConvertPersianDatesToGregorian()
    {
        if (!string.IsNullOrEmpty(AppointmentDateShamsi))
            AppointmentDate = AppointmentDateShamsi.ToDateTime();
    }
    
    public override void ConvertGregorianDatesToPersian()
    {
        if (AppointmentDate != DateTime.MinValue)
            AppointmentDateShamsi = AppointmentDate.ToPersianDate();
    }
}

// Controller
public class AppointmentController : PersianDateCrudController<AppointmentCreateEditViewModel, Appointment>
{
    // پیاده‌سازی 4 متد Abstract...
}
```

---

### **3. ماژول Reception (با تاریخ پذیرش):**

```csharp
// ViewModel
public class ReceptionCreateEditViewModel : PersianDateViewModelWithValidation
{
    public string ReceptionDateShamsi { get; set; }
    public DateTime ReceptionDate { get; set; }
    
    public override void ConvertPersianDatesToGregorian()
    {
        if (!string.IsNullOrEmpty(ReceptionDateShamsi))
            ReceptionDate = ReceptionDateShamsi.ToDateTime();
    }
    
    public override void ConvertGregorianDatesToPersian()
    {
        if (ReceptionDate != DateTime.MinValue)
            ReceptionDateShamsi = ReceptionDate.ToPersianDate();
    }
}

// Controller
public class ReceptionController : PersianDateCrudController<ReceptionCreateEditViewModel, Reception>
{
    // پیاده‌سازی 4 متد Abstract...
}
```

---

### **4. گزارش‌ها (با بازه زمانی):**

```csharp
// ViewModel
public class ReportFilterViewModel : PersianDateViewModelWithValidation
{
    public string FromDateShamsi { get; set; }
    public string ToDateShamsi { get; set; }
    
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public override void ConvertPersianDatesToGregorian()
    {
        if (!string.IsNullOrEmpty(FromDateShamsi))
            FromDate = FromDateShamsi.ToDateTime();
        
        if (!string.IsNullOrEmpty(ToDateShamsi))
            ToDate = ToDateShamsi.ToDateTime();
    }
}

// Controller (استفاده از نوع ساده)
public class ReportController : PersianDateController
{
    public async Task<ActionResult> Generate(ReportFilterViewModel model)
    {
        // آماده‌سازی Model
        model = PrepareModelForPost(model);
        
        if (ModelState.IsValid && ValidateModelWithPersianDates(model))
        {
            // تولید گزارش با تاریخ‌های میلادی
            var report = await _service.GenerateReportAsync(model.FromDate, model.ToDate);
            return View("ReportResult", report);
        }
        
        return View(model);
    }
}
```

---

## ✅ **Best Practices**

### **1. همیشه از Base Classes استفاده کنید:**

```csharp
// ✅ درست
public class MyController : PersianDateCrudController<MyViewModel, MyEntity>

// ❌ غلط (کد تکراری)
public class MyController : Controller
{
    // تبدیل دستی تاریخ‌ها...
}
```

---

### **2. Override فقط زمانی که نیاز دارید:**

```csharp
// ✅ درست: Override فقط برای سفارشی‌سازی
public override async Task<ActionResult> Create()
{
    // منطق خاص قبل از نمایش فرم
    ViewBag.CustomData = await GetCustomData();
    
    // فراخوانی پیاده‌سازی پایه
    return await base.Create();
}

// ❌ غلط: Override بدون دلیل
public override async Task<ActionResult> Create()
{
    return await base.Create(); // هیچ ارزش افزوده‌ای ندارد
}
```

---

### **3. Logging را فراموش نکنید:**

```csharp
protected override async Task<bool> SaveModelAsync(MyViewModel model)
{
    _log.Information("شروع ذخیره. Title: {Title}", model.Title);
    
    var result = await _service.CreateAsync(model);
    
    if (result.Success)
    {
        _log.Information("ذخیره موفق. Id: {Id}", result.Data);
    }
    else
    {
        _log.Warning("خطا در ذخیره. Message: {Message}", result.Message);
    }
    
    return result.Success;
}
```

---

### **4. TempData را برای پیام‌ها استفاده کنید:**

```csharp
protected override async Task<bool> UpdateModelAsync(MyViewModel model)
{
    var result = await _service.UpdateAsync(model);
    
    if (result.Success)
    {
        TempData["SuccessMessage"] = "با موفقیت به‌روزرسانی شد.";
    }
    else
    {
        TempData["ErrorMessage"] = result.Message;
    }
    
    return result.Success;
}
```

---

### **5. Validation را در دو لایه اعمال کنید:**

```csharp
// 🔹 لایه 1: ViewModel Validation
[Required(ErrorMessage = "تاریخ شروع الزامی است")]
[PersianDate(IsRequired = true)]
public string StartDateShamsi { get; set; }

// 🔹 لایه 2: Controller Validation
protected override async Task<bool> SaveModelAsync(MyViewModel model)
{
    // بررسی منطق کسب‌وکار
    if (model.EndDate < model.StartDate)
    {
        AddDateComparisonError("EndDateShamsi", "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد");
        return false;
    }
    
    var result = await _service.CreateAsync(model);
    return result.Success;
}
```

---

## ⚠️ **مشکلات رایج و راه‌حل**

### **مشکل 1: "The field ValidFrom must be a date."**

**علت:**
```csharp
// ❌ تاریخ شمسی به جای میلادی در فیلد DateTime
model.StartDate = "1404/06/23"; // نوع string است!
```

**راه‌حل:**
```csharp
// ✅ استفاده از PrepareModelForPost
model = PrepareModelForPost(model);
// حالا model.StartDate یک DateTime معتبر است
```

---

### **مشکل 2: فیلدهای تاریخ در Edit خالی هستند**

**علت:**
```csharp
// ❌ فراموش کردن تبدیل میلادی به شمسی
public async Task<ActionResult> Edit(int id)
{
    var model = await GetModelByIdAsync(id);
    return View(model); // StartDateShamsi خالی است!
}
```

**راه‌حل:**
```csharp
// ✅ استفاده از PrepareModelForEdit
public override async Task<ActionResult> Edit(int id)
{
    var model = await GetModelByIdAsync(id);
    model = PrepareModelForEdit(model); // تبدیل میلادی به شمسی
    return View(model);
}
```

---

### **مشکل 3: Override متدهای تبدیل را فراموش کردید**

**علت:**
```csharp
// ❌ فراموش کردن Override
public class MyViewModel : PersianDateViewModel
{
    // متدهای تبدیل Override نشده‌اند!
}
```

**راه‌حل:**
```csharp
// ✅ Override کامل
public class MyViewModel : PersianDateViewModel
{
    public override void ConvertPersianDatesToGregorian()
    {
        if (!string.IsNullOrEmpty(StartDateShamsi))
            StartDate = StartDateShamsi.ToDateTime();
    }
    
    public override void ConvertGregorianDatesToPersian()
    {
        if (StartDate != DateTime.MinValue)
            StartDateShamsi = StartDate.ToPersianDate();
    }
}
```

---

### **مشکل 4: تاریخ پایان قبل از تاریخ شروع**

**راه‌حل:**
```csharp
// ✅ اعتبارسنجی در Controller
protected override async Task<bool> SaveModelAsync(MyViewModel model)
{
    // بررسی منطق تاریخ
    if (model.EndDate.HasValue && model.EndDate < model.StartDate)
    {
        AddDateComparisonError("EndDateShamsi", 
            "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد");
        return false;
    }
    
    var result = await _service.CreateAsync(model);
    return result.Success;
}
```

---

## 🚀 **پیشنهادات بهینه‌سازی**

### **1. اضافه کردن Support برای ServiceResult:**

```csharp
// 🔹 پیشنهاد: تغییر Signature متدها
protected abstract Task<ServiceResult> SaveModelAsync(TViewModel model);
protected abstract Task<ServiceResult> UpdateModelAsync(TViewModel model);

// بجای:
protected abstract Task<bool> SaveModelAsync(TViewModel model);

// مزایا:
// - دریافت پیام خطای دقیق
// - ValidationErrors
// - ErrorCode
```

---

### **2. اضافه کردن Hook Methods:**

```csharp
public abstract class PersianDateCrudController<TViewModel, TEntity> : PersianDateController
{
    // Hook قبل از Create
    protected virtual Task OnBeforeCreateAsync(TViewModel model)
    {
        return Task.CompletedTask;
    }
    
    // Hook بعد از Create موفق
    protected virtual Task OnAfterCreateSuccessAsync(TViewModel model)
    {
        return Task.CompletedTask;
    }
    
    // Hook بعد از Create ناموفق
    protected virtual Task OnAfterCreateFailureAsync(TViewModel model)
    {
        return Task.CompletedTask;
    }
    
    public virtual async Task<ActionResult> Create(TViewModel model)
    {
        model = PrepareModelForPost(model);
        
        // Hook قبل از ذخیره
        await OnBeforeCreateAsync(model);
        
        if (ModelState.IsValid && ValidateModelWithPersianDates(model))
        {
            var result = await SaveModelAsync(model);
            if (result)
            {
                // Hook بعد از موفقیت
                await OnAfterCreateSuccessAsync(model);
                return RedirectToAction("Index");
            }
            else
            {
                // Hook بعد از شکست
                await OnAfterCreateFailureAsync(model);
            }
        }
        
        return View(model);
    }
}
```

**استفاده:**
```csharp
public class MyController : PersianDateCrudController<MyViewModel, MyEntity>
{
    protected override async Task OnBeforeCreateAsync(MyViewModel model)
    {
        // لاگ یا منطق خاص قبل از ذخیره
        _log.Information("قبل از ایجاد: {Title}", model.Title);
    }
    
    protected override async Task OnAfterCreateSuccessAsync(MyViewModel model)
{
        // ارسال ایمیل یا نوتیفیکیشن
        await _notificationService.SendNotificationAsync("رکورد جدید ایجاد شد");
    }
}
```

---

### **3. اضافه کردن Validation Helper:**

```csharp
public abstract class PersianDateController : Controller
{
    /// <summary>
    /// بررسی اینکه تاریخ پایان بعد از تاریخ شروع است
    /// </summary>
    protected virtual bool ValidateDateRange(DateTime startDate, DateTime? endDate, 
        string startFieldName = "StartDateShamsi", 
        string endFieldName = "EndDateShamsi")
    {
        if (endDate.HasValue && endDate < startDate)
        {
            AddDateComparisonError(endFieldName, 
                "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد");
            return false;
        }
        return true;
    }
    
    /// <summary>
    /// بررسی اینکه تاریخ در آینده است
    /// </summary>
    protected virtual bool ValidateFutureDate(DateTime date, string fieldName)
    {
        if (date < DateTime.Now)
        {
            AddDateValidationError(fieldName, 
                "تاریخ باید در آینده باشد");
            return false;
        }
        return true;
    }
}
```

---

### **4. اضافه کردن Support برای Async Create:**

```csharp
// بجای:
public virtual async Task<ActionResult> Create()
{
    var model = CreateNewModel();
    model = PrepareModelForCreate(model);
    return View(model);
}

// پیشنهاد:
public virtual async Task<ActionResult> Create()
{
    var model = await CreateNewModelAsync(); // Async
    model = PrepareModelForCreate(model);
    return View(model);
}

protected virtual Task<TViewModel> CreateNewModelAsync()
{
    return Task.FromResult(CreateNewModel());
}
```

---

## 📊 **آمار استفاده در پروژه**

### **ماژول‌های که باید از PersianDateController استفاده کنند:**

| ماژول | ViewModel | تاریخ‌ها | اولویت |
|-------|-----------|----------|--------|
| **PatientInsurance** | PatientInsuranceCreateEditViewModel | StartDate, EndDate | 🔥 بالا |
| **InsurancePlan** | InsurancePlanViewModel | ValidFrom, ValidTo | 🔥 بالا |
| **Appointment** | AppointmentCreateEditViewModel | AppointmentDate | 🔥 بالا |
| **Reception** | ReceptionCreateEditViewModel | ReceptionDate | 🔥 بالا |
| **DoctorSchedule** | DoctorScheduleViewModel | ScheduleDate | ⚡ متوسط |
| **CashSession** | CashSessionViewModel | SessionDate | ⚡ متوسط |
| **Reports** | ReportFilterViewModel | FromDate, ToDate | ⚡ متوسط |
| **FactorSetting** | FactorSettingCreateEditViewModel | EffectiveFrom, EffectiveTo | ⚡ متوسط |

---

## 🎯 **نتیجه‌گیری**

### **مزایای استفاده از PersianDateController:**

✅ **کاهش 80% کد تکراری** در Controller های دارای تاریخ  
✅ **رعایت اصول SOLID** (SRP, OCP, LSP, DIP)  
✅ **یکپارچگی** در مدیریت تاریخ‌های شمسی  
✅ **Testability** بالا (Mock کردن متدهای Abstract)  
✅ **Maintainability** آسان (تغییرات در یک نقطه)  
✅ **Extensibility** عالی (Override برای سفارشی‌سازی)  

### **زمان استفاده:**

🟢 **حتماً استفاده کنید** اگر:
- ViewModel دارای تاریخ شمسی است
- نیاز به CRUD استاندارد دارید
- می‌خواهید کد تکراری کاهش یابد

🟡 **اختیاری** اگر:
- فقط یک Action دارای تاریخ است
- منطق بسیار خاص دارید
- API Controller هستید (بدون View)

🔴 **استفاده نکنید** اگر:
- هیچ تاریخی ندارید
- Controller به صورت کامل خاص است

---

**✨ با استفاده از PersianDateController، کد شما تمیزتر، قابل نگهداری‌تر و حرفه‌ای‌تر می‌شود! ✨**


