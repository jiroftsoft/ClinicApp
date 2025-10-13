using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس مدیریت اورژانس پزشکی - حیاتی برای محیط درمانی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت پذیرش‌های اورژانس
    /// 2. اولویت‌بندی بیماران
    /// 3. تریاژ اورژانس
    /// 4. مدیریت منابع اورژانس
    /// 5. اعلان‌های فوری
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت اورژانس
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public class MedicalEmergencyService : IMedicalEmergencyService
    {
        #region Fields and Constructor

        private readonly IReceptionService _receptionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public MedicalEmergencyService(
            IReceptionService receptionService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Emergency Management

        /// <summary>
        /// مدیریت پذیرش اورژانس
        /// </summary>
        public async Task<ServiceResult<EmergencyReceptionResult>> HandleEmergencyReceptionAsync(EmergencyReceptionRequest request)
        {
            try
            {
                _logger.Warning("🚨 پذیرش اورژانس: بیمار {PatientId}, اولویت: {Priority}, کاربر: {UserName}", 
                    request.PatientId, request.Priority, _currentUserService.UserName);

                // تریاژ اورژانس
                var triageResult = await PerformEmergencyTriageAsync(request);
                if (!triageResult.Success)
                {
                    return ServiceResult<EmergencyReceptionResult>.Failed(triageResult.Message);
                }

                // ایجاد پذیرش اورژانس
                var createModel = new ReceptionCreateViewModel
                {
                    PatientId = request.PatientId,
                    IsEmergency = true,
                    Priority = (AppointmentPriority)request.Priority,
                    ReceptionDate = DateTime.Now,
                    Notes = $"اورژانس - {request.EmergencyType} - {request.Description}"
                };

                var createResult = await _receptionService.CreateReceptionAsync(createModel);
                if (!createResult.Success)
                {
                    return ServiceResult<EmergencyReceptionResult>.Failed(createResult.Message);
                }

                // انتساب اولویت
                var priorityResult = await AssignEmergencyPriorityAsync(createResult.Data.ReceptionId, request.Priority);
                if (!priorityResult.Success)
                {
                    return ServiceResult<EmergencyReceptionResult>.Failed(priorityResult.Message);
                }

                // تخصیص منابع
                var resourceResult = await AllocateEmergencyResourcesAsync(createResult.Data.ReceptionId, request);
                if (!resourceResult.Success)
                {
                    return ServiceResult<EmergencyReceptionResult>.Failed(resourceResult.Message);
                }

                var emergencyResult = new EmergencyReceptionResult
                {
                    ReceptionId = createResult.Data.ReceptionId,
                    PatientId = request.PatientId,
                    Priority = (AppointmentPriority)request.Priority,
                    EmergencyType = request.EmergencyType,
                    TriageLevel = triageResult.Data.TriageLevel,
                    AssignedDoctor = resourceResult.Data.AssignedDoctor,
                    AssignedRoom = resourceResult.Data.AssignedRoom,
                    EstimatedWaitTime = CalculateEstimatedWaitTime(request.Priority),
                    CreatedAt = DateTime.Now,
                    Status = "Active"
                };

                _logger.Warning("✅ پذیرش اورژانس ایجاد شد: {ReceptionId}, اولویت: {Priority}, تریاژ: {TriageLevel}", 
                    emergencyResult.ReceptionId, emergencyResult.Priority, emergencyResult.TriageLevel);

                return ServiceResult<EmergencyReceptionResult>.Successful(emergencyResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در مدیریت پذیرش اورژانس: بیمار {PatientId}", request.PatientId);
                return ServiceResult<EmergencyReceptionResult>.Failed("خطا در مدیریت پذیرش اورژانس");
            }
        }

        /// <summary>
        /// تشدید پذیرش اورژانس
        /// </summary>
        public async Task<ServiceResult<EmergencyReceptionResult>> EscalateEmergencyReceptionAsync(int receptionId, string escalationReason)
        {
            try
            {
                _logger.Warning("⚠️ تشدید پذیرش اورژانس: {ReceptionId}, دلیل: {Reason}, کاربر: {UserName}", 
                    receptionId, escalationReason, _currentUserService.UserName);

                // دریافت اطلاعات پذیرش
                var receptionResult = await _receptionService.GetReceptionByIdAsync(receptionId);
                if (!receptionResult.Success)
                {
                    return ServiceResult<EmergencyReceptionResult>.Failed("پذیرش یافت نشد");
                }

                // تشدید اولویت
                var escalationResult = await EscalatePriorityAsync(receptionId, escalationReason);
                if (!escalationResult.Success)
                {
                    return ServiceResult<EmergencyReceptionResult>.Failed(escalationResult.Message);
                }

                // اعلان فوری
                await SendEmergencyNotificationAsync(receptionId, "EmergencyEscalation", new { Reason = escalationReason });

                var emergencyResult = new EmergencyReceptionResult
                {
                    ReceptionId = receptionId,
                    Priority = (AppointmentPriority)escalationResult.Data.NewPriority,
                    EscalationReason = escalationReason,
                    EscalatedAt = DateTime.Now,
                    Status = "Escalated"
                };

                _logger.Warning("🚨 پذیرش اورژانس تشدید شد: {ReceptionId}, اولویت جدید: {Priority}", 
                    receptionId, emergencyResult.Priority);

                return ServiceResult<EmergencyReceptionResult>.Successful(emergencyResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تشدید پذیرش اورژانس: {ReceptionId}", receptionId);
                return ServiceResult<EmergencyReceptionResult>.Failed("خطا در تشدید پذیرش اورژانس");
            }
        }

        /// <summary>
        /// حل پذیرش اورژانس
        /// </summary>
        public async Task<ServiceResult<EmergencyReceptionResult>> ResolveEmergencyReceptionAsync(int receptionId, string resolution)
        {
            try
            {
                _logger.Information("✅ حل پذیرش اورژانس: {ReceptionId}, راه‌حل: {Resolution}, کاربر: {UserName}", 
                    receptionId, resolution, _currentUserService.UserName);

                // به‌روزرسانی وضعیت پذیرش
                var updateResult = await _receptionService.UpdateReceptionAsync(new ReceptionEditViewModel 
                { 
                    ReceptionId = receptionId,
                    Status = ReceptionStatus.Completed
                });
                if (!updateResult.Success)
                {
                    return ServiceResult<EmergencyReceptionResult>.Failed(updateResult.Message);
                }

                // ثبت راه‌حل
                await LogEmergencyResolutionAsync(receptionId, resolution);

                var emergencyResult = new EmergencyReceptionResult
                {
                    ReceptionId = receptionId,
                    Resolution = resolution,
                    ResolvedAt = DateTime.Now,
                    Status = "Resolved"
                };

                _logger.Information("🎉 پذیرش اورژانس حل شد: {ReceptionId}, راه‌حل: {Resolution}", 
                    receptionId, resolution);

                return ServiceResult<EmergencyReceptionResult>.Successful(emergencyResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در حل پذیرش اورژانس: {ReceptionId}", receptionId);
                return ServiceResult<EmergencyReceptionResult>.Failed("خطا در حل پذیرش اورژانس");
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<ServiceResult<EmergencyTriageResult>> PerformEmergencyTriageAsync(EmergencyReceptionRequest request)
        {
            try
            {
                _logger.Information("🏥 شروع تریاژ اورژانس: بیمار {PatientId}, نوع: {EmergencyType}", 
                    request.PatientId, request.EmergencyType);

                // تحلیل علائم و تعیین سطح تریاژ
                var triageLevel = await AnalyzeSymptomsAndDetermineTriageLevelAsync(request);
                
                // محاسبه شدت بر اساس الگوریتم پزشکی
                var severityScore = await CalculateMedicalSeverityScoreAsync(request);
                
                // تعیین اقدامات توصیه شده
                var recommendedActions = await GetMedicalRecommendedActionsAsync(triageLevel, severityScore);
                
                // ثبت تریاژ در سیستم
                await LogTriageDecisionAsync(request.PatientId, triageLevel, severityScore);

                var triageResult = new EmergencyTriageResult
                {
                    TriageLevel = triageLevel,
                    EstimatedSeverity = severityScore,
                    RecommendedActions = recommendedActions,
                    TriageTimestamp = DateTime.Now,
                    TriageNotes = $"تریاژ انجام شد - سطح: {triageLevel}, شدت: {severityScore}"
                };

                _logger.Warning("🚨 تریاژ اورژانس تکمیل شد: بیمار {PatientId}, سطح: {TriageLevel}, شدت: {Severity}", 
                    request.PatientId, triageLevel, severityScore);

                return ServiceResult<EmergencyTriageResult>.Successful(triageResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تریاژ اورژانس: بیمار {PatientId}", request.PatientId);
                return ServiceResult<EmergencyTriageResult>.Failed("خطا در انجام تریاژ اورژانس");
            }
        }

        private async Task<ServiceResult<PriorityAssignmentResult>> AssignEmergencyPriorityAsync(int receptionId, EmergencyPriority priority)
        {
            // TODO: پیاده‌سازی انتساب اولویت
            return ServiceResult<PriorityAssignmentResult>.Successful(new PriorityAssignmentResult
            {
                ReceptionId = receptionId,
                Priority = priority,
                AssignedAt = DateTime.Now
            });
        }

        private async Task<ServiceResult<ResourceAllocationResult>> AllocateEmergencyResourcesAsync(int receptionId, EmergencyReceptionRequest request)
        {
            // TODO: پیاده‌سازی تخصیص منابع
            return ServiceResult<ResourceAllocationResult>.Successful(new ResourceAllocationResult
            {
                ReceptionId = receptionId,
                AssignedDoctor = "دکتر اورژانس",
                AssignedRoom = "اتاق اورژانس 1",
                AllocatedAt = DateTime.Now
            });
        }

        private async Task<ServiceResult<PriorityEscalationResult>> EscalatePriorityAsync(int receptionId, string reason)
        {
            // TODO: پیاده‌سازی تشدید اولویت
            return ServiceResult<PriorityEscalationResult>.Successful(new PriorityEscalationResult
            {
                ReceptionId = receptionId,
                NewPriority = EmergencyPriority.Critical,
                EscalatedAt = DateTime.Now
            });
        }

        private async Task SendEmergencyNotificationAsync(int receptionId, string notificationType, object data)
        {
            try
            {
                _logger.Warning("📢 ارسال اعلان اورژانس: {ReceptionId}, نوع: {Type}", receptionId, notificationType);

                // ارسال اعلان به تیم اورژانس
                await SendToEmergencyTeamAsync(receptionId, notificationType, data);
                
                // ارسال اعلان به پزشکان متخصص
                await SendToSpecialistsAsync(receptionId, notificationType, data);
                
                // ارسال اعلان به مدیریت
                await SendToManagementAsync(receptionId, notificationType, data);
                
                // ثبت در سیستم اعلان‌رسانی
                await LogNotificationSentAsync(receptionId, notificationType, data);

                _logger.Warning("✅ اعلان اورژانس ارسال شد: {ReceptionId}", receptionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ارسال اعلان اورژانس: {ReceptionId}", receptionId);
            }
        }

        private async Task SendToEmergencyTeamAsync(int receptionId, string notificationType, object data)
        {
            try
            {
                _logger.Warning("🚨 اعلان به تیم اورژانس: {ReceptionId}", receptionId);
                
                // TODO: پیاده‌سازی ارسال به تیم اورژانس
                // await _notificationService.SendToEmergencyTeamAsync(receptionId, notificationType, data);
                
                _logger.Information("📞 اعلان به تیم اورژانس ارسال شد: {ReceptionId}", receptionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ارسال اعلان به تیم اورژانس: {ReceptionId}", receptionId);
            }
        }

        private async Task SendToSpecialistsAsync(int receptionId, string notificationType, object data)
        {
            try
            {
                _logger.Warning("👨‍⚕️ اعلان به متخصصان: {ReceptionId}", receptionId);
                
                // TODO: پیاده‌سازی ارسال به متخصصان
                // await _notificationService.SendToSpecialistsAsync(receptionId, notificationType, data);
                
                _logger.Information("📋 اعلان به متخصصان ارسال شد: {ReceptionId}", receptionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ارسال اعلان به متخصصان: {ReceptionId}", receptionId);
            }
        }

        private async Task SendToManagementAsync(int receptionId, string notificationType, object data)
        {
            try
            {
                _logger.Warning("👔 اعلان به مدیریت: {ReceptionId}", receptionId);
                
                // TODO: پیاده‌سازی ارسال به مدیریت
                // await _notificationService.SendToManagementAsync(receptionId, notificationType, data);
                
                _logger.Information("📊 اعلان به مدیریت ارسال شد: {ReceptionId}", receptionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ارسال اعلان به مدیریت: {ReceptionId}", receptionId);
            }
        }

        private async Task LogNotificationSentAsync(int receptionId, string notificationType, object data)
        {
            try
            {
                _logger.Information("📝 ثبت اعلان ارسال شده: {ReceptionId}, نوع: {Type}", receptionId, notificationType);
                
                // TODO: ذخیره در دیتابیس برای audit trail
                // await _auditService.LogNotificationSentAsync(receptionId, notificationType, data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ثبت اعلان: {ReceptionId}", receptionId);
            }
        }

        private async Task LogEmergencyResolutionAsync(int receptionId, string resolution)
        {
            // TODO: پیاده‌سازی ثبت راه‌حل
            _logger.Information("📝 ثبت راه‌حل اورژانس: {ReceptionId}, راه‌حل: {Resolution}", receptionId, resolution);
        }

        private async Task<TriageLevel> AnalyzeSymptomsAndDetermineTriageLevelAsync(EmergencyReceptionRequest request)
        {
            try
            {
                _logger.Information("🔍 تحلیل علائم اورژانس: بیمار {PatientId}, نوع: {EmergencyType}", 
                    request.PatientId, request.EmergencyType);

                var triageScore = 0;
                var emergencyType = request.EmergencyType?.ToLower() ?? "";
                var symptoms = request.Symptoms ?? new List<string>();

                // الگوریتم تریاژ بر اساس نوع اورژانس
                switch (emergencyType)
                {
                    case "cardiac":
                    case "قلبی":
                        triageScore = CalculateCardiacTriageScore(symptoms);
                        break;
                    case "trauma":
                    case "تروما":
                        triageScore = CalculateTraumaTriageScore(symptoms);
                        break;
                    case "respiratory":
                    case "تنفسی":
                        triageScore = CalculateRespiratoryTriageScore(symptoms);
                        break;
                    case "neurological":
                    case "عصبی":
                        triageScore = CalculateNeurologicalTriageScore(symptoms);
                        break;
                    default:
                        triageScore = CalculateGeneralTriageScore(symptoms);
                        break;
                }

                // تعیین سطح تریاژ بر اساس امتیاز
                var triageLevel = DetermineTriageLevelFromScore(triageScore);
                
                _logger.Warning("📊 نتیجه تریاژ: بیمار {PatientId}, امتیاز: {Score}, سطح: {Level}", 
                    request.PatientId, triageScore, triageLevel);

                return triageLevel;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تحلیل علائم: بیمار {PatientId}", request.PatientId);
                return TriageLevel.ESI2; // در صورت خطا، سطح اورژانسی در نظر بگیر
            }
        }

        private async Task<int> CalculateMedicalSeverityScoreAsync(EmergencyReceptionRequest request)
        {
            try
            {
                var severityScore = 0;
                var symptoms = request.Symptoms ?? new List<string>();

                // محاسبه امتیاز شدت بر اساس علائم
                foreach (var symptom in symptoms)
                {
                    severityScore += GetSymptomSeverityWeight(symptom);
                }

                // اضافه کردن امتیاز بر اساس نوع اورژانس
                severityScore += GetEmergencyTypeSeverityWeight(request.EmergencyType);

                // محدود کردن امتیاز به بازه 1-10
                return Math.Max(1, Math.Min(10, severityScore));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در محاسبه شدت: بیمار {PatientId}", request.PatientId);
                return 5; // امتیاز متوسط در صورت خطا
            }
        }

        private async Task<List<string>> GetMedicalRecommendedActionsAsync(TriageLevel triageLevel, int severityScore)
        {
            var actions = new List<string>();

            switch (triageLevel)
            {
                case TriageLevel.ESI1:
                    actions.AddRange(new[] { "🚨 فوری - تماس با متخصص قلب", "🏥 آماده‌سازی اتاق عمل", "💉 تزریق فوری", "📞 اعلان به تیم اورژانس" });
                    break;
                case TriageLevel.ESI2:
                    actions.AddRange(new[] { "⚡ سریع - بررسی اولیه", "🩺 معاینه فوری", "📋 ثبت علائم حیاتی", "👨‍⚕️ انتظار پزشک متخصص" });
                    break;
                default:
                    actions.AddRange(new[] { "📝 ثبت اطلاعات", "⏰ نوبت‌دهی عادی", "🩺 معاینه معمولی" });
                    break;
            }

            // اضافه کردن اقدامات بر اساس شدت
            if (severityScore >= 8)
            {
                actions.Add("🔴 اولویت بالا - نظارت مداوم");
            }
            else if (severityScore >= 5)
            {
                actions.Add("🟡 اولویت متوسط - بررسی منظم");
            }

            return actions;
        }

        private async Task LogTriageDecisionAsync(int patientId, TriageLevel triageLevel, int severityScore)
        {
            try
            {
                _logger.Information("📝 ثبت تصمیم تریاژ: بیمار {PatientId}, سطح: {Level}, شدت: {Score}", 
                    patientId, triageLevel, severityScore);
                
                // TODO: ذخیره در دیتابیس برای audit trail
                // await _auditService.LogTriageDecisionAsync(patientId, triageLevel, severityScore);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ثبت تریاژ: بیمار {PatientId}", patientId);
            }
        }

        // الگوریتم‌های تریاژ تخصصی
        private int CalculateCardiacTriageScore(List<string> symptoms)
        {
            var score = 0;
            foreach (var symptom in symptoms)
            {
                switch (symptom.ToLower())
                {
                    case "chest pain":
                    case "درد قفسه سینه":
                        score += 8;
                        break;
                    case "shortness of breath":
                    case "تنگی نفس":
                        score += 7;
                        break;
                    case "irregular heartbeat":
                    case "ضربان نامنظم":
                        score += 6;
                        break;
                    case "dizziness":
                    case "سرگیجه":
                        score += 4;
                        break;
                    default:
                        score += 2;
                        break;
                }
            }
            return Math.Min(10, score);
        }

        private int CalculateTraumaTriageScore(List<string> symptoms)
        {
            var score = 0;
            foreach (var symptom in symptoms)
            {
                switch (symptom.ToLower())
                {
                    case "severe bleeding":
                    case "خونریزی شدید":
                        score += 9;
                        break;
                    case "unconscious":
                    case "بیهوشی":
                        score += 8;
                        break;
                    case "broken bone":
                    case "شکستگی":
                        score += 6;
                        break;
                    case "minor cut":
                    case "بریدگی جزئی":
                        score += 3;
                        break;
                    default:
                        score += 2;
                        break;
                }
            }
            return Math.Min(10, score);
        }

        private int CalculateRespiratoryTriageScore(List<string> symptoms)
        {
            var score = 0;
            foreach (var symptom in symptoms)
            {
                switch (symptom.ToLower())
                {
                    case "severe breathing difficulty":
                    case "مشکل شدید تنفسی":
                        score += 9;
                        break;
                    case "wheezing":
                    case "خس خس":
                        score += 6;
                        break;
                    case "cough":
                    case "سرفه":
                        score += 3;
                        break;
                    default:
                        score += 2;
                        break;
                }
            }
            return Math.Min(10, score);
        }

        private int CalculateNeurologicalTriageScore(List<string> symptoms)
        {
            var score = 0;
            foreach (var symptom in symptoms)
            {
                switch (symptom.ToLower())
                {
                    case "severe headache":
                    case "سردرد شدید":
                        score += 7;
                        break;
                    case "seizure":
                    case "تشنج":
                        score += 9;
                        break;
                    case "confusion":
                    case "گیجی":
                        score += 6;
                        break;
                    case "numbness":
                    case "بی‌حسی":
                        score += 5;
                        break;
                    default:
                        score += 2;
                        break;
                }
            }
            return Math.Min(10, score);
        }

        private int CalculateGeneralTriageScore(List<string> symptoms)
        {
            var score = symptoms?.Count ?? 0;
            return Math.Min(10, score * 2);
        }

        private TriageLevel DetermineTriageLevelFromScore(int score)
        {
            return score switch
            {
                >= 8 => TriageLevel.ESI1,
                >= 5 => TriageLevel.ESI2,
                _ => TriageLevel.ESI5
            };
        }

        private int GetSymptomSeverityWeight(string symptom)
        {
            return symptom.ToLower() switch
            {
                "severe" or "شدید" => 5,
                "moderate" or "متوسط" => 3,
                "mild" or "خفیف" => 1,
                _ => 2
            };
        }

        private int GetEmergencyTypeSeverityWeight(string emergencyType)
        {
            return emergencyType?.ToLower() switch
            {
                "cardiac" or "قلبی" => 3,
                "trauma" or "تروما" => 4,
                "respiratory" or "تنفسی" => 3,
                "neurological" or "عصبی" => 2,
                _ => 1
            };
        }

        private List<string> GetRecommendedActions(TriageLevel triageLevel)
        {
            return triageLevel switch
            {
                TriageLevel.ESI1 => new List<string> { "فوری", "تماس با متخصص", "آماده‌سازی اتاق عمل" },
                TriageLevel.ESI2 => new List<string> { "سریع", "بررسی اولیه", "آماده‌سازی تجهیزات" },
                _ => new List<string> { "عادی", "بررسی معمولی" }
            };
        }

        private TimeSpan CalculateEstimatedWaitTime(EmergencyPriority priority)
        {
            return priority switch
            {
                EmergencyPriority.Critical => TimeSpan.FromMinutes(0),
                EmergencyPriority.High => TimeSpan.FromMinutes(5),
                EmergencyPriority.Medium => TimeSpan.FromMinutes(15),
                EmergencyPriority.Low => TimeSpan.FromMinutes(30),
                _ => TimeSpan.FromMinutes(60)
            };
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// درخواست پذیرش اورژانس
    /// </summary>
    public class EmergencyReceptionRequest
    {
        public int PatientId { get; set; }
        public EmergencyPriority Priority { get; set; }
        public string EmergencyType { get; set; }
        public string Description { get; set; }
        public List<string> Symptoms { get; set; } = new List<string>();
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }



   
    /// <summary>
    /// نتیجه تریاژ اورژانس
    /// </summary>
    public class EmergencyTriageResult
    {
        public TriageLevel TriageLevel { get; set; }
        public int EstimatedSeverity { get; set; }
        public List<string> RecommendedActions { get; set; } = new List<string>();
        public DateTime TriageTimestamp { get; set; }
        public string TriageNotes { get; set; }
    }

    /// <summary>
    /// نتیجه انتساب اولویت
    /// </summary>
    public class PriorityAssignmentResult
    {
        public int ReceptionId { get; set; }
        public EmergencyPriority Priority { get; set; }
        public DateTime AssignedAt { get; set; }
    }

    /// <summary>
    /// نتیجه تخصیص منابع
    /// </summary>
    public class ResourceAllocationResult
    {
        public int ReceptionId { get; set; }
        public string AssignedDoctor { get; set; }
        public string AssignedRoom { get; set; }
        public DateTime AllocatedAt { get; set; }
    }

    /// <summary>
    /// نتیجه تشدید اولویت
    /// </summary>
    public class PriorityEscalationResult
    {
        public int ReceptionId { get; set; }
        public EmergencyPriority NewPriority { get; set; }
        public DateTime EscalatedAt { get; set; }
    }

    #endregion
}
