# 📋 **فهرست‌برداری پروژه ClinicApp**

**تاریخ ایجاد:** 2024  
**هدف:** فهرست‌برداری جامع از ساختار پروژه، وابستگی‌ها و قراردادها

---

## 📁 **ساختار پروژه**

### **1️⃣ Models/** (122 فایل)

#### **Entities/** (48 موجودیت)
- `Models/Entities/Clinic/` - Clinic, Department, Service, ServiceCategory, ServiceTemplate, ServiceComponent, SharedService, FactorSetting
- `Models/Entities/Patient/` - Patient, PatientInsurance, MedicalHistory
- `Models/Entities/Doctor/` - Doctor, Specialization, DoctorSchedule, DoctorAssignmentHistory, DoctorTimeSlot, DoctorDepartment, DoctorWorkDay, DoctorTimeRange, ScheduleTemplate, ScheduleException, DoctorServiceCategory, DoctorSpecialization
- `Models/Entities/Reception/` - Reception, ReceptionItem
- `Models/Entities/Insurance/` - InsuranceProvider, InsurancePlan, PlanService, InsuranceTariff, InsuranceCalculation, BusinessRule, InsuranceType
- `Models/Entities/Payment/` - PaymentTransaction, OnlinePayment, PosTerminal, PaymentGateway, CashSession
- `Models/Entities/Triage/` - TriageProtocol, TriageAssessment, TriageQueue, TriageVitalSigns, TriageReassessment
- `Models/Entities/Appointment/` - Appointment, AppointmentSlot
- `Models/Entities/Notification/` - NotificationHistory, NotificationTemplate
- `Models/Entities/Receipt/` - ReceiptPrint
- `Models/Entities/Report/` - Report

#### **Core/** (Interfaces & Base Classes)
- `ApplicationUser` (extends IdentityUser)
- `ISoftDelete`
- `ITrackable`
- `AuditableEntity`

#### **Enums/** (37 فایل)
- Gender, ReceptionStatus, ReceptionType, PaymentStatus, PaymentMethod
- InsuranceCalculationType, InsurancePriority, InsuranceType
- PosProtocol, PosProviderType, PaymentGatewayType, OnlinePaymentType
- ServiceComponentType, ServicePriceCalculationType, FactorScope
- TriageEnums, AppointmentStatus, AppointmentType, AppointmentPriority, AppointmentSlotStatus
- CashSessionStatus, OnlinePaymentStatus
- EmergencyType, EmergencyPriority, EmergencyConflictSeverity, EmergencyBookingStatus
- WorkloadBalanceStatus, WorkLifeBalanceStatus
- NotificationStatus, NotificationChannelType
- ShiftType, BreakType, TemplateType, ExceptionType
- ReportType, Degree, AssignmentHistoryImportance, MedicalHistoryType

#### **ViewModels/** (236 فایل)
- `ViewModels/Reception/**` - ReceptionFormVM, ReceptionDraftDtos, ReceptionFacadeDtos, SidebarModels, PaymentStatus, ...

---

### **2️⃣ Controllers/** (83 فایل)

#### **Reception/** (20 کنترلر)
- `ReceptionController.cs` (Legacy)
- `ReceptionControllerV2.cs` (V2 در Controllers/Reception)
- `ReceptionV2/ReceptionControllerV2.cs` (V2 در Controllers/ReceptionV2)
- `ReceptionFacadeController.cs`
- `ReceptionAlertController.cs`
- `ReceptionCalculationController.cs`
- `ReceptionDepartmentController.cs`
- `ReceptionDepartmentDoctorController.cs`
- `ReceptionFormController.cs`
- `ReceptionHistoryController.cs`
- `ReceptionInsuranceAutoController.cs`
- `ReceptionInsuranceController.cs`
- `ReceptionInsuranceFormController.cs`
- `ReceptionListController.cs`
- `ReceptionPatientController.cs`
- `ReceptionPatientIdentityController.cs`
- `ReceptionPatientSearchController.cs`
- `ReceptionPaymentController.cs`
- `ReceptionServiceController.cs`
- `ReceptionServiceManagementController.cs`
- `ReceptionStatisticsController.cs`
- `ReceptionTriageIntegrationController.cs`

