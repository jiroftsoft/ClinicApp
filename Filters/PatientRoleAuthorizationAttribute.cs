using System;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Models.Core;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
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
        /// ✅ CRITICAL FIX: Override OnAuthorization to support AllowAnonymous
        /// این متد قبل از AuthorizeCore فراخوانی می‌شود و AllowAnonymous را check می‌کند
        /// طبق: APPOINTMENT_BOOKING_AUTHORIZATION_FIX_PLAN.md
        /// </summary>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // ✅ Check for AllowAnonymous attribute on action or controller
            // این باید قبل از AuthorizeCore check شود
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) ||
                filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
            {
                var requestPath = filterContext.HttpContext?.Request?.Url?.PathAndQuery ?? "NULL";
                _log.Debug("✅ [PatientRoleAuthorization] AllowAnonymous detected - skipping authorization for path: {Path}", requestPath);
                return; // Skip authorization - AllowAnonymous takes precedence
            }

            // ✅ If no AllowAnonymous, proceed with normal authorization
            base.OnAuthorization(filterContext);
        }

        /// <summary>
        /// بررسی احراز هویت و نقش Patient
        /// ✅ CRITICAL FIX: استفاده از OWIN Context برای اطمینان از sync شدن authentication state
        /// </summary>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            try
            {
                // ✅ DEBUGGING: Log request details
                var requestPath = httpContext.Request?.Url?.PathAndQuery ?? "NULL";
                _log.Debug("🔍 [PatientRoleAuthorization] Checking authorization for path: {Path}", requestPath);
                
                // ✅ CRITICAL FIX: استفاده از OWIN Context برای اطمینان از sync شدن authentication state
                // OWIN middleware ممکن است authentication state را set کرده باشد اما HttpContext.User sync نشده باشد
                var owinContext = httpContext.GetOwinContext();
                var owinUser = owinContext?.Authentication?.User;
                
                // ✅ Check both HttpContext.User and OWIN User
                var httpContextAuthenticated = httpContext?.User?.Identity != null && httpContext.User.Identity.IsAuthenticated;
                var owinAuthenticated = owinUser?.Identity != null && owinUser.Identity.IsAuthenticated;
                
                if (!httpContextAuthenticated && !owinAuthenticated)
                {
                    _log.Debug("کاربر احراز هویت نشده است (HttpContext: {HttpAuth}, OWIN: {OwinAuth}) - دسترسی به بخش Patient رد شد - Path: {Path}", 
                        httpContextAuthenticated, owinAuthenticated, requestPath);
                    return false;
                }

                // ✅ Use OWIN User if available (more reliable), otherwise use HttpContext.User
                var identity = owinAuthenticated ? owinUser.Identity : httpContext.User.Identity;
                var userId = identity.GetUserId();
                var userName = identity.Name;
                
                _log.Debug("🔍 [PatientRoleAuthorization] User authenticated - UserId: {UserId}, UserName: {UserName}, Source: {Source}", 
                    userId, userName, owinAuthenticated ? "OWIN" : "HttpContext");

                // ✅ بررسی نقش Patient - استفاده از OWIN User اگر available باشد
                var isPatient = owinAuthenticated 
                    ? owinUser.IsInRole(AppRoles.Patient)
                    : httpContext.User.IsInRole(AppRoles.Patient);
                
                if (!isPatient)
                {
                    _log.Warning(
                        "کاربر با شناسه {UserId} و نام {UserName} که نقش Patient ندارد، تلاش برای دسترسی به بخش Patient کرد - Path: {Path}",
                        userId,
                        userName,
                        requestPath);
                }
                else
                {
                    _log.Debug(
                        "کاربر با شناسه {UserId} و نقش Patient مجوز دسترسی دارد - Path: {Path}",
                        userId,
                        requestPath);
                }

                return isPatient;
            }
            catch (Exception ex)
            {
                var requestPath = httpContext?.Request?.Url?.PathAndQuery ?? "NULL";
                _log.Error(ex, "خطا در بررسی مجوز دسترسی به بخش Patient - Path: {Path}", requestPath);
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
                
                // ✅ CRITICAL FIX: استفاده از OWIN Context برای اطمینان از sync شدن authentication state
                // باید همان منطق AuthorizeCore را استفاده کنیم
                var owinContext = httpContext.GetOwinContext();
                var owinUser = owinContext?.Authentication?.User;
                var httpContextAuthenticated = httpContext?.User?.Identity != null && httpContext.User.Identity.IsAuthenticated;
                var owinAuthenticated = owinUser?.Identity != null && owinUser.Identity.IsAuthenticated;
                
                // اگر کاربر احراز هویت نشده است (نه در HttpContext و نه در OWIN)، به صفحه لاگین هدایت می‌شود
                if (!httpContextAuthenticated && !owinAuthenticated)
                {
                    // ✅ CRITICAL FIX: استفاده از RedirectResult با URL مستقیم برای اطمینان از route resolution صحیح
                    var loginUrl = "/Account/Login";
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        loginUrl += "?returnUrl=" + HttpUtility.UrlEncode(returnUrl);
                    }
                    
                    filterContext.Result = new RedirectResult(loginUrl);
                    
                    _log.Debug("کاربر احراز هویت نشده (HttpContext: {HttpAuth}, OWIN: {OwinAuth}) - هدایت به صفحه لاگین با returnUrl: {ReturnUrl}", 
                        httpContextAuthenticated, owinAuthenticated, returnUrl);
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

                // ✅ CRITICAL FIX: اگر کاربر authenticate شده اما نقش Patient ندارد،
                // نباید به Login redirect کنیم (چون باعث redirect loop می‌شود)
                // به جای آن، به Home redirect می‌کنیم با پیام خطا
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Index" },
                        { "area", "" }
                    });
                
                // ✅ پیام خطا را در TempData قرار می‌دهیم
                if (filterContext.Controller is Controller controller)
                {
                    NotificationHelper.SetError(controller.TempData, 
                        "شما مجوز دسترسی به بخش بیمار را ندارید. لطفاً با حساب کاربری بیمار وارد شوید.");
                }

                _log.Warning(
                    "کاربر احراز هویت شده اما بدون نقش Patient - هدایت به Home (جلوگیری از redirect loop). UserId: {UserId}, RequestedPath: {Path}",
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

