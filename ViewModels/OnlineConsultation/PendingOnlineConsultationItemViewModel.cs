using System;

namespace ClinicApp.ViewModels.OnlineConsultation;

/// <summary>
/// آیتم نوبت مشاوره آنلاین در انتظار (داشبورد پزشک)
/// </summary>
public class PendingOnlineConsultationItemViewModel
{
    public int AppointmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string PatientName { get; set; }
    public string JoinUrl { get; set; }
}
