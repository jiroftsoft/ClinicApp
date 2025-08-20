using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using ClinicApp.Models;
using Microsoft.AspNet.Identity;

namespace ClinicApp.Filters;

public class CheckProfileCompletionAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var userId = filterContext.HttpContext.User.Identity.GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            var db = new ApplicationDbContext(); // For simplicity; ideally, inject this
            var patient = db.Patients.FirstOrDefault(p => p.ApplicationUserId == userId);

            // If a patient profile doesn't exist or a key field is missing
            if (patient == null || string.IsNullOrWhiteSpace(patient.NationalCode))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Patient" },
                        { "action", "CompleteProfile" }
                    });
            }
        }
        base.OnActionExecuting(filterContext);
    }
}