using System.Threading.Tasks;
using ClinicApp.Models.Entities.Appointment;

namespace ClinicApp.Interfaces.Appointment;

/// <summary>
/// Repository اتاق مشاوره آنلاین
/// </summary>
public interface IOnlineConsultationRoomRepository
{
    Task<OnlineConsultationRoom> GetByAppointmentIdAsync(int appointmentId);
    Task<OnlineConsultationRoom> GetOrCreateForAppointmentAsync(int appointmentId, string roomName, string createdByUserId);
}
