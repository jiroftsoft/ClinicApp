namespace ClinicApp.Models.DTOs.Insurance
{
    /// <summary>
    /// یک ردیف گزارش Aging مطالبات بیمه
    /// </summary>
    public class InsuranceClaimAgingRow
    {
        public string AgeGroup { get; set; }
        public decimal TotalClaimed { get; set; }
        public decimal TotalApproved { get; set; }
        public int ClaimCount { get; set; }
    }
}
