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
    /// سرویس مدیریت اطلاعیه‌ها
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public AnnouncementService(
            IAnnouncementRepository announcementRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _announcementRepository = announcementRepository ?? throw new ArgumentNullException(nameof(announcementRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<List<AnnouncementIndexViewModel>>> GetAnnouncementsAsync(bool includeInactive = false)
        {
            try
            {
                var announcements = await _announcementRepository.GetAllAsync(includeDeleted: false);
                
                if (!includeInactive)
                {
                    announcements = announcements.Where(a => a.IsActive).ToList();
                }

                var viewModels = announcements.Select(a => new AnnouncementIndexViewModel
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    ImageUrl = a.ImageUrl,
                    LinkUrl = a.LinkUrl,
                    IsActive = a.IsActive,
                    IsImportant = a.IsImportant,
                    DisplayOrder = a.DisplayOrder,
                    Type = a.Type,
                    TargetAudience = a.TargetAudience,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate
                }).OrderByDescending(a => a.IsImportant)
                  .ThenBy(a => a.DisplayOrder)
                  .ThenByDescending(a => a.StartDate ?? DateTime.MinValue)
                  .ToList();

                return ServiceResult<List<AnnouncementIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست اطلاعیه‌ها");
                return ServiceResult<List<AnnouncementIndexViewModel>>.Failed("خطا در دریافت لیست اطلاعیه‌ها");
            }
        }

        public async Task<ServiceResult<AnnouncementDetailsViewModel>> GetAnnouncementDetailsAsync(int announcementId)
        {
            try
            {
                var announcement = await _announcementRepository.GetByIdAsync(announcementId);
                if (announcement == null)
                {
                    return ServiceResult<AnnouncementDetailsViewModel>.Failed("اطلاعیه یافت نشد");
                }

                var viewModel = new AnnouncementDetailsViewModel
                {
                    AnnouncementId = announcement.AnnouncementId,
                    Title = announcement.Title,
                    Content = announcement.Content,
                    ImageUrl = announcement.ImageUrl,
                    LinkUrl = announcement.LinkUrl,
                    IsActive = announcement.IsActive,
                    IsImportant = announcement.IsImportant,
                    DisplayOrder = announcement.DisplayOrder,
                    StartDate = announcement.StartDate,
                    EndDate = announcement.EndDate,
                    Type = announcement.Type,
                    TargetAudience = announcement.TargetAudience,
                    CreatedAt = announcement.CreatedAt,
                    CreatedByUserName = announcement.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = announcement.UpdatedAt,
                    UpdatedByUserName = announcement.UpdatedByUser?.UserName
                };

                return ServiceResult<AnnouncementDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات اطلاعیه - AnnouncementId: {AnnouncementId}", announcementId);
                return ServiceResult<AnnouncementDetailsViewModel>.Failed("خطا در دریافت جزئیات اطلاعیه");
            }
        }

        public async Task<ServiceResult<AnnouncementCreateEditViewModel>> GetAnnouncementForEditAsync(int announcementId)
        {
            try
            {
                var announcement = await _announcementRepository.GetByIdAsync(announcementId);
                if (announcement == null)
                {
                    return ServiceResult<AnnouncementCreateEditViewModel>.Failed("اطلاعیه یافت نشد");
                }

                var viewModel = new AnnouncementCreateEditViewModel
                {
                    AnnouncementId = announcement.AnnouncementId,
                    Title = announcement.Title,
                    Content = announcement.Content,
                    ImageUrl = announcement.ImageUrl,
                    LinkUrl = announcement.LinkUrl,
                    IsActive = announcement.IsActive,
                    IsImportant = announcement.IsImportant,
                    DisplayOrder = announcement.DisplayOrder,
                    StartDate = announcement.StartDate,
                    EndDate = announcement.EndDate,
                    Type = announcement.Type,
                    TargetAudience = announcement.TargetAudience
                };

                return ServiceResult<AnnouncementCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعیه برای ویرایش - AnnouncementId: {AnnouncementId}", announcementId);
                return ServiceResult<AnnouncementCreateEditViewModel>.Failed("خطا در دریافت اطلاعیه برای ویرایش");
            }
        }

        public async Task<ServiceResult<Announcement>> CreateAnnouncementAsync(AnnouncementCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد اطلاعیه جدید - Title: {Title}", model.Title);

                var announcement = new Announcement
                {
                    Title = model.Title,
                    Content = model.Content,
                    ImageUrl = model.ImageUrl,
                    LinkUrl = model.LinkUrl,
                    IsActive = model.IsActive,
                    IsImportant = model.IsImportant,
                    DisplayOrder = model.DisplayOrder,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Type = model.Type ?? "info",
                    TargetAudience = model.TargetAudience ?? "all",
                    CreatedByUserId = _currentUserService.UserId
                };

                _announcementRepository.Add(announcement);
                await _context.SaveChangesAsync();

                _logger.Information("اطلاعیه با موفقیت ایجاد شد - AnnouncementId: {AnnouncementId}", announcement.AnnouncementId);
                return ServiceResult<Announcement>.Successful(announcement, "اطلاعیه با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اطلاعیه");
                return ServiceResult<Announcement>.Failed("خطا در ایجاد اطلاعیه");
            }
        }

        public async Task<ServiceResult<Announcement>> UpdateAnnouncementAsync(AnnouncementCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی اطلاعیه - AnnouncementId: {AnnouncementId}", model.AnnouncementId);

                var announcement = await _announcementRepository.GetByIdAsync(model.AnnouncementId);
                if (announcement == null)
                {
                    return ServiceResult<Announcement>.Failed("اطلاعیه یافت نشد");
                }

                announcement.Title = model.Title;
                announcement.Content = model.Content;
                announcement.ImageUrl = model.ImageUrl;
                announcement.LinkUrl = model.LinkUrl;
                announcement.IsActive = model.IsActive;
                announcement.IsImportant = model.IsImportant;
                announcement.DisplayOrder = model.DisplayOrder;
                announcement.StartDate = model.StartDate;
                announcement.EndDate = model.EndDate;
                announcement.Type = model.Type ?? announcement.Type ?? "info";
                announcement.TargetAudience = model.TargetAudience ?? announcement.TargetAudience ?? "all";
                announcement.UpdatedByUserId = _currentUserService.UserId;

                _announcementRepository.Update(announcement);
                await _context.SaveChangesAsync();

                _logger.Information("اطلاعیه با موفقیت به‌روزرسانی شد - AnnouncementId: {AnnouncementId}", announcement.AnnouncementId);
                return ServiceResult<Announcement>.Successful(announcement, "اطلاعیه با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اطلاعیه - AnnouncementId: {AnnouncementId}", model.AnnouncementId);
                return ServiceResult<Announcement>.Failed("خطا در به‌روزرسانی اطلاعیه");
            }
        }

        public async Task<ServiceResult> DeleteAnnouncementAsync(int announcementId)
        {
            try
            {
                _logger.Information("حذف اطلاعیه - AnnouncementId: {AnnouncementId}", announcementId);

                var announcement = await _announcementRepository.GetByIdAsync(announcementId);
                if (announcement == null)
                {
                    return ServiceResult.Failed("اطلاعیه یافت نشد");
                }

                _announcementRepository.Delete(announcement);
                await _context.SaveChangesAsync();

                _logger.Information("اطلاعیه با موفقیت حذف شد - AnnouncementId: {AnnouncementId}", announcementId);
                return ServiceResult.Successful("اطلاعیه با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اطلاعیه - AnnouncementId: {AnnouncementId}", announcementId);
                return ServiceResult.Failed("خطا در حذف اطلاعیه");
            }
        }

        public async Task<ServiceResult> ActivateAnnouncementAsync(int announcementId)
        {
            try
            {
                var announcement = await _announcementRepository.GetByIdAsync(announcementId);
                if (announcement == null)
                {
                    return ServiceResult.Failed("اطلاعیه یافت نشد");
                }

                announcement.IsActive = true;
                announcement.UpdatedByUserId = _currentUserService.UserId;

                _announcementRepository.Update(announcement);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("اطلاعیه با موفقیت فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اطلاعیه - AnnouncementId: {AnnouncementId}", announcementId);
                return ServiceResult.Failed("خطا در فعال‌سازی اطلاعیه");
            }
        }

        public async Task<ServiceResult> DeactivateAnnouncementAsync(int announcementId)
        {
            try
            {
                var announcement = await _announcementRepository.GetByIdAsync(announcementId);
                if (announcement == null)
                {
                    return ServiceResult.Failed("اطلاعیه یافت نشد");
                }

                announcement.IsActive = false;
                announcement.UpdatedByUserId = _currentUserService.UserId;

                _announcementRepository.Update(announcement);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("اطلاعیه با موفقیت غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اطلاعیه - AnnouncementId: {AnnouncementId}", announcementId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی اطلاعیه");
            }
        }

        public async Task<ServiceResult> SetImportantAsync(int announcementId, bool isImportant)
        {
            try
            {
                var announcement = await _announcementRepository.GetByIdAsync(announcementId);
                if (announcement == null)
                {
                    return ServiceResult.Failed("اطلاعیه یافت نشد");
                }

                announcement.IsImportant = isImportant;
                announcement.UpdatedByUserId = _currentUserService.UserId;

                _announcementRepository.Update(announcement);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful(isImportant ? "اطلاعیه به عنوان مهم تنظیم شد" : "اطلاعیه از حالت مهم خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت مهم اطلاعیه - AnnouncementId: {AnnouncementId}", announcementId);
                return ServiceResult.Failed("خطا در تنظیم وضعیت مهم اطلاعیه");
            }
        }

        public async Task<ServiceResult<List<AnnouncementIndexViewModel>>> GetImportantAnnouncementsAsync(int count = 5)
        {
            try
            {
                var announcements = await _announcementRepository.GetAllAsync(includeDeleted: false);
                var now = DateTime.Now;

                var activeImportant = announcements
                    .Where(a => a.IsActive && a.IsImportant &&
                               (!a.StartDate.HasValue || a.StartDate.Value <= now) &&
                               (!a.EndDate.HasValue || a.EndDate.Value >= now))
                    .OrderByDescending(a => a.IsImportant)
                    .ThenBy(a => a.DisplayOrder)
                    .ThenByDescending(a => a.StartDate ?? DateTime.MinValue)
                    .Take(count)
                    .Select(a => new AnnouncementIndexViewModel
                    {
                        AnnouncementId = a.AnnouncementId,
                        Title = a.Title,
                        Content = a.Content,
                        ImageUrl = a.ImageUrl,
                        LinkUrl = a.LinkUrl,
                        IsActive = a.IsActive,
                        IsImportant = a.IsImportant,
                        DisplayOrder = a.DisplayOrder,
                        Type = a.Type,
                        TargetAudience = a.TargetAudience,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate
                    })
                    .ToList();

                return ServiceResult<List<AnnouncementIndexViewModel>>.Successful(activeImportant);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعیه‌های مهم");
                return ServiceResult<List<AnnouncementIndexViewModel>>.Failed("خطا در دریافت اطلاعیه‌های مهم");
            }
        }
    }
}

