# 📋 **فهرست‌برداری جامع پروژه ClinicApp - Reception V2 Focus**

**تاریخ ایجاد:** 2025-01-27  
**هدف:** فهرست‌برداری جامع از ساختار پروژه با تمرکز بر ماژول Reception V2  
**نسخه:** 2.0.0

---

## 📁 **ساختار پروژه - Reception V2 Focus**

### **1️⃣ Route/Filters/DI Configuration**

#### **Route Configuration**
- ✅ `App_Start/RouteConfig.cs` - Attribute Routing فعال (`routes.MapMvcAttributeRoutes()`)
  - Route Prefix: `api/v1/reception` برای `ReceptionApiV1Controller`
  - Legacy Fallback: `/Api/ReceptionApi/*` برای سازگاری با کد قدیمی
  - Reception V2 Routes: `/reception/v2`, `/ReceptionV2/ReceptionList/*`

#### **Dependency Injection**
- ✅ `App_Start/UnityConfig.cs` - Unity Container Configuration
  - ثبت `IReceptionFacade` → `ReceptionFacade`
  - ثبت `IReceptionPricingService` → `ReceptionPricingService`
  - ثبت `ILogger` → `Serilog.ILogger`
  - ثبت `IPosTerminalRepository` → `PosTerminalRepository`
  - ثبت `IPaymentTransactionRepository` → `PaymentTransactionRepository`
  - ثبت `IPosManagementService` → `PosManagementService`

#### **Filters**
- ✅ `Filters/ValidateAntiForgeryTokenOnPostsAttribute.cs` - CSRF Validation برای POST/PUT/DELETE
  - پشتیبانی از Header Token: `RequestVerificationToken` یا `X-RequestVerificationToken`
  - پاسخ JSON 400 با کد `ANTIFORGERY_MISSING` در Dev
- ✅ `Filters/NoCacheFilter.cs` - Zero Cache برای محیط درمانی
- ✅ `Filters/GlobalExceptionFilter.cs` - مدیریت خطاهای سراسری
- ✅ `Filters/CultureFilter.cs` - مدیریت Culture فارسی

---

### **2️⃣ Reception V2 - Controllers**

#### **API Controllers**
- ✅ `Controllers/Api/ReceptionApiV1Controller.cs` - API V1 پذیرش
  - Route Prefix: `[RoutePrefix("api/v1/reception")]`
  - Endpoints:
    - `GET /api/v1/reception/health`
    - `GET /api/v1/reception/bootstrap?clinicId=&deptId=`
    - `POST /api/v1/reception/draft/create`
    - `POST /api/v1/reception/patient/lookup-or-create`
    - `POST /api/v1/reception/item/add`
    - `POST /api/v1/reception/item/update`
    - `POST /api/v1/reception/item/remove`
    - `POST /api/v1/reception/insurances/set`
    - `GET /api/v1/reception/totals`
    - `POST /api/v1/reception/finalize/pos`
    - `POST /api/v1/reception/finalize/cash`
    - `GET /api/v1/reception/doctors/by-department?deptId=`
    - `GET /api/v1/reception/doctors/by-service?deptId=&serviceId=`
- ✅ `Controllers/Api/ReceptionApiController.cs` - Legacy API (Fallback)
- ✅ `Controllers/Api/ReceptionApiDtos.cs` - DTOs برای API

#### **MVC Controllers**
- ✅ `Controllers/ReceptionV2/ReceptionControllerV2.cs` - Controller اصلی V2
  - Route: `/reception/v2` یا `/ReceptionV2`
  - Actions: `Index()`, `Edit(int id)`, `Print(int id)`
- ✅ `Controllers/ReceptionV2/ReceptionListV2Controller.cs` - لیست پذیرش‌ها
  - Route: `/ReceptionV2/ReceptionList/*`

---

### **3️⃣ Reception V2 - Services**

#### **Core Services**
- ✅ `Services/Reception/ReceptionFacade.cs` - Orchestrator اصلی
  - Interface: `Interfaces/Reception/IReceptionFacade.cs`
  - وابستگی‌ها:
    - `IServiceCalculationService`
    - `ServiceCalculationEngine`
    - `ICombinedInsuranceCalculationService`
    - `IReceptionWorkflowService`
    - `IDepartmentManagementService`
    - `IPatientService`
    - `IPatientInsuranceService`
    - `IPosManagementService`
    - `IReceptionRepository`
    - `ICurrentUserService`
    - `IFinancialYearService`
    - `InsurancePlanSuggestionService`
    - `IFactorSettingService`
    - `IPricingEngine`
    - `IReceptionPricingService`
