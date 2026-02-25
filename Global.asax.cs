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
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unity.AspNet.Mvc;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace ClinicApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string path = Request.Path ?? "";

            // پذیرش بیمار در root است؛ لینک اشتباه /Admin/CMS/ReceptionV2 → /ReceptionV2
            if (path.StartsWith("/Admin/CMS/ReceptionV2", StringComparison.OrdinalIgnoreCase))
            {
                var rest = path.Length > 22 ? path.Substring(22) : "";
                Response.RedirectPermanent("/ReceptionV2" + rest, true);
                return;
            }

            // Redirect کردن URL های اشتباه View به Controller Action
            if (path.StartsWith("/Areas/Admin/Views/", StringComparison.OrdinalIgnoreCase))
            {
                // Parse کردن path: /Areas/Admin/Views/CMS/ClinicWorkingHours/Index.cshtml
                var pathAfterViews = path.Substring("/Areas/Admin/Views/".Length);
                var pathParts = pathAfterViews.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (pathParts.Length >= 2 && pathParts[0].Equals("CMS", StringComparison.OrdinalIgnoreCase))
                {
                    // استفاده از حروف اصلی برای controller name (PascalCase)
                    var controllerName = ToPascalCase(pathParts[1]);
                    var actionName = pathParts.Length > 2 ? pathParts[2].Replace(".cshtml", "") : "Index";
                    
                    // حذف پسوند .cshtml
                    if (actionName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                    {
                        actionName = actionName.Replace(".cshtml", "");
                    }
                    
                    // تبدیل action name به PascalCase
                    actionName = ToPascalCase(actionName);
                    
                    var redirectUrl = $"/Admin/CMS/{controllerName}";
                    if (!actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
                    {
                        redirectUrl += $"/{actionName}";
                    }
                    
                    Response.RedirectPermanent(redirectUrl, true);
                    return;
                }
                else if (pathParts.Length >= 1)
                {
                    // برای سایر Controllers در Admin (بدون CMS)
                    var controllerName = ToPascalCase(pathParts[0]);
                    var actionName = pathParts.Length > 1 ? pathParts[1].Replace(".cshtml", "") : "Index";
                    
                    if (actionName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                    {
                        actionName = actionName.Replace(".cshtml", "");
                    }
                    
                    // تبدیل action name به PascalCase
                    actionName = ToPascalCase(actionName);
                    
                    var redirectUrl = $"/Admin/{controllerName}";
                    if (!actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
                    {
                        redirectUrl += $"/{actionName}";
                    }
                    
                    Response.RedirectPermanent(redirectUrl, true);
                    return;
                }
            }
            else if (path.StartsWith("/Areas/Patient/Views/", StringComparison.OrdinalIgnoreCase))
            {
                // برای Patient Area
                var pathAfterViews = path.Substring("/Areas/Patient/Views/".Length);
                var pathParts = pathAfterViews.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (pathParts.Length >= 1)
                {
                    var controllerName = ToPascalCase(pathParts[0]);
                    var actionName = pathParts.Length > 1 ? pathParts[1].Replace(".cshtml", "") : "Index";
                    
                    if (actionName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                    {
                        actionName = actionName.Replace(".cshtml", "");
                    }
                    
                    // تبدیل action name به PascalCase
                    actionName = ToPascalCase(actionName);
                    
                    var redirectUrl = $"/Patient/{controllerName}";
                    if (!actionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
                    {
                        redirectUrl += $"/{actionName}";
                    }
                    
                    Response.RedirectPermanent(redirectUrl, true);
                    return;
                }
            }
            else if (path.StartsWith("/Views/", StringComparison.OrdinalIgnoreCase))
            {
                // برای non-Area Views (مثل /Views/Payment/CashierReport/Index.cshtml)
                var pathAfterViews = path.Substring("/Views/".Length);
                var pathParts = pathAfterViews.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (pathParts.Length >= 1)
                {
                    // حذف پسوند .cshtml از آخرین بخش
                    var lastPart = pathParts[pathParts.Length - 1];
                    if (lastPart.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                    {
                        lastPart = lastPart.Replace(".cshtml", "");
                        pathParts[pathParts.Length - 1] = lastPart;
                    }
                    
                    // ساخت redirect URL
                    // مثال: Payment/CashierReport/Index -> /Payment/CashierReport
                    // مثال: Payment/Index -> /Payment/Index
                    var redirectUrl = "/" + string.Join("/", pathParts);
                    
                    // اگر آخرین بخش Index باشد، آن را حذف می‌کنیم (مطابق با MVC conventions)
                    if (pathParts.Length > 0 && pathParts[pathParts.Length - 1].Equals("Index", StringComparison.OrdinalIgnoreCase))
                    {
                        if (pathParts.Length > 1)
                        {
                            // حذف آخرین بخش (Index)
                            var partsWithoutIndex = new string[pathParts.Length - 1];
                            Array.Copy(pathParts, 0, partsWithoutIndex, 0, pathParts.Length - 1);
                            redirectUrl = "/" + string.Join("/", partsWithoutIndex);
                        }
                        else
                        {
                            // فقط Index بود، به Home redirect کن
                            Response.RedirectPermanent("/", true);
                            return;
                        }
                    }
                    
                    // Security: بررسی path traversal و invalid characters
                    if (redirectUrl.Contains("..") || redirectUrl.Contains("//") || redirectUrl.Contains("\\"))
                    {
                        // Invalid path - redirect to home
                        Response.RedirectPermanent("/", true);
                        return;
                    }
                    
                    Response.RedirectPermanent(redirectUrl, true);
                    return;
                }
            }
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
    {
        // ✅ CRITICAL FIX: Ensure OWIN authentication state syncs with MVC HttpContext
        // OWIN middleware sets IOwinContext.Authentication.User, but HttpContext.User may not be synced
        // This is a known issue in ASP.NET MVC5 + OWIN integration
        
        try
        {
            var owinContext = HttpContext.Current?.GetOwinContext();
            if (owinContext == null)
            {
                return; // No OWIN context available
            }

            var hasAuthCookie = Request.Cookies["ClinicAppAuth"] != null;
            var isRequestAuthenticated = Request.IsAuthenticated;
            var owinUser = owinContext.Authentication?.User;
            var isOwinAuthenticated = owinUser != null && owinUser.Identity.IsAuthenticated;

            // ✅ ENHANCED: Force sync in multiple scenarios
            // Scenario 1: Cookie exists but Request.IsAuthenticated is false (timing issue after redirect)
            // Scenario 2: OWIN user is authenticated but HttpContext.User is not synced
            var needsSync = (hasAuthCookie && !isRequestAuthenticated) || 
                           (isOwinAuthenticated && !isRequestAuthenticated) ||
                           (isOwinAuthenticated && HttpContext.Current.User?.Identity?.Name != owinUser.Identity.Name);

            if (needsSync && isOwinAuthenticated)
            {
                // ✅ DEBUG: Log sync operation for troubleshooting
                var userId = owinUser.Identity is ClaimsIdentity claimsIdentity 
                    ? claimsIdentity.GetUserId() 
                    : owinUser.Identity.Name ?? "Unknown";
                    
                Log.Information("🔄 Syncing OWIN user to HttpContext - UserId: {UserId}, Cookie: {HasCookie}, ReqAuth: {ReqAuth}, OwinAuth: {OwinAuth}", 
                    userId, hasAuthCookie, isRequestAuthenticated, isOwinAuthenticated);
                
                // ✅ FORCE SYNC: Set HttpContext.User to OWIN user
                HttpContext.Current.User = owinUser;
                
                // ✅ VERIFY: Confirm sync completed
                var syncSuccess = HttpContext.Current.User?.Identity?.IsAuthenticated ?? false;
                Log.Information("✅ Sync complete - HttpContext.User.IsAuthenticated: {IsAuth}, Name: {Name}", 
                    syncSuccess, HttpContext.Current.User?.Identity?.Name ?? "NULL");
            }
            else if (hasAuthCookie && !isOwinAuthenticated)
            {
                // ✅ Cookie exists but OWIN hasn't validated it yet
                // This can happen immediately after login redirect
                // OWIN middleware will validate on next request cycle
                Log.Information("⏳ Cookie exists but OWIN validation pending - waiting for middleware");
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to sync OWIN authentication state to HttpContext");
        }
    }

        /// <summary>
        /// تبدیل string به PascalCase
        /// مثال: "clinicworkinghours" -> "ClinicWorkingHours"
        /// مثال: "ClinicWorkingHours" -> "ClinicWorkingHours" (بدون تغییر)
        /// </summary>
        private string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // اگر قبلاً PascalCase است (شروع با حرف بزرگ و بعد حرف کوچک)، برگردان
            if (char.IsUpper(input[0]) && input.Length > 1 && char.IsLower(input[1]))
            {
                return input;
            }

            // اگر همه حروف کوچک است، تبدیل به PascalCase
            if (input.All(c => char.IsLower(c) || char.IsDigit(c)))
            {
                // پیدا کردن کلمات بر اساس حروف بزرگ یا جداکننده‌ها
                var result = new System.Text.StringBuilder();
                bool isNewWord = true;
                
                foreach (char c in input)
                {
                    if (char.IsLetterOrDigit(c))
                    {
                        if (isNewWord)
                        {
                            result.Append(char.ToUpper(c));
                            isNewWord = false;
                        }
                        else
                        {
                            result.Append(char.ToLower(c));
                        }
                    }
                    else
                    {
                        isNewWord = true;
                    }
                }
                
                return result.ToString();
            }

            // اگر مخلوط است، سعی کن کلمات را پیدا کن
            var words = System.Text.RegularExpressions.Regex.Split(input, @"(?<!^)(?=[A-Z])|(?<=[a-z])(?=[A-Z])|_|-|\s");
            var result2 = new System.Text.StringBuilder();
            
            foreach (var word in words)
            {
                if (!string.IsNullOrEmpty(word) && char.IsLetterOrDigit(word[0]))
                {
                    result2.Append(char.ToUpper(word[0]));
                    if (word.Length > 1)
                    {
                        result2.Append(word.Substring(1).ToLower());
                    }
                }
            }
            
            return result2.Length > 0 ? result2.ToString() : input;
        }

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
            
            // ✅ ثبت Web API Configuration
            GlobalConfiguration.Configure(WebApiConfig.Register);
            
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