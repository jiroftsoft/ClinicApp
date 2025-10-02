# 🗄️ **راهنمای جامع Repositories - سیستم کلینیک شفا**

> **تاریخ ایجاد**: 1404/07/11  
> **نسخه**: 1.0  
> **وضعیت**: نهایی شده  
> **تعداد کل**: 30 فایل Repository

---

## 📑 **فهرست**

1. [مقدمه](#مقدمه)
2. [ساختار کلی](#ساختار-کلی)
3. [Core Repositories](#core-repositories)
4. [Insurance Repositories](#insurance-repositories)
5. [ClinicAdmin Repositories](#clinicadmin-repositories)
6. [Payment Repositories](#payment-repositories)
7. [Query Patterns & Optimization](#query-patterns--optimization)
8. [Best Practices](#best-practices)

---

## 🎯 **مقدمه**

**Repositories** لایه **دسترسی به داده** هستند که بین **Services** و **Database** قرار دارند. این لایه مسئولیت‌های کلیدی زیر را دارد:

- ✅ **جداسازی منطق دسترسی به داده** از منطق کسب‌وکار
- ✅ **بهینه‌سازی Query ها** با Include و AsNoTracking
- ✅ **مدیریت روابط** بین Entity ها
- ✅ **Encapsulation** پیچیدگی Entity Framework
- ✅ **Testability** با Mock کردن Repositories

---

## 📊 **ساختار کلی**

```
Repositories/ (30 فایل)
│
├── 📂 Core (Root) - 5 فایل
│   ├── ReceptionRepository ━━━━━━━━━━ پذیرش‌های بیماران
│   ├── ServiceRepository ━━━━━━━━━━━━ خدمات پزشکی
│   ├── ServiceCategoryRepository ━━━━━ دسته‌بندی خدمات
│   ├── ClinicRepository ━━━━━━━━━━━━ کلینیک‌ها
│   └── DepartmentRepository ━━━━━━━━ دپارتمان‌ها
│
├── 📂 Insurance/ - 7 فایل
│   ├── PatientInsuranceRepository ━━━━ بیمه‌های بیماران
│   ├── InsuranceCalculationRepository ━ محاسبات بیمه
│   ├── InsuranceTariffRepository ━━━━━ تعرفه‌های بیمه
│   ├── InsurancePlanRepository ━━━━━━ طرح‌های بیمه
│   ├── InsuranceProviderRepository ━━━ ارائه‌دهندگان
│   ├── PlanServiceRepository ━━━━━━━━ خدمات طرح
│   └── BusinessRuleRepository ━━━━━━━ قوانین کسب‌وکار
│
├── 📂 ClinicAdmin/ - 10 فایل
│   ├── DoctorCrudRepository ━━━━━━━━━ CRUD پزشکان
│   ├── DoctorScheduleRepository ━━━━━━ برنامه کاری
│   ├── DoctorDepartmentRepository ━━━━ روابط با دپارتمان
│   ├── DoctorServiceCategoryRepository ━ روابط با سرفصل
│   ├── DoctorAssignmentRepository ━━━━ تخصیص پزشکان
│   ├── DoctorAssignmentHistoryRepository
│   ├── DoctorDashboardRepository ━━━━━ داشبورد
│   ├── DoctorReportingRepository ━━━━━ گزارشات
│   └── SpecializationRepository ━━━━━━ تخصص‌ها
│
└── 📂 Payment/ - 5 فایل
    ├── PaymentTransactionRepository ━━━ تراکنش‌های پرداخت
    ├── OnlinePaymentRepository ━━━━━━━ پرداخت‌های آنلاین
    ├── PaymentGatewayRepository ━━━━━━ درگاه‌های پرداخت
    ├── PosTerminalRepository ━━━━━━━━ ترمینال‌های POS
    └── CashSessionRepository ━━━━━━━━ جلسات نقدی

جمع کل: 30 Repository
```

---

## 1️⃣ **Core Repositories (5 فایل)**

### 📌 **ویژگی‌های مشترک:**

```csharp
// الگوی استاندارد تمام Repositories
public class SomeRepository : ISomeRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;
    
    // Constructor Injection
    public SomeRepository(
        ApplicationDbContext context, 
        ILogger logger, 
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }
}
```

---

### 🔷 **1. ReceptionRepository** - پذیرش‌های بیماران

**📍 مکان:** `Repositories/ReceptionRepository.cs`

**🎯 مسئولیت:**
- مدیریت دسترسی به Entity های پذیرش
- Eager Loading برای روابط (Patient, Doctor, Insurance)
- Query های بهینه‌شده برای جستجو و فیلتر
- Transaction Management

**💡 Query Patterns مهم:**

#### **1. GetByIdWithDetailsAsync - Eager Loading:**
```csharp
public async Task<Reception> GetByIdWithDetailsAsync(int id)
{
    return await _context.Receptions
        .Include(r => r.Patient)                              // ✅ بیمار
        .Include(r => r.Doctor)                               // ✅ پزشک
        .Include(r => r.ActivePatientInsurance)               // ✅ بیمه فعال
        .Include(r => r.ActivePatientInsurance.InsurancePlan) // ✅ طرح بیمه
        .Include(r => r.ActivePatientInsurance.InsurancePlan.InsuranceProvider) // ✅ ارائه‌دهنده
        .Include(r => r.ReceptionItems)                       // ✅ آیتم‌های پذیرش
        .Include(r => r.Transactions)                         // ✅ تراکنش‌ها
        .Include(r => r.CreatedByUser)                        // ✅ کاربر ایجادکننده
        .Include(r => r.UpdatedByUser)                        // ✅ کاربر ویرایشگر
        .Where(r => r.ReceptionId == id && !r.IsDeleted)
        .AsNoTracking()                                       // ✅ Read-Only
        .FirstOrDefaultAsync();
}
```

**🔑 نکات کلیدی:**
- ✅ **Include Chain**: چندین سطح روابط را Eager Load می‌کند
- ✅ **AsNoTracking**: برای Query های Read-Only
- ✅ **Soft Delete Check**: `!r.IsDeleted`

---

#### **2. GetPagedAsync - Paging & Filtering:**
```csharp
public async Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetPagedAsync(
    int? patientId, 
    int? doctorId, 
    ReceptionStatus? status, 
    string searchTerm, 
    int pageNumber, 
    int pageSize)
{
    // شروع Query
    var query = _context.Receptions
        .Include(r => r.Patient)
        .Include(r => r.Doctor)
        .Include(r => r.ActivePatientInsurance)
        .Where(r => !r.IsDeleted);
    
    // فیلتر بر اساس بیمار
    if (patientId.HasValue)
        query = query.Where(r => r.PatientId == patientId.Value);
    
    // فیلتر بر اساس پزشک
    if (doctorId.HasValue)
        query = query.Where(r => r.DoctorId == doctorId.Value);
    
    // فیلتر بر اساس وضعیت
    if (status.HasValue)
        query = query.Where(r => r.Status == status.Value);
    
    // جستجو در کد ملی/نام بیمار/نام پزشک
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        query = query.Where(r => 
            r.Patient.NationalCode.Contains(searchTerm) ||
            r.Patient.FirstName.Contains(searchTerm) ||
            r.Patient.LastName.Contains(searchTerm) ||
            r.Doctor.FirstName.Contains(searchTerm) ||
            r.Doctor.LastName.Contains(searchTerm));
    }
    
    // مرتب‌سازی
    query = query.OrderByDescending(r => r.ReceptionDate);
    
    // محاسبه تعداد کل
    var totalCount = await query.CountAsync();
    
    // صفحه‌بندی
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .ToListAsync();
    
    // تبدیل به ViewModel
    var viewModels = items.Select(r => ReceptionIndexViewModel.FromEntity(r)).ToList();
    
    // ایجاد PagedResult
    var pagedResult = new PagedResult<ReceptionIndexViewModel>(
        viewModels, totalCount, pageNumber, pageSize);
    
    return ServiceResult<PagedResult<ReceptionIndexViewModel>>.Successful(pagedResult);
}
```

**🔑 نکات کلیدی:**
- ✅ **Dynamic Filtering**: فیلترهای اختیاری
- ✅ **Search Pattern**: جستجو در چندین فیلد
- ✅ **Efficient Paging**: Count ابتدا، سپس Skip/Take
- ✅ **AsNoTracking**: برای لیست‌های Read-Only

---

#### **3. Transaction Management:**
```csharp
public DbContextTransaction BeginTransaction()
{
    return _context.Database.BeginTransaction();
}

// استفاده در Service:
using (var transaction = _receptionRepository.BeginTransaction())
{
    try
    {
        // عملیات 1: ایجاد پذیرش
        _receptionRepository.Add(reception);
        await _receptionRepository.SaveChangesAsync();
        
        // عملیات 2: ایجاد آیتم‌ها
        foreach (var item in receptionItems)
        {
            _context.ReceptionItems.Add(item);
        }
        await _context.SaveChangesAsync();
        
        // عملیات 3: محاسبات بیمه
        // ...
        
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

---

### 🔷 **2. ServiceRepository** - خدمات پزشکی

**📍 مکان:** `Repositories/ServiceRepository.cs`

**🎯 مسئولیت:**
- مدیریت دسترسی به خدمات پزشکی
- Eager Loading برای ServiceCategory و Department
- Query های بهینه‌شده برای محاسبات قیمت
- مدیریت ServiceComponents

**💡 Query Patterns مهم:**

#### **1. GetByIdAsync - با روابط کامل:**
```csharp
public Task<Service> GetByIdAsync(int id)
{
    return _context.Services
        .Include(s => s.ServiceCategory.Department.Clinic)  // ✅ 3 سطح روابط
        .Include(s => s.CreatedByUser)
        .Include(s => s.UpdatedByUser)
        .FirstOrDefaultAsync(s => s.ServiceId == id);
}
```

#### **2. جستجوی بهینه:**
```csharp
public async Task<List<Service>> GetServicesAsync(int serviceCategoryId, string searchTerm)
{
    var query = _context.Services
        .AsNoTracking()                                    // ✅ Read-Only
        .Where(s => s.ServiceCategoryId == serviceCategoryId);
    
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var normalizedTerm = searchTerm.Trim();
        query = query.Where(s => 
            s.Title.Contains(normalizedTerm) || 
            s.ServiceCode.Contains(normalizedTerm));
    }
    
    return await query.OrderBy(s => s.Title).ToListAsync();
}
```

**🔑 بهینه‌سازی:**
- ✅ `AsNoTracking()` برای Query های Read-Only
- ✅ Normalize کردن searchTerm قبل از Query
- ✅ مرتب‌سازی برای نمایش بهتر

---

#### **3. Existence Checks - بهینه:**
```csharp
public Task<bool> DoesServiceExistAsync(
    int serviceCategoryId, 
    string serviceCode, 
    int? excludeServiceId = null)
{
    var query = _context.Services
        .AsNoTracking()                                    // ✅ سریع‌ترین
        .Where(s => s.ServiceCategoryId == serviceCategoryId && 
                    s.ServiceCode == serviceCode);
    
    if (excludeServiceId.HasValue)
    {
        query = query.Where(s => s.ServiceId != excludeServiceId.Value);
    }
    
    return query.AnyAsync();                               // ✅ AnyAsync بهتر از Count
}
```

**🔑 بهینه‌سازی:**
- ✅ `AnyAsync()` به جای `CountAsync() > 0` (سریع‌تر)
- ✅ `AsNoTracking()` برای Existence Checks
- ✅ Exclude Pattern برای Edit Scenarios

---

## 2️⃣ **Insurance Repositories (7 فایل)**

### 🔷 **1. PatientInsuranceRepository** - بیمه‌های بیماران

**📍 مکان:** `Repositories/Insurance/PatientInsuranceRepository.cs`

**💡 Query Patterns مهم:**

#### **1. GetActiveByPatientAsync - دریافت بیمه فعال:**
```csharp
public async Task<ServiceResult<PatientInsurance>> GetActiveByPatientAsync(int patientId)
{
    try
    {
        var today = DateTime.Today;
        
        var patientInsurance = await _context.PatientInsurances
            .Where(pi => 
                pi.PatientId == patientId &&          // ✅ بیمار مورد نظر
                pi.IsActive &&                        // ✅ فعال
                pi.StartDate <= today &&              // ✅ شروع شده
                (!pi.EndDate.HasValue || pi.EndDate >= today) && // ✅ منقضی نشده
                !pi.IsDeleted)                        // ✅ حذف نشده
            .Include(pi => pi.InsurancePlan)
            .Include(pi => pi.InsurancePlan.InsuranceProvider)
            .OrderByDescending(pi => pi.IsPrimary)    // ✅ اولویت بیمه اصلی
            .ThenByDescending(pi => pi.StartDate)     // ✅ جدیدترین
            .AsNoTracking()
            .FirstOrDefaultAsync();
        
        if (patientInsurance == null)
            return ServiceResult<PatientInsurance>.Failed("بیمه فعال یافت نشد");
        
        return ServiceResult<PatientInsurance>.Successful(patientInsurance);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در دریافت بیمه فعال بیمار");
        return ServiceResult<PatientInsurance>.Failed("خطا در دریافت بیمه فعال");
    }
}
```

**🔑 نکات کلیدی:**
- ✅ **Date Range Check**: بررسی تاریخ اعتبار
- ✅ **Primary First**: اولویت بیمه اصلی
- ✅ **Null Safety**: بررسی `EndDate.HasValue`
- ✅ **ServiceResult Pattern**: مدیریت یکپارچه خطا

---

#### **2. GetPagedWithFiltersAsync - فیلترینگ پیشرفته:**
```csharp
public async Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPagedWithFiltersAsync(
    int? patientId = null,
    string searchTerm = "",
    int? providerId = null,
    bool? isPrimary = null,
    bool? isActive = null,
    int pageNumber = 1,
    int pageSize = 20)
{
    try
    {
        var query = _context.PatientInsurances
            .Include(pi => pi.Patient)
            .Include(pi => pi.InsurancePlan.InsuranceProvider)
            .Where(pi => !pi.IsDeleted);
        
        // فیلتر بر اساس بیمار
        if (patientId.HasValue)
            query = query.Where(pi => pi.PatientId == patientId.Value);
        
        // فیلتر بر اساس ارائه‌دهنده
        if (providerId.HasValue)
            query = query.Where(pi => pi.InsurancePlan.InsuranceProviderId == providerId.Value);
        
        // فیلتر بر اساس نوع (اصلی/تکمیلی)
        if (isPrimary.HasValue)
            query = query.Where(pi => pi.IsPrimary == isPrimary.Value);
        
        // فیلتر بر اساس وضعیت فعال
        if (isActive.HasValue)
            query = query.Where(pi => pi.IsActive == isActive.Value);
        
        // جستجو
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(pi => 
                pi.PolicyNumber.ToLower().Contains(term) ||
                pi.Patient.FirstName.ToLower().Contains(term) ||
                pi.Patient.LastName.ToLower().Contains(term) ||
                pi.Patient.NationalCode.Contains(term));
        }
        
        // مرتب‌سازی
        query = query.OrderByDescending(pi => pi.IsPrimary)
                     .ThenByDescending(pi => pi.StartDate);
        
        // صفحه‌بندی
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
        
        // تبدیل به ViewModel
        var viewModels = items.Select(PatientInsuranceIndexViewModel.FromEntity).ToList();
        
        var pagedResult = new PagedResult<PatientInsuranceIndexViewModel>(
            viewModels, totalCount, pageNumber, pageSize);
        
        return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Successful(pagedResult);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در دریافت لیست بیمه‌های بیماران");
        return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Failed(
            "خطا در دریافت لیست بیمه‌های بیماران");
    }
}
```

**🔑 الگوهای کلیدی:**
- ✅ **Optional Filters**: همه فیلترها اختیاری
- ✅ **Case-Insensitive Search**: `.ToLower()`
- ✅ **Multi-Field Search**: جستجو در چند فیلد
- ✅ **Smart Ordering**: اولویت‌بندی هوشمند

---

### 🔷 **2. InsuranceCalculationRepository** - محاسبات بیمه

**📍 مکان:** `Repositories/Insurance/InsuranceCalculationRepository.cs`

**💡 Query Patterns مهم:**

#### **1. GetByReceptionIdAsync:**
```csharp
public async Task<List<InsuranceCalculation>> GetByReceptionIdAsync(int receptionId)
{
    try
    {
        return await _context.InsuranceCalculations
            .Include(ic => ic.Patient)
            .Include(ic => ic.Service)
            .Include(ic => ic.InsurancePlan)
            .Include(ic => ic.PatientInsurance)
            .Where(ic => ic.ReceptionId == receptionId)
            .AsNoTracking()
            .OrderBy(ic => ic.Service.Title)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در دریافت محاسبات بیمه پذیرش");
        throw;
    }
}
```

---

## 3️⃣ **ClinicAdmin Repositories (10 فایل)**

### 🔷 **1. DoctorCrudRepository** - مدیریت پزشکان

**📍 مکان:** `Repositories/ClinicAdmin/DoctorCrudRepository.cs`

**💡 Query Patterns مهم:**

#### **1. GetByIdWithDetailsAsync - روابط کامل:**
```csharp
public async Task<Doctor> GetByIdWithDetailsAsync(int doctorId)
{
    return await _context.Doctors
        .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
        .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization)) // ✅ تخصص‌ها
        .Include(d => d.DoctorDepartments.Select(dd => dd.Department))         // ✅ دپارتمان‌ها
        .Include(d => d.DoctorServiceCategories.Select(dsc => dsc.ServiceCategory)) // ✅ سرفصل‌ها
        .Include(d => d.Schedules)                                             // ✅ برنامه‌های کاری
        .Include(d => d.CreatedByUser)
        .Include(d => d.UpdatedByUser)
        .Include(d => d.DeletedByUser)
        .FirstOrDefaultAsync();
}
```

**🔑 نکات کلیدی:**
- ✅ **Many-to-Many**: استفاده از `.Select()` در EF6
- ✅ **Complete Relations**: تمام روابط برای Details View
- ✅ **Audit Trail**: CreatedBy, UpdatedBy, DeletedBy

---

#### **2. SearchDoctorsAsync - جستجوی پیشرفته:**
```csharp
public async Task<List<Doctor>> SearchDoctorsAsync(DoctorSearchViewModel filter)
{
    var query = _context.Doctors
        .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization))
        .Include(d => d.DoctorDepartments.Select(dd => dd.Department))
        .Where(d => !d.IsDeleted);
    
    // فیلتر بر اساس کلینیک
    if (filter.ClinicId.HasValue)
    {
        query = query.Where(d => d.DoctorDepartments
            .Any(dd => dd.Department.ClinicId == filter.ClinicId.Value));
    }
    
    // فیلتر بر اساس دپارتمان
    if (filter.DepartmentId.HasValue)
    {
        query = query.Where(d => d.DoctorDepartments
            .Any(dd => dd.DepartmentId == filter.DepartmentId.Value));
    }
    
    // فیلتر بر اساس تخصص
    if (filter.SpecializationId.HasValue)
    {
        query = query.Where(d => d.DoctorSpecializations
            .Any(ds => ds.SpecializationId == filter.SpecializationId.Value));
    }
    
    // جستجو در نام
    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
    {
        var term = filter.SearchTerm.Trim().ToLower();
        query = query.Where(d => 
            d.FirstName.ToLower().Contains(term) ||
            d.LastName.ToLower().Contains(term) ||
            d.NationalCode.Contains(term) ||
            d.MedicalCouncilCode.Contains(term));
    }
    
    // مرتب‌سازی
    query = query.OrderBy(d => d.FirstName).ThenBy(d => d.LastName);
    
    // صفحه‌بندی
    return await query
        .Skip((filter.PageNumber - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .AsNoTracking()
        .ToListAsync();
}
```

**🔑 الگوهای کلیدی:**
- ✅ **Complex Filtering**: فیلتر بر روی Many-to-Many
- ✅ **Any() Pattern**: بررسی وجود در Collection
- ✅ **Multi-Field Search**: نام، کد ملی، کد نظام پزشکی

---

## 4️⃣ **Payment Repositories (5 فایل)**

### 🔷 **1. PaymentTransactionRepository**

**📍 مکان:** `Repositories/Payment/PaymentTransactionRepository.cs`

**💡 Query Patterns:**

```csharp
public async Task<List<PaymentTransaction>> GetByReceptionIdAsync(int receptionId)
{
    return await _context.PaymentTransactions
        .Include(pt => pt.Reception)
        .Include(pt => pt.PaymentGateway)
        .Include(pt => pt.PosTerminal)
        .Include(pt => pt.CashSession)
        .Where(pt => pt.ReceptionId == receptionId && !pt.IsDeleted)
        .AsNoTracking()
        .OrderByDescending(pt => pt.CreatedAt)
        .ToListAsync();
}
```

---

## 🎨 **Query Patterns & Optimization**

### 1️⃣ **AsNoTracking Pattern**

**استفاده:**
```csharp
// ✅ درست - برای Read-Only Queries
var patients = await _context.Patients
    .AsNoTracking()
    .ToListAsync();

// ❌ اشتباه - برای Update Scenarios
var patient = await _context.Patients
    .AsNoTracking()  // ❌ نمی‌توان Update کرد
    .FirstOrDefaultAsync(p => p.PatientId == id);
patient.FirstName = "نام جدید";
await _context.SaveChangesAsync(); // ❌ کار نمی‌کند
```

**قاعده:**
- ✅ **Use AsNoTracking**: برای List, Search, Details (Read-Only)
- ❌ **Don't Use**: برای Edit, Update Operations

---

### 2️⃣ **Include Pattern - Eager Loading**

**Single Level:**
```csharp
var reception = await _context.Receptions
    .Include(r => r.Patient)  // ✅ یک سطح
    .FirstOrDefaultAsync();
```

**Multiple Levels:**
```csharp
var reception = await _context.Receptions
    .Include(r => r.ActivePatientInsurance)                               // سطح 1
    .Include(r => r.ActivePatientInsurance.InsurancePlan)                 // سطح 2
    .Include(r => r.ActivePatientInsurance.InsurancePlan.InsuranceProvider) // سطح 3
    .FirstOrDefaultAsync();
```

**Many-to-Many (EF6):**
```csharp
var doctor = await _context.Doctors
    .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization))
    .FirstOrDefaultAsync();
```

**قاعده:**
- ✅ **Use Include**: برای N+1 Query Problem
- ⚠️ **Be Careful**: Include های زیاد → Query سنگین
- ✅ **Best Practice**: فقط روابط مورد نیاز را Include کنید

---

### 3️⃣ **Paging Pattern**

```csharp
// ✅ الگوی صحیح صفحه‌بندی
public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize)
{
    // 1. Query پایه
    var query = _context.SomeEntity.AsNoTracking();
    
    // 2. فیلترها
    query = query.Where(x => x.IsActive);
    
    // 3. محاسبه تعداد کل (قبل از Skip/Take)
    var totalCount = await query.CountAsync();
    
    // 4. صفحه‌بندی
    var items = await query
        .OrderBy(x => x.Name)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    // 5. ایجاد PagedResult
    return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
}
```

**قاعده:**
- ✅ **Always**: CountAsync قبل از Skip/Take
- ✅ **Always**: OrderBy قبل از Skip/Take
- ✅ **Always**: Validate pageNumber و pageSize

---

### 4️⃣ **Search Pattern**

```csharp
// ✅ الگوی صحیح جستجو
public async Task<List<Patient>> SearchAsync(string searchTerm)
{
    var query = _context.Patients.AsNoTracking();
    
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var term = searchTerm.Trim().ToLower();          // ✅ Normalize
        query = query.Where(p => 
            p.FirstName.ToLower().Contains(term) ||      // ✅ Case-Insensitive
            p.LastName.ToLower().Contains(term) ||
            p.NationalCode.Contains(term) ||             // ✅ Exact Match
            p.PhoneNumber.Contains(term));
    }
    
    return await query
        .OrderBy(p => p.FirstName)
        .Take(20)                                        // ✅ Limit Results
        .ToListAsync();
}
```

**قاعده:**
- ✅ **Always**: Trim و ToLower
- ✅ **Multiple Fields**: جستجو در چند فیلد
- ✅ **Limit Results**: Take() برای Performance
- ⚠️ **SQL LIKE**: `Contains()` → `LIKE '%term%'` (آهسته روی Index)

---

### 5️⃣ **Existence Check Pattern**

```csharp
// ✅ درست - AnyAsync (سریع‌تر)
public async Task<bool> ExistsAsync(string nationalCode)
{
    return await _context.Patients
        .AsNoTracking()
        .AnyAsync(p => p.NationalCode == nationalCode);
}

