using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using ClinicApp.Interfaces.Notification;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Notification;
using ClinicApp.Models.Enums;
using Serilog;
// نوع اعلان نوبت (از Models.Enums) — از alias استفاده می‌شود تا با NotificationType در همین namespace (NotificationModule) تداخل نداشته باشد
using AppointmentNotificationType = ClinicApp.Models.Enums.NotificationType;

namespace ClinicApp.Services.Notification
{
    /// <summary>
    /// سرویس اعلان — فقط Enqueue بعد از Commit. ارسال توسط Background Job.
    /// Idempotency: هر ترکیب AppointmentId + AppointmentNotificationType + Channel فقط یک بار در صف قرار می‌گیرد.
    /// </summary>
    public class NotificationService : IAppointmentNotificationQueueService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationQueueRepository _queueRepository;
        private readonly ILogger _logger;

        private const string ClinicName = "کلینیک شفا";
        private const string ClinicAddress = "آدرس کلینیک شفا"; // قابل تنظیم از تنظیمات یا جدول Clinic

        public NotificationService(
            ApplicationDbContext context,
            INotificationQueueRepository queueRepository,
            ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
            _logger = logger?.ForContext<NotificationService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task EnqueueAppointmentBookingConfirmationAsync(int appointmentId)
        {
            await EnqueueAppointmentNotificationAsync(
                appointmentId,
                AppointmentNotificationType.AppointmentBookingConfirmation,
                GetBookingConfirmationTemplate(),
                scheduledTime: null);
        }

        public async Task EnqueuePaymentConfirmationAsync(int appointmentId)
        {
            await EnqueueAppointmentNotificationAsync(
                appointmentId,
                AppointmentNotificationType.PaymentConfirmation,
                GetPaymentConfirmationTemplate(),
                scheduledTime: null);
        }

        public async Task EnqueueAppointmentReminderAsync(int appointmentId, AppointmentNotificationType reminderType)
        {
            if (reminderType != AppointmentNotificationType.AppointmentReminder24h &&
                reminderType != AppointmentNotificationType.AppointmentReminder3h &&
                reminderType != AppointmentNotificationType.AppointmentReminder30min)
                return;

            var appointment = await GetAppointmentWithDetailsAsync(appointmentId);
            if (appointment?.PatientId == null || appointment.Patient?.PhoneNumber == null)
                return;

            var appointmentDate = appointment.AppointmentDate;
            DateTime scheduledTime;
            if (reminderType == AppointmentNotificationType.AppointmentReminder24h)
                scheduledTime = appointmentDate.AddHours(-24);
            else if (reminderType == AppointmentNotificationType.AppointmentReminder3h)
                scheduledTime = appointmentDate.AddHours(-3);
            else
                scheduledTime = appointmentDate.AddMinutes(-30);

            var variables = new Dictionary<string, string>
            {
                { "PatientName", GetPatientDisplayName(appointment.Patient) },
                { "DoctorName", appointment.Doctor?.FullName ?? "پزشک" },
                { "Date", appointment.AppointmentDate.ToPersianDate() },
                { "Time", TimeFormatHelper.FormatTimeToPersian(appointment.AppointmentDate.TimeOfDay) },
                { "Clinic", ClinicName },
                { "TrackingCode", GetTrackingCode(appointmentId) }
            };
            var message = NotificationTemplateEngine.Render(GetReminderTemplate(), variables);

            await EnqueueReminderAsync(appointmentId, appointment.Patient.ApplicationUserId, appointment.Patient.PhoneNumber, message, reminderType, scheduledTime);
        }

        private async Task EnqueueAppointmentNotificationAsync(
            int appointmentId,
            AppointmentNotificationType notificationType,
            string messageTemplate,
            DateTime? scheduledTime)
        {
            var appointment = await GetAppointmentWithDetailsAsync(appointmentId);
            if (appointment?.PatientId == null)
            {
                _logger.Warning("نوبت یا بیمار یافت نشد برای اعلان - AppointmentId: {AppointmentId}", appointmentId);
                return;
            }

            var variables = BuildTemplateVariables(appointment);
            var title = notificationType == AppointmentNotificationType.AppointmentBookingConfirmation ? "تأیید رزرو نوبت" : "تأیید پرداخت";
            var body = NotificationTemplateEngine.Render(messageTemplate, variables);

            var recipientPhone = appointment.Patient?.PhoneNumber;
            var recipientEmail = appointment.Patient?.Email;
            if (string.IsNullOrEmpty(recipientPhone) && string.IsNullOrEmpty(recipientEmail))
            {
                _logger.Warning("بیمار بدون شماره و ایمیل - AppointmentId: {AppointmentId}", appointmentId);
                return;
            }

            if (!string.IsNullOrEmpty(recipientPhone))
            {
                var keySms = $"A{appointmentId}_{notificationType}_Sms";
                if (!await _queueRepository.ExistsByIdempotencyKeyAsync(keySms, NotificationStatus.Queued, NotificationStatus.Sending, NotificationStatus.Sent))
                {
                    await _queueRepository.AddAsync(new NotificationQueueItem
                    {
                        AppointmentId = appointmentId,
                        PatientId = appointment.PatientId,
                        UserId = appointment.Patient?.ApplicationUserId,
                        NotificationType = notificationType,
                        Title = title,
                        Message = body,
                        Channel = NotificationChannelType.Sms,
                        Status = scheduledTime.HasValue ? NotificationStatus.Scheduled : NotificationStatus.Queued,
                        RetryCount = 0,
                        MaxRetries = 3,
                        ScheduledTime = scheduledTime,
                        IdempotencyKey = keySms,
                        Recipient = recipientPhone,
                        CreatedAt = DateTime.UtcNow
                    });
                    _logger.Information("اعلان در صف قرار گرفت - AppointmentId: {AppointmentId}, Type: {Type}, Channel: Sms", appointmentId, notificationType);
                }
            }

            if (!string.IsNullOrEmpty(recipientEmail))
            {
                var keyEmail = $"A{appointmentId}_{notificationType}_Email";
                if (!await _queueRepository.ExistsByIdempotencyKeyAsync(keyEmail, NotificationStatus.Queued, NotificationStatus.Sending, NotificationStatus.Sent))
                {
                    await _queueRepository.AddAsync(new NotificationQueueItem
                    {
                        AppointmentId = appointmentId,
                        PatientId = appointment.PatientId,
                        UserId = appointment.Patient?.ApplicationUserId,
                        NotificationType = notificationType,
                        Title = title,
                        Message = body,
                        Channel = NotificationChannelType.Email,
                        Status = scheduledTime.HasValue ? NotificationStatus.Scheduled : NotificationStatus.Queued,
                        RetryCount = 0,
                        MaxRetries = 3,
                        ScheduledTime = scheduledTime,
                        IdempotencyKey = keyEmail,
                        Recipient = recipientEmail,
                        Subject = title,
                        CreatedAt = DateTime.UtcNow
                    });
                    _logger.Information("اعلان در صف قرار گرفت - AppointmentId: {AppointmentId}, Type: {Type}, Channel: Email", appointmentId, notificationType);
                }
            }
        }

        private async Task EnqueueReminderAsync(int appointmentId, string userId, string phone, string message, AppointmentNotificationType reminderType, DateTime scheduledTimeUtc)
        {
            var key = $"A{appointmentId}_{reminderType}_Sms";
            if (await _queueRepository.ExistsByIdempotencyKeyAsync(key, NotificationStatus.Queued, NotificationStatus.Scheduled, NotificationStatus.Sending, NotificationStatus.Sent))
                return;

            await _queueRepository.AddAsync(new NotificationQueueItem
            {
                AppointmentId = appointmentId,
                NotificationType = reminderType,
                Title = "یادآوری نوبت پزشکی",
                Message = message,
                Channel = NotificationChannelType.Sms,
                Status = NotificationStatus.Scheduled,
                RetryCount = 0,
                MaxRetries = 3,
                ScheduledTime = scheduledTimeUtc,
                IdempotencyKey = key,
                Recipient = phone,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            _logger.Information("یادآوری در صف قرار گرفت - AppointmentId: {AppointmentId}, Type: {Type}, At: {At}", appointmentId, reminderType, scheduledTimeUtc);
        }

        private async Task<Models.Entities.Appointment.Appointment> GetAppointmentWithDetailsAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Patient.ApplicationUser)
                .Include(a => a.Doctor.Clinic)
                .Include(a => a.Doctor.DoctorSpecializations.Select(ds => ds.Specialization))
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);
        }

