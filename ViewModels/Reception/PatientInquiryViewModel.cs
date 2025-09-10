using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Models.Entities;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای استعلام کمکی اطلاعات بیمار
    /// این ViewModel برای بهبود دقت و سرعت پذیرش طراحی شده است
    /// </summary>
    public class PatientInquiryViewModel
    {
        /// <summary>
        /// کد ملی بیمار برای استعلام
        /// </summary>
        [Required(ErrorMessage = "کد ملی الزامی است")]
        [Display(Name = "کد ملی")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string NationalCode { get; set; }

        /// <summary>
        /// تاریخ تولد بیمار برای استعلام
        /// </summary>
        [Required(ErrorMessage = "تاریخ تولد الزامی است")]
        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// تاریخ تولد شمسی برای نمایش
        /// </summary>
        [Display(Name = "تاریخ تولد شمسی")]
        public string BirthDateShamsi { get; set; }

        /// <summary>
        /// نوع استعلام مورد نظر
        /// </summary>
        [Display(Name = "نوع استعلام")]
        public InquiryType InquiryType { get; set; } = InquiryType.Both;

        /// <summary>
        /// وضعیت استعلام
        /// </summary>
        [Display(Name = "وضعیت استعلام")]
        public InquiryStatus Status { get; set; } = InquiryStatus.NotStarted;

        /// <summary>
        /// پیغام نتیجه استعلام
        /// </summary>
        [Display(Name = "پیغام")]
        public string Message { get; set; }

        /// <summary>
        /// آیا استعلام موفق بوده است
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// زمان انجام استعلام
        /// </summary>
        public DateTime? InquiryTime { get; set; }

        /// <summary>
        /// اطلاعات هویت دریافتی از ثبت احوال
        /// </summary>
        public PatientIdentityData IdentityData { get; set; }

        /// <summary>
        /// اطلاعات بیمه دریافتی از سرویس بیمه
        /// </summary>
        public PatientInsuranceData InsuranceData { get; set; }

        /// <summary>
        /// آیا اطلاعات استعلام شده به فرم bind شده‌اند
        /// </summary>
        public bool IsDataBound { get; set; }

        public PatientInquiryViewModel()
        {
            IdentityData = new PatientIdentityData();
            InsuranceData = new PatientInsuranceData();
        }
    }

    /// <summary>
    /// نوع استعلام
    /// </summary>
    public enum InquiryType
    {
        [Display(Name = "هویت")]
        Identity = 1,
        [Display(Name = "بیمه")]
        Insurance = 2,
        [Display(Name = "هر دو")]
        Both = 3
    }

    /// <summary>
    /// وضعیت استعلام
    /// </summary>
    public enum InquiryStatus
    {
        [Display(Name = "شروع نشده")]
        NotStarted = 0,
        [Display(Name = "در حال انجام")]
        InProgress = 1,
        [Display(Name = "موفق")]
        Successful = 2,
        [Display(Name = "ناموفق")]
        Failed = 3,
        [Display(Name = "غیرفعال")]
        Disabled = 4
    }

    /// <summary>
    /// اطلاعات هویت بیمار از ثبت احوال
    /// </summary>
    public class PatientIdentityData
    {
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "نام پدر")]
        public string FatherName { get; set; }

        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "جنسیت")]
        public Gender? Gender { get; set; }

        [Display(Name = "محل تولد")]
        public string BirthPlace { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "شماره شناسنامه")]
        public string BirthCertificateNumber { get; set; }

        [Display(Name = "وضعیت تأیید")]
        public bool IsVerified { get; set; }

        [Display(Name = "تاریخ تأیید")]
        public DateTime? VerificationDate { get; set; }
    }

    /// <summary>
    /// اطلاعات بیمه بیمار از سرویس بیمه
    /// </summary>
    public class PatientInsuranceData
    {
        [Display(Name = "نام بیمه")]
        public string InsuranceName { get; set; }

        [Display(Name = "شماره بیمه")]
        public string InsuranceNumber { get; set; }

        [Display(Name = "وضعیت بیمه")]
        public string InsuranceStatus { get; set; }

        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "درصد پوشش")]
        public decimal? CoveragePercentage { get; set; }

        [Display(Name = "فرانشیز")]
        public decimal? Deductible { get; set; }

        [Display(Name = "وضعیت تأیید")]
        public bool IsVerified { get; set; }

        [Display(Name = "تاریخ تأیید")]
        public DateTime? VerificationDate { get; set; }
    }
}
