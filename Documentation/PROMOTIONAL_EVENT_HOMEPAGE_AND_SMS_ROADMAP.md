# نقشه راه و TODO — نمایش ایونت‌های تبلیغاتی در صفحه اصلی و اطلاع‌رسانی پیامکی (آسانک)

## هدف کلی

۱. **نمایش ایونت‌های تبلیغاتی** در صفحه اصلی (و در صورت نیاز داشبورد بیمار و صفحه نوبت‌گیری) به صورت **جذاب، زیبا، حرفه‌ای، رسمی و کاربرپسند** تا بازدیدکننده و بیمار از تخفیف‌ها مطلع شود.  
۲. **اطلاع‌رسانی پیامکی** به مشتریان کلینیک (از طریق سرویس آسانک) برای ایونت‌های تبلیغاتی با **نقشه راه و TODO با دقت بالا**.

---

# بخش اول: نمایش ایونت‌های تبلیغاتی در صفحه اصلی

## ۱.۱ اهداف و الزامات

| مورد | توضیح |
|------|--------|
| **محل نمایش** | صفحه اصلی سایت (Home/Index)، ترجیحاً بالای صفحه یا بلافاصله بعد از Hero/Announcements |
| **مخاطب** | بازدیدکننده عمومی (AllowAnonymous)، بیمار لاگین‌شده |
| **طراحی** | جذاب، زیبا، حرفه‌ای، رسمی، کاربرپسند، مطابق پالت درمانی (--medical-*)، بدون گرادینت فانتزی |
| **محتوا** | عنوان ایونت، بازه تاریخ، نوع و مقدار تخفیف، دکمه/لینک «رزرو نوبت» |
| **فیلتر** | فقط ایونت‌های **فعال** و در **بازه زمانی جاری** (StartDate ≤ امروز ≤ EndDate) و در صورت امکان با ظرفیت باقی‌مانده (TotalSlots) |

## ۱.۲ وابستگی‌های فنی

| لایه | وابستگی |
|------|----------|
| **داده** | `IPromotionalEventRepository.GetActiveEventsAsync(DateTime? appointmentDate)` یا سرویس مشابه — از قبل وجود دارد |
| **سرویس** | `IHomePageService` — اضافه کردن متد `GetPromotionalEventsSectionAsync()` یا بارگذاری ایونت‌ها در `GetHomePageDataAsync` |
| **ViewModel** | `HomePageViewModel` — اضافه کردن پراپرتی `List<PromotionalEventPublicViewModel> PromotionalEvents` |
| **View** | `Views/Home/Index.cshtml` — اضافه کردن یک سکشن با Partial برای ایونت‌ها |
| **Partial** | `Views/Home/Sections/_PromotionalEventsSection.cshtml` — طراحی کارت/بنر ایونت‌ها |

## ۱.۳ طراحی UI پیشنهادی (رسمی و کاربرپسند)

- **قالب:** یک سکشن با عنوان مثلاً «تخفیف‌ها و جشنواره‌های ویژه» یا «ایونت‌های تبلیغاتی».
- **کارت هر ایونت:** پس‌زمینه سفید/خاکستری روشن، حاشیه راست رنگی (مثلاً --medical-primary)، آیکون هدیه/تخفیف، عنوان، بازه تاریخ (شمسی)، متن تخفیف (مثلاً «۲۰٪ تخفیف» با رنگ قرمز ملایم یا سبز)، دکمه «رزرو نوبت» که به `/Patient/Appointment/Available` یا صفحه نوبت‌گیری لینک دهد.
- **دسترسی‌پذیری:** عنوان سکشن با `aria-label`، دکمه‌ها با `aria-label` و `title`.
- **ریسپانسیو:** در موبایل کارت‌ها به صورت ستون واحد یا اسکرول افقی در صورت نیاز.

## ۱.۴ TODO لیست — نمایش در صفحه اصلی

