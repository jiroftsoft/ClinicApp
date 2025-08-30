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
                    
                    // به‌روزرسانی فیلدها
                    existingSchedule.AppointmentDuration = schedule.AppointmentDuration;
                    existingSchedule.DefaultStartTime = schedule.DefaultStartTime;
                    existingSchedule.DefaultEndTime = schedule.DefaultEndTime;
                    existingSchedule.UpdatedAt = DateTime.Now;
                    existingSchedule.UpdatedByUserId = _currentUserService.UserId;

                    // به‌روزرسانی روزهای کاری
                    await _doctorScheduleRepository.UpdateDoctorScheduleAsync(existingSchedule);
                    
                                         // به‌روزرسانی روزهای کاری (این عملیات در repository انجام می‌شود)
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
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم برنامه کاری پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در تنظیم برنامه کاری پزشک");
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

                // دریافت برنامه کاری
                var doctorSchedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (doctorSchedule == null)
                {
                    _logger.Information("برنامه کاری برای پزشک {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<DoctorScheduleViewModel>.Successful(null);
                }

                // تبدیل به ViewModel
                var scheduleViewModel = DoctorScheduleViewModel.FromEntity(doctorSchedule);

                _logger.Information("برنامه کاری پزشک {DoctorId} با موفقیت دریافت شد", doctorId);

                return ServiceResult<DoctorScheduleViewModel>.Successful(scheduleViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت برنامه کاری پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorScheduleViewModel>.Failed("خطا در دریافت برنامه کاری پزشک");
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
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مسدود کردن بازه زمانی برای پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در مسدود کردن بازه زمانی");
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

                // تبدیل به ViewModel
                var timeSlotViewModels = availableSlots.Select(TimeSlotViewModel.FromEntity).ToList();

                _logger.Information("اسلات‌های در دسترس برای پزشک {DoctorId} در تاریخ {Date} با موفقیت دریافت شد. تعداد: {Count}", 
                    doctorId, date.ToString("yyyy/MM/dd"), timeSlotViewModels.Count);

                return ServiceResult<List<TimeSlotViewModel>>.Successful(timeSlotViewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های در دسترس برای پزشک {DoctorId} در تاریخ {Date}", doctorId, date.ToString("yyyy/MM/dd"));
                return ServiceResult<List<TimeSlotViewModel>>.Failed("خطا در دریافت اسلات‌های در دسترس");
            }
        }

        #endregion


    }
}
