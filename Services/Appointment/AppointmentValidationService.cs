using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.Models.Enums;
using Serilog;
using ClinicApp.Infrastructure; // ✅ برای ITimeProvider

namespace ClinicApp.Services.Appointment
{
    /// <summary>
    /// سرویس اعتبارسنجی پیشرفته برای رزرو نوبت
    /// رعایت SRP: فقط اعتبارسنجی
    /// </summary>
    public class AppointmentValidationService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ILogger _logger;
        private readonly ITimeProvider _timeProvider; // ✅ ENTERPRISE-GRADE: برای مدیریت زمان ایران

        public AppointmentValidationService(
            IAppointmentRepository appointmentRepository,
            IDoctorScheduleRepository doctorScheduleRepository,
            IDoctorCrudService doctorCrudService,
            ITimeProvider timeProvider, // ✅ ENTERPRISE-GRADE: برای مدیریت زمان ایران
            ILogger logger)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _logger = logger?.ForContext<AppointmentValidationService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// اعتبارسنجی کامل درخواست رزرو
        /// </summary>
        public async Task<ValidationResult> ValidateBookingRequestAsync(AppointmentBookingRequestDto request)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                // 1. اعتبارسنجی پایه
                var basicValidation = ValidateBasicFields(request);
                errors.AddRange(basicValidation.Errors);
                warnings.AddRange(basicValidation.Warnings);

                if (errors.Any())
                {
                    return ValidationResult.Failed(errors, warnings);
                }

                // 2. بررسی وجود پزشک
                var doctorValidation = await ValidateDoctorAsync(request.DoctorId);
                errors.AddRange(doctorValidation.Errors);
                warnings.AddRange(doctorValidation.Warnings);

                // 3. بررسی برنامه کاری پزشک
                var scheduleValidation = await ValidateDoctorScheduleAsync(request.DoctorId, request.AppointmentDate);
                errors.AddRange(scheduleValidation.Errors);
                warnings.AddRange(scheduleValidation.Warnings);

                // 4. بررسی دسترسی‌پذیری اسلات
                var availabilityValidation = await ValidateSlotAvailabilityAsync(
                    request.DoctorId, request.AppointmentDate, request.StartTime, request.EndTime);
                errors.AddRange(availabilityValidation.Errors);
                warnings.AddRange(availabilityValidation.Warnings);

                // 5. بررسی حداقل زمان رزرو
                var timeValidation = ValidateBookingTime(request.AppointmentDate, request.StartTime);
                errors.AddRange(timeValidation.Errors);
                warnings.AddRange(timeValidation.Warnings);

                // 6. بررسی تداخل با نوبت‌های دیگر بیمار
                var conflictValidation = await ValidatePatientConflictAsync(
                    request.PatientId, request.DoctorId, request.AppointmentDate, request.StartTime, request.EndTime);
                errors.AddRange(conflictValidation.Errors);
                warnings.AddRange(conflictValidation.Warnings);

                // 7. بررسی محدودیت‌های پزشک (ظرفیت روزانه)
                var capacityValidation = await ValidateDoctorCapacityAsync(
                    request.DoctorId, request.AppointmentDate);
                errors.AddRange(capacityValidation.Errors);
                warnings.AddRange(capacityValidation.Warnings);

                if (errors.Any())
                {
                    return ValidationResult.Failed(errors, warnings);
                }

