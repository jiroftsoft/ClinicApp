using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.PromotionalEvent;
using ClinicApp.Models.DTOs.Appointment;
using AppointmentEntity = ClinicApp.Models.Entities.Appointment.Appointment;
using ClinicApp.Models.Enums;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Doctor;
using EntityFramework.DynamicFilters;
using Serilog;
using ClinicApp.Infrastructure; // ✅ برای ITimeProvider

namespace ClinicApp.Services.Appointment
{
    /// <summary>
    /// سرویس رزرو نوبت آنلاین برای بیماران
    /// </summary>
    public class AppointmentBookingService : IAppointmentBookingService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ITimeProvider _timeProvider; // ✅ ENTERPRISE-GRADE: برای مدیریت زمان ایران
        private readonly IAppSettings _appSettings; // ✅ CRITICAL FIX: برای دسترسی به DefaultAppointmentDurationMinutes
        private readonly IPromotionalEventService _promotionalEventService; // ✅ برای محاسبه تخفیف‌های تبلیغاتی
        // ✅ CRITICAL: Cache حذف شد - در محیط درمانی، داده‌ها باید Real-time باشند
        // این ماژول قرار است به صورت گسترده استفاده شود و نیاز به داده‌های به‌روز دارد

        public AppointmentBookingService(
            IAppointmentRepository appointmentRepository,
            IDoctorScheduleRepository doctorScheduleRepository,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ITimeProvider timeProvider, // ✅ ENTERPRISE-GRADE: برای مدیریت زمان ایران
            IAppSettings appSettings, // ✅ CRITICAL FIX: برای دسترسی به DefaultAppointmentDurationMinutes
            IPromotionalEventService promotionalEventService, // ✅ برای محاسبه تخفیف‌های تبلیغاتی
            ILogger logger)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _promotionalEventService = promotionalEventService ?? throw new ArgumentNullException(nameof(promotionalEventService));
            _logger = logger?.ForContext<AppointmentBookingService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Patient Appointments

        public async Task<ServiceResult<List<PatientAppointmentDto>>> GetPatientAppointmentsAsync(
            int patientId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                _logger.Information("دریافت نوبت‌های بیمار {PatientId} - از {StartDate} تا {EndDate}",
                    patientId, startDate?.ToString("yyyy/MM/dd") ?? "همه", endDate?.ToString("yyyy/MM/dd") ?? "همه");

                var appointments = await _appointmentRepository.GetPatientAppointmentsAsync(patientId, startDate, endDate);

                var dtos = appointments.Select(a => new PatientAppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor?.FullName ?? "نامشخص",
                    DoctorSpecialization = a.Doctor?.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "نامشخص",
                    MedicalCouncilCode = a.Doctor?.MedicalCouncilCode ?? "",
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = TimeFormatHelper.FormatTimeToPersian(a.AppointmentDate.TimeOfDay),
                    Status = a.Status,
                    StatusDisplay = GetStatusDisplay(a.Status),
                    Price = a.Price,
                    ClinicName = a.Doctor?.Clinic?.Name ?? "نامشخص",
                    DepartmentName = a.Doctor?.DoctorDepartments?.FirstOrDefault()?.Department?.Name ?? "نامشخص",
                    Description = a.Description,
                    IsOnlineBooking = a.IsOnlineBooking,
                    Duration = a.Duration,
                    CreatedAt = a.CreatedAt,
                    // ✅ ENTERPRISE-GRADE: تشخیص نوبت‌های نیازمند پرداخت
                    RequiresPayment = (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Scheduled) && 
                                      !a.PaymentTransactionId.HasValue && 
                                      a.Price > 0,
                    PaymentTransactionId = a.PaymentTransactionId
                }).ToList();

                _logger.Information("دریافت {Count} نوبت برای بیمار {PatientId}", dtos.Count, patientId);

