using System;
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
            ILogger logger,
            ApplicationDbContext context)
        {
            _fy = fy ?? throw new ArgumentNullException(nameof(fy));
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
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
                _logger?.Information("🏥 V1 API: Bootstrap - ClinicId: {ClinicId}, DeptId: {DeptId}", clinicId, deptId);

                if (_facade != null)
                {
                    var result = await _facade.LoadInitialAsync(clinicId ?? 1, deptId);
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
                        return Json(ServiceResult<object>.Successful(payload, result.Message ?? "عملیات با موفقیت انجام شد."), JsonRequestBehavior.AllowGet);
                    }
                }

                // Fallback: اسکلت حداقلی
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
                _logger?.Error(ex, "خطا در Bootstrap");
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST /api/v1/reception/draft/create
        /// ایجاد پیش‌نویس پذیرش
        /// </summary>
        [HttpPost, Route("draft/create")]
        [ValidateAntiForgeryTokenOnPosts]
        public ActionResult CreateDraft()
        {
            try
            {
                _logger?.Information("🏥 V1 API: Create Draft");

                // TODO: اتصال به ReceptionFacade.CreateDraftAsync() وقتی آماده شد
                var draftId = Guid.NewGuid().ToString("N");
                _logger?.Information("🏥 V1 API: Draft created - DraftId: {DraftId}", draftId);
                return Json(ServiceResult<string>.Successful(draftId, "Draft created."));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "خطا در Create Draft");
                return Json(ServiceResult.Failed("UNHANDLED: " + ex.Message, "UNHANDLED"));
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

                if (string.IsNullOrWhiteSpace(request?.NationalCode))
                {
                    return Json(ServiceResult<PatientLookupResponseDto>.Failed("کد ملی الزامی است.", "VALIDATION"));
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
                        
                        // اگر اطلاعات هویت آمده (Quick Create)
                        var quickCreateDto = new ViewModels.Reception.PatientCreateDto
                        {
                            NationalCode = request.NationalCode,
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            PhoneNumber = request.Mobile,
                            Gender = request.Gender,
                            BirthDate = !string.IsNullOrWhiteSpace(request.BirthDateShamsi) 
                                ? Helpers.PersianDateHelper.ToGregorianDate(request.BirthDateShamsi) 
                                : (DateTime?)null,
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

