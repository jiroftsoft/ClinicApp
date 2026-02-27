# گزارش بررسی عمیق: شروع جلسه صندوق (StartSession)

**مسیر:** `POST /PosManagement/StartSession`  
**تاریخ بررسی:** ۱۴۰۴/۱۲/۰۷  
**چارچوب:** معمار نرم‌افزار، بک‌اند ارشد، امنیت، DBA، یکپارچگی مالی، SRP، ضد تقلب

---

## ۱. خلاصه اجرایی

| موضوع | وضعیت | اولویت |
|--------|--------|--------|
| جلوگیری از جلسه همزمان (Race) | ⚠️ آسیب‌پذیر | P1 |
| ثبت Audit در جدول CashSessionAuditLog | ❌ انجام نمی‌شود | P1 |
| تراکنش دیتابیس برای Check+Insert | ❌ وجود ندارد | P1 |
| ذخیره فیلد Description کاربر | ❌ در Entity ذخیره نمی‌شود | P2 |
| RowVersion / Optimistic Concurrency روی CashSession | ❌ وجود ندارد | P2 |
| ایندکس یکتا «یک جلسه باز per user» | ❌ وجود ندارد | P2 |
| SRP و جداسازی منطق در Controller | ✅ قابل قبول؛ بهبود جزئی ممکن | P3 |
| امنیت OWASP (CSRF, اعتبارسنجی ورودی) | ✅ CSRF فعال؛ اعتبارسنجی سمت سرور | P3 |

---

## ۲. مشکلات معماری (Software Architect)

### ۲.۱ نقض احتمالی SRP

- **Controller:** مسئولیت‌ها: دریافت مدل، فراخوانی Validator، حل UserId (با fallback)، فراخوانی سرویس، بازگرداندن View/Redirect. **ارزیابی:** منطق کسب‌وکار در سرویس است؛ فقط «حل UserId» در کنترلر می‌ماند که می‌توان به یک سرویس یا Extension منتقل کرد.
- **Service (StartCashSessionAsync):** هم «قانون کسب‌وکار» (حداکثر یک جلسه باز) و هم «ساخت موجودیت و ذخیره» را انجام می‌دهد. **ارزیابی:** قابل قبول؛ در صورت تمایل می‌توان «قانون یک جلسه باز» را در یک Domain Service یا در Repository با تراکنش واحد متمرکز کرد.
- **Repository:** `HasActiveSessionAsync` و `AddAsync` جدا هستند؛ هیچ متد اتمیک `TryStartSessionIfNoneActiveAsync` وجود ندارد. **نتیجه:** برای رعایت بهتر SRP و اتمیسیته، یک عملیات واحد در Repository پیشنهاد می‌شود.

### ۲.۲ جریان داده و لایه‌ها

- ورودی از View فقط `InitialCashAmount` و `Description` است؛ `Description` در Entity ذخیره نمی‌شود (در CashSession فقط پراپرتی محاسبه‌شده `Description` وجود دارد). **پیشنهاد:** در صورت نیاز به ذخیره توضیحات کاربر، فیلد `Notes` یا `UserDescription` در جدول CashSessions و در Entity اضافه شود.

---

## ۳. همزمانی و یکپارچگی مالی (Back-end + DBA)

### ۳.۱ Race: دو جلسه باز همزمان برای یک کاربر

**وضعیت فعلی:**

1. در **GET** و در **POST** هر دو بار `GetActiveCashSessionAsync` / `HasActiveSessionAsync` فراخوانی می‌شود.
2. بین «بررسی عدم وجود جلسه فعال» و «درج جلسه جدید» هیچ قفل یا تراکنش مشترکی وجود ندارد.
3. دو درخواست همزمان می‌توانند هر دو «بدون جلسه فعال» ببینند و هر دو `AddAsync` را انجام دهند → **دو جلسه باز برای یک کاربر**.

**راه‌حل‌های پیشنهادی (حداقل یکی لازم است):**

