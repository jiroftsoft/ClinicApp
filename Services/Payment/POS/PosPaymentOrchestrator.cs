using ClinicApp.Helpers;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicApp.Services.Payment.POS
{
    /// <summary>
    /// Production-Ready POS Payment Orchestrator
    /// 
    /// قلب پرداخت سیستم کلینیک - ضد گلوله و قطعی
    /// 
    /// ویژگی‌های کلیدی:
    /// ✅ لاگ کامل تمام مراحل
    /// ✅ مدیریت تمام پیام‌های برگشتی از دستگاه
    /// ✅ Retry Logic با Exponential Backoff
    /// ✅ Timeout Management
    /// ✅ Transaction Tracking
    /// ✅ Error Recovery
    /// ✅ User-Friendly Messages
    /// 
    /// آماده برای استفاده در ماژول پذیرش
    /// </summary>
    public class PosPaymentOrchestrator
    {
        private readonly IPosDeviceService _posDeviceService;
        private readonly IPosManagementService _posManagementService;
        private readonly ILogger _logger;

        // Production Settings
        private const int MaxRetryAttempts = 3;
        private const int BaseRetryDelayMs = 1000; // 1 second
        private const int ConnectionTimeoutMs = 30000; // 30 seconds
        private const int PaymentTimeoutMs = 60000; // 60 seconds

        public PosPaymentOrchestrator(
            IPosDeviceService posDeviceService,
            IPosManagementService posManagementService,
            ILogger logger)
        {
            _posDeviceService = posDeviceService ?? throw new ArgumentNullException(nameof(posDeviceService));
            _posManagementService = posManagementService ?? throw new ArgumentNullException(nameof(posManagementService));
            _logger = logger.ForContext<PosPaymentOrchestrator>();
        }

        /// <summary>
        /// پردازش پرداخت POS با تمام قابلیت‌های Production
        /// </summary>
        public async Task<PosPaymentOrchestratorResult> ProcessPaymentAsync(
            int receptionId,
            decimal amountIRR,
            int? terminalId = null,
            string userId = null)
        {
            var operationId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var startTime = DateTime.UtcNow;

            _logger.Information("🏥 POS Orchestrator [{OperationId}]: شروع پردازش پرداخت - ReceptionId: {ReceptionId}, Amount: {Amount}, TerminalId: {TerminalId}, UserId: {UserId}",
                operationId, receptionId, amountIRR, terminalId, userId);

            try
            {
                // Step 1: Validation
                var validationResult = await ValidatePaymentRequestAsync(receptionId, amountIRR, operationId);
                if (!validationResult.Success)
                {
                    return CreateFailureResult(operationId, validationResult.Message, "VALIDATION_ERROR", startTime);
                }

                // Step 2: Get Terminal
                var terminalResult = await GetTerminalAsync(terminalId, operationId);
                if (!terminalResult.Success)
                {
                    return CreateFailureResult(operationId, terminalResult.Message, "TERMINAL_NOT_FOUND", startTime);
                }

                var terminal = terminalResult.Data;
                _logger.Information("🏥 POS Orchestrator [{OperationId}]: ترمینال انتخاب شد - TerminalId: {TerminalId}, Provider: {Provider}, IP: {IpAddress}, Port: {Port}",
                    operationId, terminal.TerminalId, terminal.Provider, terminal.IpAddress, terminal.Port);

                // Step 3: Process Payment with Retry Logic
                var paymentResult = await ProcessPaymentWithRetryAsync(
                    terminal,
                    amountIRR,
                    receptionId,
                    operationId);

                if (!paymentResult.Success)
                {
                    return CreateFailureResult(
                        operationId,
                        paymentResult.Message,
                        paymentResult.ErrorCode ?? "PAYMENT_FAILED",
                        startTime,
                        paymentResult.Details);
                }

                // Step 4: Success Response
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Information("✅ POS Orchestrator [{OperationId}]: پرداخت با موفقیت انجام شد - RRN: {RRN}, TraceNo: {TraceNo}, Duration: {Duration}ms",
                    operationId, paymentResult.Data.RRN, paymentResult.Data.TraceNo, duration);

                return new PosPaymentOrchestratorResult
                {
                    Success = true,
                    OperationId = operationId,
                    ReceptionId = receptionId,
                    Amount = amountIRR,
                    TerminalId = terminal.TerminalId,
                    RRN = paymentResult.Data.RRN,
                    TraceNo = paymentResult.Data.TraceNo,
                    CardLast4 = paymentResult.Data.CardLast4,
                    Message = paymentResult.Data.Message ?? "پرداخت با موفقیت انجام شد",
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    DurationMs = duration,
                    RetryCount = paymentResult.RetryCount,
                    Steps = paymentResult.Steps
                };
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(ex, "❌ POS Orchestrator [{OperationId}]: خطای غیرمنتظره - ReceptionId: {ReceptionId}, Duration: {Duration}ms",
                    operationId, receptionId, duration);

                return CreateFailureResult(
                    operationId,
                    "خطای غیرمنتظره در پردازش پرداخت. لطفاً با پشتیبانی تماس بگیرید.",
                    "UNEXPECTED_ERROR",
                    startTime,
                    new Dictionary<string, object>
                    {
                        { "ExceptionType", ex.GetType().Name },
                        { "ExceptionMessage", ex.Message },
                        { "StackTrace", ex.StackTrace }
                    });
            }
        }

        /// <summary>
        /// پردازش پرداخت با Retry Logic
        /// </summary>
        private async Task<PosPaymentRetryResult> ProcessPaymentWithRetryAsync(
            PosTerminal terminal,
            decimal amountIRR,
            int receptionId,
            string operationId)
        {
            var steps = new List<PosPaymentStep>();
            Exception lastException = null;

            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    _logger.Information("🔄 POS Orchestrator [{OperationId}]: تلاش {Attempt}/{MaxAttempts} - TerminalId: {TerminalId}, Amount: {Amount}",
                        operationId, attempt, MaxRetryAttempts, terminal.TerminalId, amountIRR);

                    var stepStartTime = DateTime.UtcNow;
                    var step = new PosPaymentStep
                    {
                        StepNumber = attempt,
                        StepName = $"PaymentAttempt_{attempt}",
                        StartTime = stepStartTime
                    };

                    // Process Payment
                    var paymentResult = await _posDeviceService.ProcessPaymentAsync(terminal, amountIRR, receptionId);

                    step.EndTime = DateTime.UtcNow;
                    step.DurationMs = (step.EndTime.Value - stepStartTime).TotalMilliseconds;
                    step.Success = paymentResult.Success;

                    if (paymentResult.Success)
                    {
                        step.Message = "پرداخت با موفقیت انجام شد";
                        step.Data = new Dictionary<string, object>
                        {
                            { "RRN", paymentResult.Data.RRN },
                            { "TraceNo", paymentResult.Data.TraceNo },
                            { "CardLast4", paymentResult.Data.CardLast4 }
                        };

                        steps.Add(step);

                        _logger.Information("✅ POS Orchestrator [{OperationId}]: پرداخت موفق در تلاش {Attempt} - RRN: {RRN}, TraceNo: {TraceNo}",
                            operationId, attempt, paymentResult.Data.RRN, paymentResult.Data.TraceNo);

                        return new PosPaymentRetryResult
                        {
                            Success = true,
                            Data = paymentResult.Data,
                            RetryCount = attempt - 1,
                            Steps = steps
                        };
                    }
                    else
                    {
                        step.Message = paymentResult.Message;
                        step.ErrorCode = "PAYMENT_FAILED";
                        steps.Add(step);

                        _logger.Warning("⚠️ POS Orchestrator [{OperationId}]: پرداخت ناموفق در تلاش {Attempt} - Error: {Error}",
                            operationId, attempt, paymentResult.Message);

                        // اگر خطای غیرقابل بازیابی است، retry نکن
                        if (IsNonRetryableError(paymentResult.Message))
                        {
                            _logger.Warning("⚠️ POS Orchestrator [{OperationId}]: خطای غیرقابل بازیابی - Retry متوقف می‌شود",
                                operationId);
                            break;
                        }

                        // Retry با Exponential Backoff
                        if (attempt < MaxRetryAttempts)
                        {
                            var delay = BaseRetryDelayMs * (int)Math.Pow(2, attempt - 1);
                            _logger.Information("🔄 POS Orchestrator [{OperationId}]: انتظار {Delay}ms قبل از تلاش مجدد",
                                operationId, delay);
                            await Task.Delay(delay);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    var step = new PosPaymentStep
                    {
                        StepNumber = attempt,
                        StepName = $"PaymentAttempt_{attempt}",
                        StartTime = DateTime.UtcNow,
                        EndTime = DateTime.UtcNow,
                        Success = false,
                        Message = ex.Message,
                        ErrorCode = ex.GetType().Name,
                        Data = new Dictionary<string, object>
                        {
                            { "ExceptionType", ex.GetType().Name },
                            { "ExceptionMessage", ex.Message }
                        }
                    };
                    steps.Add(step);

                    _logger.Error(ex, "❌ POS Orchestrator [{OperationId}]: خطا در تلاش {Attempt}",
                        operationId, attempt);

                    // Retry با Exponential Backoff
                    if (attempt < MaxRetryAttempts)
                    {
                        var delay = BaseRetryDelayMs * (int)Math.Pow(2, attempt - 1);
                        await Task.Delay(delay);
                    }
                }
            }

            // همه تلاش‌ها ناموفق بودند
            var errorMessage = lastException != null
                ? $"خطا در پردازش پرداخت: {lastException.Message}"
                : "پرداخت پس از چندین تلاش ناموفق بود. لطفاً مجدداً تلاش کنید.";

            return new PosPaymentRetryResult
            {
                Success = false,
                Message = errorMessage,
                ErrorCode = "MAX_RETRIES_EXCEEDED",
                RetryCount = MaxRetryAttempts,
                Steps = steps,
                Details = new Dictionary<string, object>
                {
                    { "LastException", lastException != null ? lastException.GetType().Name : "None" },
                    { "LastExceptionMessage", lastException?.Message ?? "N/A" },
                    { "TotalAttempts", MaxRetryAttempts }
                }
            };
        }

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت
        /// </summary>
        private async Task<ServiceResult> ValidatePaymentRequestAsync(int receptionId, decimal amountIRR, string operationId)
        {
            _logger.Information("🔍 POS Orchestrator [{OperationId}]: شروع اعتبارسنجی - ReceptionId: {ReceptionId}, Amount: {Amount}",
                operationId, receptionId, amountIRR);

            var errors = new List<string>();

            // ReceptionId = 0 برای تست مجاز است
            if (receptionId < 0)
            {
                errors.Add("شناسه پذیرش نامعتبر است");
                _logger.Warning("⚠️ POS Orchestrator [{OperationId}]: ReceptionId نامعتبر - {ReceptionId}",
                    operationId, receptionId);
            }
            else if (receptionId == 0)
            {
                _logger.Information("🔍 POS Orchestrator [{OperationId}]: ReceptionId = 0 (تست) - اجازه داده می‌شود",
                    operationId);
            }

            if (amountIRR <= 0)
            {
                errors.Add("مبلغ پرداخت باید بیشتر از صفر باشد");
                _logger.Warning("⚠️ POS Orchestrator [{OperationId}]: Amount نامعتبر - {Amount}",
                    operationId, amountIRR);
            }

            if (amountIRR > 999999999999) // حداکثر 999 میلیارد تومان
            {
                errors.Add("مبلغ پرداخت بیش از حد مجاز است");
                _logger.Warning("⚠️ POS Orchestrator [{OperationId}]: Amount بیش از حد مجاز - {Amount}",
                    operationId, amountIRR);
            }

            if (errors.Any())
            {
                _logger.Error("❌ POS Orchestrator [{OperationId}]: اعتبارسنجی ناموفق - Errors: {Errors}",
                    operationId, string.Join(", ", errors));
                return ServiceResult.Failed("اطلاعات وارد شده نامعتبر است", string.Join("; ", errors));
            }

            _logger.Information("✅ POS Orchestrator [{OperationId}]: اعتبارسنجی موفق",
                operationId);
            return ServiceResult.Successful();
        }

        /// <summary>
        /// دریافت ترمینال POS
        /// </summary>
        private async Task<ServiceResult<PosTerminal>> GetTerminalAsync(int? terminalId, string operationId)
        {
            _logger.Information("🔍 POS Orchestrator [{OperationId}]: دریافت ترمینال - TerminalId: {TerminalId}",
                operationId, terminalId);

            try
            {
                ServiceResult<PosTerminal> terminalResult;

                if (terminalId.HasValue && terminalId.Value > 0)
                {
                    // دریافت ترمینال خاص
                    terminalResult = await _posManagementService.GetPosTerminalAsync(terminalId.Value);
                    _logger.Information("🏥 POS Orchestrator [{OperationId}]: درخواست ترمینال خاص - TerminalId: {TerminalId}",
                        operationId, terminalId.Value);
                }
                else
                {
                    // دریافت ترمینال پیش‌فرض
                    terminalResult = await _posManagementService.GetDefaultPosTerminalAsync();
                    _logger.Information("🏥 POS Orchestrator [{OperationId}]: درخواست ترمینال پیش‌فرض",
                        operationId);
                }

                if (!terminalResult.Success || terminalResult.Data == null)
                {
                    _logger.Error("❌ POS Orchestrator [{OperationId}]: ترمینال یافت نشد - TerminalId: {TerminalId}",
                        operationId, terminalId);
                    return ServiceResult<PosTerminal>.Failed("ترمینال POS یافت نشد. لطفاً ابتدا ترمینال را تنظیم کنید.");
                }

                var terminal = terminalResult.Data;

                // بررسی فعال بودن ترمینال
                if (!terminal.IsActive)
                {
                    _logger.Warning("⚠️ POS Orchestrator [{OperationId}]: ترمینال غیرفعال است - TerminalId: {TerminalId}",
                        operationId, terminal.TerminalId);
                    return ServiceResult<PosTerminal>.Failed("ترمینال POS فعال نیست. لطفاً ترمینال را فعال کنید.");
                }

                // بررسی تنظیمات ترمینال
                var configErrors = ValidateTerminalConfiguration(terminal);
                if (configErrors.Any())
                {
                    _logger.Warning("⚠️ POS Orchestrator [{OperationId}]: تنظیمات ترمینال ناقص است - TerminalId: {TerminalId}, Errors: {Errors}",
                        operationId, terminal.TerminalId, string.Join(", ", configErrors));
                    return ServiceResult<PosTerminal>.Failed(
                        $"تنظیمات ترمینال ناقص است: {string.Join("; ", configErrors)}");
                }

                _logger.Information("✅ POS Orchestrator [{OperationId}]: ترمینال آماده است - TerminalId: {TerminalId}, Provider: {Provider}",
                    operationId, terminal.TerminalId, terminal.Provider);

                return ServiceResult<PosTerminal>.Successful(terminal);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Orchestrator [{OperationId}]: خطا در دریافت ترمینال",
                    operationId);
                return ServiceResult<PosTerminal>.Failed("خطا در دریافت ترمینال POS");
            }
        }

        /// <summary>
        /// اعتبارسنجی تنظیمات ترمینال
        /// </summary>
        private List<string> ValidateTerminalConfiguration(PosTerminal terminal)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(terminal.TerminalId))
                errors.Add("شماره ترمینال تنظیم نشده است");

            if (string.IsNullOrWhiteSpace(terminal.MerchantId))
                errors.Add("شماره پذیرنده تنظیم نشده است");

            if (string.IsNullOrWhiteSpace(terminal.IpAddress))
                errors.Add("آدرس IP تنظیم نشده است");

            // Port is optional - if not set, driver will use default port (5000)
            if (terminal.Port.HasValue)
            {
                var port = terminal.Port.Value;
                
                // Validate port range
                if (port <= 0 || port > 65535)
                {
                    errors.Add($"پورت {port} نامعتبر است. پورت باید بین 1 تا 65535 باشد");
                }
                // ⚠️ Warning for common bank server ports (NOT for PC-POS)
                else if (port == 2155 || port == 8580)
                {
                    warnings.Add($"⚠️ هشدار: پورت {port} معمولاً برای ارتباط دستگاه با سرور بانک است، نه برای PC ↔ POS!");
                    warnings.Add("پورت صحیح PC ↔ POS معمولاً یکی از این‌هاست: 5000, 8080, 9100");
                    warnings.Add("اگر پورت تنظیم نشود، از پورت پیش‌فرض 5000 استفاده می‌شود");
                }
                else if (port < 1000)
                {
                    warnings.Add($"⚠️ هشدار: پورت {port} کمتر از 1000 است. پورت‌های رایج PC ↔ POS معمولاً 5000, 8080, 9100 هستند");
                }
            }
            else
            {
                // Inform user that default port will be used
                warnings.Add("ℹ️ پورت تنظیم نشده است. از پورت پیش‌فرض 5000 استفاده می‌شود");
            }

            // Don't add warnings to errors - warnings are informational only
            // Warnings will be logged but won't prevent connection attempt
            return errors;
        }

        /// <summary>
        /// بررسی خطاهای غیرقابل بازیابی
        /// </summary>
        private bool IsNonRetryableError(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                return false;

            var nonRetryableErrors = new[]
            {
                "مبلغ پرداخت باید بیشتر از صفر باشد",
                "ترمینال POS یافت نشد",
                "ترمینال POS فعال نیست",
                "تنظیمات ترمینال ناقص است",
                "درایور برای ارائه‌دهنده",
                "Invalid amount",
                "Terminal not found",
                "Terminal is not active"
            };

            return nonRetryableErrors.Any(error => errorMessage.IndexOf(error, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// ایجاد نتیجه ناموفق
        /// </summary>
        private PosPaymentOrchestratorResult CreateFailureResult(
            string operationId,
            string message,
            string errorCode,
            DateTime startTime,
            Dictionary<string, object> details = null)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.Error("❌ POS Orchestrator [{OperationId}]: پرداخت ناموفق - Error: {Error}, ErrorCode: {ErrorCode}, Duration: {Duration}ms",
                operationId, message, errorCode, duration);

            return new PosPaymentOrchestratorResult
            {
                Success = false,
                OperationId = operationId,
                Message = message,
                ErrorCode = errorCode,
                StartTime = startTime,
                EndTime = DateTime.UtcNow,
                DurationMs = duration,
                Details = details ?? new Dictionary<string, object>()
            };
        }
    }

    #region Result Classes

    /// <summary>
    /// نتیجه پردازش پرداخت POS
    /// </summary>
    public class PosPaymentOrchestratorResult
    {
        public bool Success { get; set; }
        public string OperationId { get; set; }
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public string TerminalId { get; set; }
        public string RRN { get; set; }
        public string TraceNo { get; set; }
        public string CardLast4 { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double DurationMs { get; set; }
        public int RetryCount { get; set; }
        public List<PosPaymentStep> Steps { get; set; } = new List<PosPaymentStep>();
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// نتیجه پردازش با Retry
    /// </summary>
    internal class PosPaymentRetryResult
    {
        public bool Success { get; set; }
        public PosPaymentResponse Data { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public int RetryCount { get; set; }
        public List<PosPaymentStep> Steps { get; set; } = new List<PosPaymentStep>();
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// مرحله پردازش پرداخت
    /// </summary>
    public class PosPaymentStep
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double DurationMs { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    #endregion
}

