# 📘 راهنمای کامل به‌روزرسانی به JalaliDatePicker Enterprise Component

**نسخه:** 2.0.0  
**تاریخ:** 1404/10/15  
**وضعیت:** ✅ Production-Ready

---

## 📋 **فهرست مطالب**

1. [مقدمه](#مقدمه)
2. [مراحل به‌روزرسانی](#مراحل-به‌روزرسانی)
3. [تغییرات در View (Razor)](#تغییرات-در-view-razor)
4. [تغییرات در JavaScript](#تغییرات-در-javascript)
5. [تغییرات در Layout](#تغییرات-در-layout)
6. [مثال‌های عملی](#مثال‌های-عملی)
7. [Checklist](#checklist)
8. [مشکلات رایج و راه‌حل‌ها](#مشکلات-رایج-و-راه‌حل‌ها)
9. [تست و اعتبارسنجی](#تست-و-اعتبارسنجی)

---

## 🎯 **مقدمه**

این راهنما برای به‌روزرسانی ماژول‌های قدیمی از **Persian DatePicker (babakhani)** به **JalaliDatePicker Enterprise Component** طراحی شده است.

### ✅ **مزایای JalaliDatePicker Enterprise:**
- ✅ بدون وابستگی به jQuery
- ✅ سبک‌تر و سریع‌تر
- ✅ API ساده‌تر
- ✅ دریافت تاریخ امروز از سرور
- ✅ Production-Ready و Bulletproof
- ✅ UI/UX بهینه برای محیط‌های درمانی
- ✅ پشتیبانی از Themes و Sizes
- ✅ Re-initialization خودکار برای محتوای داینامیک

### 📍 **فهرست محل‌های قدیمی (Legacy)**

هر وقت گفته شد **«datepicker قدیمی»** یا **«بروز کن به نسخه جدید»**، فایل مرجع زیر لیست تمام Viewها و اسکریپت‌هایی است که هنوز از DatePicker قدیمی استفاده می‌کنند و باید به Enterprise مهاجرت شوند:

- **`Docs/Jalili/JALALIDATEPICKER_LEGACY_LOCATIONS.md`** — محل‌های دقیق، جدول انجام‌شده/باقی‌مانده، و اقدام پیشنهادی برای هر فایل.

---

## 🔄 **مراحل به‌روزرسانی**

### **مرحله 1: بررسی Layout**

ابتدا بررسی کنید که آیا JalaliDatePicker Enterprise در Layout لود شده است یا نه.

#### ✅ **اگر در Layout لود شده است:**
- نیازی به تغییر نیست
- مستقیماً به مرحله 2 بروید

#### ❌ **اگر در Layout لود نشده است:**
- به بخش [تغییرات در Layout](#تغییرات-در-layout) بروید

---

### **مرحله 2: تغییرات در View (Razor)**

#### **2.1. تغییر Input Attributes**

**قبل (Persian DatePicker):**
```html
<input type="text" 
       class="form-control persian-date" 
       data-persian-datepicker="true"
       placeholder="تاریخ را انتخاب کنید" />
```

**بعد (JalaliDatePicker Enterprise):**
```html
<input type="text" 
       class="form-control persian-date-input" 
       data-jdp
       data-jdp-theme="medical"
       data-jdp-size="medium"
       data-no-default-date="true"
       placeholder="تاریخ را انتخاب کنید" />
```

#### **2.2. Attributes مهم:**

| Attribute | توضیح | مقدار پیش‌فرض |
|-----------|-------|---------------|
| `data-jdp` | فعال‌سازی DatePicker | - |
| `data-jdp-theme` | Theme (medical, minimal, compact) | `medical` |
| `data-jdp-size` | Size (small, medium, large) | `medium` |
| `data-no-default-date` | جلوگیری از نمایش تاریخ پیش‌فرض | `false` |
| `data-jdp-init-date` | تاریخ اولیه (مثلاً: 1404/10/15) | - |
| `data-jdp-min-date` | حداقل تاریخ قابل انتخاب | - |
| `data-jdp-max-date` | حداکثر تاریخ قابل انتخاب | - |

#### **2.3. مثال کامل:**

```html
<!-- ✅ با تاریخ اولیه -->
<input type="text" 
       class="form-control" 
       name="StartDate"
       data-jdp
       data-jdp-theme="medical"
       data-jdp-size="medium"
       data-jdp-init-date="1404/10/15"
       placeholder="تاریخ شروع" />

<!-- ✅ بدون تاریخ پیش‌فرض -->
<input type="text" 
       class="form-control" 
       name="EndDate"
       data-jdp
       data-jdp-theme="medical"
       data-jdp-size="medium"
       data-no-default-date="true"
       placeholder="تاریخ پایان" />

<!-- ✅ با محدودیت تاریخ -->
<input type="text" 
       class="form-control" 
       name="AppointmentDate"
       data-jdp
       data-jdp-theme="medical"
       data-jdp-size="medium"
       data-jdp-min-date="1404/10/15"
       data-jdp-max-date="1404/12/29"
       placeholder="تاریخ نوبت" />
```

---

### **مرحله 3: تغییرات در JavaScript**

#### **3.1. حذف Initialization قدیمی**

**قبل (Persian DatePicker):**
```javascript
// ❌ حذف کنید
$('.persian-date').pDatepicker({
    initialValue: false,
    calendarType: 'persian',
    format: 'YYYY/MM/DD',
    observer: true
});
```

**بعد (JalaliDatePicker Enterprise):**
```javascript
// ✅ نیازی به initialization دستی نیست!
// DatePicker به صورت خودکار initialize می‌شود
// فقط مطمئن شوید که Component لود شده است
```

#### **3.2. تغییر Event Listeners**

**قبل (Persian DatePicker):**
```javascript
// ❌ حذف کنید
$('.persian-date').on('pDatepicker:select', function(e) {
    var date = $(this).val();
    // ...
});
```

**بعد (JalaliDatePicker Enterprise):**
```javascript
// ✅ استفاده از pDatepicker:select (backward compatible)
document.querySelector('.persian-date-input').addEventListener('pDatepicker:select', function(e) {
    var date = this.value;
    // ...
});

// ✅ یا استفاده از jdp:change (native event)
document.querySelector('.persian-date-input').addEventListener('jdp:change', function(e) {
    var date = this.value;
    // ...
});
```

#### **3.3. Initialization برای Inputهای داینامیک**

**قبل (Persian DatePicker):**
```javascript
// ❌ حذف کنید
function addNewDateInput() {
    var html = '<input type="text" class="persian-date" />';
    $('#container').append(html);
    $('.persian-date').pDatepicker({
        initialValue: false,
        format: 'YYYY/MM/DD'
    });
}
```

**بعد (JalaliDatePicker Enterprise):**
```javascript
// ✅ استفاده از JalaliDatePickerEnterprise.init()
function addNewDateInput() {
    var html = '<input type="text" class="persian-date-input" data-jdp data-jdp-theme="medical" />';
    var $newItem = $(html);
    $('#container').append($newItem);
    
    // ✅ Initialize manually
    if (typeof JalaliDatePickerEnterprise !== 'undefined') {
        JalaliDatePickerEnterprise.init($newItem[0], {
            theme: 'medical',
            size: 'medium',
            noDefaultDate: true
        });
    } else {
        // ✅ Retry logic
        var retryCount = 0;
        var maxRetries = 10;
        var retryInterval = setInterval(function() {
            retryCount++;
            if (typeof JalaliDatePickerEnterprise !== 'undefined') {
                clearInterval(retryInterval);
                JalaliDatePickerEnterprise.init($newItem[0], {
                    theme: 'medical',
                    size: 'medium',
                    noDefaultDate: true
                });
            } else if (retryCount >= maxRetries) {
                clearInterval(retryInterval);
                console.error('❌ JalaliDatePickerEnterprise failed to load');
            }
        }, 200);
    }
}
```

#### **3.4. دریافت تاریخ امروز از سرور**

**قبل (Persian DatePicker):**
```javascript
// ❌ حذف کنید
var today = new Date();
```

**بعد (JalaliDatePicker Enterprise):**
```javascript
// ✅ استفاده از getTodayFromServer
if (typeof JalaliDatePickerEnterprise !== 'undefined') {
    JalaliDatePickerEnterprise.getTodayFromServer().then(function(todayPersian) {
        console.log('تاریخ امروز:', todayPersian); // "1404/10/15"
        // استفاده از todayPersian
    }).catch(function(error) {
        console.warn('⚠️ Failed to get today from server:', error);
        // Fallback logic
    });
}
```

#### **3.5. تبدیل تاریخ شمسی به میلادی**

**قبل (Persian DatePicker):**
```javascript
// ❌ حذف کنید
var gregorianDate = $input.pDatepicker('getDate');
```

**بعد (JalaliDatePicker Enterprise):**
```javascript
// ✅ استفاده از convertPersianToGregorian
if (typeof JalaliDatePickerEnterprise !== 'undefined') {
    var persianDate = '1404/10/15';
    var gregorianDate = JalaliDatePickerEnterprise.convertPersianToGregorian(persianDate);
    if (gregorianDate) {
        console.log('تاریخ میلادی:', gregorianDate); // Date object
    }
}
```

---

### **مرحله 4: تغییرات در Layout**

#### **4.1. بررسی Layout**

ابتدا بررسی کنید که آیا JalaliDatePicker Enterprise در Layout لود شده است:

```html
<!-- ✅ باید این فایل‌ها لود شده باشند -->
<link href="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.css" rel="stylesheet" />
<link href="~/Content/css/jalali-datepicker-enterprise.css" rel="stylesheet" />
<script src="~/Content/js/plugins/PersianDateTimePicker/jalaali.js"></script>
<script src="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.js"></script>
<script src="~/Content/js/jalali-datepicker-enterprise.js"></script>
```

#### **4.2. اضافه کردن به Layout (اگر لود نشده است)**

**در `_AdminLayout.cshtml` یا `_PatientLayoutPro.cshtml`:**

```html
<!-- ✅ JalaliDatePicker Enterprise (Production-Ready) -->
<link href="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.css" rel="stylesheet" />
<link href="~/Content/css/jalali-datepicker-enterprise.css" rel="stylesheet" />
<!-- ✅ CRITICAL: jalaali.js باید قبل از jalalidatepicker.min.js لود شود -->
<script src="~/Content/js/plugins/PersianDateTimePicker/jalaali.js"></script>
<script src="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.js"></script>
<script src="~/Content/js/jalali-datepicker-enterprise.js"></script>

<!-- ✅ Enterprise: Re-initialize for dynamic content -->
<script>
    (function () {
        'use strict';
        
        // ✅ Re-initialize for dynamic content (AJAX loaded content)
        function reinitializeDatePickers() {
            if (typeof JalaliDatePickerEnterprise !== 'undefined') {
                JalaliDatePickerEnterprise.initializeAll();
            }
        }
        
        // ✅ Listen for dynamically added inputs
        if (typeof MutationObserver !== 'undefined') {
            var observer = new MutationObserver(function(mutations) {
                mutations.forEach(function(mutation) {
                    mutation.addedNodes.forEach(function(node) {
                        if (node.nodeType === 1) { // Element node
                            var $node = typeof jQuery !== 'undefined' ? $(node) : null;
                            if ($node && ($node.is('input[data-jdp]') || $node.find('input[data-jdp]').length > 0)) {
                                setTimeout(reinitializeDatePickers, 100);
                            }
                        }
                    });
                });
            });
            
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        } else if (typeof jQuery !== 'undefined') {
            // ✅ Fallback for older browsers
            $(document).on('DOMNodeInserted', function (e) {
                var $target = $(e.target);
                if ($target.is('input[data-jdp]') || $target.find('input[data-jdp]').length > 0) {
                    setTimeout(reinitializeDatePickers, 100);
                }
            });
        }
    })();
</script>
```

#### **4.3. حذف Scripts قدیمی**

**❌ حذف کنید:**
```html
<!-- ❌ حذف کنید -->
<link href="~/Content/js/plugins/PersianDateTimePicker/persian-datepicker.min.css" rel="stylesheet" />
<script src="~/Content/js/plugins/PersianDateTimePicker/persian-datepicker.min.js"></script>
```

---

## 📝 **مثال‌های عملی**

### **مثال 1: فرم ساده**

**قبل:**
```html
<div class="form-group">
    <label>تاریخ شروع</label>
    <input type="text" 
           class="form-control persian-date" 
           data-persian-datepicker="true"
           name="StartDate" />
</div>
```

**بعد:**
```html
<div class="form-group">
    <label>تاریخ شروع</label>
    <input type="text" 
           class="form-control persian-date-input" 
           data-jdp
           data-jdp-theme="medical"
           data-jdp-size="medium"
           data-no-default-date="true"
           name="StartDate" />
</div>
```

---

### **مثال 2: Input داینامیک**

**قبل:**
```javascript
$('#addDateBtn').on('click', function() {
    var html = '<input type="text" class="persian-date" />';
    $('#container').append(html);
    $('.persian-date').pDatepicker({
        initialValue: false,
        format: 'YYYY/MM/DD'
    });
});
```

**بعد:**
```javascript
$('#addDateBtn').on('click', function() {
    var html = '<input type="text" class="persian-date-input" data-jdp data-jdp-theme="medical" />';
    var $newItem = $(html);
    $('#container').append($newItem);
    
    // ✅ Initialize manually
    if (typeof JalaliDatePickerEnterprise !== 'undefined') {
        JalaliDatePickerEnterprise.init($newItem[0], {
            theme: 'medical',
            size: 'medium',
            noDefaultDate: true
        });
    }
});
```

---

### **مثال 3: Event Handling**

**قبل:**
```javascript
$('.persian-date').on('pDatepicker:select', function() {
    var date = $(this).val();
    console.log('تاریخ انتخاب شده:', date);
});
```

**بعد:**
```javascript
// ✅ استفاده از pDatepicker:select (backward compatible)
document.querySelectorAll('.persian-date-input').forEach(function(input) {
    input.addEventListener('pDatepicker:select', function(e) {
        var date = this.value;
        console.log('تاریخ انتخاب شده:', date);
    });
});

// ✅ یا استفاده از jdp:change (native event)
document.querySelectorAll('.persian-date-input').forEach(function(input) {
    input.addEventListener('jdp:change', function(e) {
        var date = this.value;
        console.log('تاریخ انتخاب شده:', date);
    });
});
```

---

### **مثال 4: دریافت تاریخ امروز**

**قبل:**
```javascript
var today = new Date();
var todayPersian = convertToPersian(today);
```

**بعد:**
```javascript
// ✅ استفاده از getTodayFromServer
if (typeof JalaliDatePickerEnterprise !== 'undefined') {
    JalaliDatePickerEnterprise.getTodayFromServer().then(function(todayPersian) {
        console.log('تاریخ امروز:', todayPersian); // "1404/10/15"
        // استفاده از todayPersian
    }).catch(function(error) {
        console.warn('⚠️ Failed to get today from server:', error);
        // Fallback logic
    });
}
```

---

### **مثال 5: تبدیل تاریخ**

**قبل:**
```javascript
var gregorianDate = $input.pDatepicker('getDate');
```

**بعد:**
```javascript
// ✅ استفاده از convertPersianToGregorian
if (typeof JalaliDatePickerEnterprise !== 'undefined') {
    var persianDate = '1404/10/15';
    var gregorianDate = JalaliDatePickerEnterprise.convertPersianToGregorian(persianDate);
    if (gregorianDate) {
        console.log('تاریخ میلادی:', gregorianDate); // Date object
    }
}
```

---

## ✅ **Checklist**

### **مرحله 1: بررسی Layout**
- [ ] بررسی کنید که JalaliDatePicker Enterprise در Layout لود شده است
- [ ] اگر لود نشده است، به بخش [تغییرات در Layout](#تغییرات-در-layout) بروید

### **مرحله 2: تغییرات در View**
- [ ] تغییر `data-persian-datepicker="true"` به `data-jdp`
- [ ] اضافه کردن `data-jdp-theme="medical"`
- [ ] اضافه کردن `data-jdp-size="medium"`
- [ ] اضافه کردن `data-no-default-date="true"` (اگر نیاز است)
- [ ] تغییر class از `persian-date` به `persian-date-input` (اختیاری)

### **مرحله 3: تغییرات در JavaScript**
- [ ] حذف `$('.persian-date').pDatepicker({...})`
- [ ] تغییر Event Listeners از `pDatepicker:select` به `pDatepicker:select` یا `jdp:change`
- [ ] اضافه کردن Initialization برای Inputهای داینامیک
- [ ] تغییر `getTodayFromServer()` (اگر استفاده می‌شود)
- [ ] تغییر `convertPersianToGregorian()` (اگر استفاده می‌شود)

### **مرحله 4: حذف Scripts قدیمی**
- [ ] حذف `persian-datepicker.min.css`
- [ ] حذف `persian-datepicker.min.js`
- [ ] حذف Initialization قدیمی در Layout

### **مرحله 5: تست**
- [ ] تست DatePicker در صفحه
- [ ] تست انتخاب تاریخ
- [ ] تست Inputهای داینامیک
- [ ] تست Event Handling
- [ ] تست تبدیل تاریخ
- [ ] تست دریافت تاریخ امروز از سرور

---

## 🐛 **مشکلات رایج و راه‌حل‌ها**

### **مشکل 1: DatePicker initialize نمی‌شود**

**علت:** Component لود نشده است

**راه‌حل:**
```javascript
// ✅ بررسی کنید که Component لود شده است
if (typeof JalaliDatePickerEnterprise === 'undefined') {
    console.error('❌ JalaliDatePickerEnterprise not loaded');
    // بررسی کنید که Scripts در Layout لود شده‌اند
}
```

---

### **مشکل 2: Inputهای داینامیک initialize نمی‌شوند**

**علت:** MutationObserver کار نمی‌کند یا Component لود نشده است

**راه‌حل:**
```javascript
// ✅ Initialize manually
if (typeof JalaliDatePickerEnterprise !== 'undefined') {
    JalaliDatePickerEnterprise.init(inputElement, {
        theme: 'medical',
        size: 'medium',
        noDefaultDate: true
    });
} else {
    // ✅ Retry logic
    var retryCount = 0;
    var maxRetries = 10;
    var retryInterval = setInterval(function() {
        retryCount++;
        if (typeof JalaliDatePickerEnterprise !== 'undefined') {
            clearInterval(retryInterval);
            JalaliDatePickerEnterprise.init(inputElement, {
                theme: 'medical',
                size: 'medium',
                noDefaultDate: true
            });
        } else if (retryCount >= maxRetries) {
            clearInterval(retryInterval);
            console.error('❌ JalaliDatePickerEnterprise failed to load');
        }
    }, 200);
}
```

---

### **مشکل 3: تاریخ پیش‌فرض نمایش داده می‌شود**

**علت:** `data-no-default-date="true"` set نشده است

**راه‌حل:**
```html
<!-- ✅ اضافه کردن data-no-default-date="true" -->
<input type="text" 
       data-jdp
       data-jdp-theme="medical"
       data-no-default-date="true" />
```

---

### **مشکل 4: Event trigger نمی‌شود**

**علت:** Event Listener اشتباه است

**راه‌حل:**
```javascript
// ✅ استفاده از pDatepicker:select (backward compatible)
input.addEventListener('pDatepicker:select', function(e) {
    var date = this.value;
    // ...
});

// ✅ یا استفاده از jdp:change (native event)
input.addEventListener('jdp:change', function(e) {
    var date = this.value;
    // ...
});
```

---

### **مشکل 5: تبدیل تاریخ کار نمی‌کند**

**علت:** `jalaali.js` لود نشده است یا قبل از `jalalidatepicker.min.js` لود نشده است

**راه‌حل:**
```html
<!-- ✅ CRITICAL: jalaali.js باید قبل از jalalidatepicker.min.js لود شود -->
<script src="~/Content/js/plugins/PersianDateTimePicker/jalaali.js"></script>
<script src="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.js"></script>
```

---

## 🧪 **تست و اعتبارسنجی**

### **تست 1: Basic Initialization**
```javascript
// ✅ بررسی کنید که DatePicker initialize می‌شود
var input = document.querySelector('input[data-jdp]');
if (input && input.dataset.jdpInitialized === 'true') {
    console.log('✅ DatePicker initialized');
} else {
    console.error('❌ DatePicker not initialized');
}
```

---

### **تست 2: Event Handling**
```javascript
// ✅ تست Event Handling
var input = document.querySelector('input[data-jdp]');
input.addEventListener('pDatepicker:select', function(e) {
    console.log('✅ pDatepicker:select event fired');
});
input.addEventListener('jdp:change', function(e) {
    console.log('✅ jdp:change event fired');
});
```

---

### **تست 3: Dynamic Inputs**
```javascript
// ✅ تست Inputهای داینامیک
function testDynamicInput() {
    var html = '<input type="text" data-jdp data-jdp-theme="medical" />';
    var $newItem = $(html);
    $('#container').append($newItem);
    
    setTimeout(function() {
        if ($newItem[0].dataset.jdpInitialized === 'true') {
            console.log('✅ Dynamic input initialized');
        } else {
            console.error('❌ Dynamic input not initialized');
        }
    }, 500);
}
```

---

### **تست 4: Date Conversion**
```javascript
// ✅ تست تبدیل تاریخ
if (typeof JalaliDatePickerEnterprise !== 'undefined') {
    var persianDate = '1404/10/15';
    var gregorianDate = JalaliDatePickerEnterprise.convertPersianToGregorian(persianDate);
    if (gregorianDate) {
        console.log('✅ Date conversion works');
    } else {
        console.error('❌ Date conversion failed');
    }
}
```

---

### **تست 5: Server Date**
```javascript
// ✅ تست دریافت تاریخ امروز از سرور
if (typeof JalaliDatePickerEnterprise !== 'undefined') {
    JalaliDatePickerEnterprise.getTodayFromServer().then(function(todayPersian) {
        console.log('✅ Server date received:', todayPersian);
    }).catch(function(error) {
        console.error('❌ Failed to get server date:', error);
    });
}
```

---

## 📚 **مراجع**

- **راهنمای کامل:** `Docs/JALALIDATEPICKER_ENTERPRISE_GUIDE.md`
- **Migration Plan:** `Docs/JALALIDATEPICKER_MIGRATION_PLAN.md`
- **Migration Complete:** `Docs/JALALIDATEPICKER_MIGRATION_COMPLETE.md`
- **Production Checklist:** `Docs/JALALIDATEPICKER_PRODUCTION_CHECKLIST.md`
- **Final Review:** `Docs/JALALIDATEPICKER_FINAL_REVIEW.md`

---

## 🎯 **خلاصه**

### **تغییرات اصلی:**

1. **View:** `data-persian-datepicker="true"` → `data-jdp`
2. **JavaScript:** حذف `pDatepicker()` → استفاده از `JalaliDatePickerEnterprise.init()`
3. **Events:** `pDatepicker:select` → `pDatepicker:select` یا `jdp:change`
4. **Layout:** اضافه کردن Scripts Enterprise Component

### **نکات مهم:**

- ✅ Component به صورت خودکار initialize می‌شود
- ✅ برای Inputهای داینامیک، از `JalaliDatePickerEnterprise.init()` استفاده کنید
- ✅ از `getTodayFromServer()` برای دریافت تاریخ امروز استفاده کنید
- ✅ از `convertPersianToGregorian()` برای تبدیل تاریخ استفاده کنید

---

**✅ این راهنما برای استفاده در Cursor AI طراحی شده است تا به‌روزرسانی را به صورت خودکار انجام دهد.**

