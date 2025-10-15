using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// مدل فرم پذیرش
    /// </summary>
    public class ReceptionFormViewModel
    {
        [Required(ErrorMessage = "شناسه بیمار الزامی است")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "تاریخ پذیرش الزامی است")]
        public DateTime ReceptionDate { get; set; }

        public List<SelectedServiceViewModel> SelectedServices { get; set; } = new List<SelectedServiceViewModel>();
        
        public string Notes { get; set; }
        
        public int? DoctorId { get; set; }
        
        public int? DepartmentId { get; set; }
    }

    /// <summary>
    /// مدل خدمت انتخاب شده
    /// </summary>
    public class SelectedServiceViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// درخواست محاسبه فرم پذیرش
    /// </summary>
    public class ReceptionFormCalculationRequest
    {
        public int PatientId { get; set; }
        public List<int> ServiceIds { get; set; } = new List<int>();
        public DateTime ReceptionDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// نتیجه محاسبه فرم پذیرش
    /// </summary>
    public class ReceptionFormCalculation
    {
        public int PatientId { get; set; }
        public decimal TotalServiceAmount { get; set; }
        public decimal TotalInsuranceCoverage { get; set; }
        public decimal TotalPatientShare { get; set; }
        public bool HasInsurance { get; set; }
        public List<ServiceCalculationResult> ServiceCalculations { get; set; } = new List<ServiceCalculationResult>();
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// نتیجه ایجاد پذیرش
    /// </summary>
    public class ReceptionFormResult
    {
        public int ReceptionId { get; set; }
        public int PatientId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PatientShare { get; set; }
        public decimal InsuranceCoverage { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string Status { get; set; }
        public string ReceptionNumber { get; set; }
    }

    /// <summary>
    /// اطلاعات فرم پذیرش
    /// </summary>
    public class ReceptionFormInfo
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string NationalCode { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime ReceptionDate { get; set; }
        public List<ServiceOption> AvailableServices { get; set; } = new List<ServiceOption>();
        public InsuranceInfo InsuranceInfo { get; set; } = new InsuranceInfo();
    }

    /// <summary>
    /// گزینه خدمت
    /// </summary>
    public class ServiceOption
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public decimal BasePrice { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// اطلاعات بیمه
    /// </summary>
    public class InsuranceInfo
    {
        public bool HasPrimaryInsurance { get; set; }
        public bool HasSupplementaryInsurance { get; set; }
        public string PrimaryInsuranceName { get; set; }
        public string SupplementaryInsuranceName { get; set; }
        public decimal CoveragePercent { get; set; }
    }
}
