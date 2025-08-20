using ClinicApp.Helpers;
using ClinicApp.ViewModels;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// سرویس مدیریت کلینیک‌ها برای سیستم‌های پزشکی
    /// </summary>
    public interface IClinicService
    {
        /// <summary>
        /// ایجاد یک کلینیک جدید
        /// </summary>
        Task<ServiceResult<int>> CreateClinicAsync(ClinicCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی اطلاعات کلینیک
        /// </summary>
        Task<ServiceResult> UpdateClinicAsync(ClinicCreateEditViewModel model);

        /// <summary>
        /// حذف نرم یک کلینیک
        /// </summary>
        Task<ServiceResult> DeleteClinicAsync(int clinicId);

        /// <summary>
        /// دریافت اطلاعات کلینیک برای ویرایش
        /// </summary>
        Task<ServiceResult<ClinicCreateEditViewModel>> GetClinicForEditAsync(int clinicId);

        /// <summary>
        /// دریافت جزئیات کامل کلینیک
        /// </summary>
        Task<ServiceResult<ClinicDetailsViewModel>> GetClinicDetailsAsync(int clinicId);

        /// <summary>
        /// جستجو و صفحه‌بندی کلینیک‌ها
        /// </summary>
        Task<ServiceResult<PagedResult<ClinicIndexViewModel>>> SearchClinicsAsync(string searchTerm, int pageNumber, int pageSize);
    }
}