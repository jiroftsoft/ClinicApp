using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using ClinicApp.Interfaces;
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
    /// 9. Performance Optimization با Caching
    /// 10. Comprehensive Error Handling
    /// 
    /// نکته حیاتی: این کنترلر بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    //[Authorize(Roles = "Admin,InsuranceManager")]
    [MedicalEnvironmentFilter]
    public class InsuranceTariffController : BaseController
    {
        #region Dependencies and Constructor

        private readonly IInsuranceTariffService _insuranceTariffService;
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly IServiceManagementService _serviceManagementService;
        private readonly IDepartmentManagementService _departmentManagementService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;
        private readonly IMessageNotificationService _messageNotificationService;
        private readonly IValidator<InsuranceTariffCreateEditViewModel> _createEditValidator;
        private readonly IValidator<InsuranceTariffFilterViewModel> _filterValidator;

        public InsuranceTariffController(
            IInsuranceTariffService insuranceTariffService,
            IInsurancePlanService insurancePlanService,
            IServiceManagementService serviceManagementService,
            IDepartmentManagementService departmentManagementService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings,
            IMessageNotificationService messageNotificationService,
            IValidator<InsuranceTariffCreateEditViewModel> createEditValidator,
            IValidator<InsuranceTariffFilterViewModel> filterValidator)
            : base(messageNotificationService)
        {
            _insuranceTariffService = insuranceTariffService ?? throw new ArgumentNullException(nameof(insuranceTariffService));
            _insurancePlanService = insurancePlanService ?? throw new ArgumentNullException(nameof(insurancePlanService));
            _serviceManagementService = serviceManagementService ?? throw new ArgumentNullException(nameof(serviceManagementService));
            _departmentManagementService = departmentManagementService ?? throw new ArgumentNullException(nameof(departmentManagementService));
            _logger = logger.ForContext<InsuranceTariffController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _messageNotificationService = messageNotificationService ?? throw new ArgumentNullException(nameof(messageNotificationService));
            _createEditValidator = createEditValidator ?? throw new ArgumentNullException(nameof(createEditValidator));
            _filterValidator = filterValidator ?? throw new ArgumentNullException(nameof(filterValidator));
        }

        #endregion

        #region Properties and Constants

        private int PageSize => _appSettings.DefaultPageSize;
        private const string CACHE_KEY_PLANS = "InsurancePlans_Active";
        private const string CACHE_KEY_SERVICES = "Services_Active";
        private const string CACHE_KEY_PROVIDERS = "InsuranceProviders_Active";
        private const int CACHE_DURATION_MINUTES = 30;

        #endregion

        #region Index & Search Operations

        /// <summary>
        /// نمایش صفحه اصلی تعرفه‌های بیمه با آمار کامل و فیلترهای پیشرفته
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 300, VaryByParam = "searchTerm;planId;serviceId;providerId;page")]
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
        [OutputCache(Duration = 300, VaryByParam = "id", Location = OutputCacheLocation.Server)]
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
        public async Task<ActionResult> Create(int? planId = null, int? serviceId = null, int? providerId = null)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: درخواست فرم ایجاد تعرفه بیمه - CorrelationId: {CorrelationId}, PlanId: {PlanId}, ServiceId: {ServiceId}, ProviderId: {ProviderId}, User: {UserName} (Id: {UserId})",
                correlationId, planId, serviceId, providerId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var model = new InsuranceTariffCreateEditViewModel
                {
                    InsurancePlanId = planId ?? 0,
                    InsuranceProviderId = providerId ?? 0,
                    ServiceId = serviceId,
                    IsActive = true
                };

                // بارگیری SelectLists
                await LoadSelectListsForCreateEditAsync(model);

                _logger.Information("🏥 MEDICAL: فرم ایجاد تعرفه بیمه آماده شد - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                    correlationId, _currentUserService.UserName, _currentUserService.UserId);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در آماده‌سازی فرم ایجاد تعرفه بیمه - CorrelationId: {CorrelationId}, PlanId: {PlanId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})",
                    correlationId, planId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در آماده‌سازی فرم ایجاد تعرفه بیمه");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش فرم ایجاد تعرفه بیمه جدید - بهینه‌سازی شده برای محیط درمانی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(InsuranceTariffCreateEditViewModel model)
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            // Logging کامل درخواست
            _logger.Information("🏥 MEDICAL: شروع درخواست ایجاد تعرفه بیمه - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                correlationId, _currentUserService.UserName, _currentUserService.UserId);

            // 🔍 CONSOLE LOGGING - تمام مقادیر Form
            System.Console.WriteLine("🔍 ===== CREATE ACTION DEBUG START =====");
            System.Console.WriteLine($"🔍 CorrelationId: {correlationId}");
            System.Console.WriteLine($"🔍 User: {_currentUserService.UserName} (Id: {_currentUserService.UserId})");
            System.Console.WriteLine($"🔍 Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");
            
            // Logging Request.Form برای debug
            System.Console.WriteLine("🔍 Request.Form Keys and Values:");
            foreach (string key in Request.Form.AllKeys)
            {
                System.Console.WriteLine($"🔍   {key}: '{Request.Form[key]}'");
            }
            
            // Logging مدل دریافتی
            if (model != null)
            {
                System.Console.WriteLine("🔍 Model Properties:");
                System.Console.WriteLine($"🔍   InsuranceTariffId: {model.InsuranceTariffId}");
                System.Console.WriteLine($"🔍   DepartmentId: {model.DepartmentId}");
                System.Console.WriteLine($"🔍   ServiceCategoryId: {model.ServiceCategoryId}");
                System.Console.WriteLine($"🔍   ServiceId: {model.ServiceId}");
                System.Console.WriteLine($"🔍   InsuranceProviderId: {model.InsuranceProviderId}");
                System.Console.WriteLine($"🔍   InsurancePlanId: {model.InsurancePlanId}");
                System.Console.WriteLine($"🔍   TariffPrice: {model.TariffPrice}");
                System.Console.WriteLine($"🔍   PatientShare: {model.PatientShare}");
                System.Console.WriteLine($"🔍   InsurerShare: {model.InsurerShare}");
                System.Console.WriteLine($"🔍   IsActive: {model.IsActive}");
                System.Console.WriteLine($"🔍   IsAllServices: {model.IsAllServices}");
                System.Console.WriteLine($"🔍   IsAllServiceCategories: {model.IsAllServiceCategories}");
                
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
                System.Console.WriteLine("🔍 ❌ Model is NULL!");
                _logger.Warning("🏥 MEDICAL: مدل null است - CorrelationId: {CorrelationId}", correlationId);
            }
            
            System.Console.WriteLine("🔍 ===== CREATE ACTION DEBUG END =====");

            try
            {
                // اعتبارسنجی مدل
                if (model == null)
                {
                    _logger.Warning("🏥 MEDICAL: مدل تعرفه بیمه null است - CorrelationId: {CorrelationId}", correlationId);
                    _messageNotificationService.AddErrorMessage("اطلاعات تعرفه بیمه ارسال نشده است");
                    return RedirectToAction("Create");
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
        /// دریافت آمار تعرفه‌های بیمه به صورت AJAX
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 300)]
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

        /// <summary>
        /// بررسی وجود تعرفه برای طرح بیمه و خدمت مشخص
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("CheckTariffExists")]
        public async Task<JsonResult> CheckTariffExists(int planId, int serviceId, int? excludeId = null)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.Information("🏥 MEDICAL: بررسی وجود تعرفه - CorrelationId: {CorrelationId}, PlanId: {PlanId}, ServiceId: {ServiceId}, ExcludeId: {ExcludeId}, User: {UserName} (Id: {UserId})",
                correlationId, planId, serviceId, excludeId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceTariffService.CheckTariffExistsAsync(planId, serviceId, excludeId);
                if (!result.Success)
                {
                    _logger.Warning("🏥 MEDICAL: خطا در بررسی وجود تعرفه - CorrelationId: {CorrelationId}, PlanId: {PlanId}, ServiceId: {ServiceId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        correlationId, planId, serviceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = result.Message });
                }

                _logger.Debug("🏥 MEDICAL: بررسی وجود تعرفه انجام شد - CorrelationId: {CorrelationId}, PlanId: {PlanId}, ServiceId: {ServiceId}, Exists: {Exists}",
                    correlationId, planId, serviceId, result.Data);

                return Json(new { success = true, exists = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در بررسی وجود تعرفه - CorrelationId: {CorrelationId}, PlanId: {PlanId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})",
                    correlationId, planId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در بررسی وجود تعرفه" });
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
                var providersTask = _insurancePlanService.GetActiveProvidersForLookupAsync();

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
        private async Task LoadSelectListsForCreateEditAsync(InsuranceTariffCreateEditViewModel model)
        {
            try
            {
                // بارگیری موازی SelectLists
                var departmentsTask = _departmentManagementService.GetActiveDepartmentsForLookupAsync(1); // TODO: Get current clinic ID from user context
                var plansTask = _insurancePlanService.GetActivePlansForLookupAsync();
                var servicesTask = _serviceManagementService.GetActiveServicesForLookupAsync(0);
                var providersTask = _insurancePlanService.GetActiveProvidersForLookupAsync();

                await Task.WhenAll(departmentsTask, plansTask, servicesTask, providersTask);

                model.DepartmentSelectList = new SelectList(departmentsTask.Result.Data, "Id", "Name", model.DepartmentId);
                model.InsurancePlanSelectList = new SelectList(plansTask.Result.Data, "Value", "Text", model.InsurancePlanId);
                model.ServiceSelectList = new SelectList(servicesTask.Result.Data, "Value", "Text", model.ServiceId);
                model.InsuranceProviderSelectList = new SelectList(providersTask.Result.Data, "Value", "Text", model.InsuranceProviderId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در بارگیری SelectLists برای فرم - User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // تنظیم SelectLists خالی در صورت خطا
                model.DepartmentSelectList = new SelectList(new List<object>(), "Id", "Name");
                model.InsurancePlanSelectList = new SelectList(new List<object>(), "Value", "Text");
                model.ServiceSelectList = new SelectList(new List<object>(), "Value", "Text");
                model.InsuranceProviderSelectList = new SelectList(new List<object>(), "Value", "Text");
            }
        }

        /// <summary>
        /// اعتبارسنجی قوی برای InsuranceTariffCreateEditViewModel
        /// </summary>
        private async Task<ServiceResult> ValidateCreateEditModelAsync(InsuranceTariffCreateEditViewModel model)
        {
            try
            {
                var validationResult = await _createEditValidator.ValidateAsync(model);
                
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Warning("🏥 MEDICAL: اعتبارسنجی ناموفق - User: {UserName} (Id: {UserId}), Errors: {Errors}",
                        _currentUserService.UserName, _currentUserService.UserId, string.Join(", ", errors));
                    
                    var validationErrors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage, e.ErrorCode)).ToList();
                    return ServiceResult.FailedWithValidationErrors("خطاهای اعتبارسنجی", validationErrors);
                }

                return ServiceResult.Successful("اعتبارسنجی موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی - User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult.Failed("خطا در اعتبارسنجی داده‌ها", "VALIDATION_ERROR", ErrorCategory.Validation);
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
        /// دریافت آمار سریع برای dashboard
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 60, Location = OutputCacheLocation.Server)]
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
        /// دریافت دپارتمان‌ها برای cascade dropdown
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDepartments()
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت دپارتمان‌ها - User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // دریافت دپارتمان‌ها از service موجود
                var result = await _departmentManagementService.GetActiveDepartmentsForLookupAsync(1); // TODO: Get current clinic ID from user context
                
                if (result.Success)
                {
                    var departments = result.Data.Select(d => new { id = d.Id, name = d.Name }).ToList();
                    return Json(new { success = true, data = departments }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت دپارتمان‌ها - Error: {Error}",
                        result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت دپارتمان‌ها - User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در دریافت دپارتمان‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت سرفصل‌های خدماتی برای دپارتمان
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategories(int departmentId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت سرفصل‌های خدماتی - DepartmentId: {DepartmentId}, User: {UserName} (Id: {UserId})",
                    departmentId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت سرفصل‌های خدماتی از service موجود
                var result = await _serviceManagementService.GetActiveServiceCategoriesForLookupAsync(departmentId);
                
                if (result.Success)
                {
                    var categories = result.Data.Select(c => new { id = c.Id, name = c.Name }).ToList();
                    return Json(new { success = true, data = categories }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت سرفصل‌های خدماتی - DepartmentId: {DepartmentId}, Error: {Error}",
                        departmentId, result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت سرفصل‌های خدماتی - DepartmentId: {DepartmentId}, User: {UserName} (Id: {UserId})",
                    departmentId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در دریافت سرفصل‌های خدماتی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت خدمات برای سرفصل خدماتی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServices(int serviceCategoryId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت خدمات - ServiceCategoryId: {ServiceCategoryId}, User: {UserName} (Id: {UserId})",
                    serviceCategoryId, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _serviceManagementService.GetActiveServicesForLookupAsync(serviceCategoryId);
                
                if (result.Success)
                {
                    var services = result.Data.Select(s => new { id = s.Id, name = s.Name }).ToList();
                    _logger.Information("🏥 MEDICAL: خدمات با موفقیت دریافت شدند - Count: {Count}, ServiceCategoryId: {ServiceCategoryId}, User: {UserName} (Id: {UserId})",
                        services.Count, serviceCategoryId, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = true, data = services }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت خدمات - ServiceCategoryId: {ServiceCategoryId}, Error: {Error}, User: {UserName} (Id: {UserId})",
                        serviceCategoryId, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت خدمات - ServiceCategoryId: {ServiceCategoryId}, User: {UserName} (Id: {UserId})",
                    serviceCategoryId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در دریافت خدمات" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه برای ارائه‌دهنده
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetInsurancePlans(int? providerId = null)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت طرح‌های بیمه - ProviderId: {ProviderId}, User: {UserName} (Id: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _insurancePlanService.GetActivePlansForLookupAsync(providerId);
                
                if (result.Success)
                {
                    var plans = result.Data.Select(p => new { id = p.InsurancePlanId, name = p.Name }).ToList();
                    _logger.Information("🏥 MEDICAL: طرح‌های بیمه با موفقیت دریافت شدند - Count: {Count}, ProviderId: {ProviderId}, User: {UserName} (Id: {UserId})",
                        plans.Count, providerId, _currentUserService.UserName, _currentUserService.UserId);
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

                return Json(new { success = false, message = "خطا در دریافت طرح‌های بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت ارائه‌دهندگان بیمه
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetInsuranceProviders()
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت ارائه‌دهندگان بیمه - User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var result = await _insurancePlanService.GetActiveProvidersForLookupAsync();
                
                if (result.Success)
                {
                    var providers = result.Data.Select(p => new { id = p.InsuranceProviderId, name = p.Name }).ToList();
                    _logger.Information("🏥 MEDICAL: ارائه‌دهندگان بیمه با موفقیت دریافت شدند - Count: {Count}, User: {UserName} (Id: {UserId})",
                        providers.Count, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = true, data = providers }, JsonRequestBehavior.AllowGet);
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

                return Json(new { success = false, message = "خطا در دریافت ارائه‌دهندگان بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
#endregion  