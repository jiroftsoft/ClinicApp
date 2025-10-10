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

        /// <summary>
        /// اعتبارسنجی بیمه بیمار برای پذیرش (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult<object>> ValidatePatientInsuranceForReceptionAsync(int patientId, System.Collections.Generic.List<int> serviceIds, System.DateTime receptionDate);

        /// <summary>
        /// اعتبارسنجی سریع بیمه بیمار (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی سریع</returns>
        Task<ServiceResult<object>> QuickValidatePatientInsuranceAsync(int patientId, System.DateTime receptionDate);
    }
}
