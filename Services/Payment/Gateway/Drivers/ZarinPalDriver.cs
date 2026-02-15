using ClinicApp.Helpers;
using ClinicApp.Interfaces.Payment.Gateway.Drivers;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Payment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Payment;
using PaymentRequest = ClinicApp.Interfaces.Payment.Gateway.Drivers.PaymentRequest;

namespace ClinicApp.Services.Payment.Gateway.Drivers
{
    /// <summary>
    /// Driver برای درگاه پرداخت زرین‌پال (REST v4: request / verify / status / StartPay).
    /// طراحی شده طبق اصول SRP - مسئولیت: ارتباط با API درگاه پرداخت زرین‌پال.
    ///
    /// ویژگی‌های کلیدی:
    /// 1. Payment Request (درخواست پرداخت)
    /// 2. Payment Verification (تأیید پرداخت)
    /// 3. Payment Status Check (بررسی وضعیت)
    /// 4. Refund (برگشت وجه - از طریق API پشتیبانی نمی‌شود)
    /// 5. ✅ BEST PRACTICE: استفاده از PaymentGateway Entity برای تنظیمات
    ///
    /// مستندات درگاه پرداخت: https://www.zarinpal.com/docs/paymentGateway/
    /// (مستندات apiDocs/auth/guide مربوط به OAuth/GraphQL پنل است، نه درگاه پرداخت.)
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md و Docs/ZARINPAL_REVIEW.md
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
        private readonly string _inquiryUrl;

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
            // ✅ وقتی درخواست از localhost است برای تست، همیشه از Sandbox استفاده کن تا به درگاه وصل شود
            try
            {
                var req = System.Web.HttpContext.Current?.Request;
                if (req?.Url != null)
                {
                    var host = req.Url.Host ?? "";
                    if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host == "127.0.0.1")
                    {
                        _isSandbox = true;
                        _logger.Information("✅ ZarinPal: درخواست از localhost است؛ استفاده از Sandbox برای اتصال به درگاه");
                    }
                }
            }
            catch { /* نادیده گرفتن خطا در خواندن HttpContext */ }
            
            // ✅ روی localhost/سندباکس: اگر مرچنت سندباکس در config تنظیم شده باشد استفاده کن تا درگاه به‌خاطر مرچنت مخصوص mehranyad.ir درخواست را رد نکند
            if (_isSandbox)
            {
                var sandboxMerchant = ZarinPalHelper.GetSandboxMerchantId();
                if (!string.IsNullOrWhiteSpace(sandboxMerchant))
                {
                    _merchantId = sandboxMerchant;
                    _logger.Information("✅ ZarinPal: استفاده از مرچنت سندباکس برای تست محلی (Zarinpal:SandboxMerchantId)");
                }
            }
            
            // ✅ استفاده از GatewayUrl از Entity با Fallback به Web.config
            var gatewayUrl = gateway.GatewayUrl;
            if (string.IsNullOrWhiteSpace(gatewayUrl))
            {
                _logger.Warning("⚠️ ZarinPal: GatewayUrl در Entity خالی است، استفاده از Web.config");
                gatewayUrl = ZarinPalHelper.GetStartPayUrl();
            }
            
            // ✅ استخراج Base URL از GatewayUrl (مثلاً: https://payment.zarinpal.com/pg/StartPay/)
            var baseUrl = ExtractBaseUrl(gatewayUrl);
            
            // ✅ ساخت URLs بر اساس Base URL و IsTestMode
            if (_isSandbox)
            {
                _requestUrl = "https://sandbox.zarinpal.com/pg/v4/payment/request.json";
                _verifyUrl = "https://sandbox.zarinpal.com/pg/v4/payment/verify.json";
                _startPayUrl = "https://sandbox.zarinpal.com/pg/StartPay/";
                _statusUrl = "https://sandbox.zarinpal.com/pg/v4/payment/status.json";
                _inquiryUrl = "https://sandbox.zarinpal.com/pg/v4/payment/inquiry.json";
            }
            else
            {
                // طبق مستندات رسمی: connectToGateway و استعلام وضعیت (Inquiry)
                _requestUrl = "https://payment.zarinpal.com/pg/v4/payment/request.json";
                _verifyUrl = "https://payment.zarinpal.com/pg/v4/payment/verify.json";
                _startPayUrl = "https://payment.zarinpal.com/pg/StartPay/";
                _statusUrl = "https://payment.zarinpal.com/pg/v4/payment/status.json";
                _inquiryUrl = "https://payment.zarinpal.com/pg/v4/payment/inquiry.json";
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
            _inquiryUrl = _isSandbox ? "https://sandbox.zarinpal.com/pg/v4/payment/inquiry.json" : "https://payment.zarinpal.com/pg/v4/payment/inquiry.json";

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
        /// مثال: https://payment.zarinpal.com/pg/StartPay/ → https://payment.zarinpal.com
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

        /// <summary>
        /// پارس فیلد code از پاسخ زرین‌پال که ممکن است به صورت عدد یا رشته برگردد (مثلاً در سندباکس).
        /// </summary>
        private static int? ParseCodeToken(JToken token)
        {
            if (token == null) return null;
            if (token.Type == JTokenType.Integer) return token.Value<int?>();
            if (token.Type == JTokenType.String)
            {
                var s = token.ToString();
                if (string.IsNullOrEmpty(s)) return null;
                return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : (int?)null;
            }
            return null;
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
            string responseContent = null;
            int? httpStatusCode = null;
            
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
                httpStatusCode = (int)response.StatusCode;
                var httpResponseTime = DateTime.UtcNow;
                var httpDuration = (httpResponseTime - httpRequestTime).TotalMilliseconds;
                responseContent = await response.Content.ReadAsStringAsync();

                _logger.Information("📥 ZarinPal HTTP RESPONSE: پاسخ دریافت شد - StatusCode: {StatusCode}, IsSuccessStatusCode: {IsSuccess}, ContentLength: {Length}, Duration: {Duration}ms, Content: {Content}, CorrelationId: {CorrelationId}", 
                    response.StatusCode, response.IsSuccessStatusCode, responseContent?.Length ?? 0, httpDuration, responseContent, correlationId);

                // اگر پاسخ خالی یا شبیه HTML است، قبل از پارس خطا برگردان
                var contentTrimmed = responseContent?.Trim() ?? "";
                if (string.IsNullOrEmpty(contentTrimmed))
                {
                    _logger.Error("❌ ZarinPal EMPTY RESPONSE: پاسخ درگاه خالی است - StatusCode: {StatusCode}, CorrelationId: {CorrelationId}", httpStatusCode, correlationId);
                    return ServiceResult<PaymentRequestResult>.Failed("پاسخ درگاه پرداخت خالی است. لطفاً دوباره تلاش کنید.", "ZARINPAL_EMPTY_RESPONSE");
                }
                if (contentTrimmed.StartsWith("<", StringComparison.Ordinal))
                {
                    _logger.Error("❌ ZarinPal HTML RESPONSE: پاسخ درگاه به صورت HTML است (احتمالاً صفحه خطا) - StatusCode: {StatusCode}, ContentPreview: {Preview}, CorrelationId: {CorrelationId}", 
                        httpStatusCode, contentTrimmed.Length > 200 ? contentTrimmed.Substring(0, 200) + "..." : contentTrimmed, correlationId);
                    return ServiceResult<PaymentRequestResult>.Failed("درگاه پرداخت در دسترس نیست یا خطای سرور برگردانده است. لطفاً چند دقیقه دیگر تلاش کنید.", "ZARINPAL_HTML_RESPONSE");
                }

                // حذف BOM و فاصله‌های اضافه که ممکن است پارس JSON را خراب کنند
                responseContent = contentTrimmed;
                if (responseContent.StartsWith("\uFEFF", StringComparison.Ordinal))
                    responseContent = responseContent.TrimStart('\uFEFF');

                // Parse Response با تنظیمات انعطاف‌پذیر (سندباکس یا پاسخ با ساختار کمی متفاوت)
                var jsonSettings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
                ZarinPalRequestResponse zarinPalResponse;
                try
                {
                    zarinPalResponse = JsonConvert.DeserializeObject<ZarinPalRequestResponse>(responseContent, jsonSettings);
                }
                catch (JsonException)
                {
                    // تلاش دوم: اگر data.code به صورت رشته برگشته باشد با JObject پارس کن
                    try
                    {
                        var jo = JObject.Parse(responseContent);
                        zarinPalResponse = new ZarinPalRequestResponse();
                        if (jo["data"] != null)
                        {
                            zarinPalResponse.data = new ZarinPalRequestData();
                            var dataToken = jo["data"];
                            zarinPalResponse.data.code = ParseCodeToken(dataToken["code"]);
                            zarinPalResponse.data.message = dataToken["message"]?.ToString();
                            zarinPalResponse.data.authority = dataToken["authority"]?.ToString();
                            zarinPalResponse.data.fee = dataToken["fee"]?.Value<long?>();
                            zarinPalResponse.data.fee_type = dataToken["fee_type"]?.ToString();
                        }
                        if (jo["errors"] != null && jo["errors"].Type == JTokenType.Object)
                        {
                            zarinPalResponse.errors = jo["errors"].ToObject<ZarinPalError>();
                            var errToken = jo["errors"];
                            if (zarinPalResponse.errors?.code == null && errToken is JObject errObj)
                            {
                                var firstProp = errObj.First as JProperty;
                                if (firstProp?.Value is JArray)
                                {
                                    var arr = (JArray)firstProp.Value;
                                    if (arr.Count > 0)
                                        zarinPalResponse.errors = zarinPalResponse.errors ?? new ZarinPalError { code = arr.Last?.ToString(), message = arr.First?.ToString() };
                                }
                            }
                        }
                    }
                    catch (Exception parseEx)
                    {
                        _logger.Error(parseEx, "❌ ZarinPal FALLBACK PARSE FAILED: ResponseContent: {Content}, CorrelationId: {CorrelationId}", responseContent, correlationId);
                        throw new JsonException("پاسخ درگاه قابل خواندن نیست (ساختار JSON نامعتبر).", parseEx);
                    }
                }

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

                    return ServiceResult<PaymentRequestResult>.Failed(errorMessage, errorCode.ToString());
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
                
                return ServiceResult<PaymentRequestResult>.Failed("خطا در ارتباط با درگاه پرداخت", "ZARINPAL_HTTP_ERROR");
            }
            catch (JsonException jsonEx)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(jsonEx, "❌ ZarinPal JSON EXCEPTION: پاسخ درگاه قابل خواندن نیست - Message: {Message}, StatusCode: {StatusCode}, ResponseContent: {Content}, CorrelationId: {CorrelationId}", 
                    jsonEx.Message, httpStatusCode, responseContent ?? "(null)", correlationId);
                var userMessage = !string.IsNullOrWhiteSpace(responseContent) && responseContent.TrimStart().StartsWith("<", StringComparison.Ordinal)
                    ? "درگاه پرداخت در دسترس نیست یا خطای سرور برگردانده است. لطفاً چند دقیقه دیگر تلاش کنید."
                    : "پاسخ درگاه پرداخت قابل خواندن نیست. لطفاً دوباره تلاش کنید.";
                var isDebug = System.Web.HttpContext.Current?.IsDebuggingEnabled == true;
                if (isDebug && !string.IsNullOrEmpty(responseContent))
                {
                    var preview = responseContent.Length > 150 ? responseContent.Substring(0, 150) + "…" : responseContent;
                    userMessage += " (پیش‌نمایش پاسخ: " + preview + ")";
                }
                return ServiceResult<PaymentRequestResult>.Failed(userMessage, "ZARINPAL_JSON_ERROR");
            }
            catch (TaskCanceledException)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Warning("❌ ZarinPal TIMEOUT: زمان اتصال به درگاه به پایان رسید - CorrelationId: {CorrelationId}, ProcessingTime: {ProcessingTime}ms", 
                    correlationId, processingTime);
                return ServiceResult<PaymentRequestResult>.Failed("زمان اتصال به درگاه به پایان رسید. لطفاً دوباره تلاش کنید.", "ZARINPAL_TIMEOUT");
            }
            catch (OperationCanceledException)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Warning("❌ ZarinPal CANCELLED: درخواست لغو شد - CorrelationId: {CorrelationId}", correlationId);
                return ServiceResult<PaymentRequestResult>.Failed("درخواست پرداخت لغو یا منقضی شد.", "ZARINPAL_CANCELLED");
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
                
                var isDev = "Development".Equals(ConfigurationManager.AppSettings["Environment"]?.Trim(), StringComparison.OrdinalIgnoreCase);
                var message = isDev 
                    ? $"خطا در درخواست پرداخت: {ex.Message}" 
                    : "خطا در درخواست پرداخت";
                return ServiceResult<PaymentRequestResult>.Failed(message, "ZARINPAL_REQUEST_EXCEPTION");
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

                var contentTrimmed = (responseContent ?? "").Trim();
                if (string.IsNullOrEmpty(contentTrimmed))
                {
                    _logger.Error("❌ ZarinPal: پاسخ تأیید خالی است");
                    return ServiceResult<PaymentVerificationResult>.Failed("پاسخ درگاه پرداخت خالی است");
                }
                if (contentTrimmed.StartsWith("<", StringComparison.Ordinal))
                {
                    _logger.Error("❌ ZarinPal: پاسخ تأیید به صورت HTML است");
                    return ServiceResult<PaymentVerificationResult>.Failed("درگاه پرداخت در دسترس نیست. لطفاً چند دقیقه دیگر تلاش کنید.");
                }

                // Parse Response با تنظیمات انعطاف‌پذیر (سندباکس ممکن است ساختار کمی متفاوت برگرداند)
                var jsonSettings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
                ZarinPalVerifyResponse zarinPalResponse;
                try
                {
                    zarinPalResponse = JsonConvert.DeserializeObject<ZarinPalVerifyResponse>(contentTrimmed, jsonSettings);
                }
                catch (JsonException)
                {
                    var jo = JObject.Parse(contentTrimmed);
                    zarinPalResponse = new ZarinPalVerifyResponse();
                    if (jo["data"] != null)
                    {
                        zarinPalResponse.data = new ZarinPalVerifyData();
                        var d = jo["data"];
                        zarinPalResponse.data.code = ParseCodeToken(d["code"]);
                        zarinPalResponse.data.message = d["message"]?.ToString();
                        zarinPalResponse.data.ref_id = d["ref_id"]?.Value<long?>() ?? (long.TryParse(d["ref_id"]?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : (long?)null);
                        zarinPalResponse.data.card_pan = d["card_pan"]?.ToString();
                        zarinPalResponse.data.card_hash = d["card_hash"]?.ToString();
                        zarinPalResponse.data.fee = d["fee"]?.Value<long?>();
                        zarinPalResponse.data.fee_type = d["fee_type"]?.ToString();
                    }
                }

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
        /// بررسی وضعیت پرداخت در زرین‌پال — طبق مستندات از متد استعلام وضعیت (inquiry.json) استفاده می‌شود.
        /// از این متد برای تأیید تراکنش استفاده نشود؛ فقط برای اطلاع از وضعیت.
        /// </summary>
        public async Task<ServiceResult<PaymentStatusResult>> CheckPaymentStatusAsync(string transactionId, decimal amount)
        {
            try
            {
                _logger.Information("🔍 ZarinPal: بررسی وضعیت پرداخت (Inquiry) - TransactionId: {TransactionId}", transactionId);

                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    return ServiceResult<PaymentStatusResult>.Failed("TransactionId الزامی است");
                }

                var requestBody = new { merchant_id = _merchantId, authority = transactionId };
                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.Debug("📤 ZarinPal: ارسال درخواست استعلام وضعیت به {Url}", _inquiryUrl);

                var response = await _httpClient.PostAsync(_inquiryUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.Debug("📥 ZarinPal: پاسخ استعلام دریافت شد - StatusCode: {StatusCode}, Content: {Content}",
                    response.StatusCode, responseContent);

                var inquiryResponse = JsonConvert.DeserializeObject<ZarinPalInquiryResponse>(responseContent);
                if (inquiryResponse?.data == null && inquiryResponse?.errors == null)
                {
                    _logger.Error("❌ ZarinPal: پاسخ استعلام نامعتبر - Content: {Content}", responseContent);
                    return ServiceResult<PaymentStatusResult>.Failed("پاسخ نامعتبر از درگاه پرداخت");
                }

                if (inquiryResponse.errors != null)
                {
                    var errMsg = inquiryResponse.errors.message ?? GetZarinPalErrorMessage(-54);
                    return ServiceResult<PaymentStatusResult>.Failed(errMsg);
                }

                var result = new PaymentStatusResult
                {
                    Success = true,
                    TransactionId = transactionId,
                    Amount = amount
                };

                var status = inquiryResponse.data?.status?.ToUpperInvariant();
                // VERIFIED, PAID, IN_BANK, FAILED, REVERSED — طبق https://www.zarinpal.com/docs/paymentGateway/otherMethods/Inquiry.html
                switch (status)
                {
                    case "VERIFIED":
                        result.Status = "Success";
                        break;
                    case "PAID":
                        result.Status = "Success";
                        break;
                    case "IN_BANK":
                        result.Status = "Pending";
                        break;
                    case "FAILED":
                        result.Status = "Failed";
                        result.ErrorMessage = "پرداخت ناموفق (تکمیل نشده)";
                        break;
                    case "REVERSED":
                        result.Status = "Failed";
                        result.ErrorMessage = "تراکنش ریورس شده است";
                        break;
                    default:
                        result.Status = inquiryResponse.data?.code == 100 ? "Success" : "Pending";
                        break;
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
        /// تبدیل کد خطای زرین‌پال به پیام فارسی — طبق https://www.zarinpal.com/docs/paymentGateway/errorList.html
        /// </summary>
        private string GetZarinPalErrorMessage(int code)
        {
            return code switch
            {
                100 => "عملیات موفق",
                101 => "تراکنش وریفای شده است",
                -9 => "خطای اعتبارسنجی (مرچنت، callback، description، مبلغ یا referrer_id)",
                -10 => "IP یا مرچنت کد پذیرنده صحیح نیست",
                -11 => "مرچنت کد فعال نیست",
                -12 => "تلاش بیش از حد در یک بازه زمانی کوتاه",
                -13 => "محدودیت تراکنش؛ تکمیل مدارک در پشتیبانی",
                -14 => "کال‌بک URL با دامنه ثبت‌شده درگاه مغایرت دارد",
                -15 => "درگاه پرداخت به حالت تعلیق در آمده است",
                -16 => "سطح تایید پذیرنده پایین‌تر از سطح نقره‌ای است",
                -17 => "محدودیت پذیرنده در سطح آبی",
                -18 => "امکان استفاده کد درگاه در دامنهٔ ثبت‌شده نیست",
                -19 => "امکان ایجاد تراکنش برای این ترمینال وجود ندارد",
                -30 => "پذیرنده اجازه دسترسی به تسویه اشتراکی شناور را ندارد",
                -31 => "حساب بانکی تسویه را به پنل اضافه کنید",
                -32 => "مبلغ واردشده از مبلغ کل تراکنش بیشتر است",
                -33 => "درصدهای تسهیم صحیح نیست",
                -34 => "مبلغ تسهیم ثابت از مبلغ کل بیشتر است",
                -35 => "تعداد افراد دریافت‌کننده تسهیم بیش از حد مجاز است",
                -36 => "حداقل مبلغ جهت تسهیم ۱۰۰۰۰ ریال است",
                -37 => "یک یا چند شبا برای تسهیم از سمت بانک غیرفعال است",
                -38 => "عدم تعریف صحیح شبا در شاپرک",
                -39 => "خطا در تسهیم؛ به پشتیبانی اطلاع دهید",
                -40 => "پارامتر اضافی نامعتبر (expire_in)",
                -41 => "حداکثر مبلغ پرداختی ۱۰۰ میلیون تومان است",
                -50 => "مبلغ پرداخت شده با مبلغ ارسالی در متد وریفای متفاوت است",
                -51 => "پرداخت ناموفق",
                -52 => "خطای غیرمنتظره؛ به پشتیبانی ارجاع دهید",
                -53 => "پرداخت متعلق به این مرچنت کد نیست",
                -54 => "اتوریتی نامعتبر است",
                -55 => "تراکنش مورد نظر یافت نشد",
                -60 => "امکان ریورس کردن تراکنش با بانک وجود ندارد",
                -61 => "تراکنش موفق نیست یا قبلاً ریورس شده است",
                -62 => "آی‌پی درگاه ست نشده است",
                -63 => "حداکثر زمان (۳۰ دقیقه) برای ریورس منقضی شده است",
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
        /// پاسخ استعلام وضعیت پرداخت (inquiry.json) — طبق مستندات دیگر متدها / استعلام وضعیت تراکنش
        /// </summary>
        private class ZarinPalInquiryResponse
        {
            public ZarinPalInquiryData data { get; set; }
            public ZarinPalError errors { get; set; }
        }

        private class ZarinPalInquiryData
        {
            public string status { get; set; }
            public int? code { get; set; }
            public string message { get; set; }
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