#### **Api/** (7 کنترلر)
- `ReceptionApiController.cs` (با RoutePrefix("api/v1/reception"))
- `ReceptionApiDtos.cs`
- `ClinicController.cs`
- `DoctorController.cs`
- `InsuranceController.cs`
- `PatientController.cs`
- `ServiceController.cs`

#### **Payment/** (4 کنترلر)
- `PaymentController.cs`
- `Payment/Gateway/PaymentGatewayController.cs`
- `Payment/POS/PosManagementController.cs`
- `Payment/POS/PosTerminalApiController.cs`

#### **Triage/** (6 کنترلر)
- `TriageController.cs`
- `TriageDashboardController.cs`
- `TriageProtocolController.cs`
- `TriageQueueController.cs`
- `TriageReassessmentController.cs`
- `TriageReportController.cs`

#### **Base/** (2 کنترلر)
- `BaseController.cs`
- `Base/OptimizedBaseController.cs`
- `Base/PersianDateController.cs`

#### **Areas/Admin/** (35+ کنترلر)
- Service Management, Insurance Management, Doctor Management, Clinic Management, ...

---

### **3️⃣ Services/** (123 فایل)

#### **Reception/** (25+ سرویس)
- `ReceptionFacade.cs` (Orchestrator اصلی)
- `ReceptionWorkflowService.cs`
- `ReceptionEventHandler.cs`
- `ReceptionStateMachine.cs`
- `ReceptionServiceManagementService.cs`
- `ReceptionSecurityService.cs`
- `ReceptionPaymentService.cs`
- `ReceptionPatientIdentityService.cs`
- `ReceptionNavigationService.cs`
- `ReceptionInsuranceAutoService.cs`
- `ReceptionInformationService.cs`
- `ReceptionFormService.cs`
- `ReceptionDomainService.cs`
- `ReceptionDepartmentService.cs`
- `ReceptionDepartmentDoctorService.cs`
- `ReceptionCalculationService.cs`
- `ReceptionBusinessRulesEngine.cs`
- `ReceptionBusinessRules.cs`
- `ReceptionPatientService.cs`
- `ReceptionSidebarService.cs`
- `ReceptionTriageIntegrationService.cs`
- `ReceptionValidationOrchestrator.cs`
- `ReceptionTransitionRules.cs`
- `MedicalEmergencyService.cs`
- `EventStore.cs`
- Event Handlers: `SmsNotificationEventHandler`, `PushNotificationEventHandler`, `PaymentProcessingEventHandler`, `PatientValidationEventHandler`, `NotificationSendingEventHandler`, `InsuranceValidationEventHandler`, `EmailNotificationEventHandler`, `AuditLoggingEventHandler`, `AnalyticsEventHandler`

#### **Insurance/** (20+ سرویس)
- `InsuranceCalculationService.cs`
- `InsuranceTariffCalculationService.cs`
- `CombinedInsuranceCalculationService.cs`
- `ServiceCalculationEngine.cs`
- `InsurancePlanSuggestionService.cs`
- `InsuranceRulesService.cs`
- `InsuranceTariffService.cs`
- `InsuranceProviderService.cs`
- `InsurancePlanService.cs`
- `InsuranceValidationService.cs`
- `PatientInsuranceService.cs`
- `PatientInsuranceManagementService.cs`
- `PatientInsuranceValidationService.cs`
- `TariffDomainValidationService.cs`
- `SupplementaryInsuranceService.cs`
- `SupplementaryInsuranceOptimizationService.cs`
- `SupplementaryInsuranceMonitoringService.cs`
- `SupplementaryInsuranceDataFixService.cs`
- `SupplementaryCombinationService.cs`
- `AdvancedInsuranceCalculationService.cs`
- `BusinessRuleEngine.cs`
- `BulkInsuranceTariffService.cs`
- `BulkSupplementaryTariffService.cs`
- `CorrectSupplementaryInsuranceCalculationService.cs`
- `CombinedInsuranceCalculationTestService.cs`
- `SupplementaryTariffSeederService.cs`

