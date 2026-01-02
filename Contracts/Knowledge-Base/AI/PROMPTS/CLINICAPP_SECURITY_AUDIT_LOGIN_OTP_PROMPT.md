# ClinicApp – Security & Audit Deep-Dive Prompt (Login/OTP, Enterprise-Grade) 🔒
> **هدف:** این پرامپت را به Cursor (یا هر AI IDE) بده تا ماژول ورود/ثبت‌نام OTP را **سیستماتیک** بررسی کند، **ریشه‌ی واقعی مشکل** را با شواهد پیدا کند، و **اصلاحات امن + تست‌شده** در سطح Enterprise (محیط درمانی) ارائه دهد — بدون حدس و بدون اتلاف وقت روی داکیومنت‌های غیرضروری.

---

## 0) Contract Lock (غیرقابل مذاکره)
- **قبل از هر پیشنهاد/تغییر:** فقط و فقط قراردادهای مرتبط در مسیرهای زیر خوانده شوند و به صورت خلاصه تأیید شوند:
  - `CONTRACTS/` (همه‌ی فایل‌ها)
  - هر قراردادی که مستقیم به: **Auth / OTP / Security / UI(View)** مربوط است
- **قوانین پروژه (Hard Rules):**
  1) قراردادهای `CONTRACTS/` الزاماً رعایت شوند.
  2) قبل از هر تغییر، **Preflight Checklist** اجرا و نتیجه گزارش شود.
  3) Entity → ViewModel فقط با **Factory Method** (نه مپ داخل Controller).
  4) تمام خروجی‌ها با **ServiceResult Enhanced** (نه raw object).
  5) هر تغییر باید **تست مرتبط + پلن تایید** داشته باشد.
- **قواعد مطلق:**
  - حدس ممنوع. اگر چیزی معلوم نیست، دقیقاً بگو چه اطلاعاتی کم است.
  - قبل از ساخت کلاس/سرویس جدید، اول **Search** کن؛ چیزی که هست را تکرار نکن.
  - معماری، پوشه‌بندی، naming، conventions فعلی پروژه باید حفظ شود.

---

## 1) Scope
### Module Under Review
- **Module:** Login + Registration via OTP (Passwordless)
- **Entry points (حداقل):**
  - `Controllers/AccountController.cs`
  - `Services/AuthService.cs`
  - `Models/Core/OtpRequest.cs`
  - هر فایل مرتبط با: `ClientProvider`, `RateLimiter`, `OtpStateStore`, `ApplicationUserManager`, `SignIn`, Filters/Middleware

### هدف خروجی
1) تشخیص اینکه آیا **سوابق ورود** و **مشخصات امنیتی کاربر** (IP, User-Agent, Device/Session, نتیجه ورود، Lockout، RateLimit، OTP lifecycle) مطابق روال تیم‌های حرفه‌ای پیاده شده یا خیر.
2) اگر ناقص است: **Roadmap + TODO list** اولویت‌بندی‌شده برای تکمیل در سطح جهانی (Enterprise).
3) رفع مشکلات فعلی ماژول (Bug/UX/Security/Architecture) با تغییرات کوچک و امن، همراه با تست.

---

## 2) سرعت بالا (چطور جلوی هدررفت زمان روی داکیومنت را بگیری)
**الزامی برای AI:**
- به‌جای خواندن کل داکیومنت، ابتدا با جستجو، تنها فایل‌های مرتبط را پیدا کن:
  - `rg -n "OtpRequest|OtpRequests|OtpState|RateLimit|ClientIp|UserAgent|Lockout|AccessFailed|SignIn|Audit|LoginHistory|Security" .`
- فقط زمانی سراغ داکیومنت برو که:
  - قراردادی وجود دارد که روی تصمیم اثر می‌گذارد، یا
  - ابهام معماری/قواعد پروژه با کد قابل حل نیست.
- خروجی باید **روی 3–7 مسئله‌ی مهم** تمرکز کند (Noise ممنوع).

---

## 3) فرآیند سیستماتیک (الزامی)
### STEP A — Preflight
- قراردادهای مرتبط را لیست کن و بگو خوانده شد.
- وضعیت ریسک تغییرات: Critical/High/Medium/Low
- وضعیت تست‌ها: آیا پروژه تست دارد؟ مسیر/فریمورک تست چیست؟

### STEP B — Module Map (با معماری فعلی)
- مسیرهای MVC / اکشن‌های مربوط به ورود/ثبت‌نام
- سرویس‌ها و helper ها
- مدل‌های دیتابیس (OTP logs، User، …)
- View / Layout اختصاصی login (اگر باید داشته باشد)
- وابستگی‌های مستقیم و غیرمستقیم

