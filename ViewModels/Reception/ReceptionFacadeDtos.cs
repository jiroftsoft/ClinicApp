using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicApp.ViewModels.Reception
{
    #region Load DTOs

    /// <summary>
    /// DTO برای بارگذاری اولیه فرم پذیرش
    /// </summary>
    public class ReceptionLoadDto
    {
        public List<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();
        public List<ServiceDto> Services { get; set; } = new List<ServiceDto>();
        public List<ServiceDto> SharedServices { get; set; } = new List<ServiceDto>();
        public List<DoctorDto> Doctors { get; set; } = new List<DoctorDto>();
    }

    /// <summary>
    /// DTO برای بیمار
    /// </summary>
    public class PatientDto
    {
        public int PatientId { get; set; }
        public string NationalCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
    }

    /// <summary>
    /// DTO برای ایجاد بیمار
    /// </summary>
    public class PatientCreateDto
    {
        public string NationalCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
    }

    /// <summary>
    /// DTO برای بسته بیمه‌های بیمار
    /// </summary>
    public class InsuranceBundleDto
    {
        public int PatientId { get; set; }
        public List<InsuranceDto> BaseInsurances { get; set; } = new List<InsuranceDto>();
        public List<InsuranceDto> SupplementaryInsurances { get; set; } = new List<InsuranceDto>();
    }

    /// <summary>
    /// DTO برای بیمه
    /// </summary>
    public class InsuranceDto
    {
        public int InsuranceId { get; set; }
        public string InsuranceName { get; set; }
        public string InsuranceType { get; set; }
        public decimal CoveragePercentage { get; set; }
        public decimal Franchise { get; set; }
        public decimal Ceiling { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    #endregion

    #region Service DTOs

    /// <summary>
    /// DTO برای لیست انتخاب خدمات
    /// </summary>
    public class ServicePickListDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<ServicePickItemDto> Services { get; set; } = new List<ServicePickItemDto>();
    }

    /// <summary>
    /// DTO برای آیتم انتخاب خدمات
    /// </summary>
    public class ServicePickItemDto
    {
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO برای نتیجه افزودن آیتم
    /// </summary>
    public class AddItemResultDto
    {
        public int ReceptionId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ItemTotal { get; set; }
        public ReceptionTotalsDto ReceptionTotals { get; set; }
    }

    #endregion

    #region Payment DTOs

    /// <summary>
    /// DTO برای پرداخت POS
    /// </summary>
    public class PosPaymentDto
    {
        public decimal Amount { get; set; }
        public string Rrn { get; set; }
        public string TraceNo { get; set; }
        public string TerminalId { get; set; }
        public string Last4 { get; set; }
        public string CardType { get; set; }
    }

    /// <summary>
    /// DTO برای پرداخت نقدی
    /// </summary>
    public class CashPaymentDto
    {
        public decimal Amount { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO برای نتیجه نهایی‌سازی
    /// </summary>
    public class FinalizeResultDto
    {
        public int ReceptionId { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime FinalizedAt { get; set; }
        public string ReceptionNumber { get; set; }
    }

    #endregion

    #region Department & Doctor DTOs

    /// <summary>
    /// DTO برای دپارتمان
    /// </summary>
    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO برای پزشک
    /// </summary>
    public class DoctorDto
    {
        public int DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string DoctorCode { get; set; }
        public string Specialization { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO برای خدمت
    /// </summary>
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool IsHashtagged { get; set; }
    }

    #endregion

    #region Totals DTOs

    /// <summary>
    /// DTO برای مجموع‌های پذیرش
    /// </summary>
    public class ReceptionTotalsDto
    {
        public decimal GrossAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DeductionAmount { get; set; }
        public decimal BaseInsurancePayable { get; set; }
        public decimal SupplementaryInsurancePayable { get; set; }
        public decimal PatientPayable { get; set; }
        public decimal NetAmount => GrossAmount - DiscountAmount + DeductionAmount;
        public decimal TotalInsurancePayable => BaseInsurancePayable + SupplementaryInsurancePayable;
    }

    #endregion
}
