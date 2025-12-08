# 📋 تحلیل جامع ماژول زمان‌بندی برنامه کاری پزشک (DoctorSchedule)

**تاریخ تحلیل:** 1404/09/09  
**مسیر:** `/Admin/DoctorSchedule`  
**وضعیت:** در حال بررسی کامل

---

## 📊 خلاصه اجرایی

ماژول **DoctorSchedule** یک سیستم مدیریت برنامه کاری هفتگی برای پزشکان است که شامل:
- ✅ مدیریت روزهای کاری هفتگی
- ✅ مدیریت بازه‌های زمانی کاری
- ✅ محاسبه اسلات‌های در دسترس برای نوبت‌دهی
- ✅ مسدود کردن بازه‌های زمانی (مرخصی، جلسات)
- ✅ مدیریت فعال/غیرفعال کردن برنامه‌ها

---

## 🏗️ معماری و ساختار

### 1. **Layers (لایه‌ها)**

```
┌─────────────────────────────────────────┐
│  Controller Layer                       │
│  Areas/Admin/Controllers/               │
│  └── DoctorScheduleController.cs        │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│  Service Layer                           │
│  Services/ClinicAdmin/                   │
│  └── DoctorScheduleService.cs           │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│  Repository Layer                       │
│  Repositories/ClinicAdmin/               │
│  └── DoctorScheduleRepository.cs        │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│  Entity Layer                           │
│  Models/Entities/Doctor/                │
│  ├── DoctorSchedule.cs                  │
│  ├── DoctorWorkDay.cs                   │
│  └── DoctorTimeRange.cs                 │
└─────────────────────────────────────────┘
```

### 2. **Entity Relationships (روابط موجودیت‌ها)**

```
DoctorSchedule (1) ──→ (N) DoctorWorkDay
    │                        │
    │                        └──→ (N) DoctorTimeRange
    │
    └──→ (1) Doctor
```

**توضیح:**
- هر `DoctorSchedule` متعلق به یک `Doctor` است
- هر `DoctorSchedule` می‌تواند چندین `DoctorWorkDay` داشته باشد (حداکثر 7 روز)
- هر `DoctorWorkDay` می‌تواند چندین `DoctorTimeRange` داشته باشد (بازه‌های زمانی)

---

## 📁 فایل‌های کلیدی

### **Controller**
- **مسیر:** `Areas/Admin/Controllers/DoctorScheduleController.cs`
- **خطوط کد:** ~1018 خط
- **Actions:**
  - `Index()` - لیست برنامه‌های کاری
  - `Schedule(int doctorId)` - نمایش برنامه کاری پزشک
  - `AssignSchedule(int? doctorId)` - فرم تنظیم برنامه کاری
  - `AssignSchedule(DoctorScheduleViewModel model)` - POST تنظیم برنامه کاری
  - `BlockTimeRange(int? doctorId)` - فرم مسدود کردن بازه زمانی
  - `BlockTimeRange(BlockTimeRangeViewModel model)` - POST مسدود کردن
  - `AvailableSlots(int doctorId, DateTime date)` - دریافت اسلات‌های در دسترس
  - `GetDoctorSchedule(int doctorId)` - AJAX دریافت برنامه کاری
  - `CheckDoctorAvailability(int doctorId, DateTime dateTime)` - بررسی در دسترس بودن
  - `EditSchedule(int scheduleId)` - ویرایش برنامه کاری
  - `RemoveSchedule(int scheduleId)` - حذف برنامه کاری
  - `ActivateSchedule(int scheduleId)` - فعال کردن
  - `DeactivateSchedule(int scheduleId)` - غیرفعال کردن
  - `Details(int id)` - جزئیات برنامه کاری
  - `Edit(int id)` - ویرایش (سازگار با View)
  - `DebugSchedule(int doctorId)` - دیباگ (فقط برای تست)

### **Service**
- **مسیر:** `Services/ClinicAdmin/DoctorScheduleService.cs`
- **خطوط کد:** ~528 خط
- **متدهای کلیدی:**
  - `GetAllDoctorSchedulesAsync()` - دریافت لیست با صفحه‌بندی
  - `SetDoctorScheduleAsync()` - تنظیم/به‌روزرسانی برنامه کاری
  - `GetDoctorScheduleAsync()` - دریافت برنامه کاری پزشک
  - `BlockTimeRangeForDoctorAsync()` - مسدود کردن بازه زمانی
  - `GetAvailableAppointmentSlotsAsync()` - محاسبه اسلات‌های در دسترس
  - `GetDoctorScheduleByIdAsync()` - دریافت بر اساس شناسه
  - `DeleteDoctorScheduleAsync()` - حذف
  - `DeactivateDoctorScheduleAsync()` - غیرفعال کردن
  - `ActivateDoctorScheduleAsync()` - فعال کردن

