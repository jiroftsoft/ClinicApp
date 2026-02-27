# بررسی آمادگی تولید — ماژول بیمه‌ها (بیمه‌گذاران، بیمه‌شدگان، تعیین ست، تکمیلی، تعرفه)

**Stack:** ASP.NET MVC5 + EF6 + Unity + SQL Server | **Environment:** Financial + Medical Production

---

## PHASE 1 – Critical Risk Scan (فهرست ریسک‌های حیاتی)

### امنیت
- **`[Authorize]` غیرفعال:** در `InsuranceProviderController`، `PatientInsuranceController`، `InsurancePlanController`، `SupplementaryTariffController`، `BusinessRuleController`، `CombinedInsuranceCalculationController` دسترسی بدون احراز هویت امکان‌پذیر است.
- **InsuranceCalculationController:** بدون هیچ `[Authorize]`؛ هر درخواستی می‌تواند محاسبه بیمه انجام دهد.

### ریسک مالی / محاسباتی
- **تعرفه چندتایی:** `InsuranceTariffRepository.GetByPlanAndServiceAsync` فیلتر `StartDate`/`EndDate` ندارد؛ برای یک (PlanId, ServiceId) در صورت وجود چند تعرفه، `FirstOrDefaultAsync()` تعرفه نامشخص برمی‌گرداند → مبلغ محاسبه اشتباه.
- **گرد کردن:** در `SupplementaryInsuranceService` و محاسبات تعرفه سیاست گرد کردن (MidpointRounding) تعریف نشده؛ تفاوت سنت‌ها در مجموع‌گیری ممکن است.

### یکپارچگی داده
- **بیمه‌گذار:** `GetAllAsync()` و `SearchAsync()` و `GetActiveAsync()` فیلتر `!IsDeleted` ندارند؛ لیست‌ها می‌توانند ارائه‌دهنده حذف‌شده نشان دهند. `DoesCodeExistAsync` / `DoesNameExistAsync` هم بدون `!IsDeleted`؛ امکان تداخل با رکوردهای حذف‌شده.
- **بیمه بیمار:** `DoesDateOverlapExistAsync` فقط `IsActive` چک می‌کند، `!IsDeleted` ندارد؛ تداخل با بیمه‌های حذف‌شده نادیده گرفته می‌شود. `DoesPolicyNumberExistAsync` و `DoesPrimaryInsuranceExistAsync` بدون `!IsDeleted`؛ تکرار شماره بیمه/بیمه اصلی با رکورد حذف‌شده ممکن است.
- **تعرفه:** در Create/Edit تعرفه، بررسی تداخل بازه تاریخ (همان Plan+Service+نوع) برای تعرفه‌های فعال وجود ندارد؛ دو تعرفه با بازه‌های متداخل ممکن است ذخیره شوند.

### همزمانی
- **InsuranceProvider** و **InsuranceTariff** دارای `RowVersion` هستند؛ در کنترلر/سرویس هیچ `DbUpdateConcurrencyException` هندل نمی‌شود و RowVersion به کلاینت برگردانده/استفاده نمی‌شود؛ در ویرایش همزمان داده بازنویسی می‌شود.
- **PatientInsurance** و **InsuranceCalculation** بدون RowVersion؛ در صورت بروز تداخل همزمانی، تشخیص داده نمی‌شود.

### Null / استثنا
- در `InsuranceCalculationService.CalculatePatientShareAsync`: اگر `patientInsurance.InsurancePlan` یا `InsurancePlan.InsuranceProvider` لود نشده باشد، دسترسی به `CoveragePercent`/`Name` می‌تواند NullReferenceException بدهد (وابسته به همیشه Include شدن در repository).
- در `SupplementaryInsuranceService`: اگر `tariff.SupplementaryCoveragePercent` null باشد، فقط `supplementaryCoverage = 0` می‌ماند؛ منطقاً درست است ولی اگر کسب‌وکار انتظار مقدار پیش‌فرض دارد باید مستند شود.

### Audit
- **InsuranceProviderRepository.Update:** مقداردهی `CreatedByUserId` به‌جای `UpdatedByUserId`.
- **InsuranceProviderRepository.Delete:** مقداردهی `CreatedByUserId` به‌جای `DeletedByUserId`؛ `DeletedAt` و `DeletedByUserId` اصلاً ست نمی‌شوند (entity در صورت داشتن این فیلدها ناقص پر می‌شود).
- **PatientInsuranceRepository.ChangePatientSupplementaryInsuranceAsync:** استفاده از `_currentUserService.UserName` برای `UpdatedByUserId` در حالی که در متدهای مشابه همان ریپو از `UserId` استفاده شده؛ برای یکنواختی audit بهتر است یک منبع (مثلاً UserId به صورت string) استفاده شود.
- **PatientInsuranceRepository.Delete:** اگر مشابه InsuranceProvider فقط CreatedByUserId ست شود و DeletedAt/DeletedByUserId نباشد، باید اصلاح شود (در RemovePatientSupplementaryInsuranceAsync به‌درستی ست شده؛ متد Delete جداگانه را چک کنید).

