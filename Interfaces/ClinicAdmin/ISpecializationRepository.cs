using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;

namespace ClinicApp.Interfaces.ClinicAdmin;

/// <summary>
/// اینترفیس برای عملیات مدیریت تخصص‌ها در سیستم کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت کامل تخصص‌های پزشکی
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete)
/// 3. مدیریت ردیابی (Audit Trail)
/// 4. پشتیبانی از ترتیب نمایش (DisplayOrder)
/// </summary>
public interface ISpecializationRepository
{
    #region Core CRUD Operations

    /// <summary>
    /// دریافت تخصص بر اساس شناسه
    /// </summary>
    /// <param name="specializationId">شناسه تخصص</param>
    /// <returns>تخصص مورد نظر</returns>
    Task<Specialization> GetByIdAsync(int specializationId);

    /// <summary>
    /// دریافت تمام تخصص‌های فعال
    /// </summary>
    /// <returns>لیست تخصص‌های فعال مرتب شده بر اساس DisplayOrder</returns>
    Task<List<Specialization>> GetActiveSpecializationsAsync();

    /// <summary>
    /// دریافت تمام تخصص‌ها (فعال و غیرفعال)
    /// </summary>
    /// <returns>لیست تمام تخصص‌ها مرتب شده بر اساس DisplayOrder</returns>
    Task<List<Specialization>> GetAllSpecializationsAsync();

    /// <summary>
    /// بررسی وجود تخصص
    /// </summary>
    /// <param name="specializationId">شناسه تخصص</param>
    /// <returns>درست اگر تخصص وجود داشته باشد</returns>
    Task<bool> DoesSpecializationExistAsync(int specializationId);

    /// <summary>
    /// بررسی وجود نام تخصص
    /// </summary>
    /// <param name="name">نام تخصص</param>
    /// <param name="excludeSpecializationId">شناسه تخصص برای استثنا (در حالت ویرایش)</param>
    /// <returns>درست اگر نام تخصص وجود داشته باشد</returns>
    Task<bool> DoesSpecializationNameExistAsync(string name, int? excludeSpecializationId = null);

    /// <summary>
    /// دریافت تعداد پزشکان فعال مرتبط با تخصص
    /// </summary>
    Task<int> GetActiveDoctorsCountAsync(int specializationId);

    /// <summary>
    /// افزودن تخصص جدید
    /// </summary>
    /// <param name="specialization">تخصص جدید</param>
    /// <returns>تخصص افزوده شده</returns>
    Task<Specialization> AddAsync(Specialization specialization);

    /// <summary>
    /// به‌روزرسانی تخصص موجود
    /// </summary>
    /// <param name="specialization">تخصص به‌روز شده</param>
    /// <returns>تخصص به‌روز شده</returns>
    Task<Specialization> UpdateAsync(Specialization specialization);

    /// <summary>
    /// حذف نرم تخصص
    /// </summary>
    /// <param name="specializationId">شناسه تخصص</param>
    /// <param name="deletedByUserId">شناسه کاربر حذف کننده</param>
    /// <returns>درست اگر حذف موفقیت‌آمیز باشد</returns>
    Task<bool> SoftDeleteAsync(int specializationId, string deletedByUserId);

    /// <summary>
    /// بازیابی تخصص حذف شده
    /// </summary>
    /// <param name="specializationId">شناسه تخصص</param>
    /// <param name="restoredByUserId">شناسه کاربر بازیابی کننده</param>
    /// <returns>درست اگر بازیابی موفقیت‌آمیز باشد</returns>
    Task<bool> RestoreAsync(int specializationId, string restoredByUserId);

    #endregion

    #region Doctor-Specialization Relationship

    /// <summary>
    /// دریافت تخصص‌های یک پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>لیست تخصص‌های پزشک</returns>
    Task<List<Specialization>> GetDoctorSpecializationsAsync(int doctorId);

    /// <summary>
    /// به‌روزرسانی تخصص‌های یک پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="specializationIds">لیست شناسه‌های تخصص‌ها</param>
    /// <returns>درست اگر به‌روزرسانی موفقیت‌آمیز باشد</returns>
    Task<bool> UpdateDoctorSpecializationsAsync(int doctorId, List<int> specializationIds);

    /// <summary>
    /// دریافت تخصص‌ها بر اساس لیست شناسه‌ها
    /// </summary>
    /// <param name="specializationIds">لیست شناسه‌های تخصص‌ها</param>
    /// <returns>لیست تخصص‌ها</returns>
    Task<List<Specialization>> GetSpecializationsByIdsAsync(List<int> specializationIds);

    #endregion
}
