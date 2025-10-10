# 🏗️ **طراحی معماری بهینه ماژول پذیرش - Clean Architecture**

## 🎯 **اصول طراحی**

**تاریخ طراحی:** 1404/07/11  
**نسخه:** 1.0  
**وضعیت:** ✅ نهایی شده  
**معمار:** AI Senior Developer  

---

## 🏛️ **معماری پیشنهادی - Clean Architecture**

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

---

## 🔧 **طراحی Interface های جدید**

### **2. Interface های اصلی:**

#### **2.1 Core Interfaces:**
```csharp
// Domain Layer Interfaces
public interface IReception
{
    int ReceptionId { get; }
    string ReceptionNumber { get; }
    int PatientId { get; }
    int DoctorId { get; }
    DateTime ReceptionDate { get; }
    ReceptionStatus Status { get; }
    ReceptionType Type { get; }
    decimal TotalAmount { get; }
    decimal PatientCoPay { get; }
    decimal InsurerShareAmount { get; }
    bool IsEmergency { get; }
    bool IsOnlineReception { get; }
    string Notes { get; }
}

// Application Layer Interfaces
public interface IReceptionService
{
    Task<ServiceResult<ReceptionDetailsViewModel>> CreateReceptionAsync(ReceptionCreateViewModel model);
    Task<ServiceResult<ReceptionDetailsViewModel>> UpdateReceptionAsync(ReceptionEditViewModel model);
    Task<ServiceResult<bool>> DeleteReceptionAsync(int id);
    Task<ServiceResult<ReceptionDetailsViewModel>> GetReceptionAsync(int id);
    Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetReceptionsAsync(ReceptionSearchViewModel searchModel);
    Task<ServiceResult<ReceptionValidationViewModel>> ValidateReceptionAsync(int patientId, int doctorId, DateTime receptionDate);
}

// Infrastructure Layer Interfaces
public interface IReceptionRepository
{
    Task<Reception> GetByIdAsync(int id);
    Task<List<Reception>> GetByDateAsync(DateTime date);
    Task<List<Reception>> GetByPatientAsync(int patientId);
    Task<List<Reception>> GetByDoctorAsync(int doctorId);
    Task<PagedResult<Reception>> GetPagedAsync(ReceptionSearchCriteria criteria);
    Task<Reception> AddAsync(Reception reception);
    Task<Reception> UpdateAsync(Reception reception);
    Task<bool> DeleteAsync(int id);
}
```

#### **2.2 Specialized Interfaces:**
```csharp
// Business Logic Interfaces
public interface IReceptionBusinessRules
{
    Task<ValidationResult> ValidateReceptionAsync(ReceptionCreateViewModel model);
    Task<ValidationResult> ValidatePatientAsync(int patientId);
    Task<ValidationResult> ValidateDoctorAsync(int doctorId, DateTime receptionDate);
    Task<ValidationResult> ValidateServicesAsync(List<int> serviceIds);
    Task<ValidationResult> ValidateInsuranceAsync(int patientId, DateTime receptionDate);
}

// Performance Interfaces
public interface IReceptionPerformanceOptimizer
{
    Task<List<Reception>> GetReceptionsOptimizedAsync(ReceptionSearchCriteria criteria);
    Task<ReceptionDetailsViewModel> GetReceptionDetailsOptimizedAsync(int id);
    Task<List<ReceptionIndexViewModel>> GetReceptionsIndexOptimizedAsync(ReceptionSearchCriteria criteria);
}

// Security Interfaces
public interface IReceptionSecurityService
{
    Task<bool> CanCreateReceptionAsync(string userId);
    Task<bool> CanEditReceptionAsync(string userId, int receptionId);
    Task<bool> CanDeleteReceptionAsync(string userId, int receptionId);
    Task<bool> CanViewReceptionAsync(string userId, int receptionId);
}

// Caching Interfaces
public interface IReceptionCacheService
{
    Task<List<ReceptionLookupViewModel>> GetDoctorsAsync();
    Task<List<ReceptionLookupViewModel>> GetServiceCategoriesAsync();
    Task<List<ReceptionLookupViewModel>> GetServicesAsync(int categoryId);
    Task<ReceptionLookupListsViewModel> GetLookupListsAsync();
}
```

---

## 🎯 **طراحی Service های جدید**

### **3. Service های تخصصی:**

