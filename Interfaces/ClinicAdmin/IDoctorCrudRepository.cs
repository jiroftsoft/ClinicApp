using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin;

/// <summary>
/// اینترفیس اصلی برای عملیات CRUD پزشکان در سیستم کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. تمرکز صرف بر عملیات اصلی ایجاد، خواندن، به‌روزرسانی و حذف
/// 2. رعایت استانداردهای پزشکی ایران در مدیریت پزشکان
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
/// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
/// 
/// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
/// </summary>
public interface IDoctorCrudRepository
{
    #region Core CRUD Operations (عملیات اصلی CRUD)

    /// <summary>
    /// دریافت پزشک بر اساس شناسه
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>پزشک مورد نظر</returns>
    Task<Doctor> GetByIdAsync(int doctorId);

    /// <summary>
    /// دریافت پزشک بر اساس شناسه همراه با تمام روابط برای نمایش جزئیات
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>پزشک مورد نظر همراه با تمام روابط</returns>
    Task<Doctor> GetByIdWithDetailsAsync(int doctorId);

    /// <summary>
    /// بررسی وجود پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>درست اگر پزشک وجود داشته باشد</returns>
    Task<bool> DoesDoctorExistAsync(int doctorId);

    /// <summary>
    /// بررسی وجود کد نظام پزشکی
    /// </summary>
    /// <param name="medicalCouncilCode">کد نظام پزشکی</param>
    /// <param name="excludeDoctorId">شناسه پزشک برای استثنا (در حالت ویرایش)</param>
    /// <returns>درست اگر کد نظام پزشکی وجود داشته باشد</returns>
    Task<bool> DoesMedicalCouncilCodeExistAsync(string medicalCouncilCode, int? excludeDoctorId = null);

    /// <summary>
    /// بررسی وجود کد ملی
    /// </summary>
    /// <param name="nationalCode">کد ملی</param>
    /// <param name="excludeDoctorId">شناسه پزشک برای استثنا (در حالت ویرایش)</param>
    /// <returns>درست اگر کد ملی وجود داشته باشد</returns>
    Task<bool> DoesNationalCodeExistAsync(string nationalCode, int? excludeDoctorId = null);

    /// <summary>
    /// افزودن پزشک جدید
    /// </summary>
    /// <param name="doctor">پزشک جدید</param>
    /// <returns>پزشک افزوده شده</returns>
    Task<Doctor> AddAsync(Doctor doctor);

    /// <summary>
    /// به‌روزرسانی پزشک موجود
    /// </summary>
    /// <param name="doctor">پزشک به‌روز شده</param>
    /// <returns>پزشک به‌روز شده</returns>
    Task<Doctor> UpdateAsync(Doctor doctor);

    /// <summary>
    /// حذف نرم پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="deletedByUserId">شناسه کاربر حذف کننده</param>
    /// <returns>درست اگر حذف موفقیت‌آمیز باشد</returns>
    Task<bool> SoftDeleteAsync(int doctorId, string deletedByUserId);

    /// <summary>
    /// بازیابی پزشک حذف شده
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="restoredByUserId">شناسه کاربر بازیابی کننده</param>
    /// <returns>درست اگر بازیابی موفقیت‌آمیز باشد</returns>
    Task<bool> RestoreAsync(int doctorId, string restoredByUserId);

    #endregion

    #region Lookup & Search (جستجو و لیست‌ها)

    /// <summary>
    /// جستجوی پزشکان بر اساس فیلترهای مختلف
    /// </summary>
    /// <param name="filter">فیلترهای جستجو</param>
    /// <returns>لیست پزشکان مطابق با فیلترها</returns>
    Task<List<Doctor>> SearchDoctorsAsync(DoctorSearchViewModel filter);

    /// <summary>
    /// دریافت تعداد پزشکان مطابق با فیلترهای جستجو
    /// </summary>
    /// <param name="filter">فیلترهای جستجو</param>
    /// <returns>تعداد پزشکان مطابق با فیلترها</returns>
    Task<int> GetDoctorsCountAsync(DoctorSearchViewModel filter);

    #endregion
}