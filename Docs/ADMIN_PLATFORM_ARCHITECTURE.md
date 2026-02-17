# معماری پلتفرم ادمین و داشبورد مدیریت کلینیک (Admin Platform Architecture)

## سند طراحی فوق‌حرفه‌ای — لایوت اختصاصی مدیر و داشبورد توسعه‌پذیر

**نسخه:** 1.0  
**وضعیت:** طراحی و نقشهٔ راه — بدون شکستن سیستم فعلی  
**سازگاری:** ASP.NET MVC5, .NET 4.8, Clean Architecture, SOLID

---

# بخش ۱: تحلیل عمیق ساختار فعلی CMS

## ۱.۱ Routing و Area

```
Admin Area
├── Admin_default          → "Admin/{controller}/{action}/{id}"  → Namespace: Areas.Admin.Controllers
├── Admin_CMS_Default      → "Admin/CMS/{controller}/{action}/{id}" → Namespace: Areas.Admin.Controllers.CMS
├── Admin_DoctorDashboard  → "Admin/DoctorDashboard/{action}/{id}"
└── ... (سایر روت‌های خاص)
```

- **CMS URL:** `http://localhost:3560/Admin/CMS/InsuranceInfo` → Controller: `InsuranceInfoController` در `ClinicApp.Areas.Admin.Controllers.CMS`.
- **نکته:** در `_CMSMenu.cshtml` لینک ماژول‌ها با `Url.Action("Index", module.Controller, new { area = "Admin" })` ساخته می‌شود که مسیر `Admin/InsuranceInfo/...` می‌دهد؛ برای مطابقت با روت CMS باید مسیر `Admin/CMS/InsuranceInfo` تولید شود (مثلاً با Route name یا helper اختصاصی).

## ۱.۲ کنترلرهای CMS

| کنترلر | Namespace | Base | نقش |
|--------|-----------|------|-----|
| BaseCMSController | Areas.Admin.Controllers.CMS | Controller | متد کمکی GetViewPath برای مسیر View |
| InsuranceInfoController | همان | BaseCMSController | اطلاعات بیمه |
| Slider, Gallery, Video, Story, BlogPost, HealthTip, Announcement, FAQ, AboutPage, Testimonial, ContactForm | همان | BaseCMSController | محتوا و تعاملات |
| NewsletterTemplate, NewsletterCampaign, NewsletterSubscription | همان | BaseCMSController | خبرنامه |
| MedicalServiceInfo, MedicalEquipment, EmergencyContact, ClinicWorkingHours, Footer | همان | BaseCMSController | اطلاعات و تنظیمات |

همهٔ اکشن‌های CMS از طریق روت `Admin/CMS/{controller}/{action}/{id}` در دسترس هستند.

## ۱.۳ View و Layout فعلی

- **Layout مشترک همهٔ Admin (از جمله CMS):** `~/Areas/Admin/Views/Shared/_AdminLayout.cshtml`
- **ساختار _AdminLayout:**
  - Header: لوگو، جستجو، theme toggle، منوی موبایل، کاربر + خروج
  - Sidebar: دسترسی سریع (Home, Reception, Clinic, Doctor, Service, Insurance) + **Admin Modules Menu** (_AdminModulesMenu.cshtml) + **CMS Menu** (_CMSMenu.cshtml) + عملیات سریع + اطلاعات سیستم (زمان/تاریخ)
  - Main: Breadcrumb (_Breadcrumb.cshtml) + `@RenderBody()`
  - اعلان‌ها: `data-notifications`
  - اسکریپت‌ها: jQuery, Bootstrap, Validation, Toastr, SweetAlert2, admin-layout.js, medical-toast, JalaliDatePicker و غیره

## ۱.۴ وابستگی‌ها و دارایی‌ها

- **CSS:** `~/Content/admin` (admin-layout.css, notifications.css), `~/Content/css`, `~/Content/plugins/css`, notification-system.css, local-fonts.css
- **JS:** bundles (jquery, jqueryval, bootstrap, plugins), Chart.js, Toastr, SweetAlert2, admin-notification-service, admin-layout.js, medical-toast, JalaliDatePicker
- **فونت:** Vazir, Shabnam, Yekan (local-fonts.css), FontAwesome
- **تم:** `data-theme="light"` در `<html>`؛ theme toggle در هدر

## ۱.۵ خلاصهٔ نقاط قوت و بهبود

