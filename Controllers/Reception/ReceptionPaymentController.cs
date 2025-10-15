using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Services.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر تخصصی مدیریت پرداخت POS در فرم پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پرداخت با دستگاه POS
    /// 2. مدیریت تراکنش‌ها
    /// 3. تأیید پرداخت
    /// 4. مدیریت خطاهای پرداخت
    /// 5. بهینه‌سازی برای محیط درمانی
    /// 
    /// نکته حیاتی: این کنترلر از سرویس‌های تخصصی استفاده می‌کند
    /// </summary>
    [RoutePrefix("Reception/Payment")]
    public class ReceptionPaymentController : BaseController
    {
        private readonly ReceptionPaymentService _paymentService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionPaymentController(
            ReceptionPaymentService paymentService,
            ILogger logger,
            ICurrentUserService currentUserService) : base(logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger.ForContext<ReceptionPaymentController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region POS Payment Processing

        /// <summary>
        /// پردازش پرداخت با دستگاه POS
        /// </summary>
        /// <param name="paymentRequest">درخواست پرداخت</param>
        /// <returns>نتیجه پرداخت</returns>
        [HttpPost]
        [Route("ProcessPosPayment")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ProcessPosPayment(ReceptionPaymentRequestViewModel paymentRequest)
        {
            try
            {
                _logger.Information("💳 پردازش پرداخت POS برای فرم پذیرش. PatientId: {PatientId}, Amount: {Amount}, User: {UserName}", 
                    paymentRequest.PatientId, paymentRequest.Amount, _currentUserService.UserName);

                var result = await _paymentService.ProcessPosPaymentAsync(paymentRequest);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش پرداخت POS. PatientId: {PatientId}, Amount: {Amount}", 
                    paymentRequest.PatientId, paymentRequest.Amount);
                return Json(new { success = false, message = "خطا در پردازش پرداخت POS" });
            }
        }

        #endregion

        #region Payment Verification

        /// <summary>
        /// تأیید پرداخت
        /// </summary>
        /// <param name="paymentId">شناسه پرداخت</param>
        /// <returns>نتیجه تأیید</returns>
        [HttpPost]
        [Route("VerifyPayment")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> VerifyPayment(string paymentId)
        {
            try
            {
                _logger.Information("✅ تأیید پرداخت برای فرم پذیرش. PaymentId: {PaymentId}, User: {UserName}", 
                    paymentId, _currentUserService.UserName);

                var result = await _paymentService.VerifyPaymentAsync(paymentId);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تأیید پرداخت. PaymentId: {PaymentId}", paymentId);
                return Json(new { success = false, message = "خطا در تأیید پرداخت" });
            }
        }

        #endregion

        #region Payment History

        /// <summary>
        /// دریافت تاریخچه پرداخت‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>تاریخچه پرداخت‌ها</returns>
        [HttpPost]
        [Route("GetPatientPaymentHistory")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetPatientPaymentHistory(int patientId)
        {
            try
            {
                _logger.Information("📋 دریافت تاریخچه پرداخت‌های بیمار برای فرم پذیرش. PatientId: {PatientId}, User: {UserName}", 
                    patientId, _currentUserService.UserName);

                var result = await _paymentService.GetPatientPaymentHistoryAsync(patientId);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت تاریخچه پرداخت‌های بیمار. PatientId: {PatientId}", patientId);
                return Json(new { success = false, message = "خطا در دریافت تاریخچه پرداخت‌ها" });
            }
        }

        #endregion

        #region Payment Error Handling

        /// <summary>
        /// مدیریت خطاهای پرداخت
        /// </summary>
        /// <param name="errorCode">کد خطا</param>
        /// <param name="errorMessage">پیام خطا</param>
        /// <returns>اطلاعات خطا</returns>
        [HttpPost]
        [Route("HandlePaymentError")]
        [ValidateAntiForgeryToken]
        public JsonResult HandlePaymentError(string errorCode, string errorMessage)
        {
            try
            {
                _logger.Warning("⚠️ مدیریت خطای پرداخت. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, User: {UserName}", 
                    errorCode, errorMessage, _currentUserService.UserName);

                var result = _paymentService.HandlePaymentError(errorCode, errorMessage);
                
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در مدیریت خطای پرداخت. ErrorCode: {ErrorCode}", errorCode);
                return Json(new { success = false, message = "خطا در مدیریت خطای پرداخت" });
            }
        }

        #endregion

        #region Payment Status

        /// <summary>
        /// دریافت وضعیت پرداخت‌ها برای سایدبار
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetPaymentStatus()
        {
            try
            {
                _logger.Information("🏥 دریافت وضعیت پرداخت‌ها برای سایدبار. کاربر: {UserName}", _currentUserService.UserName);

                // دریافت آمار پرداخت‌ها
                var todayPayments = await _paymentService.GetTodayPaymentsCountAsync();
                var totalAmount = await _paymentService.GetTodayTotalAmountAsync();

                var result = new
                {
                    success = true,
                    data = new
                    {
                        todayPayments = todayPayments,
                        totalAmount = totalAmount
                    }
                };

                _logger.Information("✅ وضعیت پرداخت‌ها دریافت شد: تعداد={Count}, مبلغ={Amount}", todayPayments, totalAmount);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت وضعیت پرداخت‌ها");
                return Json(new { success = false, message = "خطا در دریافت وضعیت پرداخت‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
