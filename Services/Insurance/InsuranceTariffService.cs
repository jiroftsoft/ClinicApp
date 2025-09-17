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
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public InsuranceTariffService(
            IInsuranceTariffRepository tariffRepository,
            IInsurancePlanRepository planRepository,
            IServiceRepository serviceRepository,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _tariffRepository = tariffRepository ?? throw new ArgumentNullException(nameof(tariffRepository));
            _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
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

                // بررسی وجود تعرفه مشابه
                var exists = await _tariffRepository.DoesTariffExistAsync(model.InsurancePlanId, model.ServiceId);
                if (exists)
                {
                    _logger.Warning("تعرفه بیمه مشابه وجود دارد. PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<int>.Failed("تعرفه بیمه برای این طرح و خدمت قبلاً تعریف شده است");
                }

                // ایجاد entity
                var tariff = new InsuranceTariff
                {
                    ServiceId = model.ServiceId,
                    InsurancePlanId = model.InsurancePlanId,
                    TariffPrice = model.TariffPrice,
                    PatientShare = model.PatientShare,
                    InsurerShare = model.InsurerShare,
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

                // بررسی وجود تعرفه مشابه (به جز خودش)
                var exists = await _tariffRepository.DoesTariffExistAsync(model.InsurancePlanId, model.ServiceId, model.InsuranceTariffId);
                if (exists)
                {
                    _logger.Warning("تعرفه بیمه مشابه وجود دارد. Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        model.InsuranceTariffId, model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("تعرفه بیمه برای این طرح و خدمت قبلاً تعریف شده است");
                }

                // به‌روزرسانی
                existingTariff.ServiceId = model.ServiceId;
                existingTariff.InsurancePlanId = model.InsurancePlanId;
                existingTariff.TariffPrice = model.TariffPrice;
                existingTariff.PatientShare = model.PatientShare;
                existingTariff.InsurerShare = model.InsurerShare;
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

                // بررسی وجود خدمت
                var service = await _serviceRepository.GetServiceByIdAsync(model.ServiceId);
                if (service == null)
                {
                    errors.Add("ServiceId", "خدمت یافت نشد");
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
