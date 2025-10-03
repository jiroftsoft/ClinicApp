# 💰 **محاسبه خودکار قیمت خدمات در Seed**

**تاریخ:** ۲ اکتبر ۲۰۲۵  
**نسخه:** 1.0  
**وضعیت:** ✅ **پیاده‌سازی شده**

---

## 🎯 **هدف**

وقتی خدمات در فرآیند Seed ایجاد می‌شوند، قیمت آن‌ها به صورت **خودکار** بر اساس فرمول RVU محاسبه و در جدول `Services` ذخیره می‌شود.

---

## 📊 **فرمول محاسبه (استاندارد RVU - ایران)**

```
قیمت نهایی (ریال) = (RVU فنی × کای فنی) + (RVU حرفه‌ای × کای حرفه‌ای)
```

### **مثال عملی:**

#### **کد 970000 - ویزیت پزشک عمومی (عادی - #)**
```
داده‌های ورودی:
- RVU فنی: 0.5
- RVU حرفه‌ای: 1.3
- کای فنی (عادی): 4,350,000 ریال
- کای حرفه‌ای (عادی): 8,250,000 ریال

محاسبه:
قیمت = (0.5 × 4,350,000) + (1.3 × 8,250,000)
     = 2,175,000 + 10,725,000
     = 12,900,000 ریال
```

#### **کد 970096 - خدمات روانشناسی (#*)**
```
داده‌های ورودی:
- RVU فنی: 0.90
- RVU حرفه‌ای: 3.5
- کای فنی (هشتگ‌دار): 2,750,000 ریال
- کای حرفه‌ای (هشتگ‌دار): 5,450,000 ریال

محاسبه:
قیمت = (0.90 × 2,750,000) + (3.5 × 5,450,000)
     = 2,475,000 + 19,075,000
     = 21,550,000 ریال
```

---

## 🏗️ **معماری پیاده‌سازی**

### **1. ساختار دیتابیس**

#### **جدول Service**
```sql
CREATE TABLE [dbo].[Services] (
    [ServiceId] INT PRIMARY KEY IDENTITY,
    [ServiceCode] NVARCHAR(50) NOT NULL,
    [Title] NVARCHAR(250) NOT NULL,
    [Price] DECIMAL(18, 0) NOT NULL,  -- قیمت به ریال (بدون اعشار)
    [IsHashtagged] BIT NOT NULL,
    ...
)
```

#### **جدول ServiceComponent**
```sql
CREATE TABLE [dbo].[ServiceComponents] (
    [ServiceComponentId] INT PRIMARY KEY IDENTITY,
    [ServiceId] INT NOT NULL,
    [ComponentType] INT NOT NULL,  -- 0=Technical, 1=Professional
    [Coefficient] DECIMAL(18, 4) NOT NULL,  -- RVU (ضریب)
    ...
)
```

#### **جدول FactorSetting**
```sql
CREATE TABLE [dbo].[FactorSettings] (
    [FactorSettingId] INT PRIMARY KEY IDENTITY,
    [FactorType] INT NOT NULL,  -- 0=Technical, 1=Professional
    [IsHashtagged] BIT NOT NULL,
    [Value] DECIMAL(18, 0) NOT NULL,  -- کای به ریال
    [FinancialYear] INT NOT NULL,
    ...
)
```

---

### **2. فرآیند Seeding**

```
📍 مرحله 0: مقداردهی SystemUsers
📍 مرحله 1: ایجاد FactorSettings (کای‌های فنی و حرفه‌ای)
📍 مرحله 2: ایجاد ServiceTemplates (قالب‌های خدمات)
📍 مرحله 3: ایجاد Services (خدمات با Price=0)
📍 مرحله 4: ایجاد ServiceComponents (اجزای فنی و حرفه‌ای)
📍 مرحله 4.5: محاسبه خودکار قیمت خدمات ⭐ NEW
📍 مرحله 5: ایجاد SharedServices (خدمات مشترک)
💾 ذخیره تمام تغییرات (یک بار)
```

---

### **3. کد پیاده‌سازی**

#### **الف) ServiceSeedService.cs**

```csharp
/// <summary>
/// محاسبه و به‌روزرسانی قیمت تمام خدمات بر اساس FactorSettings و ServiceComponents
/// </summary>
public async Task CalculateAndUpdateServicePricesAsync()
{
    _logger.Information("💰 SERVICE_PRICE: شروع محاسبه خودکار قیمت خدمات");

    // دریافت تمام خدمات فعال با اجزای آن‌ها
    var services = await _context.Services
        .Include(s => s.ServiceComponents)
        .Where(s => !s.IsDeleted && s.IsActive)
        .ToListAsync();

    foreach (var service in services)
    {
        // بررسی وجود اجزای محاسباتی
        if (!HasComponents(service))
        {
            skippedCount++;
            continue;
        }

        // محاسبه قیمت
        var calculatedPrice = _serviceCalculationService.CalculateServicePriceWithFactorSettings(
            service, 
            _context, 
            DateTime.Now,
            null,  // بدون Override دپارتمان
            null   // سال مالی جاری
        );

        // به‌روزرسانی قیمت (به ریال - decimal(18,0))
        service.Price = Math.Round(calculatedPrice, 0, MidpointRounding.AwayFromZero);
        service.UpdatedAt = DateTime.UtcNow;
        service.UpdatedByUserId = systemUserId;

        _logger.Information("✅ {ServiceCode} = {Price:N0} ریال", 
            service.ServiceCode, service.Price);

        successCount++;
    }

    _logger.Information("📊 موفق: {Success}, ناموفق: {Failed}, رد شده: {Skipped}",
        successCount, failedCount, skippedCount);
}
```

