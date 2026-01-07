using ClinicApp.Interfaces.Payment.Gateway.Drivers;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using ClinicApp.Helpers;

namespace ClinicApp.Services.Payment.Gateway.Drivers
{
    /// <summary>
    /// درایور شبیه‌سازی شده برای تست و توسعه
    /// این درایور بدون نیاز به اتصال واقعی به درگاه پرداخت، فرآیند پرداخت را شبیه‌سازی می‌کند
    /// 
    /// ویژگی‌ها:
    /// - همیشه موفق برمی‌گرداند
    /// - ایجاد Authority و PaymentUrl شبیه‌سازی شده
    /// - صفحه شبیه‌سازی شده برای تست UI
    /// - لاگ‌گذاری کامل برای Debug
    /// 
    /// استفاده:
    /// - تست فرآیند پرداخت بدون نیاز به درگاه واقعی
    /// - توسعه و Debug
    /// - نمایش Demo
    /// </summary>
    public class SimulatedGatewayDriver : IGatewayDriver
    {
        #region Fields

        private readonly PaymentGateway _gateway;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gateway">PaymentGateway Entity</param>
        /// <param name="logger">Logger</param>
        public SimulatedGatewayDriver(PaymentGateway gateway, ILogger logger)
        {
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.Information("✅ SimulatedGatewayDriver initialized - GatewayId: {GatewayId}, Name: {Name}, IsTestMode: {IsTestMode}",
                _gateway.PaymentGatewayId, _gateway.Name, _gateway.IsTestMode);
        }

        #endregion

        #region IGatewayDriver Implementation

        /// <summary>
        /// ایجاد درخواست پرداخت شبیه‌سازی شده
        /// </summary>
        public async Task<ServiceResult<PaymentRequestResult>> RequestPaymentAsync(PaymentRequest request)
        {
            var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString("N");
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.Information("🎭 SIMULATED REQUEST: شروع درخواست پرداخت شبیه‌سازی شده - Amount: {Amount}, Description: {Description}, CallbackUrl: {CallbackUrl}, CorrelationId: {CorrelationId}",
                    request.Amount, request.Description, request.CallbackUrl, correlationId);

                // ✅ Validation
                if (request == null)
                {
                    _logger.Error("❌ SIMULATED REQUEST: PaymentRequest is null");
                    return ServiceResult<PaymentRequestResult>.Failed("درخواست پرداخت نمی‌تواند خالی باشد");
                }

                if (request.Amount <= 0)
                {
                    _logger.Error("❌ SIMULATED REQUEST: Amount is invalid - Amount: {Amount}", request.Amount);
                    return ServiceResult<PaymentRequestResult>.Failed("مبلغ پرداخت باید بیشتر از صفر باشد");
                }

                if (string.IsNullOrWhiteSpace(request.CallbackUrl))
                {
                    _logger.Error("❌ SIMULATED REQUEST: CallbackUrl is null or empty");
                    return ServiceResult<PaymentRequestResult>.Failed("آدرس Callback الزامی است");
                }

                // ✅ ایجاد Authority شبیه‌سازی شده
                // GUID با فرمت "N" = 32 کاراکتر (نه 36!)
                var authority = Guid.NewGuid().ToString("N"); // 32 کاراکتر (مشابه ZarinPal Authority)
                
                // ✅ CRITICAL FIX: ساخت URL در زمان Request (نه در Constructor)
                // استفاده از HttpContext.Current برای ساخت URL کامل
                string paymentUrl;
                try
                {
                    var httpContext = HttpContext.Current;
                    if (httpContext != null && httpContext.Request != null && httpContext.Request.Url != null)
                    {
                        var baseUrl = $"{httpContext.Request.Url.Scheme}://{httpContext.Request.Url.Authority}";
                        var escapedCallbackUrl = Uri.EscapeDataString(request.CallbackUrl ?? string.Empty);
                        paymentUrl = $"{baseUrl}/Payment/SimulatedGateway/Process?authority={authority}&amount={request.Amount}&callbackUrl={escapedCallbackUrl}&correlationId={correlationId}";
                        _logger.Information("🎭 SIMULATED REQUEST: PaymentUrl از HttpContext ساخته شد - BaseUrl: {BaseUrl}, PaymentUrl: {PaymentUrl}, CorrelationId: {CorrelationId}",
                            baseUrl, paymentUrl, correlationId);
                    }
                    else
                    {
                        // ✅ Fallback: استفاده از URL نسبی (در Controller کامل می‌شود)
                        var escapedCallbackUrl = Uri.EscapeDataString(request.CallbackUrl ?? string.Empty);
                        paymentUrl = $"/Payment/SimulatedGateway/Process?authority={authority}&amount={request.Amount}&callbackUrl={escapedCallbackUrl}&correlationId={correlationId}";
                        _logger.Warning("⚠️ SIMULATED REQUEST: HttpContext.Current null است، استفاده از URL نسبی - PaymentUrl: {PaymentUrl}, CorrelationId: {CorrelationId}",
                            paymentUrl, correlationId);
                    }
                }
                catch (Exception urlEx)
                {
                    _logger.Error(urlEx, "❌ SIMULATED REQUEST: خطا در ساخت PaymentUrl - ExceptionType: {ExceptionType}, Message: {Message}, CorrelationId: {CorrelationId}",
                        urlEx.GetType().Name, urlEx.Message, correlationId);
                    // ✅ Fallback: استفاده از URL نسبی ساده
                    paymentUrl = $"/Payment/SimulatedGateway/Process?authority={authority}&amount={request.Amount}&correlationId={correlationId}";
                    _logger.Warning("⚠️ SIMULATED REQUEST: استفاده از URL نسبی ساده (بدون CallbackUrl) - PaymentUrl: {PaymentUrl}, CorrelationId: {CorrelationId}",
                        paymentUrl, correlationId);
                }

                _logger.Information("🎭 SIMULATED REQUEST: Authority و PaymentUrl ایجاد شد - Authority: {Authority}, PaymentUrl: {PaymentUrl}, CorrelationId: {CorrelationId}",
                    authority, paymentUrl, correlationId);

                // ✅ ایجاد نتیجه موفق
                var result = new PaymentRequestResult
                {
                    Success = true,
                    Authority = authority,
                    PaymentToken = authority,
                    PaymentUrl = paymentUrl,
                    GatewayTransactionId = authority,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "Simulated", "true" },
                        { "CorrelationId", correlationId },
                        { "RequestTime", startTime.ToString("O") }
                    }
                };

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Information("✅ SIMULATED SUCCESS: درخواست پرداخت شبیه‌سازی شده موفق - Authority: {Authority}, PaymentUrl: {PaymentUrl}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}",
                    result.Authority, result.PaymentUrl, processingTime, correlationId);

