using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Service entity operations
    /// Production-ready with comprehensive service management
    /// </summary>
    public interface IServiceRepository
    {
        /// <summary>
        /// Get service by ID
        /// </summary>
        /// <param name="serviceId">Service ID</param>
        /// <returns>Service entity or null if not found</returns>
        Task<Service> GetServiceByIdAsync(int serviceId);

        /// <summary>
        /// Get services by IDs
        /// </summary>
        /// <param name="serviceIds">List of service IDs</param>
        /// <returns>List of services</returns>
        Task<List<Service>> GetServicesByIdsAsync(List<int> serviceIds);

        /// <summary>
        /// Get active services by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>List of active services</returns>
        Task<List<Service>> GetActiveServicesByCategoryAsync(int categoryId);

        /// <summary>
        /// Get all service categories
        /// </summary>
        /// <returns>List of service categories</returns>
        Task<List<ServiceCategory>> GetServiceCategoriesAsync();

        /// <summary>
        /// Get service category by ID
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Service category or null if not found</returns>
        Task<ServiceCategory> GetServiceCategoryByIdAsync(int categoryId);

        /// <summary>
        /// Get all active services
        /// </summary>
        /// <returns>List of all active services</returns>
        Task<List<Service>> GetAllActiveServicesAsync();

        /// <summary>
        /// Calculate total price for services
        /// </summary>
        /// <param name="serviceIds">List of service IDs</param>
        /// <returns>Total price</returns>
        Task<decimal> CalculateServicesTotalPriceAsync(List<int> serviceIds);
    }
}
