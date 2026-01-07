using ClinicApp.Helpers;
using ClinicApp.Interfaces.Payment.Gateway.Drivers;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Payment;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Payment;
using PaymentRequest = ClinicApp.Interfaces.Payment.Gateway.Drivers.PaymentRequest;

namespace ClinicApp.Services.Payment.Gateway.Drivers
{
    /// <summary>
    /// Driver برای درگاه پرداخت زرین‌پال
    /// طراحی شده طبق اصول SRP - مسئولیت: ارتباط با API زرین‌پال
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. Payment Request (درخواست پرداخت)
    /// 2. Payment Verification (تأیید پرداخت)
    /// 3. Payment Status Check (بررسی وضعیت)
    /// 4. Refund (برگشت وجه - در صورت پشتیبانی)
    /// 5. ✅ BEST PRACTICE: استفاده از PaymentGateway Entity برای تنظیمات
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class ZarinPalDriver : IGatewayDriver
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _merchantId;
        private readonly bool _isSandbox;
        private readonly string _requestUrl;
        private readonly string _verifyUrl;
        private readonly string _startPayUrl;
        private readonly string _statusUrl;

        #endregion

        #region Constructor

        /// <summary>
        /// ✅ BEST PRACTICE: Constructor جدید - استفاده از PaymentGateway Entity
        /// تنظیمات از Entity خوانده می‌شود (MerchantId, GatewayUrl, IsTestMode)
        /// </summary>
        /// <param name="gateway">PaymentGateway Entity شامل تمام تنظیمات</param>
        /// <param name="logger">Logger</param>
        public ZarinPalDriver(PaymentGateway gateway, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (gateway == null)
                throw new ArgumentNullException(nameof(gateway));
            
            if (gateway.GatewayType != Models.Enums.PaymentGatewayType.ZarinPal)
                throw new ArgumentException($"Gateway type must be ZarinPal, but got {gateway.GatewayType}", nameof(gateway));

            // ✅ BEST PRACTICE: خواندن تنظیمات از PaymentGateway Entity
            _merchantId = gateway.MerchantId ?? throw new ArgumentException("MerchantId is required", nameof(gateway));
            _isSandbox = gateway.IsTestMode; // IsTestMode = true = Sandbox
            
            // ✅ استفاده از GatewayUrl از Entity با Fallback به Web.config
            var gatewayUrl = gateway.GatewayUrl;
            if (string.IsNullOrWhiteSpace(gatewayUrl))
            {
                _logger.Warning("⚠️ ZarinPal: GatewayUrl در Entity خالی است، استفاده از Web.config");
                gatewayUrl = ZarinPalHelper.GetStartPayUrl();
            }
            
            // ✅ استخراج Base URL از GatewayUrl (مثلاً: https://www.zarinpal.com/pg/StartPay/)
            var baseUrl = ExtractBaseUrl(gatewayUrl);
            
            // ✅ ساخت URLs بر اساس Base URL و IsTestMode
            if (_isSandbox)
            {
                _requestUrl = "https://sandbox.zarinpal.com/pg/v4/payment/request.json";
                _verifyUrl = "https://sandbox.zarinpal.com/pg/v4/payment/verify.json";
                _startPayUrl = "https://sandbox.zarinpal.com/pg/StartPay/";
                _statusUrl = "https://sandbox.zarinpal.com/pg/v4/payment/status.json";
            }
            else
            {
                _requestUrl = "https://api.zarinpal.com/pg/v4/payment/request.json";
                _verifyUrl = "https://api.zarinpal.com/pg/v4/payment/verify.json";
                _startPayUrl = "https://www.zarinpal.com/pg/StartPay/";
                _statusUrl = "https://api.zarinpal.com/pg/v4/payment/status.json";
            }

            // ایجاد HttpClient
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30) // Timeout 30 ثانیه
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _logger.Information("✅ ZarinPal Driver initialized from Entity - GatewayId: {GatewayId}, MerchantId: {MerchantId}, IsSandbox: {IsSandbox}, GatewayUrl: {GatewayUrl}", 
                gateway.PaymentGatewayId,
                _merchantId.Substring(0, Math.Min(8, _merchantId.Length)) + "...", 
                _isSandbox,
                gatewayUrl);
        }

        /// <summary>
        /// ⚠️ DEPRECATED: Constructor قدیمی - استفاده از Web.config
        /// برای سازگاری با کد قدیمی نگه داشته شده است
        /// </summary>
        /// <param name="logger">Logger</param>
        [Obsolete("Use ZarinPalDriver(PaymentGateway, ILogger) instead. This constructor reads from Web.config.")]
        public ZarinPalDriver(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // خواندن تنظیمات از Web.config (Fallback)
            _merchantId = ZarinPalHelper.GetMerchantId();
            _isSandbox = ZarinPalHelper.IsSandbox();
            _requestUrl = ZarinPalHelper.GetRequestUrl();
            _verifyUrl = ZarinPalHelper.GetVerifyUrl();
            _startPayUrl = ZarinPalHelper.GetStartPayUrl();
            _statusUrl = ZarinPalHelper.GetStatusUrl();

            // ایجاد HttpClient
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30) // Timeout 30 ثانیه
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _logger.Warning("⚠️ ZarinPal Driver initialized from Web.config (DEPRECATED) - MerchantId: {MerchantId}, IsSandbox: {IsSandbox}", 
                _merchantId.Substring(0, Math.Min(8, _merchantId.Length)) + "...", _isSandbox);
        }

        /// <summary>
        /// استخراج Base URL از GatewayUrl
        /// مثال: https://www.zarinpal.com/pg/StartPay/ → https://www.zarinpal.com
        /// </summary>
        private string ExtractBaseUrl(string gatewayUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gatewayUrl))
                    return null;

                var uri = new Uri(gatewayUrl);
                return $"{uri.Scheme}://{uri.Host}";
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Request Payment

        /// <summary>
        /// ایجاد درخواست پرداخت در زرین‌پال
        /// </summary>
        public async Task<ServiceResult<PaymentRequestResult>> RequestPaymentAsync(PaymentRequest request)
        {
            var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString("N");
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.Information("💰 ZarinPal REQUEST: شروع درخواست پرداخت - Amount: {Amount}, Description: {Description}, CallbackUrl: {CallbackUrl}, CorrelationId: {CorrelationId}", 
                    request.Amount, request.Description, request.CallbackUrl, correlationId);

                // Validation
                var validationResult = ValidatePaymentRequest(request);
                if (!validationResult.Success)
                {
                    _logger.Warning("⚠️ ZarinPal VALIDATION: Validation ناموفق - Message: {Message}, CorrelationId: {CorrelationId}", 
                        validationResult.Message, correlationId);
                    return ServiceResult<PaymentRequestResult>.Failed(validationResult.Message);
                }
                
                _logger.Information("✅ ZarinPal VALIDATION: Validation موفق - Amount: {Amount}, CallbackUrl: {CallbackUrl}, CorrelationId: {CorrelationId}", 
                    request.Amount, request.CallbackUrl, correlationId);

                // ساخت Request Body
                var requestBody = new
                {
                    merchant_id = _merchantId,
                    amount = (long)request.Amount, // ZarinPal expects long (Rials)
                    description = request.Description ?? "پرداخت نوبت",
                    callback_url = request.CallbackUrl,
                    mobile = request.Mobile,
                    email = request.Email,
                    metadata = !string.IsNullOrWhiteSpace(request.Metadata) ? JsonConvert.DeserializeObject(request.Metadata) : null
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.Information("📤 ZarinPal HTTP REQUEST: ارسال درخواست به {Url} - MerchantId: {MerchantId}, Amount: {Amount}, CallbackUrl: {CallbackUrl}, Description: {Description}, Mobile: {Mobile}, Email: {Email}, CorrelationId: {CorrelationId}", 
                    _requestUrl, _merchantId.Substring(0, Math.Min(8, _merchantId.Length)) + "...", requestBody.amount, requestBody.callback_url, requestBody.description, requestBody.mobile, requestBody.email, correlationId);
                
                _logger.Debug("📤 ZarinPal REQUEST BODY: Request Body - {RequestBody}, CorrelationId: {CorrelationId}", jsonContent, correlationId);
                
                // ✅ CRITICAL DEBUG: لاگ کامل برای Debug
                _logger.Information("🔍 ZarinPal CONFIG: IsSandbox={IsSandbox}, RequestUrl={RequestUrl}, CallbackUrl={CallbackUrl}, MerchantIdPrefix={MerchantIdPrefix}, GatewayUrl={GatewayUrl}, CorrelationId: {CorrelationId}", 
                    _isSandbox, _requestUrl, requestBody.callback_url, _merchantId.Substring(0, Math.Min(8, _merchantId.Length)) + "...", _startPayUrl, correlationId);

                // ارسال درخواست
                var httpRequestTime = DateTime.UtcNow;
                var response = await _httpClient.PostAsync(_requestUrl, content);
                var httpResponseTime = DateTime.UtcNow;
                var httpDuration = (httpResponseTime - httpRequestTime).TotalMilliseconds;
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.Information("📥 ZarinPal HTTP RESPONSE: پاسخ دریافت شد - StatusCode: {StatusCode}, IsSuccessStatusCode: {IsSuccess}, ContentLength: {Length}, Duration: {Duration}ms, Content: {Content}, CorrelationId: {CorrelationId}", 
                    response.StatusCode, response.IsSuccessStatusCode, responseContent?.Length ?? 0, httpDuration, responseContent, correlationId);

                // Parse Response
                var zarinPalResponse = JsonConvert.DeserializeObject<ZarinPalRequestResponse>(responseContent);

                if (zarinPalResponse == null)
                {
                    _logger.Error("❌ ZarinPal PARSE ERROR: پاسخ نامعتبر - Content: {Content}, CorrelationId: {CorrelationId}", 
                        responseContent, correlationId);
                    return ServiceResult<PaymentRequestResult>.Failed("پاسخ نامعتبر از درگاه پرداخت");
                }

                _logger.Information("✅ ZarinPal PARSE: Response Parse موفق - HasErrors: {HasErrors}, HasData: {HasData}, CorrelationId: {CorrelationId}", 
                    zarinPalResponse.errors != null, zarinPalResponse.data != null, correlationId);

                // ✅ CRITICAL FIX: بررسی errors در پاسخ (اگر API خطا بدهد، errors پر می‌شود)
                if (zarinPalResponse.errors != null)
                {
                    var errorCode = zarinPalResponse.errors.code ?? "UNKNOWN";
                    var errorMessage = zarinPalResponse.errors.message ?? "خطای نامشخص از درگاه پرداخت";
                    
                    _logger.Error("❌ ZarinPal API ERROR: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, ResponseContent: {Content}, CorrelationId: {CorrelationId}",
                        errorCode, errorMessage, responseContent, correlationId);
                    
                    return ServiceResult<PaymentRequestResult>.Failed($"خطا از درگاه پرداخت: {errorMessage}");
                }

                // ✅ CRITICAL FIX: بررسی null بودن data
                if (zarinPalResponse.data == null)
                {
                    _logger.Error("❌ ZarinPal DATA NULL: data در پاسخ null است - ResponseContent: {Content}, CorrelationId: {CorrelationId}", 
                        responseContent, correlationId);
                    return ServiceResult<PaymentRequestResult>.Failed("پاسخ نامعتبر از درگاه پرداخت (data is null)");
                }

                // ✅ CRITICAL FIX: بررسی null بودن code
                if (!zarinPalResponse.data.code.HasValue)
                {
                    _logger.Error("❌ ZarinPal CODE NULL: code در پاسخ null است - ResponseContent: {Content}, DataMessage: {Message}, CorrelationId: {CorrelationId}",
                        responseContent, zarinPalResponse.data.message, correlationId);
                    return ServiceResult<PaymentRequestResult>.Failed($"پاسخ نامعتبر از درگاه پرداخت: {zarinPalResponse.data.message ?? "کد خطا نامشخص است"}");
                }
                
                _logger.Information("✅ ZarinPal CODE: Code دریافت شد - Code: {Code}, Message: {Message}, CorrelationId: {CorrelationId}", 
                    zarinPalResponse.data.code.Value, zarinPalResponse.data.message, correlationId);

                // بررسی Status Code
                if (zarinPalResponse.data.code == 100) // 100 = Success
                {
                    // ✅ CRITICAL FIX: بررسی null بودن authority
                    if (string.IsNullOrWhiteSpace(zarinPalResponse.data.authority))
                    {
                        _logger.Error("❌ ZarinPal AUTHORITY NULL: authority در پاسخ null یا خالی است - ResponseContent: {Content}, CorrelationId: {CorrelationId}",
                            responseContent, correlationId);
                        return ServiceResult<PaymentRequestResult>.Failed("کد Authority از درگاه پرداخت دریافت نشد");
                    }

                    var paymentUrl = $"{_startPayUrl}{zarinPalResponse.data.authority}";
                    
                    var result = new PaymentRequestResult
                    {
                        Success = true,
                        Authority = zarinPalResponse.data.authority,
                        PaymentUrl = paymentUrl,
                        AdditionalData = new Dictionary<string, string>
                        {
                            { "Fee", zarinPalResponse.data.fee?.ToString() ?? "0" },
                            { "FeeType", zarinPalResponse.data.fee_type ?? "Merchant" }
                        }
                    };

                    var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.Information("✅ ZarinPal SUCCESS: درخواست پرداخت موفق - Authority: {Authority}, PaymentUrl: {PaymentUrl}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}", 
                        result.Authority, result.PaymentUrl, processingTime, correlationId);

                    return ServiceResult<PaymentRequestResult>.Successful(result, "درخواست پرداخت با موفقیت ایجاد شد");
                }
                else
                {
                    var errorCode = zarinPalResponse.data.code.Value;
                    var errorMessage = GetZarinPalErrorMessage(errorCode);
                    
                    // ✅ CRITICAL FIX: استفاده از message از API اگر موجود باشد
                    if (!string.IsNullOrWhiteSpace(zarinPalResponse.data.message))
                    {
                        errorMessage = $"{errorMessage} ({zarinPalResponse.data.message})";
                    }
                    
                    var result = new PaymentRequestResult
                    {
                        Success = false,
                        ErrorCode = errorCode.ToString(),
                        ErrorMessage = errorMessage
                    };

                    var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.Warning("⚠️ ZarinPal FAILED: درخواست پرداخت ناموفق - Code: {Code}, Message: {Message}, ApiMessage: {ApiMessage}, ResponseContent: {Content}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}", 
                        result.ErrorCode, result.ErrorMessage, zarinPalResponse.data.message, responseContent, processingTime, correlationId);

                    return ServiceResult<PaymentRequestResult>.Failed(errorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(ex, "❌ ZarinPal HTTP EXCEPTION: خطا در ارتباط با درگاه پرداخت - ExceptionType: {ExceptionType}, Message: {Message}, RequestUrl: {RequestUrl}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}", 
                    ex.GetType().Name, ex.Message, _requestUrl, processingTime, correlationId);
                
                if (ex.InnerException != null)
                {
                    _logger.Error("❌ ZarinPal HTTP EXCEPTION INNER: InnerException - Type: {Type}, Message: {Message}, CorrelationId: {CorrelationId}",
                        ex.InnerException.GetType().Name, ex.InnerException.Message, correlationId);
                }
                
                return ServiceResult<PaymentRequestResult>.Failed("خطا در ارتباط با درگاه پرداخت");
            }
            catch (Exception ex)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(ex, "❌ ZarinPal EXCEPTION: خطای غیرمنتظره در درخواست پرداخت - ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}, ProcessingTime: {ProcessingTime}ms, CorrelationId: {CorrelationId}", 
                    ex.GetType().Name, ex.Message, ex.StackTrace, processingTime, correlationId);
                
                if (ex.InnerException != null)
                {
                    _logger.Error("❌ ZarinPal EXCEPTION INNER: InnerException - Type: {Type}, Message: {Message}, StackTrace: {StackTrace}, CorrelationId: {CorrelationId}",
                        ex.InnerException.GetType().Name, ex.InnerException.Message, ex.InnerException.StackTrace, correlationId);
                }
                
                return ServiceResult<PaymentRequestResult>.Failed("خطا در درخواست پرداخت");
            }
        }

        #endregion

        #region Verify Payment

        /// <summary>
        /// تأیید پرداخت در زرین‌پال
        /// </summary>
        public async Task<ServiceResult<PaymentVerificationResult>> VerifyPaymentAsync(PaymentVerificationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return ServiceResult<PaymentVerificationResult>.Failed("درخواست تأیید پرداخت نمی‌تواند خالی باشد");
                }

                _logger.Information("🔍 ZarinPal: شروع تأیید پرداخت - Authority: {Authority}, Amount: {Amount}", 
                    request.Authority, request.Amount);

                // Validation
                if (string.IsNullOrWhiteSpace(request.Authority))
                {
                    return ServiceResult<PaymentVerificationResult>.Failed("Authority Code الزامی است");
                }

                if (request.Amount <= 0)
                {
                    return ServiceResult<PaymentVerificationResult>.Failed("مبلغ پرداخت باید بیشتر از صفر باشد");
                }

                // ساخت Request Body
                var requestBody = new
                {
                    merchant_id = _merchantId,
                    amount = (long)request.Amount,
                    authority = request.Authority
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.Debug("📤 ZarinPal: ارسال درخواست تأیید به {Url}", _verifyUrl);

                // ارسال درخواست
                var response = await _httpClient.PostAsync(_verifyUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.Debug("📥 ZarinPal: پاسخ تأیید دریافت شد - StatusCode: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);

                // Parse Response
                var zarinPalResponse = JsonConvert.DeserializeObject<ZarinPalVerifyResponse>(responseContent);

                if (zarinPalResponse == null)
                {
                    _logger.Error("❌ ZarinPal: پاسخ تأیید نامعتبر - Content: {Content}", responseContent);
                    return ServiceResult<PaymentVerificationResult>.Failed("پاسخ نامعتبر از درگاه پرداخت");
                }

                // بررسی Status Code
                if (zarinPalResponse.data?.code == 100) // 100 = Success
                {
                    var result = new PaymentVerificationResult
                    {
                        Success = true,
                        IsVerified = true,
                        RefId = zarinPalResponse.data.ref_id?.ToString() ?? string.Empty,
                        Authority = request.Authority,
                        PaymentToken = request.Authority,
                        Amount = request.Amount,
                        AdditionalData = new Dictionary<string, string>
                        {
                            { "Fee", zarinPalResponse.data.fee?.ToString() ?? "0" },
                            { "FeeType", zarinPalResponse.data.fee_type ?? "Merchant" },
                            { "CardHash", zarinPalResponse.data.card_hash ?? string.Empty },
                            { "CardPan", zarinPalResponse.data.card_pan ?? string.Empty }
                        }
                    };

                    _logger.Information("✅ ZarinPal: تأیید پرداخت موفق - Authority: {Authority}, RefId: {RefId}", 
                        request.Authority, result.RefId);

                    return ServiceResult<PaymentVerificationResult>.Successful(result, "پرداخت با موفقیت تأیید شد");
                }
                else
                {
                    var errorMessage = GetZarinPalErrorMessage(zarinPalResponse.data?.code ?? -1);
                    
                    var result = new PaymentVerificationResult
                    {
                        Success = false,
                        IsVerified = false,
                        Authority = request.Authority,
                        PaymentToken = request.Authority,
                        Amount = request.Amount,
                        ErrorCode = zarinPalResponse.data?.code?.ToString() ?? "-1",
                        ErrorMessage = errorMessage
                    };

                    _logger.Warning("⚠️ ZarinPal: تأیید پرداخت ناموفق - Authority: {Authority}, Code: {Code}, Message: {Message}", 
                        request.Authority, result.ErrorCode, result.ErrorMessage);

                    return ServiceResult<PaymentVerificationResult>.Failed(errorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "❌ ZarinPal: خطا در ارتباط با درگاه پرداخت برای تأیید");
                return ServiceResult<PaymentVerificationResult>.Failed("خطا در ارتباط با درگاه پرداخت");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ ZarinPal: خطای غیرمنتظره در تأیید پرداخت");
                return ServiceResult<PaymentVerificationResult>.Failed("خطا در تأیید پرداخت");
            }
        }

        #endregion

        #region Check Payment Status

        /// <summary>
        /// بررسی وضعیت پرداخت در زرین‌پال
        /// </summary>
        public async Task<ServiceResult<PaymentStatusResult>> CheckPaymentStatusAsync(string transactionId, decimal amount)
        {
            try
            {
                _logger.Information("🔍 ZarinPal: بررسی وضعیت پرداخت - TransactionId: {TransactionId}, Amount: {Amount}", transactionId, amount);

                // Validation
                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    return ServiceResult<PaymentStatusResult>.Failed("TransactionId الزامی است");
                }

                // ساخت Request Body
                var requestBody = new
                {
                    merchant_id = _merchantId,
                    authority = transactionId // در زرین‌پال TransactionId همان Authority است
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.Debug("📤 ZarinPal: ارسال درخواست بررسی وضعیت به {Url}", _statusUrl);

                // ارسال درخواست
                var response = await _httpClient.PostAsync(_statusUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.Debug("📥 ZarinPal: پاسخ بررسی وضعیت دریافت شد - StatusCode: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);

                // Parse Response
                var zarinPalResponse = JsonConvert.DeserializeObject<ZarinPalStatusResponse>(responseContent);

                if (zarinPalResponse == null)
                {
                    _logger.Error("❌ ZarinPal: پاسخ بررسی وضعیت نامعتبر - Content: {Content}", responseContent);
                    return ServiceResult<PaymentStatusResult>.Failed("پاسخ نامعتبر از درگاه پرداخت");
                }

                // بررسی Status Code
                var result = new PaymentStatusResult
                {
                    Success = true,
                    TransactionId = transactionId,
                    Amount = amount
                };

                if (zarinPalResponse.data?.code == 100) // 100 = Success
                {
                    result.Status = "Success";
                    result.Amount = zarinPalResponse.data.amount ?? amount;
                }
                else if (zarinPalResponse.data?.code == 101) // 101 = Already Verified
                {
                    result.Status = "Success";
                    result.Amount = zarinPalResponse.data.amount ?? amount;
                }
                else if (zarinPalResponse.data?.code == -1) // -1 = Not Found
                {
                    result.Status = "Pending";
                }
                else
                {
                    result.Status = "Failed";
                    result.ErrorCode = zarinPalResponse.data?.code?.ToString() ?? "-1";
                    result.ErrorMessage = GetZarinPalErrorMessage(zarinPalResponse.data?.code ?? -1);
                }

                _logger.Information("✅ ZarinPal: وضعیت پرداخت بررسی شد - TransactionId: {TransactionId}, Status: {Status}", 
                    transactionId, result.Status);

                return ServiceResult<PaymentStatusResult>.Successful(result, "وضعیت پرداخت بررسی شد");
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "❌ ZarinPal: خطا در ارتباط با درگاه پرداخت برای بررسی وضعیت");
                return ServiceResult<PaymentStatusResult>.Failed("خطا در ارتباط با درگاه پرداخت");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ ZarinPal: خطای غیرمنتظره در بررسی وضعیت پرداخت");
                return ServiceResult<PaymentStatusResult>.Failed("خطا در بررسی وضعیت پرداخت");
            }
        }

        #endregion

        #region Refund

        /// <summary>
        /// برگشت وجه پرداخت (در صورت پشتیبانی توسط زرین‌پال)
        /// </summary>
        public async Task<ServiceResult<RefundResult>> RefundPaymentAsync(RefundRequest request)
        {
            // ⚠️ توجه: زرین‌پال API مستقیم برای Refund ندارد
            // Refund باید از طریق پنل مدیریتی زرین‌پال انجام شود
            // این متد برای سازگاری با Interface است

            if (request == null)
            {
                return ServiceResult<RefundResult>.Failed("درخواست برگشت وجه نمی‌تواند خالی باشد");
            }

            _logger.Warning("⚠️ ZarinPal: Refund از طریق API پشتیبانی نمی‌شود. باید از پنل مدیریتی استفاده شود. TransactionId: {TransactionId}, Amount: {Amount}", 
                request.TransactionId, request.Amount);

            return ServiceResult<RefundResult>.Failed(
                "برگشت وجه از طریق API پشتیبانی نمی‌شود. لطفاً از پنل مدیریتی زرین‌پال استفاده کنید.",
                "NOT_SUPPORTED"
            );
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت
        /// </summary>
        private ServiceResult ValidatePaymentRequest(PaymentRequest request)
        {
            if (request == null)
            {
                return ServiceResult.Failed("درخواست پرداخت نمی‌تواند خالی باشد");
            }

            if (request.Amount <= 0)
            {
                return ServiceResult.Failed("مبلغ پرداخت باید بیشتر از صفر باشد");
            }

            if (request.Amount < 1000) // حداقل 1000 ریال
            {
                return ServiceResult.Failed("حداقل مبلغ پرداخت 1000 ریال است");
            }

            if (string.IsNullOrWhiteSpace(request.CallbackUrl))
            {
                return ServiceResult.Failed("آدرس Callback الزامی است");
            }

            if (!Uri.IsWellFormedUriString(request.CallbackUrl, UriKind.Absolute))
            {
                return ServiceResult.Failed("آدرس Callback نامعتبر است");
            }

            return ServiceResult.Successful();
        }

        /// <summary>
        /// تبدیل کد خطای زرین‌پال به پیام فارسی
        /// </summary>
        private string GetZarinPalErrorMessage(int code)
        {
            return code switch
            {
                100 => "عملیات موفق",
                101 => "عملیات پرداخت قبلاً انجام شده است",
                -9 => "خطای اعتبارسنجی",
                -10 => "IP یا مرچنت کد صحیح نیست",
                -11 => "مرچنت کد فعال نیست",
                -12 => "تلاش بیش از حد درخواست",
                -15 => "درگاه پرداخت فعال نیست",
                -16 => "توکن‌های ارسالی از درگاه پرداخت با تنظیمات همخوانی ندارد",
                -30 => "اجازه دسترسی به تسویه اشتراکی ندارید",
                -31 => "حساب بانکی تسویه را به پنل اضافه کنید",
                -32 => "مبلغ از حد مجاز بیشتر است",
                -33 => "درخواست نامعتبر",
                -34 => "درخواست نامعتبر",
                -35 => "مبلغ از حد مجاز کمتر است",
                -40 => "درخواست نامعتبر",
                -50 => "مبلغ پرداخت شده با مبلغ وریفای شده مطابقت ندارد",
                -51 => "پرداخت ناموفق",
                -52 => "خطای غیرمنتظره",
                -53 => "حساب کاربری مسدود است",
                -54 => "آدرس IP نامعتبر است",
                _ => $"خطای نامشخص (کد: {code})"
            };
        }

        #endregion

        #region ZarinPal Response Models

        /// <summary>
        /// پاسخ درخواست پرداخت زرین‌پال
        /// </summary>
        private class ZarinPalRequestResponse
        {
            public ZarinPalRequestData data { get; set; }
            public ZarinPalError errors { get; set; }
        }

        /// <summary>
        /// داده‌های پاسخ درخواست پرداخت
        /// </summary>
        private class ZarinPalRequestData
        {
            public int? code { get; set; }
            public string message { get; set; }
            public string authority { get; set; }
            public long? fee { get; set; }
            public string fee_type { get; set; }
        }

        /// <summary>
        /// پاسخ تأیید پرداخت زرین‌پال
        /// </summary>
        private class ZarinPalVerifyResponse
        {
            public ZarinPalVerifyData data { get; set; }
            public ZarinPalError errors { get; set; }
        }

        /// <summary>
        /// داده‌های پاسخ تأیید پرداخت
        /// </summary>
        private class ZarinPalVerifyData
        {
            public int? code { get; set; }
            public string message { get; set; }
            public long? ref_id { get; set; }
            public string card_pan { get; set; }
            public string card_hash { get; set; }
            public long? fee { get; set; }
            public string fee_type { get; set; }
        }

        /// <summary>
        /// پاسخ بررسی وضعیت پرداخت زرین‌پال
        /// </summary>
        private class ZarinPalStatusResponse
        {
            public ZarinPalStatusData data { get; set; }
            public ZarinPalError errors { get; set; }
        }

        /// <summary>
        /// داده‌های پاسخ بررسی وضعیت
        /// </summary>
        private class ZarinPalStatusData
        {
            public int? code { get; set; }
            public string message { get; set; }
            public long? ref_id { get; set; }
            public decimal? amount { get; set; }
        }

        /// <summary>
        /// خطای زرین‌پال
        /// </summary>
        private class ZarinPalError
        {
            public string code { get; set; }
            public string message { get; set; }
            public object validations { get; set; }
        }

        #endregion
    }
}

