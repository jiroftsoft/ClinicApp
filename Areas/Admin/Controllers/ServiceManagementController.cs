using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// نمونه کنترلر برای نشان دادن استفاده از IServiceManagementService
    /// Sample Controller demonstrating ServiceManagementService usage
    /// 
    /// ✅ Clean Architecture Principles Applied:
    /// - Controller is thin and focuses only on HTTP concerns
    /// - All business logic is in ServiceManagementService
    /// - Consistent error handling and logging
    /// - Persian localization for medical environments
    /// </summary>
    //[Authorize]
    public class ServiceManagementController : Controller
    {
        private readonly IServiceManagementService _serviceManagementService;

        public ServiceManagementController(IServiceManagementService serviceManagementService)
        {
            _serviceManagementService = serviceManagementService;
        }

        #region Service Category Management

        /// <summary>
        /// نمایش لیست دسته‌بندی‌های خدمات یک دپارتمان
        /// </summary>
        public async Task<ActionResult> ServiceCategories(int departmentId, string searchTerm = "", int page = 1, int pageSize = 10)
        {
            var result = await _serviceManagementService.GetServiceCategoriesAsync(departmentId, searchTerm, page, pageSize);
            
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View("Error");
            }

            ViewBag.DepartmentId = departmentId;
            ViewBag.SearchTerm = searchTerm;
            
            return View(result.Data);
        }

        /// <summary>
        /// نمایش فرم ایجاد دسته‌بندی خدمات جدید
        /// </summary>
        public ActionResult CreateServiceCategory(int departmentId)
        {
            var model = new ServiceCategoryCreateEditViewModel
            {
                DepartmentId = departmentId,
                IsActive = true
            };
            
            return View(model);
        }

        /// <summary>
        /// پردازش فرم ایجاد دسته‌بندی خدمات جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateServiceCategory(ServiceCategoryCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _serviceManagementService.CreateServiceCategoryAsync(model);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("ServiceCategories", new { departmentId = model.DepartmentId });
            }

            // اضافه کردن خطاهای Validation به ModelState
            if (result.ValidationErrors?.Count > 0)
            {
                foreach (var error in result.ValidationErrors)
                {
                    ModelState.AddModelError(error.Field, error.ErrorMessage);
                }
            }
            else
            {
                ModelState.AddModelError("", result.Message);
            }
            
            return View(model);
        }

        /// <summary>
        /// نمایش جزئیات کامل یک دسته‌بندی خدمات
        /// </summary>
        public async Task<ActionResult> ServiceCategoryDetails(int id)
        {
            var result = await _serviceManagementService.GetServiceCategoryDetailsAsync(id);
            
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("ServiceCategories");
            }
            
            return View(result.Data);
        }

        #endregion

        #region Service Management

        /// <summary>
        /// نمایش لیست خدمات یک دسته‌بندی
        /// </summary>
        public async Task<ActionResult> Services(int serviceCategoryId, string searchTerm = "", int page = 1, int pageSize = 10)
        {
            var result = await _serviceManagementService.GetServicesAsync(serviceCategoryId, searchTerm, page, pageSize);
            
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View("Error");
            }

            ViewBag.ServiceCategoryId = serviceCategoryId;
            ViewBag.SearchTerm = searchTerm;
            
            return View(result.Data);
        }

        /// <summary>
        /// نمایش فرم ایجاد خدمت جدید
        /// </summary>
        public ActionResult CreateService(int serviceCategoryId)
        {
            var model = new ServiceCreateEditViewModel
            {
                ServiceCategoryId = serviceCategoryId,
                IsActive = true
            };
            
            return View(model);
        }

        /// <summary>
        /// پردازش فرم ایجاد خدمت جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateService(ServiceCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _serviceManagementService.CreateServiceAsync(model);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Services", new { serviceCategoryId = model.ServiceCategoryId });
            }

            // اضافه کردن خطاهای Validation به ModelState
            if (result.ValidationErrors?.Count > 0)
            {
                foreach (var error in result.ValidationErrors)
                {
                    ModelState.AddModelError(error.Field, error.ErrorMessage);
                }
            }
            else
            {
                ModelState.AddModelError("", result.Message);
            }
            
            return View(model);
        }

        #endregion

        #region AJAX API Methods

        /// <summary>
        /// API برای دریافت لیست دسته‌بندی‌های فعال جهت استفاده در لیست‌های کشویی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetActiveServiceCategories(int departmentId)
        {
            var result = await _serviceManagementService.GetActiveServiceCategoriesForLookupAsync(departmentId);
            
            if (result.Success)
            {
                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            
            return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// API برای دریافت لیست خدمات فعال جهت استفاده در لیست‌های کشویی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetActiveServices(int serviceCategoryId)
        {
            var result = await _serviceManagementService.GetActiveServicesForLookupAsync(serviceCategoryId);
            
            if (result.Success)
            {
                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            
            return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// API برای حذف نرم دسته‌بندی خدمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteServiceCategory(int id)
        {
            var result = await _serviceManagementService.SoftDeleteServiceCategoryAsync(id);
            
            return Json(new { success = result.Success, message = result.Message });
        }

        /// <summary>
        /// API برای حذف نرم خدمت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteService(int id)
        {
            var result = await _serviceManagementService.SoftDeleteServiceAsync(id);
            
            return Json(new { success = result.Success, message = result.Message });
        }

        #endregion
    }
}
