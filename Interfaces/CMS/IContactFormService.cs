using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت فرم تماس
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IContactFormService
    {
        Task<ServiceResult<PagedResult<ContactFormIndexViewModel>>> GetContactFormsAsync(ContactFormSearchViewModel searchModel);
        Task<ServiceResult<ContactFormDetailsViewModel>> GetContactFormDetailsAsync(int contactFormId);
        Task<ServiceResult<ContactFormCreateEditViewModel>> GetContactFormForEditAsync(int contactFormId);
        Task<ServiceResult<ContactForm>> CreateContactFormAsync(PublicContactFormViewModel model, string ipAddress = null, string userAgent = null);
        Task<ServiceResult<ContactForm>> UpdateContactFormAsync(ContactFormCreateEditViewModel model);
        Task<ServiceResult> DeleteContactFormAsync(int contactFormId);
        Task<ServiceResult> ReplyToContactFormAsync(ContactFormReplyViewModel model, string userId);
        Task<ServiceResult> MarkAsReadAsync(int contactFormId, string userId);
        Task<ServiceResult> MarkAsUnreadAsync(int contactFormId);
        Task<ServiceResult> ChangeStatusAsync(int contactFormId, ContactFormStatus status);
        Task<ServiceResult<int>> GetUnreadCountAsync();
        Task<ServiceResult<int>> GetNewCountAsync();
        Task<ServiceResult<int>> GetInProgressCountAsync();
        Task<ServiceResult<int>> GetRepliedCountAsync();
    }
}

