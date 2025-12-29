using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Admin.TimeSlotManagement;
using Serilog;
using ClinicApp.Core;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس تخصصی برای مدیریت اسلات‌های زمانی پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت اسلات‌های زمانی
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت نوبت‌دهی
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// 5. پشتیبانی از وضعیت‌های مختلف نوبت (در دسترس، رزرو شده، تکمیل شده)
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorTimeSlotService : IDoctorTimeSlotService
    {
        private readonly IDoctorTimeSlotRepository _timeSlotRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public DoctorTimeSlotService(
            IDoctorTimeSlotRepository timeSlotRepository,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _timeSlotRepository = timeSlotRepository ?? throw new ArgumentNullException(nameof(timeSlotRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<DoctorTimeSlotService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Query Operations

        /// <summary>
        /// دریافت اسلات‌های زمانی با فیلتر و صفحه‌بندی
        /// </summary>
        public async Task<ServiceResult<PagedResult<TimeSlotIndexViewModel>>> GetTimeSlotsAsync(TimeSlotFilterViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, StartDate: {StartDate}, EndDate: {EndDate}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
                    filter.DoctorId, filter.StartDate, filter.EndDate, filter.Status, filter.PageNumber, filter.PageSize);

                // ✅ اعتبارسنجی و تنظیم مقادیر پیش‌فرض
                filter.ValidateAndSetDefaults();

                // ✅ دریافت اسلات‌ها از Repository
                var (items, totalCount) = await _timeSlotRepository.GetTimeSlotsAsync(
                    filter.DoctorId,
                    filter.StartDate,
                    filter.EndDate,
                    filter.Status,
                    filter.PageNumber,
                    filter.PageSize,
                    filter.SearchTerm);

                // ✅ تبدیل به ViewModel
                var viewModels = items
                    .Select(TimeSlotIndexViewModel.FromEntity)
                    .Where(vm => vm != null)
                    .ToList();

                // ✅ ایجاد نتیجه صفحه‌بندی شده
                var pagedResult = new PagedResult<TimeSlotIndexViewModel>(
                    viewModels,
                    totalCount,
                    filter.PageNumber,
                    filter.PageSize);

                _logger.Information("اسلات‌های زمانی با موفقیت دریافت شدند - TotalItems: {TotalItems}, PageItems: {PageItems}",
                    totalCount, viewModels.Count);

                return ServiceResult<PagedResult<TimeSlotIndexViewModel>>.Successful(
                    pagedResult,
                    "اسلات‌های زمانی با موفقیت دریافت شدند.",
                    "GetTimeSlots",
                    _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های زمانی");
                return ServiceResult<PagedResult<TimeSlotIndexViewModel>>.Failed(
                    $"خطا در دریافت اسلات‌های زمانی: {ex.Message}",
                    "GetTimeSlots");
            }
        }

        /// <summary>
        /// دریافت اسلات زمانی بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<TimeSlotDetailsViewModel>> GetTimeSlotByIdAsync(int timeSlotId)
        {
            try
            {
                _logger.Information("درخواست دریافت اسلات زمانی - TimeSlotId: {TimeSlotId}", timeSlotId);

                if (timeSlotId <= 0)
                {
                    return ServiceResult<TimeSlotDetailsViewModel>.Failed(
                        "شناسه اسلات زمانی نامعتبر است.",
                        "GetTimeSlotById");
                }

                var timeSlot = await _timeSlotRepository.GetTimeSlotByIdAsync(timeSlotId);

                if (timeSlot == null)
                {
                    _logger.Warning("اسلات زمانی {TimeSlotId} یافت نشد", timeSlotId);
                    return ServiceResult<TimeSlotDetailsViewModel>.Failed(
                        "اسلات زمانی مورد نظر یافت نشد.",
                        "GetTimeSlotById");
                }

                var viewModel = TimeSlotDetailsViewModel.FromEntity(timeSlot);

                _logger.Information("اسلات زمانی {TimeSlotId} با موفقیت دریافت شد", timeSlotId);

                return ServiceResult<TimeSlotDetailsViewModel>.Successful(
                    viewModel,
                    "اسلات زمانی با موفقیت دریافت شد.",
                    "GetTimeSlotById",
                    _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات زمانی {TimeSlotId}", timeSlotId);
                return ServiceResult<TimeSlotDetailsViewModel>.Failed(
                    $"خطا در دریافت اسلات زمانی: {ex.Message}",
                    "GetTimeSlotById");
            }
        }

        /// <summary>
        /// دریافت اسلات‌های زمانی یک پزشک در یک تاریخ خاص
        /// </summary>
        public async Task<ServiceResult<List<TimeSlotIndexViewModel>>> GetTimeSlotsByDoctorAndDateAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("درخواست دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, Date: {Date}", doctorId, date);

                if (doctorId <= 0)
                {
                    return ServiceResult<List<TimeSlotIndexViewModel>>.Failed(
                        "شناسه پزشک نامعتبر است.",
                        "GetTimeSlotsByDoctorAndDate");
                }

                var slots = await _timeSlotRepository.GetTimeSlotsByDoctorAndDateAsync(doctorId, date);

                var viewModels = slots
                    .Select(TimeSlotIndexViewModel.FromEntity)
                    .Where(vm => vm != null)
                    .ToList();

                _logger.Information("اسلات‌های زمانی با موفقیت دریافت شدند - Count: {Count}", viewModels.Count);

                return ServiceResult<List<TimeSlotIndexViewModel>>.Successful(
                    viewModels,
                    "اسلات‌های زمانی با موفقیت دریافت شدند.",
                    "GetTimeSlotsByDoctorAndDate",
                    _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, Date: {Date}", doctorId, date);
                return ServiceResult<List<TimeSlotIndexViewModel>>.Failed(
                    $"خطا در دریافت اسلات‌های زمانی: {ex.Message}",
                    "GetTimeSlotsByDoctorAndDate");
            }
        }

        /// <summary>
        /// دریافت آمار اسلات‌های زمانی
        /// </summary>
        public async Task<ServiceResult<TimeSlotStatisticsViewModel>> GetTimeSlotStatisticsAsync(
            int? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                _logger.Information("درخواست دریافت آمار اسلات‌های زمانی - DoctorId: {DoctorId}, StartDate: {StartDate}, EndDate: {EndDate}",
                    doctorId, startDate, endDate);

                var statistics = await _timeSlotRepository.GetTimeSlotStatisticsAsync(doctorId, startDate, endDate);

                var viewModel = TimeSlotStatisticsViewModel.FromStatistics(statistics);

                _logger.Information("آمار اسلات‌های زمانی با موفقیت دریافت شد - Total: {Total}, Available: {Available}, Booked: {Booked}",
                    viewModel.TotalSlots, viewModel.AvailableSlots, viewModel.BookedSlots);

                return ServiceResult<TimeSlotStatisticsViewModel>.Successful(
                    viewModel,
                    "آمار اسلات‌های زمانی با موفقیت دریافت شد.",
                    "GetTimeSlotStatistics",
                    _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار اسلات‌های زمانی");
                return ServiceResult<TimeSlotStatisticsViewModel>.Failed(
                    $"خطا در دریافت آمار اسلات‌های زمانی: {ex.Message}",
                    "GetTimeSlotStatistics");
            }
        }

        #endregion

        #region Management Operations

        /// <summary>
        /// حذف نرم اسلات زمانی
        /// </summary>
        public async Task<ServiceResult> SoftDeleteTimeSlotAsync(int timeSlotId)
        {
            try
            {
                _logger.Information("درخواست حذف اسلات زمانی - TimeSlotId: {TimeSlotId} توسط کاربر {UserId}",
                    timeSlotId, _currentUserService.UserId);

                if (timeSlotId <= 0)
                {
                    return ServiceResult.Failed(
                        "شناسه اسلات زمانی نامعتبر است.",
                        "SoftDeleteTimeSlot");
                }

                var result = await _timeSlotRepository.SoftDeleteTimeSlotAsync(timeSlotId, _currentUserService.UserId);

                if (!result)
                {
                    _logger.Warning("حذف اسلات زمانی {TimeSlotId} ناموفق بود", timeSlotId);
                    return ServiceResult.Failed(
                        "حذف اسلات زمانی ناموفق بود.",
                        "SoftDeleteTimeSlot");
                }

                _logger.Information("اسلات زمانی {TimeSlotId} با موفقیت حذف شد (Soft Delete)", timeSlotId);

                return ServiceResult.Successful(
                    "اسلات زمانی با موفقیت حذف شد.",
                    "SoftDeleteTimeSlot",
                    _currentUserService.UserId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning(ex, "خطا در حذف اسلات زمانی {TimeSlotId} - {Message}", timeSlotId, ex.Message);
                return ServiceResult.Failed(
                    ex.Message,
                    "SoftDeleteTimeSlot");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اسلات زمانی {TimeSlotId}", timeSlotId);
                return ServiceResult.Failed(
                    $"خطا در حذف اسلات زمانی: {ex.Message}",
                    "SoftDeleteTimeSlot");
            }
        }

        /// <summary>
        /// تغییر وضعیت اسلات زمانی
        /// </summary>
        public async Task<ServiceResult> UpdateTimeSlotStatusAsync(int timeSlotId, AppointmentStatus status)
        {
            try
            {
                _logger.Information("درخواست تغییر وضعیت اسلات زمانی - TimeSlotId: {TimeSlotId}, Status: {Status} توسط کاربر {UserId}",
                    timeSlotId, status, _currentUserService.UserId);

                if (timeSlotId <= 0)
                {
                    return ServiceResult.Failed(
                        "شناسه اسلات زمانی نامعتبر است.",
                        "UpdateTimeSlotStatus");
                }

                var result = await _timeSlotRepository.UpdateTimeSlotStatusAsync(timeSlotId, status, _currentUserService.UserId);

                if (!result)
                {
                    _logger.Warning("تغییر وضعیت اسلات زمانی {TimeSlotId} ناموفق بود", timeSlotId);
                    return ServiceResult.Failed(
                        "تغییر وضعیت اسلات زمانی ناموفق بود.",
                        "UpdateTimeSlotStatus");
                }

                _logger.Information("وضعیت اسلات زمانی {TimeSlotId} به {Status} تغییر یافت", timeSlotId, status);

                return ServiceResult.Successful(
                    "وضعیت اسلات زمانی با موفقیت تغییر یافت.",
                    "UpdateTimeSlotStatus",
                    _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تغییر وضعیت اسلات زمانی {TimeSlotId}", timeSlotId);
                return ServiceResult.Failed(
                    $"خطا در تغییر وضعیت اسلات زمانی: {ex.Message}",
                    "UpdateTimeSlotStatus");
            }
        }

        /// <summary>
        /// آزاد کردن اسلات رزرو شده (برای لغو نوبت)
        /// </summary>
        public async Task<ServiceResult> ReleaseTimeSlotAsync(int timeSlotId)
        {
            try
            {
                _logger.Information("درخواست آزاد کردن اسلات زمانی - TimeSlotId: {TimeSlotId} توسط کاربر {UserId}",
                    timeSlotId, _currentUserService.UserId);

                if (timeSlotId <= 0)
                {
                    return ServiceResult.Failed(
                        "شناسه اسلات زمانی نامعتبر است.",
                        "ReleaseTimeSlot");
                }

                var result = await _timeSlotRepository.ReleaseTimeSlotAsync(timeSlotId, _currentUserService.UserId);

                if (!result)
                {
                    _logger.Warning("آزاد کردن اسلات زمانی {TimeSlotId} ناموفق بود", timeSlotId);
                    return ServiceResult.Failed(
                        "آزاد کردن اسلات زمانی ناموفق بود.",
                        "ReleaseTimeSlot");
                }

                _logger.Information("اسلات زمانی {TimeSlotId} با موفقیت آزاد شد", timeSlotId);

                return ServiceResult.Successful(
                    "اسلات زمانی با موفقیت آزاد شد.",
                    "ReleaseTimeSlot",
                    _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در آزاد کردن اسلات زمانی {TimeSlotId}", timeSlotId);
                return ServiceResult.Failed(
                    $"خطا در آزاد کردن اسلات زمانی: {ex.Message}",
                    "ReleaseTimeSlot");
            }
        }

        #endregion
    }
}

