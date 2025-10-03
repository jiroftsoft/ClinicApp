using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس برای مدیریت Structured Logging در ClinicApp
    /// </summary>
    public class StructuredLogger
    {
        private readonly ILogger _logger;
        private readonly string _sourceContext;

        public StructuredLogger(string sourceContext)
        {
            _logger = Log.ForContext("SourceContext", sourceContext);
            _sourceContext = sourceContext;
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Context
        /// </summary>
        public void LogOperation(string operation, object context = null, LogEventLevel level = LogEventLevel.Information)
        {
            _logger.Write(level, "🔵 OPERATION: {Operation} | Context: {@Context}", operation, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Timing
        /// </summary>
        public void LogOperationWithTiming(string operation, long durationMs, object context = null)
        {
            var level = durationMs switch
            {
                < 100 => LogEventLevel.Debug,
                < 1000 => LogEventLevel.Information,
                < 5000 => LogEventLevel.Warning,
                _ => LogEventLevel.Error
            };

            _logger.Write(level, "⚡ OPERATION_TIMING: {Operation} | Duration: {Duration}ms | Context: {@Context}",
                operation, durationMs, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Result
        /// </summary>
        public void LogOperationWithResult(string operation, bool success, object result = null, string error = null)
        {
            var level = success ? LogEventLevel.Information : LogEventLevel.Error;
            _logger.Write(level, "📊 OPERATION_RESULT: {Operation} | Success: {Success} | Result: {@Result} | Error: {Error}",
                operation, success, result, error);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Count
        /// </summary>
        public void LogOperationWithCount(string operation, int count, object context = null)
        {
            _logger.Information("📈 OPERATION_COUNT: {Operation} | Count: {Count} | Context: {@Context}",
                operation, count, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Progress
        /// </summary>
        public void LogProgress(string operation, int current, int total, string details = null)
        {
            var percentage = total > 0 ? (current * 100.0 / total) : 0;
            _logger.Information("📊 PROGRESS: {Operation} | {Current}/{Total} ({Percentage:F1}%) | Details: {Details}",
                operation, current, total, percentage, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Batch
        /// </summary>
        public void LogBatchOperation(string operation, int batchSize, int totalBatches, int currentBatch, object context = null)
        {
            _logger.Information("📦 BATCH_OPERATION: {Operation} | Batch: {CurrentBatch}/{TotalBatches} | Size: {BatchSize} | Context: {@Context}",
                operation, currentBatch, totalBatches, batchSize, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Validation
        /// </summary>
        public void LogValidation(string entityType, string operation, bool isValid, List<string> errors = null)
        {
            var level = isValid ? LogEventLevel.Debug : LogEventLevel.Warning;
            _logger.Write(level, "✅ VALIDATION: {EntityType} | {Operation} | Valid: {IsValid} | Errors: {@Errors}",
                entityType, operation, isValid, errors);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Performance Metrics
        /// </summary>
        public void LogPerformanceMetrics(string operation, Dictionary<string, object> metrics)
        {
            _logger.Information("📊 PERFORMANCE_METRICS: {Operation} | Metrics: {@Metrics}",
                operation, metrics);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با User Context
        /// </summary>
        public void LogUserOperation(string operation, string userId, object context = null)
        {
            _logger.Information("👤 USER_OPERATION: {Operation} | User: {UserId} | Context: {@Context}",
                operation, userId, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با System Context
        /// </summary>
        public void LogSystemOperation(string operation, string component, object context = null)
        {
            _logger.Information("⚙️ SYSTEM_OPERATION: {Operation} | Component: {Component} | Context: {@Context}",
                operation, component, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Business Context
        /// </summary>
        public void LogBusinessOperation(string operation, string businessArea, object context = null)
        {
            _logger.Information("💼 BUSINESS_OPERATION: {Operation} | Area: {BusinessArea} | Context: {@Context}",
                operation, businessArea, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Error Context
        /// </summary>
        public void LogErrorWithContext(string operation, Exception exception, object context = null)
        {
            _logger.Error(exception, "❌ ERROR_WITH_CONTEXT: {Operation} | Context: {@Context}",
                operation, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Warning Context
        /// </summary>
        public void LogWarningWithContext(string operation, string warning, object context = null)
        {
            _logger.Warning("⚠️ WARNING_WITH_CONTEXT: {Operation} | Warning: {Warning} | Context: {@Context}",
                operation, warning, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Debug Context
        /// </summary>
        public void LogDebugWithContext(string operation, string details, object context = null)
        {
            _logger.Debug("🔍 DEBUG_WITH_CONTEXT: {Operation} | Details: {Details} | Context: {@Context}",
                operation, details, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Info Context
        /// </summary>
        public void LogInfoWithContext(string operation, string details, object context = null)
        {
            _logger.Information("ℹ️ INFO_WITH_CONTEXT: {Operation} | Details: {Details} | Context: {@Context}",
                operation, details, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Fatal Context
        /// </summary>
        public void LogFatalWithContext(string operation, string details, object context = null)
        {
            _logger.Fatal("💀 FATAL_WITH_CONTEXT: {Operation} | Details: {Details} | Context: {@Context}",
                operation, details, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Custom Level
        /// </summary>
        public void LogWithCustomLevel(string operation, LogEventLevel level, string details, object context = null)
        {
            _logger.Write(level, "📝 CUSTOM_LOG: {Operation} | Details: {Details} | Context: {@Context}",
                operation, details, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Structured Data
        /// </summary>
        public void LogStructuredData(string operation, string dataType, object data, object context = null)
        {
            _logger.Information("📋 STRUCTURED_DATA: {Operation} | Type: {DataType} | Data: {@Data} | Context: {@Context}",
                operation, dataType, data, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Correlation ID
        /// </summary>
        public void LogWithCorrelationId(string operation, string correlationId, object context = null)
        {
            _logger.Information("🔗 CORRELATED_LOG: {Operation} | CorrelationId: {CorrelationId} | Context: {@Context}",
                operation, correlationId, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Request ID
        /// </summary>
        public void LogWithRequestId(string operation, string requestId, object context = null)
        {
            _logger.Information("🌐 REQUEST_LOG: {Operation} | RequestId: {RequestId} | Context: {@Context}",
                operation, requestId, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Session ID
        /// </summary>
        public void LogWithSessionId(string operation, string sessionId, object context = null)
        {
            _logger.Information("🔐 SESSION_LOG: {Operation} | SessionId: {SessionId} | Context: {@Context}",
                operation, sessionId, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Transaction ID
        /// </summary>
        public void LogWithTransactionId(string operation, string transactionId, object context = null)
        {
            _logger.Information("💳 TRANSACTION_LOG: {Operation} | TransactionId: {TransactionId} | Context: {@Context}",
                operation, transactionId, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Patient ID
        /// </summary>
        public void LogWithPatientId(string operation, string patientId, object context = null)
        {
            _logger.Information("🏥 PATIENT_LOG: {Operation} | PatientId: {PatientId} | Context: {@Context}",
                operation, patientId, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Doctor ID
        /// </summary>
        public void LogWithDoctorId(string operation, string doctorId, object context = null)
        {
            _logger.Information("👨‍⚕️ DOCTOR_LOG: {Operation} | DoctorId: {DoctorId} | Context: {@Context}",
                operation, doctorId, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Insurance ID
        /// </summary>
        public void LogWithInsuranceId(string operation, string insuranceId, object context = null)
        {
            _logger.Information("🏛️ INSURANCE_LOG: {Operation} | InsuranceId: {InsuranceId} | Context: {@Context}",
                operation, insuranceId, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Service ID
        /// </summary>
        public void LogWithServiceId(string operation, string serviceId, object context = null)
        {
            _logger.Information("🔧 SERVICE_LOG: {Operation} | ServiceId: {ServiceId} | Context: {@Context}",
                operation, serviceId, context);
        }

        /// <summary>
        /// لاگ‌گیری عملیات با Department ID
        /// </summary>
        public void LogWithDepartmentId(string operation, string departmentId, object context = null)
        {
            _logger.Information("🏢 DEPARTMENT_LOG: {Operation} | DepartmentId: {DepartmentId} | Context: {@Context}",
                operation, departmentId, context);
        }
    }
}
