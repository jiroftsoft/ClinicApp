namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش دپارتمان در فرم پذیرش
    /// </summary>
    public class ReceptionDepartmentViewModel
    {
        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// توضیحات دپارتمان
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int ClinicId { get; set; }

        /// <summary>
        /// آیا دپارتمان فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تعداد پزشکان
        /// </summary>
        public int DoctorCount { get; set; }

        /// <summary>
        /// تعداد دسته‌بندی خدمات
        /// </summary>
        public int ServiceCategoryCount { get; set; }

        /// <summary>
        /// نمایش نام دپارتمان
        /// </summary>
        public string DisplayName => DepartmentName;

        /// <summary>
        /// نمایش وضعیت فعال
        /// </summary>
        public string StatusDisplay => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// نمایش اطلاعات دپارتمان (فرمات شده)
        /// </summary>
        public string DepartmentInfoDisplay => $"{DepartmentName} - {Description}";

        /// <summary>
        /// نمایش تعداد پزشکان
        /// </summary>
        public string DoctorCountDisplay => $"{DoctorCount} پزشک";

        /// <summary>
        /// نمایش تعداد دسته‌بندی خدمات
        /// </summary>
        public string ServiceCategoryCountDisplay => $"{ServiceCategoryCount} دسته‌بندی خدمت";
    }
}
