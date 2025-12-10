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
    /// Repository برای عملیات داده‌ای Announcement
    /// </summary>
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Announcement> GetByIdAsync(int announcementId)
        {
            return await _context.Set<Announcement>()
                .Where(a => a.AnnouncementId == announcementId && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Announcement>> GetActiveAnnouncementsAsync(int count = 10)
        {
            var now = DateTime.Now;
            return await _context.Set<Announcement>()
                .Where(a => !a.IsDeleted && 
                           a.IsActive &&
                           (a.StartDate == null || a.StartDate <= now) &&
                           (a.EndDate == null || a.EndDate >= now))
                .OrderByDescending(a => a.IsImportant)
                .ThenBy(a => a.DisplayOrder)
                .ThenByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Announcement>> GetImportantAnnouncementsAsync(int count = 5)
        {
            var now = DateTime.Now;
            return await _context.Set<Announcement>()
                .Where(a => !a.IsDeleted && 
                           a.IsActive && 
                           a.IsImportant &&
                           (a.StartDate == null || a.StartDate <= now) &&
                           (a.EndDate == null || a.EndDate >= now))
                .OrderBy(a => a.DisplayOrder)
                .ThenByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Announcement>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<Announcement>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(a => !a.IsDeleted);
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public void Add(Announcement announcement)
        {
            if (announcement == null)
                throw new ArgumentNullException(nameof(announcement));

            _context.Set<Announcement>().Add(announcement);
        }

        public void Update(Announcement announcement)
        {
            if (announcement == null)
                throw new ArgumentNullException(nameof(announcement));

            _context.Entry(announcement).State = EntityState.Modified;
        }

        public void Delete(Announcement announcement)
        {
            if (announcement == null)
                throw new ArgumentNullException(nameof(announcement));

            announcement.IsDeleted = true;
            announcement.DeletedAt = DateTime.Now;
            _context.Entry(announcement).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(int announcementId)
        {
            return await _context.Set<Announcement>()
                .AnyAsync(a => a.AnnouncementId == announcementId && !a.IsDeleted);
        }
    }
}

