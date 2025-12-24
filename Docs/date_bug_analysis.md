# 🐛 گزارش Bug: نمایش تاریخ فردا به جای امروز

## 📝 خلاصه

**مشکل**: در صفحه `/Patient/Appointment/Available?date=1404/10/03`، تاریخ **فردا** به جای **امروز** نمایش داده می‌شود.

**تاریخ گزارش**: 2025-12-23  
**ساعت**: 21:18 (ایران)  
**شدت**: 🔴 **بحرانی** - بیمار نوبت اشتباه انتخاب می‌کند!

---

## 🔍 تحلیل مشکل

### علت اصلی

مشکل از **تبدیل اشتباه تاریخ شمسی به میلادی** و سپس **نمایش دوباره** ناشی می‌شود.

#### جریان مشکل:

1. **Frontend** تاریخ شمس `1404/10/03` در Available.cshtml

م را در URL می‌فرستد: `?date=1404/10/03`
2. **ASP.NET Model Binding** این را به عنوان `DateTime?` می‌خواند:
   ```csharp
   public async Task<ActionResult> Available(
       int? doctorId = null,
       DateTime? date = null,  // ❌ این جا مشکل است!
       int page = 1,
       int pageSize = 20)
   ```
3. **Model Binder** سعی می‌کند string `"1404/10/03"` را به `DateTime` تبدیل کند
4. چون فرمت `yyyy/MM/dd` است، تفسیر می‌کند به عنوان **میلادی** → `2404-10-03` (!)
5. این تاریخ خیلی دور در آینده است، پس چیزی نمایش نمی‌دهد
6. یا به صورت UTC parse می‌شود و وقتی به Local تبدیل می‌شود، یک روز جلو می‌رود

### کد مشکل‌دار

#### `AppointmentController.cs` خط 52-56:
```csharp
public async Task<ActionResult> Available(
    int? doctorId = null,
    DateTime? date = null,  // ❌ ASP.NET این را به میلادی تبدیل می‌کند
    int page = 1,
    int pageSize = 20)
```

#### `Available.cshtml` خط 7-26:
```csharp
// ✅ این قسمت درست است - تبدیل از میلادی به شمسی
string persianSelectedDate;

if (Model.SelectedDate == DateTime.MinValue || Model.SelectedDate == DateTime.MaxValue || 
    Model.SelectedDate.Date == DateTime.Today)
{
    persianSelectedDate = PersianDateHelper.Today;
}
else
{
    persianSelectedDate = PersianDateHelper.ToPersianDate(Model.SelectedDate);
    // ...
}
```

**مشکل**: اگر `Model.SelectedDate` اشتباه parse شده باشد، این کد کمکی نمی‌کند.

---

## ✅ راهکار

### تغییر 1: استفاده از `string` به جای `DateTime?`

```csharp
// ❌ قبل:
public async Task<ActionResult> Available(
    int? doctorId = null,
    DateTime? date = null,
    int page = 1,
    int pageSize = 20)
{
    // ...
    var selectedDate = date ?? DateTime.Today;
    // ...
}

// ✅ بعد:
public async Task<ActionResult> Available(
    int? doctorId = null,
    string date = null,  // ✅ تغییر به string
    int page = 1,
    int pageSize = 20)
{
    try
    {
        _logger.Information("درخواست نمایش نوبت‌های موجود - DoctorId: {DoctorId}, DateString: {DateString}",
            doctorId, date ?? "همه");

        // ✅ تبدیل دستی تاریخ شمسی به میلادی
        DateTime selectedDate;
        if (string.IsNullOrWhiteSpace(date))
        {
            selectedDate = DateTime.Today;
            _logger.Information("تاریخ خالی است، استفاده از امروز: {Today}", selectedDate.ToString("yyyy/MM/dd"));
        }
        else
        {
            try
            {
                // ✅ تبدیل صحیح تاریخ شمسی به میلادی
                selectedDate = PersianDateHelper.ToGregorianDate(date);
                _logger.Information("تاریخ شمسی '{PersianDate}' به میلادی '{GregorianDate}' تبدیل شد",
                    date, selectedDate.ToString("yyyy/MM/dd"));
                
                // ✅ اطمینان از اینکه فقط تاریخ است (بدون زمان)
                selectedDate = selectedDate.Date;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در تبدیل تاریخ شمسی '{PersianDate}'، استفاده از امروز", date);
                selectedDate = DateTime.Today;
            }
        }

        // بررسی اینکه تاریخ در گذشته نباشد
        if (selectedDate < DateTime.Today)
        {
            _logger.Warning("تاریخ انتخاب شده '{Date}' در گذشته است، تنظیم به امروز", selectedDate.ToString("yyyy/MM/dd"));
            selectedDate = DateTime.Today;
        }

        // ... بقیه کد
```

