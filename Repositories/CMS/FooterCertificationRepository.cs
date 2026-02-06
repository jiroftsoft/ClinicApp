using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Repositories.CMS
{
    /// <summary>
    /// Repository برای مجوزها/اعتبارهای فوتر
    /// </summary>
    public class FooterCertificationRepository : IFooterCertificationRepository
    {
        private readonly ApplicationDbContext _context;

        public FooterCertificationRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<FooterCertification> GetByIdAsync(int footerCertificationId)
        {
            return await _context.Set<FooterCertification>()
                .FirstOrDefaultAsync(f => f.FooterCertificationId == footerCertificationId);
        }

        public async Task<List<FooterCertification>> GetActiveAsync(int? clinicId = null)
        {
            var query = _context.Set<FooterCertification>()
                .Where(f => !f.IsDeleted && f.IsActive);

            if (clinicId.HasValue)
                query = query.Where(f => f.ClinicId == null || f.ClinicId == clinicId.Value);
            else
                query = query.Where(f => f.ClinicId == null);

            return await query
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.FooterCertificationId)
                .ToListAsync();
        }

        public async Task<List<FooterCertification>> GetAllAsync(bool includeDeleted = false, int? clinicId = null)
        {
            var query = _context.Set<FooterCertification>().AsQueryable();
            if (!includeDeleted)
                query = query.Where(f => !f.IsDeleted);
            if (clinicId.HasValue)
                query = query.Where(f => f.ClinicId == null || f.ClinicId == clinicId.Value);
            return await query
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        public void Add(FooterCertification entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.Set<FooterCertification>().Add(entity);
        }

        public void Update(FooterCertification entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(FooterCertification entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.Now;
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
