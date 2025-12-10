using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت ساعات کاری کلینیک (Clinic Working Hours)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IClinicWorkingHoursService
    {
        Task<ServiceResult<PagedResult<ClinicWorkingHoursIndexViewModel>>> GetClinicWorkingHoursAsync(ClinicWorkingHoursSearchViewModel filter);
        Task<ServiceResult<ClinicWorkingHoursDetailsViewModel>> GetClinicWorkingHoursDetailsAsync(int clinicWorkingHoursId);
        Task<ServiceResult<ClinicWorkingHoursCreateEditViewModel>> GetClinicWorkingHoursForEditAsync(int clinicWorkingHoursId);
        Task<ServiceResult<ClinicWorkingHours>> CreateClinicWorkingHoursAsync(ClinicWorkingHoursCreateEditViewModel model);
        Task<ServiceResult<ClinicWorkingHours>> UpdateClinicWorkingHoursAsync(ClinicWorkingHoursCreateEditViewModel model);
        Task<ServiceResult> DeleteClinicWorkingHoursAsync(int clinicWorkingHoursId);
        Task<ServiceResult> ActivateClinicWorkingHoursAsync(int clinicWorkingHoursId);
        Task<ServiceResult> DeactivateClinicWorkingHoursAsync(int clinicWorkingHoursId);
        Task<ServiceResult<List<ClinicWorkingHoursPublicViewModel>>> GetActiveWorkingHoursAsync(int? clinicId = null);
        Task<ServiceResult<List<ClinicWorkingHoursPublicViewModel>>> GetByClinicIdAsync(int clinicId);
    }
}

