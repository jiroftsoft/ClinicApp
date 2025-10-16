using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Repositories.Reception
{
    /// <summary>
    /// Repository تخصصی برای مدیریت پزشکان در ماژول پذیرش
    /// </summary>
    public interface IDoctorManagementRepository
    {
        /// <summary>
        /// دریافت پزشکان دپارتمان
        /// </summary>
        Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDepartmentDoctorsAsync(int departmentId);

        /// <summary>
        /// دریافت پزشکان فعال بر اساس شیفت
        /// </summary>
        Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetActiveDoctorsByShiftAsync(int departmentId, string shiftType);

        /// <summary>
        /// دریافت پزشک بر اساس شناسه
        /// </summary>
        Task<ServiceResult<ReceptionDoctorLookupViewModel>> GetDoctorByIdAsync(int doctorId);

        /// <summary>
        /// جستجوی پزشکان
        /// </summary>
        Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> SearchDoctorsAsync(string searchTerm, int? departmentId = null);

        /// <summary>
        /// دریافت پزشکان بر اساس تخصص
        /// </summary>
        Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsBySpecializationAsync(int specializationId);
    }
}
