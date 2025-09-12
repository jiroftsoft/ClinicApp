using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.ViewModels;
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
public interface IDoctorCrudService
{
    #region Core CRUD Operations (عملیات اصلی CRUD)

    /// <summary>
    /// دریافت لیست پزشکان با قابلیت صفحه‌بندی و جستجو.
    /// </summary>
    /// <param name="filter">فیلترهای جستجو شامل clinicId، departmentId، searchTerm، pageNumber، pageSize</param>
    /// <returns>یک نتیجه صفحه‌بندی شده از پزشکان.</returns>
    Task<ServiceResult<PagedResult<DoctorIndexViewModel>>> GetDoctorsAsync(DoctorSearchViewModel filter);

    /// <summary>
    /// دریافت جزئیات کامل یک پزشک برای نمایش.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <returns>نتیجه حاوی جزئیات پزشک.</returns>
    Task<ServiceResult<DoctorDetailsViewModel>> GetDoctorDetailsAsync(int doctorId);

    /// <summary>
    /// دریافت اطلاعات یک پزشک برای پر کردن فرم ویرایش.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <returns>نتیجه حاوی اطلاعات پزشک برای ویرایش.</returns>
    Task<ServiceResult<DoctorCreateEditViewModel>> GetDoctorForEditAsync(int doctorId);

    /// <summary>
    /// دریافت لیست تخصص‌های فعال برای نمایش در فرم‌ها.
    /// </summary>
    /// <returns>نتیجه حاوی لیست تخصص‌های فعال.</returns>
    Task<ServiceResult<List<Specialization>>> GetActiveSpecializationsAsync();

    /// <summary>
    /// ایجاد یک پزشک جدید بر اساس اطلاعات ورودی.
    /// </summary>
    /// <param name="model">مدل حاوی اطلاعات پزشک جدید.</param>
    /// <returns>نتیجه عملیات ایجاد.</returns>
    Task<ServiceResult<Doctor>> CreateDoctorAsync(DoctorCreateEditViewModel model);

    /// <summary>
    /// به‌روزرسانی اطلاعات یک پزشک موجود.
    /// </summary>
    /// <param name="model">مدل حاوی اطلاعات به‌روز شده پزشک.</param>
    /// <returns>نتیجه عملیات به‌روزرسانی.</returns>
    Task<ServiceResult<Doctor>> UpdateDoctorAsync(DoctorCreateEditViewModel model);

    /// <summary>
    /// حذف نرم یک پزشک با بررسی قوانین کسب‌وکار.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک برای حذف.</param>
    /// <returns>نتیجه عملیات حذف.</returns>
    Task<ServiceResult> SoftDeleteDoctorAsync(int doctorId);

    /// <summary>
    /// بازیابی یک پزشک حذف شده.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک برای بازیابی.</param>
    /// <returns>نتیجه عملیات بازیابی.</returns>
    Task<ServiceResult> RestoreDoctorAsync(int doctorId);

    /// <summary>
    /// دریافت پزشک بر اساس کد ملی
    /// </summary>
    /// <param name="nationalCode">کد ملی پزشک</param>
    /// <returns>نتیجه حاوی اطلاعات پزشک</returns>
    Task<ServiceResult<Doctor>> GetDoctorByNationalCodeAsync(string nationalCode);

    /// <summary>
    /// دریافت پزشک بر اساس کد نظام پزشکی
    /// </summary>
    /// <param name="medicalCouncilCode">کد نظام پزشکی</param>
    /// <returns>نتیجه حاوی اطلاعات پزشک</returns>
    Task<ServiceResult<Doctor>> GetDoctorByMedicalCouncilCodeAsync(string medicalCouncilCode);

    /// <summary>
    /// فعال کردن یک پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>نتیجه عملیات فعال‌سازی</returns>
    Task<ServiceResult> ActivateDoctorAsync(int doctorId);

    /// <summary>
    /// غیرفعال کردن یک پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>نتیجه عملیات غیرفعال‌سازی</returns>
    Task<ServiceResult> DeactivateDoctorAsync(int doctorId);

    #endregion
}