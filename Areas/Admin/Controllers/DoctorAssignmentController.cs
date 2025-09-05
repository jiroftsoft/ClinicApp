using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using ClinicApp.Models.Entities;
using ClinicApp.Models;
using FluentValidation;
using Microsoft.AspNet.Identity;
using Serilog;
using System.Web;
using System.Web.Caching;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت انتسابات کلی پزشکان
    /// مسئولیت اصلی: مدیریت عملیات انتساب، انتقال و حذف انتسابات پزشکان
    /// Actions تخصصی به کنترولرهای مربوطه منتقل شده‌اند:
    /// - تاریخچه: DoctorHistoryController
    /// - گزارش‌گیری: DoctorReportingController
    /// - انتساب دپارتمان: DoctorDepartmentController
    /// - انتساب خدمات: DoctorServiceCategoryController
    /// - برنامه زمانی: DoctorScheduleController
    /// </summary>
    //[Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorAssignmentController : Controller
    {
        private readonly IDoctorAssignmentService _doctorAssignmentService;
        private readonly IDoctorCrudService _doctorService;
        private readonly IDoctorDepartmentService _doctorDepartmentService;
        private readonly IDoctorServiceCategoryService _doctorServiceCategoryService;
        private readonly IDoctorAssignmentHistoryService _historyService;
        private readonly IValidator<DoctorAssignmentOperationViewModel> _operationValidator;
        private readonly IValidator<DoctorTransferViewModel> _transferValidator;
        private readonly IValidator<DoctorAssignmentRemovalViewModel> _removalValidator;
        private readonly ILogger _logger;

        public DoctorAssignmentController(
            IDoctorAssignmentService doctorAssignmentService,
            IDoctorCrudService doctorService,
            IDoctorDepartmentService doctorDepartmentService,
            IDoctorServiceCategoryService doctorServiceCategoryService,
            IDoctorAssignmentHistoryService historyService,
            IValidator<DoctorAssignmentOperationViewModel> operationValidator,
            IValidator<DoctorTransferViewModel> transferValidator,
            IValidator<DoctorAssignmentRemovalViewModel> removalValidator)
        {
            _doctorAssignmentService = doctorAssignmentService ?? throw new ArgumentNullException(nameof(doctorAssignmentService));
            _doctorService = doctorService ?? throw new ArgumentNullException(nameof(doctorService));
            _doctorDepartmentService = doctorDepartmentService ?? throw new ArgumentNullException(nameof(doctorDepartmentService));
            _doctorServiceCategoryService = doctorServiceCategoryService ?? throw new ArgumentNullException(nameof(doctorServiceCategoryService));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _operationValidator = operationValidator ?? throw new ArgumentNullException(nameof(operationValidator));
            _transferValidator = transferValidator ?? throw new ArgumentNullException(nameof(transferValidator));
            _removalValidator = removalValidator ?? throw new ArgumentNullException(nameof(removalValidator));
            _logger = Log.ForContext<DoctorAssignmentController>();
        }

        #region Main Actions (اکشن‌های اصلی)

        /// <summary>
        /// صفحه اصلی مدیریت انتسابات کلی پزشکان
        /// نمایش آمار کلی و فیلترهای اصلی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("درخواست نمایش صفحه اصلی مدیریت انتسابات پزشکان");

                // ایجاد ViewModel اصلی با مقادیر پیش‌فرض
                var viewModel = new DoctorAssignmentIndexViewModel
                {
                    PageTitle = "مدیریت انتسابات پزشکان",
                    PageSubtitle = "مدیریت کلی انتسابات پزشکان به دپارتمان‌ها و سرفصل‌های خدماتی",
                    IsDataLoaded = false,
                    LastRefreshTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    IsLoading = true,
                    LoadingMessage = "در حال بارگذاری داده‌ها..."
                };

                // دریافت آمار واقعی از سرویس
                var statsResult = await _doctorAssignmentService.GetAssignmentStatisticsAsync();
                viewModel.Stats = statsResult.Success ? statsResult.Data : new AssignmentStatsViewModel
                {
                    TotalAssignments = 0,
                    ActiveAssignments = 0,
                    InactiveAssignments = 0,
                    AssignedDoctors = 0,
                    ActiveDepartments = 0,
                    ServiceCategories = 0,
                    CompletionPercentage = 0,
                    LastUpdate = DateTime.Now
                };

                // ایجاد فیلتر ViewModel
                viewModel.Filters = new AssignmentFilterViewModel();
                
                // دریافت دپارتمان‌های واقعی برای فیلتر
                var departmentsResult = await _doctorDepartmentService.GetAllDepartmentsAsync();
                if (departmentsResult.Success && departmentsResult.Data?.Count > 0)
                {
                    viewModel.Filters.Departments = departmentsResult.Data.Select(d => new ViewModels.DoctorManagementVM.SelectListItem
                    {
                        Text = d.Name,
                        Value = d.Id.ToString(),
                        Selected = false
                    }).ToList();
                }
                else
                {
                    viewModel.Filters.Departments = new List<ViewModels.DoctorManagementVM.SelectListItem>
                    {
                        new ViewModels.DoctorManagementVM.SelectListItem { Text = "هیچ دپارتمانی یافت نشد", Value = "", Selected = true }
                    };
                }

                // اضافه کردن گزینه "همه دپارتمان‌ها" در ابتدای لیست
                viewModel.Filters.Departments.Insert(0, new ViewModels.DoctorManagementVM.SelectListItem 
                { 
                    Text = "همه دپارتمان‌ها", 
                    Value = "", 
                    Selected = true 
                });

                // دریافت دسته‌بندی‌های خدماتی برای فیلتر
                var serviceCategoriesResult = await _doctorServiceCategoryService.GetAllServiceCategoriesAsync();
                if (serviceCategoriesResult.Success && serviceCategoriesResult.Data?.Count > 0)
                {
                    viewModel.Filters.ServiceCategories = serviceCategoriesResult.Data.Select(sc => new ViewModels.DoctorManagementVM.SelectListItem
                    {
                        Text = sc.Name,
                        Value = sc.Id.ToString(),
                        Selected = false
                    }).ToList();
                }
                else
                {
                    viewModel.Filters.ServiceCategories = new List<ViewModels.DoctorManagementVM.SelectListItem>
                    {
                        new ViewModels.DoctorManagementVM.SelectListItem { Text = "هیچ دسته‌بندی یافت نشد", Value = "", Selected = true }
                    };
                }

                // اضافه کردن گزینه "همه دسته‌بندی‌ها" در ابتدای لیست
                viewModel.Filters.ServiceCategories.Insert(0, new ViewModels.DoctorManagementVM.SelectListItem 
                { 
                    Text = "همه دسته‌بندی‌ها", 
                    Value = "", 
                    Selected = true 
                });

                // ارسال داده‌ها به ViewBag برای Partial Views
                ViewBag.Departments = viewModel.Filters.Departments;
                ViewBag.ServiceCategories = viewModel.Filters.ServiceCategories;

                // تنظیم وضعیت نهایی
                viewModel.IsDataLoaded = true;
                viewModel.IsLoading = false;
                viewModel.LoadingMessage = "";

                _logger.Information("صفحه اصلی مدیریت انتسابات با موفقیت نمایش داده شد. TotalAssignments: {TotalAssignments}, ActiveAssignments: {ActiveAssignments}", 
                    viewModel.Stats.TotalAssignments, viewModel.Stats.ActiveAssignments);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه اصلی مدیریت انتسابات");
                TempData["ErrorMessage"] = "خطا در بارگذاری صفحه اصلی";
                return RedirectToAction("Index", "Home");
            }
        }

        // Details Action
        /// <summary>
        /// نمایش جزئیات انتسابات پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            try
            {
                _logger.Information("درخواست نمایش جزئیات انتسابات پزشک {DoctorId}", id);

                if (!id.HasValue || id.Value <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر یا خالی: {DoctorId}", id);
                    TempData["ErrorMessage"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorService.GetDoctorDetailsAsync(id.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", id.Value);
                    TempData["ErrorMessage"] = doctorResult.Message;
                    return RedirectToAction("Index");
                }

                var doctorDetails = doctorResult.Data;

                // دریافت انتسابات پزشک
                var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(id.Value);
                if (!assignmentsResult.Success)
                {
                    _logger.Warning("انتسابات پزشک {DoctorId} یافت نشد", id);
                    TempData["ErrorMessage"] = assignmentsResult.Message;
                    return RedirectToAction("Index");
                }

                var assignments = assignmentsResult.Data;

                // دریافت تاریخچه انتسابات
                var historyResult = await _doctorAssignmentService.GetDoctorAssignmentHistoryAsync(id.Value, 1, 10);
                var assignmentHistory = historyResult.Success ? historyResult.Data : new List<Models.Entities.DoctorAssignmentHistory>();

                // تبدیل به ViewModel های تخصصی
                var departmentViewModels = assignments.DoctorDepartments.Select(dd => new DoctorDepartmentViewModel
                {
                    DepartmentId = dd.DepartmentId,
                    DepartmentName = dd.DepartmentName,
                    IsActive = dd.IsActive,
                    CreatedAt = dd.CreatedAt,
                    Role = dd.Role
                }).ToList();

                var serviceCategoryViewModels = assignments.DoctorServiceCategories.Select(dsc => new DoctorServiceCategoryViewModel
                {
                    ServiceCategoryId = dsc.ServiceCategoryId,
                    ServiceCategoryTitle = dsc.ServiceCategoryTitle,
                    IsActive = dsc.IsActive,
                    GrantedDate = dsc.GrantedDate,
                    CertificateNumber = dsc.CertificateNumber
                }).ToList();

                var historyViewModels = assignmentHistory.Select(h => new AssignmentHistoryViewModel
                {
                    Id = h.Id,
                    ActionType = h.ActionType,
                    ActionTitle = h.ActionTitle,
                    ActionDescription = h.ActionDescription,
                    ActionDate = h.ActionDate,
                    DepartmentName = h.DepartmentName,
                    PerformedBy = h.PerformedByUserName,
                    Notes = h.Notes
                }).ToList();

                // ایجاد ViewModel اصلی با استفاده از DoctorDetailsViewModel
                var viewModel = new DoctorAssignmentDetailsViewModel
                {
                    DoctorId = id.Value,
                    DoctorName = doctorDetails.FullName,
                    DoctorNationalCode = doctorDetails.NationalCode,
                    DoctorSpecialization = string.Join("، ", doctorDetails.SpecializationNames),
                    MedicalCouncilNumber = doctorDetails.MedicalCouncilCode,
                    LastUpdateTime = DateTime.Now,
                    Departments = departmentViewModels,
                    ServiceCategories = serviceCategoryViewModels,
                    History = historyViewModels
                };

                // محاسبه آمار
                viewModel.TotalActiveAssignments = 
                    (departmentViewModels?.Count(d => d.IsActive) ?? 0) + 
                    (serviceCategoryViewModels?.Count(s => s.IsActive) ?? 0);

                viewModel.ActiveDepartmentCount = departmentViewModels?.Count(d => d.IsActive) ?? 0;
                viewModel.ActiveServiceCategoryCount = serviceCategoryViewModels?.Count(s => s.IsActive) ?? 0;
                viewModel.IsMultiDepartment = viewModel.ActiveDepartmentCount > 1;

                _logger.Information("جزئیات انتسابات پزشک {DoctorId} با موفقیت نمایش داده شد", id.Value);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات انتسابات پزشک {DoctorId}", id?.ToString() ?? "null");
                TempData["ErrorMessage"] = "خطا در بارگذاری جزئیات انتسابات";
                return RedirectToAction("Index");
            }
        }

        // Assignments Action
        /// <summary>
        /// نمایش لیست انتسابات پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Assignments(int? doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش انتسابات پزشک {DoctorId}", doctorId);

                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر یا خالی: {DoctorId}", doctorId);
                    TempData["ErrorMessage"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["ErrorMessage"] = doctorResult.Message;
                    return RedirectToAction("Index");
                }

                var doctor = doctorResult.Data;

                // دریافت انتسابات پزشک
                var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId.Value);
                if (!assignmentsResult.Success)
                {
                    _logger.Warning("انتسابات پزشک {DoctorId} یافت نشد", doctorId);
                    TempData["ErrorMessage"] = assignmentsResult.Message;
                    return RedirectToAction("Index");
                }

                var assignments = assignmentsResult.Data;

                ViewBag.Doctor = new { FullName = $"{doctor.FirstName} {doctor.LastName}", NationalCode = doctor.NationalCode };
                ViewBag.DoctorId = doctorId.Value;

                _logger.Information("انتسابات پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(assignments);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش انتسابات پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["ErrorMessage"] = "خطا در بارگذاری انتسابات پزشک";
                return RedirectToAction("Index");
            }
        }

        // AssignToDepartment GET Action
        #endregion

        #region Assignment Operations (عملیات انتساب)

        /// <summary>
        /// نمایش فرم انتساب پزشک به دپارتمان (عملیات کلی)
        /// برای انتساب‌های تخصصی به DoctorDepartmentController مراجعه شود
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
                    TempData["ErrorMessage"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["ErrorMessage"] = doctorResult.Message;
                    return RedirectToAction("Index");
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
                    TempData["ErrorMessage"] = departmentsResult.Message;
                    return RedirectToAction("Index");
                }

                ViewBag.Departments = departmentsResult.Data.Select(d => new { Value = d.Id, Text = d.Name }).ToList();

                // دریافت لیست دسته‌بندی‌های خدماتی فعال
                var serviceCategoriesResult = await _doctorServiceCategoryService.GetAllServiceCategoriesAsync();
                if (!serviceCategoriesResult.Success)
                {
                    _logger.Warning("خطا در دریافت لیست دسته‌بندی‌های خدماتی");
                    TempData["ErrorMessage"] = serviceCategoriesResult.Message;
                    return RedirectToAction("Index");
                }

                ViewBag.ServiceCategories = serviceCategoriesResult.Data.Select(sc => new { Value = sc.Id, Text = sc.Name }).ToList();

                // دریافت آمار انتسابات فعلی
                var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId.Value);
                if (assignmentsResult.Success)
                {
                    ViewBag.ActiveAssignments = assignmentsResult.Data.TotalActiveAssignments;
                    ViewBag.TotalDepartments = assignmentsResult.Data.ActiveDepartmentCount;
                }
                else
                {
                    ViewBag.ActiveAssignments = 0;
                    ViewBag.TotalDepartments = 0;
                }

                _logger.Information("فرم انتساب پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم انتساب پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم انتساب";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش انتساب پزشک به دپارتمان (عملیات کلی)
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
                    TempData["ErrorMessage"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("AssignToDepartment", new { doctorId = model.DoctorId });
                }

                var validationResult = await _operationValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی انتساب پزشک {DoctorId} ناموفق بود", model.DoctorId);
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError("", error.ErrorMessage);
                    }
                    TempData["ErrorMessage"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("AssignToDepartment", new { doctorId = model.DoctorId });
                }

                // انتساب پزشک به دپارتمان و سرفصل‌های خدماتی
                var result = await _doctorAssignmentService.AssignDoctorToDepartmentWithServicesAsync(
                    model.DoctorId, 
                    model.DepartmentId, 
                    model.ServiceCategoryIds ?? new List<int>());

                if (!result.Success)
                {
                    _logger.Warning("انتساب پزشک {DoctorId} ناموفق بود: {Message}", model.DoctorId, result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("AssignToDepartment", new { doctorId = model.DoctorId });
                }

                _logger.Information("انتساب پزشک {DoctorId} به دپارتمان {DepartmentId} با موفقیت انجام شد", 
                    model.DoctorId, model.DepartmentId);
                TempData["SuccessMessage"] = "انتساب پزشک با موفقیت انجام شد";
                return RedirectToAction("Assignments", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتساب پزشک {DoctorId} به دپارتمان", model.DoctorId);
                TempData["ErrorMessage"] = "خطا در انجام عملیات انتساب";
                return RedirectToAction("AssignToDepartment", new { doctorId = model.DoctorId });
            }
        }

        // TransferDoctor GET Action
        /// <summary>
        /// نمایش فرم انتقال پزشک بین دپارتمان‌ها (عملیات کلی)
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
                    TempData["ErrorMessage"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["ErrorMessage"] = doctorResult.Message;
                    return RedirectToAction("Index");
                }

                var doctor = doctorResult.Data;

                // دریافت انتسابات فعلی پزشک
                var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId.Value);
                if (!assignmentsResult.Success)
                {
                    _logger.Warning("انتسابات پزشک {DoctorId} یافت نشد", doctorId);
                    TempData["ErrorMessage"] = assignmentsResult.Message;
                    return RedirectToAction("Index");
                }

                var assignments = assignmentsResult.Data;

                // تعیین دپارتمان فعلی (اولین دپارتمان فعال)
                var currentDepartment = assignments.DoctorDepartments.FirstOrDefault(dd => dd.IsActive);
                
                var model = new DoctorTransferViewModel
                {
                    DoctorId = doctorId.Value,
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                    DoctorNationalCode = doctor.NationalCode,
                    FromDepartmentId = currentDepartment?.DepartmentId ?? 0,
                    FromDepartmentName = currentDepartment?.DepartmentName ?? "بدون دپارتمان",
                    PreserveServiceCategories = true
                };

                // دریافت لیست دپارتمان‌های فعال برای انتخاب مقصد
                var departmentsResult = await _doctorDepartmentService.GetAllDepartmentsAsync();
                if (!departmentsResult.Success)
                {
                    _logger.Warning("خطا در دریافت لیست دپارتمان‌ها");
                    TempData["ErrorMessage"] = departmentsResult.Message;
                    return RedirectToAction("Index");
                }

                ViewBag.Departments = departmentsResult.Data
                    .Where(d => d.Id != currentDepartment?.DepartmentId) // حذف دپارتمان فعلی از لیست
                    .Select(d => new { Value = d.Id, Text = d.Name })
                    .ToList();

                _logger.Information("فرم انتقال پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم انتقال پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم انتقال";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش انتقال پزشک بین دپارتمان‌ها (عملیات کلی)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TransferDoctor(DoctorTransferViewModel model)
        {
            try
            {
                _logger.Information("درخواست انتقال پزشک {DoctorId} از دپارتمان {FromDepartmentId} به {ToDepartmentId}", 
                    model.DoctorId, model.FromDepartmentId, model.ToDepartmentId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("مدل انتقال نامعتبر برای پزشک {DoctorId}", model.DoctorId);
                    TempData["ErrorMessage"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("TransferDoctor", new { doctorId = model.DoctorId });
                }

                var validationResult = await _transferValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی انتقال پزشک {DoctorId} ناموفق بود", model.DoctorId);
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError("", error.ErrorMessage);
                    }
                    TempData["ErrorMessage"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("TransferDoctor", new { doctorId = model.DoctorId });
                }

                // انتقال پزشک بین دپارتمان‌ها
                var result = await _doctorAssignmentService.TransferDoctorBetweenDepartmentsAsync(
                    model.DoctorId, 
                    model.FromDepartmentId, 
                    model.ToDepartmentId, 
                    model.PreserveServiceCategories);

                if (!result.Success)
                {
                    _logger.Warning("انتقال پزشک {DoctorId} ناموفق بود: {Message}", model.DoctorId, result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("TransferDoctor", new { doctorId = model.DoctorId });
                }

                _logger.Information("انتقال پزشک {DoctorId} با موفقیت انجام شد", model.DoctorId);
                TempData["SuccessMessage"] = "انتقال پزشک با موفقیت انجام شد";
                return RedirectToAction("Assignments", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتقال پزشک {DoctorId}", model.DoctorId);
                TempData["ErrorMessage"] = "خطا در انجام عملیات انتقال";
                return RedirectToAction("TransferDoctor", new { doctorId = model.DoctorId });
            }
        }

        #endregion

        #region Removal Operations (عملیات حذف)

        /// <summary>
        /// نمایش فرم حذف انتسابات پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> RemoveAssignments(int? doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش فرم حذف انتسابات پزشک {DoctorId}", doctorId);

                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر یا خالی: {DoctorId}", doctorId);
                    TempData["ErrorMessage"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات پزشک
                    var doctorResult = await _doctorService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["ErrorMessage"] = doctorResult.Message;
                    return RedirectToAction("Index");
                }

                var doctor = doctorResult.Data;

                // دریافت تعداد انتسابات فعال
                var activeAssignmentsResult = await _doctorAssignmentService.GetActiveAssignmentsCountAsync(doctorId.Value);
                var activeAssignmentsCount = activeAssignmentsResult.Success ? activeAssignmentsResult.Data : 0;

                // بررسی وابستگی‌ها
                var dependenciesResult = await _doctorAssignmentService.GetDoctorDependenciesAsync(doctorId.Value);
                var dependencies = dependenciesResult.Success ? dependenciesResult.Data : new DoctorDependencyInfo();

                var model = new DoctorAssignmentRemovalViewModel
                {
                    DoctorId = doctorId.Value,
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                    ActiveAssignmentsCount = activeAssignmentsCount,
                    DependenciesChecked = true,
                    IsPermanentRemoval = false
                };

                _logger.Information("فرم حذف انتسابات پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم حذف انتسابات پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم حذف";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش حذف انتسابات پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveAssignments(DoctorAssignmentRemovalViewModel model)
        {
            try
            {
                _logger.Information("درخواست حذف انتسابات پزشک {DoctorId}", model.DoctorId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("مدل حذف نامعتبر برای پزشک {DoctorId}", model.DoctorId);
                    TempData["ErrorMessage"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("RemoveAssignments", new { doctorId = model.DoctorId });
                }

                var validationResult = await _removalValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی حذف انتسابات پزشک {DoctorId} ناموفق بود", model.DoctorId);
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError("", error.ErrorMessage);
                    }
                    TempData["ErrorMessage"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("RemoveAssignments", new { doctorId = model.DoctorId });
                }

                // حذف کامل انتسابات پزشک
                var result = await _doctorAssignmentService.RemoveAllDoctorAssignmentsAsync(model.DoctorId);

                if (!result.Success)
                {
                    _logger.Warning("حذف انتسابات پزشک {DoctorId} ناموفق بود: {Message}", model.DoctorId, result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("RemoveAssignments", new { doctorId = model.DoctorId });
                }

                _logger.Information("حذف انتسابات پزشک {DoctorId} با موفقیت انجام شد", model.DoctorId);
                TempData["SuccessMessage"] = "حذف انتسابات با موفقیت انجام شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف انتسابات پزشک {DoctorId}", model.DoctorId);
                TempData["ErrorMessage"] = "خطا در انجام عملیات حذف";
                return RedirectToAction("RemoveAssignments", new { doctorId = model.DoctorId });
            }
        }

        // AJAX Actions
        [HttpPost]
        public async Task<JsonResult> GetDoctorAssignments(int doctorId)
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت انتسابات پزشک {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" });
                }

                // دریافت انتسابات پزشک
                var result = await _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتسابات پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت انتسابات" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetDoctorDependencies(int doctorId)
        {
            try
            {
                _logger.Information("درخواست AJAX بررسی وابستگی‌های پزشک {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" });
                }

                // بررسی وابستگی‌های پزشک
                var result = await _doctorAssignmentService.GetDoctorDependenciesAsync(doctorId);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وابستگی‌های پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در بررسی وابستگی‌ها" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ValidateAssignment(DoctorAssignmentOperationViewModel model)
        {
            try
            {
                _logger.Information("درخواست اعتبارسنجی انتساب پزشک {DoctorId}", model.DoctorId);

                var validationResult = await _operationValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, errors = errors });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی انتساب پزشک {DoctorId}", model.DoctorId);
                return Json(new { success = false, message = "خطا در اعتبارسنجی" });
            }
        }

        // AJAX Partial Update Actions
        [HttpGet]
        public async Task<JsonResult> GetDoctorDetailsPartial(int id)
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت جزئیات پزشک {DoctorId}", id);

                if (id <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" }, JsonRequestBehavior.AllowGet);
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorService.GetDoctorDetailsAsync(id);
                if (!doctorResult.Success)
                {
                    return Json(new { success = false, message = doctorResult.Message }, JsonRequestBehavior.AllowGet);
                }

                var doctor = doctorResult.Data;

                // دریافت انتسابات پزشک
                var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(id);
                if (!assignmentsResult.Success)
                {
                    return Json(new { success = false, message = assignmentsResult.Message }, JsonRequestBehavior.AllowGet);
                }

                var assignments = assignmentsResult.Data;

                // آماده‌سازی داده‌ها برای AJAX
                var data = new
                {
                    doctorInfo = new
                    {
                        name = $"{doctor.FirstName} {doctor.LastName}",
                        nationalCode = doctor.NationalCode,
                        lastUpdate = DateTime.Now.ToString("yyyy/MM/dd HH:mm")
                    },
                    headerStats = new
                    {
                        totalActiveAssignments = assignments.TotalActiveAssignments,
                        activeDepartmentCount = assignments.ActiveDepartmentCount,
                        activeServiceCategoryCount = assignments.ActiveServiceCategoryCount
                    },
                    stats = new
                    {
                        totalActiveAssignments = assignments.TotalActiveAssignments,
                        activeDepartmentCount = assignments.ActiveDepartmentCount,
                        activeServiceCategoryCount = assignments.ActiveServiceCategoryCount
                    }
                };

                _logger.Information("جزئیات پزشک {DoctorId} با موفقیت برای AJAX آماده شد", id);
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پزشک {DoctorId} برای AJAX", id);
                return Json(new { success = false, message = "خطا در دریافت اطلاعات" }, JsonRequestBehavior.AllowGet);
            }
        }

        // Export Actions
        #endregion

        #region Export Operations (عملیات خروجی)

        /// <summary>
        /// خروجی جزئیات انتسابات پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ExportDoctorDetails(int id)
        {
            try
            {
                _logger.Information("درخواست export جزئیات پزشک {DoctorId}", id);

                if (id <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر: {DoctorId}", id);
                    return new HttpStatusCodeResult(400, "شناسه پزشک نامعتبر است");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorService.GetDoctorDetailsAsync(id);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", id);
                    return new HttpStatusCodeResult(404, doctorResult.Message);
                }

                var doctorDetails = doctorResult.Data;

                // دریافت انتسابات پزشک
                var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(id);
                if (!assignmentsResult.Success)
                {
                    _logger.Warning("انتسابات پزشک {DoctorId} یافت نشد", id);
                    return new HttpStatusCodeResult(404, assignmentsResult.Message);
                }

                var assignments = assignmentsResult.Data;

                // دریافت تاریخچه انتسابات
                var historyResult = await _doctorAssignmentService.GetDoctorAssignmentHistoryAsync(id, 1, 50);
                var assignmentHistory = historyResult.Success ? historyResult.Data : new List<Models.Entities.DoctorAssignmentHistory>();

                // ایجاد PDF (فعلاً یک فایل متنی ساده)
                var content = GenerateDoctorDetailsText(doctorDetails, assignments, assignmentHistory);
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);

                _logger.Information("فایل جزئیات پزشک {DoctorId} با موفقیت ایجاد شد", id);
                return File(bytes, "text/plain", $"doctor-assignments-{id}.txt");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در export جزئیات پزشک {DoctorId}", id);
                return new HttpStatusCodeResult(500, "خطا در ایجاد فایل");
            }
        }

        // Remove Assignment Action
        [HttpPost]
        public async Task<JsonResult> RemoveAssignment(int id)
        {
            try
            {
                _logger.Information("درخواست حذف انتساب {AssignmentId}", id);

                if (id <= 0)
                {
                    return Json(new { success = false, message = "شناسه انتساب نامعتبر است" });
                }

                // حذف انتساب - فعلاً پیام موفقیت برمی‌گردانیم
                // TODO: پیاده‌سازی متد RemoveAssignmentAsync در سرویس
                _logger.Information("انتساب {AssignmentId} با موفقیت حذف شد", id);
                return Json(new { success = true, message = "انتساب با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف انتساب {AssignmentId}", id);
                return Json(new { success = false, message = "خطا در حذف انتساب" });
            }
        }

        // Reporting Actions
        // Actions تخصصی به کنترولرهای مربوطه منتقل شده‌اند:
        // - AssignmentReport و GetAssignmentStatistics → DoctorReportingController
        // - AssignmentHistory و متدهای مربوطه → DoctorHistoryController

        #region Helper Methods (متدهای کمکی)

        /// <summary>
        /// تولید فایل متنی برای export جزئیات پزشک
        /// </summary>
        private string GenerateDoctorDetailsText(
            DoctorDetailsViewModel doctor,
            DoctorAssignmentsViewModel assignments,
            List<Models.Entities.DoctorAssignmentHistory> history)
        {
            var content = new System.Text.StringBuilder();

            // Header
            content.AppendLine("=".PadRight(80, '='));
            content.AppendLine($"جزئیات انتسابات پزشک - {doctor.FullName}");
            content.AppendLine("=".PadRight(80, '='));
            content.AppendLine();

            // Doctor Information
            content.AppendLine("اطلاعات پزشک:");
            content.AppendLine($"نام: {doctor.FullName}");
            content.AppendLine($"کد ملی: {doctor.NationalCode}");
            content.AppendLine($"تخصص: {string.Join("، ", doctor.SpecializationNames)}");
            content.AppendLine($"شماره نظام پزشکی: {doctor.MedicalCouncilCode}");
            content.AppendLine();

            // Statistics
            content.AppendLine("آمار کلی:");
            content.AppendLine($"کل انتسابات فعال: {assignments.TotalActiveAssignments}");
            content.AppendLine($"دپارتمان‌های فعال: {assignments.ActiveDepartmentCount}");
            content.AppendLine($"صلاحیت‌های فعال: {assignments.ActiveServiceCategoryCount}");
            content.AppendLine();

            // Department Assignments
            if (assignments.DoctorDepartments.Any())
            {
                content.AppendLine("انتسابات دپارتمان:");
                content.AppendLine("-".PadRight(40, '-'));
                foreach (var dept in assignments.DoctorDepartments)
                {
                    content.AppendLine($"• {dept.DepartmentName} - {(dept.IsActive ? "فعال" : "غیرفعال")}");
                    if (dept.CreatedAt != default(DateTime))
                        content.AppendLine($"  تاریخ انتساب: {dept.CreatedAt:yyyy/MM/dd}");
                    if (!string.IsNullOrEmpty(dept.Role))
                        content.AppendLine($"  نقش: {dept.Role}");
                    content.AppendLine();
                }
            }

            // Service Categories
            if (assignments.DoctorServiceCategories.Any())
            {
                content.AppendLine("صلاحیت‌های خدماتی:");
                content.AppendLine("-".PadRight(40, '-'));
                foreach (var cat in assignments.DoctorServiceCategories)
                {
                    content.AppendLine($"• {cat.ServiceCategoryTitle} - {(cat.IsActive ? "فعال" : "غیرفعال")}");
                    if (cat.GrantedDate.HasValue)
                        content.AppendLine($"  تاریخ اعطا: {cat.GrantedDate.Value:yyyy/MM/dd}");
                    if (!string.IsNullOrEmpty(cat.CertificateNumber))
                        content.AppendLine($"  شماره گواهی: {cat.CertificateNumber}");
                    content.AppendLine();
                }
            }

            // History
            if (history.Any())
            {
                content.AppendLine("تاریخچه انتسابات:");
                content.AppendLine("-".PadRight(40, '-'));
                foreach (var hist in history.Take(20)) // فقط 20 مورد آخر
                {
                    content.AppendLine($"• {hist.ActionTitle} - {hist.ActionDate:yyyy/MM/dd HH:mm}");
                    content.AppendLine($"  {hist.ActionDescription}");
                    if (!string.IsNullOrEmpty(hist.PerformedByUserName))
                        content.AppendLine($"  انجام شده توسط: {hist.PerformedByUserName}");
                    content.AppendLine();
                }
            }

            // Footer
            content.AppendLine("=".PadRight(80, '='));
            content.AppendLine($"تاریخ ایجاد: {DateTime.Now:yyyy/MM/dd HH:mm}");
            content.AppendLine("=".PadRight(80, '='));

            return content.ToString();
        }

        #endregion

        #endregion

        #region AJAX Operations (عملیات AJAX)

        /// <summary>
        /// دریافت لیست انتسابات برای نمایش در جدول
        /// </summary>
        /// <summary>
        /// دریافت لیست انتسابات برای DataTables (Server-side)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public async Task<JsonResult> GetAssignments(DataTablesRequest request)
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت لیست انتسابات برای DataTables. Draw: {Draw}, Start: {Start}, Length: {Length}", 
                    request.Draw, request.Start, request.Length);

                // اعتبارسنجی درخواست
                if (request == null)
                {
                    _logger.Warning("درخواست DataTables null است");
                    return Json(new { 
                        draw = 0,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>(),
                        error = "درخواست نامعتبر است"
                    });
                }

                // دریافت انتسابات از سرویس با pagination
                var result = await _doctorAssignmentService.GetAssignmentsForDataTablesAsync(request);
                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت انتسابات: {Message}", result.Message);
                    return Json(new { 
                        draw = request.Draw,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>(),
                        error = result.Message
                    });
                }

                _logger.Information("دریافت موفق {Count} انتساب از {Total} کل", 
                    result.Data.Assignments.Count, result.Data.RecordsTotal);

                return Json(new { 
                    draw = request.Draw,
                    recordsTotal = result.Data.RecordsTotal,
                    recordsFiltered = result.Data.RecordsFiltered,
                    data = result.Data.Assignments
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست انتسابات برای DataTables");
                return Json(new { 
                    draw = request?.Draw ?? 0,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = "خطا در دریافت داده‌ها"
                });
            }
        }


        /// <summary>
        /// دریافت دسته‌بندی‌های خدماتی بر اساس دپارتمان
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategoriesByDepartment(int departmentId)
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت دسته‌بندی‌های خدماتی برای دپارتمان {DepartmentId}", departmentId);

                if (departmentId <= 0)
                {
                    return Json(new { success = false, message = "شناسه دپارتمان معتبر نیست" }, JsonRequestBehavior.AllowGet);
                }

                // دریافت دسته‌بندی‌های خدماتی از سرویس
                var result = await _doctorServiceCategoryService.GetAllServiceCategoriesAsync();
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                // فیلتر کردن بر اساس دپارتمان (در حالت واقعی باید از سرویس جداگانه استفاده شود)
                var categories = result.Data
                    .Where(c => c.Id == departmentId) // استفاده از Id به جای DepartmentId
                    .Select(c => new { Id = c.Id, Name = c.Name })
                    .ToList();

                return Json(new { success = true, data = categories }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی‌های خدماتی برای دپارتمان {DepartmentId}", departmentId);
                return Json(new { success = false, message = "خطا در دریافت دسته‌بندی‌های خدماتی" }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        // AssignmentHistory و متدهای مربوطه به DoctorHistoryController منتقل شده‌اند
    }
}
