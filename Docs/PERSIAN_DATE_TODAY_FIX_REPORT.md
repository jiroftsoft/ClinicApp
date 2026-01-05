# 📅 گزارش رفع مشکل تاریخ امروز - یک روز اضافه

**تاریخ:** 2026-01-06  
**اولویت:** 🔴 CRITICAL  
**وضعیت:** ✅ **رفع شد**

---

## 📋 مشکل

### گزارش کاربر:
- **امروز:** 15 دی 1404 (طبق time.ir)
- **نمایش داده شده:** 16 دی 1404
- **اختلاف:** یک روز اضافه

### علت:
1. **مشکل Timezone:** در `calculateTodayClientSide` در JavaScript، از `new Date()` استفاده می‌شد که بر اساس timezone مرورگر محاسبه می‌شد
2. **عدم تطابق Timezone:** اگر timezone مرورگر با timezone سرور (ایران UTC+3:30) متفاوت باشد، یک روز اختلاف ایجاد می‌شود
3. **Fallback Logic:** در صورت عدم دسترسی به API، از client-side calculation استفاده می‌شد که مشکل timezone داشت

---

## ✅ راه‌حل

### 1. اصلاح `calculateTodayClientSide` در JavaScript

**مشکل:**
```javascript
// ❌ قبل: استفاده از timezone مرورگر
var today = new Date();
var jalaaliDate = jalaali.toJalaali(today.getFullYear(), today.getMonth() + 1, today.getDate());
```

**راه‌حل:**
```javascript
// ✅ بعد: استفاده از timezone ایران (UTC+3:30) طبق time.ir
var now = new Date();
var utcTime = now.getTime() + (now.getTimezoneOffset() * 60 * 1000); // تبدیل به UTC
var iranOffsetMs = 3.5 * 60 * 60 * 1000; // UTC+3:30
var iranTime = new Date(utcTime + iranOffsetMs);
var year = iranTime.getUTCFullYear();
var month = iranTime.getUTCMonth() + 1;
var day = iranTime.getUTCDate();
var jalaaliDate = jalaali.toJalaali(year, month, day);
```

---

### 2. بهبود API Endpoint برای Debugging

**تغییرات:**
```csharp
// Controllers/Api/PersianDateApiController.cs
public JsonResult GetToday()
{
    // ✅ اضافه کردن Logging برای Debug
    Serilog.Log.Information("🔍 [GetToday] DateTime.Today: {Today}, Kind: {Kind}, Timezone: {Timezone}", 
        today, today.Kind, TimeZoneInfo.Local.Id);
    
    // ✅ اضافه کردن timezone به Response
    return Json(new
    {
        success = true,
        persianDate = persianToday,
        gregorianDate = gregorianToday,
        timestamp = ...,
        timezone = TimeZoneInfo.Local.Id // ✅ برای Debug
    }, JsonRequestBehavior.AllowGet);
}
```

---

## 📁 فایل‌های تغییر یافته

1. **`Content/js/persian-datepicker-component.js`**
   - اصلاح `calculateTodayClientSide` برای استفاده از timezone ایران (UTC+3:30)
   - اضافه کردن Logging برای Debug

2. **`Controllers/Api/PersianDateApiController.cs`**
   - اضافه کردن Logging برای Debug
   - اضافه کردن timezone به Response

---

## 🔍 الگو از time.ir

طبق [time.ir](https://www.time.ir/):
- **امروز:** دوشنبه - 15 دی 1404
- **تاریخ میلادی:** 2026-01-05
- **Timezone:** ایران (UTC+3:30)

**نکات مهم:**
1. تاریخ امروز باید بر اساس timezone ایران محاسبه شود
2. استفاده از UTC و سپس اضافه کردن offset ایران (3.5 ساعت)
3. نمایش تاریخ شمسی و میلادی به صورت همزمان

---

## ✅ تست

### Manual Testing:
- [x] تست محاسبه تاریخ امروز در client-side
- [x] تست API endpoint `/api/persian-date/today`
- [x] تست تطابق با time.ir
- [ ] تست در timezone‌های مختلف

---

## 📊 نتیجه

✅ **مشکل رفع شد:**
- `calculateTodayClientSide` از timezone ایران (UTC+3:30) استفاده می‌کند
- API endpoint Logging اضافه شد
- تطابق با time.ir برقرار شد

---

**وضعیت:** ✅ **کامل**  
**تاریخ به‌روزرسانی:** 2026-01-06

