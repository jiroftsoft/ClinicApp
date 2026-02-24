# نقشه راه و TODO — مسیرهای رزرو نوبت بیمار (Patient Appointment Routes)

این سند بر اساس بررسی دقیق کد و بدون حدس نوشته شده است. سه مسیر درخواستی و همگرایی آن‌ها با جریان رزرو و انتخاب نوع ویزیت (مشاوره آنلاین) در آن مشخص شده است.

---

## ۱. جدول مسیرها (Routing)

| URL (نمونه) | کنترلر | اکشن | روت ثبت‌شده | View |
|-------------|--------|------|-------------|------|
| `/Patient/Appointment/Available` | `AppointmentController` | `Available` | `Patient_default`: `Patient/{controller}/{action}/{id}` | `Areas/Patient/Views/Appointment/Available.cshtml` |
| `/Patient/Appointment/DoctorDetails?doctorId=2` | `AppointmentController` | `DoctorDetails` | همان `Patient_default` | `Areas/Patient/Views/Appointment/DoctorDetails.cshtml` |
| `/Patient/Appointment/Book/SelectDoctor` | `AppointmentBookingController` | `SelectDoctor` | `Patient_AppointmentBooking_SelectDoctor`: `Patient/Appointment/Book/SelectDoctor/{departmentId}` | `Areas/Patient/Views/AppointmentBooking/SelectDoctor.cshtml` |
| `/Patient/Appointment/Book/SelectDate/2` | `AppointmentBookingController` | `SelectDate` | `Patient_AppointmentBooking_SelectDate`: `Patient/Appointment/Book/SelectDate/{doctorId}` | `Areas/Patient/Views/AppointmentBooking/SelectDate.cshtml` |
| `/Patient/Appointment/Book/SelectTime/2/2025-02-17` | `AppointmentBookingController` | `SelectTime` | `Patient_AppointmentBooking_SelectTime` | `Areas/Patient/Views/AppointmentBooking/SelectTime.cshtml` |
| `/Patient/Appointment/Book/Confirm?doctorId=...&...` | `AppointmentBookingController` | `ConfirmBooking` (GET) | `Patient_AppointmentBooking_Confirm` | `Areas/Patient/Views/AppointmentBooking/ConfirmBooking.cshtml` |
| POST رزرو | `AppointmentBookingController` | `Reserve` (POST) | همان کنترلر، از فرم ConfirmBooking | — |
| `/Patient/Appointment/Book` | `AppointmentBookingController` | `Book` | `Patient_default` (action=Book) | فقط Redirect به SelectDoctor |

**منبع روت‌ها:** `Areas/Patient/PatientAreaRegistration.cs` — روت‌های صریح Book قبل از `Patient_default` ثبت شده‌اند؛ `Available` و `DoctorDetails` فقط با `Patient_default` با `controller=Appointment` match می‌شوند.

---

## ۲. دو نقطه ورود به جریان رزرو

### ۲.۱ ورود از «نوبت‌های موجود» (AppointmentController)

- **شروع:** `GET /Patient/Appointment/Available`  
  لیست پزشکان، تاریخ‌های موجود، و در صورت انتخاب پزشک+تاریخ، اسلات‌های زمانی نمایش داده می‌شود.
- **لینک به جزئیات پزشک:**  
  `Url.Action("DoctorDetails", "Appointment", new { area = "Patient" })` با `doctorId` در کلیک (در Available.cshtml خط ~339: `DoctorDetails` با `doctorId = doctor.DoctorId`).
- **ادامه رزرو از Available:**  
  اگر کاربر پزشک و تاریخ را انتخاب کرده و روی «رزرو نوبت» کلیک کند:
  - در صورت وجود `dateForRoute` (تاریخ شمسی تبدیل‌شده به yyyy-MM-dd): لینک به **SelectTime**:  
    `Url.Action("SelectTime", "AppointmentBooking", new { area = "Patient", doctorId = Model.SelectedDoctorId, date = dateForRoute })`
  - در غیر این صورت: لینک به **SelectDate**:  
    `Url.Action("SelectDate", "AppointmentBooking", new { area = "Patient", doctorId = Model.SelectedDoctorId })`
- **Layout این بخش:** `_PatientLayout.cshtml`. لینک «رزرو نوبت» در منو: `Url.Action("Book", "AppointmentBooking", new { area = "Patient" })` → به `Book()` می‌رود و آن به `SelectDoctor` redirect می‌کند.

