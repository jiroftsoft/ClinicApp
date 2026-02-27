# گزارش ممیزی: ماژول گزارش صندوق (Cashier Report)

**مسیر:** `http://localhost:3560/Payment/CashierReport`  
**تاریخ بررسی:** ۱۴۰۴/۱۲/۰۶  
**چارچوب:** بررسی چندنقشی با رعایت SRP (Single Responsibility Principle)

---

## ۱. نقش: معمار نرم‌افزار (Software Architect)

### ۱.۱ لایه‌بندی و مسئولیت‌ها

| مؤلفه | مسئولیت فعلی | ارزیابی SRP |
|--------|----------------|-------------|
| **CashierReportController** | دریافت درخواست، پارس تاریخ، فراخوانی سرویس، بازگرداندن View/File | ✅ یک مسئولیت (هماهنگی) |
| **ICashierReportService / CashierReportService** | تهیه داده گزارش (روزانه، ماهانه، بازه، خلاصه، مقایسه) + **Export به Excel/PDF** | ⚠️ دو مسئولیت: «داده گزارش» و «قالب خروجی» در یک کلاس |
| **دسترسی به داده** | سرویس مستقیماً از `ApplicationDbContext` استفاده می‌کند؛ هیچ Repository مخصوص گزارش وجود ندارد | ⚠️ داده‌خوانی و منطق گزارش در یک لایه |

**پیشنهادات معماری (بدون شکستن ماژول):**

- **جداسازی Export:** تعریف `ICashierReportExportService` برای Excel/PDF و تزریق آن به `CashierReportService` یا کنترلر؛ سرویس گزارش فقط DTOها را برگرداند و Export سرویس دیگر خروجی باینری تولید کند.
- **Repository گزارش (اختیاری):** در صورت رشد کوئری‌ها، می‌توان `ICashierReportRepository` تعریف کرد که فقط داده خام/تجمعی را از DB برمی‌گرداند و سرویس فقط ساخت DTO و اعتبارسنجی را انجام دهد.

### ۱.۲ امنیت و دسترسی

- **وضعیت:** `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]` روی کنترلر **کامنت** شده است.
- **ریسک:** بدون احراز هویت، هر کاربری می‌تواند به گزارش‌های مالی دسترسی پیدا کند.
- **اقدام:** فعال‌سازی مجدد `Authorize` روی کنترلر (انجام شد).

### ۱.۳ وابستگی‌ها و DI

- کنترلر و سرویس از طریق DI تزریق می‌شوند؛ وابستگی‌ها واضح و قابل تست هستند.

---

## ۲. نقش: توسعه‌دهنده ارشد بک‌اند (Senior Back-end Developer)

### ۲.۱ گزارش‌های پوشش‌داده‌شده

| گزارش | متد سرویس | کنترلر | وضعیت صحت‌سنجی |
|--------|------------|--------|-----------------|
| روزانه | `GetDailyReportAsync` | DailyReport GET/POST | ✅ منطق و اعتبارسنجی درست |
| ماهانه | `GetMonthlyReportAsync` | MonthlyReport GET/POST | ⚠️ با حلقه روزانه فراخوانی می‌شود (N کوئری) |
| بازه زمانی | `GetRangeReportAsync` | RangeReport GET/POST | ⚠️ با حلقه روزانه (N کوئری) |
| خلاصه همه منشی‌ها | `GetAllCashiersSummaryAsync` | AllCashiersSummary GET/POST | ⚠️ N+1: هر منشی جدا + سشن‌ها بدون Include تراکنش‌ها |
| مقایسه منشی‌ها | `CompareCashiersAsync` | CompareCashiers GET/POST | وابسته به گزارش روزانه/خلاصه؛ همان نگرانی‌های کارایی |
| Export Excel/PDF | `ExportToExcelAsync` / `ExportToPdfAsync` | ExportToExcel / ExportToPdf | وابسته به همان سرویس؛ حلقه روزانه برای بازه |

### ۲.۲ اعتبارسنجی و تاریخ شمسی

- کنترلر از `ParseDateFromFilter` (بر اساس `PersianDateHelper.ParsePersianDate`) و `ParseDateFromHiddenInput` استفاده می‌کند؛ در صورت خطا با `NotificationHelper` و Redirect به Index رفتار می‌کند. ✅
- اعتبارسنجی بازه (fromDate &lt;= toDate) در سرویس و کنترلر انجام می‌شود. ✅

### ۲.۳ اصلاحات انجام‌شده

1. **امنیت:** فعال‌سازی مجدد `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]` روی `CashierReportController`.
2. **N+1 در GetAllCashiersSummaryAsync:** بارگذاری سشن‌ها با `Include(cs => cs.Transactions)` تا از Lazy Load تراکنش‌ها برای هر سشن جلوگیری شود و تعداد دورهای به DB کاهش یابد.

### ۲.۴ پیشنهادات بعدی (بدون اعمال در این مرحله)

- **GetMonthlyReportAsync / GetRangeReportAsync:** به‌جای حلقه روی روزها و فراخوانی `GetDailyReportAsync`، یک کوئری تجمیعی برای بازه (مثلاً ماه یا بازه) طراحی شود تا یک یا تعداد کم round-trip به DB انجام شود.
- **GetAllCashiersSummaryAsync:** در صورت امکان، تجمیع کامل در SQL (GroupBy بر اساس UserId) برای سشن‌ها، تراکنش‌ها و اختلاف‌ها تا به‌جای حلقه روی cashierIds فقط یک یا دو کوئری اجرا شود.

