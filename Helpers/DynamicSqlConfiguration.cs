using System;
using System.Collections.Generic;
using System.Configuration;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Configuration برای Dynamic SQL Helper
    /// این کلاس تنظیمات مربوط به تولید Dynamic SQL را مدیریت می‌کند
    /// </summary>
    public static class DynamicSqlConfiguration
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(DynamicSqlConfiguration));

        /// <summary>
        /// آیا Dynamic SQL فعال است؟
        /// </summary>
        public static bool IsDynamicSqlEnabled => GetBooleanConfig("DynamicSql:Enabled", true);

        /// <summary>
        /// آیا Logging برای SQL Query ها فعال است؟
        /// </summary>
        public static bool IsSqlLoggingEnabled => GetBooleanConfig("DynamicSql:Logging:Enabled", true);

        /// <summary>
        /// سطح Logging برای SQL Query ها
        /// </summary>
        public static string SqlLogLevel => GetStringConfig("DynamicSql:Logging:Level", "Debug");

        /// <summary>
        /// آیا Validation برای SQL Query ها فعال است؟
        /// </summary>
        public static bool IsValidationEnabled => GetBooleanConfig("DynamicSql:Validation:Enabled", true);

        /// <summary>
        /// آیا Caching برای SQL Query ها فعال است؟
        /// </summary>
        public static bool IsCachingEnabled => GetBooleanConfig("DynamicSql:Caching:Enabled", false);

        /// <summary>
        /// مدت زمان Cache (به دقیقه)
        /// </summary>
        public static int CacheExpirationMinutes => GetIntConfig("DynamicSql:Caching:ExpirationMinutes", 30);

        /// <summary>
        /// دریافت Boolean Configuration
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="defaultValue">مقدار پیش‌فرض</param>
        /// <returns>مقدار Boolean</returns>
        private static bool GetBooleanConfig(string key, bool defaultValue)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return defaultValue;
                }

                return bool.Parse(value);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "⚠️ CONFIG: خطا در خواندن تنظیمات {Key}، استفاده از مقدار پیش‌فرض {DefaultValue}", 
                    key, defaultValue);
                return defaultValue;
            }
        }

        /// <summary>
        /// دریافت String Configuration
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="defaultValue">مقدار پیش‌فرض</param>
        /// <returns>مقدار String</returns>
        private static string GetStringConfig(string key, string defaultValue)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "⚠️ CONFIG: خطا در خواندن تنظیمات {Key}، استفاده از مقدار پیش‌فرض {DefaultValue}", 
                    key, defaultValue);
                return defaultValue;
            }
        }

        /// <summary>
        /// دریافت Int Configuration
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="defaultValue">مقدار پیش‌فرض</param>
        /// <returns>مقدار Int</returns>
        private static int GetIntConfig(string key, int defaultValue)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return defaultValue;
                }

                return int.Parse(value);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "⚠️ CONFIG: خطا در خواندن تنظیمات {Key}، استفاده از مقدار پیش‌فرض {DefaultValue}", 
                    key, defaultValue);
                return defaultValue;
            }
        }

        /// <summary>
        /// دریافت تنظیمات کامل
        /// </summary>
        /// <returns>Dictionary شامل تمام تنظیمات</returns>
        public static Dictionary<string, object> GetAllSettings()
        {
            return new Dictionary<string, object>
            {
                { "IsDynamicSqlEnabled", IsDynamicSqlEnabled },
                { "IsSqlLoggingEnabled", IsSqlLoggingEnabled },
                { "SqlLogLevel", SqlLogLevel },
                { "IsValidationEnabled", IsValidationEnabled },
                { "IsCachingEnabled", IsCachingEnabled },
                { "CacheExpirationMinutes", CacheExpirationMinutes }
            };
        }

        /// <summary>
        /// Log کردن تنظیمات فعلی
        /// </summary>
        public static void LogCurrentSettings()
        {
            _logger.Information("🔧 DYNAMIC_SQL_CONFIG: تنظیمات فعلی:");
            _logger.Information("   - Dynamic SQL فعال: {IsEnabled}", IsDynamicSqlEnabled);
            _logger.Information("   - Logging فعال: {IsLoggingEnabled}", IsSqlLoggingEnabled);
            _logger.Information("   - سطح Logging: {LogLevel}", SqlLogLevel);
            _logger.Information("   - Validation فعال: {IsValidationEnabled}", IsValidationEnabled);
            _logger.Information("   - Caching فعال: {IsCachingEnabled}", IsCachingEnabled);
            _logger.Information("   - مدت Cache: {CacheMinutes} دقیقه", CacheExpirationMinutes);
        }
    }
}
