using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Action Filter برای بارگذاری Stories در ViewBag
    /// استفاده می‌شود در Layout برای نمایش Stories زیر منو
    /// </summary>
    public class LoadStoriesActionFilter : ActionFilterAttribute
    {
        private static readonly ILogger _logger = Log.ForContext<LoadStoriesActionFilter>();

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                // اگر Stories قبلاً لود شده باشد، دوباره لود نکن
                if (filterContext.Controller.ViewBag.Stories != null)
                {
                    base.OnActionExecuted(filterContext);
                    return;
                }

                // دریافت StoryService از Dependency Resolver
                var storyService = DependencyResolver.Current.GetService<IStoryService>();
                if (storyService == null)
                {
                    _logger.Warning("IStoryService not found in Dependency Resolver");
                    base.OnActionExecuted(filterContext);
                    return;
                }

                // لود Stories به صورت Async (اما باید Synchronous باشد)
                var storiesTask = Task.Run(async () => await storyService.GetActiveStoriesForPublicAsync());
                storiesTask.Wait(); // Wait for completion

                var storiesResult = storiesTask.Result;
                if (storiesResult.Success && storiesResult.Data != null && storiesResult.Data.Any())
                {
                    filterContext.Controller.ViewBag.Stories = storiesResult.Data;
                    _logger.Debug("Stories loaded successfully for {Action}", filterContext.ActionDescriptor.ActionName);
                }
                else
                {
                    filterContext.Controller.ViewBag.Stories = new List<StoryPublicViewModel>();
                    _logger.Debug("No active stories found for {Action}", filterContext.ActionDescriptor.ActionName);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the application
                _logger.Error(ex, "Error loading Stories in LoadStoriesActionFilter");
                filterContext.Controller.ViewBag.Stories = new List<StoryPublicViewModel>();
            }
            finally
            {
                base.OnActionExecuted(filterContext);
            }
        }
    }
}
