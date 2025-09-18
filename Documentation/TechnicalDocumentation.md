# 🔧 مستندات فنی سیستم بیمه تکمیلی

## 🏗️ **معماری سیستم**

### **نمودار معماری کلی**
```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
├─────────────────────────────────────────────────────────────┤
│  Controllers  │  Views  │  JavaScript  │  CSS  │  HTML     │
├─────────────────────────────────────────────────────────────┤
│                    Business Logic Layer                     │
├─────────────────────────────────────────────────────────────┤
│  Services  │  Interfaces  │  ViewModels  │  Validators    │
├─────────────────────────────────────────────────────────────┤
│                    Data Access Layer                        │
├─────────────────────────────────────────────────────────────┤
│  Repositories  │  Entity Framework  │  Database Context   │
├─────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                      │
├─────────────────────────────────────────────────────────────┤
│  Cache  │  Logging  │  Monitoring  │  Security  │  Config  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🗄️ **مدل داده**

### **Entity: InsuranceTariff**
```csharp
public class InsuranceTariff : ISoftDelete, ITrackable
{
    // فیلدهای اصلی
    public int InsuranceTariffId { get; set; }
    public int InsurancePlanId { get; set; }
    public int ServiceId { get; set; }
    
    // فیلدهای بیمه تکمیلی
    public InsuranceType InsuranceType { get; set; }
    public string SupplementarySettings { get; set; }
    public decimal? SupplementaryCoveragePercent { get; set; }
    public decimal? SupplementaryMaxPayment { get; set; }
    
    // فیلدهای Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByUserId { get; set; }
    public string UpdatedByUserId { get; set; }
    public bool IsDeleted { get; set; }
}
```

### **Entity: InsuranceType (Enum)**
```csharp
public enum InsuranceType
{
    Primary = 1,        // بیمه اصلی
    Supplementary = 2   // بیمه تکمیلی
}
```

### **Entity: PatientInsurance**
```csharp
public class PatientInsurance : ISoftDelete, ITrackable
{
    public int PatientInsuranceId { get; set; }
    public int PatientId { get; set; }
    public int InsurancePlanId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Navigation Properties
    public virtual Patient Patient { get; set; }
    public virtual InsurancePlan InsurancePlan { get; set; }
}
```

---

## 🔧 **سرویس‌های اصلی**

### **SupplementaryInsuranceService**

#### **Dependency Injection**
```csharp
public SupplementaryInsuranceService(
    IPatientInsuranceRepository patientInsuranceRepository,
    IInsuranceTariffRepository tariffRepository,
    IInsurancePlanRepository planRepository,
    IServiceRepository serviceRepository,
    ISupplementaryInsuranceCacheService cacheService,
    ISupplementaryInsuranceMonitoringService monitoringService,
    ICurrentUserService currentUserService,
    ILogger logger)
