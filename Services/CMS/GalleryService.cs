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
    /// سرویس مدیریت گالری تصاویر
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class GalleryService : IGalleryService
    {
        private readonly IGalleryItemRepository _galleryItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public GalleryService(
            IGalleryItemRepository galleryItemRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _galleryItemRepository = galleryItemRepository ?? throw new ArgumentNullException(nameof(galleryItemRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<List<GalleryItemIndexViewModel>>> GetGalleryItemsAsync(string category = null)
        {
            try
            {
                var galleryItems = await _galleryItemRepository.GetAllAsync(includeDeleted: false);
                
                var query = galleryItems.AsQueryable();
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(g => g.Category == category);
                }

                var viewModels = query
                    .Where(g => g.IsActive)
                    .Select(g => new GalleryItemIndexViewModel
                    {
                        GalleryItemId = g.GalleryItemId,
                        Title = g.Title,
                        Description = g.Description,
                        ImageUrl = g.ImageUrl,
                        ThumbnailUrl = g.ThumbnailUrl,
                        Category = g.Category,
                        IsActive = g.IsActive,
                        DisplayOrder = g.DisplayOrder
                    }).OrderBy(g => g.DisplayOrder)
                      .ThenBy(g => g.Title)
                      .ToList();

                return ServiceResult<List<GalleryItemIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست گالری");
                return ServiceResult<List<GalleryItemIndexViewModel>>.Failed("خطا در دریافت لیست گالری");
            }
        }

        public async Task<ServiceResult<GalleryItemDetailsViewModel>> GetGalleryItemDetailsAsync(int galleryItemId)
        {
            try
            {
                var galleryItem = await _galleryItemRepository.GetByIdAsync(galleryItemId);
                if (galleryItem == null)
                {
                    return ServiceResult<GalleryItemDetailsViewModel>.Failed("آیتم گالری یافت نشد");
                }

                var viewModel = new GalleryItemDetailsViewModel
                {
                    GalleryItemId = galleryItem.GalleryItemId,
                    Title = galleryItem.Title,
                    Description = galleryItem.Description,
                    ImageUrl = galleryItem.ImageUrl,
                    ThumbnailUrl = galleryItem.ThumbnailUrl,
                    Category = galleryItem.Category,
                    IsActive = galleryItem.IsActive,
                    DisplayOrder = galleryItem.DisplayOrder,
                    CreatedAt = galleryItem.CreatedAt,
                    CreatedByUserName = galleryItem.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = galleryItem.UpdatedAt,
                    UpdatedByUserName = galleryItem.UpdatedByUser?.UserName
                };

                return ServiceResult<GalleryItemDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات آیتم گالری - GalleryItemId: {GalleryItemId}", galleryItemId);
                return ServiceResult<GalleryItemDetailsViewModel>.Failed("خطا در دریافت جزئیات آیتم گالری");
            }
        }

        public async Task<ServiceResult<GalleryItemCreateEditViewModel>> GetGalleryItemForEditAsync(int galleryItemId)
        {
            try
            {
                var galleryItem = await _galleryItemRepository.GetByIdAsync(galleryItemId);
                if (galleryItem == null)
                {
                    return ServiceResult<GalleryItemCreateEditViewModel>.Failed("آیتم گالری یافت نشد");
                }

                var viewModel = new GalleryItemCreateEditViewModel
                {
                    GalleryItemId = galleryItem.GalleryItemId,
                    Title = galleryItem.Title,
                    Description = galleryItem.Description,
                    ImageUrl = galleryItem.ImageUrl,
                    ThumbnailUrl = galleryItem.ThumbnailUrl,
                    Category = galleryItem.Category,
                    IsActive = galleryItem.IsActive,
                    DisplayOrder = galleryItem.DisplayOrder
                };

                return ServiceResult<GalleryItemCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آیتم گالری برای ویرایش - GalleryItemId: {GalleryItemId}", galleryItemId);
                return ServiceResult<GalleryItemCreateEditViewModel>.Failed("خطا در دریافت آیتم گالری برای ویرایش");
            }
        }

        public async Task<ServiceResult<GalleryItem>> CreateGalleryItemAsync(GalleryItemCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد آیتم گالری جدید - Title: {Title}", model.Title);

                var galleryItem = new GalleryItem
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl,
                    ThumbnailUrl = model.ThumbnailUrl,
                    Category = model.Category ?? "clinic",
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder,
                    CreatedByUserId = _currentUserService.UserId
                };

                _galleryItemRepository.Add(galleryItem);
                await _context.SaveChangesAsync();

                _logger.Information("آیتم گالری با موفقیت ایجاد شد - GalleryItemId: {GalleryItemId}", galleryItem.GalleryItemId);
                return ServiceResult<GalleryItem>.Successful(galleryItem, "آیتم گالری با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد آیتم گالری");
                return ServiceResult<GalleryItem>.Failed("خطا در ایجاد آیتم گالری");
            }
        }

        public async Task<ServiceResult<GalleryItem>> UpdateGalleryItemAsync(GalleryItemCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی آیتم گالری - GalleryItemId: {GalleryItemId}", model.GalleryItemId);

                var galleryItem = await _galleryItemRepository.GetByIdAsync(model.GalleryItemId);
                if (galleryItem == null)
                {
                    return ServiceResult<GalleryItem>.Failed("آیتم گالری یافت نشد");
                }

                galleryItem.Title = model.Title;
                galleryItem.Description = model.Description;
                galleryItem.ImageUrl = model.ImageUrl;
                galleryItem.ThumbnailUrl = model.ThumbnailUrl;
                galleryItem.Category = model.Category ?? galleryItem.Category ?? "clinic";
                galleryItem.IsActive = model.IsActive;
                galleryItem.DisplayOrder = model.DisplayOrder;
                galleryItem.UpdatedByUserId = _currentUserService.UserId;

                _galleryItemRepository.Update(galleryItem);
                await _context.SaveChangesAsync();

                _logger.Information("آیتم گالری با موفقیت به‌روزرسانی شد - GalleryItemId: {GalleryItemId}", galleryItem.GalleryItemId);
                return ServiceResult<GalleryItem>.Successful(galleryItem, "آیتم گالری با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی آیتم گالری - GalleryItemId: {GalleryItemId}", model.GalleryItemId);
                return ServiceResult<GalleryItem>.Failed("خطا در به‌روزرسانی آیتم گالری");
            }
        }

        public async Task<ServiceResult> DeleteGalleryItemAsync(int galleryItemId)
        {
            try
            {
                _logger.Information("حذف آیتم گالری - GalleryItemId: {GalleryItemId}", galleryItemId);

                var galleryItem = await _galleryItemRepository.GetByIdAsync(galleryItemId);
                if (galleryItem == null)
                {
                    return ServiceResult.Failed("آیتم گالری یافت نشد");
                }

                _galleryItemRepository.Delete(galleryItem);
                await _context.SaveChangesAsync();

                _logger.Information("آیتم گالری با موفقیت حذف شد - GalleryItemId: {GalleryItemId}", galleryItemId);
                return ServiceResult.Successful("آیتم گالری با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف آیتم گالری - GalleryItemId: {GalleryItemId}", galleryItemId);
                return ServiceResult.Failed("خطا در حذف آیتم گالری");
            }
        }

        public async Task<ServiceResult> ActivateGalleryItemAsync(int galleryItemId)
        {
            try
            {
                var galleryItem = await _galleryItemRepository.GetByIdAsync(galleryItemId);
                if (galleryItem == null)
                {
                    return ServiceResult.Failed("آیتم گالری یافت نشد");
                }

                galleryItem.IsActive = true;
                galleryItem.UpdatedByUserId = _currentUserService.UserId;

                _galleryItemRepository.Update(galleryItem);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("آیتم گالری با موفقیت فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی آیتم گالری - GalleryItemId: {GalleryItemId}", galleryItemId);
                return ServiceResult.Failed("خطا در فعال‌سازی آیتم گالری");
            }
        }

        public async Task<ServiceResult> DeactivateGalleryItemAsync(int galleryItemId)
        {
            try
            {
                var galleryItem = await _galleryItemRepository.GetByIdAsync(galleryItemId);
                if (galleryItem == null)
                {
                    return ServiceResult.Failed("آیتم گالری یافت نشد");
                }

                galleryItem.IsActive = false;
                galleryItem.UpdatedByUserId = _currentUserService.UserId;

                _galleryItemRepository.Update(galleryItem);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("آیتم گالری با موفقیت غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی آیتم گالری - GalleryItemId: {GalleryItemId}", galleryItemId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی آیتم گالری");
            }
        }

        public async Task<ServiceResult> UpdateDisplayOrderAsync(int galleryItemId, int newOrder)
        {
            try
            {
                var galleryItem = await _galleryItemRepository.GetByIdAsync(galleryItemId);
                if (galleryItem == null)
                {
                    return ServiceResult.Failed("آیتم گالری یافت نشد");
                }

                galleryItem.DisplayOrder = newOrder;
                galleryItem.UpdatedByUserId = _currentUserService.UserId;

                _galleryItemRepository.Update(galleryItem);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("ترتیب نمایش با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی ترتیب نمایش - GalleryItemId: {GalleryItemId}", galleryItemId);
                return ServiceResult.Failed("خطا در به‌روزرسانی ترتیب نمایش");
            }
        }

        public async Task<ServiceResult<List<string>>> GetCategoriesAsync()
        {
            try
            {
                var galleryItems = await _galleryItemRepository.GetAllAsync(includeDeleted: false);
                var categories = galleryItems
                    .Where(g => !string.IsNullOrEmpty(g.Category))
                    .Select(g => g.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                return ServiceResult<List<string>>.Successful(categories);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی‌های گالری");
                return ServiceResult<List<string>>.Failed("خطا در دریافت دسته‌بندی‌های گالری");
            }
        }
    }
}

