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

### پایگاه دانش (Contracts/Knowledge-Base/AI/Master)
- **INDEX.md** نقطهٔ ورود؛ 01 (DateTime)، 02 (Validation)، 03 (قرارداد توسعه)، 04 (TODO)، 05 (دیباگر)، 06 (Quick Reference).
- قبل از هر کار: قرارداد توسعه و در صورت ماژول جدید راهنمای TODO؛ برای باگ: قرارداد دیباگر (۶ مرحله، ۵ Whys، رفع اتمیک).

---

## ۳) استفاده در چت‌های بعدی

- برای **درک سریع پروژه**: همین سند + `Contracts/Knowledge-Base/AI/Master/INDEX.md` + `03-Development-Contract-Quick-Guide.md`.
- برای **داشبورد بیمار**: `Docs/PATIENT_DASHBOARD_ROADMAP.md` و `PATIENT_DASHBOARD_TECHNICAL_AUDIT.md`.
- برای **قوانین سخت و نقش‌ها**: `Contracts/1018/CLINICAPP_MASTER_CONTRACT_BRAIN_FOR_CURSOR.md` و `AI_CORE_COMMITMENT.md`.
- در صورت **اضافه شدن پوشهٔ واقعی CLINICAPP_CURSOR_ULTIMATE_PACK** به مخزن، این فایل را با مسیر دقیق آن فایل‌ها به‌روز کنید.

---

**نسخه:** 1.0  
**نویسنده:** بر اساس مطالعهٔ Docs و Contracts موجود در مخزن.
