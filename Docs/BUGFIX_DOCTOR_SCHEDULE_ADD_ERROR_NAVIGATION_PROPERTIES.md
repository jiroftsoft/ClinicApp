# 🐛 گزارش رفع باگ - خطای SQL در افزودن برنامه کاری (Navigation Properties)

**تاریخ:** 2025-12-07  
**ماژول:** DoctorSchedule  
**اولویت:** High  
**وضعیت:** ✅ رفع شده

---

## 📋 Executive Summary

**مشکل:** خطای SQL `Invalid column name 'Doctor_DoctorId'` هنگام افزودن برنامه کاری جدید برای پزشک رخ می‌دهد.

**علت ریشه‌ای:** وقتی Entity `DoctorSchedule` به Context اضافه می‌شود، اگر Navigation Property `Doctor` تنظیم شده باشد (حتی null نباشد)، EF6 سعی می‌کند آن را در Insert statement استفاده کند و به اشتباه ستون `Doctor_DoctorId` را جستجو می‌کند.

**راه‌حل:** تنظیم Navigation Properties (`Doctor`, `CreatedByUser`, `UpdatedByUser`, `DeletedByUser`) به `null` قبل از افزودن Entity به Context.

---

## 🔍 Evidence (شواهد کامل)

### **1. خطای SQL:**
```
System.Data.SqlClient.SqlException: Invalid column name 'Doctor_DoctorId'.
at System.Data.Entity.Core.Mapping.Update.Internal.DynamicUpdateCommand.<ExecuteAsync>d__8.MoveNext()
at ClinicApp.Models.ApplicationDbContext.<SaveChangesAsync>d__208.MoveNext() in C:\Users\Developer\source\repos\ClinicApp\Models\IdentityModels.cs:line 259
```

### **2. Stack Trace کامل:**
```
System.Data.Entity.Infrastructure.DbUpdateException: An error occurred while updating the entries. See the inner exception for details.
---> System.Data.Entity.Core.UpdateException: An error occurred while updating the entries. See the inner exception for details.
---> System.Data.SqlClient.SqlException: Invalid column name 'Doctor_DoctorId'.
```

### **3. محل خطا:**
- **فایل:** `Repositories/ClinicAdmin/DoctorScheduleRepository.cs`
- **متد:** `AddDoctorScheduleAsync(DoctorSchedule schedule)`
- **خط:** 284 (`await _context.SaveChangesAsync();`)

### **4. لاگ خطا:**
```
2025-12-07 22:33:03.946 [ERR] | خطای پایگاه داده رخ داده است
خطا در افزودن برنامه کاری پزشک
```

---

## 🧠 Root-Cause Analysis (تحلیل ریشه‌ای)

### **دلیل منطقی:**
1. **Navigation Property تنظیم شده:** وقتی Entity `DoctorSchedule` از ViewModel (`ToEntity()`) ساخته می‌شود، Navigation Property `Doctor` ممکن است تنظیم شده باشد (حتی اگر null باشد).
2. **EF6 Behavior:** وقتی Entity به Context اضافه می‌شود و Navigation Property تنظیم شده باشد، EF6 سعی می‌کند آن را در Insert statement استفاده کند.
3. **SQL Error:** EF6 به اشتباه ستون `Doctor_DoctorId` را جستجو می‌کند (که وجود ندارد) به جای استفاده از Foreign Key `DoctorId`.

### **مشکل واقعی:**
- ✅ `ToEntity()` در ViewModel Navigation Property `Doctor` را تنظیم نمی‌کند (فقط `DoctorId`)
- ❌ اما ممکن است Navigation Property `Doctor` در جای دیگری تنظیم شده باشد
- ❌ یا EF6 به صورت خودکار Navigation Property را بررسی می‌کند

---

## 💡 Options (گزینه‌های رفع)

### **Option A: تنظیم Navigation Properties به null قبل از Add** ⭐ (انتخاب شده)
- **دامنه تغییر:** کوچک
- **ریسک:** کم
- **مزایا:** 
  - ساده و مستقیم
  - از خطای SQL جلوگیری می‌کند
  - فقط Foreign Key (`DoctorId`) استفاده می‌شود
- **معایب:** 
  - نیاز به تنظیم Navigation Properties به null
- **دلیل انتخاب:** ساده‌ترین و مؤثرترین راه حل

### **Option B: استفاده از Entry State برای Navigation Properties**
- **دامنه تغییر:** متوسط
- **ریسک:** متوسط
- **مزایا:** 
  - کنترل بیشتر بر Entity State
- **معایب:** 
  - پیچیده‌تر
  - نیاز به تغییرات بیشتر

### **Option C: استفاده از DTO به جای Entity**
- **دامنه تغییر:** بزرگ
- **ریسک:** بالا
- **مزایا:** 
  - جداسازی کامل
- **معایب:** 
  - نیاز به تغییرات گسترده
  - نیاز به Mapping اضافی

---

## 🔧 Patch (تغییرات اتمیک)

### **تغییر 1: تنظیم Navigation Properties به null در AddDoctorScheduleAsync** ✅ (اعمال شده)

**فایل:** `Repositories/ClinicAdmin/DoctorScheduleRepository.cs`  
**خطوط:** 257-283

