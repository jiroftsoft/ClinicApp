using System;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Models.Core;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Filters
{
    /// <summary>
    /// فیلتر احراز هویت و مجوز برای دسترسی به بخش Patient
    /// این فیلتر اطمینان می‌دهد که فقط کاربران با نقش Patient می‌توانند به بخش‌های Patient دسترسی داشته باشند
    /// 
    /// ✅ Enterprise-Grade: Security, Logging, Proper Error Handling
    /// طبق: PATIENT_AUTH_INTEGRATION_ANALYSIS.md
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PatientRoleAuthorizationAttribute : AuthorizeAttribute
    {
        private static readonly ILogger _log = Log.ForContext<PatientRoleAuthorizationAttribute>();

        /// <summary>
        /// بررسی احراز هویت و نقش Patient
        /// </summary>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            try
            {
                // بررسی احراز هویت
                if (httpContext?.User?.Identity == null || !httpContext.User.Identity.IsAuthenticated)
                {
                    _log.Debug("کاربر احراز هویت نشده است - دسترسی به بخش Patient رد شد");
                    return false;
                }

                // بررسی نقش Patient
                var isPatient = httpContext.User.IsInRole(AppRoles.Patient);
                
                if (!isPatient)
                {
                    _log.Warning(
                        "کاربر با شناسه {UserId} و نام {UserName} که نقش Patient ندارد، تلاش برای دسترسی به بخش Patient کرد",
                        httpContext.User.Identity.GetUserId(),
                        httpContext.User.Identity.Name);
                }
                else
                {
                    _log.Debug(
                        "کاربر با شناسه {UserId} و نقش Patient مجوز دسترسی دارد",
                        httpContext.User.Identity.GetUserId());
                }

                return isPatient;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی مجوز دسترسی به بخش Patient");
                // در صورت خطا، دسترسی را رد می‌کنیم (Fail-Safe)
                return false;
            }
        }

        /// <summary>
        /// مدیریت پاسخ در صورت عدم مجوز
        /// </summary>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            try
            {
                var httpContext = filterContext.HttpContext;
                
                // ✅ تعریف returnUrl در ابتدای متد (برای استفاده در تمام scopeها)
                var returnUrl = httpContext.Request.Url?.PathAndQuery;
                
                // اگر کاربر احراز هویت نشده است، به صفحه لاگین هدایت می‌شود
                if (httpContext?.User?.Identity == null || !httpContext.User.Identity.IsAuthenticated)
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary
                        {
                            { "controller", "Account" },
                            { "action", "Login" },
                            { "area", "" },
                            { "returnUrl", returnUrl }
                        });
                    
                    _log.Debug("کاربر احراز هویت نشده - هدایت به صفحه لاگین با returnUrl: {ReturnUrl}", returnUrl);
                    return;
                }

                // اگر کاربر احراز هویت شده اما نقش Patient ندارد
                // برای درخواست‌های AJAX، پاسخ JSON برمی‌گردانیم
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            success = false,
                            message = "شما مجوز دسترسی به این بخش را ندارید. لطفاً با نقش مناسب وارد شوید.",
                            unauthorized = true
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    filterContext.HttpContext.Response.StatusCode = 403; // Forbidden
                    
                    _log.Warning(
                        "کاربر احراز هویت شده اما بدون نقش Patient - درخواست AJAX رد شد. UserId: {UserId}",
                        httpContext.User.Identity.GetUserId());
                    return;
                }

                // ✅ برای درخواست‌های عادی، به صفحه Login هدایت می‌شود (نه Home)
                // این بهتر است چون کاربر ممکن است با نقش دیگری لاگین کرده باشد
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" },
                        { "area", "" },
                        { "returnUrl", returnUrl }
                    });
                
                // ✅ پیام خطا را در TempData قرار می‌دهیم
                if (filterContext.Controller is Controller controller)
                {
                    NotificationHelper.SetError(controller.TempData, 
                        "شما مجوز دسترسی به بخش بیمار را ندارید. لطفاً با حساب کاربری بیمار وارد شوید.");
                }

                _log.Warning(
                    "کاربر احراز هویت شده اما بدون نقش Patient - هدایت به صفحه Login. UserId: {UserId}, ReturnUrl: {ReturnUrl}",
                    httpContext.User.Identity.GetUserId(), returnUrl);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در مدیریت پاسخ عدم مجوز دسترسی به بخش Patient");
                // در صورت خطا، به صفحه اصلی هدایت می‌کنیم
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Index" },
                        { "area", "" }
                    });
            }
        }
    }
}