                return ValidationResult.Successful(warnings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی درخواست رزرو");
                errors.Add("خطا در اعتبارسنجی. لطفاً دوباره تلاش کنید");
                return ValidationResult.Failed(errors, warnings);
            }
        }

        #region Private Validation Methods

        private ValidationResult ValidateBasicFields(AppointmentBookingRequestDto request)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (request == null)
            {
                errors.Add("اطلاعات رزرو نامعتبر است");
                return ValidationResult.Failed(errors);
            }

            if (request.DoctorId <= 0)
            {
                errors.Add("شناسه پزشک نامعتبر است");
            }

            if (request.PatientId <= 0)
            {
                errors.Add("شناسه بیمار نامعتبر است");
            }

            if (request.AppointmentDate == default(DateTime))
            {
                errors.Add("تاریخ نوبت الزامی است");
            }

            if (request.StartTime == default(TimeSpan))
            {
                errors.Add("زمان شروع الزامی است");
            }

            if (request.EndTime == default(TimeSpan))
            {
                errors.Add("زمان پایان الزامی است");
            }

            if (request.StartTime >= request.EndTime)
            {
                errors.Add("زمان پایان باید بعد از زمان شروع باشد");
            }

            var duration = (request.EndTime - request.StartTime).TotalMinutes;
            if (duration < 5)
            {
                errors.Add("حداقل مدت زمان نوبت 5 دقیقه است");
            }

            if (duration > 120)
            {
                errors.Add("حداکثر مدت زمان نوبت 120 دقیقه است");
            }

            return ValidationResult.Create(errors, warnings);
        }

        private async Task<ValidationResult> ValidateDoctorAsync(int doctorId)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    errors.Add("پزشک مورد نظر یافت نشد");
                    return ValidationResult.Failed(errors);
                }

                var doctor = doctorResult.Data;
                if (!doctor.IsActive)
                {
                    errors.Add("این پزشک در حال حاضر فعال نیست");
                }

                return ValidationResult.Create(errors, warnings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی پزشک {DoctorId}", doctorId);
                errors.Add("خطا در بررسی اطلاعات پزشک");
                return ValidationResult.Failed(errors);
            }
        }

        private async Task<ValidationResult> ValidateDoctorScheduleAsync(int doctorId, DateTime appointmentDate)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule == null)
                {
                    errors.Add("برنامه کاری برای این پزشک تعریف نشده است");
                    return ValidationResult.Failed(errors);
                }

                if (!schedule.IsActive)
                {
                    errors.Add("برنامه کاری این پزشک فعال نیست");
                    return ValidationResult.Failed(errors);
                }

                // ✅ CRITICAL FIX: بررسی روز کاری
                // ⚠️ IMPORTANT: .NET DayOfWeek enum: Sunday=0, Monday=1, ..., Saturday=6
                // ⚠️ IMPORTANT: Database DayOfWeek: یکشنبه=0, دوشنبه=1, ..., شنبه=6
                // ⚠️ IMPORTANT: Mapping: .NET Sunday (0) → یکشنبه (0), .NET Monday (1) → دوشنبه (1), etc.
                // ✅ در واقع mapping مستقیم است چون هر دو از 0 شروع می‌شوند و یکشنبه = Sunday
                var dayOfWeek = (int)appointmentDate.DayOfWeek;
                
                _logger.Debug("🔍 بررسی روز کاری - DoctorId: {DoctorId}, AppointmentDate: {Date}, .NET DayOfWeek: {DotNetDayOfWeek}, Database DayOfWeek: {DbDayOfWeek}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"), appointmentDate.DayOfWeek, dayOfWeek);
                
                var workDay = schedule.WorkDays?.FirstOrDefault(w => 
                    w.DayOfWeek == dayOfWeek && w.IsActive && !w.IsDeleted);

                if (workDay == null)
                {
                    // ✅ CRITICAL FIX: Log تمام WorkDays برای debugging DayOfWeek mapping
                    var allWorkDays = schedule.WorkDays?.Select(w => new
                    {
                        DayOfWeek = w.DayOfWeek,
                        DayName = GetPersianDayName(w.DayOfWeek),
                        IsActive = w.IsActive,
                        IsDeleted = w.IsDeleted
                    }).ToList();
                    
                    var workDaysInfo = allWorkDays != null && allWorkDays.Any()
                        ? string.Join(", ", allWorkDays.Select(w => $"{w.DayName} (DayOfWeek={w.DayOfWeek}, Active={w.IsActive}, Deleted={w.IsDeleted})"))
                        : "null";
                    
                    _logger.Warning("⚠️ پزشک {DoctorId} در {DayName} (DayOfWeek: {DayOfWeek}) برنامه کاری ندارد. WorkDays موجود: {WorkDays}",
                        doctorId, GetPersianDayName(dayOfWeek), dayOfWeek, workDaysInfo);
                    errors.Add($"پزشک در {GetPersianDayName(dayOfWeek)} برنامه کاری ندارد");
                    return ValidationResult.Failed(errors);
                }
                
                _logger.Debug("✅ روز کاری یافت شد - DoctorId: {DoctorId}, DayOfWeek: {DayOfWeek}, WorkDayId: {WorkDayId}",
                    doctorId, dayOfWeek, workDay.WorkDayId);

                // بررسی استثناها (تعطیلات، مرخصی)
                var exception = schedule.Exceptions?.FirstOrDefault(e =>
                    e.StartDate <= appointmentDate &&
                    (e.EndDate == null || e.EndDate >= appointmentDate) &&
                    !e.IsDeleted);

                if (exception != null)
                {
                    // بررسی استثناهای تمام روز (تعطیلات، مرخصی)
                    if (exception.Type == ExceptionType.PublicHoliday || 
                        exception.Type == ExceptionType.Vacation || 
                        exception.Type == ExceptionType.SickLeave ||
                        exception.Type == ExceptionType.Holiday)
                    {
                        errors.Add($"در تاریخ انتخاب شده پزشک در دسترس نیست ({exception.Reason ?? "تعطیل"})");
                    }
                    else
                    {
                        // استثناهای جزئی روز (سفر کاری، کنفرانس)
                        warnings.Add($"در تاریخ انتخاب شده پزشک محدودیت زمانی دارد: {exception.Reason ?? exception.Description}");
                    }
                }

                return ValidationResult.Create(errors, warnings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی برنامه کاری پزشک {DoctorId}", doctorId);
                errors.Add("خطا در بررسی برنامه کاری پزشک");
                return ValidationResult.Failed(errors);
            }
        }

        private async Task<ValidationResult> ValidateSlotAvailabilityAsync(
            int doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                var isAvailable = await _appointmentRepository.CheckSlotAvailabilityAsync(
                    doctorId, appointmentDate, startTime, endTime);

                if (!isAvailable)
                {
                    errors.Add("این زمان در دسترس نیست. لطفاً زمان دیگری انتخاب کنید");
                }

                return ValidationResult.Create(errors, warnings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی‌پذیری اسلات");
                errors.Add("خطا در بررسی دسترسی‌پذیری");
                return ValidationResult.Failed(errors);
            }
        }

        private ValidationResult ValidateBookingTime(DateTime appointmentDate, TimeSpan startTime)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            var appointmentDateTime = appointmentDate.Date.Add(startTime);
            var now = _timeProvider.GetIranNow();

            // بررسی تاریخ گذشته
            if (appointmentDate.Date < _timeProvider.GetIranToday())
            {
                errors.Add("نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید");
            }

            // بررسی حداقل زمان رزرو (2 ساعت قبل)
            var minimumBookingTime = now.AddHours(2);
            if (appointmentDateTime < minimumBookingTime)
            {
                errors.Add("نوبت باید حداقل 2 ساعت قبل از زمان نوبت رزرو شود");
            }

            // بررسی حداکثر زمان رزرو (90 روز بعد)
            var maximumBookingTime = now.AddDays(90);
            if (appointmentDateTime > maximumBookingTime)
            {
                errors.Add("نمی‌توانید بیش از 90 روز جلوتر نوبت رزرو کنید");
            }

            // هشدار برای نوبت‌های فوری (کمتر از 24 ساعت)
            var urgentThreshold = now.AddHours(24);
            if (appointmentDateTime < urgentThreshold && appointmentDateTime >= minimumBookingTime)
            {
                warnings.Add("این نوبت کمتر از 24 ساعت دیگر است. در صورت امکان، زمان دیگری انتخاب کنید");
            }

            return ValidationResult.Create(errors, warnings);
        }

        private async Task<ValidationResult> ValidatePatientConflictAsync(
            int patientId, int doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                // ✅ CRITICAL FIX: استفاده از متد جدید با Locking برای جلوگیری از Race Condition
                var hasOverlap = await _appointmentRepository.HasOverlappingPatientAppointmentAsync(
                    patientId, appointmentDate, startTime, endTime);

                if (hasOverlap)
                {
                    errors.Add("شما در این تاریخ و زمان قبلاً نوبت دارید. لطفاً زمان دیگری انتخاب کنید");
                    _logger.Warning("⚠️ DOUBLE BOOKING PREVENTED: بیمار {PatientId} در تاریخ {Date} زمان {StartTime} قبلاً نوبت دارد",
                        patientId, appointmentDate.ToString("yyyy/MM/dd"), startTime);
                }

                // هشدار برای چند نوبت در یک روز (بدون Locking - فقط برای هشدار)
                var patientAppointments = await _appointmentRepository.GetPatientAppointmentsAsync(
                    patientId, appointmentDate.Date, appointmentDate.Date.AddDays(1));

                var sameDayAppointments = patientAppointments.Count(a =>
                    a.AppointmentDate.Date == appointmentDate.Date &&
                    a.Status != AppointmentStatus.Cancelled);

                if (sameDayAppointments >= 3)
                {
                    warnings.Add("شما در این روز بیش از 3 نوبت دارید. لطفاً در صورت امکان، زمان دیگری انتخاب کنید");
                }

                return ValidationResult.Create(errors, warnings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی تداخل نوبت‌های بیمار");
                errors.Add("خطا در بررسی تداخل نوبت‌های شما. لطفاً دوباره تلاش کنید");
                return ValidationResult.Failed(errors);
            }
        }

        private async Task<ValidationResult> ValidateDoctorCapacityAsync(int doctorId, DateTime appointmentDate)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule == null)
                {
                    return ValidationResult.Create(errors, warnings);
                }

                // تعداد نوبت‌های رزرو شده در این روز
                var bookedAppointments = await _appointmentRepository.GetDoctorAppointmentsByDateAsync(
                    doctorId, appointmentDate);

                var bookedCount = bookedAppointments.Count(a => a.Status != AppointmentStatus.Cancelled);

                // بررسی ظرفیت روزانه
                if (schedule.MaxAppointmentsPerDay > 0 && bookedCount >= schedule.MaxAppointmentsPerDay)
                {
                    errors.Add("ظرفیت نوبت‌های این پزشک در این روز تکمیل شده است");
                }
                else if (schedule.MaxAppointmentsPerDay > 0)
                {
                    var remainingCapacity = schedule.MaxAppointmentsPerDay - bookedCount;
                    if (remainingCapacity <= 3)
                    {
                        warnings.Add($"فقط {remainingCapacity} نوبت دیگر در این روز باقی مانده است");
                    }
                }

                return ValidationResult.Create(errors, warnings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی ظرفیت پزشک");
                warnings.Add("نتوانستیم ظرفیت پزشک را بررسی کنیم");
                return ValidationResult.Create(new List<string>(), warnings);
            }
        }

        private string GetPersianDayName(int dayOfWeek)
        {
            var dayNames = new[] { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };
            return dayOfWeek >= 0 && dayOfWeek < dayNames.Length ? dayNames[dayOfWeek] : "روز نامشخص";
        }

        #endregion
    }

    /// <summary>
    /// نتیجه اعتبارسنجی
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public List<string> Errors { get; private set; }
        public List<string> Warnings { get; private set; }

        private ValidationResult(bool isValid, List<string> errors, List<string> warnings)
        {
            IsValid = isValid;
            Errors = errors ?? new List<string>();
            Warnings = warnings ?? new List<string>();
        }

        public static ValidationResult Successful(List<string> warnings = null)
        {
            return new ValidationResult(true, new List<string>(), warnings ?? new List<string>());
        }

        public static ValidationResult Failed(List<string> errors, List<string> warnings = null)
        {
            return new ValidationResult(false, errors ?? new List<string>(), warnings ?? new List<string>());
        }

        public static ValidationResult Create(List<string> errors, List<string> warnings = null)
        {
            return new ValidationResult(!errors.Any(), errors, warnings ?? new List<string>());
        }
    }
}

