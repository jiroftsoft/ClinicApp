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
using Serilog;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت مقالات و پست‌های بلاگ
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class BlogPostService : IBlogPostService
    {
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public BlogPostService(
            IBlogPostRepository blogPostRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _blogPostRepository = blogPostRepository ?? throw new ArgumentNullException(nameof(blogPostRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<PagedResult<BlogPostIndexViewModel>>> GetBlogPostsAsync(BlogPostSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست مقالات - Filter: {@Filter}", filter);

                var allPosts = await _blogPostRepository.GetAllAsync(includeDeleted: false);

                // اعمال فیلترها
                var query = allPosts.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim();
                    query = query.Where(b => b.Title.Contains(searchTerm) || 
                                            (b.Summary != null && b.Summary.Contains(searchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(filter.CategoryName))
                {
                    query = query.Where(b => b.CategoryName == filter.CategoryName);
                }

                if (filter.IsPublished.HasValue)
                {
                    query = query.Where(b => b.IsPublished == filter.IsPublished.Value);
                }

                if (filter.IsFeatured.HasValue)
                {
                    query = query.Where(b => b.IsFeatured == filter.IsFeatured.Value);
                }

                var totalCount = query.Count();
                var posts = query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var viewModels = posts.Select(b => new BlogPostIndexViewModel
                {
                    BlogPostId = b.BlogPostId,
                    Title = b.Title,
                    Summary = b.Summary,
                    CategoryName = b.CategoryName,
                    AuthorName = b.AuthorName,
                    IsPublished = b.IsPublished,
                    IsFeatured = b.IsFeatured,
                    PublishedAt = b.PublishedAt,
                    CreatedAt = b.CreatedAt,
                    ViewCount = b.ViewCount
                }).ToList();

                var pagedResult = new PagedResult<BlogPostIndexViewModel>
                {
                    Items = viewModels,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ServiceResult<PagedResult<BlogPostIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست مقالات");
                return ServiceResult<PagedResult<BlogPostIndexViewModel>>.Failed("خطا در دریافت لیست مقالات");
            }
        }

        public async Task<ServiceResult<BlogPostDetailsViewModel>> GetBlogPostDetailsAsync(int blogPostId)
        {
            try
            {
                var blogPost = await _blogPostRepository.GetByIdAsync(blogPostId);
                if (blogPost == null)
                {
                    return ServiceResult<BlogPostDetailsViewModel>.Failed("مقاله یافت نشد");
                }

                var viewModel = new BlogPostDetailsViewModel
                {
                    BlogPostId = blogPost.BlogPostId,
                    Title = blogPost.Title,
                    Summary = blogPost.Summary,
                    Content = blogPost.Content,
                    ImageUrl = blogPost.ImageUrl,
                    ThumbnailUrl = blogPost.ThumbnailUrl,
                    AuthorName = blogPost.AuthorName,
                    CategoryName = blogPost.CategoryName,
                    PublishedAt = blogPost.PublishedAt,
                    IsPublished = blogPost.IsPublished,
                    IsFeatured = blogPost.IsFeatured,
                    ViewCount = blogPost.ViewCount,
                    DisplayOrder = blogPost.DisplayOrder,
                    MetaTitle = blogPost.MetaTitle,
                    MetaDescription = blogPost.MetaDescription,
                    Slug = blogPost.Slug,
                    CreatedAt = blogPost.CreatedAt,
                    CreatedByUserName = blogPost.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = blogPost.UpdatedAt,
                    UpdatedByUserName = blogPost.UpdatedByUser?.UserName
                };

                return ServiceResult<BlogPostDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات مقاله - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult<BlogPostDetailsViewModel>.Failed("خطا در دریافت جزئیات مقاله");
            }
        }

        public async Task<ServiceResult<BlogPostCreateEditViewModel>> GetBlogPostForEditAsync(int blogPostId)
        {
            try
            {
                var blogPost = await _blogPostRepository.GetByIdAsync(blogPostId);
                if (blogPost == null)
                {
                    return ServiceResult<BlogPostCreateEditViewModel>.Failed("مقاله یافت نشد");
                }

                var viewModel = new BlogPostCreateEditViewModel
                {
                    BlogPostId = blogPost.BlogPostId,
                    Title = blogPost.Title,
                    Summary = blogPost.Summary,
                    Content = blogPost.Content,
                    ImageUrl = blogPost.ImageUrl,
                    ThumbnailUrl = blogPost.ThumbnailUrl,
                    AuthorName = blogPost.AuthorName,
                    CategoryName = blogPost.CategoryName,
                    PublishedAt = blogPost.PublishedAt,
                    IsPublished = blogPost.IsPublished,
                    IsFeatured = blogPost.IsFeatured,
                    DisplayOrder = blogPost.DisplayOrder,
                    MetaTitle = blogPost.MetaTitle,
                    MetaDescription = blogPost.MetaDescription,
                    Slug = blogPost.Slug
                };

                return ServiceResult<BlogPostCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت مقاله برای ویرایش - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult<BlogPostCreateEditViewModel>.Failed("خطا در دریافت مقاله برای ویرایش");
            }
        }

        public async Task<ServiceResult<BlogPost>> CreateBlogPostAsync(BlogPostCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد مقاله جدید - Title: {Title}", model.Title);

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var existing = await _blogPostRepository.GetBySlugAsync(model.Slug);
                    if (existing != null)
                    {
                        return ServiceResult<BlogPost>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                var blogPost = new BlogPost
                {
                    Title = model.Title,
                    Summary = model.Summary,
                    Content = model.Content,
                    ImageUrl = model.ImageUrl,
                    ThumbnailUrl = model.ThumbnailUrl,
                    AuthorName = model.AuthorName,
                    CategoryName = model.CategoryName,
                    PublishedAt = model.IsPublished ? (model.PublishedAt ?? DateTime.Now) : null,
                    IsPublished = model.IsPublished,
                    IsFeatured = model.IsFeatured,
                    DisplayOrder = model.DisplayOrder ?? 0,
                    MetaTitle = model.MetaTitle,
                    MetaDescription = model.MetaDescription,
                    Slug = model.Slug ?? GenerateSlug(model.Title),
                    CreatedByUserId = _currentUserService.UserId
                };

                _blogPostRepository.Add(blogPost);
                await _context.SaveChangesAsync();

                _logger.Information("مقاله با موفقیت ایجاد شد - BlogPostId: {BlogPostId}", blogPost.BlogPostId);
                return ServiceResult<BlogPost>.Successful(blogPost, "مقاله با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد مقاله");
                return ServiceResult<BlogPost>.Failed("خطا در ایجاد مقاله");
            }
        }

        public async Task<ServiceResult<BlogPost>> UpdateBlogPostAsync(BlogPostCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی مقاله - BlogPostId: {BlogPostId}", model.BlogPostId);

                var blogPost = await _blogPostRepository.GetByIdAsync(model.BlogPostId);
                if (blogPost == null)
                {
                    return ServiceResult<BlogPost>.Failed("مقاله یافت نشد");
                }

                // بررسی تکراری بودن Slug
                if (!string.IsNullOrEmpty(model.Slug) && model.Slug != blogPost.Slug)
                {
                    var existing = await _blogPostRepository.GetBySlugAsync(model.Slug);
                    if (existing != null && existing.BlogPostId != model.BlogPostId)
                    {
                        return ServiceResult<BlogPost>.Failed("این Slug قبلاً استفاده شده است");
                    }
                }

                blogPost.Title = model.Title;
                blogPost.Summary = model.Summary;
                blogPost.Content = model.Content;
                blogPost.ImageUrl = model.ImageUrl;
                blogPost.ThumbnailUrl = model.ThumbnailUrl;
                blogPost.AuthorName = model.AuthorName;
                blogPost.CategoryName = model.CategoryName;
                blogPost.IsPublished = model.IsPublished;
                blogPost.IsFeatured = model.IsFeatured;
                blogPost.DisplayOrder = model.DisplayOrder ?? blogPost.DisplayOrder ?? 0;
                blogPost.MetaTitle = model.MetaTitle;
                blogPost.MetaDescription = model.MetaDescription;
                blogPost.Slug = model.Slug ?? blogPost.Slug ?? GenerateSlug(model.Title);

                if (model.IsPublished && !blogPost.PublishedAt.HasValue)
                {
                    blogPost.PublishedAt = model.PublishedAt ?? DateTime.Now;
                }
                else if (!model.IsPublished)
                {
                    blogPost.PublishedAt = null;
                }
                else if (model.PublishedAt.HasValue)
                {
                    blogPost.PublishedAt = model.PublishedAt;
                }

                blogPost.UpdatedByUserId = _currentUserService.UserId;

                _blogPostRepository.Update(blogPost);
                await _context.SaveChangesAsync();

                _logger.Information("مقاله با موفقیت به‌روزرسانی شد - BlogPostId: {BlogPostId}", blogPost.BlogPostId);
                return ServiceResult<BlogPost>.Successful(blogPost, "مقاله با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی مقاله - BlogPostId: {BlogPostId}", model.BlogPostId);
                return ServiceResult<BlogPost>.Failed("خطا در به‌روزرسانی مقاله");
            }
        }

        public async Task<ServiceResult> DeleteBlogPostAsync(int blogPostId)
        {
            try
            {
                _logger.Information("حذف مقاله - BlogPostId: {BlogPostId}", blogPostId);

                var blogPost = await _blogPostRepository.GetByIdAsync(blogPostId);
                if (blogPost == null)
                {
                    return ServiceResult.Failed("مقاله یافت نشد");
                }

                _blogPostRepository.Delete(blogPost);
                await _context.SaveChangesAsync();

                _logger.Information("مقاله با موفقیت حذف شد - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult.Successful("مقاله با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف مقاله - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult.Failed("خطا در حذف مقاله");
            }
        }

        public async Task<ServiceResult> PublishBlogPostAsync(int blogPostId)
        {
            try
            {
                var blogPost = await _blogPostRepository.GetByIdAsync(blogPostId);
                if (blogPost == null)
                {
                    return ServiceResult.Failed("مقاله یافت نشد");
                }

                blogPost.IsPublished = true;
                if (!blogPost.PublishedAt.HasValue)
                {
                    blogPost.PublishedAt = DateTime.Now;
                }
                blogPost.UpdatedByUserId = _currentUserService.UserId;

                _blogPostRepository.Update(blogPost);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("مقاله با موفقیت منتشر شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتشار مقاله - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult.Failed("خطا در انتشار مقاله");
            }
        }

        public async Task<ServiceResult> UnpublishBlogPostAsync(int blogPostId)
        {
            try
            {
                var blogPost = await _blogPostRepository.GetByIdAsync(blogPostId);
                if (blogPost == null)
                {
                    return ServiceResult.Failed("مقاله یافت نشد");
                }

                blogPost.IsPublished = false;
                blogPost.UpdatedByUserId = _currentUserService.UserId;

                _blogPostRepository.Update(blogPost);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("مقاله با موفقیت از حالت انتشار خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو انتشار مقاله - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult.Failed("خطا در لغو انتشار مقاله");
            }
        }

        public async Task<ServiceResult> SetFeaturedAsync(int blogPostId, bool isFeatured)
        {
            try
            {
                var blogPost = await _blogPostRepository.GetByIdAsync(blogPostId);
                if (blogPost == null)
                {
                    return ServiceResult.Failed("مقاله یافت نشد");
                }

                blogPost.IsFeatured = isFeatured;
                blogPost.UpdatedByUserId = _currentUserService.UserId;

                _blogPostRepository.Update(blogPost);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful(isFeatured ? "مقاله به عنوان ویژه تنظیم شد" : "مقاله از حالت ویژه خارج شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه مقاله - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult.Failed("خطا در تنظیم وضعیت ویژه مقاله");
            }
        }

        public async Task<ServiceResult<BlogPost>> GetBySlugAsync(string slug)
        {
            try
            {
                var blogPost = await _blogPostRepository.GetBySlugAsync(slug);
                if (blogPost == null)
                {
                    return ServiceResult<BlogPost>.Failed("مقاله یافت نشد");
                }

                return ServiceResult<BlogPost>.Successful(blogPost);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت مقاله بر اساس Slug - Slug: {Slug}", slug);
                return ServiceResult<BlogPost>.Failed("خطا در دریافت مقاله");
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

        #endregion
    }
}

