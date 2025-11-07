using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text.RegularExpressions;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Enums;
using ClinicApp.Extensions;
using ClinicApp.Services.Reception;
using Serilog;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// API Controller Legacy برای پذیرش - برای Fallback
    /// 
    /// این Controller برای مسیرهای /Api/ReceptionApi/* استفاده می‌شود
    /// برای v1 API از ReceptionApiV1Controller استفاده کنید
    /// </summary>
    [RoutePrefix("Api/ReceptionApi")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [NoCache]
    public class ReceptionApiController : Controller
    {
        #region Dependencies

        private readonly IReceptionFacade _receptionFacade;
        private readonly ReceptionFacade _receptionFacadeImpl; // برای دسترسی به متدهای غیر-interface
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        public ReceptionApiController(IReceptionFacade receptionFacade, ILogger logger, ApplicationDbContext context)
        {
            _receptionFacade = receptionFacade ?? throw new ArgumentNullException(nameof(receptionFacade));
            _receptionFacadeImpl = receptionFacade as ReceptionFacade; // Cast برای دسترسی به متدهای غیر-interface
            _logger = logger.ForContext<ReceptionApiController>();
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion

        #region Patient Management

        /// <summary>
        /// جستجو یا ایجاد بیمار - بازگشت اطلاعات کامل هویتی + بیمه‌ها
        /// POST: /Api/ReceptionApi/PatientLookup (Legacy)
        /// POST: /Api/ReceptionApi/patient/lookup-or-create (Alternative)
        /// </summary>
        [HttpPost]
        [Route("PatientLookup")]
        [Route("patient/lookup-or-create")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> PatientLookup(PatientLookupRequest request)
        {
            try
            {
                _logger.Information("🏥 API: جستجوی بیمار - NationalCode: {NationalCode}", request.NationalCode);

                // 1) یافتن بیمار
                var patientResult = await _receptionFacade.FindOrCreatePatientAsync(request.NationalCode, request.CreateDto);
                if (!patientResult.Success || patientResult.Data == null)
                {
                    return Json(ServiceResult<PatientLookupResponseDto>.Failed(patientResult.Message ?? "بیمار یافت نشد.", "NOT_FOUND"));
                }

                var patientDto = patientResult.Data;
                if (patientDto.PatientId <= 0)
                {
                    return Json(ServiceResult<PatientLookupResponseDto>.Failed("بیمار یافت نشد.", "NOT_FOUND"));
                }

                // 2) دریافت اطلاعات کامل بیمار از دیتابیس
                var patient = await _context.Patients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PatientId == patientDto.PatientId && !p.IsDeleted);

                if (patient == null)
                {
                    return Json(ServiceResult<PatientLookupResponseDto>.Failed("بیمار یافت نشد.", "NOT_FOUND"));
                }

                // 3) بیمه‌های انتسابی (پایه/تکمیلی) برای این بیمار
                InsuranceSelectionDto assignedInsurances = null;
                if (_receptionFacadeImpl != null)
                {
                    assignedInsurances = await _receptionFacadeImpl.GetAssignedInsurancesForPatient(patient.PatientId);
                }
                else
                {
                    // Fallback: استفاده از LoadPatientInsurancesAsync و تبدیل به InsuranceSelectionDto
                    var insuranceResult = await _receptionFacade.LoadPatientInsurancesAsync(patient.PatientId);
                    if (insuranceResult.Success && insuranceResult.Data != null)
                    {
                        var bundle = insuranceResult.Data;
                        assignedInsurances = new InsuranceSelectionDto
                        {
                            BasePlanId = bundle.BaseInsurances.FirstOrDefault()?.InsuranceId,
                            SupplementaryPlanId = bundle.SupplementaryInsurances.FirstOrDefault()?.InsuranceId
                        };
                    }
                    else
                    {
                        assignedInsurances = new InsuranceSelectionDto();
                    }
                }

                // 4) ساخت DTO پاسخ
                var responseDto = new PatientLookupResponseDto
                {
                    Identity = new PatientIdentityDto
                    {
                        PatientId = patient.PatientId,
                        NationalCode = patient.NationalCode,
                        FirstName = patient.FirstName,
                        LastName = patient.LastName,
                        FatherName = patient.FatherName,
                        Mobile = patient.PhoneNumber, // PhoneNumber به عنوان Mobile
                        Phone = null, // اگر فیلد جداگانه نیاز است
                        Address = patient.Address,
                        Gender = patient.Gender.ToString(),
                        BirthDateShamsi = patient.BirthDate?.ToPersianDate() ?? string.Empty
                    },
                    Insurance = assignedInsurances
                };

                return Json(ServiceResult<PatientLookupResponseDto>.Successful(responseDto, "اطلاعات بیمار و بیمه بارگذاری شد."));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی بیمار");
                return Json(ServiceResult<PatientLookupResponseDto>.Failed("خطا در جستجوی بیمار: " + ex.Message));
            }
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات پایه بیمار (در فرم پذیرش)
        /// POST: /api/v1/reception/patient/update-basic
        /// </summary>
        [HttpPost, Route("patient/update-basic"), ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> UpdatePatientBasic(PatientUpdateBasicRequest request)
        {
            try
            {
                _logger.Information("🏥 API: به‌روزرسانی اطلاعات بیمار - PatientId: {PatientId}", request.PatientId);

                var userId = User?.Identity?.Name ?? "system";

                // دریافت بیمار از دیتابیس
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PatientId == request.PatientId && !p.IsDeleted);

                if (patient == null)
                {
                    return Json(ServiceResult<PatientIdentityDto>.Failed("بیمار یافت نشد.", "NOT_FOUND"));
                }

                // اعتبارسنجی‌های پایه
                if (string.IsNullOrWhiteSpace(request.FirstName))
                {
                    return Json(ServiceResult<PatientIdentityDto>.Failed("نام الزامی است.", "VALIDATION"));
                }

                if (string.IsNullOrWhiteSpace(request.LastName))
                {
                    return Json(ServiceResult<PatientIdentityDto>.Failed("نام خانوادگی الزامی است.", "VALIDATION"));
                }

                if (!string.IsNullOrWhiteSpace(request.Mobile) && !Regex.IsMatch(request.Mobile, @"^09\d{9}$"))
                {
                    return Json(ServiceResult<PatientIdentityDto>.Failed("شماره موبایل نامعتبر است. باید 11 رقم و با 09 شروع شود.", "VALIDATION"));
                }

                // تبدیل تاریخ شمسی به میلادی
                DateTime? birthDate = null;
                if (!string.IsNullOrWhiteSpace(request.BirthDateShamsi))
                {
                    try
                    {
                        birthDate = request.BirthDateShamsi.FromFaDate();
                        if (!birthDate.HasValue)
                        {
                            return Json(ServiceResult<PatientIdentityDto>.Failed("تاریخ تولد نامعتبر است.", "VALIDATION"));
                        }
                    }
                    catch
                    {
                        return Json(ServiceResult<PatientIdentityDto>.Failed("تاریخ تولد نامعتبر است.", "VALIDATION"));
                    }
                }

                // تبدیل جنسیت
                Gender gender = patient.Gender; // پیش‌فرض: جنسیت قبلی
                if (!string.IsNullOrWhiteSpace(request.Gender))
                {
                    if (Enum.TryParse<Gender>(request.Gender, true, out var parsedGender))
                    {
                        gender = parsedGender;
                    }
                }

                // اعمال تغییرات مجاز
                patient.FirstName = request.FirstName?.Trim();
                patient.LastName = request.LastName?.Trim();
                patient.FatherName = request.FatherName?.Trim();
                patient.PhoneNumber = request.Mobile?.Trim(); // PhoneNumber به عنوان Mobile
                patient.Address = request.Address?.Trim();
                patient.Gender = gender;
                patient.BirthDate = birthDate;

                patient.UpdatedAt = DateTime.Now;
                patient.UpdatedByUserId = userId;

                await _context.SaveChangesAsync();

                // بازگرداندن DTO تازه برای همسان‌سازی UI
                var updatedDto = new PatientIdentityDto
                {
                    PatientId = patient.PatientId,
                    NationalCode = patient.NationalCode,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    FatherName = patient.FatherName,
                    Mobile = patient.PhoneNumber,
                    Phone = null,
                    Address = patient.Address,
                    Gender = patient.Gender.ToString(),
                    BirthDateShamsi = patient.BirthDate?.ToPersianDate() ?? string.Empty
                };

                return Json(ServiceResult<PatientIdentityDto>.Successful(updatedDto, "اطلاعات به‌روزرسانی شد."));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اطلاعات بیمار");
                return Json(ServiceResult<PatientIdentityDto>.Failed("به‌روزرسانی ناموفق بود: " + ex.Message));
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
        /// POST: /api/v1/reception/draft/create
        /// </summary>
        [HttpPost, Route("draft/create"), ValidateAntiForgeryToken]
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
        /// POST: /api/v1/reception/item/add
        /// </summary>
        [HttpPost, Route("item/add"), ValidateAntiForgeryToken]
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
        /// POST: /api/v1/reception/item/remove
        /// </summary>
        [HttpPost, Route("item/remove"), ValidateAntiForgeryToken]
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
        /// POST: /api/v1/reception/insurances/set
        /// </summary>
        [HttpPost, Route("insurances/set"), ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> SetInsurances(SetInsurancesRequest request)
        {
            try
            {
                _logger.Information("🏥 API: تنظیم بیمه‌های پیش‌نویس - ReceptionId: {ReceptionId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
                    request?.ReceptionId, request?.BasePlanId, request?.SupplementaryPlanId ?? request?.SuppPlanId);

                // اعتبارسنجی اولیه
                if (request == null || request.ReceptionId <= 0)
                {
                    return Json(ServiceResult<ItemsAndTotalsDto>.Failed("درخواست نامعتبر است. ReceptionId الزامی است.", "VALIDATION"));
                }

                // اعتبارسنجی Reception وجود دارد
                var receptionExists = await _context.Receptions
                    .AnyAsync(r => r.ReceptionId == request.ReceptionId && !r.IsDeleted);
                
                if (!receptionExists)
                {
                    return Json(ServiceResult<ItemsAndTotalsDto>.Failed("پذیرش یافت نشد.", "NOT_FOUND"));
                }

                var facadeRequest = new ViewModels.Reception.SetInsurancesRequest
                {
                    ReceptionId = request.ReceptionId,
                    BasePlanId = request.BasePlanId,
                    SupplementaryPlanId = request.SupplementaryPlanId ?? request.SuppPlanId // پشتیبانی از هر دو نام
                };
                
                var result = await _receptionFacade.SetInsurancesAsync(facadeRequest);
                
                // لاگ نتیجه
                if (result.Success)
                {
                    _logger.Information("✅ API: بیمه‌های پیش‌نویس با موفقیت تنظیم شد - ReceptionId: {ReceptionId}", request.ReceptionId);
                }
                else
                {
                    _logger.Warning("⚠️ API: تنظیم بیمه‌های پیش‌نویس ناموفق - ReceptionId: {ReceptionId}, Error: {Error}",
                        request.ReceptionId, result.Message);
                }
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ API: خطا در تنظیم بیمه‌ها - ReceptionId: {ReceptionId}", request?.ReceptionId);
                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("خطا در تنظیم بیمه‌ها: " + ex.Message, "UNHANDLED"));
            }
        }

        /// <summary>
        /// نهایی‌سازی با POS
        /// POST: /api/v1/reception/finalize/pos
        /// </summary>
        [HttpPost, Route("finalize/pos"), ValidateAntiForgeryToken]
        public async Task<ActionResult> FinalizeWithPos(FinalizePosRequest request)
        {
            try
            {
                _logger.Information("🏥 API: نهایی‌سازی با POS");

                var facadeRequest = new ViewModels.Reception.FinalizePosRequest
                {
                    ReceptionId = request.ReceptionId,
                    AmountIRR = request.Amount,
                    IdempotencyKey = request.IdempotencyKey ?? Guid.NewGuid().ToString(),
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
        /// POST: /api/v1/reception/finalize/cash
        /// </summary>
        [HttpPost, Route("finalize/cash"), ValidateAntiForgeryToken]
        public async Task<ActionResult> FinalizeWithCash(FinalizeCashRequest request)
        {
            try
            {
                _logger.Information("🏥 API: نهایی‌سازی با نقدی");

                var facadeRequest = new ViewModels.Reception.FinalizeCashRequest
                {
                    ReceptionId = request.ReceptionId,
                    AmountIRR = request.Amount,
                    IdempotencyKey = request.IdempotencyKey ?? Guid.NewGuid().ToString(),
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
        /// POST: /api/v1/reception/draft/update
        /// </summary>
        [HttpPost, Route("draft/update"), ValidateAntiForgeryToken]
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
                    .Include(reception => reception.ActivePatientInsurance)
                    .FirstOrDefaultAsync(r => r.ReceptionId == id);

                if (reception == null)
                {
                    return Json(ServiceResult<object>.Failed("پذیرش یافت نشد"), JsonRequestBehavior.AllowGet);
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
                    Items = reception.ReceptionItems.Where(ri => !ri.IsDeleted).Select(ri => new
                    {
                        ServiceName = ri.Service?.Title,
                        Quantity = ri.Quantity,
                        UnitPrice = ri.UnitPrice,
                        TotalPrice = ri.UnitPrice * ri.Quantity,
                        SnapshotJson = ri.SnapshotJson,
                        PatientShareAmount = ri.PatientShareAmount,
                        InsurerShareAmount = ri.InsurerShareAmount
                    }).ToList(),
                    BasePlanId = reception.BasePlanId,
                    SupplementaryPlanId = reception.SupplementaryPlanId,
                    SupplementaryInsuranceName = reception.SupplementaryPlanId.HasValue ? 
                        (await _context.InsurancePlans
                            .Where(p => p.InsurancePlanId == reception.SupplementaryPlanId.Value)
                            .Select(p => p.Name)
                            .FirstOrDefaultAsync()) : null,
                    // 🏥 MEDICAL: دریافت اطلاعات بیمه تکمیلی از PatientInsurance
                    // اگر ActivePatientInsurance وجود دارد و InsurancePlanId آن با SupplementaryPlanId مطابقت دارد
                    SupplementaryPolicyNumber = reception.SupplementaryPlanId.HasValue && reception.ActivePatientInsurance != null ?
                        (await _context.PatientInsurances
                            .Where(pi => pi.PatientId == reception.PatientId && 
                                        pi.InsurancePlanId == reception.SupplementaryPlanId.Value &&
                                        pi.IsActive && !pi.IsDeleted)
                            .Select(pi => pi.PolicyNumber)
                            .FirstOrDefaultAsync()) : null,
                    SupplementaryCardNumber = reception.SupplementaryPlanId.HasValue && reception.ActivePatientInsurance != null ?
                        (await _context.PatientInsurances
                            .Where(pi => pi.PatientId == reception.PatientId && 
                                        pi.InsurancePlanId == reception.SupplementaryPlanId.Value &&
                                        pi.IsActive && !pi.IsDeleted)
                            .Select(pi => pi.CardNumber)
                            .FirstOrDefaultAsync()) : null
                };

                return Json(ServiceResult<object>.Successful(result), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پذیرش");
                return Json(ServiceResult<object>.Failed("خطا در دریافت جزئیات پذیرش"), JsonRequestBehavior.AllowGet);
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
        public int? SuppPlanId { get; set; } // Legacy name
        public int? SupplementaryPlanId { get; set; } // Preferred name
    }

    public class FinalizePosRequest
    {
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public string IdempotencyKey { get; set; }
        public PosPaymentDto PosPayment { get; set; }
    }

    public class FinalizeCashRequest
    {
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public string IdempotencyKey { get; set; }
        public CashPaymentDto CashPayment { get; set; }
    }

     

        #endregion
    
}
