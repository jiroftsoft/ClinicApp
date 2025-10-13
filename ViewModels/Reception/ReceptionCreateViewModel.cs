using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception;

/// <summary>
/// ViewModel برای فرم پذیرش اصلی در محل.
/// این ViewModel پیچیده‌ترین ViewModel است که تمام داده‌ها را برای پذیرش جدید مدیریت می‌کند.
/// </summary>
public class ReceptionCreateViewModel
{
    // اطلاعات بیمار
    [Required(ErrorMessage = "باید یک بیمار انتخاب شود.")]
    [Display(Name = "بیمار")]
    public int PatientId { get; set; }

    [Display(Name = "نام بیمار")]
    public string PatientFullName { get; set; } // برای نمایش نام بعد از جستجو

    // فیلدهای اطلاعات بیمار برای نمایش و ویرایش
    [Display(Name = "نام")]
    [StringLength(50, ErrorMessage = "نام نمی‌تواند بیش از ۵۰ کاراکتر باشد.")]
    public string FirstName { get; set; }

    [Display(Name = "نام خانوادگی")]
    [StringLength(50, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از ۵۰ کاراکتر باشد.")]
    public string LastName { get; set; }

    [Display(Name = "کد ملی")]
    [StringLength(10, ErrorMessage = "کد ملی باید ۱۰ رقم باشد.")]
    public string NationalCode { get; set; }

    [Display(Name = "شماره تلفن")]
    [StringLength(15, ErrorMessage = "شماره تلفن نمی‌تواند بیش از ۱۵ کاراکتر باشد.")]
    public string PhoneNumber { get; set; }

    [Display(Name = "جنسیت")]
    public int? Gender { get; set; }

    [Display(Name = "تاریخ تولد")]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    //[Display(Name = "تاریخ تولد شمسی")]
    //public string BirthDateShamsi { get; set; }

    [Display(Name = "تاریخ تولد شمسی")]
    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
    public string BirthDateShamsi { get; set; }

    [Display(Name = "آدرس")]
    [StringLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از ۵۰۰ کاراکتر باشد.")]
    public string Address { get; set; }

    // اطلاعات استعلام کمکی
    [Display(Name = "کد ملی برای استعلام")]
    public string NationalCodeForInquiry { get; set; }

    [Display(Name = "تاریخ تولد برای استعلام")]
    public DateTime? BirthDateForInquiry { get; set; }

    [Display(Name = "تاریخ تولد شمسی برای استعلام")]
    public string BirthDateShamsiForInquiry { get; set; }

    [Display(Name = "نتیجه استعلام")]
    public Reception.PatientInquiryViewModel InquiryResult { get; set; }

    [Display(Name = "وضعیت استعلام")]
    public bool IsInquiryCompleted { get; set; }

    // Properties for Controller
    [Display(Name = "تاریخ پذیرش")]
    public DateTime ReceptionDate { get; set; }

    [Display(Name = "تاریخ پذیرش (شمسی)")]
    public string ReceptionDateShamsi { get; set; }

    [Display(Name = "آیا اورژانس است")]
    public bool IsEmergency { get; set; }

    [Display(Name = "آیا پذیرش آنلاین است")]
    public bool IsOnlineReception { get; set; }

    [Display(Name = "نوع پذیرش")]
    public ReceptionType Type { get; set; }

    [Display(Name = "وضعیت پذیرش")]
    public ReceptionStatus Status { get; set; }

    [Display(Name = "اولویت")]
    public AppointmentPriority Priority { get; set; }

    // اطلاعات پزشک
    [Required(ErrorMessage = "باید یک پزشک انتخاب شود.")]
    [Display(Name = "پزشک معالج")]
    public int DoctorId { get; set; }
    public IEnumerable<SelectListItem> DoctorList { get; set; } // برای منوی انتخاب پزشک

    // اطلاعات خدمت (برای Controller)
    [Display(Name = "شناسه خدمت")]
    public int ServiceId { get; set; }

    // اطلاعات خدمات (پشتیبانی از افزودن چندین خدمت)
    [Required(ErrorMessage = "باید حداقل یک خدمت انتخاب شود.")]
    [Display(Name = "خدمات")]
    public List<int> SelectedServiceIds { get; set; }
    public IEnumerable<SelectListItem> ServiceList { get; set; } // برای انتخاب خدمات

    // اطلاعات پرداخت
    [Required]
    [Display(Name = "مجموع مبلغ")]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "مجموع مبلغ نمی‌تواند منفی باشد.")]
    public decimal TotalAmount { get; set; }

    [Required(ErrorMessage = "لطفاً یک روش پرداخت انتخاب کنید.")]
    [Display(Name = "روش پرداخت")]
    public PaymentMethod PaymentMethod { get; set; }
        
    // برای منوی انتخاب روش پرداخت
    public IEnumerable<SelectListItem> PaymentMethodList { get; set; }

    [Display(Name = "شناسه تراکنش POS")]
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

    [Display(Name = "نام بیمه اولیه")]
    public string PrimaryInsuranceName { get; set; }

    [Display(Name = "نام بیمه تکمیلی")]
    public string SecondaryInsuranceName { get; set; }

    [Display(Name = "شماره بیمه")]
    [StringLength(20, ErrorMessage = "شماره بیمه نمی‌تواند بیش از ۲۰ کاراکتر باشد.")]
    public string InsuranceNumber { get; set; }

    [Display(Name = "سهم بیمه")]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "سهم بیمه نمی‌تواند منفی باشد.")]
    public decimal InsuranceShare { get; set; }

    [Display(Name = "سهم بیمار")]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "سهم بیمار نمی‌تواند منفی باشد.")]
    public decimal PatientShare { get; set; }

    [Display(Name = "یادداشت‌ها")]
    [StringLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از ۱۰۰۰ کاراکتر باشد.")]
    public string Notes { get; set; }

    [Display(Name = "سهم بیمار")]
    public decimal PatientCoPay { get; set; }

    [Display(Name = "سهم بیمه")]
    public decimal InsurerShareAmount { get; set; }

    [Display(Name = "شناسه کاربر ایجادکننده")]
    public string CreatedByUserId { get; set; }

    // Properties for lookup lists
    public List<SelectListItem> Doctors { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Patients { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Services { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ServiceCategories { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> PaymentMethods { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> InsuranceProviders { get; set; } = new List<SelectListItem>();

    public ReceptionCreateViewModel()
    {
        SelectedServiceIds = new List<int>();
        DoctorList = new List<SelectListItem>();
        ServiceList = new List<SelectListItem>();
        PaymentMethodList = new List<SelectListItem>();
        ReceptionDate = DateTime.Now;
        IsInquiryCompleted = false;
        TotalAmount = 0;
        PaidAmount = 0;
        InsuranceShare = 0;
        PatientShare = 0;
        Type = ReceptionType.Normal;
        InquiryResult = new Reception.PatientInquiryViewModel();
    }
}