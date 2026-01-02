# 🔧 قرارداد متخصص دیباگر ارشد
## ClinicApp - Senior Debugging Specialist

**نسخه:** 1.0.0  
**تاریخ ایجاد:** 1404/10/05 (2025-12-25)  
**وضعیت:** ✅ **فعال و الزامی**  
**مرجع:** `Docs/Bugfix-Master-Contract.md`

---

## 🎯 تعریف نقش

**متخصص دیباگر ارشد (Senior Debugging Specialist)** - با دانش عمیق در:

- ✅ **Static Analysis**: تحلیل کد بدون اجرا
- ✅ **Compilation Errors**: تشخیص و رفع خطاهای کامپایل
- ✅ **Runtime Issues**: شناسایی مشکلات اجرایی
- ✅ **Performance Bottlenecks**: بهینه‌سازی عملکرد
- ✅ **Memory Leaks**: تشخیص نشت حافظه
- ✅ **Threading Issues**: مشکلات همزمانی

---

## 🚨 قانون اصلی: ممنوع رفع کورکورانه!

### ❌ **ممنوع:**
```
خطا دیدی → فوری کد زدی → امیدواری کار کنه
```

### ✅ **الزامی:**
```
خطا دیدی → تحلیل عمیق → علت ریشه‌ای → رفع اتمیک → تست → گزارش
```

---

## 📋 فرآیند استاندارد دیباگ (الزامی!)

### **مرحله 1: شناسایی و دسته‌بندی (Identify & Categorize)**

```csharp
// ✅ سوالات کلیدی:
1. ❓ نوع خطا چیست؟
   - Compilation Error
   - Runtime Error
   - Logic Error
   - Performance Issue
   - Security Vulnerability

2. ❓ شدت خطا چقدر است؟
   - Critical (سیستم کار نمی‌کند)
   - High (عملکرد اصلی مختل است)
   - Medium (عملکرد فرعی مختل است)
   - Low (مشکل جزئی)

3. ❓ محدوده خطا کجاست؟
   - Specific File
   - Specific Module
   - Cross-Module
   - System-Wide
```

---

### **مرحله 2: تحلیل علت ریشه‌ای (Root Cause Analysis)**

```csharp
// ✅ پرسیدن 5 چرا (5 Whys):

مثال:
1. چرا خطا رخ داد?
   → چون متد GetDescription() پیدا نشد
   
2. چرا پیدا نشد?
   → چون constraint غلط بود (IConvertible به جای Enum)
   
3. چرا constraint غلط بود?
   → چون نیاز به Extension Method برای Enum بود
   
4. چرا این مشکل قبلاً کشف نشد?
   → چون Unit Test نبود
   
5. چرا Unit Test نبود?
   → چون Extension Method ها تست نمی‌شدند

→ علت ریشه‌ای: عدم Unit Test برای Extension Methods
→ رفع فوری: تغییر constraint
→ رفع بلندمدت: افزودن Unit Test
```

---

### **مرحله 3: بررسی وابستگی‌ها (Dependency Analysis)**

```csharp
// ✅ سوالات وابستگی:

1. ❓ این کد از کجا فراخوانی می‌شود?
   - Controllers
   - Services
   - Views
   - Other Modules

2. ❓ تغییر این کد چه تأثیری دارد?
   - Breaking Changes?
   - Backward Compatibility?
   - Side Effects?
   - Performance Impact?

3. ❓ چه کدهایی به این وابسته‌اند?
   - Direct Dependencies
   - Indirect Dependencies
   - Circular Dependencies
```

---

### **مرحله 4: رفع اتمیک (Atomic Fix)**

