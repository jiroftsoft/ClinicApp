# تحلیل داشبورد نقش ادمین (Admin Dashboard)

## ۱. نقشهٔ کلی

در پنل ادمین **یک داشبورد مرکزی واحد** (مثل یک صفحهٔ «خانهٔ ادمین») به‌صورت کنترلر جدا تعریف نشده است. به‌جای آن چند **داشبورد تخصصی** وجود دارد که هر کدام برای یک حوزهٔ کاری هستند:

| داشبورد | کنترلر | مسیر | نقش |
|--------|--------|------|-----|
| **داشبورد پزشک** | `DoctorDashboardController` | `Admin/DoctorDashboard` | آمار پزشکان، انتسابات، جستجو، جزئیات و آمار کلی |
| **داشبورد منشی‌ها** | `CashierDashboardController` | ریشه (خارج از Area) | داشبورد مالی/صندوق منشی‌ها |
| **گزارشات صندوق** | `CashierReportController` | ریشه | گزارشات صندوق |
| **داشبورد بهینه‌سازی برنامه** | `ScheduleOptimizationController` | `Admin/ScheduleOptimization` | آمار و کارت‌های داشبورد بهینه‌سازی |
| **داشبورد دسترسی‌پذیری نوبت** | `AppointmentAvailabilityController` | `Admin/AppointmentAvailability` | کارت‌های داشبورد نوبت‌ها |
| **داشبورد پزشک (تک‌پزشک)** | `DoctorReportingController.Dashboard` | درون DoctorReporting | داشبورد یک پزشک خاص |

منوی ماژول‌های ادمین (`_AdminModulesMenu.cshtml`) به این داشبوردها لینک می‌دهد؛ برای **CashierDashboard** و **CashierReport** از `area = ""` استفاده می‌شود تا به کنترلرهای ریشه (`Controllers/Payment/`) بروند، بقیه با `area = "Admin"`.

---

## ۲. داشبورد پزشکان (DoctorDashboard) — اصلی‌ترین داشبورد ادمین

### ۲.۱ مسیر و کنترلر

- **URL:** `Admin/DoctorDashboard` یا `Admin/DoctorDashboard/Index`
- **کنترلر:** `Areas/Admin/Controllers/DoctorDashboardController.cs`
- **فیلترها:** `[Authorize]`, `[MedicalEnvironmentFilter]`, `[CheckProfileCompletion]`

### ۲.۲ جریان داده (Flow)

```
DoctorDashboardController.Index(clinicId?, departmentId?)
    → IDoctorDashboardService.GetDashboardDataAsync(clinicId, departmentId)
        → IDoctorDashboardRepository.GetDashboardDataAsync(clinicId, departmentId)
            → EF: Doctors + DoctorDepartments + DoctorSpecializations + ...
            → GetDashboardStatsAsync(clinicId)
            → GetRecentAssignmentsAsync(...)
            → GetSystemAlertsAsync(...)
    → View(result.Data)  // DoctorDashboardIndexViewModel
```

### ۲.۳ سرویس و ریپازیتوری

- **سرویس:** `Services/ClinicAdmin/DoctorDashboardService.cs`  
  - اعتبارسنجی پارامترها، فراخوانی ریپازیتوری، برگرداندن `ServiceResult<DoctorDashboardIndexViewModel>`.
- **ریپازیتوری:** `Repositories/ClinicAdmin/DoctorDashboardRepository.cs`  
  - کوئری روی `Doctors` با فیلتر کلینیک/دپارتمان، ساخت `DoctorDashboardIndexViewModel` شامل:
    - `Stats` (DashboardStatsViewModel): TotalDoctors, ActiveDoctors, TotalAssignments, CompletionPercentage, ...
    - `RecentDoctors`, `RecentAssignments`, `SystemAlerts`, `ActiveFilters`

### ۲.۴ ویو و مدل

- **ویو:** `Areas/Admin/Views/DoctorDashboard/Index.cshtml`
- **مدل انتظاری:** `DoctorDashboardIndexViewModel` (از `Interfaces/ClinicAdmin` یا `ViewModels.DoctorManagementVM`).
- **محتوای صفحه:**
  - کارت آمار: کل پزشکان، پزشکان فعال، کل انتسابات، نرخ تکمیل
  - فیلتر/جستجو: کلینیک، دپارتمان، جستجوی متن
  - عملیات سریع: لینک به DoctorCrud، DoctorReporting، DoctorDepartment، DoctorServiceCategory، DoctorSchedule
  - لیست پزشکان اخیر با لینک جزئیات

