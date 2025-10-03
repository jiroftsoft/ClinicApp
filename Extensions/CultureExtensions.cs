using System;
using System.Globalization;

namespace ClinicApp.Extensions
{
    /// <summary>
    /// Extension Methods برای مدیریت Culture
    /// این کلاس مشکلات Culture در Decimal Parsing را حل می‌کند
    /// </summary>
    public static class CultureExtensions
    {
        /// <summary>
        /// تبدیل رشته به Decimal با استفاده از InvariantCulture
        /// </summary>
        /// <param name="value">رشته ورودی</param>
        /// <param name="defaultValue">مقدار پیش‌فرض در صورت خطا</param>
        /// <returns>Decimal یا مقدار پیش‌فرض</returns>
        public static decimal ToDecimal(this string value, decimal defaultValue = 0m)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            try
            {
                return decimal.Parse(value.Trim(), CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                try
                {
                    return decimal.Parse(value.Trim(), CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    var normalizedValue = value.Trim()
                        .Replace(",", ".")  // جایگزینی کاما با نقطه
                        .Replace("٫", ".")  // جایگزینی جداکننده فارسی با نقطه
                        .Replace("٬", "."); // جایگزینی جداکننده فارسی دیگر با نقطه

                    return decimal.Parse(normalizedValue, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// تبدیل رشته به Decimal? با استفاده از InvariantCulture
        /// </summary>
        /// <param name="value">رشته ورودی</param>
        /// <returns>Decimal? یا null</returns>
        public static decimal? ToDecimalNullable(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            try
            {
                return value.ToDecimal();
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
        public static string ToPersianString(this decimal value, string format = "N0")
        {
            return value.ToString(format, new CultureInfo("fa-IR"));
        }

        /// <summary>
        /// تبدیل Decimal? به رشته با فرمت فارسی
        /// </summary>
        /// <param name="value">مقدار Decimal?</param>
        /// <param name="format">فرمت (پیش‌فرض: N0)</param>
        /// <returns>رشته فرمت شده یا "-"</returns>
        public static string ToPersianString(this decimal? value, string format = "N0")
        {
            return value?.ToString(format, new CultureInfo("fa-IR")) ?? "-";
        }

        /// <summary>
        /// تبدیل Decimal به رشته برای ذخیره‌سازی در دیتابیس
        /// </summary>
        /// <param name="value">مقدار Decimal</param>
        /// <param name="format">فرمت (پیش‌فرض: F6)</param>
        /// <returns>رشته برای ذخیره‌سازی</returns>
        public static string ToDatabaseString(this decimal value, string format = "F6")
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// تبدیل Decimal? به رشته برای ذخیره‌سازی در دیتابیس
        /// </summary>
        /// <param name="value">مقدار Decimal?</param>
        /// <param name="format">فرمت (پیش‌فرض: F6)</param>
        /// <returns>رشته برای ذخیره‌سازی یا null</returns>
        public static string ToDatabaseString(this decimal? value, string format = "F6")
        {
            return value?.ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// بررسی صحت مقدار Decimal
        /// </summary>
        /// <param name="value">رشته ورودی</param>
        /// <returns>true اگر معتبر باشد</returns>
        public static bool IsValidDecimal(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                value.ToDecimal();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
