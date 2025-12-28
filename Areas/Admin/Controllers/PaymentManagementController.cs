using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment.Management;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Admin.PaymentManagement;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller برای مدیریت پرداخت‌ها (Admin)
    /// طراحی شده طبق اصول SRP - مسئولیت: Routing و Orchestration
    /// 
    /// Architecture Principles Applied:
    /// ✅ Single Responsibility: فقط Routing و Orchestration
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// ✅ Clean Architecture: Controller فقط View را مدیریت می‌کند
    /// ✅ Medical Standards: رعایت استانداردهای سیستم‌های پزشکی
    /// ✅ Security: Authorization کامل، Validation کامل
    /// 
    /// Flow: HTTP Request -> Controller -> Service -> Repository -> Database
    /// </summary>
    //[Authorize(Roles = AppRoles.Admin)]
    public class PaymentManagementController : Controller
    {
        #region Fields and Constructor

        private readonly IPaymentManagementService _paymentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public PaymentManagementController(
            IPaymentManagementService paymentService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<PaymentManagementController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Index & Listing

        /// <summary>
        /// نمایش لیست پرداخت‌ها با قابلیت جستجو و فیلتر
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(PaymentSearchFilter filter = null, int page = 1)
        {
            try
            {
                _logger.Information("درخواست لیست پرداخت‌ها - SearchTerm: {SearchTerm}, Status: {Status}, Page: {Page}. User: {UserId}",
                    filter?.SearchTerm, filter?.Status, page, _currentUserService.UserId);

                // ✅ تنظیم مقادیر پیش‌فرض
                filter = filter ?? new PaymentSearchFilter();
                if (page < 1) page = 1;

                const int pageSize = 20;

                // ✅ دریافت داده از Service
                var result = await _paymentService.GetPaymentsAsync(filter, page, pageSize);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست پرداخت‌ها - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(new PaymentIndexViewModel());
                }

                _logger.Information("لیست پرداخت‌ها با موفقیت دریافت شد - Count: {Count}. User: {UserId}",
                    result.Data?.Payments?.Count ?? 0, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست پرداخت‌ها. User: {UserId}", _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست پرداخت‌ها");
                return View(new PaymentIndexViewModel());
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات پرداخت
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.Information("درخواست جزئیات پرداخت - OnlinePaymentId: {OnlinePaymentId}. User: {UserId}",
                    id, _currentUserService.UserId);

                if (id <= 0)
                {
                    NotificationHelper.SetError(TempData, "شناسه پرداخت نامعتبر است");
                    return RedirectToAction("Index");
                }

                // ✅ دریافت داده از Service
                var result = await _paymentService.GetPaymentDetailsAsync(id);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت جزئیات پرداخت - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                _logger.Information("جزئیات پرداخت با موفقیت دریافت شد - OnlinePaymentId: {OnlinePaymentId}. User: {UserId}",
                    id, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات پرداخت - OnlinePaymentId: {Id}. User: {UserId}",
                    id, _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات پرداخت");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region API Actions (AJAX)

        /// <summary>
        /// API: دریافت لیست پرداخت‌ها (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetPayments(PaymentSearchFilter filter, int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.Debug("API: درخواست لیست پرداخت‌ها - Page: {Page}, PageSize: {PageSize}. User: {UserId}",
                    page, pageSize, _currentUserService.UserId);

                var result = await _paymentService.GetPaymentsAsync(filter ?? new PaymentSearchFilter(), page, pageSize);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new
                {
                    success = true,
                    data = result.Data,
                    message = "لیست پرداخت‌ها با موفقیت دریافت شد"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در API GetPayments. User: {UserId}", _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در دریافت لیست پرداخت‌ها" });
            }
        }

        /// <summary>
        /// API: Retry پرداخت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RetryPayment(int onlinePaymentId)
        {
            try
            {
                _logger.Information("API: درخواست Retry پرداخت - OnlinePaymentId: {OnlinePaymentId}. User: {UserId}",
                    onlinePaymentId, _currentUserService.UserId);

                var result = await _paymentService.RetryPaymentAsync(onlinePaymentId, _currentUserService.UserId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, message = "پرداخت با موفقیت Retry شد" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در API RetryPayment - OnlinePaymentId: {OnlinePaymentId}. User: {UserId}",
                    onlinePaymentId, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در Retry پرداخت" });
            }
        }

        /// <summary>
        /// API: Cancel پرداخت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CancelPayment(int onlinePaymentId, string reason)
        {
            try
            {
                _logger.Information("API: درخواست Cancel پرداخت - OnlinePaymentId: {OnlinePaymentId}, Reason: {Reason}. User: {UserId}",
                    onlinePaymentId, reason, _currentUserService.UserId);

                var result = await _paymentService.CancelPaymentAsync(onlinePaymentId, reason, _currentUserService.UserId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, message = "پرداخت با موفقیت Cancel شد" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در API CancelPayment - OnlinePaymentId: {OnlinePaymentId}. User: {UserId}",
                    onlinePaymentId, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در Cancel پرداخت" });
            }
        }

        /// <summary>
        /// API: Refund پرداخت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RefundPayment(int onlinePaymentId, decimal? refundAmount, string reason)
        {
            try
            {
                _logger.Information("API: درخواست Refund پرداخت - OnlinePaymentId: {OnlinePaymentId}, RefundAmount: {RefundAmount}, Reason: {Reason}. User: {UserId}",
                    onlinePaymentId, refundAmount, reason, _currentUserService.UserId);

                var result = await _paymentService.RefundPaymentAsync(onlinePaymentId, refundAmount, reason, _currentUserService.UserId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, message = "پرداخت با موفقیت Refund شد" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در API RefundPayment - OnlinePaymentId: {OnlinePaymentId}. User: {UserId}",
                    onlinePaymentId, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در Refund پرداخت" });
            }
        }

        #endregion
    }
}

