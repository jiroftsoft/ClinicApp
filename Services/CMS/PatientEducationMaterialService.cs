using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.CMS;
using Serilog;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت مطالب آموزشی بیماران
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class PatientEducationMaterialService : IPatientEducationMaterialService
    {
        private readonly IPatientEducationMaterialRepository _materialRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public PatientEducationMaterialService(
            IPatientEducationMaterialRepository materialRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _materialRepository = materialRepository ?? throw new ArgumentNullException(nameof(materialRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<PatientEducationMaterialIndexViewModel>>> GetMaterialsAsync(PatientEducationMaterialSearchViewModel searchModel)
        {
            try
            {
                if (searchModel == null)
                {
                    searchModel = new PatientEducationMaterialSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var materials = await _materialRepository.SearchAsync(
                    searchModel.SearchTerm,
                    searchModel.Category,
                    searchModel.IsPublished,
                    searchModel.IsFeatured,
                    includeDeleted: false);

                // فیلتر بر اساس تاریخ (در صورت وجود)
                if (searchModel.FromDate.HasValue)
                {
                    materials = materials.Where(m => m.CreatedAt >= searchModel.FromDate.Value).ToList();
                }

                if (searchModel.ToDate.HasValue)
                {
                    materials = materials.Where(m => m.CreatedAt <= searchModel.ToDate.Value).ToList();
                }

                var totalCount = materials.Count;
                var pagedItems = materials
                    .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(m => new PatientEducationMaterialIndexViewModel
                    {
                        PatientEducationMaterialId = m.PatientEducationMaterialId,
                        Title = m.Title,
                        Description = m.Description,
                        Category = m.Category,
                        CategoryDisplay = GetEnumDescription(m.Category),
                        FileUrl = m.FileUrl,
                        FileName = m.FileName,
                        FileType = m.FileType,
                        FileSizeInBytes = m.FileSizeInBytes,
                        VideoUrl = m.VideoUrl,
                        ImageUrl = m.ImageUrl,
                        ThumbnailUrl = m.ThumbnailUrl,
                        IsPublished = m.IsPublished,
                        IsFeatured = m.IsFeatured,
                        DownloadCount = m.DownloadCount,
                        ViewCount = m.ViewCount,
                        PublishedAt = m.PublishedAt,
                        CreatedAt = m.CreatedAt
                    })
                    .ToList();

                var pagedResult = new PagedResult<PatientEducationMaterialIndexViewModel>(
                    pagedItems,
                    totalCount,
                    searchModel.PageNumber,
                    searchModel.PageSize);

                return ServiceResult<PagedResult<PatientEducationMaterialIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست مطالب آموزشی");
                return ServiceResult<PagedResult<PatientEducationMaterialIndexViewModel>>.Failed("خطا در دریافت لیست مطالب آموزشی");
            }
        }

        public async Task<ServiceResult<List<PatientEducationMaterialIndexViewModel>>> GetPublishedMaterialsAsync(int count = 10)
        {
            try
            {
                var materials = await _materialRepository.GetPublishedAsync(includeDeleted: false);
                var viewModels = materials
                    .Take(count)
                    .Select(m => new PatientEducationMaterialIndexViewModel
                    {
                        PatientEducationMaterialId = m.PatientEducationMaterialId,
                        Title = m.Title,
                        Description = m.Description,
                        Category = m.Category,
                        CategoryDisplay = GetEnumDescription(m.Category),
                        FileUrl = m.FileUrl,
                        FileName = m.FileName,
                        FileType = m.FileType,
                        FileSizeInBytes = m.FileSizeInBytes,
                        VideoUrl = m.VideoUrl,
                        ImageUrl = m.ImageUrl,
                        ThumbnailUrl = m.ThumbnailUrl,
                        IsPublished = m.IsPublished,
                        IsFeatured = m.IsFeatured,
                        DownloadCount = m.DownloadCount,
                        ViewCount = m.ViewCount,
                        PublishedAt = m.PublishedAt,
                        CreatedAt = m.CreatedAt
                    })
                    .ToList();

                return ServiceResult<List<PatientEducationMaterialIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت مطالب منتشر شده");
                return ServiceResult<List<PatientEducationMaterialIndexViewModel>>.Failed("خطا در دریافت مطالب منتشر شده");
            }
        }

        public async Task<ServiceResult<List<PatientEducationMaterialIndexViewModel>>> GetFeaturedMaterialsAsync(int count = 5)
        {
            try
            {
                var materials = await _materialRepository.GetFeaturedAsync(count, includeDeleted: false);
                var viewModels = materials
                    .Select(m => new PatientEducationMaterialIndexViewModel
                    {
                        PatientEducationMaterialId = m.PatientEducationMaterialId,
                        Title = m.Title,
                        Description = m.Description,
                        Category = m.Category,
                        CategoryDisplay = GetEnumDescription(m.Category),
                        FileUrl = m.FileUrl,
                        FileName = m.FileName,
                        FileType = m.FileType,
                        FileSizeInBytes = m.FileSizeInBytes,
                        VideoUrl = m.VideoUrl,
                        ImageUrl = m.ImageUrl,
                        ThumbnailUrl = m.ThumbnailUrl,
                        IsPublished = m.IsPublished,
                        IsFeatured = m.IsFeatured,
                        DownloadCount = m.DownloadCount,
                        ViewCount = m.ViewCount,
                        PublishedAt = m.PublishedAt,
                        CreatedAt = m.CreatedAt
                    })
                    .ToList();

                return ServiceResult<List<PatientEducationMaterialIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت مطالب ویژه");
                return ServiceResult<List<PatientEducationMaterialIndexViewModel>>.Failed("خطا در دریافت مطالب ویژه");
            }
        }

        public async Task<ServiceResult<PatientEducationMaterialDetailsViewModel>> GetMaterialDetailsAsync(int materialId)
        {
            try
            {
                var material = await _materialRepository.GetByIdAsync(materialId);
                if (material == null)
                {
                    return ServiceResult<PatientEducationMaterialDetailsViewModel>.Failed("مطلب آموزشی یافت نشد");
                }

                var viewModel = new PatientEducationMaterialDetailsViewModel
                {
                    PatientEducationMaterialId = material.PatientEducationMaterialId,
                    Title = material.Title,
                    Description = material.Description,
                    Content = material.Content,
                    FileUrl = material.FileUrl,
                    FileName = material.FileName,
                    FileType = material.FileType,
                    FileSizeInBytes = material.FileSizeInBytes,
                    VideoUrl = material.VideoUrl,
                    ImageUrl = material.ImageUrl,
                    ThumbnailUrl = material.ThumbnailUrl,
                    Category = material.Category,
                    CategoryDisplay = GetEnumDescription(material.Category),
                    Tags = material.Tags,
                    IsPublished = material.IsPublished,
                    IsFeatured = material.IsFeatured,
                    DownloadCount = material.DownloadCount,
                    ViewCount = material.ViewCount,
                    PublishedAt = material.PublishedAt,
                    DisplayOrder = material.DisplayOrder,
                    MetaTitle = material.MetaTitle,
                    MetaDescription = material.MetaDescription,
                    Slug = material.Slug,
                    CreatedAt = material.CreatedAt,
                    CreatedByUserName = material.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = material.UpdatedAt,
                    UpdatedByUserName = material.UpdatedByUser?.UserName
                };

                return ServiceResult<PatientEducationMaterialDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات مطلب آموزشی - MaterialId: {MaterialId}", materialId);
                return ServiceResult<PatientEducationMaterialDetailsViewModel>.Failed("خطا در دریافت جزئیات مطلب آموزشی");
            }
        }

        public async Task<ServiceResult<PatientEducationMaterialDetailsViewModel>> GetMaterialBySlugAsync(string slug)
        {
            try
            {
                var material = await _materialRepository.GetBySlugAsync(slug);
                if (material == null)
                {
                    return ServiceResult<PatientEducationMaterialDetailsViewModel>.Failed("مطلب آموزشی یافت نشد");
                }

                var viewModel = new PatientEducationMaterialDetailsViewModel
                {
                    PatientEducationMaterialId = material.PatientEducationMaterialId,
                    Title = material.Title,
                    Description = material.Description,
                    Content = material.Content,
                    FileUrl = material.FileUrl,
                    FileName = material.FileName,
                    FileType = material.FileType,
                    FileSizeInBytes = material.FileSizeInBytes,
                    VideoUrl = material.VideoUrl,
                    ImageUrl = material.ImageUrl,
                    ThumbnailUrl = material.ThumbnailUrl,
                    Category = material.Category,
                    CategoryDisplay = GetEnumDescription(material.Category),
                    Tags = material.Tags,
                    IsPublished = material.IsPublished,
                    IsFeatured = material.IsFeatured,
                    DownloadCount = material.DownloadCount,
                    ViewCount = material.ViewCount,
                    PublishedAt = material.PublishedAt,
                    DisplayOrder = material.DisplayOrder,
                    MetaTitle = material.MetaTitle,
                    MetaDescription = material.MetaDescription,
                    Slug = material.Slug,
                    CreatedAt = material.CreatedAt,
                    CreatedByUserName = material.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = material.UpdatedAt,
                    UpdatedByUserName = material.UpdatedByUser?.UserName
                };

                return ServiceResult<PatientEducationMaterialDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت مطلب آموزشی - Slug: {Slug}", slug);
                return ServiceResult<PatientEducationMaterialDetailsViewModel>.Failed("خطا در دریافت مطلب آموزشی");
            }
        }

        public async Task<ServiceResult<PatientEducationMaterialCreateEditViewModel>> GetMaterialForEditAsync(int materialId)
        {
            try
            {
                var material = await _materialRepository.GetByIdAsync(materialId);
                if (material == null)
                {
                    return ServiceResult<PatientEducationMaterialCreateEditViewModel>.Failed("مطلب آموزشی یافت نشد");
                }

                var viewModel = new PatientEducationMaterialCreateEditViewModel
                {
                    PatientEducationMaterialId = material.PatientEducationMaterialId,
                    Title = material.Title,
                    Description = material.Description,
                    Content = material.Content,
                    FileUrl = material.FileUrl,
                    FileName = material.FileName,
                    VideoUrl = material.VideoUrl,
                    ImageUrl = material.ImageUrl,
                    ThumbnailUrl = material.ThumbnailUrl,
                    Category = material.Category,
                    Tags = material.Tags,
                    PublishedAt = material.PublishedAt,
                    IsPublished = material.IsPublished,
                    IsFeatured = material.IsFeatured,
                    DisplayOrder = material.DisplayOrder,
                    MetaTitle = material.MetaTitle,
                    MetaDescription = material.MetaDescription,
                    Slug = material.Slug
                };

                return ServiceResult<PatientEducationMaterialCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت مطلب آموزشی برای ویرایش - MaterialId: {MaterialId}", materialId);
                return ServiceResult<PatientEducationMaterialCreateEditViewModel>.Failed("خطا در دریافت مطلب آموزشی برای ویرایش");
            }
        }

        public async Task<ServiceResult<PatientEducationMaterial>> CreateMaterialAsync(PatientEducationMaterialCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد مطلب آموزشی جدید - Title: {Title}", model.Title);

                var material = new PatientEducationMaterial
                {
                    Title = model.Title,
                    Description = model.Description,
                    Content = model.Content,
                    FileUrl = model.FileUrl,
                    FileName = model.FileName,
                    VideoUrl = model.VideoUrl,
                    ImageUrl = model.ImageUrl,
                    ThumbnailUrl = model.ThumbnailUrl,
                    Category = model.Category,
                    Tags = model.Tags,
                    IsPublished = model.IsPublished,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    Slug = GenerateSlug(model.Title),
                    CreatedByUserId = _currentUserService.UserId
                };

                if (model.IsPublished)
                {
                    material.PublishedAt = model.PublishedAt ?? DateTime.Now;
                }

                _materialRepository.Add(material);
                await _context.SaveChangesAsync();

                _logger.Information("مطلب آموزشی با موفقیت ایجاد شد - MaterialId: {MaterialId}", material.PatientEducationMaterialId);
                return ServiceResult<PatientEducationMaterial>.Successful(material, "مطلب آموزشی با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد مطلب آموزشی");
                return ServiceResult<PatientEducationMaterial>.Failed("خطا در ایجاد مطلب آموزشی");
            }
        }

        public async Task<ServiceResult<PatientEducationMaterial>> UpdateMaterialAsync(PatientEducationMaterialCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی مطلب آموزشی - MaterialId: {MaterialId}", model.PatientEducationMaterialId);

                var material = await _materialRepository.GetByIdAsync(model.PatientEducationMaterialId);
                if (material == null)
                {
                    return ServiceResult<PatientEducationMaterial>.Failed("مطلب آموزشی یافت نشد");
                }

                var wasPublished = material.IsPublished;
                material.Title = model.Title;
                material.Description = model.Description;
                material.Content = model.Content;
                material.FileUrl = model.FileUrl;
                material.FileName = model.FileName;
                material.VideoUrl = model.VideoUrl;
                material.ImageUrl = model.ImageUrl;
                material.ThumbnailUrl = model.ThumbnailUrl;
                material.Category = model.Category;
                material.Tags = model.Tags;
                material.IsPublished = model.IsPublished;
                material.IsFeatured = model.IsFeatured;
                material.DisplayOrder = model.DisplayOrder;
                material.MetaTitle = model.MetaTitle;
                material.MetaDescription = model.MetaDescription;
                material.UpdatedByUserId = _currentUserService.UserId;
                material.UpdatedAt = DateTime.Now;

                // به‌روزرسانی Slug در صورت تغییر عنوان
                if (material.Title != model.Title)
                {
                    material.Slug = GenerateSlug(model.Title);
                }

                if (model.IsPublished && !wasPublished)
                {
                    material.PublishedAt = model.PublishedAt ?? DateTime.Now;
                }
                else if (!model.IsPublished && wasPublished)
                {
                    material.PublishedAt = null;
                }
                else if (model.IsPublished && model.PublishedAt.HasValue)
                {
                    material.PublishedAt = model.PublishedAt.Value;
                }

                _materialRepository.Update(material);
                await _context.SaveChangesAsync();

                _logger.Information("مطلب آموزشی با موفقیت به‌روزرسانی شد - MaterialId: {MaterialId}", material.PatientEducationMaterialId);
                return ServiceResult<PatientEducationMaterial>.Successful(material, "مطلب آموزشی با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی مطلب آموزشی - MaterialId: {MaterialId}", model.PatientEducationMaterialId);
                return ServiceResult<PatientEducationMaterial>.Failed("خطا در به‌روزرسانی مطلب آموزشی");
            }
        }

        public async Task<ServiceResult> DeleteMaterialAsync(int materialId)
        {
            try
            {
                _logger.Information("حذف مطلب آموزشی - MaterialId: {MaterialId}", materialId);

                var material = await _materialRepository.GetByIdAsync(materialId);
                if (material == null)
                {
                    return ServiceResult.Failed("مطلب آموزشی یافت نشد");
                }

                _materialRepository.Delete(material);
                material.DeletedByUserId = _currentUserService.UserId;
                await _context.SaveChangesAsync();

                _logger.Information("مطلب آموزشی با موفقیت حذف شد - MaterialId: {MaterialId}", materialId);
                return ServiceResult.Successful("مطلب آموزشی با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف مطلب آموزشی - MaterialId: {MaterialId}", materialId);
                return ServiceResult.Failed("خطا در حذف مطلب آموزشی");
            }
        }

        public async Task<ServiceResult> PublishMaterialAsync(int materialId)
        {
            try
            {
                var material = await _materialRepository.GetByIdAsync(materialId);
                if (material == null)
                {
                    return ServiceResult.Failed("مطلب آموزشی یافت نشد");
                }

                material.IsPublished = true;
                if (!material.PublishedAt.HasValue)
                {
                    material.PublishedAt = DateTime.Now;
                }
                material.UpdatedByUserId = _currentUserService.UserId;
                material.UpdatedAt = DateTime.Now;

                _materialRepository.Update(material);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("مطلب آموزشی با موفقیت منتشر شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتشار مطلب آموزشی - MaterialId: {MaterialId}", materialId);
                return ServiceResult.Failed("خطا در انتشار مطلب آموزشی");
            }
        }

        public async Task<ServiceResult> UnpublishMaterialAsync(int materialId)
        {
            try
            {
                var material = await _materialRepository.GetByIdAsync(materialId);
                if (material == null)
                {
                    return ServiceResult.Failed("مطلب آموزشی یافت نشد");
                }

                material.IsPublished = false;
                material.UpdatedByUserId = _currentUserService.UserId;
                material.UpdatedAt = DateTime.Now;

                _materialRepository.Update(material);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("مطلب آموزشی با موفقیت از حالت انتشار خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو انتشار مطلب آموزشی - MaterialId: {MaterialId}", materialId);
                return ServiceResult.Failed("خطا در لغو انتشار مطلب آموزشی");
            }
        }

        public async Task<ServiceResult> SetFeaturedAsync(int materialId, bool isFeatured)
        {
            try
            {
                var material = await _materialRepository.GetByIdAsync(materialId);
                if (material == null)
                {
                    return ServiceResult.Failed("مطلب آموزشی یافت نشد");
                }

                material.IsFeatured = isFeatured;
                material.UpdatedByUserId = _currentUserService.UserId;
                material.UpdatedAt = DateTime.Now;

                _materialRepository.Update(material);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful(isFeatured ? "مطلب آموزشی به عنوان ویژه تنظیم شد" : "مطلب آموزشی از حالت ویژه خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم ویژه - MaterialId: {MaterialId}", materialId);
                return ServiceResult.Failed("خطا در تنظیم ویژه");
            }
        }

        public async Task<ServiceResult> IncrementDownloadCountAsync(int materialId)
        {
            try
            {
                await _materialRepository.IncrementDownloadCountAsync(materialId);
                await _context.SaveChangesAsync();
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد دانلود - MaterialId: {MaterialId}", materialId);
                return ServiceResult.Failed("خطا در افزایش تعداد دانلود");
            }
        }

        public async Task<ServiceResult> IncrementViewCountAsync(int materialId)
        {
            try
            {
                await _materialRepository.IncrementViewCountAsync(materialId);
                await _context.SaveChangesAsync();
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد مشاهده - MaterialId: {MaterialId}", materialId);
                return ServiceResult.Failed("خطا در افزایش تعداد مشاهده");
            }
        }

        #region Helper Methods

        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            return attribute?.Description ?? value.ToString();
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            // تبدیل به حروف کوچک
            var slug = title.ToLowerInvariant();

            // حذف کاراکترهای خاص و جایگزینی با خط تیره
            slug = Regex.Replace(slug, @"[^a-z0-9\u0600-\u06FF\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            // محدود کردن طول
            if (slug.Length > 200)
            {
                slug = slug.Substring(0, 200);
                slug = slug.TrimEnd('-');
            }

            return slug;
        }

        #endregion
    }
}

