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
    /// سرویس مدیریت سوالات متداول (FAQ)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class FAQService : IFAQService
    {
        private readonly IFAQRepository _faqRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public FAQService(
            IFAQRepository faqRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _faqRepository = faqRepository ?? throw new ArgumentNullException(nameof(faqRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<FAQIndexViewModel>>> GetFAQsAsync(FAQSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست FAQ - Filter: {@Filter}", filter);

                var allFAQs = await _faqRepository.GetAllAsync(includeDeleted: false);

                // اعمال فیلترها
                var query = allFAQs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(f => f.Question.Contains(searchTerm) || 
                                            f.Answer.Contains(searchTerm) ||
                                            (f.Tags != null && f.Tags.Contains(searchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(filter.Category))
                {
                    query = query.Where(f => f.Category == filter.Category);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(f => f.IsActive == filter.IsActive.Value);
                }

                if (filter.IsFeatured.HasValue)
                {
                    query = query.Where(f => f.IsFeatured == filter.IsFeatured.Value);
                }

                var totalCount = query.Count();
                var faqs = query
                    .OrderBy(f => f.DisplayOrder)
                    .ThenByDescending(f => f.ViewCount)
                    .ThenByDescending(f => f.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var viewModels = faqs.Select(f => new FAQIndexViewModel
                {
                    FAQId = f.FAQId,
                    Question = f.Question,
                    Answer = f.Answer,
                    Category = f.Category,
                    Tags = f.Tags,
                    IsActive = f.IsActive,
                    IsFeatured = f.IsFeatured,
                    DisplayOrder = f.DisplayOrder,
                    ViewCount = f.ViewCount,
                    CreatedAt = f.CreatedAt
                }).ToList();

                var pagedResult = new PagedResult<FAQIndexViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ServiceResult<PagedResult<FAQIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست FAQ");
                return ServiceResult<PagedResult<FAQIndexViewModel>>.Failed("خطا در دریافت لیست FAQ");
            }
        }

        public async Task<ServiceResult<FAQDetailsViewModel>> GetFAQDetailsAsync(int faqId)
        {
            try
            {
                var faq = await _faqRepository.GetByIdAsync(faqId);
                if (faq == null)
                {
                    return ServiceResult<FAQDetailsViewModel>.Failed("FAQ یافت نشد");
                }

                var viewModel = new FAQDetailsViewModel
                {
                    FAQId = faq.FAQId,
                    Question = faq.Question,
                    Answer = faq.Answer,
                    Category = faq.Category,
                    Tags = faq.Tags,
                    RelatedLinkUrl = faq.RelatedLinkUrl,
                    IsActive = faq.IsActive,
                    IsFeatured = faq.IsFeatured,
                    DisplayOrder = faq.DisplayOrder,
                    ViewCount = faq.ViewCount,
                    MetaTitle = faq.MetaTitle,
                    MetaDescription = faq.MetaDescription,
                    Slug = faq.Slug,
                    CreatedAt = faq.CreatedAt,
                    CreatedByUserName = faq.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = faq.UpdatedAt,
                    UpdatedByUserName = faq.UpdatedByUser?.UserName
                };

                return ServiceResult<FAQDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات FAQ - FAQId: {FAQId}", faqId);
                return ServiceResult<FAQDetailsViewModel>.Failed("خطا در دریافت جزئیات FAQ");
            }
        }

        public async Task<ServiceResult<FAQCreateEditViewModel>> GetFAQForEditAsync(int faqId)
        {
            try
            {
                var faq = await _faqRepository.GetByIdAsync(faqId);
                if (faq == null)
                {
                    return ServiceResult<FAQCreateEditViewModel>.Failed("FAQ یافت نشد");
                }

                var viewModel = new FAQCreateEditViewModel
                {
                    FAQId = faq.FAQId,
                    Question = faq.Question,
                    Answer = faq.Answer,
                    Category = faq.Category,
                    Tags = faq.Tags,
                    RelatedLinkUrl = faq.RelatedLinkUrl,
                    IsActive = faq.IsActive,
                    IsFeatured = faq.IsFeatured,
                    DisplayOrder = faq.DisplayOrder,
                    MetaTitle = faq.MetaTitle,
                    MetaDescription = faq.MetaDescription,
                    Slug = faq.Slug
                };

                return ServiceResult<FAQCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت FAQ برای ویرایش - FAQId: {FAQId}", faqId);
                return ServiceResult<FAQCreateEditViewModel>.Failed("خطا در دریافت FAQ برای ویرایش");
            }
        }

        public async Task<ServiceResult<FAQ>> CreateFAQAsync(FAQCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد FAQ جدید - Question: {Question}", model.Question);

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var existing = await _faqRepository.GetBySlugAsync(model.Slug);
                    if (existing != null)
                    {
                        return ServiceResult<FAQ>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                var faq = new FAQ
                {
                    Question = model.Question,
                    Answer = model.Answer,
                    Category = model.Category ?? "general",
                    Tags = model.Tags,
                    RelatedLinkUrl = model.RelatedLinkUrl,
                    IsActive = model.IsActive,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder,
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    Slug = model.Slug ?? GenerateSlug(model.Question),
                    CreatedByUserId = _currentUserService.UserId
                };

                _faqRepository.Add(faq);
                await _context.SaveChangesAsync();

                _logger.Information("FAQ با موفقیت ایجاد شد - FAQId: {FAQId}", faq.FAQId);
                return ServiceResult<FAQ>.Successful(faq, "FAQ با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد FAQ");
                return ServiceResult<FAQ>.Failed("خطا در ایجاد FAQ");
            }
        }

        public async Task<ServiceResult<FAQ>> UpdateFAQAsync(FAQCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی FAQ - FAQId: {FAQId}", model.FAQId);

                var faq = await _faqRepository.GetByIdAsync(model.FAQId);
                if (faq == null)
                {
                    return ServiceResult<FAQ>.Failed("FAQ یافت نشد");
                }

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug) && model.Slug != faq.Slug)
                {
                    var existing = await _faqRepository.GetBySlugAsync(model.Slug);
                    if (existing != null && existing.FAQId != model.FAQId)
                    {
                        return ServiceResult<FAQ>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                faq.Question = model.Question;
                faq.Answer = model.Answer;
                faq.Category = model.Category ?? faq.Category ?? "general";
                faq.Tags = model.Tags;
                faq.RelatedLinkUrl = model.RelatedLinkUrl;
                faq.IsActive = model.IsActive;
                faq.IsFeatured = model.IsFeatured;
                faq.DisplayOrder = model.DisplayOrder;
                faq.MetaTitle = model.MetaTitle;
                faq.MetaDescription = model.MetaDescription;
                faq.Slug = model.Slug ?? faq.Slug ?? GenerateSlug(model.Question);
                faq.UpdatedByUserId = _currentUserService.UserId;

                _faqRepository.Update(faq);
                await _context.SaveChangesAsync();

                _logger.Information("FAQ با موفقیت به‌روزرسانی شد - FAQId: {FAQId}", faq.FAQId);
                return ServiceResult<FAQ>.Successful(faq, "FAQ با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی FAQ - FAQId: {FAQId}", model.FAQId);
                return ServiceResult<FAQ>.Failed("خطا در به‌روزرسانی FAQ");
            }
        }

        public async Task<ServiceResult> DeleteFAQAsync(int faqId)
        {
            try
            {
                _logger.Information("حذف FAQ - FAQId: {FAQId}", faqId);

                var faq = await _faqRepository.GetByIdAsync(faqId);
                if (faq == null)
                {
                    return ServiceResult.Failed("FAQ یافت نشد");
                }

                _faqRepository.Delete(faq);
                await _context.SaveChangesAsync();

                _logger.Information("FAQ با موفقیت حذف شد - FAQId: {FAQId}", faqId);
                return ServiceResult.Successful("FAQ با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف FAQ - FAQId: {FAQId}", faqId);
                return ServiceResult.Failed("خطا در حذف FAQ");
            }
        }

        public async Task<ServiceResult> ActivateFAQAsync(int faqId)
        {
            try
            {
                var faq = await _faqRepository.GetByIdAsync(faqId);
                if (faq == null)
                {
                    return ServiceResult.Failed("FAQ یافت نشد");
                }

                faq.IsActive = true;
                faq.UpdatedByUserId = _currentUserService.UserId;

                _faqRepository.Update(faq);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("FAQ با موفقیت فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی FAQ - FAQId: {FAQId}", faqId);
                return ServiceResult.Failed("خطا در فعال‌سازی FAQ");
            }
        }

        public async Task<ServiceResult> DeactivateFAQAsync(int faqId)
        {
            try
            {
                var faq = await _faqRepository.GetByIdAsync(faqId);
                if (faq == null)
                {
                    return ServiceResult.Failed("FAQ یافت نشد");
                }

                faq.IsActive = false;
                faq.UpdatedByUserId = _currentUserService.UserId;

                _faqRepository.Update(faq);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("FAQ با موفقیت غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی FAQ - FAQId: {FAQId}", faqId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی FAQ");
            }
        }

        public async Task<ServiceResult> SetFeaturedAsync(int faqId, bool isFeatured)
        {
            try
            {
                var faq = await _faqRepository.GetByIdAsync(faqId);
                if (faq == null)
                {
                    return ServiceResult.Failed("FAQ یافت نشد");
                }

                faq.IsFeatured = isFeatured;
                faq.UpdatedByUserId = _currentUserService.UserId;

                _faqRepository.Update(faq);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful(isFeatured ? "FAQ به عنوان ویژه تنظیم شد" : "FAQ از حالت ویژه خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه FAQ - FAQId: {FAQId}", faqId);
                return ServiceResult.Failed("خطا در تنظیم وضعیت ویژه FAQ");
            }
        }

        public async Task<ServiceResult> IncrementViewCountAsync(int faqId)
        {
            try
            {
                var faq = await _faqRepository.GetByIdAsync(faqId);
                if (faq != null)
                {
                    faq.ViewCount++;
                    _faqRepository.Update(faq);
                    await _context.SaveChangesAsync();
                }

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد بازدید FAQ - FAQId: {FAQId}", faqId);
                // خطا در افزایش ViewCount نباید باعث شکست شود
                return ServiceResult.Successful();
            }
        }

        public async Task<ServiceResult<List<FAQPublicViewModel>>> GetPublicFAQsAsync(string category = null)
        {
            try
            {
                var faqs = await _faqRepository.GetActiveFAQsAsync(category);
                
                var viewModels = faqs.Select(f => new FAQPublicViewModel
                {
                    FAQId = f.FAQId,
                    Question = f.Question,
                    Answer = f.Answer,
                    Category = f.Category,
                    CategoryDisplayName = GetCategoryDisplayName(f.Category),
                    Tags = !string.IsNullOrEmpty(f.Tags) 
                        ? f.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList()
                        : new List<string>(),
                    RelatedLinkUrl = f.RelatedLinkUrl,
                    ViewCount = f.ViewCount,
                    Slug = f.Slug
                }).ToList();

                return ServiceResult<List<FAQPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت FAQ های عمومی");
                return ServiceResult<List<FAQPublicViewModel>>.Failed("خطا در دریافت FAQ های عمومی");
            }
        }

        public async Task<ServiceResult<List<FAQPublicViewModel>>> GetFeaturedFAQsAsync(int count = 5)
        {
            try
            {
                var faqs = await _faqRepository.GetFeaturedFAQsAsync(count);
                
                var viewModels = faqs.Select(f => new FAQPublicViewModel
                {
                    FAQId = f.FAQId,
                    Question = f.Question,
                    Answer = f.Answer,
                    Category = f.Category,
                    CategoryDisplayName = GetCategoryDisplayName(f.Category),
                    Tags = !string.IsNullOrEmpty(f.Tags) 
                        ? f.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList()
                        : new List<string>(),
                    RelatedLinkUrl = f.RelatedLinkUrl,
                    ViewCount = f.ViewCount,
                    Slug = f.Slug
                }).ToList();

                return ServiceResult<List<FAQPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت FAQ های ویژه");
                return ServiceResult<List<FAQPublicViewModel>>.Failed("خطا در دریافت FAQ های ویژه");
            }
        }

        public async Task<ServiceResult<List<FAQCategoryViewModel>>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _faqRepository.GetCategoriesAsync();
                var faqs = await _faqRepository.GetActiveFAQsAsync();
                
                var viewModels = categories.Select(c => new FAQCategoryViewModel
                {
                    Category = c,
                    DisplayName = GetCategoryDisplayName(c),
                    Count = faqs.Count(f => f.Category == c)
                }).OrderBy(c => c.DisplayName).ToList();

                return ServiceResult<List<FAQCategoryViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی‌های FAQ");
                return ServiceResult<List<FAQCategoryViewModel>>.Failed("خطا در دریافت دسته‌بندی‌های FAQ");
            }
        }

        public async Task<ServiceResult<List<FAQPublicViewModel>>> SearchFAQsAsync(string searchTerm)
        {
            try
            {
                var faqs = await _faqRepository.SearchFAQsAsync(searchTerm);
                
                var viewModels = faqs.Select(f => new FAQPublicViewModel
                {
                    FAQId = f.FAQId,
                    Question = f.Question,
                    Answer = f.Answer,
                    Category = f.Category,
                    CategoryDisplayName = GetCategoryDisplayName(f.Category),
                    Tags = !string.IsNullOrEmpty(f.Tags) 
                        ? f.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList()
                        : new List<string>(),
                    RelatedLinkUrl = f.RelatedLinkUrl,
                    ViewCount = f.ViewCount,
                    Slug = f.Slug
                }).ToList();

                return ServiceResult<List<FAQPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی FAQ");
                return ServiceResult<List<FAQPublicViewModel>>.Failed("خطا در جستجوی FAQ");
            }
        }

        public async Task<ServiceResult<FAQ>> GetBySlugAsync(string slug)
        {
            try
            {
                var faq = await _faqRepository.GetBySlugAsync(slug);
                if (faq == null)
                {
                    return ServiceResult<FAQ>.Failed("FAQ یافت نشد");
                }

                return ServiceResult<FAQ>.Successful(faq);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت FAQ بر اساس Slug - Slug: {Slug}", slug);
                return ServiceResult<FAQ>.Failed("خطا در دریافت FAQ");
            }
        }

        #region Helper Methods

        private string GenerateSlug(string question)
        {
            if (string.IsNullOrEmpty(question))
                return Guid.NewGuid().ToString("N").Substring(0, 8);

            // تبدیل به حروف کوچک و حذف کاراکترهای خاص
            var slug = question.ToLower()
                .Replace(" ", "-")
                .Replace("؟", "")
                .Replace("?", "")
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
                "appointment" => "نوبت‌دهی",
                "insurance" => "بیمه",
                "services" => "خدمات",
                "costs" => "هزینه‌ها",
                "general" => "عمومی",
                _ => category
            };
        }

        #endregion
    }
}

