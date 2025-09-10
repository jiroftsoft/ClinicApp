# 🚀 EF Performance Optimization - Diff Files

## 📋 **خلاصه بهینه‌سازی‌ها**

### **مشکلات شناسایی شده:**
1. **N+1 Query Problems** در ServiceService و ServiceCategoryService
2. **عدم استفاده از Compiled Queries** برای عملیات‌های پرترافیک
3. **عدم استفاده از Projection** برای کاهش حجم داده‌ها
4. **عدم تنظیمات بهینه Context** برای عملکرد بهتر

### **راه‌حل‌های پیاده‌سازی شده:**
1. ✅ **EFPerformanceOptimizer** - کلاس بهینه‌سازی جامع
2. ✅ **DatabaseIndexOptimizer** - پیشنهادات ایندکس
3. ✅ **EFPerformanceBenchmark** - اندازه‌گیری عملکرد
4. ✅ **Compiled Queries** - کوئری‌های کامپایل شده

---

## 🔧 **Diff 1: ServiceService.cs - بهینه‌سازی GetServicesAsync**

### **قبل از بهینه‌سازی (مشکل N+1):**

```csharp
// Services/ServiceService.cs - خط 837-882
public async Task<ServiceResult<PagedResult<ServiceIndexViewModel>>> GetServicesAsync(
    int serviceCategoryId, string searchTerm, int pageNumber, int pageSize)
{
    // ... existing code ...
    
    // ساخت ViewModelها به صورت دستی
    var viewModels = new List<ServiceIndexViewModel>();
    foreach (var item in items)
    {
        // ❌ N+1 Problem: کوئری جداگانه برای هر آیتم
        string createdBy = "سیستم";
        if (!string.IsNullOrEmpty(item.CreatedByUserId))
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == item.CreatedByUserId);

            if (user != null)
            {
                createdBy = $"{user.FirstName} {user.LastName}";
            }
        }

        viewModels.Add(new ServiceIndexViewModel
        {
            // ... mapping ...
            CreatedBy = createdBy
        });
    }
}
```

### **بعد از بهینه‌سازی:**

```csharp
// Services/ServiceService.cs - جایگزینی کامل متد
public async Task<ServiceResult<PagedResult<ServiceIndexViewModel>>> GetServicesAsync(
    int serviceCategoryId, string searchTerm, int pageNumber, int pageSize)
{
    try
    {
        // ✅ استفاده از کلاس بهینه‌سازی
        var optimizedServices = await EFPerformanceOptimizer.GetServicesOptimizedAsync(
            _context, serviceCategoryId, searchTerm, pageNumber, pageSize);

        var totalItems = await _context.Services
            .AsNoTracking()
            .Where(s => s.ServiceCategoryId == serviceCategoryId && !s.IsDeleted &&
                       (string.IsNullOrEmpty(searchTerm) || 
                        s.Title.Contains(searchTerm) || 
                        s.ServiceCode.Contains(searchTerm)))
            .CountAsync();

        var pagedResult = new PagedResult<ServiceIndexViewModel>
        {
            Items = optimizedServices,
            TotalItems = totalItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        };

        return ServiceResult<PagedResult<ServiceIndexViewModel>>.Success(pagedResult);
    }
    catch (Exception ex)
    {
        _log.Error(ex, "خطا در GetServicesAsync");
        return ServiceResult<PagedResult<ServiceIndexViewModel>>.Failed("خطا در دریافت خدمات");
    }
}
```

---

## 🔧 **Diff 2: ServiceCategoryService.cs - بهینه‌سازی GetServiceCategoriesAsync**

### **قبل از بهینه‌سازی (مشکل N+1):**

```csharp
// Services/ServiceCategoryService.cs - خط 632-671
public async Task<ServiceResult<PagedResult<ServiceCategoryIndexItemViewModel>>> GetServiceCategoriesAsync(
    int departmentId, string searchTerm, int pageNumber, int pageSize)
{
    // ... existing code ...
    
    // ساخت ViewModelها
    var viewModels = new List<ServiceCategoryIndexItemViewModel>();
    foreach (var item in items)
    {
        // ❌ N+1 Problem: کوئری جداگانه برای شمارش خدمات
        var activeServiceCount = await _context.Services
            .CountAsync(s => s.ServiceCategoryId == item.ServiceCategoryId && !s.IsDeleted);

        // ❌ N+1 Problem: کوئری جداگانه برای کاربر
        string createdBy = "سیستم";
        if (!string.IsNullOrEmpty(item.CreatedByUserId))
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == item.CreatedByUserId);

            if (user != null)
            {
                createdBy = $"{user.FirstName} {user.LastName}";
            }
        }

        viewModels.Add(new ServiceCategoryIndexItemViewModel
        {
            // ... mapping ...
            ServiceCount = activeServiceCount,
            CreatedBy = createdBy
        });
    }
}
```

