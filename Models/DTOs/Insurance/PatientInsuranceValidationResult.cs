using System;
using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Insurance
{
    /// <summary>
    /// نتیجه اعتبارسنجی بیمه بیمار
    /// </summary>
    public class PatientInsuranceValidationResult
    {
        public int PatientId { get; set; }
        public bool IsValid { get; set; }
        public DateTime ValidationDate { get; set; }
        public List<InsuranceValidationIssue> Issues { get; set; } = new List<InsuranceValidationIssue>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public InsuranceValidationStatus PrimaryInsuranceStatus { get; set; }
        public InsuranceValidationStatus SupplementaryInsuranceStatus { get; set; }
    }

    /// <summary>
    /// وضعیت اعتبارسنجی بیمه
    /// </summary>
    public class InsuranceValidationStatus
    {
        public int InsuranceId { get; set; }
        public string InsuranceType { get; set; }
        public bool IsValid { get; set; }
        public List<InsuranceValidationIssue> Issues { get; set; } = new List<InsuranceValidationIssue>();
    }

    /// <summary>
    /// مسئله اعتبارسنجی
    /// </summary>
    public class InsuranceValidationIssue
    {
        public ValidationIssueType Type { get; set; }
        public ValidationSeverity Severity { get; set; }
        public string Message { get; set; }
        public string Recommendation { get; set; }
    }

    /// <summary>
    /// نوع مسئله اعتبارسنجی
    /// </summary>
    public enum ValidationIssueType
    {
        MissingPrimaryInsurance,
        ExpiredInsurance,
        FutureStartDate,
        InactiveInsurance,
        ExpiringSoon
    }

    /// <summary>
    /// شدت مسئله
    /// </summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Critical
    }
}