| جنبه | وضعیت فعلی | هدف طراحی جدید |
|------|------------|-----------------|
| Layout | یک لایوت برای کل Admin + CMS | لایوت اختصاصی «مدیر» با تمرکز داشبورد و CMS، قابل تمایز از بقیه |
| Sidebar | ثابت از آرایه در View | قابل توسعه، آمادهٔ منوی داینامیک از DB (آینده) |
| داشبورد مرکزی | DoctorDashboard جدا؛ بدون داشبورد واحد مدیریتی | داشبورد پزشکی یکپارچه با ویجت‌ها و داده‌های آماده |
| نقش و دسترسی | بدون فیلتر نقش در منو | Role-based menu visibility و Permission-based در لایوت جدید |
| تم | Light با یک toggle | تم Light / Medical Blue و سیستم تم یکپارچه |

---

# بخش ۲: معماری لایوت ادمین (Admin Layout Architecture)

## ۲.۱ معماری کلی (نمودار متنی)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  ADMIN PLATFORM LAYOUT (_AdminPlatformLayout.cshtml)                        │
├─────────────────────────────────────────────────────────────────────────────┤
│  HEADER (Top Bar)                                                            │
│  ├── Logo + App Name (link to Dashboard)                                     │
│  ├── Global Search (optional)                                                 │
│  ├── Notification Bell + Dropdown (placeholder)                               │
│  ├── User Profile Menu (Name, Role, Logout)                                  │
│  ├── Theme Toggle (Light / Medical Blue)                                      │
│  └── Mobile: Hamburger → overlay sidebar                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│  SIDEBAR (Collapsible, Role-aware)                                           │
│  ├── Dashboard (Home) → /Admin/CMS/Dashboard or /Admin/DoctorDashboard       │
│  ├── Section: مدیریت محتوا (CMS)                                             │
│  │   └── Dynamic/Cached menu (InsuranceInfo, Slider, ...)                    │
│  ├── Section: مدیریت کلینیک (Clinic, Department, ...)                       │
│  ├── Section: مالی / پذیرش / گزارشات (future)                                │
│  └── Footer: System time, Version (optional)                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│  MAIN CONTENT                                                                │
│  ├── Breadcrumb (from _Breadcrumb or dedicated _AdminBreadcrumb)             │
│  ├── Optional: Page Title + Quick Actions bar                                │
│  └── @RenderBody()                                                            │
├─────────────────────────────────────────────────────────────────────────────┤
│  FOOTER (Optional minimal bar) | Notifications Container                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

## ۲.۲ اجزای لایوت

| جزء | مسئولیت | پشتیبانی توسعه |
|-----|---------|-----------------|
| **Header** | هویت برند، جستجو، اعلان‌ها، پروفایل کاربر، تم | اسلات برای ویجت‌های بعدی (مثلاً جستجوی سراسری) |
| **Sidebar** | ناوبری بر اساس نقش، گروه‌بندی ماژول‌ها | منوی داینامیک از DB (جدول MenuItem)، کش سمت سرور |
| **Breadcrumb** | مسیر صفحه | استفاده از _Breadcrumb فعلی یا نسخهٔ اختصاصی با همان منطق |
| **Theme** | Light / Medical Blue | CSS variables در یک فایل تم؛ `data-theme` روی `<html>` |
| **Notification** | لیست اعلان‌ها (خالی/نمایشی) | بعداً اتصال به سرویس اعلان و SignalR/پولینگ |

## ۲.۳ Role-based منو

- **منبع نقش:** `User.IsInRole(AppRoles.Admin)`, `User.IsInRole(AppRoles.Receptionist)` و غیره (یا ICurrentUserService).
- **قوانین پیشنهادی:**
  - Admin: دسترسی به همهٔ بخش‌ها از جمله CMS، داشبورد، تنظیمات.
  - Receptionist: داشبورد، پذیرش، گزارشات صندوق؛ محدودیت در CMS یا تنظیمات حساس (طبق نیاز کسب‌وکار).
- **پیاده‌سازی:** در Partial منوی سایدبار (مثلاً `_AdminPlatformSidebar.cshtml`) با چک نقش در Razor یا از طریق یک Helper/ViewModel که لیست آیتم‌های مجاز را برمی‌گرداند.

## ۲.۴ سیستم تم (Theme)

