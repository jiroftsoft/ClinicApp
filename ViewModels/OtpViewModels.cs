using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ویومدل مربوط به فرم اولیه ورود کد ملی برای ارسال OTP.
    /// این ویومدل بر اساس استانداردهای سیستم‌های پزشکی ایرانی طراحی شده است.
    /// </summary>
    public class EnterNationalCodeViewModel
    {
        /// <summary>
        /// کد ملی کاربر (به فرمت فارسی یا انگلیسی).
        /// در سیستم‌های پزشکی ایرانی، کد ملی به عنوان شناسه اصلی استفاده می‌شود.
        /// </summary>
        [Required(ErrorMessage = "وارد کردن کد ملی الزامی است.")]
        [Display(Name = "کد ملی")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد.")]
        public string NationalCode { get; set; }
    }



 


    /// <summary>
    /// نسخه نهایی ویومدل ثبت‌نام بیمار با اعتبارسنجی اعلانی و تمیز.
    /// </summary>
    public class RegisterPatientViewModel
    {
        [Required(ErrorMessage = "کد ملی الزامی است.")]
        [Display(Name = "کد ملی")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد.")]
        public string NationalCode { get; set; }

        [Required(ErrorMessage = "وارد کردن نام الزامی است.")]
        [Display(Name = "نام")]
        [StringLength(100, ErrorMessage = "نام نمی‌تواند بیشتر از 100 کاراکتر باشد.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "وارد کردن نام خانوادگی الزامی است.")]
        [Display(Name = "نام خانوادگی")]
        [StringLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از 100 کاراکتر باشد.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "وارد کردن شماره موبایل الزامی است.")]
        [Display(Name = "شماره موبایل")]
        [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "فرمت شماره موبایل نامعتبر است. (مثال: 09123456789)")]
        public string PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "آدرس ایمیل نامعتبر است.")]
        [StringLength(256, ErrorMessage = "آدرس ایمیل نمی‌تواند بیشتر از 256 کاراکتر باشد.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "انتخاب جنسیت الزامی است.")]
        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "انتخاب بیمه الزامی است.")]
        [Display(Name = "بیمه")]
        public int InsuranceId { get; set; }

        /// <summary>
        /// ✅ **تغییر اصلی اینجا اتفاق افتاده است**
        /// پراپرتی BirthDate حذف شده و تمام منطق به Attribute منتقل شده است.
        /// </summary>
        [Display(Name = "تاریخ تولد")]
        [PersianDate(
            IsRequired = false, // تاریخ تولد اختیاری است
            MustBePastDate = true, // تاریخ تولد باید در گذشته باشد
            ErrorMessage = "تاریخ تولد وارد شده معتبر نیست.",
            PastDateRequiredMessage = "تاریخ تولد نمی‌تواند در آینده باشد."
        )]
        public string BirthDatePersian { get; set; }

        [Display(Name = "آدرس")]
        [StringLength(500, ErrorMessage = "آدرس نمی‌تواند بیشتر از 500 کاراکتر باشد.")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [Display(Name = "نام پزشک معالج")]
        [StringLength(200, ErrorMessage = "نام پزشک نمی‌تواند بیشتر از 200 کاراکتر باشد.")]
        public string DoctorName { get; set; }
    }

    // For Stage 1
    public class CheckNationalCodeViewModel
    {
        [Required]
        [Display(Name = "کد ملی")]
        [StringLength(10, MinimumLength = 10)]
        public string NationalCode { get; set; }
    }

    // For Stage 2
    public class SendRegistrationOtpViewModel
    {
        [Required]
        public string NationalCode { get; set; } // Will be a hidden field

        [Required]
        [Display(Name = "شماره موبایل")]
        [RegularExpression(@"^09[0-9]{9}$")]
        public string PhoneNumber { get; set; }
    }


    /// <summary>
    /// ویومدل مربوط به فرم تایید کد OTP.
    /// این ویومدل بر اساس استانداردهای سیستم‌های پزشکی ایرانی طراحی شده است.
    ///   // For Stage 3 (VerifyOtpViewModel can be reused)
    /// </summary>
    public class VerifyRegistrationOtpViewModel
    {
        /// <summary>
        /// کد ملی کاربر که کد برای آن ارسال شده است.
        /// این فیلد معمولاً به صورت مخفی در فرم قرار می‌گیرد.
        /// </summary>
        [Required(ErrorMessage = "کد ملی الزامی است.")]
        [Display(Name = "کد ملی")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد.")]
        public string NationalCode { get; set; }
        [Required]
        public string PhoneNumber { get; set; } // We now need the phone number too

        /// <summary>
        /// کد تایید 6 رقمی وارد شده توسط کاربر.
        /// </summary>
        [Required(ErrorMessage = "وارد کردن کد تایید الزامی است.")]
        [Display(Name = "کد تایید")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "کد تایید باید یک عدد 6 رقمی باشد.")]
        public string OtpCode { get; set; }
    }
    // در فایل ViewModels/OtpViewModels.cs
    public class VerifyLoginOtpViewModel
    {
        [Required(ErrorMessage = "کد ملی الزامی است.")]
        public string NationalCode { get; set; }

        [Required(ErrorMessage = "وارد کردن کد تایید الزامی است.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "کد تایید باید یک عدد 6 رقمی باشد.")]
        public string OtpCode { get; set; }
    }
}

