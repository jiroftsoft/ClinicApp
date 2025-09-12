using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Repositories
{
    /// <summary>
    /// The final, production-ready implementation of the service category repository.
    /// It encapsulates all data access logic for the ServiceCategory entity, optimized for performance.
    /// </summary>
    public class ServiceCategoryRepository : IServiceCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<ServiceCategory> GetByIdAsync(int id)
        {
            // Eagerly load related data needed for Details views.
            // No .AsNoTracking() as the service might intend to update this entity.
            return _context.ServiceCategories
                .Include(sc => sc.Services) // Include services for business rule checks
                .Include(sc => sc.Department.Clinic)
                .Include(sc => sc.CreatedByUser)
                .Include(sc => sc.UpdatedByUser)
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == id);
        }

        public async Task<List<ServiceCategory>> GetServiceCategoriesAsync(int departmentId, string searchTerm)
        {
            var query = _context.ServiceCategories
                .AsNoTracking() // ✅ Performance optimization for read-only lists.
                .Include(sc => sc.Services) // Include services for count calculation
                .Include(sc => sc.Department) // Include department for display
                .Include(sc => sc.CreatedByUser) // Include creator info
                .Where(sc => sc.DepartmentId == departmentId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedTerm = searchTerm.Trim();
                query = query.Where(sc => sc.Title.Contains(normalizedTerm));
            }

            return await query.OrderBy(sc => sc.Title).ToListAsync();
        }

        public Task<bool> DoesCategoryExistAsync(int departmentId, string title, int? excludeCategoryId = null)
        {
            var query = _context.ServiceCategories
                .AsNoTracking() // ✅ Performance optimization for existence checks.
                .Where(sc => sc.DepartmentId == departmentId && sc.Title == title);

            if (excludeCategoryId.HasValue)
            {
                query = query.Where(sc => sc.ServiceCategoryId != excludeCategoryId.Value);
            }

            return query.AnyAsync();
        }

        public void Add(ServiceCategory category)
        {
            _context.ServiceCategories.Add(category);
        }

        public void Update(ServiceCategory category)
        {
            _context.Entry(category).State = EntityState.Modified;
        }

        public void Delete(ServiceCategory category)
        {
            _context.ServiceCategories.Remove(category);
        }

        /// <summary>
        /// ✅ **Final Method Implementation:** Fetches active categories for dropdown lists.
        /// </summary>
        public Task<List<ServiceCategory>> GetActiveServiceCategoriesAsync(int departmentId)
        {
            return _context.ServiceCategories
                .AsNoTracking() // ✅ Performance optimization
                .Where(sc => sc.DepartmentId == departmentId && sc.IsActive)
                .OrderBy(sc => sc.Title)
                .ToListAsync();
        }

        /// <summary>
        /// ✅ **Medical Environment:** Fetches all service categories across all departments.
        /// </summary>
        public async Task<List<ServiceCategory>> GetAllServiceCategoriesAsync(string searchTerm)
        {
            var query = _context.ServiceCategories
                .AsNoTracking() // ✅ Performance optimization for read-only lists
                .Include(sc => sc.Services) // Include services for count calculation
                .Include(sc => sc.Department) // Include department info for display
                .Include(sc => sc.CreatedByUser) // Include creator info
                .Where(sc => !sc.IsDeleted); // Only non-deleted categories

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedTerm = searchTerm.Trim();
                query = query.Where(sc => sc.Title.Contains(normalizedTerm) || 
                                         sc.Department.Name.Contains(normalizedTerm));
            }

            return await query.OrderBy(sc => sc.Department.Name)
                             .ThenBy(sc => sc.Title)
                             .ToListAsync();
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        /// <summary>
        /// ✅ **Final Method Implementation:** Fetches all active service categories across all departments for dropdown lists.
        /// </summary>
        public Task<List<ServiceCategory>> GetAllActiveServiceCategoriesAsync()
        {
            return _context.ServiceCategories
                .AsNoTracking() // ✅ Performance optimization
                .Include(sc => sc.Department) // Include department info for display
                .Where(sc => sc.IsActive && !sc.IsDeleted)
                .OrderBy(sc => sc.Department.Name)
                .ThenBy(sc => sc.Title)
                .ToListAsync();
        }
    }
}