using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// رابط سرویس دامنه پذیرش
    /// مسئولیت: مدیریت منطق کسب‌وکار و ایجاد Entity های معتبر
    /// </summary>
    public interface IReceptionDomainService
    {
        /// <summary>
        /// ایجاد پذیرش معتبر با اعتبارسنجی کامل
        /// </summary>
        /// <param name="model">مدل فرم پذیرش</param>
        /// <param name="calculation">نتیجه محاسبه</param>
        /// <returns>پذیرش معتبر</returns>
        Task<ServiceResult<Models.Entities.Reception.Reception>> CreateValidReceptionAsync(ReceptionFormViewModel model, ReceptionCalculationResult calculation);

        /// <summary>
        /// ایجاد آیتم‌های پذیرش معتبر
        /// </summary>
        /// <param name="serviceCalculations">محاسبات خدمات</param>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>لیست آیتم‌های معتبر</returns>
        Task<ServiceResult<List<Models.Entities.Reception.ReceptionItem>>> CreateValidReceptionItemsAsync(List<ViewModels.Reception.ServiceCalculationResult> serviceCalculations, int receptionId);

        /// <summary>
        /// اعتبارسنجی کامل پذیرش
        /// </summary>
        /// <param name="reception">پذیرش</param>
        /// <param name="items">آیتم‌های پذیرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidateReceptionCompletenessAsync(Models.Entities.Reception.Reception reception, List<Models.Entities.Reception.ReceptionItem> items);
    }
}
