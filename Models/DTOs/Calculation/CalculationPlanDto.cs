using System;

namespace ClinicApp.Models.DTOs.Calculation
{
    /// <summary>
    /// DTO برای طرح بیمه در محاسبات تعرفه بیمه
    /// </summary>
    public class CalculationPlanDto
    {
        public int InsurancePlanId { get; set; }
        public decimal CoveragePercent { get; set; }
        public string Name { get; set; }
        public string PlanCode { get; set; }
        public string Description { get; set; }
        public int InsuranceProviderId { get; set; }
        public string ProviderName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
