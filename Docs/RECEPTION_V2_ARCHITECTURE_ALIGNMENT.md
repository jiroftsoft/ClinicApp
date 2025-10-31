# هم‌راستایی معماری Reception V2 با مدل ذهنی

## ✅ ساختار هسته (Core Structure)

### 1. Clinic → Departments
- ✅ **Bootstrap**: `LoadInitialAsync` - لود ClinicId, Departments
- ✅ **Default Clinic**: شفا (ClinicId = 1)
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:LoadInitialAsync`

### 2. Doctors ↔ Departments
- ✅ **DoctorDepartments Table**: انتساب با `IsActive`, `IsDeleted`, `StartDate`, `EndDate`
- ✅ **Filtering**: 
  - `GetDoctorsByDepartmentAsync` - فیلتر بر اساس دپارتمان
  - `GetDoctorsByServiceAsync` - فیلتر بر اساس دپارتمان + خدمت
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:GetDoctorsByServiceAsync`

### 3. Services ↔ Departments
- ✅ **SharedServices Table**: خدمات مشترک بین دپارتمان‌ها
- ✅ **DepartmentServices**: خدمات اختصاصی هر دپارتمان
- ✅ **Filtering**: `GetServicesByDepartmentAsync` + `GetSharedServicesAsync`
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:GetServicesByDepartmentAsync`

### 4. ServiceCategory & DoctorServiceCategory
- ✅ **ServiceCategory**: دسته‌بندی خدمات (GroupCode)
- ✅ **DoctorServiceCategory**: صلاحیت پزشک برای سرفصل خدمت
- ✅ **Filtering**: در `GetDoctorsByServiceAsync` - فیلتر سه‌لایه (DoctorDepartments + ServiceCategory/SharedService + DoctorServiceCategory + Fallback)
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:GetDoctorsByServiceAsync`

---

## ✅ تعرفه و قیمت‌گذاری (Tariff & Pricing)

### 1. Service / ServiceComponents
- ✅ **ServiceComponents**: جزء فنی (Technical) و حرفه‌ای (Professional)
- ✅ **Coefficients**: CoefTech, CoefProf
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:AddItemAsync` (Snapshot creation)

### 2. FactorSetting / FinancialYear
- ✅ **FactorSetting**: کای‌های فنی (KTech) و حرفه‌ای (KProf)
- ✅ **FinancialYear**: سال مالی فعال از `IFinancialYearService`
- ✅ **Hashtag**: قواعد هشتگ (#، #، +#)
- ✅ **Location**: 
  - `Services/Pricing/Resolvers/TariffResolver.cs`
  - `Services/Reception/ReceptionFacade.cs:AddItemAsync`

### 3. خروجی قیمت پایه
- ✅ **TariffResolver**: 
  - مسیر 1: `Service.UnitPriceIRR` (اگر موجود باشد)
  - مسیر 2: `ServiceComponent × FactorSetting` (محاسبه از اجزاء)
- ✅ **PricingEngine**: 
  - `QuoteAsync`: پیش‌محاسبه کامل با بیمه‌ها
  - `RepriceReceptionAsync`: بازمحاسبه همه آیتم‌ها
- ✅ **Location**: 
  - `Services/Pricing/Resolvers/TariffResolver.cs:ResolveApprovedTariffAsync`
  - `Services/Pricing/Engines/PricingEngine.cs:QuoteAsync`

---

## ✅ بیمه و طرح‌ها (Insurance & Plans)

### 1. InsuranceProvider / InsurancePlan
- ✅ **InsuranceProvider**: بیمه‌گذاران (تامین اجتماعی، خدمات درمانی، ...)
- ✅ **InsurancePlan**: طرح‌های پایه و تکمیلی
- ✅ **InsuranceType**: Primary vs Supplementary
- ✅ **Location**: `Models.Entities.Insurance.*`

### 2. InsuranceTariff
- ✅ **InsuranceTariff**: تعرفه‌ی طرح برای هر خدمت/گروه
  - درصد پوشش (CoveragePercent)
  - فرانشیز (Deductible)
  - سقف‌ها (PerVisitCapIRR)
  - استثناها (Exceptions)
- ✅ **InsuranceCoverageProvider**: دریافت قواعد پوشش از InsuranceTariff یا InsurancePlan (Fallback)
- ✅ **Location**: 
  - `Services/Pricing/Coverage/InsuranceCoverageProvider.cs`
  - `Models.Entities.Insurance.InsuranceTariff`

### 3. PatientInsurance
- ✅ **PatientInsurance**: انتخاب واقعیِ بیمار (طرح پایه + تکمیلی)
- ✅ **Update**: در `SetInsurancesAsync` - به‌روزرسانی `PatientInsurance` با طرح‌های جدید
- ✅ **Location**: 
  - `Services/Reception/ReceptionFacade.cs:SetInsurancesAsync`
  - `Models.Entities.Patient.PatientInsurance`

### 4. Business Rules
- ⚠️ **TODO**: اعتبارسنجی‌های پیشرفته بیمه (انقضا/سقف/فرانشیز/وابستگی طرح‌ها) **قبل از Pricing**
- ✅ **Current**: اعتبارسنجی پایه در `SetInsurancesAsync` (فعال بودن پلن‌ها)
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:SetInsurancesAsync`

