using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
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
    [Authorize]
    public class AccountController : Controller
    {
        #region Dependencies & Constructor

        private readonly IAuthService _authService;
        private readonly IPatientService _patientService;
        private readonly ApplicationUserManager _userManager;
        private readonly ILogger _log;

        public AccountController(
            IAuthService authService,
            IPatientService patientService,
            ApplicationUserManager userManager,
            ILogger logger)
        {
            _authService = authService;
            _patientService = patientService;
            _userManager = userManager;
            _log = logger.ForContext<AccountController>();
        }

        #endregion

        // -------------------------------------------------------------------
        #region Login & Registration Flow (ورود و ثبت‌نام)
        // -------------------------------------------------------------------

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
        public async Task<JsonResult> VerifyRegistrationOtp(VerifyRegistrationOtpViewModel model)
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

                    return CreateServiceResultJson(result, Url.Action("CompleteRegistration", new { token = urlSafeToken }));
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
        public ActionResult CompleteRegistration(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "برای ثبت‌نام، لطفاً فرآیند را از ابتدا شروع کنید.";
                return RedirectToAction("Login");
            }

            try
            {
                var provider = new DpapiDataProtectionProvider("ClinicApp");
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
        #region LogOff & Helpers (خروج و متدهای کمکی)
        // -------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            var userId = User.Identity.GetUserId();
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
            return Url.Action("Index", "Dashboard", new { area = "" });
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