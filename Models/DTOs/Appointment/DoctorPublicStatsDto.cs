namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// آمار عمومی پزشک برای نمایش در صفحه جزئیات (پورتال بیمار)
    /// </summary>
    public class DoctorPublicStatsDto
    {
        /// <summary>تعداد کل نوبت‌های ثبت‌شده (غیرحذف‌شده)</summary>
        public int TotalAppointments { get; set; }

        /// <summary>تعداد نوبت‌های امروز (بر اساس تاریخ ایران)</summary>
        public int TodayAppointments { get; set; }
    }
}
