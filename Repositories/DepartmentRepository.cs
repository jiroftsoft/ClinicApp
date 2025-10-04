using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Repositories;

/// <summary>
/// The final, production-ready implementation of the department repository.
/// It encapsulates all data access logic for the Department entity.
/// </summary>
public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Department>> GetDepartmentsAsync(int clinicId, string searchTerm)
    {
        var query = _context.Departments
            .AsNoTracking() // ✅ Performance optimization
            .Include(d => d.ServiceCategories) // Include service categories for count calculation
            .Include(d => d.DoctorDepartments.Select(dd => dd.Doctor)) // Include doctors for count calculation
            .Where(d => d.ClinicId == clinicId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedTerm = searchTerm.Trim();
            query = query.Where(d => d.Name.Contains(normalizedTerm));
        }

        return await query.OrderBy(d => d.Name).ToListAsync();
    }

    public Task<Department> GetByIdAsync(int departmentId)
    {
        // We don't use AsNoTracking() here as the service might want to update this entity.
        return _context.Departments
            .Include(d => d.Clinic)
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Include(d => d.ServiceCategories) // Include for business rule validation
            .Include(d => d.DoctorDepartments.Select(dd => dd.Doctor)) // Include for business rule validation
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
    }

    public Task<bool> DoesDepartmentExistAsync(int clinicId, string name, int? excludeDepartmentId = null)
    {
        var query = _context.Departments
            .AsNoTracking() // ✅ Performance optimization
            .Where(d => d.ClinicId == clinicId && d.Name == name);

        if (excludeDepartmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId != excludeDepartmentId.Value);
        }

        return query.AnyAsync();
    }

    public void Add(Department department)
    {
        _context.Departments.Add(department);
    }

    public void Update(Department department)
    {
        _context.Entry(department).State = EntityState.Modified;
    }

    public void Delete(Department department)
    {
        _context.Departments.Remove(department);
    }

    public Task<List<Department>> GetActiveDepartmentsAsync(int clinicId)
    {
        return _context.Departments
            .AsNoTracking() // ✅ Performance optimization
            .Where(d => d.ClinicId == clinicId && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    /// <summary>
    /// ✅ **Final Method Implementation:** Fetches all active departments for dropdown lists.
    /// </summary>
    public Task<List<Department>> GetAllActiveDepartmentsAsync()
    {
        return _context.Departments
            .AsNoTracking() // ✅ Performance optimization
            .Where(d => d.IsActive && !d.IsDeleted)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

        /// <summary>
        /// Get departments by IDs for bulk operations
        /// دریافت دپارتمان‌ها بر اساس لیست شناسه‌ها برای عملیات گروهی
        /// </summary>
        public async Task<List<Department>> GetDepartmentsByIdsAsync(List<int> departmentIds)
        {
            if (departmentIds == null || !departmentIds.Any())
                return new List<Department>();

            return await _context.Departments
                .AsNoTracking()
                .Where(d => departmentIds.Contains(d.DepartmentId) && !d.IsDeleted)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

}