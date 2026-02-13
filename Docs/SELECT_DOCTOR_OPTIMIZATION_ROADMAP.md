# نقشه راه بررسی و بهینه‌سازی صفحه انتخاب پزشک (SelectDoctor)

**URL:** `http://localhost:3560/Patient/Appointment/Book/SelectDoctor`  
**مسیر:** رزرو نوبت → گام ۱ از ۴ (انتخاب پزشک)  
**مرجع قبلی:** `SELECT_DOCTOR_MODULE_REVIEW.md` (بخش‌هایی از آن در کد اعمال شده است)

---

## ۱. نمای کلی فلو و معماری

### ۱.۱ فلو کاربر
```
ورود به /Patient/Appointment/Book
    → Redirect به SelectDoctor
        → GET SelectDoctor (departmentId?, searchTerm?)
            → بارگذاری اولیه: لیست پزشکان + دپارتمان‌ها از سرور (Server-Side)
        → جستجوی زنده (اختیاری): تایپ در searchInput یا تغییر بخش
            → AJAX GET /Patient/Api/DoctorSearch/GetAvailableDoctors
            → جایگزینی لیست با renderDoctors(data)
        → کلیک «انتخاب پزشک» روی کارت
            → ناوبری به /Patient/Appointment/Book/SelectDate/{doctorId}
```

### ۱.۲ نقشه لایه‌ها
| لایه | مسئولیت | فایل(ها) |
|------|---------|----------|
| **Route** | `Patient/Appointment/Book/SelectDoctor/{departmentId}` (departmentId اختیاری) | `PatientAreaRegistration.cs` |
| **Controller** | اعتبارسنجی ورودی، فراخوانی سرویس، ساخت ViewModel، خطا → View خالی + TempData | `AppointmentBookingController.SelectDoctor` |
| **Service** | جستجوی پزشکان، بارگذاری دسته‌ای برنامه و جزئیات، مپ به DTO | `AppointmentBookingService.GetAvailableDoctorsAsync` |
| **Department** | لیست بخش‌های فعال برای فیلتر | `IDepartmentManagementService.GetActiveDepartmentsForPatientAsync` |
| **API** | سرویس همان GetAvailableDoctorsAsync برای AJAX | `DoctorSearchApiController.GetAvailableDoctors` |
| **Factory** | ساخت DoctorSelectionViewModel از لیست پزشکان و دپارتمان‌ها | `AppointmentBookingViewModelFactory.CreateDoctorSelectionViewModel` |
| **View** | فرم جستجو (GET)، لیست کارت‌ها، حالت خالی، لودینگ، اعلان‌ها | `SelectDoctor.cshtml` |
| **Partial** | یک کارت پزشک (سرور) | `_DoctorCard.cshtml` |
| **JS** | رویداد انتخاب پزشک، جستجوی با Debounce، AJAX با Retry، رندر کارت از JSON | `doctor-selection.js` |

---

## ۲. فایل‌های کلیدی

