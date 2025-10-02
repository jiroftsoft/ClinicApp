# 🔧 **راهنمای جامع Services - سیستم کلینیک شفا**

> **تاریخ ایجاد**: 1404/07/11  
> **نسخه**: 1.0  
> **وضعیت**: نهایی شده  
> **تعداد کل**: 74 فایل Service

---

## 📑 **فهرست**

1. [مقدمه](#مقدمه)
2. [ساختار کلی](#ساختار-کلی)
3. [Core Services](#core-services)
4. [Insurance Services](#insurance-services)
5. [ClinicAdmin Services](#clinicadmin-services)
6. [Payment Services](#payment-services)
7. [Utility Services](#utility-services)
8. [الگوهای طراحی](#الگوهای-طراحی)
9. [ارتباطات بین سرویس‌ها](#ارتباطات-بین-سرویس‌ها)
10. [نکات بهینه‌سازی](#نکات-بهینه‌سازی)

---

## 🎯 **مقدمه**

این پروژه شامل **74 فایل Service** است که قلب **منطق کسب‌وکار** سیستم را تشکیل می‌دهند. تمام Service ها بر اساس اصول:

- ✅ **Service Layer Pattern** - جداسازی منطق کسب‌وکار
- ✅ **Dependency Injection** - وابستگی به Interface ها
- ✅ **ServiceResult Pattern** - مدیریت یکپارچه نتایج
- ✅ **Logging & Error Handling** - لاگ‌گیری و مدیریت خطا
- ✅ **Medical Environment Standards** - استانداردهای پزشکی

طراحی شده‌اند.

---

## 📊 **ساختار کلی**

```
Services/ (74 فایل)
│
├── 📂 Core Services (Root) - 20 فایل
│   ├── Authentication & User Management
│   ├── Patient Management
│   ├── Reception Management (قلب سیستم)
│   ├── Service Management
│   └── Clinic & Department Management
│
├── 📂 Insurance/ - 23 فایل
│   ├── Calculation Services (محاسبات بیمه)
│   ├── Tariff Services (تعرفه‌ها)
│   ├── Patient Insurance (بیمه بیماران)
│   ├── Supplementary Insurance (بیمه تکمیلی)
│   └── Combined Insurance (محاسبات ترکیبی)
│
├── 📂 ClinicAdmin/ - 12 فایل
│   ├── Doctor Management (مدیریت پزشکان)
│   ├── Schedule & Appointment (برنامه‌ریزی و نوبت‌دهی)
│   ├── Assignment & Dashboard (تخصیص و داشبورد)
│   └── Emergency Booking (رزرو اورژانس)
│
├── 📂 Payment/ - 6 فایل
│   ├── Gateway Services (درگاه‌های پرداخت)
│   ├── POS Management (مدیریت کارتخوان)
│   ├── Validation (اعتبارسنجی پرداخت)
│   └── Reporting (گزارشات)
│
├── 📂 DataSeeding/ - 4 فایل
│   ├── FactorSettingSeedService
│   ├── ServiceSeedService
│   ├── ServiceTemplateSeedService
│   └── SystemSeedService
│
├── 📂 Notification/ - 1 فایل
│   └── NotificationModule
│
├── 📂 UserContext/ - 2 فایل
│   ├── IUserContextService
│   └── UserContextService
│
├── 📂 SystemSettings/ - 2 فایل
│   ├── ISystemSettingService
│   └── SystemSettingService
│
├── 📂 Idempotency/ - 2 فایل
│   ├── IIdempotencyService
│   └── InMemoryIdempotencyService
│
├── 📂 Financial/ - 1 فایل
│   └── InsuranceTariffCalculationService
│
└── 📂 Calculation/ - 1 فایل
    └── TariffCalculator
```

---

## 1️⃣ **Core Services (Root Level - 20 فایل)**

### 📌 **دسته‌بندی:**

| **دسته** | **Service ها** | **مسئولیت** |
|---------|----------------|-------------|
| **Authentication** | `AuthService`, `CurrentUserService`, `BackgroundCurrentUserService` | احراز هویت OTP، مدیریت کاربر جاری |
| **Patient** | `PatientService` | CRUD بیماران، مدیریت بیمه، تاریخچه |
| **Reception** | `ReceptionService` | پذیرش بیماران، محاسبات، استعلام خارجی |
| **Service** | `ServiceService`, `ServiceCategoryService`, `ServiceCalculationService`, `ServiceTemplateService`, `ServiceManagementService` | مدیریت خدمات، محاسبات قیمت |
| **Shared Service** | `SharedServiceManagementService` | مدیریت خدمات مشترک بین دپارتمان‌ها |
| **Factor** | `FactorSettingService` | مدیریت کای‌های فنی/حرفه‌ای |
| **Clinic** | `ClinicManagementService`, `DepartmentManagementService` | مدیریت کلینیک و دپارتمان‌ها |
| **External** | `ExternalInquiryService`, `SecurityTokenService` | استعلام خارجی، توکن امنیتی |
| **Notification** | `MessageNotificationService`, `AsanakSmsService` | ارسال پیامک و اطلاع‌رسانی |

---

### 🔷 **Service های کلیدی:**

#### **1. PatientService** - مدیریت بیماران

**📍 مکان:** `Services/PatientService.cs`

**🎯 مسئولیت:**
- CRUD کامل بیماران
- مدیریت بیمه‌های بیمار
- تاریخچه پذیرش‌ها و نوبت‌ها
- Soft Delete برای حفظ اطلاعات پزشکی

**🔗 وابستگی‌ها:**
```csharp
private readonly ApplicationDbContext _context;
private readonly ApplicationUserManager _userManager;
private readonly ILogger _log;
private readonly ICurrentUserService _currentUserService;
private readonly IAppSettings _appSettings;
```

**🔄 ارتباط با سایر ماژول‌ها:**
```
PatientService
    ├─→ ApplicationDbContext (Database)
    ├─→ ICurrentUserService (کاربر جاری)
    └─→ ApplicationUserManager (Identity)
    
استفاده می‌شود در:
    ├─→ ReceptionService (پذیرش)
    ├─→ InsuranceCalculationService (محاسبات بیمه)
    └─→ PatientInsuranceService (بیمه بیماران)
```

**💡 منطق کلیدی:**

1. **ثبت‌نام بیمار:**
```csharp
public async Task<ServiceResult> RegisterPatientAsync(RegisterPatientViewModel model, string userIp)
{
    // 1. Normalize شماره موبایل
    // 2. Rate Limiting
    // 3. بررسی وجود کاربر
    // 4. Transaction امن
    // 5. ایجاد در AspNetUsers و Patients
    // 6. تخصیص بیمه آزاد پیش‌فرض
}
```

2. **جستجوی پیشرفته:**
```csharp
public async Task<ServiceResult<PagedResult<PatientIndexViewModel>>> SearchPatientsAsync(
    string searchTerm, int pageNumber, int pageSize)
{
    // جستجو در: نام، نام خانوادگی، کد ملی، شماره تلفن
    // صفحه‌بندی بهینه
    // تبدیل به ViewModel با Factory Method
}
```

3. **حذف نرم با بررسی وابستگی‌ها:**
```csharp
public async Task<ServiceResult> DeletePatientAsync(int patientId)
{
    // بررسی پذیرش‌های فعال
    // بررسی نوبت‌های آینده
    // Soft Delete (IsDeleted = true)
    // ثبت DeletedByUserId و DeletedAt
}
```

---

#### **2. ReceptionService** - قلب تپنده سیستم

**📍 مکان:** `Services/ReceptionService.cs`

**🎯 مسئولیت:**
- ایجاد و مدیریت پذیرش
- محاسبات بیمه در حین پذیرش
- استعلام خارجی (شبکه شمس)
- Lookup Lists برای UI
- مدیریت پرداخت‌های پذیرش

**🔗 وابستگی‌ها:**
```csharp
private readonly IReceptionRepository _receptionRepository;
private readonly IPatientService _patientService;
private readonly IExternalInquiryService _externalInquiryService;
private readonly IInsuranceCalculationService _insuranceCalculationService;
private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
private readonly IPatientInsuranceService _patientInsuranceService;
private readonly IServiceCategoryService _serviceCategoryService;
private readonly IServiceService _serviceService;
private readonly IDoctorCrudService _doctorCrudService;
private readonly IServiceCalculationService _serviceCalculationService;
private readonly ICurrentUserService _currentUserService;
private readonly ILogger _logger;
```

**🔄 ارتباط با سایر ماژول‌ها:**
```
ReceptionService (قلب سیستم)
    ├─→ IPatientService (بیماران)
    ├─→ IDoctorCrudService (پزشکان)
    ├─→ IServiceService (خدمات)
    ├─→ IServiceCalculationService (محاسبات قیمت)
    ├─→ IInsuranceCalculationService (محاسبات بیمه)
    ├─→ ICombinedInsuranceCalculationService (بیمه ترکیبی)
    ├─→ IPatientInsuranceService (بیمه بیماران)
    ├─→ IExternalInquiryService (استعلام خارجی)
    ├─→ IPaymentTransactionRepository (پرداخت‌ها)
    └─→ IReceptionRepository (دسترسی به داده)
```

**💡 منطق کلیدی:**

1. **ایجاد پذیرش:**
```csharp
public async Task<ServiceResult<ReceptionDetailsViewModel>> CreateReceptionAsync(
    ReceptionCreateViewModel model)
{
    // 1. اعتبارسنجی بیمار و پزشک
    // 2. محاسبه قیمت خدمات
    // 3. محاسبات بیمه (اصلی + تکمیلی)
    // 4. ایجاد Reception
    // 5. ایجاد ReceptionItems
    // 6. ایجاد InsuranceCalculations
    // 7. Transaction امن
}
```

2. **استعلام کمکی بیمار:**
```csharp
public async Task<ServiceResult<PatientInquiryViewModel>> InquiryPatientInfoAsync(
    string nationalCode, DateTime birthDate)
{
    // 1. بررسی توکن امنیتی
    // 2. استعلام هویت (شبکه شمس)
    // 3. استعلام بیمه
    // 4. ترکیب اطلاعات
}
```

3. **محاسبات هزینه پذیرش:**
```csharp
public async Task<ServiceResult<ReceptionCostCalculationViewModel>> CalculateReceptionCostsAsync(
    int patientId, List<int> serviceIds, int? insuranceId, DateTime receptionDate)
{
    // 1. محاسبه قیمت هر خدمت
    // 2. محاسبه بیمه برای هر خدمت
    // 3. جمع کل هزینه‌ها
    // 4. محاسبه سهم بیمار و بیمه
}
```

---

#### **3. ServiceService** - مدیریت خدمات پزشکی

**📍 مکان:** `Services/ServiceService.cs`

**🎯 مسئولیت:**
- CRUD خدمات پزشکی
- مدیریت ServiceComponents (فنی/حرفه‌ای)
- به‌روزرسانی خودکار قیمت
- اعتبارسنجی کد خدمات

**🔗 وابستگی‌ها:**
```csharp
private readonly ApplicationDbContext _context;
private readonly ILogger _log;
private readonly ICurrentUserService _currentUserService;
private readonly IServiceCalculationService _serviceCalculationService;
```

**💡 منطق کلیدی:**

1. **ایجاد خدمت:**
```csharp
public async Task<ServiceResult<int>> CreateServiceAsync(ServiceCreateEditViewModel model)
{
    // 1. اعتبارسنجی ورودی‌ها
    // 2. بررسی کد تکراری
    // 3. ایجاد Service
    // 4. ایجاد ServiceComponents (فنی/حرفه‌ای)
    // 5. محاسبه و ذخیره قیمت
}
```

2. **به‌روزرسانی قیمت خودکار:**
```csharp
public async Task<ServiceResult<decimal>> UpdateServicePriceAsync(int serviceId)
{
    // 1. دریافت اجزای خدمت
    // 2. محاسبه قیمت با ServiceCalculationService
    // 3. به‌روزرسانی فیلد Price
}
```

---

#### **4. ServiceCalculationService** - محاسبات قیمت

**📍 مکان:** `Services/ServiceCalculationService.cs`

**🎯 مسئولیت:**
- محاسبه دقیق قیمت خدمات
- پشتیبانی از FactorSettings
- محاسبات خدمات مشترک با Override
- مدیریت هشتگ‌دار/غیر هشتگ‌دار

**💡 فرمول محاسبه:**

```
قیمت نهایی = (ضریب فنی × کای فنی) + (ضریب حرفه‌ای × کای حرفه‌ای)

جایی که:
- ضریب فنی/حرفه‌ای: از ServiceComponents
- کای فنی/حرفه‌ای: از FactorSettings بر اساس سال مالی

اگر هشتگ‌دار:
    کای فنی = FactorSetting (IsHashtagged=true, Technical, FinancialYear)
    کای حرفه‌ای = FactorSetting (IsHashtagged=true, Professional, FinancialYear)

اگر غیر هشتگ‌دار:
    کای فنی = FactorSetting (IsHashtagged=false, Technical, FinancialYear)
    کای حرفه‌ای = FactorSetting (IsHashtagged=false, Professional, FinancialYear)
```

**💡 منطق کلیدی:**

1. **محاسبه با FactorSettings:**
```csharp
public decimal CalculateServicePriceWithFactorSettings(
    Service service, 
    ApplicationDbContext context,
    DateTime? date = null, 
    int? departmentId = null, 
    int? financialYear = null)
{
    // 1. تعیین سال مالی
    // 2. دریافت کای‌های فعال
    // 3. بررسی Freeze شدن
    // 4. محاسبه با فرمول
    // 5. بررسی Override دپارتمان
}
```

2. **محاسبات خدمات مشترک:**
```csharp
public async Task<ServiceCalculationResult> CalculateSharedServicePriceAsync(
    int serviceId, 
    int departmentId,
    decimal? overrideTechnicalFactor = null, 
    decimal? overrideProfessionalFactor = null)
{
    // 1. دریافت SharedService
    // 2. استفاده از Override اگر موجود باشد
    // 3. محاسبه با کای‌های Override شده
}
```

---

#### **5. FactorSettingService** - مدیریت کای‌ها

**📍 مکان:** `Services/FactorSettingService.cs`

**🎯 مسئولیت:**
- CRUD کای‌های فنی/حرفه‌ای
- مدیریت سال مالی
- Freeze کردن کای‌ها
- دریافت کای فعال

**💡 منطق کلیدی:**

1. **دریافت کای فعال:**
```csharp
public async Task<FactorSetting> GetActiveFactorByTypeAsync(
    ServiceComponentType factorType, 
    int financialYear, 
    bool isHashtagged)
{
    // بازگشت کای فعال برای:
    // - نوع (Technical/Professional)
    // - سال مالی
    // - وضعیت هشتگ
}
```

2. **Freeze کردن کای‌های سال مالی:**
```csharp
public async Task<bool> FreezeFinancialYearFactorsAsync(int financialYear, string userId)
{
    // 1. پیدا کردن تمام کای‌های سال مالی
    // 2. تنظیم IsFrozen = true
    // 3. ثبت FrozenByUserId و FrozenAt
    // نتیجه: محاسبات با این کای‌ها قفل می‌شود
}
```

---

#### **6. AuthService** - احراز هویت با OTP

**📍 مکان:** `Services/AuthService.cs`

**🎯 مسئولیت:**
- ارسال OTP برای ورود/ثبت‌نام
- تأیید OTP
- ورود بدون پسورد
- مدیریت Session

**💡 منطق کلیدی:**

1. **ارسال OTP:**
```csharp
public async Task<ServiceResult> SendLoginOtpAsync(string nationalCode)
{
    // 1. Normalize کد ملی
    // 2. بررسی وجود کاربر
    // 3. تولید کد OTP
    // 4. ارسال پیامک
    // 5. ذخیره در Cache با TTL
}
```

2. **تأیید و ورود:**
```csharp
public async Task<ServiceResult> VerifyLoginOtpAndSignInAsync(
    string nationalCode, 
    string otpCode)
{
    // 1. بررسی صحت OTP
    // 2. SignIn با Identity
    // 3. ثبت LastLoginDate
}
```

---

#### **7. ExternalInquiryService** - استعلام خارجی

**📍 مکان:** `Services/ExternalInquiryService.cs`

**🎯 مسئولیت:**
- استعلام هویت از ثبت احوال
- استعلام بیمه از سرویس‌های بیمه
- مدیریت توکن امنیتی
- Cache کردن نتایج

**💡 منطق کلیدی:**

1. **استعلام کامل:**
```csharp
public async Task<ServiceResult<PatientInquiryViewModel>> InquiryCompleteAsync(
    string nationalCode, 
    DateTime birthDate, 
    InquiryType inquiryType, 
    string tokenId)
{
    // 1. بررسی Token
    // 2. استعلام هویت (شبکه شمس)
    // 3. استعلام بیمه
    // 4. ترکیب اطلاعات
    // 5. Cache کردن
}
```

---

## 2️⃣ **Insurance Services (23 فایل)**

### 📌 **دسته‌بندی:**

| **دسته** | **Service ها** | **مسئولیت** |
|---------|----------------|-------------|
| **Calculation** | `InsuranceCalculationService`, `AdvancedInsuranceCalculationService`, `InsuranceTariffCalculationService` | محاسبات بیمه اصلی |
| **Supplementary** | `SupplementaryInsuranceService`, `CorrectSupplementaryInsuranceCalculationService`, `SupplementaryInsuranceCacheService`, `SupplementaryInsuranceMonitoringService`, `SupplementaryInsuranceOptimizationService` | بیمه تکمیلی |
| **Combined** | `CombinedInsuranceCalculationService`, `CombinedInsuranceCalculationTestService`, `SupplementaryCombinationService` | محاسبات ترکیبی |
| **Tariff** | `InsuranceTariffService`, `BulkInsuranceTariffService`, `BulkSupplementaryTariffService` | مدیریت تعرفه‌ها |
| **Patient** | `PatientInsuranceService` | مدیریت بیمه‌های بیماران |
| **Plan & Provider** | `InsurancePlanService`, `InsuranceProviderService`, `InsurancePlanDependencyService` | طرح‌ها و ارائه‌دهندگان |
| **Validation** | `InsuranceValidationService`, `TariffDomainValidationService` | اعتبارسنجی |
| **Business Rules** | `BusinessRuleEngine` | موتور قوانین کسب‌وکار |
| **Data Services** | `SupplementaryInsuranceDataFixService`, `SupplementaryTariffSeederService` | اصلاح داده، Seeding |

---

### 🔷 **Service های کلیدی:**

#### **1. InsuranceCalculationService** - محاسبات بیمه اصلی

**📍 مکان:** `Services/Insurance/InsuranceCalculationService.cs`

**🎯 مسئولیت:**
- محاسبه سهم بیمار
- محاسبه سهم بیمه
- محاسبه Franchise و Copay
- ذخیره InsuranceCalculation

**🔗 وابستگی‌ها:**
```csharp
private readonly IPatientInsuranceRepository _patientInsuranceRepository;
private readonly IPlanServiceRepository _planServiceRepository;
private readonly IInsuranceCalculationRepository _insuranceCalculationRepository;
private readonly IInsuranceTariffRepository _insuranceTariffRepository;
private readonly IBusinessRuleEngine _businessRuleEngine;
private readonly ICurrentUserService _currentUserService;
```

**💡 فرمول محاسبه:**

```
Step 1: دریافت بیمه فعال بیمار
Step 2: بررسی اعتبار بیمه در تاریخ محاسبه
Step 3: دریافت تعرفه بیمه برای خدمت

محاسبه:
    سهم بیمه = قیمت خدمت × درصد پوشش
    Franchise = حداقل پرداخت بیمار
    Copay = مبلغ ثابت پرداخت بیمار
    سهم بیمار = (قیمت - سهم بیمه) + Franchise + Copay

نتیجه نهایی:
    TotalAmount = قیمت خدمت
    InsuranceCoverage = سهم بیمه
    PatientPayment = سهم بیمار
```

**💡 منطق کلیدی:**

```csharp
public async Task<ServiceResult<InsuranceCalculationResultViewModel>> 
    CalculatePatientShareAsync(int patientId, int serviceId, DateTime calculationDate)
{
    // 1. دریافت بیمه فعال بیمار
    // 2. بررسی اعتبار
    // 3. دریافت تعرفه
    // 4. محاسبه سهم‌ها
    // 5. ذخیره محاسبه
}
```

---

#### **2. CombinedInsuranceCalculationService** - محاسبات ترکیبی

**📍 مکان:** `Services/Insurance/CombinedInsuranceCalculationService.cs`

**🎯 مسئولیت:**
- محاسبه ترکیبی بیمه اصلی + تکمیلی
- رعایت سقف‌های پرداخت
- محاسبه دقیق سهم نهایی بیمار

**💡 فرمول محاسبه ترکیبی:**

```
Step 1: محاسبه بیمه اصلی
    سهم بیمه اصلی = قیمت × درصد پوشش اصلی
    سهم بیمار پس از بیمه اصلی = قیمت - سهم بیمه اصلی

Step 2: محاسبه بیمه تکمیلی
    سهم بیمه تکمیلی = سهم بیمار × درصد پوشش تکمیلی
    سهم نهایی بیمار = سهم بیمار - سهم بیمه تکمیلی

مثال:
    خدمت: 1,000,000 تومان
    بیمه اصلی: 70% = 700,000
    باقی‌مانده: 300,000
    بیمه تکمیلی: 60% از باقی‌مانده = 180,000
    سهم نهایی بیمار: 120,000 (12%)
```

**💡 منطق کلیدی:**

```csharp
public async Task<ServiceResult<CombinedInsuranceCalculationResult>> 
    CalculateCombinedInsuranceAsync(
        int patientId, 
        int serviceId, 
        decimal serviceAmount, 
        DateTime calculationDate)
{
    // 1. اعتبارسنجی
    // 2. دریافت بیمه اصلی
    // 3. محاسبه سهم بیمه اصلی
    // 4. دریافت بیمه‌های تکمیلی
    // 5. محاسبه سهم تکمیلی
    // 6. محاسبه سهم نهایی بیمار
}
```

---

#### **3. PatientInsuranceService** - مدیریت بیمه‌های بیماران

**📍 مکان:** `Services/Insurance/PatientInsuranceService.cs`

**🎯 مسئولیت:**
- CRUD بیمه‌های بیماران
- تنظیم بیمه اصلی
- اعتبارسنجی تداخل تاریخ‌ها
- محاسبات ترکیبی

**💡 منطق کلیدی:**

1. **ایجاد بیمه بیمار:**
```csharp
public async Task<ServiceResult<int>> CreatePatientInsuranceAsync(
    PatientInsuranceCreateEditViewModel model)
{
    // 1. اعتبارسنجی شماره بیمه (تکراری نباشد)
    // 2. بررسی تداخل تاریخ‌ها
    // 3. اگر اصلی است، غیرفعال کردن بیمه‌های اصلی قبلی
    // 4. ایجاد PatientInsurance
}
```

2. **اعتبارسنجی تداخل:**
```csharp
public async Task<ServiceResult<bool>> DoesDateOverlapExistAsync(
    int patientId, 
    DateTime startDate, 
    DateTime endDate, 
    int? excludeId = null)
{
    // بررسی تداخل تاریخ با بیمه‌های موجود
    // جلوگیری از تعریف بیمه‌های همزمان
}
```

---

#### **4. InsuranceTariffService** - مدیریت تعرفه‌ها

**📍 مکان:** `Services/Insurance/InsuranceTariffService.cs`

**🎯 مسئولیت:**
- CRUD تعرفه‌های بیمه
- Bulk Operations
- محاسبه تعرفه تکمیلی
- مدیریت تنظیمات بیمه تکمیلی

**💡 منطق کلیدی:**

1. **ایجاد تعرفه:**
```csharp
public async Task<ServiceResult<int>> CreateTariffAsync(
    InsuranceTariffCreateEditViewModel model)
{
    // 1. اعتبارسنجی تکراری نبودن (PlanId + ServiceId)
    // 2. ایجاد InsuranceTariff
    // 3. تنظیم درصد پوشش، فرانشیز، Copay
}
```

2. **Bulk Operations:**
```csharp
public async Task<ServiceResult<int>> CreateBulkTariffForAllServicesAsync(
    InsuranceTariffCreateEditViewModel model)
{
    // 1. دریافت تمام خدمات فعال
    // 2. ایجاد تعرفه برای همه
    // 3. Transaction امن
}
```

---

## 3️⃣ **ClinicAdmin Services (12 فایل)**

### 📌 **دسته‌بندی:**

| **دسته** | **Service ها** | **مسئولیت** |
|---------|----------------|-------------|
| **Doctor CRUD** | `DoctorCrudService` | CRUD پزشکان |
| **Schedule** | `DoctorScheduleService`, `ScheduleOptimizationService` | برنامه‌ریزی، بهینه‌سازی |
| **Appointment** | `AppointmentAvailabilityService`, `EmergencyBookingService` | نوبت‌دهی، اورژانس |
| **Assignment** | `DoctorAssignmentService`, `DoctorAssignmentHistoryService` | تخصیص به دپارتمان |
| **Relations** | `DoctorDepartmentService`, `DoctorServiceCategoryService` | روابط با دپارتمان و سرفصل |
| **Dashboard** | `DoctorDashboardService`, `DoctorReportingService` | داشبورد و گزارشات |
| **Other** | `SpecializationService` | تخصص‌های پزشکی |

---

### 🔷 **Service های کلیدی:**

#### **1. DoctorCrudService** - مدیریت پزشکان

**📍 مکان:** `Services/ClinicAdmin/DoctorCrudService.cs`

**🎯 مسئولیت:**
- CRUD کامل پزشکان
- مدیریت تخصص‌های پزشکان
- Soft Delete و Restore
- Activate/Deactivate
- جستجو و فیلتر پیشرفته

**🔗 وابستگی‌ها:**
```csharp
private readonly IDoctorCrudRepository _doctorRepository;
private readonly ISpecializationService _specializationService;
private readonly IDoctorReportingRepository _doctorReportingRepository;
private readonly ICurrentUserService _currentUserService;
private readonly IValidator<DoctorCreateEditViewModel> _validator;
private readonly IClinicRepository _clinicRepository;
```

**💡 منطق کلیدی:**

1. **ایجاد پزشک:**
```csharp
public async Task<ServiceResult<Doctor>> CreateDoctorAsync(DoctorCreateEditViewModel model)
{
    // 1. اعتبارسنجی با FluentValidation
    // 2. بررسی کد ملی تکراری
    // 3. بررسی کد نظام پزشکی
    // 4. ایجاد Doctor
    // 5. تخصیص تخصص‌ها
    // 6. Audit Trail
}
```

2. **جستجوی پزشکان:**
```csharp
public async Task<ServiceResult<PagedResult<DoctorIndexViewModel>>> GetDoctorsAsync(DoctorSearchViewModel filter)
{
    // جستجو بر اساس:
    // - نام/نام خانوادگی
    // - کد ملی
    // - تخصص
    // - کلینیک
    // - دپارتمان
    // - وضعیت فعال/غیرفعال
}
```

---

#### **2. DoctorScheduleService** - برنامه کاری

**📍 مکان:** `Services/ClinicAdmin/DoctorScheduleService.cs`

**🎯 مسئولیت:**
- تنظیم برنامه کاری هفتگی
- محاسبه زمان‌های در دسترس
- مسدود کردن بازه‌های زمانی
- تولید اسلات‌های زمانی

**💡 منطق کلیدی:**

1. **تنظیم برنامه:**
```csharp
public async Task<ServiceResult> SetDoctorScheduleAsync(int doctorId, DoctorScheduleViewModel schedule)
{
    // 1. بررسی وجود پزشک
    // 2. اعتبارسنجی برنامه
    // 3. بررسی تداخل
    // 4. ذخیره برنامه
}
```

2. **محاسبه اسلات‌ها:**
```csharp
public async Task<ServiceResult<List<TimeSlotViewModel>>> GetAvailableAppointmentSlotsAsync(int doctorId, DateTime date)
{
    // 1. دریافت برنامه کاری روز
    // 2. دریافت نوبت‌های رزرو شده
    // 3. دریافت مسدودی‌ها
    // 4. محاسبه اسلات‌های خالی
}
```

---

## 4️⃣ **Payment Services (6 فایل)**

### 📌 **دسته‌بندی:**

| **دسته** | **Service ها** | **مسئولیت** |
|---------|----------------|-------------|
| **Core** | `PaymentService` | مدیریت پرداخت‌ها |
| **Gateway** | `PaymentGatewayService` | درگاه‌های پرداخت آنلاین |
| **POS** | `PosManagementService` | مدیریت کارتخوان |
| **Validation** | `PaymentValidationService` | اعتبارسنجی |
| **Reporting** | `PaymentReportingService` | گزارشات |
| **Web** | `WebPaymentService` | پرداخت وب |

---

### 🔷 **Service های کلیدی:**

#### **1. PaymentService** - مدیریت پرداخت‌ها

**📍 مکان:** `Services/Payment/PaymentService.cs`

**🎯 مسئولیت:**
- مدیریت انواع پرداخت (نقدی، POS، آنلاین)
- محاسبه خودکار کارمزدها
- مدیریت وضعیت‌های پرداخت
- برگشت و لغو پرداخت
- گزارش‌گیری مالی

**🔗 وابستگی‌ها:**
```csharp
private readonly IPaymentTransactionRepository _paymentTransactionRepository;
private readonly IPaymentGatewayRepository _paymentGatewayRepository;
private readonly IOnlinePaymentRepository _onlinePaymentRepository;
private readonly IPosTerminalRepository _posTerminalRepository;
private readonly ICashSessionRepository _cashSessionRepository;
```

**💡 منطق کلیدی:**

1. **پرداخت نقدی:**
```csharp
public async Task<ServiceResult<PaymentTransaction>> ProcessCashPaymentAsync(CashPaymentRequest request)
{
    // 1. اعتبارسنجی درخواست
    // 2. بررسی مبلغ
    // 3. ایجاد PaymentTransaction
    // 4. ثبت در جلسه نقدی
    // 5. صدور رسید
}
```

2. **پرداخت POS:**
```csharp
public async Task<ServiceResult<PaymentTransaction>> ProcessPosPaymentAsync(PosPaymentRequest request)
{
    // 1. بررسی ترمینال POS
    // 2. اتصال به دستگاه
    // 3. پردازش تراکنش
    // 4. ثبت در سیستم
    // 5. محاسبه کارمزد
}
```

3. **پرداخت آنلاین:**
```csharp
public async Task<ServiceResult<string>> InitiateOnlinePaymentAsync(OnlinePaymentRequest request)
{
    // 1. انتخاب درگاه پرداخت
    // 2. محاسبه کارمزد
    // 3. ایجاد درخواست پرداخت
    // 4. دریافت URL پرداخت
    // 5. Redirect به درگاه
}
```

---

#### **2. PaymentGatewayService** - مدیریت درگاه‌ها

**📍 مکان:** `Services/Payment/Gateway/PaymentGatewayService.cs`

**🎯 مسئولیت:**
- یکپارچه‌سازی با درگاه‌های مختلف (ZarinPal, Saman, Mellat, PayPing)
- مدیریت تنظیمات درگاه‌ها
- تست اتصال و سلامت
- مدیریت Callback و Webhook
- محاسبه کارمزد درگاه

**💡 منطق کلیدی:**

1. **ایجاد درگاه:**
```csharp
public async Task<ServiceResult<PaymentGateway>> CreatePaymentGatewayAsync(CreatePaymentGatewayRequest request)
{
    // 1. اعتبارسنجی
    // 2. بررسی MerchantId تکراری نباشد
    // 3. تنظیم درگاه پیش‌فرض
    // 4. ایجاد و ذخیره
}
```

2. **تست اتصال:**
```csharp
public async Task<ServiceResult<bool>> TestGatewayConnectionAsync(int gatewayId)
{
    // 1. دریافت تنظیمات
    // 2. ارسال درخواست تست
    // 3. بررسی پاسخ
    // 4. ثبت نتیجه
}
```

---

#### **3. PosManagementService** - مدیریت POS

**📍 مکان:** `Services/Payment/POS/PosManagementService.cs`

**🎯 مسئولیت:**
- مدیریت ترمینال‌های POS
- مدیریت جلسات نقدی (Cash Sessions)
- محاسبه موجودی و تراز
- گزارش‌گیری از تراکنش‌های POS

**💡 منطق کلیدی:**

1. **شروع جلسه نقدی:**
```csharp
public async Task<ServiceResult<CashSession>> StartCashSessionAsync(StartCashSessionRequest request)
{
    // 1. بررسی جلسه باز نباشد
    // 2. ثبت موجودی اولیه
    // 3. ایجاد CashSession
    // 4. فعال‌سازی ترمینال
}
```

2. **بستن جلسه نقدی:**
```csharp
public async Task<ServiceResult<CashSessionSummary>> CloseCashSessionAsync(CloseCashSessionRequest request)
{
    // 1. محاسبه موجودی نهایی
    // 2. محاسبه مغایرت
    // 3. ثبت گزارش نهایی
    // 4. بستن جلسه
}
```

---

## 5️⃣ **Utility Services**

### 📌 **دسته‌بندی:**

| **دسته** | **Service ها** | **مسئولیت** |
|---------|----------------|-------------|
| **User Context** | `CurrentUserService`, `BackgroundCurrentUserService`, `UserContextService` | مدیریت کاربر جاری |
| **System Settings** | `SystemSettingService` | تنظیمات سیستم |
| **Idempotency** | `InMemoryIdempotencyService` | جلوگیری از تکرار عملیات |
| **Data Seeding** | `FactorSettingSeedService`, `ServiceSeedService`, `ServiceTemplateSeedService`, `SystemSeedService` | Seed کردن داده‌های اولیه |
| **Notification** | `MessageNotificationService`, `AsanakSmsService` | پیامک و اطلاع‌رسانی |
| **Financial** | `InsuranceTariffCalculationService` | محاسبات مالی |
| **Calculation** | `TariffCalculator` | ماشین‌حساب تعرفه |

---

## 🎨 **الگوهای طراحی**

### 1️⃣ **Service Layer Pattern**

```
Controller → Service → Repository → Database

ویژگی‌ها:
✅ جداسازی منطق کسب‌وکار
✅ Testability بالا
✅ Reusability
✅ Maintainability
```

### 2️⃣ **Dependency Injection Pattern**

```csharp
public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _log;
    
    public PatientService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _log = logger.ForContext<PatientService>();
    }
}
```

**مزایا:**
- ✅ Loose Coupling
- ✅ قابلیت Mock کردن در تست‌ها
- ✅ قابلیت جایگزینی Implementation

### 3️⃣ **ServiceResult Pattern**

```csharp
// موفقیت
return ServiceResult<int>.Successful(patientId, "بیمار با موفقیت ایجاد شد");

// خطا
return ServiceResult<int>.Failed(
    "کد ملی تکراری است", 
    "DUPLICATE_NATIONAL_CODE",
    ErrorCategory.Validation,
    SecurityLevel.Low);

// خطای اعتبارسنجی
return ServiceResult<int>.FailedWithValidationErrors(
    "خطاهای اعتبارسنجی", 
    validationErrors);
```

### 4️⃣ **Repository Pattern**

```csharp
// Service استفاده می‌کند از Repository
public class PatientService
{
    private readonly IPatientRepository _patientRepository;
    
    public async Task<Patient> GetPatientAsync(int id)
    {
        return await _patientRepository.GetByIdAsync(id);
    }
}

// Repository مسئول دسترسی به داده است
public class PatientRepository : IPatientRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Patient> GetByIdAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.Insurances)
            .FirstOrDefaultAsync(p => p.PatientId == id);
    }
}
```

### 5️⃣ **Transaction Pattern**

```csharp
public async Task<ServiceResult> CreatePatientWithInsuranceAsync(...)
{
    using (var transaction = _context.Database.BeginTransaction())
    {
        try
        {
            // ایجاد بیمار
            var patient = new Patient { ... };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            
            // ایجاد بیمه
            var insurance = new PatientInsurance { PatientId = patient.PatientId };
            _context.PatientInsurances.Add(insurance);
            await _context.SaveChangesAsync();
            
            transaction.Commit();
            return ServiceResult.Successful();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

---

## 🔗 **ارتباطات بین سرویس‌ها**

### **نمودار ارتباطات کلی:**

```
┌─────────────────────────────────────────────────────────────┐
│                        Controllers                           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Core Services Layer                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────┐         ┌──────────────────┐         │
│  │ ReceptionService │◄────────┤ PatientService   │         │
│  │  (قلب سیستم)    │         └──────────────────┘         │
│  └────────┬─────────┘                                       │
│           │                                                  │
│           ├─────────► ServiceCalculationService             │
│           ├─────────► InsuranceCalculationService           │
│           ├─────────► CombinedInsuranceCalculationService   │
│           ├─────────► ExternalInquiryService                │
│           └─────────► PaymentService                        │
│                                                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Repository Layer                         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  ApplicationDbContext                        │
└─────────────────────────────────────────────────────────────┘
```

### **ارتباطات تخصصی:**

#### **ReceptionService (قلب سیستم):**

```
ReceptionService
    ├─→ IPatientService (بیماران)
    ├─→ IDoctorCrudService (پزشکان)
    ├─→ IServiceService (خدمات)
    │   └─→ IServiceCalculationService (محاسبات قیمت)
    │       └─→ IFactorSettingService (کای‌ها)
    ├─→ IInsuranceCalculationService (بیمه اصلی)
    │   ├─→ IPatientInsuranceService (بیمه بیماران)
    │   └─→ IInsuranceTariffService (تعرفه‌ها)
    ├─→ ICombinedInsuranceCalculationService (بیمه ترکیبی)
    │   ├─→ IInsuranceCalculationService
    │   └─→ ISupplementaryInsuranceService
    ├─→ IExternalInquiryService (استعلام خارجی)
    │   └─→ ISecurityTokenService
    └─→ IPaymentTransactionRepository (پرداخت‌ها)
```

---

## 💡 **نکات بهینه‌سازی**

### ✅ **DO's - همیشه انجام بده:**

1. **استفاده از ServiceResult:**
```csharp
// ✅ درست
return ServiceResult<int>.Successful(patientId);

// ❌ اشتباه
return patientId;
```

2. **Logging در تمام عملیات:**
```csharp
_log.Information("ایجاد بیمار. NationalCode: {NationalCode}. User: {UserId}", 
    model.NationalCode, _currentUserService.UserId);
```

3. **Transaction برای عملیات چندگانه:**
```csharp
using (var transaction = _context.Database.BeginTransaction())
{
    // عملیات چندگانه
    transaction.Commit();
}
```

4. **Async/Await برای I/O:**
```csharp
public async Task<ServiceResult> CreatePatientAsync(...)
{
    await _context.SaveChangesAsync();
}
```

5. **Dependency Injection:**
```csharp
// در Startup.cs یا UnityConfig
container.RegisterType<IPatientService, PatientService>();
```

---

### ❌ **DON'Ts - هرگز انجام نده:**

1. **منطق کسب‌وکار در Controller:**
```csharp
// ❌ اشتباه - در Controller
var patient = _context.Patients.Find(id);
patient.IsDeleted = true;
_context.SaveChanges();

// ✅ درست - در Service
await _patientService.DeletePatientAsync(id);
```

2. **استفاده مستقیم از DbContext در Controller:**
```csharp
// ❌ اشتباه
public class PatientController
{
    private readonly ApplicationDbContext _context;
}

// ✅ درست
public class PatientController
{
    private readonly IPatientService _patientService;
}
```

3. **عدم مدیریت خطا:**
```csharp
// ❌ اشتباه
public async Task<int> CreatePatientAsync(...)
{
    var patient = new Patient { ... };
    _context.Patients.Add(patient);
    await _context.SaveChangesAsync();
    return patient.PatientId;
}

// ✅ درست
public async Task<ServiceResult<int>> CreatePatientAsync(...)
{
    try
    {
        var patient = new Patient { ... };
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return ServiceResult<int>.Successful(patient.PatientId);
    }
    catch (Exception ex)
    {
        _log.Error(ex, "خطا در ایجاد بیمار");
        return ServiceResult<int>.Failed("خطا در ایجاد بیمار");
    }
}
```

4. **عدم Dispose کردن Context:**
```csharp
// ✅ در Service Layer با DI مدیریت می‌شود
// DbContext به صورت Per-Request ایجاد و Dispose می‌شود
```

---

## 📊 **آمار کلی Services**

| **گروه** | **تعداد** | **استفاده** |
|---------|----------|-------------|
| **Core Services** | 20 | عملیات اصلی سیستم |
| **Insurance Services** | 23 | سیستم بیمه |
| **ClinicAdmin Services** | 12 | مدیریت پزشکان |
| **Payment Services** | 6 | سیستم پرداخت |
| **Utility Services** | 13 | سرویس‌های کمکی |
| **جمع کل** | **74** | |

---

## 🚀 **نکات کلیدی برای توسعه:**

### 1️⃣ **هنگام ایجاد Service جدید:**

```csharp
public class NewService : INewService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _log;
    
    public NewService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _log = logger.ForContext<NewService>();
    }
    
    public async Task<ServiceResult<T>> OperationAsync(...)
    {
        _log.Information("عملیات شروع شد. User: {UserId}", _currentUserService.UserId);
        
        try
        {
            // منطق اصلی
            
            return ServiceResult<T>.Successful(result);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در عملیات");
            return ServiceResult<T>.Failed("خطا در عملیات");
        }
    }
}
```

### 2️⃣ **الگوی استاندارد CRUD:**

```csharp
// Create
public async Task<ServiceResult<int>> CreateAsync(CreateEditViewModel model) { }

// Read
public async Task<ServiceResult<DetailsViewModel>> GetDetailsAsync(int id) { }
public async Task<ServiceResult<CreateEditViewModel>> GetForEditAsync(int id) { }
public async Task<ServiceResult<PagedResult<IndexViewModel>>> SearchAsync(...) { }

// Update
public async Task<ServiceResult> UpdateAsync(CreateEditViewModel model) { }

// Delete
public async Task<ServiceResult> SoftDeleteAsync(int id) { }
```

### 3️⃣ **الگوی Validation:**

```csharp
private async Task<ServiceResult> ValidateAsync(Model model)
{
    if (string.IsNullOrWhiteSpace(model.RequiredField))
        return ServiceResult.Failed("فیلد الزامی است");
        
    if (await IsDuplicateAsync(model.UniqueField))
        return ServiceResult.Failed("مقدار تکراری است");
        
    return ServiceResult.Successful();
}
```

---

## 📚 **مراجع مرتبط**

- [INTERFACES_COMPREHENSIVE_GUIDE.md](./INTERFACES_COMPREHENSIVE_GUIDE.md)
- [APP_PRINCIPLES_CONTRACT.md](./APP_PRINCIPLES_CONTRACT.md)
- [ServiceResult_Enhanced_Contract.md](./ServiceResult_Enhanced_Contract.md)
- [DATABASE_COMPREHENSIVE_SCHEMA.md](./DATABASE_COMPREHENSIVE_SCHEMA.md)

---

**✨ پایان مستند جامع Services ✨**


