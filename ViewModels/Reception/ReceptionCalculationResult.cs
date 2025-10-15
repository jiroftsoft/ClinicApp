using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// نتیجه محاسبه پذیرش
    /// </summary>
    public class ReceptionCalculationResult
    {
        public int PatientId { get; set; }
        public DateTime ReceptionDate { get; set; }
        public List<ServiceCalculationResult> ServiceCalculations { get; set; } = new List<ServiceCalculationResult>();
        public decimal TotalServiceAmount { get; set; }
        public decimal TotalInsuranceCoverage { get; set; }
        public decimal TotalPatientShare { get; set; }
        public bool HasInsurance { get; set; }
        public DateTime CalculationDate { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// نتیجه محاسبه یک خدمت
    /// </summary>
    public class ServiceCalculationResult
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public decimal ServiceAmount { get; set; }
        public decimal InsuranceCoverage { get; set; }
        public decimal PatientShare { get; set; }
        public bool HasInsurance { get; set; }
        public object InsuranceDetails { get; set; }
    }

    /// <summary>
    /// محاسبه سریع پذیرش
    /// </summary>
    public class QuickReceptionCalculation
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal ServiceAmount { get; set; }
        public decimal InsuranceCoverage { get; set; }
        public decimal PatientShare { get; set; }
        public bool HasInsurance { get; set; }
        public decimal CoveragePercent { get; set; }
    }
}
