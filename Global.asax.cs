using AutoMapper;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Infrastructure;
using ClinicApp.Models;
using ClinicApp.Models.Binders;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.SystemConsole.Themes; // ممکن است این using دیگر لازم نباشد
using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unity.AspNet.Mvc;

namespace ClinicApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // تنظیمات Culture برای پشتیبانی بهتر از فارسی
            // در .NET Framework 4.8 نیازی به RegisterProvider نیست
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fa-IR");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fa-IR");
            
            // تنظیمات Culture برای Decimal Parsing
            // این باعث می‌شود که Decimal Parsing همیشه از "." استفاده کند
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            
            // Medical Environment: Global No-Cache Filter
            GlobalFilters.Filters.Add(new NoCacheFilter());
            
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // ثبت Model Binder سفارشی برای InsuranceTariffCreateEditViewModel
            ModelBinders.Binders.Add(typeof(InsuranceTariffCreateEditViewModel), new InsuranceTariffModelBinder());
            
            // ثبت Model Binder برای Decimal - حل مشکل Culture
            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());
            
            // ✅ ثبت Model Binder برای TimeSpan - حل مشکل Model Binding برای input type="time"
            ModelBinders.Binders.Add(typeof(TimeSpan), new TimeSpanModelBinder());
            ModelBinders.Binders.Add(typeof(TimeSpan?), new TimeSpanModelBinder());
            
            // اگر UnityConfig دارید اینجا هم اضافه کنید:
            DependencyResolver.SetResolver(new UnityDependencyResolver(UnityConfig.Container));

            // اجرای فرآیند Seed فقط یک بار در زمان شروع برنامه
            using (var context = new ApplicationDbContext())
            {
                // مرحله ۱: اجرای فرآیند Seed برای اطمینان از وجود کاربران سیستمی
               IdentitySeed.SeedDefaultData(context);

                // مرحله ۲: مقداردهی اولیه و کش کردن شناسه‌های کاربران سیستمی
                SystemUsers.Initialize(context);
            }

            #region پیکربندی حرفه‌ای و بهینه‌سازی شده Serilog

            // 🚀 استفاده از کلاس‌های بهینه‌سازی شده
            Log.Logger = LoggingConfiguration.CreateOptimizedConfiguration().CreateLogger();
            
            // 🔧 تنظیمات اضافی SerilogWeb
            LoggingConfiguration.ConfigureSerilogWeb();
            
            // 🚫 فیلترهای اضافی در CreateOptimizedConfiguration اعمال شده‌اند

            Log.Information("🚀 اپلیکیشن کلینیک با موفقیت شروع به کار کرد");
            Log.Information("📊 محیط: {Environment} | نسخه: {Version} | سرور: {ServerName}", 
                GetCurrentEnvironment(), GetApplicationVersion(), Environment.MachineName);
            #endregion
        }

        protected void Application_End()
        {
            Log.Information("اپلیکیشن کلینیک بسته شد.");
            Log.CloseAndFlush();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception != null)
            {
                // Don't log 404 errors for static files that are expected to not exist
                // These files (.map, .well-known) are requested by browsers but may not exist
                var httpException = exception as System.Web.HttpException;
                if (httpException != null && httpException.GetHttpCode() == 404)
                {
                    string path = Request.Path.ToLowerInvariant();
                    if (path.EndsWith(".map", StringComparison.OrdinalIgnoreCase) || 
                        path.StartsWith("/.well-known/", StringComparison.OrdinalIgnoreCase))
                    {
                        // These are expected 404s for static files, don't log as fatal errors
                        // The IgnoreRoute in RouteConfig should handle these, but if they still
                        // reach here, we suppress the error logging
                        return;
                    }
                }
                
                Log.Fatal(exception, "خطای مدیریت نشده در سطح اپلیکیشن رخ داد.");
            }
        }

        private string GetCurrentEnvironment()
        {
            return ConfigurationManager.AppSettings["Environment"] ?? "Production";
        }

        private string GetApplicationVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version?.ToString() ?? "1.0.0.0";
        }

    }
}