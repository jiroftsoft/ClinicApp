# نقشه راه تب «خانه» — داشبورد بیمار

**هدف:** بررسی کامل و بهینه‌سازی حرفه‌ای تب خانه در پنل بیمار؛ مدیریت جامع اطلاعات پزشکی و نوبت‌ها.

**مرجع:** `PATIENT_DASHBOARD_ROADMAP.md` — فاز ۳.۳ (GetOverview).

---

## ۱. معماری فعلی تب خانه

| بخش | منبع داده | API (در حالت useOverview) | View / Container |
|-----|-----------|---------------------------|-------------------|
| آمار سریع | `GetQuickStatsAsync` | یک درخواست `GetOverview` | `#quickStatsContainer` |
| نوبت‌های اخیر | `GetRecentAppointmentsAsync` → `PatientService.GetPatientAppointmentsPagedAsync` | همان | `[data-dashboard-section="recentAppointments"]` |
| نوبت‌های آینده | `GetUpcomingAppointmentsAsync` → `GetPatientUpcomingAppointmentsPagedAsync` | همان | `[data-dashboard-section="upcomingAppointments"]` |
| پذیرش‌های اخیر | `GetRecentReceptionsAsync` → `PatientService.GetPatientReceptionsAsync` | همان | `[data-dashboard-section="recentReceptions"]` |

- **یک درخواست:** `GET /Patient/Api/PatientDashboard/GetOverview` — همهٔ داده‌ها در یک پاسخ.
- **سرویس:** `PatientDashboardService.GetOverviewAsync` با `Task.WhenAll` برای چهار تسک؛ در صورت شکست **یکی** از تسک‌ها، قبلاً کل Overview با `Failed` برمی‌گشت و در UI هر سه سکشن نوبت/پذیرش پیام خطا می‌دادند.

---

## ۲. تغییرات انجام‌شده (مقاوم‌سازی)

### ۲.۱ Backend

- **`DashboardViewModel`:** پراپرتی `SectionErrors` (نوع `Dictionary<string, string>`) اضافه شد. کلیدها: `QuickStats`, `RecentAppointments`, `UpcomingAppointments`, `RecentReceptions`.
- **`PatientDashboardService.GetOverviewAsync`:**
  - دیگر با شکست **یک** تسک، کل نتیجه `Failed` برنمی‌گردد.
  - برای هر تسک: در صورت موفقیت، داده در ViewModel قرار می‌گیرد؛ در صورت شکست، آن بخش `null` و پیام خطا در `SectionErrors` ثبت می‌شود.
  - همیشه `Successful(overview)` برگردانده می‌شود (به‌جز خطای کلی مثل exception در خود GetOverview).

### ۲.۲ Frontend

- **`patient-dashboard.js` — `loadOverview`:**
  - پس از دریافت `response.data`، `SectionErrors` (یا `sectionErrors`) خوانده می‌شود.
  - برای هر سکشن: اگر برای آن کلید خطا وجود داشت، ابتدا `hideLoading` و سپس `showError` با همان پیام فراخوانی می‌شود؛ در غیر این صورت `renderSection` با دادهٔ همان بخش.
  - در نتیجه فقط سکشنی که واقعاً خطا داده، پیام خطا می‌بیند؛ بقیه با داده یا حالت خالی نمایش داده می‌شوند.

---

## ۳. علت احتمالی «خطا در دریافت تاریخچه نوبت‌ها»

پیام **«خطا در دریافت تاریخچه نوبت‌ها»** از `PatientService` برمی‌گردد:

- **نوبت‌های اخیر:** در `GetPatientAppointmentsPagedAsync` در بلوک `catch` با `"خطا در دریافت تاریخچه نوبت‌ها."` برگردانده می‌شود.
- **نوبت‌های آینده:** در `GetPatientUpcomingAppointmentsPagedAsync` با `"خطا در دریافت نوبت‌های آینده."`.
- **پذیرش‌های اخیر:** در `GetPatientReceptionsAsync` با `"خطا در دریافت تاریخچه پذیرش‌ها. لطفاً دوباره تلاش کنید."`.

یعنی با **Exception** در یکی از این متدها (مثلاً کوئری، Include، یا مپ به ViewModel) آن بخش شکست می‌خورد. با مقاوم‌سازی بالا، فقط همان بخش خطا می‌گیرد و بقیه لود می‌شوند.

**برای رفع قطعی خطا پیشنهاد می‌شود:**

1. **لاگ سرور:** در زمان رخداد خطا، در لاگ‌های Serilog متن کامل exception را ببینید (مثلاً خطای EF روی جدول/ستون یا نال بودن یک رابطه).
2. **بررسی مدل و DB:** وجود و نام ستون‌ها/جدول‌های `Appointments`, `Doctor`, `ServiceCategory`, `IsDeleted`, و برای پذیرش `Receptions`, `ReceptionItems`, `Transactions`, `ActivePatientInsurance` و غیره.
3. **بررسی Extension:** اگر `ToPersianDateTime()` روی مقدار null صدا زده شود، ممکن است exception بدهد؛ در مپ‌ها از null-check یا مقدار پیش‌فرض استفاده شود.

---

## ۴. ساختار دادهٔ مورد انتظار در JS

