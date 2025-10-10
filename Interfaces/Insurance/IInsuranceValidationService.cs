using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Service Interface برای اعتبارسنجی بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی پوشش بیمه برای خدمات مختلف
    /// 2. استفاده از ServiceResult Enhanced pattern
    /// 3. پشتیبانی از FluentValidation
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. بررسی انقضای بیمه
    /// 6. بررسی واجد شرایط بودن خدمات
    /// 7. مدیریت بیمه اصلی و تکمیلی
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط منطق اعتبارسنجی بیمه
    /// ✅ Separation of Concerns: Repository layer فقط دسترسی به داده
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Service layer منطق کسب‌وکار
    /// </summary>
    public interface IInsuranceValidationService
    {
        #region Coverage Validation

        /// <summary>
        /// اعتبارسنجی پوشش بیمه
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="appointmentDate">تاریخ قرار ملاقات</param>
        /// <returns>نتیجه اعتبارسنجی پوشش</returns>
        Task<ServiceResult<bool>> ValidateCoverageAsync(int patientId, int serviceId, System.DateTime appointmentDate);

        /// <summary>
        /// اعتبارسنجی پوشش بیمه برای لیست خدمات
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="appointmentDate">تاریخ قرار ملاقات</param>
        /// <returns>نتیجه اعتبارسنجی پوشش</returns>
        Task<ServiceResult<System.Collections.Generic.Dictionary<int, bool>>> ValidateCoverageForServicesAsync(int patientId, System.Collections.Generic.List<int> serviceIds, System.DateTime appointmentDate);

        #endregion

        #region Insurance Expiry Validation

        /// <summary>
        /// اعتبارسنجی انقضای بیمه بیمار
        /// </summary>
        /// <param name="patientInsuranceId">شناسه بیمه بیمار</param>
        /// <returns>نتیجه اعتبارسنجی انقضا</returns>
        Task<ServiceResult<bool>> ValidateInsuranceExpiryAsync(int patientInsuranceId);

        /// <summary>
        /// اعتبارسنجی انقضای بیمه بیمار در تاریخ مشخص
        /// </summary>
        /// <param name="patientInsuranceId">شناسه بیمه بیمار</param>
        /// <param name="checkDate">تاریخ بررسی</param>
        /// <returns>نتیجه اعتبارسنجی انقضا</returns>
        Task<ServiceResult<bool>> ValidateInsuranceExpiryAsync(int patientInsuranceId, System.DateTime checkDate);

        /// <summary>
        /// اعتبارسنجی انقضای بیمه‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>نتیجه اعتبارسنجی انقضای بیمه‌ها</returns>
        Task<ServiceResult<System.Collections.Generic.Dictionary<int, bool>>> ValidatePatientInsurancesExpiryAsync(int patientId);

        #endregion

        #region Service Coverage Validation

        /// <summary>
        /// اعتبارسنجی پوشش خدمت در طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>نتیجه اعتبارسنجی پوشش خدمت</returns>
        Task<ServiceResult<bool>> ValidateServiceCoverageAsync(int planId, int serviceCategoryId);

        /// <summary>
        /// اعتبارسنجی پوشش خدمات در طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceCategoryIds">لیست شناسه‌های دسته‌بندی خدمات</param>
        /// <returns>نتیجه اعتبارسنجی پوشش خدمات</returns>
        Task<ServiceResult<System.Collections.Generic.Dictionary<int, bool>>> ValidateServiceCoverageForCategoriesAsync(int planId, System.Collections.Generic.List<int> serviceCategoryIds);

        #endregion

        #region Reception-Specific Validation

        /// <summary>
        /// اعتبارسنجی بدهی بیمار برای پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>نتیجه اعتبارسنجی بدهی</returns>
        Task<ServiceResult<bool>> ValidatePatientDebtAsync(int patientId);

        /// <summary>
        /// اعتبارسنجی ظرفیت پزشک برای پذیرش
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="appointmentDate">تاریخ قرار ملاقات</param>
        /// <returns>نتیجه اعتبارسنجی ظرفیت</returns>
        Task<ServiceResult<bool>> ValidateDoctorCapacityAsync(int doctorId, System.DateTime appointmentDate);

        /// <summary>
        /// اعتبارسنجی Real-time داده‌های پذیرش
        /// </summary>
        /// <param name="model">مدل داده‌های پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی Real-time</returns>
        Task<ServiceResult<bool>> ValidateReceptionDataRealTimeAsync(object model);

        #endregion

        #region Eligibility Validation

        /// <summary>
        /// بررسی واجد شرایط بودن بیمه برای خدمت
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>نتیجه بررسی واجد شرایط بودن</returns>
        Task<ServiceResult<bool>> CheckInsuranceEligibilityAsync(int patientId, int serviceId);

        /// <summary>
        /// بررسی واجد شرایط بودن بیمه برای خدمات
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <returns>نتیجه بررسی واجد شرایط بودن</returns>
        Task<ServiceResult<System.Collections.Generic.Dictionary<int, bool>>> CheckInsuranceEligibilityForServicesAsync(int patientId, System.Collections.Generic.List<int> serviceIds);

        #endregion

        #region Business Logic Validation

        /// <summary>
        /// اعتبارسنجی کامل بیمه بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="appointmentDate">تاریخ قرار ملاقات</param>
        /// <returns>نتیجه اعتبارسنجی کامل</returns>
        Task<ServiceResult<InsuranceValidationViewModel>> ValidateCompleteInsuranceAsync(int patientId, int serviceId, System.DateTime appointmentDate);

        /// <summary>
        /// اعتبارسنجی بیمه برای پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی بیمه برای پذیرش</returns>
        Task<ServiceResult<InsuranceValidationViewModel>> ValidateInsuranceForReceptionAsync(int patientId, System.Collections.Generic.List<int> serviceIds, System.DateTime receptionDate);

        #endregion
    }
}
