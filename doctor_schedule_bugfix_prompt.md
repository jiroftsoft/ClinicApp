# 🩺 پرامپت رفع باگ DoctorSchedule - تولید اسلات خارج از بازه زمانی

**نقش**: شما **دستیار هوش مصنوعی** هستید که طبق قراردادهای "AI_ASSISTANT_MASTER_CONTRACT" و "DEBUGGING_SPECIALIST_CONTRACT" عمل می‌کنید. باید به صورت همزمان در نقش‌های زیر عمل کنید:

1. **معمار ارشد نرم‌افزار** (Senior Software Architect)
2. **متخصص بررسی کد** (Expert Code Reviewer)
3. **متخصص ASP.NET MVC** (ASP.NET MVC Specialist)
4. **متخصص امنیت** (Security Expert)
5. **متخصص سیستم‌های پزشکی** (Medical Systems Expert) - **بسیار مهم برای این وظیفه**
6. **متخصص تجربه کاربری** (UX Expert)
7. **متخصص پایگاه داده** (Database Expert)

**هدف**: رفع باگ بحرانی در ماژول `DoctorSchedule` که در آن **اسلات‌های زمانی خارج از بازه زمانی انتخاب شده تولید می‌شوند** و بهینه‌سازی ماژول.

---

## 🛡️ مرحله 0: بررسی امنیتی AI Guard (الزامی)

قبل از نوشتن هر کدی، بررسی کنید:

1. **جلوگیری از از دست رفتن داده**: آیا این رفع باگ خطر حذف نوبت‌های معتبر گذشته را دارد؟ (باید از Soft Delete استفاده شود).
2. **حریم خصوصی**: آیا داده‌های پزشک/بیمار را در لاگ‌ها افشا می‌کنیم؟ (باید PII را ماسک کنیم).
3. **استانداردهای پزشکی**: آیا برنامه کاری اجازه تداخل "غیرممکن" را می‌دهد؟
4. **امنیت**: آیا `DoctorId` در برابر کاربر فعلی اعتبارسنجی می‌شود؟
5. **تداخل با زمان‌های مسدود شده**: آیا اسلات‌های تولید شده با `ScheduleExceptions` (زمان‌های بلاک شده) تداخل دارند؟

> 🛑 **توقف سخت**: اگر هر یک از این موارد نقض شود، متوقف شوید و توضیح بخواهید.

---

## 🐞 مشکل: اسلات‌های خارج از بازه زمانی

**علائم**: هنگامی که پزشک برنامه کاری تنظیم می‌کند (مثلاً 09:00 - 12:00)، گاهی اوقات اسلات‌ها برای 12:00-12:30 یا زمان‌های دیگر خارج از این بازه تولید می‌شوند.

**فایل هدف**: `Repositories\ClinicAdmin\DoctorScheduleRepository.cs`
**متدهای کلیدی**: 
- `GenerateAndSaveTimeSlotsAsync` (خطوط 1150-1438)
- `GenerateSlotsForDateAsync` (خطوط 1551-1679)
- `ShouldDeleteOldSlot` (خطوط 1448-1535)

---

## 🔬 پروتکل متخصص دیباگ (تحلیل سطح 4)

اجرای **فرآیند 6 مرحله‌ای دیباگ**:

### 1. شناسایی و دسته‌بندی
- **نوع**: خطای منطقی / شرایط مرزی (Boundary Condition)
- **شدت**: بالا (یکپارچگی داده)
- **دامنه**: `DoctorScheduleRepository`

### 2. تحلیل علت ریشه‌ای (5 چرا)

#### فرضیه 1: مشکل در شرط حلقه `while (currentTime < endTime)`
**چرا؟** - آیا شرط حلقه `currentTime < endTime` اجازه یک تکرار اضافی را می‌دهد؟

**تحلیل دقیق**:
```csharp
// خط 1595: while (currentTime < endTime)
// خط 1597: var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(doctorSchedule.AppointmentDuration));
// خط 1600: if (slotEndTime <= endTime)
// خط 1669: currentTime = slotEndTime;
```

