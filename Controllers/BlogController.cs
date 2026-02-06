using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر عمومی Blog برای نمایش مقالات در سایت
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class BlogController : Controller
    {
        private readonly IBlogPostService _blogPostService;
        private readonly ILogger _logger;

        public BlogController(IBlogPostService blogPostService)
        {
            _blogPostService = blogPostService ?? throw new ArgumentNullException(nameof(blogPostService));
            _logger = Log.ForContext<BlogController>();
        }

        /// <summary>
        /// صفحه اصلی Blog - لیست مقالات منتشرشده
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "category;page")]
        public async Task<ActionResult> Index(string category = null, int page = 1)
        {
            try
            {
                var searchModel = new BlogPostSearchViewModel
                {
                    PageNumber = page,
                    PageSize = 12,
                    CategoryName = category,
                    IsPublished = true
                };

                var result = await _blogPostService.GetBlogPostsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست مقالات: {ErrorMessage}", result.Message);
                    return View(new BlogIndexPageViewModel
                    {
                        Posts = new PagedResult<BlogPostIndexViewModel>(new System.Collections.Generic.List<BlogPostIndexViewModel>(), 0, page, 12),
                        Category = category
                    });
                }

                return View(new BlogIndexPageViewModel { Posts = result.Data, Category = category });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه مقالات");
                return View(new BlogIndexPageViewModel
                {
                    Posts = new PagedResult<BlogPostIndexViewModel>(new System.Collections.Generic.List<BlogPostIndexViewModel>(), 0, 1, 12),
                    Category = category
                });
            }
        }

        /// <summary>
        /// نمایش جزئیات مقاله بر اساس Slug
        /// Route: /Blog/Post/{slug}
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "slug")]
        public async Task<ActionResult> Post(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return HttpNotFound();
                }

                // Decode URL (در صورت نیاز)
                slug = System.Web.HttpUtility.UrlDecode(slug);

                // حذف کاراکترهای غیرمجاز از Slug (برای امنیت)
                slug = CleanSlug(slug);

                var result = await _blogPostService.GetBySlugAsync(slug);

                if (!result.Success || result.Data == null)
                {
                    _logger.Warning("مقاله یافت نشد - Slug: {Slug}", slug);
                    return HttpNotFound();
                }

                var blogPost = result.Data;

                // بررسی Published بودن
                if (!blogPost.IsPublished || (blogPost.PublishedAt.HasValue && blogPost.PublishedAt.Value > DateTime.Now))
                {
                    _logger.Warning("مقاله منتشر نشده است - Slug: {Slug}", slug);
                    return HttpNotFound();
                }

                // تبدیل به ViewModel
                var viewModel = new BlogPostDetailsViewModel
                {
                    BlogPostId = blogPost.BlogPostId,
                    Title = blogPost.Title,
                    Summary = blogPost.Summary,
                    Content = blogPost.Content,
                    ImageUrl = blogPost.ImageUrl ?? blogPost.ThumbnailUrl,
                    ThumbnailUrl = blogPost.ThumbnailUrl,
                    AuthorName = blogPost.AuthorName,
                    CategoryName = blogPost.CategoryName,
                    PublishedAt = blogPost.PublishedAt ?? blogPost.CreatedAt,
                    MetaTitle = blogPost.MetaTitle,
                    MetaDescription = blogPost.MetaDescription,
                    Slug = blogPost.Slug
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات مقاله - Slug: {Slug}", slug);
                return HttpNotFound();
            }
        }

        #region Helper Methods

        /// <summary>
        /// پاکسازی Slug از کاراکترهای غیرمجاز
        /// </summary>
        private string CleanSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return string.Empty;

            // حذف کاراکترهای خاص
            var invalidChars = new[] { '؟', '?', '!', '،', ',', '؛', ';', ':', '(', ')', '[', ']', '{', '}', '<', '>', '/', '\\', '|', '*', '"', '\'', '`', '~', '@', '#', '$', '%', '^', '&', '+', '=' };
            
            foreach (var c in invalidChars)
            {
                slug = slug.Replace(c.ToString(), "");
            }

            // حذف کاراکترهای غیرمجاز Path
            var pathInvalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (var c in pathInvalidChars)
            {
                slug = slug.Replace(c.ToString(), "");
            }

            // حذف فاصله‌های اضافی و خط تیره‌های تکراری
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }

        #endregion
    }
}
