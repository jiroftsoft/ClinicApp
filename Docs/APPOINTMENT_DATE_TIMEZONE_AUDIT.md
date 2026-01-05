# 🔍 ClinicApp — Appointment Date/Timezone Audit & Fix Plan

**تاریخ:** 2026-01-06  
**اولویت:** 🔴 CRITICAL  
**وضعیت:** 🔄 **در حال بررسی**

---

## A) Findings (7 مورد بحرانی)

### 1. **استفاده مستقیم از `DateTime.Today` در AppointmentBookingController**
**Evidence:**
- `Areas/Patient/Controllers/AppointmentBookingController.cs:403` - `if (date.Date < DateTime.Today)`
- `Areas/Patient/Controllers/AppointmentBookingController.cs:411` - `var maxFutureDate = DateTime.Today.AddDays(90);`
- `Areas/Patient/Controllers/AppointmentBookingController.cs:493` - `if (appointmentDate.Date < DateTime.Today)`
- `Areas/Patient/Controllers/AppointmentBookingController.cs:630` - `if (model.AppointmentDate.Date < DateTime.Today)`

**Impact:** 🔴 CRITICAL - اگر سرور timezone ایران نباشد، validation تاریخ اشتباه می‌شود

**Root Cause:** عدم استفاده از `ITimeProvider.GetIranToday()`

---

### 2. **استفاده مستقیم از `DateTime.Now` در AppointmentBookingService**
**Evidence:**
- `Services/Appointment/AppointmentBookingService.cs:169` - `if (DateTime.Now > minimumCancelTime)`

**Impact:** 🔴 CRITICAL - زمان لغو نوبت ممکن است اشتباه محاسبه شود

**Root Cause:** عدم استفاده از `ITimeProvider.GetIranNow()`

---

### 3. **استفاده مستقیم از `DateTime.Today` در AppointmentValidationService**
**Evidence:**
- `Services/Appointment/AppointmentValidationService.cs:291` - `if (appointmentDate.Date < DateTime.Today)`

**Impact:** 🔴 CRITICAL - Validation تاریخ ممکن است اشتباه باشد

**Root Cause:** عدم استفاده از `ITimeProvider.GetIranToday()`

---

### 4. **استفاده مستقیم از `DateTime.Today` در DoctorScheduleRepository**
**Evidence:**
- `Repositories/ClinicAdmin/DoctorScheduleRepository.cs:1369` - `dateToGenerate = await FindFirstWorkDayForScheduleAsync(doctorSchedule, DateTime.Today);`
- `Repositories/ClinicAdmin/DoctorScheduleRepository.cs:2111` - `a.AppointmentDate >= DateTime.Today`

**Impact:** 🟡 HIGH - تولید اسلات ممکن است برای تاریخ اشتباه باشد

**Root Cause:** عدم استفاده از `ITimeProvider.GetIranToday()`

---

### 5. **استفاده از `ToLocalTime()` در PersianDateHelper**
**Evidence:**
- `Helpers/PersianDateHelper.cs:139` - `localDateTime = dateTime.ToLocalTime();`

**Impact:** 🔴 CRITICAL - اگر سرور timezone ایران نباشد، تبدیل تاریخ اشتباه می‌شود

**Root Cause:** استفاده از `ToLocalTime()` به جای `TimeZoneInfo.ConvertTimeFromUtc()`

---

### 6. **استفاده مستقیم از `DateTime.Now` در AppointmentRepository**
**Evidence:**
- `Repositories/Appointment/AppointmentRepository.cs:101` - `appointment.CreatedAt = DateTime.Now;`
- `Repositories/Appointment/AppointmentRepository.cs:135` - `appointment.UpdatedAt = DateTime.Now;`

**Impact:** 🟡 HIGH - Timestamp ممکن است در timezone اشتباه ذخیره شود

**Root Cause:** عدم استفاده از `ITimeProvider.UtcNow` برای CreatedAt/UpdatedAt

---

