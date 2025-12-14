using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Filters
{
    /// <summary>
    /// Action Filter برای لود Footer در تمام صفحات
    /// طبق قرارداد: Footer باید در تمام صفحات نمایش داده شود
    /// </summary>
    public class LoadFooterAttribute : ActionFilterAttribute
    {
        private static readonly ILogger _logger = Log.ForContext<LoadFooterAttribute>();

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                // اگر Footer قبلاً لود شده باشد، دوباره لود نکن
                if (filterContext.Controller.ViewBag.Footer != null)
                {
                    base.OnActionExecuted(filterContext);
                    return;
                }

                // دریافت HomePageService از Dependency Resolver
                var homePageService = DependencyResolver.Current.GetService<IHomePageService>();
                if (homePageService == null)
                {
                    _logger.Warning("IHomePageService not found in Dependency Resolver");
                    base.OnActionExecuted(filterContext);
                    return;
                }

                // لود Footer به صورت Async (اما باید Synchronous باشد)
                // استفاده از Task.Run برای تبدیل Async به Sync
                var footerTask = Task.Run(async () => await homePageService.GetFooterDataAsync());
                footerTask.Wait(); // Wait for completion

                var footer = footerTask.Result;
                if (footer != null)
                {
                    filterContext.Controller.ViewBag.Footer = footer;
                    _logger.Debug("Footer loaded successfully for {Action}", filterContext.ActionDescriptor.ActionName);
                }
                else
                {
                    _logger.Warning("Footer data is null for {Action}", filterContext.ActionDescriptor.ActionName);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the application
                _logger.Error(ex, "Error loading Footer in LoadFooterAttribute");
            }
            finally
            {
                base.OnActionExecuted(filterContext);
            }
        }
    }
}
