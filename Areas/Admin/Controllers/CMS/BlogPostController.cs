using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;
using System.Collections.Generic;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت مقالات و پست‌های بلاگ
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class BlogPostController : BaseCMSController
    {
        private readonly IBlogPostService _blogPostService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImageUploadService _imageUploadService;
        private readonly IBlogPostCommentService _commentService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string BlogImageUploadPath = "~/Content/Images/blog";
        private const string BlogThumbnailUploadPath = "~/Content/Images/blog/thumbnails";
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private const int MaxImageWidth = 1920; // Full HD
        private const int MaxImageHeight = 1080; // Full HD

        public BlogPostController(
            IBlogPostService blogPostService,
            ICurrentUserService currentUserService,
            IImageUploadService imageUploadService,
            IBlogPostCommentService commentService)
        {
            _blogPostService = blogPostService ?? throw new ArgumentNullException(nameof(blogPostService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            _logger = Log.ForContext<BlogPostController>();
        }

        #region Index & Listing

        /// <summary>
        /// نمایش لیست مقالات با قابلیت جستجو و فیلتر
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(BlogPostSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست مقالات توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new BlogPostSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _blogPostService.GetBlogPostsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست مقالات: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Index"), new PagedResult<BlogPostIndexViewModel>(new List<BlogPostIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize));
                }

                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست مقالات");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست مقالات");
                return View(GetViewPath("Index"), new PagedResult<BlogPostIndexViewModel>(new List<BlogPostIndexViewModel>(), 0, 1, 10));
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات مقاله
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _blogPostService.GetBlogPostDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                // Load comments for this blog post
                var commentsResult = await _commentService.GetCommentsByBlogPostIdAsync(id, 1, 10);
                var comments = commentsResult.Success 
                    ? commentsResult.Data 
                    : new PagedResult<BlogPostCommentViewModel>(new System.Collections.Generic.List<BlogPostCommentViewModel>(), 0, 1, 10);

                // Create strongly-typed ViewModel
                var viewModel = new BlogPostDetailsWithCommentsViewModel
                {
                    BlogPost = result.Data,
                    Comments = comments
                };

                // Set ViewBag for Partial View compatibility
                ViewBag.BlogPostId = id;

                return View(GetViewPath("Details"), viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات مقاله - BlogPostId: {BlogPostId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات مقاله");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// نمایش فرم ایجاد مقاله جدید
        /// </summary>
        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                var model = new BlogPostCreateEditViewModel
                {
                    IsPublished = false,
                    IsFeatured = false,
                    DisplayOrder = 0
                };

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد مقاله");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد مقاله");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ایجاد مقاله جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in Content field
        public async Task<ActionResult> Create(BlogPostCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد مقاله جدید توسط کاربر {UserId}", _currentUserService.UserId);

                // Parse تاریخ انتشار از hidden input
                model.PublishedAt = this.ParseDateFromHiddenInput("PublishedAt", _logger);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Create"), model);
                }

                var result = await _blogPostService.CreateBlogPostAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد مقاله: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }

                _logger.Information("مقاله با موفقیت ایجاد شد - BlogPostId: {BlogPostId}", result.Data.BlogPostId);
                NotificationHelper.SetSuccess(TempData, "مقاله با موفقیت ایجاد شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد مقاله");
                NotificationHelper.SetError(TempData, "خطا در ایجاد مقاله");
                return View(GetViewPath("Create"), model);
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش مقاله
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _blogPostService.GetBlogPostForEditAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش مقاله - BlogPostId: {BlogPostId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش مقاله");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// به‌روزرسانی مقاله
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in Content field
        public async Task<ActionResult> Edit(BlogPostCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی مقاله - BlogPostId: {BlogPostId}", model.BlogPostId);

                // Parse تاریخ انتشار از hidden input
                model.PublishedAt = this.ParseDateFromHiddenInput("PublishedAt", _logger);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _blogPostService.UpdateBlogPostAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی مقاله: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }

                _logger.Information("مقاله با موفقیت به‌روزرسانی شد - BlogPostId: {BlogPostId}", model.BlogPostId);
                NotificationHelper.SetSuccess(TempData, "مقاله با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی مقاله - BlogPostId: {BlogPostId}", model.BlogPostId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی مقاله");
                return View(GetViewPath("Edit"), model);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// حذف مقاله
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.Information("درخواست حذف مقاله - BlogPostId: {BlogPostId}", id);

                var result = await _blogPostService.DeleteBlogPostAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "مقاله با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف مقاله - BlogPostId: {BlogPostId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف مقاله");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Publish/Unpublish

        /// <summary>
        /// انتشار مقاله
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Publish(int id)
        {
            try
            {
                var result = await _blogPostService.PublishBlogPostAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "مقاله با موفقیت منتشر شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتشار مقاله - BlogPostId: {BlogPostId}", id);
                NotificationHelper.SetError(TempData, "خطا در انتشار مقاله");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// لغو انتشار مقاله
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Unpublish(int id)
        {
            try
            {
                var result = await _blogPostService.UnpublishBlogPostAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "مقاله از حالت انتشار خارج شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو انتشار مقاله - BlogPostId: {BlogPostId}", id);
                NotificationHelper.SetError(TempData, "خطا در لغو انتشار مقاله");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Featured

        /// <summary>
        /// تنظیم مقاله به عنوان ویژه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetFeatured(int id, bool isFeatured)
        {
            try
            {
                var result = await _blogPostService.SetFeaturedAsync(id, isFeatured);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, isFeatured ? "مقاله به عنوان ویژه تنظیم شد" : "مقاله از حالت ویژه خارج شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه مقاله - BlogPostId: {BlogPostId}", id);
                NotificationHelper.SetError(TempData, "خطا در تنظیم وضعیت ویژه مقاله");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Image Upload

        /// <summary>
        /// پردازش آپلود تصویر مقاله
        /// </summary>
        private async Task ProcessImageUpload(BlogPostCreateEditViewModel model)
        {
            try
            {
                var imageFile = Request.Files["ImageFile"];
                var thumbnailFile = Request.Files["ThumbnailFile"];

                // اگر تصویر اصلی آپلود شده
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        imageFile,
                        BlogImageUploadPath,
                        BlogThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);

                    if (!uploadResult.Success)
                    {
                        _logger.Warning("خطا در آپلود تصویر: {ErrorMessage}", uploadResult.Message);
                        NotificationHelper.SetError(TempData, uploadResult.Message);
                        ModelState.AddModelError("ImageFile", uploadResult.Message);
                        return;
                    }

                    // تنظیم مسیر تصویر اصلی
                    model.ImageUrl = uploadResult.Data.ImageUrl;

                    // اگر thumbnail جداگانه آپلود نشده، از thumbnail خودکار استفاده کن
                    if (thumbnailFile == null || thumbnailFile.ContentLength == 0)
                    {
                        model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
                    }

                    _logger.Information("تصویر با موفقیت آپلود شد: {ImageUrl}, Thumbnail: {ThumbnailUrl}",
                        model.ImageUrl, model.ThumbnailUrl);
                }

                // اگر thumbnail جداگانه آپلود شده
                if (thumbnailFile != null && thumbnailFile.ContentLength > 0)
                {
                    var thumbnailResult = _imageUploadService.UploadImageWithThumbnail(
                        thumbnailFile,
                        BlogThumbnailUploadPath,
                        BlogThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        ThumbnailWidth,
                        ThumbnailHeight);

                    if (!thumbnailResult.Success)
                    {
                        _logger.Warning("خطا در آپلود thumbnail: {ErrorMessage}", thumbnailResult.Message);
                        NotificationHelper.SetError(TempData, thumbnailResult.Message);
                        ModelState.AddModelError("ThumbnailFile", thumbnailResult.Message);
                        return;
                    }

                    model.ThumbnailUrl = thumbnailResult.Data.ImageUrl;
                    _logger.Information("Thumbnail جداگانه با موفقیت آپلود شد: {ThumbnailUrl}", model.ThumbnailUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش آپلود تصویر");
                NotificationHelper.SetError(TempData, "خطا در آپلود تصویر");
                ModelState.AddModelError("", "خطا در آپلود تصویر");
            }
        }

        #endregion
    }
}

