# 🔧 رفع مشکل: نمایش اسلات‌های گذشته در SelectTime

**تاریخ:** 2026-01-07  
**مشکل:** اسلات‌های زمانی که زمانشان گذشته است (مثلاً 8:30-11:30 در حالی که الان ساعت 17 است) هنوز نمایش داده می‌شوند  
**وضعیت:** ✅ رفع شده

---

## 🐛 مشکل شناسایی شده

### علائم (Symptoms):
- ✅ تاریخ امروز است (17 دی 1404)
- ✅ اسلات‌های 8:30-11:30 نمایش داده می‌شوند
- ❌ **الان ساعت 17 است** - این اسلات‌ها باید فیلتر شوند

### علت ریشه‌ای (Root Cause):

**در `AppointmentBookingService.GetAvailableTimeSlotsAsync` (خط 439):**

```csharp
// ❌ قبل از رفع:
var slotDtos = availableSlots.Select(slot =>
{
    // فقط بررسی isBooked - بدون بررسی زمان گذشته
    var isBooked = bookedAppointments.Any(...);
    return new AvailableTimeSlotDto { IsAvailable = !isBooked, ... };
}).ToList();
```

**مشکل:**
- ❌ هیچ جایی چک نمی‌شود که آیا `slot.EndTime` گذشته است یا نه
- ❌ اسلات‌های گذشته در لیست نمایش داده می‌شوند

---

## ✅ راه‌حل اعمال شده

### تغییرات در `AppointmentBookingService.GetAvailableTimeSlotsAsync`:

**فایل:** `Services/Appointment/AppointmentBookingService.cs`  
**خط:** 437-466

```csharp
// ✅ CRITICAL FIX: دریافت زمان فعلی ایران برای فیلتر کردن اسلات‌های گذشته
var iranNow = _timeProvider.GetIranNow();
var iranToday = _timeProvider.GetIranToday();
var isToday = date.Date == iranToday.Date;

_logger.Debug("🔍 فیلتر اسلات‌های گذشته - Date: {Date}, IranToday: {IranToday}, IsToday: {IsToday}, IranNow: {IranNow}, CurrentTime: {CurrentTime}",
    date.ToString("yyyy/MM/dd"), iranToday.ToString("yyyy/MM/dd"), isToday, iranNow.ToString("yyyy/MM/dd HH:mm:ss"), iranNow.TimeOfDay);

// ✅ ENTERPRISE-GRADE: تبدیل به DTO و بررسی دسترسی‌پذیری با منطق Overlap صحیح
var slotDtos = availableSlots
    .Where(slot =>
    {
        // ✅ CRITICAL FIX: فیلتر کردن اسلات‌های گذشته (فقط برای امروز)
        if (isToday)
        {
            // اگر اسلات تمام شده است (EndTime <= CurrentTime)، آن را فیلتر می‌کنیم
            var slotEndTime = slot.EndTime;
            var currentTime = iranNow.TimeOfDay;
            
            if (slotEndTime <= currentTime)
            {
                _logger.Debug("⏰ اسلات گذشته فیلتر شد - Slot: {StartTime}-{EndTime}, CurrentTime: {CurrentTime}",
                    slot.StartTime, slot.EndTime, currentTime);
                return false; // اسلات گذشته را فیلتر می‌کنیم
            }
        }
        
        return true; // اسلات معتبر است
    })
    .Select(slot =>
    {
        // منطق Overlap و ساخت DTO...
    })
    .ToList();
```

---

## 🔍 منطق فیلتر

### شرایط فیلتر:

1. **اگر تاریخ = امروز (`isToday = true`):**
   - ✅ چک می‌کنیم: `slot.EndTime <= currentTime`
   - ✅ اگر `true` باشد → اسلات فیلتر می‌شود (نمایش داده نمی‌شود)

2. **اگر تاریخ > امروز (آینده):**
   - ✅ همه اسلات‌ها معتبر هستند (فیلتر نمی‌شوند)

### مثال:

**سناریو:**
- تاریخ: امروز (17 دی 1404)
- زمان فعلی: 17:00
- اسلات: 8:30-11:30

**نتیجه:**
- ✅ `isToday = true`
- ✅ `slotEndTime = 11:30`
- ✅ `currentTime = 17:00`
- ✅ `11:30 <= 17:00` → `true`
- ✅ **اسلات فیلتر می‌شود** (نمایش داده نمی‌شود)

---

## 📋 چک‌لیست تست

### قبل از تست:

- [ ] ✅ کد تغییر یافته است
- [ ] ✅ Application Restart شده است
- [ ] ✅ لاگ‌ها فعال هستند

### تست:

1. **تست 1: امروز - اسلات‌های گذشته**
   - تاریخ: امروز
   - زمان فعلی: 17:00
   - اسلات: 8:30-11:30
   - **انتظار:** اسلات نمایش داده نمی‌شود ✅

2. **تست 2: امروز - اسلات‌های آینده**
   - تاریخ: امروز
   - زمان فعلی: 10:00
   - اسلات: 11:00-11:15
   - **انتظار:** اسلات نمایش داده می‌شود ✅

3. **تست 3: آینده - همه اسلات‌ها**
   - تاریخ: فردا
   - اسلات: 8:30-11:30
   - **انتظار:** اسلات نمایش داده می‌شود ✅

---

## 🔍 لاگ‌های مورد انتظار

### لاگ فیلتر:

```
🔍 فیلتر اسلات‌های گذشته - Date: 2026/01/07, IranToday: 2026/01/07, IsToday: True, IranNow: 2026/01/07 17:00:00, CurrentTime: 17:00:00
⏰ اسلات گذشته فیلتر شد - Slot: 08:30:00-11:30:00, CurrentTime: 17:00:00
```

---

## ⚠️ نکات مهم

### 1. استفاده از `GetIranNow()`

**چرا `GetIranNow()` و نه `DateTime.Now`؟**
- ✅ `GetIranNow()` زمان ایران را برمی‌گرداند (UTC+3:30)
- ✅ `DateTime.Now` زمان محلی سرور را برمی‌گرداند (ممکن است متفاوت باشد)
- ✅ برای سیستم‌های درمانی، استفاده از زمان ایران الزامی است

### 2. فیلتر بر اساس `EndTime`

**چرا `EndTime` و نه `StartTime`؟**
- ✅ اگر `EndTime <= currentTime` → اسلات تمام شده است
- ✅ اگر `StartTime <= currentTime` اما `EndTime > currentTime` → اسلات هنوز در حال اجرا است (نباید فیلتر شود)

### 3. فقط برای امروز

**چرا فقط برای `isToday`؟**
- ✅ برای تاریخ‌های آینده، همه اسلات‌ها معتبر هستند
- ✅ فقط برای امروز باید اسلات‌های گذشته را فیلتر کنیم

---

## 🔗 فایل‌های مرتبط

- `Services/Appointment/AppointmentBookingService.cs` - تغییرات اعمال شده
- `Infrastructure/ITimeProvider.cs` - استفاده از `GetIranNow()`
- `Areas/Patient/Controllers/AppointmentBookingController.cs` - فراخوانی Service

---

## ✅ نتیجه

**مشکل رفع شد:**
- ✅ اسلات‌های گذشته برای امروز فیلتر می‌شوند
- ✅ اسلات‌های آینده نمایش داده می‌شوند
- ✅ لاگ‌های کامل برای Debug

**مرحله بعدی:** Application را Restart کنید و تست کنید.

---

**تاریخ رفع:** 2026-01-07  
**وضعیت:** ✅ تکمیل شده

