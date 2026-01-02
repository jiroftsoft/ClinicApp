namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// DTO برای به‌روزرسانی تنظیمات اعلان‌ها
    /// طبق: DEVELOPMENT_CONTRACT.md
    /// </summary>
    public class NotificationSettingsDto
    {
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool AppointmentReminders { get; set; }
    }
}

