# نقشهٔ راه و TODO — پلتفرم ادمین و داشبورد مدیریت

## نقشهٔ راه (Roadmap)

### فاز ۱ — زیرساخت لایوت و دارایی‌ها (بدون شکستن سیستم فعلی)
- تعریف لایوت اختصاصی مدیر (`_AdminPlatformLayout.cshtml`) و استفادهٔ اختیاری از آن.
- هدر و سایدبار به‌صورت Partial با قابلیت نقش (Role-aware).
- تم Light / Medical Blue و یک فایل CSS تم.
- اسکریپت حداقلی برای collapse سایدبار و toggle تم.

### فاز ۲ — داشبورد و ویجت‌ها
- کنترلر و صفحهٔ داشبورد با گرید ویجت‌ها.
- ویجت‌های نمایشی (بدون اتصال داده): خلاصه نوبت‌ها، آمار بیمار، مالی، عملیات سریع.
- ViewModel داشبورد با propertyهای آماده برای اتصال داده.

### فاز ۳ — اتصال داده و سرویس‌ها
- اتصال ویجت‌ها به سرویس‌های موجود (نوبت، بیمار، مالی).
- اعلان در هدر (در صورت وجود سرویس اعلان).
- منوی سایدبار بر اساس نقش (فیلتر آیتم‌ها).

### فاز ۴ — توسعه‌پذیری و بهینه‌سازی
- منوی داینامیک از DB (اختیاری).
- Lazy loading ویجت‌های سنگین با AJAX.
- کش منو و تنظیمات تم (localStorage از فاز ۱).

---

## TODO لیست گام‌به‌گام

### مرحله ۱: لایوت و دارایی‌های پایه
- [x] **1.1** ایجاد `Areas/Admin/Views/Shared/_AdminPlatformLayout.cshtml` (اسکلت HTML با Head، Body، جای RenderBody، Sectionهای Styles/Scripts). ✅ ایجاد شد.
- [x] **1.2** ایجاد `Areas/Admin/Views/Shared/_AdminPlatformHeader.cshtml` (لوگو، عنوان، جای نوتیفیکیشن، منوی کاربر، دکمه تم، منوی موبایل). ✅ ایجاد شد.
- [x] **1.3** ایجاد `Areas/Admin/Views/Shared/_AdminPlatformSidebar.cshtml` (لینک داشبورد، بخش CMS با آیتم‌های فعلی، بخش‌های دیگر با placeholder؛ چک نقش در Razor). ✅ ایجاد شد.
- [x] **1.4** اضافه کردن `Content/css/admin-platform.css` برای ساختار لایوت (گرید، سایدبار، هدر). ✅ ایجاد شد.
- [x] **1.5** اضافه کردن `Content/css/admin-platform-themes.css` با متغیرهای تم Light و Medical Blue. ✅ ایجاد شد.
- [x] **1.6** اضافه کردن `Content/js/admin-platform-layout.js` (toggle سایدبار، تغییر تم و ذخیره در کوکی، منوی موبایل). ✅ ایجاد شد.
- [ ] **1.7** ثبت bundleهای جدید در `BundleConfig.cs` برای admin-platform.css و در صورت نیاز admin-platform-themes.css (فعلاً لینک مستقیم در لایوت).
- [x] **1.8** در _AdminPlatformLayout: استفاده از _Breadcrumb موجود؛ قرار دادن container اعلان‌ها. ✅ انجام شده.

### مرحله ۲: روت و کنترلر داشبورد
- [x] **2.1** تعیین مسیر داشبورد: `Admin/Dashboard/Index` (کنترلر در Areas.Admin.Controllers). ✅
- [x] **2.2** ایجاد `DashboardController` با اکشن `Index` و `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]`. ✅
- [x] **2.3** ایجاد ViewModel `DashboardPageViewModel` در `ViewModels/Admin/Dashboard/` با TodayAppointments, PatientStats, FinancialOverview, ShowQuickActions. ✅
- [x] **2.4** روت از Admin_default استفاده می‌شود؛ نیازی به ثبت روت جدا نبود. ✅