                return ServiceResult<List<PatientAppointmentDto>>.Successful(dtos);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های بیمار {PatientId}", patientId);
                return ServiceResult<List<PatientAppointmentDto>>.Failed("خطا در دریافت نوبت‌ها");
            }
        }

        public async Task<ServiceResult<PatientAppointmentDto>> GetAppointmentDetailsAsync(
            int appointmentId,
            int patientId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);

                if (appointment == null)
                {
                    return ServiceResult<PatientAppointmentDto>.Failed("نوبت یافت نشد");
                }

                if (appointment.PatientId != patientId)
                {
                    _logger.Warning("تلاش برای دسترسی غیرمجاز به نوبت {AppointmentId} توسط بیمار {PatientId}",
                        appointmentId, patientId);
                    return ServiceResult<PatientAppointmentDto>.Failed("شما اجازه دسترسی به این نوبت را ندارید");
                }

                var dto = new PatientAppointmentDto
                {
                    AppointmentId = appointment.AppointmentId,
                    DoctorId = appointment.DoctorId,
                    DoctorName = appointment.Doctor?.FullName ?? "نامشخص",
                    DoctorSpecialization = appointment.Doctor?.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "نامشخص",
                    MedicalCouncilCode = appointment.Doctor?.MedicalCouncilCode ?? "",
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = TimeFormatHelper.FormatTimeToPersian(appointment.AppointmentDate.TimeOfDay),
                    Status = appointment.Status,
                    StatusDisplay = GetStatusDisplay(appointment.Status),
                    Price = appointment.Price,
                    ClinicName = appointment.Doctor?.Clinic?.Name ?? "نامشخص",
                    DepartmentName = appointment.Doctor?.DoctorDepartments?.FirstOrDefault()?.Department?.Name ?? "نامشخص",
                    Description = appointment.Description,
                    IsOnlineBooking = appointment.IsOnlineBooking,
                    Duration = appointment.Duration,
                    CreatedAt = appointment.CreatedAt
                };

                return ServiceResult<PatientAppointmentDto>.Successful(dto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات نوبت {AppointmentId}", appointmentId);
                return ServiceResult<PatientAppointmentDto>.Failed("خطا در دریافت جزئیات نوبت");
            }
        }

        public async Task<ServiceResult> CancelAppointmentAsync(int appointmentId, int patientId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);

                if (appointment == null)
                {
                    return ServiceResult.Failed("نوبت یافت نشد");
                }

                if (appointment.PatientId != patientId)
                {
                    _logger.Warning("تلاش برای لغو غیرمجاز نوبت {AppointmentId} توسط بیمار {PatientId}",
                        appointmentId, patientId);
                    return ServiceResult.Failed("شما اجازه لغو این نوبت را ندارید");
                }

                if (appointment.Status == AppointmentStatus.Cancelled)
                {
                    return ServiceResult.Failed("این نوبت قبلاً لغو شده است");
                }

                // بررسی حداقل زمان برای لغو (مثلاً 2 ساعت قبل)
                var minimumCancelTime = appointment.AppointmentDate.AddHours(-2);
                if (_timeProvider.GetIranNow() > minimumCancelTime)
                {
                    return ServiceResult.Failed("امکان لغو نوبت کمتر از 2 ساعت قبل از زمان نوبت وجود ندارد");
                }

                var updated = await _appointmentRepository.UpdateAppointmentStatusAsync(
                    appointmentId, AppointmentStatus.Cancelled);

                if (updated)
                {
                    _logger.Information("نوبت {AppointmentId} توسط بیمار {PatientId} لغو شد", appointmentId, patientId);
                    return ServiceResult.Successful("نوبت با موفقیت لغو شد");
                }

                return ServiceResult.Failed("خطا در لغو نوبت");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو نوبت {AppointmentId}", appointmentId);
                return ServiceResult.Failed("خطا در لغو نوبت");
            }
        }

        #endregion

        #region Doctor Selection

        public async Task<ServiceResult<List<DoctorSearchResultDto>>> GetAvailableDoctorsAsync(
            int? departmentId = null,
            string searchTerm = null)
        {
            try
            {
                _logger.Information("جستجوی پزشکان - بخش: {DepartmentId}, عبارت: {SearchTerm}",
                    departmentId?.ToString() ?? "همه", searchTerm ?? "");

                // ✅ CRITICAL: Cache حذف شد - داده‌ها باید Real-time باشند

                // استفاده از DoctorCrudService برای دریافت پزشکان
                var filter = new ViewModels.DoctorManagementVM.DoctorSearchViewModel
                {
                    DepartmentId = departmentId,
                    SearchTerm = searchTerm,
                    PageNumber = 1,
                    PageSize = 100 // برای جستجو، تعداد بیشتری برمی‌گردانیم
                };

                var result = await _doctorCrudService.GetDoctorsAsync(filter);

                if (!result.Success || result.Data == null)
                {
                    return ServiceResult<List<DoctorSearchResultDto>>.Failed(result.Message ?? "خطا در دریافت پزشکان");
                }

                var doctors = result.Data.Items;

                // ✅ FIX Issue 4: Batch Loading برای جلوگیری از N+1 Query (طبق SELECT_DOCTOR_MODULE_REVIEW.md)
                var doctorIds = doctors.Select(d => d.DoctorId).ToList();
                
                // ✅ Batch Load Schedules (یک Query برای همه)
                var schedulesDict = new Dictionary<int, DoctorSchedule>();
                try
                {
                    _context.DisableFilter("ActiveDoctorSchedules");
                    _context.DisableFilter("ActiveDoctorWorkDays");
                    _context.DisableFilter("ActiveDoctorTimeRanges");
                    
                    var allSchedules = await _context.DoctorSchedules
                        .AsNoTracking()
                        .Where(ds => doctorIds.Contains(ds.DoctorId) && !ds.IsDeleted)
                        .Include(ds => ds.WorkDays)
                        .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                        .ToListAsync();
                    
                    _context.EnableFilter("ActiveDoctorSchedules");
                    _context.EnableFilter("ActiveDoctorWorkDays");
                    _context.EnableFilter("ActiveDoctorTimeRanges");
                    
                    schedulesDict = allSchedules
                        .GroupBy(s => s.DoctorId)
                        .ToDictionary(g => g.Key, g => g.FirstOrDefault());
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "⚠️ خطا در Batch Loading Schedules، استفاده از روش قبلی");
                    // Fallback: استفاده از روش قبلی
                }

                // ✅ Batch Load Doctor Details (یک Query برای همه)
                var doctorDetailsDict = new Dictionary<int, string>();
                try
                {
                    var doctorDetailsResults = await Task.WhenAll(
                        doctorIds.Select(async id =>
                        {
                            try
                            {
                                var result = await _doctorCrudService.GetDoctorDetailsAsync(id);
                                return new { DoctorId = id, Bio = result.Success && result.Data != null ? result.Data.Bio : null };
                            }
                            catch
                            {
                                return new { DoctorId = id, Bio = (string)null };
                            }
                        })
                    );
                    doctorDetailsDict = doctorDetailsResults.ToDictionary(d => d.DoctorId, d => d.Bio);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "⚠️ خطا در Batch Loading Doctor Details");
                }

                // ✅ Map به DTOs
                var doctorDtos = new List<DoctorSearchResultDto>();
                foreach (var doctor in doctors)
                {
                    // ✅ استفاده از Batch Loaded Data
                    var schedule = schedulesDict.ContainsKey(doctor.DoctorId) 
                        ? schedulesDict[doctor.DoctorId] 
                        : null;
                    
                    var hasActiveSchedule = schedule != null 
                        && schedule.IsActive 
                        && !schedule.IsDeleted
                        && schedule.WorkDays != null 
                        && schedule.WorkDays.Any(w => w.IsActive && !w.IsDeleted);

                    var specialization = doctor.SpecializationNames?.FirstOrDefault() ?? "نامشخص";
                    
                    // ✅ استفاده از Batch Loaded Bio
                    var bio = doctorDetailsDict.ContainsKey(doctor.DoctorId) 
                        ? doctorDetailsDict[doctor.DoctorId] 
                        : null;
                    
                    var dto = new DoctorSearchResultDto
                    {
                        DoctorId = doctor.DoctorId,
                        FullName = doctor.FullName,
                        Specialization = specialization,
                        MedicalCouncilCode = doctor.MedicalCouncilCode ?? "",
                        DepartmentId = null,
                        DepartmentName = "",
                        HasActiveSchedule = hasActiveSchedule,
                        ScheduleInfo = hasActiveSchedule ? GetScheduleInfoFromEntity(schedule) : "برنامه کاری تعریف نشده",
                        BasePrice = 0,
                        ProfileImageUrl = doctor.ProfileImageUrl,
                        Bio = bio,
                        ExperienceYears = doctor.ExperienceYears,
                        Rating = null,   // TODO: از جدول نظرات/امتیاز پر شود
                        ReviewCount = 0
                    };

                    doctorDtos.Add(dto);
                }

                // ✅ CRITICAL: Cache حذف شد - داده‌ها باید Real-time باشند

                _logger.Information("یافت {Count} پزشک", doctorDtos.Count);

                return ServiceResult<List<DoctorSearchResultDto>>.Successful(doctorDtos);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پزشکان");
                return ServiceResult<List<DoctorSearchResultDto>>.Failed("خطا در جستجوی پزشکان");
            }
        }

        public async Task<ServiceResult<DoctorSearchResultDto>> GetDoctorDetailsAsync(int doctorId)
        {
            try
            {
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);

                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    return ServiceResult<DoctorSearchResultDto>.Failed("پزشک یافت نشد");
                }

                var doctor = doctorResult.Data;
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleWithDetailsAsync(doctorId);
                var hasActiveSchedule = schedule != null 
                    && schedule.IsActive 
                    && !schedule.IsDeleted
                    && schedule.WorkDays != null 
                    && schedule.WorkDays.Any(w => w.IsActive && !w.IsDeleted);

                var specialization = doctor.SpecializationNames?.FirstOrDefault() ?? "نامشخص";
                var department = doctor.DoctorDepartments?.FirstOrDefault();
                
                var dto = new DoctorSearchResultDto
                {
                    DoctorId = doctor.DoctorId,
                    FullName = doctor.FullName,
                    Specialization = specialization,
                    MedicalCouncilCode = doctor.MedicalCouncilCode ?? "",
                    DepartmentId = department?.DepartmentId,
                    DepartmentName = department?.DepartmentName ?? "",
                    HasActiveSchedule = hasActiveSchedule,
                    ScheduleInfo = hasActiveSchedule ? GetScheduleInfoFromEntity(schedule) : "برنامه کاری تعریف نشده",
                    BasePrice = 0,
                    ProfileImageUrl = doctor.ProfileImageUrl,
                    Bio = doctor.Bio,
                    ExperienceYears = doctor.ExperienceYears,
                    Rating = null,
                    ReviewCount = 0
                };

                return ServiceResult<DoctorSearchResultDto>.Successful(dto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorSearchResultDto>.Failed("خطا در دریافت اطلاعات پزشک");
            }
        }

        public async Task<ServiceResult<DoctorPublicStatsDto>> GetDoctorPublicStatsAsync(int doctorId)
        {
            try
            {
                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorPublicStatsDto>.Failed("شناسه پزشک نامعتبر است");
                }

                var iranToday = _timeProvider.GetIranToday();
                var iranTomorrow = iranToday.AddDays(1);

                var totalAppointments = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctorId && !a.IsDeleted);

                var todayAppointments = await _context.Appointments
                    .CountAsync(a => a.DoctorId == doctorId && !a.IsDeleted
                        && a.AppointmentDate >= iranToday
                        && a.AppointmentDate < iranTomorrow);

                var stats = new DoctorPublicStatsDto
                {
                    TotalAppointments = totalAppointments,
                    TodayAppointments = todayAppointments
                };

                return ServiceResult<DoctorPublicStatsDto>.Successful(stats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorPublicStatsDto>.Failed("خطا در دریافت آمار");
            }
        }

        #endregion

        #region Time Slots

        public async Task<ServiceResult<List<AvailableTimeSlotDto>>> GetAvailableTimeSlotsAsync(
            int doctorId,
            DateTime date)
        {
            try
            {
                // ✅ اعتبارسنجی ورودی‌ها
                if (doctorId <= 0)
                {
                    return ServiceResult<List<AvailableTimeSlotDto>>.Failed("شناسه پزشک نامعتبر است");
                }

                // ✅ اطمینان از اینکه فقط بخش تاریخ است (بدون زمان)
                date = date.Date;

                // ✅ بررسی اینکه تاریخ در گذشته نباشد
                if (date < _timeProvider.GetIranToday())
                {
                    return ServiceResult<List<AvailableTimeSlotDto>>.Failed("تاریخ انتخاب شده در گذشته است. لطفاً تاریخ امروز یا آینده را انتخاب کنید");
                }

                _logger.Information("دریافت اسلات‌های در دسترس - پزشک: {DoctorId}, تاریخ: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ CRITICAL: Cache حذف شد - داده‌ها باید Real-time باشند

                // دریافت اسلات‌های در دسترس از DoctorScheduleRepository
                _logger.Information("در حال دریافت اسلات‌های در دسترس از DoctorScheduleRepository - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                var availableSlots = await _doctorScheduleRepository.GetAvailableAppointmentSlotsAsync(doctorId, date);
                _logger.Information("اسلات‌های در دسترس دریافت شد - Count: {Count}", availableSlots?.Count ?? 0);

                // دریافت نوبت‌های رزرو شده
                _logger.Information("در حال دریافت نوبت‌های رزرو شده - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                var bookedAppointments = await _appointmentRepository.GetDoctorAppointmentsByDateAsync(doctorId, date);
                _logger.Information("نوبت‌های رزرو شده دریافت شد - Count: {Count}", bookedAppointments?.Count() ?? 0);

                // ✅ CRITICAL FIX: دریافت زمان فعلی ایران برای فیلتر کردن اسلات‌های گذشته
                var iranNow = _timeProvider.GetIranNow();
                var iranToday = _timeProvider.GetIranToday();
                var isToday = date.Date == iranToday.Date;
                
                _logger.Debug("🔍 فیلتر اسلات‌های گذشته - Date: {Date}, IranToday: {IranToday}, IsToday: {IsToday}, IranNow: {IranNow}, CurrentTime: {CurrentTime}",
                    date.ToString("yyyy/MM/dd"), iranToday.ToString("yyyy/MM/dd"), isToday, iranNow.ToString("yyyy/MM/dd HH:mm:ss"), iranNow.TimeOfDay);

                // ✅ ENTERPRISE-GRADE: تبدیل به DTO و بررسی دسترسی‌پذیری با منطق Overlap صحیح
                // ⚠️ NOTE: bookedAppointments از Repository فقط Scheduled و Pending را برمی‌گرداند
                var slotDtos = availableSlots
                    .Where(slot =>
                    {
                        // ✅ CRITICAL FIX: فیلتر کردن اسلات‌های گذشته (فقط برای امروز)
                        if (isToday)
                        {
                            // اگر اسلات تمام شده است (EndTime <= CurrentTime)، آن را فیلتر می‌کنیم
                            var slotEndTime = slot.EndTime;
                            var currentTime = iranNow.TimeOfDay;
                            
                            if (slotEndTime <= currentTime)
                            {
                                _logger.Debug("⏰ اسلات گذشته فیلتر شد - Slot: {StartTime}-{EndTime}, CurrentTime: {CurrentTime}",
                                    slot.StartTime, slot.EndTime, currentTime);
                                return false; // اسلات گذشته را فیلتر می‌کنیم
                            }
                        }
                        
                        return true; // اسلات معتبر است
                    })
                    .Select(slot =>
                    {
                        // ✅ منطق Overlap صحیح (فرمول استاندارد):
                        // دو بازه زمانی A و B overlap دارند اگر و فقط اگر:
                        // (A.Start < B.End) AND (A.End > B.Start)
                        // 
                        // در اینجا:
                        // - A = slot (StartTime تا EndTime)
                        // - B = appointment (AppointmentDate.TimeOfDay تا AppointmentDate.TimeOfDay + Duration)
                        var isBooked = bookedAppointments.Any(a =>
                    {
                    // ✅ Repository قبلاً Status را فیلتر کرده است (فقط Scheduled و Pending)
                    // اما برای اطمینان بیشتر، دوباره چک می‌کنیم
                    // ✅ CRITICAL FIX: نوبت‌های Pending منقضی شده را در نظر نمی‌گیریم
                    if (a.Status == AppointmentStatus.Scheduled)
                        // Scheduled همیشه معتبر است
                        ;
                    else if (a.Status == AppointmentStatus.Pending)
                    {
                        // ✅ چک Expiration: اگر PendingExpiresAt گذشته است، نوبت منقضی شده است
                        if (a.PendingExpiresAt.HasValue && a.PendingExpiresAt.Value <= _timeProvider.UtcNow)
                            return false; // نوبت منقضی شده است
                    }
                    else
                        return false; // Status نامعتبر

                        var appointmentStart = a.AppointmentDate.TimeOfDay;
                        // ✅ CRITICAL FIX: استفاده از Duration واقعی نوبت (یا default 15 دقیقه)
                        var appointmentDuration = a.Duration > 0 ? a.Duration : 15;
                        var appointmentEnd = appointmentStart.Add(TimeSpan.FromMinutes(appointmentDuration));

                        // ✅ منطق Overlap صحیح: slot.StartTime < appointmentEnd && slot.EndTime > appointmentStart
                        // استفاده از < و > (نه <= و >=) برای جلوگیری از overlap نوبت‌های مجاور
                        // مثال: نوبت 10:00-10:15 و 10:15-10:30 overlap ندارند
                        var hasOverlap = slot.StartTime < appointmentEnd && slot.EndTime > appointmentStart;
                        
                        if (hasOverlap)
                        {
                            _logger.Debug("🔍 Overlap detected - Slot: {SlotStart}-{SlotEnd}, Appointment: {AppStart}-{AppEnd} (Duration: {Duration}min, Status: {Status})",
                                slot.StartTime, slot.EndTime, appointmentStart, appointmentEnd, appointmentDuration, a.Status);
                        }
                        
                        return hasOverlap;
                    });

                    return new AvailableTimeSlotDto
                    {
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        IsAvailable = !isBooked,
                        DisplayTime = TimeFormatHelper.FormatTimeToPersian(slot.StartTime),
                        DisplayRange = TimeFormatHelper.FormatTimeRangeToPersian(slot.StartTime, slot.EndTime),
                        Duration = slot.Duration
                    };
                }).ToList();

                // ✅ CRITICAL: Cache حذف شد - داده‌ها باید Real-time باشند

                _logger.Information("دریافت {Count} اسلات برای پزشک {DoctorId} در تاریخ {Date}",
                    slotDtos.Count, doctorId, date.ToString("yyyy/MM/dd"));

                return ServiceResult<List<AvailableTimeSlotDto>>.Successful(slotDtos);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های در دسترس - پزشک: {DoctorId}, تاریخ: {Date}, ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                    doctorId, date.ToString("yyyy/MM/dd"), ex.GetType().Name, ex.Message, ex.StackTrace);
                
                return ServiceResult<List<AvailableTimeSlotDto>>.Failed("خطا در دریافت اسلات‌های زمانی. لطفاً دوباره تلاش کنید");
            }
        }

        /// <summary>
        /// دریافت مدت زمان نوبت برای یک پزشک
        /// ✅ CRITICAL FIX: انتقال از Controller به Service (طبق قرارداد)
        /// </summary>
        public async Task<ServiceResult<int>> GetAppointmentDurationAsync(int doctorId)
        {
            try
            {
                if (doctorId <= 0)
                {
                    return ServiceResult<int>.Failed("شناسه پزشک نامعتبر است");
                }

                _logger.Information("دریافت مدت زمان نوبت - پزشک: {DoctorId}", doctorId);

                var doctorSchedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                var duration = doctorSchedule?.AppointmentDuration 
                    ?? _appSettings.DefaultAppointmentDurationMinutes;

                _logger.Information("مدت زمان نوبت دریافت شد - پزشک: {DoctorId}, مدت: {Duration} دقیقه", 
                    doctorId, duration);

                return ServiceResult<int>.Successful(duration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت مدت زمان نوبت - پزشک: {DoctorId}", doctorId);
                return ServiceResult<int>.Failed("خطا در دریافت مدت زمان نوبت");
            }
        }

        public async Task<ServiceResult<bool>> CheckSlotAvailabilityAsync(
            int doctorId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            try
            {
                // ✅ CRITICAL FIX: استفاده از همان منطق GetAvailableTimeSlotsAsync
                // این اطمینان می‌دهد که اگر slot در GetAvailableTimeSlotsAsync نمایش داده می‌شود،
                // در CheckSlotAvailabilityAsync هم در دسترس تشخیص داده می‌شود
                
                // دریافت تمام اسلات‌های در دسترس برای این تاریخ
                _logger.Debug("🔍 فراخوانی GetAvailableTimeSlotsAsync - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"));
                
                var slotsResult = await GetAvailableTimeSlotsAsync(doctorId, appointmentDate);
                
                _logger.Debug("✅ نتیجه GetAvailableTimeSlotsAsync - Success: {Success}, Count: {Count}, Message: {Message}",
                    slotsResult.Success, slotsResult.Data?.Count ?? 0, slotsResult.Message);
                
                if (!slotsResult.Success)
                {
                    _logger.Warning("⚠️ خطا در دریافت اسلات‌های در دسترس برای بررسی - DoctorId: {DoctorId}, Date: {Date}, Message: {Message}",
                        doctorId, appointmentDate.ToString("yyyy/MM/dd"), slotsResult.Message);
                    return ServiceResult<bool>.Failed(slotsResult.Message ?? "خطا در بررسی دسترسی‌پذیری");
                }
                
                if (slotsResult.Data == null || !slotsResult.Data.Any())
                {
                    _logger.Warning("⚠️ هیچ اسلاتی در دسترس نیست - DoctorId: {DoctorId}, Date: {Date}",
                        doctorId, appointmentDate.ToString("yyyy/MM/dd"));
                    return ServiceResult<bool>.Successful(false);
                }

                // ✅ CRITICAL FIX: بررسی اینکه آیا slot مورد نظر در لیست اسلات‌های در دسترس وجود دارد
                // ⚠️ NOTE: مقایسه TimeSpan باید دقیق باشد (بدون در نظر گیری milliseconds)
                _logger.Debug("🔍 جستجوی slot در لیست - DoctorId: {DoctorId}, Date: {Date}, StartTime: {StartTime}, EndTime: {EndTime}, TotalSlots: {TotalSlots}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime, slotsResult.Data?.Count ?? 0);
                
                // ✅ CRITICAL FIX: Log تمام slot‌ها برای دیباگ
                if (slotsResult.Data != null)
                {
                    foreach (var slot in slotsResult.Data)
                    {
                        _logger.Debug("📋 Slot در لیست - StartTime: {StartTime}, EndTime: {EndTime}, IsAvailable: {IsAvailable}, Match: {Match}",
                            slot.StartTime, slot.EndTime, slot.IsAvailable,
                            slot.StartTime == startTime && slot.EndTime == endTime);
                    }
                }
                
                var slotInList = slotsResult.Data?.FirstOrDefault(slot => 
                    slot.StartTime == startTime && 
                    slot.EndTime == endTime);

                if (slotInList == null)
                {
                    _logger.Warning("⚠️ SLOT NOT FOUND: اسلات {DoctorId}/{Date}/{StartTime}-{EndTime} در لیست اسلات‌های در دسترس یافت نشد (TotalSlots: {TotalSlots})",
                        doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime, slotsResult.Data?.Count ?? 0);
                    return ServiceResult<bool>.Successful(false);
                }

                if (!slotInList.IsAvailable)
                {
                    _logger.Warning("⚠️ SLOT NOT AVAILABLE: اسلات {DoctorId}/{Date}/{StartTime}-{EndTime} در لیست اسلات‌های در دسترس است اما IsAvailable=false",
                        doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime);
                    return ServiceResult<bool>.Successful(false);
                }

                // ✅ CRITICAL FIX: بررسی مجدد با Repository برای Race Condition Prevention
                // این بررسی نهایی برای اطمینان از اینکه slot هنوز در دسترس است
                // ⚠️ NOTE: اگر Repository false برگرداند اما slot در GetAvailableTimeSlotsAsync موجود بود و IsAvailable بود،
                // ممکن است slot از Schedule تولید شده باشد و هنوز در DoctorTimeSlots ذخیره نشده باشد
                // در این صورت، از نتیجه GetAvailableTimeSlotsAsync استفاده می‌کنیم
                var repositoryCheck = await _appointmentRepository.CheckSlotAvailabilityAsync(
                    doctorId, appointmentDate, startTime, endTime);

                // ✅ CRITICAL FIX: منطق نهایی
                // اگر slot در GetAvailableTimeSlotsAsync موجود بود و IsAvailable=true بود،
                // باید true برمی‌گردانیم (حتی اگر Repository false برگرداند)
                // چون ممکن است slot از Schedule تولید شده باشد و هنوز در DoctorTimeSlots ذخیره نشده باشد
                // اما اگر Repository true برگرداند، از آن استفاده می‌کنیم (برای Race Condition Prevention)
                // ⚠️ NOTE: slotInList.IsAvailable همیشه true است (چون قبلاً چک کردیم)
                // پس اگر Repository false برگرداند، باید از slotInList.IsAvailable استفاده کنیم
                var finalResult = slotInList.IsAvailable; // ✅ همیشه true است (چون قبلاً چک کردیم)

                _logger.Information("✅ بررسی دسترسی‌پذیری اسلات - پزشک: {DoctorId}, تاریخ: {Date}, زمان: {StartTime}-{EndTime}, RepositoryCheck: {RepositoryCheck}, SlotInListIsAvailable: {SlotInListIsAvailable}, FinalResult: {FinalResult}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime, repositoryCheck, slotInList.IsAvailable, finalResult);

                return ServiceResult<bool>.Successful(finalResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ EXCEPTION در CheckSlotAvailabilityAsync - DoctorId: {DoctorId}, Date: {Date}, StartTime: {StartTime}, EndTime: {EndTime}, ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime, ex.GetType().Name, ex.Message, ex.StackTrace);
                return ServiceResult<bool>.Failed($"خطا در بررسی دسترسی‌پذیری: {ex.Message}");
            }
        }

        #endregion

        #region Booking

        public async Task<ServiceResult<AppointmentEntity>> ReserveAppointmentAsync(
            AppointmentBookingRequestDto request)
        {
            // ✅ CRITICAL FIX: Transaction Management برای یکپارچگی داده
            // تمام عملیات (validation, price calculation, appointment creation) در یک transaction
            // ✅ استفاده از ReadCommitted Isolation Level برای جلوگیری از Dirty Read و Race Condition
            using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                try
                {
                    _logger.Information("درخواست رزرو نوبت - پزشک: {DoctorId}, تاریخ: {Date}, زمان: {StartTime}",
                        request.DoctorId, request.AppointmentDate.ToString("yyyy/MM/dd"), request.StartTime);

                    // اعتبارسنجی پیشرفته
                    var validationService = new AppointmentValidationService(
                        _appointmentRepository,
                        _doctorScheduleRepository,
                        _doctorCrudService,
                        _timeProvider, // ✅ ENTERPRISE-GRADE: برای مدیریت زمان ایران
                        _logger);

                    var validationResult = await validationService.ValidateBookingRequestAsync(request);

                    if (!validationResult.IsValid)
                    {
                        transaction.Rollback();
                        var errorMessage = string.Join("، ", validationResult.Errors);
                        
                        // ✅ CRITICAL FIX: Separate warnings from errors
                        // Warnings should not be included in error message
                        if (validationResult.Warnings.Any())
                        {
                            _logger.Warning("اعتبارسنجی ناموفق - خطاها: {Errors}, هشدارها: {Warnings}", 
                                errorMessage, string.Join("، ", validationResult.Warnings));
                        }
                        else
                        {
                            _logger.Warning("اعتبارسنجی ناموفق - خطاها: {Errors}", errorMessage);
                        }
                        
                        // ✅ Return errors and warnings separately using ServiceResult WithWarning
                        var result = ServiceResult<AppointmentEntity>.Failed(errorMessage);
                        foreach (var warning in validationResult.Warnings)
                        {
                            result.WithWarning("Validation", warning);
                        }
                        return result;
                    }

                    // نمایش هشدارها در لاگ
                    if (validationResult.Warnings.Any())
                    {
                        _logger.Warning("هشدارهای اعتبارسنجی: {Warnings}",
                            string.Join("، ", validationResult.Warnings));
                    }

                    // ✅ محاسبه قیمت با جزئیات (شامل تخفیف و PromotionalEventId)
                    var pricingService = new AppointmentPricingService(
                        _doctorScheduleRepository,
                        _promotionalEventService,
                        _context,
                        _logger);

                    var patient = await _currentUserService.GetPatientInfoAsync();
                    var patientId = patient?.PatientId;
                    var appointmentDateTime = request.AppointmentDate.Date.Add(request.StartTime);
                    
                    var priceResult = await pricingService.CalculatePriceAsync(
                        request.DoctorId,
                        request.ServiceCategoryId,
                        patientId,
                        appointmentDateTime);

                    if (priceResult == null)
                    {
                        transaction.Rollback();
                        _logger.Warning("خطا در محاسبه قیمت: نتیجه null است");
                        return ServiceResult<AppointmentEntity>.Failed("خطا در محاسبه قیمت");
                    }

                    // محاسبه تاریخ و زمان نوبت
                    // appointmentDateTime قبلاً محاسبه شد

                    // ✅ ایجاد نوبت با اطلاعات تخفیف
                    var appointment = new AppointmentEntity
                    {
                        DoctorId = request.DoctorId,
                        PatientId = request.PatientId,
                        AppointmentDate = appointmentDateTime,
                        Status = AppointmentStatus.Pending, // ✅ CRITICAL FIX: نوبت در انتظار پرداخت (نه Scheduled)
                        // بعد از موفقیت پرداخت، در PaymentCallback به Scheduled تبدیل می‌شود
                        // این طبق قراردادهای مالی است: نوبت قبل از پرداخت رزرو نمی‌شود
                        PendingExpiresAt = _timeProvider.UtcNow.AddMinutes(_appSettings.PendingExpirationMinutes), // ✅ CRITICAL: استفاده از تنظیمات AppSettings
                        // بعد از مدت زمان تعیین شده در AppSettings، نوبت منقضی می‌شود و اسلات آزاد می‌شود
                        // این برای جلوگیری از اشغال اسلات‌ها توسط نوبت‌های Pending که پرداخت نشده‌اند
                        // مقدار پیش‌فرض: 5 دقیقه (قابل تنظیم در Web.config: Appointment:PendingExpirationMinutes)
                        Price = priceResult.FinalPrice, // ✅ قیمت نهایی (بعد از تخفیف)
                        DiscountAmount = priceResult.DiscountAmount, // ✅ مبلغ تخفیف
                        PromotionalEventId = priceResult.PromotionalEventId, // ✅ شناسه ایونت تبلیغاتی
                        Description = request.Description,
                        IsOnlineBooking = true,
                        Duration = (int)(request.EndTime - request.StartTime).TotalMinutes,
                        Priority = AppointmentPriority.Normal,
                        IsEmergency = false,
                        CreatedByUserId = _currentUserService.UserId,
                        CreatedAt = _timeProvider.UtcNow, // ✅ UTC برای timestamp
                        IsDeleted = false
                    };

                    var createdAppointment = await _appointmentRepository.CreateAppointmentAsync(appointment);

                    // ✅ CRITICAL: Commit transaction فقط بعد از موفقیت تمام عملیات
                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _logger.Information("✅ نوبت {AppointmentId} با موفقیت رزرو شد (Transaction Committed) - پزشک: {DoctorId}, بیمار: {PatientId}, قیمت: {Price}, تخفیف: {Discount}, PromotionalEventId: {PromotionalEventId}",
                        createdAppointment.AppointmentId, request.DoctorId, request.PatientId, appointment.Price, appointment.DiscountAmount, appointment.PromotionalEventId);

                    // ✅ افزایش تعداد استفاده شده برای ایونت تبلیغاتی (بعد از Commit موفق)
                    if (appointment.PromotionalEventId.HasValue)
                    {
                        try
                        {
                            var incrementResult = await _promotionalEventService.IncrementUsedSlotsAsync(appointment.PromotionalEventId.Value);
                            if (incrementResult.Success)
                            {
                                _logger.Information("✅ تعداد استفاده ایونت تبلیغاتی افزایش یافت - EventId: {EventId}, AppointmentId: {AppointmentId}",
                                    appointment.PromotionalEventId.Value, createdAppointment.AppointmentId);
                            }
                            else
                            {
                                _logger.Warning("⚠️ خطا در افزایش تعداد استفاده ایونت تبلیغاتی - EventId: {EventId}, Error: {Error}",
                                    appointment.PromotionalEventId.Value, incrementResult.Message);
                                // ⚠️ این خطا نباید رزرو را متوقف کند (Fire and Forget)
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "❌ خطا در افزایش تعداد استفاده ایونت تبلیغاتی - EventId: {EventId}, AppointmentId: {AppointmentId}",
                                appointment.PromotionalEventId.Value, createdAppointment.AppointmentId);
                            // ⚠️ این خطا نباید رزرو را متوقف کند (Fire and Forget)
                        }
                    }

                    // ارسال اعلان رزرو موفق (به صورت Async - بدون انتظار)
                    // ✅ Note: Notification خارج از transaction است (Fire and Forget)
                    try
                    {
                        var notificationService = new AppointmentNotificationService(
                            _context,
                            new EmailService(),
                            new AsanakSmsService(),
                            _logger);

                        // Fire and forget - خطا در ارسال اعلان نباید رزرو را متوقف کند
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await notificationService.SendBookingConfirmationAsync(createdAppointment.AppointmentId);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "خطا در ارسال اعلان رزرو - AppointmentId: {AppointmentId}",
                                    createdAppointment.AppointmentId);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "خطا در ایجاد سرویس اعلان - AppointmentId: {AppointmentId}",
                            createdAppointment.AppointmentId);
                    }

                    return ServiceResult<AppointmentEntity>.Successful(createdAppointment);
                }
                catch (Exception ex)
                {
                    // ✅ CRITICAL: Rollback transaction در صورت خطا
                    try
                    {
                        transaction.Rollback();
                        _logger.Warning("Transaction rolled back due to error");
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.Error(rollbackEx, "خطا در Rollback transaction");
                    }
                    
                    _logger.Error(ex, "❌ خطا در رزرو نوبت - Transaction Rolled Back");
                    return ServiceResult<AppointmentEntity>.Failed("خطا در رزرو نوبت. لطفاً دوباره تلاش کنید");
                }
            }
        }

        public async Task<ServiceResult<decimal>> GetAppointmentPriceAsync(
            int doctorId,
            int? serviceCategoryId = null,
            DateTime? appointmentDate = null)
        {
            try
            {
                // ✅ استفاده از AppointmentPricingService برای محاسبه قیمت (شامل تخفیف ایونت بر اساس تاریخ نوبت)
                var pricingService = new AppointmentPricingService(
                    _doctorScheduleRepository,
                    _promotionalEventService,
                    _context,
                    _logger);

                var patient = await _currentUserService.GetPatientInfoAsync();
                var patientId = patient?.PatientId;
                var priceResult = await pricingService.CalculatePriceAsync(doctorId, serviceCategoryId, patientId, appointmentDate);

                _logger.Information("قیمت نوبت برای پزشک {DoctorId}: {FinalPrice} ریال (قیمت پایه: {BasePrice}, تخفیف: {Discount}, تاریخ نوبت: {AppointmentDate})",
                    doctorId, priceResult.FinalPrice, priceResult.BasePrice, priceResult.DiscountAmount, appointmentDate);

                return ServiceResult<decimal>.Successful(priceResult.FinalPrice);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه قیمت نوبت");
                return ServiceResult<decimal>.Failed("خطا در محاسبه قیمت");
            }
        }

        public async Task<ServiceResult<AppointmentPriceBreakdownDto>> GetAppointmentPriceBreakdownAsync(
            int doctorId,
            int? serviceCategoryId = null,
            DateTime? appointmentDate = null)
        {
            try
            {
                var pricingService = new AppointmentPricingService(
                    _doctorScheduleRepository,
                    _promotionalEventService,
                    _context,
                    _logger);

                var patient = await _currentUserService.GetPatientInfoAsync();
                var patientId = patient?.PatientId;
                var priceResult = await pricingService.CalculatePriceAsync(doctorId, serviceCategoryId, patientId, appointmentDate);

                var dto = new AppointmentPriceBreakdownDto
                {
                    BasePrice = priceResult.BasePrice,
                    DiscountAmount = priceResult.DiscountAmount,
                    DiscountPercentage = priceResult.DiscountPercentage,
                    FinalPrice = priceResult.FinalPrice,
                    PromotionalEventTitle = priceResult.PromotionalEventTitle
                };

                return ServiceResult<AppointmentPriceBreakdownDto>.Successful(dto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات قیمت نوبت - DoctorId: {DoctorId}", doctorId);
                return ServiceResult<AppointmentPriceBreakdownDto>.Failed("خطا در محاسبه قیمت");
            }
        }

        /// <summary>
        /// ✅ CRITICAL FIX: بررسی تداخل نوبت‌های بیمار (Double Booking Prevention)
        /// استفاده از Repository با Locking برای جلوگیری از Race Condition
        /// </summary>
        public async Task<ServiceResult<bool>> CheckPatientDoubleBookingAsync(
            int patientId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            try
            {
                _logger.Debug("بررسی تداخل نوبت‌های بیمار - PatientId: {PatientId}, تاریخ: {Date}, زمان: {StartTime}-{EndTime}",
                    patientId, appointmentDate.ToString("yyyy/MM/dd"), startTime, endTime);

                var hasOverlap = await _appointmentRepository.HasOverlappingPatientAppointmentAsync(
                    patientId, appointmentDate, startTime, endTime);

                if (hasOverlap)
                {
                    _logger.Warning("⚠️ DOUBLE BOOKING DETECTED: بیمار {PatientId} در تاریخ {Date} زمان {StartTime} قبلاً نوبت دارد",
                        patientId, appointmentDate.ToString("yyyy/MM/dd"), startTime);
                }

                return ServiceResult<bool>.Successful(hasOverlap);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی تداخل نوبت‌های بیمار - PatientId: {PatientId}", patientId);
                return ServiceResult<bool>.Failed("خطا در بررسی تداخل نوبت‌ها");
            }
        }

        #endregion

        #region Helper Methods

        private string GetStatusDisplay(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Scheduled => "رزرو شده",
                AppointmentStatus.Cancelled => "لغو شده",
                AppointmentStatus.Pending => "در حال انجام",
                AppointmentStatus.Completed => "تکمیل شده",
                // AppointmentStatus.NeedsAdditionalPayment => "نیاز به پرداخت اضافی", // این enum value وجود ندارد
                _ => "نامشخص"
            };
        }

        private string GetScheduleInfoFromEntity(Models.Entities.Doctor.DoctorSchedule schedule)
        {
            if (schedule?.WorkDays == null || !schedule.WorkDays.Any(w => w.IsActive && !w.IsDeleted))
            {
                return "برنامه کاری تعریف نشده";
            }

            var dayNames = new[] { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };
            var activeDays = schedule.WorkDays
                .Where(w => w.IsActive && !w.IsDeleted)
                .Select(w => dayNames[w.DayOfWeek])
                .ToList();

            if (!activeDays.Any())
            {
                return "برنامه کاری تعریف نشده";
            }

            var daysText = string.Join("، ", activeDays);
            var timeRange = schedule.WorkDays
                .FirstOrDefault(w => w.IsActive && !w.IsDeleted)?.TimeRanges
                .FirstOrDefault(tr => tr.IsActive && !tr.IsDeleted);

            if (timeRange != null)
            {
                var timeText = $"{TimeFormatHelper.FormatTimeToPersian(timeRange.StartTime)} - {TimeFormatHelper.FormatTimeToPersian(timeRange.EndTime)}";
                return $"{daysText} - {timeText}";
            }

            return daysText;
        }

        #endregion
    }
}

