using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// اینترفیس برای سرویس مدیریت خدمات در ماژول پذیرش
    /// طراحی شده برای محیط پروداکشن با ترافیک بالا
    /// </summary>
    public interface IReceptionServiceManagementService
    {
        /// <summary>
        /// دریافت دسته‌بندی‌های خدمات
        /// </summary>
        /// <returns>لیست دسته‌بندی‌های خدمات</returns>
        Task<ServiceResult<List<ServiceCategoryLookupViewModel>>> GetServiceCategoriesAsync();

        /// <summary>
        /// دریافت تمام خدمات
        /// </summary>
        /// <returns>لیست تمام خدمات</returns>
        Task<ServiceResult<List<ServiceLookupViewModel>>> GetAllServicesAsync();

        /// <summary>
        /// دریافت خدمات بر اساس دسته‌بندی
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <returns>لیست خدمات دسته‌بندی</returns>
        Task<ServiceResult<List<ServiceLookupViewModel>>> GetServicesByCategoryAsync(int categoryId);

        /// <summary>
        /// جستجوی خدمات
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست خدمات یافت شده</returns>
        Task<ServiceResult<List<ServiceLookupViewModel>>> SearchServicesAsync(string searchTerm, int? serviceCategoryId = null, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// جستجوی خدمات (overload)
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی</param>
        /// <param name="specializationId">شناسه تخصص</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست خدمات یافت شده</returns>
        Task<ServiceResult<List<ServiceLookupViewModel>>> SearchServicesAsync(string searchTerm, int? serviceCategoryId, int? specializationId, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// جستجوی خدمات (overload)
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی</param>
        /// <param name="specializationId">شناسه تخصص</param>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست خدمات یافت شده</returns>
        Task<ServiceResult<List<ServiceLookupViewModel>>> SearchServicesAsync(string searchTerm, int? serviceCategoryId, int? specializationId, int? doctorId, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// محاسبه هزینه خدمات
        /// </summary>
        /// <param name="request">درخواست محاسبه</param>
        /// <returns>نتیجه محاسبه هزینه</returns>
        Task<ServiceResult<ViewModels.Reception.ServiceCalculationResult>> CalculateServiceCostsAsync(ServiceCalculationRequest request);

        /// <summary>
        /// دریافت جزئیات خدمات
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>جزئیات خدمت</returns>
        Task<ServiceResult<ServiceDetailsViewModel>> GetServiceDetailsAsync(int serviceId);

        /// <summary>
        /// اعتبارسنجی انتخاب خدمات
        /// </summary>
        /// <param name="serviceIds">شناسه‌های خدمات انتخاب شده</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult<bool>> ValidateServiceSelectionAsync(List<int> serviceIds);
    }
}