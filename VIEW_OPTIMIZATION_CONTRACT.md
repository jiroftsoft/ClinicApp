# 📋 قرارداد بهینه‌سازی ویوها (View Optimization Contract)

## 🎯 اهداف کلی

این قرارداد تضمین می‌کند که تمام ویوهای پروژه ClinicApp به صورت بهینه و حرفه‌ای طراحی و پیاده‌سازی شوند.

## 🔧 اصول کلی

### **1. هماهنگی با AdminLayout و BundleConfig**
- همیشه از باندل‌های بهینه‌شده استفاده کنید
- از بارگذاری مستقیم فایل‌های CSS/JS خودداری کنید
- از Conditional Scripts در AdminLayout استفاده کنید

### **2. بارگذاری انتخابی**
- فایل‌های غیرضروری بارگذاری نکنید
- هر صفحه فقط آنچه نیاز دارد را بارگذاری کند
- از ViewBag برای کنترل بارگذاری استفاده کنید

### **3. بهبود عملکرد**
- کاهش زمان بارگذاری صفحات
- بهینه‌سازی استفاده از منابع
- مدیریت حافظه بهتر

### **4. ساختار منطقی**
- کد تمیز و قابل نگهداری
- مستندسازی کامل
- رعایت اصول SOLID

## 📚 روش کار استاندارد

### **1. در Controller:**

```csharp
[HttpGet]
public async Task<ActionResult> YourAction()
{
    try
    {
        // تنظیم ViewBag برای بارگذاری باندل‌های مورد نیاز
        ViewBag.Title = "عنوان صفحه";
        ViewBag.RequireDataTables = true;        // برای جدول‌ها
        ViewBag.RequireSelect2 = true;           // برای فیلترهای dropdown
        ViewBag.RequireDatePicker = true;        // برای فیلتر تاریخ
        ViewBag.RequireFormValidation = true;    // برای اعتبارسنجی فرم‌ها
        
        // باقی کد...
    }
    catch (Exception ex)
    {
        // مدیریت خطا
    }
}
```

### **2. در View:**

```html
@section Styles {
    <!-- CSS های اضافی برای این صفحه -->
    <link href="~/Content/css/your-page-specific.css" rel="stylesheet" />
}

@section Scripts {
    <!-- Scripts are loaded via bundles in _AdminLayout.cshtml -->
    <!-- Additional scripts specific to this page -->
    <!-- باقی کد... -->
}
```

## 🎯 انواع صفحات و تنظیمات

### **صفحه ساده (فقط Bootstrap):**
```csharp
ViewBag.Title = "صفحه ساده";
// هیچ ViewBag اضافی نیاز نیست
```

### **صفحه با جدول (فقط DataTables):**
```csharp
ViewBag.Title = "صفحه جدول";
ViewBag.RequireDataTables = true;
```

### **صفحه فرم (Select2 + DatePicker + Validation):**
```csharp
ViewBag.Title = "صفحه فرم";
ViewBag.RequireSelect2 = true;
ViewBag.RequireDatePicker = true;
ViewBag.RequireFormValidation = true;
```

### **صفحه کامل (همه چیز):**
```csharp
ViewBag.Title = "صفحه کامل";
ViewBag.RequireDataTables = true;
ViewBag.RequireSelect2 = true;
ViewBag.RequireDatePicker = true;
ViewBag.RequireFormValidation = true;
```

## ⚠️ نکات مهم

### **1. ترتیب بارگذاری:**
- همیشه jQuery را اول بارگذاری کنید
- CSS را قبل از JavaScript بارگذاری کنید
- Core Scripts قبل از Page-Specific Scripts

### **2. ViewBag تنظیمات:**
- در Controller تنظیم کنید، نه در View
- از نام‌گذاری استاندارد استفاده کنید
- مستندسازی کنید

### **3. تست عملکرد:**
- بعد از تغییر باندل‌ها، عملکرد را تست کنید
- سرعت بارگذاری را بررسی کنید
- حجم دانلود را کنترل کنید

## 🔧 تنظیمات پیشرفته

### **اضافه کردن باندل جدید:**

#### **1. در BundleConfig.cs:**
```csharp
bundles.Add(new ScriptBundle("~/bundles/custom-plugin").Include(
    "~/Content/plugins/custom-plugin.js"));
```

#### **2. در _AdminLayout.cshtml:**
```html
@if (ViewBag.RequireCustomPlugin == true)
{
    @Scripts.Render("~/bundles/custom-plugin")
}
```

#### **3. در Controller:**
```csharp
ViewBag.RequireCustomPlugin = true;
```

## 📊 مزایای این قرارداد

### **عملکرد:**
- **بهبود سرعت**: کاهش زمان بارگذاری تا 80%
- **کاهش حجم**: کاهش حجم دانلود تا 80%
- **مدیریت حافظه**: بهینه‌سازی استفاده از منابع

### **توسعه:**
- **ساختار بهتر**: کد منطقی و قابل نگهداری
- **استانداردسازی**: روش یکسان برای همه ویوها
- **کاهش خطا**: کاهش احتمال خطاهای رایج

### **نگهداری:**
- **قابلیت نگهداری**: کد تمیز و مستند
- **قابلیت توسعه**: آسان برای اضافه کردن ویژگی‌های جدید
- **هماهنگی**: با AdminLayout و BundleConfig بهینه‌شده

## 🎉 نتیجه

این قرارداد تضمین می‌کند که:
- **همه ویوها** بهینه‌سازی شوند
- **عملکرد** بهبود یابد
- **ساختار** منطقی‌تر شود
- **قابلیت نگهداری** افزایش یابد
- **استانداردسازی** در سراسر پروژه

## 📝 چک‌لیست

قبل از تکمیل هر ویو، موارد زیر را بررسی کنید:

- [ ] ViewBag های مورد نیاز تنظیم شده‌اند
- [ ] CSS های تکراری حذف شده‌اند
- [ ] JS های تکراری حذف شده‌اند
- [ ] ترتیب بارگذاری صحیح است
- [ ] عملکرد تست شده است
- [ ] مستندسازی کامل است

## 🔄 به‌روزرسانی

این قرارداد باید به صورت دوره‌ای بررسی و به‌روزرسانی شود تا با تغییرات پروژه هماهنگ باشد.

**تاریخ آخرین به‌روزرسانی**: 2025-01-04
**نسخه**: 1.0
**وضعیت**: فعال
