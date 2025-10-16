using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Repositories.Reception
{
    /// <summary>
    /// Repository تخصصی برای مدیریت شیفت‌ها در ماژول پذیرش
    /// </summary>
    public interface IShiftManagementRepository
    {
        /// <summary>
        /// دریافت شیفت‌های فعال
        /// </summary>
        Task<ServiceResult<List<ShiftLookupViewModel>>> GetActiveShiftsAsync();

        /// <summary>
        /// بررسی فعال بودن شیفت
        /// </summary>
        Task<ServiceResult<bool>> IsShiftActiveAsync(int shiftId);

        /// <summary>
        /// دریافت اطلاعات شیفت فعلی
        /// </summary>
        Task<ServiceResult<ShiftInfoViewModel>> GetCurrentShiftInfoAsync();

        /// <summary>
        /// دریافت تخصص‌های فعال
        /// </summary>
        Task<ServiceResult<List<SpecializationLookupViewModel>>> GetActiveSpecializationsAsync();

        /// <summary>
        /// دریافت تخصص‌های دپارتمان
        /// </summary>
        Task<ServiceResult<List<SpecializationLookupViewModel>>> GetDepartmentSpecializationsAsync(int departmentId);
    }
}
