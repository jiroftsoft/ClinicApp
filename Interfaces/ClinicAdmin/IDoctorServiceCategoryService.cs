using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin;

/// <summary>
/// اینترفیس تخصصی برای مدیریت صلاحیت‌های خدماتی پزشکان در سیستم کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. تمرکز صرف بر مدیریت رابطه چند-به-چند پزشک-دسته‌بندی خدمات
/// 2. رعایت استانداردهای پزشکی ایران در مدیریت صلاحیت‌ها
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
/// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
/// 
/// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
/// </summary>
public interface IDoctorServiceCategoryService
{
    #region Doctor-ServiceCategory Management (مدیریت انتصاب پزشک به سرفصل‌های خدماتی)

    /// <summary>
    /// دریافت لیست دسته‌بندی‌های خدمات مجاز برای یک پزشک.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="searchTerm">عبارت جستجو.</param>
    /// <param name="pageNumber">شماره صفحه.</param>
    /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه.</param>
    /// <returns>یک نتیجه صفحه‌بندی شده از دسته‌بندی‌های خدمات مجاز برای پزشک.</returns>
    Task<ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>> GetServiceCategoriesForDoctorAsync(int doctorId, string searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// اعطا کردن صلاحیت ارائه یک دسته‌بندی خدمات به یک پزشک.
    /// </summary>
    /// <param name="model">مدل حاوی اطلاعات اعطای صلاحیت.</param>
    /// <returns>نتیجه عملیات اعطای صلاحیت.</returns>
    Task<ServiceResult> GrantServiceCategoryToDoctorAsync(DoctorServiceCategoryViewModel model);

    /// <summary>
    /// لغو صلاحیت ارائه یک دسته‌بندی خدمات از یک پزشک.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات.</param>
    /// <returns>نتیجه عملیات لغو صلاحیت.</returns>
    Task<ServiceResult> RevokeServiceCategoryFromDoctorAsync(int doctorId, int serviceCategoryId);

    /// <summary>
    /// به‌روزرسانی اطلاعات صلاحیت ارائه دسته‌بندی خدمات توسط پزشک.
    /// </summary>
    /// <param name="model">مدل حاوی اطلاعات به‌روز شده صلاحیت.</param>
    /// <returns>نتیجه عملیات به‌روزرسانی.</returns>
    Task<ServiceResult> UpdateDoctorServiceCategoryAsync(DoctorServiceCategoryViewModel model);

    /// <summary>
    /// دریافت لیست تمام دسته‌بندی‌های خدمات فعال برای استفاده در لیست‌های کشویی.
    /// </summary>
    /// <returns>لیستی از تمام دسته‌بندی‌های خدمات فعال.</returns>
    Task<ServiceResult<List<LookupItemViewModel>>> GetAllServiceCategoriesAsync();

    /// <summary>
    /// دریافت لیست همه انتصابات پزشکان به سرفصل‌های خدماتی (برای فیلتر "همه پزشکان")
    /// </summary>
    /// <param name="searchTerm">عبارت جستجو</param>
    /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
    /// <param name="serviceCategoryId">شناسه سرفصل خدماتی (اختیاری)</param>
    /// <param name="isActive">وضعیت فعال (اختیاری)</param>
    /// <param name="pageNumber">شماره صفحه</param>
    /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
    /// <returns>نتیجه صفحه‌بندی شده از همه انتصابات پزشکان به سرفصل‌های خدماتی</returns>
    Task<ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>> GetAllDoctorServiceCategoriesAsync(string searchTerm, int? doctorId, int? serviceCategoryId, bool? isActive, int pageNumber, int pageSize);

    #endregion

    #region Department Management (مدیریت دپارتمان‌ها)

    /// <summary>
    /// دریافت دپارتمان‌های مرتبط با پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>لیست دپارتمان‌های مرتبط با پزشک</returns>
    Task<ServiceResult<List<LookupItemViewModel>>> GetDoctorDepartmentsAsync(int doctorId);

    /// <summary>
    /// دریافت سرفصل‌های خدماتی مرتبط با دپارتمان
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>لیست سرفصل‌های خدماتی مرتبط با دپارتمان</returns>
    Task<ServiceResult<List<LookupItemViewModel>>> GetServiceCategoriesByDepartmentAsync(int departmentId);

    #endregion

}