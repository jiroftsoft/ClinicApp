using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// اینترفیس برای سرویس مدیریت دپارتمان و پزشک در ماژول پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کلینیک‌ها و دپارتمان‌ها
    /// 2. مدیریت پزشکان و تخصص‌ها
    /// 3. مدیریت شیفت‌های کاری
    /// 4. بهینه‌سازی برای محیط درمانی
    /// 5. پشتیبانی از cascade loading
    /// </summary>
    public interface IReceptionDepartmentDoctorService
    {
        #region Clinic Management

        /// <summary>
        /// دریافت کلینیک‌های فعال برای فرم پذیرش
        /// </summary>
        Task<ServiceResult<List<ClinicLookupViewModel>>> GetActiveClinicsAsync();

        /// <summary>
        /// دریافت اطلاعات کلینیک بر اساس شناسه
        /// </summary>
        Task<ServiceResult<ClinicLookupViewModel>> GetClinicByIdAsync(int clinicId);

        #endregion

        #region Department Management

        /// <summary>
        /// دریافت دپارتمان‌های کلینیک برای فرم پذیرش
        /// </summary>
        Task<ServiceResult<List<DepartmentLookupViewModel>>> GetClinicDepartmentsAsync(int clinicId);

        /// <summary>
        /// دریافت دپارتمان‌های فعال بر اساس شیفت فعلی
        /// </summary>
        Task<ServiceResult<List<DepartmentLookupViewModel>>> GetActiveDepartmentsByShiftAsync(int clinicId);

        /// <summary>
        /// دریافت اطلاعات دپارتمان بر اساس شناسه
        /// </summary>
        Task<ServiceResult<DepartmentLookupViewModel>> GetDepartmentByIdAsync(int departmentId);

        #endregion

        #region Doctor Management

        /// <summary>
        /// دریافت پزشکان دپارتمان برای فرم پذیرش
        /// </summary>
        Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDepartmentDoctorsAsync(int departmentId);

        /// <summary>
        /// دریافت پزشکان بر اساس تخصص
        /// </summary>
        Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsBySpecializationAsync(int specializationId);

        /// <summary>
        /// دریافت پزشکان فعال بر اساس شیفت فعلی
        /// </summary>
        Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetActiveDoctorsByShiftAsync(int departmentId);

        /// <summary>
        /// دریافت اطلاعات پزشک بر اساس شناسه
        /// </summary>
        Task<ServiceResult<ReceptionDoctorLookupViewModel>> GetDoctorByIdAsync(int doctorId);

        #endregion

        #region Specialization Management

        /// <summary>
        /// دریافت تخصص‌های فعال
        /// </summary>
        Task<ServiceResult<List<SpecializationLookupViewModel>>> GetActiveSpecializationsAsync();

        /// <summary>
        /// دریافت تخصص‌های دپارتمان
        /// </summary>
        Task<ServiceResult<List<SpecializationLookupViewModel>>> GetDepartmentSpecializationsAsync(int departmentId);

        #endregion

        #region Shift Management

        /// <summary>
        /// دریافت اطلاعات شیفت فعلی
        /// </summary>
        Task<ServiceResult<object>> GetCurrentShiftInfoAsync();

        /// <summary>
        /// دریافت شیفت‌های فعال
        /// </summary>
        Task<ServiceResult<List<object>>> GetActiveShiftsAsync();

        /// <summary>
        /// بررسی فعال بودن شیفت
        /// </summary>
        Task<ServiceResult<bool>> IsShiftActiveAsync(int shiftId);

        #endregion

        #region Combined Operations

        /// <summary>
        /// دریافت اطلاعات کامل دپارتمان و پزشک برای فرم پذیرش
        /// </summary>
        Task<ServiceResult<object>> GetDepartmentDoctorInfoAsync(int clinicId, int? departmentId = null);

        /// <summary>
        /// جستجوی پیشرفته پزشکان
        /// </summary>
        Task<ServiceResult<List<object>>> SearchDoctorsAsync(object searchModel);

        /// <summary>
        /// دریافت آمار دپارتمان و پزشک
        /// </summary>
        Task<ServiceResult<object>> GetDepartmentDoctorStatsAsync(int clinicId);

        #endregion
    }
}
