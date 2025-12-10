using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت مقالات و پست‌های بلاگ
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IBlogPostService
    {
        Task<ServiceResult<PagedResult<BlogPostIndexViewModel>>> GetBlogPostsAsync(BlogPostSearchViewModel filter);
        Task<ServiceResult<BlogPostDetailsViewModel>> GetBlogPostDetailsAsync(int blogPostId);
        Task<ServiceResult<BlogPostCreateEditViewModel>> GetBlogPostForEditAsync(int blogPostId);
        Task<ServiceResult<BlogPost>> CreateBlogPostAsync(BlogPostCreateEditViewModel model);
        Task<ServiceResult<BlogPost>> UpdateBlogPostAsync(BlogPostCreateEditViewModel model);
        Task<ServiceResult> DeleteBlogPostAsync(int blogPostId);
        Task<ServiceResult> PublishBlogPostAsync(int blogPostId);
        Task<ServiceResult> UnpublishBlogPostAsync(int blogPostId);
        Task<ServiceResult> SetFeaturedAsync(int blogPostId, bool isFeatured);
        Task<ServiceResult<BlogPost>> GetBySlugAsync(string slug);
    }
}

