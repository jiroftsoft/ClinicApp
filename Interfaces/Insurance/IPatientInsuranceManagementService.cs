using System;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.DTOs.Insurance;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface سرویس مدیریت بیمه بیماران
    /// </summary>
    public interface IPatientInsuranceManagementService
    {
        #region انتخاب بیمه

        /// <summary>
        /// انتخاب بیمه پایه برای بیمار
        /// </summary>
        Task<ServiceResult<PatientInsuranceSelectionResult>> SelectPrimaryInsuranceAsync(
            int patientId, int insurancePlanId, string policyNumber, DateTime startDate, DateTime? endDate = null);

        /// <summary>
        /// انتخاب بیمه تکمیلی برای بیمار
        /// </summary>
        Task<ServiceResult<PatientInsuranceSelectionResult>> SelectSupplementaryInsuranceAsync(
            int patientId, int insurancePlanId, string policyNumber, DateTime startDate, DateTime? endDate = null);

        #endregion

        #region تغییر بیمه

        /// <summary>
        /// تغییر بیمه پایه بیمار
        /// </summary>
        Task<ServiceResult<PatientInsuranceSelectionResult>> ChangePrimaryInsuranceAsync(
            int patientId, int newInsurancePlanId, string newPolicyNumber, DateTime startDate, DateTime? endDate = null);

        /// <summary>
        /// تغییر بیمه تکمیلی بیمار
        /// </summary>
        Task<ServiceResult<PatientInsuranceSelectionResult>> ChangeSupplementaryInsuranceAsync(
            int patientId, int newInsurancePlanId, string newPolicyNumber, DateTime startDate, DateTime? endDate = null);

        #endregion

        #region دریافت وضعیت

        /// <summary>
        /// دریافت وضعیت کامل بیمه بیمار
        /// </summary>
        Task<ServiceResult<PatientInsuranceStatus>> GetPatientInsuranceStatusAsync(int patientId);

        #endregion

        #region غیرفعال کردن

        /// <summary>
        /// غیرفعال کردن بیمه پایه
        /// </summary>
        Task<ServiceResult<bool>> DeactivatePrimaryInsuranceAsync(int patientId);

        /// <summary>
        /// غیرفعال کردن بیمه تکمیلی
        /// </summary>
        Task<ServiceResult<bool>> DeactivateSupplementaryInsuranceAsync(int patientId);

        #endregion
    }
}
