using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Triage;
using ClinicApp.ViewModels.Triage;
using Serilog;

namespace ClinicApp.Controllers.Triage
{
    /// <summary>
    /// کنترلر داشبورد تریاژ - SRP محور
    /// مدیریت داشبورد اصلی و آمار Real-time
    /// </summary>
    [Authorize]
    public class TriageDashboardController : BaseController
    {
        private readonly ITriageService _triageService;
        private readonly ITriageQueueService _queueService;
        private readonly ILogger _logger;

        public TriageDashboardController(
            ITriageService triageService,
            ITriageQueueService queueService,
            ILogger logger) : base(logger)
        {
            _triageService = triageService;
            _queueService = queueService;
            _logger = logger;
        }

        #region داشبورد اصلی (Main Dashboard)

        /// <summary>
        /// داشبورد اصلی تریاژ
        /// </summary>
        public async Task<ActionResult> Index(int? departmentId = null)
        {
            try
            {
                var model = new TriageDashboardViewModel
                {
                    SelectedDepartmentId = departmentId,
                    SelectedDate = DateTime.Today
                };

                // دریافت آمار روزانه
                var dailyStats = await GetDailyStats(DateTime.Today, departmentId);
                model.DailyStats = dailyStats;

                // دریافت آمار صف
                var queueStats = await _queueService.GetQueueStatsAsync(departmentId);
                if (queueStats.Success)
                {
                    model.QueueStats = new ViewModels.Triage.TriageQueueStats
                    {
                        WaitingCount = queueStats.Data.TotalWaiting,
                        CalledCount = queueStats.Data.TotalInProgress,
                        CompletedCount = queueStats.Data.TotalCompleted,
                        AverageWaitTimeMinutes = queueStats.Data.AverageWaitTimeMinutes,
                        LastUpdated = queueStats.Data.LastUpdated
                    };
                }

                // دریافت صف فوری
                var urgentQueue = await _queueService.GetUrgentQueueAsync();
                if (urgentQueue.Success)
                {
                    // TODO: Convert to DTO
                    // model.UrgentQueue = urgentQueue.Data;
                }

                // دریافت صف Overdue
                var overdueQueue = await _queueService.GetOverdueQueueAsync();
                if (overdueQueue.Success)
                {
                    // TODO: Convert to DTO
                    // model.OverdueQueue = overdueQueue.Data;
                }

                // دریافت ارزیابی‌های اخیر
                var recentAssessments = await _triageService.GetRecentAssessmentsAsync(24);
                if (recentAssessments.Success)
                {
                    // TODO: Convert to DTO
                    // model.RecentAssessments = recentAssessments.Data;
                }

                // دریافت هشدارهای فعال
                var activeAlerts = await _triageService.GetActiveAlertsAsync();
                if (activeAlerts.Success)
                {
                    // TODO: Convert to DTO
                    // model.ActiveAlerts = activeAlerts.Data;
                }

                // بررسی وضعیت سیستم
                model.IsSystemHealthy = CheckSystemHealth(model);
                model.SystemStatusMessage = GetSystemStatusMessage(model);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داشبورد تریاژ");
                TempData["Error"] = "خطا در دریافت داشبورد";
                return View(new TriageDashboardViewModel());
            }
        }

