# تحلیل عمیق مرحله ۲.۲ — کوئری اختصاصی آمار داشبورد (Real-Time، بدون کش)

## اصول طراحی (طبق درخواست)
- **بدون کش:** هیچ لایه کش (OutputCache، HttpContext، MemoryCache) برای آمار داشبورد استفاده نشود.
- **Real-Time:** هر درخواست آمار را مستقیم و تازه از دیتابیس بگیرد.
- **چابک و مقیاس‌پذیر:** داشبورد باید برای هزاران بیمار قابل استفاده باشد؛ هر بیمار فقط با حداقل کوئریِ سبک سرو شود.

---

## مشکل فعلی
- `GetQuickStatsAsync` برای هر بیمار **همه نوبت‌ها** را با `GetPatientAppointmentsAsync(patientId)` از DB می‌گیرد (با Includeهای Doctor, Specializations, PaymentTransaction).
- بعد در حافظه با LINQ تعداد کل، آینده، تکمیل‌شده و لغوشده را می‌شمارد.
- برای بیمار با صدها نوبت: بار زیاد روی DB، مصرف حافظه، و زمان پاسخ بالاتر.
- تعداد پذیرش‌ها فعلاً ۰ است (FIXME).

---

## استراتژی پیشنهادی

### ۱. فقط شمارش، بدون بارگذاری موجودیت
- **نوبت‌ها:** یک (یا حداقل) کوئری که فقط **COUNT** برمی‌گرداند: کل، آینده (تاریخ > اکنون و وضعیت ≠ لغوشده)، تکمیل‌شده، لغوشده.
- **پذیرش‌ها:** یک کوئری **COUNT** برای همان بیمار.
- بدون `Include`، بدون بارگذاری ردیف‌های کامل؛ فقط اعداد.

### ۲. بدون کش
- هیچ `[OutputCache]` یا کش درخواستی/سرور برای آمار داشبورد نگذاریم.
- هر بار با یک یا دو کوئری سبک، آمار لحظه‌ای برگردد.

### ۳. بهینه‌سازی دیتابیس
- کوئریها روی ستون‌های ایندکس‌دار (مثلاً `PatientId`, `IsDeleted`, `Status`, `AppointmentDate`) باشند.
- ترجیح: یک کوئری با تجمع شرطی (conditional aggregation) برای نوبت‌ها تا یک round-trip؛ در غیر این صورت چند `CountAsync` جدا با فیلترهای ساده و ایندکس‌پذیر.

---

## طراحی فنی

### لایه رپازیتوری (نوبت)
- **IAppointmentRepository:** متد جدید  
  `Task<PatientAppointmentCountsDto> GetPatientAppointmentCountsAsync(int patientId, DateTime asOf)`  
  خروجی: `Total`, `Upcoming`, `Completed`, `Cancelled`.
- **AppointmentRepository:** پیاده‌سازی با یک کوئری EF (مثلاً `GroupBy` ثابت + `Select` با `Count` شرطی) یا در صورت نیاز با raw SQL سبک؛ بدون بارگذاری موجودیت.

### لایه سرویس بیمار (پذیرش)
- **IPatientService:** متد جدید  
  `Task<int> GetPatientReceptionCountAsync(int patientId)`  
  فقط `COUNT` از جدول پذیرش برای آن بیمار (و در صورت وجود فیلتر حذف نرم).
- **PatientService:** پیاده‌سازی با `_context.Receptions.CountAsync(...)` بدون Include.

### لایه سرویس داشبورد
- **PatientDashboardService.GetQuickStatsAsync:**
  - وابستگی به **IAppointmentRepository** (تزریق از DI).
  - فراخوانی `GetPatientAppointmentCountsAsync(patientId, DateTime.Now)` و `GetPatientReceptionCountAsync(patientId)`.
  - ساخت `DashboardQuickStatsViewModel` از همین دو خروجی.
  - حذف کامل فراخوانی `GetPatientAppointmentsAsync` و هرگونه بارگذاری لیست نوبت برای آمار.

### DTO
- **PatientAppointmentCountsDto** (مثلاً در `Models.DTOs.Appointment`):  
  `Total`, `Upcoming`, `Completed`, `Cancelled` (همه int).

---

## گام‌های اتمیک پیاده‌سازی
1. اضافه کردن **PatientAppointmentCountsDto** و متد **GetPatientAppointmentCountsAsync** به اینترفیس و رپازیتوری نوبت.
2. پیاده‌سازی در **AppointmentRepository** با یک کوئری تجمعی (یا چند CountAsync سبک).
3. اضافه کردن **GetPatientReceptionCountAsync** به **IPatientService** و **PatientService**.
4. تزریق **IAppointmentRepository** در **PatientDashboardService** و بازنویسی **GetQuickStatsAsync** فقط با COUNTها؛ حذف وابستگی به بارگذاری لیست نوبت برای آمار.
5. تست: یک بیمار با تعداد زیاد نوبت؛ بررسی عدم بارگذاری لیست و فقط اجرای کوئری‌های سبک.

---

## چک‌لیست نهایی
- [ ] هیچ کشی برای آمار داشبورد استفاده نشود.
- [ ] هیچ بارگذاری لیست نوبت/پذیرش برای محاسبه آمار نباشد.
- [ ] آمار نوبت از یک (یا حداقل) کوئری سبک COUNT-based.
- [ ] تعداد پذیرش‌ها از یک CountAsync جدا و بدون Include.
- [ ] مقیاس‌پذیری برای هزاران بیمار با پاسخ‌زمان قابل قبول.
