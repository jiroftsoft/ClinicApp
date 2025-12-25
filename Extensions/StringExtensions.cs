using System;
using System.Text.RegularExpressions;

namespace ClinicApp.Extensions
{
    /// <summary>
    /// Extension methods for String operations that are not built-in to C#
    /// متدهای کمکی برای عملیات روی رشته که در C# وجود ندارند
    /// </summary>
    public static class StringExtensions
    {
        #region Truncate

        /// <summary>
        /// Truncates a string to a maximum length and adds a suffix (default: "...")
        /// برش امن رشته با اضافه کردن ... در انتها
        /// </summary>
        /// <param name="str">The string to truncate</param>
        /// <param name="maxLength">Maximum length before truncation</param>
        /// <param name="suffix">Suffix to add when truncated (default: "...")</param>
        /// <returns>Truncated string with suffix if needed</returns>
        /// <example>
        /// string text = "این یک متن خیلی طولانی است";
        /// string result = text.Truncate(10); // "این یک متن..."
        /// </example>
        public static string Truncate(this string str, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length <= maxLength)
                return str;

            return str.Substring(0, maxLength) + suffix;
        }

        #endregion

        #region Mask

        /// <summary>
        /// Masks sensitive data by replacing characters with asterisks, keeping only the last N characters visible
        /// پوشاندن اطلاعات حساس با * و نمایش فقط N کاراکتر آخر
        /// </summary>
        /// <param name="str">The string to mask</param>
        /// <param name="visibleChars">Number of characters to keep visible at the end (default: 4)</param>
        /// <returns>Masked string</returns>
        /// <example>
        /// string creditCard = "1234567890123456";
        /// string masked = creditCard.Mask(4); // "************3456"
        /// </example>
        public static string Mask(this string str, int visibleChars = 4)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length <= visibleChars)
                return str;

            return new string('*', str.Length - visibleChars) + str.Substring(str.Length - visibleChars);
        }

        #endregion

        #region ToSlug

        /// <summary>
        /// Converts a string to a URL-friendly slug
        /// تبدیل رشته به فرمت URL-friendly
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>URL-friendly slug</returns>
        /// <example>
        /// string title = "آموزش ASP.NET MVC #1";
        /// string slug = title.ToSlug(); // "aspnet-mvc-1"
        /// </example>
        public static string ToSlug(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            // Convert to lowercase
            str = str.ToLower().Trim();

            // Remove special characters, keep only alphanumeric and spaces
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // Replace multiple spaces with single space
            str = Regex.Replace(str, @"\s+", " ");

            // Replace spaces with hyphens
            str = str.Replace(" ", "-");

            // Remove multiple consecutive hyphens
            str = Regex.Replace(str, @"-+", "-");

            return str.Trim('-');
        }

        #endregion

        #region HasValue

        /// <summary>
        /// Checks if a string has a value (not null, empty, or whitespace)
        /// بررسی می‌کند که آیا رشته مقدار دارد (null، خالی یا فاصله نباشد)
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <returns>True if string has value, false otherwise</returns>
        /// <example>
        /// string name = "Ali";
        /// if (name.HasValue())
        /// {
        ///     // Process name
        /// }
        /// </example>
        public static bool HasValue(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        #endregion

        #region IsValidEmail

        /// <summary>
        /// Validates if a string is a valid email address
        /// اعتبارسنجی ایمیل
        /// </summary>
        /// <param name="email">The email address to validate</param>
        /// <returns>True if valid email, false otherwise</returns>
        /// <example>
        /// bool isValid = "test@example.com".IsValidEmail(); // true
        /// bool isInvalid = "invalid-email".IsValidEmail(); // false
        /// </example>
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region RemoveSpecialCharacters

        /// <summary>
        /// Removes all special characters from a string, keeping only alphanumeric characters and Persian characters
        /// حذف کاراکترهای خاص و نگه داشتن فقط حروف، اعداد و حروف فارسی
        /// </summary>
        /// <param name="str">The string to process</param>
        /// <returns>String without special characters</returns>
        public static string RemoveSpecialCharacters(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return Regex.Replace(str, @"[^a-zA-Z0-9\u0600-\u06FF\s]", "");
        }

        #endregion

        #region ToTitleCase

        /// <summary>
        /// Converts a string to title case (first letter of each word capitalized)
        /// تبدیل به Title Case
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>Title cased string</returns>
        /// <example>
        /// string text = "hello world";
        /// string result = text.ToTitleCase(); // "Hello World"
        /// </example>
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        #endregion
    }
}
