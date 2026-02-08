# 📋 گزارش بررسی کامل قراردادها و پایگاه دانش ClinicApp

**تاریخ بررسی:** ۱۴۰۴/۱۱/۰۷  
**دامنه:** از اول تا آخر (Master → Contracts → 1015 → PreFlight)  
**وضعیت:** ✅ بررسی انجام شد

---

## ۱. خلاصه اجرایی

تمام فایل‌های درخواستی در مسیرهای زیر بررسی شدند:

- **Knowledge-Base/AI/Master** (۱۵ فایل)
- **Contracts** (روت – حدود ۱۵ فایل)
- **Contracts/1015** (۳ فایل)
- **Knowledge-Base/AI/PreFlight** (۵ فایل)

ساختار کلی منسجم، هدف‌گذاری روشن (راهنما، قرارداد، PreFlight، نقش‌ها) و زنجیرهٔ ارجاع بین اسناد برقرار است. چند مورد ناسازگاری مسیر و ارجاع و تکرار بین اسناد وجود دارد که در بخش‌های بعد آمده است.

---

## ۲. بررسی به ترتیب (از اول تا آخر)

### ۲.۱) Knowledge-Base / Master

| فایل | نقش | وضعیت | نکته |
|------|-----|--------|------|
| **INDEX.md** | فهرست و نقشه راه | ✅ قوی | لینک به ۰۱–۰۸، PreFlight، FAQ، مسیر یادگیری |
| **01-Helpers-DateTime.md** | Helpers تاریخ/زمان | ✅ کامل | ۶ Helper + Enterprise Date (ITimeProvider, UTC)، ارجاع به Jalali Enterprise |
| **02-Helpers-Validation.md** | اعتبارسنجی | ✅ کامل | کد ملی، موبایل، Identity، ValidationResult |
| **03-Development-Contract-Quick-Guide.md** | قرارداد توسعه خلاصه | ✅ الزامی | رنگ، Strongly-Typed، SRP، Bulletproof، DatePicker، CKEditor، Checklist |
| **04-TODO-Implementation-Guide.md** | پیاده‌سازی ماژول | ✅ مفید | ۱۳ Phase، زمان‌بندی، Checklist |
| **06-Quick-Reference.md** | مرجع سریع | ✅ مفید | جدول Use Case → Helper، ۵۶+ مورد |
| **08-MVC-Routing-Best-Practices.md** | روتینگ MVC | ✅ کاربردی | ترتیب Route، UseNamespaceFallback، Area |
| **README.md** | راهنمای پایگاه دانش | ✅ خوب | ارجاع به PreFlight، CRITICAL-FINANCIAL، ۰۳/۰۴/۰۵ |
| **HelperExtensionsGuide.md** | جعبه ابزار | ✅ خوب | Extensions + Helpers با مثال |
| **ClinicApp – Ultra-Lean Module Review Prompt.md** | پرامپت بررسی ماژول | ✅ کوتاه | Preflight → Snapshot → Issues → Root Cause → Fix → Tests |
| **CLINICAPP_MODULE_REVIEW_PROMPT.md** | همان نقش، نسخه تفصیلی | ✅ | مشابه Ultra-Lean با جزئیات بیشتر |
| **CLINICAPP_PROMPT_MASTER.md** | مرجع نقش‌ها و قراردادها | ✅ مهم | R1–R7، C1–C5، قالب خروجی |
| **CURSOR_MODULE_REVIEW_CONTRACT.md** | قرارداد بررسی در Cursor | ✅ | Preflight، Snapshot، Critical Issues، Root Cause، Fix، Tests |
| **DEBUGGING_MASTER_PROMPT.md** | پرامپت دیباگ ارشد | ✅ طولانی | قراردادها، ۵ Whys، رفع اتمیک، چک‌لیست |
| **SYSTEM NOTE — DEBUG CONTRACT LOCK.md** | قفل حالت دیباگ | ✅ کوتاه | بدون تکرار قرارداد؛ فقط نقض و اجرا |

**نکات Master:**

- در **INDEX** به `05-Debugging-Specialist-Contract.md` و `CRITICAL-FINANCIAL-MODULE-CONTRACT.md` ارجاع داده شده؛ این دو در **PreFlight** و مسیرهای دیگر هستند؛ در INDEX مسیر دقیق (مثلاً `../PreFlight/05-...`) ذکر نشده.
- در **README** مسیر `PREFLIGHT_CHECKLIST.md` به صورت `../../PREFLIGHT_CHECKLIST.md` است؛ با ساختار واقعی `Contracts/Knowledge-Base/AI/PreFlight/PREFLIGHT_CHECKLIST.md` باید تطبیق داده شود.
- **01-Helpers-DateTime** و **03-Development-Contract** هر دو به JalaliDatePicker Enterprise و `Docs/Jalili/...` ارجاع می‌دهند؛ یکسان و سازگار است.

