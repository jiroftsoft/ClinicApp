using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Areas.Admin.Controllers
{
    //[Authorize(Roles = AppRoles.Admin)] // این کنترلر فقط برای کاربران با نقش ادمین قابل دسترس است
    public class ClinicController : Controller
    {
        private readonly IClinicManagementService _clinicService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public ClinicController(IClinicManagementService clinicService, ILogger logger, ICurrentUserService currentUserService)
        {
            _clinicService = clinicService;
            _log = logger.ForContext<ClinicController>();
            _currentUserService = currentUserService;
        }

        // GET: Admin/Clinic
        public async Task<ActionResult> Index(string searchTerm = "", int pageNumber = 1, bool isAjax = false)
        {
            _log.Information("🏥 MEDICAL: درخواست صفحه لیست کلینیک‌ها. SearchTerm: {SearchTerm}, Page: {Page}, IsAjax: {IsAjax}, User: {UserId}",
                searchTerm, pageNumber, isAjax, _currentUserService?.UserId ?? "Anonymous");

            int pageSize = 10; // می‌توان این را از IAppSettings خواند
            var result = await _clinicService.GetClinicsAsync(searchTerm, pageNumber, pageSize);

            if (!result.Success)
            {
                _log.Warning("🏥 MEDICAL: خطا در دریافت لیست کلینیک‌ها. Message: {Message}, User: {UserId}",
                    result.Message, _currentUserService?.UserId ?? "Anonymous");

                // در صورت بروز خطا در سرویس، یک پیام خطا نمایش می‌دهیم
                if (isAjax)
                {
                    return Json(new PagedResult<ClinicIndexViewModel>(), JsonRequestBehavior.AllowGet);
                }
                TempData["ErrorMessage"] = result.Message;
                return View(new PagedResult<ClinicIndexViewModel>()); // یک مدل خالی به ویو پاس می‌دهیم
            }

            _log.Information("🏥 MEDICAL: لیست کلینیک‌ها با موفقیت دریافت شد. Count: {Count}, User: {UserId}",
                result.Data?.Items?.Count ?? 0, _currentUserService?.UserId ?? "Anonymous");

            // ✅ بازگرداندن JSON برای درخواست‌های AJAX
            if (isAjax)
            {
                return Json(result.Data, JsonRequestBehavior.AllowGet);
            }

            return View(result.Data);
        }

        // GET: Admin/Clinic/Details/5
        public async Task<ActionResult> Details(int id)
        {
            _log.Information("🏥 MEDICAL: درخواست مشاهده جزئیات کلینیک {ClinicId}. User: {UserId}", 
                id, _currentUserService?.UserId ?? "Anonymous");

            try
            {
                var result = await _clinicService.GetClinicDetailsAsync(id);
                if (!result.Success)
                {
                    _log.Warning("🏥 MEDICAL: کلینیک {ClinicId} یافت نشد. Error: {Error}, User: {UserId}", 
                        id, result.Message, _currentUserService?.UserId ?? "Anonymous");

                    if (result.Code == "NOT_FOUND") 
                    {
                        return HttpNotFound("کلینیک مورد نظر یافت نشد.");
                    }
                    
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Information("🏥 MEDICAL: جزئیات کلینیک {ClinicId} با موفقیت بارگذاری شد. User: {UserId}", 
                    id, _currentUserService?.UserId ?? "Anonymous");

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در بارگذاری جزئیات کلینیک {ClinicId}. User: {UserId}", 
                    id, _currentUserService?.UserId ?? "Anonymous");
                
                TempData["ErrorMessage"] = "خطای سیستمی در بارگذاری اطلاعات رخ داد.";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Clinic/Create
        public ActionResult Create()
        {
            return View(new ClinicCreateEditViewModel());
        }

        // POST: Admin/Clinic/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ClinicCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _clinicService.CreateClinicAsync(model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            // اگر خطا از سمت سرویس باشد (مانند نام تکراری)، آن را به ModelState اضافه می‌کنیم
            AddServiceErrorsToModelState(result);
            return View(model);
        }

        // GET: Admin/Clinic/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var result = await _clinicService.GetClinicForEditAsync(id);
            if (!result.Success)
            {
                if (result.Code == "NOT_FOUND") return HttpNotFound();
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }
            return View(result.Data);
        }

        // POST: Admin/Clinic/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(ClinicCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // ✅ **اصلاح کلیدی:** به صراحت نام View را "Edit" مشخص می‌کنیم
                return View("Edit", model);
            }

            var result = await _clinicService.UpdateClinicAsync(model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            AddServiceErrorsToModelState(result);

            // ✅ **اصلاح کلیدی:** اینجا نیز نام View را "Edit" مشخص می‌کنیم
            return View("Edit", model);
        }

        // POST: Admin/Clinic/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            _log.Information("🏥 MEDICAL: درخواست حذف کلینیک {ClinicId} از طریق Controller. User: {UserId}", 
                id, _currentUserService?.UserId ?? "Anonymous");

            var result = await _clinicService.SoftDeleteClinicAsync(id);
            if (result.Success)
            {
                _log.Information("🏥 MEDICAL: کلینیک {ClinicId} با موفقیت حذف شد. User: {UserId}", 
                    id, _currentUserService?.UserId ?? "Anonymous");
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                _log.Warning("🏥 MEDICAL: حذف کلینیک {ClinicId} ناموفق بود. Error: {Error}, User: {UserId}", 
                    id, result.Message, _currentUserService?.UserId ?? "Anonymous");
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 🏥 MEDICAL: دریافت اطلاعات وابستگی‌های کلینیک (برای AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDependencyInfo(int id)
        {
            _log.Information("🏥 MEDICAL: درخواست اطلاعات وابستگی کلینیک {ClinicId} از طریق AJAX. User: {UserId}", 
                id, _currentUserService?.UserId ?? "Anonymous");

            var result = await _clinicService.GetClinicDependencyInfoAsync(id);
            if (result.Success)
            {
                return Json(new { 
                    success = true, 
                    data = result.Data,
                    canDelete = result.Data.CanBeDeleted,
                    message = result.Data.DeletionErrorMessage
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { 
                    success = false, 
                    message = result.Message 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت Anti-Forgery Token برای درخواست‌های AJAX
        /// </summary>
        [HttpGet]
        public JsonResult GetAntiForgeryToken()
        {
            var tokenData = AntiForgeryHelper.GetTokenData();
            return Json(tokenData, JsonRequestBehavior.AllowGet);
        }

        #region Private Helpers
        /// <summary>
        /// متد کمکی برای افزودن خطاهای بازگشتی از سرویس به ModelState
        /// </summary>
        private void AddServiceErrorsToModelState(ServiceResult result)
        {
            if (result.ValidationErrors != null && result.ValidationErrors.Any())
            {
                foreach (var error in result.ValidationErrors)
                {
                    // FluentValidation خطاها را با نام پراپرتی برمی‌گرداند
                    ModelState.AddModelError(error.Field ?? "", error.ErrorMessage);
                }
            }
            else if (!string.IsNullOrEmpty(result.Message))
            {
                // برای خطاهای عمومی که به فیلد خاصی مرتبط نیستند
                ModelState.AddModelError("", result.Message);
            }
        }
        #endregion
    }
}