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
    /// Repository برای عملیات داده‌ای Slider
    /// </summary>
    public class SliderRepository : ISliderRepository
    {
        private readonly ApplicationDbContext _context;

        public SliderRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Slider> GetByIdAsync(int sliderId)
        {
            return await _context.Set<Slider>()
                .Where(s => s.SliderId == sliderId && !s.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Slider>> GetActiveSlidersAsync(string position = null)
        {
            var now = DateTime.Now;
            var query = _context.Set<Slider>()
                .Where(s => !s.IsDeleted && 
                           s.IsActive &&
                           (s.StartDate == null || s.StartDate <= now) &&
                           (s.EndDate == null || s.EndDate >= now));

            if (!string.IsNullOrEmpty(position))
            {
                query = query.Where(s => s.Position == position);
            }

            return await query
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Slider>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<Slider>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(s => !s.IsDeleted);
            }

            return await query
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public void Add(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            _context.Set<Slider>().Add(slider);
        }

        public void Update(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            _context.Entry(slider).State = EntityState.Modified;
        }

        public void Delete(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            slider.IsDeleted = true;
            slider.DeletedAt = DateTime.Now;
            _context.Entry(slider).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(int sliderId)
        {
            return await _context.Set<Slider>()
                .AnyAsync(s => s.SliderId == sliderId && !s.IsDeleted);
        }
    }
}

