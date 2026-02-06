using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای تنظیمات فوتر
    /// </summary>
    public interface IFooterSettingsRepository
    {
        Task<FooterSettings> GetByClinicAsync(int? clinicId);
        Task<FooterSettings> GetDefaultAsync();
        Task<FooterSettings> GetByIdAsync(int footerSettingsId);
        void Add(FooterSettings entity);
        void Update(FooterSettings entity);
    }
}
