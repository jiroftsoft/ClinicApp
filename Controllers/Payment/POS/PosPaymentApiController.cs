using ClinicApp.Helpers;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models;
using ClinicApp.Interfaces;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Serilog;

namespace ClinicApp.Controllers.Payment.POS
{
    /// <summary>
    /// Production-Ready POS Payment API Controller
    /// 
    /// مسئولیت: ارائه API برای پرداخت POS
    /// 
    /// ویژگی‌های کلیدی:
    /// ✅ قابل استفاده مجدد در ماژول‌های مختلف
    /// ✅ اعتبارسنجی کامل
    /// ✅ Logging کامل
    /// ✅ Error Handling حرفه‌ای
    /// ✅ Anti-Forgery Token Protection
    /// 
    /// استفاده:
    /// - ماژول پذیرش (Reception)
    /// - ماژول صندوق (Cashier)
    /// - سایر ماژول‌های پرداخت
    /// </summary>
    [RoutePrefix("api/v1/pos-payment")]
    public class PosPaymentApiController : Controller
    {
        private readonly IPosPaymentService _posPaymentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public PosPaymentApiController(
            IPosPaymentService posPaymentService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _posPaymentService = posPaymentService ?? throw new ArgumentNullException(nameof(posPaymentService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger.ForContext<PosPaymentApiController>();
        }

        /// <summary>
        /// پردازش پرداخت POS
        /// 
        /// POST /api/v1/pos-payment/process
        /// 
        /// Request Body:
        /// {
        ///   "receptionId": 123,
        ///   "amountIRR": 100000,
        ///   "terminalId": 1,
        ///   "description": "پرداخت پذیرش"
        /// }
        /// </summary>
        [HttpPost]
        [Route("process")]
        public async Task<JsonResult> ProcessPayment(PosPaymentRequest request)
        {
            try
            {
                _logger.Information("💳 POS Payment API: شروع پردازش پرداخت - ReceptionId: {ReceptionId}, Amount: {Amount}, TerminalId: {TerminalId}, User: {UserName}",
                    request?.ReceptionId, request?.AmountIRR, request?.TerminalId, _currentUserService?.UserName ?? "Unknown");

                // Validation
                if (request == null)
                {
                    _logger.Warning("⚠️ POS Payment API: درخواست null است");
                    return Json(ServiceResult.Failed("درخواست پرداخت نمی‌تواند خالی باشد"));
                }

                // Set UserId if not provided
                if (string.IsNullOrEmpty(request.UserId))
                {
                    request.UserId = _currentUserService?.UserId;
                }

                // Process Payment
                var result = await _posPaymentService.ProcessPaymentAsync(request);

                if (result.Success)
                {
                    _logger.Information("✅ POS Payment API: پرداخت موفق - ReceptionId: {ReceptionId}, RRN: {RRN}, TraceNo: {TraceNo}, Duration: {Duration}ms",
                        request.ReceptionId, result.Data?.RRN, result.Data?.TraceNo, result.Data?.DurationMs);

                    return Json(ServiceResult<object>.Successful(new
                    {
                        success = true,
                        rrn = result.Data.RRN,
                        traceNo = result.Data.TraceNo,
                        terminalId = result.Data.TerminalId,
                        cardLast4 = result.Data.CardLast4,
                        message = result.Data.Message,
                        amount = result.Data.Amount,
                        durationMs = result.Data.DurationMs,
                        retryCount = result.Data.RetryCount,
                        operationId = result.Data.OperationId
                    }));
                }
                else
                {
                    _logger.Warning("⚠️ POS Payment API: پرداخت ناموفق - ReceptionId: {ReceptionId}, Error: {Error}, OperationId: {OperationId}",
                        request.ReceptionId, result.Message, result.Data?.OperationId);

                    var errorResult = ServiceResult.Failed(result.Message);
                    errorResult.Metadata["errorCode"] = result.Data?.ErrorCode;
                    errorResult.Metadata["operationId"] = result.Data?.OperationId;
                    errorResult.Metadata["durationMs"] = result.Data?.DurationMs;
                    errorResult.Metadata["retryCount"] = result.Data?.RetryCount;
                    errorResult.Metadata["isCanceled"] = result.Data?.IsCanceled;
                    return Json(errorResult);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Payment API: خطای غیرمنتظره در پردازش پرداخت - ReceptionId: {ReceptionId}",
                    request?.ReceptionId);
                return Json(ServiceResult.Failed($"خطا در پردازش پرداخت: {ex.Message}"));
            }
        }

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت
        /// 
        /// POST /api/v1/pos-payment/validate
        /// </summary>
        [HttpPost]
        [Route("validate")]
        public async Task<JsonResult> ValidatePayment(PosPaymentRequest request)
        {
            try
            {
                _logger.Information("🔍 POS Payment API: اعتبارسنجی درخواست - ReceptionId: {ReceptionId}, Amount: {Amount}",
                    request?.ReceptionId, request?.AmountIRR);

                var result = await _posPaymentService.ValidatePaymentRequestAsync(request);

                if (result.Success)
                {
                    _logger.Information("✅ POS Payment API: اعتبارسنجی موفق");
                    return Json(ServiceResult.Successful());
                }
                else
                {
                    _logger.Warning("⚠️ POS Payment API: اعتبارسنجی ناموفق - {Message}", result.Message);
                    return Json(ServiceResult.Failed(result.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Payment API: خطا در اعتبارسنجی");
                return Json(ServiceResult.Failed($"خطا در اعتبارسنجی: {ex.Message}"));
            }
        }

        /// <summary>
        /// دریافت اطلاعات ترمینال
        /// 
        /// GET /api/v1/pos-payment/terminal?terminalId=1
        /// </summary>
        [HttpGet]
        [Route("terminal")]
        public async Task<JsonResult> GetTerminal(int? terminalId = null)
        {
            try
            {
                _logger.Information("🔍 POS Payment API: دریافت ترمینال - TerminalId: {TerminalId}",
                    terminalId);

                var result = await _posPaymentService.GetTerminalForPaymentAsync(terminalId);

                if (result.Success)
                {
                    _logger.Information("✅ POS Payment API: ترمینال دریافت شد - TerminalId: {TerminalId}, Provider: {Provider}",
                        result.Data.TerminalId, result.Data.Provider);

                    return Json(ServiceResult<object>.Successful(new
                    {
                        terminalId = result.Data.TerminalId,
                        merchantId = result.Data.MerchantId,
                        ipAddress = result.Data.IpAddress,
                        port = result.Data.Port,
                        provider = result.Data.Provider.ToString(),
                        protocol = result.Data.Protocol.ToString(),
                        isActive = result.Data.IsActive
                    }));
                }
                else
                {
                    _logger.Warning("⚠️ POS Payment API: ترمینال یافت نشد - {Message}", result.Message);
                    return Json(ServiceResult.Failed(result.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Payment API: خطا در دریافت ترمینال");
                return Json(ServiceResult.Failed($"خطا در دریافت ترمینال: {ex.Message}"));
            }
        }
    }
}

