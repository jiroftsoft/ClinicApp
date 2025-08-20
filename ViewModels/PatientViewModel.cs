using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;

namespace ClinicApp.ViewModels
{
    public class PatientViewModel
    {
        public int PatientId { get; set; }

        [Display(Name = "کد ملی")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "{0} باید ۱۰ رقم باشد.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "در فیلد {0} فقط عدد مجاز است.")]
        public string NationalCode { get; set; }

        [Display(Name = "نام و نام خانوادگی")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [MaxLength(250, ErrorMessage = "حداکثر طول {0}، {1} کاراکتر است.")]
        public string FullName { get; set; }

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [RegularExpression("09[0-9]{9}", ErrorMessage = "فرمت {0} صحیح نیست.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "تاریخ تولد")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "آدرس")]
        [DataType(DataType.MultilineText)]
        [MaxLength(500)]
        public string Address { get; set; }

        [Display(Name = "بیمه")]
        public string InsuranceName { get; set; }
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
        [RegularExpression("^[0-9]*$", ErrorMessage = "در فیلد {0} فقط عدد مجاز است.")]
        public string NationalCode { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(150)]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(150)]
        public string LastName { get; set; }

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [RegularExpression("09[0-9]{9}", ErrorMessage = "فرمت {0} صحیح نیست.")]
        public string PhoneNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        [Display(Name = "تاریخ تولد")]
        public string BirthDatePersian
        {
            get
            {
                if (BirthDate.HasValue)
                {
                    var pc = new PersianCalendar();
                    return $"{pc.GetYear(BirthDate.Value)}/{pc.GetMonth(BirthDate.Value):D2}/{pc.GetDayOfMonth(BirthDate.Value):D2}";
                }
                return string.Empty; // بهتر است رشته خالی برگردانیم تا null
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // *** مرحله کلیدی: تبدیل اعداد فارسی به انگلیسی ***
                    var englishValue = PersianNumberHelper.ToEnglishNumbers(value);

                    try
                    {
                        var parts = englishValue.Split('/');
                        if (parts.Length == 3)
                        {
                            int year = int.Parse(parts[0]);
                            int month = int.Parse(parts[1]);
                            int day = int.Parse(parts[2]);
                            BirthDate = new PersianCalendar().ToDateTime(year, month, day, 0, 0, 0, 0);
                        }
                    }
                    catch
                    {
                        BirthDate = null; // در صورت فرمت نامعتبر
                    }
                }
                else
                {
                    BirthDate = null;
                }
            }
        }

        [Display(Name = "آدرس")]
        [DataType(DataType.MultilineText)]
        [MaxLength(500)]
        public string Address { get; set; }

        [Display(Name = "بیمه")]
        [Required(ErrorMessage = "انتخاب {0} الزامی است.")]
        public int InsuranceId { get; set; }

        [Display(Name = "نام بیمه")]
        public string InsuranceName { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] // اضافه شد
        public IEnumerable<SelectListItem> Insurances { get; set; } // اضافه شد

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedByUser { get; set; }

        // فیلدهای مربوط به تاریخ شمسی
        [Display(Name = "تاریخ ایجاد")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public string UpdatedAtShamsi { get; set; }
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

        [Display(Name = "شماره موبایل")]
        public string PhoneNumber { get; set; }

        [Display(Name = "بیمه")]
        public string InsuranceName { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "مانده بدهی")]
        public string DebtBalanceFormatted => DebtBalance > 0 ? DebtBalance.ToString("N0") + " ریال" : "بدون بدهی";

        public decimal DebtBalance { get; set; }
    }

    /// <summary>
    /// مدل ویو برای جزئیات بیمار - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش کامل اطلاعات بیمار
    /// 2. پشتیبانی از محیط‌های ایرانی با تاریخ شمسی
    /// 3. رعایت استانداردهای سیستم‌های پزشکی
    /// 4. امنیت بالا با عدم نمایش اطلاعات حساس
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

        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        [Display(Name = "بیمه")]
        public int InsuranceId { get; set; }

        [Display(Name = "نام بیمه")]
        public string InsuranceName { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ورود")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "تعداد پذیرش‌ها")]
        public int ReceptionCount { get; set; }

        [Display(Name = "مانده بدهی")]
        public decimal DebtBalance { get; set; }

        // فیلدهای مربوط به تاریخ شمسی
        [Display(Name = "تاریخ ایجاد")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public string UpdatedAtShamsi { get; set; }

        public string FullName { get; set; }
    }
}