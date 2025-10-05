using System;

namespace ClinicApp.Services.Interfaces
{
    /// <summary>
    /// جزئیات محاسبه قیمت خدمت
    /// </summary>
    public class ServiceCalculationDetails
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal TechnicalAmount { get; set; }
        public decimal ProfessionalAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TechnicalFactor { get; set; }
        public decimal ProfessionalFactor { get; set; }
        public DateTime CalculationDate { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? FinancialYear { get; set; }
        public string CalculationMethod { get; set; }
        public string Notes { get; set; }
    }
}
