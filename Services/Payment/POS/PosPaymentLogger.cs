using Serilog;
using System;
using System.Diagnostics;

namespace ClinicApp.Services.Payment.POS
{
    /// <summary>
    /// Production-Ready POS Payment Logger
    /// 
    /// مسئولیت: Logging اختصاصی برای پرداخت POS
    /// 
    /// ویژگی‌های کلیدی:
    /// ✅ Structured Logging
    /// ✅ Performance Metrics
    /// ✅ Error Tracking
    /// ✅ Transaction Tracking
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط Logging
    /// - Separation of Concerns: جدا از Business Logic
    /// </summary>
    public class PosPaymentLogger
    {
        private readonly ILogger _logger;
        private readonly string _context;

        public PosPaymentLogger(ILogger logger, string context = "PosPayment")
        {
            _logger = logger?.ForContext<PosPaymentLogger>() ?? throw new ArgumentNullException(nameof(logger));
            _context = context;
        }

        /// <summary>
        /// Log شروع پرداخت
        /// </summary>
        public void LogPaymentStart(string operationId, int receptionId, decimal amount, int? terminalId, string userId)
        {
            _logger.Information("💳 [{Context}] [{OperationId}] شروع پرداخت - ReceptionId: {ReceptionId}, Amount: {Amount}, TerminalId: {TerminalId}, UserId: {UserId}",
                _context, operationId, receptionId, amount, terminalId, userId);
        }

        /// <summary>
        /// Log موفقیت پرداخت
        /// </summary>
        public void LogPaymentSuccess(string operationId, string rrn, string traceNo, string cardLast4, long durationMs, int retryCount)
        {
            _logger.Information("✅ [{Context}] [{OperationId}] پرداخت موفق - RRN: {RRN}, TraceNo: {TraceNo}, CardLast4: {CardLast4}, Duration: {Duration}ms, RetryCount: {RetryCount}",
                _context, operationId, rrn, traceNo, cardLast4, durationMs, retryCount);
        }

        /// <summary>
        /// Log خطای پرداخت
        /// </summary>
        public void LogPaymentError(string operationId, string errorCode, string message, long durationMs, int retryCount, Exception ex = null)
        {
            if (ex != null)
            {
                _logger.Error(ex, "❌ [{Context}] [{OperationId}] پرداخت ناموفق - ErrorCode: {ErrorCode}, Message: {Message}, Duration: {Duration}ms, RetryCount: {RetryCount}",
                    _context, operationId, errorCode, message, durationMs, retryCount);
            }
            else
            {
                _logger.Error("❌ [{Context}] [{OperationId}] پرداخت ناموفق - ErrorCode: {ErrorCode}, Message: {Message}, Duration: {Duration}ms, RetryCount: {RetryCount}",
                    _context, operationId, errorCode, message, durationMs, retryCount);
            }
        }

        /// <summary>
        /// Log لغو پرداخت
        /// </summary>
        public void LogPaymentCancel(string operationId, string reason, long durationMs)
        {
            _logger.Warning("⚠️ [{Context}] [{OperationId}] پرداخت لغو شد - Reason: {Reason}, Duration: {Duration}ms",
                _context, operationId, reason, durationMs);
        }

        /// <summary>
        /// Log Retry
        /// </summary>
        public void LogRetry(string operationId, int attemptNumber, int maxAttempts, string reason)
        {
            _logger.Information("🔄 [{Context}] [{OperationId}] تلاش مجدد - Attempt: {Attempt}/{MaxAttempts}, Reason: {Reason}",
                _context, operationId, attemptNumber, maxAttempts, reason);
        }

        /// <summary>
        /// Log Connection Event
        /// </summary>
        public void LogConnection(string operationId, string eventType, string details)
        {
            _logger.Information("🔌 [{Context}] [{OperationId}] Connection Event - Type: {EventType}, Details: {Details}",
                _context, operationId, eventType, details);
        }

        /// <summary>
        /// Log Performance Metric
        /// </summary>
        public void LogPerformance(string operationId, string metricName, long valueMs, string unit = "ms")
        {
            _logger.Debug("📊 [{Context}] [{OperationId}] Performance - {MetricName}: {Value}{Unit}",
                _context, operationId, metricName, valueMs, unit);
        }

        /// <summary>
        /// Log Step
        /// </summary>
        public void LogStep(string operationId, string stepName, bool success, long durationMs, string details = null)
        {
            var icon = success ? "✅" : "❌";
            if (details != null)
            {
                _logger.Information("{Icon} [{Context}] [{OperationId}] Step: {StepName} - Success: {Success}, Duration: {Duration}ms, Details: {Details}",
                    icon, _context, operationId, stepName, success, durationMs, details);
            }
            else
            {
                _logger.Information("{Icon} [{Context}] [{OperationId}] Step: {StepName} - Success: {Success}, Duration: {Duration}ms",
                    icon, _context, operationId, stepName, success, durationMs);
            }
        }

        /// <summary>
        /// Create Performance Stopwatch
        /// </summary>
        public Stopwatch StartPerformanceTimer()
        {
            return Stopwatch.StartNew();
        }

        /// <summary>
        /// Log Performance from Stopwatch
        /// </summary>
        public void LogPerformanceTimer(string operationId, string metricName, Stopwatch stopwatch)
        {
            stopwatch.Stop();
            LogPerformance(operationId, metricName, stopwatch.ElapsedMilliseconds);
        }
    }
}

