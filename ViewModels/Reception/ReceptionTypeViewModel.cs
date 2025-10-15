using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش نوع پذیرش
    /// </summary>
    public class ReceptionTypeViewModel
    {
        /// <summary>
        /// مقدار نوع پذیرش
        /// </summary>
        public ReceptionType Value { get; set; }

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