- **Light:** پس‌زمینه روشن، متن تیره (هماهنگ با وضعیت فعلی).
- **Medical Blue:** رنگ اصلی آبی پزشکی (`--medical-primary`), کنتراست مناسب برای محیط درمانی.
- **ساختار پیشنهادی:** یک فایل CSS (مثلاً `admin-platform-themes.css`) با دو بلوک `[data-theme="light"]` و `[data-theme="medical-blue"]` که متغیرهای CSS (رنگ‌ها، سایه‌ها) را override می‌کنند؛ toggle در هدر همان `data-theme` را عوض کند و در localStorage ذخیره شود.

---

# بخش ۳: معماری داشبورد (Dashboard Architecture)

## ۳.۱ داشبورد به‌عنوان مرکز مدیریت

- **صفحهٔ واحد:** یک اکشن اختصاصی برای «داشبورد مدیریت» (مثلاً `Admin/CMS/Dashboard` یا `Admin/Dashboard/Index`) که فقط برای نقش‌های مجاز (Admin, Receptionist) قابل دسترسی است.
- **محتوای صفحه:** گرید ویجت‌ها (widget grid) با سکشن‌های قابل توسعه.

## ۳.۲ ویجت‌های پیشنهادی (آماده برای اتصال داده)

| ویجت | توضیح | دادهٔ مورد انتظار (آمادهٔ اتصال) |
|------|--------|-----------------------------------|
| Today Appointments Summary | خلاصه نوبت‌های امروز | Count, لیست کوتاه (از سرویس نوبت) |
| Patient Statistics | آمار بیماران (امروز/هفته/ماه) | Counts از سرویس بیمار |
| Financial Overview | خلاصه مالی (درآمد روز/هفته) | از سرویس مالی/پذیرش |
| Reception Queue | صف پذیرش (فعلاً نمایشی) | از سرویس پذیرش |
| Doctor Status | وضعیت پزشکان (فعال/غیرفعال یا امروز) | از DoctorDashboard/سرویس پزشک |
| Notifications | آخرین اعلان‌ها | از سرویس اعلان |
| Quick Actions | دکمه‌های عملیات سریع | لینک‌های ثابت |
| System Health | وضعیت سرویس (اختیاری) | از یک HealthCheck ساده |

## ۳.۳ ویجت‌ها: طراحی توسعه‌پذیر

- هر ویجت یک **Partial View** جدا (مثلاً `_WidgetTodayAppointments.cshtml`) با یک **ViewModel** اختیاری.
- کنترلر داشبورد می‌تواند یک **DashboardViewModel** واحد داشته باشد که برای هر ویجت یک property (یا یک Dictionary) داشته باشد؛ در فاز اول مقدار null یا دادهٔ نمایشی.
- **Componentization:** یک پوشهٔ `Views/Shared/DashboardWidgets` برای تمام ویجت‌ها؛ فراخوانی با `@Html.Partial("_WidgetX", Model.WidgetX)`.
- **Lazy loading (آینده):** ویجت‌ها می‌توانند با یک `data-widget-url` و AJAX بعد از لود صفحه بارگذاری شوند تا زمان اولین رندر کم شود.

## ۳.۴ گرید و ریسپانسیو

- استفاده از سیستم گرید Bootstrap (مثلاً `row` + `col-lg-3 col-md-4 col-sm-6`) برای کارت ویجت‌ها.
- هر کارت: عنوان، آیکون، محتوا، لینک «مشاهدهٔ بیشتر» (در صورت نیاز).

---

# بخش ۴: معماری فنی (Technical Architecture)

## ۴.۱ ViewModels

- **AdminPlatformLayoutViewModel (اختیاری):** برای لایوت؛ شامل: نام کاربر، نقش‌ها، لیست آیتم‌های منو (در صورت پرمقدار کردن از سرویس)، تعداد اعلان خوانده‌نشده.
- **DashboardPageViewModel:** برای صفحهٔ داشبورد؛ شامل: لیست ویجت‌ها یا یک مدل واحد با زیرمدل‌ها (TodayAppointments, PatientStats, FinancialOverview, …).
- **Widget ViewModels:** هر ویجت در صورت نیاز یک ViewModel کوچک (مثلاً `WidgetTodayAppointmentsViewModel` با Count و List).

## ۴.۲ Partial Views و استراتژی Section

