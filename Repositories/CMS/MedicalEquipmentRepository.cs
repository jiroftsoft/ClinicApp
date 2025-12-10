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
    /// Repository برای عملیات داده‌ای MedicalEquipment
    /// طراحی شده بر اساس اصول SRP و Bulletproof
    /// </summary>
    public class MedicalEquipmentRepository : IMedicalEquipmentRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicalEquipmentRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MedicalEquipment> GetByIdAsync(int medicalEquipmentId)
        {
            return await _context.Set<MedicalEquipment>()
                .Where(e => e.MedicalEquipmentId == medicalEquipmentId && !e.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<MedicalEquipment>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<MedicalEquipment>()
                .AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            return await query
                .OrderBy(e => e.DisplayOrder)
                .ThenBy(e => e.EquipmentName)
                .ToListAsync();
        }

        public async Task<List<MedicalEquipment>> GetActiveEquipmentsAsync()
        {
            return await _context.Set<MedicalEquipment>()
                .Where(e => !e.IsDeleted && e.IsActive)
                .OrderBy(e => e.DisplayOrder)
                .ThenBy(e => e.EquipmentName)
                .ToListAsync();
        }

        public async Task<List<MedicalEquipment>> GetFeaturedEquipmentsAsync(int count = 6)
        {
            return await _context.Set<MedicalEquipment>()
                .Where(e => !e.IsDeleted && 
                           e.IsActive && 
                           e.IsFeatured)
                .OrderBy(e => e.DisplayOrder)
                .ThenBy(e => e.EquipmentName)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<MedicalEquipment>> GetByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return new List<MedicalEquipment>();

            return await _context.Set<MedicalEquipment>()
                .Where(e => !e.IsDeleted && 
                           e.IsActive && 
                           e.Category == category)
                .OrderBy(e => e.DisplayOrder)
                .ThenBy(e => e.EquipmentName)
                .ToListAsync();
        }

        public async Task<List<MedicalEquipment>> SearchEquipmentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<MedicalEquipment>();

            var term = searchTerm.Trim().ToLower();
            return await _context.Set<MedicalEquipment>()
                .Where(e => !e.IsDeleted && 
                           e.IsActive &&
                           (e.EquipmentName.ToLower().Contains(term) || 
                            (e.Model != null && e.Model.ToLower().Contains(term)) ||
                            (e.Manufacturer != null && e.Manufacturer.ToLower().Contains(term)) ||
                            (e.Category != null && e.Category.ToLower().Contains(term)) ||
                            (e.Description != null && e.Description.ToLower().Contains(term)) ||
                            (e.ShortDescription != null && e.ShortDescription.ToLower().Contains(term))))
                .OrderBy(e => e.DisplayOrder)
                .ThenBy(e => e.EquipmentName)
                .ToListAsync();
        }

        public async Task<MedicalEquipment> GetBySlugAsync(string slug)
        {
            return await _context.Set<MedicalEquipment>()
                .Where(e => e.Slug == slug && !e.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public void Add(MedicalEquipment medicalEquipment)
        {
            if (medicalEquipment == null)
                throw new ArgumentNullException(nameof(medicalEquipment));

            _context.Set<MedicalEquipment>().Add(medicalEquipment);
        }

        public void Update(MedicalEquipment medicalEquipment)
        {
            if (medicalEquipment == null)
                throw new ArgumentNullException(nameof(medicalEquipment));

            _context.Entry(medicalEquipment).State = EntityState.Modified;
        }

        public void Delete(MedicalEquipment medicalEquipment)
        {
            if (medicalEquipment == null)
                throw new ArgumentNullException(nameof(medicalEquipment));

            medicalEquipment.IsDeleted = true;
            medicalEquipment.DeletedAt = DateTime.Now;
            _context.Entry(medicalEquipment).State = EntityState.Modified;
        }
    }
}

