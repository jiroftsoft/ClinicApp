using ClinicApp.Helpers;
using ClinicApp.ViewModels;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{
    public interface IDoctorService
    {
        /// <summary>
        /// ایجاد یک پزشک جدید و حساب کاربری مربوطه.
        /// </summary>
        Task<ServiceResult<int>> CreateDoctorAsync(DoctorCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی پروفایل یک پزشک موجود.
        /// </summary>
        Task<ServiceResult> UpdateDoctorAsync(DoctorCreateEditViewModel model);

        /// <summary>
        /// حذف نرم (Soft-delete) یک پزشک.
        /// </summary>
        Task<ServiceResult> DeleteDoctorAsync(int doctorId);

        /// <summary>
        /// بازیابی داده‌های یک پزشک برای پر کردن فرم ویرایش.
        /// </summary>
        Task<ServiceResult<DoctorCreateEditViewModel>> GetDoctorForEditAsync(int doctorId);

        /// <summary>
        /// بازیابی جزئیات کامل یک پزشک.
        /// </summary>
        Task<ServiceResult<DoctorDetailsViewModel>> GetDoctorDetailsAsync(int doctorId);

        /// <summary>
        /// جستجو و صفحه‌بندی پزشکان.
        /// </summary>
        Task<ServiceResult<PagedResult<DoctorIndexViewModel>>> SearchDoctorsAsync(string searchTerm, int pageNumber, int pageSize);
    }
}