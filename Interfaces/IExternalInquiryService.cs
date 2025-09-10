using System;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Interface برای سرویس استعلام کمکی اطلاعات بیمار
    /// این سرویس برای بهبود دقت و سرعت پذیرش طراحی شده است
    /// </summary>
    public interface IExternalInquiryService
    {
        /// <summary>
        /// استعلام اطلاعات هویت بیمار از ثبت احوال
        /// </summary>
        /// <param name="nationalCode">کد ملی بیمار</param>
        /// <param name="birthDate">تاریخ تولد بیمار</param>
        /// <param name="tokenId">شناسه توکن امنیتی</param>
        /// <returns>نتیجه استعلام هویت</returns>
        Task<ServiceResult<PatientIdentityData>> InquiryIdentityAsync(string nationalCode, DateTime birthDate, string tokenId = "TR127256");

        /// <summary>
        /// استعلام اطلاعات بیمه بیمار از سرویس بیمه
        /// </summary>
        /// <param name="nationalCode">کد ملی بیمار</param>
        /// <param name="birthDate">تاریخ تولد بیمار</param>
        /// <param name="tokenId">شناسه توکن امنیتی</param>
        /// <returns>نتیجه استعلام بیمه</returns>
        Task<ServiceResult<PatientInsuranceData>> InquiryInsuranceAsync(string nationalCode, DateTime birthDate, string tokenId = "TR127256");

        /// <summary>
        /// استعلام کامل (هویت + بیمه) بیمار
        /// </summary>
        /// <param name="nationalCode">کد ملی بیمار</param>
        /// <param name="birthDate">تاریخ تولد بیمار</param>
        /// <param name="inquiryType">نوع استعلام</param>
        /// <param name="tokenId">شناسه توکن امنیتی</param>
        /// <returns>نتیجه استعلام کامل</returns>
        Task<ServiceResult<PatientInquiryViewModel>> InquiryCompleteAsync(string nationalCode, DateTime birthDate, InquiryType inquiryType, string tokenId = "TR127256");

        /// <summary>
        /// بررسی وضعیت توکن امنیتی
        /// </summary>
        /// <param name="tokenId">شناسه توکن</param>
        /// <returns>وضعیت توکن</returns>
        Task<ServiceResult<bool>> CheckTokenStatusAsync(string tokenId = "TR127256");

        /// <summary>
        /// بررسی دسترسی به سرویس‌های خارجی
        /// </summary>
        /// <returns>وضعیت دسترسی</returns>
        Task<ServiceResult<ExternalServiceStatus>> CheckServiceAvailabilityAsync();

        /// <summary>
        /// اعتبارسنجی کد ملی قبل از استعلام
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult<bool>> ValidateNationalCodeAsync(string nationalCode);
    }

    /// <summary>
    /// وضعیت سرویس‌های خارجی
    /// </summary>
    public class ExternalServiceStatus
    {
        public bool CivilRegistrationAvailable { get; set; }
        public bool InsuranceServiceAvailable { get; set; }
        public bool TokenAvailable { get; set; }
        public string LastCheckTime { get; set; }
        public string OverallStatus { get; set; }
    }
}
