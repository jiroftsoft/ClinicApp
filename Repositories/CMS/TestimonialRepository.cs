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
    /// Repository برای عملیات داده‌ای Testimonial
    /// </summary>
    public class TestimonialRepository : ITestimonialRepository
    {
        private readonly ApplicationDbContext _context;

        public TestimonialRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Testimonial> GetByIdAsync(int testimonialId)
        {
            return await _context.Set<Testimonial>()
                .Where(t => t.TestimonialId == testimonialId && !t.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Testimonial>> GetApprovedTestimonialsAsync(int count = 10)
        {
            return await _context.Set<Testimonial>()
                .Where(t => !t.IsDeleted && t.IsApproved)
                .OrderByDescending(t => t.IsFeatured)
                .ThenBy(t => t.DisplayOrder)
                .ThenByDescending(t => t.ApprovedAt ?? t.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Testimonial>> GetFeaturedTestimonialsAsync(int count = 3)
        {
            return await _context.Set<Testimonial>()
                .Where(t => !t.IsDeleted && t.IsApproved && t.IsFeatured)
                .OrderBy(t => t.DisplayOrder)
                .ThenByDescending(t => t.ApprovedAt ?? t.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Testimonial>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<Testimonial>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(t => !t.IsDeleted);
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Testimonial>> GetPendingApprovalAsync()
        {
            return await _context.Set<Testimonial>()
                .Where(t => !t.IsDeleted && !t.IsApproved)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public void Add(Testimonial testimonial)
        {
            if (testimonial == null)
                throw new ArgumentNullException(nameof(testimonial));

            _context.Set<Testimonial>().Add(testimonial);
        }

        public void Update(Testimonial testimonial)
        {
            if (testimonial == null)
                throw new ArgumentNullException(nameof(testimonial));

            _context.Entry(testimonial).State = EntityState.Modified;
        }

        public void Delete(Testimonial testimonial)
        {
            if (testimonial == null)
                throw new ArgumentNullException(nameof(testimonial));

            testimonial.IsDeleted = true;
            testimonial.DeletedAt = DateTime.Now;
            _context.Entry(testimonial).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(int testimonialId)
        {
            return await _context.Set<Testimonial>()
                .AnyAsync(t => t.TestimonialId == testimonialId && !t.IsDeleted);
        }
    }
}

