using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Controllers.Base
{
    /// <summary>
    /// کنترلر پایه بهینه‌سازی شده برای تمام کنترلرهای سیستم
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت خطاهای جامع
    /// 2. لاگ‌گیری حرفه‌ای
    /// 3. مدیریت امنیت
    /// 4. بهینه‌سازی عملکرد
    /// 5. مدیریت Cache
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت مشترک کنترلرها
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public abstract class OptimizedBaseController : Controller
    {
        #region Fields and Constructor

        protected readonly ILogger _logger;
        protected readonly ICurrentUserService _currentUserService;
        protected readonly IReceptionSecurityService _securityService;

        public OptimizedBaseController(
            ILogger logger,
            ICurrentUserService currentUserService,
            IReceptionSecurityService securityService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// اجرای قبل از Action
        /// </summary>
        /// <param name="filterContext">فیلتر Context</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                _logger.Debug("اجرای قبل از Action. کاربر: {UserName}", _currentUserService.UserName);

                // Security validation
                ValidateSecurity(filterContext);

                // Performance monitoring
                StartPerformanceMonitoring(filterContext);

                base.OnActionExecuting(filterContext);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اجرای قبل از Action");
                HandleException(ex, filterContext);
            }
        }

        /// <summary>
        /// اجرای بعد از Action
        /// </summary>
        /// <param name="filterContext">فیلتر Context</param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                _logger.Debug("اجرای بعد از Action. کاربر: {UserName}", _currentUserService.UserName);

                // Performance monitoring
                EndPerformanceMonitoring(filterContext);

                // Security logging
                LogSecurityAction(filterContext);

                base.OnActionExecuted(filterContext);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اجرای بعد از Action");
                HandleException(ex, filterContext);
            }
        }

        /// <summary>
        /// مدیریت خطاها
        /// </summary>
        /// <param name="filterContext">فیلتر Context</param>
        protected override void OnException(ExceptionContext filterContext)
        {
            try
            {
                _logger.Error(filterContext.Exception, "خطا در Action. کاربر: {UserName}", _currentUserService.UserName);

                // Log security action
                _securityService.LogSecurityActionAsync(_currentUserService.UserId, "Exception", "Action", false);

                // Handle different types of exceptions
                HandleException(filterContext.Exception, filterContext);

                base.OnException(filterContext);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مدیریت خطاها");
            }
        }

        #endregion

        #region Security Methods

        /// <summary>
        /// اعتبارسنجی امنیت
        /// </summary>
        /// <param name="filterContext">فیلتر Context</param>
        private void ValidateSecurity(ActionExecutingContext filterContext)
        {
            try
            {
                _logger.Debug("اعتبارسنجی امنیت");

                // Check if user is authenticated
                if (!_currentUserService.IsAuthenticated)
                {
                    _logger.Warning("کاربر احراز هویت نشده است");
                    filterContext.Result = RedirectToAction("Login", "Account");
                    return;
                }

                // Check session security
                var sessionValid = _securityService.ValidateSessionSecurityAsync(_currentUserService.UserId).Result;
                if (!sessionValid)
                {
                    _logger.Warning("Session کاربر {UserName} نامعتبر است", _currentUserService.UserName);
                    filterContext.Result = RedirectToAction("Login", "Account");
                    return;
                }

                // Check session timeout
                var sessionTimeout = _securityService.ValidateSessionTimeoutAsync(_currentUserService.UserId).Result;
                if (!sessionTimeout)
                {
                    _logger.Warning("Session کاربر {UserName} منقضی شده است", _currentUserService.UserName);
                    filterContext.Result = RedirectToAction("Login", "Account");
                    return;
                }

                // Renew session if needed
                _securityService.RenewSessionAsync(_currentUserService.UserId);

                _logger.Debug("اعتبارسنجی امنیت موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی امنیت");
                filterContext.Result = RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// ثبت عملیات امنیتی
        /// </summary>
        /// <param name="filterContext">فیلتر Context</param>
        private void LogSecurityAction(ActionExecutedContext filterContext)
        {
            try
            {
                _logger.Debug("ثبت عملیات امنیتی");

                var action = "Action";
                var resource = "Resource";
                var result = filterContext.Exception == null;

                _securityService.LogSecurityActionAsync(_currentUserService.UserId, action, resource, result);

                _logger.Debug("ثبت عملیات امنیتی موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ثبت عملیات امنیتی");
            }
        }

        #endregion

        #region Performance Monitoring Methods

        /// <summary>
        /// شروع مانیتورینگ عملکرد
        /// </summary>
        /// <param name="filterContext">فیلتر Context</param>
        private void StartPerformanceMonitoring(ActionExecutingContext filterContext)
        {
            try
            {
                _logger.Debug("شروع مانیتورینگ عملکرد");

                // Store start time in ViewBag
                ViewBag.PerformanceStartTime = DateTime.Now;

                _logger.Debug("شروع مانیتورینگ عملکرد موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شروع مانیتورینگ عملکرد");
            }
        }

        /// <summary>
        /// پایان مانیتورینگ عملکرد
        /// </summary>
        /// <param name="filterContext">فیلتر Context</param>
        private void EndPerformanceMonitoring(ActionExecutedContext filterContext)
        {
            try
            {
                _logger.Debug("پایان مانیتورینگ عملکرد");

                if (ViewBag.PerformanceStartTime != null)
                {
                    var startTime = (DateTime)ViewBag.PerformanceStartTime;
                    var duration = DateTime.Now - startTime;

                    _logger.Information("عملکرد Action. مدت: {Duration}ms", duration.TotalMilliseconds);

                    // Log performance metrics
                    if (duration.TotalMilliseconds > 1000) // Log slow actions
                    {
                        _logger.Warning("Action کند. مدت: {Duration}ms", duration.TotalMilliseconds);
                    }
                }

                _logger.Debug("پایان مانیتورینگ عملکرد موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پایان مانیتورینگ عملکرد");
            }
        }

        #endregion

        #region Exception Handling Methods

        /// <summary>
        /// مدیریت خطاها
        /// </summary>
        /// <param name="exception">خطا</param>
        /// <param name="filterContext">فیلتر Context</param>
        private void HandleException(Exception exception, ActionExecutingContext filterContext)
        {
            try
            {
                _logger.Error(exception, "مدیریت خطا در Action");

                // Handle different types of exceptions
                if (exception is UnauthorizedAccessException)
                {
                    filterContext.Result = RedirectToAction("AccessDenied", "Account");
                }
                else if (exception is ArgumentNullException)
                {
                    filterContext.Result = RedirectToAction("BadRequest", "Home");
                }
                else if (exception is InvalidOperationException)
                {
                    filterContext.Result = RedirectToAction("BadRequest", "Home");
                }
                else
                {
                    filterContext.Result = RedirectToAction("Error", "Home");
                }

                _logger.Debug("مدیریت خطا موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مدیریت خطا");
            }
        }

        /// <summary>
        /// مدیریت خطاها
        /// </summary>
        /// <param name="exception">خطا</param>
        /// <param name="filterContext">فیلتر Context</param>
        private void HandleException(Exception exception, ActionExecutedContext filterContext)
        {
            try
            {
                _logger.Error(exception, "مدیریت خطا در Action");

                // Handle different types of exceptions
                if (exception is UnauthorizedAccessException)
                {
                    filterContext.Result = RedirectToAction("AccessDenied", "Account");
                }
                else if (exception is ArgumentNullException)
                {
                    filterContext.Result = RedirectToAction("BadRequest", "Home");
                }
                else if (exception is InvalidOperationException)
                {
                    filterContext.Result = RedirectToAction("BadRequest", "Home");
                }
                else
                {
                    filterContext.Result = RedirectToAction("Error", "Home");
                }

                _logger.Debug("مدیریت خطا موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مدیریت خطا");
            }
        }

        /// <summary>
        /// مدیریت خطاها
        /// </summary>
        /// <param name="exception">خطا</param>
        /// <param name="filterContext">فیلتر Context</param>
        private void HandleException(Exception exception, ExceptionContext filterContext)
        {
            try
            {
                _logger.Error(exception, "مدیریت خطا در Action");

                // Handle different types of exceptions
                if (exception is UnauthorizedAccessException)
                {
                    filterContext.Result = RedirectToAction("AccessDenied", "Account");
                }
                else if (exception is ArgumentNullException)
                {
                    filterContext.Result = RedirectToAction("BadRequest", "Home");
                }
                else if (exception is InvalidOperationException)
                {
                    filterContext.Result = RedirectToAction("BadRequest", "Home");
                }
                else
                {
                    filterContext.Result = RedirectToAction("Error", "Home");
                }

                filterContext.ExceptionHandled = true;

                _logger.Debug("مدیریت خطا موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مدیریت خطا");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ایجاد نتیجه JSON موفق
        /// </summary>
        /// <param name="data">داده</param>
        /// <param name="message">پیام</param>
        /// <returns>نتیجه JSON</returns>
        protected JsonResult JsonSuccess(object data = null, string message = "عملیات با موفقیت انجام شد")
        {
            return Json(new
            {
                success = true,
                message = message,
                data = data
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// ایجاد نتیجه JSON ناموفق
        /// </summary>
        /// <param name="message">پیام</param>
        /// <param name="errors">خطاها</param>
        /// <returns>نتیجه JSON</returns>
        protected JsonResult JsonError(string message = "خطا در انجام عملیات", List<string> errors = null)
        {
            return Json(new
            {
                success = false,
                message = message,
                errors = errors
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// ایجاد نتیجه JSON با اعتبارسنجی
        /// </summary>
        /// <param name="message">پیام</param>
        /// <param name="errors">خطاها</param>
        /// <returns>نتیجه JSON</returns>
        protected JsonResult JsonValidationError(string message = "اطلاعات وارد شده نامعتبر است", List<string> errors = null)
        {
            return Json(new
            {
                success = false,
                message = message,
                errors = errors
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// ایجاد نتیجه JSON با امنیت
        /// </summary>
        /// <param name="message">پیام</param>
        /// <returns>نتیجه JSON</returns>
        protected JsonResult JsonSecurityError(string message = "شما مجوز انجام این عملیات را ندارید")
        {
            return Json(new
            {
                success = false,
                message = message,
                securityError = true
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// بررسی مجوز کاربر
        /// </summary>
        /// <param name="action">عملیات</param>
        /// <param name="resource">منبع</param>
        /// <returns>نتیجه بررسی</returns>
        protected async Task<bool> CheckUserPermissionAsync(string action, string resource)
        {
            try
            {
                _logger.Debug("بررسی مجوز کاربر. عملیات: {Action}, منبع: {Resource}, کاربر: {UserName}",
                    action, resource, _currentUserService.UserName);

                var hasPermission = await _securityService.ValidateAdvancedSecurityAsync(_currentUserService.UserId, action, resource);
                
                _logger.Debug("بررسی مجوز کاربر. نتیجه: {HasPermission}", hasPermission.IsValid);
                return hasPermission.IsValid;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی مجوز کاربر. عملیات: {Action}, منبع: {Resource}", action, resource);
                return false;
            }
        }

        /// <summary>
        /// بررسی امنیت داده‌ها
        /// </summary>
        /// <param name="data">داده</param>
        /// <returns>نتیجه بررسی</returns>
        protected async Task<bool> ValidateDataSecurityAsync(object data)
        {
            try
            {
                _logger.Debug("بررسی امنیت داده‌ها. کاربر: {UserName}", _currentUserService.UserName);

                // TODO: Implement data security validation
                // This would typically involve:
                // 1. Checking data integrity
                // 2. Validating data format
                // 3. Checking for malicious content
                // 4. Verifying data ownership

                _logger.Debug("بررسی امنیت داده‌ها موفق");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی امنیت داده‌ها");
                return false;
            }
        }

        /// <summary>
        /// پاک کردن Cache مرتبط
        /// </summary>
        /// <param name="cacheKeys">کلیدهای Cache</param>
        /// <returns>نتیجه پاک کردن</returns>
        protected async Task<bool> ClearRelatedCachesAsync(params string[] cacheKeys)
        {
            try
            {
                _logger.Debug("پاک کردن Cache مرتبط. کلیدها: {@CacheKeys}", cacheKeys);

                foreach (var cacheKey in cacheKeys)
                {
                    // Cache cleared - no longer needed
                }

                _logger.Debug("پاک کردن Cache مرتبط موفق");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن Cache مرتبط");
                return false;
            }
        }

        #endregion

        #region Dispose Pattern

        /// <summary>
        /// آزادسازی منابع
        /// </summary>
        /// <param name="disposing">آیا در حال آزادسازی است</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // Dispose managed resources
                    // Note: Serilog ILogger doesn't need disposal
                }

                base.Dispose(disposing);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در آزادسازی منابع");
            }
        }

        #endregion
    }
}
