using System;
using System.Web.Mvc;

namespace ClinicApp.Filters
{
    /// <summary>
    /// فیلتر حذف کامل Cache برای محیط درمانی
    /// طبق AI_COMPLIANCE_CONTRACT: قانون 23 - پرهیز از پیچیدگی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. حذف کامل Cache در تمام صفحات
    /// 2. امنیت داده‌های بیماران
    /// 3. جلوگیری از نمایش اطلاعات قدیمی
    /// 4. سازگاری با محیط درمانی حساس
    /// </summary>
    public class NoCacheFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;
            
            try
            {
                // Medical Environment: Complete Cache Disable
                response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                response.Cache.SetNoStore();
                response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);
                response.Cache.SetExpires(DateTime.MinValue);
                response.Cache.SetValidUntilExpires(false);
                response.Cache.SetLastModified(DateTime.Now);
                
                // Only set ETag if not already set
                if (string.IsNullOrEmpty(response.Headers["ETag"]))
                {
                    response.Cache.SetETag(Guid.NewGuid().ToString());
                }
            }
            catch (InvalidOperationException)
            {
                // ETag or other cache headers already set - ignore
            }
            
            // Additional headers for medical safety (always safe to add)
            try
            {
                response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate, max-age=0");
                response.Headers.Add("Pragma", "no-cache");
                response.Headers.Add("Expires", "0");
                response.Headers.Add("Last-Modified", DateTime.Now.ToString("R"));
                response.Headers.Add("X-Content-Type-Options", "nosniff");
                response.Headers.Add("X-Frame-Options", "DENY");
                response.Headers.Add("X-XSS-Protection", "1; mode=block");
            }
            catch (Exception)
            {
                // Headers already set - ignore
            }
            
            base.OnActionExecuting(filterContext);
        }
    }
}