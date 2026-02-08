# 📋 پرونده پزشکی (EMR) – بررسی کامل، ارتباطات، منطق کور، نقشه راه و TODO تولید

**تاریخ:** ۱۴۰۴/۱۱/۰۷  
**هدف:** کامل شدن ماژول پرونده پزشکی برای کلینیک سلامت – آماده پروداکشن  
**Preflight:** نوع کار = معمولی (تکمیل ماژول)؛ ریسک امنیت/داده = بالا (داده بیمار)

---

## ۱. خلاصه وضعیت فعلی

| بخش | وضعیت | توضیح |
|-----|--------|------|
| **صفحه اختصاصی** `/Patient/MedicalRecord` | ✅ پیاده‌سازی شده | Shell + تاریخچه پزشکی + نوبت‌ها + پذیرش‌ها، Export PDF/Excel، AJAX |
| **تب داشبورد** «پرونده پزشکی» | ⚠️ Placeholder | فقط پیام «در حال توسعه» – محتوای واقعی لود نمی‌شود |
| **تاریخچه پزشکی (CRUD)** | ✅ پیاده‌سازی شده | Service، Repository، Factory، API، Modal، آپلود ضمیمه |
| **نوبت‌ها / پذیرش‌ها** | ✅ پیاده‌سازی شده | از سرویس/دیتابیس موجود، فقط خواندنی در EMR |
| **تریاژ (علائم حیاتی)** | ❌ ناقص | API و ViewModel وجود دارد؛ سرویس همیشه لیست خالی برمی‌گرداند؛ بخش در Shell/JS نیست |
| **یکپارچگی تب داشبورد با پرونده** | ❌ انجام نشده | تب با صفحهٔ اصلی پرونده ارتباط ندارد |

---

## ۲. ارتباطات ماژول (وابستگی‌ها و مصرف‌کنندگان)

### ۲.۱ وابستگی‌های ورودی (پیش‌نیازها)

| وابستگی | نقش | وضعیت |
|---------|-----|--------|
| **Patient (ApplicationUser ↔ Patient)** | تشخیص بیمار جاری | ✅ BasePatientController.GetCurrentPatientIdAsync |
| **IPatientMedicalRecordService** | منطق پرونده | ✅ MedicalRecordService |
| **IMedicalRecordRepository** | دسترسی MedicalHistory | ✅ MedicalRecordRepository |
| **IPatientService** | جزئیات بیمار | ✅ استفاده در GetMedicalRecordAsync |
| **ICurrentUserService** | کاربر جاری برای Audit | ✅ |
| **ITriageService** | لیست تریاژ بیمار | ⚠️ در MedicalRecordService تزریق/فراخوانی **نشده** – منبع منطق کور |
| **IAppointmentRepository / سرویس نوبت** | نوبت‌های بیمار | ✅ از طریق سرویس/رپازیتوری موجود |
| **Reception (پذیرش)** | پذیرش‌های بیمار | ✅ از طریق سرویس موجود |
| **IDocumentUploadService** | آپلود ضمیمه تاریخچه | ✅ در MedicalRecordApiController |
| **MedicalRecordFactory** | Entity ↔ ViewModel | ✅ |

### ۲.۲ مصرف‌کنندگان (چه کسی از این ماژول استفاده می‌کند)

| مصرف‌کننده | نحوه استفاده |
|------------|--------------|
| **منوی پروفایل / سایدبار Patient** | لینک «پرونده الکترونیک» → `/Patient/MedicalRecord` |
| **داشبورد بیمار** | تب «پرونده پزشکی» → فعلاً فقط `_MedicalRecordTab` (Placeholder) |
| **Layout Patient** | لینک سایدبار به همان صفحه پرونده |

### ۲.۳ نقشهٔ مرز ماژول

