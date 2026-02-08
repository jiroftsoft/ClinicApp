# 📘 JalaliDatePicker Enterprise Component - راهنمای کامل

> **💡 نکته:** برای به‌روزرسانی ماژول‌های قدیمی، به [راهنمای به‌روزرسانی](./JALALIDATEPICKER_MIGRATION_GUIDE.md) مراجعه کنید.

**تاریخ:** 1404/10/15  
**نسخه:** 2.0.0  
**وضعیت:** ✅ Production-Ready

---

## 🎯 **معرفی**

JalaliDatePicker Enterprise Component یک کامپوننت Enterprise-Grade برای استفاده از JalaliDatePicker در کل پروژه است که:

- ✅ **Production-Ready**: بهینه‌سازی شده برای محیط production
- ✅ **Reusable**: قابل استفاده مجدد در کل پروژه
- ✅ **Bulletproof**: مقاوم و ضد گلوله
- ✅ **Best Practices**: طبق best practices
- ✅ **Customizable**: قابل سفارشی‌سازی
- ✅ **UI/UX Optimized**: بهینه شده برای تجربه کاربری بهتر

---

## 📦 **نصب و راه‌اندازی**

### 1. فایل‌های مورد نیاز

```html
<!-- CSS -->
<link href="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.css" rel="stylesheet" />
<link href="~/Content/css/jalali-datepicker-enterprise.css" rel="stylesheet" />

<!-- JavaScript -->
<script src="~/Content/js/plugins/PersianDateTimePicker/jalaali.js"></script>
<script src="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.js"></script>
<script src="~/Content/js/jalali-datepicker-enterprise.js"></script>
```

### 2. استفاده در View

```html
<!-- استفاده ساده -->
<input type="text" data-jdp />

<!-- استفاده با theme -->
<input type="text" data-jdp data-jdp-theme="medical" />

<!-- استفاده با size -->
<input type="text" data-jdp data-jdp-size="large" />

<!-- استفاده با min/max date -->
<input type="text" 
       data-jdp 
       data-jdp-min-date="1404/10/15"
       data-jdp-max-date="1404/12/29" />
```

---

## 🎨 **Themes**

