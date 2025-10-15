using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// رابط سرویس مدیریت فرم پذیرش
    /// مسئولیت: مدیریت کامل فرم پذیرش
    /// </summary>
    public interface IReceptionFormService
    {
        /// <summary>
        /// ایجاد پذیرش جدید از فرم پذیرش
        /// </summary>
        /// <param name="model">مدل فرم پذیرش</param>
        /// <returns>نتیجه ایجاد پذیرش</returns>
        Task<ServiceResult<ReceptionFormResult>> CreateReceptionFromFormAsync(ReceptionFormViewModel model);

        /// <summary>
        /// محاسبه سریع فرم پذیرش
        /// </summary>
        /// <param name="request">درخواست محاسبه</param>
        /// <returns>نتیجه محاسبه</returns>
        Task<ServiceResult<ReceptionFormCalculation>> CalculateReceptionFormAsync(ReceptionFormCalculationRequest request);

        /// <summary>
        /// دریافت اطلاعات فرم پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>اطلاعات فرم پذیرش</returns>
        Task<ServiceResult<ReceptionFormInfo>> GetReceptionFormInfoAsync(int patientId);
    }
}