// ❌ اشتباه - CountAsync (آهسته‌تر)
public async Task<bool> ExistsAsync(string nationalCode)
{
    var count = await _context.Patients
        .CountAsync(p => p.NationalCode == nationalCode);
    return count > 0;  // ❌ Unnecessary
}

// ❌ اشتباه - FirstOrDefaultAsync (آهسته‌تر)
public async Task<bool> ExistsAsync(string nationalCode)
{
    var patient = await _context.Patients
        .FirstOrDefaultAsync(p => p.NationalCode == nationalCode);
    return patient != null;  // ❌ Loads entire entity
}
```

**قاعده:**
- ✅ **Use AnyAsync**: برای Existence Checks
- ❌ **Avoid CountAsync**: وقتی فقط می‌خواهید بدانید وجود دارد یا نه
- ❌ **Avoid FirstOrDefaultAsync**: برای Existence Checks

---

### 6️⃣ **Transaction Pattern**

```csharp
// ✅ الگوی صحیح Transaction
public async Task<ServiceResult> ComplexOperationAsync(...)
{
    using (var transaction = _context.Database.BeginTransaction())
    {
        try
        {
            // عملیات 1
            _context.Entity1.Add(entity1);
            await _context.SaveChangesAsync();
            
            // عملیات 2
            _context.Entity2.Add(entity2);
            await _context.SaveChangesAsync();
            
            // عملیات 3
            var entity3 = await _context.Entity3.FindAsync(id);
            entity3.Status = Status.Updated;
            await _context.SaveChangesAsync();
            
            transaction.Commit();
            return ServiceResult.Successful();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.Error(ex, "خطا در عملیات ترکیبی");
            return ServiceResult.Failed("خطا در عملیات");
        }
    }
}
```

**قاعده:**
- ✅ **Use Transaction**: برای عملیات چندگانه
- ✅ **Always Commit**: در انتهای try block
- ✅ **Always Rollback**: در catch block
- ✅ **using Statement**: برای Dispose خودکار

---

## 💡 **Best Practices**

### ✅ **DO's - همیشه انجام بده:**

1. **AsNoTracking برای Read-Only:**
```csharp
// ✅
var items = await _context.Items.AsNoTracking().ToListAsync();
```

2. **Include برای Eager Loading:**
```csharp
// ✅
var item = await _context.Items.Include(i => i.Related).FirstOrDefaultAsync();
```

3. **Soft Delete Check:**
```csharp
// ✅
var items = await _context.Items.Where(i => !i.IsDeleted).ToListAsync();
```

4. **Exception Handling:**
```csharp
// ✅
try
{
    return await _context.Items.ToListAsync();
}
catch (Exception ex)
{
    _logger.Error(ex, "خطا در دریافت آیتم‌ها");
    throw;
}
```

5. **Dependency Injection:**
```csharp
// ✅
public ItemRepository(ApplicationDbContext context, ILogger logger)
{
    _context = context ?? throw new ArgumentNullException(nameof(context));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
}
```

---

### ❌ **DON'Ts - هرگز انجام نده:**

1. **AsNoTracking برای Update:**
```csharp
// ❌
var item = await _context.Items.AsNoTracking().FirstOrDefaultAsync();
item.Name = "جدید";
await _context.SaveChangesAsync(); // کار نمی‌کند
```

2. **N+1 Query Problem:**
```csharp
// ❌
var patients = await _context.Patients.ToListAsync();
foreach (var patient in patients)
{
    // ❌ یک Query برای هر بیمار
    var insurances = await _context.PatientInsurances
        .Where(pi => pi.PatientId == patient.PatientId)
        .ToListAsync();
}

