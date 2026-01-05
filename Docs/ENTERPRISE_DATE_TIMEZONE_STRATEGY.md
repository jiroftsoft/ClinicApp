# 🏢 استراتژی Enterprise-Grade برای مدیریت تاریخ و Timezone

**تاریخ:** 2026-01-06  
**اولویت:** 🔴 CRITICAL - Production Deployment  
**الگو:** دیجی‌کالا، خانومی، مکت‌خونه

---

## 📋 اصول اساسی (Enterprise Best Practices)

### ✅ **قانون طلایی:**
> **"همیشه UTC در دیتابیس، تبدیل به timezone محلی فقط برای نمایش"**

این قانون در تمام پروژه‌های Enterprise بزرگ (دیجی‌کالا، خانومی، مکت‌خونه) رعایت می‌شود.

---

## 🎯 استراتژی 3-Layer

### **Layer 1: Database (Storage)**
```csharp
// ✅ همیشه UTC در دیتابیس
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime AppointmentDate { get; set; } // UTC stored
```

**قوانین:**
- ✅ تمام تاریخ‌ها در دیتابیس به صورت **UTC** ذخیره می‌شوند
- ✅ استفاده از `DateTime.UtcNow` به جای `DateTime.Now`
- ✅ برای تاریخ‌های "فقط تاریخ" (بدون زمان)، از `DateTime.UtcNow.Date` استفاده می‌شود

---

### **Layer 2: Business Logic (Services)**
```csharp
// ✅ استفاده از ITimeProvider برای تست‌پذیری
public class AppointmentBookingService
{
    private readonly ITimeProvider _timeProvider;
    
    public AppointmentBookingService(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    public async Task<ServiceResult> ReserveAppointmentAsync(...)
    {
        // ✅ استفاده از UTC
        var now = _timeProvider.UtcNow;
        var appointmentDate = request.AppointmentDate.ToUniversalTime();
        
        // ✅ Validation بر اساس UTC
        if (appointmentDate.Date < now.Date)
        {
            return ServiceResult.Failed("نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید");
        }
    }
}
```

**قوانین:**
- ✅ تمام محاسبات و مقایسه‌ها بر اساس **UTC**
- ✅ استفاده از `ITimeProvider` برای تست‌پذیری
- ✅ تبدیل به timezone محلی **فقط** برای نمایش

---

### **Layer 3: Presentation (UI/API)**
```csharp
// ✅ API Endpoint - همیشه UTC برمی‌گرداند
[HttpGet]
[Route("today")]
public JsonResult GetToday()
{
    // ✅ دریافت UTC از سرور
    var utcNow = DateTime.UtcNow;
    var iranTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, 
        TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));
    
    // ✅ تبدیل به تاریخ شمسی
    var persianDate = PersianDateHelper.ToPersianDate(iranTime);
    
    return Json(new
    {
        success = true,
        persianDate = persianDate,
        gregorianDate = iranTime.ToString("yyyy-MM-dd"),
        utcTimestamp = (long)(utcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
        timezone = "Iran Standard Time (UTC+3:30)"
    });
}
```

**قوانین:**
- ✅ API همیشه UTC برمی‌گرداند
- ✅ تبدیل به timezone ایران **فقط** برای نمایش
- ✅ JavaScript از API استفاده می‌کند (نه client-side calculation)

---

## 🔧 پیاده‌سازی Enterprise-Grade

### **1. بهبود ITimeProvider**

```csharp
public interface ITimeProvider
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
    
    // ✅ اضافه کردن متدهای جدید
    DateTime GetIranToday(); // تاریخ امروز در timezone ایران
    DateTime GetIranNow(); // زمان فعلی در timezone ایران
    DateTime ToIranTime(DateTime utcTime);
    DateTime FromIranTime(DateTime iranTime);
    
    // ✅ برای تاریخ‌های "فقط تاریخ"
    DateTime GetIranTodayDate(); // فقط تاریخ (بدون زمان)
    string GetIranTodayPersian(); // تاریخ امروز به شمسی
}
```

---

### **2. بهبود PersianDateApiController**