### تغییر 2: رفع مشکل Timezone در `PersianDateHelper.cs`

```csharp
// در خط 120-191 از PersianDateHelper.cs:

public static string ToPersianDate(DateTime dateTime)
{
    if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
    {
        _log.Warning("🔍 DateTime is MinValue or MaxValue: {DateTime}", dateTime);
        return "0000/00/00";
    }

    try
    {
        // ✅ تبدیل به Local اگر UTC است
        DateTime localDateTime = dateTime;
        if (dateTime.Kind == DateTimeKind.Utc)
        {
            localDateTime = dateTime.ToLocalTime();
            _log.Information("🔍 Converted UTC to Local: {LocalDateTime}", localDateTime);
        }
        else if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            // ✅ اگر Unspecified است، به عنوان Local در نظر می‌گیریم
            localDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            _log.Information("🔍 Specified Unspecified as Local: {LocalDateTime}", localDateTime);
        }
        
        // ✅ فقط تاریخ را در نظر می‌گیریم (بدون زمان)
        localDateTime = localDateTime.Date;
        
        // ... بقیه کد تبدیل
    }
    // ...
}
```

**مشکل**: این راهکار هنوز ممکن است کاملاً کارآمد نباشد اگر `dateTime` اشتباه از Controller بیاید.

---

## 🎯 پیاده‌سازی نهایی (توصیه شده)

### فایل: `AppointmentController.cs`

