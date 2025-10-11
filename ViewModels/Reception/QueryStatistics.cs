using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// آمار کوئری‌های دیتابیس
    /// </summary>
    public class QueryStatistics
    {
        /// <summary>
        /// تعداد کل کوئری‌ها
        /// </summary>
        public int TotalQueries { get; set; }

        /// <summary>
        /// تعداد کوئری‌های SELECT
        /// </summary>
        public int SelectQueries { get; set; }

        /// <summary>
        /// تعداد کوئری‌های INSERT
        /// </summary>
        public int InsertQueries { get; set; }

        /// <summary>
        /// تعداد کوئری‌های UPDATE
        /// </summary>
        public int UpdateQueries { get; set; }

        /// <summary>
        /// تعداد کوئری‌های DELETE
        /// </summary>
        public int DeleteQueries { get; set; }

        /// <summary>
        /// میانگین زمان اجرای کوئری‌ها (میلی‌ثانیه)
        /// </summary>
        public double AverageExecutionTimeMs { get; set; }

        /// <summary>
        /// حداکثر زمان اجرای کوئری (میلی‌ثانیه)
        /// </summary>
        public double MaxExecutionTimeMs { get; set; }

        /// <summary>
        /// حداقل زمان اجرای کوئری (میلی‌ثانیه)
        /// </summary>
        public double MinExecutionTimeMs { get; set; }

        /// <summary>
        /// تعداد کوئری‌های کند (بیش از 1000ms)
        /// </summary>
        public int SlowQueries { get; set; }

        /// <summary>
        /// تعداد کوئری‌های N+1
        /// </summary>
        public int NPlusOneQueries { get; set; }

        /// <summary>
        /// لیست کوئری‌های کند
        /// </summary>
        public List<string> SlowQueryList { get; set; } = new List<string>();

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
