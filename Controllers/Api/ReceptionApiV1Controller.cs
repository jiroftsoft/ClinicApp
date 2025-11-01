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
using ClinicApp.Models;
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
    /// و 404/500 از بین برود. بعداً می‌تونیم بقیه اکشن‌ها رو هم بهش اضافه کنیم یا به فاساد وصل کنیم.
    /// </summary>
    [RoutePrefix("api/v1/reception")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [ReceptionV2Controller.NoCache]
    public class ReceptionApiV1Controller : Controller
    {
        #region Dependencies

        private readonly IFinancialYearService _fy;
        private readonly IReceptionFacade _facade;
        private readonly IReceptionPricingService _pricing;
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
            ILogger logger,
            ApplicationDbContext context)
        {
            _fy = fy ?? throw new ArgumentNullException(nameof(fy));
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
            _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
            _logger = logger?.ForContext<ReceptionApiV1Controller>() ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Fallback ctor (اگر DI هنوز ثبت نشده)
        /// </summary>
        public ReceptionApiV1Controller()
            : this(
                  DependencyResolver.Current.GetService<IFinancialYearService>(),
                  DependencyResolver.Current.GetService<IReceptionFacade>(),
                  DependencyResolver.Current.GetService<IReceptionPricingService>(),
                  DependencyResolver.Current.GetService<ILogger>(),
                  DependencyResolver.Current.GetService<ApplicationDbContext>())
        {
        }

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
                _logger?.Information("🏥 V1 API: Create Draft - PatientId: {PatientId}, ClinicId: {ClinicId}, DeptId: {DeptId}, DoctorId: {DoctorId}",
                    request?.PatientId, request?.ClinicId, request?.DepartmentId, request?.DoctorId);

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
        /// POST /api/v1/reception/patient/lookup-or-create
        /// جستجو یا ایجاد بیمار (Idempotent: هم lookup هم quick create)
        /// </summary>
        [HttpPost, Route("patient/lookup-or-create")]
        [ValidateAntiForgeryTokenOnPosts]
        public async System.Threading.Tasks.Task<ActionResult> PatientLookupOrCreate(PatientQuickCreateDto request)
        {
            try
            {
                _logger?.Information("🏥 V1 API: Patient Lookup-Or-Create - NationalCode: {NationalCode}, HasQuickCreateData: {HasData}", 
                    request?.NationalCode, !string.IsNullOrWhiteSpace(request?.FirstName));

                // ✅ بررسی الزامات پایه
                if (string.IsNullOrWhiteSpace(request?.NationalCode))
                {
                    return Json(ServiceResult<PatientLookupResponseDto>
                        .Failed("کد ملی الزامی است.", ReceptionApiCodes.VALIDATION)
                        .WithValidationError("NationalCode", "کد ملی الزامی است."));
                }

                // ✅ بررسی اعتبار کد ملی (10 رقم)
                if (request.NationalCode.Length != 10 || !System.Text.RegularExpressions.Regex.IsMatch(request.NationalCode, @"^\d{10}$"))
                {
                    return Json(ServiceResult<PatientLookupResponseDto>
                        .Failed("کد ملی باید 10 رقم عددی باشد.", ReceptionApiCodes.VALIDATION)
                        .WithValidationError("NationalCode", "کد ملی باید 10 رقم عددی باشد."));
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
                            var findResult = await facadeImpl.FindOrCreatePatientAsync(request.NationalCode, null);
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
                                                Mobile = patient.PhoneNumber,
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
                                
                                // Fallback
                                return Json(ServiceResult.Failed("بیمار یافت نشد. لطفاً ثبت سریع بیمار را تکمیل کنید.", "NOT_FOUND"));
                            }
                            
                            return Json(ServiceResult.Failed("بیمار یافت نشد. لطفاً ثبت سریع بیمار را تکمیل کنید.", "NOT_FOUND"));
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
                            NationalCode = request.NationalCode,
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            FatherName = request.FatherName,
                            PhoneNumber = request.Mobile,
                            Gender = request.Gender,
                            BirthDate = birthDate,
                            Address = request.Address
                        };
                        
                        var createResult = await facadeImpl.FindOrCreatePatientAsync(request.NationalCode, quickCreateDto);
                        if (createResult.Success && createResult.Data != null)
                        {
                            var patientDto = createResult.Data;
                            var patientId = patientDto.PatientId;
                            
                            if (patientId > 0)
                            {
                                // ایجاد/اتصال بیمه‌ها اگر مشخص شده باشند
                                if (request.BaseInsurancePlanId.HasValue || request.SupplementaryInsurancePlanId.HasValue)
                                {
                                    await facadeImpl.SetPatientInsurancesAsync(patientId, request.BaseInsurancePlanId, request.SupplementaryInsurancePlanId);
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
                                            Mobile = patient.PhoneNumber,
                                            Phone = null,
                                            Address = patient.Address,
                                            Gender = patient.Gender.ToString(),
                                            BirthDateShamsi = patient.BirthDate?.ToPersianDate() ?? string.Empty
                                        },
                                        Insurance = insurances
                                    };
                                    
                                    return Json(ServiceResult<Controllers.Api.PatientLookupResponseDto>.Successful(response, "بیمار با موفقیت ثبت شد."));
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
                        // ✅ تبدیل DoctorDto به DoctorOptionDto برای پاسخ یکنواخت
                        var doctors = result.Data.Select(d => new DoctorOptionDto
                        {
                            DoctorId = d.DoctorId,
                            FullName = d.FullName ?? $"{d.FirstName} {d.LastName}".Trim(),
                            Title = d.Specialization ?? "",
                            DepartmentName = "", // TODO: از Department بگیریم اگر لازم است
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
        /// دریافت خدمات یک دپارتمان
        /// </summary>
        [HttpGet, Route("services/by-department")]
        public async Task<ActionResult> GetServicesByDepartment(int? deptId)
        {
            try
            {
                _logger?.Information("🏥 V1 API: دریافت خدمات - DeptId: {DeptId}", deptId);

                if (!deptId.HasValue || deptId.Value <= 0)
                {
                    return Json(ServiceResult<object>.Failed("شناسه دپارتمان نامعتبر است.", "VALIDATION"), JsonRequestBehavior.AllowGet);
                }

                var result = await _facade.GetServicesForDeptAsync(deptId.Value);
                
                if (result.Success && result.Data != null)
                {
                    // تبدیل به فرمت مورد نیاز frontend
                    var payload = new
                    {
                        services = result.Data.Services.Select(s => new
                        {
                            serviceId = s.ServiceId,
                            serviceCode = s.ServiceCode,
                            serviceName = s.ServiceName,
                            price = s.UnitPrice,
                            unitPriceIRR = s.UnitPrice, // Alias
                            isActive = s.IsActive
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
                    
                    if (result.Success && result.Data != null)
                    {
                        _logger?.Information("✅ V1 API: آیتم با موفقیت افزوده شد - ReceptionId: {ReceptionId}", request.ReceptionId);
                        
                        // ✅ محاسبه Pricing برای آخرین آیتم افزوده شده و Totals
                        try
                        {
                            // پیدا کردن آخرین ReceptionItem برای این Reception
                            var lastItem = await _context.ReceptionItems
                                .Where(i => i.ReceptionId == request.ReceptionId && 
                                           i.ServiceId == request.ServiceId && 
                                           !i.IsDeleted)
                                .OrderByDescending(i => i.ReceptionItemId)
                                .FirstOrDefaultAsync();
                            
                            if (lastItem != null)
                            {
                                // ✅ محاسبه Pricing برای آخرین آیتم
                                var pricing = await _pricing.PriceItemAsync(request.ReceptionId, lastItem.ReceptionItemId);
                                
                                // ✅ محاسبه Totals برای کل Reception
                                var totals = await _pricing.CalculateTotalsAsync(request.ReceptionId);
                                
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
                                    pricing,
                                    totals
                                }, "آیتم اضافه و محاسبه شد."));
                            }
                            else
                            {
                                // Fallback: فقط Totals را برگردان
                                var totals = await _pricing.CalculateTotalsAsync(request.ReceptionId);
                                return Json(ServiceResult<object>.Successful(new { totals }, result.Message));
                            }
                        }
                        catch (Exception pricingEx)
                        {
                            _logger?.Warning(pricingEx, "⚠️ V1 API: خطا در محاسبه Pricing/Totals پس از AddItem - ReceptionId: {ReceptionId}", 
                                request.ReceptionId);
                            // Fallback: فقط نتیجه AddItem را برگردان
                            return Json(result);
                        }
                    }
                    else
                    {
                        _logger?.Warning("⚠️ V1 API: افزودن آیتم ناموفق - ReceptionId: {ReceptionId}, Error: {Error}", 
                            request.ReceptionId, result?.Message);
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
                        // ✅ تبدیل DoctorDto به DoctorOptionDto برای پاسخ یکنواخت
                        var doctors = result.Data.Select(d => new DoctorOptionDto
                        {
                            DoctorId = d.DoctorId,
                            FullName = d.FullName ?? $"{d.FirstName} {d.LastName}".Trim(),
                            Title = d.Specialization ?? "",
                            DepartmentName = "", // TODO: از Department بگیریم اگر لازم است
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
            try
            {
                _logger?.Information("🏥 V1 API: نهایی‌سازی با POS - ReceptionId: {ReceptionId}", request?.ReceptionId);

                if (request == null || request.ReceptionId <= 0)
                {
                    return Json(ServiceResult.Failed("درخواست نامعتبر است. ReceptionId الزامی است.", "VALIDATION"));
                }

                if (_facade != null)
                {
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

                    var result = await _facade.FinalizePosAsync(facadeRequest);
                    
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
                _logger?.Error(ex, "❌ V1 API: خطا در نهایی‌سازی POS - ReceptionId: {ReceptionId}", request?.ReceptionId);
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

        #endregion
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
}