```

#### **متدهای اصلی**

##### **CalculateSupplementaryInsuranceAsync**
```csharp
public async Task<ServiceResult<SupplementaryCalculationResult>> CalculateSupplementaryInsuranceAsync(
    int patientId, 
    int serviceId, 
    decimal serviceAmount, 
    decimal primaryCoverage, 
    DateTime calculationDate)
{
    try
    {
        // 1. دریافت بیمه تکمیلی بیمار
        var supplementaryInsurance = await _patientInsuranceRepository
            .GetSupplementaryByPatientIdAsync(patientId);
        
        // 2. بررسی وجود بیمه تکمیلی فعال
        var activeSupplementary = supplementaryInsurance
            .FirstOrDefault(pi => pi.IsActive && 
                (pi.EndDate == null || pi.EndDate > calculationDate));
        
        // 3. دریافت تعرفه بیمه تکمیلی
        var tariffs = await _tariffRepository.GetByPlanIdAsync(activeSupplementary.InsurancePlanId);
        var tariff = tariffs.FirstOrDefault(t => 
            t.ServiceId == serviceId && 
            t.InsuranceType == InsuranceType.Supplementary);
        
        // 4. محاسبه مبلغ باقی‌مانده
        decimal remainingAmount = serviceAmount - primaryCoverage;
        
        // 5. محاسبه پوشش بیمه تکمیلی
        decimal supplementaryCoverage = 0;
        if (tariff.SupplementaryCoveragePercent.HasValue)
        {
            supplementaryCoverage = remainingAmount * (tariff.SupplementaryCoveragePercent.Value / 100m);
        }
        
        // 6. اعمال سقف پرداخت
        if (tariff.SupplementaryMaxPayment.HasValue && 
            supplementaryCoverage > tariff.SupplementaryMaxPayment.Value)
        {
            supplementaryCoverage = tariff.SupplementaryMaxPayment.Value;
        }
        
        // 7. محاسبه سهم نهایی بیمار
        decimal finalPatientShare = remainingAmount - supplementaryCoverage;
        if (finalPatientShare < 0) finalPatientShare = 0;
        
        // 8. ایجاد نتیجه
        var result = new SupplementaryCalculationResult
        {
            PatientId = patientId,
            ServiceId = serviceId,
            ServiceAmount = serviceAmount,
            PrimaryCoverage = primaryCoverage,
            SupplementaryCoverage = supplementaryCoverage,
            FinalPatientShare = finalPatientShare,
            TotalCoverage = primaryCoverage + supplementaryCoverage,
            CalculationDate = calculationDate,
            Notes = $"محاسبه بر اساس تعرفه: {tariff.SupplementaryCoveragePercent}%"
        };
        
        return ServiceResult<SupplementaryCalculationResult>.Successful(result);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در محاسبه بیمه تکمیلی");
        return ServiceResult<SupplementaryCalculationResult>.Failed("خطا در محاسبه بیمه تکمیلی");
    }
}
```

---

## 🗃️ **Cache Management**

### **SupplementaryInsuranceCacheService**

#### **استراتژی Cache**
```csharp
public class SupplementaryInsuranceCacheService : ISupplementaryInsuranceCacheService
{
    // Cache های درون‌حافظه‌ای
    private static readonly Dictionary<string, CachedTariffData> _tariffCache = new();
    private static readonly Dictionary<string, CachedSettingsData> _settingsCache = new();
    private static readonly Dictionary<string, CachedCalculationData> _calculationCache = new();
    