---

## PHASE 2 – Logic Validation

### قواعد کسب‌وکار
- **یک بیمه اصلی فعال به ازای هر بیمار:** در Validator و `DoesPrimaryInsuranceExistAsync` اعمال شده؛ اما `DoesPrimaryInsuranceExistAsync` فیلتر `!IsDeleted` ندارد → بعد از soft delete یک بیمه اصلی، امکان ثبت «دومین» بیمه اصلی برای همان بیمار تا زمانی که رکورد حذف‌شده در کوئری باشد.
- **تعیین ست بیمه‌ای:** «ست» به صورت صریح یک موجودیت جدا نیست؛ از طریق PatientInsurance فعال + Reception/Appointment + InsuranceCalculation نمایش داده می‌شود. اگر در یک پذیرش چند خدمت با طرح‌های متفاوت باشد، منطق «یک ست واحد برای پذیرش» باید در سرویس/UI روشن باشد.
- **بیمه تکمیلی فقط روی مبلغ باقی‌مانده:** در `SupplementaryInsuranceService` درست اعمال شده (remainingAmount = serviceAmount - primaryCoverage). وقتی remainingAmount <= 0 خروجی صفر درست است.

### حالت‌های مرزی
- **بیمه بدون EndDate (باز):** در `DoesDateOverlapExistAsync` اگر یکی از طرفین EndDate نداشته باشد، شرط `pi.EndDate >= startDate` و مشابه آن برای null باید در SQL/LINQ درست رفتار کند (در SQL معمولاً null در مقایسه نادیده گرفته می‌شود یا باید صریح handle شود).
- **تعرفه با EndDate null:** در `GetActiveTariffsForServiceAsync` با `(t.EndDate == null || t.EndDate >= effectiveDate)` درست است. در `GetByPlanAndServiceAsync` اصلاً تاریخ در نظر گرفته نمی‌شود.
- **چند تعرفه برای یک (Plan, Service) در بازه‌های مختلف:** امروز در GetByPlanAndServiceAsync یکی به صورت نامشخص انتخاب می‌شود؛ قاعده «تعرفه مؤثر در تاریخ محاسبه» اعمال نشده.

### فرض‌های نادرست
- فرض «همیشه فقط یک تعرفه فعال به ازای (PlanId, ServiceId)» در repository نقض می‌شود اگر کسب‌وکار تعرفه‌های زمانی (مثلاً سال مالی) داشته باشد.
- فرض «PatientInsurance از طریق GetActiveByPatientAsync همیشه با Plan و Provider لود می‌شود» در InsuranceCalculationService؛ اگر جایی بدون Include صدا زده شود NRE ممکن است.

### انتقال وضعیت
- تغییر وضعیت تعرفه (فعال/غیرفعال) یا حذف نرم بدون چک کردن وابستگی‌ها (مثلاً InsuranceCalculationهای وابسته به این تعرفه) می‌تواند به داده تاریخی ناسازگار منجر شود؛ نیاز به قاعده روشن برای «تعرفه تاریخی» vs «تعرفه جاری».

---

## PHASE 3 – UX برای پرسنل واقعی (منشی / صندوق)

### اصطکاک و خطا
- **مسیر طولانی برای تعیین بیمه در پذیرش:** اگر انتخاب بیمه بیمار و تعیین ست از چند صفحه/فرم جدا انجام شود، احتمال اشتباه و صرف زمان زیاد است؛ یک نقطه ورود واحد (مثلاً در همان فرم پذیرش) با انتخاب بیمه اصلی/تکمیلی و نمایش خلاصه سهم بیمار پیشنهاد می‌شود.
- **عدم نمایش واضح «بیمه مؤثر امروز»:** در لیست/کارت بیمار یا پذیرش، نمایش صریح «بیمه اصلی فعلی + تکمیلی (در صورت وجود) + اعتبار تا تاریخ X» از سردرگمی جلوگیری می‌کند.
- **پیام خطای نامشخص در محاسبه:** وقتی تعرفه یا PlanService یافت نمی‌شود، پیام از نوع «پیکربندی بیمه برای این خدمت یافت نشد» است؛ بهتر است یک راهنمای عملی اضافه شود (مثلاً «برای این طرح و خدمت تعرفه تعریف کنید» یا لینک به صفحه تعرفه).

