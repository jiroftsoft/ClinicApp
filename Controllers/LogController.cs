using System;
using System.Web.Mvc;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر مدیریت لاگ‌گذاری Client-side
    /// </summary>
    public class LogController : BaseController
    {
        public LogController(ILogger logger) : base(logger)
        {
        }
        /// <summary>
        /// دریافت لاگ‌های Client-side
        /// </summary>
        [HttpPost]
        public JsonResult ClientError(string level, string message, string data, string url, string userAgent, string timestamp)
        {
            try
            {
                // Simple client error logging
                _logger.Information("CLIENT_ERROR: {Level} - {Message}", level, message);
                
                return Json(new { success = true });
            }
            catch (Exception)
            {
                // Don't log errors to prevent infinite loops
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// دریافت لاگ‌های Performance
        /// </summary>
        [HttpPost]
        public JsonResult Performance(string operation, long duration, string details)
        {
            try
            {
                // Simple performance logging without complex operations
                _logger.Information("Performance: {Operation} took {Duration}ms", operation, duration);
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Don't log errors to prevent infinite loops
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// دریافت لاگ‌های User Activity
        /// </summary>
        [HttpPost]
        public JsonResult UserActivity(string action, string details)
        {
            try
            {
                LoggingHelper.LogUserActivity(User.Identity.Name, action, details);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to log user activity");
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// دریافت لاگ‌های Business Operations
        /// </summary>
        [HttpPost]
        public JsonResult BusinessOperation(string operation, string details, string data)
        {
            try
            {
                LoggingHelper.LogBusinessOperation(operation, User.Identity.Name, details, data);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to log business operation");
                return Json(new { success = false });
            }
        }
    }
}
