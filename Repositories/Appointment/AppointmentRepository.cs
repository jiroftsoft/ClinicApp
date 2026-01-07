using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Models;
using AppointmentEntity = ClinicApp.Models.Entities.Appointment.Appointment;
using ClinicApp.Models.Enums;
using ClinicApp.Models.Entities.Doctor; // ✅ برای DoctorTimeSlot
using Serilog;
using ClinicApp.Infrastructure; // ✅ برای ITimeProvider

namespace ClinicApp.Repositories.Appointment
{
    /// <summary>
    /// Repository برای مدیریت نوبت‌های پزشکی
    /// </summary>
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ITimeProvider _timeProvider; // ✅ ENTERPRISE-GRADE: برای مدیریت زمان ایران

        public AppointmentRepository(ApplicationDbContext context, ITimeProvider timeProvider, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
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

                appointment.CreatedAt = _timeProvider.UtcNow; // ✅ UTC برای timestamp
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
                appointment.UpdatedAt = _timeProvider.UtcNow; // ✅ UTC برای timestamp

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

        /// <summary>
        /// ✅ CRITICAL FIX: بررسی دسترسی‌پذیری اسلات با UPDLOCK برای جلوگیری از Race Condition
        /// استفاده از Raw SQL با UPDLOCK برای pessimistic locking در SQL Server
        /// ✅ بهبود: بررسی هم DoctorTimeSlot و هم Appointments
        /// </summary>
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

                // ✅ STEP 1: بررسی وجود اسلات در DoctorTimeSlots با Status = Available
                // ⚠️ NOTE: اگر slot در DoctorTimeSlots وجود نداشته باشد، ممکن است از Schedule تولید شده باشد
                // در این صورت، Service Layer باید از GetAvailableTimeSlotsAsync استفاده کند
                // این بررسی فقط برای slot‌هایی است که در DoctorTimeSlots ذخیره شده‌اند
                // ✅ CRITICAL FIX: استفاده از DbFunctions.TruncateTime به جای .Date (LINQ to Entities)
                var slotExists = await _context.DoctorTimeSlots
                    .AnyAsync(ts => ts.DoctorId == doctorId &&
                                   DbFunctions.TruncateTime(ts.AppointmentDate) == DbFunctions.TruncateTime(appointmentDate) &&
                                   ts.StartTime == startTime &&
                                   ts.EndTime == endTime &&
                                   ts.Status == AppointmentStatus.Available &&
                                   !ts.IsDeleted);

                // ⚠️ CRITICAL FIX: اگر slot در DoctorTimeSlots وجود نداشت، false برمی‌گردانیم
                // اما Service Layer باید از GetAvailableTimeSlotsAsync استفاده کند تا slot‌های تولید شده از Schedule را هم بررسی کند
                if (!slotExists)
                {
                    _logger.Debug("⚠️ SLOT NOT FOUND IN DOCTORTIMESLOTS: اسلات {DoctorId}/{Date}/{StartTime}-{EndTime} در DoctorTimeSlots یافت نشد (ممکن است از Schedule تولید شده باشد)",
                        doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime);
                    return false;
                }

                // ✅ STEP 2: بررسی overlap با Appointments موجود (با UPDLOCK برای Race Condition Prevention)
                // این باعث می‌شود که ردیف‌های مربوطه lock شوند تا Race Condition رخ ندهد
                var sql = @"
                    SELECT COUNT(*) 
                    FROM Appointments WITH (UPDLOCK, ROWLOCK)
                    WHERE DoctorId = @p0
                      AND IsDeleted = 0
                      AND Status != @p1
                      AND CAST(AppointmentDate AS DATE) = CAST(@p2 AS DATE)
                      AND (
                          (AppointmentDate >= @p3 AND AppointmentDate < @p4) OR
                          (DATEADD(MINUTE, Duration, AppointmentDate) > @p3 AND DATEADD(MINUTE, Duration, AppointmentDate) <= @p4) OR
                          (AppointmentDate <= @p3 AND DATEADD(MINUTE, Duration, AppointmentDate) >= @p4)
                      )";

                var count = await _context.Database.SqlQuery<int>(sql,
                    new System.Data.SqlClient.SqlParameter("@p0", doctorId),
                    new System.Data.SqlClient.SqlParameter("@p1", (int)AppointmentStatus.Cancelled),
                    new System.Data.SqlClient.SqlParameter("@p2", appointmentDate.Date),
                    new System.Data.SqlClient.SqlParameter("@p3", appointmentDateTime),
                    new System.Data.SqlClient.SqlParameter("@p4", appointmentEndDateTime)
                ).FirstOrDefaultAsync();

                var isAvailable = count == 0;

                _logger.Information("✅ بررسی دسترسی‌پذیری اسلات (با UPDLOCK) - پزشک: {DoctorId}, تاریخ: {Date}, زمان: {StartTime}-{EndTime}, SlotExists: {SlotExists}, OverlapCount: {Count}, در دسترس: {IsAvailable}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime, slotExists, count, isAvailable);

                return isAvailable;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بررسی دسترسی‌پذیری اسلات - پزشک: {DoctorId}, تاریخ: {Date}, زمان: {StartTime}-{EndTime}, ExceptionType: {ExceptionType}, Message: {Message}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime, ex.GetType().Name, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// ✅ ENTERPRISE-GRADE: دریافت نوبت‌های پزشک در یک تاریخ خاص
        /// فقط نوبت‌های فعال (Scheduled, Pending) را برمی‌گرداند
        /// </summary>
        public async Task<List<Models.Entities.Appointment.Appointment>> GetDoctorAppointmentsByDateAsync(
            int doctorId,
            DateTime date)
        {
            try
            {
                // ✅ CRITICAL FIX: فیلتر کردن همه Status‌های غیرفعال (نه فقط Cancelled)
                // فقط Scheduled و Pending را در نظر می‌گیریم
                // ✅ CRITICAL FIX: فیلتر کردن نوبت‌های Pending منقضی شده
                // نوبت‌های Pending که PendingExpiresAt آن‌ها گذشته است، نباید در نظر گرفته شوند
                var now = DateTime.UtcNow;
                var appointments = await _context.Appointments
                    .Where(a =>
                        a.DoctorId == doctorId &&
                        !a.IsDeleted &&
                        DbFunctions.TruncateTime(a.AppointmentDate) == DbFunctions.TruncateTime(date) &&
                        (a.Status == AppointmentStatus.Scheduled || 
                         (a.Status == AppointmentStatus.Pending && 
                          (a.PendingExpiresAt == null || a.PendingExpiresAt > now)))) // ✅ فیلتر نوبت‌های منقضی شده
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();

                _logger.Information("دریافت {Count} نوبت فعال برای پزشک {DoctorId} در تاریخ {Date}",
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

        /// <summary>
        /// ✅ CRITICAL FIX: بررسی تداخل نوبت‌های بیمار با Locking برای جلوگیری از Race Condition
        /// استفاده از UPDLOCK برای pessimistic locking در SQL Server
        /// </summary>
        public async Task<bool> HasOverlappingPatientAppointmentAsync(
            int patientId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            try
            {
                var appointmentDateTime = appointmentDate.Date.Add(startTime);
                var appointmentEndDateTime = appointmentDate.Date.Add(endTime);

                // ✅ CRITICAL FIX: منطق Overlap صحیح (فرمول استاندارد)
                // دو بازه زمانی A و B overlap دارند اگر و فقط اگر:
                // (A.Start < B.End) AND (A.End > B.Start)
                // 
                // در اینجا:
                // - A = نوبت قبلی (AppointmentDate تا AppointmentDate + Duration)
                // - B = نوبت جدید (@p3 تا @p4)
                // 
                // ⚠️ NOTE: استفاده از < و > (نه <= و >=) برای جلوگیری از overlap نوبت‌های مجاور
                // مثال: نوبت 10:00-10:15 و 10:15-10:30 overlap ندارند
                // 
                // ✅ CRITICAL: فقط نوبت‌های فعال (Scheduled, Pending) را در نظر بگیریم
                // نوبت‌های Completed, NoShow, Cancelled نباید در double booking check لحاظ شوند
                // ✅ CRITICAL FIX: نوبت‌های Pending منقضی شده نیز نباید در نظر گرفته شوند
                // ✅ CRITICAL: استفاده از Raw SQL با UPDLOCK برای pessimistic locking
                // این باعث می‌شود که ردیف‌های مربوطه lock شوند تا Race Condition رخ ندهد
                var now = DateTime.UtcNow;
                var sql = @"
                    SELECT COUNT(*) 
                    FROM Appointments WITH (UPDLOCK, ROWLOCK)
                    WHERE PatientId = @p0
                      AND IsDeleted = 0
                      AND (
                          Status = @p1  -- Scheduled
                          OR (Status = @p2 AND (PendingExpiresAt IS NULL OR PendingExpiresAt > @p6))  -- Pending که منقضی نشده
                      )
                      AND CAST(AppointmentDate AS DATE) = CAST(@p3 AS DATE)
                      AND AppointmentDate < @p5
                      AND DATEADD(MINUTE, Duration, AppointmentDate) > @p4";

                // ✅ CRITICAL FIX: استفاده از متغیر now که قبلاً تعریف شده است (خط 299)
                var count = await _context.Database.SqlQuery<int>(sql,
                    new System.Data.SqlClient.SqlParameter("@p0", patientId),
                    new System.Data.SqlClient.SqlParameter("@p1", (int)AppointmentStatus.Scheduled),
                    new System.Data.SqlClient.SqlParameter("@p2", (int)AppointmentStatus.Pending),
                    new System.Data.SqlClient.SqlParameter("@p3", appointmentDate.Date),
                    new System.Data.SqlClient.SqlParameter("@p4", appointmentDateTime),
                    new System.Data.SqlClient.SqlParameter("@p5", appointmentEndDateTime),
                    new System.Data.SqlClient.SqlParameter("@p6", now) // ✅ CRITICAL FIX: زمان فعلی برای چک Expiration
                ).FirstOrDefaultAsync();

                var hasOverlap = count > 0;

                // ✅ CRITICAL FIX: Logging دقیق‌تر برای debugging
                if (hasOverlap)
                {
                    // ✅ CRITICAL FIX: دریافت نوبت‌های overlap برای logging (با فیلتر Expiration)
                    var overlappingAppointments = await _context.Database.SqlQuery<dynamic>(@"
                        SELECT AppointmentId, DoctorId, AppointmentDate, Duration, Status, PendingExpiresAt
                        FROM Appointments
                        WHERE PatientId = @p0
                          AND IsDeleted = 0
                          AND (
                              Status = @p1  -- Scheduled
                              OR (Status = @p2 AND (PendingExpiresAt IS NULL OR PendingExpiresAt > @p6))  -- Pending که منقضی نشده
                          )
                          AND CAST(AppointmentDate AS DATE) = CAST(@p3 AS DATE)
                          AND AppointmentDate < @p5
                          AND DATEADD(MINUTE, Duration, AppointmentDate) > @p4",
                        new System.Data.SqlClient.SqlParameter("@p0", patientId),
                        new System.Data.SqlClient.SqlParameter("@p1", (int)AppointmentStatus.Scheduled),
                        new System.Data.SqlClient.SqlParameter("@p2", (int)AppointmentStatus.Pending),
                        new System.Data.SqlClient.SqlParameter("@p3", appointmentDate.Date),
                        new System.Data.SqlClient.SqlParameter("@p4", appointmentDateTime),
                        new System.Data.SqlClient.SqlParameter("@p5", appointmentEndDateTime),
                        new System.Data.SqlClient.SqlParameter("@p6", now) // ✅ CRITICAL FIX: زمان فعلی برای چک Expiration
                    ).ToListAsync();

                    // ✅ CRITICAL FIX: Logging با جزئیات کامل برای debugging
                    var overlapDetails = overlappingAppointments.Select(a => 
                    {
                        var existingStart = ((DateTime)a.AppointmentDate);
                        var existingEnd = existingStart.AddMinutes((int)a.Duration);
                        return $"AppointmentId={a.AppointmentId}, DoctorId={a.DoctorId}, ExistingTime={existingStart:HH:mm}-{existingEnd:HH:mm}, Duration={a.Duration}min, Status={a.Status}";
                    }).ToList();

                    _logger.Warning("⚠️ DOUBLE BOOKING DETECTED: بیمار {PatientId} در تاریخ {Date} زمان {StartTime}-{EndTime} با {Count} نوبت overlap دارد. نوبت جدید: {NewStartTime}-{NewEndTime}. نوبت‌های overlap: {OverlappingAppointments}",
                        patientId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime, count,
                        $"{appointmentDateTime:HH:mm}-{appointmentEndDateTime:HH:mm}",
                        string.Join(" | ", overlapDetails));
                }
                else
                {
                    _logger.Debug("✅ NO OVERLAP: بیمار {PatientId} در تاریخ {Date} زمان {StartTime}-{EndTime} (NewTime: {NewStartTime}-{NewEndTime}) هیچ overlap ندارد",
                        patientId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime,
                        $"{appointmentDateTime:HH:mm}-{appointmentEndDateTime:HH:mm}");
                }

                return hasOverlap;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی تداخل نوبت‌های بیمار - PatientId: {PatientId}, تاریخ: {Date}",
                    patientId, appointmentDate.ToString("yyyy/MM/dd"));
                throw;
            }
        }
    }
}

