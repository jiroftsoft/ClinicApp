using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت ویدیو
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IVideoService
    {
        Task<ServiceResult<List<VideoIndexViewModel>>> GetVideosAsync(VideoSearchViewModel search = null);
        Task<ServiceResult<VideoDetailsViewModel>> GetVideoDetailsAsync(int videoId);
        Task<ServiceResult<VideoCreateEditViewModel>> GetVideoForEditAsync(int videoId);
        Task<ServiceResult<Video>> CreateVideoAsync(VideoCreateEditViewModel model);
        Task<ServiceResult<Video>> UpdateVideoAsync(VideoCreateEditViewModel model);
        Task<ServiceResult> DeleteVideoAsync(int videoId);
        Task<ServiceResult> ActivateVideoAsync(int videoId);
        Task<ServiceResult> DeactivateVideoAsync(int videoId);
        Task<ServiceResult> UpdateDisplayOrderAsync(int videoId, int newOrder);
        Task<ServiceResult<List<string>>> GetCategoriesAsync();
        Task<ServiceResult<List<VideoHomePageViewModel>>> GetVideosForHomePageAsync(int count = 6, string category = null);
        Task<ServiceResult> IncrementViewCountAsync(int videoId);
    }
}

