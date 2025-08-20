using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// مدل داده‌های گزارش استفاده از خدمات پزشکی
    /// این مدل برای نمایش گزارش‌های آماری در محیط‌های پزشکی طراحی شده است
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی کامل از تاریخ‌های شمسی و میلادی
    /// 2. آمار روزانه برای تحلیل‌های پیشرفته
    /// 3. اطلاعات کامل برای تصمیم‌گیری‌های مدیریتی
    /// 4. قابلیت گزارش‌گیری در محیط‌های پزشکی
    /// </summary>
    public class ServiceReportViewModel
    {
        #region اطلاعات پایه خدمات

        /// <summary>
        /// شناسه خدمات پزشکی
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// عنوان خدمات پزشکی
        /// </summary>
        public string ServiceTitle { get; set; }

        /// <summary>
        /// کد خدمات پزشکی
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// عنوان دسته‌بندی خدمات پزشکی
        /// </summary>
        public string ServiceCategoryTitle { get; set; }

        #endregion

        #region اطلاعات زمانی گزارش

        /// <summary>
        /// تاریخ شروع بازه گزارش (میلادی)
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان بازه گزارش (میلادی)
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// تاریخ شروع بازه گزارش (شمسی)
        /// </summary>
        public string StartDateShamsi { get; set; }

        /// <summary>
        /// تاریخ پایان بازه گزارش (شمسی)
        /// </summary>
        public string EndDateShamsi { get; set; }

        #endregion

        #region آمار کلی

        /// <summary>
        /// تعداد کل استفاده‌ها در بازه گزارش
        /// </summary>
        public int TotalUsage { get; set; }

        /// <summary>
        /// درآمد کل ایجاد شده در بازه گزارش
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// میانگین روزانه استفاده
        /// </summary>
        public double AverageDailyUsage { get; set; }

        /// <summary>
        /// میانگین روزانه درآمد
        /// </summary>
        public decimal AverageDailyRevenue { get; set; }

        #endregion

        #region آمار روزانه

        /// <summary>
        /// آمار استفاده روزانه (کلید: تاریخ شمسی، مقدار: تعداد استفاده)
        /// </summary>
        public Dictionary<string, int> DailyUsage { get; set; }

        /// <summary>
        /// آمار درآمد روزانه (کلید: تاریخ شمسی، مقدار: مقدار درآمد)
        /// </summary>
        public Dictionary<string, decimal> DailyRevenue { get; set; }

        #endregion

        #region اطلاعات تکمیلی

        /// <summary>
        /// ایجاد کننده خدمات
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ ایجاد خدمات (شمسی)
        /// </summary>
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// تاریخ آخرین استفاده (شمسی)
        /// </summary>
        public string LastUsageDateShamsi { get; set; }

        /// <summary>
        /// تاریخ اولین استفاده در بازه (شمسی)
        /// </summary>
        public string FirstUsageDateShamsi { get; set; }

        #endregion

        #region اطلاعات سیستمی

        /// <summary>
        /// تاریخ تولید گزارش
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// نام کاربری تولید کننده گزارش
        /// </summary>
        public string GeneratedByUserName { get; set; }

        /// <summary>
        /// زمان تولید گزارش برای ردیابی عملکرد
        /// </summary>
        public DateTime ReportGenerationTime { get; set; }

        #endregion

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        public ServiceReportViewModel()
        {
            DailyUsage = new Dictionary<string, int>();
            DailyRevenue = new Dictionary<string, decimal>();
            GeneratedAt = DateTime.Now;
            ReportGenerationTime = DateTime.Now;
        }

        /// <summary>
        /// محاسبه میانگین روزانه استفاده
        /// </summary>
        public void CalculateAverages()
        {
            if (DailyUsage?.Count > 0)
            {
                int totalDays = (EndDate - StartDate).Days + 1;
                AverageDailyUsage = totalDays > 0 ? (double)TotalUsage / totalDays : 0;
            }

            if (DailyRevenue?.Count > 0 && TotalRevenue > 0)
            {
                int totalDays = (EndDate - StartDate).Days + 1;
                AverageDailyRevenue = totalDays > 0 ? TotalRevenue / totalDays : 0;
            }
        }

        /// <summary>
        /// دریافت تعداد روزهای دارای استفاده
        /// </summary>
        /// <returns>تعداد روزهای فعال</returns>
        public int GetActiveDaysCount()
        {
            return DailyUsage?.Count(d => d.Value > 0) ?? 0;
        }

        /// <summary>
        /// دریافت حداکثر استفاده روزانه
        /// </summary>
        /// <returns>حداکثر استفاده در یک روز</returns>
        public int GetMaxDailyUsage()
        {
            return DailyUsage?.Values.Count > 0 ? DailyUsage.Values.Max() : 0;
        }

        /// <summary>
        /// دریافت حداکثر درآمد روزانه
        /// </summary>
        /// <returns>حداکثر درآمد در یک روز</returns>
        public decimal GetMaxDailyRevenue()
        {
            return DailyRevenue?.Values.Count > 0 ? DailyRevenue.Values.Max() : 0;
        }
    }

    /// <summary>
    /// مدل آمار استفاده از خدمات پزشکی
    /// این مدل برای انتقال آمار بین لایه‌های سیستم استفاده می‌شود
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ساختار سبک برای انتقال داده
    /// 2. پشتیبانی از آمار روزانه
    /// 3. قابلیت محاسبات آماری
    /// 4. مناسب برای سرویس‌های داخلی
    /// </summary>
    public class ServiceUsageStatistics
    {
        #region آمار کلی

        /// <summary>
        /// تعداد کل استفاده‌ها
        /// </summary>
        public int TotalUsage { get; set; }

        /// <summary>
        /// درآمد کل
        /// </summary>
        public decimal TotalRevenue { get; set; }

        #endregion

        #region آمار روزانه

        /// <summary>
        /// آمار استفاده روزانه (کلید: تاریخ شمسی، مقدار: تعداد استفاده)
        /// </summary>
        public Dictionary<string, int> DailyUsage { get; set; }

        /// <summary>
        /// آمار درآمد روزانه (کلید: تاریخ شمسی، مقدار: مقدار درآمد)
        /// </summary>
        public Dictionary<string, decimal> DailyRevenue { get; set; }

        #endregion

        #region اطلاعات زمانی

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime EndDate { get; set; }

        #endregion

        #region آمار تکمیلی

        /// <summary>
        /// میانگین روزانه استفاده
        /// </summary>
        public double AverageDailyUsage { get; set; }

        /// <summary>
        /// میانگین روزانه درآمد
        /// </summary>
        public decimal AverageDailyRevenue { get; set; }

        /// <summary>
        /// تاریخ اولین استفاده
        /// </summary>
        public DateTime? FirstUsageDate { get; set; }

        /// <summary>
        /// تاریخ آخرین استفاده
        /// </summary>
        public DateTime? LastUsageDate { get; set; }

        #endregion

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        public ServiceUsageStatistics()
        {
            DailyUsage = new Dictionary<string, int>();
            DailyRevenue = new Dictionary<string, decimal>();
        }

        /// <summary>
        /// محاسبه آمارهای تکمیلی
        /// </summary>
        public void CalculateStatistics()
        {
            if (DailyUsage?.Count > 0)
            {
                int totalDays = (EndDate - StartDate).Days + 1;
                AverageDailyUsage = totalDays > 0 ? (double)TotalUsage / totalDays : 0;
            }

            if (DailyRevenue?.Count > 0 && TotalRevenue > 0)
            {
                int totalDays = (EndDate - StartDate).Days + 1;
                AverageDailyRevenue = totalDays > 0 ? TotalRevenue / totalDays : 0;
            }
        }
    }
}