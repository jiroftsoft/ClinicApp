using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.CMS;
using Newtonsoft.Json;
using Serilog;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت صفحه "درباره ما" (About Page)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class AboutPageService : IAboutPageService
    {
        private readonly IAboutPageRepository _aboutPageRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public AboutPageService(
            IAboutPageRepository aboutPageRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _aboutPageRepository = aboutPageRepository ?? throw new ArgumentNullException(nameof(aboutPageRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<Interfaces.PagedResult<AboutPageIndexViewModel>>> GetAboutPagesAsync(AboutPageSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست About Pages - Filter: {@Filter}", filter);

                var allAboutPages = await _aboutPageRepository.GetAllAsync(includeDeleted: false);

                // اعمال فیلترها
                var query = allAboutPages.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(a => a.ClinicName.Contains(searchTerm) || 
                                            (a.ClinicDescription != null && a.ClinicDescription.Contains(searchTerm)));
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(a => a.IsActive == filter.IsActive.Value);
                }

                var totalCount = query.Count();
                var aboutPages = query
                    .OrderBy(a => a.DisplayOrder)
                    .ThenByDescending(a => a.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var viewModels = aboutPages.Select(a => new AboutPageIndexViewModel
                {
                    AboutPageId = a.AboutPageId,
                    ClinicName = a.ClinicName,
                    ClinicDescription = a.ClinicDescription?.Length > 100 
                        ? a.ClinicDescription.Substring(0, 100) + "..." 
                        : a.ClinicDescription,
                    IsActive = a.IsActive,
                    DisplayOrder = a.DisplayOrder,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList();

                var pagedResult = new Interfaces.PagedResult<AboutPageIndexViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ServiceResult<Interfaces.PagedResult<AboutPageIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست About Pages");
                return ServiceResult<Interfaces.PagedResult<AboutPageIndexViewModel>>.Failed("خطا در دریافت لیست About Pages");
            }
        }

        public async Task<ServiceResult<AboutPageDetailsViewModel>> GetAboutPageDetailsAsync(int aboutPageId)
        {
            try
            {
                var aboutPage = await _aboutPageRepository.GetByIdAsync(aboutPageId);
                if (aboutPage == null)
                {
                    return ServiceResult<AboutPageDetailsViewModel>.Failed("صفحه About یافت نشد");
                }

                var viewModel = new AboutPageDetailsViewModel
                {
                    AboutPageId = aboutPage.AboutPageId,
                    ClinicName = aboutPage.ClinicName,
                    ClinicDescription = aboutPage.ClinicDescription,
                    EstablishedYear = aboutPage.EstablishedYear,
                    MissionValues = DeserializeJson<List<MissionValueViewModel>>(aboutPage.MissionValuesJson) ?? new List<MissionValueViewModel>(),
                    Licenses = DeserializeJson<List<LicenseViewModel>>(aboutPage.LicensesJson) ?? new List<LicenseViewModel>(),
                    RegulatoryBody = aboutPage.RegulatoryBody,
                    MedicalTeamDescription = aboutPage.MedicalTeamDescription,
                    InfrastructureDescription = aboutPage.InfrastructureDescription,
                    EthicalCommitments = DeserializeJson<List<EthicalCommitmentViewModel>>(aboutPage.EthicalCommitmentsJson) ?? new List<EthicalCommitmentViewModel>(),
                    HeroImageUrl = aboutPage.HeroImageUrl,
                    BackgroundImageUrl = aboutPage.BackgroundImageUrl,
                    IsActive = aboutPage.IsActive,
                    DisplayOrder = aboutPage.DisplayOrder,
                    MetaTitle = aboutPage.MetaTitle,
                    MetaDescription = aboutPage.MetaDescription,
                    Slug = aboutPage.Slug,
                    CreatedAt = aboutPage.CreatedAt,
                    CreatedByUserName = aboutPage.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = aboutPage.UpdatedAt,
                    UpdatedByUserName = aboutPage.UpdatedByUser?.UserName
                };

                return ServiceResult<AboutPageDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات About Page - AboutPageId: {AboutPageId}", aboutPageId);
                return ServiceResult<AboutPageDetailsViewModel>.Failed("خطا در دریافت جزئیات About Page");
            }
        }

        public async Task<ServiceResult<AboutPageCreateEditViewModel>> GetAboutPageForEditAsync(int aboutPageId)
        {
            try
            {
                var aboutPage = await _aboutPageRepository.GetByIdAsync(aboutPageId);
                if (aboutPage == null)
                {
                    return ServiceResult<AboutPageCreateEditViewModel>.Failed("صفحه About یافت نشد");
                }

                var viewModel = new AboutPageCreateEditViewModel
                {
                    AboutPageId = aboutPage.AboutPageId,
                    ClinicName = aboutPage.ClinicName,
                    ClinicDescription = aboutPage.ClinicDescription,
                    EstablishedYear = aboutPage.EstablishedYear,
                    MissionValues = DeserializeJson<List<MissionValueViewModel>>(aboutPage.MissionValuesJson) ?? new List<MissionValueViewModel>(),
                    Licenses = DeserializeJson<List<LicenseViewModel>>(aboutPage.LicensesJson) ?? new List<LicenseViewModel>(),
                    RegulatoryBody = aboutPage.RegulatoryBody,
                    MedicalTeamDescription = aboutPage.MedicalTeamDescription,
                    InfrastructureDescription = aboutPage.InfrastructureDescription,
                    EthicalCommitments = DeserializeJson<List<EthicalCommitmentViewModel>>(aboutPage.EthicalCommitmentsJson) ?? new List<EthicalCommitmentViewModel>(),
                    HeroImageUrl = aboutPage.HeroImageUrl,
                    BackgroundImageUrl = aboutPage.BackgroundImageUrl,
                    IsActive = aboutPage.IsActive,
                    DisplayOrder = aboutPage.DisplayOrder,
                    MetaTitle = aboutPage.MetaTitle,
                    MetaDescription = aboutPage.MetaDescription,
                    Slug = aboutPage.Slug
                };

                return ServiceResult<AboutPageCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت About Page برای ویرایش - AboutPageId: {AboutPageId}", aboutPageId);
                return ServiceResult<AboutPageCreateEditViewModel>.Failed("خطا در دریافت About Page برای ویرایش");
            }
        }

        public async Task<ServiceResult<AboutPage>> CreateAboutPageAsync(AboutPageCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد About Page جدید - ClinicName: {ClinicName}", model.ClinicName);

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var existing = await _aboutPageRepository.GetBySlugAsync(model.Slug);
                    if (existing != null)
                    {
                        return ServiceResult<AboutPage>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                var aboutPage = new AboutPage
                {
                    ClinicName = model.ClinicName,
                    ClinicDescription = model.ClinicDescription,
                    EstablishedYear = model.EstablishedYear,
                    MissionValuesJson = SerializeJson(model.MissionValues),
                    LicensesJson = SerializeJson(model.Licenses),
                    RegulatoryBody = model.RegulatoryBody,
                    MedicalTeamDescription = model.MedicalTeamDescription,
                    InfrastructureDescription = model.InfrastructureDescription,
                    EthicalCommitmentsJson = SerializeJson(model.EthicalCommitments),
                    HeroImageUrl = model.HeroImageUrl,
                    BackgroundImageUrl = model.BackgroundImageUrl,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder,
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    Slug = model.Slug ?? GenerateSlug(model.ClinicName),
                    CreatedByUserId = _currentUserService.UserId
                };

                _aboutPageRepository.Add(aboutPage);
                await _context.SaveChangesAsync();

                _logger.Information("About Page با موفقیت ایجاد شد - AboutPageId: {AboutPageId}", aboutPage.AboutPageId);
                return ServiceResult<AboutPage>.Successful(aboutPage, "About Page با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد About Page");
                return ServiceResult<AboutPage>.Failed("خطا در ایجاد About Page");
            }
        }

        public async Task<ServiceResult<AboutPage>> UpdateAboutPageAsync(AboutPageCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی About Page - AboutPageId: {AboutPageId}", model.AboutPageId);

                var aboutPage = await _aboutPageRepository.GetByIdAsync(model.AboutPageId);
                if (aboutPage == null)
                {
                    return ServiceResult<AboutPage>.Failed("صفحه About یافت نشد");
                }

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug) && model.Slug != aboutPage.Slug)
                {
                    var existing = await _aboutPageRepository.GetBySlugAsync(model.Slug);
                    if (existing != null && existing.AboutPageId != model.AboutPageId)
                    {
                        return ServiceResult<AboutPage>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                aboutPage.ClinicName = model.ClinicName;
                aboutPage.ClinicDescription = model.ClinicDescription;
                aboutPage.EstablishedYear = model.EstablishedYear;
                aboutPage.MissionValuesJson = SerializeJson(model.MissionValues);
                aboutPage.LicensesJson = SerializeJson(model.Licenses);
                aboutPage.RegulatoryBody = model.RegulatoryBody;
                aboutPage.MedicalTeamDescription = model.MedicalTeamDescription;
                aboutPage.InfrastructureDescription = model.InfrastructureDescription;
                aboutPage.EthicalCommitmentsJson = SerializeJson(model.EthicalCommitments);
                aboutPage.HeroImageUrl = model.HeroImageUrl;
                aboutPage.BackgroundImageUrl = model.BackgroundImageUrl;
                aboutPage.IsActive = model.IsActive;
                aboutPage.DisplayOrder = model.DisplayOrder;
                aboutPage.MetaTitle = model.MetaTitle;
                aboutPage.MetaDescription = model.MetaDescription;
                aboutPage.Slug = model.Slug ?? GenerateSlug(model.ClinicName);
                aboutPage.UpdatedByUserId = _currentUserService.UserId;
                aboutPage.UpdatedAt = DateTime.Now;

                _aboutPageRepository.Update(aboutPage);
                await _context.SaveChangesAsync();

                _logger.Information("About Page با موفقیت به‌روزرسانی شد - AboutPageId: {AboutPageId}", aboutPage.AboutPageId);
                return ServiceResult<AboutPage>.Successful(aboutPage, "About Page با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی About Page");
                return ServiceResult<AboutPage>.Failed("خطا در به‌روزرسانی About Page");
            }
        }

        public async Task<ServiceResult> DeleteAboutPageAsync(int aboutPageId)
        {
            try
            {
                _logger.Information("حذف About Page - AboutPageId: {AboutPageId}", aboutPageId);

                var aboutPage = await _aboutPageRepository.GetByIdAsync(aboutPageId);
                if (aboutPage == null)
                {
                    return ServiceResult.Failed("صفحه About یافت نشد");
                }

                _aboutPageRepository.Delete(aboutPage);
                await _context.SaveChangesAsync();

                _logger.Information("About Page با موفقیت حذف شد - AboutPageId: {AboutPageId}", aboutPageId);
                return ServiceResult.Successful("About Page با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف About Page - AboutPageId: {AboutPageId}", aboutPageId);
                return ServiceResult.Failed("خطا در حذف About Page");
            }
        }

        public async Task<ServiceResult> ActivateAboutPageAsync(int aboutPageId)
        {
            try
            {
                var aboutPage = await _aboutPageRepository.GetByIdAsync(aboutPageId);
                if (aboutPage == null)
                {
                    return ServiceResult.Failed("صفحه About یافت نشد");
                }

                aboutPage.IsActive = true;
                aboutPage.UpdatedByUserId = _currentUserService.UserId;
                aboutPage.UpdatedAt = DateTime.Now;

                _aboutPageRepository.Update(aboutPage);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("صفحه About فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی About Page - AboutPageId: {AboutPageId}", aboutPageId);
                return ServiceResult.Failed("خطا در فعال‌سازی About Page");
            }
        }

        public async Task<ServiceResult> DeactivateAboutPageAsync(int aboutPageId)
        {
            try
            {
                var aboutPage = await _aboutPageRepository.GetByIdAsync(aboutPageId);
                if (aboutPage == null)
                {
                    return ServiceResult.Failed("صفحه About یافت نشد");
                }

                aboutPage.IsActive = false;
                aboutPage.UpdatedByUserId = _currentUserService.UserId;
                aboutPage.UpdatedAt = DateTime.Now;

                _aboutPageRepository.Update(aboutPage);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("صفحه About غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی About Page - AboutPageId: {AboutPageId}", aboutPageId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی About Page");
            }
        }

        public async Task<ServiceResult<AboutPagePublicViewModel>> GetActiveAboutPageAsync()
        {
            try
            {
                var aboutPage = await _aboutPageRepository.GetActiveAboutPageAsync();
                if (aboutPage == null)
                {
                    return ServiceResult<AboutPagePublicViewModel>.Failed("صفحه About فعالی یافت نشد");
                }

                var viewModel = new AboutPagePublicViewModel
                {
                    AboutPageId = aboutPage.AboutPageId,
                    ClinicName = aboutPage.ClinicName,
                    ClinicDescription = aboutPage.ClinicDescription,
                    EstablishedYear = aboutPage.EstablishedYear,
                    MissionValues = DeserializeJson<List<MissionValueViewModel>>(aboutPage.MissionValuesJson) ?? new List<MissionValueViewModel>(),
                    Licenses = DeserializeJson<List<LicenseViewModel>>(aboutPage.LicensesJson) ?? new List<LicenseViewModel>(),
                    RegulatoryBody = aboutPage.RegulatoryBody,
                    MedicalTeamDescription = aboutPage.MedicalTeamDescription,
                    InfrastructureDescription = aboutPage.InfrastructureDescription,
                    EthicalCommitments = DeserializeJson<List<EthicalCommitmentViewModel>>(aboutPage.EthicalCommitmentsJson) ?? new List<EthicalCommitmentViewModel>(),
                    HeroImageUrl = aboutPage.HeroImageUrl,
                    BackgroundImageUrl = aboutPage.BackgroundImageUrl,
                    MetaTitle = aboutPage.MetaTitle,
                    MetaDescription = aboutPage.MetaDescription,
                    Slug = aboutPage.Slug
                };

                return ServiceResult<AboutPagePublicViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت About Page فعال");
                return ServiceResult<AboutPagePublicViewModel>.Failed("خطا در دریافت About Page فعال");
            }
        }

        public async Task<ServiceResult<AboutPage>> GetBySlugAsync(string slug)
        {
            try
            {
                var aboutPage = await _aboutPageRepository.GetBySlugAsync(slug);
                if (aboutPage == null)
                {
                    return ServiceResult<AboutPage>.Failed("صفحه About یافت نشد");
                }

                return ServiceResult<AboutPage>.Successful(aboutPage);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت About Page با Slug - Slug: {Slug}", slug);
                return ServiceResult<AboutPage>.Failed("خطا در دریافت About Page");
            }
        }

        #region Helper Methods

        private string SerializeJson<T>(T obj)
        {
            if (obj == null)
                return null;

            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در Serialize JSON");
                return null;
            }
        }

        private T DeserializeJson<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در Deserialize JSON");
                return default(T);
            }
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            // تبدیل به حروف کوچک و حذف کاراکترهای خاص
            var slug = text.ToLower()
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
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            
            // حذف خط تیره‌های تکراری
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            
            // حذف خط تیره از ابتدا و انتها
            slug = slug.Trim('-');

            return slug;
        }

        #endregion
    }
}