- ✅ `Services/Reception/ReceptionPricingService.cs` - سرویس Pricing
  - Interface: `Interfaces/Reception/IReceptionPricingService.cs`
  - متدهای کلیدی:
    - `PriceItemAsync(item)` - محاسبه قیمت یک آیتم
    - `RepriceAllAsync(draftId)` - بازمحاسبه تمام آیتم‌ها
    - `CalculateTotalsAsync(draftId)` - محاسبه مجموع‌ها
    - `CheckInsuranceSetAsync(serviceId, basePlanId, suppPlanId)` - بررسی تعیین‌ست

#### **Supporting Services**
- ✅ `Services/Reception/ReceptionWorkflowService.cs` - Workflow Management
- ✅ `Services/Reception/ReceptionDepartmentDoctorService.cs` - مدیریت پزشک/دپارتمان
- ✅ `Services/Reception/ReceptionServiceManagementService.cs` - مدیریت خدمات
- ✅ `Services/Reception/ReceptionPaymentService.cs` - مدیریت پرداخت
- ✅ `Services/Reception/ReceptionCalculationService.cs` - محاسبات پذیرش

---

### **4️⃣ Reception V2 - Repositories**

#### **Core Repositories**
- ✅ `Repositories/ReceptionRepository.cs` - Repository اصلی پذیرش
  - Interface: `Interfaces/Repositories/IReceptionRepository.cs`
- ✅ `Repositories/Reception/OptimizedReceptionRepository.cs` - نسخه بهینه‌شده
- ✅ `Repositories/Patient/PatientRepository.cs` - Repository بیمار
  - Interface: `Interfaces/Repositories/IPatientRepository.cs`
- ✅ `Repositories/Insurance/PatientInsuranceRepository.cs` - Repository بیمه بیمار
  - Interface: `Interfaces/Insurance/IPatientInsuranceRepository.cs`

#### **Supporting Repositories**
- ✅ `Repositories/Reception/ClinicManagementRepository.cs` - مدیریت کلینیک
- ✅ `Repositories/Reception/DoctorManagementRepository.cs` - مدیریت پزشک
- ✅ `Repositories/Reception/ShiftManagementRepository.cs` - مدیریت شیفت

---

### **5️⃣ Insurance/Tariff Services**

#### **Insurance Calculation Services**
- ✅ `Services/Insurance/InsuranceCalculationService.cs`
  - Interface: `Interfaces/Insurance/IInsuranceCalculationService.cs`
- ✅ `Services/Insurance/InsuranceTariffCalculationService.cs`
  - Interface: `Interfaces/IInsuranceTariffCalculationService.cs`
- ✅ `Services/Insurance/CombinedInsuranceCalculationService.cs`
  - Interface: `Interfaces/Insurance/ICombinedInsuranceCalculationService.cs`
- ✅ `Services/Insurance/ServiceCalculationEngine.cs` - موتور محاسبه خدمات
- ✅ `Services/Insurance/BusinessRuleEngine.cs` - موتور قوانین کسب‌وکار
  - Interface: `Interfaces/Insurance/IBusinessRuleEngine.cs`
  - Repository: `Repositories/Insurance/BusinessRuleRepository.cs`

#### **Insurance Plan Services**
- ✅ `Services/Insurance/InsurancePlanService.cs`
  - Interface: `Interfaces/Insurance/IInsurancePlanService.cs`
  - Repository: `Repositories/Insurance/InsurancePlanRepository.cs`
- ✅ `Services/Insurance/PatientInsuranceService.cs`
  - Interface: `Interfaces/Insurance/IPatientInsuranceService.cs`
  - Repository: `Repositories/Insurance/PatientInsuranceRepository.cs`

#### **Tariff Services**
- ✅ `Services/Insurance/InsuranceTariffService.cs`
  - Interface: `Interfaces/Insurance/IInsuranceTariffService.cs`
  - Repository: `Repositories/Insurance/InsuranceTariffRepository.cs`

#### **Supplementary Insurance Services**
- ✅ `Services/Insurance/SupplementaryInsuranceService.cs`
  - Interface: `Interfaces/Insurance/ISupplementaryInsuranceService.cs`
