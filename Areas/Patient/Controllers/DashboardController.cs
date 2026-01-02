using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Factories;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.Patient;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// Controller برای داشبورد بیمار
    /// Single Responsibility: مدیریت نمایش داشبورد بیمار
    /// 
    /// ✅ Enterprise-Grade: AJAX-Compatible, Authorization, ServiceResult Enhanced
    /// طبق: CLINICAPP_PATIENT_DASHBOARD_BEAST_ROADMAP_PROMPT.md
    /// </summary>
    [Authorize]
    [NoCache]
    public class DashboardController : BasePatientController
    {
        private readonly IPatientDashboardService _dashboardService;

        public DashboardController(
            IPatientDashboardService dashboardService,
            ILogger logger,
            ICurrentUserService currentUserService)
            : base(logger, currentUserService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        /// <summary>
        /// ✅ BULLETPROOF: Enhanced AJAX request detection
        /// Checks multiple sources: Request.IsAjaxRequest() + Custom Header + Query String
        /// Healthcare-Grade: Must work across all ASP.NET configurations
        /// </summary>
        private bool IsAjaxRequestEnhanced()
        {
            if (Request.IsAjaxRequest())
                return true;
            
            if (Request.Headers["X-AJAX-Request"] == "true")
                return true;
            
            if (Request.QueryString["ajax"] == "1")
                return true;
            
            return false;
        }

        /// <summary>
        /// نمایش داشبورد اصلی بیمار
        /// GET: /Patient/Dashboard
        /// ✅ AJAX-Compatible: پشتیبانی از درخواست‌های AJAX بدون رفرش صفحه
        /// ✅ BULLETPROOF: Enhanced AJAX detection to prevent layout duplication
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                _logger.Information("درخواست نمایش داشبورد بیمار - UserId: {UserId}, IsAjax: {IsAjax}", 
                    userId, IsAjaxRequestEnhanced());

                // ✅ Security: دریافت patientId از auth context
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    _logger.Warning("⚠️ Dashboard access denied - patientId is null. UserId: {UserId}", 
                        userId);
                    
                    if (IsAjaxRequestEnhanced())
                    {
                        Response.StatusCode = 401;
                        return Json(new { 
                            success = false, 
                            message = "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید.",
                            code = "UNAUTHORIZED",
                            redirectUrl = "/Account/Login" // ✅ FIXED: Absolute path for cross-area navigation
                        }, JsonRequestBehavior.AllowGet);
                    }
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید.");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ AJAX Request: Return Partial View (بدون Layout) - CRITICAL for preventing layout duplication
                if (IsAjaxRequestEnhanced())
                {
                    _logger.Information("✅ Returning PartialView for AJAX request");
                    // ✅ استفاده از Factory Pattern
                    var partialViewModel = DashboardViewModelFactory.CreateEmpty();
                    return PartialView("_DashboardShell", partialViewModel);
                }

                // ✅ Normal Request: Return Full View (با Layout)
                // ✅ استفاده از Factory Pattern - طبق DEVELOPMENT_CONTRACT.md
                var viewModel = DashboardViewModelFactory.CreateEmpty();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش داشبورد بیمار");
                if (IsAjaxRequestEnhanced())
                {
                    return Json(new { 
                        success = false, 
                        message = "خطا در بارگذاری داشبورد" 
                    }, JsonRequestBehavior.AllowGet);
                }
                NotificationHelper.SetError(TempData, "خطا در بارگذاری داشبورد");
                return View(new DashboardViewModel());
            }
        }

        /// <summary>
        /// Render Partial View for AJAX requests
        /// GET: /Patient/Dashboard/RenderPartial (Changed to GET for security simplicity)
        /// </summary>
        [HttpGet]
        public ActionResult RenderPartial(string partialName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(partialName))
                {
                    return new HttpStatusCodeResult(400, "Partial name is required");
                }

                // ✅ Security: Only allow specific partials
                var allowedPartials = new[] { 
                    "_DashboardQuickStats", 
                    "_DashboardAppointmentsList", 
                    "_DashboardReceptionsList" 
                };

                if (!allowedPartials.Contains(partialName))
                {
                    return new HttpStatusCodeResult(403, "Partial not allowed");
                }

                // ✅ Read JSON data from request body
                string jsonData = null;
                using (var reader = new System.IO.StreamReader(Request.InputStream))
                {
                    jsonData = reader.ReadToEnd();
                }

                object model = null;
                if (!string.IsNullOrWhiteSpace(jsonData))
                {
                    try
                    {
                        model = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData);
                    }
                    catch
                    {
                        // If JSON parsing fails, use empty model
                    }
                }

                return PartialView(partialName, model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در render partial: {PartialName}", partialName);
                return new HttpStatusCodeResult(500, "Error rendering partial");
            }
        }
    }
}

