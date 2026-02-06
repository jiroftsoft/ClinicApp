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
    /// Repository برای لینک‌های فوتر (LinkType: 1=QuickLink, 2=ServiceLink)
    /// </summary>
    public class FooterLinkRepository : IFooterLinkRepository
    {
        private readonly ApplicationDbContext _context;

        public FooterLinkRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<FooterLink> GetByIdAsync(int footerLinkId)
        {
            return await _context.Set<FooterLink>()
                .FirstOrDefaultAsync(f => f.FooterLinkId == footerLinkId);
        }

        public async Task<List<FooterLink>> GetActiveByTypeAsync(byte linkType, int? clinicId = null)
        {
            var query = _context.Set<FooterLink>()
                .Where(f => !f.IsDeleted && f.IsActive && f.LinkType == linkType);

            if (clinicId.HasValue)
                query = query.Where(f => f.ClinicId == null || f.ClinicId == clinicId.Value);
            else
                query = query.Where(f => f.ClinicId == null);

            return await query
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.FooterLinkId)
                .ToListAsync();
        }

        public async Task<List<FooterLink>> GetAllAsync(bool includeDeleted = false, int? clinicId = null)
        {
            var query = _context.Set<FooterLink>().AsQueryable();
            if (!includeDeleted)
                query = query.Where(f => !f.IsDeleted);
            if (clinicId.HasValue)
                query = query.Where(f => f.ClinicId == null || f.ClinicId == clinicId.Value);
            return await query
                .OrderBy(f => f.LinkType)
                .ThenBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        public void Add(FooterLink entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.Set<FooterLink>().Add(entity);
        }

        public void Update(FooterLink entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(FooterLink entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.Now;
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
