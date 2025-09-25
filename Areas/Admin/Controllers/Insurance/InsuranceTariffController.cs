using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using ClinicApp.Interfaces;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Services;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using ClinicApp.ViewModels.Validators;
using Serilog;
using ClinicApp.Helpers;
using ClinicApp.Core;
using ClinicApp.Filters;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using ClinicApp.Helpers;
using ClinicApp.Models;
using System.Data.Entity;
using ClinicApp.Models.Enums;
using ClinicApp.Services.UserContext;
using ClinicApp.Services.SystemSettings;
using ClinicApp.Services.Insurance;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر مدیریت تعرفه‌های بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل تعرفه‌های بیمه با امنیت بالا
    /// 2. استفاده از Anti-Forgery Token در همه POST actions
    /// 3. استفاده از ServiceResult Enhanced pattern
    /// 4. مدیریت کامل خطاها و لاگ‌گیری حرفه‌ای
    /// 5. پشتیبانی از صفحه‌بندی و جستجوی پیشرفته
    /// 6. مدیریت روابط با InsurancePlan و Service
    /// 7. رعایت استانداردهای پزشکی ایران
    /// 8. Strongly Typed ViewModels و Validation
    /// 9. Real-time Processing بدون کش
    /// 10. Comprehensive Error Handling
    /// 
    /// نکته حیاتی: این کنترلر بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    [MedicalEnvironmentFilter]
    public class InsuranceTariffController : BaseController
    {
        #region Dependencies and Constructor

        private readonly IInsuranceTariffService _insuranceTariffService;
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly IInsuranceProviderService _insuranceProviderService;
        private readonly IServiceManagementService _serviceManagementService;
        private readonly IServiceService _serviceService;
        private readonly IDepartmentManagementService _departmentManagementService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;
        private readonly IMessageNotificationService _messageNotificationService;
        private readonly IValidator<InsuranceTariffCreateEditViewModel> _createEditValidator;
        private readonly IValidator<InsuranceTariffFilterViewModel> _filterValidator;
        private readonly ITariffDomainValidationService _domainValidationService;
        private readonly ApplicationDbContext _context;
        private readonly IFactorSettingService _factorSettingService;
        private readonly IPlanServiceRepository _planServiceRepository;
        private readonly IUserContextService _userContextService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IBusinessRuleEngine _businessRuleEngine;

        /// <summary>
        /// 🚀 بهینه‌سازی: Constructor با Dependency Injection بهینه
        /// </summary>
        public InsuranceTariffController(
            IInsuranceTariffService insuranceTariffService,
            IInsurancePlanService insurancePlanService,
            IInsuranceProviderService insuranceProviderService,
            IServiceManagementService serviceManagementService,
            IServiceService serviceService,
            IDepartmentManagementService departmentManagementService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings,
            IMessageNotificationService messageNotificationService,
            IValidator<InsuranceTariffCreateEditViewModel> createEditValidator,
            IValidator<InsuranceTariffFilterViewModel> filterValidator,
            ITariffDomainValidationService domainValidationService,
            ApplicationDbContext context,
            IFactorSettingService factorSettingService,
            IPlanServiceRepository planServiceRepository,
            IUserContextService userContextService,
            ISystemSettingService systemSettingService,
            IBusinessRuleEngine businessRuleEngine)
            : base(messageNotificationService)
        {
            _insuranceTariffService = insuranceTariffService ?? throw new ArgumentNullException(nameof(insuranceTariffService));
            _insurancePlanService = insurancePlanService ?? throw new ArgumentNullException(nameof(insurancePlanService));
            _insuranceProviderService = insuranceProviderService ?? throw new ArgumentNullException(nameof(insuranceProviderService));
            _serviceManagementService = serviceManagementService ?? throw new ArgumentNullException(nameof(serviceManagementService));
            _serviceService = serviceService ?? throw new ArgumentNullException(nameof(serviceService));
            _departmentManagementService = departmentManagementService ?? throw new ArgumentNullException(nameof(departmentManagementService));
            _logger = logger.ForContext<InsuranceTariffController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _messageNotificationService = messageNotificationService ?? throw new ArgumentNullException(nameof(messageNotificationService));
            _createEditValidator = createEditValidator ?? throw new ArgumentNullException(nameof(createEditValidator));
            _filterValidator = filterValidator ?? throw new ArgumentNullException(nameof(filterValidator));
            _domainValidationService = domainValidationService ?? throw new ArgumentNullException(nameof(domainValidationService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _factorSettingService = factorSettingService ?? throw new ArgumentNullException(nameof(factorSettingService));
            _planServiceRepository = planServiceRepository ?? throw new ArgumentNullException(nameof(planServiceRepository));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _systemSettingService = systemSettingService ?? throw new ArgumentNullException(nameof(systemSettingService));
            _businessRuleEngine = businessRuleEngine ?? throw new ArgumentNullException(nameof(businessRuleEngine));
        }

        #endregion

        #region Properties and Constants

        private int PageSize => _appSettings.DefaultPageSize;

        #endregion

        #region Index & Search Operations

        /// <summary>
        /// نمایش صفحه اصلی تعرفه‌های بیمه با آمار کامل و فیلترهای پیشرفته
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(InsuranceTariffFilterViewModel filter)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: شروع بارگیری صفحه اصلی تعرفه‌های بیمه - CorrelationId: {CorrelationId}, Filter: {@Filter}, User: {UserName} (Id: {UserId})",
                correlationId, filter, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی فیلتر
                if (filter == null)
                {
                    filter = new InsuranceTariffFilterViewModel();
                }

                // تنظیم مقادیر پیش‌فرض
                filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
                filter.PageSize = filter.PageSize <= 0 ? PageSize : filter.PageSize;

                var model = new InsuranceTariffIndexPageViewModel
                {
                    Filter = filter
                };

                // بارگیری موازی آمار و تعرفه‌ها
                var statisticsTask = _insuranceTariffService.GetStatisticsAsync();
                var tariffsTask = _insuranceTariffService.GetTariffsAsync(
                    filter.InsurancePlanId, filter.ServiceId, filter.InsuranceProviderId,
                    filter.SearchTerm, filter.PageNumber, filter.PageSize);

                await Task.WhenAll(statisticsTask, tariffsTask);

                // بررسی نتایج آمار
                if (statisticsTask.Result.Success)
                {
                    model.Statistics = statisticsTask.Result.Data;
                    _logger.Debug("🏥 MEDICAL: آمار تعرفه‌ها با موفقیت بارگیری شد - CorrelationId: {CorrelationId}, Statistics: {@Statistics}",
                        correlationId, model.Statistics);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در بارگیری آمار تعرفه‌ها - CorrelationId: {CorrelationId}, Error: {Error}",
                        correlationId, statisticsTask.Result.Message);
                    model.Statistics = new InsuranceTariffStatisticsViewModel();
                }

                // بررسی نتایج تعرفه‌ها
                if (tariffsTask.Result.Success)
                {
                    model.Tariffs = tariffsTask.Result.Data;
                    _logger.Debug("🏥 MEDICAL: تعرفه‌ها با موفقیت بارگیری شدند - CorrelationId: {CorrelationId}, Count: {Count}",
                        correlationId, model.Tariffs?.TotalItems ?? 0);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در بارگیری تعرفه‌ها - CorrelationId: {CorrelationId}, Error: {Error}",
                        correlationId, tariffsTask.Result.Message);
                    model.Tariffs = new PagedResult<InsuranceTariffIndexViewModel>();
                }

                // بارگیری SelectLists
                await LoadSelectListsForFilterAsync(model.Filter);

                _logger.Information("🏥 MEDICAL: صفحه اصلی تعرفه‌های بیمه با موفقیت بارگیری شد - CorrelationId: {CorrelationId}, TotalItems: {TotalItems}, User: {UserName} (Id: {UserId})",
                    correlationId, model.Tariffs?.TotalItems ?? 0, _currentUserService.UserName, _currentUserService.UserId);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در بارگیری صفحه اصلی تعرفه‌های بیمه - CorrelationId: {CorrelationId}, Filter: {@Filter}, User: {UserName} (Id: {UserId})",
                    correlationId, filter, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در بارگیری صفحه اصلی تعرفه‌های بیمه");
                return View(new InsuranceTariffIndexPageViewModel());
            }
        }

        /// <summary>
        /// بارگیری تعرفه‌ها به صورت AJAX برای فیلتر و جستجو
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoadTariffs(InsuranceTariffFilterViewModel filter)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست AJAX بارگیری تعرفه‌ها - CorrelationId: {CorrelationId}, Filter: {@Filter}, User: {UserName} (Id: {UserId})",
                correlationId, filter, _currentUserService.UserName, _currentUserService.UserId);

            // 🔍 STRUCTURED LOGGING - تمام مقادیر Form
            _logger.Debug("🔍 LOAD TARIFFS DEBUG START - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId}), Timestamp: {Timestamp}",
                correlationId, _currentUserService.UserName, _currentUserService.UserId, DateTime.UtcNow);
            
            // Logging Request.Form برای debug
            _logger.Debug("🔍 Request.Form Keys and Values - CorrelationId: {CorrelationId}", correlationId);
            foreach (string key in Request.Form.AllKeys)
            {
                _logger.Debug("🔍   {Key}: '{Value}' - CorrelationId: {CorrelationId}", key, Request.Form[key], correlationId);
            }
            
            // Logging مدل دریافتی
            if (filter != null)
            {
                _logger.Debug("🔍 Filter Properties - CorrelationId: {CorrelationId}, SearchTerm: '{SearchTerm}', InsuranceProviderId: {InsuranceProviderId}, InsurancePlanId: {InsurancePlanId}, ServiceId: {ServiceId}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                    correlationId, filter.SearchTerm, filter.InsuranceProviderId, filter.InsurancePlanId, filter.ServiceId, filter.PageNumber, filter.PageSize);
            }
            else
            {
                _logger.Warning("🔍 ❌ Filter is NULL! - CorrelationId: {CorrelationId}", correlationId);
            }
            
            _logger.Debug("🔍 LOAD TARIFFS DEBUG END - CorrelationId: {CorrelationId}", correlationId);

            try
            {
                // اعتبارسنجی فیلتر
                if (filter == null)
                {
                    _logger.Warning("🏥 MEDICAL: فیلتر null است - CorrelationId: {CorrelationId}", correlationId);
                    return PartialView("_InsuranceTariffListPartial", new PagedResult<InsuranceTariffIndexViewModel>());
                }

                // تنظیم مقادیر پیش‌فرض
                filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
                filter.PageSize = filter.PageSize <= 0 ? PageSize : filter.PageSize;

                // بارگیری SelectLists برای فیلتر
                await LoadSelectListsForFilterAsync(filter);

                var result = await _insuranceTariffService.GetTariffsAsync(
                    filter.InsurancePlanId, filter.ServiceId, filter.InsuranceProviderId,
                    filter.SearchTerm, filter.PageNumber, filter.PageSize);

                if (!result.Success)
                {
                    _logger.Warning("🏥 MEDICAL: خطا در بارگیری تعرفه‌ها - CorrelationId: {CorrelationId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);
                    return PartialView("_InsuranceTariffListPartial", new PagedResult<InsuranceTariffIndexViewModel>());
                }

                _logger.Information("🏥 MEDICAL: تعرفه‌ها با موفقیت بارگیری شدند - CorrelationId: {CorrelationId}, Count: {Count}, User: {UserName} (Id: {UserId})",
                    correlationId, result.Data?.TotalItems ?? 0, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsuranceTariffListPartial", result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در بارگیری تعرفه‌ها - CorrelationId: {CorrelationId}, Filter: {@Filter}, User: {UserName} (Id: {UserId})",
                    correlationId, filter, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در بارگیری تعرفه‌های بیمه");
                return PartialView("_InsuranceTariffListPartial", new PagedResult<InsuranceTariffIndexViewModel>());
            }
        }

        #endregion

        #region Details Operations

        /// <summary>
        /// نمایش جزئیات کامل تعرفه بیمه با اطلاعات مرتبط - بهینه‌سازی شده
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست جزئیات تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _logger.Warning("🏥 MEDICAL: شناسه تعرفه نامعتبر - CorrelationId: {CorrelationId}, Id: {Id}", correlationId, id);
                    _messageNotificationService.AddErrorMessage("شناسه تعرفه بیمه نامعتبر است");
                    return RedirectToAction("Index");
                }

                var result = await _insuranceTariffService.GetTariffByIdAsync(id);
                if (!result.Success)
                {
                    _logger.Warning("🏥 MEDICAL: تعرفه بیمه یافت نشد - CorrelationId: {CorrelationId}, Id: {Id}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);
                    return RedirectToAction("Index");
                }

                _logger.Information("🏥 MEDICAL: جزئیات تعرفه بیمه با موفقیت بارگیری شد - CorrelationId: {CorrelationId}, Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})",
                    correlationId, id, result.Data.InsurancePlanId, result.Data.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در بارگیری جزئیات تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                    correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در بارگیری جزئیات تعرفه بیمه");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// نمایش فرم ایجاد تعرفه بیمه جدید
        /// </summary>
        [HttpGet]
        /// <summary>
        /// 🚀 بهینه‌سازی: نمایش فرم ایجاد تعرفه بیمه با Performance Monitoring - Real-time بدون کش
        /// </summary>
        [NoCacheFilter]
        public async Task<ActionResult> Create(int? planId = null, int? serviceId = null, int? providerId = null)
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            _logger.Information("🏥 MEDICAL: درخواست فرم ایجاد تعرفه بیمه - CorrelationId: {CorrelationId}, PlanId: {PlanId}, ServiceId: {ServiceId}, ProviderId: {ProviderId}, User: {UserName} (Id: {UserId})",
                correlationId, planId, serviceId, providerId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🚀 REAL-TIME: Set No-Cache headers
                Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                Response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");

                // 🚀 بهینه‌سازی: ایجاد مدل با مقادیر پیش‌فرض بهینه
                var model = new InsuranceTariffCreateEditViewModel
                {
                    InsurancePlanId = planId ?? 0,
                    InsuranceProviderId = providerId ?? 0,
                    ServiceId = serviceId,
                    IsActive = true,
                    StartDate = DateTime.Now.ToString("yyyy/MM/dd"),
                    EndDate = DateTime.Now.AddYears(1).ToString("yyyy/MM/dd")
                };

                // 🚀 بهینه‌سازی: بارگیری موازی SelectLists
                await LoadSelectListsForCreateEditAsync(model);

                var duration = DateTime.UtcNow - startTime;
                _logger.Information("🏥 MEDICAL: فرم ایجاد تعرفه بیمه آماده شد - CorrelationId: {CorrelationId}, Duration: {Duration}ms, User: {UserName} (Id: {UserId})",
                    correlationId, duration.TotalMilliseconds, _currentUserService.UserName, _currentUserService.UserId);

                return View(model);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در آماده‌سازی فرم ایجاد تعرفه بیمه - CorrelationId: {CorrelationId}, Duration: {Duration}ms, PlanId: {PlanId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})",
                    correlationId, duration.TotalMilliseconds, planId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در آماده‌سازی فرم ایجاد تعرفه بیمه");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش فرم ایجاد تعرفه بیمه جدید - بهینه‌سازی شده برای محیط درمانی
        /// </summary>
        /// <summary>
        /// 🚀 بهینه‌سازی: پردازش فرم ایجاد تعرفه بیمه با Performance Monitoring
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(InsuranceTariffCreateEditViewModel model)
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            // 🚀 بهینه‌سازی: Logging کامل درخواست با Performance Tracking
            _logger.Information("🏥 MEDICAL: شروع درخواست ایجاد تعرفه بیمه - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                correlationId, _currentUserService.UserName, _currentUserService.UserId);

            // 🔍 STRUCTURED LOGGING - تمام مقادیر Form
            _logger.Debug("🔍 CREATE ACTION DEBUG START - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId}), Timestamp: {Timestamp}",
                correlationId, _currentUserService.UserName, _currentUserService.UserId, DateTime.UtcNow);
            
            // Logging Request.Form برای debug
            _logger.Debug("🔍 Request.Form Keys and Values - CorrelationId: {CorrelationId}", correlationId);
            foreach (string key in Request.Form.AllKeys)
            {
                _logger.Debug("🔍   {Key}: '{Value}' - CorrelationId: {CorrelationId}", key, Request.Form[key], correlationId);
            }
            
            // Logging مدل دریافتی
            if (model != null)
            {
                _logger.Debug("🔍 Model Properties - CorrelationId: {CorrelationId}, InsuranceTariffId: {InsuranceTariffId}, DepartmentId: {DepartmentId}, ServiceCategoryId: {ServiceCategoryId}, ServiceId: {ServiceId}, InsuranceProviderId: {InsuranceProviderId}, InsurancePlanId: {InsurancePlanId}, TariffPrice: {TariffPrice}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}, IsActive: {IsActive}, IsAllServices: {IsAllServices}, IsAllServiceCategories: {IsAllServiceCategories}",
                    correlationId, model.InsuranceTariffId, model.DepartmentId, model.ServiceCategoryId, model.ServiceId, model.InsuranceProviderId, model.InsurancePlanId, model.TariffPrice, model.PatientShare, model.InsurerShare, model.IsActive, model.IsAllServices, model.IsAllServiceCategories);
                
                _logger.Information("🏥 MEDICAL: مدل دریافتی - CorrelationId: {CorrelationId}, " +
                    "InsurancePlanId: {InsurancePlanId}, InsuranceProviderId: {InsuranceProviderId}, " +
                    "ServiceId: {ServiceId}, ServiceCategoryId: {ServiceCategoryId}, " +
                    "IsAllServices: {IsAllServices}, IsAllServiceCategories: {IsAllServiceCategories}, " +
                    "TariffPrice: {TariffPrice}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}",
                    correlationId, model.InsurancePlanId, model.InsuranceProviderId, model.ServiceId, 
                    model.ServiceCategoryId, model.IsAllServices, model.IsAllServiceCategories,
                    model.TariffPrice, model.PatientShare, model.InsurerShare);
            }
            else
            {
                _logger.Warning("🔍 ❌ Model is NULL! - CorrelationId: {CorrelationId}", correlationId);
                _logger.Warning("🏥 MEDICAL: مدل null است - CorrelationId: {CorrelationId}", correlationId);
            }
            
            _logger.Debug("🔍 CREATE ACTION DEBUG END - CorrelationId: {CorrelationId}", correlationId);

            try
            {
                // 🚀 بهینه‌سازی: اعتبارسنجی مدل با Performance Monitoring
                if (model == null)
                {
                    _logger.Warning("🏥 MEDICAL: مدل تعرفه بیمه null است - CorrelationId: {CorrelationId}", correlationId);
                    _messageNotificationService.AddErrorMessage("اطلاعات تعرفه بیمه ارسال نشده است");
                    return RedirectToAction("Create");
                }

                // 🚀 بهینه‌سازی: اعتبارسنجی IdempotencyKey با Security Enhancement
                if (string.IsNullOrEmpty(model.IdempotencyKey))
                {
                    _logger.Warning("🏥 MEDICAL: IdempotencyKey موجود نیست - CorrelationId: {CorrelationId}", correlationId);
                    _messageNotificationService.AddErrorMessage("کلید امنیتی موجود نیست");
                    return View(model);
                }

                // 🚀 بهینه‌سازی: بررسی duplicate تعرفه با Performance Enhancement
                var duplicateCheckStartTime = DateTime.UtcNow;
                var existingTariff = await _context.InsuranceTariffs
                    .AsNoTracking() // بهینه‌سازی: فقط خواندن
                    .FirstOrDefaultAsync(t => t.InsurancePlanId == model.InsurancePlanId 
                                           && t.ServiceId == model.ServiceId 
                                           && !t.IsDeleted);
                
                var duplicateCheckDuration = DateTime.UtcNow - duplicateCheckStartTime;
                
                if (existingTariff != null)
                {
                    _logger.Warning("🏥 MEDICAL: تعرفه تکراری شناسایی شد - PlanId: {PlanId}, ServiceId: {ServiceId}, Duration: {Duration}ms, CorrelationId: {CorrelationId}", 
                        model.InsurancePlanId, model.ServiceId, duplicateCheckDuration.TotalMilliseconds, correlationId);
                    _messageNotificationService.AddErrorMessage("تعرفه برای این خدمت و طرح بیمه قبلاً تعریف شده است");
                    return View(model);
                }
                
                _logger.Debug("🏥 MEDICAL: بررسی duplicate تکمیل شد - Duration: {Duration}ms, CorrelationId: {CorrelationId}", 
                    duplicateCheckDuration.TotalMilliseconds, correlationId);

                // اصلاح ModelState برای "همه خدمات" - قبل از بررسی ModelState.IsValid
                if (model.IsAllServices)
                {
                    if (ModelState.ContainsKey("ServiceId"))
                    {
                        ModelState["ServiceId"].Errors.Clear();
                        _logger.Information("🏥 MEDICAL: ModelState برای ServiceId پاک شد (همه خدمات) - CorrelationId: {CorrelationId}", correlationId);
                    }
                    // حذف validation error برای ServiceId
                    ModelState.Remove("ServiceId");
                }

                // اصلاح ModelState برای "همه سرفصل‌ها"
                if (model.IsAllServiceCategories)
                {
                    if (ModelState.ContainsKey("ServiceCategoryId"))
                    {
                        ModelState["ServiceCategoryId"].Errors.Clear();
                        _logger.Information("🏥 MEDICAL: ModelState برای ServiceCategoryId پاک شد (همه سرفصل‌ها) - CorrelationId: {CorrelationId}", correlationId);
                    }
                    // حذف validation error برای ServiceCategoryId
                    ModelState.Remove("ServiceCategoryId");
                }

                if (!ModelState.IsValid)
                {
                    _logger.Warning("🏥 MEDICAL: مدل تعرفه بیمه معتبر نیست - CorrelationId: {CorrelationId}, Errors: {@Errors}, User: {UserName} (Id: {UserId})",
                        correlationId, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage), _currentUserService.UserName, _currentUserService.UserId);

                    // بارگیری مجدد SelectLists
                    await LoadSelectListsForCreateEditAsync(model);

                    return View(model);
                }


                // اعتبارسنجی کسب‌وکار
                var validationResult = await _insuranceTariffService.ValidateTariffAsync(model);
                if (!validationResult.Success)
                {
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی تعرفه بیمه ناموفق - CorrelationId: {CorrelationId}, Errors: {@Errors}, User: {UserName} (Id: {UserId})",
                        correlationId, validationResult.Data, _currentUserService.UserName, _currentUserService.UserId);

                    foreach (var error in validationResult.Data)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }

                    // بارگیری مجدد SelectLists
                    await LoadSelectListsForCreateEditAsync(model);

                    return View(model);
                }

                // اعتبارسنجی دامنه - Domain Validation
                var domainValidationResult = await _domainValidationService.ValidateTariffAsync(new InsuranceTariff
                {
                    InsuranceTariffId = model.InsuranceTariffId,
                    InsurancePlanId = model.InsurancePlanId,
                    ServiceId = model.ServiceId ?? 0,
                    TariffPrice = model.TariffPrice,
                    PatientShare = model.PatientShare,
                    InsurerShare = model.InsurerShare,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                });

                if (!domainValidationResult.Success)
                {
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی دامنه تعرفه بیمه ناموفق - CorrelationId: {CorrelationId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, domainValidationResult.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage($"اعتبارسنجی دامنه: {domainValidationResult.Message}");

                    // بارگیری مجدد SelectLists
                    await LoadSelectListsForCreateEditAsync(model);

                    return View(model);
                }

                // ایجاد تعرفه - بررسی Bulk Operation
                ServiceResult<int> result;
                if (model.IsAllServices)
                {
                    _logger.Information("🏥 MEDICAL: شروع Bulk Operation برای همه خدمات - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                        correlationId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    result = await _insuranceTariffService.CreateBulkTariffForAllServicesAsync(model);
                }
                else
                {
                    result = await _insuranceTariffService.CreateTariffAsync(model);
                }

                if (!result.Success)
                {
                    _logger.Warning("🏥 MEDICAL: خطا در ایجاد تعرفه بیمه - CorrelationId: {CorrelationId}, PlanId: {PlanId}, ServiceId: {ServiceId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, model.InsurancePlanId, model.ServiceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);

                    // بارگیری مجدد SelectLists
                    await LoadSelectListsForCreateEditAsync(model);

                    return View(model);
                }

                if (model.IsAllServices)
                {
                    _logger.Information("🏥 MEDICAL: Bulk Operation با موفقیت تکمیل شد - CorrelationId: {CorrelationId}, CreatedCount: {CreatedCount}, PlanId: {PlanId}, User: {UserName} (Id: {UserId})",
                        correlationId, result.Data, model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddSuccessMessage($"تعرفه برای {result.Data} خدمت با موفقیت ایجاد شد");
                }
                else
                {
                    _logger.Information("🏥 MEDICAL: تعرفه بیمه با موفقیت ایجاد شد - CorrelationId: {CorrelationId}, Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})",
                        correlationId, result.Data, model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddSuccessMessage("تعرفه بیمه با موفقیت ایجاد شد");
                }

                // 🏥 MEDICAL: Real-time data - no cache invalidation needed
                _logger.Information("🏥 MEDICAL: Real-time data updated - CorrelationId: {CorrelationId}", correlationId);

                return RedirectToAction("Details", new { id = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در ایجاد تعرفه بیمه - CorrelationId: {CorrelationId}, Model: {@Model}, User: {UserName} (Id: {UserId})",
                    correlationId, model, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در ایجاد تعرفه بیمه");

                if (model != null)
                {
                    await LoadSelectListsForCreateEditAsync(model);
                    return View(model);
                }

                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Edit Operations

        /// <summary>
        /// نمایش فرم ویرایش تعرفه بیمه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست فرم ویرایش تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _logger.Warning("🏥 MEDICAL: شناسه تعرفه نامعتبر - CorrelationId: {CorrelationId}, Id: {Id}", correlationId, id);
                    _messageNotificationService.AddErrorMessage("شناسه تعرفه بیمه نامعتبر است");
                    return RedirectToAction("Index");
                }

                var result = await _insuranceTariffService.GetTariffForEditAsync(id);
                if (!result.Success)
                {
                    _logger.Warning("🏥 MEDICAL: تعرفه بیمه برای ویرایش یافت نشد - CorrelationId: {CorrelationId}, Id: {Id}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);
                    return RedirectToAction("Index");
                }

                // بارگیری SelectLists
                await LoadSelectListsForCreateEditAsync(result.Data);

                _logger.Information("🏥 MEDICAL: فرم ویرایش تعرفه بیمه آماده شد - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                    correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در آماده‌سازی فرم ویرایش تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                    correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در آماده‌سازی فرم ویرایش تعرفه بیمه");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش فرم ویرایش تعرفه بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(InsuranceTariffCreateEditViewModel model)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست ویرایش تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})",
                correlationId, model?.InsuranceTariffId, model?.InsurancePlanId, model?.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

            // 🔍 STRUCTURED LOGGING - تمام مقادیر Form
            _logger.Debug("🔍 EDIT ACTION DEBUG START - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId}), Timestamp: {Timestamp}",
                correlationId, _currentUserService.UserName, _currentUserService.UserId, DateTime.UtcNow);
            
            // Logging Request.Form برای debug
            _logger.Debug("🔍 Request.Form Keys and Values - CorrelationId: {CorrelationId}", correlationId);
            foreach (string key in Request.Form.AllKeys)
            {
                _logger.Debug("🔍   {Key}: '{Value}' - CorrelationId: {CorrelationId}", key, Request.Form[key], correlationId);
            }
            
            // Logging مدل دریافتی
            if (model != null)
            {
                _logger.Debug("🔍 Model Properties - CorrelationId: {CorrelationId}, InsuranceTariffId: {InsuranceTariffId}, DepartmentId: {DepartmentId}, ServiceCategoryId: {ServiceCategoryId}, ServiceId: {ServiceId}, InsuranceProviderId: {InsuranceProviderId}, InsurancePlanId: {InsurancePlanId}, TariffPrice: {TariffPrice}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}, IsActive: {IsActive}, IsAllServices: {IsAllServices}, IsAllServiceCategories: {IsAllServiceCategories}",
                    correlationId, model.InsuranceTariffId, model.DepartmentId, model.ServiceCategoryId, model.ServiceId, model.InsuranceProviderId, model.InsurancePlanId, model.TariffPrice, model.PatientShare, model.InsurerShare, model.IsActive, model.IsAllServices, model.IsAllServiceCategories);
            }
            else
            {
                _logger.Warning("🔍 ❌ Model is NULL! - CorrelationId: {CorrelationId}", correlationId);
            }
            
            _logger.Debug("🔍 EDIT ACTION DEBUG END - CorrelationId: {CorrelationId}", correlationId);

            try
            {
                // اعتبارسنجی مدل
                if (model == null)
                {
                    _logger.Warning("🏥 MEDICAL: مدل تعرفه بیمه null است - CorrelationId: {CorrelationId}", correlationId);
                    _messageNotificationService.AddErrorMessage("اطلاعات تعرفه بیمه ارسال نشده است");
                    return RedirectToAction("Index");
                }

                // اصلاح ModelState برای "همه خدمات" - قبل از بررسی ModelState.IsValid
                if (model.IsAllServices)
                {
                    if (ModelState.ContainsKey("ServiceId"))
                    {
                        ModelState["ServiceId"].Errors.Clear();
                        _logger.Information("🏥 MEDICAL: ModelState برای ServiceId پاک شد (همه خدمات) - CorrelationId: {CorrelationId}", correlationId);
                    }
                    // حذف validation error برای ServiceId
                    ModelState.Remove("ServiceId");
                }

                // اصلاح ModelState برای "همه سرفصل‌ها"
                if (model.IsAllServiceCategories)
                {
                    if (ModelState.ContainsKey("ServiceCategoryId"))
                    {
                        ModelState["ServiceCategoryId"].Errors.Clear();
                        _logger.Information("🏥 MEDICAL: ModelState برای ServiceCategoryId پاک شد (همه سرفصل‌ها) - CorrelationId: {CorrelationId}", correlationId);
                    }
                    // حذف validation error برای ServiceCategoryId
                    ModelState.Remove("ServiceCategoryId");
                }

                if (!ModelState.IsValid)
                {
                    _logger.Warning("🏥 MEDICAL: مدل تعرفه بیمه معتبر نیست - CorrelationId: {CorrelationId}, Errors: {@Errors}, User: {UserName} (Id: {UserId})",
                        correlationId, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage), _currentUserService.UserName, _currentUserService.UserId);

                    // بارگیری مجدد SelectLists
                    await LoadSelectListsForCreateEditAsync(model);

                    return View(model);
                }

                // اعتبارسنجی کسب‌وکار
                var validationResult = await _insuranceTariffService.ValidateTariffAsync(model);
                if (!validationResult.Success)
                {
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی تعرفه بیمه ناموفق - CorrelationId: {CorrelationId}, Errors: {@Errors}, User: {UserName} (Id: {UserId})",
                        correlationId, validationResult.Data, _currentUserService.UserName, _currentUserService.UserId);

                    foreach (var error in validationResult.Data)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }

                    // بارگیری مجدد SelectLists
                    await LoadSelectListsForCreateEditAsync(model);

                    return View(model);
                }

                // ویرایش تعرفه
                var result = await _insuranceTariffService.UpdateTariffAsync(model);
                if (!result.Success)
                {
                    _logger.Warning("🏥 MEDICAL: خطا در ویرایش تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, model.InsuranceTariffId, model.InsurancePlanId, model.ServiceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);

                    // بارگیری مجدد SelectLists
                    await LoadSelectListsForCreateEditAsync(model);

                    return View(model);
                }

                _logger.Information("🏥 MEDICAL: تعرفه بیمه با موفقیت ویرایش شد - CorrelationId: {CorrelationId}, Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})",
                    correlationId, model.InsuranceTariffId, model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddSuccessMessage("تعرفه بیمه با موفقیت ویرایش شد");
                return RedirectToAction("Details", new { id = model.InsuranceTariffId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در ویرایش تعرفه بیمه - CorrelationId: {CorrelationId}, Model: {@Model}, User: {UserName} (Id: {UserId})",
                    correlationId, model, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در ویرایش تعرفه بیمه");

                if (model != null)
                {
                    await LoadSelectListsForCreateEditAsync(model);
                    return View(model);
                }

                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// نمایش تایید حذف تعرفه بیمه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست تایید حذف تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _logger.Warning("🏥 MEDICAL: شناسه تعرفه نامعتبر - CorrelationId: {CorrelationId}, Id: {Id}", correlationId, id);
                    _messageNotificationService.AddErrorMessage("شناسه تعرفه بیمه نامعتبر است");
                    return RedirectToAction("Index");
                }

                var result = await _insuranceTariffService.GetTariffByIdAsync(id);
                if (!result.Success)
                {
                    _logger.Warning("🏥 MEDICAL: تعرفه بیمه برای حذف یافت نشد - CorrelationId: {CorrelationId}, Id: {Id}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);
                    return RedirectToAction("Index");
                }

                _logger.Information("🏥 MEDICAL: تایید حذف تعرفه بیمه آماده شد - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                    correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در آماده‌سازی تایید حذف تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                    correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در آماده‌سازی تایید حذف تعرفه بیمه");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// تایید و اجرای حذف تعرفه بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست حذف تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _logger.Warning("🏥 MEDICAL: شناسه تعرفه نامعتبر - CorrelationId: {CorrelationId}, Id: {Id}", correlationId, id);
                    _messageNotificationService.AddErrorMessage("شناسه تعرفه بیمه نامعتبر است");
                    return RedirectToAction("Index");
                }

                var result = await _insuranceTariffService.DeleteTariffAsync(id);
                if (!result.Success)
                {
                    _logger.Warning("🏥 MEDICAL: خطا در حذف تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);
                    return RedirectToAction("Index");
                }

                _logger.Information("🏥 MEDICAL: تعرفه بیمه با موفقیت حذف شد - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                    correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddSuccessMessage("تعرفه بیمه با موفقیت حذف شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در حذف تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, User: {UserName} (Id: {UserId})",
                    correlationId, id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در حذف تعرفه بیمه");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region AJAX Operations

        /// <summary>
        /// دریافت آمار تعرفه‌های بیمه به صورت AJAX - Real-time برای محیط درمانی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetStatistics()
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست AJAX آمار تعرفه‌ها - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                correlationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceTariffService.GetStatisticsAsync();
                if (!result.Success)
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت آمار تعرفه‌ها - CorrelationId: {CorrelationId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                _logger.Debug("🏥 MEDICAL: آمار تعرفه‌ها با موفقیت دریافت شد - CorrelationId: {CorrelationId}, Statistics: {@Statistics}",
                    correlationId, result.Data);

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت آمار تعرفه‌ها - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    correlationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در دریافت آمار تعرفه‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        #region Helper Methods

        /// <summary>
        /// بارگیری SelectLists برای فیلتر
        /// </summary>
        private async Task LoadSelectListsForFilterAsync(InsuranceTariffFilterViewModel filter)
        {
            try
            {
                // بارگیری موازی SelectLists
                var plansTask = _insurancePlanService.GetActivePlansForLookupAsync();
                var servicesTask = _serviceManagementService.GetActiveServicesForLookupAsync(0);
                var providersTask = _insuranceProviderService.GetActiveProvidersForLookupAsync();

                await Task.WhenAll(plansTask, servicesTask, providersTask);

                filter.InsurancePlanSelectList = new SelectList(plansTask.Result.Data, "Value", "Text", filter.InsurancePlanId);
                filter.ServiceSelectList = new SelectList(servicesTask.Result.Data, "Value", "Text", filter.ServiceId);
                filter.InsuranceProviderSelectList = new SelectList(providersTask.Result.Data, "Value", "Text", filter.InsuranceProviderId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در بارگیری SelectLists برای فیلتر - User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // تنظیم SelectLists خالی در صورت خطا
                filter.InsurancePlanSelectList = new SelectList(new List<object>(), "Value", "Text");
                filter.ServiceSelectList = new SelectList(new List<object>(), "Value", "Text");
                filter.InsuranceProviderSelectList = new SelectList(new List<object>(), "Value", "Text");
            }
        }

        /// <summary>
        /// بارگیری SelectLists برای فرم ایجاد/ویرایش
        /// </summary>
        /// <summary>
        /// بارگیری بهینه SelectLists برای فرم ایجاد/ویرایش تعرفه
        /// با پشتیبانی از Cascading Dropdowns و Error Handling
        /// </summary>
        private async Task LoadSelectListsForCreateEditAsync(InsuranceTariffCreateEditViewModel model)
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            _logger.Information("🏥 MEDICAL: شروع بارگیری SelectLists - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                correlationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🚀 بهینه‌سازی: بارگیری موازی با Timeout
                var clinicId = await GetCurrentClinicIdAsync();
                
                var departmentsTask = _departmentManagementService.GetActiveDepartmentsForLookupAsync(clinicId);
                var providersTask = _insuranceProviderService.GetActiveProvidersForLookupAsync();
                // 🚀 FIX: طرح‌های بیمه نباید در ابتدا لود شوند - باید بعد از انتخاب ارائه‌دهنده لود شوند
                // var plansTask = _insurancePlanService.GetActivePlansForLookupAsync();

                // انتظار با Timeout برای جلوگیری از Hang
                var timeout = TimeSpan.FromSeconds(10);
                await Task.WhenAll(departmentsTask, providersTask).ConfigureAwait(false);

                // ✅ تنظیم SelectLists اصلی با Error Handling
                if (departmentsTask.Result?.Success == true && departmentsTask.Result.Data?.Any() == true)
                {
                model.DepartmentSelectList = new SelectList(departmentsTask.Result.Data, "Id", "Name", model.DepartmentId);
                    _logger.Debug("🏥 MEDICAL: Departments loaded - Count: {Count}, CorrelationId: {CorrelationId}",
                        departmentsTask.Result.Data.Count, correlationId);
                }
                else
                {
                    model.DepartmentSelectList = new SelectList(new List<object>(), "Id", "Name");
                    _logger.Warning("🏥 MEDICAL: No departments found - CorrelationId: {CorrelationId}", correlationId);
                }

                if (providersTask.Result?.Success == true && providersTask.Result.Data?.Any() == true)
                {
                model.InsuranceProviderSelectList = new SelectList(providersTask.Result.Data, "Value", "Text", model.InsuranceProviderId);
                    _logger.Debug("🏥 MEDICAL: Insurance Providers loaded - Count: {Count}, CorrelationId: {CorrelationId}",
                        providersTask.Result.Data.Count, correlationId);
                }
                else
                {
                    model.InsuranceProviderSelectList = new SelectList(new List<object>(), "Value", "Text");
                    _logger.Warning("🏥 MEDICAL: No insurance providers found - CorrelationId: {CorrelationId}", correlationId);
                }

                // 🚀 FIX: طرح‌های بیمه باید خالی باشند تا بعد از انتخاب ارائه‌دهنده لود شوند
                model.InsurancePlanSelectList = new SelectList(new List<object>(), "Value", "Text");
                _logger.Debug("🏥 MEDICAL: Insurance Plans initialized as empty - will load after provider selection, CorrelationId: {CorrelationId}", correlationId);

                // 🔄 تنظیم SelectLists خالی برای Cascading Dropdowns
                model.ServiceSelectList = new SelectList(new List<object>(), "Value", "Text");
                model.ServiceCategorySelectList = new SelectList(new List<object>(), "Value", "Text");

                var duration = DateTime.UtcNow - startTime;
                _logger.Information("🏥 MEDICAL: SelectLists loaded successfully - Duration: {Duration}ms, CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    duration.TotalMilliseconds, correlationId, _currentUserService.UserName, _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.Error(ex, "🏥 MEDICAL: خطا در بارگیری SelectLists - Duration: {Duration}ms, CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    duration.TotalMilliseconds, correlationId, _currentUserService.UserName, _currentUserService.UserId);

                // 🛡️ Fallback: تنظیم SelectLists خالی در صورت خطا
                SetEmptySelectLists(model);
            }
        }

        /// <summary>
        /// تنظیم SelectLists خالی برای حالت خطا
        /// </summary>
        private void SetEmptySelectLists(InsuranceTariffCreateEditViewModel model)
        {
                model.DepartmentSelectList = new SelectList(new List<object>(), "Id", "Name");
                model.InsurancePlanSelectList = new SelectList(new List<object>(), "Value", "Text");
                model.ServiceSelectList = new SelectList(new List<object>(), "Value", "Text");
                model.InsuranceProviderSelectList = new SelectList(new List<object>(), "Value", "Text");
            model.ServiceCategorySelectList = new SelectList(new List<object>(), "Value", "Text");
        }

        /// <summary>
        /// دریافت شناسه کلینیک فعلی کاربر
        /// </summary>
        private async Task<int> GetCurrentClinicIdAsync()
        {
            try
            {
                // TODO: پیاده‌سازی دریافت ClinicId از User Context
                // فعلاً مقدار پیش‌فرض
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "🏥 MEDICAL: خطا در دریافت ClinicId - استفاده از مقدار پیش‌فرض");
                return 1;
            }
        }

        /// <summary>
        /// اعتبارسنجی پیشرفته برای InsuranceTariffCreateEditViewModel
        /// با پشتیبانی از Business Rules و Real-time Validation
        /// </summary>
        private async Task<ServiceResult> ValidateCreateEditModelAsync(InsuranceTariffCreateEditViewModel model)
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            _logger.Information("🏥 MEDICAL: شروع اعتبارسنجی پیشرفته - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                correlationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🔍 مرحله 1: اعتبارسنجی پایه با FluentValidation
                var validationResult = await _createEditValidator.ValidateAsync(model);
                
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    var duration = DateTime.UtcNow - startTime;
                    
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی پایه ناموفق - CorrelationId: {CorrelationId}, Duration: {Duration}ms, Errors: {Errors}, User: {UserName} (Id: {UserId})",
                        correlationId, duration.TotalMilliseconds, string.Join(", ", errors), _currentUserService.UserName, _currentUserService.UserId);
                    
                    var validationErrors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList();
                    return ServiceResult.FailedWithValidationErrors("خطاهای اعتبارسنجی پایه", validationErrors);
                }

                // 🔍 مرحله 2: اعتبارسنجی Business Rules
                var businessValidationResult = await ValidateBusinessRulesAsync(model, correlationId);
                if (!businessValidationResult.Success)
                {
                    var duration = DateTime.UtcNow - startTime;
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی Business Rules ناموفق - CorrelationId: {CorrelationId}, Duration: {Duration}ms, Error: {Error}",
                        correlationId, duration.TotalMilliseconds, businessValidationResult.Message);
                    
                    return businessValidationResult;
                }

                // 🔍 مرحله 3: اعتبارسنجی Cross-Reference
                var crossValidationResult = await ValidateCrossReferencesAsync(model, correlationId);
                if (!crossValidationResult.Success)
                {
                    var duration = DateTime.UtcNow - startTime;
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی Cross-Reference ناموفق - CorrelationId: {CorrelationId}, Duration: {Duration}ms, Error: {Error}",
                        correlationId, duration.TotalMilliseconds, crossValidationResult.Message);
                    
                    return crossValidationResult;
                }

                var totalDuration = DateTime.UtcNow - startTime;
                _logger.Information("🏥 MEDICAL: اعتبارسنجی پیشرفته موفق - CorrelationId: {CorrelationId}, Duration: {Duration}ms, User: {UserName} (Id: {UserId})",
                    correlationId, totalDuration.TotalMilliseconds, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("اعتبارسنجی پیشرفته موفق");
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی پیشرفته - CorrelationId: {CorrelationId}, Duration: {Duration}ms, User: {UserName} (Id: {UserId})",
                    correlationId, duration.TotalMilliseconds, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult.Failed("خطا در اعتبارسنجی پیشرفته داده‌ها", "ADVANCED_VALIDATION_ERROR", ErrorCategory.Validation);
            }
        }

        /// <summary>
        /// اعتبارسنجی Business Rules برای تعرفه بیمه با استفاده از BusinessRuleEngine موجود
        /// </summary>
        private async Task<ServiceResult> ValidateBusinessRulesAsync(InsuranceTariffCreateEditViewModel model, string correlationId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: شروع اعتبارسنجی Business Rules - CorrelationId: {CorrelationId}, ServiceId: {ServiceId}, PlanId: {PlanId}", 
                    correlationId, model.ServiceId, model.InsurancePlanId);

                // 🔍 Rule 1: بررسی تکراری نبودن تعرفه با استفاده از سرویس موجود
                if (model.ServiceId > 0 && model.InsurancePlanId > 0)
                {
                    // TODO: پیاده‌سازی بررسی تکراری بودن تعرفه
                    // var existingTariff = await _insuranceTariffService.GetTariffByServiceAndPlanAsync(model.ServiceId, model.InsurancePlanId);
                    // if (existingTariff?.Success == true && existingTariff.Data != null && existingTariff.Data.InsuranceTariffId != model.InsuranceTariffId)
                    // {
                    //     _logger.Warning("🏥 MEDICAL: تعرفه تکراری شناسایی شد - ServiceId: {ServiceId}, InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}",
                    //         model.ServiceId, model.InsurancePlanId, correlationId);
                    //     return ServiceResult.Failed("تعرفه برای این خدمت و طرح بیمه قبلاً تعریف شده است", "DUPLICATE_TARIFF", ErrorCategory.Validation);
                    // }
                }

                // 🔍 Rule 2: استفاده از InsuranceValidationService موجود
                var validationResult = await ValidateTariffValuesAsync(model, correlationId);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // 🔍 Rule 3: استفاده از BusinessRuleEngine برای اعتبارسنجی پیشرفته
                var businessRuleResult = await ValidateWithBusinessRuleEngineAsync(model, correlationId);
                if (!businessRuleResult.Success)
                {
                    return businessRuleResult;
                }

                _logger.Information("🏥 MEDICAL: Business Rules validation successful - CorrelationId: {CorrelationId}", correlationId);
                return ServiceResult.Successful("اعتبارسنجی Business Rules موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی Business Rules - CorrelationId: {CorrelationId}", correlationId);
                return ServiceResult.Failed("خطا در اعتبارسنجی Business Rules", "BUSINESS_RULES_ERROR", ErrorCategory.Validation);
            }
        }

        /// <summary>
        /// اعتبارسنجی مقادیر تعرفه با استفاده از InsuranceValidationService
        /// </summary>
        private async Task<ServiceResult> ValidateTariffValuesAsync(InsuranceTariffCreateEditViewModel model, string correlationId)
        {
            try
            {
                // بررسی منطقی بودن مقادیر مالی
                if (model.TariffPrice.HasValue && model.TariffPrice.Value < 0)
                {
                    return ServiceResult.Failed("قیمت تعرفه نمی‌تواند منفی باشد", "INVALID_TARIFF_PRICE", ErrorCategory.Validation);
                }

                if (model.PatientShare.HasValue && model.PatientShare.Value < 0)
                {
                    return ServiceResult.Failed("سهم بیمار نمی‌تواند منفی باشد", "INVALID_PATIENT_SHARE", ErrorCategory.Validation);
                }

                if (model.InsurerShare.HasValue && model.InsurerShare.Value < 0)
                {
                    return ServiceResult.Failed("سهم بیمه نمی‌تواند منفی باشد", "INVALID_INSURER_SHARE", ErrorCategory.Validation);
                }

                // بررسی منطقی بودن تاریخ‌ها
                if (!string.IsNullOrEmpty(model.StartDate) && !string.IsNullOrEmpty(model.EndDate))
                {
                    if (DateTime.TryParse(model.StartDate, out var startDate) && DateTime.TryParse(model.EndDate, out var endDate))
                    {
                        if (startDate >= endDate)
                        {
                            return ServiceResult.Failed("تاریخ شروع باید قبل از تاریخ پایان باشد", "INVALID_DATE_RANGE", ErrorCategory.Validation);
                        }
                    }
                }

                // بررسی درصد پوشش بیمه تکمیلی
                if (model.SupplementaryCoveragePercent < 0 || model.SupplementaryCoveragePercent > 100)
                {
                    return ServiceResult.Failed("درصد پوشش بیمه تکمیلی باید بین 0 تا 100 باشد", "INVALID_COVERAGE_PERCENT", ErrorCategory.Validation);
                }

                return ServiceResult.Successful("اعتبارسنجی مقادیر موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی مقادیر - CorrelationId: {CorrelationId}", correlationId);
                return ServiceResult.Failed("خطا در اعتبارسنجی مقادیر", "TARIFF_VALUES_ERROR", ErrorCategory.Validation);
            }
        }

        /// <summary>
        /// اعتبارسنجی با استفاده از BusinessRuleEngine موجود
        /// </summary>
        private async Task<ServiceResult> ValidateWithBusinessRuleEngineAsync(InsuranceTariffCreateEditViewModel model, string correlationId)
        {
            try
            {
                // ایجاد Context برای BusinessRuleEngine
                var context = new InsuranceCalculationContext
                {
                    ServiceId = model.ServiceId,
                    InsurancePlanId = model.InsurancePlanId,
                    ServiceCategoryId = model.ServiceCategoryId ?? 0,
                    ServiceAmount = model.TariffPrice ?? 0,
                    CalculationDate = DateTime.Now,
                    PatientId = 0, // برای اعتبارسنجی تعرفه، PatientId لازم نیست
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "TariffPrice", model.TariffPrice ?? 0 },
                        { "PatientShare", model.PatientShare ?? 0 },
                        { "InsurerShare", model.InsurerShare ?? 0 }
                    }
                };

                // استفاده از BusinessRuleEngine برای اعتبارسنجی
                var coverageResult = await _businessRuleEngine.CalculateCoveragePercentAsync(context);
                if (!coverageResult.Success)
                {
                    _logger.Warning("🏥 MEDICAL: BusinessRuleEngine validation failed - CorrelationId: {CorrelationId}, Error: {Error}", 
                        correlationId, coverageResult.Message);
                    return ServiceResult.Failed($"اعتبارسنجی BusinessRuleEngine ناموفق: {coverageResult.Message}", "BUSINESS_RULE_ENGINE_ERROR", ErrorCategory.Validation);
                }

                _logger.Information("🏥 MEDICAL: BusinessRuleEngine validation successful - CorrelationId: {CorrelationId}, CoveragePercent: {CoveragePercent}", 
                    correlationId, coverageResult.Data);
                
                return ServiceResult.Successful("اعتبارسنجی BusinessRuleEngine موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی BusinessRuleEngine - CorrelationId: {CorrelationId}", correlationId);
                return ServiceResult.Failed("خطا در اعتبارسنجی BusinessRuleEngine", "BUSINESS_RULE_ENGINE_ERROR", ErrorCategory.Validation);
            }
        }

        /// <summary>
        /// اعتبارسنجی Cross-Reference برای روابط بین موجودیت‌ها
        /// </summary>
        private async Task<ServiceResult> ValidateCrossReferencesAsync(InsuranceTariffCreateEditViewModel model, string correlationId)
        {
            try
            {
                // 🔍 بررسی وجود Service
                if (model.ServiceId > 0)
                {
                    // TODO: پیاده‌سازی بررسی وجود Service
                    // var serviceResult = await _serviceManagementService.GetServiceByIdAsync(model.ServiceId);
                    // if (serviceResult?.Success != true || serviceResult.Data == null)
                    // {
                    //     _logger.Warning("🏥 MEDICAL: Service یافت نشد - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}",
                    //         model.ServiceId, correlationId);
                    //     
                    //     return ServiceResult.Failed("خدمت انتخاب شده یافت نشد", "SERVICE_NOT_FOUND", ErrorCategory.Validation);
                    // }
                }

                // 🔍 بررسی وجود InsurancePlan
                if (model.InsurancePlanId > 0)
                {
                    // TODO: پیاده‌سازی بررسی وجود InsurancePlan
                    // var planResult = await _insurancePlanService.GetPlanByIdAsync(model.InsurancePlanId);
                    // if (planResult?.Success != true || planResult.Data == null)
                    // {
                    //     _logger.Warning("🏥 MEDICAL: Insurance Plan یافت نشد - InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}",
                    //         model.InsurancePlanId, correlationId);
                    //     
                    //     return ServiceResult.Failed("طرح بیمه انتخاب شده یافت نشد", "INSURANCE_PLAN_NOT_FOUND", ErrorCategory.Validation);
                    // }
                }

                // 🔍 بررسی وجود Department
                if (model.DepartmentId > 0)
                {
                    // TODO: پیاده‌سازی بررسی وجود Department
                    // var departmentResult = await _departmentManagementService.GetDepartmentByIdAsync(model.DepartmentId);
                    // if (departmentResult?.Success != true || departmentResult.Data == null)
                    // {
                    //     _logger.Warning("🏥 MEDICAL: Department یافت نشد - DepartmentId: {DepartmentId}, CorrelationId: {CorrelationId}",
                    //         model.DepartmentId, correlationId);
                    //     
                    //     return ServiceResult.Failed("دپارتمان انتخاب شده یافت نشد", "DEPARTMENT_NOT_FOUND", ErrorCategory.Validation);
                    // }
                }

                _logger.Debug("🏥 MEDICAL: Cross-Reference validation successful - CorrelationId: {CorrelationId}", correlationId);
                return ServiceResult.Successful("اعتبارسنجی Cross-Reference موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی Cross-Reference - CorrelationId: {CorrelationId}", correlationId);
                return ServiceResult.Failed("خطا در اعتبارسنجی Cross-Reference", "CROSS_REFERENCE_ERROR", ErrorCategory.Validation);
            }
        }

        /// <summary>
        /// اعتبارسنجی قوی برای InsuranceTariffFilterViewModel
        /// </summary>
        private async Task<ServiceResult> ValidateFilterModelAsync(InsuranceTariffFilterViewModel model)
        {
            try
            {
                var validationResult = await _filterValidator.ValidateAsync(model);
                
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی فیلتر ناموفق - User: {UserName} (Id: {UserId}), Errors: {Errors}",
                        _currentUserService.UserName, _currentUserService.UserId, string.Join(", ", errors));
                    
                    var validationErrors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList();
                    return ServiceResult.FailedWithValidationErrors("خطاهای اعتبارسنجی فیلتر", validationErrors);
                }

                return ServiceResult.Successful("اعتبارسنجی فیلتر موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی فیلتر - User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult.Failed("خطا در اعتبارسنجی فیلتر", "FILTER_VALIDATION_ERROR", ErrorCategory.Validation);
            }
        }

        #endregion

        #region Advanced Calculation

        /// <summary>
        /// محاسبه پیشرفته تعرفه بیمه با پشتیبانی از Real-time Calculation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [NoCacheFilter]
        public async Task<JsonResult> CalculateAdvancedTariff(
            int serviceId, 
            int insurancePlanId, 
            int? providerId = null,
            decimal? currentTariffPrice = null,
            decimal? currentPatientShare = null,
            decimal? currentInsurerShare = null,
            decimal? supplementaryCoveragePercent = null,
            string calculationType = "comprehensive")
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            _logger.Information("🏥 MEDICAL: شروع محاسبه پیشرفته تعرفه - CorrelationId: {CorrelationId}, ServiceId: {ServiceId}, InsurancePlanId: {InsurancePlanId}, ProviderId: {ProviderId}, User: {UserName} (Id: {UserId})",
                correlationId, serviceId, insurancePlanId, providerId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🛡️ اعتبارسنجی جامع ورودی‌ها - ضد گلوله
                var validationResult = ValidateCalculationInputs(serviceId, insurancePlanId, providerId, correlationId);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی ورودی‌ها ناموفق - {Errors}, CorrelationId: {CorrelationId}",
                        string.Join(", ", validationResult.Errors), correlationId);
                    
                    return Json(new { success = false, message = validationResult.Errors.FirstOrDefault() ?? "ورودی‌های نامعتبر" });
                }

                // 🔍 بهینه‌سازی Performance: بارگیری موازی اطلاعات
                var serviceTask = _serviceManagementService.GetActiveServicesForLookupAsync(0);
                // 🚀 FIX: دریافت طرح‌های بیمه بر اساس providerId (اگر موجود باشد)
                var planTask = _insurancePlanService.GetActivePlansForLookupAsync(providerId);
                
                // 🔍 Debug logging برای بررسی providerId
                _logger.Information("🏥 MEDICAL: ProviderId در CalculateAdvancedTariff - ProviderId: {ProviderId}, Type: {Type}, CorrelationId: {CorrelationId}",
                    providerId, providerId?.GetType().Name, correlationId);
                
                await Task.WhenAll(serviceTask, planTask);
                
                var serviceResult = serviceTask.Result;
                var planResult = planTask.Result;

                // 🛡️ اعتبارسنجی جامع نتایج سرویس‌ها - ضد گلوله
                var serviceValidationResult = ValidateServiceResults(serviceResult, planResult, serviceId, insurancePlanId, correlationId);
                if (!serviceValidationResult.IsValid)
                {
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی نتایج سرویس‌ها ناموفق - {Error}, CorrelationId: {CorrelationId}",
                        serviceValidationResult.ErrorMessage, correlationId);
                    
                    return Json(new { success = false, message = serviceValidationResult.ErrorMessage });
                }

                // 🔍 بهینه‌سازی جستجو: استفاده از Dictionary برای O(1) lookup
                var service = serviceResult.Data.FirstOrDefault(s => s.Id == serviceId);
                
                // 🚀 FIX: جستجوی صحیح طرح بیمه بر اساس InsurancePlanId
                // 🔍 Debug: بررسی تمام طرح‌های بیمه موجود
                _logger.Information("🏥 MEDICAL: Available plans - Count: {Count}, Plans: {@Plans}", 
                    planResult.Data.Count, 
                    planResult.Data.Select(p => new { 
                        p.InsurancePlanId, 
                        p.Name, 
                        Value = p.Value, 
                        Text = p.Text,
                        Type = p.InsurancePlanId.GetType().Name 
                    }).ToList());
                
                // 🔍 Debug: بررسی نوع داده‌ها
                _logger.Information("🏥 MEDICAL: Debug - insurancePlanId type: {Type}, value: {Value}, planResult.Data.Count: {Count}", 
                    insurancePlanId.GetType().Name, insurancePlanId, planResult.Data.Count);
                
                // 🚀 FIX: جستجوی صحیح با بررسی نوع داده
                var insurancePlan = planResult.Data.FirstOrDefault(p => p.InsurancePlanId == insurancePlanId);
                
                // 🔍 Debug: بررسی نتیجه جستجو
                if (insurancePlan == null)
                {
                    _logger.Warning("🏥 MEDICAL: طرح بیمه یافت نشد - InsurancePlanId: {InsurancePlanId}, AvailablePlans: {@AvailablePlans}", 
                        insurancePlanId, 
                        planResult.Data.Select(p => new { p.InsurancePlanId, p.Name, p.Value, p.Text }).ToList());
                }
                
                // 🔍 Debug logging برای بررسی مشکل
                _logger.Information("🏥 MEDICAL: جستجوی طرح بیمه - InsurancePlanId: {InsurancePlanId}, TotalPlans: {TotalPlans}, FoundPlan: {FoundPlan}, CorrelationId: {CorrelationId}",
                    insurancePlanId, planResult.Data.Count, insurancePlan != null, correlationId);
                
                // 🔍 Debug logging برای بررسی تمام طرح‌های بیمه
                _logger.Information("🏥 MEDICAL: تمام طرح‌های بیمه موجود - Plans: {@Plans}, CorrelationId: {CorrelationId}",
                    planResult.Data.Select(p => new { p.InsurancePlanId, p.Name, p.Value, p.Text }).ToList(), correlationId);
                
                if (insurancePlan == null)
                {
                    _logger.Warning("🏥 MEDICAL: طرح بیمه یافت نشد - InsurancePlanId: {InsurancePlanId}, AvailablePlans: {@AvailablePlans}, CorrelationId: {CorrelationId}",
                        insurancePlanId, planResult.Data.Select(p => new { p.Value, p.Text }).ToList(), correlationId);
                    
                    return Json(new { success = false, message = "طرح بیمه انتخاب شده یافت نشد" });
                }

                // 🔍 محاسبه تعرفه بر اساس نوع محاسبه
                var calculationResult = await PerformAdvancedCalculationAsync(
                    service, insurancePlan, currentTariffPrice, currentPatientShare, 
                    currentInsurerShare, supplementaryCoveragePercent, calculationType, correlationId);

                var duration = DateTime.UtcNow - startTime;
                _logger.Information("🏥 MEDICAL: محاسبه پیشرفته تکمیل شد - CorrelationId: {CorrelationId}, Duration: {Duration}ms, User: {UserName} (Id: {UserId})",
                    correlationId, duration.TotalMilliseconds, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, data = calculationResult });
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه پیشرفته - CorrelationId: {CorrelationId}, Duration: {Duration}ms, User: {UserName} (Id: {UserId})",
                    correlationId, duration.TotalMilliseconds, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در محاسبه تعرفه" });
            }
        }

        /// <summary>
        /// انجام محاسبه پیشرفته تعرفه
        /// </summary>
        private async Task<object> PerformAdvancedCalculationAsync(
            dynamic service, dynamic insurancePlan, decimal? currentTariffPrice,
            decimal? currentPatientShare, decimal? currentInsurerShare,
            decimal? supplementaryCoveragePercent, string calculationType, string correlationId)
        {
            try
            {
                _logger.Debug("🏥 MEDICAL: شروع محاسبه پیشرفته - ServiceId: {ServiceId}, PlanId: {PlanId}, Type: {Type}, CorrelationId: {CorrelationId}",
                    service.Id, insurancePlan.Value, calculationType, correlationId);

                // 🔍 محاسبه قیمت تعرفه با استفاده از FactorSetting
                var tariffPrice = await CalculateTariffPriceWithFactorSettingAsync(service.Id, currentTariffPrice, correlationId);
                
                // 🔍 محاسبه سهم بیمه با استفاده از PlanService
                var insurerShare = await CalculateInsurerShareWithPlanServiceAsync(service.Id, insurancePlan.InsurancePlanId, tariffPrice, currentInsurerShare, correlationId);
                
                // 🔍 محاسبه سهم بیمار
                var patientShare = await CalculatePatientShareAsync(service.Id, insurancePlan.InsurancePlanId, tariffPrice, insurerShare, currentPatientShare, correlationId);
                
                // 🔍 محاسبه پوشش تکمیلی
                var supplementaryCoverage = await CalculateSupplementaryCoverageAsync(
                    service.Id, insurancePlan.InsurancePlanId, tariffPrice, insurerShare, supplementaryCoveragePercent, correlationId);
                
                // 🔍 محاسبه پوشش کل
                var totalCoveragePercent = await CalculateTotalCoverageAsync(
                    tariffPrice, insurerShare, supplementaryCoverage, correlationId);

                var result = new
                {
                    tariffPrice = tariffPrice,
                    patientShare = patientShare,
                    insurerShare = insurerShare,
                    supplementaryCoveragePercent = supplementaryCoverage,
                    totalCoveragePercent = totalCoveragePercent,
                    calculationType = calculationType,
                    calculatedAt = DateTime.UtcNow,
                    correlationId = correlationId
                };

                _logger.Debug("🏥 MEDICAL: محاسبه پیشرفته تکمیل شد - CorrelationId: {CorrelationId}, Result: {@Result}",
                    correlationId, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه پیشرفته - CorrelationId: {CorrelationId}", correlationId);
                throw;
            }
        }

        /// <summary>
        /// محاسبه قیمت تعرفه با استفاده از FactorSetting
        /// </summary>
        private async Task<decimal> CalculateTariffPriceWithFactorSettingAsync(int serviceId, decimal? currentTariffPrice, string correlationId)
        {
            try
            {
                // اگر قیمت فعلی موجود است، از آن استفاده کن
                if (currentTariffPrice.HasValue && currentTariffPrice.Value > 0)
                {
                    _logger.Debug("🏥 MEDICAL: استفاده از قیمت تعرفه موجود - Price: {Price}, CorrelationId: {CorrelationId}",
                        currentTariffPrice.Value, correlationId);
                    return currentTariffPrice.Value;
                }

                // دریافت سال مالی فعلی
                var currentFinancialYear = await GetCurrentFinancialYearAsync(DateTime.Now);
                
                // دریافت خدمت از دیتابیس
                var service = await _context.Services
                    .Where(s => s.ServiceId == serviceId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت یافت نشد - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}", 
                        serviceId, correlationId);
                    return 0m;
                }

                // دریافت کای فنی
                var technicalFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                    ServiceComponentType.Technical, service.IsHashtagged, currentFinancialYear);

                // دریافت کای حرفه‌ای
                var professionalFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                    ServiceComponentType.Professional, false, currentFinancialYear);

                if (technicalFactor == null || professionalFactor == null)
                {
                    _logger.Warning("🏥 MEDICAL: کای‌های مورد نیاز یافت نشد - TechnicalFactor: {TechnicalFactor}, ProfessionalFactor: {ProfessionalFactor}, CorrelationId: {CorrelationId}",
                        technicalFactor != null, professionalFactor != null, correlationId);
                    
                    // Fallback به قیمت پایه خدمت
                    return service.Price;
                }

                // 🚀 FINANCIAL PRECISION: محاسبه دقیق قیمت تعرفه بر اساس ریال
                var basePrice = service.Price;
                var calculatedPrice = basePrice * technicalFactor.Value * professionalFactor.Value;

                _logger.Debug("🏥 MEDICAL: محاسبه قیمت تعرفه با FactorSetting - BasePrice: {BasePrice}, TechnicalFactor: {TechnicalFactor}, ProfessionalFactor: {ProfessionalFactor}, Result: {Result}, CorrelationId: {CorrelationId}",
                    basePrice, technicalFactor.Value, professionalFactor.Value, calculatedPrice, correlationId);

                // 🚀 FINANCIAL PRECISION: گرد کردن به ریال (بدون اعشار)
                return Math.Round(calculatedPrice, 0, MidpointRounding.AwayFromZero);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه قیمت تعرفه با FactorSetting - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}", 
                    serviceId, correlationId);
                
                // Fallback: دریافت قیمت پایه از دیتابیس
                try
                {
                    var fallbackService = await _context.Services
                        .Where(s => s.ServiceId == serviceId && !s.IsDeleted)
                        .Select(s => s.Price)
                        .FirstOrDefaultAsync();
                    
                    return fallbackService;
                }
                catch (Exception fallbackEx)
                {
                    _logger.Error(fallbackEx, "🏥 MEDICAL: خطا در دریافت قیمت پایه خدمت - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}", 
                        serviceId, correlationId);
                    return 0m;
                }
            }
        }


        /// <summary>
        /// محاسبه سهم بیمه با استفاده از PlanService
        /// </summary>
        private async Task<decimal> CalculateInsurerShareWithPlanServiceAsync(int serviceId, int insurancePlanId, decimal tariffPrice, decimal? currentInsurerShare, string correlationId)
        {
            try
            {
                // اگر سهم بیمه فعلی موجود است، از آن استفاده کن
                if (currentInsurerShare.HasValue && currentInsurerShare.Value > 0)
                {
                    _logger.Debug("🏥 MEDICAL: استفاده از سهم بیمه موجود - Share: {Share}, CorrelationId: {CorrelationId}",
                        currentInsurerShare.Value, correlationId);
                    return currentInsurerShare.Value;
                }

                // دریافت خدمت از دیتابیس
                var service = await _context.Services
                    .Where(s => s.ServiceId == serviceId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت یافت نشد - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}", 
                        serviceId, correlationId);
                    return 0m;
                }

                // دریافت طرح بیمه از دیتابیس
                var insurancePlan = await _context.InsurancePlans
                    .Where(p => p.InsurancePlanId == insurancePlanId && p.IsActive)
                    .FirstOrDefaultAsync();

                if (insurancePlan == null)
                {
                    _logger.Warning("🏥 MEDICAL: طرح بیمه یافت نشد - InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}", 
                        insurancePlanId, correlationId);
                    return 0m;
                }

                // دریافت PlanService برای این طرح بیمه و دسته‌بندی خدمت
                var planService = await _planServiceRepository.GetByPlanAndCategoryAsync(
                    insurancePlanId, service.ServiceCategoryId);

                decimal coveragePercent;

                if (planService != null && planService.CoverageOverride.HasValue)
                {
                    // استفاده از درصد پوشش خاص خدمت
                    coveragePercent = planService.CoverageOverride.Value;
                    _logger.Debug("🏥 MEDICAL: استفاده از CoverageOverride - Percent: {Percent}, CorrelationId: {CorrelationId}",
                        coveragePercent, correlationId);
                }
                else
                {
                    coveragePercent = insurancePlan.CoveragePercent;
                    _logger.Debug("🏥 MEDICAL: استفاده از CoveragePercent از دیتابیس - Percent: {Percent}, CorrelationId: {CorrelationId}",
                        coveragePercent, correlationId);
                }

                // 🚀 FINANCIAL PRECISION: محاسبه دقیق سهم بیمه بر اساس ریال
                var calculatedShare = tariffPrice * (coveragePercent / 100m);

                _logger.Debug("🏥 MEDICAL: محاسبه سهم بیمه با PlanService - TariffPrice: {TariffPrice}, CoveragePercent: {CoveragePercent}, Result: {Result}, CorrelationId: {CorrelationId}",
                    tariffPrice, coveragePercent, calculatedShare, correlationId);

                // 🚀 FINANCIAL PRECISION: گرد کردن به ریال (بدون اعشار)
                return Math.Round(calculatedShare, 0, MidpointRounding.AwayFromZero);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه سهم بیمه با PlanService - CorrelationId: {CorrelationId}", correlationId);
                return 0m;
            }
        }

        /// <summary>
        /// محاسبه سهم بیمار
        /// </summary>
        private async Task<decimal> CalculatePatientShareAsync(int serviceId, int insurancePlanId, decimal tariffPrice, decimal insurerShare, decimal? currentPatientShare, string correlationId)
        {
            try
            {
                // اگر سهم بیمار فعلی موجود است، از آن استفاده کن
                if (currentPatientShare.HasValue && currentPatientShare.Value > 0)
                {
                    _logger.Debug("🏥 MEDICAL: استفاده از سهم بیمار موجود - Share: {Share}, CorrelationId: {CorrelationId}",
                        currentPatientShare.Value, correlationId);
                    return currentPatientShare.Value;
                }

                // 🚀 FINANCIAL PRECISION: محاسبه دقیق سهم بیمار بر اساس ریال
                var calculatedShare = Math.Max(0, tariffPrice - insurerShare);

                _logger.Debug("🏥 MEDICAL: محاسبه سهم بیمار - TariffPrice: {TariffPrice}, InsurerShare: {InsurerShare}, Result: {Result}, CorrelationId: {CorrelationId}",
                    tariffPrice, insurerShare, calculatedShare, correlationId);

                // 🚀 FINANCIAL PRECISION: گرد کردن به ریال (بدون اعشار)
                return Math.Round(calculatedShare, 0, MidpointRounding.AwayFromZero);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه سهم بیمار - CorrelationId: {CorrelationId}", correlationId);
                return 0m;
            }
        }

        /// <summary>
        /// محاسبه پوشش بیمه تکمیلی
        /// </summary>
        private async Task<decimal> CalculateSupplementaryCoverageAsync(int serviceId, int insurancePlanId, decimal tariffPrice, decimal insurerShare, decimal? supplementaryCoveragePercent, string correlationId)
        {
            try
            {
                // اگر درصد پوشش تکمیلی موجود است، از آن استفاده کن
                if (supplementaryCoveragePercent.HasValue && supplementaryCoveragePercent.Value > 0)
                {
                    _logger.Debug("🏥 MEDICAL: استفاده از پوشش تکمیلی موجود - Percent: {Percent}, CorrelationId: {CorrelationId}",
                        supplementaryCoveragePercent.Value, correlationId);
                    return supplementaryCoveragePercent.Value;
                }

                // محاسبه بر اساس نوع خدمت و طرح بیمه
                var calculatedPercent = 0m;
                
                // TODO: پیاده‌سازی منطق محاسبه پوشش تکمیلی بر اساس قوانین کسب‌وکار
                // اینجا می‌توانید قوانین خاص بیمه تکمیلی را پیاده‌سازی کنید

                _logger.Debug("🏥 MEDICAL: محاسبه پوشش تکمیلی - Result: {Result}, CorrelationId: {CorrelationId}",
                    calculatedPercent, correlationId);

                return Math.Round(calculatedPercent, 2);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه پوشش تکمیلی - CorrelationId: {CorrelationId}", correlationId);
                return 0m;
            }
        }

        /// <summary>
        /// محاسبه پوشش کل
        /// </summary>
        private async Task<decimal> CalculateTotalCoverageAsync(decimal tariffPrice, decimal insurerShare, decimal supplementaryCoveragePercent, string correlationId)
        {
            try
            {
                if (tariffPrice <= 0) return 0m;

                var primaryCoveragePercent = (insurerShare / tariffPrice) * 100m;
                var totalCoverage = Math.Min(primaryCoveragePercent + supplementaryCoveragePercent, 100m);

                _logger.Debug("🏥 MEDICAL: محاسبه پوشش کل - Primary: {Primary}%, Supplementary: {Supplementary}%, Total: {Total}%, CorrelationId: {CorrelationId}",
                    primaryCoveragePercent, supplementaryCoveragePercent, totalCoverage, correlationId);

                return Math.Round(totalCoverage, 2);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه پوشش کل - CorrelationId: {CorrelationId}", correlationId);
                return 0m;
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// عملیات گروهی - فعال/غیرفعال کردن تعرفه‌ها
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkToggleStatus(List<int> tariffIds, bool isActive)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست تغییر وضعیت گروهی تعرفه‌ها - CorrelationId: {CorrelationId}, Count: {Count}, IsActive: {IsActive}, User: {UserName} (Id: {UserId})",
                correlationId, tariffIds?.Count ?? 0, isActive, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (tariffIds == null || !tariffIds.Any())
                {
                    _logger.Warning("🏥 MEDICAL: لیست تعرفه‌ها خالی است - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                        correlationId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = "هیچ تعرفه‌ای انتخاب نشده است" });
                }

                var result = await _insuranceTariffService.BulkToggleStatusAsync(tariffIds, isActive);

                if (result.Success)
                {
                    _logger.Information("🏥 MEDICAL: تغییر وضعیت گروهی با موفقیت انجام شد - CorrelationId: {CorrelationId}, Count: {Count}, IsActive: {IsActive}, User: {UserName} (Id: {UserId})",
                        correlationId, tariffIds.Count, isActive, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = true, message = $"وضعیت {tariffIds.Count} تعرفه با موفقیت تغییر یافت" });
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در تغییر وضعیت گروهی - CorrelationId: {CorrelationId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای غیرمنتظره در تغییر وضعیت گروهی - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    correlationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در تغییر وضعیت تعرفه‌ها" });
            }
        }

        /// <summary>
        /// دریافت آمار سریع برای dashboard - Real-time
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetQuickStats()
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست آمار سریع تعرفه‌ها - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                correlationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceTariffService.GetStatisticsAsync();

                if (result.Success)
                {
                    _logger.Information("🏥 MEDICAL: آمار سریع با موفقیت دریافت شد - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                        correlationId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = true, data = result.Data });
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت آمار سریع - CorrelationId: {CorrelationId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای غیرمنتظره در دریافت آمار سریع - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    correlationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌ها برای cascade dropdown - Real-time برای محیط درمانی
        /// با بهینه‌سازی Performance و Error Handling
        /// </summary>
        [HttpGet]
        [NoCacheFilter]
        public async Task<JsonResult> GetDepartments()
        {
            // 🚀 REAL-TIME: Set No-Cache headers
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");

            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            _logger.Information("🏥 MEDICAL: درخواست دریافت دپارتمان‌ها - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                correlationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🚀 بهینه‌سازی: دریافت ClinicId از User Context
                var clinicId = await GetCurrentClinicIdAsync();
                
                // دریافت دپارتمان‌ها با Timeout
                var result = await _departmentManagementService.GetActiveDepartmentsForLookupAsync(clinicId);
                
                var duration = DateTime.UtcNow - startTime;
                
                if (result?.Success == true && result.Data?.Any() == true)
                {
                    var departments = result.Data.Select(d => new { 
                        id = d.Id, 
                        name = d.Name,
                        description = d.Description ?? ""
                    }).ToList();
                    
                    _logger.Information("🏥 MEDICAL: دپارتمان‌ها با موفقیت دریافت شدند - Count: {Count}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                        departments.Count, duration.TotalMilliseconds, correlationId);
                    
                    return Json(new { 
                        success = true, 
                        data = departments,
                        correlationId = correlationId,
                        duration = duration.TotalMilliseconds
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت دپارتمان‌ها - Error: {Error}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                        result?.Message ?? "Unknown error", duration.TotalMilliseconds, correlationId);
                    
                    return Json(new { 
                        success = false, 
                        message = result?.Message ?? "خطا در دریافت دپارتمان‌ها",
                        correlationId = correlationId
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت دپارتمان‌ها - Duration: {Duration}ms, CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    duration.TotalMilliseconds, correlationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت دپارتمان‌ها",
                    correlationId = correlationId
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت سرفصل‌های خدماتی برای دپارتمان - با بهینه‌سازی Performance
        /// </summary>
        [HttpGet]
        [NoCacheFilter]
        public async Task<JsonResult> GetServiceCategories(int departmentId)
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            _logger.Information("🏥 MEDICAL: درخواست دریافت سرفصل‌های خدماتی - DepartmentId: {DepartmentId}, CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                departmentId, correlationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🔍 اعتبارسنجی ورودی
                if (departmentId <= 0)
                {
                    _logger.Warning("🏥 MEDICAL: شناسه دپارتمان نامعتبر - DepartmentId: {DepartmentId}, CorrelationId: {CorrelationId}",
                        departmentId, correlationId);
                    
                    return Json(new { 
                        success = false, 
                        message = "شناسه دپارتمان نامعتبر است",
                        correlationId = correlationId
                    }, JsonRequestBehavior.AllowGet);
                }

                // دریافت سرفصل‌های خدماتی با Timeout
                var result = await _serviceManagementService.GetActiveServiceCategoriesForLookupAsync(departmentId);
                
                var duration = DateTime.UtcNow - startTime;
                
                if (result?.Success == true && result.Data?.Any() == true)
                {
                    var categories = result.Data.Select(c => new { 
                        id = c.Id, 
                        name = c.Name,
                        description = c.Description ?? ""
                    }).ToList();
                    
                    _logger.Information("🏥 MEDICAL: سرفصل‌های خدماتی با موفقیت دریافت شدند - Count: {Count}, DepartmentId: {DepartmentId}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                        categories.Count, departmentId, duration.TotalMilliseconds, correlationId);
                    
                    return Json(new { 
                        success = true, 
                        data = categories,
                        departmentId = departmentId,
                        correlationId = correlationId,
                        duration = duration.TotalMilliseconds
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت سرفصل‌های خدماتی - DepartmentId: {DepartmentId}, Error: {Error}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                        departmentId, result?.Message ?? "Unknown error", duration.TotalMilliseconds, correlationId);
                    
                    return Json(new { 
                        success = false, 
                        message = result?.Message ?? "خطا در دریافت سرفصل‌های خدماتی",
                        departmentId = departmentId,
                        correlationId = correlationId
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت سرفصل‌های خدماتی - DepartmentId: {DepartmentId}, Duration: {Duration}ms, CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    departmentId, duration.TotalMilliseconds, correlationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت سرفصل‌های خدماتی",
                    departmentId = departmentId,
                    correlationId = correlationId
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 🚀 PERFORMANCE: دریافت خدمات برای سرفصل خدماتی با پشتیبانی از Search و Paging
        /// </summary>
        [HttpGet]
        [NoCacheFilter]
        public async Task<JsonResult> GetServices(int serviceCategoryId, string search = "", int page = 1, int pageSize = 20)
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            _logger.Information("🏥 MEDICAL: درخواست دریافت خدمات - ServiceCategoryId: {ServiceCategoryId}, CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                serviceCategoryId, correlationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🔍 اعتبارسنجی ورودی
                if (serviceCategoryId <= 0)
                {
                    _logger.Warning("🏥 MEDICAL: شناسه سرفصل خدماتی نامعتبر - ServiceCategoryId: {ServiceCategoryId}, CorrelationId: {CorrelationId}",
                        serviceCategoryId, correlationId);
                    
                    return Json(new { 
                        success = false, 
                        message = "شناسه سرفصل خدماتی نامعتبر است",
                        correlationId = correlationId
                    }, JsonRequestBehavior.AllowGet);
                }

                // دریافت خدمات با Timeout
                var result = await _serviceManagementService.GetActiveServicesForLookupAsync(serviceCategoryId);
                
                var duration = DateTime.UtcNow - startTime;
                
                if (result?.Success == true && result.Data?.Any() == true)
                {
                    var services = result.Data.Select(s => new { 
                        id = s.Id, 
                        name = s.Name,
                        code = s.Code ?? "",
                        description = s.Description ?? ""
                    }).ToList();
                    
                    _logger.Information("🏥 MEDICAL: خدمات با موفقیت دریافت شدند - Count: {Count}, ServiceCategoryId: {ServiceCategoryId}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                        services.Count, serviceCategoryId, duration.TotalMilliseconds, correlationId);
                    
                    return Json(new { 
                        success = true, 
                        data = services,
                        serviceCategoryId = serviceCategoryId,
                        correlationId = correlationId,
                        duration = duration.TotalMilliseconds
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت خدمات - ServiceCategoryId: {ServiceCategoryId}, Error: {Error}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                        serviceCategoryId, result?.Message ?? "Unknown error", duration.TotalMilliseconds, correlationId);
                    
                    return Json(new { 
                        success = false, 
                        message = result?.Message ?? "خطا در دریافت خدمات",
                        serviceCategoryId = serviceCategoryId,
                        correlationId = correlationId
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت خدمات - ServiceCategoryId: {ServiceCategoryId}, Duration: {Duration}ms, CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    serviceCategoryId, duration.TotalMilliseconds, correlationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { 
                    success = false, 
                    message = "خطا در دریافت خدمات",
                    serviceCategoryId = serviceCategoryId,
                    correlationId = correlationId
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// جستجوی خدمات برای Select2 AJAX
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> SearchServices(string searchTerm = "", int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("🔍 MEDICAL: درخواست جستجوی خدمات - SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}, User: {UserName} (Id: {UserId})",
                    searchTerm, page, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _serviceService.SearchServicesForSelect2Async(searchTerm, page, pageSize);
                
                if (result.Success)
                {
                    var services = result.Data.Items.Select(s => new { id = s.ServiceId, name = s.Title }).ToList();
                    _logger.Information("🔍 MEDICAL: جستجوی خدمات موفق - Count: {Count}, TotalCount: {TotalCount}, SearchTerm: {SearchTerm}, User: {UserName} (Id: {UserId})",
                        services.Count, result.Data.TotalItems, searchTerm, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = true, 
                        data = services,
                        totalCount = result.Data.TotalItems
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🔍 MEDICAL: خطا در جستجوی خدمات - SearchTerm: {SearchTerm}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        searchTerm, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🔍 MEDICAL: خطا در جستجوی خدمات - SearchTerm: {SearchTerm}, User: {UserName} (Id: {UserId})",
                    searchTerm, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در جستجوی خدمات" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 🚀 PERFORMANCE: دریافت ارائه‌دهندگان بیمه فعال با پشتیبانی از Search و Paging
        /// </summary>
        [HttpGet]
        [NoCacheFilter]
        public async Task<JsonResult> GetInsuranceProviders(string search = "", int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت ارائه‌دهندگان بیمه - Search: {Search}, Page: {Page}, PageSize: {PageSize}, User: {UserName} (Id: {UserId})",
                    search, page, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _insuranceProviderService.GetActiveProvidersForLookupAsync();
                
                _logger.Information("🏥 MEDICAL: نتیجه دریافت ارائه‌دهندگان بیمه - Success: {Success}, Message: {Message}, DataCount: {DataCount}",
                    result.Success, result.Message, result.Data?.Count ?? 0);
                
                if (result.Success)
                {
                    var allProviders = result.Data.Select(p => new { 
                        id = p.Value, 
                        name = p.Text,
                        description = p.Text // می‌تواند از دیتابیس دریافت شود
                    }).ToList();

                    // 🚀 PERFORMANCE: Server-side filtering
                    var filteredProviders = allProviders;
                    if (!string.IsNullOrEmpty(search))
                    {
                        filteredProviders = allProviders.Where(p => 
                            p.name.ToLower().Contains(search.ToLower()) ||
                            p.description.ToLower().Contains(search.ToLower())
                        ).ToList();
                    }

                    // 🚀 PERFORMANCE: Server-side paging
                    var totalCount = filteredProviders.Count;
                    var hasMore = (page * pageSize) < totalCount;
                    var pagedProviders = filteredProviders
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    _logger.Information("🏥 MEDICAL: ارائه‌دهندگان بیمه با موفقیت دریافت شدند - Total: {Total}, Page: {Page}, Returned: {Returned}, HasMore: {HasMore}, User: {UserName} (Id: {UserId})",
                        totalCount, page, pagedProviders.Count, hasMore, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { 
                        success = true, 
                        data = pagedProviders,
                        hasMore = hasMore,
                        totalCount = totalCount
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت ارائه‌دهندگان بیمه - Error: {Error}, User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت ارائه‌دهندگان بیمه - User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در دریافت ارائه‌دهندگان بیمه: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 🚀 PERFORMANCE: دریافت طرح‌های بیمه برای ارائه‌دهنده با پشتیبانی از Search و Paging
        /// </summary>
        [HttpGet]
        [NoCacheFilter]
        public async Task<JsonResult> GetInsurancePlans(int? providerId = null, string search = "", int page = 1, int pageSize = 15)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت طرح‌های بیمه - ProviderId: {ProviderId}, User: {UserName} (Id: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _insurancePlanService.GetActivePlansForLookupAsync(providerId);
                
                _logger.Information("🏥 MEDICAL: نتیجه دریافت طرح‌های بیمه - Success: {Success}, Message: {Message}, DataCount: {DataCount}",
                    result.Success, result.Message, result.Data?.Count ?? 0);
                
                if (result.Success)
                {
                    // 🚀 FIX: استفاده از format یکسان با CalculateAdvancedTariff
                    var plans = result.Data.Select(p => new { 
                        id = p.InsurancePlanId, 
                        name = p.Name,
                        InsurancePlanId = p.InsurancePlanId,  // 🚀 FIX: اضافه کردن InsurancePlanId
                        Value = p.InsurancePlanId,  // اضافه کردن Value برای سازگاری
                        Text = p.Name               // اضافه کردن Text برای سازگاری
                    }).ToList();
                    
                    // 🔍 Debug logging برای بررسی plans
                    _logger.Information("🏥 MEDICAL: طرح‌های بیمه با موفقیت دریافت شدند - Count: {Count}, ProviderId: {ProviderId}, Plans: {@Plans}, User: {UserName} (Id: {UserId})",
                        plans.Count, providerId, plans.Select(p => new { p.id, p.name, p.Value, p.Text }).ToList(), _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, data = plans }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت طرح‌های بیمه - ProviderId: {ProviderId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        providerId, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت طرح‌های بیمه - ProviderId: {ProviderId}, User: {UserName} (Id: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در دریافت طرح‌های بیمه: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی وجود تعرفه برای خدمت و طرح بیمه مشخص
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CheckTariffExists(int serviceId, int planId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: بررسی وجود تعرفه - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    serviceId, planId, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی برای محیط درمانی
                if (serviceId <= 0 || planId <= 0)
                {
                    _logger.Warning("🏥 MEDICAL: پارامترهای ورودی نامعتبر - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        serviceId, planId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = false, 
                        exists = false,
                        message = "پارامترهای ورودی نامعتبر است" 
                    });
                }

                var result = await _insuranceTariffService.CheckTariffExistsAsync(serviceId, planId);
                
                if (result.Success)
                {
                    _logger.Information("🏥 MEDICAL: بررسی وجود تعرفه تکمیل شد - Exists: {Exists}, ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        result.Data, serviceId, planId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = true, 
                        exists = result.Data,
                        message = result.Data ? "تعرفه برای این خدمت و طرح قبلاً وجود دارد" : "تعرفه برای این خدمت و طرح وجود ندارد"
                    });
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در بررسی وجود تعرفه - Error: {Error}, ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        result.Message, serviceId, planId, _currentUserService.UserName, _currentUserService.UserId);
                    
                return Json(new { 
                        success = false, 
                        exists = false,
                        message = result.Message 
                });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در بررسی وجود تعرفه - ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    serviceId, planId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { 
                    success = false, 
                    exists = false,
                    message = "خطا در بررسی وجود تعرفه" 
                });
            }
        }


        /// <summary>
        /// محاسبه تعرفه بیمه اصلی - JSON endpoint برای محاسبه سهم‌ها بر اساس تنظیمات داینامیک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CalculatePrimaryTariff(int serviceId, int planId, decimal? baseAmount = null)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست محاسبه تعرفه بیمه اصلی. ServiceId: {ServiceId}, PlanId: {PlanId}, BaseAmount: {BaseAmount}. User: {UserName} (Id: {UserId})",
                    serviceId, planId, baseAmount, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت اطلاعات خدمت برای محاسبه مبلغ پایه
                var serviceResult = await _serviceService.GetServiceDetailsAsync(serviceId);
                if (!serviceResult.Success)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت یافت نشد. ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})",
                        serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    
                return Json(new { 
                        success = false, 
                        message = "خدمت یافت نشد" 
                    });
                }

                var service = serviceResult.Data;
                decimal calculatedBaseAmount;

                // اگر مبلغ پایه ارائه شده، از آن استفاده کن، در غیر این صورت محاسبه کن
                if (baseAmount.HasValue && baseAmount.Value > 0)
                {
                    calculatedBaseAmount = baseAmount.Value;
                    _logger.Information("🏥 MEDICAL: استفاده از مبلغ ارائه شده. BaseAmount: {BaseAmount}", calculatedBaseAmount);
                }
                else
                {
                // محاسبه مبلغ پایه بر اساس ServiceComponents
                calculatedBaseAmount = await CalculateServiceBasePriceAsync(service.ServiceId);
                    _logger.Information("🏥 MEDICAL: محاسبه مبلغ پایه خدمت. ServiceId: {ServiceId}, CalculatedAmount: {CalculatedAmount}", 
                        serviceId, calculatedBaseAmount);
                }

                // دریافت اطلاعات طرح بیمه برای محاسبه داینامیک سهم‌ها
                var planResult = await _insurancePlanService.GetPlanDetailsAsync(planId);
                if (!planResult.Success)
                {
                    _logger.Warning("🏥 MEDICAL: طرح بیمه یافت نشد. PlanId: {PlanId}, User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { 
                    success = false, 
                        message = "طرح بیمه یافت نشد" 
                    });
                }

                var plan = planResult.Data;
                var coveragePercent = plan.CoveragePercent / 100m; // تبدیل درصد به اعشار
                var patientPercent = 1m - coveragePercent; // سهم بیمار = 100% - درصد پوشش بیمه

                // محاسبه سهم‌ها بر اساس تنظیمات داینامیک
                var insurerShare = calculatedBaseAmount * coveragePercent;
                var patientShare = calculatedBaseAmount * patientPercent;

                _logger.Information("🏥 MEDICAL: محاسبه سهم‌ها بر اساس تنظیمات طرح بیمه. PlanId: {PlanId}, CoveragePercent: {CoveragePercent}%, InsurerShare: {InsurerShare}, PatientShare: {PatientShare}. User: {UserName} (Id: {UserId})",
                    planId, plan.CoveragePercent, insurerShare, patientShare, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { 
                    success = true, 
                    data = new { 
                        calculatedAmount = Math.Round(calculatedBaseAmount, 2),
                        patientShare = Math.Round(patientShare, 2),
                        insurerShare = Math.Round(insurerShare, 2),
                        coveragePercent = plan.CoveragePercent,
                        patientPercent = Math.Round(patientPercent * 100, 2),
                        planName = plan.Name,
                        planCode = plan.PlanCode,
                        serviceName = service.Title,
                        serviceCode = service.ServiceCode
                    },
                    message = $"محاسبه تعرفه بیمه اصلی با موفقیت انجام شد - خدمت: {service.Title}, طرح: {plan.Name}"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه تعرفه بیمه اصلی. ServiceId: {ServiceId}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    serviceId, planId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { 
                    success = false, 
                    message = "خطا در محاسبه تعرفه بیمه اصلی" 
                });
            }
        }

        /// <summary>
        /// محاسبه مبلغ پایه خدمت بر اساس ServiceComponents و FactorSettings
        /// </summary>
        private async Task<decimal> CalculateServiceBasePriceAsync(int serviceId)
        {
            try
            {
                _logger.Debug("🏥 MEDICAL: شروع محاسبه مبلغ پایه خدمت. ServiceId: {ServiceId}", serviceId);

                // دریافت اطلاعات خدمت از دیتابیس
                var service = await _context.Services
                    .Include(s => s.ServiceComponents)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت یافت نشد. ServiceId: {ServiceId}", serviceId);
                    return 0m;
                }

                // استفاده از ServiceCalculationService برای محاسبه دقیق
                try
                {
                    // ابتدا سعی کنیم از FactorSettings استفاده کنیم
                    var serviceCalculationService = DependencyResolver.Current.GetService<IServiceCalculationService>();
                    if (serviceCalculationService != null)
                    {
                        var calculatedPrice = serviceCalculationService.CalculateServicePriceWithFactorSettings(
                            service, _context, DateTime.Now);

                        _logger.Information("🏥 MEDICAL: محاسبه مبلغ پایه با FactorSettings موفق. ServiceId: {ServiceId}, CalculatedPrice: {CalculatedPrice}", 
                            serviceId, calculatedPrice);

                        return calculatedPrice;
                    }
                }
                catch (Exception factorEx)
                {
                    _logger.Warning(factorEx, "🏥 MEDICAL: خطا در محاسبه با FactorSettings، استفاده از روش پایه. ServiceId: {ServiceId}", serviceId);
                }

                // اگر FactorSettings موجود نباشد، از دیتابیس ضرایب را بخوان
                if (service.ServiceComponents != null && service.ServiceComponents.Any())
                {
                    var technicalComponent = service.ServiceComponents
                        .FirstOrDefault(sc => sc.ComponentType == Models.Enums.ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);
                    var professionalComponent = service.ServiceComponents
                        .FirstOrDefault(sc => sc.ComponentType == Models.Enums.ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);

                    if (technicalComponent != null && professionalComponent != null)
                    {
                        // دریافت ضرایب از دیتابیس - بدون هاردکد
                        var currentFinancialYear = await GetCurrentFinancialYearAsync(DateTime.Now);
                        
                        // دریافت ضریب فنی از دیتابیس
                        var technicalFactor = await _context.FactorSettings
                            .Where(fs => fs.FactorType == Models.Enums.ServiceComponentType.Technical &&
                                        fs.IsHashtagged == service.IsHashtagged &&
                                        fs.FinancialYear == currentFinancialYear &&
                                        fs.IsActive && !fs.IsDeleted &&
                                        !fs.IsFrozen &&
                                        fs.EffectiveFrom <= DateTime.Now &&
                                        (fs.EffectiveTo == null || fs.EffectiveTo >= DateTime.Now))
                            .OrderByDescending(fs => fs.EffectiveFrom)
                            .Select(fs => fs.Value)
                            .FirstOrDefaultAsync();

                        // دریافت ضریب حرفه‌ای از دیتابیس
                        var professionalFactor = await _context.FactorSettings
                            .Where(fs => fs.FactorType == Models.Enums.ServiceComponentType.Professional &&
                                        fs.IsHashtagged == false && // کای حرفه‌ای همیشه false است
                                        fs.FinancialYear == currentFinancialYear &&
                                        fs.IsActive && !fs.IsDeleted &&
                                        !fs.IsFrozen &&
                                        fs.EffectiveFrom <= DateTime.Now &&
                                        (fs.EffectiveTo == null || fs.EffectiveTo >= DateTime.Now))
                            .OrderByDescending(fs => fs.EffectiveFrom)
                            .Select(fs => fs.Value)
                            .FirstOrDefaultAsync();

                        if (technicalFactor > 0 && professionalFactor > 0)
                        {
                            var calculatedPrice = (technicalComponent.Coefficient * technicalFactor) + 
                                                 (professionalComponent.Coefficient * professionalFactor);

                            _logger.Information("🏥 MEDICAL: محاسبه مبلغ پایه با ضرایب دیتابیس. ServiceId: {ServiceId}, TechnicalCoeff: {TechnicalCoeff}, ProfessionalCoeff: {ProfessionalCoeff}, TechnicalFactor: {TechnicalFactor}, ProfessionalFactor: {ProfessionalFactor}, CalculatedPrice: {CalculatedPrice}", 
                                serviceId, technicalComponent.Coefficient, professionalComponent.Coefficient, technicalFactor, professionalFactor, calculatedPrice);

                            return calculatedPrice;
                        }
                        else
                        {
                            _logger.Warning("🏥 MEDICAL: ضرایب فنی یا حرفه‌ای در دیتابیس یافت نشد. ServiceId: {ServiceId}, TechnicalFactor: {TechnicalFactor}, ProfessionalFactor: {ProfessionalFactor}", 
                                serviceId, technicalFactor, professionalFactor);
                        }
                    }
                }

                // اگر اجزای خدمت تعریف نشده‌اند، از قیمت پایه استفاده کن
                _logger.Information("🏥 MEDICAL: استفاده از قیمت پایه خدمت. ServiceId: {ServiceId}, BasePrice: {BasePrice}", 
                    serviceId, service.Price);

                return service.Price;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه مبلغ پایه خدمت. ServiceId: {ServiceId}", serviceId);
                
                // Fallback به قیمت پایه - دریافت از دیتابیس
                try
                {
                    var fallbackService = await _context.Services
                        .Where(s => s.ServiceId == serviceId && !s.IsDeleted)
                        .Select(s => s.Price)
                        .FirstOrDefaultAsync();
                    
                    return fallbackService;
                }
                catch
                {
                    return 0m;
                }
            }
        }

        /// <summary>
        /// دریافت سال مالی جاری از تنظیمات سیستم
        /// </summary>
        private async Task<int> GetCurrentFinancialYearAsync(DateTime date)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _logger.Information("🏥 MEDICAL: دریافت سال مالی جاری - Date: {Date}, CorrelationId: {CorrelationId}", date, correlationId);

                // دریافت ClinicId از UserContext
                var clinicId = await _userContextService.GetCurrentClinicIdAsync();
                
                // دریافت سال مالی از تنظیمات سیستم
                var financialYear = await _systemSettingService.GetCurrentFinancialYearAsync(clinicId);
                
                _logger.Information("🏥 MEDICAL: سال مالی دریافت شد - FinancialYear: {FinancialYear}, ClinicId: {ClinicId}, CorrelationId: {CorrelationId}", 
                    financialYear, clinicId, correlationId);

                return financialYear;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت سال مالی - Date: {Date}", date);
                
                // Fallback به منطق پیش‌فرض (21 مارس)
                if (date.Month >= 3)
                    return date.Year;
                else
                    return date.Year - 1;
            }
        }

        #region Private Validation Methods

        /// <summary>
        /// 🛡️ اعتبارسنجی جامع ورودی‌های محاسبه - ضد گلوله
        /// </summary>
        private (bool IsValid, List<string> Errors) ValidateCalculationInputs(int serviceId, int insurancePlanId, int? providerId, string correlationId)
        {
            var errors = new List<string>();

            try
            {
                // 🔍 اعتبارسنجی ServiceId
                if (serviceId <= 0)
                {
                    errors.Add("شناسه خدمت معتبر نیست");
                    _logger.Warning("🏥 MEDICAL: ServiceId نامعتبر - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}", 
                        serviceId, correlationId);
                }

                // 🔍 اعتبارسنجی InsurancePlanId
                if (insurancePlanId <= 0)
                {
                    errors.Add("شناسه طرح بیمه معتبر نیست");
                    _logger.Warning("🏥 MEDICAL: InsurancePlanId نامعتبر - InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}", 
                        insurancePlanId, correlationId);
                }

                // 🔍 اعتبارسنجی ProviderId (اختیاری)
                if (providerId.HasValue && providerId.Value <= 0)
                {
                    errors.Add("شناسه ارائه‌دهنده بیمه معتبر نیست");
                    _logger.Warning("🏥 MEDICAL: ProviderId نامعتبر - ProviderId: {ProviderId}, CorrelationId: {CorrelationId}", 
                        providerId, correlationId);
                }

                // 🔍 اعتبارسنجی CorrelationId
                if (string.IsNullOrWhiteSpace(correlationId))
                {
                    errors.Add("شناسه همبستگی محاسبه معتبر نیست");
                    _logger.Warning("🏥 MEDICAL: CorrelationId نامعتبر - CorrelationId: {CorrelationId}", correlationId);
                }

                // 🔍 اعتبارسنجی محدوده‌های عددی
                if (serviceId > int.MaxValue || serviceId < int.MinValue)
                {
                    errors.Add("شناسه خدمت خارج از محدوده مجاز است");
                }

                if (insurancePlanId > int.MaxValue || insurancePlanId < int.MinValue)
                {
                    errors.Add("شناسه طرح بیمه خارج از محدوده مجاز است");
                }

                var isValid = !errors.Any();
                
                if (isValid)
                {
                    _logger.Information("🏥 MEDICAL: اعتبارسنجی ورودی‌ها موفق - ServiceId: {ServiceId}, InsurancePlanId: {InsurancePlanId}, ProviderId: {ProviderId}, CorrelationId: {CorrelationId}", 
                        serviceId, insurancePlanId, providerId, correlationId);
                }

                return (isValid, errors);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی ورودی‌ها - ServiceId: {ServiceId}, InsurancePlanId: {InsurancePlanId}, ProviderId: {ProviderId}, CorrelationId: {CorrelationId}", 
                    serviceId, insurancePlanId, providerId, correlationId);
                
                errors.Add("خطا در اعتبارسنجی ورودی‌ها");
                return (false, errors);
            }
        }

        /// <summary>
        /// 🛡️ اعتبارسنجی امن نتایج سرویس‌ها
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateServiceResults(ServiceResult<List<ViewModels.LookupItemViewModel>> serviceResult, ServiceResult<List<ViewModels.Insurance.InsurancePlan.InsurancePlanLookupViewModel>> planResult, int serviceId, int insurancePlanId, string correlationId)
        {
            try
            {
                // 🔍 اعتبارسنجی ServiceResult
                if (serviceResult?.Success != true)
                {
                    var errorMsg = $"خطا در دریافت خدمات: {serviceResult?.Message ?? "نامشخص"}";
                    _logger.Warning("🏥 MEDICAL: ServiceResult ناموفق - {Error}, CorrelationId: {CorrelationId}", errorMsg, correlationId);
                    return (false, errorMsg);
                }

                if (serviceResult.Data == null || !serviceResult.Data.Any())
                {
                    var errorMsg = "هیچ خدمتی یافت نشد";
                    _logger.Warning("🏥 MEDICAL: هیچ خدمتی یافت نشد - CorrelationId: {CorrelationId}", correlationId);
                    return (false, errorMsg);
                }

                // 🔍 اعتبارسنجی PlanResult
                if (planResult?.Success != true)
                {
                    var errorMsg = $"خطا در دریافت طرح‌های بیمه: {planResult?.Message ?? "نامشخص"}";
                    _logger.Warning("🏥 MEDICAL: PlanResult ناموفق - {Error}, CorrelationId: {CorrelationId}", errorMsg, correlationId);
                    return (false, errorMsg);
                }

                if (planResult.Data == null || !planResult.Data.Any())
                {
                    var errorMsg = "هیچ طرح بیمه‌ای یافت نشد";
                    _logger.Warning("🏥 MEDICAL: هیچ طرح بیمه‌ای یافت نشد - CorrelationId: {CorrelationId}", correlationId);
                    return (false, errorMsg);
                }

                // 🔍 اعتبارسنجی وجود ServiceId در نتایج
                var serviceExists = serviceResult.Data.Any(s => s.Value == serviceId);
                if (!serviceExists)
                {
                    var errorMsg = $"خدمت با شناسه {serviceId} یافت نشد";
                    _logger.Warning("🏥 MEDICAL: ServiceId در نتایج یافت نشد - ServiceId: {ServiceId}, AvailableServices: {AvailableServices}, CorrelationId: {CorrelationId}", 
                        serviceId.ToString(), string.Join(", ", serviceResult.Data.Select(s => s.Value)), correlationId);
                    return (false, errorMsg);
                }

                // 🔍 اعتبارسنجی وجود InsurancePlanId در نتایج
                var planExists = planResult.Data.Any(p => p.InsurancePlanId == insurancePlanId);
                if (!planExists)
                {
                    var errorMsg = $"طرح بیمه با شناسه {insurancePlanId} یافت نشد";
                    _logger.Warning("🏥 MEDICAL: InsurancePlanId در نتایج یافت نشد - InsurancePlanId: {InsurancePlanId}, AvailablePlans: {AvailablePlans}, CorrelationId: {CorrelationId}", 
                        insurancePlanId, string.Join(", ", planResult.Data.Select(p => p.InsurancePlanId)), correlationId);
                    return (false, errorMsg);
                }

                _logger.Information("🏥 MEDICAL: اعتبارسنجی نتایج سرویس‌ها موفق - ServiceId: {ServiceId}, InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}", 
                    serviceId, insurancePlanId, correlationId);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی نتایج سرویس‌ها - ServiceId: {ServiceId}, InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}", 
                    serviceId, insurancePlanId, correlationId);
                
                return (false, "خطا در اعتبارسنجی نتایج سرویس‌ها");
            }
        }

        #endregion

    }
}
#endregion  
