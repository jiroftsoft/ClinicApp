# بهینه‌سازی CMS — بررسی و تغییرات اعمال‌شده

## مسیر ورود
- **URL:** `http://localhost:3560/Admin/CMS`
- **روت:** `Admin_CMS_Home` → کنترلر `CmsHome`, اکشن `Index`

---

## ۱. تغییرات اعمال‌شده

### ۱.۱ صفحهٔ ورود CMS (CmsHome)
- **کنترلر:** `Areas/Admin/Controllers/CMS/CmsHomeController.cs`
- **ویو:** `Areas/Admin/Views/CMS/CmsHome/Index.cshtml`
- با باز کردن `/Admin/CMS` به‌جای رفتن مستقیم به یک ماژول (مثل InsuranceInfo)، یک صفحهٔ ورود با **گرید کارت‌های ماژول‌ها** نمایش داده می‌شود.
- ماژول‌ها بر اساس **دسته‌بندی** (محتوای بصری، محتوای متنی، اطلاعات، تعاملات، تنظیمات) گروه‌بندی شده‌اند.
- هر کارت با کلیک به مسیر `/Admin/CMS/{Controller}/Index` (مثلاً `/Admin/CMS/InsuranceInfo`) می‌رود.

### ۱.۲ اصلاح لینک‌های منوی CMS (_CMSMenu)
- قبلاً از `Url.Action("Index", module.Controller, new { area = "Admin" })` استفاده می‌شد که مسیر `/Admin/ControllerName` (خارج از CMS) می‌ساخت.
- الان از **`Url.RouteUrl("Admin_CMS_Default", new { controller = module.Controller, action = "Index" })`** استفاده می‌شود تا لینک‌ها به صورت **`/Admin/CMS/ControllerName`** باشند.
- در نتیجه از داخل هر صفحهٔ CMS با کلیک روی آیتم منو به ماژول درست داخل CMS می‌روید.

### ۱.۳ روت Admin/CMS
- روت **Admin_CMS_Home** به‌روز شد تا به‌جای `InsuranceInfo`، کنترلر **`CmsHome`** را صدا بزند.
- در نتیجه `/Admin/CMS` فقط صفحهٔ ورود (گرید ماژول‌ها) را نشان می‌دهد.

---

## ۲. جریان کاربری

1. داشبورد مدیر → کلیک روی **CMS** در سایدبار → `/Admin/CMS`
2. نمایش **صفحهٔ ورود CMS** (گرید ماژول‌ها)
3. کلیک روی یک ماژول (مثلاً «اطلاعات بیمه») → `/Admin/CMS/InsuranceInfo`
4. در صفحات ماژول‌ها، منوی سایدبار (_CMSMenu) با لینک‌های صحیح `/Admin/CMS/...` برای جابه‌جایی بین ماژول‌ها در دسترس است.

---

## ۳. فایل‌های مرتبط

| فایل | نقش |
|------|-----|
| `Areas/Admin/Controllers/CMS/CmsHomeController.cs` | کنترلر صفحهٔ ورود CMS |
| `Areas/Admin/Views/CMS/CmsHome/Index.cshtml` | ویو گرید ماژول‌ها |
| `Areas/Admin/Views/Shared/_CMSMenu.cshtml` | منوی سایدبار با لینک‌های اصلاح‌شده |
| `Areas/Admin/AdminAreaRegistration.cs` | روت‌های Admin_CMS_Home و Admin_CMS_Default |

---

## ۴. پیشنهاد برای بهینه‌سازی‌های بعدی

- **یکپارچه‌سازی لیست ماژول‌ها:** انتقال آرایهٔ ماژول‌ها به یک کلاس/سرویس مشترک تا هم در CmsHome/Index و هم در _CMSMenu از یک منبع استفاده شود.
- **کش منو:** در صورت داینامیک شدن منو از دیتابیس، استفاده از کش کوتاه‌مدت برای کاهش بار.
- **دسترسی بر اساس نقش:** در صورت نیاز، فیلتر کردن ماژول‌های قابل نمایش در CmsHome و _CMSMenu بر اساس نقش کاربر.
