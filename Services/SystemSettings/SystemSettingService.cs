using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.SystemSettings;
using ClinicApp.Services.SystemSettings;
using Serilog;

namespace ClinicApp.Services.SystemSettings
{
    /// <summary>
    /// پیاده‌سازی سرویس مدیریت تنظیمات سیستم
    /// </summary>
    public class SystemSettingService : ISystemSettingService
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
        private readonly object _cacheLock = new object();

        public SystemSettingService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// دریافت تنظیمات بر اساس کلید
        /// </summary>
        public async Task<string> GetSettingValueAsync(string settingKey, int? clinicId = null)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var cacheKey = $"{settingKey}_{clinicId ?? 0}";
                
                _logger.Information("🏥 MEDICAL: دریافت تنظیمات - Key: {SettingKey}, ClinicId: {ClinicId}, CorrelationId: {CorrelationId}", 
                    settingKey, clinicId, correlationId);

                // بررسی کش
                lock (_cacheLock)
                {
                    if (_cache.ContainsKey(cacheKey))
                    {
                        _logger.Information("🏥 MEDICAL: تنظیمات از کش دریافت شد - Key: {SettingKey}, Value: {Value}, CorrelationId: {CorrelationId}", 
                            settingKey, _cache[cacheKey], correlationId);
                        return _cache[cacheKey];
                    }
                }

                // دریافت از دیتابیس (شبیه‌سازی)
                var settingValue = await GetSettingFromDatabaseAsync(settingKey, clinicId);
                
                // ذخیره در کش
                lock (_cacheLock)
                {
                    _cache[cacheKey] = settingValue;
                }

                _logger.Information("🏥 MEDICAL: تنظیمات از دیتابیس دریافت شد - Key: {SettingKey}, Value: {Value}, CorrelationId: {CorrelationId}", 
                    settingKey, settingValue, correlationId);