### ۲.۲ ورود از «جزئیات پزشک» (AppointmentController)

- **شروع:** `GET /Patient/Appointment/DoctorDetails?doctorId=2` (و اختیاری `selectedDate`).
- **ادامه رزرو:** در DoctorDetails کاربر تاریخ و زمان را در همان صفحه انتخاب می‌کند. روی «رزرو نوبت» (`#bookAppointmentBtn`):
  - فعلاً به **SelectDate** هدایت می‌شود با query:  
    `doctorId`, `date`, `time`  
    یعنی:  
    `Url.Action("SelectDate", "AppointmentBooking")?doctorId=...&date=...&time=...`
- **مشکل:** اکشن `SelectDate(int doctorId)` فقط `doctorId` را از route می‌گیرد؛ پارامترهای **date** و **time** در query **استفاده نمی‌شوند**. بنابراین کاربر پس از رسیدن به SelectDate مجدداً باید تاریخ (و سپس در SelectTime زمان) را انتخاب کند؛ یعنی یک بار انتخاب تاریخ/زمان در DoctorDetails عملاً نادیده گرفته می‌شود.

### ۲.۳ ورود از «رزرو نوبت جدید» (Book Flow)

- **شروع:** از منو (مثلاً در _PatientLayoutPro یا _PatientLayout) با لینک به `Book` یا مستقیم `SelectDoctor`:  
  `GET /Patient/Appointment/Book/SelectDoctor` (اختیاری: `departmentId`, `searchTerm`).
- **جریان:** SelectDoctor → SelectDate → SelectTime → ConfirmBooking (GET) → Reserve (POST).
- **Layout این مراحل:** `_PatientLayoutPro.cshtml` (در Viewهای AppointmentBooking).

### ۲.۴ همگرایی

هر سه مسیر در نهایت به **AppointmentBookingController** می‌رسند:
- **SelectDate** و **SelectTime** و **ConfirmBooking** و **Reserve** فقط در این کنترلر هستند.
- سرویس مشترک: `IAppointmentBookingService` (مثلاً `GetAvailableDoctorsAsync`, `GetAvailableTimeSlotsAsync`, `GetAppointmentPriceAsync`, `ReserveAppointmentAsync`).
- **نوع ویزیت (ServiceCategoryId)** امروز در هیچ‌کدام از Viewها توسط کاربر انتخاب نمی‌شود و در اسکریپت `time-selection.js` به ConfirmBooking ارسال نمی‌شود؛ در نتیجه در Reserve همیشه `ServiceCategoryId` خالی است و نوبت به‌صورت مشاوره آنلاین ثبت نمی‌شود.

---

## ۳. وابستگی‌ها و ریسک تغییر

- **سرویس‌ها:** هر دو کنترلر از `IAppointmentBookingService` و در Appointment از `IDoctorCrudService`, `IDoctorScheduleRepository`, `IDoctorMappingService` استفاده می‌کنند. تغییر در قرارداد این اینترفیسها روی هر دو مسیر اثر می‌گذارد.
- **APIهای فرانت:** در Available از `GetAvailableData` (AppointmentController) و در SelectTime از `DoctorSearchApi` (CheckSlotAvailability, GetAvailableTimeSlots) استفاده می‌شود. تغییر در پارامتر یا خروجی این APIها باید در هر دو طرف (View/JS و کنترلر) هم‌خوان باشد.
- **احراز هویت:** Available و DoctorDetails با `[AllowAnonymous]`؛ SelectDoctor و SelectDate هم موقتاً AllowAnonymous؛ ConfirmBooking و Reserve برای رزرو واقعی به بیمار لاگین‌شده وابسته‌اند. هر تغییری در شرط لاگین فقط باید در یک نقطه (مثلاً یک Filter یا متد کمکی) اعمال شود تا رفتار در هر دو ورود یکسان بماند.
- **لینکهای خارجی:** اگر جای دیگری (ایمیل، اعلان، یا ماژول دیگر) به `/Patient/Appointment/Available` یا `/Patient/Appointment/DoctorDetails` یا `/Patient/Appointment/Book/SelectDoctor` لینک داده باشد، تغییر مسیر یا نام اکشن بدون به‌روزرسانی آن لینکها باعث شکست می‌شود. پیشنهاد: برای این سه مسیر از `Url.Action` یا نام روت استفاده شود و از hardcode URL پرهیز شود.

---

