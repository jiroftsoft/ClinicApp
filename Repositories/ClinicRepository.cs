using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;

namespace ClinicApp.Repositories
{
    /// <summary>
    /// The final, production-ready implementation of the clinic repository.
    /// It encapsulates all data access logic for the Clinic entity and is optimized for performance.
    /// </summary>
    public class ClinicRepository : IClinicRepository
    {
        private readonly ApplicationDbContext _context;

        public ClinicRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Clinic>> GetClinicsAsync(string searchTerm)
        {
            // ✅ FIX: Explicitly declare the type as IQueryable<Clinic> instead of using var.
            // ✅ CRITICAL FIX: Add IsDeleted filter to exclude soft-deleted clinics
            // ✅ PERFORMANCE FIX: Include Departments for count calculation
            IQueryable<Clinic> query = _context.Clinics.AsNoTracking()
                .Include(c => c.Departments) // 🏥 MEDICAL: Include departments for count calculation
                .Where(c => !c.IsDeleted); // 🏥 MEDICAL: Only show non-deleted clinics

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedTerm = searchTerm.Trim();
                query = query.Where(c => c.Name.Contains(normalizedTerm) || c.Address.Contains(normalizedTerm));
            }

            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        public Task<Clinic> GetByIdAsync(int clinicId)
        {
            // We do not use AsNoTracking() here because the service might want to update this entity.
            // ✅ CRITICAL FIX: Add IsDeleted filter to exclude soft-deleted clinics
            return _context.Clinics
                .Include(c => c.CreatedByUser)
                .Include(c => c.UpdatedByUser)
                .Where(c => !c.IsDeleted) // 🏥 MEDICAL: Only show non-deleted clinics
                .FirstOrDefaultAsync(c => c.ClinicId == clinicId);
        }

        public Task<bool> DoesClinicExistAsync(string name, int? excludeId = null)
        {
            var query = _context.Clinics.AsNoTracking() // ✅ Performance optimization
                                       .Where(c => c.Name == name && !c.IsDeleted); // 🏥 MEDICAL: Only check non-deleted clinics
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.ClinicId != excludeId.Value);
            }
            return query.AnyAsync();
        }

        // ✅ CORRECTED: Add is a synchronous operation. Renamed to Add.
        public void Add(Clinic clinic)
        {
            _context.Clinics.Add(clinic);
        }

        // ✅ CORRECT: Update is a synchronous operation.
        public void Update(Clinic clinic)
        {
            _context.Entry(clinic).State = EntityState.Modified;
        }

        // ✅ CORRECT: Delete is a synchronous operation.
        public void Delete(Clinic clinic)
        {
            _context.Clinics.Remove(clinic); // EF's SaveChanges override will handle the soft delete logic.
        }

        public Task<List<Clinic>> GetActiveClinicsAsync()
        {
            return _context.Clinics
                           .AsNoTracking() // ✅ Performance optimization
                           .Where(c => c.IsActive && !c.IsDeleted) // 🏥 MEDICAL: Only show active and non-deleted clinics
                           .OrderBy(c => c.Name)
                           .ToListAsync();
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}