نکته: در ویو از `Model?.TotalDoctors`, `Model?.ActiveDoctors`, `Model?.Clinics`, `Model?.Departments` استفاده شده؛ در تعریف فعلی `DoctorDashboardIndexViewModel` در اینترفیس، این مقادیر داخل `Stats` و به‌صورت لیست جدا (مثلاً Clinics/Departments) هستند. اگر مدل برگشتی از سرویس همان ساختار اینترفیس باشد، باید در ویو از `Model?.Stats?.TotalDoctors` و منبع لیست‌های کلینیک/دپارتمان اطمینان حاصل شود تا نمایش و فیلترها درست کار کنند.

### ۲.۵ اکشن‌های دیگر همین کنترلر

- **Details(id):** جزئیات یک پزشک
- **Assignments(id):** انتسابات یک پزشک
- **Search(...):** جستجو با فیلتر
- **Stats(clinicId?):** صفحهٔ آمار کلی
- **GetActiveDoctorsStats**, **GetQuickActions**, **GetDoctorStatus:** اکشن‌های AJAX برای ویو

---

## ۳. لینک «داشبورد» در Breadcrumb

در `Areas/Admin/Views/Shared/_Breadcrumb.cshtml` لینک «پنل مدیریت» به صورت زیر است:

```csharp
Url.Action("Index", "Dashboard", new { area = "Admin" })
```

در Area ادمین **کنترلری به نام `Dashboard` وجود ندارد**؛ فقط `DoctorDashboard` و بقیهٔ داشبوردهای بالا تعریف شده‌اند. بنابراین این لینک به **۴۰۴** می‌خورد.

پیشنهاد: یا کنترلر `Dashboard` در Area ادمین اضافه شود (مثلاً با یک صفحهٔ خلاصه/لندینگ) و اکشن `Index` آن به همان داشبورد موردنظر (مثلاً DoctorDashboard) redirect کند، یا متن لینک به یکی از داشبوردهای موجود (مثلاً DoctorDashboard) تغییر کند و از `Url.Action("Index", "DoctorDashboard", new { area = "Admin" })` استفاده شود.

---

## ۴. خلاصهٔ فایل‌های کلیدی

| بخش | فایل |
|-----|------|
| کنترلر داشبورد پزشکان | `Areas/Admin/Controllers/DoctorDashboardController.cs` |
| سرویس | `Services/ClinicAdmin/DoctorDashboardService.cs` |
| ریپازیتوری | `Repositories/ClinicAdmin/DoctorDashboardRepository.cs` |
| اینترفیس سرویس | `Interfaces/ClinicAdmin/IDoctorDashboardService.cs` |
| اینترفیس ریپازیتوری | `Interfaces/ClinicAdmin/IDoctorDashboardRepository.cs` |
| ویو Index | `Areas/Admin/Views/DoctorDashboard/Index.cshtml` |
| منوی ماژول‌ها | `Areas/Admin/Views/Shared/_AdminModulesMenu.cshtml` |
| Breadcrumb | `Areas/Admin/Views/Shared/_Breadcrumb.cshtml` |
| روت‌های Area | `Areas/Admin/AdminAreaRegistration.cs` (مثلاً Admin_DoctorDashboard_Routes) |

---

## ۵. جمع‌بندی برای نقش ادمین

- **ورود ادمین:** پس از لاگین، بر اساس `AccountController` به `Home` (حوزه خالی) هدایت می‌شود؛ مسیر پیش‌فرض جدا برای «اولین صفحهٔ ادمین» در کد دیده نشد.
- **داشبورد اصلی قابل‌دسترس از منو:** «داشبورد پزشک» (`DoctorDashboard`) از منوی ماژول‌ها با مسیر `Admin/DoctorDashboard` در دسترس است و عملاً **داشبورد اصلیِ قابل‌اتکا برای نقش ادمین** است.
- **داشبورد منشی‌ها و گزارش صندوق:** از همان منو با لینک بدون Area به `CashierDashboard` و `CashierReport` در ریشه پروژه می‌روند.
- **مشکل فعلی:** لینک «پنل مدیریت» در Breadcrumb به کنترلر ناموجود `Dashboard` در Area ادمین اشاره دارد و باید اصلاح یا با یک داشبورد واقعی جایگزین شود.

با این تحلیل می‌توان داشبورد نقش ادمین را بر اساس **DoctorDashboard** و در صورت نیاز با اضافه کردن یک **Dashboard مرکزی** یا اصلاح Breadcrumb، به‌صورت یکپارچه تعریف و گسترش داد.