        private static Dictionary<string, string> BuildTemplateVariables(Models.Entities.Appointment.Appointment appointment)
        {
            var doctorName = appointment.Doctor?.FullName ?? "پزشک";
            var specialty = appointment.Doctor?.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "پزشک عمومی";
            var clinicName = appointment.Doctor?.Clinic?.Name ?? ClinicName;
            var address = appointment.Doctor?.Clinic?.Address ?? ClinicAddress;

            return new Dictionary<string, string>
            {
                { "PatientName", GetPatientDisplayName(appointment.Patient) },
                { "DoctorName", doctorName },
                { "Specialty", specialty },
                { "Date", appointment.AppointmentDate.ToPersianDate() },
                { "Time", TimeFormatHelper.FormatTimeToPersian(appointment.AppointmentDate.TimeOfDay) },
                { "Clinic", clinicName },
                { "ClinicAddress", address },
                { "TrackingCode", GetTrackingCode(appointment.AppointmentId) }
            };
        }

        private static string GetPatientDisplayName(Models.Entities.Patient.Patient patient)
        {
            if (patient == null) return "بیمار گرامی";
            if (!string.IsNullOrEmpty(patient.FirstName) || !string.IsNullOrEmpty(patient.LastName))
                return $"{patient.FirstName ?? ""} {patient.LastName ?? ""}".Trim();
            return patient.ApplicationUser?.UserName ?? "بیمار گرامی";
        }

        private static string GetTrackingCode(int appointmentId) => $"APT-{appointmentId}";

        private static string GetBookingConfirmationTemplate()
        {
            return "بیمار گرامی {{PatientName}}\n\nنوبت شما با موفقیت ثبت شد ✅\n\n👨‍⚕️ پزشک: {{DoctorName}}\n📅 تاریخ: {{Date}}\n⏰ ساعت: {{Time}}\n🏥 مرکز: {{Clinic}}\n\nکد پیگیری: {{TrackingCode}}\n\nلطفاً 10 دقیقه قبل از زمان مراجعه حضور داشته باشید.";
        }

        private static string GetPaymentConfirmationTemplate()
        {
            return "بیمار گرامی {{PatientName}}\n\nپرداخت نوبت شما با موفقیت انجام شد ✅\n\n👨‍⚕️ پزشک: {{DoctorName}}\n📅 تاریخ نوبت: {{Date}}\n⏰ ساعت: {{Time}}\n🏥 مرکز: {{Clinic}}\n\nکد پیگیری: {{TrackingCode}}\n\nکلینیک شفا";
        }

        private static string GetReminderTemplate()
        {
            return "یادآوری نوبت پزشکی ⏰\n\n{{PatientName}} عزیز\nشما در تاریخ {{Date}} ساعت {{Time}} با دکتر {{DoctorName}} نوبت دارید.\n\n📍 {{Clinic}}\n\nدر صورت عدم امکان حضور، لطفاً اطلاع دهید.";
        }
    }
}
