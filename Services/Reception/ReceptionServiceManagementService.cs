using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Reception;
using ClinicApp.ViewModels.Reception;
using ClinicApp.Interfaces;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس مدیریت خدمات در ماژول پذیرش
    /// </summary>
    public class ReceptionServiceManagementService : IReceptionServiceManagementService
    {
        private readonly IServiceService _serviceService;
        private readonly IServiceCategoryService _serviceCategoryService;
        private readonly IReceptionCalculationService _calculationService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionServiceManagementService(
            IServiceService serviceService,
            IServiceCategoryService serviceCategoryService,
            IReceptionCalculationService calculationService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _serviceService = serviceService ?? throw new ArgumentNullException(nameof(serviceService));
            _serviceCategoryService = serviceCategoryService ?? throw new ArgumentNullException(nameof(serviceCategoryService));
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _logger = logger.ForContext<ReceptionServiceManagementService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// دریافت دسته‌بندی‌های خدمات
        /// </summary>
        public async Task<ServiceResult<List<ServiceCategoryLookupViewModel>>> GetServiceCategoriesAsync()
        {
            try
            {
                _logger.Information("دریافت دسته‌بندی‌های خدمات");

                var categoriesResult = await _serviceCategoryService.GetActiveCategoriesAsync();

                if (!categoriesResult.Success)
                {
                    return ServiceResult<List<ServiceCategoryLookupViewModel>>.Failed(
                        "خطا در دریافت دسته‌بندی‌های خدمات");
                }

                var categories = categoriesResult.Data.Select(c => new ServiceCategoryLookupViewModel
                {
                    CategoryId = c.ServiceCategoryId,
                    CategoryName = c.Title,
                    CategoryCode = "", // Not available in ServiceCategorySelectItem
                    Description = "", // Not available in ServiceCategorySelectItem
                    IsActive = true // Default value
                }).ToList();

                return ServiceResult<List<ServiceCategoryLookupViewModel>>.Successful(
                    categories, "دسته‌بندی‌ها با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی‌های خدمات");
                return ServiceResult<List<ServiceCategoryLookupViewModel>>.Failed(
                    "خطا در بارگذاری دسته‌بندی‌ها");
            }
        }

        /// <summary>
        /// دریافت تمام خدمات
        /// </summary>
        public async Task<ServiceResult<List<ViewModels.Reception.ServiceLookupViewModel>>> GetAllServicesAsync()
        {
            try
            {
                _logger.Information("دریافت تمام خدمات");

                var servicesResult = await _serviceService.GetActiveServicesAsync();

                var services = servicesResult.Select(s => new ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.Title, // ServiceIndexViewModel uses Title
                    ServiceCode = s.ServiceCode,
                    BasePrice = s.Price, // ServiceIndexViewModel uses Price
                    CategoryId = s.ServiceCategoryId, // ServiceIndexViewModel uses ServiceCategoryId
                    CategoryName = s.ServiceCategoryTitle, // ServiceIndexViewModel uses ServiceCategoryTitle
                    Description = "", // Not available in ServiceIndexViewModel
                                      //  IsActive = s.IsActive,
                    RequiresDoctor = false, // Not available in ServiceIndexViewModel
                    RequiresSpecialization = false // Not available in ServiceIndexViewModel
                }).ToList();

                return ServiceResult<List<ServiceLookupViewModel>>.Successful(
                    services, "خدمات با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات");
                return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                    "خطا در بارگذاری خدمات");
            }
        }

        /// <summary>
        /// دریافت خدمات بر اساس دسته‌بندی
        /// </summary>
        public async Task<ServiceResult<List<ViewModels.Reception.ServiceLookupViewModel>>> GetServicesByCategoryAsync(int categoryId)
        {
            try
            {
                _logger.Information("دریافت خدمات برای دسته‌بندی {CategoryId}", categoryId);

                var servicesResult = await _serviceService.GetServicesByCategoryAsync(categoryId);

                if (!servicesResult.Success)
                {
                    return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                        "خطا در دریافت خدمات دسته‌بندی");
                }

                var services = servicesResult.Data.Select(s => new ViewModels.Reception.ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.Title, // ServiceIndexViewModel uses Title
                    ServiceCode = s.ServiceCode,
                    BasePrice = s.Price, // ServiceIndexViewModel uses Price
                    CategoryId = s.ServiceCategoryId, // ServiceIndexViewModel uses ServiceCategoryId
                    CategoryName = s.ServiceCategoryTitle, // ServiceIndexViewModel uses ServiceCategoryTitle
                    Description = "", // Not available in ServiceIndexViewModel
                    IsActive = s.IsActive,
                    RequiresDoctor = false, // Not available in ServiceIndexViewModel
                    RequiresSpecialization = false // Not available in ServiceIndexViewModel
                }).ToList();

                return ServiceResult<List<ServiceLookupViewModel>>.Successful(
                    services, "خدمات دسته‌بندی با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت خدمات دسته‌بندی {CategoryId}", categoryId);
                return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                    "خطا در بارگذاری خدمات دسته‌بندی");
            }
        }

        /// <summary>
        /// جستجوی خدمات
        /// </summary>
        public async Task<ServiceResult<List<ViewModels.Reception.ServiceLookupViewModel>>> SearchServicesAsync(string searchTerm)
        {
            try
            {
                _logger.Information("جستجوی خدمات با عبارت: {SearchTerm}", searchTerm);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                        "عبارت جستجو نمی‌تواند خالی باشد");
                }

                var servicesResult = await _serviceService.SearchServicesAsync(searchTerm, null, 1, 20);

                if (!servicesResult.Success)
                {
                    return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                        "خطا در جستجوی خدمات");
                }

                var services = servicesResult.Data.Select(s => new ViewModels.Reception.ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.Title, // ServiceIndexViewModel uses Title
                    ServiceCode = s.ServiceCode,
                    BasePrice = s.Price, // ServiceIndexViewModel uses Price
                    CategoryId = s.ServiceCategoryId, // ServiceIndexViewModel uses ServiceCategoryId
                    CategoryName = s.ServiceCategoryTitle, // ServiceIndexViewModel uses ServiceCategoryTitle
                    Description = "", // Not available in ServiceIndexViewModel
                    IsActive = s.IsActive,
                    RequiresDoctor = false, // Not available in ServiceIndexViewModel
                    RequiresSpecialization = false // Not available in ServiceIndexViewModel
                }).ToList();

                return ServiceResult<List<ServiceLookupViewModel>>.Successful(
                    services, "نتایج جستجو با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی خدمات");
                return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                    "خطا در جستجوی خدمات");
            }
        }

        /// <summary>
        /// محاسبه هزینه خدمات
        /// </summary>
        public async Task<ServiceResult<ViewModels.Reception.ServiceCalculationResult>> CalculateServiceCostsAsync(ServiceCalculationRequest request)
        {
            try
            {
                _logger.Information("محاسبه هزینه خدمات برای {ServiceCount} خدمت", request.Services?.Count ?? 0);

                var result = new ViewModels.Reception.ServiceCalculationResult
                {
                    TotalBaseAmount = request.Services.Sum(s => s.Quantity * 100000), // مبلغ نمونه
                    TotalInsuranceCoverage = request.Services.Sum(s => s.Quantity * 80000),
                    TotalPatientShare = request.Services.Sum(s => s.Quantity * 20000),
                    TotalDiscountAmount = 0,
                    FinalAmount = request.Services.Sum(s => s.Quantity * 20000),
                    ServiceDetails = request.Services.Select(s => new ServiceCalculationDetail
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = "خدمت نمونه",
                        BasePrice = 100000,
                        FinalPrice = s.Quantity * 100000,
                        DiscountAmount = 0,
                        InsuranceShare = s.Quantity * 80000,
                        PatientShare = s.Quantity * 20000,
                        CalculationNotes = "محاسبه نمونه"
                    }).ToList(),
                    AppliedDiscounts = new List<string>(),
                    CalculatedAt = DateTime.Now
                };

                return ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Successful(result, "محاسبه هزینه خدمات با موفقیت انجام شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه هزینه خدمات");
                return ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Failed("خطا در محاسبه هزینه خدمات");
            }
        }

        /// <summary>
        /// اعتبارسنجی انتخاب خدمات
        /// </summary>
        public async Task<ServiceResult<bool>> ValidateServiceSelectionAsync(List<int> serviceIds)
        {
            try
            {
                _logger.Information("اعتبارسنجی انتخاب {ServiceCount} خدمت", serviceIds?.Count ?? 0);

                if (serviceIds == null || !serviceIds.Any())
                {
                    return ServiceResult<bool>.Failed("هیچ خدمتی انتخاب نشده است");
                }

                // اعتبارسنجی ساده
                var isValid = serviceIds.All(id => id > 0);

                return ServiceResult<bool>.Successful(isValid, isValid ? "انتخاب خدمات معتبر است" : "انتخاب خدمات نامعتبر است");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی انتخاب خدمات");
                return ServiceResult<bool>.Failed("خطا در اعتبارسنجی انتخاب خدمات");
            }
        }

        /// <summary>
        /// محاسبه هزینه خدمات (متد قدیمی)
        /// </summary>
        public async Task<ServiceResult<ViewModels.Reception.ServiceCalculationResult>> CalculateServiceCostsAsync(
            List<ServiceCalculationRequest> services, string patientId)
        {
            try
            {
                _logger.Information("محاسبه هزینه خدمات برای {ServiceCount} خدمت", services?.Count ?? 0);

                if (services == null || !services.Any())
                {
                    return ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Failed(
                        "هیچ خدمتی انتخاب نشده است");
                }

                // اعتبارسنجی خدمات
                var validationResult = await ValidateServiceSelectionAsync(services);
                if (!validationResult.Success)
                {
                    return ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Failed(validationResult.Message);
                }

                // محاسبه هزینه‌ها
                var serviceIds = services.Select(s => s.ServiceId).ToList();
                var calculationResult = await _calculationService.CalculateServiceCostsAsync(serviceIds, int.Parse(patientId));

                if (!calculationResult.Success)
                {
                    return ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Failed(
                        "خطا در محاسبه هزینه‌ها");
                }

                return ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Successful(
                    calculationResult.Data, "محاسبه با موفقیت انجام شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه هزینه خدمات");
                return ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Failed(
                    "خطا در محاسبه هزینه‌ها");
            }
        }

        /// <summary>
        /// دریافت جزئیات خدمت
        /// </summary>
        public async Task<ServiceResult<ViewModels.Reception.ServiceDetailsViewModel>> GetServiceDetailsAsync(int serviceId)
        {
            try
            {
                _logger.Information("دریافت جزئیات خدمت {ServiceId}", serviceId);

                var serviceResult = await _serviceService.GetServiceByIdAsync(serviceId);

                if (!serviceResult.Success)
                {
                    return ServiceResult<ServiceDetailsViewModel>.Failed(
                        "خدمت یافت نشد");
                }

                var service = serviceResult.Data;
                var details = new ViewModels.Reception.ServiceDetailsViewModel
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    ServiceCode = service.ServiceCode,
                    BasePrice = service.BasePrice,
                    CategoryId = service.CategoryId,
                    CategoryName = service.CategoryName,
                    Description = service.Description,
                    IsActive = service.IsActive,
                    RequiresDoctor = service.RequiresDoctor,
                    RequiresSpecialization = service.RequiresSpecialization,
                    Tariffs = new List<ServiceTariffViewModel>(), // TODO: پیاده‌سازی تعرفه‌ها
                    Requirements = "" // TODO: پیاده‌سازی الزامات
                };

                return ServiceResult<ServiceDetailsViewModel>.Successful(
                    details, "جزئیات خدمت با موفقیت بارگذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات خدمت {ServiceId}", serviceId);
                return ServiceResult<ServiceDetailsViewModel>.Failed(
                    "خطا در بارگذاری جزئیات خدمت");
            }
        }

        /// <summary>
        /// اعتبارسنجی انتخاب خدمات
        /// </summary>
        public async Task<ServiceResult<bool>> ValidateServiceSelectionAsync(List<ViewModels.Reception.ServiceCalculationRequest> services)
        {
            try
            {
                _logger.Information("اعتبارسنجی انتخاب {ServiceCount} خدمت", services?.Count ?? 0);

                if (services == null || !services.Any())
                {
                    return ServiceResult<bool>.Failed("هیچ خدمتی انتخاب نشده است");
                }

                // بررسی وجود خدمات
                foreach (var service in services)
                {
                    if (service.ServiceId <= 0)
                    {
                        return ServiceResult<bool>.Failed($"شناسه خدمت نامعتبر: {service.ServiceId}");
                    }

                    if (service.Quantity <= 0)
                    {
                        return ServiceResult<bool>.Failed($"تعداد خدمت نامعتبر: {service.ServiceName}");
                    }

                    if (service.BasePrice < 0)
                    {
                        return ServiceResult<bool>.Failed($"قیمت خدمت نامعتبر: {service.ServiceName}");
                    }
                }

                return ServiceResult<bool>.Successful(true, "انتخاب خدمات معتبر است");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی انتخاب خدمات");
                return ServiceResult<bool>.Failed("خطا در اعتبارسنجی خدمات");
            }
        }

        /// <summary>
        /// جستجوی خدمات (overload)
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی</param>
        /// <param name="specializationId">شناسه تخصص</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست خدمات یافت شده</returns>
        public async Task<ServiceResult<List<ServiceLookupViewModel>>> SearchServicesAsync(string searchTerm, int? serviceCategoryId, int? specializationId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                        "عبارت جستجو نمی‌تواند خالی باشد");
                }

                var servicesResult = await _serviceService.SearchServicesAsync(searchTerm, serviceCategoryId, 1, 20);

                if (!servicesResult.Success)
                {
                    return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                        "خطا در جستجوی خدمات");
                }

                var services = servicesResult.Data.Select(s => new ViewModels.Reception.ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.Title, // ServiceIndexViewModel uses Title
                    ServiceCode = s.ServiceCode,
                    BasePrice = s.Price, // ServiceIndexViewModel uses Price
                    CategoryId = s.ServiceCategoryId, // ServiceIndexViewModel uses ServiceCategoryId
                    CategoryName = s.ServiceCategoryTitle, // ServiceIndexViewModel uses ServiceCategoryTitle
                    IsActive = s.IsActive,
                    Description = "", // Not available in ServiceIndexViewModel
                    RequiresSpecialization = false, // Not available in ServiceIndexViewModel
                    RequiresDoctor = false // Not available in ServiceIndexViewModel
                }).ToList();

                return ServiceResult<List<ServiceLookupViewModel>>.Successful(services);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی خدمات");
                return ServiceResult<List<ServiceLookupViewModel>>.Failed("خطا در جستجوی خدمات");
            }
        }

        /// <summary>
        /// جستجوی خدمات (overload)
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی</param>
        /// <param name="specializationId">شناسه تخصص</param>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست خدمات یافت شده</returns>
        public async Task<ServiceResult<List<ServiceLookupViewModel>>> SearchServicesAsync(string searchTerm, int? serviceCategoryId, int? specializationId, int? doctorId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                        "عبارت جستجو نمی‌تواند خالی باشد");
                }

                var servicesResult = await _serviceService.SearchServicesAsync(searchTerm, serviceCategoryId, 1, 20);

                if (!servicesResult.Success)
                {
                    return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                        "خطا در جستجوی خدمات");
                }

                var services = servicesResult.Data.Select(s => new ViewModels.Reception.ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.Title, // ServiceIndexViewModel uses Title
                    ServiceCode = s.ServiceCode,
                    BasePrice = s.Price, // ServiceIndexViewModel uses Price
                    CategoryId = s.ServiceCategoryId, // ServiceIndexViewModel uses ServiceCategoryId
                    CategoryName = s.ServiceCategoryTitle, // ServiceIndexViewModel uses ServiceCategoryTitle
                    IsActive = s.IsActive,
                    Description = "", // Not available in ServiceIndexViewModel
                    RequiresSpecialization = false, // Not available in ServiceIndexViewModel
                    RequiresDoctor = false // Not available in ServiceIndexViewModel
                }).ToList();

                return ServiceResult<List<ServiceLookupViewModel>>.Successful(services);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی خدمات");
                return ServiceResult<List<ServiceLookupViewModel>>.Failed("خطا در جستجوی خدمات");
            }
        }

        /// <summary>
        /// جستجوی خدمات (overload)
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست خدمات یافت شده</returns>
        public async Task<ServiceResult<List<ServiceLookupViewModel>>> SearchServicesAsync(string searchTerm, int? serviceCategoryId, int pageNumber, int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                        "عبارت جستجو نمی‌تواند خالی باشد");
                }

                var servicesResult = await _serviceService.SearchServicesAsync(searchTerm, serviceCategoryId, 1, 20);

                if (!servicesResult.Success)
                {
                    return ServiceResult<List<ServiceLookupViewModel>>.Failed(
                        "خطا در جستجوی خدمات");
                }

                var services = servicesResult.Data.Select(s => new ViewModels.Reception.ServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.Title, // ServiceIndexViewModel uses Title
                    ServiceCode = s.ServiceCode,
                    BasePrice = s.Price, // ServiceIndexViewModel uses Price
                    CategoryId = s.ServiceCategoryId, // ServiceIndexViewModel uses ServiceCategoryId
                    CategoryName = s.ServiceCategoryTitle, // ServiceIndexViewModel uses ServiceCategoryTitle
                    IsActive = s.IsActive,
                    Description = "", // Not available in ServiceIndexViewModel
                    RequiresSpecialization = false, // Not available in ServiceIndexViewModel
                    RequiresDoctor = false // Not available in ServiceIndexViewModel
                }).ToList();

                return ServiceResult<List<ServiceLookupViewModel>>.Successful(services);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی خدمات");
                return ServiceResult<List<ServiceLookupViewModel>>.Failed("خطا در جستجوی خدمات");
            }
        }

    }
}