| روش | توضیح | سختی |
|-----|--------|------|
| **A. تراکنش + قفل** | در Repository متد `TryStartSessionAsync(userId, ...)` که در یک تراکنش با سطح ایزوله مناسب (مثلاً Serializable) یا با `UPDLOCK` روی ردیف‌های فعال کاربر، ابتدا وجود جلسه باز را چک کند و فقط در صورت نبود، INSERT کند. | متوسط |
| **B. ایندکس یکتای فیلترشده** | در SQL Server: `CREATE UNIQUE INDEX IX_CashSession_OneActivePerUser ON CashSessions(UserId) WHERE ClosedAt IS NULL AND IsDeleted = 0`. در صورت درج دومین جلسه باز برای همان کاربر، دیتابیس خطای نقض یکتا می‌دهد؛ سرویس باید این استثنا را بگیرد و با پیام مناسب به کاربر برگرداند. | کم |
| **C. ترکیب A+B** | هم تراکنش در اپلیکیشن هم ایندکس یکتا در DB برای دفاع چندلایه. | توصیه برای محیط مالی |

### ۳.۲ تراکنش دیتابیس

- **وضعیت:** `AddAsync` فقط `SaveChangesAsync` را صدا می‌زند؛ هیچ `BeginTransaction` برای «بررسی + درج» وجود ندارد.
- **پیشنهاد:** کل «بررسی HasActive + درج جلسه» داخل یک تراکنش با محدودیت ایزوله یا با استفاده از متد اتمیک Repository (مثل `TryStartSessionAsync`) انجام شود تا در محیط رقابتی دو جلسه باز ایجاد نشود.

### ۳.۳ RowVersion و Optimistic Concurrency

- **وضعیت:** موجودیت `CashSession` فیلد `RowVersion` / `Timestamp` ندارد (بر خلاف مثلاً `Reception`, `OnlinePayment`).
- **اثر:** برای عملیات «بستن جلسه» از `TryCloseSessionConditionalAsync` با `UPDATE ... WHERE ClosedAt IS NULL` استفاده شده که از double-close جلوگیری می‌کند؛ بنابراین برای «بستن» همزمانی تا حد زیادی کنترل شده است.
- **پیشنهاد:** برای یکنواختی با بقیه ماژول‌های حساس و برای به‌روزرسانی‌های آینده (مثلاً تعدیل مانده)، اضافه کردن `RowVersion` به `CashSession` و استفاده از آن در UPDATEها توصیه می‌شود (P2).

---

## ۴. امنیت (OWASP / Security Specialist)

### ۴.۱ موارد انجام‌شده

- **CSRF:** `[ValidateAntiForgeryToken]` روی POST وجود دارد.
- **اعتبارسنجی ورودی:** FluentValidation و DataAnnotations؛ محدودیت مبلغ (مثلاً حداکثر ۱۰۰ میلیون) و طول Description اعمال می‌شود.
- **احراز هویت:** کنترلر با `[Authorize(Roles = ...)]` محدود به نقش‌های مجاز است.
- **لاگ حساس:** مبلغ اولیه و UserId در Serilog ثبت می‌شود (برای Audit؛ باید مطمئن شد لاگ در محیط پروداکشن به‌صورت امن نگهداری می‌شود).

### ۴.۲ پیشنهادات

- **Idempotency (اختیاری):** در صورت نیاز برای جلوگیری از ارسال دوباره فرم و ایجاد تصادفی دو جلسه، می‌توان یک کلید یکبارمصرف (مثلاً از سمت کلاینت یا توکن سرور) استفاده کرد و در سمت سرور «شروع جلسه» را برای آن کلید فقط یک بار قبول کرد.
- **محدودیت نرخ (Rate Limit):** برای اکشن `StartSession` (و در کل برای عملیات حساس مالی) اعمال Rate Limit برای کاهش خطر سوءاستفاده و اتوماسیون توصیه می‌شود.

---

## ۵. Audit Trail و ضد تقلب

### ۵.۱ وضعیت فعلی

- **Serilog:** پیام‌های ساخت‌یافته شامل `AUDIT StartSession` با SessionId, UserId, InitialAmount, OpenedAt ثبت می‌شود.
- **جدول CashSessionAuditLog:** وجود دارد و سرویس `ICashSessionAuditService.LogActionAsync` برای ثبت اقدامات (مثل Open, Close) طراحی شده است، اما **هیچ فراخوانی از PosManagementService.StartCashSessionAsync به این سرویس انجام نمی‌شود.**

### ۵.۲ پیشنهاد

