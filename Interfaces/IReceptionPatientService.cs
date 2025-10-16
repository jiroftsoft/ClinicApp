using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس تخصصی مدیریت بیماران در ماژول پذیرش
    /// </summary>
    public interface IReceptionPatientService
    {
        /// <summary>
        /// جستجوی بیمار بر اساس کد ملی
        /// </summary>
        /// <param name="nationalCode">کد ملی بیمار</param>
        /// <returns>نتیجه جستجو</returns>
        Task<ServiceResult<PatientAccordionViewModel>> SearchPatientByNationalCodeAsync(string nationalCode);

        /// <summary>
        /// ذخیره اطلاعات بیمار جدید
        /// </summary>
        /// <param name="model">اطلاعات بیمار</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<ServiceResult<PatientAccordionViewModel>> SaveNewPatientAsync(PatientAccordionViewModel model);

        /// <summary>
        /// به‌روزرسانی اطلاعات بیمار موجود
        /// </summary>
        /// <param name="model">اطلاعات بیمار</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult<PatientAccordionViewModel>> UpdatePatientAsync(PatientAccordionViewModel model);

        /// <summary>
        /// جستجوی پیشرفته بیماران
        /// </summary>
        /// <param name="searchParams">پارامترهای جستجو</param>
        /// <returns>نتیجه جستجو</returns>
        Task<ServiceResult<List<PatientSearchResultViewModel>>> SearchPatientsAsync(SearchParameterViewModel searchParams);

        /// <summary>
        /// اعتبارسنجی اطلاعات بیمار
        /// </summary>
        /// <param name="model">اطلاعات بیمار</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        ServiceResult<bool> ValidatePatientInfo(PatientAccordionViewModel model);
    }
}