### تصمیمات خطرناک در UI
- **حذف بدون تأیید دومرحله‌ای:** در صفحات حذف بیمه‌گذار، طرح، تعرفه یا بیمه بیمار در صورت نبود تأیید صریح (مثلاً مودال با تکرار نام/شناسه)، احتمال حذف اشتباهی بالاست؛ حتماً تأیید دومرحله‌ای و در صورت امکان «غیرفعال کردن» به‌جای حذف نرم پیشنهاد شود.
- **ویرایش همزمان بدون هشدار:** با توجه به نبود هندل Concurrency در بک‌اند، در UI حداقل یک هشدار (مثلاً «اگر شخص دیگری این رکورد را تغییر داده، ذخیره ممکن است تغییرات او را بازنویسی کند») یا نمایش «آخرین بروزرسانی» پیشنهاد می‌شود.

### کاهش سردرگمی
- **تفکیک واضح بیمه پایه / تکمیلی در فرم‌ها:** در فرم ثبت/ویرایش بیمه بیمار، برچسب‌های واضح «بیمه پایه» و «بیمه تکمیلی» و در صورت امکان توضیح کوتاه (مثلاً «فقط یک بیمه پایه فعال») از اشتباه در انتخاب جلوگیری می‌کند.
- **فیلتر/جستجوی بیمه‌گذار و طرح:** در دراپ‌داون‌های طولانی، جستجو یا فیلتر تایپ‌ای (مثلاً Select2) برای یافتن سریع بیمه‌گذار/طرح ضروری است تا منشی با حداقل کلیک به گزینه درست برسد.
- **نمایش تعرفه مؤثر در تاریخ:** در صفحه تعرفه‌ها، نمایش «تعرفه مؤثر در تاریخ امروز» برای هر (طرح، خدمت) و هشدار در صورت وجود چند تعرفه با بازه متداخل، از اعمال ناخواسته تعرفه اشتباه جلوگیری می‌کند.

---

## PHASE 4 – Production Hardening

### لاگینگ
- **عملیات حساس مالی:** برای هر محاسبه نهایی سهم بیمار/بیمه (خروجی InsuranceCalculationService / CombinedInsuranceCalculationService) یک لاگ با سطح Information شامل: PatientId, ServiceId, PlanId, مبلغ خدمت، پوشش اصلی، تکمیلی، سهم بیمار، تاریخ محاسبه و UserId تا در صورت اختلاف بعداً قابل ردیابی باشد.
- **تغییرات تعرفه و طرح:** در Create/Update/Delete تعرفه و طرح بیمه، علاوه بر شناسه، مقادیر قبل و بعد (مثلاً TariffPrice, PatientShare, InsurerShare) در لاگ قرار گیرد.
- **خطاهای اعتبارسنجی:** خطاهای Validator (بیمه بیمار، تعرفه، تداخل تاریخ) با سطح Warning و متن خطا و ورودی‌های کلیدی (PatientId, PolicyNumber, بازه تاریخ) لاگ شوند تا الگوی خطاهای کاربر قابل تحلیل باشد.

### عملکرد و کوئری
- **GetByPlanAndServiceAsync:** برای استفاده در مسیر محاسبه، یک overload با `calculationDate` اضافه شود و فقط تعرفه‌ای که `StartDate <= calculationDate` و `(EndDate == null || EndDate >= calculationDate)` دارد برگردانده شود؛ ترجیح با جدیدترین StartDate یا با فیلد Priority در صورت وجود.
- **لیست بیمه‌گذاران/طرح‌ها برای دراپ‌داون:** با `AsNoTracking()` و فقط ستون‌های لازم (Id, Name, Code) به صورت projection انجام شود؛ از بارگذاری کل entity و رابطه‌های اضافی برای لیست‌های بزرگ خودداری شود.
- **GetActiveByPatientAsync:** اطمینان از یک بار Include کردن Plan و Provider تا از N+1 در حلقه‌های بعدی جلوگیری شود (در کد فعلی بسته به فراخوانی ریپو چک شود).

