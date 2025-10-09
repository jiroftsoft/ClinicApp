using System;

namespace ClinicApp.Models.DTOs.Insurance
{
    /// <summary>
    /// نتیجه انتخاب بیمه
    /// </summary>
    public class PatientInsuranceSelectionResult
    {
        public int PatientId { get; set; }
        public int InsuranceId { get; set; }
        public string InsuranceType { get; set; }
        public string InsuranceName { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// وضعیت بیمه بیمار - بهینه‌سازی شده برای محیط Production
    /// </summary>
    public class PatientInsuranceStatus
    {
        public int PatientId { get; set; }
        public bool HasPrimaryInsurance { get; set; }
        public bool HasSupplementaryInsurance { get; set; }
        public bool PrimaryInsuranceActive { get; set; }
        public DateTime ValidationDate { get; set; }
        public InsuranceInfo PrimaryInsurance { get; set; }
        public InsuranceInfo SupplementaryInsurance { get; set; }
        
        // 🏥 Medical Environment: فیلدهای اضافی برای Production
        public string ValidationStatus { get; set; }
        public string ValidationMessage { get; set; }
        public bool ExpiryWarning { get; set; }
        public CoverageAnalysis CoverageAnalysis { get; set; }
    }

    /// <summary>
    /// اطلاعات بیمه
    /// </summary>
    public class InsuranceInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// تحلیل پوشش بیمه برای محیط Production
    /// </summary>
    public class CoverageAnalysis
    {
        public decimal PrimaryCoveragePercent { get; set; }
        public decimal PrimaryDeductible { get; set; }
        public decimal SupplementaryCoveragePercent { get; set; }
        public decimal SupplementaryDeductible { get; set; }
        public decimal TotalCoveragePercent { get; set; }
    }
}
