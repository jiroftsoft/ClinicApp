using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای نمایش خدمات در فرم گروهی
    /// </summary>
    public class ServiceLookupViewModel
    {
        public int ServiceId { get; set; }
        public string Title { get; set; }
        public string ServiceCode { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public bool HasExistingTariff { get; set; }
    }
}
