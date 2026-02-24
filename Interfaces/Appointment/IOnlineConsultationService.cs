using System.Threading.Tasks;
using ClinicApp.ViewModels.OnlineConsultation;

namespace ClinicApp.Interfaces.Appointment;

/// <summary>
/// سرویس مشاوره آنلاین تصویری (ورود به اتاق Jitsi)
/// </summary>
public interface IOnlineConsultationService
{
    /// <summary>
    /// ورود بیمار به اتاق: بررسی نوبت و بیمار؛ ایجاد/بازیابی اتاق؛ برگرداندن ViewModel برای Join.
    /// </summary>
    Task<JoinConsultationViewModel> GetOrCreateRoomForPatientAsync(int appointmentId, int patientId);

    /// <summary>
    /// ورود پزشک به اتاق: بررسی نوبت و پزشک؛ ایجاد/بازیابی اتاق؛ برگرداندن ViewModel برای Join.
    /// </summary>
    Task<JoinConsultationViewModel> GetOrCreateRoomForDoctorAsync(int appointmentId, int doctorId);
}
