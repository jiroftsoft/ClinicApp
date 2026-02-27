# چک‌لیست کامل ارزیابی داشبورد درآمد بیمه

---

## ۱. معماری و لایه‌بندی (Architecture Integrity)

### لایه Domain

| مورد | وضعیت | توضیح |
|------|--------|--------|
| موجودیت‌ها اصل Single Responsibility را رعایت می‌کنند؟ | ✅ | `InsuranceClaim` مسئول داده مطالبه، `InsuranceBatch` مسئول دسته‌صورت؛ هر کدام یک مسئولیت. |
| Statusها با Enum مدیریت می‌شوند؟ (ClaimStatus, BatchStatus) | ✅ | `ClaimStatus` (Pending, Approved, PartiallyPaid, Paid, Rejected)، `BatchStatus` (Submitted, UnderReview, Settled) در `Models/Enums`. |
| Propertyهای مالی decimal(18,2) هستند؟ | ⚠️ | **خیر.** در Fluent API مقدار `HasPrecision(18, 0)` استفاده شده (ریال بدون اعشار). برای گزارش‌های درصدی و یکنواختی با استاندارد مالی معمولاً decimal(18,2) توصیه می‌شود. |
| Navigation Propertyها Lazy/Eager بهینه هستند؟ | ✅ | در Repository فقط جاهایی که لازم است `Include` شده؛ گزارش‌ها با `AsNoTracking()`. |
| Fluent API کامل نوشته شده (Index، Required، Precision)؟ | ✅ | `InsuranceClaimConfig` / `InsuranceBatchConfig`: Precision، Required/Optional، FK، Indexهای ترکیبی (Plan+SubmissionDate، Status+IsDeleted، BatchId؛ Provider+SubmissionDate، Status+IsDeleted). |

### لایه Repository

| مورد | وضعیت | توضیح |
|------|--------|--------|
| تمام Queryها IQueryable هستند نه ToList() زودهنگام؟ | ⚠️ | خیر. متدهای عمومی مثل `GetByDateRangeAsync` با `.ToListAsync()` ختم می‌شوند و لیست برمی‌گردانند؛ برای گزارش‌های سنگین می‌توان overload با IQueryable یا projection بدون بارگذاری کل entity در نظر گرفت. |
| Queryهای Aggregation در DB اجرا می‌شوند نه in-memory؟ | ⚠️ | **GetProviderBreakdownAsync:** aggregation در DB (GroupBy + Sum در LINQ به SQL). **GetAgingReportAsync:** ابتدا `.Select(...).ToListAsync()` سپس `GroupBy` و `Sum` در حافظه — aggregation کامل در DB نیست. |
| N+1 وجود ندارد؟ | ✅ | در مسیر داشبورد، GetByDateRangeAsync یک بار صدا زده می‌شود؛ GetProviderBreakdownAsync و GetAgingReportAsync هر کدام یک/دو کوئری؛ در CreateBatch حلقه GetByIdWithDetails + Update است (یک کوئری به ازای هر claim). |
| متدهای Aging و ProviderBreakdown SQL-friendly هستند؟ | ⚠️ | **ProviderBreakdown:** بله (GroupBy در DB). **Aging:** خیر؛ ابتدا لیست بارگذاری شده، بعد در حافظه گروه‌بندی می‌شود. |
| Async واقعی است یا فقط نام متد Async دارد؟ | ✅ | همهٔ متدهای ریپو/سرویس `async Task` و `await` روی عملیات دیتابیس دارند. |
| Index روی ClaimDate، ProviderId، Status، BatchId وجود دارد؟ | ✅ | **Claim:** `IX_InsuranceClaim_Plan_Submission` (InsurancePlanId, SubmissionDate)، `IX_InsuranceClaim_Status_Deleted`، `IX_InsuranceClaim_BatchId`. Provider از طریق Plan join می‌شود؛ Index روی Plan در جای خود هست. **Batch:** `IX_InsuranceBatch_Provider_Submission`، `IX_InsuranceBatch_Status_Deleted`. |

### لایه Service

