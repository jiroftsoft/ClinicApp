using System;

namespace ClinicApp.Models.DTOs.Calculation
{
    /// <summary>
    /// DTO برای نتیجه محاسبه تعرفه بیمه
    /// </summary>
    public class CalculationResultDto
    {
        public decimal TariffPrice { get; set; }
        public decimal PatientShare { get; set; }
        public decimal InsurerShare { get; set; }
        public decimal? SupplementaryCoveragePercent { get; set; }
        public decimal? PrimaryCoveragePercent { get; set; }
        public decimal? TotalCoveragePercent { get; set; }
        public decimal? PatientSharePercent { get; set; }
        public decimal? InsurerSharePercent { get; set; }
        public string CalculationType { get; set; }
        public string CorrelationId { get; set; }
        public DateTime CalculatedAt { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public CalculationServiceDto Service { get; set; }
        public CalculationPlanDto InsurancePlan { get; set; }
    }
}