        /// <summary>
        /// آمار Real-time
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetRealTimeStats(int? departmentId = null)
        {
            try
            {
                var stats = new
                {
                    TotalAssessments = await GetTotalAssessmentsCount(departmentId),
                    CriticalPatients = await GetCriticalPatientsCount(departmentId),
                    AverageWaitTime = await GetAverageWaitTime(departmentId),
                    SystemHealth = CheckSystemHealth(null)
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار Real-time");
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }

        /// <summary>
        /// نمودار روند تریاژ
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetTrendChart(DateTime startDate, DateTime endDate, int? departmentId = null)
        {
            try
            {
                var chartData = await GetTrendData(startDate, endDate, departmentId);
                return Json(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نمودار روند");
                return Json(new { success = false, message = "خطا در دریافت نمودار" });
            }
        }

        /// <summary>
        /// هشدارهای فعال
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetActiveAlerts(int? departmentId = null)
        {
            try
            {
                var alerts = await _triageService.GetActiveAlertsAsync();
                return Json(new { success = alerts.Success, data = alerts.Data, message = alerts.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت هشدارهای فعال");
                return Json(new { success = false, message = "خطا در دریافت هشدارها" });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت آمار روزانه
        /// </summary>
        private async Task<ViewModels.Triage.TriageDailyStats> GetDailyStats(DateTime date, int? departmentId)
        {
            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                var assessments = await _triageService.GetAssessmentsByDateRangeAsync(startDate, endDate);
                if (!assessments.Success)
                {
                    return new ViewModels.Triage.TriageDailyStats { Date = date };
                }

                var stats = new ViewModels.Triage.TriageDailyStats
                {
                    Date = date,
                    TotalAssessments = assessments.Data.Count,
                    CriticalLevel = assessments.Data.Count(a => a.Level == TriageLevel.ESI1),
                    HighLevel = assessments.Data.Count(a => a.Level == TriageLevel.ESI2),
                    MediumLevel = assessments.Data.Count(a => a.Level == TriageLevel.ESI3),
                    LowLevel = assessments.Data.Count(a => a.Level == TriageLevel.ESI4),
                    VeryLowLevel = assessments.Data.Count(a => a.Level == TriageLevel.ESI5),
                    CompletedAssessments = assessments.Data.Count(a => a.Status == TriageStatus.Completed),
                    PendingAssessments = assessments.Data.Count(a => a.Status == TriageStatus.Pending)
                };

                // محاسبه میانگین زمان انتظار
                var completedAssessments = assessments.Data.Where(a => a.Status == TriageStatus.Completed && a.TriageEndAt.HasValue).ToList();
                if (completedAssessments.Any())
                {
                    stats.AverageWaitTimeMinutes = (int)completedAssessments.Average(a => (a.TriageEndAt.Value - a.TriageStartAt).TotalMinutes);
                    stats.AverageAssessmentTimeMinutes = (int)completedAssessments.Average(a => (a.TriageEndAt.Value - a.TriageStartAt).TotalMinutes);
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه آمار روزانه");
                return new TriageDailyStats { Date = date };
            }
        }

        /// <summary>
        /// بررسی سلامت سیستم
        /// </summary>
        private bool CheckSystemHealth(TriageDashboardViewModel model)
        {
            try
            {
                // بررسی‌های مختلف سلامت سیستم
                var isHealthy = true;

                if (model != null)
                {
                    // بررسی تعداد بیماران بحرانی
                    if (model.CriticalPatients > 10)
                    {
                        isHealthy = false;
                    }

                    // بررسی زمان انتظار
                    if (model.DailyStats.AverageWaitTimeMinutes > 120)
                    {
                        isHealthy = false;
                    }

                    // بررسی هشدارهای فعال
                    if (model.HasActiveAlerts)
                    {
                        isHealthy = false;
                    }
                }

                return isHealthy;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی سلامت سیستم");
                return false;
            }
        }

        /// <summary>
        /// دریافت پیغام وضعیت سیستم
        /// </summary>
        private string GetSystemStatusMessage(TriageDashboardViewModel model)
        {
            if (model.IsSystemHealthy)
            {
                return "سیستم در وضعیت عادی";
            }
            else
            {
                var warnings = new List<string>();
                
                if (model.CriticalPatients > 10)
                {
                    warnings.Add($"تعداد بیماران بحرانی بالا: {model.CriticalPatients}");
                }

                if (model.DailyStats.AverageWaitTimeMinutes > 120)
                {
                    warnings.Add($"زمان انتظار طولانی: {model.DailyStats.AverageWaitTimeMinutes} دقیقه");
                }

                if (model.HasActiveAlerts)
                {
                    warnings.Add($"هشدارهای فعال: {model.ActiveAlerts.Count}");
                }

                return "هشدار: " + string.Join("، ", warnings);
            }
        }

        /// <summary>
        /// دریافت تعداد کل ارزیابی‌ها
        /// </summary>
        private async Task<int> GetTotalAssessmentsCount(int? departmentId)
        {
            try
            {
                var result = await _triageService.GetAssessmentsCountAsync();
                return result.Success ? result.Data : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// دریافت تعداد بیماران بحرانی
        /// </summary>
        private async Task<int> GetCriticalPatientsCount(int? departmentId)
        {
            try
            {
                var result = await _triageService.GetCriticalPatientsCountAsync();
                return result.Success ? result.Data : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// دریافت میانگین زمان انتظار
        /// </summary>
        private async Task<double> GetAverageWaitTime(int? departmentId)
        {
            try
            {
                var result = await _triageService.GetAverageWaitTimeAsync();
                return result.Success ? result.Data : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// دریافت داده‌های روند
        /// </summary>
        private async Task<object> GetTrendData(DateTime startDate, DateTime endDate, int? departmentId)
        {
            try
            {
                // TODO: پیاده‌سازی دریافت داده‌های روند از دیتابیس
                return new
                {
                    Labels = new string[0],
                    Data = new int[0]
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های روند");
                return new { Labels = new string[0], Data = new int[0] };
            }
        }

        #endregion
    }
}
