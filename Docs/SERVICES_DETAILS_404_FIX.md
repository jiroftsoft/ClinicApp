# رفع خطای 404 برای /Services/Details/{id}

## مشکل

درخواست به آدرس **`/Services/Details/2449`** با خطای **HTTP 404 - The resource cannot be found** مواجه می‌شد.

- لینک از **بخش خدمات صفحه اصلی** (HomePageService) با الگوی `DetailsUrl = "/Services/Details/{s.ServiceId}"` ساخته می‌شد.
- در سطح برنامه **کنترلری** برای مسیر `Services/Details` وجود نداشت.
- صفحه جزئیات واقعی خدمات در **MedicalServiceInfoController** با اکشن `Details(string slug)` و مسیر **/MedicalServiceInfo/Details** (با پارامتر slug در query) تعریف شده است.

## راه‌حل

کنترلر **ServicesController** در فضای اصلی برنامه اضافه شد تا درخواست‌های `/Services/Details/{id}` را بر اساس **ServiceId** به صفحه صحیح جزئیات (با slug) هدایت کند.

---

## تغییرات

### ۱. کنترلر جدید: `Controllers/ServicesController.cs`

- **اکشن:** `Details(int id)`
- **مسیر درخواستی:** `/Services/Details/2449` (یا هر مقدار عددی به‌جای 2449)
- **منطق:**
  1. فراخوانی `IMedicalServiceInfoService.GetByServiceIdAsync(id)`
  2. در صورت موفقیت و وجود **Slug** برای آن خدمت → **Redirect** به  
     `MedicalServiceInfo/Details?slug={slug}`
  3. در صورت نبودن رکورد یا خطا → **Redirect** به  
     `MedicalServiceInfo/Index` با پیام TempData (هشدار یا خطا)
- **وابستگی:** فقط `IMedicalServiceInfoService` (قبلاً در Unity ثبت شده است).

### ۲. مسیر (Route)

- از **مسیر پیش‌فرض** `{controller}/{action}/{id}` استفاده می‌شود؛ نیازی به تعریف مسیر جداگانه برای `/Services/Details/{id}` نیست.

### ۳. منبع لینک

- **HomePageService.GetServicesSectionAsync()** همچنان لینک را به صورت  
  `DetailsUrl = "/Services/Details/{s.ServiceId}"`  
  تولید می‌کند و نیازی به تغییر در سرویس یا ویوها نبود.

---

## جریان درخواست

1. کاربر روی کارت خدمت در صفحه اصلی کلیک می‌کند → `/Services/Details/2449`
2. **ServicesController.Details(2449)** اجرا می‌شود.
3. با **ServiceId = 2449** از طریق `GetByServiceIdAsync` رکورد **MedicalServiceInfo** (و در صورت وجود، Slug) گرفته می‌شود.
4. در صورت وجود Slug → ریدایرکت به `/MedicalServiceInfo/Details?slug=...` و نمایش صفحه جزئیات.
5. در غیر این صورت → ریدایرکت به لیست خدمات با پیام مناسب در TempData.

---

## نکات

- اگر برای یک **ServiceId** رکورد **MedicalServiceInfo** (با Slug) در دیتابیس تعریف نشده باشد، به جای 404، کاربر به صفحه لیست خدمات هدایت می‌شود و پیام «صفحه جزئیات این خدمت در حال حاضر موجود نیست» (یا مشابه) نمایش داده می‌شود.
- برای رفع 404 در آینده، اطمینان حاصل کنید برای خدمت‌های نمایش‌داده‌شده در بخش خدمات صفحه اصلی، رکورد متناظر در **MedicalServiceInfo** با **Slug** معتبر وجود داشته باشد.

تاریخ: بهمن ۱۴۰۴
