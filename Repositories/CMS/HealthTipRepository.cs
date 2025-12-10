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
    /// Repository برای عملیات داده‌ای HealthTip
    /// طراحی شده بر اساس اصول SRP و Bulletproof
    /// </summary>
    public class HealthTipRepository : IHealthTipRepository
    {
        private readonly ApplicationDbContext _context;

        public HealthTipRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<HealthTip> GetByIdAsync(int healthTipId)
        {
            return await _context.Set<HealthTip>()
                .Where(h => h.HealthTipId == healthTipId && !h.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<HealthTip>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<HealthTip>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(h => !h.IsDeleted);
            }

            return await query
                .OrderBy(h => h.DisplayOrder)
                .ThenByDescending(h => h.ViewCount)
                .ThenByDescending(h => h.PublishedAt ?? h.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<HealthTip>> GetPublishedTipsAsync(string category = null, int count = 10)
        {
            var now = DateTime.Now;
            var query = _context.Set<HealthTip>()
                .Where(h => !h.IsDeleted && 
                           h.IsPublished && 
                           (h.PublishedAt == null || h.PublishedAt <= now) &&
                           (h.ExpiryDate == null || h.ExpiryDate >= now))
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(h => h.Category == category);
            }

            return await query
                .OrderBy(h => h.DisplayOrder)
                .ThenByDescending(h => h.PublishedAt ?? h.CreatedAt)
                .ThenByDescending(h => h.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<HealthTip>> GetFeaturedTipsAsync(int count = 5)
        {
            var now = DateTime.Now;
            return await _context.Set<HealthTip>()
                .Where(h => !h.IsDeleted && 
                           h.IsPublished && 
                           h.IsFeatured &&
                           (h.PublishedAt == null || h.PublishedAt <= now) &&
                           (h.ExpiryDate == null || h.ExpiryDate >= now))
                .OrderBy(h => h.DisplayOrder)
                .ThenByDescending(h => h.PublishedAt ?? h.CreatedAt)
                .ThenByDescending(h => h.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<HealthTip>> GetByCategoryAsync(string category, int count = 10)
        {
            var now = DateTime.Now;
            return await _context.Set<HealthTip>()
                .Where(h => !h.IsDeleted && 
                           h.IsPublished && 
                           h.Category == category &&
                           (h.PublishedAt == null || h.PublishedAt <= now) &&
                           (h.ExpiryDate == null || h.ExpiryDate >= now))
                .OrderBy(h => h.DisplayOrder)
                .ThenByDescending(h => h.PublishedAt ?? h.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<HealthTip>> GetActiveTipsAsync(int count = 10)
        {
            var now = DateTime.Now;
            return await _context.Set<HealthTip>()
                .Where(h => !h.IsDeleted && 
                           h.IsPublished && 
                           (h.PublishedAt == null || h.PublishedAt <= now) &&
                           (h.ExpiryDate == null || h.ExpiryDate >= now))
                .OrderBy(h => h.DisplayOrder)
                .ThenByDescending(h => h.PublishedAt ?? h.CreatedAt)
                .ThenByDescending(h => h.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<HealthTip>> SearchTipsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<HealthTip>();

            var term = searchTerm.Trim().ToLower();
            var now = DateTime.Now;
            return await _context.Set<HealthTip>()
                .Where(h => !h.IsDeleted && 
                           h.IsPublished &&
                           (h.PublishedAt == null || h.PublishedAt <= now) &&
                           (h.ExpiryDate == null || h.ExpiryDate >= now) &&
                           (h.Title.ToLower().Contains(term) || 
                            h.Summary.ToLower().Contains(term) ||
                            h.Content.ToLower().Contains(term) ||
                            (h.Tags != null && h.Tags.ToLower().Contains(term))))
                .OrderByDescending(h => h.ViewCount)
                .ThenBy(h => h.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Set<HealthTip>()
                .Where(h => !h.IsDeleted && 
                           h.IsPublished && 
                           !string.IsNullOrEmpty(h.Category))
                .Select(h => h.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<HealthTip> GetBySlugAsync(string slug)
        {
            return await _context.Set<HealthTip>()
                .Where(h => h.Slug == slug && !h.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public void Add(HealthTip healthTip)
        {
            if (healthTip == null)
                throw new ArgumentNullException(nameof(healthTip));

            _context.Set<HealthTip>().Add(healthTip);
        }

        public void Update(HealthTip healthTip)
        {
            if (healthTip == null)
                throw new ArgumentNullException(nameof(healthTip));

            _context.Entry(healthTip).State = EntityState.Modified;
        }

        public void Delete(HealthTip healthTip)
        {
            if (healthTip == null)
                throw new ArgumentNullException(nameof(healthTip));

            healthTip.IsDeleted = true;
            healthTip.DeletedAt = DateTime.Now;
            _context.Entry(healthTip).State = EntityState.Modified;
        }
    }
}

