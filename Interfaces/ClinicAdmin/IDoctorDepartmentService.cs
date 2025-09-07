using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin;
/// <summary>
/// اینترفیس تخصصی برای مدیریت انتصاب پزشکان به دپارتمان‌ها در سیستم کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. تمرکز صرف بر مدیریت رابطه چند-به-چند پزشک-دپارتمان
/// 2. رعایت استانداردهای پزشکی ایران در مدیریت انتصاب‌ها
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
/// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
/// 
/// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
/// </summary>
public interface IDoctorDepartmentService
{
    #region Doctor-Department Management (مدیریت انتصاب پزشک به دپارتمان)

    /// <summary>
    /// دریافت لیست پزشکان فعال برای استفاده در لیست‌های کشویی (Dropdowns).
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک (اختیاری).</param>
    /// <param name="departmentId">شناسه دپارتمان (اختیاری).</param>
    /// <returns>لیستی از پزشکان فعال.</returns>
    Task<ServiceResult<List<LookupItemViewModel>>> GetActiveDoctorsForLookupAsync(int? clinicId, int? departmentId);

    /// <summary>
    /// دریافت لیست دپارتمان‌های مرتبط با یک پزشک.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="searchTerm">عبارت جستجو.</param>
    /// <param name="pageNumber">شماره صفحه.</param>
    /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه.</param>
    /// <returns>یک نتیجه صفحه‌بندی شده از دپارتمان‌های مرتبط با پزشک.</returns>
    Task<ServiceResult<PagedResult<DoctorDepartmentViewModel>>> GetDepartmentsForDoctorAsync(int doctorId, string searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// انتصاب یک پزشک به یک دپارتمان با مشخص کردن نقش و سایر جزئیات.
    /// </summary>
    /// <param name="model">مدل حاوی اطلاعات انتصاب پزشک به دپارتمان.</param>
    /// <returns>نتیجه عملیات انتصاب.</returns>
    Task<ServiceResult> AssignDoctorToDepartmentAsync(DoctorDepartmentViewModel model);

    /// <summary>
    /// لغو انتصاب یک پزشک از یک دپارتمان.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="departmentId">شناسه دپارتمان.</param>
    /// <returns>نتیجه عملیات لغو انتصاب.</returns>
    Task<ServiceResult> RevokeDoctorFromDepartmentAsync(int doctorId, int departmentId);

    /// <summary>
    /// به‌روزرسانی اطلاعات انتصاب پزشک به دپارتمان (نقش، وضعیت فعال/غیرفعال و ...).
    /// </summary>
    /// <param name="model">مدل حاوی اطلاعات به‌روز شده انتصاب.</param>
    /// <returns>نتیجه عملیات به‌روزرسانی.</returns>
    Task<ServiceResult> UpdateDoctorDepartmentAssignmentAsync(DoctorDepartmentViewModel model);

    /// <summary>
    /// دریافت لیست تمام دپارتمان‌های فعال برای استفاده در لیست‌های کشویی.
    /// </summary>
    /// <returns>لیستی از تمام دپارتمان‌های فعال.</returns>
    Task<ServiceResult<List<LookupItemViewModel>>> GetAllDepartmentsAsync();

    /// <summary>
    /// دریافت لیست دپارتمان‌ها به صورت SelectListItem برای استفاده در View
    /// </summary>
    /// <param name="addAllOption">آیا گزینه "همه" اضافه شود</param>
    /// <returns>لیست SelectListItem برای dropdown</returns>
    Task<ServiceResult<List<ClinicApp.ViewModels.DoctorManagementVM.SelectListItem>>> GetDepartmentsAsSelectListAsync(bool addAllOption = false);

    #endregion
}