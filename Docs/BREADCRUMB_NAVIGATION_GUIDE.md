# 📚 راهنمای کامل Breadcrumb Navigation

**نسخه:** 1.0.0  
**تاریخ:** 1404/10/05  
**وضعیت:** ✅ **فعال و آماده استفاده**

---

## 🎯 **هدف و کاربرد**

سیستم **Breadcrumb Navigation** (ناوبری نان‌واره‌ای) برای سیستم‌های درمانی با ساختارهای پیچیده طراحی شده است.

### **چرا Breadcrumb در سیستم‌های درمانی حیاتی است؟**

1. ✅ **ساختارهای پیچیده** - بیمارستان‌ها دارای زیرشاخه‌های زیادی هستند
2. ✅ **بازگشت سریع** - کاربران باید به راحتی به صفحات قبلی برگردند
3. ✅ **آگاهی از موقعیت** - کاربر همیشه می‌داند کجاست
4. ✅ **کاهش خطا** - جلوگیری از گم شدن در سیستم
5. ✅ **افزایش بهره‌وری** - دسترسی سریع‌تر به بخش‌های مختلف

---

## 📂 **فایل‌های مرتبط**

| فایل | مسیر | توضیحات |
|------|------|---------|
| **ViewModel** | `ViewModels/Shared/BreadcrumbItem.cs` | مدل داده Breadcrumb |
| **Partial View** | `Views/Shared/_BreadcrumbReception.cshtml` | View برای نمایش Breadcrumb |
| **CSS** | `Content/css/breadcrumb-medical.css` | استایل‌های مخصوص |
| **Layout** | `Views/Shared/_ReceptionLayout.cshtml` | Layout اصلی (Breadcrumb ادغام شده) |

---

## 🚀 **نحوه استفاده (3 مرحله ساده)**

### **مرحله 1: اضافه کردن Using ها**

در بالای فایل View خود:

```csharp
@using System.Collections.Generic
@using ClinicApp.ViewModels.Shared
```

### **مرحله 2: تنظیم ViewBag.Breadcrumbs**

در بخش `@{ ... }` فایل View:

```csharp
@{
    ViewBag.Title = "عنوان صفحه";
    Layout = "~/Views/Shared/_ReceptionLayout.cshtml";
    
    // تنظیم Breadcrumb
    ViewBag.Breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem 
        { 
            Title = "پذیرش", 
            Url = Url.Action("Index", "ReceptionV2"), 
            Icon = "fas fa-user-plus", 
            Tooltip = "بازگشت به صفحه اصلی پذیرش" 
        },
        new BreadcrumbItem 
        { 
            Title = "مدیریت POS", 
            Url = Url.Action("Index", "PosManagement"), 
            Icon = "fas fa-credit-card", 
            Tooltip = "لیست ترمینال‌های POS" 
        },
        new BreadcrumbItem 
        { 
            Title = "تست دستگاه POS", 
            Icon = "fas fa-vial", 
            IsActive = true  // صفحه فعلی
        }
    };
}
```

### **مرحله 3: استفاده از Layout صحیح**

مطمئن شوید که از `_ReceptionLayout.cshtml` استفاده می‌کنید:

```csharp
Layout = "~/Views/Shared/_ReceptionLayout.cshtml";
```

---

## 📊 **ساختار BreadcrumbItem**

| Property | نوع | الزامی | توضیحات |
|----------|-----|--------|---------|
| `Title` | `string` | ✅ بله | عنوان نمایشی (مثلاً "مدیریت POS") |
| `Url` | `string` | ❌ خیر | لینک برای کلیک (اگر نباشد، کلیک‌پذیر نیست) |
| `Icon` | `string` | ❌ خیر | آیکون FontAwesome (مثلاً "fas fa-user") |
| `IsActive` | `bool` | ❌ خیر | آیا صفحه فعلی است؟ (پیش‌فرض: false) |
| `Tooltip` | `string` | ❌ خیر | توضیحات اضافی برای نمایش در hover |

---

