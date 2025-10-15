namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش کلینیک در فرم پذیرش
    /// </summary>
    public class ReceptionClinicViewModel
    {
        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int ClinicId { get; set; }

        /// <summary>
        /// نام کلینیک
        /// </summary>
        public string ClinicName { get; set; }

        /// <summary>
        /// آدرس کلینیک
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// شماره تلفن کلینیک
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// آیا کلینیک فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تعداد دپارتمان‌ها
        /// </summary>
        public int DepartmentCount { get; set; }

        /// <summary>
        /// نمایش نام کلینیک
        /// </summary>
        public string DisplayName => ClinicName;

        /// <summary>
        /// نمایش وضعیت فعال
        /// </summary>
        public string StatusDisplay => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// نمایش اطلاعات کلینیک (فرمات شده)
        /// </summary>
        public string ClinicInfoDisplay => $"{ClinicName} - {Address}";

        /// <summary>
        /// نمایش تعداد دپارتمان‌ها
        /// </summary>
        public string DepartmentCountDisplay => $"{DepartmentCount} دپارتمان";
    }
}