- **Layout:** از `@RenderSection("Styles", required: false)` و `@RenderSection("Scripts", required: false)` برای صفحه‌های خاص استفاده شود (همانند فعلی).
- **Optional Section برای داشبورد:** مثلاً `@RenderSection("DashboardScripts", required: false)` برای اسکریپت‌های مخصوص داشبورد (مثلاً Chart.js فقط در داشبورد).
- **کامپوننت‌های قابل استفاده مجدد:** Sidebar، Header، Breadcrumb، هر ویجت به‌صورت Partial؛ بدون وابستگی به مدل سنگین در لایوت (مدل لایوت حداقلی یا ViewBag).

## ۴.۳ استراتژی اسکریپت و CSS

- **CSS:**
  - یک bundle جدید برای لایوت پلتفرم ادمین: مثلاً `~/Content/css/admin-platform.css` (شامل ساختار لایوت، سایدبار، هدر، تم).
  - فایل تم: `admin-platform-themes.css`.
  - صفحات/ویجت‌های خاص در `@section Styles { }` خود صفحه.
- **JS:**
  - `admin-platform-layout.js`: رفتار سایدبار (collapse/expand)، تم، منوی موبایل، نوتیفیکیشن در هدر.
  - داشبورد: در صورت نیاز `admin-dashboard.js` برای رفرش ویجت یا AJAX (فاز بعد).
- **مدیریت Bundle:** اضافه کردن به `BundleConfig` بدون حذف باندل‌های فعلی Admin.

## ۴.۴ ساختار پوشه‌ها (پیشنهادی)

```
Areas/Admin/
├── Controllers/
│   ├── CMS/
│   │   ├── BaseCMSController.cs
│   │   ├── InsuranceInfoController.cs
│   │   └── ...
│   └── DashboardController.cs              ← جدید (اختیاری؛ یا تحت CMS)
├── Views/
│   ├── Shared/
│   │   ├── _AdminLayout.cshtml             ← موجود (بدون تغییر برای سازگاری)
│   │   ├── _AdminPlatformLayout.cshtml     ← جدید: لایوت اختصاصی مدیر
│   │   ├── _AdminPlatformHeader.cshtml     ← جدید
│   │   ├── _AdminPlatformSidebar.cshtml     ← جدید
│   │   ├── _AdminBreadcrumb.cshtml         ← موجود یا کپی با نام جدید در صورت نیاز
│   │   ├── _CMSMenu.cshtml                 ← موجود
│   │   └── DashboardWidgets/               ← جدید
│   │       ├── _WidgetTodayAppointments.cshtml
│   │       ├── _WidgetPatientStats.cshtml
│   │       ├── _WidgetFinancialOverview.cshtml
│   │       ├── _WidgetQuickActions.cshtml
│   │       └── ...
│   ├── Dashboard/                           ← جدید
│   │   └── Index.cshtml                    ← صفحهٔ داشبورد با گرید ویجت‌ها
│   └── CMS/
│       └── ... (موجود)
Content/
├── css/
│   ├── admin-layout.css                    ← موجود
│   ├── admin-platform.css                 ← جدید: استایل لایوت پلتفرم
│   └── admin-platform-themes.css          ← جدید: تم‌ها
└── js/
    ├── admin-layout.js                    ← موجود
    └── admin-platform-layout.js            ← جدید
```

---

# بخش ۵: آماده‌سازی جریان داده (Data Flow Preparation)

- **کنترلر داشبورد:** در فاز اول می‌تواند بدون وابستگی به سرویس‌های واقعی، یک `DashboardPageViewModel` با مقدار null یا دادهٔ نمایشی (مثلاً ۰ و لیست خالی) به View بفرستد.
- **ساختار ViewModel:** وجود propertyهای ثابت برای هر ویجت (TodayAppointments, PatientStats, FinancialOverview, …) تا بعداً با سرویس‌های واقعی (IAppointmentService, IPatientService, IFinancialReportService و غیره) پر شوند.
- **اعلان‌ها:** در لایوت می‌توان یک سرویس `IAdminNotificationService` (یا استفاده از همان سرویس اعلان موجود) برای تعداد/لیست اعلان در هدر فراخوانی کرد؛ در فاز اول خالی یا نمایشی.

---

# بخش ۶: امنیت (Security Architecture)

