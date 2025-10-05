using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.DTOs.Insurance;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface سرویس اعتبارسنجی بیمه بیماران
    /// </summary>
    public interface IPatientInsuranceValidationService
    {
        /// <summary>
        /// اعتبارسنجی کامل بیمه بیمار
        /// </summary>
        Task<ServiceResult<PatientInsuranceValidationResult>> ValidatePatientInsuranceAsync(int patientId);

        /// <summary>
        /// بررسی سریع اعتبار بیمه برای استفاده در ماژول پذیرش
        /// </summary>
        Task<ServiceResult<bool>> IsPatientInsuranceValidAsync(int patientId);
    }
}
