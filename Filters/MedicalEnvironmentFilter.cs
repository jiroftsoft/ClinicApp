using System;
using System.Web.Mvc;
using Serilog;

namespace ClinicApp.Filters
{
    /// <summary>
    /// فیلتر محیط درمانی برای مدیریت serviceCategoryId
    /// Medical Environment Filter for serviceCategoryId management
    /// </summary>
    public class MedicalEnvironmentFilter : ActionFilterAttribute
    {
        private readonly ILogger _log = Log.ForContext<MedicalEnvironmentFilter>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var controller = filterContext.Controller as Controller;
                if (controller == null) return;

                // بررسی اگر در ServiceController هستیم
                if (controller.GetType().Name == "ServiceController")
                {
                    var actionName = filterContext.ActionDescriptor.ActionName;
                    
                    if (actionName == "Index")
                    {
                        // بازیابی serviceCategoryId از Session اگر در پارامترها نباشد
                        var serviceCategoryId = filterContext.ActionParameters.ContainsKey("serviceCategoryId") 
                            ? filterContext.ActionParameters["serviceCategoryId"] 
                            : null;

                        if (serviceCategoryId == null || serviceCategoryId.ToString() == "0")
                        {
                            var sessionCategoryId = controller.Session["CurrentServiceCategoryId"];
                            if (sessionCategoryId != null)
                            {
                                filterContext.ActionParameters["serviceCategoryId"] = sessionCategoryId;
                                _log.Information("🏥 MEDICAL: بازیابی serviceCategoryId از Session: {CategoryId}", sessionCategoryId);
                            }
                        }
                        else
                        {
                            // ذخیره در Session
                            controller.Session["CurrentServiceCategoryId"] = serviceCategoryId;
                            _log.Information("🏥 MEDICAL: ذخیره serviceCategoryId در Session: {CategoryId}", serviceCategoryId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در MedicalEnvironmentFilter");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
