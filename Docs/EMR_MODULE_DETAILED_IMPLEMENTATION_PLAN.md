# 📋 برنامه پیاده‌سازی دقیق ماژول EMR - طبق فرآیند 12 مرحله‌ای

**تاریخ:** 1404/10/05  
**وضعیت:** 🔧 **Implementation Planning Phase**  
**هدف:** پیاده‌سازی ماژول EMR با رعایت SRP، Component-Based، AJAX-First، و معماری پروژه

---

## ✅ STEP 0 — Preflight

### 📌 Contracts Acknowledged
- ✅ `AI_ASSISTANT_MASTER_CONTRACT.md` - بررسی شد
- ✅ `DEVELOPMENT_CONTRACT.md` - بررسی شد
- ✅ `CLINICAPP_PATIENT_DASHBOARD_BEAST_ROADMAP_PROMPT.md` - بررسی شد
- ✅ Strongly-Typed Development (ViewModels, نه ViewBag)
- ✅ ServiceResult Enhanced برای تمام عملیات
- ✅ Factory Method برای Entity → ViewModel (MUST)
- ✅ Repository Pattern برای Data Access
- ✅ Service Layer Pattern برای Business Logic
- ✅ SRP (Single Responsibility Principle)
- ✅ Component-Based Architecture
- ✅ AJAX-First (بدون رفرش صفحه)

### 📌 Scope Confirmed
**Module Name:** Electronic Medical Record (EMR) / پرونده الکترونیک سلامت  
**Scope:**
- `Areas/Patient/Controllers/MedicalRecordController.cs` (NEW)
- `Areas/Patient/Controllers/Api/MedicalRecordApiController.cs` (NEW)
- `Services/Patient/MedicalRecordService.cs` (NEW)
- `Services/Patient/MedicalHistoryService.cs` (NEW)
- `Repositories/Patient/MedicalRecordRepository.cs` (NEW)
- `Repositories/Patient/MedicalHistoryRepository.cs` (NEW)
- `ViewModels/Patient/MedicalRecord/*` (NEW)
- `Factories/Patient/MedicalRecordFactory.cs` (NEW)
- `Views/Patient/MedicalRecord/*` (NEW)
- `Content/js/medical-record.js` (NEW)

**Expected Behavior:**
1. بیمار لاگین می‌کند
2. روی "پرونده الکترونیک" در منوی پروفایل کلیک می‌کند
3. صفحه EMR با AJAX Navigation لود می‌شود (بدون رفرش)
4. اطلاعات پزشکی در بخش‌های Component-Based نمایش داده می‌شود:
   - تاریخچه پزشکی (Medical History)
   - نوبت‌ها (Appointments)
   - پذیرش‌ها (Receptions)
   - علائم حیاتی (Vital Signs)
5. هر بخش به صورت AJAX و مستقل لود می‌شود
6. بیمار می‌تواند تاریخچه پزشکی خود را اضافه/ویرایش کند

**Current Issue:**
- ❌ ماژول EMR وجود ندارد
- ❌ لینک در `_LoginPartial.cshtml` به Controller وجود ندارد
- ❌ هیچ Service، Repository، ViewModel، یا View وجود ندارد

### 📌 Risk Level: **CRITICAL**
- **Security Risk:** HIGH - اطلاعات پزشکی حساس
- **Data Integrity Risk:** HIGH - اطلاعات پزشکی باید دقیق باشد
- **Compliance Risk:** HIGH - استانداردهای پزشکی ایران
- **Architecture Risk:** MEDIUM - باید با معماری موجود سازگار باشد

---

## 📊 STEP 1 — Module & Boundary Map

### 🎯 Controllers (MVC)
**Existing Pattern:**
```
Areas/Patient/Controllers/
├── DashboardController.cs (MVC Controller)
├── AppointmentController.cs (MVC Controller)
└── Api/
    ├── PatientDashboardApiController.cs (Web API)
    └── PatientAppointmentApiController.cs (Web API)
```

**New Required:**
```
Areas/Patient/Controllers/
├── MedicalRecordController.cs (MVC Controller - AJAX-Compatible)
└── Api/
    └── MedicalRecordApiController.cs (Web API - Component Data)
```

### 🎯 Services
**Existing Pattern:**
```
Services/
├── PatientService.cs (IPatientService)
├── PatientDashboardService.cs (IPatientDashboardService)
└── Appointment/
    └── AppointmentBookingService.cs
```

**New Required:**
```
Services/Patient/
├── MedicalRecordService.cs (IPatientMedicalRecordService)
└── MedicalHistoryService.cs (IPatientMedicalHistoryService)
```

### 🎯 Repositories
**Existing Pattern:**
```
Repositories/Patient/
└── PatientRepository.cs (IPatientRepository)
```

**New Required:**
```
Repositories/Patient/
├── MedicalRecordRepository.cs (IMedicalRecordRepository)
└── MedicalHistoryRepository.cs (IMedicalHistoryRepository)
```

### 🎯 Factories (CRITICAL - MUST HAVE)
**Existing Pattern:**
- ❌ هیچ Factory Pattern موجود نیست!
- ✅ اما `PatientService` از Convert Methods استفاده می‌کند

**New Required:**
```
Factories/Patient/
├── MedicalRecordFactory.cs (Entity → ViewModel)
└── MedicalHistoryFactory.cs (Entity → ViewModel)
```

### 🎯 ViewModels
**Existing Pattern:**
```
ViewModels/Patient/
├── DashboardViewModel.cs
├── DashboardQuickStatsViewModel.cs
└── DashboardAppointmentsSectionViewModel.cs
```

**New Required:**
```
ViewModels/Patient/MedicalRecord/
├── MedicalRecordIndexViewModel.cs
├── MedicalRecordSectionViewModel.cs
├── MedicalHistoryViewModel.cs
├── MedicalHistoryCreateEditViewModel.cs
└── MedicalRecordStatsViewModel.cs
```

### 🎯 Views (Component-Based)
**Existing Pattern:**
```
Areas/Patient/Views/Dashboard/
├── Index.cshtml (Main View)
├── _DashboardShell.cshtml (Shell)
├── _DashboardQuickStats.cshtml (Component)
├── _DashboardAppointmentsList.cshtml (Component)
└── _DashboardReceptionsList.cshtml (Component)
```