### **Repository**
- **مسیر:** `Repositories/ClinicAdmin/DoctorScheduleRepository.cs`
- **خطوط کد:** ~574 خط
- **متدهای کلیدی:**
  - `GetDoctorScheduleAsync()` - دریافت برنامه کاری
  - `GetDoctorScheduleWithAllDetailsAsync()` - دریافت با جزئیات کامل
  - `AddDoctorScheduleAsync()` - افزودن برنامه جدید
  - `UpdateDoctorScheduleAsync()` - به‌روزرسانی
  - `DeleteDoctorScheduleAsync()` - حذف
  - `GetAvailableAppointmentSlotsAsync()` - محاسبه اسلات‌های در دسترس
  - `BlockTimeRangeForDoctorAsync()` - مسدود کردن بازه زمانی

### **ViewModel**
- **مسیر:** `ViewModels/DoctorManagementVM/DoctorScheduleViewModel.cs`
- **خطوط کد:** ~743 خط
- **کلاس‌های کلیدی:**
  - `DoctorScheduleViewModel` - مدل اصلی برنامه کاری
  - `WorkDayViewModel` - مدل روز کاری
  - `TimeRangeViewModel` - مدل بازه زمانی
  - `ScheduleItemViewModel` - مدل آیتم برنامه برای نمایش
  - `ScheduleTimeSlotViewModel` - مدل زمان کاری
  - `ScheduleOverviewViewModel` - مدل نمای کلی
  - `DoctorScheduleViewModelValidator` - ولیدیتور اصلی
  - `WorkDayViewModelValidator` - ولیدیتور روز کاری
  - `TimeRangeViewModelValidator` - ولیدیتور بازه زمانی

### **Entity**
- **مسیر:** `Models/Entities/Doctor/DoctorSchedule.cs`
- **فیلدهای کلیدی:**
  - `ScheduleId` - شناسه برنامه کاری
  - `DoctorId` - شناسه پزشک
  - `AppointmentDuration` - مدت زمان هر نوبت (دقیقه)
  - `DefaultStartTime` - زمان شروع پیش‌فرض
  - `DefaultEndTime` - زمان پایان پیش‌فرض
  - `IsActive` - وضعیت فعال/غیرفعال
  - `WorkDays` - لیست روزهای کاری

### **Views**
- **مسیر:** `Areas/Admin/Views/DoctorSchedule/`
- **فایل‌ها:**
  - `Index.cshtml` - لیست برنامه‌های کاری (~809 خط)
  - `AssignSchedule.cshtml` - فرم تنظیم برنامه کاری (~351 خط)
  - `Schedule.cshtml` - نمایش برنامه کاری
  - `Details.cshtml` - جزئیات برنامه کاری
  - `Edit.cshtml` - ویرایش برنامه کاری
  - `BlockTimeRange.cshtml` - فرم مسدود کردن بازه زمانی

---

## 🔍 تحلیل عمیق

### **1. جریان کار (Workflow)**

#### **ایجاد/ویرایش برنامه کاری:**
```
1. کاربر → AssignSchedule (GET)
   ↓
2. نمایش فرم با 7 روز هفته
   ↓
3. کاربر انتخاب روزها و بازه‌های زمانی
   ↓
4. اعتبارسنجی (FluentValidation)
   ↓
5. AssignSchedule (POST)
   ↓
6. Service.SetDoctorScheduleAsync()
   ↓
7. Repository.AddDoctorScheduleAsync() یا UpdateDoctorScheduleAsync()
   ↓
8. ذخیره در دیتابیس
   ↓
9. بازگشت به Schedule یا Index
```

#### **محاسبه اسلات‌های در دسترس:**
```
1. درخواست AvailableSlots(doctorId, date)
   ↓
2. Service.GetAvailableAppointmentSlotsAsync()
   ↓
3. Repository.GetAvailableAppointmentSlotsAsync()
   ↓
4. دریافت برنامه کاری پزشک
   ↓
5. دریافت روز هفته از تاریخ
   ↓
6. دریافت WorkDay مربوطه
   ↓
7. دریافت TimeRanges فعال
   ↓
8. تقسیم هر TimeRange به اسلات‌های AppointmentDuration دقیقه‌ای
   ↓
9. بررسی مسدودیت‌ها (BlockedTimeRanges)
   ↓
10. بررسی نوبت‌های موجود (Appointments)
   ↓
11. بازگرداندن اسلات‌های خالی
```

