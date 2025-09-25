using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Areas.Admin.Controllers
{
    //[Authorize(Roles = AppRoles.Admin)]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentManagementService _departmentService;
        private readonly IClinicManagementService _clinicService; // برای لیست کلینیک‌ها
        private readonly ILogger _log;

        public DepartmentController(
            IDepartmentManagementService departmentService,
            IClinicManagementService clinicService,
            ILogger logger)
        {
            _departmentService = departmentService;
            _clinicService = clinicService;
            _log = logger.ForContext<DepartmentController>();
        }

        // GET: Admin/Department?clinicId=1
        // In ~/Areas/Admin/Controllers/DepartmentController.cs

        public async Task<ActionResult> Index(int? clinicId, string searchTerm = "", int pageNumber = 1)
        {
            // Prevent caching to ensure fresh data
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");
            
            var clinicsResult = await _clinicService.GetActiveClinicsForLookupAsync();

            var pageViewModel = new DepartmentIndexPageViewModel
            {
                SearchTerm = searchTerm,
                SelectedClinicId = clinicId,
                Clinics = new SelectList(clinicsResult.Data, "Id", "Name", clinicId),
                SelectedClinicName = clinicsResult.Data?.FirstOrDefault(c => c.Id == clinicId)?.Name
            };

            if (clinicId.HasValue)
            {
                int pageSize = 10;
                var result = await _departmentService.GetDepartmentsAsync(clinicId.Value, searchTerm, pageNumber, pageSize);
                if (result.Success)
                {
                    pageViewModel.Departments = result.Data;
                }
                else
                {
                    // اگر service ناموفق بود، یک PagedResult خالی برگردان
                    pageViewModel.Departments = new ClinicApp.Interfaces.PagedResult<DepartmentIndexViewModel>(
                        new List<DepartmentIndexViewModel>(), 0, pageNumber, pageSize);
                }
            }

            if (Request.IsAjaxRequest())
            {
                // برای AJAX requests، تمام اطلاعات مورد نیاز را return کنیم
                if (pageViewModel.Departments != null)
                {
                    return Json(pageViewModel.Departments, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // اگر دپارتمانی نداریم، یک پاسخ خالی return کنیم
                    var emptyResult = new ClinicApp.Interfaces.PagedResult<DepartmentIndexViewModel>(
                        new List<DepartmentIndexViewModel>(), 0, 1, 10);
                    return Json(emptyResult, JsonRequestBehavior.AllowGet);
                }
            }

            return View(pageViewModel);
        }

        // GET: Admin/Department/Details/5
        /// <summary>
        /// (GET) Displays the complete details of a specific department.
        /// </summary>
        public async Task<ActionResult> Details(int id)
        {
            var result = await _departmentService.GetDepartmentDetailsAsync(id);

            if (!result.Success)
            {
                if (result.Code == "NOT_FOUND")
                {
                    return HttpNotFound();
                }

                TempData["ErrorMessage"] = result.Message;

                // If we have the clinic ID from the data, redirect to that specific index page.
                // Otherwise, redirect to the main clinic selection page.
                int? clinicId = (result.Data as DepartmentDetailsViewModel)?.ClinicId;
                if (clinicId.HasValue)
                {
                    return RedirectToAction("Index", new { clinicId = clinicId.Value });
                }
                return RedirectToAction("Index", "Clinic");
            }

            return View(result.Data);
        }

        // GET: Admin/Department/Create?clinicId=1
        public async Task<ActionResult> Create(int clinicId)
        {
            var clinicResult = await _clinicService.GetClinicDetailsAsync(clinicId);
            if (!clinicResult.Success)
            {
                TempData["ErrorMessage"] = "ابتدا باید یک کلینیک معتبر انتخاب کنید.";
                return RedirectToAction("Index", "Clinic");
            }

            var model = new DepartmentCreateEditViewModel
            {
                ClinicId = clinicId,
                ClinicName = clinicResult.Data.Name,
                IsActive = true
            };
            return View(model);
        }

        // POST: Admin/Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DepartmentCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _departmentService.CreateDepartmentAsync(model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "دپارتمان با موفقیت ایجاد شد.";
                return RedirectToAction("Index", new { clinicId = model.ClinicId });
            }

            AddServiceErrorsToModelState(result);
            return View(model);
        }

        // GET: Admin/Department/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var result = await _departmentService.GetDepartmentForEditAsync(id);
            if (!result.Success)
            {
                if (result.Code == "NOT_FOUND") return HttpNotFound();
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index", "Clinic");
            }
            return View(result.Data);
        }

        // POST: Admin/Department/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(DepartmentCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            var result = await _departmentService.UpdateDepartmentAsync(model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "دپارتمان با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("Index", new { clinicId = model.ClinicId });
            }

            AddServiceErrorsToModelState(result);
            return View("Edit", model);
        }

        // POST: Admin/Department/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, int clinicId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست حذف دپارتمان. DepartmentId: {DepartmentId}, ClinicId: {ClinicId}, User: {UserId}",
                    id, clinicId, User.Identity.Name);

                var result = await _departmentService.SoftDeleteDepartmentAsync(id);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: دپارتمان با موفقیت حذف شد. DepartmentId: {DepartmentId}, User: {UserId}",
                        id, User.Identity.Name);

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = true, message = result.Message }, JsonRequestBehavior.AllowGet);
                    }

                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: حذف دپارتمان ناموفق. DepartmentId: {DepartmentId}, Message: {Message}, User: {UserId}",
                        id, result.Message, User.Identity.Name);

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                    }

                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("Index", new { clinicId = clinicId });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در حذف دپارتمان. DepartmentId: {DepartmentId}, User: {UserId}",
                    id, User.Identity.Name);

                var errorMessage = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.";

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = errorMessage }, JsonRequestBehavior.AllowGet);
                }

                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("Index", new { clinicId = clinicId });
            }
        }

        #region AJAX Actions
        
        /// <summary>
        /// 🚀 PERFORMANCE: AJAX endpoint for loading departments
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetDepartments(int? clinicId = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: Loading departments for AJAX - ClinicId: {ClinicId}", clinicId);
                
                // Use default clinic if not specified
                var targetClinicId = clinicId ?? 1; // Default clinic
                
                var result = await _departmentService.GetActiveDepartmentsForLookupAsync(targetClinicId);
                
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: Departments loaded successfully - Count: {Count}", result.Data?.Count ?? 0);
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: Failed to load departments - Error: {Error}", result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: Error loading departments for AJAX");
                return Json(new { success = false, message = "خطا در بارگذاری دپارتمان‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }
        
        #endregion

        #region Private Helpers
        private void AddServiceErrorsToModelState(ServiceResult result)
        {
            if (result.ValidationErrors != null && result.ValidationErrors.Any())
            {
                foreach (var error in result.ValidationErrors)
                {
                    ModelState.AddModelError(error.Field ?? "", error.ErrorMessage);
                }
            }
            else if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError("", result.Message);
            }
        }
        #endregion
    }

   
}