#### **Payment/** (5 سرویس)
- `PaymentService.cs`
- `Payment/Gateway/PaymentGatewayService.cs`
- `Payment/Web/WebPaymentService.cs`
- `Payment/POS/PosManagementService.cs`
- `Payment/Reporting/PaymentReportingService.cs`

#### **Finance/** (2 سرویس)
- `Finance/DbFinancialYearService.cs` (پیاده‌سازی IFinancialYearService)
- `Finance/InsuranceTariffCalculationService.cs`

#### **Triage/** (4 سرویس)
- `Triage/TriageService.cs`
- `Triage/TriageQueueService.cs`
- `Triage/TriageWorkflowIntegration.cs`

#### **SystemSettings/** (2 سرویس)
- `SystemSettings/SystemSettingService.cs`
- `SystemSettings/ISystemSettingService.cs`

#### **UserContext/** (2 سرویس)
- `UserContext/UserContextService.cs`
- `UserContext/IUserContextService.cs`

#### **Idempotency/** (2 سرویس)
- `Idempotency/InMemoryIdempotencyService.cs`
- `Idempotency/IIdempotencyService.cs`

#### **DataSeeding/** (6 سرویس)
- `DataSeeding/SystemSeedService.cs`
- `DataSeeding/ServiceSeedService.cs`
- `DataSeeding/ServiceTemplateSeedService.cs`
- `DataSeeding/InsuranceTypeUpdateService.cs`
- `DataSeeding/FactorSettingSeedService.cs`

#### **دیگر سرویس‌ها**
- `ServiceCalculationService.cs`
- `ServiceService.cs`
- `ServiceCategoryService.cs`
- `ServiceManagementService.cs`
- `DepartmentManagementService.cs`
- `PatientService.cs`
- `ReceptionService.cs`
- `CurrentUserService.cs`
- `BackgroundCurrentUserService.cs`
- `ShiftHelperService.cs`
- `ShiftInfo.cs`
- `Calculation/TariffCalculator.cs`

---

### **4️⃣ Repositories/** (40 فایل)

#### **Reception/**
- `ReceptionRepository.cs` (Base)
- `Reception/OptimizedReceptionRepository.cs`
- `Reception/ClinicManagementRepository.cs`
- `Reception/DoctorManagementRepository.cs`
- `Reception/ShiftManagementRepository.cs`
- `Reception/IShiftManagementRepository.cs`
- `Reception/IClinicManagementRepository.cs`
- `Reception/IDoctorManagementRepository.cs`

#### **Patient/**
- `Patient/PatientRepository.cs`

#### **Insurance/**
- `Insurance/PatientInsuranceRepository.cs`
- `Insurance/InsuranceProviderRepository.cs`
- `Insurance/InsurancePlanRepository.cs`
- `Insurance/PlanServiceRepository.cs`
- `Insurance/InsuranceTariffRepository.cs`
- `Insurance/InsuranceCalculationRepository.cs`
- `Insurance/BusinessRuleRepository.cs`

#### **Payment/**
- `Payment/PaymentTransactionRepository.cs`
- `Payment/OnlinePaymentRepository.cs`
- `Payment/Gateway/PaymentGatewayRepository.cs`
- `Payment/POS/PosTerminalRepository.cs`
- `Payment/POS/CashSessionRepository.cs`

