using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای شبکه‌های اجتماعی فوتر
    /// </summary>
    public interface IFooterSocialRepository
    {
        Task<FooterSocial> GetByIdAsync(int footerSocialId);
        Task<List<FooterSocial>> GetActiveAsync(int? clinicId = null);
        Task<List<FooterSocial>> GetAllAsync(bool includeDeleted = false, int? clinicId = null);
        void Add(FooterSocial entity);
        void Update(FooterSocial entity);
        void Delete(FooterSocial entity);
    }
}
