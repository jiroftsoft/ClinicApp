using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// ViewModel برای داشبورد بهینه‌سازی برنامه کاری
    /// Production-Ready: تمام داده‌ها از دیتابیس واقعی دریافت می‌شوند
    /// </summary>
    public class ScheduleOptimizationDashboardViewModel
    {
        /// <summary>
        /// تعداد پزشکان فعال
        /// </summary>
        [Display(Name = "پزشکان فعال")]
        public int ActiveDoctorsCount { get; set; }

        /// <summary>
        /// تعداد نوبت‌های امروز
        /// </summary>
        [Display(Name = "نوبت‌های امروز")]
        public int TodayAppointmentsCount { get; set; }

        /// <summary>
        /// تعداد بهینه‌سازی‌های انجام شده (این ماه)
        /// </summary>
        [Display(Name = "بهینه‌سازی‌های انجام شده")]
        public int OptimizationsCount { get; set; }

        /// <summary>
        /// درصد بهینه‌سازی (میانگین)
        /// </summary>
        [Display(Name = "درصد بهینه‌سازی")]
        [DisplayFormat(DataFormatString = "{0:F1}%")]
        public decimal OptimizationPercentage { get; set; }

        /// <summary>
        /// آخرین به‌روزرسانی
        /// </summary>
        [Display(Name = "آخرین به‌روزرسانی")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// آمار Chart برای نمایش
        /// </summary>
        public OptimizationChartData ChartData { get; set; } = new OptimizationChartData();

        /// <summary>
        /// آخرین بهینه‌سازی‌ها
        /// </summary>
        public List<RecentOptimizationViewModel> RecentOptimizations { get; set; } = new List<RecentOptimizationViewModel>();

        /// <summary>
        /// پیشنهادات بهینه‌سازی
        /// </summary>
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// داده‌های Chart برای نمایش
    /// </summary>
    public class OptimizationChartData
    {
        /// <summary>
        /// برچسب‌های Chart
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();

        /// <summary>
        /// داده‌های Chart
        /// </summary>
        public List<int> Data { get; set; } = new List<int>();

        /// <summary>
        /// رنگ‌های Chart
        /// </summary>
        public List<string> BackgroundColors { get; set; } = new List<string>();
    }

    /// <summary>
    /// ViewModel برای آخرین بهینه‌سازی‌ها
    /// </summary>
    public class RecentOptimizationViewModel
    {
        /// <summary>
        /// شناسه بهینه‌سازی
        /// </summary>
        public int OptimizationId { get; set; }

        /// <summary>
        /// تاریخ بهینه‌سازی
        /// </summary>
        [Display(Name = "تاریخ")]
        public DateTime OptimizationDate { get; set; }

        /// <summary>
        /// تاریخ بهینه‌سازی به شمسی
        /// </summary>
        [Display(Name = "تاریخ (شمسی)")]
        public string OptimizationDateShamsi { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// نوع بهینه‌سازی
        /// </summary>
        [Display(Name = "نوع بهینه‌سازی")]
        public string OptimizationType { get; set; }

        /// <summary>
        /// وضعیت
        /// </summary>
        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        /// <summary>
        /// نتایج
        /// </summary>
        [Display(Name = "نتایج")]
        public string Results { get; set; }
    }
}