### STEP C — Security & Audit Baseline (Enterprise Checklist)
برای هر مورد: **وضعیت فعلی (Exists/Partial/Missing) + Evidence (فایل/لاین) + ریسک**
- ✅/❌ **Sign-in audit trail:** ثبت رخدادهای ورود/خروج/شکست/قفل‌شدن
- ✅/❌ **IP & User-Agent logging:** ذخیره IP/UA در لاگ دیتابیس (نه فقط در session)
- ✅/❌ **Session binding:** بستن OTP به IP/UA (وجود دارد؟ چطور؟)
- ✅/❌ **Rate limiting:** روی IP و روی شناسه کاربر (وجود دارد؟ پارامترها؟)
- ✅/❌ **Brute-force protection:** AccessFailed/Lockout و سقف تلاش‌ها
- ✅/❌ **Replay protection:** OTP یکبارمصرف + invalidation
- ✅/❌ **Sensitive data handling:** عدم ذخیره OTP خام، ماسک کردن شماره‌ها در لاگ
- ✅/❌ **Monitoring-ready logs:** correlationId/traceId، سطح severity، کد خطاهای استاندارد
- ✅/❌ **Admin visibility:** صفحه/گزارش برای مشاهده رخدادهای امنیتی (اختیاری ولی Enterprise)

### STEP D — Critical Findings (3 تا 7 مورد)
- فقط موارد **ریشه‌ای و مهم** را لیست کن:
  - Security holes
  - داده‌های audit ناقص
  - boundary violation
  - ناهماهنگی با ServiceResult/Factory
  - مشکلات UX که در ورود اختلال ایجاد می‌کند

### STEP E — Root Cause (Evidence-based)
- برای هر یافته: ریشه را توضیح بده و چرا گزینه‌های دیگر ریشه نیستند.

### STEP F — Fix Plan (Ranked, Minimal & Safe)
- راهکارها را رتبه‌بندی کن (1 بهترین)
- تغییرات کم‌ریسک و incremental
- reuse از کدهای موجود (قبلش search)

### STEP G — Implementation (diff-style)
- لیست فایل‌ها و تغییرات دقیق
- snippet های حداقلی
- رعایت: Factory Method + ServiceResult Enhanced + conventions

### STEP H — Tests & Verification
- unit/integration tests پیشنهادی
- سناریوهای کلیدی: OTP ارسال/تایید، mismatch IP/UA، rate limit، lockout، replay
- پلن دستی: قدم به قدم

### STEP I — Rollback/Safety
- rollback steps
- اگر ریسک بالا: feature flag یا config toggle

### STEP J — Open Questions
- فقط سوالاتی که واقعاً blocking هستند.

---

## 4) خروجی نهایی (Format سخت‌گیرانه)
1) Preflight Result  
2) Module Map  
3) Dependency/Impact Graph  
4) Enterprise Security/Audit Baseline (Exists/Partial/Missing + Evidence)  
5) Critical Issues (Evidence)  
6) Root Cause Analysis  
7) Fix Plan (Ranked)  
8) Implementation Details (Diff snippets)  
9) Test Plan  
10) Verification Steps  
11) Rollback Strategy  
12) Open Questions / Missing Info  

---

## 5) نکته‌ی کلیدی مخصوص ClinicApp (بر اساس کد فعلی)
> **AI باید این را به عنوان سرنخ بررسی کند (نه پیش‌فرض).**
- در `AuthService`، IP و User-Agent از طریق `ClientProvider` گرفته و داخل `OtpState` ذخیره می‌شود fileciteturn15file6L25-L45 و اعتبارسنجی OTP با **Session binding** به IP/UA انجام می‌شود fileciteturn15file8L19-L22.
- اما مدل دیتابیس `OtpRequest` فعلاً فقط `PhoneNumber`, `OtpCodeHash`, زمان و ... را ذخیره می‌کند و **فیلد IP/UserAgent ندارد** fileciteturn15file4L14-L42.
- همچنین لاگ دیتابیس در بعضی نقاط به شکل ساده ساخته شده (مثلاً `new OtpRequest { PhoneNumber, OtpCodeHash }`) که می‌تواند باعث از دست رفتن داده‌های audit شود fileciteturn15file6L47-L50.
- یک کلاس `SecurityAuditEntry` با فیلدهای `IpAddress` و `UserAgent` در اینترفیس امنیتی وجود دارد fileciteturn14file4L25-L36؛ باید بررسی شود آیا واقعاً در جریان ورود استفاده می‌شود یا فقط تعریف شده است.

**بنابراین هدف بررسی:** مشخص کردن اینکه «ثبت سوابق ورود» واقعاً در DB و قابل گزارش‌گیری وجود دارد یا خیر، و اگر ناقص است، طراحی حداقلی برای تکمیل آن (بدون شکستن معماری).

---

## 6) ورودی که من (کاربر) به Cursor می‌دهم
- مشکل فعلی/هدف UI: «Layout اختصاصی login، موبایل‌فرست، رسمی-درمانی، سبک و سریع، AJAX محور، بدون رنگ/انیمیشن جلف، validation سمت کلاینت و سرور»
- مسیر فایل‌ها/پوشه‌های مرتبط: (اینجا لیست کن)
- خطا/رفتار اشتباه فعلی: (اینجا بنویس)
- محدودیت زمانی: «خروجی actionable با کمترین تغییرات لازم»

---

**END**  
