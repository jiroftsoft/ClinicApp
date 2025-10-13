using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Triage;
using ClinicApp.ViewModels.Reception;
using ClinicApp.ViewModels.Triage;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس یکپارچه‌سازی پذیرش با تریاژ - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. یکپارچگی کامل بین ماژول پذیرش و تریاژ
    /// 2. مدیریت گردش کار تریاژ به پذیرش
    /// 3. انتقال خودکار بیماران از تریاژ به پذیرش
    /// 4. مدیریت اولویت‌بندی هوشمند
    /// 5. پشتیبانی از اعلان‌ها و هشدارها
    /// 6. مدیریت اورژانس و موارد فوری
    /// </summary>
    public class ReceptionTriageIntegrationService
    {
        #region Fields and Constructor

        private readonly IReceptionService _receptionService;
        private readonly ITriageService _triageService;
        private readonly ITriageQueueService _triageQueueService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionTriageIntegrationService(
            IReceptionService receptionService,
            ITriageService triageService,
            ITriageQueueService triageQueueService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _triageService = triageService ?? throw new ArgumentNullException(nameof(triageService));
            _triageQueueService = triageQueueService ?? throw new ArgumentNullException(nameof(triageQueueService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Triage to Reception Workflow

        /// <summary>
        /// انتقال بیمار از تریاژ به پذیرش
        /// </summary>
        /// <param name="triageAssessmentId">شناسه ارزیابی تریاژ</param>
        /// <param name="receptionData">اطلاعات پذیرش</param>
        /// <returns>نتیجه انتقال</returns>
        public async Task<ServiceResult<ReceptionDetailsViewModel>> TransferTriageToReceptionAsync(
            int triageAssessmentId, 
            TriageToReceptionTransferData receptionData)
        {
            try
            {
                _logger.Information("🔄 شروع انتقال بیمار از تریاژ به پذیرش - ارزیابی {AssessmentId}, کاربر: {UserName}",
                    triageAssessmentId, _currentUserService.UserName);

                // دریافت ارزیابی تریاژ
                var assessmentResult = await _triageService.GetTriageAssessmentAsync(triageAssessmentId);
                if (!assessmentResult.Success)
                {
                    return ServiceResult<ReceptionDetailsViewModel>.Failed("ارزیابی تریاژ یافت نشد: " + assessmentResult.Message);
                }

                var assessment = assessmentResult.Data;

                // ایجاد مدل پذیرش
                var receptionModel = new ReceptionCreateViewModel
                {
                    PatientId = assessment.PatientId,
                    DoctorId = receptionData.DoctorId,
                    ReceptionDate = DateTime.UtcNow,
                    TotalAmount = receptionData.TotalAmount,
                    PatientCoPay = receptionData.PatientCoPay,
                    InsurerShareAmount = receptionData.InsurerShareAmount,
                    SelectedServiceIds = receptionData.ServiceIds,
                    Priority = (AppointmentPriority)assessment.Priority,
                    IsEmergency = assessment.Level == TriageLevel.ESI1 || assessment.Level == TriageLevel.ESI2,
                    Notes = $"انتقال از تریاژ - {assessment.ChiefComplaint} - سطح: {assessment.Level}",
                    CreatedByUserId = _currentUserService.GetCurrentUserId()
                };

                // ایجاد پذیرش
                var receptionResult = await _receptionService.CreateReceptionAsync(receptionModel);
                if (!receptionResult.Success)
                {
                    return ServiceResult<ReceptionDetailsViewModel>.Failed("خطا در ایجاد پذیرش: " + receptionResult.Message);
                }

                // تکمیل ارزیابی تریاژ
                var completeResult = await _triageService.CompleteTriageAssessmentAsync(
                    triageAssessmentId, 
                    receptionData.DepartmentId, 
                    receptionData.DoctorId);

                if (!completeResult.Success)
                {
                    _logger.Warning("⚠️ خطا در تکمیل ارزیابی تریاژ: {Message}", completeResult.Message);
                }

                _logger.Information("✅ بیمار {PatientId} با موفقیت از تریاژ به پذیرش انتقال یافت - پذیرش: {ReceptionId}",
                    assessment.PatientId, receptionResult.Data.ReceptionId);

                return ServiceResult<ReceptionDetailsViewModel>.Successful(
                    receptionResult.Data, 
                    "بیمار با موفقیت از تریاژ به پذیرش انتقال یافت");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در انتقال بیمار از تریاژ به پذیرش");
                return ServiceResult<ReceptionDetailsViewModel>.Failed("خطا در انتقال بیمار از تریاژ به پذیرش: " + ex.Message);
            }
        }

        /// <summary>
        /// دریافت بیماران آماده برای انتقال به پذیرش
        /// </summary>
        /// <returns>لیست بیماران آماده</returns>
        public async Task<ServiceResult<List<TriageToReceptionCandidate>>> GetReadyForReceptionPatientsAsync()
        {
            try
            {
                _logger.Information("📋 دریافت بیماران آماده برای انتقال به پذیرش, کاربر: {UserName}",
                    _currentUserService.UserName);

                // دریافت ارزیابی‌های تکمیل شده
                var assessmentsResult = await _triageService.GetTriageAssessmentsAsync(
                    status: TriageStatus.Completed,
                    pageNumber: 1,
                    pageSize: 100);

                if (!assessmentsResult.Success)
                {
                    return ServiceResult<List<TriageToReceptionCandidate>>.Failed("خطا در دریافت ارزیابی‌ها: " + assessmentsResult.Message);
                }

                var candidates = new List<TriageToReceptionCandidate>();

                foreach (var assessment in assessmentsResult.Data.Items)
                {
                    // بررسی اینکه آیا قبلاً به پذیرش انتقال یافته یا نه
                    var existingReceptionResult = await _receptionService.GetReceptionsByPatientIdAsync(
                        assessment.PatientId, 1, 1);

                    if (existingReceptionResult.Success && existingReceptionResult.Data.Items.Any())
                    {
                        // بررسی پذیرش‌های امروز
                        var todayReceptions = existingReceptionResult.Data.Items
                            .Where(r => !string.IsNullOrEmpty(r.ReceptionDate) && DateTime.Parse(r.ReceptionDate).Date == DateTime.Today.Date)
                            .ToList();

                        if (todayReceptions.Any())
                        {
                            continue; // قبلاً پذیرش شده
                        }
                    }

                    var candidate = new TriageToReceptionCandidate
                    {
                        TriageAssessmentId = assessment.TriageAssessmentId,
                        PatientId = assessment.PatientId,
                        PatientFullName = $"{assessment.Patient?.FirstName} {assessment.Patient?.LastName}",
                        TriageLevel = assessment.Level,
                        Priority = assessment.Priority,
                        ChiefComplaint = assessment.ChiefComplaint,
                        TriageDateTime = assessment.TriageStartAt,
                        EstimatedWaitTime = assessment.EstimatedWaitTimeMinutes,
                        IsEmergency = assessment.Level == TriageLevel.ESI1 || assessment.Level == TriageLevel.ESI2,
                        RecommendedDepartment = GetRecommendedDepartment(assessment.Level),
                        RecommendedDoctor = GetRecommendedDoctor(assessment.Level)
                    };

                    candidates.Add(candidate);
                }

                // مرتب‌سازی بر اساس اولویت
                candidates = candidates
                    .OrderBy(c => c.Priority)
                    .ThenBy(c => c.TriageDateTime)
                    .ToList();

                _logger.Information("📋 {Count} بیمار آماده برای انتقال به پذیرش یافت شد",
                    candidates.Count);

                return ServiceResult<List<TriageToReceptionCandidate>>.Successful(candidates, "لیست بیماران آماده دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت بیماران آماده برای انتقال");
                return ServiceResult<List<TriageToReceptionCandidate>>.Failed("خطا در دریافت بیماران آماده: " + ex.Message);
            }
        }

        /// <summary>
        /// به‌روزرسانی اولویت پذیرش بر اساس تریاژ
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="triageAssessmentId">شناسه ارزیابی تریاژ</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        public async Task<ServiceResult<bool>> UpdateReceptionPriorityFromTriageAsync(
            int receptionId, 
            int triageAssessmentId)
        {
            try
            {
                _logger.Information("🔄 به‌روزرسانی اولویت پذیرش {ReceptionId} بر اساس تریاژ {AssessmentId}",
                    receptionId, triageAssessmentId);

                // دریافت ارزیابی تریاژ
                var assessmentResult = await _triageService.GetTriageAssessmentAsync(triageAssessmentId);
                if (!assessmentResult.Success)
                {
                    return ServiceResult<bool>.Failed("ارزیابی تریاژ یافت نشد: " + assessmentResult.Message);
                }

                var assessment = assessmentResult.Data;

                // محاسبه اولویت جدید
                var newPriority = CalculateReceptionPriority(assessment.Level, assessment.Priority);

                // دریافت پذیرش
                var receptionResult = await _receptionService.GetReceptionByIdAsync(receptionId);
                if (!receptionResult.Success)
                {
                    return ServiceResult<bool>.Failed("پذیرش یافت نشد: " + receptionResult.Message);
                }

                // به‌روزرسانی اولویت
                var updateModel = new ReceptionEditViewModel
                {
                    ReceptionId = receptionId,
                    Priority = newPriority,
                    IsEmergency = assessment.Level == TriageLevel.ESI1 || assessment.Level == TriageLevel.ESI2,
                    Notes = $"اولویت به‌روزرسانی شده بر اساس تریاژ - سطح: {assessment.Level}"
                };

                var updateResult = await _receptionService.UpdateReceptionAsync(updateModel);
                if (!updateResult.Success)
                {
                    return ServiceResult<bool>.Failed("خطا در به‌روزرسانی پذیرش: " + updateResult.Message);
                }

                _logger.Information("✅ اولویت پذیرش {ReceptionId} بر اساس تریاژ به‌روزرسانی شد",
                    receptionId);

                return ServiceResult<bool>.Successful(true, "اولویت پذیرش به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در به‌روزرسانی اولویت پذیرش");
                return ServiceResult<bool>.Failed("خطا در به‌روزرسانی اولویت: " + ex.Message);
            }
        }

        #endregion

        #region Emergency Management

        /// <summary>
        /// مدیریت پذیرش اورژانس با تریاژ
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="emergencyType">نوع اورژانس</param>
        /// <param name="symptoms">علائم</param>
        /// <returns>نتیجه پذیرش اورژانس</returns>
        public async Task<ServiceResult<EmergencyReceptionResult>> HandleEmergencyReceptionAsync(
            int patientId, 
            string emergencyType, 
            List<string> symptoms)
        {
            try
            {
                _logger.Warning("🚨 پذیرش اورژانس - بیمار {PatientId}, نوع: {EmergencyType}, کاربر: {UserName}",
                    patientId, emergencyType, _currentUserService.UserName);

                // ایجاد ارزیابی تریاژ اورژانس
                var triageData = new TriageCreateViewModel
                {
                    PatientId = patientId,
                    ChiefComplaint = emergencyType,
                    ArrivalAt = DateTime.UtcNow,
                    TriageStartAt = DateTime.UtcNow
                };

                var triageResult = await _triageService.CreateTriageAssessmentAsync(
                    patientId, 
                    emergencyType, 
                    null);

                if (!triageResult.Success)
                {
                    return ServiceResult<EmergencyReceptionResult>.Failed("خطا در ایجاد ارزیابی تریاژ: " + triageResult.Message);
                }

                // ایجاد پذیرش اورژانس
                var receptionModel = new ReceptionCreateViewModel
                {
                    PatientId = patientId,
                    ReceptionDate = DateTime.UtcNow,
                    IsEmergency = true,
                    Priority = AppointmentPriority.Critical,
                    Notes = $"اورژانس - {emergencyType} - علائم: {string.Join(", ", symptoms)}",
                    CreatedByUserId = _currentUserService.GetCurrentUserId()
                };

                var receptionResult = await _receptionService.CreateReceptionAsync(receptionModel);
                if (!receptionResult.Success)
                {
                    return ServiceResult<EmergencyReceptionResult>.Failed("خطا در ایجاد پذیرش اورژانس: " + receptionResult.Message);
                }

                var result = new EmergencyReceptionResult
                {
                    ReceptionId = receptionResult.Data.ReceptionId,
                    PatientId = patientId,
                    EmergencyType = emergencyType,
                    TriageLevel = triageResult.Data.Level,
                    Priority = AppointmentPriority.Critical,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Emergency Created"
                };

                _logger.Warning("🚨 پذیرش اورژانس با موفقیت ایجاد شد - پذیرش: {ReceptionId}, تریاژ: {TriageId}",
                    receptionResult.Data.ReceptionId, triageResult.Data.TriageAssessmentId);

                return ServiceResult<EmergencyReceptionResult>.Successful(result, "پذیرش اورژانس با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پذیرش اورژانس");
                return ServiceResult<EmergencyReceptionResult>.Failed("خطا در پذیرش اورژانس: " + ex.Message);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// محاسبه اولویت پذیرش بر اساس تریاژ
        /// </summary>
        private AppointmentPriority CalculateReceptionPriority(TriageLevel triageLevel, int triagePriority)
        {
            return triageLevel switch
            {
                TriageLevel.ESI1 => AppointmentPriority.Critical,
                TriageLevel.ESI2 => AppointmentPriority.High,
                TriageLevel.ESI3 => AppointmentPriority.Medium,
                TriageLevel.ESI4 => AppointmentPriority.Low,
                TriageLevel.ESI5 => AppointmentPriority.Low,
                _ => AppointmentPriority.Medium
            };
        }

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
        /// دریافت پزشک پیشنهادی بر اساس سطح تریاژ
        /// </summary>
        private string GetRecommendedDoctor(TriageLevel triageLevel)
        {
            return triageLevel switch
            {
                TriageLevel.ESI1 => "پزشک اورژانس",
                TriageLevel.ESI2 => "پزشک اورژانس",
                TriageLevel.ESI3 => "پزشک عمومی",
                TriageLevel.ESI4 => "پزشک عمومی",
                TriageLevel.ESI5 => "پزشک عمومی",
                _ => "پزشک عمومی"
            };
        }

        #endregion
    }

    #region Supporting Classes


    /// <summary>
    /// کاندیدای انتقال از تریاژ به پذیرش
    /// </summary>
    public class TriageToReceptionCandidate
    {
        public int TriageAssessmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
        public TriageLevel TriageLevel { get; set; }
        public int Priority { get; set; }
        public string ChiefComplaint { get; set; }
        public DateTime TriageDateTime { get; set; }
        public int? EstimatedWaitTime { get; set; }
        public bool IsEmergency { get; set; }
        public string RecommendedDepartment { get; set; }
        public string RecommendedDoctor { get; set; }
    }

    /// <summary>
    /// نتیجه پذیرش اورژانس
    /// </summary>
    public class EmergencyReceptionResult
    {
        public int ReceptionId { get; set; }
        public int PatientId { get; set; }
        public string EmergencyType { get; set; }
        public TriageLevel TriageLevel { get; set; }
        public AppointmentPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string AssignedDoctor { get; set; }
        public string AssignedRoom { get; set; }
        public TimeSpan EstimatedWaitTime { get; set; }
        public DateTime? EscalatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string EscalationReason { get; set; }
        public string Resolution { get; set; }
    }

    #endregion
}