- بلافاصله پس از ایجاد موفق جلسه، یک رکورد Audit با `Action = "Open"` و مقادیر مرتبط (مثلاً NewValue شامل OpeningBalance، UserId، OpenedAt و در صورت وجود فیلد، Description/Notes) در `CashSessionAuditLog` از طریق `ICashSessionAuditService.LogActionAsync` ثبت شود تا ردیابی ضد تقلب و ممیزی مالی کامل شود.

---

## ۶. بهینه‌سازی پایگاه داده (DBA)

### ۶.۱ ایندکس‌های موجود

- بر اساس پیکربندی EF: ایندکس روی `UserId`, `OpenedAt`, `ClosedAt`, `Status`, `IsDeleted`, `CreatedAt` و ایندکس ترکیبی `(UserId, Status, OpenedAt)` وجود دارد.

### ۶.۲ پیشنهادات

1. **ایندکس یکتای فیلترشده (جلوگیری از دو جلسه باز per user):**  
   `CREATE UNIQUE NONCLUSTERED INDEX IX_CashSession_OneActivePerUser ON dbo.CashSessions(UserId) WHERE (ClosedAt IS NULL AND IsDeleted = 0);`  
   این ایندکس هم قید یکتایی را در دیتابیس اعمال می‌کند هم کوئری «آیا این کاربر جلسه باز دارد؟» را می‌تواند با Seek کارآمد پشتیبانی کند.

2. **کارایی HasActiveSessionAsync:** با وجود ایندکس بالا، کوئری `AnyAsync(cs => !cs.IsDeleted && cs.UserId == userId && (Status == Active/Open) && cs.ClosedAt == null)` از ایندکس فیلترشده بهره می‌برد.

3. **RowVersion:** در صورت اضافه شدن به جدول CashSessions، یک ستون `rowversion` (یا `timestamp`) برای Optimistic Concurrency در به‌روزرسانی‌های بعدی (بستن، تعدیل) مفید است.

---

## ۷. جمع‌بندی اقدامات پیشنهادی

### P1 (باید انجام شود)

1. **جلوگیری از Race:** پیاده‌سازی یکی از موارد زیر (ترجیحاً هر دو):  
   - متد اتمیک در Repository مثلاً `TryStartSessionAsync` که در یک تراکنش، بررسی «عدم جلسه باز» + درج را انجام دهد؛ یا  
   - ایندکس یکتای فیلترشده روی `(UserId) WHERE ClosedAt IS NULL AND IsDeleted = 0` و هندل کردن استثنای نقض یکتا در سرویس با پیام مناسب.
2. **ثبت Audit در CashSessionAuditLog:** پس از ایجاد موفق جلسه، فراخوانی `ICashSessionAuditService.LogActionAsync(cashSessionId, "Open", null, new { OpeningBalance, ... }, reason)` با توضیح/دلیل مناسب.

### P2 (بهبود)

3. **ذخیره توضیحات کاربر:** در صورت نیاز کسب‌وکار، اضافه کردن فیلد `Notes` یا `UserDescription` (nvarchar) به جدول و Entity و پر کردن آن از ViewModel.
4. **اضافه کردن RowVersion** به CashSession و پیکربندی EF برای Concurrency.
5. **Migration برای ایندکس یکتا** در صورت انتخاب راه‌حل ایندکس.

### P3 (اختیاری)

6. انتقال «حل UserId با fallback» از Controller به یک Helper یا سرویس برای تمیزتر شدن کنترلر.
7. در نظر گرفتن Idempotency یا Rate Limit برای اکشن StartSession طبق سیاست امنیتی.

---

## ۸. نتیجه‌گیری

ماژول شروع جلسه صندوق از نظر لایه‌بندی (Controller → Service → Repository) و اعتبارسنجی و امنیت پایه (CSRF, Authorization, Validation) در وضعیت قابل قبولی است، اما برای **محیط مالی پروداکشن** باید حتماً:

- از **ایجاد همزمان دو جلسه باز برای یک کاربر** (با تراکنش اتمیک و/یا ایندکس یکتا) جلوگیری شود،  
- **Audit شروع جلسه** در جدول `CashSessionAuditLog` ثبت شود،  
- در صورت نیاز، **توضیحات کاربر** و **RowVersion** برای یکپارچگی و ممیزی اضافه شوند.

پس از تأیید شما، می‌توان مرحله‌به‌مرحله پیاده‌سازی P1 و سپس P2 را انجام داد.
