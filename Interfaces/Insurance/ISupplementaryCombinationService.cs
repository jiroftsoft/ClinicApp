using System.Threading.Tasks;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.ViewModels.Insurance.Supplementary;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface for managing supplementary insurance combinations
    /// رابط مدیریت ترکیبات بیمه تکمیلی
    /// </summary>
    public interface ISupplementaryCombinationService
    {
        /// <summary>
        /// Creates a supplementary insurance combination
        /// ایجاد ترکیب بیمه تکمیلی
        /// </summary>
        /// <param name="serviceId">Service ID - شناسه خدمت</param>
        /// <param name="primaryPlanId">Primary insurance plan ID - شناسه طرح بیمه اصلی</param>
        /// <param name="supplementaryPlanId">Supplementary insurance plan ID - شناسه طرح بیمه تکمیلی</param>
        /// <param name="coveragePercent">Coverage percentage - درصد پوشش</param>
        /// <param name="maxPayment">Maximum payment - حداکثر پرداخت</param>
        /// <returns>Created insurance tariff - تعرفه بیمه ایجاد شده</returns>
        Task<InsuranceTariff> CreateCombinationAsync(
            int serviceId, 
            int primaryPlanId, 
            int supplementaryPlanId, 
            decimal coveragePercent, 
            decimal maxPayment);

        /// <summary>
        /// Validates if a combination is possible
        /// اعتبارسنجی امکان ترکیب
        /// </summary>
        /// <param name="primaryPlanId">Primary plan ID - شناسه طرح اصلی</param>
        /// <param name="supplementaryPlanId">Supplementary plan ID - شناسه طرح تکمیلی</param>
        /// <returns>Validation result - نتیجه اعتبارسنجی</returns>
        Task<bool> ValidateCombinationAsync(int primaryPlanId, int supplementaryPlanId);

        /// <summary>
        /// Checks for duplicate combinations
        /// بررسی ترکیبات تکراری
        /// </summary>
        /// <param name="serviceId">Service ID - شناسه خدمت</param>
        /// <param name="primaryPlanId">Primary plan ID - شناسه طرح اصلی</param>
        /// <param name="supplementaryPlanId">Supplementary plan ID - شناسه طرح تکمیلی</param>
        /// <returns>True if duplicate exists - در صورت وجود تکراری true</returns>
        Task<bool> IsDuplicateCombinationAsync(int serviceId, int primaryPlanId, int supplementaryPlanId);
    }
}
