using System;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Models.Entities.Appointment;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Serilog;
using ClinicApp.Services;

namespace ClinicApp.Services.Appointment
{
    /// <summary>
    /// سرویس ارسال اعلان‌ها برای نوبت‌ها
    /// رعایت SRP: فقط ارسال اعلان‌ها
    /// </summary>
    public class AppointmentNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IIdentityMessageService _emailService;
        private readonly IIdentityMessageService _smsService;
        private readonly ILogger _logger;

        public AppointmentNotificationService(
            ApplicationDbContext context,
            IIdentityMessageService emailService,
            IIdentityMessageService smsService,
            ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger?.ForContext<AppointmentNotificationService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// ارسال اعلان رزرو موفق
        /// </summary>
        public async Task SendBookingConfirmationAsync(int appointmentId)
        {
            try
            {
                _logger.Information("ارسال اعلان رزرو موفق - AppointmentId: {AppointmentId}", appointmentId);

                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

                if (appointment == null || appointment.Patient == null)
                {
                    _logger.Warning("نوبت یا بیمار یافت نشد - AppointmentId: {AppointmentId}", appointmentId);
                    return;
                }

                var patient = appointment.Patient;
                var doctor = appointment.Doctor;

                // ارسال SMS
                if (!string.IsNullOrEmpty(patient.PhoneNumber))
                {
                    var smsMessage = new IdentityMessage
                    {
                        Destination = patient.PhoneNumber,
                        Body = $"✅ نوبت شما با موفقیت رزرو شد\n" +
                               $"👨‍⚕️ پزشک: {doctor?.FullName ?? "نامشخص"}\n" +
                               $"📅 تاریخ: {appointment.AppointmentDate.ToPersianDate()}\n" +
                               $"🕐 زمان: {TimeFormatHelper.FormatTimeToPersian(appointment.AppointmentDate.TimeOfDay)}\n" +
                               $"💰 مبلغ: {appointment.Price:N0} تومان\n" +
                               $"کلینیک درمانی شفا"
                    };

                    await _smsService.SendAsync(smsMessage);
                    _logger.Information("SMS رزرو موفق ارسال شد - AppointmentId: {AppointmentId}, Phone: {Phone}",
                        appointmentId, patient.PhoneNumber);
                }

                // ارسال Email (در صورت وجود)
                if (!string.IsNullOrEmpty(patient.Email))
                {
                    var emailMessage = new IdentityMessage
                    {
                        Destination = patient.Email,
                        Subject = "تایید رزرو نوبت - کلینیک درمانی شفا",
                        Body = $@"
                            <html dir='rtl'>
                            <body style='font-family: Tahoma, Arial;'>
                                <h2>نوبت شما با موفقیت رزرو شد</h2>
                                <p>سلام {patient.FirstName} {patient.LastName}</p>
                                <p>نوبت شما با مشخصات زیر رزرو شد:</p>
                                <ul>
                                    <li><strong>پزشک:</strong> {doctor?.FullName ?? "نامشخص"}</li>
                                    <li><strong>تاریخ:</strong> {appointment.AppointmentDate.ToPersianDate()}</li>
                                    <li><strong>زمان:</strong> {TimeFormatHelper.FormatTimeToPersian(appointment.AppointmentDate.TimeOfDay)}</li>
                                    <li><strong>مبلغ:</strong> {appointment.Price:N0} تومان</li>
                                </ul>
                                <p>لطفاً در زمان مقرر در کلینیک حضور داشته باشید.</p>
                                <p>با تشکر<br/>کلینیک درمانی شفا</p>
                            </body>
                            </html>"
                    };

                    await _emailService.SendAsync(emailMessage);
                    _logger.Information("Email رزرو موفق ارسال شد - AppointmentId: {AppointmentId}, Email: {Email}",
                        appointmentId, patient.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال اعلان رزرو موفق - AppointmentId: {AppointmentId}", appointmentId);
                // خطا را لاگ می‌کنیم اما exception را throw نمی‌کنیم تا فرآیند رزرو متوقف نشود
            }
        }

        /// <summary>
        /// ارسال اعلان پرداخت موفق
        /// </summary>
        public async Task SendPaymentConfirmationAsync(int appointmentId)
        {
            try
            {
                _logger.Information("ارسال اعلان پرداخت موفق - AppointmentId: {AppointmentId}", appointmentId);

                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

                if (appointment == null || appointment.Patient == null)
                {
                    _logger.Warning("نوبت یا بیمار یافت نشد - AppointmentId: {AppointmentId}", appointmentId);
                    return;
                }

                var patient = appointment.Patient;

                // ارسال SMS
                if (!string.IsNullOrEmpty(patient.PhoneNumber))
                {
                    var smsMessage = new IdentityMessage
                    {
                        Destination = patient.PhoneNumber,
                        Body = $"✅ پرداخت نوبت شما با موفقیت انجام شد\n" +
                               $"💰 مبلغ: {appointment.Price:N0} تومان\n" +
                               $"📅 تاریخ نوبت: {appointment.AppointmentDate.ToPersianDate()}\n" +
                               $"کلینیک درمانی شفا"
                    };

                    await _smsService.SendAsync(smsMessage);
                    _logger.Information("SMS پرداخت موفق ارسال شد - AppointmentId: {AppointmentId}", appointmentId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال اعلان پرداخت موفق - AppointmentId: {AppointmentId}", appointmentId);
            }
        }

        /// <summary>
        /// ارسال یادآوری نوبت (24 ساعت قبل)
        /// این متد باید توسط یک Background Job فراخوانی شود
        /// </summary>
        public async Task SendAppointmentReminderAsync(int appointmentId)
        {
            try
            {
                _logger.Information("ارسال یادآوری نوبت - AppointmentId: {AppointmentId}", appointmentId);

                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

                if (appointment == null || appointment.Patient == null)
                {
                    _logger.Warning("نوبت یا بیمار یافت نشد - AppointmentId: {AppointmentId}", appointmentId);
                    return;
                }

                // بررسی اینکه نوبت در 24 ساعت آینده است
                var timeUntilAppointment = appointment.AppointmentDate - DateTime.Now;
                if (timeUntilAppointment.TotalHours < 23 || timeUntilAppointment.TotalHours > 25)
                {
                    _logger.Information("نوبت در بازه زمانی مناسب برای یادآوری نیست - AppointmentId: {AppointmentId}, HoursUntil: {Hours}",
                        appointmentId, timeUntilAppointment.TotalHours);
                    return;
                }

                var patient = appointment.Patient;
                var doctor = appointment.Doctor;

                // ارسال SMS
                if (!string.IsNullOrEmpty(patient.PhoneNumber))
                {
                    var smsMessage = new IdentityMessage
                    {
                        Destination = patient.PhoneNumber,
                        Body = $"🔔 یادآوری نوبت\n" +
                               $"👨‍⚕️ پزشک: {doctor?.FullName ?? "نامشخص"}\n" +
                               $"📅 فردا: {appointment.AppointmentDate.ToPersianDate()}\n" +
                               $"🕐 ساعت: {TimeFormatHelper.FormatTimeToPersian(appointment.AppointmentDate.TimeOfDay)}\n" +
                               $"لطفاً در زمان مقرر حضور داشته باشید\n" +
                               $"کلینیک درمانی شفا"
                    };

                    await _smsService.SendAsync(smsMessage);
                    _logger.Information("SMS یادآوری نوبت ارسال شد - AppointmentId: {AppointmentId}", appointmentId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال یادآوری نوبت - AppointmentId: {AppointmentId}", appointmentId);
            }
        }
    }
}