### **2. ساختار داده**

#### **DoctorSchedule Entity:**
```csharp
public class DoctorSchedule
{
    public int ScheduleId { get; set; }
    public int DoctorId { get; set; }
    public int AppointmentDuration { get; set; } = 30; // دقیقه
    public TimeSpan? DefaultStartTime { get; set; }
    public TimeSpan? DefaultEndTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation Properties
    public virtual Doctor Doctor { get; set; }
    public virtual ICollection<DoctorWorkDay> WorkDays { get; set; }
}
```

#### **DoctorWorkDay Entity:**
```csharp
public class DoctorWorkDay
{
    public int WorkDayId { get; set; }
    public int ScheduleId { get; set; }
    public int DayOfWeek { get; set; } // 0=یکشنبه, 6=شنبه
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation Properties
    public virtual DoctorSchedule Schedule { get; set; }
    public virtual ICollection<DoctorTimeRange> TimeRanges { get; set; }
}
```

#### **DoctorTimeRange Entity:**
```csharp
public class DoctorTimeRange
{
    public int TimeRangeId { get; set; }
    public int WorkDayId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation Properties
    public virtual DoctorWorkDay WorkDay { get; set; }
}
```

### **3. اعتبارسنجی (Validation)**

#### **قوانین اعتبارسنجی:**

**DoctorScheduleViewModel:**
- ✅ `DoctorId > 0`
- ✅ `AppointmentDuration` بین 5 تا 120 دقیقه
- ✅ حداقل یک `WorkDay` باید تعیین شود
- ✅ حداکثر 7 `WorkDay` (یک روز برای هر روز هفته)
- ✅ روزهای کاری تکراری مجاز نیست
- ✅ حداقل یک `WorkDay` باید فعال باشد

**WorkDayViewModel:**
- ✅ `DayOfWeek` بین 0 تا 6
- ✅ `DayName` الزامی است
- ✅ برای روزهای فعال، حداقل یک `TimeRange` باید تعیین شود
- ✅ حداکثر 10 `TimeRange` در روز
- ✅ بازه‌های زمانی نباید با هم تداخل داشته باشند

**TimeRangeViewModel:**
- ✅ `StartTime` و `EndTime` الزامی هستند
- ✅ `StartTime` و `EndTime` باید بین 00:00 تا 23:59 باشند
- ✅ `EndTime` باید بعد از `StartTime` باشد
- ✅ حداقل مدت زمان: 15 دقیقه
- ✅ حداکثر مدت زمان: 8 ساعت (480 دقیقه)

### **4. محاسبه اسلات‌های در دسترس**

**الگوریتم:**
1. دریافت `DoctorSchedule` برای `doctorId`
2. دریافت `DayOfWeek` از `date`
3. پیدا کردن `DoctorWorkDay` مربوطه که `IsActive = true`
4. دریافت `DoctorTimeRange`های فعال برای آن روز
5. برای هر `TimeRange`:
   - تقسیم به اسلات‌های `AppointmentDuration` دقیقه‌ای
   - مثال: اگر `StartTime = 09:00`, `EndTime = 12:00`, `AppointmentDuration = 30`
     - اسلات 1: 09:00 - 09:30
     - اسلات 2: 09:30 - 10:00
     - اسلات 3: 10:00 - 10:30
     - ...
     - اسلات 6: 11:30 - 12:00
6. بررسی مسدودیت‌ها (`BlockedTimeRanges`)
7. بررسی نوبت‌های موجود (`Appointments`)
8. بازگرداندن اسلات‌های خالی

---

## ✅ نقاط قوت

1. **معماری لایه‌ای:** جداسازی صحیح Controller → Service → Repository
2. **اعتبارسنجی قوی:** استفاده از FluentValidation با قوانین جامع
3. **مدیریت خطا:** لاگ‌گیری کامل با Serilog
4. **Soft Delete:** پشتیبانی از حذف نرم برای حفظ اطلاعات
5. **Audit Trail:** ردیابی کامل ایجاد/ویرایش/حذف
6. **ViewModel Pattern:** استفاده صحیح از ViewModel برای جداسازی لایه‌ها
7. **Factory Methods:** `FromEntity()` و `ToEntity()` برای تبدیل
8. **پشتیبانی از تقویم شمسی:** استفاده از PersianDateHelper
9. **UI/UX حرفه‌ای:** طراحی مدرن با Bootstrap و گرادیان‌ها
10. **AJAX Support:** پشتیبانی از درخواست‌های AJAX

