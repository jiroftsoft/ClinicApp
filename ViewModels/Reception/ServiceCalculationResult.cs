using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    public class ServiceCalculationResult
    {
        public decimal TotalAmount { get; set; }
        public List<ServiceCalculationDetail> ServiceDetails { get; set; } = new List<ServiceCalculationDetail>();
        public decimal InsuranceCoverage { get; set; }
        public decimal PatientShare { get; set; }
        public bool IsCalculationSuccessful { get; set; }
        public string CalculationMessage { get; set; }
        public DateTime CalculatedAt { get; set; }
        public string CalculatedBy { get; set; }
        
        // Additional properties for compatibility
        public int ServiceId { get; set; }
        public decimal ServiceAmount { get; set; }
        public decimal TotalBaseAmount { get; set; }
        public decimal TotalInsuranceCoverage { get; set; }
        public decimal TotalPatientShare { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public List<ServiceCalculationItemResult> ServiceDetailsExtended { get; set; } = new List<ServiceCalculationItemResult>();
        public List<string> AppliedDiscounts { get; set; } = new List<string>();
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public bool HasInsurance { get; set; }
        public string InsuranceDetails { get; set; }
    }

    public class ServiceCalculationDetail
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal InsuranceShare { get; set; }
        public decimal PatientShare { get; set; }
        public string CalculationNotes { get; set; }
    }

    public class ServiceCalculationItemResult
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal InsuranceCoverage { get; set; }
        public decimal PatientShare { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public bool IsInsuranceCovered { get; set; }
    }
}