| فایل | نقش |
|------|-----|
| `Areas/Patient/Controllers/AppointmentBookingController.cs` | اکشن `SelectDoctor`، اعتبار departmentId/searchTerm، NoCache، AllowAnonymous |
| `Areas/Patient/Controllers/Api/DoctorSearchApiController.cs` | `GetAvailableDoctors(departmentId, searchTerm)`، AllowAnonymous |
| `Services/Appointment/AppointmentBookingService.cs` | `GetAvailableDoctorsAsync`، استفاده از DoctorCrudService، Batch Load Schedules و DoctorDetails، مپ به DoctorSearchResultDto |
| `Interfaces/Appointment/IAppointmentBookingService.cs` | قرارداد GetAvailableDoctorsAsync |
| `Services/DepartmentManagementService.cs` | GetActiveDepartmentsForPatientAsync |
| `Factories/Patient/AppointmentBookingViewModelFactory.cs` | CreateDoctorSelectionViewModel |
| `ViewModels/Patient/DoctorSelectionViewModel.cs` | Doctors, SelectedDepartmentId, SearchTerm, Departments |
| `Models/DTOs/Appointment/DoctorSearchResultDto.cs` | DoctorId, FullName, Specialization, HasActiveSchedule, ScheduleInfo, ... |
| `Areas/Patient/Views/AppointmentBooking/SelectDoctor.cshtml` | مدل DoctorSelectionViewModel، فرم جستجو، لیست کارت، لودینگ، Diagnostic فقط در Debug |
| `Areas/Patient/Views/Shared/_DoctorCard.cshtml` | یک کارت پزشک؛ لینک «انتخاب پزشک» با Url.Action به SelectDate |
| `Scripts/patient/doctor-selection.js` | init، bindEvents، handleSelectDoctor، handleSearchInput، performSearch، renderDoctors، createDoctorCard، ajaxWithRetry، showError؛ وابسته به showLoading/hideLoading سراسری |
| `Content/css/appointment-booking-views.css` | استایل صفحه انتخاب پزشک، کارت، جستجو، empty-state |
| `Content/css/appointment-booking-progress.css` | نوار پیشرفت گام ۱/۴ |
| `Areas/Patient/Views/Shared/_PatientLayoutPro.cshtml` | تعریف توابع سراسری `showLoading()` و `hideLoading()` |

---

## ۳. امنیت

- **AllowAnonymous:** صفحه و API هر دو AllowAnonymous تا کاربران قبل از لاگین بتوانند پزشکان را ببینند؛ برای ادامه به SelectDate/رزرو، احراز هویت لازم است.
- **ورودی:** در Controller محدودیت `departmentId <= 0` → null، `searchTerm` Trim و حداکثر ۱۰۰ کاراکتر؛ در API همان پارامترها بدون اعتبار اضافی صریح (وابسته به سرویس).
- **خروجی:** در View از Razor برای خروجی (کاهش XSS)؛ در JS کارت‌های پویا با رشته‌های واردشده از API بدون escape صریح — **پیشنهاد:** استفاده از `escapeHtml` یا `text()` برای نام/تخصص/ scheduleInfo در `createDoctorCard`.
- **Cache:** با `[NoCache]` روی SelectDoctor پاسخ بدون کش برای داده به‌روز.

---

## ۴. عملکرد (Performance)

- **بار اول:** یک درخواست GET برای صفحه (لیست پزشکان + دپارتمان‌ها در همان پاسخ)، بدون درخواست اضافی برای لیست.
- **جستجوی زنده:** یک درخواست GET به API با Debounce ۵۰۰ms؛ حداکثر ۳ تلاش مجدد با Exponential Backoff و Timeout ۳۰ ثانیه.
- **سرویس:**
  - استفاده از `DoctorCrudService.GetDoctorsAsync` با PageSize=100.
  - بارگذاری دسته‌ای برنامه‌ها (Batch) برای همه doctorIds و یک بار غیرفعال/فعال کردن فیلترهای EF.
  - بارگذاری جزئیات (Bio) به صورت `Task.WhenAll` برای هر پزشک — هنوز N درخواست به GetDoctorDetailsAsync؛ در صورت نیاز می‌توان یک متد Batch در سرویس تعریف کرد.
- **N+1:** در بخش Schedules برطرف شده؛ در بخش Doctor Details هر پزشک یک فراخوانی جدا دارد.

---

## ۵. UX و فرانت‌اند

