using System;
using System.Collections.Generic;
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
    /// کنترلر مدیریت ارزیابی‌های تریاژ - SRP محور
    /// </summary>
    public class TriageController : BaseController
    {
        private readonly ITriageService _triageService;
        private readonly ITriageQueueService _queueService;
        private readonly ILogger _logger;

        public TriageController(
            ITriageService triageService,
            ITriageQueueService queueService,
            ILogger logger) : base(logger)
        {
            _triageService = triageService;
            _queueService = queueService;
            _logger = logger;
        }

        #region ارزیابی تریاژ (Triage Assessment)

        /// <summary>
        /// لیست ارزیابی‌های تریاژ
        /// </summary>
        public async Task<ActionResult> Index(int? departmentId, string status = "All", int page = 1)
        {
            try
            {
                TriageStatus? statusEnum = null;
                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    if (Enum.TryParse<TriageStatus>(status, out var parsedStatus))
                    {
                        statusEnum = parsedStatus;
                    }
                }

                var assessments = await _triageService.GetTriageAssessmentsAsync(
                    status: statusEnum,
                    pageNumber: page,
                    pageSize: 20);

                if (!assessments.Success)
                {
                    TempData["Error"] = assessments.Message;
                    return View(new List<TriageAssessment>());
                }

                ViewBag.DepartmentId = departmentId;
                ViewBag.Status = status;
                ViewBag.Page = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)assessments.Data.TotalCount / 20);

                return View(assessments.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست ارزیابی‌های تریاژ");
                TempData["Error"] = "خطا در دریافت لیست ارزیابی‌ها";
                return View(new List<TriageAssessment>());
            }
        }

        /// <summary>
        /// جزئیات ارزیابی تریاژ
        /// </summary>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var assessment = await _triageService.GetTriageAssessmentAsync(id);
                if (!assessment.Success)
                {
                    TempData["Error"] = assessment.Message;
                    return RedirectToAction("Index");
                }

                // دریافت خلاصه
                var summary = await _triageService.GetSummaryAsync(id);
                if (summary.Success)
                {
                    ViewBag.Summary = summary.Data;
                }

                // دریافت پایش‌های مجدد
                var reassessments = await _triageService.GetReassessmentsAsync(id);
                if (reassessments.Success)
                {
                    ViewBag.Reassessments = reassessments.Data;
                }

                return View(assessment.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات ارزیابی تریاژ {AssessmentId}", id);
                TempData["Error"] = "خطا در دریافت جزئیات ارزیابی";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// فرم ایجاد ارزیابی تریاژ
        /// </summary>
        public ActionResult Create(int patientId)
        {
            var model = new TriageCreateViewModel
            {
                PatientId = patientId,
                PatientFullName = "نام بیمار", // TODO: دریافت از سرویس بیمار
                ArrivalAt = DateTime.UtcNow,
                TriageStartAt = DateTime.UtcNow
            };

            return View(model);
        }

        /// <summary>
        /// ایجاد ارزیابی تریاژ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TriageCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // تبدیل ViewModel به Entity
                var vitalSigns = new TriageVitalSigns
                {
                    SystolicBP = model.SBP,
                    DiastolicBP = model.DBP,
                    HeartRate = model.HR,
                    RespiratoryRate = model.RR,
                    Temperature = model.TempC != null ? (decimal?)model.TempC : null,
                    OxygenSaturation = model.SpO2,
                    GcsE = model.GcsE,
                    GcsV = model.GcsV,
                    GcsM = model.GcsM,
                    OnOxygen = model.OnOxygen,
                    OxygenDevice = model.OxygenDevice,
                    O2FlowLpm = model.O2FlowLpm,
                    MeasurementTime = DateTime.UtcNow
                };

                var result = await _triageService.CreateTriageAssessmentAsync(
                    model.PatientId,
                    model.ChiefComplaint,
                    vitalSigns);

                if (result.Success)
                {
                    TempData["Success"] = "ارزیابی تریاژ با موفقیت ایجاد شد";
                    return RedirectToAction("Details", new { id = result.Data.TriageAssessmentId });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد ارزیابی تریاژ");
                ModelState.AddModelError("", "خطا در ایجاد ارزیابی تریاژ");
                return View(model);
            }
        }

        /// <summary>
        /// فرم ویرایش ارزیابی تریاژ
        /// </summary>
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var assessment = await _triageService.GetTriageAssessmentAsync(id);
                if (!assessment.Success)
                {
                    TempData["Error"] = assessment.Message;
                    return RedirectToAction("Index");
                }

                var model = new TriageEditViewModel
                {
                    AssessmentId = assessment.Data.TriageAssessmentId,
                    PatientId = assessment.Data.PatientId,
                    PatientFullName = "نام بیمار", // TODO: دریافت از سرویس بیمار
                    ChiefComplaint = assessment.Data.ChiefComplaint,
                    AssessmentNotes = assessment.Data.AssessmentNotes,
                    Level = assessment.Data.Level,
                    Priority = assessment.Data.Priority,
                    Status = assessment.Data.Status,
                    ArrivalAt = assessment.Data.ArrivalAt,
                    TriageStartAt = assessment.Data.TriageStartAt,
                    TriageEndAt = assessment.Data.TriageEndAt,
                    IsOpen = assessment.Data.IsOpen,
                    IsolationRequired = assessment.Data.Isolation != null && assessment.Data.Isolation != IsolationType.None,
                    Isolation = assessment.Data.Isolation,
                    RedFlag_Sepsis = assessment.Data.RedFlag_Sepsis,
                    RedFlag_Stroke = assessment.Data.RedFlag_Stroke,
                    RedFlag_ACS = assessment.Data.RedFlag_ACS,
                    RedFlag_Trauma = assessment.Data.RedFlag_Trauma,
                    IsPregnant = assessment.Data.IsPregnant,
                    RecommendedDepartmentId = assessment.Data.RecommendedDepartmentId,
                    RecommendedDoctorId = assessment.Data.RecommendedDoctorId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت فرم ویرایش ارزیابی تریاژ {AssessmentId}", id);
                TempData["Error"] = "خطا در دریافت فرم ویرایش";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ویرایش ارزیابی تریاژ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TriageEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _triageService.UpdateTriageAssessmentAsync(
                    model.AssessmentId,
                    model.Level,
                    model.Priority,
                    model.AssessmentNotes);

                if (result.Success)
                {
                    TempData["Success"] = "ارزیابی تریاژ با موفقیت به‌روزرسانی شد";
                    return RedirectToAction("Details", new { id = model.AssessmentId });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش ارزیابی تریاژ {AssessmentId}", model.AssessmentId);
                ModelState.AddModelError("", "خطا در ویرایش ارزیابی تریاژ");
                return View(model);
            }
        }

        /// <summary>
        /// تکمیل ارزیابی تریاژ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Complete(int id, int? recommendedDepartmentId = null, int? recommendedDoctorId = null)
        {
            try
            {
                var result = await _triageService.CompleteTriageAssessmentAsync(
                    id,
                    recommendedDepartmentId,
                    recommendedDoctorId);

                if (result.Success)
                {
                    TempData["Success"] = "ارزیابی تریاژ با موفقیت تکمیل شد";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تکمیل ارزیابی تریاژ {AssessmentId}", id);
                TempData["Error"] = "خطا در تکمیل ارزیابی تریاژ";
                return RedirectToAction("Details", new { id });
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// دریافت خلاصه ارزیابی (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetSummary(int assessmentId)
        {
            try
            {
                var result = await _triageService.GetSummaryAsync(assessmentId);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خلاصه ارزیابی {AssessmentId}", assessmentId);
                return Json(new { success = false, message = "خطا در دریافت خلاصه ارزیابی" });
            }
        }

        /// <summary>
        /// بررسی قابلیت لینک به پذیرش (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CanLinkToReception(int assessmentId)
        {
            try
            {
                var result = await _triageService.CanLinkToReceptionAsync(assessmentId);
                return Json(new { success = result.Success, data = result.Data, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی قابلیت لینک به پذیرش {AssessmentId}", assessmentId);
                return Json(new { success = false, message = "خطا در بررسی قابلیت لینک" });
            }
        }

        #endregion
    }
}
