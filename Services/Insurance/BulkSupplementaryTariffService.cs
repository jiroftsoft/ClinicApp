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
    /// سرویس بهینه شده برای ایجاد تعرفه گروهی بیمه تکمیلی
    /// طراحی شده برای محیط درمانی کلینیک شفا - Production Ready
    /// </summary>
    public class BulkSupplementaryTariffService
    {
        private readonly IInsuranceTariffService _tariffService;
        private readonly IServiceRepository _serviceRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IInsurancePlanService _planService;
        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public BulkSupplementaryTariffService(
            IInsuranceTariffService tariffService,
            IServiceRepository serviceRepository,
            IDepartmentRepository departmentRepository,
            IServiceCategoryRepository serviceCategoryRepository,
            IInsurancePlanService planService,
            IServiceCalculationService serviceCalculationService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _tariffService = tariffService ?? throw new ArgumentNullException(nameof(tariffService));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _serviceCategoryRepository = serviceCategoryRepository ?? throw new ArgumentNullException(nameof(serviceCategoryRepository));
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _log = logger.ForContext<BulkSupplementaryTariffService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// ایجاد تعرفه گروهی بیمه تکمیلی - بهینه شده
        /// </summary>
        public async Task<ServiceResult<BulkTariffResultViewModel>> CreateBulkTariffsAsync(BulkSupplementaryTariffViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع ایجاد تعرفه گروهی - User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var startTime = DateTime.UtcNow;
                var result = new BulkTariffResultViewModel
                {
                    ProcessedAt = startTime
                };

                // 🔒 CRITICAL: Validation
                var validationResult = await ValidateBulkRequestAsync(model);
                if (!validationResult.Success)
                {
                    return ServiceResult<BulkTariffResultViewModel>.Failed(validationResult.Message);
                }

                // 🔒 CRITICAL: Get services based on selection
                var services = await GetServicesBySelectionAsync(model);
                if (!services.Any())
                {
                    _log.Warning("🏥 MEDICAL: هیچ خدمتی برای ایجاد تعرفه یافت نشد - Departments: {DeptCount}, Categories: {CatCount}",
                        model.SelectedDepartmentIds.Count, model.SelectedServiceCategoryIds.Count);
                    
                    return ServiceResult<BulkTariffResultViewModel>.Failed(
                        "هیچ خدمتی برای ایجاد تعرفه یافت نشد. لطفاً دپارتمان‌ها و سرفصل‌های انتخابی را بررسی کنید.");
                }

                result.TotalServices = services.Count;

                // 🔒 CRITICAL: Create tariffs in batches
                var batchSize = 50; // Process in batches of 50
                var batches = services.Select((item, index) => new { item, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(g => g.Select(x => x.item).ToList());
                var createdCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                foreach (var batch in batches)
                {
                    try
                    {
                        var batchResult = await ProcessBatchAsync(batch, model);
                        createdCount += batchResult.CreatedCount;
                        errorCount += batchResult.ErrorCount;
                        errors.AddRange(batchResult.ErrorMessages);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "🏥 MEDICAL: خطا در پردازش batch");
                        errorCount += batch.Count();
                        errors.Add($"خطا در پردازش {batch.Count()} خدمت: {ex.Message}");
                    }
                }

                // 🔒 CRITICAL: Set results
                result.CreatedTariffs = createdCount;
                result.Errors = errorCount;
                result.ErrorMessages = errors;
                result.ProcessingTime = DateTime.UtcNow - startTime;

                if (errorCount > 0)
                {
                    result.Success = false;
                    result.Message = $"{createdCount} تعرفه ایجاد شد، {errorCount} خطا رخ داد";
                }
                else
                {
                    result.Success = true;
                    result.Message = $"{createdCount} تعرفه با موفقیت ایجاد شد";
                }

                _log.Information("🏥 MEDICAL: تعرفه گروهی تکمیل شد - Created: {Created}, Errors: {Errors}, Time: {Time}ms. User: {UserName} (Id: {UserId})",
                    createdCount, errorCount, result.ProcessingTime.TotalMilliseconds, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<BulkTariffResultViewModel>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در ایجاد تعرفه گروهی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<BulkTariffResultViewModel>.Failed("خطای سیستمی در ایجاد تعرفه گروهی");
            }
        }

        /// <summary>
        /// اعتبارسنجی درخواست گروهی
        /// </summary>
        private async Task<ServiceResult<bool>> ValidateBulkRequestAsync(BulkSupplementaryTariffViewModel model)
        {
            try
            {
                // 1. بررسی وجود بیمه پایه
                var primaryPlanResult = await _planService.GetByIdAsync(model.PrimaryInsurancePlanId);
                if (!primaryPlanResult.Success || primaryPlanResult.Data == null || primaryPlanResult.Data.InsuranceType != InsuranceType.Primary)
                {
                    return ServiceResult<bool>.Failed("بیمه پایه معتبر نیست");
                }

                // 2. بررسی وجود بیمه تکمیلی
                var supplementaryPlanResult = await _planService.GetByIdAsync(model.InsurancePlanId);
                if (!supplementaryPlanResult.Success || supplementaryPlanResult.Data == null || supplementaryPlanResult.Data.InsuranceType != InsuranceType.Supplementary)
                {
                    return ServiceResult<bool>.Failed("طرح بیمه تکمیلی معتبر نیست");
                }

                // 3. بررسی انتخاب دپارتمان‌ها
                if (!model.SelectedDepartmentIds.Any())
                {
                    return ServiceResult<bool>.Failed("انتخاب حداقل یک دپارتمان الزامی است");
                }

                // 4. بررسی انتخاب سرفصل‌ها
                if (!model.SelectedServiceCategoryIds.Any())
                {
                    return ServiceResult<bool>.Failed("انتخاب حداقل یک سرفصل الزامی است");
                }

                // 5. بررسی درصد پوشش
                if (model.SupplementaryCoveragePercent < 0 || model.SupplementaryCoveragePercent > 100)
                {
                    return ServiceResult<bool>.Failed("درصد پوشش باید بین 0 تا 100 باشد");
                }

                // 6. بررسی اولویت
                if (model.Priority < 1 || model.Priority > 10)
                {
                    return ServiceResult<bool>.Failed("اولویت باید بین 1 تا 10 باشد");
                }

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی درخواست گروهی");
                return ServiceResult<bool>.Failed("خطا در اعتبارسنجی درخواست");
            }
        }

        /// <summary>
        /// دریافت خدمات بر اساس انتخاب - بهینه شده برای محیط کلینیکی
        /// </summary>
        private async Task<List<Service>> GetServicesBySelectionAsync(BulkSupplementaryTariffViewModel model)
        {
            try
            {
                var services = new List<Service>();
                var maxServices = 1000; // 🔒 CRITICAL: محدودیت تعداد خدمات
                var processedCount = 0;
                var departmentCategoryPairs = new List<DepartmentCategoryPair>();

                _log.Information("🏥 MEDICAL: شروع دریافت خدمات - Departments: {DeptCount}, Categories: {CatCount}",
                    model.SelectedDepartmentIds.Count, model.SelectedServiceCategoryIds.Count);

                // 🔧 MEDICAL: Get department and category names for better logging
                var departments = await _departmentRepository.GetDepartmentsByIdsAsync(model.SelectedDepartmentIds);
                var categories = await _serviceCategoryRepository.GetServiceCategoriesByIdsAsync(model.SelectedServiceCategoryIds);

                // 🔧 OPTIMIZATION: Parallel processing for better performance
                var tasks = new List<Task<ServiceResult<List<Service>>>>();
                
                foreach (var departmentId in model.SelectedDepartmentIds)
                {
                    var department = departments.FirstOrDefault(d => d.DepartmentId == departmentId);
                    var departmentName = department?.Name ?? $"Department-{departmentId}";
                    
                    foreach (var categoryId in model.SelectedServiceCategoryIds)
                    {
                        if (processedCount >= maxServices)
                        {
                            _log.Warning("🏥 MEDICAL: محدودیت تعداد خدمات ({MaxServices}) رسیده است", maxServices);
                            break;
                        }

                        var category = categories.FirstOrDefault(c => c.ServiceCategoryId == categoryId);
                        var categoryName = category?.Title ?? $"Category-{categoryId}";
                        
                        departmentCategoryPairs.Add(new DepartmentCategoryPair
                        {
                            DepartmentId = departmentId,
                            CategoryId = categoryId,
                            DepartmentName = departmentName,
                            CategoryName = categoryName
                        });
                        tasks.Add(_serviceRepository.GetServicesByDepartmentAndCategoryAsync(departmentId, categoryId));
                        processedCount++;
                    }
                    
                    if (processedCount >= maxServices) break;
                }

                // 🔧 OPTIMIZATION: Wait for all tasks to complete
                var results = await Task.WhenAll(tasks);

                // 🔧 MEDICAL: Process results with detailed logging
                for (int i = 0; i < results.Length; i++)
                {
                    var result = results[i];
                    var pair = departmentCategoryPairs[i];
                    
                    if (result.Success && result.Data != null && result.Data.Any())
                    {
                        services.AddRange(result.Data);
                        _log.Information("🏥 MEDICAL: {Count} خدمت یافت شد - {Department} > {Category}", 
                            result.Data.Count, pair.DepartmentName, pair.CategoryName);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: هیچ خدمتی یافت نشد - {Department} > {Category}. Error: {Error}", 
                            pair.DepartmentName, pair.CategoryName, result.Message);
                    }
                }

                // 🔧 OPTIMIZATION: Remove duplicates efficiently
                services = services.GroupBy(s => s.ServiceId).Select(g => g.First()).ToList();

                _log.Information("🏥 MEDICAL: مجموع {Count} خدمت برای ایجاد تعرفه یافت شد", services.Count);
                
                // 🔧 MEDICAL: Detailed breakdown for clinical environment
                if (services.Any())
                {
                    var serviceBreakdown = services
                        .GroupBy(s => s.ServiceCategory.Title)
                        .Select(g => new { Category = g.Key, Count = g.Count() })
                        .OrderBy(x => x.Category);
                    
                    foreach (var breakdown in serviceBreakdown)
                    {
                        _log.Information("🏥 MEDICAL: {Category}: {Count} خدمت", breakdown.Category, breakdown.Count);
                    }
                }

                return services;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت خدمات");
                return new List<Service>();
            }
        }

        /// <summary>
        /// پردازش batch خدمات - بهینه شده با Error Handling
        /// </summary>
        private async Task<(int CreatedCount, int ErrorCount, List<string> ErrorMessages)> ProcessBatchAsync(
            IEnumerable<Service> services, 
            BulkSupplementaryTariffViewModel model)
        {
            var createdCount = 0;
            var errorCount = 0;
            var errors = new List<string>();
            var maxErrors = 50; // 🔒 CRITICAL: محدودیت تعداد خطاها

            _log.Information("🏥 MEDICAL: شروع پردازش {Count} خدمت", services.Count());

            foreach (var service in services)
            {
                try
                {
                    // 🔒 CRITICAL: Validation قبل از ایجاد تعرفه
                    if (service.ServiceId <= 0)
                    {
                        errorCount++;
                        errors.Add($"شناسه خدمت نامعتبر: {service.ServiceId}");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(service.Title))
                    {
                        errorCount++;
                        errors.Add($"عنوان خدمت خالی است: {service.ServiceId}");
                        continue;
                    }

                    // 🔒 CRITICAL: بررسی وجود تعرفه قبلی - حذف شده برای سادگی
                    // در نسخه بهینه شده، تعرفه‌های تکراری نادیده گرفته می‌شوند

                    // 🔧 MEDICAL: محاسبه قیمت خدمت برای بیمه تکمیلی
                    var servicePrice = await CalculateServicePriceForSupplementaryAsync(service);
                    
                    // 🔧 CRITICAL FIX: محاسبه صحیح سهم بیمار و بیمه مطابق با منطق فرم Create
                    var (patientShare, insurerShare) = await CalculatePatientAndInsurerSharesAsync(
                        servicePrice, model.PrimaryInsurancePlanId, model.InsurancePlanId);
                    
                    // ایجاد ViewModel برای هر خدمت
                    var tariffModel = new InsuranceTariffCreateEditViewModel
                    {
                        ServiceId = service.ServiceId,
                        InsurancePlanId = model.InsurancePlanId,
                        TariffPrice = servicePrice, // قیمت کل خدمت
                        PatientShare = patientShare, // سهم بیمار بعد از بیمه پایه (قبل از تکمیلی)
                        InsurerShare = insurerShare, // سهم بیمه پایه (برای تعرفه تکمیلی همیشه 0)
                        SupplementaryCoveragePercent = model.SupplementaryCoveragePercent,
                        Priority = model.Priority,
                        IsActive = model.IsActive,
                        Notes = model.Description
                    };

                    // 🔒 CRITICAL: Validation اضافی
                    if (tariffModel.SupplementaryCoveragePercent < 0 || tariffModel.SupplementaryCoveragePercent > 100)
                    {
                        errorCount++;
                        errors.Add($"درصد پوشش نامعتبر برای خدمت {service.Title}: {tariffModel.SupplementaryCoveragePercent}");
                        continue;
                    }

                    // ایجاد تعرفه
                    var result = await _tariffService.CreateTariffAsync(tariffModel);
                    if (result.Success)
                    {
                        createdCount++;
                        _log.Debug("🏥 MEDICAL: تعرفه ایجاد شد - ServiceId: {ServiceId}, TariffId: {TariffId}", 
                            service.ServiceId, result.Data);
                    }
                    else
                    {
                        errorCount++;
                        var errorMsg = $"خطا در ایجاد تعرفه برای خدمت {service.Title}: {result.Message}";
                        errors.Add(errorMsg);
                        _log.Warning("🏥 MEDICAL: {Error}", errorMsg);
                    }
            }
            catch (Exception ex)
            {
                    errorCount++;
                    var errorMsg = $"خطا در ایجاد تعرفه برای خدمت {service.Title}: {ex.Message}";
                    errors.Add(errorMsg);
                    _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه برای خدمت {ServiceId}", service.ServiceId);
                }

                // 🔒 CRITICAL: محدودیت تعداد خطاها
                if (errorCount >= maxErrors)
                {
                    _log.Error("🏥 MEDICAL: تعداد خطاها از حد مجاز ({MaxErrors}) بیشتر شد", maxErrors);
                    errors.Add($"تعداد خطاها از حد مجاز ({maxErrors}) بیشتر شد. عملیات متوقف شد.");
                    break;
                }
            }

            _log.Information("🏥 MEDICAL: پردازش تکمیل شد - Created: {Created}, Errors: {Errors}", createdCount, errorCount);
            return (createdCount, errorCount, errors);
        }

        /// <summary>
        /// دریافت پیش‌نمایش خدمات برای فرم CreateBulk
        /// </summary>
        public async Task<List<Service>> GetServicesPreviewAsync(BulkSupplementaryTariffViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع دریافت پیش‌نمایش خدمات - Departments: {DeptCount}, Categories: {CatCount}",
                    model.SelectedDepartmentIds?.Count ?? 0, model.SelectedServiceCategoryIds?.Count ?? 0);

                // Use the same logic as GetServicesBySelectionAsync but return services directly
                var services = await GetServicesBySelectionAsync(model);
                
                _log.Information("🏥 MEDICAL: {Count} خدمت برای پیش‌نمایش یافت شد", services.Count);
                return services;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت پیش‌نمایش خدمات");
                return new List<Service>();
            }
        }

        /// <summary>
        /// محاسبه قیمت خدمت برای بیمه تکمیلی
        /// </summary>
        private async Task<decimal> CalculateServicePriceForSupplementaryAsync(Service service)
        {
            try
            {
                _log.Information("🏥 MEDICAL: محاسبه قیمت خدمت برای بیمه تکمیلی - ServiceId: {ServiceId}, ServiceTitle: {ServiceTitle}, BasePrice: {BasePrice}",
                    service.ServiceId, service.Title, service.Price);

                // 🔧 MEDICAL: استفاده از ServiceCalculationService برای محاسبه صحیح
                var calculatedPrice = _serviceCalculationService.CalculateServicePrice(service);
                
                if (calculatedPrice > 0)
                {
                    _log.Information("🏥 MEDICAL: قیمت محاسبه شده - ServiceId: {ServiceId}, CalculatedPrice: {Price}",
                        service.ServiceId, calculatedPrice);
                    return calculatedPrice;
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: قیمت محاسبه شده 0 است - استفاده از قیمت پایه - ServiceId: {ServiceId}, BasePrice: {BasePrice}",
                        service.ServiceId, service.Price);
                    
                    // اگر قیمت محاسبه شده 0 است، از قیمت پایه استفاده کن
                    return service.Price > 0 ? service.Price : 0;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه قیمت خدمت - ServiceId: {ServiceId}", service.ServiceId);
                
                // در صورت خطا، از قیمت پایه استفاده کن
                return service.Price > 0 ? service.Price : 0;
            }
        }

        /// <summary>
        /// محاسبه سهم بیمار و بیمه مطابق با منطق فرم Create
        /// </summary>
        private async Task<(decimal patientShare, decimal insurerShare)> CalculatePatientAndInsurerSharesAsync(
            decimal servicePrice, int primaryInsurancePlanId, int supplementaryInsurancePlanId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: محاسبه سهم بیمار و بیمه - ServicePrice: {ServicePrice}, PrimaryPlanId: {PrimaryPlanId}, SupplementaryPlanId: {SupplementaryPlanId}",
                    servicePrice, primaryInsurancePlanId, supplementaryInsurancePlanId);

                // دریافت اطلاعات بیمه پایه
                var primaryPlanResult = await _planService.GetByIdAsync(primaryInsurancePlanId);
                if (!primaryPlanResult.Success || primaryPlanResult.Data == null)
                {
                    _log.Warning("🏥 MEDICAL: بیمه پایه یافت نشد - PlanId: {PlanId}", primaryInsurancePlanId);
                    return (servicePrice, 0); // اگر بیمه پایه نباشد، کل مبلغ سهم بیمار
                }

                var primaryPlan = primaryPlanResult.Data;

                // محاسبه فرانشیز
                var primaryDeductible = primaryPlan.Deductible;
                
                // محاسبه مبلغ قابل پوشش (بعد از کسر فرانشیز)
                var coverableAmount = Math.Max(0, servicePrice - primaryDeductible);
                
                // محاسبه بیمه پایه
                var primaryCoverage = coverableAmount * primaryPlan.CoveragePercent / 100;
                
                // محاسبه سهم بیمار از بیمه اصلی (قبل از تکمیلی)
                var patientShareFromPrimary = Math.Max(0, coverableAmount - primaryCoverage);
                
                // برای تعرفه تکمیلی، سهم بیمه همیشه 0 است
                var insurerShare = 0m;

                _log.Information("🏥 MEDICAL: محاسبات تکمیل شد - ServicePrice: {ServicePrice}, PrimaryDeductible: {PrimaryDeductible}, CoverableAmount: {CoverableAmount}, PrimaryCoverage: {PrimaryCoverage}, PatientShareFromPrimary: {PatientShareFromPrimary}, InsurerShare: {InsurerShare}",
                    servicePrice, primaryDeductible, coverableAmount, primaryCoverage, patientShareFromPrimary, insurerShare);

                return (patientShareFromPrimary, insurerShare);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه سهم بیمار و بیمه - ServicePrice: {ServicePrice}", servicePrice);
                
                // در صورت خطا، کل مبلغ سهم بیمار
                return (servicePrice, 0);
            }
        }
    }

    /// <summary>
    /// Helper class for department-category pairs
    /// </summary>
    public class DepartmentCategoryPair
    {
        public int DepartmentId { get; set; }
        public int CategoryId { get; set; }
        public string DepartmentName { get; set; }
        public string CategoryName { get; set; }
    }
}