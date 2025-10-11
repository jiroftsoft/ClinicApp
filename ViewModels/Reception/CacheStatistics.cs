using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// آمار کش سیستم
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>
        /// تعداد کل کلیدهای کش
        /// </summary>
        public int TotalCacheKeys { get; set; }

        /// <summary>
        /// استفاده کل از حافظه (بایت)
        /// </summary>
        public long TotalMemoryUsage { get; set; }

        /// <summary>
        /// تعداد hit های کش
        /// </summary>
        public int CacheHits { get; set; }

        /// <summary>
        /// تعداد miss های کش
        /// </summary>
        public int CacheMisses { get; set; }

        /// <summary>
        /// نرخ موفقیت کش (درصد)
        /// </summary>
        public double HitRate => CacheHits + CacheMisses > 0 ? (double)CacheHits / (CacheHits + CacheMisses) * 100 : 0;

        /// <summary>
        /// تعداد کلیدهای منقضی شده
        /// </summary>
        public int ExpiredKeys { get; set; }

        /// <summary>
        /// تعداد کلیدهای پاک شده
        /// </summary>
        public int EvictedKeys { get; set; }

        /// <summary>
        /// زمان آخرین به‌روزرسانی
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        /// <summary>
        /// زمان شروع آمارگیری
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// زمان پایان آمارگیری
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// مدت زمان آمارگیری
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;
    }
}
