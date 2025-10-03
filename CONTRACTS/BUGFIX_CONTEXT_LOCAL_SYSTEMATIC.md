# 🐛 **رفع سیستماتیک مشکل Context.Local - چرا SharedServices و ServiceComponents ایجاد نشده‌اند؟**

**تاریخ:** ۲ اکتبر ۲۰۲۵  
**شدت:** 🔴 **Critical**  
**وضعیت:** ✅ **رفع شد**

---

## 🔍 **تشخیص سیستماتیک مشکل:**

### **فلوچارت مشکل:**

```
SystemSeedService.SeedAllDataAsync():

مرحله 3: SeedSampleServicesAsync()
          → AddRange(services) → Context.Services.Local ✅
          → services در Memory هستند

مرحله 4: SeedServiceComponentsAsync()
          → var services = await _context.Services.ToListAsync() ❌
          → می‌خواهد از DB بخواند!
          → ولی Services هنوز در DB نیستند (uncommitted)!
          → services = [] ❌
          → return (هیچ ServiceComponent ایجاد نمی‌شود)

مرحله 5: SeedSharedServicesAsync()
          → var services = await _context.Services.ToListAsync() ❌
          → می‌خواهد از DB بخواند!
          → ولی Services هنوز در DB نیستند (uncommitted)!
          → services = [] ❌
          → return (هیچ SharedService ایجاد نمی‌شود)

مرحله 6: SaveChanges() ✅
          → Services در DB ذخیره می‌شوند

مرحله 7: Validation
          → localSharedServicesCount = 0 ❌
          → localServiceComponentsCount = 0 ❌
          → localIsValid = false ❌
```

---

## ❌ **علت دقیق مشکل:**

### **1. Transaction Isolation:**

```csharp
// مرحله 3:
_context.Services.AddRange(services); // → Context.Local
// Services در Memory هستند، نه در DB

// مرحله 4:
var services = await _context.Services.ToListAsync(); // ❌
// این Query به DB می‌زند
// ولی Services هنوز uncommitted هستند!
// نتیجه: services = []

// مرحله 6:
await _context.SaveChangesAsync(); // حالا Services در DB هستند
```

### **2. Context.Local vs Database Query:**

```csharp
// ❌ مشکل:
var services = await _context.Services.ToListAsync();
// Query به DB → فقط committed data

// ✅ راه‌حل:
var services = _context.Services.Local.ToList();
// از Memory → شامل uncommitted entities
```

---

## ✅ **راه‌حل سیستماتیک:**

### **1. SeedServiceComponentsAsync (اصلاح شده):**

```csharp
public async Task SeedServiceComponentsAsync()
{
    try
    {
        _logger.Information("شروع ایجاد اجزای خدمات");

        // ✅ استفاده از Context.Local
        var services = _context.Services.Local
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToList();

        _logger.Information($"تعداد خدمات در Context.Local: {services.Count}");

        // ✅ Fallback به DB اگر Local خالی بود
        if (!services.Any())
        {
            _logger.Information("⚠️ Context.Local خالی است - بررسی دیتابیس...");
            services = await _context.Services
                .Where(s => !s.IsDeleted && s.IsActive)
                .ToListAsync();
            _logger.Information($"تعداد خدمات در دیتابیس: {services.Count}");
        }

        if (!services.Any())
        {
            _logger.Warning("هیچ خدمتی یافت نشد. ابتدا خدمات را ایجاد کنید.");
            return;
        }

        // ادامه کد...
    }
}
```

### **2. SeedSharedServicesAsync (اصلاح شده):**

```csharp
public async Task SeedSharedServicesAsync()
{
    try
    {
        _logger.Information("شروع ایجاد خدمات مشترک");

        // ✅ استفاده از Context.Local
        var services = _context.Services.Local
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToList();

        _logger.Information($"تعداد خدمات در Context.Local: {services.Count}");

        // ✅ Fallback به DB اگر Local خالی بود
        if (!services.Any())
        {
            _logger.Information("⚠️ Context.Local خالی است - بررسی دیتابیس...");
            services = await _context.Services
                .Where(s => !s.IsDeleted && s.IsActive)
                .ToListAsync();
            _logger.Information($"تعداد خدمات در دیتابیس: {services.Count}");
        }

        // ادامه کد...
    }
}
```

