# 🌱 **تحلیل کامل SystemSeedController و وابستگی‌های آن**

> **پروژه**: کلینیک شفا  
> **موضوع**: مدیریت و ایجاد داده‌های اولیه سیستم (Seed Data Management)  
> **تاریخ**: 1404/07/11  
> **اهمیت**: 🔥 بالا (برای راه‌اندازی اولیه سیستم و محیط‌های تست)

---

## 📑 **فهرست**

1. [نقش و مسئولیت](#نقش-و-مسئولیت)
2. [معماری و طراحی](#معماری-و-طراحی)
3. [وابستگی‌ها](#وابستگیها)
4. [SystemSeedController](#systemseedcontroller)
5. [SystemSeedService](#systemseedservice)
6. [FactorSettingSeedService](#factorsettingseedservice)
7. [ServiceSeedService](#serviceseedservice)
8. [ServiceTemplateSeedService](#servicetemplateseedservice)
9. [SystemUsers Helper](#systemusers-helper)
10. [View (Index.cshtml)](#view-indexcshtml)
11. [فرآیند Seeding](#فرآیند-seeding)
12. [Registration در Unity DI](#registration-در-unity-di)
13. [Use Cases](#use-cases)
14. [Best Practices](#best-practices)
15. [نکات امنیتی](#نکات-امنیتی)
16. [خلاصه](#خلاصه)

---

## 🎯 **نقش و مسئولیت**

### **هدف اصلی:**

`SystemSeedController` یک **Controller اداری** است که برای **ایجاد و مدیریت داده‌های اولیه سیستم** طراحی شده است.

### **مسئولیت‌ها:**

```
1. ایجاد کای‌های پایه (FactorSettings) برای محاسبات مالی
2. ایجاد قالب‌های خدمات (ServiceTemplates) مطابق مصوبه 1404
3. ایجاد خدمات نمونه (Sample Services) با اجزای فنی/حرفه‌ای
4. ایجاد اجزای خدمات (ServiceComponents)
5. ایجاد خدمات مشترک (SharedServices)
6. بررسی وضعیت داده‌های اولیه
7. پاک کردن داده‌های اولیه (برای محیط تست)
8. تست محاسبات و خدمات مشترک
```

### **چرا نیاز داریم؟**

```csharp
// ❌ مشکل: بدون Seed Data
// سیستم خالی است و نمی‌تواند محاسبات انجام دهد
var service = new Service { Title = "ویزیت" };
// قیمت چقدر؟ کای فنی چقدر؟ کای حرفه‌ای چقدر؟ ❌

// ✅ راه‌حل: با Seed Data
SystemSeedController.SeedAllData()
// حالا سیستم دارای:
// - کای‌های مصوبه 1404 (65000، 41000، ...)
// - قالب‌های خدمات (ویزیت عمومی، متخصص، ...)
// - خدمات نمونه با اجزای کامل
// - آماده برای محاسبات ✅
```

---

## 🏗️ **معماری و طراحی**

### **الگوهای طراحی:**

#### ✅ **1. Service Layer Pattern:**
```
Controller → Service → Repository → Database
```

#### ✅ **2. Separation of Concerns:**
```
- SystemSeedController: مدیریت UI و Requests
- SystemSeedService: هماهنگی کلی Seeding
- Specialized Services: Seeding های تخصصی (Factor, Service, Template)
- Helper Classes: کمک‌کننده‌های مشترک (SystemUsers)
```

#### ✅ **3. Dependency Injection:**
```
تمام وابستگی‌ها از طریق Constructor Injection تزریق می‌شوند
```

#### ✅ **4. Transaction Pattern:**
```
هر عملیات Seeding در یک Transaction انجام می‌شود
```

---

## 🔗 **وابستگی‌ها**

### **نمودار وابستگی:**

```
SystemSeedController
    ├── SystemSeedService (اصلی)
    ├── ILogger (لاگ‌گیری)
    ├── ICurrentUserService (کاربر فعلی)
    ├── IMessageNotificationService (پیام‌ها)
    └── ApplicationDbContext (دیتابیس)

SystemSeedService
    ├── ApplicationDbContext
    ├── ILogger
    ├── ICurrentUserService
    ├── FactorSettingSeedService
    ├── ServiceSeedService
    └── ServiceTemplateSeedService

FactorSettingSeedService
    ├── ApplicationDbContext
    ├── ILogger
    └── ICurrentUserService

ServiceSeedService
    ├── ApplicationDbContext
    ├── ILogger
    └── ICurrentUserService

ServiceTemplateSeedService
    ├── ApplicationDbContext
    ├── ILogger
    └── ICurrentUserService
```

### **لیست کامل وابستگی‌ها:**

| وابستگی | نقش | لایه |
|---------|-----|------|
| `SystemSeedService` | هماهنگی کلی Seeding | Service |
| `FactorSettingSeedService` | Seeding کای‌ها | Service |
| `ServiceSeedService` | Seeding خدمات | Service |
| `ServiceTemplateSeedService` | Seeding قالب‌ها | Service |
| `ILogger` | لاگ‌گیری عملیات | Infrastructure |
| `ICurrentUserService` | دریافت کاربر فعلی | Service |
| `IMessageNotificationService` | پیام‌ها و نوتیفیکیشن | Service |
| `ApplicationDbContext` | دسترسی به دیتابیس | Data |
| `SystemUsers` (Helper) | مدیریت کاربران سیستمی | Helper |

---

## 📦 **1. SystemSeedController**

### **مسیر:**
```
Areas/Admin/Controllers/SystemSeedController.cs
```

### **Constructor:**

```csharp
public class SystemSeedController : BaseController
{
    private readonly SystemSeedService _systemSeedService;
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _context;

    public SystemSeedController(
        SystemSeedService systemSeedService,
        ILogger logger,
        ICurrentUserService currentUserService,
        IMessageNotificationService messageNotificationService,
        ApplicationDbContext context)
        : base(messageNotificationService)
    {
        _systemSeedService = systemSeedService;
        _logger = logger;
        _currentUserService = currentUserService;
        _context = context;
    }
}
```

### **Actions:**

#### **1. Index() - GET:**
```csharp
/// <summary>
/// صفحه اصلی مدیریت داده‌های اولیه
/// نمایش وضعیت کلی سیستم
/// </summary>
public async Task<ActionResult> Index()
{
    try
    {
        var status = await _systemSeedService.GetSeedDataStatusAsync();
        return View(status);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در دریافت وضعیت داده‌های اولیه");
        TempData["ErrorMessage"] = "خطا در دریافت وضعیت داده‌های اولیه";
        return View(new SeedDataStatus());
    }
}
```

**خروجی:**
- `SeedDataStatus` شامل:
  - `FactorsExist`: آیا کای‌ها موجود است؟
  - `ServicesExist`: آیا خدمات موجود است؟
  - `IsComplete`: آیا همه چیز کامل است؟
  - `FactorSettingsCount`: تعداد کای‌ها
  - `ServicesCount`: تعداد خدمات
  - `SharedServicesCount`: تعداد خدمات مشترک
  - `ServiceComponentsCount`: تعداد اجزای خدمات

---

#### **2. SeedAllData() - POST:**
```csharp
/// <summary>
/// ایجاد تمام داده‌های اولیه به صورت یک‌جا
/// </summary>
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> SeedAllData()
{
    try
    {
        _logger.Information("شروع ایجاد داده‌های اولیه توسط کاربر");
        await _systemSeedService.SeedAllDataAsync();
        
        TempData["SuccessMessage"] = "داده‌های اولیه با موفقیت ایجاد شدند";
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ایجاد داده‌های اولیه");
        TempData["ErrorMessage"] = "خطا در ایجاد داده‌های اولیه: " + ex.Message;
        return RedirectToAction("Index");
    }
}
```

**عملیات:**
```
1. SystemUsers.Initialize() - مقداردهی کاربران سیستمی
2. SeedFactorSettings() - ایجاد کای‌های پایه
3. SeedPreviousYearFactors() - ایجاد کای‌های سال قبل
4. SeedServiceTemplates() - ایجاد قالب‌های خدمات
5. SeedSampleServices() - ایجاد خدمات نمونه
6. SeedServiceComponents() - ایجاد اجزای خدمات
7. SeedSharedServices() - ایجاد خدمات مشترک
8. Validate() - اعتبارسنجی نهایی
```

---

#### **3. SeedDataStepByStep() - POST:**
```csharp
/// <summary>
/// ایجاد داده‌های اولیه به صورت مرحله‌ای
/// مفید برای نمایش پیشرفت و دیباگ
/// </summary>
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> SeedDataStepByStep()
{
    try
    {
        _logger.Information("شروع ایجاد داده‌های اولیه به صورت مرحله‌ای");
        await _systemSeedService.SeedDataStepByStepAsync();
        
        TempData["SuccessMessage"] = "داده‌های اولیه به صورت مرحله‌ای با موفقیت ایجاد شدند";
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ایجاد داده‌های اولیه به صورت مرحله‌ای");
        TempData["ErrorMessage"] = "خطا در ایجاد داده‌های اولیه: " + ex.Message;
        return RedirectToAction("Index");
    }
}
```

**تفاوت با SeedAllData:**
- هر مرحله جداگانه اجرا می‌شود
- تاخیر 1 ثانیه بین مراحل
- لاگ‌گیری دقیق‌تر
- نمایش پیشرفت برای کاربر

---

#### **4. GetStatus() - GET (JSON):**
```csharp
/// <summary>
/// بررسی وضعیت داده‌های اولیه (AJAX)
/// </summary>
[HttpGet]
public async Task<JsonResult> GetStatus()
{
    try
    {
        var status = await _systemSeedService.GetSeedDataStatusAsync();
        return Json(status, JsonRequestBehavior.AllowGet);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در دریافت وضعیت داده‌های اولیه");
        return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
    }
}
```

---

#### **5. ClearSeedData() - POST:**
```csharp
/// <summary>
/// پاک کردن داده‌های اولیه (فقط برای تست)
/// ⚠️ عملیات خطرناک - فقط در محیط تست
/// </summary>
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> ClearSeedData()
{
    try
    {
        _logger.Warning("پاک کردن داده‌های اولیه توسط کاربر");
        await _systemSeedService.ClearSeedDataAsync();
        
        TempData["SuccessMessage"] = "داده‌های اولیه با موفقیت پاک شدند";
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در پاک کردن داده‌های اولیه");
        TempData["ErrorMessage"] = "خطا در پاک کردن داده‌های اولیه: " + ex.Message;
        return RedirectToAction("Index");
    }
}
```

**عملیات:**
```
1. حذف SharedServices ایجاد شده توسط "system-seed"
2. حذف ServiceComponents ایجاد شده توسط "system-seed"
3. حذف Services ایجاد شده توسط "system-seed"
4. حذف FactorSettings ایجاد شده توسط "system-seed" یا کاربر فعلی
```

---

#### **6. TestCalculations() - GET:**
```csharp
/// <summary>
/// تست محاسبات با داده‌های اولیه
/// TODO: پیاده‌سازی تست‌های محاسباتی
/// </summary>
[HttpGet]
public async Task<ActionResult> TestCalculations()
{
    try
    {
        _logger.Information("شروع تست محاسبات با داده‌های اولیه");
        
        // بررسی وجود داده‌های اولیه
        var status = await _systemSeedService.GetSeedDataStatusAsync();
        if (!status.IsComplete)
        {
            TempData["WarningMessage"] = "ابتدا داده‌های اولیه را ایجاد کنید";
            return RedirectToAction("Index");
        }

        // TODO: پیاده‌سازی تست محاسبات با ServiceCalculationService
        
        TempData["SuccessMessage"] = "تست محاسبات با موفقیت انجام شد";
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در تست محاسبات");
        TempData["ErrorMessage"] = "خطا در تست محاسبات: " + ex.Message;
        return RedirectToAction("Index");
    }
}
```

---

#### **7. TestSharedServices() - GET (JSON):**
```csharp
/// <summary>
/// تست ایجاد خدمات مشترک
/// بررسی وجود خدمات و دپارتمان‌ها
/// </summary>
[HttpGet]
public async Task<JsonResult> TestSharedServices()
{
    try
    {
        _logger.Information("شروع تست ایجاد خدمات مشترک");

        var services = await _context.Services
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToListAsync();

        var departments = await _context.Departments
            .Where(d => !d.IsDeleted && d.IsActive)
            .ToListAsync();

        var result = new
        {
            success = true,
            servicesCount = services.Count,
            departmentsCount = departments.Count,
            services = services.Select(s => new { s.ServiceId, s.Title, s.ServiceCode }).ToList(),
            departments = departments.Select(d => new { d.DepartmentId, d.Name }).ToList()
        };

        return Json(result, JsonRequestBehavior.AllowGet);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در تست خدمات مشترک");
        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
    }
}
```

---

#### **8. GetSystemReport() - GET (JSON):**
```csharp
/// <summary>
/// دریافت گزارش وضعیت سیستم
/// </summary>
[HttpGet]
public async Task<JsonResult> GetSystemReport()
{
    try
    {
        var status = await _systemSeedService.GetSeedDataStatusAsync();
        var report = new
        {
            status.IsComplete,
            status.FactorsExist,
            status.ServicesExist,
            Counts = new
            {
                FactorSettings = status.FactorSettingsCount,
                Services = status.ServicesCount,
                SharedServices = status.SharedServicesCount,
                ServiceComponents = status.ServiceComponentsCount
            },
            Timestamp = DateTime.Now,
            UserId = _currentUserService.GetCurrentUserId()
        };

        return Json(report, JsonRequestBehavior.AllowGet);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در دریافت گزارش سیستم");
        return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
    }
}
```

---

## 📦 **2. SystemSeedService**

### **مسیر:**
```
Services/DataSeeding/SystemSeedService.cs
```

### **نقش:**
```
هماهنگی و مدیریت کلی فرآیند Seeding
```

### **متدهای کلیدی:**

#### **1. SeedAllDataAsync():**

```csharp
/// <summary>
/// ایجاد تمام داده‌های اولیه سیستم
/// </summary>
public async Task SeedAllDataAsync()
{
    try
    {
        _logger.Information("شروع ایجاد داده‌های اولیه سیستم");

        // 0. مقداردهی اولیه SystemUsers (اولویت اول)
        _logger.Information("مرحله 0: مقداردهی اولیه SystemUsers");
        SystemUsers.Initialize(_context);

        // 1. ایجاد کای‌های پایه
        _logger.Information("مرحله 1: ایجاد کای‌های پایه");
        await _factorSeedService.SeedFactorSettingsAsync();
        await _factorSeedService.SeedPreviousYearFactorsAsync();

        // 2. ایجاد قالب‌های خدمات (بهترین روش)
        _logger.Information("مرحله 2: ایجاد قالب‌های خدمات");
        await _serviceTemplateSeedService.SeedServiceTemplatesAsync();

        // 3. ایجاد خدمات نمونه
        _logger.Information("مرحله 3: ایجاد خدمات نمونه");
        await _serviceSeedService.SeedSampleServicesAsync();

        // 4. ایجاد اجزای خدمات
        _logger.Information("مرحله 4: ایجاد اجزای خدمات");
        await _serviceSeedService.SeedServiceComponentsAsync();

        // 5. ایجاد خدمات مشترک
        _logger.Information("مرحله 5: ایجاد خدمات مشترک");
        await _serviceSeedService.SeedSharedServicesAsync();

        // 6. اعتبارسنجی
        _logger.Information("مرحله 6: اعتبارسنجی داده‌ها");
        var factorsValid = await _factorSeedService.ValidateRequiredFactorsAsync();
        var servicesValid = await _serviceSeedService.ValidateSeededDataAsync();

        if (factorsValid && servicesValid)
        {
            _logger.Information("✅ تمام داده‌های اولیه با موفقیت ایجاد شدند");
        }
        else
        {
            _logger.Warning("⚠️ برخی از داده‌های اولیه به درستی ایجاد نشدند");
        }
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ایجاد داده‌های اولیه سیستم");
        throw;
    }
}
```

---

#### **2. GetSeedDataStatusAsync():**

```csharp
/// <summary>
/// بررسی وضعیت داده‌های اولیه
/// </summary>
public async Task<SeedDataStatus> GetSeedDataStatusAsync()
{
    try
    {
        var status = new SeedDataStatus();

        // بررسی کای‌ها
        status.FactorsExist = await _factorSeedService.ValidateRequiredFactorsAsync();

        // بررسی خدمات
        status.ServicesExist = await _serviceSeedService.ValidateSeededDataAsync();

        // شمارش رکوردها
        status.FactorSettingsCount = await _context.FactorSettings
            .Where(f => !f.IsDeleted)
            .CountAsync();

        status.ServicesCount = await _context.Services
            .Where(s => !s.IsDeleted && s.IsActive)
            .CountAsync();

        status.SharedServicesCount = await _context.SharedServices
            .Where(ss => !ss.IsDeleted && ss.IsActive)
            .CountAsync();

        status.ServiceComponentsCount = await _context.ServiceComponents
            .Where(sc => !sc.IsDeleted && sc.IsActive)
            .CountAsync();

        status.IsComplete = status.FactorsExist && status.ServicesExist;

        return status;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در بررسی وضعیت داده‌های اولیه");
        return new SeedDataStatus { IsComplete = false };
    }
}
```

---

#### **3. ClearSeedDataAsync():**

```csharp
/// <summary>
/// پاک کردن داده‌های اولیه (برای تست)
/// </summary>
public async Task ClearSeedDataAsync()
{
    try
    {
        _logger.Warning("شروع پاک کردن داده‌های اولیه");

        // پاک کردن خدمات مشترک
        var sharedServices = await _context.SharedServices
            .Where(ss => ss.CreatedByUserId == "system-seed")
            .ToListAsync();
        _context.SharedServices.RemoveRange(sharedServices);

        // پاک کردن اجزای خدمات
        var serviceComponents = await _context.ServiceComponents
            .Where(sc => sc.CreatedByUserId == "system-seed")
            .ToListAsync();
        _context.ServiceComponents.RemoveRange(serviceComponents);

        // پاک کردن خدمات
        var services = await _context.Services
            .Where(s => s.CreatedByUserId == "system-seed")
            .ToListAsync();
        _context.Services.RemoveRange(services);

        // پاک کردن کای‌ها
        var factors = await _context.FactorSettings
            .Where(f => f.CreatedByUserId == "system-seed" || f.CreatedByUserId == _currentUserService.UserId)
            .ToListAsync();
        _context.FactorSettings.RemoveRange(factors);

        await _context.SaveChangesAsync();
        _logger.Information("داده‌های اولیه با موفقیت پاک شدند");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در پاک کردن داده‌های اولیه");
        throw;
    }
}
```

---

## 📦 **3. FactorSettingSeedService**

### **مسیر:**
```
Services/DataSeeding/FactorSettingSeedService.cs
```

### **نقش:**
```
ایجاد کای‌های فنی و حرفه‌ای مطابق با مصوبه 1404
```

### **متدهای کلیدی:**

#### **1. SeedFactorSettingsAsync():**

```csharp
/// <summary>
/// ایجاد کای‌های پایه برای سال مالی 1404
/// </summary>
public async Task SeedFactorSettingsAsync()
{
    try
    {
        _logger.Information("شروع ایجاد کای‌های پایه برای سال مالی 1404");

        var currentYear = GetCurrentPersianYear(); // 1404
        var existingFactors = await _context.FactorSettings
            .Where(f => f.FinancialYear == currentYear && !f.IsDeleted)
            .ToListAsync();

        if (existingFactors.Any())
        {
            _logger.Information($"کای‌های سال مالی {currentYear} قبلاً ایجاد شده‌اند");
            return;
        }

        var factorSettings = new List<FactorSetting>
        {
            // کای فنی پایه - 4,350,000 ریال (مصوبه 1404)
            new FactorSetting
            {
                FactorType = ServiceComponentType.Technical,
                IsHashtagged = false,
                Value = 4350000m,
                EffectiveFrom = new DateTime(2025, 3, 21),
                EffectiveTo = new DateTime(2026, 3, 20),
                FinancialYear = currentYear,
                IsActiveForCurrentYear = true,
                IsFrozen = false,
                IsActive = true,
                Description = "کای فنی پایه برای کلیه خدمات (مصوبه 1404 - 4,350,000 ریال)"
            },
            
            // کای فنی کد ۷ - 2,750,000 ریال
            new FactorSetting
            {
                FactorType = ServiceComponentType.Technical,
                IsHashtagged = true,
                Value = 2750000m,
                // ...
            },
            
            // کای حرفه‌ای پایه - 1,370,000 ریال
            new FactorSetting
            {
                FactorType = ServiceComponentType.Professional,
                IsHashtagged = false,
                Value = 1370000m,
                // ...
            }
            
            // ... سایر کای‌ها
        };

        _context.FactorSettings.AddRange(factorSettings);
        await _context.SaveChangesAsync();
        
        _logger.Information($"تعداد {factorSettings.Count} کای جدید ایجاد شد");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ایجاد کای‌های پایه");
        throw;
    }
}
```

**کای‌های ایجاد شده:**

| نوع | IsHashtagged | مقدار (ریال) | توضیحات |
|-----|--------------|--------------|---------|
| Technical | false | 4,350,000 | کای فنی پایه |
| Technical | true | 2,750,000 | کای فنی کد ۷ |
| Technical | true | 2,600,000 | کای فنی کدهای ۸ و ۹ |
| Technical | false | 1,900,000 | کای فنی دندانپزشکی |
| Professional | false | 1,370,000 | کای حرفه‌ای پایه |
| Professional | false | 1,370,000 | کای حرفه‌ای ویزیت سرپایی |

---

#### **2. ValidateRequiredFactorsAsync():**

```csharp
/// <summary>
/// اعتبارسنجی وجود کای‌های مورد نیاز
/// </summary>
public async Task<bool> ValidateRequiredFactorsAsync()
{
    try
    {
        var currentYear = GetCurrentPersianYear();
        
        // بررسی وجود کای فنی
        var technicalExists = await _context.FactorSettings
            .AnyAsync(f => f.FactorType == ServiceComponentType.Technical &&
                          f.FinancialYear == currentYear &&
                          !f.IsDeleted &&
                          f.IsActive);

        // بررسی وجود کای حرفه‌ای
        var professionalExists = await _context.FactorSettings
            .AnyAsync(f => f.FactorType == ServiceComponentType.Professional &&
                          f.FinancialYear == currentYear &&
                          !f.IsDeleted &&
                          f.IsActive);

        return technicalExists && professionalExists;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در اعتبارسنجی کای‌ها");
        return false;
    }
}
```

---

## 📦 **4. ServiceSeedService**

### **مسیر:**
```
Services/DataSeeding/ServiceSeedService.cs
```

### **نقش:**
```
ایجاد خدمات نمونه، اجزای خدمات، و خدمات مشترک
```

### **متدهای کلیدی:**

#### **1. SeedSampleServicesAsync():**

```csharp
/// <summary>
/// ایجاد خدمات نمونه با اجزای فنی و حرفه‌ای
/// </summary>
public async Task SeedSampleServicesAsync()
{
    try
    {
        _logger.Information("شروع ایجاد خدمات نمونه");

        var serviceCategories = await _context.ServiceCategories
            .Where(sc => !sc.IsDeleted)
            .ToListAsync();

        if (!serviceCategories.Any())
        {
            _logger.Warning("هیچ دسته‌بندی خدماتی یافت نشد");
            return;
        }

        var sampleServices = new List<Service>
        {
            // ویزیت پزشک عمومی - کد 970000
            new Service
            {
                Title = "ویزیت پزشک عمومی در مراکز سرپایی",
                ServiceCode = "970000",
                Description = "ویزیت پزشک عمومی در مراکز سرپایی - مصوبه 1404",
                IsHashtagged = true,
                Price = 0, // محاسبه خواهد شد
                IsActive = true,
                ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
            },
            
            // ویزیت پزشک متخصص - کد 970015
            new Service
            {
                Title = "ویزیت پزشک متخصص در مراکز سرپایی غیرتمام‌وقت",
                ServiceCode = "970015",
                IsHashtagged = true,
                Price = 0,
                IsActive = true,
                ServiceCategoryId = serviceCategories.FirstOrDefault()?.ServiceCategoryId ?? 1
            }
            
            // ... سایر خدمات
        };

        // بررسی تکراری نبودن
        var existingCodes = await _context.Services
            .Where(s => !s.IsDeleted)
            .Select(s => s.ServiceCode)
            .ToListAsync();

        var newServices = sampleServices
            .Where(s => !existingCodes.Contains(s.ServiceCode))
            .ToList();

        if (newServices.Any())
        {
            _context.Services.AddRange(newServices);
            await _context.SaveChangesAsync();
            _logger.Information($"تعداد {newServices.Count} خدمت جدید ایجاد شد");
        }
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ایجاد خدمات نمونه");
        throw;
    }
}
```

---

#### **2. SeedServiceComponentsAsync():**

```csharp
/// <summary>
/// ایجاد اجزای خدمات (ضرایب فنی و حرفه‌ای)
/// </summary>
public async Task SeedServiceComponentsAsync()
{
    try
    {
        _logger.Information("شروع ایجاد اجزای خدمات");

        var services = await _context.Services
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToListAsync();

        foreach (var service in services)
        {
            // بررسی وجود ServiceComponents
            var hasComponents = await _context.ServiceComponents
                .AnyAsync(sc => sc.ServiceId == service.ServiceId && !sc.IsDeleted);

            if (hasComponents)
            {
                continue; // قبلاً ایجاد شده
            }

            // ایجاد جزء فنی
            var technicalComponent = new ServiceComponent
            {
                ServiceId = service.ServiceId,
                ComponentType = ServiceComponentType.Technical,
                Coefficient = GetTechnicalCoefficient(service.ServiceCode), // مثلاً 0.5
                IsActive = true,
                Description = $"ضریب فنی خدمت {service.Title}"
            };

            // ایجاد جزء حرفه‌ای
            var professionalComponent = new ServiceComponent
            {
                ServiceId = service.ServiceId,
                ComponentType = ServiceComponentType.Professional,
                Coefficient = GetProfessionalCoefficient(service.ServiceCode), // مثلاً 1.3
                IsActive = true,
                Description = $"ضریب حرفه‌ای خدمت {service.Title}"
            };

            _context.ServiceComponents.Add(technicalComponent);
            _context.ServiceComponents.Add(professionalComponent);
        }

        await _context.SaveChangesAsync();
        _logger.Information("اجزای خدمات با موفقیت ایجاد شدند");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ایجاد اجزای خدمات");
        throw;
    }
}
```

---

#### **3. SeedSharedServicesAsync():**

```csharp
/// <summary>
/// ایجاد خدمات مشترک (اگر دپارتمان‌های مختلف وجود دارند)
/// </summary>
public async Task SeedSharedServicesAsync()
{
    try
    {
        _logger.Information("شروع ایجاد خدمات مشترک");

        var services = await _context.Services
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToListAsync();

        var departments = await _context.Departments
            .Where(d => !d.IsDeleted && d.IsActive)
            .ToListAsync();

        if (departments.Count < 2)
        {
            _logger.Information("تعداد دپارتمان‌ها کافی نیست برای ایجاد خدمات مشترک");
            return;
        }

        foreach (var service in services.Take(5)) // فقط 5 خدمت اول
        {
            foreach (var department in departments)
            {
                var exists = await _context.SharedServices
                    .AnyAsync(ss => ss.ServiceId == service.ServiceId &&
                                   ss.DepartmentId == department.DepartmentId &&
                                   !ss.IsDeleted);

                if (!exists)
                {
                    var sharedService = new SharedService
                    {
                        ServiceId = service.ServiceId,
                        DepartmentId = department.DepartmentId,
                        OverridePriceRial = null, // می‌تواند override شود
                        TechnicalCoefficientOverride = null,
                        ProfessionalCoefficientOverride = null,
                        IsActive = true
                    };

                    _context.SharedServices.Add(sharedService);
                }
            }
        }

        await _context.SaveChangesAsync();
        _logger.Information("خدمات مشترک با موفقیت ایجاد شدند");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ایجاد خدمات مشترک");
        throw;
    }
}
```

---

## 📦 **5. ServiceTemplateSeedService**

### **مسیر:**
```
Services/DataSeeding/ServiceTemplateSeedService.cs
```

### **نقش:**
```
ایجاد قالب‌های خدمات (بهترین روش برای مدیریت مقادیر پیش‌فرض)
```

### **متد کلیدی:**

#### **SeedServiceTemplatesAsync():**

```csharp
/// <summary>
/// ایجاد قالب‌های خدمات مطابق با مصوبه 1404
/// </summary>
public async Task SeedServiceTemplatesAsync()
{
    try
    {
        _logger.Information("شروع ایجاد قالب‌های خدمات مطابق با مصوبه 1404");

        var systemUserId = await GetValidUserIdForSeedAsync();
        var currentTime = DateTime.UtcNow;

        var serviceTemplates = new List<ServiceTemplate>
        {
            // ویزیت‌های پزشک عمومی
            new ServiceTemplate
            {
                ServiceCode = "970000",
                ServiceName = "ویزیت پزشک عمومی در مراکز سرپایی",
                DefaultTechnicalCoefficient = 0.5m,
                DefaultProfessionalCoefficient = 1.3m,
                Description = "قالب پیش‌فرض برای ویزیت پزشک عمومی - مصوبه 1404",
                IsActive = true,
                CreatedAt = currentTime,
                CreatedByUserId = systemUserId
            },
            
            // ویزیت‌های پزشک متخصص
            new ServiceTemplate
            {
                ServiceCode = "970015",
                ServiceName = "ویزیت پزشک متخصص در مراکز سرپایی غیرتمام‌وقت",
                DefaultTechnicalCoefficient = 0.7m,
                DefaultProfessionalCoefficient = 1.8m,
                Description = "قالب پیش‌فرض برای ویزیت پزشک متخصص غیرتمام‌وقت - مصوبه 1404",
                IsActive = true,
                CreatedAt = currentTime,
                CreatedByUserId = systemUserId
            }
            
            // ... بیش از 20 قالب دیگر
        };

        // بررسی تکراری نبودن
        var existingCodes = await _context.ServiceTemplates
            .Where(st => !st.IsDeleted)
            .Select(st => st.ServiceCode)
            .ToListAsync();

        var newTemplates = serviceTemplates
            .Where(st => !existingCodes.Contains(st.ServiceCode))
            .ToList();

        if (newTemplates.Any())
        {
            _context.ServiceTemplates.AddRange(newTemplates);
            await _context.SaveChangesAsync();
            _logger.Information("تعداد {Count} قالب خدمت جدید ایجاد شد", newTemplates.Count);
        }
        else
        {
            _logger.Information("همه قالب‌های خدمات قبلاً ایجاد شده‌اند");
        }
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ایجاد قالب‌های خدمات");
        throw;
    }
}
```

**مزایای ServiceTemplate:**
```
✅ مقادیر پیش‌فرض مرکزی برای ضرایب
✅ مطابق با مصوبه 1404
✅ قابل استفاده در Service Creation
✅ کاهش کد تکراری
✅ مدیریت آسان‌تر
```

---

## 📦 **6. SystemUsers Helper**

### **مسیر:**
```
Helpers/SystemUsers.cs
```

### **نقش:**
```
مدیریت شناسه‌های کاربران سیستمی (System و Admin)
```

### **Constants:**

```csharp
public static class SystemUsers
{
    // کد ملی کاربر سیستم
    public const string SystemUserNationalCode = "3031945451";
    
    // کد ملی کاربر ادمین
    public const string AdminUserNationalCode = "3020347998";
    
    // شناسه‌های کش شده
    public static string SystemUserId { get; private set; }
    public static string AdminUserId { get; private set; }
    
    public static bool IsInitialized { get; private set; }
}
```

### **متد اصلی:**

```csharp
/// <summary>
/// مقداردهی اولیه کلاس با کش کردن شناسه‌های کاربران سیستمی
/// </summary>
public static void Initialize(ApplicationDbContext context)
{
    if (IsInitialized)
    {
        Log.Debug("SystemUsers قبلاً مقداردهی شده است");
        return;
    }

    try
    {
        Log.Information("در حال بارگذاری و کش کردن شناسه‌ی کاربران سیستمی...");

        // پاک کردن مقادیر قبلی
        SystemUserId = null;
        AdminUserId = null;
        IsInitialized = false;

        // دریافت کاربران سیستمی
        LoadSystemUsers(context);
        LoadAdminUsers(context);

        // بررسی کامل بودن اطلاعات
        ValidateSystemUsers();

        // علامت‌گذاری به عنوان مقداردهی شده
        IsInitialized = true;

        Log.Information("SystemUsers با موفقیت مقداردهی شد");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "خطا در مقداردهی اولیه SystemUsers");
        throw;
    }
}
```

**اهمیت:**
```
✅ کش کردن شناسه‌های کاربران سیستمی
✅ جلوگیری از کوئری‌های تکراری
✅ مدیریت CreatedByUserId برای Seed Data
✅ پشتیبانی از سیستم پسورد‌لس
```

---

## 📦 **7. View (Index.cshtml)**

### **مسیر:**
```
Areas/Admin/Views/SystemSeed/Index.cshtml
```

### **نقش:**
```
رابط کاربری برای مدیریت داده‌های اولیه
```

### **بخش‌های کلیدی:**

#### **1. وضعیت کلی:**

```cshtml
<div class="alert @(Model.IsComplete ? "alert-success" : "alert-warning")">
    <h4>
        <i class="fas @(Model.IsComplete ? "fa-check-circle" : "fa-exclamation-triangle")"></i>
        وضعیت داده‌های اولیه
    </h4>
    <p>
        @if (Model.IsComplete)
        {
            <text>✅ تمام داده‌های اولیه ایجاد شده‌اند</text>
        }
        else
        {
            <text>⚠️ برخی از داده‌های اولیه ایجاد نشده‌اند</text>
        }
    </p>
</div>
```

---

#### **2. آمار داده‌ها:**

```cshtml
<div class="row">
    <!-- کای‌ها -->
    <div class="col-md-3">
        <div class="info-box">
            <span class="info-box-icon bg-info">
                <i class="fas fa-cogs"></i>
            </span>
            <div class="info-box-content">
                <span class="info-box-text">کای‌های تنظیم شده</span>
                <span class="info-box-number">@Model.FactorSettingsCount</span>
            </div>
        </div>
    </div>
    
    <!-- خدمات -->
    <div class="col-md-3">
        <div class="info-box">
            <span class="info-box-icon bg-success">
                <i class="fas fa-stethoscope"></i>
            </span>
            <div class="info-box-content">
                <span class="info-box-text">خدمات فعال</span>
                <span class="info-box-number">@Model.ServicesCount</span>
            </div>
        </div>
    </div>
    
    <!-- ... بقیه آمار -->
</div>
```

---

#### **3. دکمه‌های عملیات:**

```cshtml
<div class="btn-group">
    @if (!Model.IsComplete)
    {
        <button type="button" class="btn btn-primary" onclick="seedAllData()">
            <i class="fas fa-database"></i>
            ایجاد تمام داده‌های اولیه
        </button>
        
        <button type="button" class="btn btn-info" onclick="seedDataStepByStep()">
            <i class="fas fa-step-forward"></i>
            ایجاد مرحله‌ای
        </button>
    }
    
    <button type="button" class="btn btn-secondary" onclick="refreshStatus()">
        <i class="fas fa-sync"></i>
        بروزرسانی وضعیت
    </button>
    
    @if (Model.IsComplete)
    {
        <button type="button" class="btn btn-danger" onclick="clearSeedData()">
            <i class="fas fa-trash"></i>
            پاک کردن داده‌ها
        </button>
    }
</div>
```

---

#### **4. JavaScript Functions:**

```javascript
// تابع کمکی برای ارسال درخواست POST با Anti-Forgery Token
function postWithAntiForgery(url, successCallback, errorCallback) {
    var token = getAntiForgeryToken();
    if (!token) {
        showError('خطا در دریافت توکن امنیتی');
        return;
    }

    $.ajax({
        url: url,
        type: 'POST',
        data: {
            __RequestVerificationToken: token
        },
        success: successCallback,
        error: errorCallback
    });
}

// ایجاد تمام داده‌ها
function seedAllData() {
    if (confirm('آیا مطمئن هستید؟')) {
        showLoading();
        postWithAntiForgery(
            '@Url.Action("SeedAllData", "SystemSeed")',
            function() {
                hideLoading();
                location.reload();
            },
            function(xhr) {
                hideLoading();
                showError('خطا در ایجاد داده‌های اولیه: ' + xhr.responseText);
            }
        );
    }
}
```

---

## 🔄 **فرآیند Seeding**

### **جریان کامل:**

```
┌────────────────────────────────────────────────────────────────┐
│  کاربر Admin وارد صفحه SystemSeed/Index می‌شود                │
└────────────────────────────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ SystemSeedController.Index()              │
        │ - GetSeedDataStatusAsync()                │
        │ - نمایش وضعیت فعلی سیستم                 │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ کاربر کلیک روی "ایجاد تمام داده‌های اولیه"│
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ SystemSeedController.SeedAllData() [POST] │
        │ ├─ ValidateAntiForgeryToken               │
        │ └─ SystemSeedService.SeedAllDataAsync()   │
        └───────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│              SystemSeedService.SeedAllDataAsync()               │
└─────────────────────────────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 0. SystemUsers.Initialize()               │
        │    - کش کردن SystemUserId                │
        │    - کش کردن AdminUserId                 │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 1. FactorSettingSeedService               │
        │    ├─ SeedFactorSettingsAsync()           │
        │    │  - کای فنی: 4,350,000 ریال          │
        │    │  - کای حرفه‌ای: 1,370,000 ریال       │
        │    │  - کای کد ۷: 2,750,000 ریال          │
        │    │  - ... (10+ کای دیگر)               │
        │    └─ SeedPreviousYearFactorsAsync()      │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 2. ServiceTemplateSeedService             │
        │    └─ SeedServiceTemplatesAsync()         │
        │       - 970000: پزشک عمومی               │
        │       - 970015: پزشک متخصص              │
        │       - 970030: پزشک فوق تخصص           │
        │       - ... (20+ قالب دیگر)              │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 3. ServiceSeedService                     │
        │    └─ SeedSampleServicesAsync()           │
        │       - ویزیت پزشک عمومی (970000)       │
        │       - ویزیت پزشک متخصص (970015)       │
        │       - ... (10+ خدمت دیگر)              │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 4. ServiceSeedService                     │
        │    └─ SeedServiceComponentsAsync()        │
        │       برای هر Service:                    │
        │       ├─ Technical Component (0.5)        │
        │       └─ Professional Component (1.3)     │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 5. ServiceSeedService                     │
        │    └─ SeedSharedServicesAsync()           │
        │       - اگر دپارتمان‌های مختلف وجود دارند│
        │       - ایجاد SharedService برای هر خدمت │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ 6. Validation                             │
        │    ├─ ValidateRequiredFactorsAsync()      │
        │    └─ ValidateSeededDataAsync()           │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ ✅ Success Message                         │
        │ "داده‌های اولیه با موفقیت ایجاد شدند"    │
        └───────────────────────────────────────────┘
                            ↓
        ┌───────────────────────────────────────────┐
        │ Redirect به Index                         │
        │ - نمایش وضعیت جدید                       │
        │ - آمار به‌روزرسانی شده                   │
        └───────────────────────────────────────────┘
```

---

## 🔧 **Registration در Unity DI**

### **مسیر:**
```
App_Start/UnityConfig.cs
```

### **ثبت سرویس‌ها:**

```csharp
public static class UnityConfig
{
    public static void RegisterComponents()
    {
        var container = new UnityContainer();

        // ... سایر ثبت‌ها ...

        // ✅ ثبت سرویس‌های Seed Data
        container.RegisterType<FactorSettingSeedService>(new PerRequestLifetimeManager());
        container.RegisterType<ServiceSeedService>(new PerRequestLifetimeManager());
        container.RegisterType<ServiceTemplateSeedService>(new PerRequestLifetimeManager());
        container.RegisterType<SystemSeedService>(new PerRequestLifetimeManager());

        // ... سایر ثبت‌ها ...
    }
}
```

**Lifetime:**
- `PerRequestLifetimeManager`: یک نمونه برای هر HTTP Request
- مناسب برای Service Layer
- کارایی بالا و مدیریت منابع بهینه

---

## 💡 **Use Cases**

### **1. راه‌اندازی اولیه سیستم (Production):**

```
Scenario: سیستم تازه نصب شده و خالی است
Action: Admin → SystemSeed/Index → "ایجاد تمام داده‌های اولیه"
Result:
  ✅ کای‌های مصوبه 1404 ایجاد می‌شوند
  ✅ قالب‌های خدمات ایجاد می‌شوند
  ✅ خدمات نمونه ایجاد می‌شوند
  ✅ سیستم آماده برای استفاده
```

---

### **2. محیط تست (Development/Staging):**

```
Scenario: توسعه‌دهنده نیاز به داده‌های نمونه دارد
Action: Developer → SystemSeed/Index → "ایجاد مرحله‌ای"
Result:
  ✅ مراحل به صورت جداگانه اجرا می‌شوند
  ✅ لاگ‌های دقیق‌تر برای دیباگ
  ✅ تاخیر 1 ثانیه بین مراحل برای نمایش
```

---

### **3. پاک کردن و ایجاد مجدد (Testing):**

```
Scenario: تست‌های خودکار نیاز به داده‌های تمیز دارند
Action:
  1. SystemSeed/ClearSeedData → پاک کردن داده‌ها
  2. SystemSeed/SeedAllData → ایجاد مجدد
Result:
  ✅ محیط تست تمیز
  ✅ داده‌های جدید و یکسان
  ✅ تست‌های قابل تکرار
```

---

### **4. بررسی وضعیت سیستم:**

```
Scenario: Admin می‌خواهد بداند سیستم کامل است یا نه
Action: SystemSeed/Index → مشاهده آمار
Result:
  ℹ️ تعداد کای‌ها: 10
  ℹ️ تعداد خدمات: 15
  ℹ️ تعداد خدمات مشترک: 30
  ℹ️ وضعیت: ✅ کامل / ⚠️ ناقص
```

---

## ✅ **Best Practices**

### **1. همیشه SystemUsers را Initialize کنید:**

```csharp
// ✅ درست: قبل از Seeding
SystemUsers.Initialize(_context);
await _factorSeedService.SeedFactorSettingsAsync();

// ❌ غلط: بدون Initialize
await _factorSeedService.SeedFactorSettingsAsync();
// CreatedByUserId ممکن است null باشد!
```

---

### **2. بررسی تکراری نبودن:**

```csharp
// ✅ درست: بررسی وجود قبل از Insert
var existingCodes = await _context.Services
    .Where(s => !s.IsDeleted)
    .Select(s => s.ServiceCode)
    .ToListAsync();

var newServices = sampleServices
    .Where(s => !existingCodes.Contains(s.ServiceCode))
    .ToList();

// ❌ غلط: Insert بدون بررسی
_context.Services.AddRange(sampleServices);
// ممکن است Duplicate Key Error!
```

---

### **3. Logging دقیق:**

```csharp
// ✅ درست: لاگ در هر مرحله
_logger.Information("شروع ایجاد کای‌های پایه");
await _factorSeedService.SeedFactorSettingsAsync();
_logger.Information("کای‌های پایه ایجاد شدند");

// ❌ غلط: بدون لاگ
await _factorSeedService.SeedFactorSettingsAsync();
```

---

### **4. Transaction Management:**

```csharp
// ✅ درست: استفاده از Transaction
using (var transaction = _context.Database.BeginTransaction())
{
    try
    {
        await SeedFactorSettings();
        await SeedServices();
        await SeedComponents();
        
        await _context.SaveChangesAsync();
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}

// ❌ غلط: بدون Transaction
await SeedFactorSettings();
await SeedServices(); // اگر اینجا خطا بیفتد، FactorSettings باقی می‌ماند!
```

---

### **5. Validation بعد از Seeding:**

```csharp
// ✅ درست: Validation نهایی
await SeedAllDataAsync();
var factorsValid = await ValidateRequiredFactorsAsync();
var servicesValid = await ValidateSeededDataAsync();

if (!factorsValid || !servicesValid)
{
    _logger.Warning("برخی از داده‌ها ایجاد نشدند");
}

// ❌ غلط: بدون Validation
await SeedAllDataAsync();
// فرض می‌کنیم همه چیز OK است!
```

---

## 🔒 **نکات امنیتی**

### **1. ValidateAntiForgeryToken:**

```csharp
// ✅ همه POST Actions دارای [ValidateAntiForgeryToken]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> SeedAllData() { ... }
```

---

### **2. Authorization:**

```csharp
// ⚠️ فقط Admin باید دسترسی داشته باشد
[Authorize(Roles = "Admin")]
public class SystemSeedController : BaseController { ... }
```

---

### **3. ClearSeedData محدود باشد:**

```csharp
// ✅ فقط در محیط تست
if (!IsDevelopmentEnvironment())
{
    return HttpNotFound(); // یا Forbidden
}

await _systemSeedService.ClearSeedDataAsync();
```

---

### **4. Logging حساس:**

```csharp
// ✅ لاگ عملیات حساس
_logger.Warning("پاک کردن داده‌های اولیه توسط کاربر {UserId}", _currentUserService.UserId);
await _systemSeedService.ClearSeedDataAsync();
```

---

## 📊 **خلاصه**

### **نقش SystemSeedController:**

```
✅ مدیریت ایجاد داده‌های اولیه سیستم
✅ کای‌های مصوبه 1404 (Technical & Professional)
✅ قالب‌های خدمات (ServiceTemplates)
✅ خدمات نمونه با اجزای کامل
✅ خدمات مشترک برای دپارتمان‌های مختلف
✅ بررسی وضعیت و Validation
✅ پاک کردن برای محیط تست
```

### **وابستگی‌های اصلی:**

```
1. SystemSeedService → هماهنگی کلی
2. FactorSettingSeedService → کای‌ها
3. ServiceSeedService → خدمات
4. ServiceTemplateSeedService → قالب‌ها
5. SystemUsers → کاربران سیستمی
```

### **فرآیند:**

```
SystemUsers.Initialize()
    ↓
SeedFactorSettings() - کای‌های 1404
    ↓
SeedServiceTemplates() - قالب‌های خدمات
    ↓
SeedSampleServices() - خدمات نمونه
    ↓
SeedServiceComponents() - اجزای خدمات
    ↓
SeedSharedServices() - خدمات مشترک
    ↓
Validate() - اعتبارسنجی نهایی
```

### **زمان استفاده:**

```
✅ راه‌اندازی اولیه سیستم
✅ محیط تست و توسعه
✅ بررسی وضعیت سیستم
✅ تست محاسبات
⚠️ فقط توسط Admin
⚠️ ClearSeedData فقط در محیط تست
```

---

**🎯 نتیجه‌گیری:**

`SystemSeedController` یک **ابزار حیاتی** برای راه‌اندازی اولیه سیستم کلینیک شفا است. با **معماری لایه‌ای** و **وابستگی‌های مشخص**، فرآیند ایجاد داده‌های اولیه را **ایمن، قابل ردیابی، و قابل تکرار** می‌کند. 🚀


