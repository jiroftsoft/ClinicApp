using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Services.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// Interface برای سرویس مدیریت فرآیندهای پذیرش
    /// </summary>
    public interface IReceptionWorkflowService
    {
        /// <summary>
        /// شروع فرآیند پذیرش
        /// </summary>
        /// <param name="request">درخواست شروع فرآیند</param>
        /// <returns>نتیجه شروع فرآیند</returns>
        Task<ServiceResult<ReceptionWorkflowResult>> StartReceptionWorkflowAsync(ReceptionWorkflowRequest request);

        /// <summary>
        /// پردازش مرحله پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="stepName">نام مرحله</param>
        /// <param name="stepData">داده‌های مرحله</param>
        /// <returns>نتیجه پردازش مرحله</returns>
        Task<ServiceResult<ReceptionWorkflowResult>> ProcessReceptionStepAsync(int receptionId, string stepName, object stepData);

        /// <summary>
        /// تکمیل فرآیند پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>نتیجه تکمیل فرآیند</returns>
        Task<ServiceResult<ReceptionWorkflowResult>> CompleteReceptionWorkflowAsync(int receptionId);

        /// <summary>
        /// لغو فرآیند پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="reason">دلیل لغو</param>
        /// <returns>نتیجه لغو فرآیند</returns>
        Task<ServiceResult<ReceptionWorkflowResult>> CancelReceptionWorkflowAsync(int receptionId, string reason);

        /// <summary>
        /// افزودن آیتم به پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="quantity">تعداد</param>
        /// <param name="unitPrice">قیمت واحد</param>
        /// <returns>نتیجه افزودن آیتم</returns>
        Task<ServiceResult<bool>> AddItemAsync(int receptionId, int serviceId, int quantity, decimal unitPrice);

        /// <summary>
        /// تنظیم بیمه‌های پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="basePlanId">شناسه بیمه پایه</param>
        /// <param name="suppPlanId">شناسه بیمه تکمیلی</param>
        /// <returns>نتیجه تنظیم بیمه‌ها</returns>
        Task<ServiceResult<bool>> SetInsurancesAsync(int receptionId, int? basePlanId, int? suppPlanId);

        /// <summary>
        /// نهایی‌سازی پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>نتیجه نهایی‌سازی</returns>
        Task<ServiceResult<bool>> FinalizeAsync(int receptionId);
    }
}