## 🎨 **آیکون‌های پیشنهادی (FontAwesome)**

| بخش | آیکون | کد |
|------|-------|-----|
| **پذیرش** | 👤➕ | `fas fa-user-plus` |
| **مدیریت POS** | 💳 | `fas fa-credit-card` |
| **تست POS** | 🧪 | `fas fa-vial` |
| **تنظیمات** | ⚙️ | `fas fa-cog` |
| **گزارشات** | 📊 | `fas fa-chart-bar` |
| **بیماران** | 🏥 | `fas fa-hospital-user` |
| **پزشکان** | 👨‍⚕️ | `fas fa-user-md` |
| **خدمات** | 🔧 | `fas fa-tools` |
| **اطلاعات** | ℹ️ | `fas fa-info-circle` |
| **ویرایش** | ✏️ | `fas fa-edit` |

---

## 📝 **مثال‌های کاربردی**

### **مثال 1: مسیر ساده (2 سطح)**

```csharp
ViewBag.Breadcrumbs = new List<BreadcrumbItem>
{
    new BreadcrumbItem { Title = "پذیرش", Url = Url.Action("Index", "ReceptionV2"), Icon = "fas fa-home" },
    new BreadcrumbItem { Title = "لیست بیماران", Icon = "fas fa-users", IsActive = true }
};
```

**نمایش:**
```
پذیرش > لیست بیماران
```

---

### **مثال 2: مسیر پیچیده (4 سطح)**

```csharp
ViewBag.Breadcrumbs = new List<BreadcrumbItem>
{
    new BreadcrumbItem { Title = "پذیرش", Url = Url.Action("Index", "ReceptionV2"), Icon = "fas fa-user-plus" },
    new BreadcrumbItem { Title = "مدیریت POS", Url = Url.Action("Index", "PosManagement"), Icon = "fas fa-credit-card" },
    new BreadcrumbItem { Title = "جزئیات ترمینال", Url = Url.Action("TerminalDetails", "PosManagement", new { id = 1 }), Icon = "fas fa-info-circle" },
    new BreadcrumbItem { Title = "تست اتصال", Icon = "fas fa-plug", IsActive = true }
};
```

**نمایش:**
```
پذیرش > مدیریت POS > جزئیات ترمینال > تست اتصال
```

---

### **مثال 3: بدون آیکون (ساده)**

```csharp
ViewBag.Breadcrumbs = new List<BreadcrumbItem>
{
    new BreadcrumbItem { Title = "داشبورد", Url = "/" },
    new BreadcrumbItem { Title = "تنظیمات", Url = "/Settings" },
    new BreadcrumbItem { Title = "کاربران", IsActive = true }
};
```

---

## 🎨 **ظاهر و استایل**

### **رنگ‌بندی (طبق استانداردهای پزشکی):**

- **لینک‌ها:** آبی (`--medical-primary`)
- **صفحه فعلی:** مشکی (`--medical-text-primary`)
- **Hover:** پس‌زمینه آبی روشن
- **Separator:** خاکستری روشن (chevron-left)

### **Responsive:**

- **Desktop:** نمایش کامل با آیکون‌ها
- **Tablet:** نمایش کامل
- **Mobile:** بدون آیکون‌ها (صرفه‌جویی در فضا)

---

## ♿ **Accessibility (دسترس‌پذیری)**

✅ **WCAG 2.1 سازگار:**
- استفاده از `<nav>` با `aria-label`
- استفاده از `<ol>` برای ترتیب سلسله‌مراتبی
- استفاده از `aria-current="page"` برای صفحه فعلی
- پشتیبانی کامل از keyboard navigation
- Focus indicators واضح

---

## 🔧 **سفارشی‌سازی CSS**

اگر نیاز به تغییر استایل دارید، فایل زیر را ویرایش کنید:

**فایل:** `Content/css/breadcrumb-medical.css`

```css
/* تغییر رنگ لینک‌ها */
.breadcrumb-link {
    color: #your-color;
}

/* تغییر فونت */
.breadcrumb-reception {
    font-family: 'Your-Font', sans-serif;
}
```

