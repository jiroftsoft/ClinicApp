# بهینه‌سازی تخصصی هر پارشال صفحه اصلی (Home)

هر پارشال به صورت جداگانه بررسی و بهینه شده است برای محیط واقعی: سریع، ریسپانسیو، بدون ایراد نمایش و با رعایت دسترسی‌پذیری.

---

## Index.cshtml

- **رفع باگ:** حذف `@{ }` و `;` اضافی؛ همهٔ سکشن‌ها با `@Html.RenderPartial(...)` بدون بلوک اضافه.
- **ساختار:** هر سکشن با `aria-label` و در صورت نیاز `data-lazy-load` و `data-section-type` / `data-section-id`.

---

## ۱. _AnnouncementsSection

| مورد | قبل | بعد |
|-----|-----|-----|
| Landmark | `<section class="announcements-section">` (تکراری با Index) | `<div class="announcements-inner">` |
| آیکون | بدون `aria-hidden` | `<i ... aria-hidden="true">` برای آیکون تزئینی |

- **CSS:** از باندل `homepage-sections`.
- **کاروسل:** Bootstrap با `data-ride="carousel"`؛ وابسته به لود صحیح Bootstrap در Layout.

---

## ۲. _PromotionalEventsSection

| مورد | قبل | بعد |
|-----|-----|-----|
| Landmark | `<section>` تکراری | `<div class="promotional-events-inner">` |
| لینک CTA | مسیر ثابت `/Patient/Appointment/Available` | `Url.Action("Available", "Appointment", new { area = "Patient" })` |
| CSS | لینک مستقیم در پارشال | حذف لینک؛ از باندل |

---

## ۳. _HeroSection

- قبلاً بهینه شده: init روی DOM + `window.load`، حذف console، اسلاید اول در CSS قابل مشاهده.
- **ساختار:** کاروسل با `#heroCarousel`؛ اسلاید تکی با `<div class="hero-single">`.

---

## ۴. _ValuePropositionSection

- انیمیشن با اسکریپت **inline** (بدون @section).
- **CSS:** از باندل.

---

## ۵. _QuickAppointmentSection

- **ساختار:** بدون تغییر؛ لینک‌ها از ViewModel.
- **CSS:** از باندل.

---

## ۶. _ServicesSection

- لینک «مشاهده تمام خدمات»: `Url.Action("Index", "MedicalServiceInfo")`.
- **CSS:** از باندل؛ بدون @section.

---

## ۷. _MedicalServicesSection

- تصاویر با `loading="lazy"` و ارتفاع ثابت در CSS؛ اسکریپت انیمیشن inline.
- **CSS:** از باندل.

---

## ۸. _MedicalEquipmentSection

| مورد | قبل | بعد |
|-----|-----|-----|
| Landmark | `<section class="medical-equipment-showcase">` | `<div class="medical-equipment-inner">` |
| شرط خروج | `@if (equipments != null && ...)` با بلوک | `@if (equipments == null || !equipments.Any()) { return; }` و محتوای بدون بلوک اضافه |

- **CSS:** از باندل.

---

## ۹. _InsuranceInfoSection

| مورد | قبل | بعد |
|-----|-----|-----|
| Landmark | `<section class="insurance-info-section">` | `<div class="insurance-info-inner">` |
| CSS | لینک مستقیم insurance-info-section.css | حذف؛ از باندل؛ فقط Swiper CSS در پارشال |
| Console | `console.warn` / `console.error` | حذف |

---

## ۱۰. _DoctorsSection

- تصاویر با `loading="lazy"` و `onerror`؛ اسکریپت انیمیشن inline.
- **CSS:** از باندل.

---

## ۱۱. _TestimonialsSection

| مورد | قبل | بعد |
|-----|-----|-----|
| Landmark | `<section class="testimonials-section">` | `<div class="testimonials-inner">` |
| CSS | لینک testimonials-section + Swiper | فقط لینک Swiper؛ testimonials از باندل |
| Console | در اسکریپت Swiper | حذف |

---

## ۱۲. _GallerySection

| مورد | قبل | بعد |
|-----|-----|-----|
| Landmark | `<section class="gallery-section">` | `<div class="gallery-inner">` |
| @section | Styles + Scripts (در پارشال اجرا نمی‌شود) | حذف؛ لینک gallery-lightbox.js و اسکریپت انیمیشن inline |
| CSS | در @section | اضافه به باندل `homepage-sections` |
| onerror تصویر | مقدار داینامیک در رشته | مسیر ثابت `/Content/Images/default-gallery.jpg` |