#### **3.1 Core Services:**
```csharp
// ReceptionService - اصلی
public class ReceptionService : IReceptionService
{
    private readonly IReceptionRepository _repository;
    private readonly IReceptionBusinessRules _businessRules;
    private readonly IReceptionPerformanceOptimizer _performanceOptimizer;
    private readonly IReceptionSecurityService _securityService;
    private readonly IReceptionCacheService _cacheService;
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;

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

#### **3.2 Specialized Services:**
```csharp
// ReceptionBusinessRules - منطق کسب‌وکار
public class ReceptionBusinessRules : IReceptionBusinessRules
{
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly IServiceService _serviceService;
    private readonly IInsuranceService _insuranceService;
    private readonly IReceptionRepository _receptionRepository;
    private readonly ILogger _logger;

    public async Task<ValidationResult> ValidateReceptionAsync(ReceptionCreateViewModel model)
    {
        var errors = new List<string>();

        // Validate Patient
        var patientValidation = await ValidatePatientAsync(model.PatientId);
        if (!patientValidation.IsValid)
            errors.AddRange(patientValidation.Errors);

        // Validate Doctor
        var doctorValidation = await ValidateDoctorAsync(model.DoctorId, model.ReceptionDate);
        if (!doctorValidation.IsValid)
            errors.AddRange(doctorValidation.Errors);

        // Validate Services
        var servicesValidation = await ValidateServicesAsync(model.SelectedServiceIds);
        if (!servicesValidation.IsValid)
            errors.AddRange(servicesValidation.Errors);

        // Validate Insurance
        var insuranceValidation = await ValidateInsuranceAsync(model.PatientId, model.ReceptionDate);
        if (!insuranceValidation.IsValid)
            errors.AddRange(insuranceValidation.Errors);

        return errors.Any() 
            ? ValidationResult.Failed(string.Join(", ", errors))
            : ValidationResult.Success();
    }
}

// ReceptionPerformanceOptimizer - بهینه‌سازی عملکرد
public class ReceptionPerformanceOptimizer : IReceptionPerformanceOptimizer
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;

    // Compiled Queries
    private static readonly Func<ApplicationDbContext, int, Task<Reception>> GetReceptionById =
        EF.CompileAsyncQuery((ApplicationDbContext context, int id) =>
            context.Receptions.FirstOrDefault(r => r.ReceptionId == id));

    private static readonly Func<ApplicationDbContext, DateTime, Task<List<Reception>>> GetReceptionsByDate =
        EF.CompileAsyncQuery((ApplicationDbContext context, DateTime date) =>
            context.Receptions.Where(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted));

    public async Task<List<Reception>> GetReceptionsOptimizedAsync(ReceptionSearchCriteria criteria)
    {
        var cacheKey = $"receptions_{criteria.GetHashCode()}";
        
        if (_cache.TryGetValue(cacheKey, out List<Reception> cachedResult))
            return cachedResult;

        var query = _context.Receptions
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        if (criteria.PatientId.HasValue)
            query = query.Where(r => r.PatientId == criteria.PatientId.Value);

        if (criteria.DoctorId.HasValue)
            query = query.Where(r => r.DoctorId == criteria.DoctorId.Value);

        if (criteria.DateFrom.HasValue)
            query = query.Where(r => r.ReceptionDate >= criteria.DateFrom.Value);

        if (criteria.DateTo.HasValue)
            query = query.Where(r => r.ReceptionDate <= criteria.DateTo.Value);

        if (criteria.Status.HasValue)
            query = query.Where(r => r.Status == criteria.Status.Value);

        var result = await query
            .OrderByDescending(r => r.ReceptionDate)
            .Skip(criteria.PageNumber * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
        return result;
    }
}

// ReceptionSecurityService - امنیت
public class ReceptionSecurityService : IReceptionSecurityService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    public async Task<bool> CanCreateReceptionAsync(string userId)
    {
        // Check if user has Receptionist or Admin role
        return await _authorizationService.IsInRoleAsync(userId, "Receptionist") ||
               await _authorizationService.IsInRoleAsync(userId, "Admin");
    }

    public async Task<bool> CanEditReceptionAsync(string userId, int receptionId)
    {
        // Check if user is the creator or has Admin role
        var reception = await _receptionRepository.GetByIdAsync(receptionId);
        return reception.CreatedByUserId == userId ||
               await _authorizationService.IsInRoleAsync(userId, "Admin");
    }
}

