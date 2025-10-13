using ClinicApp.Models.Entities.Triage;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Triage;
using ClinicApp.Services.UserContext;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Enums;

namespace ClinicApp.Services.Triage
{
    /// <summary>
    /// سرویس مدیریت صف تریاژ - SRP محور
    /// </summary>
    public class TriageQueueService : ITriageQueueService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public TriageQueueService(
            ApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ServiceResult<TriageQueue>> AddToTriageQueueAsync(int assessmentId, int priority, DateTime? queueTimeUtc = null)
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

                var queueTime = queueTimeUtc ?? DateTime.UtcNow;
                var userId = _currentUserService.GetCurrentUserId();

                var queue = new TriageQueue
                {
                    PatientId = assessment.PatientId,
                    TriageAssessmentId = assessmentId,
                    Priority = priority,
                    QueueTime = queueTime,
                    Status = TriageQueueStatus.Waiting,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                // تنظیم SLA بر اساس سطح تریاژ
                queue.NextReassessmentDueAt = queueTime.AddMinutes(
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

        public async Task<ServiceResult<List<TriageQueueItemDto>>> GetWaitingAsync(int? departmentId = null)
        {
            try
            {
                var query = _context.TriageQueues
                    .Include(q => q.TriageAssessment)
                    .Include(q => q.TriageAssessment.Patient)
                    .Where(q => q.Status == TriageQueueStatus.Waiting && !q.IsDeleted);

                if (departmentId.HasValue)
                {
                    query = query.Where(q => q.TargetDepartmentId == departmentId);
                }

                var queues = await query
                    .OrderBy(q => q.Priority)
                    .ThenBy(q => q.QueueTime)
                    .ToListAsync();

                var result = queues.Select(q => new TriageQueueItemDto
                {
                    QueueId = q.TriageQueueId,
                    TriageAssessmentId = q.TriageAssessmentId,
                    PatientId = q.PatientId,
                    PatientFullName = q.TriageAssessment?.Patient?.FullName ?? "نامشخص",
                    Level = q.TriageAssessment?.Level ?? TriageLevel.ESI5,
                    Priority = q.Priority,
                    QueueTimeUtc = q.QueueTime,
                    NextReassessmentDueAtUtc = q.NextReassessmentDueAt,
                    ReassessmentCount = q.ReassessmentCount,
                    Status = "Waiting",
                    ChiefComplaint = q.TriageAssessment?.ChiefComplaint ?? "",
                    IsOverdue = q.NextReassessmentDueAt.HasValue && q.NextReassessmentDueAt < DateTime.UtcNow,
                    EstimatedWaitTimeMinutes = q.EstimatedWaitTimeMinutes
                }).ToList();

                return ServiceResult<List<TriageQueueItemDto>>.Successful(result, "لیست صف انتظار دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست صف انتظار");
                return ServiceResult<List<TriageQueueItemDto>>.Failed("خطا در دریافت لیست صف انتظار: " + ex.Message);
            }
        }

        public async Task<ServiceResult<TriageQueue>> CallNextAsync(int? departmentId = null)
        {
            try
            {
                var query = _context.TriageQueues
                    .Where(q => q.Status == TriageQueueStatus.Waiting && !q.IsDeleted);

                if (departmentId.HasValue)
                {
                    query = query.Where(q => q.TargetDepartmentId == departmentId);
                }

                var nextQueue = await query
                    .OrderBy(q => q.Priority)
                    .ThenBy(q => q.QueueTime)
                    .FirstOrDefaultAsync();

                if (nextQueue == null)
                {
                    return ServiceResult<TriageQueue>.Failed("هیچ بیمار در صف انتظار نیست.");
                }

                nextQueue.Status = TriageQueueStatus.Called;
                nextQueue.CalledTime = DateTime.UtcNow;
                nextQueue.CalledByUserId = _currentUserService.GetCurrentUserId();
                nextQueue.UpdatedAt = DateTime.UtcNow;
                nextQueue.UpdatedByUserId = _currentUserService.GetCurrentUserId();

                await _context.SaveChangesAsync();

                _logger.Information("بیمار {PatientId} فراخوانی شد", nextQueue.PatientId);

                return ServiceResult<TriageQueue>.Successful(nextQueue, "بیمار فراخوانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فراخوانی بیمار بعدی");
                return ServiceResult<TriageQueue>.Failed("خطا در فراخوانی بیمار بعدی: " + ex.Message);
            }
        }

        public async Task<ServiceResult> CompleteAsync(int queueId)
        {
            try
            {
                var queue = await _context.TriageQueues
                    .FirstOrDefaultAsync(q => q.TriageQueueId == queueId && !q.IsDeleted);

                if (queue == null)
                {
                    return ServiceResult.Failed("صف تریاژ یافت نشد.");
                }

                queue.Status = TriageQueueStatus.Completed;
                queue.CompletedTime = DateTime.UtcNow;
                queue.CompletedByUserId = _currentUserService.GetCurrentUserId();
                queue.UpdatedAt = DateTime.UtcNow;
                queue.UpdatedByUserId = _currentUserService.GetCurrentUserId();

                await _context.SaveChangesAsync();

                _logger.Information("صف تریاژ {QueueId} تکمیل شد", queueId);

                return ServiceResult.Successful("صف تریاژ تکمیل شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تکمیل صف تریاژ");
                return ServiceResult.Failed("خطا در تکمیل صف تریاژ: " + ex.Message);
            }
        }

        public async Task<ServiceResult> CloseActiveQueueIfAnyAsync(int assessmentId)
        {
            try
            {
                var activeQueues = await _context.TriageQueues
                    .Where(q => q.TriageAssessmentId == assessmentId && 
                               (q.Status == TriageQueueStatus.Waiting || q.Status == TriageQueueStatus.Called) &&
                               !q.IsDeleted)
                    .ToListAsync();

                if (activeQueues.Any())
                {
                    var userId = _currentUserService.GetCurrentUserId();
                    var now = DateTime.UtcNow;
                    
                    foreach (var queue in activeQueues)
                    {
                        queue.Status = TriageQueueStatus.Completed;
                        queue.CompletedTime = now;
                        queue.CompletedByUserId = userId;
                        queue.UpdatedAt = now;
                        queue.UpdatedByUserId = userId;
                    }

                    await _context.SaveChangesAsync();
                    _logger.Information("صف‌های فعال برای ارزیابی {AssessmentId} بسته شدند", assessmentId);
                }

                return ServiceResult.Successful("صف‌های فعال بسته شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بستن صف‌های فعال");
                return ServiceResult.Failed("خطا در بستن صف‌های فعال: " + ex.Message);
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

        public async Task<ServiceResult> ReorderQueueByPriorityAsync(int? departmentId = null)
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
                var userId = _currentUserService.GetCurrentUserId();
                for (int i = 0; i < orderedQueues.Count; i++)
                {
                    orderedQueues[i].QueuePosition = i + 1; // موقعیت در صف
                    orderedQueues[i].UpdatedAt = DateTime.UtcNow;
                    orderedQueues[i].UpdatedByUserId = userId;
                }

                await _context.SaveChangesAsync();

                return ServiceResult.Successful("صف بر اساس اولویت مرتب شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مرتب‌سازی صف");
                return ServiceResult.Failed("خطا در مرتب‌سازی صف: " + ex.Message);
            }
        }

        public async Task<ServiceResult<TriageQueueStats>> GetQueueStatsAsync(int? departmentId = null)
        {
            try
            {
                var query = _context.TriageQueues.Where(q => !q.IsDeleted);

                if (departmentId.HasValue)
                {
                    query = query.Where(q => q.TargetDepartmentId == departmentId);
                }

                var queues = await query.ToListAsync();
                var today = DateTime.UtcNow.Date;

                var stats = new TriageQueueStats
                {
                    TotalWaiting = queues.Count(q => q.Status == TriageQueueStatus.Waiting),
                    TotalInProgress = queues.Count(q => q.Status == TriageQueueStatus.InProgress),
                    TotalCompleted = queues.Count(q => q.Status == TriageQueueStatus.Completed),
                    CriticalWaiting = queues.Count(q => q.Status == TriageQueueStatus.Waiting && 
                                                      q.TriageAssessment.Level == TriageLevel.ESI1),
                    HighPriorityWaiting = queues.Count(q => q.Status == TriageQueueStatus.Waiting && 
                                                           (q.TriageAssessment.Level == TriageLevel.ESI1 || 
                                                            q.TriageAssessment.Level == TriageLevel.ESI2)),
                    OverdueCount = queues.Count(q => q.Status == TriageQueueStatus.Waiting && 
                                                    q.NextReassessmentDueAt.HasValue && 
                                                    q.NextReassessmentDueAt < DateTime.UtcNow),
                    CompletedToday = queues.Count(q => q.Status == TriageQueueStatus.Completed && 
                                                      q.CompletedTime.HasValue && 
                                                      q.CompletedTime.Value.Date == today),
                    LastUpdated = DateTime.UtcNow
                };

                // محاسبه میانگین زمان انتظار
                var completedQueues = queues.Where(q => q.Status == TriageQueueStatus.Completed && 
                                                       q.CalledTime.HasValue && 
                                                       q.QueueTime != default).ToList();
                
                if (completedQueues.Any())
                {
                    stats.AverageWaitTimeMinutes = completedQueues
                        .Average(q => (q.CalledTime.Value - q.QueueTime).TotalMinutes);
                }

                return ServiceResult<TriageQueueStats>.Successful(stats, "آمار صف دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار صف");
                return ServiceResult<TriageQueueStats>.Failed("خطا در دریافت آمار صف: " + ex.Message);
            }
        }
    

    /// <summary>
    /// دریافت صف فوری
    /// </summary>
    public async Task<ServiceResult<List<TriageQueue>>> GetUrgentQueueAsync()
    {
        try
        {
            var urgentQueues = await _context.TriageQueues
                .Where(q => !q.IsDeleted && 
                           q.Status == TriageQueueStatus.Waiting && 
                           (q.Priority <= 2 || q.RequiresImmediateCare))
                .OrderBy(q => q.Priority)
                .ThenBy(q => q.QueueTime)
                .ToListAsync();

            return ServiceResult<List<TriageQueue>>.Successful(urgentQueues, "صف فوری دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت صف فوری");
            return ServiceResult<List<TriageQueue>>.Failed("خطا در دریافت صف فوری: " + ex.Message);
        }
    }

    /// <summary>
    /// دریافت صف تاخیردار
    /// </summary>
    public async Task<ServiceResult<List<TriageQueue>>> GetOverdueQueueAsync()
    {
        try
        {
            var overdueTime = DateTime.UtcNow.AddHours(-2); // تاخیر بیش از 2 ساعت
            var overdueQueues = await _context.TriageQueues
                .Where(q => !q.IsDeleted && 
                           q.Status == TriageQueueStatus.Waiting && 
                           q.QueueTime < overdueTime)
                .OrderBy(q => q.QueueTime)
                .ToListAsync();

            return ServiceResult<List<TriageQueue>>.Successful(overdueQueues, "صف تاخیردار دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "خطا در دریافت صف تاخیردار");
            return ServiceResult<List<TriageQueue>>.Failed("خطا در دریافت صف تاخیردار: " + ex.Message);
        }
    }
}
}