                return ServiceResult<PaymentRequestResult>.Successful(result, "درخواست پرداخت شبیه‌سازی شده با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(ex, "❌ SIMULATED EXCEPTION: خطای غیرمنتظره در درخواست پرداخت شبیه‌سازی شده - ExceptionType: {ExceptionType}, Message: {Message}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}",
                    ex.GetType().Name, ex.Message, processingTime, correlationId);

                return ServiceResult<PaymentRequestResult>.Failed("خطا در ایجاد درخواست پرداخت شبیه‌سازی شده");
            }
        }

        /// <summary>
        /// تأیید پرداخت شبیه‌سازی شده
        /// همیشه موفق برمی‌گرداند
        /// </summary>
        public async Task<ServiceResult<PaymentVerificationResult>> VerifyPaymentAsync(PaymentVerificationRequest request)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.Information("🎭 SIMULATED VERIFY: شروع تأیید پرداخت شبیه‌سازی شده - Authority: {Authority}, Amount: {Amount}",
                    request.Authority, request.Amount);

                // ✅ Validation
                if (request == null)
                {
                    _logger.Error("❌ SIMULATED VERIFY: PaymentVerificationRequest is null");
                    return ServiceResult<PaymentVerificationResult>.Failed("درخواست تأیید پرداخت نمی‌تواند خالی باشد");
                }

                if (string.IsNullOrWhiteSpace(request.Authority))
                {
                    _logger.Error("❌ SIMULATED VERIFY: Authority is null or empty");
                    return ServiceResult<PaymentVerificationResult>.Failed("کد Authority الزامی است");
                }

                // ✅ همیشه موفق برمی‌گرداند
                var refId = "SIM-" + new Random().Next(100000, 999999).ToString();

                var result = new PaymentVerificationResult
                {
                    Success = true,
                    IsVerified = true,
                    Authority = request.Authority,
                    PaymentToken = request.Authority,
                    RefId = refId,
                    GatewayTransactionId = request.Authority,
                    Amount = request.Amount,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "Simulated", "true" },
                        { "VerifyTime", DateTime.UtcNow.ToString("O") }
                    }
                };

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Information("✅ SIMULATED VERIFY SUCCESS: تأیید پرداخت شبیه‌سازی شده موفق - Authority: {Authority}, RefId: {RefId}, ProcessingTime: {ProcessingTime}ms",
                    result.Authority, result.RefId, processingTime);

                return ServiceResult<PaymentVerificationResult>.Successful(result, "پرداخت شبیه‌سازی شده با موفقیت تأیید شد");
            }
            catch (Exception ex)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(ex, "❌ SIMULATED VERIFY EXCEPTION: خطای غیرمنتظره در تأیید پرداخت شبیه‌سازی شده - ExceptionType: {ExceptionType}, Message: {Message}, ProcessingTime: {ProcessingTime}ms",
                    ex.GetType().Name, ex.Message, processingTime);

                return ServiceResult<PaymentVerificationResult>.Failed("خطا در تأیید پرداخت شبیه‌سازی شده");
            }
        }

        /// <summary>
        /// بررسی وضعیت پرداخت شبیه‌سازی شده
        /// همیشه موفق برمی‌گرداند
        /// </summary>
        public async Task<ServiceResult<PaymentStatusResult>> CheckPaymentStatusAsync(string transactionId, decimal amount)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.Information("🎭 SIMULATED STATUS: بررسی وضعیت پرداخت شبیه‌سازی شده - TransactionId: {TransactionId}, Amount: {Amount}",
                    transactionId, amount);

                // ✅ Validation
                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    _logger.Error("❌ SIMULATED STATUS: TransactionId is null or empty");
                    return ServiceResult<PaymentStatusResult>.Failed("شناسه تراکنش الزامی است");
                }

                // ✅ همیشه موفق برمی‌گرداند
                var result = new PaymentStatusResult
                {
                    Success = true,
                    TransactionId = transactionId,
                    Status = "Paid", // همیشه Paid
                    Amount = amount,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "Simulated", "true" },
                        { "CheckTime", DateTime.UtcNow.ToString("O") }
                    }
                };

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Information("✅ SIMULATED STATUS SUCCESS: بررسی وضعیت پرداخت شبیه‌سازی شده موفق - TransactionId: {TransactionId}, Status: {Status}, ProcessingTime: {ProcessingTime}ms",
                    result.TransactionId, result.Status, processingTime);

                return ServiceResult<PaymentStatusResult>.Successful(result, "وضعیت پرداخت شبیه‌سازی شده بررسی شد");
            }
            catch (Exception ex)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(ex, "❌ SIMULATED STATUS EXCEPTION: خطای غیرمنتظره در بررسی وضعیت پرداخت شبیه‌سازی شده - ExceptionType: {ExceptionType}, Message: {Message}, ProcessingTime: {ProcessingTime}ms",
                    ex.GetType().Name, ex.Message, processingTime);

                return ServiceResult<PaymentStatusResult>.Failed("خطا در بررسی وضعیت پرداخت شبیه‌سازی شده");
            }
        }

        /// <summary>
        /// برگشت وجه شبیه‌سازی شده
        /// همیشه موفق برمی‌گرداند
        /// </summary>
        public async Task<ServiceResult<RefundResult>> RefundPaymentAsync(RefundRequest request)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.Information("🎭 SIMULATED REFUND: شروع برگشت وجه شبیه‌سازی شده - TransactionId: {TransactionId}, Amount: {Amount}, Reason: {Reason}",
                    request.TransactionId, request.Amount, request.Reason);

                // ✅ Validation
                if (request == null)
                {
                    _logger.Error("❌ SIMULATED REFUND: RefundRequest is null");
                    return ServiceResult<RefundResult>.Failed("درخواست برگشت وجه نمی‌تواند خالی باشد");
                }

                if (string.IsNullOrWhiteSpace(request.TransactionId))
                {
                    _logger.Error("❌ SIMULATED REFUND: TransactionId is null or empty");
                    return ServiceResult<RefundResult>.Failed("شناسه تراکنش الزامی است");
                }

                // ✅ همیشه موفق برمی‌گرداند
                var refundId = "REFUND-SIM-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

                var result = new RefundResult
                {
                    Success = true,
                    RefundId = refundId,
                    GatewayRefundId = refundId,
                    RefundAmount = request.Amount,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "Simulated", "true" },
                        { "RefundTime", DateTime.UtcNow.ToString("O") },
                        { "Reason", request.Reason ?? "Test Refund" }
                    }
                };

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Information("✅ SIMULATED REFUND SUCCESS: برگشت وجه شبیه‌سازی شده موفق - RefundId: {RefundId}, Amount: {Amount}, ProcessingTime: {ProcessingTime}ms",
                    result.RefundId, result.RefundAmount, processingTime);

                return ServiceResult<RefundResult>.Successful(result, "برگشت وجه شبیه‌سازی شده با موفقیت انجام شد");
            }
            catch (Exception ex)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(ex, "❌ SIMULATED REFUND EXCEPTION: خطای غیرمنتظره در برگشت وجه شبیه‌سازی شده - ExceptionType: {ExceptionType}, Message: {Message}, ProcessingTime: {ProcessingTime}ms",
                    ex.GetType().Name, ex.Message, processingTime);

                return ServiceResult<RefundResult>.Failed("خطا در برگشت وجه شبیه‌سازی شده");
            }
        }

        #endregion
    }
}

