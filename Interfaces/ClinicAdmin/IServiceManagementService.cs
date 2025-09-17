using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels;

namespace ClinicApp.Interfaces.ClinicAdmin;

/// <summary>
/// سرویس تخصصی برای مدیریت کامل دسته‌بندی‌های خدمات و خود خدمات.
/// </summary>
public interface IServiceManagementService
{
    #region Service Category Management (مدیریت دسته‌بندی خدمات)

    /// <summary>
    /// دریافت لیست دسته‌بندی‌های خدمات یک دپارتمان خاص.
    /// </summary>
    Task<ServiceResult<PagedResult<ServiceCategoryIndexViewModel>>> GetServiceCategoriesAsync(int departmentId, string searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// دریافت جزئیات کامل یک دسته‌بندی خدمات.
    /// </summary>
    Task<ServiceResult<ServiceCategoryDetailsViewModel>> GetServiceCategoryDetailsAsync(int serviceCategoryId);

    /// <summary>
    /// دریافت اطلاعات یک دسته‌بندی خدمات برای فرم ویرایش.
    /// </summary>
    Task<ServiceResult<ServiceCategoryCreateEditViewModel>> GetServiceCategoryForEditAsync(int serviceCategoryId);

    /// <summary>
    /// ایجاد یک دسته‌بندی خدمات جدید.
    /// </summary>
    Task<ServiceResult> CreateServiceCategoryAsync(ServiceCategoryCreateEditViewModel model);

    /// <summary>
    /// به‌روزرسانی یک دسته‌بندی خدمات موجود.
    /// </summary>
    Task<ServiceResult> UpdateServiceCategoryAsync(ServiceCategoryCreateEditViewModel model);

    /// <summary>
    /// حذف نرم یک دسته‌بندی خدمات.
    /// </summary>
    Task<ServiceResult> SoftDeleteServiceCategoryAsync(int serviceCategoryId);

    /// <summary>
    /// بازیابی یک دسته‌بندی خدمات حذف شده.
    /// </summary>
    Task<ServiceResult> RestoreServiceCategoryAsync(int serviceCategoryId);

    /// <summary>
    /// دریافت لیست دسته‌بندی‌های فعال یک دپارتمان برای لیست‌های کشویی.
    /// </summary>
    Task<ServiceResult<List<LookupItemViewModel>>> GetActiveServiceCategoriesForLookupAsync(int departmentId);

    /// <summary>
    /// دریافت لیست تمام دسته‌بندی‌های خدمات (Medical Environment).
    /// </summary>
    Task<ServiceResult<PagedResult<ServiceCategoryIndexViewModel>>> GetAllServiceCategoriesAsync(string searchTerm, int pageNumber, int pageSize);

    #endregion

    #region Service Management (مدیریت خدمات)

    /// <summary>
    /// دریافت لیست خدمات یک دسته‌بندی خاص.
    /// </summary>
    Task<ServiceResult<PagedResult<ServiceIndexViewModel>>> GetServicesAsync(int serviceCategoryId, string searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// دریافت جزئیات کامل یک خدمت.
    /// </summary>
    Task<ServiceResult<ServiceDetailsViewModel>> GetServiceDetailsAsync(int serviceId);

    /// <summary>
    /// دریافت اطلاعات یک خدمت برای فرم ویرایش.
    /// </summary>
    Task<ServiceResult<ServiceCreateEditViewModel>> GetServiceForEditAsync(int serviceId);

    /// <summary>
    /// ایجاد یک خدمت جدید.
    /// </summary>
    Task<ServiceResult> CreateServiceAsync(ServiceCreateEditViewModel model);

    /// <summary>
    /// به‌روزرسانی یک خدمت موجود.
    /// </summary>
    Task<ServiceResult> UpdateServiceAsync(ServiceCreateEditViewModel model);

    /// <summary>
    /// حذف نرم یک خدمت.
    /// </summary>
    Task<ServiceResult> SoftDeleteServiceAsync(int serviceId);

    /// <summary>
    /// بازیابی یک خدمت حذف شده.
    /// </summary>
    Task<ServiceResult> RestoreServiceAsync(int serviceId);

    /// <summary>
    /// دریافت لیست خدمات فعال یک دسته‌بندی برای لیست‌های کشویی.
    /// </summary>
        Task<ServiceResult<List<LookupItemViewModel>>> GetActiveServicesForLookupAsync(int serviceCategoryId = 0);

    /// <summary>
    /// بررسی تکراری بودن کد خدمت - Medical Environment
    /// </summary>
    Task<bool> IsServiceCodeDuplicateAsync(string serviceCode, int? serviceCategoryId = null, int? excludeServiceId = null);

    #endregion

    #region ServiceComponents Management (مدیریت اجزای خدمات)

    /// <summary>
    /// دریافت اجزای یک خدمت
    /// </summary>
    Task<ServiceResult<List<ServiceComponentViewModel>>> GetServiceComponentsAsync(int serviceId);

    /// <summary>
    /// دریافت جزئیات یک جزء خدمت
    /// </summary>
    Task<ServiceResult<ServiceComponentDetailsViewModel>> GetServiceComponentDetailsAsync(int serviceComponentId);

    /// <summary>
    /// دریافت اطلاعات یک جزء خدمت برای ویرایش
    /// </summary>
    Task<ServiceResult<ServiceComponentCreateEditViewModel>> GetServiceComponentForEditAsync(int serviceComponentId);

    /// <summary>
    /// ایجاد جزء جدید برای خدمت
    /// </summary>
    Task<ServiceResult> CreateServiceComponentAsync(ServiceComponentCreateEditViewModel model);

    /// <summary>
    /// به‌روزرسانی جزء خدمت
    /// </summary>
    Task<ServiceResult> UpdateServiceComponentAsync(ServiceComponentCreateEditViewModel model);

    /// <summary>
    /// حذف نرم جزء خدمت
    /// </summary>
    Task<ServiceResult> SoftDeleteServiceComponentAsync(int serviceComponentId);

    /// <summary>
    /// بازیابی جزء خدمت حذف شده
    /// </summary>
    Task<ServiceResult> RestoreServiceComponentAsync(int serviceComponentId);

    /// <summary>
    /// بررسی وضعیت اجزای یک خدمت
    /// </summary>
    Task<ServiceResult<ServiceComponentsStatusViewModel>> GetServiceComponentsStatusAsync(int serviceId);

    #endregion
}