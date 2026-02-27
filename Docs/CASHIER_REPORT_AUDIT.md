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

- **وضعیت:** `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]` روی کنترلر **فعال** است.
- فقط نقش‌های Admin و Receptionist به گزارش صندوق دسترسی دارند.

### ۱.۳ وابستگی‌ها و DI

- کنترلر و سرویس از طریق DI تزریق می‌شوند؛ وابستگی‌ها واضح و قابل تست هستند.

---

## ۲. نقش: توسعه‌دهنده ارشد بک‌اند (Senior Back-end Developer)

### ۲.۱ گزارش‌های پوشش‌داده‌شده

| گزارش | متد سرویس | کنترلر | وضعیت صحت‌سنجی |
|--------|------------|--------|-----------------|
| روزانه | `GetDailyReportAsync` | DailyReport GET/POST | ✅ منطق و اعتبارسنجی درست |
| ماهانه | `GetMonthlyReportAsync` | MonthlyReport GET/POST | ✅ بهینه: یک بار بارگذاری بازه ماه با `GetDailyReportsForRangeInternalAsync` |
| بازه زمانی | `GetRangeReportAsync` | RangeReport GET/POST | ✅ بهینه: یک بار بارگذاری بازه با همان متد داخلی |
| خلاصه همه منشی‌ها | `GetAllCashiersSummaryAsync` | AllCashiersSummary GET/POST | ✅ بهینه: سشن‌ها با `Include(Transactions)` |
| مقایسه منشی‌ها | `CompareCashiersAsync` | CompareCashiers GET/POST | ✅ وابسته به خلاصه (یک کوئری) |
| Export Excel/PDF | `ExportToExcelAsync` / `ExportToPdfAsync` | ExportToExcel / ExportToPdf | ✅ بهینه: استفاده از `GetDailyReportsForRangeInternalAsync` به‌جای حلقه روزانه |

### ۲.۲ اعتبارسنجی و تاریخ شمسی

- کنترلر از `ParseDateFromFilter` (بر اساس `PersianDateHelper.ParsePersianDate`) و `ParseDateFromHiddenInput` استفاده می‌کند؛ در صورت خطا با `NotificationHelper` و Redirect به Index رفتار می‌کند. ✅
- اعتبارسنجی بازه (fromDate &lt;= toDate) در سرویس و کنترلر انجام می‌شود. ✅

### ۲.۳ اصلاحات انجام‌شده (بهینه‌سازی پروداکشن)

1. **امنیت:** فعال‌سازی مجدد `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]` روی `CashierReportController`.
2. **N+1 در GetAllCashiersSummaryAsync:** بارگذاری سشن‌ها با `Include(cs => cs.Transactions)`.
3. **گزارش ماهانه / بازه / Export:** متد داخلی `GetDailyReportsForRangeInternalAsync` اضافه شد که با **۳ round-trip** (User، CashSessions+Transactions، PaymentDiscrepancies) کل بازه را بارگذاری می‌کند و گزارش‌های روزانه را در حافظه می‌سازد. `GetMonthlyReportAsync`، `GetRangeReportAsync` و Export Excel/PDF از این متد استفاده می‌کنند و دیگر حلقه روزانه ندارند.

### ۲.۴ پیشنهادات بعدی (اختیاری)

- **GetAllCashiersSummaryAsync:** در صورت نیاز به مقیاس بالاتر، می‌توان تجمیع را در SQL (GroupBy بر اساس UserId) انجام داد تا به‌جای حلقه روی cashierIds فقط یک یا دو کوئری اجرا شود.

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
- **GetMonthlyReportAsync / GetRangeReportAsync / Export:** اکنون از `GetDailyReportsForRangeInternalAsync` استفاده می‌شود (۳ کوئری ثابت برای هر بازه).

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
| P3 | کارایی گزارش ماهانه و بازه با حلقه روزانه | ✅ رفع شد با `GetDailyReportsForRangeInternalAsync` |

### ۵.۲ صحت‌سنجی گزارش‌ها (داده واقعی)

