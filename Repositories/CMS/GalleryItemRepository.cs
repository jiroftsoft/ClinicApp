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
    /// Repository برای عملیات داده‌ای GalleryItem
    /// </summary>
    public class GalleryItemRepository : IGalleryItemRepository
    {
        private readonly ApplicationDbContext _context;

        public GalleryItemRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<GalleryItem> GetByIdAsync(int galleryItemId)
        {
            return await _context.Set<GalleryItem>()
                .Where(g => g.GalleryItemId == galleryItemId && !g.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<GalleryItem>> GetActiveItemsAsync(int count = 10, string category = null)
        {
            var query = _context.Set<GalleryItem>()
                .Where(g => !g.IsDeleted && g.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(g => g.Category == category);
            }

            return await query
                .OrderBy(g => g.DisplayOrder)
                .ThenByDescending(g => g.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<GalleryItem>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<GalleryItem>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(g => !g.IsDeleted);
            }

            return await query
                .OrderBy(g => g.DisplayOrder)
                .ThenByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<GalleryItem>> GetByCategoryAsync(string category, int count = 10)
        {
            return await _context.Set<GalleryItem>()
                .Where(g => !g.IsDeleted && g.IsActive && g.Category == category)
                .OrderBy(g => g.DisplayOrder)
                .ThenByDescending(g => g.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public void Add(GalleryItem galleryItem)
        {
            if (galleryItem == null)
                throw new ArgumentNullException(nameof(galleryItem));

            _context.Set<GalleryItem>().Add(galleryItem);
        }

        public void Update(GalleryItem galleryItem)
        {
            if (galleryItem == null)
                throw new ArgumentNullException(nameof(galleryItem));

            _context.Entry(galleryItem).State = EntityState.Modified;
        }

        public void Delete(GalleryItem galleryItem)
        {
            if (galleryItem == null)
                throw new ArgumentNullException(nameof(galleryItem));

            galleryItem.IsDeleted = true;
            galleryItem.DeletedAt = DateTime.Now;
            _context.Entry(galleryItem).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(int galleryItemId)
        {
            return await _context.Set<GalleryItem>()
                .AnyAsync(g => g.GalleryItemId == galleryItemId && !g.IsDeleted);
        }
    }
}

