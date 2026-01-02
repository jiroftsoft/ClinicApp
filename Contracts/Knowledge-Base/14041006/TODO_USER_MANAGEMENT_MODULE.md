# 📋 TODO: ماژول مدیریت کاربران
## ClinicApp - User Management Module Implementation

**تاریخ شروع:** ___________  
**تاریخ پایان:** ___________  
**وضعیت:** [ ] در حال انجام | [ ] تکمیل شده

---

## 🎯 هدف

ایجاد **قدرتمندترین سیستم مدیریت کاربران** برای مدیر مجموعه با رعایت:
- ✅ تمام 7 نقش متخصص
- ✅ تمام قراردادهای Critical
- ✅ استانداردهای UI/UX
- ✅ امنیت کامل
- ✅ Audit Trail کامل

---

## 📋 Phase 1: Repository Layer (1 روز)

### ✅ Interface Definition
- [ ] ایجاد `Interfaces/UserManagement/IUserRepository.cs`
- [ ] تعریف متدهای CRUD
- [ ] تعریف متدهای Search & Filter
- [ ] تعریف متدهای Role Management
- [ ] تعریف متدهای Statistics

### ✅ Implementation
- [ ] ایجاد `Repositories/UserManagement/UserRepository.cs`
- [ ] پیاده‌سازی `GetByIdAsync`
- [ ] پیاده‌سازی `GetByNationalCodeAsync`
- [ ] پیاده‌سازی `GetByEmailAsync`
- [ ] پیاده‌سازی `GetAllAsync`
- [ ] پیاده‌سازی `GetActiveUsersAsync`
- [ ] پیاده‌سازی `GetDeletedUsersAsync`
- [ ] پیاده‌سازی `SearchAsync` با Pagination
- [ ] پیاده‌سازی `AddAsync` با Audit Trail
- [ ] پیاده‌سازی `UpdateAsync` با Audit Trail
- [ ] پیاده‌سازی `SoftDeleteAsync` با Audit Trail
- [ ] پیاده‌سازی `RestoreAsync` با Audit Trail
- [ ] پیاده‌سازی `GetUserRolesAsync`
- [ ] پیاده‌سازی `IsInRoleAsync`
- [ ] پیاده‌سازی `GetTotalUsersCountAsync`
- [ ] پیاده‌سازی `GetActiveUsersCountAsync`
- [ ] پیاده‌سازی `GetUsersCountByRoleAsync`

### ✅ Dependency Injection
- [ ] ثبت `IUserRepository` در `UnityConfig.cs`
- [ ] تست Dependency Injection

### ✅ Testing
- [ ] Unit Tests برای Repository Methods
- [ ] Integration Tests با Database

---

## 📋 Phase 2: Service Layer (2 روز)

### ✅ Interface Definition
- [ ] ایجاد `Interfaces/UserManagement/IUserManagementService.cs`
- [ ] تعریف متدهای CRUD
- [ ] تعریف متدهای Role Management
- [ ] تعریف متدهای Activation/Deactivation
- [ ] تعریف متدهای Validation
- [ ] تعریف متدهای Statistics

### ✅ Implementation
- [ ] ایجاد `Services/UserManagement/UserManagementService.cs`
- [ ] پیاده‌سازی `GetUsersAsync` با Filter و Pagination
- [ ] پیاده‌سازی `GetUserDetailsAsync`
- [ ] پیاده‌سازی `GetUserForEditAsync`
- [ ] پیاده‌سازی `CreateUserAsync`:
  - [ ] Validation (NationalCode, Email)
  - [ ] ایجاد ApplicationUser
  - [ ] Assign Roles
  - [ ] Audit Trail
  - [ ] Logging
- [ ] پیاده‌سازی `UpdateUserAsync`:
  - [ ] Validation
  - [ ] Update ApplicationUser
  - [ ] Update Roles
  - [ ] Audit Trail
  - [ ] Logging
- [ ] پیاده‌سازی `DeleteUserAsync`:
  - [ ] Soft Delete
  - [ ] Audit Trail
  - [ ] Logging
- [ ] پیاده‌سازی `AssignRoleAsync`
- [ ] پیاده‌سازی `RemoveRoleAsync`
- [ ] پیاده‌سازی `GetAvailableRolesAsync`
- [ ] پیاده‌سازی `GetUserRolesAsync`
- [ ] پیاده‌سازی `ActivateUserAsync`
- [ ] پیاده‌سازی `DeactivateUserAsync`
- [ ] پیاده‌سازی `ValidateNationalCodeAsync`
- [ ] پیاده‌سازی `ValidateEmailAsync`
- [ ] پیاده‌سازی `GetStatisticsAsync`

### ✅ Business Logic
- [ ] Validation Rules
- [ ] Error Handling
- [ ] Transaction Management
- [ ] Logging کامل

### ✅ Dependency Injection
- [ ] ثبت `IUserManagementService` در `UnityConfig.cs`
- [ ] تست Dependency Injection

### ✅ Testing
- [ ] Unit Tests برای Service Methods
- [ ] Integration Tests
- [ ] Business Logic Tests

---

