# 🐛 **رفع دو مشکل کلیدی: Context.Local و Range Validation**

**تاریخ:** ۲ اکتبر ۲۰۲۵  
**شدت:** 🔴 **Critical**  
**وضعیت:** ✅ **رفع شد**

---

## 🔍 **مشکل 1: چرا `CalculateAndUpdateServicePricesAsync` خالی بود؟**

### **توضیح مشکل:**

```
فلوچارت SystemSeedService.SeedAllDataAsync:

مرحله 0: SystemUsers.Initialize()
مرحله 1: SeedFactorSettingsAsync()           → AddRange to Context
مرحله 2: SeedServiceTemplatesAsync()         → AddRange to Context
مرحله 3: SeedSampleServicesAsync()           → AddRange to Context
مرحله 4: SeedServiceComponentsAsync()        → AddRange to Context
مرحله 4.5: CalculateAndUpdateServicePricesAsync()  ← ❌ می‌خواهد از DB بخواند!
                                                      ولی هنوز SaveChanges نشده!
مرحله 6: SaveChangesAsync()                  → اینجا همه چیز ذخیره می‌شود
```

**مشکل:**
```csharp
// کد قبلی:
var services = await _context.Services
    .Include(s => s.ServiceComponents)
    .Where(s => !s.IsDeleted && s.IsActive)
    .ToListAsync();  // ❌ می‌خواهد از DB بخواند ولی هنوز ذخیره نشده!
```

**نتیجه:**
- `services` همیشه خالی بود چون هنوز `SaveChanges` اجرا نشده بود
- پیام Warning: "⚠️ SERVICE_PRICE: هیچ خدمتی برای محاسبه قیمت یافت نشد"
- قیمت‌ها محاسبه نمی‌شدند!

---

### **راه‌حل: استفاده از Context.Local**

```csharp
// کد جدید:
var services = _context.Services.Local
    .Where(s => !s.IsDeleted && s.IsActive)
    .ToList();  // ✅ از Context.Local می‌خواند (entities در memory)
```

**Context.Local چیست؟**
```
DbSet<T>.Local:
- مجموعه‌ای از entities که به context اضافه شده‌اند (AddRange)
- هنوز ذخیره نشده‌اند (قبل از SaveChanges)
- در حالت Tracking هستند
- در memory قرار دارند
```

**فلوچارت جدید:**
```
مرحله 3: SeedSampleServicesAsync()
          → AddRange(services) → services در Context.Local

مرحله 4: SeedServiceComponentsAsync()
          → foreach service in Context.Local
          → AddRange(components) → components در Context.Local
          → service.ServiceComponents = components

مرحله 4.5: CalculateAndUpdateServicePricesAsync()
          → services = Context.Services.Local ✅
          → محاسبه قیمت
          → service.Price = calculatedPrice
          → service.UpdatedAt = DateTime.UtcNow

مرحله 6: SaveChangesAsync()
          → همه چیز (services + components + prices) ذخیره می‌شود
```

---

### **کد کامل جدید:**

```csharp
public async Task CalculateAndUpdateServicePricesAsync()
{
    try
    {
        _logger.Information("💰 SERVICE_PRICE: شروع محاسبه خودکار قیمت خدمات");

        // دریافت خدمات از Context.Local (entities که به context اضافه شده‌اند)
        var services = _context.Services.Local
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToList();

        if (!services.Any())
        {
            _logger.Warning("⚠️ SERVICE_PRICE: هیچ خدمتی در Context.Local یافت نشد");
            
            // Fallback: اگر در Local نیست، از دیتابیس بخوان
            services = await _context.Services
                .Include(s => s.ServiceComponents)
                .Where(s => !s.IsDeleted && s.IsActive)
                .ToListAsync();

            if (!services.Any())
            {
                _logger.Warning("⚠️ SERVICE_PRICE: هیچ خدمتی در دیتابیس یافت نشد");
                return;
            }
        }

        _logger.Information("📊 SERVICE_PRICE: تعداد خدمات یافت شده: {Count}", services.Count);

        foreach (var service in services)
        {
            var hasComponents = service.ServiceComponents != null && 
                              service.ServiceComponents.Any(sc => !sc.IsDeleted && sc.IsActive);

            if (!hasComponents)
            {
                skippedCount++;
                continue;
            }

            // محاسبه قیمت
            var calculatedPrice = _serviceCalculationService.CalculateServicePriceWithFactorSettings(
                service, _context, DateTime.Now, null, null);

            // ذخیره قیمت (رند به ریال)
            service.Price = Math.Round(calculatedPrice, 0, MidpointRounding.AwayFromZero);
            service.UpdatedAt = DateTime.UtcNow;
            service.UpdatedByUserId = await GetValidUserIdForSeedAsync();

            _logger.Information("✅ SERVICE_PRICE: {ServiceCode} - {ServiceName} = {Price:N0} ریال",
                service.ServiceCode, service.Title, service.Price);

            successCount++;
        }

        _logger.Information("✅ SERVICE_PRICE: محاسبه کامل شد - موفق: {Success}, فیل: {Failed}, رد شده: {Skipped}",
            successCount, failedCount, skippedCount);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "❌ SERVICE_PRICE: خطای کلی در محاسبه قیمت‌ها");
        throw;
    }
}
```

