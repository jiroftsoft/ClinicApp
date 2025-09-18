using System;
using System.Collections.Generic;

namespace ClinicApp.Models.Insurance
{
    /// <summary>
    /// وضعیت سلامت سیستم بیمه تکمیلی
    /// </summary>
    public class SystemHealthStatus
    {
        /// <summary>
        /// وضعیت کلی سیستم
        /// </summary>
        public HealthStatus OverallStatus { get; set; } = HealthStatus.Healthy;

        /// <summary>
        /// زمان بررسی سلامت
        /// </summary>
        public DateTime CheckTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// مدت زمان فعال بودن سیستم (به ساعت)
        /// </summary>
        public double SystemUptimeHours { get; set; }

        /// <summary>
        /// درصد استفاده از CPU
        /// </summary>
        public double CpuUsagePercent { get; set; }

        /// <summary>
        /// درصد استفاده از حافظه
        /// </summary>
        public double MemoryUsagePercent { get; set; }

        /// <summary>
        /// درصد استفاده از دیسک
        /// </summary>
        public double DiskUsagePercent { get; set; }

        /// <summary>
        /// وضعیت اتصال به پایگاه داده
        /// </summary>
        public HealthStatus DatabaseStatus { get; set; } = HealthStatus.Healthy;

        /// <summary>
        /// زمان پاسخ پایگاه داده (به میلی‌ثانیه)
        /// </summary>
        public double DatabaseResponseTime { get; set; }

        /// <summary>
        /// وضعیت Cache
        /// </summary>
        public HealthStatus CacheStatus { get; set; } = HealthStatus.Healthy;

        /// <summary>
        /// درصد Cache Hit
        /// </summary>
        public double CacheHitRate { get; set; }

        /// <summary>
        /// تعداد آیتم‌های Cache
        /// </summary>
        public int CacheItemCount { get; set; }

        /// <summary>
        /// وضعیت سرویس‌های خارجی
        /// </summary>
        public HealthStatus ExternalServicesStatus { get; set; } = HealthStatus.Healthy;

        /// <summary>
        /// وضعیت شبکه
        /// </summary>
        public HealthStatus NetworkStatus { get; set; } = HealthStatus.Healthy;

        /// <summary>
        /// وضعیت احراز هویت
        /// </summary>
        public HealthStatus AuthenticationStatus { get; set; } = HealthStatus.Healthy;

        /// <summary>
        /// وضعیت مجوزها
        /// </summary>
        public HealthStatus AuthorizationStatus { get; set; } = HealthStatus.Healthy;

        /// <summary>
        /// تعداد خطاهای فعال
        /// </summary>
        public int ActiveErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای بحرانی
        /// </summary>
        public int CriticalErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای هشدار
        /// </summary>
        public int WarningErrors { get; set; }

        /// <summary>
        /// تعداد کاربران فعال
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// تعداد درخواست‌های فعال
        /// </summary>
        public int ActiveRequests { get; set; }

        /// <summary>
        /// میانگین زمان پاسخ (به میلی‌ثانیه)
        /// </summary>
        public double AverageResponseTime { get; set; }

        /// <summary>
        /// حداکثر زمان پاسخ (به میلی‌ثانیه)
        /// </summary>
        public double MaxResponseTime { get; set; }

        /// <summary>
        /// تعداد درخواست‌ها در دقیقه
        /// </summary>
        public double RequestsPerMinute { get; set; }

        /// <summary>
        /// تعداد درخواست‌ها در ساعت
        /// </summary>
        public double RequestsPerHour { get; set; }

        /// <summary>
        /// درصد موفقیت درخواست‌ها
        /// </summary>
        public double RequestSuccessRate { get; set; }

        /// <summary>
        /// وضعیت سرویس‌های مختلف
        /// </summary>
        public List<ServiceHealthStatus> ServiceStatuses { get; set; } = new List<ServiceHealthStatus>();

        /// <summary>
        /// هشدارهای فعال
        /// </summary>
        public List<HealthAlert> ActiveAlerts { get; set; } = new List<HealthAlert>();

        /// <summary>
        /// توصیه‌های بهبود
        /// </summary>
        public List<HealthRecommendation> Recommendations { get; set; } = new List<HealthRecommendation>();

        /// <summary>
        /// زمان آخرین به‌روزرسانی
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// دریافت خلاصه وضعیت سلامت
        /// </summary>
        /// <returns>خلاصه وضعیت سلامت</returns>
        public string GetSummary()
        {
            return $"System Health - Status: {OverallStatus}, " +
                   $"CPU: {CpuUsagePercent:F1}%, Memory: {MemoryUsagePercent:F1}%, " +
                   $"Database: {DatabaseStatus}, Cache: {CacheStatus}, " +
                   $"Active Errors: {ActiveErrors}, Active Users: {ActiveUsers}, " +
                   $"Response Time: {AverageResponseTime:F1}ms, Success Rate: {RequestSuccessRate:F1}%";
        }