- **گزارش روزانه (`GetDailyReportAsync`):** سشن‌ها با `OpenedAt` در همان روز (startOfDay تا endOfDay)، تراکنش‌ها = همه تراکنش‌های همان سشن‌ها (بدون فیلتر تاریخ تراکنش)، اختلاف‌ها با `ReportedAt` در همان روز. داده بازگشتی با منطق کسب‌وکار همخوان است.
- **متد بازه (`GetDailyReportsForRangeInternalAsync`):** برای هر روز در بازه، همان منطق گزارش روزانه اعمال شده (سشن بر اساس روز باز شدن، تراکنش‌های همان سشن‌ها، اختلاف بر اساس روز گزارش). خروجی معادل فراخوانی `GetDailyReportAsync` برای هر روز است.
- **گزارش ماهانه / بازه / Export:** از همان متد بازه استفاده می‌کنند؛ تجمیع (Sum، Average، لیست سشن‌ها و اختلاف‌ها) روی خروجی روزانه انجام می‌شود و داده واقعی بازمی‌گردد.
- **خلاصه منشی‌ها (`GetAllCashiersSummaryAsync`):** سشن‌ها با `OpenedAt` در بازه، تراکنش‌ها با `CreatedAt` در بازه، اختلاف‌ها با `ReportedAt` در بازه. مرز «تا تاریخ» با `rangeEndExclusive = toDate.Date.AddDays(1)` یکسان است. بهینه‌سازی: **۵ کوئری ثابت** (لیست UserIdها، Users، CashSessions+Transactions، PaymentDiscrepancies، تجمیع در حافظه) به‌جای ۱ + N×۳.
- **مقایسه منشی‌ها:** وابسته به خلاصه منشی‌ها. منشی‌های انتخاب‌شده حتی در صورت نداشتن هیچ سشن/تراکنش در بازه با مقادیر صفر (SessionCount=0, TotalAmount=0, …) در جدول نمایش داده می‌شوند تا پیام «داده‌ای یافت نشد» فقط برای خطا باشد؛ نام منشی‌های بدون فعالیت از جدول Users بارگذاری می‌شود.
- **فیلتر تاریخ:** بازه شمسی در کنترلر به میلادی تبدیل و به سرویس ارسال می‌شود؛ فیلتر در کوئری‌ها اعمال می‌شود.
- **Export:** همان داده‌های گزارش با قالب Excel/PDF خروجی گرفته می‌شود.

---

## ۶. نقشه راه بهینه‌سازی (ماژول گزارش صندوق)

| مرحله | گزارش/متد | اقدام | وضعیت |
|--------|------------|--------|--------|
| ۱ | کنترلر | فعال‌سازی `Authorize` | ✅ |
| ۲ | GetAllCashiersSummaryAsync | `Include(cs => cs.Transactions)` | ✅ |
| ۳ | گزارش ماهانه / بازه / Export | متد داخلی بازه + حذف حلقه روزانه | ✅ |
| ۴ | خلاصه همه منشی‌ها | ۵ کوئری ثابت، فرم فیلتر روی صفحه، نرمال تاریخ | ✅ |
| ۵ | گزارش روزانه (DailyReport) | کوئری بدون Include(User) اضافی؛ اعتبارسنجی cashierId/date؛ Audit Log؛ ViewModel مالی؛ Export امن؛ چاپ | ✅ |

**قلب سیستم گزارش‌گیری کلینیک:** این ماژول بدون تغییر API و رفتار ظاهری برای پروداکشن بهینه شده است. گزارش روزانه تک‌روز بدون تغییر باقی مانده است.

---

## ۷. جمع‌بندی و اقدامات انجام‌شده

- **انجام شده:**  
  - فعال‌سازی مجدد `Authorize` روی `CashierReportController`.  
  - رفع N+1 در `GetAllCashiersSummaryAsync` با `Include(cs => cs.Transactions)`.  
  - افزودن `GetDailyReportsForRangeInternalAsync` و استفاده در گزارش ماهانه، بازه‌زمانی و Export Excel/PDF؛ حذف حلقه روزانه و کاهش round-trip به DB به حد ثابت (۳ کوئری به‌ازای هر بازه).
- **مستند شده (بدون تغییر کد):**  
  - جداسازی سرویس Export و Repository گزارش در صورت نیاز برای SRP بهتر.

ماژول گزارش صندوق برای محیط پروداکشن بهینه شده و رفتار و خروجی گزارش‌ها بدون شکست حفظ شده است.
