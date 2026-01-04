using System;
using System.Linq;
using System.Security.Claims;
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
    /// ✅ MODERN STANDARD: Claims-Based Authorization برای بخش Patient
    /// این روش استاندارد امروزی است که در تمام پروژه‌های مدرن استفاده می‌شود
    /// 
    /// مزایا:
    /// - استفاده از Claims به جای Roles (انعطاف‌پذیرتر)
    /// - پشتیبانی از Policy-Based Authorization
    /// - سازگار با ASP.NET Core Identity
    /// - تست‌پذیر و قابل نگهداری
    /// 
    /// طبق: DEVELOPMENT_CONTRACT.md - Strongly-Typed Development
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PatientClaimAuthorizationAttribute : AuthorizeAttribute
    {
        private static readonly ILogger _log = Log.ForContext<PatientClaimAuthorizationAttribute>();

        // ✅ Claim Types - Strongly-Typed Constants
        private const string PatientRoleClaim = ClaimTypes.Role;
        private const string PatientRoleValue = AppRoles.Patient;
        private const string PatientIdClaim = "PatientId"; // Custom claim for Patient ID

        /// <summary>
        /// ✅ Override OnAuthorization to support AllowAnonymous
        /// </summary>
        public override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
        {
            // ✅ Check for AllowAnonymous attribute
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) ||
                filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
            {
                var requestPath = filterContext.HttpContext?.Request?.Url?.PathAndQuery ?? "NULL";
                _log.Debug("✅ [PatientClaimAuthorization] AllowAnonymous detected - skipping authorization for path: {Path}", requestPath);
                return;
            }

            base.OnAuthorization(filterContext);
        }

        /// <summary>
        /// ✅ MODERN: Claims-Based Authorization
        /// بررسی Claims به جای Roles - روش استاندارد امروزی
        /// </summary>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            try
            {
                var requestPath = httpContext.Request?.Url?.PathAndQuery ?? "NULL";
                _log.Debug("🔍 [PatientClaimAuthorization] Checking Claims-Based authorization for path: {Path}", requestPath);

                // ✅ MODERN: Get Claims from OWIN Context (most reliable)
                var owinContext = httpContext.GetOwinContext();
                var owinUser = owinContext?.Authentication?.User;
                
                // ✅ Check authentication state
                var httpContextAuthenticated = httpContext?.User?.Identity != null && httpContext.User.Identity.IsAuthenticated;
                var owinAuthenticated = owinUser?.Identity != null && owinUser.Identity.IsAuthenticated;
                
                if (!httpContextAuthenticated && !owinAuthenticated)
                {
                    _log.Debug("کاربر احراز هویت نشده است - دسترسی به بخش Patient رد شد - Path: {Path}", requestPath);
                    return false;
                }

                // ✅ Use OWIN User if available (more reliable), otherwise use HttpContext.User
                var identity = owinAuthenticated ? owinUser.Identity as ClaimsIdentity : httpContext.User.Identity as ClaimsIdentity;
                
                if (identity == null)
                {
                    _log.Warning("❌ [PatientClaimAuthorization] Identity is not ClaimsIdentity - Path: {Path}", requestPath);
                    return false;
                }

                var userId = identity.GetUserId();
                var userName = identity.Name;
                
                _log.Debug("🔍 [PatientClaimAuthorization] User authenticated - UserId: {UserId}, UserName: {UserName}, Source: {Source}", 
                    userId, userName, owinAuthenticated ? "OWIN" : "HttpContext");

                // ✅ MODERN: Check Claims instead of Roles
                // Method 1: Check Role Claim (ClaimTypes.Role)
                var hasPatientRoleClaim = identity.HasClaim(PatientRoleClaim, PatientRoleValue);
                
                // Method 2: Check using IsInRole (backward compatible)
                var isInPatientRole = identity.IsInRole(PatientRoleValue);
                
                // Method 3: Check Custom PatientId Claim (if exists)
                var hasPatientIdClaim = identity.HasClaim(c => c.Type == PatientIdClaim);

                var isAuthorized = hasPatientRoleClaim || isInPatientRole;

                if (isAuthorized)
                {
                    _log.Debug(
                        "✅ کاربر با شناسه {UserId} و Claims مناسب مجوز دسترسی دارد - Path: {Path}, HasRoleClaim: {HasRoleClaim}, IsInRole: {IsInRole}, HasPatientIdClaim: {HasPatientId}",
                        userId, requestPath, hasPatientRoleClaim, isInPatientRole, hasPatientIdClaim);
                }
                else
                {
                    _log.Warning(
                        "❌ کاربر با شناسه {UserId} و نام {UserName} که Claim Patient ندارد، تلاش برای دسترسی به بخش Patient کرد - Path: {Path}",
                        userId, userName, requestPath);
                }

                return isAuthorized;
            }
            catch (Exception ex)
            {
                var requestPath = httpContext?.Request?.Url?.PathAndQuery ?? "NULL";
                _log.Error(ex, "❌ خطا در بررسی Claims-Based authorization به بخش Patient - Path: {Path}", requestPath);
                return false; // Fail-safe
            }
        }

        /// <summary>
        /// ✅ MODERN: Handle unauthorized requests with proper redirect logic
        /// </summary>
        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            try
            {
                var httpContext = filterContext.HttpContext;
                var returnUrl = httpContext.Request.Url?.PathAndQuery;

                // ✅ Get authentication state from OWIN
                var owinContext = httpContext.GetOwinContext();
                var owinUser = owinContext?.Authentication?.User;
                var httpContextAuthenticated = httpContext?.User?.Identity != null && httpContext.User.Identity.IsAuthenticated;
                var owinAuthenticated = owinUser?.Identity != null && owinUser.Identity.IsAuthenticated;

                // ✅ If user is not authenticated, redirect to Login
                if (!httpContextAuthenticated && !owinAuthenticated)
                {
                    var loginUrl = "/Account/Login";
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        loginUrl += "?returnUrl=" + HttpUtility.UrlEncode(returnUrl);
                    }
                    
                    filterContext.Result = new RedirectResult(loginUrl);
                    
                    _log.Debug("کاربر احراز هویت نشده - هدایت به صفحه لاگین با returnUrl: {ReturnUrl}", returnUrl);
                    return;
                }

                // ✅ If user is authenticated but doesn't have Patient claim, redirect to Home
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            success = false,
                            message = "شما مجوز دسترسی به بخش بیمار را ندارید. لطفاً با حساب کاربری بیمار وارد شوید.",
                            unauthorized = true
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    filterContext.HttpContext.Response.StatusCode = 403;
                    
                    _log.Warning("کاربر احراز هویت شده اما بدون Claim Patient - درخواست AJAX رد شد. UserId: {UserId}",
                        httpContext.User.Identity.GetUserId());
                    return;
                }

                // ✅ Redirect to Home with error message
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Index" },
                        { "area", "" }
                    });
                
                if (filterContext.Controller is Controller controller)
                {
                    NotificationHelper.SetError(controller.TempData, 
                        "شما مجوز دسترسی به بخش بیمار را ندارید. لطفاً با حساب کاربری بیمار وارد شوید.");
                }

                _log.Warning(
                    "کاربر احراز هویت شده اما بدون Claim Patient - هدایت به Home. UserId: {UserId}, RequestedPath: {Path}",
                    httpContext.User.Identity.GetUserId(), returnUrl);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ خطا در مدیریت پاسخ عدم مجوز دسترسی به بخش Patient");
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