**New Required:**
```
Areas/Patient/Views/MedicalRecord/
├── Index.cshtml (Main View - AJAX-Compatible)
├── _MedicalRecordShell.cshtml (Shell)
├── _MedicalHistorySection.cshtml (Component - AJAX)
├── _AppointmentsSection.cshtml (Component - AJAX)
├── _ReceptionsSection.cshtml (Component - AJAX)
├── _VitalSignsSection.cshtml (Component - AJAX)
└── _MedicalHistoryForm.cshtml (Component - Create/Edit)
```

### 🎯 JavaScript (AJAX-First)
**Existing Pattern:**
```
Content/js/
├── patient-dashboard.js (AJAX Component Loading)
└── user-profile-menu.js (AJAX Navigation)
```

**New Required:**
```
Content/js/
└── medical-record.js (AJAX Component Manager)
```

### 🎯 Data / DB Touchpoints
**Existing Entities:**
- ✅ `Models/Entities/Patient/MedicalHistory.cs`
- ✅ `Models/Entities/Appointment/Appointment.cs`
- ✅ `Models/Entities/Reception/Reception.cs`
- ✅ `Models/Entities/Triage/TriageAssessment.cs`
- ✅ `Models/Entities/Triage/TriageVitalSigns.cs`

**DB Context:**
- ✅ `ApplicationDbContext.MedicalHistories`
- ✅ `ApplicationDbContext.Appointments`
- ✅ `ApplicationDbContext.Receptions`
- ✅ `ApplicationDbContext.TriageAssessments`

### 🎯 Filters / Cross-cutting Concerns
**Existing:**
- ✅ `[Authorize]` - Authorization
- ✅ `[NoCache]` - جلوگیری از Cache
- ✅ `BasePatientController` - Base Controller با Security

**Required:**
- ✅ استفاده از `[Authorize]` در Controller
- ✅ استفاده از `[NoCache]` در Controller
- ✅ استفاده از `BasePatientController`

---

## 🔗 STEP 2 — Dependency & Impact Map

### 📥 Depends On (وابستگی‌ها)
1. **IPatientService** - دریافت اطلاعات بیمار
2. **IAppointmentBookingService** - دریافت نوبت‌ها
3. **IReceptionService** (اگر موجود باشد) - دریافت پذیرش‌ها
4. **ICurrentUserService** - Authorization
5. **ApplicationDbContext** - دسترسی به دیتابیس
6. **ILogger** - Logging
7. **IAppSettings** - تنظیمات

### 📤 Used By (استفاده‌کنندگان)
1. **Views/Shared/_LoginPartial.cshtml** - لینک "پرونده الکترونیک"
2. **Patient Dashboard** - لینک به EMR
3. **User Profile Menu** - لینک در Dropdown

### 🔄 Change Impact Zones
1. **Views/Shared/_LoginPartial.cshtml:90** - لینک موجود است، باید Controller ایجاد شود
2. **Areas/Patient/PatientAreaRegistration.cs** - Route خودکار (نیاز به تغییر ندارد)
3. **App_Start/UnityConfig.cs** - باید Service و Repository ثبت شوند
4. **Content/js/user-profile-menu.js** - AJAX Navigation موجود است (نیاز به تغییر ندارد)

---

## ⚠️ STEP 3 — Critical Issues (Max 5)

### 🔴 Issue #1: Missing Factory Pattern (ARCHITECTURE VIOLATION)
**Evidence:**
- `Services/PatientService.cs:72` - استفاده از `ConvertToPatientIndexViewModel` (مستقیم)
- `Services/PatientDashboardService.cs:148` - استفاده از `Select` برای تبدیل (مستقیم)
- هیچ Factory Pattern در پروژه وجود ندارد
- **Contract Violation:** "Entity → ViewModel ONLY via Factory Method"

**Why it matters:**
- **Architecture:** نقض قرارداد پروژه
- **Maintainability:** تغییرات در Entity باید در یک مکان مدیریت شود
- **Testability:** Factory Method قابل تست است
- **Reusability:** Factory Method قابل استفاده مجدد است

**Impact:** **CRITICAL** - نقض قرارداد اصلی پروژه

---

### 🔴 Issue #2: Missing Component-Based AJAX Architecture (ARCHITECTURE VIOLATION)
**Evidence:**
- `Content/js/patient-dashboard.js` - Component-Based AJAX موجود است
- `Areas/Patient/Views/Dashboard/_DashboardShell.cshtml` - Shell Pattern موجود است
- اما EMR باید از همان Pattern استفاده کند

**Why it matters:**
- **Consistency:** باید با Dashboard سازگار باشد
- **User Experience:** SPA-like experience
- **Performance:** Lazy Loading برای بخش‌ها
- **Maintainability:** Component-Based قابل نگهداری است

**Impact:** **HIGH** - نیاز به پیاده‌سازی Component-Based

---

### 🔴 Issue #3: Missing ServiceResult Enhanced Pattern (ARCHITECTURE VIOLATION)
**Evidence:**
- `Services/PatientDashboardService.cs:88` - استفاده از `ServiceResult<T>.Successful`
- `Models/Core/ServiceResult.cs` - ServiceResult Enhanced موجود است
- اما EMR Service باید از همان Pattern استفاده کند

**Why it matters:**
- **Consistency:** باید با سایر Services سازگار باشد
- **Error Handling:** ServiceResult Enhanced برای مدیریت خطا
- **Logging:** ServiceResult Enhanced برای Logging
- **Contract:** "All outputs via ServiceResult Enhanced"

**Impact:** **HIGH** - نقض قرارداد پروژه

---

### 🔴 Issue #4: Missing Authorization in Service Layer (SECURITY RISK)
**Evidence:**
- `Services/PatientDashboardService.cs:361` - `ValidatePatientAccessAsync` موجود است
- اما EMR Service باید از همان Pattern استفاده کند

**Why it matters:**
- **Security:** بیماران نباید پرونده سایر بیماران را ببینند
- **Compliance:** رعایت استانداردهای حریم خصوصی
- **Defense in Depth:** Authorization در Controller و Service

**Impact:** **CRITICAL** - ریسک امنیتی بالا

---

### 🟡 Issue #5: Missing Repository Pattern for MedicalHistory (MAINTAINABILITY DEBT)
**Evidence:**
- `Repositories/Patient/PatientRepository.cs` - Repository Pattern موجود است
- اما `MedicalHistory` Repository وجود ندارد

