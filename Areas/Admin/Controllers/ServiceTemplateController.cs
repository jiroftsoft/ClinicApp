using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Services;
using ClinicApp.ViewModels;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using Serilog;
using System.Data.Entity;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت قالب‌های خدمات - محیط درمانی با اطمینان 100%
    /// Service Template Management Controller for Medical Environment with 100% Reliability
    /// </summary>
    //[Authorize]
    public class ServiceTemplateController : BaseController
    {
        private readonly ServiceTemplateService _serviceTemplateService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ServiceTemplateController(
            ServiceTemplateService serviceTemplateService,
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService)
            : base(messageNotificationService)
        {
            _serviceTemplateService = serviceTemplateService ?? throw new ArgumentNullException(nameof(serviceTemplateService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Index and List Operations

        /// <summary>
        /// نمایش لیست قالب‌های خدمات
        /// </summary>
        public async Task<ActionResult> Index(int? page, int? pageSize, string searchTerm, bool? isHashtagged, bool? isActive)
        {
            try
            {
                var currentPage = page ?? 1;
                var currentPageSize = Math.Min(pageSize ?? 20, 100);

                _logger.Information("دریافت لیست قالب‌های خدمات - صفحه: {Page}, اندازه: {PageSize}, جستجو: {SearchTerm}", 
                    currentPage, currentPageSize, searchTerm);

                var query = _context.ServiceTemplates
                    .Where(st => !st.IsDeleted);

                // اعمال فیلترها
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(st => st.ServiceName.Contains(searchTerm) || 
                                            st.ServiceCode.Contains(searchTerm) ||
                                            st.Description.Contains(searchTerm));
                }

                if (isHashtagged.HasValue)
                {
                    query = query.Where(st => st.IsHashtagged == isHashtagged.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(st => st.IsActive == isActive.Value);
                }

                var totalCount = await query.CountAsync();
                var serviceTemplates = await query
                    .OrderBy(st => st.ServiceCode)
                    .Skip((currentPage - 1) * currentPageSize)
                    .Take(currentPageSize)
                    .ToListAsync();

                var viewModel = serviceTemplates.Select(st => new ServiceTemplateViewModel
                {
                    ServiceTemplateId = st.ServiceTemplateId,
                    ServiceCode = st.ServiceCode,
                    ServiceName = st.ServiceName,
                    DefaultTechnicalCoefficient = st.DefaultTechnicalCoefficient,
                    DefaultProfessionalCoefficient = st.DefaultProfessionalCoefficient,
                    IsHashtagged = st.IsHashtagged,
                    Description = st.Description,
                    IsActive = st.IsActive,
                    CreatedAt = st.CreatedAt,
                    CreatedBy = st.CreatedByUserId,
                    UpdatedAt = st.UpdatedAt,
                    UpdatedBy = st.UpdatedByUserId
                }).ToList();

                // ایجاد PagedResult
                var pagedResult = new PagedResult<ServiceTemplateViewModel>(viewModel, totalCount, currentPage, currentPageSize);

                // اطلاعات Pagination
                ViewBag.CurrentPage = currentPage;
                ViewBag.PageSize = currentPageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize);
                ViewBag.SearchTerm = searchTerm;
                ViewBag.IsHashtagged = isHashtagged;
                ViewBag.IsActive = isActive;

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست قالب‌های خدمات");
                TempData["ErrorMessage"] = "خطا در دریافت لیست قالب‌های خدمات";
                
                // ایجاد PagedResult خالی
                var emptyPagedResult = new PagedResult<ServiceTemplateViewModel>(new List<ServiceTemplateViewModel>(), 0, 1, 20);
                
                return View(emptyPagedResult);
            }
        }

        #endregion

        #region Details Operations

        /// <summary>
        /// نمایش جزئیات قالب خدمت
        /// </summary>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var serviceTemplate = await _context.ServiceTemplates
                    .Include(st => st.CreatedByUser)
                    .Include(st => st.UpdatedByUser)
                    .FirstOrDefaultAsync(st => st.ServiceTemplateId == id && !st.IsDeleted);

                if (serviceTemplate == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new ServiceTemplateDetailsViewModel
                {
                    ServiceTemplateId = serviceTemplate.ServiceTemplateId,
                    ServiceCode = serviceTemplate.ServiceCode,
                    ServiceName = serviceTemplate.ServiceName,
                    DefaultTechnicalCoefficient = serviceTemplate.DefaultTechnicalCoefficient,
                    DefaultProfessionalCoefficient = serviceTemplate.DefaultProfessionalCoefficient,
                    IsHashtagged = serviceTemplate.IsHashtagged,
                    Description = serviceTemplate.Description,
                    IsActive = serviceTemplate.IsActive,
                    CreatedAt = serviceTemplate.CreatedAt,
                    CreatedBy = serviceTemplate.CreatedByUserId,
                    CreatedByName = serviceTemplate.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = serviceTemplate.UpdatedAt,
                    UpdatedBy = serviceTemplate.UpdatedByUserId,
                    UpdatedByName = serviceTemplate.UpdatedByUser?.UserName ?? "سیستم"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات قالب خدمت {ServiceTemplateId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت جزئیات قالب خدمت";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// نمایش فرم ایجاد قالب خدمت
        /// </summary>
        public ActionResult Create()
        {
            try
            {
                var viewModel = new ServiceTemplateCreateEditViewModel
                {
                    IsActive = true,
                    IsHashtagged = false
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد قالب خدمت");
                TempData["ErrorMessage"] = "خطا در نمایش فرم ایجاد قالب خدمت";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ایجاد قالب خدمت جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ServiceTemplateCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // اعتبارسنجی Business Rules
                var validationResult = await ValidateServiceTemplateAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }

                    return View(model);
                }

                var serviceTemplate = new ServiceTemplate
                {
                    ServiceCode = model.ServiceCode,
                    ServiceName = model.ServiceName,
                    DefaultTechnicalCoefficient = model.DefaultTechnicalCoefficient,
                    DefaultProfessionalCoefficient = model.DefaultProfessionalCoefficient,
                    IsHashtagged = model.IsHashtagged,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.GetCurrentUserId()
                };

                _context.ServiceTemplates.Add(serviceTemplate);
                await _context.SaveChangesAsync();

                _logger.Information("قالب خدمت جدید ایجاد شد - شناسه: {ServiceTemplateId}, کد: {ServiceCode}", 
                    serviceTemplate.ServiceTemplateId, model.ServiceCode);

                TempData["SuccessMessage"] = "قالب خدمت با موفقیت ایجاد شد";
                return RedirectToAction("Details", new { id = serviceTemplate.ServiceTemplateId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد قالب خدمت");
                TempData["ErrorMessage"] = "خطا در ایجاد قالب خدمت: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Edit Operations

        /// <summary>
        /// نمایش فرم ویرایش قالب خدمت
        /// </summary>
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var serviceTemplate = await _context.ServiceTemplates
                    .FirstOrDefaultAsync(st => st.ServiceTemplateId == id && !st.IsDeleted);

                if (serviceTemplate == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new ServiceTemplateCreateEditViewModel
                {
                    ServiceTemplateId = serviceTemplate.ServiceTemplateId,
                    ServiceCode = serviceTemplate.ServiceCode,
                    ServiceName = serviceTemplate.ServiceName,
                    DefaultTechnicalCoefficient = serviceTemplate.DefaultTechnicalCoefficient,
                    DefaultProfessionalCoefficient = serviceTemplate.DefaultProfessionalCoefficient,
                    IsHashtagged = serviceTemplate.IsHashtagged,
                    Description = serviceTemplate.Description,
                    IsActive = serviceTemplate.IsActive
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش قالب خدمت {ServiceTemplateId}", id);
                TempData["ErrorMessage"] = "خطا در نمایش فرم ویرایش قالب خدمت";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ویرایش قالب خدمت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ServiceTemplateCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var serviceTemplate = await _context.ServiceTemplates
                    .FirstOrDefaultAsync(st => st.ServiceTemplateId == model.ServiceTemplateId && !st.IsDeleted);

                if (serviceTemplate == null)
                {
                    return HttpNotFound();
                }

                // اعتبارسنجی Business Rules
                var validationResult = await ValidateServiceTemplateAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }

                    return View(model);
                }

                // به‌روزرسانی فیلدها
                serviceTemplate.ServiceCode = model.ServiceCode;
                serviceTemplate.ServiceName = model.ServiceName;
                serviceTemplate.DefaultTechnicalCoefficient = model.DefaultTechnicalCoefficient;
                serviceTemplate.DefaultProfessionalCoefficient = model.DefaultProfessionalCoefficient;
                serviceTemplate.IsHashtagged = model.IsHashtagged;
                serviceTemplate.Description = model.Description;
                serviceTemplate.IsActive = model.IsActive;
                serviceTemplate.UpdatedAt = DateTime.UtcNow;
                serviceTemplate.UpdatedByUserId = _currentUserService.GetCurrentUserId();

                await _context.SaveChangesAsync();

                _logger.Information("قالب خدمت ویرایش شد - شناسه: {ServiceTemplateId}, کد: {ServiceCode}", 
                    model.ServiceTemplateId, model.ServiceCode);

                TempData["SuccessMessage"] = "قالب خدمت با موفقیت ویرایش شد";
                return RedirectToAction("Details", new { id = serviceTemplate.ServiceTemplateId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش قالب خدمت {ServiceTemplateId}", model.ServiceTemplateId);
                TempData["ErrorMessage"] = "خطا در ویرایش قالب خدمت: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// حذف نرم قالب خدمت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var serviceTemplate = await _context.ServiceTemplates
                    .FirstOrDefaultAsync(st => st.ServiceTemplateId == id && !st.IsDeleted);

                if (serviceTemplate == null)
                {
                    return HttpNotFound();
                }

                // حذف نرم
                serviceTemplate.IsDeleted = true;
                serviceTemplate.DeletedAt = DateTime.UtcNow;
                serviceTemplate.DeletedByUserId = _currentUserService.GetCurrentUserId();

                await _context.SaveChangesAsync();

                _logger.Information("قالب خدمت حذف شد - شناسه: {ServiceTemplateId}", id);

                TempData["SuccessMessage"] = "قالب خدمت با موفقیت حذف شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف قالب خدمت {ServiceTemplateId}", id);
                TempData["ErrorMessage"] = "خطا در حذف قالب خدمت: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region AJAX Operations

        /// <summary>
        /// دریافت قالب خدمت بر اساس کد خدمت
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetByServiceCode(string serviceCode)
        {
            try
            {
                var template = await _serviceTemplateService.GetByServiceCodeAsync(serviceCode);
                
                if (template == null)
                {
                    return Json(new { success = false, message = "قالب خدمت یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var result = new
                {
                    success = true,
                    template = new
                    {
                        template.ServiceTemplateId,
                        template.ServiceCode,
                        template.ServiceName,
                        template.DefaultTechnicalCoefficient,
                        template.DefaultProfessionalCoefficient,
                        template.IsHashtagged,
                        template.Description,
                        template.IsActive
                    }
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت قالب خدمت {ServiceCode}", serviceCode);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست قالب‌های خدمات برای منوی انتخاب
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTemplatesForSelect()
        {
            try
            {
                var templates = await _context.ServiceTemplates
                    .Where(st => !st.IsDeleted && st.IsActive)
                    .Select(st => new
                    {
                        st.ServiceTemplateId,
                        st.ServiceCode,
                        st.ServiceName,
                        st.DefaultTechnicalCoefficient,
                        st.DefaultProfessionalCoefficient,
                        st.IsHashtagged
                    })
                    .OrderBy(st => st.ServiceCode)
                    .ToListAsync();

                return Json(templates, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست قالب‌های خدمات");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// کپی قالب خدمت
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CopyTemplate(int templateId, string newServiceCode)
        {
            try
            {
                var originalTemplate = await _context.ServiceTemplates
                    .FirstOrDefaultAsync(st => st.ServiceTemplateId == templateId && !st.IsDeleted);

                if (originalTemplate == null)
                {
                    return Json(new { success = false, message = "قالب اصلی یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                // بررسی وجود کد جدید
                var existingTemplate = await _context.ServiceTemplates
                    .FirstOrDefaultAsync(st => st.ServiceCode == newServiceCode && !st.IsDeleted);

                if (existingTemplate != null)
                {
                    return Json(new { success = false, message = "قالب با این کد از قبل موجود است" }, JsonRequestBehavior.AllowGet);
                }

                var newTemplate = new ServiceTemplate
                {
                    ServiceCode = newServiceCode,
                    ServiceName = originalTemplate.ServiceName + " (کپی)",
                    DefaultTechnicalCoefficient = originalTemplate.DefaultTechnicalCoefficient,
                    DefaultProfessionalCoefficient = originalTemplate.DefaultProfessionalCoefficient,
                    IsHashtagged = originalTemplate.IsHashtagged,
                    Description = originalTemplate.Description,
                    IsActive = originalTemplate.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.GetCurrentUserId()
                };

                _context.ServiceTemplates.Add(newTemplate);
                await _context.SaveChangesAsync();

                _logger.Information("قالب خدمت کپی شد - شناسه اصلی: {OriginalId}, شناسه جدید: {NewId}, کد جدید: {NewCode}", 
                    templateId, newTemplate.ServiceTemplateId, newServiceCode);

                return Json(new { 
                    success = true, 
                    message = "قالب خدمت با موفقیت کپی شد",
                    newTemplateId = newTemplate.ServiceTemplateId
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در کپی قالب خدمت {TemplateId}", templateId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// اعتبارسنجی Business Rules برای قالب خدمت
        /// </summary>
        private async Task<ServiceTemplateValidationResult> ValidateServiceTemplateAsync(ServiceTemplateCreateEditViewModel model)
        {
            var result = new ServiceTemplateValidationResult { IsValid = true, Errors = new Dictionary<string, string>() };

            try
            {
                // بررسی وجود کد خدمت مشابه
                var existingTemplate = await _context.ServiceTemplates
                    .FirstOrDefaultAsync(st => st.ServiceCode == model.ServiceCode && 
                                             !st.IsDeleted &&
                                             (model.ServiceTemplateId == 0 || st.ServiceTemplateId != model.ServiceTemplateId));

                if (existingTemplate != null)
                {
                    result.IsValid = false;
                    result.Errors["ServiceCode"] = "قالب با این کد خدمت از قبل موجود است";
                }

                // اعتبارسنجی ضرایب
                if (model.DefaultTechnicalCoefficient < 0.01m || model.DefaultTechnicalCoefficient > 999999.99m)
                {
                    result.IsValid = false;
                    result.Errors["DefaultTechnicalCoefficient"] = "ضریب فنی باید بین 0.01 تا 999999.99 باشد";
                }

                if (model.DefaultProfessionalCoefficient < 0.01m || model.DefaultProfessionalCoefficient > 999999.99m)
                {
                    result.IsValid = false;
                    result.Errors["DefaultProfessionalCoefficient"] = "ضریب حرفه‌ای باید بین 0.01 تا 999999.99 باشد";
                }

                // اعتبارسنجی نام خدمت
                if (string.IsNullOrWhiteSpace(model.ServiceName))
                {
                    result.IsValid = false;
                    result.Errors["ServiceName"] = "نام خدمت الزامی است";
                }
                else if (model.ServiceName.Length > 200)
                {
                    result.IsValid = false;
                    result.Errors["ServiceName"] = "نام خدمت نمی‌تواند بیش از 200 کاراکتر باشد";
                }

                // اعتبارسنجی کد خدمت
                if (string.IsNullOrWhiteSpace(model.ServiceCode))
                {
                    result.IsValid = false;
                    result.Errors["ServiceCode"] = "کد خدمت الزامی است";
                }
                else if (model.ServiceCode.Length > 50)
                {
                    result.IsValid = false;
                    result.Errors["ServiceCode"] = "کد خدمت نمی‌تواند بیش از 50 کاراکتر باشد";
                }

                // اعتبارسنجی توضیحات
                if (!string.IsNullOrEmpty(model.Description) && model.Description.Length > 500)
                {
                    result.IsValid = false;
                    result.Errors["Description"] = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی قالب خدمت");
                result.IsValid = false;
                result.Errors["General"] = "خطا در اعتبارسنجی";
                return result;
            }
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// نتیجه اعتبارسنجی برای قالب‌های خدمات
    /// </summary>
    public class ServiceTemplateValidationResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
    }

    #endregion
}
