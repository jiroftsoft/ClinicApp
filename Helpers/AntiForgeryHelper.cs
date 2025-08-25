using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس کمکی برای مدیریت Anti-Forgery Token در درخواست‌های AJAX
    /// </summary>
    public static class AntiForgeryHelper
    {
        /// <summary>
        /// دریافت Anti-Forgery Token برای استفاده در AJAX
        /// </summary>
        /// <returns>Token string</returns>
        public static string GetToken()
        {
            string cookieToken, formToken;
            AntiForgery.GetTokens(null, out cookieToken, out formToken);
            return formToken;
        }

        /// <summary>
        /// دریافت Anti-Forgery Token به همراه نام فیلد
        /// </summary>
        /// <returns>object containing token name and value</returns>
        public static object GetTokenData()
        {
            string cookieToken, formToken;
            AntiForgery.GetTokens(null, out cookieToken, out formToken);
            return new
            {
                name = "__RequestVerificationToken",
                value = formToken
            };
        }
    }
}