**Why it matters:**
- **Consistency:** باید با سایر Repositories سازگار باشد
- **Testability:** Repository Pattern قابل تست است
- **Maintainability:** Data Access در یک لایه جداگانه

**Impact:** **MEDIUM** - نیاز به پیاده‌سازی Repository

---

## 🔍 STEP 4 — Root Cause Analysis

### 🔴 Issue #1: Missing Factory Pattern
**Root Cause:**
قرارداد پروژه می‌گوید "Entity → ViewModel ONLY via Factory Method" اما در کد موجود (`PatientService`, `PatientDashboardService`) از Convert Methods مستقیم استفاده می‌شود. این یک **Architecture Debt** است که باید در EMR Module رفع شود.

**Why it causes the observed behavior:**
- تبدیل Entity به ViewModel در Service Layer انجام می‌شود
- هیچ Factory Pattern وجود ندارد
- نقض قرارداد پروژه

**Why other causes are NOT root cause:**
- ❌ مشکل از ViewModel نیست - ViewModel ها درست هستند
- ❌ مشکل از Entity نیست - Entity ها درست هستند
- ✅ مشکل از عدم وجود Factory Pattern است

**Evidence:**
- `Services/PatientService.cs:72` - `ConvertToPatientIndexViewModel` (مستقیم)
- هیچ `Factories/Patient/` وجود ندارد

---

### 🔴 Issue #2: Missing Component-Based AJAX Architecture
**Root Cause:**
Dashboard از Component-Based AJAX استفاده می‌کند اما EMR باید از همان Pattern استفاده کند. این یک **Consistency Issue** است.

**Why it causes the observed behavior:**
- EMR باید با Dashboard سازگار باشد
- Component-Based برای Lazy Loading بهتر است
- AJAX-First برای UX بهتر است

**Why other causes are NOT root cause:**
- ❌ مشکل از JavaScript نیست - `patient-dashboard.js` موجود است
- ❌ مشکل از View نیست - Pattern موجود است
- ✅ مشکل از عدم پیاده‌سازی در EMR است

**Evidence:**
- `Content/js/patient-dashboard.js` - Component-Based AJAX موجود است
- `Areas/Patient/Views/Dashboard/_DashboardShell.cshtml` - Shell Pattern موجود است

---

### 🔴 Issue #3: Missing ServiceResult Enhanced Pattern
**Root Cause:**
قرارداد پروژه می‌گوید "All outputs via ServiceResult Enhanced" و `PatientDashboardService` از این Pattern استفاده می‌کند. EMR Service باید از همان Pattern استفاده کند.

**Why it causes the observed behavior:**
- ServiceResult Enhanced برای Error Handling بهتر است
- ServiceResult Enhanced برای Logging بهتر است
- Consistency با سایر Services

**Why other causes are NOT root cause:**
- ❌ مشکل از ServiceResult نیست - ServiceResult موجود است
- ❌ مشکل از Pattern نیست - Pattern موجود است
- ✅ مشکل از عدم استفاده در EMR Service است

**Evidence:**
- `Services/PatientDashboardService.cs:88` - `ServiceResult<T>.Successful`
- `Models/Core/ServiceResult.cs` - ServiceResult Enhanced موجود است

---

### 🔴 Issue #4: Missing Authorization in Service Layer
**Root Cause:**
`PatientDashboardService` از `ValidatePatientAccessAsync` استفاده می‌کند. EMR Service باید از همان Pattern استفاده کند.

**Why it causes the observed behavior:**
- Security: بیماران نباید پرونده سایر بیماران را ببینند
- Defense in Depth: Authorization در Controller و Service
- Compliance: رعایت استانداردهای حریم خصوصی

**Why other causes are NOT root cause:**
- ❌ مشکل از `CurrentUserService` نیست - Service موجود است
- ❌ مشکل از `BasePatientController` نیست - Base Controller موجود است
- ✅ مشکل از عدم استفاده در EMR Service است

**Evidence:**
- `Services/PatientDashboardService.cs:361` - `ValidatePatientAccessAsync` موجود است

---

### 🟡 Issue #5: Missing Repository Pattern for MedicalHistory
**Root Cause:**
`PatientRepository` وجود دارد اما `MedicalHistoryRepository` وجود ندارد. این یک **Consistency Issue** است.

**Why it causes the observed behavior:**
- Data Access باید در Repository Layer باشد
- Consistency با سایر Repositories
- Testability بهتر

**Why other causes are NOT root cause:**
- ❌ مشکل از Entity نیست - Entity موجود است
- ❌ مشکل از DbContext نیست - DbContext موجود است
- ✅ مشکل از عدم وجود Repository است

**Evidence:**
- `Repositories/Patient/PatientRepository.cs` - Repository Pattern موجود است
- اما `MedicalHistoryRepository` وجود ندارد

---

## 🛠️ STEP 5 — Fix Design (Minimal & Safe)

### 🎯 Strategy: Incremental Implementation with Architecture Compliance

#### Phase 1: Core Infrastructure (Factory + Repository + Service)
1. **Create Factory Pattern:**
   - `Factories/Patient/MedicalRecordFactory.cs`
   - `Factories/Patient/MedicalHistoryFactory.cs`
   - تبدیل Entity → ViewModel

2. **Create Repository:**
   - `Repositories/Patient/MedicalRecordRepository.cs`
   - `Repositories/Patient/MedicalHistoryRepository.cs`
   - Data Access Layer

3. **Create Service:**
   - `Services/Patient/MedicalRecordService.cs`
   - `Services/Patient/MedicalHistoryService.cs`
   - Business Logic Layer
   - استفاده از Factory Method
   - استفاده از ServiceResult Enhanced
   - استفاده از Authorization

#### Phase 2: API Layer (Component-Based AJAX)
1. **Create API Controller:**
   - `Areas/Patient/Controllers/Api/MedicalRecordApiController.cs`
   - Endpoint برای هر Component
   - ServiceResult Enhanced Response

2. **Create JavaScript:**
   - `Content/js/medical-record.js`
   - Component-Based AJAX Loading
   - Pattern مشابه `patient-dashboard.js`

#### Phase 3: MVC Layer (Views)
1. **Create MVC Controller:**
   - `Areas/Patient/Controllers/MedicalRecordController.cs`
   - AJAX-Compatible
   - استفاده از `BasePatientController`

