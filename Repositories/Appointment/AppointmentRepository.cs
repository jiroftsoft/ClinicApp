using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Models;
using AppointmentEntity = ClinicApp.Models.Entities.Appointment.Appointment;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Repositories.Appointment
{
    /// <summary>
    /// Repository برای مدیریت نوبت‌های پزشکی
    /// </summary>
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public AppointmentRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger?.ForContext<AppointmentRepository>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Models.Entities.Appointment.Appointment>> GetPatientAppointmentsAsync(
            int patientId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = _context.Appointments
                    .Where(a => a.PatientId == patientId && !a.IsDeleted)
                    .Include(a => a.Doctor)
                    .Include(a => a.Doctor.DoctorSpecializations.Select(ds => ds.Specialization))
                    .Include(a => a.PaymentTransaction)
                    .AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(a => a.AppointmentDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(a => a.AppointmentDate <= endDate.Value);
                }

                var appointments = await query
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.CreatedAt)
                    .ToListAsync();

                _logger.Information("دریافت {Count} نوبت برای بیمار {PatientId}", appointments.Count, patientId);
                return appointments;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های بیمار {PatientId}", patientId);
                throw;
            }
        }

        public async Task<AppointmentEntity> GetAppointmentByIdAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Doctor.DoctorSpecializations.Select(ds => ds.Specialization))
                    .Include(a => a.Patient)
                    .Include(a => a.PaymentTransaction)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

                if (appointment == null)
                {
                    _logger.Warning("نوبت با شناسه {AppointmentId} یافت نشد", appointmentId);
                }

                return appointment;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت {AppointmentId}", appointmentId);
                throw;
            }
        }

        public async Task<AppointmentEntity> CreateAppointmentAsync(AppointmentEntity appointment)
        {
            try
            {
                if (appointment == null)
                {
                    throw new ArgumentNullException(nameof(appointment));
                }

                appointment.CreatedAt = DateTime.Now;
                appointment.IsDeleted = false;

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _logger.Information("نوبت جدید با شناسه {AppointmentId} ایجاد شد - پزشک: {DoctorId}, بیمار: {PatientId}, تاریخ: {Date}",
                    appointment.AppointmentId, appointment.DoctorId, appointment.PatientId, appointment.AppointmentDate);

                return appointment;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد نوبت");
                throw;
            }
        }

        public async Task<bool> UpdateAppointmentStatusAsync(
            int appointmentId,
            AppointmentStatus status)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

                if (appointment == null)
                {
                    _logger.Warning("نوبت با شناسه {AppointmentId} برای به‌روزرسانی یافت نشد", appointmentId);
                    return false;
                }

                appointment.Status = status;
                appointment.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.Information("وضعیت نوبت {AppointmentId} به {Status} تغییر یافت", appointmentId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی وضعیت نوبت {AppointmentId}", appointmentId);
                throw;
            }
        }

        public async Task<bool> CheckSlotAvailabilityAsync(
            int doctorId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            try
            {
                var appointmentDateTime = appointmentDate.Date.Add(startTime);
                var appointmentEndDateTime = appointmentDate.Date.Add(endTime);

                // بررسی نوبت‌های موجود که با این بازه زمانی تداخل دارند
                var conflictingAppointment = await _context.Appointments
                    .AnyAsync(a =>
                        a.DoctorId == doctorId &&
                        !a.IsDeleted &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.AppointmentDate.Date == appointmentDate.Date &&
                        ((a.AppointmentDate >= appointmentDateTime && a.AppointmentDate < appointmentEndDateTime) ||
                         (a.AppointmentDate.AddMinutes(a.Duration) > appointmentDateTime && 
                          a.AppointmentDate.AddMinutes(a.Duration) <= appointmentEndDateTime) ||
                         (a.AppointmentDate <= appointmentDateTime && 
                          a.AppointmentDate.AddMinutes(a.Duration) >= appointmentEndDateTime)));

                var isAvailable = !conflictingAppointment;

                _logger.Information("بررسی دسترسی‌پذیری اسلات - پزشک: {DoctorId}, تاریخ: {Date}, زمان: {StartTime}-{EndTime}, در دسترس: {IsAvailable}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime, isAvailable);

                return isAvailable;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی‌پذیری اسلات - پزشک: {DoctorId}, تاریخ: {Date}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"));
                throw;
            }
        }

        public async Task<List<Models.Entities.Appointment.Appointment>> GetDoctorAppointmentsByDateAsync(
            int doctorId,
            DateTime date)
        {
            try
            {
                var appointments = await _context.Appointments
                    .Where(a =>
                        a.DoctorId == doctorId &&
                        !a.IsDeleted &&
                        a.AppointmentDate.Date == date.Date &&
                        a.Status != AppointmentStatus.Cancelled)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();

                _logger.Information("دریافت {Count} نوبت برای پزشک {DoctorId} در تاریخ {Date}",
                    appointments.Count, doctorId, date.ToString("yyyy/MM/dd"));

                return appointments;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های پزشک {DoctorId} در تاریخ {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                throw;
            }
        }
    }
}

