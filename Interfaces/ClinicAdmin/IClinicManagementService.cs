using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.ViewModels;

namespace ClinicApp.Interfaces.ClinicAdmin;

/// <summary>
/// سرویس تخصصی برای مدیریت کامل کلینیک‌ها در سیستم.
/// این سرویس مسئولیت تمام عملیات CRUD، جستجو و بازیابی اطلاعات مربوط به موجودیت کلینیک را بر عهده دارد.
/// </summary>
public interface IClinicManagementService
{
    /// <summary>
    /// دریافت لیست کلینیک‌ها با قابلیت صفحه‌بندی و جستجو.
    /// </summary>
    /// <param name="searchTerm">عبارت برای جستجو در نام یا آدرس کلینیک.</param>
    /// <param name="pageNumber">شماره صفحه.</param>
    /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه.</param>
    /// <returns>یک نتیجه صفحه‌بندی شده از کلینیک‌ها.</returns>
    Task<ServiceResult<PagedResult<ClinicIndexViewModel>>> GetClinicsAsync(string searchTerm, int pageNumber, int pageSize);

    /// <summary>
    /// دریافت جزئیات کامل یک کلینیک برای نمایش.
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک.</param>
    /// <returns>نتیجه حاوی جزئیات کلینیک.</returns>
    Task<ServiceResult<ClinicDetailsViewModel>> GetClinicDetailsAsync(int clinicId);

    /// <summary>
    /// دریافت اطلاعات یک کلینیک برای پر کردن فرم ویرایش.
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک.</param>
    /// <returns>نتیجه حاوی اطلاعات کلینیک برای ویرایش.</returns>
    Task<ServiceResult<ClinicCreateEditViewModel>> GetClinicForEditAsync(int clinicId);

    /// <summary>
    /// ایجاد یک کلینیک جدید بر اساس اطلاعات ورودی.
    /// </summary>
    /// <param name="model">مدل حاوی اطلاعات کلینیک جدید.</param>
    /// <returns>نتیجه عملیات ایجاد.</returns>
    Task<ServiceResult> CreateClinicAsync(ClinicCreateEditViewModel model);

    /// <summary>
    /// به‌روزرسانی اطلاعات یک کلینیک موجود.
    /// </summary>
    /// <param name="model">مدل حاوی اطلاعات به‌روز شده کلینیک.</param>
    /// <returns>نتیجه عملیات به‌روزرسانی.</returns>
    Task<ServiceResult> UpdateClinicAsync(ClinicCreateEditViewModel model);

    /// <summary>
    /// حذف نرم یک کلینیک با بررسی قوانین کسب‌وکار.
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک برای حذف.</param>
    /// <returns>نتیجه عملیات حذف.</returns>
    Task<ServiceResult> SoftDeleteClinicAsync(int clinicId);

    /// <summary>
    /// بازیابی یک کلینیک حذف شده.
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک برای بازیابی.</param>
    /// <returns>نتیجه عملیات بازیابی.</returns>
    Task<ServiceResult> RestoreClinicAsync(int clinicId);

    /// <summary>
    /// دریافت لیست کلینیک‌های فعال برای استفاده در لیست‌های کشویی (Dropdowns).
    /// </summary>
    /// <returns>لیستی از کلینیک‌های فعال.</returns>
    Task<ServiceResult<List<LookupItemViewModel>>> GetActiveClinicsForLookupAsync();

    /// <summary>
    /// 🏥 MEDICAL: دریافت اطلاعات وابستگی‌های کلینیک برای اعتبارسنجی حذف
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک</param>
    /// <returns>اطلاعات وابستگی‌های کلینیک</returns>
    Task<ServiceResult<ClinicDependencyInfo>> GetClinicDependencyInfoAsync(int clinicId);
}