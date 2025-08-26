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
    /// کنترلر مدیریت انتصابات دپارتمان‌های پزشکان در سیستم کلینیک شفا
    /// مسئولیت: مدیریت انتصابات دپارتمان‌ها و دسترسی‌های پزشکان
    /// </summary>
    [Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorDepartmentController : Controller
    {
        private readonly IDoctorDepartmentService _doctorDepartmentService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorDepartmentViewModel> _departmentValidator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DoctorDepartmentController(
            IDoctorDepartmentService doctorDepartmentService,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            IValidator<DoctorDepartmentViewModel> departmentValidator,
            IMapper mapper)
        {
            _doctorDepartmentService = doctorDepartmentService ?? throw new ArgumentNullException(nameof(doctorDepartmentService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _departmentValidator = departmentValidator ?? throw new ArgumentNullException(nameof(departmentValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = Log.ForContext<DoctorDepartmentController>();
        }

        #region Department Assignments

        /// <summary>
        /// نمایش انتصابات دپارتمان پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> DepartmentAssignments(int doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش انتصابات دپارتمان پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

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

                // دریافت انتصابات دپارتمان
                var assignmentsResult = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, 100);
                if (!assignmentsResult.Success)
                {
                    TempData["Error"] = assignmentsResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                ViewBag.Doctor = doctorResult.Data;
                return View(assignmentsResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش انتصابات دپارتمان پزشک {DoctorId}", doctorId);
                TempData["Error"] = "خطا در بارگذاری انتصابات دپارتمان پزشک";
                return RedirectToAction("Index", "Doctor");
            }
        }

        /// <summary>
        /// اضافه کردن پزشک به دپارتمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddToDepartment(DoctorDepartmentViewModel model)
        {
            try
            {
                _logger.Information("درخواست اضافه کردن پزشک {DoctorId} به دپارتمان {DepartmentId} توسط کاربر {UserId}", 
                    model.DoctorId, model.DepartmentId, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "اطلاعات وارد شده صحیح نیست.";
                    return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _departmentValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
                }

                // اضافه کردن به دپارتمان
                var result = await _doctorDepartmentService.AssignDoctorToDepartmentAsync(model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
                }

                _logger.Information("پزشک {DoctorId} با موفقیت به دپارتمان {DepartmentId} اضافه شد", model.DoctorId, model.DepartmentId);

                TempData["Success"] = "پزشک با موفقیت به دپارتمان اضافه شد.";
                return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اضافه کردن پزشک {DoctorId} به دپارتمان {DepartmentId}", model.DoctorId, model.DepartmentId);
                TempData["Error"] = "خطا در اضافه کردن پزشک به دپارتمان";
                return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
            }
        }

        /// <summary>
        /// حذف پزشک از دپارتمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveFromDepartment(int doctorId, int departmentId)
        {
            try
            {
                _logger.Information("درخواست حذف پزشک {DoctorId} از دپارتمان {DepartmentId} توسط کاربر {UserId}", 
                    doctorId, departmentId, _currentUserService.UserId);

                if (doctorId <= 0 || departmentId <= 0)
                {
                    return Json(new { success = false, message = "پارامترهای ورودی نامعتبر است." });
                }

                // حذف از دپارتمان
                var result = await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(doctorId, departmentId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("پزشک {DoctorId} با موفقیت از دپارتمان {DepartmentId} حذف شد", doctorId, departmentId);

                return Json(new { success = true, message = "پزشک با موفقیت از دپارتمان حذف شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف پزشک {DoctorId} از دپارتمان {DepartmentId}", doctorId, departmentId);
                return Json(new { success = false, message = "خطا در حذف پزشک از دپارتمان" });
            }
        }

        /// <summary>
        /// به‌روزرسانی انتصاب دپارتمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateDepartmentAssignment(DoctorDepartmentViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی انتصاب دپارتمان پزشک {DoctorId} توسط کاربر {UserId}", 
                    model.DoctorId, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "اطلاعات وارد شده صحیح نیست.";
                    return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _departmentValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
                }

                // به‌روزرسانی انتصاب
                var result = await _doctorDepartmentService.UpdateDoctorDepartmentAssignmentAsync(model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
                }

                _logger.Information("انتصاب دپارتمان پزشک {DoctorId} با موفقیت به‌روزرسانی شد", model.DoctorId);

                TempData["Success"] = "انتصاب دپارتمان با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی انتصاب دپارتمان پزشک {DoctorId}", model.DoctorId);
                TempData["Error"] = "خطا در به‌روزرسانی انتصاب دپارتمان";
                return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// انتصاب دسته‌ای پزشکان به دپارتمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkAssignToDepartment(int[] doctorIds, int departmentId)
        {
            try
            {
                _logger.Information("درخواست انتصاب دسته‌ای {DoctorCount} پزشک به دپارتمان {DepartmentId} توسط کاربر {UserId}", 
                    doctorIds?.Length ?? 0, departmentId, _currentUserService.UserId);

                if (doctorIds == null || doctorIds.Length == 0)
                {
                    return Json(new { success = false, message = "هیچ پزشکی انتخاب نشده است." });
                }

                if (departmentId <= 0)
                {
                    return Json(new { success = false, message = "دپارتمان نامعتبر است." });
                }

                int successCount = 0;
                var errors = new System.Collections.Generic.List<string>();

                foreach (var doctorId in doctorIds)
                {
                    try
                    {
                        var assignmentModel = new DoctorDepartmentViewModel
                        {
                            DoctorId = doctorId,
                            DepartmentId = departmentId,
                            IsActive = true,
                            StartDate = DateTime.Now
                        };

                        var result = await _doctorDepartmentService.AssignDoctorToDepartmentAsync(assignmentModel);
                        if (result.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            errors.Add($"خطا در انتصاب پزشک {doctorId}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "خطا در انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);
                        errors.Add($"خطا در انتصاب پزشک {doctorId}");
                    }
                }

                var message = successCount > 0 
                    ? $"تعداد {successCount} پزشک با موفقیت انتصاب شدند."
                    : "هیچ پزشکی انتصاب نشد.";

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
                _logger.Error(ex, "خطا در انتصاب دسته‌ای پزشکان به دپارتمان {DepartmentId}", departmentId);
                return Json(new { success = false, message = "خطا در انتصاب دسته‌ای پزشکان" });
            }
        }

        /// <summary>
        /// لغو انتصاب دسته‌ای پزشکان از دپارتمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkRemoveFromDepartment(int[] doctorIds, int departmentId)
        {
            try
            {
                _logger.Information("درخواست لغو انتصاب دسته‌ای {DoctorCount} پزشک از دپارتمان {DepartmentId} توسط کاربر {UserId}", 
                    doctorIds?.Length ?? 0, departmentId, _currentUserService.UserId);

                if (doctorIds == null || doctorIds.Length == 0)
                {
                    return Json(new { success = false, message = "هیچ پزشکی انتخاب نشده است." });
                }

                if (departmentId <= 0)
                {
                    return Json(new { success = false, message = "دپارتمان نامعتبر است." });
                }

                int successCount = 0;
                var errors = new System.Collections.Generic.List<string>();

                foreach (var doctorId in doctorIds)
                {
                    try
                    {
                        var result = await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(doctorId, departmentId);
                        if (result.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            errors.Add($"خطا در لغو انتصاب پزشک {doctorId}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "خطا در لغو انتصاب پزشک {DoctorId} از دپارتمان {DepartmentId}", doctorId, departmentId);
                        errors.Add($"خطا در لغو انتصاب پزشک {doctorId}");
                    }
                }

                var message = successCount > 0 
                    ? $"انتساب تعداد {successCount} پزشک با موفقیت لغو شد."
                    : "هیچ انتصابی لغو نشد.";

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
                _logger.Error(ex, "خطا در لغو انتصاب دسته‌ای پزشکان از دپارتمان {DepartmentId}", departmentId);
                return Json(new { success = false, message = "خطا در لغو انتصاب دسته‌ای پزشکان" });
            }
        }

        #endregion

        #region AJAX Operations

        /// <summary>
        /// دریافت لیست انتصابات دپارتمان پزشک (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorDepartmentAssignments(int doctorId)
        {
            try
            {
                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, 100);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتصابات دپارتمان پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت انتصابات دپارتمان پزشک" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی دسترسی پزشک به دپارتمان (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> CheckDoctorDepartmentAccess(int doctorId, int departmentId)
        {
            try
            {
                if (doctorId <= 0 || departmentId <= 0)
                {
                    return Json(new { success = false, message = "پارامترهای ورودی نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, 100);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var hasAccess = result.Data?.Items?.Any(assignment => 
                    assignment.DepartmentId == departmentId && assignment.IsActive) ?? false;

                return Json(new { success = true, hasAccess = hasAccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);
                return Json(new { success = false, message = "خطا در بررسی دسترسی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان دپارتمان (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDepartmentDoctors(int departmentId)
        {
            try
            {
                if (departmentId <= 0)
                {
                    return Json(new { success = false, message = "شناسه دپارتمان نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                // این متد باید در سرویس پیاده‌سازی شود
                // فعلاً یک نمونه ساده
                return Json(new { success = true, data = new object[0] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان دپارتمان {DepartmentId}", departmentId);
                return Json(new { success = false, message = "خطا در دریافت لیست پزشکان دپارتمان" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
