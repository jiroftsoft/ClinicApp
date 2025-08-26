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
    /// <param name="model">مدل حاوی اطلاعات صلاحیت.</param>
    /// <returns>نتیجه عملیات اعطا صلاحیت.</returns>
    Task<ServiceResult> GrantServiceCategoryToDoctorAsync(DoctorServiceCategoryViewModel model);

    /// <summary>
    /// لغو صلاحیت ارائه یک دسته‌بندی خدمات از یک پزشک.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات.</param>
    /// <returns>نتیجه عملیات لغو صلاحیت.</returns>
    Task<ServiceResult> RevokeServiceCategoryFromDoctorAsync(int doctorId, int serviceCategoryId);

    /// <summary>
    /// به‌روزرسانی اطلاعات صلاحیت پزشک در ارائه یک دسته‌بندی خدمات.
    /// </summary>
    /// <param name="model">مدل حاوی اطلاعات به‌روز شده صلاحیت.</param>
    /// <returns>نتیجه عملیات به‌روزرسانی.</returns>
    Task<ServiceResult> UpdateDoctorServiceCategoryPermissionAsync(DoctorServiceCategoryViewModel model);

    #endregion
}