### مرحله ۳: صفحهٔ داشبورد و ویجت‌ها
- [x] **3.1** ایجاد `Areas/Admin/Views/Dashboard/Index.cshtml` با Layout = _AdminPlatformLayout و گرید ویجت‌ها. ✅
- [x] **3.2** ایجاد پوشه `Areas/Admin/Views/Shared/DashboardWidgets/`. ✅ ایجاد شد.
- [x] **3.3** ایجاد Partial `_WidgetQuickActions.cshtml` (دکمه‌های لینک ثابت: پذیرش جدید، نوبت، گزارش و غیره). ✅ ایجاد شد.
- [x] **3.4** ایجاد Partial `_WidgetTodayAppointments.cshtml` با مدل اختیاری؛ نمایش عنوان و placeholder متن یا «۰ نوبت» تا اتصال داده. ✅ ایجاد شد.
- [x] **3.5** ایجاد Partial `_WidgetPatientStats.cshtml` و `_WidgetFinancialOverview.cshtml` به‌همین شکل نمایشی. ✅
- [x] **3.6** در کنترلر داشبورد: پر کردن DashboardPageViewModel با مقادیر نمایشی (۰) و ارسال به View؛ در View فراخوانی هر ویجت با `@Html.Partial(..., Model)`. ✅

### مرحله ۴: منو و نقش
- [ ] **4.1** در _AdminPlatformSidebar: شرط نمایش بر اساس `User.IsInRole(AppRoles.Admin)` و `User.IsInRole(AppRoles.Receptionist)` برای بخش‌های حساس.
- [ ] **4.2** اطمینان از تولید لینک صحیح CMS: برای آیتم‌های CMS استفاده از مسیر `/Admin/CMS/{controller}/Index` (مثلاً با `Url.RouteUrl("Admin_CMS_Default", new { controller = "InsuranceInfo", action = "Index" })` یا helper).
- [ ] **4.3** اضافه کردن لینک «داشبورد» در منوی سایدبار به مسیر اکشن داشبورد انتخاب‌شده.

### مرحله ۵: اتصال داده (بعد از تثبیت UI)
- [ ] **5.1** ایجاد یا استفاده از سرویس برای خلاصه نوبت امروز (مثلاً از IAppointmentService یا سرویس موجود).
- [ ] **5.2** پر کردن ویجت نوبت‌ها و بیمار در DashboardController از سرویس.
- [ ] **5.3** پر کردن ویجت مالی از سرویس مالی/پذیرش در صورت وجود.
- [ ] **5.4** اتصال نوتیفیکیشن هدر به سرویس اعلان (در صورت وجود).

### مرحله ۶: تست و مستندسازی
- [ ] **6.1** تست ورود با نقش Admin و رسیدن به داشبورد و صفحات CMS با لایوت جدید.
- [ ] **6.2** تست تم Light و Medical Blue و ذخیرهٔ انتخاب در localStorage.
- [ ] **6.3** به‌روزرسانی مستندات (این فایل و ADMIN_PLATFORM_ARCHITECTURE.md) با مسیرهای نهایی و نام فایل‌ها.

---

## وابستگی بین کارها

- 1.1 ← پیش‌نیاز 1.2, 1.3 (لایوت باید وجود داشته باشد تا Partialها در آن قرار گیرند).
- 1.4, 1.5, 1.6 ← برای ظاهر و رفتار لایوت لازم است.
- 2.2, 2.3 ← قبل از 3.1 و 3.6.
- 3.1 وابسته به 1.1 و 2.2.
- 4.1 و 4.2 مستقل از فاز داشبورد؛ می‌توان همزمان با مرحله ۱ انجام شود.
- مرحله ۵ بعد از تثبیت ۳ و ۴.

---

## نکات مهم

- هیچ View یا کنترلر موجودی حذف یا تغییر رفتار نکن؛ فقط فایل‌ها و کنترلرهای **جدید** اضافه شوند.
- استفاده از لایوت جدید فقط با تنظیم صریح `Layout = "~/Areas/Admin/Views/Shared/_AdminPlatformLayout.cshtml"` در Viewهای جدید (مثلاً Dashboard).
- CMS فعلی (مثل InsuranceInfo) تا زمان تصمیم مهاجرت، با _AdminLayout فعلی کار کند.
