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

                var departmentInfo = new
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

                var departments = result.Data.Select(d => new { Id = d.Id, Name = d.Name }).ToList();

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
        /// نمایش فرم انتساب پزشک به دپارتمان
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> AssignToDepartment(int? doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش فرم انتساب پزشک {DoctorId} به دپارتمان", doctorId);

                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر یا خالی: {DoctorId}", doctorId);
                    TempData["Error"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["Error"] = doctorResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                var doctor = doctorResult.Data;

                var model = new DoctorAssignmentOperationViewModel
                {
                    DoctorId = doctorId.Value,
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                    DoctorNationalCode = doctor.NationalCode,
                    IsActive = true
                };

                // دریافت لیست دپارتمان‌های فعال
                var departmentsResult = await _doctorDepartmentService.GetAllDepartmentsAsync();
                if (!departmentsResult.Success)
                {
                    _logger.Warning("خطا در دریافت لیست دپارتمان‌ها");
                    TempData["Error"] = departmentsResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                ViewBag.Departments = departmentsResult.Data.Select(d => new { Value = d.Id, Text = d.Text }).ToList();

                _logger.Information("فرم انتساب پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم انتساب پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["Error"] = "خطا در بارگذاری فرم انتساب";
                return RedirectToAction("Index", "Doctor");
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
                _logger.Information("درخواست انتساب پزشک {DoctorId} به دپارتمان {DepartmentId}", 
                    model.DoctorId, model.DepartmentId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("مدل انتساب نامعتبر برای پزشک {DoctorId}", model.DoctorId);
                    TempData["Error"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("AssignToDepartment", new { doctorId = model.DoctorId });
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
                return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتساب پزشک {DoctorId} به دپارتمان", model.DoctorId);
                TempData["Error"] = "خطا در انجام عملیات انتساب";
                return RedirectToAction("AssignToDepartment", new { doctorId = model.DoctorId });
            }
        }

        /// <summary>
        /// نمایش فرم انتقال پزشک بین دپارتمان‌ها
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> TransferDoctor(int? doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش فرم انتقال پزشک {DoctorId}", doctorId);

                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر یا خالی: {DoctorId}", doctorId);
                    TempData["Error"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["Error"] = doctorResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                var doctor = doctorResult.Data;

                // دریافت انتصابات فعلی پزشک
                var assignmentsResult = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId.Value, "", 1, 100);
                if (!assignmentsResult.Success)
                {
                    _logger.Warning("انتسابات پزشک {DoctorId} یافت نشد", doctorId);
                    TempData["Error"] = assignmentsResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                var assignments = assignmentsResult.Data;

                // تعیین دپارتمان فعلی (اولین دپارتمان فعال)
                var currentDepartment = assignments.Items.FirstOrDefault(dd => dd.IsActive);
                
                var model = new DoctorTransferViewModel
                {
                    DoctorId = doctorId.Value,
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                    DoctorNationalCode = doctor.NationalCode,
                    FromDepartmentId = currentDepartment?.DepartmentId ?? 0,
                    FromDepartmentName = currentDepartment?.DepartmentName ?? "بدون دپارتمان",
                    PreserveServiceCategories = true
                };

                // دریافت لیست دپارتمان‌های فعال
                var departmentsResult = await _doctorDepartmentService.GetAllDepartmentsAsync();
                if (!departmentsResult.Success)
                {
                    _logger.Warning("خطا در دریافت لیست دپارتمان‌ها");
                    TempData["Error"] = departmentsResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                ViewBag.Departments = departmentsResult.Data
                    .Where(d => d.Id != currentDepartment?.DepartmentId)
                    .Select(d => new { Value = d.Id, Text = d.Text })
                    .ToList();

                _logger.Information("فرم انتقال پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم انتقال پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["Error"] = "خطا در بارگذاری فرم انتقال";
                return RedirectToAction("Index", "Doctor");
            }
        }

        /// <summary>
        /// پردازش انتقال پزشک بین دپارتمان‌ها
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TransferDoctor(DoctorTransferViewModel model)
        {
            try
            {
                _logger.Information("درخواست انتقال پزشک {DoctorId} از دپارتمان {FromDepartmentId} به دپارتمان {ToDepartmentId}", 
                    model.DoctorId, model.FromDepartmentId, model.ToDepartmentId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("مدل انتقال نامعتبر برای پزشک {DoctorId}", model.DoctorId);
                    TempData["Error"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("TransferDoctor", new { doctorId = model.DoctorId });
                }

                if (model.FromDepartmentId == model.ToDepartmentId)
                {
                    TempData["Error"] = "دپارتمان مبدا و مقصد نمی‌توانند یکسان باشند";
                    return RedirectToAction("TransferDoctor", new { doctorId = model.DoctorId });
                }

                // حذف از دپارتمان فعلی
                var removeResult = await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(model.DoctorId, model.FromDepartmentId);
                if (!removeResult.Success)
                {
                    _logger.Warning("خطا در حذف از دپارتمان فعلی: {Message}", removeResult.Message);
                    TempData["Error"] = $"خطا در حذف از دپارتمان فعلی: {removeResult.Message}";
                    return RedirectToAction("TransferDoctor", new { doctorId = model.DoctorId });
                }

                // انتساب به دپارتمان جدید
                var assignModel = new DoctorDepartmentViewModel
                {
                    DoctorId = model.DoctorId,
                    DepartmentId = model.ToDepartmentId,
                    IsActive = true,
                    StartDate = DateTime.Now,
                    Role = model.Role ?? "پزشک عادی"
                };

                var assignResult = await _doctorDepartmentService.AssignDoctorToDepartmentAsync(assignModel);
                if (!assignResult.Success)
                {
                    _logger.Warning("خطا در انتساب به دپارتمان جدید: {Message}", assignResult.Message);
                    TempData["Error"] = $"خطا در انتساب به دپارتمان جدید: {assignResult.Message}";
                    return RedirectToAction("TransferDoctor", new { doctorId = model.DoctorId });
                }

                _logger.Information("انتقال پزشک {DoctorId} با موفقیت انجام شد", model.DoctorId);
                TempData["Success"] = "انتقال پزشک با موفقیت انجام شد";
                return RedirectToAction("DepartmentAssignments", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتقال پزشک {DoctorId}", model.DoctorId);
                TempData["Error"] = "خطا در انجام عملیات انتقال";
                return RedirectToAction("TransferDoctor", new { doctorId = model.DoctorId });
            }
        }

        #endregion
    }
}
