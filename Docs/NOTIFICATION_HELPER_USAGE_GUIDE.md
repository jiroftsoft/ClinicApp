# 📢 راهنمای استفاده از NotificationHelper.js

**تاریخ:** 1404/10/05 (2025-12-25)  
**فایل:** `Content/js/notification-helper.js`  
**نوع:** JavaScript Utility Library  
**وضعیت:** ✅ **Production Ready**

---

## 📋 فهرست مطالب

1. [معرفی](#1-معرفی)
2. [نصب و راه‌اندازی](#2-نصب-و-راهاندازی)
3. [API Reference](#3-api-reference)
4. [مثال‌های کاربردی](#4-مثالهای-کاربردی)
5. [تنظیمات پیشرفته](#5-تنظیمات-پیشرفته)

---

## 1️⃣ معرفی

`NotificationHelper.js` یک کتابخانه سبک و قدرتمند برای نمایش پیام‌های کاربرپسند در فرم پذیرش است.

### ویژگی‌ها:
- ✅ **پیام‌های Toastr:** برای اطلاع‌رسانی‌های سریع
- ✅ **پیام‌های SweetAlert2:** برای تأییدیه‌ها و پیام‌های مهم
- ✅ **RTL Support:** پشتیبانی کامل از راست‌چین
- ✅ **Fallback:** در صورت عدم وجود کتابخانه، از `alert()` و `confirm()` استفاده می‌کند
- ✅ **Zero Dependency:** بدون وابستگی به فریمورک خاص
- ✅ **Medical Optimized:** رنگ‌ها و تنظیمات مناسب برای محیط پزشکی

---

## 2️⃣ نصب و راه‌اندازی

### الف) Include در Layout:

```html
<!-- در _ReceptionLayout.cshtml -->
<!-- Toastr (Required) -->
<link href="~/Content/plugins/toastr/toastr.min.css" rel="stylesheet" />
<script src="~/Content/plugins/toastr/toastr.min.js"></script>

<!-- SweetAlert2 (Required) -->
<script src="~/Content/plugins/SweetAlert2/sweetalert2@11.js"></script>

<!-- NotificationHelper (Main) -->
<script src="~/Content/js/notification-helper.js"></script>
```

### ب) بررسی Load شدن:

```javascript
// در Console مرورگر:
console.log(NotificationHelper); // باید object را نشان دهد
console.log(Notify);             // Alias (کوتاه‌تر)
```

---

## 3️⃣ API Reference

### **1. Success Message**
```javascript
NotificationHelper.success(message, title, options);
// یا
Notify.success(message, title, options);
```

**پارامترها:**
- `message` (string): متن پیام (اجباری)
- `title` (string, optional): عنوان پیام (پیش‌فرض: "موفقیت")
- `options` (object, optional): تنظیمات اضافی Toastr

**مثال:**
```javascript
Notify.success('پذیرش با موفقیت ثبت شد');
Notify.success('پذیرش با موفقیت ثبت شد', 'موفقیت');
Notify.success('پذیرش با موفقیت ثبت شد', 'موفقیت', { timeOut: 3000 });
```

---

### **2. Error Message**
```javascript
NotificationHelper.error(message, title, options);
```

**مثال:**
```javascript
Notify.error('خطا در ثبت پذیرش');
Notify.error('کد ملی نامعتبر است', 'خطای اعتبارسنجی');
```

---

### **3. Warning Message**
```javascript
NotificationHelper.warning(message, title, options);
```

**مثال:**
```javascript
Notify.warning('جلسه صندوق باز نیست');
Notify.warning('بیمار بیمه ندارد', 'هشدار');
```

---

### **4. Info Message**
```javascript
NotificationHelper.info(message, title, options);
```

**مثال:**
```javascript
Notify.info('در حال بارگذاری اطلاعات بیمه...');
Notify.info('محاسبات بیمه کامل شد', 'اطلاعات');
```

---

### **5. Confirm Dialog**
```javascript
NotificationHelper.confirm(message, title, onConfirm, onCancel, options);
```

**پارامترها:**
- `message` (string): متن پیام
- `title` (string, optional): عنوان
- `onConfirm` (function, optional): callback در صورت تأیید
- `onCancel` (function, optional): callback در صورت انصراف
- `options` (object, optional): تنظیمات اضافی SweetAlert2

**مثال:**
```javascript
Notify.confirm(
    'آیا از حذف این آیتم مطمئن هستید؟',
    'تأیید حذف',
    function() {
        // کد حذف
        console.log('حذف شد');
    },
    function() {
        // کد انصراف
        console.log('انصراف');
    }
);
```

---

### **6. Success Alert (SweetAlert)**
```javascript
NotificationHelper.successAlert(message, title, callback, options);
```

**مثال:**
```javascript
Notify.successAlert(
    'پذیرش با موفقیت نهایی شد',
    'موفقیت',
    function() {
        location.reload(); // Reload بعد از بستن
    }
);
```

---

### **7. Critical Error**
```javascript
NotificationHelper.criticalError(message, title, options);
```

**مثال:**
```javascript
Notify.criticalError(
    'ارتباط با سرور قطع شد. لطفاً صفحه را رفرش کنید.',
    'خطای بحرانی'
);
```

---

### **8. Show Loading**
```javascript
NotificationHelper.showLoading(message);
```

**مثال:**
```javascript
Notify.showLoading('در حال پردازش پرداخت...');

// بعد از اتمام:
setTimeout(() => {
    Notify.hideLoading();
    Notify.success('پرداخت موفق');
}, 3000);
```

---

### **9. Hide Loading**
```javascript
NotificationHelper.hideLoading();
```

---

### **10. Clear All Notifications**
```javascript
NotificationHelper.clearAll(); // پاک کردن تمام Toastr ها
NotificationHelper.clearLast(); // پاک کردن آخرین Toastr
```

---

## 4️⃣ مثال‌های کاربردی

### **Scenario 1: ثبت پذیرش موفق**
```javascript
// بعد از دریافت Response موفق از سرور:
$.ajax({
    url: '/api/reception/create',
    method: 'POST',
    data: formData,
    success: function(response) {
        if (response.success) {
            Notify.success('پذیرش با موفقیت ثبت شد', 'موفقیت');
        }
    },
    error: function(xhr) {
        Notify.error('خطا در ثبت پذیرش', 'خطا');
    }
});
```

---

### **Scenario 2: تأیید حذف آیتم**
```javascript
$('#btnDeleteItem').on('click', function() {
    Notify.confirm(
        'آیا از حذف این خدمت مطمئن هستید؟',
        'تأیید حذف',
        function() {
            // حذف آیتم
            deleteItem(itemId);
        }
    );
});
```

---

### **Scenario 3: Loading در حین پرداخت POS**
```javascript
function processPosPayment(amount) {
    // نمایش Loading
    Notify.showLoading('در حال اتصال به دستگاه POS...');
    
    // ارسال درخواست
    $.ajax({
        url: '/api/pos/payment',
        method: 'POST',
        data: { amount: amount },
        success: function(response) {
            Notify.hideLoading();
            
            if (response.success) {
                Notify.successAlert(
                    'پرداخت با موفقیت انجام شد',
                    'موفقیت',
                    function() {
                        location.reload();
                    }
                );
            } else {
                Notify.error(response.message, 'خطای پرداخت');
            }
        },
        error: function() {
            Notify.hideLoading();
            Notify.criticalError('خطا در ارتباط با دستگاه POS');
        }
    });
}
```

---

### **Scenario 4: Warning برای بیمار بدون بیمه**
```javascript
function checkPatientInsurance(patientId) {
    if (!patientHasInsurance(patientId)) {
        Notify.warning(
            'این بیمار بیمه ندارد. تمام هزینه‌ها به عهده بیمار است.',
            'هشدار'
        );
    }
}
```

---

### **Scenario 5: Info برای محاسبات**
```javascript
function calculateInsurance() {
    Notify.info('در حال محاسبه سهم بیمه...', 'اطلاعات');
    
    // محاسبه
    setTimeout(() => {
        Notify.clearLast(); // پاک کردن Info
        Notify.success('محاسبات بیمه کامل شد');
    }, 2000);
}
```

---

## 5️⃣ تنظیمات پیشرفته

### **الف) تغییر موقعیت Toastr:**
```javascript
// در فایل notification-helper.js:
positionClass: "toast-top-left"    // بالا - چپ (پیش‌فرض RTL)
// یا
positionClass: "toast-top-right"   // بالا - راست
positionClass: "toast-bottom-left" // پایین - چپ
positionClass: "toast-bottom-right"// پایین - راست
```

---

### **ب) تغییر زمان نمایش:**
```javascript
// در فراخوانی:
Notify.success('پیام', 'عنوان', {
    timeOut: 10000 // 10 ثانیه
});
```

---

### **ج) تغییر رنگ دکمه‌های SweetAlert:**
```javascript
Notify.confirm(
    'پیام',
    'عنوان',
    onConfirm,
    onCancel,
    {
        confirmButtonColor: '#00796b', // سبز
        cancelButtonColor: '#e53935'   // قرمز
    }
);
```

---

### **د) غیرفعال کردن ProgressBar:**
```javascript
Notify.success('پیام', 'عنوان', {
    progressBar: false
});
```

---

## 📊 مقایسه با روش‌های قبلی

### **قبل:**
```javascript
// استفاده مستقیم از toastr
toastr.options = {
    closeButton: true,
    progressBar: true,
    // ... 20 خط تنظیمات
};
toastr.success('پیام');
```

### **بعد:**
```javascript
// استفاده از NotificationHelper
Notify.success('پیام');
```

**مزایا:**
- ✅ کوتاه‌تر (1 خط vs 20+ خط)
- ✅ تنظیمات یکپارچه
- ✅ Fallback خودکار
- ✅ TypeScript-like API

---

## 🧪 تست

### **Test 1: بررسی Load شدن**
```javascript
// در Console:
console.log(NotificationHelper); // باید object باشد
console.log(Notify);             // باید object باشد
```

### **Test 2: تست پیام‌ها**
```javascript
// در Console:
Notify.success('تست موفقیت');
Notify.error('تست خطا');
Notify.warning('تست هشدار');
Notify.info('تست اطلاعات');
```

### **Test 3: تست تأیید**
```javascript
// در Console:
Notify.confirm('تست تأیید', 'عنوان', function() {
    console.log('تأیید شد');
}, function() {
    console.log('انصراف شد');
});
```

---

## ⚠️ نکات مهم

### **1. Toastr و SweetAlert2 باید Load شده باشند:**
```html
<!-- قبل از notification-helper.js -->
<script src="~/Content/plugins/toastr/toastr.min.js"></script>
<script src="~/Content/plugins/SweetAlert2/sweetalert2@11.js"></script>
```

### **2. Fallback خودکار:**
اگر Toastr یا SweetAlert2 موجود نباشد، از `alert()` و `confirm()` استفاده می‌کند.

### **3. Global Namespace:**
دو نام global تعریف شده:
- `NotificationHelper` (کامل)
- `Notify` (کوتاه - **توصیه می‌شود**)

---

## 🎯 Best Practices

### **1. استفاده از Alias کوتاه:**
```javascript
// ✅ GOOD
Notify.success('پیام');

// ❌ BAD (طولانی)
NotificationHelper.success('پیام');
```

### **2. استفاده مناسب از نوع پیام:**
```javascript
// ✅ GOOD
Notify.success('عملیات موفق');     // برای موفقیت
Notify.error('خطا');                // برای خطا
Notify.warning('هشدار');            // برای هشدار
Notify.info('اطلاعات');             // برای اطلاعات

// ❌ BAD
Notify.success('خطا');              // استفاده نادرست از نوع
```

### **3. استفاده از Confirm برای عملیات مخرب:**
```javascript
// ✅ GOOD
Notify.confirm('آیا مطمئن هستید؟', 'حذف', deleteItem);

// ❌ BAD (بدون تأیید)
deleteItem(); // مستقیماً حذف
```

---

## 📚 منابع مرتبط

- [Toastr Documentation](https://github.com/CodeSeven/toastr)
- [SweetAlert2 Documentation](https://sweetalert2.github.io/)
- `Content/js/admin-notification-service.js` - نسخه Admin
- `Docs/RECEPTION_LAYOUT_IMPLEMENTATION_REPORT.md` - گزارش Layout

---

**تاریخ تکمیل:** 1404/10/05  
**وضعیت:** ✅ **Production Ready**  
**نگارش:** 1.0.0

---

🎉 **آماده استفاده!** 🎉