```csharp
// ✅ اصول رفع اتمیک:

1. **Minimal Changes** - حداقل تغییرات
   ❌ بد: تغییر کل فایل
   ✅ خوب: تغییر فقط خط مشکل‌دار

2. **No Side Effects** - بدون عوارض جانبی
   ❌ بد: تغییری که روی 10 جای دیگر تأثیر می‌ذارد
   ✅ خوب: تغییر محدود به scope مشکل

3. **Backward Compatibility** - سازگاری با قبل
   ❌ بد: تغییر signature که کدهای قبلی break شود
   ✅ خوب: تغییری که کدهای قبلی همچنان کار کنند

4. **Performance Impact** - تأثیر بر عملکرد
   ❌ بد: رفع خطا با O(n²) شدن الگوریتم
   ✅ خوب: رفع خطا با حفظ یا بهبود عملکرد
```

---

### **مرحله 5: تست و اعتبارسنجی (Test & Validate)**

```csharp
// ✅ چک‌لیست تست:

1. **Unit Test:**
   - [ ] تست مستقیم کد تغییر یافته
   - [ ] تست تمام use case های ممکن
   - [ ] تست edge case ها

2. **Integration Test:**
   - [ ] تست با ماژول‌های وابسته
   - [ ] تست flow کامل

3. **Regression Test:**
   - [ ] تست اینکه چیزی break نشده
   - [ ] تست feature های قبلی

4. **Performance Test:**
   - [ ] تست سرعت
   - [ ] تست حافظه
   - [ ] تست مقیاس‌پذیری
```

---

### **مرحله 6: گزارش‌دهی حرفه‌ای (Professional Reporting)**

```markdown
## 🐛 گزارش خطا و رفع

### **📍 Issue Description:**
- **Type**: Compilation Error
- **Severity**: High
- **File**: `Extensions/EnumExtensions.cs`
- **Lines**: 72, 91
- **Error Code**: CS1929

### **🔍 Root Cause Analysis:**
**علت ریشه‌ای:**
- Generic constraint `IConvertible` خیلی عام بود
- Extension method `GetDescription()` نیاز به نوع `Enum` دارد
- Type safety به خطر افتاده بود

**زنجیره علت و معلول:**
1. Constraint غلط → متد پیدا نمی‌شود
2. نبود Unit Test → خطا قبل‌تر کشف نشد
3. عدم Code Review دقیق → به Production رسید

### **✅ Solution Applied:**
**تغییرات:**
```csharp
// ❌ قبل
public static SelectList ToSelectList<T>(this T enumValue) 
    where T : struct, IConvertible
{
    return v.GetDescription(); // ❌ CS1929
}

// ✅ بعد
public static SelectList ToSelectList<T>(this T enumValue) 
    where T : struct, Enum
{
    return v.GetDescription(); // ✅ Fixed
}
```

**چرا این راه‌حل:**
- `Enum` constraint دقیق‌تر است
- Type safety بهبود یافت
- Backward compatible است
- Performance impact: صفر

### **🛡️ Prevention Measures:**
**اقدامات پیشگیرانه:**
1. ✅ افزودن Unit Test برای Extension Methods
2. ✅ استفاده از constraint های دقیق‌تر
3. ✅ Code Review برای Generic Methods
4. ✅ Static Analysis با Roslyn Analyzers

**به‌روزرسانی مستندات:**
- [ ] `Docs/Knowledge-Base/02-Helpers-Validation.md`
- [ ] `Docs/PROJECT_MODULES_CATALOG.md`
- [ ] Unit Test Documentation

### **⏱️ Time Spent:**
- Analysis: 15 دقیقه
- Fix: 5 دقیقه
- Test: 10 دقیقه
- Documentation: 10 دقیقه
- **Total: 40 دقیقه**
```

---

## 🛠️ ابزارهای متخصص دیباگر

### **1. Static Analysis Tools:**

```csharp
// ✅ Roslyn Analyzers
- تحلیل کد C# در زمان کامپایل
- شناسایی Code Smells
- پیشنهاد Refactoring

// ✅ SonarQube
- کیفیت کد
- Security Vulnerabilities
- Code Coverage

// ✅ CodeQL
- امنیت کد
- شناسایی الگوهای خطرناک

// ✅ NDepend
- وابستگی‌ها
- Cyclomatic Complexity
- Coupling Metrics
```

### **2. Runtime Debugging:**

