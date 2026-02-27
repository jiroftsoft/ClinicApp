using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Admin
{
    /// <summary>
    /// فیلتر داشبورد درآمد بیمه‌ها
    /// </summary>
    public class InsuranceRevenueFilterViewModel
    {
        [Display(Name = "از تاریخ")]
        public string StartDatePersian { get; set; }

        [Display(Name = "تا تاریخ")]
        public string EndDatePersian { get; set; }

        [Display(Name = "بیمه‌گذار")]
        public int? InsuranceProviderId { get; set; }

        [Display(Name = "وضعیت مطالبه")]
        public string ClaimStatus { get; set; }

        public System.DateTime? StartDate { get; set; }
        public System.DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// KPIهای داشبورد درآمد بیمه
    /// </summary>
    public class InsuranceKPIViewModel
    {
        public decimal TotalClaims { get; set; }
        public decimal TotalRealized { get; set; }
        public decimal Outstanding { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal DeductionRatePercent { get; set; }
        public double AverageSettlementDays { get; set; }
        public int ClaimCount { get; set; }
    }

    /// <summary>
    /// یک ردیف گزارش Aging
    /// </summary>
    public class InsuranceAgingItemViewModel
    {
        public string AgeGroup { get; set; }
        public decimal TotalClaimed { get; set; }
        public decimal TotalApproved { get; set; }
        public int ClaimCount { get; set; }
    }

    /// <summary>
    /// تحلیل به تفکیک بیمه‌گذار
    /// </summary>
    public class InsuranceProviderBreakdownViewModel
    {
        public int InsuranceProviderId { get; set; }
        public string ProviderName { get; set; }
        public decimal TotalClaimed { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Outstanding { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal DeductionRatePercent { get; set; }
        public double AverageSettlementDays { get; set; }
        public int ClaimCount { get; set; }
    }

    /// <summary>
    /// داده نمودار (برچسب‌ها و مجموعه‌ها برای Chart.js)
    /// </summary>
    public class InsuranceChartDataViewModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> ClaimedValues { get; set; } = new List<decimal>();
        public List<decimal> ApprovedValues { get; set; } = new List<decimal>();
        public List<decimal> DeductionValues { get; set; } = new List<decimal>();
        public List<decimal> PieValues { get; set; } = new List<decimal>();
        public List<string> PieLabels { get; set; } = new List<string>();
    }

    /// <summary>
    /// مدل کامل داشبورد درآمد بیمه (برای Index)
    /// </summary>
    public class InsuranceRevenueDashboardViewModel
    {
        public InsuranceRevenueFilterViewModel Filter { get; set; } = new InsuranceRevenueFilterViewModel();
        public InsuranceKPIViewModel KPIs { get; set; } = new InsuranceKPIViewModel();
        public List<InsuranceAgingItemViewModel> AgingItems { get; set; } = new List<InsuranceAgingItemViewModel>();
        public List<InsuranceProviderBreakdownViewModel> ProviderBreakdown { get; set; } = new List<InsuranceProviderBreakdownViewModel>();
        public InsuranceChartDataViewModel ChartData { get; set; } = new InsuranceChartDataViewModel();
    }
}
