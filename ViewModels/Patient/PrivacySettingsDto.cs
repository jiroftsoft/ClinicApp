namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// DTO برای به‌روزرسانی تنظیمات حریم خصوصی
    /// طبق: DEVELOPMENT_CONTRACT.md
    /// </summary>
    public class PrivacySettingsDto
    {
        public bool ShareMedicalInfo { get; set; }
        public bool ShowNameInReviews { get; set; }
    }
}

