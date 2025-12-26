using System;
using System.Linq;
using FluentValidation;
using ClinicApp.Helpers;
using ClinicApp.Controllers.Api;

namespace ClinicApp.Models.Validators
{
    /// <summary>
    /// 🏥 اعتبارسنجی قدرتمند برای ایجاد سریع بیمار
    /// 
    /// این Validator تمام قوانین اعتبارسنجی را برای Patient Quick Create پیاده‌سازی می‌کند:
    /// ✅ کد ملی ایرانی (الگوریتم استاندارد)
    /// ✅ شماره موبایل ایرانی (09XXXXXXXXX)
    /// ✅ نام و نام خانوادگی (فقط حروف فارسی/انگلیسی)
    /// ✅ تاریخ تولد شمسی
    /// ✅ پیام‌های خطای کاربرپسند
    /// 
    /// استفاده:
    /// var validator = new PatientFastCreateValidator();
    /// var result = await validator.ValidateAsync(dto);
    /// 
    /// @version 1.0.0
    /// @date 1404/10/05
    /// </summary>
    public class PatientFastCreateValidator : AbstractValidator<PatientQuickCreateDto>
    {
        public PatientFastCreateValidator()
        {
            // =====================================================
            // 🆔 کد ملی - Iranian National Code
            // =====================================================
            
            RuleFor(x => x.NationalCode)
                .NotEmpty()
                .WithMessage("کد ملی الزامی است")
                .Length(10)
                .WithMessage("کد ملی باید دقیقاً 10 رقم باشد")
                .Must(BeNumericOnly)
                .WithMessage("کد ملی فقط باید شامل اعداد باشد")
                .Must(BeValidNationalCode)
                .WithMessage("کد ملی نامعتبر است (رقم کنترل اشتباه)");

            // =====================================================
            // 👤 نام - First Name
            // =====================================================
            
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("نام الزامی است")
                .MinimumLength(2)
                .WithMessage("نام باید حداقل 2 کاراکتر باشد")
                .MaximumLength(50)
                .WithMessage("نام نباید بیش از 50 کاراکتر باشد")
                .Must(BeValidName)
                .WithMessage("نام فقط باید شامل حروف فارسی یا انگلیسی باشد");

            // =====================================================
            // 👤 نام خانوادگی - Last Name
            // =====================================================
            
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("نام خانوادگی الزامی است")
                .MinimumLength(2)
                .WithMessage("نام خانوادگی باید حداقل 2 کاراکتر باشد")
                .MaximumLength(50)
                .WithMessage("نام خانوادگی نباید بیش از 50 کاراکتر باشد")
                .Must(BeValidName)
                .WithMessage("نام خانوادگی فقط باید شامل حروف فارسی یا انگلیسی باشد");

            // =====================================================
            // 👨 نام پدر - Father Name (اختیاری)
            // =====================================================
            
            RuleFor(x => x.FatherName)
                .MaximumLength(50)
                .WithMessage("نام پدر نباید بیش از 50 کاراکتر باشد")
                .Must(BeValidName)
                .When(x => !string.IsNullOrWhiteSpace(x.FatherName))
                .WithMessage("نام پدر فقط باید شامل حروف فارسی یا انگلیسی باشد");

            // =====================================================
            // 📱 موبایل - Mobile Number
            // =====================================================
            
            RuleFor(x => x.Mobile)
                .NotEmpty()
                .WithMessage("شماره موبایل الزامی است")
                .Must(BeValidIranianMobile)
                .WithMessage("شماره موبایل باید با 09 شروع شود و 11 رقم باشد");

            // =====================================================
            // 📅 تاریخ تولد - Birth Date (اختیاری)
            // =====================================================
            
            RuleFor(x => x.BirthDateShamsi)
                .Must(BeValidPersianDate)
                .When(x => !string.IsNullOrWhiteSpace(x.BirthDateShamsi))
                .WithMessage("تاریخ تولد نامعتبر است")
                .Must(BeReasonableAge)
                .When(x => !string.IsNullOrWhiteSpace(x.BirthDateShamsi))
                .WithMessage("تاریخ تولد باید بین 0 تا 150 سال باشد");

            // =====================================================
            // 📍 آدرس - Address (اختیاری)
            // =====================================================
            
            RuleFor(x => x.Address)
                .MaximumLength(500)
                .WithMessage("آدرس نباید بیش از 500 کاراکتر باشد");

            // =====================================================
            // ⚥ جنسیت - Gender (اختیاری)
            // =====================================================
            
            RuleFor(x => x.Gender)
                .Must(BeValidGender)
                .When(x => !string.IsNullOrWhiteSpace(x.Gender))
                .WithMessage("جنسیت باید 'مرد' یا 'زن' باشد");
        }