2. **Create Views:**
   - `Areas/Patient/Views/MedicalRecord/Index.cshtml`
   - `Areas/Patient/Views/MedicalRecord/_MedicalRecordShell.cshtml`
   - Component Views (Partial)

#### Phase 4: Integration
1. **Register in UnityConfig:**
   - Service Registration
   - Repository Registration

2. **Test & Verify:**
   - Unit Tests
   - Integration Tests
   - Manual Verification

### 🎯 Design Principles
1. **SRP (Single Responsibility):**
   - Repository: فقط Data Access
   - Service: فقط Business Logic
   - Factory: فقط Entity → ViewModel
   - Controller: فقط Orchestration

2. **Component-Based:**
   - هر بخش (Medical History, Appointments, etc.) یک Component مستقل
   - AJAX Loading برای هر Component
   - Lazy Loading برای Performance

3. **AJAX-First:**
   - بدون رفرش صفحه
   - استفاده از `user-profile-menu.js` برای Navigation
   - Component-Based Loading

4. **Architecture Compliance:**
   - Factory Method برای Entity → ViewModel
   - ServiceResult Enhanced برای تمام Outputs
   - Repository Pattern برای Data Access
   - Authorization در Controller و Service

---

## 📝 STEP 6 — Implementation Plan

### 📁 File Structure (Complete)
```
Areas/Patient/
├── Controllers/
│   ├── MedicalRecordController.cs (NEW)
│   └── Api/
│       └── MedicalRecordApiController.cs (NEW)
├── Views/
│   └── MedicalRecord/
│       ├── Index.cshtml (NEW)
│       ├── _MedicalRecordShell.cshtml (NEW)
│       ├── _MedicalHistorySection.cshtml (NEW)
│       ├── _AppointmentsSection.cshtml (NEW)
│       ├── _ReceptionsSection.cshtml (NEW)
│       ├── _VitalSignsSection.cshtml (NEW)
│       └── _MedicalHistoryForm.cshtml (NEW)

Services/Patient/
├── MedicalRecordService.cs (NEW)
└── MedicalHistoryService.cs (NEW)

Repositories/Patient/
├── MedicalRecordRepository.cs (NEW)
└── MedicalHistoryRepository.cs (NEW)

Interfaces/
└── IPatientMedicalRecordService.cs (NEW)

ViewModels/Patient/MedicalRecord/
├── MedicalRecordIndexViewModel.cs (NEW)
├── MedicalRecordSectionViewModel.cs (NEW)
├── MedicalHistoryViewModel.cs (NEW)
├── MedicalHistoryCreateEditViewModel.cs (NEW)
└── MedicalRecordStatsViewModel.cs (NEW)

Factories/Patient/
├── MedicalRecordFactory.cs (NEW)
└── MedicalHistoryFactory.cs (NEW)

Content/js/
└── medical-record.js (NEW)
```

### 🔧 Implementation Details

#### 1. Factory Layer (CRITICAL - MUST HAVE)
```csharp
// Factories/Patient/MedicalRecordFactory.cs
using ClinicApp.Models.Entities.Patient;
using ClinicApp.ViewModels.Patient.MedicalRecord;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicApp.Factories.Patient
{
    /// <summary>
    /// Factory برای تبدیل Entity به ViewModel
    /// Single Responsibility: فقط تبدیل Entity → ViewModel
    /// ✅ Contract Compliance: "Entity → ViewModel ONLY via Factory Method"
    /// </summary>
    public static class MedicalRecordFactory
    {
        /// <summary>
        /// تبدیل MedicalHistory Entity به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static MedicalHistoryViewModel ToViewModel(MedicalHistory entity)
        {
            if (entity == null) return null;
            
            return new MedicalHistoryViewModel
            {
                MedicalHistoryId = entity.MedicalHistoryId,
                PatientId = entity.PatientId,
                Type = entity.Type,
                TypeText = GetMedicalHistoryTypeText(entity.Type),
                Title = entity.Title,
                Description = entity.Description,
                StartDate = entity.StartDate,
                StartDateShamsi = entity.StartDate?.ToPersianDate(),
                EndDate = entity.EndDate,
                EndDateShamsi = entity.EndDate?.ToPersianDate(),
                IsActive = entity.IsActive,
                Severity = entity.Severity,
                DoctorName = entity.DoctorName,
                MedicalCenter = entity.MedicalCenter,
                Attachments = entity.Attachments,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                UpdatedAt = entity.UpdatedAt,
                UpdatedAtShamsi = entity.UpdatedAt?.ToPersianDateTime()
            };
        }
        
        /// <summary>
        /// تبدیل لیست MedicalHistory به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static List<MedicalHistoryViewModel> ToViewModelList(
            IEnumerable<MedicalHistory> entities)
        {
            return entities?.Select(ToViewModel).Where(vm => vm != null).ToList() 
                ?? new List<MedicalHistoryViewModel>();
        }
        
        /// <summary>
        /// تبدیل ViewModel به Entity (برای Create/Update)
        /// ✅ Factory Method Pattern
        /// </summary>
        public static MedicalHistory ToEntity(MedicalHistoryCreateEditViewModel viewModel, 
            int patientId, string createdByUserId)
        {
            if (viewModel == null) return null;
            
            return new MedicalHistory
            {
                MedicalHistoryId = viewModel.MedicalHistoryId ?? 0,
                PatientId = patientId,
                Type = viewModel.Type,
                Title = viewModel.Title,
                Description = viewModel.Description,
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                IsActive = viewModel.IsActive,
                Severity = viewModel.Severity,
                DoctorName = viewModel.DoctorName,
                MedicalCenter = viewModel.MedicalCenter,
                Attachments = viewModel.Attachments,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.Now
            };
        }
        
        /// <summary>
        /// تبدیل نوع تاریخچه پزشکی به متن فارسی
        /// </summary>
        private static string GetMedicalHistoryTypeText(MedicalHistoryType type)
        {
            switch (type)
            {
                case MedicalHistoryType.Disease:
                    return "بیماری";
                case MedicalHistoryType.Surgery:
                    return "جراحی";
                case MedicalHistoryType.Injury:
                    return "آسیب";
                case MedicalHistoryType.Medication:
                    return "دارو";
                case MedicalHistoryType.Allergy:
                    return "آلرژی";
                case MedicalHistoryType.FamilyHistory:
                    return "سابقه خانوادگی";
                case MedicalHistoryType.Other:
                    return "سایر";
                default:
                    return "نامشخص";
            }
        }
    }
}
```

