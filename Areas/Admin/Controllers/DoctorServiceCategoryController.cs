using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;
using System.Linq;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت صلاحیت‌های خدماتی پزشکان در سیستم کلینیک شفا
    /// مسئولیت: مدیریت صلاحیت‌های دسترسی پزشکان به دسته‌بندی‌های خدمات
    /// </summary>
    [Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorServiceCategoryController : Controller
    {
        private readonly IDoctorServiceCategoryService _doctorServiceCategoryService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorServiceCategoryViewModel> _serviceCategoryValidator;
        private readonly ILogger _logger;

        public DoctorServiceCategoryController(
            IDoctorServiceCategoryService doctorServiceCategoryService,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            IValidator<DoctorServiceCategoryViewModel> serviceCategoryValidator
            )
        {
            _doctorServiceCategoryService = doctorServiceCategoryService ?? throw new ArgumentNullException(nameof(doctorServiceCategoryService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _serviceCategoryValidator = serviceCategoryValidator ?? throw new ArgumentNullException(nameof(serviceCategoryValidator));
            _logger = Log.ForContext<DoctorServiceCategoryController>();
        }

        #region Service Category Permissions

        /// <summary>
        /// نمایش صلاحیت‌های دسته‌بندی خدمات پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ServiceCategoryPermissions(int doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش صلاحیت‌های دسته‌بندی خدمات پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

                if (doctorId <= 0)
                {
                    TempData["Error"] = "شناسه پزشک نامعتبر است.";
                    return RedirectToAction("Index", "Doctor");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    TempData["Error"] = "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت صلاحیت‌های دسته‌بندی خدمات
                var permissionsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, 100);
                if (!permissionsResult.Success)
                {
                    TempData["Error"] = permissionsResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                ViewBag.Doctor = doctorResult.Data;
                return View(permissionsResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صلاحیت‌های دسته‌بندی خدمات پزشک {DoctorId}", doctorId);
                TempData["Error"] = "خطا در بارگذاری صلاحیت‌های دسته‌بندی خدمات پزشک";
                return RedirectToAction("Index", "Doctor");
            }
        }

        /// <summary>
        /// اعطای صلاحیت دسته‌بندی خدمات به پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GrantPermission(DoctorServiceCategoryViewModel model)
        {
            try
            {
                _logger.Information("درخواست اعطای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} به پزشک {DoctorId} توسط کاربر {UserId}", 
                    model.ServiceCategoryId, model.DoctorId, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "اطلاعات وارد شده صحیح نیست.";
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _serviceCategoryValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                // اعطای صلاحیت
                var result = await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                _logger.Information("صلاحیت دسته‌بندی خدمات {ServiceCategoryId} با موفقیت به پزشک {DoctorId} اعطا شد", model.ServiceCategoryId, model.DoctorId);

                TempData["Success"] = "صلاحیت دسته‌بندی خدمات با موفقیت اعطا شد.";
                return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعطای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} به پزشک {DoctorId}", model.ServiceCategoryId, model.DoctorId);
                TempData["Error"] = "خطا در اعطای صلاحیت دسته‌بندی خدمات";
                return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
            }
        }

        /// <summary>
        /// لغو صلاحیت دسته‌بندی خدمات از پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RevokePermission(int doctorId, int serviceCategoryId)
        {
            try
            {
                _logger.Information("درخواست لغو صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از پزشک {DoctorId} توسط کاربر {UserId}", 
                    serviceCategoryId, doctorId, _currentUserService.UserId);

                if (doctorId <= 0 || serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "پارامترهای ورودی نامعتبر است." });
                }

                // لغو صلاحیت
                var result = await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(doctorId, serviceCategoryId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("صلاحیت دسته‌بندی خدمات {ServiceCategoryId} با موفقیت از پزشک {DoctorId} لغو شد", serviceCategoryId, doctorId);

                return Json(new { success = true, message = "صلاحیت دسته‌بندی خدمات با موفقیت لغو شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از پزشک {DoctorId}", serviceCategoryId, doctorId);
                return Json(new { success = false, message = "خطا در لغو صلاحیت دسته‌بندی خدمات" });
            }
        }

        /// <summary>
        /// به‌روزرسانی صلاحیت دسته‌بندی خدمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateServiceCategoryPermission(DoctorServiceCategoryViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی صلاحیت دسته‌بندی خدمات پزشک {DoctorId} توسط کاربر {UserId}", 
                    model.DoctorId, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "اطلاعات وارد شده صحیح نیست.";
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _serviceCategoryValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                // به‌روزرسانی صلاحیت
                var result = await _doctorServiceCategoryService.UpdateDoctorServiceCategoryAsync(model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                _logger.Information("صلاحیت دسته‌بندی خدمات پزشک {DoctorId} با موفقیت به‌روزرسانی شد", model.DoctorId);

                TempData["Success"] = "صلاحیت دسته‌بندی خدمات با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی صلاحیت دسته‌بندی خدمات پزشک {DoctorId}", model.DoctorId);
                TempData["Error"] = "خطا در به‌روزرسانی صلاحیت دسته‌بندی خدمات";
                return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// اعطای دسته‌ای صلاحیت دسته‌بندی خدمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkGrantPermission(int[] doctorIds, int serviceCategoryId)
        {
            try
            {
                _logger.Information("درخواست اعطای دسته‌ای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} به {DoctorCount} پزشک توسط کاربر {UserId}", 
                    serviceCategoryId, doctorIds?.Length ?? 0, _currentUserService.UserId);

                if (doctorIds == null || doctorIds.Length == 0)
                {
                    return Json(new { success = false, message = "هیچ پزشکی انتخاب نشده است." });
                }

                if (serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "دسته‌بندی خدمات نامعتبر است." });
                }

                int successCount = 0;
                var errors = new System.Collections.Generic.List<string>();

                foreach (var doctorId in doctorIds)
                {
                    try
                    {
                                                 var permissionModel = new DoctorServiceCategoryViewModel
                         {
                             DoctorId = doctorId,
                             ServiceCategoryId = serviceCategoryId,
                             IsActive = true,
                             GrantedDate = DateTime.Now
                         };

                                                 var result = await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(permissionModel);
                        if (result.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            errors.Add($"خطا در اعطای صلاحیت به پزشک {doctorId}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "خطا در اعطای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} به پزشک {DoctorId}", serviceCategoryId, doctorId);
                        errors.Add($"خطا در اعطای صلاحیت به پزشک {doctorId}");
                    }
                }

                var message = successCount > 0 
                    ? $"صلاحیت دسته‌بندی خدمات به تعداد {successCount} پزشک با موفقیت اعطا شد."
                    : "هیچ صلاحیتی اعطا نشد.";

                if (errors.Any())
                {
                    message += $" خطاها: {string.Join(", ", errors.Take(3))}";
                }

                return Json(new { 
                    success = successCount > 0, 
                    message = message,
                    successCount = successCount,
                    errorCount = errors.Count
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعطای دسته‌ای صلاحیت دسته‌بندی خدمات {ServiceCategoryId}", serviceCategoryId);
                return Json(new { success = false, message = "خطا در اعطای دسته‌ای صلاحیت دسته‌بندی خدمات" });
            }
        }

        /// <summary>
        /// لغو دسته‌ای صلاحیت دسته‌بندی خدمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkRevokePermission(int[] doctorIds, int serviceCategoryId)
        {
            try
            {
                _logger.Information("درخواست لغو دسته‌ای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از {DoctorCount} پزشک توسط کاربر {UserId}", 
                    serviceCategoryId, doctorIds?.Length ?? 0, _currentUserService.UserId);

                if (doctorIds == null || doctorIds.Length == 0)
                {
                    return Json(new { success = false, message = "هیچ پزشکی انتخاب نشده است." });
                }

                if (serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "دسته‌بندی خدمات نامعتبر است." });
                }

                int successCount = 0;
                var errors = new System.Collections.Generic.List<string>();

                foreach (var doctorId in doctorIds)
                {
                    try
                    {
                                                 var result = await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(doctorId, serviceCategoryId);
                        if (result.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            errors.Add($"خطا در لغو صلاحیت از پزشک {doctorId}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "خطا در لغو صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از پزشک {DoctorId}", serviceCategoryId, doctorId);
                        errors.Add($"خطا در لغو صلاحیت از پزشک {doctorId}");
                    }
                }

                var message = successCount > 0 
                    ? $"صلاحیت دسته‌بندی خدمات از تعداد {successCount} پزشک با موفقیت لغو شد."
                    : "هیچ صلاحیتی لغو نشد.";

                if (errors.Any())
                {
                    message += $" خطاها: {string.Join(", ", errors.Take(3))}";
                }

                return Json(new { 
                    success = successCount > 0, 
                    message = message,
                    successCount = successCount,
                    errorCount = errors.Count
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو دسته‌ای صلاحیت دسته‌بندی خدمات {ServiceCategoryId}", serviceCategoryId);
                return Json(new { success = false, message = "خطا در لغو دسته‌ای صلاحیت دسته‌بندی خدمات" });
            }
        }

        #endregion

        #region AJAX Operations

        /// <summary>
        /// دریافت لیست صلاحیت‌های دسته‌بندی خدمات پزشک (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorServiceCategoryPermissions(int doctorId)
        {
            try
            {
                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, 100);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت صلاحیت‌های دسته‌بندی خدمات پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت صلاحیت‌های دسته‌بندی خدمات پزشک" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی دسترسی پزشک به دسته‌بندی خدمات (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> CheckDoctorServiceCategoryAccess(int doctorId, int serviceCategoryId)
        {
            try
            {
                if (doctorId <= 0 || serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "پارامترهای ورودی نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, 100);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var hasAccess = result.Data?.Items?.Any(permission => 
                    permission.ServiceCategoryId == serviceCategoryId && permission.IsActive) ?? false;

                return Json(new { success = true, hasAccess = hasAccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی پزشک {DoctorId} به دسته‌بندی خدمات {ServiceCategoryId}", doctorId, serviceCategoryId);
                return Json(new { success = false, message = "خطا در بررسی دسترسی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان با صلاحیت دسته‌بندی خدمات (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategoryDoctors(int serviceCategoryId)
        {
            try
            {
                if (serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "شناسه دسته‌بندی خدمات نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                // این متد باید در سرویس پیاده‌سازی شود
                // فعلاً یک نمونه ساده
                return Json(new { success = true, data = new object[0] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان با صلاحیت دسته‌بندی خدمات {ServiceCategoryId}", serviceCategoryId);
                return Json(new { success = false, message = "خطا در دریافت لیست پزشکان با صلاحیت دسته‌بندی خدمات" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدماتی برای استفاده در لیست‌های کشویی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategories()
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت لیست دسته‌بندی‌های خدماتی");

                var result = await _doctorServiceCategoryService.GetAllServiceCategoriesAsync();
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var categories = result.Data.Select(c => new { Id = c.Id, Name = c.Name }).ToList();

                return Json(new { success = true, data = categories }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست دسته‌بندی‌های خدماتی");
                return Json(new { success = false, message = "خطا در دریافت دسته‌بندی‌های خدماتی" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
