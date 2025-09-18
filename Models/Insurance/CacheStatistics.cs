using System;

namespace ClinicApp.Models.Insurance
{
    /// <summary>
    /// آمار Cache برای بیمه تکمیلی
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>
        /// تعداد کل آیتم‌های Cache شده
        /// </summary>
        public int TotalCachedItems { get; set; }

        /// <summary>
        /// تعداد Cache Hit ها
        /// </summary>
        public int CacheHits { get; set; }

        /// <summary>
        /// تعداد Cache Miss ها
        /// </summary>
        public int CacheMisses { get; set; }

        /// <summary>
        /// درصد Cache Hit
        /// </summary>
        public double CacheHitRate => TotalRequests > 0 ? (double)CacheHits / TotalRequests * 100 : 0;

        /// <summary>
        /// تعداد کل درخواست‌ها
        /// </summary>
        public int TotalRequests => CacheHits + CacheMisses;

        /// <summary>
        /// آخرین زمان پاک کردن Cache
        /// </summary>
        public DateTime? LastCacheClear { get; set; }

        /// <summary>
        /// زمان ایجاد Cache
        /// </summary>
        public DateTime CacheCreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// مدت زمان فعال بودن Cache (به دقیقه)
        /// </summary>
        public double CacheUptimeMinutes => (DateTime.UtcNow - CacheCreatedAt).TotalMinutes;

        /// <summary>
        /// میانگین زمان پاسخ Cache (به میلی‌ثانیه)
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// حداکثر زمان پاسخ Cache (به میلی‌ثانیه)
        /// </summary>
        public double MaxResponseTimeMs { get; set; }

        /// <summary>
        /// حداقل زمان پاسخ Cache (به میلی‌ثانیه)
        /// </summary>
        public double MinResponseTimeMs { get; set; }

        /// <summary>
        /// تعداد خطاهای Cache
        /// </summary>
        public int CacheErrors { get; set; }

        /// <summary>
        /// درصد خطاهای Cache
        /// </summary>
        public double CacheErrorRate => TotalRequests > 0 ? (double)CacheErrors / TotalRequests * 100 : 0;

        /// <summary>
        /// حجم Cache (به بایت)
        /// </summary>
        public long CacheSizeBytes { get; set; }

        /// <summary>
        /// حجم Cache (به مگابایت)
        /// </summary>
        public double CacheSizeMB => CacheSizeBytes / (1024.0 * 1024.0);

        /// <summary>
        /// تعداد آیتم‌های منقضی شده
        /// </summary>
        public int ExpiredItems { get; set; }

        /// <summary>
        /// تعداد آیتم‌های پاک شده
        /// </summary>
        public int EvictedItems { get; set; }

        /// <summary>
        /// آخرین زمان به‌روزرسانی آمار
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// به‌روزرسانی آمار Cache
        /// </summary>
        /// <param name="isHit">آیا Cache Hit بوده است</param>
        /// <param name="responseTimeMs">زمان پاسخ (به میلی‌ثانیه)</param>
        public void UpdateStatistics(bool isHit, double responseTimeMs = 0)
        {
            if (isHit)
            {
                CacheHits++;
            }
            else
            {
                CacheMisses++;
            }

            if (responseTimeMs > 0)
            {
                if (MaxResponseTimeMs == 0 || responseTimeMs > MaxResponseTimeMs)
                    MaxResponseTimeMs = responseTimeMs;

                if (MinResponseTimeMs == 0 || responseTimeMs < MinResponseTimeMs)
                    MinResponseTimeMs = responseTimeMs;

                // محاسبه میانگین
                var totalTime = AverageResponseTimeMs * (TotalRequests - 1) + responseTimeMs;
                AverageResponseTimeMs = totalTime / TotalRequests;
            }

            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// ثبت خطای Cache
        /// </summary>
        public void RecordCacheError()
        {
            CacheErrors++;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// پاک کردن Cache
        /// </summary>
        public void ClearCache()
        {
            TotalCachedItems = 0;
            CacheHits = 0;
            CacheMisses = 0;
            CacheErrors = 0;
            ExpiredItems = 0;
            EvictedItems = 0;
            CacheSizeBytes = 0;
            AverageResponseTimeMs = 0;
            MaxResponseTimeMs = 0;
            MinResponseTimeMs = 0;
            LastCacheClear = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// دریافت خلاصه آمار
        /// </summary>
        /// <returns>خلاصه آمار Cache</returns>
        public string GetSummary()
        {
            return $"Cache Statistics - Items: {TotalCachedItems}, Hit Rate: {CacheHitRate:F1}%, " +
                   $"Response Time: {AverageResponseTimeMs:F1}ms, Errors: {CacheErrors}, " +
                   $"Uptime: {CacheUptimeMinutes:F1}min";
        }
    }
}
