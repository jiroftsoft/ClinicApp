# اصلاحات اعمال‌شده بر اساس بررسی حیاتی (Critical Review)

## ۱. لینک و انکر شکسته در About.cshtml (حیاتی)

| مورد | قبل | بعد |
|-----|-----|-----|
| لینک `#medical-equipment` | عنصری با `id="medical-equipment"` در صفحه وجود نداشت | به بلوک «تجهیزات و زیرساخت‌ها» اضافه شد: `id="medical-equipment"` و `aria-labelledby="about-equipment-title"` |
| عنوان بخش | بدون id | `id="about-equipment-title"` برای ارجاع aria |

اسکریپت پایین صفحه (`document.getElementById('medical-equipment')` و smooth scroll) اکنون هدف صحیح دارد.

---

## ۲. دسترسی‌پذیری (Accessibility)

| فایل | تغییر |
|------|--------|
| **_TestimonialsSection.cshtml** | `aria-label="نظر بیمار"` به `aria-label="نظر @tFirst درباره @tService"` تغییر کرد تا برای هر کارت مشخص باشد (نام کوچک + نوع خدمت). متغیرهای `tFirst` و `tService` در ابتدای هر آیتم حلقه تعریف شدند. |

---

## ۳. لینک‌های خارجی و امنیت (rel="noopener noreferrer")

| فایل | وضعیت |
|------|--------|
| **_FAQSection.cshtml** | لینک «اطلاعات بیشتر» با `target="_blank"` اکنون `rel="noopener noreferrer"` دارد (و یک مورد تکراری حذف شد). |
| **_SidebarSection.cshtml** | قبلاً `rel="noopener noreferrer"` داشت. |
| **_ContactSection.cshtml** | قبلاً `rel="noopener noreferrer"` داشت. |

---

## ۴. اسکریپت و منطق (InsuranceInfoSection)

| مورد | قبل | بعد |
|-----|-----|-----|
| شرط loop | `slidesCount > 6` | `slidesCount >= 7` با کامنت فارسی «فقط اگر ۷ اسلاید یا بیشتر» |
| Console | `console.warn` و `console.log` و `console.error` در مسیر init/Swiper | حذف شد. |

---

## ۵. پارشال‌ها و @section (وضعیت قبلی)

بر اساس بررسی قبلی:

- **_GallerySection**, **_FooterSliderSection**, **_SidebarSliderSection**, **_VideoSection**: بلوک‌های `@section` حذف شده و CSS/JS از باندل یا به‌صورت inline در پارشال لود می‌شوند.
- در پارشال‌ویوها از `@section` استفاده نمی‌شود؛ فقط در Viewهای اصلی (مثل Index و About).

---

## ۶. encoding فارسی در _PromotionalEventsSection

- متن‌های ثابت («تخفیف‌ها و جشنواره‌های ویژه» و …) در خود view به‌صورت UTF-8 هستند.
- اگر در مرورگر به‌صورت کاراکترهای کدشده دیده شوند، معمولاً به ذخیره‌سازی فایل با encoding نادرست برمی‌گردد. توصیه: ذخیره فایل‌های `.cshtml` با **UTF-8** (ترجیحاً با BOM در محیط‌های Windows).
- در Layout مقدار `<meta charset="utf-8" />` تنظیم شده است.

---

## ۷. مدل _MedicalEquipmentSection

- مدل پارشال `HomePageViewModel` است و در Index با `Model` (همان HomePageViewModel) فراخوانی می‌شود؛ از نظر نوع مدل سازگار است.

---

## خلاصه اولویت‌بندی

| اولویت | انجام‌شده |
|--------|-----------|
| لینک و انکر About (#medical-equipment) | ✅ |
| بهبود aria-label در Testimonials | ✅ |
| rel="noopener noreferrer" برای لینک‌های target="_blank" | ✅ (FAQ؛ بقیه قبلاً داشتند) |
| اصلاح شرط loop و حذف console در InsuranceInfoSection | ✅ |
| وضعیت @section در پارشال‌ها | ✅ (قبلاً اصلاح شده) |

---

تاریخ: بهمن ۱۴۰۴
