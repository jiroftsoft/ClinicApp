# مسیر انتخاب بیمار برای مشاوره آنلاین

این سند مسیر رزرو نوبت بیمار و **محل انتخاب نوع ویزیت (مشاوره آنلاین)** را مشخص می‌کند.

---

## ۱. مسیر فعلی رزرو نوبت (۴ مرحله)

| مرحله | عنوان صفحه | مسیر (URL) | کنترلر / اکشن |
|--------|-------------|------------|----------------|
| **۱** | انتخاب پزشک | `/Patient/Appointment/Book/SelectDoctor` | `AppointmentBookingController.SelectDoctor` |
| **۲** | انتخاب تاریخ | `/Patient/Appointment/Book/SelectDate/{doctorId}` | `AppointmentBookingController.SelectDate` |
| **۳** | انتخاب زمان | `/Patient/Appointment/Book/SelectTime/{doctorId}/{date}` | `AppointmentBookingController.SelectTime` |
| **۴** | تأیید نهایی و پرداخت | `/Patient/Appointment/Book/Confirm?doctorId=...&appointmentDate=...&startTime=...&endTime=...` | `AppointmentBookingController.ConfirmBooking` (GET) سپس `Reserve` (POST) |

**نمونه مسیر کامل (با دامنه):**
- پایه: `https://mehranyad.ir/` (یا دامنه سایت)
- مرحله ۱: `https://mehranyad.ir/Patient/Appointment/Book/SelectDoctor`
- مرحله ۲: `https://mehranyad.ir/Patient/Appointment/Book/SelectDate/2` (مثال: doctorId=2)
- مرحله ۳: `https://mehranyad.ir/Patient/Appointment/Book/SelectTime/2/2025-02-17` (مثال: doctorId=2, date=2025-02-17)
- مرحله ۴: `https://mehranyad.ir/Patient/Appointment/Book/Confirm?doctorId=2&appointmentDate=2025-02-17&startTime=09:00&endTime=09:30`

---

## ۲. وضعیت فعلی: انتخاب «نوع ویزیت» / دسته‌بندی خدمت

- در **مرحله ۳ (SelectTime)** بیمار فقط **زمان** را انتخاب می‌کند؛ در اسکریپت `Scripts/patient/time-selection.js` هنگام کلیک روی «ادامه به تأیید نهایی» فقط این پارامترها به **ConfirmBooking** ارسال می‌شوند:
  - `doctorId`, `appointmentDate`, `startTime`, `endTime`
  - **`serviceCategoryId`** می‌تواند در URL اختیاری باشد.
- در **مرحله ۴ (ConfirmBooking)** — **پیاده‌سازی شده (فاز ۲.۲):**
  - کنترلر GET لیست دسته‌بندی‌های خدمتی آن پزشک را از `IAppointmentBookingService.GetServiceCategoriesForDoctorLookupAsync(doctorId)` می‌گیرد و با **ViewBag.ServiceCategories** به View می‌دهد.
  - در View اگر برای پزشک دسته‌بندی خدمتی تعریف شده باشد، یک **دراپ‌داون «نوع ویزیت»** نمایش داده می‌شود (شامل مثلاً «مشاوره آنلاین» با `ServiceCategoryId = 7`). مقدار انتخاب‌شده با اسکریپت به فیلد مخفی `ServiceCategoryId` همگام و در POST به **Reserve** ارسال می‌شود.
- اگر برای پزشک هیچ دسته‌بندی خدمتی انتساب داده نشده باشد، فقط فیلد مخفی `ServiceCategoryId` (خالی) ارسال می‌شود و نوبت به‌صورت ویزیت عادی ذخیره می‌شود.

**جمع‌بندی:** مسیر انتخاب بیمار برای مشاوره آنلاین در **صفحه تأیید نهایی (ConfirmBooking)** با دراپ‌داون «نوع ویزیت» پیاده‌سازی شده است. برای نمایش گزینه «مشاوره آنلاین» باید دسته‌بندی مربوط (مثلاً با Id=7) در ادمین به آن پزشک **انتساب** شده باشد.

---

## ۳. محل پیشنهادی برای اضافه کردن انتخاب مشاوره آنلاین

### گزینه الف: در صفحه **تأیید نهایی (ConfirmBooking)** — پیشنهاد اول

