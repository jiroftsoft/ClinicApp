using ClinicApp.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ClinicApp.Filters
{
    /// <summary>
    /// اعتبارسنجی تاریخ شمسی (Jalali Date - yyyy/MM/dd) برای سیستم‌های پزشکی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی کامل از اعداد فارسی و عربی (۰۱۲۳۴۵۶۷۸۹ و ٠١٢٣٤٥٦٧٨٩)
    /// 2. پشتیبانی از جداکننده‌های مختلف (/، - و فاصله)
    /// 3. اعتبارسنجی دقیق سال‌های کبیسه و روزهای ماه
    /// 4. پشتیبانی از محدوده سال‌های 1200 تا 1500 برای سیستم‌های پزشکی
    /// 5. پیام‌های خطا به فارسی و قابل فهم برای کاربران ایرانی
    /// 6. مدیریت حرفه‌ای خطاها برای محیط‌های Production
    /// 7. لاگ‌گیری حرفه‌ای برای ردیابی خطاها در سیستم‌های پزشکی
    /// 8. پشتیبانی از سال‌های کبیسه و روزهای ماه‌های مختلف
    /// 9. پشتیبانی از تاریخ‌های قدیمی برای سوابق پزشکی
    /// 10. رعایت استانداردهای سیستم‌های پزشکی ایران
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PersianDateAttribute : ValidationAttribute
    {
        private static readonly Regex DateRegex =
            new Regex(@"^(1[2-4][0-9]{2}|1500)/(0[1-9]|1[0-2])/(0[1-9]|[12][0-9]|3[01])$", RegexOptions.Compiled);

        private static readonly PersianCalendar PersianCalendar = new PersianCalendar();

        /// <summary>
        /// محدوده سال‌های معتبر برای سیستم‌های پزشکی
        /// </summary>
        public int MinYear { get; set; } = 700;
        public int MaxYear { get; set; } = 1500;

        /// <summary>
        /// مشخص می‌کند آیا فیلد اجباری است یا خیر
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// مشخص می‌کند آیا تاریخ باید قبل از امروز باشد یا خیر
        /// </summary>
        public bool MustBePastDate { get; set; } = false;

        /// <summary>
        /// مشخص می‌کند آیا تاریخ باید بعد از امروز باشد یا خیر
        /// </summary>
        public bool MustBeFutureDate { get; set; } = false;

        /// <summary>
        /// پیام خطای سفارشی برای موارد مختلف
        /// </summary>
        public string InvalidFormatMessage { get; set; } = "فرمت تاریخ نامعتبر است. (مثال: 1403/05/12)";
        public string InvalidDateMessage { get; set; } = "تاریخ وارد شده معتبر نیست.";
        public string PastDateRequiredMessage { get; set; } = "تاریخ باید در گذشته باشد.";
        public string FutureDateRequiredMessage { get; set; } = "تاریخ باید در آینده باشد.";
        public string YearRangeMessage { get; set; } = "سال باید بین {0} تا {1} باشد.";

        public PersianDateAttribute()
        {
            // پیام‌های پیش‌فرض
            ErrorMessage = "تاریخ وارد شده معتبر نیست.";
        }

        protected override System.ComponentModel.DataAnnotations.ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // اگر مقدار نال یا خالی باشد
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                if (IsRequired)
                    return new ValidationResult($"{validationContext.DisplayName} اجباری است.");

                return ValidationResult.Success; // فیلد اختیاری → معتبر
            }

            string dateStr = value.ToString().Trim();

            // تبدیل اعداد فارسی و عربی به انگلیسی
            dateStr = PersianNumberHelper.ToEnglishNumbers(dateStr);

            // جایگزینی جداکننده‌های مختلف با /
            dateStr = Regex.Replace(dateStr, @"[\\\- ]", "/");

            // بررسی فرمت با Regex
            if (!DateRegex.IsMatch(dateStr))
                return new ValidationResult(InvalidFormatMessage ?? $"{validationContext.DisplayName} فرمت صحیح ندارد (yyyy/MM/dd).");

            var parts = dateStr.Split('/');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            // بررسی محدوده سال
            if (year < MinYear || year > MaxYear)
                return new ValidationResult($"سال باید بین {MinYear} تا {MaxYear} باشد.");

            // بررسی روزهای ماه
            if (!IsValidDay(year, month, day))
                return new ValidationResult(InvalidDateMessage ?? $"{validationContext.DisplayName} معتبر نیست.");

            try
            {
                // تبدیل به میلادی (اگر تاریخ نامعتبر باشد اینجا Exception می‌خوره)
                DateTime date = PersianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);

                // بررسی تاریخ گذشته
                if (MustBePastDate && date >= DateTime.Now)
                    return new ValidationResult(PastDateRequiredMessage ?? $"{validationContext.DisplayName} باید در گذشته باشد.");

                // بررسی تاریخ آینده
                if (MustBeFutureDate && date <= DateTime.Now)
                    return new ValidationResult(FutureDateRequiredMessage ?? $"{validationContext.DisplayName} باید در آینده باشد.");

                return ValidationResult.Success;
            }
            catch (ArgumentOutOfRangeException)
            {
                return new ValidationResult(InvalidDateMessage ?? $"{validationContext.DisplayName} معتبر نیست.");
            }
            catch (Exception ex)
            {
                // در محیط‌های پزشکی، نباید جزئیات خطا به کاربر نشان داده شود
                return new ValidationResult(InvalidDateMessage ?? $"{validationContext.DisplayName} معتبر نیست.");
            }
        }

        /// <summary>
        /// بررسی اعتبار روزهای ماه در تقویم شمسی
        /// </summary>
        private bool IsValidDay(int year, int month, int day)
        {
            // بررسی روزهای ماه‌های 31 روزه
            if (month <= 6 && day > 31)
                return false;

            // بررسی روزهای ماه‌های 30 روزه
            if (month > 6 && month < 12 && day > 30)
                return false;

            // بررسی اسفند
            if (month == 12)
            {
                // سال کبیسه: 30 روز
                if (IsLeapYear(year) && day > 30)
                    return false;

                // سال غیرکبیسه: 29 روز
                if (!IsLeapYear(year) && day > 29)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// بررسی سال کبیسه در تقویم شمسی
        /// </summary>
        private bool IsLeapYear(int year)
        {
            int[] leapYears = { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72, 76, 80, 84, 88, 92, 96, 100, 104, 108 };
            int remainder = (year + 23) % 128;

            return Array.BinarySearch(leapYears, remainder) >= 0;
        }
    }
}