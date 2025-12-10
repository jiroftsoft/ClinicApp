using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// سرویس مدیریت اطلاعات خدمات پزشکی (Medical Service Information)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class MedicalServiceInfoService : IMedicalServiceInfoService
    {
        private readonly IMedicalServiceInfoRepository _medicalServiceInfoRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public MedicalServiceInfoService(
            IMedicalServiceInfoRepository medicalServiceInfoRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _medicalServiceInfoRepository = medicalServiceInfoRepository ?? throw new ArgumentNullException(nameof(medicalServiceInfoRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<MedicalServiceInfoIndexViewModel>>> GetMedicalServiceInfosAsync(MedicalServiceInfoSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست اطلاعات خدمات پزشکی - Filter: {@Filter}", filter);

                var allServiceInfos = await _medicalServiceInfoRepository.GetAllAsync(includeDeleted: false);

                // اعمال فیلترها
                var query = allServiceInfos.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(m => m.Service.Title.Contains(searchTerm) || 
                                            (m.Description != null && m.Description.Contains(searchTerm)) ||
                                            (m.Features != null && m.Features.Contains(searchTerm)));
                }

                if (filter.ServiceCategoryId.HasValue)
                {
                    query = query.Where(m => m.Service.ServiceCategoryId == filter.ServiceCategoryId.Value);
                }

                if (filter.ServiceId.HasValue)
                {
                    query = query.Where(m => m.ServiceId == filter.ServiceId.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(m => m.IsActive == filter.IsActive.Value);
                }

                if (filter.IsFeatured.HasValue)
                {
                    query = query.Where(m => m.IsFeatured == filter.IsFeatured.Value);
                }

                var totalCount = query.Count();
                var serviceInfos = query
                    .OrderBy(m => m.DisplayOrder)
                    .ThenBy(m => m.Service.Title)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var viewModels = serviceInfos.Select(m => new MedicalServiceInfoIndexViewModel
                {
                    MedicalServiceInfoId = m.MedicalServiceInfoId,
                    ServiceId = m.ServiceId,
                    ServiceTitle = m.Service.Title,
                    ServiceCode = m.Service.ServiceCode,
                    ServiceCategoryName = m.Service.ServiceCategory?.Title ?? "بدون دسته‌بندی",
                    Description = m.Description,
                    ImageUrl = m.ImageUrl,
                    ThumbnailUrl = m.ThumbnailUrl,
                    Price = m.Price ?? m.Service.Price,
                    IsActive = m.IsActive,
                    IsFeatured = m.IsFeatured,
                    DisplayOrder = m.DisplayOrder,
                    ViewCount = m.ViewCount,
                    CreatedAt = m.CreatedAt
                }).ToList();

                var pagedResult = new PagedResult<MedicalServiceInfoIndexViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ServiceResult<PagedResult<MedicalServiceInfoIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست اطلاعات خدمات پزشکی");
                return ServiceResult<PagedResult<MedicalServiceInfoIndexViewModel>>.Failed("خطا در دریافت لیست اطلاعات خدمات پزشکی");
            }
        }

        public async Task<ServiceResult<MedicalServiceInfoDetailsViewModel>> GetMedicalServiceInfoDetailsAsync(int medicalServiceInfoId)
        {
            try
            {
                var medicalServiceInfo = await _medicalServiceInfoRepository.GetByIdAsync(medicalServiceInfoId);
                if (medicalServiceInfo == null)
                {
                    return ServiceResult<MedicalServiceInfoDetailsViewModel>.Failed("اطلاعات خدمت پزشکی یافت نشد");
                }

                var viewModel = new MedicalServiceInfoDetailsViewModel
                {
                    MedicalServiceInfoId = medicalServiceInfo.MedicalServiceInfoId,
                    ServiceId = medicalServiceInfo.ServiceId,
                    ServiceTitle = medicalServiceInfo.Service.Title,
                    ServiceCode = medicalServiceInfo.Service.ServiceCode,
                    ServiceCategoryName = medicalServiceInfo.Service.ServiceCategory?.Title ?? "بدون دسته‌بندی",
                    Description = medicalServiceInfo.Description,
                    FullDescription = medicalServiceInfo.FullDescription,
                    Features = medicalServiceInfo.Features,
                    ImageUrl = medicalServiceInfo.ImageUrl,
                    ThumbnailUrl = medicalServiceInfo.ThumbnailUrl,
                    VideoUrl = medicalServiceInfo.VideoUrl,
                    Price = medicalServiceInfo.Price ?? medicalServiceInfo.Service.Price,
                    ServicePrice = medicalServiceInfo.Service.Price,
                    InsuranceCoverage = medicalServiceInfo.InsuranceCoverage,
                    EstimatedDuration = medicalServiceInfo.EstimatedDuration,
                    RequiredDocuments = medicalServiceInfo.RequiredDocuments,
                    IsActive = medicalServiceInfo.IsActive,
                    IsFeatured = medicalServiceInfo.IsFeatured,
                    DisplayOrder = medicalServiceInfo.DisplayOrder,
                    ViewCount = medicalServiceInfo.ViewCount,
                    MetaTitle = medicalServiceInfo.MetaTitle,
                    MetaDescription = medicalServiceInfo.MetaDescription,
                    Slug = medicalServiceInfo.Slug,
                    RelatedLinkUrl = medicalServiceInfo.RelatedLinkUrl,
                    CreatedAt = medicalServiceInfo.CreatedAt,
                    CreatedByUserName = medicalServiceInfo.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = medicalServiceInfo.UpdatedAt,
                    UpdatedByUserName = medicalServiceInfo.UpdatedByUser?.UserName
                };

                return ServiceResult<MedicalServiceInfoDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfoId);
                return ServiceResult<MedicalServiceInfoDetailsViewModel>.Failed("خطا در دریافت جزئیات اطلاعات خدمت پزشکی");
            }
        }

        public async Task<ServiceResult<MedicalServiceInfoCreateEditViewModel>> GetMedicalServiceInfoForEditAsync(int medicalServiceInfoId)
        {
            try
            {
                var medicalServiceInfo = await _medicalServiceInfoRepository.GetByIdAsync(medicalServiceInfoId);
                if (medicalServiceInfo == null)
                {
                    return ServiceResult<MedicalServiceInfoCreateEditViewModel>.Failed("اطلاعات خدمت پزشکی یافت نشد");
                }

                var viewModel = new MedicalServiceInfoCreateEditViewModel
                {
                    MedicalServiceInfoId = medicalServiceInfo.MedicalServiceInfoId,
                    ServiceId = medicalServiceInfo.ServiceId,
                    Description = medicalServiceInfo.Description,
                    FullDescription = medicalServiceInfo.FullDescription,
                    Features = medicalServiceInfo.Features,
                    ImageUrl = medicalServiceInfo.ImageUrl,
                    ThumbnailUrl = medicalServiceInfo.ThumbnailUrl,
                    VideoUrl = medicalServiceInfo.VideoUrl,
                    Price = medicalServiceInfo.Price,
                    InsuranceCoverage = medicalServiceInfo.InsuranceCoverage,
                    EstimatedDuration = medicalServiceInfo.EstimatedDuration,
                    RequiredDocuments = medicalServiceInfo.RequiredDocuments,
                    IsActive = medicalServiceInfo.IsActive,
                    IsFeatured = medicalServiceInfo.IsFeatured,
                    DisplayOrder = medicalServiceInfo.DisplayOrder,
                    MetaTitle = medicalServiceInfo.MetaTitle,
                    MetaDescription = medicalServiceInfo.MetaDescription,
                    Slug = medicalServiceInfo.Slug,
                    RelatedLinkUrl = medicalServiceInfo.RelatedLinkUrl
                };

                return ServiceResult<MedicalServiceInfoCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات خدمت پزشکی برای ویرایش - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfoId);
                return ServiceResult<MedicalServiceInfoCreateEditViewModel>.Failed("خطا در دریافت اطلاعات خدمت پزشکی برای ویرایش");
            }
        }

        public async Task<ServiceResult<MedicalServiceInfo>> CreateMedicalServiceInfoAsync(MedicalServiceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد اطلاعات خدمت پزشکی جدید - ServiceId: {ServiceId}", model.ServiceId);

                // بررسی وجود Service
                var service = await _context.Services
                    .Include(s => s.ServiceCategory)
                    .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && !s.IsDeleted);
                
                if (service == null)
                {
                    return ServiceResult<MedicalServiceInfo>.Failed("خدمت انتخاب شده یافت نشد");
                }

                // بررسی تکراری بودن ServiceId
                var existing = await _medicalServiceInfoRepository.GetByServiceIdAsync(model.ServiceId);
                if (existing != null && !existing.IsDeleted)
                {
                    return ServiceResult<MedicalServiceInfo>.Failed("برای این خدمت قبلاً اطلاعات CMS ایجاد شده است");
                }

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var existingSlug = await _medicalServiceInfoRepository.GetBySlugAsync(model.Slug);
                    if (existingSlug != null)
                    {
                        return ServiceResult<MedicalServiceInfo>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                var medicalServiceInfo = new MedicalServiceInfo
                {
                    ServiceId = model.ServiceId,
                    Description = model.Description,
                    FullDescription = model.FullDescription,
                    Features = model.Features,
                    ImageUrl = model.ImageUrl,
                    ThumbnailUrl = model.ThumbnailUrl,
                    VideoUrl = model.VideoUrl,
                    Price = model.Price,
                    InsuranceCoverage = model.InsuranceCoverage,
                    EstimatedDuration = model.EstimatedDuration,
                    RequiredDocuments = model.RequiredDocuments,
                    IsActive = model.IsActive,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    Slug = model.Slug ?? GenerateSlug(service.Title),
                    RelatedLinkUrl = model.RelatedLinkUrl,
                    CreatedByUserId = _currentUserService.UserId
                };

                _medicalServiceInfoRepository.Add(medicalServiceInfo);
                await _context.SaveChangesAsync();

                _logger.Information("اطلاعات خدمت پزشکی با موفقیت ایجاد شد - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfo.MedicalServiceInfoId);
                return ServiceResult<MedicalServiceInfo>.Successful(medicalServiceInfo, "اطلاعات خدمت پزشکی با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اطلاعات خدمت پزشکی");
                return ServiceResult<MedicalServiceInfo>.Failed("خطا در ایجاد اطلاعات خدمت پزشکی");
            }
        }

        public async Task<ServiceResult<MedicalServiceInfo>> UpdateMedicalServiceInfoAsync(MedicalServiceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", model.MedicalServiceInfoId);

                var medicalServiceInfo = await _medicalServiceInfoRepository.GetByIdAsync(model.MedicalServiceInfoId);
                if (medicalServiceInfo == null)
                {
                    return ServiceResult<MedicalServiceInfo>.Failed("اطلاعات خدمت پزشکی یافت نشد");
                }

                // بررسی تغییر ServiceId
                if (model.ServiceId != medicalServiceInfo.ServiceId)
                {
                    var service = await _context.Services
                        .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && !s.IsDeleted);
                    
                    if (service == null)
                    {
                        return ServiceResult<MedicalServiceInfo>.Failed("خدمت انتخاب شده یافت نشد");
                    }

                    // بررسی تکراری بودن ServiceId جدید
                    var existing = await _medicalServiceInfoRepository.GetByServiceIdAsync(model.ServiceId);
                    if (existing != null && existing.MedicalServiceInfoId != model.MedicalServiceInfoId && !existing.IsDeleted)
                    {
                        return ServiceResult<MedicalServiceInfo>.Failed("برای این خدمت قبلاً اطلاعات CMS ایجاد شده است");
                    }

                    medicalServiceInfo.ServiceId = model.ServiceId;
                }

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug) && model.Slug != medicalServiceInfo.Slug)
                {
                    var existingSlug = await _medicalServiceInfoRepository.GetBySlugAsync(model.Slug);
                    if (existingSlug != null && existingSlug.MedicalServiceInfoId != model.MedicalServiceInfoId)
                    {
                        return ServiceResult<MedicalServiceInfo>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                medicalServiceInfo.Description = model.Description;
                medicalServiceInfo.FullDescription = model.FullDescription;
                medicalServiceInfo.Features = model.Features;
                medicalServiceInfo.ImageUrl = model.ImageUrl;
                medicalServiceInfo.ThumbnailUrl = model.ThumbnailUrl;
                medicalServiceInfo.VideoUrl = model.VideoUrl;
                medicalServiceInfo.Price = model.Price;
                medicalServiceInfo.InsuranceCoverage = model.InsuranceCoverage;
                medicalServiceInfo.EstimatedDuration = model.EstimatedDuration;
                medicalServiceInfo.RequiredDocuments = model.RequiredDocuments;
                medicalServiceInfo.IsActive = model.IsActive;
                medicalServiceInfo.IsFeatured = model.IsFeatured;
                medicalServiceInfo.DisplayOrder = model.DisplayOrder;
                medicalServiceInfo.MetaTitle = model.MetaTitle;
                medicalServiceInfo.MetaDescription = model.MetaDescription;
                medicalServiceInfo.Slug = model.Slug ?? medicalServiceInfo.Slug ?? GenerateSlug(medicalServiceInfo.Service?.Title ?? "");
                medicalServiceInfo.RelatedLinkUrl = model.RelatedLinkUrl;
                medicalServiceInfo.UpdatedByUserId = _currentUserService.UserId;

                _medicalServiceInfoRepository.Update(medicalServiceInfo);
                await _context.SaveChangesAsync();

                _logger.Information("اطلاعات خدمت پزشکی با موفقیت به‌روزرسانی شد - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfo.MedicalServiceInfoId);
                return ServiceResult<MedicalServiceInfo>.Successful(medicalServiceInfo, "اطلاعات خدمت پزشکی با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", model.MedicalServiceInfoId);
                return ServiceResult<MedicalServiceInfo>.Failed("خطا در به‌روزرسانی اطلاعات خدمت پزشکی");
            }
        }

        public async Task<ServiceResult> DeleteMedicalServiceInfoAsync(int medicalServiceInfoId)
        {
            try
            {
                _logger.Information("حذف اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfoId);

                var medicalServiceInfo = await _medicalServiceInfoRepository.GetByIdAsync(medicalServiceInfoId);
                if (medicalServiceInfo == null)
                {
                    return ServiceResult.Failed("اطلاعات خدمت پزشکی یافت نشد");
                }

                _medicalServiceInfoRepository.Delete(medicalServiceInfo);
                await _context.SaveChangesAsync();

                _logger.Information("اطلاعات خدمت پزشکی با موفقیت حذف شد - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfoId);
                return ServiceResult.Successful("اطلاعات خدمت پزشکی با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfoId);
                return ServiceResult.Failed("خطا در حذف اطلاعات خدمت پزشکی");
            }
        }

        public async Task<ServiceResult> ActivateMedicalServiceInfoAsync(int medicalServiceInfoId)
        {
            try
            {
                var medicalServiceInfo = await _medicalServiceInfoRepository.GetByIdAsync(medicalServiceInfoId);
                if (medicalServiceInfo == null)
                {
                    return ServiceResult.Failed("اطلاعات خدمت پزشکی یافت نشد");
                }

                medicalServiceInfo.IsActive = true;
                medicalServiceInfo.UpdatedByUserId = _currentUserService.UserId;

                _medicalServiceInfoRepository.Update(medicalServiceInfo);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("اطلاعات خدمت پزشکی با موفقیت فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfoId);
                return ServiceResult.Failed("خطا در فعال‌سازی اطلاعات خدمت پزشکی");
            }
        }

        public async Task<ServiceResult> DeactivateMedicalServiceInfoAsync(int medicalServiceInfoId)
        {
            try
            {
                var medicalServiceInfo = await _medicalServiceInfoRepository.GetByIdAsync(medicalServiceInfoId);
                if (medicalServiceInfo == null)
                {
                    return ServiceResult.Failed("اطلاعات خدمت پزشکی یافت نشد");
                }

                medicalServiceInfo.IsActive = false;
                medicalServiceInfo.UpdatedByUserId = _currentUserService.UserId;

                _medicalServiceInfoRepository.Update(medicalServiceInfo);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("اطلاعات خدمت پزشکی با موفقیت غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfoId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی اطلاعات خدمت پزشکی");
            }
        }

        public async Task<ServiceResult> SetFeaturedAsync(int medicalServiceInfoId, bool isFeatured)
        {
            try
            {
                var medicalServiceInfo = await _medicalServiceInfoRepository.GetByIdAsync(medicalServiceInfoId);
                if (medicalServiceInfo == null)
                {
                    return ServiceResult.Failed("اطلاعات خدمت پزشکی یافت نشد");
                }

                medicalServiceInfo.IsFeatured = isFeatured;
                medicalServiceInfo.UpdatedByUserId = _currentUserService.UserId;

                _medicalServiceInfoRepository.Update(medicalServiceInfo);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful(isFeatured ? "اطلاعات خدمت پزشکی به عنوان ویژه تنظیم شد" : "اطلاعات خدمت پزشکی از حالت ویژه خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfoId);
                return ServiceResult.Failed("خطا در تنظیم وضعیت ویژه اطلاعات خدمت پزشکی");
            }
        }

        public async Task<ServiceResult> IncrementViewCountAsync(int medicalServiceInfoId)
        {
            try
            {
                var medicalServiceInfo = await _medicalServiceInfoRepository.GetByIdAsync(medicalServiceInfoId);
                if (medicalServiceInfo != null)
                {
                    medicalServiceInfo.ViewCount++;
                    _medicalServiceInfoRepository.Update(medicalServiceInfo);
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد بازدید اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", medicalServiceInfoId);
                // خطا در افزایش ViewCount نباید باعث شکست شود
                return ServiceResult.Successful();
            }
        }

        public async Task<ServiceResult<List<MedicalServiceInfoPublicViewModel>>> GetPublicServiceInfosAsync(int? serviceCategoryId = null)
        {
            try
            {
                var serviceInfos = await _medicalServiceInfoRepository.GetActiveServiceInfosAsync(serviceCategoryId);
                
                var viewModels = serviceInfos.Select(m => new MedicalServiceInfoPublicViewModel
                {
                    MedicalServiceInfoId = m.MedicalServiceInfoId,
                    ServiceId = m.ServiceId,
                    ServiceTitle = m.Service.Title,
                    ServiceCode = m.Service.ServiceCode,
                    ServiceCategoryName = m.Service.ServiceCategory?.Title ?? "بدون دسته‌بندی",
                    Description = m.Description,
                    FullDescription = m.FullDescription,
                    Features = !string.IsNullOrEmpty(m.Features) 
                        ? m.Features.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(f => f.Trim())
                            .ToList()
                        : new List<string>(),
                    ImageUrl = m.ImageUrl,
                    ThumbnailUrl = m.ThumbnailUrl,
                    VideoUrl = m.VideoUrl,
                    Price = m.Price ?? m.Service.Price,
                    ServicePrice = m.Service.Price,
                    InsuranceCoverage = !string.IsNullOrEmpty(m.InsuranceCoverage) 
                        ? m.InsuranceCoverage.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .ToList()
                        : new List<string>(),
                    EstimatedDuration = m.EstimatedDuration,
                    RequiredDocuments = !string.IsNullOrEmpty(m.RequiredDocuments) 
                        ? m.RequiredDocuments.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(d => d.Trim())
                            .ToList()
                        : new List<string>(),
                    ViewCount = m.ViewCount,
                    Slug = m.Slug,
                    RelatedLinkUrl = m.RelatedLinkUrl
                }).ToList();

                return ServiceResult<List<MedicalServiceInfoPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات خدمات پزشکی عمومی");
                return ServiceResult<List<MedicalServiceInfoPublicViewModel>>.Failed("خطا در دریافت اطلاعات خدمات پزشکی عمومی");
            }
        }

        public async Task<ServiceResult<List<MedicalServiceInfoPublicViewModel>>> GetFeaturedServiceInfosAsync(int count = 6)
        {
            try
            {
                var serviceInfos = await _medicalServiceInfoRepository.GetFeaturedServiceInfosAsync(count);
                
                var viewModels = serviceInfos.Select(m => new MedicalServiceInfoPublicViewModel
                {
                    MedicalServiceInfoId = m.MedicalServiceInfoId,
                    ServiceId = m.ServiceId,
                    ServiceTitle = m.Service.Title,
                    ServiceCode = m.Service.ServiceCode,
                    ServiceCategoryName = m.Service.ServiceCategory?.Title ?? "بدون دسته‌بندی",
                    Description = m.Description,
                    FullDescription = m.FullDescription,
                    Features = !string.IsNullOrEmpty(m.Features) 
                        ? m.Features.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(f => f.Trim())
                            .ToList()
                        : new List<string>(),
                    ImageUrl = m.ImageUrl,
                    ThumbnailUrl = m.ThumbnailUrl,
                    VideoUrl = m.VideoUrl,
                    Price = m.Price ?? m.Service.Price,
                    ServicePrice = m.Service.Price,
                    InsuranceCoverage = !string.IsNullOrEmpty(m.InsuranceCoverage) 
                        ? m.InsuranceCoverage.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .ToList()
                        : new List<string>(),
                    EstimatedDuration = m.EstimatedDuration,
                    RequiredDocuments = !string.IsNullOrEmpty(m.RequiredDocuments) 
                        ? m.RequiredDocuments.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(d => d.Trim())
                            .ToList()
                        : new List<string>(),
                    ViewCount = m.ViewCount,
                    Slug = m.Slug,
                    RelatedLinkUrl = m.RelatedLinkUrl
                }).ToList();

                return ServiceResult<List<MedicalServiceInfoPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات خدمات پزشکی ویژه");
                return ServiceResult<List<MedicalServiceInfoPublicViewModel>>.Failed("خطا در دریافت اطلاعات خدمات پزشکی ویژه");
            }
        }

        public async Task<ServiceResult<List<MedicalServiceInfoPublicViewModel>>> GetByServiceCategoryAsync(int serviceCategoryId, int count = 10)
        {
            try
            {
                var serviceInfos = await _medicalServiceInfoRepository.GetByServiceCategoryAsync(serviceCategoryId, count);
                
                var viewModels = serviceInfos.Select(m => new MedicalServiceInfoPublicViewModel
                {
                    MedicalServiceInfoId = m.MedicalServiceInfoId,
                    ServiceId = m.ServiceId,
                    ServiceTitle = m.Service.Title,
                    ServiceCode = m.Service.ServiceCode,
                    ServiceCategoryName = m.Service.ServiceCategory?.Title ?? "بدون دسته‌بندی",
                    Description = m.Description,
                    FullDescription = m.FullDescription,
                    Features = !string.IsNullOrEmpty(m.Features) 
                        ? m.Features.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(f => f.Trim())
                            .ToList()
                        : new List<string>(),
                    ImageUrl = m.ImageUrl,
                    ThumbnailUrl = m.ThumbnailUrl,
                    VideoUrl = m.VideoUrl,
                    Price = m.Price ?? m.Service.Price,
                    ServicePrice = m.Service.Price,
                    InsuranceCoverage = !string.IsNullOrEmpty(m.InsuranceCoverage) 
                        ? m.InsuranceCoverage.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .ToList()
                        : new List<string>(),
                    EstimatedDuration = m.EstimatedDuration,
                    RequiredDocuments = !string.IsNullOrEmpty(m.RequiredDocuments) 
                        ? m.RequiredDocuments.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(d => d.Trim())
                            .ToList()
                    : new List<string>(),
                    ViewCount = m.ViewCount,
                    Slug = m.Slug,
                    RelatedLinkUrl = m.RelatedLinkUrl
                }).ToList();

                return ServiceResult<List<MedicalServiceInfoPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات خدمات پزشکی بر اساس دسته‌بندی");
                return ServiceResult<List<MedicalServiceInfoPublicViewModel>>.Failed("خطا در دریافت اطلاعات خدمات پزشکی بر اساس دسته‌بندی");
            }
        }

        public async Task<ServiceResult<List<MedicalServiceInfoPublicViewModel>>> SearchServiceInfosAsync(string searchTerm)
        {
            try
            {
                var serviceInfos = await _medicalServiceInfoRepository.SearchServiceInfosAsync(searchTerm);
                
                var viewModels = serviceInfos.Select(m => new MedicalServiceInfoPublicViewModel
                {
                    MedicalServiceInfoId = m.MedicalServiceInfoId,
                    ServiceId = m.ServiceId,
                    ServiceTitle = m.Service.Title,
                    ServiceCode = m.Service.ServiceCode,
                    ServiceCategoryName = m.Service.ServiceCategory?.Title ?? "بدون دسته‌بندی",
                    Description = m.Description,
                    FullDescription = m.FullDescription,
                    Features = !string.IsNullOrEmpty(m.Features) 
                        ? m.Features.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(f => f.Trim())
                            .ToList()
                        : new List<string>(),
                    ImageUrl = m.ImageUrl,
                    ThumbnailUrl = m.ThumbnailUrl,
                    VideoUrl = m.VideoUrl,
                    Price = m.Price ?? m.Service.Price,
                    ServicePrice = m.Service.Price,
                    InsuranceCoverage = !string.IsNullOrEmpty(m.InsuranceCoverage) 
                        ? m.InsuranceCoverage.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .ToList()
                        : new List<string>(),
                    EstimatedDuration = m.EstimatedDuration,
                    RequiredDocuments = !string.IsNullOrEmpty(m.RequiredDocuments) 
                        ? m.RequiredDocuments.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(d => d.Trim())
                            .ToList()
                        : new List<string>(),
                    ViewCount = m.ViewCount,
                    Slug = m.Slug,
                    RelatedLinkUrl = m.RelatedLinkUrl
                }).ToList();

                return ServiceResult<List<MedicalServiceInfoPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی اطلاعات خدمات پزشکی");
                return ServiceResult<List<MedicalServiceInfoPublicViewModel>>.Failed("خطا در جستجوی اطلاعات خدمات پزشکی");
            }
        }

        public async Task<ServiceResult<MedicalServiceInfo>> GetBySlugAsync(string slug)
        {
            try
            {
                var medicalServiceInfo = await _medicalServiceInfoRepository.GetBySlugAsync(slug);
                if (medicalServiceInfo == null)
                {
                    return ServiceResult<MedicalServiceInfo>.Failed("اطلاعات خدمت پزشکی یافت نشد");
                }

                return ServiceResult<MedicalServiceInfo>.Successful(medicalServiceInfo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات خدمت پزشکی بر اساس Slug - Slug: {Slug}", slug);
                return ServiceResult<MedicalServiceInfo>.Failed("خطا در دریافت اطلاعات خدمت پزشکی");
            }
        }

        public async Task<ServiceResult<MedicalServiceInfo>> GetByServiceIdAsync(int serviceId)
        {
            try
            {
                var medicalServiceInfo = await _medicalServiceInfoRepository.GetByServiceIdAsync(serviceId);
                if (medicalServiceInfo == null)
                {
                    return ServiceResult<MedicalServiceInfo>.Failed("اطلاعات خدمت پزشکی یافت نشد");
                }

                return ServiceResult<MedicalServiceInfo>.Successful(medicalServiceInfo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات خدمت پزشکی بر اساس ServiceId - ServiceId: {ServiceId}", serviceId);
                return ServiceResult<MedicalServiceInfo>.Failed("خطا در دریافت اطلاعات خدمت پزشکی");
            }
        }

        #region Helper Methods

        private string GenerateSlug(string serviceTitle)
        {
            if (string.IsNullOrEmpty(serviceTitle))
                return Guid.NewGuid().ToString("N").Substring(0, 8);

            // تبدیل به حروف کوچک و حذف کاراکترهای خاص
            var slug = serviceTitle.ToLower()
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

        #endregion
    }
}

