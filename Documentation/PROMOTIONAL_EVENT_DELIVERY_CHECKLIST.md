# چک‌لیست تحویل نهایی — ماژول ایونت‌های تبلیغاتی (PromotionalEvent)

## فاز نهایی تحویل پروژه

این سند وضعیت ماژول **PromotionalEvent** را از نظر **بهینه‌سازی**، **ارتباط با سایر ماژول‌ها** و **آمادگی تحویل** خلاصه می‌کند.

---

## ۱. آیا نیاز به بهینه‌سازی دارد؟

| مورد | وضعیت | توضیح |
|------|--------|--------|
| **ساختار کد** | ✅ مناسب تحویل | Entity، Repository، Service، Controller، ViewModels، Validator مطابق SRP و قراردادهای پروژه |
| **قرارداد مالی** | ✅ رعایت شده | DiscountValue و مبالغ مرتبط decimal(18,0) برای IRR |
| **امنیت** | ✅ اصلاح شد | `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]` روی کنترلر فعال شد |
| **دسترسی در منو** | ✅ اصلاح شد | لینک «ایونت‌های تبلیغاتی» به منوی Admin (دسته «مدیریت نوبت‌ها») اضافه شد |
| **تاریخ نوبت در قیمت** | ✅ انجام شده | GetAppointmentPriceAsync و API با پارامتر اختیاری appointmentDate برای اعمال صحیح تخفیف |
| **چند ایونت هم‌زمان** | ⚠️ طراحی فعلی | فقط یک PromotionalEventId در نوبت ذخیره و IncrementUsedSlots برای همان یک ایونت؛ در صورت نیاز کسب‌وکار به شمارش همه ایونت‌ها، در فاز بعد قابل توسعه است |
| **EndDate (آخر روز)** | ⚠️ اختیاری | در صورت نیاز می‌توان در UI/سرویس EndDate را به انتهای روز تنظیم کرد |

**جمع‌بندی بهینه‌سازی:** ماژول برای تحویل **آماده** است. بهینه‌سازی‌های ضروری (Authorize، منو، تاریخ در قیمت) اعمال شده‌اند. موارد اختیاری (چند ایونت، EndDate) در صورت نیاز در فازهای بعد قابل انجام است.

---

## ۲. آیا ماژول کامل با سایر ماژول‌ها ارتباط دارد؟

| ماژول مرتبط | نوع ارتباط | وضعیت |
|-------------|------------|--------|
| **DoctorSchedule** | قیمت پایه نوبت از ConsultationFee؛ تخفیف روی همین مبلغ اعمال می‌شود | ✅ کامل — مستند در DoctorSchedule_PromotionalEvent_Integration.md |
| **Appointment** | ذخیره Price، DiscountAmount، PromotionalEventId؛ رابطه FK با PromotionalEvent | ✅ کامل |
| **AppointmentPricingService** | محاسبه قیمت با GetBasePrice (DoctorSchedule) + CalculateDiscount (PromotionalEvent) | ✅ کامل |
| **AppointmentBookingService** | رزرو با appointmentDate؛ ذخیره تخفیف؛ IncrementUsedSlots بعد از Commit | ✅ کامل |
| **Patient (رزرو آنلاین)** | ConfirmBooking با parsedAppointmentDate برای نمایش قیمت با تخفیف؛ API GetAppointmentPrice با appointmentDate اختیاری | ✅ کامل |
| **Admin (مدیریت)** | CRUD ایونت، جستجو، انتخاب پزشکان، تاریخ شمسی؛ منو و Authorize | ✅ کامل |
| **Reception (پذیرش)** | قیمت نوبت از قبل در Appointment ذخیره شده؛ پذیرش از نوبتِ محاسبه‌شده استفاده می‌کند | ✅ بدون شکاف — ارتباط غیرمستقیم از طریق نوبت |
| **Payment** | مبلغ پرداختی از Appointment.Price (قیمت نهایی پس از تخفیف) | ✅ بدون شکاف |

**جمع‌بندی ارتباط:** ارتباط ماژول PromotionalEvent با **DoctorSchedule**، **Appointment**، **قیمت‌گذاری نوبت**، **رزرو آنلاین** و **ادمین** کامل است. ارتباط با **پذیرش** و **پرداخت** به‌صورت غیرمستقیم (از طریق نوبت و مبلغ ذخیره‌شده) برقرار است و شکافی برای تحویل وجود ندارد.

---

## ۳. چک‌لیست تحویل نهایی

| ردیف | مورد | وضعیت |
|------|------|--------|
| 1 | Entity و Config با decimal(18,0) و قرارداد مالی | ✅ |
| 2 | CRUD کامل (Create, Update, Delete نرم) | ✅ |
| 3 | محاسبه تخفیف با تاریخ نوبت و محدودیت پزشک/تعداد | ✅ |
| 4 | ذخیره PromotionalEventId و DiscountAmount در Appointment | ✅ |
| 5 | IncrementUsedSlots بعد از رزرو موفق | ✅ |
| 6 | GetAppointmentPrice با appointmentDate اختیاری (نمایش صحیح تخفیف) | ✅ |
| 7 | Authorize روی کنترلر (Admin, Receptionist) | ✅ |
| 8 | لینک «ایونت‌های تبلیغاتی» در منوی Admin | ✅ |
| 9 | ViewModels، Factory، Validator، Views | ✅ |
| 10 | DI و DbSet و Migration | ✅ |
| 11 | مستندات (PROMOTIONAL_EVENT_FULL_REVIEW، DoctorSchedule_PromotionalEvent_Integration) | ✅ |

---

## ۴. جمع‌بندی برای تحویل

- **بهینه‌سازی:** ماژول برای تحویل بهینه است؛ موارد ضروری (امنیت، منو، تاریخ در قیمت) اعمال شده‌اند.
- **ارتباط با ماژول‌ها:** ارتباط با DoctorSchedule، Appointment، قیمت‌گذاری، رزرو آنلاین و ادمین کامل است؛ با پذیرش و پرداخت به‌صورت غیرمستقیم از طریق نوبت.
- **تحویل:** چک‌لیست بالا برای فاز نهایی تحویل پروژه تکمیل است. در صورت نیاز به «چند ایونت با شمارش جداگانه» یا «EndDate به صورت آخر روز»، می‌توان در فاز بعد پیاده‌سازی کرد.
