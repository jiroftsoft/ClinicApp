using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش اولویت پذیرش
    /// </summary>
    public class ReceptionPriorityViewModel
    {
        /// <summary>
        /// مقدار اولویت
        /// </summary>
        public AppointmentPriority Value { get; set; }

        /// <summary>
        /// متن نمایشی
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// نمایش نام
        /// </summary>
        public string DisplayName => Text;
    }
}