| مورد | وضعیت | توضیح |
|------|--------|--------|
| Controller فقط با Service کار می‌کند؟ | ✅ | `InsuranceRevenueController` فقط `IInsuranceRevenueService` و برای لیست بیمه‌گذاران `IInsuranceProviderRepository` (برای dropdown) استفاده می‌کند. |
| Business Logic داخل Controller نیست؟ | ✅ | پارس تاریخ و ساخت فیلتر در کنترلر است؛ محاسبه KPI، Aging، Breakdown و Chart فقط در سرویس. |
| ServiceResult&lt;T&gt; در همه متدها استفاده شده؟ | ✅ | GetDashboardDataAsync، GetKPIsAsync، GetAgingReportAsync، GetChartDataAsync، GetProviderBreakdownAsync، CreateBatchAsync، ExportToExcelAsync همگی `ServiceResult<T>` برمی‌گردانند. |
| Validation مالی داخل Service انجام می‌شود؟ | ⚠️ | اعتبارسنجی بازه تاریخ و خالی نبودن claimIds هست؛ هیچ چک صریح برای مقادیر منفی مبلغ یا جمع‌های نامعتبر (مثلاً ApprovedAmount > ClaimedAmount) در سرویس دیده نمی‌شود. |
| Transaction boundary مشخص است (مثلاً CreateBatch)؟ | ❌ | **CreateBatchAsync** چند بار `SaveChangesAsync` (Add batch، به‌روزرسانی claimها، به‌روزرسانی batch) بدون `Database.BeginTransaction` یا `TransactionScope` فراخوانی می‌کند؛ در صورت خطا بین این مراحل، داده نیمه‌کاره می‌ماند. |
| محاسبه KPI تکراری نیست (DRY رعایت شده)؟ | ⚠️ | GetKPIsAsync و ExportToExcel هر دو از `GetByDateRangeAsync` و سپس محاسبه روی لیست استفاده می‌کنند؛ منطق Sum/Filter (مثلاً TotalRealized فقط Paid) در دو جا تکرار شده. می‌توان یک متد کمکی در سرویس برای «خلاصه مالی بازه» در نظر گرفت. |

---

## ۲. صحت مالی (Financial Accuracy Audit)

| مورد | وضعیت | توضیح |
|------|--------|--------|
| TotalClaimAmount دقیق محاسبه می‌شود؟ | ✅ | در GetKPIsAsync: `totalClaims = nonRejected.Sum(c => c.ClaimedAmount)`؛ Rejected حذف شده. |
| PaidAmount فقط Claimهای Paid را شامل می‌شود؟ | ✅ | `totalRealized = claims.Where(c => c.Status == ClaimStatus.Paid).Sum(c => c.FinalSettlement)`. |
| Outstanding = Total - Paid درست است؟ | ✅ | `outstanding = claims.Where(c => c.Status != Paid && c.Status != Rejected).Sum(c => c.ApprovedAmount - c.FinalSettlement)` با چک `if (outstanding < 0) outstanding = 0`. |
| DeductionRate تقسیم بر صفر ندارد؟ | ✅ | `DeductionRatePercent = totalClaims > 0 ? Math.Round(...) : 0` و در ریپو `TotalClaimed > 0 ? ... : 0`. |
| AverageCollectionDays درست محاسبه شده؟ | ✅ | فقط claimهای `Status == Paid` و `PaymentDate.HasValue`؛ میانگین `(PaymentDate - SubmissionDate).TotalDays`. |
| Accrual vs Cash Flow تفکیک شده؟ | ⚠️ | تفکیک صریح بین «مبلغ تعهدی (Accrual)» و «وجوه نقد (Cash Flow)» در KPI یا گزارش جدا وجود ندارد؛ TotalRealized نقد است، Outstanding تعهدی. |
| Claimهای Rejected در KPI درست لحاظ شده‌اند؟ | ✅ | در TotalClaims از `nonRejected` استفاده شده؛ در TotalRealized و Outstanding فقط Paid و غیر Rejected؛ Rejected در مبلغ مطالبه و واریز وارد نمی‌شود. |
| Rounding مالی کنترل شده (Math.Round)؟ | ✅ | DeductionRatePercent با `Math.Round(..., 1)`؛ AverageSettlementDays با `Math.Round(avgDays, 1)`. |

---

## ۳. شاخص‌های مدیریتی (Executive Readiness)

