# 🔄 راهنمای Migration به استراتژی Enterprise-Grade برای تاریخ

**تاریخ:** 2026-01-06  
**اولویت:** 🔴 CRITICAL - Production Deployment  
**هدف:** تبدیل تمام کدها به استراتژی Enterprise-Grade

---

## 📋 وضعیت فعلی

### ❌ **مشکلات موجود:**
1. استفاده مستقیم از `DateTime.Now` و `DateTime.Today` در Services
2. عدم استفاده از `ITimeProvider` برای تست‌پذیری
3. عدم اطمینان از UTC در دیتابیس
4. مشکل timezone در client-side calculation

---

## ✅ راه‌حل Enterprise-Grade

### **قانون طلایی:**
> **"همیشه UTC در دیتابیس، تبدیل به timezone محلی فقط برای نمایش"**

---

## 🔧 مراحل Migration

### **STEP 1: Dependency Injection Setup**

```csharp
// ✅ در Global.asax.cs یا Startup.cs
// تزریق ITimeProvider به DI Container
container.RegisterType<ITimeProvider, DefaultTimeProvider>(new ContainerControlledLifetimeManager());
```

---

### **STEP 2: Update Services**

#### **قبل:**
```csharp
public class AppointmentBookingService
{
    public async Task<ServiceResult> ReserveAppointmentAsync(...)
    {
        if (date < DateTime.Today) // ❌ مشکل timezone
        {
            return ServiceResult.Failed("...");
        }
        
        var appointment = new Appointment
        {
            CreatedAt = DateTime.Now // ❌ مشکل timezone
        };
    }
}
```

#### **بعد:**
```csharp
public class AppointmentBookingService
{
    private readonly ITimeProvider _timeProvider;
    
    public AppointmentBookingService(
        ITimeProvider timeProvider,
        ...)
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
            return ServiceResult.Failed("...");
        }
        
        // ✅ ذخیره در دیتابیس به صورت UTC
        var appointment = new Appointment
        {
            AppointmentDate = request.AppointmentDate.ToUniversalTime(),
            CreatedAt = _timeProvider.UtcNow // ✅ UTC
        };
    }
}
```

---

### **STEP 3: Update Entities**

#### **قبل:**
```csharp
public class Appointment
{
    public DateTime CreatedAt { get; set; } = DateTime.Now; // ❌
}
```

#### **بعد:**
```csharp
public class Appointment
{
    // ✅ UTC در دیتابیس
    // Note: مقداردهی اولیه در Service انجام می‌شود
    public DateTime CreatedAt { get; set; }
}
```

---

### **STEP 4: Update Controllers**

#### **قبل:**
```csharp
public JsonResult GetToday()
{
    var today = DateTime.Today; // ❌ مشکل timezone
    var persianToday = PersianDateHelper.ToPersianDate(today);
    return Json(new { persianDate = persianToday });
}
```

#### **بعد:**
```csharp
public JsonResult GetToday()
{
    // ✅ ENTERPRISE: استفاده از UTC و تبدیل به timezone ایران
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

## 📊 فایل‌های نیاز به Migration

### **Services:**
- [ ] `Services/Appointment/AppointmentBookingService.cs`
- [ ] `Services/Appointment/AppointmentValidationService.cs`
- [ ] `Services/ClinicAdmin/DoctorScheduleService.cs`
- [ ] `Services/PatientDashboardService.cs`

### **Entities:**
- [ ] بررسی تمام Entities برای `CreatedAt`, `UpdatedAt`
- [ ] اطمینان از UTC در تمام تاریخ‌ها

### **Controllers:**
- [ ] `Controllers/Api/PersianDateApiController.cs` ✅ (انجام شد)
- [ ] بررسی سایر Controllers برای استفاده از تاریخ

---

## ✅ چک‌لیست Migration

- [ ] Dependency Injection برای `ITimeProvider` تنظیم شده
- [ ] تمام Services از `ITimeProvider` استفاده می‌کنند
- [ ] تمام `DateTime.Now` به `_timeProvider.UtcNow` تبدیل شده
- [ ] تمام `DateTime.Today` به `_timeProvider.GetIranToday()` تبدیل شده
- [ ] تمام تاریخ‌ها در دیتابیس UTC هستند
- [ ] API endpoint از UTC → Iran conversion استفاده می‌کند
- [ ] JavaScript از API استفاده می‌کند
- [ ] تست در timezone‌های مختلف انجام شده

---

**وضعیت:** 🔄 **در حال Migration**  
**تاریخ به‌روزرسانی:** 2026-01-06

