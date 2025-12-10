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
    /// سرویس مدیریت اطلاعات بیمه (Insurance Information)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class InsuranceInfoService : IInsuranceInfoService
    {
        private readonly IInsuranceInfoRepository _insuranceInfoRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public InsuranceInfoService(
            IInsuranceInfoRepository insuranceInfoRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _insuranceInfoRepository = insuranceInfoRepository ?? throw new ArgumentNullException(nameof(insuranceInfoRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<InsuranceInfoIndexViewModel>>> GetInsuranceInfosAsync(InsuranceInfoSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست اطلاعات بیمه - Filter: {@Filter}", filter);

                var allInsurances = await _insuranceInfoRepository.GetAllAsync(includeDeleted: false);

                // اعمال فیلترها
                var query = allInsurances.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(i => i.InsuranceName.Contains(searchTerm) || 
                                            (i.Description != null && i.Description.Contains(searchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(filter.InsuranceType))
                {
                    query = query.Where(i => i.InsuranceType == filter.InsuranceType);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(i => i.IsActive == filter.IsActive.Value);
                }

                if (filter.IsFeatured.HasValue)
                {
                    query = query.Where(i => i.IsFeatured == filter.IsFeatured.Value);
                }

                var totalCount = query.Count();
                var insurances = query
                    .OrderBy(i => i.DisplayOrder)
                    .ThenBy(i => i.InsuranceName)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var viewModels = insurances.Select(i => new InsuranceInfoIndexViewModel
                {
                    InsuranceInfoId = i.InsuranceInfoId,
                    InsuranceName = i.InsuranceName,
                    InsuranceType = i.InsuranceType,
                    Description = i.Description,
                    LogoUrl = i.LogoUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    ContactPhone = i.ContactPhone,
                    WebsiteUrl = i.WebsiteUrl,
                    CoveragePercentage = i.CoveragePercentage,
                    IsActive = i.IsActive,
                    IsFeatured = i.IsFeatured,
                    DisplayOrder = i.DisplayOrder,
                    ViewCount = i.ViewCount,
                    CreatedAt = i.CreatedAt
                }).ToList();

                var pagedResult = new PagedResult<InsuranceInfoIndexViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ServiceResult<PagedResult<InsuranceInfoIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست اطلاعات بیمه");
                return ServiceResult<PagedResult<InsuranceInfoIndexViewModel>>.Failed("خطا در دریافت لیست اطلاعات بیمه");
            }
        }

        public async Task<ServiceResult<InsuranceInfoDetailsViewModel>> GetInsuranceInfoDetailsAsync(int insuranceInfoId)
        {
            try
            {
                var insuranceInfo = await _insuranceInfoRepository.GetByIdAsync(insuranceInfoId);
                if (insuranceInfo == null)
                {
                    return ServiceResult<InsuranceInfoDetailsViewModel>.Failed("اطلاعات بیمه یافت نشد");
                }

                var viewModel = new InsuranceInfoDetailsViewModel
                {
                    InsuranceInfoId = insuranceInfo.InsuranceInfoId,
                    InsuranceName = insuranceInfo.InsuranceName,
                    InsuranceType = insuranceInfo.InsuranceType,
                    Description = insuranceInfo.Description,
                    FullDescription = insuranceInfo.FullDescription,
                    LogoUrl = insuranceInfo.LogoUrl,
                    ThumbnailUrl = insuranceInfo.ThumbnailUrl,
                    ContactPhone = insuranceInfo.ContactPhone,
                    WebsiteUrl = insuranceInfo.WebsiteUrl,
                    Address = insuranceInfo.Address,
                    CoveragePercentage = insuranceInfo.CoveragePercentage,
                    IsActive = insuranceInfo.IsActive,
                    IsFeatured = insuranceInfo.IsFeatured,
                    DisplayOrder = insuranceInfo.DisplayOrder,
                    ViewCount = insuranceInfo.ViewCount,
                    MetaTitle = insuranceInfo.MetaTitle,
                    MetaDescription = insuranceInfo.MetaDescription,
                    Slug = insuranceInfo.Slug,
                    CreatedAt = insuranceInfo.CreatedAt,
                    CreatedByUserName = insuranceInfo.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = insuranceInfo.UpdatedAt,
                    UpdatedByUserName = insuranceInfo.UpdatedByUser?.UserName
                };

                return ServiceResult<InsuranceInfoDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", insuranceInfoId);
                return ServiceResult<InsuranceInfoDetailsViewModel>.Failed("خطا در دریافت جزئیات اطلاعات بیمه");
            }
        }

        public async Task<ServiceResult<InsuranceInfoCreateEditViewModel>> GetInsuranceInfoForEditAsync(int insuranceInfoId)
        {
            try
            {
                var insuranceInfo = await _insuranceInfoRepository.GetByIdAsync(insuranceInfoId);
                if (insuranceInfo == null)
                {
                    return ServiceResult<InsuranceInfoCreateEditViewModel>.Failed("اطلاعات بیمه یافت نشد");
                }

                var viewModel = new InsuranceInfoCreateEditViewModel
                {
                    InsuranceInfoId = insuranceInfo.InsuranceInfoId,
                    InsuranceName = insuranceInfo.InsuranceName,
                    InsuranceType = insuranceInfo.InsuranceType,
                    Description = insuranceInfo.Description,
                    FullDescription = insuranceInfo.FullDescription,
                    LogoUrl = insuranceInfo.LogoUrl,
                    ThumbnailUrl = insuranceInfo.ThumbnailUrl,
                    ContactPhone = insuranceInfo.ContactPhone,
                    WebsiteUrl = insuranceInfo.WebsiteUrl,
                    Address = insuranceInfo.Address,
                    CoveragePercentage = insuranceInfo.CoveragePercentage,
                    IsActive = insuranceInfo.IsActive,
                    IsFeatured = insuranceInfo.IsFeatured,
                    DisplayOrder = insuranceInfo.DisplayOrder,
                    MetaTitle = insuranceInfo.MetaTitle,
                    MetaDescription = insuranceInfo.MetaDescription,
                    Slug = insuranceInfo.Slug
                };

                return ServiceResult<InsuranceInfoCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات بیمه برای ویرایش - InsuranceInfoId: {InsuranceInfoId}", insuranceInfoId);
                return ServiceResult<InsuranceInfoCreateEditViewModel>.Failed("خطا در دریافت اطلاعات بیمه برای ویرایش");
            }
        }

        public async Task<ServiceResult<InsuranceInfo>> CreateInsuranceInfoAsync(InsuranceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد اطلاعات بیمه جدید - InsuranceName: {InsuranceName}", model.InsuranceName);

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var existing = await _insuranceInfoRepository.GetBySlugAsync(model.Slug);
                    if (existing != null)
                    {
                        return ServiceResult<InsuranceInfo>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                var insuranceInfo = new InsuranceInfo
                {
                    InsuranceName = model.InsuranceName,
                    InsuranceType = model.InsuranceType ?? "basic",
                    Description = model.Description,
                    FullDescription = model.FullDescription,
                    LogoUrl = model.LogoUrl,
                    ThumbnailUrl = model.ThumbnailUrl,
                    ContactPhone = model.ContactPhone,
                    WebsiteUrl = model.WebsiteUrl,
                    Address = model.Address,
                    CoveragePercentage = model.CoveragePercentage,
                    IsActive = model.IsActive,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    Slug = model.Slug ?? GenerateSlug(model.InsuranceName),
                    CreatedByUserId = _currentUserService.UserId
                };

                _insuranceInfoRepository.Add(insuranceInfo);
                await _context.SaveChangesAsync();

                _logger.Information("اطلاعات بیمه با موفقیت ایجاد شد - InsuranceInfoId: {InsuranceInfoId}", insuranceInfo.InsuranceInfoId);
                return ServiceResult<InsuranceInfo>.Successful(insuranceInfo, "اطلاعات بیمه با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اطلاعات بیمه");
                return ServiceResult<InsuranceInfo>.Failed("خطا در ایجاد اطلاعات بیمه");
            }
        }

        public async Task<ServiceResult<InsuranceInfo>> UpdateInsuranceInfoAsync(InsuranceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", model.InsuranceInfoId);

                var insuranceInfo = await _insuranceInfoRepository.GetByIdAsync(model.InsuranceInfoId);
                if (insuranceInfo == null)
                {
                    return ServiceResult<InsuranceInfo>.Failed("اطلاعات بیمه یافت نشد");
                }

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug) && model.Slug != insuranceInfo.Slug)
                {
                    var existing = await _insuranceInfoRepository.GetBySlugAsync(model.Slug);
                    if (existing != null && existing.InsuranceInfoId != model.InsuranceInfoId)
                    {
                        return ServiceResult<InsuranceInfo>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                insuranceInfo.InsuranceName = model.InsuranceName;
                insuranceInfo.InsuranceType = model.InsuranceType ?? insuranceInfo.InsuranceType ?? "basic";
                insuranceInfo.Description = model.Description;
                insuranceInfo.FullDescription = model.FullDescription;
                insuranceInfo.LogoUrl = model.LogoUrl;
                insuranceInfo.ThumbnailUrl = model.ThumbnailUrl;
                insuranceInfo.ContactPhone = model.ContactPhone;
                insuranceInfo.WebsiteUrl = model.WebsiteUrl;
                insuranceInfo.Address = model.Address;
                insuranceInfo.CoveragePercentage = model.CoveragePercentage;
                insuranceInfo.IsActive = model.IsActive;
                insuranceInfo.IsFeatured = model.IsFeatured;
                insuranceInfo.DisplayOrder = model.DisplayOrder;
                insuranceInfo.MetaTitle = model.MetaTitle;
                insuranceInfo.MetaDescription = model.MetaDescription;
                insuranceInfo.Slug = model.Slug ?? insuranceInfo.Slug ?? GenerateSlug(model.InsuranceName);
                insuranceInfo.UpdatedByUserId = _currentUserService.UserId;

                _insuranceInfoRepository.Update(insuranceInfo);
                await _context.SaveChangesAsync();

                _logger.Information("اطلاعات بیمه با موفقیت به‌روزرسانی شد - InsuranceInfoId: {InsuranceInfoId}", insuranceInfo.InsuranceInfoId);
                return ServiceResult<InsuranceInfo>.Successful(insuranceInfo, "اطلاعات بیمه با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", model.InsuranceInfoId);
                return ServiceResult<InsuranceInfo>.Failed("خطا در به‌روزرسانی اطلاعات بیمه");
            }
        }

        public async Task<ServiceResult> DeleteInsuranceInfoAsync(int insuranceInfoId)
        {
            try
            {
                _logger.Information("حذف اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", insuranceInfoId);

                var insuranceInfo = await _insuranceInfoRepository.GetByIdAsync(insuranceInfoId);
                if (insuranceInfo == null)
                {
                    return ServiceResult.Failed("اطلاعات بیمه یافت نشد");
                }

                _insuranceInfoRepository.Delete(insuranceInfo);
                await _context.SaveChangesAsync();

                _logger.Information("اطلاعات بیمه با موفقیت حذف شد - InsuranceInfoId: {InsuranceInfoId}", insuranceInfoId);
                return ServiceResult.Successful("اطلاعات بیمه با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", insuranceInfoId);
                return ServiceResult.Failed("خطا در حذف اطلاعات بیمه");
            }
        }

        public async Task<ServiceResult> ActivateInsuranceInfoAsync(int insuranceInfoId)
        {
            try
            {
                var insuranceInfo = await _insuranceInfoRepository.GetByIdAsync(insuranceInfoId);
                if (insuranceInfo == null)
                {
                    return ServiceResult.Failed("اطلاعات بیمه یافت نشد");
                }

                insuranceInfo.IsActive = true;
                insuranceInfo.UpdatedByUserId = _currentUserService.UserId;

                _insuranceInfoRepository.Update(insuranceInfo);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("اطلاعات بیمه با موفقیت فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", insuranceInfoId);
                return ServiceResult.Failed("خطا در فعال‌سازی اطلاعات بیمه");
            }
        }

        public async Task<ServiceResult> DeactivateInsuranceInfoAsync(int insuranceInfoId)
        {
            try
            {
                var insuranceInfo = await _insuranceInfoRepository.GetByIdAsync(insuranceInfoId);
                if (insuranceInfo == null)
                {
                    return ServiceResult.Failed("اطلاعات بیمه یافت نشد");
                }

                insuranceInfo.IsActive = false;
                insuranceInfo.UpdatedByUserId = _currentUserService.UserId;

                _insuranceInfoRepository.Update(insuranceInfo);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("اطلاعات بیمه با موفقیت غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", insuranceInfoId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی اطلاعات بیمه");
            }
        }

        public async Task<ServiceResult> SetFeaturedAsync(int insuranceInfoId, bool isFeatured)
        {
            try
            {
                var insuranceInfo = await _insuranceInfoRepository.GetByIdAsync(insuranceInfoId);
                if (insuranceInfo == null)
                {
                    return ServiceResult.Failed("اطلاعات بیمه یافت نشد");
                }

                insuranceInfo.IsFeatured = isFeatured;
                insuranceInfo.UpdatedByUserId = _currentUserService.UserId;

                _insuranceInfoRepository.Update(insuranceInfo);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful(isFeatured ? "اطلاعات بیمه به عنوان ویژه تنظیم شد" : "اطلاعات بیمه از حالت ویژه خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", insuranceInfoId);
                return ServiceResult.Failed("خطا در تنظیم وضعیت ویژه اطلاعات بیمه");
            }
        }

        public async Task<ServiceResult> IncrementViewCountAsync(int insuranceInfoId)
        {
            try
            {
                var insuranceInfo = await _insuranceInfoRepository.GetByIdAsync(insuranceInfoId);
                if (insuranceInfo != null)
                {
                    insuranceInfo.ViewCount++;
                    _insuranceInfoRepository.Update(insuranceInfo);
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد بازدید اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", insuranceInfoId);
                // خطا در افزایش ViewCount نباید باعث شکست شود
                return ServiceResult.Successful();
            }
        }

        public async Task<ServiceResult<List<InsuranceInfoPublicViewModel>>> GetPublicInsuranceInfosAsync(string insuranceType = null)
        {
            try
            {
                var insurances = await _insuranceInfoRepository.GetActiveInsurancesAsync(insuranceType);
                
                var viewModels = insurances.Select(i => new InsuranceInfoPublicViewModel
                {
                    InsuranceInfoId = i.InsuranceInfoId,
                    InsuranceName = i.InsuranceName,
                    InsuranceType = i.InsuranceType,
                    TypeDisplayName = GetTypeDisplayName(i.InsuranceType),
                    Description = i.Description,
                    FullDescription = i.FullDescription,
                    LogoUrl = i.LogoUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    ContactPhone = i.ContactPhone,
                    WebsiteUrl = i.WebsiteUrl,
                    Address = i.Address,
                    CoveragePercentage = i.CoveragePercentage,
                    ViewCount = i.ViewCount,
                    Slug = i.Slug
                }).ToList();

                return ServiceResult<List<InsuranceInfoPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات بیمه عمومی");
                return ServiceResult<List<InsuranceInfoPublicViewModel>>.Failed("خطا در دریافت اطلاعات بیمه عمومی");
            }
        }

        public async Task<ServiceResult<List<InsuranceInfoPublicViewModel>>> GetFeaturedInsuranceInfosAsync(int count = 5)
        {
            try
            {
                var insurances = await _insuranceInfoRepository.GetFeaturedInsurancesAsync(count);
                
                var viewModels = insurances.Select(i => new InsuranceInfoPublicViewModel
                {
                    InsuranceInfoId = i.InsuranceInfoId,
                    InsuranceName = i.InsuranceName,
                    InsuranceType = i.InsuranceType,
                    TypeDisplayName = GetTypeDisplayName(i.InsuranceType),
                    Description = i.Description,
                    FullDescription = i.FullDescription,
                    LogoUrl = i.LogoUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    ContactPhone = i.ContactPhone,
                    WebsiteUrl = i.WebsiteUrl,
                    Address = i.Address,
                    CoveragePercentage = i.CoveragePercentage,
                    ViewCount = i.ViewCount,
                    Slug = i.Slug
                }).ToList();

                return ServiceResult<List<InsuranceInfoPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات بیمه ویژه");
                return ServiceResult<List<InsuranceInfoPublicViewModel>>.Failed("خطا در دریافت اطلاعات بیمه ویژه");
            }
        }

        public async Task<ServiceResult<List<InsuranceInfoTypeViewModel>>> GetInsuranceTypesAsync()
        {
            try
            {
                var types = await _insuranceInfoRepository.GetInsuranceTypesAsync();
                var insurances = await _insuranceInfoRepository.GetActiveInsurancesAsync();
                
                var viewModels = types.Select(t => new InsuranceInfoTypeViewModel
                {
                    InsuranceType = t,
                    DisplayName = GetTypeDisplayName(t),
                    IconClass = GetTypeIcon(t),
                    Count = insurances.Count(i => i.InsuranceType == t)
                }).OrderBy(t => t.DisplayName).ToList();

                return ServiceResult<List<InsuranceInfoTypeViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انواع بیمه");
                return ServiceResult<List<InsuranceInfoTypeViewModel>>.Failed("خطا در دریافت انواع بیمه");
            }
        }

        public async Task<ServiceResult<List<InsuranceInfoPublicViewModel>>> SearchInsuranceInfosAsync(string searchTerm)
        {
            try
            {
                var insurances = await _insuranceInfoRepository.SearchInsurancesAsync(searchTerm);
                
                var viewModels = insurances.Select(i => new InsuranceInfoPublicViewModel
                {
                    InsuranceInfoId = i.InsuranceInfoId,
                    InsuranceName = i.InsuranceName,
                    InsuranceType = i.InsuranceType,
                    TypeDisplayName = GetTypeDisplayName(i.InsuranceType),
                    Description = i.Description,
                    FullDescription = i.FullDescription,
                    LogoUrl = i.LogoUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    ContactPhone = i.ContactPhone,
                    WebsiteUrl = i.WebsiteUrl,
                    Address = i.Address,
                    CoveragePercentage = i.CoveragePercentage,
                    ViewCount = i.ViewCount,
                    Slug = i.Slug
                }).ToList();

                return ServiceResult<List<InsuranceInfoPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی اطلاعات بیمه");
                return ServiceResult<List<InsuranceInfoPublicViewModel>>.Failed("خطا در جستجوی اطلاعات بیمه");
            }
        }

        public async Task<ServiceResult<InsuranceInfo>> GetBySlugAsync(string slug)
        {
            try
            {
                var insuranceInfo = await _insuranceInfoRepository.GetBySlugAsync(slug);
                if (insuranceInfo == null)
                {
                    return ServiceResult<InsuranceInfo>.Failed("اطلاعات بیمه یافت نشد");
                }

                return ServiceResult<InsuranceInfo>.Successful(insuranceInfo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات بیمه بر اساس Slug - Slug: {Slug}", slug);
                return ServiceResult<InsuranceInfo>.Failed("خطا در دریافت اطلاعات بیمه");
            }
        }

        #region Helper Methods

        private string GenerateSlug(string insuranceName)
        {
            if (string.IsNullOrEmpty(insuranceName))
                return Guid.NewGuid().ToString("N").Substring(0, 8);

            // تبدیل به حروف کوچک و حذف کاراکترهای خاص
            var slug = insuranceName.ToLower()
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

        private string GetTypeDisplayName(string insuranceType)
        {
            if (string.IsNullOrEmpty(insuranceType))
                return "عمومی";

            return insuranceType switch
            {
                "basic" => "بیمه پایه",
                "supplementary" => "بیمه تکمیلی",
                "private" => "بیمه خصوصی",
                "government" => "بیمه دولتی",
                _ => insuranceType
            };
        }

        private string GetTypeIcon(string insuranceType)
        {
            return insuranceType switch
            {
                "basic" => "fas fa-shield-alt",
                "supplementary" => "fas fa-plus-circle",
                "private" => "fas fa-building",
                "government" => "fas fa-landmark",
                _ => "fas fa-info-circle"
            };
        }

        #endregion
    }
}