- ✅ `Services/Insurance/CorrectSupplementaryInsuranceCalculationService.cs`
  - Interface: `Interfaces/Insurance/ISupplementaryInsuranceCalculationService.cs`

---

### **6️⃣ Doctor/Department Services**

#### **Doctor Services**
- ✅ `Services/ClinicAdmin/DoctorCrudService.cs` - CRUD پزشک
  - Interface: `Interfaces/ClinicAdmin/IDoctorCrudService.cs`
  - Repository: `Repositories/ClinicAdmin/DoctorCrudRepository.cs`
- ✅ `Services/ClinicAdmin/DoctorDepartmentService.cs` - مدیریت پزشک↔دپارتمان
  - Interface: `Interfaces/ClinicAdmin/IDoctorDepartmentService.cs`
  - Repository: `Repositories/ClinicAdmin/DoctorDepartmentRepository.cs`
- ✅ `Services/ClinicAdmin/DoctorServiceCategoryService.cs` - مدیریت پزشک↔خدمت
  - Interface: `Interfaces/ClinicAdmin/IDoctorServiceCategoryService.cs`
  - Repository: `Repositories/ClinicAdmin/DoctorServiceCategoryRepository.cs`

#### **Department Services**
- ✅ `Services/DepartmentManagementService.cs` - مدیریت دپارتمان
  - Interface: `Interfaces/ClinicAdmin/IDepartmentManagementService.cs`
  - Repository: `Repositories/DepartmentRepository.cs`

#### **Entities**
- ✅ `Models/Entities/Doctor/Doctor.cs` - موجودیت پزشک
- ✅ `Models/Entities/Doctor/DoctorDepartment.cs` - رابطه پزشک↔دپارتمان
- ✅ `Models/Entities/Doctor/DoctorServiceCategory.cs` - رابطه پزشک↔خدمت
- ✅ `Models/Entities/Clinic/Department.cs` - موجودیت دپارتمان

---

### **7️⃣ POS Payment Services**

#### **POS Terminal**
- ✅ `Models/Entities/Payment/PosTerminal.cs` - موجودیت ترمینال POS
- ✅ `Repositories/Payment/POS/PosTerminalRepository.cs` - Repository ترمینال
  - Interface: `Interfaces/Payment/POS/IPosTerminalRepository.cs`
- ✅ `Services/Payment/POS/PosManagementService.cs` - سرویس مدیریت POS
  - Interface: `Interfaces/Payment/POS/IPosManagementService.cs`

#### **Payment Transaction**
- ✅ `Models/Entities/Payment/PaymentTransaction.cs` - موجودیت تراکنش پرداخت
- ✅ `Repositories/Payment/PaymentTransactionRepository.cs` - Repository تراکنش
  - Interface: `Interfaces/Payment/IPaymentTransactionRepository.cs`

#### **POS Provider Interfaces** (نیاز به بررسی)
- ⚠️ `Interfaces/Payment/IPosProviderClient.cs` - Interface کلاینت POS (نیاز به بررسی وجود)
- ⚠️ `Interfaces/Payment/IPosProviderResolver.cs` - Interface Resolver POS (نیاز به بررسی وجود)
- ⚠️ `Interfaces/Payment/IPosPaymentService.cs` - Interface سرویس پرداخت POS (نیاز به بررسی وجود)

#### **POS Provider Implementations** (نیاز به بررسی)
- ⚠️ `Services/Payment/POS/PosProviderResolver.cs` - Resolver POS (نیاز به بررسی وجود)
- ⚠️ `Services/Payment/POS/Clients/FakePosClient.cs` - کلاینت Fake POS (نیاز به بررسی وجود)
- ⚠️ `Services/Payment/POS/PosPaymentService.cs` - سرویس پرداخت POS (نیاز به بررسی وجود)

#### **POS API Controllers**
- ✅ `Controllers/Payment/POS/PosTerminalApiController.cs` - API ترمینال POS
  - Routes:
    - `GET /api/v1/pos/terminals/{id}`
    - `PUT /api/v1/pos/terminals/{id}`
    - `GET /api/v1/pos/terminals/default`
    - `POST /api/v1/pos/terminals/{id}/default`
    - `POST /api/v1/pos/terminals/{id}/active`
    - `POST /api/v1/pos/process-payment`
- ✅ `Controllers/Payment/POS/PosManagementController.cs` - مدیریت POS

---

### **8️⃣ Reception V2 - Views**

