# 👤 ClinicApp – User Profile Module Implementation Roadmap

## 📋 Preflight Result

### ✅ Contracts Confirmed
- **ServiceResult Enhanced**: Used throughout (Helpers/ServiceResult.cs)
- **Factory Method Pattern**: `FromEntity()` static methods in ViewModels
- **Security Rules**: CSRF (`[ValidateAntiForgeryToken]`), NoCache attribute exists
- **NotificationHelper**: TempData-based notifications (Helpers/NotificationHelper.cs)
- **ICurrentUserService**: Available for getting current user

### ✅ Existing Code Found
- **ApplicationUser Entity**: `Models/Core/ApplicationUser.cs` (FirstName, LastName, Email, PhoneNumber, Gender, Address)
- **NoCache Attribute**: `Controllers/ReceptionV2/ReceptionControllerV2.cs` (lines 237-254)
- **UserManagementService**: Exists but for Admin use only
- **UserCreateEditViewModel**: Exists in `ViewModels/UserManagement/UserManagementViewModels.cs` but includes Roles (not needed for self-edit)

### ⚠️ Gaps Identified
- No `IUserProfileService` for self-profile management
- No `UserProfileEditViewModel` (separate from admin `UserCreateEditViewModel`)
- No Profile actions in `AccountController`
- No `Profile.cshtml` view

---

## 🔍 Discovery Findings

### Existing Reuse
1. **ServiceResult Enhanced**: ✅ Ready to use
2. **Factory Method Pattern**: ✅ Pattern established (`FromEntity()`)
3. **NoCache Filter**: ✅ Exists in `ReceptionV2Controller` (can extract to shared)
4. **NotificationHelper**: ✅ Ready to use
5. **ICurrentUserService**: ✅ Ready to use
6. **ApplicationUserManager**: ✅ Available via DI

### What to Create
1. `IUserProfileService` interface
2. `UserProfileService` implementation
3. `UserProfileEditViewModel` (without Roles, IsActive)
4. `AccountController.Profile()` actions (GET/POST)
5. `Views/Account/Profile.cshtml`
6. Extract `NoCacheAttribute` to shared `Filters/` folder

---

## 🗺️ Module Map

```
User Request
    ↓
[GET] /Account/Profile
    ↓
AccountController.Profile() [Authorize, NoCache]
    ↓
IUserProfileService.GetMyProfileAsync()
    ↓
ApplicationUserManager.FindByIdAsync(currentUserId)
    ↓
UserProfileEditViewModel.FromEntity(user) [Factory Method]
    ↓
View: Profile.cshtml (strongly-typed)
    ↓
User submits form
    ↓
[POST] /Account/Profile
    ↓
AccountController.Profile(model) [Authorize, NoCache, ValidateAntiForgeryToken]
    ↓
IUserProfileService.UpdateMyProfileAsync(model)
    ↓
Validation → ServiceResult
    ↓
ApplicationUserManager.UpdateAsync(user)
    ↓
NotificationHelper.SetSuccess()
    ↓
Redirect to Profile (GET)
```

---

## 📐 Implementation Plan

### Phase 1: Infrastructure
1. Extract `NoCacheAttribute` to `Filters/NoCacheAttribute.cs`
2. Create `IUserProfileService` interface
3. Create `UserProfileService` implementation
4. Register service in `UnityConfig.cs`

### Phase 2: ViewModel & Factory
5. Create `ViewModels/Account/UserProfileEditViewModel.cs` with Factory Method

### Phase 3: Controller
6. Add `Profile()` GET action to `AccountController`
7. Add `Profile()` POST action to `AccountController`

### Phase 4: View
8. Create `Views/Account/Profile.cshtml` (mobile-first, formal UI)

### Phase 5: Integration
9. Add navigation link in layout (if needed)
10. Test end-to-end flow

---

## 🔧 Implementation Details

### 1. NoCacheAttribute (Extract to Shared)

**File**: `Filters/NoCacheAttribute.cs` (NEW)

