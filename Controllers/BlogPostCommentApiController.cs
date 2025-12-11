using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// API Controller برای کامنت و لایک مقالات بلاگ (Public)
    /// طراحی شده بر اساس اصول SRP و Production-Ready
    /// </summary>
    public class BlogPostCommentApiController : Controller
    {
        private readonly IBlogPostCommentService _commentService;
        private readonly IBlogPostLikeService _likeService;
        private readonly ICurrentUserService _currentUserService;

        public BlogPostCommentApiController(
            IBlogPostCommentService commentService,
            IBlogPostLikeService likeService,
            ICurrentUserService currentUserService)
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            _likeService = likeService ?? throw new ArgumentNullException(nameof(likeService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Comment API

        /// <summary>
        /// ایجاد کامنت جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateComment(BlogPostCommentCreateViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "داده‌های ارسالی نامعتبر است." });
                }

                // تنظیم اطلاعات امنیتی
                model.IpAddress = Request.UserHostAddress;
                model.UserAgent = Request.UserAgent;
                model.AuthorUserId = _currentUserService.UserId;

                var result = await _commentService.CreateCommentAsync(model);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, message = "کامنت شما با موفقیت ثبت شد و پس از تأیید نمایش داده می‌شود.", data = result.Data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در ثبت کامنت." });
            }
        }

        /// <summary>
        /// دریافت کامنت‌های یک مقاله
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetComments(int blogPostId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var result = await _commentService.GetCommentsByBlogPostIdAsync(blogPostId, pageNumber, pageSize);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در دریافت کامنت‌ها." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// گزارش کامنت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ReportComment(int commentId)
        {
            try
            {
                var result = await _commentService.MarkAsReportedAsync(commentId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, message = "کامنت گزارش شد. از همکاری شما متشکریم." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در گزارش کامنت." });
            }
        }

        #endregion

        #region Like API

        /// <summary>
        /// Toggle Like (لایک/آنلایک)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ToggleLike(int blogPostId, string guestIdentifier = null)
        {
            try
            {
                var result = await _likeService.ToggleLikeAsync(
                    blogPostId,
                    userId: _currentUserService.UserId,
                    guestIdentifier: guestIdentifier,
                    ipAddress: Request.UserHostAddress,
                    userAgent: Request.UserAgent);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                // دریافت تعداد لایک‌ها
                var likeCountResult = await _likeService.GetLikeCountAsync(blogPostId);
                var likeCount = likeCountResult.Success ? likeCountResult.Data : 0;

                return Json(new
                {
                    success = true,
                    isLiked = result.Data,
                    likeCount = likeCount,
                    message = result.Data ? "لایک شد" : "لایک حذف شد"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در لایک/آنلایک." });
            }
        }

        /// <summary>
        /// دریافت تعداد لایک‌ها
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetLikeCount(int blogPostId)
        {
            try
            {
                var result = await _likeService.GetLikeCountAsync(blogPostId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, likeCount = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در دریافت تعداد لایک‌ها." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی اینکه آیا کاربر لایک کرده است
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> HasLiked(int blogPostId, string guestIdentifier = null)
        {
            try
            {
                var result = await _likeService.HasUserLikedAsync(
                    blogPostId,
                    userId: _currentUserService.UserId,
                    guestIdentifier: guestIdentifier);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, hasLiked = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در بررسی لایک." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}

