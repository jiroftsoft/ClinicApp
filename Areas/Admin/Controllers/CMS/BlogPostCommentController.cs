using System;
using System.Collections.Generic;
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
    /// کنترلر مدیریت کامنت‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class BlogPostCommentController : BaseCMSController
    {
        private readonly IBlogPostCommentService _commentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public BlogPostCommentController(
            IBlogPostCommentService commentService,
            ICurrentUserService currentUserService)
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<BlogPostCommentController>();
        }

        #region Index & Listing

        /// <summary>
        /// نمایش لیست کامنت‌ها
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(BlogPostCommentSearchViewModel searchModel)
        {
            try
            {
                if (searchModel == null)
                {
                    searchModel = new BlogPostCommentSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 20
                    };
                }

                ServiceResult<PagedResult<BlogPostCommentViewModel>> result;

                if (searchModel.IsSpam == true)
                {
                    result = await _commentService.GetSpamCommentsAsync(searchModel.PageNumber, searchModel.PageSize);
                }
                else if (searchModel.IsReported == true)
                {
                    result = await _commentService.GetReportedCommentsAsync(searchModel.PageNumber, searchModel.PageSize);
                }
                else if (searchModel.IsApproved == false)
                {
                    result = await _commentService.GetPendingCommentsAsync(searchModel.PageNumber, searchModel.PageSize);
                }
                else
                {
                    // کامنت‌های تأیید شده
                    if (searchModel.BlogPostId.HasValue)
                    {
                        result = await _commentService.GetCommentsByBlogPostIdAsync(searchModel.BlogPostId.Value, searchModel.PageNumber, searchModel.PageSize);
                    }
                    else
                    {
                        result = await _commentService.GetPendingCommentsAsync(searchModel.PageNumber, searchModel.PageSize);
                    }
                }

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Index"), new PagedResult<BlogPostCommentViewModel>(new System.Collections.Generic.List<BlogPostCommentViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize));
                }

                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست کامنت‌ها");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست کامنت‌ها");
                return View(GetViewPath("Index"), new PagedResult<BlogPostCommentViewModel>(new System.Collections.Generic.List<BlogPostCommentViewModel>(), 0, 1, 20));
            }
        }

        #endregion

        #region Approve/Reject

        /// <summary>
        /// تأیید کامنت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve(int id)
        {
            try
            {
                var result = await _commentService.ApproveCommentAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "کامنت با موفقیت تأیید شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تأیید کامنت - CommentId: {CommentId}", id);
                NotificationHelper.SetError(TempData, "خطا در تأیید کامنت");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// رد کامنت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reject(int id)
        {
            try
            {
                var result = await _commentService.RejectCommentAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "کامنت رد شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در رد کامنت - CommentId: {CommentId}", id);
                NotificationHelper.SetError(TempData, "خطا در رد کامنت");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Spam Management

        /// <summary>
        /// علامت‌گذاری کامنت به عنوان اسپم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsSpam(int id)
        {
            try
            {
                var result = await _commentService.MarkAsSpamAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "کامنت به عنوان اسپم علامت‌گذاری شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در علامت‌گذاری کامنت به عنوان اسپم - CommentId: {CommentId}", id);
                NotificationHelper.SetError(TempData, "خطا در علامت‌گذاری کامنت به عنوان اسپم");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Report Management

        /// <summary>
        /// علامت‌گذاری کامنت به عنوان گزارش شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsReported(int id)
        {
            try
            {
                var result = await _commentService.MarkAsReportedAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "کامنت گزارش شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در گزارش کامنت - CommentId: {CommentId}", id);
                NotificationHelper.SetError(TempData, "خطا در گزارش کامنت");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// لغو گزارش کامنت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UnmarkAsReported(int id)
        {
            try
            {
                var result = await _commentService.UnmarkAsReportedAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "گزارش کامنت لغو شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو گزارش کامنت - CommentId: {CommentId}", id);
                NotificationHelper.SetError(TempData, "خطا در لغو گزارش کامنت");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// حذف کامنت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _commentService.DeleteCommentAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "کامنت با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف کامنت - CommentId: {CommentId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف کامنت");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Partial Views

        /// <summary>
        /// دریافت کامنت‌های یک مقاله (برای Partial View)
        /// </summary>
        [ChildActionOnly]
        public async Task<ActionResult> GetCommentsByBlogPost(int blogPostId)
        {
            try
            {
                var result = await _commentService.GetCommentsByBlogPostIdAsync(blogPostId, 1, 10);
                if (result.Success)
                {
                    ViewBag.BlogPostId = blogPostId;
                    return PartialView("~/Areas/Admin/Views/CMS/BlogPostComment/_CommentsList.cshtml", result.Data);
                }
                return PartialView("~/Areas/Admin/Views/CMS/BlogPostComment/_CommentsList.cshtml", new PagedResult<BlogPostCommentViewModel>(new System.Collections.Generic.List<BlogPostCommentViewModel>(), 0, 1, 10));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کامنت‌ها - BlogPostId: {BlogPostId}", blogPostId);
                return PartialView("~/Areas/Admin/Views/CMS/BlogPostComment/_CommentsList.cshtml", new PagedResult<BlogPostCommentViewModel>(new System.Collections.Generic.List<BlogPostCommentViewModel>(), 0, 1, 10));
            }
        }

        #endregion
    }
}