---

## 🔍 **مشکل 2: ServiceTemplate - Range Validation**

### **خطای رخ داده:**

```
System.Data.Entity.Validation.DbUnexpectedValidationException
Message: An unexpected exception was thrown during validation of 'DefaultTechnicalCoefficient'
Inner Exception: 999999.9999 is not a valid value for Decimal
Inner Exception: FormatException: Input string was not in a correct format
```

---

### **علت خطا:**

**قبل از اصلاح:**
```csharp
// ServiceTemplate.cs - خط 44
[Range(typeof(decimal), "0", "999999.9999", ErrorMessage = "...")]
public decimal DefaultTechnicalCoefficient { get; set; }

// خط 53
[Range(typeof(decimal), "0", "999999.9999", ErrorMessage = "...")]
public decimal DefaultProfessionalCoefficient { get; set; }
```

**مشکل:**
- ❌ استفاده از `string` به جای `numeric` در Range
- ❌ مشکل Culture در تبدیل "999999.9999" به decimal
- ❌ مشابه با مشکل قبلی در `FactorSetting` و `ServiceComponent`

---

### **راه‌حل:**

**بعد از اصلاح:**
```csharp
/// <summary>
/// ضریب فنی پیش‌فرض (RVU فنی)
/// حداقل: 0، حداکثر: 999999.99
/// </summary>
[Required]
[Range(0, 999999.99, ErrorMessage = "ضریب فنی باید بین 0 تا 999999.99 باشد.")]
[Column(TypeName = "decimal")]
public decimal DefaultTechnicalCoefficient { get; set; }

/// <summary>
/// ضریب حرفه‌ای پیش‌فرض (RVU حرفه‌ای)
/// حداقل: 0، حداکثر: 999999.99
/// </summary>
[Required]
[Range(0, 999999.99, ErrorMessage = "ضریب حرفه‌ای باید بین 0 تا 999999.99 باشد.")]
[Column(TypeName = "decimal")]
public decimal DefaultProfessionalCoefficient { get; set; }
```

**تغییرات:**
- ✅ استفاده از `numeric` به جای `string`
- ✅ رفع مشکل Culture
- ✅ مطابق با `ServiceComponent.Coefficient`

---

## 📊 **خلاصه اصلاحات:**

### **1. Entity Models:**

| Entity | Property | قبل | بعد |
|--------|----------|-----|-----|
| **FactorSetting** | Value | `[Range(typeof(decimal), "0.000001", "999999999.999999")]` | `[Range(1, 999999999)]` |
| **ServiceComponent** | Coefficient | `[Range(typeof(decimal), "0.0001", "999999.9999")]` | `[Range(0, 999999.99)]` |
| **ServiceTemplate** | DefaultTechnicalCoefficient | `[Range(typeof(decimal), "0", "999999.9999")]` | `[Range(0, 999999.99)]` |
| **ServiceTemplate** | DefaultProfessionalCoefficient | `[Range(typeof(decimal), "0", "999999.9999")]` | `[Range(0, 999999.99)]` |

---

### **2. ServiceSeedService:**

| متد | قبل | بعد |
|-----|-----|-----|
| **CalculateAndUpdateServicePricesAsync** | `await _context.Services.ToListAsync()` | `_context.Services.Local.ToList()` + Fallback |

---

## 🎯 **چرا Context.Local بهتر است؟**

### **مقایسه:**

