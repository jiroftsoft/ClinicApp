using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت اطلاعیه‌ها
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IAnnouncementService
    {
        Task<ServiceResult<List<AnnouncementIndexViewModel>>> GetAnnouncementsAsync(bool includeInactive = false);
        Task<ServiceResult<AnnouncementDetailsViewModel>> GetAnnouncementDetailsAsync(int announcementId);
        Task<ServiceResult<AnnouncementCreateEditViewModel>> GetAnnouncementForEditAsync(int announcementId);
        Task<ServiceResult<Announcement>> CreateAnnouncementAsync(AnnouncementCreateEditViewModel model);
        Task<ServiceResult<Announcement>> UpdateAnnouncementAsync(AnnouncementCreateEditViewModel model);
        Task<ServiceResult> DeleteAnnouncementAsync(int announcementId);
        Task<ServiceResult> ActivateAnnouncementAsync(int announcementId);
        Task<ServiceResult> DeactivateAnnouncementAsync(int announcementId);
        Task<ServiceResult> SetImportantAsync(int announcementId, bool isImportant);
        Task<ServiceResult<List<AnnouncementIndexViewModel>>> GetImportantAnnouncementsAsync(int count = 5);
    }
}

