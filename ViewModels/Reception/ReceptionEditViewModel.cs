using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception;

/// <summary>
/// ViewModel برای ویرایش پذیرش موجود
/// </summary>
public class ReceptionEditViewModel
{
    public int ReceptionId { get; set; }

    // اطلاعات بیمار
    [Required(ErrorMessage = "باید یک بیمار انتخاب شود.")]
    [Display(Name = "بیمار")]
    public int PatientId { get; set; }

    [Display(Name = "نام بیمار")]
    public string PatientFullName { get; set; }

    // اطلاعات پزشک
    [Required(ErrorMessage = "باید یک پزشک انتخاب شود.")]
    [Display(Name = "پزشک معالج")]
    public int DoctorId { get; set; }

    [Display(Name = "نام پزشک")]
    public string DoctorFullName { get; set; }

    // اطلاعات خدمات
    [Required(ErrorMessage = "باید حداقل یک خدمت انتخاب شود.")]
    [Display(Name = "خدمات")]
    public List<int> SelectedServiceIds { get; set; }

    [Display(Name = "خدمات انتخاب شده")]
    public string SelectedServicesNames { get; set; }

    // اطلاعات پذیرش
    [Required(ErrorMessage = "تاریخ پذیرش الزامی است.")]
    [Display(Name = "تاریخ پذیرش")]
    [DataType(DataType.DateTime)]
    public DateTime ReceptionDate { get; set; }

    [Display(Name = "تاریخ پذیرش (شمسی)")]
    public string ReceptionDateShamsi { get; set; }

    [Display(Name = "نوع پذیرش")]
    public ReceptionType Type { get; set; }

    [Display(Name = "اولویت")]
    public AppointmentPriority Priority { get; set; }

    [Display(Name = "آیا اورژانس است")]
    public bool IsEmergency { get; set; }

    [Display(Name = "آیا پذیرش آنلاین است")]
    public bool IsOnlineReception { get; set; }

    [Display(Name = "یادداشت‌ها")]
    [StringLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از ۱۰۰۰ کاراکتر باشد.")]
    public string Notes { get; set; }

    // اطلاعات پرداخت
    [Required]
    [Display(Name = "مجموع مبلغ")]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "مجموع مبلغ نمی‌تواند منفی باشد.")]
    public decimal TotalAmount { get; set; }

    [Required(ErrorMessage = "لطفاً یک روش پرداخت انتخاب کنید.")]
    [Display(Name = "روش پرداخت")]
    public PaymentMethod PaymentMethod { get; set; }

    [Display(Name = "شناسه تراکنش POS")]
    [StringLength(50, ErrorMessage = "شناسه تراکنش نمی‌تواند بیش از ۵۰ کاراکتر باشد.")]
    public string PosTransactionId { get; set; }

    [Display(Name = "مبلغ پرداخت شده")]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "مبلغ پرداخت شده نمی‌تواند منفی باشد.")]
    public decimal PaidAmount { get; set; }

    [Display(Name = "مبلغ باقی‌مانده")]
    [DataType(DataType.Currency)]
    public decimal RemainingAmount { get; set; }

    // اطلاعات بیمه
    [Display(Name = "بیمه اولیه")]
    public int? PrimaryInsuranceId { get; set; }

    [Display(Name = "بیمه تکمیلی")]
    public int? SecondaryInsuranceId { get; set; }

    [Display(Name = "سهم بیمه")]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "سهم بیمه نمی‌تواند منفی باشد.")]
    public decimal InsuranceShare { get; set; }

    [Display(Name = "سهم بیمار")]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "سهم بیمار نمی‌تواند منفی باشد.")]
    public decimal PatientShare { get; set; }

    // اطلاعات وضعیت
    [Display(Name = "وضعیت پذیرش")]
    public ReceptionStatus Status { get; set; }

    [Display(Name = "تاریخ ایجاد")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "تاریخ آخرین ویرایش")]
    public DateTime? UpdatedAt { get; set; }

    // لیست‌های کمکی
    public IEnumerable<SelectListItem> PatientList { get; set; }
    public IEnumerable<SelectListItem> DoctorList { get; set; }
    public IEnumerable<SelectListItem> ServiceList { get; set; }
    public IEnumerable<SelectListItem> InsuranceList { get; set; }
    public IEnumerable<SelectListItem> PaymentMethodList { get; set; }

    // Properties for lookup lists (for compatibility)
    public List<SelectListItem> Doctors { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Patients { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Services { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ServiceCategories { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> PaymentMethods { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> InsuranceProviders { get; set; } = new List<SelectListItem>();

    public ReceptionEditViewModel()
    {
        SelectedServiceIds = new List<int>();
        PatientList = new List<SelectListItem>();
        DoctorList = new List<SelectListItem>();
        ServiceList = new List<SelectListItem>();
        InsuranceList = new List<SelectListItem>();
        PaymentMethodList = new List<SelectListItem>();
        CreatedAt = DateTime.Now;
        TotalAmount = 0;
        PaidAmount = 0;
        InsuranceShare = 0;
        PatientShare = 0;
        Status = ReceptionStatus.Pending;
        Type = ReceptionType.Normal;
    }
}