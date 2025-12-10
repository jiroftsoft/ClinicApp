using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Repository interface for BlogPost entity operations
    /// </summary>
    public interface IBlogPostRepository
    {
        Task<BlogPost> GetByIdAsync(int blogPostId);
        Task<List<BlogPost>> GetPublishedPostsAsync(int count = 10);
        Task<List<BlogPost>> GetFeaturedPostsAsync(int count = 3);
        Task<List<BlogPost>> GetAllAsync(bool includeDeleted = false);
        Task<List<BlogPost>> GetByCategoryAsync(string categoryName, int count = 10);
        void Add(BlogPost blogPost);
        void Update(BlogPost blogPost);
        void Delete(BlogPost blogPost);
        Task<bool> ExistsAsync(int blogPostId);
        Task<BlogPost> GetBySlugAsync(string slug);
    }
}

