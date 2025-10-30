using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.Finance;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Insurance;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using Serilog;

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

                // 1. بارگذاری دپارتمان‌ها
                var departmentsResult = await _departmentManagementService.GetAllDepartmentsAsync();
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
                }

                // 2. بارگذاری خدمات دپارتمان (اگر انتخاب شده)
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

                // 3. بارگذاری خدمات مشترک
                var sharedServicesResult = await GetSharedServicesAsync();
                if (sharedServicesResult.Success)
                {
                    result.SharedServices = sharedServicesResult.Data;
                }

                _logger.Information("✅ FACADE: بارگذاری اولیه تکمیل شد - Departments: {DeptCount}, Services: {ServiceCount}", 
                    result.Departments?.Count ?? 0, result.Services?.Count ?? 0);

                return ServiceResult<ReceptionLoadDto>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در بارگذاری اولیه فرم پذیرش");
                return ServiceResult<ReceptionLoadDto>.Failed("خطا در بارگذاری اولیه فرم پذیرش");
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
                        PhoneNumber = dtoIfNotExists.PhoneNumber,
                        Email = dtoIfNotExists.Email,
                        BirthDate = dtoIfNotExists.BirthDate,
                        Gender = Enum.TryParse<Gender>(dtoIfNotExists.Gender, out var gender) ? gender : Gender.Unknown,
                        Address = dtoIfNotExists.Address
                    };

                    var createResult = await _patientService.CreatePatientAsync(createViewModel);
                    if (createResult.Success)
                    {
                        // Since CreatePatientAsync returns ServiceResult (not ServiceResult<T>), 
                        // we need to create the PatientDto from the input data
                        return ServiceResult<PatientDto>.Successful(new PatientDto
                        {
                            PatientId = 0, // Will be set by the service
                            NationalCode = dtoIfNotExists.NationalCode,
                            FirstName = dtoIfNotExists.FirstName,
                            LastName = dtoIfNotExists.LastName,
                            PhoneNumber = dtoIfNotExists.PhoneNumber,
                            Email = dtoIfNotExists.Email
                        });
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

                // var year = _financialYearService.GetCurrentYear(); // TODO: Add FinancialYear field to Reception

                var draft = new Models.Entities.Reception.Reception
                {
                    PatientId = request.PatientId,
                    DoctorId = request.DoctorId,
                    ReceptionDate = DateTime.Now,
                    Status = ReceptionStatus.Pending, // Draft status
                    Type = ReceptionType.Normal,
                    Priority = AppointmentPriority.Normal,
                    TotalAmount = 0,
                    PatientCoPay = 0,
                    InsurerShareAmount = 0,
                    FinancialYear = 1404 // TODO: Get from DbFinancialYearService
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

                // var year = draft.FinancialYear; // TODO: Add FinancialYear field to Reception
                var year = 1404; // Default year for now

                // دریافت ضرایب
                var factor = await _context.FactorSettings
                    .Where(f => f.FinancialYear == year && !f.IsDeleted)
                    .Select(f => new { f.Value, f.FactorType })
                    .ToListAsync();

                var proFactor = factor.FirstOrDefault(f => f.FactorType == ServiceComponentType.Professional)?.Value ?? 0;
                var techFactor = factor.FirstOrDefault(f => f.FactorType == ServiceComponentType.Technical)?.Value ?? 0;

                // دریافت اطلاعات خدمت
        var service = await _context.Services
            .Where(s => s.ServiceId == request.ServiceId && s.IsActive && !s.IsDeleted)
            .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
            .FirstOrDefaultAsync();

                if (service == null)
                    return ServiceResult<ItemsAndTotalsDto>.Failed("خدمت یافت نشد");

                var qty = request.Quantity <= 0 ? 1 : request.Quantity;
                // TODO: محاسبه قیمت بر اساس ServiceComponents
                var unit = 1000m; // قیمت ثابت موقت
                var total = unit * qty;

                // محاسبه سهم بیمار و بیمه برای این آیتم
                var itemBasePercent = 0m;
                var itemSuppPercent = 0m;

                // دریافت اطلاعات بیمه پایه
                if (draft.BasePlanId.HasValue)
                {
                    var basePlan = await _context.InsurancePlans
                        .Where(p => p.InsurancePlanId == draft.BasePlanId.Value && !p.IsDeleted && p.IsActive)
                        .Select(p => new { p.CoveragePercent })
                        .FirstOrDefaultAsync();
                    
                    if (basePlan != null)
                    {
                        itemBasePercent = basePlan.CoveragePercent;
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
                        itemSuppPercent = suppPlan.CoveragePercent;
                    }
                }

                // محاسبه سهم بیمه پایه
                var itemBasePay = total * (itemBasePercent / 100m);
                var itemPatientAfterBase = total - itemBasePay;
                
                // محاسبه سهم بیمه تکمیلی (از مبلغ باقی‌مانده)
                var itemSuppPay = itemPatientAfterBase * (itemSuppPercent / 100m);
                var itemPatientShare = itemPatientAfterBase - itemSuppPay;

                var item = new Models.Entities.Reception.ReceptionItem
                {
                    ReceptionId = draft.ReceptionId,
                    ServiceId = service.ServiceId,
                    Quantity = qty,
                    UnitPrice = unit,
                    PatientShareAmount = itemPatientShare,
                    InsurerShareAmount = itemBasePay + itemSuppPay
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
                _logger.Information("🏥 FACADE: تنظیم بیمه‌های پیش‌نویس");

                var draft = await _context.Receptions
                    .Include(d => d.ReceptionItems)
                    .FirstOrDefaultAsync(d => d.ReceptionId == request.ReceptionId && d.Status == ReceptionStatus.Pending);
                
                if (draft == null)
                    return ServiceResult<ItemsAndTotalsDto>.Failed("پیش‌نویس یافت نشد");

                draft.BasePlanId = request.BasePlanId;
                draft.SupplementaryPlanId = request.SupplementaryPlanId;
                draft.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();

                return await RecalculateDraftAsync(draft);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACADE: خطا در تنظیم بیمه‌ها");
                return ServiceResult<ItemsAndTotalsDto>.Failed("خطا در تنظیم بیمه‌ها");
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

                // بررسی وجود پرداخت قبلی - TODO: Add IdempotencyKey field to PaymentTransaction
                // var exists = await _context.PaymentTransactions.AnyAsync(p => p.IdempotencyKey == request.IdempotencyKey);
                // if (exists)
                // {
                //     return ServiceResult<FinalizeResponse>.Failed("پرداخت قبلاً انجام شده است");
                // }

                var draft = await _context.Receptions
                    .Include(d => d.ReceptionItems)
                    .FirstOrDefaultAsync(d => d.ReceptionId == request.ReceptionId && d.Status == ReceptionStatus.Pending);
                
                if (draft == null)
                    return ServiceResult<FinalizeResponse>.Failed("پیش‌نویس یافت نشد");

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
                draft.Status = ReceptionStatus.Completed; // TODO: Add enum value
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

                // بررسی وجود پرداخت قبلی - TODO: Add IdempotencyKey field to PaymentTransaction
                // var exists = await _context.PaymentTransactions.AnyAsync(p => p.IdempotencyKey == request.IdempotencyKey);
                // if (exists)
                // {
                //     return ServiceResult<FinalizeResponse>.Failed("پرداخت قبلاً انجام شده است");
                // }

                var draft = await _context.Receptions
                    .Include(d => d.ReceptionItems)
                    .FirstOrDefaultAsync(d => d.ReceptionId == request.ReceptionId && d.Status == ReceptionStatus.Pending);
                
                if (draft == null)
                    return ServiceResult<FinalizeResponse>.Failed("پیش‌نویس یافت نشد");

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
                draft.Status = ReceptionStatus.Completed; // TODO: Add enum value
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
    }
}
