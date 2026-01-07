using ClinicApp.Helpers;
using ClinicApp.Infrastructure;
using ClinicApp.Interfaces.Payment.Web;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Payment.Web;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ClinicApp.Controllers.Payment
{
    /// <summary>
    /// Controller برای صفحه شبیه‌سازی شده درگاه پرداخت
    /// این Controller برای تست و توسعه استفاده می‌شود
    /// </summary>
    [AllowAnonymous] // ✅ برای تست، AllowAnonymous است
    [RoutePrefix("Payment/SimulatedGateway")]
    public class SimulatedGatewayController : Controller
    {
        #region Fields

        private readonly IWebPaymentService _webPaymentService;
        private readonly ILogger _logger;
        private readonly ITimeProvider _timeProvider;

        #endregion

        #region Constructor

        public SimulatedGatewayController(
            IWebPaymentService webPaymentService,
            ILogger logger,
            ITimeProvider timeProvider)
        {
            _webPaymentService = webPaymentService ?? throw new ArgumentNullException(nameof(webPaymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        #endregion

        #region Actions

        /// <summary>
        /// صفحه شبیه‌سازی شده درگاه پرداخت
        /// GET: /Payment/SimulatedGateway/Process?authority=xxx&amount=xxx&callbackUrl=xxx&correlationId=xxx
        /// </summary>
        [HttpGet]
        [Route("Process")]
        public async Task<ActionResult> Process(string authority, decimal? amount, string callbackUrl, string correlationId)
        {
            try
            {
                _logger.Information("🎭 SIMULATED GATEWAY: نمایش صفحه شبیه‌سازی شده - Authority: {Authority}, Amount: {Amount}, CallbackUrl: {CallbackUrl}, CorrelationId: {CorrelationId}",
                    authority, amount, callbackUrl, correlationId);

                if (string.IsNullOrWhiteSpace(authority))
                {
                    _logger.Warning("⚠️ SIMULATED GATEWAY: Authority در QueryString موجود نیست");
                    ViewBag.ErrorMessage = "کد Authority نامعتبر است";
                    return View("~/Views/Shared/Error.cshtml");
                }

                // ✅ دریافت اطلاعات پرداخت از QueryString
                ViewBag.Authority = authority;
                ViewBag.Amount = amount ?? 0;
                ViewBag.CallbackUrl = callbackUrl ?? "/Patient/AppointmentBooking/PaymentCallback";
                ViewBag.CorrelationId = correlationId ?? Guid.NewGuid().ToString("N");
                ViewBag.Description = "پرداخت شبیه‌سازی شده";

                // ✅ CRITICAL FIX: مشخص کردن مسیر View به صورت صریح
                // View در Views/Payment/SimulatedGateway/Process.cshtml است
                return View("~/Views/Payment/SimulatedGateway/Process.cshtml");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SIMULATED GATEWAY: خطا در نمایش صفحه شبیه‌سازی شده");
                ViewBag.ErrorMessage = "خطا در نمایش صفحه درگاه پرداخت";
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        /// <summary>
        /// پردازش پرداخت شبیه‌سازی شده (همیشه موفق)
        /// POST: /Payment/SimulatedGateway/ProcessPayment
        /// </summary>
        [HttpPost]
        [Route("ProcessPayment")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProcessPayment(string authority, string action, string callbackUrl, string correlationId)
        {
            try
            {
                _logger.Information("🎭 SIMULATED GATEWAY: پردازش پرداخت شبیه‌سازی شده - Authority: {Authority}, Action: {Action}, CallbackUrl: {CallbackUrl}, CorrelationId: {CorrelationId}", 
                    authority, action, callbackUrl, correlationId);

                if (string.IsNullOrWhiteSpace(authority))
                {
                    _logger.Warning("⚠️ SIMULATED GATEWAY: Authority در Request موجود نیست");
                    return Json(new { success = false, message = "کد Authority نامعتبر است" });
                }

                // ✅ CRITICAL FIX: تبدیل CallbackUrl absolute به relative اگر دامنه متفاوت باشد
                // این کار برای جلوگیری از redirect به دامنه خارجی (مثلاً mehranyad.ir) است
                var finalCallbackUrl = callbackUrl ?? "/Patient/AppointmentBooking/PaymentCallback";
                
                // ✅ اگر CallbackUrl absolute است و به دامنه دیگری اشاره می‌کند، فقط path را استخراج کن
                string callbackPath = finalCallbackUrl;
                if (Uri.TryCreate(finalCallbackUrl, UriKind.Absolute, out Uri absoluteUri))
                {
                    // ✅ CallbackUrl absolute است - بررسی دامنه
                    var currentHost = Request.Url.Host;
                    var callbackHost = absoluteUri.Host;
                    
                    if (currentHost != callbackHost)
                    {
                        // ✅ دامنه متفاوت است - فقط path را استفاده کن (relative URL)
                        callbackPath = absoluteUri.AbsolutePath;
                        _logger.Information("🔄 SIMULATED GATEWAY: CallbackUrl از absolute به relative تبدیل شد - Original: {OriginalUrl}, Path: {Path}, CurrentHost: {CurrentHost}, CallbackHost: {CallbackHost}, CorrelationId: {CorrelationId}",
                            finalCallbackUrl, callbackPath, currentHost, callbackHost, correlationId);
                    }
                    else
                    {
                        // ✅ دامنه یکسان است - می‌توانیم از absolute URL استفاده کنیم
                        callbackPath = finalCallbackUrl;
                    }
                }
                else
                {
                    // ✅ CallbackUrl از قبل relative است
                    callbackPath = finalCallbackUrl;
                }
                
                // ✅ Build callback URL with query parameters using current request's domain
                string redirectUrl;
                if (action == "cancel")
                {
                    _logger.Information("🎭 SIMULATED GATEWAY: کاربر پرداخت را لغو کرد - Authority: {Authority}, CorrelationId: {CorrelationId}", 
                        authority, correlationId);
                    
                    // ✅ ساخت URL با استفاده از دامنه فعلی
                    var callbackUri = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port, callbackPath);
                    var query = HttpUtility.ParseQueryString(callbackUri.Query);
                    query["Status"] = "NOK";
                    query["Authority"] = authority;
                    if (!string.IsNullOrWhiteSpace(correlationId))
                        query["CorrelationId"] = correlationId;
                    callbackUri.Query = query.ToString();
                    redirectUrl = callbackUri.ToString();
                }
                else
                {
                    // ✅ action = "success" (پرداخت موفق)
                    _logger.Information("✅ SIMULATED GATEWAY: پرداخت شبیه‌سازی شده موفق - Authority: {Authority}, CorrelationId: {CorrelationId}", 
                        authority, correlationId);
                    
                    // ✅ ساخت URL با استفاده از دامنه فعلی
                    var successCallbackUri = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port, callbackPath);
                    var successQuery = HttpUtility.ParseQueryString(successCallbackUri.Query);
                    successQuery["Status"] = "OK";
                    successQuery["Authority"] = authority;
                    if (!string.IsNullOrWhiteSpace(correlationId))
                        successQuery["CorrelationId"] = correlationId;
                    successCallbackUri.Query = successQuery.ToString();
                    redirectUrl = successCallbackUri.ToString();
                }
                
                // ✅ Check if this is an AJAX request - return JSON with redirect URL
                // This allows the client to handle the redirect and bypasses CSP form-action restrictions
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, redirect = redirectUrl });
                }
                
                // ✅ For non-AJAX requests, use server-side redirect
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SIMULATED GATEWAY: خطا در پردازش پرداخت شبیه‌سازی شده");
                return Json(new { success = false, message = "خطا در پردازش پرداخت" });
            }
        }

        #endregion
    }
}