```csharp
/// <summary>
/// صفحه عمومی نمایش نوبت‌های موجود (بدون نیاز به لاگین)
/// GET: /Patient/Appointment/Available
/// </summary>
[HttpGet]
[AllowAnonymous]
public async Task<ActionResult> Available(
    int? doctorId = null,
    string date = null,  // ✅ تغییر از DateTime? به string
    int page = 1,
    int pageSize = 20)
{
    try
    {
        _logger.Information("درخواست نمایش نوبت‌های موجود - DoctorId: {DoctorId}, DateString: {DateString}",
            doctorId, date ?? "همه");

        // دریافت لیست پزشکان
        var doctorsResult = await _bookingService.GetAvailableDoctorsAsync();
        if (!doctorsResult.Success)
        {
            NotificationHelper.SetError(TempData, "خطا در دریافت لیست پزشکان");
            return View(new AvailableAppointmentsViewModel
            {
                Doctors = new List<DoctorSearchResultDto>(),
                AvailableSlots = new List<AvailableTimeSlotDto>()
            });
        }

        // ✅ تبدیل دستی تاریخ شمسی به میلادی
        DateTime selectedDate;
        if (string.IsNullOrWhiteSpace(date))
        {
            selectedDate = DateTime.Today;
            _logger.Information("تاریخ خالی است، استفاده از امروز: {Today}", selectedDate.ToString("yyyy/MM/dd"));
        }
        else
        {
            try
            {
                // ✅ تبدیل صحیح تاریخ شمسی به میلادی با PersianDateHelper
                selectedDate = PersianDateHelper.ToGregorianDate(date).Date;
                _logger.Information("تاریخ شمسی '{PersianDate}' به میلادی '{GregorianDate}' تبدیل شد",
                    date, selectedDate.ToString("yyyy/MM/dd"));
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در تبدیل تاریخ شمسی '{PersianDate}'، استفاده از امروز", date);
                selectedDate = DateTime.Today;
            }
        }

        // بررسی اینکه تاریخ در گذشته نباشد
        if (selectedDate < DateTime.Today)
        {
            _logger.Warning("تاریخ انتخاب شده '{Date}' در گذشته است، تنظیم به امروز", 
                selectedDate.ToString("yyyy/MM/dd"));
            selectedDate = DateTime.Today;
        }
        
        var viewModel = new AvailableAppointmentsViewModel
        {
            Doctors = doctorsResult.Data ?? new List<DoctorSearchResultDto>(),
            SelectedDoctorId = doctorId,
            SelectedDate = selectedDate,  // ✅ حالا این تاریخ صحیح است
            AvailableSlots = new List<AvailableTimeSlotDto>()
        };

        // اگر پزشک انتخاب شده، اسلات‌های موجود را دریافت کن
        if (doctorId.HasValue)
        {
            // ✅ استفاده از selectedDate که قبلاً parse شده
            if (selectedDate < DateTime.Today)
            {
                NotificationHelper.SetWarning(TempData, "تاریخ انتخاب شده در گذشته است. لطفاً تاریخ معتبری انتخاب کنید.");
                viewModel.SelectedDate = DateTime.Today;
            }
            else
            {
                var slotsResult = await _bookingService.GetAvailableTimeSlotsAsync(doctorId.Value, selectedDate);
                if (slotsResult.Success && slotsResult.Data != null)
                {
                    viewModel.AvailableSlots = slotsResult.Data.Where(s => s.IsAvailable).ToList();
                    _logger.Information("دریافت {Count} اسلات موجود برای تاریخ {Date}", 
                        viewModel.AvailableSlots.Count, selectedDate.ToString("yyyy/MM/dd"));
                }
                else
                {
                    _logger.Warning("خطا در دریافت اسلات‌ها: {Message}", slotsResult.Message);
                }
            }
        }

        return View(viewModel);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در نمایش نوبت‌های موجود");
        TempData["Error"] = "خطا در بارگذاری صفحه";
        return View(new AvailableAppointmentsViewModel
        {
            Doctors = new List<DoctorSearchResultDto>(),
            AvailableSlots = new List<AvailableTimeSlotDto>()
        });
    }
}
```

---

## 🧪 تست

### قبل از رفع:
- URL: `/Patient/Appointment/Available?date=1404/10/03`
- نتیجه: تاریخ فردا نمایش داده می‌شود ❌

### بعد از رفع:
- URL: `/Patient/Appointment/Available?date=1404/10/03`
- نتیجه: تاریخ امروز (1404/10/03) صحیح نمایش داده می‌شود ✅

---

## 📋 Checklist پیاده‌سازی

- [ ] تغییر parameter `date` از `DateTime?` به `string` در `AppointmentController.Available`
- [ ] اضافه کردن manual parsing با `PersianDateHelper.ToGregorianDate`
- [ ] اضافه کردن Logging برای Debug
- [ ] حذف خطوط 75-81 که قبلاً تلاش برای اصلاح می‌کردند (دیگر لازم نیست)
- [ ] تست با تاریخ امروز
- [ ] تست با تاریخ فردا
- [ ] تست با تاریخ گذشته
- [ ] تست با تاریخ خالی

---

## ⚠️ نکات مهم

1. **همیشه تاریخ شمسی را به صورت `string` در URL** ارسال و دریافت کنید
2. **هرگز از `DateTime` parameter برای تاریخ شمسی استفاده نکنید**
3. **همیشه `.Date` را بعد از parse اضافه کنید** تا زمان صفر شود
4. **Logging اضافه کنید** تا مشکلات Timezone را شناسایی کنید

---

**تهیه‌کننده**: تحلیل باگ سیستم نوبت‌دهی  
**تاریخ**: 2025-12-23
