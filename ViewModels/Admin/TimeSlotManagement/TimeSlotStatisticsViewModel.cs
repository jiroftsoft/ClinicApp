using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Admin.TimeSlotManagement
{
    /// <summary>
    /// ViewModel برای نمایش آمار اسلات‌های زمانی
    /// </summary>
    public class TimeSlotStatisticsViewModel
    {
        [Display(Name = "کل اسلات‌ها")]
        public int TotalSlots { get; set; }

        [Display(Name = "در دسترس")]
        public int AvailableSlots { get; set; }

        [Display(Name = "رزرو شده")]
        public int BookedSlots { get; set; }

        [Display(Name = "انجام شده")]
        public int CompletedSlots { get; set; }

        [Display(Name = "لغو شده")]
        public int CancelledSlots { get; set; }

        [Display(Name = "عدم حضور")]
        public int NoShowSlots { get; set; }

        [Display(Name = "حذف شده")]
        public int DeletedSlots { get; set; }

        /// <summary>
        /// درصد اسلات‌های در دسترس
        /// </summary>
        [Display(Name = "درصد در دسترس")]
        public double AvailablePercentage => TotalSlots > 0 
            ? Math.Round((double)AvailableSlots / TotalSlots * 100, 2) 
            : 0;

        /// <summary>
        /// درصد اسلات‌های رزرو شده
        /// </summary>
        [Display(Name = "درصد رزرو شده")]
        public double BookedPercentage => TotalSlots > 0 
            ? Math.Round((double)BookedSlots / TotalSlots * 100, 2) 
            : 0;

        /// <summary>
        /// تبدیل از TimeSlotStatistics
        /// </summary>
        public static TimeSlotStatisticsViewModel FromStatistics(Interfaces.ClinicAdmin.TimeSlotStatistics statistics)
        {
            if (statistics == null) return null;

            return new TimeSlotStatisticsViewModel
            {
                TotalSlots = statistics.TotalSlots,
                AvailableSlots = statistics.AvailableSlots,
                BookedSlots = statistics.BookedSlots,
                CompletedSlots = statistics.CompletedSlots,
                CancelledSlots = statistics.CancelledSlots,
                NoShowSlots = statistics.NoShowSlots,
                DeletedSlots = statistics.DeletedSlots
            };
        }
    }
}

