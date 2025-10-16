using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Repositories.Reception
{
    /// <summary>
    /// Repository تخصصی برای مدیریت کلینیک‌ها در ماژول پذیرش
    /// </summary>
    public interface IClinicManagementRepository
    {
        /// <summary>
        /// دریافت لیست کلینیک‌های فعال
        /// </summary>
        Task<ServiceResult<List<ClinicLookupViewModel>>> GetActiveClinicsAsync();

        /// <summary>
        /// دریافت کلینیک بر اساس شناسه
        /// </summary>
        Task<ServiceResult<ClinicLookupViewModel>> GetClinicByIdAsync(int clinicId);

        /// <summary>
        /// دریافت دپارتمان‌های کلینیک
        /// </summary>
        Task<ServiceResult<List<DepartmentLookupViewModel>>> GetClinicDepartmentsAsync(int clinicId);

        /// <summary>
        /// دریافت دپارتمان‌های فعال بر اساس شیفت
        /// </summary>
        Task<ServiceResult<List<DepartmentLookupViewModel>>> GetActiveDepartmentsByShiftAsync(int clinicId, string shiftType);
    }
}