// ✅ درست
var patients = await _context.Patients
    .Include(p => p.Insurances)  // یک Query برای همه
    .ToListAsync();
```

3. **CountAsync برای Existence:**
```csharp
// ❌
var count = await _context.Items.CountAsync();
if (count > 0) { ... }

// ✅
if (await _context.Items.AnyAsync()) { ... }
```

4. **ToList قبل از Where:**
```csharp
// ❌ همه داده‌ها را می‌آورد، بعد فیلتر می‌کند
var items = _context.Items.ToList().Where(i => i.IsActive).ToList();

// ✅ در SQL فیلتر می‌کند
var items = await _context.Items.Where(i => i.IsActive).ToListAsync();
```

5. **String Concatenation در Query:**
```csharp
// ❌ SQL Injection Risk
var query = $"SELECT * FROM Patients WHERE NationalCode = '{nationalCode}'";

// ✅ Parameterized
var patient = await _context.Patients
    .FirstOrDefaultAsync(p => p.NationalCode == nationalCode);
```

---

## 📊 **آمار کلی Repositories**

| **گروه** | **تعداد** | **استفاده** |
|---------|----------|-------------|
| **Core** | 5 | عملیات اصلی سیستم |
| **Insurance** | 7 | سیستم بیمه |
| **ClinicAdmin** | 10 | مدیریت پزشکان و دپارتمان |
| **Payment** | 5 | سیستم پرداخت |
| **Interfaces** | 3 | Interfaces عمومی |
| **جمع کل** | **30** | |

---

## 🚀 **نکات بهینه‌سازی**

### 1️⃣ **Index ها:**
```csharp
// در Entity Configuration
modelBuilder.Entity<Patient>()
    .HasIndex(p => p.NationalCode);  // ✅ برای WHERE و JOIN

