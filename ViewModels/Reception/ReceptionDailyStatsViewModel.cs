using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای آمار روزانه پذیرش‌ها
    /// </summary>
    public class ReceptionDailyStatsViewModel
    {
        [Display(Name = "تاریخ")]
        public string Date { get; set; }

        [Display(Name = "تعداد کل پذیرش‌ها")]
        public int TotalReceptions { get; set; }

        [Display(Name = "تعداد پذیرش‌های اورژانس")]
        public int EmergencyReceptions { get; set; }

        [Display(Name = "تعداد پذیرش‌های عادی")]
        public int NormalReceptions { get; set; }

        [Display(Name = "مجموع درآمد")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "میانگین زمان انتظار")]
        public string AverageWaitingTime { get; set; }

        [Display(Name = "تعداد پزشکان فعال")]
        public int ActiveDoctors { get; set; }
    }
}
