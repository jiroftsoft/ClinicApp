# 🔧 گزارش رفع مشکل دکمه "امروز" در PersianDatePicker

**تاریخ:** 2026-01-06  
**اولویت:** 🔴 CRITICAL  
**وضعیت:** 🔄 **در حال بررسی**

---

## 📋 مشکل

### گزارش کاربر:
- **امروز:** 15 دی 1404 (طبق time.ir)
- **نمایش داده شده:** 16 دی 1404
- **سناریو:** کلیک روی دکمه "امروز" در PersianDatePicker

### علت احتمالی:
1. **PersianDatePicker خودش تاریخ امروز را محاسبه می‌کند** - از timezone مرورگر استفاده می‌کند
2. **عدم استفاده از API** - دکمه "امروز" از client-side calculation استفاده می‌کند
3. **مشکل Timezone** - timezone مرورگر با timezone سرور متفاوت است

---

## ✅ راه‌حل Enterprise-Grade

### **قانون طلایی:**
> **"همه چیز از Backend - هیچ client-side calculation برای تاریخ امروز"**

---

## 🔧 پیاده‌سازی

### **1. Override دکمه "امروز" در PersianDatePicker**

```javascript
// ✅ بعد از initialize شدن datePicker
var datePickerInstance = $input.data('pDatepicker');
if (datePickerInstance) {
    // ✅ Override دکمه "امروز"
    var $todayBtn = datePickerInstance.$container.find('.pdp-today-btn, .pdp-toolbox-today');
    if ($todayBtn.length > 0) {
        $todayBtn.off('click').on('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            // ✅ دریافت تاریخ امروز از سرور
            var serverTodayPersian = $input.data('server-today-persian');
            if (serverTodayPersian) {
                // ✅ تبدیل تاریخ شمسی به میلادی برای setDate
                var gregorianDate = self.convertPersianToGregorian(serverTodayPersian);
                if (gregorianDate) {
                    datePickerInstance.setDate(new Date(gregorianDate));
                    $input.val(serverTodayPersian);
                    $input.trigger('change');
                    self.logger.log('✅ دکمه "امروز" کلیک شد - استفاده از تاریخ از سرور:', serverTodayPersian);
                }
            } else {
                // ✅ Fallback: دریافت از API
                self.getTodayFromServer().then(function(todayPersianDate) {
                    if (todayPersianDate) {
                        var gregorianDate = self.convertPersianToGregorian(todayPersianDate);
                        if (gregorianDate) {
                            datePickerInstance.setDate(new Date(gregorianDate));
                            $input.val(todayPersianDate);
                            $input.trigger('change');
                        }
                    }
                });
            }
        });
    }
}
```

---

## 📊 فایل‌های نیاز به تغییر

1. **`Content/js/persian-datepicker-component.js`**
   - Override دکمه "امروز" بعد از initialize
   - استفاده از تاریخ از API

2. **`Controllers/Api/PersianDateApiController.cs`** ✅ (قبلاً انجام شد)
   - استفاده از UTC → Iran conversion

---

## ✅ چک‌لیست

- [ ] Override دکمه "امروز" در PersianDatePicker
- [ ] استفاده از تاریخ از API (نه client-side calculation)
- [ ] تست در timezone‌های مختلف
- [ ] تست در مرورگرهای مختلف

---

**وضعیت:** 🔄 **در حال پیاده‌سازی**  
**تاریخ به‌روزرسانی:** 2026-01-06