---

## ⚠️ **نکات مهم**

### **✅ انجام دهید:**
1. ✅ همیشه اولین آیتم را لینک صفحه اصلی قرار دهید
2. ✅ آخرین آیتم را `IsActive = true` کنید
3. ✅ از آیکون‌های مناسب استفاده کنید
4. ✅ Tooltip برای راهنمایی بیشتر اضافه کنید

### **❌ انجام ندهید:**
1. ❌ بیش از 5 سطح Breadcrumb نداشته باشید
2. ❌ عناوین خیلی طولانی استفاده نکنید (حداکثر 30 کاراکتر)
3. ❌ آیتم فعلی را کلیک‌پذیر نکنید
4. ❌ از رنگ‌های جیق و جلف استفاده نکنید

---

## 🐛 **عیب‌یابی (Troubleshooting)**

### **مشکل 1: Breadcrumb نمایش داده نمی‌شود**

**راه حل:**
1. مطمئن شوید `ViewBag.Breadcrumbs` تنظیم شده است
2. چک کنید که Layout صحیح است (`_ReceptionLayout.cshtml`)
3. مطمئن شوید `using`ها اضافه شده‌اند

### **مشکل 2: استایل‌ها اعمال نمی‌شوند**

**راه حل:**
1. Cache مرورگر را پاک کنید (`Ctrl + F5`)
2. مطمئن شوید `breadcrumb-medical.css` لود می‌شود
3. Developer Tools را باز کنید و Network tab را چک کنید

### **مشکل 3: آیکون‌ها نمایش داده نمی‌شوند**

**راه حل:**
1. مطمئن شوید FontAwesome لود شده است
2. کلاس آیکون را چک کنید (مثلاً `fas` نه `fa`)
3. کد آیکون صحیح است؟ (مثلاً `fa-user` نه `user`)

---

## 📚 **مثال کامل (Full Example)**

```csharp
@using System.Collections.Generic
@using ClinicApp.ViewModels.Shared

@{
    ViewBag.Title = "جزئیات بیمار";
    Layout = "~/Views/Shared/_ReceptionLayout.cshtml";
    
    ViewBag.Breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem 
        { 
            Title = "داشبورد", 
            Url = Url.Action("Index", "Dashboard"), 
            Icon = "fas fa-home",
            Tooltip = "صفحه اصلی"
        },
        new BreadcrumbItem 
        { 
            Title = "مدیریت بیماران", 
            Url = Url.Action("Index", "Patients"), 
            Icon = "fas fa-hospital-user",
            Tooltip = "لیست تمام بیماران"
        },
        new BreadcrumbItem 
        { 
            Title = "جزئیات بیمار: علی رضایی", 
            Icon = "fas fa-user",
            IsActive = true
        }
    };
}

<div class="container-fluid">
    <h1>جزئیات بیمار</h1>
    <!-- محتوای صفحه -->
</div>
```

---

## 📊 **آمار و گزارش**

- **تعداد فایل‌های ایجاد شده:** 4 فایل
- **سطوح پشتیبانی شده:** حداکثر 5 سطح
- **Responsive:** ✅ کاملاً responsive
- **Accessibility:** ✅ WCAG 2.1
- **RTL:** ✅ کاملاً پشتیبانی می‌شود
- **Browser Support:** تمام مرورگرهای مدرن

---

## 🔗 **منابع مرتبط**

- [قرارداد توسعه](DEVELOPMENT_CONTRACT.md)
- [راهنمای UI/UX](03-Development-Contract-Quick-Guide.md)
- [راهنمای Layout پذیرش](RECEPTION_LAYOUT_IMPLEMENTATION_REPORT.md)

---

**نسخه:** 1.0.0  
**آخرین به‌روزرسانی:** 1404/10/05  
**وضعیت:** ✅ **آماده استفاده در محیط Production**

---

🎉 **سیستم Breadcrumb Navigation شما آماده است!** 🚀

