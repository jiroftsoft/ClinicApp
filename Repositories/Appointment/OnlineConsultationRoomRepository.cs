using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Infrastructure;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Appointment;
using Serilog;

namespace ClinicApp.Repositories.Appointment
{
    public class OnlineConsultationRoomRepository : IOnlineConsultationRoomRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITimeProvider _timeProvider;
        private readonly ILogger _logger;

        public OnlineConsultationRoomRepository(ApplicationDbContext context, ITimeProvider timeProvider, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _logger = logger?.ForContext<OnlineConsultationRoomRepository>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OnlineConsultationRoom> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.OnlineConsultationRooms
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
        }

        public async Task<OnlineConsultationRoom> GetOrCreateForAppointmentAsync(int appointmentId, string roomName, string createdByUserId)
        {
            var existing = await GetByAppointmentIdAsync(appointmentId);
            if (existing != null)
                return existing;

            var room = new OnlineConsultationRoom
            {
                AppointmentId = appointmentId,
                RoomName = roomName,
                CreatedAt = _timeProvider.UtcNow,
                CreatedByUserId = createdByUserId
            };
            _context.OnlineConsultationRooms.Add(room);
            await _context.SaveChangesAsync();
            _logger.Information("اتاق مشاوره آنلاین ایجاد شد - AppointmentId: {AppointmentId}, RoomName: {RoomName}", appointmentId, roomName);
            return room;
        }
    }
}
