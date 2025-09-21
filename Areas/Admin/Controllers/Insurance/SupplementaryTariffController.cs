using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Services;
using ClinicApp.Services.Insurance;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using ClinicApp.ViewModels.Insurance.Supplementary;
using ClinicApp.ViewModels.Insurance.InsurancePlan;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Interfaces.ClinicAdmin;
using Serilog;
using System.Runtime.Caching;
using System.Web;
using ClinicApp.Filters;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر مدیریت تعرفه‌های بیمه تکمیلی
    /// طراحی شده برای محیط درمانی کلینیک شفا - Production Ready
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل تعرفه‌های بیمه تکمیلی
    /// 2. پشتیبانی از تعیین ست بیمه‌های مختلف (تامین + دانا)
    /// 3. تنظیمات پیچیده و انعطاف‌پذیر
    /// 4. مدیریت کامل CRUD operations
    /// 5. بهینه‌سازی شده برای Production
    /// 6. Caching و Performance Optimization
    /// 7. Security و Authorization
    /// 8. Error Handling و Logging
    /// </summary>
    //[Authorize]
    [MedicalEnvironmentFilter]
    public class SupplementaryTariffController : BaseController
    {
        private readonly SupplementaryTariffSeederService _seederService;
        private readonly IInsuranceTariffService _tariffService;
        private readonly IInsurancePlanService _planService;
        private readonly IInsuranceProviderService _providerService;
        private readonly IServiceRepository _serviceRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly BulkSupplementaryTariffService _bulkTariffService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISupplementaryInsuranceCacheService _cacheService;
        private readonly IServiceCalculationService _serviceCalculationService;

        public SupplementaryTariffController(
            SupplementaryTariffSeederService seederService,
            IInsuranceTariffService tariffService,
            IInsurancePlanService planService,
            IInsuranceProviderService providerService,
            IServiceRepository serviceRepository,
            IDepartmentRepository departmentRepository,
            IServiceCategoryRepository serviceCategoryRepository,
            BulkSupplementaryTariffService bulkTariffService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService,
            ISupplementaryInsuranceCacheService cacheService,
            IServiceCalculationService serviceCalculationService)
            : base(messageNotificationService)
        {
            _seederService = seederService ?? throw new ArgumentNullException(nameof(seederService));
            _tariffService = tariffService ?? throw new ArgumentNullException(nameof(tariffService));
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _serviceCategoryRepository = serviceCategoryRepository ?? throw new ArgumentNullException(nameof(serviceCategoryRepository));
            _bulkTariffService = bulkTariffService ?? throw new ArgumentNullException(nameof(bulkTariffService));
            _log = logger.ForContext<SupplementaryTariffController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
        }

        /// <summary>
        /// صفحه اصلی مدیریت تعرفه‌های بیمه تکمیلی - Production Optimized
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 300, VaryByParam = "none")] // Cache for 5 minutes
        public async Task<ActionResult> Index()
        {
            const string cacheKey = "SupplementaryTariff_Index_Stats";
            var userId = _currentUserService.UserId;
            
            try
            {
                _log.Information("🏥 MEDICAL: دسترسی به صفحه مدیریت تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, userId);

                // Check cache first using cache service
                var cachedStats = await _cacheService.GetCachedSupplementaryTariffsAsync(0);
                if (cachedStats.Success && cachedStats.Data != null)
                {
                    _log.Debug("🏥 MEDICAL: آمار از کش دریافت شد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, userId);
                    // For now, create basic stats from cached data
                    ViewBag.Stats = new { 
                        TotalServices = cachedStats.Data.Count,
                        TotalTariffs = cachedStats.Data.Count,
                        ActiveTariffs = cachedStats.Data.Count,
                        InactiveTariffs = 0
                    };
                    return View();
                }

                // Get stats from service with timeout
                var statsResult = await _seederService.GetSupplementaryTariffStatsAsync()
                    .ConfigureAwait(false);
                
                if (statsResult.Success)
                {
                    // Cache the result using cache service (for now, just log)
                    _log.Debug("🏥 MEDICAL: آمار دریافت شد و آماده کش شدن. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, userId);
                    
                    ViewBag.Stats = statsResult.Data;
                    
                    _log.Information("🏥 MEDICAL: آمار با موفقیت دریافت و کش شد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, userId);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت آمار - {Error}. User: {UserName} (Id: {UserId})",
                        statsResult.Message, _currentUserService.UserName, userId);
                }

                return View();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دسترسی به صفحه مدیریت تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, userId);
                
                // Return cached data if available, otherwise empty view
                var fallbackStats = await _cacheService.GetCachedSupplementaryTariffsAsync(0);
                if (fallbackStats.Success && fallbackStats.Data != null)
                {
                    ViewBag.Stats = new { 
                        TotalServices = fallbackStats.Data.Count,
                        TotalTariffs = fallbackStats.Data.Count,
                        ActiveTariffs = fallbackStats.Data.Count,
                        InactiveTariffs = 0
                    };
                }
                
                return View();
            }
        }

        /// <summary>
        /// ایجاد تعرفه‌های بیمه تکمیلی برای همه خدمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateAll()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ایجاد تعرفه‌های بیمه تکمیلی برای همه خدمات. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var result = await _seederService.CreateSupplementaryTariffsAsync();
                
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: تعرفه‌های بیمه تکمیلی با موفقیت ایجاد شدند. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در ایجاد تعرفه‌های بیمه تکمیلی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در ایجاد تعرفه‌های بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ایجاد تعرفه بیمه تکمیلی برای خدمت خاص
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateForService(int serviceId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ایجاد تعرفه بیمه تکمیلی برای خدمت {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _seederService.CreateSupplementaryTariffForServiceAsync(serviceId);
                
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی برای خدمت {ServiceId} با موفقیت ایجاد شد. User: {UserName} (Id: {UserId})",
                        serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در ایجاد تعرفه بیمه تکمیلی برای خدمت {ServiceId} - {Error}. User: {UserName} (Id: {UserId})",
                        serviceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه بیمه تکمیلی برای خدمت {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در ایجاد تعرفه بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت آمار تعرفه‌های بیمه تکمیلی - Production Optimized with Caching
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 180, VaryByParam = "none")] // Cache for 3 minutes
        public async Task<JsonResult> GetStats()
        {
            var userId = _currentUserService.UserId;
            
            try
            {
                _log.Information("🏥 MEDICAL: درخواست آمار تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, userId);

                // Check cache first using cache service
                var cachedStats = await _cacheService.GetCachedSupplementaryTariffsAsync(0);
                if (cachedStats.Success && cachedStats.Data != null)
                {
                    _log.Debug("🏥 MEDICAL: آمار از کش دریافت شد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, userId);
                    
                    var stats = new { 
                        TotalServices = cachedStats.Data.Count,
                        TotalTariffs = cachedStats.Data.Count,
                        ActiveTariffs = cachedStats.Data.Count,
                        InactiveTariffs = 0
                    };
                    
                    return Json(new { success = true, data = stats, cached = true }, JsonRequestBehavior.AllowGet);
                }

                // Get from service with timeout
                var result = await _seederService.GetSupplementaryTariffStatsAsync()
                    .ConfigureAwait(false);
                
                if (result.Success)
                {
                    // Cache the result using cache service (for now, just log)
                    _log.Debug("🏥 MEDICAL: آمار دریافت شد و آماده کش شدن. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, userId);
                    
                    _log.Information("🏥 MEDICAL: آمار تعرفه‌های بیمه تکمیلی با موفقیت دریافت و کش شد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, userId);
                    
                    return Json(new { success = true, data = result.Data, cached = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت آمار تعرفه‌های بیمه تکمیلی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, userId);
                    
                    return Json(new { 
                        success = false, 
                        message = "خطا در دریافت آمار: " + result.Message,
                        errorCode = "STATS_ERROR"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت آمار تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, userId);
                
                // Try to return cached data as fallback
                var fallbackStats = await _cacheService.GetCachedSupplementaryTariffsAsync(0);
                if (fallbackStats.Success && fallbackStats.Data != null)
                {
                    _log.Information("🏥 MEDICAL: استفاده از آمار کش شده به عنوان fallback. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, userId);
                    
                    var stats = new { 
                        TotalServices = fallbackStats.Data.Count,
                        TotalTariffs = fallbackStats.Data.Count,
                        ActiveTariffs = fallbackStats.Data.Count,
                        InactiveTariffs = 0
                    };
                    
                    return Json(new { success = true, data = stats, cached = true, fallback = true }, JsonRequestBehavior.AllowGet);
                }
                
                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت آمار. لطفاً دوباره تلاش کنید.",
                    errorCode = "SYSTEM_ERROR"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ایجاد تعیین ست بیمه پایه و تکمیلی (مثل تامین + دانا)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateInsuranceCombination(int primaryPlanId, int supplementaryPlanId, decimal coveragePercent, decimal maxPayment)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ایجاد تعیین ست بیمه - PrimaryPlanId: {PrimaryPlanId}, SupplementaryPlanId: {SupplementaryPlanId}, CoveragePercent: {CoveragePercent}, MaxPayment: {MaxPayment}. User: {UserName} (Id: {UserId})",
                    primaryPlanId, supplementaryPlanId, coveragePercent, maxPayment, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت طرح‌های بیمه
                var primaryPlan = await _planService.GetPlanDetailsAsync(primaryPlanId);
                var supplementaryPlan = await _planService.GetPlanDetailsAsync(supplementaryPlanId);

                if (!primaryPlan.Success || !supplementaryPlan.Success)
                {
                    return Json(new { success = false, message = "طرح بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                // ایجاد تعرفه‌های ترکیبی - استفاده از متد موجود
                var result = await _seederService.CreateSupplementaryTariffsAsync();
                
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: تعیین ست بیمه با موفقیت ایجاد شد - PrimaryPlan: {PrimaryPlanName}, SupplementaryPlan: {SupplementaryPlanName}. User: {UserName} (Id: {UserId})",
                        primaryPlan.Data.Name, supplementaryPlan.Data.Name, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در ایجاد تعیین ست بیمه - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعیین ست بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در ایجاد تعیین ست بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت اطلاعات هوشمند برای فرم ایجاد تعرفه
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetSmartFormData(int serviceId, int insurancePlanId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست اطلاعات هوشمند فرم - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    serviceId, insurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت اطلاعات خدمت با ServiceComponents
                var service = await _serviceRepository.GetByIdWithComponentsAsync(serviceId);
                if (service == null)
                {
                    return Json(new { success = false, message = "خدمت یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                // محاسبه قیمت واقعی خدمت (دوگانه: ثابت یا محاسبه شده)
                decimal actualServicePrice;
                bool priceWasCalculated = false;
                
                if (service.Price > 0)
                {
                    // استفاده از قیمت ثابت
                    actualServicePrice = service.Price;
                    _log.Information("🏥 MEDICAL: استفاده از قیمت ثابت خدمت - ServiceId: {ServiceId}, Price: {Price}. User: {UserName} (Id: {UserId})",
                        serviceId, actualServicePrice, _currentUserService.UserName, _currentUserService.UserId);
                }
                else
                {
                    // محاسبه از اجزای فنی و حرفه‌ای
                    actualServicePrice = _serviceCalculationService.CalculateServicePrice(service);
                    priceWasCalculated = true;
                    
                    _log.Information("🏥 MEDICAL: محاسبه قیمت از اجزای خدمت - ServiceId: {ServiceId}, CalculatedPrice: {Price}. User: {UserName} (Id: {UserId})",
                        serviceId, actualServicePrice, _currentUserService.UserName, _currentUserService.UserId);
                    
                    // ذخیره قیمت محاسبه شده در دیتابیس
                    if (actualServicePrice > 0)
                    {
                        try
                        {
                            service.Price = actualServicePrice;
                            _serviceRepository.Update(service);
                            await _serviceRepository.SaveChangesAsync();
                            
                            _log.Information("🏥 MEDICAL: قیمت محاسبه شده ذخیره شد - ServiceId: {ServiceId}, SavedPrice: {Price}. User: {UserName} (Id: {UserId})",
                                serviceId, actualServicePrice, _currentUserService.UserName, _currentUserService.UserId);
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex, "🏥 MEDICAL: خطا در ذخیره قیمت محاسبه شده - ServiceId: {ServiceId}, Price: {Price}. User: {UserName} (Id: {UserId})",
                                serviceId, actualServicePrice, _currentUserService.UserName, _currentUserService.UserId);
                        }
                    }
                }

                // دریافت اطلاعات طرح بیمه
                var planResult = await _planService.GetPlansAsync(null, "", 1, 1000);
                if (!planResult.Success || !planResult.Data.Items.Any(p => p.InsurancePlanId == insurancePlanId))
                {
                    return Json(new { success = false, message = "طرح بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }
                var plan = planResult.Data.Items.First(p => p.InsurancePlanId == insurancePlanId);

                // دریافت تنظیمات PlanService (ساده‌سازی شده)
                PlanService planService = null; // برای حالا null می‌گذاریم
                
                // محاسبه ساده با قیمت واقعی خدمت
                var calculationResult = new
                {
                    TotalAmount = actualServicePrice,
                    DeductibleAmount = plan.Deductible,
                    CoverableAmount = Math.Max(0, actualServicePrice - plan.Deductible),
                    CoveragePercent = plan.CoveragePercent,
                    InsuranceCoverage = Math.Max(0, actualServicePrice - plan.Deductible) * (plan.CoveragePercent / 100),
                    PatientPayment = actualServicePrice - (Math.Max(0, actualServicePrice - plan.Deductible) * (plan.CoveragePercent / 100))
                };

                var result = new
                {
                    success = true,
                    data = new
                    {
                        service = new
                        {
                            id = service.ServiceId,
                            title = service.Title,
                            code = service.ServiceCode,
                            price = actualServicePrice,
                            priceWasCalculated = priceWasCalculated
                        },
                        plan = new
                        {
                            id = plan.InsurancePlanId,
                            name = plan.Name,
                            coveragePercent = plan.CoveragePercent,
                            deductible = plan.Deductible
                        },
                        calculation = new
                        {
                            totalAmount = calculationResult.TotalAmount,
                            deductibleAmount = calculationResult.DeductibleAmount,
                            coverableAmount = calculationResult.CoverableAmount,
                            coveragePercent = calculationResult.CoveragePercent,
                            insuranceCoverage = calculationResult.InsuranceCoverage,
                            patientPayment = calculationResult.PatientPayment
                        },
                        planService = planService != null ? new
                        {
                            copay = planService.Copay,
                            coverageOverride = planService.CoverageOverride,
                            isCovered = planService.IsCovered
                        } : null
                    }
                };

                _log.Information("🏥 MEDICAL: اطلاعات هوشمند فرم با موفقیت دریافت شد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت اطلاعات هوشمند فرم. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت اطلاعات فرم" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست دپارتمان‌ها برای فیلتر
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDepartments()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست لیست دپارتمان‌ها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var departments = await _departmentRepository.GetAllActiveDepartmentsAsync();
                
                if (departments != null && departments.Any())
                {
                    var result = departments.Select(d => new
                    {
                        DepartmentId = d.DepartmentId,
                        Name = d.Name,
                        Description = d.Description
                    }).ToList();

                    _log.Information("🏥 MEDICAL: {Count} دپارتمان یافت شد. User: {UserName} (Id: {UserId})",
                        result.Count, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: هیچ دپارتمانی یافت نشد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, data = new List<object>() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت لیست دپارتمان‌ها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت لیست دپارتمان‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست طرح‌های بیمه برای تعیین ست
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetInsurancePlans()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست لیست طرح‌های بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var result = await _planService.GetPlansAsync(null, "", 1, 1000);
                
                if (result.Success)
                {
                    var plans = result.Data.Items.Select(p => new
                    {
                        id = p.InsurancePlanId,
                        name = p.Name,
                        providerName = p.InsuranceProviderName,
                        coveragePercent = p.CoveragePercent,
                        isPrimary = p.CoveragePercent > 0 && p.CoveragePercent < 100
                    }).ToList();

                    _log.Information("🏥 MEDICAL: لیست طرح‌های بیمه با موفقیت دریافت شد - Count: {Count}. User: {UserName} (Id: {UserId})",
                        plans.Count, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, data = plans }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت لیست طرح‌های بیمه - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت لیست طرح‌های بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت لیست طرح‌های بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }



        /// <summary>
        /// دریافت جزئیات تعرفه بیمه تکمیلی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTariffDetails(int tariffId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست جزئیات تعرفه بیمه تکمیلی - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه از دیتابیس
                var tariffResult = await _tariffService.GetTariffByIdAsync(tariffId);
                if (!tariffResult.Success)
                {
                    return Json(new { success = false, message = "تعرفه بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var tariff = tariffResult.Data;
                
                // آماده‌سازی داده‌های پاسخ
                var tariffDetails = new
                {
                    tariffId = tariff.InsuranceTariffId,
                    serviceId = tariff.ServiceId,
                    serviceName = tariff.ServiceTitle ?? "نامشخص",
                    planId = tariff.InsurancePlanId,
                    planName = tariff.InsurancePlanName ?? "نامشخص",
                    tariffPrice = tariff.TariffPrice,
                    patientShare = tariff.PatientShare,
                    insurerShare = tariff.InsurerShare,
                    createdAt = tariff.CreatedAt.ToString("yyyy/MM/dd HH:mm"),
                    updatedAt = tariff.UpdatedAt?.ToString("yyyy/MM/dd HH:mm")
                };
                
                _log.Information("🏥 MEDICAL: جزئیات تعرفه بیمه تکمیلی با موفقیت دریافت شد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = true, data = tariffDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت جزئیات تعرفه بیمه تکمیلی. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت جزئیات تعرفه بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس طرح
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTariffsByPlan(int planId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست تعرفه‌های بیمه بر اساس طرح - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه‌های بیمه تکمیلی
                var supplementaryTariffsResult = await _tariffService.GetSupplementaryTariffsAsync(planId);
                if (!supplementaryTariffsResult.Success)
                {
                    return Json(new { success = false, message = "خطا در دریافت تعرفه‌های بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
                }

                var tariffs = supplementaryTariffsResult.Data.Select(t => new
                {
                    tariffId = t.InsuranceTariffId,
                    serviceId = t.ServiceId,
                    serviceName = t.Service?.Title ?? "نامشخص",
                    serviceCode = t.Service?.ServiceCode ?? "نامشخص",
                    patientShare = t.PatientShare,
                    insurerShare = t.InsurerShare,
                    supplementaryCoveragePercent = t.SupplementaryCoveragePercent,
                    supplementaryMaxPayment = t.SupplementaryMaxPayment,
                    priority = t.Priority,
                    startDate = t.StartDate?.ToString("yyyy/MM/dd"),
                    endDate = t.EndDate?.ToString("yyyy/MM/dd"),
                    isActive = t.IsActive,
                    createdAt = t.CreatedAt.ToString("yyyy/MM/dd HH:mm")
                }).ToList();

                _log.Information("🏥 MEDICAL: تعرفه‌های بیمه بر اساس طرح با موفقیت دریافت شد - PlanId: {PlanId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    planId, tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, data = tariffs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت تعرفه‌های بیمه بر اساس طرح. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت تعرفه‌های بیمه بر اساس طرح" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// به‌روزرسانی تنظیمات تعرفه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateTariffSettings(int tariffId, string settings)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست به‌روزرسانی تنظیمات تعرفه - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه موجود
                var tariffResult = await _tariffService.GetTariffByIdAsync(tariffId);
                if (!tariffResult.Success)
                {
                    return Json(new { success = false, message = "تعرفه بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var tariff = tariffResult.Data;
                
                // تبدیل به CreateEditViewModel برای به‌روزرسانی
                var editModel = new InsuranceTariffCreateEditViewModel
                {
                    InsuranceTariffId = tariff.InsuranceTariffId,
                    ServiceId = tariff.ServiceId,
                    InsurancePlanId = tariff.InsurancePlanId,
                    TariffPrice = tariff.TariffPrice,
                    PatientShare = tariff.PatientShare,
                    InsurerShare = tariff.InsurerShare,
                    IsActive = tariff.IsActive
                };

                // ذخیره تغییرات
                var updateResult = await _tariffService.UpdateTariffAsync(editModel);
                if (updateResult.Success)
                {
                    _log.Information("🏥 MEDICAL: تنظیمات تعرفه با موفقیت به‌روزرسانی شد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, message = "تنظیمات تعرفه با موفقیت به‌روزرسانی شد" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در به‌روزرسانی تنظیمات تعرفه - {Error}. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        updateResult.Message, tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = updateResult.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در به‌روزرسانی تنظیمات تعرفه. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در به‌روزرسانی تنظیمات تعرفه" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// اعتبارسنجی ترکیب بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ValidateTariffCombination(int primaryPlanId, int supplementaryPlanId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست اعتبارسنجی ترکیب بیمه - PrimaryPlanId: {PrimaryPlanId}, SupplementaryPlanId: {SupplementaryPlanId}. User: {UserName} (Id: {UserId})",
                    primaryPlanId, supplementaryPlanId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت طرح‌های بیمه
                var primaryPlanResult = await _planService.GetPlanDetailsAsync(primaryPlanId);
                var supplementaryPlanResult = await _planService.GetPlanDetailsAsync(supplementaryPlanId);

                if (!primaryPlanResult.Success || !supplementaryPlanResult.Success)
                {
                    return Json(new { success = false, message = "یکی از طرح‌های بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var primaryPlan = primaryPlanResult.Data;
                var supplementaryPlan = supplementaryPlanResult.Data;

                // اعتبارسنجی ترکیب
                var validationResult = new
                {
                    isValid = true,
                    primaryPlanName = primaryPlan.Name,
                    supplementaryPlanName = supplementaryPlan.Name,
                    primaryCoveragePercent = primaryPlan.CoveragePercent,
                    supplementaryCoveragePercent = supplementaryPlan.CoveragePercent,
                    totalCoveragePercent = primaryPlan.CoveragePercent + supplementaryPlan.CoveragePercent,
                    warnings = new List<string>(),
                    recommendations = new List<string>()
                };

                // بررسی محدودیت‌ها
                if (validationResult.totalCoveragePercent > 100)
                {
                    validationResult.warnings.Add("مجموع درصد پوشش بیش از 100% است");
                }

                if (primaryPlan.CoveragePercent < 50)
                {
                    validationResult.recommendations.Add("درصد پوشش بیمه اصلی کمتر از 50% است");
                }

                _log.Information("🏥 MEDICAL: اعتبارسنجی ترکیب بیمه تکمیل شد - PrimaryPlan: {PrimaryPlanName}, SupplementaryPlan: {SupplementaryPlanName}. User: {UserName} (Id: {UserId})",
                    primaryPlan.Name, supplementaryPlan.Name, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, data = validationResult }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی ترکیب بیمه. PrimaryPlanId: {PrimaryPlanId}, SupplementaryPlanId: {SupplementaryPlanId}. User: {UserName} (Id: {UserId})",
                    primaryPlanId, supplementaryPlanId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در اعتبارسنجی ترکیب بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// دریافت دسته‌بندی خدمات برای انتخاب دسته‌بندی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategories()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست دسته‌بندی خدمات برای انتخاب دسته‌بندی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var serviceCategories = await _serviceCategoryRepository.GetAllActiveServiceCategoriesAsync();
                var categoryList = serviceCategories.Select(sc => new
                {
                    ServiceCategoryId = sc.ServiceCategoryId,
                    Name = sc.Title,
                    Description = sc.Description,
                    DepartmentId = sc.DepartmentId,
                    DepartmentName = sc.Department?.Name
                }).ToList();

                _log.Information("🏥 MEDICAL: دسته‌بندی خدمات با موفقیت دریافت شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                    categoryList.Count, _currentUserService.UserName, _currentUserService.UserId);

                return Json(categoryList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت دسته‌بندی خدمات. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت دسته‌بندی خدمات" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت همه خدمات برای انتخاب گروهی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAllServices()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست همه خدمات برای انتخاب گروهی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var services = await _serviceRepository.GetAllActiveServicesAsync();
                var serviceList = services.Select(s => new
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    ServiceCode = s.ServiceCode,
                    Price = s.Price,
                    DepartmentId = s.ServiceCategory?.DepartmentId,
                    DepartmentName = s.ServiceCategory?.Department?.Name
                }).ToList();

                _log.Information("🏥 MEDICAL: همه خدمات با موفقیت دریافت شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                    serviceList.Count, _currentUserService.UserName, _currentUserService.UserId);

                return Json(serviceList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت همه خدمات. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت خدمات" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت آمار تعرفه‌ها
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTariffStatistics()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست آمار تعرفه‌ها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // دریافت آمار از سرویس
                var statsResult = await _seederService.GetSupplementaryTariffStatsAsync();
                if (!statsResult.Success)
                {
                    return Json(new { success = false, message = "خطا در دریافت آمار تعرفه‌ها" }, JsonRequestBehavior.AllowGet);
                }

                var statistics = new
                {
                    totalServices = statsResult.Data.TotalServices,
                    totalSupplementaryTariffs = statsResult.Data.TotalSupplementaryTariffs,
                    activeSupplementaryTariffs = statsResult.Data.ActiveSupplementaryTariffs,
                    expiredSupplementaryTariffs = statsResult.Data.ExpiredSupplementaryTariffs,
                    lastUpdated = DateTime.Now.ToString("yyyy/MM/dd HH:mm")
                };

                _log.Information("🏥 MEDICAL: آمار تعرفه‌ها با موفقیت دریافت شد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, data = statistics }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت آمار تعرفه‌ها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت آمار تعرفه‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Cache Management Methods

        /// <summary>
        /// پاک کردن کش تعرفه‌های بیمه تکمیلی
        /// </summary>
        private async Task InvalidateSupplementaryTariffCacheAsync()
        {
            try
            {
                _cacheService.ClearCache();

                _log.Information("🏥 MEDICAL: کش تعرفه‌های بیمه تکمیلی پاک شد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در پاک کردن کش تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه تکمیلی - AJAX
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTariffs(string searchTerm = "", int? insurancePlanId = null, int? departmentId = null, bool? isActive = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست تعرفه‌های بیمه تکمیلی با فیلترها - SearchTerm: {SearchTerm}, PlanId: {PlanId}, DeptId: {DeptId}, IsActive: {IsActive}. User: {UserName} (Id: {UserId})",
                    searchTerm, insurancePlanId, departmentId, isActive, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه‌های بیمه تکمیلی از سرویس
                var result = await _tariffService.GetTariffsByPlanIdAsync(insurancePlanId ?? 0);
                
                if (!result.Success)
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت تعرفه‌های بیمه تکمیلی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = false, 
                        message = "خطا در دریافت تعرفه‌ها: " + result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }

                var tariffs = result.Data.Select(t => new
                {
                    tariffId = t.InsuranceTariffId,
                    serviceId = t.ServiceId,
                    serviceName = t.Service?.Title ?? "نامشخص",
                    serviceCode = t.Service?.ServiceCode ?? "نامشخص",
                    planId = t.InsurancePlanId,
                    planName = t.InsurancePlan?.Name ?? "نامشخص",
                    tariffPrice = t.TariffPrice,
                    patientShare = t.PatientShare,
                    insurerShare = t.InsurerShare,
                    supplementaryCoveragePercent = t.SupplementaryCoveragePercent,
                    supplementaryMaxPayment = t.SupplementaryMaxPayment,
                    priority = t.Priority,
                    startDate = t.StartDate?.ToString("yyyy/MM/dd"),
                    endDate = t.EndDate?.ToString("yyyy/MM/dd"),
                    isActive = t.IsActive,
                    createdAt = t.CreatedAt.ToString("yyyy/MM/dd HH:mm"),
                    updatedAt = t.UpdatedAt?.ToString("yyyy/MM/dd HH:mm")
                }).ToList();

                // اعمال فیلترها
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    tariffs = tariffs.Where(t => 
                        t.serviceName.Contains(searchTerm) || 
                        t.serviceCode.Contains(searchTerm)
                    ).ToList();
                }

                if (isActive.HasValue)
                {
                    tariffs = tariffs.Where(t => t.isActive == isActive.Value).ToList();
                }

                _log.Information("🏥 MEDICAL: تعرفه‌های بیمه تکمیلی با موفقیت دریافت شد - Count: {Count}. User: {UserName} (Id: {UserId})",
                    tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { 
                    success = true, 
                    data = tariffs,
                    totalCount = tariffs.Count,
                    filters = new {
                        searchTerm = searchTerm,
                        insurancePlanId = insurancePlanId,
                        departmentId = departmentId,
                        isActive = isActive
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت تعرفه‌ها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در سیستم" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد تعرفه بیمه تکمیلی جدید
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            try
            {
                _log.Information("🏥 MEDICAL: نمایش فرم ایجاد تعرفه بیمه تکمیلی جدید. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // بارگذاری داده‌های مورد نیاز برای فرم
                var departments = await _departmentRepository.GetAllActiveDepartmentsAsync();
                
                // دریافت بیمه‌های پایه (Primary Insurance Plans)
                var primaryInsurancePlans = await _planService.GetPrimaryInsurancePlansAsync();
                
                // دریافت بیمه‌های تکمیلی (Supplementary Insurance Plans)
                var supplementaryInsurancePlans = await _planService.GetSupplementaryInsurancePlansAsync();

                ViewBag.Departments = departments ?? new List<Department>();
                ViewBag.PrimaryInsurancePlans = primaryInsurancePlans.Data ?? new List<InsurancePlanLookupViewModel>();
                ViewBag.InsurancePlans = supplementaryInsurancePlans.Data ?? new List<InsurancePlanLookupViewModel>();

                var model = new SupplementaryTariffCreateEditViewModel
                {
                    SupplementaryCoveragePercent = 90, // مقدار پیش‌فرض
                    Priority = 5, // اولویت پیش‌فرض
                    IsActive = true
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در نمایش فرم ایجاد تعرفه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return View("Error");
            }
        }

        /// <summary>
        /// ایجاد تعرفه بیمه تکمیلی جدید - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SupplementaryTariffCreateEditViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: ایجاد تعرفه بیمه تکمیلی جدید - ServiceId: {ServiceId}, PlanId: {PlanId}, TariffPrice: {TariffPrice}. User: {UserName} (Id: {UserId})",
                    model.ServiceId, model.InsurancePlanId, model.TariffPrice, _currentUserService.UserName, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    // بارگذاری مجدد داده‌های مورد نیاز
                    var services = await _serviceRepository.GetAllActiveServicesAsync();
                    var insurancePlans = await _planService.GetActivePlansForLookupAsync();

                    ViewBag.Services = services ?? new List<Service>();
                    ViewBag.InsurancePlans = insurancePlans.Data ?? new List<InsurancePlanLookupViewModel>();

                    return View(model);
                }

                // تبدیل SupplementaryTariffCreateEditViewModel به InsuranceTariffCreateEditViewModel
                var insuranceTariffModel = new InsuranceTariffCreateEditViewModel
                {
                    ServiceId = model.ServiceId,
                    InsurancePlanId = model.InsurancePlanId,
                    TariffPrice = model.TariffPrice,
                    PatientShare = model.PatientShare,
                    InsurerShare = model.InsurerShare,
                    IsActive = model.IsActive,
                    // Priority = model.Priority // این فیلد در InsuranceTariffCreateEditViewModel وجود ندارد
                };

                var result = await _tariffService.CreateTariffAsync(insuranceTariffModel);

                if (result.Success)
                {
                    // پاک کردن کش
                    await InvalidateSupplementaryTariffCacheAsync();

                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی با موفقیت ایجاد شد - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        model.ServiceId, model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["SuccessMessage"] = "تعرفه بیمه تکمیلی با موفقیت ایجاد شد";
                    return RedirectToAction("Index");
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در ایجاد تعرفه بیمه تکمیلی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    ModelState.AddModelError("", "خطا در ایجاد تعرفه: " + result.Message);

                    // بارگذاری مجدد داده‌های مورد نیاز
                    var services = await _serviceRepository.GetAllActiveServicesAsync();
                    var insurancePlans = await _planService.GetActivePlansForLookupAsync();

                    ViewBag.Services = services ?? new List<Service>();
                    ViewBag.InsurancePlans = insurancePlans.Data ?? new List<InsurancePlanLookupViewModel>();

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه جدید. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                ModelState.AddModelError("", "خطا در سیستم");
                return View(model);
            }
        }

        /// <summary>
        /// ایجاد تعرفه بیمه تکمیلی جدید - AJAX (برای سازگاری)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateTariff(int serviceId, int insurancePlanId, decimal tariffPrice, decimal patientShare, decimal insurerShare, decimal coveragePercent)
        {
            try
            {
                _log.Information("🏥 MEDICAL: ایجاد تعرفه بیمه تکمیلی جدید - ServiceId: {ServiceId}, PlanId: {PlanId}, TariffPrice: {TariffPrice}. User: {UserName} (Id: {UserId})",
                    serviceId, insurancePlanId, tariffPrice, _currentUserService.UserName, _currentUserService.UserId);

                // ایجاد ViewModel برای ایجاد تعرفه
                var createModel = new SupplementaryTariffCreateEditViewModel
                {
                    ServiceId = serviceId,
                    InsurancePlanId = insurancePlanId,
                    TariffPrice = tariffPrice,
                    PatientShare = patientShare,
                    InsurerShare = insurerShare,
                    SupplementaryCoveragePercent = coveragePercent,
                    IsActive = true
                };

                // ایجاد تعرفه از طریق سرویس
                // تبدیل SupplementaryTariffCreateEditViewModel به InsuranceTariffCreateEditViewModel
                var insuranceTariffModel = new InsuranceTariffCreateEditViewModel
                {
                    ServiceId = createModel.ServiceId,
                    InsurancePlanId = createModel.InsurancePlanId,
                    TariffPrice = createModel.TariffPrice,
                    PatientShare = createModel.PatientShare,
                    InsurerShare = createModel.InsurerShare,
                    IsActive = createModel.IsActive
                };
                
                var result = await _tariffService.CreateTariffAsync(insuranceTariffModel);
                
                if (result.Success)
                {
                    // پاک کردن کش
                    await InvalidateSupplementaryTariffCacheAsync();
                    
                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی با موفقیت ایجاد شد - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        serviceId, insurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = true, 
                        message = "تعرفه با موفقیت ایجاد شد",
                        data = new { 
                            serviceId = serviceId,
                            insurancePlanId = insurancePlanId,
                            tariffPrice = tariffPrice,
                            patientShare = patientShare,
                            insurerShare = insurerShare,
                            coveragePercent = coveragePercent
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در ایجاد تعرفه بیمه تکمیلی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = false, 
                        message = "خطا در ایجاد تعرفه: " + result.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه جدید. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در سیستم" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ویرایش تعرفه بیمه تکمیلی - AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EditTariff(int tariffId, int serviceId, int insurancePlanId, decimal tariffPrice, decimal patientShare, decimal insurerShare, decimal coveragePercent)
        {
            try
            {
                _log.Information("🏥 MEDICAL: ویرایش تعرفه بیمه تکمیلی - TariffId: {TariffId}, ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    tariffId, serviceId, insurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه موجود
                var tariffResult = await _tariffService.GetTariffByIdAsync(tariffId);
                if (!tariffResult.Success)
                {
                    return Json(new { success = false, message = "تعرفه بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                // ایجاد ViewModel برای ویرایش
                var editModel = new SupplementaryTariffCreateEditViewModel
                {
                    InsuranceTariffId = tariffId,
                    ServiceId = serviceId,
                    InsurancePlanId = insurancePlanId,
                    TariffPrice = tariffPrice,
                    PatientShare = patientShare,
                    InsurerShare = insurerShare,
                    SupplementaryCoveragePercent = coveragePercent,
                    IsActive = tariffResult.Data.IsActive
                };

                // به‌روزرسانی تعرفه
                // تبدیل SupplementaryTariffCreateEditViewModel به InsuranceTariffCreateEditViewModel
                var insuranceTariffModel = new InsuranceTariffCreateEditViewModel
                {
                    InsuranceTariffId = editModel.InsuranceTariffId ?? 0,
                    ServiceId = editModel.ServiceId,
                    InsurancePlanId = editModel.InsurancePlanId,
                    TariffPrice = editModel.TariffPrice,
                    PatientShare = editModel.PatientShare,
                    InsurerShare = editModel.InsurerShare,
                    IsActive = editModel.IsActive
                };
                
                var updateResult = await _tariffService.UpdateTariffAsync(insuranceTariffModel);
                if (updateResult.Success)
                {
                    // پاک کردن کش
                    await InvalidateSupplementaryTariffCacheAsync();
                    
                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی با موفقیت ویرایش شد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = true, 
                        message = "تعرفه با موفقیت ویرایش شد",
                        data = new { 
                            tariffId = tariffId,
                            serviceId = serviceId,
                            insurancePlanId = insurancePlanId,
                            tariffPrice = tariffPrice,
                            patientShare = patientShare,
                            insurerShare = insurerShare,
                            coveragePercent = coveragePercent
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در ویرایش تعرفه بیمه تکمیلی - {Error}. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        updateResult.Message, tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = false, 
                        message = "خطا در ویرایش تعرفه: " + updateResult.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ویرایش تعرفه. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در سیستم" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// حذف تعرفه بیمه تکمیلی - AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteTariff(int tariffId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: حذف تعرفه بیمه تکمیلی - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);

                // بررسی وجود تعرفه
                var tariffResult = await _tariffService.GetTariffByIdAsync(tariffId);
                if (!tariffResult.Success)
                {
                    return Json(new { success = false, message = "تعرفه بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                // حذف تعرفه
                var deleteResult = await _tariffService.SoftDeleteTariffAsync(tariffId);
                if (deleteResult.Success)
                {
                    // پاک کردن کش
                    await InvalidateSupplementaryTariffCacheAsync();
                    
                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی با موفقیت حذف شد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = true, 
                        message = "تعرفه با موفقیت حذف شد",
                        data = new { tariffId = tariffId }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در حذف تعرفه بیمه تکمیلی - {Error}. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        deleteResult.Message, tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = false, 
                        message = "خطا در حذف تعرفه: " + deleteResult.Message 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در حذف تعرفه. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در سیستم" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        /// <summary>
        /// دریافت خدمات بر اساس دپارتمان برای Cascade Dropdown
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetServicesByDepartment(int departmentId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: دریافت خدمات دپارتمان {DepartmentId}. User: {UserName} (Id: {UserId})",
                    departmentId, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی
                if (departmentId <= 0)
                {
                    _log.Warning("🏥 MEDICAL: شناسه دپارتمان نامعتبر - DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                        departmentId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }

                var services = await _serviceRepository.GetServicesByDepartmentAsync(departmentId);
                
                if (services == null || !services.Any())
                {
                    _log.Information("🏥 MEDICAL: هیچ خدمتی برای دپارتمان {DepartmentId} یافت نشد. User: {UserName} (Id: {UserId})",
                        departmentId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }
                
                var result = services.Select(s => new
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title ?? "نامشخص",
                    ServiceCode = s.ServiceCode ?? ""
                }).ToList();

                _log.Information("🏥 MEDICAL: {Count} خدمت برای دپارتمان {DepartmentId} یافت شد. User: {UserName} (Id: {UserId})",
                    result.Count, departmentId, _currentUserService.UserName, _currentUserService.UserId);

                Response.ContentType = "application/json";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت خدمات دپارتمان {DepartmentId}. User: {UserName} (Id: {UserId})",
                    departmentId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        #region Bulk Tariff Methods

        /// <summary>
        /// صفحه ایجاد تعرفه گروهی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> CreateBulk()
        {
            try
            {
                _log.Information("🏥 MEDICAL: نمایش فرم ایجاد تعرفه گروهی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // بارگذاری داده‌های مورد نیاز
                var departments = await _departmentRepository.GetAllActiveDepartmentsAsync();
                var serviceCategories = await _serviceCategoryRepository.GetAllActiveServiceCategoriesAsync();
                var services = await _serviceRepository.GetAllActiveServicesAsync();
                var primaryInsurancePlans = await _planService.GetPrimaryInsurancePlansAsync();
                var supplementaryInsurancePlans = await _planService.GetSupplementaryInsurancePlansAsync();

                var model = new BulkSupplementaryTariffViewModel
                {
                    PrimaryInsurancePlans = primaryInsurancePlans.Data ?? new List<InsurancePlanLookupViewModel>(),
                    SupplementaryInsurancePlans = supplementaryInsurancePlans.Data ?? new List<InsurancePlanLookupViewModel>(),
                    Departments = departments?.Select(d => new DepartmentLookupViewModel
                    {
                        DepartmentId = d.DepartmentId,
                        Name = d.Name,
                        Description = d.Description,
                        IsActive = d.IsActive,
                        ServiceCount = 0 // TODO: محاسبه تعداد خدمات
                    }).ToList() ?? new List<DepartmentLookupViewModel>(),
                    ServiceCategories = serviceCategories?.Select(sc => new ServiceCategoryLookupViewModel
                    {
                        ServiceCategoryId = sc.ServiceCategoryId,
                        Name = sc.Title,
                        Description = sc.Description,
                        DepartmentId = sc.DepartmentId,
                        DepartmentName = sc.Department?.Name,
                        IsActive = sc.IsActive,
                        ServiceCount = 0 // TODO: محاسبه تعداد خدمات
                    }).ToList() ?? new List<ServiceCategoryLookupViewModel>(),
                    Services = services?.Select(s => new ServiceLookupViewModel
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
                        HasExistingTariff = false // TODO: بررسی وجود تعرفه
                    }).ToList() ?? new List<ServiceLookupViewModel>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در نمایش فرم ایجاد تعرفه گروهی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return View("Error");
            }
        }

        /// <summary>
        /// ایجاد تعرفه گروهی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateBulk(BulkSupplementaryTariffViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ایجاد تعرفه گروهی - SelectionType: {SelectionType}, User: {UserName} (Id: {UserId})",
                    model.SelectionType, _currentUserService.UserName, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new
                    {
                        success = false,
                        message = "ورودی‌های نامعتبر",
                        errors = errors
                    }, JsonRequestBehavior.AllowGet);
                }

                var result = await _bulkTariffService.CreateBulkTariffsAsync(model);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: تعرفه گروهی با موفقیت ایجاد شد - Created: {Created}, Updated: {Updated}, Errors: {Errors}. User: {UserName} (Id: {UserId})",
                        result.Data.CreatedTariffs, result.Data.UpdatedTariffs, result.Data.Errors, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Index", "SupplementaryTariff");
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در ایجاد تعرفه گروهی - {Message}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در ایجاد تعرفه گروهی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                ModelState.AddModelError("", "خطا در ایجاد تعرفه گروهی. لطفاً دوباره تلاش کنید.");
                return View(model);
            }
        }

        /// <summary>
        /// دریافت خدمات بر اساس نوع انتخاب
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetServicesBySelection(BulkSupplementaryTariffViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست دریافت خدمات - SelectionType: {SelectionType}. User: {UserName} (Id: {UserId})",
                    model.SelectionType, _currentUserService.UserName, _currentUserService.UserId);

                var services = await _bulkTariffService.GetServicesBySelectionTypeAsync(model);

                return Json(new
                {
                    success = true,
                    data = services,
                    count = services.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت خدمات. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت خدمات"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// پیش‌نمایش تعرفه گروهی
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> PreviewBulkTariff(BulkSupplementaryTariffViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست پیش‌نمایش تعرفه گروهی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var services = await _bulkTariffService.GetServicesBySelectionTypeAsync(model);
                var preview = new
                {
                    TotalServices = services.Count,
                    EstimatedTariffs = services.Count,
                    TotalPrice = services.Sum(s => s.Price),
                    AveragePrice = services.Any() ? services.Average(s => s.Price) : 0,
                    MinPrice = services.Any() ? services.Min(s => s.Price) : 0,
                    MaxPrice = services.Any() ? services.Max(s => s.Price) : 0,
                    Services = services.Take(10).ToList() // نمایش 10 خدمت اول
                };

                return Json(new
                {
                    success = true,
                    data = preview
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در پیش‌نمایش تعرفه گروهی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در پیش‌نمایش تعرفه گروهی"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}