# بررسی و بهینه‌سازی Views/Home و پارشال‌ها

## خلاصه

- **Index**: استفاده از `Html.RenderPartial` به‌جای `Html.Partial`، اضافه شدن `aria-label` برای سکشن‌ها، لود `faq-accordion.js` در Scripts.
- **HeroSection**: حذف تمام `console.log` و اسکریپت‌های دیباگ، ساده‌سازی init کاروسل، اسلاید تکی با `<div>` به‌جای `<section>` تکراری.
- **پارشال‌ها**: در پارشال `@section` اجرا نمی‌شود؛ CSS سکشن‌ها به باندل `homepage-sections` منتقل و لینک/اسکریپت تکراری حذف شد.
- **Bundle**: اضافه شدن `hero-section`, `hero-carousel`, `value-proposition-section`, `quick-appointment-section`, `announcements-section`, `faq-section`, `modern-services-section` به باندل.
- **_ServicesSection**: لینک «مشاهده تمام خدمات» از `/Services` به `Url.Action("Index", "MedicalServiceInfo")` تغییر کرد (کنترلر Services وجود ندارد).

---

## ۱. Index.cshtml

| مورد | قبل | بعد |
|------|-----|-----|
| رندر پارشال | `@Html.Partial("~/Views/Home/Sections/...")` | `@{ Html.RenderPartial("Sections/...", Model.XXX); }` (نوشتن مستقیم در خروجی، بدون رشته میانی) |
| دسترسی‌پذیری | بدون aria-label روی سکشن‌ها | `aria-label` برای هر سکشن (مثلاً «اطلاعیه‌های مهم»، «اسلایدر صفحه اصلی») |
| اسکریپت FAQ | فقط در پارشال با `@section Scripts` (اجرا نمی‌شد) | لود `faq-accordion.js` در `@section Scripts` در Index |

---

## ۲. HeroSection (_HeroSection.cshtml)

| مورد | قبل | بعد |
|------|-----|-----|
| دیباگ | چندین بلوک `<script>` با `console.log` / `console.warn` / `console.error` | حذف کامل؛ بدون لاگ در production |
| Init کاروسل | چک هر ۲۰۰ms تا ۱۰ بار + لاگ | یک حلقه ساده با `setTimeout` تا `initHeroCarousel` یا `HeroCarousel` موجود شود |
| اسلاید تکی | `<section class="hero-section">` داخل سکشن اصلی (تکرار landmark) | `<div class="hero-single">` برای جلوگیری از دو بار `<section>` |
| استایل | لینک مستقیم به hero-section.css و hero-carousel.css | حذف لینک؛ لود از باندل `homepage-sections` |

---

## ۳. پارشال‌ها و @section

در ASP.NET MVC، داخل پارشال‌هایی که با `Html.Partial` / `Html.RenderPartial` فراخوانی می‌شوند، **`@section` اجرا نمی‌شود**. فقط در View اصلی (مثلاً Index) سکشن‌ها در Layout رندر می‌شوند.

| پارشال | قبل | بعد |
|--------|-----|-----|
| _ValuePropositionSection | `@section Styles` + `@section Scripts` | حذف سکشن‌ها؛ اسکریپت انیمیشن به صورت inline در پارشال |
| _QuickAppointmentSection | `@section Styles` | حذف؛ CSS از باندل |
| _AnnouncementsSection | `<link>` مستقیم | حذف لینک؛ CSS از باندل |
| _FAQSection | `<link>` + `@section Scripts` (اسکریپت اجرا نمی‌شد) | حذف لینک و سکشن؛ CSS از باندل، اسکریپت از Index |
| _ServicesSection | `@section Styles` | حذف؛ CSS از باندل |
| _MedicalServicesSection | `<link>` + اسکریپت inline | حذف لینک؛ CSS از باندل، اسکریپت همان‌جا |
| _DoctorsSection | `<link>` + اسکریپت inline | حذف لینک؛ CSS از باندل، اسکریپت همان‌جا |

---

## ۴. Bundle (BundleConfig.cs)

