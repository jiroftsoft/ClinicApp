namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش بیمه‌گذار در فرم پذیرش
    /// </summary>
    public class ReceptionInsuranceProviderViewModel
    {
        /// <summary>
        /// شناسه بیمه‌گذار
        /// </summary>
        public int InsuranceProviderId { get; set; }

        /// <summary>
        /// نام بیمه‌گذار
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// توضیحات بیمه‌گذار
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// آیا بیمه‌گذار فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// نمایش نام بیمه‌گذار (فرمات شده)
        /// </summary>
        public string DisplayName => $"{Name}";

        /// <summary>
        /// نمایش وضعیت فعال
        /// </summary>
        public string StatusDisplay => IsActive ? "فعال" : "غیرفعال";
    }
}
