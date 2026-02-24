namespace ClinicApp.ViewModels.OnlineConsultation;

/// <summary>
/// ViewModel برای صفحه ورود به اتاق مشاوره آنلاین (Jitsi) — مناسب پروداکشن درمانی.
/// </summary>
public class JoinConsultationViewModel
{
    public int AppointmentId { get; set; }
    public string RoomName { get; set; }
    public string JitsiBaseUrl { get; set; }
    public string PatientName { get; set; }
    public string DoctorName { get; set; }

    /// <summary>
    /// آیا ورود به اتاق در این لحظه مجاز است (اتاق و بازه زمانی معتبر).
    /// </summary>
    public bool CanJoin { get; set; }

    /// <summary>
    /// پیام امن برای نمایش به کاربر در صورت عدم امکان ورود (بدون افشای جزئیات داخلی).
    /// </summary>
    public string UserMessage { get; set; }
}
