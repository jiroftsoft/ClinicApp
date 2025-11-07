using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// Interface برای ReceptionFacade - Orchestrator نازک ماژول پذیرش
    /// 
    /// مسئولیت: هماهنگی سرویس‌های موجود بدون اضافه کردن منطق جدید
    /// هدف: API-محور و اتمیک کردن فراخوانی‌ها
    /// </summary>
    public interface IReceptionFacade
    {
        #region Loaders

        /// <summary>
        /// بارگذاری اولیه فرم پذیرش
        /// </summary>
        Task<ServiceResult<ReceptionLoadDto>> LoadInitialAsync(int clinicId, int? deptId);

        /// <summary>
        /// دریافت پزشکان یک دپارتمان
        /// </summary>
        Task<ServiceResult<List<DoctorDto>>> GetDoctorsByDepartmentAsync(int deptId, int? clinicId = null);

        /// <summary>
        /// دریافت پزشکان مجاز برای یک خدمت در دپارتمان
        /// </summary>
        Task<ServiceResult<List<DoctorDto>>> GetDoctorsByServiceAsync(int departmentId, int serviceId, int? clinicId = null);

        /// <summary>
        /// جستجو یا ایجاد بیمار
        /// </summary>
        Task<ServiceResult<PatientDto>> FindOrCreatePatientAsync(string nationalCode, PatientCreateDto dtoIfNotExists);

        /// <summary>
        /// بارگذاری بیمه‌های بیمار
        /// </summary>
        Task<ServiceResult<InsuranceBundleDto>> LoadPatientInsurancesAsync(int patientId);

        #endregion

        #region Items & Calculation

        /// <summary>
        /// دریافت خدمات دپارتمان
        /// </summary>
        Task<ServiceResult<ServicePickListDto>> GetServicesForDeptAsync(int deptId);

        /// <summary>
        /// افزودن آیتم به پذیرش - سه محرک محاسبه
        /// </summary>
        Task<ServiceResult<AddItemResultDto>> AddItemAsync(int receptionId, int serviceId, int quantity, int year);

        /// <summary>
        /// ایجاد پیش‌نویس پذیرش
        /// </summary>
        Task<ServiceResult<CreateDraftResponse>> CreateDraftAsync(CreateDraftRequest request);

        /// <summary>
        /// 🏥 MEDICAL: حذف Draft ناقص (بدون خدمت)
        /// </summary>
        Task<ServiceResult> DeleteIncompleteDraftAsync(int receptionId);

        /// <summary>
        /// 🏥 MEDICAL: پاکسازی Draft های ناقص قدیمی (بیش از 24 ساعت)
        /// </summary>
        Task<ServiceResult<int>> CleanupOldIncompleteDraftsAsync(int hoursOld = 24);

        /// <summary>
        /// افزودن آیتم به پیش‌نویس
        /// </summary>
        Task<ServiceResult<ItemsAndTotalsDto>> AddItemAsync(AddItemRequest request);

        /// <summary>
        /// حذف آیتم از پیش‌نویس
        /// </summary>
        Task<ServiceResult<ItemsAndTotalsDto>> RemoveItemAsync(RemoveItemRequest request);

        #endregion

        #region Insurances & Finalize

        /// <summary>
        /// تنظیم بیمه‌های پذیرش
        /// </summary>
        Task<ServiceResult<bool>> SetInsurancesAsync(int receptionId, int? basePlanId, int? suppPlanId);

        /// <summary>
        /// تنظیم بیمه‌های پیش‌نویس
        /// </summary>
        Task<ServiceResult<ItemsAndTotalsDto>> SetInsurancesAsync(SetInsurancesRequest request);

        /// <summary>
        /// به‌روزرسانی پیش‌نویس پذیرش و بازمحاسبه مجموع‌ها
        /// </summary>
        Task<ServiceResult<ItemsAndTotalsDto>> UpdateDraftAsync(ClinicApp.Dtos.Reception.UpdateDraftRequest request);

        /// <summary>
        /// نهایی‌سازی با پرداخت POS
        /// </summary>
        Task<ServiceResult<FinalizeResultDto>> FinalizeWithPosAsync(int receptionId, PosPaymentDto pos);

        /// <summary>
        /// نهایی‌سازی با پرداخت نقدی
        /// </summary>
        Task<ServiceResult<FinalizeResultDto>> FinalizeWithCashAsync(int receptionId, CashPaymentDto cash);

        /// <summary>
        /// نهایی‌سازی با پرداخت POS (جدید)
        /// </summary>
        Task<ServiceResult<FinalizeResponse>> FinalizePosAsync(FinalizePosRequest request);

        /// <summary>
        /// نهایی‌سازی با پرداخت نقدی (جدید)
        /// </summary>
        Task<ServiceResult<FinalizeResponse>> FinalizeCashAsync(FinalizeCashRequest request);

        #endregion

        #region Coverage & Price Preview

        /// <summary>
        /// دریافت جزئیات پوشش بیمه (پایه + تکمیلی + مؤثر)
        /// </summary>
        Task<ServiceResult<Controllers.Api.InsuranceCoverageDto>> GetInsuranceCoverageAsync(int patientId, int? basePlanId, int? suppPlanId);

        /// <summary>
        /// پیش‌نمایش قیمت خدمت (بدون persist)
        /// </summary>
        Task<ServiceResult<Controllers.Api.PricePreviewResultDto>> PreviewItemPriceAsync(Controllers.Api.PricePreviewRequestDto request);

        #endregion
    }
}
