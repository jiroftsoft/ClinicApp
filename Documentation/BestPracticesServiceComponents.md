# بهترین روش‌های پیاده‌سازی ServiceComponents

## 🎯 مشکل روش فعلی

### ❌ روش Hard-coded (فعلی):
```csharp
private decimal GetDefaultTechnicalCoefficient(string serviceCode)
{
    return serviceCode switch
    {
        "970000" => 0.5m,  // ویزیت پزشک عمومی
        "970005" => 0.5m,  // ویزیت دندان‌پزشک عمومی
        // ... 27 خط کد تکراری
        _ => 1.0m
    };
}
```

**مشکلات:**
- Hard-coded values
- Maintenance nightmare
- Not scalable
- Violation of DRY principle

## ✅ بهترین روش‌ها

### 1. روش Database-Driven (پیشنهاد اول)

#### الف) جدول ServiceTemplate
```sql
CREATE TABLE ServiceTemplates (
    ServiceTemplateId INT PRIMARY KEY,
    ServiceCode NVARCHAR(50) NOT NULL,
    ServiceName NVARCHAR(200) NOT NULL,
    DefaultTechnicalCoefficient DECIMAL(18,2) NOT NULL,
    DefaultProfessionalCoefficient DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);
```

#### ب) Seed Data
```sql
INSERT INTO ServiceTemplates VALUES
('970000', 'ویزیت پزشک عمومی', 0.5, 1.3, 1, GETDATE(), NULL),
('970005', 'ویزیت دندان‌پزشک عمومی', 0.5, 1.3, 1, GETDATE(), NULL),
-- ... سایر خدمات
```

#### ج) Service Implementation
```csharp
public class ServiceTemplateService
{
    public async Task<ServiceTemplate> GetTemplateByCodeAsync(string serviceCode)
    {
        return await _context.ServiceTemplates
            .FirstOrDefaultAsync(st => st.ServiceCode == serviceCode && st.IsActive);
    }
}

// در ServiceSeedService
private async Task<decimal> GetDefaultTechnicalCoefficientAsync(string serviceCode)
{
    var template = await _serviceTemplateService.GetTemplateByCodeAsync(serviceCode);
    return template?.DefaultTechnicalCoefficient ?? 1.0m;
}
```

### 2. روش Configuration-Based (پیشنهاد دوم)

#### الف) appsettings.json
```json
{
  "ServiceDefaults": {
    "VisitServices": {
      "970000": {
        "TechnicalCoefficient": 0.5,
        "ProfessionalCoefficient": 1.3,
        "Name": "ویزیت پزشک عمومی"
      },
      "970005": {
        "TechnicalCoefficient": 0.5,
        "ProfessionalCoefficient": 1.3,
        "Name": "ویزیت دندان‌پزشک عمومی"
      }
    }
  }
}
```

#### ب) Configuration Service
```csharp
public class ServiceDefaultsConfiguration
{
    public Dictionary<string, ServiceDefault> VisitServices { get; set; }
}

public class ServiceDefault
{
    public decimal TechnicalCoefficient { get; set; }
    public decimal ProfessionalCoefficient { get; set; }
    public string Name { get; set; }
}

public class ServiceDefaultsService
{
    private readonly ServiceDefaultsConfiguration _config;
    
    public ServiceDefaultsService(IOptions<ServiceDefaultsConfiguration> config)
    {
        _config = config.Value;
    }
    
    public ServiceDefault GetServiceDefault(string serviceCode)
    {
        return _config.VisitServices.TryGetValue(serviceCode, out var serviceDefault) 
            ? serviceDefault 
            : new ServiceDefault { TechnicalCoefficient = 1.0m, ProfessionalCoefficient = 1.0m };
    }
}
```

### 3. روش Enum-Based (پیشنهاد سوم)

#### الف) Service Code Enum
```csharp
public enum ServiceCode
{
    [ServiceDefaults(TechnicalCoefficient = 0.5, ProfessionalCoefficient = 1.3)]
    GeneralPractitionerVisit = 970000,
    
    [ServiceDefaults(TechnicalCoefficient = 0.5, ProfessionalCoefficient = 1.3)]
    GeneralDentistVisit = 970005,
    
    [ServiceDefaults(TechnicalCoefficient = 0.7, ProfessionalCoefficient = 1.8)]
    SpecialistVisit = 970015
}

[AttributeUsage(AttributeTargets.Field)]
public class ServiceDefaultsAttribute : Attribute
{
    public decimal TechnicalCoefficient { get; set; }
    public decimal ProfessionalCoefficient { get; set; }
}
```

#### ب) Reflection Service
```csharp
public class ServiceDefaultsService
{
    public (decimal Technical, decimal Professional) GetDefaults(string serviceCode)
    {
        if (Enum.TryParse<ServiceCode>(serviceCode, out var serviceCodeEnum))
        {
            var attribute = serviceCodeEnum.GetType()
                .GetField(serviceCodeEnum.ToString())
                ?.GetCustomAttribute<ServiceDefaultsAttribute>();
                
            if (attribute != null)
            {
                return (attribute.TechnicalCoefficient, attribute.ProfessionalCoefficient);
            }
        }
        
        return (1.0m, 1.0m); // Default values
    }
}
```

## 🏆 مقایسه روش‌ها

| روش | مزایا | معایب | امتیاز |
|-----|-------|-------|--------|
| Database-Driven | ✅ Flexible, ✅ Maintainable, ✅ Scalable | ❌ Database dependency | 9/10 |
| Configuration-Based | ✅ Easy to change, ✅ No DB dependency | ❌ Requires restart | 8/10 |
| Enum-Based | ✅ Type-safe, ✅ Compile-time check | ❌ Limited flexibility | 7/10 |
| Hard-coded (فعلی) | ✅ Simple | ❌ Not maintainable | 3/10 |

## 🎯 توصیه نهایی

**روش Database-Driven** بهترین انتخاب است چون:
1. **Flexibility:** تغییرات بدون تغییر کد
2. **Maintainability:** مدیریت آسان
3. **Scalability:** پشتیبانی از خدمات جدید
4. **Audit Trail:** امکان ردیابی تغییرات
5. **Multi-tenant:** پشتیبانی از چندین کلینیک

## 📋 مراحل پیاده‌سازی

1. ایجاد جدول `ServiceTemplates`
2. ایجاد Migration
3. Seed کردن داده‌های اولیه
4. ایجاد `ServiceTemplateService`
5. به‌روزرسانی `ServiceSeedService`
6. تست و اعتبارسنجی
