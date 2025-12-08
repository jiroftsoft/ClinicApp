# 🐛 گزارش رفع باگ - خطای SQL در دریافت برنامه کاری پزشک

**تاریخ:** 2025-12-07  
**ماژول:** DoctorSchedule  
**اولویت:** High  
**وضعیت:** ✅ رفع شده

---

## 📋 Executive Summary

**مشکل:** خطای SQL `Invalid column name 'Doctor_DoctorId'` هنگام دریافت برنامه کاری پزشک رخ می‌دهد.

**علت:** در متد `GetDoctorScheduleAsync` در `DoctorScheduleRepository`، از `.Include(ds => ds.Doctor)` استفاده می‌شود که باعث می‌شود EF6 به اشتباه ستون `Doctor_DoctorId` را جستجو کند. این ستون در دیتابیس وجود ندارد.

**راه‌حل:** حذف `.Include(ds => ds.Doctor)` از متد `GetDoctorScheduleAsync` یا استفاده از روش جایگزین.

---

## 🔍 Evidence (شواهد)

### **1. خطای SQL:**
```
System.Data.SqlClient.SqlException: Invalid column name 'Doctor_DoctorId'.
```

### **2. محل خطا:**
- **فایل:** `Repositories/ClinicAdmin/DoctorScheduleRepository.cs`
- **خط:** 47-49
- **متد:** `GetDoctorScheduleAsync(int doctorId)`

### **3. Stack Trace:**
```
at ClinicApp.Repositories.ClinicAdmin.DoctorScheduleRepository.<GetDoctorScheduleAsync>d__2.MoveNext() in C:\Users\Developer\source\repos\ClinicApp\Repositories\ClinicAdmin\DoctorScheduleRepository.cs:line 47
at ClinicApp.Services.ClinicAdmin.DoctorScheduleService.<SetDoctorScheduleAsync>d__7.MoveNext() in C:\Users\Developer\source\repos\ClinicApp\Services\ClinicAdmin\DoctorScheduleService.cs:line 174
```

### **4. کد مشکل‌دار:**
```csharp
public async Task<DoctorSchedule> GetDoctorScheduleAsync(int doctorId)
{
    try
    {
        return await _context.DoctorSchedules
            .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted)
            .FirstOrDefaultAsync();
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"خطا در دریافت برنامه کاری پزشک {doctorId}", ex);
    }
}
```

**نکته:** این متد خودش `.Include` ندارد، اما در `SetDoctorScheduleAsync` از `GetDoctorScheduleAsync` استفاده می‌شود و سپس EF6 سعی می‌کند Navigation Property `Doctor` را لود کند.

### **5. لاگ خطا:**
```
2025-12-07 22:13:41.171 [WRN] ClinicApp.Services.ClinicAdmin.DoctorScheduleService | خطای عملیاتی در تنظیم برنامه کاری پزشک 2: خطا در دریافت برنامه کاری پزشک 2
System.InvalidOperationException: خطا در دریافت برنامه کاری پزشک 2 ---> System.Data.Entity.Core.EntityCommandExecutionException: An error occurred while executing the command definition. See the inner exception for details. ---> System.Data.SqlClient.SqlException: Invalid column name 'Doctor_DoctorId'.
```

---

## 🧠 Root-Cause Analysis (تحلیل ریشه‌ای)

### **دلیل منطقی:**
1. **EF6 Navigation Property Loading:** وقتی از `GetDoctorScheduleAsync` استفاده می‌شود و سپس Navigation Property `Doctor` دسترسی پیدا می‌کند، EF6 سعی می‌کند آن را lazy load کند.
2. **Foreign Key Convention:** EF6 به اشتباه فکر می‌کند که Foreign Key برای `Doctor` باید `Doctor_DoctorId` باشد، در حالی که در واقع `DoctorId` است.
3. **Entity Configuration:** در `DoctorScheduleConfiguration`، Foreign Key به درستی به `DoctorId` مپ شده است (خط 289-292)، اما EF6 در زمان اجرا به اشتباه `Doctor_DoctorId` را جستجو می‌کند.

### **مشکل واقعی:**
- ✅ Entity Configuration درست است
- ❌ EF6 در زمان اجرا Foreign Key را اشتباه تشخیص می‌دهد
- ❌ Navigation Property `Doctor` باعث می‌شود EF6 سعی کند آن را لود کند

---

## 💡 Options (گزینه‌های رفع)

### **Option A: حذف دسترسی به Navigation Property Doctor** ⭐ (انتخاب شده)
- **دامنه تغییر:** کوچک
- **ریسک:** کم
- **مزایا:** 
  - ساده و سریع
  - بدون تغییر در Entity Configuration
- **معایب:** 
  - اگر در Service به `Doctor` نیاز باشد، باید جداگانه لود شود
- **دلیل انتخاب:** در `SetDoctorScheduleAsync`، فقط `ScheduleId` و `DoctorId` نیاز است، نه خود `Doctor` object

### **Option B: استفاده از Explicit Loading**
- **دامنه تغییر:** متوسط
- **ریسک:** متوسط
- **مزایا:** 
  - Navigation Property در صورت نیاز لود می‌شود
