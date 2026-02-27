namespace ClinicApp.Models.DTOs.Insurance
{
    /// <summary>
    /// یک ردیف تحلیل به تفکیک بیمه‌گذار
    /// </summary>
    public class InsuranceProviderBreakdownRow
    {
        public int InsuranceProviderId { get; set; }
        public string ProviderName { get; set; }
        public decimal TotalClaimed { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal DeductionRatePercent { get; set; }
        public double AverageSettlementDays { get; set; }
        public int ClaimCount { get; set; }
    }
}