| ردیف | وظیفه | وضعیت | اولویت |
|------|--------|--------|--------|
| 1 | تعریف `PromotionalEventPublicViewModel` (EventId, Title, Description, StartDate, EndDate, DiscountType, DiscountValue, DiscountDisplayText, CtaUrl) | ⬜ | بالا |
| 2 | اضافه کردن `List<PromotionalEventPublicViewModel> PromotionalEvents` به `HomePageViewModel` | ⬜ | بالا |
| 3 | اضافه کردن متد `GetPromotionalEventsSectionAsync(int? clinicId)` به `IHomePageService` و پیاده‌سازی در `HomePageService` با استفاده از `IPromotionalEventRepository.GetActiveEventsAsync(DateTime.Now)` | ⬜ | بالا |
| 4 | در `HomePageService.GetHomePageDataAsync` بارگذاری موازی ایونت‌ها و پر کردن `viewModel.PromotionalEvents` | ⬜ | بالا |
| 5 | ثبت `IPromotionalEventRepository` در `HomePageService` از طریق سازنده (در صورت نبودن، از سرویس PromotionalEvent استفاده شود) | ⬜ | بالا |
| 6 | ایجاد Partial View `Views/Home/Sections/_PromotionalEventsSection.cshtml` با طراحی کارت ایونت (پالت درمانی، بدون گرادینت، دکمه CTA) | ⬜ | بالا |
| 7 | در `Views/Home/Index.cshtml` اضافه کردن سکشن ایونت‌ها (پس از Announcements یا پس از Quick Appointment) با شرط `Model.PromotionalEvents != null && Model.PromotionalEvents.Any()` | ⬜ | بالا |
| 8 | (اختیاری) نمایش همان سکشن یا ویجت کوچک در داشبورد بیمار (`Patient/Dashboard/Index`) | ⬜ | متوسط |
| 9 | (اختیاری) نمایش بنر/کارت ایونت‌های فعال در صفحه لیست پزشکان (`Patient/Appointment/Available`) | ⬜ | متوسط |

---

# بخش دوم: اطلاع‌رسانی پیامکی (آسانک) به مشتریان

## ۲.۱ وضعیت فعلی سرویس پیامکی

| مورد | وضعیت |
|------|--------|
| **سرویس ارسال** | `AsanakSmsService` (آسانک) — پیاده‌سازی شده، تنظیمات از Web.config (Asanak:Username, Password, SourceNumber, Enabled) |
| **استفاده فعلی** | Identity (تأیید شماره، 2FA)، `NewsletterSmsService` برای ارسال SMS خبرنامه به مشترکین |
| **الگوی خبرنامه** | `INewsletterSmsService.SendNewsletterSmsAsync(NewsletterCampaign campaign, NewsletterSubscription subscription)` — ارسال به یک مشترک با قالب و متغیرها |

## ۲.۲ سناریوهای اطلاع‌رسانی ایونت

| سناریو | توضیح | اولویت |
|--------|--------|--------|
| **الف) ارسال دستی از ادمین** | در صفحه جزئیات/ویرایش ایونت، دکمه «ارسال پیامک به مشتریان»؛ انتخاب مخاطب (بیماران با شماره / مشترکین خبرنامه / هر دو) و ارسال با قالب ثابت | بالا |
| **ب) ارسال در زمان ایجاد/فعال‌سازی ایونت** | پس از ذخیره ایونت جدید یا فعال شدن ایونت، گزینه «هم‌زمان پیامک ارسال شود» (با انتخاب مخاطب) | متوسط |
| **ج) ارسال زمان‌بندی‌شده** | مثلاً یک روز قبل از شروع ایونت، ارسال خودکار به لیست مخاطبان (نیاز به Job/Background Task) | پایین |

## ۲.۳ مخاطبان پیامک

| مخاطب | منبع داده | ملاحظات |
|--------|-----------|----------|
| **بیماران دارای شماره تلفن** | جدول Patient (PhoneNumber)، ترجیحاً با رضایت برای دریافت پیام تبلیغاتی (در صورت وجود فیلد OptIn) | رعایت حریم خصوصی و عدم اسپم |
| **مشترکین خبرنامه** | NewsletterSubscription (PhoneNumber)، معمولاً قبلاً رضایت داده‌اند | مناسب برای اطلاع‌رسانی عمومی |
| **ترکیب هر دو** | اتحاد دو لیست با حذف تکراری شماره | جلوگیری از ارسال تکراری |

