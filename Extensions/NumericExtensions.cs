using System;
using System.Globalization;

namespace ClinicApp.Extensions
{
    /// <summary>
    /// Extension methods for numeric operations
    /// متدهای کمکی برای عملیات عددی
    /// </summary>
    public static class NumericExtensions
    {
        #region Decimal Extensions

        /// <summary>
        /// Rounds a decimal to specified number of decimals
        /// گرد کردن اعشار
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <param name="decimals">Number of decimal places (default: 2)</param>
        /// <returns>Rounded value</returns>
        public static decimal RoundTo(this decimal value, int decimals = 2)
        {
            return Math.Round(value, decimals);
        }

        /// <summary>
        /// Converts decimal to percentage of a total
        /// تبدیل به درصد
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="total">Total value</param>
        /// <returns>Percentage</returns>
        public static decimal ToPercentage(this decimal value, decimal total)
        {
            return total == 0 ? 0 : (value / total) * 100;
        }

        /// <summary>
        /// Formats decimal as currency in Persian format
        /// فرمت پول به فارسی
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="includeCurrency">Include currency symbol (default: true)</param>
        /// <returns>Formatted currency string</returns>
        /// <example>
        /// decimal price = 1500000;
        /// string formatted = price.ToCurrency(); // "1,500,000 ریال"
        /// </example>
        public static string ToCurrency(this decimal value, bool includeCurrency = true)
        {
            var culture = CultureInfo.GetCultureInfo("fa-IR");
            var formatted = value.ToString("N0", culture);
            return includeCurrency ? $"{formatted} ریال" : formatted;
        }

        /// <summary>
        /// Applies discount percentage to a price
        /// اعمال درصد تخفیف
        /// </summary>
        /// <param name="price">Original price</param>
        /// <param name="discountPercent">Discount percentage</param>
        /// <returns>Price after discount</returns>
        public static decimal ApplyDiscount(this decimal price, decimal discountPercent)
        {
            return price - (price * discountPercent / 100);
        }

        #endregion

        #region Integer Extensions

        /// <summary>
        /// Checks if an integer is between two values (inclusive)
        /// بررسی قرار گرفتن عدد بین دو مقدار
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>True if value is between min and max</returns>
        public static bool IsBetween(this int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Checks if a decimal is between two values (inclusive)
        /// بررسی قرار گرفتن عدد اعشاری بین دو مقدار
        /// </summary>
        public static bool IsBetween(this decimal value, decimal min, decimal max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Formats integer with thousand separators
        /// فرمت عدد با جداکننده هزارگان
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <returns>Formatted string</returns>
        public static string ToFormattedString(this int value)
        {
            return value.ToString("N0", CultureInfo.GetCultureInfo("fa-IR"));
        }

        /// <summary>
        /// Formats long with thousand separators
        /// فرمت عدد بزرگ با جداکننده هزارگان
        /// </summary>
        public static string ToFormattedString(this long value)
        {
            return value.ToString("N0", CultureInfo.GetCultureInfo("fa-IR"));
        }

        #endregion

        #region File Size

        /// <summary>
        /// Converts bytes to human-readable file size (B, KB, MB, GB, TB)
        /// تبدیل بایت به واحدهای قابل خواندن
        /// </summary>
        /// <param name="bytes">Size in bytes</param>
        /// <returns>Formatted file size</returns>
        /// <example>
        /// long fileSize = 1536000;
        /// string readable = fileSize.ToFileSize(); // "1.46 MB"
        /// </example>
        public static string ToFileSize(this long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Converts bytes to human-readable file size (int overload)
        /// تبدیل بایت به واحدهای قابل خواندن
        /// </summary>
        public static string ToFileSize(this int bytes)
        {
            return ((long)bytes).ToFileSize();
        }

        #endregion

        #region Boolean Extensions

        /// <summary>
        /// Converts boolean to Persian yes/no string
        /// تبدیل بولین به بله/خیر
        /// </summary>
        /// <param name="value">Boolean value</param>
        /// <returns>"بله" or "خیر"</returns>
        public static string ToPersianYesNo(this bool value)
        {
            return value ? "بله" : "خیر";
        }

        /// <summary>
        /// Converts boolean to active/inactive string
        /// تبدیل بولین به فعال/غیرفعال
        /// </summary>
        public static string ToActiveInactive(this bool value)
        {
            return value ? "فعال" : "غیرفعال";
        }

        #endregion
    }
}