## ۴. نقشه راه (پیشنهادی، بدون تغییر غیرضروری در ماژول‌های دیگر)

### فاز ۱ — یکپارچه‌سازی و بهینه‌سازی مسیر (بدون شکستن جریان فعلی)

| ردیف | کار | دلیل | ریسک برای ماژول‌های دیگر |
|------|-----|------|---------------------------|
| ۱.۱ | **DoctorDetails → SelectTime به‌جای SelectDate وقتی date (و ترجیحاً time) مشخص است:** در DoctorDetails.cshtml به‌جای لینک به SelectDate با query، اگر `selectedDate` معتبر است به **SelectTime** لینک داده شود با `doctorId` و `date` (فرمت yyyy-MM-dd). در صورت امکان، اگر زمان هم انتخاب شده با اسلات مشخص است، همان زمان در لینک SelectTime یا در مرحله بعد (Confirm) استفاده شود تا UX یکپارچه شود. | استفاده از انتخاب کاربر و جلوگیری از انتخاب مجدد تاریخ در SelectDate. | فقط View و یک خط JS در DoctorDetails؛ کنترلر SelectDate و SelectTime بدون تغییر امضا. |
| ۱.۲ | **ثبت روت صریح برای Available و DoctorDetails (اختیاری):** در `PatientAreaRegistration` قبل از `Patient_default` دو روت با نام مثلاً `Patient_Appointment_Available` و `Patient_Appointment_DoctorDetails` با URLهای `Patient/Appointment/Available` و `Patient/Appointment/DoctorDetails/{doctorId}` تا وابستگی به ترتیب و constraint کنترلر در default کم شود. | پایداری URL و امکان ارجاع با نام روت. | کم؛ فقط اضافه کردن روت؛ رفتار فعلی همان است. |

### فاز ۲ — انتخاب نوع ویزیت (مشاوره آنلاین)

| ردیف | کار | دلیل | ریسک |
|------|-----|------|------|
| ۲.۱ | **دریافت دسته‌بندی‌های خدمتی پزشک برای بیمار:** در لایه سرویس/API یک متد (مثلاً از طریق IAppointmentBookingService یا سرویس جدا) که برای یک `doctorId` فقط دسته‌بندی‌های اختصاص‌یافته به آن پزشک (DoctorServiceCategory فعال) را به‌صورت لیست ساده (Id, Title) برگرداند. این داده فقط برای نمایش در dropdown استفاده شود؛ نیازی به افشای منطق داخلی نیست. | منبع واحد برای گزینه‌های «نوع ویزیت» در هر دو مسیر (Available/DoctorDetails و Book). | اگر از همان لایه DoctorServiceCategory استفاده شود، فقط خواندن است؛ ایجاد/حذف انتساب در ادمین تغییری نمی‌کند. |
| ۲.۲ | **صفحه ConfirmBooking: اضافه کردن dropdown «نوع ویزیت»:** در GET ConfirmBooking لیست دسته‌بندی‌های آن پزشک را از متد بالا بگیرید و به View پاس دهید. در `ConfirmBooking.cshtml` یک `<select>` برای `ServiceCategoryId` با گزینه‌های «ویزیت حضوری» (در صورت نیاز مقدار خالی یا یک دسته پیش‌فرض) و «مشاوره آنلاین تصویری» (شناسه ۷ یا مقدار از تنظیمات) و در صورت وجود دسته‌های دیگر همان پزشک، آن‌ها را هم اضافه کنید. مقدار انتخاب‌شده در همان فرم Reserve ارسال شود (همان فیلد مخفی یا به‌جای آن binding مستقیم به مدل). | یک نقطه واحد برای انتخاب نوع ویزیت بدون توجه به ورود از Available، DoctorDetails یا SelectDoctor؛ نوبت با ServiceCategoryId=7 به‌صورت مشاوره آنلاین ذخیره می‌شود. | فقط یک View و یک اکشن GET؛ Reserve و AppointmentBookingService از قبل ServiceCategoryId را می‌پذیرند. |
| ۲.۳ | **اختیاری — SelectTime:** در صورت نیاز می‌توان در SelectTime یک انتخاب نوع ویزیت (رادیو یا dropdown) اضافه کرد و در `time-selection.js` هنگام ساخت URL ConfirmBooking پارامتر `serviceCategoryId` را به query اضافه کرد تا در ConfirmBooking از قبل مقدار داشته باشد. | کاهش یک کلیک در صفحه تأیید. | فقط View SelectTime و اسکریپت؛ منطق نهایی همچنان در Confirm/Reserve است. |