---

## 📊 **مقایسه قبل و بعد:**

| مرحله | قبل | بعد |
|-------|-----|-----|
| **مرحله 3** | AddRange → Context.Local ✅ | AddRange → Context.Local ✅ |
| **مرحله 4** | Query از DB ❌ (services = []) | Context.Local ✅ (services = 15) |
| **مرحله 5** | Query از DB ❌ (services = []) | Context.Local ✅ (services = 15) |
| **مرحله 6** | SaveChanges ✅ | SaveChanges ✅ |
| **مرحله 7** | Validation فیل ❌ | Validation موفق ✅ |

---

## 🎯 **فلوچارت جدید (صحیح):**

```
مرحله 3: SeedSampleServicesAsync()
          → AddRange(services) → Context.Services.Local ✅
          → 15 خدمت در Memory

مرحله 4: SeedServiceComponentsAsync()
          → services = Context.Services.Local ✅
          → 15 خدمت پیدا شد
          → ایجاد 30 ServiceComponent (فنی + حرفه‌ای)
          → AddRange(components) → Context.ServiceComponents.Local ✅

مرحله 5: SeedSharedServicesAsync()
          → services = Context.Services.Local ✅
          → 15 خدمت پیدا شد
          → ایجاد 6 SharedService (3 خدمت × 2 دپارتمان)
          → AddRange(sharedServices) → Context.SharedServices.Local ✅

مرحله 6: SaveChanges() ✅
          → همه چیز در DB ذخیره می‌شود

مرحله 7: Validation
          → localServicesCount = 15 ✅
          → localSharedServicesCount = 6 ✅
          → localServiceComponentsCount = 30 ✅
          → localIsValid = true ✅
```

---

## 🛡️ **ویژگی‌های ضدگلوله:**

### **1. دو لایه بررسی:**
```csharp
// لایه 1: Context.Local (اولویت)
var services = _context.Services.Local.ToList();

// لایه 2: Database (Fallback)
if (!services.Any())
{
    services = await _context.Services.ToListAsync();
}
```

### **2. Logging دقیق:**
```csharp
_logger.Information($"تعداد خدمات در Context.Local: {services.Count}");
_logger.Information("⚠️ Context.Local خالی است - بررسی دیتابیس...");
_logger.Information($"تعداد خدمات در دیتابیس: {services.Count}");
```

### **3. Fallback Mechanism:**
```
اگر Context.Local خالی بود:
→ احتمالاً قبلاً ذخیره شده‌اند
→ از DB بخوان
→ ادامه عملیات
```

---

## 🧪 **تست سناریوها:**

### **سناریو 1: Seed عادی (اولین بار)**
```
مرحله 3: Services → Context.Local ✅
مرحله 4: Context.Local → 15 خدمت ✅
مرحله 5: Context.Local → 15 خدمت ✅
نتیجه: موفق ✅
```

### **سناریو 2: Seed مجدد (داده‌ها موجود)**
```
مرحله 3: Services → فیلتر می‌شود (duplicate) → Context.Local خالی
مرحله 4: Context.Local خالی → Fallback به DB → 15 خدمت ✅
مرحله 5: Context.Local خالی → Fallback به DB → 15 خدمت ✅
نتیجه: موفق ✅
```

### **سناریو 3: هیچ Service وجود ندارد**
```
مرحله 3: Services → Context.Local خالی
مرحله 4: Context.Local خالی → DB خالی → return ✅
مرحله 5: Context.Local خالی → DB خالی → return ✅
نتیجه: منطقی ✅
```

---

## 📝 **لاگ‌های مورد انتظار:**

