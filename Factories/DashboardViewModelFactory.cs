using ClinicApp.ViewModels.Patient;

namespace ClinicApp.Factories
{
    /// <summary>
    /// Factory برای ساخت DashboardViewModel
    /// Single Responsibility: ساخت ViewModel برای Dashboard
    /// طبق: DEVELOPMENT_CONTRACT.md - Factory Method Pattern
    /// </summary>
    public static class DashboardViewModelFactory
    {
        /// <summary>
        /// ساخت DashboardViewModel خالی برای initial load
        /// </summary>
        public static DashboardViewModel CreateEmpty()
        {
            return new DashboardViewModel
            {
                QuickStats = null, // Will be loaded via AJAX
                RecentAppointments = null, // Will be loaded via AJAX
                UpcomingAppointments = null, // Will be loaded via AJAX
                RecentReceptions = null // Will be loaded via AJAX
            };
        }

        /// <summary>
        /// ساخت DashboardViewModel با داده
        /// </summary>
        public static DashboardViewModel Create(
            DashboardQuickStatsViewModel quickStats = null,
            DashboardAppointmentsSectionViewModel recentAppointments = null,
            DashboardAppointmentsSectionViewModel upcomingAppointments = null,
            DashboardReceptionsSectionViewModel recentReceptions = null)
        {
            return new DashboardViewModel
            {
                QuickStats = quickStats,
                RecentAppointments = recentAppointments,
                UpcomingAppointments = upcomingAppointments,
                RecentReceptions = recentReceptions
            };
        }
    }
}