### 7. **استفاده مستقیم از `DateTime.UtcNow` در AppointmentBookingController (Payment)**
**Evidence:**
- `Areas/Patient/Controllers/AppointmentBookingController.cs:715` - `idempotencyKey = $"payment_{appointmentId}_{_currentUserService.UserId}_{DateTime.UtcNow:yyyyMMddHHmm}";`
- `Areas/Patient/Controllers/AppointmentBookingController.cs:811` - `CreatedAt = DateTime.UtcNow;`

**Impact:** 🟢 MEDIUM - این استفاده درست است (UTC برای timestamp)، اما باید از `ITimeProvider` استفاده شود برای consistency

**Root Cause:** عدم استفاده از `ITimeProvider.UtcNow`

---

## B) Fix Plan (Ranked)

### Plan A: سریع‌ترین Fix امن (کمترین diff)

**مرحله 1: Inject ITimeProvider در Controllers/Services**
- `AppointmentBookingController`: اضافه کردن `ITimeProvider` به constructor
- `AppointmentBookingService`: اضافه کردن `ITimeProvider` به constructor
- `AppointmentValidationService`: اضافه کردن `ITimeProvider` به constructor
- `DoctorScheduleRepository`: اضافه کردن `ITimeProvider` به constructor
- `AppointmentRepository`: اضافه کردن `ITimeProvider` به constructor

**مرحله 2: جایگزینی DateTime.Now/Today**
- `DateTime.Today` → `_timeProvider.GetIranToday()`
- `DateTime.Now` → `_timeProvider.GetIranNow()`
- `DateTime.UtcNow` → `_timeProvider.UtcNow`

**مرحله 3: Fix PersianDateHelper.ToPersianDate**
- جایگزینی `ToLocalTime()` با `TimeZoneInfo.ConvertTimeFromUtc()`

---

### Plan B: اگر نیاز به Refactor (Clock/Timezone abstraction)

**ITimeProvider** قبلاً وجود دارد و در UnityConfig ثبت شده است. فقط باید استفاده شود.

---

## C) Diffs

### 1. AppointmentBookingController.cs

```csharp
// اضافه کردن ITimeProvider به constructor
private readonly ITimeProvider _timeProvider;

public AppointmentBookingController(
    IAppointmentBookingService bookingService,
    ICurrentUserService currentUserService,
    ITimeProvider timeProvider, // ✅ اضافه شد
    ILogger logger)
{
    _bookingService = bookingService;
    _currentUserService = currentUserService;
    _timeProvider = timeProvider; // ✅ اضافه شد
    _logger = logger;
}

// جایگزینی DateTime.Today
if (date.Date < _timeProvider.GetIranToday()) // ✅ تغییر شد
var maxFutureDate = _timeProvider.GetIranToday().AddDays(90); // ✅ تغییر شد
```

### 2. AppointmentBookingService.cs

```csharp
// اضافه کردن ITimeProvider به constructor
private readonly ITimeProvider _timeProvider;

public AppointmentBookingService(
    IAppointmentRepository appointmentRepository,
    ITimeProvider timeProvider, // ✅ اضافه شد
    ILogger logger)
{
    _appointmentRepository = appointmentRepository;
    _timeProvider = timeProvider; // ✅ اضافه شد
    _logger = logger;
}

// جایگزینی DateTime.Now
if (_timeProvider.GetIranNow() > minimumCancelTime) // ✅ تغییر شد
```

### 3. AppointmentValidationService.cs

```csharp
// اضافه کردن ITimeProvider به constructor
private readonly ITimeProvider _timeProvider;

public AppointmentValidationService(ITimeProvider timeProvider) // ✅ اضافه شد
{
    _timeProvider = timeProvider;
}

// جایگزینی DateTime.Today
if (appointmentDate.Date < _timeProvider.GetIranToday()) // ✅ تغییر شد
```

### 4. DoctorScheduleRepository.cs

```csharp
// اضافه کردن ITimeProvider به constructor
private readonly ITimeProvider _timeProvider;

public DoctorScheduleRepository(
    ApplicationDbContext context,
    ITimeProvider timeProvider, // ✅ اضافه شد
    ILogger logger)
{
    _context = context;
    _timeProvider = timeProvider; // ✅ اضافه شد
    _logger = logger;
}

// جایگزینی DateTime.Today
dateToGenerate = await FindFirstWorkDayForScheduleAsync(doctorSchedule, _timeProvider.GetIranToday()); // ✅ تغییر شد
a.AppointmentDate >= _timeProvider.GetIranToday() // ✅ تغییر شد
```

