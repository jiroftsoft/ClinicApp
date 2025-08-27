using ClinicApp.Core;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using ClinicApp.Models.Entities;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ویومدل پایه بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 2. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی
    /// 3. پشتیبانی از محیط‌های پزشکی ایرانی با تاریخ شمسی و اعداد فارسی
    /// 4. رعایت استانداردهای امنیتی سیستم‌های پزشکی
    /// 5. پشتیبانی از سیستم بیمه‌ها و محاسبات مالی
    /// </summary>
    public class PatientViewModel
    {
        public int PatientId { get; set; }

        [Display(Name = "کد ملی")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "{0} باید ۱۰ رقم باشد.")]
        public string NationalCode { get; set; }

        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }

        [Display(Name = "شماره موبایل")]
        public string PhoneNumber { get; set; }

        [Display(Name = "تاریخ تولد")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "تاریخ تولد (شمسی)")]
        public string BirthDateShamsi { get; set; }

        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        [Display(Name = "جنسیت")]
        public string GenderDisplay => Gender == Gender.Male ? "مرد" : "زن";


        [Display(Name = "آدرس")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [Display(Name = "بیمه")]
        public string InsuranceName { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ورود")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "تاریخ آخرین ورود (شمسی)")]
        public string LastLoginDateShamsi { get; set; }

        [Display(Name = "تعداد پذیرش‌ها")]
        public int ReceptionCount { get; set; }

        [Display(Name = "مانده بدهی")]
        public decimal DebtBalance { get; set; }

        [Display(Name = "مانده بدهی")]
        public string DebtBalanceFormatted => DebtBalance > 0 ?
            DebtBalance.ToString("N0") + " ریال" : "بدون بدهی";

        // فیلدهای مربوط به سیستم حذف نرم
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedAtShamsi { get; set; }
        public string DeletedByUser { get; set; }
    }

    /// <summary>
    /// ویومدل اختصاصی برای فرم‌های ایجاد و ویرایش بیمار
    /// شامل تمام فیلدهای قابل ویرایش و قوانین اعتبارسنجی
    /// </summary>
    public class PatientCreateEditViewModel
    {
        // PatientId فقط در حالت ویرایش مقدار دارد
        public int PatientId { get; set; }

        [Display(Name = "کد ملی")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "{0} باید ۱۰ رقم باشد.")]
        public string NationalCode { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(150, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(150, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string LastName { get; set; }

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [RegularExpression("09[0-9]{9}", ErrorMessage = "فرمت {0} صحیح نیست.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "آدرس ایمیل نامعتبر است.")]
        [MaxLength(256, ErrorMessage = "آدرس ایمیل نمی‌تواند بیش از 256 کاراکتر باشد.")]
        public string Email { get; set; }

        [Display(Name = "جنسیت")]
        [Required(ErrorMessage = "انتخاب {0} الزامی است.")]
        public Gender Gender { get; set; }

        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "تاریخ تولد (شمسی)")]
        [PersianDate(
            IsRequired = false,
            MustBePastDate = true,
            ErrorMessage = "تاریخ تولد وارد شده معتبر نیست.",
            PastDateRequiredMessage = "تاریخ تولد نمی‌تواند در آینده باشد."
        )]
        public string BirthDateShamsi { get; set; }

        [Display(Name = "آدرس")]
        [DataType(DataType.MultilineText)]
        [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string Address { get; set; }

        [Display(Name = "بیمه")]
        [Required(ErrorMessage = "انتخاب {0} الزامی است.")]
        public int InsuranceId { get; set; }

        [Display(Name = "نام بیمه")]
        public string InsuranceName { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Insurances { get; set; }

        [Display(Name = "نام پزشک معالج")]
        [MaxLength(200, ErrorMessage = "نام پزشک نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string DoctorName { get; set; }

        // فیلدهای مربوط به تاریخ ایجاد و ویرایش
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedByUser { get; set; }
    }

    /// <summary>
    /// ویومدل سبک برای نمایش هر ردیف در لیست بیماران
    /// فقط شامل اطلاعات ضروری برای نمایش در جدول
    /// </summary>
    public class PatientIndexViewModel
    {
        public int PatientId { get; set; }

        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }
        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        [Display(Name = "جنسیت")]
        public string GenderDisplay => this.Gender == Gender.Male ? "مرد" : "زن";

        [Display(Name = "شماره موبایل")]
        public string PhoneNumber { get; set; }

        [Display(Name = "بیمه")]
        public string InsuranceName { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "مانده بدهی")]
        public string DebtBalanceFormatted => DebtBalance > 0 ?
            DebtBalance.ToString("N0") + " ریال" : "بدون بدهی";

        public decimal DebtBalance { get; set; }

        [Display(Name = "تاریخ آخرین ورود")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "تاریخ آخرین ورود (شمسی)")]
        public string LastLoginDateShamsi { get; set; }

        public int ReceptionCount { get; set; }
        [Display(Name = "تاریخ تولد (شمسی)")]
        public string BirthDateShamsi { get; set; }

    }

    /// <summary>
    /// مدل ویو برای جزئیات بیمار - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش کامل اطلاعات بیمار با رعایت استانداردهای پزشکی
    /// 2. پشتیبانی از محیط‌های ایرانی با تاریخ شمسی و اعداد فارسی
    /// 3. رعایت استانداردهای سیستم‌های پزشکی ایران
    /// 4. امنیت بالا با عدم نمایش اطلاعات حساس
    /// 5. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 6. مدیریت کامل ردیابی (Audit Trail)
    /// </summary>
    public class PatientDetailsViewModel
    {
        public int PatientId { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        [Display(Name = "جنسیت")]
        public string GenderDisplay => this.Gender == Gender.Male ? "مرد" : "زن";

        [Display(Name = "نام کامل")]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "تاریخ تولد (شمسی)")]
        public string BirthDateShamsi { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "بیمه")]
        public int InsuranceId { get; set; }

        [Display(Name = "نام بیمه")]
        public string InsuranceName { get; set; }

        [Display(Name = "نام پزشک معالج")]
        public string DoctorName { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ورود")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "تاریخ آخرین ورود (شمسی)")]
        public string LastLoginDateShamsi { get; set; }

        [Display(Name = "تعداد پذیرش‌ها")]
        public int ReceptionCount { get; set; }

        [Display(Name = "مانده بدهی")]
        public decimal DebtBalance { get; set; }

        [Display(Name = "مانده بدهی")]
        public string DebtBalanceFormatted => DebtBalance > 0 ?
            DebtBalance.ToString("N0") + " ریال" : "بدون بدهی";

        // فیلدهای مربوط به سیستم حذف نرم
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedAtShamsi { get; set; }
        public string DeletedByUser { get; set; }
    }
}