using System.Collections.Generic;
using ClinicApp.Models.Entities;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.ClinicAdmin
{
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
    public interface IDoctorAssignmentRepository
    {
        #region Assignment Management (مدیریت انتسابات)

        /// <summary>
        /// دریافت تمام انتسابات یک پزشک (دپارتمان‌ها و سرفصل‌های خدماتی).
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <returns>نتیجه حاوی تمام انتسابات پزشک.</returns>
        Task<DoctorAssignments> GetDoctorAssignmentsAsync(int doctorId);

        #endregion
    }
}

/// <summary>
/// مدل اطلاعات انتسابات پزشک برای مدیریت کامل انتسابات
/// </summary>
public class DoctorAssignments
{
    public int DoctorId { get; set; }
    public List<DoctorDepartment> DoctorDepartments { get; set; } = new List<DoctorDepartment>();
    public List<DoctorServiceCategory> DoctorServiceCategories { get; set; } = new List<DoctorServiceCategory>();
}

/// <summary>
/// مدل اطلاعات وابستگی‌های پزشک برای بررسی امکان حذف
/// </summary>
public class DoctorDependencyInfo
{
    public int DoctorId { get; set; }
    public bool HasActiveAppointments { get; set; }
    public int ActiveAppointmentsCount { get; set; }
    public bool HasActiveReceptions { get; set; }
    public int ActiveReceptionsCount { get; set; }
}