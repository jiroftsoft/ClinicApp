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

    #endregion
}