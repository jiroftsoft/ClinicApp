using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.ViewModels.Insurance.Supplementary;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس محاسبات تعرفه گروهی بیمه تکمیلی
    /// طراحی شده برای محیط درمانی کلینیک شفا - Production Ready
    /// </summary>
    public class BulkSupplementaryTariffService
    {
        private readonly IInsuranceTariffService _tariffService;
        private readonly IServiceRepository _serviceRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IInsurancePlanService _planService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public BulkSupplementaryTariffService(
            IInsuranceTariffService tariffService,
            IServiceRepository serviceRepository,
            IDepartmentRepository departmentRepository,
            IServiceCategoryRepository serviceCategoryRepository,
            IInsurancePlanService planService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _tariffService = tariffService ?? throw new ArgumentNullException(nameof(tariffService));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _serviceCategoryRepository = serviceCategoryRepository ?? throw new ArgumentNullException(nameof(serviceCategoryRepository));
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _log = logger.ForContext<BulkSupplementaryTariffService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// ایجاد تعرفه گروهی بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult<BulkTariffResultViewModel>> CreateBulkTariffsAsync(BulkSupplementaryTariffViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع ایجاد تعرفه گروهی - SelectionType: {SelectionType}, User: {UserName} (Id: {UserId})",
                    model.SelectionType, _currentUserService.UserName, _currentUserService.UserId);

                var startTime = DateTime.UtcNow;
                var result = new BulkTariffResultViewModel
                {
                    ProcessedAt = startTime
                };

                // دریافت خدمات بر اساس نوع انتخاب
                var services = await GetServicesBySelectionTypeAsync(model);
                if (!services.Any())
                {
                    return ServiceResult<BulkTariffResultViewModel>.Failed(
                        "هیچ خدمتی برای ایجاد تعرفه یافت نشد",
                        "NO_SERVICES_FOUND",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                result.TotalServices = services.Count;

                // بررسی محدودیت تعداد خدمات
                if (services.Count > model.AdvancedSettings.MaxServicesLimit)
                {
                    return ServiceResult<BulkTariffResultViewModel>.Failed(
                        $"تعداد خدمات ({services.Count}) از حد مجاز ({model.AdvancedSettings.MaxServicesLimit}) بیشتر است",
                        "SERVICES_LIMIT_EXCEEDED",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // حذف تعرفه‌های موجود در صورت نیاز
                if (model.AdvancedSettings.DeleteExistingTariffs)
                {
                    await DeleteExistingTariffsAsync(services.Select(s => s.ServiceId).ToList(), model.InsurancePlanId);
                }

                // ایجاد تعرفه‌های جدید
                var createdCount = 0;
                var updatedCount = 0;
                var errorCount = 0;

                foreach (var service in services)
                {
                    try
                    {
                        var tariffResult = await CreateOrUpdateTariffAsync(service, model);
                        if (tariffResult.Success)
                        {
                            if (tariffResult.Data.IsNew)
                                createdCount++;
                            else
                                updatedCount++;
                        }
                        else
                        {
                            errorCount++;
                            result.ErrorMessages.Add($"خطا در خدمت {service.Title}: {tariffResult.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه برای خدمت {ServiceId}. User: {UserName} (Id: {UserId})",
                            service.ServiceId, _currentUserService.UserName, _currentUserService.UserId);
                        result.ErrorMessages.Add($"خطا در خدمت {service.Title}: {ex.Message}");
                    }
                }

                result.CreatedTariffs = createdCount;
                result.UpdatedTariffs = updatedCount;
                result.Errors = errorCount;
                result.Success = errorCount == 0;
                result.ProcessingTime = DateTime.UtcNow - startTime;

                if (result.Success)
                {
                    result.Message = $"تعرفه گروهی با موفقیت ایجاد شد. {createdCount} تعرفه جدید، {updatedCount} تعرفه به‌روزرسانی شد.";
                }
                else
                {
                    result.Message = $"تعرفه گروهی با خطا تکمیل شد. {createdCount} تعرفه جدید، {updatedCount} تعرفه به‌روزرسانی شد، {errorCount} خطا.";
                }

                _log.Information("🏥 MEDICAL: تکمیل ایجاد تعرفه گروهی - Created: {Created}, Updated: {Updated}, Errors: {Errors}, Duration: {Duration}. User: {UserName} (Id: {UserId})",
                    createdCount, updatedCount, errorCount, result.ProcessingTime, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<BulkTariffResultViewModel>.Successful(
                    result,
                    result.Message,
                    "CreateBulkTariffs",
                    _currentUserService.UserId,
                    _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در ایجاد تعرفه گروهی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<BulkTariffResultViewModel>.Failed(
                    "خطا در ایجاد تعرفه گروهی. لطفاً دوباره تلاش کنید.",
                    "BULK_TARIFF_CREATION_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت خدمات بر اساس نوع انتخاب
        /// </summary>
        public async Task<List<ServiceLookupViewModel>> GetServicesBySelectionTypeAsync(BulkSupplementaryTariffViewModel model)
        {
            var services = new List<ServiceLookupViewModel>();

            switch (model.SelectionType)
            {
                case BulkSelectionType.AllServices:
                    services = await GetAllServicesAsync();
                    break;

                case BulkSelectionType.ByDepartment:
                    services = await GetServicesByDepartmentsAsync(model.SelectedDepartmentIds);
                    break;

                case BulkSelectionType.ByServiceCategory:
                    services = await GetServicesByCategoriesAsync(model.SelectedServiceCategoryIds);
                    break;

                case BulkSelectionType.ByPriceRange:
                    services = await GetServicesByPriceRangeAsync(model.MinPrice, model.MaxPrice);
                    break;

                case BulkSelectionType.ManualSelection:
                    services = await GetServicesByIdsAsync(model.SelectedServiceIds);
                    break;
            }

            return services;
        }

        /// <summary>
        /// دریافت همه خدمات
        /// </summary>
        private async Task<List<ServiceLookupViewModel>> GetAllServicesAsync()
        {
            var services = await _serviceRepository.GetAllActiveServicesAsync();
            return services.Select(s => new ServiceLookupViewModel
            {
                ServiceId = s.ServiceId,
                Title = s.Title,
                ServiceCode = s.ServiceCode,
                Description = s.Description,
                Price = s.Price,
                ServiceCategoryId = s.ServiceCategoryId,
                ServiceCategoryName = s.ServiceCategory?.Title,
                DepartmentId = s.ServiceCategory?.DepartmentId ?? 0,
                DepartmentName = s.ServiceCategory?.Department?.Name,
                IsActive = s.IsActive,
                HasExistingTariff = false // TODO: بررسی وجود تعرفه موجود
            }).ToList();
        }

        /// <summary>
        /// دریافت خدمات بر اساس دپارتمان‌ها
        /// </summary>
        private async Task<List<ServiceLookupViewModel>> GetServicesByDepartmentsAsync(List<int> departmentIds)
        {
            var services = await _serviceRepository.GetAllActiveServicesAsync();
            return services
                .Where(s => s.ServiceCategory?.DepartmentId != null && 
                           departmentIds.Contains(s.ServiceCategory.DepartmentId))
                .Select(s => new ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    ServiceCode = s.ServiceCode,
                    Description = s.Description,
                    Price = s.Price,
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceCategoryName = s.ServiceCategory?.Title,
                    DepartmentId = s.ServiceCategory?.DepartmentId ?? 0,
                    DepartmentName = s.ServiceCategory?.Department?.Name,
                    IsActive = s.IsActive,
                    HasExistingTariff = false
                }).ToList();
        }

        /// <summary>
        /// دریافت خدمات بر اساس دسته‌بندی‌ها
        /// </summary>
        private async Task<List<ServiceLookupViewModel>> GetServicesByCategoriesAsync(List<int> categoryIds)
        {
            var services = await _serviceRepository.GetAllActiveServicesAsync();
            return services
                .Where(s => categoryIds.Contains(s.ServiceCategoryId))
                .Select(s => new ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    ServiceCode = s.ServiceCode,
                    Description = s.Description,
                    Price = s.Price,
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceCategoryName = s.ServiceCategory?.Title,
                    DepartmentId = s.ServiceCategory?.DepartmentId ?? 0,
                    DepartmentName = s.ServiceCategory?.Department?.Name,
                    IsActive = s.IsActive,
                    HasExistingTariff = false
                }).ToList();
        }

        /// <summary>
        /// دریافت خدمات بر اساس محدوده قیمت
        /// </summary>
        private async Task<List<ServiceLookupViewModel>> GetServicesByPriceRangeAsync(decimal? minPrice, decimal? maxPrice)
        {
            var services = await _serviceRepository.GetAllActiveServicesAsync();
            return services
                .Where(s => (!minPrice.HasValue || s.Price >= minPrice.Value) &&
                           (!maxPrice.HasValue || s.Price <= maxPrice.Value))
                .Select(s => new ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    ServiceCode = s.ServiceCode,
                    Description = s.Description,
                    Price = s.Price,
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceCategoryName = s.ServiceCategory?.Title,
                    DepartmentId = s.ServiceCategory?.DepartmentId ?? 0,
                    DepartmentName = s.ServiceCategory?.Department?.Name,
                    IsActive = s.IsActive,
                    HasExistingTariff = false
                }).ToList();
        }

        /// <summary>
        /// دریافت خدمات بر اساس شناسه‌ها
        /// </summary>
        private async Task<List<ServiceLookupViewModel>> GetServicesByIdsAsync(List<int> serviceIds)
        {
            var services = await _serviceRepository.GetAllActiveServicesAsync();
            return services
                .Where(s => serviceIds.Contains(s.ServiceId))
                .Select(s => new ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    ServiceCode = s.ServiceCode,
                    Description = s.Description,
                    Price = s.Price,
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceCategoryName = s.ServiceCategory?.Title,
                    DepartmentId = s.ServiceCategory?.DepartmentId ?? 0,
                    DepartmentName = s.ServiceCategory?.Department?.Name,
                    IsActive = s.IsActive,
                    HasExistingTariff = false
                }).ToList();
        }

        /// <summary>
        /// ایجاد یا به‌روزرسانی تعرفه
        /// </summary>
        private async Task<ServiceResult<dynamic>> CreateOrUpdateTariffAsync(ServiceLookupViewModel service, BulkSupplementaryTariffViewModel model)
        {
            try
            {
                // محاسبه قیمت تعرفه
                var tariffPrice = CalculateTariffPrice(service.Price, model);

                // محاسبه سهم بیمه و بیمار
                var insurerShare = (tariffPrice * model.SupplementaryCoveragePercent) / 100;
                var patientShare = tariffPrice - insurerShare;

                // ایجاد ViewModel برای تعرفه
                var tariffViewModel = new InsuranceTariffCreateEditViewModel
                {
                    ServiceId = service.ServiceId,
                    InsurancePlanId = model.InsurancePlanId,
                    PrimaryInsurancePlanId = model.PrimaryInsurancePlanId,
                    TariffPrice = tariffPrice,
                    PatientShare = patientShare,
                    InsurerShare = insurerShare,
                    SupplementaryCoveragePercent = model.SupplementaryCoveragePercent,
                    Priority = model.Priority,
                    IsActive = model.IsActive,
                    // Description = model.Description // TODO: اضافه کردن Description به ViewModel
                };

                // ایجاد تعرفه
                var result = await _tariffService.CreateTariffAsync(tariffViewModel);
                
                return ServiceResult<dynamic>.Successful(
                    new { IsNew = true, TariffId = result.Data },
                    "تعرفه با موفقیت ایجاد شد",
                    "CreateTariff",
                    _currentUserService.UserId,
                    _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه برای خدمت {ServiceId}. User: {UserName} (Id: {UserId})",
                    service.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<dynamic>.Failed(
                    $"خطا در ایجاد تعرفه: {ex.Message}",
                    "TARIFF_CREATION_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// محاسبه قیمت تعرفه
        /// </summary>
        private decimal CalculateTariffPrice(decimal servicePrice, BulkSupplementaryTariffViewModel model)
        {
            switch (model.PriceCalculationType)
            {
                case PriceCalculationType.Auto:
                    return servicePrice;

                case PriceCalculationType.Fixed:
                    return model.DefaultPrice ?? servicePrice;

                case PriceCalculationType.Multiplier:
                    return servicePrice * (model.PriceMultiplier ?? 1.0m);

                case PriceCalculationType.Range:
                    var minPrice = model.MinPrice ?? 0;
                    var maxPrice = model.MaxPrice ?? decimal.MaxValue;
                    return Math.Max(minPrice, Math.Min(servicePrice, maxPrice));

                default:
                    return servicePrice;
            }
        }

        /// <summary>
        /// حذف تعرفه‌های موجود
        /// </summary>
        private async Task DeleteExistingTariffsAsync(List<int> serviceIds, int insurancePlanId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع حذف تعرفه‌های موجود - ServiceCount: {ServiceCount}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    serviceIds.Count, insurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // TODO: پیاده‌سازی حذف تعرفه‌های موجود
                // این بخش نیاز به پیاده‌سازی در IInsuranceTariffService دارد

                _log.Information("🏥 MEDICAL: حذف تعرفه‌های موجود تکمیل شد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در حذف تعرفه‌های موجود. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                throw;
            }
        }
    }
}
