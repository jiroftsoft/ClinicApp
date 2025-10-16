using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش الزامات خدمات
    /// </summary>
    public class ServiceRequirementViewModel
    {
        /// <summary>
        /// شناسه الزام
        /// </summary>
        public int RequirementId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// نوع الزام
        /// </summary>
        public string RequirementType { get; set; }

        /// <summary>
        /// عنوان الزام
        /// </summary>
        public string RequirementTitle { get; set; }

        /// <summary>
        /// توضیحات الزام
        /// </summary>
        public string RequirementDescription { get; set; }

        /// <summary>
        /// آیا الزامی است
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// آیا فعال است
        /// </summary>
        public bool IsActive { get; set; }
    }
}