---

## ⚠️ مشکلات و نقاط ضعف

### **1. مشکلات احتمالی در Repository**

#### **مشکل 1: به‌روزرسانی WorkDays و TimeRanges**
```csharp
// در UpdateDoctorScheduleAsync()
// فقط فیلدهای اصلی به‌روزرسانی می‌شوند
// WorkDays و TimeRanges به‌روزرسانی نمی‌شوند!
```

**راه‌حل:** باید منطق به‌روزرسانی `WorkDays` و `TimeRanges` اضافه شود.

#### **مشکل 2: عدم مدیریت Transaction**
```csharp
// در SetDoctorScheduleAsync()
// اگر به‌روزرسانی WorkDays موفق شود ولی TimeRanges ناموفق باشد
// داده‌ها ناهماهنگ می‌شوند
```

**راه‌حل:** استفاده از `DbContextTransaction` برای اتمیک کردن عملیات.

### **2. مشکلات احتمالی در Service**

#### **مشکل 1: عدم بررسی تداخل بازه‌های زمانی**
```csharp
// در SetDoctorScheduleAsync()
// بررسی نمی‌شود که آیا بازه‌های زمانی با هم تداخل دارند یا نه
```

**راه‌حل:** اضافه کردن منطق بررسی تداخل قبل از ذخیره.

#### **مشکل 2: عدم بررسی نوبت‌های موجود در حذف**
```csharp
// در DeleteDoctorScheduleAsync()
// بررسی نمی‌شود که آیا نوبت‌های فعالی برای این برنامه وجود دارد یا نه
```

**راه‌حل:** بررسی نوبت‌های فعال قبل از حذف.

### **3. مشکلات احتمالی در View**

#### **مشکل 1: عدم وجود JavaScript برای مدیریت پویای TimeRanges**
```javascript
// در AssignSchedule.cshtml
// باید امکان افزودن/حذف پویای TimeRange وجود داشته باشد
```

**راه‌حل:** اضافه کردن JavaScript برای مدیریت پویای TimeRanges.

#### **مشکل 2: عدم اعتبارسنجی Client-Side**
```javascript
// اعتبارسنجی فقط Server-Side است
// باید اعتبارسنجی Client-Side هم اضافه شود
```

**راه‌حل:** اضافه کردن اعتبارسنجی JavaScript.

### **4. مشکلات احتمالی در محاسبه اسلات‌ها**

#### **مشکل 1: عدم در نظر گیری تعطیلات**
```csharp
// در GetAvailableAppointmentSlotsAsync()
// تعطیلات رسمی در نظر گرفته نمی‌شوند
```

**راه‌حل:** اضافه کردن جدول تعطیلات و بررسی آن.

#### **مشکل 2: عدم در نظر گیری استثناهای برنامه کاری**
```csharp
// در GetAvailableAppointmentSlotsAsync()
// ScheduleExceptions در نظر گرفته نمی‌شوند
```

**راه‌حل:** بررسی `ScheduleExceptions` قبل از محاسبه اسلات‌ها.

---

## 🔧 پیشنهادات بهبود

### **1. بهبود Repository**

#### **الف) اضافه کردن Transaction Management:**
```csharp
public async Task<DoctorSchedule> UpdateDoctorScheduleWithWorkDaysAsync(DoctorSchedule schedule)
{
    using (var transaction = _context.Database.BeginTransaction())
    {
        try
        {
            // به‌روزرسانی Schedule
            // به‌روزرسانی WorkDays
            // به‌روزرسانی TimeRanges
            
            await _context.SaveChangesAsync();
            transaction.Commit();
            return schedule;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

#### **ب) اضافه کردن متدهای Batch:**
```csharp
public async Task BulkUpdateWorkDaysAsync(int scheduleId, List<DoctorWorkDay> workDays)
{
    // حذف WorkDays قدیمی
    // افزودن WorkDays جدید
    // بهینه‌سازی برای عملکرد بهتر
}
```

### **2. بهبود Service**

#### **الف) اضافه کردن Caching:**
```csharp
// Cache کردن برنامه‌های کاری برای کاهش بار دیتابیس
private readonly IMemoryCache _cache;

