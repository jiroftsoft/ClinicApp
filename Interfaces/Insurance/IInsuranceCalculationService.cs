using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Service Interface برای محاسبات بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبه سهم بیمار و بیمه برای خدمات مختلف
    /// 2. استفاده از ServiceResult Enhanced pattern
    /// 3. پشتیبانی از FluentValidation
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. محاسبه هزینه‌های پذیرش و قرار ملاقات
    /// 6. مدیریت بیمه اصلی و تکمیلی
    /// 7. محاسبه Franchise و Copay
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط منطق محاسبات بیمه
    /// ✅ Separation of Concerns: Repository layer فقط دسترسی به داده
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Service layer منطق کسب‌وکار
    /// </summary>
    public interface IInsuranceCalculationService
    {
        #region Calculation Operations

        /// <summary>
        /// محاسبه سهم بیمار برای یک خدمت
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>نتیجه محاسبه سهم بیمار</returns>
        Task<ServiceResult<InsuranceCalculationResultViewModel>> CalculatePatientShareAsync(int patientId, int serviceId, System.DateTime calculationDate);

        /// <summary>
        /// محاسبه هزینه‌های پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه محاسبه هزینه‌های پذیرش</returns>
        Task<ServiceResult<InsuranceCalculationResultViewModel>> CalculateReceptionCostsAsync(int patientId, System.Collections.Generic.List<int> serviceIds, System.DateTime receptionDate);

        /// <summary>
        /// محاسبه هزینه قرار ملاقات
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="appointmentDate">تاریخ قرار ملاقات</param>
        /// <returns>نتیجه محاسبه هزینه قرار ملاقات</returns>
        Task<ServiceResult<InsuranceCalculationResultViewModel>> CalculateAppointmentCostAsync(int patientId, int serviceId, System.DateTime appointmentDate);

        /// <summary>
        /// دریافت نتیجه محاسبه بیمه
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>نتیجه محاسبه بیمه</returns>
        Task<ServiceResult<InsuranceCalculationResultViewModel>> GetInsuranceCalculationResultAsync(int patientId, int serviceId, System.DateTime calculationDate);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی پوشش بیمه برای خدمت
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="calculationDate">تاریخ بررسی</param>
        /// <returns>نتیجه بررسی پوشش</returns>
        Task<ServiceResult<bool>> IsServiceCoveredAsync(int patientId, int serviceId, System.DateTime calculationDate);

        /// <summary>
        /// بررسی اعتبار بیمه بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="calculationDate">تاریخ بررسی</param>
        /// <returns>نتیجه بررسی اعتبار</returns>
        Task<ServiceResult<bool>> IsPatientInsuranceValidAsync(int patientId, System.DateTime calculationDate);

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// محاسبه Franchise
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>مقدار Franchise</returns>
        Task<ServiceResult<decimal>> CalculateFranchiseAsync(int patientId, int serviceId, System.DateTime calculationDate);

        /// <summary>
        /// محاسبه Copay
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>مقدار Copay</returns>
        Task<ServiceResult<decimal>> CalculateCopayAsync(int patientId, int serviceId, System.DateTime calculationDate);

        /// <summary>
        /// محاسبه Coverage Percentage
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>درصد پوشش</returns>
        Task<ServiceResult<decimal>> CalculateCoveragePercentageAsync(int patientId, int serviceId, System.DateTime calculationDate);

        #endregion

        #region InsuranceCalculation Management Operations

        /// <summary>
        /// ذخیره محاسبه بیمه در دیتابیس
        /// </summary>
        /// <param name="calculation">محاسبه بیمه</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<ServiceResult<InsuranceCalculation>> SaveCalculationAsync(InsuranceCalculation calculation);

        /// <summary>
        /// دریافت محاسبات بیمه بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست محاسبات بیمه بیمار</returns>
        Task<ServiceResult<List<InsuranceCalculation>>> GetPatientCalculationsAsync(int patientId);

        /// <summary>
        /// دریافت محاسبات بیمه پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>لیست محاسبات بیمه پذیرش</returns>
        Task<ServiceResult<List<InsuranceCalculation>>> GetReceptionCalculationsAsync(int receptionId);

        /// <summary>
        /// دریافت محاسبات بیمه قرار ملاقات
        /// </summary>
        /// <param name="appointmentId">شناسه قرار ملاقات</param>
        /// <returns>لیست محاسبات بیمه قرار ملاقات</returns>
        Task<ServiceResult<List<InsuranceCalculation>>> GetAppointmentCalculationsAsync(int appointmentId);

        /// <summary>
        /// دریافت آمار محاسبات بیمه
        /// </summary>
        /// <returns>آمار محاسبات بیمه</returns>
        Task<ServiceResult<object>> GetCalculationStatisticsAsync();

        /// <summary>
        /// جستجوی محاسبات بیمه
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="patientId">شناسه بیمار (اختیاری)</param>
        /// <param name="serviceId">شناسه خدمت (اختیاری)</param>
        /// <param name="planId">شناسه طرح بیمه (اختیاری)</param>
        /// <param name="isValid">وضعیت اعتبار (اختیاری)</param>
        /// <param name="fromDate">تاریخ شروع (اختیاری)</param>
        /// <param name="toDate">تاریخ پایان (اختیاری)</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتایج جستجو</returns>
        Task<ServiceResult<(List<InsuranceCalculation> Items, int TotalCount)>> SearchCalculationsAsync(
            string searchTerm = null,
            int? patientId = null,
            int? serviceId = null,
            int? planId = null,
            bool? isValid = null,
            System.DateTime? fromDate = null,
            System.DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// به‌روزرسانی وضعیت اعتبار محاسبه
        /// </summary>
        /// <param name="calculationId">شناسه محاسبه</param>
        /// <param name="isValid">وضعیت اعتبار</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult<bool>> UpdateCalculationValidityAsync(int calculationId, bool isValid);

        /// <summary>
        /// حذف محاسبه بیمه
        /// </summary>
        /// <param name="calculationId">شناسه محاسبه</param>
        /// <returns>نتیجه حذف</returns>
        Task<ServiceResult<bool>> DeleteCalculationAsync(int calculationId);

        #endregion
    }
}
