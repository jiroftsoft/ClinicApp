using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Repository interface for ContactForm entity operations
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IContactFormRepository
    {
        Task<ContactForm> GetByIdAsync(int contactFormId);
        Task<List<ContactForm>> GetAllAsync(bool includeDeleted = false);
        Task<List<ContactForm>> GetByStatusAsync(ContactFormStatus status, bool includeDeleted = false);
        Task<List<ContactForm>> GetByCategoryAsync(ContactFormCategory category, bool includeDeleted = false);
        Task<List<ContactForm>> GetUnreadAsync(bool includeDeleted = false);
        Task<int> GetUnreadCountAsync();
        Task<List<ContactForm>> SearchAsync(string searchTerm, ContactFormCategory? category, ContactFormStatus? status, bool? isRead, bool includeDeleted = false);
        void Add(ContactForm contactForm);
        void Update(ContactForm contactForm);
        void Delete(ContactForm contactForm);
        Task<bool> ExistsAsync(int contactFormId);
    }
}

