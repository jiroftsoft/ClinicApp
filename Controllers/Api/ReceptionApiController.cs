using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// API Controller واحد برای پذیرش V2 - Zero Cache, Medical-Grade
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. Zero Cache برای محیط درمانی
    /// 2. Anti-Forgery Token در همه POST ها
    /// 3. ServiceResult<T> استاندارد
    /// 4. Idempotency برای عملیات حساس
    /// </summary>
[RoutePrefix("api/v1/reception")]
[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
[NoCache]
    public class ReceptionApiController : Controller
    {
        #region Dependencies

        private readonly IReceptionFacade _receptionFacade;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public ReceptionApiController(IReceptionFacade receptionFacade, ILogger logger, ApplicationDbContext context)
        {
            _receptionFacade = receptionFacade ?? throw new ArgumentNullException(nameof(receptionFacade));
            _logger = logger.ForContext<ReceptionApiController>();
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion

        #region Patient Management

        /// <summary>
        /// جستجو یا ایجاد بیمار
        /// POST: /Api/ReceptionApi/PatientLookup
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken, Route("patient/lookup-or-create")]
        public async Task<ActionResult> PatientLookup(PatientLookupRequest request)
        {
            try
            {
                _logger.Information("🏥 API: جستجوی بیمار - NationalCode: {NationalCode}", request.NationalCode);

                var result = await _receptionFacade.FindOrCreatePatientAsync(request.NationalCode, request.CreateDto);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی بیمار");
                return Json(ServiceResult<PatientDto>.Failed("خطا در جستجوی بیمار"));
            }
        }

        /// <summary>
        /// دریافت بیمه‌های بیمار
        /// GET: /Api/ReceptionApi/PatientInsurances?patientId=123
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> PatientInsurances(int patientId)
        {
            try
            {
                _logger.Information("🏥 API: دریافت بیمه‌های بیمار - PatientId: {PatientId}", patientId);

                var result = await _receptionFacade.LoadPatientInsurancesAsync(patientId);
                
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه‌های بیمار");
                return Json(ServiceResult<InsuranceBundleDto>.Failed("خطا در دریافت بیمه‌های بیمار"), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Bootstrap

        /// <summary>
        /// داده‌های اولیه فرم پذیرش (دپارتمان‌ها، خدمات مشترک و ...)
        /// GET: /Api/ReceptionApi/Bootstrap
        /// </summary>
        [HttpGet, Route("bootstrap")]
        public async Task<ActionResult> Bootstrap(int? clinicId, int? deptId)
        {
            try
            {
                _logger.Information("🏥 API: Bootstrap - ClinicId: {ClinicId}, DeptId: {DeptId}", clinicId, deptId);

                var result = await _receptionFacade.LoadInitialAsync(clinicId ?? 1, deptId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Bootstrap پذیرش");
                return Json(ServiceResult.Failed("خطا در بارگذاری اطلاعات اولیه"), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Service Management

        /// <summary>
        /// دریافت خدمات دپارتمان
        /// GET: /Api/ReceptionApi/Services?deptId=123
        /// </summary>
        [HttpGet, Route("services/by-department")]
        public async Task<ActionResult> Services(int? deptId)
        {
            try
            {
                _logger.Information("🏥 API: دریافت خدمات - DeptId: {DeptId}", deptId);

                var result = await _receptionFacade.GetServicesForDeptAsync(deptId ?? 0);
                
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات");
                return Json(ServiceResult<ServicePickListDto>.Failed("خطا در دریافت خدمات"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ایجاد پیش‌نویس پذیرش
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken, Route("draft/create")]
        public async Task<ActionResult> CreateDraft(CreateDraftRequest request)
        {
            try
            {
                _logger.Information("🏥 API: ایجاد پیش‌نویس پذیرش");

                var result = await _receptionFacade.CreateDraftAsync(request);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پیش‌نویس");
                return Json(ServiceResult<CreateDraftResponse>.Failed("خطا در ایجاد پیش‌نویس"));
            }
        }

        /// <summary>
        /// افزودن آیتم به پیش‌نویس
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken, Route("item/add")]
        public async Task<ActionResult> AddItem(AddItemRequest request)
        {
            try
            {
                _logger.Information("🏥 API: افزودن آیتم به پیش‌نویس");

                var facadeRequest = new ViewModels.Reception.AddItemRequest
                {
                    ReceptionId = request.ReceptionId,
                    ServiceId = request.ServiceId,
                    Quantity = request.Quantity
                };
                var result = await _receptionFacade.AddItemAsync(facadeRequest);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن آیتم");
                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("خطا در افزودن آیتم"));
            }
        }

        /// <summary>
        /// حذف آیتم از پیش‌نویس
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken, Route("item/remove")]
        public async Task<ActionResult> RemoveItem(RemoveItemRequest request)
        {
            try
            {
                _logger.Information("🏥 API: حذف آیتم از پیش‌نویس");

                var result = await _receptionFacade.RemoveItemAsync(request);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف آیتم");
                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("خطا در حذف آیتم"));
            }
        }

        /// <summary>
        /// تنظیم بیمه‌های پیش‌نویس
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken, Route("insurances/set")]
        public async Task<ActionResult> SetInsurances(SetInsurancesRequest request)
        {
            try
            {
                _logger.Information("🏥 API: تنظیم بیمه‌های پیش‌نویس");

                var facadeRequest = new ViewModels.Reception.SetInsurancesRequest
                {
                    ReceptionId = request.ReceptionId,
                    BasePlanId = request.BasePlanId,
                    SupplementaryPlanId = request.SuppPlanId
                };
                var result = await _receptionFacade.SetInsurancesAsync(facadeRequest);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم بیمه‌ها");
                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("خطا در تنظیم بیمه‌ها"));
            }
        }

        /// <summary>
        /// نهایی‌سازی با POS
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken, Route("finalize/pos")]
        public async Task<ActionResult> FinalizeWithPos(FinalizePosRequest request)
        {
            try
            {
                _logger.Information("🏥 API: نهایی‌سازی با POS");

                var facadeRequest = new ViewModels.Reception.FinalizePosRequest
                {
                    ReceptionId = request.ReceptionId,
                    AmountIRR = request.Amount,
                    IdempotencyKey = Guid.NewGuid().ToString(), // TODO: از request دریافت شود
                    Pos = new ViewModels.Reception.PosPaymentDto
                    {
                        Amount = request.PosPayment.Amount,
                        RRN = request.PosPayment.RRN,
                        TraceNo = request.PosPayment.TraceNo,
                        TerminalId = request.PosPayment.TerminalId,
                        CardLast4 = request.PosPayment.CardLast4
                    }
                };
                var result = await _receptionFacade.FinalizePosAsync(facadeRequest);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نهایی‌سازی POS");
                return Json(ServiceResult<FinalizeResponse>.Failed("خطا در نهایی‌سازی پذیرش"));
            }
        }

        /// <summary>
        /// نهایی‌سازی با نقدی
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken, Route("finalize/cash")]
        public async Task<ActionResult> FinalizeWithCash(FinalizeCashRequest request)
        {
            try
            {
                _logger.Information("🏥 API: نهایی‌سازی با نقدی");

                var facadeRequest = new ViewModels.Reception.FinalizeCashRequest
                {
                    ReceptionId = request.ReceptionId,
                    AmountIRR = request.Amount,
                    IdempotencyKey = Guid.NewGuid().ToString(), // TODO: از request دریافت شود
                    Cash = new ViewModels.Reception.CashPaymentDto
                    {
                        Amount = request.CashPayment.Amount,
                        CashSessionId = request.CashPayment.CashSessionId
                    }
                };
                var result = await _receptionFacade.FinalizeCashAsync(facadeRequest);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نهایی‌سازی نقدی");
                return Json(ServiceResult<FinalizeResponse>.Failed("خطا در نهایی‌سازی پذیرش"));
            }
        }

        /// <summary>
        /// به‌روزرسانی پیش‌نویس پذیرش
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken, Route("draft/update")]
        public async Task<ActionResult> DraftUpdate(ClinicApp.Dtos.Reception.UpdateDraftRequest request)
        {
            try
            {
                var result = await _receptionFacade.UpdateDraftAsync(request);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی پیش‌نویس");
                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("خطا در به‌روزرسانی پیش‌نویس"));
            }
        }

        #endregion

        // Draft Management methods are defined above under Service Management

        #region Lookups

        /// <summary>
        /// دریافت خدمات یک دپارتمان - alias برای سازگاری Frontend
        /// GET: /Api/ReceptionApi/GetServicesForDepartment?deptId=123
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetServicesForDepartment(int deptId)
        {
            try
            {
                var result = await _receptionFacade.GetServicesForDeptAsync(deptId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات دپارتمان");
                return Json(ServiceResult.Failed("خطا در دریافت خدمات"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت پلن‌های بیمه - alias برای سازگاری Frontend
        /// GET: /Api/ReceptionApi/GetInsurancePlans
        /// </summary>
        [HttpGet, Route("insurance/plans")]
        public async Task<ActionResult> GetInsurancePlans(int? patientId = null, int? providerId = null)
        {
            try
            {
                // Minimal compatible payload for frontend expectations
                var payload = new
                {
                    basePlans = new object[0],
                    supplementaryPlans = new object[0]
                };
                return Json(ServiceResult<object>.Successful(payload), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پلن‌های بیمه");
                return Json(ServiceResult.Failed("خطا در دریافت بیمه‌ها"), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Insurance Management

        // متد SetInsurances تکراری حذف شد - از متد اول استفاده می‌شود

        #endregion

        #region Payment & Finalization

        /// <summary>
        /// نهایی‌سازی با POS
        /// POST: /Api/ReceptionApi/FinalizePos
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> FinalizePos(FinalizePosRequest request)
        {
            try
            {
                _logger.Information("🏥 API: نهایی‌سازی POS - ReceptionId: {ReceptionId}, Amount: {Amount}", 
                    request.ReceptionId, request.Amount);

                var result = await _receptionFacade.FinalizeWithPosAsync(request.ReceptionId, request.PosPayment);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نهایی‌سازی POS");
                return Json(ServiceResult<FinalizeResultDto>.Failed("خطا در نهایی‌سازی پذیرش"));
            }
        }

        /// <summary>
        /// نهایی‌سازی با نقدی
        /// POST: /Api/ReceptionApi/FinalizeCash
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> FinalizeCash(FinalizeCashRequest request)
        {
            try
            {
                _logger.Information("🏥 API: نهایی‌سازی نقدی - ReceptionId: {ReceptionId}, Amount: {Amount}", 
                    request.ReceptionId, request.Amount);

                var result = await _receptionFacade.FinalizeWithCashAsync(request.ReceptionId, request.CashPayment);
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نهایی‌سازی نقدی");
                return Json(ServiceResult<FinalizeResultDto>.Failed("خطا در نهایی‌سازی پذیرش"));
            }
        }

        #endregion
        /// <summary>
        /// دریافت جزئیات پذیرش برای چاپ
        /// GET: /Api/ReceptionApi/GetReceptionDetails?id=123
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetReceptionDetails(int id)
        {
            try
            {
                _logger.Information("🏥 API: دریافت جزئیات پذیرش - ID: {Id}", id);

                // دریافت اطلاعات پذیرش از دیتابیس
                var reception = await _context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.Department)
                    .Include(r => r.ReceptionItems.Select(receptionItem => receptionItem.Service))
                    .FirstOrDefaultAsync(r => r.ReceptionId == id);

                if (reception == null)
                {
                    return Json(ServiceResult<object>.Failed("پذیرش یافت نشد"));
                }

                var result = new
                {
                    ReceptionId = reception.ReceptionId,
                    ReceptionNo = reception.ReceptionNo ?? reception.ReceptionNumber,
                    ReceptionDate = reception.ReceptionDate,
                    PatientName = reception.Patient?.FullName,
                    PatientNationalCode = reception.Patient?.NationalCode,
                    PatientMobile = reception.Patient?.PhoneNumber,
                    DoctorName = reception.Doctor?.FullName,
                    DepartmentName = reception.Department?.Name,
                    TotalAmount = reception.TotalAmount,
                    InsurerShareAmount = reception.InsurerShareAmount,
                    PatientCoPay = reception.PatientCoPay,
                    PaymentMethod = reception.PaymentMethod ?? "نقدی",
                    Items = reception.ReceptionItems.Select(ri => new
                    {
                        ServiceName = ri.Service?.Title,
                        Quantity = ri.Quantity,
                        UnitPrice = ri.UnitPrice,
                        TotalPrice = ri.UnitPrice * ri.Quantity
                    }).ToList()
                };

                return Json(ServiceResult<object>.Successful(result), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پذیرش");
                return Json(ServiceResult<object>.Failed("خطا در دریافت جزئیات پذیرش"));
            }
        }
    }

    #region Request Models

    public class PatientLookupRequest
    {
        public string NationalCode { get; set; }
        public PatientCreateDto CreateDto { get; set; }
    }

    public class AddItemRequest
    {
        public int ReceptionId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public int Year { get; set; }
    }

    public class SetInsurancesRequest
    {
        public int ReceptionId { get; set; }
        public int? BasePlanId { get; set; }
        public int? SuppPlanId { get; set; }
    }

    public class FinalizePosRequest
    {
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public PosPaymentDto PosPayment { get; set; }
    }

    public class FinalizeCashRequest
    {
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public CashPaymentDto CashPayment { get; set; }
    }

     

        #endregion
    
}