**مشکل احتمالی**: 
- اگر `currentTime = 11:45` و `endTime = 12:00` و `AppointmentDuration = 30` دقیقه باشد
- `slotEndTime = 12:15` خواهد بود
- شرط `slotEndTime <= endTime` (12:15 <= 12:00) = false
- اما حلقه ادامه می‌یابد و `currentTime = 12:15` می‌شود
- در تکرار بعدی، اگر `currentTime < endTime` (12:15 < 12:00) = false، حلقه متوقف می‌شود
- **اما**: اگر منطق دیگری وجود داشته باشد که اسلات را قبل از بررسی `slotEndTime <= endTime` ایجاد کند، مشکل رخ می‌دهد

#### فرضیه 2: مشکل در `ShouldDeleteOldSlot` - عدم حذف اسلات‌های خارج از بازه
**چرا؟** - آیا `ShouldDeleteOldSlot` در هنگام به‌روزرسانی‌ها، اسلات‌های "خارج از بازه" را به عنوان حذف‌شده علامت‌گذاری نمی‌کند؟

**تحلیل دقیق**:
```csharp
// خط 1508-1510: بررسی اینکه اسلات درون TimeRange است
if (oldSlot.StartTime >= timeRange.StartTime &&
    oldSlot.EndTime <= timeRange.EndTime &&
    oldSlot.Duration == doctorSchedule.AppointmentDuration)
```

**مشکل احتمالی**:
- اگر `TimeRange` تغییر کرده باشد (مثلاً از 09:00-12:00 به 09:00-11:30)
- اسلات قدیمی 11:30-12:00 باید حذف شود
- اما اگر `ShouldDeleteOldSlot` فقط بررسی کند که آیا اسلات در **هر** TimeRange معتبری قرار دارد یا نه
- ممکن است اسلات‌های خارج از بازه جدید را پیدا نکند

#### فرضیه 3: مشکل در مقایسه‌های `TimeSpan` - مسائل دقت
**چرا؟** - آیا مقایسه‌های `TimeSpan` از مشکلات دقت رنج می‌برند؟

**تحلیل دقیق**:
```csharp
// خط 1590: var currentTime = timeRange.StartTime; // TimeSpan
// خط 1597: var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(doctorSchedule.AppointmentDuration));
// خط 1600: if (slotEndTime <= endTime) // مقایسه TimeSpan
```

**مشکل احتمالی**:
- `TimeSpan` از `Ticks` استفاده می‌کند (100 نانوثانیه)
- اگر `AppointmentDuration` به صورت دقیقه باشد و تبدیل به `TimeSpan` شود
- ممکن است مشکلات گرد کردن رخ دهد
- اما این احتمال کم است چون `TimeSpan.FromMinutes` دقیق است

#### فرضیه 4: مشکل در بارگذاری `TimeRanges` - Lazy Loading یا Eager Loading
**چرا؟** - آیا `TimeRanges` به درستی بارگذاری می‌شوند؟

**تحلیل دقیق**:
```csharp
// خط 1191-1193: Include برای بارگذاری TimeRanges
.Include(ds => ds.WorkDays)
.Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
```

**مشکل احتمالی**:
- اگر `TimeRanges` null باشد یا به درستی بارگذاری نشده باشد
- ممکن است اسلات‌ها بر اساس داده‌های ناقص تولید شوند
- کد در خطوط 1203-1210 این مشکل را برطرف می‌کند، اما باید بررسی شود

#### فرضیه 5: مشکل در بررسی `ScheduleExceptions` - تداخل با زمان‌های بلاک شده
**چرا؟** - آیا اسلات‌های تولید شده با `ScheduleExceptions` (زمان‌های بلاک شده) تداخل دارند؟

**تحلیل دقیق**:
```csharp
// خط 1603-1610: بررسی ScheduleExceptions جزئی
var hasPartialException = allScheduleExceptions != null && allScheduleExceptions.Any(se =>
    se != null &&
    se.StartDate.Date == dateOnly &&
    (se.EndDate == null || se.EndDate.Value.Date == dateOnly) &&
    se.StartTime.HasValue &&
    se.EndTime.HasValue &&
    se.StartTime.Value < slotEndTime &&
    se.EndTime.Value > currentTime);
```

**مشکل احتمالی**:
- اگر منطق بررسی `ScheduleExceptions` ناقص باشد
- ممکن است اسلات‌هایی که باید بلاک شوند، تولید شوند
- این یک مشکل امنیتی و استاندارد پزشکی است

