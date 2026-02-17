# فهرست و نقشهٔ CLINICAPP_CURSOR_ULTIMATE_PACK و درک پروژه

**تاریخ:** ۱۴۰۴/۱۱/۲۷  
**وضعیت:** پوشهٔ `CLINICAPP_CURSOR_ULTIMATE_PACK` در مخزن فعلی یافت نشد؛ این سند نقشهٔ معادل‌ها و خلاصهٔ درک پروژه را بر اساس اسناد موجود ثبت می‌کند.

---

## ۱) نقشهٔ فایل‌های ULTIMATE_PACK به اسناد موجود

| # | فایل در پک | معادل / مرجع در پروژه | توضیح کوتاه |
|---|-------------|------------------------|--------------|
| 1 | `00_BOOT_ULTRA.md` | `Contracts/AI_CORE_COMMITMENT.md` + `Contracts/1018/CLINICAPP_MASTER_CONTRACT_BRAIN_FOR_CURSOR.md` | قوانین راه‌اندازی، Token Policy، تعهدات AI، ممنوعیت‌ها |
| 2 | `01_MASTER_CONTEXT.md` | `Contracts/Knowledge-Base/AI/Master/INDEX.md` + همان MASTER CONTRACT | پایگاه دانش، فهرست Helpers و قراردادها |
| 3 | `02_FLOW_DISCIPLINE_CONTRACT.md` | `Contracts/Knowledge-Base/AI/Master/03-Development-Contract-Quick-Guide.md` | انضباط جریان، SRP، Strongly-Typed، رنگ‌بندی |
| 4 | `03_ULTRA_FOCUS_EXECUTION.md` | `AI_CORE_COMMITMENT.md` (NO BLIND CHANGES) + `05-Debugging-Specialist-Contract.md` | اجرای متمرکز، رفع اتمیک، بدون Fix کورکورانه |
| 5 | `10_BEAST_MODULE_REVIEW_BUILD.md` | `Docs/PATIENT_DASHBOARD_TECHNICAL_AUDIT.md` + ماژول‌های دیگر در `Docs/` | بررسی عمیق ماژول، معماری، Performance، Security |
| 6 | `11_SAFE_MODULE_FIX.md` | `Contracts/Knowledge-Base/AI/Master/05-Debugging-Specialist-Contract.md` | رفع امن و مرحله‌ای، تحلیل علت ریشه‌ای |
| 7 | `12_FLOW_INTEGRITY_PROMPT.md` | قرارداد توسعه + مسیرهای احراز هویت و نوبت‌گیری | یکپارچگی جریان (Auth، Booking، Payment) |
| 8 | `20_VIEW_UI_BEAST_MODE.md` | `03-Development-Contract-Quick-Guide.md` (بخش UI/رنگ/فرم) | استانداردهای View و UI، پالت پزشکی |
| 9 | `21_MAIN_MENU_EXECUTION.md` | `Docs/NAVIGATION-ROADMAP.md`، منو در Layoutها | منوی اصلی و ناوبری |
| 10 | `30_PATIENT_DASHBOARD_ROADMAP.md` | `Docs/PATIENT_DASHBOARD_ROADMAP.md` | نقشهٔ راه داشبورد بیمار، فازها و چک‌لیست |
| 11 | `40_AUTH_STATE_DEBUG.md` | `Docs/OTP_*`، `PATIENT_AREA_AUTH_GUIDE`، AccountController | وضعیت احراز هویت، OTP، دیباگ لاگین |
| 12 | `50_FINAL_PREDEPLOY_AUDIT.md` | `Docs/PATIENT_DASHBOARD_TECHNICAL_AUDIT.md`، چک‌لیست‌های Docs | حسابرسی نهایی قبل از استقرار |
| 13 | `60_SCENARIO_MATRIX_TEMPLATE.md` | سناریوهای مستند در `Docs/` (مثلاً لغو نوبت، پرداخت) | قالب سناریوها و ماتریس حالت‌ها |
| 14 | `61_FLOW_STATE_MACHINE_TEMPLATE.md` | جریان‌های رزرو/پرداخت/اعلان در کد و Docs | ماشین حالت و جریان‌های اصلی |
| 15 | `62_PRODUCTION_READINESS_CHECKLIST.md` | انتهای `PATIENT_DASHBOARD_TECHNICAL_AUDIT` و چک‌لیست‌های Docs | چک‌لیست آمادگی تولید |
| 16 | `README_INDEX.md` | این فایل + `Contracts/Knowledge-Base/AI/Master/INDEX.md` | فهرست کلی و نقطهٔ ورود |

