using System;
using System.Text.RegularExpressions;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای عملیات روی رشته‌ها
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// حذف تمام تگ‌های HTML از رشته
        /// </summary>
        /// <param name="html">رشته حاوی HTML</param>
        /// <returns>رشته بدون تگ‌های HTML</returns>
        public static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // حذف تگ‌های HTML
            var stripped = Regex.Replace(html, "<.*?>", string.Empty);
            
            // حذف entity های HTML
            stripped = System.Web.HttpUtility.HtmlDecode(stripped);
            
            // حذف فضاهای خالی اضافی
            stripped = Regex.Replace(stripped, @"\s+", " ");
            
            return stripped.Trim();
        }

        /// <summary>
        /// کوتاه کردن متن با حذف HTML
        /// </summary>
        /// <param name="html">رشته حاوی HTML</param>
        /// <param name="maxLength">حداکثر طول</param>
        /// <param name="suffix">پسوند (مثلاً "...")</param>
        /// <returns>متن کوتاه شده</returns>
        public static string StripHtmlAndTruncate(string html, int maxLength, string suffix = "...")
        {
            var stripped = StripHtml(html);
            
            if (stripped.Length <= maxLength)
                return stripped;
            
            return stripped.Substring(0, maxLength) + suffix;
        }
    }
}

