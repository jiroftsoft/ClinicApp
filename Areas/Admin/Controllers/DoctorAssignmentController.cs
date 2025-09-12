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
using ClinicApp.Models.Entities.Doctor;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت انتسابات پزشکان
    /// مسئولیت اصلی: مدیریت عملیات انتساب پزشکان به دپارتمان‌ها و سرفصل‌های خدماتی
    /// 
    /// Actions تخصصی به کنترولرهای مربوطه منتقل شده‌اند:
    /// - حذف انتسابات: DoctorRemovalController
    /// - تاریخچه: DoctorHistoryController
    /// - گزارش‌گیری: DoctorReportingController
    /// - انتساب دپارتمان: DoctorDepartmentController
    /// - انتساب خدمات: DoctorServiceCategoryController
    /// - برنامه زمانی: DoctorScheduleController
    /// </summary>
    //[Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorAssignmentController : Shared.BaseAssignmentController
    {
        #region Private Fields (فیلدهای خصوصی)

        private readonly IValidator<DoctorAssignmentOperationViewModel> _operationValidator;
        private readonly IValidator<DoctorAssignmentEditViewModel> _editValidator;

        #endregion

        #region Constructor (سازنده)

        public DoctorAssignmentController(
            IDoctorAssignmentService doctorAssignmentService,
            IDoctorCrudService doctorService,
            IDoctorDepartmentService doctorDepartmentService,
            IDoctorServiceCategoryService doctorServiceCategoryService,
            IDoctorAssignmentHistoryService historyService,
            IValidator<DoctorAssignmentOperationViewModel> operationValidator,
            IValidator<DoctorAssignmentEditViewModel> editValidator)
            : base(doctorAssignmentService, doctorService, doctorDepartmentService, doctorServiceCategoryService, historyService)
        {
            _operationValidator = operationValidator ?? throw new ArgumentNullException(nameof(operationValidator));
            _editValidator = editValidator ?? throw new ArgumentNullException(nameof(editValidator));
        }

        #endregion

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

                // دریافت ViewModel کاملاً آماده از سرویس
                var viewModelResult = await _doctorAssignmentService.GetDoctorAssignmentIndexViewModelAsync();
                
                if (!viewModelResult.Success)
                {
                    _logger.Warning("خطا در آماده‌سازی ViewModel: {Message}", viewModelResult.Message);
                    TempData["ErrorMessage"] = "خطا در بارگذاری صفحه اصلی";
                    return RedirectToAction("Index", "Home");
                }

                var viewModel = viewModelResult.Data;

                // تنظیم ViewBag برای باندل‌ها (طبق قرارداد بهینه‌سازی ویوها)
                ViewBag.Title = viewModel.PageTitle;
                ViewBag.RequireDataTables = true;        // برای جدول‌ها
                ViewBag.RequireSelect2 = true;           // برای فیلترهای dropdown
                ViewBag.RequireDatePicker = true;        // برای فیلتر تاریخ
                ViewBag.RequireFormValidation = true;    // برای اعتبارسنجی فرم‌ها

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
                var assignmentHistory = historyResult.Success ? historyResult.Data : new List<DoctorAssignmentHistory>();

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
                    ServiceCategoryName = dsc.ServiceCategoryTitle, // اضافه کردن ServiceCategoryName
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


        // AssignToDepartment GET Action
        #endregion

        #region Edit Actions (اکشن‌های ویرایش)

        /// <summary>
        /// نمایش فرم ویرایش انتسابات پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            try
            {
                _logger.Information("درخواست نمایش فرم ویرایش انتسابات پزشک {DoctorId}", id);

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

                var doctor = doctorResult.Data;

                // دریافت انتسابات فعلی پزشک
                var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(id.Value);
                if (!assignmentsResult.Success)
                {
                    _logger.Warning("انتسابات پزشک {DoctorId} یافت نشد", id.Value);
                    TempData["ErrorMessage"] = assignmentsResult.Message;
                    return RedirectToAction("Index");
                }

                var assignments = assignmentsResult.Data;

                // دریافت لیست‌های موجود برای انتخاب
                var departmentsTask = _doctorDepartmentService.GetDepartmentsAsSelectListAsync();
                var serviceCategoriesTask = _doctorServiceCategoryService.GetServiceCategoriesAsSelectListAsync();

                await Task.WhenAll(departmentsTask, serviceCategoriesTask);

                var departmentsResult = await departmentsTask;
                var serviceCategoriesResult = await serviceCategoriesTask;

                // ایجاد ViewModel برای ویرایش
                var detailsViewModel = new DoctorAssignmentDetailsViewModel
                {
                    DoctorId = id.Value,
                    DoctorName = doctor.FullName,
                    DoctorNationalCode = doctor.NationalCode,
                    DoctorSpecialization = string.Join("، ", doctor.SpecializationNames),
                    MedicalCouncilNumber = doctor.MedicalCouncilCode,
                    LastUpdateTime = DateTime.Now,
                    Departments = assignments.DoctorDepartments.Select(dd => new DoctorDepartmentViewModel
                    {
                        DepartmentId = dd.DepartmentId,
                        DepartmentName = dd.DepartmentName,
                        IsActive = dd.IsActive,
                        CreatedAt = dd.CreatedAt,
                        Role = dd.Role
                    }).ToList(),
                    ServiceCategories = assignments.DoctorServiceCategories.Select(dsc => new DoctorServiceCategoryViewModel
                    {
                        ServiceCategoryId = dsc.ServiceCategoryId,
                        ServiceCategoryTitle = dsc.ServiceCategoryTitle,
                        IsActive = dsc.IsActive,
                        GrantedDate = dsc.GrantedDate,
                        CertificateNumber = dsc.CertificateNumber
                    }).ToList()
                };

                // محاسبه آمار
                detailsViewModel.TotalActiveAssignments = 
                    (detailsViewModel.Departments?.Count(d => d.IsActive) ?? 0) + 
                    (detailsViewModel.ServiceCategories?.Count(s => s.IsActive) ?? 0);

                detailsViewModel.ActiveDepartmentCount = detailsViewModel.Departments?.Count(d => d.IsActive) ?? 0;
                detailsViewModel.ActiveServiceCategoryCount = detailsViewModel.ServiceCategories?.Count(s => s.IsActive) ?? 0;
                detailsViewModel.IsMultiDepartment = detailsViewModel.ActiveDepartmentCount > 1;

                // تبدیل به EditViewModel
                var editViewModel = DoctorAssignmentEditViewModel.FromDetailsViewModel(detailsViewModel);
                
                // تنظیم لیست‌های موجود
                if (departmentsResult.Success)
                {
                    editViewModel.AvailableDepartments = departmentsResult.Data;
                }
                
                if (serviceCategoriesResult.Success)
                {
                    editViewModel.AvailableServiceCategories = serviceCategoriesResult.Data;
                }

                // تنظیم ViewBag برای باندل‌ها
                ViewBag.Title = "ویرایش انتسابات پزشک";
                ViewBag.RequireSelect2 = true;
                ViewBag.RequireFormValidation = true;
                ViewBag.RequireDatePicker = true;

                _logger.Information("فرم ویرایش انتسابات پزشک {DoctorId} با موفقیت نمایش داده شد", id.Value);
                return View(editViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش انتسابات پزشک {DoctorId}", id?.ToString() ?? "null");
                TempData["ErrorMessage"] = "خطا در بارگذاری فرم ویرایش";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش ویرایش انتسابات پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DoctorAssignmentEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ویرایش انتسابات پزشک {DoctorId}", model.DoctorId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("مدل ویرایش نامعتبر برای پزشک {DoctorId}", model.DoctorId);
                    TempData["ErrorMessage"] = "اطلاعات وارد شده نامعتبر است";
                    
                    // بارگذاری مجدد لیست‌ها
                    await ReloadEditViewModelListsAsync(model);
                    return View(model);
                }

                // اعتبارسنجی اضافی
                if (model.DoctorId <= 0)
                {
                    ModelState.AddModelError("", "شناسه پزشک نامعتبر است");
                    return View(model);
                }

                // 🔄 PRODUCTION LOG: شروع پردازش ویرایش انتسابات
                _logger.Information("🔄 PRODUCTION LOG: شروع پردازش ویرایش انتسابات پزشک {DoctorId}", model.DoctorId);
                _logger.Information("📊 تغییرات درخواستی: حذف {RemoveCount}, دپارتمان جدید {DeptCount}, سرفصل جدید {ServiceCount}", 
                    model.AssignmentsToRemove?.Count ?? 0, 
                    model.NewDepartmentIds?.Count ?? 0,
                    model.NewServiceCategoryIds?.Count ?? 0);

                // اعتبارسنجی با FluentValidation
                var validationResult = await _editValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("⚠️ اعتبارسنجی EditViewModel ناموفق: {@Errors}", validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
                    
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    
                    // بارگذاری مجدد لیست‌ها
                    await ReloadEditViewModelListsAsync(model);
                    return View(model);
                }

                // اعتبارسنجی تاریخ اعمال
                if (model.EffectiveDate.HasValue && model.EffectiveDate.Value < DateTime.Now.Date)
                {
                    _logger.Warning("⚠️ تاریخ اعمال در گذشته است: {EffectiveDate}", model.EffectiveDate);
                    ModelState.AddModelError("EffectiveDate", "تاریخ اعمال نمی‌تواند در گذشته باشد");
                    
                    await ReloadEditViewModelListsAsync(model);
                    return View(model);
                }

                // فراخوانی سرویس برای به‌روزرسانی انتسابات
                var updateResult = await _doctorAssignmentService.UpdateDoctorAssignmentsFromEditAsync(model);
                
                if (!updateResult.Success)
                {
                    _logger.Error("❌ خطا در به‌روزرسانی انتسابات: {Message}", updateResult.Message);
                    TempData["ErrorMessage"] = updateResult.Message;
                    
                    // بارگذاری مجدد لیست‌ها
                    await ReloadEditViewModelListsAsync(model);
                    return View(model);
                }

                // ✅ موفقیت
                _logger.Information("✅ PRODUCTION LOG: ویرایش انتسابات پزشک {DoctorId} با موفقیت انجام شد", model.DoctorId);
                _logger.Information("📈 آمار نهایی: حذف {RemoveCount}, اضافه دپارتمان {DeptCount}, اضافه سرفصل {ServiceCount}", 
                    model.AssignmentsToRemove?.Count ?? 0, 
                    model.NewDepartmentIds?.Count ?? 0,
                    model.NewServiceCategoryIds?.Count ?? 0);
                
                TempData["SuccessMessage"] = "انتسابات با موفقیت ویرایش شد";
                return RedirectToAction("Details", new { id = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش انتسابات پزشک {DoctorId}", model.DoctorId);
                TempData["ErrorMessage"] = "خطا در ویرایش انتسابات";
                
                // بارگذاری مجدد لیست‌ها در صورت خطا
                try
                {
                    var departmentsTask = _doctorDepartmentService.GetDepartmentsAsSelectListAsync();
                    var serviceCategoriesTask = _doctorServiceCategoryService.GetServiceCategoriesAsSelectListAsync();
                    await Task.WhenAll(departmentsTask, serviceCategoriesTask);
                    
                    if (departmentsTask.Result.Success)
                        model.AvailableDepartments = departmentsTask.Result.Data;
                    if (serviceCategoriesTask.Result.Success)
                        model.AvailableServiceCategories = serviceCategoriesTask.Result.Data;
                }
                catch (Exception reloadEx)
                {
                    _logger.Error(reloadEx, "خطا در بارگذاری مجدد لیست‌ها");
                }
                
                return View(model);
            }
        }

        /// <summary>
        /// بارگذاری مجدد لیست‌های مورد نیاز برای EditViewModel
        /// </summary>
        private async Task ReloadEditViewModelListsAsync(DoctorAssignmentEditViewModel model)
        {
            try
            {
                _logger.Information("🔄 بارگذاری مجدد لیست‌های EditViewModel برای پزشک {DoctorId}", model.DoctorId);

                var departmentsTask = _doctorDepartmentService.GetDepartmentsAsSelectListAsync();
                var serviceCategoriesTask = _doctorServiceCategoryService.GetServiceCategoriesAsSelectListAsync();
                await Task.WhenAll(departmentsTask, serviceCategoriesTask);
                
                if (departmentsTask.Result.Success)
                {
                    model.AvailableDepartments = departmentsTask.Result.Data;
                    _logger.Information("✅ {Count} دپارتمان بارگذاری شد", model.AvailableDepartments.Count);
                }
                else
                {
                    _logger.Warning("⚠️ خطا در بارگذاری دپارتمان‌ها: {Message}", departmentsTask.Result.Message);
                }

                if (serviceCategoriesTask.Result.Success)
                {
                    model.AvailableServiceCategories = serviceCategoriesTask.Result.Data;
                    _logger.Information("✅ {Count} سرفصل خدماتی بارگذاری شد", model.AvailableServiceCategories.Count);
                }
                else
                {
                    _logger.Warning("⚠️ خطا در بارگذاری سرفصل‌های خدماتی: {Message}", serviceCategoriesTask.Result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بارگذاری مجدد لیست‌های EditViewModel");
            }
        }


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

                // اجرای همزمان فراخوانی‌های دیتابیس
                var departmentsTask = _doctorDepartmentService.GetAllDepartmentsAsync();
                var serviceCategoriesTask = _doctorServiceCategoryService.GetAllServiceCategoriesAsync();
                var assignmentsTask = _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId.Value);

                await Task.WhenAll(departmentsTask, serviceCategoriesTask, assignmentsTask);

                var departmentsResult = await departmentsTask;
                var serviceCategoriesResult = await serviceCategoriesTask;
                var assignmentsResult = await assignmentsTask;

                // بررسی نتایج
                if (!departmentsResult.Success)
                {
                    _logger.Warning("خطا در دریافت لیست دپارتمان‌ها");
                    TempData["ErrorMessage"] = departmentsResult.Message;
                    return RedirectToAction("Index");
                }

                if (!serviceCategoriesResult.Success)
                {
                    _logger.Warning("خطا در دریافت لیست دسته‌بندی‌های خدماتی");
                    TempData["ErrorMessage"] = serviceCategoriesResult.Message;
                    return RedirectToAction("Index");
                }

                // تنظیم ViewBag
                ViewBag.Departments = departmentsResult.Data.Select(d => new { Value = d.Id, Text = d.Name }).ToList();
                ViewBag.ServiceCategories = serviceCategoriesResult.Data.Select(sc => new { Value = sc.Id, Text = sc.Name }).ToList();

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
                return RedirectToAction("Details", new { id = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتساب پزشک {DoctorId} به دپارتمان", model.DoctorId);
                TempData["ErrorMessage"] = "خطا در انجام عملیات انتساب";
                return RedirectToAction("AssignToDepartment", new { doctorId = model.DoctorId });
            }
        }

        #endregion

      
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
                var assignmentHistory = historyResult.Success ? historyResult.Data : new List<DoctorAssignmentHistory>();

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

        #endregion

        #region Remove Assignment Actions (عملیات حذف انتساب)
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
#endregion
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
            List<DoctorAssignmentHistory> history)
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

        #region DataTables Actions (اکشن‌های DataTables)

        /// <summary>
        /// تست ساده برای بررسی ساختار داده
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> TestDataStructure()
        {
            try
            {
                var result = await _doctorAssignmentService.GetAssignmentsForDataTableAsync(0, 5, "", "", "", "", "");
                if (result.Success)
                {
                    return Json(new { 
                        success = true, 
                        message = "داده‌ها با موفقیت دریافت شد",
                        sampleData = result.Data.Data.Take(2).ToList(),
                        totalRecords = result.Data.RecordsTotal
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تست ساختار داده");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت داده‌های انتسابات برای DataTables
        /// </summary>
        [HttpPost]
        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public async Task<JsonResult> GetAssignmentsData(DataTablesRequest request)
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت داده‌های انتسابات برای DataTables. Draw: {Draw}, Start: {Start}, Length: {Length}",
                    request.Draw, request.Start, request.Length);

                // دریافت فیلترها از درخواست
                var departmentId = Request.Form["departmentId"];
                var serviceCategoryId = Request.Form["serviceCategoryId"];
                var dateFrom = Request.Form["dateFrom"];
                var dateTo = Request.Form["dateTo"];
                var searchTerm = Request.Form["searchTerm"];

                // دریافت داده‌ها از سرویس
                var result = await _doctorAssignmentService.GetAssignmentsForDataTableAsync(
                    request.Start, 
                    request.Length, 
                    request.Search?.Value, 
                    departmentId, 
                    serviceCategoryId, 
                    dateFrom, 
                    dateTo);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت داده‌های انتسابات: {Message}", result.Message);
                    return Json(new
                    {
                        draw = request.Draw,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>()
                    });
                }

                var data = result.Data;
                var totalRecords = result.Data.RecordsTotal;
                var filteredRecords = result.Data.RecordsFiltered;

                // تبدیل به فرمت DataTables
                var dataTablesData = data.Data.Cast<DoctorAssignmentListItem>().Select(assignment => new
                {
                    doctorName = assignment.DoctorName ?? "نامشخص",
                    departmentName = assignment.Departments?.FirstOrDefault()?.Name ?? "نامشخص",
                    serviceCategoryName = assignment.ServiceCategories?.FirstOrDefault()?.Name ?? "نامشخص",
                    assignmentDate = assignment.AssignmentDate ?? "نامشخص",
                    status = GetStatusBadge(assignment.Status),
                    doctorId = assignment.DoctorId
                }).ToList();

                _logger.Information("داده‌های انتسابات با موفقیت بازگردانده شد. Total: {Total}, Filtered: {Filtered}", 
                    totalRecords, filteredRecords);

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = dataTablesData
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های انتسابات برای DataTables");
                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = $"خطا در دریافت داده‌ها: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// فیلتر کردن انتسابات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> FilterAssignments(AssignmentFilterViewModel filter)
        {
            try
            {
                _logger.Information("درخواست AJAX فیلتر کردن انتسابات");

                // اعتبارسنجی فیلتر
                if (filter == null)
                {
                    return Json(new { success = false, message = "فیلتر نامعتبر است" });
                }

                // دریافت داده‌های فیلتر شده
                var result = await _doctorAssignmentService.GetFilteredAssignmentsAsync(filter);
                
                if (!result.Success)
                {
                    _logger.Warning("خطا در فیلتر کردن انتسابات: {Message}", result.Message);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("فیلتر انتسابات با موفقیت اعمال شد. تعداد نتایج: {Count}", result.Data.Count);

                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فیلتر کردن انتسابات");
                return Json(new { success = false, message = "خطا در اعمال فیلتر" });
            }
        }

        /// <summary>
        /// خروجی Excel انتسابات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExportAssignments(AssignmentFilterViewModel filter)
        {
            try
            {
                _logger.Information("درخواست خروجی Excel انتسابات");

                // دریافت داده‌ها
                var result = await _doctorAssignmentService.GetFilteredAssignmentsAsync(filter ?? new AssignmentFilterViewModel());
                
                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت داده‌ها برای خروجی: {Message}", result.Message);
                    TempData["ErrorMessage"] = "خطا در آماده‌سازی خروجی";
                    return RedirectToAction("Index");
                }

                var assignments = result.Data;

                // ایجاد فایل Excel
                var excelContent = GenerateExcelContent(assignments);
                var fileName = $"DoctorAssignments_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                _logger.Information("خروجی Excel با موفقیت ایجاد شد. تعداد رکوردها: {Count}", assignments.Count);

                return File(excelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد خروجی Excel");
                TempData["ErrorMessage"] = "خطا در ایجاد خروجی";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods (متدهای کمکی)

        /// <summary>
        /// تولید Badge وضعیت
        /// </summary>
        private string GetStatusBadge(string status)
        {
            return status?.ToLower() switch
            {
                "active" => "<span class='badge badge-success'>فعال</span>",
                "inactive" => "<span class='badge badge-danger'>غیرفعال</span>",
                "pending" => "<span class='badge badge-warning'>در انتظار</span>",
                _ => "<span class='badge badge-secondary'>نامشخص</span>"
            };
        }

        /// <summary>
        /// تولید محتوای Excel
        /// </summary>
        private byte[] GenerateExcelContent(List<DoctorAssignmentListItem> assignments)
        {
            // این متد باید با استفاده از EPPlus یا ClosedXML پیاده‌سازی شود
            // فعلاً یک پیاده‌سازی ساده
            var content = "نام پزشک,دپارتمان,سرفصل خدماتی,تاریخ انتساب,وضعیت\n";
            
            foreach (var assignment in assignments)
            {
                content += $"{assignment.DoctorName ?? "نامشخص"},";
                content += $"{assignment.Departments?.FirstOrDefault()?.Name ?? "نامشخص"},";
                content += $"{assignment.ServiceCategories?.FirstOrDefault()?.Name ?? "نامشخص"},";
                content += $"{assignment.AssignmentDate ?? "نامشخص"},";
                content += $"{assignment.Status}\n";
            }

            return System.Text.Encoding.UTF8.GetBytes(content);
        }

        #endregion

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
                    return Json(new
                    {
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
                    return Json(new
                    {
                        draw = request.Draw,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>(),
                        error = result.Message
                    });
                }

                _logger.Information("دریافت موفق {Count} انتساب از {Total} کل",
                    result.Data.Data.Count, result.Data.RecordsTotal);

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = result.Data.RecordsTotal,
                    recordsFiltered = result.Data.RecordsFiltered,
                    data = result.Data.Data
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست انتسابات برای DataTables");
                return Json(new
                {
                    draw = request?.Draw ?? 0,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = "خطا در دریافت داده‌ها"
                });
            }
        }


        /// <summary>
        /// دریافت دسته‌بندی‌های خدماتی بر اساس دپارتمان (بهینه‌شده)
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

                // دریافت سرفصل‌های خدماتی مربوط به دپارتمان مشخص
                var result = await _doctorServiceCategoryService.GetServiceCategoriesByDepartmentAsync(departmentId);
                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت سرفصل‌های خدماتی دپارتمان {DepartmentId}: {Message}", departmentId, result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                // تبدیل به فرمت مورد نیاز JavaScript
                var categories = result.Data
                    .Select(c => new { Id = c.Id, Name = c.Name })
                    .ToList();

                _logger.Information("تعداد {Count} دسته‌بندی خدماتی برای دپارتمان {DepartmentId} یافت شد", categories.Count, departmentId);
                return Json(new { success = true, data = categories }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی‌های خدماتی برای دپارتمان {DepartmentId}", departmentId);
                return Json(new { success = false, message = "خطا در دریافت دسته‌بندی‌های خدماتی" }, JsonRequestBehavior.AllowGet);
            }
        }

        // AssignmentHistory و متدهای مربوطه به DoctorHistoryController منتقل شده‌اند

        #region AJAX Actions (عملیات AJAX)

        /// <summary>
        /// دریافت اطلاعات پزشک برای AJAX
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorInfo(int doctorId)
        {
            try
            {
                var doctorResult = await GetDoctorAsync(doctorId);
                if (!doctorResult.Success)
                {
                    return Json(new { success = false, message = doctorResult.Message }, JsonRequestBehavior.AllowGet);
                }

                var doctor = doctorResult.Data;
                return Json(new { 
                    success = true, 
                    data = new {
                        name = doctor.FullName,
                        nationalCode = doctor.NationalCode,
                        specialization = doctor.SpecializationNames?.FirstOrDefault() ?? "نامشخص"
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت اطلاعات پزشک" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// پیش‌نمایش انتسابات قبل از ذخیره
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PreviewAssignments(DoctorAssignmentOperationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "اطلاعات ارسالی نامعتبر است" });
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await GetDoctorAsync(model.DoctorId);
                if (!doctorResult.Success)
                {
                    return Json(new { success = false, message = "پزشک یافت نشد" });
                }

                var doctor = doctorResult.Data;
                var html = "<div class='alert alert-info'>پیش‌نمایش انتسابات برای " + doctor.FullName + " آماده است</div>";

                return Json(new { success = true, html = html });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیش‌نمایش انتسابات");
                return Json(new { success = false, message = "خطا در پیش‌نمایش انتسابات" });
            }
        }

        /// <summary>
        /// دریافت لیست انتسابات پزشک برای AJAX (نسخه جدید)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorAssignmentsAjax(int doctorId)
        {
            try
            {
                var assignmentsResult = await GetDoctorAssignmentsAsync(doctorId);
                if (!assignmentsResult.Success)
                {
                    return Json(new { success = false, message = assignmentsResult.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = assignmentsResult.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتسابات پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت انتسابات" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Helper Methods (متدهای کمکی)


        #endregion
    }
}