### **موفق (Context.Local):**
```
شروع ایجاد اجزای خدمات
تعداد خدمات در Context.Local: 15
پردازش خدمت: ویزیت پزشک عمومی (کد: 970000)
ضریب فنی برای 970000: 0.5
ضریب حرفه‌ای برای 970000: 1.3
...
تعداد اجزای خدمات ایجاد شده: 30
```

### **موفق (Fallback):**
```
شروع ایجاد اجزای خدمات
تعداد خدمات در Context.Local: 0
⚠️ Context.Local خالی است - بررسی دیتابیس...
تعداد خدمات در دیتابیس: 15
پردازش خدمت: ویزیت پزشک عمومی (کد: 970000)
...
```

### **فیل:**
```
شروع ایجاد اجزای خدمات
تعداد خدمات در Context.Local: 0
⚠️ Context.Local خالی است - بررسی دیتابیس...
تعداد خدمات در دیتابیس: 0
هیچ خدمتی یافت نشد. ابتدا خدمات را ایجاد کنید.
```

---

## 🎓 **نکات آموزشی:**

### **Context.Local چگونه کار می‌کند؟**

```csharp
// AddRange:
_context.Services.AddRange(services);
// services به Context.Services.Local اضافه می‌شوند

// دسترسی:
var local = _context.Services.Local; // IEnumerable<Service>
// شامل: Added, Modified, Unchanged entities

// Query:
var active = _context.Services.Local
    .Where(s => s.IsActive)
    .ToList(); // بدون Query به DB
```

### **Transaction Isolation:**

```sql
-- SQL Server Default: READ COMMITTED
BEGIN TRANSACTION
  INSERT INTO Services (...) -- uncommitted
  -- در همین Connection:
  SELECT * FROM Services -- می‌بیند ✅
  -- در Connection جدید:
  SELECT * FROM Services -- نمی‌بیند ❌
COMMIT
```

### **Entity Framework Context:**

```csharp
// SaveChanges:
await _context.SaveChangesAsync();
// فقط changes را به DB می‌فرستد
// Transaction را Commit نمی‌کند!

// Query بعد از SaveChanges:
var count = await _context.Services.CountAsync();
// ممکن است uncommitted data را نبیند!
```

---

## 📚 **Best Practices:**

### **✅ DO:**
```csharp
// 1. استفاده از Context.Local در Transaction
var entities = _context.Entities.Local.Where(...).ToList();

// 2. Fallback به DB
if (!entities.Any())
{
    entities = await _context.Entities.ToListAsync();
}

// 3. Logging دقیق
_logger.Information("Context.Local: {Count}", localCount);
_logger.Information("Database: {Count}", dbCount);
```

### **❌ DON'T:**
```csharp
// 1. Query از DB قبل از SaveChanges
var entities = await _context.Entities.ToListAsync(); // ❌

// 2. بدون Fallback
if (localEntities.Count == 0) return; // ❌

// 3. بدون Logging
var count = _context.Entities.Local.Count(); // ❌ چرا؟
```

---

## 🔄 **مراحل Debug:**

### **1. بررسی Context.Local:**
```csharp
var localCount = _context.Services.Local.Count();
_logger.Information("Context.Services.Local.Count: {Count}", localCount);
```

### **2. بررسی Database:**
```csharp
var dbCount = await _context.Services.CountAsync();
_logger.Information("Database Services.Count: {Count}", dbCount);
```

### **3. بررسی Transaction:**
```csharp
var transaction = _context.Database.CurrentTransaction;
_logger.Information("Transaction: {Transaction}", transaction?.TransactionId);
```

---

## 🎯 **خلاصه:**

| مشکل | علت | راه‌حل |
|------|-----|-------|
| **ServiceComponents = 0** | Query از DB قبل از SaveChanges | Context.Local |
| **SharedServices = 0** | Query از DB قبل از SaveChanges | Context.Local |
| **Validation فیل** | Entities در Memory نیستند | Context.Local + Fallback |

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** ۲ اکتبر ۲۰۲۵  
**نسخه:** 1.0


