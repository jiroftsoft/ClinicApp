using System;
using System.Web.Mvc;
using Serilog;

namespace ClinicApp.Filters
{
    /// <summary>
    /// Action Filter برای بارگذاری Stories در Layout
    /// ⚠️ DEPRECATED: این Filter دیگر استفاده نمی‌شود
    /// به جای آن از LayoutDataHelper.GetLayoutData() در _Layout.cshtml استفاده می‌شود (Strongly-Typed)
    /// </summary>
    [Obsolete("استفاده از LayoutDataHelper.GetLayoutData() در _Layout.cshtml (Strongly-Typed)")]
    public class LoadStoriesActionFilter : ActionFilterAttribute
    {
        private static readonly ILogger _logger = Log.ForContext<LoadStoriesActionFilter>();

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // این Filter دیگر استفاده نمی‌شود
            // Stories از طریق LayoutDataHelper.GetLayoutData() در _Layout.cshtml لود می‌شود (Strongly-Typed)
            base.OnActionExecuted(filterContext);
        }
    }
}
