using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت مقالات و پست‌های بلاگ
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class BlogPostController : Controller
    {
        private readonly IBlogPostService _blogPostService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public BlogPostController(
            IBlogPostService blogPostService,
            ICurrentUserService currentUserService)
        {
            _blogPostService = blogPostService ?? throw new ArgumentNullException(nameof(blogPostService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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
                    TempData["Error"] = result.Message;
                    return View(new PagedResult<BlogPostIndexViewModel>(new System.Collections.Generic.List<BlogPostIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize));
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست مقالات");
                TempData["Error"] = "خطا در بارگذاری لیست مقالات";
                return View(new PagedResult<BlogPostIndexViewModel>(new System.Collections.Generic.List<BlogPostIndexViewModel>(), 0, 1, 10));
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
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات مقاله - BlogPostId: {BlogPostId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات مقاله";
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

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد مقاله");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد مقاله";
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

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _blogPostService.CreateBlogPostAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد مقاله: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("مقاله با موفقیت ایجاد شد - BlogPostId: {BlogPostId}", result.Data.BlogPostId);
                TempData["Success"] = "مقاله با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد مقاله");
                TempData["Error"] = "خطا در ایجاد مقاله";
                return View(model);
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
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش مقاله - BlogPostId: {BlogPostId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش مقاله";
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

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _blogPostService.UpdateBlogPostAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی مقاله: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("مقاله با موفقیت به‌روزرسانی شد - BlogPostId: {BlogPostId}", model.BlogPostId);
                TempData["Success"] = "مقاله با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی مقاله - BlogPostId: {BlogPostId}", model.BlogPostId);
                TempData["Error"] = "خطا در به‌روزرسانی مقاله";
                return View(model);
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
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "مقاله با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف مقاله - BlogPostId: {BlogPostId}", id);
                TempData["Error"] = "خطا در حذف مقاله";
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
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "مقاله با موفقیت منتشر شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتشار مقاله - BlogPostId: {BlogPostId}", id);
                TempData["Error"] = "خطا در انتشار مقاله";
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
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "مقاله از حالت انتشار خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو انتشار مقاله - BlogPostId: {BlogPostId}", id);
                TempData["Error"] = "خطا در لغو انتشار مقاله";
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
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = isFeatured ? "مقاله به عنوان ویژه تنظیم شد" : "مقاله از حالت ویژه خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه مقاله - BlogPostId: {BlogPostId}", id);
                TempData["Error"] = "خطا در تنظیم وضعیت ویژه مقاله";
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}