- **فرم جستجو:** روش GET با `action="@Url.Action("SelectDoctor")"`؛ ارسال فرم باعث رفرش صفحه با پارامترهای جدید می‌شود؛ جستجوی زنده جداگانه با AJAX و جایگزینی لیست.
- **دو منبع رندر کارت:**
  - **سرور:** از طریق `_DoctorCard` با مدل `DoctorSearchResultDto`؛ خروجی با نام‌های Pascal (مثل FullName).
  - **کلاینت:** در `createDoctorCard(doctor)` با propertyهای **camelCase** (doctor.doctorId, doctor.fullName, doctor.specialization, doctor.scheduleInfo). اگر API با PascalCase سریالایز شود، این مقادیر در کلاینت `undefined` می‌شوند و کارت بعد از جستجوی AJAX خراب نمایش داده می‌شود. **باید سازگاری نام propertyها (یا پشتیبانی هر دو) در JS بررسی و اصلاح شود.**
- **لودینگ:** استفاده از `showLoading()` / `hideLoading()` که در Layout تعریف شده‌اند؛ اگر صفحه با Layout دیگری لود شود، این توابع ممکن است تعریف نشوند و خطای JS رخ دهد.
- **حالت خالی:** هم در View (سرور) و هم در `renderDoctors` (کلاینت) پیام «پزشکی یافت نشد» نمایش داده می‌شود.
- **دکمه بازگشت:** لینک به `MyAppointments` در هدر صفحه.

---

## ۶. مسائل و ریسک‌های شناخته‌شده

| موضوع | توضیح | اولویت |
|--------|--------|--------|
| **حالت نام property در JSON API** | اگر API با PascalCase برگردد، `createDoctorCard` با doctor.fullName و ... مقدار نمی‌گیرد. | بالا |
| **عدم Escape در کارت پویا** | نام پزشک و متن‌های دیگر از API مستقیماً در HTML قرار می‌گیرند؛ در صورت وجود کاراکترهای خاص یا اسکریپت، ریسک XSS. | متوسط |
| **وابستگی showLoading/hideLoading به Layout** | وابستگی به توابع سراسری در _PatientLayoutPro؛ در صورت تغییر Layout یا بارگذاری جزئی صفحه ممکن است شکسته شود. | متوسط |
| **Doctor Details به صورت N فراخوانی** | GetDoctorDetailsAsync برای هر پزشک در GetAvailableDoctorsAsync؛ برای لیست بزرگ می‌توان Batch یا یک endpoint مشترک در نظر گرفت. | پایین (بهینه‌سازی) |
| **صفحه‌بندی لیست پزشکان** | در سرویس PageSize=100 ثابت؛ در صورت تعداد زیاد پزشک، بار اول سنگین یا لیست بلند؛ امکان افزودن صفحه‌بندی یا «بارگذاری بیشتر». | پایین |

---

## ۷. TODO لیست — بهینه‌سازی و رفع ایرادات

### انجام‌شده (طبق کد فعلی)
- [x] انتقال دریافت دپارتمان‌ها از Controller به Service (`GetActiveDepartmentsForPatientAsync`).
- [x] اعتبارسنجی departmentId و searchTerm در Controller (طول، Trim).
- [x] Conditional Rendering برای Diagnostic View فقط در Debug.
- [x] حذف Cache (NoCache) برای داده Real-time.
- [x] Batch Loading برای Schedules در GetAvailableDoctorsAsync.
- [x] استفاده از Factory برای ساخت DoctorSelectionViewModel.
- [x] رویداد فقط برای `button.select-doctor-btn`؛ لینک‌های `<a>` بدون جلوگیری از ناوبری.
- [x] Retry و Timeout در AJAX جستجو.
- [x] URL صحیح SelectDate در _DoctorCard با Fallbackها.

### اولویت بالا
- [x] **سازگاری نام property در پاسخ API و JS:** در `createDoctorCard` از هر دو نام (camelCase و PascalCase) پشتیبانی می‌شود، مثلاً `doctor.fullName ?? doctor.FullName`.
- [x] **Escape خروجی در کارت پویا:** در `createDoctorCard` برای fullName، specialization و scheduleInfo از تابع `escape` (textContent + innerHTML) استفاده شده تا از XSS جلوگیری شود.

