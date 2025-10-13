using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Services.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// Interface برای سرویس مدیریت اورژانس پزشکی
    /// </summary>
    public interface IMedicalEmergencyService
    {
        /// <summary>
        /// مدیریت پذیرش اورژانس
        /// </summary>
        /// <param name="request">درخواست پذیرش اورژانس</param>
        /// <returns>نتیجه پذیرش اورژانس</returns>
        Task<ServiceResult<EmergencyReceptionResult>> HandleEmergencyReceptionAsync(EmergencyReceptionRequest request);

        /// <summary>
        /// تشدید پذیرش اورژانس
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="escalationReason">دلیل تشدید</param>
        /// <returns>نتیجه تشدید</returns>
        Task<ServiceResult<EmergencyReceptionResult>> EscalateEmergencyReceptionAsync(int receptionId, string escalationReason);

        /// <summary>
        /// حل پذیرش اورژانس
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="resolution">راه‌حل</param>
        /// <returns>نتیجه حل</returns>
        Task<ServiceResult<EmergencyReceptionResult>> ResolveEmergencyReceptionAsync(int receptionId, string resolution);
    }
}
