using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers.Base
{
    /// <summary>
    /// Base Controller برای تمام Patient Area Controllers
    /// طبق appointment_controller_review.md - فاز 1
    /// </summary>
    public abstract class BasePatientController : Controller
    {
        protected readonly ILogger _logger;
        protected readonly ICurrentUserService _currentUserService;

        protected BasePatientController(
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? 
                throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// دریافت شناسه بیمار از کاربر فعلی
        /// </summary>
        protected async Task<int?> GetCurrentPatientIdAsync()
        {
            try
            {
                var patient = await _currentUserService.GetPatientInfoAsync();
                return patient?.PatientId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت شناسه بیمار");
                return null;
            }
        }

        /// <summary>
        /// JSON Result موفق
        /// </summary>
        protected JsonResult SuccessJsonResult(object data, string message = null)
        {
            // ✅ استفاده از if-else به جای conditional expression برای جلوگیری از Type Inference Error
            if (message != null)
            {
                return Json(new { success = true, data, message }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// JSON Result خطا
        /// </summary>
        protected JsonResult ErrorJsonResult(string message)
        {
            return Json(new { success = false, message }, JsonRequestBehavior.AllowGet);
        }
    }
}

