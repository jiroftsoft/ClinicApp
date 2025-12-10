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
    /// Repository برای عملیات داده‌ای EmergencyContact
    /// طراحی شده بر اساس اصول SRP و Bulletproof
    /// </summary>
    public class EmergencyContactRepository : IEmergencyContactRepository
    {
        private readonly ApplicationDbContext _context;

        public EmergencyContactRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<EmergencyContact> GetByIdAsync(int emergencyContactId)
        {
            return await _context.Set<EmergencyContact>()
                .Where(e => e.EmergencyContactId == emergencyContactId && !e.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<EmergencyContact>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<EmergencyContact>()
                .AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            return await query
                .OrderBy(e => e.DisplayOrder)
                .ThenBy(e => e.Title)
                .ToListAsync();
        }

        public async Task<List<EmergencyContact>> GetActiveContactsAsync()
        {
            return await _context.Set<EmergencyContact>()
                .Where(e => !e.IsDeleted && e.IsActive)
                .OrderBy(e => e.DisplayOrder)
                .ThenBy(e => e.Title)
                .ToListAsync();
        }

        public async Task<List<EmergencyContact>> GetAlwaysVisibleContactsAsync()
        {
            return await _context.Set<EmergencyContact>()
                .Where(e => !e.IsDeleted && 
                           e.IsActive && 
                           e.IsAlwaysVisible)
                .OrderBy(e => e.DisplayOrder)
                .ThenBy(e => e.Title)
                .ToListAsync();
        }

        public async Task<List<EmergencyContact>> GetByContactTypeAsync(string contactType)
        {
            if (string.IsNullOrWhiteSpace(contactType))
                return new List<EmergencyContact>();

            return await _context.Set<EmergencyContact>()
                .Where(e => !e.IsDeleted && 
                           e.IsActive && 
                           e.ContactType == contactType)
                .OrderBy(e => e.DisplayOrder)
                .ThenBy(e => e.Title)
                .ToListAsync();
        }

        public async Task<List<EmergencyContact>> SearchContactsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<EmergencyContact>();

            var term = searchTerm.Trim().ToLower();
            return await _context.Set<EmergencyContact>()
                .Where(e => !e.IsDeleted && 
                           e.IsActive &&
                           (e.Title.ToLower().Contains(term) || 
                            (e.PhoneNumber != null && e.PhoneNumber.Contains(term)) ||
                            (e.ContactType != null && e.ContactType.ToLower().Contains(term)) ||
                            (e.ShortDescription != null && e.ShortDescription.ToLower().Contains(term)) ||
                            (e.Instructions != null && e.Instructions.ToLower().Contains(term))))
                .OrderBy(e => e.DisplayOrder)
                .ThenBy(e => e.Title)
                .ToListAsync();
        }

        public async Task<EmergencyContact> GetBySlugAsync(string slug)
        {
            return await _context.Set<EmergencyContact>()
                .Where(e => e.Slug == slug && !e.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public void Add(EmergencyContact emergencyContact)
        {
            if (emergencyContact == null)
                throw new ArgumentNullException(nameof(emergencyContact));

            _context.Set<EmergencyContact>().Add(emergencyContact);
        }

        public void Update(EmergencyContact emergencyContact)
        {
            if (emergencyContact == null)
                throw new ArgumentNullException(nameof(emergencyContact));

            _context.Entry(emergencyContact).State = EntityState.Modified;
        }

        public void Delete(EmergencyContact emergencyContact)
        {
            if (emergencyContact == null)
                throw new ArgumentNullException(nameof(emergencyContact));

            emergencyContact.IsDeleted = true;
            emergencyContact.DeletedAt = DateTime.Now;
            _context.Entry(emergencyContact).State = EntityState.Modified;
        }
    }
}

