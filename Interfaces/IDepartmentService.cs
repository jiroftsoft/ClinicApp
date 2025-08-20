using ClinicApp.Helpers;
using ClinicApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces;

/// <summary>
/// سرویس مدیریت دپارتمان‌ها برای سیستم‌های پزشکی
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    /// ایجاد یک دپارتمان جدید
    /// </summary>
    Task<ServiceResult<int>> CreateDepartmentAsync(DepartmentCreateEditViewModel model);

    /// <summary>
    /// به‌روزرسانی اطلاعات دپارتمان
    /// </summary>
    Task<ServiceResult> UpdateDepartmentAsync(DepartmentCreateEditViewModel model);

    /// <summary>
    /// حذف نرم یک دپارتمان
    /// </summary>
    Task<ServiceResult> DeleteDepartmentAsync(int departmentId);

    /// <summary>
    /// دریافت اطلاعات دپارتمان برای ویرایش
    /// </summary>
    Task<ServiceResult<DepartmentCreateEditViewModel>> GetDepartmentForEditAsync(int departmentId);

    /// <summary>
    /// دریافت جزئیات کامل دپارتمان
    /// </summary>
    Task<ServiceResult<DepartmentDetailsViewModel>> GetDepartmentDetailsAsync(int departmentId);

    /// <summary>
    /// جستجو و صفحه‌بندی دپارتمان‌ها
    /// </summary>
    Task<ServiceResult<PagedResult<DepartmentIndexViewModel>>> SearchDepartmentsAsync(string searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// دریافت لیست دپارتمان‌های فعال برای استفاده در کنترل‌های انتخاب
    /// </summary>
    /// <returns>لیست دپارتمان‌های فعال</returns>
    Task<IEnumerable<DepartmentSelectItem>> GetActiveDepartmentsAsync();
    /// <summary>
    /// بررسی وجود دپارتمان فعال با شناسه مشخص
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>آیا دپارتمان فعال وجود دارد؟</returns>
    Task<bool> IsActiveDepartmentExistsAsync(int departmentId);
    /// <summary>
    /// دریافت لیست دپارتمان‌های فعال برای یک کلینیک خاص
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک</param>
    /// <returns>لیست دپارتمان‌های فعال برای کلینیک مورد نظر</returns>
    Task<IEnumerable<DepartmentSelectItem>> GetActiveDepartmentsByClinicAsync(int clinicId);
}
/// <summary>
/// مدل ساده برای انتخاب دپارتمان‌ها در کنترل‌های انتخاب
/// </summary>
public class DepartmentSelectItem
{
    public int DepartmentId { get; set; }
    public string Name { get; set; }
    public int ClinicId { get; set; }
    public string ClinicName { get; set; }
}