```
[ورود بیمار]
     ↓
[BasePatientController] → GetCurrentPatientIdAsync()
     ↓
[MedicalRecordController] ← GET /Patient/MedicalRecord
     ↓
[IPatientMedicalRecordService.GetMedicalRecordAsync]
     ├── PatientService (اطلاعات بیمار)
     ├── IMedicalRecordRepository (تاریخچه پزشکی)
     └── (نوبت/پذیرش از سرویس‌های موجود)
     ↓
[MedicalRecordFactory] → ViewModels
     ↓
[_MedicalRecordShell] + medical-record.js
     ├── AJAX: GetMedicalHistories → _MedicalHistorySection
     ├── AJAX: GetAppointments     → _AppointmentsSection
     └── AJAX: GetReceptions      → _ReceptionsSection
```

---

## ۳. منطق کور (مناطقی که ناقص یا نامشخص هستند)

### ۳.۱ تریاژ در پرونده پزشکی

- **کد:** `MedicalRecordService.GetTriageAssessmentsAsync` همیشه `new List<MedicalRecordTriageViewModel>()` برمی‌گرداند.
- **یادداشت در کد:** `// FIXME(Phase 2): دریافت از TriageService یا Repository`
- **واقعیت:** `ITriageService.GetPatientTriageAssessmentsAsync(int patientId)` در پروژه وجود دارد و قابل استفاده است.
- **اقدام:** تزریق `ITriageService` در `MedicalRecordService` و فراخوانی `GetPatientTriageAssessmentsAsync`؛ سپس تبدیل با `MedicalRecordFactory.ToTriageViewModelList`. در UI هم بخش تریاژ به Shell و JS اضافه شود (یا در فاز بعد به صورت صریح در نقشه راه و TODO آورده شود).

### ۳.۲ تب «پرونده پزشکی» در داشبورد

- **وضعیت:** `DashboardController.MedicalRecordTab()` فقط `PartialView("_MedicalRecordTab")` برمی‌گرداند که یک placeholder است.
- **منطق کور:** مشخص نیست تب باید «همان Shell پرونده» را نشان دهد یا فقط لینک به صفحهٔ کامل. از نظر UX دو گزینه معقول است:
  - **الف)** لود همان Shell (یا iframe/لینک) داخل تب تا کاربر بدون خروج از داشبورد پرونده را ببیند.
  - **ب)** نمایش خلاصه (مثلاً تعداد تاریخچه/نوبت/پذیرش) + دکمه «مشاهده پرونده کامل» → `/Patient/MedicalRecord`.
- **اقدام:** در نقشه راه و TODO تصمیم UX (الف یا ب) گرفته و پیاده‌سازی شود.

### ۳.۳ ضمیمه‌های تاریخچه پزشکی

- **وضعیت:** آپلود در API با `IDocumentUploadService` انجام می‌شود؛ مسیرها در فیلد `Attachments` (رشته با جداکننده) ذخیره می‌شوند.
- **منطق کور احتمالی:** حذف فایل از دیسک هنگام حذف/ویرایش رکورد تاریخچه؛ محدودیت نوع/حجم فایل در سمت سرور؛ دسترسی فقط برای همان بیمار. بهتر است در چک‌لیست پروداکشن صریح شود.

### ۳.۴ دسترسی و امنیت

- **وضعیت:** هر اکشن با `GetCurrentPatientIdAsync()` و در سرویس با `ValidatePatientAccessAsync(patientId)` محدود به بیمار جاری است.
- **منطق کور:** اگر جایی مستقیم با `patientId` ورودی از کلاینت کار شود (بدون تطبیق با کاربر جاری) خطر دارد. در کد فعلی Controller/API از `GetCurrentPatientIdAsync()` استفاده شده که درست است.

---

## ۴. پیش‌نیازهای تکمیل ماژول (برای پروداکشن)

1. **احراز هویت و دسترسی:** فقط بیمار لاگین‌شده به پروندهٔ خودش دسترسی داشته باشد (الان برقرار است؛ در هر تغییر جدید هم رعایت شود).
2. **دادهٔ پایه:** Patient، Appointment، Reception، MedicalHistory، TriageAssessment در دیتابیس و سرویس‌ها موجود باشند (الان هست).
3. **Triage:** برای نمایش تریاژ در پرونده، `ITriageService` و موجودیت/جدول تریاژ باید در محیط پروداکشن فعال و قابل اتکا باشند.
4. **آپلود:** مسیر `~/Content/Uploads/MedicalHistory` و سرویس آپلود در سرور پروداکشن پیکربندی و تست شده باشد.
5. **قراردادهای پروژه:** طبق همان Preflight و قرارداد توسعه (رنگ/فونت، Strongly-Typed، ServiceResult، Factory، لاگ، Validation) هر تغییر جدید اضافه شود.

