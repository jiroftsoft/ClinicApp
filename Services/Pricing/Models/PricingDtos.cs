using System;
using System.Collections.Generic;

namespace ClinicApp.Services.Pricing.Models
{
    /// <summary>
    /// ورودی اصلی برای پیش‌محاسبه/Quote
    /// </summary>
    public class QuoteRequestDto
    {
        public int ClinicId { get; set; }
        public int DepartmentId { get; set; }
        public int DoctorId { get; set; }
        public int ServiceId { get; set; }
        public int? FinancialYearId { get; set; } // اگر null باشد از سرویس مالی سال فعال گرفته می‌شود
        public PartyInsuranceDto Primary { get; set; }
        public PartyInsuranceDto Supplementary { get; set; }
    }

    /// <summary>
    /// DTO برای بیمه پایه یا تکمیلی
    /// </summary>
    public class PartyInsuranceDto
    {
        public int? InsurancePlanId { get; set; }
    }

    /// <summary>
    /// خروجی کامل شکسته‌شده
    /// </summary>
    public class QuoteResultDto
    {
        public int ServiceId { get; set; }
        public long ApprovedTariff { get; set; } // تعرفه مصوب به ریال
        public CoverageBreakdownDto Primary { get; set; }
        public CoverageBreakdownDto Supplementary { get; set; }
        public long PatientInitialCoinsurance { get; set; } // قبل از تکمیلی
        public long PatientFinal { get; set; }              // پس از تکمیلی
        public string RoundingPolicy { get; set; } = "AwayFromZero";
        public List<string> Notes { get; set; } = new List<string>();
        
        // Friendly strings for UI
        public string ApprovedTariffStr { get; set; }
        public string PatientInitialCoinsuranceStr { get; set; }
        public string PatientFinalStr { get; set; }
    }

    /// <summary>
    /// DTO برای جزئیات پوشش بیمه (پایه یا تکمیلی)
    /// </summary>
    public class CoverageBreakdownDto
    {
        public int? PlanId { get; set; }
        public bool IsCovered { get; set; }
        public decimal CoveragePercent { get; set; } // 0..100
        public bool CapApplied { get; set; }
        public long? CapValue { get; set; }          // سقف بر حسب ریال (در صورت وجود)
        public long Pays { get; set; }               // سهم پرداختی این طرف
        public string CoverageRuleName { get; set; } // مثلا "ویزیت سرپایی گروه ۳"
        
        // Friendly strings for UI
        public string CoveragePercentStr { get; set; }
        public string CapValueStr { get; set; }
        public string PaysStr { get; set; }
    }

    /// <summary>
    /// قاعده‌ی پوشش از Provider
    /// </summary>
    public class CoverageRule
    {
        public bool IsCovered { get; set; }
        public decimal CoveragePercent { get; set; } // 0..100
        public long? PerVisitCapIRR { get; set; }    // سقف هر ویزیت
        public string RuleName { get; set; }

        public static CoverageRule None() => new CoverageRule
        {
            IsCovered = false,
            CoveragePercent = 0m,
            PerVisitCapIRR = null,
            RuleName = "No Coverage"
        };
    }
}
