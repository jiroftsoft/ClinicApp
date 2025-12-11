using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Repositories.CMS
{
    /// <summary>
    /// Repository برای مدیریت لایک‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP و Production-Ready
    /// </summary>
    public class BlogPostLikeRepository : IBlogPostLikeRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogPostLikeRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BlogPostLike> GetByIdAsync(int likeId)
        {
            return await _context.BlogPostLikes
                .Include(l => l.BlogPost)
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.BlogPostLikeId == likeId);
        }

        public async Task<BlogPostLike> GetByUserAndBlogPostAsync(int blogPostId, string userId = null, string guestIdentifier = null)
        {
            // بررسی تکراری نبودن: یک کاربر (userId یا guestIdentifier) نمی‌تواند دو بار یک پست را لایک کند
            if (!string.IsNullOrEmpty(userId))
            {
                return await _context.BlogPostLikes
                    .Where(l => l.BlogPostId == blogPostId && l.UserId == userId)
                    .FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(guestIdentifier))
            {
                return await _context.BlogPostLikes
                    .Where(l => l.BlogPostId == blogPostId && l.GuestIdentifier == guestIdentifier)
                    .FirstOrDefaultAsync();
            }

            return null;
        }

        public async Task<int> GetLikeCountAsync(int blogPostId)
        {
            return await _context.BlogPostLikes
                .CountAsync(l => l.BlogPostId == blogPostId);
        }

        public async Task<bool> HasUserLikedAsync(int blogPostId, string userId = null, string guestIdentifier = null)
        {
            var like = await GetByUserAndBlogPostAsync(blogPostId, userId, guestIdentifier);
            return like != null;
        }

        public async Task<BlogPostLike> CreateAsync(BlogPostLike like)
        {
            if (like == null)
                throw new ArgumentNullException(nameof(like));

            // بررسی تکراری نبودن: یک کاربر (userId یا guestIdentifier) نمی‌تواند دو بار یک پست را لایک کند
            var existingLike = await GetByUserAndBlogPostAsync(like.BlogPostId, like.UserId, like.GuestIdentifier);
            if (existingLike != null)
            {
                // اگر قبلاً لایک کرده، همان را برمی‌گرداند (برای جلوگیری از duplicate)
                return existingLike;
            }

            // بررسی اینکه userId و guestIdentifier هر دو null نباشند
            if (string.IsNullOrEmpty(like.UserId) && string.IsNullOrEmpty(like.GuestIdentifier))
            {
                throw new InvalidOperationException("UserId یا GuestIdentifier باید مقدار داشته باشد.");
            }

            like.CreatedAt = DateTime.Now;
            _context.BlogPostLikes.Add(like);
            await _context.SaveChangesAsync();
            return like;
        }

        public async Task<bool> DeleteAsync(int likeId)
        {
            var like = await GetByIdAsync(likeId);
            if (like == null)
                return false;

            _context.BlogPostLikes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByUserAndBlogPostAsync(int blogPostId, string userId = null, string guestIdentifier = null)
        {
            var like = await GetByUserAndBlogPostAsync(blogPostId, userId, guestIdentifier);
            if (like == null)
                return false;

            _context.BlogPostLikes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

