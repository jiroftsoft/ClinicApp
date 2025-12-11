using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت کامنت‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP و Production-Ready
    /// </summary>
    public class BlogPostCommentService : IBlogPostCommentService
    {
        private readonly IBlogPostCommentRepository _commentRepository;
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public BlogPostCommentService(
            IBlogPostCommentRepository commentRepository,
            IBlogPostRepository blogPostRepository,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
            _blogPostRepository = blogPostRepository ?? throw new ArgumentNullException(nameof(blogPostRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<BlogPostCommentViewModel>> CreateCommentAsync(BlogPostCommentCreateViewModel model)
        {
            try
            {
                // Validation
                if (model == null)
                    return ServiceResult<BlogPostCommentViewModel>.Failed("مدل کامنت نمی‌تواند خالی باشد.");

                if (string.IsNullOrWhiteSpace(model.CommentText))
                    return ServiceResult<BlogPostCommentViewModel>.Failed("متن کامنت الزامی است.");

                // بررسی وجود مقاله
                var blogPost = await _blogPostRepository.GetByIdAsync(model.BlogPostId);
                if (blogPost == null)
                    return ServiceResult<BlogPostCommentViewModel>.Failed("مقاله یافت نشد.");

                // ایجاد کامنت
                var comment = new BlogPostComment
                {
                    BlogPostId = model.BlogPostId,
                    CommentText = model.CommentText,
                    AuthorName = model.AuthorName,
                    AuthorEmail = model.AuthorEmail,
                    AuthorPhone = model.AuthorPhone,
                    AuthorUserId = model.AuthorUserId ?? _currentUserService.UserId,
                    ParentCommentId = model.ParentCommentId,
                    IpAddress = model.IpAddress,
                    UserAgent = model.UserAgent,
                    IsApproved = false, // نیاز به تأیید ادمین
                    CreatedByUserId = _currentUserService.UserId
                };

                var createdComment = await _commentRepository.CreateAsync(comment);

                _logger.Information("کامنت جدید ایجاد شد - CommentId: {CommentId}, BlogPostId: {BlogPostId}",
                    createdComment.BlogPostCommentId, model.BlogPostId);

                var viewModel = MapToViewModel(createdComment);
                return ServiceResult<BlogPostCommentViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد کامنت - BlogPostId: {BlogPostId}", model?.BlogPostId);
                return ServiceResult<BlogPostCommentViewModel>.Failed("خطا در ایجاد کامنت.");
            }
        }

        public async Task<ServiceResult<BlogPostCommentViewModel>> GetCommentByIdAsync(int commentId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (comment == null)
                    return ServiceResult<BlogPostCommentViewModel>.Failed("کامنت یافت نشد.");

                var viewModel = MapToViewModel(comment);
                return ServiceResult<BlogPostCommentViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کامنت - CommentId: {CommentId}", commentId);
                return ServiceResult<BlogPostCommentViewModel>.Failed("خطا در دریافت کامنت.");
            }
        }

        public async Task<ServiceResult<PagedResult<BlogPostCommentViewModel>>> GetCommentsByBlogPostIdAsync(int blogPostId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var comments = await _commentRepository.GetApprovedCommentsAsync(blogPostId);
                var totalCount = comments.Count;

                var pagedComments = comments
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(MapToViewModel)
                    .ToList();

                var result = new PagedResult<BlogPostCommentViewModel>(pagedComments, totalCount, pageNumber, pageSize);
                return ServiceResult<PagedResult<BlogPostCommentViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کامنت‌ها - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult<PagedResult<BlogPostCommentViewModel>>.Failed("خطا در دریافت کامنت‌ها.");
            }
        }

        public async Task<ServiceResult<bool>> ApproveCommentAsync(int commentId)
        {
            try
            {
                var result = await _commentRepository.ApproveAsync(commentId);
                if (result)
                {
                    _logger.Information("کامنت تأیید شد - CommentId: {CommentId}", commentId);
                }
                return ServiceResult<bool>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تأیید کامنت - CommentId: {CommentId}", commentId);
                return ServiceResult<bool>.Failed("خطا در تأیید کامنت.");
            }
        }

        public async Task<ServiceResult<bool>> RejectCommentAsync(int commentId)
        {
            try
            {
                var result = await _commentRepository.RejectAsync(commentId);
                if (result)
                {
                    _logger.Information("کامنت رد شد - CommentId: {CommentId}", commentId);
                }
                return ServiceResult<bool>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در رد کامنت - CommentId: {CommentId}", commentId);
                return ServiceResult<bool>.Failed("خطا در رد کامنت.");
            }
        }

        public async Task<ServiceResult<bool>> DeleteCommentAsync(int commentId)
        {
            try
            {
                var result = await _commentRepository.DeleteAsync(commentId);
                if (result)
                {
                    _logger.Information("کامنت حذف شد - CommentId: {CommentId}", commentId);
                }
                return ServiceResult<bool>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف کامنت - CommentId: {CommentId}", commentId);
                return ServiceResult<bool>.Failed("خطا در حذف کامنت.");
            }
        }

        public async Task<ServiceResult<bool>> MarkAsSpamAsync(int commentId)
        {
            try
            {
                var result = await _commentRepository.MarkAsSpamAsync(commentId);
                if (result)
                {
                    _logger.Information("کامنت به عنوان اسپم علامت‌گذاری شد - CommentId: {CommentId}", commentId);
                }
                return ServiceResult<bool>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در علامت‌گذاری کامنت به عنوان اسپم - CommentId: {CommentId}", commentId);
                return ServiceResult<bool>.Failed("خطا در علامت‌گذاری کامنت به عنوان اسپم.");
            }
        }

        public async Task<ServiceResult<bool>> MarkAsReportedAsync(int commentId)
        {
            try
            {
                var result = await _commentRepository.MarkAsReportedAsync(commentId);
                if (result)
                {
                    _logger.Information("کامنت گزارش شد - CommentId: {CommentId}", commentId);
                }
                return ServiceResult<bool>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در گزارش کامنت - CommentId: {CommentId}", commentId);
                return ServiceResult<bool>.Failed("خطا در گزارش کامنت.");
            }
        }

        public async Task<ServiceResult<bool>> UnmarkAsReportedAsync(int commentId)
        {
            try
            {
                var result = await _commentRepository.UnmarkAsReportedAsync(commentId);
                if (result)
                {
                    _logger.Information("گزارش کامنت لغو شد - CommentId: {CommentId}", commentId);
                }
                return ServiceResult<bool>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو گزارش کامنت - CommentId: {CommentId}", commentId);
                return ServiceResult<bool>.Failed("خطا در لغو گزارش کامنت.");
            }
        }

        public async Task<ServiceResult<PagedResult<BlogPostCommentViewModel>>> GetPendingCommentsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var comments = await _commentRepository.GetPendingCommentsAsync();
                var totalCount = comments.Count;

                var pagedComments = comments
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(MapToViewModel)
                    .ToList();

                var result = new PagedResult<BlogPostCommentViewModel>(pagedComments, totalCount, pageNumber, pageSize);
                return ServiceResult<PagedResult<BlogPostCommentViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کامنت‌های در انتظار تأیید");
                return ServiceResult<PagedResult<BlogPostCommentViewModel>>.Failed("خطا در دریافت کامنت‌های در انتظار تأیید.");
            }
        }

        public async Task<ServiceResult<PagedResult<BlogPostCommentViewModel>>> GetSpamCommentsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var comments = await _commentRepository.GetSpamCommentsAsync();
                var totalCount = comments.Count;

                var pagedComments = comments
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(MapToViewModel)
                    .ToList();

                var result = new PagedResult<BlogPostCommentViewModel>(pagedComments, totalCount, pageNumber, pageSize);
                return ServiceResult<PagedResult<BlogPostCommentViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کامنت‌های اسپم");
                return ServiceResult<PagedResult<BlogPostCommentViewModel>>.Failed("خطا در دریافت کامنت‌های اسپم.");
            }
        }

        public async Task<ServiceResult<PagedResult<BlogPostCommentViewModel>>> GetReportedCommentsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var comments = await _commentRepository.GetReportedCommentsAsync();
                var totalCount = comments.Count;

                var pagedComments = comments
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(MapToViewModel)
                    .ToList();

                var result = new PagedResult<BlogPostCommentViewModel>(pagedComments, totalCount, pageNumber, pageSize);
                return ServiceResult<PagedResult<BlogPostCommentViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کامنت‌های گزارش شده");
                return ServiceResult<PagedResult<BlogPostCommentViewModel>>.Failed("خطا در دریافت کامنت‌های گزارش شده.");
            }
        }

        public async Task<ServiceResult<int>> GetCommentCountAsync(int blogPostId, bool approvedOnly = true)
        {
            try
            {
                var count = await _commentRepository.GetCommentCountAsync(blogPostId, approvedOnly);
                return ServiceResult<int>.Successful(count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد کامنت‌ها - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult<int>.Failed("خطا در دریافت تعداد کامنت‌ها.");
            }
        }

        #region Private Helper Methods

        private BlogPostCommentViewModel MapToViewModel(BlogPostComment comment)
        {
            if (comment == null)
                return null;

            return new BlogPostCommentViewModel
            {
                BlogPostCommentId = comment.BlogPostCommentId,
                BlogPostId = comment.BlogPostId,
                CommentText = comment.CommentText,
                AuthorName = comment.AuthorName ?? comment.AuthorUser?.UserName ?? "ناشناس",
                AuthorEmail = comment.AuthorEmail,
                AuthorPhone = comment.AuthorPhone,
                AuthorUserId = comment.AuthorUserId,
                AuthorUserName = comment.AuthorUser?.UserName,
                IsApproved = comment.IsApproved,
                IsSpam = comment.IsSpam,
                IsReported = comment.IsReported,
                ParentCommentId = comment.ParentCommentId,
                ParentCommentAuthorName = comment.ParentComment?.AuthorName,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                ReplyCount = comment.Replies?.Count(r => r.IsApproved && !r.IsDeleted && !r.IsSpam) ?? 0,
                Replies = comment.Replies?.Where(r => r.IsApproved && !r.IsDeleted && !r.IsSpam)
                    .OrderBy(r => r.CreatedAt)
                    .Select(MapToViewModel)
                    .ToList() ?? new List<BlogPostCommentViewModel>()
            };
        }

        #endregion
    }
}