---

## ۱۳. _BlogSection

| مورد | قبل | بعد |
|-----|-----|-----|
| Landmark | `<section class="blog-section">` | `<div class="blog-inner">` |
| شرط | `@if (Model.Posts != null && ...)` با بلوک | خروج زودهنگام و `<div class="blog-inner">` |
| CSS | لینک مستقیم | حذف؛ از باندل |
| اسکریپت انیمیشن | وابسته به DOMContentLoaded | IIFE با چک `readyState` و در صورت نیاز DOMContentLoaded |

---

## ۱۴. _HealthTipsSection

| مورد | قبل | بعد |
|-----|-----|-----|
| Landmark | `<section class="health-tips-section">` | `<div class="health-tips-inner">` |
| CSS | لینک مستقیم | حذف؛ از باندل |

---

## ۱۵. _VideoSection

| مورد | قبل | بعد |
|-----|-----|-----|
| Landmark | `<section class="video-section">` | `<div class="video-inner">` |
| @section | Styles + Scripts (در پارشال اجرا نمی‌شود) | حذف؛ لینک video-modal.js و اسکریپت انیمیشن inline |
| CSS | در @section | اضافه به باندل `homepage-sections` |
| انتخابگر کلیک | اسکریپت به `.video-thumbnail-container` وابسته بود | کلاس `video-thumbnail-container` به wrapper اضافه شد |
| Console | در اسکریپت | حذف |

---

## ۱۶. _StoriesSection

| مورد | قبل | بعد |
|-----|-----|-----|
| Landmark | `<section class="stories-section">` | `<div class="stories-inner">` |
| آیکون‌ها | بدون `aria-hidden` | `aria-hidden="true"` برای آیکون‌های تزئینی |

---

## ۱۷. _FAQSection

- قبلاً بهینه شده: CSS از باندل؛ اسکریپت آکوردئون از Index.
- **ساختار:** آکوردئون Bootstrap با `data-toggle="collapse"`.

---

## ۱۸. _ContactSection

- بدون تغییر ساختاری؛ لینک‌ها و داده از ViewModel.
- **CSS:** از باندل.

---

## ۱۹. _FooterSliderSection

- حذف `@section Styles`؛ **CSS:** از باندل (فایل `footer-slider-section.css` به باندل اضافه شد).

---

## ۲۰. _SidebarSection

- بدون تغییر؛ **CSS:** از باندل (medical-sidebar).

---

## ۲۱. _SidebarSliderSection

- حذف `@section Styles`؛ **CSS:** فایل `sidebar-slider-section.css` به باندل `homepage-sections` اضافه شد.

---

## ۲۲. _EmergencyContactsSection

- بدون تغییر محتوا؛ فقط یک نوار تماس اضطراری با `role="alert"` و `aria-live="polite"`.

---

## Bundle (homepage-sections)

فایل‌های CSS اضافه‌شده به باندل:

- `gallery-section.css`
- `video-section.css`
- `sidebar-slider-section.css`

بقیهٔ فایل‌های سکشن‌ها قبلاً در باندل بودند.

---

## اصول اعمال‌شده در همهٔ پارشال‌ها

1. **Landmark تکراری:** جایی که Index خودش `<section class="...">` دارد، در پارشال از `<div class="...-inner">` استفاده شد تا فقط یک `<section>` با `aria-label` وجود داشته باشد.
2. **@section در پارشال:** در پارشال `@section` اجرا نمی‌شود؛ همهٔ CSS از باندل در Index و در صورت نیاز لینک/اسکریپت به صورت inline در پارشال.
3. **لینک‌ها:** استفاده از `Url.Action` به‌جای مسیر ثابت.
4. **دسترسی‌پذیری:** `aria-hidden="true"` برای آیکون‌های تزئینی، `aria-label` برای لینک/دکمه.
5. **Console:** حذف `console.log` / `console.warn` / `console.error` در اسکریپت‌های production.
6. **انیمیشن:** اسکریپت‌های Intersection Observer به صورت IIFE با چک `document.readyState` تا در هر ترتیب لود درست اجرا شوند.

---

تاریخ: بهمن ۱۴۰۴