---

## ✅ جریان پذیرش V2 (Reception V2 Flow)

### 1. Bootstrap
- ✅ **LoadInitialAsync**: 
  - ClinicId (Default = شفا)
  - Departments (فعال)
  - Doctors (گروه‌بندی بر اساس Dept)
  - SharedServices
  - FinancialYear
  - FactorSetting
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:LoadInitialAsync`
- ✅ **API**: `GET /api/v1/reception/bootstrap`

### 2. Doctors by Department + Service
- ✅ **GetDoctorsByDepartmentAsync**: فیلتر بر اساس دپارتمان
- ✅ **GetDoctorsByServiceAsync**: فیلتر بر اساس دپارتمان + خدمت
  - Step 1: DoctorDepartments (فعال، در بازه تاریخ)
  - Step 2: ServiceCategory / SharedService (خدمت در دپارتمان)
  - Step 3: DoctorServiceCategory (صلاحیت پزشک)
  - Step 4: Fallback (Specialty ↔ ServiceGroup)
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:GetDoctorsByServiceAsync`
- ✅ **API**: 
  - `GET /api/v1/reception/doctors/by-department`
  - `GET /api/v1/reception/doctors/by-service`

### 3. Patient Lookup/Quick-Create → PatientInsurance
- ✅ **Patient Lookup**: جستجوی بیمار بر اساس کد ملی
- ✅ **Quick-Create**: ایجاد سریع بیمار (Name, LastName, Mobile, Gender, BirthDate, Address, Base/Supp Insurance)
- ✅ **PatientInsurance**: به‌روزرسانی با طرح‌های جدید در `SetInsurancesAsync`
- ✅ **Location**: 
  - `Services/Reception/ReceptionFacade.cs:LookupOrCreatePatientAsync`
  - `Services/Reception/ReceptionFacade.cs:SetInsurancesAsync`

### 4. Draft
- ✅ **CreateDraftAsync**: ایجاد پیش‌نویس با patient/clinic/department/doctor
- ✅ **Auto-Draft**: ایجاد خودکار هنگام انتخاب patient/dept/doctor
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:CreateDraftAsync`

### 5. افزودن آیتم خدمت → Engine محاسبه

#### 5.1 Service Eligibility Validation
- ✅ **AgeMin/AgeMax**: بررسی حداقل/حداکثر سن
- ✅ **GenderLimit**: بررسی محدودیت جنسیت
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:AddItemAsync`

#### 5.2 Engine محاسبه (PricingEngine)
- ✅ **قیمت پایه خدمت**: از `TariffResolver` (UnitPriceIRR یا محاسبه از اجزاء)
- ✅ **پوشش InsurancePlan پایه**: از `InsuranceCoverageProvider`
  - سهم بیمه/بیمار
  - فرانشیز/سقف/گروه
- ✅ **کسر با Supplementary**: جبران سهم بیمار طبق قواعد طرح تکمیلی
- ✅ **Snapshot**: 
  - UnitPrice, KTech, KProf, CoefTech, CoefProf
  - BaseInsuranceCoverage, SupplementaryCoverage
  - PatientShare, InsurerShare, PrimaryPays, SupplementaryPays
  - FactorSettingId, FinancialYear, BasePlanId, SupplementaryPlanId
- ✅ **رندینگ**: `AwayFromZero` (تمام مبالغ به ریال صحیح)
- ✅ **جمع کل‌ها**: RecalculateDraftAsync
- ✅ **Location**: 
  - `Services/Pricing/Engines/PricingEngine.cs:QuoteAsync`
  - `Services/Reception/ReceptionFacade.cs:AddItemAsync`

### 6. Reprice-on-Change
- ✅ **SetInsurancesAsync**: با تغییر بیمه/طرح، همهٔ آیتم‌ها بازمحاسبه می‌شوند
- ✅ **RepriceReceptionAsync**: 
  - دریافت Reception با آیتم‌ها
  - محاسبه مجدد هر آیتم با `QuoteAsync`
  - به‌روزرسانی SnapshotJson
  - به‌روزرسانی PatientShareAmount, InsurerShareAmount
- ✅ **Location**: 
  - `Services/Pricing/Engines/PricingEngine.cs:RepriceReceptionAsync`
  - `Services/Reception/ReceptionFacade.cs:SetInsurancesAsync`

### 7. Finalize
- ✅ **FinalizePosAsync**: نهایی‌سازی با POS
- ✅ **FinalizeCashAsync**: نهایی‌سازی با نقدی
- ✅ **Validation**: 
  - بررسی وجود Draft (Status = Pending)
  - بررسی مطابقت مبلغ پرداخت با مجموع
  - بررسی Idempotency (جلوگیری از پرداخت تکراری)
