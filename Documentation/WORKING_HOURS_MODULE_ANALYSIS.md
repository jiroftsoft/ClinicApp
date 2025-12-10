# 🔍 تحلیل جامع ماژول Working Hours & Schedule

**تاریخ تحلیل**: 2025-01-XX  
**تحلیلگر**: Senior Module Analyst  
**وضعیت**: ✅ آماده برای پیاده‌سازی

---

## 📊 خلاصه اجرایی

### ✅ نتیجه تحلیل
**ماژول ClinicWorkingHours تکراری نیست** و برای پیاده‌سازی آماده است.

### 🎯 هدف ماژول
مدیریت **ساعات کاری عمومی کلینیک** برای نمایش در:
- صفحه تماس (Contact Page)
- صفحه اصلی (HomePage)
- اطلاع‌رسانی عمومی به بیماران

---

## 🔍 تحلیل ماژول‌های موجود

### 1️⃣ **DoctorSchedule Entity** ✅
**مسیر**: `Models/Entities/Doctor/DoctorSchedule.cs`

**ویژگی‌ها**:
- برنامه کاری **هر پزشک به صورت جداگانه**
- شامل: WorkDays, TimeRanges, Exceptions
- برای **نوبت‌دهی پزشکان** استفاده می‌شود
- هر DoctorSchedule به یک Doctor خاص متصل است

**نتیجه**: ✅ **متفاوت از ClinicWorkingHours**

---

### 2️⃣ **Clinic Entity** ✅
**مسیر**: `Models/Entities/Clinic/Clinic.cs`

**ویژگی‌ها**:
- Name, Address, PhoneNumber
- **هیچ فیلدی برای Working Hours ندارد** ❌
- فقط اطلاعات پایه کلینیک

**نتیجه**: ✅ **نیاز به ماژول جدید برای Working Hours**

---

### 3️⃣ **ContactSectionViewModel** ✅
**مسیر**: `ViewModels/HomePageViewModel.cs`

**ویژگی‌ها**:
- شامل `WorkingDays` (List<WorkingDayViewModel>)
- اما این فقط در **ViewModel** است
- در **Entity Clinic** وجود ندارد

**نتیجه**: ✅ **نیاز به Entity برای ذخیره Working Hours**

---

## 🎯 طراحی ماژول ClinicWorkingHours

### **تفاوت با DoctorSchedule**:

| ویژگی | DoctorSchedule | ClinicWorkingHours |
|-------|----------------|-------------------|
| **هدف** | نوبت‌دهی پزشکان | اطلاع‌رسانی عمومی |
| **محدوده** | هر پزشک جداگانه | کل کلینیک |
| **استفاده** | سیستم نوبت‌دهی | صفحه تماس و اطلاع‌رسانی |
| **پیچیدگی** | شامل Exceptions, TimeRanges | ساده‌تر - فقط روزهای هفته |

---

## 📋 فیلدهای پیشنهادی

### **ClinicWorkingHours Entity**:
```csharp
- ClinicWorkingHoursId (PK)
- ClinicId (FK) - اختیاری (اگر چند کلینیک داریم)
- DayOfWeek (شنبه=0, یکشنبه=1, ...)
- StartTime (TimeSpan)
- EndTime (TimeSpan)
- IsOpen (bool)
- IsActive (bool)
- DisplayOrder (int)
- Notes (string) - اختیاری
- ISoftDelete, ITrackable
```

---

## 🔗 یکپارچه‌سازی

### **با HomePageService**:
- `GetContactSectionAsync()` باید از `IClinicWorkingHoursService` استفاده کند
- جایگزین کردن `WorkingDays` سخت‌کد شده با داده‌های واقعی از دیتابیس

### **با ContactSection View**:
- نمایش Working Hours از دیتابیس به جای داده‌های سخت‌کد شده

---

## ✅ نتیجه‌گیری

1. ✅ **ماژول تکراری نیست** - DoctorSchedule برای پزشکان است، ClinicWorkingHours برای کلینیک
2. ✅ **نیاز واقعی وجود دارد** - Clinic Entity فیلد Working Hours ندارد
3. ✅ **یکپارچه‌سازی امکان‌پذیر است** - با HomePageService و ContactSection
4. ✅ **آماده برای پیاده‌سازی** - طبق اصول SRP و Strongly-Typed

---

## 🚀 پیشنهاد پیاده‌سازی

**مراحل**:
1. ✅ Entity و Configuration
2. ✅ Repository (Interface + Implementation)
3. ✅ ViewModels
4. ✅ Service (Interface + Implementation)
5. ✅ ثبت در UnityConfig و DbContext
6. ✅ Admin Controller و Views
7. ✅ به‌روزرسانی HomePageService
8. ✅ به‌روزرسانی ContactSection View
9. ✅ تست و بهینه‌سازی

---

**✅ تأیید برای شروع پیاده‌سازی**

