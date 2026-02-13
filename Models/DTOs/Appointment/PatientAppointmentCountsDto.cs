namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// خروجی کوئری اختصاصی آمار نوبت‌های یک بیمار — فقط اعداد، بدون بارگذاری موجودیت.
    /// برای داشبورد Real-Time و مقیاس‌پذیر (هزاران بیمار).
    /// </summary>
    public class PatientAppointmentCountsDto
    {
        public int Total { get; set; }
        public int Upcoming { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }
}
