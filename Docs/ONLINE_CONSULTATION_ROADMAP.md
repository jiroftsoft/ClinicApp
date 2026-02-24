# نقشه راه و TODO — ماژول مشاوره آنلاین تصویری

طبق قرارداد توسعه و 04-TODO-Implementation-Guide. شکست ماژول = صفر.

---

## Phase 1 — تحلیل و طراحی (قبل از کد)

- [ ] **۱.۱** تعیین معیار «نوبت مشاوره آنلاین»: فیلد `AppointmentTypeId` یا `IsOnlineConsultation` روی `Appointment`؛ یا سرویس/دسته‌بندی مشخص. در صورت نبودن در Entity، Migration اضافه شود.
- [ ] **۱.۲** طراحی جدول `OnlineConsultationRooms`: `RoomId`, `AppointmentId` (FK), `RoomName` (یکتا برای Jitsi), `StartedAt`, `EndedAt`, `CreatedAt`. ایندکس روی `AppointmentId`.
- [ ] **۱.۳** طراحی ViewModelها: `JoinConsultationViewModel` (RoomName, AppointmentId, PatientName, DoctorName, JitsiBaseUrl), `PendingOnlineConsultationItemViewModel` برای داشبورد پزشک.
- [ ] **۱.۴** طراحی Interfaceها: `IOnlineConsultationRoomRepository`, `IOnlineConsultationService`. سرویس: ایجاد/بازیابی اتاق، تولید نام یکتا، بررسی صلاحیت (بیمار/پزشک همان نوبت).

---

## Phase 2 — بک‌اند (بدون تغییر در جریان پرداخت فعلی)

- [ ] **۲.۱** Entity `OnlineConsultationRoom` + Config در پوشه Entities. رعایت ISoftDelete/ITrackable در صورت الزام پروژه.
- [ ] **۲.۲** Migration فقط برای جدول جدید (و در صورت نیاز فیلد روی Appointment). اجرا و تست روی DB.
- [ ] **۲.۳** `IOnlineConsultationRoomRepository` + `OnlineConsultationRoomRepository`: GetByAppointmentIdAsync, GetOrCreateForAppointmentAsync. بدون دسترسی مستقیم از Controller.
- [ ] **۲.۴** `IOnlineConsultationService` + `OnlineConsultationService`: وابستگی به IOnlineConsultationRoomRepository, ITimeProvider؛ متد GetOrCreateRoomAsync(appointmentId, userId) با بررسی دسترسی و نوع نوبت؛ نام اتاق یکتا (مثلاً `ClinicApp-Consult-{AppointmentId}-{shortGuid}`). خروجی ViewModel برای Join.
- [ ] **۲.۵** ثبت در UnityConfig: Repository و Service با PerRequestLifetimeManager.

---

## Phase 3 — پس از پرداخت: SMS به پزشک + صف اعلان

- [x] **۳.۱** تعیین نقطه فراخوانی بعد از پرداخت موفق نوبت (مثلاً بعد از ConfirmBooking یا در PaymentSuccess). فقط برای نوبت‌های نوع «مشاوره آنلاین».
- [x] **۳.۲** گسترش صف اعلان: نوع جدید مثلاً `OnlineConsultationRequestToDoctor`؛ یا استفاده از قالب SMS موجود با پارامتر لینک اتاق. ارسال SMS به شماره پزشک با متن «درخواست مشاوره آنلاین از [نام بیمار]. لینک ورود: {JoinUrl}».
- [x] **۳.۳** سرویس اعلان: متد EnqueueOnlineConsultationRequestToDoctorAsync(appointmentId) که لینک Join را از سرویس اتاق بگیرد و در صف قرار دهد. فراخوانی این متد فقط از همان نقطهٔ پس از پرداخت موفق (بدون تغییر در منطق پرداخت).

---

## Phase 4 — داشبورد پزشک: نوتیف و ورود به اتاق

- [x] **۴.۱** در `IDoctorDashboardService`: متد GetPendingOnlineConsultationsAsync(doctorId) که نوبت‌های پرداخت‌شدهٔ نوع مشاوره آنلاین را برگرداند که هنوز اتاق بسته نشده یا در بازه مجاز هستند.
- [x] **۴.۲** در `DoctorDashboardIndexViewModel`: لیست `PendingOnlineConsultations` (مثلاً `List<PendingOnlineConsultationItemViewModel>`).
- [x] **۴.۳** در `DoctorDashboardService.GetDashboardDataAsync`: پر کردن `PendingOnlineConsultations` از همان متد جدید. فقط خواندن؛ بدون تغییر در سایر ویوها.
- [x] **۴.۴** در View داشبورد پزشک (مثلاً Index): یک بلوک/کارت «درخواست‌های مشاوره آنلاین» با لیست و دکمه «ورود به اتاق» که به `Admin/OnlineConsultation/Join/{appointmentId}` یا معادل Patient برای پزشک لینک دهد. استفاده از رنگ‌های مجاز (--medical-*) و بدون گرادینت ممنوع.

