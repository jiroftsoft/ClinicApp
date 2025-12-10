using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Repository interface for GalleryItem entity operations
    /// </summary>
    public interface IGalleryItemRepository
    {
        Task<GalleryItem> GetByIdAsync(int galleryItemId);
        Task<List<GalleryItem>> GetActiveItemsAsync(int count = 10, string category = null);
        Task<List<GalleryItem>> GetAllAsync(bool includeDeleted = false);
        Task<List<GalleryItem>> GetByCategoryAsync(string category, int count = 10);
        void Add(GalleryItem galleryItem);
        void Update(GalleryItem galleryItem);
        void Delete(GalleryItem galleryItem);
        Task<bool> ExistsAsync(int galleryItemId);
    }
}