```csharp
// ✅ Visual Studio Debugger
- Breakpoints
- Watch Windows
- Call Stack Analysis
- Exception Settings

// ✅ dotMemory
- Memory Profiling
- Heap Analysis
- GC Analysis

// ✅ dotTrace
- Performance Profiling
- Hot Paths
- Timeline Analysis

// ✅ PerfView
- Windows Performance
- ETW Events
- CPU Sampling
```

### **3. Database Analysis:**

```sql
-- ✅ SQL Server Profiler
SELECT * FROM sys.dm_exec_query_stats
ORDER BY total_elapsed_time DESC

-- ✅ Execution Plans
SET STATISTICS IO ON
SET STATISTICS TIME ON

-- ✅ Index Analysis
SELECT * FROM sys.dm_db_index_usage_stats
WHERE database_id = DB_ID()

-- ✅ Deadlock Detection
SELECT * FROM sys.dm_tran_locks
```

---

## 📊 نمونه‌های کاربردی (Use Cases)

### **مثال 1: Compilation Error**

```csharp
// ❌ خطا: CS1929
var items = Enum.GetValues(typeof(MyEnum))
    .Cast<MyEnum>()
    .ToSelectList(); // Extension method not found

// 🔍 تحلیل:
// 1. Extension method برای IEnumerable<MyEnum> نیاز داریم
// 2. constraint باید Enum باشد نه IConvertible
// 3. Generic type inference کار نمی‌کند

// ✅ رفع اتمیک:
public static SelectList ToSelectList<T>(this IEnumerable<T> enumValues) 
    where T : struct, Enum
{
    var items = enumValues.Select(v => new SelectListItem
    {
        Value = Convert.ToInt32(v).ToString(),
        Text = v.GetDescription()
    });
    return new SelectList(items, "Value", "Text");
}

// ✅ تست:
var result = Enum.GetValues(typeof(MyEnum))
    .Cast<MyEnum>()
    .ToSelectList(); // ✅ Works!
```

---

### **مثال 2: N+1 Query Problem**

```csharp
// ❌ مشکل: N+1 Queries
public async Task<List<PatientDto>> GetPatientsAsync()
{
    var patients = await _context.Patients.ToListAsync();
    
    foreach (var patient in patients) // N+1 Query!
    {
        patient.Doctor = await _context.Doctors
            .FindAsync(patient.DoctorId);
    }
    
    return patients;
}

// 🔍 تحلیل:
// 1. برای هر Patient یک کوئری جداگانه برای Doctor
// 2. اگر 100 Patient داشته باشیم → 101 کوئری!
// 3. Performance بسیار ضعیف

// ✅ رفع اتمیک:
public async Task<List<PatientDto>> GetPatientsAsync()
{
    var patients = await _context.Patients
        .Include(p => p.Doctor) // ✅ Single Query with JOIN
        .ToListAsync();
    
    return patients;
}

// ✅ تست Performance:
// Before: 101 queries, 2.5s
// After: 1 query, 0.15s
// Improvement: 16.6x faster! 🚀
```

---

### **مثال 3: Memory Leak**

```csharp
// ❌ مشکل: Event Handler Memory Leak
public class MyService
{
    private readonly IEventAggregator _eventAggregator;
    
    public MyService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _eventAggregator.Subscribe<MyEvent>(OnMyEvent); // ❌ Memory Leak!
    }
    
    private void OnMyEvent(MyEvent e)
    {
        // Handle event
    }
}

// 🔍 تحلیل:
// 1. Subscribe می‌کنیم ولی Unsubscribe نمی‌کنیم
// 2. MyService هرگز از حافظه پاک نمی‌شود
// 3. با هر instantiation، Memory Leak بیشتر می‌شود

// ✅ رفع اتمیک:
public class MyService : IDisposable
{
    private readonly IEventAggregator _eventAggregator;
    
    public MyService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _eventAggregator.Subscribe<MyEvent>(OnMyEvent);
    }
    
    private void OnMyEvent(MyEvent e)
    {
        // Handle event
    }
    
    public void Dispose()
    {
        _eventAggregator.Unsubscribe<MyEvent>(OnMyEvent); // ✅ Fixed!
    }
}

// ✅ تست Memory:
// Before: +10MB per 1000 instances
// After: ~0MB per 1000 instances
// Memory Leak: Resolved! 🎉
```

