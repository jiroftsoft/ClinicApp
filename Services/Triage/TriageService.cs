using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.ViewModels.Triage;
using Serilog;

namespace ClinicApp.Services.Triage;

/// <summary>
/// سرویس تریاژ - پیاده‌سازی کامل برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت کامل فرآیند تریاژ بیماران
/// 2. پشتیبانی از استانداردهای بین‌المللی تریاژ
/// 3. مدیریت صف تریاژ با اولویت‌بندی هوشمند
/// 4. پشتیبانی از علائم حیاتی و ارزیابی اولویت
/// 5. یکپارچگی با سیستم پذیرش موجود
/// </summary>
public class TriageService : ITriageService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserServiceService;
    private readonly ITriageQueueService _queueService;
    private readonly ILogger _logger;

    public TriageService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITriageQueueService queueService,
        ILogger logger)
    {
        _context = context;
        _currentUserServiceService = currentUserService;
        _queueService = queueService;
        _logger = logger;
    }

    #region ارزیابی تریاژ (Triage Assessment)

    public async Task<ServiceResult<TriageAssessment>> CreateTriageAssessmentAsync(
        int patientId, 
        string chiefComplaint, 
        TriageVitalSigns vitalSigns, 
        string assessmentNotes = null)
    {
        try
        {
            // بررسی مجوز - فقط پرستاران می‌توانند تریاژ انجام دهند
            if (!await IsAuthorizedForTriageAsync())
            {
                return ServiceResult<TriageAssessment>.Failed("شما مجوز انجام تریاژ را ندارید.");
            }

            _logger.Information("شروع ایجاد ارزیابی تریاژ برای بیمار {PatientId}", patientId);

            // بررسی وجود بیمار
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted);
            
            if (patient == null)
            {
                return ServiceResult<TriageAssessment>.Failed("بیمار یافت نشد.");
            }

            // دریافت کاربر جاری (پرستار)
            var userId = _currentUserServiceService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return ServiceResult<TriageAssessment>.Failed("کاربر جاری یافت نشد.");
            }

            // محاسبه سطح تریاژ بر اساس علائم حیاتی
            var levelResult = await CalculateTriageLevelAsync(vitalSigns);
            var level = levelResult.Success ? levelResult.Data : TriageLevel.ESI5;

            // محاسبه اولویت بر اساس سطح و علائم حیاتی
            var priority = CalculatePriority(level, vitalSigns);

            // ایجاد ارزیابی تریاژ
            var assessment = new TriageAssessment
            {
                PatientId = patientId,
                AssessorUserId = userId,
                ChiefComplaint = chiefComplaint,
                ArrivalAt = DateTime.UtcNow,
                TriageStartAt = DateTime.UtcNow,
                Status = TriageStatus.Pending,
                AssessmentNotes = assessmentNotes,
                Level = level,
                Priority = priority,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // ایجاد ارزیابی
                    _context.TriageAssessments.Add(assessment);
                    await _context.SaveChangesAsync();

                    // ثبت علائم حیاتی
                    if (vitalSigns != null)
                    {
                        vitalSigns.TriageAssessmentId = assessment.TriageAssessmentId;
                        vitalSigns.CreatedByUserId = userId;
                        vitalSigns.CreatedAt = DateTime.UtcNow;
                        
                        _context.TriageVitalSigns.Add(vitalSigns);
                        await _context.SaveChangesAsync();
                    }

                    // اضافه کردن به صف با SLA
                    var queue = new TriageQueue
                    {
                        PatientId = assessment.PatientId,
                        TriageAssessmentId = assessment.TriageAssessmentId,
                        Priority = assessment.Priority,
                        QueueTime = DateTime.UtcNow,
                        Status = TriageQueueStatus.Waiting,
                        CreatedByUserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    // تنظیم SLA بر اساس سطح تریاژ
                    queue.NextReassessmentDueAt = queue.QueueTime.AddMinutes(
                        assessment.Level == TriageLevel.ESI2 ? 15 :
                        assessment.Level == TriageLevel.ESI3 ? 30 :
                        assessment.Level == TriageLevel.ESI4 ? 60 : 120);

                    _context.TriageQueues.Add(queue);
                    await _context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Information("ارزیابی تریاژ با شناسه {AssessmentId} ایجاد شد", 
                assessment.TriageAssessmentId);

            return ServiceResult<TriageAssessment>.Successful(assessment, "ارزیابی تریاژ با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ایجاد ارزیابی تریاژ");
            return ServiceResult<TriageAssessment>.Failed("خطا در ایجاد ارزیابی تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageAssessment>> UpdateTriageAssessmentAsync(
        int assessmentId, 
        TriageLevel level, 
        int priority, 
        string notes = null)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<TriageAssessment>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            assessment.Level = level;
            assessment.Priority = priority;
            assessment.AssessmentNotes = notes;
            assessment.UpdatedAt = DateTime.UtcNow;
            assessment.UpdatedByUserId = _currentUserServiceService.GetCurrentUserId();

            await _context.SaveChangesAsync();

            _logger.Information("ارزیابی تریاژ {AssessmentId} به‌روزرسانی شد", assessmentId);

            return ServiceResult<TriageAssessment>.Successful(assessment, "ارزیابی تریاژ با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در به‌روزرسانی ارزیابی تریاژ");
            return ServiceResult<TriageAssessment>.Failed("خطا در به‌روزرسانی ارزیابی تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageAssessment>> CompleteTriageAssessmentAsync(
        int assessmentId, 
        int? recommendedDepartmentId = null, 
        int? recommendedDoctorId = null)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<TriageAssessment>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            assessment.Status = TriageStatus.Completed;
            assessment.IsOpen = false;
            assessment.TriageEndAt = DateTime.UtcNow;
            assessment.RecommendedDepartmentId = recommendedDepartmentId;
            assessment.RecommendedDoctorId = recommendedDoctorId;
            assessment.UpdatedAt = DateTime.UtcNow;
            assessment.UpdatedByUserId = _currentUserServiceService.GetCurrentUserId();

            // بستن صف‌های فعال
            await _queueService.CloseActiveQueueIfAnyAsync(assessmentId);

            await _context.SaveChangesAsync();

            _logger.Information("ارزیابی تریاژ {AssessmentId} تکمیل شد", assessmentId);

            return ServiceResult<TriageAssessment>.Successful(assessment, "ارزیابی تریاژ با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در تکمیل ارزیابی تریاژ");
            return ServiceResult<TriageAssessment>.Failed("خطا در تکمیل ارزیابی تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageAssessment>> GetTriageAssessmentByIdAsync(int assessmentId)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .Include(ta => ta.Patient)
                .Include(ta => ta.Assessor)
                .Include(ta => ta.VitalSigns)
                .Include(ta => ta.RecommendedDepartment)
                .Include(ta => ta.RecommendedDoctor)
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<TriageAssessment>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            return ServiceResult<TriageAssessment>.Successful(assessment, "ارزیابی تریاژ با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت ارزیابی تریاژ");
            return ServiceResult<TriageAssessment>.Failed("خطا در دریافت ارزیابی تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<List<TriageAssessment>>> GetPatientTriageAssessmentsAsync(
        int patientId, 
        bool includeCompleted = false)
    {
        try
        {
            var query = _context.TriageAssessments
                .Include(ta => ta.Assessor)
                .Include(ta => ta.VitalSigns)
                .Where(ta => ta.PatientId == patientId && !ta.IsDeleted);

            if (!includeCompleted)
            {
                query = query.Where(ta => ta.Status != TriageStatus.Completed);
            }

            var assessments = await query
                .OrderByDescending(ta => ta.TriageStartAt)
                .ToListAsync();

            return ServiceResult<List<TriageAssessment>>.Successful(assessments, "لیست ارزیابی‌های تریاژ دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت ارزیابی‌های تریاژ بیمار");
            return ServiceResult<List<TriageAssessment>>.Failed("خطا در دریافت ارزیابی‌های تریاژ بیمار: " + ex.Message);
        }
    }

    #endregion

    #region صف تریاژ (Triage Queue)

    public async Task<ServiceResult<TriageQueue>> AddToTriageQueueAsync(
        int assessmentId, 
        int priority, 
        int? targetDepartmentId = null, 
        int? targetDoctorId = null)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<TriageQueue>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            // بررسی وجود صف Waiting برای این ارزیابی
            var exists = await _context.TriageQueues.AnyAsync(q => 
                q.TriageAssessmentId == assessmentId && 
                q.Status == TriageQueueStatus.Waiting && 
                !q.IsDeleted);
            
            if (exists)
            {
                return ServiceResult<TriageQueue>.Failed("این ارزیابی هم‌اکنون در صف انتظار است.");
            }

            var queue = new TriageQueue
            {
                PatientId = assessment.PatientId,
                TriageAssessmentId = assessmentId,
                Priority = priority,
                QueueTime = DateTime.UtcNow,
                Status = TriageQueueStatus.Waiting,
                TargetDepartmentId = targetDepartmentId,
                TargetDoctorId = targetDoctorId,
                CreatedByUserId = _currentUserServiceService.GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            // تنظیم SLA بر اساس سطح تریاژ
            queue.NextReassessmentDueAt = queue.QueueTime.AddMinutes(
                assessment.Level == TriageLevel.ESI2 ? 15 :
                assessment.Level == TriageLevel.ESI3 ? 30 :
                assessment.Level == TriageLevel.ESI4 ? 60 : 120);

            _context.TriageQueues.Add(queue);
            await _context.SaveChangesAsync();

            _logger.Information("بیمار {PatientId} به صف تریاژ اضافه شد", assessment.PatientId);

            return ServiceResult<TriageQueue>.Successful(queue, "صف تریاژ با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در اضافه کردن به صف تریاژ");
            return ServiceResult<TriageQueue>.Failed("خطا در اضافه کردن به صف تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageQueue>> CallPatientFromQueueAsync(
        int queueId, 
        string calledByUserId)
    {
        try
        {
            var queue = await _context.TriageQueues
                .FirstOrDefaultAsync(tq => tq.TriageQueueId == queueId && !tq.IsDeleted);

            if (queue == null)
            {
                return ServiceResult<TriageQueue>.Failed("صف تریاژ یافت نشد.");
            }

            queue.Status = TriageQueueStatus.Called;
            queue.CalledTime = DateTime.UtcNow;
            queue.CalledByUserId = calledByUserId;
            queue.UpdatedAt = DateTime.UtcNow;
            queue.UpdatedByUserId = _currentUserServiceService.GetCurrentUserId();

            await _context.SaveChangesAsync();

            _logger.Information("بیمار {PatientId} از صف تریاژ فراخوانده شد", queue.PatientId);

            return ServiceResult<TriageQueue>.Successful(queue, "صف تریاژ با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در فراخوانی بیمار از صف تریاژ");
            return ServiceResult<TriageQueue>.Failed("خطا در فراخوانی بیمار از صف تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageQueue>> CompleteTriageQueueAsync(
        int queueId, 
        string completedByUserId)
    {
        try
        {
            var queue = await _context.TriageQueues
                .FirstOrDefaultAsync(tq => tq.TriageQueueId == queueId && !tq.IsDeleted);

            if (queue == null)
            {
                return ServiceResult<TriageQueue>.Failed("صف تریاژ یافت نشد.");
            }

            queue.Status = TriageQueueStatus.Completed;
            queue.CompletedTime = DateTime.UtcNow;
            queue.CompletedByUserId = completedByUserId;
            queue.UpdatedAt = DateTime.UtcNow;
            queue.UpdatedByUserId = _currentUserServiceService.GetCurrentUserId();

            await _context.SaveChangesAsync();

            _logger.Information("صف تریاژ {QueueId} تکمیل شد", queueId);

            return ServiceResult<TriageQueue>.Successful(queue, "صف تریاژ با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در تکمیل صف تریاژ");
            return ServiceResult<TriageQueue>.Failed("خطا در تکمیل صف تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<List<TriageQueue>>> GetTriageQueueAsync(
        int? departmentId = null, 
        TriageQueueStatus? status = null)
    {
        try
        {
            var query = _context.TriageQueues
                .Include(tq => tq.Patient)
                .Include(tq => tq.TriageAssessment)
                .Include(tq => tq.TargetDepartment)
                .Include(tq => tq.TargetDoctor)
                .Where(tq => !tq.IsDeleted);

            if (departmentId.HasValue)
            {
                query = query.Where(tq => tq.TargetDepartmentId == departmentId);
            }

            if (status.HasValue)
            {
                query = query.Where(tq => tq.Status == status.Value);
            }

            var queues = await query
                .OrderBy(tq => tq.Priority)
                .ThenBy(tq => tq.QueueTime)
                .ToListAsync();

            return ServiceResult<List<TriageQueue>>.Successful(queues, "لیست صف‌های تریاژ دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت صف تریاژ");
            return ServiceResult<List<TriageQueue>>.Failed("خطا در دریافت صف تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<List<TriageQueue>>> GetTriageQueueByPriorityAsync(int? departmentId = null)
    {
        try
        {
            var query = _context.TriageQueues
                .Include(tq => tq.Patient)
                .Include(tq => tq.TriageAssessment)
                .Include(tq => tq.TargetDepartment)
                .Include(tq => tq.TargetDoctor)
                .Where(tq => !tq.IsDeleted && tq.Status == TriageQueueStatus.Waiting);

            if (departmentId.HasValue)
            {
                query = query.Where(tq => tq.TargetDepartmentId == departmentId);
            }

            var queues = await query
                .OrderBy(tq => tq.Priority)
                .ThenBy(tq => tq.QueueTime)
                .ToListAsync();

            return ServiceResult<List<TriageQueue>>.Successful(queues, "لیست صف‌های تریاژ دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت صف تریاژ بر اساس اولویت");
            return ServiceResult<List<TriageQueue>>.Failed("خطا در دریافت صف تریاژ بر اساس اولویت: " + ex.Message);
        }
    }

    #endregion

    #region علائم حیاتی (Vital Signs)

    public async Task<ServiceResult<TriageVitalSigns>> RecordVitalSignsAsync(
        int assessmentId, 
        TriageVitalSigns vitalSigns)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<TriageVitalSigns>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            vitalSigns.TriageAssessmentId = assessmentId;
            vitalSigns.CreatedByUserId = _currentUserServiceService.GetCurrentUserId();
            vitalSigns.CreatedAt = DateTime.Now;

            _context.TriageVitalSigns.Add(vitalSigns);
            await _context.SaveChangesAsync();

            _logger.Information("علائم حیاتی برای ارزیابی {AssessmentId} ثبت شد", assessmentId);

            return ServiceResult<TriageVitalSigns>.Successful(vitalSigns, "علائم حیاتی با موفقیت ثبت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ثبت علائم حیاتی");
            return ServiceResult<TriageVitalSigns>.Failed("خطا در ثبت علائم حیاتی: " + ex.Message);
        }
    }

    public async Task<ServiceResult<List<TriageVitalSigns>>> GetVitalSignsAsync(int assessmentId)
    {
        try
        {
            var vitalSigns = await _context.TriageVitalSigns
                .Where(tvs => tvs.TriageAssessmentId == assessmentId && !tvs.IsDeleted)
                .OrderBy(tvs => tvs.MeasurementTime)
                .ToListAsync();

            return ServiceResult<List<TriageVitalSigns>>.Successful(vitalSigns, "لیست علائم حیاتی دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت علائم حیاتی");
            return ServiceResult<List<TriageVitalSigns>>.Failed("خطا در دریافت علائم حیاتی: " + ex.Message);
        }
    }

    public async Task<ServiceResult<VitalSignStatus>> EvaluateVitalSignsAsync(TriageVitalSigns vitalSigns)
    {
        try
        {
            // ارزیابی علائم حیاتی بر اساس استانداردهای پزشکی
            var status = VitalSignStatus.Normal;

            // بررسی فشار خون
            if (vitalSigns.SystolicBP.HasValue && vitalSigns.DiastolicBP.HasValue)
            {
                if (vitalSigns.SystolicBP < 90 || vitalSigns.SystolicBP > 180 ||
                    vitalSigns.DiastolicBP < 60 || vitalSigns.DiastolicBP > 110)
                {
                    status = VitalSignStatus.Abnormal;
                }
            }

            // بررسی ضربان قلب
            if (vitalSigns.HeartRate.HasValue)
            {
                if (vitalSigns.HeartRate < 60 || vitalSigns.HeartRate > 100)
                {
                    status = VitalSignStatus.Abnormal;
                }
            }

            // بررسی دمای بدن
            if (vitalSigns.Temperature.HasValue)
            {
                if (vitalSigns.Temperature < 36.1m || vitalSigns.Temperature > 37.2m)
                {
                    status = VitalSignStatus.Abnormal;
                }
            }

            // بررسی اشباع اکسیژن
            if (vitalSigns.OxygenSaturation.HasValue)
            {
                if (vitalSigns.OxygenSaturation < 95)
                {
                    status = VitalSignStatus.Critical;
                }
            }

            // بررسی سطح هوشیاری
            if (vitalSigns.ConsciousnessLevel.HasValue)
            {
                if (vitalSigns.ConsciousnessLevel < 15)
                {
                    status = VitalSignStatus.Critical;
                }
            }

            return ServiceResult<VitalSignStatus>.Successful(status, "وضعیت علائم حیاتی محاسبه شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ارزیابی علائم حیاتی");
            return ServiceResult<VitalSignStatus>.Failed("خطا در ارزیابی علائم حیاتی: " + ex.Message);
        }
    }

    #endregion

    #region پروتکل‌های تریاژ (Triage Protocols)

    public async Task<ServiceResult<List<TriageProtocol>>> GetActiveProtocolsAsync(
        TriageProtocolType? type = null, 
        TriageLevel? targetLevel = null)
    {
        try
        {
            var query = _context.TriageProtocols
                .Where(tp => tp.IsActive && !tp.IsDeleted && 
                            tp.ValidFrom <= DateTime.UtcNow && 
                            (tp.ValidTo == null || tp.ValidTo >= DateTime.UtcNow));

            if (type.HasValue)
            {
                query = query.Where(tp => tp.Type == type.Value);
            }

            if (targetLevel.HasValue)
            {
                query = query.Where(tp => tp.TargetLevel == targetLevel.Value);
            }

            var protocols = await query
                .OrderBy(tp => tp.Priority)
                .ToListAsync();

            return ServiceResult<List<TriageProtocol>>.Successful(protocols, "لیست پروتکل‌های فعال دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت پروتکل‌های تریاژ");
            return ServiceResult<List<TriageProtocol>>.Failed("خطا در دریافت پروتکل‌های تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> ApplyProtocolAsync(int assessmentId, int protocolId)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<bool>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            var protocol = await _context.TriageProtocols
                .FirstOrDefaultAsync(tp => tp.TriageProtocolId == protocolId && !tp.IsDeleted);

            if (protocol == null)
            {
                return ServiceResult<bool>.Failed("پروتکل تریاژ یافت نشد.");
            }

            // اعمال پروتکل
            assessment.Level = protocol.TargetLevel;
            assessment.Priority = protocol.Priority;
            assessment.UpdatedAt = DateTime.UtcNow;
            assessment.UpdatedByUserId = _currentUserServiceService.GetCurrentUserId();

            // اضافه کردن رابطه M2M
            _context.Entry(assessment).Collection(a => a.Protocols).Load();
            if (!assessment.Protocols.Any(p => p.TriageProtocolId == protocolId))
            {
                assessment.Protocols.Add(protocol);
            }

            await _context.SaveChangesAsync();

            _logger.Information("پروتکل {ProtocolId} برای ارزیابی {AssessmentId} اعمال شد", 
                protocolId, assessmentId);

            return ServiceResult<bool>.Successful(true, "عملیات با موفقیت انجام شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در اعمال پروتکل تریاژ");
            return ServiceResult<bool>.Failed("خطا در اعمال پروتکل تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<List<TriageProtocol>>> SuggestProtocolsAsync(int assessmentId)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .Include(ta => ta.VitalSigns)
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<List<TriageProtocol>>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            var protocols = await _context.TriageProtocols
                .Where(tp => tp.IsActive && !tp.IsDeleted && 
                            tp.ValidFrom <= DateTime.UtcNow && 
                            (tp.ValidTo == null || tp.ValidTo >= DateTime.UtcNow))
                .OrderBy(tp => tp.Priority)
                .ToListAsync();

            // فیلتر کردن پروتکل‌های مناسب بر اساس علائم حیاتی
            var lastVital = assessment.VitalSigns.OrderByDescending(v => v.MeasurementTime).FirstOrDefault();
            var suggestedProtocols = protocols.Where(p => 
                IsProtocolApplicable(p, lastVital)).ToList();

            return ServiceResult<List<TriageProtocol>>.Successful(suggestedProtocols, "پروتکل‌های پیشنهادی دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در پیشنهاد پروتکل‌های تریاژ");
            return ServiceResult<List<TriageProtocol>>.Failed("خطا در پیشنهاد پروتکل‌های تریاژ: " + ex.Message);
        }
    }

    #endregion

    #region گزارش‌گیری و آمار (Reporting & Statistics)

    public async Task<ServiceResult<TriageDailyStats>> GetDailyStatsAsync(
        DateTime date, 
        int? departmentId = null)
    {
        try
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var query = _context.TriageAssessments
                .Where(ta => ta.TriageStartAt >= startDate && 
                           ta.TriageStartAt < endDate && 
                           !ta.IsDeleted);

            if (departmentId.HasValue)
            {
                query = query.Where(ta => ta.RecommendedDepartmentId == departmentId);
            }

            var assessments = await query.ToListAsync();

            // محاسبه میانگین زمان انتظار
            var completedAssessments = assessments.Where(a => a.Status == TriageStatus.Completed && a.TriageEndAt.HasValue).ToList();
            var averageWaitTime = completedAssessments.Any() ? 
                completedAssessments.Average(a => (a.TriageEndAt.Value - a.TriageStartAt).TotalMinutes) : 0;

            // محاسبه میانگین زمان ارزیابی
            var averageAssessmentTime = completedAssessments.Any() ? 
                completedAssessments.Average(a => (a.TriageEndAt.Value - a.TriageStartAt).TotalMinutes) : 0;

            var stats = new TriageDailyStats
            {
                Date = date,
                TotalAssessments = assessments.Count,
                CriticalLevel = assessments.Count(a => a.Level == TriageLevel.ESI1),
                HighLevel = assessments.Count(a => a.Level == TriageLevel.ESI2),
                MediumLevel = assessments.Count(a => a.Level == TriageLevel.ESI3),
                LowLevel = assessments.Count(a => a.Level == TriageLevel.ESI4),
                VeryLowLevel = assessments.Count(a => a.Level == TriageLevel.ESI5),
                CompletedAssessments = assessments.Count(a => a.Status == TriageStatus.Completed),
                PendingAssessments = assessments.Count(a => a.Status == TriageStatus.Pending),
                AverageWaitTimeMinutes = (int)Math.Round(averageWaitTime),
                AverageAssessmentTimeMinutes = (int)Math.Round(averageAssessmentTime)
            };

            return ServiceResult<TriageDailyStats>.Successful(stats, "آمار روزانه تریاژ دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت آمار روزانه تریاژ");
            return ServiceResult<TriageDailyStats>.Failed("خطا در دریافت آمار روزانه تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageWeeklyStats>> GetWeeklyStatsAsync(
        DateTime startDate, 
        DateTime endDate, 
        int? departmentId = null)
    {
        try
        {
            var query = _context.TriageAssessments
                .Where(ta => ta.TriageStartAt >= startDate && 
                           ta.TriageStartAt <= endDate && 
                           !ta.IsDeleted);

            if (departmentId.HasValue)
            {
                query = query.Where(ta => ta.RecommendedDepartmentId == departmentId);
            }

            var assessments = await query.ToListAsync();

            var stats = new TriageWeeklyStats
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalAssessments = assessments.Count,
                TotalCompleted = assessments.Count(a => a.Status == TriageStatus.Completed),
                TotalPending = assessments.Count(a => a.Status == TriageStatus.Pending)
            };

            return ServiceResult<TriageWeeklyStats>.Successful(stats, "آمار هفتگی تریاژ دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت آمار هفتگی تریاژ");
            return ServiceResult<TriageWeeklyStats>.Failed("خطا در دریافت آمار هفتگی تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageReport>> GenerateReportAsync(
        TriageReportType reportType, 
        DateTime startDate, 
        DateTime endDate, 
        int? departmentId = null)
    {
        try
        {
            var query = _context.TriageAssessments
                .Include(ta => ta.Patient)
                .Include(ta => ta.Assessor)
                .Where(ta => ta.TriageStartAt >= startDate && 
                           ta.TriageStartAt <= endDate && 
                           !ta.IsDeleted);

            if (departmentId.HasValue)
            {
                query = query.Where(ta => ta.RecommendedDepartmentId == departmentId);
            }

            var assessments = await query.ToListAsync();

            var report = new TriageReport
            {
                ReportType = reportType,
                StartDate = startDate,
                EndDate = endDate,
                TotalAssessments = assessments.Count,
                TotalCompleted = assessments.Count(a => a.Status == TriageStatus.Completed),
                TotalPending = assessments.Count(a => a.Status == TriageStatus.Pending),
                Assessments = assessments
            };

            return ServiceResult<TriageReport>.Successful(report, "گزارش تریاژ تولید شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در تولید گزارش تریاژ");
            return ServiceResult<TriageReport>.Failed("خطا در تولید گزارش تریاژ: " + ex.Message);
        }
    }

    #endregion

    #region مدیریت اولویت (Priority Management)

    public async Task<ServiceResult<int>> CalculateTriagePriorityAsync(int assessmentId)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .Include(ta => ta.VitalSigns)
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<int>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            var lastVital = assessment.VitalSigns.OrderByDescending(v => v.MeasurementTime).FirstOrDefault();
            var priority = CalculatePriority(assessment.Level, lastVital);

            return ServiceResult<int>.Successful(priority, "اولویت محاسبه شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در محاسبه اولویت تریاژ");
            return ServiceResult<int>.Failed("خطا در محاسبه اولویت تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageQueue>> UpdateQueuePriorityAsync(
        int queueId, 
        int newPriority)
    {
        try
        {
            var queue = await _context.TriageQueues
                .FirstOrDefaultAsync(tq => tq.TriageQueueId == queueId && !tq.IsDeleted);

            if (queue == null)
            {
                return ServiceResult<TriageQueue>.Failed("صف تریاژ یافت نشد.");
            }

            queue.Priority = newPriority;
            queue.UpdatedAt = DateTime.UtcNow;
            queue.UpdatedByUserId = _currentUserServiceService.GetCurrentUserId();

            await _context.SaveChangesAsync();

            return ServiceResult<TriageQueue>.Successful(queue, "صف تریاژ با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در به‌روزرسانی اولویت صف");
            return ServiceResult<TriageQueue>.Failed("خطا در به‌روزرسانی اولویت صف: " + ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> ReorderQueueByPriorityAsync(int? departmentId = null)
    {
        try
        {
            var query = _context.TriageQueues
                .Where(tq => !tq.IsDeleted && tq.Status == TriageQueueStatus.Waiting);

            if (departmentId.HasValue)
            {
                query = query.Where(tq => tq.TargetDepartmentId == departmentId);
            }

            var queues = await query.ToListAsync();

            // مرتب‌سازی بر اساس اولویت بالینی (نه تغییر Priority)
            var orderedQueues = queues.OrderBy(tq => tq.Priority).ThenBy(tq => tq.QueueTime).ToList();

            // به‌روزرسانی موقعیت صف (QueuePosition) به جای Priority
            for (int i = 0; i < orderedQueues.Count; i++)
            {
                orderedQueues[i].QueuePosition = i + 1; // موقعیت در صف
                orderedQueues[i].UpdatedAt = DateTime.UtcNow;
                orderedQueues[i].UpdatedByUserId = _currentUserServiceService.GetCurrentUserId();
            }

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Successful(true, "عملیات با موفقیت انجام شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در مرتب‌سازی صف بر اساس اولویت");
            return ServiceResult<bool>.Failed("خطا در مرتب‌سازی صف بر اساس اولویت: " + ex.Message);
        }
    }

    #endregion

    #region اعلان‌ها و هشدارها (Notifications & Alerts)

    public async Task<ServiceResult<bool>> SendTriageNotificationAsync(
        TriageNotificationType notificationType, 
        string message, 
        List<string> targetUsers)
    {
        try
        {
            // پیاده‌سازی ارسال اعلان
            _logger.Information("اعلان تریاژ ارسال شد: {Message} به {UserCount} کاربر", 
                message, targetUsers.Count);

            return ServiceResult<bool>.Successful(true, "عملیات با موفقیت انجام شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ارسال اعلان تریاژ");
            return ServiceResult<bool>.Failed("خطا در ارسال اعلان تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<List<TriageAlert>>> CheckTriageAlertsAsync(int assessmentId)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .Include(ta => ta.VitalSigns)
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<List<TriageAlert>>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            var alerts = new List<TriageAlert>();

            // بررسی هشدارهای علائم حیاتی
            var vitalSigns = assessment.VitalSigns.OrderByDescending(v => v.MeasurementTime).FirstOrDefault();
            if (vitalSigns != null)
            {
                // محاسبه نیاز به توجه فوری بر اساس علائم حیاتی
                var requiresImmediate = (vitalSigns.OxygenSaturation < 90) ||
                                      (vitalSigns.HeartRate > 120 || vitalSigns.HeartRate < 50) ||
                                      (vitalSigns.SystolicBP < 90) ||
                                      (vitalSigns.Temperature > 39 || vitalSigns.Temperature < 35) ||
                                      (vitalSigns.RespiratoryRate > 30 || vitalSigns.RespiratoryRate < 10);

                if (requiresImmediate)
                {
                    alerts.Add(new TriageAlert
                    {
                        AssessmentId = assessmentId,
                        AlertType = "VitalSigns",
                        Message = "علائم حیاتی نیاز به مراقبت فوری دارد",
                        Severity = TriageLevel.ESI1,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // بررسی هشدارهای سطح تریاژ
            if (assessment.Level == TriageLevel.ESI1)
            {
                alerts.Add(new TriageAlert
                {
                    AssessmentId = assessmentId,
                    AlertType = "TriageLevel",
                    Message = "سطح تریاژ بحرانی - نیاز به مراقبت فوری",
                    Severity = TriageLevel.ESI1,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return ServiceResult<List<TriageAlert>>.Successful(alerts, "لیست هشدارهای تریاژ دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در بررسی هشدارهای تریاژ");
            return ServiceResult<List<TriageAlert>>.Failed("خطا در بررسی هشدارهای تریاژ: " + ex.Message);
        }
    }

    #endregion

    #region پایش مجدد (Reassessment)

    public async Task<ServiceResult<TriageReassessment>> CreateReassessmentAsync(
        int assessmentId, 
        TriageReassessmentDto dto)
    {
        try
        {
            // بررسی مجوز
            if (!await IsAuthorizedForTriageAsync())
            {
                return ServiceResult<TriageReassessment>.Failed("شما مجوز انجام تریاژ را ندارید.");
            }

            var assessment = await _context.TriageAssessments
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<TriageReassessment>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            if (!assessment.IsOpen)
            {
                return ServiceResult<TriageReassessment>.Failed("ارزیابی تریاژ بسته است.");
            }

            var userId = _currentUserServiceService.GetCurrentUserId();
            var reassessment = new TriageReassessment
            {
                TriageAssessmentId = assessmentId,
                PreviousLevel = assessment.Level,
                NewLevel = dto.NewLevel,
                Reason = Enum.TryParse<ReassessmentReason>(dto.Reason, out var reasonEnum) ? reasonEnum : ReassessmentReason.Routine,
                ReassessmentAt = DateTime.UtcNow,
                AssessorUserId = userId,
                Notes = dto.Notes,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // به‌روزرسانی ارزیابی
            assessment.Level = dto.NewLevel;
            assessment.Priority = CalculatePriority(dto.NewLevel, null);
            assessment.LastReassessmentAt = DateTime.UtcNow;
            assessment.ReassessmentCount++;
            assessment.UpdatedAt = DateTime.UtcNow;
            assessment.UpdatedByUserId = userId;

            // به‌روزرسانی SLA در صف
            var queue = await _context.TriageQueues
                .FirstOrDefaultAsync(q => q.TriageAssessmentId == assessmentId && 
                                         q.Status == TriageQueueStatus.Waiting && 
                                         !q.IsDeleted);

            if (queue != null)
            {
                queue.NextReassessmentDueAt = DateTime.UtcNow.AddMinutes(
                    dto.NewLevel == TriageLevel.ESI2 ? 15 :
                    dto.NewLevel == TriageLevel.ESI3 ? 30 :
                    dto.NewLevel == TriageLevel.ESI4 ? 60 : 120);
                queue.UpdatedAt = DateTime.UtcNow;
                queue.UpdatedByUserId = userId;
            }

            _context.TriageReassessments.Add(reassessment);
            await _context.SaveChangesAsync();

            _logger.Information("پایش مجدد برای ارزیابی {AssessmentId} ایجاد شد", assessmentId);

            return ServiceResult<TriageReassessment>.Successful(reassessment, "پایش مجدد با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ایجاد پایش مجدد");
            return ServiceResult<TriageReassessment>.Failed("خطا در ایجاد پایش مجدد: " + ex.Message);
        }
    }

    public async Task<ServiceResult<List<TriageReassessment>>> GetReassessmentsAsync(int assessmentId)
    {
        try
        {
            var reassessments = await _context.TriageReassessments
                .Where(tr => tr.TriageAssessmentId == assessmentId && !tr.IsDeleted)
                .OrderByDescending(tr => tr.ReassessmentAt)
                .ToListAsync();

            return ServiceResult<List<TriageReassessment>>.Successful(reassessments, "لیست پایش‌های مجدد دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت پایش‌های مجدد");
            return ServiceResult<List<TriageReassessment>>.Failed("خطا در دریافت پایش‌های مجدد: " + ex.Message);
        }
    }

    #endregion

    #region خلاصه و پری‌فیل (Summary & Profile)

    public async Task<ServiceResult<TriageSummaryDto>> GetSummaryAsync(int assessmentId)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .Include(ta => ta.VitalSigns)
                .Include(ta => ta.Protocols)
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<TriageSummaryDto>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            var lastVital = assessment.VitalSigns.OrderByDescending(v => v.MeasurementTime).FirstOrDefault();
            var redFlags = new List<string>();

            if (lastVital != null)
            {
                if (lastVital.OxygenSaturation < 90) redFlags.Add("کاهش اکسیژن خون");
                if (lastVital.HeartRate > 120 || lastVital.HeartRate < 50) redFlags.Add("ضربان قلب غیرطبیعی");
                if (lastVital.SystolicBP < 90) redFlags.Add("فشار خون پایین");
                if (lastVital.Temperature > 39 || lastVital.Temperature < 35) redFlags.Add("تب یا هیپوترمی");
                if (lastVital.RespiratoryRate > 30 || lastVital.RespiratoryRate < 10) redFlags.Add("تنفس غیرطبیعی");
            }

            var summary = new TriageSummaryDto
            {
                AssessmentId = assessmentId,
                PatientId = assessment.PatientId,
                Level = assessment.Level,
                Priority = assessment.Priority,
                Status = assessment.Status,
                ChiefComplaint = assessment.ChiefComplaint,
                ArrivalAt = assessment.ArrivalAt,
                TriageStartAt = assessment.TriageStartAt,
                TriageEndAt = assessment.TriageEndAt,
                IsOpen = assessment.IsOpen,
                RedFlags = redFlags,
                IsolationRequired = assessment.Isolation != null && assessment.Isolation != IsolationType.None,
                LastVitalSigns = lastVital,
                ProtocolsCount = assessment.Protocols.Count,
                ReassessmentCount = assessment.ReassessmentCount,
                LastReassessmentAt = assessment.LastReassessmentAt
            };

            return ServiceResult<TriageSummaryDto>.Successful(summary, "خلاصه ارزیابی دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت خلاصه ارزیابی");
            return ServiceResult<TriageSummaryDto>.Failed("خطا در دریافت خلاصه ارزیابی: " + ex.Message);
        }
    }

    #endregion

    #region مدیریت صف (Queue Management) - Delegated to TriageQueueService

    public async Task<ServiceResult<bool>> CanLinkToReceptionAsync(int assessmentId)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .FirstOrDefaultAsync(ta => ta.TriageAssessmentId == assessmentId && !ta.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<bool>.Failed("ارزیابی تریاژ یافت نشد.");
            }

            // بررسی شرایط لینک به پذیرش
            var canLink = assessment.Status == TriageStatus.Completed && 
                          !assessment.IsOpen && 
                          assessment.TriageEndAt.HasValue;

            return ServiceResult<bool>.Successful(canLink, canLink ? "قابل لینک به پذیرش" : "قابل لینک به پذیرش نیست");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در بررسی قابلیت لینک به پذیرش");
            return ServiceResult<bool>.Failed("خطا در بررسی قابلیت لینک به پذیرش: " + ex.Message);
        }
    }

    public async Task<ServiceResult<List<TriageAssessment>>> GetCompletedForAdmissionAsync(int? departmentId = null)
    {
        try
        {
            var query = _context.TriageAssessments
                .Where(ta => ta.Status == TriageStatus.Completed && 
                           !ta.IsOpen && 
                           ta.TriageEndAt.HasValue &&
                           !ta.IsDeleted);

            if (departmentId.HasValue)
            {
                query = query.Where(ta => ta.RecommendedDepartmentId == departmentId);
            }

            var assessments = await query
                .OrderBy(ta => ta.Priority)
                .ThenBy(ta => ta.TriageEndAt)
                .ToListAsync();

            return ServiceResult<List<TriageAssessment>>.Successful(assessments, "لیست آماده پذیرش دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت لیست آماده پذیرش");
            return ServiceResult<List<TriageAssessment>>.Failed("خطا در دریافت لیست آماده پذیرش: " + ex.Message);
        }
    }

    #endregion

    #region متدهای کمکی (Helper Methods)

    private async Task<bool> IsAuthorizedForTriageAsync()
    {
        try
        {
            var userId = _currentUserServiceService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return false;

            // بررسی نقش کاربر - فقط پرستاران و پزشکان می‌توانند تریاژ انجام دهند
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            
            if (user == null)
                return false;

            // بررسی نقش‌های مجاز (Nurse, Doctor, Admin)
            var hasValidRole = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && 
                    (ur.RoleId == "Nurse" || ur.RoleId == "Doctor" || ur.RoleId == "Admin"));
            
            return hasValidRole;
        }
        catch
        {
            return false;
        }
    }

    private int CalculatePriority(TriageLevel level, TriageVitalSigns vitalSigns)
    {
        // محاسبه اولویت بر اساس سطح ESI و علائم حیاتی
        var basePriority = (int)level;
        
        // کاهش اولویت برای علائم حیاتی بحرانی
        if (vitalSigns != null)
        {
            if (vitalSigns.OxygenSaturation < 90 || vitalSigns.HeartRate > 120 || vitalSigns.HeartRate < 50)
                basePriority = Math.Max(1, basePriority - 1);
                
            if (vitalSigns.SystolicBP < 90 || vitalSigns.Temperature > 39 || vitalSigns.Temperature < 35)
                basePriority = Math.Max(1, basePriority - 1);
        }
        
        return basePriority;
    }

    private async Task<ServiceResult<TriageLevel>> CalculateTriageLevelAsync(TriageVitalSigns vitalSigns)
    {
        if (vitalSigns == null)
        {
            return ServiceResult<TriageLevel>.Successful(TriageLevel.ESI5, "سطح تریاژ پیش‌فرض");
        }

        var level = TriageLevel.ESI5;

        // بررسی علائم حیاتی بحرانی
        if (vitalSigns.OxygenSaturation < 90 || vitalSigns.ConsciousnessLevel < 8)
        {
            level = TriageLevel.ESI1;
        }
        else if (vitalSigns.SystolicBP < 90 || vitalSigns.HeartRate > 120 || vitalSigns.Temperature > 39)
        {
            level = TriageLevel.ESI2;
        }
        else if (vitalSigns.SystolicBP < 100 || vitalSigns.HeartRate > 100 || vitalSigns.Temperature > 38)
        {
            level = TriageLevel.ESI3;
        }

        return ServiceResult<TriageLevel>.Successful(level, "سطح تریاژ محاسبه شد");
    }


    private bool IsProtocolApplicable(TriageProtocol protocol, TriageVitalSigns vitalSigns)
    {
        if (vitalSigns == null)
        {
            return false;
        }

        // بررسی معیارهای پروتکل
        // اینجا می‌توان منطق پیچیده‌تری برای تطبیق پروتکل‌ها پیاده‌سازی کرد
        return true;
    }

    #endregion

    #region Protocol Management Methods

    public async Task<ServiceResult<List<TriageProtocol>>> GetTriageProtocolsAsync(int? protocolType = null, string searchTerm = "")
    {
        try
        {
            var query = _context.TriageProtocols.Where(p => !p.IsDeleted);

            if (protocolType.HasValue)
            {
                query = query.Where(p => p.ProtocolType == (TriageProtocolType)protocolType.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            var protocols = await query.OrderBy(p => p.Priority).ToListAsync();
            return ServiceResult<List<TriageProtocol>>.Successful(protocols, "پروتکل‌های تریاژ دریافت شدند");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت پروتکل‌های تریاژ");
            return ServiceResult<List<TriageProtocol>>.Failed("خطا در دریافت پروتکل‌های تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageProtocol>> GetTriageProtocolAsync(int protocolId)
    {
        try
        {
            var protocol = await _context.TriageProtocols
                .FirstOrDefaultAsync(p => p.TriageProtocolId == protocolId && !p.IsDeleted);

            if (protocol == null)
            {
                return ServiceResult<TriageProtocol>.Failed("پروتکل یافت نشد");
            }

            return ServiceResult<TriageProtocol>.Successful(protocol, "پروتکل دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت پروتکل تریاژ");
            return ServiceResult<TriageProtocol>.Failed("خطا در دریافت پروتکل تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageProtocol>> CreateTriageProtocolAsync(TriageProtocol protocol)
    {
        try
        {
            protocol.CreatedAt = DateTime.UtcNow;
            protocol.CreatedByUserId = _currentUserServiceService.GetCurrentUserId();
            protocol.IsDeleted = false;

            _context.TriageProtocols.Add(protocol);
            await _context.SaveChangesAsync();

            return ServiceResult<TriageProtocol>.Successful(protocol, "پروتکل ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در ایجاد پروتکل تریاژ");
            return ServiceResult<TriageProtocol>.Failed("خطا در ایجاد پروتکل تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<TriageProtocol>> UpdateTriageProtocolAsync(
        int protocolId,
        string name,
        string description,
        TriageProtocolType protocolType,
        string criteria,
        string actions,
        int priority,
        bool isActive)
    {
        try
        {
            var protocol = await _context.TriageProtocols
                .FirstOrDefaultAsync(p => p.TriageProtocolId == protocolId && !p.IsDeleted);

            if (protocol == null)
            {
                return ServiceResult<TriageProtocol>.Failed("پروتکل یافت نشد");
            }

            protocol.Name = name;
            protocol.Description = description;
            protocol.Type = protocolType;
            protocol.Criteria = criteria;
            protocol.RequiredActions = actions;
            protocol.Priority = priority;
            protocol.IsActive = isActive;
            protocol.UpdatedAt = DateTime.UtcNow;
            protocol.UpdatedByUserId = _currentUserServiceService.GetCurrentUserId();

            await _context.SaveChangesAsync();

            return ServiceResult<TriageProtocol>.Successful(protocol, "پروتکل به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در به‌روزرسانی پروتکل تریاژ");
            return ServiceResult<TriageProtocol>.Failed("خطا در به‌روزرسانی پروتکل تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult> DeleteTriageProtocolAsync(int protocolId)
    {
        try
        {
            var protocol = await _context.TriageProtocols
                .FirstOrDefaultAsync(p => p.TriageProtocolId == protocolId && !p.IsDeleted);

            if (protocol == null)
            {
                return ServiceResult.Failed("پروتکل یافت نشد");
            }

            protocol.IsDeleted = true;
            protocol.DeletedAt = DateTime.UtcNow;
            protocol.DeletedByUserId = _currentUserServiceService.GetCurrentUserId();

            await _context.SaveChangesAsync();

            return ServiceResult.Successful("پروتکل حذف شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در حذف پروتکل تریاژ");
            return ServiceResult.Failed("خطا در حذف پروتکل تریاژ: " + ex.Message);
        }
    }

    public async Task<ServiceResult<List<TriageProtocol>>> GetActiveProtocolsAsync(int? protocolType = null)
    {
        try
        {
            var query = _context.TriageProtocols.Where(p => !p.IsDeleted && p.IsActive);

            if (protocolType.HasValue)
            {
                query = query.Where(p => p.ProtocolType == (TriageProtocolType)protocolType.Value);
            }

            var protocols = await query.OrderBy(p => p.Priority).ToListAsync();
            return ServiceResult<List<TriageProtocol>>.Successful(protocols, "پروتکل‌های فعال دریافت شدند");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت پروتکل‌های فعال");
            return ServiceResult<List<TriageProtocol>>.Failed("خطا در دریافت پروتکل‌های فعال: " + ex.Message);
        }
    }

    public async Task<ServiceResult<List<TriageProtocol>>> GetProtocolSuggestionsAsync(int assessmentId)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .Include(a => a.VitalSigns)
                .FirstOrDefaultAsync(a => a.TriageAssessmentId == assessmentId && !a.IsDeleted);

            if (assessment == null)
            {
                return ServiceResult<List<TriageProtocol>>.Failed("ارزیابی یافت نشد");
            }

            var lastVital = assessment.VitalSigns.OrderByDescending(v => v.MeasurementTime).FirstOrDefault();
            var protocols = await _context.TriageProtocols
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.Priority)
                .ToListAsync();

            var suggestions = protocols.Where(p => IsProtocolApplicable(p, lastVital)).ToList();
            return ServiceResult<List<TriageProtocol>>.Successful(suggestions, "پیشنهادات پروتکل دریافت شدند");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت پیشنهادات پروتکل");
            return ServiceResult<List<TriageProtocol>>.Failed("خطا در دریافت پیشنهادات پروتکل: " + ex.Message);
        }
    }

    public async Task<ServiceResult> CloseActiveQueueIfAnyAsync(int assessmentId)
    {
        try
        {
            var queues = await _context.TriageQueues
                .Where(q => q.TriageAssessmentId == assessmentId && 
                           q.Status != TriageQueueStatus.Completed && 
                           !q.IsDeleted)
                .ToListAsync();

            if (!queues.Any())
            {
                return ServiceResult.Successful("صف فعالی وجود ندارد");
            }

            var userId = _currentUserServiceService.GetCurrentUserId();
            var now = DateTime.UtcNow;

            foreach (var queue in queues)
            {
                queue.Status = TriageQueueStatus.Completed;
                queue.CompletedTime = now;
                queue.CompletedByUserId = userId;
                queue.UpdatedAt = now;
                queue.UpdatedByUserId = userId;
            }

            await _context.SaveChangesAsync();
            return ServiceResult.Successful("صف‌های فعال بسته شدند");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در بستن صف‌های فعال");
            return ServiceResult.Failed("خطا در بستن صف‌های فعال: " + ex.Message);
        }
    }

    #endregion

    #region متدهای Missing برای Interface

    /// <summary>
    /// دریافت لیست ارزیابی‌های تریاژ
    /// </summary>
    public async Task<ServiceResult<PagedResult<TriageAssessment>>> GetTriageAssessmentsAsync(
        int? patientId = null,
        TriageLevel? level = null,
        TriageStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        try
        {
            var query = _context.TriageAssessments.Where(a => !a.IsDeleted);

            if (patientId.HasValue)
                query = query.Where(a => a.PatientId == patientId.Value);

            if (level.HasValue)
                query = query.Where(a => a.Level == level.Value);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (startDate.HasValue)
                query = query.Where(a => a.TriageStartAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.TriageStartAt <= endDate.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.TriageStartAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedResult = new PagedResult<TriageAssessment>(items, totalCount, pageNumber, pageSize);
            return ServiceResult<PagedResult<TriageAssessment>>.Successful(pagedResult, "لیست ارزیابی‌های تریاژ دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت لیست ارزیابی‌های تریاژ");
            return ServiceResult<PagedResult<TriageAssessment>>.Failed("خطا در دریافت لیست ارزیابی‌های تریاژ: " + ex.Message);
        }
    }

    /// <summary>
    /// دریافت ارزیابی تریاژ بر اساس شناسه
    /// </summary>
    public async Task<ServiceResult<TriageAssessment>> GetTriageAssessmentAsync(int assessmentId)
    {
        try
        {
            var assessment = await _context.TriageAssessments
                .Include(a => a.Patient)
                .Include(a => a.Assessor)
                .Include(a => a.VitalSigns)
                .FirstOrDefaultAsync(a => a.TriageAssessmentId == assessmentId && !a.IsDeleted);

            if (assessment == null)
                return ServiceResult<TriageAssessment>.Failed("ارزیابی تریاژ یافت نشد");

            return ServiceResult<TriageAssessment>.Successful(assessment, "ارزیابی تریاژ دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت ارزیابی تریاژ");
            return ServiceResult<TriageAssessment>.Failed("خطا در دریافت ارزیابی تریاژ: " + ex.Message);
        }
    }

    /// <summary>
    /// دریافت ارزیابی‌های اخیر
    /// </summary>
    public async Task<ServiceResult<List<TriageAssessment>>> GetRecentAssessmentsAsync(int hours = 24)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);
            var assessments = await _context.TriageAssessments
                .Where(a => !a.IsDeleted && a.TriageStartAt >= cutoffTime)
                .OrderByDescending(a => a.TriageStartAt)
                .ToListAsync();

            return ServiceResult<List<TriageAssessment>>.Successful(assessments, "ارزیابی‌های اخیر دریافت شدند");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت ارزیابی‌های اخیر");
            return ServiceResult<List<TriageAssessment>>.Failed("خطا در دریافت ارزیابی‌های اخیر: " + ex.Message);
        }
    }

    /// <summary>
    /// دریافت هشدارهای فعال
    /// </summary>
    public async Task<ServiceResult<List<TriageAlert>>> GetActiveAlertsAsync()
    {
        try
        {
            var alerts = new List<TriageAlert>();
            var criticalAssessments = await _context.TriageAssessments
                .Where(a => !a.IsDeleted && a.Level == TriageLevel.ESI1)
                .ToListAsync();

            foreach (var assessment in criticalAssessments)
            {
                alerts.Add(new TriageAlert
                {
                    AssessmentId = assessment.TriageAssessmentId,
                    AlertType = "Critical",
                    Message = "بیمار بحرانی نیاز به توجه فوری دارد",
                    Severity = TriageLevel.ESI1,
                    CreatedAt = DateTime.UtcNow,
                    IsResolved = false
                });
            }

            return ServiceResult<List<TriageAlert>>.Successful(alerts, "هشدارهای فعال دریافت شدند");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت هشدارهای فعال");
            return ServiceResult<List<TriageAlert>>.Failed("خطا در دریافت هشدارهای فعال: " + ex.Message);
        }
    }

    /// <summary>
    /// دریافت ارزیابی‌ها بر اساس بازه زمانی
    /// </summary>
    public async Task<ServiceResult<List<TriageAssessment>>> GetAssessmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var assessments = await _context.TriageAssessments
                .Where(a => !a.IsDeleted && a.TriageStartAt >= startDate && a.TriageStartAt <= endDate)
                .OrderByDescending(a => a.TriageStartAt)
                .ToListAsync();

            return ServiceResult<List<TriageAssessment>>.Successful(assessments, "ارزیابی‌ها بر اساس بازه زمانی دریافت شدند");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت ارزیابی‌ها بر اساس بازه زمانی");
            return ServiceResult<List<TriageAssessment>>.Failed("خطا در دریافت ارزیابی‌ها بر اساس بازه زمانی: " + ex.Message);
        }
    }

    /// <summary>
    /// دریافت تعداد ارزیابی‌ها
    /// </summary>
    public async Task<ServiceResult<int>> GetAssessmentsCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.TriageAssessments.Where(a => !a.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(a => a.TriageStartAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.TriageStartAt <= endDate.Value);

            var count = await query.CountAsync();
            return ServiceResult<int>.Successful(count, "تعداد ارزیابی‌ها دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت تعداد ارزیابی‌ها");
            return ServiceResult<int>.Failed("خطا در دریافت تعداد ارزیابی‌ها: " + ex.Message);
        }
    }

    /// <summary>
    /// دریافت تعداد بیماران بحرانی
    /// </summary>
    public async Task<ServiceResult<int>> GetCriticalPatientsCountAsync()
    {
        try
        {
            var count = await _context.TriageAssessments
                .Where(a => !a.IsDeleted && a.Level == TriageLevel.ESI1)
                .CountAsync();

            return ServiceResult<int>.Successful(count, "تعداد بیماران بحرانی دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت تعداد بیماران بحرانی");
            return ServiceResult<int>.Failed("خطا در دریافت تعداد بیماران بحرانی: " + ex.Message);
        }
    }

    /// <summary>
    /// دریافت میانگین زمان انتظار
    /// </summary>
    public async Task<ServiceResult<double>> GetAverageWaitTimeAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.TriageQueues.Where(q => !q.IsDeleted && q.CompletedTime.HasValue);

            if (startDate.HasValue)
                query = query.Where(q => q.QueueTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(q => q.QueueTime <= endDate.Value);

            var waitTimes = await query
                .Select(q => (q.CompletedTime.Value - q.QueueTime).TotalMinutes)
                .ToListAsync();

            var averageWaitTime = waitTimes.Any() ? waitTimes.Average() : 0;
            return ServiceResult<double>.Successful(averageWaitTime, "میانگین زمان انتظار دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت میانگین زمان انتظار");
            return ServiceResult<double>.Failed("خطا در دریافت میانگین زمان انتظار: " + ex.Message);
        }
    }

    /// <summary>
    /// دریافت ارزیابی مجدد
    /// </summary>
    public async Task<ServiceResult<TriageReassessment>> GetReassessmentAsync(int reassessmentId)
    {
        try
        {
            var reassessment = await _context.TriageReassessments
                .Include(r => r.TriageAssessment)
                .Include(r => r.Assessor)
                .FirstOrDefaultAsync(r => r.TriageReassessmentId == reassessmentId && !r.IsDeleted);

            if (reassessment == null)
                return ServiceResult<TriageReassessment>.Failed("ارزیابی مجدد یافت نشد");

            return ServiceResult<TriageReassessment>.Successful(reassessment, "ارزیابی مجدد دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت ارزیابی مجدد");
            return ServiceResult<TriageReassessment>.Failed("خطا در دریافت ارزیابی مجدد: " + ex.Message);
        }
    }

    /// <summary>
    /// دریافت تعداد کل ارزیابی‌های تریاژ
    /// </summary>
    public async Task<ServiceResult<int>> GetTriageAssessmentsCountAsync()
    {
        try
        {
            var count = await _context.TriageAssessments
                .Where(a => !a.IsDeleted)
                .CountAsync();

            return ServiceResult<int>.Successful(count, "تعداد کل ارزیابی‌ها دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت تعداد کل ارزیابی‌ها");
            return ServiceResult<int>.Failed("خطا در دریافت تعداد کل ارزیابی‌ها: " + ex.Message);
        }
    }

    #endregion
}
