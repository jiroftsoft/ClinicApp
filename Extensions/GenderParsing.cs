using System;
using System.Globalization;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Extensions
{
    /// <summary>
    /// تبدیل رشته به enum جنسیت با پشتیبانی از ورودی‌های فارسی و انگلیسی
    /// </summary>
    public static class GenderParsing
    {
        private static readonly ILogger _logger = Log.ForContext("SourceContext", "GenderParsing");

        /// <summary>
        /// تلاش برای تبدیل رشته به enum جنسیت
        /// </summary>
        /// <param name="input">ورودی رشته</param>
        /// <param name="result">نتیجه تبدیل</param>
        /// <returns>true اگر تبدیل موفق باشد</returns>
        public static bool TryParse(string input, out Gender result)
        {
            result = Gender.Male; // مقدار پیش‌فرض

            if (string.IsNullOrWhiteSpace(input))
            {
                _logger.Warning("ورودی جنسیت خالی است");
                return false;
            }

            var normalizedInput = input.Trim().ToLowerInvariant();

            // نگاشت‌های فارسی
            if (normalizedInput == "مرد" || normalizedInput == "male" || normalizedInput == "m" || normalizedInput == "1")
            {
                result = Gender.Male;
                return true;
            }

            // نگاشت‌های فارسی
            if (normalizedInput == "زن" || normalizedInput == "female" || normalizedInput == "f" || normalizedInput == "2")
            {
                result = Gender.Female;
                return true;
            }

            _logger.Warning("ورودی جنسیت نامعتبر: {Input}", input);
            return false;
        }

        /// <summary>
        /// تبدیل رشته به enum جنسیت با مقدار پیش‌فرض
        /// </summary>
        /// <param name="input">ورودی رشته</param>
        /// <param name="fallback">مقدار پیش‌فرض در صورت عدم موفقیت</param>
        /// <returns>enum جنسیت</returns>
        public static Gender ParseOrDefault(string input, Gender fallback = Gender.Male)
        {
            if (TryParse(input, out var result))
            {
                return result;
            }

            _logger.Warning("ورودی جنسیت نامعتبر، استفاده از مقدار پیش‌فرض: {Input} -> {Fallback}", input, fallback);
            return fallback;
        }

        /// <summary>
        /// تبدیل enum جنسیت به رشته فارسی
        /// </summary>
        /// <param name="gender">enum جنسیت</param>
        /// <returns>رشته فارسی</returns>
        public static string ToPersianString(Gender gender)
        {
            return gender switch
            {
                Gender.Male => "مرد",
                Gender.Female => "زن",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// تبدیل enum جنسیت به رشته انگلیسی
        /// </summary>
        /// <param name="gender">enum جنسیت</param>
        /// <returns>رشته انگلیسی</returns>
        public static string ToEnglishString(Gender gender)
        {
            return gender switch
            {
                Gender.Male => "Male",
                Gender.Female => "Female",
                _ => "Unknown"
            };
        }
    }
}
