using System;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Repository لایک‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface IBlogPostLikeRepository
    {
        Task<BlogPostLike> GetByIdAsync(int likeId);
        Task<BlogPostLike> GetByUserAndBlogPostAsync(int blogPostId, string userId = null, string guestIdentifier = null);
        Task<int> GetLikeCountAsync(int blogPostId);
        Task<bool> HasUserLikedAsync(int blogPostId, string userId = null, string guestIdentifier = null);
        Task<BlogPostLike> CreateAsync(BlogPostLike like);
        Task<bool> DeleteAsync(int likeId);
        Task<bool> DeleteByUserAndBlogPostAsync(int blogPostId, string userId = null, string guestIdentifier = null);
    }
}

