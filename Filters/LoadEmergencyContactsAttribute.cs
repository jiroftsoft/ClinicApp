using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces.CMS;
using Serilog;

namespace ClinicApp.Filters
{
    /// <summary>
    /// Action Filter برای لود Emergency Contacts در تمام صفحات
    /// طبق قرارداد: Emergency Contacts باید در تمام صفحات نمایش داده شود (Sticky Bar)
    /// </summary>
    public class LoadEmergencyContactsAttribute : ActionFilterAttribute
    {
        private static readonly ILogger _logger = Log.ForContext<LoadEmergencyContactsAttribute>();

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                // اگر Emergency Contacts قبلاً لود شده باشد، دوباره لود نکن
                if (filterContext.Controller.ViewBag.EmergencyContacts != null)
                {
                    base.OnActionExecuted(filterContext);
                    return;
                }

                // دریافت EmergencyContactService از Dependency Resolver
                var emergencyContactService = DependencyResolver.Current.GetService<IEmergencyContactService>();
                if (emergencyContactService == null)
                {
                    _logger.Warning("IEmergencyContactService not found in Dependency Resolver");
                    base.OnActionExecuted(filterContext);
                    return;
                }

                // لود Emergency Contacts به صورت Async (اما باید Synchronous باشد)
                var contactsTask = Task.Run(async () => await emergencyContactService.GetActiveContactsAsync());
                contactsTask.Wait(); // Wait for completion

                var contactsResult = contactsTask.Result;
                if (contactsResult != null && contactsResult.Success && contactsResult.Data != null && contactsResult.Data.Any())
                {
                    filterContext.Controller.ViewBag.EmergencyContacts = contactsResult.Data;
                    _logger.Debug("Emergency Contacts loaded successfully for {Action}", filterContext.ActionDescriptor.ActionName);
                }
                else
                {
                    _logger.Debug("No active emergency contacts found for {Action}", filterContext.ActionDescriptor.ActionName);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the application
                _logger.Error(ex, "Error loading Emergency Contacts in LoadEmergencyContactsAttribute");
            }
            finally
            {
                base.OnActionExecuted(filterContext);
            }
        }
    }
}
