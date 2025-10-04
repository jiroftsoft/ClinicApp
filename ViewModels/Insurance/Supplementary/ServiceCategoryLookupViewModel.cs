using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای نمایش دسته‌بندی خدمات در فرم گروهی
    /// </summary>
    public class ServiceCategoryLookupViewModel
    {
        public int ServiceCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public int ServiceCount { get; set; }
        
        /// <summary>
        /// Value property for dropdown binding
        /// </summary>
        public string Value => ServiceCategoryId.ToString();
        
        /// <summary>
        /// Text property for dropdown display
        /// </summary>
        public string Text => Name;
    }
}
