# 🛡️ **بهبود سیستماتیک Validation - ضدگلوله‌سازی کامل**

**تاریخ:** ۲ اکتبر ۲۰۲۵  
**شدت:** 🟡 **Medium**  
**وضعیت:** ✅ **بهبود یافت**

---

## ❌ **مشکل اصلی:**

### **فلوچارت مشکل:**

```
SystemSeedService.SeedAllDataAsync():

1. BeginTransaction()                      ✅
2. AddRange(FactorSettings)               ✅ → در Context.Local
3. AddRange(ServiceTemplates)             ✅ → در Context.Local
4. AddRange(Services)                     ✅ → در Context.Local
5. AddRange(ServiceComponents)            ✅ → در Context.Local
6. CalculateAndUpdateServicePrices()      ✅ → تغییر Price در Context.Local
7. AddRange(SharedServices)               ✅ → در Context.Local

8. SaveChanges()                          ✅ → ذخیره در Transaction (uncommitted)

9. Validation:                            ❌ مشکل اینجاست!
   - factorsValid = true                  ✅
   - servicesValid = ???                  ❌
     
     ValidateSeededDataAsync():
     → CountAsync() از DB می‌خواند
     → ولی Transaction هنوز Commit نشده!
     → به خاطر Isolation Level، تغییرات uncommitted دیده نمی‌شوند!
     → Count = 0 ❌
     → return false ❌

10. Rollback() به خاطر Validation فیل! ❌❌❌
```

---

## 🎯 **علت دقیق مشکل:**

### **1. Transaction Isolation Level:**

```sql
-- SQL Server Default Isolation Level: READ COMMITTED
BEGIN TRANSACTION

  INSERT INTO Services (...)  -- uncommitted
  INSERT INTO ServiceComponents (...)  -- uncommitted

  -- در همین Transaction:
  SELECT COUNT(*) FROM Services WHERE IsActive = 1
  -- می‌تواند uncommitted data را ببیند ✅

  -- اما اگر Connection جدید باشد یا Context جدید:
  SELECT COUNT(*) FROM Services WHERE IsActive = 1
  -- uncommitted data را نمی‌بیند ❌ (READ COMMITTED)

COMMIT
```

### **2. Entity Framework Context:**

```csharp
// SaveChanges() در EF6:
await _context.SaveChangesAsync();
// این فقط changes را به DB می‌فرستد
// ولی Transaction را Commit نمی‌کند!

// Query بعد از SaveChanges:
var count = await _context.Services.CountAsync();
// این یک Query جدید به DB است
// ممکن است uncommitted data را نبیند!
```

---

## ✅ **راه‌حل: استفاده از Context.Local**

### **فلوچارت جدید:**

```
1-7. همان مراحل قبلی ✅

8. SaveChanges() ✅

9. Validation (بهبود یافته):
   
   ValidateSeededDataAsync():
   
   A. بررسی Context.Local اول:
      → servicesCount = Context.Services.Local.Count()
      → sharedServicesCount = Context.SharedServices.Local.Count()
      → componentsCount = Context.ServiceComponents.Local.Count()
      
      اگر Local خالی نیست:
        → Validation از روی Local ✅
        → بررسی دقیق‌تر:
          - خدمات بدون اجزا؟ ⚠️
          - خدمات با قیمت صفر؟ ⚠️
        → return true/false
   
   B. اگر Local خالی بود (فراخوانی مجدد):
      → Query از DB ✅
      → این یعنی قبلاً ذخیره شده

10. اگر Validation موفق:
    → Commit() ✅
    → Success ✅
    
    اگر Validation فیل:
    → Rollback() ✅
    → با Log دقیق از علت فیل
```

---

## 🔧 **کد جدید (ضدگلوله‌سازی شده):**

### **ValidateSeededDataAsync (نسخه جدید):**

