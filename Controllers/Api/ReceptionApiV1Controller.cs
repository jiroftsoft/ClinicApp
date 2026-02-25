using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text.RegularExpressions;
using ClinicApp.Controllers.ReceptionV2;
using ClinicApp.Helpers;
using ClinicApp.Filters;
using ClinicApp.Interfaces.Finance;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using ClinicApp.Models.DTOs.Insurance;
using ClinicApp.Extensions;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// Controller V1 برای API پذیرش - حداقل لازم + Health & Draft/Create
    /// 
    /// این کنترلر فقط برای مسیرهای /api/v1/reception/ است تا v1 واقعی داشته باشیم
    /// فقط کاربران با نقش Admin یا Receptionist (منشی).
    /// </summary>
    [RoutePrefix("api/v1/reception")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [ReceptionV2Controller.NoCache]
    public class ReceptionApiV1Controller : Controller
    {
        #region Dependencies

        private readonly IFinancialYearService _fy;
        private readonly IReceptionFacade _facade;
        private readonly IReceptionPricingService _pricing;
        private readonly IInsuranceStatusCheckerService _insuranceStatusChecker; // ✅ کامپوننت قابل استفاده مجدد
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor

        /// <summary>
        /// DI ctor
        /// </summary>
        public ReceptionApiV1Controller(
            IFinancialYearService fy,
            IReceptionFacade facade,
            IReceptionPricingService pricing,
            IInsuranceStatusCheckerService insuranceStatusChecker,
            ILogger logger,
            ApplicationDbContext context)
        {
            _fy = fy ?? throw new ArgumentNullException(nameof(fy));
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
            _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
            _insuranceStatusChecker = insuranceStatusChecker ?? throw new ArgumentNullException(nameof(insuranceStatusChecker));
            _logger = logger?.ForContext<ReceptionApiV1Controller>() ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ✅ Production: Fallback ctor حذف شد تا در صورت نقص DI خطا در startup رخ دهد، نه در اولین درخواست با 500.
        // تمام وابستگی‌ها باید از DI تزریق شوند (UnityConfig).

        #endregion

        #region Actions

        /// <summary>
        /// GET /api/v1/reception/health
        /// Health check endpoint
        /// </summary>
        [HttpGet, Route("health")]
        public ActionResult Health()
        {
            try
            {
                _logger?.Information("🏥 V1 API: Health check");
                return Json(ServiceResult.Successful("ok"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در Health check");
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// GET /api/v1/reception/bootstrap
        /// داده‌های اولیه فرم پذیرش (دپارتمان‌ها، خدمات مشترک و ...)
        /// </summary>
        [HttpGet, Route("bootstrap")]
        public async System.Threading.Tasks.Task<ActionResult> Bootstrap(int? clinicId, int? deptId)
        {
            try
            {
                // ✅ Default ClinicId = 1 (Shafa) if not provided or invalid
                var cid = (clinicId.HasValue && clinicId.Value > 0) ? clinicId.Value : 1;
                
                _logger?.Information("🏥 V1 API: Bootstrap - ClinicId: {ClinicId} (default: {DefaultClinicId}), DeptId: {DeptId}", 
                    clinicId, cid, deptId);

                if (_facade != null)
                {
                    var result = await _facade.LoadInitialAsync(cid, deptId);
                    
                    _logger?.Information("🔍 V1 API: LoadInitialAsync result - Success: {Success}, HasData: {HasData}, DoctorsCount: {DoctorsCount}, DepartmentsCount: {DepartmentsCount}, ClinicsCount: {ClinicsCount}", 
                        result.Success, result.Data != null, 
                        result.Data?.Doctors?.Count ?? 0,
                        result.Data?.Departments?.Count ?? 0,
                        result.Data?.Clinics?.Count ?? 0);
                    
                    if (result.Success && result.Data != null)
                    {
                        var payload = new
                        {
                            Clinics = result.Data.Clinics ?? Enumerable.Empty<ViewModels.Reception.ClinicDto>().ToList(),
                            Departments = result.Data.Departments ?? Enumerable.Empty<ViewModels.Reception.DepartmentDto>().ToList(),
                            Services = result.Data.Services ?? Enumerable.Empty<ViewModels.Reception.ServiceDto>().ToList(),
                            SharedServices = result.Data.SharedServices ?? Enumerable.Empty<ViewModels.Reception.ServiceDto>().ToList(),
                            Doctors = result.Data.Doctors ?? Enumerable.Empty<ViewModels.Reception.DoctorDto>().ToList(),
                            FactorSetting = result.Data.FactorSetting, // ✅ اضافه شد
                            FinancialYear = _fy?.GetCurrentYear() ?? DateTime.Now.Year
                        };
                        
                        _logger?.Information("🔍 V1 API: Payload created - Doctors: {DoctorsCount}, Departments: {DepartmentsCount}, Clinics: {ClinicsCount}", 
                            payload.Doctors.Count, payload.Departments.Count, payload.Clinics.Count);
                        
                        return Json(ServiceResult<object>.Successful(payload, result.Message ?? "عملیات با موفقیت انجام شد."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: LoadInitialAsync failed or returned null - Success: {Success}, Message: {Message}", 
                            result.Success, result.Message);
                    }
                }

                // Fallback: اسکلت حداقلی
                _logger?.Warning("⚠️ V1 API: Using fallback minimal payload - _facade is null or LoadInitialAsync failed");
                var minimalPayload = new
                {
                    Clinics = Enumerable.Empty<ViewModels.Reception.ClinicDto>().ToList(),
                    Departments = Enumerable.Empty<ViewModels.Reception.DepartmentDto>().ToList(),
                    Services = Enumerable.Empty<ViewModels.Reception.ServiceDto>().ToList(),
                    SharedServices = Enumerable.Empty<ViewModels.Reception.ServiceDto>().ToList(),
                    Doctors = Enumerable.Empty<ViewModels.Reception.DoctorDto>().ToList(),
                    FactorSetting = (ViewModels.Reception.FactorSettingDto)null, // ✅ اضافه شد
                    FinancialYear = _fy?.GetCurrentYear() ?? DateTime.Now.Year
                };
                return Json(ServiceResult<object>.Successful(minimalPayload, "عملیات با موفقیت انجام شد."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در Bootstrap - ClinicId: {ClinicId}, DeptId: {DeptId}, Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                    clinicId, deptId, ex.GetType().Name, ex.Message, ex.StackTrace);
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/draft/create
        /// ایجاد پیش‌نویس پذیرش
        /// </summary>
        [HttpPost, Route("draft/create")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> CreateDraft(ViewModels.Reception.CreateDraftRequest request)
        {
            try
            {
                // ✅ CRITICAL: جلوگیری از NullReferenceException در قلب سیستم
                if (request == null)
                {
                    _logger?.Warning("⚠️ V1 API: Create Draft - request is null");
                    return Json(ServiceResult.Failed("اطلاعات پذیرش ارسال نشده است.", "INVALID_REQUEST"));
                }

                _logger?.Information("🏥 V1 API: Create Draft - PatientId: {PatientId}, ClinicId: {ClinicId}, DeptId: {DeptId}, DoctorId: {DoctorId}",
                    request.PatientId, request.ClinicId, request.DepartmentId, request.DoctorId);

                if (_facade != null)
                {
                    var result = await _facade.CreateDraftAsync(request);
                    if (result.Success && result.Data != null)
                    {
                        _logger?.Information("✅ V1 API: Draft created successfully - ReceptionId: {ReceptionId}", result.Data.ReceptionId);
                        return Json(ServiceResult<object>.Successful(new { receptionId = result.Data.ReceptionId, status = result.Data.Status }, "پیش‌نویس با موفقیت ایجاد شد."));
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: Draft creation failed - {Error}", result.Message);
                        return Json(ServiceResult.Failed(result.Message ?? "خطا در ایجاد پیش‌نویس", result.Code ?? "CREATE_FAILED"));
                    }
                }

                _logger?.Warning("⚠️ V1 API: Facade not available");
                return Json(ServiceResult.Failed("سرویس پذیرش در دسترس نیست.", "SERVICE_UNAVAILABLE"));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در Create Draft");
#if DEBUG
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
#else
                return Json(ServiceResult.Failed("خطای غیرمنتظره رخ داد.", "UNHANDLED"));
#endif
            }
        }

        /// <summary>
        /// POST /api/v1/reception/draft/delete-incomplete
        /// حذف Draft ناقص (بدون خدمت)
        /// 
        /// این endpoint از query string (برای sendBeacon) و request body (برای AJAX) پشتیبانی می‌کند
        /// </summary>
        [HttpPost, Route("draft/delete-incomplete")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> DeleteIncompleteDraft(DeleteIncompleteDraftRequest request = null)
        {
            try
            {
                // ✅ اولویت 1: خواندن از Model Binding (برای AJAX)
                int receptionId = 0;
                string receptionIdFromQuery = null;
                
                if (request != null && request.ReceptionId > 0)
                {
                    receptionId = request.ReceptionId;
                    _logger?.Information("🏥 V1 API: ReceptionId از Model Binding: {ReceptionId}", receptionId);
                }

                // ✅ اولویت 2: خواندن از Query String (برای sendBeacon)
                if (receptionId <= 0)
                {
                    receptionIdFromQuery = Request.QueryString["receptionId"];
                    if (!string.IsNullOrWhiteSpace(receptionIdFromQuery))
                    {
                        if (int.TryParse(receptionIdFromQuery, out int parsedId))
                        {
                            receptionId = parsedId;
                            _logger?.Information("🏥 V1 API: ReceptionId از Query String: {ReceptionId}", receptionId);
                        }
                    }
                }

                // ✅ اولویت 3: خواندن از Request Body به صورت دستی (fallback)
                if (receptionId <= 0)
                {
                    try
                    {
                        _logger?.Information("🏥 V1 API: تلاش برای خواندن receptionId از Request Body (fallback)...");
                        _logger?.Information("🏥 V1 API: Request.ContentType: {ContentType}", Request.ContentType ?? "NULL");
                        _logger?.Information("🏥 V1 API: Request.HttpMethod: {Method}", Request.HttpMethod);
                        
                        // Reset InputStream position (ممکن است قبلاً خوانده شده باشد)
                        if (Request.InputStream.CanSeek)
                        {
                            Request.InputStream.Position = 0;
                        }
                        
                        string requestBody = null;
                        using (var reader = new System.IO.StreamReader(Request.InputStream, System.Text.Encoding.UTF8, true, 1024, true))
                        {
                            requestBody = reader.ReadToEnd();
                        }
                        
                        _logger?.Information("🏥 V1 API: Request Body length: {Length}", requestBody?.Length ?? 0);
                        _logger?.Information("🏥 V1 API: Request Body (first 200 chars): {Body}", 
                            requestBody?.Length > 200 ? requestBody.Substring(0, 200) : requestBody ?? "NULL");
                        
                        if (!string.IsNullOrWhiteSpace(requestBody))
                        {
                            var json = System.Web.Helpers.Json.Decode(requestBody);
                            if (json != null)
                            {
                                _logger?.Information("🏥 V1 API: JSON decoded successfully");
                                _logger?.Information("🏥 V1 API: json.receptionId: {ReceptionId}", json.receptionId ?? "NULL");
                                _logger?.Information("🏥 V1 API: json.ReceptionId: {ReceptionId}", json.ReceptionId ?? "NULL");
                                
                                receptionId = json.receptionId ?? json.ReceptionId ?? 0;
                                _logger?.Information("🏥 V1 API: receptionId extracted from body: {ReceptionId}", receptionId);
                            }
                            else
                            {
                                _logger?.Warning("⚠️ V1 API: JSON decode returned null");
                            }
                        }
                        else
                        {
                            _logger?.Warning("⚠️ V1 API: Request Body is null or empty");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.Warning(ex, "⚠️ V1 API: خطا در خواندن Request Body - {Message}", ex.Message);
                    }
                }

                _logger?.Information("🏥 V1 API: Delete Incomplete Draft - ReceptionId: {ReceptionId}, Method: {Method}, HasQueryString: {HasQueryString}", 
                    receptionId, Request.HttpMethod, !string.IsNullOrWhiteSpace(receptionIdFromQuery));

                if (receptionId <= 0)
                {
                    return Json(ServiceResult.Failed("شناسه پذیرش نامعتبر است.", "VALIDATION"), JsonRequestBehavior.AllowGet);
                }

                // ✅ بررسی Anti-Forgery Token (فقط برای AJAX، نه برای sendBeacon)
                // sendBeacon نمی‌تواند header بفرستد و از query string استفاده می‌کند
                // اگر receptionId از query string آمده، token را skip می‌کنیم
                var isSendBeaconRequest = !string.IsNullOrWhiteSpace(receptionIdFromQuery);
                
                if (!isSendBeaconRequest)
                {
                    // برای AJAX، token را بررسی کن
                    try
                    {
                        var token = Request.Headers["RequestVerificationToken"] ?? Request.Headers["X-RequestVerificationToken"];
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            _logger?.Warning("⚠️ V1 API: Anti-Forgery Token missing for AJAX request");
                            // برای sendBeacon که از query string استفاده می‌کند، token را skip می‌کنیم
                            // اما برای AJAX که از body استفاده می‌کند، token اجباری است
                            // در اینجا چون receptionId از query string نیست، پس AJAX است و باید token داشته باشد
                            return Json(ServiceResult.Failed("توکن امنیتی یافت نشد.", "ANTIFORGERY_MISSING"), JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch
                    {
                        // اگر خطا در بررسی token رخ داد، ادامه بده (برای sendBeacon)
                    }
                }
                else
                {
                    _logger?.Information("ℹ️ V1 API: sendBeacon request detected, skipping anti-forgery token validation");
                }

                if (_facade != null)
                {
                    var result = await _facade.DeleteIncompleteDraftAsync(receptionId);
                    if (result.Success)
                    {
                        _logger?.Information("✅ V1 API: Draft ناقص حذف شد - ReceptionId: {ReceptionId}", receptionId);
                        return Json(ServiceResult.Successful(result.Message ?? "پذیرش ناقص با موفقیت حذف شد."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: حذف Draft ناقص ناموفق - {Error}", result.Message);
                        return Json(ServiceResult.Failed(result.Message ?? "خطا در حذف پذیرش ناقص", result.Code ?? "DELETE_FAILED"), JsonRequestBehavior.AllowGet);
                    }
                }

                _logger?.Warning("⚠️ V1 API: Facade not available");
                return Json(ServiceResult.Failed("سرویس پذیرش در دسترس نیست.", "SERVICE_UNAVAILABLE"));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در حذف Draft ناقص");
#if DEBUG
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
#else
                return Json(ServiceResult.Failed("خطای غیرمنتظره رخ داد.", "UNHANDLED"));
#endif
            }
        }

        /// <summary>
        /// POST /api/v1/reception/draft/cleanup-pending
        /// پاکسازی Draft های Pending کاربر فعلی
        /// 
        /// این endpoint برای حذف Draft‌هایی استفاده می‌شود که کاربر ایجاد کرده ولی نهایی نکرده است
        /// مثلاً وقتی کاربر Draft ایجاد می‌کند و بدون کلیک روی "ذخیره و پذیرش" به صفحه لیست می‌رود
        /// </summary>
        [HttpPost, Route("draft/cleanup-pending")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> CleanupPendingDrafts()
        {
            try
            {
                _logger?.Information("🏥 V1 API: Cleanup Pending Drafts for current user");

                if (_facade != null)
                {
                    var result = await _facade.CleanupPendingDraftsForCurrentUserAsync();
                    if (result.Success)
                    {
                        _logger?.Information("✅ V1 API: {Count} Draft Pending حذف شد", result.Data);
                        return Json(ServiceResult<int>.Successful(result.Data, result.Message ?? "Draft های Pending با موفقیت حذف شدند."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: پاکسازی Draft های Pending ناموفق - {Error}", result.Message);
                        return Json(ServiceResult<int>.Failed(result.Message ?? "خطا در پاکسازی Draft های Pending", result.Code ?? "CLEANUP_FAILED"), JsonRequestBehavior.AllowGet);
                    }
                }

                _logger?.Warning("⚠️ V1 API: Facade not available");
                return Json(ServiceResult<int>.Failed("سرویس پذیرش در دسترس نیست.", "SERVICE_UNAVAILABLE"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در پاکسازی Draft های Pending");
#if DEBUG
                return Json(ServiceResult<int>.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"), JsonRequestBehavior.AllowGet);
#else
                return Json(ServiceResult<int>.Failed("خطای غیرمنتظره رخ داد.", "UNHANDLED"), JsonRequestBehavior.AllowGet);
#endif
            }
        }

        /// <summary>
        /// POST /api/v1/reception/patient/lookup-or-create
        /// جستجو یا ایجاد بیمار (Idempotent: هم lookup هم quick create)
        /// </summary>
        [HttpPost, Route("patient/lookup-or-create")]
        [ValidateAntiForgeryTokenOnPosts]
        public async System.Threading.Tasks.Task<ActionResult> PatientLookupOrCreate(PatientQuickCreateDto request)
        {
            try
            {
                // ✅ وقتی کلاینت با Content-Type: application/json ارسال می‌کند، در MVC مدل از بدنه بایند نمی‌شود؛ خواندن دستی
                if ((request == null || string.IsNullOrWhiteSpace(request.NationalCode)) &&
                    Request.ContentType != null && Request.ContentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string body = null;
                    try
                    {
                        if (Request.InputStream.CanSeek)
                            Request.InputStream.Position = 0;
                        using (var reader = new System.IO.StreamReader(Request.InputStream, System.Text.Encoding.UTF8, true, 1024, true))
                            body = reader.ReadToEnd();
                    }
                    catch (Exception ex)
                    {
                        _logger?.Warning(ex, "🏥 V1 API: خطا در خواندن Request Body برای Patient Lookup");
                    }
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        try
                        {
                            var json = System.Web.Helpers.Json.Decode(body);
                            if (json != null)
                            {
                                if (request == null) request = new PatientQuickCreateDto();
                                request.NationalCode = request.NationalCode ?? json.NationalCode ?? json.nationalCode;
                                request.FirstName = request.FirstName ?? json.FirstName ?? json.firstName;
                                request.LastName = request.LastName ?? json.LastName ?? json.lastName;
                                request.FatherName = request.FatherName ?? json.FatherName ?? json.fatherName;
                                request.Mobile = request.Mobile ?? json.Mobile ?? json.mobile;
                                request.Gender = request.Gender ?? json.Gender ?? json.gender;
                                request.BirthDateShamsi = request.BirthDateShamsi ?? json.BirthDateShamsi ?? json.birthDateShamsi;
                                request.Address = request.Address ?? json.Address ?? json.address;
                                if (json.BaseInsurancePlanId != null || json.baseInsurancePlanId != null)
                                    request.BaseInsurancePlanId = request.BaseInsurancePlanId ?? (int?)json.BaseInsurancePlanId ?? (int?)json.baseInsurancePlanId;
                                if (json.SupplementaryInsurancePlanId != null || json.supplementaryInsurancePlanId != null)
                                    request.SupplementaryInsurancePlanId = request.SupplementaryInsurancePlanId ?? (int?)json.SupplementaryInsurancePlanId ?? (int?)json.supplementaryInsurancePlanId;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.Warning(ex, "🏥 V1 API: خطا در پارس JSON برای Patient Lookup");
                        }
                    }
                }

                _logger?.Information("🏥 V1 API: Patient Lookup-Or-Create - NationalCode: {NationalCode}, HasQuickCreateData: {HasData}", 
                    request?.NationalCode, !string.IsNullOrWhiteSpace(request?.FirstName));

                // ✅ بررسی الزامات پایه
                if (string.IsNullOrWhiteSpace(request?.NationalCode))
                {
                    return Json(ServiceResult<PatientLookupResponseDto>
                        .Failed("کد ملی الزامی است.", ReceptionApiCodes.VALIDATION)
                        .WithValidationError("NationalCode", "کد ملی الزامی است."));
                }

                // ✅ HIS Production: نرمال‌سازی کد ملی (فارسی/عربی → انگلیسی + Trim) قبل از هر اعتبارسنجی و جستجو
                var normalizedNationalCode = (PersianNumberHelper.ToEnglishNumbers(request.NationalCode ?? "") ?? "").Trim();

                // ✅ بررسی اعتبار کد ملی (10 رقم) روی مقدار نرمال
                if (normalizedNationalCode.Length != 10 || !Regex.IsMatch(normalizedNationalCode, @"^\d{10}$"))
                {
                    return Json(ServiceResult<PatientLookupResponseDto>
                        .Failed("کد ملی باید 10 رقم عددی باشد.", ReceptionApiCodes.VALIDATION)
                        .WithValidationError("NationalCode", "کد ملی باید 10 رقم عددی باشد."));
                }

                // ✅ اعتبارسنجی کامل کد ملی ایرانی (الگوریتم استاندارد + رقم کنترل) روی مقدار نرمال
                var ncValidation = IranianNationalCodeValidator.Validate(normalizedNationalCode);
                if (!ncValidation.IsValid)
                {
                    _logger?.Warning("🏥 V1 API: کد ملی نامعتبر - NationalCode: {NationalCode}, Message: {Message}",
                        normalizedNationalCode, ncValidation.Message);
                    return Json(ServiceResult<PatientLookupResponseDto>
                        .Failed(ncValidation.Message ?? "کد ملی نامعتبر است.", ReceptionApiCodes.VALIDATION)
                        .WithValidationError("NationalCode", ncValidation.Message ?? "کد ملی نامعتبر است."));
                }

                // ✅ بررسی اینکه آیا این Lookup است یا Quick Create
                bool isQuickCreate = !string.IsNullOrWhiteSpace(request.FirstName) || !string.IsNullOrWhiteSpace(request.LastName);

                // ✅ اگر Quick Create است، فیلدهای الزامی را بررسی کن
                if (isQuickCreate)
                {
                    var validationErrors = new List<ValidationError>();

                    // بررسی ModelState برای فیلدهای دارای StringLength
                    if (!ModelState.IsValid)
                    {
                        var modelErrors = ModelState
                            .Where(ms => ms.Value.Errors.Any())
                            .SelectMany(ms => ms.Value.Errors.Select(e => new ValidationError(ms.Key, e.ErrorMessage)))
                            .ToList();
                        validationErrors.AddRange(modelErrors);
                    }

                    // ✅ اعتبارسنجی فیلدهای الزامی برای Quick Create
                    if (string.IsNullOrWhiteSpace(request.FirstName))
                    {
                        validationErrors.Add(new ValidationError("FirstName", "نام الزامی است."));
                    }

                    if (string.IsNullOrWhiteSpace(request.LastName))
                    {
                        validationErrors.Add(new ValidationError("LastName", "نام خانوادگی الزامی است."));
                    }

                    if (string.IsNullOrWhiteSpace(request.Mobile))
                    {
                        validationErrors.Add(new ValidationError("Mobile", "شماره موبایل الزامی است."));
                    }
                    else if (!System.Text.RegularExpressions.Regex.IsMatch(request.Mobile, @"^09\d{9}$"))
                    {
                        validationErrors.Add(new ValidationError("Mobile", "شماره موبایل باید 11 رقم و با 09 شروع شود."));
                    }

                    if (validationErrors.Any())
                    {
                        _logger?.Warning("🏥 V1 API: Quick Create validation failed - Errors: {Count}", validationErrors.Count);
                        
                        return Json(ServiceResult<PatientLookupResponseDto>
                            .FailedWithValidationErrors("اعتبارسنجی ناموفق", validationErrors)
                            .WithCode(ReceptionApiCodes.VALIDATION));
                    }
                }

                // استفاده مستقیم از ReceptionFacade
                if (_facade != null)
                {
                    var facadeImpl = _facade as Services.Reception.ReceptionFacade;
                    if (facadeImpl != null)
                    {
                        // اگر فقط کدملی آمده (Lookup فقط)
                        if (string.IsNullOrWhiteSpace(request.FirstName) && string.IsNullOrWhiteSpace(request.LastName))
                        {
                            var findResult = await facadeImpl.FindOrCreatePatientAsync(normalizedNationalCode, null);
                            if (findResult.Success && findResult.Data != null)
                            {
                                var patientDto = findResult.Data;
                                var patientId = patientDto.PatientId;
                                
                                if (patientId > 0)
                                {
                                    var insurances = await facadeImpl.GetAssignedInsurancesForPatient(patientId);
                                    
                                    // دریافت اطلاعات کامل بیمار از دیتابیس
                                    var patient = await _context.Patients
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted);
                                    
                                    if (patient != null)
                                    {
                                        var response = new Controllers.Api.PatientLookupResponseDto
                                        {
                                            Identity = new Controllers.Api.PatientIdentityDto
                                            {
                                                PatientId = patient.PatientId,
                                                NationalCode = patient.NationalCode,
                                                FirstName = patient.FirstName,
                                                LastName = patient.LastName,
                                                FatherName = patient.FatherName,
                                                Mobile = RegexHelper.NormalizeMobile(patient.PhoneNumber),
                                                Phone = null,
                                                Address = patient.Address,
                                                Gender = patient.Gender.ToString(),
                                                BirthDateShamsi = patient.BirthDate?.ToPersianDate() ?? string.Empty
                                            },
                                            Insurance = insurances
                                        };
                                        
                                        return Json(ServiceResult<Controllers.Api.PatientLookupResponseDto>.Successful(response, "بیمار یافت شد."));
                                    }
                                }
                                
                                // Fallback (patientId <= 0)
                                return Json(ServiceResult.Failed("بیمار یافت نشد. لطفاً ثبت سریع بیمار را تکمیل کنید.", "NOT_FOUND"));
                            }
                            
                            // جستجو ناموفق: فقط در صورت NOT_FOUND مودال باز می‌شود؛ در خطای سرور (مثلاً GENERAL_ERROR) همان Code/Message برگردانده می‌شود
                            var code = findResult.Code ?? "NOT_FOUND";
                            var msg = findResult.Message ?? "بیمار یافت نشد. لطفاً ثبت سریع بیمار را تکمیل کنید.";
                            return Json(ServiceResult.Failed(msg, code));
                        }
                        
                        // ✅ گام ۵: اگر اطلاعات هویت آمده (Quick Create) - با تبدیل بهتر تاریخ
                        DateTime? birthDate = null;
                        if (!string.IsNullOrWhiteSpace(request.BirthDateShamsi))
                        {
                            try
                            {
                                birthDate = Helpers.PersianDateHelper.ToGregorianDate(request.BirthDateShamsi);
                            }
                            catch (Exception dateEx)
                            {
                                _logger?.Warning(dateEx, "🏥 V1 API: خطا در تبدیل تاریخ شمسی - BirthDateShamsi: {BirthDateShamsi}", request.BirthDateShamsi);
                                return Json(ServiceResult<PatientLookupResponseDto>
                                    .Failed($"تاریخ تولد معتبر نیست: {dateEx.Message}", ReceptionApiCodes.VALIDATION)
                                    .WithValidationError("BirthDateShamsi", $"تاریخ تولد معتبر نیست. فرمت صحیح: yyyy/MM/dd"));
                            }
                        }

                        var quickCreateDto = new ViewModels.Reception.PatientCreateDto
                        {
                            NationalCode = normalizedNationalCode,
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            FatherName = request.FatherName,
                            PhoneNumber = request.Mobile,
                            Gender = request.Gender,
                            BirthDate = birthDate,
                            Address = request.Address
                        };
                        
                        var createResult = await facadeImpl.FindOrCreatePatientAsync(normalizedNationalCode, quickCreateDto);
                        if (createResult.Success && createResult.Data != null)
                        {
                            var patientDto = createResult.Data;
                            
                            // ✅ بررسی معتبر بودن PatientId قبل از استفاده
                            // طبق قرارداد: جلوگیری از NullReferenceException
                            if (patientDto == null)
                            {
                                _logger?.Error("❌ V1 API: patientDto null است - NationalCode: {NationalCode}", request.NationalCode);
                                return Json(ServiceResult.Failed("خطا در ایجاد بیمار: داده‌های بیمار null است.", "PATIENT_DTO_NULL"));
                            }
                            
                            var patientId = patientDto.PatientId;
                            
                            // ✅ بررسی معتبر بودن PatientId
                            if (patientId <= 0)
                            {
                                _logger?.Error("❌ V1 API: PatientId نامعتبر است - PatientId: {PatientId}, NationalCode: {NationalCode}", 
                                    patientId, request.NationalCode);
                                return Json(ServiceResult.Failed(
                                    "بیمار ایجاد شد اما شناسه بیمار نامعتبر است. لطفاً دوباره تلاش کنید.",
                                    "INVALID_PATIENT_ID"));
                            }
                            
                            if (patientId > 0)
                            {
                                // ✅ ایجاد/اتصال بیمه‌ها اگر مشخص شده باشند - با try/catch برای جلوگیری از شکست کل عملیات
                                string insuranceError = null;
                                if (request.BaseInsurancePlanId.HasValue || request.SupplementaryInsurancePlanId.HasValue)
                                {
                                    try
                                    {
                                        await facadeImpl.SetPatientInsurancesAsync(patientId, request.BaseInsurancePlanId, request.SupplementaryInsurancePlanId);
                                        _logger?.Information("✅ V1 API: بیمه‌های بیمار با موفقیت تنظیم شد - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                                            patientId, request.BaseInsurancePlanId, request.SupplementaryInsurancePlanId);
                                    }
                                    catch (InvalidOperationException ioEx)
                                    {
                                        // ⚠️ خطای business logic: بیمه یافت نشد یا نوع آن نامعتبر است
                                        insuranceError = ioEx.Message;
                                        _logger?.Warning(ioEx, "⚠️ V1 API: خطا در تنظیم بیمه‌های بیمار (خطای business logic) - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}, Error: {Error}", 
                                            patientId, request.BaseInsurancePlanId, request.SupplementaryInsurancePlanId, ioEx.Message);
                                    }
                                    catch (Exception insuranceEx)
                                    {
                                        // ⚠️ خطای غیرمنتظره در تنظیم بیمه
                                        insuranceError = $"خطا در تنظیم بیمه‌های بیمار: {insuranceEx.Message}";
                                        _logger?.Error(insuranceEx, "⚠️ V1 API: خطا در تنظیم بیمه‌های بیمار (خطای غیرمنتظره) - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                                            patientId, request.BaseInsurancePlanId, request.SupplementaryInsurancePlanId);
                                    }
                                }
                                
                                // دریافت اطلاعات کامل بیمار و بیمه‌ها
                                var patient = await _context.Patients
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted);
                                
                                var insurances = await facadeImpl.GetAssignedInsurancesForPatient(patientId);
                                
                                if (patient != null)
                                {
                                    var response = new Controllers.Api.PatientLookupResponseDto
                                    {
                                        Identity = new Controllers.Api.PatientIdentityDto
                                        {
                                            PatientId = patient.PatientId,
                                            NationalCode = patient.NationalCode,
                                            FirstName = patient.FirstName,
                                            LastName = patient.LastName,
                                            FatherName = patient.FatherName,
                                            Mobile = RegexHelper.NormalizeMobile(patient.PhoneNumber),
                                            Phone = null,
                                            Address = patient.Address,
                                            Gender = patient.Gender.ToString(),
                                            BirthDateShamsi = patient.BirthDate?.ToPersianDate() ?? string.Empty
                                        },
                                        Insurance = insurances
                                    };
                                    
                                    // ✅ اگر خطای بیمه داشتیم، آن را در Metadata بفرستیم
                                    var result = ServiceResult<Controllers.Api.PatientLookupResponseDto>.Successful(response, 
                                        string.IsNullOrWhiteSpace(insuranceError) ? "بیمار با موفقیت ثبت شد." : "بیمار ثبت شد اما تنظیم بیمه با خطا مواجه شد.");
                                    
                                    if (!string.IsNullOrWhiteSpace(insuranceError))
                                    {
                                        // ✅ اضافه کردن خطای بیمه به Metadata
                                        result.Metadata["InsuranceError"] = insuranceError;
                                        _logger?.Warning("⚠️ V1 API: بیمار ثبت شد اما تنظیم بیمه ناموفق بود - PatientId: {PatientId}, Error: {Error}", patientId, insuranceError);
                                    }
                                    
                                    return Json(result);
                                }
                            }
                        }
                        
                        return Json(ServiceResult.Failed(createResult?.Message ?? "خطا در ثبت بیمار.", "CREATE_FAILED"));
                    }
                }
                
                return Json(ServiceResult.Failed("بیمار یافت نشد.", "NOT_FOUND"));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در Patient Lookup-Or-Create");
#if DEBUG
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
#else
                return Json(ServiceResult.Failed("خطای غیرمنتظره رخ داد.", "UNHANDLED"));
#endif
            }
        }

        /// <summary>
        /// GET /api/v1/reception/insurance/plans
        /// دریافت پلن‌های بیمه (پایه و تکمیلی)
        /// </summary>
        [HttpGet, Route("insurance/plans")]
        public async Task<ActionResult> GetInsurancePlans(int? patientId = null, int? providerId = null)
        {
            try
            {
                _logger?.Information("🏥 V1 API: Get Insurance Plans - PatientId: {PatientId}, ProviderId: {ProviderId}", patientId, providerId);

                // دریافت بیمه‌های پایه (Primary)
                var basePlansQuery = _context.InsurancePlans
                    .Where(p => !p.IsDeleted && p.IsActive && p.InsuranceType == Models.Entities.Insurance.InsuranceType.Primary);

                // دریافت بیمه‌های تکمیلی (Supplementary)
                var supplementaryPlansQuery = _context.InsurancePlans
                    .Where(p => !p.IsDeleted && p.IsActive && p.InsuranceType == Models.Entities.Insurance.InsuranceType.Supplementary);

                // اگر providerId مشخص شده، فیلتر کن
                if (providerId.HasValue)
                {
                    basePlansQuery = basePlansQuery.Where(p => p.InsuranceProviderId == providerId.Value);
                    supplementaryPlansQuery = supplementaryPlansQuery.Where(p => p.InsuranceProviderId == providerId.Value);
                }

                // لود کردن بیمه‌ها
                var basePlans = await basePlansQuery
                    .Include(p => p.InsuranceProvider)
                    .OrderBy(p => p.InsuranceProvider.Name)
                    .ThenBy(p => p.Name)
                    .AsNoTracking()
                    .Select(p => new
                    {
                        insurancePlanId = p.InsurancePlanId,
                        insuranceId = p.InsurancePlanId, // Alias برای سازگاری
                        name = p.Name,
                        insuranceName = p.Name, // Alias برای سازگاری
                        planCode = p.PlanCode,
                        coveragePercent = p.CoveragePercent, // CoveragePercent (نه CoveragePercentage)
                        coveragePercentage = p.CoveragePercent, // Alias برای سازگاری با frontend
                        providerId = p.InsuranceProviderId,
                        providerName = p.InsuranceProvider.Name,
                        isActive = p.IsActive
                    })
                    .ToListAsync();

                var supplementaryPlans = await supplementaryPlansQuery
                    .Include(p => p.InsuranceProvider)
                    .OrderBy(p => p.InsuranceProvider.Name)
                    .ThenBy(p => p.Name)
                    .AsNoTracking()
                    .Select(p => new
                    {
                        insurancePlanId = p.InsurancePlanId,
                        insuranceId = p.InsurancePlanId, // Alias برای سازگاری
                        name = p.Name,
                        insuranceName = p.Name, // Alias برای سازگاری
                        planCode = p.PlanCode,
                        coveragePercent = p.CoveragePercent, // CoveragePercent (نه CoveragePercentage)
                        coveragePercentage = p.CoveragePercent, // Alias برای سازگاری با frontend
                        providerId = p.InsuranceProviderId,
                        providerName = p.InsuranceProvider.Name,
                        isActive = p.IsActive
                    })
                    .ToListAsync();

                var payload = new
                {
                    basePlans = basePlans,
                    supplementaryPlans = supplementaryPlans
                };

                _logger?.Information("🏥 V1 API: Insurance Plans loaded - Base: {BaseCount}, Supplementary: {SuppCount}", 
                    basePlans.Count, supplementaryPlans.Count);
                
                return Json(ServiceResult<object>.Successful(payload, "عملیات با موفقیت انجام شد."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در Get Insurance Plans");
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// GET /api/v1/reception/doctors/by-department
        /// ✅ دریافت پزشکان یک دپارتمان (بهینه شده با DoctorOptionDto)
        /// </summary>
        [HttpGet, Route("doctors/by-department")]
        public async Task<ActionResult> GetDoctorsByDepartment(int deptId, int? clinicId = null)
        {
            try
            {
                _logger?.Information("🏥 V1 API: دریافت پزشکان دپارتمان - DeptId: {DeptId}, ClinicId: {ClinicId}", deptId, clinicId);

                if (deptId <= 0)
                {
                    return Json(ServiceResult.Failed("شناسه دپارتمان نامعتبر است.", "VALIDATION"), JsonRequestBehavior.AllowGet);
                }

                if (_facade != null)
                {
                    var result = await _facade.GetDoctorsByDepartmentAsync(deptId, clinicId);
                    
                    if (result.Success && result.Data != null)
                    {
                        // ✅ دریافت نام دپارتمان از دیتابیس
                        var department = await _context.Departments
                            .AsNoTracking()
                            .Where(d => d.DepartmentId == deptId && !d.IsDeleted)
                            .Select(d => new { d.Name })
                            .FirstOrDefaultAsync();
                        
                        var departmentName = department?.Name ?? "";
                        
                        // ✅ تبدیل DoctorDto به DoctorOptionDto برای پاسخ یکنواخت
                        var doctors = result.Data.Select(d => new DoctorOptionDto
                        {
                            DoctorId = d.DoctorId,
                            FullName = d.FullName ?? $"{d.FirstName} {d.LastName}".Trim(),
                            Title = d.Specialization ?? "",
                            DepartmentName = departmentName, // ✅ از Department گرفته شد
                            IsActive = d.IsActive
                        }).ToList();

                        _logger?.Information("✅ V1 API: پزشکان دریافت شد - Count: {Count}", doctors.Count);
                        return Json(ServiceResult<object>.Successful(new { doctors }, "پزشکان با موفقیت دریافت شد."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: دریافت پزشکان ناموفق - Message: {Message}", result.Message);
                        return Json(ServiceResult.Failed(result.Message, result.Code), JsonRequestBehavior.AllowGet);
                    }
                }

                _logger?.Warning("⚠️ V1 API: _facade is null");
                return Json(ServiceResult.Failed("سرویس در دسترس نیست", "SERVICE_UNAVAILABLE"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در دریافت پزشکان دپارتمان - DeptId: {DeptId}, ClinicId: {ClinicId}", deptId, clinicId);
                return Json(ServiceResult.Failed("خطا در دریافت فهرست پزشکان.", "DOCTORS_FETCH_FAILED").WithExceptionDev(ex), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// GET /api/v1/reception/services/by-department
        /// دریافت خدمات یک دپارتمان؛ در صورت ارسال basePlanId/suppPlanId وضعیت تعیین‌ست برای هر خدمت برمی‌گردد.
        /// </summary>
        [HttpGet, Route("services/by-department")]
        public async Task<ActionResult> GetServicesByDepartment(int? deptId, int? basePlanId, int? suppPlanId)
        {
            try
            {
                _logger?.Information("🏥 V1 API: دریافت خدمات - DeptId: {DeptId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                    deptId, basePlanId, suppPlanId);

                if (!deptId.HasValue || deptId.Value <= 0)
                {
                    return Json(ServiceResult<object>.Failed("شناسه دپارتمان نامعتبر است.", "VALIDATION"), JsonRequestBehavior.AllowGet);
                }

                var result = await _facade.GetServicesForDeptAsync(deptId.Value);
                
                if (result.Success && result.Data != null)
                {
                    var services = result.Data.Services;
                    var serviceIds = services.Select(s => s.ServiceId).ToList();
                    Dictionary<int, (bool hasTariffSet, string warning)> tariffStatus = null;
                    if (_pricing != null && (basePlanId.HasValue || suppPlanId.HasValue) && serviceIds.Any())
                    {
                        tariffStatus = await _pricing.GetServicesTariffStatusAsync(serviceIds, basePlanId, suppPlanId);
                    }

                    var payload = new
                    {
                        services = services.Select(s =>
                        {
                            var status = tariffStatus != null && tariffStatus.ContainsKey(s.ServiceId)
                                ? tariffStatus[s.ServiceId]
                                : (hasTariffSet: true, warning: (string)null);
                            return new
                            {
                                serviceId = s.ServiceId,
                                serviceCode = s.ServiceCode,
                                serviceName = s.ServiceName,
                                price = s.UnitPrice,
                                unitPriceIRR = s.UnitPrice,
                                isActive = s.IsActive,
                                hasTariffSet = status.hasTariffSet,
                                tariffWarning = status.warning
                            };
                        }).ToList()
                    };
                    
                    return Json(ServiceResult<object>.Successful(payload, result.Message ?? "عملیات با موفقیت انجام شد."), JsonRequestBehavior.AllowGet);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در دریافت خدمات");
                return Json(ServiceResult<object>.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/patient/update-basic
        /// به‌روزرسانی اطلاعات پایه بیمار
        /// </summary>
        [HttpPost, Route("patient/update-basic")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> UpdatePatientBasic(PatientUpdateBasicRequest request)
        {
            try
            {
                _logger?.Information("🏥 V1 API: به‌روزرسانی اطلاعات بیمار - PatientId: {PatientId}", request?.PatientId);

                if (request == null || request.PatientId <= 0)
                {
                    return Json(ServiceResult<PatientIdentityDto>.Failed("درخواست نامعتبر است.", "VALIDATION"));
                }

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

                // نرمال‌سازی موبایل (پذیرش +989... و 09... و 0098...) برای نمایش و اعتبارسنجی یکسان
                var mobileNormalized = RegexHelper.NormalizeMobile(request.Mobile);
                if (!string.IsNullOrWhiteSpace(request.Mobile) && (string.IsNullOrWhiteSpace(mobileNormalized) || !Regex.IsMatch(mobileNormalized, @"^09\d{9}$")))
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

                // اعمال تغییرات مجاز (موبایل به فرمت 09xxxxxxxxx ذخیره می‌شود)
                patient.FirstName = request.FirstName?.Trim();
                patient.LastName = request.LastName?.Trim();
                patient.FatherName = request.FatherName?.Trim();
                patient.PhoneNumber = !string.IsNullOrWhiteSpace(mobileNormalized) ? mobileNormalized : request.Mobile?.Trim();
                patient.Address = request.Address?.Trim();
                patient.Gender = gender;
                patient.BirthDate = birthDate;

                patient.UpdatedAt = DateTime.Now;
                patient.UpdatedByUserId = userId;

                await _context.SaveChangesAsync();

                // بازگرداندن DTO تازه برای همسان‌سازی UI (موبایل همیشه به فرمت 09... برای فرم)
                var updatedDto = new PatientIdentityDto
                {
                    PatientId = patient.PatientId,
                    NationalCode = patient.NationalCode,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    FatherName = patient.FatherName,
                    Mobile = RegexHelper.NormalizeMobile(patient.PhoneNumber),
                    Phone = null,
                    Address = patient.Address,
                    Gender = patient.Gender.ToString(),
                    BirthDateShamsi = patient.BirthDate?.ToPersianDate() ?? string.Empty
                };

                return Json(ServiceResult<PatientIdentityDto>.Successful(updatedDto, "اطلاعات به‌روزرسانی شد."));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در به‌روزرسانی اطلاعات بیمار");
                return Json(ServiceResult<PatientIdentityDto>.Failed("به‌روزرسانی ناموفق بود: " + ex.Message, "UNHANDLED"));
            }
        }

        /// <summary>
        /// POST /api/v1/reception/insurances/set
        /// تنظیم بیمه‌های پیش‌نویس
        /// </summary>
        [HttpPost, Route("insurances/set")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> SetInsurances(SetInsurancesRequestDto request)
        {
            try
            {
                _logger?.Information("🏥 V1 API: تنظیم بیمه‌های پیش‌نویس - ReceptionId: {ReceptionId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                    request?.ReceptionId, request?.BasePlanId, request?.SupplementaryPlanId);

                // اعتبارسنجی اولیه
                if (request == null || request.ReceptionId <= 0)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است. ReceptionId الزامی است.", "VALIDATION"));
                }

                // اعتبارسنجی Reception وجود دارد
                var receptionExists = await _context.Receptions
                    .AnyAsync(r => r.ReceptionId == request.ReceptionId && !r.IsDeleted);
                
                if (!receptionExists)
                {
                    return Json(ServiceResult.Failed("پذیرش یافت نشد.", "NOT_FOUND"));
                }

                if (_facade != null)
                {
                    var facadeRequest = new ViewModels.Reception.SetInsurancesRequest
                    {
                        ReceptionId = request.ReceptionId,
                        BasePlanId = request.BasePlanId,
                        SupplementaryPlanId = request.SupplementaryPlanId
                    };

                    var result = await _facade.SetInsurancesAsync(facadeRequest);
                    
                    // لاگ نتیجه
                    if (result.Success)
                    {
                        _logger?.Information("✅ V1 API: بیمه‌های پیش‌نویس با موفقیت تنظیم شد - ReceptionId: {ReceptionId}", request.ReceptionId);
                        
                        // ✅ پس از Reprice، Totals و Pricings را محاسبه و ضمیمه پاسخ کنید
                        try
                        {
                            var (totals, pricings) = await _pricing.RepriceAllAsync(request.ReceptionId);
                            
                            var responseData = new
                            {
                                totals,
                                pricings = pricings ?? new List<Controllers.Api.PricingBreakdownDto>()
                            };
                            
                            _logger?.Information("✅ V1 API: Reprice کامل شد - ReceptionId: {ReceptionId}, ItemsCount: {Count}, Gross: {Gross}", 
                                request.ReceptionId, pricings?.Count ?? 0, totals?.GrossIRR ?? 0);
                            
                            return Json(ServiceResult<object>.Successful(responseData, "بیمه اعمال و محاسبه شد.")
                                .WithCode(ReceptionApiCodes.PRICING_RECALCULATED));
                        }
                        catch (Exception pricingEx)
                        {
                            _logger?.Warning(pricingEx, "⚠️ V1 API: خطا در RepriceAll پس از SetInsurances - ReceptionId: {ReceptionId}", 
                                request.ReceptionId);
                            
                            // Fallback: فقط Totals را محاسبه کن
                            try
                            {
                                var totals = await _pricing.CalculateTotalsAsync(request.ReceptionId);
                                return Json(ServiceResult<object>.Successful(new { totals }, "بیمه اعمال شد (خطا در Reprice)."));
                            }
                            catch (Exception totalsEx)
                            {
                                _logger?.Warning(totalsEx, "⚠️ V1 API: خطا در محاسبه Totals (fallback) - ReceptionId: {ReceptionId}", 
                                    request.ReceptionId);
                                // Fallback نهایی: فقط نتیجه SetInsurances را برگردان
                                return Json(result);
                            }
                        }
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: تنظیم بیمه‌های پیش‌نویس ناموفق - ReceptionId: {ReceptionId}, Error: {Error}", 
                            request.ReceptionId, result.Message);
                    }
                    
                    return Json(result);
                }

                return Json(ServiceResult.Failed("سرویس در دسترس نیست.", "SERVICE_UNAVAILABLE"));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در تنظیم بیمه‌ها - ReceptionId: {ReceptionId}", request?.ReceptionId);
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
            }
        }

        /// <summary>
        /// GET /api/v1/reception/insurance/coverage
        /// دریافت جزئیات پوشش بیمه (پایه + تکمیلی + مؤثر)
        /// </summary>
        [HttpGet, Route("insurance/coverage")]
        public async Task<ActionResult> GetInsuranceCoverage(int patientId = 0, int? basePlanId = null, int? supplementaryPlanId = null)
        {
            try
            {
                _logger?.Information("🏥 V1 API: Get Insurance Coverage - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                    patientId, basePlanId, supplementaryPlanId);

                if (_facade != null)
                {
                    var result = await _facade.GetInsuranceCoverageAsync(patientId, basePlanId, supplementaryPlanId);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return Json(ServiceResult<InsuranceCoverageDto>.Failed("سرویس در دسترس نیست.", "SERVICE_UNAVAILABLE"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در دریافت پوشش بیمه");
                return Json(ServiceResult<InsuranceCoverageDto>.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// GET /api/v1/reception/item/price/preview
        /// پیش‌نمایش قیمت خدمت (بدون persist)
        /// </summary>
        [HttpGet, Route("item/price/preview")]
        public async Task<ActionResult> PreviewItemPrice(PricePreviewRequestDto request)
        {
            try
            {
                _logger?.Information("🏥 V1 API: Price Preview - ServiceCode: {ServiceCode}, PatientId: {PatientId}", 
                    request?.ServiceCodeOrName, request?.PatientId);

                if (string.IsNullOrWhiteSpace(request?.ServiceCodeOrName))
                {
                    return Json(ServiceResult<PricePreviewResultDto>.Failed("کد یا نام خدمت الزامی است.", "VALIDATION"), JsonRequestBehavior.AllowGet);
                }

                if (_facade != null)
                {
                    var result = await _facade.PreviewItemPriceAsync(request);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return Json(ServiceResult<PricePreviewResultDto>.Failed("سرویس در دسترس نیست.", "SERVICE_UNAVAILABLE"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در پیش‌نمایش قیمت");
                return Json(ServiceResult<PricePreviewResultDto>.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/item/add
        /// افزودن آیتم به پیش‌نویس
        /// </summary>
        [HttpPost, Route("item/add")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> AddItem(AddItemRequestDto request)
        {
            try
            {
                _logger?.Information("🏥 V1 API: افزودن آیتم به پیش‌نویس - ReceptionId: {ReceptionId}, ServiceId: {ServiceId}, Quantity: {Quantity}", 
                    request?.ReceptionId, request?.ServiceId, request?.Quantity);

                if (request == null || request.ReceptionId <= 0 || request.ServiceId <= 0 || request.Quantity <= 0)
                {
                    return Json(ServiceResult<ItemsAndTotalsDto>.Failed("درخواست نامعتبر است. ReceptionId, ServiceId و Quantity الزامی هستند.", "VALIDATION"));
                }

                if (_facade != null)
                {
                    var facadeRequest = new ViewModels.Reception.AddItemRequest
                    {
                        ReceptionId = request.ReceptionId,
                        ServiceId = request.ServiceId,
                        Quantity = request.Quantity
                    };

                    var result = await _facade.AddItemAsync(facadeRequest);
                    
                    // 🚨 PROFESSIONAL FIX: حتی اگر Success false باشد، اگر Data موجود است، آن را برگردان
                    // این برای حالتی است که آیتم ذخیره شده اما خطا در محاسبه بیمه رخ داده
                    if (result.Data != null)
                    {
                        _logger?.Information("✅ V1 API: آیتم افزوده شد (Success: {Success}) - ReceptionId: {ReceptionId}, ItemsCount: {ItemsCount}", 
                            result.Success, request.ReceptionId, result.Data.Items?.Count ?? 0);
                        
                        // 🚨 PROFESSIONAL FIX: استفاده از ItemsAndTotalsDto که از Facade برگشته است
                        // این شامل Items با InsuranceCalculation است
                        var itemsAndTotals = result.Data;
                        
                        // ✅ محاسبه Pricing برای آخرین آیتم افزوده شده (برای سازگاری با کد قدیمی)
                        try
                        {
                            // پیدا کردن آخرین ReceptionItem برای این Reception
                            var lastItem = await _context.ReceptionItems
                                .Include(i => i.Service)  // ✅ Load Service برای دریافت Code و Name
                                .Where(i => i.ReceptionId == request.ReceptionId && 
                                           i.ServiceId == request.ServiceId && 
                                           !i.IsDeleted)
                                .OrderByDescending(i => i.ReceptionItemId)
                                .FirstOrDefaultAsync();
                            
                            if (lastItem != null)
                            {
                                // ✅ محاسبه Pricing برای آخرین آیتم
                                var pricing = await _pricing.PriceItemAsync(request.ReceptionId, lastItem.ReceptionItemId);
                                
                                // ✅ تبدیل TotalsDto به فرمت مورد انتظار
                                var totals = new
                                {
                                    GrossIRR = (long)itemsAndTotals.Totals.Gross,
                                    BaseCoveredIRR = (long)itemsAndTotals.Totals.Base,
                                    SuppCoveredIRR = (long)itemsAndTotals.Totals.Supplementary,
                                    PatientPayableIRR = (long)itemsAndTotals.Totals.Patient,
                                    GrossIRRStr = itemsAndTotals.Totals.Gross.ToString("N0"),
                                    BaseCoveredIRRStr = itemsAndTotals.Totals.Base.ToString("N0"),
                                    SuppCoveredIRRStr = itemsAndTotals.Totals.Supplementary.ToString("N0"),
                                    PatientPayableIRRStr = itemsAndTotals.Totals.Patient.ToString("N0")
                                };
                                
                                // 🚨 PROFESSIONAL: برگرداندن Items (که شامل InsuranceCalculation است) + item + pricing + totals
                                return Json(ServiceResult<object>.Successful(new 
                                { 
                                    item = new
                                    {
                                        ServiceId = request.ServiceId,
                                        Quantity = request.Quantity,
                                        ReceptionItemId = lastItem.ReceptionItemId,
                                        Code = lastItem.Service?.ServiceCode ?? "",
                                        Name = lastItem.Service?.Title ?? ""
                                    },
                                    items = itemsAndTotals.Items, // 🚨 NEW: شامل InsuranceCalculation
                                    Items = itemsAndTotals.Items, // 🚨 NEW: برای سازگاری
                                    pricing,
                                    totals
                                }, "آیتم اضافه و محاسبه شد."));
                            }
                            else
                            {
                                // Fallback: فقط Items و Totals را برگردان
                                var totals = new
                                {
                                    GrossIRR = (long)itemsAndTotals.Totals.Gross,
                                    BaseCoveredIRR = (long)itemsAndTotals.Totals.Base,
                                    SuppCoveredIRR = (long)itemsAndTotals.Totals.Supplementary,
                                    PatientPayableIRR = (long)itemsAndTotals.Totals.Patient,
                                    GrossIRRStr = itemsAndTotals.Totals.Gross.ToString("N0"),
                                    BaseCoveredIRRStr = itemsAndTotals.Totals.Base.ToString("N0"),
                                    SuppCoveredIRRStr = itemsAndTotals.Totals.Supplementary.ToString("N0"),
                                    PatientPayableIRRStr = itemsAndTotals.Totals.Patient.ToString("N0")
                                };
                                return Json(ServiceResult<object>.Successful(new 
                                { 
                                    items = itemsAndTotals.Items, // 🚨 NEW
                                    Items = itemsAndTotals.Items, // 🚨 NEW
                                    totals 
                                }, result.Message));
                            }
                        }
                        catch (Exception pricingEx)
                        {
                            _logger?.Warning(pricingEx, "⚠️ V1 API: خطا در محاسبه Pricing/Totals پس از AddItem - ReceptionId: {ReceptionId}", 
                                request.ReceptionId);
                            // Fallback: فقط نتیجه AddItem را برگردان (که شامل Items است)
                            return Json(result);
                        }
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: افزودن آیتم ناموفق - ReceptionId: {ReceptionId}, Error: {Error}, HasData: {HasData}", 
                            request.ReceptionId, result?.Message, result?.Data != null);
                        
                        // 🚨 PROFESSIONAL FIX: حتی اگر Success false باشد، اگر Data موجود است، آن را برگردان
                        if (result?.Data != null)
                        {
                            _logger?.Information("🏥 V1 API: بازگرداندن Data حتی با Success=false - ReceptionId: {ReceptionId}, ItemsCount: {Count}", 
                                request.ReceptionId, result.Data.Items?.Count ?? 0);
                            
                            var itemsAndTotals = result.Data;
                            var totals = new
                            {
                                GrossIRR = (long)itemsAndTotals.Totals.Gross,
                                BaseCoveredIRR = (long)itemsAndTotals.Totals.Base,
                                SuppCoveredIRR = (long)itemsAndTotals.Totals.Supplementary,
                                PatientPayableIRR = (long)itemsAndTotals.Totals.Patient,
                                GrossIRRStr = itemsAndTotals.Totals.Gross.ToString("N0"),
                                BaseCoveredIRRStr = itemsAndTotals.Totals.Base.ToString("N0"),
                                SuppCoveredIRRStr = itemsAndTotals.Totals.Supplementary.ToString("N0"),
                                PatientPayableIRRStr = itemsAndTotals.Totals.Patient.ToString("N0")
                            };
                            
                            return Json(ServiceResult<object>.Successful(new 
                            { 
                                items = itemsAndTotals.Items,
                                Items = itemsAndTotals.Items,
                                totals 
                            }, result.Message ?? "آیتم افزوده شد اما خطا در محاسبه رخ داد."));
                        }
                    }
                    
                    return Json(result);
                }

                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("سرویس در دسترس نیست.", "SERVICE_UNAVAILABLE"));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در افزودن آیتم - ReceptionId: {ReceptionId}, ServiceId: {ServiceId}", 
                    request?.ReceptionId, request?.ServiceId);
                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
            }
        }

        /// <summary>
        /// GET /api/v1/reception/totals
        /// دریافت جمع‌های پذیرش
        /// </summary>
        [HttpGet, Route("totals")]
        public async Task<ActionResult> GetTotals(int receptionId)
        {
            try
            {
                _logger?.Information("🏥 V1 API: دریافت جمع‌های پذیرش - ReceptionId: {ReceptionId}", receptionId);

                if (receptionId <= 0)
                {
                    return Json(ServiceResult.Failed("ReceptionId الزامی است.", "VALIDATION"), JsonRequestBehavior.AllowGet);
                }

                var totals = await _pricing.CalculateTotalsAsync(receptionId);
                return Json(ServiceResult<object>.Successful(new { totals }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در دریافت جمع‌ها - ReceptionId: {ReceptionId}", receptionId);
                return Json(ServiceResult.Failed("خطا در دریافت جمع‌ها.", "TOTALS_FETCH_FAILED").WithExceptionDev(ex), 
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/item/remove
        /// حذف آیتم از پیش‌نویس
        /// </summary>
        [HttpPost, Route("item/remove")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> RemoveItem(RemoveItemRequestDto request)
        {
            try
            {
                _logger?.Information("🏥 V1 API: حذف آیتم از پیش‌نویس - ReceptionId: {ReceptionId}, ServiceId: {ServiceId}", 
                    request?.ReceptionId, request?.ServiceId);

                if (request == null || request.ReceptionId <= 0 || request.ServiceId <= 0)
                {
                    return Json(ServiceResult<ItemsAndTotalsDto>.Failed("درخواست نامعتبر است. ReceptionId و ServiceId الزامی هستند.", "VALIDATION"));
                }

                if (_facade != null)
                {
                    var facadeRequest = new ViewModels.Reception.RemoveItemRequest
                    {
                        ReceptionId = request.ReceptionId,
                        ServiceId = request.ServiceId
                    };

                    var result = await _facade.RemoveItemAsync(facadeRequest);
                    
                    if (result.Success)
                    {
                        _logger?.Information("✅ V1 API: آیتم با موفقیت حذف شد - ReceptionId: {ReceptionId}", request.ReceptionId);
                        
                        // ✅ پس از حذف، Totals را محاسبه و ضمیمه پاسخ کنید
                        try
                        {
                            var totals = await _pricing.CalculateTotalsAsync(request.ReceptionId);
                            
                            // ✅ دریافت لیست آیتم‌های باقیمانده برای نمایش در UI
                            var remainingItems = await _context.ReceptionItems
                                .Include(i => i.Service)
                                .Where(i => i.ReceptionId == request.ReceptionId && !i.IsDeleted)
                                .Select(i => new
                                {
                                    ServiceId = i.ServiceId,
                                    Code = i.Service.ServiceCode,
                                    Name = i.Service.Title,
                                    Qty = i.Quantity,
                                    UnitPriceIRR = (long)i.UnitPrice,
                                    TotalIRR = (long)(i.UnitPrice * i.Quantity)
                                })
                                .ToListAsync();
                            
                            return Json(ServiceResult<object>.Successful(new 
                            { 
                                items = remainingItems,
                                totals
                            }, "آیتم حذف و محاسبه شد."));
                        }
                        catch (Exception pricingEx)
                        {
                            _logger?.Warning(pricingEx, "⚠️ V1 API: خطا در محاسبه Totals پس از RemoveItem - ReceptionId: {ReceptionId}", 
                                request.ReceptionId);
                            // Fallback: فقط نتیجه RemoveItem را برگردان
                            return Json(result);
                        }
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: حذف آیتم ناموفق - ReceptionId: {ReceptionId}, Error: {Error}", 
                            request.ReceptionId, result.Message);
                    }
                    
                    return Json(result);
                }

                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("سرویس در دسترس نیست.", "SERVICE_UNAVAILABLE"));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در حذف آیتم - ReceptionId: {ReceptionId}, ServiceId: {ServiceId}", 
                    request?.ReceptionId, request?.ServiceId);
                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
            }
        }

        /// <summary>
        /// POST /api/v1/reception/item/update-service
        /// ✅ تغییر خدمت/تعداد یک آیتم با پیش‌چک تعیین‌ست و Reprice
        /// </summary>
        [HttpPost, Route("item/update-service")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> UpdateItemService(UpdateItemServiceRequestDto request)
        {
            try
            {
                _logger?.Information("🏥 V1 API: تغییر خدمت آیتم - ReceptionItemId: {ReceptionItemId}, ServiceId: {ServiceId}, Quantity: {Quantity}", 
                    request?.ReceptionItemId, request?.ServiceId, request?.Quantity);

                if (request == null || request.ReceptionItemId <= 0 || request.ServiceId <= 0 || request.Quantity <= 0)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است. ReceptionItemId، ServiceId و Quantity الزامی هستند.", ReceptionApiCodes.VALIDATION));
                }

                // ✅ بررسی Reception وجود دارد
                var reception = await _context.Receptions
                    .Include(r => r.ReceptionItems)
                    .FirstOrDefaultAsync(r => r.ReceptionId == request.ReceptionId && !r.IsDeleted);

                if (reception == null)
                {
                    return Json(ServiceResult.Failed("پذیرش یافت نشد.", ReceptionApiCodes.NOT_FOUND));
                }

                // ✅ بررسی ReceptionItem وجود دارد
                var item = reception.ReceptionItems.FirstOrDefault(i => i.ReceptionItemId == request.ReceptionItemId && !i.IsDeleted);
                if (item == null)
                {
                    return Json(ServiceResult.Failed("آیتم پذیرش یافت نشد.", ReceptionApiCodes.NOT_FOUND));
                }

                // ✅ 1) پیش‌چک تعیین‌ست بیمه‌ای
                if (_pricing != null)
                {
                    var (ok, code, message, meta) = await _pricing.CheckInsuranceSetAsync(
                        serviceId: request.ServiceId,
                        departmentId: request.DepartmentId,
                        doctorId: request.DoctorId,
                        financialYearId: request.FinancialYearId,
                        basePlanId: request.BasePlanId,
                        suppPlanId: request.SupplementaryPlanId);

                    if (!ok)
                    {
                        _logger?.Warning("⚠️ V1 API: تعیین‌ست بیمه‌ای ناقص - ServiceId: {ServiceId}, Code: {Code}", 
                            request.ServiceId, code);
                        
                        var errorResult = ServiceResult.Failed(message, code);
                        if (meta != null)
                        {
                            errorResult.WithMetadata("meta", meta);
                        }
                        
                        return Json(errorResult);
                    }
                }

                // ✅ 2) Update خدمت/تعداد
                item.ServiceId = request.ServiceId;
                item.Quantity = request.Quantity;
                item.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();

                _logger?.Information("✅ V1 API: آیتم به‌روزرسانی شد - ReceptionItemId: {ReceptionItemId}", request.ReceptionItemId);

                // ✅ 3) Reprice آیتم
                if (_pricing != null)
                {
                    try
                    {
                        var pricing = await _pricing.PriceItemAsync(request.ReceptionId, request.ReceptionItemId);
                        var totals = await _pricing.CalculateTotalsAsync(request.ReceptionId);

                        var responseData = new
                        {
                            itemId = request.ReceptionItemId,
                            pricing,
                            totals
                        };

                        _logger?.Information("✅ V1 API: Reprice آیتم کامل شد - ReceptionItemId: {ReceptionItemId}", request.ReceptionItemId);
                        
                        return Json(ServiceResult<object>.Successful(responseData, "آیتم به‌روزرسانی و مجدد محاسبه شد.")
                            .WithCode(ReceptionApiCodes.SUCCESS));
                    }
                    catch (Exception pricingEx)
                    {
                        _logger?.Warning(pricingEx, "⚠️ V1 API: خطا در Reprice آیتم - ReceptionItemId: {ReceptionItemId}", 
                            request.ReceptionItemId);
                        // Fallback: فقط پیام موفقیت
                        return Json(ServiceResult<object>.Successful(new { itemId = request.ReceptionItemId }, 
                            "آیتم به‌روزرسانی شد (خطا در محاسبه قیمت)."));
                    }
                }

                return Json(ServiceResult<object>.Successful(new { itemId = request.ReceptionItemId }, 
                    "آیتم به‌روزرسانی شد."));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در به‌روزرسانی خدمت آیتم - ReceptionItemId: {ReceptionItemId}", 
                    request?.ReceptionItemId);
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, ReceptionApiCodes.UNHANDLED).WithExceptionDev(ex));
            }
        }

        /// <summary>
        /// POST /api/v1/reception/draft/update
        /// به‌روزرسانی پیش‌نویس پذیرش
        /// </summary>
        [HttpPost, Route("draft/update")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> UpdateDraft(ClinicApp.Dtos.Reception.UpdateDraftRequest request)
        {
            try
            {
                _logger?.Information("🏥 V1 API: به‌روزرسانی پیش‌نویس - ReceptionId: {ReceptionId}", request?.ReceptionId);

                if (request == null || request.ReceptionId <= 0)
                {
                    return Json(ServiceResult<ItemsAndTotalsDto>.Failed("درخواست نامعتبر است. ReceptionId الزامی است.", "VALIDATION"));
                }

                if (_facade != null)
                {
                    var result = await _facade.UpdateDraftAsync(request);
                    
                    if (result.Success)
                    {
                        _logger?.Information("✅ V1 API: پیش‌نویس با موفقیت به‌روزرسانی شد - ReceptionId: {ReceptionId}", request.ReceptionId);
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: به‌روزرسانی پیش‌نویس ناموفق - ReceptionId: {ReceptionId}, Error: {Error}", 
                            request.ReceptionId, result.Message);
                    }
                    
                    return Json(result);
                }

                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("سرویس در دسترس نیست.", "SERVICE_UNAVAILABLE"));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در به‌روزرسانی پیش‌نویس - ReceptionId: {ReceptionId}", request?.ReceptionId);
                return Json(ServiceResult<ItemsAndTotalsDto>.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
            }
        }

        /// <summary>
        /// GET /api/v1/reception/doctors/by-service
        /// دریافت پزشکان مجاز برای یک خدمت در دپارتمان
        /// </summary>
        [HttpGet, Route("doctors/by-service")]
        public async Task<ActionResult> GetDoctorsByService(int deptId, int serviceId, int? clinicId = null)
        {
            try
            {
                _logger?.Information("🏥 V1 API: دریافت پزشکان مجاز برای خدمت - DeptId: {DeptId}, ServiceId: {ServiceId}, ClinicId: {ClinicId}", 
                    deptId, serviceId, clinicId);

                if (deptId <= 0 || serviceId <= 0)
                {
                    return Json(ServiceResult.Failed("پارامترها نامعتبر است. DeptId و ServiceId الزامی هستند.", "VALIDATION"), JsonRequestBehavior.AllowGet);
                }

                if (_facade != null)
                {
                    var result = await _facade.GetDoctorsByServiceAsync(deptId, serviceId, clinicId);
                    
                    if (result.Success && result.Data != null)
                    {
                        // ✅ دریافت نام دپارتمان از دیتابیس
                        var department = await _context.Departments
                            .AsNoTracking()
                            .Where(d => d.DepartmentId == deptId && !d.IsDeleted)
                            .Select(d => new { d.Name })
                            .FirstOrDefaultAsync();
                        
                        var departmentName = department?.Name ?? "";
                        
                        // ✅ تبدیل DoctorDto به DoctorOptionDto برای پاسخ یکنواخت
                        var doctors = result.Data.Select(d => new DoctorOptionDto
                        {
                            DoctorId = d.DoctorId,
                            FullName = d.FullName ?? $"{d.FirstName} {d.LastName}".Trim(),
                            Title = d.Specialization ?? "",
                            DepartmentName = departmentName, // ✅ از Department گرفته شد
                            IsActive = d.IsActive
                        }).ToList();

                        _logger?.Information("✅ V1 API: پزشکان مجاز دریافت شد - Count: {Count}", doctors.Count);
                        return Json(ServiceResult<object>.Successful(new { doctors }, "پزشکان مجاز با موفقیت دریافت شد."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: دریافت پزشکان مجاز ناموفق - Message: {Message}", result.Message);
                        return Json(ServiceResult.Failed(result.Message, result.Code), JsonRequestBehavior.AllowGet);
                    }
                }

                _logger?.Warning("⚠️ V1 API: _facade is null");
                return Json(ServiceResult<List<ViewModels.Reception.DoctorDto>>.Failed("سرویس در دسترس نیست"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در دریافت پزشکان مجاز برای خدمت - DeptId: {DeptId}, ServiceId: {ServiceId}, ClinicId: {ClinicId}", 
                    deptId, serviceId, clinicId);
                return Json(ServiceResult.Failed("خطا در فیلتر پزشکان بر اساس خدمت.", "DOCTORS_FILTER_FAILED").WithExceptionDev(ex), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/finalize/pos
        /// نهایی‌سازی پذیرش با POS
        /// </summary>
        [HttpPost, Route("finalize/pos")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> FinalizeWithPos(Controllers.Api.FinalizePosRequest request)
        {
            // ✅ Generate Correlation ID for tracking
            var correlationId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // ✅ Log START with all details
                _logger?.Information("💰 POS PAYMENT START - CorrelationId: {CorrelationId}, ReceptionId: {ReceptionId}, Amount: {Amount}, IdempotencyKey: {IdempotencyKey}, UserAgent: {UserAgent}, IP: {IP}",
                    correlationId, 
                    request?.ReceptionId, 
                    request?.Amount, 
                    request?.IdempotencyKey,
                    Request.UserAgent,
                    Request.UserHostAddress);

                if (request == null || request.ReceptionId <= 0)
                {
                    _logger?.Warning("⚠️ POS PAYMENT VALIDATION FAILED - CorrelationId: {CorrelationId}, Reason: Invalid ReceptionId", correlationId);
                    return Json(ServiceResult.Failed("درخواست نامعتبر است. ReceptionId الزامی است.", "VALIDATION"));
                }
                
                // ✅ اعتبارسنجی اولیه مبلغ (validation دقیق‌تر در Facade انجام می‌شود)
                if (request.Amount < 0)
                {
                    _logger?.Warning("⚠️ POS PAYMENT VALIDATION FAILED - CorrelationId: {CorrelationId}, Reason: Negative Amount: {Amount}", correlationId, request.Amount);
                    return Json(ServiceResult.Failed("مبلغ پرداخت نمی‌تواند منفی باشد.", "VALIDATION"));
                }

                if (_facade != null)
                {
                    _logger?.Information("📦 CONTROLLER: ساخت Facade Request - ReceptionId: {ReceptionId}, Amount: {Amount}, PosPayment: {PosPayment}",
                        request.ReceptionId, request.Amount, request.PosPayment != null ? "Present" : "Null");
                    
                    var facadeRequest = new ViewModels.Reception.FinalizePosRequest
                    {
                        ReceptionId = request.ReceptionId,
                        AmountIRR = request.Amount,
                        IdempotencyKey = request.IdempotencyKey ?? System.Guid.NewGuid().ToString(),
                        Pos = request.PosPayment != null ? new ViewModels.Reception.PosPaymentDto
                        {
                            Amount = request.PosPayment.Amount,
                            RRN = request.PosPayment.RRN,
                            TraceNo = request.PosPayment.TraceNo,
                            TerminalId = request.PosPayment.TerminalId,
                            CardLast4 = request.PosPayment.CardLast4
                        } : null
                    };

                    _logger?.Information("🔄 CONTROLLER: فراخوانی Facade.FinalizePosAsync - CorrelationId: {CorrelationId}, ReceptionId: {ReceptionId}, AmountIRR: {AmountIRR}, RRN: {RRN}, TraceNo: {TraceNo}",
                        correlationId, facadeRequest.ReceptionId, facadeRequest.AmountIRR, facadeRequest.Pos?.RRN, facadeRequest.Pos?.TraceNo);

                    var result = await _facade.FinalizePosAsync(facadeRequest);
                    
                    stopwatch.Stop();
                    
                    if (result.Success)
                    {
                        _logger?.Information("✅ POS PAYMENT SUCCESS - CorrelationId: {CorrelationId}, ReceptionId: {ReceptionId}, Amount: {Amount}, Duration: {Duration}ms",
                            correlationId, request.ReceptionId, request.Amount, stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        _logger?.Warning("⚠️ POS PAYMENT FAILED - CorrelationId: {CorrelationId}, ReceptionId: {ReceptionId}, Error: {Error}, Code: {Code}, Duration: {Duration}ms", 
                            correlationId, request.ReceptionId, result.Message, result.Code, stopwatch.ElapsedMilliseconds);
                    }
                    
                    return Json(result);
                }

                _logger?.Error("❌ POS PAYMENT ERROR - CorrelationId: {CorrelationId}, Reason: Facade is null", correlationId);
                return Json(ServiceResult.Failed("سرویس در دسترس نیست.", "SERVICE_UNAVAILABLE"));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.Error(ex, "❌ POS PAYMENT EXCEPTION - CorrelationId: {CorrelationId}, ReceptionId: {ReceptionId}, Duration: {Duration}ms, Exception: {Exception}",
                    correlationId, request?.ReceptionId, stopwatch.ElapsedMilliseconds, ex.Message);
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED").WithExceptionDev(ex));
            }
        }

        /// <summary>
        /// POST /api/v1/reception/finalize/cash
        /// نهایی‌سازی پذیرش با نقدی
        /// </summary>
        [HttpPost, Route("finalize/cash")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> FinalizeWithCash(Controllers.Api.FinalizeCashRequest request)
        {
            try
            {
                _logger?.Information("🏥 V1 API: نهایی‌سازی با نقدی - ReceptionId: {ReceptionId}", request?.ReceptionId);

                if (request == null || request.ReceptionId <= 0)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است. ReceptionId الزامی است.", "VALIDATION"));
                }
                
                // ✅ اعتبارسنجی اولیه مبلغ (validation دقیق‌تر در Facade انجام می‌شود)
                if (request.Amount < 0)
                {
                    return Json(ServiceResult.Failed("مبلغ پرداخت نمی‌تواند منفی باشد.", "VALIDATION"));
                }

                if (_facade != null)
                {
                    var facadeRequest = new ViewModels.Reception.FinalizeCashRequest
                    {
                        ReceptionId = request.ReceptionId,
                        AmountIRR = request.Amount,
                        IdempotencyKey = request.IdempotencyKey ?? System.Guid.NewGuid().ToString(),
                        Cash = request.CashPayment != null ? new ViewModels.Reception.CashPaymentDto
                        {
                            CashSessionId = request.CashPayment.CashSessionId
                        } : null
                    };

                    var result = await _facade.FinalizeCashAsync(facadeRequest);
                    
                    if (result.Success)
                    {
                        _logger?.Information("✅ V1 API: پذیرش با موفقیت نهایی شد - ReceptionId: {ReceptionId}", request.ReceptionId);
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: نهایی‌سازی پذیرش ناموفق - ReceptionId: {ReceptionId}, Error: {Error}", 
                            request.ReceptionId, result.Message);
                    }
                    
                    return Json(result);
                }

                return Json(ServiceResult.Failed("سرویس در دسترس نیست.", "SERVICE_UNAVAILABLE"));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در نهایی‌سازی Cash - ReceptionId: {ReceptionId}", request?.ReceptionId);
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED").WithExceptionDev(ex));
            }
        }

        /// <summary>
        /// GET /api/v1/reception/edit/{id}
        /// دریافت اطلاعات پذیرش برای ویرایش
        /// </summary>
        [HttpGet, Route("edit/{id:int}")]
        public async Task<ActionResult> GetReceptionForEdit(int id)
        {
            try
            {
                _logger.Information("🏥 V1 API: دریافت پذیرش برای ویرایش - ReceptionId: {Id}", id);

                var result = await _facade.LoadReceptionForEditAsync(id);

                if (result.Success)
                {
                    return Json(ServiceResult<ReceptionEditLoadDto>.Successful(result.Data), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(ServiceResult<ReceptionEditLoadDto>.Failed(result.Message, result.Code ?? "ERROR"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ V1 API: خطا در دریافت پذیرش برای ویرایش - ReceptionId: {Id}", id);
                return Json(ServiceResult<ReceptionEditLoadDto>.Failed($"خطا در دریافت پذیرش: {ex.Message}", "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/update
        /// به‌روزرسانی پذیرش
        /// </summary>
        [HttpPost, Route("update")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> UpdateReception(UpdateReceptionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(ServiceResult<UpdateReceptionResponse>.Failed("درخواست نامعتبر است", "INVALID_REQUEST"), JsonRequestBehavior.AllowGet);
                }

                _logger.Information("🏥 V1 API: به‌روزرسانی پذیرش - ReceptionId: {ReceptionId}", request.ReceptionId);

                var result = await _facade.UpdateReceptionAsync(request);

                if (result.Success)
                {
                    return Json(ServiceResult<UpdateReceptionResponse>.Successful(result.Data), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(ServiceResult<UpdateReceptionResponse>.Failed(result.Message, result.Code ?? "ERROR"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ V1 API: خطا در به‌روزرسانی پذیرش - ReceptionId: {ReceptionId}", request?.ReceptionId ?? 0);
                return Json(ServiceResult<UpdateReceptionResponse>.Failed($"خطا در به‌روزرسانی پذیرش: {ex.Message}", "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/cancel
        /// لغو پذیرش
        /// </summary>
        [HttpPost, Route("cancel")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<ActionResult> CancelReception(CancelReceptionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(ServiceResult<CancelReceptionResponse>.Failed("درخواست نامعتبر است", "INVALID_REQUEST"), JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 10)
                {
                    return Json(ServiceResult<CancelReceptionResponse>.Failed("دلیل لغو الزامی است و باید حداقل 10 کاراکتر باشد", "INVALID_REASON"), JsonRequestBehavior.AllowGet);
                }

                _logger.Information("🚫 V1 API: لغو پذیرش - ReceptionId: {ReceptionId}", request.ReceptionId);

                var result = await _facade.CancelReceptionAsync(request);

                if (result.Success)
                {
                    return Json(ServiceResult<CancelReceptionResponse>.Successful(result.Data), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(ServiceResult<CancelReceptionResponse>.Failed(result.Message, result.Code ?? "ERROR"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ V1 API: خطا در لغو پذیرش - ReceptionId: {ReceptionId}", request?.ReceptionId ?? 0);
                return Json(ServiceResult<CancelReceptionResponse>.Failed($"خطا در لغو پذیرش: {ex.Message}", "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// GET /api/v1/reception/details/{id}
        /// دریافت جزئیات کامل پذیرش برای نمایش در Modal
        /// </summary>
        [HttpGet, Route("details/{id:int}")]
        public async Task<ActionResult> GetReceptionDetails(int id)
        {
            try
            {
                _logger.Information("🏥 V1 API: دریافت جزئیات کامل پذیرش - ReceptionId: {Id}", id);

                if (id <= 0)
                {
                    return Json(ServiceResult<ReceptionDetailsFullDto>.Failed("شناسه پذیرش نامعتبر است", "INVALID_ID"), JsonRequestBehavior.AllowGet);
                }

                var result = await _facade.GetReceptionDetailsFullAsync(id);

                if (result.Success)
                {
                    return Json(ServiceResult<ReceptionDetailsFullDto>.Successful(result.Data), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(ServiceResult<ReceptionDetailsFullDto>.Failed(result.Message, result.Code ?? "ERROR"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ V1 API: خطا در دریافت جزئیات کامل پذیرش - ReceptionId: {Id}", id);
                return Json(ServiceResult<ReceptionDetailsFullDto>.Failed($"خطا در دریافت جزئیات پذیرش: {ex.Message}", "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/insurance/check-status
        /// بررسی جامع وضعیت بیمه بیمار - برای استفاده در فرم پذیرش
        /// 
        /// این endpoint برای بررسی وضعیت بیمه و نمایش هشدارهای واضح به منشی‌ها استفاده می‌شود
        /// </summary>
        [HttpPost, Route("insurance/check-status")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<JsonResult> CheckInsuranceStatus(InsuranceStatusCheckRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است", "INVALID_REQUEST"), JsonRequestBehavior.AllowGet);
                }

                var patientId = request.PatientId;
                var checkDate = request.CheckDate;

                _logger?.Information("🏥 V1 API: بررسی وضعیت بیمه. PatientId: {PatientId}, CheckDate: {CheckDate}",
                    patientId, checkDate);

                if (patientId <= 0)
                {
                    return Json(ServiceResult.Failed("شناسه بیمار نامعتبر است", "INVALID_PATIENT_ID"), JsonRequestBehavior.AllowGet);
                }

                var result = await _insuranceStatusChecker.CheckInsuranceForReceptionAsync(patientId, checkDate ?? DateTime.Now);

                if (!result.Success)
                {
                    _logger?.Warning("⚠️ V1 API: خطا در بررسی وضعیت بیمه. PatientId: {PatientId}, Error: {Error}",
                        patientId, result.Message);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                _logger?.Information("✅ V1 API: بررسی وضعیت بیمه تکمیل شد. PatientId: {PatientId}, IsValid: {IsValid}, CanProceed: {CanProceed}",
                    patientId, result.Data?.IsValid, result.Data?.CanProceedWithReception);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در بررسی وضعیت بیمه. PatientId: {PatientId}", request?.PatientId ?? 0);
                return Json(ServiceResult.Failed("خطا در بررسی وضعیت بیمه: " + ex.Message, "INSURANCE_CHECK_ERROR"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/insurance/check-expiry
        /// بررسی انقضای بیمه - برای هشدار به منشی
        /// </summary>
        [HttpPost, Route("insurance/check-expiry")]
        [ValidateAntiForgeryTokenOnPosts]
        public async Task<JsonResult> CheckInsuranceExpiry(InsuranceExpiryCheckRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است", "INVALID_REQUEST"), JsonRequestBehavior.AllowGet);
                }

                var patientId = request.PatientId;
                var warningDays = request.WarningDays ?? 30;

                _logger?.Information("🏥 V1 API: بررسی انقضای بیمه. PatientId: {PatientId}, WarningDays: {WarningDays}",
                    patientId, warningDays);

                if (patientId <= 0)
                {
                    return Json(ServiceResult.Failed("شناسه بیمار نامعتبر است", "INVALID_PATIENT_ID"), JsonRequestBehavior.AllowGet);
                }

                var result = await _insuranceStatusChecker.CheckInsuranceExpiryAsync(patientId, DateTime.Now, warningDays);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "❌ V1 API: خطا در بررسی انقضای بیمه. PatientId: {PatientId}", request?.PatientId ?? 0);
                return Json(ServiceResult.Failed("خطا در بررسی انقضای بیمه: " + ex.Message, "INSURANCE_EXPIRY_CHECK_ERROR"), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }

    /// <summary>
    /// DTO برای حذف Draft ناقص
    /// </summary>
    public class DeleteIncompleteDraftRequest
    {
        public int ReceptionId { get; set; }
    }

    /// <summary>
    /// DTO برای درخواست تنظیم بیمه‌ها
    /// </summary>
    public class SetInsurancesRequestDto
    {
        public int ReceptionId { get; set; }
        public int? BasePlanId { get; set; }
        public int? SupplementaryPlanId { get; set; }
    }

    /// <summary>
    /// DTO برای درخواست جستجوی بیمار
    /// </summary>
    public class PatientLookupRequestDto
    {
        public string NationalCode { get; set; }
        public string Mobile { get; set; }
    }

    /// <summary>
    /// DTO برای درخواست بررسی وضعیت بیمه
    /// </summary>
    public class InsuranceStatusCheckRequest
    {
        public int PatientId { get; set; }
        public DateTime? CheckDate { get; set; }
    }

    /// <summary>
    /// DTO برای درخواست بررسی انقضای بیمه
    /// </summary>
    public class InsuranceExpiryCheckRequest
    {
        public int PatientId { get; set; }
        public int? WarningDays { get; set; }
    }
}

