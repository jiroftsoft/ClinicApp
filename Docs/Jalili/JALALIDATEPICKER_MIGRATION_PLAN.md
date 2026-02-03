# 📋 برنامه Migration از Persian DatePicker به JalaliDatePicker

**تاریخ:** 1404/10/15  
**وضعیت:** 🔄 **در حال بررسی**

---

## 🔍 **مرحله 1: بررسی تفاوت‌های API**

### 1.1. پلاگین فعلی (Persian DatePicker - babakhani)

**API:**
```javascript
$input.pDatepicker({
    calendarType: 'persian',
    format: 'YYYY/MM/DD',
    initialValue: false,
    observer: false,
    minDate: new Date(),
    onSelect: function(unix) { ... }
});
```

**Selector:**
```html
<input data-persian-datepicker="true" />
```

**Data Attributes:**
- `data-persian-datepicker="true"`
- `data-no-default-date="true"`

---

### 1.2. پلاگین جدید (JalaliDatePicker - majidh1)

**API:**
```javascript
// Global initialization
jalaliDatepicker.startWatch({
    minDate: { year: 1404, month: 10, day: 15 },
    maxDate: { year: 1404, month: 12, day: 29 },
    initDate: null, // یا { year, month, day }
    date: true,
    time: false,
    showTodayBtn: true,
    showEmptyBtn: true,
    hideAfterChange: true
});

// Manual show
jalaliDatepicker.show(inputElement);
```

**Selector:**
```html
<input data-jdp />
```

**Data Attributes:**
- `data-jdp` (برای فعال‌سازی)
- `data-jdp-min-date` (حداقل تاریخ)
- `data-jdp-max-date` (حداکثر تاریخ)
- `data-jdp-only-date` (فقط تاریخ)
- `data-jdp-only-time` (فقط زمان)