#### **ClinicAdmin/**
- `ClinicAdmin/DoctorAssignmentRepository.cs`
- `ClinicAdmin/DoctorDashboardRepository.cs`
- `ClinicAdmin/DoctorReportingRepository.cs`
- `ClinicAdmin/DoctorServiceCategoryRepository.cs`
- `ClinicAdmin/DoctorScheduleRepository.cs`
- `ClinicAdmin/DoctorDepartmentRepository.cs`
- `ClinicAdmin/DoctorAssignmentHistoryRepository.cs`
- `ClinicAdmin/IDoctorAssignmentHistoryRepository.cs`
- `ClinicAdmin/SpecializationRepository.cs`
- `ClinicAdmin/DoctorCrudRepository.cs`

#### **Base/**
- `Base/BaseRepository.cs`

#### **دیگر**
- `ServiceRepository.cs`
- `ServiceCategoryRepository.cs`
- `DepartmentRepository.cs`
- `ClinicRepository.cs`

#### **Interfaces/**
- `Interfaces/Repositories/IPatientRepository.cs`
- `Interfaces/Repositories/IDoctorRepository.cs`
- `Interfaces/Repositories/IPaymentRepository.cs`
- `Interfaces/Repositories/IInsuranceRepository.cs`

---

### **5️⃣ Views/** (77 فایل .cshtml)

#### **Reception/**
- Legacy Views در `Views/Reception/`
- `Views/ReceptionV2/Index.cshtml` (V2)
- `Views/ReceptionV2/Print.cshtml`
- `Views/ReceptionV2/Partials/` - _ClinicDept.cshtml, _Insurance.cshtml, _ItemsGrid.cshtml, _Patient.cshtml, _Payment.cshtml, _ServicePicker.cshtml, _Totals.cshtml

---

### **6️⃣ Scripts/** (187 فایل)

#### **reception.v2/** (11 فایل)
- `reception-api.js` (Wrapper با fallback + Anti-Forgery)
- `reception-main.js`
- `reception-utils.js`
- `patient-lookup.js`
- `clinic-dept-doctor.js`
- `service-lookup.js`
- `insurance-panel.js`
- `payment-panel.js`
- `auto-draft-manager.js`
- `totals-panel.js`
- `form-change-detector.js`

#### **reception/** (Legacy)
- `reception-main.js`
- `reception-modules.js`
- `modules/payment-processing.js`
- ...

---

### **7️⃣ App_Start/** (10 فایل)

- `RouteConfig.cs` (Attribute Routing + Legacy Routes)
- `BundleConfig.cs`
- `FilterConfig.cs`
- `UnityConfig.cs` (DI Container Registration)
- `IdentityConfig.cs`
- `Startup.Auth.cs`
- `UnityMvcActivator.cs`
- `IdentitySeed.cs`
- `DataSeeding/` (6 فایل Seed)

---

### **8️⃣ Helpers/** (35 فایل)

- `ServiceResult.cs`
- `ValidationResult.cs`
- `SecurityValidationResult.cs`
- `PersianDateHelper.cs`
- `PersianDatePickerHelper.cs`
- `CultureHelper.cs`
- `PhoneNumberHelper.cs`
- `PhoneNumberValidator.cs`
- `IranianNationalCodeValidator.cs`
- `InsurancePriorityHelper.cs`
- `AgeCalculationHelper.cs`
- `LoggingHelper.cs`
- `StructuredLogger.cs`
- `AppHelper.cs`
- `AppSettings.cs`
- `ApplicationVersion.cs`
- `DynamicSqlHelper.cs`
- `DynamicSqlConfiguration.cs`
- `SafeSqlBuilder.cs`
- `RegexHelper.cs`
- `ErrorMessageHelper.cs`
- `HtmlHelpers.cs`
- `IdentityExtensions.cs`
- `ReceptionAjaxHelper.cs`
- `RateLimiter.cs`
- `SystemUsers.cs`
- `AntiForgeryTokenHelper.cs`
- `Insurance/` (Helper classes)
- `Security/` (Helper classes)
- `Validation/` (Helper classes)
- `MedicalReportExcelGenerator.cs`

---

### **9️⃣ Filters/** (12 فایل)

