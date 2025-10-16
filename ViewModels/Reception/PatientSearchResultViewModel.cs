using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش نتایج جستجوی بیماران
    /// </summary>
    public class PatientSearchResultViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// کد ملی
        /// </summary>
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        /// <summary>
        /// نام
        /// </summary>
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        /// <summary>
        /// نام کامل
        /// </summary>
        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        /// <summary>
        /// تاریخ تولد
        /// </summary>
        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// تاریخ تولد شمسی
        /// </summary>
        [Display(Name = "تاریخ تولد شمسی")]
        public string BirthDateShamsi { get; set; }

        /// <summary>
        /// جنسیت
        /// </summary>
        [Display(Name = "جنسیت")]
        public string Gender { get; set; }

        /// <summary>
        /// جنسیت فارسی
        /// </summary>
        [Display(Name = "جنسیت")]
        public string GenderPersian => Gender switch
        {
            "Male" => "مرد",
            "Female" => "زن",
            _ => "نامشخص"
        };

        /// <summary>
        /// شماره تلفن
        /// </summary>
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// آدرس
        /// </summary>
        [Display(Name = "آدرس")]
        public string Address { get; set; }

        /// <summary>
        /// سن
        /// </summary>
        [Display(Name = "سن")]
        public int? Age => BirthDate.HasValue ? DateTime.Now.Year - BirthDate.Value.Year : null;

        /// <summary>
        /// وضعیت فعال
        /// </summary>
        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        /// <summary>
        /// وضعیت فعال فارسی
        /// </summary>
        [Display(Name = "وضعیت")]
        public string IsActivePersian => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ثبت")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// تاریخ ایجاد شمسی
        /// </summary>
        [Display(Name = "تاریخ ثبت شمسی")]
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// آیا بیمار قبلاً پذیرش شده است؟
        /// </summary>
        public bool HasPreviousReceptions { get; set; }

        /// <summary>
        /// تعداد پذیرش‌های قبلی
        /// </summary>
        public int PreviousReceptionsCount { get; set; }

        /// <summary>
        /// آخرین پذیرش
        /// </summary>
        public DateTime? LastReceptionDate { get; set; }

        /// <summary>
        /// آخرین پذیرش شمسی
        /// </summary>
        public string LastReceptionDateShamsi { get; set; }
    }
}
