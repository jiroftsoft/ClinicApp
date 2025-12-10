using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Repository interface for Announcement entity operations
    /// </summary>
    public interface IAnnouncementRepository
    {
        Task<Announcement> GetByIdAsync(int announcementId);
        Task<List<Announcement>> GetActiveAnnouncementsAsync(int count = 10);
        Task<List<Announcement>> GetImportantAnnouncementsAsync(int count = 5);
        Task<List<Announcement>> GetAllAsync(bool includeDeleted = false);
        void Add(Announcement announcement);
        void Update(Announcement announcement);
        void Delete(Announcement announcement);
        Task<bool> ExistsAsync(int announcementId);
    }
}