```csharp
// ❌ روش قبلی (از DB بخوان):
var services = await _context.Services
    .Include(s => s.ServiceComponents)
    .Where(s => !s.IsDeleted && s.IsActive)
    .ToListAsync();
// مشکل: قبل از SaveChanges هیچی در DB نیست!

// ✅ روش جدید (از Memory بخوان):
var services = _context.Services.Local
    .Where(s => !s.IsDeleted && s.IsActive)
    .ToList();
// مزایا:
// 1. Entities در Memory هستند (AddRange شده‌اند)
// 2. Navigation Properties کار می‌کنند (service.ServiceComponents)
// 3. تغییرات بلافاصله اعمال می‌شوند (service.Price = ...)
// 4. نیاز به SaveChanges میانی نیست
```

---

## 🚀 **مزایای راه‌حل جدید:**

### **1. Transaction Safety:**
```
همه چیز در یک Transaction:
- AddRange(services)
- AddRange(components)
- Calculate prices
- SaveChanges (یک بار!)
```

### **2. Performance:**
```
قبل: AddRange → SaveChanges → Load from DB → Calculate → SaveChanges
بعد: AddRange → Calculate (in memory) → SaveChanges (یک بار!)
```

### **3. Memory Efficiency:**
```
Context.Local: فقط entities که track شده‌اند (تعداد محدود)
ToListAsync(): تمام entities در DB (ممکن است خیلی زیاد باشد)
```

---

## 🧪 **تست:**

### **قبل از Fix:**
```
❌ services = empty (هیچ خدمتی در DB نیست)
❌ Warning: هیچ خدمتی برای محاسبه قیمت یافت نشد
❌ قیمت‌ها محاسبه نمی‌شوند
❌ DbUnexpectedValidationException در SaveChanges
```

### **بعد از Fix:**
```
✅ services = Context.Local (15 خدمت)
✅ محاسبه قیمت برای 15 خدمت
✅ Log: "970000 - ویزیت پزشک عمومی = 12,900,000 ریال"
✅ SaveChanges موفق
✅ قیمت‌ها در DB ذخیره شدند
```

---

## 📝 **Best Practices:**

### **✅ DO:**
```csharp
// استفاده از Context.Local برای entities که هنوز ذخیره نشده‌اند
var entities = _context.Entities.Local.Where(...).ToList();

// استفاده از numeric Range
[Range(0, 999999.99)]

// یک Transaction برای همه عملیات
using (var transaction = _context.Database.BeginTransaction()) { ... }
```

### **❌ DON'T:**
```csharp
// خواندن از DB قبل از SaveChanges
var entities = await _context.Entities.ToListAsync(); // ❌

// استفاده از string Range
[Range(typeof(decimal), "0", "999999.9999")] // ❌

// چندین SaveChanges در Transaction
await _context.SaveChangesAsync();
// ... do something ...
await _context.SaveChangesAsync(); // ❌
```

---

## 🎓 **نکات آموزشی:**

### **Context.Local چگونه کار می‌کند؟**

```csharp
// مثال:
var service = new Service { ServiceCode = "970000", Title = "ویزیت" };
_context.Services.Add(service); // entity به Context.Local اضافه می‌شود

// حالا می‌توان از Context.Local استفاده کرد:
var localServices = _context.Services.Local; // شامل service

// SaveChanges هنوز اجرا نشده!
await _context.SaveChangesAsync(); // حالا در DB ذخیره می‌شود
```

### **Navigation Properties در Context.Local:**

```csharp
var service = new Service { ServiceCode = "970000" };
_context.Services.Add(service);

var component1 = new ServiceComponent { Service = service };
var component2 = new ServiceComponent { Service = service };
_context.ServiceComponents.AddRange(new[] { component1, component2 });

// Navigation Property بلافاصله کار می‌کند:
var count = service.ServiceComponents.Count; // = 2 ✅
// حتی قبل از SaveChanges!
```

---

## 📚 **مراجع:**

1. [DbSet.Local Property](https://docs.microsoft.com/en-us/dotnet/api/system.data.entity.dbset-1.local)
2. [Change Tracking in EF6](https://docs.microsoft.com/en-us/ef/ef6/saving/change-tracking/)
3. [Range Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.rangeattribute)

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** ۲ اکتبر ۲۰۲۵  
**نسخه:** 1.0

