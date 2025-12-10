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
    /// Repository برای عملیات داده‌ای FAQ
    /// طراحی شده بر اساس اصول SRP و Bulletproof
    /// </summary>
    public class FAQRepository : IFAQRepository
    {
        private readonly ApplicationDbContext _context;

        public FAQRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<FAQ> GetByIdAsync(int faqId)
        {
            return await _context.Set<FAQ>()
                .Where(f => f.FAQId == faqId && !f.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<FAQ>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<FAQ>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(f => !f.IsDeleted);
            }

            return await query
                .OrderBy(f => f.DisplayOrder)
                .ThenByDescending(f => f.ViewCount)
                .ThenByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FAQ>> GetActiveFAQsAsync(string category = null)
        {
            var query = _context.Set<FAQ>()
                .Where(f => !f.IsDeleted && f.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(f => f.Category == category);
            }

            return await query
                .OrderBy(f => f.DisplayOrder)
                .ThenByDescending(f => f.ViewCount)
                .ThenByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FAQ>> GetFeaturedFAQsAsync(int count = 5)
        {
            return await _context.Set<FAQ>()
                .Where(f => !f.IsDeleted && 
                           f.IsActive && 
                           f.IsFeatured)
                .OrderBy(f => f.DisplayOrder)
                .ThenByDescending(f => f.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<FAQ>> GetByCategoryAsync(string category, int count = 10)
        {
            return await _context.Set<FAQ>()
                .Where(f => !f.IsDeleted && 
                           f.IsActive && 
                           f.Category == category)
                .OrderBy(f => f.DisplayOrder)
                .ThenByDescending(f => f.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<FAQ>> SearchFAQsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<FAQ>();

            var term = searchTerm.Trim().ToLower();
            return await _context.Set<FAQ>()
                .Where(f => !f.IsDeleted && 
                           f.IsActive &&
                           (f.Question.ToLower().Contains(term) || 
                            f.Answer.ToLower().Contains(term) ||
                            (f.Tags != null && f.Tags.ToLower().Contains(term))))
                .OrderByDescending(f => f.ViewCount)
                .ThenBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Set<FAQ>()
                .Where(f => !f.IsDeleted && 
                           f.IsActive && 
                           !string.IsNullOrEmpty(f.Category))
                .Select(f => f.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<FAQ> GetBySlugAsync(string slug)
        {
            return await _context.Set<FAQ>()
                .Where(f => f.Slug == slug && !f.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public void Add(FAQ faq)
        {
            if (faq == null)
                throw new ArgumentNullException(nameof(faq));

            _context.Set<FAQ>().Add(faq);
        }

        public void Update(FAQ faq)
        {
            if (faq == null)
                throw new ArgumentNullException(nameof(faq));

            _context.Entry(faq).State = EntityState.Modified;
        }

        public void Delete(FAQ faq)
        {
            if (faq == null)
                throw new ArgumentNullException(nameof(faq));

            faq.IsDeleted = true;
            faq.DeletedAt = DateTime.Now;
            _context.Entry(faq).State = EntityState.Modified;
        }
    }
}