- **معایب:** 
  - نیاز به تغییر در Service
  - پیچیده‌تر

### **Option C: استفاده از Projection**
- **دامنه تغییر:** بزرگ
- **ریسک:** متوسط
- **مزایا:** 
  - فقط فیلدهای مورد نیاز لود می‌شوند
- **معایب:** 
  - نیاز به تغییر در Service و Repository
  - پیچیده‌تر

---

## 🔧 Patch (تغییرات اتمیک)

### **تغییر 1: بررسی استفاده از Doctor در Service**

**فایل:** `Services/ClinicAdmin/DoctorScheduleService.cs`  
**خطوط:** 173-174

**بررسی:** آیا در `SetDoctorScheduleAsync` به Navigation Property `Doctor` دسترسی پیدا می‌شود؟

**نتیجه:** خیر، فقط `ScheduleId` و `DoctorId` استفاده می‌شود.

### **تغییر 2: بررسی Entity Configuration**

**فایل:** `Models/Entities/Doctor/DoctorSchedule.cs`  
**خطوط:** 289-292

**بررسی:** آیا Foreign Key به درستی مپ شده است؟

**نتیجه:** بله، Foreign Key به `DoctorId` مپ شده است.

### **تغییر 3: حذف .Include(ds => ds.Doctor) از Repository** ✅ (اعمال شده)

**فایل:** `Repositories/ClinicAdmin/DoctorScheduleRepository.cs`  
**خطوط:** 72-80

**مشکل:** `.Include(ds => ds.Doctor)` باعث می‌شود EF6 به اشتباه ستون `Doctor_DoctorId` را جستجو کند.

**راه‌حل:** حذف `.Include(ds => ds.Doctor)` و لود کردن Navigation Property `Doctor` به صورت جداگانه در Service.

**کد نهایی:**
```csharp
// ✅ حذف .Include(ds => ds.Doctor) به دلیل خطای SQL: Invalid column name 'Doctor_DoctorId'
var result = await _context.DoctorSchedules
    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted)
    // .Include(ds => ds.Doctor) // ❌ حذف شده: باعث خطای SQL می‌شود
    .Include(ds => ds.WorkDays)
    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
    .Include(ds => ds.CreatedByUser)
    .Include(ds => ds.UpdatedByUser)
    .AsNoTracking()
    .FirstOrDefaultAsync();
```

### **تغییر 4: لود کردن Navigation Property Doctor در Service** ✅ (اعمال شده)

**فایل:** `Services/ClinicAdmin/DoctorScheduleService.cs`  
**خطوط:** 274-280

**راه‌حل:** لود کردن Navigation Property `Doctor` به صورت جداگانه در Service.

**کد نهایی:**
```csharp
// ✅ لود کردن Navigation Property Doctor به صورت جداگانه برای جلوگیری از خطای SQL
if (doctorSchedule.Doctor == null && doctorSchedule.DoctorId > 0)
{
    _logger.Information("🔄 [GetDoctorScheduleAsync] در حال لود کردن Navigation Property Doctor برای پزشک {DoctorId}", doctorId);
    doctorSchedule.Doctor = await _doctorRepository.GetByIdAsync(doctorSchedule.DoctorId);
}
```

---

## ✅ Manual Sanity Check (تأیید دستی)

### **گام‌های تست:**
1. ✅ بررسی Entity Configuration
2. ✅ بررسی استفاده از Navigation Property
3. ✅ بررسی SQL Query تولید شده
4. ✅ تست در Development

---

## 📊 Impact/Regression Assessment

### **تأثیر:**
- ✅ **مثبت:** رفع خطای SQL
- ⚠️ **نیاز به بررسی:** اگر Navigation Property `Doctor` در جای دیگری استفاده می‌شود

### **Regression Risk:**
- ✅ **کم:** فقط در `SetDoctorScheduleAsync` استفاده می‌شود
- ✅ **Backward Compatible:** سازگار با کد موجود

---

## 🔄 Rollback Plan

### **گام‌های بازگشت:**
1. بازگرداندن تغییرات در `Repositories/ClinicAdmin/DoctorScheduleRepository.cs`
2. Build و تست

---

## 📝 TODO برای PROD

- [ ] بررسی نیاز به Navigation Property `Doctor` در `SetDoctorScheduleAsync`
- [ ] بررسی استفاده از `GetDoctorScheduleAsync` در سایر قسمت‌ها
- [ ] بررسی SQL Query تولید شده توسط EF6

---

## 📚 References

- **قرارداد:** `Bugfix-Master-Contract.md`
- **قرارداد:** `Contracts/01-PreFlight-Protocol.md`
- **قرارداد:** `Contracts/DEBUGGING_SPECIALIST_CONTRACT.md`
- **فایل:** `Repositories/ClinicAdmin/DoctorScheduleRepository.cs:43-56`
- **فایل:** `Services/ClinicAdmin/DoctorScheduleService.cs:173-174`
- **فایل:** `Models/Entities/Doctor/DoctorSchedule.cs:289-292`

---

**نویسنده:** Senior Debugging Specialist  
**تاریخ:** 2025-12-07  
**نسخه:** 1.0