#### 2. Repository Layer
```csharp
// Repositories/Patient/MedicalRecordRepository.cs
namespace ClinicApp.Repositories.Patient
{
    /// <summary>
    /// Repository برای دسترسی به داده‌های EMR
    /// Single Responsibility: فقط Data Access
    /// </summary>
    public interface IMedicalRecordRepository
    {
        Task<List<MedicalHistory>> GetMedicalHistoriesByPatientIdAsync(
            int patientId, bool includeDeleted = false);
        Task<MedicalHistory> GetMedicalHistoryByIdAsync(int medicalHistoryId);
        Task<MedicalHistory> CreateMedicalHistoryAsync(MedicalHistory entity);
        Task<MedicalHistory> UpdateMedicalHistoryAsync(MedicalHistory entity);
        Task<bool> DeleteMedicalHistoryAsync(int medicalHistoryId, string deletedByUserId);
    }
    
    public class MedicalRecordRepository : BaseRepository<MedicalHistory>, 
        IMedicalRecordRepository
    {
        public MedicalRecordRepository(ApplicationDbContext context) : base(context) { }
        
        public async Task<List<MedicalHistory>> GetMedicalHistoriesByPatientIdAsync(
            int patientId, bool includeDeleted = false)
        {
            var query = _context.MedicalHistories
                .Where(mh => mh.PatientId == patientId);
                
            if (!includeDeleted)
            {
                query = query.Where(mh => !mh.IsDeleted);
            }
            
            return await query
                .OrderByDescending(mh => mh.StartDate ?? mh.CreatedAt)
                .ToListAsync();
        }
        
        // ... سایر متدها
    }
}
```

#### 3. Service Layer (ServiceResult Enhanced + Factory)
```csharp
// Services/Patient/MedicalRecordService.cs
namespace ClinicApp.Services.Patient
{
    /// <summary>
    /// Service برای مدیریت پرونده الکترونیک بیمار
    /// Single Responsibility: فقط Business Logic
    /// </summary>
    public interface IPatientMedicalRecordService
    {
        Task<ServiceResult<MedicalRecordIndexViewModel>> GetMedicalRecordAsync(int patientId);
        Task<ServiceResult<List<MedicalHistoryViewModel>>> GetMedicalHistoriesAsync(int patientId);
        Task<ServiceResult<MedicalHistoryViewModel>> GetMedicalHistoryByIdAsync(
            int medicalHistoryId, int patientId);
        Task<ServiceResult> CreateMedicalHistoryAsync(
            MedicalHistoryCreateEditViewModel model, int patientId);
        Task<ServiceResult> UpdateMedicalHistoryAsync(
            MedicalHistoryCreateEditViewModel model, int patientId);
    }
    
    public class MedicalRecordService : IPatientMedicalRecordService
    {
        private readonly IMedicalRecordRepository _repository;
        private readonly IPatientService _patientService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;
        
        public MedicalRecordService(
            IMedicalRecordRepository repository,
            IPatientService patientService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<MedicalRecordService>();
        }
        
        /// <summary>
        /// دریافت پرونده الکترونیک بیمار
        /// ✅ ServiceResult Enhanced
        /// ✅ Factory Method برای تبدیل Entity → ViewModel
        /// ✅ Authorization
        /// </summary>
        public async Task<ServiceResult<MedicalRecordIndexViewModel>> GetMedicalRecordAsync(
            int patientId)
        {
            try
            {
                // ✅ Authorization
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<MedicalRecordIndexViewModel>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }
                
                _logger.Information("دریافت پرونده الکترونیک - PatientId: {PatientId}", patientId);
                
                // ✅ دریافت تاریخچه پزشکی
                var medicalHistories = await _repository.GetMedicalHistoriesByPatientIdAsync(patientId);
                
                // ✅ Factory Method برای تبدیل Entity → ViewModel
                var medicalHistoryViewModels = MedicalRecordFactory.ToViewModelList(medicalHistories);
                
                var viewModel = new MedicalRecordIndexViewModel
                {
                    PatientId = patientId,
                    MedicalHistories = medicalHistoryViewModels,
                    // سایر بخش‌ها از API Controller لود می‌شوند (Component-Based)
                };
                
                // ✅ ServiceResult Enhanced
                return ServiceResult<MedicalRecordIndexViewModel>.Successful(
                    viewModel,
                    "پرونده الکترونیک با موفقیت دریافت شد.",
                    operationName: "GetMedicalRecord",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرونده الکترونیک - PatientId: {PatientId}", patientId);
                return ServiceResult<MedicalRecordIndexViewModel>.Failed(
                    "خطا در دریافت پرونده الکترونیک",
                    "GET_MEDICAL_RECORD_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
        
        // ✅ Authorization Helper
        private async Task<bool> ValidatePatientAccessAsync(int patientId)
        {
            try
            {
                var currentPatient = await _currentUserService.GetPatientInfoAsync();
                if (currentPatient == null) return false;
                return currentPatient.PatientId == patientId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error validating patient access - PatientId: {PatientId}", patientId);
                return false;
            }
        }
    }
}
```

#### 4. API Controller (Component-Based AJAX)
```csharp
// Areas/Patient/Controllers/Api/MedicalRecordApiController.cs
namespace ClinicApp.Areas.Patient.Controllers.Api
{
    /// <summary>
    /// API Controller برای Component-Based AJAX Loading
    /// Single Responsibility: فقط API Endpoints
    /// </summary>
    [Authorize]
    [Route("api/patient/medical-record")]
    public class MedicalRecordApiController : BasePatientController
    {
        private readonly IPatientMedicalRecordService _medicalRecordService;
        
        public MedicalRecordApiController(
            IPatientMedicalRecordService medicalRecordService,
            ILogger logger,
            ICurrentUserService currentUserService)
            : base(logger, currentUserService)
        {
            _medicalRecordService = medicalRecordService ?? 
                throw new ArgumentNullException(nameof(medicalRecordService));
        }
        
        /// <summary>
        /// دریافت بخش تاریخچه پزشکی (Component)
        /// GET: /api/patient/medical-record/medical-histories
        /// </summary>
        [HttpGet]
        [Route("medical-histories")]
        public async Task<JsonResult> GetMedicalHistories()
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }
                
                var result = await _medicalRecordService.GetMedicalHistoriesAsync(patientId.Value);
                
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }
                
                return SuccessJsonResult(result.Data, result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه پزشکی");
                return ErrorJsonResult("خطا در دریافت تاریخچه پزشکی");
            }
        }
    }
}
```

