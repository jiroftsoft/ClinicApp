using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.Patient;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// Controller برای مدیریت تنظیمات بیمار
    /// Single Responsibility: مدیریت تنظیمات حساب، اعلان‌ها، و حریم خصوصی
    /// 
    /// ✅ Enterprise-Grade: AJAX-Compatible, Authorization, Clean Architecture
    /// طبق: DEVELOPMENT_CONTRACT.md
    /// </summary>
    [Authorize(Roles = "Patient")]
    [NoCache]
    public class SettingsController : BasePatientController
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IPatientSettingsService _settingsService;

        public SettingsController(
            ILogger logger,
            ICurrentUserService currentUserService,
            IPatientSettingsService settingsService)
            : base(logger, currentUserService)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        /// <summary>
        /// صفحه اصلی تنظیمات
        /// GET: /Patient/Settings
        /// ✅ Tab-based navigation: Account, Notifications, Privacy, Security
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(string tab = "account")
        {
            try
            {
                var userId = User.Identity.GetUserId();
                _logger.Information("درخواست نمایش تنظیمات - UserId: {UserId}, Tab: {Tab}", userId, tab);

                // ✅ Security: دریافت patientId از auth context
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    _logger.Warning("⚠️ Settings access denied - patientId is null. UserId: {UserId}", userId);
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید.");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ Set active tab
                ViewBag.ActiveTab = tab.ToLower();

                return View();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش تنظیمات");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری تنظیمات");
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// تنظیمات حساب کاربری
        /// GET: /Patient/Settings/Account
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Account()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                _logger.Information("درخواست نمایش تنظیمات حساب - UserId: {UserId}", userId);

                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد.");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ Redirect to main settings with account tab
                return RedirectToAction("Index", new { tab = "account" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش تنظیمات حساب");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری تنظیمات حساب");
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// تنظیمات اعلان‌ها
        /// GET: /Patient/Settings/Notifications
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Notifications()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                _logger.Information("درخواست نمایش تنظیمات اعلان‌ها - UserId: {UserId}", userId);

                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد.");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ Redirect to main settings with notifications tab
                return RedirectToAction("Index", new { tab = "notifications" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش تنظیمات اعلان‌ها");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری تنظیمات اعلان‌ها");
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// تنظیمات حریم خصوصی
        /// GET: /Patient/Settings/Privacy
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Privacy()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                _logger.Information("درخواست نمایش تنظیمات حریم خصوصی - UserId: {UserId}", userId);

                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد.");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ Redirect to main settings with privacy tab
                return RedirectToAction("Index", new { tab = "privacy" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش تنظیمات حریم خصوصی");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری تنظیمات حریم خصوصی");
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// تنظیمات امنیتی
        /// GET: /Patient/Settings/Security
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Security()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                _logger.Information("درخواست نمایش تنظیمات امنیتی - UserId: {UserId}", userId);

                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد.");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ Redirect to main settings with security tab
                return RedirectToAction("Index", new { tab = "security" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش تنظیمات امنیتی");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری تنظیمات امنیتی");
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// به‌روزرسانی تنظیمات اعلان‌ها
        /// POST: /Patient/Settings/UpdateNotifications
        /// ✅ طبق DEVELOPMENT_CONTRACT.md - ServiceResult Enhanced
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateNotifications(bool emailNotifications, bool smsNotifications, bool appointmentReminders)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                _logger.Information("درخواست به‌روزرسانی تنظیمات اعلان‌ها - UserId: {UserId}", userId);

                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد.");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ استفاده از Service Layer
                var dto = new NotificationSettingsDto
                {
                    EmailNotifications = emailNotifications,
                    SmsNotifications = smsNotifications,
                    AppointmentReminders = appointmentReminders
                };

                var result = await _settingsService.UpdateNotificationSettingsAsync(patientId.Value, dto);

                if (result.Success)
                {
                    _logger.Information("تنظیمات اعلان‌ها به‌روزرسانی شد - PatientId: {PatientId}", patientId);
                    NotificationHelper.SetSuccess(TempData, result.Message);
                }
                else
                {
                    _logger.Warning("خطا در به‌روزرسانی تنظیمات - PatientId: {PatientId}, Error: {Error}", patientId, result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                }

                // ✅ AJAX: بازگشت JSON برای تب تنظیمات داشبورد (بدون رفرش)
                if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = result.Success, message = result.Message }, JsonRequestBehavior.DenyGet);
                }

                return RedirectToAction("Index", new { tab = "notifications" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تنظیمات اعلان‌ها");
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی تنظیمات اعلان‌ها");
                if (Request.IsAjaxRequest() || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "خطا در به‌روزرسانی تنظیمات اعلان‌ها" }, JsonRequestBehavior.DenyGet);
                }
                return RedirectToAction("Index", new { tab = "notifications" });
            }
        }
    }
}

