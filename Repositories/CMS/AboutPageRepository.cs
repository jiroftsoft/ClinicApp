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
    /// Repository برای عملیات داده‌ای AboutPage
    /// طراحی شده بر اساس اصول SRP و Bulletproof
    /// </summary>
    public class AboutPageRepository : IAboutPageRepository
    {
        private readonly ApplicationDbContext _context;

        public AboutPageRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<AboutPage> GetByIdAsync(int aboutPageId)
        {
            return await _context.Set<AboutPage>()
                .Where(a => a.AboutPageId == aboutPageId && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<AboutPage>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<AboutPage>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(a => !a.IsDeleted);
            }

            return await query
                .OrderBy(a => a.DisplayOrder)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<AboutPage> GetActiveAboutPageAsync()
        {
            return await _context.Set<AboutPage>()
                .Where(a => !a.IsDeleted && a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ThenByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<AboutPage> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return null;

            return await _context.Set<AboutPage>()
                .Where(a => !a.IsDeleted && a.Slug == slug)
                .FirstOrDefaultAsync();
        }

        public void Add(AboutPage aboutPage)
        {
            if (aboutPage == null)
                throw new ArgumentNullException(nameof(aboutPage));

            _context.Set<AboutPage>().Add(aboutPage);
        }

        public void Update(AboutPage aboutPage)
        {
            if (aboutPage == null)
                throw new ArgumentNullException(nameof(aboutPage));

            _context.Entry(aboutPage).State = EntityState.Modified;
        }

        public void Delete(AboutPage aboutPage)
        {
            if (aboutPage == null)
                throw new ArgumentNullException(nameof(aboutPage));

            _context.Set<AboutPage>().Remove(aboutPage);
        }
    }
}
