# 🔍 ClinicApp – منطق نهایی تولید اسلات‌های زمانی

> **تاریخ بررسی:** 2025-01-14  
> **بررسی‌کننده:** AI Assistant (Senior Staff Engineer)  
> **وضعیت:** ✅ **منطق نهایی پیاده‌سازی شده**

---

## 📋 خلاصه اجرایی

### ✅ منطق نهایی
1. **برنامه هفتگی:** منشی می‌تواند برای هفته آینده یا تاریخ‌های خاص (مثلاً 25-26) برنامه تنظیم کند
2. **تولید اسلات:** فقط برای همان تاریخ خاص تولید می‌شود (نه برای چند هفته آینده)
3. **On-Demand Generation:** برای تاریخ‌های دیگر، اسلات‌ها در `GetAvailableAppointmentSlotsAsync` تولید می‌شوند
4. **رعایت تقویم شمسی:** شنبه = اولین روز هفته (مطابق time.ir)

---

## 🔍 تحلیل دقیق

### 1) منطق تولید اسلات

#### 1.1) `GenerateAndSaveTimeSlotsAsync`
```csharp
public async Task GenerateAndSaveTimeSlotsAsync(int doctorId, int scheduleId, DateTime? targetDate = null)
```
- **منطق:** 
  - اگر `targetDate` مشخص شده باشد، برای همان تاریخ اسلات تولید می‌شود (منشی تاریخ خاص را انتخاب کرده)
  - اگر `targetDate` null باشد، اولین روز کاری آینده (در 7 روز آینده) استفاده می‌شود
- **فراخوانی:** 
  - هنگام ایجاد یا به‌روزرسانی برنامه کاری (بدون `targetDate`)
  - توسط منشی برای تاریخ‌های خاص (با `targetDate`)
- **نتیجه:** فقط برای یک تاریخ خاص اسلات تولید می‌شود

#### 1.2) `GetAvailableAppointmentSlotsAsync`
```csharp
// ✅ CRITICAL FIX: اگر هیچ اسلاتی در دیتابیس وجود ندارد، از Schedule تولید می‌کنیم
if (!existingSlots.Any())
{
    // تولید اسلات از Schedule
}
```
- **منطق:** On-Demand Generation
- **نتیجه:** برای هر تاریخ درخواست، اگر اسلاتی نباشد، از Schedule تولید می‌شود

### 2) تطابق DayOfWeek با تقویم شمسی

#### 2.1) در C#:
- Sunday = 0
- Monday = 1
- Tuesday = 2
- Wednesday = 3
- Thursday = 4
- Friday = 5
- Saturday = 6

#### 2.2) در دیتابیس (طبق مدل):
- 0 = یکشنبه
- 1 = دوشنبه
- 2 = سه‌شنبه
- 3 = چهارشنبه
- 4 = پنج‌شنبه
- 5 = جمعه
- 6 = شنبه

#### 2.3) در تقویم شمسی (مطابق time.ir):
- شنبه = اولین روز هفته
- یکشنبه = دومین روز هفته
- ...
- جمعه = آخرین روز هفته

#### 2.4) تطابق:
- ✅ تطابق درست است: `dayOfWeek در C# = dayOfWeek در دیتابیس` (بدون تبدیل)
- ✅ شنبه = 6 در C# = 6 در دیتابیس

### 3) سناریوهای استفاده

#### 3.1) منشی برای هفته آینده برنامه تنظیم می‌کند
- منشی تاریخ شنبه هفته آینده را انتخاب می‌کند
- `GenerateAndSaveTimeSlotsAsync(doctorId, scheduleId, targetDate: شنبه هفته آینده)`
- اسلات‌ها فقط برای شنبه هفته آینده تولید می‌شوند

#### 3.2) منشی برای تاریخ‌های خاص برنامه تنظیم می‌کند
- منشی تاریخ‌های 25-26 را انتخاب می‌کند
- `GenerateAndSaveTimeSlotsAsync(doctorId, scheduleId, targetDate: 25)`
- `GenerateAndSaveTimeSlotsAsync(doctorId, scheduleId, targetDate: 26)`
- اسلات‌ها فقط برای 25 و 26 تولید می‌شوند

#### 3.3) کاربر به صفحه SelectTime می‌رود
- کاربر تاریخ 2026-01-06 را انتخاب می‌کند
- `GetAvailableAppointmentSlotsAsync(doctorId, date: 2026-01-06)`
- اگر اسلاتی در دیتابیس نباشد، از Schedule تولید می‌شود
- اسلات‌ها در دیتابیس ذخیره می‌شوند

---

## ✅ تست‌های مورد نیاز

### 1) Unit Tests
- ✅ `GenerateSlotsForDateAsync` برای یک تاریخ خاص
- ✅ `GetAvailableAppointmentSlotsAsync` برای یک تاریخ خاص
- ✅ `IsPersianHoliday` برای تاریخ‌های مختلف
- ✅ تطابق DayOfWeek (شنبه = 6)

### 2) Integration Tests
- ✅ تولید اسلات برای تاریخ خاص (منشی)
- ✅ On-Demand Generation برای تاریخ‌های مختلف
- ✅ Transaction Management

### 3) Scenario Tests
- ✅ منشی برای هفته آینده برنامه تنظیم می‌کند
- ✅ منشی برای تاریخ‌های خاص برنامه تنظیم می‌کند
- ✅ کاربر به صفحه SelectTime می‌رود (On-Demand Generation)

---

## 🎯 نتیجه‌گیری

### وضعیت فعلی
- ✅ **منطق نهایی:** برنامه هفتگی است و منشی برای تاریخ‌های خاص برنامه تنظیم می‌کند
- ✅ **تولید اسلات:** فقط برای همان تاریخ خاص تولید می‌شود
- ✅ **On-Demand Generation:** برای تاریخ‌های دیگر، اسلات‌ها On-Demand تولید می‌شوند
- ✅ **رعایت تقویم شمسی:** شنبه = اولین روز هفته (مطابق time.ir)

### وضعیت مطلوب
- ✅ **یکپارچه‌سازی کامل:** منطق نهایی پیاده‌سازی شده
- ✅ **تست کامل:** نیاز به Unit/Integration/Scenario Tests
- ✅ **گارانتی:** تضمین تولید اسلات برای تاریخ‌های خاص و On-Demand

---

**END OF DOCUMENTATION**

