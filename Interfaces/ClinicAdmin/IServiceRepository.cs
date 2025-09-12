using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// The final, specialized repository interface for the Service entity.
    /// It provides only the essential, low-level data access methods.
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: Only manages Service data persistence
    /// ✅ Separation of Concerns: Business logic is in ServiceManagementService
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
        /// <summary>
        /// Fetches a single Service by its ID.
        /// دریافت یک خدمت بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه خدمت</param>
        /// <returns>خدمت مورد نظر یا null در صورت عدم وجود</returns>
        Task<Service> GetByIdAsync(int id);

        /// <summary>
        /// Fetches a list of Services for a specific category, with an optional search term.
        /// دریافت لیست خدمات یک دسته‌بندی با امکان جستجو
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <param name="searchTerm">عبارت جستجو (اختیاری)</param>
        /// <returns>لیست خدمات</returns>
        Task<List<Service>> GetServicesAsync(int serviceCategoryId, string searchTerm);

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
        /// Checks if a service code exists globally across all categories.
        /// بررسی وجود کد خدمت در تمام دسته‌بندی‌ها
        /// </summary>
        /// <param name="serviceCode">کد خدمت</param>
        /// <param name="excludeServiceId">شناسه خدمت که باید از بررسی مستثنی شود (برای ویرایش)</param>
        /// <returns>true اگر کد خدمت در هر دسته‌بندی وجود دارد</returns>
        Task<bool> DoesServiceCodeExistGloballyAsync(string serviceCode, int? excludeServiceId = null);

        /// <summary>
        /// Fetches active services for a category (for dropdown lists).
        /// دریافت خدمات فعال یک دسته‌بندی برای لیست‌های کشویی
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>لیست خدمات فعال</returns>
        Task<List<Service>> GetActiveServicesAsync(int serviceCategoryId);

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
    }
}