---

## ۵. نقشه راه (مراحل پیشنهادی)

### فاز ۱ – تکمیل منطق و یکپارچگی (اولویت بالا)

| مرحله | کار | خروجی |
|--------|-----|--------|
| ۱.۱ | اتصال تریاژ: تزریق `ITriageService` در `MedicalRecordService` و پر کردن `GetTriageAssessmentsAsync` از `GetPatientTriageAssessmentsAsync` + Factory | تریاژ واقعی در API |
| ۱.۲ | اضافه کردن بخش «تریاژ / علائم حیاتی» به `_MedicalRecordShell` و به `medical-record.js` (section جدید + partial و API) | نمایش تریاژ در صفحه پرونده |
| ۱.۳ | تکمیل تب داشبورد: تصمیم UX (همان Shell در تب یا خلاصه + لینک)؛ پیاده‌سازی (مثلاً لود Shell via AJAX یا Partial که همان Shell را رندر کند) | تب «پرونده پزشکی» با محتوای واقعی |

### فاز ۲ – سخت‌گیری پروداکشن

| مرحله | کار | خروجی |
|--------|-----|--------|
| ۲.۱ | Validation سمت سرور برای Create/Update تاریخچه (طول فیلدها، نوع MedicalHistoryType، تاریخ‌ها) و برگرداندن پیام خطای مناسب در API | خطای کمتر و امن‌تر |
| ۲.۲ | محدودیت آپلود: نوع فایل، حداکثر حجم، و در صورت نیاز اسکن امنیتی (مثلاً فقط لیست پسوند مجاز و حداکثر سایز) | کاهش ریسک آپلود |
| ۲.۳ | لاگ دسترسی به پرونده (مثلاً در Controller یا Service هنگام GetMedicalRecord) برای Audit | قابلیت ردیابی |
| ۲.۴ | رفتار خطا و انقضا: وقتی توکن منقضی یا 401 است، در JS ریدایرکت به لاگین با `returnUrl` (الان تا حدی هست؛ یکسان‌سازی برای همهٔ اکشن‌های پرونده) | UX و امنیت یکسان |

### فاز ۳ – کیفیت و نگهداری

| مرحله | کار | خروجی |
|--------|-----|--------|
| ۳.۱ | تست دستی: ورود به عنوان بیمار، باز کردن پرونده، CRUD تاریخچه، آپلود، مشاهده نوبت/پذیرش، (بعد از ۱.۲) تریاژ، و تب داشبورد | چک‌لیست تست |
| ۳.۲ | به‌روزرسانی مستندات (مثلاً همین سند یا EMR_MODULE_*.md) با وضعیت نهایی و مسیرها | مستندات به‌روز |
| ۳.۳ | در صورت نیاز: Unit/Integration تست برای MedicalRecordService (حداقل GetMedicalRecordAsync و CreateMedicalHistoryAsync) | پوشش تست |

---

## ۶. TODO لیست پیاده‌سازی (Production-Ready)

چک‌لیست زیر به ترتیب اولویت برای «کامل شدن» ماژول و آماده پروداکشن پیشنهاد شده است.

### Backend

- [ ] **EMR-1** تزریق `ITriageService` در `MedicalRecordService` و پیاده‌سازی واقعی `GetTriageAssessmentsAsync` با `GetPatientTriageAssessmentsAsync` و `MedicalRecordFactory.ToTriageViewModelList`.
- [ ] **EMR-2** Validation مدل `MedicalHistoryCreateEditViewModel` در سرویس (یا Attribute) برای Create/Update و برگرداندن `ServiceResult` با پیام خطای فارسی.
- [ ] **EMR-3** در Create/Update تاریخچه پزشکی، اعمال محدودیت نوع و حجم فایل برای آپلود (و در صورت نیاز پاکسازی فایل‌های قدیمی در حذف/ویرایش).
- [ ] **EMR-4** لاگ Audit برای دسترسی به پرونده (مثلاً در `GetMedicalRecordAsync` یا Controller با سطح Information).

