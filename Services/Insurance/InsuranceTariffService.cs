using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس مدیریت تعرفه‌های بیمه
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// </summary>
    public class InsuranceTariffService : IInsuranceTariffService
    {
        private readonly IInsuranceTariffRepository _tariffRepository;
        private readonly IInsurancePlanRepository _planRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public InsuranceTariffService(
            IInsuranceTariffRepository tariffRepository,
            IInsurancePlanRepository planRepository,
            IServiceRepository serviceRepository,
            IServiceCalculationService serviceCalculationService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _tariffRepository = tariffRepository ?? throw new ArgumentNullException(nameof(tariffRepository));
            _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region CRUD Operations

        /// <summary>
        /// دریافت تعرفه‌های بیمه با صفحه‌بندی
        /// </summary>
        public async Task<ServiceResult<PagedResult<InsuranceTariffIndexViewModel>>> GetTariffsAsync(
            int? planId = null,
            int? serviceId = null,
            int? providerId = null,
            string searchTerm = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                _logger.Information("درخواست دریافت تعرفه‌های بیمه. PlanId: {PlanId}, ServiceId: {ServiceId}, ProviderId: {ProviderId}, SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                    planId, serviceId, providerId, searchTerm, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _tariffRepository.GetPagedAsync(planId, serviceId, providerId, searchTerm, pageNumber, pageSize);
                
                var viewModels = result.Items.Select(InsuranceTariffIndexViewModel.FromEntity).ToList();
                
                var pagedResult = new PagedResult<InsuranceTariffIndexViewModel>
                {
                    Items = viewModels,
                    TotalItems = result.TotalItems,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };

                _logger.Information("تعرفه‌های بیمه با موفقیت دریافت شد. Count: {Count}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    viewModels.Count, pageNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<InsuranceTariffIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<PagedResult<InsuranceTariffIndexViewModel>>.Failed("خطا در دریافت تعرفه‌های بیمه");
            }
        }

        /// <summary>
        /// دریافت جزئیات تعرفه بیمه
        /// </summary>
        public async Task<ServiceResult<InsuranceTariffDetailsViewModel>> GetTariffDetailsAsync(int id)
        {
            try
            {
                _logger.Information("درخواست جزئیات تعرفه بیمه. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var tariff = await _tariffRepository.GetByIdWithDetailsAsync(id);
                if (tariff == null)
                {
                    _logger.Warning("تعرفه بیمه یافت نشد. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<InsuranceTariffDetailsViewModel>.Failed("تعرفه بیمه یافت نشد");
                }

                var viewModel = InsuranceTariffDetailsViewModel.FromEntity(tariff);

                _logger.Information("جزئیات تعرفه بیمه با موفقیت دریافت شد. Id: {Id}, ServiceTitle: {ServiceTitle}, PlanName: {PlanName}. User: {UserName} (Id: {UserId})",
                    id, tariff.Service?.Title, tariff.InsurancePlan?.Name, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceTariffDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات تعرفه بیمه. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<InsuranceTariffDetailsViewModel>.Failed("خطا در دریافت جزئیات تعرفه بیمه");
            }
        }

        /// <summary>
        /// دریافت تعرفه بیمه برای ویرایش
        /// </summary>
        public async Task<ServiceResult<InsuranceTariffCreateEditViewModel>> GetTariffForEditAsync(int id)
        {
            try
            {
                _logger.Information("درخواست تعرفه بیمه برای ویرایش. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var tariff = await _tariffRepository.GetByIdWithDetailsAsync(id);
                if (tariff == null)
                {
                    _logger.Warning("تعرفه بیمه یافت نشد. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<InsuranceTariffCreateEditViewModel>.Failed("تعرفه بیمه یافت نشد");
                }

                var viewModel = InsuranceTariffCreateEditViewModel.FromEntity(tariff);

                _logger.Information("تعرفه بیمه برای ویرایش با موفقیت دریافت شد. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceTariffCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه بیمه برای ویرایش. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<InsuranceTariffCreateEditViewModel>.Failed("خطا در دریافت تعرفه بیمه برای ویرایش");
            }
        }

        /// <summary>
        /// ایجاد تعرفه بیمه جدید - با پشتیبانی Transaction
        /// </summary>
        public async Task<ServiceResult<int>> CreateTariffAsync(InsuranceTariffCreateEditViewModel model)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست ایجاد تعرفه بیمه جدید. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی
                var validationResult = await ValidateTariffAsync(model);
                if (!validationResult.Success)
                {
                    var errorMessages = validationResult.Data?.Values != null 
                        ? string.Join(", ", validationResult.Data.Values) 
                        : "خطاهای نامشخص";
                    
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی تعرفه بیمه ناموفق. PlanId: {PlanId}, ServiceId: {ServiceId}, Errors: {Errors}. User: {UserName} (Id: {UserId})",
                        model.InsurancePlanId, model.ServiceId, errorMessages, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<int>.Failed("اطلاعات وارد شده معتبر نیست");
                }

                // بررسی وجود تعرفه مشابه (فقط اگر ServiceId مشخص باشد)
                if (model.ServiceId.HasValue)
                {
                    var exists = await _tariffRepository.DoesTariffExistAsync(model.InsurancePlanId, model.ServiceId.Value);
                    if (exists)
                    {
                        _logger.Warning("🏥 MEDICAL: تعرفه بیمه مشابه وجود دارد. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                        return ServiceResult<int>.Failed("تعرفه بیمه برای این طرح و خدمت قبلاً تعریف شده است");
                    }
                }

                // محاسبات داینامیک
                var calculatedValues = await CalculateTariffValuesAsync(model);
                
                // دریافت نوع بیمه از طرح بیمه
                var insurancePlan = await _planRepository.GetByIdAsync(model.InsurancePlanId);
                var insuranceType = insurancePlan?.InsuranceType ?? (InsuranceType)1; // پیش‌فرض بیمه پایه
                
                _logger.Information("🏥 MEDICAL: نوع بیمه تعیین شد - PlanId: {PlanId}, InsuranceType: {InsuranceType}. User: {UserName} (Id: {UserId})",
                    model.InsurancePlanId, insuranceType, _currentUserService.UserName, _currentUserService.UserId);
                
                // ایجاد entity
                var tariff = new InsuranceTariff
                {
                    ServiceId = model.ServiceId ?? 0, // 0 برای "همه خدمات"
                    InsurancePlanId = model.InsurancePlanId,
                    InsuranceType = insuranceType, // ✅ تنظیم نوع بیمه از طرح بیمه
                    TariffPrice = calculatedValues.TariffPrice,
                    PatientShare = calculatedValues.PatientShare,
                    InsurerShare = calculatedValues.InsurerShare,
                    // 🔧 CRITICAL FIX: اضافه کردن فیلدهای بیمه تکمیلی
                    SupplementaryCoveragePercent = model.SupplementaryCoveragePercent,
                    SupplementaryMaxPayment = model.SupplementaryMaxPayment,
                    SupplementaryDeductible = model.SupplementaryDeductible,
                    MinPatientCopay = model.MinPatientCopay,
                    Priority = model.Priority,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Notes = model.Notes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId,
                    IsDeleted = false
                };

                // استفاده از Transaction برای تضمین یکپارچگی (Entity Framework 6)
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _logger.Information("🏥 MEDICAL: شروع Transaction برای ایجاد تعرفه. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                        var result = await _tariffRepository.AddAsync(tariff);
                        
                        // Commit Transaction
                        transaction.Commit();
                        
                        _logger.Information("🏥 MEDICAL: تعرفه بیمه جدید با موفقیت ایجاد شد. Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            result.InsuranceTariffId, model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                        return ServiceResult<int>.Successful(result.InsuranceTariffId, "تعرفه بیمه جدید با موفقیت ایجاد شد");
                    }
                    catch (Exception ex)
                    {
                        // Rollback Transaction در صورت خطا
                        transaction.Rollback();
                        _logger.Error(ex, "🏥 MEDICAL: خطا در Transaction - Rollback انجام شد. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه بیمه جدید. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<int>.Failed("خطا در ایجاد تعرفه بیمه جدید");
            }
        }

        /// <summary>
        /// به‌روزرسانی تعرفه بیمه
        /// </summary>
        public async Task<ServiceResult> UpdateTariffAsync(InsuranceTariffCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی تعرفه بیمه. Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.InsuranceTariffId, model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی
                var validationResult = await ValidateTariffAsync(model);
                if (!validationResult.Success)
                {
                    var errorMessages = validationResult.Data?.Values != null 
                        ? string.Join(", ", validationResult.Data.Values) 
                        : "خطاهای نامشخص";
                    
                    _logger.Warning("اعتبارسنجی تعرفه بیمه ناموفق. Id: {Id}, Errors: {Errors}. User: {UserName} (Id: {UserId})",
                        model.InsuranceTariffId, errorMessages, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("اطلاعات وارد شده معتبر نیست");
                }

                // دریافت تعرفه موجود
                var existingTariff = await _tariffRepository.GetByIdAsync(model.InsuranceTariffId);
                if (existingTariff == null)
                {
                    _logger.Warning("تعرفه بیمه یافت نشد. Id: {Id}. User: {UserName} (Id: {UserId})",
                        model.InsuranceTariffId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("تعرفه بیمه یافت نشد");
                }

                // بررسی وجود تعرفه مشابه (به جز خودش) - فقط اگر ServiceId مشخص باشد
                if (model.ServiceId.HasValue)
                {
                    var exists = await _tariffRepository.DoesTariffExistAsync(model.InsurancePlanId, model.ServiceId.Value, model.InsuranceTariffId);
                    if (exists)
                    {
                        _logger.Warning("تعرفه بیمه مشابه وجود دارد. Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            model.InsuranceTariffId, model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                        return ServiceResult.Failed("تعرفه بیمه برای این طرح و خدمت قبلاً تعریف شده است");
                    }
                }

                // محاسبات داینامیک
                var calculatedValues = await CalculateTariffValuesAsync(model);
                
                // به‌روزرسانی
                existingTariff.ServiceId = model.ServiceId ?? 0; // 0 برای "همه خدمات"
                existingTariff.InsurancePlanId = model.InsurancePlanId;
                existingTariff.TariffPrice = calculatedValues.TariffPrice;
                existingTariff.PatientShare = calculatedValues.PatientShare;
                existingTariff.InsurerShare = calculatedValues.InsurerShare;
                // 🔧 CRITICAL FIX: اضافه کردن فیلدهای بیمه تکمیلی
                existingTariff.SupplementaryCoveragePercent = model.SupplementaryCoveragePercent;
                existingTariff.SupplementaryMaxPayment = model.SupplementaryMaxPayment;
                existingTariff.SupplementaryDeductible = model.SupplementaryDeductible;
                existingTariff.MinPatientCopay = model.MinPatientCopay;
                existingTariff.Priority = model.Priority;
                existingTariff.StartDate = model.StartDate;
                existingTariff.EndDate = model.EndDate;
                existingTariff.Notes = model.Notes;
                existingTariff.UpdatedAt = DateTime.UtcNow;
                existingTariff.UpdatedByUserId = _currentUserService.UserId;

                await _tariffRepository.UpdateAsync(existingTariff);

                _logger.Information("تعرفه بیمه با موفقیت به‌روزرسانی شد. Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.InsuranceTariffId, model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("تعرفه بیمه با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تعرفه بیمه. Id: {Id}. User: {UserName} (Id: {UserId})",
                    model.InsuranceTariffId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در به‌روزرسانی تعرفه بیمه");
            }
        }

        /// <summary>
        /// حذف نرم تعرفه بیمه
        /// </summary>
        public async Task<ServiceResult> SoftDeleteTariffAsync(int id)
        {
            try
            {
                _logger.Information("درخواست حذف تعرفه بیمه. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _tariffRepository.SoftDeleteAsync(id, _currentUserService.UserId);
                if (!result)
                {
                    _logger.Warning("تعرفه بیمه یافت نشد یا قبلاً حذف شده. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("تعرفه بیمه یافت نشد");
                }

                _logger.Information("تعرفه بیمه با موفقیت حذف شد. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("تعرفه بیمه با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تعرفه بیمه. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در حذف تعرفه بیمه");
            }
        }

        /// <summary>
        /// بررسی وجود تعرفه برای خدمت و طرح بیمه مشخص
        /// </summary>
        public async Task<ServiceResult<bool>> CheckTariffExistsAsync(int serviceId, int planId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: بررسی وجود تعرفه - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    serviceId, planId, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی برای محیط درمانی
                if (serviceId <= 0)
                {
                    _logger.Warning("🏥 MEDICAL: شناسه خدمت نامعتبر - ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<bool>.Failed("شناسه خدمت نامعتبر است");
                }

                if (planId <= 0)
                {
                    _logger.Warning("🏥 MEDICAL: شناسه طرح بیمه نامعتبر - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<bool>.Failed("شناسه طرح بیمه نامعتبر است");
                }

                var existingTariff = await _tariffRepository.GetByPlanAndServiceAsync(planId, serviceId);
                var exists = existingTariff != null;

                _logger.Information("🏥 MEDICAL: بررسی وجود تعرفه تکمیل شد - Exists: {Exists}, ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    exists, serviceId, planId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(exists);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در بررسی وجود تعرفه - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    serviceId, planId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Failed("خطا در بررسی وجود تعرفه");
            }
        }

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// دریافت تعرفه بیمه بر اساس طرح و خدمت
        /// </summary>
        public async Task<ServiceResult<InsuranceTariff>> GetTariffByPlanAndServiceAsync(int planId, int serviceId)
        {
            try
            {
                _logger.Information("درخواست تعرفه بیمه. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    planId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                var tariff = await _tariffRepository.GetByPlanAndServiceAsync(planId, serviceId);
                if (tariff == null)
                {
                    _logger.Information("تعرفه بیمه یافت نشد. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        planId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<InsuranceTariff>.Failed("تعرفه بیمه یافت نشد");
                }

                _logger.Information("تعرفه بیمه با موفقیت دریافت شد. PlanId: {PlanId}, ServiceId: {ServiceId}, TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    planId, serviceId, tariff.InsuranceTariffId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceTariff>.Successful(tariff);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه بیمه. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    planId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<InsuranceTariff>.Failed("خطا در دریافت تعرفه بیمه");
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس طرح بیمه
        /// </summary>
        public async Task<ServiceResult<List<InsuranceTariff>>> GetTariffsByPlanIdAsync(int planId)
        {
            try
            {
                _logger.Information("درخواست تعرفه‌های بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                var tariffs = await _tariffRepository.GetByPlanIdAsync(planId);

                _logger.Information("تعرفه‌های بیمه با موفقیت دریافت شد. PlanId: {PlanId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    planId, tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceTariff>>.Successful(tariffs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<InsuranceTariff>>.Failed("خطا در دریافت تعرفه‌های بیمه");
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس خدمت
        /// </summary>
        public async Task<ServiceResult<List<InsuranceTariff>>> GetTariffsByServiceIdAsync(int serviceId)
        {
            try
            {
                _logger.Information("درخواست تعرفه‌های بیمه. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);

                var tariffs = await _tariffRepository.GetByServiceIdAsync(serviceId);

                _logger.Information("تعرفه‌های بیمه با موفقیت دریافت شد. ServiceId: {ServiceId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    serviceId, tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceTariff>>.Successful(tariffs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<InsuranceTariff>>.Failed("خطا در دریافت تعرفه‌های بیمه");
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// اعتبارسنجی تعرفه بیمه
        /// </summary>
        public async Task<ServiceResult<Dictionary<string, string>>> ValidateTariffAsync(InsuranceTariffCreateEditViewModel model)
        {
            var errors = new Dictionary<string, string>();

            try
            {
                _logger.Information("🏥 MEDICAL: شروع اعتبارسنجی تعرفه - PlanId: {PlanId}, ServiceId: {ServiceId}, TariffPrice: {TariffPrice}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}. User: {UserName} (Id: {UserId})",
                    model.InsurancePlanId, model.ServiceId, model.TariffPrice, model.PatientShare, model.InsurerShare, _currentUserService.UserName, _currentUserService.UserId);

                // بررسی وجود طرح بیمه
                var planExists = await _planRepository.DoesExistAsync(model.InsurancePlanId);
                if (!planExists)
                {
                    errors.Add("InsurancePlanId", "طرح بیمه یافت نشد");
                }

                // بررسی وجود خدمت (فقط اگر ServiceId مشخص باشد)
                if (model.ServiceId.HasValue)
                {
                    var service = await _serviceRepository.GetServiceByIdAsync(model.ServiceId.Value);
                    if (service == null)
                    {
                        errors.Add("ServiceId", "خدمت یافت نشد");
                    }
                }

                // اعتبارسنجی قیمت تعرفه
                if (model.TariffPrice.HasValue && model.TariffPrice < 0)
                {
                    errors.Add("TariffPrice", "قیمت تعرفه نمی‌تواند منفی باشد");
                }

                // اعتبارسنجی سهم بیمار (مبلغ به تومان)
                if (model.PatientShare.HasValue && model.PatientShare < 0)
                {
                    errors.Add("PatientShare", "سهم بیمار نمی‌تواند منفی باشد");
                }

                // اعتبارسنجی سهم بیمه (مبلغ به تومان)
                if (model.InsurerShare.HasValue && model.InsurerShare < 0)
                {
                    errors.Add("InsurerShare", "سهم بیمه نمی‌تواند منفی باشد");
                }

                // اعتبارسنجی منطقی بودن مبالغ
                if (model.TariffPrice.HasValue && model.TariffPrice > 0)
                {
                    // بررسی سهم بیمه
                    if (model.InsurerShare.HasValue && model.InsurerShare > model.TariffPrice)
                    {
                        errors.Add("InsurerShare", "سهم بیمه نمی‌تواند بیشتر از قیمت تعرفه باشد");
                    }

                    // بررسی سهم بیمار
                    if (model.PatientShare.HasValue && model.PatientShare > model.TariffPrice)
                    {
                        errors.Add("PatientShare", "سهم بیمار نمی‌تواند بیشتر از قیمت تعرفه باشد");
                    }

                    // بررسی مجموع سهم‌ها
                    if (model.PatientShare.HasValue && model.InsurerShare.HasValue)
                    {
                        var totalShare = model.PatientShare.Value + model.InsurerShare.Value;
                        if (totalShare > model.TariffPrice)
                        {
                            errors.Add("TotalShare", "مجموع سهم بیمار و بیمه نمی‌تواند بیشتر از قیمت تعرفه باشد");
                        }
                    }
                }

                _logger.Information("🏥 MEDICAL: اعتبارسنجی تکمیل شد - تعداد خطاها: {ErrorCount}. User: {UserName} (Id: {UserId})",
                    errors.Count, _currentUserService.UserName, _currentUserService.UserId);

                return errors.Count > 0 
                    ? ServiceResult<Dictionary<string, string>>.Failed("اعتبارسنجی ناموفق")
                    : ServiceResult<Dictionary<string, string>>.Successful(new Dictionary<string, string>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی تعرفه بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<Dictionary<string, string>>.Failed("خطا در اعتبارسنجی تعرفه بیمه");
            }
        }

        /// <summary>
        /// بررسی وجود تعرفه بیمه
        /// </summary>
        public async Task<ServiceResult<bool>> DoesTariffExistAsync(int planId, int serviceId, int? excludeId = null)
        {
            try
            {
                var exists = await _tariffRepository.DoesTariffExistAsync(planId, serviceId, excludeId);
                return ServiceResult<bool>.Successful(exists);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود تعرفه بیمه. PlanId: {PlanId}, ServiceId: {ServiceId}", planId, serviceId);
                return ServiceResult<bool>.Failed("خطا در بررسی وجود تعرفه بیمه");
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// تغییر وضعیت گروهی تعرفه‌ها
        /// </summary>
        public async Task<ServiceResult> BulkToggleStatusAsync(List<int> tariffIds, bool isActive)
        {
            try
            {
                _logger.Information("درخواست تغییر وضعیت گروهی تعرفه‌ها. Count: {Count}, IsActive: {IsActive}. User: {UserName} (Id: {UserId})",
                    tariffIds?.Count ?? 0, isActive, _currentUserService.UserName, _currentUserService.UserId);

                if (tariffIds == null || !tariffIds.Any())
                {
                    _logger.Warning("لیست تعرفه‌ها خالی است. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("هیچ تعرفه‌ای انتخاب نشده است");
                }

                var updatedCount = 0;
                foreach (var tariffId in tariffIds)
                {
                    try
                    {
                        var tariff = await _tariffRepository.GetByIdAsync(tariffId);
                        if (tariff != null && !tariff.IsDeleted)
                        {
                            tariff.IsActive = isActive;
                            tariff.UpdatedAt = DateTime.UtcNow;
                            tariff.UpdatedByUserId = _currentUserService.UserId;
                            updatedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "خطا در به‌روزرسانی تعرفه. Id: {Id}. User: {UserName} (Id: {UserId})",
                            tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }

                if (updatedCount > 0)
                {
                    await _tariffRepository.SaveChangesAsync();
                    _logger.Information("وضعیت {Count} تعرفه با موفقیت تغییر یافت. User: {UserName} (Id: {UserId})",
                        updatedCount, _currentUserService.UserName, _currentUserService.UserId);
                }

                return ServiceResult.Successful($"وضعیت {updatedCount} تعرفه با موفقیت تغییر یافت");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تغییر وضعیت گروهی تعرفه‌ها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در تغییر وضعیت تعرفه‌ها");
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// ایجاد تعرفه برای همه خدمات (Bulk Operation)
        /// </summary>
        public async Task<ServiceResult<int>> CreateBulkTariffForAllServicesAsync(InsuranceTariffCreateEditViewModel model)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: شروع Bulk Operation برای همه خدمات - PlanId: {PlanId}, User: {UserName} (Id: {UserId})",
                    model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت همه خدمات فعال
                var allServices = await _serviceRepository.GetAllActiveServicesAsync();
                if (!allServices.Any())
                {
                    _logger.Warning("🏥 MEDICAL: هیچ خدمت فعالی یافت نشد - User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<int>.Failed("هیچ خدمت فعالی یافت نشد");
                }

                var createdCount = 0;
                var errors = new List<string>();

                // 🚀 P1 FIX: استفاده از تراکنش برای Bulk Operation
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // ایجاد تعرفه برای هر خدمت
                        foreach (var service in allServices)
                {
                    try
                    {
                        // بررسی وجود تعرفه مشابه
                        var exists = await _tariffRepository.DoesTariffExistAsync(model.InsurancePlanId, service.ServiceId, 0);
                        if (exists)
                        {
                            _logger.Information("🏥 MEDICAL: تعرفه برای خدمت {ServiceId} ({ServiceName}) قبلاً وجود دارد - User: {UserName} (Id: {UserId})",
                                service.ServiceId, service.Title, _currentUserService.UserName, _currentUserService.UserId);
                            continue;
                        }

                        // محاسبه مقادیر تعرفه
                        var calculatedValues = await CalculateTariffValuesForServiceAsync(model, service);

                        // ایجاد تعرفه
                        var tariff = new InsuranceTariff
                        {
                            ServiceId = service.ServiceId,
                            InsurancePlanId = model.InsurancePlanId,
                            TariffPrice = calculatedValues.TariffPrice,
                            PatientShare = calculatedValues.PatientShare,
                            InsurerShare = calculatedValues.InsurerShare,
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = _currentUserService.UserId,
                            IsDeleted = false
                        };

                        await _tariffRepository.CreateAsync(tariff);
                        createdCount++;

                        _logger.Information("🏥 MEDICAL: تعرفه برای خدمت {ServiceId} ({ServiceName}) ایجاد شد - User: {UserName} (Id: {UserId})",
                            service.ServiceId, service.Title, _currentUserService.UserName, _currentUserService.UserId);
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"خطا در ایجاد تعرفه برای خدمت {service.ServiceId} ({service.Title}): {ex.Message}";
                        errors.Add(errorMsg);
                        _logger.Error(ex, "🏥 MEDICAL: {ErrorMsg} - User: {UserName} (Id: {UserId})",
                            errorMsg, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }

                        _logger.Information("🏥 MEDICAL: Bulk Operation تکمیل شد - Created: {CreatedCount}, Errors: {ErrorCount}, User: {UserName} (Id: {UserId})",
                            createdCount, errors.Count, _currentUserService.UserName, _currentUserService.UserId);

                        if (errors.Any())
                        {
                            // 🚀 P1 FIX: در صورت خطا، تراکنش را rollback کن
                            transaction.Rollback();
                            return ServiceResult<int>.Failed($"تعداد {createdCount} تعرفه ایجاد شد، اما {errors.Count} خطا رخ داد: {string.Join("; ", errors)}");
                        }

                        // 🚀 P1 FIX: در صورت موفقیت، تراکنش را commit کن
                        transaction.Commit();
                        return ServiceResult<int>.Successful(createdCount);
                    }
                    catch (Exception ex)
                    {
                        // 🚀 P1 FIX: در صورت خطای کلی، تراکنش را rollback کن
                        transaction.Rollback();
                        _logger.Error(ex, "🏥 MEDICAL: خطا در Bulk Operation - تراکنش rollback شد - User: {UserName} (Id: {UserId})",
                            _currentUserService.UserName, _currentUserService.UserId);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در Bulk Operation - PlanId: {PlanId}, User: {UserName} (Id: {UserId})",
                    model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<int>.Failed("خطا در ایجاد تعرفه برای همه خدمات");
            }
        }

        /// <summary>
        /// محاسبه مقادیر تعرفه برای یک خدمت خاص
        /// </summary>
        private async Task<(decimal? TariffPrice, decimal? PatientShare, decimal? InsurerShare)> CalculateTariffValuesForServiceAsync(
            InsuranceTariffCreateEditViewModel model, Models.Entities.Clinic.Service service)
        {
            try
            {
                // دریافت طرح بیمه
                var plan = await _planRepository.GetByIdAsync(model.InsurancePlanId);
                if (plan == null)
                {
                    _logger.Warning("🏥 MEDICAL: طرح بیمه یافت نشد - PlanId: {PlanId}, User: {UserName} (Id: {UserId})",
                        model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                    return (null, null, null);
                }

                // 🔧 CRITICAL FIX: تبدیل واحد از تومان (UI) به ریال (Database)
                decimal? tariffPrice = model.TariffPrice.HasValue ? model.TariffPrice * 10 : null; // تومان → ریال
                decimal? patientShare = model.PatientShare.HasValue ? model.PatientShare * 10 : null; // تومان → ریال
                decimal? insurerShare = model.InsurerShare.HasValue ? model.InsurerShare * 10 : null; // تومان → ریال

                // محاسبه قیمت تعرفه با استفاده از موتور اصلی محاسبات
                if (!tariffPrice.HasValue)
                {
                    _logger.Information("🏥 MEDICAL: شروع محاسبه قیمت خدمت در Bulk Operation - ServiceId: {ServiceId}, ServiceTitle: {ServiceTitle}, BasePrice: {BasePrice}, IsHashtagged: {IsHashtagged}. User: {UserName} (Id: {UserId})",
                        service.ServiceId, service.Title, service.Price, service.IsHashtagged, _currentUserService.UserName, _currentUserService.UserId);

                    // بررسی ServiceComponents
                    var serviceWithComponents = await _serviceRepository.GetByIdWithComponentsAsync(service.ServiceId);
                    if (serviceWithComponents?.ServiceComponents != null && serviceWithComponents.ServiceComponents.Any())
                    {
                        _logger.Information("🏥 MEDICAL: ServiceComponents موجود است در Bulk - Count: {Count}. User: {UserName} (Id: {UserId})",
                            serviceWithComponents.ServiceComponents.Count, _currentUserService.UserName, _currentUserService.UserId);

                        foreach (var component in serviceWithComponents.ServiceComponents)
                        {
                            _logger.Information("🏥 MEDICAL: ServiceComponent در Bulk - Type: {Type}, Coefficient: {Coefficient}, IsActive: {IsActive}, IsDeleted: {IsDeleted}. User: {UserName} (Id: {UserId})",
                                component.ComponentType, component.Coefficient, component.IsActive, component.IsDeleted, _currentUserService.UserName, _currentUserService.UserId);
                        }
                    }
                    else
                    {
                        _logger.Warning("🏥 MEDICAL: ServiceComponents موجود نیست یا خالی است در Bulk - ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            service.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                    }

                    tariffPrice = _serviceCalculationService.CalculateServicePrice(service);
                    _logger.Information("🏥 MEDICAL: قیمت تعرفه محاسبه شد در Bulk - ServiceId: {ServiceId}, CalculatedPrice: {Price}, BasePrice: {BasePrice}. User: {UserName} (Id: {UserId})",
                        service.ServiceId, tariffPrice, service.Price, _currentUserService.UserName, _currentUserService.UserId);

                    // اگر قیمت محاسبه شده 0 است، بررسی بیشتر
                    if (tariffPrice == 0)
                    {
                        _logger.Warning("🏥 MEDICAL: قیمت محاسبه شده 0 است در Bulk - ServiceId: {ServiceId}, ServiceTitle: {ServiceTitle}, BasePrice: {BasePrice}. User: {UserName} (Id: {UserId})",
                            service.ServiceId, service.Title, service.Price, _currentUserService.UserName, _currentUserService.UserId);

                        // بررسی ServiceComponents به صورت مستقیم
                        var directComponents = await _serviceRepository.GetServiceComponentsAsync(service.ServiceId);

                        _logger.Information("🏥 MEDICAL: بررسی مستقیم ServiceComponents در Bulk - ServiceId: {ServiceId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                            service.ServiceId, directComponents.Count, _currentUserService.UserName, _currentUserService.UserId);

                        foreach (var comp in directComponents)
                        {
                            _logger.Information("🏥 MEDICAL: Direct ServiceComponent در Bulk - Type: {Type}, Coefficient: {Coefficient}, IsActive: {IsActive}. User: {UserName} (Id: {UserId})",
                                comp.ComponentType, comp.Coefficient, comp.IsActive, _currentUserService.UserName, _currentUserService.UserId);
                        }

                        // اگر ServiceComponents موجود نیست، از قیمت پایه استفاده کن
                        if (!directComponents.Any())
                        {
                            _logger.Warning("🏥 MEDICAL: هیچ ServiceComponent یافت نشد در Bulk - استفاده از قیمت پایه. ServiceId: {ServiceId}, BasePrice: {BasePrice}. User: {UserName} (Id: {UserId})",
                                service.ServiceId, service.Price, _currentUserService.UserName, _currentUserService.UserId);
                            
                            if (service.Price > 0)
                            {
                                tariffPrice = service.Price;
                                _logger.Information("🏥 MEDICAL: قیمت پایه استفاده شد در Bulk - ServiceId: {ServiceId}, Price: {Price}. User: {UserName} (Id: {UserId})",
                                    service.ServiceId, tariffPrice, _currentUserService.UserName, _currentUserService.UserId);
                            }
                            else
                            {
                                _logger.Error("🏥 MEDICAL: قیمت پایه هم 0 است در Bulk - ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                                    service.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                            }
                        }
                    }
                }

                // محاسبه سهم بیمه و بیمار بر اساس مبلغ (منطق صحیح)
                if (tariffPrice.HasValue)
                {
                    // محاسبه مبلغ سهم بیمه
                    decimal insurerAmount = (tariffPrice.Value * plan.CoveragePercent) / 100;
                    decimal patientAmount = tariffPrice.Value - insurerAmount;

                    // تنظیم مقادیر نهایی بر اساس مبلغ
                    if (!insurerShare.HasValue)
                    {
                        insurerShare = insurerAmount;
                        _logger.Information("🏥 MEDICAL: سهم بیمه محاسبه شد - PlanId: {PlanId}, CoveragePercent: {CoveragePercent}%, InsurerAmount: {InsurerAmount}, User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, plan.CoveragePercent, insurerAmount, _currentUserService.UserName, _currentUserService.UserId);
                    }

                    if (!patientShare.HasValue)
                    {
                        patientShare = patientAmount;
                        _logger.Information("🏥 MEDICAL: سهم بیمار محاسبه شد - PlanId: {PlanId}, PatientAmount: {PatientAmount}, User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, patientAmount, _currentUserService.UserName, _currentUserService.UserId);
                    }

                    _logger.Information("🏥 MEDICAL: محاسبات کامل تعرفه - ServicePrice: {ServicePrice}, InsurerShare: {InsurerShare}, PatientShare: {PatientShare}, CoveragePercent: {CoveragePercent}%, User: {UserName} (Id: {UserId})",
                        tariffPrice.Value, insurerShare.Value, patientShare.Value, plan.CoveragePercent, _currentUserService.UserName, _currentUserService.UserId);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: قیمت تعرفه محاسبه نشده - محاسبه سهم‌ها امکان‌پذیر نیست. PlanId: {PlanId}, User: {UserName} (Id: {UserId})",
                        model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                }

                return (tariffPrice, patientShare, insurerShare);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه مقادیر تعرفه - ServiceId: {ServiceId}, PlanId: {PlanId}, User: {UserName} (Id: {UserId})",
                    service.ServiceId, model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                return (null, null, null);
            }
        }

        #endregion

        #region Calculation Operations

        /// <summary>
        /// محاسبه مقادیر تعرفه به صورت داینامیک
        /// </summary>
        private async Task<(decimal? TariffPrice, decimal? PatientShare, decimal? InsurerShare)> CalculateTariffValuesAsync(InsuranceTariffCreateEditViewModel model)
        {
            try
            {
                _logger.Information("شروع محاسبات داینامیک تعرفه. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت طرح بیمه
                var plan = await _planRepository.GetByIdAsync(model.InsurancePlanId);
                if (plan == null)
                {
                    _logger.Warning("طرح بیمه یافت نشد. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                    return (null, null, null);
                }

                // 🔧 CRITICAL FIX: تبدیل واحد از تومان (UI) به ریال (Database)
                decimal? tariffPrice = model.TariffPrice.HasValue ? model.TariffPrice * 10 : null; // تومان → ریال
                decimal? patientShare = model.PatientShare.HasValue ? model.PatientShare * 10 : null; // تومان → ریال
                decimal? insurerShare = model.InsurerShare.HasValue ? model.InsurerShare * 10 : null; // تومان → ریال

                // محاسبه قیمت تعرفه با استفاده از موتور اصلی محاسبات
                if (!tariffPrice.HasValue)
                {
                    if (model.ServiceId.HasValue)
                    {
                        // محاسبه برای خدمت خاص
                        var service = await _serviceRepository.GetServiceByIdAsync(model.ServiceId.Value);
                        if (service != null)
                        {
                            _logger.Information("🏥 MEDICAL: شروع محاسبه قیمت خدمت - ServiceId: {ServiceId}, ServiceTitle: {ServiceTitle}, BasePrice: {BasePrice}, IsHashtagged: {IsHashtagged}. User: {UserName} (Id: {UserId})",
                                model.ServiceId, service.Title, service.Price, service.IsHashtagged, _currentUserService.UserName, _currentUserService.UserId);

                            // بررسی ServiceComponents
                            var serviceWithComponents = await _serviceRepository.GetByIdWithComponentsAsync(model.ServiceId.Value);
                            if (serviceWithComponents?.ServiceComponents != null && serviceWithComponents.ServiceComponents.Any())
                            {
                                _logger.Information("🏥 MEDICAL: ServiceComponents موجود است - Count: {Count}. User: {UserName} (Id: {UserId})",
                                    serviceWithComponents.ServiceComponents.Count, _currentUserService.UserName, _currentUserService.UserId);

                                foreach (var component in serviceWithComponents.ServiceComponents)
                                {
                                    _logger.Information("🏥 MEDICAL: ServiceComponent - Type: {Type}, Coefficient: {Coefficient}, IsActive: {IsActive}, IsDeleted: {IsDeleted}. User: {UserName} (Id: {UserId})",
                                        component.ComponentType, component.Coefficient, component.IsActive, component.IsDeleted, _currentUserService.UserName, _currentUserService.UserId);
                                }
                            }
                            else
                            {
                                _logger.Warning("🏥 MEDICAL: ServiceComponents موجود نیست یا خالی است - ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                                    model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                            }

                            // استفاده از ServiceCalculationService برای محاسبه دقیق
                            tariffPrice = _serviceCalculationService.CalculateServicePrice(service);
                            _logger.Information("🏥 MEDICAL: قیمت تعرفه محاسبه شد - ServiceId: {ServiceId}, CalculatedPrice: {Price}, BasePrice: {BasePrice}. User: {UserName} (Id: {UserId})",
                                model.ServiceId, tariffPrice, service.Price, _currentUserService.UserName, _currentUserService.UserId);

                            // اگر قیمت محاسبه شده 0 است، بررسی بیشتر
                            if (tariffPrice == 0)
                            {
                                _logger.Warning("🏥 MEDICAL: قیمت محاسبه شده 0 است - ServiceId: {ServiceId}, ServiceTitle: {ServiceTitle}, BasePrice: {BasePrice}. User: {UserName} (Id: {UserId})",
                                    model.ServiceId, service.Title, service.Price, _currentUserService.UserName, _currentUserService.UserId);

                                // بررسی ServiceComponents به صورت مستقیم
                                var directComponents = await _serviceRepository.GetServiceComponentsAsync(model.ServiceId.Value);

                                _logger.Information("🏥 MEDICAL: بررسی مستقیم ServiceComponents - ServiceId: {ServiceId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                                    model.ServiceId, directComponents.Count, _currentUserService.UserName, _currentUserService.UserId);

                                foreach (var comp in directComponents)
                                {
                                    _logger.Information("🏥 MEDICAL: Direct ServiceComponent - Type: {Type}, Coefficient: {Coefficient}, IsActive: {IsActive}. User: {UserName} (Id: {UserId})",
                                        comp.ComponentType, comp.Coefficient, comp.IsActive, _currentUserService.UserName, _currentUserService.UserId);
                                }

                                // اگر ServiceComponents موجود نیست، از قیمت پایه استفاده کن
                                if (!directComponents.Any())
                                {
                                    _logger.Warning("🏥 MEDICAL: هیچ ServiceComponent یافت نشد - استفاده از قیمت پایه. ServiceId: {ServiceId}, BasePrice: {BasePrice}. User: {UserName} (Id: {UserId})",
                                        model.ServiceId, service.Price, _currentUserService.UserName, _currentUserService.UserId);
                                    
                                    if (service.Price > 0)
                                    {
                                        tariffPrice = service.Price;
                                        _logger.Information("🏥 MEDICAL: قیمت پایه استفاده شد - ServiceId: {ServiceId}, Price: {Price}. User: {UserName} (Id: {UserId})",
                                            model.ServiceId, tariffPrice, _currentUserService.UserName, _currentUserService.UserId);
                                    }
                                    else
                                    {
                                        _logger.Error("🏥 MEDICAL: قیمت پایه هم 0 است - ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                                            model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.Warning("🏥 MEDICAL: خدمت یافت نشد - ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                                model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                        }
                    }
                    else if (model.IsAllServices)
                    {
                        // برای "همه خدمات" - استفاده از قیمت پیش‌فرض یا 0
                        tariffPrice = 0; // یا می‌توانید قیمت پیش‌فرض تعریف کنید
                        _logger.Information("🏥 MEDICAL: قیمت تعرفه برای 'همه خدمات' تنظیم شد - TariffPrice: {Price}. User: {UserName} (Id: {UserId})",
                            tariffPrice, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }

                // محاسبه سهم بیمه و بیمار بر اساس مبلغ (منطق صحیح)
                if (tariffPrice.HasValue)
                {
                    // محاسبه مبلغ سهم بیمه
                    decimal insurerAmount = (tariffPrice.Value * plan.CoveragePercent) / 100;
                    decimal patientAmount = tariffPrice.Value - insurerAmount;

                    // تنظیم مقادیر نهایی بر اساس مبلغ
                    if (!insurerShare.HasValue)
                    {
                        insurerShare = insurerAmount;
                        _logger.Information("🏥 MEDICAL: سهم بیمه محاسبه شد در Bulk - PlanId: {PlanId}, CoveragePercent: {CoveragePercent}%, InsurerAmount: {InsurerAmount}, User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, plan.CoveragePercent, insurerAmount, _currentUserService.UserName, _currentUserService.UserId);
                    }

                    if (!patientShare.HasValue)
                    {
                        patientShare = patientAmount;
                        _logger.Information("🏥 MEDICAL: سهم بیمار محاسبه شد در Bulk - PlanId: {PlanId}, PatientAmount: {PatientAmount}, User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, patientAmount, _currentUserService.UserName, _currentUserService.UserId);
                    }

                    _logger.Information("🏥 MEDICAL: محاسبات کامل تعرفه در Bulk - ServicePrice: {ServicePrice}, InsurerShare: {InsurerShare}, PatientShare: {PatientShare}, CoveragePercent: {CoveragePercent}%, User: {UserName} (Id: {UserId})",
                        tariffPrice.Value, insurerShare.Value, patientShare.Value, plan.CoveragePercent, _currentUserService.UserName, _currentUserService.UserId);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: قیمت تعرفه محاسبه نشده در Bulk - محاسبه سهم‌ها امکان‌پذیر نیست. PlanId: {PlanId}, User: {UserName} (Id: {UserId})",
                        model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                }

                _logger.Information("محاسبات داینامیک تکمیل شد. TariffPrice: {TariffPrice}, PatientShare: {PatientShare}%, InsurerShare: {InsurerShare}%. User: {UserName} (Id: {UserId})",
                    tariffPrice, patientShare, insurerShare, _currentUserService.UserName, _currentUserService.UserId);

                return (tariffPrice, patientShare, insurerShare);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبات داینامیک تعرفه. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                return (null, null, null);
            }
        }

        #endregion

        #region Statistics Operations

        /// <summary>
        /// دریافت آمار تعرفه‌های بیمه
        /// </summary>
        public async Task<ServiceResult<InsuranceTariffStatisticsViewModel>> GetStatisticsAsync()
        {
            try
            {
                _logger.Information("درخواست آمار تعرفه‌های بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // بهینه‌سازی: محاسبه همزمان آمار و تعداد کل
                var statisticsTask = _tariffRepository.GetStatisticsAsync();
                var totalCountTask = _tariffRepository.GetTotalCountAsync();

                await Task.WhenAll(statisticsTask, totalCountTask);

                var statistics = await statisticsTask;
                var totalCount = await totalCountTask;

                // 🚀 P0 FIX: محاسبه آمار کامل برای محیط درمانی
                var activeTariffs = statistics.ContainsKey("ActiveTariffs") ? statistics["ActiveTariffs"] : 0;
                var inactiveTariffs = statistics.ContainsKey("InactiveTariffs") ? statistics["InactiveTariffs"] : 0;
                var totalServices = statistics.ContainsKey("TotalServices") ? statistics["TotalServices"] : 0;

                var viewModel = new InsuranceTariffStatisticsViewModel
                {
                    TotalTariffs = totalCount,
                    ActiveTariffs = activeTariffs,
                    InactiveTariffs = inactiveTariffs,
                    TotalServices = totalServices,
                    TariffsWithCustomPrice = statistics.ContainsKey("TariffsWithCustomPrice") ? statistics["TariffsWithCustomPrice"] : 0,
                    TariffsWithCustomPatientShare = statistics.ContainsKey("TariffsWithCustomPatientShare") ? statistics["TariffsWithCustomPatientShare"] : 0,
                    TariffsWithCustomInsurerShare = statistics.ContainsKey("TariffsWithCustomInsurerShare") ? statistics["TariffsWithCustomInsurerShare"] : 0
                };

                _logger.Information("آمار تعرفه‌های بیمه با موفقیت دریافت شد. Total: {Total}, CustomPrice: {CustomPrice}, CustomPatientShare: {CustomPatientShare}, CustomInsurerShare: {CustomInsurerShare}. User: {UserName} (Id: {UserId})",
                    viewModel.TotalTariffs, viewModel.TariffsWithCustomPrice, viewModel.TariffsWithCustomPatientShare, viewModel.TariffsWithCustomInsurerShare, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceTariffStatisticsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار تعرفه‌های بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<InsuranceTariffStatisticsViewModel>.Failed("خطا در دریافت آمار تعرفه‌های بیمه");
            }
        }

        #endregion

        #region Supplementary Insurance Methods

        /// <summary>
        /// دریافت همه تعرفه‌های بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult<List<InsuranceTariff>>> GetAllSupplementaryTariffsAsync()
        {
            try
            {
                _logger.Information("درخواست همه تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var tariffs = await _tariffRepository.GetAllActiveAsync();
                var supplementaryTariffs = tariffs
                    .Where(t => t.InsuranceType == InsuranceType.Supplementary && !t.IsDeleted)
                    .OrderBy(t => t.Service?.Title)
                    .ToList();

                _logger.Information("همه تعرفه‌های بیمه تکمیلی با موفقیت دریافت شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                    supplementaryTariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceTariff>>.Successful(supplementaryTariffs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت همه تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<InsuranceTariff>>.Failed("خطا در دریافت تعرفه‌های بیمه تکمیلی");
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه تکمیلی با فیلترهای بهینه‌سازی شده
        /// </summary>
        public async Task<ServiceResult<List<InsuranceTariff>>> GetFilteredSupplementaryTariffsAsync(
            string searchTerm = "", 
            int? departmentId = null, 
            bool? isActive = null)
        {
            try
            {
                _logger.Information("درخواست تعرفه‌های بیمه تکمیلی با فیلترها - SearchTerm: {SearchTerm}, DeptId: {DeptId}, IsActive: {IsActive}. User: {UserName} (Id: {UserId})",
                    searchTerm, departmentId, isActive, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _tariffRepository.GetFilteredSupplementaryTariffsAsync(
                    searchTerm: searchTerm,
                    departmentId: departmentId,
                    isActive: isActive);

                _logger.Information("تعرفه‌های بیمه تکمیلی با فیلترها با موفقیت دریافت شد - Count: {Count}. User: {UserName} (Id: {UserId})",
                    result.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceTariff>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه تکمیلی با فیلترها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<InsuranceTariff>>.Failed("خطا در دریافت تعرفه‌های بیمه تکمیلی");
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه تکمیلی با اعتبارسنجی تاریخ
        /// </summary>
        public async Task<ServiceResult<List<InsuranceTariff>>> GetSupplementaryTariffsAsync(int planId, DateTime? calculationDate = null)
        {
            try
            {
                var effectiveDate = calculationDate ?? DateTime.Now;
                
                _logger.Information("درخواست تعرفه‌های بیمه تکمیلی. PlanId: {PlanId}, CalculationDate: {CalculationDate}. User: {UserName} (Id: {UserId})",
                    planId, effectiveDate, _currentUserService.UserName, _currentUserService.UserId);

                var tariffs = await _tariffRepository.GetByPlanIdAsync(planId);
                
                // اعتبارسنجی تعرفه‌ها بر اساس تاریخ
                var validSupplementaryTariffs = tariffs
                    .Where(t => t.InsuranceType == InsuranceType.Supplementary)
                    .Where(t => IsTariffValidForDate(t, effectiveDate))
                    .OrderBy(t => t.Priority ?? 0)
                    .ToList();

                _logger.Information("تعرفه‌های بیمه تکمیلی معتبر دریافت شد. PlanId: {PlanId}, ValidCount: {ValidCount}, TotalCount: {TotalCount}. User: {UserName} (Id: {UserId})",
                    planId, validSupplementaryTariffs.Count, tariffs.Count(t => t.InsuranceType == InsuranceType.Supplementary), _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceTariff>>.Successful(validSupplementaryTariffs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعرفه‌های بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<InsuranceTariff>>.Failed("خطا در دریافت تعرفه‌های بیمه تکمیلی");
            }
        }

        /// <summary>
        /// محاسبه تعرفه بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult<decimal>> CalculateSupplementaryTariffAsync(int serviceId, int planId, decimal baseAmount, DateTime? calculationDate = null)
        {
            try
            {
                var effectiveDate = calculationDate ?? DateTime.Now;
                
                _logger.Information("محاسبه تعرفه بیمه تکمیلی. ServiceId: {ServiceId}, PlanId: {PlanId}, BaseAmount: {BaseAmount}, CalculationDate: {CalculationDate}. User: {UserName} (Id: {UserId})",
                    serviceId, planId, baseAmount, effectiveDate, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه‌های بیمه تکمیلی معتبر
                var supplementaryTariffs = await GetSupplementaryTariffsAsync(planId, effectiveDate);
                if (!supplementaryTariffs.Success)
                {
                    return ServiceResult<decimal>.Failed("خطا در دریافت تعرفه‌های بیمه تکمیلی");
                }

                var tariff = supplementaryTariffs.Data.FirstOrDefault(t => t.ServiceId == serviceId);
                if (tariff == null)
                {
                    _logger.Warning("تعرفه بیمه تکمیلی معتبر برای خدمت یافت نشد. ServiceId: {ServiceId}, PlanId: {PlanId}, CalculationDate: {CalculationDate}. User: {UserName} (Id: {UserId})",
                        serviceId, planId, effectiveDate, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<decimal>.Failed("تعرفه بیمه تکمیلی معتبر برای این خدمت تعریف نشده است");
                }

                // اعتبارسنجی اضافی تعرفه
                if (!IsTariffValidForDate(tariff, effectiveDate))
                {
                    _logger.Warning("تعرفه بیمه تکمیلی برای تاریخ محاسبه معتبر نیست. TariffId: {TariffId}, ServiceId: {ServiceId}, CalculationDate: {CalculationDate}. User: {UserName} (Id: {UserId})",
                        tariff.InsuranceTariffId, serviceId, effectiveDate, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<decimal>.Failed("تعرفه بیمه تکمیلی برای تاریخ محاسبه معتبر نیست");
                }

                // محاسبه تعرفه بر اساس تنظیمات
                decimal calculatedAmount = baseAmount;
                
                if (tariff.SupplementaryCoveragePercent.HasValue)
                {
                    calculatedAmount = baseAmount * (tariff.SupplementaryCoveragePercent.Value / 100);
                    _logger.Debug("محاسبه بر اساس درصد پوشش. BaseAmount: {BaseAmount}, CoveragePercent: {CoveragePercent}, CalculatedAmount: {CalculatedAmount}",
                        baseAmount, tariff.SupplementaryCoveragePercent.Value, calculatedAmount);
                }

                if (tariff.SupplementaryMaxPayment.HasValue && calculatedAmount > tariff.SupplementaryMaxPayment.Value)
                {
                    _logger.Debug("اعمال سقف پرداخت. CalculatedAmount: {CalculatedAmount}, MaxPayment: {MaxPayment}",
                        calculatedAmount, tariff.SupplementaryMaxPayment.Value);
                    calculatedAmount = tariff.SupplementaryMaxPayment.Value;
                }

                _logger.Information("محاسبه تعرفه بیمه تکمیلی تکمیل شد. ServiceId: {ServiceId}, PlanId: {PlanId}, BaseAmount: {BaseAmount}, CalculatedAmount: {CalculatedAmount}, TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    serviceId, planId, baseAmount, calculatedAmount, tariff.InsuranceTariffId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<decimal>.Successful(calculatedAmount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه تعرفه بیمه تکمیلی. ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    serviceId, planId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<decimal>.Failed("خطا در محاسبه تعرفه بیمه تکمیلی");
            }
        }

        /// <summary>
        /// دریافت تنظیمات بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult<Dictionary<string, object>>> GetSupplementarySettingsAsync(int planId)
        {
            try
            {
                _logger.Information("درخواست تنظیمات بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                var supplementaryTariffs = await GetSupplementaryTariffsAsync(planId);
                if (!supplementaryTariffs.Success)
                {
                    return ServiceResult<Dictionary<string, object>>.Failed("خطا در دریافت تعرفه‌های بیمه تکمیلی");
                }

                var settings = new Dictionary<string, object>();
                
                foreach (var tariff in supplementaryTariffs.Data)
                {
                    if (!string.IsNullOrEmpty(tariff.SupplementarySettings))
                    {
                        try
                        {
                            // Parse JSON settings if available
                            var tariffSettings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(tariff.SupplementarySettings);
                            foreach (var setting in tariffSettings)
                            {
                                settings[$"{tariff.ServiceId}_{setting.Key}"] = setting.Value;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning("خطا در تجزیه تنظیمات JSON. ServiceId: {ServiceId}, Settings: {Settings}. User: {UserName} (Id: {UserId})",
                                tariff.ServiceId, tariff.SupplementarySettings, _currentUserService.UserName, _currentUserService.UserId);
                        }
                    }
                }

                _logger.Information("تنظیمات بیمه تکمیلی با موفقیت دریافت شد. PlanId: {PlanId}, SettingsCount: {Count}. User: {UserName} (Id: {UserId})",
                    planId, settings.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<Dictionary<string, object>>.Successful(settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تنظیمات بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<Dictionary<string, object>>.Failed("خطا در دریافت تنظیمات بیمه تکمیلی");
            }
        }

        /// <summary>
        /// به‌روزرسانی تنظیمات بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult> UpdateSupplementarySettingsAsync(int planId, Dictionary<string, object> settings)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی تنظیمات بیمه تکمیلی. PlanId: {PlanId}, SettingsCount: {Count}. User: {UserName} (Id: {UserId})",
                    planId, settings.Count, _currentUserService.UserName, _currentUserService.UserId);

                var supplementaryTariffs = await GetSupplementaryTariffsAsync(planId);
                if (!supplementaryTariffs.Success)
                {
                    return ServiceResult.Failed("خطا در دریافت تعرفه‌های بیمه تکمیلی");
                }

                foreach (var tariff in supplementaryTariffs.Data)
                {
                    var tariffSettings = new Dictionary<string, object>();
                    
                    // استخراج تنظیمات مربوط به این تعرفه
                    foreach (var setting in settings)
                    {
                        if (setting.Key.StartsWith($"{tariff.ServiceId}_"))
                        {
                            var key = setting.Key.Substring($"{tariff.ServiceId}_".Length);
                            tariffSettings[key] = setting.Value;
                        }
                    }

                    if (tariffSettings.Any())
                    {
                        tariff.SupplementarySettings = System.Text.Json.JsonSerializer.Serialize(tariffSettings);
                        tariff.UpdatedAt = DateTime.UtcNow;
                        tariff.UpdatedByUserId = _currentUserService.UserId;

                        await _tariffRepository.UpdateAsync(tariff);
                    }
                }

                _logger.Information("تنظیمات بیمه تکمیلی با موفقیت به‌روزرسانی شد. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("تنظیمات بیمه تکمیلی با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تنظیمات بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در به‌روزرسانی تنظیمات بیمه تکمیلی");
            }
        }

        #endregion

        #region Additional Methods for Controller Compatibility

        /// <summary>
        /// دریافت تعرفه بیمه بر اساس شناسه (نام متد جایگزین)
        /// </summary>
        public async Task<ServiceResult<InsuranceTariffDetailsViewModel>> GetTariffByIdAsync(int id)
        {
            return await GetTariffDetailsAsync(id);
        }

        /// <summary>
        /// بررسی وجود تعرفه بیمه (نام متد جایگزین)
        /// </summary>
        public async Task<ServiceResult<bool>> CheckTariffExistsAsync(int planId, int serviceId, int? excludeId = null)
        {
            return await DoesTariffExistAsync(planId, serviceId, excludeId);
        }

        /// <summary>
        /// حذف تعرفه بیمه
        /// </summary>
        public async Task<ServiceResult> DeleteTariffAsync(int id)
        {
            try
            {
                _logger.Information("درخواست حذف تعرفه بیمه. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var tariff = await _tariffRepository.GetByIdAsync(id);
                if (tariff == null)
                {
                    _logger.Warning("تعرفه بیمه برای حذف یافت نشد. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("تعرفه بیمه یافت نشد");
                }

                _tariffRepository.Delete(tariff);
                await _tariffRepository.SaveChangesAsync();

                _logger.Information("تعرفه بیمه با موفقیت حذف شد. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("تعرفه بیمه با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تعرفه بیمه. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در حذف تعرفه بیمه");
            }
        }

        /// <summary>
        /// بررسی اعتبار تعرفه برای تاریخ مشخص
        /// </summary>
        private bool IsTariffValidForDate(InsuranceTariff tariff, DateTime calculationDate)
        {
            try
            {
                // بررسی تاریخ شروع
                if (tariff.StartDate.HasValue && tariff.StartDate.Value > calculationDate)
                {
                    _logger.Debug("تعرفه هنوز فعال نشده. TariffId: {TariffId}, StartDate: {StartDate}, CalculationDate: {CalculationDate}",
                        tariff.InsuranceTariffId, tariff.StartDate, calculationDate);
                    return false;
                }

                // بررسی تاریخ پایان
                if (tariff.EndDate.HasValue && tariff.EndDate.Value < calculationDate)
                {
                    _logger.Debug("تعرفه منقضی شده. TariffId: {TariffId}, EndDate: {EndDate}, CalculationDate: {CalculationDate}",
                        tariff.InsuranceTariffId, tariff.EndDate, calculationDate);
                    return false;
                }

                // بررسی وضعیت فعال بودن
                if (!tariff.IsActive)
                {
                    _logger.Debug("تعرفه غیرفعال است. TariffId: {TariffId}",
                        tariff.InsuranceTariffId);
                    return false;
                }

                // بررسی حذف نرم
                if (tariff.IsDeleted)
                {
                    _logger.Debug("تعرفه حذف شده است. TariffId: {TariffId}",
                        tariff.InsuranceTariffId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی اعتبار تعرفه. TariffId: {TariffId}, CalculationDate: {CalculationDate}",
                    tariff.InsuranceTariffId, calculationDate);
                return false;
            }
        }

        #endregion
    }
}
