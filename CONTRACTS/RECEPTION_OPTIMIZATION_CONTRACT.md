# 📋 **قرارداد بهینه‌سازی ماژول پذیرش - کلینیک شفا**

## 🎯 **اطلاعات قرارداد**

**تاریخ ایجاد:** 1404/07/11  
**نسخه:** 1.0  
**وضعیت:** ✅ نهایی شده  
**معمار:** AI Senior Developer  

---

## 📜 **قوانین الزام‌آور**

### **1. اصل اتمیک بودن تغییرات (Atomic Changes)**
- ✅ **هر تغییر باید اتمیک باشد** - یا کاملاً موفق یا کاملاً ناموفق
- ✅ **عدم شکست سیستم** - هیچ تغییری نباید باعث شکست سیستم شود
- ✅ **قابلیت Rollback** - هر تغییر باید قابل بازگشت باشد
- ✅ **تست قبل از اعمال** - هر تغییر باید قبل از اعمال تست شود

### **2. اصل تک وظیفه‌ای (Single Responsibility)**
- ✅ **هر کلاس یک وظیفه** - هر کلاس فقط یک مسئولیت داشته باشد
- ✅ **جداسازی نگرانی‌ها** - هر لایه مسئولیت مشخص داشته باشد
- ✅ **عدم تکرار کد** - کد تکراری نباید وجود داشته باشد
- ✅ **قابلیت نگهداری** - کد باید قابل نگهداری باشد

### **3. اصل امنیت (Security First)**
- ✅ **اعتبارسنجی کامل** - تمام ورودی‌ها باید اعتبارسنجی شوند
- ✅ **Anti-Forgery Token** - تمام POST requests باید محافظت شوند
- ✅ **Authorization** - تمام عملیات باید مجوز داشته باشند
- ✅ **Audit Trail** - تمام عملیات باید ثبت شوند

### **4. اصل عملکرد (Performance Optimization)**
- ✅ **Compiled Queries** - کوئری‌های پرترافیک باید کامپایل شوند
- ✅ **N+1 Query Prevention** - از N+1 Query جلوگیری شود
- ✅ **Caching Strategy** - استراتژی کش مناسب پیاده‌سازی شود
- ✅ **Memory Optimization** - استفاده از حافظه بهینه شود

---

## 🏗️ **معماری الزام‌آور**

### **1. لایه‌های معماری:**

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Controllers   │  │      Views      │  │   ViewModels │ │
│  │                 │  │                 │  │              │ │
│  │ ReceptionController │ │ Index.cshtml   │  │ ReceptionVM  │ │
│  │ PatientController   │ │ Create.cshtml  │  │ PatientVM   │ │
│  │ DoctorController    │ │ Edit.cshtml    │  │ DoctorVM    │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │     Services     │  │   Validators    │  │   Mappers    │ │
│  │                 │  │                 │  │              │ │
│  │ ReceptionService │  │ ReceptionValidator│ │ ReceptionMapper│ │
│  │ PatientService   │  │ PatientValidator  │ │ PatientMapper  │ │
│  │ DoctorService    │  │ DoctorValidator   │ │ DoctorMapper   │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                    Domain Layer                             │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │    Entities     │  │   Value Objects │  │   Interfaces │ │
│  │                 │  │                 │  │              │ │
│  │ Reception       │  │ ReceptionNumber  │  │ IReception   │ │
│  │ Patient         │  │ PatientCode      │  │ IPatient     │ │
│  │ Doctor          │  │ DoctorCode       │  │ IDoctor      │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                      │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Repositories  │  │     Database    │  │   External   │ │
│  │                 │  │                 │  │   Services   │ │
│  │ ReceptionRepo   │  │ ApplicationDbContext│ │ InsuranceAPI │ │
│  │ PatientRepo     │  │ SQL Server      │  │ PaymentAPI   │ │
│  │ DoctorRepo      │  │ Entity Framework│  │ ExternalAPI  │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### **2. Interface های الزام‌آور:**

#### **2.1 Core Interfaces:**
```csharp
// Domain Layer
public interface IReception { }
public interface IPatient { }
public interface IDoctor { }

// Application Layer
public interface IReceptionService { }
public interface IReceptionBusinessRules { }
public interface IReceptionPerformanceOptimizer { }
public interface IReceptionSecurityService { }
public interface IReceptionCacheService { }

// Infrastructure Layer
public interface IReceptionRepository { }
public interface IPatientRepository { }
public interface IDoctorRepository { }
```

