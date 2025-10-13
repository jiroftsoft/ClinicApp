using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception;

/// <summary>
/// ViewModel دقیق برای نمایش تمام اطلاعات یک پذیرش.
/// </summary>
public class ReceptionDetailsViewModel
{
    public int ReceptionId { get; set; }
        
    public int PatientId { get; set; }
    public int DoctorId { get; set; }

    [Display(Name = "نام بیمار")]
    public string PatientFullName { get; set; }

    [Display(Name = "کد ملی")]
    public string PatientNationalCode { get; set; }

    [Display(Name = "شماره تماس بیمار")]
    public string PatientPhoneNumber { get; set; }

    [Display(Name = "نام پزشک")]
    public string DoctorFullName { get; set; }

    [Display(Name = "تخصص پزشک")]
    public string DoctorSpecialization { get; set; }

    [Display(Name = "تاریخ پذیرش")]
    public string ReceptionDate { get; set; }

    [Display(Name = "نوع پذیرش")]
    public string Type { get; set; }

    [Display(Name = "وضعیت")]
    public string Status { get; set; }

    [Display(Name = "مجموع مبلغ")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "مبلغ پرداخت شده")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal PaidAmount { get; set; }

    [Display(Name = "مبلغ پرداخت شده")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal AmountPaid => PaidAmount;

    [Display(Name = "مبلغ باقی‌مانده")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal RemainingAmount => TotalAmount - PaidAmount;

    [Display(Name = "روش پرداخت")]
    public string PaymentMethod { get; set; }

    [Display(Name = "شناسه تراکنش POS")]
    public string PosTransactionId { get; set; }

    [Display(Name = "بیمه اولیه")]
    public string PrimaryInsurance { get; set; }

    [Display(Name = "بیمه تکمیلی")]
    public string SecondaryInsurance { get; set; }

    [Display(Name = "سهم بیمه")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal InsuranceShare { get; set; }

    [Display(Name = "سهم بیمار")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal PatientShare { get; set; }

    [Display(Name = "یادداشت‌ها")]
    public string Notes { get; set; }

    // Additional properties for compatibility
    [Display(Name = "شماره پذیرش")]
    public string ReceptionNumber { get; set; }

    [Display(Name = "اولویت")]
    public AppointmentPriority Priority { get; set; }

    [Display(Name = "آیا اورژانس است")]
    public bool IsEmergency { get; set; }

    [Display(Name = "آیا پذیرش آنلاین است")]
    public bool IsOnlineReception { get; set; }

    [Display(Name = "آیتم‌های پذیرش")]
    public List<ReceptionItemViewModel> ReceptionItems { get; set; } = new List<ReceptionItemViewModel>();

    [Display(Name = "تاریخ ایجاد")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "آخرین ویرایش")]
    public DateTime? UpdatedAt { get; set; }

    [Display(Name = "ایجاد کننده")]
    public string CreatedBy { get; set; }

    public List<ReceptionItemViewModel> Services { get; set; }
    public List<ClinicApp.ViewModels.Payment.ReceptionPaymentViewModel> Payments { get; set; }

    public ReceptionDetailsViewModel()
    {
        Services = new List<ReceptionItemViewModel>();
        Payments = new List<ClinicApp.ViewModels.Payment.ReceptionPaymentViewModel>();
        CreatedAt = DateTime.Now;
        TotalAmount = 0;
        PaidAmount = 0;
        InsuranceShare = 0;
        PatientShare = 0;
        Status = "در انتظار";
        Type = "عادی";
    }
}