# 🔧 Double Booking Logic Fix - Applied

**Date:** 2026-01-06  
**Module:** Appointment Booking / Reserve  
**Issue:** "شما در این تاریخ و زمان قبلاً نوبت دارید" خطا به اشتباه نمایش داده می‌شد

---

## 🐛 Problem

خطای "شما در این تاریخ و زمان قبلاً نوبت دارید" به اشتباه نمایش داده می‌شد حتی زمانی که:
- نوبت قبلی وجود نداشت
- یا نوبت‌های مجاور (adjacent) overlap نداشتند

---

## 🔍 Root Cause Analysis

### Issue 1: منطق Overlap اشتباه
**Evidence:** `Repositories/Appointment/AppointmentRepository.cs:282-286`

**مشکل:**
- منطق قبلی از `>=` و `<=` استفاده می‌کرد که نوبت‌های مجاور را به اشتباه overlap تشخیص می‌داد
- مثال: نوبت 10:00-10:15 و 10:15-10:30 به اشتباه overlap تشخیص داده می‌شد

**راه‌حل:**
- استفاده از فرمول استاندارد overlap: `(A.Start < B.End) AND (A.End > B.Start)`
- استفاده از `<` و `>` (نه `<=` و `>=`) برای جلوگیری از overlap نوبت‌های مجاور

### Issue 2: شامل کردن نوبت‌های غیرفعال
**Evidence:** `Repositories/Appointment/AppointmentRepository.cs:290`

**مشکل:**
- همه نوبت‌های غیر Cancelled (شامل Completed, NoShow) در double booking check لحاظ می‌شدند
- این باعث می‌شد که بیمار نتواند بعد از یک نوبت Completed یا NoShow دوباره نوبت بگیرد

**راه‌حل:**
- فقط نوبت‌های فعال (Scheduled, Pending) را در نظر بگیریم
- نوبت‌های Completed, NoShow, Cancelled را ignore کنیم

---

## ✅ Fixes Applied

### Fix 1: اصلاح منطق Overlap
**File:** `Repositories/Appointment/AppointmentRepository.cs`

**Before:**
```sql
AND (
    (AppointmentDate >= @p3 AND AppointmentDate < @p4) OR
    (DATEADD(MINUTE, Duration, AppointmentDate) > @p3 AND DATEADD(MINUTE, Duration, AppointmentDate) <= @p4) OR
    (AppointmentDate <= @p3 AND DATEADD(MINUTE, Duration, AppointmentDate) >= @p4)
)
```

**After:**
```sql
AND AppointmentDate < @p4
AND DATEADD(MINUTE, Duration, AppointmentDate) > @p3
```

**توضیح:**
- فرمول استاندارد overlap: `(A.Start < B.End) AND (A.End > B.Start)`
- استفاده از `<` و `>` برای جلوگیری از overlap نوبت‌های مجاور

### Fix 2: محدود کردن Status
**File:** `Repositories/Appointment/AppointmentRepository.cs`

**Before:**
```sql
AND Status != @p1  -- همه غیر Cancelled
```

**After:**
```sql
AND Status IN (@p1, @p2)  -- فقط Scheduled و Pending
```

**توضیح:**
- فقط نوبت‌های فعال (Scheduled, Pending) را در نظر می‌گیریم
- نوبت‌های Completed, NoShow, Cancelled را ignore می‌کنیم

### Fix 3: بهبود Logging
**File:** `Repositories/Appointment/AppointmentRepository.cs`

**تغییرات:**
- اضافه کردن جزئیات کامل برای debugging
- نمایش زمان دقیق نوبت‌های overlap
- نمایش Status و Duration برای هر نوبت

---

## 🧪 Test Scenarios

### Scenario 1: نوبت‌های مجاور (Adjacent)
- **نوبت قبلی:** 10:00-10:15
- **نوبت جدید:** 10:15-10:30
- **نتیجه:** ✅ No overlap (درست)

### Scenario 2: نوبت‌های Overlap
- **نوبت قبلی:** 10:00-10:15
- **نوبت جدید:** 10:10-10:20
- **نتیجه:** ✅ Overlap detected (درست)

### Scenario 3: نوبت Completed
- **نوبت قبلی:** 10:00-10:15 (Status = Completed)
- **نوبت جدید:** 10:00-10:15
- **نتیجه:** ✅ No overlap (درست - می‌تواند دوباره نوبت بگیرد)

### Scenario 4: نوبت Cancelled
- **نوبت قبلی:** 10:00-10:15 (Status = Cancelled)
- **نوبت جدید:** 10:00-10:15
- **نتیجه:** ✅ No overlap (درست - می‌تواند دوباره نوبت بگیرد)

---

## 📋 Verification Steps

1. **تست نوبت‌های مجاور:**
   - یک نوبت 10:00-10:15 رزرو کنید
   - سعی کنید نوبت 10:15-10:30 رزرو کنید
   - ✅ باید موفق شود

2. **تست نوبت‌های Overlap:**
   - یک نوبت 10:00-10:15 رزرو کنید
   - سعی کنید نوبت 10:10-10:20 رزرو کنید
   - ✅ باید خطای "شما در این تاریخ و زمان قبلاً نوبت دارید" نمایش دهد

3. **تست نوبت Completed:**
   - یک نوبت 10:00-10:15 با Status = Completed ایجاد کنید
   - سعی کنید نوبت 10:00-10:15 رزرو کنید
   - ✅ باید موفق شود

4. **بررسی لاگ‌ها:**
   - لاگ‌ها باید جزئیات کامل نوبت‌های overlap را نمایش دهند
   - ✅ باید زمان دقیق و Status را نمایش دهد

---

## 🔄 Rollback Plan

اگر مشکل پیش آمد:
1. Revert تغییرات در `Repositories/Appointment/AppointmentRepository.cs`
2. استفاده از منطق قبلی با `Status != Cancelled`
3. بررسی لاگ‌ها برای پیدا کردن مشکل

---

## ✅ Status

- ✅ منطق Overlap اصلاح شد
- ✅ Status محدود شد (فقط Scheduled و Pending)
- ✅ Logging بهبود یافت
- ✅ آماده برای تست

---

**Next Steps:**
1. تست با سناریوهای مختلف
2. بررسی لاگ‌ها برای اطمینان از صحت منطق
3. اگر مشکل دیگری وجود داشت، بررسی و رفع

