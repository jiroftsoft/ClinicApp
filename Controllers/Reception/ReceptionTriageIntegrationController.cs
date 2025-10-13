using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Reception;
using ClinicApp.Services.Triage;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر یکپارچه‌سازی پذیرش با تریاژ - مدیریت گردش کار تریاژ به پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. انتقال بیماران از تریاژ به پذیرش
    /// 2. مدیریت اولویت‌بندی هوشمند
    /// 3. پذیرش اورژانس
    /// 4. مدیریت صف و انتظار
    /// 5. گزارش‌گیری یکپارچه
    /// </summary>
    public class ReceptionTriageIntegrationController : BaseController
    {
        #region Fields and Constructor

        private readonly ReceptionTriageIntegrationService _integrationService;
        private readonly IReceptionService _receptionService;
        private readonly ITriageService _triageService;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionTriageIntegrationController(
            ReceptionTriageIntegrationService integrationService,
            IReceptionService receptionService,
            ITriageService triageService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _triageService = triageService ?? throw new ArgumentNullException(nameof(triageService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #endregion

        #region Triage to Reception Transfer

        /// <summary>
        /// صفحه انتقال بیماران از تریاژ به پذیرش
        /// </summary>
        /// <returns>صفحه انتقال</returns>
        [HttpGet]
        public async Task<ActionResult> TransferIndex()
        {
            try
            {
                Log.Information("📋 نمایش صفحه انتقال بیماران از تریاژ به پذیرش, کاربر: {UserName}",
                    _currentUserService.UserName);

                var model = new TriageToReceptionTransferViewModel();

                // دریافت بیماران آماده برای انتقال
                var candidatesResult = await _integrationService.GetReadyForReceptionPatientsAsync();
                if (candidatesResult.Success)
                {
                    model.ReadyPatients = candidatesResult.Data.Select(c => new ViewModels.Reception.TriageToReceptionCandidate
                    {
                        TriageAssessmentId = c.TriageAssessmentId,
                        PatientId = c.PatientId,
                        PatientFullName = c.PatientFullName,
                        TriageLevel = c.TriageLevel,
                        Priority = c.Priority,
                        ChiefComplaint = c.ChiefComplaint,
                        TriageDateTime = c.TriageDateTime,
                        EstimatedWaitTime = c.EstimatedWaitTime,
                        IsEmergency = c.IsEmergency,
                        RecommendedDepartment = c.RecommendedDepartment,
                        RecommendedDoctor = c.RecommendedDoctor
                    }).ToList();
                }
                else
                {
                    model.ReadyPatients = new List<ViewModels.Reception.TriageToReceptionCandidate>();
                    TempData["ErrorMessage"] = "خطا در دریافت بیماران آماده: " + candidatesResult.Message;
                }

                // دریافت آمار کلی
                var statsResult = await _triageService.GetTriageAssessmentsCountAsync();
                if (statsResult.Success)
                {
                    model.TotalAssessments = statsResult.Data;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ خطا در نمایش صفحه انتقال بیماران");
                TempData["ErrorMessage"] = "خطا در نمایش صفحه انتقال بیماران: " + ex.Message;
                return View(new TriageToReceptionTransferViewModel());
            }
        }

        /// <summary>
        /// انتقال بیمار از تریاژ به پذیرش
        /// </summary>
        /// <param name="triageAssessmentId">شناسه ارزیابی تریاژ</param>
        /// <param name="transferData">داده‌های انتقال</param>
        /// <returns>نتیجه انتقال</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TransferPatient(int triageAssessmentId, TriageToReceptionTransferData transferData)
        {
            try
            {
                Log.Information("🔄 انتقال بیمار از تریاژ {AssessmentId} به پذیرش, کاربر: {UserName}",
                    triageAssessmentId, _currentUserService.UserName);

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "اطلاعات وارد شده صحیح نیست";
                    return RedirectToAction("TransferIndex");
                }

                var result = await _integrationService.TransferTriageToReceptionAsync(triageAssessmentId, transferData);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "بیمار با موفقیت از تریاژ به پذیرش انتقال یافت";
                    return RedirectToAction("Details", "Reception", new { id = result.Data.ReceptionId });
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در انتقال بیمار: " + result.Message;
                    return RedirectToAction("TransferIndex");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ خطا در انتقال بیمار از تریاژ به پذیرش");
                TempData["ErrorMessage"] = "خطا در انتقال بیمار: " + ex.Message;
                return RedirectToAction("TransferIndex");
            }
        }

        /// <summary>
        /// دریافت جزئیات بیمار برای انتقال
        /// </summary>
        /// <param name="triageAssessmentId">شناسه ارزیابی تریاژ</param>
        /// <returns>جزئیات بیمار</returns>
        [HttpGet]
        public async Task<ActionResult> GetPatientDetails(int triageAssessmentId)
        {
            try
            {
                Log.Information("📋 دریافت جزئیات بیمار برای انتقال - ارزیابی {AssessmentId}",
                    triageAssessmentId);

                var assessmentResult = await _triageService.GetTriageAssessmentAsync(triageAssessmentId);
                if (!assessmentResult.Success)
                {
                    return Json(new { success = false, message = "ارزیابی تریاژ یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var assessment = assessmentResult.Data;
                var patient = assessment.Patient;

                var details = new
                {
                    success = true,
                    patientId = patient.PatientId,
                    patientName = $"{patient.FirstName} {patient.LastName}",
                    nationalCode = patient.NationalCode,
                    phoneNumber = patient.PhoneNumber,
                    triageLevel = assessment.Level.ToString(),
                    priority = assessment.Priority,
                    chiefComplaint = assessment.ChiefComplaint,
                    symptoms = assessment.Symptoms,
                    medicalHistory = assessment.MedicalHistory,
                    allergies = assessment.Allergies,
                    medications = assessment.Medications,
                    isEmergency = assessment.Level == TriageLevel.ESI1 || assessment.Level == TriageLevel.ESI2,
                    recommendedDepartment = GetRecommendedDepartment(assessment.Level),
                    estimatedWaitTime = assessment.EstimatedWaitTimeMinutes
                };

                return Json(details, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ خطا در دریافت جزئیات بیمار");
                return Json(new { success = false, message = "خطا در دریافت جزئیات بیمار: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Emergency Management

        /// <summary>
        /// صفحه پذیرش اورژانس
        /// </summary>
        /// <returns>صفحه پذیرش اورژانس</returns>
        [HttpGet]
        public ActionResult EmergencyReception()
        {
            try
            {
                Log.Information("🚨 نمایش صفحه پذیرش اورژانس, کاربر: {UserName}",
                    _currentUserService.UserName);

                var model = new EmergencyReceptionViewModel
                {
                    EmergencyTypes = GetEmergencyTypes(),
                    Symptoms = GetCommonSymptoms()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ خطا در نمایش صفحه پذیرش اورژانس");
                TempData["ErrorMessage"] = "خطا در نمایش صفحه پذیرش اورژانس: " + ex.Message;
                return View(new EmergencyReceptionViewModel());
            }
        }

        /// <summary>
        /// ایجاد پذیرش اورژانس
        /// </summary>
        /// <param name="model">مدل پذیرش اورژانس</param>
        /// <returns>نتیجه ایجاد پذیرش</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateEmergencyReception(EmergencyReceptionViewModel model)
        {
            try
            {
                Log.Warning("🚨 ایجاد پذیرش اورژانس - بیمار {PatientId}, نوع: {EmergencyType}, کاربر: {UserName}",
                    model.PatientId, model.EmergencyType, _currentUserService.UserName);

                if (!ModelState.IsValid)
                {
                    model.EmergencyTypes = GetEmergencyTypes();
                    model.Symptoms = GetCommonSymptoms();
                    TempData["ErrorMessage"] = "اطلاعات وارد شده صحیح نیست";
                    return View("EmergencyReception", model);
                }

                var result = await _integrationService.HandleEmergencyReceptionAsync(
                    model.PatientId,
                    model.EmergencyType,
                    model.SelectedSymptoms);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "پذیرش اورژانس با موفقیت ایجاد شد";
                    return RedirectToAction("Details", "Reception", new { id = result.Data.ReceptionId });
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در ایجاد پذیرش اورژانس: " + result.Message;
                    model.EmergencyTypes = GetEmergencyTypes();
                    model.Symptoms = GetCommonSymptoms();
                    return View("EmergencyReception", model);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ خطا در ایجاد پذیرش اورژانس");
                TempData["ErrorMessage"] = "خطا در ایجاد پذیرش اورژانس: " + ex.Message;
                model.EmergencyTypes = GetEmergencyTypes();
                model.Symptoms = GetCommonSymptoms();
                return View("EmergencyReception", model);
            }
        }

        #endregion

        #region Priority Management

        /// <summary>
        /// به‌روزرسانی اولویت پذیرش بر اساس تریاژ
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="triageAssessmentId">شناسه ارزیابی تریاژ</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePriority(int receptionId, int triageAssessmentId)
        {
            try
            {
                Log.Information("🔄 به‌روزرسانی اولویت پذیرش {ReceptionId} بر اساس تریاژ {AssessmentId}",
                    receptionId, triageAssessmentId);

                var result = await _integrationService.UpdateReceptionPriorityFromTriageAsync(receptionId, triageAssessmentId);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "اولویت پذیرش به‌روزرسانی شد";
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در به‌روزرسانی اولویت: " + result.Message;
                }

                return RedirectToAction("Details", "Reception", new { id = receptionId });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ خطا در به‌روزرسانی اولویت پذیرش");
                TempData["ErrorMessage"] = "خطا در به‌روزرسانی اولویت: " + ex.Message;
                return RedirectToAction("Details", "Reception", new { id = receptionId });
            }
        }

        #endregion

        #region Reports and Analytics

        /// <summary>
        /// گزارش یکپارچه تریاژ و پذیرش
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش یکپارچه</returns>
        [HttpGet]
        public async Task<ActionResult> IntegratedReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                Log.Information("📊 گزارش یکپارچه تریاژ و پذیرش - از {StartDate} تا {EndDate}, کاربر: {UserName}",
                    startDate, endDate, _currentUserService.UserName);

                var model = new IntegratedReportViewModel
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-7),
                    EndDate = endDate ?? DateTime.Today
                };

                // دریافت آمار تریاژ
                var triageStatsResult = await _triageService.GetAssessmentsByDateRangeAsync(
                    model.StartDate, model.EndDate);

                if (triageStatsResult.Success)
                {
                    model.TriageStats = triageStatsResult.Data;
                }

                // دریافت آمار پذیرش
                var receptionStatsResult = await _receptionService.GetReceptionsByDateRangeAsync(
                    model.StartDate, model.EndDate);

                if (receptionStatsResult.Success)
                {
                    model.ReceptionStats = receptionStatsResult.Data.Items;
                }

                // محاسبه آمار یکپارچه
                model.CalculateIntegratedStats();

                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ خطا در تولید گزارش یکپارچه");
                TempData["ErrorMessage"] = "خطا در تولید گزارش: " + ex.Message;
                return View(new IntegratedReportViewModel());
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت بخش پیشنهادی بر اساس سطح تریاژ
        /// </summary>
        private string GetRecommendedDepartment(TriageLevel triageLevel)
        {
            return triageLevel switch
            {
                TriageLevel.ESI1 => "اورژانس",
                TriageLevel.ESI2 => "اورژانس",
                TriageLevel.ESI3 => "درمانگاه",
                TriageLevel.ESI4 => "درمانگاه",
                TriageLevel.ESI5 => "درمانگاه",
                _ => "درمانگاه"
            };
        }

        /// <summary>
        /// دریافت انواع اورژانس
        /// </summary>
        private List<string> GetEmergencyTypes()
        {
            return new List<string>
            {
                "سکته قلبی",
                "سکته مغزی",
                "تصادف",
                "سقوط",
                "سوختگی",
                "مسمومیت",
                "خونریزی",
                "تنفس مشکل",
                "درد قفسه سینه",
                "سایر موارد اورژانس"
            };
        }

        /// <summary>
        /// دریافت علائم رایج
        /// </summary>
        private List<string> GetCommonSymptoms()
        {
            return new List<string>
            {
                "درد قفسه سینه",
                "تنفس مشکل",
                "درد شدید",
                "خونریزی",
                "از دست دادن هوشیاری",
                "تهوع و استفراغ",
                "سردرد شدید",
                "درد شکم",
                "تب بالا",
                "سایر علائم"
            };
        }

        #endregion
    }
}
