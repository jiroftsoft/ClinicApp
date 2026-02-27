using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Models.DTOs.Appointment;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس گزارش نوبت‌های رزرو شده توسط بیماران — SRP: فقط تهیه داده گزارش.
    /// </summary>
    public class PatientBookedAppointmentsReportService : IPatientBookedAppointmentsReportService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger _logger = Log.ForContext<PatientBookedAppointmentsReportService>();

        public PatientBookedAppointmentsReportService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository ?? throw new System.ArgumentNullException(nameof(appointmentRepository));
        }

        public async Task<PatientBookedAppointmentsReportResult> GetReportAsync(System.DateTime? fromDate, System.DateTime? toDate, string visitType = "all")
        {
            var items = await _appointmentRepository.GetPatientBookedAppointmentsForReportAsync(fromDate, toDate, visitType).ConfigureAwait(false);
            _logger.Information("گزارش نوبت‌های رزرو بیماران: {Count} ردیف، نوع ویزیت: {VisitType}", items?.Count ?? 0, visitType ?? "all");
            return new PatientBookedAppointmentsReportResult
            {
                Items = items ?? new System.Collections.Generic.List<PatientBookedAppointmentReportItemDto>(),
                FromDate = fromDate,
                ToDate = toDate,
                VisitType = visitType ?? "all"
            };
        }
    }
}
