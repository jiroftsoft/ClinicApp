using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس تخصصی برای مدیریت برنامه کاری پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت برنامه‌های کاری پزشکان
    /// 2. رعایت استانداردهای پزشکی ایران در برنامه‌ریزی نوبت‌دهی
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 6. محاسبه خودکار زمان‌های در دسترس برای نوبت‌دهی
    /// 7. مدیریت مسدودیت‌های زمانی (مرخصی، جلسات)
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IDoctorCrudRepository _doctorRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorScheduleViewModel> _validator;
        private readonly ILogger _logger;

        public DoctorScheduleService(
            IDoctorScheduleRepository doctorScheduleRepository,
            IDoctorCrudRepository doctorRepository,
            ICurrentUserService currentUserService,
            IValidator<DoctorScheduleViewModel> validator
            )
        {
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = Log.ForContext<DoctorScheduleService>();
        }

        #region List and Search Operations

        /// <summary>
        /// دریافت لیست تمام برنامه‌های کاری پزشکان با صفحه‌بندی
        /// </summary>
        public async Task<ServiceResult<PagedResult<DoctorScheduleViewModel>>> GetAllDoctorSchedulesAsync(string searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست برنامه‌های کاری پزشکان. Page: {Page}, PageSize: {PageSize}", pageNumber, pageSize);

                // اعتبارسنجی پارامترها
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                // ✅ دریافت تمام برنامه‌های کاری
                var schedules = await _doctorScheduleRepository.GetAllDoctorSchedulesAsync();

                // ✅ فیلتر بر اساس عبارت جستجو (با Null Safety)
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    schedules = schedules.Where(s => 
                        (s.Doctor != null && (
                            (!string.IsNullOrEmpty(s.Doctor.FirstName) && s.Doctor.FirstName.Contains(searchTerm)) ||
                            (!string.IsNullOrEmpty(s.Doctor.LastName) && s.Doctor.LastName.Contains(searchTerm)) ||
                            (!string.IsNullOrEmpty(s.Doctor.FullName) && s.Doctor.FullName.Contains(searchTerm))
                        ))
                    ).ToList();
                }

                // ✅ تبدیل به ViewModel با Null Safety
                var viewModels = new List<DoctorScheduleViewModel>();
                foreach (var schedule in schedules)
                {
                    try
                    {
                        var viewModel = DoctorScheduleViewModel.FromEntity(schedule);
                        if (viewModel != null)
                        {
                            viewModels.Add(viewModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        // ✅ Log کردن خطا در تبدیل ViewModel اما ادامه دادن
                        _logger.Warning(ex, "خطا در تبدیل برنامه کاری {ScheduleId} به ViewModel. این مورد نادیده گرفته می‌شود.", schedule?.ScheduleId ?? 0);
                    }
                }

                // اعمال صفحه‌بندی
                var totalItems = viewModels.Count;
                var pagedItems = viewModels
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResult<DoctorScheduleViewModel>(pagedItems, totalItems, pageNumber, pageSize);

                _logger.Information("لیست برنامه‌های کاری با موفقیت آماده شد. TotalItems: {TotalItems}", totalItems);

                return ServiceResult<PagedResult<DoctorScheduleViewModel>>.Successful(pagedResult);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning(ex, "خطای عملیاتی در دریافت لیست برنامه‌های کاری پزشکان: {Message}", ex.Message);
                return ServiceResult<PagedResult<DoctorScheduleViewModel>>.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در دریافت لیست برنامه‌های کاری پزشکان");
                return ServiceResult<PagedResult<DoctorScheduleViewModel>>.Failed("خطا در دریافت لیست برنامه‌های کاری پزشکان. لطفاً دوباره تلاش کنید.");
            }
        }

        #endregion

        #region Scheduling & Availability (برنامه‌ریزی و زمان‌های در دسترس)

        /// <summary>
        /// تنظیم یا به‌روزرسانی برنامه کاری هفتگی یک پزشک
        /// </summary>
        public async Task<ServiceResult> SetDoctorScheduleAsync(int doctorId, DoctorScheduleViewModel schedule)
        {
            try
            {
                _logger.Information("درخواست تنظیم برنامه کاری پزشک با شناسه: {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                if (schedule == null)
                {
                    return ServiceResult.Failed("برنامه کاری نمی‌تواند خالی باشد.");
                }

                // تنظیم شناسه پزشک در مدل
                schedule.DoctorId = doctorId;

                // اعتبارسنجی مدل
                var validationResult = await _validator.ValidateAsync(schedule);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                    _logger.Warning("اعتبارسنجی مدل برنامه کاری پزشک ناموفق: {@Errors}", errors);
                    return ServiceResult.FailedWithValidationErrors("اطلاعات وارد شده صحیح نیست", errors);
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی وجود برنامه کاری قبلی
                var existingSchedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (existingSchedule != null)
                {
                    // به‌روزرسانی برنامه موجود
                    _logger.Information("به‌روزرسانی برنامه کاری موجود برای پزشک {DoctorId}", doctorId);
                    
                    // ✅ تبدیل ViewModel به Entity برای به‌روزرسانی کامل (شامل WorkDays و TimeRanges)
                    var scheduleEntity = schedule.ToEntity();
                    scheduleEntity.ScheduleId = existingSchedule.ScheduleId; // حفظ ScheduleId موجود
                    scheduleEntity.DoctorId = doctorId; // اطمینان از صحت DoctorId
                    scheduleEntity.UpdatedAt = DateTime.Now;
                    scheduleEntity.UpdatedByUserId = _currentUserService.UserId;

                    // ✅ به‌روزرسانی کامل (شامل WorkDays و TimeRanges)
                    await _doctorScheduleRepository.UpdateDoctorScheduleAsync(scheduleEntity);
                    
                    _logger.Information("برنامه کاری پزشک {DoctorId} با موفقیت به‌روزرسانی شد. WorkDays: {WorkDaysCount}, TimeRanges: {TimeRangesCount}", 
                        doctorId, 
                        schedule.WorkDays?.Count ?? 0,
                        schedule.WorkDays?.Sum(w => w.TimeRanges?.Count ?? 0) ?? 0);
                }
                else
                {
                    // ایجاد برنامه جدید
                    _logger.Information("ایجاد برنامه کاری جدید برای پزشک {DoctorId}", doctorId);
                    
                    // تبدیل ViewModel به Entity
                    var doctorSchedule = schedule.ToEntity();
                    
                    // تنظیم اطلاعات ردیابی
                    var currentUserId = _currentUserService.UserId;
                    doctorSchedule.CreatedByUserId = currentUserId;
                    doctorSchedule.UpdatedByUserId = currentUserId;
                    doctorSchedule.CreatedAt = DateTime.Now;
                    doctorSchedule.UpdatedAt = DateTime.Now;

                    // ذخیره در دیتابیس
                    var createdSchedule = await _doctorScheduleRepository.AddDoctorScheduleAsync(doctorSchedule);
                    
                                         // ذخیره روزهای کاری (این عملیات در repository انجام می‌شود)
                }

                _logger.Information("برنامه کاری پزشک {DoctorId} با موفقیت تنظیم شد", doctorId);

                return ServiceResult.Successful("برنامه کاری پزشک با موفقیت تنظیم شد.");
            }
            catch (InvalidOperationException ex)
            {
                // ✅ مدیریت خطاهای عملیاتی (مثل تداخل بازه‌های زمانی)
                _logger.Warning(ex, "خطای عملیاتی در تنظیم برنامه کاری پزشک {DoctorId}: {Message}", doctorId, ex.Message);
                return ServiceResult.Failed(ex.Message); // بازگرداندن پیام خطای اصلی
            }
            catch (ArgumentException ex)
            {
                // ✅ مدیریت خطاهای اعتبارسنجی پارامترها
                _logger.Warning(ex, "خطای اعتبارسنجی در تنظیم برنامه کاری پزشک {DoctorId}: {Message}", doctorId, ex.Message);
                return ServiceResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                // ✅ مدیریت خطاهای غیرمنتظره
                _logger.Error(ex, "خطای غیرمنتظره در تنظیم برنامه کاری پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در تنظیم برنامه کاری پزشک. لطفاً با پشتیبانی تماس بگیرید.");
            }
        }

        /// <summary>
        /// دریافت برنامه کاری هفتگی یک پزشک
        /// </summary>
        public async Task<ServiceResult<DoctorScheduleViewModel>> GetDoctorScheduleAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت برنامه کاری پزشک با شناسه: {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorScheduleViewModel>.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<DoctorScheduleViewModel>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // ✅ دریافت برنامه کاری با جزئیات کامل
                _logger.Information("🔍 [GetDoctorScheduleAsync] در حال فراخوانی GetDoctorScheduleWithAllDetailsAsync برای پزشک {DoctorId}", doctorId);
                System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] در حال فراخوانی GetDoctorScheduleWithAllDetailsAsync برای پزشک {doctorId}");
                
                var doctorSchedule = await _doctorScheduleRepository.GetDoctorScheduleWithAllDetailsAsync(doctorId);
                
                if (doctorSchedule == null)
                {
                    _logger.Information("ℹ️ [GetDoctorScheduleAsync] برنامه کاری برای پزشک {DoctorId} یافت نشد (null)", doctorId);
                    System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] برنامه کاری برای پزشک {doctorId} یافت نشد (null)");
                    return ServiceResult<DoctorScheduleViewModel>.Successful(null);
                }
                
                // ✅ لود کردن Navigation Property Doctor به صورت جداگانه برای جلوگیری از خطای SQL
                // ✅ این کار به دلیل حذف .Include(ds => ds.Doctor) از Repository انجام می‌شود
                if (doctorSchedule.Doctor == null && doctorSchedule.DoctorId > 0)
                {
                    _logger.Information("🔄 [GetDoctorScheduleAsync] در حال لود کردن Navigation Property Doctor برای پزشک {DoctorId}", doctorId);
                    doctorSchedule.Doctor = await _doctorRepository.GetByIdAsync(doctorSchedule.DoctorId);
                }

                // ✅ لاگ اطلاعات برای دیباگ
                _logger.Information("✅ [GetDoctorScheduleAsync] برنامه کاری از Repository دریافت شد. ScheduleId: {ScheduleId}, WorkDaysCount: {WorkDaysCount}, TimeRangesCount: {TimeRangesCount}", 
                    doctorSchedule.ScheduleId,
                    doctorSchedule.WorkDays?.Count ?? 0,
                    doctorSchedule.WorkDays?.Sum(w => w.TimeRanges?.Count ?? 0) ?? 0);
                System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] برنامه کاری از Repository دریافت شد. ScheduleId: {doctorSchedule.ScheduleId}, WorkDaysCount: {doctorSchedule.WorkDays?.Count ?? 0}");

                // ✅ لاگ جزئیات WorkDays و TimeRanges
                if (doctorSchedule.WorkDays != null)
                {
                    foreach (var workDay in doctorSchedule.WorkDays)
                    {
                        _logger.Information("📅 [GetDoctorScheduleAsync] WorkDay: DayOfWeek={DayOfWeek}, IsActive={IsActive}, TimeRangesCount={TimeRangesCount}", 
                            workDay.DayOfWeek, workDay.IsActive, workDay.TimeRanges?.Count ?? 0);
                        System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] WorkDay: DayOfWeek={workDay.DayOfWeek}, IsActive={workDay.IsActive}, TimeRangesCount={workDay.TimeRanges?.Count ?? 0}");
                        
                        if (workDay.TimeRanges != null)
                        {
                            foreach (var timeRange in workDay.TimeRanges)
                            {
                                _logger.Information("⏰ [GetDoctorScheduleAsync] TimeRange: StartTime={StartTime}, EndTime={EndTime}, IsDeleted={IsDeleted}", 
                                    timeRange.StartTime, timeRange.EndTime, timeRange.IsDeleted);
                                System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] TimeRange: StartTime={timeRange.StartTime}, EndTime={timeRange.EndTime}, IsDeleted={timeRange.IsDeleted}");
                            }
                        }
                    }
                }

                // ✅ تبدیل به ViewModel با Null Check و Error Handling کامل
                DoctorScheduleViewModel scheduleViewModel;
                try
                {
                    _logger.Information("🔄 [GetDoctorScheduleAsync] در حال تبدیل Entity به ViewModel برای پزشک {DoctorId}", doctorId);
                    System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] در حال تبدیل Entity به ViewModel برای پزشک {doctorId}");
                    
                    scheduleViewModel = DoctorScheduleViewModel.FromEntity(doctorSchedule);
                    
                    _logger.Information("🔄 [GetDoctorScheduleAsync] تبدیل Entity به ViewModel انجام شد. ViewModel is null: {IsNull}", scheduleViewModel == null);
                    System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] تبدیل Entity به ViewModel انجام شد. ViewModel is null: {scheduleViewModel == null}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "❌ [GetDoctorScheduleAsync] خطا در تبدیل برنامه کاری پزشک {DoctorId} به ViewModel. ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                        doctorId, ex.GetType().Name, ex.Message, ex.StackTrace);
                    System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] ❌ خطا در تبدیل: {ex.GetType().Name} - {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] StackTrace: {ex.StackTrace}");
                    
                    if (ex.InnerException != null)
                    {
                        _logger.Error(ex.InnerException, "❌ [GetDoctorScheduleAsync] InnerException: {Message}, Type: {Type}", 
                            ex.InnerException.Message, ex.InnerException.GetType().Name);
                        System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] InnerException: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                    }
                    
                    return ServiceResult<DoctorScheduleViewModel>.Failed("خطا در تبدیل داده‌های برنامه کاری. لطفاً دوباره تلاش کنید.");
                }
                
                if (scheduleViewModel == null)
                {
                    _logger.Information("ℹ️ [GetDoctorScheduleAsync] برنامه کاری پزشک {DoctorId} به ViewModel تبدیل نشد (null). احتمالاً داده‌های نامعتبر یا برنامه جدید. بازگرداندن null برای ایجاد مدل جدید", doctorId);
                    System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] برنامه کاری پزشک {doctorId} به ViewModel تبدیل نشد (null)");
                    // ✅ به جای خطا، null برمی‌گردانیم تا Controller بتواند مدل جدید ایجاد کند
                    // این یک خطا نیست، بلکه نشان می‌دهد که برنامه کاری وجود ندارد یا داده‌های نامعتبر است
                    return ServiceResult<DoctorScheduleViewModel>.Successful(null);
                }

                _logger.Information("✅ [GetDoctorScheduleAsync] برنامه کاری پزشک {DoctorId} با موفقیت دریافت شد. WorkDays: {WorkDaysCount}, TimeRanges: {TimeRangesCount}", 
                    doctorId, 
                    scheduleViewModel.WorkDays?.Count ?? 0,
                    scheduleViewModel.WorkDays?.Sum(w => w.TimeRanges?.Count ?? 0) ?? 0);
                System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleAsync] ✅ برنامه کاری پزشک {doctorId} با موفقیت دریافت شد. WorkDays: {scheduleViewModel.WorkDays?.Count ?? 0}, TimeRanges: {scheduleViewModel.WorkDays?.Sum(w => w.TimeRanges?.Count ?? 0) ?? 0}");

                return ServiceResult<DoctorScheduleViewModel>.Successful(scheduleViewModel);
            }
            catch (InvalidOperationException ex)
            {
                // ✅ مدیریت خطاهای عملیاتی - با جزئیات بیشتر برای لاگ
                _logger.Warning(ex, "خطای عملیاتی در دریافت برنامه کاری پزشک {DoctorId}: {Message}. StackTrace: {StackTrace}", 
                    doctorId, ex.Message, ex.StackTrace);
                
                // ✅ اگر خطا از Repository است، پیام بهتری نمایش می‌دهیم
                var errorMessage = ex.Message.Contains("خطا در دریافت") 
                    ? $"خطا در دریافت برنامه کاری پزشک {doctorId}. لطفاً دوباره تلاش کنید."
                    : ex.Message;
                
                return ServiceResult<DoctorScheduleViewModel>.Failed(errorMessage);
            }
            catch (Exception ex)
            {
                // ✅ مدیریت خطاهای غیرمنتظره - با جزئیات کامل برای لاگ
                _logger.Error(ex, "خطای غیرمنتظره در دریافت برنامه کاری پزشک {DoctorId}. ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                    doctorId, ex.GetType().Name, ex.Message, ex.StackTrace);
                
                if (ex.InnerException != null)
                {
                    _logger.Error(ex.InnerException, "InnerException برای DoctorId {DoctorId}: {Message}", doctorId, ex.InnerException.Message);
                }
                
                return ServiceResult<DoctorScheduleViewModel>.Failed($"خطا در دریافت برنامه کاری پزشک {doctorId}. لطفاً دوباره تلاش کنید.");
            }
        }

        /// <summary>
        /// مسدود کردن یک بازه زمانی برای پزشک (مثلا برای مرخصی یا جلسه)
        /// </summary>
        public async Task<ServiceResult> BlockTimeRangeForDoctorAsync(int doctorId, DateTime start, DateTime end, string reason)
        {
            try
            {
                _logger.Information("درخواست مسدود کردن بازه زمانی برای پزشک {DoctorId} از {Start} تا {End}", doctorId, start, end);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                if (start >= end)
                {
                    return ServiceResult.Failed("زمان شروع باید قبل از زمان پایان باشد.");
                }

                if (start < DateTime.Now)
                {
                    return ServiceResult.Failed("زمان شروع نمی‌تواند در گذشته باشد.");
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    return ServiceResult.Failed("دلیل مسدودیت الزامی است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // مسدود کردن بازه زمانی
                await _doctorScheduleRepository.BlockTimeRangeForDoctorAsync(doctorId, start, end, reason);

                _logger.Information("بازه زمانی برای پزشک {DoctorId} با موفقیت مسدود شد", doctorId);

                return ServiceResult.Successful("بازه زمانی با موفقیت مسدود شد.");
            }
            catch (InvalidOperationException ex)
            {
                // ✅ مدیریت خطاهای عملیاتی (مثل وجود نوبت‌های رزرو شده)
                _logger.Warning(ex, "خطای عملیاتی در مسدود کردن بازه زمانی برای پزشک {DoctorId}: {Message}", doctorId, ex.Message);
                return ServiceResult.Failed(ex.Message);
            }
            catch (ArgumentException ex)
            {
                // ✅ مدیریت خطاهای اعتبارسنجی پارامترها
                _logger.Warning(ex, "خطای اعتبارسنجی در مسدود کردن بازه زمانی برای پزشک {DoctorId}: {Message}", doctorId, ex.Message);
                return ServiceResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                // ✅ مدیریت خطاهای غیرمنتظره
                _logger.Error(ex, "خطای غیرمنتظره در مسدود کردن بازه زمانی برای پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در مسدود کردن بازه زمانی. لطفاً دوباره تلاش کنید.");
            }
        }

        /// <summary>
        /// محاسبه و بازگرداندن تمام اسلات‌های زمانی خالی و قابل رزرو برای یک پزشک در یک روز مشخص
        /// </summary>
        public async Task<ServiceResult<List<TimeSlotViewModel>>> GetAvailableAppointmentSlotsAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("درخواست دریافت اسلات‌های در دسترس برای پزشک {DoctorId} در تاریخ {Date}", doctorId, date.ToString("yyyy/MM/dd"));

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<List<TimeSlotViewModel>>.Failed("شناسه پزشک نامعتبر است.");
                }

                if (date.Date < DateTime.Today)
                {
                    return ServiceResult<List<TimeSlotViewModel>>.Failed("تاریخ مورد نظر نمی‌تواند در گذشته باشد.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<List<TimeSlotViewModel>>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت اسلات‌های در دسترس
                var availableSlots = await _doctorScheduleRepository.GetAvailableAppointmentSlotsAsync(doctorId, date);

                // تبدیل به ViewModel - فقط از properties موجود استفاده می‌کنیم
                var timeSlotViewModels = availableSlots.Select(slot => new TimeSlotViewModel
                {
                    SlotId = slot.TimeSlotId, // استفاده از TimeSlotId موجود
                    SlotDate = slot.AppointmentDate, // استفاده از AppointmentDate موجود
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    Duration = slot.Duration, // استفاده از Duration موجود
                    Price = 0, // مقدار پیش‌فرض - در آینده می‌توان از جدول جداگانه استفاده کرد
                    Status = slot.Status.ToString(), // استفاده از AppointmentStatus موجود
                    IsAvailable = slot.Status == AppointmentStatus.Available, // مقایسه صحیح enum ها
                    IsEmergencySlot = false, // مقدار پیش‌فرض - در آینده می‌توان اضافه کرد
                    IsWalkInAllowed = false, // مقدار پیش‌فرض - در آینده می‌توان اضافه کرد
                    Priority = "عادی", // مقدار پیش‌فرض - در آینده می‌توان اضافه کرد
                    DoctorName = doctor?.FullName ?? "نامشخص",
                    Specialization = doctor?.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "نامشخص", // استفاده از navigation property صحیح
                    ClinicName = doctor?.Clinic?.Name,
                    ClinicAddress = doctor?.Clinic?.Address
                }).ToList();

                _logger.Information("اسلات‌های در دسترس برای پزشک {DoctorId} در تاریخ {Date} با موفقیت دریافت شد. تعداد: {Count}", 
                    doctorId, date.ToString("yyyy/MM/dd"), timeSlotViewModels.Count);

                return ServiceResult<List<TimeSlotViewModel>>.Successful(timeSlotViewModels);
            }
            catch (InvalidOperationException ex)
            {
                // ✅ مدیریت خطاهای عملیاتی
                _logger.Warning(ex, "خطای عملیاتی در دریافت اسلات‌های در دسترس برای پزشک {DoctorId} در تاریخ {Date}: {Message}", 
                    doctorId, date.ToString("yyyy/MM/dd"), ex.Message);
                return ServiceResult<List<TimeSlotViewModel>>.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                // ✅ مدیریت خطاهای غیرمنتظره
                _logger.Error(ex, "خطای غیرمنتظره در دریافت اسلات‌های در دسترس برای پزشک {DoctorId} در تاریخ {Date}", 
                    doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<List<TimeSlotViewModel>>.Failed("خطا در دریافت اسلات‌های در دسترس. لطفاً دوباره تلاش کنید.");
            }
        }

        #endregion

        #region Schedule Management Operations

        /// <summary>
        /// دریافت برنامه کاری بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<DoctorScheduleViewModel>> GetDoctorScheduleByIdAsync(int scheduleId)
        {
            try
            {
                _logger.Information("درخواست دریافت برنامه کاری {ScheduleId}", scheduleId);

                if (scheduleId <= 0)
                {
                    return ServiceResult<DoctorScheduleViewModel>.Failed("شناسه برنامه کاری نامعتبر است.");
                }

                // دریافت برنامه کاری
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleByIdAsync(scheduleId);
                if (schedule == null)
                {
                    _logger.Warning("برنامه کاری با شناسه {ScheduleId} یافت نشد", scheduleId);
                    return ServiceResult<DoctorScheduleViewModel>.Failed("برنامه کاری مورد نظر یافت نشد.");
                }

                // ✅ لود کردن Navigation Property Doctor به صورت جداگانه برای جلوگیری از خطای SQL
                if (schedule.Doctor == null && schedule.DoctorId > 0)
                {
                    schedule.Doctor = await _doctorRepository.GetByIdAsync(schedule.DoctorId);
                }

                // تبدیل به ViewModel
                var viewModel = DoctorScheduleViewModel.FromEntity(schedule);
                if (viewModel == null)
                {
                    _logger.Error("خطا در تبدیل برنامه کاری {ScheduleId} به ViewModel", scheduleId);
                    return ServiceResult<DoctorScheduleViewModel>.Failed("خطا در تبدیل داده‌ها");
                }

                _logger.Information("برنامه کاری {ScheduleId} با موفقیت دریافت شد", scheduleId);
                return ServiceResult<DoctorScheduleViewModel>.Successful(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning(ex, "خطای عملیاتی در دریافت برنامه کاری {ScheduleId}: {Message}", scheduleId, ex.Message);
                return ServiceResult<DoctorScheduleViewModel>.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در دریافت برنامه کاری {ScheduleId}", scheduleId);
                return ServiceResult<DoctorScheduleViewModel>.Failed("خطا در دریافت برنامه کاری. لطفاً دوباره تلاش کنید.");
            }
        }

        /// <summary>
        /// حذف برنامه کاری
        /// </summary>
        public async Task<ServiceResult> DeleteDoctorScheduleAsync(int scheduleId)
        {
            try
            {
                _logger.Information("درخواست حذف برنامه کاری {ScheduleId} توسط کاربر {UserId}", scheduleId, _currentUserService.UserId);

                if (scheduleId <= 0)
                {
                    return ServiceResult.Failed("شناسه برنامه کاری نامعتبر است.");
                }

                // بررسی وجود برنامه کاری
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleByIdAsync(scheduleId);
                if (schedule == null)
                {
                    _logger.Warning("برنامه کاری با شناسه {ScheduleId} یافت نشد", scheduleId);
                    return ServiceResult.Failed("برنامه کاری مورد نظر یافت نشد.");
                }

                // حذف برنامه کاری
                await _doctorScheduleRepository.DeleteDoctorScheduleAsync(scheduleId);

                _logger.Information("برنامه کاری {ScheduleId} با موفقیت حذف شد", scheduleId);
                return ServiceResult.Successful("برنامه کاری با موفقیت حذف شد.");
            }
            catch (InvalidOperationException ex)
            {
                // ✅ مدیریت خطاهای عملیاتی (مثل وجود نوبت‌های فعال)
                _logger.Warning(ex, "خطای عملیاتی در حذف برنامه کاری {ScheduleId}: {Message}", scheduleId, ex.Message);
                return ServiceResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در حذف برنامه کاری {ScheduleId}", scheduleId);
                return ServiceResult.Failed("خطا در حذف برنامه کاری. لطفاً دوباره تلاش کنید.");
            }
        }

        /// <summary>
        /// غیرفعال کردن برنامه کاری
        /// </summary>
        public async Task<ServiceResult> DeactivateDoctorScheduleAsync(int scheduleId)
        {
            try
            {
                _logger.Information("درخواست غیرفعال کردن برنامه کاری {ScheduleId} توسط کاربر {UserId}", scheduleId, _currentUserService.UserId);

                if (scheduleId <= 0)
                {
                    return ServiceResult.Failed("شناسه برنامه کاری نامعتبر است.");
                }

                // بررسی وجود برنامه کاری
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleByIdAsync(scheduleId);
                if (schedule == null)
                {
                    _logger.Warning("برنامه کاری با شناسه {ScheduleId} یافت نشد", scheduleId);
                    return ServiceResult.Failed("برنامه کاری مورد نظر یافت نشد.");
                }

                // غیرفعال کردن برنامه کاری
                await _doctorScheduleRepository.DeactivateDoctorScheduleAsync(scheduleId);

                _logger.Information("برنامه کاری {ScheduleId} با موفقیت غیرفعال شد", scheduleId);
                return ServiceResult.Successful("برنامه کاری با موفقیت غیرفعال شد.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning(ex, "خطای عملیاتی در غیرفعال کردن برنامه کاری {ScheduleId}: {Message}", scheduleId, ex.Message);
                return ServiceResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در غیرفعال کردن برنامه کاری {ScheduleId}", scheduleId);
                return ServiceResult.Failed("خطا در غیرفعال کردن برنامه کاری. لطفاً دوباره تلاش کنید.");
            }
        }

        /// <summary>
        /// فعال کردن مجدد برنامه کاری
        /// </summary>
        public async Task<ServiceResult> ActivateDoctorScheduleAsync(int scheduleId)
        {
            try
            {
                _logger.Information("درخواست فعال کردن مجدد برنامه کاری {ScheduleId} توسط کاربر {UserId}", scheduleId, _currentUserService.UserId);

                if (scheduleId <= 0)
                {
                    return ServiceResult.Failed("شناسه برنامه کاری نامعتبر است.");
                }

                // بررسی وجود برنامه کاری
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleByIdAsync(scheduleId);
                if (schedule == null)
                {
                    _logger.Warning("برنامه کاری با شناسه {ScheduleId} یافت نشد", scheduleId);
                    return ServiceResult.Failed("برنامه کاری مورد نظر یافت نشد.");
                }

                // فعال کردن مجدد برنامه کاری
                await _doctorScheduleRepository.ActivateDoctorScheduleAsync(scheduleId);

                _logger.Information("برنامه کاری {ScheduleId} با موفقیت فعال شد", scheduleId);
                return ServiceResult.Successful("برنامه کاری با موفقیت فعال شد.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning(ex, "خطای عملیاتی در فعال کردن مجدد برنامه کاری {ScheduleId}: {Message}", scheduleId, ex.Message);
                return ServiceResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در فعال کردن مجدد برنامه کاری {ScheduleId}", scheduleId);
                return ServiceResult.Failed("خطا در فعال کردن مجدد برنامه کاری. لطفاً دوباره تلاش کنید.");
            }
        }

        /// <summary>
        /// تولید و ذخیره اسلات‌های زمانی برای یک پزشک در دیتابیس
        /// </summary>
        public async Task<ServiceResult> GenerateAndSaveTimeSlotsAsync(int doctorId, int scheduleId, int daysAhead = 90)
        {
            try
            {
                _logger.Information("درخواست تولید اسلات‌های زمانی - DoctorId: {DoctorId}, ScheduleId: {ScheduleId}, DaysAhead: {DaysAhead}",
                    doctorId, scheduleId, daysAhead);

                if (doctorId <= 0 || scheduleId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک یا برنامه کاری نامعتبر است.");
                }

                // بررسی وجود برنامه کاری
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleByIdAsync(scheduleId);
                if (schedule == null || schedule.DoctorId != doctorId)
                {
                    _logger.Warning("برنامه کاری با شناسه {ScheduleId} برای پزشک {DoctorId} یافت نشد", scheduleId, doctorId);
                    return ServiceResult.Failed("برنامه کاری مورد نظر یافت نشد.");
                }

                // تولید و ذخیره اسلات‌های زمانی
                await _doctorScheduleRepository.GenerateAndSaveTimeSlotsAsync(doctorId, scheduleId, daysAhead);

                _logger.Information("اسلات‌های زمانی با موفقیت تولید شدند - DoctorId: {DoctorId}, ScheduleId: {ScheduleId}",
                    doctorId, scheduleId);
                return ServiceResult.Successful("اسلات‌های زمانی با موفقیت تولید شدند.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning(ex, "خطای عملیاتی در تولید اسلات‌های زمانی - DoctorId: {DoctorId}, ScheduleId: {ScheduleId}: {Message}",
                    doctorId, scheduleId, ex.Message);
                return ServiceResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در تولید اسلات‌های زمانی - DoctorId: {DoctorId}, ScheduleId: {ScheduleId}",
                    doctorId, scheduleId);
                return ServiceResult.Failed($"خطا در تولید اسلات‌های زمانی: {ex.Message}");
            }
        }

        #endregion


    }
}
