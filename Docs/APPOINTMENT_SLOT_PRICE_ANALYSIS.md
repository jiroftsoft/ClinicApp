# 🔍 گزارش عمیق: بررسی Price در AppointmentSlots

**تاریخ:** 2026-01-06  
**وضعیت:** 🔴 **مشکل شناسایی شد**  
**نوع:** تحلیل معماری و باگ

---

## 🎯 **سوال کاربر:**
**قیمت `Price` در جدول `AppointmentSlots` از کجا مقدار می‌گیرد؟**

---

## 📊 **تحلیل عمیق**

### ✅ **1. دو Entity جداگانه وجود دارد:**

#### **`DoctorTimeSlot`** (جدول: `DoctorTimeSlots`)
```csharp
// Models/Entities/Doctor/DoctorTimeSlot.cs
public class DoctorTimeSlot : ISoftDelete, ITrackable
{
    public int TimeSlotId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Duration { get; set; }
    public AppointmentStatus Status { get; set; }
    // ❌ فیلد Price وجود ندارد!
}
```

**استفاده در کد:**
- ✅ `Repositories/ClinicAdmin/DoctorScheduleRepository.cs:1873` - ایجاد می‌شود
- ✅ `_context.DoctorTimeSlots.AddRange(generatedSlots)` - ذخیره می‌شود
- ✅ در `GenerateSlotsForDateAsync` استفاده می‌شود

---

#### **`AppointmentSlot`** (جدول: `AppointmentSlots`)
```csharp
// Models/Entities/Appointment/AppointmentSlot.cs
public class AppointmentSlot : ISoftDelete, ITrackable
{
    public int SlotId { get; set; }
    public int ScheduleId { get; set; }
    public DateTime SlotDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentSlotStatus Status { get; set; }
    public decimal Price { get; set; } // ✅ فیلد Price وجود دارد!
    // ...
}
```

**استفاده در کد:**
- ❌ **هیچ جا استفاده نمی‌شود!**
- ❌ هیچ `new AppointmentSlot` در کد وجود ندارد
- ❌ هیچ `AppointmentSlots.Add` در کد وجود ندارد
- ❌ هیچ Repository برای `AppointmentSlot` وجود ندارد

---

### 🔴 **2. مشکل شناسایی شده:**

#### **مشکل اصلی:**
```
❌ AppointmentSlot یک Entity "مرده" است!
   - در دیتابیس وجود دارد (جدول AppointmentSlots)
   - در کد استفاده نمی‌شود
   - Price هیچ وقت تنظیم نمی‌شود
   - احتمالاً از Migration قدیمی باقی مانده است
```

#### **جریان فعلی:**
```
1. GenerateSlotsForDateAsync() 
   → DoctorTimeSlot ایجاد می‌کند (بدون Price)
   → در DoctorTimeSlots ذخیره می‌شود

2. Appointment.Price
   → از AppointmentPricingService محاسبه می‌شود
   → از DoctorSchedule.ConsultationFee می‌آید
   → در Appointment ذخیره می‌شود
```

---

### 📋 **3. بررسی دقیق کد:**

#### **جایی که اسلات‌ها ایجاد می‌شوند:**
```csharp
// Repositories/ClinicAdmin/DoctorScheduleRepository.cs:1873
var newSlot = new DoctorTimeSlot
{
    DoctorId = doctorId,
    AppointmentDate = dateOnly,
    StartTime = currentTime,
    EndTime = slotEndTime,
    Duration = doctorSchedule.AppointmentDuration,
    Status = AppointmentStatus.Available,
    // ❌ Price تنظیم نمی‌شود!
    // ❌ ConsultationFee از DoctorSchedule استفاده نمی‌شود!
};
```

**نتیجه:** `DoctorTimeSlot` بدون `Price` ایجاد می‌شود.

---

#### **جایی که قیمت محاسبه می‌شود:**
```csharp
// Services/Appointment/AppointmentPricingService.cs:94
private async Task<decimal> GetBasePriceAsync(int doctorId, int? serviceCategoryId)
{
    var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
    if (schedule != null && schedule.ConsultationFee > 0)
    {
        return schedule.ConsultationFee; // ✅ از DoctorSchedule.ConsultationFee
    }
    return DEFAULT_CONSULTATION_FEE; // 500,000 تومان
}
```

**نتیجه:** قیمت از `DoctorSchedule.ConsultationFee` می‌آید.

---

#### **جایی که قیمت ذخیره می‌شود:**
```csharp
// Services/Appointment/AppointmentBookingService.cs:754
var appointment = new AppointmentEntity
{
    // ...
    Price = priceResult.Data, // ✅ در Appointment.Price ذخیره می‌شود
    // ...
};
```

**نتیجه:** قیمت در `Appointment.Price` ذخیره می‌شود، نه در `AppointmentSlot.Price`.

---

## 🚨 **مشکلات شناسایی شده:**

