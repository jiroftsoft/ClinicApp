using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت ساعات کاری کلینیک (Clinic Working Hours)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class ClinicWorkingHoursService : IClinicWorkingHoursService
    {
        private readonly IClinicWorkingHoursRepository _clinicWorkingHoursRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ClinicWorkingHoursService(
            IClinicWorkingHoursRepository clinicWorkingHoursRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _clinicWorkingHoursRepository = clinicWorkingHoursRepository ?? throw new ArgumentNullException(nameof(clinicWorkingHoursRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<ClinicWorkingHoursIndexViewModel>>> GetClinicWorkingHoursAsync(ClinicWorkingHoursSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست ساعات کاری کلینیک - Filter: {@Filter}", filter);

                var allWorkingHours = await _clinicWorkingHoursRepository.GetAllAsync(includeDeleted: false);

                // اعمال فیلترها
                var query = allWorkingHours.AsQueryable();

                if (filter.ClinicId.HasValue)
                {
                    query = query.Where(w => w.ClinicId == filter.ClinicId || w.ClinicId == null);
                }

                if (filter.DayOfWeek.HasValue)
                {
                    query = query.Where(w => w.DayOfWeek == filter.DayOfWeek.Value);
                }

                if (filter.IsOpen.HasValue)
                {
                    query = query.Where(w => w.IsOpen == filter.IsOpen.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(w => w.IsActive == filter.IsActive.Value);
                }

                var totalCount = query.Count();
                var workingHours = query
                    .OrderBy(w => w.DisplayOrder)
                    .ThenBy(w => w.DayOfWeek)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var viewModels = workingHours.Select(w => new ClinicWorkingHoursIndexViewModel
                {
                    ClinicWorkingHoursId = w.ClinicWorkingHoursId,
                    ClinicId = w.ClinicId,
                    ClinicName = w.ClinicId.HasValue ? "کلینیک " + w.ClinicId : "پیش‌فرض",
                    DayOfWeek = w.DayOfWeek,
                    DayName = w.DayName,
                    StartTime = w.StartTime,
                    EndTime = w.EndTime,
                    TimeRange = FormatTimeRange(w.StartTime, w.EndTime, w.IsOpen),
                    IsOpen = w.IsOpen,
                    IsActive = w.IsActive,
                    DisplayOrder = w.DisplayOrder,
                    Notes = w.Notes,
                    CreatedAt = w.CreatedAt
                }).ToList();

                var pagedResult = new PagedResult<ClinicWorkingHoursIndexViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ServiceResult<PagedResult<ClinicWorkingHoursIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست ساعات کاری کلینیک");
                return ServiceResult<PagedResult<ClinicWorkingHoursIndexViewModel>>.Failed("خطا در دریافت لیست ساعات کاری کلینیک");
            }
        }

        public async Task<ServiceResult<ClinicWorkingHoursDetailsViewModel>> GetClinicWorkingHoursDetailsAsync(int clinicWorkingHoursId)
        {
            try
            {
                var workingHours = await _clinicWorkingHoursRepository.GetByIdAsync(clinicWorkingHoursId);
                if (workingHours == null)
                {
                    return ServiceResult<ClinicWorkingHoursDetailsViewModel>.Failed("ساعات کاری یافت نشد");
                }

                var viewModel = new ClinicWorkingHoursDetailsViewModel
                {
                    ClinicWorkingHoursId = workingHours.ClinicWorkingHoursId,
                    ClinicId = workingHours.ClinicId,
                    ClinicName = workingHours.ClinicId.HasValue ? "کلینیک " + workingHours.ClinicId : "پیش‌فرض",
                    DayOfWeek = workingHours.DayOfWeek,
                    DayName = workingHours.DayName,
                    StartTime = workingHours.StartTime,
                    EndTime = workingHours.EndTime,
                    TimeRange = FormatTimeRange(workingHours.StartTime, workingHours.EndTime, workingHours.IsOpen),
                    IsOpen = workingHours.IsOpen,
                    IsActive = workingHours.IsActive,
                    DisplayOrder = workingHours.DisplayOrder,
                    Notes = workingHours.Notes,
                    CreatedAt = workingHours.CreatedAt,
                    CreatedByUserName = workingHours.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = workingHours.UpdatedAt,
                    UpdatedByUserName = workingHours.UpdatedByUser?.UserName
                };

                return ServiceResult<ClinicWorkingHoursDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", clinicWorkingHoursId);
                return ServiceResult<ClinicWorkingHoursDetailsViewModel>.Failed("خطا در دریافت جزئیات ساعات کاری کلینیک");
            }
        }

        public async Task<ServiceResult<ClinicWorkingHoursCreateEditViewModel>> GetClinicWorkingHoursForEditAsync(int clinicWorkingHoursId)
        {
            try
            {
                var workingHours = await _clinicWorkingHoursRepository.GetByIdAsync(clinicWorkingHoursId);
                if (workingHours == null)
                {
                    return ServiceResult<ClinicWorkingHoursCreateEditViewModel>.Failed("ساعات کاری یافت نشد");
                }

                var viewModel = new ClinicWorkingHoursCreateEditViewModel
                {
                    ClinicWorkingHoursId = workingHours.ClinicWorkingHoursId,
                    ClinicId = workingHours.ClinicId,
                    DayOfWeek = workingHours.DayOfWeek,
                    DayName = workingHours.DayName,
                    StartTime = workingHours.StartTime,
                    EndTime = workingHours.EndTime,
                    IsOpen = workingHours.IsOpen,
                    IsActive = workingHours.IsActive,
                    DisplayOrder = workingHours.DisplayOrder,
                    Notes = workingHours.Notes
                };

                return ServiceResult<ClinicWorkingHoursCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ساعات کاری برای ویرایش - ClinicWorkingHoursId: {ClinicWorkingHoursId}", clinicWorkingHoursId);
                return ServiceResult<ClinicWorkingHoursCreateEditViewModel>.Failed("خطا در دریافت ساعات کاری برای ویرایش");
            }
        }

        public async Task<ServiceResult<ClinicWorkingHours>> CreateClinicWorkingHoursAsync(ClinicWorkingHoursCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد ساعات کاری جدید - DayName: {DayName}", model.DayName);

                // بررسی تکراری بودن DayOfWeek برای همان ClinicId
                var existing = await _clinicWorkingHoursRepository.GetByDayOfWeekAsync(model.DayOfWeek, model.ClinicId);
                if (existing != null && existing.ClinicId == model.ClinicId)
                {
                    return ServiceResult<ClinicWorkingHours>.Failed($"برای این روز ({model.DayName}) قبلاً ساعات کاری ثبت شده است");
                }

                var workingHours = new ClinicWorkingHours
                {
                    ClinicId = model.ClinicId,
                    DayOfWeek = model.DayOfWeek,
                    DayName = model.DayName,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    IsOpen = model.IsOpen,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder,
                    Notes = model.Notes,
                    CreatedByUserId = _currentUserService.UserId
                };

                _clinicWorkingHoursRepository.Add(workingHours);
                await _context.SaveChangesAsync();

                _logger.Information("ساعات کاری با موفقیت ایجاد شد - ClinicWorkingHoursId: {ClinicWorkingHoursId}", workingHours.ClinicWorkingHoursId);
                return ServiceResult<ClinicWorkingHours>.Successful(workingHours, "ساعات کاری با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد ساعات کاری کلینیک");
                return ServiceResult<ClinicWorkingHours>.Failed("خطا در ایجاد ساعات کاری کلینیک");
            }
        }

        public async Task<ServiceResult<ClinicWorkingHours>> UpdateClinicWorkingHoursAsync(ClinicWorkingHoursCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی ساعات کاری - ClinicWorkingHoursId: {ClinicWorkingHoursId}", model.ClinicWorkingHoursId);

                var workingHours = await _clinicWorkingHoursRepository.GetByIdAsync(model.ClinicWorkingHoursId);
                if (workingHours == null)
                {
                    return ServiceResult<ClinicWorkingHours>.Failed("ساعات کاری یافت نشد");
                }

                // بررسی تکراری بودن DayOfWeek برای همان ClinicId (اگر تغییر کرده باشد)
                if (workingHours.DayOfWeek != model.DayOfWeek || workingHours.ClinicId != model.ClinicId)
                {
                    var existing = await _clinicWorkingHoursRepository.GetByDayOfWeekAsync(model.DayOfWeek, model.ClinicId);
                    if (existing != null && existing.ClinicWorkingHoursId != model.ClinicWorkingHoursId && existing.ClinicId == model.ClinicId)
                    {
                        return ServiceResult<ClinicWorkingHours>.Failed($"برای این روز ({model.DayName}) قبلاً ساعات کاری ثبت شده است");
                    }
                }

                workingHours.ClinicId = model.ClinicId;
                workingHours.DayOfWeek = model.DayOfWeek;
                workingHours.DayName = model.DayName;
                workingHours.StartTime = model.StartTime;
                workingHours.EndTime = model.EndTime;
                workingHours.IsOpen = model.IsOpen;
                workingHours.IsActive = model.IsActive;
                workingHours.DisplayOrder = model.DisplayOrder;
                workingHours.Notes = model.Notes;
                workingHours.UpdatedByUserId = _currentUserService.UserId;

                _clinicWorkingHoursRepository.Update(workingHours);
                await _context.SaveChangesAsync();

                _logger.Information("ساعات کاری با موفقیت به‌روزرسانی شد - ClinicWorkingHoursId: {ClinicWorkingHoursId}", workingHours.ClinicWorkingHoursId);
                return ServiceResult<ClinicWorkingHours>.Successful(workingHours, "ساعات کاری با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", model.ClinicWorkingHoursId);
                return ServiceResult<ClinicWorkingHours>.Failed("خطا در به‌روزرسانی ساعات کاری کلینیک");
            }
        }

        public async Task<ServiceResult> DeleteClinicWorkingHoursAsync(int clinicWorkingHoursId)
        {
            try
            {
                _logger.Information("حذف ساعات کاری - ClinicWorkingHoursId: {ClinicWorkingHoursId}", clinicWorkingHoursId);

                var workingHours = await _clinicWorkingHoursRepository.GetByIdAsync(clinicWorkingHoursId);
                if (workingHours == null)
                {
                    return ServiceResult.Failed("ساعات کاری یافت نشد");
                }

                _clinicWorkingHoursRepository.Delete(workingHours);
                await _context.SaveChangesAsync();

                _logger.Information("ساعات کاری با موفقیت حذف شد - ClinicWorkingHoursId: {ClinicWorkingHoursId}", clinicWorkingHoursId);
                return ServiceResult.Successful("ساعات کاری با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", clinicWorkingHoursId);
                return ServiceResult.Failed("خطا در حذف ساعات کاری کلینیک");
            }
        }

        public async Task<ServiceResult> ActivateClinicWorkingHoursAsync(int clinicWorkingHoursId)
        {
            try
            {
                var workingHours = await _clinicWorkingHoursRepository.GetByIdAsync(clinicWorkingHoursId);
                if (workingHours == null)
                {
                    return ServiceResult.Failed("ساعات کاری یافت نشد");
                }

                workingHours.IsActive = true;
                workingHours.UpdatedByUserId = _currentUserService.UserId;

                _clinicWorkingHoursRepository.Update(workingHours);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("ساعات کاری با موفقیت فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", clinicWorkingHoursId);
                return ServiceResult.Failed("خطا در فعال‌سازی ساعات کاری کلینیک");
            }
        }

        public async Task<ServiceResult> DeactivateClinicWorkingHoursAsync(int clinicWorkingHoursId)
        {
            try
            {
                var workingHours = await _clinicWorkingHoursRepository.GetByIdAsync(clinicWorkingHoursId);
                if (workingHours == null)
                {
                    return ServiceResult.Failed("ساعات کاری یافت نشد");
                }

                workingHours.IsActive = false;
                workingHours.UpdatedByUserId = _currentUserService.UserId;

                _clinicWorkingHoursRepository.Update(workingHours);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("ساعات کاری با موفقیت غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی ساعات کاری کلینیک - ClinicWorkingHoursId: {ClinicWorkingHoursId}", clinicWorkingHoursId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی ساعات کاری کلینیک");
            }
        }

        public async Task<ServiceResult<List<ClinicWorkingHoursPublicViewModel>>> GetActiveWorkingHoursAsync(int? clinicId = null)
        {
            try
            {
                var workingHours = await _clinicWorkingHoursRepository.GetActiveWorkingHoursAsync(clinicId);
                
                var viewModels = workingHours.Select(w => new ClinicWorkingHoursPublicViewModel
                {
                    ClinicWorkingHoursId = w.ClinicWorkingHoursId,
                    DayOfWeek = w.DayOfWeek,
                    DayName = w.DayName,
                    StartTime = w.StartTime,
                    EndTime = w.EndTime,
                    TimeRange = FormatTimeRange(w.StartTime, w.EndTime, w.IsOpen),
                    IsOpen = w.IsOpen,
                    Notes = w.Notes
                }).ToList();

                return ServiceResult<List<ClinicWorkingHoursPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ساعات کاری فعال کلینیک");
                return ServiceResult<List<ClinicWorkingHoursPublicViewModel>>.Failed("خطا در دریافت ساعات کاری فعال کلینیک");
            }
        }

        public async Task<ServiceResult<List<ClinicWorkingHoursPublicViewModel>>> GetByClinicIdAsync(int clinicId)
        {
            try
            {
                var workingHours = await _clinicWorkingHoursRepository.GetByClinicIdAsync(clinicId);
                
                var viewModels = workingHours.Select(w => new ClinicWorkingHoursPublicViewModel
                {
                    ClinicWorkingHoursId = w.ClinicWorkingHoursId,
                    DayOfWeek = w.DayOfWeek,
                    DayName = w.DayName,
                    StartTime = w.StartTime,
                    EndTime = w.EndTime,
                    TimeRange = FormatTimeRange(w.StartTime, w.EndTime, w.IsOpen),
                    IsOpen = w.IsOpen,
                    Notes = w.Notes
                }).ToList();

                return ServiceResult<List<ClinicWorkingHoursPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ساعات کاری بر اساس ClinicId");
                return ServiceResult<List<ClinicWorkingHoursPublicViewModel>>.Failed("خطا در دریافت ساعات کاری بر اساس ClinicId");
            }
        }

        #region Helper Methods

        private string FormatTimeRange(TimeSpan startTime, TimeSpan endTime, bool isOpen)
        {
            if (!isOpen)
            {
                return "تعطیل";
            }

            return $"{startTime:hh\\:mm} - {endTime:hh\\:mm}";
        }

        #endregion
    }
}

