using System;
using System.Web;
using System.Web.Mvc;

namespace ClinicApp.Filters
{
    /// <summary>
    /// فیلتر ضد کش برای مسیرهای درمانی - Real-time data for clinical safety
    /// 
    /// این فیلتر تمام هدرهای ضد کش را برای اطمینان از نمایش داده‌های real-time
    /// در محیط‌های درمانی اعمال می‌کند.
    /// </summary>
    public class NoCacheFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;
            var cache = response.Cache;

            // 🏥 MEDICAL: Set aggressive no-cache headers for clinical safety
            cache.SetCacheability(HttpCacheability.NoCache);
            cache.SetNoStore();
            cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            cache.SetExpires(DateTime.UtcNow.AddSeconds(-1));
            cache.AppendCacheExtension("must-revalidate, proxy-revalidate");

            // Additional headers for maximum compatibility
            response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate, max-age=0");
            response.Headers.Add("Pragma", "no-cache");
            response.Headers.Add("Expires", "0");

            base.OnResultExecuting(filterContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 🏥 MEDICAL: Log cache prevention for audit trail
            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var actionName = filterContext.ActionDescriptor.ActionName;
            
            // Only log for clinical controllers to avoid noise
            if (IsClinicalController(controllerName))
            {
                System.Diagnostics.Debug.WriteLine($"🏥 MEDICAL: NoCache applied to {controllerName}.{actionName}");
            }

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// تشخیص کنترلرهای درمانی برای لاگ‌گیری
        /// </summary>
        private bool IsClinicalController(string controllerName)
        {
            var clinicalControllers = new[]
            {
                "InsuranceTariff", "SupplementaryTariff", "CombinedInsuranceCalculation",
                "InsuranceCalculation", "PatientInsurance", "Reception",
                "Doctor", "Appointment", "EmergencyBooking", "ScheduleOptimization"
            };

            return Array.Exists(clinicalControllers, name => 
                controllerName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