    // تنظیمات Cache
    private static readonly TimeSpan TariffCacheExpiry = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan SettingsCacheExpiry = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan CalculationCacheExpiry = TimeSpan.FromMinutes(15);
}
```

#### **متدهای Cache**

##### **GetCachedSupplementaryTariffsAsync**
```csharp
public async Task<ServiceResult<List<SupplementaryTariffViewModel>>> GetCachedSupplementaryTariffsAsync(int planId)
{
    var cacheKey = $"tariffs_{planId}";
    
    // بررسی Cache
    if (_tariffCache.ContainsKey(cacheKey) && 
        _tariffCache[cacheKey].ExpiresAt > DateTime.UtcNow)
    {
        return ServiceResult<List<SupplementaryTariffViewModel>>.Successful(
            _tariffCache[cacheKey].Data);
    }
    
    // دریافت از Database
    var tariffs = await _tariffRepository.GetByPlanIdAsync(planId);
    var supplementaryTariffs = tariffs
        .Where(t => t.InsuranceType == InsuranceType.Supplementary)
        .Select(t => new SupplementaryTariffViewModel
        {
            TariffId = t.InsuranceTariffId,
            PlanId = t.InsurancePlanId,
            ServiceId = t.ServiceId,
            ServiceName = t.Service?.Title,
            CoveragePercent = t.SupplementaryCoveragePercent ?? 0,
            MaxPayment = t.SupplementaryMaxPayment ?? 0,
            Settings = t.SupplementarySettings
        })
        .ToList();
    
    // ذخیره در Cache
    _tariffCache[cacheKey] = new CachedTariffData
    {
        Data = supplementaryTariffs,
        ExpiresAt = DateTime.UtcNow.Add(TariffCacheExpiry)
    };
    
    return ServiceResult<List<SupplementaryTariffViewModel>>.Successful(supplementaryTariffs);
}
```

---

## 📊 **Monitoring و Logging**

### **SupplementaryInsuranceMonitoringService**

#### **رویدادهای Monitoring**
```csharp
public class CalculationEvent
{
    public int PatientId { get; set; }
    public int ServiceId { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal PrimaryCoverage { get; set; }
    public decimal SupplementaryCoverage { get; set; }
    public decimal FinalPatientShare { get; set; }
    public long Duration { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime CalculationDate { get; set; }
}
```

#### **متدهای Monitoring**

##### **LogCalculationEvent**
```csharp
public void LogCalculationEvent(CalculationEvent calculationEvent)
{
    try
    {
        if (calculationEvent.Success)
        {
            _logger.Information("✅ محاسبه موفق - PatientId: {PatientId}, Duration: {Duration}ms",
                calculationEvent.PatientId, calculationEvent.Duration);
        }
        else
        {
            _logger.Error("❌ محاسبه ناموفق - PatientId: {PatientId}, Error: {Error}",
                calculationEvent.PatientId, calculationEvent.ErrorMessage);
        }
        
        // ذخیره در حافظه برای گزارش‌گیری
        _calculationEvents.Add(calculationEvent);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ثبت رویداد محاسبه");
    }
}
```

##### **GetPerformanceReport**
```csharp
public PerformanceReport GetPerformanceReport(DateTime? fromDate = null, DateTime? toDate = null)
{
    var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
    var to = toDate ?? DateTime.UtcNow;
    
    var eventsInRange = _calculationEvents
        .Where(e => e.CalculationDate >= from && e.CalculationDate <= to)
        .ToList();
    
    var report = new PerformanceReport
    {
        FromDate = from,
        ToDate = to,
        GeneratedAt = DateTime.UtcNow,
        TotalCalculations = eventsInRange.Count,
        SuccessfulCalculations = eventsInRange.Count(e => e.Success),
        FailedCalculations = eventsInRange.Count(e => !e.Success),
        AverageDuration = eventsInRange.Any() ? 
            eventsInRange.Average(e => e.Duration) : 0,
        SuccessRate = eventsInRange.Any() ? 
            (double)eventsInRange.Count(e => e.Success) / eventsInRange.Count * 100 : 0
    };
    
    return report;
}
```

---

## 🗄️ **Database Schema**

### **جدول InsuranceTariffs**
```sql
CREATE TABLE InsuranceTariffs (
    InsuranceTariffId INT IDENTITY(1,1) PRIMARY KEY,
    InsurancePlanId INT NOT NULL,
    ServiceId INT NOT NULL,
    InsuranceType INT NOT NULL, -- 1: Primary, 2: Supplementary
    SupplementarySettings NVARCHAR(2000) NULL,
    SupplementaryCoveragePercent DECIMAL(5,2) NULL,
    SupplementaryMaxPayment DECIMAL(18,2) NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    CreatedByUserId NVARCHAR(450) NOT NULL,
    UpdatedByUserId NVARCHAR(450) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_InsuranceTariffs_InsurancePlans 
        FOREIGN KEY (InsurancePlanId) REFERENCES InsurancePlans(InsurancePlanId),
    CONSTRAINT FK_InsuranceTariffs_Services 
        FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId)
);

-- Indexes
CREATE INDEX IX_InsuranceTariff_InsuranceType ON InsuranceTariffs(InsuranceType);
CREATE INDEX IX_InsuranceTariff_SupplementaryCoveragePercent ON InsuranceTariffs(SupplementaryCoveragePercent);
CREATE INDEX IX_InsuranceTariff_SupplementaryMaxPayment ON InsuranceTariffs(SupplementaryMaxPayment);
```

### **Migration Script**
```csharp
public partial class AddSupplementaryInsuranceFields : DbMigration
{
    public override void Up()
    {
        AddColumn("dbo.InsuranceTariffs", "InsuranceType", c => c.Int(nullable: false, defaultValue: 1));
        AddColumn("dbo.InsuranceTariffs", "SupplementarySettings", c => c.String(maxLength: 2000));
        AddColumn("dbo.InsuranceTariffs", "SupplementaryCoveragePercent", c => c.Decimal(precision: 5, scale: 2));
        AddColumn("dbo.InsuranceTariffs", "SupplementaryMaxPayment", c => c.Decimal(precision: 18, scale: 2));
        
        CreateIndex("dbo.InsuranceTariffs", "InsuranceType", name: "IX_InsuranceTariff_InsuranceType");
        CreateIndex("dbo.InsuranceTariffs", "SupplementaryCoveragePercent", name: "IX_InsuranceTariff_SupplementaryCoveragePercent");
        CreateIndex("dbo.InsuranceTariffs", "SupplementaryMaxPayment", name: "IX_InsuranceTariff_SupplementaryMaxPayment");
    }
    
