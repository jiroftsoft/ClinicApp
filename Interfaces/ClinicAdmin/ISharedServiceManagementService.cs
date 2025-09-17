using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// Interface برای مدیریت خدمات مشترک بین دپارتمان‌ها
    /// طراحی شده برای محیط‌های درمانی با اطمینان 100%
    /// 
    /// اصول طراحی:
    /// ✅ Single Responsibility: فقط مدیریت خدمات مشترک
    /// ✅ Open/Closed: قابل توسعه بدون تغییر کد موجود
    /// ✅ Liskov Substitution: قابل جایگزینی با implementation های مختلف
    /// ✅ Interface Segregation: Interface های کوچک و متمرکز
    /// ✅ Dependency Inversion: وابستگی به abstraction نه concrete
    /// </summary>
    public interface ISharedServiceManagementService
    {
        #region CRUD Operations (عملیات پایه)

        /// <summary>
        /// دریافت لیست خدمات مشترک با فیلتر و صفحه‌بندی
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <param name="serviceId">شناسه خدمت (اختیاری)</param>
        /// <param name="isActive">وضعیت فعال/غیرفعال (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه عملیات با داده‌های صفحه‌بندی شده</returns>
        Task<ServiceResult<PagedResult<SharedServiceIndexViewModel>>> GetSharedServicesAsync(
            int? departmentId = null,
            int? serviceId = null,
            bool? isActive = null,
            string searchTerm = "",
            int pageNumber = 1,
            int pageSize = 20);

        /// <summary>
        /// دریافت جزئیات کامل یک خدمت مشترک
        /// </summary>
        /// <param name="sharedServiceId">شناسه خدمت مشترک</param>
        /// <returns>جزئیات خدمت مشترک</returns>
        Task<ServiceResult<SharedServiceDetailsViewModel>> GetSharedServiceDetailsAsync(int sharedServiceId);

        /// <summary>
        /// دریافت اطلاعات یک خدمت مشترک برای ویرایش
        /// </summary>
        /// <param name="sharedServiceId">شناسه خدمت مشترک</param>
        /// <returns>اطلاعات خدمت مشترک برای ویرایش</returns>
        Task<ServiceResult<SharedServiceCreateEditViewModel>> GetSharedServiceForEditAsync(int sharedServiceId);

        /// <summary>
        /// ایجاد خدمت مشترک جدید
        /// </summary>
        /// <param name="model">مدل ایجاد خدمت مشترک</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> CreateSharedServiceAsync(SharedServiceCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی خدمت مشترک موجود
        /// </summary>
        /// <param name="model">مدل ویرایش خدمت مشترک</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> UpdateSharedServiceAsync(SharedServiceCreateEditViewModel model);

        /// <summary>
        /// حذف نرم خدمت مشترک
        /// </summary>
        /// <param name="sharedServiceId">شناسه خدمت مشترک</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> SoftDeleteSharedServiceAsync(int sharedServiceId);

        /// <summary>
        /// بازیابی خدمت مشترک حذف شده
        /// </summary>
        /// <param name="sharedServiceId">شناسه خدمت مشترک</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> RestoreSharedServiceAsync(int sharedServiceId);

        #endregion

        #region Business Operations (عملیات کسب‌وکار)

        /// <summary>
        /// اضافه کردن خدمت به دپارتمان
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="notes">توضیحات خاص دپارتمان</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> AddServiceToDepartmentAsync(int serviceId, int departmentId, string notes = null);

        /// <summary>
        /// حذف خدمت از دپارتمان
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> RemoveServiceFromDepartmentAsync(int serviceId, int departmentId);

        /// <summary>
        /// فعال/غیرفعال کردن خدمت در دپارتمان
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="isActive">وضعیت فعال/غیرفعال</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> ToggleServiceInDepartmentAsync(int serviceId, int departmentId, bool isActive);

        /// <summary>
        /// کپی کردن خدمت به دپارتمان‌های دیگر
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentIds">لیست شناسه‌های دپارتمان</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> CopyServiceToDepartmentsAsync(int serviceId, List<int> departmentIds);

        #endregion

        #region Query Operations (عملیات جستجو)

        /// <summary>
        /// دریافت تمام خدمات مشترک یک دپارتمان
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>لیست خدمات مشترک</returns>
        Task<ServiceResult<List<SharedServiceIndexViewModel>>> GetDepartmentSharedServicesAsync(int departmentId);

        /// <summary>
        /// دریافت تمام دپارتمان‌هایی که یک خدمت در آن‌ها مشترک است
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>لیست دپارتمان‌ها</returns>
        Task<ServiceResult<List<DepartmentLookupViewModel>>> GetServiceSharedDepartmentsAsync(int serviceId);

        /// <summary>
        /// بررسی اینکه آیا خدمت در دپارتمان موجود است یا نه
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>true اگر خدمت در دپارتمان موجود باشد</returns>
        Task<bool> IsServiceInDepartmentAsync(int serviceId, int departmentId);

        #endregion

        #region Lookup Operations (عملیات جستجو برای Dropdown)

        /// <summary>
        /// دریافت لیست خدمات فعال برای Dropdown
        /// </summary>
        /// <param name="excludeDepartmentId">شناسه دپارتمان برای حذف (اختیاری)</param>
        /// <returns>لیست خدمات برای Dropdown</returns>
        Task<ServiceResult<List<LookupItemViewModel>>> GetActiveServicesForLookupAsync(int? excludeDepartmentId = null);

        /// <summary>
        /// دریافت لیست دپارتمان‌های فعال برای Dropdown
        /// </summary>
        /// <param name="excludeServiceId">شناسه خدمت برای حذف (اختیاری)</param>
        /// <returns>لیست دپارتمان‌ها برای Dropdown</returns>
        Task<ServiceResult<List<LookupItemViewModel>>> GetActiveDepartmentsForLookupAsync(int? excludeServiceId = null);

        #endregion

        #region Statistics and Reports (آمار و گزارش‌ها)

        /// <summary>
        /// دریافت آمار خدمات مشترک
        /// </summary>
        /// <returns>آمار خدمات مشترک</returns>
        Task<ServiceResult<SharedServiceStatisticsViewModel>> GetSharedServiceStatisticsAsync();

        /// <summary>
        /// بررسی اینکه آیا خدمت در دپارتمان مشترک است
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>true اگر مشترک باشد</returns>
        bool IsServiceInDepartment(int serviceId, int departmentId);

        /// <summary>
        /// اضافه کردن خدمت به دپارتمان
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult<bool>> AddServiceToDepartment(int serviceId, int departmentId, string userId);

        /// <summary>
        /// حذف خدمت از دپارتمان
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult<bool>> RemoveServiceFromDepartment(int serviceId, int departmentId, string userId);

        /// <summary>
        /// دریافت آمار کلی خدمات مشترک (متد همزمان)
        /// </summary>
        /// <returns>آمار خدمات مشترک</returns>
        ServiceResult<SharedServiceStatisticsViewModel> GetSharedServiceStatistics();

        /// <summary>
        /// دریافت گزارش استفاده از خدمات مشترک
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش استفاده</returns>
        Task<ServiceResult<List<SharedServiceUsageReportViewModel>>> GetSharedServiceUsageReportAsync(
            int? departmentId = null,
            System.DateTime? startDate = null,
            System.DateTime? endDate = null);

        #endregion

        #region Validation Operations (عملیات اعتبارسنجی)

        /// <summary>
        /// بررسی تکراری نبودن خدمت در دپارتمان
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="excludeSharedServiceId">شناسه خدمت مشترک برای حذف از بررسی (اختیاری)</param>
        /// <returns>true اگر تکراری باشد</returns>
        Task<bool> IsSharedServiceDuplicateAsync(int serviceId, int departmentId, int? excludeSharedServiceId = null);

        /// <summary>
        /// اعتبارسنجی کامل مدل خدمت مشترک
        /// </summary>
        /// <param name="model">مدل برای اعتبارسنجی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateSharedServiceModelAsync(SharedServiceCreateEditViewModel model);

        #endregion
    }
}
