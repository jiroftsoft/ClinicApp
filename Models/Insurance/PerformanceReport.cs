using System;
using System.Collections.Generic;

namespace ClinicApp.Models.Insurance
{
    /// <summary>
    /// گزارش عملکرد سیستم بیمه تکمیلی
    /// </summary>
    public class PerformanceReport
    {
        /// <summary>
        /// تاریخ شروع گزارش
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// تاریخ پایان گزارش
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// تعداد کل محاسبات
        /// </summary>
        public int TotalCalculations { get; set; }

        /// <summary>
        /// تعداد محاسبات موفق
        /// </summary>
        public int SuccessfulCalculations { get; set; }

        /// <summary>
        /// تعداد محاسبات ناموفق
        /// </summary>
        public int FailedCalculations { get; set; }

        /// <summary>
        /// درصد موفقیت
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// درصد خطا
        /// </summary>
        public double ErrorRate { get; set; }

        /// <summary>
        /// تجزیه خطاها
        /// </summary>
        public Dictionary<string, int> ErrorBreakdown { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// میانگین زمان محاسبه (به میلی‌ثانیه)
        /// </summary>
        public double AverageCalculationTime { get; set; }

        /// <summary>
        /// حداکثر زمان محاسبه (به میلی‌ثانیه)
        /// </summary>
        public double MaxCalculationTime { get; set; }

        /// <summary>
        /// حداقل زمان محاسبه (به میلی‌ثانیه)
        /// </summary>
        public double MinCalculationTime { get; set; }

        /// <summary>
        /// تعداد خطاها
        /// </summary>
        public int TotalErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای بحرانی
        /// </summary>
        public int CriticalErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای هشدار
        /// </summary>
        public int WarningErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای عادی
        /// </summary>
        public int NormalErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای حل شده
        /// </summary>
        public int ResolvedErrors { get; set; }

        /// <summary>
        /// تعداد خطاهای حل نشده
        /// </summary>
        public int UnresolvedErrors { get; set; }

        /// <summary>
        /// درصد حل خطاها
        /// </summary>
        public double ErrorResolutionRate => TotalErrors > 0 ? (double)ResolvedErrors / TotalErrors * 100 : 0;

        /// <summary>
        /// تعداد کاربران فعال
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// تعداد کلینیک‌های فعال
        /// </summary>
        public int ActiveClinics { get; set; }

        /// <summary>
        /// تعداد بیماران پردازش شده
        /// </summary>
        public int ProcessedPatients { get; set; }

        /// <summary>
        /// تعداد خدمات پردازش شده
        /// </summary>
        public int ProcessedServices { get; set; }

        /// <summary>
        /// میانگین مبلغ خدمات
        /// </summary>
        public decimal AverageServiceAmount { get; set; }

        /// <summary>
        /// میانگین پوشش بیمه اصلی
        /// </summary>
        public decimal AveragePrimaryCoverage { get; set; }

        /// <summary>
        /// میانگین پوشش بیمه تکمیلی
        /// </summary>
        public decimal AverageSupplementaryCoverage { get; set; }

        /// <summary>
        /// میانگین سهم بیمار
        /// </summary>
        public decimal AveragePatientShare { get; set; }

        /// <summary>
        /// میانگین درصد پوشش کل
        /// </summary>
        public double AverageTotalCoveragePercent { get; set; }

        /// <summary>
        /// میانگین درصد سهم بیمار
        /// </summary>
        public double AveragePatientSharePercent { get; set; }

        /// <summary>
        /// میانگین صرفه‌جویی بیمار
        /// </summary>
        public decimal AveragePatientSavings { get; set; }

        /// <summary>
        /// میانگین کارایی بیمه تکمیلی
        /// </summary>
        public double AverageSupplementaryEfficiency { get; set; }

        /// <summary>
        /// آمار عملکرد روزانه
        /// </summary>
        public List<DailyPerformance> DailyPerformances { get; set; } = new List<DailyPerformance>();

        /// <summary>
        /// آمار عملکرد ساعتی
        /// </summary>
        public List<HourlyPerformance> HourlyPerformances { get; set; } = new List<HourlyPerformance>();

        /// <summary>
        /// آمار عملکرد بر اساس نوع خدمت
        /// </summary>
        public List<ServiceTypePerformance> ServiceTypePerformances { get; set; } = new List<ServiceTypePerformance>();

        /// <summary>
        /// آمار عملکرد بر اساس کلینیک
        /// </summary>
        public List<ClinicPerformance> ClinicPerformances { get; set; } = new List<ClinicPerformance>();

        /// <summary>
        /// زمان ایجاد گزارش
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// مدت زمان تولید گزارش (به میلی‌ثانیه)
        /// </summary>
        public long GenerationTime { get; set; }

        /// <summary>
        /// دریافت خلاصه گزارش
        /// </summary>
        /// <returns>خلاصه گزارش عملکرد</returns>
        public string GetSummary()
        {
            return $"Performance Report - Period: {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}, " +
                   $"Total Calculations: {TotalCalculations}, Success Rate: {SuccessRate:F1}%, " +
                   $"Average Time: {AverageCalculationTime:F1}ms, Total Errors: {TotalErrors}, " +
                   $"Active Users: {ActiveUsers}, Processed Patients: {ProcessedPatients}";
        }
    }

    /// <summary>
    /// آمار عملکرد روزانه
    /// </summary>
    public class DailyPerformance
    {
        public DateTime Date { get; set; }
        public int TotalCalculations { get; set; }
        public int SuccessfulCalculations { get; set; }
        public int FailedCalculations { get; set; }
        public double SuccessRate { get; set; }
        public double AverageCalculationTime { get; set; }
        public int TotalErrors { get; set; }
        public int ActiveUsers { get; set; }
        public int ProcessedPatients { get; set; }
    }

    /// <summary>
    /// آمار عملکرد ساعتی
    /// </summary>
    public class HourlyPerformance
    {
        public int Hour { get; set; }
        public int TotalCalculations { get; set; }
        public int SuccessfulCalculations { get; set; }
        public int FailedCalculations { get; set; }
        public double SuccessRate { get; set; }
        public double AverageCalculationTime { get; set; }
        public int TotalErrors { get; set; }
        public int ActiveUsers { get; set; }
    }

    /// <summary>
    /// آمار عملکرد بر اساس نوع خدمت
    /// </summary>
    public class ServiceTypePerformance
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int TotalCalculations { get; set; }
        public int SuccessfulCalculations { get; set; }
        public int FailedCalculations { get; set; }
        public double SuccessRate { get; set; }
        public double AverageCalculationTime { get; set; }
        public decimal AverageServiceAmount { get; set; }
        public decimal AveragePrimaryCoverage { get; set; }
        public decimal AverageSupplementaryCoverage { get; set; }
        public decimal AveragePatientShare { get; set; }
    }

    /// <summary>
    /// آمار عملکرد بر اساس کلینیک
    /// </summary>
    public class ClinicPerformance
    {
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        public int TotalCalculations { get; set; }
        public int SuccessfulCalculations { get; set; }
        public int FailedCalculations { get; set; }
        public double SuccessRate { get; set; }
        public double AverageCalculationTime { get; set; }
        public int TotalErrors { get; set; }
        public int ActiveUsers { get; set; }
        public int ProcessedPatients { get; set; }
    }
}
