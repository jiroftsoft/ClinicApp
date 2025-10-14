using System;
using System.Web;
using System.Web.Mvc;

namespace ClinicApp.Filters
{
    /// <summary>
    /// فیلتر No-Cache برای محیط درمانی
    /// این فیلتر هدرهای مناسب برای جلوگیری از کش شدن صفحات حساس درمانی را تنظیم می‌کند
    /// </summary>
    public class NoStoreAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;
            
            // تنظیم هدرهای No-Cache برای محیط درمانی
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            response.Cache.SetMaxAge(TimeSpan.Zero);
            
            // هدرهای اضافی برای اطمینان از عدم کش شدن
            response.Headers.Add("Pragma", "no-cache");
            response.Headers.Add("Expires", "0");
            response.Headers.Add("Last-Modified", DateTime.UtcNow.ToString("R"));
            
            base.OnResultExecuting(filterContext);
        }
    }
}
