using AutoMapper;
using ClinicApp.Helpers;
using ClinicApp.Infrastructure;
using ClinicApp.Models;
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
            
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
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

            #region پیکربندی نهایی و حرفه‌ای Serilog

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)

                // غنی‌سازی لاگ‌ها (Enrichers)
                // توجه: SerilogWeb.Classic به صورت خودکار ClientIp, UserAgent, RequestId و ... را اضافه می‌کند.
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "ClinicApp")
                .Enrich.WithProperty("Environment", GetCurrentEnvironment())

                // فیلتر کردن لاگ‌های مربوط به فایل‌های استاتیک
                .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p =>
                    p.EndsWith(".css") || p.EndsWith(".js") || p.EndsWith(".png") || p.EndsWith(".jpg")))

                // پیکربندی مقصدها (Sinks)
                .WriteTo.Async(a => a.File(
                    path: Server.MapPath("~/App_Data/Logs/log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 90,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}{Properties:j}"
                ))

                .WriteTo.Async(a => a.Console(theme: AnsiConsoleTheme.Code))

                .WriteTo.Async(a => a.MSSqlServer(
                    connectionString: ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString,
                    sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true }
                ))

                // 🚀 پیکربندی Sink برای Seq
                .WriteTo.Async(a => a.Seq(
                    serverUrl: ConfigurationManager.AppSettings["SeqUrl"] ?? "http://localhost:5341",
                    apiKey: ConfigurationManager.AppSettings["SeqApiKey"]
                ))

                .CreateLogger();

            Log.Information("اپلیکیشن کلینیک با موفقیت شروع به کار کرد. محیط: {Environment}", GetCurrentEnvironment());
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
                Log.Fatal(exception, "خطای مدیریت نشده در سطح اپلیکیشن رخ داد.");
            }
        }

        private string GetCurrentEnvironment()
        {
            return ConfigurationManager.AppSettings["Environment"] ?? "Production";
        }

    }
}