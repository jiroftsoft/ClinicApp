# Patient Dashboard - PHASE 0: Discovery & Architecture Fit

## 📋 Module Map

### Existing Architecture

#### Controllers
- ✅ `Areas/Patient/Controllers/AppointmentController.cs` - موجود
- ✅ `Areas/Patient/Controllers/Api/PatientAppointmentApiController.cs` - موجود
- ✅ `Areas/Patient/Controllers/Base/BasePatientController.cs` - موجود
- ❌ `Areas/Patient/Controllers/DashboardController.cs` - **نیاز به ایجاد**

#### Services
- ✅ `Services/PatientService.cs` - موجود
  - `GetPatientAppointmentsAsync(int patientId, int pageNumber, int pageSize)` ✅
  - `GetPatientReceptionsAsync(int patientId, int pageNumber, int pageSize)` ✅
- ✅ `Services/Appointment/AppointmentBookingService.cs` - موجود
  - `GetPatientAppointmentsAsync(int patientId, DateTime? startDate, DateTime? endDate)` ✅
- ✅ `Services/UserProfileService.cs` - موجود
- ❌ `Services/PatientDashboardService.cs` - **نیاز به ایجاد**

#### ViewModels
- ✅ `ViewModels/Patient/PatientAppointmentListViewModel.cs` - موجود
- ✅ `Models/DTOs/Appointment/PatientAppointmentDto.cs` - موجود
- ❌ `ViewModels/Patient/DashboardViewModel.cs` - **نیاز به ایجاد**
- ❌ `ViewModels/Patient/DashboardSectionViewModel.cs` - **نیاز به ایجاد**

#### Views
- ✅ `Areas/Patient/Views/Appointment/MyAppointments.cshtml` - موجود
- ✅ `Areas/Patient/Views/Shared/_PatientLayout.cshtml` - موجود
- ❌ `Areas/Patient/Views/Dashboard/Index.cshtml` - **نیاز به ایجاد**
- ❌ `Areas/Patient/Views/Dashboard/_DashboardRecentAppointments.cshtml` - **نیاز به ایجاد**

#### API Endpoints
- ✅ `Areas/Patient/Controllers/Api/PatientAppointmentApiController.cs` - موجود
  - `GetAppointments()` ✅
  - `GetAppointmentDetails(int id)` ✅
- ❌ `Areas/Patient/Controllers/Api/PatientDashboardApiController.cs` - **نیاز به ایجاد**

## 🔗 Dependency Graph

```
DashboardController
  ├── ICurrentUserService (✅ موجود)
  ├── IPatientDashboardService (❌ نیاز به ایجاد)
  └── ILogger (✅ موجود)

PatientDashboardService
  ├── ICurrentUserService (✅ موجود)
  ├── IAppointmentBookingService (✅ موجود)
  ├── IPatientService (✅ موجود)
  └── ILogger (✅ موجود)

PatientDashboardApiController
  ├── IPatientDashboardService (❌ نیاز به ایجاد)
  ├── ICurrentUserService (✅ موجود)
  └── ILogger (✅ موجود)
```

## ✅ Reuse Findings

### Exists (قابل استفاده مجدد)
1. ✅ `BasePatientController` - برای authorization و helper methods
2. ✅ `ICurrentUserService.GetPatientInfoAsync()` - برای دریافت patientId
3. ✅ `IAppointmentBookingService.GetPatientAppointmentsAsync()` - برای دریافت نوبت‌ها
4. ✅ `PatientService.GetPatientAppointmentsAsync()` - برای pagination
5. ✅ `PatientAppointmentDto` - برای نمایش نوبت‌ها
6. ✅ `_PatientLayout.cshtml` - برای layout
7. ✅ `ServiceResult` Enhanced - برای API responses
8. ✅ `UserProfileMenu` AJAX navigation - برای navigation بدون رفرش

### Missing (نیاز به ایجاد)
1. ❌ `IPatientDashboardService` - Service interface
2. ❌ `PatientDashboardService` - Service implementation
3. ❌ `DashboardController` - MVC Controller
4. ❌ `PatientDashboardApiController` - API Controller
5. ❌ `DashboardViewModel` - ViewModel
6. ❌ `Dashboard/Index.cshtml` - Main view
7. ❌ Dashboard section components (Partials)
8. ❌ Dashboard JavaScript module

## 🔐 Authorization Boundary

### Current Implementation
- ✅ `BasePatientController.GetCurrentPatientIdAsync()` - از `ICurrentUserService` استفاده می‌کند
- ✅ `ICurrentUserService.GetPatientInfoAsync()` - از `ApplicationUser` استفاده می‌کند
- ✅ Authorization: `[Authorize]` attribute در controllers

### Security Requirements
- ✅ Patient identity از auth context (نه query params)
- ✅ هر `patientId` در URL باید validate شود
- ✅ No PII leakage در errors/logs
- ✅ `[NoCache]` attribute برای sensitive pages

## 📊 Data Sources

### Available Data
1. ✅ **Appointments**: `IAppointmentBookingService.GetPatientAppointmentsAsync()`
2. ✅ **Receptions**: `PatientService.GetPatientReceptionsAsync()`
3. ✅ **Profile**: `IUserProfileService.GetMyProfileAsync()`
4. ❓ **Documents**: نیاز به بررسی
5. ❓ **Prescriptions**: نیاز به بررسی
6. ❓ **Invoices/Payments**: نیاز به بررسی
7. ❓ **Visits**: نیاز به بررسی

## 🎯 Next Steps

1. ✅ PHASE 0 Complete - Discovery done
2. ⏭️ PHASE 1 - Flow & Scenario Design
3. ⏭️ PHASE 2 - Minimal Vertical Slice