### **1. AppointmentSlot استفاده نمی‌شود**
- ❌ Entity در دیتابیس وجود دارد اما در کد استفاده نمی‌شود
- ❌ `Price` در `AppointmentSlot` هیچ وقت تنظیم نمی‌شود
- ❌ احتمالاً از Migration قدیمی باقی مانده است

### **2. DoctorTimeSlot بدون Price**
- ❌ `DoctorTimeSlot` فیلد `Price` ندارد
- ❌ در زمان ایجاد اسلات، `Price` تنظیم نمی‌شود
- ❌ `ConsultationFee` از `DoctorSchedule` استفاده نمی‌شود

### **3. عدم هماهنگی**
- ❌ `AppointmentSlot` در دیتابیس وجود دارد اما استفاده نمی‌شود
- ❌ `DoctorTimeSlot` استفاده می‌شود اما `Price` ندارد
- ❌ قیمت فقط در `Appointment.Price` ذخیره می‌شود

---

## ✅ **راه‌حل پیشنهادی:**

### **گزینه 1: استفاده از AppointmentSlot (توصیه می‌شود)**

#### **مرحله 1: اضافه کردن Price به DoctorTimeSlot**
```csharp
// Models/Entities/Doctor/DoctorTimeSlot.cs
public class DoctorTimeSlot : ISoftDelete, ITrackable
{
    // ... existing properties ...
    
    /// <summary>
    /// قیمت اسلات (ریال) - از DoctorSchedule.ConsultationFee
    /// </summary>
    [Range(0, 10000000, ErrorMessage = "قیمت باید بین 0 تا 10,000,000 ریال باشد.")]
    public decimal Price { get; set; }
}
```

#### **مرحله 2: تنظیم Price در GenerateSlotsForDateAsync**
```csharp
// Repositories/ClinicAdmin/DoctorScheduleRepository.cs:1873
var newSlot = new DoctorTimeSlot
{
    DoctorId = doctorId,
    AppointmentDate = dateOnly,
    StartTime = currentTime,
    EndTime = slotEndTime,
    Duration = doctorSchedule.AppointmentDuration,
    Status = AppointmentStatus.Available,
    Price = doctorSchedule.ConsultationFee, // ✅ اضافه شد
    CreatedAt = DateTime.Now,
    CreatedByUserId = doctorSchedule.UpdatedByUserId ?? doctorSchedule.CreatedByUserId
};
```

#### **مرحله 3: Migration**
```sql
ALTER TABLE DoctorTimeSlots
ADD Price DECIMAL(18, 4) NOT NULL DEFAULT 0;
```

---

### **گزینه 2: حذف AppointmentSlot (اگر استفاده نمی‌شود)**

اگر `AppointmentSlot` واقعاً استفاده نمی‌شود:
1. بررسی کامل کد برای اطمینان
2. Migration برای حذف جدول `AppointmentSlots`
3. حذف Entity `AppointmentSlot`

---

## 📊 **نمودار جریان فعلی:**

```
┌─────────────────────────────────────────────────────────┐
│ 1. GenerateSlotsForDateAsync()                         │
│    → DoctorTimeSlot ایجاد می‌کند                       │
│    → Price تنظیم نمی‌شود ❌                             │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 2. ذخیره در DoctorTimeSlots                             │
│    → Price = 0 (پیش‌فرض)                                 │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 3. بیمار نوبت رزرو می‌کند                               │
│    → AppointmentPricingService.CalculatePriceAsync()   │
│    → از DoctorSchedule.ConsultationFee می‌آید            │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 4. ذخیره در Appointment.Price                           │
│    → Price = ConsultationFee (مثلاً 500000)             │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 5. AppointmentSlot.Price                                │
│    → هیچ وقت تنظیم نمی‌شود ❌                           │
│    → احتمالاً 0 یا NULL است                            │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 **نتیجه‌گیری:**

### **پاسخ به سوال کاربر:**
```
❌ Price در AppointmentSlots از هیچ جا مقدار نمی‌گیرد!
   - AppointmentSlot در کد استفاده نمی‌شود
   - Price هیچ وقت تنظیم نمی‌شود
   - احتمالاً 0 یا NULL است
```

### **جریان واقعی:**
```
✅ قیمت از DoctorSchedule.ConsultationFee می‌آید
✅ در Appointment.Price ذخیره می‌شود
✅ از AppointmentPricingService محاسبه می‌شود
```

### **مشکل:**
```
❌ AppointmentSlot یک Entity "مرده" است
❌ DoctorTimeSlot بدون Price است
❌ عدم هماهنگی بین Entity ها
```

---

## ✅ **اقدامات لازم:**

1. **اضافه کردن Price به DoctorTimeSlot** (توصیه می‌شود)
2. **تنظیم Price در GenerateSlotsForDateAsync** از `DoctorSchedule.ConsultationFee`
3. **Migration برای اضافه کردن فیلد Price به DoctorTimeSlots**
4. **بررسی و حذف AppointmentSlot** (اگر واقعاً استفاده نمی‌شود)

---

**📌 این گزارش بر اساس تحلیل عمیق کد موجود تهیه شده است.**

