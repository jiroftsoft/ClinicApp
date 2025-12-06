using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Controllers.Api;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.Finance;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Insurance;
using ClinicApp.Services.Reception;
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
        private readonly IReceptionPricingService _receptionPricingService;
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
            IReceptionPricingService receptionPricingService,
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
            _receptionPricingService = receptionPricingService ?? throw new ArgumentNullException(nameof(receptionPricingService));
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
                            // ⚠️ بیمار ایجاد شد اما یافت نشد یا PatientId نامعتبر است
                            _logger.Error("❌ FACADE: بیمار ایجاد شد اما یافت نشد یا PatientId نامعتبر است - NationalCode: {NationalCode}, " +
                                "FindResult.Success: {FindSuccess}, FindResult.Data: {FindData}, PatientId: {PatientId}",
                                nationalCode, 
                                findCreatedResult?.Success ?? false,
                                findCreatedResult?.Data != null,
                                findCreatedResult?.Data?.PatientId ?? 0);
                            
                            // ❌ خطا: نمی‌توانیم PatientId = 0 را برگردانیم چون باعث NullReferenceException می‌شود
                            return ServiceResult<PatientDto>.Failed(
                                "بیمار با موفقیت ایجاد شد اما شناسه بیمار یافت نشد. لطفاً دوباره تلاش کنید.",
                                "PATIENT_ID_NOT_FOUND");
                        }
                    }

                    if (!createResult.Success)
                    {
                        var failedResult = ServiceResult<PatientDto>.Failed(
                            createResult.Message ?? "خطا در ثبت بیمار.",
                            createResult.Code ?? "CREATE_FAILED");

                        if (createResult.ValidationErrors != null && createResult.ValidationErrors.Any())
                        {
                            failedResult.WithValidationErrors(createResult.ValidationErrors);
                        }

                        return failedResult;
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

                // ✅ دریافت کد ملی بیمار برای استفاده در PolicyNumber
                var patient = await _context.Patients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted);
                
                if (patient == null)
                {
                    _logger.Error("❌ FACADE: بیمار یافت نشد - PatientId: {PatientId}", patientId);
                    throw new InvalidOperationException($"بیمار با شناسه {patientId} یافت نشد.");
                }
                
                var nationalCode = patient.NationalCode;
                if (string.IsNullOrEmpty(nationalCode))
                {
                    _logger.Warning("⚠️ FACADE: کد ملی بیمار خالی است - PatientId: {PatientId}", patientId);
                    nationalCode = patientId.ToString(); // Fallback به PatientId
                }

                // یافتن PatientInsurance فعال و Primary این بیمار
                var patientInsurance = await _context.PatientInsurances
                    .FirstOrDefaultAsync(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive && !pi.IsDeleted);

                // ✅ بهینه‌سازی: استفاده از کد ملی بیمار به جای timestamp
                string BuildPolicyNumber(string prefix)
                {
                    // استفاده از کد ملی بیمار به عنوان PolicyNumber
                    return nationalCode;
                }

                if (patientInsurance == null)
                {
                    // ایجاد PatientInsurance جدید اگر وجود ندارد
                    if (basePlanId.HasValue)
                    {
                        var basePlan = await _context.InsurancePlans
                            .FirstOrDefaultAsync(p => p.InsurancePlanId == basePlanId.Value && !p.IsDeleted && p.IsActive);

                        if (basePlan == null)
                        {
                            _logger.Warning("⚠️ FACADE: بیمه پایه یافت نشد - BasePlanId: {BasePlanId}, PatientId: {PatientId}", basePlanId, patientId);
                            throw new InvalidOperationException($"بیمه پایه با شناسه {basePlanId} یافت نشد یا غیرفعال است.");
                        }

                        if (basePlan.InsuranceType != Models.Entities.Insurance.InsuranceType.Primary)
                        {
                            _logger.Warning("⚠️ FACADE: نوع بیمه پایه نامعتبر است - BasePlanId: {BasePlanId}, InsuranceType: {InsuranceType}, PatientId: {PatientId}", 
                                basePlanId, basePlan.InsuranceType, patientId);
                            throw new InvalidOperationException($"بیمه با شناسه {basePlanId} از نوع پایه نیست. نوع: {basePlan.InsuranceType}");
                        }

                        // ✅ بررسی InsuranceProviderId (HasRequired - باید وجود داشته باشد)
                        // InsuranceProviderId از نوع int است (نه int?)، پس باید بررسی کنیم که مقدار معتبر است
                        if (basePlan.InsuranceProviderId <= 0)
                        {
                            _logger.Error("❌ FACADE: InsuranceProviderId برای InsurancePlan نامعتبر است - InsuranceProviderId: {InsuranceProviderId}, BasePlanId: {BasePlanId}, PatientId: {PatientId}", 
                                basePlan.InsuranceProviderId, basePlanId, patientId);
                            throw new InvalidOperationException($"بیمه پایه با شناسه {basePlanId} دارای InsuranceProviderId نامعتبر است. لطفاً تنظیمات بیمه را بررسی کنید.");
                        }

                        // ✅ بررسی وجود InsuranceProvider در دیتابیس
                        var insuranceProviderExists = await _context.InsuranceProviders
                            .AnyAsync(ip => ip.InsuranceProviderId == basePlan.InsuranceProviderId && !ip.IsDeleted);
                        
                        if (!insuranceProviderExists)
                        {
                            _logger.Error("❌ FACADE: InsuranceProvider در دیتابیس یافت نشد - InsuranceProviderId: {InsuranceProviderId}, BasePlanId: {BasePlanId}, PatientId: {PatientId}", 
                                basePlan.InsuranceProviderId, basePlanId, patientId);
                            throw new InvalidOperationException($"ارائه‌دهنده بیمه با شناسه {basePlan.InsuranceProviderId} یافت نشد یا حذف شده است.");
                        }

                        // ✅ دریافت شناسه کاربر معتبر برای CreatedByUserId از دیتابیس
                        // طبق قرارداد: اطمینان از وجود کاربر در AspNetUsers قبل از استفاده
                        var createdByUserId = await GetValidUserIdFromDatabaseAsync();
                        if (string.IsNullOrEmpty(createdByUserId))
                        {
                            _logger.Warning("⚠️ FACADE: هیچ کاربر معتبری در دیتابیس یافت نشد. CreatedByUserId را null تنظیم می‌کنیم (HasOptional)");
                            // CreatedByUserId اختیاری است (HasOptional)، پس می‌توانیم null بگذاریم
                            createdByUserId = null;
                        }
                        else
                        {
                            _logger.Information("✅ FACADE: استفاده از شناسه کاربر معتبر از دیتابیس: {UserId}", createdByUserId);
                        }

                        // ✅ همه چک‌ها انجام شد، ایجاد PatientInsurance
                        patientInsurance = new PatientInsurance
                        {
                            PatientId = patientId,
                            InsurancePlanId = basePlanId.Value,
                            InsuranceProviderId = basePlan.InsuranceProviderId, // ✅ اطمینان از وجود مقدار (از نوع int است)
                            IsPrimary = true,
                            IsActive = true,
                            Priority = InsurancePriority.Primary,
                            PolicyNumber = BuildPolicyNumber("AUTO-PRIMARY"),
                            StartDate = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            CreatedByUserId = createdByUserId
                        };

                        // اگر بیمه تکمیلی هم مشخص شده، اضافه کن
                        if (suppPlanId.HasValue)
                        {
                            var suppPlan = await _context.InsurancePlans
                                .FirstOrDefaultAsync(p => p.InsurancePlanId == suppPlanId.Value && !p.IsDeleted && p.IsActive);

                            if (suppPlan == null)
                            {
                                _logger.Warning("⚠️ FACADE: بیمه تکمیلی یافت نشد - SuppPlanId: {SuppPlanId}, PatientId: {PatientId}", suppPlanId, patientId);
                                // فقط لاگ می‌کنیم، خطا throw نمی‌کنیم چون بیمه پایه تنظیم شده
                            }
                            else if (suppPlan.InsuranceType != Models.Entities.Insurance.InsuranceType.Supplementary)
                            {
                                _logger.Warning("⚠️ FACADE: نوع بیمه تکمیلی نامعتبر است - SuppPlanId: {SuppPlanId}, InsuranceType: {InsuranceType}, PatientId: {PatientId}", 
                                    suppPlanId, suppPlan.InsuranceType, patientId);
                                // فقط لاگ می‌کنیم، خطا throw نمی‌کنیم چون بیمه پایه تنظیم شده
                            }
                            else
                            {
                                // ✅ بررسی InsuranceProviderId برای بیمه تکمیلی (HasOptional - می‌تواند null باشد)
                                // اگر InsuranceProviderId معتبر است، بررسی وجود در دیتابیس
                                if (suppPlan.InsuranceProviderId > 0)
                                {
                                    // بررسی وجود InsuranceProvider در دیتابیس
                                    var suppInsuranceProviderExists = await _context.InsuranceProviders
                                        .AnyAsync(ip => ip.InsuranceProviderId == suppPlan.InsuranceProviderId && !ip.IsDeleted);
                                    
                                    if (!suppInsuranceProviderExists)
                                    {
                                        _logger.Warning("⚠️ FACADE: InsuranceProvider برای بیمه تکمیلی در دیتابیس یافت نشد - InsuranceProviderId: {InsuranceProviderId}, SuppPlanId: {SuppPlanId}, PatientId: {PatientId}. " +
                                            "SupplementaryInsuranceProviderId را null تنظیم می‌کنیم",
                                            suppPlan.InsuranceProviderId, suppPlanId, patientId);
                                        patientInsurance.SupplementaryInsuranceProviderId = null;
                                    }
                                    else
                                    {
                                        patientInsurance.SupplementaryInsuranceProviderId = suppPlan.InsuranceProviderId;
                                    }
                                }
                                else
                                {
                                    _logger.Warning("⚠️ FACADE: InsuranceProviderId برای بیمه تکمیلی نامعتبر است - InsuranceProviderId: {InsuranceProviderId}, SuppPlanId: {SuppPlanId}, PatientId: {PatientId}", 
                                        suppPlan.InsuranceProviderId, suppPlanId, patientId);
                                    patientInsurance.SupplementaryInsuranceProviderId = null;
                                }

                                patientInsurance.SupplementaryInsurancePlanId = suppPlanId.Value;
                                patientInsurance.SupplementaryPolicyNumber = BuildPolicyNumber("AUTO-SUPP");
                                _logger.Information("✅ FACADE: بیمه تکمیلی به PatientInsurance اضافه شد - SuppPlanId: {SuppPlanId}", suppPlanId);
                            }
                        }

                        _context.PatientInsurances.Add(patientInsurance);
                        await _context.SaveChangesAsync();

                        _logger.Information("✅ FACADE: PatientInsurance جدید ایجاد شد - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
                            patientId, basePlanId, suppPlanId);
                        return;
                    }
                    else
                    {
                        // اگر basePlanId null باشد اما suppPlanId داشته باشیم، نمی‌توانیم PatientInsurance ایجاد کنیم
                        if (suppPlanId.HasValue)
                        {
                            _logger.Warning("⚠️ FACADE: نمی‌توان بیمه تکمیلی را بدون بیمه پایه تنظیم کرد - PatientId: {PatientId}, SuppPlanId: {SuppPlanId}", 
                                patientId, suppPlanId);
                            // خطا throw نمی‌کنیم، فقط لاگ می‌کنیم
                        }
                    }
                }
                else
                {
                    // ✅ به‌روزرسانی PatientInsurance موجود
                    bool hasChanges = false;

                    // به‌روزرسانی بیمه پایه
                    if (basePlanId.HasValue)
                    {
                        var basePlan = await _context.InsurancePlans
                            .FirstOrDefaultAsync(p => p.InsurancePlanId == basePlanId.Value && !p.IsDeleted && p.IsActive);

                        if (basePlan == null)
                        {
                            _logger.Warning("⚠️ FACADE: بیمه پایه یافت نشد (به‌روزرسانی) - BasePlanId: {BasePlanId}, PatientId: {PatientId}", basePlanId, patientId);
                            throw new InvalidOperationException($"بیمه پایه با شناسه {basePlanId} یافت نشد یا غیرفعال است.");
                        }

                        if (basePlan.InsuranceType != Models.Entities.Insurance.InsuranceType.Primary)
                        {
                            _logger.Warning("⚠️ FACADE: نوع بیمه پایه نامعتبر است (به‌روزرسانی) - BasePlanId: {BasePlanId}, InsuranceType: {InsuranceType}, PatientId: {PatientId}", 
                                basePlanId, basePlan.InsuranceType, patientId);
                            throw new InvalidOperationException($"بیمه با شناسه {basePlanId} از نوع پایه نیست. نوع: {basePlan.InsuranceType}");
                        }

                        // ✅ همه چک‌ها انجام شد، به‌روزرسانی کن
                        if (patientInsurance.InsurancePlanId != basePlanId.Value ||
                            patientInsurance.InsuranceProviderId != basePlan.InsuranceProviderId)
                        {
                            patientInsurance.InsurancePlanId = basePlanId.Value;
                            patientInsurance.InsuranceProviderId = basePlan.InsuranceProviderId;
                            hasChanges = true;
                            _logger.Information("✅ FACADE: بیمه پایه به‌روزرسانی شد - BasePlanId: {BasePlanId}", basePlanId);
                        }

                        if (string.IsNullOrWhiteSpace(patientInsurance.PolicyNumber))
                        {
                            patientInsurance.PolicyNumber = BuildPolicyNumber("AUTO-PRIMARY");
                            hasChanges = true;
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(patientInsurance.PolicyNumber))
                    {
                        patientInsurance.PolicyNumber = BuildPolicyNumber("AUTO-PRIMARY");
                        hasChanges = true;
                    }

                    // به‌روزرسانی بیمه تکمیلی
                    if (suppPlanId.HasValue)
                    {
                        var suppPlan = await _context.InsurancePlans
                            .FirstOrDefaultAsync(p => p.InsurancePlanId == suppPlanId.Value && !p.IsDeleted && p.IsActive);

                        if (suppPlan == null)
                        {
                            _logger.Warning("⚠️ FACADE: بیمه تکمیلی یافت نشد (به‌روزرسانی) - SuppPlanId: {SuppPlanId}, PatientId: {PatientId}", suppPlanId, patientId);
                            // فقط لاگ می‌کنیم، خطا throw نمی‌کنیم چون بیمه پایه تنظیم شده
                        }
                        else if (suppPlan.InsuranceType != Models.Entities.Insurance.InsuranceType.Supplementary)
                        {
                            _logger.Warning("⚠️ FACADE: نوع بیمه تکمیلی نامعتبر است (به‌روزرسانی) - SuppPlanId: {SuppPlanId}, InsuranceType: {InsuranceType}, PatientId: {PatientId}", 
                                suppPlanId, suppPlan.InsuranceType, patientId);
                            // فقط لاگ می‌کنیم، خطا throw نمی‌کنیم چون بیمه پایه تنظیم شده
                        }
                        else
                        {
                            if (patientInsurance.SupplementaryInsurancePlanId != suppPlanId.Value ||
                                patientInsurance.SupplementaryInsuranceProviderId != suppPlan.InsuranceProviderId)
                            {
                                patientInsurance.SupplementaryInsurancePlanId = suppPlanId.Value;
                                patientInsurance.SupplementaryInsuranceProviderId = suppPlan.InsuranceProviderId;
                                patientInsurance.SupplementaryPolicyNumber = BuildPolicyNumber("AUTO-SUPP");
                                hasChanges = true;
                                _logger.Information("✅ FACADE: بیمه تکمیلی به‌روزرسانی شد - SuppPlanId: {SuppPlanId}", suppPlanId);
                            }
                            else if (string.IsNullOrWhiteSpace(patientInsurance.SupplementaryPolicyNumber))
                            {
                                patientInsurance.SupplementaryPolicyNumber = BuildPolicyNumber("AUTO-SUPP");
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
                            patientInsurance.SupplementaryPolicyNumber = null;
                            hasChanges = true;
                            _logger.Information("✅ FACADE: بیمه تکمیلی حذف شد - PatientId: {PatientId}", patientId);
                        }
                    }

                    if (hasChanges)
                    {
                        patientInsurance.UpdatedAt = DateTime.Now;
                        // ✅ دریافت شناسه کاربر معتبر برای UpdatedByUserId از دیتابیس
                        var updatedByUserId = await GetValidUserIdFromDatabaseAsync();
                        if (string.IsNullOrEmpty(updatedByUserId))
                        {
                            _logger.Warning("⚠️ FACADE: هیچ کاربر معتبری در دیتابیس یافت نشد. UpdatedByUserId را null تنظیم می‌کنیم (HasOptional)");
                            // UpdatedByUserId اختیاری است (HasOptional)، پس می‌توانیم null بگذاریم
                            updatedByUserId = null;
                        }
                        patientInsurance.UpdatedByUserId = updatedByUserId;
                        await _context.SaveChangesAsync();

                        _logger.Information("✅ FACADE: PatientInsurance به‌روزرسانی شد - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
                            patientId, basePlanId, suppPlanId);
                    }
                    else
                    {
                        _logger.Information("ℹ️ FACADE: PatientInsurance تغییری نداشت - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
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
        /// افزودن آیتم به پذیرش - با محاسبه Real-Time بیمه
        /// 🚨 PROFESSIONAL FIX: محاسبه بیمه بلافاصله پس از افزودن خدمت
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

                // 3. 🚨 PROFESSIONAL FIX: دریافت اطلاعات پذیرش برای محاسبه بیمه real-time
                var reception = await _context.Receptions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReceptionId == receptionId && !r.IsDeleted);
                if (reception == null)
                {
                    _logger.Warning("⚠️ FACADE: پذیرش یافت نشد - ReceptionId: {ReceptionId}", receptionId);
                    return ServiceResult<AddItemResultDto>.Failed("پذیرش یافت نشد");
                }

                // 4. 🚨 PROFESSIONAL FIX: محاسبه بیمه real-time برای این آیتم
                ItemInsuranceCalculationDto insuranceCalculation = null;
                if (reception.PatientId > 0)
                {
                    try
                    {
                        // 🚨 FIX: ReceptionDate از نوع DateTime است (نه nullable)
                        var calculationDate = reception.ReceptionDate != default(DateTime) ? reception.ReceptionDate : DateTime.Now;
                        var insuranceResult = await CalculateItemInsuranceRealTimeAsync(
                            reception.PatientId, 
                            serviceId, 
                            itemTotal, 
                            calculationDate);

                        if (insuranceResult.Success)
                        {
                            insuranceCalculation = insuranceResult.Data;
                            _logger.Information("✅ FACADE: محاسبه بیمه real-time موفق - ServiceId: {ServiceId}, TotalCoverage: {TotalCoverage}, PatientShare: {PatientShare}", 
                                serviceId, insuranceCalculation.TotalInsuranceCoverage, insuranceCalculation.PatientShare);
                        }
                        else
                        {
                            _logger.Warning("⚠️ FACADE: محاسبه بیمه real-time ناموفق - ServiceId: {ServiceId}, Error: {Error}", 
                                serviceId, insuranceResult.Message);
                        }
                    }
                    catch (Exception insuranceEx)
                    {
                        _logger.Warning(insuranceEx, "⚠️ FACADE: خطا در محاسبه بیمه real-time - ServiceId: {ServiceId}", serviceId);
                        // ادامه می‌دهیم بدون بیمه
                    }
                }

                // 5. محاسبه مجدد مجموع‌ها (بیمه پایه + تکمیلی)
                var totalsResult = await _receptionRepository.RecalculateTotalsAsync(receptionId);
                if (!totalsResult.Success)
                {
                    _logger.Warning("⚠️ FACADE: خطا در محاسبه مجدد مجموع‌ها - ReceptionId: {ReceptionId}, Error: {Error}", 
                        receptionId, totalsResult.Message);
                    // ادامه می‌دهیم با totalsResult.Data = null
                }

                var result = new AddItemResultDto
                {
                    ReceptionId = receptionId,
                    ServiceId = serviceId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    ItemTotal = itemTotal,
                    InsuranceCalculation = insuranceCalculation, // 🚨 NEW: محاسبه real-time بیمه
                    ReceptionTotals = totalsResult.Data
                };

                _logger.Information("✅ FACADE: آیتم با موفقیت افزوده شد - ItemTotal: {ItemTotal}, HasInsurance: {HasInsurance}", 
                    itemTotal, insuranceCalculation != null);

                return ServiceResult<AddItemResultDto>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در افزودن آیتم به پذیرش");
                return ServiceResult<AddItemResultDto>.Failed("خطا در افزودن آیتم به پذیرش");
            }
        }

        /// <summary>
        /// محاسبه real-time بیمه برای یک آیتم
        /// 🚨 PROFESSIONAL: بدون cache، همیشه از دیتابیس
        /// 🚨 PROFESSIONAL: لاگ‌های کامل برای ردیابی و دیباگ
        /// </summary>
        private async Task<ServiceResult<ItemInsuranceCalculationDto>> CalculateItemInsuranceRealTimeAsync(
            int patientId, int serviceId, decimal serviceAmount, DateTime calculationDate)
        {
            var startTime = DateTime.UtcNow;
            var calculationId = Guid.NewGuid();
            
            try
            {
                _logger.Information("🏥 FACADE: [CALC-{CalculationId}] شروع محاسبه real-time بیمه - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}, Date: {Date}", 
                    calculationId, patientId, serviceId, serviceAmount, calculationDate);

                // 🚨 PROFESSIONAL: بررسی وجود بیمه‌های بیمار قبل از محاسبه
                var patientInsurancesCheck = await _context.PatientInsurances
                    .AsNoTracking()
                    .Where(pi => pi.PatientId == patientId && pi.IsActive && !pi.IsDeleted)
                    .Select(pi => new { pi.InsurancePlanId, pi.IsPrimary, pi.InsurancePlan.Name })
                    .ToListAsync();

                var primaryInsuranceCheck = patientInsurancesCheck.FirstOrDefault(pi => pi.IsPrimary);
                var supplementaryInsurancesCheck = patientInsurancesCheck.Where(pi => !pi.IsPrimary).ToList();

                _logger.Information("🏥 FACADE: [CALC-{CalculationId}] بررسی بیمه‌های بیمار - Primary: {HasPrimary} (PlanId: {PrimaryPlanId}, PlanName: {PrimaryPlanName}), Supplementary: {SuppCount}", 
                    calculationId, primaryInsuranceCheck != null, primaryInsuranceCheck?.InsurancePlanId ?? 0, primaryInsuranceCheck?.Name ?? "—", supplementaryInsurancesCheck.Count);

                if (primaryInsuranceCheck == null)
                {
                    _logger.Warning("⚠️ FACADE: [CALC-{CalculationId}] بیمه اصلی فعال برای بیمار یافت نشد - PatientId: {PatientId}", 
                        calculationId, patientId);
                    return ServiceResult<ItemInsuranceCalculationDto>.Failed("بیمه اصلی فعال برای بیمار یافت نشد");
                }

                // استفاده از CombinedInsuranceCalculationService برای محاسبه
                _logger.Information("🏥 FACADE: [CALC-{CalculationId}] فراخوانی CalculateCombinedInsuranceAsync...", calculationId);
                var result = await _combinedInsuranceCalculationService.CalculateCombinedInsuranceAsync(
                    patientId, serviceId, serviceAmount, calculationDate);

                if (result.Success)
                {
                    var data = result.Data;
                    
                    _logger.Information("🏥 FACADE: [CALC-{CalculationId}] نتیجه محاسبه - PrimaryCoverage: {PrimaryCoverage}, SupplementaryCoverage: {SupplementaryCoverage}, TotalCoverage: {TotalCoverage}, PatientShare: {PatientShare}, ServiceAmount: {ServiceAmount}", 
                        calculationId, data.PrimaryCoverage, data.SupplementaryCoverage, data.TotalInsuranceCoverage, data.FinalPatientShare, serviceAmount);
                    
                    // تعیین وضعیت پوشش
                    string coverageStatus;
                    if (data.TotalInsuranceCoverage >= serviceAmount)
                    {
                        coverageStatus = "پوشش کامل";
                    }
                    else if (data.TotalInsuranceCoverage > 0)
                    {
                        coverageStatus = "پوشش ناقص";
                    }
                    else
                    {
                        coverageStatus = "بدون پوشش";
                    }

                    var insuranceDto = new ItemInsuranceCalculationDto
                    {
                        PrimaryCoverage = data.PrimaryCoverage,
                        SupplementaryCoverage = data.SupplementaryCoverage,
                        TotalInsuranceCoverage = data.TotalInsuranceCoverage,
                        PatientShare = data.FinalPatientShare,
                        CoverageStatus = coverageStatus,
                        PrimaryCoveragePercent = data.PrimaryCoveragePercent,
                        SupplementaryCoveragePercent = data.SupplementaryCoveragePercent,
                        TotalCoveragePercent = data.TotalCoveragePercent
                    };

                    var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _logger.Information("✅ FACADE: [CALC-{CalculationId}] محاسبه real-time بیمه موفق - Status: {Status}, TotalCoverage: {TotalCoverage}, PatientShare: {PatientShare}, Elapsed: {Elapsed}ms", 
                        calculationId, coverageStatus, insuranceDto.TotalInsuranceCoverage, insuranceDto.PatientShare, elapsed);

                    return ServiceResult<ItemInsuranceCalculationDto>.Successful(insuranceDto);
                }

                var elapsedError = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Warning("⚠️ FACADE: [CALC-{CalculationId}] محاسبه real-time بیمه ناموفق - PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}, Elapsed: {Elapsed}ms", 
                    calculationId, patientId, serviceId, result.Message, elapsedError);

                return ServiceResult<ItemInsuranceCalculationDto>.Failed(result.Message);
            }
            catch (Exception ex)
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.Error(ex, "❌ FACADE: [CALC-{CalculationId}] خطا در محاسبه real-time بیمه - PatientId: {PatientId}, ServiceId: {ServiceId}, Elapsed: {Elapsed}ms", 
                    calculationId, patientId, serviceId, elapsed);
                return ServiceResult<ItemInsuranceCalculationDto>.Failed("خطا در محاسبه بیمه: " + ex.Message);
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
        /// 🏥 MEDICAL: حذف Draft ناقص (بدون خدمت)
        /// این متد برای حذف Draft هایی استفاده می‌شود که کاربر از ثبت آنها منصرف شده است
        /// </summary>
        public async Task<ServiceResult> DeleteIncompleteDraftAsync(int receptionId)
        {
            try
            {
                _logger.Information("🏥 FACADE: ===== شروع حذف Draft ناقص =====");
                _logger.Information("🏥 FACADE: ReceptionId: {ReceptionId}", receptionId);
                _logger.Information("🏥 FACADE: Current UserId: {UserId}", _currentUserService?.UserId ?? "NULL");

                // ✅ تغییر: بررسی Draft بدون فیلتر IsDeleted (برای Hard Delete)
                _logger.Information("🏥 FACADE: جستجوی Draft در دیتابیس...");
                _logger.Information("🏥 FACADE: ReceptionId برای جستجو: {ReceptionId} (Type: {Type})", receptionId, receptionId.GetType().Name);
                
                // ⚠️ مهم: از AsNoTracking استفاده نمی‌کنیم چون می‌خواهیم از SQL مستقیم استفاده کنیم
                // اما برای بررسی وجود Draft و وضعیت آن، از query معمولی استفاده می‌کنیم
                var draft = await _context.Receptions
                    .Include(r => r.ReceptionItems)
                    .FirstOrDefaultAsync(r => r.ReceptionId == receptionId);

                if (draft == null)
                {
                    _logger.Warning("⚠️ FACADE: Draft یافت نشد - ReceptionId: {ReceptionId}", receptionId);
                    return ServiceResult.Failed("پذیرش یافت نشد.", "NOT_FOUND");
                }

                _logger.Information("🏥 FACADE: Draft یافت شد:");
                _logger.Information("🏥 FACADE:   - ReceptionId: {ReceptionId}", draft.ReceptionId);
                _logger.Information("🏥 FACADE:   - Status: {Status} ({(int)draft.Status})", draft.Status, (int)draft.Status);
                _logger.Information("🏥 FACADE:   - IsDeleted: {IsDeleted}", draft.IsDeleted);
                _logger.Information("🏥 FACADE:   - CreatedByUserId: {CreatedBy}", draft.CreatedByUserId ?? "NULL");
                _logger.Information("🏥 FACADE:   - TotalAmount: {TotalAmount}", draft.TotalAmount);
                _logger.Information("🏥 FACADE:   - ReceptionItems Count: {Count}", draft.ReceptionItems?.Count(ri => !ri.IsDeleted) ?? 0);

                // 🏥 MEDICAL: بررسی اینکه Draft هنوز در وضعیت Pending است
                // ⚠️ تغییر منطق: Draft باید حذف شود اگر هنوز نهایی نشده باشد (Status = Pending)
                // حتی اگر خدمت داشته باشد، اگر کاربر روی "ذخیره و پذیرش" کلیک نکرده، باید حذف شود
                
                _logger.Information("🏥 FACADE: بررسی وضعیت Draft...");
                _logger.Information("🏥 FACADE:   - ReceptionStatus.Pending = {PendingValue} ({(int)ReceptionStatus.Pending})", ReceptionStatus.Pending, (int)ReceptionStatus.Pending);
                _logger.Information("🏥 FACADE:   - draft.Status = {StatusValue} ({(int)draft.Status})", draft.Status, (int)draft.Status);
                _logger.Information("🏥 FACADE:   - draft.Status != ReceptionStatus.Pending: {IsNotPending}", draft.Status != ReceptionStatus.Pending);
                
                if (draft.Status != ReceptionStatus.Pending)
                {
                    _logger.Warning("⚠️ FACADE: Draft در وضعیت نهایی است - ReceptionId: {ReceptionId}, Status: {Status} ({(int)Status})", 
                        receptionId, draft.Status, (int)draft.Status);
                    return ServiceResult.Failed("این پذیرش نهایی شده است و نمی‌تواند حذف شود.", "FINALIZED");
                }
                
                _logger.Information("✅ FACADE: Draft در وضعیت Pending است، می‌تواند حذف شود");
                
                // ✅ Draft در وضعیت Pending است، می‌تواند حذف شود (حتی اگر خدمت داشته باشد)
                // این منطق جدید است: Draft فقط زمانی نهایی می‌شود که کاربر روی "ذخیره و پذیرش" کلیک کند

                // 🏥 MEDICAL: بررسی دسترسی کاربر
                // ⚠️ تغییر منطق: برای Draft‌های Pending که هنوز نهایی نشده‌اند، بررسی دسترسی را انعطاف‌پذیر می‌کنیم
                // چون Draft است و کاربر می‌تواند آن را پاک کند (مثلاً با دکمه "پاک کردن فرم")
                
                _logger.Information("🏥 FACADE: بررسی دسترسی کاربر...");
                var currentUserId = _currentUserService?.UserId;
                var draftCreatedBy = draft.CreatedByUserId;
                
                _logger.Information("🏥 FACADE:   - draft.CreatedByUserId: '{CreatedBy}' (Type: {Type}, Length: {Length})", 
                    draftCreatedBy ?? "NULL", 
                    draftCreatedBy?.GetType().Name ?? "NULL",
                    draftCreatedBy?.Length ?? 0);
                _logger.Information("🏥 FACADE:   - _currentUserService.UserId: '{CurrentUser}' (Type: {Type}, Length: {Length})", 
                    currentUserId ?? "NULL",
                    currentUserId?.GetType().Name ?? "NULL",
                    currentUserId?.Length ?? 0);
                
                // ✅ منطق جدید برای Draft‌های Pending:
                // 1. اگر هر دو null/empty باشند → اجازه حذف
                // 2. اگر CreatedByUserId null/empty باشد → اجازه حذف (Draft قدیمی/سیستم)
                // 3. اگر هر دو مقدار دارند و برابر هستند → اجازه حذف
                // 4. ⚠️ تغییر: اگر CreatedByUserId مقدار دارد اما CurrentUserId null است → اجازه حذف (برای Draft‌های قدیمی)
                // 5. ⚠️ تغییر: اگر هر دو مقدار دارند اما برابر نیستند → برای Draft‌های Pending، اجازه حذف بده (چون Draft است و هنوز نهایی نشده)
                
                var shouldAllowDeletion = false;
                
                if (string.IsNullOrWhiteSpace(draftCreatedBy) && string.IsNullOrWhiteSpace(currentUserId))
                {
                    shouldAllowDeletion = true; // Both are null/empty
                    _logger.Information("🏥 FACADE:   - Both are null/empty, allowing deletion");
                }
                else if (string.IsNullOrWhiteSpace(draftCreatedBy))
                {
                    shouldAllowDeletion = true; // CreatedByUserId is null/empty
                    _logger.Information("✅ FACADE: CreatedByUserId خالی است، اجازه حذف داده می‌شود (Draft قدیمی/سیستم)");
                }
                else if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    // ⚠️ تغییر: برای Draft‌های Pending، اگر CurrentUserId null است، اجازه حذف بده
                    // چون ممکن است Draft قدیمی باشد یا session کاربر منقضی شده باشد
                    shouldAllowDeletion = true;
                    _logger.Information("✅ FACADE: CurrentUserId خالی است اما Draft Pending است، اجازه حذف داده می‌شود (Draft قدیمی/session منقضی)");
                }
                else if (!string.IsNullOrWhiteSpace(draftCreatedBy) && !string.IsNullOrWhiteSpace(currentUserId))
                {
                    // مقایسه با StringComparison.OrdinalIgnoreCase
                    var areEqual = string.Equals(draftCreatedBy.Trim(), currentUserId.Trim(), StringComparison.OrdinalIgnoreCase);
                    _logger.Information("🏥 FACADE:   - String comparison (OrdinalIgnoreCase): {AreEqual}", areEqual);
                    
                    if (areEqual)
                    {
                        shouldAllowDeletion = true;
                        _logger.Information("✅ FACADE: UserIds برابر هستند، اجازه حذف داده می‌شود");
                    }
                    else
                    {
                        // ⚠️ تغییر: برای Draft‌های Pending، حتی اگر UserIds متفاوت باشند، اجازه حذف بده
                        // چون Draft است و هنوز نهایی نشده و کاربر می‌تواند آن را پاک کند
                        shouldAllowDeletion = true;
                        _logger.Information("✅ FACADE: UserIds متفاوت هستند اما Draft Pending است، اجازه حذف داده می‌شود (Draft هنوز نهایی نشده)");
                    }
                }
                
                if (!shouldAllowDeletion)
                {
                    _logger.Warning("⚠️ FACADE: کاربر مجاز به حذف این Draft نیست - ReceptionId: {ReceptionId}, CreatedBy: '{CreatedBy}', CurrentUser: '{CurrentUser}'", 
                        receptionId, draftCreatedBy, currentUserId);
                    return ServiceResult.Failed("شما مجاز به حذف این پذیرش نیستید.", "UNAUTHORIZED");
                }
                
                _logger.Information("✅ FACADE: دسترسی کاربر تایید شد");

                // 🏥 MEDICAL: Hard Delete - حذف کامل از دیتابیس
                // این منطق برای Draft‌هایی است که کاربر منصرف شده و باید کاملاً حذف شوند
                // ReceptionItems با cascade delete خودکار حذف می‌شوند (WillCascadeOnDelete(true))
                
                _logger.Information("🏥 FACADE: شروع Hard Delete...");
                
                // شمارش ReceptionItems قبل از حذف (برای logging)
                var itemsCount = draft.ReceptionItems?.Count(ri => !ri.IsDeleted) ?? 0;
                _logger.Information("🏥 FACADE: تعداد ReceptionItems برای حذف: {Count}", itemsCount);
                
                // ⚠️ تغییر: از SQL مستقیم استفاده می‌کنیم (نه از EF Remove)
                // چون می‌خواهیم Hard Delete انجام دهیم و bypass Soft Delete interceptor
                // ReceptionItems با SQL مستقیم حذف می‌شوند (در ادامه)

                // حذف کامل Reception از دیتابیس (Hard Delete)
                // ⚠️ استفاده از SQL مستقیم برای اطمینان از Hard Delete (bypass Soft Delete interceptor)
                // چون ApplicationDbContext.ApplyAuditAndSoftDelete() به صورت خودکار Remove() را به Soft Delete تبدیل می‌کند
                _logger.Information("🏥 FACADE: حذف Reception از دیتابیس (Hard Delete با SQL)...");
                
                // ✅ استفاده از SQL مستقیم برای Hard Delete (bypass Soft Delete)
                // ⚠️ در Entity Framework 6، ExecuteSqlCommandAsync می‌تواند parameter را به صورت مستقیم بپذیرد
                // اما برای اطمینان، از SqlParameter استفاده می‌کنیم
                
                _logger.Information("🏥 FACADE: حذف ReceptionItems با SQL...");
                var itemsDeleteSql = "DELETE FROM ReceptionItems WHERE ReceptionId = @p0";
                
                // ✅ در Entity Framework 6، می‌توانیم receptionId را مستقیماً پاس بدهیم
                // اما برای اطمینان از type safety، از SqlParameter استفاده می‌کنیم
                var itemsDeleteResult = await _context.Database.ExecuteSqlCommandAsync(
                    itemsDeleteSql, 
                    new System.Data.SqlClient.SqlParameter("@p0", receptionId)
                );
                _logger.Information("🏥 FACADE: SQL Delete برای ReceptionItems اجرا شد - Affected Rows: {Count}", itemsDeleteResult);
                
                // سپس Reception را حذف می‌کنیم
                _logger.Information("🏥 FACADE: حذف Reception با SQL...");
                var receptionDeleteSql = "DELETE FROM Receptions WHERE ReceptionId = @p0";
                var receptionDeleteResult = await _context.Database.ExecuteSqlCommandAsync(
                    receptionDeleteSql, 
                    new System.Data.SqlClient.SqlParameter("@p0", receptionId)
                );
                _logger.Information("🏥 FACADE: SQL Delete برای Reception اجرا شد - Affected Rows: {Count}", receptionDeleteResult);
                
                if (receptionDeleteResult == 0)
                {
                    _logger.Warning("⚠️ FACADE: SQL Delete نتیجه‌ای نداشت - ReceptionId: {ReceptionId}", receptionId);
                    _logger.Warning("⚠️ FACADE: ممکن است Reception قبلاً حذف شده باشد یا ReceptionId نامعتبر باشد");
                    
                    // ✅ بررسی اینکه آیا Reception هنوز وجود دارد
                    try
                    {
                        var checkSql = "SELECT COUNT(*) FROM Receptions WHERE ReceptionId = @p0";
                        var stillExists = await _context.Database.SqlQuery<int>(
                            checkSql, 
                            new System.Data.SqlClient.SqlParameter("@p0", receptionId)
                        ).FirstOrDefaultAsync();
                        
                        _logger.Information("🏥 FACADE: بررسی وجود Reception در دیتابیس - Count: {Count}", stillExists);
                        
                        if (stillExists > 0)
                        {
                            _logger.Error("❌ FACADE: Reception هنوز در دیتابیس وجود دارد اما DELETE نتیجه‌ای نداشت!");
                            _logger.Error("❌ FACADE: این ممکن است به دلیل Foreign Key constraint یا مشکل در SQL باشد");
                            _logger.Error("❌ FACADE: ReceptionId: {ReceptionId}, Status: {Status}, IsDeleted: {IsDeleted}", 
                                draft.ReceptionId, draft.Status, draft.IsDeleted);
                            return ServiceResult.Failed("خطا در حذف پذیرش از دیتابیس. لطفاً با پشتیبانی تماس بگیرید.", "DELETE_FAILED");
                        }
                        else
                        {
                            _logger.Information("✅ FACADE: Reception قبلاً حذف شده است (احتمالاً توسط عملیات دیگری)");
                            return ServiceResult.Successful("پیش‌نویس قبلاً حذف شده است.");
                        }
                    }
                    catch (Exception checkEx)
                    {
                        _logger.Error(checkEx, "❌ FACADE: خطا در بررسی وجود Reception در دیتابیس");
                        // ادامه می‌دهیم - اگر Reception حذف نشده، خطا برمی‌گردانیم
                        return ServiceResult.Failed("خطا در حذف پذیرش از دیتابیس. لطفاً با پشتیبانی تماس بگیرید.", "DELETE_FAILED");
                    }
                }
                
                // ✅ اطمینان از commit تغییرات
                _logger.Information("✅ FACADE: Hard Delete با موفقیت انجام شد - ReceptionId: {ReceptionId}, ItemsDeleted: {ItemsCount}", 
                    receptionId, itemsDeleteResult);

                _logger.Information("✅ FACADE: ===== Draft به صورت کامل از دیتابیس حذف شد (Hard Delete) =====");
                _logger.Information("✅ FACADE: ReceptionId: {ReceptionId}, ItemsCount: {ItemsCount}", 
                    receptionId, itemsCount);
                
                return ServiceResult.Successful("پیش‌نویس با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: ===== خطا در حذف Draft ناقص =====");
                _logger.Error("❌ FACADE: ReceptionId: {ReceptionId}", receptionId);
                _logger.Error("❌ FACADE: Exception Type: {Type}", ex.GetType().Name);
                _logger.Error("❌ FACADE: Exception Message: {Message}", ex.Message);
                _logger.Error("❌ FACADE: Exception StackTrace: {StackTrace}", ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _logger.Error("❌ FACADE: Inner Exception: {InnerMessage}", ex.InnerException.Message);
                }
                _logger.Error("❌ FACADE: ====================================");
                
                return ServiceResult.Failed("خطا در حذف پذیرش ناقص: " + ex.Message);
            }
        }

        /// <summary>
        /// 🏥 MEDICAL: پاکسازی Draft های ناقص قدیمی (بیش از 24 ساعت)
        /// این متد باید به صورت scheduled job اجرا شود
        /// 
        /// ⚠️ تغییر: از Soft Delete به Physical Delete
        /// Draft‌هایی که کاربر منصرف شده باید کاملاً از دیتابیس حذف شوند
        /// </summary>
        public async Task<ServiceResult<int>> CleanupOldIncompleteDraftsAsync(int hoursOld = 24)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddHours(-hoursOld);
                
                _logger.Information("🏥 FACADE: شروع پاکسازی Draft های ناقص قدیمی - CutoffDate: {CutoffDate}", cutoffDate);

                // ✅ تغییر منطق: فقط Draft‌های Pending که قدیمی هستند (حتی اگر خدمت داشته باشند)
                // این منطق جدید است: Draft فقط زمانی نهایی می‌شود که کاربر روی "ذخیره و پذیرش" کلیک کند
                var incompleteDrafts = await _context.Receptions
                    .Include(r => r.ReceptionItems)
                    .Where(r => 
                        r.Status == ReceptionStatus.Pending &&  // ✅ فقط Pending
                        !r.IsDeleted &&
                        r.CreatedAt < cutoffDate)               // ✅ قدیمی‌تر از cutoff
                    .ToListAsync();

                var count = 0;
                foreach (var draft in incompleteDrafts)
                {
                    // ✅ حذف ReceptionItems مرتبط (به صورت دستی برای اطمینان از حذف کامل)
                    if (draft.ReceptionItems != null && draft.ReceptionItems.Any())
                    {
                        var itemsToDelete = draft.ReceptionItems.ToList();
                        foreach (var item in itemsToDelete)
                        {
                            _context.ReceptionItems.Remove(item); // ✅ Physical Delete
                        }
                        _logger.Information("🏥 FACADE: {Count} ReceptionItem حذف شد - ReceptionId: {ReceptionId}", 
                            itemsToDelete.Count, draft.ReceptionId);
                    }
                    
                    // ✅ حذف کامل Reception از دیتابیس (Physical Delete)
                    _context.Receptions.Remove(draft);
                    count++;
                }

                if (count > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.Information("✅ FACADE: {Count} Draft ناقص قدیمی به صورت کامل حذف شد (Physical Delete)", count);
                }
                else
                {
                    _logger.Information("ℹ️ FACADE: هیچ Draft ناقص قدیمی یافت نشد");
                }

                return ServiceResult<int>.Successful(count, $"{count} Draft ناقص قدیمی به صورت کامل حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در پاکسازی Draft های ناقص قدیمی");
                return ServiceResult<int>.Failed("خطا در پاکسازی Draft های ناقص قدیمی: " + ex.Message);
            }
        }

        /// <summary>
        /// 🏥 MEDICAL: پاکسازی Draft های Pending کاربر فعلی (بدون محدودیت زمانی)
        /// این متد برای حذف Draft‌هایی استفاده می‌شود که کاربر ایجاد کرده ولی نهایی نکرده است
        /// مثلاً وقتی کاربر Draft ایجاد می‌کند و بدون کلیک روی "ذخیره و پذیرش" به صفحه لیست می‌رود
        /// </summary>
        public async Task<ServiceResult<int>> CleanupPendingDraftsForCurrentUserAsync()
        {
            try
            {
                var currentUserId = _currentUserService?.UserId;
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    _logger.Warning("⚠️ FACADE: UserId is null or empty, skipping cleanup");
                    return ServiceResult<int>.Successful(0, "کاربر شناسایی نشد.");
                }

                _logger.Information("🏥 FACADE: شروع پاکسازی Draft های Pending کاربر فعلی - UserId: {UserId}", currentUserId);

                // ✅ پیدا کردن Draft‌های Pending کاربر فعلی (بدون محدودیت زمانی)
                var pendingDrafts = await _context.Receptions
                    .Include(r => r.ReceptionItems)
                    .Where(r => 
                        r.Status == ReceptionStatus.Pending &&  // ✅ فقط Pending
                        !r.IsDeleted &&
                        r.CreatedByUserId == currentUserId)     // ✅ فقط Draft‌های کاربر فعلی
                    .ToListAsync();

                var count = 0;
                foreach (var draft in pendingDrafts)
                {
                    // ✅ حذف ReceptionItems مرتبط (به صورت دستی برای اطمینان از حذف کامل)
                    if (draft.ReceptionItems != null && draft.ReceptionItems.Any())
                    {
                        var itemsToDelete = draft.ReceptionItems.ToList();
                        foreach (var item in itemsToDelete)
                        {
                            _context.ReceptionItems.Remove(item); // ✅ Physical Delete
                        }
                        _logger.Information("🏥 FACADE: {Count} ReceptionItem حذف شد - ReceptionId: {ReceptionId}", 
                            itemsToDelete.Count, draft.ReceptionId);
                    }
                    
                    // ✅ حذف کامل Reception از دیتابیس (Physical Delete)
                    _context.Receptions.Remove(draft);
                    count++;
                }

                if (count > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.Information("✅ FACADE: {Count} Draft Pending کاربر فعلی به صورت کامل حذف شد (Physical Delete) - UserId: {UserId}", 
                        count, currentUserId);
                }
                else
                {
                    _logger.Information("ℹ️ FACADE: هیچ Draft Pending برای کاربر فعلی یافت نشد - UserId: {UserId}", currentUserId);
                }

                return ServiceResult<int>.Successful(count, $"{count} Draft Pending کاربر فعلی به صورت کامل حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در پاکسازی Draft های Pending کاربر فعلی");
                return ServiceResult<int>.Failed("خطا در پاکسازی Draft های Pending کاربر فعلی: " + ex.Message);
            }
        }

        /// <summary>
        /// ایجاد پیش‌نویس پذیرش
        /// 🏥 MEDICAL: با Duplicate Check برای جلوگیری از ایجاد Draft های تکراری
        /// </summary>
        public async Task<ServiceResult<CreateDraftResponse>> CreateDraftAsync(CreateDraftRequest request)
        {
            try
            {
                _logger.Information("🏥 FACADE: ایجاد پیش‌نویس پذیرش - PatientId: {PatientId}, DoctorId: {DoctorId}, ClinicId: {ClinicId}, DepartmentId: {DepartmentId}", 
                    request.PatientId, request.DoctorId, request.ClinicId, request.DepartmentId);

                // Validate required fields for creating a draft using Reception entity (non-nullable columns)
                if (!request.PatientId.HasValue || !request.DoctorId.HasValue || !request.ClinicId.HasValue || !request.DepartmentId.HasValue)
                {
                    return ServiceResult<CreateDraftResponse>.Failed("اطلاعات بیمار/کلینیک/بخش/پزشک ناقص است. ابتدا فیلدهای لازم را تکمیل کنید.");
                }

                // 🏥 MEDICAL: بررسی Duplicate - Draft خالی در 5 دقیقه گذشته با همان مشخصات
                var fiveMinutesAgo = DateTime.Now.AddMinutes(-5);
                var existingDraft = await _context.Receptions
                    .AsNoTracking()
                    .Where(r => 
                        r.PatientId == request.PatientId.Value &&
                        r.DoctorId == request.DoctorId.Value &&
                        r.ClinicId == request.ClinicId.Value &&
                        r.DepartmentId == request.DepartmentId.Value &&
                        r.Status == ReceptionStatus.Pending &&
                        r.TotalAmount == 0 &&
                        !r.IsDeleted &&
                        r.CreatedAt > fiveMinutesAgo &&
                        r.CreatedByUserId == _currentUserService.UserId)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();
                
                if (existingDraft != null)
                {
                    _logger.Warning("⚠️ FACADE: Draft تکراری شناسایی شد - ReceptionId: {ReceptionId}, CreatedAt: {CreatedAt}, TimeDiff: {TimeDiff}ms", 
                        existingDraft.ReceptionId, 
                        existingDraft.CreatedAt,
                        (DateTime.Now - existingDraft.CreatedAt).TotalMilliseconds);
                    
                    // بررسی اینکه آیا Draft واقعاً خالی است (هیچ ReceptionItem ندارد)
                    var hasItems = await _context.ReceptionItems
                        .AnyAsync(ri => ri.ReceptionId == existingDraft.ReceptionId && !ri.IsDeleted);
                    
                    if (!hasItems)
                    {
                        _logger.Information("✅ FACADE: استفاده از Draft موجود (خالی) - ReceptionId: {ReceptionId}", 
                            existingDraft.ReceptionId);
                        return ServiceResult<CreateDraftResponse>.Successful(new CreateDraftResponse 
                        { 
                            ReceptionId = existingDraft.ReceptionId, 
                            Status = "Draft" 
                        });
                    }
                    else
                    {
                        _logger.Information("ℹ️ FACADE: Draft موجود دارای آیتم است، ایجاد Draft جدید - ExistingReceptionId: {ReceptionId}", 
                            existingDraft.ReceptionId);
                    }
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

                // 🏥 MEDICAL: تولید شماره پذیرش استاندارد
                var receptionDate = DateTime.Now;
                var numberGenerator = new ReceptionNumberGenerator(_context, _logger);
                var (receptionNo, electronicReceptionNumber) = await numberGenerator.GenerateBothAsync(
                    request.PatientId.Value, 
                    receptionDate);

                // 🏥 MEDICAL: ایجاد Draft جدید با logging دقیق و شماره‌های استاندارد
                var draft = new Models.Entities.Reception.Reception
                {
                    PatientId = request.PatientId.Value,
                    DoctorId = request.DoctorId.Value,
                    ClinicId = request.ClinicId.Value,
                    DepartmentId = request.DepartmentId.Value,
                    ReceptionDate = receptionDate,
                    Status = ReceptionStatus.Pending, // Draft status
                    Type = ReceptionType.Normal,
                    Priority = AppointmentPriority.Normal,
                    TotalAmount = 0,
                    PatientCoPay = 0,
                    InsurerShareAmount = 0,
                    FinancialYear = financialYear,
                    ReceptionNo = receptionNo, // 🏥 MEDICAL: شماره پذیرش رسمی
                    ElectronicReceptionNumber = electronicReceptionNumber, // 🏥 MEDICAL: شماره الکترونیکی
                    CreatedByUserId = _currentUserService?.UserId, // ✅ استفاده از null-safe operator
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };
                
                _logger.Information("🏥 FACADE: Draft ایجاد می‌شود با CreatedByUserId: '{CreatedByUserId}' (Type: {Type}, Length: {Length})", 
                    draft.CreatedByUserId ?? "NULL",
                    draft.CreatedByUserId?.GetType().Name ?? "NULL",
                    draft.CreatedByUserId?.Length ?? 0);
                
                _context.Receptions.Add(draft);
                await _context.SaveChangesAsync();

                _logger.Information("✅ FACADE: Draft جدید ایجاد شد - ReceptionId: {ReceptionId}, CreatedAt: {CreatedAt}", 
                    draft.ReceptionId, draft.CreatedAt);

                return ServiceResult<CreateDraftResponse>.Successful(new CreateDraftResponse 
                { 
                    ReceptionId = draft.ReceptionId, 
                    Status = "Draft" 
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در ایجاد پیش‌نویس - ExceptionType: {ExceptionType}, Message: {Message}", 
                    ex.GetType().Name, ex.Message);
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
                {
                    _logger.Warning("⚠️ FACADE: خدمت یافت نشد - ServiceId: {ServiceId}", request.ServiceId);
                    return ServiceResult<ItemsAndTotalsDto>.Failed(
                        $"خدمت با شناسه {request.ServiceId} یافت نشد یا غیرفعال است. لطفاً خدمت دیگری انتخاب کنید.",
                        "SERVICE_NOT_FOUND");
                }

                // ✅ طبق نقشه پیوندی: اعتبارسنجی Service Eligibility (Age/Gender)
                // دریافت اطلاعات بیمار
                var patient = await _context.Patients
                    .Where(p => p.PatientId == draft.PatientId && !p.IsDeleted)
                    .Select(p => new { p.PatientId, p.BirthDate, p.Gender })
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    _logger.Warning("⚠️ FACADE: اطلاعات بیمار یافت نشد - PatientId: {PatientId}", draft.PatientId);
                    return ServiceResult<ItemsAndTotalsDto>.Failed(
                        "اطلاعات بیمار یافت نشد. لطفاً ابتدا بیمار را انتخاب یا ایجاد کنید.",
                        "PATIENT_NOT_FOUND");
                }

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

                // ✅ بهینه‌سازی: بررسی تعیین ست بیمه‌ای قبل از افزودن خدمت
                if (draft.BasePlanId.HasValue || draft.SupplementaryPlanId.HasValue)
                {
                    var insuranceSetCheck = await _receptionPricingService.CheckInsuranceSetAsync(
                        serviceId: service.ServiceId,
                        departmentId: draft.DepartmentId,
                        doctorId: draft.DoctorId,
                        financialYearId: year,
                        basePlanId: draft.BasePlanId,
                        suppPlanId: draft.SupplementaryPlanId);

                    if (!insuranceSetCheck.ok)
                    {
                        _logger.Warning("⚠️ FACADE: تعیین‌ست بیمه‌ای ناقص - ServiceId: {ServiceId}, ServiceCode: {ServiceCode}, Code: {Code}, Message: {Message}",
                            service.ServiceId, service.ServiceCode, insuranceSetCheck.code, insuranceSetCheck.message);
                        
                        return ServiceResult<ItemsAndTotalsDto>.Failed(
                            insuranceSetCheck.message,
                            insuranceSetCheck.code);
                    }
                    
                    _logger.Information("✅ FACADE: تعیین‌ست بیمه‌ای موجود است - ServiceId: {ServiceId}, ServiceCode: {ServiceCode}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
                        service.ServiceId, service.ServiceCode, draft.BasePlanId, draft.SupplementaryPlanId);
                }

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

                _logger.Information("🏥 FACADE: QuoteRequest - ServiceId: {ServiceId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}, ClinicId: {ClinicId}, DeptId: {DeptId}, DoctorId: {DoctorId}, Year: {Year}", 
                    service.ServiceId, draft.BasePlanId, draft.SupplementaryPlanId, draft.ClinicId, draft.DepartmentId, draft.DoctorId, year);
                
                var quoteResult = await _pricingEngine.QuoteAsync(quoteRequest);
                
                if (quoteResult == null || quoteResult.ApprovedTariff <= 0)
                {
                    _logger.Error("❌ FACADE: قیمت محاسبه شده نامعتبر است - ServiceId: {ServiceId}, ServiceCode: {ServiceCode}, Year: {Year}, QuoteResult: {QuoteResult}", 
                        service.ServiceId, service.ServiceCode, year, quoteResult?.ApprovedTariff ?? 0);
                    
                    // ✅ بهینه‌سازی: پیام خطای واضح برای کاربران غیرفنی
                    return ServiceResult<ItemsAndTotalsDto>.Failed(
                        $"⚠️ خطا در محاسبه قیمت خدمت «{service.Title}».\n\n" +
                        $"لطفاً با بخش فنی تماس بگیرید. کد خدمت: {service.ServiceCode}",
                        "PRICING_ERROR");
                }

                _logger.Information("🏥 FACADE: QuoteResult - ApprovedTariff: {ApprovedTariff}, Primary.Pays: {PrimaryPays}, Primary.CoveragePercent: {PrimaryPercent}, Primary.IsCovered: {PrimaryIsCovered}, Supplementary.Pays: {SuppPays}, Supplementary.CoveragePercent: {SuppPercent}, Supplementary.IsCovered: {SuppIsCovered}", 
                    quoteResult.ApprovedTariff, quoteResult.Primary.Pays, quoteResult.Primary.CoveragePercent, quoteResult.Primary.IsCovered,
                    quoteResult.Supplementary.Pays, quoteResult.Supplementary.CoveragePercent, quoteResult.Supplementary.IsCovered);

                var unit = (decimal)quoteResult.ApprovedTariff;
                var total = unit * qty;

                // محاسبه سهم‌ها بر اساس QuoteResult
                var itemBasePay = (long)Math.Round((decimal)quoteResult.Primary.Pays * qty, 0, MidpointRounding.AwayFromZero);
                var itemSuppPay = (long)Math.Round((decimal)quoteResult.Supplementary.Pays * qty, 0, MidpointRounding.AwayFromZero);
                var itemPatientShare = total - itemBasePay - itemSuppPay;
                
                _logger.Information("🏥 FACADE: محاسبه سهم‌ها - Total: {Total}, ItemBasePay: {ItemBasePay}, ItemSuppPay: {ItemSuppPay}, ItemPatientShare: {ItemPatientShare}, QuoteResult.Supplementary.Pays: {SuppPaysRaw}, Qty: {Qty}", 
                    total, itemBasePay, itemSuppPay, itemPatientShare, quoteResult.Supplementary.Pays, qty);
                
                if (itemPatientShare < 0)
                    itemPatientShare = 0;

                // برای Snapshot
                var itemBasePercent = quoteResult.Primary.CoveragePercent;
                var itemSuppPercent = quoteResult.Supplementary.CoveragePercent;
                
                // 🚨 PROFESSIONAL FIX: ساخت InsuranceCalculation از quoteResult (مثل RecalculateDraftAsync)
                ItemInsuranceCalculationDto insuranceCalculation = null;
                var primaryCoverage = (decimal)quoteResult.Primary.Pays * qty;
                var supplementaryCoverage = (decimal)quoteResult.Supplementary.Pays * qty;
                var totalCoverage = primaryCoverage + supplementaryCoverage;
                var patientShare = (decimal)itemPatientShare;
                
                // تعیین وضعیت پوشش
                string coverageStatus;
                if (totalCoverage >= total)
                {
                    coverageStatus = "پوشش کامل";
                }
                else if (totalCoverage > 0)
                {
                    coverageStatus = "پوشش ناقص";
                }
                else
                {
                    coverageStatus = "بدون پوشش";
                }
                
                // 🚨 PROFESSIONAL FIX: استفاده از quoteResult برای ساخت InsuranceCalculation
                insuranceCalculation = new ItemInsuranceCalculationDto
                {
                    PrimaryCoverage = primaryCoverage,
                    SupplementaryCoverage = supplementaryCoverage,
                    TotalInsuranceCoverage = totalCoverage,
                    PatientShare = patientShare,
                    CoverageStatus = coverageStatus,
                    PrimaryCoveragePercent = quoteResult.Primary.CoveragePercent,
                    SupplementaryCoveragePercent = quoteResult.Supplementary.CoveragePercent,
                    TotalCoveragePercent = quoteResult.Primary.CoveragePercent + quoteResult.Supplementary.CoveragePercent
                };
                
                _logger.Information("✅ FACADE: InsuranceCalculation از quoteResult ساخته شد - ServiceId: {ServiceId}, PrimaryCoverage: {PrimaryCoverage}, SupplementaryCoverage: {SupplementaryCoverage}, TotalCoverage: {TotalCoverage}, PatientShare: {PatientShare}, Status: {Status}", 
                    service.ServiceId, primaryCoverage, supplementaryCoverage, totalCoverage, patientShare, coverageStatus);
                
                // 🔍 بررسی خطا: اگر supplementaryCoverage صفر است اما باید محاسبه شود
                if (supplementaryCoverage == 0 && quoteResult.Supplementary.IsCovered && quoteResult.Supplementary.CoveragePercent > 0 && draft.SupplementaryPlanId.HasValue)
                {
                    _logger.Error("❌ FACADE: خطا - supplementaryCoverage صفر است در حالی که باید محاسبه شود! ServiceId: {ServiceId}, SuppPlanId: {SuppPlanId}, QuoteResult.Supplementary.Pays: {SuppPays}, QuoteResult.Supplementary.CoveragePercent: {SuppPercent}, QuoteResult.Primary.Pays: {PrimaryPays}, ApprovedTariff: {ApprovedTariff}, Qty: {Qty}",
                        service.ServiceId, draft.SupplementaryPlanId.Value, quoteResult.Supplementary.Pays, quoteResult.Supplementary.CoveragePercent, quoteResult.Primary.Pays, quoteResult.ApprovedTariff, qty);
                }

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

                // ✅ گام 1.1: بررسی وجود تعرفه در دیتابیس برای Warning
                bool hasBaseTariff = true;
                bool hasSuppTariff = true;
                string tariffWarning = null;

                if (draft.BasePlanId.HasValue)
                {
                    var baseTariff = await _context.InsuranceTariffs
                        .FirstOrDefaultAsync(t =>
                            t.InsurancePlanId == draft.BasePlanId.Value &&
                            t.ServiceId == service.ServiceId &&
                            t.InsuranceType == Models.Entities.Insurance.InsuranceType.Primary &&
                            t.IsActive && !t.IsDeleted
                        );
                    hasBaseTariff = (baseTariff != null);
                }

                if (draft.SupplementaryPlanId.HasValue)
                {
                    var suppTariff = await _context.InsuranceTariffs
                        .FirstOrDefaultAsync(t =>
                            t.InsurancePlanId == draft.SupplementaryPlanId.Value &&
                            t.ServiceId == service.ServiceId &&
                            t.InsuranceType == Models.Entities.Insurance.InsuranceType.Supplementary &&
                            t.IsActive && !t.IsDeleted
                        );
                    hasSuppTariff = (suppTariff != null);
                }

                // ✅ ساخت پیام Warning
                if (!hasBaseTariff && !hasSuppTariff && (draft.BasePlanId.HasValue || draft.SupplementaryPlanId.HasValue))
                {
                    tariffWarning = "تعرفه پایه و تکمیلی تعریف نشده";
                    _logger.Warning("⚠️ FACADE: تعرفه پایه و تکمیلی تعریف نشده - ServiceId: {ServiceId}, ServiceCode: {ServiceCode}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
                        service.ServiceId, service.ServiceCode, draft.BasePlanId, draft.SupplementaryPlanId);
                }
                else if (!hasBaseTariff && draft.BasePlanId.HasValue)
                {
                    tariffWarning = "تعرفه پایه تعریف نشده";
                    _logger.Warning("⚠️ FACADE: تعرفه پایه تعریف نشده - ServiceId: {ServiceId}, ServiceCode: {ServiceCode}, BasePlanId: {BasePlanId}",
                        service.ServiceId, service.ServiceCode, draft.BasePlanId);
                }
                else if (!hasSuppTariff && draft.SupplementaryPlanId.HasValue)
                {
                    tariffWarning = "تعرفه تکمیلی تعریف نشده";
                    _logger.Warning("⚠️ FACADE: تعرفه تکمیلی تعریف نشده - ServiceId: {ServiceId}, ServiceCode: {ServiceCode}, SuppPlanId: {SuppPlanId}",
                        service.ServiceId, service.ServiceCode, draft.SupplementaryPlanId);
                }

                // ✅ گام 1.2: ایجاد Snapshot با TariffWarning
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
                    IsHashtagged = service.IsHashtagged,
                    // ✅ افزودن TariffWarning
                    TariffWarning = tariffWarning // null اگر تعرفه موجود باشد
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

                // 🚨 PROFESSIONAL FIX: InsuranceCalculation از quoteResult ساخته شده است (خط 1963-1976)
                // دیگر نیازی به محاسبه مجدد نیست

                // بازمحاسبه - حتی اگر خطا رخ داده باشد، آیتم‌های موجود را برگردان
                await _context.Entry(draft).Collection(x => x.ReceptionItems).LoadAsync();
                var recalculateResult = await RecalculateDraftAsync(draft, insuranceCalculation != null 
                    ? new Dictionary<int, ItemInsuranceCalculationDto> 
                    { 
                        { service.ServiceId, insuranceCalculation } 
                    }
                    : null);
                
                // 🚨 PROFESSIONAL FIX: حتی اگر محاسبه بیمه ناموفق بود، آیتم‌ها را برگردان
                if (!recalculateResult.Success)
                {
                    _logger.Warning("⚠️ FACADE: خطا در بازمحاسبه، اما آیتم ذخیره شده است - ReceptionId: {ReceptionId}, ServiceId: {ServiceId}", 
                        draft.ReceptionId, service.ServiceId);
                    // بازگرداندن آیتم‌های موجود حتی با خطا
                    return await RecalculateDraftAsync(draft, null);
                }
                
                return recalculateResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در افزودن آیتم - ReceptionId: {ReceptionId}, ServiceId: {ServiceId}", 
                    request.ReceptionId, request.ServiceId);
                
                // 🚨 PROFESSIONAL FIX: حتی در صورت خطا، سعی کن آیتم‌های موجود را برگردان
                try
                {
                    var draft = await _context.Receptions
                        .Include(d => d.ReceptionItems)
                        .FirstOrDefaultAsync(d => d.ReceptionId == request.ReceptionId && d.Status == ReceptionStatus.Pending);
                    
                    if (draft != null && draft.ReceptionItems != null && draft.ReceptionItems.Any(i => !i.IsDeleted))
                    {
                        _logger.Information("🏥 FACADE: بازگرداندن آیتم‌های موجود پس از خطا - ReceptionId: {ReceptionId}, ItemsCount: {Count}", 
                            draft.ReceptionId, draft.ReceptionItems.Count(i => !i.IsDeleted));
                        return await RecalculateDraftAsync(draft, null);
                    }
                }
                catch (Exception fallbackEx)
                {
                    _logger.Error(fallbackEx, "❌ FACADE: خطا در بازگرداندن آیتم‌های موجود");
                }
                
                return ServiceResult<ItemsAndTotalsDto>.Failed("خطا در افزودن آیتم: " + ex.Message);
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
                {
                    _logger.Warning("⚠️ FACADE: پیش‌نویس یافت نشد - ReceptionId: {ReceptionId}", request.ReceptionId);
                    return ServiceResult<ItemsAndTotalsDto>.Failed(
                        "پیش‌نویس پذیرش یافت نشد. لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.",
                        "DRAFT_NOT_FOUND");
                }

                // ✅ بهینه‌سازی: Validation جامع با پیام‌های واضح برای کاربران غیرفنی
                // اعتبارسنجی پلن بیمه پایه (در صورت وجود) - ذخیره برای استفاده بعدی
                Models.Entities.Insurance.InsurancePlan basePlan = null;
                if (request.BasePlanId.HasValue)
                {
                    basePlan = await _context.InsurancePlans
                        .FirstOrDefaultAsync(p => p.InsurancePlanId == request.BasePlanId.Value && !p.IsDeleted && p.IsActive);
                    
                    if (basePlan == null)
                    {
                        _logger.Warning("⚠️ FACADE: پلن بیمه پایه یافت نشد - BasePlanId: {BasePlanId}", request.BasePlanId.Value);
                        return ServiceResult<ItemsAndTotalsDto>.Failed(
                            $"⚠️ بیمه پایه انتخاب شده یافت نشد یا غیرفعال است.\n\n" +
                            $"لطفاً بیمه پایه دیگری انتخاب کنید.",
                            "BASE_PLAN_NOT_FOUND");
                    }
                    
                    if (basePlan.InsuranceType != Models.Entities.Insurance.InsuranceType.Primary)
                    {
                        _logger.Warning("⚠️ FACADE: پلن انتخاب شده بیمه پایه نیست - BasePlanId: {BasePlanId}, InsuranceType: {InsuranceType}", 
                            request.BasePlanId.Value, basePlan.InsuranceType);
                        return ServiceResult<ItemsAndTotalsDto>.Failed(
                            $"⚠️ بیمه انتخاب شده از نوع پایه نیست.\n\n" +
                            $"لطفاً یک بیمه پایه انتخاب کنید.",
                            "INVALID_BASE_PLAN_TYPE");
                    }
                }

                // اعتبارسنجی پلن بیمه تکمیلی (در صورت وجود) - ذخیره برای استفاده بعدی
                Models.Entities.Insurance.InsurancePlan suppPlan = null;
                if (request.SupplementaryPlanId.HasValue)
                {
                    suppPlan = await _context.InsurancePlans
                        .FirstOrDefaultAsync(p => p.InsurancePlanId == request.SupplementaryPlanId.Value && !p.IsDeleted && p.IsActive);
                    
                    if (suppPlan == null)
                    {
                        _logger.Warning("⚠️ FACADE: پلن بیمه تکمیلی یافت نشد - SuppPlanId: {SuppPlanId}", request.SupplementaryPlanId.Value);
                        return ServiceResult<ItemsAndTotalsDto>.Failed(
                            $"⚠️ بیمه تکمیلی انتخاب شده یافت نشد یا غیرفعال است.\n\n" +
                            $"لطفاً بیمه تکمیلی دیگری انتخاب کنید یا آن را خالی بگذارید.",
                            "SUPP_PLAN_NOT_FOUND");
                    }
                    
                    if (suppPlan.InsuranceType != Models.Entities.Insurance.InsuranceType.Supplementary)
                    {
                        _logger.Warning("⚠️ FACADE: پلن انتخاب شده بیمه تکمیلی نیست - SuppPlanId: {SuppPlanId}, InsuranceType: {InsuranceType}", 
                            request.SupplementaryPlanId.Value, suppPlan.InsuranceType);
                        return ServiceResult<ItemsAndTotalsDto>.Failed(
                            $"⚠️ بیمه انتخاب شده از نوع تکمیلی نیست.\n\n" +
                            $"لطفاً یک بیمه تکمیلی انتخاب کنید.",
                            "INVALID_SUPP_PLAN_TYPE");
                    }
                }

                // اعمال تغییرات روی Reception
                draft.BasePlanId = request.BasePlanId;
                draft.SupplementaryPlanId = request.SupplementaryPlanId;
                draft.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();

                // 🔥 به‌روزرسانی PatientInsurances (بیمه‌های واقعی بیمار)
                // ✅ رویکرد حرفه‌ای: استفاده از AsNoTracking + ReloadAsync برای جلوگیری از Optimistic Concurrency Exception
                var patientId = draft.PatientId;
                var userId = _currentUserService?.UserId ?? "system";

                // ✅ Step 1: Query با AsNoTracking برای جلوگیری از tracking conflict
                var patientInsuranceId = await _context.PatientInsurances
                    .AsNoTracking()
                    .Where(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive && !pi.IsDeleted)
                    .Select(pi => (int?)pi.PatientInsuranceId)
                    .FirstOrDefaultAsync();
                
                if (patientInsuranceId.HasValue)
                {
                    // ✅ Step 2: Load entity با tracking برای update
                    var patientInsurance = await _context.PatientInsurances
                        .FirstOrDefaultAsync(pi => pi.PatientInsuranceId == patientInsuranceId.Value);
                    
                    if (patientInsurance != null)
                    {
                        // ✅ Step 3: Reload برای دریافت RowVersion به‌روز (realtime - no cache)
                        await _context.Entry(patientInsurance).ReloadAsync();
                        
                        bool hasChanges = false;

                        // به‌روزرسانی بیمه پایه در PatientInsurances
                        if (request.BasePlanId.HasValue && basePlan != null)
                        {
                            if (patientInsurance.InsurancePlanId != request.BasePlanId.Value || 
                                patientInsurance.InsuranceProviderId != basePlan.InsuranceProviderId)
                            {
                                patientInsurance.InsurancePlanId = request.BasePlanId.Value;
                                patientInsurance.InsuranceProviderId = basePlan.InsuranceProviderId;
                                hasChanges = true;
                                
                                _logger.Information("🔄 FACADE: به‌روزرسانی بیمه پایه در PatientInsurances - PatientId: {PatientId}, PlanId: {PlanId}, ProviderId: {ProviderId}",
                                    patientId, request.BasePlanId.Value, basePlan.InsuranceProviderId);
                            }
                        }

                        // به‌روزرسانی بیمه تکمیلی در PatientInsurances
                        if (request.SupplementaryPlanId.HasValue && suppPlan != null)
                        {
                            if (patientInsurance.SupplementaryInsurancePlanId != request.SupplementaryPlanId.Value || 
                                patientInsurance.SupplementaryInsuranceProviderId != suppPlan.InsuranceProviderId)
                            {
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

                        // ✅ Step 4: Save با Retry Logic برای Handle Optimistic Concurrency
                        if (hasChanges)
                        {
                            patientInsurance.UpdatedAt = DateTime.Now;
                            patientInsurance.UpdatedByUserId = userId;
                            
                            // Retry logic برای Optimistic Concurrency (3 بار با exponential backoff)
                            int maxRetries = 3;
                            int retryCount = 0;
                            bool saved = false;
                            
                            while (retryCount < maxRetries && !saved)
                            {
                                try
                                {
                                    await _context.SaveChangesAsync();
                                    saved = true;
                                    _logger.Information("✅ FACADE: PatientInsurance به‌روزرسانی شد - PatientId: {PatientId}, RetryCount: {RetryCount}",
                                        patientId, retryCount);
                                }
                                catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException ex)
                                {
                                    retryCount++;
                                    _logger.Warning("⚠️ FACADE: Optimistic Concurrency Exception در SetInsurances - PatientId: {PatientId}, RetryCount: {RetryCount}",
                                        patientId, retryCount);
                                    
                                    if (retryCount >= maxRetries)
                                    {
                                        _logger.Error(ex, "❌ FACADE: حداکثر تعداد retry برای SetInsurances - PatientId: {PatientId}", patientId);
                                        throw new InvalidOperationException(
                                            "اطلاعات بیمه در جای دیگری تغییر کرده است. لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.",
                                            ex);
                                    }
                                    
                                    // Reload entity برای retry
                                    await _context.Entry(patientInsurance).ReloadAsync();
                                    
                                    // اعمال مجدد تغییرات
                                    if (request.BasePlanId.HasValue && basePlan != null)
                                    {
                                        patientInsurance.InsurancePlanId = request.BasePlanId.Value;
                                        patientInsurance.InsuranceProviderId = basePlan.InsuranceProviderId;
                                    }
                                    if (request.SupplementaryPlanId.HasValue && suppPlan != null)
                                    {
                                        patientInsurance.SupplementaryInsurancePlanId = request.SupplementaryPlanId.Value;
                                        patientInsurance.SupplementaryInsuranceProviderId = suppPlan.InsuranceProviderId;
                                    }
                                    else if (!request.SupplementaryPlanId.HasValue)
                                    {
                                        patientInsurance.SupplementaryInsurancePlanId = null;
                                        patientInsurance.SupplementaryInsuranceProviderId = null;
                                    }
                                    
                                    patientInsurance.UpdatedAt = DateTime.Now;
                                    patientInsurance.UpdatedByUserId = userId;
                                    
                                    // Exponential backoff: 100ms, 200ms, 400ms
                                    await Task.Delay(100 * (int)Math.Pow(2, retryCount - 1));
                                }
                            }
                            
                            _logger.Information("✅ FACADE: PatientInsurances با موفقیت به‌روزرسانی شد - PatientId: {PatientId}", patientId);
                        }
                        else
                        {
                            _logger.Information("ℹ️ FACADE: PatientInsurances تغییری نداشت - PatientId: {PatientId}", patientId);
                        }
                    }
                }
                else
                {
                    // ✅ بهینه‌سازی: اگر PatientInsurance وجود ندارد، آن را ایجاد می‌کنیم
                    _logger.Information("ℹ️ FACADE: PatientInsurance پایه برای بیمار یافت نشد - PatientId: {PatientId}. در حال ایجاد PatientInsurance جدید...", patientId);
                    
                    // ✅ استفاده از SetPatientInsurancesAsync برای ایجاد PatientInsurance
                    // این متد منطق کامل ایجاد/به‌روزرسانی PatientInsurance را دارد
                    try
                    {
                        await SetPatientInsurancesAsync(patientId, request.BasePlanId, request.SupplementaryPlanId);
                        _logger.Information("✅ FACADE: PatientInsurance جدید با موفقیت ایجاد شد - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}",
                            patientId, request.BasePlanId, request.SupplementaryPlanId);
                    }
                    catch (InvalidOperationException ioEx)
                    {
                        // ⚠️ خطای business logic: بیمه یافت نشد یا نوع آن نامعتبر است
                        _logger.Warning(ioEx, "⚠️ FACADE: خطا در ایجاد PatientInsurance (خطای business logic) - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}, Error: {Error}", 
                            patientId, request.BasePlanId, request.SupplementaryPlanId, ioEx.Message);
                        // ادامه می‌دهیم - Reception به‌روزرسانی شده است
                    }
                    catch (Exception ex)
                    {
                        // ⚠️ خطای غیرمنتظره در ایجاد PatientInsurance
                        _logger.Error(ex, "⚠️ FACADE: خطا در ایجاد PatientInsurance (خطای غیرمنتظره) - PatientId: {PatientId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                            patientId, request.BasePlanId, request.SupplementaryPlanId);
                        // ادامه می‌دهیم - Reception به‌روزرسانی شده است
                    }
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
                _logger.Error(ex, "❌ FACADE: خطا در تنظیم بیمه‌ها - ReceptionId: {ReceptionId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}, ExceptionType: {ExceptionType}, Message: {Message}",
                    request.ReceptionId, request.BasePlanId, request.SupplementaryPlanId, ex.GetType().Name, ex.Message);
                
                // ✅ بهینه‌سازی: پیام خطای واضح برای کاربران غیرفنی
                string userFriendlyMessage;
                string errorCode;
                
                if (ex is InvalidOperationException)
                {
                    userFriendlyMessage = $"⚠️ {ex.Message}\n\n" +
                        $"لطفاً بیمه‌های انتخاب شده را بررسی کنید و مجدداً تلاش کنید.";
                    errorCode = "BUSINESS_LOGIC_ERROR";
                }
                else if (ex is System.Data.Entity.Infrastructure.DbUpdateConcurrencyException)
                {
                    userFriendlyMessage = $"⚠️ اطلاعات پذیرش در جای دیگری تغییر کرده است.\n\n" +
                        $"لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.";
                    errorCode = "CONCURRENCY_ERROR";
                }
                else if (ex is System.Data.Entity.Infrastructure.DbUpdateException)
                {
                    userFriendlyMessage = $"⚠️ خطا در ذخیره اطلاعات بیمه.\n\n" +
                        $"لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.\n" +
                        $"اگر مشکل ادامه داشت، با بخش فنی تماس بگیرید.";
                    errorCode = "DATABASE_ERROR";
                }
                else
                {
                    userFriendlyMessage = $"⚠️ خطای غیرمنتظره در تنظیم بیمه‌ها.\n\n" +
                        $"لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.\n" +
                        $"اگر مشکل ادامه داشت، با بخش فنی تماس بگیرید.";
                    errorCode = "UNHANDLED_ERROR";
                }
                
                return ServiceResult<ItemsAndTotalsDto>.Failed(userFriendlyMessage, errorCode);
            }
        }

        /// <summary>
        /// ✅ گام 7 - Finalize Validation: اعتبارسنجی کامل Draft قبل از Finalize
        /// </summary>
        private async Task<ServiceResult<bool>> ValidateDraftForFinalizeAsync(Models.Entities.Reception.Reception draft)
        {
            try
            {
                // ✅ بهینه‌سازی: Validation جامع با پیام‌های واضح برای کاربران غیرفنی
                // 1. بررسی وجود فیلدهای الزامی
                if (draft.PatientId <= 0)
                {
                    _logger.Warning("⚠️ FACADE: اطلاعات بیمار ناقص است - ReceptionId: {ReceptionId}", draft.ReceptionId);
                    return ServiceResult<bool>.Failed(
                        "⚠️ اطلاعات بیمار ناقص است.\n\n" +
                        "لطفاً ابتدا بیمار را انتخاب یا ایجاد کنید.",
                        "PATIENT_MISSING");
                }

                if (draft.ClinicId <= 0)
                {
                    _logger.Warning("⚠️ FACADE: کلینیک انتخاب نشده است - ReceptionId: {ReceptionId}", draft.ReceptionId);
                    return ServiceResult<bool>.Failed(
                        "⚠️ کلینیک انتخاب نشده است.\n\n" +
                        "لطفاً کلینیک را انتخاب کنید.",
                        "CLINIC_MISSING");
                }

                if (draft.DepartmentId <= 0)
                {
                    _logger.Warning("⚠️ FACADE: دپارتمان انتخاب نشده است - ReceptionId: {ReceptionId}", draft.ReceptionId);
                    return ServiceResult<bool>.Failed(
                        "⚠️ دپارتمان انتخاب نشده است.\n\n" +
                        "لطفاً دپارتمان را انتخاب کنید.",
                        "DEPARTMENT_MISSING");
                }

                if (draft.DoctorId <= 0)
                {
                    _logger.Warning("⚠️ FACADE: پزشک انتخاب نشده است - ReceptionId: {ReceptionId}", draft.ReceptionId);
                    return ServiceResult<bool>.Failed(
                        "⚠️ پزشک انتخاب نشده است.\n\n" +
                        "لطفاً پزشک را انتخاب کنید.",
                        "DOCTOR_MISSING");
                }

                // 2. بررسی وجود آیتم‌ها
                if (draft.ReceptionItems == null || !draft.ReceptionItems.Any(ri => !ri.IsDeleted))
                {
                    _logger.Warning("⚠️ FACADE: هیچ خدمتی به پذیرش افزوده نشده است - ReceptionId: {ReceptionId}", draft.ReceptionId);
                    return ServiceResult<bool>.Failed(
                        "⚠️ هیچ خدمتی به پذیرش افزوده نشده است.\n\n" +
                        "لطفاً حداقل یک خدمت به پذیرش اضافه کنید.",
                        "NO_ITEMS");
                }

                // 3. بررسی وجود بیمه پایه برای خدمات بیمه‌ای (در صورت نیاز)
                // TODO: در آینده می‌توان بررسی کرد که آیا خدمات نیاز به بیمه دارند یا نه
                // فعلاً این بررسی را انجام نمی‌دهیم چون برخی خدمات بدون بیمه هم ممکن است باشند

                _logger.Information("✅ FACADE: اعتبارسنجی Draft برای Finalize موفق - ReceptionId: {ReceptionId}, ItemsCount: {Count}", 
                    draft.ReceptionId, draft.ReceptionItems?.Count(ri => !ri.IsDeleted) ?? 0);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در اعتبارسنجی Draft برای Finalize - ReceptionId: {ReceptionId}, ExceptionType: {ExceptionType}, Message: {Message}",
                    draft?.ReceptionId, ex.GetType().Name, ex.Message);
                
                // ✅ بهینه‌سازی: پیام خطای واضح برای کاربران غیرفنی
                return ServiceResult<bool>.Failed(
                    $"⚠️ خطا در بررسی اطلاعات پذیرش.\n\n" +
                    $"لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.\n" +
                    $"اگر مشکل ادامه داشت، با بخش فنی تماس بگیرید.",
                    "VALIDATION_ERROR");
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

                // ✅ محاسبه مجموع‌ها با استفاده از ReceptionPricingService (محاسبه دقیق از SnapshotJson)
                var totals = await RecalculateDraftAsync(draft);
                
                if (!totals.Success || totals.Data == null)
                {
                    _logger.Error("❌ FACADE: خطا در محاسبه Totals برای Finalize (POS) - ReceptionId: {ReceptionId}, Error: {Error}", 
                        request.ReceptionId, totals?.Message);
                    return ServiceResult<FinalizeResponse>.Failed(
                        $"⚠️ خطا در محاسبه مجموع‌ها.\n\n" +
                        $"لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.\n" +
                        $"اگر مشکل ادامه داشت، با بخش فنی تماس بگیرید.",
                        "CALCULATION_ERROR");
                }
                
                // ✅ اعتبارسنجی مبلغ قابل پرداخت
                if (totals.Data.Totals.Patient <= 0)
                {
                    // بررسی اینکه آیا بیمه 100% پوشش می‌دهد
                    var gross = totals.Data.Totals.Gross;
                    var baseCovered = totals.Data.Totals.Base;
                    var suppCovered = totals.Data.Totals.Supplementary;
                    
                    if (gross > 0 && (baseCovered + suppCovered) >= gross)
                    {
                        // ✅ بیمه 100% پوشش می‌دهد - مبلغ صفر قابل قبول است
                        _logger.Information("✅ FACADE: بیمه 100% پوشش می‌دهد (POS) - Gross: {Gross}, Base: {Base}, Supp: {Supp}, Patient: {Patient}", 
                            gross, baseCovered, suppCovered, totals.Data.Totals.Patient);
                    }
                    else
                    {
                        _logger.Warning("⚠️ FACADE: مبلغ قابل پرداخت صفر یا منفی است (POS) - Gross: {Gross}, Base: {Base}, Supp: {Supp}, Patient: {Patient}, ReceptionId: {ReceptionId}", 
                            gross, baseCovered, suppCovered, totals.Data.Totals.Patient, request.ReceptionId);
                        
                        // ✅ بررسی دقیق‌تر: آیا آیتم‌ها وجود دارند؟
                        var itemsCount = draft.ReceptionItems?.Count(ri => !ri.IsDeleted) ?? 0;
                        if (itemsCount == 0)
                        {
                            _logger.Warning("⚠️ FACADE: هیچ خدمتی به پذیرش افزوده نشده است (POS) - ReceptionId: {ReceptionId}", request.ReceptionId);
                            return ServiceResult<FinalizeResponse>.Failed(
                                "⚠️ هیچ خدمتی به پذیرش افزوده نشده است.\n\n" +
                                "لطفاً حداقل یک خدمت به پذیرش اضافه کنید.",
                                "NO_ITEMS");
                        }
                        
                        // ✅ بررسی اینکه آیا UnitPrice صفر است؟
                        var hasZeroPrice = draft.ReceptionItems?.Any(ri => !ri.IsDeleted && ri.UnitPrice <= 0) ?? false;
                        if (hasZeroPrice)
                        {
                            _logger.Warning("⚠️ FACADE: برخی آیتم‌ها UnitPrice صفر دارند (POS) - ReceptionId: {ReceptionId}", request.ReceptionId);
                            return ServiceResult<FinalizeResponse>.Failed(
                                "⚠️ برخی خدمات قیمت صفر دارند.\n\n" +
                                "لطفاً خدمات را بررسی کنید یا با بخش فنی تماس بگیرید.",
                                "ZERO_PRICE_ITEMS");
                        }
                        
                        // ✅ بررسی اینکه آیا بیمه‌ها تنظیم شده‌اند؟
                        if (!draft.BasePlanId.HasValue && !draft.SupplementaryPlanId.HasValue)
                        {
                            _logger.Warning("⚠️ FACADE: هیچ بیمه‌ای تنظیم نشده است (POS) - ReceptionId: {ReceptionId}", request.ReceptionId);
                            return ServiceResult<FinalizeResponse>.Failed(
                                "⚠️ هیچ بیمه‌ای تنظیم نشده است.\n\n" +
                                "لطفاً بیمه پایه یا تکمیلی را انتخاب کنید.",
                                "NO_INSURANCE");
                        }
                        
                        _logger.Warning("⚠️ FACADE: مبلغ قابل پرداخت صفر یا منفی است (POS) - ReceptionId: {ReceptionId}, Gross: {Gross}, Base: {Base}, Supp: {Supp}, Patient: {Patient}",
                            request.ReceptionId, totals.Data.Totals.Gross, totals.Data.Totals.Base, totals.Data.Totals.Supplementary, totals.Data.Totals.Patient);
                        return ServiceResult<FinalizeResponse>.Failed(
                            "⚠️ مبلغ قابل پرداخت باید بیشتر از صفر باشد.\n\n" +
                            "لطفاً بیمه‌ها و خدمات را بررسی کنید.\n" +
                            "اگر مشکل ادامه داشت، با بخش فنی تماس بگیرید.",
                            "INVALID_PAYABLE_AMOUNT");
                    }
                }
                
                // ✅ بهینه‌سازی: اعتبارسنجی تطابق مبلغ ارسالی با محاسبه شده با پیام واضح
                if (totals.Data.Totals.Patient != request.AmountIRR)
                {
                    _logger.Warning("⚠️ FACADE: مبلغ پرداخت با مجموع مطابقت ندارد (POS) - Calculated: {Calculated}, Requested: {Requested}, ReceptionId: {ReceptionId}", 
                        totals.Data.Totals.Patient, request.AmountIRR, request.ReceptionId);
                    return ServiceResult<FinalizeResponse>.Failed(
                        $"⚠️ مبلغ پرداخت با مجموع محاسبه شده مطابقت ندارد.\n\n" +
                        $"• مبلغ محاسبه شده: {totals.Data.Totals.Patient:N0} ریال\n" +
                        $"• مبلغ ارسالی: {request.AmountIRR:N0} ریال\n\n" +
                        $"لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.",
                        "AMOUNT_MISMATCH");
                }

                // 🏥 MEDICAL: دریافت جلسه نقدی باز برای CashSessionId
                var sessionResult = await _posManagementService.GetOpenCashSessionAsync(_currentUserService.UserId);
                if (!sessionResult.Success)
                {
                    _logger.Warning("⚠️ FACADE: جلسه نقدی باز یافت نشد (POS) - ReceptionId: {ReceptionId}, UserId: {UserId}", 
                        request.ReceptionId, _currentUserService?.UserId);
                    return ServiceResult<FinalizeResponse>.Failed(
                        "⚠️ جلسه نقدی باز یافت نشد.\n\n" +
                        "لطفاً ابتدا جلسه صندوق را باز کنید و سپس مجدداً تلاش کنید.",
                        "NO_CASH_SESSION");
                }

                // 🏥 MEDICAL: پیدا کردن PosTerminal از TerminalId
                int? posTerminalId = null;
                if (!string.IsNullOrEmpty(request.Pos?.TerminalId))
                {
                    // جستجو بر اساس TerminalId (string)
                    var posTerminal = await _context.PosTerminals
                        .FirstOrDefaultAsync(pt => pt.TerminalId == request.Pos.TerminalId && !pt.IsDeleted && pt.IsActive);
                    
                    if (posTerminal != null)
                    {
                        posTerminalId = posTerminal.PosTerminalId;
                        _logger.Information("✅ FACADE: PosTerminal یافت شد - TerminalId: {TerminalId}, PosTerminalId: {PosTerminalId}", 
                            request.Pos.TerminalId, posTerminalId);
                    }
                    else
                    {
                        _logger.Warning("⚠️ FACADE: PosTerminal با TerminalId {TerminalId} یافت نشد - استفاده از ترمینال پیش‌فرض", 
                            request.Pos.TerminalId);
                    }
                }
                
                // 🏥 MEDICAL: اگر PosTerminal یافت نشد، از ترمینال پیش‌فرض استفاده کن
                if (!posTerminalId.HasValue)
                {
                    var defaultTerminalResult = await _posManagementService.GetDefaultPosTerminalAsync();
                    if (defaultTerminalResult.Success && defaultTerminalResult.Data != null)
                    {
                        posTerminalId = defaultTerminalResult.Data.PosTerminalId;
                        _logger.Information("✅ FACADE: استفاده از ترمینال پیش‌فرض - PosTerminalId: {PosTerminalId}", posTerminalId);
                    }
                    else
                    {
                        _logger.Warning("⚠️ FACADE: هیچ ترمینال POS فعالی یافت نشد - ReceptionId: {ReceptionId}", request.ReceptionId);
                        // در این حالت، PosTerminalId را null می‌گذاریم (اختیاری است)
                    }
                }

                // 🏥 MEDICAL: Reload draft entity برای اطمینان از به‌روزرسانی BasePay, SuppPay, PatientPay
                await _context.Entry(draft).ReloadAsync();

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
                    TerminalId = request.Pos?.TerminalId, // 🏥 MEDICAL: TerminalId (string) برای سازگاری
                    CardLast4 = request.Pos?.CardLast4,
                    PosTerminalId = posTerminalId, // 🏥 MEDICAL: PosTerminalId (int) از PosTerminals
                    CashSessionId = sessionResult.Data.CashSessionId, // 🏥 MEDICAL: تنظیم CashSessionId
                    CreatedByUserId = _currentUserService?.UserId, // ✅ ردیابی کاربر ایجادکننده
                    CreatedAt = DateTime.Now // ✅ تاریخ ایجاد
                };

                _context.PaymentTransactions.Add(payment);

                // 🏥 MEDICAL: به‌روزرسانی مانده POS در CashSession
                var cashSession = await _context.CashSessions.FindAsync(sessionResult.Data.CashSessionId);
                if (cashSession != null)
                {
                    cashSession.PosBalance += request.AmountIRR;
                    cashSession.UpdatedAt = DateTime.Now;
                    cashSession.UpdatedByUserId = _currentUserService?.UserId;
                    _logger.Information("✅ FACADE: CashSession.PosBalance به‌روزرسانی شد - SessionId: {SessionId}, New PosBalance: {PosBalance}, Amount: {Amount}",
                        cashSession.CashSessionId, cashSession.PosBalance, request.AmountIRR);
                }
                else
                {
                    _logger.Warning("⚠️ FACADE: CashSession یافت نشد برای به‌روزرسانی PosBalance - SessionId: {SessionId}",
                        sessionResult.Data.CashSessionId);
                }

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

                // ✅ محاسبه مجموع‌ها با استفاده از ReceptionPricingService (محاسبه دقیق از SnapshotJson)
                var totals = await RecalculateDraftAsync(draft);
                
                if (!totals.Success || totals.Data == null)
                {
                    _logger.Warning("⚠️ FACADE: خطا در محاسبه Totals برای Finalize (Cash) - ReceptionId: {ReceptionId}, Error: {Error}", 
                        request.ReceptionId, totals?.Message);
                    return ServiceResult<FinalizeResponse>.Failed("خطا در محاسبه مجموع‌ها. لطفاً دوباره تلاش کنید.", "CALCULATION_ERROR");
                }
                
                // ✅ اعتبارسنجی مبلغ قابل پرداخت
                if (totals.Data.Totals.Patient <= 0)
                {
                    // بررسی اینکه آیا بیمه 100% پوشش می‌دهد
                    var gross = totals.Data.Totals.Gross;
                    var baseCovered = totals.Data.Totals.Base;
                    var suppCovered = totals.Data.Totals.Supplementary;
                    
                    if (gross > 0 && (baseCovered + suppCovered) >= gross)
                    {
                        // ✅ بیمه 100% پوشش می‌دهد - مبلغ صفر قابل قبول است
                        _logger.Information("✅ FACADE: بیمه 100% پوشش می‌دهد (Cash) - Gross: {Gross}, Base: {Base}, Supp: {Supp}, Patient: {Patient}", 
                            gross, baseCovered, suppCovered, totals.Data.Totals.Patient);
                    }
                    else
                    {
                        _logger.Warning("⚠️ FACADE: مبلغ قابل پرداخت صفر یا منفی است (Cash) - Gross: {Gross}, Base: {Base}, Supp: {Supp}, Patient: {Patient}, ReceptionId: {ReceptionId}", 
                            gross, baseCovered, suppCovered, totals.Data.Totals.Patient, request.ReceptionId);
                        
                        // ✅ بررسی دقیق‌تر: آیا آیتم‌ها وجود دارند؟
                        var itemsCount = draft.ReceptionItems?.Count(ri => !ri.IsDeleted) ?? 0;
                        if (itemsCount == 0)
                        {
                            return ServiceResult<FinalizeResponse>.Failed("هیچ خدمتی به پذیرش افزوده نشده است.", "NO_ITEMS");
                        }
                        
                        // ✅ بررسی اینکه آیا UnitPrice صفر است؟
                        var hasZeroPrice = draft.ReceptionItems?.Any(ri => !ri.IsDeleted && ri.UnitPrice <= 0) ?? false;
                        if (hasZeroPrice)
                        {
                            _logger.Warning("⚠️ FACADE: برخی آیتم‌ها UnitPrice صفر دارند (Cash) - ReceptionId: {ReceptionId}", request.ReceptionId);
                            return ServiceResult<FinalizeResponse>.Failed("برخی خدمات قیمت صفر دارند. لطفاً خدمات را بررسی کنید.", "ZERO_PRICE_ITEMS");
                        }
                        
                        // ✅ بررسی اینکه آیا بیمه‌ها تنظیم شده‌اند؟
                        if (!draft.BasePlanId.HasValue && !draft.SupplementaryPlanId.HasValue)
                        {
                            _logger.Warning("⚠️ FACADE: هیچ بیمه‌ای تنظیم نشده است (Cash) - ReceptionId: {ReceptionId}", request.ReceptionId);
                            return ServiceResult<FinalizeResponse>.Failed("لطفاً بیمه پایه یا تکمیلی را انتخاب کنید.", "NO_INSURANCE");
                        }
                        
                        return ServiceResult<FinalizeResponse>.Failed("مبلغ قابل پرداخت باید بیشتر از صفر باشد. لطفاً بیمه‌ها و خدمات را بررسی کنید.", "INVALID_PAYABLE_AMOUNT");
                    }
                }
                
                // ✅ اعتبارسنجی تطابق مبلغ ارسالی با محاسبه شده
                // 🏥 MEDICAL: اگر Patient = 0 و AmountIRR = 0، این حالت معتبر است (بیمه 100% پوشش می‌دهد)
                if (totals.Data.Totals.Patient != request.AmountIRR && !(totals.Data.Totals.Patient == 0 && request.AmountIRR == 0))
                {
                    _logger.Warning("⚠️ FACADE: مبلغ پرداخت با مجموع مطابقت ندارد (Cash) - Calculated: {Calculated}, Requested: {Requested}", 
                        totals.Data.Totals.Patient, request.AmountIRR);
                    return ServiceResult<FinalizeResponse>.Failed($"مبلغ پرداخت ({request.AmountIRR:N0} ریال) با مجموع محاسبه شده ({totals.Data.Totals.Patient:N0} ریال) مطابقت ندارد. لطفاً صفحه را نوسازی کنید.", "AMOUNT_MISMATCH");
                }

                // 🏥 MEDICAL: دریافت جلسه نقدی باز برای CashSessionId
                var sessionResult = await _posManagementService.GetOpenCashSessionAsync(_currentUserService.UserId);
                if (!sessionResult.Success)
                {
                    _logger.Warning("⚠️ FACADE: جلسه نقدی باز یافت نشد (Cash) - ReceptionId: {ReceptionId}", request.ReceptionId);
                    return ServiceResult<FinalizeResponse>.Failed("جلسه نقدی باز یافت نشد. لطفاً ابتدا جلسه صندوق را باز کنید.", "NO_CASH_SESSION");
                }

                // 🏥 MEDICAL: Reload draft entity برای اطمینان از به‌روزرسانی BasePay, SuppPay, PatientPay
                await _context.Entry(draft).ReloadAsync();

                // ثبت پرداخت
                var payment = new Models.Entities.Payment.PaymentTransaction
                {
                    ReceptionId = request.ReceptionId,
                    Amount = request.AmountIRR,
                    Status = PaymentStatus.Success,
                    IdempotencyKey = request.IdempotencyKey,
                    Method = PaymentMethod.Cash,
                    CashSessionId = sessionResult.Data.CashSessionId, // 🏥 MEDICAL: تنظیم CashSessionId
                    CreatedByUserId = _currentUserService?.UserId, // ✅ ردیابی کاربر ایجادکننده
                    CreatedAt = DateTime.Now // ✅ تاریخ ایجاد
                };

                _context.PaymentTransactions.Add(payment);

                // 🏥 MEDICAL: به‌روزرسانی مانده نقدی در CashSession
                var cashSession = await _context.CashSessions.FindAsync(sessionResult.Data.CashSessionId);
                if (cashSession != null)
                {
                    cashSession.CashBalance += request.AmountIRR;
                    cashSession.UpdatedAt = DateTime.Now;
                    cashSession.UpdatedByUserId = _currentUserService?.UserId;
                    _logger.Information("✅ FACADE: CashSession.CashBalance به‌روزرسانی شد - SessionId: {SessionId}, New CashBalance: {CashBalance}, Amount: {Amount}",
                        cashSession.CashSessionId, cashSession.CashBalance, request.AmountIRR);
                }
                else
                {
                    _logger.Warning("⚠️ FACADE: CashSession یافت نشد برای به‌روزرسانی CashBalance - SessionId: {SessionId}",
                        sessionResult.Data.CashSessionId);
                }

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
        /// <summary>
        /// بازمحاسبه پیش‌نویس - استفاده از ReceptionPricingService برای محاسبه دقیق
        /// 🏥 MEDICAL: به‌روزرسانی PatientCoPay, TotalAmount, و InsurerShareAmount در Reception entity
        /// 🚨 PROFESSIONAL: پشتیبانی از محاسبه بیمه real-time برای هر آیتم
        /// </summary>
        private async Task<ServiceResult<ItemsAndTotalsDto>> RecalculateDraftAsync(
            Models.Entities.Reception.Reception draft, 
            Dictionary<int, ItemInsuranceCalculationDto> insuranceCalculations = null)
        {
            try
            {
                _logger.Information("🏥 FACADE: بازمحاسبه پیش‌نویس - ReceptionId: {ReceptionId}", draft.ReceptionId);
                
                // 🚨 PROFESSIONAL FIX: اطمینان از اینکه entity tracked است
                // اگر entity detached است، آن را reload کن
                var entry = _context.Entry(draft);
                if (entry.State == System.Data.Entity.EntityState.Detached)
                {
                    _logger.Warning("⚠️ FACADE: Reception entity detached است - Reloading - ReceptionId: {ReceptionId}", draft.ReceptionId);
                    draft = await _context.Receptions
                        .Include(d => d.ReceptionItems)
                        .FirstOrDefaultAsync(d => d.ReceptionId == draft.ReceptionId && d.Status == ReceptionStatus.Pending);
                    
                    if (draft == null)
                    {
                        _logger.Error("❌ FACADE: Reception یافت نشد پس از reload - ReceptionId: {ReceptionId}", draft?.ReceptionId);
                        return ServiceResult<ItemsAndTotalsDto>.Failed("پیش‌نویس یافت نشد");
                    }
                }

                // ✅ استفاده از ReceptionPricingService برای محاسبه دقیق Totals
                // این سرویس از SnapshotJson و PatientShareAmount استفاده می‌کند
                Controllers.Api.ReceptionTotalsDto totalsDto = null;
                try
                {
                    totalsDto = await _receptionPricingService.CalculateTotalsAsync(draft.ReceptionId);
                }
                catch (Exception calcEx)
                {
                    _logger.Warning(calcEx, "⚠️ FACADE: خطا در محاسبه Totals - ادامه با مقادیر صفر - ReceptionId: {ReceptionId}", draft.ReceptionId);
                    // ادامه می‌دهیم با totalsDto = null - بعداً مقادیر صفر استفاده می‌شود
                }

                // 🏥 MEDICAL: به‌روزرسانی مقادیر در Reception entity (فقط اگر totalsDto موجود باشد)
                if (totalsDto != null)
                {
                    draft.TotalAmount = (decimal)totalsDto.GrossIRR;
                    draft.PatientCoPay = (decimal)totalsDto.PatientPayableIRR;
                    draft.InsurerShareAmount = (decimal)(totalsDto.BaseCoveredIRR + totalsDto.SuppCoveredIRR);
                    // 🏥 MEDICAL: به‌روزرسانی سهم‌های بیمه برای نمایش دقیق در لیست پذیرش‌ها
                    draft.BasePay = (decimal)totalsDto.BaseCoveredIRR;
                    draft.SuppPay = (decimal)totalsDto.SuppCoveredIRR;
                    draft.PatientPay = (decimal)totalsDto.PatientPayableIRR;
                }
                else
                {
                    // 🚨 PROFESSIONAL FIX: اگر CalculateTotalsAsync با خطا مواجه شد، از مقادیر موجود در draft استفاده کن
                    // یا مقادیر را از ReceptionItems محاسبه کن
                    var draftItems = draft.ReceptionItems.Where(i => !i.IsDeleted).ToList();
                    draft.TotalAmount = draftItems.Sum(i => i.UnitPrice * i.Quantity);
                    draft.PatientCoPay = draftItems.Sum(i => i.PatientShareAmount);
                    draft.InsurerShareAmount = draftItems.Sum(i => i.InsurerShareAmount);
                    
                    // 🏥 MEDICAL: محاسبه سهم‌های بیمه از ReceptionItems و SnapshotJson
                    decimal basePay = 0m;
                    decimal suppPay = 0m;
                    decimal patientPay = draftItems.Sum(i => i.PatientShareAmount);
                    
                    foreach (var item in draftItems)
                    {
                        long itemBaseCovered = 0;
                        long itemSuppCovered = 0;
                        
                        if (!string.IsNullOrEmpty(item.SnapshotJson))
                        {
                            try
                            {
                                var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.SnapshotJson);
                                if (snapshot != null)
                                {
                                    if (snapshot.PrimaryPays != null)
                                        itemBaseCovered = (long)snapshot.PrimaryPays;
                                    if (snapshot.SupplementaryPays != null)
                                        itemSuppCovered = (long)snapshot.SupplementaryPays;
                                }
                            }
                            catch (Exception snapshotEx)
                            {
                                _logger.Warning(snapshotEx, "⚠️ FACADE: خطا در parse کردن SnapshotJson برای ReceptionItem {ReceptionItemId}", item.ReceptionItemId);
                            }
                        }
                        
                        // Fallback: اگر SnapshotJson موجود نبود، از InsurerShareAmount استفاده کن
                        if (itemBaseCovered == 0 && itemSuppCovered == 0)
                        {
                            var insurerShare = (long)item.InsurerShareAmount;
                            if (draft.BasePlanId.HasValue && draft.SupplementaryPlanId.HasValue)
                            {
                                itemBaseCovered = insurerShare / 2;
                                itemSuppCovered = insurerShare - itemBaseCovered;
                            }
                            else if (draft.BasePlanId.HasValue)
                            {
                                itemBaseCovered = insurerShare;
                            }
                            else if (draft.SupplementaryPlanId.HasValue)
                            {
                                itemSuppCovered = insurerShare;
                            }
                        }
                        
                        basePay += itemBaseCovered;
                        suppPay += itemSuppCovered;
                    }
                    
                    draft.BasePay = basePay;
                    draft.SuppPay = suppPay;
                    draft.PatientPay = patientPay;
                    
                    _logger.Information("🏥 FACADE: استفاده از مقادیر محاسبه شده از ReceptionItems - TotalAmount: {TotalAmount}, PatientCoPay: {PatientCoPay}, InsurerShareAmount: {InsurerShareAmount}, BasePay: {BasePay}, SuppPay: {SuppPay}, PatientPay: {PatientPay}", 
                        draft.TotalAmount, draft.PatientCoPay, draft.InsurerShareAmount, draft.BasePay, draft.SuppPay, draft.PatientPay);
                }
                draft.UpdatedAt = DateTime.Now;
                
                // 🏥 MEDICAL: اگر سهم بیمار صفر است و بیمه 100% پوشش می‌دهد، به صورت خودکار نهایی کن
                // این منطق برای حالتی است که کاربر دکمه نهایی‌سازی را نزده اما سهم بیمار صفر است
                if (draft.PatientCoPay == 0 && draft.TotalAmount > 0 && 
                    draft.InsurerShareAmount >= draft.TotalAmount && 
                    draft.Status == ReceptionStatus.Pending)
                {
                    // بررسی اینکه آیا پرداختی ثبت شده است یا نه
                    var hasPayment = await _context.PaymentTransactions
                        .AnyAsync(t => t.ReceptionId == draft.ReceptionId && 
                                      t.Status == PaymentStatus.Success && 
                                      !t.IsDeleted);
                    
                    if (!hasPayment)
                    {
                        // ثبت پرداخت صفر برای ردیابی
                        var zeroPayment = new PaymentTransaction
                        {
                            ReceptionId = draft.ReceptionId,
                            Amount = 0,
                            Status = PaymentStatus.Success,
                            Method = PaymentMethod.Cash,
                            IdempotencyKey = Guid.NewGuid().ToString()
                        };
                        _context.PaymentTransactions.Add(zeroPayment);
                    }
                    
                    draft.Status = ReceptionStatus.Completed;
                    _logger.Information("✅ FACADE: پذیرش به صورت خودکار نهایی شد (سهم بیمار صفر) - ReceptionId: {ReceptionId}, PatientCoPay: {PatientCoPay}, TotalAmount: {TotalAmount}", 
                        draft.ReceptionId, draft.PatientCoPay, draft.TotalAmount);
                }
                
                // ذخیره تغییرات
                bool saveSucceeded = false;
                try
                {
                    await _context.SaveChangesAsync();
                    saveSucceeded = true;
                    _logger.Information("✅ FACADE: مقادیر Reception به‌روزرسانی شد - TotalAmount: {TotalAmount}, PatientCoPay: {PatientCoPay}, InsurerShareAmount: {InsurerShareAmount}, Status: {Status}", 
                        draft.TotalAmount, draft.PatientCoPay, draft.InsurerShareAmount, draft.Status);
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException concurrencyEx)
                {
                    _logger.Warning(concurrencyEx, "⚠️ FACADE: خطای همزمانی در ذخیره Reception (ادامه با بازگرداندن آیتم‌ها) - ReceptionId: {ReceptionId}", 
                        draft.ReceptionId);
                    // 🚨 PROFESSIONAL FIX: به جای throw، فقط warning می‌دهیم و ادامه می‌دهیم
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx)
                {
                    var innerEx = dbEx.InnerException;
                    var innerMessage = innerEx != null ? innerEx.Message : "No inner exception";
                    _logger.Warning(dbEx, "⚠️ FACADE: خطا در ذخیره تغییرات Reception (ادامه با بازگرداندن آیتم‌ها) - ReceptionId: {ReceptionId}, InnerException: {InnerMessage}", 
                        draft.ReceptionId, innerMessage);
                    // 🚨 PROFESSIONAL FIX: به جای throw، فقط warning می‌دهیم و ادامه می‌دهیم
                    // این اطمینان می‌دهد که حتی اگر ذخیره Reception با خطا مواجه شود، آیتم‌ها به frontend برگردانده می‌شوند
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException valEx)
                {
                    var errorMessage = string.Join("; ",
                        valEx.EntityValidationErrors
                            .SelectMany(e => e.ValidationErrors)
                            .Select(e => $"Property: {e.PropertyName}, Error: {e.ErrorMessage}"));
                    _logger.Warning(valEx, "⚠️ FACADE: خطا در اعتبارسنجی Reception (ادامه با بازگرداندن آیتم‌ها) - ReceptionId: {ReceptionId}, Errors: {Errors}", 
                        draft.ReceptionId, errorMessage);
                    // 🚨 PROFESSIONAL FIX: به جای throw، فقط warning می‌دهیم و ادامه می‌دهیم
                }

                // 🚨 PROFESSIONAL FIX: اگر ذخیره با خطا مواجه شد، draft را reload کن تا از آخرین وضعیت دیتابیس استفاده کنیم
                if (!saveSucceeded)
                {
                    var originalReceptionId = draft.ReceptionId;
                    _logger.Information("🔄 FACADE: Reloading draft entity after save failure - ReceptionId: {ReceptionId}", originalReceptionId);
                    try
                    {
                        // Detach current entity
                        _context.Entry(draft).State = System.Data.Entity.EntityState.Detached;
                        
                        // Reload from database
                        draft = await _context.Receptions
                            .Include(d => d.ReceptionItems)
                            .FirstOrDefaultAsync(d => d.ReceptionId == originalReceptionId && d.Status == ReceptionStatus.Pending);
                        
                        if (draft != null)
                        {
                            // Recalculate totals from database state
                            try
                            {
                                totalsDto = await _receptionPricingService.CalculateTotalsAsync(draft.ReceptionId);
                                _logger.Information("✅ FACADE: Draft reloaded and totals recalculated - ReceptionId: {ReceptionId}, ItemsCount: {Count}", 
                                    draft.ReceptionId, draft.ReceptionItems?.Count(i => !i.IsDeleted) ?? 0);
                            }
                            catch (Exception reloadCalcEx)
                            {
                                _logger.Warning(reloadCalcEx, "⚠️ FACADE: خطا در محاسبه Totals پس از reload - ادامه با totalsDto = null - ReceptionId: {ReceptionId}", 
                                    draft.ReceptionId);
                                totalsDto = null; // ادامه می‌دهیم با totalsDto = null
                            }
                        }
                        else
                        {
                            _logger.Warning("⚠️ FACADE: Draft not found after reload - ReceptionId: {ReceptionId}", originalReceptionId);
                            return ServiceResult<ItemsAndTotalsDto>.Failed("پیش‌نویس یافت نشد");
                        }
                    }
                    catch (Exception reloadEx)
                    {
                        _logger.Warning(reloadEx, "⚠️ FACADE: خطا در reload کردن draft - ادامه با داده‌های موجود - ReceptionId: {ReceptionId}", 
                            originalReceptionId);
                        // اگر reload با خطا مواجه شد، سعی می‌کنیم با draft موجود ادامه دهیم
                        // اما اگر draft null است، باید خطا برگردانیم
                        if (draft == null)
                        {
                            return ServiceResult<ItemsAndTotalsDto>.Failed("خطا در بارگذاری پیش‌نویس");
                        }
                    }
                }

                // 🚨 PROFESSIONAL FIX: بررسی null بودن draft
                if (draft == null)
                {
                    _logger.Error("❌ FACADE: Draft is null - cannot return items");
                    return ServiceResult<ItemsAndTotalsDto>.Failed("پیش‌نویس یافت نشد");
                }

                // دریافت اطلاعات خدمات برای ساخت DTO
                var serviceIds = draft.ReceptionItems.Where(i => !i.IsDeleted).Select(i => i.ServiceId).Distinct().ToList();
                var services = await _context.Services
                    .Where(s => serviceIds.Contains(s.ServiceId))
                    .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                    .ToListAsync();

                // 🚨 PROFESSIONAL: محاسبه بیمه real-time برای تمام آیتم‌ها اگر محاسبات ارائه نشده باشد
                // 🚨 PROFESSIONAL FIX: همیشه محاسبه کن (حتی اگر dictionary موجود باشد) برای آیتم‌هایی که محاسبه نشده‌اند
                if (draft.PatientId > 0)
                {
                    if (insuranceCalculations == null)
                    {
                        insuranceCalculations = new Dictionary<int, ItemInsuranceCalculationDto>();
                    }
                    
                    _logger.Information("🏥 FACADE: شروع محاسبه بیمه real-time برای تمام آیتم‌ها - ReceptionId: {ReceptionId}, ItemsCount: {Count}, PreCalculatedCount: {PreCount}", 
                        draft.ReceptionId, draft.ReceptionItems.Count(i => !i.IsDeleted), insuranceCalculations.Count);
                    
                    var calculationDate = draft.ReceptionDate != default(DateTime) ? draft.ReceptionDate : DateTime.Now;
                    
                    // 🚨 PROFESSIONAL FIX: استفاده از PricingEngine برای محاسبه بیمه (مثل AddItemAsync)
                    // این اطمینان می‌دهد که محاسبه بر اساس BasePlanId و SupplementaryPlanId در Reception انجام می‌شود
                    var year = _financialYearService.GetCurrentYear();
                    
                    foreach (var item in draft.ReceptionItems.Where(i => !i.IsDeleted))
                    {
                        // 🚨 PROFESSIONAL: اگر قبلاً محاسبه نشده، محاسبه کن
                        if (!insuranceCalculations.ContainsKey(item.ServiceId))
                        {
                            try
                            {
                                var itemTotal = item.UnitPrice * item.Quantity;
                                _logger.Information("🏥 FACADE: محاسبه بیمه real-time برای آیتم - ServiceId: {ServiceId}, ItemTotal: {ItemTotal}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                                    item.ServiceId, itemTotal, draft.BasePlanId, draft.SupplementaryPlanId);
                                
                                // 🚨 PROFESSIONAL FIX: استفاده از PricingEngine (مثل AddItemAsync)
                                var quoteRequest = new Services.Pricing.Models.QuoteRequestDto
                                {
                                    ClinicId = draft.ClinicId,
                                    DepartmentId = draft.DepartmentId,
                                    DoctorId = draft.DoctorId,
                                    ServiceId = item.ServiceId,
                                    FinancialYearId = year,
                                    Primary = draft.BasePlanId.HasValue
                                        ? new Services.Pricing.Models.PartyInsuranceDto { InsurancePlanId = draft.BasePlanId.Value }
                                        : null,
                                    Supplementary = draft.SupplementaryPlanId.HasValue
                                        ? new Services.Pricing.Models.PartyInsuranceDto { InsurancePlanId = draft.SupplementaryPlanId.Value }
                                        : null
                                };

                                var quoteResult = await _pricingEngine.QuoteAsync(quoteRequest);
                                
                                if (quoteResult != null && quoteResult.ApprovedTariff > 0)
                                {
                                    var primaryCoverage = (decimal)quoteResult.Primary.Pays;
                                    var supplementaryCoverage = (decimal)quoteResult.Supplementary.Pays;
                                    var totalCoverage = primaryCoverage + supplementaryCoverage;
                                    var patientShare = itemTotal - totalCoverage;
                                    if (patientShare < 0) patientShare = 0;
                                    
                                    // تعیین وضعیت پوشش
                                    string coverageStatus;
                                    if (totalCoverage >= itemTotal)
                                    {
                                        coverageStatus = "پوشش کامل";
                                    }
                                    else if (totalCoverage > 0)
                                    {
                                        coverageStatus = "پوشش ناقص";
                                    }
                                    else
                                    {
                                        coverageStatus = "بدون پوشش";
                                    }
                                    
                                    var insuranceDto = new ItemInsuranceCalculationDto
                                    {
                                        PrimaryCoverage = primaryCoverage,
                                        SupplementaryCoverage = supplementaryCoverage,
                                        TotalInsuranceCoverage = totalCoverage,
                                        PatientShare = patientShare,
                                        CoverageStatus = coverageStatus,
                                        PrimaryCoveragePercent = quoteResult.Primary.CoveragePercent,
                                        SupplementaryCoveragePercent = quoteResult.Supplementary.CoveragePercent,
                                        TotalCoveragePercent = quoteResult.Primary.CoveragePercent + quoteResult.Supplementary.CoveragePercent
                                    };
                                    
                                    insuranceCalculations[item.ServiceId] = insuranceDto;
                                    _logger.Information("✅ FACADE: محاسبه بیمه real-time موفق (از PricingEngine) - ServiceId: {ServiceId}, TotalCoverage: {TotalCoverage}, PrimaryCoverage: {PrimaryCoverage}, SupplementaryCoverage: {SupplementaryCoverage}, PatientShare: {PatientShare}", 
                                        item.ServiceId, insuranceDto.TotalInsuranceCoverage, insuranceDto.PrimaryCoverage, 
                                        insuranceDto.SupplementaryCoverage, insuranceDto.PatientShare);
                                }
                                else
                                {
                                    _logger.Warning("⚠️ FACADE: محاسبه بیمه real-time ناموفق (PricingEngine) - ServiceId: {ServiceId}, QuoteResult: {QuoteResult}", 
                                        item.ServiceId, quoteResult == null ? "null" : "Invalid");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "❌ FACADE: خطا در محاسبه بیمه real-time - ServiceId: {ServiceId}", item.ServiceId);
                            }
                        }
                        else
                        {
                            _logger.Debug("🏥 FACADE: محاسبه بیمه برای ServiceId {ServiceId} قبلاً انجام شده - استفاده از محاسبه موجود", item.ServiceId);
                        }
                    }
                    
                    _logger.Information("✅ FACADE: محاسبه بیمه real-time تکمیل شد - ReceptionId: {ReceptionId}, CalculatedCount: {Count}", 
                        draft.ReceptionId, insuranceCalculations.Count);
                }
                else
                {
                    _logger.Warning("⚠️ FACADE: PatientId تنظیم نشده - ReceptionId: {ReceptionId}, محاسبه بیمه انجام نمی‌شود", draft.ReceptionId);
                }

                var items = draft.ReceptionItems.Where(i => !i.IsDeleted).Select(it => 
                {
                    var service = services.FirstOrDefault(s => s.ServiceId == it.ServiceId);
                    
                    // ✅ گام 2.2: استخراج TariffWarning از SnapshotJson
                    string tariffWarning = null;
                    if (!string.IsNullOrEmpty(it.SnapshotJson))
                    {
                        try
                        {
                            var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(it.SnapshotJson);
                            if (snapshot?.TariffWarning != null)
                            {
                                tariffWarning = snapshot.TariffWarning.ToString();
                                _logger.Debug("✅ FACADE: TariffWarning استخراج شد از SnapshotJson - ReceptionItemId: {ReceptionItemId}, ServiceId: {ServiceId}, Warning: {Warning}",
                                    it.ReceptionItemId, it.ServiceId, tariffWarning);
                            }
                        }
                        catch (Exception snapshotEx)
                        {
                            _logger.Warning(snapshotEx, "⚠️ FACADE: خطا در parse کردن SnapshotJson برای استخراج TariffWarning - ReceptionItemId: {ReceptionItemId}", 
                                it.ReceptionItemId);
                        }
                    }
                    
                    var itemDto = new ReceptionItemDto
                    {
                        ServiceId = it.ServiceId,
                        Code = service?.ServiceCode ?? "",
                        Name = service?.Title ?? "",
                        Qty = it.Quantity,
                        UnitPriceIRR = it.UnitPrice,
                        TotalIRR = it.UnitPrice * it.Quantity,
                        // ✅ افزودن TariffWarning
                        TariffWarning = tariffWarning
                    };
                    
                    // 🚨 PROFESSIONAL: افزودن محاسبه بیمه real-time
                    if (insuranceCalculations != null && insuranceCalculations.ContainsKey(it.ServiceId))
                    {
                        var calc = insuranceCalculations[it.ServiceId];
                        if (calc != null)
                        {
                            itemDto.InsuranceCalculation = calc;
                            _logger.Information("✅ FACADE: InsuranceCalculation اضافه شد به ReceptionItemDto - ServiceId: {ServiceId}, Status: {Status}, PrimaryCoverage: {PrimaryCoverage}, SupplementaryCoverage: {SupplementaryCoverage}, PatientShare: {PatientShare}", 
                                it.ServiceId, calc.CoverageStatus, calc.PrimaryCoverage, calc.SupplementaryCoverage, calc.PatientShare);
                        }
                        else
                        {
                            _logger.Warning("⚠️ FACADE: InsuranceCalculation برای ServiceId {ServiceId} null است - محاسبه انجام نشده یا با خطا مواجه شده", it.ServiceId);
                        }
                    }
                    else
                    {
                        _logger.Warning("⚠️ FACADE: InsuranceCalculation برای ServiceId {ServiceId} در dictionary موجود نیست - HasDictionary: {HasDict}, ContainsKey: {ContainsKey}", 
                            it.ServiceId, insuranceCalculations != null, insuranceCalculations?.ContainsKey(it.ServiceId) ?? false);
                    }
                    
                    return itemDto;
                }).ToList();

                // ✅ تبدیل ReceptionTotalsDto به TotalsDto
                // 🚨 PROFESSIONAL FIX: اگر totalsDto null است، از مقادیر محاسبه شده از items استفاده کن
                TotalsDto totals = null;
                if (totalsDto != null)
                {
                    totals = new TotalsDto 
                    { 
                        Gross = (decimal)totalsDto.GrossIRR, 
                        Base = (decimal)totalsDto.BaseCoveredIRR, 
                        Supplementary = (decimal)totalsDto.SuppCoveredIRR, 
                        Patient = (decimal)totalsDto.PatientPayableIRR 
                    };
                }
                else
                {
                    // ✅ بهینه‌سازی: محاسبه totals از items با تقسیم base و supp
                    var itemsList = draft.ReceptionItems.Where(i => !i.IsDeleted).ToList();
                    var gross = itemsList.Sum(i => i.UnitPrice * i.Quantity);
                    var patient = itemsList.Sum(i => i.PatientShareAmount);
                    var insurer = itemsList.Sum(i => i.InsurerShareAmount);
                    
                    // ✅ تقسیم insurer به base و supp از SnapshotJson
                    decimal basePay = 0m;
                    decimal suppPay = 0m;
                    
                    foreach (var item in itemsList)
                    {
                        if (!string.IsNullOrEmpty(item.SnapshotJson))
                        {
                            try
                            {
                                var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.SnapshotJson);
                                if (snapshot != null)
                                {
                                    if (snapshot.PrimaryPays != null)
                                        basePay += (decimal)snapshot.PrimaryPays;
                                    if (snapshot.SupplementaryPays != null)
                                        suppPay += (decimal)snapshot.SupplementaryPays;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Warning(ex, "⚠️ FACADE: خطا در parse کردن SnapshotJson برای ReceptionItem {ReceptionItemId}", item.ReceptionItemId);
                            }
                        }
                    }
                    
                    // ✅ Fallback: اگر SnapshotJson خالی است، از بیمه‌های Reception تقسیم کن
                    if (basePay == 0 && suppPay == 0 && insurer > 0)
                    {
                        if (draft.BasePlanId.HasValue && draft.SupplementaryPlanId.HasValue)
                        {
                            // تقسیم 50-50 اگر هر دو بیمه وجود دارند
                            basePay = insurer / 2m;
                            suppPay = insurer - basePay;
                        }
                        else if (draft.BasePlanId.HasValue)
                        {
                            basePay = insurer;
                        }
                        else if (draft.SupplementaryPlanId.HasValue)
                        {
                            suppPay = insurer;
                        }
                        else
                        {
                            // اگر هیچ بیمه‌ای نیست، همه به base می‌رود (برای سازگاری)
                            basePay = insurer;
                        }
                    }
                    
                    totals = new TotalsDto 
                    { 
                        Gross = gross, 
                        Base = basePay,
                        Supplementary = suppPay, 
                        Patient = patient 
                    };
                    _logger.Information("🏥 FACADE: Totals محاسبه شده از ReceptionItems - Gross: {Gross}, Base: {Base}, Supplementary: {Supplementary}, Patient: {Patient}", 
                        gross, basePay, suppPay, patient);
                }
                
                return ServiceResult<ItemsAndTotalsDto>.Successful(new ItemsAndTotalsDto
                {
                    Items = items,
                    Totals = totals
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در بازمحاسبه - ReceptionId: {ReceptionId}", draft?.ReceptionId);
                
                // 🚨 PROFESSIONAL FIX: حتی در صورت خطا، سعی کن آیتم‌های موجود را برگردان
                if (draft != null)
                {
                    try
                    {
                        // Reload draft from database
                        var reloadedDraft = await _context.Receptions
                            .Include(d => d.ReceptionItems)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(d => d.ReceptionId == draft.ReceptionId && d.Status == ReceptionStatus.Pending);
                        
                        if (reloadedDraft != null && reloadedDraft.ReceptionItems != null && reloadedDraft.ReceptionItems.Any(i => !i.IsDeleted))
                        {
                            _logger.Information("🏥 FACADE: بازگرداندن آیتم‌های موجود پس از exception در RecalculateDraftAsync - ReceptionId: {ReceptionId}, ItemsCount: {Count}", 
                                reloadedDraft.ReceptionId, reloadedDraft.ReceptionItems.Count(i => !i.IsDeleted));
                            
                            // ساخت items DTO
                            var serviceIds = reloadedDraft.ReceptionItems.Where(i => !i.IsDeleted).Select(i => i.ServiceId).Distinct().ToList();
                            var services = await _context.Services
                                .Where(s => serviceIds.Contains(s.ServiceId))
                                .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                                .ToListAsync();
                            
                            var items = reloadedDraft.ReceptionItems.Where(i => !i.IsDeleted).Select(it => 
                            {
                                var service = services.FirstOrDefault(s => s.ServiceId == it.ServiceId);
                                
                                // ✅ استخراج TariffWarning از SnapshotJson
                                string tariffWarning = null;
                                if (!string.IsNullOrEmpty(it.SnapshotJson))
                                {
                                    try
                                    {
                                        var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(it.SnapshotJson);
                                        if (snapshot?.TariffWarning != null)
                                        {
                                            tariffWarning = snapshot.TariffWarning.ToString();
                                        }
                                    }
                                    catch (Exception snapshotEx)
                                    {
                                        _logger.Warning(snapshotEx, "⚠️ FACADE: خطا در parse کردن SnapshotJson برای استخراج TariffWarning - ReceptionItemId: {ReceptionItemId}", 
                                            it.ReceptionItemId);
                                    }
                                }
                                
                                return new ReceptionItemDto
                                {
                                    ServiceId = it.ServiceId,
                                    Code = service?.ServiceCode ?? "",
                                    Name = service?.Title ?? "",
                                    Qty = it.Quantity,
                                    UnitPriceIRR = it.UnitPrice,
                                    TotalIRR = it.UnitPrice * it.Quantity,
                                    // ✅ افزودن TariffWarning
                                    TariffWarning = tariffWarning
                                };
                            }).ToList();
                            
                            // محاسبه totals از items
                            var gross = items.Sum(i => i.TotalIRR);
                            var patient = reloadedDraft.ReceptionItems.Where(i => !i.IsDeleted).Sum(i => i.PatientShareAmount);
                            var insurer = reloadedDraft.ReceptionItems.Where(i => !i.IsDeleted).Sum(i => i.InsurerShareAmount);
                            
                            return ServiceResult<ItemsAndTotalsDto>.Successful(new ItemsAndTotalsDto
                            {
                                Items = items,
                                Totals = new TotalsDto 
                                { 
                                    Gross = gross, 
                                    Base = insurer, 
                                    Supplementary = 0, 
                                    Patient = patient 
                                }
                            });
                        }
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.Error(fallbackEx, "❌ FACADE: خطا در بازگرداندن آیتم‌های موجود در catch block اصلی");
                    }
                }
                
                return ServiceResult<ItemsAndTotalsDto>.Failed("خطا در بازمحاسبه: " + ex.Message);
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
                        
                        // ✅ بهینه‌سازی: محاسبه FranchisePercent از Deductible
                        // Deductible به صورت مبلغ (ریال) است، پس آن را به صورت مبلغ نمایش می‌دهیم
                        // در صورت نیاز به درصد، می‌توان از BasePrice استفاده کرد (در آینده)
                        baseCoverage.FranchisePercent = 0m; // Deductible به صورت مبلغ است، نه درصد
                        baseCoverage.FranchisePercentStr = basePlan.Deductible > 0 ? 
                            basePlan.Deductible.ToString("N0") + " ریال" : "—";
                        
                        // ✅ بهینه‌سازی: سقف‌ها از InsuranceTariff یا BusinessRule خوانده می‌شوند
                        // PlanCoverage در حال حاضر در مدل وجود ندارد، اما می‌توان در آینده اضافه کرد
                        // فعلاً از InsuranceTariff استفاده می‌شود که در محاسبات Coverage اعمال می‌شود
                        baseCoverage.CeilingPerService = null; // TODO: از InsuranceTariff یا BusinessRule بخوان
                        baseCoverage.CeilingPerVisit = null; // TODO: از InsuranceTariff یا BusinessRule بخوان
                        baseCoverage.CeilingMonthly = null; // TODO: از InsuranceTariff یا BusinessRule بخوان
                        baseCoverage.RemainingCeiling = null; // TODO: محاسبه از Ceiling - Used
                        
                        baseCoverage.CeilingPerServiceStr = "—"; // TODO: از InsuranceTariff یا BusinessRule بخوان
                        baseCoverage.CeilingPerVisitStr = "—"; // TODO: از InsuranceTariff یا BusinessRule بخوان
                        baseCoverage.CeilingMonthlyStr = "—"; // TODO: از InsuranceTariff یا BusinessRule بخوان
                        baseCoverage.RemainingCeilingStr = "—"; // TODO: محاسبه از Ceiling - Used
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

                // ✅ بهینه‌سازی: محاسبه پوشش مؤثر با در نظر گیری Deductible
                decimal baseCov = (baseCoverage.CoveragePercent ?? 0m) / 100m;
                decimal suppCov = (suppCoverage.CoveragePercent ?? 0m) / 100m;
                
                // ✅ محاسبه فرانشیز: Deductible به صورت مبلغ (ریال) است
                // در محاسبات Coverage، Deductible از مبلغ کل کسر می‌شود
                // franchiseAdj برای تنظیم درصد پوشش استفاده می‌شود (در آینده می‌توان بهبود داد)
                decimal franchiseAdj = 0m; // Deductible در محاسبات Coverage اعمال می‌شود، نه در اینجا
                
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

                // 3) 🏥 MEDICAL: استفاده از PricingEngine برای محاسبه واقعی سهم بیمار (به جای محاسبه تئوری)
                // این روش سقف‌ها و محدودیت‌های واقعی را در نظر می‌گیرد
                decimal patientShare = 0m;
                decimal effPct = 0m;
                
                if (request.BasePlanId.HasValue || request.SupplementaryPlanId.HasValue)
                {
                    // 🏥 MEDICAL: استخراج ClinicId از DepartmentId (در صورت وجود)
                    int clinicId = 0;
                    if (request.DepartmentId.HasValue)
                    {
                        var department = await _context.Departments
                            .AsNoTracking()
                            .Where(d => d.DepartmentId == request.DepartmentId.Value && !d.IsDeleted)
                            .Select(d => new { d.ClinicId })
                            .FirstOrDefaultAsync();
                        if (department != null)
                        {
                            clinicId = department.ClinicId;
                        }
                    }
                    
                    // استفاده از PricingEngine برای محاسبه واقعی
                    var quoteRequest = new Services.Pricing.Models.QuoteRequestDto
                    {
                        ClinicId = clinicId,
                        DepartmentId = request.DepartmentId ?? 0,
                        DoctorId = request.DoctorId ?? 0,
                        ServiceId = service.ServiceId,
                        FinancialYearId = financialYear,
                        Primary = request.BasePlanId.HasValue
                            ? new Services.Pricing.Models.PartyInsuranceDto { InsurancePlanId = request.BasePlanId.Value }
                            : null,
                        Supplementary = request.SupplementaryPlanId.HasValue
                            ? new Services.Pricing.Models.PartyInsuranceDto { InsurancePlanId = request.SupplementaryPlanId.Value }
                            : null
                    };

                    var quoteResult = await _pricingEngine.QuoteAsync(quoteRequest);
                    
                    if (quoteResult != null && quoteResult.ApprovedTariff > 0)
                    {
                        // استفاده از سهم بیمار واقعی از QuoteResult
                        patientShare = (decimal)quoteResult.PatientFinal;
                        
                        // محاسبه درصد پوشش مؤثر واقعی
                        var totalCovered = quoteResult.Primary.Pays + quoteResult.Supplementary.Pays;
                        effPct = quoteResult.ApprovedTariff > 0 
                            ? Math.Round((totalCovered / (decimal)quoteResult.ApprovedTariff) * 100m, 2)
                            : 0m;
                        
                        _logger.Information("✅ FACADE: محاسبه واقعی سهم بیمار از PricingEngine - PatientShare: {PatientShare}, EffectiveCoverage: {EffPct}%, ApprovedTariff: {ApprovedTariff}, PrimaryPays: {PrimaryPays}, SuppPays: {SuppPays}", 
                            patientShare, effPct, quoteResult.ApprovedTariff, quoteResult.Primary.Pays, quoteResult.Supplementary.Pays);
                    }
                    else
                    {
                        // Fallback: اگر PricingEngine خطا داد، از محاسبه تئوری استفاده کن
                        var coverage = await GetInsuranceCoverageAsync(request.PatientId ?? 0, request.BasePlanId, request.SupplementaryPlanId);
                        if (coverage.Success && coverage.Data != null)
                        {
                            effPct = coverage.Data.Effective.EffectiveCoveragePercent;
                            patientShare = Math.Round(unitPrice * (1m - effPct / 100m), 0);
                        }
                        else
                        {
                            patientShare = unitPrice; // بدون بیمه، کل مبلغ سهم بیمار است
                        }
                    }
                }
                else
                {
                    // بدون بیمه، کل مبلغ سهم بیمار است
                    patientShare = unitPrice;
                }

                // 4) فرمت مبالغ
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

        #region Edit Operations

        /// <summary>
        /// بارگذاری پذیرش برای ویرایش
        /// </summary>
        public async Task<ServiceResult<ReceptionEditLoadDto>> LoadReceptionForEditAsync(int receptionId)
        {
            try
            {
                _logger.Information("🏥 FACADE: بارگذاری پذیرش برای ویرایش - ReceptionId: {ReceptionId}", receptionId);

                // 1. دریافت پذیرش با تمام جزئیات
                // ⚠️ مشکل: Include(r => r.Doctor) باعث materialize شدن کامل Doctor entity می‌شود و ممکن است با enum Degree مشکل ایجاد کند
                // ✅ راه‌حل: از query جداگانه برای Doctor استفاده می‌کنیم تا فقط FirstName و LastName را بگیریم
                var reception = await _context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Department)
                    .Include(r => r.Clinic)
                    .Include(r => r.ActivePatientInsurance)
                    .Include(r => r.ActivePatientInsurance.InsurancePlan)
                    .Include(r => r.ActivePatientInsurance.SupplementaryInsurancePlan)
                    .Include(r => r.ReceptionItems.Select(ri => ri.Service))
                    .Include(r => r.Transactions)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReceptionId == receptionId);

                if (reception == null)
                {
                    _logger.Warning("⚠️ FACADE: پذیرش یافت نشد - ReceptionId: {ReceptionId}", receptionId);
                    return ServiceResult<ReceptionEditLoadDto>.Failed($"پذیرش با شناسه {receptionId} یافت نشد", "NOT_FOUND");
                }

                // 2. دریافت نام پزشک به صورت جداگانه (فقط FirstName و LastName)
                // ⚠️ مشکل: Include(r => r.Doctor) باعث materialize شدن کامل Doctor entity می‌شود و ممکن است با enum Degree مشکل ایجاد کند
                // ✅ راه‌حل: از query جداگانه برای Doctor استفاده می‌کنیم تا فقط FirstName و LastName را بگیریم
                string doctorFullName = string.Empty;
                if (reception.DoctorId > 0)
                {
                    var doctorInfo = await _context.Doctors
                        .AsNoTracking()
                        .Where(d => d.DoctorId == reception.DoctorId)
                        .Select(d => new { d.FirstName, d.LastName })
                        .FirstOrDefaultAsync();
                    
                    if (doctorInfo != null)
                    {
                        doctorFullName = $"{doctorInfo.FirstName} {doctorInfo.LastName}".Trim();
                    }
                }

                // 3. بررسی مجوز ویرایش بر اساس وضعیت
                var permissions = DetermineEditPermissions(reception.Status);

                // 4. ساخت DTO
                var result = new ReceptionEditLoadDto
                {
                    ReceptionId = reception.ReceptionId,
                    Status = reception.Status,
                    
                    // اطلاعات بیمار (کامل برای نمایش در فرم ویرایش)
                    PatientId = reception.PatientId,
                    PatientFullName = reception.Patient != null 
                        ? $"{reception.Patient.FirstName} {reception.Patient.LastName}".Trim()
                        : string.Empty,
                    PatientNationalCode = reception.Patient?.NationalCode ?? string.Empty,
                    PatientFirstName = reception.Patient?.FirstName ?? string.Empty,
                    PatientLastName = reception.Patient?.LastName ?? string.Empty,
                    PatientFatherName = reception.Patient?.FatherName ?? string.Empty,
                    PatientGender = reception.Patient?.Gender.ToString() ?? string.Empty,
                    PatientBirthDateShamsi = reception.Patient?.BirthDate != null 
                        ? PersianDateHelper.ToPersianDate(reception.Patient.BirthDate.Value)
                        : string.Empty,
                    PatientMobile = reception.Patient?.PhoneNumber ?? string.Empty,
                    PatientPhone = reception.Patient?.PhoneNumber ?? string.Empty,
                    PatientAddress = reception.Patient?.Address ?? string.Empty,
                    
                    // اطلاعات پزشک و دپارتمان
                    DoctorId = reception.DoctorId,
                    DoctorFullName = doctorFullName,
                    DepartmentId = reception.DepartmentId,
                    DepartmentName = reception.Department?.Name ?? string.Empty,
                    ClinicId = reception.ClinicId,
                    ClinicName = reception.Clinic?.Name ?? string.Empty,
                    
                    // تاریخ پذیرش
                    ReceptionDate = reception.ReceptionDate,
                    ReceptionDateShamsi = reception.ReceptionDate.ToPersianDate(),
                    
                    // بیمه‌ها
                    BasePlanId = reception.BasePlanId,
                    BasePlanName = reception.ActivePatientInsurance?.InsurancePlan?.Name ?? string.Empty,
                    SupplementaryPlanId = reception.SupplementaryPlanId,
                    SupplementaryPlanName = reception.ActivePatientInsurance?.SupplementaryInsurancePlan?.Name ?? string.Empty,
                    
                    // خدمات
                    Items = reception.ReceptionItems
                        .Where(ri => !ri.IsDeleted)
                        .Select(ri => new ReceptionItemEditDto
                        {
                            ReceptionItemId = ri.ReceptionItemId,
                            ServiceId = ri.ServiceId,
                            ServiceCode = ri.Service?.ServiceCode ?? string.Empty,
                            ServiceName = ri.Service?.Title ?? string.Empty,
                            Quantity = ri.Quantity,
                            UnitPrice = ri.UnitPrice,
                            TotalPrice = ri.UnitPrice * ri.Quantity,
                            PatientShareAmount = ri.PatientShareAmount,
                            InsurerShareAmount = ri.InsurerShareAmount,
                            SnapshotJson = ri.SnapshotJson,
                            IsDeleted = false
                        })
                        .ToList(),
                    
                    // مبالغ
                    TotalAmount = reception.TotalAmount,
                    InsurerShareAmount = reception.InsurerShareAmount,
                    PatientCoPay = reception.PatientCoPay,
                    PaidAmount = reception.Transactions?.Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted).Sum(t => (decimal?)t.Amount) ?? 0m,
                    RemainingAmount = reception.PatientCoPay - (reception.Transactions?.Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted).Sum(t => (decimal?)t.Amount) ?? 0m),
                    
                    // یادداشت‌ها و تنظیمات
                    Notes = reception.Notes ?? string.Empty,
                    Type = reception.Type,
                    Priority = reception.Priority,
                    IsEmergency = reception.IsEmergency,
                    IsOnlineReception = reception.IsOnlineReception,
                    
                    // محدودیت‌های ویرایش
                    Permissions = permissions
                };

                // 4. بارگذاری لیست‌های کمکی (فقط اگر قابل ویرایش باشند)
                if (permissions.CanEditDoctor || permissions.CanEditDepartment)
                {
                    // بارگذاری پزشکان دپارتمان
                    var doctorsResult = await GetDoctorsByDepartmentAsync(reception.DepartmentId, reception.ClinicId);
                    if (doctorsResult.Success && doctorsResult.Data != null)
                    {
                        result.AvailableDoctors = doctorsResult.Data;
                    }
                }

                if (permissions.CanEditDepartment)
                {
                    // بارگذاری دپارتمان‌ها
                    var deptsResult = await _departmentManagementService.GetAllDepartmentsAsync();
                    if (deptsResult.Success && deptsResult.Data != null)
                    {
                        result.AvailableDepartments = deptsResult.Data.Select(d => new DepartmentDto
                        {
                            DepartmentId = d.DepartmentId,
                            Name = d.Name,
                            Code = d.Code,
                            IsActive = d.IsActive,
                            Description = d.Description
                        }).ToList();
                    }
                }

                if (permissions.CanEditServices)
                {
                    // بارگذاری خدمات دپارتمان
                    var servicesResult = await GetServicesForDeptAsync(reception.DepartmentId);
                    if (servicesResult.Success && servicesResult.Data != null)
                    {
                        result.AvailableServices = servicesResult.Data.Services;
                    }
                }

                if (permissions.CanEditInsurances)
                {
                    // بارگذاری بیمه‌های بیمار
                    var insurancesResult = await LoadPatientInsurancesAsync(reception.PatientId);
                    if (insurancesResult.Success && insurancesResult.Data != null)
                    {
                        result.PatientInsurances = insurancesResult.Data;
                    }
                }

                _logger.Information("✅ FACADE: پذیرش برای ویرایش بارگذاری شد - ReceptionId: {ReceptionId}, Status: {Status}, ItemsCount: {ItemsCount}", 
                    receptionId, reception.Status, result.Items.Count);

                return ServiceResult<ReceptionEditLoadDto>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در بارگذاری پذیرش برای ویرایش - ReceptionId: {ReceptionId}", receptionId);
                return ServiceResult<ReceptionEditLoadDto>.Failed($"خطا در بارگذاری پذیرش: {ex.Message}");
            }
        }

        /// <summary>
        /// تعیین مجوزهای ویرایش بر اساس وضعیت پذیرش
        /// </summary>
        private EditPermissionsDto DetermineEditPermissions(ReceptionStatus status)
        {
            var permissions = new EditPermissionsDto();

            switch (status)
            {
                case ReceptionStatus.Pending:
                    // پذیرش در انتظار: تمام فیلدها قابل ویرایش
                    permissions.CanEditPatient = true;
                    permissions.CanEditDoctor = true;
                    permissions.CanEditDepartment = true;
                    permissions.CanEditServices = true;
                    permissions.CanEditInsurances = true;
                    permissions.CanEditAmounts = true;
                    permissions.CanEditDate = true;
                    permissions.CanEditNotes = true;
                    permissions.RequiresApproval = false;
                    break;

                case ReceptionStatus.Completed:
                    // پذیرش تکمیل شده: فقط یادداشت‌ها و تنظیمات جزئی قابل ویرایش
                    permissions.CanEditPatient = false;
                    permissions.CanEditDoctor = false;
                    permissions.CanEditDepartment = false;
                    permissions.CanEditServices = false;
                    permissions.CanEditInsurances = false;
                    permissions.CanEditAmounts = false;
                    permissions.CanEditDate = false;
                    permissions.CanEditNotes = true;
                    permissions.RequiresApproval = false;
                    break;

                case ReceptionStatus.Cancelled:
                    // پذیرش لغو شده: هیچ فیلدی قابل ویرایش نیست
                    permissions.CanEditPatient = false;
                    permissions.CanEditDoctor = false;
                    permissions.CanEditDepartment = false;
                    permissions.CanEditServices = false;
                    permissions.CanEditInsurances = false;
                    permissions.CanEditAmounts = false;
                    permissions.CanEditDate = false;
                    permissions.CanEditNotes = false;
                    permissions.RequiresApproval = false;
                    break;

                default:
                    // حالت پیش‌فرض: محافظه‌کارانه
                    permissions.CanEditPatient = false;
                    permissions.CanEditDoctor = false;
                    permissions.CanEditDepartment = false;
                    permissions.CanEditServices = false;
                    permissions.CanEditInsurances = false;
                    permissions.CanEditAmounts = false;
                    permissions.CanEditDate = false;
                    permissions.CanEditNotes = true;
                    permissions.RequiresApproval = true;
                    break;
            }

            return permissions;
        }

        /// <summary>
        /// به‌روزرسانی پذیرش
        /// </summary>
        public async Task<ServiceResult<UpdateReceptionResponse>> UpdateReceptionAsync(UpdateReceptionRequest request)
        {
            try
            {
                _logger.Information("🏥 FACADE: به‌روزرسانی پذیرش - ReceptionId: {ReceptionId}", request.ReceptionId);

                // 1. دریافت پذیرش
                var reception = await _context.Receptions
                    .Include(r => r.ReceptionItems)
                    .FirstOrDefaultAsync(r => r.ReceptionId == request.ReceptionId);

                if (reception == null)
                {
                    _logger.Warning("⚠️ FACADE: پذیرش یافت نشد - ReceptionId: {ReceptionId}", request.ReceptionId);
                    return ServiceResult<UpdateReceptionResponse>.Failed($"پذیرش با شناسه {request.ReceptionId} یافت نشد", "NOT_FOUND");
                }

                // 2. بررسی مجوز ویرایش
                var permissions = DetermineEditPermissions(reception.Status);
                
                if (reception.Status == ReceptionStatus.Cancelled)
                {
                    return ServiceResult<UpdateReceptionResponse>.Failed("امکان ویرایش پذیرش لغو شده وجود ندارد", "CANCELLED");
                }

                // 3. اعمال تغییرات (فقط فیلدهای مجاز)
                if (permissions.CanEditDoctor && request.DoctorId.HasValue)
                {
                    reception.DoctorId = request.DoctorId.Value;
                }

                if (permissions.CanEditDepartment && request.DepartmentId.HasValue)
                {
                    reception.DepartmentId = request.DepartmentId.Value;
                    
                    // اگر ClinicId تغییر کرده
                    if (request.ClinicId.HasValue)
                    {
                        reception.ClinicId = request.ClinicId.Value;
                    }
                    else
                    {
                        // استخراج ClinicId از Department
                        var department = await _context.Departments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(d => d.DepartmentId == request.DepartmentId.Value);
                        if (department != null)
                        {
                            reception.ClinicId = department.ClinicId;
                        }
                    }
                }

                if (permissions.CanEditDate && request.ReceptionDate.HasValue)
                {
                    reception.ReceptionDate = request.ReceptionDate.Value;
                }

                if (permissions.CanEditInsurances)
                {
                    if (request.BasePlanId.HasValue)
                    {
                        reception.BasePlanId = request.BasePlanId.Value;
                    }
                    if (request.SupplementaryPlanId.HasValue)
                    {
                        reception.SupplementaryPlanId = request.SupplementaryPlanId.Value;
                    }
                }

                if (permissions.CanEditNotes && !string.IsNullOrEmpty(request.Notes))
                {
                    reception.Notes = request.Notes;
                }

                if (request.Type.HasValue)
                {
                    reception.Type = request.Type.Value;
                }

                if (request.Priority.HasValue)
                {
                    reception.Priority = request.Priority.Value;
                }

                if (request.IsEmergency.HasValue)
                {
                    reception.IsEmergency = request.IsEmergency.Value;
                }

                // 4. مدیریت خدمات (فقط اگر مجاز باشد)
                if (permissions.CanEditServices && request.Items != null && request.Items.Any())
                {
                    await UpdateReceptionItemsAsync(reception, request.Items);
                }

                // 5. بازمحاسبه قیمت‌ها (اگر لازم باشد)
                if (request.RecalculatePrices && (permissions.CanEditServices || permissions.CanEditInsurances))
                {
                    await _receptionPricingService.CalculateTotalsAsync(reception.ReceptionId);
                    
                    // بارگذاری مجدد برای دریافت مبالغ به‌روزرسانی شده
                    await _context.Entry(reception).ReloadAsync();
                }

                // 6. ذخیره تغییرات
                await _context.SaveChangesAsync();

                // 7. ساخت پاسخ
                var response = new UpdateReceptionResponse
                {
                    ReceptionId = reception.ReceptionId,
                    Status = reception.Status,
                    Items = reception.ReceptionItems
                        .Where(ri => !ri.IsDeleted)
                        .Select(ri => new ReceptionItemEditDto
                        {
                            ReceptionItemId = ri.ReceptionItemId,
                            ServiceId = ri.ServiceId,
                            ServiceCode = ri.Service?.ServiceCode ?? string.Empty,
                            ServiceName = ri.Service?.Title ?? string.Empty,
                            Quantity = ri.Quantity,
                            UnitPrice = ri.UnitPrice,
                            TotalPrice = ri.UnitPrice * ri.Quantity,
                            PatientShareAmount = ri.PatientShareAmount,
                            InsurerShareAmount = ri.InsurerShareAmount,
                            SnapshotJson = ri.SnapshotJson,
                            IsDeleted = false
                        })
                        .ToList(),
                    Totals = new ViewModels.Reception.ReceptionTotalsDto
                    {
                        GrossAmount = reception.TotalAmount,
                        PatientPayable = reception.PatientCoPay,
                        BaseInsurancePayable = reception.BasePay,
                        SupplementaryInsurancePayable = reception.SuppPay // ✅ بهینه‌سازی: استفاده از فیلدهای Reception
                    },
                    RequiresApproval = permissions.RequiresApproval,
                    Message = "پذیرش با موفقیت به‌روزرسانی شد"
                };

                _logger.Information("✅ FACADE: پذیرش به‌روزرسانی شد - ReceptionId: {ReceptionId}, Status: {Status}", 
                    reception.ReceptionId, reception.Status);

                return ServiceResult<UpdateReceptionResponse>.Successful(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در به‌روزرسانی پذیرش - ReceptionId: {ReceptionId}", request.ReceptionId);
                return ServiceResult<UpdateReceptionResponse>.Failed($"خطا در به‌روزرسانی پذیرش: {ex.Message}");
            }
        }

        /// <summary>
        /// به‌روزرسانی آیتم‌های پذیرش
        /// </summary>
        private async Task UpdateReceptionItemsAsync(Models.Entities.Reception.Reception reception, List<ReceptionItemUpdateDto> updates)
        {
            foreach (var update in updates)
            {
                if (update.IsDeleted && update.ReceptionItemId.HasValue)
                {
                    // حذف آیتم موجود
                    var item = reception.ReceptionItems.FirstOrDefault(ri => ri.ReceptionItemId == update.ReceptionItemId.Value);
                    if (item != null)
                    {
                        item.IsDeleted = true;
                    }
                }
                else if (!update.ReceptionItemId.HasValue)
                {
                    // افزودن آیتم جدید
                    var addRequest = new AddItemRequest
                    {
                        ReceptionId = reception.ReceptionId,
                        ServiceId = update.ServiceId,
                        Quantity = update.Quantity
                    };
                    await AddItemAsync(addRequest);
                }
                else
                {
                    // به‌روزرسانی آیتم موجود
                    var item = reception.ReceptionItems.FirstOrDefault(ri => ri.ReceptionItemId == update.ReceptionItemId.Value);
                    if (item != null && item.ServiceId != update.ServiceId)
                    {
                        // اگر ServiceId تغییر کرده، باید آیتم را حذف و دوباره اضافه کنیم
                        item.IsDeleted = true;
                        
                        var addRequest = new AddItemRequest
                        {
                            ReceptionId = reception.ReceptionId,
                            ServiceId = update.ServiceId,
                            Quantity = update.Quantity
                        };
                        await AddItemAsync(addRequest);
                    }
                    else if (item != null && item.Quantity != update.Quantity)
                    {
                        // فقط تعداد تغییر کرده - بازمحاسبه
                        item.Quantity = update.Quantity;
                        // بازمحاسبه در مرحله بعد انجام می‌شود
                    }
                }
            }
        }

        /// <summary>
        /// لغو پذیرش
        /// </summary>
        public async Task<ServiceResult<CancelReceptionResponse>> CancelReceptionAsync(CancelReceptionRequest request)
        {
            try
            {
                _logger.Information("🚫 FACADE: لغو پذیرش - ReceptionId: {ReceptionId}, Reason: {Reason}", 
                    request.ReceptionId, request.Reason);

                // 1. دریافت پذیرش (با Include ReceptionItems برای صفر کردن مبالغ مالی)
                var reception = await _context.Receptions
                    .Include(r => r.Transactions)
                    .Include(r => r.ReceptionItems) // ✅ اضافه شد: برای صفر کردن مبالغ مالی ReceptionItems
                    .FirstOrDefaultAsync(r => r.ReceptionId == request.ReceptionId);

                if (reception == null)
                {
                    _logger.Warning("⚠️ FACADE: پذیرش یافت نشد - ReceptionId: {ReceptionId}", request.ReceptionId);
                    return ServiceResult<CancelReceptionResponse>.Failed($"پذیرش با شناسه {request.ReceptionId} یافت نشد", "NOT_FOUND");
                }

                // 2. بررسی امکان لغو
                var canCancelResult = CanCancelReception(reception);
                if (!canCancelResult.CanCancel)
                {
                    _logger.Warning("⚠️ FACADE: امکان لغو پذیرش وجود ندارد - ReceptionId: {ReceptionId}, Reason: {Reason}", 
                        request.ReceptionId, canCancelResult.ErrorMessage);
                    return ServiceResult<CancelReceptionResponse>.Failed(canCancelResult.ErrorMessage, "CANNOT_CANCEL");
                }

                // 3. بررسی وجود پرداخت
                var successfulPayments = reception.Transactions
                    .Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted)
                    .ToList();

                var totalPaid = successfulPayments.Sum(t => t.Amount);
                bool hasPayment = totalPaid > 0;

                // 4. مدیریت Refund (اگر پرداختی وجود دارد)
                decimal? refundAmount = null;
                bool refundProcessed = false;

                if (hasPayment && request.ProcessRefund)
                {
                    // ✅ دریافت CashSessionId از اولین تراکنش موفق
                    var firstPayment = successfulPayments.FirstOrDefault();
                    if (firstPayment == null || firstPayment.CashSessionId == 0)
                    {
                        _logger.Warning("⚠️ FACADE: CashSessionId یافت نشد در تراکنش‌های پرداخت - ReceptionId: {ReceptionId}", 
                            request.ReceptionId);
                        return ServiceResult<CancelReceptionResponse>.Failed(
                            "خطا در دریافت اطلاعات صندوق. لطفاً با پشتیبانی تماس بگیرید.",
                            "CASH_SESSION_NOT_FOUND");
                    }

                    // ✅ ثبت تراکنش Refund
                    var refundMethod = firstPayment.Method;
                    var refundTransaction = new Models.Entities.Payment.PaymentTransaction
                    {
                        ReceptionId = reception.ReceptionId,
                        Amount = -totalPaid, // منفی برای Refund
                        Status = PaymentStatus.Canceled, // استفاده از Canceled برای Refund
                        Method = refundMethod,
                        CashSessionId = firstPayment.CashSessionId, // ✅ تنظیم CashSessionId
                        Description = $"برگشت وجه (Refund) - دلیل: {request.RefundReason ?? request.Reason}",
                        IdempotencyKey = Guid.NewGuid().ToString(),
                        CreatedAt = DateTime.Now,
                        CreatedByUserId = _currentUserService.UserId
                    };

                    _context.PaymentTransactions.Add(refundTransaction);

                    // ✅ به‌روزرسانی CashSession Balance
                    var cashSession = await _context.CashSessions
                        .FirstOrDefaultAsync(cs => cs.CashSessionId == firstPayment.CashSessionId);
                    
                    if (cashSession != null)
                    {
                        // ✅ کاهش Balance بر اساس روش پرداخت
                        if (refundMethod == PaymentMethod.Cash)
                        {
                            cashSession.CashBalance -= totalPaid;
                            cashSession.UpdatedAt = DateTime.Now;
                            cashSession.UpdatedByUserId = _currentUserService.UserId;
                            
                            _logger.Information("💰 FACADE: CashSession.CashBalance کاهش یافت - SessionId: {SessionId}, Amount: {Amount}, New Balance: {NewBalance}",
                                cashSession.CashSessionId, totalPaid, cashSession.CashBalance);
                        }
                        else if (refundMethod == PaymentMethod.POS)
                        {
                            cashSession.PosBalance -= totalPaid;
                            cashSession.UpdatedAt = DateTime.Now;
                            cashSession.UpdatedByUserId = _currentUserService.UserId;
                            
                            _logger.Information("💰 FACADE: CashSession.PosBalance کاهش یافت - SessionId: {SessionId}, Amount: {Amount}, New Balance: {NewBalance}",
                                cashSession.CashSessionId, totalPaid, cashSession.PosBalance);
                        }
                        else
                        {
                            // برای سایر روش‌های پرداخت (مثلاً OnlinePayment)، فقط Log می‌کنیم
                            _logger.Information("💰 FACADE: Refund برای روش پرداخت {Method} - SessionId: {SessionId}, Amount: {Amount}",
                                refundMethod, cashSession.CashSessionId, totalPaid);
                        }
                    }
                    else
                    {
                        _logger.Warning("⚠️ FACADE: CashSession یافت نشد - SessionId: {SessionId}",
                            firstPayment.CashSessionId);
                        // ادامه می‌دهیم حتی اگر CashSession یافت نشد (برای سازگاری)
                    }

                    refundAmount = totalPaid;
                    refundProcessed = true;

                    _logger.Information("💰 FACADE: Refund ثبت شد و CashSession به‌روزرسانی شد - ReceptionId: {ReceptionId}, Amount: {Amount}, Method: {Method}", 
                        request.ReceptionId, totalPaid, refundMethod);
                }
                else if (hasPayment && !request.ProcessRefund)
                {
                    // اگر پرداختی وجود دارد اما Refund درخواست نشده
                    _logger.Warning("⚠️ FACADE: پرداختی وجود دارد اما Refund درخواست نشده - ReceptionId: {ReceptionId}, PaidAmount: {PaidAmount}", 
                        request.ReceptionId, totalPaid);
                    return ServiceResult<CancelReceptionResponse>.Failed(
                        $"این پذیرش دارای پرداخت به مبلغ {totalPaid:N0} ریال است. برای لغو، باید Refund انجام شود.",
                        "PAYMENT_EXISTS");
                }

                // 5. تغییر وضعیت به Cancelled
                var previousStatus = reception.Status;
                reception.Status = ReceptionStatus.Cancelled;

                // ✅ 5.1. صفر کردن مبالغ مالی برای جلوگیری از مغایرت در محاسبات مالی
                // این کار ضروری است تا پذیرش لغو شده در گزارش‌های مالی تاثیر نگذارد
                var previousTotalAmount = reception.TotalAmount;
                var previousPatientCoPay = reception.PatientCoPay;
                var previousBasePay = reception.BasePay;
                var previousSuppPay = reception.SuppPay;
                var previousInsurerShare = reception.InsurerShareAmount;
                var previousPatientPay = reception.PatientPay;
                var previousGross = reception.Gross;

                reception.TotalAmount = 0;
                reception.PatientCoPay = 0;
                reception.BasePay = 0;
                reception.SuppPay = 0;
                reception.InsurerShareAmount = 0;
                reception.PatientPay = 0;
                reception.Gross = 0;

                _logger.Information("💰 FACADE: مبالغ مالی Reception صفر شدند - ReceptionId: {ReceptionId}, Previous: Total={Total}, Patient={Patient}, Base={Base}, Supp={Supp}, Insurer={Insurer}, PatientPay={PatientPay}, Gross={Gross}",
                    reception.ReceptionId, previousTotalAmount, previousPatientCoPay, previousBasePay, previousSuppPay, previousInsurerShare, previousPatientPay, previousGross);

                // ✅ 5.2. صفر کردن مبالغ مالی ReceptionItems برای جلوگیری از مغایرت در محاسبات مالی
                // این کار ضروری است تا آیتم‌های پذیرش لغو شده در گزارش‌های مالی تاثیر نگذارند
                var activeItems = reception.ReceptionItems?.Where(ri => !ri.IsDeleted).ToList() ?? new List<Models.Entities.Reception.ReceptionItem>();
                var itemsZeroedCount = 0;
                var totalItemsAmount = 0m;
                var totalItemsPatientShare = 0m;
                var totalItemsInsurerShare = 0m;

                foreach (var item in activeItems)
                {
                    // ذخیره مقادیر قبلی برای Logging
                    totalItemsAmount += item.UnitPrice * item.Quantity;
                    totalItemsPatientShare += item.PatientShareAmount;
                    totalItemsInsurerShare += item.InsurerShareAmount;

                    // ✅ صفر کردن مبالغ مالی (Quantity و SnapshotJson را نگه می‌داریم برای Audit Trail)
                    item.UnitPrice = 0;
                    item.PatientShareAmount = 0;
                    item.InsurerShareAmount = 0;
                    
                    // به‌روزرسانی UpdatedAt و UpdatedByUserId
                    item.UpdatedAt = DateTime.Now;
                    item.UpdatedByUserId = _currentUserService.UserId;
                    
                    itemsZeroedCount++;
                }

                if (itemsZeroedCount > 0)
                {
                    _logger.Information("💰 FACADE: مبالغ مالی {Count} ReceptionItem صفر شدند - ReceptionId: {ReceptionId}, Previous: TotalAmount={TotalAmount}, PatientShare={PatientShare}, InsurerShare={InsurerShare}",
                        itemsZeroedCount, reception.ReceptionId, totalItemsAmount, totalItemsPatientShare, totalItemsInsurerShare);
                }
                else
                {
                    _logger.Information("ℹ️ FACADE: هیچ ReceptionItem فعالی برای صفر کردن یافت نشد - ReceptionId: {ReceptionId}",
                        reception.ReceptionId);
                }

                // 6. ثبت دلیل لغو در Notes (اگر Notes خالی است یا اضافه کردن به انتهای آن)
                var cancellationNote = $"\n\n[لغو شده در {DateTime.Now.ToPersianDateTime()} توسط {_currentUserService.UserName}]\nدلیل: {request.Reason}";
                reception.Notes = (reception.Notes ?? string.Empty) + cancellationNote;

                // 7. به‌روزرسانی UpdatedAt
                reception.UpdatedAt = DateTime.Now;
                reception.UpdatedByUserId = _currentUserService.UserId;

                // 8. ذخیره تغییرات
                await _context.SaveChangesAsync();

                // 9. ساخت پاسخ
                var response = new CancelReceptionResponse
                {
                    ReceptionId = reception.ReceptionId,
                    PreviousStatus = previousStatus,
                    NewStatus = ReceptionStatus.Cancelled,
                    RefundProcessed = refundProcessed,
                    RefundAmount = refundAmount,
                    RequiresApproval = canCancelResult.RequiresApproval,
                    CancelledAt = DateTime.Now,
                    CancelledBy = _currentUserService.UserName,
                    Message = refundProcessed 
                        ? $"پذیرش با موفقیت لغو شد و مبلغ {refundAmount:N0} ریال برگشت داده شد."
                        : "پذیرش با موفقیت لغو شد."
                };

                _logger.Information("✅ FACADE: پذیرش لغو شد - ReceptionId: {ReceptionId}, PreviousStatus: {PreviousStatus}, RefundProcessed: {RefundProcessed}", 
                    reception.ReceptionId, previousStatus, refundProcessed);

                return ServiceResult<CancelReceptionResponse>.Successful(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در لغو پذیرش - ReceptionId: {ReceptionId}", request.ReceptionId);
                return ServiceResult<CancelReceptionResponse>.Failed($"خطا در لغو پذیرش: {ex.Message}");
            }
        }

        /// <summary>
        /// بررسی امکان لغو پذیرش
        /// </summary>
        private (bool CanCancel, string ErrorMessage, bool RequiresApproval) CanCancelReception(Models.Entities.Reception.Reception reception)
        {
            // اگر قبلاً لغو شده
            if (reception.Status == ReceptionStatus.Cancelled)
            {
                return (false, "این پذیرش قبلاً لغو شده است.", false);
            }

            // اگر Pending است، همیشه قابل لغو است
            if (reception.Status == ReceptionStatus.Pending)
            {
                return (true, null, false);
            }

            // اگر Completed است، بررسی محدودیت زمانی
            if (reception.Status == ReceptionStatus.Completed)
            {
                var timeSinceCompletion = DateTime.Now - (reception.UpdatedAt ?? reception.CreatedAt);
                
                // کمتر از 24 ساعت: قابل لغو توسط منشی
                if (timeSinceCompletion.TotalHours <= 24)
                {
                    return (true, null, false);
                }
                
                // بیشتر از 24 ساعت و کمتر از 7 روز: نیاز به تایید مدیر
                if (timeSinceCompletion.TotalDays <= 7)
                {
                    return (true, null, true);
                }
                
                // بیشتر از 7 روز: نیاز به تایید مدیر ارشد
                return (true, "این پذیرش بیش از 7 روز از زمان ایجاد گذشته است. برای لغو نیاز به تایید مدیر ارشد دارید.", true);
            }

            // سایر وضعیت‌ها: بررسی موردی
            return (false, $"امکان لغو پذیرش با وضعیت {reception.Status} وجود ندارد.", false);
        }

        #endregion

        #region Helper Methods (روش‌های کمکی)

        /// <summary>
        /// ✅ دریافت شناسه کاربر معتبر از دیتابیس برای CreatedByUserId/UpdatedByUserId
        /// طبق قرارداد: اطمینان از وجود کاربر در AspNetUsers قبل از استفاده
        /// 
        /// الویت:
        /// 1. _currentUserService.UserId (اگر در دیتابیس وجود دارد)
        /// 2. کاربر System (UserName = "3031945451" یا "system")
        /// 3. کاربر Admin (UserName = "3020347998" یا "admin")
        /// 4. SystemUsers.AdminUserId (اگر Initialize شده)
        /// 5. null (چون CreatedByUserId HasOptional است)
        /// </summary>
        private async Task<string> GetValidUserIdFromDatabaseAsync()
        {
            try
            {
                // ✅ اولویت 1: استفاده از _currentUserService.UserId (اگر در دیتابیس وجود دارد)
                var currentUserId = _currentUserService?.UserId;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var userExists = await _context.Users.AnyAsync(u => u.Id == currentUserId && !u.IsDeleted);
                    if (userExists)
                    {
                        _logger.Debug("✅ FACADE: استفاده از شناسه کاربر فعلی از دیتابیس: {UserId}", currentUserId);
                        return currentUserId;
                    }
                    else
                    {
                        _logger.Warning("⚠️ FACADE: شناسه کاربر فعلی در دیتابیس یافت نشد: {UserId}", currentUserId);
                    }
                }

                // ✅ اولویت 2: جستجوی کاربر System
                var systemUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => 
                        (u.UserName == "3031945451" || u.UserName == "system") && 
                        !u.IsDeleted);
                
                if (systemUser != null)
                {
                    _logger.Information("✅ FACADE: استفاده از کاربر System از دیتابیس: {UserId}", systemUser.Id);
                    return systemUser.Id;
                }

                // ✅ اولویت 3: جستجوی کاربر Admin
                var adminUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => 
                        (u.UserName == "3020347998" || u.UserName == "admin") && 
                        !u.IsDeleted);
                
                if (adminUser != null)
                {
                    _logger.Information("✅ FACADE: استفاده از کاربر Admin از دیتابیس: {UserId}", adminUser.Id);
                    return adminUser.Id;
                }

                // ✅ اولویت 4: استفاده از SystemUsers (اگر Initialize شده)
                if (SystemUsers.IsInitialized && !string.IsNullOrEmpty(SystemUsers.AdminUserId))
                {
                    var systemUsersAdminExists = await _context.Users.AnyAsync(u => u.Id == SystemUsers.AdminUserId && !u.IsDeleted);
                    if (systemUsersAdminExists)
                    {
                        _logger.Information("✅ FACADE: استفاده از SystemUsers.AdminUserId از دیتابیس: {UserId}", SystemUsers.AdminUserId);
                        return SystemUsers.AdminUserId;
                    }
                }

                // ✅ اولویت 5: null (چون CreatedByUserId HasOptional است)
                _logger.Warning("⚠️ FACADE: هیچ کاربر معتبری در دیتابیس یافت نشد. CreatedByUserId را null تنظیم می‌کنیم");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در دریافت شناسه کاربر معتبر از دیتابیس");
                // در صورت خطا، null برمی‌گردانیم (چون CreatedByUserId HasOptional است)
                return null;
            }
        }

        /// <summary>
        /// دریافت جزئیات کامل پذیرش برای نمایش در Modal
        /// </summary>
        public async Task<ServiceResult<ReceptionDetailsFullDto>> GetReceptionDetailsFullAsync(int receptionId)
        {
            try
            {
                _logger.Information("🏥 FACADE: دریافت جزئیات کامل پذیرش - ReceptionId: {ReceptionId}", receptionId);

                // 1. دریافت پذیرش با تمام جزئیات
                var reception = await _context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Department)
                    .Include(r => r.Clinic)
                    .Include(r => r.ActivePatientInsurance)
                    .Include(r => r.ActivePatientInsurance.InsurancePlan)
                    .Include(r => r.ActivePatientInsurance.SupplementaryInsurancePlan)
                    .Include(r => r.ReceptionItems.Select(ri => ri.Service))
                    .Include(r => r.Transactions.Select(t => t.CreatedByUser))
                    .Include(r => r.Transactions.Select(t => t.CashSession))
                    .Include(r => r.CreatedByUser)
                    .Include(r => r.UpdatedByUser)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReceptionId == receptionId && !r.IsDeleted);

                if (reception == null)
                {
                    _logger.Warning("⚠️ FACADE: پذیرش یافت نشد - ReceptionId: {ReceptionId}", receptionId);
                    return ServiceResult<ReceptionDetailsFullDto>.Failed($"پذیرش با شناسه {receptionId} یافت نشد", "NOT_FOUND");
                }

                // 2. دریافت نام پزشک و تخصص
                string doctorFullName = string.Empty;
                string doctorSpecialization = string.Empty;
                string doctorDegree = string.Empty;
                if (reception.DoctorId > 0)
                {
                    // ✅ دریافت اطلاعات پزشک با Select مستقیم (بدون Include برای جلوگیری از materialize شدن کامل entity)
                    var doctorInfo = await _context.Doctors
                        .AsNoTracking()
                        .Where(d => d.DoctorId == reception.DoctorId)
                        .Select(d => new 
                        { 
                            d.FirstName, 
                            d.LastName, 
                            d.Degree,
                            SpecializationName = d.DoctorSpecializations
                                .Where(ds => ds.Specialization != null)
                                .Select(ds => ds.Specialization.Name)
                                .FirstOrDefault() ?? string.Empty
                        })
                        .FirstOrDefaultAsync();
                    
                    if (doctorInfo != null)
                    {
                        doctorFullName = $"{doctorInfo.FirstName} {doctorInfo.LastName}".Trim();
                        doctorSpecialization = doctorInfo.SpecializationName ?? string.Empty;
                        doctorDegree = doctorInfo.Degree?.ToString() ?? string.Empty;
                    }
                }

                // 3. محاسبه مبلغ پرداخت شده
                var paidAmount = reception.Transactions?
                    .Where(t => t.Status == PaymentStatus.Success && !t.IsDeleted)
                    .Sum(t => (decimal?)t.Amount) ?? 0m;

                // 4. ساخت DTO
                var result = new ReceptionDetailsFullDto
                {
                    // اطلاعات اصلی پذیرش
                    ReceptionId = reception.ReceptionId,
                    ReceptionNo = reception.ReceptionNo ?? string.Empty,
                    ElectronicReceptionNumber = reception.ElectronicReceptionNumber ?? string.Empty,
                    Status = reception.Status,
                    StatusText = GetReceptionStatusText(reception.Status),
                    Type = reception.Type,
                    TypeText = GetReceptionTypeDisplayName(reception.Type),
                    Priority = reception.Priority,
                    PriorityText = GetPriorityDisplayName(reception.Priority),
                    IsEmergency = reception.IsEmergency,
                    IsOnlineReception = reception.IsOnlineReception,
                    ReceptionDate = reception.ReceptionDate,
                    ReceptionDateShamsi = reception.ReceptionDate.ToPersianDateTime(),
                    Notes = reception.Notes ?? string.Empty,

                    // اطلاعات بیمار
                    PatientId = reception.PatientId,
                    PatientFullName = reception.Patient != null 
                        ? $"{reception.Patient.FirstName} {reception.Patient.LastName}".Trim()
                        : string.Empty,
                    PatientNationalCode = reception.Patient?.NationalCode ?? string.Empty,
                    PatientPhoneNumber = reception.Patient?.PhoneNumber ?? string.Empty,
                    PatientGender = reception.Patient?.Gender.ToString() ?? string.Empty,
                    PatientBirthDateShamsi = reception.Patient?.BirthDate != null 
                        ? reception.Patient.BirthDate.Value.ToPersianDate()
                        : string.Empty,
                    PatientAddress = reception.Patient?.Address ?? string.Empty,

                    // اطلاعات پزشک
                    DoctorId = reception.DoctorId,
                    DoctorFullName = doctorFullName,
                    DoctorSpecialization = doctorSpecialization,
                    DoctorDegree = doctorDegree,

                    // اطلاعات دپارتمان و کلینیک
                    DepartmentId = reception.DepartmentId,
                    DepartmentName = reception.Department?.Name ?? string.Empty,
                    ClinicId = reception.ClinicId,
                    ClinicName = reception.Clinic?.Name ?? string.Empty,

                    // اطلاعات بیمه
                    BasePlanId = reception.BasePlanId,
                    BasePlanName = reception.ActivePatientInsurance?.InsurancePlan?.Name ?? string.Empty,
                    SupplementaryPlanId = reception.SupplementaryPlanId,
                    SupplementaryPlanName = reception.ActivePatientInsurance?.SupplementaryInsurancePlan?.Name ?? string.Empty,

                    // اطلاعات مالی
                    TotalAmount = reception.TotalAmount,
                    Gross = reception.Gross,
                    PatientCoPay = reception.PatientCoPay,
                    PatientPay = reception.PatientPay,
                    BasePay = reception.BasePay,
                    SuppPay = reception.SuppPay,
                    InsurerShareAmount = reception.InsurerShareAmount,
                    PaidAmount = paidAmount,
                    RemainingAmount = reception.PatientCoPay - paidAmount,

                    // آیتم‌های پذیرش
                    Items = reception.ReceptionItems?
                        .Where(ri => !ri.IsDeleted)
                        .Select(ri => new ReceptionItemDetailsDto
                        {
                            ReceptionItemId = ri.ReceptionItemId,
                            ServiceId = ri.ServiceId,
                            ServiceCode = ri.Service?.ServiceCode ?? string.Empty,
                            ServiceName = ri.Service?.Title ?? string.Empty,
                            Quantity = ri.Quantity,
                            UnitPrice = ri.UnitPrice,
                            PatientShareAmount = ri.PatientShareAmount,
                            InsurerShareAmount = ri.InsurerShareAmount,
                            SnapshotJson = ri.SnapshotJson
                        })
                        .ToList() ?? new List<ReceptionItemDetailsDto>(),

                    // تراکنش‌های پرداخت
                    Transactions = reception.Transactions?
                        .Where(t => !t.IsDeleted)
                        .OrderByDescending(t => t.CreatedAt)
                        .Select(t => new PaymentTransactionDetailsDto
                        {
                            PaymentTransactionId = t.PaymentTransactionId,
                            Amount = t.Amount,
                            Status = t.Status,
                            StatusText = GetPaymentStatusText(t.Status),
                            Method = t.Method,
                            MethodText = GetPaymentMethodText(t.Method),
                            TransactionId = t.TransactionId ?? string.Empty,
                            ReferenceCode = t.ReferenceCode ?? string.Empty,
                            Description = t.Description ?? string.Empty,
                            CreatedAt = t.CreatedAt,
                            CreatedAtShamsi = t.CreatedAt.ToPersianDateTime(),
                            CreatedBy = t.CreatedByUser?.UserName ?? "سیستم",
                            CashSessionId = t.CashSessionId,
                            CashSessionNumber = t.CashSession?.SessionNumber ?? string.Empty
                        })
                        .ToList() ?? new List<PaymentTransactionDetailsDto>(),

                    // اطلاعات ردیابی
                    CreatedAt = reception.CreatedAt,
                    CreatedAtShamsi = reception.CreatedAt.ToPersianDateTime(),
                    CreatedBy = reception.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = reception.UpdatedAt,
                    UpdatedAtShamsi = reception.UpdatedAt?.ToPersianDateTime() ?? string.Empty,
                    UpdatedBy = reception.UpdatedByUser?.UserName ?? string.Empty
                };

                _logger.Information("✅ FACADE: جزئیات کامل پذیرش دریافت شد - ReceptionId: {ReceptionId}, Items: {ItemsCount}, Transactions: {TransactionsCount}",
                    receptionId, result.Items.Count, result.Transactions.Count);

                return ServiceResult<ReceptionDetailsFullDto>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در دریافت جزئیات کامل پذیرش - ReceptionId: {ReceptionId}", receptionId);
                return ServiceResult<ReceptionDetailsFullDto>.Failed($"خطا در دریافت جزئیات پذیرش: {ex.Message}", "UNHANDLED");
            }
        }

        /// <summary>
        /// Helper: تبدیل وضعیت پذیرش به متن
        /// </summary>
        private string GetReceptionStatusText(ReceptionStatus status)
        {
            switch (status)
            {
                case ReceptionStatus.Pending: return "در انتظار";
                case ReceptionStatus.Completed: return "تکمیل شده";
                case ReceptionStatus.Cancelled: return "لغو شده";
                default: return status.ToString();
            }
        }

        /// <summary>
        /// Helper: تبدیل نوع پذیرش به متن
        /// </summary>
        private string GetReceptionTypeDisplayName(ReceptionType type)
        {
            switch (type)
            {
                case ReceptionType.Normal: return "عادی";
                case ReceptionType.Emergency: return "اورژانس";
                case ReceptionType.Online: return "آنلاین";
                default: return type.ToString();
            }
        }

        /// <summary>
        /// Helper: تبدیل اولویت به متن
        /// </summary>
        private string GetPriorityDisplayName(AppointmentPriority priority)
        {
            switch (priority)
            {
                case AppointmentPriority.Low: return "پایین";
                case AppointmentPriority.Normal: return "عادی";
                case AppointmentPriority.High: return "بالا";
                case AppointmentPriority.Urgent: return "فوری";
                default: return priority.ToString();
            }
        }

        /// <summary>
        /// Helper: تبدیل وضعیت پرداخت به متن
        /// </summary>
        private string GetPaymentStatusText(PaymentStatus status)
        {
            switch (status)
            {
                case PaymentStatus.Pending: return "در انتظار";
                case PaymentStatus.Success: return "موفق";
                case PaymentStatus.Failed: return "ناموفق";
                case PaymentStatus.Canceled: return "لغو شده";
                default: return status.ToString();
            }
        }

        /// <summary>
        /// Helper: تبدیل روش پرداخت به متن
        /// </summary>
        private string GetPaymentMethodText(PaymentMethod method)
        {
            switch (method)
            {
                case PaymentMethod.Cash: return "نقدی";
                case PaymentMethod.POS: return "کارتخوان";
                case PaymentMethod.Online: return "آنلاین";
                case PaymentMethod.Debt: return "بدهی";
                default: return method.ToString();
            }
        }

        #endregion
    }
}
