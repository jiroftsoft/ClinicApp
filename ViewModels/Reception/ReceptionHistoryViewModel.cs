using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای مدیریت سوابق پذیرش
    /// </summary>
    public class ReceptionHistoryViewModel
    {
        public int ReceptionId { get; set; }
        
        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }
        
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }
        
        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }
        
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }
        
        [Display(Name = "تاریخ پذیرش")]
        public string ReceptionDate { get; set; }
        
        [Display(Name = "وضعیت")]
        public string Status { get; set; }
        
        [Display(Name = "مبلغ کل")]
        public decimal TotalAmount { get; set; }
        
        [Display(Name = "سهم بیمه")]
        public decimal InsuranceShare { get; set; }
        
        [Display(Name = "سهم بیمار")]
        public decimal PatientShare { get; set; }
        
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedDate { get; set; }
        
        [Display(Name = "تاریخ آخرین بروزرسانی")]
        public DateTime? LastModifiedDate { get; set; }
    }

    /// <summary>
    /// ViewModel برای فیلترهای جستجوی سوابق پذیرش
    /// </summary>
    public class ReceptionHistorySearchViewModel
    {
        [Display(Name = "از تاریخ")]
        public string DateFrom { get; set; }
        
        [Display(Name = "تا تاریخ")]
        public string DateTo { get; set; }
        
        [Display(Name = "کد ملی بیمار")]
        public string NationalCode { get; set; }
        
        [Display(Name = "وضعیت پذیرش")]
        public string Status { get; set; }
        
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }
        
        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }
    }

    /// <summary>
    /// ViewModel برای اطلاعات بیمار در جزئیات پذیرش
    /// </summary>
    public class PatientInfoViewModel
    {
        public int PatientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    /// <summary>
    /// ViewModel برای اطلاعات خدمت در جزئیات پذیرش
    /// </summary>
    public class ServiceInfoViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public decimal ServicePrice { get; set; }
        public string ServiceDescription { get; set; }
    }

    /// <summary>
    /// ViewModel برای اطلاعات پزشک در جزئیات پذیرش
    /// </summary>
    public class DoctorInfoViewModel
    {
        public int DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
        public string MedicalNumber { get; set; }
    }

    /// <summary>
    /// ViewModel برای اطلاعات بیمه در جزئیات پذیرش
    /// </summary>
    public class InsuranceInfoViewModel
    {
        public string PrimaryInsurance { get; set; }
        public string SupplementaryInsurance { get; set; }
        public decimal InsuranceShare { get; set; }
        public decimal PatientShare { get; set; }
    }

    /// <summary>
    /// ViewModel برای اطلاعات پرداخت در جزئیات پذیرش
    /// </summary>
    public class PaymentInfoViewModel
    {
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string PaymentStatus { get; set; }
        public string ReferenceNumber { get; set; }
    }
}
