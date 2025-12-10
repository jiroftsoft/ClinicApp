using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت گالری تصاویر
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IGalleryService
    {
        Task<ServiceResult<List<GalleryItemIndexViewModel>>> GetGalleryItemsAsync(string category = null);
        Task<ServiceResult<GalleryItemDetailsViewModel>> GetGalleryItemDetailsAsync(int galleryItemId);
        Task<ServiceResult<GalleryItemCreateEditViewModel>> GetGalleryItemForEditAsync(int galleryItemId);
        Task<ServiceResult<GalleryItem>> CreateGalleryItemAsync(GalleryItemCreateEditViewModel model);
        Task<ServiceResult<GalleryItem>> UpdateGalleryItemAsync(GalleryItemCreateEditViewModel model);
        Task<ServiceResult> DeleteGalleryItemAsync(int galleryItemId);
        Task<ServiceResult> ActivateGalleryItemAsync(int galleryItemId);
        Task<ServiceResult> DeactivateGalleryItemAsync(int galleryItemId);
        Task<ServiceResult> UpdateDisplayOrderAsync(int galleryItemId, int newOrder);
        Task<ServiceResult<List<string>>> GetCategoriesAsync();
    }
}

