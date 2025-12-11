# 📅 راهنمای استفاده از Component Persian DatePicker

**نسخه:** 1.0  
**تاریخ:** 1404/09/12  
**نویسنده:** تیم توسعه کلینیک شفا

---

## 📋 فهرست مطالب

1. [معرفی](#معرفی)
2. [نصب و راه‌اندازی](#نصب-و-راه‌اندازی)
3. [استفاده پایه](#استفاده-پایه)
4. [استفاده پیشرفته](#استفاده-پیشرفته)
5. [مدیریت در Controller](#مدیریت-در-controller)
6. [مثال‌های عملی](#مثال‌های-عملی)
7. [عیب‌یابی](#عیب‌یابی)

---

## 🎯 معرفی

Component Persian DatePicker یک راه‌حل قابل استفاده مجدد برای استفاده از Persian DatePicker در تمام فرم‌های پروژه است. این Component:

- ✅ **قابل استفاده مجدد** - یک بار کد، استفاده در همه جا
- ✅ **سازگار با Model Binding** - تبدیل خودکار تاریخ شمسی به میلادی
- ✅ **پشتیبانی کامل از Validation** - اعتبارسنجی خودکار
- ✅ **بهینه برای فارسی** - پشتیبانی کامل از RTL

---

## 🚀 نصب و راه‌اندازی

### فایل‌های Component:

1. `Areas/Admin/Views/Shared/_PersianDatePicker.cshtml` - Component اصلی
2. `Areas/Admin/Views/Shared/_PersianDatePickerScript.cshtml` - Script های لازم
3. `Helpers/PersianDateHelper.cs` - Helper برای تبدیل تاریخ

---

## 📝 استفاده پایه

### روش 1: استفاده از Partial View (توصیه می‌شود)

```csharp
@model MyModel

@using (Html.BeginForm())
{
    <div class="row">
        <div class="col-md-6">
            @{
                ViewBag.PersianDatePickerId = "startDatePicker";
                ViewBag.PersianDatePickerName = "StartDate";
                ViewBag.PersianDatePickerValue = Model.StartDate;
                ViewBag.PersianDatePickerLabel = "تاریخ شروع";
                ViewBag.PersianDatePickerPlaceholder = "تاریخ شروع (اختیاری)";
                ViewBag.PersianDatePickerHelpText = "اگر خالی باشد، اطلاعیه از همین الان فعال می‌شود";
                ViewBag.PersianDatePickerRequired = false;
                ViewBag.PersianDatePickerValidationMessage = Html.ValidationMessageFor(m => m.StartDate, "", new { @class = "text-danger" }).ToString();
            }
            @Html.Partial("_PersianDatePicker")
        </div>
    </div>
    
    <button type="submit" class="btn btn-primary">ذخیره</button>
}

@section Scripts {
    @Html.Partial("_PersianDatePickerScript")
}
```

### روش 2: استفاده از Helper Extension (پیشرفته)

```csharp
@Html.PersianDatePickerFor(m => m.StartDate, new { @class = "form-control" })
```

---

## 🎨 استفاده پیشرفته

### تنظیمات کامل Component:

```csharp
@{
    ViewBag.PersianDatePickerId = "myDatePicker";
    ViewBag.PersianDatePickerName = "MyDate";
    ViewBag.PersianDatePickerValue = Model.MyDate;
    ViewBag.PersianDatePickerLabel = "تاریخ";
    ViewBag.PersianDatePickerPlaceholder = "تاریخ را انتخاب کنید";
    ViewBag.PersianDatePickerHelpText = "راهنمای انتخاب تاریخ";
    ViewBag.PersianDatePickerRequired = true;
    ViewBag.PersianDatePickerCssClass = "form-control custom-class";
    ViewBag.PersianDatePickerValidationMessage = Html.ValidationMessageFor(m => m.MyDate).ToString();
}
@Html.Partial("_PersianDatePicker")
```

### پارامترهای ViewBag:

| پارامتر | نوع | توضیحات | پیش‌فرض |
|---------|-----|---------|---------|
| `PersianDatePickerId` | string | ID عنصر input | "persianDatePicker" |
| `PersianDatePickerName` | string | نام فیلد | "PersianDate" |
| `PersianDatePickerValue` | DateTime? | مقدار تاریخ (میلادی) | null |
| `PersianDatePickerLabel` | string | برچسب فیلد | "تاریخ" |
| `PersianDatePickerPlaceholder` | string | متن placeholder | "تاریخ را انتخاب کنید" |
| `PersianDatePickerHelpText` | string | متن راهنما | null |
| `PersianDatePickerRequired` | bool | الزامی بودن | false |
| `PersianDatePickerCssClass` | string | کلاس CSS اضافی | "form-control" |
| `PersianDatePickerValidationMessage` | string | پیام validation | null |

---

## 🔧 مدیریت در Controller

### تبدیل خودکار تاریخ شمسی به میلادی:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(MyModel model)
{
    // تبدیل تاریخ‌های شمسی به میلادی از hidden inputs
    var startDateHidden = Request.Form["StartDate_Hidden"];
    if (!string.IsNullOrEmpty(startDateHidden))
    {
        if (DateTime.TryParse(startDateHidden, out DateTime startDate))
        {
            model.StartDate = startDate;
        }
        else
        {
            // اگر parse نشد، از PersianDateHelper استفاده کن
            var startDatePersian = Request.Form["StartDate"];
            if (!string.IsNullOrEmpty(startDatePersian))
            {
                model.StartDate = Helpers.PersianDateHelper.ParsePersianDate(startDatePersian);
            }
        }
    }

    // حذف خطاهای validation برای تاریخ‌ها
    ModelState.Remove("StartDate");
    ModelState.Remove("EndDate");

    if (!ModelState.IsValid)
    {
        return View(model);
    }

    // ادامه عملیات...
}
```

---

## 💡 مثال‌های عملی

### مثال 1: فرم ساده با یک تاریخ

```csharp
@model AnnouncementCreateEditViewModel

@using (Html.BeginForm())
{
    <div class="form-group">
        @{
            ViewBag.PersianDatePickerId = "startDatePicker";
            ViewBag.PersianDatePickerName = "StartDate";
            ViewBag.PersianDatePickerValue = Model.StartDate;
            ViewBag.PersianDatePickerLabel = "تاریخ شروع";
            ViewBag.PersianDatePickerPlaceholder = "تاریخ شروع (اختیاری)";
            ViewBag.PersianDatePickerHelpText = "اگر خالی باشد، اطلاعیه از همین الان فعال می‌شود";
            ViewBag.PersianDatePickerRequired = false;
            ViewBag.PersianDatePickerValidationMessage = Html.ValidationMessageFor(m => m.StartDate, "", new { @class = "text-danger" }).ToString();
        }
        @Html.Partial("_PersianDatePicker")
    </div>
    
    <button type="submit" class="btn btn-primary">ذخیره</button>
}

@section Scripts {
    @Html.Partial("_PersianDatePickerScript")
}
```

### مثال 2: فرم با دو تاریخ (شروع و پایان)

```csharp
@model AnnouncementCreateEditViewModel

@using (Html.BeginForm())
{
    <div class="row">
        <div class="col-md-6">
            @{
                ViewBag.PersianDatePickerId = "startDatePicker";
                ViewBag.PersianDatePickerName = "StartDate";
                ViewBag.PersianDatePickerValue = Model.StartDate;
                ViewBag.PersianDatePickerLabel = "تاریخ شروع";
                ViewBag.PersianDatePickerPlaceholder = "تاریخ شروع (اختیاری)";
                ViewBag.PersianDatePickerHelpText = "اگر خالی باشد، اطلاعیه از همین الان فعال می‌شود";
                ViewBag.PersianDatePickerRequired = false;
                ViewBag.PersianDatePickerValidationMessage = Html.ValidationMessageFor(m => m.StartDate, "", new { @class = "text-danger" }).ToString();
            }
            @Html.Partial("_PersianDatePicker")
        </div>
        <div class="col-md-6">
            @{
                ViewBag.PersianDatePickerId = "endDatePicker";
                ViewBag.PersianDatePickerName = "EndDate";
                ViewBag.PersianDatePickerValue = Model.EndDate;
                ViewBag.PersianDatePickerLabel = "تاریخ پایان";
                ViewBag.PersianDatePickerPlaceholder = "تاریخ پایان (اختیاری)";
                ViewBag.PersianDatePickerHelpText = "اگر خالی باشد، اطلاعیه تا زمان غیرفعال شدن دستی فعال می‌ماند";
                ViewBag.PersianDatePickerRequired = false;
                ViewBag.PersianDatePickerValidationMessage = Html.ValidationMessageFor(m => m.EndDate, "", new { @class = "text-danger" }).ToString();
            }
            @Html.Partial("_PersianDatePicker")
        </div>
    </div>
    
    <button type="submit" class="btn btn-primary">ذخیره</button>
}

@section Scripts {
    @Html.Partial("_PersianDatePickerScript")
}
```

---

## 🔍 عیب‌یابی

### مشکل 1: تاریخ ذخیره نمی‌شود

**علت:** تاریخ به میلادی تبدیل نشده است.

**راه‌حل:** مطمئن شوید که در Controller تاریخ‌ها را تبدیل می‌کنید:

```csharp
var startDateHidden = Request.Form["StartDate_Hidden"];
if (!string.IsNullOrEmpty(startDateHidden))
{
    if (DateTime.TryParse(startDateHidden, out DateTime startDate))
    {
        model.StartDate = startDate;
    }
}
```

### مشکل 2: Validation خطا می‌دهد

**علت:** ModelState هنوز خطاهای تاریخ را دارد.

**راه‌حل:** قبل از بررسی ModelState، خطاهای تاریخ را حذف کنید:

```csharp
ModelState.Remove("StartDate");
ModelState.Remove("EndDate");
```

### مشکل 3: DatePicker لود نمی‌شود

**علت:** Script لود نشده است.

**راه‌حل:** مطمئن شوید که `_PersianDatePickerScript` را در `@section Scripts` اضافه کرده‌اید.

---

## ✅ بهترین روش‌ها

1. **همیشه از Component استفاده کنید** - کد تکراری ننویسید
2. **در Controller تاریخ‌ها را تبدیل کنید** - قبل از ذخیره در دیتابیس
3. **Validation را مدیریت کنید** - خطاهای تاریخ را از ModelState حذف کنید
4. **از Helper استفاده کنید** - برای تبدیل تاریخ‌ها از `PersianDateHelper.ParsePersianDate` استفاده کنید

---

## 📚 منابع بیشتر

- `Helpers/PersianDateHelper.cs` - متدهای تبدیل تاریخ
- `Areas/Admin/Views/Shared/_PersianDatePicker.cshtml` - Component اصلی
- `Areas/Admin/Views/Shared/_PersianDatePickerScript.cshtml` - Script های لازم

---

**آخرین به‌روزرسانی:** 1404/09/12

