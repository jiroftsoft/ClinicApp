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
    /// Repository برای شبکه‌های اجتماعی فوتر
    /// </summary>
    public class FooterSocialRepository : IFooterSocialRepository
    {
        private readonly ApplicationDbContext _context;

        public FooterSocialRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<FooterSocial> GetByIdAsync(int footerSocialId)
        {
            return await _context.Set<FooterSocial>()
                .FirstOrDefaultAsync(f => f.FooterSocialId == footerSocialId);
        }

        public async Task<List<FooterSocial>> GetActiveAsync(int? clinicId = null)
        {
            var query = _context.Set<FooterSocial>()
                .Where(f => !f.IsDeleted && f.IsActive);

            if (clinicId.HasValue)
                query = query.Where(f => f.ClinicId == null || f.ClinicId == clinicId.Value);
            else
                query = query.Where(f => f.ClinicId == null);

            return await query
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.FooterSocialId)
                .ToListAsync();
        }

        public async Task<List<FooterSocial>> GetAllAsync(bool includeDeleted = false, int? clinicId = null)
        {
            var query = _context.Set<FooterSocial>().AsQueryable();
            if (!includeDeleted)
                query = query.Where(f => !f.IsDeleted);
            if (clinicId.HasValue)
                query = query.Where(f => f.ClinicId == null || f.ClinicId == clinicId.Value);
            return await query
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        public void Add(FooterSocial entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.Set<FooterSocial>().Add(entity);
        }

        public void Update(FooterSocial entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(FooterSocial entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.Now;
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
