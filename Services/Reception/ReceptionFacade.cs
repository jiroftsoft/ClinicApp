using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Controllers.Api;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.Finance;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Insurance;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using Serilog;
using AddItemRequest = ClinicApp.ViewModels.Reception.AddItemRequest;
using FinalizeCashRequest = ClinicApp.ViewModels.Reception.FinalizeCashRequest;
using FinalizePosRequest = ClinicApp.ViewModels.Reception.FinalizePosRequest;
using SetInsurancesRequest = ClinicApp.ViewModels.Reception.SetInsurancesRequest;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// Facade مخصوص ماژول پذیرش - Orchestrator نازک
    /// 
    /// مسئولیت: هماهنگی سرویس‌های موجود بدون اضافه کردن منطق جدید
    /// هدف: API-محور و اتمیک کردن فراخوانی‌ها
    /// 
    /// Architecture Principles:
    /// ✅ Facade Pattern: یک نقطه ورود برای ماژول پذیرش
    /// ✅ Orchestration: هماهنگی سرویس‌های موجود
    /// ✅ No Business Logic: فقط ترتیب فراخوانی‌ها
    /// ✅ ServiceResult<T>: پاسخ یکپارچه
    /// </summary>
    public class ReceptionFacade : IReceptionFacade
    {
        #region Dependencies

        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly ServiceCalculationEngine _serviceCalculationEngine;
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly IReceptionWorkflowService _receptionWorkflowService;
        private readonly IDepartmentManagementService _departmentManagementService;
        private readonly IPatientService _patientService;
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly IPosManagementService _posManagementService;
        private readonly IReceptionRepository _receptionRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFinancialYearService _financialYearService;
        private readonly InsurancePlanSuggestionService _insurancePlanSuggestionService;
        private readonly IFactorSettingService _factorSettingService;
        private readonly Services.Pricing.Interfaces.IPricingEngine _pricingEngine;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public ReceptionFacade(
            IServiceCalculationService serviceCalculationService,
            ServiceCalculationEngine serviceCalculationEngine,
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            IReceptionWorkflowService receptionWorkflowService,
            IDepartmentManagementService departmentManagementService,
            IPatientService patientService,
            IPatientInsuranceService patientInsuranceService,
            IPosManagementService posManagementService,
            IReceptionRepository receptionRepository,
            ICurrentUserService currentUserService,
            IFinancialYearService financialYearService,
            InsurancePlanSuggestionService insurancePlanSuggestionService,
            IFactorSettingService factorSettingService,
            Services.Pricing.Interfaces.IPricingEngine pricingEngine,
            ApplicationDbContext context,
            ILogger logger)
        {
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _serviceCalculationEngine = serviceCalculationEngine ?? throw new ArgumentNullException(nameof(serviceCalculationEngine));
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _receptionWorkflowService = receptionWorkflowService ?? throw new ArgumentNullException(nameof(receptionWorkflowService));
            _departmentManagementService = departmentManagementService ?? throw new ArgumentNullException(nameof(departmentManagementService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _posManagementService = posManagementService ?? throw new ArgumentNullException(nameof(posManagementService));
            _receptionRepository = receptionRepository ?? throw new ArgumentNullException(nameof(receptionRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _financialYearService = financialYearService ?? throw new ArgumentNullException(nameof(financialYearService));
            _insurancePlanSuggestionService = insurancePlanSuggestionService ?? throw new ArgumentNullException(nameof(insurancePlanSuggestionService));
            _factorSettingService = factorSettingService ?? throw new ArgumentNullException(nameof(factorSettingService));
            _pricingEngine = pricingEngine ?? throw new ArgumentNullException(nameof(pricingEngine));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger.ForContext<ReceptionFacade>();
        }

        #endregion

        #region Loaders

        /// <summary>
        /// بارگذاری اولیه فرم پذیرش
        /// </summary>
        public async Task<ServiceResult<ReceptionLoadDto>> LoadInitialAsync(int clinicId, int? deptId)
        {
            try
            {
                _logger.Information("🏥 FACADE: بارگذاری اولیه فرم پذیرش - ClinicId: {ClinicId}, DeptId: {DeptId}", clinicId, deptId);

                var result = new ReceptionLoadDto();

                // 1. بارگذاری کلینیک‌ها
                var clinics = await _context.Clinics
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new ClinicDto
                    {
                        ClinicId = c.ClinicId,
                        Name = c.Name,
                        Code = c.Code,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();
                result.Clinics = clinics;

                // 2. بارگذاری دپارتمان‌ها
                var departmentsResult = await _departmentManagementService.GetAllDepartmentsAsync();
                _logger.Information("🔍 FACADE: GetAllDepartmentsAsync result - Success: {Success}, Count: {Count}", 
                    departmentsResult.Success, departmentsResult.Data?.Count ?? 0);
                
                if (departmentsResult.Success)
                {
                    // Convert ClinicAdmin.DepartmentDto to Reception.DepartmentDto
                    result.Departments = departmentsResult.Data.Select(d => new ViewModels.Reception.DepartmentDto
                    {
                        DepartmentId = d.DepartmentId,
                        Name = d.Name,
                        Code = d.Code,
                        IsActive = d.IsActive,
                        Description = d.Description
                    }).ToList();
                    
                    _logger.Information("✅ FACADE: Departments converted - Count: {Count}", result.Departments.Count);
                }
                else
                {
                    _logger.Warning("⚠️ FACADE: GetAllDepartmentsAsync failed - Message: {Message}", departmentsResult.Message);
                    result.Departments = new List<ViewModels.Reception.DepartmentDto>();
                }

                // 3. بارگذاری پزشک‌ها (اگر دپارتمان انتخاب شده)
                // ✅ طبق نقشه پیوندی: بررسی StartDate/EndDate + ClinicId + IsActive
                // 🎯 رویکرد حرفه‌ای: اگر دپارتمان انتخاب شده، فقط پزشکان آن دپارتمان را لود کن
                if (deptId.HasValue)
                {
                    var now = DateTime.Now;
                    
                    // ✅ اصلاح: ابتدا Department را پیدا کن تا ClinicId را بدست آوری
                    var department = await _context.Departments
                        .AsNoTracking()
                        .Where(d => d.DepartmentId == deptId.Value && !d.IsDeleted && d.IsActive)
                        .FirstOrDefaultAsync();
                    
                    if (department == null)
                    {
                        _logger.Warning("⚠️ FACADE: دپارتمان یافت نشد - DepartmentId: {DeptId}", deptId.Value);
                        result.Doctors = new List<DoctorDto>();
                    }
                    else
                    {
                        _logger.Information("🔍 FACADE: بارگذاری پزشکان برای DepartmentId: {DeptId}, DeptName: {DeptName}, DeptClinicId: {DeptClinicId}, Now: {Now}", 
                            deptId.Value, department.Name, department.ClinicId, now);
                        
                        // 🎯 رویکرد حرفه‌ای: فیلترها را به ترتیب اعمال کن و لاگ کن
                        // Step 1: فقط DepartmentId
                        var step1 = await _context.DoctorDepartments
                            .AsNoTracking()
                            .Where(dd => dd.DepartmentId == deptId.Value)
                            .CountAsync();
                        
                        _logger.Information("🔍 FACADE: Step 1 - فقط DepartmentId: {Count}", step1);
                        
                        // Step 2: + Doctor IsActive/IsDeleted
                        var step2 = await _context.DoctorDepartments
                            .AsNoTracking()
                            .Include(dd => dd.Doctor)
                            .Where(dd => dd.DepartmentId == deptId.Value &&
                                       !dd.Doctor.IsDeleted &&
                                       dd.Doctor.IsActive)
                            .CountAsync();
                        
                        _logger.Information("🔍 FACADE: Step 2 - + Doctor IsActive/IsDeleted: {Count}", step2);
                        
                        // Step 3: + DoctorDepartment IsActive/IsDeleted
                        var step3 = await _context.DoctorDepartments
                            .AsNoTracking()
                            .Include(dd => dd.Doctor)
                            .Where(dd => dd.DepartmentId == deptId.Value &&
                                       !dd.Doctor.IsDeleted &&
                                       dd.Doctor.IsActive &&
                                       !dd.IsDeleted &&
                                       dd.IsActive)
                            .CountAsync();
                        
                        _logger.Information("🔍 FACADE: Step 3 - + DoctorDepartment IsActive/IsDeleted: {Count}", step3);
                        
                        // Step 4: + Date Range
                        // 🎯 طبق قرارداد: StartDate می‌تواند null باشد (یعنی از ابتدا فعال) یا در گذشته/حال/آینده باشد
                        // 🎯 منطق: اگر StartDate در آینده است، یعنی یک انتساب پیش‌رو است و باید نمایش داده شود
                        // 🎯 EndDate: اگر null باشد یعنی فعال است، اگر در گذشته باشد یعنی غیرفعال شده
                        // ✅ اصلاح: فقط EndDate را چک کن (اگر EndDate != null && EndDate <= now، یعنی غیرفعال شده)
                        var step4 = await _context.DoctorDepartments
                            .AsNoTracking()
                            .Include(dd => dd.Doctor)
                            .Where(dd => dd.DepartmentId == deptId.Value &&
                                       !dd.Doctor.IsDeleted &&
                                       dd.Doctor.IsActive &&
                                       !dd.IsDeleted &&
                                       dd.IsActive &&
                                       (dd.EndDate == null || dd.EndDate > now)) // ✅ فقط EndDate را چک کن (اگر EndDate در گذشته باشد، ignore کن)
                            .CountAsync();
                        
                        _logger.Information("🔍 FACADE: Step 4 - + Date Range (only EndDate check): {Count}", step4);
                        
                        // 🎯 Query نهایی: اگر Step 4 صفر است، اما Step 3 > 0 است، فیلتر تاریخ را ignore کن
                        var doctorDepartments = new List<Models.Entities.Doctor.DoctorDepartment>();
                        
                        // 🎯 Query نهایی: فقط EndDate را چک کن (اگر EndDate در گذشته باشد، ignore کن)
                        // ✅ StartDate را ignore می‌کنیم چون ممکن است در آینده باشد (انتساب پیش‌رو)
                        if (step3 > 0)
                        {
                            doctorDepartments = await _context.DoctorDepartments
                                .AsNoTracking()
                                .Include(dd => dd.Doctor)
                                .Include(dd => dd.Department)
                                .Include(dd => dd.Doctor.DoctorSpecializations)
                                .Include(dd => dd.Doctor.DoctorSpecializations.Select(ds => ds.Specialization))
                                .Where(dd => dd.DepartmentId == deptId.Value &&
                                           !dd.Doctor.IsDeleted &&
                                           dd.Doctor.IsActive &&
                                           !dd.IsDeleted &&
                                           dd.IsActive &&
                                           (dd.EndDate == null || dd.EndDate > now)) // ✅ فقط EndDate را چک کن
                                .ToListAsync();
                            
                            _logger.Information("✅ FACADE: Query نهایی - Count: {Count} (فقط EndDate چک شده است، StartDate ignore شده)", doctorDepartments.Count);
                        }
                        else
                        {
                            _logger.Warning("⚠️ FACADE: هیچ پزشکی برای DepartmentId {DeptId} پیدا نشد (Step 3 = 0)", deptId.Value);
                            doctorDepartments = new List<Models.Entities.Doctor.DoctorDepartment>();
                        }
                        
                        // Map به DoctorDto (بعد از materialize شدن برای استفاده از computed property)
                        var doctors = doctorDepartments.Select(dd => new DoctorDto
                        {
                            DoctorId = dd.DoctorId,
                            FirstName = dd.Doctor.FirstName ?? "",
                            LastName = dd.Doctor.LastName ?? "",
                            DoctorCode = dd.Doctor.DoctorCode ?? "",
                            Specialization = dd.Doctor.SpecializationName ?? "", // استفاده از computed property
                            IsActive = dd.Doctor.IsActive
                        }).ToList();
                        
                        _logger.Information("✅ FACADE: پزشکان map شده - Count: {Count}", doctors.Count);
                        if (doctors.Count > 0)
                        {
                            _logger.Information("✅ FACADE: اولین پزشک - DoctorId: {DoctorId}, Name: {FirstName} {LastName}", 
                                doctors[0].DoctorId, doctors[0].FirstName, doctors[0].LastName);
                        }
                        
                        result.Doctors = doctors;
                    }
                }
                else
                {
                    _logger.Information("🔍 FACADE: deptId ندارد، Doctors را خالی می‌گذاریم");
                    result.Doctors = new List<DoctorDto>();
                }

                // 4. بارگذاری خدمات دپارتمان (اگر انتخاب شده)
                if (deptId.HasValue)
                {
                    var servicesResult = await GetServicesForDeptAsync(deptId.Value);
                    if (servicesResult.Success)
                    {
                        result.Services = servicesResult.Data.Services.Select(s => new ServiceDto
                        {
                            ServiceId = s.ServiceId,
                            ServiceCode = s.ServiceCode,
                            ServiceName = s.ServiceName,
                            Price = s.UnitPrice,
                            IsActive = s.IsActive
                        }).ToList();
                    }
                }

                // 5. بارگذاری خدمات مشترک
                var sharedServicesResult = await GetSharedServicesAsync();
                if (sharedServicesResult.Success)
                {
                    result.SharedServices = sharedServicesResult.Data;
                }

                // 6. بارگذاری تنظیمات ضرایب (FactorSetting) برای سال مالی جاری
                var financialYear = _financialYearService.GetCurrentYear();
                try
                {
                    var techFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                        ServiceComponentType.Technical, false, financialYear);
                    var techFactorHashtagged = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                        ServiceComponentType.Technical, true, financialYear);
                    var profFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                        ServiceComponentType.Professional, false, financialYear);
                    var profFactorHashtagged = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                        ServiceComponentType.Professional, true, financialYear);

                    // تبدیل به bool برای جلوگیری از خطای CS0019
                    var techFactorIsActive = techFactor?.IsActive ?? false;
                    var profFactorIsActive = profFactor?.IsActive ?? false;
                    var techFactorIsFrozen = techFactor?.IsFrozen ?? false;
                    var profFactorIsFrozen = profFactor?.IsFrozen ?? false;

                    result.FactorSetting = new FactorSettingDto
                    {
                        FinancialYear = financialYear,
                        TechnicalFactor = techFactor?.Value,
                        TechnicalFactorHashtagged = techFactorHashtagged?.Value,
                        ProfessionalFactor = profFactor?.Value,
                        ProfessionalFactorHashtagged = profFactorHashtagged?.Value,
                        IsActive = techFactorIsActive || profFactorIsActive,
                        IsFrozen = techFactorIsFrozen || profFactorIsFrozen
                    };

                    _logger.Information("✅ FACADE: تنظیمات ضرایب بارگذاری شد - FinancialYear: {Year}, Tech: {Tech}, Prof: {Prof}", 
                        financialYear, techFactor?.Value ?? 0, profFactor?.Value ?? 0);
                }
                catch (Exception factorEx)
                {
                    _logger.Warning(factorEx, "⚠️ FACADE: خطا در بارگذاری تنظیمات ضرایب - FinancialYear: {Year}", financialYear);
                    // FactorSetting را null می‌گذاریم (optional)
                    result.FactorSetting = null;
                }

                _logger.Information("✅ FACADE: بارگذاری اولیه تکمیل شد - Clinics: {ClinicCount}, Departments: {DeptCount}, Doctors: {DoctorCount}, Services: {ServiceCount}", 
                    result.Clinics?.Count ?? 0,
                    result.Departments?.Count ?? 0, 
                    result.Doctors?.Count ?? 0,
                    result.Services?.Count ?? 0);

                return ServiceResult<ReceptionLoadDto>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در بارگذاری اولیه فرم پذیرش - ClinicId: {ClinicId}, DeptId: {DeptId}, Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                    clinicId, deptId, ex.GetType().Name, ex.Message, ex.StackTrace);
                return ServiceResult<ReceptionLoadDto>.Failed($"خطا در بارگذاری اولیه فرم پذیرش: {ex.Message}");
            }
        }

        /// <summary>
        /// دریافت پزشکان یک دپارتمان
        /// </summary>
        public async Task<ServiceResult<List<DoctorDto>>> GetDoctorsByDepartmentAsync(int deptId, int? clinicId = null)
        {
            try
            {
                _logger.Information("🏥 FACADE: دریافت پزشکان دپارتمان - DeptId: {DeptId}, ClinicId: {ClinicId}", deptId, clinicId);

                var now = DateTime.Now;

                // ✅ ابتدا Department را پیدا کن تا ClinicId را بدست آوری
                var department = await _context.Departments
                    .AsNoTracking()
                    .Where(d => d.DepartmentId == deptId && !d.IsDeleted && d.IsActive)
                    .FirstOrDefaultAsync();

                if (department == null)
                {
                    _logger.Warning("⚠️ FACADE: دپارتمان یافت نشد - DepartmentId: {DeptId}", deptId);
                    return ServiceResult<List<DoctorDto>>.Failed($"دپارتمان با شناسه {deptId} یافت نشد");
                }

                // ✅ استفاده از ClinicId از Department اگر clinicId null است
                var effectiveClinicId = clinicId ?? department.ClinicId;

                _logger.Information("🔍 FACADE: بارگذاری پزشکان برای DepartmentId: {DeptId}, DeptName: {DeptName}, ClinicId: {ClinicId}, Now: {Now}", 
                    deptId, department.Name, effectiveClinicId, now);

                // 🎯 Query نهایی: فقط EndDate را چک کن (اگر EndDate در گذشته باشد، ignore کن)
                // ✅ StartDate را ignore می‌کنیم چون ممکن است در آینده باشد (انتساب پیش‌رو)
                // ⚠️ مشکل: Include باعث materialize شدن کامل Doctor entity می‌شود و ممکن است با enum Degree مشکل ایجاد کند
                // ✅ راه‌حل: از Select مستقیم استفاده می‌کنیم تا فقط property های لازم را بگیریم
                // ✅ برای SpecializationName، باید از DoctorSpecializations به صورت مستقیم در Select استفاده کنیم
                // ⚠️ مشکل: در LINQ to Entities نمی‌توانیم از computed property (SpecializationName) استفاده کنیم
                // ✅ راه‌حل: باید از DoctorSpecializations به صورت مستقیم در Select استفاده کنیم
                var doctors = await _context.DoctorDepartments
                    .AsNoTracking()
                    .Where(dd => dd.DepartmentId == deptId &&
                               !dd.Doctor.IsDeleted &&
                               dd.Doctor.IsActive &&
                               !dd.IsDeleted &&
                               dd.IsActive &&
                               (dd.EndDate == null || dd.EndDate > now)) // ✅ فقط EndDate را چک کن
                    .Select(dd => new 
                    {
                        DoctorId = dd.Doctor.DoctorId,
                        FirstName = dd.Doctor.FirstName ?? "",
                        LastName = dd.Doctor.LastName ?? "",
                        DoctorCode = dd.Doctor.DoctorCode ?? "",
                        Specialization = dd.Doctor.DoctorSpecializations
                            .Where(ds => ds.Specialization != null)
                            .Select(ds => ds.Specialization.Name)
                            .FirstOrDefault() ?? "",
                        IsActive = dd.Doctor.IsActive
                    })
                    .Distinct() // جلوگیری از تکراری شدن در صورت چند DoctorDepartment برای یک Doctor
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .Select(d => new DoctorDto
                    {
                        DoctorId = d.DoctorId,
                        FirstName = d.FirstName,
                        LastName = d.LastName,
                        DoctorCode = d.DoctorCode,
                        Specialization = d.Specialization,
                        IsActive = d.IsActive
                    })
                    .ToListAsync();

                _logger.Information("✅ FACADE: Query نهایی - Count: {Count} (فقط EndDate چک شده است، StartDate ignore شده، Select مستقیم بدون Include)", doctors.Count);
                if (doctors.Count > 0)
                {
                    _logger.Information("✅ FACADE: اولین پزشک - DoctorId: {DoctorId}, Name: {FirstName} {LastName}", 
                        doctors[0].DoctorId, doctors[0].FirstName, doctors[0].LastName);
                }

                return ServiceResult<List<DoctorDto>>.Successful(doctors, $"تعداد {doctors.Count} پزشک یافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در دریافت پزشکان دپارتمان - DeptId: {DeptId}, ClinicId: {ClinicId}, Exception: {ExceptionType}, Message: {Message}", 
                    deptId, clinicId, ex.GetType().Name, ex.Message);
                return ServiceResult<List<DoctorDto>>.Failed($"خطا در دریافت پزشکان دپارتمان: {ex.Message}");
            }
        }

        /// <summary>
        /// دریافت پزشکان مجاز برای یک خدمت در دپارتمان
        /// ✅ طبق نقشه پیوندی: فیلتر سه‌لایه (DoctorDepartments + ServiceCategory/SharedService + DoctorServiceCategory + Fallback)
        /// </summary>
        public async Task<ServiceResult<List<DoctorDto>>> GetDoctorsByServiceAsync(int departmentId, int serviceId, int? clinicId = null)
        {
            try
            {
                _logger.Information("🏥 FACADE: دریافت پزشکان مجاز برای خدمت - DeptId: {DeptId}, ServiceId: {ServiceId}, ClinicId: {ClinicId}", 
                    departmentId, serviceId, clinicId);

                var now = DateTime.Now;

                // ✅ 1) بررسی اینکه خدمت در این دپارتمان ارائه می‌شود
                // راه 1: از طریق ServiceCategory.DepartmentId
                // راه 2: از طریق SharedService
                var service = await _context.Services
                    .AsNoTracking()
                    .Include(s => s.ServiceCategory)
                    .Where(s => s.ServiceId == serviceId && !s.IsDeleted && s.IsActive)
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    _logger.Warning("⚠️ FACADE: خدمت یافت نشد - ServiceId: {ServiceId}", serviceId);
                    return ServiceResult<List<DoctorDto>>.Failed($"خدمت با شناسه {serviceId} یافت نشد");
                }

                // بررسی اینکه خدمت در این دپارتمان ارائه می‌شود
                var serviceInDepartment = false;
                
                // راه 1: از طریق ServiceCategory.DepartmentId
                if (service.ServiceCategory != null && 
                    service.ServiceCategory.DepartmentId == departmentId &&
                    !service.ServiceCategory.IsDeleted && 
                    service.ServiceCategory.IsActive)
                {
                    serviceInDepartment = true;
                    _logger.Information("✅ FACADE: خدمت در دپارتمان ارائه می‌شود (از طریق ServiceCategory)");
                }
                else
                {
                    // راه 2: از طریق SharedService
                    var isShared = await _context.SharedServices
                        .AsNoTracking()
                        .AnyAsync(ss => ss.ServiceId == serviceId &&
                                       ss.DepartmentId == departmentId &&
                                       ss.IsActive &&
                                       !ss.IsDeleted);
                    
                    if (isShared)
                    {
                        serviceInDepartment = true;
                        _logger.Information("✅ FACADE: خدمت در دپارتمان ارائه می‌شود (از طریق SharedService)");
                    }
                }

                if (!serviceInDepartment)
                {
                    _logger.Warning("⚠️ FACADE: خدمت در این دپارتمان ارائه نمی‌شود - ServiceId: {ServiceId}, DeptId: {DeptId}", 
                        serviceId, departmentId);
                    return ServiceResult<List<DoctorDto>>.Successful(new List<DoctorDto>(), "خدمت در این دپارتمان ارائه نمی‌شود");
                }

                // ✅ 2) پایه: پزشکان فعال در دپارتمان
                var department = await _context.Departments
                    .AsNoTracking()
                    .Where(d => d.DepartmentId == departmentId && !d.IsDeleted && d.IsActive)
                    .FirstOrDefaultAsync();

                if (department == null)
                {
                    _logger.Warning("⚠️ FACADE: دپارتمان یافت نشد - DepartmentId: {DeptId}", departmentId);
                    return ServiceResult<List<DoctorDto>>.Failed($"دپارتمان با شناسه {departmentId} یافت نشد");
                }

                var effectiveClinicId = clinicId ?? department.ClinicId;

                // ✅ 3) Query پایه: پزشکان فعال در دپارتمان
                var doctorsBase = await _context.DoctorDepartments
                    .AsNoTracking()
                    .Where(dd => dd.DepartmentId == departmentId &&
                               !dd.Doctor.IsDeleted &&
                               dd.Doctor.IsActive &&
                               !dd.IsDeleted &&
                               dd.IsActive &&
                               (dd.EndDate == null || dd.EndDate > now))
                    .Select(dd => new
                    {
                        DoctorId = dd.Doctor.DoctorId,
                        FirstName = dd.Doctor.FirstName ?? "",
                        LastName = dd.Doctor.LastName ?? "",
                        DoctorCode = dd.Doctor.DoctorCode ?? "",
                        Specialization = dd.Doctor.DoctorSpecializations
                            .Where(ds => ds.Specialization != null)
                            .Select(ds => ds.Specialization.Name)
                            .FirstOrDefault() ?? "",
                        IsActive = dd.Doctor.IsActive
                    })
                    .Distinct()
                    .ToListAsync();

                _logger.Information("🔍 FACADE: پزشکان پایه در دپارتمان - Count: {Count}", doctorsBase.Count);

                if (doctorsBase.Count == 0)
                {
                    _logger.Warning("⚠️ FACADE: هیچ پزشکی در دپارتمان فعال نیست");
                    return ServiceResult<List<DoctorDto>>.Successful(new List<DoctorDto>(), "هیچ پزشکی در این دپارتمان فعال نیست");
                }

                // ✅ 4) فیلتر بر اساس DoctorServiceCategory (اگر موجود)
                var doctorIdsByCategory = new List<int>();

                if (service.ServiceCategoryId > 0)
                {
                    doctorIdsByCategory = await _context.DoctorServiceCategories
                        .AsNoTracking()
                        .Where(dsc => dsc.ServiceCategoryId == service.ServiceCategoryId &&
                                     dsc.IsActive &&
                                     !dsc.IsDeleted &&
                                     // بازه اثر (اختیاری)
                                     (dsc.ExpiryDate == null || dsc.ExpiryDate >= now))
                        .Select(dsc => dsc.DoctorId)
                        .Distinct()
                        .ToListAsync();

                    _logger.Information("🔍 FACADE: پزشکان مجاز برای دسته خدمت - CategoryId: {CategoryId}, Count: {Count}", 
                        service.ServiceCategoryId, doctorIdsByCategory.Count);
                }

                // ✅ 5) فیلتر نهایی
                List<DoctorDto> filteredDoctors;

                if (doctorIdsByCategory.Count > 0)
                {
                    // اگر DoctorServiceCategory موجود است، فقط پزشکان مجاز را برگردان
                    filteredDoctors = doctorsBase
                        .Where(d => doctorIdsByCategory.Contains(d.DoctorId))
                        .OrderBy(d => d.LastName)
                        .ThenBy(d => d.FirstName)
                        .Select(d => new DoctorDto
                        {
                            DoctorId = d.DoctorId,
                            FirstName = d.FirstName,
                            LastName = d.LastName,
                            DoctorCode = d.DoctorCode,
                            Specialization = d.Specialization,
                            IsActive = d.IsActive
                        })
                        .ToList();
                    
                    _logger.Information("✅ FACADE: پزشکان فیلتر شده (با DoctorServiceCategory) - Count: {Count}", filteredDoctors.Count);
                }
                else
                {
                    // ✅ Fallback: اگر DoctorServiceCategory موجود نیست، از منطق ServiceGroup ↔ Specialty استفاده کن
                    // برای خدمات تخصصی (GroupCode = 1-7)، فقط پزشکان متخصص را برگردان
                    // برای خدمات عمومی، همه پزشکان را برگردان
                    if (service.GroupCode.HasValue && service.GroupCode.Value > 1)
                    {
                        // خدمت تخصصی: فقط پزشکان متخصص (نه عمومی)
                        filteredDoctors = doctorsBase
                            .Where(d => !string.IsNullOrEmpty(d.Specialization) && 
                                       !d.Specialization.Contains("عمومی") &&
                                       !d.Specialization.Contains("General"))
                            .OrderBy(d => d.LastName)
                            .ThenBy(d => d.FirstName)
                            .Select(d => new DoctorDto
                            {
                                DoctorId = d.DoctorId,
                                FirstName = d.FirstName,
                                LastName = d.LastName,
                                DoctorCode = d.DoctorCode,
                                Specialization = d.Specialization,
                                IsActive = d.IsActive
                            })
                            .ToList();
                        
                        _logger.Information("✅ FACADE: پزشکان فیلتر شده (Fallback - تخصصی) - GroupCode: {GroupCode}, Count: {Count}", 
                            service.GroupCode.Value, filteredDoctors.Count);
                    }
                    else
                    {
                        // خدمت عمومی: همه پزشکان
                        filteredDoctors = doctorsBase
                            .OrderBy(d => d.LastName)
                            .ThenBy(d => d.FirstName)
                            .Select(d => new DoctorDto
                            {
                                DoctorId = d.DoctorId,
                                FirstName = d.FirstName,
                                LastName = d.LastName,
                                DoctorCode = d.DoctorCode,
                                Specialization = d.Specialization,
                                IsActive = d.IsActive
                            })
                            .ToList();
                        
                        _logger.Information("✅ FACADE: پزشکان فیلتر شده (Fallback - عمومی) - Count: {Count}", filteredDoctors.Count);
                    }
                }

                if (filteredDoctors.Count > 0)
                {
                    _logger.Information("✅ FACADE: اولین پزشک - DoctorId: {DoctorId}, Name: {FirstName} {LastName}", 
                        filteredDoctors[0].DoctorId, filteredDoctors[0].FirstName, filteredDoctors[0].LastName);
                }
                else
                {
                    _logger.Warning("⚠️ FACADE: هیچ پزشک مجازی برای این خدمت یافت نشد");
                }

                return ServiceResult<List<DoctorDto>>.Successful(filteredDoctors, 
                    filteredDoctors.Count > 0 ? $"تعداد {filteredDoctors.Count} پزشک مجاز یافت شد" : "هیچ پزشک مجازی برای این خدمت یافت نشد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در دریافت پزشکان مجاز برای خدمت - DeptId: {DeptId}, ServiceId: {ServiceId}, ClinicId: {ClinicId}, Exception: {ExceptionType}, Message: {Message}", 
                    departmentId, serviceId, clinicId, ex.GetType().Name, ex.Message);
                return ServiceResult<List<DoctorDto>>.Failed($"خطا در دریافت پزشکان مجاز برای خدمت: {ex.Message}");
            }
        }

        /// <summary>
        /// جستجو یا ایجاد بیمار
        /// </summary>
        public async Task<ServiceResult<PatientDto>> FindOrCreatePatientAsync(string nationalCode, PatientCreateDto dtoIfNotExists)
        {
            try
            {
                _logger.Information("🏥 FACADE: جستجو یا ایجاد بیمار - NationalCode: {NationalCode}", nationalCode);

                // 1. جستجوی بیمار
                var findResult = await _patientService.FindByNationalCodeAsync(nationalCode);
                if (findResult.Success && findResult.Data != null)
                {
                    return ServiceResult<PatientDto>.Successful(new PatientDto
                    {
                        PatientId = findResult.Data.PatientId,
                        NationalCode = findResult.Data.NationalCode,
                        FirstName = findResult.Data.FirstName,
                        LastName = findResult.Data.LastName,
                        PhoneNumber = findResult.Data.PhoneNumber,
                        Email = findResult.Data.Email
                    });
                }

                // 2. ایجاد بیمار جدید (اگر dtoIfNotExists ارائه شده)
                if (dtoIfNotExists != null)
                {
                    // تبدیل PatientCreateDto به PatientCreateEditViewModel
                    var createViewModel = new PatientCreateEditViewModel
                    {
                        NationalCode = dtoIfNotExists.NationalCode,
                        FirstName = dtoIfNotExists.FirstName,
                        LastName = dtoIfNotExists.LastName,
                        FatherName = dtoIfNotExists.FatherName,
                        PhoneNumber = dtoIfNotExists.PhoneNumber,
                        Email = dtoIfNotExists.Email,
                        BirthDate = dtoIfNotExists.BirthDate,
                        Gender = Enum.TryParse<Gender>(dtoIfNotExists.Gender, out var gender) ? gender : Gender.Unknown,
                        Address = dtoIfNotExists.Address
                    };

                    var createResult = await _patientService.CreatePatientAsync(createViewModel);
                    if (createResult.Success)
                    {
                        // پس از ایجاد موفقیت‌آمیز، بیمار را دوباره پیدا کنیم تا PatientId واقعی را بگیریم
                        var findCreatedResult = await _patientService.FindByNationalCodeAsync(nationalCode);
                        if (findCreatedResult.Success && findCreatedResult.Data != null)
                        {
                            _logger.Information("✅ FACADE: بیمار جدید ایجاد شد - PatientId: {PatientId}", findCreatedResult.Data.PatientId);
                            return ServiceResult<PatientDto>.Successful(new PatientDto
                            {
                                PatientId = findCreatedResult.Data.PatientId, // ✅ PatientId واقعی
                                NationalCode = findCreatedResult.Data.NationalCode ?? dtoIfNotExists.NationalCode,
                                FirstName = findCreatedResult.Data.FirstName ?? dtoIfNotExists.FirstName,
                                LastName = findCreatedResult.Data.LastName ?? dtoIfNotExists.LastName,
                                PhoneNumber = findCreatedResult.Data.PhoneNumber ?? dtoIfNotExists.PhoneNumber,
                                Email = findCreatedResult.Data.Email ?? dtoIfNotExists.Email
                            });
                        }
                        else
                        {
                            _logger.Warning("⚠️ FACADE: بیمار ایجاد شد اما یافت نشد - NationalCode: {NationalCode}", nationalCode);
                            // Fallback: از اطلاعات ورودی استفاده کنیم (PatientId = 0)
                            return ServiceResult<PatientDto>.Successful(new PatientDto
                            {
                                PatientId = 0, // ⚠️ باید بعداً از frontend دوباره lookup شود
                                NationalCode = dtoIfNotExists.NationalCode,
                                FirstName = dtoIfNotExists.FirstName,
                                LastName = dtoIfNotExists.LastName,
                                PhoneNumber = dtoIfNotExists.PhoneNumber,
                                Email = dtoIfNotExists.Email
                            });
                        }
                    }
                }

                return ServiceResult<PatientDto>.Failed("بیمار یافت نشد و اطلاعات ایجاد ارائه نشده");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در جستجو یا ایجاد بیمار");
                return ServiceResult<PatientDto>.Failed("خطا در جستجو یا ایجاد بیمار");
            }
        }

        /// <summary>
        /// بارگذاری بیمه‌های بیمار
        /// </summary>
        public async Task<ServiceResult<InsuranceBundleDto>> LoadPatientInsurancesAsync(int patientId)
        {
            try
            {
                _logger.Information("🏥 FACADE: بارگذاری بیمه‌های بیمار - PatientId: {PatientId}", patientId);

                var result = await _patientInsuranceService.GetPatientInsurancesAsync(patientId, null, 1, 100);
                if (result.Success)
                {
                    var bundle = new InsuranceBundleDto
                    {
                        PatientId = patientId,
                        BaseInsurances = result.Data.Where(i => i.InsuranceType == "اصلی").Select(i => new InsuranceDto
                        {
                            InsuranceId = i.InsurancePlanId,
                            InsuranceName = i.InsurancePlanName,
                            InsuranceType = i.InsuranceType,
                            CoveragePercentage = i.CoveragePercent,
                            IsActive = i.IsActive
                        }).ToList(),
                        SupplementaryInsurances = result.Data.Where(i => i.InsuranceType == "تکمیلی").Select(i => new InsuranceDto
                        {
                            InsuranceId = i.InsurancePlanId,
                            InsuranceName = i.InsurancePlanName,
                            InsuranceType = i.InsuranceType,
                            CoveragePercentage = i.CoveragePercent,
                            IsActive = i.IsActive
                        }).ToList()
                    };

                    return ServiceResult<InsuranceBundleDto>.Successful(bundle);
                }

                return ServiceResult<InsuranceBundleDto>.Failed(result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در بارگذاری بیمه‌های بیمار");
                return ServiceResult<InsuranceBundleDto>.Failed("خطا در بارگذاری بیمه‌های بیمار");
            }
        }

        /// <summary>
        /// تنظیم بیمه‌های بیمار (برای Quick Create)
        /// </summary>
        public async Task SetPatientInsurancesAsync(int patientId, int? basePlanId, int? suppPlanId)
        {
            try
            {
                _logger.Information("🏥 FACADE: تنظیم بیمه‌های بیمار - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
                    patientId, basePlanId, suppPlanId);

                // یافتن PatientInsurance فعال و Primary این بیمار
                var patientInsurance = await _context.PatientInsurances
                    .FirstOrDefaultAsync(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive && !pi.IsDeleted);

                if (patientInsurance == null)
                {
                    // ایجاد PatientInsurance جدید اگر وجود ندارد
                    if (basePlanId.HasValue)
                    {
                        var basePlan = await _context.InsurancePlans
                            .FirstOrDefaultAsync(p => p.InsurancePlanId == basePlanId.Value && !p.IsDeleted && p.IsActive);

                        if (basePlan != null && basePlan.InsuranceType == Models.Entities.Insurance.InsuranceType.Primary)
                        {
                            patientInsurance = new PatientInsurance
                            {
                                PatientId = patientId,
                                InsurancePlanId = basePlanId.Value,
                                InsuranceProviderId = basePlan.InsuranceProviderId,
                                IsPrimary = true,
                                IsActive = true,
                                StartDate = DateTime.Now,
                                CreatedAt = DateTime.Now,
                                CreatedByUserId = _currentUserService?.UserId ?? "system"
                            };

                            // اگر بیمه تکمیلی هم مشخص شده، اضافه کن
                            if (suppPlanId.HasValue)
                            {
                                var suppPlan = await _context.InsurancePlans
                                    .FirstOrDefaultAsync(p => p.InsurancePlanId == suppPlanId.Value && !p.IsDeleted && p.IsActive);

                                if (suppPlan != null && suppPlan.InsuranceType == Models.Entities.Insurance.InsuranceType.Supplementary)
                                {
                                    patientInsurance.SupplementaryInsurancePlanId = suppPlanId.Value;
                                    patientInsurance.SupplementaryInsuranceProviderId = suppPlan.InsuranceProviderId;
                                }
                            }

                            _context.PatientInsurances.Add(patientInsurance);
                            await _context.SaveChangesAsync();

                            _logger.Information("✅ FACADE: PatientInsurance جدید ایجاد شد - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
                                patientId, basePlanId, suppPlanId);
                            return;
                        }
                    }
                }
                else
                {
                    // به‌روزرسانی PatientInsurance موجود
                    bool hasChanges = false;

                    // به‌روزرسانی بیمه پایه
                    if (basePlanId.HasValue)
                    {
                        var basePlan = await _context.InsurancePlans
                            .FirstOrDefaultAsync(p => p.InsurancePlanId == basePlanId.Value && !p.IsDeleted && p.IsActive);

                        if (basePlan != null && basePlan.InsuranceType == Models.Entities.Insurance.InsuranceType.Primary)
                        {
                            if (patientInsurance.InsurancePlanId != basePlanId.Value ||
                                patientInsurance.InsuranceProviderId != basePlan.InsuranceProviderId)
                            {
                                patientInsurance.InsurancePlanId = basePlanId.Value;
                                patientInsurance.InsuranceProviderId = basePlan.InsuranceProviderId;
                                hasChanges = true;
                            }
                        }
                    }

                    // به‌روزرسانی بیمه تکمیلی
                    if (suppPlanId.HasValue)
                    {
                        var suppPlan = await _context.InsurancePlans
                            .FirstOrDefaultAsync(p => p.InsurancePlanId == suppPlanId.Value && !p.IsDeleted && p.IsActive);

                        if (suppPlan != null && suppPlan.InsuranceType == Models.Entities.Insurance.InsuranceType.Supplementary)
                        {
                            if (patientInsurance.SupplementaryInsurancePlanId != suppPlanId.Value ||
                                patientInsurance.SupplementaryInsuranceProviderId != suppPlan.InsuranceProviderId)
                            {
                                patientInsurance.SupplementaryInsurancePlanId = suppPlanId.Value;
                                patientInsurance.SupplementaryInsuranceProviderId = suppPlan.InsuranceProviderId;
                                hasChanges = true;
                            }
                        }
                    }
                    else
                    {
                        // اگر suppPlanId null باشد، بیمه تکمیلی را حذف کن
                        if (patientInsurance.SupplementaryInsurancePlanId.HasValue)
                        {
                            patientInsurance.SupplementaryInsurancePlanId = null;
                            patientInsurance.SupplementaryInsuranceProviderId = null;
                            hasChanges = true;
                        }
                    }

                    if (hasChanges)
                    {
                        patientInsurance.UpdatedAt = DateTime.Now;
                        patientInsurance.UpdatedByUserId = _currentUserService?.UserId ?? "system";
                        await _context.SaveChangesAsync();

                        _logger.Information("✅ FACADE: PatientInsurance به‌روزرسانی شد - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
                            patientId, basePlanId, suppPlanId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در تنظیم بیمه‌های بیمار - PatientId: {PatientId}", patientId);
                throw; // Re-throw برای Controller
            }
        }

        /// <summary>
        /// دریافت بیمه‌های انتسابی فعال بیمار (برای فرم پذیرش)
        /// </summary>
        public async Task<Controllers.Api.InsuranceSelectionDto> GetAssignedInsurancesForPatient(int patientId)
        {
            try
            {
                _logger.Information("🏥 FACADE: دریافت بیمه‌های انتسابی بیمار - PatientId: {PatientId}", patientId);

                // دریافت آخرین بیمه فعال بیمار
                var activeInsurances = await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && pi.IsActive && !pi.IsDeleted)
                    .Include(pi => pi.InsurancePlan)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .Include(pi => pi.SupplementaryInsuranceProvider)
                    .Include(pi => pi.SupplementaryInsurancePlan)
                    .OrderByDescending(pi => pi.StartDate)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (activeInsurances == null)
                {
                    return new Controllers.Api.InsuranceSelectionDto
                    {
                        BaseInsuranceId = null,
                        BasePlanId = null,
                        SupplementaryInsuranceId = null,
                        SupplementaryPlanId = null,
                        SuggestedBasePlanId = null,
                        SuggestedSupplementaryPlanId = null
                    };
                }

                // استخراج بیمه پایه
                var baseInsuranceId = activeInsurances.InsurancePlan?.InsuranceProviderId;
                var basePlanId = activeInsurances.InsurancePlanId;

                // استخراج بیمه تکمیلی (اگر وجود داشته باشد)
                var suppInsuranceId = activeInsurances.SupplementaryInsuranceProviderId;
                var suppPlanId = activeInsurances.SupplementaryInsurancePlanId;

                // پیشنهاد پلن‌های پیش‌فرض
                var (suggestedBasePlan, suggestedSuppPlan) = await _insurancePlanSuggestionService.SuggestDefaultsAsync(baseInsuranceId, suppInsuranceId);

                return new Controllers.Api.InsuranceSelectionDto
                {
                    BaseInsuranceId = baseInsuranceId,
                    BasePlanId = basePlanId,
                    SupplementaryInsuranceId = suppInsuranceId,
                    SupplementaryPlanId = suppPlanId,
                    SuggestedBasePlanId = suggestedBasePlan,
                    SuggestedSupplementaryPlanId = suggestedSuppPlan
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در دریافت بیمه‌های انتسابی بیمار");
                return new Controllers.Api.InsuranceSelectionDto();
            }
        }

        #endregion

        #region Draft Update

        /// <summary>
        /// به‌روزرسانی پیش‌نویس پذیرش و بازمحاسبه مجموع‌ها
        /// </summary>
        public async Task<ServiceResult<ItemsAndTotalsDto>> UpdateDraftAsync(ClinicApp.Dtos.Reception.UpdateDraftRequest request)
        {
            try
            {
                var draft = await _context.Receptions
                    .Include(d => d.ReceptionItems)
                    .FirstOrDefaultAsync(d => d.ReceptionId == request.ReceptionId && d.Status == ReceptionStatus.Pending);

                if (draft == null)
                    return ServiceResult<ItemsAndTotalsDto>.Failed("پیش‌نویس یافت نشد");

                if (request.ClinicId.HasValue) draft.ClinicId = request.ClinicId.Value;
                
                // اگر DepartmentId تغییر کرده، Doctor-Department را اعتبارسنجی کن
                if (request.DepartmentId.HasValue)
                {
                    var oldDeptId = draft.DepartmentId;
                    draft.DepartmentId = request.DepartmentId.Value;
                    
                    // اگر Department تغییر کرد و DoctorId مشخص شده، اعتبارسنجی کن
                    var doctorIdToValidate = request.DoctorId.HasValue ? request.DoctorId.Value : draft.DoctorId;
                    if (request.DepartmentId.Value != oldDeptId && doctorIdToValidate > 0)
                    {
                        var doctorDept = await _context.DoctorDepartments
                            .AsNoTracking()
                            .Where(dd => dd.DoctorId == doctorIdToValidate && 
                                        dd.DepartmentId == request.DepartmentId.Value && 
                                        !dd.IsDeleted &&
                                        dd.IsActive &&
                                        (dd.EndDate == null || dd.EndDate > DateTime.Now))
                            .FirstOrDefaultAsync();

                        if (doctorDept == null)
                        {
                            _logger.Warning("⚠️ FACADE: پزشک انتخابی به دپارتمان جدید منتسب نیست - DoctorId: {DoctorId}, DepartmentId: {DepartmentId}", 
                                doctorIdToValidate, request.DepartmentId.Value);
                            return ServiceResult<ItemsAndTotalsDto>.Failed(
                                "پزشک انتخابی به دپارتمان انتخاب شده منتسب نیست.", 
                                "VALIDATION");
                        }
                    }
                }
                
                // اگر DoctorId تغییر کرده، Doctor-Department را اعتبارسنجی کن
                if (request.DoctorId.HasValue)
                {
                    var oldDoctorId = draft.DoctorId;
                    draft.DoctorId = request.DoctorId.Value;
                    
                    if (request.DoctorId.Value != oldDoctorId && draft.DepartmentId > 0)
                    {
                        var doctorDept = await _context.DoctorDepartments
                            .AsNoTracking()
                            .Where(dd => dd.DoctorId == request.DoctorId.Value && 
                                        dd.DepartmentId == draft.DepartmentId && 
                                        !dd.IsDeleted &&
                                        dd.IsActive &&
                                        (dd.EndDate == null || dd.EndDate > DateTime.Now))
                            .FirstOrDefaultAsync();

                        if (doctorDept == null)
                        {
                            _logger.Warning("⚠️ FACADE: پزشک جدید به دپارتمان انتخاب شده منتسب نیست - DoctorId: {DoctorId}, DepartmentId: {DepartmentId}", 
                                request.DoctorId.Value, draft.DepartmentId);
                            return ServiceResult<ItemsAndTotalsDto>.Failed(
                                "پزشک انتخابی به دپارتمان انتخاب شده منتسب نیست.", 
                                "VALIDATION");
                        }
                    }
                }
                
                if (request.PatientId.HasValue) draft.PatientId = request.PatientId.Value;
                if (request.BasePlanId.HasValue) draft.BasePlanId = request.BasePlanId.Value;
                if (request.SupplementaryPlanId.HasValue) draft.SupplementaryPlanId = request.SupplementaryPlanId.Value;

                draft.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return await RecalculateDraftAsync(draft);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در به‌روزرسانی پیش‌نویس");
                return ServiceResult<ItemsAndTotalsDto>.Failed("خطا در به‌روزرسانی پیش‌نویس");
            }
        }

        #endregion

        #region Items & Calculation

        /// <summary>
        /// دریافت خدمات دپارتمان
        /// </summary>
        public async Task<ServiceResult<ServicePickListDto>> GetServicesForDeptAsync(int deptId)
        {
            try
            {
                _logger.Information("🏥 FACADE: دریافت خدمات دپارتمان - DeptId: {DeptId}", deptId);

                // استفاده از DepartmentManagementService برای دریافت خدمات
                var result = await _departmentManagementService.GetDepartmentServicesAsync(deptId);
                if (result.Success)
                {
                    var pickList = new ServicePickListDto
                    {
                        DepartmentId = deptId,
                        Services = result.Data.Select(s => new ServicePickItemDto
                        {
                            ServiceId = s.ServiceId,
                            ServiceCode = s.ServiceCode,
                            ServiceName = s.ServiceName,
                            UnitPrice = s.Price,
                            IsActive = s.IsActive
                        }).ToList()
                    };

                    return ServiceResult<ServicePickListDto>.Successful(pickList);
                }

                return ServiceResult<ServicePickListDto>.Failed(result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در دریافت خدمات دپارتمان");
                return ServiceResult<ServicePickListDto>.Failed("خطا در دریافت خدمات دپارتمان");
            }
        }

        /// <summary>
        /// افزودن آیتم به پذیرش - سه محرک محاسبه
        /// </summary>
        public async Task<ServiceResult<AddItemResultDto>> AddItemAsync(int receptionId, int serviceId, int quantity, int year)
        {
            try
            {
                _logger.Information("🏥 FACADE: افزودن آیتم به پذیرش - ReceptionId: {ReceptionId}, ServiceId: {ServiceId}, Quantity: {Quantity}, Year: {Year}", 
                    receptionId, serviceId, quantity, year);

                // 1. محاسبه قیمت پایه خدمت (K & FactorSetting)
                var unitPrice = await _serviceCalculationEngine.CalculateUnitPriceIRRAsync(serviceId, year);
                var itemTotal = unitPrice * quantity;

                // 2. افزودن به پذیرش
                var addResult = await _receptionWorkflowService.AddItemAsync(receptionId, serviceId, quantity, unitPrice);
                if (!addResult.Success)
                {
                    return ServiceResult<AddItemResultDto>.Failed(addResult.Message);
                }

                // 3. محاسبه مجدد مجموع‌ها (بیمه پایه + تکمیلی)
                var totalsResult = await _receptionRepository.RecalculateTotalsAsync(receptionId);
                if (!totalsResult.Success)
                {
                    return ServiceResult<AddItemResultDto>.Failed(totalsResult.Message);
                }

                var result = new AddItemResultDto
                {
                    ReceptionId = receptionId,
                    ServiceId = serviceId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    ItemTotal = itemTotal,
                    ReceptionTotals = totalsResult.Data
                };

                _logger.Information("✅ FACADE: آیتم با موفقیت افزوده شد - ItemTotal: {ItemTotal}", itemTotal);

                return ServiceResult<AddItemResultDto>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در افزودن آیتم به پذیرش");
                return ServiceResult<AddItemResultDto>.Failed("خطا در افزودن آیتم به پذیرش");
            }
        }

        #endregion

        #region Insurances & Finalize

        /// <summary>
        /// تنظیم بیمه‌های پذیرش
        /// </summary>
        public async Task<ServiceResult<bool>> SetInsurancesAsync(int receptionId, int? basePlanId, int? suppPlanId)
        {
            try
            {
                _logger.Information("🏥 FACADE: تنظیم بیمه‌های پذیرش - ReceptionId: {ReceptionId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                    receptionId, basePlanId, suppPlanId);

                var result = await _receptionWorkflowService.SetInsurancesAsync(receptionId, basePlanId, suppPlanId);
                if (result.Success)
                {
                    // محاسبه مجدد مجموع‌ها
                    await _receptionRepository.RecalculateTotalsAsync(receptionId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در تنظیم بیمه‌های پذیرش");
                return ServiceResult<bool>.Failed("خطا در تنظیم بیمه‌های پذیرش");
            }
        }

        /// <summary>
        /// نهایی‌سازی با پرداخت POS
        /// </summary>
        public async Task<ServiceResult<FinalizeResultDto>> FinalizeWithPosAsync(int receptionId, PosPaymentDto pos)
        {
            try
            {
                _logger.Information("🏥 FACADE: نهایی‌سازی با پرداخت POS - ReceptionId: {ReceptionId}, Amount: {Amount}", 
                    receptionId, pos.Amount);

                // 1. اعتبارسنجی مبلغ
                var validationResult = await _posManagementService.ValidatePaymentAsync(receptionId, pos.Amount);
                if (!validationResult.Success)
                {
                    return ServiceResult<FinalizeResultDto>.Failed(validationResult.Message);
                }

                // 2. ثبت پرداخت POS
                var paymentResult = await _posManagementService.RegisterPosPaymentAsync(receptionId, pos);
                if (!paymentResult.Success)
                {
                    return ServiceResult<FinalizeResultDto>.Failed(paymentResult.Message);
                }

                // 3. نهایی‌سازی پذیرش
                var finalizeResult = await _receptionWorkflowService.FinalizeAsync(receptionId);
                if (!finalizeResult.Success)
                {
                    return ServiceResult<FinalizeResultDto>.Failed(finalizeResult.Message);
                }

                var result = new FinalizeResultDto
                {
                    ReceptionId = receptionId,
                    Status = "Finalized",
                    PaymentMethod = "POS",
                    PaymentAmount = pos.Amount,
                    FinalizedAt = DateTime.Now
                };

                _logger.Information("✅ FACADE: پذیرش با موفقیت نهایی شد - ReceptionId: {ReceptionId}", receptionId);

                return ServiceResult<FinalizeResultDto>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در نهایی‌سازی با پرداخت POS");
                return ServiceResult<FinalizeResultDto>.Failed("خطا در نهایی‌سازی با پرداخت POS");
            }
        }

        /// <summary>
        /// نهایی‌سازی با پرداخت نقدی
        /// </summary>
        public async Task<ServiceResult<FinalizeResultDto>> FinalizeWithCashAsync(int receptionId, CashPaymentDto cash)
        {
            try
            {
                _logger.Information("🏥 FACADE: نهایی‌سازی با پرداخت نقدی - ReceptionId: {ReceptionId}, Amount: {Amount}", 
                    receptionId, cash.Amount);

                // 1. دریافت جلسه نقدی باز
                var sessionResult = await _posManagementService.GetOpenCashSessionAsync(_currentUserService.UserId);
                if (!sessionResult.Success)
                {
                    return ServiceResult<FinalizeResultDto>.Failed("جلسه نقدی باز یافت نشد");
                }

                // 2. ثبت پرداخت نقدی
                var paymentResult = await _posManagementService.RegisterCashPaymentAsync(receptionId, cash, sessionResult.Data.CashSessionId);
                if (!paymentResult.Success)
                {
                    return ServiceResult<FinalizeResultDto>.Failed(paymentResult.Message);
                }

                // 3. نهایی‌سازی پذیرش
                var finalizeResult = await _receptionWorkflowService.FinalizeAsync(receptionId);
                if (!finalizeResult.Success)
                {
                    return ServiceResult<FinalizeResultDto>.Failed(finalizeResult.Message);
                }

                var result = new FinalizeResultDto
                {
                    ReceptionId = receptionId,
                    Status = "Finalized",
                    PaymentMethod = "Cash",
                    PaymentAmount = cash.Amount,
                    FinalizedAt = DateTime.Now
                };

                _logger.Information("✅ FACADE: پذیرش با موفقیت نهایی شد - ReceptionId: {ReceptionId}", receptionId);

                return ServiceResult<FinalizeResultDto>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در نهایی‌سازی با پرداخت نقدی");
                return ServiceResult<FinalizeResultDto>.Failed("خطا در نهایی‌سازی با پرداخت نقدی");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت خدمات مشترک
        /// </summary>
        private async Task<ServiceResult<List<ServiceDto>>> GetSharedServicesAsync()
        {
            try
            {
                // استفاده از DepartmentManagementService برای دریافت خدمات مشترک
                var result = await _departmentManagementService.GetSharedServicesAsync();
                if (result.Success)
                {
                    // Convert ClinicAdmin.ServiceDto to Reception.ServiceDto
                    var receptionServices = result.Data.Select(s => new ViewModels.Reception.ServiceDto
                    {
                        ServiceId = s.ServiceId,
                        ServiceCode = s.ServiceCode,
                        ServiceName = s.ServiceName,
                        Price = s.Price,
                        IsActive = s.IsActive
                    }).ToList();
                    
                    return ServiceResult<List<ViewModels.Reception.ServiceDto>>.Successful(receptionServices);
                }
                
                return ServiceResult<List<ViewModels.Reception.ServiceDto>>.Failed(result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در دریافت خدمات مشترک");
                return ServiceResult<List<ServiceDto>>.Failed("خطا در دریافت خدمات مشترک");
            }
        }

        #endregion

        #region Draft Management

        /// <summary>
        /// ایجاد پیش‌نویس پذیرش
        /// </summary>
        public async Task<ServiceResult<CreateDraftResponse>> CreateDraftAsync(CreateDraftRequest request)
        {
            try
            {
                _logger.Information("🏥 FACADE: ایجاد پیش‌نویس پذیرش");

                // Validate required fields for creating a draft using Reception entity (non-nullable columns)
                if (!request.PatientId.HasValue || !request.DoctorId.HasValue || !request.ClinicId.HasValue || !request.DepartmentId.HasValue)
                {
                    return ServiceResult<CreateDraftResponse>.Failed("اطلاعات بیمار/کلینیک/بخش/پزشک ناقص است. ابتدا فیلدهای لازم را تکمیل کنید.");
                }

                // 🔍 اعتبارسنجی: بررسی عضویت پزشک به دپارتمان
                // ✅ طبق نقشه پیوندی: بررسی ClinicId + StartDate/EndDate + IsActive
                var now = DateTime.Now;
                var doctorDept = await _context.DoctorDepartments
                    .AsNoTracking()
                    .Include(dd => dd.Department)
                    .Where(dd => dd.DoctorId == request.DoctorId.Value && 
                                dd.DepartmentId == request.DepartmentId.Value && 
                                dd.Department.ClinicId == request.ClinicId.Value && // ✅ همان کلینیک
                                !dd.IsDeleted &&
                                dd.IsActive &&
                                (dd.StartDate == null || dd.StartDate <= now) && // ✅ بازه تاریخ معتبر
                                (dd.EndDate == null || dd.EndDate > now))
                    .FirstOrDefaultAsync();

                if (doctorDept == null)
                {
                    _logger.Warning("⚠️ FACADE: پزشک انتخابی به دپارتمان انتخاب شده منتسب نیست - DoctorId: {DoctorId}, DepartmentId: {DepartmentId}", 
                        request.DoctorId.Value, request.DepartmentId.Value);
                    return ServiceResult<CreateDraftResponse>.Failed(
                        "پزشک انتخابی به دپارتمان انتخاب شده منتسب نیست.", 
                        "VALIDATION");
                }

                _logger.Information("✅ FACADE: اعتبارسنجی Doctor-Department موفق - DoctorId: {DoctorId}, DepartmentId: {DepartmentId}", 
                    request.DoctorId.Value, request.DepartmentId.Value);

                // دریافت سال مالی جاری
                var financialYear = _financialYearService.GetCurrentYear();

                var draft = new Models.Entities.Reception.Reception
                {
                    PatientId = request.PatientId.Value,
                    DoctorId = request.DoctorId.Value,
                    ClinicId = request.ClinicId.Value,
                    DepartmentId = request.DepartmentId.Value,
                    ReceptionDate = DateTime.Now,
                    Status = ReceptionStatus.Pending, // Draft status
                    Type = ReceptionType.Normal,
                    Priority = AppointmentPriority.Normal,
                    TotalAmount = 0,
                    PatientCoPay = 0,
                    InsurerShareAmount = 0,
                    FinancialYear = financialYear
                };
                
                _context.Receptions.Add(draft);
                await _context.SaveChangesAsync();

                return ServiceResult<CreateDraftResponse>.Successful(new CreateDraftResponse 
                { 
                    ReceptionId = draft.ReceptionId, 
                    Status = "Draft" 
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در ایجاد پیش‌نویس");
                return ServiceResult<CreateDraftResponse>.Failed("خطا در ایجاد پیش‌نویس پذیرش");
            }
        }

        /// <summary>
        /// افزودن آیتم به پیش‌نویس
        /// </summary>
        public async Task<ServiceResult<ItemsAndTotalsDto>> AddItemAsync(AddItemRequest request)
        {
            try
            {
                _logger.Information("🏥 FACADE: افزودن آیتم به پیش‌نویس");

                var draft = await _context.Receptions
                    .Include(d => d.ReceptionItems)
                    .FirstOrDefaultAsync(d => d.ReceptionId == request.ReceptionId && d.Status == ReceptionStatus.Pending);
                
                if (draft == null)
                    return ServiceResult<ItemsAndTotalsDto>.Failed("پیش‌نویس یافت نشد");

                // دریافت سال مالی از پیش‌نویس
                var year = draft.FinancialYear;

                // دریافت اطلاعات خدمت با قیود Eligibility
                var service = await _context.Services
                    .Where(s => s.ServiceId == request.ServiceId && s.IsActive && !s.IsDeleted)
                    .Select(s => new { s.ServiceId, s.ServiceCode, s.Title, s.AgeMin, s.AgeMax, s.GenderLimit, s.GroupCode, s.IsHashtagged })
                    .FirstOrDefaultAsync();

                if (service == null)
                    return ServiceResult<ItemsAndTotalsDto>.Failed("خدمت یافت نشد");

                // ✅ طبق نقشه پیوندی: اعتبارسنجی Service Eligibility (Age/Gender)
                // دریافت اطلاعات بیمار
                var patient = await _context.Patients
                    .Where(p => p.PatientId == draft.PatientId && !p.IsDeleted)
                    .Select(p => new { p.PatientId, p.BirthDate, p.Gender })
                    .FirstOrDefaultAsync();

                if (patient == null)
                    return ServiceResult<ItemsAndTotalsDto>.Failed("اطلاعات بیمار یافت نشد");

                // محاسبه سن بیمار
                int? patientAge = null;
                if (patient.BirthDate.HasValue)
                {
                    var today = DateTime.Today;
                    patientAge = today.Year - patient.BirthDate.Value.Year;
                    if (patient.BirthDate.Value.Date > today.AddYears(-patientAge.Value))
                        patientAge--;
                }

                // بررسی AgeMin
                if (service.AgeMin.HasValue && (!patientAge.HasValue || patientAge.Value < service.AgeMin.Value))
                {
                    _logger.Warning("⚠️ FACADE: حداقل سن برای این خدمت {AgeMin} سال است - ServiceId: {ServiceId}, PatientAge: {PatientAge}", 
                        service.AgeMin.Value, service.ServiceId, patientAge);
                    return ServiceResult<ItemsAndTotalsDto>.Failed(
                        $"حداقل سن برای این خدمت {service.AgeMin.Value} سال است.", 
                        "AGE_LIMIT");
                }

                // بررسی AgeMax
                if (service.AgeMax.HasValue && (!patientAge.HasValue || patientAge.Value > service.AgeMax.Value))
                {
                    _logger.Warning("⚠️ FACADE: حداکثر سن برای این خدمت {AgeMax} سال است - ServiceId: {ServiceId}, PatientAge: {PatientAge}", 
                        service.AgeMax.Value, service.ServiceId, patientAge);
                    return ServiceResult<ItemsAndTotalsDto>.Failed(
                        $"حداکثر سن برای این خدمت {service.AgeMax.Value} سال است.", 
                        "AGE_LIMIT");
                }

                // بررسی GenderLimit
                if (service.GenderLimit.HasValue && patient.Gender != service.GenderLimit.Value)
                {
                    _logger.Warning("⚠️ FACADE: این خدمت فقط برای {GenderLimit} مجاز است - ServiceId: {ServiceId}, PatientGender: {PatientGender}", 
                        service.GenderLimit.Value, service.ServiceId, patient.Gender);
                    return ServiceResult<ItemsAndTotalsDto>.Failed(
                        $"این خدمت فقط برای {service.GenderLimit.Value} مجاز است.", 
                        "GENDER_LIMIT");
                }

                var qty = request.Quantity <= 0 ? 1 : request.Quantity;

                // ✅ استفاده از PricingEngine برای محاسبه دقیق سهم‌های بیمه
                var quoteRequest = new Services.Pricing.Models.QuoteRequestDto
                {
                    ClinicId = draft.ClinicId,
                    DepartmentId = draft.DepartmentId,
                    DoctorId = draft.DoctorId,
                    ServiceId = service.ServiceId,
                    FinancialYearId = year,
                    Primary = draft.BasePlanId.HasValue
                        ? new Services.Pricing.Models.PartyInsuranceDto { InsurancePlanId = draft.BasePlanId.Value }
                        : null,
                    Supplementary = draft.SupplementaryPlanId.HasValue
                        ? new Services.Pricing.Models.PartyInsuranceDto { InsurancePlanId = draft.SupplementaryPlanId.Value }
                        : null
                };

                var quoteResult = await _pricingEngine.QuoteAsync(quoteRequest);
                
                if (quoteResult == null || quoteResult.ApprovedTariff <= 0)
                {
                    _logger.Warning("⚠️ FACADE: قیمت محاسبه شده نامعتبر است - ServiceId: {ServiceId}, Year: {Year}", 
                        service.ServiceId, year);
                    return ServiceResult<ItemsAndTotalsDto>.Failed("خطا در محاسبه قیمت خدمت");
                }

                var unit = (decimal)quoteResult.ApprovedTariff;
                var total = unit * qty;

                // محاسبه سهم‌ها بر اساس QuoteResult
                var itemBasePay = (long)Math.Round((decimal)quoteResult.Primary.Pays * qty, 0, MidpointRounding.AwayFromZero);
                var itemSuppPay = (long)Math.Round((decimal)quoteResult.Supplementary.Pays * qty, 0, MidpointRounding.AwayFromZero);
                var itemPatientShare = total - itemBasePay - itemSuppPay;
                
                if (itemPatientShare < 0)
                    itemPatientShare = 0;

                // برای Snapshot
                var itemBasePercent = quoteResult.Primary.CoveragePercent;
                var itemSuppPercent = quoteResult.Supplementary.CoveragePercent;

                // ✅ طبق نقشه پیوندی: ایجاد SnapshotJson (Immutable snapshot)
                // دریافت ServiceComponents و FactorSetting برای Snapshot
                var serviceComponents = await _context.ServiceComponents
                    .Where(sc => sc.ServiceId == service.ServiceId && sc.IsActive && !sc.IsDeleted)
                    .Select(sc => new { sc.ComponentType, sc.Coefficient })
                    .ToListAsync();

                var techComponent = serviceComponents.FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical);
                var profComponent = serviceComponents.FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional);

                var factors = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(ServiceComponentType.Technical, service.IsHashtagged, year);
                var profFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(ServiceComponentType.Professional, service.IsHashtagged, year);

                var coefTech = techComponent?.Coefficient ?? 0m;
                var coefProf = profComponent?.Coefficient ?? 0m;
                var kTech = factors?.Value ?? 0m;
                var kProf = profFactor?.Value ?? 0m;

                // محاسبه TechAmount و ProfAmount
                var techAmount = coefTech * kTech;
                var profAmount = coefProf * kProf;
                var baseKaPriceIRR = techAmount + profAmount; // یا unit اگر از Price استفاده می‌شود

                // ایجاد Snapshot
                var snapshot = new
                {
                    ServiceId = service.ServiceId,
                    ServiceCode = service.ServiceCode,
                    ServiceName = service.Title,
                    Quantity = qty,
                    UnitPrice = unit,
                    KTech = kTech,
                    KProf = kProf,
                    CoefTech = coefTech,
                    CoefProf = coefProf,
                    BaseKaPriceIRR = unit, // قیمت نهایی محاسبه شده
                    TechAmount = techAmount,
                    ProfAmount = profAmount,
                    GrossAmount = total,
                    BaseInsuranceCoverage = itemBasePercent,
                    SupplementaryCoverage = itemSuppPercent,
                    PatientShare = (decimal)itemPatientShare,
                    InsurerShare = (decimal)(itemBasePay + itemSuppPay),
                    PrimaryPays = itemBasePay,
                    SupplementaryPays = itemSuppPay,
                    RoundingMode = "AwayFromZero",
                    RoundingDelta = 0,
                    FactorSettingId = factors?.FactorSettingId,
                    FinancialYear = year,
                    BasePlanId = draft.BasePlanId,
                    SupplementaryPlanId = draft.SupplementaryPlanId,
                    CalculatedAt = DateTime.Now,
                    GroupCode = service.GroupCode,
                    IsHashtagged = service.IsHashtagged
                };

                var item = new Models.Entities.Reception.ReceptionItem
                {
                    ReceptionId = draft.ReceptionId,
                    ServiceId = service.ServiceId,
                    Quantity = qty,
                    UnitPrice = unit,
                    PatientShareAmount = (decimal)itemPatientShare,
                    InsurerShareAmount = (decimal)(itemBasePay + itemSuppPay),
                    SnapshotJson = Newtonsoft.Json.JsonConvert.SerializeObject(snapshot)
                };
                
                _context.ReceptionItems.Add(item);
                await _context.SaveChangesAsync();

                // بازمحاسبه
                await _context.Entry(draft).Collection(x => x.ReceptionItems).LoadAsync();
                return await RecalculateDraftAsync(draft);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در افزودن آیتم");
                return ServiceResult<ItemsAndTotalsDto>.Failed("خطا در افزودن آیتم");
            }
        }

        /// <summary>
        /// حذف آیتم از پیش‌نویس
        /// </summary>
        public async Task<ServiceResult<ItemsAndTotalsDto>> RemoveItemAsync(RemoveItemRequest request)
        {
            try
            {
                _logger.Information("🏥 FACADE: حذف آیتم از پیش‌نویس");

                var item = await _context.ReceptionItems
                    .FirstOrDefaultAsync(i => i.ReceptionId == request.ReceptionId && i.ServiceId == request.ServiceId);
                
                if (item == null)
                    return ServiceResult<ItemsAndTotalsDto>.Successful(new ItemsAndTotalsDto { Totals = new TotalsDto() });

                _context.ReceptionItems.Remove(item);
                await _context.SaveChangesAsync();

                var draft = await _context.Receptions
                    .Include(d => d.ReceptionItems)
                    .FirstAsync(x => x.ReceptionId == request.ReceptionId);
                
                return await RecalculateDraftAsync(draft);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در حذف آیتم");
                return ServiceResult<ItemsAndTotalsDto>.Failed("خطا در حذف آیتم");
            }
        }

        /// <summary>
        /// تنظیم بیمه‌های پیش‌نویس
        /// </summary>
        public async Task<ServiceResult<ItemsAndTotalsDto>> SetInsurancesAsync(SetInsurancesRequest request)
        {
            try
            {
                _logger.Information("🏥 FACADE: تنظیم بیمه‌های پیش‌نویس - ReceptionId: {ReceptionId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
                    request.ReceptionId, request.BasePlanId, request.SupplementaryPlanId);

                var draft = await _context.Receptions
                    .Include(d => d.ReceptionItems)
                    .FirstOrDefaultAsync(d => d.ReceptionId == request.ReceptionId && d.Status == ReceptionStatus.Pending);
                
                if (draft == null)
                    return ServiceResult<ItemsAndTotalsDto>.Failed("پیش‌نویس یافت نشد");

                // اعتبارسنجی پلن بیمه پایه (در صورت وجود) - ذخیره برای استفاده بعدی
                Models.Entities.Insurance.InsurancePlan basePlan = null;
                if (request.BasePlanId.HasValue)
                {
                    basePlan = await _context.InsurancePlans
                        .FirstOrDefaultAsync(p => p.InsurancePlanId == request.BasePlanId.Value && !p.IsDeleted && p.IsActive);
                    
                    if (basePlan == null)
                        return ServiceResult<ItemsAndTotalsDto>.Failed("پلن بیمه پایه یافت نشد یا غیرفعال است.");
                    
                    if (basePlan.InsuranceType != Models.Entities.Insurance.InsuranceType.Primary)
                        return ServiceResult<ItemsAndTotalsDto>.Failed("پلن انتخاب شده بیمه پایه نیست.");
                }

                // اعتبارسنجی پلن بیمه تکمیلی (در صورت وجود) - ذخیره برای استفاده بعدی
                Models.Entities.Insurance.InsurancePlan suppPlan = null;
                if (request.SupplementaryPlanId.HasValue)
                {
                    suppPlan = await _context.InsurancePlans
                        .FirstOrDefaultAsync(p => p.InsurancePlanId == request.SupplementaryPlanId.Value && !p.IsDeleted && p.IsActive);
                    
                    if (suppPlan == null)
                        return ServiceResult<ItemsAndTotalsDto>.Failed("پلن بیمه تکمیلی یافت نشد یا غیرفعال است.");
                    
                    if (suppPlan.InsuranceType != Models.Entities.Insurance.InsuranceType.Supplementary)
                        return ServiceResult<ItemsAndTotalsDto>.Failed("پلن انتخاب شده بیمه تکمیلی نیست.");
                }

                // اعمال تغییرات روی Reception
                draft.BasePlanId = request.BasePlanId;
                draft.SupplementaryPlanId = request.SupplementaryPlanId;
                draft.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();

                // 🔥 به‌روزرسانی PatientInsurances (بیمه‌های واقعی بیمار)
                var patientId = draft.PatientId;
                var userId = _currentUserService?.UserId ?? "system";

                // یافتن PatientInsurance فعال و Primary این بیمار (که بیمه پایه و تکمیلی در همان رکورد است)
                var patientInsurance = await _context.PatientInsurances
                    .FirstOrDefaultAsync(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive && !pi.IsDeleted);
                
                if (patientInsurance != null)
                {
                    bool hasChanges = false;

                    // به‌روزرسانی بیمه پایه در PatientInsurances (از basePlan قبلاً query شده استفاده می‌کنیم)
                    if (request.BasePlanId.HasValue && basePlan != null)
                    {
                        if (patientInsurance.InsurancePlanId != request.BasePlanId.Value || 
                            patientInsurance.InsuranceProviderId != basePlan.InsuranceProviderId)
                        {
                            // به‌روزرسانی InsurancePlanId و InsuranceProviderId
                            patientInsurance.InsurancePlanId = request.BasePlanId.Value;
                            patientInsurance.InsuranceProviderId = basePlan.InsuranceProviderId;
                            hasChanges = true;
                            
                            _logger.Information("🔄 FACADE: به‌روزرسانی بیمه پایه در PatientInsurances - PatientId: {PatientId}, PlanId: {PlanId}, ProviderId: {ProviderId}",
                                patientId, request.BasePlanId.Value, basePlan.InsuranceProviderId);
                        }
                    }

                    // به‌روزرسانی بیمه تکمیلی در PatientInsurances (از suppPlan قبلاً query شده استفاده می‌کنیم)
                    if (request.SupplementaryPlanId.HasValue && suppPlan != null)
                    {
                        if (patientInsurance.SupplementaryInsurancePlanId != request.SupplementaryPlanId.Value || 
                            patientInsurance.SupplementaryInsuranceProviderId != suppPlan.InsuranceProviderId)
                        {
                            // به‌روزرسانی SupplementaryInsurancePlanId و SupplementaryInsuranceProviderId
                            patientInsurance.SupplementaryInsurancePlanId = request.SupplementaryPlanId.Value;
                            patientInsurance.SupplementaryInsuranceProviderId = suppPlan.InsuranceProviderId;
                            hasChanges = true;
                            
                            _logger.Information("🔄 FACADE: به‌روزرسانی بیمه تکمیلی در PatientInsurances - PatientId: {PatientId}, SuppPlanId: {SuppPlanId}, SuppProviderId: {SuppProviderId}",
                                patientId, request.SupplementaryPlanId.Value, suppPlan.InsuranceProviderId);
                        }
                    }
                    else
                    {
                        // اگر SupplementaryPlanId null باشد، بیمه تکمیلی را حذف می‌کنیم
                        if (patientInsurance.SupplementaryInsurancePlanId.HasValue)
                        {
                            patientInsurance.SupplementaryInsurancePlanId = null;
                            patientInsurance.SupplementaryInsuranceProviderId = null;
                            hasChanges = true;
                            
                            _logger.Information("🔄 FACADE: حذف بیمه تکمیلی از PatientInsurances - PatientId: {PatientId}", patientId);
                        }
                    }

                    // ذخیره تغییرات PatientInsurances (فقط در صورت تغییر)
                    if (hasChanges)
                    {
                        patientInsurance.UpdatedAt = DateTime.Now;
                        patientInsurance.UpdatedByUserId = userId;
                        await _context.SaveChangesAsync();
                        
                        _logger.Information("✅ FACADE: PatientInsurances با موفقیت به‌روزرسانی شد - PatientId: {PatientId}", patientId);
                    }
                    else
                    {
                        _logger.Information("ℹ️ FACADE: PatientInsurances تغییری نداشت - PatientId: {PatientId}", patientId);
                    }
                }
                else
                {
                    _logger.Warning("⚠️ FACADE: PatientInsurance پایه برای بیمار یافت نشد - PatientId: {PatientId}. فقط Reception به‌روزرسانی شد.", patientId);
                    // TODO: در آینده می‌توانیم PatientInsurance ایجاد کنیم، اما برای حالا فقط Reception را update می‌کنیم
                }

                _logger.Information("✅ FACADE: بیمه‌های پیش‌نویس و PatientInsurances با موفقیت تنظیم شد - ReceptionId: {ReceptionId}, PatientId: {PatientId}", 
                    request.ReceptionId, patientId);

                // 🔄 Reprice-on-change: بازمحاسبه تمام آیتم‌ها با بیمه‌های جدید با استفاده از PricingEngine
                if (draft.ReceptionItems != null && draft.ReceptionItems.Any())
                {
                    _logger.Information("🔄 FACADE: شروع بازمحاسبه آیتم‌ها با بیمه‌های جدید - ItemsCount: {Count}", 
                        draft.ReceptionItems.Count);
                    
                    try
                    {
                        // ✅ استفاده از PricingEngine.RepriceReceptionAsync برای محاسبه دقیق
                        await _pricingEngine.RepriceReceptionAsync(draft.ReceptionId);
                        _logger.Information("✅ FACADE: تمام آیتم‌ها با بیمه‌های جدید بازمحاسبه شدند");
                    }
                    catch (Exception repriceEx)
                    {
                        _logger.Error(repriceEx, "⚠️ FACADE: خطا در بازمحاسبه آیتم‌ها - ReceptionId: {ReceptionId}", draft.ReceptionId);
                        // ادامه می‌دهیم - اگر Reprice با خطا مواجه شد، آیتم‌ها با قیمت قبلی باقی می‌مانند
                    }
                }

                // Reload draft with updated items for RecalculateDraftAsync
                await _context.Entry(draft).Collection(x => x.ReceptionItems).LoadAsync();
                
                return await RecalculateDraftAsync(draft);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در تنظیم بیمه‌ها");
                return ServiceResult<ItemsAndTotalsDto>.Failed("خطا در تنظیم بیمه‌ها: " + ex.Message);
            }
        }

        /// <summary>
        /// ✅ گام 7 - Finalize Validation: اعتبارسنجی کامل Draft قبل از Finalize
        /// </summary>
        private async Task<ServiceResult<bool>> ValidateDraftForFinalizeAsync(Models.Entities.Reception.Reception draft)
        {
            try
            {
                // 1. بررسی وجود فیلدهای الزامی
                if (draft.PatientId <= 0)
                    return ServiceResult<bool>.Failed("اطلاعات بیمار ناقص است.", "VALIDATION");

                if (draft.ClinicId <= 0)
                    return ServiceResult<bool>.Failed("کلینیک انتخاب نشده است.", "VALIDATION");

                if (draft.DepartmentId <= 0)
                    return ServiceResult<bool>.Failed("دپارتمان انتخاب نشده است.", "VALIDATION");

                if (draft.DoctorId <= 0)
                    return ServiceResult<bool>.Failed("پزشک انتخاب نشده است.", "VALIDATION");

                // 2. بررسی وجود آیتم‌ها
                if (draft.ReceptionItems == null || !draft.ReceptionItems.Any(ri => !ri.IsDeleted))
                    return ServiceResult<bool>.Failed("هیچ خدمتی به پذیرش افزوده نشده است.", "VALIDATION");

                // 3. بررسی وجود بیمه پایه برای خدمات بیمه‌ای (در صورت نیاز)
                // TODO: در آینده می‌توان بررسی کرد که آیا خدمات نیاز به بیمه دارند یا نه
                // فعلاً این بررسی را انجام نمی‌دهیم چون برخی خدمات بدون بیمه هم ممکن است باشند

                _logger.Information("✅ FACADE: اعتبارسنجی Draft برای Finalize موفق - ReceptionId: {ReceptionId}, ItemsCount: {Count}", 
                    draft.ReceptionId, draft.ReceptionItems?.Count(ri => !ri.IsDeleted) ?? 0);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در اعتبارسنجی Draft برای Finalize - ReceptionId: {ReceptionId}", draft?.ReceptionId);
                return ServiceResult<bool>.Failed("خطا در اعتبارسنجی Draft: " + ex.Message);
            }
        }

        /// <summary>
        /// نهایی‌سازی با POS
        /// </summary>
        public async Task<ServiceResult<FinalizeResponse>> FinalizePosAsync(FinalizePosRequest request)
        {
            try
            {
                _logger.Information("🏥 FACADE: نهایی‌سازی با POS");

                // بررسی وجود پرداخت قبلی (Idempotency Check)
                if (!string.IsNullOrEmpty(request.IdempotencyKey))
                {
                    var exists = await _context.PaymentTransactions
                        .AnyAsync(p => p.IdempotencyKey == request.IdempotencyKey && !p.IsDeleted);
                    if (exists)
                    {
                        _logger.Warning("⚠️ FACADE: پرداخت تکراری شناسایی شد - IdempotencyKey: {Key}", 
                            request.IdempotencyKey);
                        return ServiceResult<FinalizeResponse>.Failed("پرداخت قبلاً انجام شده است");
                    }
                }

                var draft = await _context.Receptions
                    .Include(d => d.ReceptionItems)
                    .FirstOrDefaultAsync(d => d.ReceptionId == request.ReceptionId && d.Status == ReceptionStatus.Pending);
                
                if (draft == null)
                    return ServiceResult<FinalizeResponse>.Failed("پیش‌نویس یافت نشد");

                // ✅ گام 7 - Finalize Validation: اعتبارسنجی کامل Draft
                var validationResult = await ValidateDraftForFinalizeAsync(draft);
                if (!validationResult.Success)
                {
                    _logger.Warning("⚠️ FACADE: اعتبارسنجی Draft برای Finalize ناموفق - ReceptionId: {ReceptionId}, Message: {Message}", 
                        request.ReceptionId, validationResult.Message);
                    return ServiceResult<FinalizeResponse>.Failed(validationResult.Message, validationResult.Code);
                }

                // محاسبه مجموع‌ها
                var totals = await RecalculateDraftAsync(draft);
                if (totals.Data.Totals.Patient != request.AmountIRR)
                    return ServiceResult<FinalizeResponse>.Failed("مبلغ پرداخت با مجموع مطابقت ندارد");

                // ثبت پرداخت
                var payment = new Models.Entities.Payment.PaymentTransaction
                {
                    ReceptionId = request.ReceptionId,
                    Amount = request.AmountIRR,
                    Status = PaymentStatus.Success,
                    IdempotencyKey = request.IdempotencyKey,
                    Method = PaymentMethod.POS,
                    ReferenceCode = request.Pos?.RRN,
                    TransactionId = request.Pos?.TraceNo,
                    TerminalId = request.Pos?.TerminalId,
                    CardLast4 = request.Pos?.CardLast4
                };

                _context.PaymentTransactions.Add(payment);

                // نهایی‌سازی پیش‌نویس
                draft.Status = ReceptionStatus.Completed;
                draft.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // شماره رسید
                var receiptNo = $"R{DateTime.Now:yyyyMMddHHmmss}-{request.ReceptionId}";
                return ServiceResult<FinalizeResponse>.Successful(new FinalizeResponse
                {
                    Status = "Finalized",
                    Receipt = new ReceiptDto 
                    { 
                        No = receiptNo, 
                        PrintedUrl = $"/reception/print/{request.ReceptionId}" 
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در نهایی‌سازی POS");
                return ServiceResult<FinalizeResponse>.Failed("خطا در نهایی‌سازی پذیرش");
            }
        }

        /// <summary>
        /// نهایی‌سازی با نقدی
        /// </summary>
        public async Task<ServiceResult<FinalizeResponse>> FinalizeCashAsync(FinalizeCashRequest request)
        {
            try
            {
                _logger.Information("🏥 FACADE: نهایی‌سازی با نقدی");

                // بررسی وجود پرداخت قبلی (Idempotency Check)
                if (!string.IsNullOrEmpty(request.IdempotencyKey))
                {
                    var exists = await _context.PaymentTransactions
                        .AnyAsync(p => p.IdempotencyKey == request.IdempotencyKey && !p.IsDeleted);
                    if (exists)
                    {
                        _logger.Warning("⚠️ FACADE: پرداخت تکراری شناسایی شد - IdempotencyKey: {Key}", 
                            request.IdempotencyKey);
                        return ServiceResult<FinalizeResponse>.Failed("پرداخت قبلاً انجام شده است");
                    }
                }

                var draft = await _context.Receptions
                    .Include(d => d.ReceptionItems)
                    .FirstOrDefaultAsync(d => d.ReceptionId == request.ReceptionId && d.Status == ReceptionStatus.Pending);
                
                if (draft == null)
                    return ServiceResult<FinalizeResponse>.Failed("پیش‌نویس یافت نشد");

                // ✅ گام 7 - Finalize Validation: اعتبارسنجی کامل Draft
                var validationResult = await ValidateDraftForFinalizeAsync(draft);
                if (!validationResult.Success)
                {
                    _logger.Warning("⚠️ FACADE: اعتبارسنجی Draft برای Finalize ناموفق - ReceptionId: {ReceptionId}, Message: {Message}", 
                        request.ReceptionId, validationResult.Message);
                    return ServiceResult<FinalizeResponse>.Failed(validationResult.Message, validationResult.Code);
                }

                // محاسبه مجموع‌ها
                var totals = await RecalculateDraftAsync(draft);
                if (totals.Data.Totals.Patient != request.AmountIRR)
                    return ServiceResult<FinalizeResponse>.Failed("مبلغ پرداخت با مجموع مطابقت ندارد");

                // ثبت پرداخت
                var payment = new Models.Entities.Payment.PaymentTransaction
                {
                    ReceptionId = request.ReceptionId,
                    Amount = request.AmountIRR,
                    Status = PaymentStatus.Success,
                    IdempotencyKey = request.IdempotencyKey,
                    Method = PaymentMethod.Cash
                };

                _context.PaymentTransactions.Add(payment);

                // نهایی‌سازی پیش‌نویس
                draft.Status = ReceptionStatus.Completed;
                draft.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // شماره رسید
                var receiptNo = $"R{DateTime.Now:yyyyMMddHHmmss}-{request.ReceptionId}";
                return ServiceResult<FinalizeResponse>.Successful(new FinalizeResponse
                {
                    Status = "Finalized",
                    Receipt = new ReceiptDto 
                    { 
                        No = receiptNo, 
                        PrintedUrl = $"/reception/print/{request.ReceptionId}" 
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در نهایی‌سازی نقدی");
                return ServiceResult<FinalizeResponse>.Failed("خطا در نهایی‌سازی پذیرش");
            }
        }

        /// <summary>
        /// بازمحاسبه پیش‌نویس
        /// </summary>
        private async Task<ServiceResult<ItemsAndTotalsDto>> RecalculateDraftAsync(Models.Entities.Reception.Reception draft)
        {
            try
            {
                // Gross
                var gross = draft.ReceptionItems.Sum(i => i.UnitPrice * i.Quantity);

                // درصدها از پلن‌های بیمه
                var basePercent = 0m;
                var suppPercent = 0m;

                // دریافت اطلاعات بیمه پایه
                if (draft.BasePlanId.HasValue)
                {
                    var basePlan = await _context.InsurancePlans
                        .Where(p => p.InsurancePlanId == draft.BasePlanId.Value && !p.IsDeleted && p.IsActive)
                        .Select(p => new { p.CoveragePercent, p.Deductible })
                        .FirstOrDefaultAsync();
                    
                    if (basePlan != null)
                    {
                        basePercent = basePlan.CoveragePercent;
                    }
                }

                // دریافت اطلاعات بیمه تکمیلی
                if (draft.SupplementaryPlanId.HasValue)
                {
                    var suppPlan = await _context.InsurancePlans
                        .Where(p => p.InsurancePlanId == draft.SupplementaryPlanId.Value && !p.IsDeleted && p.IsActive)
                        .Select(p => new { p.CoveragePercent })
                        .FirstOrDefaultAsync();
                    
                    if (suppPlan != null)
                    {
                        suppPercent = suppPlan.CoveragePercent;
                    }
                }

                // محاسبه سهم بیمه پایه
                var basePay = gross * (basePercent / 100m);
                var patientAfterBase = gross - basePay;
                
                // محاسبه سهم بیمه تکمیلی (از مبلغ باقی‌مانده)
                var suppPay = patientAfterBase * (suppPercent / 100m);
                var patient = patientAfterBase - suppPay;

                // دریافت اطلاعات خدمات
                var serviceIds = draft.ReceptionItems.Select(i => i.ServiceId).ToList();
        var services = await _context.Services
            .Where(s => serviceIds.Contains(s.ServiceId))
            .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
            .ToListAsync();

                var items = draft.ReceptionItems.Select(it => 
                {
                    var service = services.FirstOrDefault(s => s.ServiceId == it.ServiceId);
                    return new ReceptionItemDto
                    {
                        ServiceId = it.ServiceId,
                        Code = service?.ServiceCode ?? "",
                        Name = service?.Title ?? "",
                        Qty = it.Quantity,
                        UnitPriceIRR = it.UnitPrice,
                        TotalIRR = it.UnitPrice * it.Quantity
                    };
                }).ToList();

                return ServiceResult<ItemsAndTotalsDto>.Successful(new ItemsAndTotalsDto
                {
                    Items = items,
                    Totals = new TotalsDto 
                    { 
                        Gross = gross, 
                        Base = basePay, 
                        Supplementary = suppPay, 
                        Patient = patient 
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در بازمحاسبه");
                return ServiceResult<ItemsAndTotalsDto>.Failed("خطا در بازمحاسبه");
            }
        }

        #endregion

        #region Coverage & Price Preview

        /// <summary>
        /// دریافت جزئیات پوشش بیمه (پایه + تکمیلی + مؤثر)
        /// </summary>
        public async Task<ServiceResult<Controllers.Api.InsuranceCoverageDto>> GetInsuranceCoverageAsync(int patientId, int? basePlanId, int? suppPlanId)
        {
            try
            {
                _logger.Information("🏥 FACADE: دریافت پوشش بیمه - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                    patientId, basePlanId, suppPlanId);

                var baseCoverage = new Controllers.Api.InsuranceCoverageSliceDto();
                var suppCoverage = new Controllers.Api.InsuranceCoverageSliceDto();
                
                // بارگذاری بیمه پایه
                if (basePlanId.HasValue)
                {
                    var basePlan = await _context.InsurancePlans
                        .Include(p => p.InsuranceProvider)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.InsurancePlanId == basePlanId.Value && !p.IsDeleted && p.IsActive);
                    
                    if (basePlan != null)
                    {
                        baseCoverage.PlanName = basePlan.Name;
                        baseCoverage.CoveragePercent = basePlan.CoveragePercent;
                        
                        // ✅ محاسبه FranchisePercent: اگر Deductible مبلغی است، باید به درصد تبدیل شود
                        // اما چون نمی‌دانیم BasePrice چیست، فعلاً Deductible را به صورت مبلغ نمایش می‌دهیم
                        // TODO: اگر FranchisePercent در InsurancePlan وجود دارد، از آن استفاده کن
                        baseCoverage.FranchisePercent = 0m; // فعلاً 0 - بعداً از PlanCoverage یا BusinessRule بخوان
                        baseCoverage.FranchisePercentStr = basePlan.Deductible > 0 ? 
                            basePlan.Deductible.ToString("N0") + " ریال" : "—";
                        
                        // ✅ سقف‌ها: فعلاً از InsuranceTariff یا BusinessRule بخوان (بعداً پیاده‌سازی می‌شود)
                        // TODO: از PlanCoverage بخوان: AnnualCap, DailyCap, VisitCap
                        baseCoverage.CeilingPerService = null;
                        baseCoverage.CeilingPerVisit = null;
                        baseCoverage.CeilingMonthly = null;
                        baseCoverage.RemainingCeiling = null;
                        
                        baseCoverage.CeilingPerServiceStr = "—";
                        baseCoverage.CeilingPerVisitStr = "—";
                        baseCoverage.CeilingMonthlyStr = "—";
                        baseCoverage.RemainingCeilingStr = "—";
                    }
                }
                else
                {
                    baseCoverage.PlanName = "—";
                }

                // بارگذاری بیمه تکمیلی
                if (suppPlanId.HasValue)
                {
                    var suppPlan = await _context.InsurancePlans
                        .Include(p => p.InsuranceProvider)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.InsurancePlanId == suppPlanId.Value && !p.IsDeleted && p.IsActive);
                    
                    if (suppPlan != null)
                    {
                        suppCoverage.PlanName = suppPlan.Name;
                        suppCoverage.CoveragePercent = suppPlan.CoveragePercent;
                        
                        // ✅ محاسبه FranchisePercent: مشابه بیمه پایه
                        suppCoverage.FranchisePercent = 0m; // فعلاً 0 - بعداً از PlanCoverage یا BusinessRule بخوان
                        suppCoverage.FranchisePercentStr = suppPlan.Deductible > 0 ? 
                            suppPlan.Deductible.ToString("N0") + " ریال" : "—";
                        
                        // ✅ سقف‌ها: مشابه بیمه پایه
                        suppCoverage.CeilingPerService = null;
                        suppCoverage.CeilingPerVisit = null;
                        suppCoverage.CeilingMonthly = null;
                        suppCoverage.RemainingCeiling = null;
                        
                        suppCoverage.CeilingPerServiceStr = "—";
                        suppCoverage.CeilingPerVisitStr = "—";
                        suppCoverage.CeilingMonthlyStr = "—";
                        suppCoverage.RemainingCeilingStr = "—";
                    }
                }
                else
                {
                    suppCoverage.PlanName = "—";
                }

                // محاسبه پوشش مؤثر
                decimal baseCov = (baseCoverage.CoveragePercent ?? 0m) / 100m;
                decimal suppCov = (suppCoverage.CoveragePercent ?? 0m) / 100m;
                decimal franchiseAdj = 0m; // TODO: فرانشیز را از Deductible محاسبه کن
                
                // قاعده ترکیب: ابتدا پایه، سپس تکمیلی روی سهم باقیمانده بیمار
                decimal effective = Math.Min(1m, baseCov + (1m - baseCov) * suppCov);
                decimal patientShare = Math.Max(0m, 1m - effective + franchiseAdj);
                
                var effectiveCoverage = new Controllers.Api.InsuranceCoverageEffectiveDto
                {
                    EffectiveCoveragePercent = Math.Round(effective * 100m, 2),
                    PatientSharePercent = Math.Round(patientShare * 100m, 2),
                    Notes = "قاعده ترکیب: ابتدا بیمه پایه، سپس بیمه تکمیلی روی سهم باقیمانده بیمار."
                };

                var result = new Controllers.Api.InsuranceCoverageDto
                {
                    Base = baseCoverage,
                    Supplementary = suppCoverage,
                    Effective = effectiveCoverage
                };

                _logger.Information("✅ FACADE: پوشش بیمه محاسبه شد - Effective: {Effective}%, PatientShare: {PatientShare}%", 
                    effectiveCoverage.EffectiveCoveragePercent, effectiveCoverage.PatientSharePercent);

                return ServiceResult<Controllers.Api.InsuranceCoverageDto>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در دریافت پوشش بیمه");
                return ServiceResult<Controllers.Api.InsuranceCoverageDto>.Failed("خطا در دریافت پوشش بیمه: " + ex.Message);
            }
        }

        /// <summary>
        /// پیش‌نمایش قیمت خدمت (بدون persist)
        /// </summary>
        public async Task<ServiceResult<Controllers.Api.PricePreviewResultDto>> PreviewItemPriceAsync(Controllers.Api.PricePreviewRequestDto request)
        {
            try
            {
                _logger.Information("🏥 FACADE: پیش‌نمایش قیمت - ServiceCode: {ServiceCode}, PatientId: {PatientId}", 
                    request?.ServiceCodeOrName, request?.PatientId);

                if (string.IsNullOrWhiteSpace(request?.ServiceCodeOrName))
                {
                    return ServiceResult<Controllers.Api.PricePreviewResultDto>.Failed("کد یا نام خدمت الزامی است.", "VALIDATION");
                }

                // 1) یافتن خدمت
                var service = await _context.Services
                    .Where(s => (s.ServiceCode == request.ServiceCodeOrName || 
                               s.Title.Contains(request.ServiceCodeOrName)) &&
                               s.IsActive && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    return ServiceResult<Controllers.Api.PricePreviewResultDto>.Failed("خدمت یافت نشد.", "NOT_FOUND");
                }

                // 2) محاسبه قیمت پایه
                var financialYear = _financialYearService.GetCurrentYear();
                var unitPrice = await _serviceCalculationEngine.CalculateUnitPriceIRRAsync(service.ServiceId, financialYear);
                
                if (unitPrice <= 0)
                {
                    _logger.Warning("⚠️ FACADE: قیمت محاسبه شده نامعتبر - ServiceId: {ServiceId}, Year: {Year}", 
                        service.ServiceId, financialYear);
                    return ServiceResult<Controllers.Api.PricePreviewResultDto>.Failed("خطا در محاسبه قیمت خدمت", "CALCULATION_ERROR");
                }

                // 3) دریافت پوشش مؤثر
                var coverage = await GetInsuranceCoverageAsync(request.PatientId ?? 0, request.BasePlanId, request.SupplementaryPlanId);
                decimal effPct = 0m;
                if (coverage.Success && coverage.Data != null)
                {
                    effPct = coverage.Data.Effective.EffectiveCoveragePercent;
                }

                // 4) محاسبه سهم بیمار
                decimal patientShare = Math.Round(unitPrice * (1m - effPct / 100m), 0);

                // 5) فرمت مبالغ
                var priceStr = unitPrice.ToString("N0") + " ریال";
                var patientShareStr = patientShare.ToString("N0") + " ریال";

                var result = new Controllers.Api.PricePreviewResultDto
                {
                    Price = unitPrice,
                    PatientShare = patientShare,
                    EffectiveCoveragePercent = effPct,
                    PriceStr = priceStr,
                    PatientShareStr = patientShareStr
                };

                _logger.Information("✅ FACADE: پیش‌نمایش قیمت محاسبه شد - Price: {Price}, PatientShare: {PatientShare}, EffectiveCoverage: {EffPct}%", 
                    unitPrice, patientShare, effPct);

                return ServiceResult<Controllers.Api.PricePreviewResultDto>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در پیش‌نمایش قیمت");
                return ServiceResult<Controllers.Api.PricePreviewResultDto>.Failed("خطا در پیش‌نمایش قیمت: " + ex.Message);
            }
        }

        #endregion
    }
}
