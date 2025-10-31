using System;
using Serilog;
using Serilog.Events;

namespace ClinicApp.Helpers.Security
{
    /// <summary>
    /// 🔒 Security Logger برای ثبت رویدادهای امنیتی طبق قرارداد 04-Security-Requirements
    /// 
    /// این کلاس مسئولیت ثبت تمام رویدادهای امنیتی را بر عهده دارد:
    /// - تلاش‌های ورود (موفق/ناموفق)
    /// - دسترسی به داده‌های حساس
    /// - نقض‌های امنیتی
    /// - تغییرات مجوزها
    /// </summary>
    public static class SecurityLogger
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(SecurityLogger));

        /// <summary>
        /// ثبت تلاش ورود کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="success">وضعیت موفقیت</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userAgent">User Agent</param>
        /// <param name="additionalInfo">اطلاعات اضافی</param>
        public static void LogLoginAttempt(string userId, bool success, string ipAddress = null, string userAgent = null, object additionalInfo = null)
        {
            var level = success ? LogEventLevel.Information : LogEventLevel.Warning;
            _logger.Write(level, 
                "🔐 SECURITY_LOGIN: UserId: {UserId} | Success: {Success} | IP: {IPAddress} | UserAgent: {UserAgent} | AdditionalInfo: {@AdditionalInfo}",
                userId, success, ipAddress, userAgent, additionalInfo);
        }

        /// <summary>
        /// ثبت دسترسی به داده‌های حساس
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="entityType">نوع موجودیت</param>
        /// <param name="entityId">شناسه موجودیت</param>
        /// <param name="action">عمل انجام شده</param>
        /// <param name="ipAddress">آدرس IP</param>
        public static void LogDataAccess(string userId, string entityType, string entityId, string action, string ipAddress = null)
        {
            _logger.Information(
                "🔒 SECURITY_DATA_ACCESS: UserId: {UserId} | EntityType: {EntityType} | EntityId: {EntityId} | Action: {Action} | IP: {IPAddress}",
                userId, entityType, entityId, action, ipAddress);
        }

        /// <summary>
        /// ثبت نقض امنیتی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="violationType">نوع نقض</param>
        /// <param name="details">جزئیات</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="severity">شدت نقض (Low, Medium, High, Critical)</param>
        public static void LogSecurityViolation(string userId, string violationType, string details, string ipAddress = null, string severity = "Medium")
        {
            var level = severity switch
            {
                "Critical" => LogEventLevel.Fatal,
                "High" => LogEventLevel.Error,
                "Medium" => LogEventLevel.Warning,
                "Low" => LogEventLevel.Information,
                _ => LogEventLevel.Warning
            };

            _logger.Write(level,
                "🚨 SECURITY_VIOLATION: UserId: {UserId} | ViolationType: {ViolationType} | Details: {Details} | IP: {IPAddress} | Severity: {Severity}",
                userId, violationType, details, ipAddress, severity);
        }

        /// <summary>
        /// ثبت تغییر مجوز کاربر
        /// </summary>
        /// <param name="changedByUserId">شناسه کاربری که تغییر را اعمال کرده</param>
        /// <param name="targetUserId">شناسه کاربر هدف</param>
        /// <param name="oldRoles">نقش‌های قبلی</param>
        /// <param name="newRoles">نقش‌های جدید</param>
        public static void LogPermissionChange(string changedByUserId, string targetUserId, string oldRoles, string newRoles)
        {
            _logger.Warning(
                "⚠️ SECURITY_PERMISSION_CHANGE: ChangedBy: {ChangedByUserId} | TargetUser: {TargetUserId} | OldRoles: {OldRoles} | NewRoles: {NewRoles}",
                changedByUserId, targetUserId, oldRoles, newRoles);
        }

        /// <summary>
        /// ثبت تغییرات داده‌های حساس
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="entityType">نوع موجودیت</param>
        /// <param name="entityId">شناسه موجودیت</param>
        /// <param name="fieldName">نام فیلد تغییر یافته</param>
        /// <param name="ipAddress">آدرس IP</param>
        public static void LogSensitiveDataChange(string userId, string entityType, string entityId, string fieldName, string ipAddress = null)
        {
            _logger.Warning(
                "🔐 SECURITY_SENSITIVE_DATA_CHANGE: UserId: {UserId} | EntityType: {EntityType} | EntityId: {EntityId} | FieldName: {FieldName} | IP: {IPAddress}",
                userId, entityType, entityId, fieldName, ipAddress);
        }

        /// <summary>
        /// ثبت تلاش دسترسی غیرمجاز
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="resource">منبع مورد تلاش</param>
        /// <param name="action">عمل مورد تلاش</param>
        /// <param name="ipAddress">آدرس IP</param>
        public static void LogUnauthorizedAccess(string userId, string resource, string action, string ipAddress = null)
        {
            _logger.Warning(
                "🚫 SECURITY_UNAUTHORIZED_ACCESS: UserId: {UserId} | Resource: {Resource} | Action: {Action} | IP: {IPAddress}",
                userId, resource, action, ipAddress);
        }

        /// <summary>
        /// ثبت تلاش دسترسی به داده‌های حذف شده (Soft Deleted)
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="entityType">نوع موجودیت</param>
        /// <param name="entityId">شناسه موجودیت</param>
        public static void LogDeletedDataAccess(string userId, string entityType, string entityId)
        {
            _logger.Warning(
                "🗑️ SECURITY_DELETED_DATA_ACCESS: UserId: {UserId} | EntityType: {EntityType} | EntityId: {EntityId}",
                userId, entityType, entityId);
        }

        /// <summary>
        /// ثبت تغییر رمز عبور
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="success">وضعیت موفقیت</param>
        /// <param name="ipAddress">آدرس IP</param>
        public static void LogPasswordChange(string userId, bool success, string ipAddress = null)
        {
            var level = success ? LogEventLevel.Information : LogEventLevel.Warning;
            _logger.Write(level,
                "🔑 SECURITY_PASSWORD_CHANGE: UserId: {UserId} | Success: {Success} | IP: {IPAddress}",
                userId, success, ipAddress);
        }

        /// <summary>
        /// ثبت بازنشانی رمز عبور
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="success">وضعیت موفقیت</param>
        /// <param name="ipAddress">آدرس IP</param>
        public static void LogPasswordReset(string userId, bool success, string ipAddress = null)
        {
            var level = success ? LogEventLevel.Information : LogEventLevel.Warning;
            _logger.Write(level,
                "🔄 SECURITY_PASSWORD_RESET: UserId: {UserId} | Success: {Success} | IP: {IPAddress}",
                userId, success, ipAddress);
        }
    }
}

