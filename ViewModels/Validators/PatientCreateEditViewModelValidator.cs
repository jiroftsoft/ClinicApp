using ClinicApp.ViewModels;
using FluentValidation;
using System;
using System.Text.RegularExpressions;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// Validator اختصاصی برای PatientCreateEditViewModel
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی جامع کد ملی ایرانی
    /// 2. اعتبارسنجی تاریخ تولد
    /// 3. اعتبارسنجی شماره تلفن ایرانی
    /// 4. اعتبارسنجی ایمیل
    /// 5. رعایت استانداردهای پزشکی ایران
    /// </summary>
    public class PatientCreateEditViewModelValidator : AbstractValidator<PatientCreateEditViewModel>
    {
        public PatientCreateEditViewModelValidator()
        {
            // اعتبارسنجی کد ملی
            RuleFor(x => x.NationalCode)
                .NotEmpty()
                .WithMessage("کد ملی الزامی است.")
                .WithErrorCode("REQUIRED_NATIONAL_CODE")
                .Length(10, 10)
                .WithMessage("کد ملی باید ۱۰ رقم باشد.")
                .WithErrorCode("INVALID_NATIONAL_CODE_LENGTH")
                .Must(BeValidNationalCode)
                .WithMessage("کد ملی نامعتبر است.")
                .WithErrorCode("INVALID_NATIONAL_CODE");

            // اعتبارسنجی نام
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("نام الزامی است.")
                .WithErrorCode("REQUIRED_FIRST_NAME")
                .MaximumLength(150)
                .WithMessage("نام نمی‌تواند بیش از ۱۵۰ کاراکتر باشد.")
                .WithErrorCode("FIRST_NAME_TOO_LONG")
                .Matches(@"^[\u0600-\u06FF\s]+$")
                .WithMessage("نام باید فقط شامل حروف فارسی باشد.")
                .WithErrorCode("INVALID_FIRST_NAME_FORMAT");

            // اعتبارسنجی نام خانوادگی
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("نام خانوادگی الزامی است.")
                .WithErrorCode("REQUIRED_LAST_NAME")
                .MaximumLength(150)
                .WithMessage("نام خانوادگی نمی‌تواند بیش از ۱۵۰ کاراکتر باشد.")
                .WithErrorCode("LAST_NAME_TOO_LONG")
                .Matches(@"^[\u0600-\u06FF\s]+$")
                .WithMessage("نام خانوادگی باید فقط شامل حروف فارسی باشد.")
                .WithErrorCode("INVALID_LAST_NAME_FORMAT");

            // اعتبارسنجی تاریخ تولد
            RuleFor(x => x.BirthDate)
                .NotNull()
                .WithMessage("تاریخ تولد الزامی است.")
                .WithErrorCode("REQUIRED_BIRTH_DATE")
                .Must(BeValidBirthDate)
                .WithMessage("تاریخ تولد نامعتبر است.")
                .WithErrorCode("INVALID_BIRTH_DATE");

            // اعتبارسنجی جنسیت
            RuleFor(x => x.Gender)
                .IsInEnum()
                .WithMessage("جنسیت نامعتبر است.")
                .WithErrorCode("INVALID_GENDER");

            // اعتبارسنجی شماره تلفن (اختیاری)
            RuleFor(x => x.PhoneNumber)
                .Must(BeValidPhoneNumber)
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("شماره تلفن نامعتبر است.")
                .WithErrorCode("INVALID_PHONE_NUMBER");

            // اعتبارسنجی ایمیل (اختیاری)
            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("ایمیل نامعتبر است.")
                .WithErrorCode("INVALID_EMAIL")
                .MaximumLength(254)
                .WithMessage("ایمیل نمی‌تواند بیش از ۲۵۴ کاراکتر باشد.")
                .WithErrorCode("EMAIL_TOO_LONG");

            // اعتبارسنجی آدرس (اختیاری)
            RuleFor(x => x.Address)
                .MaximumLength(500)
                .WithMessage("آدرس نمی‌تواند بیش از ۵۰۰ کاراکتر باشد.")
                .WithErrorCode("ADDRESS_TOO_LONG");

            // اعتبارسنجی نام پزشک معالج (اختیاری)
            RuleFor(x => x.DoctorName)
                .MaximumLength(200)
                .WithMessage("نام پزشک نمی‌تواند بیش از ۲۰۰ کاراکتر باشد.")
                .WithErrorCode("DOCTOR_NAME_TOO_LONG")
                .Matches(@"^[\u0600-\u06FF\s]+$")
                .When(x => !string.IsNullOrEmpty(x.DoctorName))
                .WithMessage("نام پزشک باید فقط شامل حروف فارسی باشد.")
                .WithErrorCode("INVALID_DOCTOR_NAME_FORMAT");
        }

        #region Private Validation Methods

        /// <summary>
        /// اعتبارسنجی کد ملی ایرانی
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <returns>true اگر معتبر باشد</returns>
        private bool BeValidNationalCode(string nationalCode)
        {
            if (string.IsNullOrEmpty(nationalCode) || nationalCode.Length != 10)
                return false;

            // بررسی اینکه همه کاراکترها عدد باشند
            if (!Regex.IsMatch(nationalCode, @"^\d{10}$"))
                return false;

            // بررسی کدهای غیرمجاز
            var invalidCodes = new[] { "0000000000", "1111111111", "2222222222", "3333333333", 
                                     "4444444444", "5555555555", "6666666666", "7777777777", 
                                     "8888888888", "9999999999" };
            
            if (Array.Exists(invalidCodes, code => code == nationalCode))
                return false;

            // الگوریتم اعتبارسنجی کد ملی
            var sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(nationalCode[i].ToString()) * (10 - i);
            }

            var remainder = sum % 11;
            var checkDigit = int.Parse(nationalCode[9].ToString());

            if (remainder < 2)
                return checkDigit == remainder;
            else
                return checkDigit == (11 - remainder);
        }

        /// <summary>
        /// اعتبارسنجی تاریخ تولد
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <returns>true اگر معتبر باشد</returns>
        private bool BeValidBirthDate(DateTime? birthDate)
        {
            if (!birthDate.HasValue)
                return false;

            var today = DateTime.Today;
            var age = today.Year - birthDate.Value.Year;

            // بررسی سن منطقی (بین 0 تا 150 سال)
            if (age < 0 || age > 150)
                return false;

            // بررسی اینکه تاریخ تولد در آینده نباشد
            if (birthDate.Value > today)
                return false;

            return true;
        }

        /// <summary>
        /// اعتبارسنجی شماره تلفن ایرانی
        /// </summary>
        /// <param name="phoneNumber">شماره تلفن</param>
        /// <returns>true اگر معتبر باشد</returns>
        private bool BeValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return true; // اختیاری است

            // الگوهای شماره تلفن ایرانی
            var patterns = new[]
            {
                @"^09\d{9}$",           // موبایل: 09123456789
                @"^0\d{10}$",           // ثابت: 02112345678
                @"^\+98\d{10}$"         // بین‌المللی: +989123456789
            };

            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(phoneNumber, pattern))
                    return true;
            }

            return false;
        }

        #endregion
    }
}