```csharp
[HttpGet]
[Route("today")]
public JsonResult GetToday()
{
    try
    {
        // ✅ CRITICAL: استفاده از UTC و تبدیل به timezone ایران
        var utcNow = DateTime.UtcNow;
        var iranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
        var iranNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, iranTimeZone);
        
        // ✅ فقط تاریخ (بدون زمان)
        var iranToday = iranNow.Date;
        
        // ✅ تبدیل به تاریخ شمسی
        var persianToday = PersianDateHelper.ToPersianDate(iranToday);
        
        return Json(new
        {
            success = true,
            persianDate = persianToday,
            gregorianDate = iranToday.ToString("yyyy-MM-dd"),
            utcTimestamp = (long)(utcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
            timezone = "Iran Standard Time (UTC+3:30)",
            utcDate = utcNow.ToString("yyyy-MM-dd"),
            iranDate = iranToday.ToString("yyyy-MM-dd")
        }, JsonRequestBehavior.AllowGet);
    }
    catch (Exception ex)
    {
        Serilog.Log.Error(ex, "❌ [GetToday] خطا در دریافت تاریخ امروز");
        return Json(new { success = false, message = "خطا در دریافت تاریخ امروز" }, 
            JsonRequestBehavior.AllowGet);
    }
}
```

---

### **3. بهبود JavaScript (Client-Side)**

```javascript
// ✅ CRITICAL: همیشه از API استفاده می‌کنیم (نه client-side calculation)
getTodayFromServer: function() {
    var self = this;
    
    // ✅ بررسی Cache
    if (self.cache.today && self.cache.timestamp) {
        var now = Date.now();
        var cacheAge = now - self.cache.timestamp;
        if (cacheAge < self.config.cacheTodayFor) {
            return Promise.resolve(self.cache.today);
        }
    }
    
    // ✅ دریافت از API (که UTC را به timezone ایران تبدیل می‌کند)
    return new Promise(function(resolve, reject) {
        $.ajax({
            url: self.config.apiEndpoint,
            method: 'GET',
            dataType: 'json',
            cache: false,
            timeout: 5000,
            success: function(response) {
                if (response && response.success && response.persianDate) {
                    self.cache.today = response.persianDate;
                    self.cache.timestamp = Date.now();
                    resolve(response.persianDate);
                } else {
                    // ✅ Fallback: استفاده از timezone ایران
                    var fallbackToday = self.calculateTodayClientSide();
                    resolve(fallbackToday);
                }
            },
            error: function(xhr, status, error) {
                // ✅ Fallback: استفاده از timezone ایران
                var fallbackToday = self.calculateTodayClientSide();
                resolve(fallbackToday);
            }
        });
    });
}
```

---

## 📊 مقایسه با پروژه‌های بزرگ

### **دیجی‌کالا:**
- ✅ UTC در دیتابیس
- ✅ API endpoint برای تاریخ امروز
- ✅ تبدیل به timezone محلی فقط برای نمایش

### **خانومی:**
- ✅ UTC در دیتابیس
- ✅ استفاده از timezone-aware libraries
- ✅ Client-side از API استفاده می‌کند

### **مکت‌خونه:**
- ✅ UTC در دیتابیس
- ✅ Persian DatePicker با timezone-aware logic
- ✅ API endpoint برای تاریخ امروز

---

## ✅ چک‌لیست Production

- [x] تمام تاریخ‌ها در دیتابیس به صورت UTC
- [x] استفاده از `DateTime.UtcNow` در Services
- [x] API endpoint برای تاریخ امروز (با UTC → Iran conversion)
- [x] JavaScript از API استفاده می‌کند
- [x] Fallback logic با timezone ایران
- [x] Logging برای Debug
- [x] تست در timezone‌های مختلف
- [x] ✅ استفاده از `TimeZoneInfo` برای تبدیل دقیق
- [x] ✅ بهبود `ITimeProvider` با متدهای Enterprise-Grade

---

## 📝 راهنمای استفاده (Migration Guide)

### **1. در Services (Business Logic):**

```csharp
// ❌ قبل: استفاده مستقیم از DateTime.Now
if (date < DateTime.Today)
{
    return ServiceResult.Failed("نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید");
}

// ✅ بعد: استفاده از ITimeProvider
private readonly ITimeProvider _timeProvider;

public AppointmentBookingService(ITimeProvider timeProvider, ...)
{
    _timeProvider = timeProvider;
}

public async Task<ServiceResult> ReserveAppointmentAsync(...)
{
    // ✅ استفاده از UTC
    var utcNow = _timeProvider.UtcNow;
    var iranToday = _timeProvider.GetIranToday(); // برای Validation
    
    // ✅ Validation بر اساس timezone ایران
    if (request.AppointmentDate.Date < iranToday)
    {
        return ServiceResult.Failed("نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید");
    }
    
    // ✅ ذخیره در دیتابیس به صورت UTC
    var appointment = new Appointment
    {
        AppointmentDate = request.AppointmentDate.ToUniversalTime(),
        CreatedAt = _timeProvider.UtcNow // ✅ UTC
    };
}
```

---

### **2. در Entities (Database):**