- **QuickStats:** آبجکت با `TotalAppointments`, `UpcomingAppointments`, `CompletedAppointments`, `CancelledAppointments`, `TotalReceptions`.
- **نوبت‌ها:** آبجکت با آرایهٔ `Appointments` (هر آیتم: `AppointmentId`, `DoctorName`, `AppointmentDateShamsi`, `AppointmentTime`, `Status`, `StatusText`, `Price`, …) و اختیاری `HasMore`.
- **پذیرش‌ها:** آبجکت با آرایهٔ `Receptions` (هر آیتم: `ReceptionId`, `DoctorName`, `ReceptionDateShamsi`, `Status`, `StatusText`, `TotalAmount`, …) و اختیاری `HasMore`.

سریالایزیشن API معمولاً PascalCase است؛ در JS با `d.QuickStats || d.quickStats` و مشابه آن هر دو حالت پشتیبانی شده است.

---

## ۵. TODO لیست — بررسی و بهینه‌سازی تب خانه

### انجام‌شده

- [x] مقاوم‌سازی GetOverview: در صورت شکست یک بخش، بقیه بازگردانده شوند و فقط همان بخش خطا داشته باشد.
- [x] اضافه کردن `SectionErrors` به `DashboardViewModel` و پر کردن آن در سرویس.
- [x] در `loadOverview` نمایش خطا فقط برای سکشن‌های دارای خطا و مخفی کردن لودینگ قبل از نمایش خطا.

### باقی‌مانده (اولویت‌دار)

- [ ] **ریشه‌یابی خطا:** با استفاده از لاگ سرور و stack trace، علت دقیق exception در `GetPatientAppointmentsPagedAsync` / `GetPatientUpcomingAppointmentsPagedAsync` / `GetPatientReceptionsAsync` مشخص و رفع شود.
- [ ] **واحد تست (اختیاری):** حداقل یک تست برای `GetOverviewAsync` وقتی یکی از تسک‌ها Failed است؛ بررسی اینکه Overview موفق و `SectionErrors` پر است.

### بهینه‌سازی و UX

- [ ] **دکمه «مشاهده همه» برای پذیرش‌های اخیر:** در `_DashboardOverview.cshtml` برای کارت پذیرش‌های اخیر دکمهٔ «مشاهده همه» (مشابه نوبت‌ها) اضافه شود در صورت وجود مسیر/تب مناسب.
- [ ] **تلاش مجدد (Retry) در حالت Overview:** در حال حاضر `reloadSection('recentAppointments')` یک درخواست جدا به `GetRecentAppointments` می‌زند. اگر بخواهیم بعد از خطا فقط همان بخش دوباره از Overview لود شود، می‌توان یک بار دیگر `loadOverview` را صدا زد یا همان رفتار فعلی (درخواست جدا برای آن سکشن) را مستند کرد.
- [ ] **خالی بودن آمار (QuickStats):** در صورت شکست GetQuickStats، سکشن آمار ممکن است المنت `.dashboard-section-error` نداشته باشد (ساختار متفاوت با کارت‌ها). در صورت نیاز، یک بلوک خطا/خالی برای `#quickStatsContainer` تعریف و در JS برای خطای QuickStats همان الگو اعمال شود.
- [ ] **یکپارچگی با PATIENT_DASHBOARD_ROADMAP:** موارد مربوط به تب خانه (مثلاً 1.3 HasMore/TotalCount) در این سند ارجاع داده شوند تا یک نقشهٔ واحد برای داشبورد بیمار باشد.

---

## ۶. فایل‌های مرتبط

| فایل | نقش |
|------|-----|
| `Areas/Patient/Views/Dashboard/_DashboardOverview.cshtml` | قالب تب خانه و کارت‌های نوبت/پذیرش/آمار |
| `Areas/Patient/Views/Dashboard/Index.cshtml` | صفحهٔ داشبورد و بارگذاری اسکریپت‌ها |
| `Content/js/patient-dashboard.js` | لود Overview، renderSection، showError، SectionErrors |
| `Content/css/patient-dashboard-unified.css` | استایل کارت‌ها و لودینگ/خطا |
| `Services/PatientDashboardService.cs` | GetOverviewAsync، GetRecentAppointmentsAsync، GetUpcomingAppointmentsAsync، GetRecentReceptionsAsync |
| `Services/PatientService.cs` | GetPatientAppointmentsPagedAsync، GetPatientUpcomingAppointmentsPagedAsync، GetPatientReceptionsAsync |
| `Areas/Patient/Controllers/Api/PatientDashboardApiController.cs` | GetOverview، GetCurrentPatientIdAsync |
| `ViewModels/Patient/DashboardViewModel.cs` | QuickStats، RecentAppointments، UpcomingAppointments، RecentReceptions، SectionErrors |

---

## ۷. جمع‌بندی

با مقاوم‌سازی انجام‌شده، تب خانه حتی وقتی یکی از بخش‌های نوبت/پذیرش/آمار خطا بدهد، بقیه به‌درستی نمایش داده می‌شوند و فقط همان بخش پیام خطا و در صورت وجود دکمهٔ «تلاش مجدد» را نشان می‌دهد. گام بعدی، ریشه‌یابی و رفع exception در سرویس نوبت/پذیرش با استفاده از لاگ و بررسی مدل/DB است؛ سپس می‌توان موارد TODO بهینه‌سازی و UX را طبق همین نقشه راه پیش برد.
