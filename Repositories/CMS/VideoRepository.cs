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
    /// Repository برای عملیات داده‌ای Video
    /// طراحی شده بر اساس اصول SRP و برای محیط Production درمانی
    /// </summary>
    public class VideoRepository : IVideoRepository
    {
        private readonly ApplicationDbContext _context;

        public VideoRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Video> GetByIdAsync(int videoId)
        {
            return await _context.Set<Video>()
                .Where(v => v.VideoId == videoId && !v.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Video>> GetActiveVideosAsync(int count = 10, string category = null)
        {
            var query = _context.Set<Video>()
                .Where(v => !v.IsDeleted && v.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(v => v.Category == category);
            }

            return await query
                .OrderBy(v => v.DisplayOrder)
                .ThenByDescending(v => v.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Video>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<Video>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(v => !v.IsDeleted);
            }

            return await query
                .OrderBy(v => v.DisplayOrder)
                .ThenByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Video>> GetByCategoryAsync(string category, int count = 10)
        {
            return await _context.Set<Video>()
                .Where(v => !v.IsDeleted && v.IsActive && v.Category == category)
                .OrderBy(v => v.DisplayOrder)
                .ThenByDescending(v => v.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Video>> GetVideosForHomePageAsync(int count = 6, string category = null)
        {
            var query = _context.Set<Video>()
                .Where(v => !v.IsDeleted && v.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(v => v.Category == category);
            }

            return await query
                .OrderBy(v => v.DisplayOrder)
                .ThenByDescending(v => v.ViewCount)
                .ThenByDescending(v => v.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Set<Video>()
                .Where(v => !v.IsDeleted && v.IsActive && !string.IsNullOrEmpty(v.Category))
                .Select(v => v.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public void Add(Video video)
        {
            if (video == null)
                throw new ArgumentNullException(nameof(video));

            _context.Set<Video>().Add(video);
        }

        public void Update(Video video)
        {
            if (video == null)
                throw new ArgumentNullException(nameof(video));

            _context.Entry(video).State = EntityState.Modified;
        }

        public void Delete(Video video)
        {
            if (video == null)
                throw new ArgumentNullException(nameof(video));

            video.IsDeleted = true;
            video.DeletedAt = DateTime.UtcNow;
            _context.Entry(video).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(int videoId)
        {
            return await _context.Set<Video>()
                .AnyAsync(v => v.VideoId == videoId && !v.IsDeleted);
        }
    }
}