### اولویت متوسط
- [x] **تعریف محلی showLoading/hideLoading برای SelectDoctor:** در `doctor-selection.js` متدهای `_showLoading` و `_hideLoading` اضافه شده‌اند؛ در صورت وجود توابع سراسری از آن‌ها استفاده می‌کنند، وگرنه `#loadingState` را نمایش/مخفی می‌کنند.
- [ ] **یکسان‌سازی رندر کارت سرور و کلاینت:** یا استفاده از یک template سمت کلاینت که از همان ساختار _DoctorCard پیروی کند (از جمله لینک با data-doctor-id و data-href)، یا بارگذاری HTML کارت از طریق یک Partial/Action که با پارامتر لیست پزشکان فراخوانی شود تا یک منبع حقیقت برای ظاهر کارت وجود داشته باشد.
- [ ] **اعتبارسنجی و محدودیت در API:** در DoctorSearchApiController برای searchTerm طول و برای departmentId محدوده عددی معقول اعمال شود (هماهنگ با Controller صفحه).

### اولویت پایین (بهینه‌سازی)
- [ ] **صفحه‌بندی یا Lazy Load:** در صورت افزایش تعداد پزشکان، امکان صفحه‌بندی در بار اول یا «مشاهده بیشتر» یا Infinite Scroll برای بهبود بار اول و UX.
- [ ] **Batch برای Doctor Details:** در GetAvailableDoctorsAsync در صورت نیاز به Bio برای همه، طراحی متد Batch در DoctorCrudService یا یک کوئری مشترک برای کاهش N فراخوانی.
- [ ] **امتیاز و تعداد نظرات (Rating/ReviewCount):** در DTO موجود است؛ اگر منبع داده آماده شود، نمایش در _DoctorCard و در createDoctorCard برای تجربه مشابه سایت‌های مرجع.
- [ ] **آزمایش E2E:** یک سناریوی تست برای فلو کامل SelectDoctor → SelectDate (ورود، جستجو، کلیک انتخاب) برای جلوگیری از بازگشت خطاها.

---

## ۸. چک‌لیست تست دستی

- [ ] بارگذاری `/Patient/Appointment/Book/SelectDoctor` بدون پارامتر → نمایش لیست پزشکان و فیلتر بخش.
- [ ] بارگذاری با `?departmentId=2` و `?searchTerm=احمد` → فیلتر درست اعمال شود.
- [ ] جستجوی زنده: تایپ در جستجو (حداقل ۲ کاراکتر) یا تغییر بخش → درخواست AJAX، نمایش لودینگ، جایگزینی لیست؛ نام و تخصص پزشکان در کارت‌ها درست نمایش داده شود (بررسی Pascal/camel).
- [ ] کلیک «انتخاب پزشک» روی کارت (هم از رندر سرور و هم از رندر AJAX) → ناوبری به `/Patient/Appointment/Book/SelectDate/{doctorId}`.
- [ ] قطع شبکه و جستجو → پس از Retry، پیام خطای مناسب نمایش داده شود.
- [ ] بارگذاری با Layout _PatientLayoutPro → بعد از جستجو، showLoading/hideLoading بدون خطا اجرا شوند.

---

## ۹. جمع‌بندی

صفحه SelectDoctor از نظر معماری (Controller → Service، Factory، API جدا)، امنیت اولیه (ورودی، NoCache)، و مقاومت در برابر خطای شبکه (Retry، Timeout) در وضعیت قابل قبولی است. برای «بررسی عمیق و بهینه‌سازی حرفه‌ای» تمرکز روی **سازگاری نام propertyهای JSON با JS**، **امنیت خروجی در کارت پویا**، و **استقلال لودینگ از Layout** توصیه می‌شود؛ سپس یکسان‌سازی رندر کارت و بهینه‌سازیهای اختیاری (صفحه‌بندی، Batch جزئیات) طبق همین نقشه راه و TODO لیست قابل انجام است.
