using ClinicApp.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ویومدل مربوط به فرم اولیه ورود شماره موبایل برای ارسال OTP.
    /// </summary>
    public class EnterPhoneNumberViewModel
    {
        /// <summary>
        /// شماره موبایل کاربر.
        /// </summary>
        [Required(ErrorMessage = "وارد کردن شماره موبایل الزامی است.")]
        [Display(Name = "شماره موبایل")]
        [RegularExpression(@"^09[0-9]{9}$", ErrorMessage = "فرمت شماره موبایل نامعتبر است. (مثال: 09123456789)")]
        public string PhoneNumber { get; set; }
    }
}


/// <summary>
/// ویومدل مربوط به فرم تایید کد OTP.
/// </summary>
public class VerifyOtpViewModel
{
    /// <summary>
    /// شماره موبایلی که کد برای آن ارسال شده است.
    /// این فیلد معمولاً به صورت مخفی در فرم قرار می‌گیرد.
    /// </summary>
    [Required]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// کد تایید 6 رقمی وارد شده توسط کاربر.
    /// </summary>
    [Required(ErrorMessage = "وارد کردن کد تایید الزامی است.")]
    [Display(Name = "کد تایید")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "کد تایید باید یک عدد 6 رقمی باشد.")]
    public string OtpCode { get; set; }
}


/// <summary>
    /// ویومدل مربوط به فرم تکمیل اطلاعات و ثبت‌نام نهایی بیمار.
    /// </summary>
public class RegisterPatientViewModel
{
    /// <summary>
            /// شماره موبایل تایید شده از مرحله قبل.
            /// </summary>
    [Required]
    [Display(Name = "شماره همراه")]
    public string PhoneNumber { get; set; }

    /// <summary>
            /// کد ملی بیمار که به عنوان نام کاربری نیز استفاده خواهد شد.
            /// </summary>
    [Required(ErrorMessage = "وارد کردن کد ملی الزامی است.")]
    [Display(Name = "کد ملی")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید یک عدد 10 رقمی باشد.")]
    public string NationalCode { get; set; }

    /// <summary>
            /// نام بیمار.
            /// </summary>
    [Required(ErrorMessage = "وارد کردن نام الزامی است.")]
    [Display(Name = "نام")]
    [StringLength(100, ErrorMessage = "نام نمی‌تواند بیشتر از 100 کاراکتر باشد.")]
    public string FirstName { get; set; }

    /// <summary>
            /// نام خانوادگی بیمار.
            /// </summary>
    [Required(ErrorMessage = "وارد کردن نام خانوادگی الزامی است.")]
    [Display(Name = "نام خانوادگی")]
    [StringLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از 100 کاراکتر باشد.")]
    public string LastName { get; set; }

    /// <summary>
            /// تاریخ تولد بیمار (اختیاری).
            /// </summary>
    public DateTime? BirthDate { get; set; }

    public int InsuranceId { get; set; }

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

    /// <summary>
            /// آدرس محل سکونت بیمار (اختیاری).
            /// </summary>
    [Display(Name = "آدرس")]
    [StringLength(500, ErrorMessage = "آدرس نمی‌تواند بیشتر از 500 کاراکتر باشد.")]
    [DataType(DataType.MultilineText)]
    public string Address { get; set; }
}

