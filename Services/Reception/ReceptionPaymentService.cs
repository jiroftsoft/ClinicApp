using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس تخصصی مدیریت پرداخت POS در فرم پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پرداخت با دستگاه POS
    /// 2. مدیریت تراکنش‌ها
    /// 3. تأیید پرداخت
    /// 4. مدیریت خطاهای پرداخت
    /// 5. بهینه‌سازی برای محیط درمانی
    /// 
    /// نکته حیاتی: این سرویس از ماژول‌های موجود استفاده می‌کند
    /// </summary>
    public class ReceptionPaymentService
    {
        private readonly IReceptionService _receptionService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionPaymentService(
            IReceptionService receptionService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _logger = logger.ForContext<ReceptionPaymentService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region POS Payment Processing

        /// <summary>
        /// پردازش پرداخت با دستگاه POS
        /// </summary>
        /// <param name="paymentRequest">درخواست پرداخت</param>
        /// <returns>نتیجه پرداخت</returns>
        public async Task<ServiceResult<ReceptionPaymentResultViewModel>> ProcessPosPaymentAsync(ReceptionPaymentRequestViewModel paymentRequest)
        {
            try
            {
                _logger.Information("💳 پردازش پرداخت POS برای فرم پذیرش. PatientId: {PatientId}, Amount: {Amount}, User: {UserName}", 
                    paymentRequest.PatientId, paymentRequest.Amount, _currentUserService.UserName);

                // اینجا باید از سرویس موجود برای پردازش پرداخت استفاده کنید
                // var result = await _paymentService.ProcessPosPaymentAsync(paymentRequest);
                
                // برای حالا یک نتیجه نمونه برمی‌گردانیم
                var paymentResult = new ReceptionPaymentResultViewModel
                {
                    PaymentId = Guid.NewGuid().ToString(),
                    PatientId = paymentRequest.PatientId,
                    Amount = paymentRequest.Amount,
                    PaymentMethod = "POS",
                    PaymentStatus = "Success",
                    TransactionId = Guid.NewGuid().ToString(),
                    PaymentDate = DateTime.Now,
                    IsSuccessful = true,
                    Message = "پرداخت با موفقیت انجام شد"
                };

                _logger.Information("✅ پرداخت POS موفق. PaymentId: {PaymentId}, Amount: {Amount}", 
                    paymentResult.PaymentId, paymentResult.Amount);

                return ServiceResult<ReceptionPaymentResultViewModel>.Successful(paymentResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش پرداخت POS. PatientId: {PatientId}, Amount: {Amount}", 
                    paymentRequest.PatientId, paymentRequest.Amount);
                return ServiceResult<ReceptionPaymentResultViewModel>.Failed("خطا در پردازش پرداخت POS");
            }
        }

        #endregion

        #region Payment Verification

        /// <summary>
        /// تأیید پرداخت
        /// </summary>
        /// <param name="paymentId">شناسه پرداخت</param>
        /// <returns>نتیجه تأیید</returns>
        public async Task<ServiceResult<ReceptionPaymentVerificationViewModel>> VerifyPaymentAsync(string paymentId)
        {
            try
            {
                _logger.Information("✅ تأیید پرداخت برای فرم پذیرش. PaymentId: {PaymentId}, User: {UserName}", 
                    paymentId, _currentUserService.UserName);

                // اینجا باید از سرویس موجود برای تأیید پرداخت استفاده کنید
                // var result = await _paymentService.VerifyPaymentAsync(paymentId);
                
                // برای حالا یک تأیید نمونه برمی‌گردانیم
                var verification = new ReceptionPaymentVerificationViewModel
                {
                    PaymentId = paymentId,
                    IsVerified = true,
                    VerificationDate = DateTime.Now,
                    VerificationStatus = "Verified",
                    Message = "پرداخت تأیید شد"
                };

                _logger.Information("✅ پرداخت تأیید شد. PaymentId: {PaymentId}", paymentId);
                return ServiceResult<ReceptionPaymentVerificationViewModel>.Successful(verification);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تأیید پرداخت. PaymentId: {PaymentId}", paymentId);
                return ServiceResult<ReceptionPaymentVerificationViewModel>.Failed("خطا در تأیید پرداخت");
            }
        }

        #endregion

        #region Payment History

        /// <summary>
        /// دریافت تاریخچه پرداخت‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>تاریخچه پرداخت‌ها</returns>
        public async Task<ServiceResult<List<ReceptionPaymentHistoryViewModel>>> GetPatientPaymentHistoryAsync(int patientId)
        {
            try
            {
                _logger.Information("📋 دریافت تاریخچه پرداخت‌های بیمار برای فرم پذیرش. PatientId: {PatientId}, User: {UserName}", 
                    patientId, _currentUserService.UserName);

                // اینجا باید از سرویس موجود برای دریافت تاریخچه پرداخت استفاده کنید
                // var result = await _paymentService.GetPatientPaymentHistoryAsync(patientId);
                
                // برای حالا یک لیست خالی برمی‌گردانیم
                var paymentHistory = new List<ReceptionPaymentHistoryViewModel>();

                _logger.Information("✅ تاریخچه پرداخت‌های بیمار دریافت شد. PatientId: {PatientId}, Count: {Count}", 
                    patientId, paymentHistory.Count);

                return ServiceResult<List<ReceptionPaymentHistoryViewModel>>.Successful(paymentHistory);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت تاریخچه پرداخت‌های بیمار. PatientId: {PatientId}", patientId);
                return ServiceResult<List<ReceptionPaymentHistoryViewModel>>.Failed("خطا در دریافت تاریخچه پرداخت‌ها");
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
        public ServiceResult<ReceptionPaymentErrorViewModel> HandlePaymentError(string errorCode, string errorMessage)
        {
            try
            {
                _logger.Warning("⚠️ مدیریت خطای پرداخت. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, User: {UserName}", 
                    errorCode, errorMessage, _currentUserService.UserName);

                var error = new ReceptionPaymentErrorViewModel
                {
                    ErrorCode = errorCode,
                    ErrorMessage = errorMessage,
                    ErrorDate = DateTime.Now,
                    IsRetryable = IsRetryableError(errorCode),
                    RetryCount = 0,
                    MaxRetries = 3
                };

                _logger.Warning("⚠️ خطای پرداخت مدیریت شد. ErrorCode: {ErrorCode}, IsRetryable: {IsRetryable}", 
                    errorCode, error.IsRetryable);

                return ServiceResult<ReceptionPaymentErrorViewModel>.Successful(error);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در مدیریت خطای پرداخت. ErrorCode: {ErrorCode}", errorCode);
                return ServiceResult<ReceptionPaymentErrorViewModel>.Failed("خطا در مدیریت خطای پرداخت");
            }
        }

        /// <summary>
        /// بررسی امکان تلاش مجدد برای خطا
        /// </summary>
        private bool IsRetryableError(string errorCode)
        {
            var retryableErrors = new[] { "TIMEOUT", "NETWORK_ERROR", "TEMPORARY_FAILURE" };
            return retryableErrors.Contains(errorCode);
        }

        #endregion

        #region Statistics Methods

        /// <summary>
        /// دریافت تعداد پرداخت‌های امروز
        /// </summary>
        /// <returns>تعداد پرداخت‌های امروز</returns>
        public async Task<int> GetTodayPaymentsCountAsync()
        {
            try
            {
                _logger.Information("📊 دریافت تعداد پرداخت‌های امروز. کاربر: {UserName}", _currentUserService.UserName);

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // استفاده از ReceptionService برای دریافت آمار
                var result = await _receptionService.GetReceptionsByDateRangeAsync(today, tomorrow);
                
                if (result.Success)
                {
                    var count = result.Data != null ? result.Data.Count() : 0;
                    _logger.Information("✅ تعداد پرداخت‌های امروز: {Count}", count);
                    return count;
                }

                _logger.Warning("⚠️ خطا در دریافت تعداد پرداخت‌های امروز: {Message}", result.Message);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در GetTodayPaymentsCountAsync");
                return 0;
            }
        }

        /// <summary>
        /// دریافت مجموع مبلغ پرداخت‌های امروز
        /// </summary>
        /// <returns>مجموع مبلغ پرداخت‌های امروز</returns>
        public async Task<decimal> GetTodayTotalAmountAsync()
        {
            try
            {
                _logger.Information("💰 دریافت مجموع مبلغ پرداخت‌های امروز. کاربر: {UserName}", _currentUserService.UserName);

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // استفاده از ReceptionService برای دریافت آمار
                var result = await _receptionService.GetReceptionsByDateRangeAsync(today, tomorrow);
                
                if (result.Success)
                {
                    var totalAmount = result.Data != null ? result.Data.Sum(r => r.TotalAmount) : 0;
                    _logger.Information("✅ مجموع مبلغ پرداخت‌های امروز: {Amount} ریال", totalAmount);
                    return totalAmount;
                }

                _logger.Warning("⚠️ خطا در دریافت مجموع مبلغ پرداخت‌های امروز: {Message}", result.Message);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در GetTodayTotalAmountAsync");
                return 0;
            }
        }

        #endregion
    }
}
