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
using Newtonsoft.Json;
using Serilog;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت تجهیزات پزشکی (Medical Equipment)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class MedicalEquipmentService : IMedicalEquipmentService
    {
        private readonly IMedicalEquipmentRepository _medicalEquipmentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public MedicalEquipmentService(
            IMedicalEquipmentRepository medicalEquipmentRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _medicalEquipmentRepository = medicalEquipmentRepository ?? throw new ArgumentNullException(nameof(medicalEquipmentRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<MedicalEquipmentIndexViewModel>>> GetMedicalEquipmentsAsync(MedicalEquipmentSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست تجهیزات پزشکی - Filter: {@Filter}", filter);

                var allEquipments = await _medicalEquipmentRepository.GetAllAsync(includeDeleted: false);

                // اعمال فیلترها
                var query = allEquipments.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(e => e.EquipmentName.Contains(searchTerm) || 
                                            (e.Model != null && e.Model.Contains(searchTerm)) ||
                                            (e.Manufacturer != null && e.Manufacturer.Contains(searchTerm)) ||
                                            (e.Category != null && e.Category.Contains(searchTerm)) ||
                                            (e.Description != null && e.Description.Contains(searchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(filter.Category))
                {
                    query = query.Where(e => e.Category == filter.Category);
                }

                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    query = query.Where(e => e.Status == filter.Status);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(e => e.IsActive == filter.IsActive.Value);
                }

                if (filter.IsFeatured.HasValue)
                {
                    query = query.Where(e => e.IsFeatured == filter.IsFeatured.Value);
                }

                var totalCount = query.Count();
                var equipments = query
                    .OrderBy(e => e.DisplayOrder)
                    .ThenBy(e => e.EquipmentName)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var viewModels = equipments.Select(e => new MedicalEquipmentIndexViewModel
                {
                    MedicalEquipmentId = e.MedicalEquipmentId,
                    EquipmentName = e.EquipmentName,
                    Model = e.Model,
                    Manufacturer = e.Manufacturer,
                    Category = e.Category,
                    CategoryDisplayName = GetCategoryDisplayName(e.Category),
                    ImageUrl = e.ImageUrl,
                    Status = e.Status,
                    IsActive = e.IsActive,
                    IsFeatured = e.IsFeatured,
                    DisplayOrder = e.DisplayOrder,
                    PurchaseDate = e.PurchaseDate,
                    CreatedAt = e.CreatedAt
                }).ToList();

                var pagedResult = new PagedResult<MedicalEquipmentIndexViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ServiceResult<PagedResult<MedicalEquipmentIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست تجهیزات پزشکی");
                return ServiceResult<PagedResult<MedicalEquipmentIndexViewModel>>.Failed("خطا در دریافت لیست تجهیزات پزشکی");
            }
        }

        public async Task<ServiceResult<MedicalEquipmentDetailsViewModel>> GetMedicalEquipmentDetailsAsync(int medicalEquipmentId)
        {
            try
            {
                var equipment = await _medicalEquipmentRepository.GetByIdAsync(medicalEquipmentId);
                if (equipment == null)
                {
                    return ServiceResult<MedicalEquipmentDetailsViewModel>.Failed("تجهیز پزشکی یافت نشد");
                }

                var viewModel = new MedicalEquipmentDetailsViewModel
                {
                    MedicalEquipmentId = equipment.MedicalEquipmentId,
                    EquipmentName = equipment.EquipmentName,
                    Model = equipment.Model,
                    Manufacturer = equipment.Manufacturer,
                    Category = equipment.Category,
                    CategoryDisplayName = GetCategoryDisplayName(equipment.Category),
                    Description = equipment.Description,
                    TechnicalSpecifications = equipment.TechnicalSpecifications,
                    ImageUrl = equipment.ImageUrl,
                    ImageUrls = ParseJsonArray(equipment.ImageUrls),
                    VideoUrl = equipment.VideoUrl,
                    PurchaseDate = equipment.PurchaseDate,
                    InstallationDate = equipment.InstallationDate,
                    WarrantyExpiryDate = equipment.WarrantyExpiryDate,
                    Status = equipment.Status,
                    IsActive = equipment.IsActive,
                    IsFeatured = equipment.IsFeatured,
                    DisplayOrder = equipment.DisplayOrder,
                    Features = ParseJsonArray(equipment.Features),
                    ShortDescription = equipment.ShortDescription,
                    Slug = equipment.Slug,
                    MetaTitle = equipment.MetaTitle,
                    MetaDescription = equipment.MetaDescription,
                    ViewCount = equipment.ViewCount,
                    CreatedAt = equipment.CreatedAt,
                    CreatedByUserName = equipment.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = equipment.UpdatedAt,
                    UpdatedByUserName = equipment.UpdatedByUser?.UserName
                };

                return ServiceResult<MedicalEquipmentDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", medicalEquipmentId);
                return ServiceResult<MedicalEquipmentDetailsViewModel>.Failed("خطا در دریافت جزئیات تجهیز پزشکی");
            }
        }

        public async Task<ServiceResult<MedicalEquipmentCreateEditViewModel>> GetMedicalEquipmentForEditAsync(int medicalEquipmentId)
        {
            try
            {
                var equipment = await _medicalEquipmentRepository.GetByIdAsync(medicalEquipmentId);
                if (equipment == null)
                {
                    return ServiceResult<MedicalEquipmentCreateEditViewModel>.Failed("تجهیز پزشکی یافت نشد");
                }

                var viewModel = new MedicalEquipmentCreateEditViewModel
                {
                    MedicalEquipmentId = equipment.MedicalEquipmentId,
                    EquipmentName = equipment.EquipmentName,
                    Model = equipment.Model,
                    Manufacturer = equipment.Manufacturer,
                    Category = equipment.Category,
                    Description = equipment.Description,
                    TechnicalSpecifications = equipment.TechnicalSpecifications,
                    ImageUrl = equipment.ImageUrl,
                    ImageUrls = equipment.ImageUrls,
                    VideoUrl = equipment.VideoUrl,
                    PurchaseDate = equipment.PurchaseDate,
                    InstallationDate = equipment.InstallationDate,
                    WarrantyExpiryDate = equipment.WarrantyExpiryDate,
                    Status = equipment.Status,
                    IsActive = equipment.IsActive,
                    IsFeatured = equipment.IsFeatured,
                    DisplayOrder = equipment.DisplayOrder,
                    Features = equipment.Features,
                    ShortDescription = equipment.ShortDescription,
                    Slug = equipment.Slug,
                    MetaTitle = equipment.MetaTitle,
                    MetaDescription = equipment.MetaDescription
                };

                return ServiceResult<MedicalEquipmentCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تجهیز پزشکی برای ویرایش - MedicalEquipmentId: {MedicalEquipmentId}", medicalEquipmentId);
                return ServiceResult<MedicalEquipmentCreateEditViewModel>.Failed("خطا در دریافت تجهیز پزشکی برای ویرایش");
            }
        }

        public async Task<ServiceResult<MedicalEquipment>> CreateMedicalEquipmentAsync(MedicalEquipmentCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد تجهیز پزشکی جدید - EquipmentName: {EquipmentName}", model.EquipmentName);

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var existingSlug = await _medicalEquipmentRepository.GetBySlugAsync(model.Slug);
                    if (existingSlug != null)
                    {
                        return ServiceResult<MedicalEquipment>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                var equipment = new MedicalEquipment
                {
                    EquipmentName = model.EquipmentName,
                    Model = model.Model,
                    Manufacturer = model.Manufacturer,
                    Category = model.Category,
                    Description = model.Description,
                    TechnicalSpecifications = model.TechnicalSpecifications,
                    ImageUrl = model.ImageUrl,
                    ImageUrls = model.ImageUrls,
                    VideoUrl = model.VideoUrl,
                    PurchaseDate = model.PurchaseDate,
                    InstallationDate = model.InstallationDate,
                    WarrantyExpiryDate = model.WarrantyExpiryDate,
                    Status = model.Status ?? "Active",
                    IsActive = model.IsActive,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    Features = model.Features,
                    ShortDescription = model.ShortDescription,
                    Slug = model.Slug ?? GenerateSlug(model.EquipmentName),
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    CreatedByUserId = _currentUserService.UserId
                };

                _medicalEquipmentRepository.Add(equipment);
                await _context.SaveChangesAsync();

                _logger.Information("تجهیز پزشکی با موفقیت ایجاد شد - MedicalEquipmentId: {MedicalEquipmentId}", equipment.MedicalEquipmentId);
                return ServiceResult<MedicalEquipment>.Successful(equipment, "تجهیز پزشکی با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تجهیز پزشکی");
                return ServiceResult<MedicalEquipment>.Failed("خطا در ایجاد تجهیز پزشکی");
            }
        }

        public async Task<ServiceResult<MedicalEquipment>> UpdateMedicalEquipmentAsync(MedicalEquipmentCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", model.MedicalEquipmentId);

                var equipment = await _medicalEquipmentRepository.GetByIdAsync(model.MedicalEquipmentId);
                if (equipment == null)
                {
                    return ServiceResult<MedicalEquipment>.Failed("تجهیز پزشکی یافت نشد");
                }

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug) && model.Slug != equipment.Slug)
                {
                    var existingSlug = await _medicalEquipmentRepository.GetBySlugAsync(model.Slug);
                    if (existingSlug != null && existingSlug.MedicalEquipmentId != model.MedicalEquipmentId)
                    {
                        return ServiceResult<MedicalEquipment>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                equipment.EquipmentName = model.EquipmentName;
                equipment.Model = model.Model;
                equipment.Manufacturer = model.Manufacturer;
                equipment.Category = model.Category;
                equipment.Description = model.Description;
                equipment.TechnicalSpecifications = model.TechnicalSpecifications;
                equipment.ImageUrl = model.ImageUrl;
                equipment.ImageUrls = model.ImageUrls;
                equipment.VideoUrl = model.VideoUrl;
                equipment.PurchaseDate = model.PurchaseDate;
                equipment.InstallationDate = model.InstallationDate;
                equipment.WarrantyExpiryDate = model.WarrantyExpiryDate;
                equipment.Status = model.Status ?? equipment.Status ?? "Active";
                equipment.IsActive = model.IsActive;
                equipment.IsFeatured = model.IsFeatured;
                equipment.DisplayOrder = model.DisplayOrder;
                equipment.Features = model.Features;
                equipment.ShortDescription = model.ShortDescription;
                equipment.Slug = model.Slug ?? equipment.Slug ?? GenerateSlug(model.EquipmentName);
                equipment.MetaTitle = model.MetaTitle;
                equipment.MetaDescription = model.MetaDescription;
                equipment.UpdatedByUserId = _currentUserService.UserId;

                _medicalEquipmentRepository.Update(equipment);
                await _context.SaveChangesAsync();

                _logger.Information("تجهیز پزشکی با موفقیت به‌روزرسانی شد - MedicalEquipmentId: {MedicalEquipmentId}", equipment.MedicalEquipmentId);
                return ServiceResult<MedicalEquipment>.Successful(equipment, "تجهیز پزشکی با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", model.MedicalEquipmentId);
                return ServiceResult<MedicalEquipment>.Failed("خطا در به‌روزرسانی تجهیز پزشکی");
            }
        }

        public async Task<ServiceResult> DeleteMedicalEquipmentAsync(int medicalEquipmentId)
        {
            try
            {
                _logger.Information("حذف تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", medicalEquipmentId);

                var equipment = await _medicalEquipmentRepository.GetByIdAsync(medicalEquipmentId);
                if (equipment == null)
                {
                    return ServiceResult.Failed("تجهیز پزشکی یافت نشد");
                }

                _medicalEquipmentRepository.Delete(equipment);
                await _context.SaveChangesAsync();

                _logger.Information("تجهیز پزشکی با موفقیت حذف شد - MedicalEquipmentId: {MedicalEquipmentId}", medicalEquipmentId);
                return ServiceResult.Successful("تجهیز پزشکی با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", medicalEquipmentId);
                return ServiceResult.Failed("خطا در حذف تجهیز پزشکی");
            }
        }

        public async Task<ServiceResult> ActivateMedicalEquipmentAsync(int medicalEquipmentId)
        {
            try
            {
                var equipment = await _medicalEquipmentRepository.GetByIdAsync(medicalEquipmentId);
                if (equipment == null)
                {
                    return ServiceResult.Failed("تجهیز پزشکی یافت نشد");
                }

                equipment.IsActive = true;
                equipment.UpdatedByUserId = _currentUserService.UserId;

                _medicalEquipmentRepository.Update(equipment);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("تجهیز پزشکی با موفقیت فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", medicalEquipmentId);
                return ServiceResult.Failed("خطا در فعال‌سازی تجهیز پزشکی");
            }
        }

        public async Task<ServiceResult> DeactivateMedicalEquipmentAsync(int medicalEquipmentId)
        {
            try
            {
                var equipment = await _medicalEquipmentRepository.GetByIdAsync(medicalEquipmentId);
                if (equipment == null)
                {
                    return ServiceResult.Failed("تجهیز پزشکی یافت نشد");
                }

                equipment.IsActive = false;
                equipment.UpdatedByUserId = _currentUserService.UserId;

                _medicalEquipmentRepository.Update(equipment);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("تجهیز پزشکی با موفقیت غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", medicalEquipmentId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی تجهیز پزشکی");
            }
        }

        public async Task<ServiceResult> SetFeaturedAsync(int medicalEquipmentId, bool isFeatured)
        {
            try
            {
                var equipment = await _medicalEquipmentRepository.GetByIdAsync(medicalEquipmentId);
                if (equipment == null)
                {
                    return ServiceResult.Failed("تجهیز پزشکی یافت نشد");
                }

                equipment.IsFeatured = isFeatured;
                equipment.UpdatedByUserId = _currentUserService.UserId;

                _medicalEquipmentRepository.Update(equipment);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful(isFeatured ? "تجهیز پزشکی به عنوان ویژه تنظیم شد" : "تجهیز پزشکی از حالت ویژه خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", medicalEquipmentId);
                return ServiceResult.Failed("خطا در تنظیم وضعیت ویژه تجهیز پزشکی");
            }
        }

        public async Task<ServiceResult> IncrementViewCountAsync(int medicalEquipmentId)
        {
            try
            {
                var equipment = await _medicalEquipmentRepository.GetByIdAsync(medicalEquipmentId);
                if (equipment == null)
                {
                    return ServiceResult.Failed("تجهیز پزشکی یافت نشد");
                }

                equipment.ViewCount++;
                _medicalEquipmentRepository.Update(equipment);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد بازدید تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", medicalEquipmentId);
                return ServiceResult.Failed("خطا در افزایش تعداد بازدید");
            }
        }

        public async Task<ServiceResult<List<MedicalEquipmentPublicViewModel>>> GetActiveEquipmentsAsync()
        {
            try
            {
                var equipments = await _medicalEquipmentRepository.GetActiveEquipmentsAsync();
                
                var viewModels = equipments.Select(e => new MedicalEquipmentPublicViewModel
                {
                    MedicalEquipmentId = e.MedicalEquipmentId,
                    EquipmentName = e.EquipmentName,
                    Model = e.Model,
                    Manufacturer = e.Manufacturer,
                    Category = e.Category,
                    CategoryDisplayName = GetCategoryDisplayName(e.Category),
                    Description = e.Description,
                    ShortDescription = e.ShortDescription,
                    ImageUrl = e.ImageUrl,
                    ImageUrls = ParseJsonArray(e.ImageUrls),
                    VideoUrl = e.VideoUrl,
                    PurchaseDate = e.PurchaseDate,
                    Status = e.Status,
                    Features = ParseJsonArray(e.Features),
                    Slug = e.Slug,
                    ViewCount = e.ViewCount
                }).ToList();

                return ServiceResult<List<MedicalEquipmentPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تجهیزات پزشکی فعال");
                return ServiceResult<List<MedicalEquipmentPublicViewModel>>.Failed("خطا در دریافت تجهیزات پزشکی فعال");
            }
        }

        public async Task<ServiceResult<List<MedicalEquipmentPublicViewModel>>> GetFeaturedEquipmentsAsync(int count = 6)
        {
            try
            {
                var equipments = await _medicalEquipmentRepository.GetFeaturedEquipmentsAsync(count);
                
                var viewModels = equipments.Select(e => new MedicalEquipmentPublicViewModel
                {
                    MedicalEquipmentId = e.MedicalEquipmentId,
                    EquipmentName = e.EquipmentName,
                    Model = e.Model,
                    Manufacturer = e.Manufacturer,
                    Category = e.Category,
                    CategoryDisplayName = GetCategoryDisplayName(e.Category),
                    Description = e.Description,
                    ShortDescription = e.ShortDescription,
                    ImageUrl = e.ImageUrl,
                    ImageUrls = ParseJsonArray(e.ImageUrls),
                    VideoUrl = e.VideoUrl,
                    PurchaseDate = e.PurchaseDate,
                    Status = e.Status,
                    Features = ParseJsonArray(e.Features),
                    Slug = e.Slug,
                    ViewCount = e.ViewCount
                }).ToList();

                return ServiceResult<List<MedicalEquipmentPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تجهیزات پزشکی ویژه");
                return ServiceResult<List<MedicalEquipmentPublicViewModel>>.Failed("خطا در دریافت تجهیزات پزشکی ویژه");
            }
        }

        public async Task<ServiceResult<List<MedicalEquipmentPublicViewModel>>> GetByCategoryAsync(string category)
        {
            try
            {
                var equipments = await _medicalEquipmentRepository.GetByCategoryAsync(category);
                
                var viewModels = equipments.Select(e => new MedicalEquipmentPublicViewModel
                {
                    MedicalEquipmentId = e.MedicalEquipmentId,
                    EquipmentName = e.EquipmentName,
                    Model = e.Model,
                    Manufacturer = e.Manufacturer,
                    Category = e.Category,
                    CategoryDisplayName = GetCategoryDisplayName(e.Category),
                    Description = e.Description,
                    ShortDescription = e.ShortDescription,
                    ImageUrl = e.ImageUrl,
                    ImageUrls = ParseJsonArray(e.ImageUrls),
                    VideoUrl = e.VideoUrl,
                    PurchaseDate = e.PurchaseDate,
                    Status = e.Status,
                    Features = ParseJsonArray(e.Features),
                    Slug = e.Slug,
                    ViewCount = e.ViewCount
                }).ToList();

                return ServiceResult<List<MedicalEquipmentPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تجهیزات پزشکی بر اساس دسته‌بندی");
                return ServiceResult<List<MedicalEquipmentPublicViewModel>>.Failed("خطا در دریافت تجهیزات پزشکی بر اساس دسته‌بندی");
            }
        }

        public async Task<ServiceResult<List<MedicalEquipmentPublicViewModel>>> SearchEquipmentsAsync(string searchTerm)
        {
            try
            {
                var equipments = await _medicalEquipmentRepository.SearchEquipmentsAsync(searchTerm);
                
                var viewModels = equipments.Select(e => new MedicalEquipmentPublicViewModel
                {
                    MedicalEquipmentId = e.MedicalEquipmentId,
                    EquipmentName = e.EquipmentName,
                    Model = e.Model,
                    Manufacturer = e.Manufacturer,
                    Category = e.Category,
                    CategoryDisplayName = GetCategoryDisplayName(e.Category),
                    Description = e.Description,
                    ShortDescription = e.ShortDescription,
                    ImageUrl = e.ImageUrl,
                    ImageUrls = ParseJsonArray(e.ImageUrls),
                    VideoUrl = e.VideoUrl,
                    PurchaseDate = e.PurchaseDate,
                    Status = e.Status,
                    Features = ParseJsonArray(e.Features),
                    Slug = e.Slug,
                    ViewCount = e.ViewCount
                }).ToList();

                return ServiceResult<List<MedicalEquipmentPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی تجهیزات پزشکی");
                return ServiceResult<List<MedicalEquipmentPublicViewModel>>.Failed("خطا در جستجوی تجهیزات پزشکی");
            }
        }

        public async Task<ServiceResult<MedicalEquipment>> GetBySlugAsync(string slug)
        {
            try
            {
                var equipment = await _medicalEquipmentRepository.GetBySlugAsync(slug);
                if (equipment == null)
                {
                    return ServiceResult<MedicalEquipment>.Failed("تجهیز پزشکی یافت نشد");
                }

                return ServiceResult<MedicalEquipment>.Successful(equipment);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تجهیز پزشکی بر اساس Slug - Slug: {Slug}", slug);
                return ServiceResult<MedicalEquipment>.Failed("خطا در دریافت تجهیز پزشکی");
            }
        }

        #region Helper Methods

        private string GenerateSlug(string equipmentName)
        {
            if (string.IsNullOrEmpty(equipmentName))
                return Guid.NewGuid().ToString("N").Substring(0, 8);

            // تبدیل به حروف کوچک و حذف کاراکترهای خاص
            var slug = equipmentName.ToLower()
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
            return category switch
            {
                "Imaging" => "تصویربرداری",
                "Laboratory" => "آزمایشگاه",
                "Surgery" => "جراحی",
                "Monitoring" => "مانیتورینگ",
                "Therapy" => "درمانی",
                "Diagnostic" => "تشخیصی",
                "Emergency" => "اورژانس",
                "Rehabilitation" => "توانبخشی",
                _ => category ?? "عمومی"
            };
        }

        private List<string> ParseJsonArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        #endregion
    }
}

