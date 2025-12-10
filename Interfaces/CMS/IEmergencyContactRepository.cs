using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای عملیات داده‌ای EmergencyContact
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IEmergencyContactRepository
    {
        Task<EmergencyContact> GetByIdAsync(int emergencyContactId);
        Task<List<EmergencyContact>> GetAllAsync(bool includeDeleted = false);
        Task<List<EmergencyContact>> GetActiveContactsAsync();
        Task<List<EmergencyContact>> GetAlwaysVisibleContactsAsync();
        Task<List<EmergencyContact>> GetByContactTypeAsync(string contactType);
        Task<List<EmergencyContact>> SearchContactsAsync(string searchTerm);
        Task<EmergencyContact> GetBySlugAsync(string slug);
        void Add(EmergencyContact emergencyContact);
        void Update(EmergencyContact emergencyContact);
        void Delete(EmergencyContact emergencyContact);
    }
}