### 3. تحلیل وابستگی‌ها
- بررسی فراخوانی‌ها: `AddDoctorScheduleAsync`, `UpdateDoctorScheduleAsync`
- بررسی تأثیر بر `AppointmentBookingService`
- بررسی تأثیر بر `BlockTimeRangeForDoctorAsync` (زمان‌های بلاک شده)

### 4. رفع اتمیک
- **محدودیت**: رفع منطق **داخل** متدهای Repository بدون تغییر قرارداد عمومی در صورت امکان
- **نیازمندی**: استفاده دقیق از مقایسه `TimeSpan`
- **تغییر کد**: بررسی منطق `slotEndTime <= endTime`
- **استانداردهای پزشکی**: اطمینان از عدم تداخل با زمان‌های بلاک شده
- **Soft Delete**: استفاده از `IsDeleted` به جای حذف فیزیکی

### 5. تست و اعتبارسنجی
- **نیازمندی UnitTest**: پیشنهاد یک مورد تست با:
    - بازه: 08:00 - 08:30
    - مدت زمان: 20 دقیقه
    - مورد انتظار: اسلات 08:00-08:20
    - **بررسی باگ**: اطمینان از اینکه 08:20-08:40 تولید نمی‌شود
    - **بررسی استانداردهای پزشکی**: اطمینان از عدم تداخل با زمان‌های بلاک شده

### 6. گزارش‌دهی حرفه‌ای
- خلاصه آنچه رفع شد
- گزارش تغییرات با شواهد (مسیر فایل + شماره خط)

---

## 📝 دستورالعمل‌های پیاده‌سازی (برای Cursor)

بازسازی `DoctorScheduleRepository.cs` برای:

### 1. رفع منطق حلقه در `GenerateSlotsForDateAsync`

**مشکل**: حلقه `while (currentTime < endTime)` ممکن است اجازه تولید اسلات خارج از بازه را بدهد.

**راه‌حل اتمیک**:
```csharp
// ✅ رفع: بررسی دقیق‌تر قبل از ایجاد اسلات
while (currentTime < endTime)
{
    var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(doctorSchedule.AppointmentDuration));
    
    // ✅ بررسی اولیه: اگر slotEndTime > endTime، حلقه را متوقف کن
    if (slotEndTime > endTime)
    {
        System.Diagnostics.Debug.WriteLine($"[GenerateSlotsForDateAsync] ⚠️ اسلات خارج از TimeRange است - StartTime: {currentTime}, SlotEndTime: {slotEndTime}, TimeRangeEnd: {endTime} - حلقه متوقف می‌شود");
        break; // ✅ توقف حلقه به جای ادامه
    }
    
    // ✅ بررسی دقیق: اسلات باید کاملاً درون TimeRange باشد
    if (slotEndTime <= endTime)
    {
        // ... ادامه منطق موجود
    }
}
```

**تغییرات مورد نیاز**:
- **خط 1595-1670**: بهبود منطق حلقه
- **افزودن `break`** به جای ادامه حلقه وقتی `slotEndTime > endTime`
- **بررسی دوگانه**: هم قبل از حلقه و هم در شرط `if`

### 2. رفع منطق در `ShouldDeleteOldSlot`

**مشکل**: اسلات‌های قدیمی که دیگر در `TimeRange` معتبر نیستند، ممکن است حذف نشوند.

