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
        private readonly IGatewayDriverFactory _driverFactory; // ✅ Gateway Driver Factory
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public WebPaymentService(
            IPaymentGatewayRepository paymentGatewayRepository,
            IOnlinePaymentRepository onlinePaymentRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            IPaymentService paymentService,
            IGatewayDriverFactory driverFactory, // ✅ Gateway Driver Factory
            ILogger logger)
        {
            _paymentGatewayRepository = paymentGatewayRepository ?? throw new ArgumentNullException(nameof(paymentGatewayRepository));
            _onlinePaymentRepository = onlinePaymentRepository ?? throw new ArgumentNullException(nameof(onlinePaymentRepository));
            _paymentTransactionRepository = paymentTransactionRepository ?? throw new ArgumentNullException(nameof(paymentTransactionRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _driverFactory = driverFactory ?? throw new ArgumentNullException(nameof(driverFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Payment Gateway Integration

        /// <summary>
        /// ایجاد درخواست پرداخت در درگاه
        /// </summary>
        public async Task<ServiceResult<PaymentGatewayResponse>> CreatePaymentRequestAsync(CreatePaymentRequest request)
        {
            var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString("N");
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.Information("💰 WEB PAYMENT REQUEST: شروع ایجاد درخواست پرداخت - GatewayType: {GatewayType}, Amount: {Amount}, OnlinePaymentId: {OnlinePaymentId}, CallbackUrl: {CallbackUrl}, CorrelationId: {CorrelationId}", 
                    request.GatewayType, request.Amount, request.OnlinePaymentId, request.CallbackUrl, correlationId);

                // اعتبارسنجی درخواست
                var validationResult = await ValidateCreatePaymentRequestAsync(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("⚠️ WEB PAYMENT VALIDATION: اعتبارسنجی درخواست پرداخت ناموفق - Message: {Message}, CorrelationId: {CorrelationId}", 
                        validationResult.Message, correlationId);
                    return ServiceResult<PaymentGatewayResponse>.Failed(validationResult.Message);
                }
                
                _logger.Information("✅ WEB PAYMENT VALIDATION: اعتبارسنجی موفق - Amount: {Amount}, CallbackUrl: {CallbackUrl}, CorrelationId: {CorrelationId}", 
                    request.Amount, request.CallbackUrl, correlationId);

                // ✅ CRITICAL FIX: استفاده از GetDefaultPaymentGatewayAsync به جای FirstOrDefault
                // این متد منطق کامل انتخاب Gateway را دارد (Default → ZarinPal → First Active)
                _logger.Information("🔍 WEB PAYMENT GATEWAY SELECTION: شروع انتخاب Gateway - CorrelationId: {CorrelationId}", correlationId);
                
                var gatewayResult = await GetDefaultPaymentGatewayAsync();
                if (!gatewayResult.Success || gatewayResult.Data == null)
                {
                    _logger.Error("❌ WEB PAYMENT GATEWAY SELECTION: درگاه پرداخت پیش‌فرض یافت نشد - ErrorMessage: {ErrorMessage}, CorrelationId: {CorrelationId}", 
                        gatewayResult.Message, correlationId);
                    return ServiceResult<PaymentGatewayResponse>.Failed(gatewayResult.Message ?? "درگاه پرداخت پیش‌فرض یافت نشد");
                }

                var gateway = gatewayResult.Data;
                
                _logger.Information("✅ WEB PAYMENT GATEWAY SELECTION: Gateway انتخاب شد - GatewayId: {GatewayId}, GatewayType: {GatewayType}, Name: {Name}, IsSandbox: {IsSandbox}, IsActive: {IsActive}, CorrelationId: {CorrelationId}", 
                    gateway.PaymentGatewayId, gateway.GatewayType, gateway.Name, gateway.IsTestMode, gateway.IsActive, correlationId);

                // ✅ بررسی GatewayType Match (اگر request.GatewayType مشخص شده باشد)
                if (request.GatewayType != PaymentGatewayType.ZarinPal && gateway.GatewayType != request.GatewayType)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: GatewayType mismatch - Request: {RequestType}, Gateway: {GatewayType}",
                        request.GatewayType, gateway.GatewayType);
                    // در این حالت، از Gateway یافت شده استفاده می‌کنیم (نه request.GatewayType)
                    // چون GetDefaultPaymentGatewayAsync بهترین Gateway را انتخاب کرده است
                }

                // ✅ بررسی Driver Support
                if (!_driverFactory.IsSupported(gateway.GatewayType))
                {
                    _logger.Error("❌ WEB PAYMENT DRIVER SUPPORT: GatewayType {GatewayType} پشتیبانی نمی‌شود - CorrelationId: {CorrelationId}", 
                        gateway.GatewayType, correlationId);
                    return ServiceResult<PaymentGatewayResponse>.Failed($"درگاه پرداخت {gateway.GatewayType} پشتیبانی نمی‌شود");
                }
                
                _logger.Information("✅ WEB PAYMENT DRIVER SUPPORT: Driver برای GatewayType {GatewayType} پشتیبانی می‌شود - CorrelationId: {CorrelationId}", 
                    gateway.GatewayType, correlationId);

                // ✅ ایجاد درخواست پرداخت در درگاه با استفاده از Driver
                _logger.Information("🔧 WEB PAYMENT DRIVER CALL: فراخوانی CreateGatewayPaymentRequestAsync - GatewayId: {GatewayId}, GatewayType: {GatewayType}, Amount: {Amount}, CallbackUrl: {CallbackUrl}, CorrelationId: {CorrelationId}",
                    gateway.PaymentGatewayId, gateway.GatewayType, request.Amount, request.CallbackUrl, correlationId);
                
                var gatewayResponse = await CreateGatewayPaymentRequestAsync(gateway, request);
                
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Information("📥 WEB PAYMENT DRIVER RESPONSE: پاسخ CreateGatewayPaymentRequestAsync - Success: {Success}, HasData: {HasData}, Message: {Message}, Code: {Code}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}",
                    gatewayResponse.Success, gatewayResponse.Data != null, gatewayResponse.Message, gatewayResponse.Code, processingTime, correlationId);
                
                if (!gatewayResponse.Success)
                {
                    _logger.Error("❌ WEB PAYMENT DRIVER ERROR: خطا در ایجاد درخواست پرداخت در درگاه - Success: {Success}, Message: {Message}, Code: {Code}, HasData: {HasData}, DataErrorCode: {DataErrorCode}, DataErrorMessage: {DataErrorMessage}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}",
                        gatewayResponse.Success, gatewayResponse.Message, gatewayResponse.Code, gatewayResponse.Data != null, 
                        gatewayResponse.Data?.ErrorCode, gatewayResponse.Data?.ErrorMessage, processingTime, correlationId);
                    
                    // ✅ CRITICAL FIX: برگرداندن پیام خطای دقیق‌تر از Driver
                    var errorMessage = gatewayResponse.Data?.ErrorMessage ?? gatewayResponse.Message ?? "خطا در ایجاد درخواست پرداخت در درگاه";
                    return ServiceResult<PaymentGatewayResponse>.Failed(errorMessage, gatewayResponse.Data?.ErrorCode ?? gatewayResponse.Code);
                }

                _logger.Information("✅ WEB PAYMENT SUCCESS: درخواست پرداخت با موفقیت در درگاه ایجاد شد - Authority: {Authority}, PaymentUrl: {PaymentUrl}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}", 
                    gatewayResponse.Data.GatewayTransactionId, gatewayResponse.Data.PaymentUrl, processingTime, correlationId);
                return ServiceResult<PaymentGatewayResponse>.Successful(gatewayResponse.Data, "درخواست پرداخت با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(ex, "❌ WEB PAYMENT EXCEPTION: خطای غیرمنتظره در CreatePaymentRequestAsync - ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}, GatewayType: {GatewayType}, Amount: {Amount}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}",
                    ex.GetType().Name, ex.Message, ex.StackTrace, request.GatewayType, request.Amount, processingTime, correlationId);
                
                if (ex.InnerException != null)
                {
                    _logger.Error("❌ WEB PAYMENT EXCEPTION INNER: InnerException - Type: {Type}, Message: {Message}, StackTrace: {StackTrace}, CorrelationId: {CorrelationId}",
                        ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace, correlationId);
                }
                
                // ✅ CRITICAL FIX: برگرداندن پیام خطای دقیق‌تر
                var errorMessage = $"خطا در ایجاد درخواست پرداخت در درگاه: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" (InnerException: {ex.InnerException.Message})";
                }
                
                return ServiceResult<PaymentGatewayResponse>.Failed(errorMessage, "PAYMENT_REQUEST_EXCEPTION");
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

                // ✅ CRITICAL FIX: استفاده از GetDefaultPaymentGatewayAsync برای انتخاب Gateway
                // یا جستجوی Gateway بر اساس نوع
                PaymentGateway gateway;
                if (gatewayType == PaymentGatewayType.ZarinPal)
                {
                    var gatewayResult = await GetDefaultPaymentGatewayAsync();
                    if (!gatewayResult.Success || gatewayResult.Data == null)
                    {
                        _logger.Warning("⚠️ WEB PAYMENT: درگاه پرداخت پیش‌فرض یافت نشد - {ErrorMessage}", gatewayResult.Message);
                        return ServiceResult<PaymentCallbackResult>.Failed(gatewayResult.Message ?? "درگاه پرداخت یافت نشد");
                    }
                    gateway = gatewayResult.Data;
                }
                else
                {
                    // ✅ برای Gateway های دیگر، جستجو بر اساس نوع
                    var gateways = await _paymentGatewayRepository.GetByTypeAsync(gatewayType);
                    gateway = gateways?.FirstOrDefault(g => g.IsActive && !g.IsDeleted);
                    
                    if (gateway == null)
                    {
                        _logger.Warning("⚠️ WEB PAYMENT: درگاه پرداخت {GatewayType} یافت نشد", gatewayType);
                        return ServiceResult<PaymentCallbackResult>.Failed("درگاه پرداخت یافت نشد");
                    }
                }

                // ✅ بررسی Driver Support
                if (!_driverFactory.IsSupported(gateway.GatewayType))
                {
                    _logger.Error("❌ WEB PAYMENT: GatewayType {GatewayType} پشتیبانی نمی‌شود", gateway.GatewayType);
                    return ServiceResult<PaymentCallbackResult>.Failed($"درگاه پرداخت {gateway.GatewayType} پشتیبانی نمی‌شود");
                }

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
            var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString("N");
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.Information("🔧 WEB PAYMENT GATEWAY REQUEST: شروع CreateGatewayPaymentRequestAsync - GatewayId: {GatewayId}, GatewayType: {GatewayType}, Amount: {Amount}, CallbackUrl: {CallbackUrl}, CorrelationId: {CorrelationId}",
                    gateway.PaymentGatewayId, gateway.GatewayType, request.Amount, request.CallbackUrl, correlationId);

                // ✅ CRITICAL: Validation قبل از فراخوانی Driver
                if (gateway == null)
                {
                    _logger.Error("❌ WEB PAYMENT: Gateway is null");
                    return ServiceResult<PaymentGatewayResponse>.Failed("درگاه پرداخت نامعتبر است");
                }

                if (!gateway.IsActive)
                {
                    _logger.Error("❌ WEB PAYMENT: Gateway is not active - GatewayId: {GatewayId}", gateway.PaymentGatewayId);
                    return ServiceResult<PaymentGatewayResponse>.Failed("درگاه پرداخت غیرفعال است");
                }

                if (gateway.IsDeleted)
                {
                    _logger.Error("❌ WEB PAYMENT: Gateway is deleted - GatewayId: {GatewayId}", gateway.PaymentGatewayId);
                    return ServiceResult<PaymentGatewayResponse>.Failed("درگاه پرداخت حذف شده است");
                }

                // ✅ Validation CallbackUrl
                if (string.IsNullOrWhiteSpace(request.CallbackUrl))
                {
                    _logger.Error("❌ WEB PAYMENT: CallbackUrl is null or empty");
                    return ServiceResult<PaymentGatewayResponse>.Failed("آدرس Callback الزامی است");
                }

                if (!Uri.IsWellFormedUriString(request.CallbackUrl, UriKind.Absolute))
                {
                    _logger.Error("❌ WEB PAYMENT: CallbackUrl is not a valid absolute URI - CallbackUrl: {CallbackUrl}", request.CallbackUrl);
                    return ServiceResult<PaymentGatewayResponse>.Failed("آدرس Callback نامعتبر است");
                }

                // ✅ Validation Amount
                if (request.Amount <= 0)
                {
                    _logger.Error("❌ WEB PAYMENT: Amount is invalid - Amount: {Amount}", request.Amount);
                    return ServiceResult<PaymentGatewayResponse>.Failed("مبلغ پرداخت باید بیشتر از صفر باشد");
                }

                if (request.Amount < 1000)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: Amount is less than minimum (1000) - Amount: {Amount}", request.Amount);
                    // ادامه می‌دهیم - Driver خودش validation می‌کند
                }

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
                    AdditionalData = request.AdditionalData,
                    CorrelationId = request.CorrelationId // ✅ ENTERPRISE-GRADE: انتقال CorrelationId به Driver
                };

                // ✅ BEST PRACTICE: انتخاب Driver بر اساس PaymentGateway Entity از Factory
                var driver = _driverFactory.GetDriver(gateway);
                _logger.Information("🔧 WEB PAYMENT DRIVER SELECTED: Driver انتخاب شد از Entity - GatewayId: {GatewayId}, GatewayType: {GatewayType}, Amount: {Amount}, CallbackUrl: {CallbackUrl}, Description: {Description}, Mobile: {Mobile}, Email: {Email}, CorrelationId: {CorrelationId}",
                    gateway.PaymentGatewayId, gateway.GatewayType, driverRequest.Amount, driverRequest.CallbackUrl, driverRequest.Description, driverRequest.Mobile, driverRequest.Email, correlationId);
                
                var driverCallTime = DateTime.UtcNow;
                var driverResult = await driver.RequestPaymentAsync(driverRequest);
                var driverResponseTime = DateTime.UtcNow;
                var driverDuration = (driverResponseTime - driverCallTime).TotalMilliseconds;
                
                _logger.Information("🔧 WEB PAYMENT DRIVER RESPONSE: Driver Response - Success: {Success}, Message: {Message}, HasData: {HasData}, DataSuccess: {DataSuccess}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                    driverResult.Success, driverResult.Message, driverResult.Data != null, 
                    driverResult.Data?.Success, driverResult.Data?.ErrorCode, driverResult.Data?.ErrorMessage, driverDuration, correlationId);
                
                if (!driverResult.Success || driverResult.Data == null)
                {
                    _logger.Error("❌ WEB PAYMENT DRIVER FAILED: Driver درخواست پرداخت ناموفق - Success: {Success}, Message: {Message}, HasData: {HasData}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                        driverResult.Success, driverResult.Message, driverResult.Data != null, driverDuration, correlationId);
                    
                    // ✅ CRITICAL FIX: اگر Exception در InnerException است، آن را لاگ می‌کنیم
                    if (driverResult.Data != null && driverResult.Data.ErrorMessage != null)
                    {
                        _logger.Error("❌ WEB PAYMENT DRIVER ERROR DETAILS: Driver Error Details - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, CorrelationId: {CorrelationId}",
                            driverResult.Data.ErrorCode, driverResult.Data.ErrorMessage, correlationId);
                        
                        // ✅ برگرداندن پیام خطای دقیق‌تر
                        return ServiceResult<PaymentGatewayResponse>.Failed(
                            driverResult.Data.ErrorMessage ?? driverResult.Message ?? "خطا در درخواست پرداخت",
                            driverResult.Data.ErrorCode);
                    }
                    
                    return ServiceResult<PaymentGatewayResponse>.Failed(
                        driverResult.Message ?? "خطا در درخواست پرداخت");
                }

                // ✅ بررسی Success flag در Data
                if (!driverResult.Data.Success)
                {
                    _logger.Error("❌ WEB PAYMENT DRIVER DATA FAILED: Driver Data.Success is false - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, CorrelationId: {CorrelationId}",
                        driverResult.Data.ErrorCode, driverResult.Data.ErrorMessage, correlationId);
                    
                    return ServiceResult<PaymentGatewayResponse>.Failed(
                        driverResult.Data.ErrorMessage ?? "خطا در درخواست پرداخت",
                        driverResult.Data.ErrorCode);
                }

                // ✅ Validation PaymentUrl
                if (string.IsNullOrWhiteSpace(driverResult.Data.PaymentUrl))
                {
                    _logger.Error("❌ WEB PAYMENT VALIDATION: PaymentUrl is null or empty - Authority: {Authority}, CorrelationId: {CorrelationId}",
                        driverResult.Data.Authority, correlationId);
                    return ServiceResult<PaymentGatewayResponse>.Failed("آدرس درگاه پرداخت دریافت نشد");
                }

                // ✅ Validation Authority
                if (string.IsNullOrWhiteSpace(driverResult.Data.Authority))
                {
                    _logger.Error("❌ WEB PAYMENT VALIDATION: Authority is null or empty - PaymentUrl: {PaymentUrl}, CorrelationId: {CorrelationId}",
                        driverResult.Data.PaymentUrl, correlationId);
                    return ServiceResult<PaymentGatewayResponse>.Failed("کد Authority دریافت نشد");
                }
                
                _logger.Information("✅ WEB PAYMENT VALIDATION: PaymentUrl و Authority معتبر هستند - Authority: {Authority}, PaymentUrl: {PaymentUrl}, CorrelationId: {CorrelationId}",
                    driverResult.Data.Authority, driverResult.Data.PaymentUrl, correlationId);

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

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Information("✅ WEB PAYMENT GATEWAY SUCCESS: Driver درخواست پرداخت موفق - Authority: {Authority}, PaymentUrl: {PaymentUrl}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}", 
                    response.GatewayTransactionId, response.PaymentUrl, processingTime, correlationId);

                return ServiceResult<PaymentGatewayResponse>.Successful(response);
            }
            catch (Exception ex)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(ex, "❌ WEB PAYMENT GATEWAY EXCEPTION: خطای غیرمنتظره در CreateGatewayPaymentRequestAsync - ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}, GatewayId: {GatewayId}, GatewayType: {GatewayType}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}",
                    ex.GetType().Name, ex.Message, ex.StackTrace, gateway?.PaymentGatewayId, gateway?.GatewayType, processingTime, correlationId);
                
                if (ex.InnerException != null)
                {
                    _logger.Error("❌ WEB PAYMENT GATEWAY EXCEPTION INNER: InnerException - Type: {Type}, Message: {Message}, StackTrace: {StackTrace}, CorrelationId: {CorrelationId}",
                        ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace, correlationId);
                }
                
                // ✅ CRITICAL FIX: برگرداندن پیام خطای دقیق‌تر
                var errorMessage = $"خطا در ایجاد درخواست پرداخت در درگاه: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" (InnerException: {ex.InnerException.Message})";
                }
                
                return ServiceResult<PaymentGatewayResponse>.Failed(errorMessage, "GATEWAY_REQUEST_EXCEPTION");
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

                // ✅ BEST PRACTICE: انتخاب Driver بر اساس PaymentGateway Entity از Factory
                var driver = _driverFactory.GetDriver(gateway);
                
                // ✅ فراخوانی Driver برای Verify
                var verifyRequest = new PaymentVerificationRequest
                {
                    Authority = callbackData.PaymentToken ?? callbackData.TransactionId, // Authority = PaymentToken
                    Amount = callbackData.Amount ?? onlinePayment.Amount,
                    AdditionalData = callbackData.AdditionalData
                };
                var verifyResult = await driver.VerifyPaymentAsync(verifyRequest);

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

                // ✅ دریافت OnlinePayment برای Amount و GatewayType
                var onlinePayment = await _onlinePaymentRepository.GetByPaymentTokenAsync(transactionId);
                if (onlinePayment == null)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: OnlinePayment با TransactionId {TransactionId} یافت نشد", transactionId);
                    return ServiceResult<PaymentStatus>.Failed("پرداخت یافت نشد");
                }

                // ✅ دریافت Gateway برای GatewayType
                var gateway = onlinePayment.PaymentGateway;
                if (gateway == null)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: Gateway برای OnlinePayment {OnlinePaymentId} یافت نشد", onlinePayment.OnlinePaymentId);
                    return ServiceResult<PaymentStatus>.Failed("درگاه پرداخت یافت نشد");
                }

                // ✅ بررسی Driver Support
                if (!_driverFactory.IsSupported(gateway.GatewayType))
                {
                    _logger.Error("❌ WEB PAYMENT: GatewayType {GatewayType} پشتیبانی نمی‌شود", gateway.GatewayType);
                    return ServiceResult<PaymentStatus>.Failed($"درگاه پرداخت {gateway.GatewayType} پشتیبانی نمی‌شود");
                }

                // ✅ BEST PRACTICE: انتخاب Driver بر اساس PaymentGateway Entity از Factory
                var driver = _driverFactory.GetDriver(gateway);
                
                // ✅ استفاده از Driver برای بررسی وضعیت
                var statusResult = await driver.CheckPaymentStatusAsync(transactionId, onlinePayment.Amount);
                
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

                // ✅ دریافت Gateway برای GatewayType
                var gateway = onlinePayment.PaymentGateway;
                if (gateway == null)
                {
                    _logger.Warning("⚠️ WEB PAYMENT: Gateway برای OnlinePayment {OnlinePaymentId} یافت نشد", onlinePayment.OnlinePaymentId);
                    return ServiceResult<WebRefundResult>.Failed("درگاه پرداخت یافت نشد");
                }

                // ✅ بررسی Driver Support
                if (!_driverFactory.IsSupported(gateway.GatewayType))
                {
                    _logger.Error("❌ WEB PAYMENT: GatewayType {GatewayType} پشتیبانی نمی‌شود", gateway.GatewayType);
                    return ServiceResult<WebRefundResult>.Failed($"درگاه پرداخت {gateway.GatewayType} پشتیبانی نمی‌شود");
                }

                // ✅ BEST PRACTICE: انتخاب Driver بر اساس PaymentGateway Entity از Factory
                var driver = _driverFactory.GetDriver(gateway);

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
                var refundResult = await driver.RefundPaymentAsync(refundRequest);

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
            try
            {
                _logger.Information("🔍 WEB PAYMENT: شروع جستجوی درگاه پرداخت پیش‌فرض...");

                // ✅ STEP 1: جستجوی درگاه پیش‌فرض (IsDefault = true)
                var defaultGateways = await _paymentGatewayRepository.GetDefaultGatewaysAsync();
                var defaultGateway = defaultGateways?.FirstOrDefault();
                
                _logger.Debug("🔍 WEB PAYMENT: STEP 1 - تعداد درگاه‌های پیش‌فرض: {Count}, اولین درگاه: {GatewayId}",
                    defaultGateways?.Count() ?? 0, defaultGateway?.PaymentGatewayId);

                if (defaultGateway != null && defaultGateway.IsActive && !defaultGateway.IsDeleted)
                {
                    _logger.Information("✅ WEB PAYMENT: درگاه پیش‌فرض یافت شد - GatewayId: {GatewayId}, Name: {Name}, Type: {Type}",
                        defaultGateway.PaymentGatewayId, defaultGateway.Name, defaultGateway.GatewayType);
                    return ServiceResult<PaymentGateway>.Successful(defaultGateway);
                }

                // ✅ STEP 2: اگر درگاه پیش‌فرض یافت نشد، جستجوی درگاه ZarinPal فعال
                _logger.Debug("⚠️ WEB PAYMENT: درگاه پیش‌فرض یافت نشد. جستجوی درگاه ZarinPal فعال...");
                var zarinPalGateways = await _paymentGatewayRepository.GetByTypeAsync(PaymentGatewayType.ZarinPal);
                var activeZarinPalGateway = zarinPalGateways?.FirstOrDefault(g => g.IsActive && !g.IsDeleted);
                
                _logger.Debug("🔍 WEB PAYMENT: STEP 2 - تعداد درگاه‌های ZarinPal: {Count}, اولین درگاه فعال: {GatewayId}, IsActive: {IsActive}, IsDeleted: {IsDeleted}",
                    zarinPalGateways?.Count() ?? 0, 
                    activeZarinPalGateway?.PaymentGatewayId,
                    activeZarinPalGateway?.IsActive,
                    activeZarinPalGateway?.IsDeleted);

                if (activeZarinPalGateway != null)
                {
                    _logger.Information("✅ WEB PAYMENT: درگاه ZarinPal فعال یافت شد - GatewayId: {GatewayId}, Name: {Name}",
                        activeZarinPalGateway.PaymentGatewayId, activeZarinPalGateway.Name);
                    return ServiceResult<PaymentGateway>.Successful(activeZarinPalGateway);
                }

                // ✅ STEP 3: اگر ZarinPal یافت نشد، جستجوی اولین درگاه فعال
                _logger.Debug("⚠️ WEB PAYMENT: درگاه ZarinPal یافت نشد. جستجوی اولین درگاه فعال...");
                var activeGateways = await _paymentGatewayRepository.GetActiveGatewaysAsync();
                var firstActiveGateway = activeGateways?.FirstOrDefault();
                
                _logger.Debug("🔍 WEB PAYMENT: STEP 3 - تعداد درگاه‌های فعال: {Count}, اولین درگاه: {GatewayId}, Type: {Type}",
                    activeGateways?.Count() ?? 0, 
                    firstActiveGateway?.PaymentGatewayId,
                    firstActiveGateway?.GatewayType);

                if (firstActiveGateway != null)
                {
                    _logger.Information("✅ WEB PAYMENT: اولین درگاه فعال یافت شد - GatewayId: {GatewayId}, Name: {Name}, Type: {Type}",
                        firstActiveGateway.PaymentGatewayId, firstActiveGateway.Name, firstActiveGateway.GatewayType);
                    return ServiceResult<PaymentGateway>.Successful(firstActiveGateway);
                }

                // ✅ STEP 4: اگر هیچ درگاهی یافت نشد، تلاش برای ایجاد خودکار از Web.config
                _logger.Warning("⚠️ WEB PAYMENT: هیچ درگاه پرداخت فعالی یافت نشد. تلاش برای ایجاد خودکار از Web.config...");
                
                try
                {
                    // ✅ CRITICAL FIX: تنظیم CallbackUrl پیش‌فرض (یک بار تعریف می‌شود و در کل scope استفاده می‌شود)
                    // این URL نسبی است و در Controller به URL کامل تبدیل می‌شود
                    var defaultCallbackUrl = "/Patient/AppointmentBooking/PaymentCallback";
                    
                    // ✅ تلاش برای خواندن Merchant ID از Web.config
                    var merchantId = ZarinPalHelper.GetMerchantId();
                    _logger.Information("🔍 WEB PAYMENT: STEP 4 - MerchantId از Web.config: {MerchantId} (Length: {Length})",
                        merchantId?.Substring(0, Math.Min(10, merchantId?.Length ?? 0)) + "...",
                        merchantId?.Length ?? 0);
                    
                    if (!string.IsNullOrWhiteSpace(merchantId))
                    {
                        // ✅ بررسی اینکه آیا درگاه با این Merchant ID وجود دارد
                        var existingGateway = await _paymentGatewayRepository.GetByMerchantIdAsync(merchantId);
                        _logger.Debug("🔍 WEB PAYMENT: STEP 4.1 - بررسی درگاه موجود با MerchantId: {MerchantId}, یافت شد: {Found}, GatewayId: {GatewayId}, IsDeleted: {IsDeleted}",
                            merchantId?.Substring(0, Math.Min(10, merchantId?.Length ?? 0)) + "...",
                            existingGateway != null,
                            existingGateway?.PaymentGatewayId,
                            existingGateway?.IsDeleted);
                        
                        if (existingGateway != null)
                        {
                            // ✅ CRITICAL FIX: اگر CallbackUrl خالی است، آن را تنظیم می‌کنیم
                            var needsUpdate = false;
                            
                            if (string.IsNullOrWhiteSpace(existingGateway.CallbackUrl))
                            {
                                existingGateway.CallbackUrl = defaultCallbackUrl;
                                needsUpdate = true;
                                _logger.Warning("⚠️ WEB PAYMENT: CallbackUrl برای درگاه {GatewayId} خالی بود، تنظیم شد", existingGateway.PaymentGatewayId);
                            }
                            
                            // ✅ اگر درگاه وجود دارد اما غیرفعال است، فعال می‌کنیم
                            if (!existingGateway.IsActive)
                            {
                                existingGateway.IsActive = true;
                                needsUpdate = true;
                                _logger.Information("✅ WEB PAYMENT: درگاه با MerchantId {MerchantId} فعال شد", merchantId);
                            }
                            
                            if (needsUpdate)
                            {
                                await _paymentGatewayRepository.UpdateAsync(existingGateway);
                                _logger.Information("✅ WEB PAYMENT: درگاه با MerchantId {MerchantId} به‌روزرسانی شد", merchantId);
                            }
                            
                            if (!existingGateway.IsDeleted)
                            {
                                _logger.Information("✅ WEB PAYMENT: درگاه با MerchantId {MerchantId} یافت شد - GatewayId: {GatewayId}, CallbackUrl: {CallbackUrl}", 
                                    merchantId, existingGateway.PaymentGatewayId, existingGateway.CallbackUrl);
                                return ServiceResult<PaymentGateway>.Successful(existingGateway);
                            }
                        }
                        
                        // ✅ اگر درگاه وجود ندارد، ایجاد می‌کنیم
                        var isSandbox = ZarinPalHelper.IsSandbox();
                        
                        var newGateway = new PaymentGateway
                        {
                            Name = isSandbox ? "زرین‌پال (Sandbox)" : "زرین‌پال (Production)",
                            GatewayType = PaymentGatewayType.ZarinPal,
                            MerchantId = merchantId,
                            ApiKey = merchantId, // برای ZarinPal، ApiKey همان MerchantId است
                            GatewayUrl = ZarinPalHelper.GetStartPayUrl(), // ✅ URL برای redirect به درگاه
                            CallbackUrl = defaultCallbackUrl, // ✅ CRITICAL FIX: تنظیم CallbackUrl پیش‌فرض (در Controller به URL کامل تبدیل می‌شود)
                            IsActive = true,
                            IsDefault = true, // ✅ به عنوان پیش‌فرض تنظیم می‌شود
                            Description = $"درگاه پرداخت زرین‌پال - ایجاد شده خودکار از Web.config (Sandbox: {isSandbox})",
                            CreatedByUserId = null, // ✅ CRITICAL FIX: null چون این درگاه خودکار است و User ID واقعی نداریم
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        // ✅ پاک کردن درگاه‌های پیش‌فرض قبلی
                        await _paymentGatewayRepository.ClearDefaultGatewaysAsync();
                        _logger.Debug("🔍 WEB PAYMENT: STEP 4.2 - درگاه‌های پیش‌فرض قبلی پاک شدند");
                        
                        _logger.Information("🔍 WEB PAYMENT: STEP 4.3 - ایجاد درگاه جدید - Name: {Name}, MerchantId: {MerchantId}, CallbackUrl: {CallbackUrl}, IsSandbox: {IsSandbox}",
                            newGateway.Name,
                            merchantId?.Substring(0, Math.Min(10, merchantId?.Length ?? 0)) + "...",
                            newGateway.CallbackUrl,
                            isSandbox);
                        
                        var createdGateway = await _paymentGatewayRepository.CreateAsync(newGateway);
                        _logger.Information("✅ WEB PAYMENT: درگاه پرداخت زرین‌پال به صورت خودکار ایجاد شد - GatewayId: {GatewayId}, MerchantId: {MerchantId}, CallbackUrl: {CallbackUrl}", 
                            createdGateway.PaymentGatewayId, 
                            merchantId?.Substring(0, Math.Min(10, merchantId?.Length ?? 0)) + "...",
                            createdGateway.CallbackUrl);
                        
                        return ServiceResult<PaymentGateway>.Successful(createdGateway);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "❌ WEB PAYMENT: خطا در ایجاد خودکار درگاه از Web.config - ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                        ex.GetType().Name, ex.Message, ex.StackTrace);
                    
                    // ✅ CRITICAL FIX: اگر خطای Validation است، جزئیات بیشتری لاگ می‌کنیم
                    if (ex is System.Data.Entity.Validation.DbEntityValidationException validationEx)
                    {
                        foreach (var validationError in validationEx.EntityValidationErrors)
                        {
                            foreach (var error in validationError.ValidationErrors)
                            {
                                _logger.Error("❌ WEB PAYMENT: Validation Error - Property: {Property}, Error: {Error}",
                                    error.PropertyName, error.ErrorMessage);
                            }
                        }
                    }
                    
                    // ادامه می‌دهیم و خطا را برمی‌گردانیم
                }
                
                // ❌ STEP 5: اگر همه تلاش‌ها ناموفق بود
                _logger.Error("❌ WEB PAYMENT: هیچ درگاه پرداخت فعالی یافت نشد و امکان ایجاد خودکار وجود ندارد");
                return ServiceResult<PaymentGateway>.Failed("درگاه پرداخت پیش‌فرض یافت نشد. لطفاً با پشتیبانی تماس بگیرید", "GATEWAY_NOT_FOUND");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ WEB PAYMENT: خطا در دریافت درگاه پرداخت پیش‌فرض");
                return ServiceResult<PaymentGateway>.Failed("خطا در دریافت درگاه پرداخت. لطفاً دوباره تلاش کنید", "GATEWAY_ERROR");
            }
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