                return settingValue;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت تنظیمات - Key: {SettingKey}, ClinicId: {ClinicId}", settingKey, clinicId);
                return GetDefaultValue(settingKey);
            }
        }

        /// <summary>
        /// دریافت تنظیمات به صورت تایپ شده
        /// </summary>
        public async Task<T> GetSettingValueAsync<T>(string settingKey, T defaultValue = default, int? clinicId = null)
        {
            try
            {
                var stringValue = await GetSettingValueAsync(settingKey, clinicId);
                
                if (string.IsNullOrEmpty(stringValue))
                    return defaultValue;

                return (T)Convert.ChangeType(stringValue, typeof(T));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در تبدیل تنظیمات - Key: {SettingKey}, Type: {Type}", settingKey, typeof(T).Name);
                return defaultValue;
            }
        }

        /// <summary>
        /// تنظیم مقدار تنظیمات
        /// </summary>
        public async Task<bool> SetSettingValueAsync(string settingKey, string settingValue, string dataType, string category, string description = null, int? clinicId = null)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _logger.Information("🏥 MEDICAL: تنظیم مقدار - Key: {SettingKey}, Value: {Value}, ClinicId: {ClinicId}, CorrelationId: {CorrelationId}", 
                    settingKey, settingValue, clinicId, correlationId);

                // ذخیره در دیتابیس (شبیه‌سازی)
                var success = await SaveSettingToDatabaseAsync(settingKey, settingValue, dataType, category, description, clinicId);
                
                if (success)
                {
                    // بروزرسانی کش
                    var cacheKey = $"{settingKey}_{clinicId ?? 0}";
                    lock (_cacheLock)
                    {
                        _cache[cacheKey] = settingValue;
                    }
                    
                    _logger.Information("🏥 MEDICAL: تنظیمات با موفقیت ذخیره شد - Key: {SettingKey}, CorrelationId: {CorrelationId}", 
                        settingKey, correlationId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در ذخیره تنظیمات - Key: {SettingKey}, ClinicId: {ClinicId}", settingKey, clinicId);
                return false;
            }
        }

        /// <summary>
        /// دریافت تمام تنظیمات یک دسته‌بندی
        /// </summary>
        public async Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category, int? clinicId = null)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _logger.Information("🏥 MEDICAL: دریافت تنظیمات دسته‌بندی - Category: {Category}, ClinicId: {ClinicId}, CorrelationId: {CorrelationId}", 
                    category, clinicId, correlationId);

                // شبیه‌سازی دریافت از دیتابیس
                var settings = await GetSettingsFromDatabaseByCategoryAsync(category, clinicId);
                
                _logger.Information("🏥 MEDICAL: تنظیمات دسته‌بندی دریافت شد - Count: {Count}, CorrelationId: {CorrelationId}", 
                    settings.Count, correlationId);

                return settings;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت تنظیمات دسته‌بندی - Category: {Category}, ClinicId: {ClinicId}", category, clinicId);
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// دریافت سال مالی جاری
        /// </summary>
        public async Task<int> GetCurrentFinancialYearAsync(int? clinicId = null)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _logger.Information("🏥 MEDICAL: دریافت سال مالی جاری - ClinicId: {ClinicId}, CorrelationId: {CorrelationId}", clinicId, correlationId);

                // دریافت از تنظیمات
                var financialYear = await GetSettingValueAsync<int>(SettingKeys.CurrentFinancialYear, 1403, clinicId);
                
                // اگر تنظیمات وجود نداشت، محاسبه بر اساس تاریخ
                if (financialYear == 0)
                {
                    var now = DateTime.Now;
                    financialYear = now.Month >= 3 ? now.Year : now.Year - 1;
                    
                    _logger.Information("🏥 MEDICAL: سال مالی محاسبه شد - Year: {Year}, CorrelationId: {CorrelationId}", 
                        financialYear, correlationId);
                }

                return financialYear;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت سال مالی - ClinicId: {ClinicId}", clinicId);
                return DateTime.Now.Year;
            }
        }

        /// <summary>
        /// دریافت تنظیمات محاسبه بیمه
        /// </summary>
        public async Task<InsuranceCalculationSettings> GetInsuranceCalculationSettingsAsync(int? clinicId = null)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _logger.Information("🏥 MEDICAL: دریافت تنظیمات محاسبه بیمه - ClinicId: {ClinicId}, CorrelationId: {CorrelationId}", clinicId, correlationId);

                var settings = new InsuranceCalculationSettings
                {
                    DefaultCoveragePercent = await GetSettingValueAsync<decimal>(SettingKeys.DefaultCoveragePercent, 80m, clinicId),
                    MaxCoveragePercent = await GetSettingValueAsync<decimal>(SettingKeys.MaxCoveragePercent, 100m, clinicId),
                    MinCoveragePercent = await GetSettingValueAsync<decimal>(SettingKeys.MinCoveragePercent, 0m, clinicId),
                    DefaultTechnicalFactor = await GetSettingValueAsync<decimal>(SettingKeys.DefaultTechnicalFactor, 1.0m, clinicId),
                    DefaultProfessionalFactor = await GetSettingValueAsync<decimal>(SettingKeys.DefaultProfessionalFactor, 1.0m, clinicId),
                    CalculationPrecision = await GetSettingValueAsync<int>(SettingKeys.CalculationPrecision, 2, clinicId)
                };

                _logger.Information("🏥 MEDICAL: تنظیمات محاسبه بیمه دریافت شد - DefaultCoverage: {DefaultCoverage}%, CorrelationId: {CorrelationId}", 
                    settings.DefaultCoveragePercent, correlationId);

                return settings;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت تنظیمات محاسبه بیمه - ClinicId: {ClinicId}", clinicId);
                return new InsuranceCalculationSettings();
            }
        }

        /// <summary>
        /// دریافت تنظیمات سیستم
        /// </summary>
        public async Task<SystemConfigurationSettings> GetSystemConfigurationSettingsAsync(int? clinicId = null)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _logger.Information("🏥 MEDICAL: دریافت تنظیمات سیستم - ClinicId: {ClinicId}, CorrelationId: {CorrelationId}", clinicId, correlationId);

                var settings = new SystemConfigurationSettings
                {
                    SystemName = await GetSettingValueAsync(SettingKeys.SystemName, "سیستم مدیریت کلینیک", clinicId),
                    SystemVersion = await GetSettingValueAsync(SettingKeys.SystemVersion, "1.0.0", clinicId),
                    MaintenanceMode = await GetSettingValueAsync<bool>(SettingKeys.MaintenanceMode, false, clinicId),
                    SessionTimeout = await GetSettingValueAsync<int>(SettingKeys.SessionTimeout, 30, clinicId),
                    MaxLoginAttempts = await GetSettingValueAsync<int>(SettingKeys.MaxLoginAttempts, 5, clinicId),
                    PasswordExpiryDays = await GetSettingValueAsync<int>(SettingKeys.PasswordExpiryDays, 90, clinicId)
                };

                _logger.Information("🏥 MEDICAL: تنظیمات سیستم دریافت شد - SystemName: {SystemName}, Version: {Version}, CorrelationId: {CorrelationId}", 
                    settings.SystemName, settings.SystemVersion, correlationId);

                return settings;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت تنظیمات سیستم - ClinicId: {ClinicId}", clinicId);
                return new SystemConfigurationSettings();
            }
        }

        #region Private Methods

        private async Task<string> GetSettingFromDatabaseAsync(string settingKey, int? clinicId)
        {
            // شبیه‌سازی تأخیر دیتابیس
            await Task.Delay(10);

            // مقادیر پیش‌فرض برای تنظیمات مهم
            var defaultSettings = new Dictionary<string, string>
            {
                { SettingKeys.CurrentFinancialYear, "1403" },
                { SettingKeys.DefaultCoveragePercent, "80" },
                { SettingKeys.MaxCoveragePercent, "100" },
                { SettingKeys.MinCoveragePercent, "0" },
                { SettingKeys.DefaultTechnicalFactor, "1.0" },
                { SettingKeys.DefaultProfessionalFactor, "1.0" },
                { SettingKeys.CalculationPrecision, "2" },
                { SettingKeys.SystemName, "سیستم مدیریت کلینیک" },
                { SettingKeys.SystemVersion, "1.0.0" },
                { SettingKeys.MaintenanceMode, "false" },
                { SettingKeys.SessionTimeout, "30" },
                { SettingKeys.MaxLoginAttempts, "5" },
                { SettingKeys.PasswordExpiryDays, "90" }
            };

            return defaultSettings.ContainsKey(settingKey) ? defaultSettings[settingKey] : GetDefaultValue(settingKey);
        }

        private async Task<bool> SaveSettingToDatabaseAsync(string settingKey, string settingValue, string dataType, string category, string description, int? clinicId)
        {
            // شبیه‌سازی ذخیره در دیتابیس
            await Task.Delay(50);
            return true;
        }

        private async Task<Dictionary<string, string>> GetSettingsFromDatabaseByCategoryAsync(string category, int? clinicId)
        {
            // شبیه‌سازی دریافت تنظیمات بر اساس دسته‌بندی
            await Task.Delay(20);

            var settings = new Dictionary<string, string>();

            switch (category)
            {
                case SettingCategories.Financial:
                    settings[SettingKeys.CurrentFinancialYear] = "1403";
                    break;
                case SettingCategories.Insurance:
                    settings[SettingKeys.DefaultCoveragePercent] = "80";
                    settings[SettingKeys.MaxCoveragePercent] = "100";
                    settings[SettingKeys.MinCoveragePercent] = "0";
                    break;
                case SettingCategories.Calculation:
                    settings[SettingKeys.DefaultTechnicalFactor] = "1.0";
                    settings[SettingKeys.DefaultProfessionalFactor] = "1.0";
                    settings[SettingKeys.CalculationPrecision] = "2";
                    break;
                case SettingCategories.System:
                    settings[SettingKeys.SystemName] = "سیستم مدیریت کلینیک";
                    settings[SettingKeys.SystemVersion] = "1.0.0";
                    settings[SettingKeys.MaintenanceMode] = "false";
                    break;
                case SettingCategories.Security:
                    settings[SettingKeys.SessionTimeout] = "30";
                    settings[SettingKeys.MaxLoginAttempts] = "5";
                    settings[SettingKeys.PasswordExpiryDays] = "90";
                    break;
            }

            return settings;
        }

        private string GetDefaultValue(string settingKey)
        {
            var defaultValues = new Dictionary<string, string>
            {
                { SettingKeys.CurrentFinancialYear, "1403" },
                { SettingKeys.DefaultCoveragePercent, "80" },
                { SettingKeys.MaxCoveragePercent, "100" },
                { SettingKeys.MinCoveragePercent, "0" },
                { SettingKeys.DefaultTechnicalFactor, "1.0" },
                { SettingKeys.DefaultProfessionalFactor, "1.0" },
                { SettingKeys.CalculationPrecision, "2" },
                { SettingKeys.SystemName, "سیستم مدیریت کلینیک" },
                { SettingKeys.SystemVersion, "1.0.0" },
                { SettingKeys.MaintenanceMode, "false" },
                { SettingKeys.SessionTimeout, "30" },
                { SettingKeys.MaxLoginAttempts, "5" },
                { SettingKeys.PasswordExpiryDays, "90" }
            };

            return defaultValues.ContainsKey(settingKey) ? defaultValues[settingKey] : "";
        }

        #endregion
    }
}
