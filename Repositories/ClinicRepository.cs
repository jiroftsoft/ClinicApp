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
            // 🏥 MEDICAL: Enhanced includes for detailed view
            return _context.Clinics
                .Include(c => c.CreatedByUser)
                .Include(c => c.UpdatedByUser)
                .Include(c => c.Departments.Select(d => d.ServiceCategories.Select(sc => sc.Services)))
                .Include(c => c.Departments.Select(d => d.DoctorDepartments.Select(dd => dd.Doctor)))
                .Include(c => c.Doctors)
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

        /// <summary>
        /// 🏥 MEDICAL: بررسی وابستگی‌های کلینیک قبل از حذف
        /// </summary>
        public async Task<ClinicDependencyInfo> GetClinicDependencyInfoAsync(int clinicId)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Departments.Select(d => d.ServiceCategories.Select(sc => sc.Services)))
                .Include(c => c.Departments.Select(d => d.DoctorDepartments.Select(dd => dd.Doctor)))
                .Where(c => c.ClinicId == clinicId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (clinic == null)
                return null;

            var dependencyInfo = new ClinicDependencyInfo
            {
                ClinicId = clinic.ClinicId,
                ClinicName = clinic.Name,
                TotalDepartmentCount = clinic.Departments?.Count ?? 0,
                ActiveDepartmentCount = clinic.Departments?.Count(d => !d.IsDeleted && d.IsActive) ?? 0
            };

            // محاسبه وابستگی‌های غیرمستقیم
            if (clinic.Departments != null)
            {
                foreach (var department in clinic.Departments.Where(d => !d.IsDeleted))
                {
                    var deptInfo = new DepartmentDependencyInfo
                    {
                        DepartmentId = department.DepartmentId,
                        DepartmentName = department.Name,
                        IsActive = department.IsActive,
                        ServiceCategoryCount = department.ServiceCategories?.Count(sc => !sc.IsDeleted) ?? 0,
                        ServiceCount = department.ServiceCategories?.Sum(sc => sc.Services?.Count(s => !s.IsDeleted) ?? 0) ?? 0,
                        DoctorCount = department.DoctorDepartments?.Count(dd => dd.Doctor != null && !dd.Doctor.IsDeleted) ?? 0
                    };

                    dependencyInfo.Departments.Add(deptInfo);
                }

                dependencyInfo.TotalServiceCategoryCount = dependencyInfo.Departments.Sum(d => d.ServiceCategoryCount);
                dependencyInfo.ActiveServiceCategoryCount = dependencyInfo.Departments.Where(d => d.IsActive).Sum(d => d.ServiceCategoryCount);
                
                dependencyInfo.TotalServiceCount = dependencyInfo.Departments.Sum(d => d.ServiceCount);
                dependencyInfo.ActiveServiceCount = dependencyInfo.Departments.Where(d => d.IsActive).Sum(d => d.ServiceCount);
                
                dependencyInfo.TotalDoctorCount = dependencyInfo.Departments.Sum(d => d.DoctorCount);
                dependencyInfo.ActiveDoctorCount = dependencyInfo.Departments.Where(d => d.IsActive).Sum(d => d.DoctorCount);
            }

            return dependencyInfo;
        }

        /// <summary>
        /// 🏥 MEDICAL: بررسی امکان حذف کلینیک بر اساس وابستگی‌ها
        /// </summary>
        public async Task<bool> CanDeleteClinicAsync(int clinicId)
        {
            var dependencyInfo = await GetClinicDependencyInfoAsync(clinicId);
            return dependencyInfo?.CanBeDeleted ?? false;
        }
    }
}