### Medical Theme (پیش‌فرض)
```html
<input data-jdp data-jdp-theme="medical" />
```
- رنگ آبی (#2196F3)
- مناسب برای محیط‌های پزشکی
- دکمه‌های واضح و قابل دسترس

### Minimal Theme
```html
<input data-jdp data-jdp-theme="minimal" />
```
- طراحی مینیمال
- بدون دکمه‌های اضافی
- مناسب برای فرم‌های ساده

### Compact Theme
```html
<input data-jdp data-jdp-theme="compact" />
```
- طراحی فشرده
- مناسب برای فضاهای محدود
- DropDown برای سال غیرفعال

---

## 📏 **Sizes**

### Small
```html
<input data-jdp data-jdp-size="small" />
```
- فونت کوچک‌تر
- padding کمتر
- مناسب برای جدول‌ها

### Medium (پیش‌فرض)
```html
<input data-jdp data-jdp-size="medium" />
```
- اندازه استاندارد
- مناسب برای اکثر موارد

### Large
```html
<input data-jdp data-jdp-size="large" />
```
- فونت بزرگ‌تر
- padding بیشتر
- مناسب برای صفحات مهم

---

## ⚙️ **Configuration**

### استفاده از Data Attributes

```html
<input type="text" 
       data-jdp
       data-jdp-theme="medical"
       data-jdp-size="large"
       data-jdp-min-date="1404/10/15"
       data-jdp-max-date="1404/12/29"
       data-jdp-init-date="1404/10/20"
       data-no-default-date="true" />
```

### استفاده از JavaScript

```javascript
// Manual initialization
var picker = JalaliDatePickerEnterprise.init('#myDateInput', {
    theme: 'medical',
    size: 'large',
    minDate: { year: 1404, month: 10, day: 15 },
    maxDate: { year: 1404, month: 12, day: 29 },
    noDefaultDate: true,
    onSelect: function(persianDate, dateObj) {
        console.log('Selected:', persianDate);
    }
});

// Get instance
var picker = JalaliDatePickerEnterprise.getInstance('#myDateInput');

// Set date programmatically
picker.setDate('1404/10/15');
picker.setDate({ year: 1404, month: 10, day: 15 });

// Get date
var date = picker.getDate(); // { year: 1404, month: 10, day: 15 }

// Show/Hide
picker.show();
picker.hide();

// Destroy
picker.destroy();
```

### استفاده از JSON Config

```html
<input type="text" 
       data-jdp
       data-jdp-config='{"theme": "medical", "size": "large", "noDefaultDate": true}' />
```

---

## 🎯 **API Reference**

### Methods

#### `init(input, config)`
Initialize کردن DatePicker

**Parameters:**
- `input` (HTMLElement|string): input element یا selector
- `config` (Object): configuration object

**Returns:** Instance object

#### `getInstance(input)`
دریافت instance موجود

**Parameters:**
- `input` (HTMLElement|string): input element یا selector

**Returns:** Instance object یا null

#### `initializeAll()`
Initialize کردن تمام DatePicker ها در صفحه

#### `getTodayFromServer()`
دریافت تاریخ امروز از سرور

**Returns:** Promise<string>

---

### Instance Methods

#### `setDate(date)`
تنظیم تاریخ

**Parameters:**
- `date` (string|Object): تاریخ شمسی (string) یا object {year, month, day}

#### `getDate()`
دریافت تاریخ فعلی

**Returns:** Object {year, month, day} یا null

#### `show()`
نمایش DatePicker

#### `hide()`
مخفی کردن DatePicker

#### `destroy()`
حذف instance

---

## 🎨 **Customization**

### Custom Theme

```javascript
// اضافه کردن theme جدید
JalaliDatePickerEnterprise.config.themes.myTheme = {
    zIndex: 10000,
    container: 'body',
    showTodayBtn: true,
    showEmptyBtn: true
};

// استفاده
<input data-jdp data-jdp-theme="myTheme" />
```

### Custom Size

```javascript
// اضافه کردن size جدید
JalaliDatePickerEnterprise.config.sizes.xlarge = {
    inputClass: 'form-control form-control-xl',
    topSpace: 15,
    bottomSpace: 15
};

// استفاده
<input data-jdp data-jdp-size="xlarge" />
```

---

## 📱 **Responsive Design**

کامپوننت به صورت خودکار responsive است:

- **Desktop**: نمایش کامل با تمام ویژگی‌ها
- **Tablet**: بهینه‌سازی شده برای صفحه‌های متوسط
- **Mobile**: نمایش تمام صفحه با UI بهینه

---

## ♿ **Accessibility**

- ✅ Keyboard Navigation Support
- ✅ Screen Reader Support
- ✅ Focus Management
- ✅ ARIA Attributes
- ✅ WCAG 2.1 AA Compliance

---

## 🚀 **Performance**

- ✅ Lazy Loading
- ✅ Caching برای تاریخ امروز
- ✅ Event Debouncing
- ✅ Memory Management
- ✅ Instance Registry

---

## 🐛 **Error Handling**

کامپوننت به صورت خودکار خطاها را handle می‌کند:

- ✅ Network Errors
- ✅ Invalid Dates
- ✅ Server Errors
- ✅ Retry Logic

---

## 📝 **Examples**

### مثال 1: استفاده ساده

```html
<input type="text" 
       name="BirthDate" 
       data-jdp 
       data-jdp-theme="medical" />
```

### مثال 2: با min/max date

```html
<input type="text" 
       name="AppointmentDate" 
       data-jdp 
       data-jdp-theme="medical"
       data-jdp-size="large"
       data-jdp-min-date="1404/10/15"
       data-jdp-max-date="1404/12/29" />
```

### مثال 3: با callback

```javascript
JalaliDatePickerEnterprise.init('#myDateInput', {
    theme: 'medical',
    size: 'large',
    onSelect: function(persianDate, dateObj) {
        console.log('Selected:', persianDate);
        // Do something with selected date
    }
});
```

### مثال 4: در Partial View

```razor
@{
    ViewBag.PersianDatePickerId = "appointmentDate";
    ViewBag.PersianDatePickerName = "AppointmentDate";
    ViewBag.PersianDatePickerValue = Model.AppointmentDate;
    ViewBag.PersianDatePickerLabel = "تاریخ نوبت";
    ViewBag.PersianDatePickerSize = "large";
}
@Html.Partial("_PersianDatePicker")
```

---

## 🔧 **Troubleshooting**

### مشکل: DatePicker initialize نمی‌شود

**راه حل:**
1. بررسی کنید که `jalaali.js` قبل از `jalalidatepicker.min.js` لود شده باشد
2. بررسی کنید که `jalali-datepicker-enterprise.js` لود شده باشد
3. بررسی console برای خطاها

### مشکل: تاریخ تبدیل نمی‌شود

**راه حل:**
1. بررسی کنید که `jalaali` library در دسترس است
2. بررسی کنید که تاریخ با فرمت صحیح است (YYYY/MM/DD)
3. بررسی console برای خطاها

### مشکل: Theme اعمال نمی‌شود

**راه حل:**
1. بررسی کنید که `jalali-datepicker-enterprise.css` لود شده باشد
2. بررسی کنید که theme name صحیح است
3. بررسی console برای خطاها

### مشکل: DatePicker داخل مودال باز نمی‌شود یا تقویم پشت مودال پنهان است

**علت:** z-index پیش‌فرض پایین‌تر از مودال Bootstrap است؛ یا اینپوتها بعد از اجرای اولیهٔ `startWatch` (مثلاً با AJAX) به DOM اضافه شده‌اند.

**راه حل (خلاصه):**
1. در `config.defaultOptions` مقدارهای `container: 'body'` و `zIndex: 1060` را تنظیم کنید.
2. متد `startWatchAgain()` را در ماژول اضافه کنید و آن را **بعد از باز شدن مودال** یا **بعد از لود محتوای AJAX** فراخوانی کنید.
3. جزئیات کامل و چک‌لیست: **پایگاه دانش** → [01-Helpers-DateTime.md](../../Contracts/Knowledge-Base/AI/Master/01-Helpers-DateTime.md) → بخش **«۹. استفاده از DatePicker داخل مودال»**.

---

## 📚 **مراجع**

- [JalaliDatePicker GitHub](https://github.com/majidh1/JalaliDatePicker)
- [JalaliDatePicker Documentation](https://majidh1.github.io/JalaliDatePicker/)
- [Codepen Examples](https://codepen.io/collection/wajWMo)

---

**✅ کامپوننت آماده استفاده در production است!**

