using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس کمکی برای مدیریت پیشرفته لاگ‌گیری در ClinicApp
    /// </summary>
    public static class LoggingHelper
    {
        /// <summary>
        /// لاگ‌گیری عملیات‌های مهم کسب‌وکار
        /// </summary>
        public static void LogBusinessOperation(string operation, string userId, string details, object data = null)
        {
            Log.Information("🔵 BUSINESS_OPERATION: {Operation} | User: {UserId} | Details: {Details} | Data: {@Data}",
                operation, userId, details, data);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های امنیتی
        /// </summary>
        public static void LogSecurityEvent(string eventType, string userId, string ipAddress, string details)
        {
            Log.Warning("🔒 SECURITY_EVENT: {EventType} | User: {UserId} | IP: {IpAddress} | Details: {Details}",
                eventType, userId, ipAddress, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های مالی
        /// </summary>
        public static void LogFinancialOperation(string operation, decimal amount, string currency, string userId, string details)
        {
            Log.Information("💰 FINANCIAL_OPERATION: {Operation} | Amount: {Amount} {Currency} | User: {UserId} | Details: {Details}",
                operation, amount, currency, userId, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های پزشکی
        /// </summary>
        public static void LogMedicalOperation(string operation, string patientId, string doctorId, string details)
        {
            Log.Information("🏥 MEDICAL_OPERATION: {Operation} | Patient: {PatientId} | Doctor: {DoctorId} | Details: {Details}",
                operation, patientId, doctorId, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Performance
        /// </summary>
        public static void LogPerformance(string operation, long durationMs, string details = null)
        {
            var level = durationMs switch
            {
                < 100 => LogEventLevel.Debug,
                < 1000 => LogEventLevel.Information,
                < 5000 => LogEventLevel.Warning,
                _ => LogEventLevel.Error
            };

            Log.Write(level, "⚡ PERFORMANCE: {Operation} | Duration: {Duration}ms | Details: {Details}",
                operation, durationMs, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Database
        /// </summary>
        public static void LogDatabaseOperation(string operation, string tableName, int recordCount, long durationMs)
        {
            Log.Information("🗄️ DATABASE_OPERATION: {Operation} | Table: {TableName} | Records: {RecordCount} | Duration: {Duration}ms",
                operation, tableName, recordCount, durationMs);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Cache
        /// </summary>
        public static void LogCacheOperation(string operation, string key, bool hit, long durationMs = 0)
        {
            var level = hit ? LogEventLevel.Debug : LogEventLevel.Information;
            Log.Write(level, "💾 CACHE_OPERATION: {Operation} | Key: {Key} | Hit: {Hit} | Duration: {Duration}ms",
                operation, key, hit, durationMs);
        }

        /// <summary>
        /// لاگ‌گیری Health Check
        /// </summary>
        public static void LogHealthCheck(string service, bool isHealthy, string details = null)
        {
            var level = isHealthy ? LogEventLevel.Information : LogEventLevel.Warning;
            Log.Write(level, "🏥 HEALTH_CHECK: {Service} | Status: {Status} | Details: {Details}",
                service, isHealthy ? "Healthy" : "Unhealthy", details);
        }

        /// <summary>
        /// لاگ‌گیری Audit Trail
        /// </summary>
        public static void LogAuditTrail(string entity, string operation, string entityId, string userId, object oldValues = null, object newValues = null)
        {
            Log.Information("📋 AUDIT_TRAIL: {Entity} | {Operation} | EntityId: {EntityId} | User: {UserId} | Old: {@OldValues} | New: {@NewValues}",
                entity, operation, entityId, userId, oldValues, newValues);
        }

        /// <summary>
        /// لاگ‌گیری Error Tracking
        /// </summary>
        public static void LogErrorTracking(string errorType, string errorMessage, string userId = null, object context = null)
        {
            Log.Error("🚨 ERROR_TRACKING: {ErrorType} | Message: {ErrorMessage} | User: {UserId} | Context: {@Context}",
                errorType, errorMessage, userId, context);
        }

        /// <summary>
        /// لاگ‌گیری Performance Metrics
        /// </summary>
        public static void LogPerformanceMetrics(string metricName, double value, string unit, string details = null)
        {
            Log.Information("📊 PERFORMANCE_METRICS: {MetricName} | Value: {Value} {Unit} | Details: {Details}",
                metricName, value, unit, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های API
        /// </summary>
        public static void LogApiCall(string method, string endpoint, int statusCode, long durationMs, string userId = null)
        {
            var level = statusCode switch
            {
                >= 200 and < 300 => LogEventLevel.Debug,
                >= 300 and < 400 => LogEventLevel.Information,
                >= 400 and < 500 => LogEventLevel.Warning,
                >= 500 => LogEventLevel.Error,
                _ => LogEventLevel.Warning
            };

            Log.Write(level, "🌐 API_CALL: {Method} {Endpoint} | Status: {StatusCode} | Duration: {Duration}ms | User: {UserId}",
                method, endpoint, statusCode, durationMs, userId);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Seed
        /// </summary>
        public static void LogSeedOperation(string operation, int recordCount, bool success, string details = null)
        {
            var level = success ? LogEventLevel.Information : LogEventLevel.Error;
            Log.Write(level, "🌱 SEED_OPERATION: {Operation} | Records: {RecordCount} | Success: {Success} | Details: {Details}",
                operation, recordCount, success, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Validation
        /// </summary>
        public static void LogValidation(string entityType, string operation, bool isValid, string errors = null)
        {
            var level = isValid ? LogEventLevel.Debug : LogEventLevel.Warning;
            Log.Write(level, "✅ VALIDATION: {EntityType} | {Operation} | Valid: {IsValid} | Errors: {Errors}",
                entityType, operation, isValid, errors);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Integration
        /// </summary>
        public static void LogIntegration(string serviceName, string operation, bool success, string details = null)
        {
            var level = success ? LogEventLevel.Information : LogEventLevel.Error;
            Log.Write(level, "🔗 INTEGRATION: {ServiceName} | {Operation} | Success: {Success} | Details: {Details}",
                serviceName, operation, success, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Background Job
        /// </summary>
        public static void LogBackgroundJob(string jobName, string status, int processedCount, string details = null)
        {
            Log.Information("⚙️ BACKGROUND_JOB: {JobName} | Status: {Status} | Processed: {ProcessedCount} | Details: {Details}",
                jobName, status, processedCount, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Cleanup
        /// </summary>
        public static void LogCleanup(string operation, int deletedCount, string details = null)
        {
            Log.Information("🧹 CLEANUP: {Operation} | Deleted: {DeletedCount} | Details: {Details}",
                operation, deletedCount, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Export/Import
        /// </summary>
        public static void LogDataTransfer(string operation, string format, int recordCount, bool success, string details = null)
        {
            var level = success ? LogEventLevel.Information : LogEventLevel.Error;
            Log.Write(level, "📊 DATA_TRANSFER: {Operation} | Format: {Format} | Records: {RecordCount} | Success: {Success} | Details: {Details}",
                operation, format, recordCount, success, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Report
        /// </summary>
        public static void LogReportGeneration(string reportType, string parameters, int recordCount, long durationMs)
        {
            Log.Information("📈 REPORT: {ReportType} | Parameters: {Parameters} | Records: {RecordCount} | Duration: {Duration}ms",
                reportType, parameters, recordCount, durationMs);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Notification
        /// </summary>
        public static void LogNotification(string type, string recipient, bool success, string details = null)
        {
            var level = success ? LogEventLevel.Information : LogEventLevel.Warning;
            Log.Write(level, "📧 NOTIFICATION: {Type} | Recipient: {Recipient} | Success: {Success} | Details: {Details}",
                type, recipient, success, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Audit
        /// </summary>
        public static void LogAudit(string entityType, string entityId, string operation, string userId, string changes = null)
        {
            Log.Information("📋 AUDIT: {EntityType} | ID: {EntityId} | Operation: {Operation} | User: {UserId} | Changes: {Changes}",
                entityType, entityId, operation, userId, changes);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های System Health
        /// </summary>
        public static void LogSystemHealth(string component, string status, string details = null)
        {
            var level = status.ToLower() switch
            {
                "healthy" => LogEventLevel.Debug,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "critical" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };

            Log.Write(level, "🏥 SYSTEM_HEALTH: {Component} | Status: {Status} | Details: {Details}",
                component, status, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های User Activity
        /// </summary>
        public static void LogUserActivity(string userId, string action, string details = null)
        {
            Log.Information("👤 USER_ACTIVITY: User: {UserId} | Action: {Action} | Details: {Details}",
                userId, action, details);
        }

        /// <summary>
        /// لاگ‌گیری عملیات‌های Error Recovery
        /// </summary>
        public static void LogErrorRecovery(string operation, string errorType, bool recovered, string details = null)
        {
            var level = recovered ? LogEventLevel.Information : LogEventLevel.Error;
            Log.Write(level, "🔄 ERROR_RECOVERY: {Operation} | Error: {ErrorType} | Recovered: {Recovered} | Details: {Details}",
                operation, errorType, recovered, details);
        }
    }
}
