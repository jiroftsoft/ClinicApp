using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;

namespace ClinicApp.Repositories.CMS
{
    /// <summary>
    /// Repository برای عملیات داده‌ای PatientEducationMaterial
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class PatientEducationMaterialRepository : IPatientEducationMaterialRepository
    {
        private readonly ApplicationDbContext _context;

        public PatientEducationMaterialRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PatientEducationMaterial> GetByIdAsync(int materialId)
        {
            return await _context.Set<PatientEducationMaterial>()
                .Where(p => p.PatientEducationMaterialId == materialId && !p.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PatientEducationMaterial>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<PatientEducationMaterial>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PatientEducationMaterial>> GetPublishedAsync(bool includeDeleted = false)
        {
            var query = _context.Set<PatientEducationMaterial>()
                .Where(p => p.IsPublished && !p.IsDeleted);
            
            if (includeDeleted)
            {
                query = _context.Set<PatientEducationMaterial>()
                    .Where(p => p.IsPublished);
            }

            return await query
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .ThenBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<PatientEducationMaterial>> GetByCategoryAsync(PatientEducationCategory category, bool includeDeleted = false)
        {
            var query = _context.Set<PatientEducationMaterial>()
                .Where(p => p.Category == category);
            
            if (!includeDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PatientEducationMaterial>> GetFeaturedAsync(int count = 10, bool includeDeleted = false)
        {
            var query = _context.Set<PatientEducationMaterial>()
                .Where(p => p.IsFeatured && p.IsPublished);
            
            if (!includeDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            return await query
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<PatientEducationMaterial>> SearchAsync(string searchTerm, PatientEducationCategory? category, bool? isPublished, bool? isFeatured, bool includeDeleted = false)
        {
            var query = _context.Set<PatientEducationMaterial>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim();
                query = query.Where(p => 
                    p.Title.Contains(search) ||
                    p.Description.Contains(search) ||
                    p.Content.Contains(search) ||
                    (p.Tags != null && p.Tags.Contains(search)));
            }

            if (category.HasValue)
            {
                query = query.Where(p => p.Category == category.Value);
            }

            if (isPublished.HasValue)
            {
                query = query.Where(p => p.IsPublished == isPublished.Value);
            }

            if (isFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == isFeatured.Value);
            }

            return await query
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .ThenBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PatientEducationMaterial> GetBySlugAsync(string slug)
        {
            return await _context.Set<PatientEducationMaterial>()
                .Where(p => p.Slug == slug && !p.IsDeleted && p.IsPublished)
                .FirstOrDefaultAsync();
        }

        public void Add(PatientEducationMaterial material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            _context.Set<PatientEducationMaterial>().Add(material);
        }

        public void Update(PatientEducationMaterial material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            _context.Entry(material).State = EntityState.Modified;
        }

        public void Delete(PatientEducationMaterial material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            material.IsDeleted = true;
            material.DeletedAt = DateTime.Now;
            _context.Entry(material).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(int materialId)
        {
            return await _context.Set<PatientEducationMaterial>()
                .AnyAsync(p => p.PatientEducationMaterialId == materialId && !p.IsDeleted);
        }

        public async Task IncrementDownloadCountAsync(int materialId)
        {
            var material = await GetByIdAsync(materialId);
            if (material != null)
            {
                material.DownloadCount++;
                _context.Entry(material).State = EntityState.Modified;
            }
        }

        public async Task IncrementViewCountAsync(int materialId)
        {
            var material = await GetByIdAsync(materialId);
            if (material != null)
            {
                material.ViewCount++;
                _context.Entry(material).State = EntityState.Modified;
            }
        }
    }
}