**راه‌حل اتمیک**:
```csharp
// ✅ رفع: بررسی دقیق‌تر برای اطمینان از حذف اسلات‌های خارج از بازه
private bool ShouldDeleteOldSlot(DoctorTimeSlot oldSlot, DoctorSchedule doctorSchedule, List<ScheduleException> scheduleExceptions)
{
    // ✅ Null Safety: بررسی null بودن ورودی‌ها
    if (oldSlot == null || doctorSchedule == null)
    {
        return false; // اگر داده‌ها null باشند، حذف نکن
    }
    
    // ✅ بررسی تعطیلات رسمی
    if (IsPersianHoliday(oldSlot.AppointmentDate))
    {
        return true; // حذف شود
    }
    
    // ✅ بررسی ScheduleExceptions (استفاده از لیست از پیش بارگذاری شده)
    var slotDate = oldSlot.AppointmentDate.Date;
    var hasException = scheduleExceptions != null && scheduleExceptions.Any(se =>
        se != null &&
        se.StartDate.Date <= slotDate &&
        (se.EndDate == null || se.EndDate.Value.Date >= slotDate) &&
        (!se.StartTime.HasValue || !se.EndTime.HasValue || // استثنای تمام روز
         (se.StartTime.Value <= oldSlot.StartTime && se.EndTime.Value >= oldSlot.EndTime))); // استثنای جزئی
    
    if (hasException)
    {
        return true; // حذف شود - در زمان بلاک شده قرار دارد
    }
    
    // ✅ بررسی دقیق: اسلات باید در یک TimeRange معتبر قرار داشته باشد
    var dayOfWeek = (int)oldSlot.AppointmentDate.DayOfWeek;
    var workDays = doctorSchedule.WorkDays?
        .Where(wd => wd != null && 
                    wd.DayOfWeek == dayOfWeek && 
                    wd.IsActive && 
                    !wd.IsDeleted)
        .ToList();
    
    if (workDays == null || !workDays.Any())
    {
        return true; // حذف شود - هیچ WorkDay فعالی برای این روز وجود ندارد
    }
    
    bool isSlotValid = false;
    foreach (var workDay in workDays)
    {
        if (workDay?.TimeRanges == null)
            continue;
        
        var activeTimeRanges = workDay.TimeRanges
            .Where(tr => tr != null && tr.IsActive && !tr.IsDeleted)
            .ToList();
        
        foreach (var timeRange in activeTimeRanges)
        {
            if (timeRange == null)
                continue;
            
            // ✅ بررسی دقیق: اسلات باید کاملاً درون TimeRange باشد
            // ✅ StartTime اسلات باید >= StartTime TimeRange
            // ✅ EndTime اسلات باید <= EndTime TimeRange
            // ✅ Duration اسلات باید برابر با AppointmentDuration باشد
            if (oldSlot.StartTime >= timeRange.StartTime &&
                oldSlot.EndTime <= timeRange.EndTime &&
                oldSlot.Duration == doctorSchedule.AppointmentDuration)
            {
                // ✅ این اسلات در یک TimeRange معتبر قرار دارد
                isSlotValid = true;
                System.Diagnostics.Debug.WriteLine($"[ShouldDeleteOldSlot] ✅ اسلات {oldSlot.TimeSlotId} معتبر است - StartTime: {oldSlot.StartTime}, EndTime: {oldSlot.EndTime}, TimeRange: {timeRange.StartTime}-{timeRange.EndTime}");
                break; // نیازی به بررسی بیشتر نیست
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ShouldDeleteOldSlot] ⚠️ اسلات {oldSlot.TimeSlotId} در TimeRange {timeRange.StartTime}-{timeRange.EndTime} قرار ندارد - StartTime: {oldSlot.StartTime}, EndTime: {oldSlot.EndTime}, Duration: {oldSlot.Duration}, ExpectedDuration: {doctorSchedule.AppointmentDuration}");
            }
        }
        
        if (isSlotValid)
            break; // اگر اسلات معتبر است، نیازی به بررسی WorkDay های دیگر نیست
    }
    
    // ✅ اگر اسلات در هیچ TimeRange معتبری قرار نگرفت، باید حذف شود
    if (!isSlotValid)
    {
        System.Diagnostics.Debug.WriteLine($"[ShouldDeleteOldSlot] 🗑️ اسلات {oldSlot.TimeSlotId} حذف می‌شود - در هیچ TimeRange معتبری قرار ندارد");
        return true; // این اسلات دیگر معتبر نیست
    }
    
    return false; // این اسلات هنوز معتبر است
}
```

**تغییرات مورد نیاز**:
- **خط 1448-1535**: بهبود منطق `ShouldDeleteOldSlot`
- **افزودن بررسی دقیق‌تر** برای اطمینان از حذف اسلات‌های خارج از بازه
- **بررسی ScheduleExceptions** برای اطمینان از عدم تداخل با زمان‌های بلاک شده

### 3. اعمال استانداردهای پزشکی

