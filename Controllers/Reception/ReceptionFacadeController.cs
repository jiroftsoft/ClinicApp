using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// Controller برای ReceptionFacade - API-محور و اتمیک
    /// 
    /// مسئولیت: ارائه API تمیز برای ماژول پذیرش
    /// هدف: استفاده از Facade Pattern برای هماهنگی سرویس‌ها
    /// </summary>
    [Authorize]
    [ValidateAntiForgeryToken]
    public class ReceptionFacadeController : Controller
    {
        #region Dependencies

        private readonly IReceptionFacade _receptionFacade;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public ReceptionFacadeController(IReceptionFacade receptionFacade, ILogger logger)
        {
            _receptionFacade = receptionFacade ?? throw new ArgumentNullException(nameof(receptionFacade));
            _logger = logger.ForContext<ReceptionFacadeController>();
        }

        #endregion

        #region Loaders

        /// <summary>
        /// بارگذاری اولیه فرم پذیرش
        /// GET: /ReceptionFacade/LoadInitial?clinicId=1&deptId=2
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> LoadInitial(int clinicId, int? deptId = null)
        {
            try
            {
                _logger.Information("🏥 API: بارگذاری اولیه فرم پذیرش - ClinicId: {ClinicId}, DeptId: {DeptId}", clinicId, deptId);

                var result = await _receptionFacade.LoadInitialAsync(clinicId, deptId);
                
                if (result.Success)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return Json(ServiceResult<ReceptionLoadDto>.Failed(result.Message), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ API: خطا در بارگذاری اولیه فرم پذیرش");
                return Json(ServiceResult<ReceptionLoadDto>.Failed("خطا در بارگذاری اولیه فرم پذیرش"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// جستجو یا ایجاد بیمار
        /// POST: /ReceptionFacade/PatientLookup
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> PatientLookup(string nationalCode, PatientCreateDto dtoIfNotExists = null)
        {
            try
            {
                _logger.Information("🏥 API: جستجو یا ایجاد بیمار - NationalCode: {NationalCode}", nationalCode);

                var result = await _receptionFacade.FindOrCreatePatientAsync(nationalCode, dtoIfNotExists);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ API: خطا در جستجو یا ایجاد بیمار");
                return Json(ServiceResult<PatientDto>.Failed("خطا در جستجو یا ایجاد بیمار"));
            }
        }

        /// <summary>
        /// بارگذاری بیمه‌های بیمار
        /// GET: /ReceptionFacade/PatientInsurances?patientId=1
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> PatientInsurances(int patientId)
        {
            try
            {
                _logger.Information("🏥 API: بارگذاری بیمه‌های بیمار - PatientId: {PatientId}", patientId);

                var result = await _receptionFacade.LoadPatientInsurancesAsync(patientId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ API: خطا در بارگذاری بیمه‌های بیمار");
                return Json(ServiceResult<InsuranceBundleDto>.Failed("خطا در بارگذاری بیمه‌های بیمار"), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Items & Calculation

        /// <summary>
        /// دریافت خدمات دپارتمان
        /// GET: /ReceptionFacade/Services?deptId=1
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Services(int deptId)
        {
            try
            {
                _logger.Information("🏥 API: دریافت خدمات دپارتمان - DeptId: {DeptId}", deptId);

                var result = await _receptionFacade.GetServicesForDeptAsync(deptId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ API: خطا در دریافت خدمات دپارتمان");
                return Json(ServiceResult<ServicePickListDto>.Failed("خطا در دریافت خدمات دپارتمان"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// افزودن آیتم به پذیرش
        /// POST: /ReceptionFacade/AddItem
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> AddItem(int receptionId, int serviceId, int quantity, int year)
        {
            try
            {
                _logger.Information("🏥 API: افزودن آیتم به پذیرش - ReceptionId: {ReceptionId}, ServiceId: {ServiceId}, Quantity: {Quantity}, Year: {Year}", 
                    receptionId, serviceId, quantity, year);

                var result = await _receptionFacade.AddItemAsync(receptionId, serviceId, quantity, year);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ API: خطا در افزودن آیتم به پذیرش");
                return Json(ServiceResult<AddItemResultDto>.Failed("خطا در افزودن آیتم به پذیرش"));
            }
        }

        #endregion

        #region Insurances & Finalize

        /// <summary>
        /// تنظیم بیمه‌های پذیرش
        /// POST: /ReceptionFacade/SetInsurances
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> SetInsurances(int receptionId, int? basePlanId, int? suppPlanId)
        {
            try
            {
                _logger.Information("🏥 API: تنظیم بیمه‌های پذیرش - ReceptionId: {ReceptionId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                    receptionId, basePlanId, suppPlanId);

                var result = await _receptionFacade.SetInsurancesAsync(receptionId, basePlanId, suppPlanId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ API: خطا در تنظیم بیمه‌های پذیرش");
                return Json(ServiceResult<bool>.Failed("خطا در تنظیم بیمه‌های پذیرش"));
            }
        }

        /// <summary>
        /// نهایی‌سازی با پرداخت POS
        /// POST: /ReceptionFacade/FinalizePos
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> FinalizePos(int receptionId, PosPaymentDto pos)
        {
            try
            {
                _logger.Information("🏥 API: نهایی‌سازی با پرداخت POS - ReceptionId: {ReceptionId}, Amount: {Amount}", 
                    receptionId, pos.Amount);

                var result = await _receptionFacade.FinalizeWithPosAsync(receptionId, pos);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ API: خطا در نهایی‌سازی با پرداخت POS");
                return Json(ServiceResult<FinalizeResultDto>.Failed("خطا در نهایی‌سازی با پرداخت POS"));
            }
        }

        /// <summary>
        /// نهایی‌سازی با پرداخت نقدی
        /// POST: /ReceptionFacade/FinalizeCash
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> FinalizeCash(int receptionId, CashPaymentDto cash)
        {
            try
            {
                _logger.Information("🏥 API: نهایی‌سازی با پرداخت نقدی - ReceptionId: {ReceptionId}, Amount: {Amount}", 
                    receptionId, cash.Amount);

                var result = await _receptionFacade.FinalizeWithCashAsync(receptionId, cash);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ API: خطا در نهایی‌سازی با پرداخت نقدی");
                return Json(ServiceResult<FinalizeResultDto>.Failed("خطا در نهایی‌سازی با پرداخت نقدی"));
            }
        }

        #endregion
    }
}
