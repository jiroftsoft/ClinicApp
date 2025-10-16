using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// رابط سرویس محاسبات تخصصی پذیرش
    /// مسئولیت: محاسبات تخصصی برای فرم پذیرش
    /// </summary>
    public interface IReceptionCalculationService
    {
        /// <summary>
        /// محاسبه کامل پذیرش برای فرم پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه محاسبه پذیرش</returns>
        Task<ServiceResult<ReceptionCalculationResult>> CalculateReceptionAsync(int patientId, List<int> serviceIds, DateTime receptionDate);

        /// <summary>
        /// محاسبه یک خدمت برای پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه محاسبه خدمت</returns>
        Task<ServiceResult<ViewModels.Reception.ServiceCalculationResult>> CalculateServiceForReceptionAsync(int patientId, int serviceId, DateTime receptionDate);

        /// <summary>
        /// محاسبه سریع برای نمایش در فرم پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="customAmount">مبلغ سفارشی (اختیاری)</param>
        /// <returns>نتیجه محاسبه سریع</returns>
        Task<ServiceResult<QuickReceptionCalculation>> CalculateQuickReceptionAsync(int patientId, int serviceId, decimal? customAmount = null);

        /// <summary>
        /// محاسبه هزینه خدمات
        /// </summary>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>نتیجه محاسبه هزینه</returns>
        Task<ServiceResult<ViewModels.Reception.ServiceCalculationResult>> CalculateServiceCostsAsync(List<int> serviceIds, int patientId);

        /// <summary>
        /// محاسبه هزینه خدمات (overload)
        /// </summary>
        /// <param name="request">درخواست محاسبه</param>
        /// <returns>نتیجه محاسبه هزینه</returns>
        Task<ServiceResult<ViewModels.Reception.ServiceCalculationResult>> CalculateServiceCostsAsync(ServiceCalculationRequest request);

        /// <summary>
        /// محاسبه اطلاعات پرداخت
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId, List<int> serviceIds);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId, List<int> serviceIds, DateTime receptionDate);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <param name="customAmount">مبلغ سفارشی</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId, List<int> serviceIds, DateTime receptionDate, decimal? customAmount);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <param name="customAmount">مبلغ سفارشی</param>
        /// <param name="discountAmount">مبلغ تخفیف</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId, List<int> serviceIds, DateTime receptionDate, decimal? customAmount, decimal? discountAmount);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <param name="customAmount">مبلغ سفارشی</param>
        /// <param name="discountAmount">مبلغ تخفیف</param>
        /// <param name="insurancePlanId">شناسه طرح بیمه</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId, List<int> serviceIds, DateTime receptionDate, decimal? customAmount, decimal? discountAmount, int? insurancePlanId);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <param name="customAmount">مبلغ سفارشی</param>
        /// <param name="discountAmount">مبلغ تخفیف</param>
        /// <param name="insurancePlanId">شناسه طرح بیمه</param>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId, List<int> serviceIds, DateTime receptionDate, decimal? customAmount, decimal? discountAmount, int? insurancePlanId, int? doctorId);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <param name="customAmount">مبلغ سفارشی</param>
        /// <param name="discountAmount">مبلغ تخفیف</param>
        /// <param name="insurancePlanId">شناسه طرح بیمه</param>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId, List<int> serviceIds, DateTime receptionDate, decimal? customAmount, decimal? discountAmount, int? insurancePlanId, int? doctorId, int? clinicId);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <param name="customAmount">مبلغ سفارشی</param>
        /// <param name="discountAmount">مبلغ تخفیف</param>
        /// <param name="insurancePlanId">شناسه طرح بیمه</param>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId, List<int> serviceIds, DateTime receptionDate, decimal? customAmount, decimal? discountAmount, int? insurancePlanId, int? doctorId, int? clinicId, int? departmentId);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <param name="customAmount">مبلغ سفارشی</param>
        /// <param name="discountAmount">مبلغ تخفیف</param>
        /// <param name="insurancePlanId">شناسه طرح بیمه</param>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="specializationId">شناسه تخصص</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId, List<int> serviceIds, DateTime receptionDate, decimal? customAmount, decimal? discountAmount, int? insurancePlanId, int? doctorId, int? clinicId, int? departmentId, int? specializationId);

        /// <summary>
        /// محاسبه اطلاعات پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <param name="customAmount">مبلغ سفارشی</param>
        /// <param name="discountAmount">مبلغ تخفیف</param>
        /// <param name="insurancePlanId">شناسه طرح بیمه</param>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="specializationId">شناسه تخصص</param>
        /// <param name="shiftId">شناسه شیفت</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> CalculatePaymentInfoAsync(int receptionId, int patientId, List<int> serviceIds, DateTime receptionDate, decimal? customAmount, decimal? discountAmount, int? insurancePlanId, int? doctorId, int? clinicId, int? departmentId, int? specializationId, int? shiftId);
    }
}
