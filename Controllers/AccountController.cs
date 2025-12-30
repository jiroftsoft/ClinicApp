using ClinicApp.Constants;
using ClinicApp.Core;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Security;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Account;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.DataProtection;
using Serilog;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// Account Controller - Login, Registration, Logout
    /// ✅ Most actions are [AllowAnonymous] - only LogOff requires [Authorize]
    /// </summary>
    public class AccountController : Controller
    {
        #region Dependencies & Constructor

        private readonly IAuthService _authService;
        private readonly IPatientService _patientService;
        private readonly ApplicationUserManager _userManager;
        private readonly ILogger _log;
        private readonly ILoginHistoryService _loginHistoryService;
        private readonly IUserProfileService _userProfileService;
        private readonly ICurrentUserService _currentUserService;

        public AccountController(
            IAuthService authService,
            IPatientService patientService,
            ApplicationUserManager userManager,
            ILogger logger,
            ILoginHistoryService loginHistoryService,
            IUserProfileService userProfileService,
            ICurrentUserService currentUserService)
        {
            _authService = authService;
            _patientService = patientService;
            _userManager = userManager;
            _log = logger.ForContext<AccountController>();
            _loginHistoryService = loginHistoryService;
            _userProfileService = userProfileService;
            _currentUserService = currentUserService;
        }

        #endregion

        // -------------------------------------------------------------------
        #region Login & Registration Flow (ورود و ثبت‌نام)
        // -------------------------------------------------------------------

        [AllowAnonymous]
        public ActionResult LoginModal(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_LoginModal");
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (_authService.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CheckUser(CheckNationalCodeViewModel model)
        {
            if (!ModelState.IsValid) return CreateValidationErrorsJson();

            try
            {
                var result = await _authService.CheckUserExistsAsync(model.NationalCode);
                // ✅ Use the generic helper to ensure the 'data' payload is always included
                return CreateServiceResultJson(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "System error in CheckUser for {NationalCode}", model.NationalCode);
                return CreateServiceResultJson(ServiceResult.Failed("A system error occurred.", "SYSTEM_ERROR"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SendLoginOtp(EnterNationalCodeViewModel model)
        {
            if (!ModelState.IsValid) return CreateValidationErrorsJson();

            try
            {
                var result = await _authService.SendLoginOtpAsync(model.NationalCode);
                return CreateServiceResultJson(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "System error in SendLoginOtp for {NationalCode}", model.NationalCode);
                return CreateServiceResultJson(ServiceResult.Failed("A system error occurred.", "SYSTEM_ERROR"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SendRegistrationOtp(SendRegistrationOtpViewModel model)
        {
            if (!ModelState.IsValid) return CreateValidationErrorsJson();

            try
            {
                var result = await _authService.SendRegistrationOtpAsync(model.NationalCode, model.PhoneNumber);
                return CreateServiceResultJson(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "System error in SendRegistrationOtp for {NationalCode}", model.NationalCode);
                return CreateServiceResultJson(ServiceResult.Failed("A system error occurred.", "SYSTEM_ERROR"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> VerifyLoginOtp(VerifyLoginOtpViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return CreateValidationErrorsJson();

            try
            {
                var result = await _authService.VerifyLoginOtpAndSignInAsync(model.NationalCode, model.OtpCode);
                return CreateServiceResultJson(result, result.Success ? GetSafeRedirectUrl(returnUrl) : null);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "System error in VerifyLoginOtp for {NationalCode}", model.NationalCode);
                return CreateServiceResultJson(ServiceResult.Failed("A system error occurred.", "SYSTEM_ERROR"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> VerifyRegistrationOtp(VerifyRegistrationOtpViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return CreateValidationErrorsJson();

            try
            {
                var result = await _authService.VerifyRegistrationOtpAsync(model.NationalCode, model.PhoneNumber, model.OtpCode);

                if (result.Success)
                {
                    var provider = new DpapiDataProtectionProvider("ClinicApp");
                    var dataProtector = provider.Create("RegistrationToken");
                    string payload = $"{model.NationalCode}:{model.PhoneNumber}:{DateTime.UtcNow.AddMinutes(15).Ticks}";
                    byte[] protectedBytes = dataProtector.Protect(Encoding.UTF8.GetBytes(payload));
                    string urlSafeToken = Convert.ToBase64String(protectedBytes);

                    // ✅ Pass returnUrl as query parameter to preserve flow context
                    var completeRegistrationUrl = Url.Action("CompleteRegistration", new { token = urlSafeToken });
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        completeRegistrationUrl += "&returnUrl=" + Uri.EscapeDataString(returnUrl);
                    }

                    return CreateServiceResultJson(result, completeRegistrationUrl);
                }

                return CreateServiceResultJson(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "System error in VerifyRegistrationOtp for {NationalCode}", model.NationalCode);
                return CreateServiceResultJson(ServiceResult.Failed("A system error occurred.", "SYSTEM_ERROR"));
            }
        }

        [AllowAnonymous]
        public ActionResult CompleteRegistration(string token, string returnUrl)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "برای ثبت‌نام، لطفاً فرآیند را از ابتدا شروع کنید.";
                return RedirectToAction("Login");
            }

            try
            {
                var provider = new DpapiDataProtectionProvider("ClinicApp"); // ✅ Fixed: Added 'var' keyword
                var dataProtector = provider.Create("RegistrationToken");
                byte[] protectedBytes = Convert.FromBase64String(token);
                byte[] unprotectedBytes = dataProtector.Unprotect(protectedBytes);
                string payload = Encoding.UTF8.GetString(unprotectedBytes);

                var parts = payload.Split(':');
                if (parts.Length != 3) throw new InvalidOperationException("Payload format is incorrect.");

                var nationalCode = parts[0];
                var phoneNumber = parts[1];
                long expiryTicks = long.Parse(parts[2]);

                if (DateTime.UtcNow.Ticks > expiryTicks)
                {
                    _log.Warning("Expired registration token was used.");
                    TempData["ErrorMessage"] = "The registration link has expired. Please try again.";
                    return RedirectToAction("Login");
                }

                var model = new RegisterPatientViewModel { NationalCode = nationalCode, PhoneNumber = phoneNumber };
                ViewBag.ReturnUrl = returnUrl; // ✅ Set returnUrl for View to preserve flow context
                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Invalid, tampered, or expired registration token was used.");
                TempData["ErrorMessage"] = "The registration link is invalid. Please try again.";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompleteRegistration(RegisterPatientViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var result = await _patientService.RegisterPatientAsync(model, Request.UserHostAddress);
                if (result.Success)
                {
                    await _authService.SignInWithNationalCodeAsync(model.NationalCode);
                    TempData["SuccessMessage"] = "Registration successful! Welcome to Shefa Clinic.";
                    return RedirectToLocal(returnUrl);
                }
                AddErrorsToModelState(result);
                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "System error during final registration for {NationalCode}", model.NationalCode);
                ModelState.AddModelError("", "An unexpected system error occurred. Please contact support.");
                return View(model);
            }
        }

        #endregion

        // -------------------------------------------------------------------
        #region Profile Management (مدیریت پروفایل)
        // -------------------------------------------------------------------

        /// <summary>
        /// نمایش پروفایل کاربر
        /// GET: /Account/Profile
        /// ✅ AJAX-Compatible: پشتیبانی از درخواست‌های AJAX بدون رفرش صفحه
        /// </summary>
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
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = UserProfileConstants.Messages.PleaseLoginAgain, redirectUrl = Url.Action("Login", "Account") }, JsonRequestBehavior.AllowGet);
                    }
                    NotificationHelper.SetError(TempData, UserProfileConstants.Messages.PleaseLoginAgain);
                    return RedirectToAction(UserProfileConstants.Actions.Login);
                }

                var result = await _userProfileService.GetMyProfileAsync(userId);
                if (!result.Success)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = result.Message, redirectUrl = Url.Action("Login", "Account") }, JsonRequestBehavior.AllowGet);
                    }
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction(UserProfileConstants.Actions.Login);
                }

                // ✅ AJAX Request: Return Partial View (بدون Layout)
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_UserProfileComponent", result.Data);
                }

                // ✅ Normal Request: Return Full View (با Layout)
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در نمایش پروفایل");
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = UserProfileConstants.Messages.LoadProfileError }, JsonRequestBehavior.AllowGet);
                }
                NotificationHelper.SetError(TempData, UserProfileConstants.Messages.LoadProfileError);
                return RedirectToAction(UserProfileConstants.Actions.Login);
            }
        }

        /// <summary>
        /// بارگذاری کامپوننت پروفایل به صورت Partial View (Reusable)
        /// GET: /Account/LoadProfileComponent
        /// ✅ Enterprise-Grade: قابل استفاده در Dashboard, Modal, یا هر صفحه‌ای
        /// </summary>
        [HttpGet]
        [Authorize]
        [NoCache]
        public async Task<PartialViewResult> LoadProfileComponent(
            string containerClass = null,
            bool? showHeader = null,
            string formId = null,
            string apiUrl = null,
            string cancelUrl = null,
            string cancelButtonText = null,
            string submitButtonText = null)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return PartialView("_UserProfileComponent", null);
                }

                var result = await _userProfileService.GetMyProfileAsync(userId);
                if (!result.Success)
                {
                    return PartialView("_UserProfileComponent", null);
                }

                // ✅ Configurable via ViewBag
                ViewBag.ContainerClass = containerClass;
                ViewBag.ShowHeader = showHeader;
                ViewBag.FormId = formId;
                ViewBag.ApiUrl = apiUrl;
                ViewBag.CancelUrl = cancelUrl;
                ViewBag.CancelButtonText = cancelButtonText;
                ViewBag.SubmitButtonText = submitButtonText;

                return PartialView("_UserProfileComponent", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بارگذاری کامپوننت پروفایل");
                return PartialView("_UserProfileComponent", null);
            }
        }

        /// <summary>
        /// دریافت اطلاعات پروفایل به صورت JSON (API Endpoint)
        /// GET: /Account/GetProfile
        /// ✅ Enterprise-Grade: API-First Design
        /// </summary>
        [HttpGet]
        [Authorize]
        [NoCache]
        public async Task<JsonResult> GetProfile()
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new
                    {
                        success = false,
                        message = UserProfileConstants.Messages.PleaseLoginAgain,
                        code = UserProfileConstants.ErrorCodes.InvalidUserId
                    }, JsonRequestBehavior.AllowGet);
                }

                var result = await _userProfileService.GetMyProfileAsync(userId);
                if (!result.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        code = result.Code
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    message = "اطلاعات پروفایل با موفقیت دریافت شد.",
                    code = "SUCCESS",
                    data = result.Data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت اطلاعات پروفایل");
                return Json(new
                {
                    success = false,
                    message = UserProfileConstants.Messages.GetProfileError,
                    code = "SYSTEM_ERROR"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// به‌روزرسانی پروفایل کاربر (AJAX - بدون رفرش صفحه)
        /// POST: /Account/Profile
        /// </summary>
        [HttpPost]
        [Authorize]
        [NoCache]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Profile(UserProfileEditViewModel model)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new
                    {
                        success = false,
                        message = UserProfileConstants.Messages.PleaseLoginAgain,
                        code = UserProfileConstants.ErrorCodes.InvalidUserId
                    });
                }

                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => new
                        {
                            field = x.Key,
                            message = e.ErrorMessage
                        }))
                        .ToList();

                    return Json(new
                    {
                        success = false,
                        message = UserProfileConstants.Messages.RequiredFieldsMissing,
                        code = "VALIDATION_ERROR",
                        validationErrors = validationErrors
                    });
                }

                var result = await _userProfileService.UpdateMyProfileAsync(userId, model);
                if (!result.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        code = result.Code
                    });
                }

                // ✅ Reload profile data after successful update
                var updatedProfile = await _userProfileService.GetMyProfileAsync(userId);
                
                return Json(new
                {
                    success = true,
                    message = UserProfileConstants.Messages.ProfileUpdatedSuccessfully,
                    code = "SUCCESS",
                    data = updatedProfile.Success ? updatedProfile.Data : null
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در به‌روزرسانی پروفایل");
                return Json(new
                {
                    success = false,
                    message = UserProfileConstants.Messages.UpdateProfileError,
                    code = "SYSTEM_ERROR"
                });
            }
        }

        #endregion

        // -------------------------------------------------------------------
        #region LogOff & Helpers (خروج و متدهای کمکی)
        // -------------------------------------------------------------------

        [HttpPost]
        [Authorize] // ✅ LogOff requires authentication
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            var userId = User.Identity.GetUserId();
            var sessionId = Session?.SessionID;

            // ثبت تاریخچه خروج (قبل از SignOut)
            if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(sessionId))
            {
                try
                {
                    await _loginHistoryService.LogLogoutAsync(userId, sessionId);
                }
                catch (Exception ex)
                {
                    // Log error but don't prevent logout
                    _log.Warning(ex, "Failed to log logout for UserId: {UserId}", userId);
                }
            }

            _authService.SignOut();
            _log.Information("User {UserId} logged off.", userId);
            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            return Redirect(GetSafeRedirectUrl(returnUrl));
        }

        private string GetSafeRedirectUrl(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }
            
            // ✅ Default redirect: If user is Patient, go to MyAppointments; otherwise Dashboard
            if (User.Identity.IsAuthenticated)
            {
                // Check if user has Patient role
                if (User.IsInRole(AppRoles.Patient))
                {
                    return Url.Action("MyAppointments", "Appointment", new { area = "Patient" });
                }
                // For Admin/Doctor users, go to Dashboard
                return Url.Action("Index", "Dashboard", new { area = "" });
            }
            
            // ✅ For anonymous users, go to home page
            return Url.Action("Index", "Home", new { area = "" });
        }

        private void AddErrorsToModelState(ServiceResult result)
        {
            if (result.ValidationErrors != null && result.ValidationErrors.Any())
            {
                foreach (var error in result.ValidationErrors)
                {
                    ModelState.AddModelError(error.Field ?? "", error.ErrorMessage);
                }
            }
            else if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError("", result.Message);
            }
        }

        private JsonResult CreateServiceResultJson(ServiceResult result, string redirectUrl = null)
        {
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                code = result.Code,
                redirectUrl
            });
        }

        private JsonResult CreateServiceResultJson<T>(ServiceResult<T> result, string redirectUrl = null)
        {
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                code = result.Code,
                redirectUrl,
                data = result.Data
            });
        }

        private JsonResult CreateValidationErrorsJson()
        {
            var errorPayload = ModelState
                .Where(ms => ms.Value.Errors.Any())
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return Json(new
            {
                success = false,
                message = "The provided information is invalid.",
                code = "VALIDATION_ERROR",
                errors = errorPayload
            });
        }

        #endregion
    }
}