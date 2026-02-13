# بررسی صفحه انتخاب زمان نوبت (SelectTime)

**URLهای معتبر:**
- `http://localhost:3560/Patient/Appointment/Book/SelectTime/1/2026-02-19` (Route با path)
- `http://localhost:3560/Patient/AppointmentBooking/SelectTime?doctorId=1&date=2026-02-19` (Route پیش‌فرض با query)

**نقش:** گام ۳ از ۴ — انتخاب اسلات زمانی برای پزشک و تاریخ مشخص.

---

## ۱. فلو و معماری

### ۱.۱ Controller — `AppointmentBookingController.SelectTime(int? doctorId, string date = null)`

- **ورودی:** `doctorId` از route یا query؛ `date` از route یا `Request.QueryString["date"]`.
- **اعتبارسنجی:**
  - doctorId مثبت؛ در غیر این صورت → Redirect به SelectDoctor.
  - date خالی → Redirect به SelectDate با همان doctorId.
  - پارس تاریخ: ابتدا Gregorian، در صورت سال < 2000 یا شکست، Persian با `PersianDateHelper.ParsePersianDate`.
  - تاریخ گذشته (مقایسه با `_timeProvider.GetIranToday()`) → Redirect به SelectDate.
  - تاریخ بیش از ۹۰ روز آینده → Redirect به SelectDate.
- **سرویس:**
  - `GetDoctorDetailsAsync(validDoctorId)` — در صورت نبود پزشک → Redirect به SelectDoctor.
  - `GetAvailableTimeSlotsAsync(validDoctorId, parsedDate)` — در صورت خطا → Redirect به SelectDate.
  - `GetAppointmentDurationAsync(validDoctorId)` — در غیر این صورت استفاده از `_appSettings.DefaultAppointmentDurationMinutes`.
- **خروجی:** View با `TimeSlotSelectionViewModel` (DoctorId, DoctorName, SelectedDate, AvailableSlots, AppointmentDuration).

### ۱.۲ View — `SelectTime.cshtml`

- **مدل:** `TimeSlotSelectionViewModel`
- **محتوا:** هدر صفحه، کارت اطلاعات پزشک و تاریخ و آمار در دسترس/رزرو شده، هشدار فوریت (کم بودن اسلات)، گرید اسلات‌ها (`_TimeSlotCard`)، بخش «زمان انتخاب شده»، دکمه ادامه (دسکتاپ و موبایل sticky bar)، فیلدهای پنهان doctorId, selectedDate, selectedStartTime, selectedEndTime، و `__RequestVerificationToken`.
- **اسکریپت:** `appConfig.appointmentBooking.confirmBookingUrl`, `checkSlotAvailabilityUrl`, `getAvailableSlotsUrl` از Razor؛ سپس `time-selection.js`.

### ۱.۳ JS — `time-selection.js`

- **init:** خواندن doctorId و selectedDate از hidden؛ bindEvents؛ startRealTimeUpdates (هر ۱۵ ثانیه)؛ restoreSelection از sessionStorage.
- **رویدادها:** کلیک روی کارت/دکمه اسلات → selectSlot؛ پاک کردن انتخاب؛ ادامه → checkSlotAvailability (POST به CheckSlotAvailability) و در صورت isAvailable → proceedToConfirm (رفتن به Confirm با query).
- **Real-time:** هر ۱۵ ثانیه GetAvailableTimeSlots و به‌روزرسانی وضعیت اسلات‌ها در UI با updateSlotsUI.

---

## ۲. فایل‌های کلیدی

