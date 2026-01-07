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

    #region Appointment Settings
    /// <summary>
    /// مدت زمان انقضای نوبت‌های Pending (به دقیقه)
    /// بعد از این مدت، نوبت‌های Pending منقضی می‌شوند و اسلات آزاد می‌شود
    /// </summary>
    int PendingExpirationMinutes { get; }
    
    /// <summary>
    /// حداکثر تعداد تاریخ‌های نوبت موجود برای نمایش در کارت پزشک
    /// </summary>
    int AppointmentAvailableDatesMaxCount { get; }
    
    /// <summary>
    /// تعداد روزهای آینده برای بررسی نوبت‌های موجود
    /// </summary>
    int AppointmentAvailableDatesDaysToCheck { get; }
    
    /// <summary>
    /// اندازه پیش‌فرض صفحه‌بندی برای لیست پزشکان
    /// </summary>
    int AppointmentDoctorsPageSize { get; }

    #endregion

    #region Application Information Settings
    /// <summary>
    /// نسخه برنامه
    /// </summary>
    string ApplicationVersion { get; }
    
    /// <summary>
    /// محیط اجرای برنامه (Development, Staging, Production)
    /// </summary>
    string Environment { get; }
    #endregion

    #region Payment Settings
    /// <summary>
    /// Base URL برای ساخت CallbackUrl در درگاه‌های پرداخت
    /// مثال: https://yourdomain.com (بدون trailing slash)
    /// اگر تنظیم نشده باشد، از Request.Url استفاده می‌شود (Fallback)
    /// </summary>
    string PaymentBaseUrl { get; }
    #endregion
}