        // =====================================================
        // 🔧 VALIDATION METHODS - متدهای اعتبارسنجی
        // =====================================================

        /// <summary>
        /// بررسی عددی بودن رشته
        /// </summary>
        private bool BeNumericOnly(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return value.All(char.IsDigit);
        }

        /// <summary>
        /// اعتبارسنجی کد ملی ایرانی
        /// استفاده از Helper موجود در پروژه
        /// </summary>
        private bool BeValidNationalCode(string nationalCode)
        {
            if (string.IsNullOrWhiteSpace(nationalCode)) return false;
            
            // استفاده از IranianNationalCodeValidator موجود
            return IranianNationalCodeValidator.IsValid(nationalCode);
        }

        /// <summary>
        /// اعتبارسنجی نام (فقط حروف فارسی/انگلیسی و فاصله)
        /// </summary>
        private bool BeValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            
            // فقط حروف فارسی (آ-ی)، انگلیسی (a-z, A-Z) و فاصله
            // Unicode Range برای فارسی: \u0600-\u06FF
            return System.Text.RegularExpressions.Regex.IsMatch(
                name, 
                @"^[\u0600-\u06FFa-zA-Z\s]+$"
            );
        }

        /// <summary>
        /// اعتبارسنجی شماره موبایل ایرانی
        /// فرمت: 09XXXXXXXXX
        /// </summary>
        private bool BeValidIranianMobile(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile)) return false;
            
            // حذف فاصله و خط فاصله
            mobile = mobile.Replace(" ", "").Replace("-", "");
            
            // بررسی فرمت
            if (!System.Text.RegularExpressions.Regex.IsMatch(mobile, @"^09\d{9}$"))
            {
                return false;
            }
            
            // بررسی کد اپراتور
            var operatorCode = mobile.Substring(2, 2);
            var validOperators = new[] 
            { 
                "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
                "20", "21", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
                "90", "91", "92", "93", "94", "95", "96", "97", "98", "99"
            };
            
            return validOperators.Contains(operatorCode);
        }

        /// <summary>
        /// اعتبارسنجی تاریخ شمسی
        /// </summary>
        private bool BeValidPersianDate(string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate)) return true; // اختیاری
            
            try
            {
                // استفاده از PersianDateHelper موجود
                var gregorianDate = PersianDateHelper.ParsePersianDate(persianDate);
                return gregorianDate.HasValue;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// بررسی محدوده سنی معقول (0-150 سال)
        /// </summary>
        private bool BeReasonableAge(string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate)) return true;
            
            try
            {
                var gregorianDateNullable = PersianDateHelper.ParsePersianDate(persianDate);
                if (!gregorianDateNullable.HasValue)
                {
                    return false;
                }
                
                var gregorianDate = gregorianDateNullable.Value;
                var age = DateTime.Now.Year - gregorianDate.Year;
                if (gregorianDate.Date > DateTime.Now.AddYears(-age))
                {
                    age--;
                }
                
                return age >= 0 && age <= 150;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// اعتبارسنجی جنسیت
        /// </summary>
        private bool BeValidGender(string gender)
        {
            if (string.IsNullOrWhiteSpace(gender)) return true; // اختیاری
            
            var validGenders = new[] { "مرد", "زن", "Male", "Female", "M", "F" };
            return validGenders.Contains(gender, StringComparer.OrdinalIgnoreCase);
        }
    }
}

