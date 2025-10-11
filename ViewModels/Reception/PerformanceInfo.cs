using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// اطلاعات عملکرد سیستم
    /// </summary>
    public class PerformanceInfo
    {
        /// <summary>
        /// زمان شروع
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// زمان پایان
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// مدت زمان اجرا (میلی‌ثانیه)
        /// </summary>
        public long ExecutionTimeMs => (long)(EndTime - StartTime).TotalMilliseconds;

        /// <summary>
        /// تعداد کوئری‌های اجرا شده
        /// </summary>
        public int QueryCount { get; set; }

        /// <summary>
        /// تعداد رکوردهای پردازش شده
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// استفاده از حافظه (بایت)
        /// </summary>
        public long MemoryUsageBytes { get; set; }

        /// <summary>
        /// استفاده از CPU (درصد)
        /// </summary>
        public double CpuUsagePercent { get; set; }

        /// <summary>
        /// آیا عملکرد بهینه است
        /// </summary>
        public bool IsOptimal { get; set; }

        /// <summary>
        /// پیام عملکرد
        /// </summary>
        public string PerformanceMessage { get; set; }
    }
}