#### 5. MVC Controller (AJAX-Compatible)
```csharp
// Areas/Patient/Controllers/MedicalRecordController.cs
namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// Controller برای پرونده الکترونیک بیمار
    /// Single Responsibility: فقط Orchestration
    /// ✅ AJAX-Compatible
    /// </summary>
    [Authorize]
    [NoCache]
    public class MedicalRecordController : BasePatientController
    {
        private readonly IPatientMedicalRecordService _medicalRecordService;
        
        public MedicalRecordController(
            IPatientMedicalRecordService medicalRecordService,
            ILogger logger,
            ICurrentUserService currentUserService)
            : base(logger, currentUserService)
        {
            _medicalRecordService = medicalRecordService ?? 
                throw new ArgumentNullException(nameof(medicalRecordService));
        }
        
        /// <summary>
        /// نمایش صفحه اصلی پرونده الکترونیک
        /// GET: /Patient/MedicalRecord
        /// ✅ AJAX-Compatible: پشتیبانی از درخواست‌های AJAX
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { 
                            success = false, 
                            message = "اطلاعات بیمار یافت نشد",
                            redirectUrl = Url.Action("Login", "Account", new { area = "" })
                        }, JsonRequestBehavior.AllowGet);
                    }
                    return RedirectToAction("Login", "Account", new { area = "" });
                }
                
                // ✅ AJAX Request: Return Partial View (بدون Layout)
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_MedicalRecordShell", new MedicalRecordIndexViewModel
                    {
                        PatientId = patientId.Value,
                        MedicalHistories = null // Will be loaded via AJAX
                    });
                }
                
                // ✅ Normal Request: Return Full View (با Layout)
                var result = await _medicalRecordService.GetMedicalRecordAsync(patientId.Value);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(new MedicalRecordIndexViewModel());
                }
                
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش پرونده الکترونیک");
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "خطا در بارگذاری" }, 
                        JsonRequestBehavior.AllowGet);
                }
                NotificationHelper.SetError(TempData, "خطا در بارگذاری پرونده الکترونیک");
                return View(new MedicalRecordIndexViewModel());
            }
        }
    }
}
```

#### 6. JavaScript (Component-Based AJAX)
```javascript
// Content/js/medical-record.js
/**
 * Medical Record Component Manager
 * Single Responsibility: مدیریت AJAX Loading برای Components
 * Pattern: مشابه patient-dashboard.js
 */
(function($) {
    'use strict';
    
    var MedicalRecord = {
        config: {
            apiBaseUrl: '/api/patient/medical-record',
            containerSelector: '#medicalRecordContainer',
            sections: {
                medicalHistory: '#medicalHistorySection',
                appointments: '#appointmentsSection',
                receptions: '#receptionsSection',
                vitalSigns: '#vitalSignsSection'
            }
        },
        
        /**
         * Initialize
         */
        init: function() {
            this.loadAllSections();
            this.bindEvents();
        },
        
        /**
         * Load all sections via AJAX (Component-Based)
         */
        loadAllSections: function() {
            this.loadSection('medical-history');
            this.loadSection('appointments');
            this.loadSection('receptions');
            this.loadSection('vital-signs');
        },
        
        /**
         * Load a specific section (Component)
         */
        loadSection: function(sectionName) {
            var self = this;
            var $container = $(this.config.sections[sectionName] || 
                '#' + sectionName + 'Section');
            
            if ($container.length === 0) return;
            
            // Show loading
            $container.html('<div class="text-center p-4"><div class="spinner-border"></div></div>');
            
            $.ajax({
                url: this.config.apiBaseUrl + '/' + sectionName.replace('-', '-'),
                method: 'GET',
                dataType: 'json',
                success: function(response) {
                    if (response.success && response.data) {
                        // Render section (will be handled by API returning HTML or data)
                        self.renderSection($container, sectionName, response.data);
                    } else {
                        self.showError($container, response.message || 'خطا در بارگذاری');
                    }
                },
                error: function(xhr) {
                    if (xhr.status === 401) {
                        if (window.openLoginModal) {
                            window.openLoginModal();
                        }
                    } else {
                        self.showError($container, 'خطا در ارتباط با سرور');
                    }
                }
            });
        },
        
        /**
         * Render section
         */
        renderSection: function($container, sectionName, data) {
            // Render based on section type
            // This will be handled by API returning HTML or by client-side rendering
            $container.html(data.html || this.generateSectionHTML(sectionName, data));
        },
        
        /**
         * Show error
         */
        showError: function($container, message) {
            $container.html('<div class="alert alert-danger">' + message + '</div>');
        },
        
        /**
         * Bind events
         */
        bindEvents: function() {
            var self = this;
            
            // Refresh button
            $(document).on('click', '.refresh-medical-record', function() {
                self.loadAllSections();
            });
        }
    };
    
    // Initialize on document ready
    $(document).ready(function() {
        if ($('#medicalRecordContainer').length > 0) {
            MedicalRecord.init();
        }
    });
    
    // Expose globally
    window.MedicalRecord = MedicalRecord;
})(jQuery);
```

#### 7. View (Component-Based)
```html
@* Areas/Patient/Views/MedicalRecord/_MedicalRecordShell.cshtml *@
@model ClinicApp.ViewModels.Patient.MedicalRecord.MedicalRecordIndexViewModel

<div id="medicalRecordContainer" class="medical-record-container">
    <div class="medical-record-header">
        <h2>پرونده الکترونیک سلامت</h2>
        <button class="btn btn-primary refresh-medical-record">
            <i class="fas fa-sync-alt"></i> به‌روزرسانی
        </button>
    </div>
    
    <div class="medical-record-sections">
        @* Component: Medical History *@
        <div id="medicalHistorySection" class="medical-record-section">
            <div class="text-center p-4">
                <div class="spinner-border"></div>
            </div>
        </div>
        
        @* Component: Appointments *@
        <div id="appointmentsSection" class="medical-record-section">
            <div class="text-center p-4">
                <div class="spinner-border"></div>
            </div>
        </div>
        
        @* Component: Receptions *@
        <div id="receptionsSection" class="medical-record-section">
            <div class="text-center p-4">
                <div class="spinner-border"></div>
            </div>
        </div>
        
        @* Component: Vital Signs *@
        <div id="vitalSignsSection" class="medical-record-section">
            <div class="text-center p-4">
                <div class="spinner-border"></div>
            </div>
        </div>
    </div>
</div>

<script src="~/Content/js/medical-record.js"></script>
```