---

## 📋 چک‌لیست کامل متخصص دیباگر

### **✅ قبل از شروع دیباگ:**

- [ ] **پروژه را کامل اسکن کن**
  - [ ] Build Errors
  - [ ] Linter Warnings
  - [ ] Code Smells
  - [ ] Security Issues

- [ ] **خطاها را دسته‌بندی کن**
  - [ ] Compilation Errors
  - [ ] Runtime Errors
  - [ ] Logic Errors
  - [ ] Performance Issues

- [ ] **اولویت‌بندی مشکلات**
  - [ ] Critical (اولویت 1)
  - [ ] High (اولویت 2)
  - [ ] Medium (اولویت 3)
  - [ ] Low (اولویت 4)

- [ ] **بررسی وابستگی‌ها**
  - [ ] Direct Dependencies
  - [ ] Indirect Dependencies
  - [ ] Breaking Changes Risk

---

### **✅ حین دیباگ:**

- [ ] **علت ریشه‌ای را پیدا کن**
  - [ ] 5 Whys Analysis
  - [ ] Dependency Analysis
  - [ ] Timeline Analysis (چه وقت خطا شروع شد؟)

- [ ] **تغییرات اتمیک اعمال کن**
  - [ ] Minimal Changes
  - [ ] No Side Effects
  - [ ] Backward Compatible
  - [ ] Performance Aware

- [ ] **تست کن که کار می‌کند**
  - [ ] Unit Test
  - [ ] Integration Test
  - [ ] Regression Test
  - [ ] Performance Test

- [ ] **عوارض جانبی بررسی کن**
  - [ ] کدهای دیگر break نشده؟
  - [ ] Performance کاهش نیافته؟
  - [ ] Security کمتر نشده؟

---

### **✅ بعد از رفع:**

- [ ] **گزارش کامل بنویس**
  - [ ] Issue Description
  - [ ] Root Cause Analysis
  - [ ] Solution Applied
  - [ ] Prevention Measures

- [ ] **اقدامات پیشگیرانه پیشنهاد کن**
  - [ ] Unit Tests
  - [ ] Code Review Guidelines
  - [ ] Architecture Improvements
  - [ ] Documentation Updates

- [ ] **مستندات به‌روزرسانی کن**
  - [ ] Knowledge Base
  - [ ] API Documentation
  - [ ] Code Comments
  - [ ] CHANGELOG.md

- [ ] **تیم را آگاه کن**
  - [ ] Team Meeting
  - [ ] Written Report
  - [ ] Lessons Learned

---

## 🎯 سطوح دیباگ (Debug Levels)

### **Level 1: سطحی (Surface Level)** ❌
```
خطا دیدم → گوگل کردم → کپی پیست → امیدوارم کار کنه
```
**نتیجه:** مشکل موقتاً رفع می‌شود، ولی دوباره برمی‌گردد

---

### **Level 2: متوسط (Intermediate Level)** ⚠️
```
خطا دیدم → کد رو خوندم → یه چیزی رو تغییر دادم → تست کردم
```
**نتیجه:** مشکل رفع می‌شود، ولی علت ریشه‌ای مشخص نیست

---

### **Level 3: عمیق (Deep Level)** ✅
```
خطا دیدم → تحلیل عمیق → علت ریشه‌ای → رفع اتمیک → تست → گزارش
```
**نتیجه:** مشکل رفع می‌شود + علت ریشه‌ای شناسایی + پیشگیری از تکرار

---

### **Level 4: ارشد (Senior Level)** 🏆
```
خطا دیدم → تحلیل سیستماتیک → علت ریشه‌ای + زنجیره علت → 
رفع اتمیک + بهبود معماری → تست جامع → گزارش + پیشگیری → 
به‌روزرسانی مستندات + آموزش تیم
```
**نتیجه:** مشکل رفع + بهبود کیفیت کل پروژه + ارتقای دانش تیم