### ایندکس
- **InsuranceTariff:** ایندکس ترکیبی برای کوئری «تعرفه مؤثر در تاریخ» پیشنهاد می‌شود، مثلاً: `(InsurancePlanId, ServiceId, IsDeleted, IsActive)` و در صورت پرس‌وجو بر اساس تاریخ: `(InsurancePlanId, ServiceId, StartDate, EndDate)`.
- **PatientInsurance:** برای `DoesDateOverlapExistAsync` و `DoesPrimaryInsuranceExistAsync` ایندکس ترکیبی `(PatientId, IsDeleted, IsActive, StartDate, EndDate)` و `(PatientId, IsPrimary, IsDeleted)` سرعت را بهبود می‌دهد.
- **InsuranceProvider:** ایندکس `(IsDeleted, IsActive)` برای لیست‌های فیلترشده (در صورت اضافه کردن فیلتر IsDeleted به GetAll/GetActive) مفید است.

### کش
- **لیست بیمه‌گذاران و طرح‌های فعال:** برای دراپ‌داون‌های پرتکرار (مثلاً در فرم پذیرش)، کش کوتاه‌مدت (مثلاً ۱–۵ دقیقه) با کلید نوع `InsuranceProviders_Active` و `InsurancePlans_ByProvider_{id}` باعث کاهش بار دیتابیس می‌شود؛ در صورت تغییر (Create/Update/Delete) باید cache invalidate شود.
- **تعرفه به ازای (Plan, Service, Date):** در صورت استفاده بسیار زیاد در محاسبه، کش با کلید `Tariff_{planId}_{serviceId}_{date}` و TTL کوتاه (مثلاً ۵ دقیقه) قابل بررسی است؛ با توجه به حساسیت مالی، TTL کوتاه و invalidation در تغییر تعرفه ضروری است.

### کارهای فنی قابل انجام (اقدام‌های اتمیک)
1. در تمام کنترلرهای بیمه `[Authorize]` با نقش مناسب (حداقل Admin برای تعریف بیمه‌گذار/طرح/تعرفه، Reception/Doctor برای بیمه بیمار و محاسبه) فعال شود.
2. در `InsuranceProviderRepository`: در `GetAllAsync`, `GetActiveAsync`, `SearchAsync`, `SearchActiveAsync` شرط `!ip.IsDeleted` اضافه شود؛ در `DoesCodeExistAsync` و `DoesNameExistAsync` شرط `!IsDeleted` اضافه شود (مگر کسب‌وکار صریحاً بخواهد کد/نام حذف‌شده مجدد استفاده شود).
3. در `InsuranceProviderRepository.Update`: به‌جای `CreatedByUserId` فقط `UpdatedByUserId` ست شود. در `Delete`: `DeletedAt`, `DeletedByUserId` ست شوند و به‌جای `CreatedByUserId` از `DeletedByUserId` استفاده شود (با توجه به نوع فیلد در entity).
4. در `PatientInsuranceRepository`: در `DoesDateOverlapExistAsync`, `DoesPolicyNumberExistAsync`, `DoesPrimaryInsuranceExistAsync` شرط `!pi.IsDeleted` اضافه شود.
5. در `PatientInsuranceRepository.ChangePatientSupplementaryInsuranceAsync`: برای یکنواختی audit با بقیه متدها از همان منبعی که برای UpdatedByUserId در UpdatePatientPrimaryInsuranceAsync استفاده می‌شود (مثلاً UserId به صورت string) استفاده شود.
6. در `InsuranceTariffRepository.GetByPlanAndServiceAsync`: یک overload با پارامتر `DateTime? calculationDate` اضافه شود و فیلتر `StartDate`/`EndDate` اعمال شود؛ در صورت چند رکورد، یک قاعده (مثلاً OrderBy StartDate descending و FirstOrDefault) مستند و پیاده شود.
7. در سرویس/کنترلر تعرفه: در Create/Update قبل از ذخیره، با استفاده از متد جدید ریپو یا سرویس، وجود تعرفه دیگر با همان Plan+Service+نوع و بازه متداخل چک شود و در صورت تداخل خطای validation برگردانده شود.
8. در کنترلرهایی که entity با RowVersion ویرایش می‌کنند (مثلاً InsuranceProvider، InsuranceTariff): در POST Edit، RowVersion از مدل به entity کپی شود؛ در catch، در صورت `DbUpdateConcurrencyException` یک پیام مناسب و بازگرداندن به فرم با داده به‌روز شده انجام شود.
9. در `InsuranceCalculationService.CalculatePatientShareAsync`: اطمینان از اینکه متد repository که PatientInsurance را برمی‌گرداند همیشه Plan و InsuranceProvider را Include می‌کند؛ در غیر این صورت در سرویس قبل از استفاده null-check اضافه شود.

---

*پایان سند بررسی — فقط اقدام‌های مهندسی قابل انجام ذکر شده، بدون بازنویسی کامل کد مگر موارد حیاتی.*