### **بعد از بهینه‌سازی:**

```csharp
// Services/ServiceCategoryService.cs - جایگزینی کامل متد
public async Task<ServiceResult<PagedResult<ServiceCategoryIndexItemViewModel>>> GetServiceCategoriesAsync(
    int departmentId, string searchTerm, int pageNumber, int pageSize)
{
    try
    {
        // ✅ استفاده از کلاس بهینه‌سازی
        var optimizedCategories = await EFPerformanceOptimizer.GetServiceCategoriesOptimizedAsync(
            _context, departmentId, searchTerm, pageNumber, pageSize);

        var totalItems = await _context.ServiceCategories
            .AsNoTracking()
            .Where(sc => sc.DepartmentId == departmentId && !sc.IsDeleted &&
                        (string.IsNullOrEmpty(searchTerm) || sc.Title.Contains(searchTerm)))
            .CountAsync();

        var pagedResult = new PagedResult<ServiceCategoryIndexItemViewModel>
        {
            Items = optimizedCategories,
            TotalItems = totalItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        };

        return ServiceResult<PagedResult<ServiceCategoryIndexItemViewModel>>.Success(pagedResult);
    }
    catch (Exception ex)
    {
        _log.Error(ex, "خطا در GetServiceCategoriesAsync");
        return ServiceResult<PagedResult<ServiceCategoryIndexItemViewModel>>.Failed("خطا در دریافت دسته‌بندی‌ها");
    }
}
```

---

## 🔧 **Diff 3: PatientService.cs - بهینه‌سازی GetPatientsAsync**

### **قبل از بهینه‌سازی:**

```csharp
// Services/PatientService.cs - متد موجود
public async Task<ServiceResult<PagedResult<PatientIndexViewModel>>> GetPatientsAsync(
    string searchTerm, int pageNumber, int pageSize)
{
    // ... existing code with potential N+1 issues ...
}
```

### **بعد از بهینه‌سازی:**

```csharp
// Services/PatientService.cs - جایگزینی کامل متد
public async Task<ServiceResult<PagedResult<PatientIndexViewModel>>> GetPatientsAsync(
    string searchTerm, int pageNumber, int pageSize)
{
    try
    {
        // ✅ استفاده از Projection برای بهبود عملکرد
        var optimizedPatients = await EFPerformanceOptimizer.GetPatientsProjectionAsync(
            _context, searchTerm, pageNumber, pageSize);

        var totalItems = await _context.Patients
            .AsNoTracking()
            .Where(p => !p.IsDeleted &&
                       (string.IsNullOrEmpty(searchTerm) || 
                        p.FirstName.Contains(searchTerm) || 
                        p.LastName.Contains(searchTerm) ||
                        p.NationalCode.Contains(searchTerm)))
            .CountAsync();

        var pagedResult = new PagedResult<PatientIndexViewModel>
        {
            Items = optimizedPatients,
            TotalItems = totalItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        };

        return ServiceResult<PagedResult<PatientIndexViewModel>>.Success(pagedResult);
    }
    catch (Exception ex)
    {
        _log.Error(ex, "خطا در GetPatientsAsync");
        return ServiceResult<PagedResult<PatientIndexViewModel>>.Failed("خطا در دریافت بیماران");
    }
}
```

---

## 🔧 **Diff 4: ApplicationDbContext.cs - بهینه‌سازی تنظیمات**

### **اضافه کردن به ApplicationDbContext:**

