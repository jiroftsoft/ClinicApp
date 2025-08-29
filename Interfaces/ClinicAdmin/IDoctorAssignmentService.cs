using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin;

/// <summary>
/// اینترفیس تخصصی برای مدیریت انتسابات پزشکان در سیستم کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. تمرکز صرف بر مدیریت کامل انتسابات پزشکان
/// 2. رعایت استانداردهای پزشکی ایران در مدیریت انتسابات
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
/// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
/// 6. عملیات ترکیبی و انتقال پزشکان بین دپارتمان‌ها
/// 
/// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
/// </summary>
public interface IDoctorAssignmentService
{
    #region Assignment Management (مدیریت انتسابات)

    /// <summary>
    /// به‌روزرسانی کامل انتسابات یک پزشک (دپارتمان‌ها و سرفصل‌های خدماتی).
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="assignments">مدل حاوی تمام انتسابات.</param>
    /// <returns>نتیجه عملیات به‌روزرسانی.</returns>
    Task<ServiceResult> UpdateDoctorAssignmentsAsync(int doctorId, DoctorAssignmentsViewModel assignments);

    /// <summary>
    /// دریافت تمام انتسابات یک پزشک (دپارتمان‌ها و سرفصل‌های خدماتی).
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <returns>نتیجه حاوی تمام انتسابات پزشک.</returns>
    Task<ServiceResult<DoctorAssignmentsViewModel>> GetDoctorAssignmentsAsync(int doctorId);

    /// <summary>
    /// انتساب همزمان پزشک به دپارتمان و سرفصل‌های خدماتی مرتبط.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="departmentId">شناسه دپارتمان.</param>
    /// <param name="serviceCategoryIds">لیست شناسه‌های سرفصل‌های خدماتی.</param>
    /// <returns>نتیجه عملیات انتساب.</returns>
    Task<ServiceResult> AssignDoctorToDepartmentWithServicesAsync(int doctorId, int departmentId, List<int> serviceCategoryIds);

    /// <summary>
    /// انتقال پزشک بین دپارتمان‌ها با حفظ صلاحیت‌های خدماتی.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="fromDepartmentId">شناسه دپارتمان مبدا.</param>
    /// <param name="toDepartmentId">شناسه دپارتمان مقصد.</param>
    /// <param name="preserveServiceCategories">آیا صلاحیت‌های خدماتی حفظ شوند.</param>
    /// <returns>نتیجه عملیات انتقال.</returns>
    Task<ServiceResult> TransferDoctorBetweenDepartmentsAsync(int doctorId, int fromDepartmentId, int toDepartmentId, bool preserveServiceCategories = true);

    /// <summary>
    /// حذف کامل تمام انتسابات یک پزشک.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <returns>نتیجه عملیات حذف.</returns>
    Task<ServiceResult> RemoveAllDoctorAssignmentsAsync(int doctorId);

    #endregion
}