#### **Main Views**
- ✅ `Views/ReceptionV2/Index.cshtml` - صفحه اصلی فرم پذیرش V2
  - Layout: `~/Views/Shared/_Layout.cshtml`
  - Model: `ReceptionFormVM`
  - Anti-Forgery Token: `@Html.AntiForgeryToken()`
- ✅ `Views/ReceptionV2/Edit.cshtml` - صفحه ویرایش پذیرش
- ✅ `Views/ReceptionV2/Print.cshtml` - صفحه چاپ پذیرش
- ✅ `Views/ReceptionV2/PrintInsurance.cshtml` - صفحه چاپ بیمه

#### **Partial Views**
- ✅ `Views/ReceptionV2/Partials/_ReceptionSummaryHeader.cshtml` - هدر خلاصه پذیرش
- ✅ `Views/ReceptionV2/Partials/_IdentitySection.cshtml` - بخش هویت
- ✅ `Views/ReceptionV2/Partials/_Patient.cshtml` - بخش بیمار
- ✅ `Views/ReceptionV2/Partials/_ClinicDept.cshtml` - بخش کلینیک/دپارتمان
- ✅ `Views/ReceptionV2/Partials/_Insurance.cshtml` - بخش بیمه
- ✅ `Views/ReceptionV2/Partials/_ServicePicker.cshtml` - انتخاب خدمت
- ✅ `Views/ReceptionV2/Partials/_ItemsGrid.cshtml` - جدول آیتم‌ها
- ✅ `Views/ReceptionV2/Partials/_Totals.cshtml` - بخش مجموع‌ها
- ✅ `Views/ReceptionV2/Partials/_Payment.cshtml` - بخش پرداخت
- ✅ `Views/ReceptionV2/Partials/_CoverageModal.cshtml` - مودال پوشش بیمه
- ✅ `Views/ReceptionV2/Partials/_PatientFastCreateModal.cshtml` - مودال ثبت سریع بیمار
- ✅ `Views/ReceptionV2/Partials/_PosPaymentModal.cshtml` - مودال پرداخت POS

---

### **9️⃣ Reception V2 - Scripts**

#### **Core Scripts**
- ✅ `Scripts/reception.v2/reception-api.js` - Wrapper API با Anti-Forgery Token
  - تزریق هدر `__RequestVerificationToken` در همه POSTها
  - Fallback برای خطاها
- ✅ `Scripts/reception.v2/reception-main.js` - اسکریپت اصلی
- ✅ `Scripts/reception.v2/reception-utils.js` - توابع کمکی

#### **Feature Scripts**
- ✅ `Scripts/reception.v2/patient-lookup.js` - جستجو/ایجاد بیمار
- ✅ `Scripts/reception.v2/clinic-dept-doctor.js` - مدیریت کلینیک/دپارتمان/پزشک
- ✅ `Scripts/reception.v2/service-lookup.js` - جستجو/افزودن خدمت
- ✅ `Scripts/reception.v2/insurance-panel.js` - مدیریت بیمه
- ✅ `Scripts/reception.v2/payment-panel.js` - مدیریت پرداخت
- ✅ `Scripts/reception.v2/pricing-ui.js` - UI محاسبات قیمت
- ✅ `Scripts/reception.v2/totals-panel.js` - نمایش مجموع‌ها
- ✅ `Scripts/reception.v2/auto-draft-manager.js` - مدیریت خودکار Draft
- ✅ `Scripts/reception.v2/form-change-detector.js` - تشخیص تغییرات فرم
- ✅ `Scripts/reception.v2/coverage-modal.js` - مودال پوشش بیمه
- ✅ `Scripts/reception.v2/reception-edit.js` - ویرایش پذیرش
- ✅ `Scripts/reception.v2/reception-list.js` - لیست پذیرش‌ها
- ✅ `Scripts/reception.v2/summary-header.js` - هدر خلاصه

---

### **🔟 ViewModels/DTOs**

#### **Reception DTOs**
- ✅ `ViewModels/Reception/ReceptionFacadeDtos.cs` - DTOs اصلی Facade
  - `ReceptionLoadDto` - داده‌های اولیه
  - `PatientDto` - اطلاعات بیمار
  - `DoctorDto` - اطلاعات پزشک
  - `ServicePickListDto` - لیست خدمات
  - `AddItemRequest` / `AddItemResultDto` - افزودن آیتم
  - `SetInsurancesRequest` - تنظیم بیمه‌ها
  - `ItemsAndTotalsDto` - آیتم‌ها و مجموع‌ها
  - `FinalizePosRequest` / `FinalizeCashRequest` - نهایی‌سازی
  - `FinalizeResponse` - پاسخ نهایی‌سازی
  - `InsuranceBundleDto` - بسته بیمه
  - `CreateDraftRequest` / `CreateDraftResponse` - ایجاد Draft
  - `UpdateReceptionRequest` / `UpdateReceptionResponse` - به‌روزرسانی
  - `CancelReceptionRequest` / `CancelReceptionResponse` - لغو

