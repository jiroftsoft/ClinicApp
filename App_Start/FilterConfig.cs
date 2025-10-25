using System.Web;
using System.Web.Mvc;
using ClinicApp.Filters;

namespace ClinicApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            
            // فیلتر Culture برای پشتیبانی صحیح از زبان فارسی
            filters.Add(new CultureFilter());
            
            // 🏥 MEDICAL: فیلتر ضد کش برای مسیرهای درمانی - Real-time data for clinical safety
            filters.Add(new NoCacheFilter());
            
            // 🔒 SECURITY: Global Anti-Forgery Filter برای تمام POST requests
            filters.Add(new ValidateAntiForgeryTokenAttribute());
        }
    }
}