using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Insurance;
using ClinicApp.ViewModels.Insurance.Supplementary;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس Monitoring و Analytics برای بیمه تکمیلی
    /// طراحی شده برای نظارت و تحلیل عملکرد سیستم‌های پزشکی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نظارت بر عملکرد محاسبات بیمه تکمیلی
    /// 2. تحلیل آمار استفاده و کارایی
    /// 3. تشخیص مشکلات و خطاها
    /// 4. گزارش‌گیری جامع
    /// 5. هشدارهای خودکار
    /// </summary>
    public class SupplementaryInsuranceMonitoringService : ISupplementaryInsuranceMonitoringService
    {
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        // آمار عملکرد
        private static readonly Dictionary<string, PerformanceMetrics> _performanceMetrics = new Dictionary<string, PerformanceMetrics>();
        private static readonly List<CalculationEvent> _calculationEvents = new List<CalculationEvent>();
        private static readonly List<ErrorEvent> _errorEvents = new List<ErrorEvent>();

        public SupplementaryInsuranceMonitoringService(
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _log = logger.ForContext<SupplementaryInsuranceMonitoringService>();
        }

        #region ISupplementaryInsuranceMonitoringService Implementation

        /// <summary>
        /// ثبت رویداد محاسبه
        /// </summary>
        public void LogCalculationEvent(CalculationEvent calculationEvent)
        {
            try
            {
                calculationEvent.CalculationDate = DateTime.UtcNow;
                calculationEvent.UserId = _currentUserService.UserId;
                calculationEvent.UserName = _currentUserService.UserName;

                lock (_calculationEvents)
                {
                    _calculationEvents.Add(calculationEvent);
                    
                    // نگه داشتن فقط 1000 رویداد اخیر
                    if (_calculationEvents.Count > 1000)
                    {
                        _calculationEvents.RemoveAt(0);
                    }
                }

                // ثبت در لاگ
                _log.Information("🏥 MEDICAL: رویداد محاسبه ثبت شد - PatientId: {PatientId}, ServiceId: {ServiceId}, Duration: {Duration}ms, Success: {Success}. User: {UserName} (Id: {UserId})",
                    calculationEvent.PatientId, calculationEvent.ServiceId, calculationEvent.Duration, calculationEvent.Success, 
                    _currentUserService.UserName, _currentUserService.UserId);

                // به‌روزرسانی آمار عملکرد
                UpdatePerformanceMetrics(calculationEvent);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ثبت رویداد محاسبه");
            }
        }

        /// <summary>
        /// ثبت رویداد خطا
        /// </summary>
        public void LogErrorEvent(ErrorEvent errorEvent)
        {
            try
            {
                errorEvent.ErrorTime = DateTime.UtcNow;
                errorEvent.UserId = _currentUserService.UserId;
                errorEvent.UserName = _currentUserService.UserName;

                lock (_errorEvents)
                {
                    _errorEvents.Add(errorEvent);
                    
                    // نگه داشتن فقط 500 رویداد خطای اخیر
                    if (_errorEvents.Count > 500)
                    {
                        _errorEvents.RemoveAt(0);
                    }
                }

                // ثبت در لاگ
                _log.Error("🏥 MEDICAL: رویداد خطا ثبت شد - ErrorType: {ErrorType}, Message: {Message}, PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    errorEvent.ErrorType, errorEvent.Message, errorEvent.PatientId, errorEvent.ServiceId, 
                    _currentUserService.UserName, _currentUserService.UserId);

                // بررسی نیاز به هشدار
                CheckForAlerts(errorEvent);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ثبت رویداد خطا");
            }
        }

        /// <summary>
        /// دریافت آمار عملکرد
        /// </summary>
        public Models.Insurance.PerformanceReport GetPerformanceReport(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var from = fromDate.HasValue ? fromDate.Value : DateTime.UtcNow.AddDays(-30);
                var to = toDate.HasValue ? toDate.Value : DateTime.UtcNow;

                var report = new Models.Insurance.PerformanceReport
                {
                    FromDate = from,
                    ToDate = to,
                    GeneratedAt = DateTime.UtcNow
                };

                lock (_calculationEvents)
                {
                    var eventsInRange = _calculationEvents.Where(e => e.CalculationDate >= from && e.CalculationDate <= to).ToList();
                    
                    report.TotalCalculations = eventsInRange.Count;
                    report.SuccessfulCalculations = eventsInRange.Count(e => e.Success);
                    report.FailedCalculations = eventsInRange.Count(e => !e.Success);
                    report.SuccessRate = report.TotalCalculations > 0 ? (double)report.SuccessfulCalculations / report.TotalCalculations * 100 : 0;
                    
                    if (eventsInRange.Any())
                    {
                        report.AverageCalculationTime = eventsInRange.Average(e => e.Duration);
                        report.MinCalculationTime = eventsInRange.Min(e => e.Duration);
                        report.MaxCalculationTime = eventsInRange.Max(e => e.Duration);
                    }
                }

                lock (_errorEvents)
                {
                    var errorsInRange = _errorEvents.Where(e => e.ErrorTime >= from && e.ErrorTime <= to).ToList();
                    report.TotalErrors = errorsInRange.Count;
                    report.ErrorRate = report.TotalCalculations > 0 ? (double)report.TotalErrors / report.TotalCalculations * 100 : 0;
                    
                    report.ErrorBreakdown = errorsInRange
                        .GroupBy(e => e.ErrorType)
                        .ToDictionary(g => g.Key, g => g.Count());
                }

                // محاسبه آمار Cache
                // report.CacheStatistics = GetCacheStatistics();

                _log.Information("🏥 MEDICAL: گزارش عملکرد تولید شد - TotalCalculations: {TotalCalculations}, SuccessRate: {SuccessRate}%, ErrorRate: {ErrorRate}%. User: {UserName} (Id: {UserId})",
                    report.TotalCalculations, report.SuccessRate, report.ErrorRate, _currentUserService.UserName, _currentUserService.UserId);

                return report;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در تولید گزارش عملکرد");
                return new Models.Insurance.PerformanceReport();
            }
        }

        /// <summary>
        /// دریافت آمار استفاده
        /// </summary>
        public UsageStatistics GetUsageStatistics(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var from = fromDate.HasValue ? fromDate.Value : DateTime.UtcNow.AddDays(-30);
                var to = toDate.HasValue ? toDate.Value : DateTime.UtcNow;

                var statistics = new UsageStatistics
                {
                    FromDate = from,
                    ToDate = to,
                    GeneratedAt = DateTime.UtcNow
                };

                lock (_calculationEvents)
                {
                    var eventsInRange = _calculationEvents.Where(e => e.CalculationDate >= from && e.CalculationDate <= to).ToList();
                    
                    // آمار روزانه
                    statistics.DailyUsages = eventsInRange
                        .GroupBy(e => e.CalculationDate.Date)
                        .Select(g => new DailyUsage
                        {
                            Date = g.Key,
                            TotalRequests = g.Count(),
                            SuccessfulRequests = g.Count(e => e.Success),
                            FailedRequests = g.Count(e => !e.Success),
                            SuccessRate = g.Count() > 0 ? (double)g.Count(e => e.Success) / g.Count() * 100 : 0,
                            UniqueUsers = g.Select(e => e.UserId).Distinct().Count(),
                            UniquePatients = g.Select(e => e.PatientId).Distinct().Count(),
                            AverageResponseTime = g.Average(e => e.Duration),
                            CacheHits = 0,
                            CacheMisses = 0,
                            CacheHitRate = 0
                        })
                        .ToList();
                    
                    // آمار ساعتی
                    statistics.HourlyUsages = eventsInRange
                        .GroupBy(e => e.CalculationDate.Hour)
                        .Select(g => new HourlyUsage
                        {
                            Hour = g.Key,
                            TotalRequests = g.Count(),
                            SuccessfulRequests = g.Count(e => e.Success),
                            FailedRequests = g.Count(e => !e.Success),
                            SuccessRate = g.Count() > 0 ? (double)g.Count(e => e.Success) / g.Count() * 100 : 0,
                            UniqueUsers = g.Select(e => e.UserId).Distinct().Count(),
                            AverageResponseTime = g.Average(e => e.Duration),
                            CacheHits = 0,
                            CacheMisses = 0,
                            CacheHitRate = 0
                        })
                        .ToList();
                    
                    // آمار کاربران
                    statistics.UserUsages = eventsInRange
                        .GroupBy(e => e.UserId)
                        .Select(g => new UserUsage
                        {
                            UserId = g.Key,
                            UserName = g.First().UserName,
                            TotalRequests = g.Count(),
                            SuccessfulRequests = g.Count(e => e.Success),
                            FailedRequests = g.Count(e => !e.Success),
                            SuccessRate = g.Count() > 0 ? (double)g.Count(e => e.Success) / g.Count() * 100 : 0,
                            AverageResponseTime = g.Average(e => e.Duration),
                            UniquePatients = g.Select(e => e.PatientId).Distinct().Count(),
                            UniqueServices = g.Select(e => e.ServiceId).Distinct().Count(),
                            FirstRequest = g.Min(e => e.CalculationDate),
                            LastRequest = g.Max(e => e.CalculationDate)
                        })
                        .ToList();
                    
                    // آمار خدمات
                    statistics.RequestTypeUsages = eventsInRange
                        .GroupBy(e => e.ServiceId)
                        .Select(g => new RequestTypeUsage
                        {
                            RequestType = $"Service_{g.Key}",
                            TotalRequests = g.Count(),
                            SuccessfulRequests = g.Count(e => e.Success),
                            FailedRequests = g.Count(e => !e.Success),
                            SuccessRate = g.Count() > 0 ? (double)g.Count(e => e.Success) / g.Count() * 100 : 0,
                            AverageResponseTime = g.Average(e => e.Duration),
                            UniqueUsers = g.Select(e => e.UserId).Distinct().Count()
                        })
                        .ToList();
                }

                _log.Information("🏥 MEDICAL: آمار استفاده تولید شد - TotalEvents: {TotalEvents}, UniqueUsers: {UniqueUsers}, UniqueServices: {UniqueServices}. User: {UserName} (Id: {UserId})",
                    statistics.DailyUsages.Sum(d => d.TotalRequests), statistics.UserUsages.Count, statistics.RequestTypeUsages.Count, 
                    _currentUserService.UserName, _currentUserService.UserId);

                return statistics;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در تولید آمار استفاده");
                return new UsageStatistics();
            }
        }

        /// <summary>
        /// بررسی سلامت سیستم
        /// </summary>
        public SystemHealthStatus GetSystemHealthStatus()
        {
            try
            {
                var health = new SystemHealthStatus
                {
                    CheckTime = DateTime.UtcNow,
                    OverallStatus = HealthStatus.Healthy
                };

                // بررسی آمار خطاهای اخیر
                var recentErrors = _errorEvents.Where(e => e.ErrorTime >= DateTime.UtcNow.AddHours(-1)).ToList();
                if (recentErrors.Count > 10)
                {
                    health.OverallStatus = HealthStatus.Critical;
                    health.ActiveAlerts.Add(new HealthAlert
                    {
                        AlertId = Guid.NewGuid().ToString(),
                        AlertType = "HighErrorRate",
                        Severity = HealthStatus.Critical,
                        Title = "تعداد خطاهای بالا",
                        Message = "تعداد خطاهای بالا در ساعت اخیر",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else if (recentErrors.Count > 5)
                {
                    health.OverallStatus = HealthStatus.Warning;
                    health.ActiveAlerts.Add(new HealthAlert
                    {
                        AlertId = Guid.NewGuid().ToString(),
                        AlertType = "MediumErrorRate",
                        Severity = HealthStatus.Warning,
                        Title = "تعداد خطاهای متوسط",
                        Message = "تعداد خطاهای متوسط در ساعت اخیر",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // بررسی زمان محاسبه
                var recentCalculations = _calculationEvents.Where(e => e.CalculationDate >= DateTime.UtcNow.AddHours(-1)).ToList();
                if (recentCalculations.Any())
                {
                    var avgTime = recentCalculations.Average(e => e.Duration);
                    if (avgTime > 5000) // بیش از 5 ثانیه
                    {
                        health.OverallStatus = HealthStatus.Warning;
                        health.ActiveAlerts.Add(new HealthAlert
                        {
                            AlertId = Guid.NewGuid().ToString(),
                            AlertType = "HighCalculationTime",
                            Severity = HealthStatus.Warning,
                            Title = "زمان محاسبه بالا",
                            Message = "زمان محاسبه بالا",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // بررسی نرخ موفقیت
                if (recentCalculations.Any())
                {
                    var successRate = (double)recentCalculations.Count(e => e.Success) / recentCalculations.Count * 100;
                    if (successRate < 90)
                    {
                        health.OverallStatus = HealthStatus.Critical;
                        health.ActiveAlerts.Add(new HealthAlert
                        {
                            AlertId = Guid.NewGuid().ToString(),
                            AlertType = "LowSuccessRate",
                            Severity = HealthStatus.Critical,
                            Title = "نرخ موفقیت پایین",
                            Message = $"نرخ موفقیت پایین: {successRate:F1}%",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    else if (successRate < 95)
                    {
                        health.OverallStatus = HealthStatus.Warning;
                        health.ActiveAlerts.Add(new HealthAlert
                        {
                            AlertId = Guid.NewGuid().ToString(),
                            AlertType = "MediumSuccessRate",
                            Severity = HealthStatus.Warning,
                            Title = "نرخ موفقیت متوسط",
                            Message = $"نرخ موفقیت متوسط: {successRate:F1}%",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                _log.Information("🏥 MEDICAL: وضعیت سلامت سیستم بررسی شد - Status: {Status}, Alerts: {AlertsCount}. User: {UserName} (Id: {UserId})",
                    health.OverallStatus, health.ActiveAlerts.Count, _currentUserService.UserName, _currentUserService.UserId);

                return health;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وضعیت سلامت سیستم");
                return new SystemHealthStatus { OverallStatus = HealthStatus.Unknown };
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// به‌روزرسانی آمار عملکرد
        /// </summary>
        private void UpdatePerformanceMetrics(CalculationEvent calculationEvent)
        {
            try
            {
                var key = $"{calculationEvent.PatientId}_{calculationEvent.ServiceId}";
                
                if (!_performanceMetrics.ContainsKey(key))
                {
                    _performanceMetrics[key] = new PerformanceMetrics
                    {
                        PatientId = calculationEvent.PatientId,
                        ServiceId = calculationEvent.ServiceId
                    };
                }

                var metrics = _performanceMetrics[key];
                metrics.TotalCalculations++;
                metrics.TotalDuration += calculationEvent.Duration;
                metrics.AverageDuration = metrics.TotalDuration / metrics.TotalCalculations;
                
                if (calculationEvent.Success)
                {
                    metrics.SuccessfulCalculations++;
                }
                else
                {
                    metrics.FailedCalculations++;
                }

                metrics.SuccessRate = (double)metrics.SuccessfulCalculations / metrics.TotalCalculations * 100;
                metrics.LastCalculationTime = calculationEvent.CalculationDate;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در به‌روزرسانی آمار عملکرد");
            }
        }

        /// <summary>
        /// بررسی نیاز به هشدار
        /// </summary>
        private void CheckForAlerts(ErrorEvent errorEvent)
        {
            try
            {
                // بررسی خطاهای مکرر
                var recentErrors = _errorEvents.Where(e => 
                    e.ErrorTime >= DateTime.UtcNow.AddMinutes(-10) && 
                    e.ErrorType == errorEvent.ErrorType).Count();

                if (recentErrors > 5)
                {
                    _log.Warning("🏥 MEDICAL: هشدار - خطای مکرر {ErrorType} در 10 دقیقه اخیر: {Count} بار. User: {UserName} (Id: {UserId})",
                        errorEvent.ErrorType, recentErrors, _currentUserService.UserName, _currentUserService.UserId);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی هشدارها");
            }
        }

        /// <summary>
        /// دریافت آمار Cache
        /// </summary>
        private Dictionary<string, object> GetCacheStatistics()
        {
            try
            {
                // این متد می‌تواند از SupplementaryInsuranceCacheService استفاده کند
                return new Dictionary<string, object>
                {
                    ["cache_hit_rate"] = 0.85, // مثال
                    ["cache_size"] = _performanceMetrics.Count,
                    ["cache_evictions"] = 0
                };
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت آمار Cache");
                return new Dictionary<string, object>();
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// آمار عملکرد
    /// </summary>
    public class PerformanceMetrics
    {
        public int PatientId { get; set; }
        public int ServiceId { get; set; }
        public int TotalCalculations { get; set; }
        public int SuccessfulCalculations { get; set; }
        public int FailedCalculations { get; set; }
        public double SuccessRate { get; set; }
        public long TotalDuration { get; set; }
        public double AverageDuration { get; set; }
        public DateTime LastCalculationTime { get; set; }
    }

    #endregion
}