## 📋 Phase 3: ViewModel Layer (0.5 روز)

### ✅ ViewModels
- [ ] ایجاد `ViewModels/UserManagement/UserIndexViewModel.cs`
- [ ] ایجاد `ViewModels/UserManagement/UserListItemViewModel.cs`
- [ ] ایجاد `ViewModels/UserManagement/UserCreateEditViewModel.cs`
- [ ] ایجاد `ViewModels/UserManagement/UserDetailsViewModel.cs`
- [ ] ایجاد `ViewModels/UserManagement/RoleViewModel.cs`
- [ ] ایجاد `ViewModels/UserManagement/UserStatisticsViewModel.cs`
- [ ] ایجاد `ViewModels/UserManagement/UserSearchFilter.cs`

### ✅ Validation Attributes
- [ ] `[Required]` برای فیلدهای الزامی
- [ ] `[StringLength]` برای محدودیت طول
- [ ] `[EmailAddress]` برای Email
- [ ] `[Phone]` برای PhoneNumber
- [ ] Custom Validation برای NationalCode

### ✅ Factory Pattern
- [ ] ایجاد `Factories/UserViewModelFactory.cs`
- [ ] متد `ToViewModel` (Entity → ViewModel)
- [ ] متد `ToEntity` (ViewModel → Entity)

---

## 📋 Phase 4: Controller (1 روز)

### ✅ Controller Setup
- [ ] ایجاد `Areas/Admin/Controllers/UserManagementController.cs`
- [ ] تزریق `IUserManagementService`
- [ ] تزریق `ICurrentUserService`
- [ ] تزریق `ILogger`
- [ ] `[Authorize(Roles = AppRoles.Admin)]` برای کل Controller

### ✅ CRUD Actions
- [ ] `Index` (GET) - لیست کاربران
- [ ] `Create` (GET) - فرم ایجاد
- [ ] `Create` (POST) - ایجاد کاربر
- [ ] `Edit` (GET) - فرم ویرایش
- [ ] `Edit` (POST) - ویرایش کاربر
- [ ] `Details` (GET) - جزئیات کاربر
- [ ] `Delete` (POST) - حذف نرم کاربر

### ✅ Role Management Actions
- [ ] `AssignRole` (POST) - اختصاص نقش
- [ ] `RemoveRole` (POST) - حذف نقش
- [ ] `GetUserRoles` (GET) - دریافت نقش‌های کاربر

### ✅ Activation/Deactivation Actions
- [ ] `Activate` (POST) - فعال‌سازی کاربر
- [ ] `Deactivate` (POST) - غیرفعال‌سازی کاربر

### ✅ Validation & Error Handling
- [ ] `ModelState` Validation
- [ ] `ServiceResult` Handling
- [ ] `NotificationHelper` برای پیام‌ها
- [ ] Error Logging

### ✅ GetViewPath
- [ ] استفاده از `GetViewPath()` برای تمام Views

---

## 📋 Phase 5: Views (2 روز)

### ✅ Index View
- [ ] ایجاد `Areas/Admin/Views/UserManagement/Index.cshtml`
- [ ] Search Panel (NationalCode, Email, Name)
- [ ] Filter Panel (IsActive, Role)
- [ ] Data Table (Responsive):
  - [ ] FullName
  - [ ] NationalCode (Masked)
  - [ ] Email
  - [ ] PhoneNumber (Masked)
  - [ ] Roles
  - [ ] IsActive Status
  - [ ] CreatedAt
  - [ ] Action Buttons
- [ ] Pagination
- [ ] Statistics Cards:
  - [ ] Total Users
  - [ ] Active Users
  - [ ] Users by Role

### ✅ Create View
- [ ] ایجاد `Areas/Admin/Views/UserManagement/Create.cshtml`
- [ ] Form با Validation:
  - [ ] FirstName
  - [ ] LastName
  - [ ] NationalCode (با Real-time Validation)
  - [ ] Email (با Real-time Validation)
  - [ ] PhoneNumber
  - [ ] Gender
  - [ ] Address
  - [ ] IsActive
  - [ ] Roles (Multi-select)
- [ ] Submit Button
- [ ] Cancel Button

### ✅ Edit View
- [ ] ایجاد `Areas/Admin/Views/UserManagement/Edit.cshtml`
- [ ] Form مشابه Create
- [ ] Hidden Field برای UserId
- [ ] نمایش LastLoginDate (Read-only)

### ✅ Details View
- [ ] ایجاد `Areas/Admin/Views/UserManagement/Details.cshtml`
- [ ] User Information Display
- [ ] Roles Display
- [ ] Audit Trail Display:
  - [ ] CreatedAt, CreatedBy
  - [ ] UpdatedAt, UpdatedBy
  - [ ] DeletedAt, DeletedBy (در صورت حذف)
- [ ] Activity Log
- [ ] Action Buttons (Edit, Delete, Activate/Deactivate)

### ✅ Partial Views
- [ ] `_UserSearchPanel.cshtml`
- [ ] `_UserFilterPanel.cshtml`
- [ ] `_UserStatisticsCards.cshtml`
- [ ] `_UserRoleAssignment.cshtml`