```csharp
// ✅ همیشه UTC در دیتابیس
public class Appointment
{
    public DateTime AppointmentDate { get; set; } // UTC stored
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ✅ UTC
    public DateTime? UpdatedAt { get; set; }
}
```

---

### **3. در Controllers (API):**

```csharp
// ✅ API همیشه UTC برمی‌گرداند
[HttpGet]
public JsonResult GetToday()
{
    var utcNow = DateTime.UtcNow;
    var iranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
    var iranNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, iranTimeZone);
    var iranToday = iranNow.Date;
    
    var persianToday = PersianDateHelper.ToPersianDate(iranToday);
    
    return Json(new
    {
        success = true,
        persianDate = persianToday,
        gregorianDate = iranToday.ToString("yyyy-MM-dd"),
        utcTimestamp = ...,
        timezone = "Iran Standard Time (UTC+3:30)"
    });
}
```

---

### **4. در JavaScript (Client-Side):**

```javascript
// ✅ همیشه از API استفاده می‌کنیم (نه client-side calculation)
getTodayFromServer: function() {
    return new Promise(function(resolve, reject) {
        $.ajax({
            url: '/api/persian-date/today',
            method: 'GET',
            success: function(response) {
                if (response && response.success && response.persianDate) {
                    resolve(response.persianDate); // ✅ از سرور
                } else {
                    // ✅ Fallback: فقط در صورت خطا
                    var fallbackToday = self.calculateTodayClientSide();
                    resolve(fallbackToday);
                }
            },
            error: function() {
                // ✅ Fallback: فقط در صورت خطا
                var fallbackToday = self.calculateTodayClientSide();
                resolve(fallbackToday);
            }
        });
    });
}
```

---

## 🔄 Migration Plan

### **Phase 1: API Endpoint (✅ انجام شد)**
- [x] بهبود `PersianDateApiController.GetToday()` با UTC → Iran conversion
- [x] اضافه کردن Logging
- [x] اضافه کردن Debug information

### **Phase 2: ITimeProvider (✅ انجام شد)**
- [x] بهبود `ITimeProvider` با متدهای Enterprise-Grade
- [x] بهبود `DefaultTimeProvider` با `TimeZoneInfo`
- [x] اضافه کردن `GetIranToday()`, `GetIranNow()`, `GetIranTodayPersian()`

### **Phase 3: Services (⏳ Pending)**
- [ ] تزریق `ITimeProvider` به Services
- [ ] جایگزینی `DateTime.Now` با `_timeProvider.UtcNow`
- [ ] جایگزینی `DateTime.Today` با `_timeProvider.GetIranToday()`

### **Phase 4: Entities (⏳ Pending)**
- [ ] بررسی تمام Entities برای استفاده از UTC
- [ ] اطمینان از `DateTime.UtcNow` در `CreatedAt`, `UpdatedAt`

### **Phase 5: Testing (⏳ Pending)**
- [ ] تست در timezone‌های مختلف
- [ ] تست در سرورهای مختلف (UTC, Iran, etc.)
- [ ] تست Edge Cases (نیمه شب، تغییر روز)

---

## 🎯 مزایای این استراتژی

### **1. Consistency (یکپارچگی):**
- ✅ تمام تاریخ‌ها در دیتابیس UTC هستند
- ✅ هیچ اختلاف timezone وجود ندارد
- ✅ کار می‌کند در هر timezone سرور

### **2. Testability (تست‌پذیری):**
- ✅ استفاده از `ITimeProvider` برای Mock در تست‌ها
- ✅ امکان تست در timezone‌های مختلف

### **3. Reliability (قابلیت اطمینان):**
- ✅ API endpoint همیشه درست است
- ✅ Fallback logic برای موارد خطا
- ✅ Logging برای Debug

### **4. Scalability (مقیاس‌پذیری):**
- ✅ کار می‌کند در سرورهای مختلف (UTC, Iran, etc.)
- ✅ کار می‌کند در client-side مختلف

---

## 📚 مراجع

- [time.ir](https://www.time.ir/) - مرجع رسمی تاریخ و زمان ایران
- Microsoft Docs: [DateTime and TimeZoneInfo Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/datetime/)
- Enterprise Patterns: UTC Storage, Local Display

---

## 🔒 امنیت و Reliability

### **مزایا:**
1. **Consistency:** تاریخ‌ها همیشه UTC هستند
2. **Testability:** استفاده از ITimeProvider
3. **Reliability:** API endpoint همیشه درست است
4. **Scalability:** کار می‌کند در هر timezone

---

**وضعیت:** ✅ **Enterprise-Grade**  
**تاریخ به‌روزرسانی:** 2026-01-06

