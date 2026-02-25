using System.Collections.Generic;

namespace ClinicApp.ViewModels.Admin.ReceptionistDashboard
{
    /// <summary>
    /// مدل صفحهٔ داشبورد اختصاصی منشی — قابل گسترش و کاستومایز.
    /// </summary>
    public class ReceptionistDashboardPageViewModel
    {
        public WidgetReceptionQuickActionsViewModel QuickActions { get; set; }
        public WidgetTodayAppointmentsReceptionViewModel TodayAppointments { get; set; }
        public WidgetPendingReceptionsViewModel PendingReceptions { get; set; }
        public WidgetTodayDoctorsViewModel TodayDoctors { get; set; }
        public WidgetTodayStatsViewModel TodayStats { get; set; }
    }

    public class WidgetReceptionQuickActionsViewModel
    {
        public bool ShowReceptionNew { get; set; } = true;
        public bool ShowReceptionList { get; set; } = true;
        public bool ShowCashierDashboard { get; set; } = true;
    }

    public class WidgetTodayAppointmentsReceptionViewModel
    {
        public int Count { get; set; }
        public List<TodayAppointmentReceptionItemViewModel> Items { get; set; } = new List<TodayAppointmentReceptionItemViewModel>();
    }

    public class TodayAppointmentReceptionItemViewModel
    {
        public string PatientName { get; set; }
        public string Time { get; set; }
        public string DoctorName { get; set; }
    }

    public class WidgetPendingReceptionsViewModel
    {
        public int Count { get; set; }
        public List<PendingReceptionItemViewModel> Items { get; set; } = new List<PendingReceptionItemViewModel>();
    }

    public class PendingReceptionItemViewModel
    {
        public int ReceptionId { get; set; }
        public string ReceptionNo { get; set; }
        public string PatientName { get; set; }
    }

    public class WidgetTodayDoctorsViewModel
    {
        public int Count { get; set; }
        public List<TodayDoctorItemViewModel> Items { get; set; } = new List<TodayDoctorItemViewModel>();
    }

    public class TodayDoctorItemViewModel
    {
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
    }

    public class WidgetTodayStatsViewModel
    {
        public int ReceptionsToday { get; set; }
        public decimal RevenueToday { get; set; }
    }
}
