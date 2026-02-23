using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using Microsoft.AspNet.Identity;

namespace ClinicApp.Filters;

public class CheckProfileCompletionAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var user = filterContext.HttpContext.User;
        var userId = user?.Identity?.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            base.OnActionExecuting(filterContext);
            return;
        }

        // مدیر و منشی نیازی به پرونده بیمار ندارند — به‌سمت CompleteProfile هدایت نشوند
        if (user.IsInRole(AppRoles.Admin) || user.IsInRole(AppRoles.Receptionist))
        {
            base.OnActionExecuting(filterContext);
            return;
        }

        var db = new ApplicationDbContext(); // For simplicity; ideally, inject this
        var patient = db.Patients.FirstOrDefault(p => p.ApplicationUserId == userId);

        if (patient == null || string.IsNullOrWhiteSpace(patient.NationalCode))
        {
            var routeValues = new RouteValueDictionary
            {
                { "controller", "Patient" },
                { "action", "CompleteProfile" },
                { "area", "" } // روت اصلی تا مسیر /Patient/CompleteProfile شود، نه /Admin/CMS/Patient/CompleteProfile
            };
            filterContext.Result = new RedirectToRouteResult(routeValues);
        }

        base.OnActionExecuting(filterContext);
    }
}