public async Task<DoctorScheduleViewModel> GetDoctorScheduleAsync(int doctorId)
{
    var cacheKey = $"DoctorSchedule_{doctorId}";
    if (_cache.TryGetValue(cacheKey, out DoctorScheduleViewModel cached))
    {
        return cached;
    }
    
    // دریافت از دیتابیس
    // Cache کردن
    return result;
}
```

#### **ب) اضافه کردن Background Jobs:**
```csharp
// برای محاسبه اسلات‌های هفته آینده
public async Task PreCalculateNextWeekSlotsAsync()
{
    // محاسبه و Cache کردن اسلات‌های هفته آینده
}
```

### **3. بهبود UI/UX**

#### **الف) اضافه کردن Calendar View:**
```html
<!-- نمایش برنامه کاری به صورت تقویم هفتگی -->
<div class="weekly-calendar">
    <!-- هر روز هفته -->
    <!-- هر بازه زمانی -->
</div>
```

#### **ب) اضافه کردن Drag & Drop:**
```javascript
// امکان جابجایی بازه‌های زمانی با Drag & Drop
$('.time-range-item').draggable();
$('.work-day-card').droppable();
```

### **4. بهبود Performance**

#### **الف) اضافه کردن Indexes:**
```sql
-- Index برای جستجوی سریع‌تر
CREATE INDEX IX_DoctorSchedule_DoctorId_IsDeleted 
ON DoctorSchedules(DoctorId, IsDeleted);

CREATE INDEX IX_DoctorWorkDay_ScheduleId_DayOfWeek 
ON DoctorWorkDays(ScheduleId, DayOfWeek);
```

#### **ب) اضافه کردن Pagination برای WorkDays:**
```csharp
// اگر تعداد WorkDays زیاد شود
public async Task<PagedResult<DoctorWorkDay>> GetWorkDaysPagedAsync(int scheduleId, int page, int pageSize)
{
    // صفحه‌بندی WorkDays
}
```

---

## 📝 چک‌لیست بررسی

### **Backend:**
- [ ] بررسی صحت به‌روزرسانی WorkDays و TimeRanges
- [ ] بررسی مدیریت Transaction
- [ ] بررسی بررسی تداخل بازه‌های زمانی
- [ ] بررسی بررسی نوبت‌های موجود در حذف
- [ ] بررسی بررسی تعطیلات در محاسبه اسلات‌ها
- [ ] بررسی بررسی ScheduleExceptions
- [ ] بررسی Performance و Indexes
- [ ] بررسی Error Handling

### **Frontend:**
- [ ] بررسی JavaScript برای مدیریت پویای TimeRanges
- [ ] بررسی اعتبارسنجی Client-Side
- [ ] بررسی UI/UX و Responsive Design
- [ ] بررسی Calendar View
- [ ] بررسی Drag & Drop (اختیاری)

### **Testing:**
- [ ] Unit Tests برای Service
- [ ] Unit Tests برای Repository
- [ ] Integration Tests برای Controller
- [ ] E2E Tests برای UI

---

## 🎯 اولویت‌بندی مشکلات

### **اولویت بالا (Critical):**
1. ✅ بررسی به‌روزرسانی WorkDays و TimeRanges
2. ✅ بررسی مدیریت Transaction
3. ✅ بررسی بررسی تداخل بازه‌های زمانی

### **اولویت متوسط (Important):**
4. ✅ بررسی بررسی نوبت‌های موجود در حذف
5. ✅ بررسی بررسی تعطیلات در محاسبه اسلات‌ها
6. ✅ اضافه کردن JavaScript برای مدیریت پویای TimeRanges

### **اولویت پایین (Nice to Have):**
7. ✅ اضافه کردن Caching
8. ✅ اضافه کردن Calendar View
9. ✅ اضافه کردن Drag & Drop

---

## 📚 مستندات مرتبط

- `SPECIALIZED_MODULES_ANALYSIS.md` - تحلیل ماژول‌های تخصصی
- `ARCHITECTURE_ANALYSIS_REPORT.md` - گزارش تحلیل معماری
- `Models/Entities/Doctor/DoctorSchedule.cs` - Entity اصلی
- `ViewModels/DoctorManagementVM/DoctorScheduleViewModel.cs` - ViewModel

---

## 🔄 مراحل بعدی

1. **بررسی دقیق Repository:** بررسی منطق به‌روزرسانی WorkDays و TimeRanges
2. **بررسی دقیق Service:** بررسی منطق محاسبه اسلات‌ها
3. **بررسی دقیق View:** بررسی JavaScript و UI/UX
4. **تست کامل:** تست تمام سناریوها
5. **بهینه‌سازی:** بهبود Performance و اضافه کردن Caching

---

**تهیه شده توسط:** AI Assistant  
**تاریخ:** 1404/09/09  
**نسخه:** 1.0