#### **2.2 Specialized Interfaces:**
```csharp
// Business Logic
public interface IReceptionValidator { }
public interface IReceptionCalculator { }
public interface IReceptionScheduler { }

// Performance
public interface IReceptionQueryOptimizer { }
public interface IReceptionCacheManager { }
public interface IReceptionMemoryOptimizer { }

// Security
public interface IReceptionAuthorizationService { }
public interface IReceptionAuditService { }
public interface IReceptionEncryptionService { }
```

---

## 🔧 **قوانین پیاده‌سازی**

### **1. قوانین Controller:**
```csharp
// ✅ الزامی: هر Controller باید:
[Authorize(Roles = "Receptionist,Admin")]
[ValidateAntiForgeryToken]
public class ReceptionController : BaseController
{
    // ✅ الزامی: Dependency Injection
    private readonly IReceptionService _receptionService;
    private readonly IReceptionBusinessRules _businessRules;
    private readonly IReceptionSecurityService _securityService;
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;

    // ✅ الزامی: Constructor Injection
    public ReceptionController(
        IReceptionService receptionService,
        IReceptionBusinessRules businessRules,
        IReceptionSecurityService securityService,
        ILogger logger,
        ICurrentUserService currentUserService)
    {
        _receptionService = receptionService;
        _businessRules = businessRules;
        _securityService = securityService;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    // ✅ الزامی: هر Action باید:
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> CreateReception(ReceptionCreateViewModel model)
    {
        try
        {
            // 1. Security Check
            if (!await _securityService.CanCreateReceptionAsync(_currentUserService.UserId))
            {
                return Json(new { success = false, message = "عدم دسترسی" });
            }

            // 2. Business Rules Validation
            var validationResult = await _businessRules.ValidateReceptionAsync(model);
            if (!validationResult.IsValid)
            {
                return Json(new { success = false, errors = validationResult.Errors });
            }

            // 3. Service Call
            var result = await _receptionService.CreateReceptionAsync(model);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new { success = true, data = result.Data });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ایجاد پذیرش");
            return Json(new { success = false, message = "خطا در ایجاد پذیرش" });
        }
    }
}
```

### **2. قوانین Service:**
```csharp
// ✅ الزامی: هر Service باید:
public class ReceptionService : IReceptionService
{
    // ✅ الزامی: Dependency Injection
    private readonly IReceptionRepository _repository;
    private readonly IReceptionBusinessRules _businessRules;
    private readonly IReceptionPerformanceOptimizer _performanceOptimizer;
    private readonly IReceptionSecurityService _securityService;
    private readonly IReceptionCacheService _cacheService;
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;

    // ✅ الزامی: Constructor Injection
    public ReceptionService(
        IReceptionRepository repository,
        IReceptionBusinessRules businessRules,
        IReceptionPerformanceOptimizer performanceOptimizer,
        IReceptionSecurityService securityService,
        IReceptionCacheService cacheService,
        ILogger logger,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _businessRules = businessRules;
        _performanceOptimizer = performanceOptimizer;
        _securityService = securityService;
        _cacheService = cacheService;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    // ✅ الزامی: هر متد باید:
    public async Task<ServiceResult<ReceptionDetailsViewModel>> CreateReceptionAsync(ReceptionCreateViewModel model)
    {
        try
        {
            // 1. Security Check
            if (!await _securityService.CanCreateReceptionAsync(_currentUserService.UserId))
            {
                return ServiceResult<ReceptionDetailsViewModel>.Failed(
                    "عدم دسترسی برای ایجاد پذیرش",
                    "ACCESS_DENIED",
                    ErrorCategory.Security,
                    SecurityLevel.High);
            }

            // 2. Business Rules Validation
            var validationResult = await _businessRules.ValidateReceptionAsync(model);
            if (!validationResult.IsValid)
            {
                return ServiceResult<ReceptionDetailsViewModel>.Failed(
                    validationResult.ErrorMessage,
                    "VALIDATION_FAILED",
                    ErrorCategory.Validation,
                    SecurityLevel.Low);
            }

            // 3. Create Reception
            var reception = new Reception
            {
                PatientId = model.PatientId,
                DoctorId = model.DoctorId,
                ReceptionDate = model.ReceptionDate,
                Status = ReceptionStatus.Pending,
                Type = model.Type,
                TotalAmount = model.TotalAmount,
                PatientCoPay = model.PatientShare,
                InsurerShareAmount = model.InsuranceShare,
                Notes = model.Notes,
                IsEmergency = model.IsEmergency,
                IsOnlineReception = model.IsOnlineReception,
                CreatedByUserId = _currentUserService.UserId,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            var createdReception = await _repository.AddAsync(reception);
            
            // 4. Return Result
            var result = ReceptionDetailsViewModel.FromEntity(createdReception);
            return ServiceResult<ReceptionDetailsViewModel>.Successful(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ایجاد پذیرش. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                model.PatientId, model.DoctorId, _currentUserService.UserName);
            
            return ServiceResult<ReceptionDetailsViewModel>.Failed(
                "خطا در ایجاد پذیرش. لطفاً مجدداً تلاش کنید.",
                "CREATE_RECEPTION_ERROR",
                ErrorCategory.System,
                SecurityLevel.Medium);
        }
    }
}
```

