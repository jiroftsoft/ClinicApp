using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای مجوزها/اعتبارهای فوتر
    /// </summary>
    public interface IFooterCertificationRepository
    {
        Task<FooterCertification> GetByIdAsync(int footerCertificationId);
        Task<List<FooterCertification>> GetActiveAsync(int? clinicId = null);
        Task<List<FooterCertification>> GetAllAsync(bool includeDeleted = false, int? clinicId = null);
        void Add(FooterCertification entity);
        void Update(FooterCertification entity);
        void Delete(FooterCertification entity);
    }
}