#### 8. UnityConfig Registration
```csharp
// App_Start/UnityConfig.cs
// در متد RegisterTypes:

// Repository
container.RegisterType<IMedicalRecordRepository, MedicalRecordRepository>(
    new PerRequestLifetimeManager());
container.RegisterType<IMedicalHistoryRepository, MedicalHistoryRepository>(
    new PerRequestLifetimeManager());

// Service
container.RegisterType<IPatientMedicalRecordService, MedicalRecordService>(
    new PerRequestLifetimeManager());
container.RegisterType<IPatientMedicalHistoryService, MedicalHistoryService>(
    new PerRequestLifetimeManager());
```

---

## ✅ STEP 7 — Tests & Verification

### 🧪 Unit Tests

#### 1. Factory Tests
```csharp
// Tests/Factories/Patient/MedicalRecordFactoryTests.cs
[TestClass]
public class MedicalRecordFactoryTests
{
    [TestMethod]
    public void ToViewModel_ValidEntity_ReturnsViewModel()
    {
        // Arrange
        var entity = new MedicalHistory
        {
            MedicalHistoryId = 1,
            Type = MedicalHistoryType.Disease,
            Title = "Test",
            // ...
        };
        
        // Act
        var viewModel = MedicalRecordFactory.ToViewModel(entity);
        
        // Assert
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(1, viewModel.MedicalHistoryId);
        Assert.AreEqual("بیماری", viewModel.TypeText);
    }
    
    [TestMethod]
    public void ToViewModel_NullEntity_ReturnsNull()
    {
        // Act
        var viewModel = MedicalRecordFactory.ToViewModel(null);
        
        // Assert
        Assert.IsNull(viewModel);
    }
}
```

#### 2. Service Tests
```csharp
// Tests/Services/Patient/MedicalRecordServiceTests.cs
[TestClass]
public class MedicalRecordServiceTests
{
    [TestMethod]
    public async Task GetMedicalRecordAsync_ValidPatientId_ReturnsSuccess()
    {
        // Arrange
        var patientId = 1;
        var mockRepository = new Mock<IMedicalRecordRepository>();
        var mockPatientService = new Mock<IPatientService>();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        
        mockCurrentUserService.Setup(s => s.GetPatientInfoAsync())
            .ReturnsAsync(new PatientInfo { PatientId = patientId });
        
        mockRepository.Setup(r => r.GetMedicalHistoriesByPatientIdAsync(patientId, false))
            .ReturnsAsync(new List<MedicalHistory>());
        
        var service = new MedicalRecordService(
            mockRepository.Object,
            mockPatientService.Object,
            mockCurrentUserService.Object,
            Mock.Of<ILogger>());
        
        // Act
        var result = await service.GetMedicalRecordAsync(patientId);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
    }
    
    [TestMethod]
    public async Task GetMedicalRecordAsync_DifferentPatientId_ReturnsUnauthorized()
    {
        // Arrange
        var requestedPatientId = 1;
        var currentPatientId = 2;
        
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(s => s.GetPatientInfoAsync())
            .ReturnsAsync(new PatientInfo { PatientId = currentPatientId });
        
        var service = new MedicalRecordService(
            Mock.Of<IMedicalRecordRepository>(),
            Mock.Of<IPatientService>(),
            mockCurrentUserService.Object,
            Mock.Of<ILogger>());
        
        // Act
        var result = await service.GetMedicalRecordAsync(requestedPatientId);
        
        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("UNAUTHORIZED_ACCESS", result.Code);
    }
}
```

#### 3. Repository Tests
```csharp
// Tests/Repositories/Patient/MedicalRecordRepositoryTests.cs
[TestClass]
public class MedicalRecordRepositoryTests
{
    [TestMethod]
    public async Task GetMedicalHistoriesByPatientIdAsync_ValidId_ReturnsList()
    {
        // Arrange
        var patientId = 1;
        var context = new TestApplicationDbContext();
        var repository = new MedicalRecordRepository(context);
        
        // Act
        var result = await repository.GetMedicalHistoriesByPatientIdAsync(patientId);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.All(mh => mh.PatientId == patientId));
        Assert.IsTrue(result.All(mh => !mh.IsDeleted));
    }
}
```

### 🔍 Integration Tests
```csharp
// Tests/Integration/MedicalRecordIntegrationTests.cs
[TestClass]
public class MedicalRecordIntegrationTests
{
    [TestMethod]
    public async Task Index_AuthenticatedPatient_ReturnsView()
    {
        // Arrange
        var controller = CreateController();
        var patientId = await GetCurrentPatientIdAsync();
        
        // Act
        var result = await controller.Index();
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }
    
    [TestMethod]
    public async Task Index_AJAXRequest_ReturnsPartialView()
    {
        // Arrange
        var controller = CreateController();
        controller.Request = CreateAjaxRequest();
        
        // Act
        var result = await controller.Index();
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(PartialViewResult));
    }
}
```

### ✅ Manual Verification Steps
1. ✅ لاگین به عنوان بیمار
2. ✅ کلیک روی "پرونده الکترونیک" در منوی پروفایل
3. ✅ بررسی AJAX Navigation (بدون رفرش صفحه)
4. ✅ بررسی Component-Based Loading (هر بخش جداگانه لود می‌شود)
5. ✅ بررسی Authorization (تلاش برای دسترسی به پرونده بیمار دیگر)
6. ✅ بررسی Factory Method (تبدیل Entity → ViewModel)
7. ✅ بررسی ServiceResult Enhanced (Error Handling)
8. ✅ بررسی Performance (Lazy Loading)

---

## 🔄 STEP 8 — Rollback & Safety

### 🛡️ Safe Rollback Steps

