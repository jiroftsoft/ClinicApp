# بهینه‌سازی پارشال‌های صفحه اصلی (Home) – محیط واقعی

## هدف
- نمایش **قطعی** اسلایدر در بار اول (بدون نیاز به رفرش)
- **سریع**، **ریسپانسیو**، و بدون خطای نمایش در پارشال‌ها
- اطمینان از لود صحیح CSS/JS و init درست کاروسل‌ها

---

## ۱. رفع ایراد: اسلایدر در بار اول نمایش داده نمی‌شد

### علت
- اسکریپت کاروسل Hero قبل از آماده بودن کامل DOM یا قبل از لود CSS اجرا می‌شد.
- در بار اول ممکن بود `#heroCarousel` هنوز در DOM نباشد یا CSS باندل دیر لود شود و اسلایدها مخفی بمانند.
- با رفرش، کش مرورگر و ترتیب لود عوض می‌شد و اسلایدر درست نمایش داده می‌شد.

### تغییرات انجام‌شده

#### الف) `hero-carousel.js`
- **Init یکبار**: با فلگ `isInitialized` از اجرای دوبارهٔ init جلوگیری شد.
- **چند نقطهٔ اجرا**:
  - اگر `document.readyState === 'complete' || 'interactive'` → بعد از ۵۰ms یکبار `tryInit()`.
  - در غیر این صورت ثبت روی `DOMContentLoaded` و بعد از ۵۰ms اجرای `tryInit()`.
  - **همیشه** ثبت روی `window.load` و بعد از ۱۰۰ms:
    - اگر هنوز init نشده: `tryInit()`
    - اگر قبلاً init شده: `showSlide(currentIndex, false)` و در صورت نیاز `startAutoSlide()` تا اسلاید اول و اتوپلی قطعی باشند.
  - **Fallback**: بعد از ۱.۵ ثانیه اگر `#heroCarousel` هست و هنوز init نشده، دوباره `tryInit()`.
- **حذف `console.log`** در مسیر اصلی برای محیط production.

#### ب) پارشال `_HeroSection.cshtml`
- اسکریپت inline فقط روی `window.load` بعد از ۱۵۰ms، `initHeroCarousel()` را صدا می‌زند تا بعد از لود کامل منابع (CSS، تصاویر) کاروسل حتماً init شود.

#### ج) `hero-carousel.css`
- قانون اضافه شد تا اسلاید اول (با کلاس `.active`) حتی **قبل از اجرای JS** با `opacity: 1` و `visibility: visible` و `display: flex` نمایش داده شود و در بار اول جای خالی نماند.

---

## ۲. باندل و لود CSS پارشال‌ها

- **`footer-slider-section.css`** به باندل `~/Content/css/homepage-sections` اضافه شد تا اسلایدر فوتر در صفحهٔ اصلی استایل داشته باشد.
- در پارشال **`_FooterSliderSection.cshtml`** بلوک `@section Styles` حذف شد (در پارشال اجرا نمی‌شود) و توضیح داده شد که استایل از باندل لود می‌شود.

---

## ۳. پارشال‌هایی که قبلاً بهینه شده‌اند

| پارشال | وضعیت |
|--------|--------|
| **_HeroSection** | کاروسل سفارشی با init روی DOM + load + fallback؛ اسلاید اول در CSS قابل مشاهده |
| **_AnnouncementsSection** | کاروسل Bootstrap (`data-ride="carousel"`)؛ وابسته به لود صحیح `bootstrap.bundle.min.js` در Layout |
| **_ValuePropositionSection** | انیمیشن با اسکریپت inline (بدون @section) |
| **_MedicalServicesSection** | تصاویر با `loading="lazy"`؛ wrapper با ارتفاع ثابت در CSS برای کاهش CLS |
| **_DoctorsSection** | تصاویر با `loading="lazy"` و `onerror`؛ wrapper با ارتفاع ثابت |
| **_FooterSliderSection** | CSS از باندل homepage-sections؛ بدون @section در پارشال |

---

## ۴. نکات برای محیط واقعی

1. **Bootstrap کاروسل (اطلاعیه‌ها)**  
   اگر از Bootstrap 5 استفاده می‌کنید، برای کاروسل از `data-bs-ride="carousel"` و `data-bs-slide="prev/next"` استفاده کنید. در صورت استفاده از Bootstrap 4، `data-ride` و `data-slide` فعلی درست است.

2. **ترتیب اسکریپت در Layout**  
   اطمینان حاصل کنید `hero-carousel.js` فقط یک بار لود شود (فعلاً از داخل پارشال Hero لود می‌شود). اگر در آینده آن را به `@section Scripts` منتقل کردید، دیگر در پارشال اسکریپت کاروسل را لود نکنید.

3. **تصاویر**  
   در MedicalServices و Doctors ارتفاع wrapper در CSS ثابت است؛ برای بهبود بیشتر LCP می‌توان به تگ `<img>` مقدار `width` و `height` (یا فقط نسبت ابعاد در CSS) داد تا از جابه‌جایی layout جلوگیری شود.

4. **پیشنهاد تست**  
   - بار اول با کش خالی (Ctrl+F5 یا حذف کش) تست کنید.  
   - سرعت شبکه را روی Slow 3G قرار دهید و دوباره بار اول را چک کنید تا اسلایدر حتماً نمایش داده شود.

---

تاریخ: بهمن ۱۴۰۴