---

## ۲) خلاصهٔ درک پروژه (بر اساس اسناد مطالعه‌شده)

### معماری و لایه‌ها
- **ASP.NET MVC5 + EF6**؛ محیط درمانی رسمی (Medical Production).
- **Controller → Service → Repository**؛ View فقط نمایش، بدون منطق کسب‌وکار.
- خروجی سرویس‌ها با **ServiceResult Enhanced**؛ Entity → ViewModel با **Factory Method**.
- **Reuse-first**: قبل از ساخت هر چیز جدید، جستجو برای استفاده از Helper/الگوی موجود.

### قرارداد توسعه (از 03-Development-Contract-Quick-Guide)
- **رنگ‌ها:** فقط پالت `--medical-*` (آبی درمانی، سبز، قرمز و …)؛ بدون گرادینت/رنگ‌های جیق.
- **Strongly-Typed:** ViewModel برای همه؛ بدون Anonymous در خروجی کنترلر.
- **تاریخ:** استاندارد شمسی؛ Helperها و DatePicker طبق `01-Helpers-DateTime.md`.
- **اعتبارسنجی:** کد ملی، موبایل و غیره از Helpers مستند.
- **پیام‌ها:** `NotificationHelper.SetSuccess/SetError` و مشابه؛ یکسان در کل پروژه.

### قوانین سخت (از AI_CORE_COMMITMENT و MASTER CONTRACT)
- ممنوع: حدس بدون شواهد، Fix کورکورانه، تغییر بدون تست، Hard Delete در دادهٔ پزشکی/مالی، پول با float/double، POST حساس بدون Anti-Forgery، افشای دادهٔ حساس در UI/Log.
- ماژول‌های حیاتی (Auth، Patient، Financial، DB): **STOP → درک منطق → تأیید کاربر → تحلیل اثر → پیشنهاد → پیاده‌سازی → تأیید نهایی**.

### داشبورد بیمار
- تب‌ها: خانه (Overview)، پروفایل، نوبت‌ها، پرونده پزشکی، تنظیمات.
- Overview: یک درخواست **GetOverview** (آمار + نوبت‌های اخیر/آینده + پذیرش‌ها)؛ تلاش مجدد خودکار در صورت خطای سکشن.
- **GetCurrentPatientIdAsync** با کش درخواستی (HttpContext.Items) تا تکرار کوئری نشود.
- پروفایل/تنظیمات از طریق API و فرم با AntiForgery.

### احراز هویت و Patient Area
- نقش Patient؛ کنترلرهای Patient از BasePatientController با GetCurrentPatientIdAsync.
- OTP و جریان ورود/ثبت‌نام در Docs و AccountController مستند شده است.

### مالی و نوبت
- مبالغ فقط **decimal**؛ ماژول مالی بدون Code Review/گیت تغییر نمی‌کند.
- لغو نوبت: تا ۲ ساعت قبل؛ سناریو اعتبار/کیف پول در `Docs/APPOINTMENT_CANCELLATION_SCENARIO.md`.
- اعلان: صف اعلان + Hangfire؛ بعد از Commit پرداخت، Enqueue و پردازش در پس‌زمینه.

### پایگاه دانش (Contracts/Knowledge-Base/AI)
- نقطهٔ ورود: **Master/README.md** و **Master/INDEX.md**.
- الزامی قبل از هر پاسخ: **PreFlight/PREFLIGHT_CHECKLIST.md** (۱۵ قانون ممنوعه، ۱۲ دروازه امنیتی، Hard Stop، ساختار اجباری پاسخ).
- الزامی توسعه: **03** (قرارداد)، **04** (TODO)، **05** (دیباگر)؛ برای باگ: ۶ مرحله + ۵ Whys + رفع اتمیک؛ **ممنوع رفع کورکورانه**.

---

## ۳) نقشهٔ کامل Contracts\Knowledge-Base\AI (۲۹ فایل)

### داخل **AI** (مستقیم — ۳ فایل)
| فایل | نقش |
|------|-----|
| `CLINICAPP_MODULE_REVIEW_PROMPT.md` | پرامپت بررسی ماژول (یک ماژول، فقط مسائل بحرانی، شواهد، رفع امن). |
| `MAIN_MENU_BEAST_ANALYSIS.md` | تحلیل منوی اصلی، جریان «رزرو نوبت»، Mobile-First، Flow Integrity (returnUrl)، مسائل و طرح پیاده‌سازی. |
| `OPTIMIZATION_REVIEW_AND_ACTION_PLAN.md` | بررسی قراردادها و برنامه بهینه‌سازی (Flow Discipline، View/UI Beast، Main Menu و غیره). |

