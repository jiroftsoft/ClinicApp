using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels; // ViewModels
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Serilog;

namespace ClinicApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        #region Dependencies & Constructor

        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationSignInManager _signInManager;
        private readonly IAuthService _authService;
        private readonly IPatientService _patientService;
        private readonly ILogger _log;

        public AccountController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IAuthService authService,
            IPatientService patientService,
            ILogger logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
            _patientService = patientService;
            _log = logger.ForContext<AccountController>();
        }



        #endregion

        #region OTP Login/Register Flow (جریان اصلی ورود و ثبت‌نام)

        /// <summary>
                /// (GET) نمایش فرم اولیه ورود که فقط شماره موبایل را می‌گیرد
                /// این اکشن می‌تواند به صورت یک Partial View در مودال لود شود
                /// </summary>
                [AllowAnonymous]
                public ActionResult Login(string returnUrl)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    ViewBag.ReturnUrl = returnUrl;

                    // Pass a new ViewModel to the View
                    return View(new EnterPhoneNumberViewModel());
                }

        /// <summary>
        /// (POST) شماره موبایل را دریافت، اعتبارسنجی و کد OTP را ارسال می‌کند
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SendOtp(EnterPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "فرمت شماره موبایل نامعتبر است." });
            }

            var result = await _authService.SendLoginOtpAsync(model.PhoneNumber);

            if (result)
            {
                _log.Information("OTP sent to {PhoneNumber}", model.PhoneNumber);
                return Json(new { success = true, message = "کد تایید با موفقیت ارسال شد." });
            }

            _log.Warning("Failed to send OTP to {PhoneNumber}", model.PhoneNumber);
            return Json(new { success = false, message = "خطا در ارسال پیامک. لطفاً مجدداً تلاش کنید." });
        }

        /// <summary>
        /// (POST) کد OTP را تایید می‌کند.
        /// اگر کاربر وجود داشت، او را وارد می‌کند.
        /// اگر کاربر جدید بود، به مرحله تکمیل ثبت‌نام هدایت می‌کند.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "اطلاعات ورودی نامعتبر است." });
            }

            var signInStatus = await _authService.VerifyLoginOtpAndSignInAsync(model.PhoneNumber, model.OtpCode);

            switch (signInStatus)
            {
                case SignInStatus.Success:
                    _log.Information("User {PhoneNumber} logged in successfully via OTP.", model.PhoneNumber);
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Dashboard") });

                case SignInStatus.RequiresVerification: // به معنی این است که شماره تایید شده ولی کاربر وجود ندارد
                    _log.Information("OTP for {PhoneNumber} verified. Redirecting to complete registration.", model.PhoneNumber);
                    TempData["VerifiedPhoneNumber"] = model.PhoneNumber; // انتقال امن شماره به مرحله بعد
                    return Json(new { success = true, redirectUrl = Url.Action("CompleteRegistration", "Account") });

                default:
                    _log.Warning("Invalid OTP for {PhoneNumber}. Code: {OtpCode}", model.PhoneNumber, model.OtpCode);
                    return Json(new { success = false, message = "کد وارد شده نامعتبر یا منقضی شده است." });
            }
        }

        /// <summary>
        /// (GET) نمایش فرم تکمیل اطلاعات برای کاربر جدید
        /// </summary>
        [AllowAnonymous]
        public ActionResult CompleteRegistration()
        {
            var phoneNumber = TempData["VerifiedPhoneNumber"] as string;
            if (string.IsNullOrEmpty(phoneNumber))
            {
                // اگر کاربر بدون طی کردن مراحل قبلی وارد این صفحه شود، او را به صفحه ورود هدایت می‌کنیم
                return RedirectToAction("Login");
            }

            var model = new RegisterPatientViewModel { PhoneNumber = phoneNumber };
            return View(model);
        }

        /// <summary>
        /// (POST) ثبت نهایی اطلاعات پروفایل بیمار
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompleteRegistration(RegisterPatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // دریافت IP کاربر
            string userIp = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.UserHostAddress;

            var result = await _patientService.RegisterPatientAsync(model, userIp);

            if (result.Succeeded)
            {
                // ورود خودکار کاربر پس از ثبت‌نام موفق
                var user = await _userManager.FindByNameAsync(model.NationalCode);
                await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                _log.Information("کاربر جدید با موفقیت ثبت‌نام و وارد سیستم شد. کد ملی: {NationalCode}", model.NationalCode);

                TempData["SuccessMessage"] = "ثبت‌نام شما با موفقیت انجام شد. به کلینیک شفا خوش آمدید!";
                return RedirectToAction("Index", "Dashboard");
            }

            AddErrors(result);
            return View(model);
        }

        #endregion

        #region LogOff

        /// <summary>
        /// خروج کاربر از سیستم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Helpers
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}