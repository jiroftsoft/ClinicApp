namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// آیتم اعلان ویزیت آنلاین برای زنگوله بیمار (ورود به اتاق).
    /// </summary>
    public class OnlineConsultationNotificationItemViewModel
    {
        public int AppointmentId { get; set; }
        public string DoctorName { get; set; }
        public string DateShamsi { get; set; }
        public string TimeText { get; set; }
        /// <summary>
        /// آدرس نسبی ورود به اتاق، مثلاً /Patient/Consultation/Join/74
        /// </summary>
        public string JoinUrl { get; set; }
    }
}
