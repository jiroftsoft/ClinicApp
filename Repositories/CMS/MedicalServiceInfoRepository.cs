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
    /// Repository برای عملیات داده‌ای MedicalServiceInfo
    /// طراحی شده بر اساس اصول SRP و Bulletproof
    /// </summary>
    public class MedicalServiceInfoRepository : IMedicalServiceInfoRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicalServiceInfoRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MedicalServiceInfo> GetByIdAsync(int medicalServiceInfoId)
        {
            return await _context.Set<MedicalServiceInfo>()
                .Include(m => m.Service)
                .Where(m => m.MedicalServiceInfoId == medicalServiceInfoId && !m.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<MedicalServiceInfo> GetByServiceIdAsync(int serviceId)
        {
            return await _context.Set<MedicalServiceInfo>()
                .Include(m => m.Service)
                .Where(m => m.ServiceId == serviceId && !m.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<MedicalServiceInfo>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<MedicalServiceInfo>()
                .Include(m => m.Service)
                .AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(m => !m.IsDeleted);
            }

            return await query
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Service.Title)
                .ToListAsync();
        }

        public async Task<List<MedicalServiceInfo>> GetActiveServiceInfosAsync(int? serviceCategoryId = null)
        {
            var query = _context.Set<MedicalServiceInfo>()
                .Include(m => m.Service)
                .Where(m => !m.IsDeleted && m.IsActive)
                .AsQueryable();

            if (serviceCategoryId.HasValue)
            {
                query = query.Where(m => m.Service.ServiceCategoryId == serviceCategoryId.Value);
            }

            return await query
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Service.Title)
                .ToListAsync();
        }

        public async Task<List<MedicalServiceInfo>> GetFeaturedServiceInfosAsync(int count = 6)
        {
            return await _context.Set<MedicalServiceInfo>()
                .Include(m => m.Service)
                .Where(m => !m.IsDeleted && 
                           m.IsActive && 
                           m.IsFeatured)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Service.Title)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<MedicalServiceInfo>> GetByServiceCategoryAsync(int serviceCategoryId, int count = 10)
        {
            return await _context.Set<MedicalServiceInfo>()
                .Include(m => m.Service)
                .Where(m => !m.IsDeleted && 
                           m.IsActive && 
                           m.Service.ServiceCategoryId == serviceCategoryId)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Service.Title)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<MedicalServiceInfo>> SearchServiceInfosAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<MedicalServiceInfo>();

            var term = searchTerm.Trim().ToLower();
            return await _context.Set<MedicalServiceInfo>()
                .Include(m => m.Service)
                .Where(m => !m.IsDeleted && 
                           m.IsActive &&
                           (m.Service.Title.ToLower().Contains(term) || 
                            (m.Description != null && m.Description.ToLower().Contains(term)) ||
                            (m.FullDescription != null && m.FullDescription.ToLower().Contains(term)) ||
                            (m.Features != null && m.Features.ToLower().Contains(term))))
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Service.Title)
                .ToListAsync();
        }

        public async Task<MedicalServiceInfo> GetBySlugAsync(string slug)
        {
            return await _context.Set<MedicalServiceInfo>()
                .Include(m => m.Service)
                .Where(m => m.Slug == slug && !m.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public void Add(MedicalServiceInfo medicalServiceInfo)
        {
            if (medicalServiceInfo == null)
                throw new ArgumentNullException(nameof(medicalServiceInfo));

            _context.Set<MedicalServiceInfo>().Add(medicalServiceInfo);
        }

        public void Update(MedicalServiceInfo medicalServiceInfo)
        {
            if (medicalServiceInfo == null)
                throw new ArgumentNullException(nameof(medicalServiceInfo));

            _context.Entry(medicalServiceInfo).State = EntityState.Modified;
        }

        public void Delete(MedicalServiceInfo medicalServiceInfo)
        {
            if (medicalServiceInfo == null)
                throw new ArgumentNullException(nameof(medicalServiceInfo));

            medicalServiceInfo.IsDeleted = true;
            medicalServiceInfo.DeletedAt = DateTime.Now;
            _context.Entry(medicalServiceInfo).State = EntityState.Modified;
        }
    }
}

