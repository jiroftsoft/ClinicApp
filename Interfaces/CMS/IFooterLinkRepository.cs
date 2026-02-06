using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای لینک‌های فوتر (سریع / خدمات)
    /// LinkType: 1 = QuickLink, 2 = ServiceLink
    /// </summary>
    public interface IFooterLinkRepository
    {
        Task<FooterLink> GetByIdAsync(int footerLinkId);
        Task<List<FooterLink>> GetActiveByTypeAsync(byte linkType, int? clinicId = null);
        Task<List<FooterLink>> GetAllAsync(bool includeDeleted = false, int? clinicId = null);
        void Add(FooterLink entity);
        void Update(FooterLink entity);
        void Delete(FooterLink entity);
    }
}
