using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.SystemConsole.Themes;
using SerilogWeb.Classic;
using System;
using System.Configuration;
using System.Web;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس مدیریت پیکربندی Serilog برای ClinicApp
    /// </summary>
    public static class LoggingConfiguration
    {
        /// <summary>
        /// ایجاد Logger Configuration بهینه‌سازی شده
        /// </summary>
        public static LoggerConfiguration CreateOptimizedConfiguration()
        {
            var environment = GetCurrentEnvironment();
            var logLevel = GetLogLevel(environment);
            var retentionDays = GetRetentionDays(environment);

            var config = new LoggerConfiguration()
                // 🎯 تنظیمات سطح لاگ‌گیری
                .MinimumLevel.Is(logLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("EntityFramework", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Data.Entity", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Web", LogEventLevel.Warning)

                // 🔍 غنی‌سازی لاگ‌ها
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "ClinicApp")
                .Enrich.WithProperty("Environment", environment)
                .Enrich.WithProperty("Version", GetApplicationVersion())
                .Enrich.WithProperty("ServerName", Environment.MachineName)

                // 🚫 فیلتر کردن لاگ‌های غیرضروری
                .Filter.ByExcluding(IsStaticFileRequest)
                .Filter.ByExcluding(IsHealthCheckRequest)
                .Filter.ByExcluding(IsFaviconRequest)
                .Filter.ByExcluding(Matching.FromSource("Microsoft.Owin"))
                .Filter.ByExcluding(Matching.FromSource("System.Web.Http"))

                // 📁 Sink 1: فایل لاگ اصلی
                .WriteTo.Async(a => a.File(
                    path: HttpContext.Current.Server.MapPath("~/App_Data/Logs/clinicapp-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: retentionDays,
                    fileSizeLimitBytes: 100 * 1024 * 1024, // 100MB
                    rollOnFileSizeLimit: true,
                    outputTemplate: GetFileOutputTemplate()
                ))

                // 📁 Sink 2: فایل لاگ خطاها
                .WriteTo.Async(a => a.File(
                    path: HttpContext.Current.Server.MapPath("~/App_Data/Logs/errors-.log"),
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 365, // 1 سال
                    fileSizeLimitBytes: 50 * 1024 * 1024, // 50MB
                    rollOnFileSizeLimit: true,
                    outputTemplate: GetErrorOutputTemplate()
                ))

                // 📁 Sink 3: فایل لاگ Performance
                .WriteTo.Async(a => a.File(
                    path: HttpContext.Current.Server.MapPath("~/App_Data/Logs/performance-.log"),
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30, // 1 ماه
                    fileSizeLimitBytes: 25 * 1024 * 1024, // 25MB
                    rollOnFileSizeLimit: true,
                    outputTemplate: GetPerformanceOutputTemplate()
                ))

                // 🖥️ Sink 4: Console (فقط در Development)
                .WriteTo.Conditional(evt => environment == "Development",
                    sink => sink.Console(
                        theme: AnsiConsoleTheme.Code,
                        outputTemplate: GetConsoleOutputTemplate()
                    ))

                // 🗄️ Sink 5: SQL Server Database
                .WriteTo.Async(a => a.MSSqlServer(
                    connectionString: ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "ApplicationLogs",
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 50
                    },
                    restrictedToMinimumLevel: LogEventLevel.Information
                ))

                // 🔍 Sink 6: Seq (فقط در Development و Staging)
                .WriteTo.Conditional(evt => environment != "Production",
                    sink => sink.Async(a => a.Seq(
                        serverUrl: ConfigurationManager.AppSettings["SeqUrl"] ?? "http://localhost:5341",
                        apiKey: ConfigurationManager.AppSettings["SeqApiKey"],
                        restrictedToMinimumLevel: LogEventLevel.Debug
                    )))

                // 📧 Sink 7: Email برای خطاهای بحرانی
                .WriteTo.Conditional(evt => environment == "Production" && evt.Level >= LogEventLevel.Fatal,
                    sink => sink.Email(
                        fromEmail: ConfigurationManager.AppSettings["Email:FromAddress"],
                        toEmail: ConfigurationManager.AppSettings["Email:AdminEmail"],
                        mailServer: ConfigurationManager.AppSettings["Email:SmtpServer"],
                        mailSubject: "🚨 خطای بحرانی در ClinicApp",
                        outputTemplate: GetEmailOutputTemplate()
                    ));

            // اعمال فیلترهای اضافی
            config = ConfigureAdditionalFilters(config);
            
            return config;
        }

        /// <summary>
        /// دریافت محیط فعلی
        /// </summary>
        private static string GetCurrentEnvironment()
        {
            return ConfigurationManager.AppSettings["Environment"] ?? "Production";
        }

        /// <summary>
        /// دریافت سطح لاگ بر اساس محیط
        /// </summary>
        private static LogEventLevel GetLogLevel(string environment)
        {
            return environment switch
            {
                "Development" => LogEventLevel.Debug,
                "Staging" => LogEventLevel.Information,
                "Production" => LogEventLevel.Warning,
                _ => LogEventLevel.Information
            };
        }

        /// <summary>
        /// دریافت تعداد روزهای نگهداری لاگ
        /// </summary>
        private static int GetRetentionDays(string environment)
        {
            return environment switch
            {
                "Development" => 7,    // 1 هفته
                "Staging" => 30,       // 1 ماه
                "Production" => 90,    // 3 ماه
                _ => 30
            };
        }

        /// <summary>
        /// دریافت نسخه اپلیکیشن
        /// </summary>
        private static string GetApplicationVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version?.ToString() ?? "1.0.0.0";
        }

        /// <summary>
        /// بررسی آیا درخواست فایل استاتیک است
        /// </summary>
        private static bool IsStaticFileRequest(LogEvent evt)
        {
            if (evt.Properties.TryGetValue("RequestPath", out var requestPath))
            {
                var path = requestPath.ToString().Trim('"');
                return path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".png") ||
                       path.EndsWith(".jpg") || path.EndsWith(".gif") || path.EndsWith(".ico") ||
                       path.EndsWith(".woff") || path.EndsWith(".woff2") || path.EndsWith(".ttf") ||
                       path.EndsWith(".svg") || path.EndsWith(".eot");
            }
            return false;
        }

        /// <summary>
        /// بررسی آیا درخواست Health Check است
        /// </summary>
        private static bool IsHealthCheckRequest(LogEvent evt)
        {
            if (evt.Properties.TryGetValue("RequestPath", out var requestPath))
            {
                var path = requestPath.ToString().Trim('"');
                return path.Contains("/health") || path.Contains("/ping") || path.Contains("/status");
            }
            return false;
        }

        /// <summary>
        /// بررسی آیا درخواست Favicon است
        /// </summary>
        private static bool IsFaviconRequest(LogEvent evt)
        {
            if (evt.Properties.TryGetValue("RequestPath", out var requestPath))
            {
                var path = requestPath.ToString().Trim('"');
                return path.Contains("/favicon.ico");
            }
            return false;
        }

        /// <summary>
        /// دریافت Template برای فایل لاگ اصلی
        /// </summary>
        private static string GetFileOutputTemplate()
        {
            return "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}{Properties:j}{NewLine}---{NewLine}";
        }

        /// <summary>
        /// دریافت Template برای فایل لاگ خطاها
        /// </summary>
        private static string GetErrorOutputTemplate()
        {
            return "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}{NewLine}---{NewLine}";
        }

        /// <summary>
        /// دریافت Template برای فایل لاگ Performance
        /// </summary>
        private static string GetPerformanceOutputTemplate()
        {
            return "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Properties:j}{NewLine}---{NewLine}";
        }

        /// <summary>
        /// دریافت Template برای Console
        /// </summary>
        private static string GetConsoleOutputTemplate()
        {
            return "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
        }

        /// <summary>
        /// دریافت Template برای Email
        /// </summary>
        private static string GetEmailOutputTemplate()
        {
            return "خطای بحرانی در {Application} - {Environment}{NewLine}{NewLine}{Timestamp:yyyy-MM-dd HH:mm:ss}{NewLine}{Level}: {Message}{NewLine}{Exception}";
        }

        /// <summary>
        /// تنظیمات اضافی برای SerilogWeb
        /// </summary>
        public static void ConfigureSerilogWeb()
        {
            var environment = GetCurrentEnvironment();
            var logLevel = GetLogLevel(environment); // از نوع LogEventLevel

            // پیکربندی صحیح SerilogWeb.Classic
            SerilogWebClassic.Configure(cfg => cfg
                .LogAtLevel(logLevel)                 // سطح لاگ درخواست‌ها
                .EnableFormDataLogging(forms => forms // اگر خواستید فرم‌ها فقط هنگام خطا لاگ شوند
                    .OnlyOnError())
            );
        }

        /// <summary>
        /// تنظیمات اضافی برای فیلتر کردن لاگ‌های خاص
        /// </summary>
        public static LoggerConfiguration ConfigureAdditionalFilters(LoggerConfiguration config)
        {
            // فیلتر کردن لاگ‌های مربوط به Entity Framework
            config = config.Filter.ByExcluding(Matching.FromSource("System.Data.Entity.Infrastructure"));
            
            // فیلتر کردن لاگ‌های مربوط به ASP.NET
            config = config.Filter.ByExcluding(Matching.FromSource("System.Web.Mvc"));
            
            // فیلتر کردن لاگ‌های مربوط به Unity
            config = config.Filter.ByExcluding(Matching.FromSource("Unity"));
            
            return config;
        }
    }
}