### **3. قوانین Repository:**
```csharp
// ✅ الزامی: هر Repository باید:
public class ReceptionRepository : BaseRepository<Reception>, IReceptionRepository
{
    // ✅ الزامی: Constructor Injection
    public ReceptionRepository(ApplicationDbContext context, ILogger logger) 
        : base(context, logger)
    {
    }

    // ✅ الزامی: هر متد باید:
    public async Task<List<Reception>> GetByDateAsync(DateTime date)
    {
        return await _dbSet
            .Where(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted)
            .AsNoTracking() // ✅ الزامی: AsNoTracking برای Read-only
            .ToListAsync();
    }

    // ✅ الزامی: Compiled Queries برای متدهای پرترافیک
    private static readonly Func<ApplicationDbContext, int, Task<Reception>> GetReceptionById =
        EF.CompileAsyncQuery((ApplicationDbContext context, int id) =>
            context.Receptions.FirstOrDefault(r => r.ReceptionId == id));

    public async Task<Reception> GetByIdCompiledAsync(int id)
    {
        return await GetReceptionById(_context, id);
    }
}
```

---

## 🚀 **قوانین بهینه‌سازی عملکرد**

### **1. قوانین Database:**
```csharp
// ✅ الزامی: استفاده از Compiled Queries
private static readonly Func<ApplicationDbContext, DateTime, Task<List<Reception>>> GetReceptionsByDate =
    EF.CompileAsyncQuery((ApplicationDbContext context, DateTime date) =>
        context.Receptions.Where(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted));

// ✅ الزامی: استفاده از AsNoTracking
var receptions = await _context.Receptions
    .Where(r => !r.IsDeleted)
    .AsNoTracking()
    .ToListAsync();

// ✅ الزامی: استفاده از Include بهینه
var reception = await _context.Receptions
    .Include(r => r.Patient)
    .Include(r => r.Doctor)
    .Include(r => r.ReceptionItems)
    .FirstOrDefaultAsync(r => r.ReceptionId == id);
```

### **2. قوانین Caching:**
```csharp
// ✅ الزامی: Cache Strategy
public async Task<List<ReceptionLookupViewModel>> GetDoctorsAsync()
{
    var cacheKey = "reception_doctors";
    
    if (_cache.TryGetValue(cacheKey, out List<ReceptionLookupViewModel> cachedDoctors))
        return cachedDoctors;

    var doctors = await _doctorService.GetActiveDoctorsAsync();
    var lookupDoctors = doctors.Select(d => new ReceptionLookupViewModel
    {
        Id = d.DoctorId,
        Name = d.FullName,
        Code = d.DoctorCode
    }).ToList();

    _cache.Set(cacheKey, lookupDoctors, TimeSpan.FromHours(1));
    return lookupDoctors;
}
```

### **3. قوانین Memory:**
```csharp
// ✅ الزامی: Memory Optimization
public async Task<List<Reception>> GetReceptionsOptimizedAsync(ReceptionSearchCriteria criteria)
{
    var query = _context.Receptions
        .Where(r => !r.IsDeleted)
        .AsNoTracking(); // ✅ الزامی: AsNoTracking

    if (criteria.PatientId.HasValue)
        query = query.Where(r => r.PatientId == criteria.PatientId.Value);

    return await query
        .OrderByDescending(r => r.ReceptionDate)
        .Skip(criteria.PageNumber * criteria.PageSize)
        .Take(criteria.PageSize)
        .ToListAsync();
}
```