### فاز ۳ — مستندسازی و پایداری لینک‌ها

| ردیف | کار | دلیل |
|------|-----|------|
| ۳.۱ | در تمام Viewهای Patient که به Available، DoctorDetails یا SelectDoctor لینک می‌دهند از `Url.Action("Available", "Appointment", new { area = "Patient" })` و مشابه برای DoctorDetails و SelectDoctor استفاده شود و از hardcode مسیر `/Patient/...` پرهیز شود. | جلوگیری از شکست در صورت تغییر روت یا area. |
| ۳.۲ | به‌روزرسانی سند `Docs/ONLINE_CONSULTATION_PATIENT_PATH.md` پس از انجام فاز ۲ با مسیر نهایی و نام فایل/اکشنهایی که انتخاب نوع ویزیت را انجام می‌دهند. | هماهنگی با مستند مشاوره آنلاین. |

---

## ۵. TODO لیست (اقدام‌های قابل پیگیری)

- [ ] **۱.۱** در `DoctorDetails.cshtml` منطق دکمه «رزرو نوبت» را طوری تغییر دهید که در صورت وجود `selectedDate` معتبر به `SelectTime(doctorId, date)` هدایت شود، نه به `SelectDate(doctorId)`؛ در صورت نیاز فرمت `date` را به yyyy-MM-dd یکسان کنید.
- [ ] **۱.۲** (اختیاری) در `PatientAreaRegistration.cs` روت‌های صریح برای `Available` و `DoctorDetails` اضافه کنید و در لینک‌ها از نام روت استفاده کنید.
- [ ] **۲.۱** متد/API «دسته‌بندی‌های خدمتی یک پزشک» را در لایه سرویس یا API بیمار اضافه کنید و از DoctorServiceCategory/سرویس ادمین فقط خواندن کنید.
- [ ] **۲.۲** در GET `ConfirmBooking` لیست دسته‌بندی‌های آن پزشک را بگیرید و به View پاس دهید؛ در `ConfirmBooking.cshtml` dropdown نوع ویزیت (شامل مشاوره آنلاین با شناسه ۷) را اضافه کنید و مقدار را به فرم Reserve ببندید.
- [ ] **۲.۳** (اختیاری) در `SelectTime.cshtml` و `time-selection.js` انتخاب نوع ویزیت و ارسال `serviceCategoryId` در URL ConfirmBooking را اضافه کنید.
- [x] **۳.۱** در Viewهای Patient از `Url.Action`/`Url.RouteUrl` و در اسکریپت‌ها از `appConfig` (پر شده از View) استفاده شود؛ hardcode مسیر حذف یا به fallback حداقلی تقلیل یافت. انجام‌شده: _DoctorCard، SelectDoctor، ConfirmBooking، doctor-selection.js، confirm-booking.js.
- [ ] **۳.۲** به‌روزرسانی `ONLINE_CONSULTATION_PATIENT_PATH.md` پس از اتمام فاز ۲.

---

## ۶. خلاصه

- **مسیرها:** `/Patient/Appointment/Available` و `/Patient/Appointment/DoctorDetails` روی `AppointmentController`؛ `/Patient/Appointment/Book/SelectDoctor` و مراحل بعدی روی `AppointmentBookingController`. هر دو جریان در SelectDate/SelectTime/Confirm/Reserve یکی می‌شوند.
- **مشکل فعلی DoctorDetails:** ارسال به SelectDate با `date` و `time` در query که در SelectDate استفاده نمی‌شود؛ پیشنهاد هدایت مستقیم به SelectTime با `doctorId` و `date`.
- **مشکل انتخاب مشاوره آنلاین:** در هیچ مرحله‌ای `ServiceCategoryId` توسط کاربر تنظیم نمی‌شود؛ پیشنهاد اضافه کردن انتخاب در ConfirmBooking (و اختیاری در SelectTime) و استفاده از همان مدل و Reserve فعلی.
- با رعایت فازها و TODOها، تغییرات فقط در محدوده Patient و یک نقطه سرویس (خواندن دسته‌بندی پزشک) انجام می‌شود و به ماژول‌های ادمین، پرداخت، یا مشاوره آنلاین (Join/SMS) صدمه نمی‌زند؛ فقط با ارسال صحیح `ServiceCategoryId` نوبت به‌صورت مشاوره آنلاین ثبت می‌شود.