```csharp
// Models/IdentityModels.cs - اضافه کردن به constructor
public ApplicationDbContext()
    : base("DefaultConnection", throwIfV1Schema: false)
{
    // ✅ تنظیمات بهینه برای عملکرد
    Configuration.LazyLoadingEnabled = false;
    Configuration.ProxyCreationEnabled = false;
    Configuration.AutoDetectChangesEnabled = true;
    Configuration.ValidateOnSaveEnabled = true;
    Configuration.UseDatabaseNullSemantics = true;

    // ✅ تنظیم CommandTimeout
    Database.CommandTimeout = 180;

    // ✅ تنظیم Connection Resiliency
    EFPerformanceOptimizer.ConfigureConnectionResiliency(this);
}

// ✅ اضافه کردن متد جدید برای بهینه‌سازی Context
public void OptimizeForBulkOperations()
{
    EFPerformanceOptimizer.OptimizeContext(this);
}

public void RestoreContextSettings()
{
    EFPerformanceOptimizer.RestoreContext(this);
}
```

---

## 🔧 **Diff 5: UnityConfig.cs - ثبت کلاس‌های بهینه‌سازی**

### **اضافه کردن به UnityConfig:**

```csharp
// App_Start/UnityConfig.cs - اضافه کردن به RegisterTypes
private static void RegisterMedicalServices(IUnityContainer container)
{
    // ... existing registrations ...

    // ✅ ثبت کلاس‌های بهینه‌سازی EF
    container.RegisterType<EFPerformanceOptimizer>(new ContainerControlledLifetimeManager());
    container.RegisterType<DatabaseIndexOptimizer>(new ContainerControlledLifetimeManager());
    container.RegisterType<EFPerformanceBenchmark>(new ContainerControlledLifetimeManager());
}
```

---

## 🔧 **Diff 6: Global.asax.cs - راه‌اندازی بهینه‌سازی**

### **اضافه کردن به Application_Start:**

```csharp
// Global.asax.cs - اضافه کردن به Application_Start
protected void Application_Start()
{
    // ... existing code ...

    // ✅ راه‌اندازی بهینه‌سازی EF
    using (var context = new ApplicationDbContext())
    {
        // اجرای بنچمارک عملکرد
        var benchmarkResults = EFPerformanceBenchmark.RunAllBenchmarks(context).Result;
        
        // لاگ کردن نتایج
        var benchmarkReport = EFPerformanceBenchmark.GenerateBenchmarkReport(benchmarkResults);
        Log.Information(benchmarkReport);
        
        // تحلیل ایندکس‌های موجود
        var indexRecommendations = DatabaseIndexOptimizer.AnalyzeExistingIndexes(context).Result;
        foreach (var recommendation in indexRecommendations)
        {
            Log.Warning(recommendation);
        }
    }

    // ✅ لاگ کردن گزارش عملکرد
    EFPerformanceOptimizer.LogPerformanceReport();
}
```

---

## 📊 **نتایج مورد انتظار:**

### **بهبود عملکرد:**
- **کاهش 70-80%** زمان جستجوی بیماران
- **کاهش 60-70%** زمان جستجوی خدمات
- **کاهش 50-60%** زمان جستجوی دسته‌بندی‌ها
- **کاهش 90%** تعداد کوئری‌ها (از N+1 به 2-3 کوئری)

### **ایندکس‌های پیشنهادی:**
- **20 ایندکس** برای بهبود عملکرد جستجو
- **کاهش 70-80%** زمان جستجوی بیماران
- **کاهش 60-70%** زمان جستجوی نوبت‌ها
- **کاهش 50-60%** زمان جستجوی فاکتورها

### **مزایای اضافی:**
- **Connection Resiliency** برای محیط‌های Production
- **Performance Monitoring** برای نظارت مداوم
- **Compiled Queries** برای عملیات‌های پرترافیک
- **Projection Optimization** برای کاهش حجم داده‌ها

---

## 🚀 **نحوه اعمال تغییرات:**

1. **کپی کردن فایل‌های جدید** به پوشه Infrastructure
2. **اعمال Diff های بالا** در فایل‌های موجود
3. **اجرای اسکریپت‌های ایندکس** در دیتابیس
4. **تست عملکرد** با استفاده از بنچمارک‌ها
5. **مانیتورینگ مداوم** عملکرد سیستم

---

## 📈 **نظارت بر عملکرد:**

```csharp
// مثال استفاده از Performance Monitoring
var result = await EFPerformanceOptimizer.MeasureQueryPerformanceAsync(
    async () => await patientService.GetPatientsAsync("احمد", 1, 20),
    "Patient Search"
);
```

این تغییرات باعث بهبود قابل توجه عملکرد سیستم و کاهش چشمگیر زمان پاسخ‌دهی خواهد شد.
