using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.ViewModels.Patient;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// سرویس مدیریت تنظیمات بیمار
    /// Single Responsibility: مدیریت تنظیمات حساب، اعلان‌ها، و حریم خصوصی
    /// طبق: DEVELOPMENT_CONTRACT.md - ServiceResult Enhanced
    /// </summary>
    public interface IPatientSettingsService
    {
        /// <summary>
        /// دریافت تنظیمات بیمار
        /// </summary>
        Task<ServiceResult<PatientSettingsViewModel>> GetSettingsAsync(int patientId);

        /// <summary>
        /// به‌روزرسانی تنظیمات اعلان‌ها
        /// </summary>
        Task<ServiceResult> UpdateNotificationSettingsAsync(int patientId, NotificationSettingsDto dto);

        /// <summary>
        /// به‌روزرسانی تنظیمات حریم خصوصی
        /// </summary>
        Task<ServiceResult> UpdatePrivacySettingsAsync(int patientId, PrivacySettingsDto dto);
    }
}

