using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Services.Insurance;
using ClinicApp.Models.Entities;
using ClinicApp.Services;
using ClinicApp.ViewModels.Insurance.InsurancePlan;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر مدیریت طرح‌های بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل طرح‌های بیمه (Basic, Standard, Premium, Supplementary)
    /// 2. استفاده از Anti-Forgery Token در همه POST actions
    /// 3. استفاده از ServiceResult Enhanced pattern
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. مدیریت روابط با InsuranceProvider
    /// 7. رعایت استانداردهای پزشکی ایران
    /// 
    /// نکته حیاتی: این کنترلر بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class InsurancePlanController : Controller
    {
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly IInsuranceProviderService _insuranceProviderService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;
        private readonly IMessageNotificationService _messageNotificationService;
        private readonly IInsurancePlanDependencyService _dependencyService;

        public InsurancePlanController(
            IInsurancePlanService insurancePlanService,
            IInsuranceProviderService insuranceProviderService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings,
            IMessageNotificationService messageNotificationService,
            IInsurancePlanDependencyService dependencyService)
        {
            _insurancePlanService = insurancePlanService ?? throw new ArgumentNullException(nameof(insurancePlanService));
            _insuranceProviderService = insuranceProviderService ?? throw new ArgumentNullException(nameof(insuranceProviderService));
            _log = logger.ForContext<InsurancePlanController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _messageNotificationService = messageNotificationService ?? throw new ArgumentNullException(nameof(messageNotificationService));
            _dependencyService = dependencyService ?? throw new ArgumentNullException(nameof(dependencyService));
        }

        private int PageSize => _appSettings.DefaultPageSize;

        #region Index & Search

        /// <summary>
        /// نمایش صفحه اصلی طرح‌های بیمه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(string searchTerm = "", int? providerId = null, bool? isActive = null, int page = 1)
        {
            _log.Information("بازدید از صفحه اصلی طرح‌های بیمه. SearchTerm: {SearchTerm}, ProviderId: {ProviderId}, IsActive: {IsActive}, Page: {Page}. User: {UserName} (Id: {UserId})",
                searchTerm, providerId, isActive, page, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var model = new InsurancePlanIndexPageViewModel
                {
                    SearchTerm = searchTerm,
                    ProviderId = providerId,
                    IsActive = isActive,
                    CurrentPage = page,
                    PageSize = PageSize
                };

                // بارگیری لیست ارائه‌دهندگان بیمه
                var providersResult = await _insuranceProviderService.GetActiveProvidersForLookupAsync();
                if (providersResult.Success)
                {
                    model.InsuranceProviders = providersResult.Data;
                    model.CreateInsuranceProviderSelectList();
                    model.CreateStatusSelectList();
                }

                // بارگیری طرح‌های بیمه
                var plansResult = await _insurancePlanService.GetPlansAsync(providerId, searchTerm, page, PageSize);
                if (plansResult.Success)
                {
                    model.InsurancePlans = plansResult.Data.Items;
                    model.TotalCount = plansResult.Data.TotalItems;
                }

                // محاسبه آمار
                model.ActivePlansCount = model.InsurancePlans.Count(p => p.IsActive);
                model.InactivePlansCount = model.InsurancePlans.Count(p => !p.IsActive);
                model.TotalPlansCount = model.InsurancePlans.Count;

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در نمایش صفحه اصلی طرح‌های بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در نمایش صفحه اصلی طرح‌های بیمه");
                return View(new InsurancePlanIndexPageViewModel());
            }
        }

        /// <summary>
        /// بارگیری لیست طرح‌های بیمه با صفحه‌بندی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LoadPlans")]
        public async Task<PartialViewResult> LoadPlans(int? providerId = null, string searchTerm = "", int page = 1)
        {
            _log.Information(
                "درخواست لود طرح‌های بیمه. ProviderId: {ProviderId}, SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                providerId, searchTerm, page, PageSize, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insurancePlanService.GetPlansAsync(providerId, searchTerm, page, PageSize);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در لود طرح‌های بیمه. ProviderId: {ProviderId}, SearchTerm: {SearchTerm}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        providerId, searchTerm, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return PartialView("_InsurancePlanListPartial", new PagedResult<InsurancePlanIndexViewModel>());
                }

                _log.Information(
                    "لود طرح‌های بیمه با موفقیت انجام شد. Count: {Count}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    result.Data.TotalItems, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsurancePlanListPartial", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در لود طرح‌های بیمه. ProviderId: {ProviderId}, SearchTerm: {SearchTerm}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    providerId, searchTerm, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsurancePlanListPartial", new PagedResult<InsurancePlanIndexViewModel>());
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات طرح بیمه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            _log.Information(
                "درخواست جزئیات طرح بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insurancePlanService.GetPlanDetailsAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در دریافت جزئیات طرح بیمه. PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "جزئیات طرح بیمه با موفقیت دریافت شد. PlanId: {PlanId}, Name: {Name}. User: {UserName} (Id: {UserId})",
                    id, result.Data.Name, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در دریافت جزئیات طرح بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در دریافت جزئیات طرح بیمه");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// نمایش فرم ایجاد طرح بیمه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Create(int? providerId = null)
        {
            _log.Information("بازدید از فرم ایجاد طرح بیمه. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                providerId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var model = new InsurancePlanCreateEditViewModel
                {
                    InsuranceProviderId = providerId ?? 0,
                    IsActive = true,
                    ValidFrom = DateTime.Now,
                    CoveragePercent = 0,
                    Deductible = 0
                };

                // بارگیری لیست ارائه‌دهندگان بیمه
                await LoadInsuranceProvidersForViewModelAsync(model);

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در نمایش فرم ایجاد طرح بیمه. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در نمایش فرم ایجاد طرح بیمه");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ایجاد طرح بیمه جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(InsurancePlanCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ایجاد طرح بیمه جدید. Name: {Name}, PlanCode: {PlanCode}, ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                model?.Name, model?.PlanCode, model?.InsuranceProviderId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // تبدیل تاریخ‌های شمسی به میلادی قبل از validation
                if (model != null)
                {
                    model.ConvertPersianDatesToGregorian();
                }

                if (!ModelState.IsValid)
                {
                    _log.Warning(
                        "مدل طرح بیمه معتبر نیست. Name: {Name}, PlanCode: {PlanCode}, ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                        model?.Name, model?.PlanCode, model?.InsuranceProviderId, _currentUserService.UserName, _currentUserService.UserId);

                    // بارگیری مجدد لیست ارائه‌دهندگان بیمه
                    await LoadInsuranceProvidersForViewModelAsync(model);

                    return View(model);
                }

                var result = await _insurancePlanService.CreatePlanAsync(model);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در ایجاد طرح بیمه. Name: {Name}, PlanCode: {PlanCode}, ProviderId: {ProviderId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model?.Name, model?.PlanCode, model?.InsuranceProviderId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);

                    // بارگیری مجدد لیست ارائه‌دهندگان بیمه
                    await LoadInsuranceProvidersForViewModelAsync(model);

                    return View(model);
                }

                _log.Information(
                    "طرح بیمه جدید با موفقیت ایجاد شد. PlanId: {PlanId}, Name: {Name}, PlanCode: {PlanCode}. User: {UserName} (Id: {UserId})",
                    result.Data, model.Name, model.PlanCode, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddSuccessMessage("طرح بیمه جدید با موفقیت ایجاد شد");
                return RedirectToAction("Details", new { id = result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در ایجاد طرح بیمه. Name: {Name}, PlanCode: {PlanCode}, ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                    model?.Name, model?.PlanCode, model?.InsuranceProviderId, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در ایجاد طرح بیمه");

                // بارگیری مجدد لیست ارائه‌دهندگان بیمه
                await LoadInsuranceProvidersForViewModelAsync(model);

                return View(model);
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش طرح بیمه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            _log.Information(
                "درخواست فرم ویرایش طرح بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insurancePlanService.GetPlanForEditAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در دریافت طرح بیمه برای ویرایش. PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);
                    return RedirectToAction("Index");
                }

                // بارگیری لیست ارائه‌دهندگان بیمه
                await LoadInsuranceProvidersForViewModelAsync(result.Data);

                // تبدیل تاریخ‌های میلادی به شمسی برای نمایش در فرم
                result.Data.ConvertGregorianDatesToPersian();

                _log.Information(
                    "فرم ویرایش طرح بیمه با موفقیت دریافت شد. PlanId: {PlanId}, Name: {Name}. User: {UserName} (Id: {UserId})",
                    id, result.Data.Name, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در دریافت فرم ویرایش طرح بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در دریافت فرم ویرایش طرح بیمه");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// به‌روزرسانی طرح بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(InsurancePlanCreateEditViewModel model)
        {
            _log.Information(
                "درخواست به‌روزرسانی طرح بیمه. PlanId: {PlanId}, Name: {Name}, PlanCode: {PlanCode}. User: {UserName} (Id: {UserId})",
                model?.InsurancePlanId, model?.Name, model?.PlanCode, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // تبدیل تاریخ‌های شمسی به میلادی قبل از validation
                if (model != null)
                {
                    model.ConvertPersianDatesToGregorian();
                }

                if (!ModelState.IsValid)
                {
                    _log.Warning(
                        "مدل طرح بیمه معتبر نیست. PlanId: {PlanId}, Name: {Name}, PlanCode: {PlanCode}. User: {UserName} (Id: {UserId})",
                        model?.InsurancePlanId, model?.Name, model?.PlanCode, _currentUserService.UserName, _currentUserService.UserId);

                    // بارگیری مجدد لیست ارائه‌دهندگان بیمه
                    await LoadInsuranceProvidersForViewModelAsync(model);

                    return View(model);
                }

                var result = await _insurancePlanService.UpdatePlanAsync(model);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در به‌روزرسانی طرح بیمه. PlanId: {PlanId}, Name: {Name}, PlanCode: {PlanCode}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model?.InsurancePlanId, model?.Name, model?.PlanCode, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);

                    // بارگیری مجدد لیست ارائه‌دهندگان بیمه
                    await LoadInsuranceProvidersForViewModelAsync(model);

                    return View(model);
                }

                _log.Information(
                    "طرح بیمه با موفقیت به‌روزرسانی شد. PlanId: {PlanId}, Name: {Name}, PlanCode: {PlanCode}. User: {UserName} (Id: {UserId})",
                    model.InsurancePlanId, model.Name, model.PlanCode, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddSuccessMessage("طرح بیمه با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Details", new { id = model.InsurancePlanId });
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در به‌روزرسانی طرح بیمه. PlanId: {PlanId}, Name: {Name}, PlanCode: {PlanCode}. User: {UserName} (Id: {UserId})",
                    model?.InsurancePlanId, model?.Name, model?.PlanCode, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در به‌روزرسانی طرح بیمه");

                // بارگیری مجدد لیست ارائه‌دهندگان بیمه
                await LoadInsuranceProvidersForViewModelAsync(model);

                return View(model);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// نمایش فرم تأیید حذف طرح بیمه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            _log.Information(
                "درخواست فرم حذف طرح بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insurancePlanService.GetPlanDetailsAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در دریافت طرح بیمه برای حذف. PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "فرم حذف طرح بیمه با موفقیت دریافت شد. PlanId: {PlanId}, Name: {Name}. User: {UserName} (Id: {UserId})",
                    id, result.Data.Name, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در دریافت فرم حذف طرح بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در دریافت فرم حذف طرح بیمه");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// حذف نرم طرح بیمه (با بررسی وابستگی‌ها)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            _log.Information(
                "درخواست حذف طرح بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // بررسی وابستگی‌ها قبل از حذف
                var dependencyResult = await _dependencyService.CanDeletePlanAsync(id);
                if (!dependencyResult.Success)
                {
                    _log.Warning(
                        "خطا در بررسی وابستگی‌های طرح بیمه. PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, dependencyResult.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(dependencyResult.Message);
                    return RedirectToAction("Index");
                }

                if (!dependencyResult.Data)
                {
                    // دریافت جزئیات وابستگی‌ها برای نمایش پیام دقیق
                    var dependencyInfo = await _dependencyService.CheckDependenciesAsync(id);
                    if (dependencyInfo.Success)
                    {
                        var message = $"طرح بیمه قابل حذف نیست. وابستگی‌ها: {dependencyInfo.Data.DependencySummary}";

                        _log.Warning(
                            "تلاش برای حذف طرح بیمه با وابستگی. PlanId: {PlanId}, Dependencies: {Dependencies}. User: {UserName} (Id: {UserId})",
                            id, dependencyInfo.Data.DependencySummary, _currentUserService.UserName, _currentUserService.UserId);

                        _messageNotificationService.AddErrorMessage(message);
                    }
                    else
                    {
                        _messageNotificationService.AddErrorMessage("طرح بیمه قابل حذف نیست. وابستگی‌هایی وجود دارد.");
                    }

                    return RedirectToAction("Index");
                }

                // حذف طرح بیمه
                var result = await _insurancePlanService.SoftDeletePlanAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در حذف طرح بیمه. PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "طرح بیمه با موفقیت حذف شد. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddSuccessMessage("طرح بیمه با موفقیت حذف شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در حذف طرح بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در حذف طرح بیمه");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Dependency Actions

        /// <summary>
        /// نمایش وابستگی‌های طرح بیمه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Dependencies(int id)
        {
            _log.Information(
                "درخواست نمایش وابستگی‌های طرح بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _dependencyService.CheckDependenciesAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در دریافت وابستگی‌های طرح بیمه. PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    _messageNotificationService.AddErrorMessage(result.Message);
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "وابستگی‌های طرح بیمه با موفقیت دریافت شد. PlanId: {PlanId}, Dependencies: {Dependencies}. User: {UserName} (Id: {UserId})",
                    id, result.Data.DependencySummary, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در دریافت وابستگی‌های طرح بیمه. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                _messageNotificationService.AddErrorMessage("خطا در دریافت وابستگی‌های طرح بیمه");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// بررسی وجود کد طرح بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CheckPlanCodeExists(string planCode, int? excludeId = null)
        {
            try
            {
                var result = await _insurancePlanService.DoesPlanCodeExistAsync(planCode, excludeId);
                return Json(new { exists = result.Success && result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود کد طرح بیمه. PlanCode: {PlanCode}", planCode);
                return Json(new { exists = false });
            }
        }

        /// <summary>
        /// بررسی وجود نام طرح بیمه در ارائه‌دهنده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("CheckNameExistsInProvider")]
        public async Task<JsonResult> CheckNameExistsInProvider(string name, int providerId, int? excludeId = null)
        {
            try
            {
                var result = await _insurancePlanService.DoesNameExistInProviderAsync(name, providerId, excludeId);
                return Json(new { exists = result.Success && result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود نام طرح بیمه در ارائه‌دهنده. Name: {Name}, ProviderId: {ProviderId}", name, providerId);
                return Json(new { exists = false });
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه بر اساس ارائه‌دهنده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetPlansByProvider(int providerId)
        {
            try
            {
                var result = await _insurancePlanService.GetActivePlansForLookupAsync(providerId);
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت طرح‌های بیمه بر اساس ارائه‌دهنده. ProviderId: {ProviderId}", providerId);
                return Json(new { success = false, message = "خطا در دریافت طرح‌های بیمه" });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// بارگیری لیست ارائه‌دهندگان بیمه برای ViewBag
        /// طبق اصل DRY - یکبار تعریف، چندبار استفاده
        /// </summary>
        private async Task LoadInsuranceProvidersAsync()
        {
            try
            {
                var providersResult = await _insuranceProviderService.GetActiveProvidersForLookupAsync();
                if (providersResult.Success)
                {
                    ViewBag.InsuranceProviderSelectList = new SelectList(providersResult.Data, "Id", "Name");
                    _log.Debug("لیست ارائه‌دهندگان بیمه با موفقیت بارگیری شد. تعداد: {Count}", providersResult.Data.Count);
                }
                else
                {
                    ViewBag.InsuranceProviderSelectList = new SelectList(new List<object>(), "Id", "Name");
                    _log.Warning("خطا در بارگیری لیست ارائه‌دهندگان بیمه: {Message}", providersResult.Message);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بارگیری لیست ارائه‌دهندگان بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                ViewBag.InsuranceProviderSelectList = new SelectList(new List<object>(), "Id", "Name");
            }
        }

        /// <summary>
        /// بارگیری لیست ارائه‌دهندگان بیمه برای ViewModel
        /// طبق اصل DRY - یکبار تعریف، چندبار استفاده
        /// </summary>
        private async Task LoadInsuranceProvidersForViewModelAsync(InsurancePlanCreateEditViewModel model)
        {
            try
            {
                var providersResult = await _insuranceProviderService.GetActiveProvidersForLookupAsync();
                if (providersResult.Success)
                {
                    model.InsuranceProviders = providersResult.Data;
                    model.CreateInsuranceProviderSelectList();
                    _log.Debug("لیست ارائه‌دهندگان بیمه برای ViewModel با موفقیت بارگیری شد. تعداد: {Count}", providersResult.Data.Count);
                }
                else
                {
                    model.InsuranceProviders = new List<InsuranceProviderLookupViewModel>();
                    model.CreateInsuranceProviderSelectList();
                    _log.Warning("خطا در بارگیری لیست ارائه‌دهندگان بیمه برای ViewModel: {Message}", providersResult.Message);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بارگیری لیست ارائه‌دهندگان بیمه برای ViewModel. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                model.InsuranceProviders = new List<InsuranceProviderLookupViewModel>();
                model.CreateInsuranceProviderSelectList();
            }
        }

        #endregion
    }
}
