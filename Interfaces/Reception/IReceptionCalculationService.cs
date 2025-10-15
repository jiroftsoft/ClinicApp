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
    }
}
