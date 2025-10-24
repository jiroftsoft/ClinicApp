using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.ViewModels.Reception;
using ClinicApp.ViewModels;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// Interface سرویس مدیریت بیماران در پذیرش
    /// </summary>
    public interface IReceptionPatientService
    {
        /// <summary>
        /// جستجوی بیمار بر اساس کد ملی
        /// </summary>
        Task<ServiceResult<ReceptionPatientLookupViewModel>> LookupPatientByNationalCodeAsync(string nationalCode);

        /// <summary>
        /// جستجوی بیماران بر اساس نام
        /// </summary>
        Task<ServiceResult<List<ReceptionPatientLookupViewModel>>> SearchPatientsByNameAsync(string name, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// ایجاد بیمار جدید در حین پذیرش
        /// </summary>
        Task<ServiceResult<ReceptionPatientLookupViewModel>> CreatePatientInlineAsync(PatientCreateEditViewModel model);

        /// <summary>
        /// دریافت تاریخچه پذیرش‌های بیمار
        /// </summary>
        Task<ServiceResult<List<ReceptionPatientHistoryViewModel>>> GetPatientReceptionHistoryAsync(int patientId, int pageNumber = 1, int pageSize = 10);
    }
}
