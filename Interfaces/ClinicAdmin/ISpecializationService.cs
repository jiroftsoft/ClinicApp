using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;

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
public interface ISpecializationService
{
    #region Core Operations

    /// <summary>
    /// دریافت لیست تخصص‌های فعال برای نمایش در Dropdown
    /// </summary>
    /// <returns>نتیجه حاوی لیست تخصص‌های فعال</returns>
    Task<ServiceResult<List<Specialization>>> GetActiveSpecializationsAsync();

    /// <summary>
    /// دریافت لیست تمام تخصص‌ها برای مدیریت
    /// </summary>
    /// <returns>نتیجه حاوی لیست تمام تخصص‌ها</returns>
    Task<ServiceResult<List<Specialization>>> GetAllSpecializationsAsync();

    /// <summary>
    /// دریافت تخصص بر اساس شناسه
    /// </summary>
    /// <param name="specializationId">شناسه تخصص</param>
    /// <returns>نتیجه حاوی تخصص مورد نظر</returns>
    Task<ServiceResult<Specialization>> GetSpecializationByIdAsync(int specializationId);

    /// <summary>
    /// ایجاد تخصص جدید
    /// </summary>
    /// <param name="specialization">تخصص جدید</param>
    /// <returns>نتیجه عملیات ایجاد</returns>
    Task<ServiceResult<Specialization>> CreateSpecializationAsync(Specialization specialization);

    /// <summary>
    /// به‌روزرسانی تخصص موجود
    /// </summary>
    /// <param name="specialization">تخصص به‌روز شده</param>
    /// <returns>نتیجه عملیات به‌روزرسانی</returns>
    Task<ServiceResult<Specialization>> UpdateSpecializationAsync(Specialization specialization);

    /// <summary>
    /// حذف نرم تخصص
    /// </summary>
    /// <param name="specializationId">شناسه تخصص</param>
    /// <returns>نتیجه عملیات حذف</returns>
    Task<ServiceResult> SoftDeleteSpecializationAsync(int specializationId);

    /// <summary>
    /// بازیابی تخصص حذف شده
    /// </summary>
    /// <param name="specializationId">شناسه تخصص</param>
    /// <returns>نتیجه عملیات بازیابی</returns>
    Task<ServiceResult> RestoreSpecializationAsync(int specializationId);

    #endregion

    #region Doctor-Specialization Relationship

    /// <summary>
    /// دریافت تخصص‌های یک پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>نتیجه حاوی لیست تخصص‌های پزشک</returns>
    Task<ServiceResult<List<Specialization>>> GetDoctorSpecializationsAsync(int doctorId);

    /// <summary>
    /// به‌روزرسانی تخصص‌های یک پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="specializationIds">لیست شناسه‌های تخصص‌ها</param>
    /// <returns>نتیجه عملیات به‌روزرسانی</returns>
    Task<ServiceResult> UpdateDoctorSpecializationsAsync(int doctorId, List<int> specializationIds);

    /// <summary>
    /// دریافت تخصص‌ها بر اساس لیست شناسه‌ها
    /// </summary>
    /// <param name="specializationIds">لیست شناسه‌های تخصص‌ها</param>
    /// <returns>نتیجه حاوی لیست تخصص‌ها</returns>
    Task<ServiceResult<List<Specialization>>> GetSpecializationsByIdsAsync(List<int> specializationIds);

    #endregion

    #region Validation

    /// <summary>
    /// بررسی وجود نام تخصص
    /// </summary>
    /// <param name="name">نام تخصص</param>
    /// <param name="excludeSpecializationId">شناسه تخصص برای استثنا (در حالت ویرایش)</param>
    /// <returns>نتیجه بررسی</returns>
    Task<ServiceResult<bool>> DoesSpecializationNameExistAsync(string name, int? excludeSpecializationId = null);

    /// <summary>
    /// دریافت تعداد پزشکان فعال مرتبط با تخصص
    /// </summary>
    /// <param name="specializationId">شناسه تخصص</param>
    /// <returns>نتیجه حاوی تعداد پزشکان فعال</returns>
    Task<ServiceResult<int>> GetActiveDoctorsCountAsync(int specializationId);

    #endregion
}