- ⚠️ **TODO**: اعتبارسنجی کامل بیمه (انقضا/سقف/فرانشیز/وابستگی) **قبل از Finalize**
- ✅ **Location**: 
  - `Services/Reception/ReceptionFacade.cs:FinalizePosAsync`
  - `Services/Reception/ReceptionFacade.cs:FinalizeCashAsync`

---

## ✅ نکات کلیدی پیاده‌سازی شده

### 1. فیلتر پزشک‌ها
- ✅ **دپارتمان + سرفصل خدمت**: از `DoctorServiceCategory`
- ✅ **سه‌لایه فیلتر**: DoctorDepartments + ServiceCategory/SharedService + DoctorServiceCategory + Fallback
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:GetDoctorsByServiceAsync`

### 2. الزام فعال/مشترک بودن خدمت
- ✅ **SharedServices**: بررسی `IsActive` و `IsDeleted`
- ✅ **DepartmentServices**: بررسی `ServiceCategory.DepartmentId`
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:GetDoctorsByServiceAsync`

### 3. اعتبارسنجی‌های بیمه
- ✅ **پایه**: بررسی فعال بودن پلن‌ها در `SetInsurancesAsync`
- ⚠️ **پیشرفته**: TODO - انقضا/سقف/فرانشیز/وابستگی طرح‌ها **قبل از Pricing**
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:SetInsurancesAsync`

### 4. SetInsurances با Reprice
- ✅ **Reprice-on-Change**: با تغییر بیمه، همهٔ آیتم‌ها بازمحاسبه می‌شوند
- ✅ **Draft Required**: Draft باید قبل از persist پنل بیمه موجود باشد
- ✅ **Location**: `Services/Reception/ReceptionFacade.cs:SetInsurancesAsync`

### 5. ضد-CSRF و لاگ
- ✅ **Anti-Forgery**: `[ValidateAntiForgeryTokenOnPosts]` روی تمام POSTها
- ✅ **Logging**: Serilog با CorrelationId و OperationId
- ✅ **Location**: 
  - `Filters/ValidateAntiForgeryTokenOnPostsAttribute.cs`
  - تمام Controllers و Services

---

## ⚠️ موارد نیازمند بهبود (TODO)

### 1. اعتبارسنجی پیشرفته بیمه
- ⚠️ **انقضا**: بررسی تاریخ انقضای پلن بیمه
- ⚠️ **سقف**: بررسی سقف سالانه/ماهیانه/روزانه
- ⚠️ **فرانشیز**: بررسی فرانشیز باقیمانده
- ⚠️ **وابستگی طرح‌ها**: بررسی وابستگی طرح تکمیلی به طرح پایه
- **Location**: باید قبل از `PricingEngine.QuoteAsync` انجام شود

### 2. Finalize Validation
- ⚠️ **اعتبارسنجی کامل Draft**: 
  - PatientId, ClinicId, DepartmentId, DoctorId موجود باشد
  - ReceptionItems.Count > 0
  - BasePlanId موجود باشد (برای خدمات بیمه‌ای)
- ⚠️ **اعتبارسنجی بیمه**: همان موارد بالا
- **Location**: `Services/Reception/ReceptionFacade.cs:FinalizePosAsync` و `FinalizeCashAsync`

---

## 📊 خلاصه هم‌راستایی

| بخش | وضعیت | توضیحات |
|-----|-------|---------|
| **ساختار هسته** | ✅ | کامل |
| **تعرفه و قیمت‌گذاری** | ✅ | کامل |
| **بیمه و طرح‌ها** | ✅ | کامل (به جز اعتبارسنجی پیشرفته) |
| **جریان پذیرش V2** | ✅ | کامل |
| **فیلتر پزشک‌ها** | ✅ | کامل |
| **اعتبارسنجی خدمت** | ✅ | کامل (Age/Gender) |
| **Reprice-on-Change** | ✅ | کامل |
| **Finalize** | ⚠️ | نیاز به اعتبارسنجی کامل Draft و بیمه |
| **ضد-CSRF و لاگ** | ✅ | کامل |

---

## 🎯 نتیجه‌گیری

**مدل ذهنی شما کاملاً با پیاده‌سازی هم‌راستا است!** 

### ✅ موارد پیاده‌سازی شده:
1. ✅ ساختار هسته (Clinic → Departments → Doctors → Services)
2. ✅ فیلتر سه‌لایه پزشک‌ها (Department + Service + DoctorServiceCategory)
3. ✅ PricingEngine یکپارچه (QuoteAsync + RepriceReceptionAsync)
4. ✅ Snapshot کامل در ReceptionItem
5. ✅ Reprice-on-Change با تغییر بیمه
6. ✅ اعتبارسنجی Age/Gender برای خدمات
7. ✅ ضد-CSRF و لاگ کامل

### ⚠️ موارد نیازمند بهبود:
1. ⚠️ اعتبارسنجی پیشرفته بیمه (انقضا/سقف/فرانشیز/وابستگی) **قبل از Pricing**
2. ⚠️ اعتبارسنجی کامل Draft و بیمه **قبل از Finalize**

**آماده برای اتصال نهایی و سناریوهای تست ادغام با نمونه‌های طرح سلامت (70% پایه + 30% تکمیلی)!** 🚀