        /// <summary>
        /// بررسی آیا سیستم سالم است
        /// </summary>
        /// <returns>آیا سیستم سالم است</returns>
        public bool IsHealthy()
        {
            return OverallStatus == HealthStatus.Healthy;
        }

        /// <summary>
        /// بررسی آیا سیستم نیاز به توجه دارد
        /// </summary>
        /// <returns>آیا سیستم نیاز به توجه دارد</returns>
        public bool NeedsAttention()
        {
            return OverallStatus == HealthStatus.Warning || OverallStatus == HealthStatus.Critical;
        }

        /// <summary>
        /// بررسی آیا سیستم بحرانی است
        /// </summary>
        /// <returns>آیا سیستم بحرانی است</returns>
        public bool IsCritical()
        {
            return OverallStatus == HealthStatus.Critical;
        }

        /// <summary>
        /// محاسبه امتیاز سلامت (0-100)
        /// </summary>
        /// <returns>امتیاز سلامت</returns>
        public int GetHealthScore()
        {
            int score = 100;

            // کاهش امتیاز بر اساس خطاها
            score -= CriticalErrors * 20;
            score -= WarningErrors * 5;
            score -= ActiveErrors * 2;

            // کاهش امتیاز بر اساس استفاده از منابع
            if (CpuUsagePercent > 80) score -= 10;
            if (MemoryUsagePercent > 80) score -= 10;
            if (DiskUsagePercent > 90) score -= 15;

            // کاهش امتیاز بر اساس زمان پاسخ
            if (AverageResponseTime > 5000) score -= 15;
            if (DatabaseResponseTime > 1000) score -= 10;

            // کاهش امتیاز بر اساس نرخ موفقیت
            if (RequestSuccessRate < 95) score -= 10;
            if (RequestSuccessRate < 90) score -= 15;

            // کاهش امتیاز بر اساس وضعیت سرویس‌ها
            foreach (var service in ServiceStatuses)
            {
                if (service.Status == HealthStatus.Critical) score -= 20;
                else if (service.Status == HealthStatus.Warning) score -= 5;
            }

            return Math.Max(0, Math.Min(100, score));
        }
    }

    /// <summary>
    /// وضعیت سلامت سرویس
    /// </summary>
    public class ServiceHealthStatus
    {
        /// <summary>
        /// نام سرویس
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// وضعیت سرویس
        /// </summary>
        public HealthStatus Status { get; set; }

        /// <summary>
        /// زمان پاسخ سرویس (به میلی‌ثانیه)
        /// </summary>
        public double ResponseTime { get; set; }

        /// <summary>
        /// تعداد خطاهای سرویس
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// آخرین زمان بررسی
        /// </summary>
        public DateTime LastChecked { get; set; }

        /// <summary>
        /// پیام وضعیت
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// جزئیات اضافی
        /// </summary>
        public string Details { get; set; }
    }

    /// <summary>
    /// هشدار سلامت
    /// </summary>
    public class HealthAlert
    {
        /// <summary>
        /// شناسه هشدار
        /// </summary>
        public string AlertId { get; set; }

        /// <summary>
        /// نوع هشدار
        /// </summary>
        public string AlertType { get; set; }

        /// <summary>
        /// سطح هشدار
        /// </summary>
        public HealthStatus Severity { get; set; }

        /// <summary>
        /// عنوان هشدار
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// پیام هشدار
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// زمان ایجاد هشدار
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// آیا هشدار فعال است
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// زمان حل هشدار
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// کاربر حل کننده هشدار
        /// </summary>
        public string ResolvedBy { get; set; }
    }

    /// <summary>
    /// توصیه بهبود سلامت
    /// </summary>
    public class HealthRecommendation
    {
        /// <summary>
        /// شناسه توصیه
        /// </summary>
        public string RecommendationId { get; set; }

        /// <summary>
        /// نوع توصیه
        /// </summary>
        public string RecommendationType { get; set; }

        /// <summary>
        /// اولویت توصیه
        /// </summary>
        public HealthStatus Priority { get; set; }

        /// <summary>
        /// عنوان توصیه
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// توضیحات توصیه
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// اقدامات پیشنهادی
        /// </summary>
        public string SuggestedActions { get; set; }

        /// <summary>
        /// زمان ایجاد توصیه
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// آیا توصیه پیاده‌سازی شده است
        /// </summary>
        public bool IsImplemented { get; set; } = false;

        /// <summary>
        /// زمان پیاده‌سازی توصیه
        /// </summary>
        public DateTime? ImplementedAt { get; set; }

        /// <summary>
        /// کاربر پیاده‌سازی کننده
        /// </summary>
        public string ImplementedBy { get; set; }
    }

    /// <summary>
    /// وضعیت سلامت
    /// </summary>
    public enum HealthStatus
    {
        /// <summary>
        /// سالم
        /// </summary>
        Healthy = 1,

        /// <summary>
        /// هشدار
        /// </summary>
        Warning = 2,

        /// <summary>
        /// بحرانی
        /// </summary>
        Critical = 3,

        /// <summary>
        /// نامشخص
        /// </summary>
        Unknown = 4
    }
}
