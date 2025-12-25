using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper class for HTML operations
    /// کلاس کمکی برای عملیات HTML
    /// </summary>
    public static class HtmlHelper
    {
        #region HTML Stripping

        /// <summary>
        /// Removes all HTML tags from a string
        /// حذف تمام تگ‌های HTML
        /// </summary>
        /// <param name="html">HTML string</param>
        /// <returns>Plain text</returns>
        /// <example>
        /// string html = "&lt;p&gt;سلام &lt;b&gt;دنیا&lt;/b&gt;&lt;/p&gt;";
        /// string text = HtmlHelper.StripHtml(html); // "سلام دنیا"
        /// </example>
        public static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            return Regex.Replace(html, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Removes specific HTML tags
        /// حذف تگ‌های خاص
        /// </summary>
        public static string RemoveTags(string html, params string[] tags)
        {
            if (string.IsNullOrWhiteSpace(html) || tags == null || tags.Length == 0)
                return html;

            foreach (var tag in tags)
            {
                var pattern = $"<{tag}[^>]*>.*?</{tag}>";
                html = Regex.Replace(html, pattern, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }

            return html;
        }

        #endregion

        #region HTML Encoding/Decoding

        /// <summary>
        /// Converts text to HTML (encodes and replaces newlines)
        /// تبدیل متن به HTML
        /// </summary>
        public static string TextToHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;")
                .Replace("\n", "<br/>");
        }

        /// <summary>
        /// HTML encodes a string
        /// رمزنگاری HTML
        /// </summary>
        public static string Encode(string text)
        {
            return HttpUtility.HtmlEncode(text);
        }

        /// <summary>
        /// HTML decodes a string
        /// رمزگشایی HTML
        /// </summary>
        public static string Decode(string html)
        {
            return HttpUtility.HtmlDecode(html);
        }

        #endregion

        #region HTML Building

        /// <summary>
        /// Builds an anchor tag
        /// ساخت تگ لینک
        /// </summary>
        public static string BuildLink(string url, string text, string cssClass = null, string target = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                return text;

            var classAttr = !string.IsNullOrWhiteSpace(cssClass) ? $" class=\"{cssClass}\"" : "";
            var targetAttr = !string.IsNullOrWhiteSpace(target) ? $" target=\"{target}\"" : "";

            return $"<a href=\"{url}\"{classAttr}{targetAttr}>{text}</a>";
        }

        /// <summary>
        /// Builds an image tag
        /// ساخت تگ تصویر
        /// </summary>
        public static string BuildImage(string src, string alt = "", string cssClass = null, int? width = null, int? height = null)
        {
            if (string.IsNullOrWhiteSpace(src))
                return string.Empty;

            var classAttr = !string.IsNullOrWhiteSpace(cssClass) ? $" class=\"{cssClass}\"" : "";
            var widthAttr = width.HasValue ? $" width=\"{width.Value}\"" : "";
            var heightAttr = height.HasValue ? $" height=\"{height.Value}\"" : "";
            var altAttr = $" alt=\"{alt ?? ""}\"";

            return $"<img src=\"{src}\"{altAttr}{classAttr}{widthAttr}{heightAttr} />";
        }

        #endregion

        #region Text Truncation with HTML

        /// <summary>
        /// Truncates HTML while preserving tags
        /// برش HTML با حفظ تگ‌ها
        /// </summary>
        public static string TruncateHtml(string html, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(html))
                return html;

            var plainText = StripHtml(html);
            if (plainText.Length <= maxLength)
                return html;

            var truncatedText = plainText.Substring(0, maxLength);
            return truncatedText + suffix;
        }

        #endregion

        #region Sanitization

        /// <summary>
        /// Sanitizes HTML to prevent XSS attacks
        /// پاکسازی HTML برای جلوگیری از XSS
        /// </summary>
        public static string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            // Remove dangerous tags
            html = RemoveTags(html, "script", "iframe", "object", "embed", "form");

            // Remove event handlers
            html = Regex.Replace(html, @"on\w+\s*=\s*[""][^""]*[""]", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"on\w+\s*=\s*'[^']*'", "", RegexOptions.IgnoreCase);

            // Remove javascript: protocol
            html = Regex.Replace(html, @"javascript:", "", RegexOptions.IgnoreCase);

            return html;
        }

        #endregion

        #region List Generation

        /// <summary>
        /// Generates an unordered list from items
        /// تولید لیست از آیتم‌ها
        /// </summary>
        public static string BuildUnorderedList(IEnumerable<string> items, string cssClass = null)
        {
            if (items == null || !items.Any())
                return string.Empty;

            var classAttr = !string.IsNullOrWhiteSpace(cssClass) ? $" class=\"{cssClass}\"" : "";
            var listItems = string.Join("", items.Select(item => $"<li>{item}</li>"));

            return $"<ul{classAttr}>{listItems}</ul>";
        }

        /// <summary>
        /// Generates an ordered list from items
        /// تولید لیست شماره‌دار
        /// </summary>
        public static string BuildOrderedList(IEnumerable<string> items, string cssClass = null)
        {
            if (items == null || !items.Any())
                return string.Empty;

            var classAttr = !string.IsNullOrWhiteSpace(cssClass) ? $" class=\"{cssClass}\"" : "";
            var listItems = string.Join("", items.Select(item => $"<li>{item}</li>"));

            return $"<ol{classAttr}>{listItems}</ol>";
        }

        #endregion

        #region Paragraph Formatting

        /// <summary>
        /// Wraps text lines in paragraph tags
        /// قرار دادن متن در تگ پاراگراف
        /// </summary>
        public static string WrapInParagraphs(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", lines.Select(line => $"<p>{Encode(line)}</p>"));
        }

        #endregion
    }
}
