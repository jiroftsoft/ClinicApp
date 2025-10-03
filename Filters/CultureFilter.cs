using System;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;

namespace ClinicApp.Filters
{
    /// <summary>
    /// Filter برای تنظیم Culture در هر Request
    /// این Filter مشکل Culture در Decimal Parsing را حل می‌کند
    /// </summary>
    public class CultureFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // تنظیم Culture برای Request فعلی
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fa-IR");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            
            // تنظیم Culture برای Decimal Parsing
            // این باعث می‌شود که Decimal Parsing همیشه از "." استفاده کند
            Thread.CurrentThread.CurrentCulture = 
                new CultureInfo("fa-IR") { NumberFormat = { NumberDecimalSeparator = "." } };
            
            base.OnActionExecuting(filterContext);
        }
    }
}