**نیازمندی‌ها**:
- اطمینان از عدم تداخل با زمان‌های بلاک شده (`ScheduleExceptions`)
- رعایت Soft Delete (استفاده از `IsDeleted` به جای حذف فیزیکی)
- بررسی تعطیلات رسمی ایران
- بررسی نوبت‌های رزرو شده قبل از حذف اسلات

**راه‌حل**:
```csharp
// ✅ در GenerateSlotsForDateAsync: بررسی ScheduleExceptions قبل از ایجاد اسلات
var hasPartialException = allScheduleExceptions != null && allScheduleExceptions.Any(se =>
    se != null &&
    se.StartDate.Date == dateOnly &&
    (se.EndDate == null || se.EndDate.Value.Date == dateOnly) &&
    se.StartTime.HasValue &&
    se.EndTime.HasValue &&
    se.StartTime.Value < slotEndTime &&
    se.EndTime.Value > currentTime);

if (!hasPartialException)
{
    // ✅ فقط اگر در زمان بلاک شده نیست، اسلات ایجاد کن
    // ... ادامه منطق
}
```

### 4. بهینه‌سازی عملکرد

**نیازمندی‌ها**:
- جلوگیری از N+1 Query (قبلاً تا حدی انجام شده، اما دوباره بررسی شود)
- استفاده از `AsNoTracking` در صورت مناسب
- استفاده از Batch Operations برای حذف اسلات‌های قدیمی

**راه‌حل**:
```csharp
// ✅ استفاده از Batch Operations برای حذف اسلات‌های قدیمی
if (slotsToDelete.Any())
{
    // ✅ Soft Delete: استفاده از IsDeleted به جای حذف فیزیکی
    foreach (var slot in slotsToDelete)
    {
        slot.IsDeleted = true;
        slot.DeletedAt = DateTime.Now;
        slot.DeletedByUserId = doctorSchedule.UpdatedByUserId ?? doctorSchedule.CreatedByUserId;
    }
    
    // ✅ به‌روزرسانی دسته‌ای
    _context.DoctorTimeSlots.UpdateRange(slotsToDelete);
}
```

### 5. لاگ‌گذاری

**نیازمندی‌ها**:
- جایگزینی `Debug.WriteLine` با `ILogger` تزریق شده (در صورت امکان)
- لاگ "خلاصه تولید اسلات" (مثلاً "5 اسلات برای 2024-01-01 تولید شد")
- ماسک کردن PII در لاگ‌ها (کد ملی، شماره موبایل، و غیره)

**راه‌حل**:
```csharp
// ✅ استفاده از ILogger به جای Debug.WriteLine (در صورت امکان)
_logger.LogInformation("تولید {Count} اسلات برای تاریخ {Date} برای پزشک {DoctorId}", 
    slotsForDate.Count, 
    dateOnly.ToString("yyyy/MM/dd"), 
    doctorId); // ✅ DoctorId را ماسک نکنیم (نیاز به شناسایی دارد)
```

---

## 🚀 دستور اجرا

شروع با تحلیل `GenerateSlotsForDateAsync`. اگر نقص منطقی پیدا کردید، فوراً با رویکرد **Commit اتمیک** رفع کنید. سپس، بررسی کنید که `ShouldDeleteOldSlot` اطمینان حاصل می‌کند که اسلات‌های یتیم پاک می‌شوند.

**محدودیت‌ها**:
- استفاده از `decimal` برای هر محاسبه مالی (در صورت وجود)
- استفاده از `PersianDateHelper` برای هر تاریخ لاگ
- رعایت `CRITICAL-FINANCIAL-MODULE-CONTRACT` در صورت لمس هر رابطه پرداخت (احتمالاً اینجا نیست، اما بررسی کنید)
- **استانداردهای پزشکی**: اطمینان از عدم تداخل با زمان‌های بلاک شده
- **Soft Delete**: استفاده از `IsDeleted` به جای حذف فیزیکی

---

## 📋 چک‌لیست رفع باگ

### ✅ قبل از شروع:
- [ ] **فایل را کامل اسکن کنید**: `Repositories\ClinicAdmin\DoctorScheduleRepository.cs`
- [ ] **مشکل را دسته‌بندی کنید**: خطای منطقی / شرایط مرزی
- [ ] **اولویت‌بندی مشکلات**: اول `GenerateSlotsForDateAsync`، سپس `ShouldDeleteOldSlot`
- [ ] **بررسی وابستگی‌ها**: `AddDoctorScheduleAsync`, `UpdateDoctorScheduleAsync`, `AppointmentBookingService`