---

### ۲.۲) Contracts (روت)

| فایل | نقش | وضعیت | نکته |
|------|-----|--------|------|
| **AI_PREFLIGHT_INDEX.md** | نقشه PreFlight و V3 | ✅ خوب | چه فایلی کی استفاده شود؛ Workflow مالی/باگ |
| **AI_EXECUTION_CONTRACT.md** | قرارداد اجرایی هر پاسخ | ✅ الزامی | چک ۳۰ ثانیه‌ای، ۱۵ ممنوعیت، مالی/باگ |
| **AI_PREFLIGHT_QUICK_V3.md** | چک سریع روزانه | ✅ | خلاصه برای قبل از هر پاسخ |
| **AI_PREFLIGHT_MASTER_V3.md** | PreFlight کامل + مالی + دیباگ | ✅ | STEP 0–7، ادغام چند قرارداد |
| **AI_PREFLIGHT_V3_README / V3_SUMMARY** | راهنما و خلاصه V3 | ✅ | تفاوت V2/V3، Workflow |
| **AI_PREFLIGHT_* (V2)** | نسخه قبلی | ✅ مرجع | برای سازگاری با اشاره‌های قدیمی |
| **AI_PREFLIGHT_KNOWLEDGE_BASE_* / REMINDER** | ادغام KB و یادآوری | ✅ | یکپارچه با Master/PreFlight |
| **CONTRACTS_ENGINEERING_ANALYSIS/REPORT** | تحلیل مهندسی قراردادها | ✅ | مرجع برای طراحی قراردادها |
| **CRITICAL_MODULE_SAFETY_CONTRACT.md** | ایمنی ماژول‌های حیاتی | ✅ مهم | NO BLIND CHANGES، سطوح Critical/High/Moderate |
| **NEW_CONTRACT_SUMMARY.md** | خلاصه قراردادهای جدید | ✅ | مرجع سریع |
| **PASTE_THIS_EVERY_CHAT.md** | یادآوری برای هر چت | ✅ | لینک به Execution + Safety + Quick V3 |

**نکات Contracts روت:**

- **AI_PREFLIGHT_INDEX** مسیر `PREFLIGHT_CHECKLIST.md` را بدون پوشهٔ PreFlight ذکر می‌کند؛ بهتر است مسیر کامل `Knowledge-Base/AI/PreFlight/PREFLIGHT_CHECKLIST.md` در Index درج شود.
- ارجاع‌های متعدد به `CONTRACTS/` به‌صورت کلی درست است؛ برای پرامپت‌های جدید می‌توان یک «فهرست مسیرهای دقیق» در INDEX یا در PASTE_THIS اضافه کرد.

---

### ۲.۳) Contracts/1015

| فایل | نقش | وضعیت | نکته |
|------|-----|--------|------|
| **CLINICAPP_APPOINTMENT_BOOKING_MODULE_PROMPT.md** | پرامپت ماژول نوبت‌دهی | ✅ تخصصی | قراردادها، نقش‌ها، Mission، قوانین، ورودی/خروجی |
| **CLINICAPP_CURSOR_7ROLES_CONTRACT_PROMPT_OPTIMIZED.md** | ۷ نقش بهینه‌شده برای Cursor | ✅ | هم‌راستا با CLINICAPP_PROMPT_MASTER |
| **CLINICAPP_RULES_SNAPSHOT_ONE_PAGE.md** | خلاصه یک‌صفحه‌ای قوانین | ✅ | برای چک سریع |

**نکته 1015:** در **CLINICAPP_APPOINTMENT_BOOKING_MODULE_PROMPT** به مسیرهایی مثل `/Docs/AI/**` و `/Docs/AI/CURSOR/**` ارجاع داده شده؛ در پروژهٔ فعلی ساختار `Contracts/Knowledge-Base/AI/` است. بهتر است این ارجاع‌ها به مسیر واقعی (مثلاً `Contracts/Knowledge-Base/AI/` و زیرپوشه‌ها) به‌روز شوند تا Cursor/AI بتواند همان فایل‌ها را باز کند.

---

### ۲.۴) Knowledge-Base / PreFlight

| فایل | نقش | وضعیت | نکته |
|------|-----|--------|------|
| **PREFLIGHT_CHECKLIST.md** | چک‌لیست پیش‌پرواز | ✅ اجباری | STEP 0 (۱۵ ممنوعیت)، STEP 1 (۱۲ دروازه)، بعد Hard Stop |
| **05-Debugging-Specialist-Contract.md** | قرارداد متخصص دیباگ | ✅ الزامی | ۶ مرحله، ۵ Whys، رفع اتمیک، ممنوع رفع کورکورانه |
| **Bugfix-Master-Contract.md** | قرارداد رفع باگ | ✅ | هم‌راستا با ۰۵ و DEBUGGING_MASTER_PROMPT |
| **ClinicApp_Knowledge_Base.md** | خلاصه پایگاه دانش | ✅ | مرجع سریع به Helpers و قراردادها |
| **COMPREHENSIVE_DEEP_REVIEW_REPORT.md** | گزارش بررسی عمیق | ✅ | الگوی گزارش‌نویسی برای ماژول‌ها |

