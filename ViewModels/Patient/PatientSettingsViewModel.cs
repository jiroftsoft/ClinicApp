namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای تنظیمات بیمار
    /// طبق: DEVELOPMENT_CONTRACT.md - Strongly-Typed
    /// </summary>
    public class PatientSettingsViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        
        // Notification Settings
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool AppointmentReminders { get; set; }
        
        // Privacy Settings
        public bool ShareMedicalInfo { get; set; }
        public bool ShowNameInReviews { get; set; }
    }
}

