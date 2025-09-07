using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
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
    //[Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorDepartmentController : Controller
    {
        private readonly IDoctorDepartmentService _doctorDepartmentService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorDepartmentViewModel> _departmentValidator;
        private readonly ILogger _logger;

        public DoctorDepartmentController(
            IDoctorDepartmentService doctorDepartmentService,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            IValidator<DoctorDepartmentViewModel> departmentValidator)
        {
            _doctorDepartmentService = doctorDepartmentService ?? throw new ArgumentNullException(nameof(doctorDepartmentService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _departmentValidator = departmentValidator ?? throw new ArgumentNullException(nameof(departmentValidator));
            _logger = Log.ForContext<DoctorDepartmentController>();
        }

        #region Department Assignments

        /// <summary>
        /// نمایش انتصابات دپارتمان پزشک - نسخه Ultimate Production Ready
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> DepartmentAssignments(int doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش انتصابات دپارتمان پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

                // اعتبارسنجی ورودی
                if (doctorId <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر: {DoctorId}", doctorId);
                    TempData["ErrorMessage"] = "شناسه پزشک نامعتبر است.";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    TempData["ErrorMessage"] = "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت انتصابات دپارتمان
                var assignmentsResult = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, 100);
                if (!assignmentsResult.Success)
                {
                    _logger.Warning("خطا در دریافت انتسابات پزشک {DoctorId}: {Message}", doctorId, assignmentsResult.Message);
                    TempData["ErrorMessage"] = assignmentsResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت لیست دپارتمان‌های موجود
                var availableDepartmentsResult = await _doctorDepartmentService.GetAllDepartmentsAsync();
                var availableDepartments = new List<System.Web.Mvc.SelectListItem>();
                if (availableDepartmentsResult.Success)
                {
                    availableDepartments = availableDepartmentsResult.Data
                        .Select(d => new System.Web.Mvc.SelectListItem 
                        { 
                            Value = d.Id.ToString(), 
                            Text = d.Name 
                        })
                        .ToList();
                }

                // محاسبه آمار
                var assignments = assignmentsResult.Data.Items;
                var stats = new DepartmentAssignmentStatsViewModel
                {
                    TotalAssignments = assignments.Count,
                    ActiveAssignments = assignments.Count(a => a.IsActive),
                    InactiveAssignments = assignments.Count(a => !a.IsActive),
                    LastAssignmentDate = assignments.Any() ? assignments.Max(a => a.CreatedAt) : (DateTime?)null
                };

                // ایجاد ViewModel نهایی
                var viewModel = new DoctorDepartmentAssignmentsViewModel
                {
                    Doctor = doctorResult.Data,
                    Assignments = assignments.ToList(),
                    Stats = stats,
                    AvailableDepartments = availableDepartments
                };

                _logger.Information("انتسابات دپارتمان پزشک {DoctorId} با موفقیت نمایش داده شد. تعداد انتسابات: {Count}", 
                    doctorId, assignments.Count);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش انتصابات دپارتمان پزشک {DoctorId}", doctorId);
                TempData["ErrorMessage"] = "خطا در بارگذاری انتصابات دپارتمان پزشک";
                return View(new DoctorDepartmentAssignmentsViewModel
                {
                    Doctor = new DoctorDetailsViewModel { DoctorId = doctorId, FullName = "نامشخص" },
                    Assignments = new List<DoctorDepartmentViewModel>(),
                    Stats = new DepartmentAssignmentStatsViewModel(),
                    AvailableDepartments = new List<System.Web.Mvc.SelectListItem>()
                });
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

        /// <summary>
        /// دریافت اطلاعات دپارتمان برای مقایسه (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDepartmentInfo(int id)
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت اطلاعات دپارتمان {DepartmentId}", id);

                if (id <= 0)
                {
                    return Json(new { success = false, message = "شناسه دپارتمان نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorDepartmentService.GetAllDepartmentsAsync();
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var department = result.Data.FirstOrDefault(d => d.Id == id);
                if (department == null)
                {
                    return Json(new { success = false, message = "دپارتمان مورد نظر یافت نشد." }, JsonRequestBehavior.AllowGet);
                }

                var departmentInfo = new DepartmentInfoResponse
                {
                    Id = department.Id,
                    Name = department.Name,
                    Code = department.Code ?? "بدون کد",
                    DoctorCount = 0, // این مقدار باید از سرویس دریافت شود
                    ServiceCount = 0  // این مقدار باید از سرویس دریافت شود
                };

                return Json(new { success = true, data = departmentInfo }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات دپارتمان {DepartmentId}", id);
                return Json(new { success = false, message = "خطا در دریافت اطلاعات دپارتمان" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست دپارتمان‌ها برای استفاده در فیلترها و لیست‌های کشویی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDepartments()
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت لیست دپارتمان‌ها");

                var result = await _doctorDepartmentService.GetAllDepartmentsAsync();
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var departments = result.Data.Select(d => new DepartmentListItemResponse { Id = d.Id, Name = d.Name }).ToList();

                return Json(new { success = true, data = departments }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست دپارتمان‌ها");
                return Json(new { success = false, message = "خطا در دریافت دپارتمان‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Assignment Operations

        /// <summary>
        /// نمایش فرم انتساب پزشک به دپارتمان - نسخه Ultimate Production Ready
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> AssignToDepartment(int? doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش فرم انتساب پزشک {DoctorId} به دپارتمان", doctorId);

                // اعتبارسنجی ورودی
                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر یا خالی: {DoctorId}", doctorId);
                    TempData["ErrorMessage"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["ErrorMessage"] = doctorResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت لیست دپارتمان‌های فعال
                var departmentsResult = await _doctorDepartmentService.GetAllDepartmentsAsync();
                var departments = new List<System.Web.Mvc.SelectListItem>();
                if (departmentsResult.Success)
                {
                    departments = departmentsResult.Data
                        .Select(d => new System.Web.Mvc.SelectListItem 
                        { 
                            Value = d.Id.ToString(), 
                            Text = d.Name 
                        })
                        .ToList();
                }
                else
                {
                    _logger.Warning("خطا در دریافت لیست دپارتمان‌ها: {Message}", departmentsResult.Message);
                }

                // ایجاد ViewModel نهایی
                var viewModel = new DoctorDepartmentAssignFormViewModel
                {
                    Doctor = doctorResult.Data,
                    Assignment = new DoctorAssignmentOperationViewModel
                    {
                        DoctorId = doctorId.Value,
                        DoctorName = doctorResult.Data.FullName,
                        DoctorNationalCode = doctorResult.Data.NationalCode,
                        IsActive = true
                    },
                    Departments = departments
                };

                _logger.Information("فرم انتساب پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم انتساب پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم انتساب";
                return View(new DoctorDepartmentAssignFormViewModel
                {
                    Doctor = new DoctorDetailsViewModel { DoctorId = doctorId ?? 0, FullName = "نامشخص" },
                    Assignment = new DoctorAssignmentOperationViewModel(),
                    Departments = new List<System.Web.Mvc.SelectListItem>()
                });
            }
        }

        /// <summary>
        /// پردازش انتساب پزشک به دپارتمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignToDepartment(DoctorAssignmentOperationViewModel model)
        {
            try
            {
                _logger.Information("درخواست انتساب پزشک {DoctorId} به دپارتمان {DepartmentId}. ModelState.IsValid: {IsValid}", 
                    model?.DoctorId, model?.DepartmentId, ModelState.IsValid);
                
                // Log all form values for debugging
                _logger.Information("Form values - DoctorId: {DoctorId}, DepartmentId: {DepartmentId}, IsActive: {IsActive}", 
                    model?.DoctorId, model?.DepartmentId, model?.IsActive);

                if (!ModelState.IsValid)
                {
                    var modelStateErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    _logger.Warning("مدل انتساب نامعتبر برای پزشک {DoctorId}. خطاها: {@Errors}", model?.DoctorId, modelStateErrors);
                    
                    // Log all ModelState keys and values for debugging
                    foreach (var key in ModelState.Keys)
                    {
                        var value = ModelState[key];
                        _logger.Information("ModelState Key: {Key}, Value: {Value}, Errors: {Errors}", 
                            key, value?.Value?.AttemptedValue, string.Join(", ", value?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>()));
                    }
                    
                    TempData["Error"] = $"اطلاعات وارد شده نامعتبر است: {string.Join(", ", modelStateErrors)}";
                    return RedirectToAction("AssignToDepartment", new { doctorId = model?.DoctorId ?? 0 });
                }

                // تبدیل به DoctorDepartmentViewModel
                var departmentModel = new DoctorDepartmentViewModel
                {
                    DoctorId = model.DoctorId,
                    DepartmentId = model.DepartmentId,
                    IsActive = model.IsActive,
                    StartDate = DateTime.Now,
                    Role = "پزشک عادی" // نقش پیش‌فرض
                };

                // اعتبارسنجی با FluentValidation
                var validationResult = await _departmentValidator.ValidateAsync(departmentModel);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return RedirectToAction("AssignToDepartment", new { doctorId = model.DoctorId });
                }

                // انتساب پزشک به دپارتمان
                var result = await _doctorDepartmentService.AssignDoctorToDepartmentAsync(departmentModel);

                if (!result.Success)
                {
                    _logger.Warning("انتساب پزشک {DoctorId} ناموفق بود: {Message}", model.DoctorId, result.Message);
                    TempData["Error"] = result.Message;
                    return RedirectToAction("AssignToDepartment", new { doctorId = model.DoctorId });
                }

                _logger.Information("انتساب پزشک {DoctorId} به دپارتمان {DepartmentId} با موفقیت انجام شد", 
                    model.DoctorId, model.DepartmentId);
                TempData["Success"] = "انتساب پزشک با موفقیت انجام شد";
                return RedirectToAction("Details", "DoctorAssignment", new { id = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتساب پزشک {DoctorId} به دپارتمان", model.DoctorId);
                TempData["Error"] = "خطا در انجام عملیات انتساب";
                return RedirectToAction("AssignToDepartment", new { doctorId = model.DoctorId });
            }
        }

        #endregion

        #region Assignment Management Actions

        /// <summary>
        /// فعال کردن انتصاب دپارتمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ActivateAssignment(string assignmentId)
        {
            try
            {
                _logger.Information("درخواست فعال کردن انتصاب دپارتمان {AssignmentId} توسط کاربر {UserId}", 
                    assignmentId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(assignmentId))
                {
                    return Json(new { success = false, message = "شناسه انتصاب نامعتبر است." });
                }

                // تجزیه assignmentId (فرمت: DoctorId_DepartmentId)
                var parts = assignmentId.Split('_');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int doctorId) || !int.TryParse(parts[1], out int departmentId))
                {
                    return Json(new { success = false, message = "فرمت شناسه انتصاب نامعتبر است." });
                }

                // فعال کردن انتصاب - استفاده از متد موجود
                var assignmentModel = new DoctorDepartmentViewModel
                {
                    DoctorId = doctorId,
                    DepartmentId = departmentId,
                    IsActive = true
                };
                var result = await _doctorDepartmentService.UpdateDoctorDepartmentAssignmentAsync(assignmentModel);
                if (!result.Success)
                {
                    _logger.Warning("فعال کردن انتصاب {AssignmentId} ناموفق بود: {Message}", assignmentId, result.Message);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("انتصاب دپارتمان {AssignmentId} با موفقیت فعال شد", assignmentId);
                return Json(new { success = true, message = "انتصاب با موفقیت فعال شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال کردن انتصاب دپارتمان {AssignmentId}", assignmentId);
                return Json(new { success = false, message = "خطا در فعال کردن انتصاب" });
            }
        }

        /// <summary>
        /// غیرفعال کردن انتصاب دپارتمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeactivateAssignment(string assignmentId)
        {
            try
            {
                _logger.Information("درخواست غیرفعال کردن انتصاب دپارتمان {AssignmentId} توسط کاربر {UserId}", 
                    assignmentId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(assignmentId))
                {
                    return Json(new { success = false, message = "شناسه انتصاب نامعتبر است." });
                }

                // تجزیه assignmentId (فرمت: DoctorId_DepartmentId)
                var parts = assignmentId.Split('_');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int doctorId) || !int.TryParse(parts[1], out int departmentId))
                {
                    return Json(new { success = false, message = "فرمت شناسه انتصاب نامعتبر است." });
                }

                // غیرفعال کردن انتصاب - استفاده از متد موجود
                var assignmentModel = new DoctorDepartmentViewModel
                {
                    DoctorId = doctorId,
                    DepartmentId = departmentId,
                    IsActive = false
                };
                var result = await _doctorDepartmentService.UpdateDoctorDepartmentAssignmentAsync(assignmentModel);
                if (!result.Success)
                {
                    _logger.Warning("غیرفعال کردن انتصاب {AssignmentId} ناموفق بود: {Message}", assignmentId, result.Message);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("انتصاب دپارتمان {AssignmentId} با موفقیت غیرفعال شد", assignmentId);
                return Json(new { success = true, message = "انتصاب با موفقیت غیرفعال شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال کردن انتصاب دپارتمان {AssignmentId}", assignmentId);
                return Json(new { success = false, message = "خطا در غیرفعال کردن انتصاب" });
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش انتصاب دپارتمان
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> EditAssignment(string assignmentId)
        {
            try
            {
                _logger.Information("درخواست ویرایش انتصاب دپارتمان {AssignmentId} توسط کاربر {UserId}", 
                    assignmentId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(assignmentId))
                {
                    TempData["ErrorMessage"] = "شناسه انتصاب نامعتبر است.";
                    return RedirectToAction("Index", "Doctor");
                }

                // تجزیه assignmentId (فرمت: DoctorId_DepartmentId)
                var parts = assignmentId.Split('_');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int doctorId) || !int.TryParse(parts[1], out int departmentId))
                {
                    TempData["ErrorMessage"] = "فرمت شناسه انتصاب نامعتبر است.";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت اطلاعات انتصاب
                var assignmentResult = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, 100);
                if (!assignmentResult.Success)
                {
                    TempData["ErrorMessage"] = assignmentResult.Message;
                    return RedirectToAction("DepartmentAssignments", new { doctorId = doctorId });
                }

                var assignment = assignmentResult.Data.Items.FirstOrDefault(a => a.DepartmentId == departmentId);
                if (assignment == null)
                {
                    TempData["ErrorMessage"] = "انتصاب مورد نظر یافت نشد.";
                    return RedirectToAction("DepartmentAssignments", new { doctorId = doctorId });
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success)
                {
                    TempData["ErrorMessage"] = doctorResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت لیست دپارتمان‌ها
                var departmentsResult = await _doctorDepartmentService.GetAllDepartmentsAsync();
                var departments = new List<System.Web.Mvc.SelectListItem>();
                if (departmentsResult.Success)
                {
                    departments = departmentsResult.Data
                        .Select(d => new System.Web.Mvc.SelectListItem 
                        { 
                            Value = d.Id.ToString(), 
                            Text = d.Name,
                            Selected = d.Id == departmentId
                        })
                        .ToList();
                }

                var viewModel = new DoctorDepartmentAssignFormViewModel
                {
                    Doctor = doctorResult.Data,
                    Assignment = new DoctorAssignmentOperationViewModel
                    {
                        DoctorId = doctorId,
                        DoctorName = doctorResult.Data.FullName,
                        DoctorNationalCode = doctorResult.Data.NationalCode,
                        DepartmentId = departmentId,
                        DepartmentName = assignment.DepartmentName,
                        IsActive = assignment.IsActive,
                        Description = assignment.Description
                    },
                    Departments = departments
                };

                _logger.Information("فرم ویرایش انتصاب دپارتمان {AssignmentId} با موفقیت نمایش داده شد", assignmentId);
                return View("AssignToDepartment", viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش انتصاب دپارتمان {AssignmentId}", assignmentId);
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم ویرایش";
                return RedirectToAction("Index", "Doctor");
            }
        }

        /// <summary>
        /// حذف انتصاب دپارتمان
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RemoveAssignment(string assignmentId)
        {
            try
            {
                _logger.Information("درخواست حذف انتصاب دپارتمان {AssignmentId} توسط کاربر {UserId}", 
                    assignmentId, _currentUserService.UserId);

                if (string.IsNullOrEmpty(assignmentId))
                {
                    return Json(new { success = false, message = "شناسه انتصاب نامعتبر است." });
                }

                // تجزیه assignmentId (فرمت: DoctorId_DepartmentId)
                var parts = assignmentId.Split('_');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int doctorId) || !int.TryParse(parts[1], out int departmentId))
                {
                    return Json(new { success = false, message = "فرمت شناسه انتصاب نامعتبر است." });
                }

                // حذف انتصاب
                var result = await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(doctorId, departmentId);
                if (!result.Success)
                {
                    _logger.Warning("حذف انتصاب {AssignmentId} ناموفق بود: {Message}", assignmentId, result.Message);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("انتصاب دپارتمان {AssignmentId} با موفقیت حذف شد", assignmentId);
                return Json(new { success = true, message = "انتصاب با موفقیت حذف شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف انتصاب دپارتمان {AssignmentId}", assignmentId);
                return Json(new { success = false, message = "خطا در حذف انتصاب" });
            }
        }

        #endregion
    }

    /// <summary>
    /// Response class برای اطلاعات دپارتمان
    /// </summary>
    public class DepartmentInfoResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int DoctorCount { get; set; }
        public int ServiceCount { get; set; }
    }

    /// <summary>
    /// Response class برای لیست دپارتمان‌ها
    /// </summary>
    public class DepartmentListItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
