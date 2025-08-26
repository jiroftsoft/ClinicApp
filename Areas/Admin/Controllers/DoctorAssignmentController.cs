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
using System.Collections.Generic;
using System.Linq;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت انتصابات پزشکان در سیستم کلینیک شفا
    /// مسئولیت: مدیریت انتصابات کلی (دپارتمان + سرفصل‌های خدماتی)
    /// </summary>
    [Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorAssignmentController : Controller
    {
        private readonly IDoctorAssignmentService _doctorAssignmentService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorAssignmentsViewModel> _assignmentsValidator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DoctorAssignmentController(
            IDoctorAssignmentService doctorAssignmentService,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            IValidator<DoctorAssignmentsViewModel> assignmentsValidator,
            IMapper mapper)
        {
            _doctorAssignmentService = doctorAssignmentService ?? throw new ArgumentNullException(nameof(doctorAssignmentService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _assignmentsValidator = assignmentsValidator ?? throw new ArgumentNullException(nameof(assignmentsValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = Log.ForContext<DoctorAssignmentController>();
        }

        #region Assignments Management

        /// <summary>
        /// نمایش صفحه مدیریت انتصابات پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Assignments(int doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش انتصابات پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

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

                // دریافت انتصابات فعلی
                var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId);
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
                _logger.Error(ex, "خطا در نمایش انتصابات پزشک {DoctorId}", doctorId);
                TempData["Error"] = "خطا در بارگذاری انتصابات پزشک";
                return RedirectToAction("Index", "Doctor");
            }
        }

        /// <summary>
        /// به‌روزرسانی انتصابات پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateAssignments(DoctorAssignmentsViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی انتصابات پزشک {DoctorId} توسط کاربر {UserId}", model.DoctorId, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "اطلاعات وارد شده صحیح نیست.";
                    return RedirectToAction("Assignments", new { doctorId = model.DoctorId });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _assignmentsValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return RedirectToAction("Assignments", new { doctorId = model.DoctorId });
                }

                // به‌روزرسانی انتصابات
                var result = await _doctorAssignmentService.UpdateDoctorAssignmentsAsync(model.DoctorId, model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Assignments", new { doctorId = model.DoctorId });
                }

                _logger.Information("انتصابات پزشک {DoctorId} با موفقیت به‌روزرسانی شد", model.DoctorId);

                TempData["Success"] = "انتصابات پزشک با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("Assignments", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی انتصابات پزشک {DoctorId}", model.DoctorId);
                TempData["Error"] = "خطا در به‌روزرسانی انتصابات پزشک";
                return RedirectToAction("Assignments", new { doctorId = model.DoctorId });
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
                var errors = new List<string>();

                foreach (var doctorId in doctorIds)
                {
                    try
                    {
                        // دریافت انتصابات فعلی پزشک
                        var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId);
                        if (assignmentsResult.Success && assignmentsResult.Data != null)
                        {
                            var assignments = assignmentsResult.Data;

                            // بررسی اینکه آیا پزشک قبلاً در این دپارتمان انتصاب دارد
                            var existingAssignment = assignments.DoctorDepartments?.FirstOrDefault(dd => dd.DepartmentId == departmentId);
                            if (existingAssignment == null)
                            {
                                // اضافه کردن انتصاب جدید
                                assignments.DoctorDepartments.Add(new DoctorDepartmentViewModel
                                {
                                    DoctorId = doctorId,
                                    DepartmentId = departmentId,
                                    IsActive = true,
                                    StartDate = DateTime.Now
                                });

                                // به‌روزرسانی انتصابات
                                var updateResult = await _doctorAssignmentService.UpdateDoctorAssignmentsAsync(doctorId, assignments);
                                if (updateResult.Success)
                                {
                                    successCount++;
                                }
                                else
                                {
                                    errors.Add($"خطا در انتصاب پزشک {doctorId}: {updateResult.Message}");
                                }
                            }
                            else
                            {
                                successCount++; // قبلاً انتصاب دارد
                            }
                        }
                        else
                        {
                            errors.Add($"خطا در دریافت انتصابات پزشک {doctorId}");
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
                var errors = new List<string>();

                foreach (var doctorId in doctorIds)
                {
                    try
                    {
                        // دریافت انتصابات فعلی پزشک
                        var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId);
                        if (assignmentsResult.Success && assignmentsResult.Data != null)
                        {
                            var assignments = assignmentsResult.Data;

                            // حذف انتصاب از دپارتمان
                            assignments.DoctorDepartments?.RemoveAll(dd => dd.DepartmentId == departmentId);

                            // به‌روزرسانی انتصابات
                            var updateResult = await _doctorAssignmentService.UpdateDoctorAssignmentsAsync(doctorId, assignments);
                            if (updateResult.Success)
                            {
                                successCount++;
                            }
                            else
                            {
                                errors.Add($"خطا در لغو انتصاب پزشک {doctorId}: {updateResult.Message}");
                            }
                        }
                        else
                        {
                            errors.Add($"خطا در دریافت انتصابات پزشک {doctorId}");
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
        /// دریافت لیست انتصابات پزشک (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorAssignments(int doctorId)
        {
            try
            {
                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتصابات پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت انتصابات پزشک" }, JsonRequestBehavior.AllowGet);
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

                var result = await _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var hasAccess = result.Data?.DoctorDepartments?.Any(dd => 
                    dd.DepartmentId == departmentId && dd.IsActive) ?? false;

                return Json(new { success = true, hasAccess = hasAccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);
                return Json(new { success = false, message = "خطا در بررسی دسترسی" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
