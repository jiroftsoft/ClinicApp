using System;
using System.Globalization;
using ClinicApp.Extensions;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper کلاس برای مدیریت Culture در پروژه
    /// این کلاس مشکلات Culture در Decimal Parsing و Number Formatting را حل می‌کند
    /// </summary>
    public static class CultureHelper
    {
        /// <summary>
        /// Culture ثابت برای محاسبات و ذخیره‌سازی
        /// همیشه از "." به عنوان جداکننده اعشار استفاده می‌کند
        /// </summary>
        public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Culture فارسی برای نمایش
        /// از "," به عنوان جداکننده هزارگان و "." به عنوان جداکننده اعشار استفاده می‌کند
        /// </summary>
        public static readonly CultureInfo PersianCulture = new CultureInfo("fa-IR");

        /// <summary>
        /// تبدیل رشته به Decimal با استفاده از InvariantCulture
        /// این متد مشکل Culture در Decimal Parsing را حل می‌کند
        /// </summary>
        /// <param name="value">رشته ورودی</param>
        /// <param name="defaultValue">مقدار پیش‌فرض در صورت خطا</param>
        /// <returns>Decimal یا مقدار پیش‌فرض</returns>
        public static decimal ParseDecimal(string value, decimal defaultValue = 0m)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            try
            {
                // اول سعی کن با InvariantCulture
                return decimal.Parse(value.Trim(), InvariantCulture);
            }
            catch (FormatException)
            {
                try
                {
                    // اگر شکست خورد، سعی کن با CurrentCulture
                    return decimal.Parse(value.Trim(), CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    // اگر هر دو شکست خوردند، سعی کن با Replace کردن جداکننده‌ها
                    var normalizedValue = value.Trim()
                        .Replace(",", ".")  // جایگزینی کاما با نقطه
                        .Replace("٫", ".")  // جایگزینی جداکننده فارسی با نقطه
                        .Replace("٬", "."); // جایگزینی جداکننده فارسی دیگر با نقطه

                    return decimal.Parse(normalizedValue, InvariantCulture);
                }
            }
            catch (OverflowException)
            {
                return defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// تبدیل رشته به Decimal? با استفاده از InvariantCulture
        /// </summary>
        /// <param name="value">رشته ورودی</param>
        /// <returns>Decimal? یا null</returns>
        public static decimal? ParseDecimalNullable(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            try
            {
                return ParseDecimal(value);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// تبدیل Decimal به رشته با فرمت فارسی
        /// </summary>
        /// <param name="value">مقدار Decimal</param>
        /// <param name="format">فرمت (پیش‌فرض: N0)</param>
        /// <returns>رشته فرمت شده</returns>
        public static string FormatDecimal(decimal value, string format = "N0")
        {
            return value.ToString(format, PersianCulture);
        }

        /// <summary>
        /// تبدیل Decimal به رشته با فرمت فارسی (با پشتیبانی از null)
        /// </summary>
        /// <param name="value">مقدار Decimal?</param>
        /// <param name="format">فرمت (پیش‌فرض: N0)</param>
        /// <returns>رشته فرمت شده یا "-"</returns>
        public static string FormatDecimal(decimal? value, string format = "N0")
        {
            return value?.ToString(format, PersianCulture) ?? "-";
        }

        /// <summary>
        /// تبدیل Decimal به رشته برای ذخیره‌سازی در دیتابیس
        /// همیشه از "." به عنوان جداکننده اعشار استفاده می‌کند
        /// </summary>
        /// <param name="value">مقدار Decimal</param>
        /// <param name="format">فرمت (پیش‌فرض: F6)</param>
        /// <returns>رشته برای ذخیره‌سازی</returns>
        public static string FormatDecimalForDatabase(decimal value, string format = "F6")
        {
            return value.ToString(format, InvariantCulture);
        }

        /// <summary>
        /// تبدیل Decimal? به رشته برای ذخیره‌سازی در دیتابیس
        /// </summary>
        /// <param name="value">مقدار Decimal?</param>
        /// <param name="format">فرمت (پیش‌فرض: F6)</param>
        /// <returns>رشته برای ذخیره‌سازی یا null</returns>
        public static string FormatDecimalForDatabase(decimal? value, string format = "F6")
        {
            return value?.ToString(format, InvariantCulture);
        }

        /// <summary>
        /// بررسی صحت مقدار Decimal
        /// </summary>
        /// <param name="value">رشته ورودی</param>
        /// <returns>true اگر معتبر باشد</returns>
        public static bool IsValidDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                ParseDecimal(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// تبدیل رشته تاریخ فارسی به DateTime
        /// </summary>
        /// <param name="persianDate">تاریخ فارسی (مثل: 1404/01/01)</param>
        /// <returns>DateTime یا null</returns>
        public static DateTime? ParsePersianDate(string persianDate)
        {
            if (string.IsNullOrEmpty(persianDate))
                return null;

            try
            {
                // استفاده از PersianDateExtensions
                return persianDate.ToDateTime();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// تبدیل DateTime به رشته تاریخ فارسی
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>رشته تاریخ فارسی</returns>
        public static string FormatPersianDate(DateTime date)
        {
            return date.ToPersianDateString();
        }

        /// <summary>
        /// تبدیل DateTime? به رشته تاریخ فارسی
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>رشته تاریخ فارسی یا "-"</returns>
        public static string FormatPersianDate(DateTime? date)
        {
            return date?.ToPersianDateString() ?? "-";
        }
    }
}