---

## ۳. نقش: توسعه‌دهنده فرانت‌اند (Front-end Developer)

### ۳.۱ ویوها و یکنواختی

- **Index:** فیلتر با نوع گزارش، منشی، تاریخ شمسی؛ استفاده از `_PersianDatePicker` و ارسال به اکشن‌های مناسب. ✅
- **DailyReport, MonthlyReport, RangeReport, AllCashiersSummary, CompareCashiers:** هر کدام ویومدل و View جدا؛ ساختار یکسان با فیلتر و نمایش جدول/کارت. ✅
- استایل: `cashier-reports.css`؛ Layout: `_ReceptionLayout`. ✅

### ۳.۲ تجربه کاربری و خروجی

- دکمه‌های Export به Excel/PDF در صفحات گزارش وجود دارد؛ خروجی با نام فایل و نوع MIME صحیح برگردانده می‌شود. ✅
- پیشنهاد: در صورت نیاز، امکان چاپ از طریق یک layout مخصوص چاپ (مشابه PatientBookedAppointmentsPrint) برای گزارش صندوق اضافه شود.

---

## ۴. نقش: مدیر پایگاه داده (DBA)

### ۴.۱ کوئری‌ها و کارایی

- **GetDailyReportAsync:** کوئری روی `CashSessions` با فیلتر تاریخ و کاربر؛ بارگذاری تراکنش‌ها و اختلاف‌ها با Include. قابل قبول برای گزارش روزانه.
- **GetAllCashiersSummaryAsync (قبل از اصلاح):** برای هر منشی جداگانه: Users, CashSessions, (Lazy) Transactions, PaymentDiscrepancies → N+1. **بعد از اصلاح:** سشن‌ها با `Include(Transactions)` بارگذاری می‌شوند؛ تعداد round-trip کمتر.
- **GetMonthlyReportAsync / GetRangeReportAsync:** حلقه روی روزها و فراخوانی GetDailyReportAsync → برای بازه طولانی تعداد کوئری‌ها زیاد است. پیشنهاد: کوئری تجمیعی برای بازه.

### ۴.۲ ایندکس‌ها

- جداول مرتبط (`CashSessions`, `PaymentTransactions`, `PaymentDiscrepancies`) طبق پیکربندی EF ایندکس روی فیلدهای کلیدی دارند. برای فیلترهای رایج (UserId, OpenedAt, CreatedAt, ReportedAt) وجود ایندکس مناسب توصیه می‌شود و در صورت کندی در محیط عملیاتی، بررسی Execution Plan و افزودن ایندکس ترکیبی پیشنهاد می‌شود.

---

## ۵. نقش: مشاور فنی و تحلیل‌گر سیستم

### ۵.۱ خلاصه یافته‌ها

| اولویت | مورد | وضعیت |
|--------|------|--------|
| P1 | امنیت: Authorize روی کنترلر غیرفعال بود | ✅ رفع شد |
| P1 | N+1 در GetAllCashiersSummaryAsync (سشن‌ها بدون Include تراکنش‌ها) | ✅ رفع شد |
| P2 | ترکیب مسئولیت «داده گزارش» و «Export» در یک سرویس (SRP) | مستند شد؛ refactor اختیاری |
| P2 | عدم استفاده از Repository مخصوص گزارش؛ استفاده مستقیم از DbContext | مستند شد؛ قابل بهبود در فاز بعد |
| P3 | کارایی گزارش ماهانه و بازه با حلقه روزانه | پیشنهاد بهینه‌سازی کوئری تجمیعی |

### ۵.۲ صحت‌سنجی گزارش‌ها

- **منطق عددی:** جمع تراکنش‌ها، مبالغ، نرخ موفقیت و اختلاف‌ها در DTOها با داده‌های منبع (سشن‌ها، تراکنش‌ها، اختلاف‌ها) همخوان است.
- **فیلتر تاریخ:** بازه شمسی در کنترلر به میلادی تبدیل و به سرویس ارسال می‌شود؛ فیلتر در کوئری‌ها اعمال می‌شود.
- **Export:** همان داده‌های گزارش با قالب Excel/PDF خروجی گرفته می‌شود؛ تناقض منطقی مشاهده نشد.

---

## ۶. جمع‌بندی و اقدامات انجام‌شده

- **انجام شده:**  
  - فعال‌سازی مجدد `Authorize` روی `CashierReportController`.  
  - رفع N+1 در `GetAllCashiersSummaryAsync` با استفاده از `Include(cs => cs.Transactions)` در بارگذاری سشن‌ها.
- **مستند شده (بدون تغییر کد):**  
  - پیشنهاد جداسازی سرویس Export و در صورت نیاز معرفی Repository گزارش برای رعایت بهتر SRP.  
  - پیشنهاد بهینه‌سازی گزارش ماهانه و بازه با کوئری تجمیعی.

با این تغییرات، ماژول گزارش صندوق از نظر امنیت و کاهش N+1 در خلاصه منشی‌ها بهبود یافته و اصول SRP در سطح معماری و کد مستند و تا حد ممکن رعایت شده است.