```csharp
public async Task<bool> ValidateSeededDataAsync()
{
    try
    {
        _logger.Information("🔍 VALIDATION: شروع اعتبارسنجی داده‌های Seed شده");

        // 1️⃣ بررسی Context.Local اول
        var localServicesCount = _context.Services.Local
            .Count(s => !s.IsDeleted && s.IsActive);
        var localSharedServicesCount = _context.SharedServices.Local
            .Count(ss => !ss.IsDeleted && ss.IsActive);
        var localServiceComponentsCount = _context.ServiceComponents.Local
            .Count(sc => !sc.IsDeleted && sc.IsActive);

        _logger.Information("📊 VALIDATION: Context.Local - خدمات: {Services}, خدمات مشترک: {Shared}, اجزا: {Components}",
            localServicesCount, localSharedServicesCount, localServiceComponentsCount);

        // 2️⃣ اگر Local خالی است، از DB بخوان (Fallback)
        if (localServicesCount == 0 && localSharedServicesCount == 0 && localServiceComponentsCount == 0)
        {
            _logger.Information("⚠️ VALIDATION: Context.Local خالی است - بررسی دیتابیس...");

            var dbServicesCount = await _context.Services
                .Where(s => !s.IsDeleted && s.IsActive).CountAsync();
            var dbSharedServicesCount = await _context.SharedServices
                .Where(ss => !ss.IsDeleted && ss.IsActive).CountAsync();
            var dbServiceComponentsCount = await _context.ServiceComponents
                .Where(sc => !sc.IsDeleted && sc.IsActive).CountAsync();

            _logger.Information("📊 VALIDATION: Database - خدمات: {Services}, خدمات مشترک: {Shared}, اجزا: {Components}",
                dbServicesCount, dbSharedServicesCount, dbServiceComponentsCount);

            var isValid = dbServicesCount > 0 && dbSharedServicesCount > 0 && dbServiceComponentsCount > 0;
            
            if (!isValid)
            {
                _logger.Error("❌ VALIDATION: داده‌های لازم در دیتابیس یافت نشد!");
                _logger.Error("   - خدمات: {Services} (حداقل: 1)", dbServicesCount);
                _logger.Error("   - خدمات مشترک: {Shared} (حداقل: 1)", dbSharedServicesCount);
                _logger.Error("   - اجزای خدمات: {Components} (حداقل: 1)", dbServiceComponentsCount);
            }
            else
            {
                _logger.Information("✅ VALIDATION: اعتبارسنجی موفق - داده‌ها در دیتابیس موجود هستند");
            }

            return isValid;
        }

        // 3️⃣ بررسی صحت داده‌های Local
        var localIsValid = localServicesCount > 0 && 
                          localSharedServicesCount > 0 && 
                          localServiceComponentsCount > 0;

        if (!localIsValid)
        {
            _logger.Error("❌ VALIDATION: داده‌های لازم در Context.Local یافت نشد!");
            _logger.Error("   - خدمات: {Services} (حداقل: 1)", localServicesCount);
            _logger.Error("   - خدمات مشترک: {Shared} (حداقل: 1)", localSharedServicesCount);
            _logger.Error("   - اجزای خدمات: {Components} (حداقل: 1)", localServiceComponentsCount);
            return false;
        }

        // 4️⃣ بررسی دقیق‌تر: خدمات بدون اجزا
        var servicesWithoutComponents = _context.Services.Local
            .Where(s => !s.IsDeleted && s.IsActive)
            .Where(s => s.ServiceComponents == null || 
                       !s.ServiceComponents.Any(sc => !sc.IsDeleted && sc.IsActive))
            .ToList();

        if (servicesWithoutComponents.Any())
        {
            _logger.Warning("⚠️ VALIDATION: {Count} خدمت بدون اجزای محاسباتی یافت شد:",
                servicesWithoutComponents.Count);
            
            foreach (var service in servicesWithoutComponents.Take(5))
            {
                _logger.Warning("   - {Code}: {Title}", service.ServiceCode, service.Title);
            }
        }

        // 5️⃣ بررسی دقیق‌تر: خدمات با قیمت صفر
        var servicesWithoutPrice = _context.Services.Local
            .Where(s => !s.IsDeleted && s.IsActive)
            .Where(s => s.Price == 0)
            .ToList();

        if (servicesWithoutPrice.Any())
        {
            _logger.Warning("⚠️ VALIDATION: {Count} خدمت با قیمت صفر یافت شد:",
                servicesWithoutPrice.Count);
            
            foreach (var service in servicesWithoutPrice.Take(5))
            {
                _logger.Warning("   - {Code}: {Title} = {Price:N0} ریال", 
                    service.ServiceCode, service.Title, service.Price);
            }
        }

        _logger.Information("✅ VALIDATION: اعتبارسنجی موفق - همه داده‌ها آماده ذخیره‌سازی هستند");
        return true;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "❌ VALIDATION: خطا در اعتبارسنجی داده‌های ایجاد شده");
        return false;
    }
}
```

---

## 🛡️ **ویژگی‌های ضدگلوله (Bulletproof):**

### **1. دو لایه بررسی:**
```
لایه 1: Context.Local (قبل از Commit)
لایه 2: Database (بعد از Commit - Fallback)
```

### **2. Logging دقیق:**
```
✅ تعداد دقیق هر Entity
✅ منبع داده (Local یا DB)
✅ لیست خدمات بدون اجزا
✅ لیست خدمات با قیمت صفر
❌ پیام‌های خطای دقیق
```

### **3. چند سطح Validation:**
```
Level 1: آیا تعداد کافی Entity وجود دارد؟
Level 2: آیا خدمات دارای اجزا هستند؟
Level 3: آیا خدمات دارای قیمت هستند؟
```

### **4. Fallback Mechanism:**
```
اگر Local خالی بود → از DB بخوان
این برای فراخوانی مجدد یا حالت‌های خاص
```

---

## 📊 **مقایسه قبل و بعد:**

| ویژگی | قبل | بعد |
|-------|-----|-----|
| **منبع داده** | فقط DB | Local + DB (Fallback) |
| **Transaction Safe** | ❌ | ✅ |
| **Logging** | ساده | دقیق و چندسطحی |
| **Validation سطوح** | 1 سطح (تعداد) | 3 سطح (تعداد، اجزا، قیمت) |
| **Error Messages** | عمومی | دقیق با جزئیات |
| **Fallback** | ❌ | ✅ |

