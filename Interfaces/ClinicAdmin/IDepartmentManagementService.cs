using ClinicApp.Helpers;
using ClinicApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// سرویس تخصصی برای مدیریت کامل دپارتمان‌ها در سیستم.
    /// این سرویس مسئولیت تمام عملیات CRUD، جستجو و بازیابی اطلاعات مربوط به موجودیت دپارتمان را بر عهده دارد.
    /// </summary>
    public interface IDepartmentManagementService
    {
        /// <summary>
        /// دریافت لیست دپارتمان‌های یک کلینیک خاص با قابلیت صفحه‌بندی و جستجو.
        /// </summary>
        Task<ServiceResult<ClinicApp.Interfaces.PagedResult<DepartmentIndexViewModel>>> GetDepartmentsAsync(int clinicId, string searchTerm, int pageNumber, int pageSize);

        /// <summary>
        /// دریافت جزئیات کامل یک دپارتمان برای نمایش.
        /// </summary>
        Task<ServiceResult<DepartmentDetailsViewModel>> GetDepartmentDetailsAsync(int departmentId);

        /// <summary>
        /// دریافت اطلاعات یک دپارتمان برای پر کردن فرم ویرایش.
        /// </summary>
        Task<ServiceResult<DepartmentCreateEditViewModel>> GetDepartmentForEditAsync(int departmentId);

        /// <summary>
        /// ایجاد یک دپارتمان جدید. در صورت موفقیت، موجودیت ایجاد شده را برمی‌گرداند.
        /// </summary>
        Task<ServiceResult<Department>> CreateDepartmentAsync(DepartmentCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی اطلاعات یک دپارتمان موجود. در صورت موفقیت، موجودیت آپدیت شده را برمی‌گرداند.
        /// </summary>
        Task<ServiceResult<Department>> UpdateDepartmentAsync(DepartmentCreateEditViewModel model);

        /// <summary>
        /// حذف نرم یک دپارتمان با بررسی قوانین کسب‌وکار.
        /// </summary>
        Task<ServiceResult> SoftDeleteDepartmentAsync(int departmentId);

        /// <summary>
        /// بازیابی یک دپارتمان حذف شده.
        /// </summary>
        Task<ServiceResult> RestoreDepartmentAsync(int departmentId);

        /// <summary>
        /// دریافت لیست دپارتمان‌های فعال یک کلینیک برای استفاده در لیست‌های کشویی.
        /// </summary>
        Task<ServiceResult<List<LookupItemViewModel>>> GetActiveDepartmentsForLookupAsync(int clinicId);
    }
}