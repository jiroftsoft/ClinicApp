using ClinicApp.Interfaces.Payment.Web;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Interfaces.Payment.Gateway;
using GatewayConnectionTest = ClinicApp.Interfaces.Payment.Web.GatewayConnectionTest;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Models.Statistics;
using ClinicApp.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGatewayStatistics = ClinicApp.Models.Statistics.PaymentGatewayStatistics;

namespace ClinicApp.Services.Payment.Web
{
    /// <summary>
    /// Service برای مدیریت پرداخت‌های آنلاین
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت منطق کسب‌وکار پرداخت‌های وب
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل پرداخت‌های آنلاین
    /// 2. یکپارچه‌سازی با درگاه‌های پرداخت
    /// 3. مدیریت Callback ها و Webhook ها
    /// 4. پردازش پرداخت‌های غیرهمزمان
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public class WebPaymentService : IWebPaymentService
    {
        #region Fields

        private readonly IPaymentGatewayRepository _paymentGatewayRepository;
        private readonly IOnlinePaymentRepository _onlinePaymentRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IPaymentService _paymentService;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public WebPaymentService(
            IPaymentGatewayRepository paymentGatewayRepository,
            IOnlinePaymentRepository onlinePaymentRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            IPaymentService paymentService,
            ILogger logger)
        {
            _paymentGatewayRepository = paymentGatewayRepository ?? throw new ArgumentNullException(nameof(paymentGatewayRepository));
            _onlinePaymentRepository = onlinePaymentRepository ?? throw new ArgumentNullException(nameof(onlinePaymentRepository));
            _paymentTransactionRepository = paymentTransactionRepository ?? throw new ArgumentNullException(nameof(paymentTransactionRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Payment Gateway Integration

        /// <summary>
        /// ایجاد درخواست پرداخت در درگاه
        /// </summary>
        public async Task<ServiceResult<PaymentGatewayResponse>> CreatePaymentRequestAsync(CreatePaymentRequest request)
        {
            try
            {
                _logger.Information("شروع ایجاد درخواست پرداخت در درگاه {GatewayType} برای مبلغ {Amount}", 
                    request.GatewayType, request.Amount);

                // اعتبارسنجی درخواست
                var validationResult = await ValidateCreatePaymentRequestAsync(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("اعتبارسنجی درخواست پرداخت ناموفق: {Message}", validationResult.Message);
                    return ServiceResult<PaymentGatewayResponse>.Failed(validationResult.Message);
                }

                // دریافت اطلاعات درگاه پرداخت
                var gateways = await _paymentGatewayRepository.GetByTypeAsync(request.GatewayType);
                if (gateways == null || !gateways.Any())
                {
                    _logger.Warning("درگاه پرداخت {GatewayType} یافت نشد", request.GatewayType);
                    return ServiceResult<PaymentGatewayResponse>.Failed("درگاه پرداخت یافت نشد");
                }

                var gateway = gateways.FirstOrDefault();

                // بررسی فعال بودن درگاه
                if (!gateway.IsActive)
                {
                    _logger.Warning("درگاه پرداخت {GatewayType} غیرفعال است", request.GatewayType);
                    return ServiceResult<PaymentGatewayResponse>.Failed("درگاه پرداخت غیرفعال است");
                }

                // ایجاد درخواست پرداخت در درگاه (بر اساس نوع درگاه)
                var gatewayResponse = await CreateGatewayPaymentRequestAsync(gateway, request);
                if (!gatewayResponse.Success)
                {
                    _logger.Error("خطا در ایجاد درخواست پرداخت در درگاه: {Message}", gatewayResponse.Message);
                    return ServiceResult<PaymentGatewayResponse>.Failed("خطا در ایجاد درخواست پرداخت در درگاه");
                }

                _logger.Information("درخواست پرداخت با موفقیت در درگاه ایجاد شد. توکن: {PaymentToken}", 
                    gatewayResponse.Data.PaymentToken);
                return ServiceResult<PaymentGatewayResponse>.Successful(gatewayResponse.Data, "درخواست پرداخت با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد درخواست پرداخت در درگاه {GatewayType}", request.GatewayType);
                return ServiceResult<PaymentGatewayResponse>.Failed("خطا در ایجاد درخواست پرداخت در درگاه");
            }
        }

        /// <summary>
        /// پردازش Callback درگاه پرداخت
        /// </summary>
        public async Task<ServiceResult<PaymentCallbackResult>> ProcessPaymentCallbackAsync(PaymentGatewayType gatewayType, PaymentCallbackData callbackData)
        {
            try
            {
                _logger.Information("شروع پردازش Callback درگاه {GatewayType} برای تراکنش {TransactionId}", 
                    gatewayType, callbackData.TransactionId);

                // اعتبارسنجی Callback
                var validationResult = await ValidatePaymentCallbackAsync(gatewayType, callbackData);
                if (!validationResult.Success)
                {
                    _logger.Warning("اعتبارسنجی Callback ناموفق: {Message}", validationResult.Message);
                    return ServiceResult<PaymentCallbackResult>.Failed(validationResult.Message);
                }

                // دریافت اطلاعات درگاه پرداخت
                var gateways = await _paymentGatewayRepository.GetByTypeAsync(gatewayType);
                if (gateways == null || !gateways.Any())
                {
                    _logger.Warning("درگاه پرداخت {GatewayType} یافت نشد", gatewayType);
                    return ServiceResult<PaymentCallbackResult>.Failed("درگاه پرداخت یافت نشد");
                }

                var gateway = gateways.FirstOrDefault();

                // پردازش Callback بر اساس نوع درگاه
                var callbackResult = await ProcessGatewayCallbackAsync(gateway, callbackData);
                if (!callbackResult.Success)
                {
                    _logger.Error("خطا در پردازش Callback: {Message}", callbackResult.Message);
                    return ServiceResult<PaymentCallbackResult>.Failed("خطا در پردازش Callback");
                }

                _logger.Information("Callback با موفقیت پردازش شد. توکن: {PaymentToken}", callbackResult.Data.PaymentToken);
                return ServiceResult<PaymentCallbackResult>.Successful(callbackResult.Data, "Callback با موفقیت پردازش شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش Callback درگاه {GatewayType}", gatewayType);
                return ServiceResult<PaymentCallbackResult>.Failed("خطا در پردازش Callback");
            }
        }

        #endregion

        #region Payment Processing

        /// <summary>
        /// پردازش پرداخت آنلاین
        /// </summary>
        public async Task<ServiceResult<WebPaymentResult>> ProcessWebPaymentAsync(WebPaymentRequest request)
        {
            try
            {
                _logger.Information("شروع پردازش پرداخت آنلاین برای پذیرش {ReceptionId} با مبلغ {Amount}", 
                    request.ReceptionId, request.Amount);

                // اعتبارسنجی درخواست
                var validationResult = await ValidatePaymentRequestAsync(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("اعتبارسنجی درخواست پرداخت آنلاین ناموفق: {Message}", validationResult.Message);
                    return ServiceResult<WebPaymentResult>.Failed(validationResult.Message);
                }

                // استفاده از PaymentService برای ایجاد پرداخت آنلاین
                var onlinePaymentRequest = new OnlinePaymentRequest
                {
                    ReceptionId = request.ReceptionId,
                    AppointmentId = request.AppointmentId,
                    PatientId = request.PatientId,
                    PaymentType = request.PaymentType,
                    Amount = request.Amount,
                    PaymentGatewayId = request.PaymentGatewayId,
                    Description = request.Description,
                    UserIpAddress = request.UserIpAddress,
                    UserAgent = request.UserAgent,
                    CreatedByUserId = request.CreatedByUserId
                };

                var onlinePaymentResult = await _paymentService.ProcessOnlinePaymentAsync(onlinePaymentRequest);
                if (!onlinePaymentResult.Success)
                {
                    _logger.Error("خطا در ایجاد پرداخت آنلاین: {Message}", onlinePaymentResult.Message);
                    return ServiceResult<WebPaymentResult>.Failed("خطا در ایجاد پرداخت آنلاین", onlinePaymentResult.Message);
                }

                var onlinePayment = onlinePaymentResult.Data;

                // ایجاد درخواست پرداخت در درگاه
                var createPaymentRequest = new CreatePaymentRequest
                {
                    OnlinePaymentId = onlinePayment.OnlinePaymentId,
                    GatewayType = onlinePayment.PaymentGateway.GatewayType,
                    Amount = onlinePayment.Amount,
                    Description = onlinePayment.Description,
                    CallbackUrl = request.CallbackUrl,
                    UserIpAddress = request.UserIpAddress,
                    UserAgent = request.UserAgent
                };

                var gatewayResponse = await CreatePaymentRequestAsync(createPaymentRequest);
                if (!gatewayResponse.Success)
                {
                    _logger.Error("خطا در ایجاد درخواست پرداخت در درگاه: {Message}", gatewayResponse.Message);
                    return ServiceResult<WebPaymentResult>.Failed("خطا در ایجاد درخواست پرداخت در درگاه");
                }

                // به‌روزرسانی پرداخت آنلاین با اطلاعات درگاه
                onlinePayment.PaymentToken = gatewayResponse.Data.PaymentToken;
                onlinePayment.GatewayTransactionId = gatewayResponse.Data.GatewayTransactionId;
                onlinePayment.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _onlinePaymentRepository.UpdateAsync(onlinePayment);
                if (updateResult == null)
                {
                    _logger.Error("خطا در به‌روزرسانی پرداخت آنلاین");
                    return ServiceResult<WebPaymentResult>.Failed("خطا در به‌روزرسانی پرداخت آنلاین");
                }

                var result = new WebPaymentResult
                {
                    OnlinePaymentId = onlinePayment.OnlinePaymentId,
                    PaymentToken = onlinePayment.PaymentToken,
                    PaymentUrl = gatewayResponse.Data.PaymentUrl,
                    Status = onlinePayment.Status,
                    GatewayTransactionId = onlinePayment.GatewayTransactionId,
                    CreatedAt = onlinePayment.CreatedAt
                };

                _logger.Information("پرداخت آنلاین با موفقیت پردازش شد. شناسه: {OnlinePaymentId}", onlinePayment.OnlinePaymentId);
                return ServiceResult<WebPaymentResult>.Successful(result, "پرداخت آنلاین با موفقیت پردازش شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش پرداخت آنلاین برای پذیرش {ReceptionId}", request.ReceptionId);
                return ServiceResult<WebPaymentResult>.Failed("خطا در پردازش پرداخت آنلاین");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// اعتبارسنجی درخواست ایجاد پرداخت
        /// </summary>
        private async Task<ServiceResult> ValidateCreatePaymentRequestAsync(CreatePaymentRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("درخواست ایجاد پرداخت نمی‌تواند خالی باشد");
                return ServiceResult.Failed("درخواست ایجاد پرداخت نامعتبر است", string.Join("; ", errors));
            }

            if (request.OnlinePaymentId <= 0)
                errors.Add("شناسه پرداخت آنلاین نامعتبر است");

            if (request.Amount <= 0)
                errors.Add("مبلغ پرداخت باید بیشتر از صفر باشد");

            if (string.IsNullOrWhiteSpace(request.CallbackUrl))
                errors.Add("آدرس Callback الزامی است");

            if (errors.Any())
                return ServiceResult.Failed("اطلاعات وارد شده نامعتبر است", string.Join("; ", errors));

            return ServiceResult.Successful();
        }


        /// <summary>
        /// ایجاد درخواست پرداخت در درگاه (بر اساس نوع درگاه)
        /// </summary>
        private async Task<ServiceResult<PaymentGatewayResponse>> CreateGatewayPaymentRequestAsync(PaymentGateway gateway, CreatePaymentRequest request)
        {
            // TODO: پیاده‌سازی بر اساس نوع درگاه (ZarinPal, PayPing, etc.)
            // این بخش در مرحله بعدی پیاده‌سازی خواهد شد
            
            var response = new PaymentGatewayResponse
            {
                Success = true,
                GatewayTransactionId = GenerateGatewayTransactionId(),
                PaymentUrl = $"https://gateway.example.com/pay/{GeneratePaymentToken()}",
                PaymentToken = GeneratePaymentToken(),
                AdditionalData = new Dictionary<string, string>()
            };

            return ServiceResult<PaymentGatewayResponse>.Successful(response);
        }

        /// <summary>
        /// پردازش Callback درگاه (بر اساس نوع درگاه)
        /// </summary>
        private async Task<ServiceResult<PaymentCallbackResult>> ProcessGatewayCallbackAsync(PaymentGateway gateway, PaymentCallbackData callbackData)
        {
            // TODO: پیاده‌سازی بر اساس نوع درگاه
            // این بخش در مرحله بعدی پیاده‌سازی خواهد شد
            
            var result = new PaymentCallbackResult
            {
                Success = true,
                PaymentToken = callbackData.PaymentToken,
                Status = OnlinePaymentStatus.Successful,
                GatewayTransactionId = callbackData.TransactionId
            };

            return ServiceResult<PaymentCallbackResult>.Successful(result);
        }

        /// <summary>
        /// تولید شناسه تراکنش درگاه
        /// </summary>
        private string GenerateGatewayTransactionId()
        {
            return $"GW_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        /// <summary>
        /// تولید توکن پرداخت
        /// </summary>
        private string GeneratePaymentToken()
        {
            return $"TOKEN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper()}";
        }

        #endregion

        #region Placeholder Methods (To be implemented in next parts)

        public async Task<ServiceResult<PaymentWebhookResult>> ProcessPaymentWebhookAsync(PaymentGatewayType gatewayType, PaymentWebhookData webhookData)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("ProcessPaymentWebhookAsync will be implemented in next part");
        }

        public async Task<ServiceResult<PaymentStatus>> CheckPaymentStatusAsync(PaymentGatewayType gatewayType, string transactionId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("CheckPaymentStatusAsync will be implemented in next part");
        }

        public async Task<ServiceResult> CancelPaymentInGatewayAsync(PaymentGatewayType gatewayType, string transactionId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("CancelPaymentInGatewayAsync will be implemented in next part");
        }

        public async Task<ServiceResult<WebPaymentResult>> CompleteWebPaymentAsync(string paymentToken, PaymentCallbackData callbackData)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("CompleteWebPaymentAsync will be implemented in next part");
        }

        public async Task<ServiceResult> CancelWebPaymentAsync(string paymentToken, string reason)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("CancelWebPaymentAsync will be implemented in next part");
        }

        public async Task<ServiceResult<WebRefundResult>> RefundWebPaymentAsync(string paymentToken, decimal refundAmount, string reason)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("RefundWebPaymentAsync will be implemented in next part");
        }

        public async Task<ServiceResult<IEnumerable<PaymentGateway>>> GetActivePaymentGatewaysAsync()
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetActivePaymentGatewaysAsync will be implemented in next part");
        }

        public async Task<ServiceResult<PaymentGateway>> GetDefaultPaymentGatewayAsync()
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetDefaultPaymentGatewayAsync will be implemented in next part");
        }