#### **Reception ViewModels**
- ✅ `ViewModels/Reception/ReceptionFormVM.cs` - ViewModel فرم پذیرش
- ✅ `ViewModels/Reception/ReceptionInsuranceViewModel.cs` - ViewModel بیمه
- ✅ `ViewModels/Reception/ReceptionInsuranceShareViewModel.cs` - سهم بیمه

---

### **1️⃣1️⃣ Models/Entities**

#### **Reception Entities**
- ✅ `Models/Entities/Reception/Reception.cs` - موجودیت پذیرش
  - Properties: `Id`, `PatientId`, `ClinicId`, `DepartmentId`, `DoctorId`, `Status`, `TotalAmount`, `RowVersion`
- ✅ `Models/Entities/Reception/ReceptionItem.cs` - موجودیت آیتم پذیرش
  - Properties: `Id`, `ReceptionId`, `ServiceId`, `Quantity`, `UnitPrice`, `TotalPrice`, `RowVersion`

#### **Insurance Entities**
- ✅ `Models/Entities/Insurance/InsuranceProvider.cs` - ارائه‌دهنده بیمه
- ✅ `Models/Entities/Insurance/InsurancePlan.cs` - پلن بیمه
- ✅ `Models/Entities/Insurance/InsuranceTariff.cs` - تعرفه بیمه
- ✅ `Models/Entities/Insurance/InsuranceCalculation.cs` - محاسبه بیمه
- ✅ `Models/Entities/Insurance/BusinessRule.cs` - قانون کسب‌وکار
- ✅ `Models/Entities/Patient/PatientInsurance.cs` - بیمه بیمار

#### **Payment Entities**
- ✅ `Models/Entities/Payment/PosTerminal.cs` - ترمینال POS
  - Properties: `Id`, `Name`, `Provider`, `IsActive`, `IsDefault`
- ✅ `Models/Entities/Payment/PaymentTransaction.cs` - تراکنش پرداخت
  - Properties: `TransactionId`, `ReceptionId`, `Amount`, `Method`, `Status`

---

## 📊 **آمار کلی - Reception V2**

| دسته‌بندی | تعداد | وضعیت |
|----------|------|-------|
| Controllers (API) | 1 | ✅ |
| Controllers (MVC) | 2 | ✅ |
| Services (Core) | 6 | ✅ |
| Services (Supporting) | 10+ | ✅ |
| Repositories | 8+ | ✅ |
| Views (Main) | 4 | ✅ |
| Views (Partials) | 12 | ✅ |
| Scripts | 14 | ✅ |
| DTOs/ViewModels | 20+ | ✅ |
| Entities | 10+ | ✅ |

---

## 🔍 **نقاط نیازمند بررسی**

### **POS Payment (فاز F)**
- ⚠️ بررسی وجود `IPosProviderClient`, `IPosProviderResolver`, `IPosPaymentService`
- ⚠️ بررسی وجود `PosProviderResolver`, `FakePosClient`, `PosPaymentService`
- ⚠️ بررسی وجود `pos-payment.js` برای مدیریت پرداخت POS

### **Bootstrap Endpoint (فاز C)**
- ⚠️ بررسی کامل بودن `GET /api/v1/reception/bootstrap`
- ⚠️ بررسی وجود `PosTerminals` و `DefaultPosTerminalId` در پاسخ
- ⚠️ بررسی Lazy Loading برای Doctors

### **Pricing Endpoints (فاز D)**
- ⚠️ بررسی کامل بودن `POST /api/v1/reception/insurances/set`
- ⚠️ بررسی کامل بودن `POST /api/v1/reception/item/add` با Pricing
- ⚠️ بررسی کامل بودن `POST /api/v1/reception/item/update` با Reprice

---

**تاریخ به‌روزرسانی:** 2025-01-27  
**نسخه:** 2.0.0  
**وضعیت:** ✅ فاز A تکمیل شد