### ✅ حین رفع:
- [ ] **علت ریشه‌ای را پیدا کنید**: استفاده از تحلیل 5 چرا
- [ ] **تغییرات اتمیک اعمال کنید**: فقط تغییرات لازم
- [ ] **تست کنید که کار می‌کند**: Build → سبز
- [ ] **عوارض جانبی بررسی کنید**: آیا تغییرات بر سایر بخش‌ها تأثیر می‌گذارد؟

### ✅ بعد از رفع:
- [ ] **گزارش کامل بنویسید**: شامل شواهد (مسیر فایل + شماره خط)
- [ ] **اقدامات پیشگیرانه پیشنهاد کنید**: Unit Tests، Code Review
- [ ] **مستندات به‌روزرسانی کنید**: اگر لازم است
- [ ] **تیم را آگاه کنید**: اگر تغییرات مهم است

---

## 🧪 تست‌های پیشنهادی

### تست 1: تولید اسلات در بازه زمانی دقیق
```csharp
// ورودی:
// - TimeRange: 08:00 - 08:30
// - AppointmentDuration: 20 دقیقه
// - انتظار: اسلات 08:00-08:20
// - بررسی باگ: اطمینان از اینکه 08:20-08:40 تولید نمی‌شود
```

### تست 2: تولید اسلات با زمان بلاک شده
```csharp
// ورودی:
// - TimeRange: 09:00 - 12:00
// - ScheduleException: 10:00 - 11:00 (بلاک شده)
// - AppointmentDuration: 30 دقیقه
// - انتظار: اسلات 09:00-09:30, 09:30-10:00, 11:00-11:30, 11:30-12:00
// - بررسی باگ: اطمینان از اینکه اسلات‌های 10:00-11:00 تولید نمی‌شوند
```

### تست 3: حذف اسلات قدیمی خارج از بازه
```csharp
// ورودی:
// - اسلات قدیمی: 11:30-12:00
// - TimeRange جدید: 09:00 - 11:30
// - انتظار: اسلات 11:30-12:00 باید حذف شود (Soft Delete)
// - بررسی باگ: اطمینان از اینکه اسلات حذف می‌شود
```

---

## 📊 گزارش خروجی مورد انتظار

پس از رفع باگ، یک گزارش Markdown با ساختار زیر ایجاد کنید:

```markdown
# 🐛 گزارش رفع باگ: تولید اسلات خارج از بازه زمانی

## 📋 خلاصه اجرایی
- **مشکل**: اسلات‌های زمانی خارج از بازه زمانی انتخاب شده تولید می‌شدند
- **علت ریشه‌ای**: [توضیح بر اساس تحلیل 5 چرا]
- **راه‌حل**: [توضیح تغییرات اعمال شده]

## 🔍 شواهد
- **فایل**: `Repositories\ClinicAdmin\DoctorScheduleRepository.cs`
- **خطوط تغییر یافته**: [شماره خطوط]
- **متدهای تغییر یافته**: `GenerateSlotsForDateAsync`, `ShouldDeleteOldSlot`

## 💡 تحلیل ریشه‌ای (5 چرا)
1. **چرا؟** - [توضیح]
2. **چرا؟** - [توضیح]
3. **چرا؟** - [توضیح]
4. **چرا؟** - [توضیح]
5. **چرا؟** - [توضیح]

## ✅ راه‌حل اعمال شده
- [توضیح تغییرات]

## 🧪 تست‌ها
- [نتایج تست‌ها]

## ⚠️ تأثیر/رگرسیون
- [ریسک‌های احتمالی]

## 🔄 Rollback
- [گام‌های بازگشت]
```

---

**امضا**: *دستیار هوش مصنوعی ارشد*

**تاریخ ایجاد**: 2025-01-XX
**نسخه**: 2.0
**وضعیت**: آماده استفاده در Cursor

---

*این پرامپت طبق قراردادهای Bugfix-Master-Contract.md، DEBUGGING_SPECIALIST_CONTRACT.md، و استانداردهای پزشکی ClinicApp تهیه شده است.*