| فایل | نقش |
|------|-----|
| `Areas/Patient/Controllers/AppointmentBookingController.cs` | SelectTime، پارس تاریخ، اعتبارسنجی، فراخوانی سرویس و Factory |
| `Areas/Patient/Views/AppointmentBooking/SelectTime.cshtml` | View، appConfig، لود time-selection.js |
| `Areas/Patient/Views/Shared/_TimeSlotCard.cshtml` | یک اسلات با data-start-time، data-end-time، data-is-available |
| `Scripts/patient/time-selection.js` | انتخاب اسلات، بررسی دسترسی، ادامه به Confirm، به‌روزرسانی دوره‌ای |
| `ViewModels/Patient/TimeSlotSelectionViewModel.cs` | DoctorId, DoctorName, SelectedDate, AvailableSlots, AppointmentDuration |
| `Models/DTOs/Appointment/AvailableTimeSlotDto.cs` | StartTime, EndTime, IsAvailable, DisplayTime, DisplayRange, Duration |
| `Services/Appointment/AppointmentBookingService.cs` | GetAvailableTimeSlotsAsync، CheckSlotAvailabilityAsync |
| `Areas/Patient/Controllers/Api/DoctorSearchApiController.cs` | GetAvailableTimeSlots، CheckSlotAvailability |

---

## ۳. تغییرات اعمال‌شده

- **time-selection.js**
  - **لودینگ:** اضافه شدن `_showLoading` و `_hideLoading` با fallback در صورت نبود توابع سراسری؛ استفاده از آن‌ها در checkSlotAvailability به‌جای مستقیم showLoading/hideLoading.
  - **updateSlotsUI:** پشتیبانی از هر دو نام property (camelCase و PascalCase) برای startTime و isAvailable؛ نرمال‌سازی زمان با `_normalizeTime` (برش به hh:mm) تا تطابق با `data-start-time` کارت سرور (که با `ToString(@"hh\:mm")` تولید می‌شود) و با فرمت احتمالی پاسخ API (مثلاً "09:30:00") حفظ شود.

---

## ۴. نکات امنیت و رفتار

- **احراز هویت:** در حال حاضر موقتاً غیرفعال (AllowAnonymous در مراحل قبل؛ در SelectTime هم کامنت شده).
- **ورودی:** doctorId و date اعتبارسنجی می‌شوند؛ تاریخ با زمان ایران و محدوده ۹۰ روز.
- **CheckSlotAvailability:** POST بدون ارسال توکن ضد جعل (طبق طراحی برای AllowAnonymous)؛ در صورت نیاز می‌توان بعداً محدودیت نرخ یا احراز هویت اضافه کرد.

---

## ۵. URL و Route

- **Route نام‌دار:** `Patient/Appointment/Book/SelectTime/{doctorId}/{date}` با constraint تاریخ `yyyy-MM-dd`.
- **Route پیش‌فرض:** `Patient/{controller}/{action}/{id}` با controller=AppointmentBooking → URL به صورت `Patient/AppointmentBooking/SelectTime?doctorId=1&date=2026-02-19` نیز کار می‌کند و Controller از QueryString تاریخ را می‌خواند.
- **توصیه:** برای یکپارچگی و سئو ترجیح با URL دارای path است: `/Patient/Appointment/Book/SelectTime/1/2026-02-19`.

---

## ۶. چک‌لیست تست

- [ ] بارگذاری با `/Patient/Appointment/Book/SelectTime/1/2026-02-19` و با `?doctorId=1&date=2026-02-19` — هر دو صفحه را با لیست اسلات نمایش دهند.
- [ ] انتخاب یک اسلات در دسترس — نمایش زمان انتخاب‌شده و فعال شدن دکمه ادامه.
- [ ] کلیک ادامه — درخواست CheckSlotAvailability و در صورت موفق، انتقال به Confirm با پارامترهای صحیح.
- [ ] پس از به‌روزرسانی دوره‌ای (۱۵ ثانیه)، اسلاتی که رزرو شده به‌درستی به «رزرو شده» تغییر کند (و انتخاب قبلی در صورت رزرو شدن پاک شود در restoreSelection).
- [ ] در صورت قطع شبکه یا خطای سرور در CheckSlotAvailability — پیام خطا و فعال شدن مجدد دکمه ادامه.

---

## ۷. جمع‌بندی

صفحه SelectTime با اعتبارسنجی قوی، استفاده از سرویس و Factory، و پشتیبانی از هر دو URL (path و query) پیاده‌سازی شده است. با اصلاحات لودینگ و تطابق زمان و نام property در JS، رفتار Real-time و ادامه به Confirm پایدارتر شده است.