modelBuilder.Entity<Reception>()
    .HasIndex(r => r.PatientId);     // ✅ برای Foreign Key
```

### 2️⃣ **Projection:**
```csharp
// ❌ کل Entity را می‌آورد
var patients = await _context.Patients.ToListAsync();

// ✅ فقط فیلدهای مورد نیاز
var patients = await _context.Patients
    .Select(p => new { p.PatientId, p.FirstName, p.LastName })
    .ToListAsync();
```

### 3️⃣ **Compiled Queries:**
```csharp
// برای Query های پرتکرار
private static readonly Func<ApplicationDbContext, int, Task<Patient>> _getPatientById =
    EF.CompileAsyncQuery((ApplicationDbContext ctx, int id) =>
        ctx.Patients.FirstOrDefault(p => p.PatientId == id));
```

---

## 📚 **مراجع مرتبط**

- [SERVICES_COMPREHENSIVE_GUIDE.md](./SERVICES_COMPREHENSIVE_GUIDE.md)
- [INTERFACES_COMPREHENSIVE_GUIDE.md](./INTERFACES_COMPREHENSIVE_GUIDE.md)
- [APP_PRINCIPLES_CONTRACT.md](./APP_PRINCIPLES_CONTRACT.md)
- [DATABASE_COMPREHENSIVE_SCHEMA.md](./DATABASE_COMPREHENSIVE_SCHEMA.md)

---

**✨ پایان مستند جامع Repositories ✨**