## ۲.۴ محتوا و قالب پیامک

- **محدودیت:** حداکثر ۱۶۰ کاراکتر (SMS استاندارد).
- **پیشنهاد قالب:**  
  `«[نام کلینیک]» [عنوان ایونت]. تخفیف [مقدار]. تا [تاریخ پایان]. رزرو: [لینک کوتاه یا متن]`
- **متغیرها:** عنوان ایونت، نوع/مقدار تخفیف، تاریخ پایان، نام کلینیک، لینک نوبت‌گیری (در صورت استفاده از لینک کوتاه).

## ۲.۵ وابستگی‌های فنی پیامک

| لایه | وابستگی |
|------|----------|
| **ارسال SMS** | استفاده از `AsanakSmsService` (یا واسط مشترک مثل `IIdentityMessageService`) برای ارسال واقعی |
| **لیست مخاطبان** | سرویس/ریپوزیتوری برای دریافت بیماران دارای PhoneNumber و/یا مشترکین خبرنامه با PhoneNumber |
| **قالب و ارسال دسته‌ای** | سرویس جدید مثلاً `IPromotionalEventSmsService.NotifyCustomersAsync(eventId, audienceType, template)` |
| **ادمین** | اکشن در `PromotionalEventController` مثلاً `SendSmsToCustomers(int eventId)` با انتخاب مخاطب و تأیید قبل از ارسال |

## ۲.۶ TODO لیست — اطلاع‌رسانی پیامکی

| ردیف | وظیفه | وضعیت | اولویت |
|------|--------|--------|--------|
| 10 | تعریف اینترفیس `IPromotionalEventSmsService` با متد `SendEventSmsToCustomersAsync(int eventId, PromotionalEventAudience audience, string customMessage = null)` و enum `PromotionalEventAudience` (PatientsWithPhone, NewsletterSubscribers, Both) | ⬜ | بالا |
| 11 | پیاده‌سازی `PromotionalEventSmsService`: دریافت ایونت از ریپوزیتوری، ساخت متن پیامک (حداکثر 160 کاراکتر)، دریافت لیست شماره‌ها بر اساس audience، ارسال با `AsanakSmsService` (یا IIdentityMessageService)، لاگ و مدیریت خطا | ⬜ | بالا |
| 12 | سرویس/متد کمکی برای دریافت لیست شماره موبایل بیماران (Patient.PhoneNumber با اعتبارسنجی و نرمال‌سازی) و حذف تکراری | ⬜ | بالا |
| 13 | سرویس/متد کمکی برای دریافت لیست شماره مشترکین خبرنامه (NewsletterSubscription.PhoneNumber) با وضعیت تأیید شده در صورت وجود | ⬜ | بالا |
| 14 | ثبت `IPromotionalEventSmsService` در DI (UnityConfig) | ⬜ | بالا |
| 15 | اکشن GET `PromotionalEventController.SendSms(int id)`: نمایش صفحه تأیید ارسال (عنوان ایونت، تعداد تقریبی مخاطب برای هر گزینه، هشدار هزینه پیامک) | ⬜ | بالا |
| 16 | اکشن POST `PromotionalEventController.SendSms(int id, PromotionalEventAudience audience, string customMessage)`: فراخوانی `IPromotionalEventSmsService` و نمایش نتیجه (موفق/ناموفق، تعداد ارسال شده) | ⬜ | بالا |
| 17 | در View جزئیات/ویرایش ایونت، دکمه «ارسال پیامک به مشتریان» با لینک به `SendSms(id)` | ⬜ | بالا |
| 18 | (اختیاری) ذخیره تاریخ/تعداد آخرین ارسال پیامک برای ایونت در دیتابیس یا جدول NotificationHistory برای جلوگیری از ارسال مکرر و گزارش | ⬜ | متوسط |
| 19 | (اختیاری) گزینه «ارسال پیامک بعد از ذخیره» در فرم ایجاد/ویرایش ایونت با انتخاب مخاطب | ⬜ | متوسط |
| 20 | (اختیاری) Job زمان‌بندی‌شده برای ارسال یک روز قبل از StartDate ایونت (وابسته به زیرساخت Job در پروژه) | ⬜ | پایین |

