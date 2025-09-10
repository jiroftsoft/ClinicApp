using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر تست توکن امنیتی KeyA
    /// این کنترلر برای بررسی ارتباط با توکن سخت‌افزاری طراحی شده است
    /// </summary>
    public class TokenTestController : Controller
    {
        private readonly ISecurityTokenService _tokenService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public TokenTestController(
            ISecurityTokenService tokenService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _tokenService = tokenService;
            _log = logger.ForContext<TokenTestController>();
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// صفحه اصلی تست توکن
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            _log.Information("بازدید از صفحه تست توکن. کاربر: {UserName} (شناسه: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            return View();
        }

        /// <summary>
        /// بررسی وجود توکن امنیتی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CheckTokenPresence(string tokenId = "TR127256")
        {
            try
            {
                _log.Information(
                    "درخواست بررسی وجود توکن. شناسه: {TokenId}. کاربر: {UserName} (شناسه: {UserId})",
                    tokenId, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _tokenService.CheckTokenPresenceAsync(tokenId);

                if (result.Success)
                {
                    var tokenStatus = result.Data as dynamic;
                    _log.Information(
                        "بررسی توکن موفق. وضعیت: {Status}. کاربر: {UserName} (شناسه: {UserId})",
                        tokenStatus?.Status, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        data = new
                        {
                            tokenId = tokenStatus?.TokenId,
                            isConnected = tokenStatus?.IsConnected,
                            isValid = tokenStatus?.IsValid,
                            status = tokenStatus?.Status,
                            deviceInfo = tokenStatus?.DeviceInfo,
                            errorMessage = tokenStatus?.ErrorMessage,
                            checkTime = tokenStatus?.CheckTime?.ToString("yyyy-MM-dd HH:mm:ss")
                        }
                    });
                }
                else
                {
                    _log.Warning(
                        "بررسی توکن ناموفق. خطا: {Error}. کاربر: {UserName} (شناسه: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        error = "TOKEN_CHECK_ERROR"
                    });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در بررسی توکن. شناسه: {TokenId}. کاربر: {UserName} (شناسه: {UserId})",
                    tokenId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطای سیستمی در بررسی توکن",
                    error = "SYSTEM_ERROR"
                });
            }
        }

        /// <summary>
        /// تست کامل ارتباط با توکن
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> TestTokenConnection(string tokenId = "TR127256")
        {
            try
            {
                _log.Information(
                    "درخواست تست کامل توکن. شناسه: {TokenId}. کاربر: {UserName} (شناسه: {UserId})",
                    tokenId, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _tokenService.TestTokenConnectionAsync(tokenId);

                if (result.Success)
                {
                    var testResult = result.Data as dynamic;
                    _log.Information(
                        "تست توکن تکمیل شد. نتیجه: {Status}. کاربر: {UserName} (شناسه: {UserId})",
                        testResult?.OverallStatus, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        data = new
                        {
                            tokenId = testResult?.TokenId,
                            isSuccessful = testResult?.IsSuccessful,
                            overallStatus = testResult?.OverallStatus,
                            testTime = testResult?.TestTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                            tests = testResult?.Tests
                        }
                    });
                }
                else
                {
                    _log.Warning(
                        "تست توکن ناموفق. خطا: {Error}. کاربر: {UserName} (شناسه: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        error = "TOKEN_TEST_ERROR"
                    });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در تست توکن. شناسه: {TokenId}. کاربر: {UserName} (شناسه: {UserId})",
                    tokenId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطای سیستمی در تست توکن",
                    error = "SYSTEM_ERROR"
                });
            }
        }

        /// <summary>
        /// دریافت لیست دستگاه‌های USB
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetUsbDevices()
        {
            try
            {
                _log.Information("درخواست لیست دستگاه‌های USB. کاربر: {UserName} (شناسه: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // این متد می‌تواند در SecurityTokenService پیاده‌سازی شود
                // فعلاً شبیه‌سازی می‌کنیم
                await Task.Delay(1000);

                var devices = new[]
                {
                    "KeyA Security Token (TR127256)",
                    "USB Mass Storage Device",
                    "USB Composite Device",
                    "USB Root Hub"
                };

                _log.Information("لیست دستگاه‌های USB ارسال شد. تعداد: {Count}",
                    devices.Length);

                return Json(new
                {
                    success = true,
                    message = "لیست دستگاه‌های USB دریافت شد",
                    data = devices
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت لیست دستگاه‌های USB");
                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت لیست دستگاه‌های USB",
                    error = "USB_DEVICES_ERROR"
                });
            }
        }
    }
}