به باندل `~/Content/css/homepage-sections` اضافه شد:

- `hero-section.css`
- `hero-carousel.css`
- `value-proposition-section.css`
- `quick-appointment-section.css`
- `announcements-section.css`
- `faq-section.css`
- `modern-services-section.css`

ترتیب فایل‌های قبلی (medical-services-section، doctors-section، ...) حفظ شده است.

---

## ۵. کامپوننت _SectionWrapper

- **وضعیت**: در هیچ View یا پارشال دیگری استفاده نشده (فقط در خود فایل اشاره شده).
- **مشکل**: از `@RenderBody()` استفاده می‌کند که فقط در Layout معنا دارد؛ در پارشال بدنه‌ای رندر نمی‌شود.
- **پیشنهاد**: یا حذف شود، یا به عنوان Layout جدا (مثلاً برای یک صفحه خاص) با تعریف بدنه استفاده شود؛ در ساختار فعلی Home استفاده نمی‌شود.

---

## ۶. چک‌لیست سکشن‌ها

| سکشن | پارشال | داده از | نکته |
|------|--------|---------|------|
| Announcements | _AnnouncementsSection | Model.Announcements | کاروسل Bootstrap؛ نیاز به JS بوت‌استرپ |
| PromotionalEvents | _PromotionalEventsSection | Model.PromotionalEvents | |
| Hero | _HeroSection | Model.Hero | کاروسل سفارشی یا اسلاید تکی |
| ValueProposition | _ValuePropositionSection | Model.ValueProposition | انیمیشن با اسکریپت inline |
| Services | _ServicesSection | Model.Services | لینک «مشاهده تمام» به MedicalServiceInfo/Index |
| MedicalServices | _MedicalServicesSection | Model.MedicalServiceInfos | |
| MedicalEquipment | _MedicalEquipmentSection | Model | |
| InsuranceInfo | _InsuranceInfoSection | Model.InsuranceInfos | |
| Doctors | _DoctorsSection | Model.Doctors | |
| QuickAppointment | _QuickAppointmentSection | Model.QuickAppointment | |
| Testimonials | _TestimonialsSection | Model.Testimonials | |
| Gallery | _GallerySection | Model.Gallery | |
| Blog | _BlogSection | Model.Blog | |
| HealthTips | _HealthTipsSection | Model.HealthTips | |
| Video | _VideoSection | Model.Videos | |
| Stories | _StoriesSection | Model.Stories | |
| FAQ | _FAQSection | Model.FAQs | آکوردئون؛ faq-accordion.js از Index |
| Contact | _ContactSection | Model.Contact | Model.EmergencyContacts از سرویس |
| Sidebar | _SidebarSection | Model.Sidebar | |
| FooterSliders | _FooterSliderSection | Model.FooterSliders | |

---

## ۷. پیشنهادهای بعدی (اختیاری)

1. **Controller Partial Actions**: سکشن‌هایی که با `[ChildActionOnly]` و کش جدا تعریف شده‌اند (مثل AnnouncementsSection، FAQSection) در Index فعلاً با دادهٔ همان ViewModel رندر می‌شوند؛ اگر بخواهید هر سکشن از اکشن جدا و با کش مستقل لود شود، می‌توان با `@Html.Action(...)` آن‌ها را فراخوانی کرد.
2. **RTL**: در پارشال‌هایی که آیکون جهت دارند (مثل فلش کاروسل)، اطمینان از کلاس/جهت RTL در CSS (مثلاً چرخش آیکون).
3. **prefers-reduced-motion**: در اسکریپت‌های انیمیشن (مثلاً ValueProposition، MedicalServices، Doctors) می‌توان با `matchMedia('(prefers-reduced-motion: reduce)')` انیمیشن را غیرفعال کرد.
4. **_MainMenuQuickActions**: در Index استفاده نمی‌شود (کامنت حذف دسترسی سریع وجود دارد)؛ در صورت استفاده در Layout یا جای دیگر، لینک‌ها و aria-labelها بررسی شوند.

---

تاریخ بررسی: بهمن ۱۴۰۴