| مورد | وضعیت | توضیح |
|------|--------|--------|
| درصد وصول هر بیمه وجود دارد؟ | ⚠️ | در ViewModel/جدول تفکیک بیمه‌گذار، **درصد وصول** (مثلاً TotalPaid/TotalClaimed*100) به‌صورت فیلد جدا محاسبه و نمایش داده نشده؛ با داشتن TotalPaid و TotalClaimed می‌توان در View یا سرویس اضافه کرد. |
| درصد کسری هر بیمه وجود دارد؟ | ✅ | `DeductionRatePercent` در ProviderBreakdown و KPI وجود دارد. |
| میانگین روز وصول هر بیمه وجود دارد؟ | ✅ | `AverageSettlementDays` در ProviderBreakdown و KPI. |
| Aging 0-30 / 31-60 / 61-90 / 90+ صحیح است؟ | ✅ | در GetAgingReportAsync با تابع Bucket و GetDays دقیقاً همین چهار بازه با برچسب «0-30 روز»، «31-60 روز»، «61-90 روز»، «بیش از 90 روز» ساخته می‌شود. |
| Top Risk Insurance شناسایی می‌شود؟ | ⚠️ | خیر. هیچ شاخص یا فلگ «پرخطر» (مثلاً بر اساس DeductionRate بالا یا Aging بالا) محاسبه یا در UI نشان داده نمی‌شود. |
| امکان فیلتر بر اساس بازه شمسی دقیق است؟ | ✅ | فیلتر با `StartDatePersian` و `EndDatePersian` و `PersianDateHelper.ParsePersianDate` به بازه میلادی تبدیل و در همهٔ کوئری‌ها استفاده می‌شود. |
| روند ماهانه واقعی ساخته شده (نه fake grouping)؟ | ✅ | در GetChartDataAsync گروه‌بندی با `GroupBy(c => new { Year, Month })` روی همان claimهای بازه فیلتر انجام می‌شود؛ برچسب‌ها و مقادیر از داده واقعی هستند. |

---

## ۴. Performance & Scalability

| مورد | وضعیت | توضیح |
|------|--------|--------|
| AsNoTracking برای گزارش‌ها استفاده شده؟ | ✅ | در `GetByIdAsync`، `GetByDateRangeAsync`، `GetByPlanIdAsync`، `GetByBatchIdAsync` و در مسیر ProviderBreakdown از `AsNoTracking()` استفاده شده. |
| Queryها Projection-based هستند؟ | ⚠️ | GetByDateRangeAsync کل entity را برمی‌گرداند؛ GetAgingReportAsync فقط `.Select(c => new { SubmissionDate, ClaimedAmount, ApprovedAmount })` دارد؛ GetProviderBreakdownAsync projection در DB دارد. برای بازه‌های بزرگ، GetByDateRangeAsync می‌تواند با projection سبک‌تر شود. |
| Index مناسب تعریف شده؟ | ✅ | طبق بخش ۱؛ ترکیب Plan+SubmissionDate، Status+IsDeleted، BatchId و معادل برای Batch. |
| ExportToExcel کل دیتا را یک‌باره لود نمی‌کند؟ | ❌ | ExportToExcel هر سه مجموعه را یک‌باره می‌گیرد: `GetByDateRangeAsync` (همه claimهای بازه)، `GetProviderBreakdownAsync`، `GetAgingReportAsync`. در بازهٔ بزرگ و حجم بالا، همه در حافظه لود می‌شوند. |
| Cache برای KPI وجود دارد؟ | ❌ | هیچ لایه کش (مثلاً در-memory یا distributed) برای KPI یا breakdown استفاده نشده. |
| در دیتای ۱ میلیون رکورد تست شده؟ | ❌ | در کد اثری از تست با حجم بالا یا محدودیت صفحه‌بندی برای Export دیده نمی‌شود. |

---

## ۵. امنیت و انطباق

