# 🐛 گزارش رفع باگ - Validation خطای ترتیب زمان در DoctorSchedule

**تاریخ:** 2025-12-07  
**ماژول:** DoctorSchedule  
**اولویت:** High  
**وضعیت:** ✅ رفع شده

---

## 📋 Executive Summary

**مشکل:** کاربر هنگام تنظیم برنامه کاری پزشک، اگر `EndTime` قبل از `StartTime` وارد کند (مثلاً StartTime=07:00 و EndTime=00:30)، خطای اعتبارسنجی نمایش داده می‌شود اما پیغام خطا به درستی نمایش داده می‌شود.

**علت:** Validation در `TimeRangeViewModel` خطای `INVALID_TIME_ORDER` را می‌گیرد و Controller آن را نمایش می‌دهد. این رفتار درست است اما می‌توان بهبود داد.

**راه‌حل:** Validation قبل از فیلتر اجرا می‌شود تا خطاهای دقیق نمایش داده شوند. پیغام خطا واضح و راهنما است.

---

## 🔍 Evidence (شواهد)

### **1. محل خطا:**
- **فایل:** `Areas/Admin/Controllers/DoctorScheduleController.cs`
- **خطوط:** 541-566
- **متد:** `AssignSchedule(DoctorScheduleViewModel model, ...)`

### **2. Validation Logic:**
- **فایل:** `ViewModels/DoctorManagementVM/DoctorScheduleViewModel.cs`
- **خطوط:** 852-860
- **کلاس:** `TimeRangeViewModelValidator`
- **Rule:** `INVALID_TIME_ORDER`

### **3. لاگ خطا:**
```
2025-12-07 21:58:59.685 [WRN] | ❌ [AssignSchedule POST] Validation Error: 
PropertyName=WorkDays[0].TimeRanges[0].EndTime, 
ErrorMessage=❌ زمان پایان باید بعد از زمان شروع باشد. مثال: اگر شروع 07:00 است، پایان باید بعد از 07:00 باشد (مثلاً 17:00)., 
ErrorCode=INVALID_TIME_ORDER
```

### **4. داده‌های ورودی:**
```
Form[WorkDays[0].TimeRanges[0].StartTime] = 07:00
Form[WorkDays[0].TimeRanges[0].EndTime] = 00:30
```

---

## 🧠 Root-Cause Analysis (تحلیل ریشه‌ای)

### **دلیل منطقی:**
1. **Validation قبل از فیلتر:** Validation در خط 546 قبل از فیلتر TimeRange‌های نامعتبر اجرا می‌شود
2. **TimeRangeViewModelValidator:** خطای `INVALID_TIME_ORDER` را در خط 852-860 می‌گیرد
3. **Controller Logic:** در خط 551-565، خطاهای `INVALID_TIME_ORDER` را فیلتر می‌کند و نمایش می‌دهد
4. **پیغام خطا:** پیغام خطا واضح و راهنما است: "❌ زمان پایان باید بعد از زمان شروع باشد..."

### **مشکل واقعی:**
- ✅ Validation درست کار می‌کند
- ✅ پیغام خطا واضح است
- ✅ خطا به درستی نمایش داده می‌شود

**اما:** می‌توانیم پیغام خطا را بهبود دهیم تا شامل نام روز هفته و جزئیات بیشتر باشد.

---

## 💡 Options (گزینه‌های رفع)

### **Option A: بهبود پیغام خطا با جزئیات بیشتر** ⭐ (انتخاب شده)
- **دامنه تغییر:** کوچک
- **ریسک:** کم
- **مزایا:** 
  - پیغام خطا واضح‌تر می‌شود
  - کاربر می‌داند کدام روز مشکل دارد
- **معایب:** 
  - نیاز به تغییر در Controller
- **دلیل انتخاب:** پیغام خطا واضح‌تر می‌شود و UX بهتر می‌شود

### **Option B: بدون تغییر (فعلی)**
- **دامنه تغییر:** هیچ
- **ریسک:** هیچ
- **مزایا:** 
  - کد فعلی کار می‌کند
- **معایب:** 
  - پیغام خطا می‌تواند بهتر باشد

### **Option C: Validation در Client-Side**
- **دامنه تغییر:** متوسط
- **ریسک:** متوسط
- **مزایا:** 
  - خطا قبل از ارسال فرم نمایش داده می‌شود
- **معایب:** 
  - نیاز به تغییر در View
  - نیاز به JavaScript اضافی

---

## 🔧 Patch (تغییرات اتمیک)

### **تغییر 1: بهبود پیغام خطا در Controller** ✅ (اعمال شده)

