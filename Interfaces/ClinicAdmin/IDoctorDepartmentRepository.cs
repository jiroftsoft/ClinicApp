using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;

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
public interface IDoctorDepartmentRepository
{
    #region Doctor-Department Management (مدیریت انتصاب پزشک به دپارتمان)

    /// <summary>
    /// دریافت انتصاب پزشک به دپارتمان بر اساس شناسه‌ها
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>ارتباط پزشک-دپارتمان</returns>
    Task<DoctorDepartment> GetDoctorDepartmentAsync(int doctorId, int departmentId);

    /// <summary>
    /// دریافت انتصاب پزشک به دپارتمان همراه با جزئیات
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>ارتباط پزشک-دپارتمان همراه با جزئیات</returns>
    Task<DoctorDepartment> GetDoctorDepartmentWithDetailsAsync(int doctorId, int departmentId);

    /// <summary>
    /// دریافت لیست انتصابات پزشک به دپارتمان‌ها
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="searchTerm">عبارت جستجو</param>
    /// <param name="pageNumber">شماره صفحه</param>
    /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
    /// <returns>لیست انتصابات پزشک به دپارتمان‌ها</returns>
    Task<List<DoctorDepartment>> GetDoctorDepartmentsAsync(int doctorId, string searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// دریافت تعداد انتصابات پزشک به دپارتمان‌ها
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="searchTerm">عبارت جستجو</param>
    /// <returns>تعداد انتصابات پزشک به دپارتمان‌ها</returns>
    Task<int> GetDoctorDepartmentsCountAsync(int doctorId, string searchTerm);

    /// <summary>
    /// افزودن انتصاب پزشک به دپارتمان
    /// </summary>
    /// <param name="doctorDepartment">ارتباط پزشک-دپارتمان</param>
    /// <returns>ارتباط پزشک-دپارتمان افزوده شده</returns>
    Task<DoctorDepartment> AddDoctorDepartmentAsync(DoctorDepartment doctorDepartment);

    /// <summary>
    /// به‌روزرسانی انتصاب پزشک به دپارتمان
    /// </summary>
    /// <param name="doctorDepartment">ارتباط پزشک-دپارتمان به‌روز شده</param>
    /// <returns>ارتباط پزشک-دپارتمان به‌روز شده</returns>
    Task<DoctorDepartment> UpdateDoctorDepartmentAsync(DoctorDepartment doctorDepartment);

    /// <summary>
    /// حذف انتصاب پزشک از دپارتمان
    /// </summary>
    /// <param name="doctorDepartment">ارتباط پزشک-دپارتمان برای حذف</param>
    /// <returns>درست اگر حذف موفقیت‌آمیز باشد</returns>
    Task<bool> DeleteDoctorDepartmentAsync(DoctorDepartment doctorDepartment);

    /// <summary>
    /// بررسی وجود انتصاب پزشک به دپارتمان
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <param name="excludeId">شناسه برای استثنا (در حالت ویرایش)</param>
    /// <returns>درست اگر انتصاب وجود داشته باشد</returns>
    Task<bool> DoesDoctorDepartmentExistAsync(int doctorId, int departmentId, int? excludeId = null);

    /// <summary>
    /// دریافت لیست پزشکان فعال در یک دپارتمان برای استفاده در لیست‌های کشویی
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>لیست پزشکان فعال در دپارتمان</returns>
    Task<List<Doctor>> GetActiveDoctorsForDepartmentLookupAsync(int departmentId);

    /// <summary>
    /// ذخیره تمام تغییرات در انتظار به پایگاه داده
    /// </summary>
    Task SaveChangesAsync();

    #endregion
}