```csharp
using System;
using System.Web;
using System.Web.Mvc;

namespace ClinicApp.Filters
{
    /// <summary>
    /// Prevents caching of sensitive pages (medical/admin environment)
    /// </summary>
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var response = context.HttpContext.Response;
            
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Expires"] = "0";
            
            base.OnResultExecuting(context);
        }
    }
}
```

### 2. IUserProfileService Interface

**File**: `Interfaces/IUserProfileService.cs` (NEW)

```csharp
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Account;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Service for user self-profile management
    /// </summary>
    public interface IUserProfileService
    {
        Task<ServiceResult<UserProfileEditViewModel>> GetMyProfileAsync(string userId);
        Task<ServiceResult> UpdateMyProfileAsync(string userId, UserProfileEditViewModel model);
    }
}
```

### 3. UserProfileService Implementation

**File**: `Services/UserProfileService.cs` (NEW)

```csharp
using System;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.Account;
using Microsoft.AspNet.Identity;
using Serilog;
using System.Linq;

namespace ClinicApp.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public UserProfileService(
            ApplicationUserManager userManager,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _logger = logger?.ForContext<UserProfileService>();
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResult<UserProfileEditViewModel>> GetMyProfileAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<UserProfileEditViewModel>.Failed("شناسه کاربر معتبر نیست.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    return ServiceResult<UserProfileEditViewModel>.Failed("کاربر یافت نشد.");
                }

                var viewModel = UserProfileEditViewModel.FromEntity(user);
                return ServiceResult<UserProfileEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پروفایل - UserId: {UserId}", userId);
                return ServiceResult<UserProfileEditViewModel>.Failed("خطا در دریافت اطلاعات پروفایل.");
            }
        }

        public async Task<ServiceResult> UpdateMyProfileAsync(string userId, UserProfileEditViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult.Failed("شناسه کاربر معتبر نیست.");
                }

                // ✅ Security: Ensure user can only update their own profile
                if (userId != _currentUserService.UserId)
                {
                    _logger.Warning("تلاش برای ویرایش پروفایل کاربر دیگر - RequestedUserId: {RequestedUserId}, CurrentUserId: {CurrentUserId}",
                        userId, _currentUserService.UserId);
                    return ServiceResult.Failed("شما فقط می‌توانید پروفایل خود را ویرایش کنید.", "UNAUTHORIZED");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    return ServiceResult.Failed("کاربر یافت نشد.");
                }

                // ✅ Update only allowed fields (no NationalCode, no Roles, no IsActive)
                user.FirstName = model.FirstName?.Trim();
                user.LastName = model.LastName?.Trim();
                user.Email = model.Email?.Trim();
                user.Gender = model.Gender;
                user.Address = model.Address?.Trim();
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedByUserId = userId;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    _logger.Warning("خطا در به‌روزرسانی پروفایل - UserId: {UserId}, Errors: {Errors}", userId, errors);
                    return ServiceResult.Failed("خطا در به‌روزرسانی پروفایل: " + errors);
                }

                _logger.Information("پروفایل با موفقیت به‌روزرسانی شد - UserId: {UserId}", userId);
                return ServiceResult.Successful("پروفایل با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی پروفایل - UserId: {UserId}", userId);
                return ServiceResult.Failed("خطا در به‌روزرسانی پروفایل.");
            }
        }
    }
}
```

### 4. UserProfileEditViewModel

**File**: `ViewModels/Account/UserProfileEditViewModel.cs` (NEW)