### ✅ Layout & Styling
- [ ] استفاده از `--medical-*` colors
- [ ] Responsive Design
- [ ] RTL Support
- [ ] Font: Vazir یا IRANSansX

---

## 📋 Phase 6: JavaScript (1 روز)

### ✅ Main Script
- [ ] ایجاد `Scripts/admin/user-management.js`
- [ ] Search با Debounce (300ms)
- [ ] Filter Handling
- [ ] AJAX Calls
- [ ] Real-time Validation:
  - [ ] NationalCode Validation
  - [ ] Email Validation
- [ ] SweetAlert2 Confirmations:
  - [ ] Delete Confirmation
  - [ ] Deactivate Confirmation
  - [ ] Role Assignment Confirmation

### ✅ Event Handlers
- [ ] Search Input Handler
- [ ] Filter Change Handler
- [ ] Role Assignment Handler
- [ ] Activate/Deactivate Handler
- [ ] Delete Handler

### ✅ Error Handling
- [ ] AJAX Error Handling
- [ ] Toastr Error Messages
- [ ] Validation Error Display

---

## 📋 Phase 7: Security & Testing (1.5 روز)

### ✅ Security
- [ ] Authorization Tests
- [ ] Input Validation Tests
- [ ] SQL Injection Tests
- [ ] XSS Tests
- [ ] CSRF Tests

### ✅ Unit Tests
- [ ] Repository Tests
- [ ] Service Tests
- [ ] Controller Tests

### ✅ Integration Tests
- [ ] End-to-End Flow Tests
- [ ] Database Tests
- [ ] Role Management Tests

### ✅ Performance Tests
- [ ] Pagination Performance
- [ ] Search Performance
- [ ] Large Dataset Tests

---

## 📋 Phase 8: Documentation (0.5 روز)

### ✅ Documentation
- [ ] API Documentation
- [ ] User Guide
- [ ] Security Documentation
- [ ] Deployment Guide

---

## ✅ Checklist نهایی قبل از Commit

### UI/UX
- [ ] فونت Vazir یا IRANSansX
- [ ] رنگ‌های استاندارد `--medical-*`
- [ ] هیچ رنگ جیق و جلف
- [ ] Responsive Design

### Strongly-Typed
- [ ] تمام View ها دارای `@model`
- [ ] هیچ `ViewBag`/`ViewData` برای داده‌های اصلی
- [ ] تمام Actions از `GetViewPath()` استفاده می‌کنند

### Bulletproof
- [ ] تمام async ها دارای try-catch
- [ ] تمام null reference بررسی شده
- [ ] تمام `ModelState` بررسی شده
- [ ] تمام `ServiceResult` بررسی شده

### SRP
- [ ] Controller: routing و orchestration
- [ ] Service: business logic
- [ ] Repository: data access

### Notifications
- [ ] تمام پیام‌ها با `NotificationHelper`
- [ ] تمام confirmations با SweetAlert2
- [ ] هیچ `alert()` یا `confirm()`

### Security
- [ ] تمام inputs validated
- [ ] تمام forms دارای CSRF protection
- [ ] تمام SQL queries parameterized
- [ ] Authorization کامل

### Audit Trail
- [ ] CreatedAt, CreatedBy
- [ ] UpdatedAt, UpdatedBy
- [ ] DeletedAt, DeletedBy
- [ ] Logging کامل

---

## ⏱️ زمان‌بندی کلی

| Phase | توضیحات | زمان |
|-------|---------|------|
| 1 | Repository Layer | 1 روز |
| 2 | Service Layer | 2 روز |
| 3 | ViewModel Layer | 0.5 روز |
| 4 | Controller | 1 روز |
| 5 | Views | 2 روز |
| 6 | JavaScript | 1 روز |
| 7 | Security & Testing | 1.5 روز |
| 8 | Documentation | 0.5 روز |
| **کل** | | **9.5 روز** |

---

## 🚨 نکات Critical

### ✅ الزامات:
1. ✅ Soft Delete فقط (نه Hard Delete)
2. ✅ Audit Trail کامل
3. ✅ Logging کامل
4. ✅ Validation کامل
5. ✅ Authorization کامل
6. ✅ Mask کردن اطلاعات حساس در Logs

### ❌ ممنوعیت‌ها:
1. ❌ Hard Delete
2. ❌ تغییر بدون Log
3. ❌ تغییر بدون Validation
4. ❌ تغییر بدون Authorization
5. ❌ افشای اطلاعات حساس در Logs
6. ❌ استفاده از `ViewBag`/`ViewData` برای داده‌های اصلی

---

## 📚 مراجع

- `Docs/Knowledge-Base/14041006/USER_MANAGEMENT_MODULE_DEEP_ANALYSIS.md`
- `Docs/Knowledge-Base/CRITICAL-FINANCIAL-MODULE-CONTRACT.md`
- `Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md`
- `Docs/Knowledge-Base/04-TODO-Implementation-Guide.md`
- `Models/Core/ApplicationUser.cs`
- `Models/Core/AppRoles.cs`

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** 1404/10/06  
**وضعیت:** 📋 **TODO List آماده**