---

# بخش سوم: اولویت‌بندی و ترتیب پیشنهادی

1. **فاز ۱ — نمایش در صفحه اصلی:** انجام TODOهای 1 تا 7 (ViewModel، سرویس، Partial، یکپارچه‌سازی در Home/Index).
2. **فاز ۲ — پیامک دستی از ادمین:** انجام TODOهای 10 تا 17 (سرویس پیامک ایونت، اکشن‌های SendSms، دکمه در View).
3. **فاز ۳ — اختیاری:** TODOهای 8، 9 (نمایش در Dashboard و Available)، 18، 19، 20.

---

# بخش چهارم: چک‌لیست تحویل

| ردیف | مورد | وضعیت |
|------|--------|--------|
| C1 | ایونت‌های فعال در صفحه اصلی با طراحی رسمی و کاربرپسند نمایش داده می‌شوند | ⬜ |
| C2 | لینک/دکمه «رزرو نوبت» در کارت ایونت به مسیر نوبت‌گیری هدایت می‌کند | ⬜ |
| C3 | سرویس پیامک ایونت با آسانک برای ارسال به بیماران/مشترکین پیاده‌سازی شده است | ⬜ |
| C4 | ادمین می‌تواند از صفحه ایونت با یک کلیک «ارسال پیامک به مشتریان» را با انتخاب مخاطب اجرا کند | ⬜ |
| C5 | متن پیامک حداکثر 160 کاراکتر و شامل عنوان ایونت و تخفیف است | ⬜ |
| C6 | رعایت حریم خصوصی و عدم ارسال به شماره‌های بدون رضایت (در صورت وجود OptIn) | ⬜ |

---

# مراجع کد موجود

| مورد | مسیر |
|------|------|
| سرویس پیامک آسانک | `Services/AsanakSmsService.cs` — `SendAsync(IdentityMessage)` |
| ارسال SMS خبرنامه | `Services/NewsletterSmsService.cs`، `Interfaces/INewsletterSmsService.cs` |
| ایونت‌های فعال | `Repositories/PromotionalEvent/PromotionalEventRepository.cs` — `GetActiveEventsAsync(DateTime?)` |
| سرویس صفحه اصلی | `Services/HomePageService.cs` — `GetHomePageDataAsync`، لود موازی سکشن‌ها |
| ViewModel صفحه اصلی | `ViewModels/HomePageViewModel.cs` |
| نمونه سکشن صفحه اصلی | `Views/Home/Sections/_AnnouncementsSection.cshtml` |
| بیمار — شماره تلفن | `Models/Entities/Patient/Patient.cs` — `PhoneNumber` |
| مشترک خبرنامه | `Models/Entities/CMS/NewsletterSubscription.cs` — `PhoneNumber` |
| ثبت DI | `App_Start/UnityConfig.cs` |

---

# جمع‌بندی

- **صفحه اصلی:** با انجام فاز ۱، ایونت‌های تبلیغاتی در Home به صورت جذاب و رسمی نمایش داده می‌شوند و کاربر از تخفیف‌ها مطلع می‌شود.
- **پیامک (آسانک):** با انجام فاز ۲، از طریق ادمین می‌توان به مشتریان کلینیک (بیماران یا مشترکین خبرنامه) برای ایونت مشخص اطلاع پیامکی داد؛ سرویس پیامکی موجود (آسانک) و الگوی خبرنامه در طراحی در نظر گرفته شده‌اند.
- **دقت بالا:** تمام وابستگی‌ها (سرویس، ریپوزیتوری، ViewModel، View، DI و امنیت/حریم خصوصی) در نقشه راه و TODO لیست بالا شفاف و قابل پیاده‌سازی هستند.