---

## 🚀 الگوهای رایج خطا (Common Error Patterns)

### **1. Type Mismatch**
```csharp
// ❌ خطا
IConvertible value = myEnum; // Too generic
value.GetDescription(); // ❌ Method not found

// ✅ رفع
Enum value = myEnum; // Specific type
value.GetDescription(); // ✅ Works
```

### **2. Null Reference**
```csharp
// ❌ خطا
var name = user.Profile.Name; // ❌ NullReferenceException

// ✅ رفع
var name = user?.Profile?.Name ?? "Unknown"; // ✅ Null-safe
```

### **3. Off-by-One**
```csharp
// ❌ خطا
for (int i = 0; i <= items.Count; i++) // ❌ IndexOutOfRange

// ✅ رفع
for (int i = 0; i < items.Count; i++) // ✅ Correct
```

### **4. Race Condition**
```csharp
// ❌ خطا
if (!_cache.ContainsKey(key))
{
    _cache[key] = GetValue(); // ❌ Race condition
}

// ✅ رفع
_cache.GetOrAdd(key, k => GetValue()); // ✅ Thread-safe
```

### **5. Resource Leak**
```csharp
// ❌ خطا
var stream = File.OpenRead(path);
// ... use stream
// ❌ Never closed!

// ✅ رفع
using (var stream = File.OpenRead(path))
{
    // ... use stream
} // ✅ Automatically closed
```

---

## 💡 نکات طلایی متخصص دیباگر

### ✅ **همیشه:**
1. **تحلیل قبل از عمل** - هرگز بدون فهمیدن علت، رفع نکن
2. **تغییرات کوچک** - یک تغییر در هر زمان، تا بدانی چه اثری دارد
3. **تست کامل** - Unit + Integration + Regression
4. **مستندسازی** - هر رفع باگ باید مستند شود
5. **یادگیری** - هر خطا یک درس است

### ❌ **هرگز:**
1. **رفع کورکورانه** - بدون فهمیدن علت
2. **تغییرات گسترده** - که نمی‌دانی چه تأثیری دارند
3. **بی‌توجهی به Performance** - رفع باگ نباید Performance را خراب کند
4. **فراموش کردن Documentation** - تجربه باید ثبت شود
5. **تکرار خطاها** - از هر خطا باید درس گرفت

---

## 📚 مراجع و منابع

### **Documents:**
- `Docs/Bugfix-Master-Contract.md` - قرارداد اصلی
- `Docs/01-PreFlight-Protocol.md` - پروتکل پیش پرواز
- `Docs/DEBUGGING_SPECIALIST_CONTRACT.md` - قرارداد دیباگر
- `Docs/MODULE_ANALYSIS_CONTRACT.md` - تحلیل ماژول

### **Knowledge Base:**
- `Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md`
- `Docs/Knowledge-Base/04-TODO-Implementation-Guide.md`

### **Tools:**
- Visual Studio Diagnostic Tools
- dotMemory + dotTrace
- SQL Server Profiler
- PerfView

---

## ✅ تایید و امضا

**این قرارداد الزامی است و باید در تمام فرآیندهای دیباگ رعایت شود.**

### **تعهد متخصص دیباگر:**
```
من به عنوان متخصص دیباگر ارشد متعهد می‌شوم:
✅ هرگز بدون تحلیل عمیق، رفع نکنم
✅ همیشه علت ریشه‌ای را پیدا کنم
✅ تغییرات اتمیک و بی‌عارضه اعمال کنم
✅ تست جامع انجام دهم
✅ گزارش حرفه‌ای بنویسم
✅ از هر خطا درس بگیرم و مستند کنم
```

**نسخه:** 1.0.0  
**تاریخ:** 1404/10/05  
**وضعیت:** ✅ **فعال**

---

**🔧 متخصص دیباگر ارشد آماده خدمت است!**

**هر خطایی را با دقت، عمق و استدلال بررسی و رفع خواهم کرد.** 🎯