### API

- [ ] **EMR-5** API برای بخش تریاژ: اگر بخش تریاژ در Shell اضافه شد، endpoint (مثلاً همان `GetTriageAssessments`) با صفحه‌بندی و پاسخ یکسان با بقیهٔ بخش‌ها برگردانده شود.
- [ ] **EMR-6** پاسخ یکسان برای 401 در همهٔ اکشن‌های MedicalRecord API (مثلاً `redirectUrl` برای لاگین).

### Frontend (Views + JS)

- [ ] **EMR-7** تکمیل تب «پرونده پزشکی» در داشبورد: یا لود Partial همان Shell (از MedicalRecord) یا نمایش خلاصه + لینک به `/Patient/MedicalRecord`؛ حذف placeholder «در حال توسعه».
- [ ] **EMR-8** (اختیاری در فاز ۱) اضافه کردن بخش تریاژ/علائم حیاتی به `_MedicalRecordShell` و `medical-record.js` (section + partial + فراخوانی API).
- [ ] **EMR-9** در صورت استفاده از رنگ/فونت پروژه، مطابقت با پالت `--medical-*` و فونت‌های تعریف‌شده در قرارداد توسعه.
- [ ] **EMR-10** نمایش خطاهای API در UI (مثلاً toast یا alert یکسان با بقیهٔ Patient Area) و دکمه «تلاش مجدد» برای بخش‌های ناموفق.

### امنیت و عملیات

- [ ] **EMR-11** اطمینان از اینکه هیچ اکشنی با `patientId` از ورودی کاربر کار نمی‌کند؛ فقط از `GetCurrentPatientIdAsync()` استفاده شود.
- [ ] **EMR-12** بررسی پوشه و دسترسی آپلود در سرور پروداکشن (`Content/Uploads/MedicalHistory`) و تنظیمات IIS/سرور.

### تست و مستندات

- [ ] **EMR-13** تست دستی کامل: لاگین بیمار، پرونده، تاریخچه (افزودن/ویرایش/حذف)، آپلود، نوبت، پذیرش، تریاژ (پس از پیاده‌سازی)، تب داشبورد، Export PDF/Excel.
- [ ] **EMR-14** به‌روزرسانی `EMR_MODULE_COMPREHENSIVE_REVIEW.md` یا این سند با وضعیت نهایی و تاریخ.

---

## ۷. جمع‌بندی

- **ارتباط ماژول:** وابستگی‌ها (Patient، سرویس/رپازیتوری پرونده، نوبت، پذیرش، آپلود، و ترجیحاً Triage) و مصرف‌کنندگان (منو، سایدبار، داشبورد) مشخص است.
- **منطق کور اصلی:** (۱) تریاژ در سرویس پرونده خالی است در حالی که `ITriageService.GetPatientTriageAssessmentsAsync` وجود دارد؛ (۲) تب داشبورد فقط placeholder است و تصمیم UX برای تکمیل آن لازم است.
- **پیش‌نیازها:** همان احراز هویت، دادهٔ پایه، Triage، آپلود و رعایت قراردادهای پروژه.
- **نقشه راه:** فاز ۱ (تریاژ + تب داشبورد)، فاز ۲ (Validation، آپلود، لاگ، خطا)، فاز ۳ (تست، مستندات، در صورت نیاز تست واحد).
- **TODO لیست:** ۱۴ آیتم با اولویت Backend → API → Frontend → امنیت/عملیات → تست/مستندات برای پیاده‌سازی حرفه‌ای و آماده پروداکشن.

با انجام این موارد، ماژول پرونده پزشکی از نظر عملکرد، امنیت و نگهداری برای محیط پروداکشن کلینیک سلامت قابل تحویل خواهد بود.
