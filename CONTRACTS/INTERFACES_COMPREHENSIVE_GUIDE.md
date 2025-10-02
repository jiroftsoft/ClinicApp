# 📚 **راهنمای جامع Interfaces - سیستم کلینیک شفا**

> **تاریخ ایجاد**: 1404/07/11  
> **نسخه**: 1.0  
> **وضعیت**: نهایی شده

---

## 📑 **فهرست**

1. [مقدمه](#مقدمه)
2. [Core Interfaces](#core-interfaces)
3. [ClinicAdmin Interfaces](#clinicadmin-interfaces)
4. [Insurance Interfaces](#insurance-interfaces)
5. [Payment Interfaces](#payment-interfaces)
6. [OTP & Repositories](#otp--repositories)
7. [الگوهای طراحی](#الگوهای-طراحی)

---

## 🎯 **مقدمه**

این پروژه شامل **86 Interface** است که به صورت **گروه‌بندی شده** و **سازماندهی شده** طراحی شده‌اند. تمام Interface ها بر اساس اصول:

- ✅ **SOLID Principles**
- ✅ **Clean Architecture**
- ✅ **Separation of Concerns**
- ✅ **High Testability**
- ✅ **Medical Environment Standards**

طراحی شده‌اند.

---

## 1️⃣ **Core Interfaces (Root Level - 21 فایل)**

### 📌 **دسته‌بندی کلی:**

| **دسته** | **Interface ها** | **توضیحات** |
|---------|-----------------|-------------|
| **Authentication & User** | `IAuthService`, `ICurrentUserService` | مدیریت احراز هویت OTP و اطلاعات کاربر جاری |
| **Patient Management** | `IPatientService` | مدیریت کامل بیماران (CRUD, Search, Insurance, History) |
| **Reception Management** | `IReceptionService`, `IReceptionRepository` | مدیریت پذیرش‌های بیماران (قلب سیستم) |
| **Service Management** | `IServiceService`, `IServiceCategoryService`, `IServiceCalculationService` | مدیریت خدمات پزشکی، دسته‌بندی و محاسبات قیمت |
| **Factor Settings** | `IFactorSettingService` | مدیریت کای‌ها (ضرایب فنی/حرفه‌ای) برای محاسبات |
| **Notification** | `INotificationService`, `INotificationHistoryService`, `INotificationTemplateService`, `INotificationChannel` | سیستم اطلاع‌رسانی چندکاناله |
| **Security & External** | `ISecurityTokenService`, `IExternalInquiryService` | مدیریت توکن امنیتی و استعلام خارجی |
| **Utility** | `IAppSettings`, `PagedResult<T>`, `IServiceRepository` | تنظیمات سیستم، صفحه‌بندی، Repository عمومی |

---

### 🔷 **Interface های کلیدی:**

#### **1. `ICurrentUserService`** - مدیریت کاربر جاری

```csharp
public interface ICurrentUserService
{
    // Core Properties
    string UserId { get; }
    string UserName { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsDoctor { get; }
    bool IsReceptionist { get; }
    bool IsPatient { get; }
    DateTime UtcNow { get; }
    DateTime Now { get; }
    ClaimsPrincipal ClaimsPrincipal { get; }
    IEnumerable<string> Roles { get; }
    
    // Security Methods
    bool IsInRole(string role);
    bool HasPermission(string permission);
    Task<bool> HasAccessToServiceAsync(int serviceId);
    Task<bool> HasAccessToInsuranceAsync(int insuranceId);
    Task<bool> HasAccessToDepartmentAsync(int departmentId);
    
    // Helper Methods
    Task<Doctor> GetDoctorInfoAsync();
    Task<Patient> GetPatientInfoAsync();
    Task<List<Department>> GetDoctorActiveDepartmentsAsync();
    Task<List<ServiceCategory>> GetDoctorAuthorizedServiceCategoriesAsync();
}
```

**🎯 استفاده در:**
- همه کنترلرها برای دسترسی به اطلاعات کاربر
- سرویس‌ها برای Audit Trail و Authorization
- فیلترها برای بررسی دسترسی

---

#### **2. `IAuthService`** - احراز هویت با OTP

```csharp
public interface IAuthService
{
    // OTP Login Flow
    Task<ServiceResult> SendLoginOtpAsync(string nationalCode);
    Task<ServiceResult> VerifyLoginOtpAndSignInAsync(string nationalCode, string otpCode);
    Task<ServiceResult> SignInWithNationalCodeAsync(string nationalCode);
    void SignOut();
    
    // Registration Flow
    Task<ServiceResult> CheckUserExistsAsync(string nationalCode);
    Task<ServiceResult> SendRegistrationOtpAsync(string nationalCode, string phoneNumber);
    Task<ServiceResult> VerifyRegistrationOtpAsync(string nationalCode, string phoneNumber, string otpCode);
    
    // Authentication State
    bool IsAuthenticated { get; }
    string GetCurrentUserId();
}
```

**🎯 ویژگی‌های کلیدی:**
- سیستم پسورد‌لس (Passwordless) با OTP
- پشتیبانی از کد ملی به عنوان UserName
- مدیریت جداگانه ورود و ثبت‌نام

---

#### **3. `IPatientService`** - مدیریت بیماران

```csharp
public interface IPatientService
{
    // CRUD Operations
    Task<ServiceResult> RegisterPatientAsync(RegisterPatientViewModel model, string userIp);
    Task<ServiceResult> CreatePatientAsync(PatientCreateEditViewModel model);
    Task<ServiceResult> UpdatePatientAsync(PatientCreateEditViewModel model);
    Task<ServiceResult> DeletePatientAsync(int patientId);
    
    // Retrieval Operations
    Task<ServiceResult<PagedResult<PatientIndexViewModel>>> SearchPatientsAsync(string searchTerm, int pageNumber, int pageSize);
    Task<ServiceResult<PatientDetailsViewModel>> GetPatientDetailsAsync(int patientId);
    Task<ServiceResult<PatientCreateEditViewModel>> GetPatientForEditAsync(int patientId);
    
    // Lookup Operations
    Task<ServiceResult<List<PatientLookupViewModel>>> GetActivePatientsForLookupAsync();
    Task<ServiceResult<PagedResult<PatientIndexViewModel>>> SearchPatientsForSelect2Async(string query, int page, int pageSize);
    
    // Validation Operations
    Task<bool> CheckNationalCodeExistsAsync(string nationalCode);
    Task<ServiceResult> CheckPatientDependenciesAsync(int patientId);
    
    // Related Data Operations
    Task<ServiceResult<List<PatientInsuranceViewModel>>> GetPatientInsurancesAsync(int patientId, int pageNumber, int pageSize);
    Task<ServiceResult<List<PatientAppointmentViewModel>>> GetPatientAppointmentsAsync(int patientId, int pageNumber, int pageSize);
    Task<ServiceResult<List<PatientReceptionViewModel>>> GetPatientReceptionsAsync(int patientId, int pageNumber, int pageSize);
}
```

**🎯 ویژگی‌های کلیدی:**
- Soft Delete برای حفظ اطلاعات پزشکی
- مدیریت کامل بیمه‌های بیمار
- تاریخچه کامل پذیرش‌ها و نوبت‌ها
- پشتیبانی از Select2 برای جستجوی پیشرفته

---

#### **4. `IReceptionService`** - مدیریت پذیرش (قلب سیستم)

```csharp
public interface IReceptionService
{
    // Core CRUD Operations
    Task<ServiceResult<ReceptionDetailsViewModel>> CreateReceptionAsync(ReceptionCreateViewModel model);
    Task<ServiceResult<ReceptionDetailsViewModel>> UpdateReceptionAsync(ReceptionEditViewModel model);
    Task<ServiceResult> DeleteReceptionAsync(int id);
    Task<ServiceResult<ReceptionDetailsViewModel>> GetReceptionDetailsAsync(int id);
    
    // Search and List Operations
    Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetReceptionsAsync(
        int? patientId, int? doctorId, ReceptionStatus? status, string searchTerm, int pageNumber, int pageSize);
    Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> SearchReceptionsAsync(string searchTerm, int pageNumber, int pageSize);
    Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetPatientReceptionsAsync(int patientId, int pageNumber, int pageSize);
    Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetDoctorReceptionsAsync(int doctorId, DateTime? date, int pageNumber, int pageSize);
    
    // Patient Lookup Operations
    Task<ServiceResult<ReceptionPatientLookupViewModel>> LookupPatientByNationalCodeAsync(string nationalCode);
    Task<ServiceResult<PagedResult<ReceptionPatientLookupViewModel>>> SearchPatientsByNameAsync(string name, int pageNumber, int pageSize);
    Task<ServiceResult<ReceptionPatientLookupViewModel>> CreatePatientInlineAsync(PatientCreateEditViewModel model);
    
    // Service and Doctor Lookup Operations
    Task<ServiceResult<List<ReceptionServiceCategoryLookupViewModel>>> GetServiceCategoriesAsync();
    Task<ServiceResult<List<ReceptionServiceLookupViewModel>>> GetServicesByCategoryAsync(int categoryId);
    Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsAsync();
    Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsBySpecializationAsync(int specializationId);
    Task<ServiceResult<List<ReceptionDoctorDepartmentLookupViewModel>>> GetDoctorDepartmentsAsync(int doctorId);
    
    // Insurance Operations
    Task<ServiceResult<List<ReceptionPatientInsuranceLookupViewModel>>> GetPatientActiveInsurancesAsync(int patientId);
    Task<ServiceResult<ReceptionCostCalculationViewModel>> CalculateReceptionCostsAsync(
        int patientId, List<int> serviceIds, int? insuranceId, DateTime receptionDate);
    
    // External Inquiry Operations
    Task<ServiceResult<PatientInquiryViewModel>> InquiryPatientInfoAsync(string nationalCode, DateTime birthDate);
    Task<ServiceResult<PatientInquiryViewModel>> InquiryPatientIdentityAsync(string nationalCode, DateTime birthDate);
    
    // Business Logic Operations
    Task<ServiceResult<ReceptionValidationViewModel>> ValidateReceptionAsync(int patientId, int doctorId, DateTime receptionDate);
    Task<ServiceResult<ReceptionDailyStatsViewModel>> GetDailyStatsAsync(DateTime date);
    Task<ServiceResult<ReceptionDoctorStatsViewModel>> GetDoctorStatsAsync(int doctorId, DateTime date);
    
    // Payment Operations
    Task<ServiceResult<PaymentTransactionViewModel>> AddPaymentAsync(int receptionId, PaymentTransactionCreateViewModel paymentModel);
    Task<ServiceResult<List<PaymentTransactionViewModel>>> GetReceptionPaymentsAsync(int receptionId);
    
    // Lookup Lists for UI
    Task<ServiceResult<ReceptionLookupListsViewModel>> GetLookupListsAsync();
}
```

**🎯 ویژگی‌های کلیدی:**
- ماژول پذیرش = **قلب تپنده سیستم**
- یکپارچه‌سازی با سیستم‌های بیمار، دکتر، خدمات، بیمه، پرداخت
- استعلام خارجی (شبکه شمس) برای اطلاعات بیمار
- محاسبات بیمه در حین پذیرش
- مدیریت کامل Lookup Lists برای UI

---

#### **5. `IServiceCalculationService`** - محاسبات قیمت خدمات

```csharp
public interface IServiceCalculationService
{
    // Basic Calculation Methods
    decimal CalculateServicePrice(Service service);
    decimal CalculateServicePrice(int serviceId, ApplicationDbContext context);
    decimal CalculateServicePriceWithFactorSettings(Service service, ApplicationDbContext context, 
        DateTime? date = null, int? departmentId = null, int? financialYear = null);
    ServiceCalculationDetails CalculateServicePriceWithDetails(Service service, ApplicationDbContext context,
        DateTime? date = null, int? departmentId = null, int? financialYear = null);
    
    // Shared Service Calculation Methods
    Task<ServiceCalculationResult> CalculateSharedServicePriceAsync(int serviceId, int departmentId, 
        ApplicationDbContext context, decimal? overrideTechnicalFactor = null, 
        decimal? overrideProfessionalFactor = null, DateTime? date = null);
    Task<bool> IsServiceSharedInDepartmentAsync(int serviceId, int departmentId, ApplicationDbContext context);
    
    // Validation and Helper Methods
    bool HasCompleteComponents(Service service);
    ServiceComponent GetTechnicalComponent(Service service);
    ServiceComponent GetProfessionalComponent(Service service);
    int GetCurrentFinancialYear(DateTime? date = null);
    bool IsFinancialYearFrozen(int financialYear, ApplicationDbContext context);
    
    // Advanced Calculation Methods
    decimal CalculateServicePriceWithDiscount(decimal basePrice, decimal discountPercent);
    decimal CalculateServicePriceWithTax(decimal basePrice, decimal taxPercent);
    decimal CalculateServicePriceWithHashtagLogic(Service service, ApplicationDbContext context, DateTime? date = null);
    decimal CalculateServicePriceWithDepartmentOverride(Service service, int departmentId, 
        ApplicationDbContext context, DateTime? date = null);
}
```

**🎯 فرمول محاسبه:**

```
قیمت نهایی = (ضریب فنی × کای فنی) + (ضریب حرفه‌ای × کای حرفه‌ای)

با پشتیبانی از:
- هشتگ‌دار/غیر هشتگ‌دار
- Override دپارتمان (SharedService)
- سال مالی
- Freeze شدن سال مالی
```

---

#### **6. `IFactorSettingService`** - مدیریت کای‌ها

```csharp
public interface IFactorSettingService
{
    // CRUD Operations
    Task<IEnumerable<FactorSetting>> GetAllFactorsAsync();
    Task<FactorSetting> GetFactorByIdAsync(int id);
    Task<FactorSetting> CreateFactorAsync(FactorSetting factor);
    Task<FactorSetting> UpdateFactorAsync(FactorSetting factor);
    Task DeleteFactorAsync(int id);
    
    // Query Operations
    Task<IEnumerable<FactorSetting>> GetFactorsByTypeAsync(ServiceComponentType factorType, int financialYear);
    Task<FactorSetting> GetActiveFactorByTypeAsync(ServiceComponentType factorType, int financialYear, bool isHashtagged = false);
    Task<FactorSetting> GetActiveFactorByTypeAndHashtaggedAsync(ServiceComponentType factorType, bool isHashtagged, int financialYear);
    Task<IEnumerable<FactorSetting>> GetCurrentYearFactorsAsync();
    Task<IEnumerable<FactorSetting>> GetFactorsByFinancialYearAsync(int financialYear);
    
    // Filtered Operations
    Task<IEnumerable<FactorSetting>> GetFilteredFactorsAsync(string searchTerm, ServiceComponentType? factorType, 
        int? financialYear, bool? isActive, int page, int pageSize);
    Task<int> GetFactorsCountAsync(string searchTerm, ServiceComponentType? factorType, int? financialYear, bool? isActive);
    Task<int> GetActiveFactorsCountForYearAsync(int financialYear);
    
    // Business Logic
    Task<bool> ExistsFactorAsync(ServiceComponentType factorType, int financialYear, bool isHashtagged = false);
    Task<bool> IsFactorUsedInCalculationsAsync(int factorId);
    
    // Freeze Operations
    Task<bool> FreezeFinancialYearFactorsAsync(int financialYear, string userId);
    Task<IEnumerable<FactorSetting>> GetFrozenFactorsAsync(int? financialYear = null);
}
```

**🎯 ویژگی‌های کلیدی:**
- مدیریت کای‌های فنی/حرفه‌ای
- پشتیبانی از سال مالی
- Freeze کردن کای‌ها (قفل محاسبات)
- تفکیک هشتگ‌دار/غیر هشتگ‌دار

---

#### **7. `INotificationService`** - سیستم اطلاع‌رسانی

```csharp
public interface INotificationService
{
    // Send Operations
    Task<NotificationResult> SendAsync(NotificationRequest request);
    Task<NotificationBatchResult> SendBatchAsync(IEnumerable<NotificationRequest> requests);
    
    // Template Operations
    Task<NotificationResult> SendTemplateAsync(string templateKey, object recipient, params object[] parameters);
    Task<NotificationBatchResult> SendBatchTemplateAsync(string templateKey, IEnumerable<object> recipients, params object[] parameters);
    
    // Schedule Operations
    Task<NotificationScheduleResult> ScheduleAsync(NotificationRequest request, DateTime scheduledTime);
    Task<bool> CancelScheduledAsync(Guid notificationId);
    
    // Status Operations
    Task<NotificationStatus> GetStatusAsync(Guid notificationId);
}
```

**🎯 کانال‌های پشتیبانی شده:**
- SMS
- Email
- Push Notification
- In-App Notification

---

#### **8. `ISecurityTokenService` & `IExternalInquiryService`** - امنیت و استعلام

```csharp
// Security Token
public interface ISecurityTokenService
{
    Task<ServiceResult<object>> CheckTokenPresenceAsync(string tokenId = "TR127256");
    Task<ServiceResult<object>> TestTokenConnectionAsync(string tokenId = "TR127256");
}

// External Inquiry (شبکه شمس)
public interface IExternalInquiryService
{
    Task<ServiceResult<PatientIdentityData>> InquiryIdentityAsync(string nationalCode, DateTime birthDate, string tokenId);
    Task<ServiceResult<PatientInsuranceData>> InquiryInsuranceAsync(string nationalCode, DateTime birthDate, string tokenId);
    Task<ServiceResult<PatientInquiryViewModel>> InquiryCompleteAsync(string nationalCode, DateTime birthDate, InquiryType inquiryType, string tokenId);
    Task<ServiceResult<bool>> CheckTokenStatusAsync(string tokenId);
    Task<ServiceResult<ExternalServiceStatus>> CheckServiceAvailabilityAsync();
    Task<ServiceResult<bool>> ValidateNationalCodeAsync(string nationalCode);
}
```

---

#### **9. `PagedResult<T>` & `IAppSettings`** - Utility

```csharp
// Paged Result
public class PagedResult<T> : IEnumerable<T>
{
    public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
    public SecurityLevel SecurityLevel { get; set; }
    public bool ContainsSensitiveData { get; set; }
}

// App Settings
public interface IAppSettings
{
    // Basic Settings
    int DefaultPageSize { get; }
    int MaxLoginAttempts { get; }
    int SessionTimeoutMinutes { get; }
    
    // Security Settings
    bool RequireTwoFactorAuthentication { get; }
    int PasswordComplexityLevel { get; }
    bool EnableBruteForceProtection { get; }
    
    // Medical System Settings
    int MaxAppointmentDurationMinutes { get; }
    int DefaultAppointmentDurationMinutes { get; }
    bool EnableInsuranceValidation { get; }
    
    // Notification Settings
    string SmsProvider { get; }
    bool EnableEmailNotifications { get; }
    int AppointmentReminderHours { get; }
}
```

---

## 2️⃣ **ClinicAdmin Interfaces (27 فایل)**

### 📌 **دسته‌بندی:**

| **دسته** | **Interface ها** | **توضیحات** |
|---------|-----------------|-------------|
| **Clinic Management** | `IClinicManagementService`, `IClinicRepository` | مدیریت کلینیک‌ها |
| **Department Management** | `IDepartmentManagementService`, `IDepartmentRepository` | مدیریت دپارتمان‌ها |
| **Doctor Management** | `IDoctorCrudService`, `IDoctorCrudRepository`, `IDoctorRepository` | عملیات CRUD پزشکان |
| **Doctor Assignment** | `IDoctorAssignmentService`, `IDoctorAssignmentRepository`, `IDoctorAssignmentHistoryService` | تخصیص پزشکان به دپارتمان‌ها |
| **Doctor Schedule** | `IDoctorScheduleService`, `IDoctorScheduleRepository` | مدیریت برنامه کاری پزشکان |
| **Doctor Relations** | `IDoctorDepartmentService`, `IDoctorDepartmentRepository`, `IDoctorServiceCategoryService`, `IDoctorServiceCategoryRepository` | روابط پزشک با دپارتمان و سرفصل‌ها |
| **Appointment Management** | `IAppointmentAvailabilityService`, `IScheduleOptimizationService`, `IEmergencyBookingService` | مدیریت نوبت‌دهی و بهینه‌سازی |
| **Dashboard & Reporting** | `IDoctorDashboardService`, `IDoctorDashboardRepository`, `IDoctorReportingService`, `IDoctorReportingRepository` | داشبورد و گزارشات پزشکان |
| **Shared Services** | `ISharedServiceManagementService` | مدیریت خدمات مشترک بین دپارتمان‌ها |
| **Service Management** | `IServiceManagementService`, `IServiceCategoryRepository` | مدیریت خدمات و دسته‌بندی‌ها |
| **Specialization** | `ISpecializationService`, `ISpecializationRepository` | مدیریت تخصص‌های پزشکی |

---

### 🔷 **Interface های کلیدی:**

#### **1. `IDoctorCrudService`** - مدیریت پزشکان

```csharp
public interface IDoctorCrudService
{
    // Core CRUD Operations
    Task<ServiceResult<PagedResult<DoctorIndexViewModel>>> GetDoctorsAsync(DoctorSearchViewModel filter);
    Task<ServiceResult<DoctorDetailsViewModel>> GetDoctorDetailsAsync(int doctorId);
    Task<ServiceResult<DoctorCreateEditViewModel>> GetDoctorForEditAsync(int doctorId);
    Task<ServiceResult<Doctor>> CreateDoctorAsync(DoctorCreateEditViewModel model);
    Task<ServiceResult<Doctor>> UpdateDoctorAsync(DoctorCreateEditViewModel model);
    Task<ServiceResult> SoftDeleteDoctorAsync(int doctorId);
    Task<ServiceResult> RestoreDoctorAsync(int doctorId);
    
    // Query Operations
    Task<ServiceResult<List<Specialization>>> GetActiveSpecializationsAsync();
    Task<ServiceResult<Doctor>> GetDoctorByNationalCodeAsync(string nationalCode);
    Task<ServiceResult<Doctor>> GetDoctorByMedicalCouncilCodeAsync(string medicalCouncilCode);
    
    // Status Operations
    Task<ServiceResult> ActivateDoctorAsync(int doctorId);
    Task<ServiceResult> DeactivateDoctorAsync(int doctorId);
}
```

---

#### **2. `IDoctorScheduleService`** - برنامه کاری پزشکان

```csharp
public interface IDoctorScheduleService
{
    // Scheduling & Availability
    Task<ServiceResult> SetDoctorScheduleAsync(int doctorId, DoctorScheduleViewModel schedule);
    Task<ServiceResult<DoctorScheduleViewModel>> GetDoctorScheduleAsync(int doctorId);
    Task<ServiceResult> BlockTimeRangeForDoctorAsync(int doctorId, DateTime start, DateTime end, string reason);
    Task<ServiceResult<List<TimeSlotViewModel>>> GetAvailableAppointmentSlotsAsync(int doctorId, DateTime date);
    
    // List and Search Operations
    Task<ServiceResult<PagedResult<DoctorScheduleViewModel>>> GetAllDoctorSchedulesAsync(string searchTerm, int pageNumber, int pageSize);
    
    // Schedule Management Operations
    Task<ServiceResult<DoctorScheduleViewModel>> GetDoctorScheduleByIdAsync(int scheduleId);
    Task<ServiceResult> DeleteDoctorScheduleAsync(int scheduleId);
    Task<ServiceResult> DeactivateDoctorScheduleAsync(int scheduleId);
    Task<ServiceResult> ActivateDoctorScheduleAsync(int scheduleId);
}
```

---

#### **3. `IAppointmentAvailabilityService`** - دسترسی‌پذیری نوبت‌ها

```csharp
public interface IAppointmentAvailabilityService
{
    // Availability Operations
    Task<ServiceResult<List<DateTime>>> GetAvailableDatesAsync(int doctorId, DateTime startDate, DateTime endDate);
    Task<ServiceResult<List<TimeSlotViewModel>>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date);
    Task<ServiceResult<bool>> IsSlotAvailableAsync(int slotId);
    
    // Slot Reservation
    Task<ServiceResult<bool>> ReserveSlotAsync(int slotId, int patientId, TimeSpan reservationDuration);
    Task<ServiceResult<bool>> ReleaseSlotAsync(int slotId);
    
    // Slot Generation
    Task<ServiceResult<bool>> GenerateWeeklySlotsAsync(int doctorId, DateTime weekStart);
    Task<ServiceResult<bool>> GenerateMonthlySlotsAsync(int doctorId, DateTime monthStart);
}
```

---

#### **4. `IScheduleOptimizationService`** - بهینه‌سازی برنامه کاری

```csharp
public interface IScheduleOptimizationService
{
    // Schedule Optimization
    Task<ServiceResult<WorkloadBalanceResult>> OptimizeDailyScheduleAsync(int doctorId, DateTime date);
    Task<ServiceResult<List<WorkloadBalanceResult>>> OptimizeWeeklyScheduleAsync(int doctorId, DateTime weekStart);
    Task<ServiceResult<Dictionary<string, List<WorkloadBalanceResult>>>> OptimizeMonthlyScheduleAsync(int doctorId, DateTime monthStart);
    
    // Workload Balance
    Task<ServiceResult<bool>> BalanceWorkloadAsync(int doctorId, DateTime startDate, DateTime endDate);
    Task<ServiceResult<List<BreakTimeSlot>>> OptimizeBreakTimesAsync(int doctorId, DateTime date);
    Task<ServiceResult<bool>> OptimizeAppointmentPrioritiesAsync(int doctorId, DateTime date);
    
    // Patient Distribution
    Task<ServiceResult<PatientDistributionResult>> OptimizePatientDistributionAsync(int doctorId, DateTime date);
    Task<ServiceResult<List<EmergencyTimeSlot>>> OptimizeEmergencyTimesAsync(int doctorId, DateTime date);
    
    // Work-Life Balance
    Task<ServiceResult<WorkLifeBalanceReport>> OptimizeWorkLifeBalanceAsync(int doctorId, DateTime startDate, DateTime endDate);
    Task<ServiceResult<CostOptimizationReport>> OptimizeCostsAsync(int doctorId, DateTime startDate, DateTime endDate);
}
```

---

#### **5. `IEmergencyBookingService`** - مدیریت رزروهای اورژانس

```csharp
public interface IEmergencyBookingService
{
    // Emergency Booking Operations
    Task<ServiceResult<bool>> CanBookEmergencyAsync(int doctorId, DateTime date, TimeSpan time, EmergencyPriority priority);
    Task<ServiceResult<EmergencyBookingResult>> BookEmergencyAsync(EmergencyBookingRequest request);
    Task<ServiceResult<bool>> CancelEmergencyAsync(int emergencyBookingId, string cancellationReason);
    
    // Query Operations
    Task<ServiceResult<List<EmergencyBooking>>> GetEmergencyBookingsAsync(int doctorId, DateTime? date, EmergencyPriority? priority);
    Task<ServiceResult<EmergencyBookingStatistics>> GetEmergencyStatisticsAsync(int doctorId, DateTime startDate, DateTime endDate);
    
    // Priority Management
    Task<ServiceResult<List<EmergencyPriority>>> GetEmergencyPrioritiesAsync();
    Task<ServiceResult<bool>> SetEmergencyPriorityAsync(int emergencyBookingId, EmergencyPriority priority);
    
    // Conflict Resolution
    Task<ServiceResult<List<EmergencyConflict>>> CheckEmergencyConflictsAsync(int doctorId, DateTime date, TimeSpan time);
    Task<ServiceResult<bool>> ResolveEmergencyConflictsAsync(int doctorId, DateTime date, List<EmergencyConflict> conflicts);
    
    // Notification & Reporting
    Task<ServiceResult<bool>> SendEmergencyNotificationAsync(int emergencyBookingId, NotificationChannelType channel);
    Task<ServiceResult<EmergencyReport>> GetEmergencyReportAsync(int doctorId, DateTime startDate, DateTime endDate);
}
```

---

#### **6. `ISharedServiceManagementService`** - خدمات مشترک بین دپارتمان‌ها

```csharp
public interface ISharedServiceManagementService
{
    // CRUD Operations
    Task<ServiceResult<PagedResult<SharedServiceIndexViewModel>>> GetSharedServicesAsync(
        int? departmentId, int? serviceId, bool? isActive, string searchTerm, int pageNumber, int pageSize);
    Task<ServiceResult<SharedServiceDetailsViewModel>> GetSharedServiceDetailsAsync(int sharedServiceId);
    Task<ServiceResult<SharedServiceCreateEditViewModel>> GetSharedServiceForEditAsync(int sharedServiceId);
    Task<ServiceResult> CreateSharedServiceAsync(SharedServiceCreateEditViewModel model);
    Task<ServiceResult> UpdateSharedServiceAsync(SharedServiceCreateEditViewModel model);
    Task<ServiceResult> SoftDeleteSharedServiceAsync(int sharedServiceId);
    Task<ServiceResult> RestoreSharedServiceAsync(int sharedServiceId);
    
    // Business Operations
    Task<ServiceResult> AddServiceToDepartmentAsync(int serviceId, int departmentId, string notes = null);
    Task<ServiceResult> RemoveServiceFromDepartmentAsync(int serviceId, int departmentId);
    Task<ServiceResult> ToggleServiceInDepartmentAsync(int serviceId, int departmentId, bool isActive);
    Task<ServiceResult> CopyServiceToDepartmentsAsync(int serviceId, List<int> departmentIds);
    
    // Query Operations
    Task<ServiceResult<List<SharedServiceIndexViewModel>>> GetDepartmentSharedServicesAsync(int departmentId);
    Task<ServiceResult<List<DepartmentLookupViewModel>>> GetServiceSharedDepartmentsAsync(int serviceId);
    Task<bool> IsServiceInDepartmentAsync(int serviceId, int departmentId);
    
    // Statistics
    Task<ServiceResult<SharedServiceStatisticsViewModel>> GetSharedServiceStatisticsAsync();
    Task<ServiceResult<List<SharedServiceUsageReportViewModel>>> GetSharedServiceUsageReportAsync(
        int? departmentId, DateTime? startDate, DateTime? endDate);
}
```

**🎯 ویژگی کلیدی:**
- امکان Override کردن `TechnicalFactor` و `ProfessionalFactor` برای هر دپارتمان
- مدیریت خدمات مشترک بدون نیاز به ایجاد خدمات جداگانه

---

## 3️⃣ **Insurance Interfaces (20 فایل)**

### 📌 **دسته‌بندی:**

| **دسته** | **Interface ها** | **توضیحات** |
|---------|-----------------|-------------|
| **Insurance Provider & Plan** | `IInsuranceProviderService`, `IInsuranceProviderRepository`, `IInsurancePlanService`, `IInsurancePlanRepository` | مدیریت ارائه‌دهندگان و طرح‌های بیمه |
| **Insurance Tariff** | `IInsuranceTariffService`, `IInsuranceTariffRepository` | مدیریت تعرفه‌های بیمه |
| **Patient Insurance** | `IPatientInsuranceService`, `IPatientInsuranceRepository` | مدیریت بیمه‌های بیماران |
| **Insurance Calculation** | `IInsuranceCalculationService`, `IInsuranceCalculationRepository`, `IAdvancedInsuranceCalculationService` | محاسبات بیمه اصلی |
| **Supplementary Insurance** | `ISupplementaryInsuranceService`, `ISupplementaryInsuranceCalculationService`, `ISupplementaryCombinationService`, `ISupplementaryInsuranceMonitoringService`, `ISupplementaryInsuranceCacheService` | سیستم بیمه تکمیلی |
| **Combined Insurance** | `ICombinedInsuranceCalculationService` | محاسبات ترکیبی بیمه اصلی + تکمیلی |
| **Business Rules** | `IBusinessRuleEngine`, `IBusinessRuleRepository` | موتور قوانین کسب‌وکار بیمه |
| **Validation** | `IInsuranceValidationService`, `ITariffDomainValidationService` | اعتبارسنجی بیمه و تعرفه |
| **Supporting Services** | `IPlanServiceRepository` | سرویس‌های پشتیبانی |

---

### 🔷 **Interface های کلیدی:**

#### **1. `IInsuranceCalculationService`** - محاسبات بیمه اصلی

```csharp
public interface IInsuranceCalculationService
{
    // Calculation Operations
    Task<ServiceResult<InsuranceCalculationResultViewModel>> CalculatePatientShareAsync(int patientId, int serviceId, DateTime calculationDate);
    Task<ServiceResult<InsuranceCalculationResultViewModel>> CalculateReceptionCostsAsync(int patientId, List<int> serviceIds, DateTime receptionDate);
    Task<ServiceResult<InsuranceCalculationResultViewModel>> CalculateAppointmentCostAsync(int patientId, int serviceId, DateTime appointmentDate);
    Task<ServiceResult<InsuranceCalculationResultViewModel>> GetInsuranceCalculationResultAsync(int patientId, int serviceId, DateTime calculationDate);
    
    // Validation Operations
    Task<ServiceResult<bool>> IsServiceCoveredAsync(int patientId, int serviceId, DateTime calculationDate);
    Task<ServiceResult<bool>> IsPatientInsuranceValidAsync(int patientId, DateTime calculationDate);
    
    // Business Logic Operations
    Task<ServiceResult<decimal>> CalculateFranchiseAsync(int patientId, int serviceId, DateTime calculationDate);
    Task<ServiceResult<decimal>> CalculateCopayAsync(int patientId, int serviceId, DateTime calculationDate);
    Task<ServiceResult<decimal>> CalculateCoveragePercentageAsync(int patientId, int serviceId, DateTime calculationDate);
    
    // Management Operations
    Task<ServiceResult<InsuranceCalculation>> SaveCalculationAsync(InsuranceCalculation calculation);
    Task<ServiceResult<List<InsuranceCalculation>>> GetPatientCalculationsAsync(int patientId);
    Task<ServiceResult<List<InsuranceCalculation>>> GetReceptionCalculationsAsync(int receptionId);
    Task<ServiceResult<List<InsuranceCalculation>>> GetAppointmentCalculationsAsync(int appointmentId);
    Task<ServiceResult<object>> GetCalculationStatisticsAsync();
}
```

**🎯 فرمول محاسبه بیمه:**

```
سهم بیمار = (قیمت خدمت - سهم بیمه) + Franchise + Copay

جایی که:
- سهم بیمه = قیمت خدمت × درصد پوشش
- Franchise = حداقل پرداخت بیمار
- Copay = مبلغ ثابت پرداخت بیمار
```

---

#### **2. `ISupplementaryInsuranceService`** - بیمه تکمیلی

```csharp
public interface ISupplementaryInsuranceService
{
    // Supplementary Insurance Calculation
    Task<ServiceResult<SupplementaryCalculationResult>> CalculateSupplementaryInsuranceAsync(
        int patientId, int serviceId, decimal serviceAmount, decimal primaryCoverage, DateTime calculationDate);
    
    // Supplementary Settings Management
    Task<ServiceResult<SupplementarySettings>> GetSupplementarySettingsAsync(int planId);
    Task<ServiceResult> UpdateSupplementarySettingsAsync(int planId, SupplementarySettings settings);
    
    // Supplementary Tariff Management
    Task<ServiceResult<List<SupplementaryTariffViewModel>>> GetSupplementaryTariffsAsync(int planId);
    
    // Advanced Supplementary Insurance Calculation
    Task<ServiceResult<SupplementaryCalculationResult>> CalculateAdvancedSupplementaryInsuranceAsync(
        int patientId, int serviceId, decimal serviceAmount, decimal primaryCoverage, DateTime calculationDate,
        Dictionary<string, object> advancedSettings = null);
    
    // Comparison Operations
    Task<ServiceResult<List<SupplementaryCalculationResult>>> CompareSupplementaryInsuranceOptionsAsync(
        int patientId, int serviceId, decimal serviceAmount, decimal primaryCoverage, DateTime calculationDate,
        List<int> supplementaryPlanIds = null);
}
```

---

#### **3. `IPatientInsuranceService`** - مدیریت بیمه‌های بیماران

```csharp
public interface IPatientInsuranceService
{
    // CRUD Operations
    Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPatientInsurancesAsync(
        int? patientId, string searchTerm, int pageNumber, int pageSize);
    Task<ServiceResult<PatientInsuranceDetailsViewModel>> GetPatientInsuranceDetailsAsync(int patientInsuranceId);
    Task<ServiceResult<PatientInsuranceCreateEditViewModel>> GetPatientInsuranceForEditAsync(int patientInsuranceId);
    Task<ServiceResult<int>> CreatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model);
    Task<ServiceResult> UpdatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model);
    Task<ServiceResult> SoftDeletePatientInsuranceAsync(int patientInsuranceId);
    
    // Lookup Operations
    Task<ServiceResult<List<PatientInsuranceLookupViewModel>>> GetActivePatientInsurancesForLookupAsync(int patientId);
    
    // Validation Operations
    Task<ServiceResult<bool>> DoesPolicyNumberExistAsync(string policyNumber, int? excludeId = null);
    Task<ServiceResult<bool>> DoesPrimaryInsuranceExistAsync(int patientId, int? excludeId = null);
    Task<ServiceResult<bool>> DoesDateOverlapExistAsync(int patientId, DateTime startDate, DateTime endDate, int? excludeId = null);
    Task<ServiceResult<Dictionary<string, string>>> ValidatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model);
    
    // Business Logic Operations
    Task<ServiceResult<List<PatientInsuranceIndexViewModel>>> GetPatientInsurancesByPatientAsync(int patientId);
    Task<ServiceResult<List<PatientInsuranceIndexViewModel>>> GetSupplementaryInsurancesByPatientAsync(int patientId);
    Task<ServiceResult<PatientInsuranceDetailsViewModel>> GetPrimaryInsuranceByPatientAsync(int patientId);
    Task<ServiceResult> SetPrimaryInsuranceAsync(int patientInsuranceId);
    Task<ServiceResult<bool>> IsPatientInsuranceValidAsync(int patientInsuranceId, DateTime checkDate);
    
    // Combined Insurance Calculation Methods
    Task<ServiceResult<CombinedInsuranceCalculationResult>> CalculateCombinedInsuranceForPatientAsync(
        int patientId, int serviceId, decimal serviceAmount, DateTime? calculationDate = null);
    Task<ServiceResult<List<PatientInsuranceLookupViewModel>>> GetActiveAndSupplementaryByPatientIdAsync(int patientId);
}
```

---

#### **4. `IInsuranceTariffService`** - مدیریت تعرفه‌های بیمه

```csharp
public interface IInsuranceTariffService
{
    // CRUD Operations
    Task<ServiceResult<PagedResult<InsuranceTariffIndexViewModel>>> GetTariffsAsync(
        int? planId, int? serviceId, int? providerId, string searchTerm, int pageNumber, int pageSize);
    Task<ServiceResult<InsuranceTariffDetailsViewModel>> GetTariffDetailsAsync(int id);
    Task<ServiceResult<InsuranceTariffCreateEditViewModel>> GetTariffForEditAsync(int id);
    Task<ServiceResult<int>> CreateTariffAsync(InsuranceTariffCreateEditViewModel model);
    Task<ServiceResult> UpdateTariffAsync(InsuranceTariffCreateEditViewModel model);
    Task<ServiceResult> SoftDeleteTariffAsync(int id);
    
    // Business Logic Operations
    Task<ServiceResult<InsuranceTariff>> GetTariffByPlanAndServiceAsync(int planId, int serviceId);
    Task<ServiceResult<List<InsuranceTariff>>> GetTariffsByPlanIdAsync(int planId);
    Task<ServiceResult<List<InsuranceTariff>>> GetTariffsByServiceIdAsync(int serviceId);
    
    // Validation Operations
    Task<ServiceResult<Dictionary<string, string>>> ValidateTariffAsync(InsuranceTariffCreateEditViewModel model);
    Task<ServiceResult<bool>> DoesTariffExistAsync(int planId, int serviceId, int? excludeId = null);
    
    // Bulk Operations
    Task<ServiceResult> BulkToggleStatusAsync(List<int> tariffIds, bool isActive);
    Task<ServiceResult<int>> CreateBulkTariffForAllServicesAsync(InsuranceTariffCreateEditViewModel model);
    
    // Supplementary Insurance Methods
    Task<ServiceResult<List<InsuranceTariff>>> GetAllSupplementaryTariffsAsync();
    Task<ServiceResult<decimal>> CalculateSupplementaryTariffAsync(int serviceId, int planId, decimal baseAmount, DateTime? calculationDate);
    Task<ServiceResult<Dictionary<string, object>>> GetSupplementarySettingsAsync(int planId);
    Task<ServiceResult> UpdateSupplementarySettingsAsync(int planId, Dictionary<string, object> settings);
    
    // Statistics Operations
    Task<ServiceResult<InsuranceTariffStatisticsViewModel>> GetStatisticsAsync();
}
```

---

#### **5. `ICombinedInsuranceCalculationService`** - محاسبات ترکیبی

```csharp
public interface ICombinedInsuranceCalculationService
{
    // Combined Calculation
    Task<ServiceResult<CombinedInsuranceCalculationResult>> CalculateCombinedInsuranceAsync(
        int patientId, int serviceId, decimal serviceAmount, DateTime calculationDate);
    
    // Breakdown Calculation
    Task<ServiceResult<InsuranceBreakdown>> GetInsuranceBreakdownAsync(
        int patientId, int serviceId, decimal serviceAmount, DateTime calculationDate);
}
```

**🎯 فرمول ترکیبی:**

```
Step 1: محاسبه بیمه اصلی
    سهم بیمه اصلی = قیمت × درصد پوشش اصلی
    سهم بیمار پس از بیمه اصلی = قیمت - سهم بیمه اصلی

Step 2: محاسبه بیمه تکمیلی
    سهم بیمه تکمیلی = سهم بیمار × درصد پوشش تکمیلی
    سهم نهایی بیمار = سهم بیمار - سهم بیمه تکمیلی

نتیجه نهایی:
    سهم بیمه اصلی
    سهم بیمه تکمیلی
    سهم نهایی بیمار
    کل پرداختی
```

---

## 4️⃣ **Payment Interfaces (11 فایل)**

### 📌 **دسته‌بندی:**

| **دسته** | **Interface ها** | **توضیحات** |
|---------|-----------------|-------------|
| **Payment Service** | `IPaymentService` | سرویس اصلی مدیریت پرداخت |
| **Payment Gateway** | `IPaymentGatewayService`, `IPaymentGatewayRepository` | درگاه‌های پرداخت آنلاین |
| **POS Terminal** | `IPosManagementService`, `IPosTerminalRepository` | مدیریت ترمینال‌های POS |
| **Cash Session** | `ICashSessionRepository` | مدیریت نقدینه و جلسات کارتخوان |
| **Online Payment** | `IOnlinePaymentRepository` | مدیریت پرداخت‌های آنلاین |
| **Payment Transaction** | `IPaymentTransactionRepository` | تراکنش‌های پرداخت |
| **Payment Validation** | `IPaymentValidationService` | اعتبارسنجی پرداخت‌ها |
| **Payment Reporting** | `IPaymentReportingService` | گزارشات پرداخت |
| **Web Payment** | `IWebPaymentService` | سرویس پرداخت وب |
| **Repository** | `IPaymentRepository` | Repository عمومی پرداخت |

---

## 5️⃣ **OTP & Repositories (5 فایل)**

### 📌 **دسته‌بندی:**

| **دسته** | **Interface ها** | **توضیحات** |
|---------|-----------------|-------------|
| **OTP System** | `OTPSystem.cs` | سیستم OTP و احراز هویت |
| **General Repositories** | `IDoctorRepository`, `IInsuranceRepository`, `IPatientRepository`, `IPaymentRepository` | Repository های عمومی |

---

## 🎨 **الگوهای طراحی استفاده شده**

### 1️⃣ **Repository Pattern**

```
Controller → Service → Repository → Database

ویژگی‌ها:
✅ Separation of Concerns
✅ Testability
✅ Maintainability
✅ دسترسی متمرکز به داده
```

### 2️⃣ **Service Layer Pattern**

```
Service ها مسئول:
✅ منطق کسب‌وکار
✅ اعتبارسنجی پیچیده
✅ تراکنش‌های چندگانه
✅ هماهنگی بین Entity ها
```

### 3️⃣ **ServiceResult Pattern**

```csharp
public class ServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Code { get; set; }
    public SecurityLevel SecurityLevel { get; set; }
    public ErrorCategory Category { get; set; }
    public List<ValidationError> ValidationErrors { get; set; }
}

public class ServiceResult<T> : ServiceResult
{
    public T Data { get; set; }
}
```

### 4️⃣ **Factory Method Pattern** (ViewModels)

```csharp
public static DoctorIndexViewModel FromEntity(Doctor doctor)
{
    return new DoctorIndexViewModel
    {
        DoctorId = doctor.DoctorId,
        FullName = $"{doctor.FirstName} {doctor.LastName}",
        // ...
    };
}
```

---

## 📝 **نکات مهم برای استفاده**

### ✅ **DO:**
1. همیشه از `ServiceResult` برای برگشت نتایج استفاده کن
2. از Interface ها در Constructor Injection استفاده کن
3. از `async/await` برای تمام عملیات I/O استفاده کن
4. همه خطاها رو log کن
5. از Soft Delete برای موجودیت‌های پزشکی استفاده کن

### ❌ **DON'T:**
1. منطق کسب‌وکار رو در Controller ننویس
2. مستقیم به DbContext در Controller دسترسی نداشته باش
3. از AutoMapper در این پروژه استفاده نکن (Factory Method الزامیه)
4. پسورد برای کاربران ایجاد نکن (سیستم OTP)
5. Entity ها رو physical delete نکن

---

## 🔗 **روابط بین Interface ها**

```
IReceptionService
    ├─→ IPatientService
    ├─→ IDoctorCrudService
    ├─→ IServiceService
    ├─→ IInsuranceCalculationService
    ├─→ IPaymentService
    └─→ IExternalInquiryService

IInsuranceCalculationService
    ├─→ IPatientInsuranceService
    ├─→ IInsuranceTariffService
    └─→ ISupplementaryInsuranceService

IServiceCalculationService
    ├─→ IFactorSettingService
    └─→ ISharedServiceManagementService
```

---

## 📊 **آمار کلی Interfaces**

| **گروه** | **تعداد Interface** | **استفاده** |
|---------|-------------------|-------------|
| **Core** | 21 | عملیات اصلی سیستم |
| **ClinicAdmin** | 27 | مدیریت پزشکان و دپارتمان‌ها |
| **Insurance** | 20 | سیستم بیمه |
| **Payment** | 11 | سیستم پرداخت |
| **OTP & Repos** | 5 | احراز هویت و Repository های عمومی |
| **Medical** | 0 | (فولدر خالی - آینده) |
| **جمع کل** | **86** | |

---

## 🚀 **مسیر توسعه آینده**

1. ✅ **Medical History Module** - افزودن Interface های Medical folder
2. ✅ **Prescription Management** - مدیریت نسخه‌های پزشکی
3. ✅ **Lab Integration** - یکپارچه‌سازی با آزمایشگاه
4. ✅ **Imaging Integration** - یکپارچه‌سازی با تصویربرداری
5. ✅ **Telemedicine** - پزشکی از راه دور

---

## 📚 **مراجع مرتبط**

- [APP_PRINCIPLES_CONTRACT.md](./APP_PRINCIPLES_CONTRACT.md)
- [ServiceResult_Enhanced_Contract.md](./ServiceResult_Enhanced_Contract.md)
- [DATABASE_COMPREHENSIVE_SCHEMA.md](./DATABASE_COMPREHENSIVE_SCHEMA.md)
- [TechnicalDocumentation.md](../Documentation/TechnicalDocumentation.md)

---

**✨ پایان مستند جامع Interfaces ✨**


