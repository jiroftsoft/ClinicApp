using System.Collections.Generic;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel اصلی برای داشبورد بیمار
    /// Single Responsibility: نمایش داده‌های داشبورد
    /// 
    /// ✅ Enterprise-Grade: Strongly-Typed, No Magic Strings
    /// طبق: DEVELOPMENT_CONTRACT.md
    /// </summary>
    public class DashboardViewModel
    {
        public DashboardQuickStatsViewModel QuickStats { get; set; }
        public DashboardAppointmentsSectionViewModel RecentAppointments { get; set; }
        public DashboardAppointmentsSectionViewModel UpcomingAppointments { get; set; }
        public DashboardReceptionsSectionViewModel RecentReceptions { get; set; }

        /// <summary>
        /// خطاهای هر سکشن (در صورت شکست جزئی) — کلید: QuickStats, RecentAppointments, UpcomingAppointments, RecentReceptions
        /// </summary>
        public Dictionary<string, string> SectionErrors { get; set; }
    }

    /// <summary>
    /// آمار سریع داشبورد
    /// </summary>
    public class DashboardQuickStatsViewModel
    {
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int TotalReceptions { get; set; }
    }

    /// <summary>
    /// Section نوبت‌ها
    /// </summary>
    public class DashboardAppointmentsSectionViewModel
    {
        public List<DashboardAppointmentItemViewModel> Appointments { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }

    /// <summary>
    /// آیتم نوبت در داشبورد
    /// </summary>
    public class DashboardAppointmentItemViewModel
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        public System.DateTime AppointmentDate { get; set; }
        public string AppointmentDateShamsi { get; set; }
        public string AppointmentTime { get; set; }
        public string Status { get; set; }
        public string StatusText { get; set; }
        public decimal? Price { get; set; }
    }

    /// <summary>
    /// Section پذیرش‌ها
    /// </summary>
    public class DashboardReceptionsSectionViewModel
    {
        public List<DashboardReceptionItemViewModel> Receptions { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }

    /// <summary>
    /// آیتم پذیرش در داشبورد
    /// </summary>
    public class DashboardReceptionItemViewModel
    {
        public int ReceptionId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public System.DateTime ReceptionDate { get; set; }
        public string ReceptionDateShamsi { get; set; }
        public string Status { get; set; }
        public string StatusText { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}