    public override void Down()
    {
        DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_SupplementaryMaxPayment");
        DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_SupplementaryCoveragePercent");
        DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_InsuranceType");
        
        DropColumn("dbo.InsuranceTariffs", "SupplementaryMaxPayment");
        DropColumn("dbo.InsuranceTariffs", "SupplementaryCoveragePercent");
        DropColumn("dbo.InsuranceTariffs", "SupplementarySettings");
        DropColumn("dbo.InsuranceTariffs", "InsuranceType");
    }
}
```

---

## 🔒 **امنیت**

### **Authentication & Authorization**
```csharp
[Authorize(Roles = "Admin,Doctor,Reception")]
public class PatientInsuranceController : BaseController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<ActionResult> CalculateSupplementaryInsurance(SupplementaryCalculationRequest request)
    {
        // Implementation
    }
}
```

### **Input Validation**
```csharp
public class SupplementaryCalculationRequestValidator : AbstractValidator<SupplementaryCalculationRequest>
{
    public SupplementaryCalculationRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("شناسه بیمار الزامی است");
            
        RuleFor(x => x.ServiceId)
            .GreaterThan(0)
            .WithMessage("شناسه خدمت الزامی است");
            
        RuleFor(x => x.ServiceAmount)
            .GreaterThan(0)
            .WithMessage("مبلغ خدمت باید مثبت باشد");
            
        RuleFor(x => x.PrimaryCoverage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("پوشش بیمه اصلی نمی‌تواند منفی باشد");
    }
}
```

---

## ⚡ **بهینه‌سازی عملکرد**

### **Query Optimization**
```csharp
// استفاده از Include برای کاهش تعداد Query ها
var tariffs = await _context.InsuranceTariffs
    .Include(t => t.Service)
    .Include(t => t.InsurancePlan)
    .Where(t => t.InsurancePlanId == planId && 
                t.InsuranceType == InsuranceType.Supplementary)
    .ToListAsync();
```

### **Async/Await Pattern**
```csharp
public async Task<ServiceResult<SupplementaryCalculationResult>> CalculateAsync(
    int patientId, int serviceId, decimal serviceAmount, decimal primaryCoverage)
{
    // استفاده از async/await برای عملیات I/O
    var patientInsurance = await _patientInsuranceRepository
        .GetSupplementaryByPatientIdAsync(patientId);
        
    var tariffs = await _tariffRepository
        .GetByPlanIdAsync(patientInsurance.InsurancePlanId);
        
    // محاسبات CPU-intensive
    var result = await Task.Run(() => 
        PerformCalculation(patientInsurance, tariffs, serviceAmount, primaryCoverage));
        
    return result;
}
```

### **Memory Management**
```csharp
public class SupplementaryInsuranceCacheService : IDisposable
{
    private readonly Timer _cleanupTimer;
    
    public SupplementaryInsuranceCacheService()
    {
        // پاک‌سازی خودکار Cache هر 5 دقیقه
        _cleanupTimer = new Timer(CleanupExpiredCache, null, 
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }
    
    private void CleanupExpiredCache(object state)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _tariffCache
            .Where(kvp => kvp.Value.ExpiresAt <= now)
            .Select(kvp => kvp.Key)
            .ToList();
            
        foreach (var key in expiredKeys)
        {
            _tariffCache.Remove(key);
        }
    }
    
    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}
