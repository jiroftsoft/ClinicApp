using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای صفحه اصلی پذیرش
    /// </summary>
    public class ReceptionIndexViewModel
    {
        [Display(Name = "شناسه پذیرش")]
        public int ReceptionId { get; set; }

        [Display(Name = "نام کامل بیمار")]
        public string PatientFullName { get; set; }

        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "نام کامل پزشک")]
        public string DoctorFullName { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        public string ReceptionDate { get; set; }

        [Display(Name = "مجموع مبلغ")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "مبلغ پرداخت شده")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "مبلغ باقی‌مانده")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "روش پرداخت")]
        public string PaymentMethod { get; set; }

        [Display(Name = "تاریخ جستجو")]
        public string SearchDate { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        [Display(Name = "نوع پذیرش")]
        public string Type { get; set; }

        [Display(Name = "پزشک")]
        public string Doctor { get; set; }

        [Display(Name = "بیمار")]
        public string Patient { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "تلفن بیمار")]
        public string PatientPhoneNumber { get; set; }

        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        [Display(Name = "سهم بیمار")]
        public decimal PatientCoPay { get; set; }

        [Display(Name = "سهم بیمه")]
        public decimal InsurerShareAmount { get; set; }

        [Display(Name = "اولویت")]
        public AppointmentPriority Priority { get; set; }

        [Display(Name = "اورژانس")]
        public bool IsEmergency { get; set; }

        [Display(Name = "پذیرش آنلاین")]
        public bool IsOnlineReception { get; set; }

        [Display(Name = "نام بخش")]
        public string DepartmentName { get; set; }

        [Display(Name = "لیست پذیرش‌ها")]
        public List<ReceptionListItemViewModel> Receptions { get; set; } = new List<ReceptionListItemViewModel>();

        [Display(Name = "آمار روزانه")]
        public ReceptionDailyStatsViewModel DailyStats { get; set; }

        [Display(Name = "آمار پزشکان")]
        public List<ReceptionDoctorStatsViewModel> DoctorStats { get; set; } = new List<ReceptionDoctorStatsViewModel>();

        [Display(Name = "فیلترها")]
        public ReceptionSearchCriteria SearchCriteria { get; set; } = new ReceptionSearchCriteria();
    }

    /// <summary>
    /// ViewModel برای نمایش آیتم پذیرش در لیست
    /// </summary>
    public class ReceptionListItemViewModel
    {
        [Display(Name = "شناسه")]
        public int ReceptionId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        public string ReceptionDate { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        [Display(Name = "نوع")]
        public string Type { get; set; }

        [Display(Name = "مجموع مبلغ")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "مبلغ پرداخت شده")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "مبلغ باقی‌مانده")]
        public decimal RemainingAmount { get; set; }
    }
}
