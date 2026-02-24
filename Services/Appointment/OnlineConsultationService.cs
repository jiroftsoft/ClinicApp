using System;
using System.Threading.Tasks;
using ClinicApp.Infrastructure;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Models.Entities.Appointment;
using ClinicApp.ViewModels.OnlineConsultation;
using Serilog;

namespace ClinicApp.Services.Appointment
{
    /// <summary>
    /// سرویس مشاوره آنلاین تصویری (اتاق Jitsi) — آماده پروداکشن درمانی.
    /// اعتبارسنجی ورودی، بازه زمانی ورود، Feature Flag و لاگ بدون افشای اطلاعات حساس.
    /// </summary>
    public class OnlineConsultationService : IOnlineConsultationService
    {
        private const string RoomNamePrefix = "ClinicApp-Consult";
        private const string DefaultJitsiBaseUrl = "https://meet.jit.si";
        private const int RoomNameGuidLength = 8;

        private readonly IOnlineConsultationRoomRepository _roomRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppSettings _appSettings;
        private readonly ITimeProvider _timeProvider;
        private readonly ILogger _logger;

        public OnlineConsultationService(
            IOnlineConsultationRoomRepository roomRepository,
            IAppointmentRepository appointmentRepository,
            IAppSettings appSettings,
            ITimeProvider timeProvider,
            ILogger logger)
        {
            _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _logger = logger?.ForContext<OnlineConsultationService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<JoinConsultationViewModel> GetOrCreateRoomForPatientAsync(int appointmentId, int patientId)
        {
            const string msgNotAvailable = "اتاق مشاوره در حال حاضر در دسترس نیست. در بازه زمانی نوبت مراجعه کنید یا با پشتیبانی تماس بگیرید.";
            const string msgNotFound = "نوبت یافت نشد یا امکان ورود به این نوبت وجود ندارد.";
            const string msgNotAuthorized = "دسترسی به این نوبت برای شما مجاز نیست.";

            string notAllowedReason = null;
            if (!IsValidAppointmentId(appointmentId))
            {
                _logger.Warning("OnlineConsultation: شناسه نوبت نامعتبر - AppointmentId: {AppointmentId}", appointmentId);
                return BuildErrorModel(appointmentId, msgNotFound);
            }
            if (!CheckFeatureAndTimeWindow(out notAllowedReason))
            {
                if (!string.IsNullOrEmpty(notAllowedReason))
                    _logger.Information("OnlineConsultation: درخواست ورود بیمار رد شد - Reason: {Reason}, AppointmentId: {AppointmentId}", notAllowedReason, appointmentId);
                return BuildErrorModel(appointmentId, msgNotAvailable);
            }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
            if (appointment == null || appointment.IsDeleted)
            {
                _logger.Warning("OnlineConsultation: نوبت یافت نشد یا حذف شده - AppointmentId: {AppointmentId}", appointmentId);
                return BuildErrorModel(appointmentId, msgNotFound);
            }
            if (!appointment.IsOnlineConsultation)
            {
                _logger.Warning("OnlineConsultation: نوبت از نوع مشاوره آنلاین نیست - AppointmentId: {AppointmentId}", appointmentId);
                return BuildErrorModel(appointmentId, msgNotAvailable);
            }
            if (appointment.PatientId != patientId)
            {
                _logger.Warning("OnlineConsultation: دسترسی غیرمجاز بیمار به نوبت - AppointmentId: {AppointmentId}", appointmentId);
                return BuildErrorModel(appointmentId, msgNotAuthorized);
            }

            if (!IsWithinJoinWindow(appointment.AppointmentDate, out var windowReason))
            {
                _logger.Information("OnlineConsultation: خارج از بازه مجاز ورود - AppointmentId: {AppointmentId}, Reason: {Reason}", appointmentId, windowReason);
                return BuildErrorModel(appointmentId, msgNotAvailable);
            }

            var roomName = GenerateRoomName(appointmentId);
            await _roomRepository.GetOrCreateForAppointmentAsync(appointmentId, roomName, null);

            var vm = BuildViewModel(appointmentId, roomName, appointment);
            vm.CanJoin = true;
            return vm;
        }

        public async Task<JoinConsultationViewModel> GetOrCreateRoomForDoctorAsync(int appointmentId, int doctorId)
        {
            const string msgNotAvailable = "اتاق مشاوره در حال حاضر در دسترس نیست. در بازه زمانی نوبت مراجعه کنید.";
            const string msgNotFound = "نوبت یافت نشد یا امکان ورود به این نوبت وجود ندارد.";
            const string msgNotAuthorized = "دسترسی به این نوبت برای شما مجاز نیست.";

            string notAllowedReason = null;
            if (!IsValidAppointmentId(appointmentId))
                return BuildErrorModel(appointmentId, msgNotFound);
            if (!CheckFeatureAndTimeWindow(out notAllowedReason))
            {
                if (!string.IsNullOrEmpty(notAllowedReason))
                    _logger.Information("OnlineConsultation: درخواست ورود پزشک رد شد - Reason: {Reason}, AppointmentId: {AppointmentId}", notAllowedReason, appointmentId);
                return BuildErrorModel(appointmentId, msgNotAvailable);
            }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
            if (appointment == null || appointment.IsDeleted)
            {
                _logger.Warning("OnlineConsultation: نوبت یافت نشد یا حذف شده - AppointmentId: {AppointmentId}", appointmentId);
                return BuildErrorModel(appointmentId, msgNotFound);
            }
            if (!appointment.IsOnlineConsultation)
                return BuildErrorModel(appointmentId, msgNotAvailable);
            if (appointment.DoctorId != doctorId)
            {
                _logger.Warning("OnlineConsultation: دسترسی غیرمجاز پزشک به نوبت - AppointmentId: {AppointmentId}", appointmentId);
                return BuildErrorModel(appointmentId, msgNotAuthorized);
            }

            if (!IsWithinJoinWindow(appointment.AppointmentDate, out var windowReason))
            {
                _logger.Information("OnlineConsultation: خارج از بازه مجاز ورود - AppointmentId: {AppointmentId}, Reason: {Reason}", appointmentId, windowReason);
                return BuildErrorModel(appointmentId, msgNotAvailable);
            }

            var roomName = GenerateRoomName(appointmentId);
            await _roomRepository.GetOrCreateForAppointmentAsync(appointmentId, roomName, null);

            var vm = BuildViewModel(appointmentId, roomName, appointment);
            vm.CanJoin = true;
            return vm;
        }

        private static bool IsValidAppointmentId(int appointmentId)
        {
            return appointmentId > 0 && appointmentId <= int.MaxValue;
        }

        private bool CheckFeatureAndTimeWindow(out string reason)
        {
            reason = null;
            if (!_appSettings.EnableOnlineConsultation)
            {
                reason = "ModuleDisabled";
                return false;
            }
            return true;
        }

        private bool IsWithinJoinWindow(DateTime appointmentDate, out string reason)
        {
            reason = null;
            var nowUtc = _timeProvider.UtcNow;
            DateTime windowStartUtc;
            DateTime windowEndUtc;

            // اگر فقط تاریخ ذخیره شده (زمان 00:00) باشد، کل آن روز در ایران مجاز است
            var isDateOnly = appointmentDate.TimeOfDay == TimeSpan.Zero;
            if (isDateOnly)
            {
                var dateOnly = appointmentDate.Date;
                var startOfDayIran = dateOnly; // 00:00
                var endOfDayIran = dateOnly.AddDays(1).AddTicks(-1); // 23:59:59.999
                windowStartUtc = _timeProvider.FromIranTime(startOfDayIran);
                windowEndUtc = _timeProvider.FromIranTime(endOfDayIran);
            }
            else
            {
                var appointmentUtc = appointmentDate.Kind == DateTimeKind.Utc
                    ? appointmentDate
                    : _timeProvider.FromIranTime(appointmentDate);
                windowStartUtc = appointmentUtc.AddMinutes(-_appSettings.OnlineConsultationJoinAllowedMinutesBefore);
                windowEndUtc = appointmentUtc.AddMinutes(_appSettings.OnlineConsultationJoinAllowedMinutesAfter);
            }

            if (nowUtc < windowStartUtc)
            {
                reason = "BeforeWindow";
                return false;
            }
            if (nowUtc > windowEndUtc)
            {
                reason = "AfterWindow";
                return false;
            }
            return true;
        }

        private static string GenerateRoomName(int appointmentId)
        {
            var guid = Guid.NewGuid().ToString("N").Substring(0, RoomNameGuidLength);
            return $"{RoomNamePrefix}-{appointmentId}-{guid}";
        }

        private static JoinConsultationViewModel BuildErrorModel(int appointmentId, string userMessage)
        {
            return new JoinConsultationViewModel
            {
                AppointmentId = appointmentId,
                RoomName = null,
                JitsiBaseUrl = null,
                PatientName = null,
                DoctorName = null,
                CanJoin = false,
                UserMessage = userMessage ?? "اتاق مشاوره در دسترس نیست."
            };
        }

        private JoinConsultationViewModel BuildViewModel(int appointmentId, string roomName, Models.Entities.Appointment.Appointment appointment)
        {
            var patientName = appointment.Patient != null
                ? appointment.Patient.FullName
                : (string.IsNullOrWhiteSpace(appointment.PatientName) ? "بیمار" : appointment.PatientName);
            var doctorName = appointment.Doctor != null ? appointment.Doctor.FullName : "پزشک";
            var baseUrl = _appSettings.JitsiBaseUrl ?? DefaultJitsiBaseUrl;
            if (!string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = baseUrl.TrimEnd('/');
            return new JoinConsultationViewModel
            {
                AppointmentId = appointmentId,
                RoomName = roomName ?? "",
                JitsiBaseUrl = baseUrl ?? DefaultJitsiBaseUrl,
                PatientName = patientName ?? "بیمار",
                DoctorName = doctorName ?? "پزشک",
                CanJoin = true,
                UserMessage = null
            };
        }
    }
}
