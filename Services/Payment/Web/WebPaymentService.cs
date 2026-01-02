using ClinicApp.Interfaces.Payment.Web;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Interfaces.Payment.Gateway;
using ClinicApp.Interfaces.Payment.Gateway.Drivers;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatewayConnectionTest = ClinicApp.Interfaces.Payment.Web.GatewayConnectionTest;
using PaymentGatewayStatistics = ClinicApp.Interfaces.Payment.Gateway.PaymentGatewayStatistics;
using PaymentStatus = ClinicApp.Interfaces.Payment.Web.PaymentStatus;
using GatewayPaymentRequest = ClinicApp.Interfaces.Payment.Gateway.Drivers.PaymentRequest;

namespace ClinicApp.Services.Payment.Web
{
    /// <summary>
    /// Service برای مدیریت پرداخت‌های آنلاین
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت منطق کسب‌وکار پرداخت‌های وب
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل پرداخت‌های آنلاین
    /// 2. یکپارچه‌سازی با درگاه‌های پرداخت (ZarinPal, PayPing, etc.)
    /// 3. مدیریت Callback ها و Webhook ها
    /// 4. پردازش پرداخت‌های غیرهمزمان
    /// 5. بهینه‌سازی برای عملکرد بالا
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class WebPaymentService : IWebPaymentService
    {
        #region Fields

        private readonly IPaymentGatewayRepository _paymentGatewayRepository;
        private readonly IOnlinePaymentRepository _onlinePaymentRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IPaymentService _paymentService;
        private readonly IGatewayDriver _gatewayDriver; // ✅ ZarinPal Driver
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public WebPaymentService(
            IPaymentGatewayRepository paymentGatewayRepository,
            IOnlinePaymentRepository onlinePaymentRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            IPaymentService paymentService,
            IGatewayDriver gatewayDriver, // ✅ ZarinPal Driver
            ILogger logger)
        {
            _paymentGatewayRepository = paymentGatewayRepository ?? throw new ArgumentNullException(nameof(paymentGatewayRepository));
            _onlinePaymentRepository = onlinePaymentRepository ?? throw new ArgumentNullException(nameof(onlinePaymentRepository));
            _paymentTransactionRepository = paymentTransactionRepository ?? throw new ArgumentNullException(nameof(paymentTransactionRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _gatewayDriver = gatewayDriver ?? throw new ArgumentNullException(nameof(gatewayDriver));
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
                _logger.Information("💰 WEB PAYMENT: شروع ایجاد درخواست پرداخت در درگاه {GatewayType} برای مبلغ {Amount}", 
                    request.GatewayType, request.Amount);

                // اعتبارسنجی درخواست
                var validationResult = await ValidateCreatePaymentRequestAsync(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: اعتبارسنجی درخواست پرداخت ناموفق: {Message}", validationResult.Message);
                    return ServiceResult<PaymentGatewayResponse>.Failed(validationResult.Message);
                }

                // ✅ دریافت اطلاعات درگاه پرداخت با Caching (داده‌هایی که کم تغییر می‌کنند)
                var cacheKey = $"PaymentGateways_{request.GatewayType}";
                var cachedGateways = CacheHelper.Get<List<PaymentGateway>>(cacheKey);
                
                List<PaymentGateway> gateways;
                if (cachedGateways != null)
                {
                    _logger.Debug("📦 CACHE HIT: دریافت درگاه‌های پرداخت از Cache - GatewayType: {GatewayType}", request.GatewayType);
                    gateways = cachedGateways;
                }
                else
                {
                    _logger.Debug("📦 CACHE MISS: دریافت درگاه‌های پرداخت از Database - GatewayType: {GatewayType}", request.GatewayType);
                    var gatewaysEnumerable = await _paymentGatewayRepository.GetByTypeAsync(request.GatewayType);
                    gateways = gatewaysEnumerable?.ToList() ?? new List<PaymentGateway>(); // ✅ تبدیل IEnumerable به List
                    if (gateways.Any())
                    {
                        CacheHelper.Set(cacheKey, gateways, expirationMinutes: 30); // Cache برای 30 دقیقه
                    }
                }

                if (gateways == null || !gateways.Any())
                {
                    _logger.Warning("⚠️ WEB PAYMENT: درگاه پرداخت {GatewayType} یافت نشد", request.GatewayType);
                    return ServiceResult<PaymentGatewayResponse>.Failed("درگاه پرداخت یافت نشد");
                }

                var gateway = gateways.FirstOrDefault();

                // بررسی فعال بودن درگاه
                if (!gateway.IsActive)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: درگاه پرداخت {GatewayType} غیرفعال است", request.GatewayType);
                    return ServiceResult<PaymentGatewayResponse>.Failed("درگاه پرداخت غیرفعال است");
                }

                // ✅ ایجاد درخواست پرداخت در درگاه با استفاده از Driver
                var gatewayResponse = await CreateGatewayPaymentRequestAsync(gateway, request);
                if (!gatewayResponse.Success)
                {
                    _logger.Error("❌ WEB PAYMENT: خطا در ایجاد درخواست پرداخت در درگاه: {Message}", gatewayResponse.Message);
                    return ServiceResult<PaymentGatewayResponse>.Failed("خطا در ایجاد درخواست پرداخت در درگاه");
                }

                _logger.Information("✅ WEB PAYMENT: درخواست پرداخت با موفقیت در درگاه ایجاد شد. Authority: {Authority}, PaymentUrl: {PaymentUrl}", 
                    gatewayResponse.Data.GatewayTransactionId, gatewayResponse.Data.PaymentUrl);
                return ServiceResult<PaymentGatewayResponse>.Successful(gatewayResponse.Data, "درخواست پرداخت با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در ایجاد درخواست پرداخت در درگاه {GatewayType}", request.GatewayType);
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
                _logger.Information("🔍 WEB PAYMENT: شروع پردازش Callback درگاه {GatewayType} برای Authority {Authority}", 
                    gatewayType, callbackData.TransactionId);

                // اعتبارسنجی Callback
                var validationResult = await ValidatePaymentCallbackAsync(gatewayType, callbackData);
                if (!validationResult.Success)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: اعتبارسنجی Callback ناموفق: {Message}", validationResult.Message);
                    return ServiceResult<PaymentCallbackResult>.Failed(validationResult.Message);
                }

                // دریافت اطلاعات درگاه پرداخت
                var gateways = await _paymentGatewayRepository.GetByTypeAsync(gatewayType);
                if (gateways == null || !gateways.Any())
                {
                    _logger.Warning("⚠️ WEB PAYMENT: درگاه پرداخت {GatewayType} یافت نشد", gatewayType);
                    return ServiceResult<PaymentCallbackResult>.Failed("درگاه پرداخت یافت نشد");
                }

                var gateway = gateways.FirstOrDefault();

                // ✅ پردازش Callback بر اساس نوع درگاه با استفاده از Driver
                var callbackResult = await ProcessGatewayCallbackAsync(gateway, callbackData);
                if (!callbackResult.Success)
                {
                    _logger.Error("❌ WEB PAYMENT: خطا در پردازش Callback: {Message}", callbackResult.Message);
                    return ServiceResult<PaymentCallbackResult>.Failed("خطا در پردازش Callback");
                }

                _logger.Information("✅ WEB PAYMENT: Callback با موفقیت پردازش شد. Authority: {Authority}, Status: {Status}", 
                    callbackData.TransactionId, callbackResult.Data.Status);

                return ServiceResult<PaymentCallbackResult>.Successful(callbackResult.Data, "Callback با موفقیت پردازش شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در پردازش Callback - GatewayType: {GatewayType}, TransactionId: {TransactionId}, PaymentToken: {PaymentToken}",
                    gatewayType, callbackData?.TransactionId, callbackData?.PaymentToken);
                return ServiceResult<PaymentCallbackResult>.Failed(
                    "خطا در پردازش Callback. لطفاً با پشتیبانی تماس بگیرید.",
                    "CALLBACK_PROCESSING_ERROR");
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
                _logger.Information("💰 WEB PAYMENT: شروع پردازش پرداخت آنلاین برای پذیرش {ReceptionId} با مبلغ {Amount}", 
                    request.ReceptionId, request.Amount);

                // اعتبارسنجی درخواست
                var validationResult = await ValidatePaymentRequestAsync(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: اعتبارسنجی درخواست پرداخت آنلاین ناموفق: {Message}", validationResult.Message);
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
                    _logger.Error("❌ WEB PAYMENT: خطا در ایجاد پرداخت آنلاین: {Message}", onlinePaymentResult.Message);
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
                    _logger.Error("❌ WEB PAYMENT: خطا در ایجاد درخواست پرداخت در درگاه - OnlinePaymentId: {OnlinePaymentId}, GatewayType: {GatewayType}, Error: {Message}",
                        onlinePayment.OnlinePaymentId, onlinePayment.PaymentGateway.GatewayType, gatewayResponse.Message);
                    
                    // ✅ به‌روزرسانی وضعیت پرداخت به Failed
                    onlinePayment.Status = OnlinePaymentStatus.Failed;
                    onlinePayment.ErrorMessage = gatewayResponse.Message ?? "خطا در ایجاد درخواست پرداخت در درگاه";
                    onlinePayment.UpdatedAt = DateTime.UtcNow;
                    await _onlinePaymentRepository.UpdateAsync(onlinePayment);
                    
                    return ServiceResult<WebPaymentResult>.Failed(
                        gatewayResponse.Message ?? "خطا در ایجاد درخواست پرداخت در درگاه",
                        "GATEWAY_REQUEST_FAILED");
                }

                // ✅ به‌روزرسانی پرداخت آنلاین با اطلاعات درگاه
                onlinePayment.PaymentToken = gatewayResponse.Data.PaymentToken ?? gatewayResponse.Data.GatewayTransactionId; // Authority
                onlinePayment.GatewayTransactionId = gatewayResponse.Data.GatewayTransactionId; // Authority
                onlinePayment.PaymentUrl = gatewayResponse.Data.PaymentUrl;
                onlinePayment.PaymentStartDate = DateTime.UtcNow;
                onlinePayment.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _onlinePaymentRepository.UpdateAsync(onlinePayment);
                if (updateResult == null)
                {
                    _logger.Error("❌ WEB PAYMENT: خطا در به‌روزرسانی پرداخت آنلاین - OnlinePaymentId: {OnlinePaymentId}, PaymentToken: {PaymentToken}",
                        onlinePayment.OnlinePaymentId, onlinePayment.PaymentToken);
                    return ServiceResult<WebPaymentResult>.Failed(
                        "خطا در به‌روزرسانی پرداخت آنلاین",
                        "UPDATE_FAILED");
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

                _logger.Information("✅ WEB PAYMENT: پرداخت آنلاین با موفقیت پردازش شد. شناسه: {OnlinePaymentId}, PaymentUrl: {PaymentUrl}", 
                    onlinePayment.OnlinePaymentId, result.PaymentUrl);

                return ServiceResult<WebPaymentResult>.Successful(result, "پرداخت آنلاین با موفقیت پردازش شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در پردازش پرداخت آنلاین - ReceptionId: {ReceptionId}, AppointmentId: {AppointmentId}, PatientId: {PatientId}, Amount: {Amount}",
                    request?.ReceptionId, request?.AppointmentId, request?.PatientId, request?.Amount);
                return ServiceResult<WebPaymentResult>.Failed(
                    "خطا در پردازش پرداخت آنلاین. لطفاً با پشتیبانی تماس بگیرید.",
                    "PROCESSING_ERROR");
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
        /// ✅ ایجاد درخواست پرداخت در درگاه با استفاده از Driver
        /// </summary>
        private async Task<ServiceResult<PaymentGatewayResponse>> CreateGatewayPaymentRequestAsync(PaymentGateway gateway, CreatePaymentRequest request)
        {
            try
            {
                _logger.Debug("🔧 WEB PAYMENT: استفاده از Gateway Driver برای {GatewayType}", gateway.GatewayType);

                // ✅ تبدیل CreatePaymentRequest به PaymentRequest (Driver)
                var driverRequest = new GatewayPaymentRequest
                {
                    Amount = request.Amount,
                    Description = request.Description,
                    CallbackUrl = request.CallbackUrl,
                    Mobile = request.AdditionalData?.ContainsKey("Mobile") == true ? request.AdditionalData["Mobile"] : null,
                    Email = request.AdditionalData?.ContainsKey("Email") == true ? request.AdditionalData["Email"] : null,
                    Metadata = request.AdditionalData != null ? Newtonsoft.Json.JsonConvert.SerializeObject(request.AdditionalData) : null,
                    AdditionalData = request.AdditionalData
                };

                // ✅ فراخوانی Driver
                var driverResult = await _gatewayDriver.RequestPaymentAsync(driverRequest);
                
                if (!driverResult.Success || driverResult.Data == null)
                {
                    _logger.Error("❌ WEB PAYMENT: Driver درخواست پرداخت ناموفق - {Message}", driverResult.Message);
                    return ServiceResult<PaymentGatewayResponse>.Failed(driverResult.Message ?? "خطا در درخواست پرداخت");
                }

                // ✅ تبدیل PaymentRequestResult به PaymentGatewayResponse
                var response = new PaymentGatewayResponse
                {
                    Success = driverResult.Data.Success,
                    GatewayTransactionId = driverResult.Data.Authority, // Authority = GatewayTransactionId
                    PaymentUrl = driverResult.Data.PaymentUrl,
                    PaymentToken = driverResult.Data.Authority, // Authority = PaymentToken
                    ErrorCode = driverResult.Data.ErrorCode,
                    ErrorMessage = driverResult.Data.ErrorMessage,
                    AdditionalData = driverResult.Data.AdditionalData ?? new Dictionary<string, string>()
                };

                _logger.Information("✅ WEB PAYMENT: Driver درخواست پرداخت موفق - Authority: {Authority}, PaymentUrl: {PaymentUrl}", 
                    response.GatewayTransactionId, response.PaymentUrl);

                return ServiceResult<PaymentGatewayResponse>.Successful(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطای غیرمنتظره در CreateGatewayPaymentRequestAsync");
                return ServiceResult<PaymentGatewayResponse>.Failed("خطا در ایجاد درخواست پرداخت در درگاه");
            }
        }

        /// <summary>
        /// ✅ پردازش Callback درگاه با استفاده از Driver
        /// </summary>
        private async Task<ServiceResult<PaymentCallbackResult>> ProcessGatewayCallbackAsync(PaymentGateway gateway, PaymentCallbackData callbackData)
        {
            try
            {
                _logger.Debug("🔧 WEB PAYMENT: استفاده از Gateway Driver برای پردازش Callback - {GatewayType}", gateway.GatewayType);

                // ✅ دریافت OnlinePayment از PaymentToken (Authority)
                var onlinePayment = await _onlinePaymentRepository.GetByPaymentTokenAsync(callbackData.PaymentToken);
                if (onlinePayment == null)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: OnlinePayment با PaymentToken {PaymentToken} یافت نشد", callbackData.PaymentToken);
                    return ServiceResult<PaymentCallbackResult>.Failed("پرداخت یافت نشد");
                }

                // ✅ فراخوانی Driver برای Verify
                var verifyRequest = new PaymentVerificationRequest
                {
                    Authority = callbackData.PaymentToken ?? callbackData.TransactionId, // Authority = PaymentToken
                    Amount = callbackData.Amount ?? onlinePayment.Amount,
                    AdditionalData = callbackData.AdditionalData
                };
                var verifyResult = await _gatewayDriver.VerifyPaymentAsync(verifyRequest);

                if (!verifyResult.Success || verifyResult.Data == null)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: Driver Verify ناموفق - Authority: {Authority}, Message: {Message}", 
                        callbackData.TransactionId, verifyResult.Message);

                    var failedResult = new PaymentCallbackResult
                    {
                        Success = false,
                        PaymentToken = callbackData.PaymentToken,
                        Status = OnlinePaymentStatus.Failed,
                        GatewayTransactionId = callbackData.TransactionId,
                        ErrorMessage = verifyResult.Message ?? "تأیید پرداخت ناموفق"
                    };

                    return ServiceResult<PaymentCallbackResult>.Failed(verifyResult.Message ?? "تأیید پرداخت ناموفق");
                }

                // ✅ تبدیل PaymentVerificationResult به PaymentCallbackResult
                var verificationData = verifyResult.Data;
                var result = new PaymentCallbackResult
                {
                    Success = verifyResult.Data.Success,
                    PaymentToken = callbackData.PaymentToken,
                    Status = verifyResult.Data.Success ? OnlinePaymentStatus.Successful : OnlinePaymentStatus.Failed,
                    GatewayTransactionId = verifyResult.Data.RefId ?? callbackData.TransactionId, // RefId = GatewayTransactionId
                    ErrorMessage = verifyResult.Data.ErrorMessage
                };

                _logger.Information("✅ WEB PAYMENT: Driver Verify موفق - Authority: {Authority}, RefId: {RefId}", 
                    callbackData.TransactionId, verifyResult.Data.RefId);

                return ServiceResult<PaymentCallbackResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطای غیرمنتظره در ProcessGatewayCallbackAsync");
                return ServiceResult<PaymentCallbackResult>.Failed("خطا در پردازش Callback");
            }
        }

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت
        /// </summary>
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
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در اعتبارسنجی درخواست پرداخت");
                return ServiceResult.Failed("خطا در اعتبارسنجی درخواست پرداخت");
            }
        }

        /// <summary>
        /// اعتبارسنجی Callback
        /// </summary>
        public async Task<ServiceResult> ValidatePaymentCallbackAsync(PaymentGatewayType gatewayType, PaymentCallbackData callbackData)
        {
            try
            {
                if (callbackData == null)
                {
                    return ServiceResult.Failed("داده‌های Callback نمی‌تواند خالی باشد");
                }

                if (string.IsNullOrWhiteSpace(callbackData.PaymentToken) && string.IsNullOrWhiteSpace(callbackData.TransactionId))
                {
                    return ServiceResult.Failed("PaymentToken یا TransactionId الزامی است");
                }

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در اعتبارسنجی Callback");
                return ServiceResult.Failed("خطا در اعتبارسنجی Callback");
            }
        }

        #endregion

        #region Placeholder Methods (To be implemented in next parts)

        public async Task<ServiceResult<PaymentWebhookResult>> ProcessPaymentWebhookAsync(PaymentGatewayType gatewayType, PaymentWebhookData webhookData)
        {
            // FIXME(Phase 2): Implement webhook processing for payment gateways
            _logger.Warning("⚠️ WEB PAYMENT: ProcessPaymentWebhookAsync not implemented yet");
            return await Task.FromResult(ServiceResult<PaymentWebhookResult>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<PaymentStatus>> CheckPaymentStatusAsync(PaymentGatewayType gatewayType, string transactionId)
        {
            try
            {
                _logger.Information("🔍 WEB PAYMENT: بررسی وضعیت پرداخت - GatewayType: {GatewayType}, TransactionId: {TransactionId}", 
                    gatewayType, transactionId);

                // ✅ دریافت OnlinePayment برای Amount
                var onlinePayment = await _onlinePaymentRepository.GetByPaymentTokenAsync(transactionId);
                if (onlinePayment == null)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: OnlinePayment با TransactionId {TransactionId} یافت نشد", transactionId);
                    return ServiceResult<PaymentStatus>.Failed("پرداخت یافت نشد");
                }

                // ✅ استفاده از Driver برای بررسی وضعیت
                var statusResult = await _gatewayDriver.CheckPaymentStatusAsync(transactionId, onlinePayment.Amount);
                
                if (!statusResult.Success || statusResult.Data == null)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: بررسی وضعیت ناموفق - {Message}", statusResult.Message);
                    return ServiceResult<PaymentStatus>.Failed(statusResult.Message ?? "خطا در بررسی وضعیت");
                }

                // ✅ تبدیل PaymentStatusResult به PaymentStatus
                var paymentStatus = new PaymentStatus
                {
                    TransactionId = statusResult.Data.TransactionId,
                    Amount = statusResult.Data.Amount
                };
                
                // تبدیل Status String به Enum
                switch (statusResult.Data.Status?.ToLower())
                {
                    case "success":
                    case "successful":
                        paymentStatus.Status = OnlinePaymentStatus.Successful;
                        break;
                    case "failed":
                        paymentStatus.Status = OnlinePaymentStatus.Failed;
                        break;
                    case "canceled":
                    case "cancelled":
                        paymentStatus.Status = OnlinePaymentStatus.Canceled;
                        break;
                    default:
                        paymentStatus.Status = OnlinePaymentStatus.Pending;
                        break;
                }

                _logger.Information("✅ WEB PAYMENT: وضعیت پرداخت بررسی شد - Status: {Status}", paymentStatus.Status);

                return ServiceResult<PaymentStatus>.Successful(paymentStatus, "وضعیت پرداخت بررسی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در بررسی وضعیت پرداخت");
                return ServiceResult<PaymentStatus>.Failed("خطا در بررسی وضعیت پرداخت");
            }
        }

        public async Task<ServiceResult> CancelPaymentInGatewayAsync(PaymentGatewayType gatewayType, string transactionId)
        {
            // ⚠️ توجه: زرین‌پال API مستقیم برای Cancel ندارد
            // Cancel باید از طریق پنل مدیریتی زرین‌پال انجام شود
            _logger.Warning("⚠️ WEB PAYMENT: Cancel از طریق API پشتیبانی نمی‌شود");
            return ServiceResult.Failed("لغو پرداخت از طریق API پشتیبانی نمی‌شود", "NOT_SUPPORTED");
        }

        /// <summary>
        /// تکمیل پرداخت آنلاین
        /// </summary>
        public async Task<ServiceResult<WebPaymentResult>> CompleteWebPaymentAsync(string paymentToken, PaymentCallbackData callbackData)
        {
            try
            {
                _logger.Information("✅ WEB PAYMENT: شروع تکمیل پرداخت - PaymentToken: {PaymentToken}", paymentToken);

                // ✅ دریافت OnlinePayment
                var onlinePayment = await _onlinePaymentRepository.GetByPaymentTokenAsync(paymentToken);
                if (onlinePayment == null)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: OnlinePayment با PaymentToken {PaymentToken} یافت نشد", paymentToken);
                    return ServiceResult<WebPaymentResult>.Failed("پرداخت یافت نشد");
                }

                // ✅ پردازش Callback
                var callbackResult = await ProcessPaymentCallbackAsync(onlinePayment.PaymentGateway.GatewayType, callbackData);
                if (!callbackResult.Success || callbackResult.Data == null)
                {
                    _logger.Error("❌ WEB PAYMENT: خطا در پردازش Callback - {Message}", callbackResult.Message);
                    return ServiceResult<WebPaymentResult>.Failed(callbackResult.Message ?? "خطا در پردازش Callback");
                }

                var result = callbackResult.Data;

                // ✅ به‌روزرسانی OnlinePayment
                onlinePayment.Status = result.Status;
                onlinePayment.GatewayTransactionId = result.GatewayTransactionId;
                // استفاده از callbackData برای ReferenceCode و ErrorCode (چون PaymentCallbackResult این property ها را ندارد)
                onlinePayment.GatewayReferenceCode = callbackData.ReferenceCode ?? result.GatewayTransactionId;
                onlinePayment.PaymentCompletionDate = DateTime.UtcNow;
                onlinePayment.ErrorCode = callbackData.ErrorCode;
                onlinePayment.ErrorMessage = result.ErrorMessage ?? callbackData.ErrorMessage;
                onlinePayment.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _onlinePaymentRepository.UpdateAsync(onlinePayment);
                if (updateResult == null)
                {
                    _logger.Error("❌ WEB PAYMENT: خطا در به‌روزرسانی OnlinePayment");
                    return ServiceResult<WebPaymentResult>.Failed("خطا در به‌روزرسانی پرداخت");
                }

                var webPaymentResult = new WebPaymentResult
                {
                    OnlinePaymentId = onlinePayment.OnlinePaymentId,
                    PaymentToken = onlinePayment.PaymentToken,
                    PaymentUrl = onlinePayment.PaymentUrl,
                    Status = onlinePayment.Status,
                    GatewayTransactionId = onlinePayment.GatewayTransactionId,
                    CreatedAt = onlinePayment.CreatedAt
                };

                _logger.Information("✅ WEB PAYMENT: پرداخت با موفقیت تکمیل شد - OnlinePaymentId: {OnlinePaymentId}, Status: {Status}",
                    onlinePayment.OnlinePaymentId, onlinePayment.Status);

                return ServiceResult<WebPaymentResult>.Successful(webPaymentResult, "پرداخت با موفقیت تکمیل شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در تکمیل پرداخت - PaymentToken: {PaymentToken}, Status: {Status}, TransactionId: {TransactionId}",
                    paymentToken, callbackData?.Status, callbackData?.TransactionId);
                return ServiceResult<WebPaymentResult>.Failed(
                    "خطا در تکمیل پرداخت. لطفاً با پشتیبانی تماس بگیرید.",
                    "COMPLETION_ERROR");
            }
        }

        /// <summary>
        /// لغو پرداخت آنلاین
        /// </summary>
        public async Task<ServiceResult> CancelWebPaymentAsync(string paymentToken, string reason)
        {
            try
            {
                _logger.Information("🚫 WEB PAYMENT: شروع لغو پرداخت - PaymentToken: {PaymentToken}, Reason: {Reason}",
                    paymentToken, reason);

                // ✅ Validation
                if (string.IsNullOrWhiteSpace(paymentToken))
                {
                    return ServiceResult.Failed("توکن پرداخت الزامی است");
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    reason = "لغو توسط کاربر";
                }

                // ✅ دریافت OnlinePayment
                var onlinePayment = await _onlinePaymentRepository.GetByPaymentTokenAsync(paymentToken);
                if (onlinePayment == null)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: OnlinePayment با PaymentToken {PaymentToken} یافت نشد", paymentToken);
                    return ServiceResult.Failed("پرداخت یافت نشد");
                }

                // ✅ بررسی وضعیت - فقط پرداخت‌های Pending قابل Cancel هستند
                if (onlinePayment.Status != OnlinePaymentStatus.Pending)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: پرداخت با وضعیت {Status} قابل Cancel نیست - PaymentToken: {PaymentToken}",
                        onlinePayment.Status, paymentToken);
                    return ServiceResult.Failed($"پرداخت با وضعیت {GetStatusDisplay(onlinePayment.Status)} قابل Cancel نیست");
                }

                // ✅ تلاش برای Cancel در درگاه (اختیاری - ZarinPal API مستقیم ندارد)
                try
                {
                    var cancelResult = await CancelPaymentInGatewayAsync(
                        onlinePayment.PaymentGateway.GatewayType,
                        onlinePayment.GatewayTransactionId ?? paymentToken);

                    if (!cancelResult.Success)
                    {
                        _logger.Warning("⚠️ WEB PAYMENT: Cancel در درگاه ناموفق - {Message}", cancelResult.Message);
                        // ادامه می‌دهیم - Cancel داخلی انجام می‌شود
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "⚠️ WEB PAYMENT: خطا در Cancel در درگاه - ادامه با Cancel داخلی");
                    // ادامه می‌دهیم - Cancel داخلی انجام می‌شود
                }

                // ✅ به‌روزرسانی وضعیت به Canceled
                onlinePayment.Status = OnlinePaymentStatus.Canceled;
                onlinePayment.ErrorMessage = $"لغو شده: {reason}";
                onlinePayment.PaymentCompletionDate = DateTime.UtcNow;
                onlinePayment.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _onlinePaymentRepository.UpdateAsync(onlinePayment);
                if (updateResult == null)
                {
                    _logger.Error("❌ WEB PAYMENT: خطا در به‌روزرسانی OnlinePayment برای Cancel");
                    return ServiceResult.Failed("خطا در لغو پرداخت");
                }

                _logger.Information("✅ WEB PAYMENT: پرداخت با موفقیت لغو شد - OnlinePaymentId: {OnlinePaymentId}, Reason: {Reason}",
                    onlinePayment.OnlinePaymentId, reason);

                return ServiceResult.Successful("پرداخت با موفقیت لغو شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در لغو پرداخت - PaymentToken: {PaymentToken}, Reason: {Reason}",
                    paymentToken, reason);
                return ServiceResult.Failed(
                    "خطا در لغو پرداخت. لطفاً با پشتیبانی تماس بگیرید.",
                    "CANCEL_ERROR");
            }
        }

        /// <summary>
        /// Helper برای نمایش وضعیت پرداخت
        /// </summary>
        private string GetStatusDisplay(OnlinePaymentStatus status)
        {
            return status switch
            {
                OnlinePaymentStatus.Pending => "در انتظار",
                OnlinePaymentStatus.Processing => "در حال پردازش",
                OnlinePaymentStatus.Successful => "موفق",
                OnlinePaymentStatus.Failed => "ناموفق",
                OnlinePaymentStatus.Canceled => "لغو شده",
                OnlinePaymentStatus.Refunded => "برگشت خورده",
                OnlinePaymentStatus.Expired => "منقضی شده",
                _ => status.ToString()
            };
        }

        public async Task<ServiceResult<WebRefundResult>> RefundWebPaymentAsync(string paymentToken, decimal refundAmount, string reason)
        {
            try
            {
                _logger.Information("🔄 WEB PAYMENT: شروع برگشت وجه - PaymentToken: {PaymentToken}, Amount: {Amount}", 
                    paymentToken, refundAmount);

                // ✅ دریافت OnlinePayment
                var onlinePayment = await _onlinePaymentRepository.GetByPaymentTokenAsync(paymentToken);
                if (onlinePayment == null)
                {
                    return ServiceResult<WebRefundResult>.Failed("پرداخت یافت نشد");
                }

                // ✅ فراخوانی Driver برای Refund
                var refundRequest = new RefundRequest
                {
                    TransactionId = onlinePayment.GatewayTransactionId ?? paymentToken,
                    Amount = refundAmount,
                    Reason = reason,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "OnlinePaymentId", onlinePayment.OnlinePaymentId.ToString() },
                        { "OriginalAmount", onlinePayment.Amount.ToString("F2") }
                    }
                };
                var refundResult = await _gatewayDriver.RefundPaymentAsync(refundRequest);

                if (!refundResult.Success || refundResult.Data == null)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: Refund ناموفق - {Message}", refundResult.Message);
                    return ServiceResult<WebRefundResult>.Failed(refundResult.Message ?? "برگشت وجه ناموفق");
                }

                var result = new WebRefundResult
                {
                    Success = refundResult.Data.Success,
                    RefundId = refundResult.Data.RefundId,
                    RefundAmount = refundResult.Data.RefundAmount,
                    GatewayRefundId = refundResult.Data.GatewayRefundId,
                    ErrorMessage = refundResult.Data.ErrorMessage,
                    RefundedAt = DateTime.UtcNow
                };

                _logger.Information("✅ WEB PAYMENT: Refund موفق - RefundId: {RefundId}", result.RefundId);

                return ServiceResult<WebRefundResult>.Successful(result, "برگشت وجه با موفقیت انجام شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در برگشت وجه");
                return ServiceResult<WebRefundResult>.Failed("خطا در برگشت وجه");
            }
        }

        public async Task<ServiceResult<IEnumerable<PaymentGateway>>> GetActivePaymentGatewaysAsync()
        {
            // FIXME(Phase 2): Implement active payment gateways retrieval
            _logger.Warning("⚠️ WEB PAYMENT: GetActivePaymentGatewaysAsync not implemented yet");
            return await Task.FromResult(ServiceResult<IEnumerable<PaymentGateway>>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<PaymentGateway>> GetDefaultPaymentGatewayAsync()
        {
            // FIXME(Phase 2): Implement default payment gateway retrieval
            _logger.Warning("⚠️ WEB PAYMENT: GetDefaultPaymentGatewayAsync not implemented yet");
            return await Task.FromResult(ServiceResult<PaymentGateway>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult> SetDefaultPaymentGatewayAsync(int gatewayId, string userId)
        {
            // FIXME(Phase 2): Implement default payment gateway setting
            _logger.Warning("⚠️ WEB PAYMENT: SetDefaultPaymentGatewayAsync not implemented yet");
            return await Task.FromResult(ServiceResult.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<GatewayConnectionTest>> TestGatewayConnectionAsync(int gatewayId)
        {
            // FIXME(Phase 2): Implement gateway connection testing
            _logger.Warning("⚠️ WEB PAYMENT: TestGatewayConnectionAsync not implemented yet");
            return await Task.FromResult(ServiceResult<GatewayConnectionTest>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult> ValidatePaymentWebhookAsync(PaymentGatewayType gatewayType, PaymentWebhookData webhookData)
        {
            // FIXME(Phase 2): Implement webhook validation
            _logger.Warning("⚠️ WEB PAYMENT: ValidatePaymentWebhookAsync not implemented yet");
            return await Task.FromResult(ServiceResult.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<WebPaymentStatistics>> GetWebPaymentStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // FIXME(Phase 2): Implement web payment statistics calculation
            _logger.Warning("⚠️ WEB PAYMENT: GetWebPaymentStatisticsAsync not implemented yet");
            return await Task.FromResult(ServiceResult<WebPaymentStatistics>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        public async Task<ServiceResult<PaymentGatewayStatistics>> GetPaymentGatewayStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // FIXME(Phase 2): Implement actual payment gateway statistics calculation
                // Returning placeholder data for now (empty statistics).
                _logger.Warning("⚠️ WEB PAYMENT: GetPaymentGatewayStatisticsAsync returning placeholder data");
                return await Task.FromResult(ServiceResult<PaymentGatewayStatistics>.Successful(new PaymentGatewayStatistics(), "آمار درگاه‌های پرداخت دریافت شد"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در دریافت آمار درگاه‌های پرداخت");
                return ServiceResult<PaymentGatewayStatistics>.Failed("خطا در دریافت آمار درگاه‌های پرداخت");
            }
        }

        public async Task<ServiceResult<DailyWebPaymentStatistics>> GetDailyWebPaymentStatisticsAsync(DateTime date)
        {
            // FIXME(Phase 2): Implement daily web payment statistics calculation
            _logger.Warning("⚠️ WEB PAYMENT: GetDailyWebPaymentStatisticsAsync not implemented yet");
            return await Task.FromResult(ServiceResult<DailyWebPaymentStatistics>.Failed("این قابلیت در نسخه بعدی پیاده‌سازی خواهد شد", "NOT_IMPLEMENTED"));
        }

        #endregion
    }
}
