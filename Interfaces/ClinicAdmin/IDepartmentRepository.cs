using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Interfaces.ClinicAdmin;

/// <summary>
/// اینترفیس تخصصی برای عملیات داده مربوط به موجودیت دپارتمان.
/// این کلاس تمام جزئیات دسترسی به پایگاه داده را از لایه سرویس پنهان می‌کند.
/// </summary>
public interface IDepartmentRepository
{
    /// <summary>
    /// دریافت لیست دپارتمان‌های یک کلینیک خاص با قابلیت جستجو.
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک مادر.</param>
    /// <param name="searchTerm">عبارت برای جستجو در نام دپارتمان.</param>
    /// <returns>لیستی از دپارتمان‌های یافت شده.</returns>
    Task<List<Department>> GetDepartmentsAsync(int clinicId, string searchTerm);

    /// <summary>
    /// دریافت یک دپارتمان با شناسه آن به همراه اطلاعات کلینیک و کاربران مرتبط.
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان.</param>
    /// <returns>موجودیت دپارتمان یافت شده یا null.</returns>
    Task<Department> GetByIdAsync(int departmentId);

    /// <summary>
    /// بررسی وجود دپارتمان با نام تکراری در **یک کلینیک مشخص**.
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک برای محدود کردن جستجو.</param>
    /// <param name="name">نام دپارتمان برای بررسی.</param>
    /// <param name="excludeDepartmentId">شناسه دپارتمانی که باید از بررسی نادیده گرفته شود (برای سناریوی ویرایش).</param>
    /// <returns>True اگر نام تکراری باشد.</returns>
    Task<bool> DoesDepartmentExistAsync(int clinicId, string name, int? excludeDepartmentId = null);

    /// <summary>
    /// افزودن یک دپارتمان جدید به زمینه کاری (Context). (عملیات همگام)
    /// </summary>
    /// <param name="department">موجودیت دپارتمان برای افزودن.</param>
    void Add(Department department);

    /// <summary>
    /// علامت‌گذاری یک دپارتمان به عنوان "ویرایش شده". (عملیات همگام)
    /// </summary>
    /// <param name="department">موجودیت دپارتمان برای ویرایش.</param>
    void Update(Department department);

    /// <summary>
    /// علامت‌گذاری یک دپارتمان برای حذف. (عملیات همگام)
    /// </summary>
    /// <param name="department">موجودیت دپارتمان برای حذف.</param>
    void Delete(Department department);

    /// <summary>
    /// دریافت لیست تمام دپارتمان‌های فعال متعلق به یک کلینیک خاص.
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک.</param>
    /// <returns>لیستی از دپارتمان‌های فعال.</returns>
    Task<List<Department>> GetActiveDepartmentsAsync(int clinicId);

    /// <summary>
    /// ذخیره تمام تغییرات در صف در پایگاه داده.
    /// </summary>
    Task SaveChangesAsync();

    /// <summary>
    /// دریافت لیست تمام دپارتمان‌های فعال برای استفاده در لیست‌های کشویی.
    /// </summary>
    /// <returns>لیستی از تمام دپارتمان‌های فعال.</returns>
    Task<List<Department>> GetAllActiveDepartmentsAsync();
}