| مورد | وضعیت | توضیح |
|------|--------|--------|
| [Authorize(Roles="Admin,Finance")] کامل است؟ | ✅ | کنترلر با `[Authorize(Roles = AppRoles.Admin + ",Finance")]` محافظت شده. |
| همه POSTها [ValidateAntiForgeryToken] دارند؟ | ✅ | GetKPIs، GetAgingData، GetChartData، ExportToExcel همگی `[HttpPost]` و `[ValidateAntiForgeryToken]`. |
| JSON endpointها اطلاعات حساس اضافه نمی‌دهند؟ | ✅ | خروجی JSON فقط KPI، Aging، Chart و breakdown است؛ شناسه کاربر یا توکن در پاسخ نیست. |
| Excel Injection کنترل شده؟ (سلول‌هایی که با = شروع می‌شوند) | ❌ | مقادیر (از جمله `ProviderName`، `AgeGroup`) مستقیم در سلول نوشته می‌شوند؛ اگر مقدار با `=` شروع شود، Excel آن را به‌عنوان فرمول تفسیر می‌کند. پیشنهاد: پیشوند `'` یا اسکیپ کاراکترهای خطرناک برای سلول متنی. |
| لاگ‌های مالی بدون اطلاعات حساس هستند؟ | ✅ | لاگ‌ها شامل User، ClaimId، BatchNumber و پیام‌های کلی هستند؛ مبلغ یا شماره حساب در لاگ دیده نشد. |
| Audit trail برای تغییر Status Claim وجود دارد؟ | ⚠️ | فیلدهای ITrackable (CreatedAt/By، UpdatedAt/By) و ISoftDelete در entity هست و در UpdateAsync ست می‌شوند؛ هیچ جدول/لاگ جداگانه‌ای برای «تاریخچه تغییر وضعیت» (مثلاً از Pending به Paid) وجود ندارد. |

---

## ۶. کیفیت کد (Code Quality)

| مورد | وضعیت | توضیح |
|------|--------|--------|
| Magic number نداریم؟ | ⚠️ | در GetAgingReportAsync اعداد 30، 60، 90 برای باکت‌ها به‌صورت ثابت در متد هستند؛ بهتر است به ثابت‌های با نام (مثلاً AgingBucketDays) منتقل شوند. |
| متدهای بزرگ بالای ۵۰ خط نداریم؟ | ✅ | طولانی‌ترین متدها (GetDashboardDataAsync، GetKPIsAsync، CreateBatchAsync، ExportToExcelAsync) زیر ۵۰ خط هستند. |
| Naming استاندارد است؟ | ✅ | نام متدها و فیلدها انگلیسی و قابل فهم؛ اصطلاحات دامنه (Claim، Batch، Aging، Breakdown) یکدست استفاده شده‌اند. |
| Dependency Injection تمیز است؟ | ✅ | کنترلر و سرویس فقط از طریق constructor از اینترفیس‌ها استفاده می‌کنند؛ ثبت در UnityConfig. |
| هیچ منطق مالی داخل View نیست؟ | ✅ | View فقط نمایش از Model و شکل‌دهی برای Chart (labels/values از ViewModel)؛ هیچ Sum یا شرط مالی در Razor نیست. |
| ViewModelها فقط DataCarrier هستند؟ | ✅ | ViewModelهای InsuranceRevenue فقط پراپرتی برای داده و بدون منطق کسب‌وکار هستند. |

---

## جمع‌بندی اقدامات پیشنهادی (اولویت‌دار)

1. **CreateBatchAsync:** استفاده از `context.Database.BeginTransaction()` و commit تنها پس از به‌روزرسانی موفق batch و claimها.
2. **Export Excel:** جلوگیری از Excel Injection با پیشوند `'` یا اسکیپ برای مقادیر متنی که ممکن است با `=` شروع شوند.
3. **GetAgingReportAsync (Repository):** انتقال منطق گروه‌بندی و Sum به داخل یک کوئری (پروژه‌شن یا raw SQL) تا aggregation در DB انجام شود و از بارگذاری کل لیست در حافظه جلوگیری شود.
4. **ExportToExcel:** برای بازه‌های بزرگ، محدود کردن تعداد claimها (صفحه‌بندی یا cap) یا استفاده از streaming تا کل دیتا یک‌باره در حافظه نباشد.
5. **دقت اعشار مالی:** در صورت نیاز استاندارد یکسان decimal(18,2) برای مبالغ در entity/مایگریشن و هماهنگی با KPI.
6. **درصد وصول در تفکیک بیمه‌گذار:** اضافه کردن فیلد (مثلاً CollectionRatePercent) در DTO و ViewModel و نمایش در جدول.
7. **اعتبارسنجی مالی در سرویس:** چک کردن مقادیر منفی و رابطه‌های منطقی (مثلاً ApprovedAmount ≤ ClaimedAmount) در Create/Update claim در صورت وجود endpoint مستقیم برای آن.

با انجام موارد بالا، داشبورد درآمد بیمه از نظر معماری، صحت مالی، عملکرد و امنیت به سطح production-ready نزدیک‌تر می‌شود.
