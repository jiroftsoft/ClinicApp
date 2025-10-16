using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    public class ServiceCalculationRequest
    {
        public List<int> ServiceIds { get; set; } = new List<int>();
        public int PatientId { get; set; }
        public int? InsuranceId { get; set; }
        public int? SpecializationId { get; set; }
        public int? DoctorId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string AdditionalNotes { get; set; }
        
        // Additional properties for compatibility
        [Required(ErrorMessage = "لیست خدمات الزامی است.")]
        public List<ServiceCalculationItem> Services { get; set; } = new List<ServiceCalculationItem>();

        public int? InsurancePlanId { get; set; }
        public bool ApplyDiscounts { get; set; } = true;
        
        // Properties for individual service calculation
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public string ServiceName { get; set; }
        public decimal BasePrice { get; set; }
    }

    /// <summary>
    /// ViewModel برای آیتم محاسبه هزینه خدمت
    /// </summary>
    public class ServiceCalculationItem
    {
        [Required(ErrorMessage = "شناسه خدمت الزامی است.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "تعداد الزامی است.")]
        [Range(1, int.MaxValue, ErrorMessage = "تعداد باید مثبت باشد.")]
        public int Quantity { get; set; } = 1;

        public decimal? CustomPrice { get; set; }
        public bool ApplyInsurance { get; set; } = true;
    }
}
