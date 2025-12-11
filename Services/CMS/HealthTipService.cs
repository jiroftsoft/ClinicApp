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
    /// سرویس مدیریت نکات سلامت (Health Tips)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class HealthTipService : IHealthTipService
    {
        private readonly IHealthTipRepository _healthTipRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public HealthTipService(
            IHealthTipRepository healthTipRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _healthTipRepository = healthTipRepository ?? throw new ArgumentNullException(nameof(healthTipRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<HealthTipIndexViewModel>>> GetHealthTipsAsync(HealthTipSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست نکات سلامت - Filter: {@Filter}", filter);

                var allTips = await _healthTipRepository.GetAllAsync(includeDeleted: false);

                // اعمال فیلترها
                var query = allTips.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(h => h.Title.Contains(searchTerm) || 
                                            (h.Summary != null && h.Summary.Contains(searchTerm)) ||
                                            (h.Tags != null && h.Tags.Contains(searchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(filter.Category))
                {
                    query = query.Where(h => h.Category == filter.Category);
                }

                if (filter.IsPublished.HasValue)
                {
                    query = query.Where(h => h.IsPublished == filter.IsPublished.Value);
                }

                if (filter.IsFeatured.HasValue)
                {
                    query = query.Where(h => h.IsFeatured == filter.IsFeatured.Value);
                }

                var totalCount = query.Count();
                var tips = query
                    .OrderBy(h => h.DisplayOrder)
                    .ThenByDescending(h => h.ViewCount)
                    .ThenByDescending(h => h.PublishedAt ?? h.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var viewModels = tips.Select(h => new HealthTipIndexViewModel
                {
                    HealthTipId = h.HealthTipId,
                    Title = h.Title,
                    Summary = h.Summary,
                    Category = h.Category,
                    ImageUrl = h.ImageUrl,
                    ThumbnailUrl = h.ThumbnailUrl,
                    IsPublished = h.IsPublished,
                    IsFeatured = h.IsFeatured,
                    PublishedAt = h.PublishedAt,
                    ExpiryDate = h.ExpiryDate,
                    ViewCount = h.ViewCount,
                    ShareCount = h.ShareCount,
                    DisplayOrder = h.DisplayOrder,
                    CreatedAt = h.CreatedAt
                }).ToList();

                var pagedResult = new PagedResult<HealthTipIndexViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ServiceResult<PagedResult<HealthTipIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست نکات سلامت");
                return ServiceResult<PagedResult<HealthTipIndexViewModel>>.Failed("خطا در دریافت لیست نکات سلامت");
            }
        }

        public async Task<ServiceResult<HealthTipDetailsViewModel>> GetHealthTipDetailsAsync(int healthTipId)
        {
            try
            {
                var healthTip = await _healthTipRepository.GetByIdAsync(healthTipId);
                if (healthTip == null)
                {
                    return ServiceResult<HealthTipDetailsViewModel>.Failed("نکته سلامت یافت نشد");
                }

                var viewModel = new HealthTipDetailsViewModel
                {
                    HealthTipId = healthTip.HealthTipId,
                    Title = healthTip.Title,
                    Summary = healthTip.Summary,
                    Content = healthTip.Content,
                    ImageUrl = healthTip.ImageUrl,
                    ThumbnailUrl = healthTip.ThumbnailUrl,
                    Category = healthTip.Category,
                    Tags = healthTip.Tags,
                    PublishedAt = healthTip.PublishedAt,
                    ExpiryDate = healthTip.ExpiryDate,
                    IsPublished = healthTip.IsPublished,
                    IsFeatured = healthTip.IsFeatured,
                    ViewCount = healthTip.ViewCount,
                    ShareCount = healthTip.ShareCount,
                    DisplayOrder = healthTip.DisplayOrder,
                    MetaTitle = healthTip.MetaTitle,
                    MetaDescription = healthTip.MetaDescription,
                    Slug = healthTip.Slug,
                    RelatedLinkUrl = healthTip.RelatedLinkUrl,
                    CreatedAt = healthTip.CreatedAt,
                    CreatedByUserName = healthTip.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = healthTip.UpdatedAt,
                    UpdatedByUserName = healthTip.UpdatedByUser?.UserName
                };

                return ServiceResult<HealthTipDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات نکته سلامت - HealthTipId: {HealthTipId}", healthTipId);
                return ServiceResult<HealthTipDetailsViewModel>.Failed("خطا در دریافت جزئیات نکته سلامت");
            }
        }

        public async Task<ServiceResult<HealthTipCreateEditViewModel>> GetHealthTipForEditAsync(int healthTipId)
        {
            try
            {
                var healthTip = await _healthTipRepository.GetByIdAsync(healthTipId);
                if (healthTip == null)
                {
                    return ServiceResult<HealthTipCreateEditViewModel>.Failed("نکته سلامت یافت نشد");
                }

                var viewModel = new HealthTipCreateEditViewModel
                {
                    HealthTipId = healthTip.HealthTipId,
                    Title = healthTip.Title,
                    Summary = healthTip.Summary,
                    Content = healthTip.Content,
                    ImageUrl = healthTip.ImageUrl,
                    ThumbnailUrl = healthTip.ThumbnailUrl,
                    Category = healthTip.Category,
                    Tags = healthTip.Tags,
                    PublishedAt = healthTip.PublishedAt,
                    ExpiryDate = healthTip.ExpiryDate,
                    IsPublished = healthTip.IsPublished,
                    IsFeatured = healthTip.IsFeatured,
                    DisplayOrder = healthTip.DisplayOrder,
                    MetaTitle = healthTip.MetaTitle,
                    MetaDescription = healthTip.MetaDescription,
                    Slug = healthTip.Slug,
                    RelatedLinkUrl = healthTip.RelatedLinkUrl
                };

                return ServiceResult<HealthTipCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نکته سلامت برای ویرایش - HealthTipId: {HealthTipId}", healthTipId);
                return ServiceResult<HealthTipCreateEditViewModel>.Failed("خطا در دریافت نکته سلامت برای ویرایش");
            }
        }

        public async Task<ServiceResult<HealthTip>> CreateHealthTipAsync(HealthTipCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد نکته سلامت جدید - Title: {Title}", model.Title);

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var existing = await _healthTipRepository.GetBySlugAsync(model.Slug);
                    if (existing != null)
                    {
                        return ServiceResult<HealthTip>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                // منطق ذخیره PublishedAt:
                // 1. اگر IsPublished = true و PublishedAt مقدار دارد، از PublishedAt استفاده کن
                // 2. اگر IsPublished = true و PublishedAt null است، از DateTime.Now استفاده کن
                // 3. اگر IsPublished = false، PublishedAt = null
                DateTime? publishedAt = null;
                if (model.IsPublished)
                {
                    publishedAt = model.PublishedAt ?? DateTime.Now;
                    _logger.Debug("PublishedAt تنظیم شد - IsPublished: {IsPublished}, PublishedAt: {PublishedAt}, Model.PublishedAt: {ModelPublishedAt}",
                        model.IsPublished, publishedAt, model.PublishedAt);
                }
                else
                {
                    _logger.Debug("PublishedAt = null - IsPublished: {IsPublished}", model.IsPublished);
                }

                var healthTip = new HealthTip
                {
                    Title = model.Title,
                    Summary = model.Summary,
                    Content = model.Content,
                    ImageUrl = model.ImageUrl,
                    ThumbnailUrl = model.ThumbnailUrl,
                    Category = model.Category ?? "general",
                    Tags = model.Tags,
                    PublishedAt = publishedAt,
                    ExpiryDate = model.ExpiryDate,
                    IsPublished = model.IsPublished,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    Slug = model.Slug ?? GenerateSlug(model.Title),
                    RelatedLinkUrl = model.RelatedLinkUrl,
                    CreatedByUserId = _currentUserService.UserId
                };

                _healthTipRepository.Add(healthTip);
                await _context.SaveChangesAsync();

                _logger.Information("نکته سلامت با موفقیت ایجاد شد - HealthTipId: {HealthTipId}", healthTip.HealthTipId);
                return ServiceResult<HealthTip>.Successful(healthTip, "نکته سلامت با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد نکته سلامت");
                return ServiceResult<HealthTip>.Failed("خطا در ایجاد نکته سلامت");
            }
        }

        public async Task<ServiceResult<HealthTip>> UpdateHealthTipAsync(HealthTipCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی نکته سلامت - HealthTipId: {HealthTipId}", model.HealthTipId);

                var healthTip = await _healthTipRepository.GetByIdAsync(model.HealthTipId);
                if (healthTip == null)
                {
                    return ServiceResult<HealthTip>.Failed("نکته سلامت یافت نشد");
                }

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug) && model.Slug != healthTip.Slug)
                {
                    var existing = await _healthTipRepository.GetBySlugAsync(model.Slug);
                    if (existing != null && existing.HealthTipId != model.HealthTipId)
                    {
                        return ServiceResult<HealthTip>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                healthTip.Title = model.Title;
                healthTip.Summary = model.Summary;
                healthTip.Content = model.Content;
                healthTip.ImageUrl = model.ImageUrl;
                healthTip.ThumbnailUrl = model.ThumbnailUrl;
                healthTip.Category = model.Category ?? healthTip.Category ?? "general";
                healthTip.Tags = model.Tags;
                healthTip.IsPublished = model.IsPublished;
                healthTip.IsFeatured = model.IsFeatured;
                healthTip.DisplayOrder = model.DisplayOrder;
                healthTip.MetaTitle = model.MetaTitle;
                healthTip.MetaDescription = model.MetaDescription;
                healthTip.Slug = model.Slug ?? healthTip.Slug ?? GenerateSlug(model.Title);
                healthTip.RelatedLinkUrl = model.RelatedLinkUrl;
                healthTip.ExpiryDate = model.ExpiryDate;

                // منطق به‌روزرسانی PublishedAt:
                // 1. اگر IsPublished = true و PublishedAt مقدار دارد، از PublishedAt استفاده کن
                // 2. اگر IsPublished = true و PublishedAt null است، از DateTime.Now استفاده کن
                // 3. اگر IsPublished = false، PublishedAt = null
                if (model.IsPublished)
                {
                    healthTip.PublishedAt = model.PublishedAt ?? DateTime.Now;
                    _logger.Debug("PublishedAt تنظیم شد - IsPublished: {IsPublished}, PublishedAt: {PublishedAt}, Model.PublishedAt: {ModelPublishedAt}",
                        model.IsPublished, healthTip.PublishedAt, model.PublishedAt);
                }
                else
                {
                    healthTip.PublishedAt = null;
                    _logger.Debug("PublishedAt = null - IsPublished: {IsPublished}", model.IsPublished);
                }

                healthTip.UpdatedByUserId = _currentUserService.UserId;

                _healthTipRepository.Update(healthTip);
                await _context.SaveChangesAsync();

                _logger.Information("نکته سلامت با موفقیت به‌روزرسانی شد - HealthTipId: {HealthTipId}", healthTip.HealthTipId);
                return ServiceResult<HealthTip>.Successful(healthTip, "نکته سلامت با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی نکته سلامت - HealthTipId: {HealthTipId}", model.HealthTipId);
                return ServiceResult<HealthTip>.Failed("خطا در به‌روزرسانی نکته سلامت");
            }
        }

        public async Task<ServiceResult> DeleteHealthTipAsync(int healthTipId)
        {
            try
            {
                _logger.Information("حذف نکته سلامت - HealthTipId: {HealthTipId}", healthTipId);

                var healthTip = await _healthTipRepository.GetByIdAsync(healthTipId);
                if (healthTip == null)
                {
                    return ServiceResult.Failed("نکته سلامت یافت نشد");
                }

                _healthTipRepository.Delete(healthTip);
                await _context.SaveChangesAsync();

                _logger.Information("نکته سلامت با موفقیت حذف شد - HealthTipId: {HealthTipId}", healthTipId);
                return ServiceResult.Successful("نکته سلامت با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نکته سلامت - HealthTipId: {HealthTipId}", healthTipId);
                return ServiceResult.Failed("خطا در حذف نکته سلامت");
            }
        }

        public async Task<ServiceResult> PublishHealthTipAsync(int healthTipId)
        {
            try
            {
                var healthTip = await _healthTipRepository.GetByIdAsync(healthTipId);
                if (healthTip == null)
                {
                    return ServiceResult.Failed("نکته سلامت یافت نشد");
                }

                healthTip.IsPublished = true;
                if (!healthTip.PublishedAt.HasValue)
                {
                    healthTip.PublishedAt = DateTime.Now;
                }
                healthTip.UpdatedByUserId = _currentUserService.UserId;

                _healthTipRepository.Update(healthTip);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("نکته سلامت با موفقیت منتشر شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتشار نکته سلامت - HealthTipId: {HealthTipId}", healthTipId);
                return ServiceResult.Failed("خطا در انتشار نکته سلامت");
            }
        }

        public async Task<ServiceResult> UnpublishHealthTipAsync(int healthTipId)
        {
            try
            {
                var healthTip = await _healthTipRepository.GetByIdAsync(healthTipId);
                if (healthTip == null)
                {
                    return ServiceResult.Failed("نکته سلامت یافت نشد");
                }

                healthTip.IsPublished = false;
                healthTip.UpdatedByUserId = _currentUserService.UserId;

                _healthTipRepository.Update(healthTip);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("نکته سلامت با موفقیت از حالت انتشار خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو انتشار نکته سلامت - HealthTipId: {HealthTipId}", healthTipId);
                return ServiceResult.Failed("خطا در لغو انتشار نکته سلامت");
            }
        }

        public async Task<ServiceResult> SetFeaturedAsync(int healthTipId, bool isFeatured)
        {
            try
            {
                var healthTip = await _healthTipRepository.GetByIdAsync(healthTipId);
                if (healthTip == null)
                {
                    return ServiceResult.Failed("نکته سلامت یافت نشد");
                }

                healthTip.IsFeatured = isFeatured;
                healthTip.UpdatedByUserId = _currentUserService.UserId;

                _healthTipRepository.Update(healthTip);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful(isFeatured ? "نکته سلامت به عنوان ویژه تنظیم شد" : "نکته سلامت از حالت ویژه خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه نکته سلامت - HealthTipId: {HealthTipId}", healthTipId);
                return ServiceResult.Failed("خطا در تنظیم وضعیت ویژه نکته سلامت");
            }
        }

        public async Task<ServiceResult> IncrementViewCountAsync(int healthTipId)
        {
            try
            {
                var healthTip = await _healthTipRepository.GetByIdAsync(healthTipId);
                if (healthTip != null)
                {
                    healthTip.ViewCount++;
                    _healthTipRepository.Update(healthTip);
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد بازدید نکته سلامت - HealthTipId: {HealthTipId}", healthTipId);
                // خطا در افزایش ViewCount نباید باعث شکست شود
                return ServiceResult.Successful();
            }
        }

        public async Task<ServiceResult> IncrementShareCountAsync(int healthTipId)
        {
            try
            {
                var healthTip = await _healthTipRepository.GetByIdAsync(healthTipId);
                if (healthTip != null)
                {
                    healthTip.ShareCount++;
                    _healthTipRepository.Update(healthTip);
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد اشتراک‌گذاری نکته سلامت - HealthTipId: {HealthTipId}", healthTipId);
                // خطا در افزایش ShareCount نباید باعث شکست شود
                return ServiceResult.Successful();
            }
        }

        public async Task<ServiceResult<List<HealthTipPublicViewModel>>> GetPublicHealthTipsAsync(string category = null, int count = 10)
        {
            try
            {
                var tips = await _healthTipRepository.GetPublishedTipsAsync(category, count);
                
                var viewModels = tips.Select(h => new HealthTipPublicViewModel
                {
                    HealthTipId = h.HealthTipId,
                    Title = h.Title,
                    Summary = h.Summary,
                    Content = h.Content,
                    ImageUrl = h.ImageUrl,
                    ThumbnailUrl = h.ThumbnailUrl,
                    Category = h.Category,
                    CategoryDisplayName = GetCategoryDisplayName(h.Category),
                    Tags = !string.IsNullOrEmpty(h.Tags) 
                        ? h.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList()
                        : new List<string>(),
                    PublishedAt = h.PublishedAt,
                    ExpiryDate = h.ExpiryDate,
                    ViewCount = h.ViewCount,
                    ShareCount = h.ShareCount,
                    Slug = h.Slug,
                    RelatedLinkUrl = h.RelatedLinkUrl
                }).ToList();

                return ServiceResult<List<HealthTipPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نکات سلامت عمومی");
                return ServiceResult<List<HealthTipPublicViewModel>>.Failed("خطا در دریافت نکات سلامت عمومی");
            }
        }

        public async Task<ServiceResult<List<HealthTipPublicViewModel>>> GetFeaturedHealthTipsAsync(int count = 5)
        {
            try
            {
                var tips = await _healthTipRepository.GetFeaturedTipsAsync(count);
                
                var viewModels = tips.Select(h => new HealthTipPublicViewModel
                {
                    HealthTipId = h.HealthTipId,
                    Title = h.Title,
                    Summary = h.Summary,
                    Content = h.Content,
                    ImageUrl = h.ImageUrl,
                    ThumbnailUrl = h.ThumbnailUrl,
                    Category = h.Category,
                    CategoryDisplayName = GetCategoryDisplayName(h.Category),
                    Tags = !string.IsNullOrEmpty(h.Tags) 
                        ? h.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList()
                        : new List<string>(),
                    PublishedAt = h.PublishedAt,
                    ExpiryDate = h.ExpiryDate,
                    ViewCount = h.ViewCount,
                    ShareCount = h.ShareCount,
                    Slug = h.Slug,
                    RelatedLinkUrl = h.RelatedLinkUrl
                }).ToList();

                return ServiceResult<List<HealthTipPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نکات سلامت ویژه");
                return ServiceResult<List<HealthTipPublicViewModel>>.Failed("خطا در دریافت نکات سلامت ویژه");
            }
        }

        public async Task<ServiceResult<List<HealthTipCategoryViewModel>>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _healthTipRepository.GetCategoriesAsync();
                var tips = await _healthTipRepository.GetPublishedTipsAsync();
                
                var viewModels = categories.Select(c => new HealthTipCategoryViewModel
                {
                    Category = c,
                    DisplayName = GetCategoryDisplayName(c),
                    IconClass = GetCategoryIcon(c),
                    Count = tips.Count(h => h.Category == c)
                }).OrderBy(c => c.DisplayName).ToList();

                return ServiceResult<List<HealthTipCategoryViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی‌های نکات سلامت");
                return ServiceResult<List<HealthTipCategoryViewModel>>.Failed("خطا در دریافت دسته‌بندی‌های نکات سلامت");
            }
        }

        public async Task<ServiceResult<List<HealthTipPublicViewModel>>> SearchHealthTipsAsync(string searchTerm)
        {
            try
            {
                var tips = await _healthTipRepository.SearchTipsAsync(searchTerm);
                
                var viewModels = tips.Select(h => new HealthTipPublicViewModel
                {
                    HealthTipId = h.HealthTipId,
                    Title = h.Title,
                    Summary = h.Summary,
                    Content = h.Content,
                    ImageUrl = h.ImageUrl,
                    ThumbnailUrl = h.ThumbnailUrl,
                    Category = h.Category,
                    CategoryDisplayName = GetCategoryDisplayName(h.Category),
                    Tags = !string.IsNullOrEmpty(h.Tags) 
                        ? h.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList()
                        : new List<string>(),
                    PublishedAt = h.PublishedAt,
                    ExpiryDate = h.ExpiryDate,
                    ViewCount = h.ViewCount,
                    ShareCount = h.ShareCount,
                    Slug = h.Slug,
                    RelatedLinkUrl = h.RelatedLinkUrl
                }).ToList();

                return ServiceResult<List<HealthTipPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی نکات سلامت");
                return ServiceResult<List<HealthTipPublicViewModel>>.Failed("خطا در جستجوی نکات سلامت");
            }
        }

        public async Task<ServiceResult<HealthTip>> GetBySlugAsync(string slug)
        {
            try
            {
                var healthTip = await _healthTipRepository.GetBySlugAsync(slug);
                if (healthTip == null)
                {
                    return ServiceResult<HealthTip>.Failed("نکته سلامت یافت نشد");
                }

                return ServiceResult<HealthTip>.Successful(healthTip);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نکته سلامت بر اساس Slug - Slug: {Slug}", slug);
                return ServiceResult<HealthTip>.Failed("خطا در دریافت نکته سلامت");
            }
        }

        #region Helper Methods

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return Guid.NewGuid().ToString("N").Substring(0, 8);

            // تبدیل به حروف کوچک و حذف کاراکترهای خاص
            var slug = title.ToLower()
                .Replace(" ", "-")
                .Replace("آ", "a")
                .Replace("ا", "a")
                .Replace("ب", "b")
                .Replace("پ", "p")
                .Replace("ت", "t")
                .Replace("ث", "s")
                .Replace("ج", "j")
                .Replace("چ", "ch")
                .Replace("ح", "h")
                .Replace("خ", "kh")
                .Replace("د", "d")
                .Replace("ذ", "z")
                .Replace("ر", "r")
                .Replace("ز", "z")
                .Replace("ژ", "zh")
                .Replace("س", "s")
                .Replace("ش", "sh")
                .Replace("ص", "s")
                .Replace("ض", "z")
                .Replace("ط", "t")
                .Replace("ظ", "z")
                .Replace("ع", "a")
                .Replace("غ", "gh")
                .Replace("ف", "f")
                .Replace("ق", "gh")
                .Replace("ک", "k")
                .Replace("گ", "g")
                .Replace("ل", "l")
                .Replace("م", "m")
                .Replace("ن", "n")
                .Replace("و", "v")
                .Replace("ه", "h")
                .Replace("ی", "y");

            // حذف کاراکترهای غیرمجاز
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                slug = slug.Replace(c.ToString(), "");
            }

            return slug;
        }

        private string GetCategoryDisplayName(string category)
        {
            if (string.IsNullOrEmpty(category))
                return "عمومی";

            return category switch
            {
                "prevention" => "پیشگیری",
                "nutrition" => "تغذیه",
                "exercise" => "ورزش",
                "diseases" => "بیماری‌ها",
                "general" => "عمومی",
                _ => category
            };
        }

        private string GetCategoryIcon(string category)
        {
            return category switch
            {
                "prevention" => "fas fa-shield-alt",
                "nutrition" => "fas fa-apple-alt",
                "exercise" => "fas fa-running",
                "diseases" => "fas fa-stethoscope",
                "general" => "fas fa-heart",
                _ => "fas fa-info-circle"
            };
        }

        #endregion
    }
}

