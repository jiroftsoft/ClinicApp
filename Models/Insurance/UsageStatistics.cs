using System;
using System.Collections.Generic;

namespace ClinicApp.Models.Insurance
{
    /// <summary>
    /// آمار استفاده از سیستم بیمه تکمیلی
    /// </summary>
    public class UsageStatistics
    {
        /// <summary>
        /// تاریخ شروع آمار
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// تاریخ پایان آمار
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// تعداد کل درخواست‌ها
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// تعداد درخواست‌های موفق
        /// </summary>
        public int SuccessfulRequests { get; set; }

        /// <summary>
        /// تعداد درخواست‌های ناموفق
        /// </summary>
        public int FailedRequests { get; set; }

        /// <summary>
        /// درصد موفقیت درخواست‌ها
        /// </summary>
        public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests * 100 : 0;

        /// <summary>
        /// تعداد کاربران منحصر به فرد
        /// </summary>
        public int UniqueUsers { get; set; }

        /// <summary>
        /// تعداد کلینیک‌های فعال
        /// </summary>
        public int ActiveClinics { get; set; }

        /// <summary>
        /// تعداد بیماران منحصر به فرد
        /// </summary>
        public int UniquePatients { get; set; }

        /// <summary>
        /// تعداد خدمات منحصر به فرد
        /// </summary>
        public int UniqueServices { get; set; }

        /// <summary>
        /// تعداد طرح‌های بیمه منحصر به فرد
        /// </summary>
        public int UniqueInsurancePlans { get; set; }

        /// <summary>
        /// میانگین درخواست‌ها در روز
        /// </summary>
        public double AverageRequestsPerDay { get; set; }

        /// <summary>
        /// میانگین درخواست‌ها در ساعت
        /// </summary>
        public double AverageRequestsPerHour { get; set; }

        /// <summary>
        /// حداکثر درخواست‌ها در یک روز
        /// </summary>
        public int MaxRequestsPerDay { get; set; }

        /// <summary>
        /// حداکثر درخواست‌ها در یک ساعت
        /// </summary>
        public int MaxRequestsPerHour { get; set; }

        /// <summary>
        /// میانگین زمان پاسخ (به میلی‌ثانیه)
        /// </summary>
        public double AverageResponseTime { get; set; }

        /// <summary>
        /// حداکثر زمان پاسخ (به میلی‌ثانیه)
        /// </summary>
        public double MaxResponseTime { get; set; }

        /// <summary>
        /// حداقل زمان پاسخ (به میلی‌ثانیه)
        /// </summary>
        public double MinResponseTime { get; set; }

        /// <summary>
        /// تعداد درخواست‌های Cache Hit
        /// </summary>
        public int CacheHits { get; set; }

        /// <summary>
        /// تعداد درخواست‌های Cache Miss
        /// </summary>
        public int CacheMisses { get; set; }

        /// <summary>
        /// درصد Cache Hit
        /// </summary>
        public double CacheHitRate => (CacheHits + CacheMisses) > 0 ? (double)CacheHits / (CacheHits + CacheMisses) * 100 : 0;

        /// <summary>
        /// تعداد خطاهای سیستم
        /// </summary>
        public int SystemErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای اعتبارسنجی
        /// </summary>
        public int ValidationErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای شبکه
        /// </summary>
        public int NetworkErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای پایگاه داده
        /// </summary>
        public int DatabaseErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای احراز هویت
        /// </summary>
        public int AuthenticationErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای مجوز
        /// </summary>
        public int AuthorizationErrors { get; set; }

        /// <summary>
        /// آمار استفاده روزانه
        /// </summary>
        public List<DailyUsage> DailyUsages { get; set; } = new List<DailyUsage>();

        /// <summary>
        /// آمار استفاده ساعتی
        /// </summary>
        public List<HourlyUsage> HourlyUsages { get; set; } = new List<HourlyUsage>();

        /// <summary>
        /// آمار استفاده بر اساس کاربر
        /// </summary>
        public List<UserUsage> UserUsages { get; set; } = new List<UserUsage>();

        /// <summary>
        /// آمار استفاده بر اساس کلینیک
        /// </summary>
        public List<ClinicUsage> ClinicUsages { get; set; } = new List<ClinicUsage>();

        /// <summary>
        /// آمار استفاده بر اساس نوع درخواست
        /// </summary>
        public List<RequestTypeUsage> RequestTypeUsages { get; set; } = new List<RequestTypeUsage>();

        /// <summary>
        /// آمار استفاده بر اساس دستگاه
        /// </summary>
        public List<DeviceUsage> DeviceUsages { get; set; } = new List<DeviceUsage>();

        /// <summary>
        /// آمار استفاده بر اساس مرورگر
        /// </summary>
        public List<BrowserUsage> BrowserUsages { get; set; } = new List<BrowserUsage>();

        /// <summary>
        /// زمان ایجاد آمار
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// مدت زمان تولید آمار (به میلی‌ثانیه)
        /// </summary>
        public long GenerationTime { get; set; }

        /// <summary>
        /// دریافت خلاصه آمار
        /// </summary>
        /// <returns>خلاصه آمار استفاده</returns>
        public string GetSummary()
        {
            return $"Usage Statistics - Period: {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}, " +
                   $"Total Requests: {TotalRequests}, Success Rate: {SuccessRate:F1}%, " +
                   $"Unique Users: {UniqueUsers}, Active Clinics: {ActiveClinics}, " +
                   $"Average Response Time: {AverageResponseTime:F1}ms, Cache Hit Rate: {CacheHitRate:F1}%";
        }
    }

    /// <summary>
    /// آمار استفاده روزانه
    /// </summary>
    public class DailyUsage
    {
        public DateTime Date { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public int UniqueUsers { get; set; }
        public int UniquePatients { get; set; }
        public double AverageResponseTime { get; set; }
        public int CacheHits { get; set; }
        public int CacheMisses { get; set; }
        public double CacheHitRate { get; set; }
    }

    /// <summary>
    /// آمار استفاده ساعتی
    /// </summary>
    public class HourlyUsage
    {
        public int Hour { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public int UniqueUsers { get; set; }
        public double AverageResponseTime { get; set; }
        public int CacheHits { get; set; }
        public int CacheMisses { get; set; }
        public double CacheHitRate { get; set; }
    }

    /// <summary>
    /// آمار استفاده بر اساس کاربر
    /// </summary>
    public class UserUsage
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public int UniquePatients { get; set; }
        public int UniqueServices { get; set; }
        public DateTime FirstRequest { get; set; }
        public DateTime LastRequest { get; set; }
    }

    /// <summary>
    /// آمار استفاده بر اساس کلینیک
    /// </summary>
    public class ClinicUsage
    {
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public int UniqueUsers { get; set; }
        public int UniquePatients { get; set; }
        public int UniqueServices { get; set; }
    }

    /// <summary>
    /// آمار استفاده بر اساس نوع درخواست
    /// </summary>
    public class RequestTypeUsage
    {
        public string RequestType { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public int UniqueUsers { get; set; }
    }

    /// <summary>
    /// آمار استفاده بر اساس دستگاه
    /// </summary>
    public class DeviceUsage
    {
        public string DeviceType { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public int UniqueUsers { get; set; }
    }

    /// <summary>
    /// آمار استفاده بر اساس مرورگر
    /// </summary>
    public class BrowserUsage
    {
        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public int UniqueUsers { get; set; }
    }
}
