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
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security.DataProtection;
using Serilog;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Services;

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
        private readonly AsanakSmsService _smsService;

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
            _smsService = new AsanakSmsService(); // ✅ Initialize SMS service for welcome message
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
                // ✅ CRITICAL FIX: جلوگیری از redirect loop
                // اگر returnUrl مربوط به Patient Area است، باید چک کنیم که کاربر نقش Patient دارد
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/Patient/", StringComparison.OrdinalIgnoreCase))
                {
                    if (!User.IsInRole(AppRoles.Patient))
                    {
                        _log.Warning("⚠️ [Login] کاربر authenticate شده اما نقش Patient ندارد - redirect به صفحهٔ پیش‌فرض. UserId: {UserId}, ReturnUrl: {ReturnUrl}",
                            User.Identity.GetUserId(), returnUrl);
                        NotificationHelper.SetError(TempData,
                            "شما مجوز دسترسی به بخش بیمار را ندارید. لطفاً با حساب کاربری بیمار وارد شوید.");
                        return Redirect(GetDefaultLandingUrl());
                    }
                }

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
                _log.Error(ex, "System error in CheckUser for NationalCode: {MaskedNC}", MaskHelper.MaskNationalCode(model.NationalCode));
                return CreateServiceResultJson(ServiceResult.Failed("A system error occurred.", "SYSTEM_ERROR"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SendLoginOtp(EnterNationalCodeViewModel model)
        {
            _log.Information("📨 [Controller.SendLoginOtp] START - MaskedNC: {MaskedNC}, IsAjax: {IsAjax}, IP: {IP}",
                MaskHelper.MaskNationalCode(model?.NationalCode),
                Request.IsAjaxRequest(),
                Request.UserHostAddress);

            if (!ModelState.IsValid)
            {
                _log.Warning("[Controller.SendLoginOtp] ModelState INVALID - Errors: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return CreateValidationErrorsJson();
            }

            try
            {
                var result = await _authService.SendLoginOtpAsync(model.NationalCode);
                
                _log.Information("[Controller.SendLoginOtp] Service returned - Success: {Success}, Code: {Code}",
                    result.Success, result.Code);
                
                return CreateServiceResultJson(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ [Controller.SendLoginOtp] EXCEPTION - MaskedNC: {MaskedNC}, ExceptionType: {ExceptionType}",
                    MaskHelper.MaskNationalCode(model.NationalCode), ex.GetType().Name);
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
                _log.Error(ex, "System error in SendRegistrationOtp for NationalCode: {MaskedNC}", MaskHelper.MaskNationalCode(model.NationalCode));
                return CreateServiceResultJson(ServiceResult.Failed("A system error occurred.", "SYSTEM_ERROR"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyLoginOtp(VerifyLoginOtpViewModel model, string returnUrl)
        {
            var isAjaxRequest = IsAjaxRequestEnhanced();  // ✅ Use enhanced detection
            
            // ✅ SECURITY: Log with masked sensitive data
            _log.Information("VerifyLoginOtp START - NationalCode: {MaskedNC}, OtpCode Length: {OtpLength}, IsAjax: {IsAjax}",
                MaskHelper.MaskNationalCode(model?.NationalCode),
                model?.OtpCode?.Length ?? 0,
                isAjaxRequest);
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _log.Warning("ModelState invalid - Errors: {Errors}", string.Join(", ", errors));
                
                if (isAjaxRequest)
                    return CreateValidationErrorsJson();
                
                TempData["ErrorMessage"] = "اطلاعات وارد شده معتبر نیست.";
                return RedirectToAction("Login", new { returnUrl });
            }

            try
            {
                var result = await _authService.VerifyLoginOtpAndSignInAsync(model.NationalCode, model.OtpCode);
                
                if (result.Success)
                {
                    var redirectUrl = GetSafeRedirectUrl(returnUrl);
                    _log.Information("✅ Login successful - redirecting to: {RedirectUrl}", redirectUrl);
                    
                    // ✅ Server-side redirect for normal form submission (fixes Cookie Timing Issue)
                    if (!isAjaxRequest)
                    {
                        return RedirectToLocal(redirectUrl);
                    }
                    
                    // ✅ JSON response for AJAX requests
                    return CreateServiceResultJson(result, redirectUrl);
                }
                else
                {
                    _log.Warning("❌ Login failed - Code: {Code}, Message: {Message}", result.Code, result.Message);
                    
                    // ✅ OPTIMIZATION: Always return JSON for OTP verification (even if not detected as AJAX)
                    // This prevents redirect to Login page when OTP is wrong, keeping user in modal
                    // The frontend now always uses AJAX for both login and registration flows
                    return CreateServiceResultJson(result);
                    
                    // ✅ OLD CODE: Removed to prevent redirect to Login page
                    // if (!isAjaxRequest)
                    // {
                    //     TempData["ErrorMessage"] = result.Message;
                    //     return RedirectToAction("Login", new { returnUrl });
                    // }
                    // return CreateServiceResultJson(result);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ System error in VerifyLoginOtp for NationalCode: {MaskedNC}", 
                    MaskHelper.MaskNationalCode(model.NationalCode));
                
                if (!isAjaxRequest)
                {
                    TempData["ErrorMessage"] = "خطای سیستمی رخ داد. لطفاً دوباره تلاش کنید.";
                    return RedirectToAction("Login", new { returnUrl });
                }
                
                return CreateServiceResultJson(ServiceResult.Failed("خطای سیستمی رخ داد.", "SYSTEM_ERROR"));
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
                    TempData["ErrorMessage"] = "لینک ثبت‌نام منقضی شده است. لطفاً از ابتدا فرآیند ثبت‌نام را انجام دهید.";
                    return RedirectToAction("Login");
                }

                var model = new RegisterPatientViewModel { NationalCode = nationalCode, PhoneNumber = phoneNumber };
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Invalid, tampered, or expired registration token was used.");
                TempData["ErrorMessage"] = "لینک ثبت‌نام نامعتبر یا منقضی است. لطفاً از ابتدا فرآیند ثبت‌نام را انجام دهید.";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompleteRegistration(RegisterPatientViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            try
            {
                var result = await _patientService.RegisterPatientAsync(model, Request.UserHostAddress);
                if (result.Success)
                {
                    await _authService.SignInWithNationalCodeAsync(model.NationalCode);
                    
                    // ✅ OPTIMIZATION: فارسی کردن پیام موفقیت
                    TempData["SuccessMessage"] = "ثبت‌نام با موفقیت انجام شد. به کلینیک درمانی شفا خوش آمدید.";
                    
                    // ✅ OPTIMIZATION: ارسال SMS خوش‌آمدگویی بعد از ثبت‌نام موفق
                    try
                    {
                        var welcomeMessage = new IdentityMessage
                        {
                            Destination = model.PhoneNumber,
                            Body = "به کلینیک درمانی شفا خوش آمدید. ثبت‌نام شما با موفقیت انجام شد."
                        };
                        
                        // ✅ ارسال SMS به صورت Async (بدون انتظار برای پاسخ)
                        _ = _smsService.SendAsync(welcomeMessage).ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                            {
                                _log.Warning(task.Exception, "خطا در ارسال SMS خوش‌آمدگویی به شماره {PhoneNumber}", 
                                    MaskHelper.MaskPhoneNumber(model.PhoneNumber));
                            }
                            else
                            {
                                _log.Information("SMS خوش‌آمدگویی با موفقیت ارسال شد به شماره {PhoneNumber}", 
                                    MaskHelper.MaskPhoneNumber(model.PhoneNumber));
                            }
                        }, TaskContinuationOptions.ExecuteSynchronously);
                    }
                    catch (Exception smsEx)
                    {
                        // ✅ خطای SMS نباید ثبت‌نام را متوقف کند
                        _log.Warning(smsEx, "خطا در ارسال SMS خوش‌آمدگویی (ثبت‌نام موفق بود) - شماره: {PhoneNumber}", 
                            MaskHelper.MaskPhoneNumber(model.PhoneNumber));
                    }
                    
                    return RedirectToLocal(returnUrl);
                }
                AddErrorsToModelState(result);
                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "System error during final registration for NationalCode: {MaskedNC}", MaskHelper.MaskNationalCode(model.NationalCode));
                ModelState.AddModelError("", "خطای سیستمی رخ داده است. لطفاً دوباره تلاش کنید یا با پشتیبانی کلینیک تماس بگیرید.");
                return View(model);
            }
        }

        #endregion

        // -------------------------------------------------------------------
        #region Profile Management (مدیریت پروفایل)
        // -------------------------------------------------------------------

        /// <summary>
        /// ✅ BULLETPROOF: Enhanced AJAX request detection
        /// Checks multiple sources: Request.IsAjaxRequest() + Custom Header + Query String
        /// Healthcare-Grade: Must work across all ASP.NET configurations
        /// </summary>
        private bool IsAjaxRequestEnhanced()
        {
            // Check standard ASP.NET method
            if (Request.IsAjaxRequest())
                return true;
            
            // Check custom header (added by user-profile-menu.js)
            if (Request.Headers["X-AJAX-Request"] == "true")
                return true;
            
            // Check query parameter as final fallback
            if (Request.QueryString["ajax"] == "1")
                return true;
            
            return false;
        }

        /// <summary>
        /// نمایش پروفایل کاربر
        /// GET: /Account/Profile
        /// ✅ AJAX-Compatible: پشتیبانی از درخواست‌های AJAX بدون رفرش صفحه
        /// ✅ BULLETPROOF: Enhanced AJAX detection to prevent layout duplication
        /// </summary>
        [HttpGet]
        [Authorize]
        [NoCache]
        public async Task<ActionResult> Profile()
        {
            try
            {
                // ✅ CRITICAL: Log authentication state for debugging
                _log.Information("🔍 Profile GET - Request.IsAuthenticated: {IsAuth}, User.Identity.IsAuthenticated: {UserAuth}, IsAjax: {IsAjax}",
                    Request.IsAuthenticated,
                    User?.Identity?.IsAuthenticated ?? false,
                    IsAjaxRequestEnhanced());

                // ✅ STANDARD: Use User.Identity.GetUserId() directly (not CurrentUserService)
                var userId = User.Identity.GetUserId();
                
                // ✅ PRODUCTION: Proper unauthorized handling
                if (string.IsNullOrEmpty(userId))
                {
                    _log.Warning("⚠️ Profile access denied - UserId is null. User: {UserName}, IsAuthenticated: {IsAuth}",
                        User?.Identity?.Name ?? "NULL",
                        User?.Identity?.IsAuthenticated ?? false);

                    if (IsAjaxRequestEnhanced())
                    {
                        // ✅ Return 401 status for AJAX requests
                        Response.StatusCode = 401;
                        return Json(new
                        {
                            success = false,
                            message = UserProfileConstants.Messages.PleaseLoginAgain,
                            code = "UNAUTHORIZED",
                            redirectUrl = Url.Action("Login", "Account")
                        }, JsonRequestBehavior.AllowGet);
                    }
                    
                    NotificationHelper.SetError(TempData, UserProfileConstants.Messages.PleaseLoginAgain);
                    return RedirectToAction(UserProfileConstants.Actions.Login);
                }

                var result = await _userProfileService.GetMyProfileAsync(userId);
                if (!result.Success)
                {
                    if (IsAjaxRequestEnhanced())
                    {
                        return Json(new { success = false, message = result.Message, redirectUrl = Url.Action("Login", "Account") }, JsonRequestBehavior.AllowGet);
                    }
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction(UserProfileConstants.Actions.Login);
                }

                // ✅ AJAX Request: Return Partial View (بدون Layout) - CRITICAL for preventing layout duplication
                if (IsAjaxRequestEnhanced())
                {
                    _log.Information("✅ Returning PartialView for AJAX request");
                    return PartialView("_UserProfileComponent", result.Data);
                }

                // ✅ Normal Request: Return Full View (با Layout)
                _log.Information("✅ Returning full View for normal request");
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در نمایش پروفایل");
                if (IsAjaxRequestEnhanced())
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
                // ✅ CRITICAL: Log authentication state
                _log.Information("🔍 Profile POST - Request.IsAuthenticated: {IsAuth}, User.Identity.IsAuthenticated: {UserAuth}",
                    Request.IsAuthenticated,
                    User?.Identity?.IsAuthenticated ?? false);

                var userId = _currentUserService.UserId;
                
                // ✅ PRODUCTION: Proper unauthorized handling
                if (string.IsNullOrEmpty(userId))
                {
                    _log.Warning("⚠️ Profile update denied - UserId is null");
                    
                    Response.StatusCode = 401;
                    return Json(new
                    {
                        success = false,
                        message = UserProfileConstants.Messages.PleaseLoginAgain,
                        code = "UNAUTHORIZED",
                        redirectUrl = Url.Action("Login", "Account")
                    });
                }

                if (!ModelState.IsValid)
                {
                    return CreateValidationErrorsJson();
                }

                var result = await _userProfileService.UpdateMyProfileAsync(userId, model);
                if (!result.Success)
                {
                    return CreateServiceResultJson(result);
                }

                // ✅ Reload profile data after successful update
                var updatedProfile = await _userProfileService.GetMyProfileAsync(userId);
                
                return CreateServiceResultJson(updatedProfile);
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

            // ✅ خروج با OWIN context همین درخواست تا کوکی در همین پاسخ باطل شود (جلوگیری از نمایش لاگین در صفحه اصلی)
            try
            {
                var owinCtx = HttpContext?.GetOwinContext();
                if (owinCtx?.Authentication != null)
                {
                    owinCtx.Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    owinCtx.Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                }
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "OWIN SignOut failed, falling back to AuthService. UserId: {UserId}", userId);
            }
            _authService.SignOut();
            _log.Information("User {UserId} logged off.", userId);
            // ✅ جلوگیری از کش شدن صفحه اصلی بعد از ریدایرکت
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.AppendHeader("Pragma", "no-cache");
            Response.AppendHeader("Expires", "0");
            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            return Redirect(GetSafeRedirectUrl(returnUrl));
        }

        /// <summary>
        /// تعیین صفحهٔ پیش‌فرض پس از لاگین بر اساس نقش کاربر (مدیر/منشی → پنل ادمین، بیمار/سایر → صفحهٔ اصلی).
        /// </summary>
        private string GetDefaultLandingUrl()
        {
            if (_authService.IsAuthenticated &&
                (User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Receptionist)))
            {
                return Url.Action("Index", "DoctorDashboard", new { area = "Admin" });
            }
            return Url.Action("Index", "Home", new { area = "" });
        }

        private string GetSafeRedirectUrl(string returnUrl)
        {
            // ✅ اگر returnUrl خالی است، بر اساس نقش کاربر به پنل ادمین یا صفحهٔ اصلی
            if (string.IsNullOrEmpty(returnUrl))
            {
                return GetDefaultLandingUrl();
            }

            // ✅ Decode URL-encoded returnUrl (مثل http%3A%2F%2Flocalhost%3A3560%2F)
            try
            {
                returnUrl = Uri.UnescapeDataString(returnUrl);
            }
            catch
            {
                // اگر decode ناموفق بود، از returnUrl اصلی استفاده می‌کنیم
            }

            // ✅ اگر returnUrl یک URL کامل است (مثل http://localhost:3560/)، path را استخراج می‌کنیم
            if (Uri.TryCreate(returnUrl, UriKind.Absolute, out Uri absoluteUri))
            {
                // ✅ بررسی اینکه آیا URL مربوط به همین host است
                var currentHost = Request.Url?.Host ?? "";
                if (absoluteUri.Host.Equals(currentHost, StringComparison.OrdinalIgnoreCase) ||
                    absoluteUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    // ✅ استفاده از PathAndQuery (مثل / یا /Patient/Appointment/Book/SelectDate/2)
                    returnUrl = absoluteUri.PathAndQuery;
                }
                else
                {
                    _log.Warning("⚠️ [GetSafeRedirectUrl] External URL detected - using default landing. ReturnUrl: {ReturnUrl}", returnUrl);
                    return GetDefaultLandingUrl();
                }
            }

            if (Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            _log.Warning("⚠️ [GetSafeRedirectUrl] Invalid returnUrl - using default landing. ReturnUrl: {ReturnUrl}", returnUrl);
            return GetDefaultLandingUrl();
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