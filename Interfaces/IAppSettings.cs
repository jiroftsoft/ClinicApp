namespace ClinicApp.Interfaces;

/// <summary>
/// رابط برای دسترسی به تنظیمات سیستم
/// این رابط برای افزایش قابلیت تست‌پذیری و جداسازی وابستگی‌ها طراحی شده است
/// </summary>
public interface IAppSettings
{
    #region Basic Settings
    int DefaultPageSize { get; }
    int MaxLoginAttempts { get; }
    int RateLimitMinutes { get; }
    int SessionTimeoutMinutes { get; }
    bool EnableAuditLogging { get; }
    #endregion

    #region Security Settings
    bool RequireTwoFactorAuthentication { get; }
    int PasswordComplexityLevel { get; }
    bool EnableBruteForceProtection { get; }
    int AccountLockoutDurationMinutes { get; }
    bool EnablePasswordHistory { get; }
    int PasswordHistoryCount { get; }
    int PasswordExpirationDays { get; }
    #endregion

    #region Notification Settings
    string SmsProvider { get; }
    bool EnableEmailNotifications { get; }
    bool EnableSmsNotifications { get; }
    int AppointmentReminderHours { get; }
    int MaxNotificationRetries { get; }
    int NotificationRetryDelaySeconds { get; }
    #endregion

    #region Medical System Settings
    int MaxAppointmentDurationMinutes { get; }
    int MinAppointmentIntervalMinutes { get; }
    int DefaultAppointmentDurationMinutes { get; }
    bool EnablePatientPortal { get; }
    bool EnableElectronicPrescriptions { get; }
    bool EnableMedicalRecordSharing { get; }
    bool EnableInsuranceValidation { get; }
    int MaxPatientAge { get; }
    int MinPatientAge { get; }
    int MaxRegisterAttempts { get; set; }

    #endregion
}