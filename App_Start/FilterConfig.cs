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
            
            // 🏥 V2: فیلتر Zero-Cache برای Reception V2
            filters.Add(new ClinicApp.Filters.NoCacheAttribute());
            
            // 🔒 SECURITY: Anti-Forgery فقط برای متدهای تغییردهنده (POST/PUT/DELETE)
            filters.Add(new ClinicApp.Filters.ValidateAntiForgeryTokenOnPostsAttribute());
            
            // 📊 LOGGING: CorrelationId Filter برای ردیابی درخواست‌ها
            filters.Add(new ClinicApp.Filters.CorrelationIdFilter());
            
            // 🚨 EXCEPTION: Global Exception Filter برای ServiceResult
            filters.Add(new ClinicApp.Filters.GlobalExceptionFilter());

            // ⏱️ Request timing for performance visibility
            filters.Add(new ClinicApp.Filters.RequestTimingFilter());
        }
    }
}