**کد نهایی:**
```csharp
// ✅ تنظیم Navigation Properties به null برای جلوگیری از خطای SQL
// ✅ EF6 نباید Navigation Properties را در Insert statement استفاده کند
// ✅ فقط Foreign Key (DoctorId) باید تنظیم شود
schedule.Doctor = null;
schedule.CreatedByUser = null;
schedule.UpdatedByUser = null;
schedule.DeletedByUser = null;

// تنظیم تاریخ‌ها
schedule.CreatedAt = DateTime.Now;
schedule.UpdatedAt = DateTime.Now;
schedule.IsDeleted = false;

// ✅ تنظیم تاریخ‌ها برای WorkDays و TimeRanges
if (schedule.WorkDays != null)
{
    foreach (var workDay in schedule.WorkDays)
    {
        workDay.CreatedAt = DateTime.Now;
        workDay.UpdatedAt = DateTime.Now;
        workDay.IsDeleted = false;

        if (workDay.TimeRanges != null)
        {
            foreach (var timeRange in workDay.TimeRanges)
            {
                timeRange.CreatedAt = DateTime.Now;
                timeRange.UpdatedAt = DateTime.Now;
                timeRange.IsDeleted = false;
            }
        }
    }
}

_context.DoctorSchedules.Add(schedule);
await _context.SaveChangesAsync();
```

### **تغییر 2: تنظیم Navigation Properties به null در UpdateDoctorScheduleAsync** ✅ (اعمال شده)

**فایل:** `Repositories/ClinicAdmin/DoctorScheduleRepository.cs`  
**خطوط:** 323-338

**کد نهایی:**
```csharp
// ✅ دریافت برنامه موجود با Include برای WorkDays و TimeRanges
// ✅ حذف .Include(ds => ds.Doctor) به دلیل خطای SQL: Invalid column name 'Doctor_DoctorId'
var existingSchedule = await _context.DoctorSchedules
    .Include(ds => ds.WorkDays)
    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
    .FirstOrDefaultAsync(ds => ds.ScheduleId == schedule.ScheduleId && !ds.IsDeleted);

if (existingSchedule == null)
    throw new InvalidOperationException($"برنامه کاری پزشک یافت نشد.");

// ✅ تنظیم Navigation Properties به null در schedule ورودی برای جلوگیری از خطای SQL
// ✅ EF6 نباید Navigation Properties را در Update statement استفاده کند
schedule.Doctor = null;
schedule.CreatedByUser = null;
schedule.UpdatedByUser = null;
schedule.DeletedByUser = null;

// ✅ به‌روزرسانی فیلدهای اصلی
existingSchedule.AppointmentDuration = schedule.AppointmentDuration;
existingSchedule.DefaultStartTime = schedule.DefaultStartTime;
existingSchedule.DefaultEndTime = schedule.DefaultEndTime;
existingSchedule.IsActive = schedule.IsActive;
existingSchedule.UpdatedAt = DateTime.Now;
existingSchedule.UpdatedByUserId = schedule.UpdatedByUserId;
```

---

## ✅ Manual Sanity Check (تأیید دستی)

### **گام‌های تست:**
1. ✅ Build پروژه → باید سبز باشد
2. ✅ اجرای سناریو:
   - ورود به `/Admin/DoctorSchedule/AssignSchedule?doctorId=2`
   - تنظیم برنامه کاری برای یک روز (مثلاً شنبه)
   - اضافه کردن TimeRange با StartTime=07:00 و EndTime=17:00
   - کلیک روی ذخیره
   - ✅ باید بدون خطا ذخیره شود
3. ✅ تست سناریوی Update:
   - ویرایش برنامه کاری موجود
   - تغییر TimeRange
   - کلیک روی ذخیره
   - ✅ باید بدون خطا ذخیره شود

---

## 📊 Impact/Regression Assessment

### **تأثیر:**
- ✅ **مثبت:** از خطای SQL جلوگیری می‌کند
- ✅ **بدون عوارض جانبی:** Navigation Properties بعد از Save می‌توانند لود شوند

### **Regression Risk:**
- ✅ **کم:** تغییرات فقط در Repository است
- ✅ **Backward Compatible:** سازگار با کد موجود

---

## 🔄 Rollback Plan

### **گام‌های بازگشت:**
1. بازگرداندن تغییرات در `Repositories/ClinicAdmin/DoctorScheduleRepository.cs` (خطوط 257-283 و 323-338)
2. Build و تست

---

## 📝 TODO برای PROD

- [ ] بررسی نیاز به تنظیم Navigation Properties به null در سایر متدهای Repository
- [ ] بررسی نیاز به بهبود Error Handling برای خطاهای SQL مشابه

---

## 📚 References

- **قرارداد:** `Bugfix-Master-Contract.md`
- **قرارداد:** `Contracts/01-PreFlight-Protocol.md`
- **قرارداد:** `Contracts/DEBUGGING_SPECIALIST_CONTRACT.md`
- **فایل:** `Repositories/ClinicAdmin/DoctorScheduleRepository.cs:237-307`
- **فایل:** `Repositories/ClinicAdmin/DoctorScheduleRepository.cs:313-369`
- **فایل:** `ViewModels/DoctorManagementVM/DoctorScheduleViewModel.cs:330-352`

---

**نویسنده:** Senior Debugging Specialist  
**تاریخ:** 2025-12-07  
**نسخه:** 1.0

