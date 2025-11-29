using System;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.DTOs.Insurance;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface سرویس بررسی جامع وضعیت بیمه - قابل استفاده مجدد در تمام ماژول‌ها
    /// 
    /// این سرویس برای بررسی وضعیت بیمه بیمار و نمایش هشدارهای واضح به منشی‌ها طراحی شده است
    /// </summary>
    public interface IInsuranceStatusCheckerService
    {
        /// <summary>
        /// بررسی جامع وضعیت بیمه بیمار
        /// 
        /// این متد تمام جنبه‌های بیمه را بررسی می‌کند:
        /// - وجود بیمه پایه و تکمیلی
        /// - تاریخ انقضا
        /// - وضعیت فعال/غیرفعال
        /// - هشدارهای لازم برای منشی
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="checkDate">تاریخ بررسی (معمولاً تاریخ پذیرش) - اگر null باشد از تاریخ امروز استفاده می‌شود</param>
        /// <returns>نتیجه بررسی جامع وضعیت بیمه</returns>
        Task<ServiceResult<InsuranceStatusCheckResult>> CheckInsuranceStatusAsync(int patientId, DateTime? checkDate = null);

        /// <summary>
        /// بررسی سریع وضعیت بیمه (برای استفاده در real-time validation)
        /// 
        /// این متد فقط بررسی می‌کند که آیا بیمه معتبر است یا نه
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="checkDate">تاریخ بررسی</param>
        /// <returns>true اگر بیمه معتبر باشد، false در غیر این صورت</returns>
        Task<ServiceResult<bool>> IsInsuranceValidAsync(int patientId, DateTime? checkDate = null);

        /// <summary>
        /// بررسی انقضای بیمه (برای هشدار به منشی)
        /// 
        /// این متد بررسی می‌کند که آیا بیمه منقضی شده یا در حال انقضا است
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="checkDate">تاریخ بررسی</param>
        /// <param name="warningDays">تعداد روزهای قبل از انقضا که باید هشدار داده شود (پیش‌فرض: 30)</param>
        /// <returns>نتیجه بررسی انقضا</returns>
        Task<ServiceResult<InsuranceExpiryCheckResult>> CheckInsuranceExpiryAsync(int patientId, DateTime? checkDate = null, int warningDays = 30);

        /// <summary>
        /// بررسی وضعیت بیمه برای پذیرش (بهینه‌سازی شده برای فرم پذیرش)
        /// 
        /// این متد بررسی می‌کند که آیا می‌توان پذیرش را با این بیمه انجام داد یا نه
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه بررسی برای پذیرش</returns>
        Task<ServiceResult<InsuranceStatusCheckResult>> CheckInsuranceForReceptionAsync(int patientId, DateTime receptionDate);
    }

    /// <summary>
    /// نتیجه بررسی انقضای بیمه
    /// </summary>
    public class InsuranceExpiryCheckResult
    {
        public int PatientId { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoon { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public DateTime? PrimaryInsuranceExpiryDate { get; set; }
        public DateTime? SupplementaryInsuranceExpiryDate { get; set; }
        public string WarningMessage { get; set; }
    }
}

