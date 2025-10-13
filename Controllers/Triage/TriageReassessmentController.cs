using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Triage;
using ClinicApp.ViewModels.Triage;
using Serilog;

namespace ClinicApp.Controllers.Triage
{
    /// <summary>
    /// کنترلر مدیریت پایش مجدد تریاژ - SRP محور
    /// </summary>
    [Authorize]
    public class TriageReassessmentController : BaseController
    {
        private readonly ITriageService _triageService;
        private readonly ILogger _logger;

        public TriageReassessmentController(
            ITriageService triageService,
            ILogger logger) : base(logger)
        {
            _triageService = triageService;
            _logger = logger;
        }

        #region پایش مجدد (Reassessment)

        /// <summary>
        /// لیست پایش‌های مجدد
        /// </summary>
        public async Task<ActionResult> Index(int assessmentId)
        {
            try
            {
                var reassessments = await _triageService.GetReassessmentsAsync(assessmentId);
                if (!reassessments.Success)
                {
                    TempData["Error"] = reassessments.Message;
                    return View(new List<TriageReassessment>());
                }

                ViewBag.AssessmentId = assessmentId;
                return View(reassessments.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پایش‌های مجدد {AssessmentId}", assessmentId);
                TempData["Error"] = "خطا در دریافت لیست پایش‌های مجدد";
                return View(new List<TriageReassessment>());
            }
        }

        /// <summary>
        /// فرم ایجاد پایش مجدد
        /// </summary>
        public ActionResult Create(int assessmentId)
        {
            var model = new TriageReassessmentViewModel
            {
                TriageAssessmentId = assessmentId,
                At = DateTime.UtcNow,
                Reason = ReassessmentReason.Routine
            };

            return View(model);
        }

        /// <summary>
        /// ایجاد پایش مجدد
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TriageReassessmentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // تبدیل ViewModel به DTO
                var dto = new TriageReassessmentDto
                {
                    AtUtc = model.At,
                    NewLevel = (TriageLevel)model.NewLevel,
                    Changes = model.Changes,
                    Actions = model.Actions,
                    Reason = model.Reason.ToString(),
                    Vitals = model.HasVitals ? System.Text.Json.JsonSerializer.Serialize(new TriageVitalSignsDto
                    {
                        SBP = model.SBP,
                        DBP = model.DBP,
                        HR = model.HR,
                        RR = model.RR,
                        TempC = model.TempC,
                        SpO2 = model.SpO2,
                        GcsE = model.GcsE,
                        GcsV = model.GcsV,
                        GcsM = model.GcsM,
                        OnOxygen = model.OnOxygen,
                        OxygenDevice = model.OxygenDevice,
                        O2FlowLpm = model.O2FlowLpm,
                        Notes = model.VitalSignsNotes
                    }) : null
                };

                var result = await _triageService.CreateReassessmentAsync(model.TriageAssessmentId, dto);
                if (result.Success)
                {
                    TempData["Success"] = "پایش مجدد با موفقیت ثبت شد";
                    return RedirectToAction("Index", new { assessmentId = model.TriageAssessmentId });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پایش مجدد {AssessmentId}", model.TriageAssessmentId);
                ModelState.AddModelError("", "خطا در ایجاد پایش مجدد");
                return View(model);
            }
        }

        /// <summary>
        /// جزئیات پایش مجدد
        /// </summary>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var reassessment = await _triageService.GetReassessmentAsync(id);
                if (!reassessment.Success)
                {
                    TempData["Error"] = reassessment.Message;
                    return RedirectToAction("Index");
                }

                return View(reassessment.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پایش مجدد {ReassessmentId}", id);
                TempData["Error"] = "خطا در دریافت جزئیات پایش مجدد";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// دریافت لیست پایش‌های مجدد (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetReassessments(int assessmentId)
        {
            try
            {
                var result = await _triageService.GetReassessmentsAsync(assessmentId);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پایش‌های مجدد {AssessmentId}", assessmentId);
                return Json(new { success = false, message = "خطا در دریافت لیست پایش‌های مجدد" });
            }
        }

        /// <summary>
        /// ایجاد پایش مجدد (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CreateReassessment(TriageReassessmentDto dto)
        {
            try
            {
                var result = await _triageService.CreateReassessmentAsync(dto.TriageAssessmentId, dto);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پایش مجدد {AssessmentId}", dto.TriageAssessmentId);
                return Json(new { success = false, message = "خطا در ایجاد پایش مجدد" });
            }
        }

        /// <summary>
        /// دریافت جزئیات پایش مجدد (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetReassessmentDetails(int reassessmentId)
        {
            try
            {
                var result = await _triageService.GetReassessmentAsync(reassessmentId);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پایش مجدد {ReassessmentId}", reassessmentId);
                return Json(new { success = false, message = "خطا در دریافت جزئیات پایش مجدد" });
            }
        }

        #endregion
    }