---

## 🔒 **قوانین امنیت**

### **1. قوانین Authorization:**
```csharp
// ✅ الزامی: هر Controller باید:
[Authorize(Roles = "Receptionist,Admin")]
public class ReceptionController : BaseController
{
    // ✅ الزامی: هر Action باید:
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> CreateReception(ReceptionCreateViewModel model)
    {
        // ✅ الزامی: Security Check
        if (!await _securityService.CanCreateReceptionAsync(_currentUserService.UserId))
        {
            return Json(new { success = false, message = "عدم دسترسی" });
        }
        
        // ... rest of the method
    }
}
```

### **2. قوانین Input Validation:**
```csharp
// ✅ الزامی: Input Sanitization
public async Task<ValidationResult> ValidateReceptionInputAsync(string input)
{
    if (string.IsNullOrWhiteSpace(input))
        return ValidationResult.Failed("ورودی نمی‌تواند خالی باشد");

    // ✅ الزامی: XSS Protection
    if (input.Contains("<script>") || input.Contains("javascript:"))
        return ValidationResult.Failed("ورودی نامعتبر");

    // ✅ الزامی: SQL Injection Protection
    if (input.Contains("'; DROP TABLE"))
        return ValidationResult.Failed("ورودی نامعتبر");

    return ValidationResult.Success();
}
```

### **3. قوانین Audit Trail:**
```csharp
// ✅ الزامی: Audit Logging
public async Task<bool> LogSecurityActionAsync(string userId, string action, string resource, bool result)
{
    var auditEntry = new SecurityAuditEntry
    {
        UserId = userId,
        Action = action,
        Resource = resource,
        Result = result,
        IpAddress = HttpContext.Current.Request.UserHostAddress,
        UserAgent = HttpContext.Current.Request.UserAgent,
        Timestamp = DateTime.Now
    };

    await _auditRepository.AddAsync(auditEntry);
    return true;
}
```

---

## 📊 **قوانین تست**

### **1. قوانین Unit Test:**
```csharp
// ✅ الزامی: هر Service باید Unit Test داشته باشد
[TestClass]
public class ReceptionServiceTests
{
    [TestMethod]
    public async Task CreateReceptionAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        var model = new ReceptionCreateViewModel
        {
            PatientId = 1,
            DoctorId = 1,
            ReceptionDate = DateTime.Today.AddDays(1),
            TotalAmount = 100000,
            PatientShare = 20000,
            InsuranceShare = 80000
        };

        // Act
        var result = await _receptionService.CreateReceptionAsync(model);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(ReceptionStatus.Pending, result.Data.Status);
    }
}
```

### **2. قوانین Integration Test:**
```csharp
// ✅ الزامی: هر Controller باید Integration Test داشته باشد
[TestClass]
public class ReceptionControllerIntegrationTests
{
    [TestMethod]
    public async Task CreateReception_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var model = new ReceptionCreateViewModel
        {
            PatientId = 1,
            DoctorId = 1,
            ReceptionDate = DateTime.Today.AddDays(1)
        };

        // Act
        var result = await _controller.CreateReception(model);

        // Assert
        Assert.IsNotNull(result);
        var jsonResult = result as JsonResult;
        Assert.IsNotNull(jsonResult);
        
        var data = jsonResult.Data as dynamic;
        Assert.IsTrue(data.success);
    }
}
```

---

## 🎯 **نتیجه‌گیری**

این قرارداد شامل تمام قوانین الزام‌آور برای بهینه‌سازی ماژول پذیرش است:

### **✅ الزامات کلیدی:**
1. **اتمیک بودن تغییرات** - عدم شکست سیستم
2. **تک وظیفه‌ای بودن** - هر کلاس یک مسئولیت
3. **امنیت بالا** - اعتبارسنجی کامل
4. **عملکرد بهینه** - Compiled Queries و Caching
5. **تست‌پذیری** - Unit Tests و Integration Tests

### **🚀 آماده برای پیاده‌سازی:**
- Interface های تعریف شده
- قوانین پیاده‌سازی
- قوانین امنیت
- قوانین عملکرد
- قوانین تست

---

**📝 تهیه شده توسط:** AI Senior Developer  
**📅 تاریخ:** 1404/07/11  
**🔄 نسخه:** 1.0  
**✅ وضعیت:** نهایی شده
