using System;
using System.Web.Mvc;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// Base Controller برای ماژول پذیرش
    /// </summary>
    public abstract class ReceptionBaseController : Controller
    {
        protected readonly ILogger _logger;

        protected ReceptionBaseController(ILogger logger)
        {
            _logger = logger.ForContext(GetType());
        }

        /// <summary>
        /// مدیریت خطاهای سرویس
        /// </summary>
        protected ActionResult HandleServiceError<T>(ServiceResult<T> result)
        {
            if (!result.Success)
            {
                _logger.Warning("Service error: {Message}", result.Message);
                return Json(ServiceResult<T>.Failed(result.Message), JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// مدیریت استثناها
        /// </summary>
        protected ActionResult HandleException(Exception ex, string operation, string details = null)
        {
            _logger.Error(ex, "Error in {Operation}: {Details}", operation, details);
            return Json(ServiceResult<object>.Failed($"خطا در {operation}"), JsonRequestBehavior.AllowGet);
        }
    }
}