    /// <summary>
    /// ViewModel برای پایش مجدد تریاژ
    /// </summary>
    public class TriageReassessmentViewModel
    {
        [Required(ErrorMessage = "شناسه ارزیابی الزامی است")]
        public int TriageAssessmentId { get; set; }

        [Required(ErrorMessage = "زمان پایش مجدد الزامی است")]
        [Display(Name = "زمان پایش مجدد")]
        public DateTime At { get; set; }

        [Display(Name = "سطح جدید")]
        public int? NewLevel { get; set; }

        [Required(ErrorMessage = "تغییرات الزامی است")]
        [StringLength(1000, ErrorMessage = "تغییرات نمی‌تواند بیش از 1000 کاراکتر باشد")]
        [Display(Name = "تغییرات")]
        public string Changes { get; set; }

        [StringLength(500, ErrorMessage = "اقدامات نمی‌تواند بیش از 500 کاراکتر باشد")]
        [Display(Name = "اقدامات")]
        public string Actions { get; set; }

        [Required(ErrorMessage = "دلیل پایش مجدد الزامی است")]
        [Display(Name = "دلیل پایش مجدد")]
        public ReassessmentReason Reason { get; set; }

        [Display(Name = "ثبت علائم حیاتی")]
        public bool HasVitals { get; set; }

        // علائم حیاتی - اختیاری
        [Display(Name = "فشار خون سیستولیک")]
        [Range(40, 260, ErrorMessage = "فشار خون سیستولیک باید بین 40 تا 260 باشد")]
        public int? SBP { get; set; }

        [Display(Name = "فشار خون دیاستولیک")]
        [Range(20, 150, ErrorMessage = "فشار خون دیاستولیک باید بین 20 تا 150 باشد")]
        public int? DBP { get; set; }

        [Display(Name = "ضربان قلب")]
        [Range(30, 220, ErrorMessage = "ضربان قلب باید بین 30 تا 220 باشد")]
        public int? HR { get; set; }

        [Display(Name = "میزان تنفس")]
        [Range(5, 60, ErrorMessage = "میزان تنفس باید بین 5 تا 60 باشد")]
        public int? RR { get; set; }

        [Display(Name = "دمای بدن (°C)")]
        [Range(30, 45, ErrorMessage = "دمای بدن باید بین 30 تا 45 درجه سانتی‌گراد باشد")]
        public double? TempC { get; set; }

        [Display(Name = "اشباع اکسیژن (%)")]
        [Range(50, 100, ErrorMessage = "اشباع اکسیژن باید بین 50 تا 100 باشد")]
        public int? SpO2 { get; set; }

        [Display(Name = "GCS Eye")]
        [Range(1, 4, ErrorMessage = "GCS Eye باید بین 1 تا 4 باشد")]
        public int? GcsE { get; set; }

        [Display(Name = "GCS Verbal")]
        [Range(1, 5, ErrorMessage = "GCS Verbal باید بین 1 تا 5 باشد")]
        public int? GcsV { get; set; }

        [Display(Name = "GCS Motor")]
        [Range(1, 6, ErrorMessage = "GCS Motor باید بین 1 تا 6 باشد")]
        public int? GcsM { get; set; }

        [Display(Name = "تحت اکسیژن")]
        public bool OnOxygen { get; set; }

        [Display(Name = "نوع دستگاه اکسیژن")]
        public OxygenDevice? OxygenDevice { get; set; }

        [Display(Name = "جریان اکسیژن (L/min)")]
        [Range(0, 50, ErrorMessage = "جریان اکسیژن باید بین 0 تا 50 لیتر در دقیقه باشد")]
        public decimal? O2FlowLpm { get; set; }

        [Display(Name = "یادداشت‌های علائم حیاتی")]
        [StringLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string VitalSignsNotes { get; set; }

        // محاسبه شده
        public int? GcsTotal => (GcsE ?? 0) + (GcsV ?? 0) + (GcsM ?? 0);
        
        public bool RequiresImmediateAttention => 
            (SpO2 < 90) || 
            (HR > 120 || HR < 50) || 
            (SBP < 90) || 
            (TempC > 39 || TempC < 35) || 
            (RR > 30 || RR < 10) ||
            (GcsTotal < 8);

        public string UrgencyLevel => RequiresImmediateAttention ? "بحرانی" : "عادی";
    }
}
