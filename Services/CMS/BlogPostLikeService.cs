using System;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Entities.CMS;
using Serilog;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت لایک‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP و Production-Ready
    /// </summary>
    public class BlogPostLikeService : IBlogPostLikeService
    {
        private readonly IBlogPostLikeRepository _likeRepository;
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public BlogPostLikeService(
            IBlogPostLikeRepository likeRepository,
            IBlogPostRepository blogPostRepository,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _likeRepository = likeRepository ?? throw new ArgumentNullException(nameof(likeRepository));
            _blogPostRepository = blogPostRepository ?? throw new ArgumentNullException(nameof(blogPostRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<bool>> ToggleLikeAsync(int blogPostId, string userId = null, string guestIdentifier = null, string ipAddress = null, string userAgent = null)
        {
            try
            {
                // بررسی وجود مقاله
                var blogPost = await _blogPostRepository.GetByIdAsync(blogPostId);
                if (blogPost == null)
                    return ServiceResult<bool>.Failed("مقاله یافت نشد.");

                // اگر userId وجود دارد، از آن استفاده کن، در غیر این صورت از guestIdentifier
                userId = userId ?? _currentUserService.UserId;

                // بررسی اینکه آیا قبلاً لایک کرده است
                var existingLike = await _likeRepository.GetByUserAndBlogPostAsync(blogPostId, userId, guestIdentifier);

                if (existingLike != null)
                {
                    // اگر قبلاً لایک کرده، آن را حذف کن (Unlike)
                    var deleted = await _likeRepository.DeleteAsync(existingLike.BlogPostLikeId);
                    if (deleted)
                    {
                        _logger.Information("لایک حذف شد - BlogPostId: {BlogPostId}, UserId: {UserId}", blogPostId, userId ?? guestIdentifier);
                    }
                    return ServiceResult<bool>.Successful(false); // false = unlike
                }
                else
                {
                    // ایجاد لایک جدید
                    var like = new BlogPostLike
                    {
                        BlogPostId = blogPostId,
                        UserId = userId,
                        GuestIdentifier = guestIdentifier,
                        IpAddress = ipAddress,
                        UserAgent = userAgent,
                        CreatedByUserId = userId
                    };

                    await _likeRepository.CreateAsync(like);
                    _logger.Information("لایک ایجاد شد - BlogPostId: {BlogPostId}, UserId: {UserId}", blogPostId, userId ?? guestIdentifier);
                    return ServiceResult<bool>.Successful(true); // true = like
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در toggle لایک - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult<bool>.Failed("خطا در لایک/آنلایک.");
            }
        }

        public async Task<ServiceResult<int>> GetLikeCountAsync(int blogPostId)
        {
            try
            {
                var count = await _likeRepository.GetLikeCountAsync(blogPostId);
                return ServiceResult<int>.Successful(count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد لایک‌ها - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult<int>.Failed("خطا در دریافت تعداد لایک‌ها.");
            }
        }

        public async Task<ServiceResult<bool>> HasUserLikedAsync(int blogPostId, string userId = null, string guestIdentifier = null)
        {
            try
            {
                userId = userId ?? _currentUserService.UserId;
                var hasLiked = await _likeRepository.HasUserLikedAsync(blogPostId, userId, guestIdentifier);
                return ServiceResult<bool>.Successful(hasLiked);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی لایک کاربر - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult<bool>.Failed("خطا در بررسی لایک کاربر.");
            }
        }

        public async Task<ServiceResult<bool>> UnlikeAsync(int blogPostId, string userId = null, string guestIdentifier = null)
        {
            try
            {
                userId = userId ?? _currentUserService.UserId;
                var deleted = await _likeRepository.DeleteByUserAndBlogPostAsync(blogPostId, userId, guestIdentifier);
                if (deleted)
                {
                    _logger.Information("لایک حذف شد - BlogPostId: {BlogPostId}, UserId: {UserId}", blogPostId, userId ?? guestIdentifier);
                }
                return ServiceResult<bool>.Successful(deleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف لایک - BlogPostId: {BlogPostId}", blogPostId);
                return ServiceResult<bool>.Failed("خطا در حذف لایک.");
            }
        }
    }
}