#### 1. Feature Flag
```csharp
// AppSettings
"EnableMedicalRecord": "true"

// Controller
if (!_appSettings.EnableMedicalRecord)
{
    return RedirectToAction("Index", "Dashboard");
}
```

#### 2. Database Migration
- اگر Entity جدید اضافه شود، Migration باید Reversible باشد
- Backup قبل از Migration
- استفاده از `Add-Migration` و `Update-Database`

#### 3. Code Rollback
- Git Branch برای EMR Module: `feature/emr-module`
- امکان Rollback سریع: `git checkout main`

### 🔒 Safety Measures

#### 1. Authorization
- بررسی در Controller: `BasePatientController.GetCurrentPatientIdAsync()`
- بررسی در Service: `ValidatePatientAccessAsync()`
- Logging تمام دسترسی‌ها

#### 2. Data Validation
- Validation در ViewModel: `[Required]`, `[MaxLength]`
- Validation در Service: Business Rules
- Validation در Repository: Data Integrity

#### 3. Error Handling
- Try-Catch در تمام لایه‌ها
- ServiceResult Enhanced برای Error Messages
- User-Friendly Error Messages
- Logging برای Debugging

---

## 📊 STEP 9 — ServiceResult Example

### ✅ Successful Response
```csharp
// Service Layer
return ServiceResult<MedicalRecordIndexViewModel>.Successful(
    viewModel,
    "پرونده الکترونیک با موفقیت دریافت شد.",
    operationName: "GetMedicalRecord",
    userId: _currentUserService.UserId,
    userFullName: _currentUserService.UserName);

// JSON Response (API)
{
    "success": true,
    "data": {
        "patientId": 1,
        "medicalHistories": [...],
        // ...
    },
    "message": "پرونده الکترونیک با موفقیت دریافت شد.",
    "code": "SUCCESS",
    "operationName": "GetMedicalRecord",
    "userId": "user-id",
    "userFullName": "User Name"
}
```

### ❌ Failed Response
```csharp
// Service Layer
return ServiceResult<MedicalRecordIndexViewModel>.Failed(
    "دسترسی غیرمجاز",
    "UNAUTHORIZED_ACCESS",
    ErrorCategory.Security,
    SecurityLevel.High);

// JSON Response (API)
{
    "success": false,
    "message": "دسترسی غیرمجاز",
    "code": "UNAUTHORIZED_ACCESS",
    "errorCategory": "Security",
    "securityLevel": "High"
}
```

---

## ✅ STEP 10 — Verification Steps

### 🔍 Pre-Implementation Verification
1. ✅ بررسی قراردادها (Contracts)
2. ✅ بررسی معماری موجود (Architecture)
3. ✅ بررسی Pattern های موجود (Patterns)
4. ✅ بررسی Dependencies (Dependencies)

### 🔍 Post-Implementation Verification
1. ✅ Unit Tests Pass
2. ✅ Integration Tests Pass
3. ✅ Manual Testing
4. ✅ Performance Testing
5. ✅ Security Testing
6. ✅ UI/UX Testing

### 🔍 Production Verification
1. ✅ Feature Flag فعال است
2. ✅ Monitoring فعال است
3. ✅ Logging فعال است
4. ✅ Error Tracking فعال است

---

## ❓ STEP 11 — Open Questions / Missing Info

### ❓ Business Logic Questions
1. **ویرایش تاریخچه پزشکی:**
   - آیا بیمار می‌تواند تاریخچه پزشکی خود را ویرایش کند؟
   - آیا نیاز به تایید پزشک وجود دارد؟
   - آیا نیاز به Workflow برای تایید وجود دارد؟

2. **دسترسی:**
   - آیا پزشک می‌تواند پرونده بیمار را ببیند؟
   - آیا منشی می‌تواند پرونده بیمار را ببیند؟
   - آیا نیاز به Role-Based Access Control وجود دارد؟

3. **Export:**
   - آیا نیاز به Export به PDF وجود دارد؟
   - آیا نیاز به Export به Excel وجود دارد؟
   - آیا نیاز به Print وجود دارد؟

### 📋 Missing Information
1. **Requirements:**
   - نیازمندی‌های دقیق Business
   - استانداردهای پزشکی ایران برای EMR
   - الزامات Compliance

2. **Integration:**
   - آیا نیاز به Integration با سیستم‌های خارجی وجود دارد؟
   - آیا نیاز به API برای دسترسی خارجی وجود دارد؟
   - آیا نیاز به Webhook وجود دارد؟

3. **Performance:**
   - چه تعداد رکورد در هر بخش انتظار می‌رود؟
   - آیا نیاز به Pagination برای هر بخش وجود دارد؟
   - آیا نیاز به Caching وجود دارد؟

---

## 📋 STEP 12 — Summary & Next Steps

### ✅ What Will Be Created
1. **Factory Layer:** `Factories/Patient/MedicalRecordFactory.cs`
2. **Repository Layer:** `Repositories/Patient/MedicalRecordRepository.cs`
3. **Service Layer:** `Services/Patient/MedicalRecordService.cs`
4. **API Layer:** `Areas/Patient/Controllers/Api/MedicalRecordApiController.cs`
5. **MVC Layer:** `Areas/Patient/Controllers/MedicalRecordController.cs`
6. **View Layer:** `Areas/Patient/Views/MedicalRecord/*.cshtml`
7. **JavaScript:** `Content/js/medical-record.js`
8. **ViewModels:** `ViewModels/Patient/MedicalRecord/*.cs`

### ✅ Architecture Compliance
- ✅ SRP (Single Responsibility Principle)
- ✅ Factory Method برای Entity → ViewModel
- ✅ ServiceResult Enhanced برای تمام Outputs
- ✅ Repository Pattern برای Data Access
- ✅ Component-Based Architecture
- ✅ AJAX-First (بدون رفرش صفحه)
- ✅ Authorization در Controller و Service

### 🎯 Next Steps
1. **Review این برنامه با تیم**
2. **تصمیم‌گیری در مورد Open Questions**
3. **شروع پیاده‌سازی Phase 1 (Factory + Repository + Service)**
4. **تست و تایید هر Phase قبل از ادامه**

---

**تهیه شده توسط:** AI Assistant (Senior Staff Engineer)  
**تاریخ:** 1404/10/05  
**وضعیت:** ✅ Ready for Implementation

