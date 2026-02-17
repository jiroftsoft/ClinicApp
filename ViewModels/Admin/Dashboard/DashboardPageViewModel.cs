using System.Collections.Generic;

namespace ClinicApp.ViewModels.Admin.Dashboard
{
    /// <summary>
    /// مدل صفحهٔ داشبورد پلتفرم ادمین — آمادهٔ اتصال به سرویس‌های نوبت/بیمار/مالی در فاز بعد
    /// طبق Docs/ADMIN_PLATFORM_ARCHITECTURE.md
    /// </summary>
    public class DashboardPageViewModel
    {
        /// <summary>خلاصه نوبت‌های امروز (فاز بعد: از IAppointmentService)</summary>
        public WidgetTodayAppointmentsViewModel TodayAppointments { get; set; }

        /// <summary>آمار بیمار (فاز بعد: از IPatientService)</summary>
        public WidgetPatientStatsViewModel PatientStats { get; set; }

        /// <summary>خلاصه مالی (فاز بعد: از سرویس مالی/پذیرش)</summary>
        public WidgetFinancialOverviewViewModel FinancialOverview { get; set; }

        /// <summary>عملیات سریع — بدون نیاز به دادهٔ سرور</summary>
        public bool ShowQuickActions { get; set; } = true;
    }

    /// <summary>ویجت نوبت‌های امروز</summary>
    public class WidgetTodayAppointmentsViewModel
    {
        public int Count { get; set; }
        public List<TodayAppointmentItemViewModel> Items { get; set; } = new List<TodayAppointmentItemViewModel>();
    }

    public class TodayAppointmentItemViewModel
    {
        public string PatientName { get; set; }
        public string Time { get; set; }
    }

    /// <summary>ویجت آمار بیمار — placeholder برای فاز بعد</summary>
    public class WidgetPatientStatsViewModel
    {
        public int TodayCount { get; set; }
        public int WeekCount { get; set; }
        public int MonthCount { get; set; }
    }

    /// <summary>ویجت خلاصه مالی — placeholder برای فاز بعد</summary>
    public class WidgetFinancialOverviewViewModel
    {
        public decimal TodayRevenue { get; set; }
        public decimal WeekRevenue { get; set; }
    }
}