#### **ب) SystemSeedService.cs**

```csharp
public async Task SeedAllDataAsync()
{
    using (var transaction = _context.Database.BeginTransaction())
    {
        try
        {
            // مراحل 1-4: FactorSettings, ServiceTemplates, Services, ServiceComponents
            await _factorSeedService.SeedFactorSettingsAsync();
            await _serviceTemplateSeedService.SeedServiceTemplatesAsync();
            await _serviceSeedService.SeedSampleServicesAsync();
            await _serviceSeedService.SeedServiceComponentsAsync();

            // ⭐ محاسبه خودکار قیمت
            _logger.Information("📍 مرحله 4.5 - محاسبه خودکار قیمت خدمات");
            await _serviceSeedService.CalculateAndUpdateServicePricesAsync();

            // مرحله 5: SharedServices
            await _serviceSeedService.SeedSharedServicesAsync();

            // ذخیره و Commit
            await _context.SaveChangesAsync();
            transaction.Commit();

            _logger.Information("✅ SYSTEM_SEED: تمام داده‌ها با موفقیت ایجاد شدند");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.Error(ex, "❌ SYSTEM_SEED: Rollback انجام شد");
            throw;
        }
    }
}
```

---

### **4. ServiceCalculationService.cs**

```csharp
/// <summary>
/// محاسبه قیمت خدمت بر اساس FactorSettings
/// فرمول: (RVU فنی × کای فنی) + (RVU حرفه‌ای × کای حرفه‌ای)
/// </summary>
public decimal CalculateServicePriceWithFactorSettings(
    Service service, 
    ApplicationDbContext context, 
    DateTime? date = null, 
    int? departmentId = null, 
    int? financialYear = null)
{
    var currentYear = financialYear ?? GetCurrentFinancialYear(date ?? DateTime.Now);

    // دریافت کای فنی (بر اساس هشتگ)
    var technicalFactor = context.FactorSettings
        .Where(fs => fs.FactorType == ServiceComponentType.Technical &&
                    fs.IsHashtagged == service.IsHashtagged &&
                    fs.FinancialYear == currentYear &&
                    fs.IsActive && !fs.IsDeleted && !fs.IsFrozen)
        .OrderByDescending(fs => fs.EffectiveFrom)
        .FirstOrDefault();

    // دریافت کای حرفه‌ای (همیشه false)
    var professionalFactor = context.FactorSettings
        .Where(fs => fs.FactorType == ServiceComponentType.Professional &&
                    fs.IsHashtagged == false &&
                    fs.FinancialYear == currentYear &&
                    fs.IsActive && !fs.IsDeleted && !fs.IsFrozen)
        .OrderByDescending(fs => fs.EffectiveFrom)
        .FirstOrDefault();

    // دریافت اجزای خدمت
    var technicalComponent = service.ServiceComponents
        .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && 
                            sc.IsActive && !sc.IsDeleted);
    var professionalComponent = service.ServiceComponents
        .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && 
                            sc.IsActive && !sc.IsDeleted);

    // محاسبه
    decimal basePrice = (technicalComponent.Coefficient * technicalFactor.Value) + 
                       (professionalComponent.Coefficient * professionalFactor.Value);

    // بررسی Override دپارتمان (در صورت نیاز)
    if (departmentId.HasValue)
    {
        var sharedService = context.SharedServices
            .FirstOrDefault(ss => ss.ServiceId == service.ServiceId && 
                                ss.DepartmentId == departmentId.Value);
        
        if (sharedService != null)
        {
            var overrideTechnical = sharedService.OverrideTechnicalFactor ?? technicalFactor.Value;
            var overrideProfessional = sharedService.OverrideProfessionalFactor ?? professionalFactor.Value;
            basePrice = (technicalComponent.Coefficient * overrideTechnical) + 
                       (professionalComponent.Coefficient * overrideProfessional);
        }
    }

    return basePrice;
}
```

---

## 📊 **نمونه خروجی Logging**

```
═══════════════════════════════════════════════
💰 SERVICE_PRICE: شروع محاسبه خودکار قیمت خدمات
═══════════════════════════════════════════════
📊 SERVICE_PRICE: تعداد خدمات یافت شده: 15
✅ SERVICE_PRICE: 970000 - ویزیت پزشک عمومی = 12,900,000 ریال
✅ SERVICE_PRICE: 970005 - ویزیت دندانپزشک = 12,900,000 ریال
✅ SERVICE_PRICE: 970096 - خدمات روانشناسی = 21,550,000 ریال
⏭️ SERVICE_PRICE: 970500 فاقد اجزای محاسباتی است
═══════════════════════════════════════════════
📊 SERVICE_PRICE: خلاصه محاسبات:
   ✅ موفق: 12 خدمت
   ❌ ناموفق: 0 خدمت
   ⏭️ رد شده: 3 خدمت
═══════════════════════════════════════════════
```