- **Authorization:** تمام اکشن‌های داشبورد و صفحات حساس با `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]` یا یک Attribute سفارشی نقش‌محور.
- **Role-based rendering:** در Partial منو فقط لینک‌هایی که کاربر مجوز دارد ببیند (بر اساس همان نقش‌ها).
- **Permission-based (آینده):** در صورت تعریف جدول Permission و نقش-مجوز، منو و دکمه‌ها بر اساس Permission رندر شوند؛ در فاز اول فقط نقش کافی است.
- **Anti-forgery و HTTPS:** همان رویهٔ فعلی پروژه در فرم‌ها و درخواست‌های حساس.

---

# بخش ۷: عملکرد (Performance)

- **Lazy loading ویجت‌ها (اختیاری):** در فاز بعد، ویجت‌های سنگین با AJAX و `data-widget-url` بعد از لود اولیه بارگذاری شوند.
- **کش:** برای منوی سایدبار در صورت داینامیک شدن از DB، کش در سطح سرور (مثلاً MemoryCache با وابستگی به نقش) با TTL کوتاه.
- **Bundle و Minification:** فایل‌های جدید لایوت و داشبورد در Bundle با minification در Release.
- **تصاویر و فونت:** همان بهینه‌سازی فعلی (preload فونت‌های بحرانی در لایوت).

---

# بخش ۸: قوانین پیاده‌سازی و سازگاری

- **عدم شکستن سیستم فعلی:** `_AdminLayout.cshtml` و تمام Viewهای فعلی CMS و Admin بدون تغییر باقی بمانند؛ لایوت جدید (`_AdminPlatformLayout`) فقط برای صفحاتی که صریحاً آن را انتخاب کنند (مثلاً Dashboard و در آینده سایر صفحات مدیریتی).
- **سازگاری با MVC5 و .NET 4.8:** بدون استفاده از کامپوننت‌های غیرمستقل از فریم‌ورک؛ فقط Razor، Partial، Section، و در صورت نیاز Child Action یا AJAX.
- **SOLID و Clean Architecture:** سرویس‌های داده در لایهٔ Service؛ کنترلر فقط هماهنگ‌کننده؛ ViewModelها در لایهٔ مناسب (ViewModels یا در Area).
- **قابلیت نگهداری:** نام‌گذاری یکسان، پوشه‌بندی واضح، مستندات کوتاه در بالای هر Partial و کنترلر جدید.

---

این سند پایهٔ **نقشهٔ راه** و **لیست گام‌به‌گام TODO** است که در فایل جداگانهٔ `ADMIN_PLATFORM_ROADMAP_AND_TODO.md` آمده است.

---

# ضمیمه: فایل‌های اسکلت ایجادشده

برای شروع بدون شکستن سیستم فعلی، اسکلت‌های زیر اضافه شده‌اند:

| فایل | نقش |
|------|-----|
| `Areas/Admin/Views/Shared/_AdminPlatformLayout.cshtml` | لایوت اختصاصی مدیر؛ استفاده اختیاری با `Layout = "~/Areas/Admin/Views/Shared/_AdminPlatformLayout.cshtml"` |
| `Areas/Admin/Views/Shared/_AdminPlatformHeader.cshtml` | هدر: لوگو، نوتیفیکیشن، تم، کاربر، خروج |
| `Areas/Admin/Views/Shared/_AdminPlatformSidebar.cshtml` | سایدبار نقش‌آگاه؛ لینک CMS با `Url.RouteUrl("Admin_CMS_Default", ...)` |
| `Content/css/admin-platform.css` | استایل ساختار لایوت، هدر، سایدبار، گرید ویجت |
| `Content/css/admin-platform-themes.css` | تم Light و Medical Blue (متغیرهای CSS) |
| `Content/js/admin-platform-layout.js` | toggle سایدبار موبایل، تغییر تم و ذخیره در کوکی |
| `Areas/Admin/Views/Shared/DashboardWidgets/_WidgetQuickActions.cshtml` | ویجت عملیات سریع (لینک‌های ثابت) |
| `Areas/Admin/Views/Shared/DashboardWidgets/_WidgetTodayAppointments.cshtml` | ویجت نوبت‌های امروز (placeholder؛ آماده اتصال داده) |

**نکته:** صفحات فعلی CMS (مثل InsuranceInfo) همچنان از `_AdminLayout.cshtml` استفاده می‌کنند. برای استفاده از لایوت جدید، در View موردنظر `Layout = "~/Areas/Admin/Views/Shared/_AdminPlatformLayout.cshtml"` قرار دهید. پس از ایجاد کنترلر و View داشبورد (طبق TODO)، آن صفحه می‌تواند از همین لایوت و ویجت‌ها استفاده کند.