---

## Phase 5 — ورود به اتاق (Patient + Admin)

- [x] **۵.۱** Patient: کنترلر `Areas/Patient/Controllers/OnlineConsultationController.cs` با اکشن `Join(appointmentId)`. [Authorize]; فراخوانی سرویس؛ بررسی PatientId == نوبت. برگرداندن View با ViewModel (RoomName, JitsiBaseUrl از AppSettings).
- [x] **۵.۲** Admin (پزشک): کنترلر `Areas/Admin/Controllers/OnlineConsultationController.cs` با اکشن `Join(appointmentId)`. [Authorize] نقش پزشک یا بررسی DoctorId. همان سرویس؛ همان View.
- [x] **۵.۳** یک View مشترک (مثلاً Shared یا در هر Area): صفحه فقط شامل iframe/اسکریپت Jitsi با نام اتاق از ViewModel. پالت --medical-*؛ بدون گرادینت.
- [x] **۵.۴** AppSettings: کلید `Jitsi:BaseUrl` (پیش‌فرض meet.jit.si یا آدرس self-hosted). خواندن در سرویس/کنترلر و پر کردن ViewModel.
- [x] **۵.۵** Route در PatientAreaRegistration و Admin: مثلاً `Patient/Consultation/Join/{appointmentId}` و `Admin/OnlineConsultation/Join/{appointmentId}` با constraint عددی و UseNamespaceFallback = false.

---

## Phase 6 — لینک در «نوبت‌های من» (بیمار)

- [x] **۶.۱** در صفحه/لیست نوبت‌های بیمار، برای نوبت‌های نوع مشاوره آنلاین و پرداخت‌شده، لینک «ورود به مشاوره تصویری» که به `Patient/Consultation/Join/{appointmentId}` برود.
- [x] **۶.۲** بدون تغییر در منطق رزرو یا پرداخت؛ فقط اضافه کردن شرط و لینک در View.

---

## Phase 7 — تست و رعایت قرارداد

- [ ] **۷.۱** تست: رزرو نوبت مشاوره → پرداخت → دریافت SMS توسط پزشک؛ نوتیف در داشبورد پزشک؛ ورود بیمار و پزشک به همان اتاق Jitsi.
- [ ] **۷.۲** رعایت: Controller بدون DbContext؛ همه دادهٔ View از ViewModel؛ NotificationHelper برای پیام‌ها؛ ITimeProvider در سرویس برای زمان؛ تاریخ در DB به UTC.

---

## پروداکشن و سخت‌سازی

- [x] **Feature Flag** `Jitsi:EnableOnlineConsultation`؛ خاموش کردن ماژول بدون تغییر کد.
- [x] **بازه ورود** `JoinAllowedMinutesBefore` / `JoinAllowedMinutesAfter`؛ ورود فقط در بازه مجاز.
- [x] **اعتبارسنجی** شناسه نوبت و عدم افشای جزئیات در پاسخ/لاگ.
- [x] **صفحه خطا و توضیح حریم خصوصی** در View؛ لینک بازگشت به نوبت‌های من.
- [x] **فیلتر ۷ روز** برای لیست داشبورد پزشک.
- [x] **چک‌لیست پروداکشن:** `Docs/ONLINE_CONSULTATION_PRODUCTION.md`

---

## وابستگی‌ها (رعایت ترتیب)

- ۲ وابسته به ۱
- ۳ وابسته به ۲ و ۵ (لینک Join باید وجود داشته باشد تا در SMS قرار گیرد؛ می‌توان مرحله ۳ را بعد از ۵ انجام داد)
- ۴ وابسته به ۲
- ۵ وابسته به ۲
- ۶ وابسته به ۵

**ترتیب پیشنهادی:** 1 → 2 → 5 → 3 → 4 → 6 → 7.

---

## ریسک‌ها و جلوگیری از شکست

- **ماژول نوبت/پرداخت:** فقط نقطهٔ واحد بعد از پرداخت موفق را گسترش دهید؛ بدون تغییر در ConfirmBooking یا جریان پرداخت.
- **داشبورد پزشک:** فقط یک متد و یک لیست جدید؛ بدون حذف یا تغییر فیلدهای موجود در ViewModel.
- **Jitsi:** فقط URL و نام اتاق از بک‌اند؛ بدون قرار دادن توکن/کلید در فرانت. در صورت self-host، تنظیم CORS و HTTPS روی سرور Jitsi.