        public async Task<ServiceResult> SetDefaultPaymentGatewayAsync(int gatewayId, string userId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("SetDefaultPaymentGatewayAsync will be implemented in next part");
        }

        public async Task<ServiceResult<GatewayConnectionTest>> TestGatewayConnectionAsync(int gatewayId)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("TestGatewayConnectionAsync will be implemented in next part");
        }

        public async Task<ServiceResult> ValidatePaymentCallbackAsync(PaymentGatewayType gatewayType, PaymentCallbackData callbackData)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("ValidatePaymentCallbackAsync will be implemented in next part");
        }

        public async Task<ServiceResult> ValidatePaymentWebhookAsync(PaymentGatewayType gatewayType, PaymentWebhookData webhookData)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("ValidatePaymentWebhookAsync will be implemented in next part");
        }

        public async Task<ServiceResult<WebPaymentStatistics>> GetWebPaymentStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetWebPaymentStatisticsAsync will be implemented in next part");
        }

        public async Task<ServiceResult<Interfaces.Payment.Web.PaymentGatewayStatistics>> GetPaymentGatewayStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // TODO: Implement payment gateway statistics
                return ServiceResult<Interfaces.Payment.Web.PaymentGatewayStatistics>.Successful(new Interfaces.Payment.Web.PaymentGatewayStatistics(), "آمار درگاه‌های پرداخت دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار درگاه‌های پرداخت");
                return ServiceResult<Interfaces.Payment.Web.PaymentGatewayStatistics>.Failed("خطا در دریافت آمار درگاه‌های پرداخت");
            }
        }

        public async Task<ServiceResult<DailyWebPaymentStatistics>> GetDailyWebPaymentStatisticsAsync(DateTime date)
        {
            // TODO: Implement in next part
            throw new NotImplementedException("GetDailyWebPaymentStatisticsAsync will be implemented in next part");
        }

        public async Task<ServiceResult> ValidatePaymentRequestAsync(WebPaymentRequest request)
        {
            try
            {
                var errors = new List<string>();

                if (request == null)
                {
                    errors.Add("درخواست پرداخت نمی‌تواند خالی باشد");
                    return ServiceResult.Failed("درخواست پرداخت نامعتبر است", string.Join("; ", errors));
                }

                if (request.ReceptionId <= 0)
                    errors.Add("شناسه پذیرش نامعتبر است");

                if (request.PatientId <= 0)
                    errors.Add("شناسه بیمار نامعتبر است");

                if (request.Amount <= 0)
                    errors.Add("مبلغ پرداخت باید بیشتر از صفر باشد");

                if (request.PaymentGatewayId <= 0)
                    errors.Add("شناسه درگاه پرداخت نامعتبر است");

                if (string.IsNullOrWhiteSpace(request.CreatedByUserId))
                    errors.Add("شناسه کاربر ایجادکننده الزامی است");

                if (string.IsNullOrWhiteSpace(request.UserIpAddress))
                    errors.Add("آدرس IP کاربر الزامی است");

                if (errors.Any())
                    return ServiceResult.Failed("اطلاعات وارد شده نامعتبر است", string.Join("; ", errors));

                return ServiceResult.Successful("درخواست پرداخت معتبر است");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی درخواست پرداخت");
                return ServiceResult.Failed("خطا در اعتبارسنجی درخواست پرداخت");
            }
        }

        #endregion
    }
}
