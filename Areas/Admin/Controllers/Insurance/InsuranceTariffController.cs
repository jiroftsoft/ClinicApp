using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using ClinicApp.Interfaces;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Services;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using ClinicApp.ViewModels.Validators;
using Serilog;
using ClinicApp.Core;
using ClinicApp.Filters;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using ClinicApp.Validators;
using ClinicApp.Models.DTOs.Calculation;
using ClinicApp.Models;
using System.Data.Entity;
using ClinicApp.Models.Enums;
using ClinicApp.Services.UserContext;
using ClinicApp.Services.SystemSettings;
using ClinicApp.Services.Insurance;
using ClinicApp.Services.Idempotency;
using ClinicApp.Models.DTOs;

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
        private readonly IInsuranceTariffCalculationService _tariffCalculationService;
        private readonly IFactorSettingService _factorSettingService;
        private readonly IPlanServiceRepository _planServiceRepository;
        private readonly IUserContextService _userContextService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IBusinessRuleEngine _businessRuleEngine;
        private readonly IIdempotencyService _idempotencyService;

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
            IInsuranceTariffCalculationService tariffCalculationService,
            IFactorSettingService factorSettingService,
            IPlanServiceRepository planServiceRepository,
            IUserContextService userContextService,
            ISystemSettingService systemSettingService,
            IBusinessRuleEngine businessRuleEngine,
            IIdempotencyService idempotencyService)
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
            _tariffCalculationService = tariffCalculationService ?? throw new ArgumentNullException(nameof(tariffCalculationService));
            _factorSettingService = factorSettingService ?? throw new ArgumentNullException(nameof(factorSettingService));
            _planServiceRepository = planServiceRepository ?? throw new ArgumentNullException(nameof(planServiceRepository));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _systemSettingService = systemSettingService ?? throw new ArgumentNullException(nameof(systemSettingService));
            _businessRuleEngine = businessRuleEngine ?? throw new ArgumentNullException(nameof(businessRuleEngine));
            _idempotencyService = idempotencyService ?? throw new ArgumentNullException(nameof(idempotencyService));
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
        public async Task<ActionResult> LoadTariffs(InsuranceTariffFilterViewModel filter)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست AJAX بارگیری تعرفه‌ها - CorrelationId: {CorrelationId}, Filter: {@Filter}, User: {UserName} (Id: {UserId})",
                correlationId, filter, _currentUserService.UserName, _currentUserService.UserId);

            // 🔍 STRUCTURED LOGGING - تمام مقادیر Form
            _logger.Debug("🔍 LOAD TARIFFS DEBUG START - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId}), Timestamp: {Timestamp}",
                correlationId, _currentUserService.UserName, _currentUserService.UserId, DateTime.UtcNow);
            
            // Logging Request.Form برای debug (with sensitive data masking)
            _logger.Debug("🔍 Request.Form Keys and Values - CorrelationId: {CorrelationId}", correlationId);
            foreach (string key in Request.Form.AllKeys)
            {
                var value = MaskSensitiveData(key, Request.Form[key]);
                _logger.Debug("🔍   {Key}: '{Value}' - CorrelationId: {CorrelationId}", key, value, correlationId);
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
                // 🚀 P0 FIX: یکنواخت‌سازی هدرهای Cache
                Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                Response.Cache.SetExpires(DateTime.UtcNow.AddSeconds(-1));
                Response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);

                // 🚀 بهینه‌سازی: ایجاد مدل با مقادیر پیش‌فرض بهینه
                var model = new InsuranceTariffCreateEditViewModel
                {
                    InsurancePlanId = planId ?? 0,
                    InsuranceProviderId = providerId ?? 0,
                    ServiceId = serviceId,
                    IsActive = true,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1)
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
            
            // 🚀 P0 FIX: بررسی Raw Form Data برای IsAllServices
            var rawIsAllServices = Request.Form["IsAllServices"];
            _logger.Information("🏥 MEDICAL: Raw IsAllServices from Request.Form: '{RawValue}' - CorrelationId: {CorrelationId}",
                rawIsAllServices, correlationId);
            
            // Logging Request.Form برای debug (with sensitive data masking)
            _logger.Debug("🔍 Request.Form Keys and Values - CorrelationId: {CorrelationId}", correlationId);
            foreach (string key in Request.Form.AllKeys)
            {
                var value = MaskSensitiveData(key, Request.Form[key]);
                _logger.Debug("🔍   {Key}: '{Value}' - CorrelationId: {CorrelationId}", key, value, correlationId);
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
                    
                    // 🚀 P0 FIX: بارگیری SelectLists قبل از return View
                    await LoadSelectListsForCreateEditAsync(model);
                    return View(model);
                }

                // 🔒 اعتبارسنجی Idempotency واقعی با سرویس جدید
                var isIdempotencyValid = await _idempotencyService.TryUseKeyAsync(model.IdempotencyKey, 30, "InsuranceTariff");
                if (!isIdempotencyValid)
                {
                    _logger.Warning("🏥 MEDICAL: IdempotencyKey تکراری یا نامعتبر - Key: {Key}, CorrelationId: {CorrelationId}", 
                        model.IdempotencyKey, correlationId);
                    _messageNotificationService.AddErrorMessage("درخواست تکراری یا نامعتبر است");
                    
                    // 🚀 P0 FIX: بارگیری SelectLists قبل از return View
                    await LoadSelectListsForCreateEditAsync(model);
                    return View(model);
                }

                // 🚀 بهینه‌سازی: بررسی duplicate تعرفه با Performance Enhancement
                var duplicateCheckStartTime = DateTime.UtcNow;
                
                // بررسی duplicate فقط برای تعرفه‌های تکی (نه همه خدمات)
                var isDuplicate = await _tariffCalculationService.IsTariffDuplicateAsync(
                    model.InsurancePlanId, model.ServiceId, model.IsAllServices);
                
                var duplicateCheckDuration = DateTime.UtcNow - duplicateCheckStartTime;
                
                if (isDuplicate)
                {
                    _logger.Warning("🏥 MEDICAL: تعرفه تکراری شناسایی شد - PlanId: {PlanId}, ServiceId: {ServiceId}, Duration: {Duration}ms, CorrelationId: {CorrelationId}", 
                        model.InsurancePlanId, model.ServiceId, duplicateCheckDuration.TotalMilliseconds, correlationId);
                    _messageNotificationService.AddErrorMessage("تعرفه برای این خدمت و طرح بیمه قبلاً تعریف شده است");
                    
                    // 🚀 P0 FIX: بارگیری SelectLists قبل از return View
                    await LoadSelectListsForCreateEditAsync(model);
                    return View(model);
                }
                
                _logger.Debug("🏥 MEDICAL: بررسی duplicate تکمیل شد - Duration: {Duration}ms, CorrelationId: {CorrelationId}", 
                    duplicateCheckDuration.TotalMilliseconds, correlationId);

                // 🚀 P0 FIX: Manual Override برای IsAllServices
                if (rawIsAllServices == "true" || rawIsAllServices == "True")
                {
                    model.IsAllServices = true;
                    _logger.Information("🏥 MEDICAL: Manual override - IsAllServices set to true from raw value: '{RawValue}' - CorrelationId: {CorrelationId}",
                        rawIsAllServices, correlationId);
                }
                
                // 🚀 P1 FIX: استفاده از helper method متمرکز
                _logger.Information("🏥 MEDICAL: Model received - IsAllServices: {IsAllServices}, IsAllServiceCategories: {IsAllServiceCategories}, ServiceId: {ServiceId}, ServiceCategoryId: {ServiceCategoryId}, PlanId: {PlanId}",
                    model.IsAllServices, model.IsAllServiceCategories, model.ServiceId, model.ServiceCategoryId, model.InsurancePlanId);
                
                _logger.Debug("🏥 MEDICAL: Before normalization - IsAllServices: {IsAllServices}, IsAllServiceCategories: {IsAllServiceCategories}, ServiceId: {ServiceId}, ServiceCategoryId: {ServiceCategoryId}",
                    model.IsAllServices, model.IsAllServiceCategories, model.ServiceId, model.ServiceCategoryId);
                
                NormalizeModelStateForAllFlags(ModelState, model);
                
                _logger.Debug("🏥 MEDICAL: After normalization - ModelState.IsValid: {IsValid}, ServiceId: {ServiceId}, ServiceCategoryId: {ServiceCategoryId}",
                    ModelState.IsValid, model.ServiceId, model.ServiceCategoryId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("🏥 MEDICAL: مدل تعرفه بیمه معتبر نیست - CorrelationId: {CorrelationId}, Errors: {@Errors}, User: {UserName} (Id: {UserId})",
                        correlationId, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage), _currentUserService.UserName, _currentUserService.UserId);

                    // 🚀 P0 FIX: بررسی AJAX Request برای JSON Response
                    if (Request.IsAjaxRequest())
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();
                        
                        return Json(new { 
                            success = false, 
                            message = "خطا در اعتبارسنجی فرم",
                            errors = errors,
                            correlationId = correlationId
                        });
                    }

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

                    // 🚀 P0 FIX: بررسی AJAX Request برای JSON Response
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { 
                            success = false, 
                            message = result.Message, 
                            correlationId = correlationId,
                            isBulkOperation = model.IsAllServices,
                            planId = model.InsurancePlanId,
                            serviceId = model.ServiceId
                        });
                    }

                    // بارگیری مجدد SelectLists
                    await LoadSelectListsForCreateEditAsync(model);

                    return View(model);
                }

                if (model.IsAllServices)
                {
                    _logger.Information("🏥 MEDICAL: Bulk Operation با موفقیت تکمیل شد - CorrelationId: {CorrelationId}, CreatedCount: {CreatedCount}, PlanId: {PlanId}, User: {UserName} (Id: {UserId})",
                        correlationId, result.Data, model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddSuccessMessage($"تعرفه برای {result.Data} خدمت با موفقیت ایجاد شد");
                    
                    // 🚀 P0 FIX: بررسی AJAX Request برای JSON Response
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { 
                            success = true, 
                            message = $"تعرفه برای {result.Data} خدمت با موفقیت ایجاد شد",
                            createdCount = result.Data,
                            correlationId = correlationId,
                            redirectUrl = Url.Action("Index", "InsuranceTariff")
                        });
                    }
                    
                    // 🚀 P0 FIX: برای Bulk Create به Index ریدایرکت کن، نه Details
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.Information("🏥 MEDICAL: تعرفه بیمه با موفقیت ایجاد شد - CorrelationId: {CorrelationId}, Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})",
                        correlationId, result.Data, model.InsurancePlanId, model.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddSuccessMessage("تعرفه بیمه با موفقیت ایجاد شد");
                    
                    // 🚀 P0 FIX: بررسی AJAX Request برای JSON Response
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { 
                            success = true, 
                            message = "تعرفه بیمه با موفقیت ایجاد شد",
                            tariffId = result.Data,
                            correlationId = correlationId,
                            redirectUrl = Url.Action("Details", "InsuranceTariff", new { id = result.Data })
                        });
                    }
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

                // 🚀 P0 FIX: بررسی AJAX Request برای JSON Response
                if (Request.IsAjaxRequest())
                {
                    return Json(new { 
                        success = false, 
                        message = "خطای سیستمی در ایجاد تعرفه بیمه",
                        correlationId = correlationId,
                        error = ex.Message
                    });
                }

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

            // 🚀 P0 FIX: Set no-cache headers for medical/financial data
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");

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

            // 🚀 P0 FIX: Set no-cache headers for medical/financial data
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");

            _logger.Information("🏥 MEDICAL: درخواست ویرایش تعرفه بیمه - CorrelationId: {CorrelationId}, Id: {Id}, PlanId: {PlanId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})",
                correlationId, model?.InsuranceTariffId, model?.InsurancePlanId, model?.ServiceId, _currentUserService.UserName, _currentUserService.UserId);

            // 🔍 STRUCTURED LOGGING - تمام مقادیر Form
            _logger.Debug("🔍 EDIT ACTION DEBUG START - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId}), Timestamp: {Timestamp}",
                correlationId, _currentUserService.UserName, _currentUserService.UserId, DateTime.UtcNow);
            
            // Logging Request.Form برای debug (with sensitive data masking)
            _logger.Debug("🔍 Request.Form Keys and Values - CorrelationId: {CorrelationId}", correlationId);
            foreach (string key in Request.Form.AllKeys)
            {
                var value = MaskSensitiveData(key, Request.Form[key]);
                _logger.Debug("🔍   {Key}: '{Value}' - CorrelationId: {CorrelationId}", key, value, correlationId);
            }
            
            // Logging مدل دریافتی
            if (model != null)
            {
                _logger.Debug("🔍 Model Properties - CorrelationId: {CorrelationId}, InsuranceTariffId: {InsuranceTariffId}, DepartmentId: {DepartmentId}, ServiceCategoryId: {ServiceCategoryId}, ServiceId: {ServiceId}, InsuranceProviderId: {InsuranceProviderId}, InsurancePlanId: {InsurancePlanId}, TariffPrice: {TariffPrice}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}, IsActive: {IsActive}",
                    correlationId, model.InsuranceTariffId, model.DepartmentId, model.ServiceCategoryId, model.ServiceId, model.InsuranceProviderId, model.InsurancePlanId, model.TariffPrice, model.PatientShare, model.InsurerShare, model.IsActive);
            }

            // 🔍 MEDICAL: اعتبارسنجی سرور با FluentValidation
            var validator = new InsuranceTariffValidator();
            var fluentValidationResult = await validator.ValidateAsync(model);
            
            if (!fluentValidationResult.IsValid)
            {
                _logger.Warning("🏥 MEDICAL: اعتبارسنجی سرور ناموفق - CorrelationId: {CorrelationId}, Errors: {@Errors}",
                    correlationId, fluentValidationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
                
                foreach (var error in fluentValidationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                
                // بارگذاری مجدد SelectList ها
                await LoadSelectListsForCreateEditAsync(model);
                return View(model);
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

                // 🚀 P1 FIX: Edit mode - no need for All Services logic
                _logger.Information("🏥 MEDICAL: Edit Model received - ServiceId: {ServiceId}, ServiceCategoryId: {ServiceCategoryId}, PlanId: {PlanId}",
                    model.ServiceId, model.ServiceCategoryId, model.InsurancePlanId);
                
                _logger.Debug("🏥 MEDICAL: Edit mode - ServiceId: {ServiceId}, ServiceCategoryId: {ServiceCategoryId}",
                    model.ServiceId, model.ServiceCategoryId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("🏥 MEDICAL: مدل تعرفه بیمه معتبر نیست - CorrelationId: {CorrelationId}, Errors: {@Errors}, User: {UserName} (Id: {UserId})",
                        correlationId, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage), _currentUserService.UserName, _currentUserService.UserId);

                    // 🚀 P0 FIX: بررسی AJAX Request برای JSON Response
                    if (Request.IsAjaxRequest())
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();
                        
                        return Json(new { 
                            success = false, 
                            message = "خطا در اعتبارسنجی فرم",
                            errors = errors,
                            correlationId = correlationId
                        });
                    }

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

                    // 🚀 P1 FIX: انتشار CorrelationId در پاسخ JSON
                    return Json(new { success = false, message = result.Message, correlationId = correlationId }, JsonRequestBehavior.AllowGet);
                }

                _logger.Debug("🏥 MEDICAL: آمار تعرفه‌ها با موفقیت دریافت شد - CorrelationId: {CorrelationId}, Statistics: {@Statistics}",
                    correlationId, result.Data);

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت آمار تعرفه‌ها - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    correlationId, _currentUserService.UserName, _currentUserService.UserId);

                // 🚀 P1 FIX: انتشار CorrelationId در پاسخ JSON
                return Json(new { success = false, message = "خطا در دریافت آمار تعرفه‌ها", correlationId = correlationId }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        #region Helper Methods

        /// <summary>
        /// متمرکزسازی تغییر ModelState برای "همه خدمات/سرفصل‌ها"
        /// </summary>
        private void NormalizeModelStateForAllFlags(ModelStateDictionary modelState, InsuranceTariffCreateEditViewModel model)
        {
            // 🚀 P1 FIX: متمرکزسازی منطق ModelState برای All Services/Categories
            if (model.IsAllServices)
            {
                // حذف خطاهای مربوط به ServiceId و ServiceCategoryId
                modelState.Remove("ServiceId");
                modelState.Remove("ServiceCategoryId");
                
                // تنظیم ServiceId به null
                model.ServiceId = null;
                model.ServiceCategoryId = null;
                
                _logger.Debug("🏥 MEDICAL: ModelState normalized for IsAllServices - ServiceId and ServiceCategoryId cleared");
            }
            else if (model.IsAllServiceCategories)
            {
                // حذف خطای مربوط به ServiceId
                modelState.Remove("ServiceId");
                
                // تنظیم ServiceId به null
                model.ServiceId = null;
                
                _logger.Debug("🏥 MEDICAL: ModelState normalized for IsAllServiceCategories - ServiceId cleared");
            }
        }

        /// <summary>
        /// بارگیری SelectLists برای فیلتر - بهینه‌سازی شده برای محیط درمانی
        /// </summary>
        private async Task LoadSelectListsForFilterAsync(InsuranceTariffFilterViewModel filter)
        {
            try
            {
                // 🚀 P0 FIX: بارگیری موازی SelectLists با بهینه‌سازی
                var clinicId = await GetCurrentClinicIdAsync();
                
                _logger.Debug("🏥 MEDICAL: شروع بارگیری SelectLists - ClinicId: {ClinicId}, User: {UserName} (Id: {UserId})",
                    clinicId, _currentUserService.UserName, _currentUserService.UserId);
                
                var departmentsTask = _departmentManagementService.GetActiveDepartmentsForLookupAsync(clinicId);
                var plansTask = _insurancePlanService.GetActivePlansForLookupAsync();
                var servicesTask = _serviceManagementService.GetActiveServicesForLookupAsync(0);
                var providersTask = _insuranceProviderService.GetActiveProvidersForLookupAsync();

                await Task.WhenAll(departmentsTask, plansTask, servicesTask, providersTask);
                
                _logger.Debug("🏥 MEDICAL: نتایج SelectLists - Departments: {DeptSuccess}, Plans: {PlanSuccess}, Services: {ServiceSuccess}, Providers: {ProviderSuccess}",
                    departmentsTask.Result?.Success, plansTask.Result?.Success, servicesTask.Result?.Success, providersTask.Result?.Success);

                // 🚀 P0 FIX: تنظیم SelectLists جدید برای سازگاری با Viewها
                if (departmentsTask.Result?.Success == true && departmentsTask.Result.Data?.Any() == true)
                {
                    filter.Departments = new SelectList(departmentsTask.Result.Data, "Id", "Name", filter.DepartmentId);
                }
                else
                {
                    filter.Departments = new SelectList(new List<object>(), "Id", "Name");
                }

                if (plansTask.Result?.Success == true && plansTask.Result.Data?.Any() == true)
                {
                    filter.InsurancePlanSelectList = new SelectList(plansTask.Result.Data, "Id", "Name", filter.InsurancePlanId);
                }
                else
                {
                    filter.InsurancePlanSelectList = new SelectList(new List<object>(), "Id", "Name");
                }

                if (servicesTask.Result?.Success == true && servicesTask.Result.Data?.Any() == true)
                {
                    filter.ServiceSelectList = new SelectList(servicesTask.Result.Data, "Id", "Name", filter.ServiceId);
                }
                else
                {
                    filter.ServiceSelectList = new SelectList(new List<object>(), "Id", "Name");
                }

                if (providersTask.Result?.Success == true && providersTask.Result.Data?.Any() == true)
                {
                    filter.InsuranceProviders = new SelectList(providersTask.Result.Data, "Id", "Name", filter.InsuranceProviderId);
                    filter.InsuranceProviderSelectList = new SelectList(providersTask.Result.Data, "Id", "Name", filter.InsuranceProviderId);
                }
                else
                {
                    filter.InsuranceProviders = new SelectList(new List<object>(), "Id", "Name");
                    filter.InsuranceProviderSelectList = new SelectList(new List<object>(), "Id", "Name");
                }

                _logger.Debug("🏥 MEDICAL: SelectLists برای فیلتر با موفقیت بارگیری شدند - Departments: {DeptCount}, Plans: {PlanCount}, Services: {ServiceCount}, Providers: {ProviderCount}",
                    filter.Departments?.Count() ?? 0, filter.InsurancePlanSelectList?.Count() ?? 0, 
                    filter.ServiceSelectList?.Count() ?? 0, filter.InsuranceProviders?.Count() ?? 0);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در بارگیری SelectLists برای فیلتر - User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // 🚀 P0 FIX: تنظیم SelectLists خالی در صورت خطا
                filter.Departments = new SelectList(new List<object>(), "Id", "Name");
                filter.InsurancePlanSelectList = new SelectList(new List<object>(), "Id", "Name");
                filter.ServiceSelectList = new SelectList(new List<object>(), "Id", "Name");
                filter.InsuranceProviders = new SelectList(new List<object>(), "Id", "Name");
                filter.InsuranceProviderSelectList = new SelectList(new List<object>(), "Id", "Name");
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
                await Task.WhenAll(departmentsTask, providersTask);

                // ✅ تنظیم SelectLists اصلی با Error Handling
                if (departmentsTask.Result?.Success == true && departmentsTask.Result.Data?.Any() == true)
                {
                    // تنظیم SelectLists جدید برای سازگاری با Viewها
                    model.Departments = new SelectList(departmentsTask.Result.Data, "Id", "Name", model.DepartmentId);
                model.DepartmentSelectList = new SelectList(departmentsTask.Result.Data, "Id", "Name", model.DepartmentId);
                    _logger.Debug("🏥 MEDICAL: Departments loaded - Count: {Count}, CorrelationId: {CorrelationId}",
                        departmentsTask.Result.Data.Count, correlationId);
                }
                else
                {
                    model.Departments = new SelectList(new List<object>(), "Id", "Name");
                    model.DepartmentSelectList = new SelectList(new List<object>(), "Id", "Name");
                    _logger.Warning("🏥 MEDICAL: No departments found - CorrelationId: {CorrelationId}", correlationId);
                }

                if (providersTask.Result?.Success == true && providersTask.Result.Data?.Any() == true)
                {
                    // تنظیم SelectLists جدید برای سازگاری با Viewها
                    model.InsuranceProviders = new SelectList(providersTask.Result.Data, "Id", "Name", model.InsuranceProviderId);
                    model.InsuranceProviderSelectList = new SelectList(providersTask.Result.Data, "Id", "Name", model.InsuranceProviderId);
                    _logger.Debug("🏥 MEDICAL: Insurance Providers loaded - Count: {Count}, SelectedId: {SelectedId}, CorrelationId: {CorrelationId}",
                        providersTask.Result.Data.Count, model.InsuranceProviderId, correlationId);
                }
                else
                {
                    model.InsuranceProviders = new SelectList(new List<object>(), "Id", "Name");
                    model.InsuranceProviderSelectList = new SelectList(new List<object>(), "Id", "Name");
                    _logger.Warning("🏥 MEDICAL: No insurance providers found - CorrelationId: {CorrelationId}", correlationId);
                }

                // 🚀 FIX: طرح‌های بیمه باید خالی باشند تا بعد از انتخاب ارائه‌دهنده لود شوند
                model.InsurancePlans = new SelectList(new List<object>(), "Id", "Name");
                model.InsurancePlanSelectList = new SelectList(new List<object>(), "Id", "Name");
                _logger.Debug("🏥 MEDICAL: Insurance Plans initialized as empty - will load after provider selection, CorrelationId: {CorrelationId}", correlationId);

                // 🔄 تنظیم SelectLists برای حالت ویرایش
                if (model.InsuranceTariffId > 0) // حالت ویرایش
                {
                    // بارگیری Service Categories برای دپارتمان انتخاب شده
                    if (model.DepartmentId > 0)
                    {
                        var categoriesResult = await _serviceManagementService.GetActiveServiceCategoriesForLookupAsync(model.DepartmentId);
                        if (categoriesResult.Success && categoriesResult.Data?.Any() == true)
                        {
                            model.ServiceCategories = new SelectList(categoriesResult.Data, "Id", "Name", model.ServiceCategoryId);
                            model.ServiceCategorySelectList = new SelectList(categoriesResult.Data, "Id", "Name", model.ServiceCategoryId);
                        }
                    }

                    // بارگیری Services برای دسته‌بندی انتخاب شده
                    if (model.ServiceCategoryId.HasValue && model.ServiceCategoryId > 0)
                    {
                        var servicesResult = await _serviceManagementService.GetActiveServicesForLookupAsync(model.ServiceCategoryId.Value);
                        if (servicesResult.Success && servicesResult.Data?.Any() == true)
                        {
                            // Fix: Use ServiceId only if it's greater than 0
                            var selectedServiceId = model.ServiceId > 0 ? model.ServiceId : (int?)null;
                            model.Services = new SelectList(servicesResult.Data, "Id", "Name", selectedServiceId);
                            model.ServiceSelectList = new SelectList(servicesResult.Data, "Id", "Name", selectedServiceId);
                        }
                    }
                    else
                    {
                        // اگر ServiceCategoryId null است، Services را خالی تنظیم کن
                        model.Services = new SelectList(new List<object>(), "Id", "Name");
                        model.ServiceSelectList = new SelectList(new List<object>(), "Id", "Name");
                    }

                    // بارگیری Insurance Plans برای ارائه‌دهنده انتخاب شده
                    if (model.InsuranceProviderId > 0)
                    {
                        var plansResult = await _insurancePlanService.GetActivePlansForLookupAsync(model.InsuranceProviderId);
                        if (plansResult.Success && plansResult.Data?.Any() == true)
                        {
                            // Fix: Use InsurancePlanId only if it's greater than 0
                            var selectedPlanId = model.InsurancePlanId > 0 ? model.InsurancePlanId : (int?)null;
                            model.InsurancePlans = new SelectList(plansResult.Data, "InsurancePlanId", "Name", selectedPlanId);
                            model.InsurancePlanSelectList = new SelectList(plansResult.Data, "InsurancePlanId", "Name", selectedPlanId);
                        }
                    }
                }
                else // حالت ایجاد
                {
                    // تنظیم SelectLists خالی برای Cascading Dropdowns
                    model.Services = new SelectList(new List<object>(), "Id", "Name");
                    model.ServiceCategories = new SelectList(new List<object>(), "Id", "Name");
                    model.ServiceSelectList = new SelectList(new List<object>(), "Id", "Name");
                    model.ServiceCategorySelectList = new SelectList(new List<object>(), "Id", "Name");
                }

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
            // تنظیم SelectLists جدید برای سازگاری با Viewها
            model.Departments = new SelectList(new List<object>(), "Id", "Name");
            model.ServiceCategories = new SelectList(new List<object>(), "Id", "Name");
            model.Services = new SelectList(new List<object>(), "Id", "Name");
            model.InsuranceProviders = new SelectList(new List<object>(), "Id", "Name");
            model.InsurancePlans = new SelectList(new List<object>(), "Id", "Name");

            // Legacy SelectLists برای سازگاری با کد قدیمی
                model.DepartmentSelectList = new SelectList(new List<object>(), "Id", "Name");
            model.InsurancePlanSelectList = new SelectList(new List<object>(), "Id", "Name");
            model.ServiceSelectList = new SelectList(new List<object>(), "Id", "Name");
            model.InsuranceProviderSelectList = new SelectList(new List<object>(), "Id", "Name");
            model.ServiceCategorySelectList = new SelectList(new List<object>(), "Id", "Name");
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
                if (model.StartDate.HasValue && model.EndDate.HasValue)
                {
                    if (model.StartDate.Value >= model.EndDate.Value)
                    {
                        return ServiceResult.Failed("تاریخ شروع باید قبل از تاریخ پایان باشد", "INVALID_DATE_RANGE", ErrorCategory.Validation);
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
                    CalculationDate = DateTime.UtcNow,
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
                    // 🚀 P0 FIX: پیاده‌سازی بررسی وجود Service
                    var serviceResult = await _serviceManagementService.GetServiceDetailsAsync(model.ServiceId.Value);
                    if (serviceResult?.Success != true || serviceResult.Data == null)
                    {
                        _logger.Warning("🏥 MEDICAL: Service یافت نشد - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}",
                            model.ServiceId, correlationId);
                        
                        return ServiceResult.Failed("خدمت انتخاب شده یافت نشد", "SERVICE_NOT_FOUND", ErrorCategory.Validation);
                    }
                }

                // 🔍 بررسی وجود InsurancePlan
                if (model.InsurancePlanId > 0)
                {
                    // 🚀 P0 FIX: پیاده‌سازی بررسی وجود InsurancePlan
                    var planResult = await _insurancePlanService.GetPlanDetailsAsync(model.InsurancePlanId);
                    if (planResult?.Success != true || planResult.Data == null)
                    {
                        _logger.Warning("🏥 MEDICAL: Insurance Plan یافت نشد - InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}",
                            model.InsurancePlanId, correlationId);
                        
                        return ServiceResult.Failed("طرح بیمه انتخاب شده یافت نشد", "INSURANCE_PLAN_NOT_FOUND", ErrorCategory.Validation);
                    }
                }

                // 🔍 بررسی وجود Department
                if (model.DepartmentId > 0)
                {
                    // 🚀 P0 FIX: پیاده‌سازی بررسی وجود Department
                    var departmentResult = await _departmentManagementService.GetDepartmentDetailsAsync(model.DepartmentId);
                    if (departmentResult?.Success != true || departmentResult.Data == null)
                    {
                        _logger.Warning("🏥 MEDICAL: Department یافت نشد - DepartmentId: {DepartmentId}, CorrelationId: {CorrelationId}",
                            model.DepartmentId, correlationId);
                        
                        return ServiceResult.Failed("دپارتمان انتخاب شده یافت نشد", "DEPARTMENT_NOT_FOUND", ErrorCategory.Validation);
                    }
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

        #region Security Helpers

        /// <summary>
        /// 🔒 ماسک کردن داده‌های حساس در لاگ‌ها
        /// </summary>
        private string MaskSensitiveData(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // فیلدهای غیرحساس که مجاز به لاگ کردن هستند (Whitelist approach - محدودتر)
            var safeKeys = new[]
            {
                "DepartmentId",
                "ServiceCategoryId", 
                "ServiceId",
                "InsuranceProviderId",
                "InsurancePlanId",
                "TariffPrice",
                "PatientShare",
                "InsurerShare",
                "IsActive",
                "IsAllServices",
                "IsAllServiceCategories",
                "PageNumber",
                "PageSize"
                // SearchTerm حذف شد - ممکن است حاوی PII باشد
            };

            // فقط فیلدهای مجاز را لاگ کن
            if (safeKeys.Contains(key))
            {
                return value;
            }

            return "***MASKED***";
        }

        // 🚀 P0 FIX: متدهای قدیمی Idempotency حذف شدند - حالا از IIdempotencyService استفاده می‌شود

        #endregion

        #region Advanced Calculation

        /// <summary>
        /// محاسبه پیشرفته تعرفه بیمه با پشتیبانی از Real-time Calculation
        /// </summary>
        [HttpPost]
        [NoCacheFilter]
        public async Task<JsonResult> CalculateAdvancedTariff(
            int serviceId, 
            int insurancePlanId, 
            int? providerId = null,
            decimal? currentTariffPrice = null,
            decimal? currentPatientShare = null,
            decimal? currentInsurerShare = null,
            decimal? supplementaryCoveragePercent = null,
            decimal? patientSharePercent = null,
            decimal? insurerSharePercent = null,
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
                    
                    // 🚀 P1 FIX: انتشار CorrelationId در پاسخ JSON
                    return Json(new { success = false, message = validationResult.Errors.FirstOrDefault() ?? "ورودی‌های نامعتبر", correlationId = correlationId });
                }

                // 🔍 مرحله 1: دریافت اطلاعات خدمت با لاگ‌گذاری دقیق
                _logger.Information("🏥 MEDICAL: مرحله 1 - شروع دریافت اطلاعات خدمت - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}",
                    serviceId, correlationId);
                
                var serviceTask = _serviceManagementService.GetServiceDetailsAsync(serviceId);
                var serviceResult = await serviceTask;
                
                if (serviceResult?.Success != true || serviceResult.Data == null)
                {
                    _logger.Error("🏥 MEDICAL: خطا در دریافت اطلاعات خدمت - ServiceId: {ServiceId}, Success: {Success}, Message: {Message}, CorrelationId: {CorrelationId}",
                        serviceId, serviceResult?.Success, serviceResult?.Message, correlationId);
                    
                    return Json(new { 
                        success = false, 
                        message = "خطا در دریافت اطلاعات خدمت: " + (serviceResult?.Message ?? "خدمت یافت نشد"), 
                        correlationId = correlationId 
                    });
                }
                
                _logger.Information("🏥 MEDICAL: مرحله 1 - اطلاعات خدمت با موفقیت دریافت شد - ServiceId: {ServiceId}, ServiceName: {ServiceName}, CorrelationId: {CorrelationId}",
                    serviceId, serviceResult.Data.Title, correlationId);

                // 🔍 مرحله 2: دریافت اطلاعات طرح بیمه با لاگ‌گذاری دقیق
                _logger.Information("🏥 MEDICAL: مرحله 2 - شروع دریافت اطلاعات طرح بیمه - InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}",
                    insurancePlanId, correlationId);
                
                var planTask = _insurancePlanService.GetPlanDetailsAsync(insurancePlanId);
                var planResult = await planTask;
                
                if (planResult?.Success != true || planResult.Data == null)
                {
                    _logger.Error("🏥 MEDICAL: خطا در دریافت اطلاعات طرح بیمه - InsurancePlanId: {InsurancePlanId}, Success: {Success}, Message: {Message}, CorrelationId: {CorrelationId}",
                        insurancePlanId, planResult?.Success, planResult?.Message, correlationId);
                    
                    return Json(new { 
                        success = false, 
                        message = "خطا در دریافت طرح بیمه: " + (planResult?.Message ?? "طرح بیمه یافت نشد"), 
                        correlationId = correlationId 
                    });
                }
                
                _logger.Information("🏥 MEDICAL: مرحله 2 - اطلاعات طرح بیمه با موفقیت دریافت شد - InsurancePlanId: {InsurancePlanId}, PlanName: {PlanName}, CorrelationId: {CorrelationId}",
                    insurancePlanId, planResult.Data.Name, correlationId);

                // 🛡️ اعتبارسنجی جامع نتایج سرویس‌ها - ضد گلوله
                _logger.Information("🏥 MEDICAL: مرحله 3 - شروع اعتبارسنجی نتایج سرویس‌ها - CorrelationId: {CorrelationId}",
                    correlationId);
                
                if (serviceResult?.Data == null)
                {
                    _logger.Error("🏥 MEDICAL: داده‌های خدمت null است - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}",
                        serviceId, correlationId);
                    return Json(new { 
                        success = false, 
                        message = "داده‌های خدمت یافت نشد", 
                        correlationId = correlationId 
                    });
                }
                
                if (planResult?.Data == null)
                {
                    _logger.Error("🏥 MEDICAL: داده‌های طرح بیمه null است - InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}",
                        insurancePlanId, correlationId);
                    return Json(new { 
                        success = false, 
                        message = "داده‌های طرح بیمه یافت نشد", 
                        correlationId = correlationId 
                    });
                }
                
                _logger.Information("🏥 MEDICAL: مرحله 3 - اعتبارسنجی نتایج سرویس‌ها موفق - CorrelationId: {CorrelationId}",
                    correlationId);

                // 🚀 PERFORMANCE: Direct access to service and plan data (no more searching through lists)
                _logger.Information("🏥 MEDICAL: Direct access to service and plan data - ServiceId: {ServiceId}, InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}",
                    serviceId, insurancePlanId, correlationId);

                // 🔍 تبدیل به DTO برای Strongly Typed محاسبه
                var serviceDto = new CalculationServiceDto
                {
                    ServiceId = serviceResult.Data.ServiceId,
                    ServiceCategoryId = serviceResult.Data.ServiceCategoryId,
                    Price = serviceResult.Data.Price,
                    Name = serviceResult.Data.Title,
                    Code = serviceResult.Data.ServiceCode ?? "",
                    Description = serviceResult.Data.Description ?? "",
                    IsActive = !serviceResult.Data.IsDeleted,
                    CreatedAt = serviceResult.Data.CreatedAt,
                    UpdatedAt = serviceResult.Data.UpdatedAt
                };

                var planDto = new CalculationPlanDto
                {
                    InsurancePlanId = planResult.Data.InsurancePlanId,
                    CoveragePercent = planResult.Data.CoveragePercent,
                    Name = planResult.Data.Name,
                    PlanCode = planResult.Data.PlanCode ?? "",
                    Description = planResult.Data.Description ?? "",
                    InsuranceProviderId = planResult.Data.InsuranceProviderId,
                    ProviderName = "", // باید از دیتابیس دریافت شود
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                // 🔍 مرحله 4: محاسبه تعرفه با لاگ‌گذاری دقیق
                _logger.Information("🏥 MEDICAL: مرحله 4 - شروع محاسبه تعرفه - ServiceId: {ServiceId}, InsurancePlanId: {InsurancePlanId}, CalculationType: {CalculationType}, CorrelationId: {CorrelationId}",
                    serviceId, insurancePlanId, calculationType, correlationId);
                
                var calculationResult = await PerformAdvancedCalculationAsync(
                    serviceDto, planDto, currentTariffPrice, currentPatientShare, 
                    currentInsurerShare, supplementaryCoveragePercent, patientSharePercent, insurerSharePercent,
                    calculationType, correlationId);

                var duration = DateTime.UtcNow - startTime;
                _logger.Information("🏥 MEDICAL: مرحله 4 - محاسبه تعرفه تکمیل شد - CorrelationId: {CorrelationId}, Duration: {Duration}ms, User: {UserName} (Id: {UserId})",
                    correlationId, duration.TotalMilliseconds, _currentUserService.UserName, _currentUserService.UserId);

                // 🔍 مرحله 5: آماده‌سازی پاسخ با لاگ‌گذاری دقیق
                _logger.Information("🏥 MEDICAL: مرحله 5 - آماده‌سازی پاسخ - CorrelationId: {CorrelationId}",
                    correlationId);
                
                // 🚀 P1 FIX: استفاده از ApiResponse استاندارد
                var response = ApiResponse<object>.CreateSuccess(
                    calculationResult, 
                    "محاسبه با موفقیت انجام شد", 
                    correlationId, 
                    (long)duration.TotalMilliseconds);
                
                _logger.Information("🏥 MEDICAL: مرحله 5 - پاسخ آماده شد - CorrelationId: {CorrelationId}, Success: {Success}",
                    correlationId, response.Success);
                
                return Json(response);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در محاسبه پیشرفته - CorrelationId: {CorrelationId}, Duration: {Duration}ms, ServiceId: {ServiceId}, InsurancePlanId: {InsurancePlanId}, User: {UserName} (Id: {UserId})",
                    correlationId, duration.TotalMilliseconds, serviceId, insurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                
                // 🔍 لاگ‌گذاری دقیق خطا برای تشخیص مشکل
                _logger.Error("🏥 MEDICAL: جزئیات خطا - ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}, CorrelationId: {CorrelationId}",
                    ex.GetType().Name, ex.Message, ex.StackTrace, correlationId);
                
                // 🚀 P1 FIX: استفاده از ApiResponse استاندارد
                var response = ApiResponse.CreateError(
                    "خطا در محاسبه تعرفه. لطفاً دوباره تلاش کنید.", 
                    correlationId, 
                    null, 
                    (long)duration.TotalMilliseconds);
                
                return Json(response);
            }
        }

        /// <summary>
        /// انجام محاسبه پیشرفته تعرفه - Strongly Typed
        /// </summary>
        private async Task<CalculationResultDto> PerformAdvancedCalculationAsync(
            CalculationServiceDto service, CalculationPlanDto insurancePlan, decimal? currentTariffPrice,
            decimal? currentPatientShare, decimal? currentInsurerShare,
            decimal? supplementaryCoveragePercent, decimal? patientSharePercent, decimal? insurerSharePercent,
            string calculationType, string correlationId)
        {
            try
            {
                _logger.Debug("🏥 MEDICAL: شروع محاسبه پیشرفته - ServiceId: {ServiceId}, PlanId: {PlanId}, Type: {Type}, CorrelationId: {CorrelationId}",
                    service.ServiceId, insurancePlan.InsurancePlanId, calculationType, correlationId);

                // 🔍 محاسبه قیمت تعرفه با استفاده از FactorSetting
                var tariffPrice = await _tariffCalculationService.CalculateTariffPriceWithFactorSettingAsync(service.ServiceId, currentTariffPrice, correlationId);
                
                // 🔍 محاسبه سهم بیمه با استفاده از PlanService
                var insurerShare = await CalculateInsurerShareWithPlanServiceAsync(service.ServiceId, insurancePlan.InsurancePlanId, tariffPrice, currentInsurerShare, correlationId);
                
                // 🔍 محاسبه سهم بیمار
                var patientShare = await CalculatePatientShareAsync(service.ServiceId, insurancePlan.InsurancePlanId, tariffPrice, insurerShare, currentPatientShare, correlationId);
                
                // 🔍 محاسبه پوشش تکمیلی
                var supplementaryCoverage = await CalculateSupplementaryCoverageAsync(
                    service.ServiceId, insurancePlan.InsurancePlanId, tariffPrice, insurerShare, supplementaryCoveragePercent, correlationId);
                
                // 🔍 محاسبه پوشش کل
                var totalCoveragePercent = await CalculateTotalCoverageAsync(
                    tariffPrice, insurerShare, supplementaryCoverage, correlationId);

                // 🔍 محاسبه درصدها بر اساس مقادیر محاسبه شده
                var calculatedPatientSharePercent = tariffPrice > 0 ? (patientShare / tariffPrice) * 100m : 0m;
                var calculatedInsurerSharePercent = tariffPrice > 0 ? (insurerShare / tariffPrice) * 100m : 0m;

                // 🔍 اعمال درصدهای ورودی کاربر (اگر ارائه شده باشند)
                if (patientSharePercent.HasValue && insurerSharePercent.HasValue)
                {
                    // اعتبارسنجی: مجموع درصدها نباید بیش از 100 باشد
                    if (patientSharePercent.Value + insurerSharePercent.Value > 100m)
                    {
                        _logger.Warning("🏥 MEDICAL: مجموع درصدهای ورودی بیش از 100 است - PatientPercent: {PatientPercent}%, InsurerPercent: {InsurerPercent}%, Sum: {Sum}%, CorrelationId: {CorrelationId}",
                            patientSharePercent.Value, insurerSharePercent.Value, patientSharePercent.Value + insurerSharePercent.Value, correlationId);
                        
                        return new CalculationResultDto
                        {
                            IsSuccess = false,
                            ErrorMessage = "مجموع درصدها نمی‌تواند بیش از 100 باشد",
                            CorrelationId = correlationId,
                            CalculatedAt = DateTime.UtcNow
                        };
                    }

                    // محاسبه مجدد سهم‌ها بر اساس درصدهای ورودی
                    patientShare = Math.Round(tariffPrice * (patientSharePercent.Value / 100m), 0, MidpointRounding.AwayFromZero);
                    insurerShare = Math.Round(tariffPrice * (insurerSharePercent.Value / 100m), 0, MidpointRounding.AwayFromZero);
                    
                    _logger.Information("🏥 MEDICAL: محاسبه مجدد سهم‌ها بر اساس درصدهای ورودی - PatientPercent: {PatientPercent}%, InsurerPercent: {InsurerPercent}%, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}, CorrelationId: {CorrelationId}",
                        patientSharePercent.Value, insurerSharePercent.Value, patientShare, insurerShare, correlationId);
                }

                var result = new CalculationResultDto
                {
                    TariffPrice = tariffPrice,
                    PatientShare = patientShare,
                    InsurerShare = insurerShare,
                    SupplementaryCoveragePercent = supplementaryCoverage,
                    PrimaryCoveragePercent = totalCoveragePercent,
                    PatientSharePercent = calculatedPatientSharePercent,
                    InsurerSharePercent = calculatedInsurerSharePercent,
                    CalculationType = calculationType,
                    CalculatedAt = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    IsSuccess = true,
                    Service = service,
                    InsurancePlan = insurancePlan
                };

                _logger.Debug("🏥 MEDICAL: محاسبه پیشرفته تکمیل شد - CorrelationId: {CorrelationId}, Result: {@Result}",
                    correlationId, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه پیشرفته - CorrelationId: {CorrelationId}", correlationId);
                
                return new CalculationResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    CorrelationId = correlationId,
                    CalculatedAt = DateTime.UtcNow,
                    Service = service,
                    InsurancePlan = insurancePlan
                };
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
                
                // 🚀 P0 FIX: منطق پوشش تکمیلی در کنترلر جداگانه (SupplementaryTariffController) پیاده‌سازی شده است
                // این متد فقط برای سازگاری با API موجود باقی مانده و مقدار 0 برمی‌گرداند

                _logger.Debug("🏥 MEDICAL: محاسبه پوشش تکمیلی - Result: {Result}, CorrelationId: {CorrelationId}",
                    calculatedPercent, correlationId);

                return Math.Round(calculatedPercent, 0, MidpointRounding.AwayFromZero);
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

                return Math.Round(totalCoverage, 0, MidpointRounding.AwayFromZero);
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

                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت آمار سریع - CorrelationId: {CorrelationId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    // 🚀 P1 FIX: انتشار CorrelationId در پاسخ JSON
                    return Json(new { success = false, message = result.Message, correlationId = correlationId }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای غیرمنتظره در دریافت آمار سریع - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    correlationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در دریافت آمار" }, JsonRequestBehavior.AllowGet);
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
            // 🚀 P0 FIX: یکنواخت‌سازی هدرهای Cache
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddSeconds(-1));
            Response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);

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
                    // 🚀 PERFORMANCE: Server-side filtering and paging
                    var allServices = result.Data.AsQueryable();
                    
                    // 🔍 اعمال فیلتر جستجو
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        var searchLower = search.ToLower();
                        allServices = allServices.Where(s => 
                            (s.Name ?? "").ToLower().Contains(searchLower) ||
                            (s.Code ?? "").ToLower().Contains(searchLower) ||
                            (s.Description ?? "").ToLower().Contains(searchLower)
                        );
                    }
                    
                    // 📊 محاسبه آمار
                    var totalCount = allServices.Count();
                    var hasMore = (page * pageSize) < totalCount;
                    
                    // 🔄 اعمال paging
                    var services = allServices
                        .OrderBy(s => s.Name)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(s => new { 
                            id = s.Id, 
                            name = s.Name,
                            code = s.Code ?? "",
                            description = s.Description ?? ""
                        }).ToList();
                    
                    _logger.Information("🏥 MEDICAL: خدمات با موفقیت دریافت شدند - Count: {Count}, Total: {Total}, Page: {Page}, PageSize: {PageSize}, HasMore: {HasMore}, ServiceCategoryId: {ServiceCategoryId}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                        services.Count, totalCount, page, pageSize, hasMore, serviceCategoryId, duration.TotalMilliseconds, correlationId);
                    
                    return Json(new { 
                        success = true, 
                        data = services,
                        hasMore = hasMore,
                        totalCount = totalCount,
                        page = page,
                        pageSize = pageSize,
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
            var correlationId = "search_services_" + DateTime.Now.Ticks + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("🔍 MEDICAL: درخواست جستجوی خدمات - SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}, User: {UserName} (Id: {UserId}), CorrelationId: {CorrelationId}",
                    searchTerm, page, pageSize, _currentUserService.UserName, _currentUserService.UserId, correlationId);

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
                    // 🚀 P1 FIX: انتشار CorrelationId در پاسخ JSON
                    return Json(new { success = false, message = result.Message, correlationId = correlationId }, JsonRequestBehavior.AllowGet);
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
            var correlationId = "get_providers_" + DateTime.Now.Ticks + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت ارائه‌دهندگان بیمه - Search: {Search}, Page: {Page}, PageSize: {PageSize}, User: {UserName} (Id: {UserId}), CorrelationId: {CorrelationId}",
                    search, page, pageSize, _currentUserService.UserName, _currentUserService.UserId, correlationId);

                var result = await _insuranceProviderService.GetActiveProvidersForLookupAsync();
                
                _logger.Information("🏥 MEDICAL: نتیجه دریافت ارائه‌دهندگان بیمه - Success: {Success}, Message: {Message}, DataCount: {DataCount}",
                    result.Success, result.Message, result.Data?.Count ?? 0);
                
                if (result.Success)
                {
                    var allProviders = result.Data.Select(p => new { 
                        id = p.Id, 
                        name = p.Name,
                        description = p.Name // می‌تواند از دیتابیس دریافت شود
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
                    // 🚀 P1 FIX: انتشار CorrelationId در پاسخ JSON
                    return Json(new { success = false, message = result.Message, correlationId = correlationId }, JsonRequestBehavior.AllowGet);
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
            var correlationId = "get_plans_" + DateTime.Now.Ticks + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت طرح‌های بیمه - ProviderId: {ProviderId}, User: {UserName} (Id: {UserId}), CorrelationId: {CorrelationId}",
                    providerId, _currentUserService.UserName, _currentUserService.UserId, correlationId);

                var result = await _insurancePlanService.GetActivePlansForLookupAsync(providerId);
                
                _logger.Information("🏥 MEDICAL: نتیجه دریافت طرح‌های بیمه - Success: {Success}, Message: {Message}, DataCount: {DataCount}",
                    result.Success, result.Message, result.Data?.Count ?? 0);
                
                if (result.Success)
                {
                    // 🚀 PERFORMANCE: Server-side filtering and paging
                    var allPlans = result.Data.AsQueryable();
                    
                    // 🔍 اعمال فیلتر جستجو
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        var searchLower = search.ToLower();
                        allPlans = allPlans.Where(p => 
                            (p.Name ?? "").ToLower().Contains(searchLower) ||
                            (p.PlanCode ?? "").ToLower().Contains(searchLower) ||
                            (p.InsuranceProviderName ?? "").ToLower().Contains(searchLower)
                        );
                    }
                    
                    // 📊 محاسبه آمار
                    var totalCount = allPlans.Count();
                    var hasMore = (page * pageSize) < totalCount;
                    
                    // 🔄 اعمال paging
                    var plans = allPlans
                        .OrderBy(p => p.Name)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(p => new { 
                            id = p.InsurancePlanId, 
                            name = p.Name,
                            planCode = p.PlanCode ?? "",
                            coveragePercent = p.CoveragePercent,
                            providerName = p.InsuranceProviderName ?? "",
                            InsurancePlanId = p.InsurancePlanId,  // 🚀 FIX: اضافه کردن InsurancePlanId
                            Value = p.InsurancePlanId,  // اضافه کردن Value برای سازگاری
                            Text = p.Name               // اضافه کردن Text برای سازگاری
                        }).ToList();
                    
                    // 🔍 Debug logging برای بررسی plans
                    _logger.Information("🏥 MEDICAL: طرح‌های بیمه با موفقیت دریافت شدند - Count: {Count}, Total: {Total}, Page: {Page}, PageSize: {PageSize}, HasMore: {HasMore}, ProviderId: {ProviderId}, User: {UserName} (Id: {UserId})",
                        plans.Count, totalCount, page, pageSize, hasMore, providerId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = true, 
                        data = plans,
                        hasMore = hasMore,
                        totalCount = totalCount,
                        page = page,
                        pageSize = pageSize
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت طرح‌های بیمه - ProviderId: {ProviderId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        providerId, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    // 🚀 P1 FIX: انتشار CorrelationId در پاسخ JSON
                    return Json(new { success = false, message = result.Message, correlationId = correlationId }, JsonRequestBehavior.AllowGet);
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
                calculatedBaseAmount = await _tariffCalculationService.CalculateServiceBasePriceAsync(service.ServiceId);
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
                        calculatedAmount = Math.Round(calculatedBaseAmount, 0, MidpointRounding.AwayFromZero),
                        patientShare = Math.Round(patientShare, 0, MidpointRounding.AwayFromZero),
                        insurerShare = Math.Round(insurerShare, 0, MidpointRounding.AwayFromZero),
                        coveragePercent = plan.CoveragePercent,
                        patientPercent = Math.Round(patientPercent * 100, 0, MidpointRounding.AwayFromZero),
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
        private (bool IsValid, string ErrorMessage) ValidateServiceResults(ServiceResult<ViewModels.ServiceDetailsViewModel> serviceResult, ServiceResult<ViewModels.Insurance.InsurancePlan.InsurancePlanDetailsViewModel> planResult, int serviceId, int insurancePlanId, string correlationId)
        {
            try
            {
                // 🔍 اعتبارسنجی ServiceResult
                if (serviceResult?.Success != true)
                {
                    var errorMsg = $"خطا در دریافت خدمت: {serviceResult?.Message ?? "نامشخص"}";
                    _logger.Warning("🏥 MEDICAL: ServiceResult ناموفق - {Error}, CorrelationId: {CorrelationId}", errorMsg, correlationId);
                    return (false, errorMsg);
                }

                if (serviceResult.Data == null)
                {
                    var errorMsg = "خدمت مورد نظر یافت نشد";
                    _logger.Warning("🏥 MEDICAL: خدمت مورد نظر یافت نشد - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}", serviceId, correlationId);
                    return (false, errorMsg);
                }

                // 🔍 اعتبارسنجی PlanResult
                if (planResult?.Success != true)
                {
                    var errorMsg = $"خطا در دریافت طرح بیمه: {planResult?.Message ?? "نامشخص"}";
                    _logger.Warning("🏥 MEDICAL: PlanResult ناموفق - {Error}, CorrelationId: {CorrelationId}", errorMsg, correlationId);
                    return (false, errorMsg);
                }

                if (planResult.Data == null)
                {
                    var errorMsg = "طرح بیمه مورد نظر یافت نشد";
                    _logger.Warning("🏥 MEDICAL: طرح بیمه مورد نظر یافت نشد - InsurancePlanId: {InsurancePlanId}, CorrelationId: {CorrelationId}", insurancePlanId, correlationId);
                    return (false, errorMsg);
                }

                // 🔍 اعتبارسنجی تطابق ServiceId
                if (serviceResult.Data.ServiceId != serviceId)
                {
                    var errorMsg = $"خدمت با شناسه {serviceId} یافت نشد (دریافت شده: {serviceResult.Data.ServiceId})";
                    _logger.Warning("🏥 MEDICAL: ServiceId تطابق ندارد - درخواست: {RequestedServiceId}, دریافت شده: {ReceivedServiceId}, CorrelationId: {CorrelationId}", 
                        serviceId, serviceResult.Data.ServiceId, correlationId);
                    return (false, errorMsg);
                }

                // 🔍 اعتبارسنجی تطابق InsurancePlanId
                if (planResult.Data.InsurancePlanId != insurancePlanId)
                {
                    var errorMsg = $"طرح بیمه با شناسه {insurancePlanId} یافت نشد (دریافت شده: {planResult.Data.InsurancePlanId})";
                    _logger.Warning("🏥 MEDICAL: InsurancePlanId تطابق ندارد - درخواست: {RequestedPlanId}, دریافت شده: {ReceivedPlanId}, CorrelationId: {CorrelationId}", 
                        insurancePlanId, planResult.Data.InsurancePlanId, correlationId);
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
