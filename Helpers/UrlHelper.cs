using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper class for URL operations
    /// کلاس کمکی برای عملیات URL
    /// </summary>
    public static class UrlHelper
    {
        #region Query String Building

        /// <summary>
        /// Builds a query string from dictionary
        /// ساخت Query String از Dictionary
        /// </summary>
        /// <param name="parameters">Parameters dictionary</param>
        /// <returns>Query string with leading ?</returns>
        /// <example>
        /// var query = UrlHelper.BuildQueryString(new Dictionary&lt;string, string&gt;
        /// {
        ///     { "name", "علی" },
        ///     { "age", "30" }
        /// });
        /// // "?name=%D8%B9%D9%84%DB%8C&amp;age=30"
        /// </example>
        public static string BuildQueryString(Dictionary<string, string> parameters)
        {
            if (parameters == null || !parameters.Any())
                return string.Empty;

            var pairs = parameters
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");

            return "?" + string.Join("&", pairs);
        }

        /// <summary>
        /// Builds a query string from object properties
        /// ساخت Query String از Properties شیء
        /// </summary>
        public static string BuildQueryString(object obj)
        {
            if (obj == null)
                return string.Empty;

            var properties = obj.GetType().GetProperties();
            var parameters = new Dictionary<string, string>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                if (value != null)
                {
                    parameters[prop.Name] = value.ToString();
                }
            }

            return BuildQueryString(parameters);
        }

        #endregion

        #region Query String Parsing

        /// <summary>
        /// Parses query string to dictionary
        /// تجزیه Query String به Dictionary
        /// </summary>
        public static Dictionary<string, string> ParseQueryString(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return new Dictionary<string, string>();

            try
            {
                var uri = new Uri(url);
                var query = HttpUtility.ParseQueryString(uri.Query);
                
                return query.AllKeys
                    .Where(key => key != null)
                    .ToDictionary(key => key, key => query[key]);
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Gets a specific query parameter value
        /// دریافت مقدار پارامتر خاص
        /// </summary>
        public static string GetQueryParameter(string url, string parameterName)
        {
            var parameters = ParseQueryString(url);
            return parameters.ContainsKey(parameterName) ? parameters[parameterName] : null;
        }

        #endregion

        #region URL Combining

        /// <summary>
        /// Combines URL parts safely
        /// ترکیب بخش‌های URL به صورت امن
        /// </summary>
        /// <param name="parts">URL parts</param>
        /// <returns>Combined URL</returns>
        /// <example>
        /// string url = UrlHelper.CombineUrl("http://example.com", "api", "users", "123");
        /// // "http://example.com/api/users/123"
        /// </example>
        public static string CombineUrl(params string[] parts)
        {
            if (parts == null || parts.Length == 0)
                return string.Empty;

            var cleanedParts = parts
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim('/'))
                .Where(p => !string.IsNullOrEmpty(p));

            return string.Join("/", cleanedParts);
        }

        #endregion

        #region URL Validation

        /// <summary>
        /// Validates if a string is a valid URL
        /// اعتبارسنجی URL
        /// </summary>
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Validates if a string is a relative URL
        /// بررسی URL نسبی
        /// </summary>
        public static bool IsRelativeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Relative, out _);
        }

        #endregion

        #region URL Parts Extraction

        /// <summary>
        /// Gets domain from URL
        /// دریافت دامنه از URL
        /// </summary>
        public static string GetDomain(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets protocol from URL (http/https)
        /// دریافت پروتکل
        /// </summary>
        public static string GetProtocol(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            try
            {
                var uri = new Uri(url);
                return uri.Scheme;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets path from URL
        /// دریافت مسیر
        /// </summary>
        public static string GetPath(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            try
            {
                var uri = new Uri(url);
                return uri.AbsolutePath;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region URL Modification

        /// <summary>
        /// Adds or updates a query parameter
        /// افزودن یا به‌روزرسانی پارامتر
        /// </summary>
        public static string AddOrUpdateParameter(string url, string parameterName, string parameterValue)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            try
            {
                var uri = new Uri(url);
                var parameters = ParseQueryString(url);
                
                parameters[parameterName] = parameterValue;
                
                var baseUrl = url.Split('?')[0];
                var queryString = BuildQueryString(parameters);
                
                return baseUrl + queryString;
            }
            catch
            {
                return url;
            }
        }

        /// <summary>
        /// Removes a query parameter
        /// حذف پارامتر
        /// </summary>
        public static string RemoveParameter(string url, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            try
            {
                var parameters = ParseQueryString(url);
                
                if (parameters.ContainsKey(parameterName))
                {
                    parameters.Remove(parameterName);
                }
                
                var baseUrl = url.Split('?')[0];
                var queryString = BuildQueryString(parameters);
                
                return baseUrl + queryString;
            }
            catch
            {
                return url;
            }
        }

        #endregion

        #region Encoding/Decoding

        /// <summary>
        /// URL encodes a string
        /// رمزنگاری URL
        /// </summary>
        public static string Encode(string value)
        {
            return Uri.EscapeDataString(value ?? string.Empty);
        }

        /// <summary>
        /// URL decodes a string
        /// رمزگشایی URL
        /// </summary>
        public static string Decode(string value)
        {
            return Uri.UnescapeDataString(value ?? string.Empty);
        }

        #endregion
    }
}
