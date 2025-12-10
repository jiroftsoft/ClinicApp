using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت تماس‌های اضطراری (Emergency Contact)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IEmergencyContactService
    {
        Task<ServiceResult<PagedResult<EmergencyContactIndexViewModel>>> GetEmergencyContactsAsync(EmergencyContactSearchViewModel filter);
        Task<ServiceResult<EmergencyContactDetailsViewModel>> GetEmergencyContactDetailsAsync(int emergencyContactId);
        Task<ServiceResult<EmergencyContactCreateEditViewModel>> GetEmergencyContactForEditAsync(int emergencyContactId);
        Task<ServiceResult<EmergencyContact>> CreateEmergencyContactAsync(EmergencyContactCreateEditViewModel model);
        Task<ServiceResult<EmergencyContact>> UpdateEmergencyContactAsync(EmergencyContactCreateEditViewModel model);
        Task<ServiceResult> DeleteEmergencyContactAsync(int emergencyContactId);
        Task<ServiceResult> ActivateEmergencyContactAsync(int emergencyContactId);
        Task<ServiceResult> DeactivateEmergencyContactAsync(int emergencyContactId);
        Task<ServiceResult> SetAlwaysVisibleAsync(int emergencyContactId, bool isAlwaysVisible);
        Task<ServiceResult<List<EmergencyContactPublicViewModel>>> GetActiveContactsAsync();
        Task<ServiceResult<List<EmergencyContactPublicViewModel>>> GetAlwaysVisibleContactsAsync();
        Task<ServiceResult<List<EmergencyContactPublicViewModel>>> GetByContactTypeAsync(string contactType);
        Task<ServiceResult<List<EmergencyContactPublicViewModel>>> SearchContactsAsync(string searchTerm);
        Task<ServiceResult<EmergencyContact>> GetBySlugAsync(string slug);
    }
}

