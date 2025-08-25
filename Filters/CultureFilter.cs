using System.Globalization;
using System.Threading;
using System.Web.Mvc;

namespace ClinicApp.Filters
{
    /// <summary>
    /// فیلتر تنظیم Culture برای پشتیبانی صحیح از زبان فارسی
    /// این فیلتر در هر request اجرا می‌شود و اطمینان می‌دهد که
    /// تمام متن‌ها به درستی نمایش داده شوند
    /// </summary>
    public class CultureFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // تنظیم Culture برای thread فعلی
            var culture = new CultureInfo("fa-IR");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // تنظیم Response Encoding
            if (filterContext.HttpContext.Response != null)
            {
                filterContext.HttpContext.Response.ContentEncoding = System.Text.Encoding.UTF8;
                filterContext.HttpContext.Response.HeaderEncoding = System.Text.Encoding.UTF8;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