- `NoCacheAttribute.cs`
- `NoCacheFilter.cs`
- `NoStoreAttribute.cs`
- `CultureFilter.cs`
- `CorrelationIdFilter.cs`
- `GlobalExceptionFilter.cs`
- `RequestTimingFilter.cs`
- `ValidateAntiForgeryTokenOnPostsAttribute.cs`
- `PersianDateAttribute.cs`
- `CheckProfileCompletionAttribute.cs`
- `MedicalEnvironmentFilter.cs`
- `ReceptionExportHelper.cs`

---

### **🔟 Extensions/** (6 فایل)

- `ApplicationUserManagerExtensions.cs`
- `CultureExtensions.cs`
- `DateTimeExtensions.cs`
- `EnumExtensions.cs`
- `GenderParsing.cs`
- `PersianDateExtensions.cs`

---

### **1️⃣1️⃣ Interfaces/** (105 فایل)

#### **Reception/**
- `Interfaces/Reception/IReceptionFacade.cs`

#### **Finance/**
- `Interfaces/Finance/IFinancialYearService.cs`

#### **Triage/**
- `Interfaces/Triage/ITriageService.cs`
- `Interfaces/Triage/ITriageQueueService.cs`

#### **UserContext/**
- `Interfaces/UserContext/IUserContextService.cs`

#### **SystemSettings/**
- `Interfaces/SystemSettings/ISystemSettingService.cs`

#### **Payment/**
- `Interfaces/Payment/IPaymentService.cs`
- `Interfaces/Payment/IPosManagementService.cs`

#### **Insurance/**
- `Interfaces/Insurance/IInsuranceCalculationService.cs`
- `Interfaces/Insurance/IInsuranceRulesService.cs`
- `Interfaces/Insurance/IPatientInsuranceService.cs`
- `Interfaces/Insurance/IPatientInsuranceValidationService.cs`
- `Interfaces/Insurance/IPatientInsuranceManagementService.cs`

#### **Repositories/**
- `Interfaces/Repositories/IPatientRepository.cs`
- `Interfaces/Repositories/IDoctorRepository.cs`
- `Interfaces/Repositories/IPaymentRepository.cs`
- `Interfaces/Repositories/IInsuranceRepository.cs`
- `Interfaces/Repositories/IReceptionRepository.cs`

#### **Services/**
- `Interfaces/Service/IServiceCalculationService.cs`
- `Interfaces/Service/IServiceService.cs`
- `Interfaces/Service/IFactorSettingService.cs`

---

### **1️⃣2️⃣ Contracts/** (6 فایل)

- `01-PreFlight-Protocol.md`
- `02-Architecture-Guidelines.md`
- `03-Code-Quality-Standards.md`
- `04-Security-Requirements.md`
- `Bugfix-Master-Contract.md`
- `MODULE_ANALYSIS_CONTRACT.md`
- `DEBUGGING_SPECIALIST_CONTRACT.md`

---

## 📊 **آمار کلی**

| لایه | تعداد فایل |
|------|-----------|
| Entities | 48 |
| Enums | 37 |
| ViewModels | 236 |
| Controllers | 83 |
| Services | 123 |
| Repositories | 40 |
| Views | 77 (.cshtml) |
| Scripts | 187 (.js) |
| Helpers | 35 |
| Filters | 12 |
| Extensions | 6 |
| Interfaces | 105 |
| **مجموع** | **~896 فایل** |

---

## 🔗 **وابستگی‌های اصلی**

```
Entity (48) 
  ↓
EntityTypeConfiguration (Models/Configurations)
  ↓
ApplicationDbContext (Models/IdentityModels.cs)
  ↓
Repository Interface → Repository Implementation (40)
  ↓
Service Interface → Service Implementation (123)
  ↓
Facade (ReceptionFacade)
  ↓
Controller (83) → View (77) + Scripts (187)
```

---

**تاریخ به‌روزرسانی:** 2024  
**نسخه:** 1.0

