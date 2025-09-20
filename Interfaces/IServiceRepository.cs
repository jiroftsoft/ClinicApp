using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Repository interface for Service entity operations
    /// Comprehensive interface that combines all service-related data access methods
    /// Production-ready with comprehensive service management
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: Only manages Service data persistence
    /// ✅ Separation of Concerns: Business logic is in Service Layer
    /// ✅ High Testability: Simple interface, easy to mock for unit tests
    /// ✅ Clean Architecture: Repository layer focuses purely on data access
    /// 
    /// این Interface طبق اصول Clean Architecture طراحی شده:
    /// - فقط عملیات داده‌ای اساسی
    /// - منطق کسب‌وکار در Service Layer
    /// - قابلیت تست بالا
    /// - جداسازی کامل concerns
    /// </summary>
    public interface IServiceRepository
    {
        #region Basic CRUD Operations (عملیات پایه CRUD)

        /// <summary>
        /// Fetches a single Service by its ID.
        /// دریافت یک خدمت بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه خدمت</param>
        /// <returns>خدمت مورد نظر یا null در صورت عدم وجود</returns>
        Task<Service> GetByIdAsync(int id);

        /// <summary>
        /// Get service by ID (alternative method name for compatibility)
        /// دریافت خدمت بر اساس شناسه (نام متد جایگزین برای سازگاری)
        /// </summary>
        /// <param name="serviceId">Service ID</param>
        /// <returns>Service entity or null if not found</returns>
        Task<Service> GetServiceByIdAsync(int serviceId);

        /// <summary>
        /// Adds a new Service to the context (in-memory).
        /// افزودن خدمت جدید به context (در حافظه)
        /// </summary>
        /// <param name="service">خدمت جدید</param>
        void Add(Service service);

        /// <summary>
        /// Marks an existing Service as modified (in-memory).
        /// علامت‌گذاری خدمت به عنوان تغییر یافته (در حافظه)
        /// </summary>
        /// <param name="service">خدمت برای به‌روزرسانی</param>
        void Update(Service service);

        /// <summary>
        /// Marks a Service for removal from the context (in-memory).
        /// علامت‌گذاری خدمت برای حذف از context (در حافظه)
        /// </summary>
        /// <param name="service">خدمت برای حذف</param>
        void Delete(Service service);

        /// <summary>
        /// Saves all pending changes to the database.
        /// ذخیره تمام تغییرات معلق در پایگاه داده
        /// </summary>
        /// <returns>تسک async برای ذخیره‌سازی</returns>
        Task SaveChangesAsync();

        #endregion

        #region Service Retrieval Methods (متدهای دریافت خدمات)

        /// <summary>
        /// Fetches a list of Services for a specific category, with an optional search term.
        /// دریافت لیست خدمات یک دسته‌بندی با امکان جستجو
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <param name="searchTerm">عبارت جستجو (اختیاری)</param>
        /// <returns>لیست خدمات</returns>
        Task<List<Service>> GetServicesAsync(int serviceCategoryId, string searchTerm);

        /// <summary>
        /// Get services by IDs
        /// دریافت خدمات بر اساس لیست شناسه‌ها
        /// </summary>
        /// <param name="serviceIds">List of service IDs</param>
        /// <returns>List of services</returns>
        Task<List<Service>> GetServicesByIdsAsync(List<int> serviceIds);

        /// <summary>
        /// Fetches active services for a category (for dropdown lists).
        /// دریافت خدمات فعال یک دسته‌بندی برای لیست‌های کشویی
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>لیست خدمات فعال</returns>
        Task<List<Service>> GetActiveServicesAsync(int serviceCategoryId);

        /// <summary>
        /// Get active services by category (alternative method name for compatibility)
        /// دریافت خدمات فعال بر اساس دسته‌بندی (نام متد جایگزین برای سازگاری)
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>List of active services</returns>
        Task<List<Service>> GetActiveServicesByCategoryAsync(int categoryId);

        /// <summary>
        /// Get all active services
        /// دریافت تمام خدمات فعال
        /// </summary>
        /// <returns>List of all active services</returns>
        Task<List<Service>> GetAllActiveServicesAsync();

        #endregion

        #region Service Validation Methods (متدهای اعتبارسنجی خدمات)

        /// <summary>
        /// Checks if a service with a specific code already exists within a category.
        /// بررسی وجود خدمت با کد مشخص در یک دسته‌بندی
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <param name="serviceCode">کد خدمت</param>
        /// <param name="excludeServiceId">شناسه خدمت که باید از بررسی مستثنی شود (برای ویرایش)</param>
        /// <returns>true اگر خدمت تکراری وجود دارد</returns>
        Task<bool> DoesServiceExistAsync(int serviceCategoryId, string serviceCode, int? excludeServiceId = null);

        /// <summary>
        /// Checks if a service exists by ServiceId.
        /// بررسی وجود خدمت بر اساس ServiceId
        /// </summary>
        /// <param name="serviceId">Service ID</param>
        /// <returns>True if service exists and is not deleted</returns>
        Task<bool> DoesServiceExistByIdAsync(int serviceId);

        /// <summary>
        /// Checks if a service code exists globally across all categories.
        /// بررسی وجود کد خدمت در تمام دسته‌بندی‌ها
        /// </summary>
        /// <param name="serviceCode">کد خدمت</param>
        /// <param name="excludeServiceId">شناسه خدمت که باید از بررسی مستثنی شود (برای ویرایش)</param>
        /// <returns>true اگر کد خدمت در هر دسته‌بندی وجود دارد</returns>
        Task<bool> DoesServiceCodeExistGloballyAsync(string serviceCode, int? excludeServiceId = null);

        #endregion

        #region Service Category Methods (متدهای دسته‌بندی خدمات)

        /// <summary>
        /// Get all service categories
        /// دریافت تمام دسته‌بندی‌های خدمات
        /// </summary>
        /// <returns>List of service categories</returns>
        Task<List<ServiceCategory>> GetServiceCategoriesAsync();

        /// <summary>
        /// Get service category by ID
        /// دریافت دسته‌بندی خدمت بر اساس شناسه
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Service category or null if not found</returns>
        Task<ServiceCategory> GetServiceCategoryByIdAsync(int categoryId);

        #endregion

        #region Service Components Management (مدیریت اجزای خدمات)

        /// <summary>
        /// Fetches a service with its components.
        /// دریافت خدمت همراه با اجزای آن
        /// </summary>
        /// <param name="id">شناسه خدمت</param>
        /// <returns>خدمت همراه با اجزای آن</returns>
        Task<Service> GetByIdWithComponentsAsync(int id);

        /// <summary>
        /// Fetches service components for a specific service.
        /// دریافت اجزای یک خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>لیست اجزای خدمت</returns>
        Task<List<ServiceComponent>> GetServiceComponentsAsync(int serviceId);

        /// <summary>
        /// Adds a new service component.
        /// افزودن جزء جدید به خدمت
        /// </summary>
        /// <param name="component">جزء جدید</param>
        void AddServiceComponent(ServiceComponent component);

        /// <summary>
        /// Updates an existing service component.
        /// به‌روزرسانی جزء موجود
        /// </summary>
        /// <param name="component">جزء برای به‌روزرسانی</param>
        void UpdateServiceComponent(ServiceComponent component);

        /// <summary>
        /// Removes a service component.
        /// حذف جزء خدمت
        /// </summary>
        /// <param name="component">جزء برای حذف</param>
        void DeleteServiceComponent(ServiceComponent component);

        #endregion

        #region Service Calculation Methods (متدهای محاسبه خدمات)

        /// <summary>
        /// Calculate total price for services
        /// محاسبه مجموع قیمت خدمات
        /// </summary>
        /// <param name="serviceIds">List of service IDs</param>
        /// <returns>Total price</returns>
        Task<decimal> CalculateServicesTotalPriceAsync(List<int> serviceIds);

        /// <summary>
        /// Get active services for lookup (like dropdowns)
        /// دریافت خدمات فعال برای انتخاب (مثل Dropdown ها)
        /// </summary>
        /// <returns>List of active services for selection</returns>
        Task<ServiceResult<List<Service>>> GetActiveServicesForLookupAsync();

        #endregion
    }
}
