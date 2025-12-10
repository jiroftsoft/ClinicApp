using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت اسلایدرها
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface ISliderService
    {
        Task<ServiceResult<List<SliderIndexViewModel>>> GetSlidersAsync(string position = null);
        Task<ServiceResult<SliderDetailsViewModel>> GetSliderDetailsAsync(int sliderId);
        Task<ServiceResult<SliderCreateEditViewModel>> GetSliderForEditAsync(int sliderId);
        Task<ServiceResult<Slider>> CreateSliderAsync(SliderCreateEditViewModel model);
        Task<ServiceResult<Slider>> UpdateSliderAsync(SliderCreateEditViewModel model);
        Task<ServiceResult> DeleteSliderAsync(int sliderId);
        Task<ServiceResult> ActivateSliderAsync(int sliderId);
        Task<ServiceResult> DeactivateSliderAsync(int sliderId);
        Task<ServiceResult> UpdateDisplayOrderAsync(int sliderId, int newOrder);
    }
}