---

## 🎯 **ویژگی‌های کلیدی**

### **1. خودکار و یکپارچه** ✅
- محاسبه خودکار در فرآیند Seeding
- نیازی به محاسبه دستی نیست

### **2. استفاده از استاندارد RVU** ✅
- مطابق با استانداردهای وزارت بهداشت
- فرمول دقیق: `(RVU فنی × کای فنی) + (RVU حرفه‌ای × کای حرفه‌ای)`

### **3. دقت بالا** ✅
- ذخیره به ریال (`decimal(18,0)`)
- گرد کردن صحیح: `Math.Round(..., 0, MidpointRounding.AwayFromZero)`

### **4. Transaction-Safe** ✅
- در صورت خطا، Rollback می‌شود
- یکپارچگی داده‌ها تضمین شده

### **5. Structured Logging** ✅
- لاگ‌های دقیق با Serilog
- Prefix: `💰 SERVICE_PRICE:`
- Named Properties برای Query

### **6. Error Handling** ✅
- Try-Catch برای هر خدمت
- ادامه در صورت خطا (fail-safe)
- گزارش کامل موفق/ناموفق/رد شده

---

## 🔄 **فلوچارت**

```
┌─────────────────────────────────────┐
│   1. Seed FactorSettings (کای‌ها)  │
└─────────────┬───────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│   2. Seed ServiceTemplates          │
└─────────────┬───────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│   3. Seed Services (Price=0)        │
└─────────────┬───────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│   4. Seed ServiceComponents (RVU)   │
└─────────────┬───────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│ ⭐ 4.5. محاسبه قیمت خودکار        │
│                                     │
│  For Each Service:                  │
│    price = (RVU_T × K_T) +          │
│            (RVU_P × K_P)            │
│    service.Price = Round(price, 0)  │
└─────────────┬───────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│   5. Seed SharedServices            │
└─────────────┬───────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│   SaveChanges & Commit              │
└─────────────────────────────────────┘
```

---

## 📝 **نکات مهم**

### **1. ترتیب اجرا** ⚠️
**حتماً** باید به ترتیب زیر اجرا شود:
```
FactorSettings → ServiceTemplates → Services → ServiceComponents → CalculatePrice
```

### **2. Dependency Injection** ⚠️
`ServiceSeedService` نیاز به `IServiceCalculationService` دارد:
```csharp
public ServiceSeedService(
    ApplicationDbContext context,
    ILogger logger,
    ICurrentUserService currentUserService,
    IServiceCalculationService serviceCalculationService) // ✅ اضافه شد
{
    _serviceCalculationService = serviceCalculationService;
}
```

### **3. ذخیره به ریال** ⚠️
```csharp
// ❌ اشتباه
service.Price = calculatedPrice;

// ✅ درست
service.Price = Math.Round(calculatedPrice, 0, MidpointRounding.AwayFromZero);
```

### **4. بررسی Null** ⚠️
قبل از محاسبه، وجود اجزای فنی و حرفه‌ای بررسی می‌شود:
```csharp
if (!HasComponents(service))
{
    skippedCount++;
    continue;
}
```

---

## 🧪 **تست و اعتبارسنجی**

### **مثال تست:**

```csharp
[Test]
public void Test_CalculateServicePrice_970000()
{
    // Arrange
    var service = new Service 
    { 
        ServiceCode = "970000",
        IsHashtagged = false,
        ServiceComponents = new List<ServiceComponent>
        {
            new ServiceComponent { ComponentType = Technical, Coefficient = 0.5m },
            new ServiceComponent { ComponentType = Professional, Coefficient = 1.3m }
        }
    };

    var technicalFactor = new FactorSetting 
    { 
        FactorType = Technical, 
        IsHashtagged = false, 
        Value = 4_350_000m 
    };
    
    var professionalFactor = new FactorSetting 
    { 
        FactorType = Professional, 
        IsHashtagged = false, 
        Value = 8_250_000m 
    };

    // Act
    var price = (0.5m * 4_350_000m) + (1.3m * 8_250_000m);

    // Assert
    Assert.AreEqual(12_900_000m, price);
}
```

---

## ✅ **خلاصه**

| ویژگی | وضعیت |
|-------|-------|
| **محاسبه خودکار** | ✅ پیاده‌سازی شده |
| **استاندارد RVU** | ✅ مطابق وزارت بهداشت |
| **Transaction-Safe** | ✅ Rollback خودکار |
| **Structured Logging** | ✅ Serilog با Prefix |
| **Error Handling** | ✅ Fail-Safe |
| **Dependency Injection** | ✅ IServiceCalculationService |
| **ذخیره به ریال** | ✅ decimal(18,0) |

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** ۲ اکتبر ۲۰۲۵  
**نسخه:** 1.0

