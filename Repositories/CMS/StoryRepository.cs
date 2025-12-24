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
    /// Repository برای عملیات داده‌ای Story
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class StoryRepository : IStoryRepository
    {
        private readonly ApplicationDbContext _context;

        public StoryRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Story>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<Story>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(s => !s.IsDeleted);
            }

            return await query
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Story> GetByIdAsync(int storyId, bool includeDeleted = false)
        {
            var query = _context.Set<Story>()
                .Where(s => s.StoryId == storyId);
            
            if (!includeDeleted)
            {
                query = query.Where(s => !s.IsDeleted);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Story>> GetActiveStoriesAsync()
        {
            var now = DateTime.Now;
            return await _context.Set<Story>()
                .Where(s => !s.IsDeleted && 
                           s.IsActive &&
                           (s.StartDate == null || s.StartDate <= now) &&
                           (s.EndDate == null || s.EndDate >= now))
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Story> AddAsync(Story story)
        {
            if (story == null)
                throw new ArgumentNullException(nameof(story));

            _context.Set<Story>().Add(story);
            await _context.SaveChangesAsync();
            return story;
        }

        public async Task<Story> UpdateAsync(Story story)
        {
            if (story == null)
                throw new ArgumentNullException(nameof(story));

            _context.Entry(story).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return story;
        }

        public async Task<bool> DeleteAsync(int storyId, string deletedByUserId)
        {
            var story = await GetByIdAsync(storyId, includeDeleted: false);
            if (story == null)
                return false;

            story.IsDeleted = true;
            story.DeletedAt = DateTime.Now;
            story.DeletedByUserId = deletedByUserId;
            _context.Entry(story).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementViewCountAsync(int storyId)
        {
            var story = await GetByIdAsync(storyId, includeDeleted: false);
            if (story == null)
                return false;

            story.ViewCount++;
            _context.Entry(story).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
