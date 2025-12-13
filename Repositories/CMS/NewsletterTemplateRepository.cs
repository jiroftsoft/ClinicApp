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
    /// Repository برای عملیات داده‌ای NewsletterTemplate
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class NewsletterTemplateRepository : INewsletterTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public NewsletterTemplateRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<NewsletterTemplate> GetByIdAsync(int templateId)
        {
            return await _context.Set<NewsletterTemplate>()
                .Where(t => t.NewsletterTemplateId == templateId && !t.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<NewsletterTemplate>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterTemplate>().AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(t => !t.IsDeleted);
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<List<NewsletterTemplate>> GetActiveAsync(bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterTemplate>()
                .Where(t => t.IsActive);

            if (!includeDeleted)
            {
                query = query.Where(t => !t.IsDeleted);
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public void Add(NewsletterTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            _context.Set<NewsletterTemplate>().Add(template);
        }

        public void Update(NewsletterTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            _context.Entry(template).State = EntityState.Modified;
        }

        public void Delete(NewsletterTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            template.IsDeleted = true;
            template.DeletedAt = DateTime.Now;
            _context.Entry(template).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(int templateId)
        {
            return await _context.Set<NewsletterTemplate>()
                .AnyAsync(t => t.NewsletterTemplateId == templateId && !t.IsDeleted);
        }
    }
}

