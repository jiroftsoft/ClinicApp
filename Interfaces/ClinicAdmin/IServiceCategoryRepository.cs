using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// The final, specialized repository interface for the ServiceCategory entity.
    /// It provides only the essential, low-level data access methods.
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: Only manages ServiceCategory data persistence
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
    public interface IServiceCategoryRepository
    {
        /// <summary>
        /// Fetches a single ServiceCategory by its ID. Can optionally include soft-deleted items.
        /// دریافت یک دسته‌بندی خدمات بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه دسته‌بندی خدمات</param>
        /// <returns>دسته‌بندی خدمات مورد نظر یا null در صورت عدم وجود</returns>
        Task<ServiceCategory> GetByIdAsync(int id);

        /// <summary>
        /// Fetches a list of ServiceCategories for a specific department, with an optional search term.
        /// دریافت لیست دسته‌بندی‌های خدمات یک دپارتمان با امکان جستجو
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="searchTerm">عبارت جستجو (اختیاری)</param>
        /// <returns>لیست دسته‌بندی‌های خدمات</returns>
        Task<List<ServiceCategory>> GetServiceCategoriesAsync(int departmentId, string searchTerm);

        /// <summary>
        /// Checks if a category with a specific title already exists within a department.
        /// بررسی وجود دسته‌بندی با عنوان مشخص در یک دپارتمان
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="title">عنوان دسته‌بندی</param>
        /// <param name="excludeCategoryId">شناسه دسته‌بندی که باید از بررسی مستثنی شود (برای ویرایش)</param>
        /// <returns>true اگر دسته‌بندی تکراری وجود دارد</returns>
        Task<bool> DoesCategoryExistAsync(int departmentId, string title, int? excludeCategoryId = null);

        /// <summary>
        /// Adds a new ServiceCategory to the context (in-memory).
        /// افزودن دسته‌بندی خدمات جدید به context (در حافظه)
        /// </summary>
        /// <param name="category">دسته‌بندی خدمات جدید</param>
        void Add(ServiceCategory category);

        /// <summary>
        /// Marks an existing ServiceCategory as modified (in-memory).
        /// علامت‌گذاری دسته‌بندی خدمات به عنوان تغییر یافته (در حافظه)
        /// </summary>
        /// <param name="category">دسته‌بندی خدمات برای به‌روزرسانی</param>
        void Update(ServiceCategory category);

        /// <summary>
        /// Marks a ServiceCategory for removal from the context (in-memory).
        /// علامت‌گذاری دسته‌بندی خدمات برای حذف از context (در حافظه)
        /// </summary>
        /// <param name="category">دسته‌بندی خدمات برای حذف</param>
        void Delete(ServiceCategory category);

        /// <summary>
        /// Fetches active ServiceCategories for a department (for dropdown lists).
        /// دریافت دسته‌بندی‌های فعال یک دپارتمان برای لیست‌های کشویی
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>لیست دسته‌بندی‌های فعال</returns>
        Task<List<ServiceCategory>> GetActiveServiceCategoriesAsync(int departmentId);

        /// <summary>
        /// Fetches all ServiceCategories across all departments (Medical Environment).
        /// دریافت تمام دسته‌بندی‌های خدمات از همه دپارتمان‌ها
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو (اختیاری)</param>
        /// <returns>لیست تمام دسته‌بندی‌های خدمات</returns>
        Task<List<ServiceCategory>> GetAllServiceCategoriesAsync(string searchTerm);

        /// <summary>
        /// Saves all pending changes to the database.
        /// ذخیره تمام تغییرات در انتظار به پایگاه داده
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Fetches all active ServiceCategories across all departments for dropdown lists.
        /// دریافت تمام دسته‌بندی‌های خدمات فعال از همه دپارتمان‌ها برای لیست‌های کشویی
        /// </summary>
        /// <returns>لیست تمام دسته‌بندی‌های خدمات فعال</returns>
        Task<List<ServiceCategory>> GetAllActiveServiceCategoriesAsync();

        /// <summary>
        /// دریافت دسته‌بندی‌های خدمات بر اساس لیست شناسه‌ها برای عملیات گروهی
        /// </summary>
        /// <param name="categoryIds">لیست شناسه‌های دسته‌بندی‌های خدمات</param>
        /// <returns>لیستی از دسته‌بندی‌های خدمات یافت شده</returns>
        Task<List<ServiceCategory>> GetServiceCategoriesByIdsAsync(List<int> categoryIds);
    }
}