```

---

## 🧪 **Testing Strategy**

### **Unit Tests**
```csharp
[Test]
public async Task CalculateSupplementaryInsuranceAsync_WithValidData_ShouldReturnSuccessfulResult()
{
    // Arrange
    var patientId = 1;
    var serviceId = 1;
    var serviceAmount = 1000000m;
    var primaryCoverage = 500000m;
    var calculationDate = DateTime.UtcNow;
    
    var supplementaryInsurance = new List<PatientInsurance>
    {
        new PatientInsurance { PatientId = patientId, InsurancePlanId = 1, IsActive = true }
    };
    
    var tariffs = new List<InsuranceTariff>
    {
        new InsuranceTariff 
        { 
            ServiceId = serviceId, 
            InsuranceType = InsuranceType.Supplementary,
            SupplementaryCoveragePercent = 50,
            SupplementaryMaxPayment = 200000
        }
    };
    
    _mockPatientInsuranceRepository
        .Setup(x => x.GetSupplementaryByPatientIdAsync(patientId))
        .ReturnsAsync(supplementaryInsurance);
        
    _mockTariffRepository
        .Setup(x => x.GetByPlanIdAsync(1))
        .ReturnsAsync(tariffs);
    
    // Act
    var result = await _service.CalculateSupplementaryInsuranceAsync(
        patientId, serviceId, serviceAmount, primaryCoverage, calculationDate);
    
    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Data);
    Assert.Equal(250000m, result.Data.SupplementaryCoverage);
    Assert.Equal(250000m, result.Data.FinalPatientShare);
}
```

### **Integration Tests**
```csharp
[Test]
public async Task CompleteCalculationFlow_WithValidData_ShouldWorkEndToEnd()
{
    // Arrange
    SetupTestData();
    
    // Act
    var result = await _supplementaryService.CalculateSupplementaryInsuranceAsync(
        1, 1, 2000000m, 800000m, DateTime.UtcNow);
    
    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Data);
    
    // بررسی صحت محاسبات
    var remainingAmount = 2000000m - 800000m;
    var expectedSupplementaryCoverage = Math.Min(remainingAmount * 0.6m, 500000m);
    Assert.Equal(expectedSupplementaryCoverage, result.Data.SupplementaryCoverage);
}
```

---

## 📈 **Monitoring و Alerting**

### **Health Checks**
```csharp
public class SupplementaryInsuranceHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // بررسی اتصال به Database
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Database connection failed");
            }
            
            // بررسی Cache
            var cacheStats = _cacheService.GetCacheStatistics();
            if (cacheStats.CacheErrors > 10)
            {
                return HealthCheckResult.Degraded($"Cache errors: {cacheStats.CacheErrors}");
            }
            
            return HealthCheckResult.Healthy("All systems operational");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Health check failed", ex);
        }
    }
}
```

### **Performance Counters**
```csharp
public class PerformanceCounters
{
    private static readonly Counter _calculationCounter = 
        Metrics.CreateCounter("supplementary_calculations_total", "Total calculations");
        
    private static readonly Histogram _calculationDuration = 
        Metrics.CreateHistogram("supplementary_calculation_duration_seconds", "Calculation duration");
        
    private static readonly Gauge _activeCalculations = 
        Metrics.CreateGauge("supplementary_active_calculations", "Active calculations");
    
    public void RecordCalculation(bool success, double duration)
    {
        _calculationCounter.Inc(new[] { new KeyValuePair<string, string>("success", success.ToString()) });
        _calculationDuration.Observe(duration);
    }
}
```

---

## 🔄 **Deployment و Configuration**

### **Configuration Settings**
```json
{
  "SupplementaryInsurance": {
    "CacheSettings": {
      "TariffCacheExpiryMinutes": 30,
      "SettingsCacheExpiryMinutes": 60,
      "CalculationCacheExpiryMinutes": 15,
      "MaxCacheSize": 1000
    },
    "MonitoringSettings": {
      "EnableLogging": true,
      "LogLevel": "Information",
      "EnablePerformanceCounters": true,
      "AlertThresholds": {
        "ErrorRate": 5.0,
        "ResponseTime": 2000,
        "MemoryUsage": 80.0
      }
    },
    "CalculationSettings": {
      "MaxCalculationTime": 5000,
      "EnableAdvancedSettings": true,
      "DefaultCoveragePercent": 50,
      "DefaultMaxPayment": 100000
    }
  }
}
```

### **Dependency Injection Configuration**
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSupplementaryInsuranceServices(this IServiceCollection services)
    {
        services.AddScoped<ISupplementaryInsuranceService, SupplementaryInsuranceService>();
        services.AddScoped<ISupplementaryInsuranceCacheService, SupplementaryInsuranceCacheService>();
        services.AddScoped<ISupplementaryInsuranceMonitoringService, SupplementaryInsuranceMonitoringService>();
        
        services.AddSingleton<IMemoryCache, MemoryCache>();
        services.AddHealthChecks()
            .AddCheck<SupplementaryInsuranceHealthCheck>("supplementary-insurance");
            
        return services;
    }
}
```

---

**آخرین به‌روزرسانی**: 15 دی 1403  
**نسخه**: 1.0.0  
**نویسنده**: تیم توسعه سیستم‌های درمانی
