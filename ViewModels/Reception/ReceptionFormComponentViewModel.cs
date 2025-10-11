using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای کامپوننت فرم پذیرش
    /// </summary>
    public class ReceptionFormComponentViewModel
    {
        [Display(Name = "شناسه پذیرش")]
        public int? ReceptionId { get; set; }

        [Display(Name = "شناسه بیمار")]
        [Required(ErrorMessage = "باید یک بیمار انتخاب شود")]
        public int? PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "شناسه پزشک")]
        [Required(ErrorMessage = "باید یک پزشک انتخاب شود")]
        public int? DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        [Required(ErrorMessage = "تاریخ پذیرش الزامی است")]
        public string ReceptionDate { get; set; }

        [Display(Name = "نوع پذیرش")]
        [Required]
        public ReceptionType Type { get; set; } = ReceptionType.Normal;

        [Display(Name = "وضعیت پذیرش")]
        [Required]
        public ReceptionStatus Status { get; set; } = ReceptionStatus.Pending;

        [Display(Name = "اولویت")]
        [Required]
        public AppointmentPriority Priority { get; set; } = AppointmentPriority.Normal;

        [Display(Name = "آیا اورژانس")]
        public bool IsEmergency { get; set; } = false;

        [Display(Name = "آیا آنلاین")]
        public bool IsOnlineReception { get; set; } = false;

        [Display(Name = "خدمات انتخاب شده")]
        public List<int> SelectedServiceIds { get; set; } = new List<int>();

        [Display(Name = "لیست خدمات")]
        public List<ServiceLookupViewModel> Services { get; set; } = new List<ServiceLookupViewModel>();

        [Display(Name = "مجموع مبلغ")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "مبلغ پرداخت شده")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "مبلغ باقی‌مانده")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "روش پرداخت")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "شناسه تراکنش POS")]
        public string PosTransactionId { get; set; }

        [Display(Name = "بیمه اولیه")]
        public int? PrimaryInsuranceId { get; set; }

        [Display(Name = "بیمه تکمیلی")]
        public int? SecondaryInsuranceId { get; set; }

        [Display(Name = "نام بیمه اولیه")]
        public string PrimaryInsuranceName { get; set; }

        [Display(Name = "نام بیمه تکمیلی")]
        public string SecondaryInsuranceName { get; set; }

        [Display(Name = "شماره بیمه")]
        public string InsuranceNumber { get; set; }

        [Display(Name = "سهم بیمه")]
        public decimal InsuranceShare { get; set; }

        [Display(Name = "سهم بیمار")]
        public decimal PatientShare { get; set; }

        [Display(Name = "یادداشت‌ها")]
        [StringLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از ۱۰۰۰ کاراکتر باشد")]
        public string Notes { get; set; }

        [Display(Name = "وضعیت فرم")]
        public string FormStatus { get; set; } = "آماده";

        [Display(Name = "آیا در حال ذخیره")]
        public bool IsSaving { get; set; } = false;

        [Display(Name = "لیست پزشکان")]
        public List<DoctorLookupViewModel> Doctors { get; set; } = new List<DoctorLookupViewModel>();

        [Display(Name = "لیست بیمه‌ها")]
        public List<InsuranceLookupViewModel> Insurances { get; set; } = new List<InsuranceLookupViewModel>();
    }

    /// <summary>
    /// ViewModel برای نمایش خدمت در لیست
    /// </summary>
    public class ServiceLookupViewModel
    {
        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }

        [Display(Name = "دسته‌بندی")]
        public string Category { get; set; }

        [Display(Name = "قیمت")]
        public decimal Price { get; set; }

        [Display(Name = "آیا فعال")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// ViewModel برای نمایش پزشک در لیست
    /// </summary>
    public class DoctorLookupViewModel
    {
        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "تخصص")]
        public string Specialization { get; set; }

        [Display(Name = "آیا فعال")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// ViewModel برای نمایش بیمه در لیست
    /// </summary>
    public class InsuranceLookupViewModel
    {
        [Display(Name = "شناسه بیمه")]
        public int InsuranceId { get; set; }

        [Display(Name = "نام بیمه")]
        public string InsuranceName { get; set; }

        [Display(Name = "نوع بیمه")]
        public string InsuranceType { get; set; }

        [Display(Name = "آیا فعال")]
        public bool IsActive { get; set; } = true;
    }
}
