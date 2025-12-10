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
    /// Repository برای عملیات داده‌ای InsuranceInfo
    /// طراحی شده بر اساس اصول SRP و Bulletproof
    /// </summary>
    public class InsuranceInfoRepository : IInsuranceInfoRepository
    {
        private readonly ApplicationDbContext _context;

        public InsuranceInfoRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<InsuranceInfo> GetByIdAsync(int insuranceInfoId)
        {
            return await _context.Set<InsuranceInfo>()
                .Where(i => i.InsuranceInfoId == insuranceInfoId && !i.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<InsuranceInfo>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<InsuranceInfo>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(i => !i.IsDeleted);
            }

            return await query
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.InsuranceName)
                .ToListAsync();
        }

        public async Task<List<InsuranceInfo>> GetActiveInsurancesAsync(string insuranceType = null)
        {
            var query = _context.Set<InsuranceInfo>()
                .Where(i => !i.IsDeleted && i.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(insuranceType))
            {
                query = query.Where(i => i.InsuranceType == insuranceType);
            }

            return await query
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.InsuranceName)
                .ToListAsync();
        }

        public async Task<List<InsuranceInfo>> GetFeaturedInsurancesAsync(int count = 5)
        {
            return await _context.Set<InsuranceInfo>()
                .Where(i => !i.IsDeleted && 
                           i.IsActive && 
                           i.IsFeatured)
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.InsuranceName)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<InsuranceInfo>> GetByTypeAsync(string insuranceType, int count = 10)
        {
            return await _context.Set<InsuranceInfo>()
                .Where(i => !i.IsDeleted && 
                           i.IsActive && 
                           i.InsuranceType == insuranceType)
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.InsuranceName)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<InsuranceInfo>> SearchInsurancesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<InsuranceInfo>();

            var term = searchTerm.Trim().ToLower();
            return await _context.Set<InsuranceInfo>()
                .Where(i => !i.IsDeleted && 
                           i.IsActive &&
                           (i.InsuranceName.ToLower().Contains(term) || 
                            (i.Description != null && i.Description.ToLower().Contains(term)) ||
                            (i.FullDescription != null && i.FullDescription.ToLower().Contains(term))))
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.InsuranceName)
                .ToListAsync();
        }

        public async Task<List<string>> GetInsuranceTypesAsync()
        {
            return await _context.Set<InsuranceInfo>()
                .Where(i => !i.IsDeleted && 
                           i.IsActive && 
                           !string.IsNullOrEmpty(i.InsuranceType))
                .Select(i => i.InsuranceType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<InsuranceInfo> GetBySlugAsync(string slug)
        {
            return await _context.Set<InsuranceInfo>()
                .Where(i => i.Slug == slug && !i.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public void Add(InsuranceInfo insuranceInfo)
        {
            if (insuranceInfo == null)
                throw new ArgumentNullException(nameof(insuranceInfo));

            _context.Set<InsuranceInfo>().Add(insuranceInfo);
        }

        public void Update(InsuranceInfo insuranceInfo)
        {
            if (insuranceInfo == null)
                throw new ArgumentNullException(nameof(insuranceInfo));

            _context.Entry(insuranceInfo).State = EntityState.Modified;
        }

        public void Delete(InsuranceInfo insuranceInfo)
        {
            if (insuranceInfo == null)
                throw new ArgumentNullException(nameof(insuranceInfo));

            insuranceInfo.IsDeleted = true;
            insuranceInfo.DeletedAt = DateTime.Now;
            _context.Entry(insuranceInfo).State = EntityState.Modified;
        }
    }
}

