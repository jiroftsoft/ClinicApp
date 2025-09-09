using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر مدیریت ارائه‌دهندگان بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل ارائه‌دهندگان بیمه (SSO, FREE, MILITARY, HEALTH, SUPPLEMENTARY)
    /// 2. استفاده از Anti-Forgery Token در همه POST actions
    /// 3. استفاده از ServiceResult Enhanced pattern
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. رعایت استانداردهای پزشکی ایران
    /// 
    /// نکته حیاتی: این کنترلر بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    //[Authorize(Roles = AppRoles.Admin)]
    [RouteArea("Admin")]
    [RoutePrefix("Insurance/Provider")]
    public class InsuranceProviderController : Controller
    {
        private readonly IInsuranceProviderService _insuranceProviderService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;

        public InsuranceProviderController(
            IInsuranceProviderService insuranceProviderService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings)
        {
            _insuranceProviderService = insuranceProviderService ?? throw new ArgumentNullException(nameof(insuranceProviderService));
            _log = logger.ForContext<InsuranceProviderController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        private int PageSize => _appSettings.DefaultPageSize;

        #region Index & Search

        /// <summary>
        /// نمایش صفحه اصلی ارائه‌دهندگان بیمه
        /// </summary>
        [HttpGet]
       
        public ActionResult Index()
        {
            _log.Information("بازدید از صفحه اصلی ارائه‌دهندگان بیمه. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            return View();
        }

        /// <summary>
        /// بارگیری لیست ارائه‌دهندگان بیمه با صفحه‌بندی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LoadProviders")]
        public async Task<PartialViewResult> LoadProviders(string searchTerm = "", int page = 1)
        {
            _log.Information(
                "درخواست لود ارائه‌دهندگان بیمه. SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                searchTerm, page, PageSize, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceProviderService.GetProvidersAsync(searchTerm, page, PageSize);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در لود ارائه‌دهندگان بیمه. SearchTerm: {SearchTerm}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        searchTerm, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return PartialView("_InsuranceProviderListPartial", new PagedResult<InsuranceProviderIndexViewModel>());
                }

                _log.Information(
                    "لود ارائه‌دهندگان بیمه با موفقیت انجام شد. Count: {Count}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    result.Data.TotalItems, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsuranceProviderListPartial", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در لود ارائه‌دهندگان بیمه. SearchTerm: {SearchTerm}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    searchTerm, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsuranceProviderListPartial", new PagedResult<InsuranceProviderIndexViewModel>());
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات ارائه‌دهنده بیمه
        /// </summary>
        [HttpGet]
        [Route("Details/{id:int}")]
        public async Task<ActionResult> Details(int id)
        {
            _log.Information(
                "درخواست جزئیات ارائه‌دهنده بیمه. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceProviderService.GetProviderDetailsAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در دریافت جزئیات ارائه‌دهنده بیمه. ProviderId: {ProviderId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "جزئیات ارائه‌دهنده بیمه با موفقیت دریافت شد. ProviderId: {ProviderId}, Name: {Name}. User: {UserName} (Id: {UserId})",
                    id, result.Data.Name, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در دریافت جزئیات ارائه‌دهنده بیمه. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در دریافت جزئیات ارائه‌دهنده بیمه";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// نمایش فرم ایجاد ارائه‌دهنده بیمه
        /// </summary>
        [HttpGet]
        [Route("Create")]
        public ActionResult Create()
        {
            _log.Information("بازدید از فرم ایجاد ارائه‌دهنده بیمه. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            var model = new InsuranceProviderCreateEditViewModel
            {
                IsActive = true
            };

            return View(model);
        }

        /// <summary>
        /// ایجاد ارائه‌دهنده بیمه جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<ActionResult> Create(InsuranceProviderCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ایجاد ارائه‌دهنده بیمه جدید. Name: {Name}, Code: {Code}. User: {UserName} (Id: {UserId})",
                model?.Name, model?.Code, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (!ModelState.IsValid)
                {
                    _log.Warning(
                        "مدل ارائه‌دهنده بیمه معتبر نیست. Name: {Name}, Code: {Code}. User: {UserName} (Id: {UserId})",
                        model?.Name, model?.Code, _currentUserService.UserName, _currentUserService.UserId);

                    return View(model);
                }

                var result = await _insuranceProviderService.CreateProviderAsync(model);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در ایجاد ارائه‌دهنده بیمه. Name: {Name}, Code: {Code}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model?.Name, model?.Code, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return View(model);
                }

                _log.Information(
                    "ارائه‌دهنده بیمه جدید با موفقیت ایجاد شد. ProviderId: {ProviderId}, Name: {Name}, Code: {Code}. User: {UserName} (Id: {UserId})",
                    result.Data, model.Name, model.Code, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "ارائه‌دهنده بیمه جدید با موفقیت ایجاد شد";
                return RedirectToAction("Details", new { id = result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در ایجاد ارائه‌دهنده بیمه. Name: {Name}, Code: {Code}. User: {UserName} (Id: {UserId})",
                    model?.Name, model?.Code, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در ایجاد ارائه‌دهنده بیمه";
                return View(model);
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش ارائه‌دهنده بیمه
        /// </summary>
        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<ActionResult> Edit(int id)
        {
            _log.Information(
                "درخواست فرم ویرایش ارائه‌دهنده بیمه. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceProviderService.GetProviderForEditAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در دریافت ارائه‌دهنده بیمه برای ویرایش. ProviderId: {ProviderId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "فرم ویرایش ارائه‌دهنده بیمه با موفقیت دریافت شد. ProviderId: {ProviderId}, Name: {Name}. User: {UserName} (Id: {UserId})",
                    id, result.Data.Name, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در دریافت فرم ویرایش ارائه‌دهنده بیمه. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در دریافت فرم ویرایش ارائه‌دهنده بیمه";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// به‌روزرسانی ارائه‌دهنده بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<ActionResult> Edit(InsuranceProviderCreateEditViewModel model)
        {
            _log.Information(
                "درخواست به‌روزرسانی ارائه‌دهنده بیمه. ProviderId: {ProviderId}, Name: {Name}, Code: {Code}. User: {UserName} (Id: {UserId})",
                model?.InsuranceProviderId, model?.Name, model?.Code, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (!ModelState.IsValid)
                {
                    _log.Warning(
                        "مدل ارائه‌دهنده بیمه معتبر نیست. ProviderId: {ProviderId}, Name: {Name}, Code: {Code}. User: {UserName} (Id: {UserId})",
                        model?.InsuranceProviderId, model?.Name, model?.Code, _currentUserService.UserName, _currentUserService.UserId);

                    return View(model);
                }

                var result = await _insuranceProviderService.UpdateProviderAsync(model);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در به‌روزرسانی ارائه‌دهنده بیمه. ProviderId: {ProviderId}, Name: {Name}, Code: {Code}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model?.InsuranceProviderId, model?.Name, model?.Code, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return View(model);
                }

                _log.Information(
                    "ارائه‌دهنده بیمه با موفقیت به‌روزرسانی شد. ProviderId: {ProviderId}, Name: {Name}, Code: {Code}. User: {UserName} (Id: {UserId})",
                    model.InsuranceProviderId, model.Name, model.Code, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "ارائه‌دهنده بیمه با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Details", new { id = model.InsuranceProviderId });
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در به‌روزرسانی ارائه‌دهنده بیمه. ProviderId: {ProviderId}, Name: {Name}, Code: {Code}. User: {UserName} (Id: {UserId})",
                    model?.InsuranceProviderId, model?.Name, model?.Code, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در به‌روزرسانی ارائه‌دهنده بیمه";
                return View(model);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// نمایش فرم تأیید حذف ارائه‌دهنده بیمه
        /// </summary>
        [HttpGet]
        [Route("Delete/{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            _log.Information(
                "درخواست فرم حذف ارائه‌دهنده بیمه. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceProviderService.GetProviderDetailsAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در دریافت ارائه‌دهنده بیمه برای حذف. ProviderId: {ProviderId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "فرم حذف ارائه‌دهنده بیمه با موفقیت دریافت شد. ProviderId: {ProviderId}, Name: {Name}. User: {UserName} (Id: {UserId})",
                    id, result.Data.Name, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در دریافت فرم حذف ارائه‌دهنده بیمه. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در دریافت فرم حذف ارائه‌دهنده بیمه";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// حذف نرم ارائه‌دهنده بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Delete")]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            _log.Information(
                "درخواست حذف ارائه‌دهنده بیمه. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceProviderService.SoftDeleteProviderAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در حذف ارائه‌دهنده بیمه. ProviderId: {ProviderId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "ارائه‌دهنده بیمه با موفقیت حذف شد. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "ارائه‌دهنده بیمه با موفقیت حذف شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در حذف ارائه‌دهنده بیمه. ProviderId: {ProviderId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در حذف ارائه‌دهنده بیمه";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// بررسی وجود کد ارائه‌دهنده بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("CheckCodeExists")]
        public async Task<JsonResult> CheckCodeExists(string code, int? excludeId = null)
        {
            try
            {
                var result = await _insuranceProviderService.DoesCodeExistAsync(code, excludeId);
                return Json(new { exists = result.Success && result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود کد ارائه‌دهنده بیمه. Code: {Code}", code);
                return Json(new { exists = false });
            }
        }

        /// <summary>
        /// بررسی وجود نام ارائه‌دهنده بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("CheckNameExists")]
        public async Task<JsonResult> CheckNameExists(string name, int? excludeId = null)
        {
            try
            {
                var result = await _insuranceProviderService.DoesNameExistAsync(name, excludeId);
                return Json(new { exists = result.Success && result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود نام ارائه‌دهنده بیمه. Name: {Name}", name);
                return Json(new { exists = false });
            }
        }

        #endregion
    }
}
