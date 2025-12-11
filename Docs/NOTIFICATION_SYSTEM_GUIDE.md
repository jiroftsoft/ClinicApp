# راهنمای سیستم اعلان‌رسانی (Notification System)

## 📋 فهرست مطالب
1. [معرفی](#معرفی)
2. [استفاده از NotificationHelper](#استفاده-از-notificationhelper)
3. [استفاده از AdminNotification (JavaScript)](#استفاده-از-adminnotification-javascript)
4. [استفاده از SweetAlert](#استفاده-از-sweetalert)
5. [بهترین روش‌ها](#بهترین-روش‌ها)

---

## معرفی

سیستم اعلان‌رسانی این پروژه بر اساس اصول زیر طراحی شده است:

- ✅ **SRP (Single Responsibility Principle)**: هر کلاس/تابع یک مسئولیت دارد
- ✅ **Strongly-Typed**: استفاده از ViewModels به جای ViewBag/ViewData
- ✅ **Production-Ready**: آماده برای محیط production
- ✅ **User-Friendly**: پیام‌های کاربرپسند و زیبا

### کتابخانه‌های استفاده شده:
- **Toastr**: برای پیام‌های عادی (Success, Error, Warning, Info)
- **SweetAlert2**: برای پیام‌های مهم و تأییدیه‌ها

---

## استفاده از NotificationHelper

### در Controller

```csharp
using ClinicApp.Helpers;

public class MyController : Controller
{
    public ActionResult Create(MyViewModel model)
    {
        try
        {
            // عملیات موفق
            NotificationHelper.SetSuccess(TempData, "مقاله با موفقیت ایجاد شد");
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // عملیات ناموفق
            NotificationHelper.SetError(TempData, "خطا در ایجاد مقاله");
            return View(model);
        }
    }
}
```

### انواع پیام‌ها:

```csharp
// پیام موفقیت
NotificationHelper.SetSuccess(TempData, "عملیات با موفقیت انجام شد", "موفقیت");

// پیام خطا
NotificationHelper.SetError(TempData, "خطایی رخ داده است", "خطا");

// پیام هشدار
NotificationHelper.SetWarning(TempData, "هشدار: این عملیات قابل بازگشت نیست", "هشدار");

// پیام اطلاعات
NotificationHelper.SetInfo(TempData, "اطلاعات مهم", "اطلاعات");
```

---

## استفاده از AdminNotification (JavaScript)

### در View یا JavaScript:

```javascript
// پیام موفقیت
AdminNotification.success("مقاله با موفقیت ایجاد شد", "موفقیت");

// پیام خطا
AdminNotification.error("خطا در ایجاد مقاله", "خطا");

// پیام هشدار
AdminNotification.warning("هشدار: این عملیات قابل بازگشت نیست", "هشدار");

// پیام اطلاعات
AdminNotification.info("اطلاعات مهم", "اطلاعات");
```

---

## استفاده از SweetAlert

### برای تأییدیه:

```javascript
AdminNotification.confirm(
    "آیا مطمئن هستید که می‌خواهید این مقاله را حذف کنید؟",
    "تأیید حذف",
    function() {
        // در صورت تأیید
        // انجام عملیات حذف
    },
    function() {
        // در صورت انصراف
        console.log("عملیات لغو شد");
    }
);
```

### برای پیام‌های بحرانی:

```javascript
AdminNotification.criticalError(
    "خطای مهمی رخ داده است. لطفاً صفحه را refresh کنید.",
    "خطای بحرانی"
);
```

### برای پیام‌های موفقیت مهم:

```javascript
AdminNotification.successAlert(
    "مقاله با موفقیت ایجاد شد",
    "موفقیت"
);
```

---

## بهترین روش‌ها

### ✅ DO (انجام دهید):

1. **استفاده از NotificationHelper در Controller**:
   ```csharp
   NotificationHelper.SetSuccess(TempData, "پیام موفقیت");
   ```

2. **استفاده از Strongly-Typed ViewModels**:
   ```csharp
   // ✅ درست
   var model = new MyViewModel { Message = "پیام" };
   return View(model);
   
   // ❌ اشتباه
   ViewBag.Message = "پیام";
   ```

3. **استفاده از SweetAlert برای عملیات حساس**:
   ```javascript
   AdminNotification.confirm("آیا مطمئن هستید؟", "تأیید", onConfirm, onCancel);
   ```

### ❌ DON'T (انجام ندهید):

1. **استفاده مستقیم از TempData در View**:
   ```csharp
   // ❌ اشتباه
   @if (TempData["Success"] != null) { ... }
   
   // ✅ درست - استفاده از NotificationHelper
   NotificationHelper.SetSuccess(TempData, "پیام");
   ```

2. **استفاده از ViewBag/ViewData برای داده‌های حساس**:
   ```csharp
   // ❌ اشتباه
   ViewBag.UserId = userId;
   
   // ✅ درست
   var model = new MyViewModel { UserId = userId };
   return View(model);
   ```

3. **استفاده از alert() برای پیام‌ها**:
   ```javascript
   // ❌ اشتباه
   alert("پیام");
   
   // ✅ درست
   AdminNotification.success("پیام");
   ```

---

## مثال کامل

### Controller:

```csharp
public class BlogPostController : BaseCMSController
{
    [HttpPost]
    public async Task<ActionResult> Create(BlogPostCreateEditViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(GetViewPath("Create"), model);
            }

            var result = await _blogPostService.CreateBlogPostAsync(model);

            if (!result.Success)
            {
                NotificationHelper.SetError(TempData, result.Message);
                return View(GetViewPath("Create"), model);
            }

            NotificationHelper.SetSuccess(TempData, "مقاله با موفقیت ایجاد شد");
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ایجاد مقاله");
            NotificationHelper.SetError(TempData, "خطا در ایجاد مقاله");
            return View(GetViewPath("Create"), model);
        }
    }
}
```

### View:

```html
@* پیام‌ها به صورت خودکار با Toaster نمایش داده می‌شوند *@
@* نیازی به کد اضافی نیست *@
```

---

## نکات مهم

1. **پیام‌ها به صورت خودکار نمایش داده می‌شوند**: پس از استفاده از `NotificationHelper` در Controller، پیام‌ها به صورت خودکار در View نمایش داده می‌شوند.

2. **ViewBag.Title مجاز است**: استفاده از `ViewBag.Title` برای عنوان صفحه مجاز است (مورد غیر حساس).

3. **ViewBag/ViewData برای داده‌های حساس ممنوع است**: برای داده‌های حساس از ViewModels استفاده کنید.

4. **SRP رعایت شده است**: هر کلاس/تابع یک مسئولیت دارد.

---

## خلاصه

- ✅ استفاده از `NotificationHelper` در Controller
- ✅ استفاده از `AdminNotification` در JavaScript
- ✅ استفاده از `SweetAlert` برای تأییدیه‌ها
- ✅ حذف `TempData` alerts از View
- ✅ استفاده از Strongly-Typed ViewModels
- ✅ رعایت SRP و Production-Ready

---

**تاریخ ایجاد**: 2024  
**نسخه**: 1.0  
**نویسنده**: ClinicApp Development Team