// ReceptionCacheService - کش
public class ReceptionCacheService : IReceptionCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IDoctorService _doctorService;
    private readonly IServiceService _serviceService;
    private readonly ILogger _logger;

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
}
```

---

## 🔧 **طراحی Repository های جدید**

### **4. Repository Pattern:**

#### **4.1 Base Repository:**
```csharp
public interface IBaseRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
}

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly ILogger _logger;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
}
```

#### **4.2 Reception Repository:**
```csharp
public interface IReceptionRepository : IBaseRepository<Reception>
{
    Task<List<Reception>> GetByDateAsync(DateTime date);
    Task<List<Reception>> GetByPatientAsync(int patientId);
    Task<List<Reception>> GetByDoctorAsync(int doctorId);
    Task<PagedResult<Reception>> GetPagedAsync(ReceptionSearchCriteria criteria);
    Task<List<Reception>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<ReceptionStatistics> GetStatisticsAsync(DateTime date);
    Task<bool> HasActiveReceptionAsync(int patientId, DateTime date);
    Task<int> GetReceptionCountByDoctorAsync(int doctorId, DateTime date);
}

public class ReceptionRepository : BaseRepository<Reception>, IReceptionRepository
{
    public ReceptionRepository(ApplicationDbContext context, ILogger logger) 
        : base(context, logger)
    {
    }

    public async Task<List<Reception>> GetByDateAsync(DateTime date)
    {
        return await _dbSet
            .Where(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Reception>> GetByPatientAsync(int patientId)
    {
        return await _dbSet
            .Where(r => r.PatientId == patientId && !r.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(r => r.ReceptionDate)
            .ToListAsync();
    }

    public async Task<List<Reception>> GetByDoctorAsync(int doctorId)
    {
        return await _dbSet
            .Where(r => r.DoctorId == doctorId && !r.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(r => r.ReceptionDate)
            .ToListAsync();
    }

    public async Task<PagedResult<Reception>> GetPagedAsync(ReceptionSearchCriteria criteria)
    {
        var query = _dbSet.Where(r => !r.IsDeleted);

        if (criteria.PatientId.HasValue)
            query = query.Where(r => r.PatientId == criteria.PatientId.Value);

        if (criteria.DoctorId.HasValue)
            query = query.Where(r => r.DoctorId == criteria.DoctorId.Value);

        if (criteria.DateFrom.HasValue)
            query = query.Where(r => r.ReceptionDate >= criteria.DateFrom.Value);

        if (criteria.DateTo.HasValue)
            query = query.Where(r => r.ReceptionDate <= criteria.DateTo.Value);

        if (criteria.Status.HasValue)
            query = query.Where(r => r.Status == criteria.Status.Value);

        var totalCount = await query.CountAsync();
        
        var items = await query
            .AsNoTracking()
            .OrderByDescending(r => r.ReceptionDate)
            .Skip(criteria.PageNumber * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        return new PagedResult<Reception>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = criteria.PageNumber,
            PageSize = criteria.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / criteria.PageSize)
        };
    }

    public async Task<bool> HasActiveReceptionAsync(int patientId, DateTime date)
    {
        return await _dbSet
            .AnyAsync(r => r.PatientId == patientId && 
                          r.ReceptionDate.Date == date.Date && 
                          r.Status == ReceptionStatus.Pending &&
                          !r.IsDeleted);
    }

    public async Task<int> GetReceptionCountByDoctorAsync(int doctorId, DateTime date)
    {
        return await _dbSet
            .CountAsync(r => r.DoctorId == doctorId && 
                           r.ReceptionDate.Date == date.Date && 
                           !r.IsDeleted);
    }
}
```

---

## 🎯 **نتیجه‌گیری**

این معماری جدید با رعایت اصول Clean Architecture و SOLID Principles طراحی شده است:

### **✅ مزایا:**
1. **جداسازی نگرانی‌ها** - هر لایه مسئولیت مشخص دارد
2. **قابلیت تست‌پذیری** - Interface ها قابل Mock هستند
3. **قابلیت نگهداری** - کد تمیز و قابل فهم
4. **قابلیت توسعه** - باز برای توسعه، بسته برای تغییر
5. **عملکرد بهینه** - استفاده از Compiled Queries و Caching
6. **امنیت بالا** - Authorization و Validation کامل

### **🚀 آماده برای پیاده‌سازی:**
- Interface های تعریف شده
- Service های تخصصی
- Repository Pattern
- Security Layer
- Performance Optimization
- Caching Strategy

---

**📝 تهیه شده توسط:** AI Senior Developer  
**📅 تاریخ:** 1404/07/11  
**🔄 نسخه:** 1.0  
**✅ وضعیت:** نهایی شده
