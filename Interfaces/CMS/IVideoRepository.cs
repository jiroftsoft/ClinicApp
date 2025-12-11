using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Repository interface for Video entity operations
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface IVideoRepository
    {
        Task<Video> GetByIdAsync(int videoId);
        Task<List<Video>> GetActiveVideosAsync(int count = 10, string category = null);
        Task<List<Video>> GetAllAsync(bool includeDeleted = false);
        Task<List<Video>> GetByCategoryAsync(string category, int count = 10);
        Task<List<Video>> GetVideosForHomePageAsync(int count = 6, string category = null);
        Task<List<string>> GetCategoriesAsync();
        void Add(Video video);
        void Update(Video video);
        void Delete(Video video);
        Task<bool> ExistsAsync(int videoId);
    }
}

