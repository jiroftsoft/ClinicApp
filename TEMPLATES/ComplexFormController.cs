using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using ClinicApp.Core;
using ClinicApp.Services;
using ClinicApp.ViewModels.[Module];
using ClinicApp.Extensions;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller برای مدیریت [Module]
    /// استاندارد کامل برای فرم‌های پیچیده
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class [Module]Controller : Controller
    {
        #region Fields - فیلدها

        private readonly I[Module]Service _service;
        private readonly ILogger<[Module]Controller> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;

        #endregion

        #region Constructor - سازنده

        public [Module]Controller(
            I[Module]Service service,
            ILogger<[Module]Controller> logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings)
        {
            _service = service;
            _logger = logger;
            _currentUserService = currentUserService;
            _appSettings = appSettings;
        }

        #endregion

        #region Index Action - اکشن فهرست

        /// <summary>
        /// نمایش فهرست [Module]
        /// </summary>
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.LogInformation("🏥 [Module] Index action started by user {UserId}", 
                    _currentUserService.GetCurrentUserId());

                var result = await _service.GetAllAsync();
                
                if (result.IsSuccess)
                {
                    var viewModels = result.Data.Select([Module]ListViewModel.FromEntity).ToList();
                    _logger.LogInformation("✅ [Module] Index successful - {Count} items loaded", viewModels.Count);
                    return View(viewModels);
                }
                else
                {
                    _logger.LogError("❌ [Module] Index failed - {Error}", result.ErrorMessage);
                    TempData["Error"] = result.ErrorMessage;
                    return View(new List<[Module]ListViewModel>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] Index exception occurred");
                TempData["Error"] = "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.";
                return View(new List<[Module]ListViewModel>());
            }
        }

        #endregion

        #region Create Actions - اکشن‌های ایجاد

        /// <summary>
        /// نمایش فرم ایجاد [Module]
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Create(int? insuranceProviderId = null)
        {
            try
            {
                _logger.LogInformation("🏥 [Module] Create GET action started by user {UserId}", 
                    _currentUserService.GetCurrentUserId());

                var model = [Module]CreateEditViewModel.CreateNew(insuranceProviderId);
                
                // Load lookup data
                await LoadLookupData(model);
                
                _logger.LogInformation("✅ [Module] Create GET successful");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] Create GET exception occurred");
                TempData["Error"] = "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش فرم ایجاد [Module]
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Module]CreateEditViewModel model)
        {
            try
            {
                _logger.LogInformation("🏥 [Module] Create POST action started by user {UserId}", 
                    _currentUserService.GetCurrentUserId());

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("⚠️ [Module] Create failed - Model validation errors: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    
                    await LoadLookupData(model);
                    return View(model);
                }

                var result = await _service.CreateAsync(model);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("✅ [Module] Create successful - ID: {Id}", result.Data.Id);
                    TempData["Success"] = "[Module] با موفقیت ایجاد شد.";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogError("❌ [Module] Create failed - {Error}", result.ErrorMessage);
                    ModelState.AddModelError("", result.ErrorMessage);
                    await LoadLookupData(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] Create POST exception occurred");
                ModelState.AddModelError("", "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.");
                await LoadLookupData(model);
                return View(model);
            }
        }

        #endregion

        #region Edit Actions - اکشن‌های ویرایش

        /// <summary>
        /// نمایش فرم ویرایش [Module]
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation("🏥 [Module] Edit GET action started by user {UserId} for ID: {Id}", 
                    _currentUserService.GetCurrentUserId(), id);

                var result = await _service.GetByIdAsync(id);
                
                if (result.IsSuccess)
                {
                    var model = [Module]CreateEditViewModel.FromEntity(result.Data);
                    await LoadLookupData(model);
                    
                    _logger.LogInformation("✅ [Module] Edit GET successful for ID: {Id}", id);
                    return View(model);
                }
                else
                {
                    _logger.LogError("❌ [Module] Edit GET failed for ID: {Id} - {Error}", id, result.ErrorMessage);
                    TempData["Error"] = result.ErrorMessage;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] Edit GET exception occurred for ID: {Id}", id);
                TempData["Error"] = "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش فرم ویرایش [Module]
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Module]CreateEditViewModel model)
        {
            try
            {
                _logger.LogInformation("🏥 [Module] Edit POST action started by user {UserId} for ID: {Id}", 
                    _currentUserService.GetCurrentUserId(), model.Id);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("⚠️ [Module] Edit failed - Model validation errors: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    
                    await LoadLookupData(model);
                    return View(model);
                }

                var result = await _service.UpdateAsync(model.Id, model);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("✅ [Module] Edit successful for ID: {Id}", model.Id);
                    TempData["Success"] = "[Module] با موفقیت به‌روزرسانی شد.";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogError("❌ [Module] Edit failed for ID: {Id} - {Error}", model.Id, result.ErrorMessage);
                    ModelState.AddModelError("", result.ErrorMessage);
                    await LoadLookupData(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] Edit POST exception occurred for ID: {Id}", model.Id);
                ModelState.AddModelError("", "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.");
                await LoadLookupData(model);
                return View(model);
            }
        }

        #endregion

        #region Details Action - اکشن جزئیات

        /// <summary>
        /// نمایش جزئیات [Module]
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("🏥 [Module] Details action started by user {UserId} for ID: {Id}", 
                    _currentUserService.GetCurrentUserId(), id);

                var result = await _service.GetByIdAsync(id);
                
                if (result.IsSuccess)
                {
                    var model = [Module]DetailsViewModel.FromEntity(result.Data);
                    _logger.LogInformation("✅ [Module] Details successful for ID: {Id}", id);
                    return View(model);
                }
                else
                {
                    _logger.LogError("❌ [Module] Details failed for ID: {Id} - {Error}", id, result.ErrorMessage);
                    TempData["Error"] = result.ErrorMessage;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] Details exception occurred for ID: {Id}", id);
                TempData["Error"] = "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Delete Actions - اکشن‌های حذف

        /// <summary>
        /// نمایش فرم تأیید حذف [Module]
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("🏥 [Module] Delete GET action started by user {UserId} for ID: {Id}", 
                    _currentUserService.GetCurrentUserId(), id);

                var result = await _service.GetByIdAsync(id);
                
                if (result.IsSuccess)
                {
                    var model = [Module]DetailsViewModel.FromEntity(result.Data);
                    _logger.LogInformation("✅ [Module] Delete GET successful for ID: {Id}", id);
                    return View(model);
                }
                else
                {
                    _logger.LogError("❌ [Module] Delete GET failed for ID: {Id} - {Error}", id, result.ErrorMessage);
                    TempData["Error"] = result.ErrorMessage;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] Delete GET exception occurred for ID: {Id}", id);
                TempData["Error"] = "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش حذف [Module]
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("🏥 [Module] Delete POST action started by user {UserId} for ID: {Id}", 
                    _currentUserService.GetCurrentUserId(), id);

                var result = await _service.DeleteAsync(id);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("✅ [Module] Delete successful for ID: {Id}", id);
                    TempData["Success"] = "[Module] با موفقیت حذف شد.";
                }
                else
                {
                    _logger.LogError("❌ [Module] Delete failed for ID: {Id} - {Error}", id, result.ErrorMessage);
                    TempData["Error"] = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] Delete POST exception occurred for ID: {Id}", id);
                TempData["Error"] = "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید.";
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Toggle Status Action - اکشن تغییر وضعیت

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال [Module]
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ToggleStatus(int id)
        {
            try
            {
                _logger.LogInformation("🏥 [Module] ToggleStatus action started by user {UserId} for ID: {Id}", 
                    _currentUserService.GetCurrentUserId(), id);

                var result = await _service.ToggleStatusAsync(id);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("✅ [Module] ToggleStatus successful for ID: {Id}", id);
                    return Json(new { success = true, message = "وضعیت با موفقیت تغییر کرد." });
                }
                else
                {
                    _logger.LogError("❌ [Module] ToggleStatus failed for ID: {Id} - {Error}", id, result.ErrorMessage);
                    return Json(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] ToggleStatus exception occurred for ID: {Id}", id);
                return Json(new { success = false, message = "خطای غیرمنتظره‌ای رخ داد." });
            }
        }

        #endregion

        #region AJAX Actions - اکشن‌های AJAX

        /// <summary>
        /// جستجوی [Module] برای AJAX
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> Search(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("🔄 [Module] Search AJAX request: {SearchTerm}", searchTerm);

                var result = await _service.SearchAsync(searchTerm, pageNumber, pageSize);
                
                if (result.IsSuccess)
                {
                    var viewModels = result.Data.Select([Module]LookupViewModel.FromEntity).ToList();
                    _logger.LogInformation("✅ [Module] Search successful - {Count} items found", viewModels.Count);
                    
                    return Json(new { 
                        success = true, 
                        data = viewModels,
                        totalCount = result.TotalCount,
                        pageNumber = pageNumber,
                        pageSize = pageSize
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.LogError("❌ [Module] Search failed - {Error}", result.ErrorMessage);
                    return Json(new { success = false, message = result.ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] Search exception occurred");
                return Json(new { success = false, message = "خطای غیرمنتظره‌ای رخ داد." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت [Module] بر اساس شناسه برای AJAX
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("🔄 [Module] GetById AJAX request for ID: {Id}", id);

                var result = await _service.GetByIdAsync(id);
                
                if (result.IsSuccess)
                {
                    var viewModel = [Module]LookupViewModel.FromEntity(result.Data);
                    _logger.LogInformation("✅ [Module] GetById successful for ID: {Id}", id);
                    
                    return Json(new { success = true, data = viewModel }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.LogError("❌ [Module] GetById failed for ID: {Id} - {Error}", id, result.ErrorMessage);
                    return Json(new { success = false, message = result.ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] GetById exception occurred for ID: {Id}", id);
                return Json(new { success = false, message = "خطای غیرمنتظره‌ای رخ داد." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Helper Methods - متدهای کمکی

        /// <summary>
        /// بارگذاری داده‌های جستجو
        /// </summary>
        private async Task LoadLookupData([Module]CreateEditViewModel model)
        {
            try
            {
                // Load insurance providers
                var providersResult = await _service.GetInsuranceProvidersAsync();
                if (providersResult.IsSuccess)
                {
                    ViewBag.InsuranceProviders = providersResult.Data.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name,
                        Selected = p.Id == model.InsuranceProviderId
                    }).ToList();
                }

                // Set module name for view
                ViewBag.ModuleName = "[Module]";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error loading lookup data");
            }
        }

        #endregion
    }
}