**مستندات:** [JalaliDatePicker GitHub](https://github.com/majidh1/JalaliDatePicker)

---

## 📊 **مرحله 2: مقایسه ویژگی‌ها**

| ویژگی | Persian DatePicker | JalaliDatePicker |
|-------|-------------------|------------------|
| **No Dependencies** | ❌ (نیاز به jQuery) | ✅ (بدون وابستگی) |
| **Lightweight** | ❌ (حجم بیشتر) | ✅ (سبک‌تر) |
| **API ساده** | ❌ (پیچیده) | ✅ (ساده‌تر) |
| **مشکلات initialization** | ❌ (مشکل highlight خودکار) | ✅ (ساده‌تر) |
| **Server-side date** | ✅ (پشتیبانی می‌شود) | ✅ (پشتیبانی می‌شود) |
| **minDate/maxDate** | ✅ | ✅ |
| **no-default-date** | ❌ (مشکل دارد) | ✅ (با `initDate: null`) |

---

## 🎯 **مرحله 3: برنامه Migration**

### 3.1. فایل‌های مورد نیاز

**فایل‌های JalaliDatePicker:**
- ✅ `Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.js` (موجود است)
- ✅ `Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.css` (موجود است)

**فایل‌های جدید:**
- `Content/js/jalali-datepicker-component.js` (wrapper component جدید)

---

### 3.2. تغییرات در Component

**قبل (Persian DatePicker):**
```javascript
var PersianDatePickerComponent = {
    config: {
        selector: 'input[data-persian-datepicker="true"]',
        // ...
    },
    initializeDatePicker: function($input) {
        $input.pDatepicker(datePickerConfig);
    }
};
```

**بعد (JalaliDatePicker):**
```javascript
var JalaliDatePickerComponent = {
    config: {
        selector: 'input[data-jdp]',
        // ...
    },
    initializeDatePicker: function(input) {
        // JalaliDatePicker خودش با data-jdp کار می‌کند
        // فقط باید startWatch را یک بار فراخوانی کنیم
    }
};
```

---

### 3.3. تغییرات در Views

**قبل:**
```html
<input type="text" 
       name="SelectedDate" 
       data-persian-datepicker="true"
       data-no-default-date="true" />
```

**بعد:**
```html
<input type="text" 
       name="SelectedDate" 
       data-jdp
       data-jdp-min-date="1404/10/15" />
```

---

### 3.4. تغییرات در Script Loading

**قبل:**
```html
<link href="~/Content/js/plugins/persian-datepicker/persian-datepicker.min.css" rel="stylesheet" />
<script src="~/Content/js/plugins/persian-datepicker/jalaali.min.js"></script>
<script src="~/Content/js/plugins/persian-datepicker/persian-date.min.js"></script>
<script src="~/Content/js/plugins/persian-datepicker/persian-datepicker.min.js"></script>
<script src="~/Content/js/persian-datepicker-component.js"></script>
```

**بعد:**
```html
<link href="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.css" rel="stylesheet" />
<script src="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.js"></script>
<script src="~/Content/js/jalali-datepicker-component.js"></script>
```

---

## ✅ **مرحله 4: مزایای Migration**

1. **✅ بدون وابستگی:** JalaliDatePicker بدون jQuery کار می‌کند
2. **✅ سبک‌تر:** حجم کمتر و عملکرد بهتر
3. **✅ API ساده‌تر:** استفاده و نگهداری آسان‌تر
4. **✅ بدون مشکل initialization:** مشکل highlight خودکار حل می‌شود
5. **✅ پشتیبانی بهتر:** پلاگین فعال‌تر و به‌روزتر

---

## ⚠️ **مرحله 5: نکات مهم**

1. **Hidden Input:** باید بررسی شود که hidden input برای فرم POST همچنان کار می‌کند
2. **Event Handling:** باید بررسی شود که event handling (onSelect) همچنان کار می‌کند
3. **Server-side Date:** باید بررسی شود که دریافت تاریخ از سرور همچنان کار می‌کند
4. **minDate/maxDate:** باید بررسی شود که minDate/maxDate از سرور set می‌شود

---

## 📝 **مرحله 6: فایل‌های نیاز به تغییر**

### 6.1. JavaScript Files
- ✅ `Content/js/persian-datepicker-component.js` → `Content/js/jalali-datepicker-component.js` (جدید)
- ✅ `Content/js/persian-datepicker-manager.js` → حذف یا نگه‌داری برای backward compatibility

### 6.2. View Files
- ✅ `Areas/Admin/Views/Shared/_PersianDatePicker.cshtml`
- ✅ `Areas/Admin/Views/Shared/_PersianDatePickerScript.cshtml`
- ✅ `Areas/Patient/Views/AppointmentBooking/SelectDate.cshtml`
- ✅ سایر View هایی که از `data-persian-datepicker` استفاده می‌کنند

### 6.3. Layout Files
- ✅ `Areas/Admin/Views/Shared/_AdminLayout.cshtml`
- ✅ `Areas/Patient/Views/Shared/_PatientLayoutPro.cshtml`
- ✅ `Views/Shared/_Layout.cshtml`

---

## 🚀 **مرحله 7: مراحل اجرا**

1. ✅ **بررسی و تست JalaliDatePicker** در محیط development
2. ✅ **ایجاد wrapper component جدید** (`jalali-datepicker-component.js`)
3. ✅ **تغییر selector ها** از `data-persian-datepicker` به `data-jdp`
4. ✅ **به‌روزرسانی View ها** و Partial ها
5. ✅ **به‌روزرسانی Script Loading** در Layout ها
6. ✅ **تست کامل** در تمام صفحات
7. ✅ **حذف فایل‌های قدیمی** (اختیاری - برای backward compatibility)

---

## 📚 **مراجع:**

- [JalaliDatePicker GitHub](https://github.com/majidh1/JalaliDatePicker)
- [JalaliDatePicker Documentation](https://majidh1.github.io/JalaliDatePicker/)
- `Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.js` (کد source)

---

**✅ برنامه Migration آماده است. آماده برای شروع پیاده‌سازی؟**

