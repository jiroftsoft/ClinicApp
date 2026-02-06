using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Repositories.CMS
{
    /// <summary>
    /// Repository برای تنظیمات فوتر
    /// </summary>
    public class FooterSettingsRepository : IFooterSettingsRepository
    {
        private readonly ApplicationDbContext _context;

        public FooterSettingsRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<FooterSettings> GetByClinicAsync(int? clinicId)
        {
            var query = _context.Set<FooterSettings>()
                .Where(f => f.IsActive);

            // اول رکورد مخصوص کلینیک، بعد سراسری (ClinicId == null)
            var byClinic = await query
                .Where(f => f.ClinicId == clinicId)
                .OrderByDescending(f => f.UpdatedAt)
                .FirstOrDefaultAsync();

            if (byClinic != null)
                return byClinic;

            return await query
                .Where(f => f.ClinicId == null)
                .OrderByDescending(f => f.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<FooterSettings> GetDefaultAsync()
        {
            return await _context.Set<FooterSettings>()
                .Where(f => f.IsActive && f.ClinicId == null)
                .OrderByDescending(f => f.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<FooterSettings> GetByIdAsync(int footerSettingsId)
        {
            return await _context.Set<FooterSettings>()
                .FirstOrDefaultAsync(f => f.FooterSettingsId == footerSettingsId);
        }

        public void Add(FooterSettings entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.Set<FooterSettings>().Add(entity);
        }

        public void Update(FooterSettings entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
        }
    }
}