### **AI\Master** (۱۸ فایل) — هستهٔ پایگاه دانش
| فایل | نقش |
|------|-----|
| `README.md` | راهنمای کلی پایگاه دانش؛ فهرست الزامات (PreFlight، 03، 04، 05)، مسیر یادگیری، FAQ. |
| `INDEX.md` | فهرست کامل؛ دسترسی سریع به هر Helper/قرارداد؛ جستجو بر اساس موضوع؛ آمار؛ مسیر یادگیری. |
| `01-Helpers-DateTime.md` | تاریخ و زمان: PersianDateHelper، DatePicker، Extensionها، Parse در Controller، **DatePicker داخل مودال** (z-index، container، startWatchAgain). |
| `02-Helpers-Validation.md` | اعتبارسنجی: کد ملی، موبایل، Identity، ValidationResult، SecurityValidationResult. |
| `03-Development-Contract-Quick-Guide.md` | **قرارداد توسعه**: رنگ‌های --medical-*، Strongly-Typed، Bulletproof، SRP، پیام‌ها، DatePicker، آپلود، CKEditor، فرم‌های درمانی، **Checklist نهایی قبل از Commit**. |
| `04-TODO-Implementation-Guide.md` | **راهنمای TODO**: ۱۳ فاز پیاده‌سازی (تحلیل، Backend، Controller، View، UI، رنگ، اعلان، DatePicker، CKEditor، آپلود، فرم، تست، استقرار)، Template، زمان‌بندی. |
| `06-Quick-Reference.md` | مرجع سریع: جدول Helpers، Use Case → Helper، مثال‌های یک‌خطی. |
| `08-MVC-Routing-Best-Practices.md` | **روتینگ MVC**: ترتیب Route (خاص قبل از عمومی)، UseNamespaceFallback، درس‌های واقعی پروژه. |
| `HelperExtensionsGuide.md` | جعبه ابزار: ۵ Extension + ۸ Helper، ۱۰۰+ متد (String، DateTime، عدد، Collection، Cache، امنیت، فایل، تصویر، HTML/URL). |
| `05-Debugging-Specialist-Contract.md` | در **PreFlight** — متن معادل در Master هم ارجاع دارد. |
| `ClinicApp – Ultra-Lean Module Review Prompt.md` | پرامپت بررسی ماژول (لین). |
| `CLINICAPP_MODULE_REVIEW_PROMPT.md` | پرامپت بررسی ماژول (نسخهٔ دیگر). |
| `CLINICAPP_PROMPT_MASTER.md` | **Prompt Master**: قانون طلایی، قرارداد خروجی (Assumptions, Findings, Risks, Plan, Diff, Tests, Rollback)، ۷ نقش (معمار، Code Reviewer، MVC، امنیت، پزشکی، UX، DB)، قراردادهای Critical (C1–C5). |
| `CURSOR_MODULE_REVIEW_CONTRACT.md` | قرارداد بررسی ماژول برای Cursor: Preflight، نقشهٔ ماژول/وابستگی، حداکثر ۵ مسئله بحرانی، Root Cause، Fix کم‌دامنه، تست؛ ممنوع حدس و رفع قبل از ریشه. |
| `DEBUGGING_MASTER_PROMPT.md` | پرامپت دیباگ (فرآیند اجباری). |
| `SYSTEM NOTE — DEBUG CONTRACT LOCK.md` | قفل قرارداد دیباگ (یادآوری سیستم). |

### **AI\PreFlight** (۶ فایل) — دروازهٔ امن و ساختار پاسخ
| فایل | نقش |
|------|-----|
| `PREFLIGHT_CHECKLIST.md` | **الزامی قبل از هر پاسخ**: STEP 0 (۱۵ قانون ممنوعه)، STEP 1 (۱۲ دروازه امنیتی)، STEP 2 (چک‌لیست قبل از پاسخ)، STEP 3 (Hard Stop)، STEP 4 (ساختار اجباری Bugfix/Module/Feature)، STEP 5–7؛ در صورت تعارض → Hard Stop. |
| `05-Debugging-Specialist-Contract.md` | **متخصص دیباگر**: ۶ مرحله (شناسایی، Root Cause با ۵ Whys، وابستگی‌ها، رفع اتمیک، تست، گزارش)； **ممنوع رفع کورکورانه**؛ دسته‌بندی خطا و استراتژی رفع. |
| `Bugfix-Master-Contract.md` | **قرارداد Bugfix**: ۱۰ نقش همزمان، قیود ثابت (MONEY decimal، EF6، PATTERNS، NO_DELETE، LOGGING)، فرآیند A→F (کشف شواهد، ریشه، گزینه‌ها، Patch اتمیک، تأیید دستی، گزارش)، جدول دسته‌بندی خطا، الگوهای Facade/Converter. |
| `ClinicApp_Knowledge_Base.md` | خلاصهٔ پایگاه دانش: ماژول‌های بررسی‌شده، ساختار پروژه، Helpers/Extensions، Seed، مدل‌های کلیدی. |
| `COMPREHENSIVE_DEEP_REVIEW_REPORT.md` | گزارش بررسی عمیق. |

