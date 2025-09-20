using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Helpers;
using System;

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

        /// <summary>
        /// بررسی وجود خدمت بر اساس ServiceId
        /// </summary>
        public Task<bool> DoesServiceExistByIdAsync(int serviceId)
        {
            return _context.Services
                .AsNoTracking() // ✅ Performance optimization for existence checks.
                .Where(s => s.ServiceId == serviceId && !s.IsDeleted)
                .AnyAsync();
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

        #region ServiceComponents Management (مدیریت اجزای خدمات)

        public Task<Service> GetByIdWithComponentsAsync(int id)
        {
            return _context.Services
                .Include(s => s.ServiceCategory.Department.Clinic)
                .Include(s => s.ServiceComponents)
                .Include(s => s.CreatedByUser)
                .Include(s => s.UpdatedByUser)
                .FirstOrDefaultAsync(s => s.ServiceId == id);
        }

        public Task<List<ServiceComponent>> GetServiceComponentsAsync(int serviceId)
        {
            return _context.ServiceComponents
                .AsNoTracking()
                .Where(sc => sc.ServiceId == serviceId && !sc.IsDeleted)
                .OrderBy(sc => sc.ComponentType)
                .ToListAsync();
        }

        public void AddServiceComponent(ServiceComponent component)
        {
            _context.ServiceComponents.Add(component);
        }

        public void UpdateServiceComponent(ServiceComponent component)
        {
            _context.Entry(component).State = EntityState.Modified;
        }

        public void DeleteServiceComponent(ServiceComponent component)
        {
            _context.ServiceComponents.Remove(component);
        }

        #endregion

        #region Additional Methods for Compatibility (متدهای اضافی برای سازگاری)

        /// <summary>
        /// Get service by ID (alternative method name for compatibility)
        /// دریافت خدمت بر اساس شناسه (نام متد جایگزین برای سازگاری)
        /// </summary>
        public Task<Service> GetServiceByIdAsync(int serviceId)
        {
            return GetByIdAsync(serviceId);
        }

        /// <summary>
        /// Get services by IDs
        /// دریافت خدمات بر اساس لیست شناسه‌ها
        /// </summary>
        public async Task<List<Service>> GetServicesByIdsAsync(List<int> serviceIds)
        {
            if (serviceIds == null || !serviceIds.Any())
                return new List<Service>();

            return await _context.Services
                .Where(s => serviceIds.Contains(s.ServiceId) && !s.IsDeleted)
                .Include(s => s.ServiceCategory)
                .ToListAsync();
        }

        /// <summary>
        /// Get active services by category (alternative method name for compatibility)
        /// دریافت خدمات فعال بر اساس دسته‌بندی (نام متد جایگزین برای سازگاری)
        /// </summary>
        public Task<List<Service>> GetActiveServicesByCategoryAsync(int categoryId)
        {
            return GetActiveServicesAsync(categoryId);
        }

        /// <summary>
        /// Get all active services
        /// دریافت تمام خدمات فعال
        /// </summary>
        public async Task<List<Service>> GetAllActiveServicesAsync()
        {
            return await _context.Services
                .Where(s => s.IsActive && !s.IsDeleted)
                .Include(s => s.ServiceCategory)
                .OrderBy(s => s.Title)
                .ToListAsync();
        }

        /// <summary>
        /// Get all service categories
        /// دریافت تمام دسته‌بندی‌های خدمات
        /// </summary>
        public async Task<List<ServiceCategory>> GetServiceCategoriesAsync()
        {
            return await _context.ServiceCategories
                .Where(sc => !sc.IsDeleted)
                .Include(sc => sc.Department)
                .OrderBy(sc => sc.Title)
                .ToListAsync();
        }

        /// <summary>
        /// Get service category by ID
        /// دریافت دسته‌بندی خدمت بر اساس شناسه
        /// </summary>
        public async Task<ServiceCategory> GetServiceCategoryByIdAsync(int categoryId)
        {
            return await _context.ServiceCategories
                .Where(sc => sc.ServiceCategoryId == categoryId && !sc.IsDeleted)
                .Include(sc => sc.Department)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Calculate total price for services
        /// محاسبه مجموع قیمت خدمات
        /// </summary>
        public async Task<decimal> CalculateServicesTotalPriceAsync(List<int> serviceIds)
        {
            if (serviceIds == null || !serviceIds.Any())
                return 0;

            return await _context.Services
                .Where(s => serviceIds.Contains(s.ServiceId) && !s.IsDeleted)
                .SumAsync(s => s.Price);
        }

        /// <summary>
        /// Get active services for lookup (like dropdowns)
        /// دریافت خدمات فعال برای انتخاب (مثل Dropdown ها)
        /// </summary>
        public async Task<ServiceResult<List<Service>>> GetActiveServicesForLookupAsync()
        {
            try
            {
                var services = await _context.Services
                    .AsNoTracking()
                    .Include(s => s.ServiceCategory)
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Title)
                    .ToListAsync();

                return ServiceResult<List<Service>>.Successful(services, "لیست خدمات فعال دریافت شد");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Service>>.Failed("خطا در دریافت لیست خدمات فعال");
            }
        }

        #endregion
    }
}