### 5. AppointmentRepository.cs

```csharp
// اضافه کردن ITimeProvider به constructor
private readonly ITimeProvider _timeProvider;

public AppointmentRepository(
    ApplicationDbContext context,
    ITimeProvider timeProvider, // ✅ اضافه شد
    ILogger logger)
{
    _context = context;
    _timeProvider = timeProvider; // ✅ اضافه شد
    _logger = logger;
}

// جایگزینی DateTime.Now
appointment.CreatedAt = _timeProvider.UtcNow; // ✅ تغییر شد (UTC برای timestamp)
appointment.UpdatedAt = _timeProvider.UtcNow; // ✅ تغییر شد
```

### 6. PersianDateHelper.cs

```csharp
// Fix ToPersianDate method
public static string ToPersianDate(DateTime dateTime)
{
    // ...
    
    // ✅ CRITICAL FIX: استفاده از TimeZoneInfo به جای ToLocalTime()
    DateTime localDateTime = dateTime;
    if (dateTime.Kind == DateTimeKind.Utc)
    {
        // ✅ استفاده از TimeZoneInfo برای تبدیل دقیق
        var iranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
        localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, iranTimeZone);
        _log.Information("🔍 Converted UTC to Iran Time: {LocalDateTime}", localDateTime);
    }
    // ...
}
```

---

## D) Tests

### Unit Tests — Conversion
```csharp
[Test]
public void ToPersianDate_WithUtcDateTime_ShouldConvertToIranTime()
{
    // Arrange
    var utcDate = new DateTime(2025, 12, 25, 10, 0, 0, DateTimeKind.Utc);
    
    // Act
    var persianDate = PersianDateHelper.ToPersianDate(utcDate);
    
    // Assert
    Assert.AreEqual("1404/10/05", persianDate);
}

[Test]
public void GetIranToday_ShouldReturnTodayInIranTimezone()
{
    // Arrange
    var timeProvider = new DefaultTimeProvider();
    
    // Act
    var iranToday = timeProvider.GetIranToday();
    
    // Assert
    Assert.AreEqual(DateTimeKind.Unspecified, iranToday.Kind); // یا Local
    Assert.AreEqual(0, iranToday.Hour);
    Assert.AreEqual(0, iranToday.Minute);
    Assert.AreEqual(0, iranToday.Second);
}
```

### Unit/Integration — Booking Rules
```csharp
[Test]
public void ReserveAppointment_WithPastDate_ShouldFail()
{
    // Arrange
    var pastDate = _timeProvider.GetIranToday().AddDays(-1);
    
    // Act & Assert
    var result = await _bookingService.ReserveAppointmentAsync(new AppointmentBookingRequestDto
    {
        AppointmentDate = pastDate
    });
    
    Assert.IsFalse(result.Success);
    Assert.Contains("گذشته", result.Message);
}
```

---

## E) Verification (5 قدم Manual + log checkpoints)

### 1. بررسی Logs
- بررسی لاگ‌های `PersianDateHelper.ToPersianDate` برای اطمینان از تبدیل درست
- بررسی لاگ‌های `ITimeProvider.GetIranToday()` برای اطمینان از تاریخ درست

### 2. تست Validation
- تست رزرو نوبت برای تاریخ گذشته (باید reject شود)
- تست رزرو نوبت برای تاریخ آینده (باید accept شود)

### 3. تست Timezone
- تست در سرور با timezone ایران
- تست در سرور با timezone متفاوت (باید همان نتیجه را بدهد)

### 4. تست Slot Generation
- بررسی تولید اسلات برای تاریخ درست
- بررسی query های `AppointmentDate >= Today`

### 5. تست Payment Timestamp
- بررسی `CreatedAt` و `UpdatedAt` در UTC
- بررسی `idempotencyKey` با timestamp درست

---

**وضعیت:** 🔄 **در حال پیاده‌سازی**  
**تاریخ به‌روزرسانی:** 2026-01-06

