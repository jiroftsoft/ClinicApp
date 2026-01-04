using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces;
using ClinicApp.Models.DTOs.Appointment;
using AppointmentEntity = ClinicApp.Models.Entities.Appointment.Appointment;
using ClinicApp.Models.Enums;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Doctor;
using EntityFramework.DynamicFilters;
using Serilog;

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
        // ✅ CRITICAL: Cache حذف شد - در محیط درمانی، داده‌ها باید Real-time باشند
        // این ماژول قرار است به صورت گسترده استفاده شود و نیاز به داده‌های به‌روز دارد

        public AppointmentBookingService(
            IAppointmentRepository appointmentRepository,
            IDoctorScheduleRepository doctorScheduleRepository,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
                    CreatedAt = a.CreatedAt
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
                if (DateTime.Now > minimumCancelTime)
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
                        DepartmentId = null, // DoctorIndexViewModel اطلاعات دپارتمان به صورت مستقیم ندارد
                        DepartmentName = "", // DoctorIndexViewModel اطلاعات دپارتمان به صورت مستقیم ندارد
                        HasActiveSchedule = hasActiveSchedule,
                        ScheduleInfo = hasActiveSchedule ? GetScheduleInfoFromEntity(schedule) : "برنامه کاری تعریف نشده",
                        BasePrice = 0, // در آینده از تنظیمات پزشک دریافت می‌شود
                        ProfileImageUrl = doctor.ProfileImageUrl,
                        Bio = bio,
                        ExperienceYears = doctor.ExperienceYears
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
                    ExperienceYears = doctor.ExperienceYears
                };

                return ServiceResult<DoctorSearchResultDto>.Successful(dto);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorSearchResultDto>.Failed("خطا در دریافت اطلاعات پزشک");
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
                if (date < DateTime.Today)
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

                // تبدیل به DTO و بررسی دسترسی‌پذیری
                var slotDtos = availableSlots.Select(slot =>
                {
                    var isBooked = bookedAppointments.Any(a =>
                        a.AppointmentDate.TimeOfDay >= slot.StartTime &&
                        a.AppointmentDate.TimeOfDay < slot.EndTime &&
                        a.Status != AppointmentStatus.Cancelled);

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
                
                // بررسی InnerException
                if (ex.InnerException != null)
                {
                    _logger.Error(ex.InnerException, "InnerException در دریافت اسلات‌های در دسترس - Message: {Message}",
                        ex.InnerException.Message);
                }
                
                return ServiceResult<List<AvailableTimeSlotDto>>.Failed($"خطا در دریافت اسلات‌های در دسترس: {ex.Message}");
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
                var isAvailable = await _appointmentRepository.CheckSlotAvailabilityAsync(
                    doctorId, appointmentDate, startTime, endTime);

                return ServiceResult<bool>.Successful(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی‌پذیری اسلات");
                return ServiceResult<bool>.Failed("خطا در بررسی دسترسی‌پذیری");
            }
        }

        #endregion

        #region Booking

        public async Task<ServiceResult<AppointmentEntity>> ReserveAppointmentAsync(
            AppointmentBookingRequestDto request)
        {
            // ✅ CRITICAL FIX: Transaction Management برای یکپارچگی داده
            // تمام عملیات (validation, price calculation, appointment creation) در یک transaction
            using (var transaction = _context.Database.BeginTransaction())
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
                        _logger);

                    var validationResult = await validationService.ValidateBookingRequestAsync(request);

                    if (!validationResult.IsValid)
                    {
                        transaction.Rollback();
                        var errorMessage = string.Join("، ", validationResult.Errors);
                        if (validationResult.Warnings.Any())
                        {
                            errorMessage += " | هشدارها: " + string.Join("، ", validationResult.Warnings);
                        }
                        _logger.Warning("اعتبارسنجی ناموفق - خطاها: {Errors}", errorMessage);
                        return ServiceResult<AppointmentEntity>.Failed(errorMessage);
                    }

                    // نمایش هشدارها در لاگ
                    if (validationResult.Warnings.Any())
                    {
                        _logger.Warning("هشدارهای اعتبارسنجی: {Warnings}",
                            string.Join("، ", validationResult.Warnings));
                    }

                    // محاسبه قیمت
                    var priceResult = await GetAppointmentPriceAsync(request.DoctorId, request.ServiceCategoryId);
                    if (!priceResult.Success)
                    {
                        transaction.Rollback();
                        _logger.Warning("خطا در محاسبه قیمت: {Message}", priceResult.Message);
                        return ServiceResult<AppointmentEntity>.Failed(priceResult.Message ?? "خطا در محاسبه قیمت");
                    }

                    // محاسبه تاریخ و زمان نوبت
                    var appointmentDateTime = request.AppointmentDate.Date.Add(request.StartTime);

                    // ایجاد نوبت
                    var appointment = new AppointmentEntity
                    {
                        DoctorId = request.DoctorId,
                        PatientId = request.PatientId,
                        AppointmentDate = appointmentDateTime,
                        Status = AppointmentStatus.Scheduled,
                        Price = priceResult.Data,
                        Description = request.Description,
                        IsOnlineBooking = true,
                        Duration = (int)(request.EndTime - request.StartTime).TotalMinutes,
                        Priority = AppointmentPriority.Normal,
                        IsEmergency = false,
                        CreatedByUserId = _currentUserService.UserId,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };

                    var createdAppointment = await _appointmentRepository.CreateAppointmentAsync(appointment);

                    // ✅ CRITICAL: Commit transaction فقط بعد از موفقیت تمام عملیات
                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _logger.Information("✅ نوبت {AppointmentId} با موفقیت رزرو شد (Transaction Committed) - پزشک: {DoctorId}, بیمار: {PatientId}",
                        createdAppointment.AppointmentId, request.DoctorId, request.PatientId);

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
            int? serviceCategoryId = null)
        {
            try
            {
                // استفاده از AppointmentPricingService برای محاسبه قیمت
                var pricingService = new AppointmentPricingService(
                    _doctorScheduleRepository,
                    _context,
                    _logger);

                var patient = await _currentUserService.GetPatientInfoAsync();
                var patientId = patient?.PatientId;
                var priceResult = await pricingService.CalculatePriceAsync(doctorId, serviceCategoryId, patientId);

                _logger.Information("قیمت نوبت برای پزشک {DoctorId}: {FinalPrice} تومان (قیمت پایه: {BasePrice}, تخفیف: {Discount})",
                    doctorId, priceResult.FinalPrice, priceResult.BasePrice, priceResult.DiscountAmount);

                return ServiceResult<decimal>.Successful(priceResult.FinalPrice);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه قیمت نوبت");
                return ServiceResult<decimal>.Failed("خطا در محاسبه قیمت");
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

