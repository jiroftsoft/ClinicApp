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
    /// The final, production-ready implementation of the service repository.
    /// It encapsulates all data access logic for the Service entity, optimized for performance.
    /// </summary>
    public class ServiceRepository : IServiceRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Service> GetByIdAsync(int id)
        {
            // Eagerly load related data needed for Details views.
            // No .AsNoTracking() as the service might intend to update this entity.
            return _context.Services
                .Include(s => s.ServiceCategory.Department.Clinic)
                .Include(s => s.CreatedByUser)
                .Include(s => s.UpdatedByUser)
                .FirstOrDefaultAsync(s => s.ServiceId == id);
        }

        public async Task<List<Service>> GetServicesAsync(int serviceCategoryId, string searchTerm)
        {
            var query = _context.Services
                .AsNoTracking() // ✅ Performance optimization for read-only lists.
                .Where(s => s.ServiceCategoryId == serviceCategoryId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedTerm = searchTerm.Trim();
                query = query.Where(s => s.Title.Contains(normalizedTerm) || 
                                       s.ServiceCode.Contains(normalizedTerm));
            }

            return await query.OrderBy(s => s.Title).ToListAsync();
        }

        public Task<bool> DoesServiceExistAsync(int serviceCategoryId, string serviceCode, int? excludeServiceId = null)
        {
            var query = _context.Services
                .AsNoTracking() // ✅ Performance optimization for existence checks.
                .Where(s => s.ServiceCategoryId == serviceCategoryId && s.ServiceCode == serviceCode);

            if (excludeServiceId.HasValue)
            {
                query = query.Where(s => s.ServiceId != excludeServiceId.Value);
            }

            return query.AnyAsync();
        }

        public Task<bool> DoesServiceCodeExistGloballyAsync(string serviceCode, int? excludeServiceId = null)
        {
            var query = _context.Services
                .AsNoTracking() // ✅ Performance optimization for existence checks.
                .Where(s => s.ServiceCode == serviceCode);

            if (excludeServiceId.HasValue)
            {
                query = query.Where(s => s.ServiceId != excludeServiceId.Value);
            }

            return query.AnyAsync();
        }

        public Task<List<Service>> GetActiveServicesAsync(int serviceCategoryId)
        {
            return _context.Services
                .AsNoTracking() // ✅ Performance optimization
                .Where(s => s.ServiceCategoryId == serviceCategoryId && s.IsActive)
                .OrderBy(s => s.Title)
                .ToListAsync();
        }

        public void Add(Service service)
        {
            _context.Services.Add(service);
        }

        public void Update(Service service)
        {
            _context.Entry(service).State = EntityState.Modified;
        }

        public void Delete(Service service)
        {
            _context.Services.Remove(service);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
