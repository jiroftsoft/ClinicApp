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
        private readonly ILogger _logger;

        public InsuranceTariffService(
            IInsuranceTariffRepository tariffRepository,
            IInsurancePlanRepository planRepository,
            IServiceRepository serviceRepository,
            IServiceCalculationService serviceCalculationService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _tariffRepository = tariffRepository ?? throw new ArgumentNullException(nameof(tariffRepository));
            _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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
        /// ایجاد تعرفه بیمه جدید
        /// </summary>
        public async Task<ServiceResult<int>> CreateTariffAsync(InsuranceTariffCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد تعرفه بیمه جدید. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی
                var validationResult = await ValidateTariffAsync(model);
                if (!validationResult.Success)
                {
                    _logger.Warning("اعتبارسنجی تعرفه بیمه ناموفق. PlanId: {PlanId}, ServiceId: {ServiceId}, Errors: {Errors}. User: {UserName} (Id: {UserId})",
                        model.InsurancePlanId, model.ServiceId, string.Join(", ", validationResult.Data.Values), _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<int>.Failed("اطلاعات وارد شده معتبر نیست");
                }

                // بررسی وجود تعرفه مشابه (فقط اگر ServiceId مشخص باشد)
                if (model.ServiceId.HasValue)
                {
                    var exists = await _tariffRepository.DoesTariffExistAsync(model.InsurancePlanId, model.ServiceId.Value);
                    if (exists)
                    {
                        _logger.Warning("تعرفه بیمه مشابه وجود دارد. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                        return ServiceResult<int>.Failed("تعرفه بیمه برای این طرح و خدمت قبلاً تعریف شده است");
                    }
                }

                // محاسبات داینامیک
                var calculatedValues = await CalculateTariffValuesAsync(model);
                
                // ایجاد entity
                var tariff = new InsuranceTariff
                {
                    ServiceId = model.ServiceId ?? 0, // 0 برای "همه خدمات"
                    InsurancePlanId = model.InsurancePlanId,
                    TariffPrice = calculatedValues.TariffPrice,
                    PatientShare = calculatedValues.PatientShare,
                    InsurerShare = calculatedValues.InsurerShare,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId,
                    IsDeleted = false
                };

                var result = await _tariffRepository.AddAsync(tariff);

                _logger.Information("تعرفه بیمه جدید با موفقیت ایجاد شد. Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    result.InsuranceTariffId, model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<int>.Successful(result.InsuranceTariffId, "تعرفه بیمه جدید با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تعرفه بیمه جدید. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
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
                    _logger.Warning("اعتبارسنجی تعرفه بیمه ناموفق. Id: {Id}, Errors: {Errors}. User: {UserName} (Id: {UserId})",
                        model.InsuranceTariffId, string.Join(", ", validationResult.Data.Values), _currentUserService.UserName, _currentUserService.UserId);
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

                // اعتبارسنجی سهم بیمار
                if (model.PatientShare.HasValue && (model.PatientShare < 0 || model.PatientShare > 100))
                {
                    errors.Add("PatientShare", "سهم بیمار باید بین 0 تا 100 درصد باشد");
                }

                // اعتبارسنجی سهم بیمه
                if (model.InsurerShare.HasValue && (model.InsurerShare < 0 || model.InsurerShare > 100))
                {
                    errors.Add("InsurerShare", "سهم بیمه باید بین 0 تا 100 درصد باشد");
                }

                // اعتبارسنجی مجموع سهم‌ها
                if (model.PatientShare.HasValue && model.InsurerShare.HasValue)
                {
                    var totalShare = model.PatientShare.Value + model.InsurerShare.Value;
                    if (totalShare > 100)
                    {
                        errors.Add("TotalShare", "مجموع سهم بیمار و بیمه نمی‌تواند بیش از 100 درصد باشد");
                    }
                }

                // اعتبارسنجی قیمت تعرفه
                if (model.TariffPrice.HasValue && model.TariffPrice < 0)
                {
                    errors.Add("TariffPrice", "قیمت تعرفه نمی‌تواند منفی باشد");
                }

                return errors.Count > 0 
                    ? ServiceResult<Dictionary<string, string>>.Failed("اعتبارسنجی ناموفق")
                    : ServiceResult<Dictionary<string, string>>.Successful(new Dictionary<string, string>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی تعرفه بیمه");
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
                    return ServiceResult<int>.Failed($"تعداد {createdCount} تعرفه ایجاد شد، اما {errors.Count} خطا رخ داد: {string.Join("; ", errors)}");
                }

                return ServiceResult<int>.Successful(createdCount);
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

                decimal? tariffPrice = model.TariffPrice;
                decimal? patientShare = model.PatientShare;
                decimal? insurerShare = model.InsurerShare;

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

                // محاسبه سهم بیمه و بیمار بر اساس درصد (مطابق با Entity Model)
                if (tariffPrice.HasValue)
                {
                    // تنظیم مقادیر نهایی بر اساس درصد
                    if (!insurerShare.HasValue)
                    {
                        insurerShare = plan.CoveragePercent; // درصد پوشش بیمه
                        _logger.Information("🏥 MEDICAL: سهم بیمه محاسبه شد - PlanId: {PlanId}, CoveragePercent: {CoveragePercent}%, User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, plan.CoveragePercent, _currentUserService.UserName, _currentUserService.UserId);
                    }

                    if (!patientShare.HasValue)
                    {
                        patientShare = 100 - plan.CoveragePercent; // درصد سهم بیمار
                        _logger.Information("🏥 MEDICAL: سهم بیمار محاسبه شد - PlanId: {PlanId}, PatientShare: {PatientShare}%, User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, patientShare, _currentUserService.UserName, _currentUserService.UserId);
                    }

                    _logger.Information("🏥 MEDICAL: محاسبات کامل تعرفه - ServicePrice: {ServicePrice}, InsurerShare: {InsurerShare}%, PatientShare: {PatientShare}%, CoveragePercent: {CoveragePercent}%, User: {UserName} (Id: {UserId})",
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

                decimal? tariffPrice = model.TariffPrice;
                decimal? patientShare = model.PatientShare;
                decimal? insurerShare = model.InsurerShare;

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

                // محاسبه سهم بیمه و بیمار بر اساس درصد (مطابق با Entity Model)
                if (tariffPrice.HasValue)
                {
                    // تنظیم مقادیر نهایی بر اساس درصد
                    if (!insurerShare.HasValue)
                    {
                        insurerShare = plan.CoveragePercent; // درصد پوشش بیمه
                        _logger.Information("🏥 MEDICAL: سهم بیمه محاسبه شد در Bulk - PlanId: {PlanId}, CoveragePercent: {CoveragePercent}%, User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, plan.CoveragePercent, _currentUserService.UserName, _currentUserService.UserId);
                    }

                    if (!patientShare.HasValue)
                    {
                        patientShare = 100 - plan.CoveragePercent; // درصد سهم بیمار
                        _logger.Information("🏥 MEDICAL: سهم بیمار محاسبه شد در Bulk - PlanId: {PlanId}, PatientShare: {PatientShare}%, User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, patientShare, _currentUserService.UserName, _currentUserService.UserId);
                    }

                    _logger.Information("🏥 MEDICAL: محاسبات کامل تعرفه در Bulk - ServicePrice: {ServicePrice}, InsurerShare: {InsurerShare}%, PatientShare: {PatientShare}%, CoveragePercent: {CoveragePercent}%, User: {UserName} (Id: {UserId})",
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

                var viewModel = new InsuranceTariffStatisticsViewModel
                {
                    TotalTariffs = totalCount,
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

        #endregion
    }
}