### **AI\View** (۱ فایل)
| فایل | نقش |
|------|-----|
| `CLINICAPP_FINAL_VIEW_REVIEW_OPTIMIZATION_PROMPT.md` | پرامپت بررسی و بهینه‌سازی نهایی View/UI. |

### **AI\DB** (۱ فایل)
| فایل | نقش |
|------|-----|
| `Database-Connection-Guide.md` | راهنمای اتصال: Connection String، SSMS، sqlcmd، دستورات مفید، اسکریپت‌ها. |

### **AI\PROMPTS** (۱ فایل)
| فایل | نقش |
|------|-----|
| `Google-Grade Root Cause Analysis Contract.md` | قرارداد تحلیل علت ریشه‌ای سطح Enterprise: ۵ مرحله (Problem Reframing، System Mapping، Evidence، Root Cause، Solution Design)، بدون حدس، اولویت داده/امنیت/سازگاری. |

### **AI\ARCH** (۱ فایل)
| فایل | نقش |
|------|-----|
| `CLINICAPP_SCENARIO_MATRIX_TEMPLATE.md` | **قالب ماتریس سناریو**: Flow Overview، Actor، شاخه‌های اجباری (احراز هویت، شبکه، ناوبری، تکمیل جزئی، اعتبار و خطا، حفظ زمینه)، جدول سناریو؛ «سناریوی گم‌شده = باگ». |

### **AI\REVIEWS** (۲ فایل)
| فایل | نقش |
|------|-----|
| `CLINICAPP_VIEW_REVIEW_CHECKLIST.md` | چک‌لیست یک‌صفحه‌ای بررسی View: Healthcare UI، SRP، AJAX، Validation، ServiceResult، Reuse، Performance، امنیت، RTL/دسترسی. |
| `Bugfix-Master-Contract.md` | کپی/ارجاع به قرارداد Bugfix (PreFlight). |

---

## ۴) استفاده در چت‌های بعدی

- **درک سریع پروژه:** همین سند + `Contracts/Knowledge-Base/AI/Master/INDEX.md` + `03-Development-Contract-Quick-Guide.md`.
- **داشبورد بیمار:** `Docs/PATIENT_DASHBOARD_ROADMAP.md` و `PATIENT_DASHBOARD_TECHNICAL_AUDIT.md`.
- **قوانین سخت و نقش‌ها:** `Contracts/1018/CLINICAPP_MASTER_CONTRACT_BRAIN_FOR_CURSOR.md` و `AI_CORE_COMMITMENT.md`.
- **قبل از هر پاسخ AI:** `Contracts/Knowledge-Base/AI/PreFlight/PREFLIGHT_CHECKLIST.md` (دروازه‌ها، Hard Stop، ساختار پاسخ).
- **رفع باگ:** `PreFlight/05-Debugging-Specialist-Contract.md` + `Bugfix-Master-Contract.md`؛ بدون رفع کورکورانه.
- **بررسی ماژول:** `Master/CURSOR_MODULE_REVIEW_CONTRACT.md` یا `CLINICAPP_PROMPT_MASTER.md`.
- **سناریو و جریان:** `ARCH/CLINICAPP_SCENARIO_MATRIX_TEMPLATE.md`؛ **View:** `REVIEWS/CLINICAPP_VIEW_REVIEW_CHECKLIST.md`.
- در صورت **اضافه شدن پوشهٔ واقعی CLINICAPP_CURSOR_ULTIMATE_PACK** به مخزن، این فایل را با مسیر دقیق آن فایل‌ها به‌روز کنید.

---

**نسخه:** 1.1  
**آخرین به‌روزرسانی:** نقشهٔ کامل ۲۹ فایل پایگاه دانش `Contracts\Knowledge-Base\AI` اضافه شد.  
**نویسنده:** بر اساس مطالعهٔ Docs و Contracts موجود در مخزن.