```csharp
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Account
{
    /// <summary>
    /// ViewModel for user self-profile editing
    /// Excludes: NationalCode (immutable), Roles, IsActive (admin-only)
    /// </summary>
    public class UserProfileEditViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(100, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(100, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string LastName { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; } // Read-only, for display only

        [Display(Name = "ایمیل")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [EmailAddress(ErrorMessage = "فرمت {0} معتبر نیست.")]
        [MaxLength(256, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string Email { get; set; }

        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; } // Read-only (requires OTP verification to change)

        [Display(Name = "جنسیت")]
        [Required(ErrorMessage = "انتخاب {0} الزامی است.")]
        public Gender Gender { get; set; }

        [Display(Name = "آدرس")]
        [MaxLength(500, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        /// <summary>
        /// ✅ Factory Method: Create ViewModel from Entity
        /// </summary>
        public static UserProfileEditViewModel FromEntity(ApplicationUser user)
        {
            if (user == null) return null;

            return new UserProfileEditViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                NationalCode = user.NationalCode,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                Address = user.Address
            };
        }
    }
}
```

### 5. AccountController Actions

**File**: `Controllers/AccountController.cs` (MODIFY)

Add after `CompleteRegistration` section:

```csharp
// -------------------------------------------------------------------
#region Profile Management (مدیریت پروفایل)
// -------------------------------------------------------------------

[HttpGet]
[Authorize]
[NoCache]
public async Task<ActionResult> Profile()
{
    try
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            NotificationHelper.SetError(TempData, "لطفاً دوباره وارد شوید.");
            return RedirectToAction("Login");
        }

        var result = await _userProfileService.GetMyProfileAsync(userId);
        if (!result.Success)
        {
            NotificationHelper.SetError(TempData, result.Message);
            return RedirectToAction("Login");
        }

        return View(result.Data);
    }
    catch (Exception ex)
    {
        _log.Error(ex, "خطا در نمایش پروفایل");
        NotificationHelper.SetError(TempData, "خطا در بارگذاری پروفایل");
        return RedirectToAction("Login");
    }
}

[HttpPost]
[Authorize]
[NoCache]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Profile(UserProfileEditViewModel model)
{
    try
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            NotificationHelper.SetError(TempData, "لطفاً دوباره وارد شوید.");
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
            return View(model);
        }

        var result = await _userProfileService.UpdateMyProfileAsync(userId, model);
        if (!result.Success)
        {
            NotificationHelper.SetError(TempData, result.Message);
            return View(model);
        }

        NotificationHelper.SetSuccess(TempData, "پروفایل با موفقیت به‌روزرسانی شد.");
        return RedirectToAction("Profile");
    }
    catch (Exception ex)
    {
        _log.Error(ex, "خطا در به‌روزرسانی پروفایل");
        NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی پروفایل");
        return View(model);
    }
}

#endregion
```

### 6. Profile View

**File**: `Views/Account/Profile.cshtml` (NEW)

Mobile-first, formal, healthcare UI.

---

## 📊 ServiceResult Examples

### Success
```json
{
  "success": true,
  "message": "پروفایل با موفقیت به‌روزرسانی شد.",
  "code": "SUCCESS"
}
```

### Validation Error
```json
{
  "success": false,
  "message": "لطفاً تمام فیلدهای الزامی را پر کنید.",
  "code": "VALIDATION_ERROR",
  "validationErrors": [
    {
      "field": "Email",
      "errorMessage": "فرمت ایمیل معتبر نیست."
    }
  ]
}
```

---

## ✅ Tests & Verification

### Manual Checklist
- [ ] Authenticated user can access `/Account/Profile`
- [ ] Unauthenticated user redirected to Login
- [ ] Profile form displays current user data
- [ ] Valid update succeeds with success notification
- [ ] Invalid input shows validation errors
- [ ] CSRF token required (missing token = error)
- [ ] Cache headers: `Cache-Control: no-cache, no-store`
- [ ] User cannot edit another user's profile
- [ ] NationalCode and PhoneNumber are read-only

---

## 🔄 Rollback

1. Revert commit: `git revert <commit-hash>`
2. Remove files:
   - `Filters/NoCacheAttribute.cs`
   - `Interfaces/IUserProfileService.cs`
   - `Services/UserProfileService.cs`
   - `ViewModels/Account/UserProfileEditViewModel.cs`
   - `Views/Account/Profile.cshtml`
3. Remove UnityConfig registration
4. Remove AccountController actions

---

## ❓ Open Questions

None (all requirements clear from contracts).

