using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای نمایش دپارتمان‌ها در فرم گروهی
    /// </summary>
    public class DepartmentLookupViewModel
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int ServiceCount { get; set; }
        
        /// <summary>
        /// Value property for dropdown binding
        /// </summary>
        public string Value => DepartmentId.ToString();
        
        /// <summary>
        /// Text property for dropdown display
        /// </summary>
        public string Text => Name;
    }
}