**نکات PreFlight:**

- **PREFLIGHT_CHECKLIST** به `Contracts/04-AI-No-Fly-Zone.md` و `Contracts/05-AI-Guard-Prompt-Mandatory.md` و `Contracts/01-PreFlight-Protocol.md` ارجاع می‌دهد. اگر این فایل‌ها با نام/مسیر دیگری هستند (مثلاً محتوا داخل AI_PREFLIGHT_MASTER_V3 یا فایل‌های دیگر)، یا باید مسیرها اصلاح شوند یا یک خط «معادل در V3» در خود PREFLIGHT_CHECKLIST اضافه شود.
- **05-Debugging-Specialist-Contract** و **Bugfix-Master-Contract** و **DEBUGGING_MASTER_PROMPT** در Master هم‌هدف هستند؛ حفظ هر دو برای «قرارداد کوتاه» (PreFlight) و «پرامپت بلند» (Master) منطقی است؛ فقط در INDEX/README ذکر شود که ۰۵ منبع رسمی قرارداد دیباگ است و DEBUGGING_MASTER همان را به صورت پرامپت بسط می‌دهد.

---

## ۳. یکپارچگی و ارجاعات

- **جریان پیشنهادی:**  
  `PASTE_THIS_EVERY_CHAT` → `AI_EXECUTION_CONTRACT` → `CRITICAL_MODULE_SAFETY_CONTRACT` → `AI_PREFLIGHT_QUICK_V3` → در صورت مالی/باگ → `AI_PREFLIGHT_MASTER_V3` (STEP 2/3) و در صورت نیاز به Helper/استاندارد → `Master/INDEX` و فایل‌های ۰۱، ۰۳، ۰۵.
- **نقش‌ها:** در **CLINICAPP_PROMPT_MASTER** و **7ROLES** و **APPOINTMENT_BOOKING** یکسان‌سازی شده (معماری، کدریویو، امنیت، پزشکی، UX، دیتابیس و غیره).
- **خروجی:** قالب Findings / Risks / Plan / Diff / Tests / Rollback در PROMPT_MASTER و قراردادهای بررسی ماژول مشترک است.

---

## ۴. توصیه‌های اصلاحی (اولویت‌دار)

1. **مسیرها در INDEX و README (Master):**  
   - برای ۰۵ و CRITICAL-FINANCIAL و PREFLIGHT مسیر دقیق (نسبت به Master) درج شود، مثلاً:  
     `../PreFlight/05-Debugging-Specialist-Contract.md` و `PREFLIGHT_CHECKLIST.md`.
2. **PREFLIGHT_CHECKLIST (PreFlight):**  
   - ارجاع به `Contracts/04-AI-No-Fly-Zone.md` و مشابه آن؛ یا به فایل‌های واقعی تصحیح شود یا یک باکس «در V3: معادل در AI_PREFLIGHT_MASTER_V3.md → STEP 0» اضافه شود.
3. **CLINICAPP_APPOINTMENT_BOOKING_MODULE_PROMPT (1015):**  
   - مسیرهای `/Docs/AI/` و `/Docs/AI/CURSOR/` به ساختار فعلی (مثلاً `Contracts/Knowledge-Base/AI/` و اگر پوشه CURSOR معادل دارد) به‌روز شوند.
4. **یک فهرست مسیرهای حیاتی در یک جا:**  
   - در **AI_PREFLIGHT_INDEX** یا **PASTE_THIS** یک بخش کوتاه «مسیرهای دقیق قراردادها و PreFlight» اضافه شود تا هم انسان هم ابزار بتوانند یک بار مسیرها را از یک منبع بخوانند.

---

## ۵. جمع‌بندی

- **از اول تا آخر** همهٔ فایل‌های درخواستی بررسی شدند؛ محتوا هم‌جهت و قابل استفاده است.
- **قوی:** Master (۰۱–۰۸، Quick Ref، HelperExtensions)، قرارداد توسعه، PreFlight (چک‌لیست و ۰۵)، AI_EXECUTION و CRITICAL_MODULE_SAFETY و زنجیرهٔ PreFlight V3.
- **نیاز به به‌روزرسانی جزئی:** مسیرهای ارجاع در INDEX، README، PREFLIGHT_CHECKLIST و Appointment Booking prompt تا با ساختار واقعی پوشه‌ها یکسان شوند.

اگر بخواهید، می‌توانم متن دقیق اصلاحات (مثلاً برای INDEX، README و PREFLIGHT_CHECKLIST) را به صورت patch یا ویرایش فایل‌ها پیشنهاد بدهم.
