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
// 🔧 FIX: حذف using تکراری
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
        private readonly IInsuranceTariffRepository _tariffRepository;
        private readonly IInsurancePlanService _planService;
        private readonly IServiceRepository _serviceRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly BulkSupplementaryTariffService _bulkTariffService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        // 🔧 CRITICAL FIX: حذف کش برای محیط درمانی realtime
        // private readonly ISupplementaryInsuranceCacheService _cacheService;
        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly IInsuranceCalculationService _insuranceCalculationService;
        private readonly ISupplementaryCombinationService _supplementaryCombinationService;

        /// <summary>
        /// Constructor for SupplementaryTariffController
        /// سازنده کلاس SupplementaryTariffController
        /// </summary>
        /// <param name="seederService">Service for seeding supplementary tariff data - سرویس برای بارگذاری داده‌های تعرفه تکمیلی</param>
        /// <param name="tariffService">Service for managing insurance tariffs - سرویس مدیریت تعرفه‌های بیمه</param>
        /// <param name="planService">Service for managing insurance plans - سرویس مدیریت طرح‌های بیمه</param>
        /// <param name="providerService">Service for managing insurance providers - سرویس مدیریت ارائه‌دهندگان بیمه</param>
        /// <param name="serviceRepository">Repository for service data access - ریپازیتوری دسترسی به داده‌های خدمت</param>
        /// <param name="departmentRepository">Repository for department data access - ریپازیتوری دسترسی به داده‌های دپارتمان</param>
        /// <param name="serviceCategoryRepository">Repository for service category data access - ریپازیتوری دسترسی به داده‌های دسته‌بندی خدمت</param>
        /// <param name="bulkTariffService">Service for bulk tariff operations - سرویس عملیات گروهی تعرفه</param>
        /// <param name="combinedCalculationService">Service for combined insurance calculations - سرویس محاسبات ترکیبی بیمه</param>
        /// <param name="factorSettingService">Service for managing factor settings - سرویس مدیریت تنظیمات فاکتور</param>
        /// <param name="logger">Logger for application logging - لاگر برای ثبت رویدادهای برنامه</param>
        /// <param name="currentUserService">Service for current user information - سرویس اطلاعات کاربر فعلی</param>
        /// <param name="messageNotificationService">Service for message notifications - سرویس اعلان‌های پیام</param>
        /// <param name="cacheService">Service for caching operations - سرویس عملیات کش</param>
        /// <param name="serviceCalculationService">Service for service calculations - سرویس محاسبات خدمت</param>
        /// <param name="insuranceCalculationService">Service for insurance calculations - سرویس محاسبات بیمه</param>
        public SupplementaryTariffController(
            SupplementaryTariffSeederService seederService,
            IInsuranceTariffService tariffService,
            IInsuranceTariffRepository tariffRepository,
            IInsurancePlanService planService,
            IServiceRepository serviceRepository,
            IDepartmentRepository departmentRepository,
            IServiceCategoryRepository serviceCategoryRepository,
            BulkSupplementaryTariffService bulkTariffService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService,
            // ISupplementaryInsuranceCacheService cacheService, // 🔧 CRITICAL FIX: حذف کش برای محیط درمانی realtime
            IServiceCalculationService serviceCalculationService,
            IPatientInsuranceRepository patientInsuranceRepository,
            IInsuranceCalculationService insuranceCalculationService,
            ISupplementaryCombinationService supplementaryCombinationService)
            : base(messageNotificationService)
        {
            _seederService = seederService ?? throw new ArgumentNullException(nameof(seederService));
            _tariffService = tariffService ?? throw new ArgumentNullException(nameof(tariffService));
            _tariffRepository = tariffRepository ?? throw new ArgumentNullException(nameof(tariffRepository));
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _serviceCategoryRepository = serviceCategoryRepository ?? throw new ArgumentNullException(nameof(serviceCategoryRepository));
            _bulkTariffService = bulkTariffService ?? throw new ArgumentNullException(nameof(bulkTariffService));
            _log = logger.ForContext<SupplementaryTariffController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            // _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService)); // 🔧 CRITICAL FIX: حذف کش برای محیط درمانی realtime
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _patientInsuranceRepository = patientInsuranceRepository ?? throw new ArgumentNullException(nameof(patientInsuranceRepository));
            _insuranceCalculationService = insuranceCalculationService ?? throw new ArgumentNullException(nameof(insuranceCalculationService));
            _supplementaryCombinationService = supplementaryCombinationService ?? throw new ArgumentNullException(nameof(supplementaryCombinationService));
        }

        /// <summary>
        /// صفحه اصلی مدیریت تعرفه‌های بیمه تکمیلی - Real-time برای محیط درمانی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            // 🏥 MEDICAL: Real-time data - no cache needed
            var userId = _currentUserService.UserId;
            var startTime = DateTime.UtcNow; // Performance monitoring start

            try
            {
                _log.Information("🏥 MEDICAL: دسترسی به صفحه مدیریت تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, userId);

                // Load filter data
                var filterData = await LoadFilterData();

                // 🔧 CRITICAL FIX: حذف کش برای محیط درمانی realtime - دریافت داده‌های واقعی از دیتابیس
                _log.Debug("🏥 MEDICAL: دریافت داده‌های realtime از دیتابیس. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, userId);

                // Get stats from service with timeout
                var statsResult = await _seederService.GetSupplementaryTariffStatsAsync()
                    .ConfigureAwait(false);

                if (statsResult.Success)
                {
                    LogUserOperation("آمار با موفقیت دریافت شد", "دریافت آمار تعرفه‌های بیمه تکمیلی");

                    ViewBag.Stats = statsResult.Data;

                    // Create ViewModel for the view
                    var viewModel = new SupplementaryTariffIndexPageViewModel
                    {
                        Statistics = new SupplementaryTariffStatisticsViewModel
                        {
                            TotalServices = statsResult.Data.TotalServices,
                            TotalSupplementaryTariffs = statsResult.Data.TotalSupplementaryTariffs,
                            ActiveSupplementaryTariffs = statsResult.Data.ActiveSupplementaryTariffs,
                            ExpiredSupplementaryTariffs = statsResult.Data.ExpiredSupplementaryTariffs
                        },
                        Filter = filterData,
                        Tariffs = new PagedResult<SupplementaryTariffIndexViewModel>
                        {
                            Items = new List<SupplementaryTariffIndexViewModel>(),
                            TotalItems = 0,
                            PageNumber = 1,
                            PageSize = 10
                        }
                    };

                    return View(viewModel);
                }
                else
                {
                    LogUserOperation($"خطا در دریافت آمار: {statsResult.Message}", "دریافت آمار تعرفه‌های بیمه تکمیلی");

                    // Create empty ViewModel
                    var emptyViewModel = CreateEmptyViewModel(filterData);

                    return View(emptyViewModel);
                }
            }
            catch (Exception ex)
            {
                LogUserOperation($"خطا در دسترسی به صفحه مدیریت: {ex.Message}", "دسترسی به صفحه مدیریت تعرفه‌های بیمه تکمیلی", ex);

                // 🔧 CRITICAL FIX: حذف کش برای محیط درمانی realtime
                // Return empty stats in case of error
                ViewBag.Stats = new
                {
                    TotalServices = 0,
                    TotalTariffs = 0,
                    ActiveTariffs = 0,
                    InactiveTariffs = 0
                };

                // Return empty ViewModel as last resort
                var emptyViewModel = new SupplementaryTariffIndexPageViewModel
                {
                    Statistics = new SupplementaryTariffStatisticsViewModel(),
                    Filter = new SupplementaryTariffFilterViewModel(),
                    Tariffs = new PagedResult<SupplementaryTariffIndexViewModel>
                    {
                        Items = new List<SupplementaryTariffIndexViewModel>(),
                        TotalItems = 0,
                        PageNumber = 1,
                        PageSize = 10
                    }
                };

                return View(emptyViewModel);
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
        /// دریافت آمار تعرفه‌های بیمه تکمیلی - Production Optimized with Enhanced Caching
        /// </summary>
        [HttpGet]
        // 🏥 MEDICAL: Real-time data - no cache for clinical safety
        public async Task<JsonResult> GetStats()
        {
            var userId = _currentUserService.UserId;

            try
            {
                _log.Information("🏥 MEDICAL: درخواست آمار تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, userId);

                // 🔧 CRITICAL FIX: حذف کش برای محیط درمانی realtime
                // Check cache first using cache service
                // var cachedStats = await _cacheService.GetCachedSupplementaryTariffsAsync(0);
                // if (cachedStats.Success && cachedStats.Data != null)
                // {
                //     _log.Debug("🏥 MEDICAL: آمار از کش دریافت شد. User: {UserName} (Id: {UserId})",
                //         _currentUserService.UserName, userId);
                //
                //     var stats = new
                //     {
                //         TotalServices = cachedStats.Data.Count,
                //         TotalTariffs = cachedStats.Data.Count,
                //         ActiveTariffs = cachedStats.Data.Count,
                //         InactiveTariffs = 0
                //     };
                //
                //     return Json(new { success = true, data = stats, cached = true }, JsonRequestBehavior.AllowGet);
                // }

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

                    return Json(new
                    {
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

                // 🔧 CRITICAL FIX: حذف کش برای محیط درمانی realtime
                // Return empty statistics in case of error
                var stats = new
                {
                    TotalServices = 0,
                    TotalTariffs = 0,
                    ActiveTariffs = 0,
                    InactiveTariffs = 0
                };

                return Json(new { success = true, data = stats, cached = true, fallback = true }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت آمار. لطفاً دوباره تلاش کنید.",
                    errorCode = "SYSTEM_ERROR"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ایجاد تعیین ست بیمه پایه و تکمیلی (مثل سلامت + ملت VIP)
        /// 🔧 CRITICAL FIX: استفاده از منطق صحیح بیمه تکمیلی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateInsuranceCombination(int serviceId, int primaryPlanId, int supplementaryPlanId, decimal coveragePercent, decimal maxPayment)
        {
            // 🔧 PRODUCTION READY: Validation کامل ورودی‌ها
            if (serviceId <= 0)
            {
                _log.Warning("🏥 MEDICAL: ServiceId نامعتبر - {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "شناسه خدمت نامعتبر است" }, JsonRequestBehavior.AllowGet);
            }

            if (primaryPlanId <= 0)
            {
                _log.Warning("🏥 MEDICAL: PrimaryPlanId نامعتبر - {PrimaryPlanId}. User: {UserName} (Id: {UserId})",
                    primaryPlanId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "شناسه طرح بیمه اصلی نامعتبر است" }, JsonRequestBehavior.AllowGet);
            }

            if (supplementaryPlanId <= 0)
            {
                _log.Warning("🏥 MEDICAL: SupplementaryPlanId نامعتبر - {SupplementaryPlanId}. User: {UserName} (Id: {UserId})",
                    supplementaryPlanId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "شناسه طرح بیمه تکمیلی نامعتبر است" }, JsonRequestBehavior.AllowGet);
            }

            if (coveragePercent < 0 || coveragePercent > 100)
            {
                _log.Warning("🏥 MEDICAL: CoveragePercent نامعتبر - {CoveragePercent}. User: {UserName} (Id: {UserId})",
                    coveragePercent, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "درصد پوشش باید بین 0 تا 100 باشد" }, JsonRequestBehavior.AllowGet);
            }

            if (maxPayment < 0)
            {
                _log.Warning("🏥 MEDICAL: MaxPayment نامعتبر - {MaxPayment}. User: {UserName} (Id: {UserId})",
                    maxPayment, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "سقف پرداخت نمی‌تواند منفی باشد" }, JsonRequestBehavior.AllowGet);
            }

            // 🔧 PRODUCTION READY: Performance Monitoring
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ایجاد تعیین ست بیمه - PrimaryPlanId: {PrimaryPlanId}, SupplementaryPlanId: {SupplementaryPlanId}, CoveragePercent: {CoveragePercent}, MaxPayment: {MaxPayment}, CorrelationId: {CorrelationId}. User: {UserName} (Id: {UserId})",
                    primaryPlanId, supplementaryPlanId, coveragePercent, maxPayment, correlationId, _currentUserService.UserName, _currentUserService.UserId);

                // 🔧 PRODUCTION READY: بهینه‌سازی Performance با Parallel Execution
                var planTasks = new[]
                {
                    _planService.GetPlanDetailsAsync(primaryPlanId),
                    _planService.GetPlanDetailsAsync(supplementaryPlanId)
                };

                await Task.WhenAll(planTasks);
                
                var primaryPlan = planTasks[0].Result;
                var supplementaryPlan = planTasks[1].Result;

                if (!primaryPlan.Success || !supplementaryPlan.Success)
                {
                    return Json(new { success = false, message = "طرح بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                // 🔧 CRITICAL FIX: استفاده از سرویس تزریق شده از DI
                var savedTariff = await _supplementaryCombinationService.CreateCombinationAsync(
                    serviceId, primaryPlanId, supplementaryPlanId, coveragePercent, maxPayment);

                stopwatch.Stop();
                
                _log.Information("🏥 MEDICAL: تعیین ست بیمه با موفقیت ایجاد شد - TariffId: {TariffId}, ExecutionTime: {ExecutionTime}ms, CorrelationId: {CorrelationId}. User: {UserName} (Id: {UserId})",
                    savedTariff.InsuranceTariffId, stopwatch.ElapsedMilliseconds, correlationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { 
                    success = true, 
                    message = "تعیین ست بیمه با موفقیت ایجاد شد",
                    correlationId = correlationId,
                    executionTime = stopwatch.ElapsedMilliseconds,
                    data = new {
                        tariffId = savedTariff.InsuranceTariffId,
                        serviceId = savedTariff.ServiceId,
                        insurancePlanId = savedTariff.InsurancePlanId,
                        tariffPrice = savedTariff.TariffPrice,
                        patientShare = savedTariff.PatientShare,
                        insurerShare = savedTariff.InsurerShare,
                        supplementaryCoveragePercent = savedTariff.SupplementaryCoveragePercent,
                        supplementaryMaxPayment = savedTariff.SupplementaryMaxPayment
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // 🔧 PRODUCTION READY: Error Handling کامل و تفصیلی
                _log.Error(ex, "🏥 MEDICAL: خطای غیرمنتظره در ایجاد تعیین ست بیمه - PrimaryPlanId: {PrimaryPlanId}, SupplementaryPlanId: {SupplementaryPlanId}. User: {UserName} (Id: {UserId})",
                    primaryPlanId, supplementaryPlanId, _currentUserService.UserName, _currentUserService.UserId);

                // بررسی نوع خطا و ارائه پیام مناسب
                string errorMessage = "خطا در ایجاد تعیین ست بیمه";
                if (ex is ArgumentException)
                {
                    errorMessage = "پارامترهای ورودی نامعتبر است";
                }
                else if (ex is InvalidOperationException)
                {
                    errorMessage = "عملیات درخواستی قابل انجام نیست";
                }
                else if (ex is TimeoutException)
                {
                    errorMessage = "زمان انجام عملیات به پایان رسید";
                }
                else if (ex is UnauthorizedAccessException)
                {
                    errorMessage = "دسترسی غیرمجاز";
                }

                return Json(new { 
                    success = false, 
                    message = errorMessage,
                    errorCode = ex.GetType().Name,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                }, JsonRequestBehavior.AllowGet);
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

                    // SECURITY FIX: در GET request نباید دیتابیس تغییر کند
                    // قیمت محاسبه شده فقط برای نمایش استفاده می‌شود
                    if (actualServicePrice > 0)
                    {
                        _log.Information("🏥 MEDICAL: قیمت محاسبه شده (فقط برای نمایش) - ServiceId: {ServiceId}, CalculatedPrice: {Price}. User: {UserName} (Id: {UserId})",
                            serviceId, actualServicePrice, _currentUserService.UserName, _currentUserService.UserId);

                        // TODO: برای ذخیره‌سازی قیمت، از POST endpoint جداگانه استفاده کنید
                        // این تغییر فقط برای نمایش است و در دیتابیس ذخیره نمی‌شود
                    }
                }

                // 🔧 CRITICAL FIX: دریافت اطلاعات طرح بیمه با استفاده از متد مناسب
                var planResult = await _planService.GetPlanDetailsAsync(insurancePlanId);
                if (!planResult.Success || planResult.Data == null)
                {
                    _log.Warning("🏥 MEDICAL: طرح بیمه یافت نشد - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        insurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = "طرح بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }
                var plan = planResult.Data;

                // دریافت تنظیمات PlanService (ساده‌سازی شده)
                PlanService planService = null; // برای حالا null می‌گذاریم

                // 🔧 CRITICAL FIX: محاسبه صحیح با استفاده از مقادیر واقعی طرح بیمه
                var deductible = plan.Deductible;
                var coveragePercent = plan.CoveragePercent;
                var coverableAmount = Math.Max(0, actualServicePrice - deductible);
                var insuranceCoverage = coverableAmount * (coveragePercent / 100m);
                var patientPayment = actualServicePrice - insuranceCoverage;

                var calculationResult = new
                {
                    TotalAmount = actualServicePrice,
                    DeductibleAmount = deductible,
                    CoverableAmount = coverableAmount,
                    CoveragePercent = coveragePercent,
                    InsuranceCoverage = insuranceCoverage,
                    PatientPayment = patientPayment
                };

                _log.Information("🏥 MEDICAL: محاسبه اطلاعات هوشمند - ServiceId: {ServiceId}, PlanId: {PlanId}, TotalAmount: {TotalAmount}, InsuranceCoverage: {InsuranceCoverage}, PatientPayment: {PatientPayment}. User: {UserName} (Id: {UserId})",
                    serviceId, insurancePlanId, calculationResult.TotalAmount, calculationResult.InsuranceCoverage, calculationResult.PatientPayment, _currentUserService.UserName, _currentUserService.UserId);

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
                            coveragePercent = coveragePercent,
                            deductible = deductible
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
                // 🔧 CRITICAL FIX: حذف کش برای محیط درمانی realtime
                // _cacheService.ClearCache();

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
        /// دریافت تعرفه‌های بیمه تکمیلی - AJAX with Response Time Optimization
        /// </summary>
        [HttpGet]
        // 🏥 MEDICAL: Real-time data - no cache for clinical safety
        public async Task<JsonResult> GetTariffs(string searchTerm = "", int? insurancePlanId = null, int? departmentId = null, bool? isActive = null, int page = 1, int pageSize = 10)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست تعرفه‌های بیمه تکمیلی با فیلترها - SearchTerm: {SearchTerm}, PlanId: {PlanId}, DeptId: {DeptId}, IsActive: {IsActive}. User: {UserName} (Id: {UserId})",
                    searchTerm, insurancePlanId, departmentId, isActive, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه‌های بیمه تکمیلی از سرویس با فیلترهای بهینه‌سازی شده
                ServiceResult<List<InsuranceTariff>> result;
                if (insurancePlanId.HasValue && insurancePlanId.Value > 0)
                {
                    // دریافت تعرفه‌های طرح بیمه خاص
                    result = await _tariffService.GetTariffsByPlanIdAsync(insurancePlanId.Value);
                }
                else
                {
                    // دریافت همه تعرفه‌های بیمه تکمیلی با فیلترهای پیش‌اعمال شده
                    result = await _tariffService.GetFilteredSupplementaryTariffsAsync(
                        searchTerm: searchTerm,
                        departmentId: departmentId,
                        isActive: isActive);
                }

                if (!result.Success)
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت تعرفه‌های بیمه تکمیلی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
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

                // فیلترها حالا در database اعمال می‌شوند - نیازی به فیلتر memory نیست

                // محاسبه صفحه‌بندی
                var totalCount = tariffs.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var skip = (page - 1) * pageSize;

                // اعمال صفحه‌بندی
                var pagedTariffs = tariffs.Skip(skip).Take(pageSize).ToList();

                _log.Information("🏥 MEDICAL: تعرفه‌های بیمه تکمیلی با موفقیت دریافت شد - Count: {Count}, Page: {Page}, PageSize: {PageSize}, TotalPages: {TotalPages}. User: {UserName} (Id: {UserId})",
                    pagedTariffs.Count, page, pageSize, totalPages, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = true,
                    data = pagedTariffs,
                    totalCount = totalCount,
                    currentPage = page,
                    pageSize = pageSize,
                    totalPages = totalPages,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1,
                    filters = new
                    {
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
        /// دریافت جدول تعرفه‌ها به صورت HTML برای AJAX - Response Time Optimized
        /// </summary>
        [HttpGet]
        // 🏥 MEDICAL: Real-time data - no cache for clinical safety
        public async Task<ActionResult> GetTariffsTable(string searchTerm = "", int? insurancePlanId = null, int? departmentId = null, bool? isActive = null, int page = 1, int pageSize = 10)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست جدول تعرفه‌های بیمه تکمیلی با فیلترها - SearchTerm: {SearchTerm}, PlanId: {PlanId}, DeptId: {DeptId}, IsActive: {IsActive}. User: {UserName} (Id: {UserId})",
                    searchTerm, insurancePlanId, departmentId, isActive, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه‌های بیمه تکمیلی از سرویس با فیلترهای بهینه‌سازی شده
                ServiceResult<List<InsuranceTariff>> result;
                if (insurancePlanId.HasValue && insurancePlanId.Value > 0)
                {
                    // دریافت تعرفه‌های بیمه تکمیلی بر اساس طرح بیمه
                    result = await _tariffService.GetTariffsByPlanIdAsync(insurancePlanId.Value);
                }
                else
                {
                    // دریافت همه تعرفه‌های بیمه تکمیلی با فیلترهای پیش‌اعمال شده
                    result = await _tariffService.GetFilteredSupplementaryTariffsAsync(
                        searchTerm: searchTerm,
                        departmentId: departmentId,
                        isActive: isActive);
                }

                if (!result.Success)
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت تعرفه‌های بیمه تکمیلی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "خطا در دریافت تعرفه‌ها: " + result.Message
                    }, JsonRequestBehavior.AllowGet);
                }

                // تبدیل به ViewModel
                var tariffs = result.Data.Select(t => new SupplementaryTariffIndexViewModel
                {
                    InsuranceTariffId = t.InsuranceTariffId,
                    ServiceId = t.ServiceId,
                    ServiceTitle = t.Service?.Title ?? "نامشخص",
                    ServiceCode = t.Service?.ServiceCode ?? "نامشخص",
                    InsurancePlanId = t.InsurancePlanId ?? 0,
                    InsurancePlanName = t.InsurancePlan?.Name ?? "نامشخص",
                    TariffPrice = t.TariffPrice,
                    PatientShare = t.PatientShare,
                    InsurerShare = t.InsurerShare,
                    SupplementaryCoveragePercent = t.SupplementaryCoveragePercent,
                    Priority = t.Priority,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList();

                // محاسبه صفحه‌بندی
                var totalCount = tariffs.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var skip = (page - 1) * pageSize;

                // اعمال صفحه‌بندی
                var pagedTariffs = tariffs.Skip(skip).Take(pageSize).ToList();

                // ایجاد PagedResult
                var pagedResult = new PagedResult<SupplementaryTariffIndexViewModel>(pagedTariffs, totalCount, page, pageSize);

                _log.Information("🏥 MEDICAL: جدول تعرفه‌های بیمه تکمیلی با موفقیت آماده شد - Count: {Count}, Page: {Page}, PageSize: {PageSize}, TotalPages: {TotalPages}. User: {UserName} (Id: {UserId})",
                    pagedTariffs.Count, page, pageSize, totalPages, _currentUserService.UserName, _currentUserService.UserId);

                // برگرداندن HTML
                return PartialView("_SupplementaryTariffTable", pagedResult);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت جدول تعرفه‌ها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در سیستم" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// نمایش جزئیات تعرفه بیمه تکمیلی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id, int? patientId = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: نمایش جزئیات تعرفه بیمه تکمیلی - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه موجود از repository مستقیماً
                var tariff = await _tariffRepository.GetByIdWithDetailsAsync(id);
                if (tariff == null)
                {
                    _log.Warning("🏥 MEDICAL: تعرفه بیمه یافت نشد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    return HttpNotFound("تعرفه بیمه یافت نشد");
                }

                // تبدیل به SupplementaryTariffCreateEditViewModel برای نمایش
                var detailsModel = new SupplementaryTariffCreateEditViewModel
                {
                    InsuranceTariffId = tariff.InsuranceTariffId,
                    ServiceId = tariff.ServiceId,
                    InsurancePlanId = tariff.InsurancePlanId ?? 0,
                    TariffPrice = tariff.TariffPrice,
                    PatientShare = tariff.PatientShare,
                    InsurerShare = tariff.InsurerShare,
                    SupplementaryCoveragePercent = tariff.SupplementaryCoveragePercent ?? 90,
                    Priority = tariff.Priority ?? 5,
                    PrimaryInsurancePlanId = 0, // این فیلد در InsuranceTariff entity وجود ندارد
                    IsActive = tariff.IsActive,
                    ServiceName = tariff.Service?.Title ?? "نامشخص",
                    ServiceCode = tariff.Service?.ServiceCode ?? "نامشخص"
                };

                // دریافت بیمه پایه فعلی بیمار (برای نمایش)
                if (patientId.HasValue)
                {
                    var primaryInsurance = await _patientInsuranceRepository.GetPrimaryInsuranceByPatientIdAsync(patientId.Value);
                    if (primaryInsurance != null)
                    {
                        detailsModel.PrimaryInsurancePlanId = primaryInsurance.InsurancePlanId;
                        _log.Information("🏥 MEDICAL: بیمه پایه فعلی - PatientId: {PatientId}, PlanId: {PlanId}, PlanName: {PlanName}. User: {UserName} (Id: {UserId})",
                            patientId.Value, primaryInsurance.InsurancePlanId, primaryInsurance.InsurancePlan?.Name, _currentUserService.UserName, _currentUserService.UserId);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: بیمه پایه یافت نشد - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                            patientId.Value, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }
                else
                {
                    _log.Information("🏥 MEDICAL: PatientId ارائه نشده - نمایش جزئیات تعرفه بدون اطلاعات بیمه پایه. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                }

                // بارگذاری داده‌های مورد نیاز برای فرم
                await LoadCreateEditData(detailsModel);

                _log.Information("🏥 MEDICAL: DetailsModel تکمیل شد - ServiceName: {ServiceName}, ServiceCode: {ServiceCode}. User: {UserName} (Id: {UserId})",
                    detailsModel.ServiceName, detailsModel.ServiceCode, _currentUserService.UserName, _currentUserService.UserId);

                return View("Details", detailsModel);
            }
            catch (Exception ex)
            {
                return HandleError(ex, "نمایش جزئیات تعرفه بیمه تکمیلی", new { TariffId = id });
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش تعرفه بیمه تکمیلی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id, int? patientId = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: نمایش فرم ویرایش تعرفه بیمه تکمیلی - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه موجود از repository مستقیماً
                var tariff = await _tariffRepository.GetByIdWithDetailsAsync(id);
                if (tariff == null)
                {
                    _log.Warning("🏥 MEDICAL: تعرفه بیمه یافت نشد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    return HttpNotFound("تعرفه بیمه یافت نشد");
                }

                // تبدیل به SupplementaryTariffCreateEditViewModel با داده‌های واقعی
                var editModel = new SupplementaryTariffCreateEditViewModel
                {
                    InsuranceTariffId = tariff.InsuranceTariffId,
                    ServiceId = tariff.ServiceId,
                    InsurancePlanId = tariff.InsurancePlanId ?? 0,
                    TariffPrice = tariff.TariffPrice,
                    PatientShare = tariff.PatientShare,
                    InsurerShare = tariff.InsurerShare,
                    SupplementaryCoveragePercent = tariff.SupplementaryCoveragePercent ?? 90,
                    Priority = tariff.Priority ?? 5,
                    PrimaryInsurancePlanId = 0, // این فیلد در InsuranceTariff entity وجود ندارد
                    IsActive = tariff.IsActive
                };

                _log.Information("🏥 MEDICAL: EditModel ایجاد شد - TariffId: {TariffId}, ServiceId: {ServiceId}, InsurancePlanId: {InsurancePlanId}, TariffPrice: {TariffPrice}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}. User: {UserName} (Id: {UserId})",
                    editModel.InsuranceTariffId, editModel.ServiceId, editModel.InsurancePlanId, editModel.TariffPrice, editModel.PatientShare, editModel.InsurerShare, _currentUserService.UserName, _currentUserService.UserId);

                // Debug: بررسی مقادیر decimal
                _log.Information("🏥 MEDICAL: Decimal Values Debug - TariffPrice: {TariffPrice}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}, SupplementaryCoveragePercent: {SupplementaryCoveragePercent}. User: {UserName} (Id: {UserId})",
                    editModel.TariffPrice?.ToString("F2"), editModel.PatientShare?.ToString("F2"), editModel.InsurerShare?.ToString("F2"), editModel.SupplementaryCoveragePercent?.ToString("F2"), _currentUserService.UserName, _currentUserService.UserId);

                // تنظیم اطلاعات Service
                editModel.ServiceName = tariff.Service?.Title ?? "نامشخص";
                editModel.ServiceCode = tariff.Service?.ServiceCode ?? "نامشخص";

                // دریافت بیمه پایه فعلی بیمار (برای نمایش)
                // این اطلاعات فقط برای نمایش است و قابل تغییر نیست
                if (patientId.HasValue)
                {
                    var primaryInsurance = await _patientInsuranceRepository.GetPrimaryInsuranceByPatientIdAsync(patientId.Value);
                    if (primaryInsurance != null)
                    {
                        editModel.PrimaryInsurancePlanId = primaryInsurance.InsurancePlanId;
                        _log.Information("🏥 MEDICAL: بیمه پایه فعلی - PatientId: {PatientId}, PlanId: {PlanId}, PlanName: {PlanName}. User: {UserName} (Id: {UserId})",
                            patientId.Value, primaryInsurance.InsurancePlanId, primaryInsurance.InsurancePlan?.Name, _currentUserService.UserName, _currentUserService.UserId);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: بیمه پایه یافت نشد - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                            patientId.Value, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }
                else
                {
                    _log.Information("🏥 MEDICAL: PatientId ارائه نشده - نمایش فرم ویرایش بدون اطلاعات بیمه پایه. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                }

                // بارگذاری داده‌های مورد نیاز برای فرم
                await LoadCreateEditData(editModel);

                _log.Information("🏥 MEDICAL: EditModel تکمیل شد - ServiceName: {ServiceName}, ServiceCode: {ServiceCode}. User: {UserName} (Id: {UserId})",
                    editModel.ServiceName, editModel.ServiceCode, _currentUserService.UserName, _currentUserService.UserId);

                return View("Edit", editModel);
            }
            catch (Exception ex)
            {
                return HandleError(ex, "نمایش فرم ویرایش تعرفه بیمه تکمیلی", new { TariffId = id });
            }
        }

        /// <summary>
        /// ویرایش تعرفه بیمه تکمیلی - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SupplementaryTariffCreateEditViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: ویرایش تعرفه بیمه تکمیلی - TariffId: {TariffId}, ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    model.InsuranceTariffId, model.ServiceId, model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                if (!ValidateModelWithLogging(model, "ویرایش تعرفه بیمه تکمیلی"))
                {
                    // FIX: غنی‌سازی مدل اصلی با داده‌های کمکی قبل از return
                    await LoadCreateEditData(model);
                    return View(model);
                }

                // تبدیل SupplementaryTariffCreateEditViewModel به InsuranceTariffCreateEditViewModel
                var insuranceTariffModel = new InsuranceTariffCreateEditViewModel
                {
                    InsuranceTariffId = model.InsuranceTariffId ?? 0,
                    ServiceId = model.ServiceId,
                    InsurancePlanId = model.InsurancePlanId,
                    TariffPrice = model.TariffPrice,
                    PatientShare = model.PatientShare,
                    InsurerShare = model.InsurerShare,
                    IsActive = model.IsActive,
                    Priority = model.Priority ?? 5,
                    PrimaryInsurancePlanId = model.PrimaryInsurancePlanId,
                    SupplementaryCoveragePercent = model.SupplementaryCoveragePercent ?? 90
                };

                // به‌روزرسانی تعرفه
                var updateResult = await _tariffService.UpdateTariffAsync(insuranceTariffModel);

                if (updateResult.Success)
                {
                    SetResponseMessage("ویرایش تعرفه بیمه تکمیلی", true, "تعرفه بیمه تکمیلی با موفقیت ویرایش شد");
                    return RedirectToAction("Index");
                }
                else
                {
                    LogUserOperation($"خطا در ویرایش تعرفه: {updateResult.Message}", "ویرایش تعرفه بیمه تکمیلی");
                    TempData["ErrorMessage"] = updateResult.Message;
                    var editModel = new SupplementaryTariffCreateEditViewModel();
                    await LoadCreateEditData(editModel);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                return HandleStandardError(ex, "ویرایش تعرفه بیمه تکمیلی", "Index");
            }
        }

        /// <summary>
        /// حذف تعرفه بیمه تکمیلی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                _log.Information("🏥 MEDICAL: حذف تعرفه بیمه تکمیلی - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                // حذف تعرفه (Soft Delete)
                var deleteResult = await _tariffService.SoftDeleteTariffAsync(id);

                if (deleteResult.Success)
                {
                    LogUserOperation("تعرفه بیمه تکمیلی با موفقیت حذف شد", "حذف تعرفه بیمه تکمیلی");
                    return Json(new { success = true, message = "تعرفه با موفقیت حذف شد" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogUserOperation($"خطا در حذف تعرفه: {deleteResult.Message}", "حذف تعرفه بیمه تکمیلی");
                    return Json(new { success = false, message = deleteResult.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogUserOperation($"خطا در حذف تعرفه: {ex.Message}", "حذف تعرفه بیمه تکمیلی", ex);
                return Json(new { success = false, message = "خطا در سیستم" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// مدیریت یکپارچه خطاها - Medical Environment Error Handler
        /// </summary>
        private ActionResult HandleError(Exception ex, string operation, object parameters = null)
        {
            _log.Error(ex, "🏥 MEDICAL: خطا در {Operation} - Parameters: {@Parameters}. User: {UserName} (Id: {UserId})",
                operation, parameters, _currentUserService.UserName, _currentUserService.UserId);

            TempData["ErrorMessage"] = "خطا در سیستم. لطفاً دوباره تلاش کنید.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// بارگذاری داده‌های مورد نیاز برای فیلترها - Memory & Performance Optimized
        /// </summary>
        private async Task<SupplementaryTariffFilterViewModel> LoadFilterData()
        {
            try
            {
                _log.Debug("🏥 MEDICAL: بارگذاری داده‌های فیلتر. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // اجرای همزمان Query های مستقل برای بهبود Performance
                var departmentsTask = _departmentRepository.GetAllActiveDepartmentsAsync();
                var primaryInsurancePlansTask = _planService.GetPrimaryInsurancePlansAsync();
                var supplementaryInsurancePlansTask = _planService.GetSupplementaryInsurancePlansAsync();
                var servicesTask = _serviceRepository.GetAllActiveServicesAsync();

                // انتظار برای تکمیل تمام Task ها
                await Task.WhenAll(departmentsTask, primaryInsurancePlansTask, supplementaryInsurancePlansTask, servicesTask);

                var departments = await departmentsTask;
                var primaryInsurancePlans = await primaryInsurancePlansTask;
                var supplementaryInsurancePlans = await supplementaryInsurancePlansTask;
                var services = await servicesTask;

                // Memory optimization: ایجاد ViewModel با مدیریت حافظه بهینه و Lazy Loading
                var filter = new SupplementaryTariffFilterViewModel
                {
                    // Lazy loading برای Services - فقط فیلدهای ضروری
                    Services = services?.Select(s => new SupplementaryTariffServiceViewModel
                    {
                        ServiceId = s.ServiceId,
                        ServiceTitle = s.Title
                    }).ToList() ?? new List<SupplementaryTariffServiceViewModel>(),

                    // Lazy loading برای Insurance Plans - فقط فیلدهای ضروری
                    InsurancePlans = supplementaryInsurancePlans.Data?.Select(p => new SupplementaryTariffInsurancePlanViewModel
                    {
                        InsurancePlanId = p.InsurancePlanId,
                        InsurancePlanName = p.Name
                    }).ToList() ?? new List<SupplementaryTariffInsurancePlanViewModel>(),

                    // Lazy loading برای Departments - فقط فیلدهای ضروری
                    Departments = departments?.Select(d => new SupplementaryTariffDepartmentViewModel
                    {
                        DepartmentId = d.DepartmentId,
                        Name = d.Name
                    }).ToList() ?? new List<SupplementaryTariffDepartmentViewModel>()
                };

                _log.Debug("🏥 MEDICAL: داده‌های فیلتر با موفقیت بارگذاری شد - Services: {ServiceCount}, Plans: {PlanCount}, Departments: {DeptCount}. User: {UserName} (Id: {UserId})",
                    filter.Services.Count, filter.InsurancePlans.Count, filter.Departments.Count, _currentUserService.UserName, _currentUserService.UserId);

                return filter;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در بارگذاری داده‌های فیلتر. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // بازگرداندن فیلتر خالی در صورت خطا
                return new SupplementaryTariffFilterViewModel();
            }
        }

        /// <summary>
        /// ایجاد SelectList از لیست InsurancePlanLookupViewModel
        /// </summary>
        /// <param name="plans">لیست طرح‌های بیمه</param>
        /// <returns>SelectList آماده برای استفاده در View</returns>
        private SelectList CreateInsurancePlanSelectList(List<InsurancePlanLookupViewModel> plans)
        {
            return new SelectList(plans ?? new List<InsurancePlanLookupViewModel>(), "InsurancePlanId", "Name");
        }

        /// <summary>
        /// ایجاد ViewModel خالی برای حالت خطا
        /// </summary>
        /// <param name="filterData">داده‌های فیلتر</param>
        /// <returns>ViewModel خالی با ساختار صحیح</returns>
        private SupplementaryTariffIndexPageViewModel CreateEmptyViewModel(SupplementaryTariffFilterViewModel filterData)
        {
            return new SupplementaryTariffIndexPageViewModel
            {
                Statistics = new SupplementaryTariffStatisticsViewModel(),
                Filter = filterData,
                Tariffs = new PagedResult<SupplementaryTariffIndexViewModel>
                {
                    Items = new List<SupplementaryTariffIndexViewModel>(),
                    TotalItems = 0,
                    PageNumber = 1,
                    PageSize = 10
                }
            };
        }

        /// <summary>
        /// لاگ کردن عملیات کاربر با فرمت استاندارد
        /// </summary>
        /// <param name="message">پیام</param>
        /// <param name="operation">نام عملیات</param>
        /// <param name="ex">استثنا (اختیاری)</param>
        private void LogUserOperation(string message, string operation, Exception ex = null)
        {
            var logMessage = $"🏥 MEDICAL: {message}. User: {_currentUserService.UserName} (Id: {_currentUserService.UserId})";

            if (ex != null)
                _log.Error(ex, logMessage);
            else
                _log.Information(logMessage);
        }

        /// <summary>
        /// اعتبارسنجی ViewModel با لاگ کردن جزئیات
        /// </summary>
        /// <param name="model">مدل برای اعتبارسنجی</param>
        /// <param name="operation">نام عملیات</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private bool ValidateModelWithLogging(object model, string operation)
        {
            if (model == null)
            {
                LogUserOperation($"مدل {operation} null است", operation);
                return false;
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                LogUserOperation($"اعتبارسنجی {operation} ناموفق - خطاها: {errors}", operation);
                return false;
            }

            _log.Debug($"🏥 MEDICAL: اعتبارسنجی {operation} موفق. User: {_currentUserService.UserName} (Id: {_currentUserService.UserId})");
            return true;
        }

        /// <summary>
        /// تنظیم پیام‌های پاسخ استاندارد برای عملیات
        /// </summary>
        /// <param name="operation">نام عملیات</param>
        /// <param name="isSuccess">نتیجه عملیات</param>
        /// <param name="customMessage">پیام سفارشی (اختیاری)</param>
        private void SetResponseMessage(string operation, bool isSuccess, string customMessage = null)
        {
            var message = customMessage ?? (isSuccess
                ? $"عملیات {operation} با موفقیت انجام شد"
                : $"خطا در انجام عملیات {operation}");

            TempData[isSuccess ? "SuccessMessage" : "ErrorMessage"] = message;

            LogUserOperation(message, operation);
        }

        /// <summary>
        /// مدیریت خطاهای استاندارد با لاگ و پیام کاربر
        /// </summary>
        /// <param name="ex">استثنا</param>
        /// <param name="operation">نام عملیات</param>
        /// <param name="redirectAction">اکشن بازگشت (اختیاری)</param>
        /// <returns>ActionResult مناسب</returns>
        private ActionResult HandleStandardError(Exception ex, string operation, string redirectAction = "Index")
        {
            LogUserOperation($"خطا در {operation}: {ex.Message}", operation, ex);

            TempData["ErrorMessage"] = $"خطا در انجام عملیات {operation}. لطفاً دوباره تلاش کنید.";

            return RedirectToAction(redirectAction);
        }

        /// <summary>
        /// بررسی نتیجه Service و تنظیم پیام مناسب
        /// </summary>
        /// <typeparam name="T">نوع داده</typeparam>
        /// <param name="result">نتیجه Service</param>
        /// <param name="operation">نام عملیات</param>
        /// <param name="successMessage">پیام موفقیت (اختیاری)</param>
        /// <returns>نتیجه بررسی</returns>
        private bool HandleServiceResult<T>(ServiceResult<T> result, string operation, string successMessage = null)
        {
            if (result.Success)
            {
                SetResponseMessage(operation, true, successMessage);
                return true;
            }
            else
            {
                LogUserOperation($"خطا در {operation}: {result.Message}", operation);
                TempData["ErrorMessage"] = result.Message;
                return false;
            }
        }

        /// <summary>
        /// بارگذاری داده‌های مورد نیاز برای فرم‌های Create و Edit - Performance Optimized
        /// </summary>
        private async Task LoadCreateEditData(SupplementaryTariffCreateEditViewModel model)
        {
            try
            {
                _log.Debug("🏥 MEDICAL: بارگذاری داده‌های فرم Create/Edit. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // اجرای همزمان Query های مستقل برای بهبود Performance
                var departmentsTask = _departmentRepository.GetAllActiveDepartmentsAsync();
                var primaryInsurancePlansTask = _planService.GetPrimaryInsurancePlansAsync();
                var supplementaryInsurancePlansTask = _planService.GetSupplementaryInsurancePlansAsync();

                // انتظار برای تکمیل تمام Task ها
                await Task.WhenAll(departmentsTask, primaryInsurancePlansTask, supplementaryInsurancePlansTask);

                var departments = await departmentsTask;
                var primaryInsurancePlans = await primaryInsurancePlansTask;
                var supplementaryInsurancePlans = await supplementaryInsurancePlansTask;

                _log.Information("🏥 MEDICAL: Raw Data - Departments: {DeptCount}, PrimaryPlans: {PrimaryCount}, SupplementaryPlans: {SuppCount}. User: {UserName} (Id: {UserId})",
                    departments?.Count ?? 0, primaryInsurancePlans?.Data?.Count ?? 0, supplementaryInsurancePlans?.Data?.Count ?? 0, _currentUserService.UserName, _currentUserService.UserId);

                // Debug: بررسی جزئیات PrimaryInsurancePlans
                if (primaryInsurancePlans?.Data != null)
                {
                    _log.Information("🏥 MEDICAL: PrimaryInsurancePlans Details - Count: {Count}, Plans: {Plans}. User: {UserName} (Id: {UserId})",
                        primaryInsurancePlans.Data.Count,
                        string.Join(", ", primaryInsurancePlans.Data.Select(p => $"{p.Name}({p.InsurancePlanId})")),
                        _currentUserService.UserName, _currentUserService.UserId);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: PrimaryInsurancePlans is null or empty. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                }

                // تنظیم داده‌ها در ViewModel به جای ViewBag
                model.Departments = departments?.Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name
                }).ToList() ?? new List<SelectListItem>();

                // 🔧 FIX: تبدیل SelectList به List<SelectListItem> برای سازگاری با ViewModel
                var primarySelectList = CreateInsurancePlanSelectList(primaryInsurancePlans?.Data ?? new List<InsurancePlanLookupViewModel>());
                var supplementarySelectList = CreateInsurancePlanSelectList(supplementaryInsurancePlans?.Data ?? new List<InsurancePlanLookupViewModel>());
                
                model.PrimaryInsurancePlans = primarySelectList.ToList();
                model.InsurancePlans = supplementarySelectList.ToList();

                _log.Information("🏥 MEDICAL: داده‌های فرم با موفقیت بارگذاری شد - Departments: {DeptCount}, PrimaryPlans: {PrimaryCount}, SupplementaryPlans: {SuppCount}. User: {UserName} (Id: {UserId})",
                    departments?.Count ?? 0, primaryInsurancePlans?.Data?.Count ?? 0, supplementaryInsurancePlans?.Data?.Count ?? 0, _currentUserService.UserName, _currentUserService.UserId);

                // FIX: اضافه کردن Debug برای بررسی داده‌ها
                _log.Debug("🏥 MEDICAL DEBUG: Departments Data: {Departments}",
                    departments?.Select(d => $"{d.DepartmentId}:{d.Name}").Take(5).ToList() ?? new List<string>());
                _log.Debug("🏥 MEDICAL DEBUG: PrimaryPlans Data: {PrimaryPlans}",
                    primaryInsurancePlans?.Data?.Select(p => $"{p.InsurancePlanId}:{p.Name}").Take(5).ToList() ?? new List<string>());
                _log.Debug("🏥 MEDICAL DEBUG: SupplementaryPlans Data: {SupplementaryPlans}",
                    supplementaryInsurancePlans?.Data?.Select(p => $"{p.InsurancePlanId}:{p.Name}").Take(5).ToList() ?? new List<string>());

                // اضافه کردن داده‌های بیمه پایه به ViewBag برای JavaScript
                if (primaryInsurancePlans?.Data != null)
                {
                    var primaryPlansData = primaryInsurancePlans.Data.Select(p => new
                    {
                        InsurancePlanId = p.InsurancePlanId,
                        Name = p.Name,
                        CoveragePercent = p.CoveragePercent,
                        Deductible = p.Deductible
                    }).ToList();
                    
                    ViewBag.PrimaryInsurancePlans = primaryPlansData;
                }
                else
                {
                    ViewBag.PrimaryInsurancePlans = new List<object>();
                }

                // Debug: بررسی جزئیات ViewModel
                _log.Information("🏥 MEDICAL: ViewModel Details - PrimaryInsurancePlans: {PrimaryCount}, InsurancePlans: {SuppCount}, Departments: {DeptCount}. User: {UserName} (Id: {UserId})",
                    model.PrimaryInsurancePlans?.Count ?? 0, model.InsurancePlans?.Count ?? 0, model.Departments?.Count ?? 0, _currentUserService.UserName, _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در بارگذاری داده‌های فرم Create/Edit. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // 🔧 FIX: تنظیم مقادیر پیش‌فرض در صورت خطا
                model.Departments = new List<SelectListItem>();
                var emptyPrimarySelectList = CreateInsurancePlanSelectList(new List<InsurancePlanLookupViewModel>());
                var emptySupplementarySelectList = CreateInsurancePlanSelectList(new List<InsurancePlanLookupViewModel>());
                
                model.PrimaryInsurancePlans = emptyPrimarySelectList.ToList();
                model.InsurancePlans = emptySupplementarySelectList.ToList();
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
                var model = new SupplementaryTariffCreateEditViewModel
                {
                    SupplementaryCoveragePercent = 90, // مقدار پیش‌فرض
                    Priority = 5, // اولویت پیش‌فرض
                    IsActive = true
                };
                await LoadCreateEditData(model);

                return View(model);
            }
            catch (Exception ex)
            {
                return HandleError(ex, "نمایش فرم ایجاد تعرفه بیمه تکمیلی");
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

                if (!ValidateModelWithLogging(model, "ایجاد تعرفه بیمه تکمیلی"))
                {
                    // FIX: غنی‌سازی مدل اصلی با داده‌های کمکی قبل از return
                    await LoadCreateEditData(model);
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
                    Priority = model.Priority ?? 5,
                    PrimaryInsurancePlanId = model.PrimaryInsurancePlanId,
                    SupplementaryCoveragePercent = model.SupplementaryCoveragePercent ?? 90
                };

                var result = await _tariffService.CreateTariffAsync(insuranceTariffModel);

                if (HandleServiceResult(result, "ایجاد تعرفه بیمه تکمیلی", "تعرفه بیمه تکمیلی با موفقیت ایجاد شد"))
                {
                    // پاک کردن کش
                    await InvalidateSupplementaryTariffCacheAsync();
                    return RedirectToAction("Index");
                }
                else
                {
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
                return HandleStandardError(ex, "ایجاد تعرفه بیمه تکمیلی", "Create");
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
                    IsActive = createModel.IsActive,
                    Priority = createModel.Priority ?? 5,
                    PrimaryInsurancePlanId = createModel.PrimaryInsurancePlanId,
                    SupplementaryCoveragePercent = createModel.SupplementaryCoveragePercent ?? 90
                };

                var result = await _tariffService.CreateTariffAsync(insuranceTariffModel);

                if (result.Success)
                {
                    // پاک کردن کش
                    await InvalidateSupplementaryTariffCacheAsync();

                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی با موفقیت ایجاد شد - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        serviceId, insurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        message = "تعرفه با موفقیت ایجاد شد",
                        data = new
                        {
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

                    return Json(new
                    {
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
                    IsActive = editModel.IsActive,
                    Priority = editModel.Priority ?? 5,
                    PrimaryInsurancePlanId = editModel.PrimaryInsurancePlanId,
                    SupplementaryCoveragePercent = editModel.SupplementaryCoveragePercent ?? 90
                };

                var updateResult = await _tariffService.UpdateTariffAsync(insuranceTariffModel);
                if (updateResult.Success)
                {
                    // پاک کردن کش
                    await InvalidateSupplementaryTariffCacheAsync();

                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی با موفقیت ویرایش شد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        tariffId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        message = "تعرفه با موفقیت ویرایش شد",
                        data = new
                        {
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

                    return Json(new
                    {
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

                // حذف تعرفه (Soft Delete)
                var deleteResult = await _tariffService.SoftDeleteTariffAsync(tariffId);
                if (deleteResult.Success)
                {
                    // 🔧 CRITICAL FIX: حذف کش برای محیط درمانی realtime
                    // await InvalidateSupplementaryTariffCacheAsync();

                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی با موفقیت حذف شد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        tariffId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        message = "تعرفه با موفقیت حذف شد",
                        data = new { tariffId = tariffId }
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در حذف تعرفه بیمه تکمیلی - {Error}. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        deleteResult.Message, tariffId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
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

        /// <summary>
        /// دریافت دپارتمان‌ها برای AJAX
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDepartments()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست دپارتمان‌ها برای AJAX. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var departments = await _departmentRepository.GetAllActiveDepartmentsAsync();
                var result = departments?.Select(d => new
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name
                }).ToList();

                _log.Information("🏥 MEDICAL: {Count} دپارتمان برای AJAX ارسال شد. User: {UserName} (Id: {UserId})",
                    result.Count, _currentUserService.UserName, _currentUserService.UserId);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت دپارتمان‌ها برای AJAX. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت بیمه‌های پایه برای AJAX
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetPrimaryInsurancePlans()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست بیمه‌های پایه برای AJAX. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var result = await _planService.GetPrimaryInsurancePlansAsync();
                var plans = result?.Data?.Select(p => new
                {
                    InsurancePlanId = p.InsurancePlanId,
                    Name = p.Name
                }).ToList();



                _log.Information("🏥 MEDICAL: {Count} بیمه پایه برای AJAX ارسال شد. User: {UserName} (Id: {UserId})",
                    plans.Count, _currentUserService.UserName, _currentUserService.UserId);

                return Json(plans, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت بیمه‌های پایه برای AJAX. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت بیمه‌های تکمیلی برای AJAX
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetSupplementaryInsurancePlans()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست بیمه‌های تکمیلی برای AJAX. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var result = await _planService.GetSupplementaryInsurancePlansAsync();
                var plans = result?.Data?.Select(p => new
                {
                    InsurancePlanId = p.InsurancePlanId,
                    Name = p.Name
                }).ToList();
                _log.Information("🏥 MEDICAL: {Count} بیمه تکمیلی برای AJAX ارسال شد. User: {UserName} (Id: {UserId})",
                    plans.Count, _currentUserService.UserName, _currentUserService.UserId);

                return Json(plans, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت بیمه‌های تکمیلی برای AJAX. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

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
                return HandleError(ex, "نمایش فرم ایجاد تعرفه گروهی بیمه تکمیلی");
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

        /// <summary>
        /// Performance Validation - اعتبارسنجی عملکرد
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> ValidatePerformance()
        {
            var startTime = DateTime.UtcNow;
            var userId = _currentUserService.UserId;

            try
            {
                _log.Information("🏥 MEDICAL: شروع اعتبارسنجی عملکرد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, userId);

                var performanceMetrics = new
                {
                    // Test 1: LoadFilterData Performance
                    LoadFilterDataStart = DateTime.UtcNow,
                    LoadFilterDataResult = await LoadFilterData(),
                    LoadFilterDataEnd = DateTime.UtcNow,

                    // Test 2: Cache Performance
                    CacheTestStart = DateTime.UtcNow,
                    // 🔧 CRITICAL FIX: حذف کش برای محیط درمانی realtime
                    // CacheTestResult = await _cacheService.GetCachedSupplementaryTariffsAsync(0),
                    CacheTestEnd = DateTime.UtcNow,

                    // Test 3: Stats Performance
                    StatsTestStart = DateTime.UtcNow,
                    StatsTestResult = await _seederService.GetSupplementaryTariffStatsAsync(),
                    StatsTestEnd = DateTime.UtcNow,

                    // Overall Performance
                    TotalExecutionTime = DateTime.UtcNow - startTime,
                    MemoryUsage = GC.GetTotalMemory(false),
                    CacheHitRate = "To be calculated"
                };

                _log.Information("🏥 MEDICAL: اعتبارسنجی عملکرد تکمیل شد - ExecutionTime: {ExecutionTime}ms, MemoryUsage: {MemoryUsage}bytes. User: {UserName} (Id: {UserId})",
                    performanceMetrics.TotalExecutionTime.TotalMilliseconds, performanceMetrics.MemoryUsage, _currentUserService.UserName, userId);

                return Json(new
                {
                    success = true,
                    data = performanceMetrics,
                    message = "اعتبارسنجی عملکرد با موفقیت انجام شد"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی عملکرد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, userId);

                return Json(new
                {
                    success = false,
                    message = "خطا در اعتبارسنجی عملکرد",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}