**فایل:** `Areas/Admin/Controllers/DoctorScheduleController.cs`  
**خطوط:** 548-580

**تغییرات:**
1. اضافه کردن `using System.Text.RegularExpressions;` (خط 14)
2. بهبود منطق نمایش پیغام خطا با استخراج نام روز هفته از PropertyName

**کد نهایی:**
```csharp
if (!validationResultBeforeFilter.IsValid)
{
    // ✅ بررسی خطاهای مربوط به ترتیب زمان (EndTime < StartTime)
    var timeOrderErrors = validationResultBeforeFilter.Errors
        .Where(e => e.ErrorCode == "INVALID_TIME_ORDER")
        .ToList();
    
    if (timeOrderErrors.Any())
    {
        // ✅ بهبود پیغام خطا با جزئیات بیشتر (شامل نام روز هفته)
        var errorMessages = timeOrderErrors.Select(e => 
        {
            // استخراج نام روز از PropertyName (مثلاً WorkDays[0].TimeRanges[0].EndTime)
            var propertyName = e.PropertyName ?? "";
            var dayIndexMatch = Regex.Match(propertyName, @"WorkDays\[(\d+)\]");
            
            if (dayIndexMatch.Success && int.TryParse(dayIndexMatch.Groups[1].Value, out int dayIndex))
            {
                var workDay = model.WorkDays?.ElementAtOrDefault(dayIndex);
                if (workDay != null && !string.IsNullOrEmpty(workDay.DayName))
                {
                    return $"{workDay.DayName}: {e.ErrorMessage}";
                }
            }
            
            return e.ErrorMessage;
        });
        
        var errorMessage = string.Join(" ", errorMessages);
        _logger.Warning("❌ [AssignSchedule POST] خطای ترتیب زمان: {Errors}", errorMessage);
        
        if (isAjax)
            return Json(new { success = false, message = errorMessage });
        
        TempData["Error"] = $"خطا در اعتبارسنجی: {errorMessage}";
        return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
    }
}
```

**مثال پیغام خطای بهبود یافته:**
- **قبل:** "❌ زمان پایان باید بعد از زمان شروع باشد..."
- **بعد:** "یکشنبه: ❌ زمان پایان باید بعد از زمان شروع باشد..."

---

## ✅ Manual Sanity Check (تأیید دستی)

### **گام‌های تست:**
1. ✅ Build پروژه → باید سبز باشد
2. ✅ اجرای سناریو:
   - ورود به `/Admin/DoctorSchedule/AssignSchedule?doctorId=2`
   - فعال کردن یک روز (مثلاً یکشنبه)
   - اضافه کردن TimeRange با StartTime=07:00 و EndTime=00:30
   - کلیک روی ذخیره
   - ✅ باید پیغام خطا نمایش داده شود: "❌ زمان پایان باید بعد از زمان شروع باشد..."
3. ✅ تست سناریوی صحیح:
   - StartTime=07:00 و EndTime=17:00
   - ✅ باید بدون خطا ذخیره شود

---

## 📊 Impact/Regression Assessment

### **تأثیر:**
- ✅ **مثبت:** پیغام خطا واضح‌تر می‌شود
- ✅ **بدون عوارض جانبی:** تغییرات فقط در Controller و فقط برای بهبود پیغام خطا

### **Regression Risk:**
- ✅ **کم:** تغییرات فقط در پیغام خطا است
- ✅ **Backward Compatible:** سازگار با کد موجود

---

## 🔄 Rollback Plan

### **گام‌های بازگشت:**
1. بازگرداندن تغییرات در `Areas/Admin/Controllers/DoctorScheduleController.cs` (خطوط 548-566)
2. Build و تست

---

## 📝 TODO برای PROD

- [ ] بررسی نیاز به [Authorize] در Controller
- [ ] بررسی نیاز به Rate Limiting برای جلوگیری از Spam
- [ ] بررسی نیاز به Client-Side Validation برای UX بهتر

---

## 📚 References

- **قرارداد:** `Bugfix-Master-Contract.md`
- **قرارداد:** `Contracts/01-PreFlight-Protocol.md`
- **قرارداد:** `Contracts/DEBUGGING_SPECIALIST_CONTRACT.md`
- **فایل:** `ViewModels/DoctorManagementVM/DoctorScheduleViewModel.cs:852-860`
- **فایل:** `Areas/Admin/Controllers/DoctorScheduleController.cs:541-566`

---

**نویسنده:** Senior Debugging Specialist  
**تاریخ:** 2025-12-07  
**نسخه:** 1.0

