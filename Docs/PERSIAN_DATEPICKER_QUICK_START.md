# ⚡ راهنمای سریع Persian DatePicker Component

## 🚀 استفاده در 3 مرحله

### مرحله 1: اضافه کردن Script

در `@section Scripts`:

```csharp
@section Scripts {
    @Html.Partial("_PersianDatePickerScript")
}
```

### مرحله 2: استفاده از Component

```csharp
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
```

### مرحله 3: مدیریت در Controller

```csharp
[HttpPost]
public ActionResult Create(MyModel model)
{
    // تبدیل تاریخ شمسی به میلادی
    var startDateHidden = Request.Form["StartDate_Hidden"];
    if (!string.IsNullOrEmpty(startDateHidden))
    {
        if (DateTime.TryParse(startDateHidden, out DateTime startDate))
        {
            model.StartDate = startDate;
        }
    }
    
    // حذف خطاهای validation
    ModelState.Remove("StartDate");
    
    // ادامه...
}
```

---

## 📋 مثال کامل

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
    </div>
    
    <button type="submit" class="btn btn-primary">ذخیره</button>
}

@section Scripts {
    @Html.Partial("_PersianDatePickerScript")
}
```

---

## ✅ نکات مهم

1. **فقط یک بار Script را اضافه کنید** - `_PersianDatePickerScript` را فقط یک بار در صفحه لود کنید
2. **در Controller تاریخ‌ها را تبدیل کنید** - از `Request.Form["FieldName_Hidden"]` استفاده کنید
3. **Validation را حذف کنید** - `ModelState.Remove("FieldName")` قبل از بررسی ModelState

---

## 📚 برای اطلاعات بیشتر

راهنمای کامل: [PERSIAN_DATEPICKER_COMPONENT_GUIDE.md](./PERSIAN_DATEPICKER_COMPONENT_GUIDE.md)

