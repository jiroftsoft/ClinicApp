using System;
using System.Configuration;
using ClinicApp.Interfaces;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// پیاده‌سازی حرفه‌ای تنظیمات سیستم با استفاده از Web.config برای سیستم‌های پزشکی
    /// 
    /// این کلاس با توجه به استانداردهای سیستم‌های پزشکی طراحی شده و:
    /// 
    /// 1. کاملاً سازگار با سیستم پسورد‌لس و OTP
    /// 2. پشتیبانی کامل از محیط‌های وب و غیر-وب
    /// 3. رعایت اصول امنیتی سیستم‌های پزشکی
    /// 4. قابلیت تست‌پذیری بالا
    /// 5. مدیریت خطاها و لاگ‌گیری حرفه‌ای
    /// 6. پشتیبانی از سیستم حذف نرم و ردیابی
    /// 
    /// استفاده:
    /// var settings = new AppSettings();
    /// int pageSize = settings.DefaultPageSize;
    /// 
    /// نکته حیاتی: این کلاس برای سیستم‌های پزشکی طراحی شده و تمام تنظیمات ضروری را پشتیبانی می‌کند
    /// </summary>
    public class AppSettings : IAppSettings
    {
        private static readonly ILogger _log = Log.ForContext<AppSettings>();
        private static AppSettings _instance;
        private static readonly object _lock = new object();

        #region Basic Settings (تنظیمات پایه)

        public int DefaultPageSize { get; set; }
        public int MaxLoginAttempts { get; set; }
        public int RateLimitMinutes { get; set; }
        public int SessionTimeoutMinutes { get; set; }
        public bool EnableAuditLogging { get; set; }

        #endregion

        #region Security Settings (تنظیمات امنیتی)

        public bool RequireTwoFactorAuthentication { get; set; }
        public int PasswordComplexityLevel { get; set; }
        public bool EnableBruteForceProtection { get; set; }
        public int AccountLockoutDurationMinutes { get; set; }
        public bool EnablePasswordHistory { get; set; }
        public int PasswordHistoryCount { get; set; }
        public int PasswordExpirationDays { get; set; }

        #endregion

        #region Notification Settings (تنظیمات اطلاع‌رسانی)

        public string SmsProvider { get; set; }
        public bool EnableEmailNotifications { get; set; }
        public bool EnableSmsNotifications { get; set; }
        public int AppointmentReminderHours { get; set; }
        public int MaxNotificationRetries { get; set; }
        public int NotificationRetryDelaySeconds { get; set; }

        #endregion

        #region Medical System Settings (تنظیمات سیستم‌های پزشکی)

        public int MaxAppointmentDurationMinutes { get; set; }
        public int MinAppointmentIntervalMinutes { get; set; }
        public int DefaultAppointmentDurationMinutes { get; set; }
        public bool EnablePatientPortal { get; set; }
        public bool EnableElectronicPrescriptions { get; set; }
        public bool EnableMedicalRecordSharing { get; set; }
        public bool EnableInsuranceValidation { get; set; }
        public int MaxPatientAge { get; set; }
        public int MinPatientAge { get; set; }
        public int MaxRegisterAttempts { get; set; }

        #endregion

        #region Constructor & Initialization (سازنده و مقداردهی اولیه)

        private AppSettings()
        {
            LoadBasicSettings();
            LoadSecuritySettings();
            LoadNotificationSettings();
            LoadMedicalSystemSettings();

            _log.Information("تنظیمات سیستم با موفقیت بارگذاری شدند");
        }

        /// <summary>
        /// دریافت نمونه تنظیمات به صورت Thread-Safe
        /// </summary>
        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AppSettings();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Settings Loaders (بارگیری تنظیمات)

        private void LoadBasicSettings()
        {
            // DefaultPageSize
            DefaultPageSize = GetIntSetting("AppSettings:DefaultPageSize",
                SystemConstants.DefaultPageSize,
                "اندازه پیش‌فرض صفحه‌بندی",
                5, 100);

            // MaxLoginAttempts
            MaxLoginAttempts = GetIntSetting("AppSettings:MaxLoginAttempts",
                SystemConstants.MaxLoginAttempts,
                "حداکثر تلاش‌های ورود",
                3, 10);

            // RateLimitMinutes
            RateLimitMinutes = GetIntSetting("AppSettings:RateLimitMinutes",
                SystemConstants.RateLimitMinutes,
                "زمان محدودیت نرخ درخواست‌ها",
                1, 60);

            // SessionTimeoutMinutes
            SessionTimeoutMinutes = GetIntSetting("AppSettings:SessionTimeoutMinutes",
                30,
                "زمان انقضای جلسه",
                5, 120);

            // EnableAuditLogging
            EnableAuditLogging = GetBoolSetting("AppSettings:EnableAuditLogging",
                true,
                "فعال‌سازی لاگ‌گیری ردیابی");
        }

        private void LoadSecuritySettings()
        {
            // RequireTwoFactorAuthentication
            RequireTwoFactorAuthentication = GetBoolSetting("Security:RequireTwoFactorAuthentication",
                true,
                "نیاز به احراز هویت دو مرحله‌ای");

            // PasswordComplexityLevel
            PasswordComplexityLevel = GetIntSetting("Security:PasswordComplexityLevel",
                3,
                "سطح پیچیدگی رمز عبور",
                1, 4);

            // EnableBruteForceProtection
            EnableBruteForceProtection = GetBoolSetting("Security:EnableBruteForceProtection",
                true,
                "فعال‌سازی محافظت در برابر حمله Brute Force");

            // AccountLockoutDurationMinutes
            AccountLockoutDurationMinutes = GetIntSetting("Security:AccountLockoutDurationMinutes",
                30,
                "مدت زمان قفل حساب کاربری",
                5, 120);

            // EnablePasswordHistory
            EnablePasswordHistory = GetBoolSetting("Security:EnablePasswordHistory",
                true,
                "فعال‌سازی تاریخچه رمز عبور");

            // PasswordHistoryCount
            PasswordHistoryCount = GetIntSetting("Security:PasswordHistoryCount",
                5,
                "تعداد رمزهای عبور ذخیره شده در تاریخچه",
                1, 10);

            // PasswordExpirationDays
            PasswordExpirationDays = GetIntSetting("Security:PasswordExpirationDays",
                90,
                "مدت زمان انقضای رمز عبور",
                30, 365);
        }

        private void LoadNotificationSettings()
        {
            // SmsProvider
            SmsProvider = GetStringSetting("Notifications:SmsProvider",
                "Asanak",
                "ارائه‌دهنده سرویس پیامک");

            // EnableEmailNotifications
            EnableEmailNotifications = GetBoolSetting("Notifications:EnableEmailNotifications",
                false,
                "فعال‌سازی اطلاع‌رسانی ایمیلی");

            // EnableSmsNotifications
            EnableSmsNotifications = GetBoolSetting("Notifications:EnableSmsNotifications",
                true,
                "فعال‌سازی اطلاع‌رسانی پیامکی");

            // AppointmentReminderHours
            AppointmentReminderHours = GetIntSetting("Notifications:AppointmentReminderHours",
                24,
                "ساعت‌های یادآوری نوبت",
                1, 72);

            // MaxNotificationRetries
            MaxNotificationRetries = GetIntSetting("Notifications:MaxNotificationRetries",
                3,
                "حداکثر تلاش‌های ارسال اطلاع‌رسانی",
                1, 10);

            // NotificationRetryDelaySeconds
            NotificationRetryDelaySeconds = GetIntSetting("Notifications:NotificationRetryDelaySeconds",
                30,
                "تاخیر بین تلاش‌های ارسال اطلاع‌رسانی",
                5, 300);
        }

        private void LoadMedicalSystemSettings()
        {
            // MaxAppointmentDurationMinutes
            MaxAppointmentDurationMinutes = GetIntSetting("MedicalSystem:MaxAppointmentDurationMinutes",
                180,
                "حداکثر مدت زمان نوبت",
                30, 360);

            // MinAppointmentIntervalMinutes
            MinAppointmentIntervalMinutes = GetIntSetting("MedicalSystem:MinAppointmentIntervalMinutes",
                15,
                "حداقل فاصله بین نوبت‌ها",
                5, 60);

            // DefaultAppointmentDurationMinutes
            DefaultAppointmentDurationMinutes = GetIntSetting("MedicalSystem:DefaultAppointmentDurationMinutes",
                30,
                "مدت زمان پیش‌فرض نوبت",
                5, 120);

            // EnablePatientPortal
            EnablePatientPortal = GetBoolSetting("MedicalSystem:EnablePatientPortal",
                true,
                "فعال‌سازی پورتال بیمار");

            // EnableElectronicPrescriptions
            EnableElectronicPrescriptions = GetBoolSetting("MedicalSystem:EnableElectronicPrescriptions",
                true,
                "فعال‌سازی نسخه‌های الکترونیکی");

            // EnableMedicalRecordSharing
            EnableMedicalRecordSharing = GetBoolSetting("MedicalSystem:EnableMedicalRecordSharing",
                true,
                "فعال‌سازی اشتراک‌گذاری پرونده‌های پزشکی");

            // EnableInsuranceValidation
            EnableInsuranceValidation = GetBoolSetting("MedicalSystem:EnableInsuranceValidation",
                true,
                "فعال‌سازی اعتبارسنجی بیمه");

            // MaxPatientAge
            MaxPatientAge = GetIntSetting("MedicalSystem:MaxPatientAge",
                120,
                "حداکثر سن بیمار",
                18, 150);

            // MinPatientAge
            MinPatientAge = GetIntSetting("MedicalSystem:MinPatientAge",
                0,
                "حداقل سن بیمار",
                0, 18);
        }

        #endregion

        #region Helper Methods (روش‌های کمکی)

        /// <summary>
        /// دریافت تنظیمات عددی با اعتبارسنجی محدوده
        /// </summary>
        private int GetIntSetting(string key, int defaultValue, string settingName, int minValue, int maxValue)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    _log.Warning("تنظیم '{SettingName}' در Web.config یافت نشد. مقدار پیش‌فرض ({DefaultValue}) استفاده شد.",
                        settingName, defaultValue);
                    return defaultValue;
                }

                if (int.TryParse(value, out int result))
                {
                    if (result >= minValue && result <= maxValue)
                    {
                        _log.Information("تنظیم '{SettingName}' به مقدار {Value} بارگذاری شد.", settingName, result);
                        return result;
                    }
                    else
                    {
                        _log.Warning("مقدار تنظیم '{SettingName}' ({Value}) خارج از محدوده مجاز ({Min}-{Max}) است. مقدار پیش‌فرض ({DefaultValue}) استفاده شد.",
                            settingName, result, minValue, maxValue, defaultValue);
                        return defaultValue;
                    }
                }
                else
                {
                    _log.Error("مقدار تنظیم '{SettingName}' ({Value}) به عدد تبدیل نمی‌شود. مقدار پیش‌فرض ({DefaultValue}) استفاده شد.",
                        settingName, value, defaultValue);
                    return defaultValue;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بارگذاری تنظیم '{SettingName}'. مقدار پیش‌فرض ({DefaultValue}) استفاده شد.",
                    settingName, defaultValue);
                return defaultValue;
            }
        }

        /// <summary>
        /// دریافت تنظیمات بولین
        /// </summary>
        private bool GetBoolSetting(string key, bool defaultValue, string settingName)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    _log.Warning("تنظیم '{SettingName}' در Web.config یافت نشد. مقدار پیش‌فرض ({DefaultValue}) استفاده شد.",
                        settingName, defaultValue);
                    return defaultValue;
                }

                if (bool.TryParse(value, out bool result))
                {
                    _log.Information("تنظیم '{SettingName}' به مقدار {Value} بارگذاری شد.", settingName, result);
                    return result;
                }
                else
                {
                    _log.Warning("مقدار تنظیم '{SettingName}' ({Value}) به بولین تبدیل نمی‌شود. مقدار پیش‌فرض ({DefaultValue}) استفاده شد.",
                        settingName, value, defaultValue);
                    return defaultValue;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بارگذاری تنظیم '{SettingName}'. مقدار پیش‌فرض ({DefaultValue}) استفاده شد.",
                    settingName, defaultValue);
                return defaultValue;
            }
        }

        /// <summary>
        /// دریافت تنظیمات رشته‌ای
        /// </summary>
        private string GetStringSetting(string key, string defaultValue, string settingName)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    _log.Warning("تنظیم '{SettingName}' در Web.config یافت نشد. مقدار پیش‌فرض ({DefaultValue}) استفاده شد.",
                        settingName, defaultValue);
                    return defaultValue;
                }

                _log.Information("تنظیم '{SettingName}' به مقدار {Value} بارگذاری شد.", settingName, value);
                return value;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بارگذاری تنظیم '{SettingName}'. مقدار پیش‌فرض ({DefaultValue}) استفاده شد.",
                    settingName, defaultValue);
                return defaultValue;
            }
        }

        /// <summary>
        /// به‌روزرسانی تنظیمات پویا (برای استفاده در محیط‌های پس‌زمینه)
        /// </summary>
        public void RefreshSettings()
        {
            _log.Information("در حال به‌روزرسانی تنظیمات...");
            LoadBasicSettings();
            LoadSecuritySettings();
            LoadNotificationSettings();
            LoadMedicalSystemSettings();
            _log.Information("تنظیمات با موفقیت به‌روزرسانی شدند");
        }

        #endregion
    }

    /// <summary>
    /// ثابت‌های سیستمی که برای سیستم‌های پزشکی تعریف شده‌اند
    /// </summary>
    public static class SystemConstants
    {
        
        public const string FreeInsuranceName = "بیمه آزاد";
        public const string PatientRole = "Patient";
        public const int DefaultPageSize = 50; // ✅ بهینه‌سازی برای 7000 بیمار
        public const int MaxLoginAttempts = 5;
        public const int RateLimitMinutes = 1;
        public const int MaxFailedAccessAttemptsBeforeLockout = 5;
        public const int DefaultAccountLockoutTimeSpanMinutes = 30;
    }

  
}