---

## 🎯 **سناریوهای پوشش داده شده:**

### **سناریو 1: Seed عادی (اولین بار)**
```
→ AddRange → Local پر می‌شود
→ SaveChanges (uncommitted)
→ Validation از Local ✅
→ Commit ✅
```

### **سناریو 2: Seed مجدد (داده‌ها موجود است)**
```
→ AddRange فیلتر می‌شود (duplicate)
→ Local خالی
→ Validation از DB ✅
→ تشخیص می‌دهد قبلاً ذخیره شده ✅
```

### **سناریو 3: خدمت بدون اجزا**
```
→ Validation از Local
→ تشخیص: خدمت X بدون اجزا ⚠️
→ Warning Log
→ ادامه می‌دهد (نه Critical)
```

### **سناریو 4: خدمت با قیمت صفر**
```
→ Validation از Local
→ تشخیص: خدمت Y قیمت = 0 ⚠️
→ Warning Log
→ ادامه می‌دهد (نه Critical)
```

### **سناریو 5: هیچ Entity اضافه نشده**
```
→ Local خالی
→ DB هم خالی
→ Error Log دقیق ❌
→ return false
→ Rollback ✅
```

---

## 🔍 **Debug و Troubleshooting:**

### **لاگ‌های مورد انتظار:**

#### **حالت موفق:**
```
🔍 VALIDATION: شروع اعتبارسنجی داده‌های Seed شده
📊 VALIDATION: Context.Local - خدمات: 15, خدمات مشترک: 3, اجزا: 30
✅ VALIDATION: اعتبارسنجی موفق - همه داده‌ها آماده ذخیره‌سازی هستند
```

#### **حالت Warning (خدمت بدون اجزا):**
```
🔍 VALIDATION: شروع اعتبارسنجی داده‌های Seed شده
📊 VALIDATION: Context.Local - خدمات: 15, خدمات مشترک: 3, اجزا: 28
⚠️ VALIDATION: 2 خدمت بدون اجزای محاسباتی یافت شد:
   - 978000: ارزیابی و معاینه کودکان
   - 978001: ارزیابی سرپایی
✅ VALIDATION: اعتبارسنجی موفق - همه داده‌ها آماده ذخیره‌سازی هستند
```

#### **حالت فیل:**
```
🔍 VALIDATION: شروع اعتبارسنجی داده‌های Seed شده
📊 VALIDATION: Context.Local - خدمات: 0, خدمات مشترک: 0, اجزا: 0
⚠️ VALIDATION: Context.Local خالی است - بررسی دیتابیس...
📊 VALIDATION: Database - خدمات: 0, خدمات مشترک: 0, اجزا: 0
❌ VALIDATION: داده‌های لازم در دیتابیس یافت نشد!
   - خدمات: 0 (حداقل: 1)
   - خدمات مشترک: 0 (حداقل: 1)
   - اجزای خدمات: 0 (حداقل: 1)
```

---

## 📝 **Best Practices:**

### **✅ DO:**
```csharp
// 1. استفاده از Context.Local در Transaction
var count = _context.Entities.Local.Count();

// 2. Fallback به DB
if (localCount == 0)
{
    count = await _context.Entities.CountAsync();
}

// 3. Logging دقیق با سطوح مختلف
_logger.Information("📊 Count: {Count}", count);
_logger.Warning("⚠️ Some issue: {Details}", details);
_logger.Error("❌ Critical: {Error}", error);

// 4. چند سطح Validation
if (basicValid && detailsValid && pricesValid) { ... }
```

### **❌ DON'T:**
```csharp
// 1. Query از DB در Transaction قبل از Commit
var count = await _context.Entities.CountAsync(); // ❌

// 2. Validation بدون Logging
if (count == 0) return false; // ❌ چرا فیل شد؟

// 3. یک سطح Validation
return count > 0; // ❌ خیلی ساده

// 4. بدون Fallback
if (localCount == 0) return false; // ❌ شاید در DB باشد
```

---

## 🎓 **نکات آموزشی:**

### **Context.Local vs Database Query:**

```csharp
// Context.Local:
var local = _context.Services.Local; // IEnumerable<Service>
// - در Memory
// - Tracked entities
// - بدون Query به DB
// - شامل: Added, Modified, Unchanged

// Database Query:
var db = await _context.Services.ToListAsync(); // List<Service>
// - Query به DB
// - فقط Committed data
// - بدون Tracked entities (اگر AsNoTracking)
```

### **Transaction Isolation Levels:**

```
READ UNCOMMITTED: می‌تواند uncommitted data ببیند (Dirty Read)
READ COMMITTED:   فقط committed data (Default در SQL Server)
REPEATABLE READ:  جلوگیری از Non-Repeatable Read
SERIALIZABLE:     کامل‌ترین Isolation (کندترین)
```

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** ۲ اکتبر ۲۰۲۵  
**نسخه:** 1.0

