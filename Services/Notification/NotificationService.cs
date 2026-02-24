using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using ClinicApp.Interfaces;
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
        private readonly IAppSettings _appSettings;
        private readonly ILogger _logger;

        private const string ClinicName = "کلینیک شفا";
        private const string ClinicAddress = "آدرس کلینیک شفا"; // قابل تنظیم از تنظیمات یا جدول Clinic

        public NotificationService(
            ApplicationDbContext context,
            INotificationQueueRepository queueRepository,
            IAppSettings appSettings,
            ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
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

        /// <summary>
        /// بعد از پرداخت موفق نوبت مشاوره آنلاین — ارسال SMS به پزشک با لینک ورود به اتاق.
        /// فقط وقتی ماژول فعال و نوبت IsOnlineConsultation و پزشک دارای شماره تلفن است.
        /// </summary>
        public async Task EnqueueOnlineConsultationRequestToDoctorAsync(int appointmentId)
        {
            if (!_appSettings.EnableOnlineConsultation)
            {
                _logger.Debug("EnqueueOnlineConsultationRequestToDoctor: ماژول غیرفعال - AppointmentId: {AppointmentId}", appointmentId);
                return;
            }
            var appointment = await GetAppointmentWithDetailsAsync(appointmentId);
            if (appointment == null || !appointment.IsOnlineConsultation)
            {
                _logger.Debug("EnqueueOnlineConsultationRequestToDoctor: نوبت یافت نشد یا مشاوره آنلاین نیست - AppointmentId: {AppointmentId}", appointmentId);
                return;
            }

            var doctorPhone = appointment.Doctor?.PhoneNumber;
            if (string.IsNullOrWhiteSpace(doctorPhone))
            {
                _logger.Warning("EnqueueOnlineConsultationRequestToDoctor: پزشک بدون شماره تلفن - AppointmentId: {AppointmentId}, DoctorId: {DoctorId}", appointmentId, appointment.DoctorId);
                return;
            }

            var baseUrl = _appSettings.PaymentBaseUrl?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                _logger.Warning("EnqueueOnlineConsultationRequestToDoctor: PaymentBaseUrl تنظیم نشده - لینک ورود در SMS قرار نمی‌گیرد");
            }

            var joinUrl = string.IsNullOrWhiteSpace(baseUrl)
                ? $"/Admin/OnlineConsultation/Join/{appointmentId}"
                : $"{baseUrl}/Admin/OnlineConsultation/Join/{appointmentId}";
            var patientName = GetPatientDisplayName(appointment.Patient);
            var message = $"درخواست مشاوره آنلاین از {patientName}. لینک ورود به اتاق: {joinUrl}";

            var key = $"A{appointmentId}_{AppointmentNotificationType.OnlineConsultationRequestToDoctor}_Sms";
            if (await _queueRepository.ExistsByIdempotencyKeyAsync(key, NotificationStatus.Queued, NotificationStatus.Sending, NotificationStatus.Sent))
            {
                _logger.Debug("EnqueueOnlineConsultationRequestToDoctor: اعلان قبلاً در صف - IdempotencyKey: {Key}", key);
                return;
            }

            await _queueRepository.AddAsync(new NotificationQueueItem
            {
                AppointmentId = appointmentId,
                UserId = null,
                PatientId = null,
                NotificationType = AppointmentNotificationType.OnlineConsultationRequestToDoctor,
                Title = "درخواست مشاوره آنلاین",
                Message = message,
                Channel = NotificationChannelType.Sms,
                Status = NotificationStatus.Queued,
                RetryCount = 0,
                MaxRetries = 3,
                ScheduledTime = null,
                IdempotencyKey = key,
                Recipient = doctorPhone,
                CreatedAt = DateTime.UtcNow
            });
            _logger.Information("اعلان مشاوره آنلاین به پزشک در صف قرار گرفت - AppointmentId: {AppointmentId}, DoctorId: {DoctorId}", appointmentId, appointment.DoctorId);
        }

        /// <summary>
        /// بعد از پرداخت موفق نوبت مشاوره آنلاین — ارسال SMS به بیمار با لینک ورود به اتاق.
        /// </summary>
        public async Task EnqueueOnlineConsultationRequestToPatientAsync(int appointmentId)
        {
            if (!_appSettings.EnableOnlineConsultation)
            {
                _logger.Debug("EnqueueOnlineConsultationRequestToPatient: ماژول غیرفعال - AppointmentId: {AppointmentId}", appointmentId);
                return;
            }
            var appointment = await GetAppointmentWithDetailsAsync(appointmentId);
            if (appointment == null || !appointment.IsOnlineConsultation || appointment.PatientId == null)
            {
                _logger.Debug("EnqueueOnlineConsultationRequestToPatient: نوبت یافت نشد یا مشاوره آنلاین نیست - AppointmentId: {AppointmentId}", appointmentId);
                return;
            }

            var patientPhone = appointment.Patient?.PhoneNumber;
            if (string.IsNullOrWhiteSpace(patientPhone))
            {
                _logger.Warning("EnqueueOnlineConsultationRequestToPatient: بیمار بدون شماره تلفن - AppointmentId: {AppointmentId}", appointmentId);
                return;
            }

            var baseUrl = _appSettings.PaymentBaseUrl?.TrimEnd('/');
            var joinUrl = string.IsNullOrWhiteSpace(baseUrl)
                ? $"/Patient/Consultation/Join/{appointmentId}"
                : $"{baseUrl}/Patient/Consultation/Join/{appointmentId}";
            var doctorName = appointment.Doctor?.FullName ?? "پزشک";
            var message = $"پرداخت نوبت مشاوره آنلاین شما با دکتر {doctorName} با موفقیت انجام شد. لینک ورود به اتاق: {joinUrl}";

            var key = $"A{appointmentId}_{AppointmentNotificationType.OnlineConsultationRequestToPatient}_Sms";
            if (await _queueRepository.ExistsByIdempotencyKeyAsync(key, NotificationStatus.Queued, NotificationStatus.Sending, NotificationStatus.Sent))
            {
                _logger.Debug("EnqueueOnlineConsultationRequestToPatient: اعلان قبلاً در صف - IdempotencyKey: {Key}", key);
                return;
            }

            await _queueRepository.AddAsync(new NotificationQueueItem
            {
                AppointmentId = appointmentId,
                UserId = appointment.Patient?.ApplicationUserId,
                PatientId = appointment.PatientId,
                NotificationType = AppointmentNotificationType.OnlineConsultationRequestToPatient,
                Title = "نوبت مشاوره آنلاین",
                Message = message,
                Channel = NotificationChannelType.Sms,
                Status = NotificationStatus.Queued,
                RetryCount = 0,
                MaxRetries = 3,
                ScheduledTime = null,
                IdempotencyKey = key,
                Recipient = patientPhone,
                CreatedAt = DateTime.UtcNow
            });
            _logger.Information("اعلان مشاوره آنلاین به بیمار در صف قرار گرفت - AppointmentId: {AppointmentId}", appointmentId);
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