- **صفحه:** `Areas/Patient/Views/AppointmentBooking/ConfirmBooking.cshtml`
- **کنترلر:** `AppointmentBookingController.ConfirmBooking` (GET) و `Reserve` (POST)
- **کار:** در همین صفحه یک **لیست/دراپ‌داون «نوع ویزیت»** اضافه شود که از **دسته‌بندی‌های اختصاص‌یافته به همان پزشک** پر شود (مثلاً از سرویس/API که دسته‌بندی‌های آن پزشک را برمی‌گرداند). یکی از گزینه‌ها **«مشاوره آنلاین تصویری»** با `ServiceCategoryId = 7` باشد.
- **مدل:** همان `AppointmentBookingViewModel` که الان `ServiceCategoryId` دارد؛ مقدار انتخاب‌شده از دراپ‌داون در فیلد مخفی یا مستقیم در مدل قرار گیرد و در POST به `Reserve` ارسال شود.
- **مزیت:** یک مرحله اضافه نمی‌شود؛ بیمار در همان صفحه تأیید، نوع ویزیت را انتخاب می‌کند.

### گزینه ب: در صفحه **انتخاب زمان (SelectTime)** قبل از «ادامه به تأیید»

- **صفحه:** `Areas/Patient/Views/AppointmentBooking/SelectTime.cshtml`
- **اسکریپت:** `Scripts/patient/time-selection.js` — تابع `proceedToConfirm` که با `confirmBookingUrl` و `params` لینک ConfirmBooking را می‌سازد.
- **کار:** قبل از رفتن به ConfirmBooking، یک انتخاب «نوع ویزیت» (مثلاً رادیو یا دراپ‌داون) نمایش داده شود؛ در صورت انتخاب «مشاوره آنلاین تصویری»، مقدار `serviceCategoryId=7` به `params` در `URLSearchParams` اضافه شود تا در URL صفحه ConfirmBooking بیاید و در مدل و سپس Reserve استفاده شود.
- **مزیت:** بیمار قبل از دیدن صفحه پرداخت نوع ویزیت را مشخص می‌کند.

---

## ۴. خلاصه مسیر بعد از پیاده‌سازی (وضعیت فعلی)

1. بیمار از **همان مسیر ۴ مرحله‌ای** بالا می‌آید (SelectDoctor → SelectDate → SelectTime → ConfirmBooking).
2. در **صفحه تأیید نهایی (ConfirmBooking)** دراپ‌داون **«نوع ویزیت»** نمایش داده می‌شود (در صورت انتساب دسته‌بندی‌های خدمتی به آن پزشک). بیمار گزینه **«مشاوره آنلاین»** (یا عنوان تعریف‌شده برای دسته با Id=7) را انتخاب می‌کند.
3. مقدار **ServiceCategoryId** انتخاب‌شده در POST **Reserve** ارسال می‌شود.
4. در `AppointmentBookingService` شرط `request.ServiceCategoryId == OnlineConsultationServiceCategoryId` برقرار می‌شود و نوبت با **IsOnlineConsultation = true** ذخیره می‌شود.
5. بعد از پرداخت، لینک ورود به اتاق برای بیمار و پزشک فعال می‌شود (طبق `Docs/ONLINE_CONSULTATION_FLOW.md`).

---

## ۵. فایل‌های مرتبط برای تغییر

| هدف | فایل |
|-----|------|
| نمایش/انتخاب نوع ویزیت در تأیید نهایی | `Areas/Patient/Views/AppointmentBooking/ConfirmBooking.cshtml` |
| پر کردن لیست دسته‌بندی‌های پزشک برای انتخاب | سرویس/API که دسته‌بندی‌های آن پزشک را برگرداند (مثلاً از `DoctorServiceCategory` یا معادل در لایه Patient)؛ در کنترلر `ConfirmBooking` (GET) به View پاس داده شود. |
| ارسال serviceCategoryId از انتخاب زمان به تأیید | `Scripts/patient/time-selection.js` (تابع `proceedToConfirm` و ساخت `params`) |
| صفحه انتخاب زمان (در صورت گزینه ب) | `Areas/Patient/Views/AppointmentBooking/SelectTime.cshtml` |
| ذخیره نوبت با IsOnlineConsultation | `Services/Appointment/AppointmentBookingService.cs` (قبلاً بر اساس `ServiceCategoryId` پیاده‌سازی شده) |

با این مسیرها و فایل‌ها می‌توان دقیقاً مشخص کرد کجا باید انتخاب «مشاوره آنلاین» برای بیمار اضافه شود و مسیر انتخاب بیمار برای مشاوره آنلاین کامل شود.

---

**نقشه راه یکپارچه (مسیرهای Available، DoctorDetails، Book و TODO):** `Docs/PATIENT_APPOINTMENT_ROUTES_ROADMAP.md`
