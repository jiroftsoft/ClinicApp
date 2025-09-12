using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;
using DoctorDependencyInfo = ClinicApp.Models.DoctorDependencyInfo;
using SelectListItem = ClinicApp.ViewModels.DoctorManagementVM.SelectListItem;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت حذف انتسابات پزشکان
    /// مسئولیت اصلی: مدیریت عملیات حذف انتسابات پزشکان
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. حذف انتسابات پزشکان به دپارتمان‌ها و سرفصل‌های خدماتی
    /// 2. بررسی وابستگی‌ها قبل از حذف
    /// 3. مدیریت تراکنش‌های چندگانه
    /// 4. اعتبارسنجی کامل قبل از حذف
    /// 5. ثبت تاریخچه عملیات حذف
    /// </summary>
    //[Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorRemovalController : Shared.BaseAssignmentController
    {
        #region Private Fields (فیلدهای خصوصی)

        private readonly IValidator<DoctorAssignmentRemovalViewModel> _removalValidator;

        #endregion

        #region Constructor (سازنده)

        public DoctorRemovalController(
            IDoctorAssignmentService doctorAssignmentService,
            IDoctorCrudService doctorService,
            IDoctorDepartmentService doctorDepartmentService,
            IDoctorServiceCategoryService doctorServiceCategoryService,
            IDoctorAssignmentHistoryService historyService,
            IValidator<DoctorAssignmentRemovalViewModel> removalValidator)
            : base(doctorAssignmentService, doctorService, doctorDepartmentService, doctorServiceCategoryService, historyService)
        {
            _removalValidator = removalValidator ?? throw new ArgumentNullException(nameof(removalValidator));
        }

        #endregion

        #region Main Actions (اکشن‌های اصلی)

        /// <summary>
        /// صفحه اصلی مدیریت حذف انتسابات پزشکان
        /// نمایش لیست پزشکان و گزینه‌های حذف
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("🔍 درخواست نمایش صفحه اصلی مدیریت حذف انتسابات پزشکان");

                // دریافت آمار کلی حذف انتسابات
                var statsResult = await _doctorAssignmentService.GetAssignmentStatisticsAsync();
                var stats = statsResult.Success ? statsResult.Data : new AssignmentStatsViewModel();

                // دریافت لیست پزشکان قابل حذف
                var doctorsResult = await GetDoctorsForRemovalAsync();
                var doctors = doctorsResult.Success ? doctorsResult.Data : new List<DoctorRemovalListItem>();

                // دریافت فیلترها
                var filters = await GetRemovalFiltersAsync();

                // ایجاد ViewModel با استفاده از Factory Method
                var viewModel = DoctorRemovalIndexViewModel.CreateWithData(
                    statistics: stats,
                    doctors: doctors,
                    filters: filters
                );

                _logger.Information("✅ صفحه اصلی مدیریت حذف انتسابات با موفقیت نمایش داده شد. تعداد پزشکان: {Count}", doctors.Count);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                HandleGeneralError(ex, "نمایش صفحه اصلی حذف انتسابات");
                return RedirectToAction("Index", "DoctorAssignment");
            }
        }

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

                // بررسی وجود پزشک
                if (!await ValidateDoctorExistsAsync(doctorId.Value))
                {
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await GetDoctorAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
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
                    DoctorNationalCode = doctor.NationalCode,
                    ActiveAssignmentsCount = activeAssignmentsCount,
                    DependenciesChecked = true,
                    IsPermanentRemoval = false,
                    ConfirmRemoval = false,
                    ConfirmResponsibility = false
                };

                _logger.Information("فرم حذف انتسابات پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(model);
            }
            catch (Exception ex)
            {
                HandleGeneralError(ex, "نمایش فرم حذف انتسابات", doctorId);
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
                _logger.Information("🗑️ درخواست حذف انتسابات پزشک {DoctorId}", model.DoctorId);

                // اعتبارسنجی مدل
                if (!await ValidateModelAsync(model, _removalValidator))
                {
                    _logger.Warning("❌ اعتبارسنجی مدل ناموفق برای پزشک {DoctorId}", model.DoctorId);
                    TempData["ErrorMessage"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("RemoveAssignments", new { doctorId = model.DoctorId });
                }

                // بررسی تأییدات کاربر
                if (!model.ConfirmRemoval || !model.ConfirmResponsibility)
                {
                    _logger.Warning("❌ تأییدات کاربر انجام نشده برای پزشک {DoctorId}", model.DoctorId);
                    TempData["ErrorMessage"] = "لطفاً تأییدات لازم را انجام دهید";
                    return RedirectToAction("RemoveAssignments", new { doctorId = model.DoctorId });
                }

                // بررسی وابستگی‌ها قبل از حذف
                var dependenciesResult = await _doctorAssignmentService.GetDoctorDependenciesAsync(model.DoctorId);
                var dependencies = dependenciesResult.Success ? dependenciesResult.Data : new DoctorDependencyInfo();

                if (dependencies.HasActiveDepartmentAssignments || dependencies.TotalFutureAppointments > 0)
                {
                    _logger.Warning("❌ پزشک {DoctorId} دارای وابستگی است و قابل حذف نیست", model.DoctorId);
                    TempData["ErrorMessage"] = "امکان حذف انتسابات وجود ندارد. پزشک دارای وابستگی‌های فعال است.";
                    return RedirectToAction("RemoveAssignments", new { doctorId = model.DoctorId });
                }

                // حذف کامل انتسابات پزشک
                _logger.Information("🔄 شروع حذف انتسابات پزشک {DoctorId}", model.DoctorId);
                var result = await _doctorAssignmentService.RemoveAllDoctorAssignmentsAsync(model.DoctorId);

                if (!result.Success)
                {
                    _logger.Error("❌ حذف انتسابات پزشک {DoctorId} ناموفق بود: {Message}", model.DoctorId, result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("RemoveAssignments", new { doctorId = model.DoctorId });
                }

                // ثبت تاریخچه حذف
                await LogAssignmentOperationAsync(
                    model.DoctorId,
                    "RemoveAllAssignments",
                    "حذف کامل انتسابات",
                    $"دلیل: {model.RemovalReason}. حذف {(model.IsPermanentRemoval ? "قطعی" : "نرم")}",
                    importance: AssignmentHistoryImportance.Critical);

                _logger.Information("✅ حذف انتسابات پزشک {DoctorId} با موفقیت انجام شد", model.DoctorId);
                TempData["SuccessMessage"] = "حذف انتسابات با موفقیت انجام شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                HandleGeneralError(ex, "حذف انتسابات", model.DoctorId);
                return RedirectToAction("RemoveAssignments", new { doctorId = model.DoctorId });
            }
        }

        /// <summary>
        /// تأیید حذف انتسابات (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ConfirmRemoval(int doctorId)
        {
            try
            {
                _logger.Information("🔍 درخواست تأیید حذف انتسابات پزشک {DoctorId}", doctorId);

                // بررسی وجود پزشک
                if (!await ValidateDoctorExistsAsync(doctorId))
                {
                    _logger.Warning("❌ پزشک {DoctorId} یافت نشد", doctorId);
                    return Json(new { success = false, message = "پزشک مورد نظر یافت نشد" });
                }

                // دریافت اطلاعات وابستگی‌ها
                var dependenciesResult = await _doctorAssignmentService.GetDoctorDependenciesAsync(doctorId);
                var dependencies = dependenciesResult.Success ? dependenciesResult.Data : new DoctorDependencyInfo();

                // دریافت تعداد انتسابات فعال
                var activeAssignmentsResult = await _doctorAssignmentService.GetActiveAssignmentsCountAsync(doctorId);
                var activeAssignmentsCount = activeAssignmentsResult.Success ? activeAssignmentsResult.Data : 0;

                // بررسی امکان حذف
                if (dependencies.HasActiveDepartmentAssignments || dependencies.TotalFutureAppointments > 0)
                {
                    _logger.Warning("❌ پزشک {DoctorId} دارای وابستگی است", doctorId);
                    return Json(new { 
                        success = false, 
                        message = "امکان حذف انتسابات وجود ندارد. پزشک دارای وابستگی‌های فعال است.",
                        canRemove = false,
                        dependencies = new {
                            hasActiveDepartmentAssignments = dependencies.HasActiveDepartmentAssignments,
                            totalFutureAppointments = dependencies.TotalFutureAppointments,
                            activeAssignmentsCount = activeAssignmentsCount
                        }
                    });
                }

                _logger.Information("✅ پزشک {DoctorId} قابل حذف است", doctorId);
                return Json(new { 
                    success = true, 
                    message = "امکان حذف انتسابات وجود دارد",
                    canRemove = true,
                    dependencies = new {
                        hasActiveDepartmentAssignments = dependencies.HasActiveDepartmentAssignments,
                        totalFutureAppointments = dependencies.TotalFutureAppointments,
                        activeAssignmentsCount = activeAssignmentsCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تأیید حذف انتسابات پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در بررسی امکان حذف" });
            }
        }

        /// <summary>
        /// حذف دسته‌ای انتسابات (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> BulkRemoval(int[] doctorIds)
        {
            try
            {
                _logger.Information("🗑️ درخواست حذف دسته‌ای انتسابات برای {Count} پزشک", doctorIds?.Length ?? 0);

                if (doctorIds == null || doctorIds.Length == 0)
                {
                    _logger.Warning("❌ هیچ پزشکی برای حذف دسته‌ای انتخاب نشده است");
                    return Json(new { success = false, message = "هیچ پزشکی انتخاب نشده است" });
                }

                // بررسی حداکثر تعداد
                if (doctorIds.Length > 50)
                {
                    _logger.Warning("❌ تعداد پزشکان انتخاب شده بیش از حد مجاز است: {Count}", doctorIds.Length);
                    return Json(new { success = false, message = "حداکثر 50 پزشک قابل انتخاب است" });
                }

                var results = new List<object>();
                var successCount = 0;
                var failureCount = 0;

                foreach (var doctorId in doctorIds)
                {
                    try
                    {
                        // بررسی وابستگی‌ها قبل از حذف
                        var dependenciesResult = await _doctorAssignmentService.GetDoctorDependenciesAsync(doctorId);
                        var dependencies = dependenciesResult.Success ? dependenciesResult.Data : new DoctorDependencyInfo();

                        if (dependencies.HasActiveDepartmentAssignments || dependencies.TotalFutureAppointments > 0)
                        {
                            failureCount++;
                            results.Add(new { 
                                doctorId, 
                                success = false, 
                                message = "دارای وابستگی‌های فعال" 
                            });
                            continue;
                        }

                        var result = await _doctorAssignmentService.RemoveAllDoctorAssignmentsAsync(doctorId);
                        if (result.Success)
                        {
                            successCount++;
                            results.Add(new { doctorId, success = true, message = "موفق" });
                            
                            // ثبت تاریخچه
                            await LogAssignmentOperationAsync(
                                doctorId,
                                "BulkRemoveAssignments",
                                "حذف دسته‌ای انتسابات",
                                "انتسابات در عملیات حذف دسته‌ای حذف شد",
                                importance: AssignmentHistoryImportance.Critical);
                        }
                        else
                        {
                            failureCount++;
                            results.Add(new { doctorId, success = false, message = result.Message });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "❌ خطا در حذف انتسابات پزشک {DoctorId}", doctorId);
                        failureCount++;
                        results.Add(new { doctorId, success = false, message = "خطا در حذف" });
                    }
                }

                _logger.Information("✅ حذف دسته‌ای انتسابات تکمیل شد. موفق: {SuccessCount}, ناموفق: {FailureCount}", successCount, failureCount);

                return Json(new { 
                    success = true, 
                    message = $"حذف دسته‌ای تکمیل شد. {successCount} موفق، {failureCount} ناموفق",
                    successCount,
                    failureCount,
                    results
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در حذف دسته‌ای انتسابات");
                return Json(new { success = false, message = "خطا در حذف دسته‌ای" });
            }
        }

        #endregion

        #region Private Helper Methods (متدهای کمکی خصوصی)

        /// <summary>
        /// دریافت لیست پزشکان قابل حذف انتسابات
        /// </summary>
        private async Task<ServiceResult<List<DoctorRemovalListItem>>> GetDoctorsForRemovalAsync(DoctorRemovalFiltersViewModel filters = null)
        {
            try
            {
                _logger.Information("🔍 دریافت لیست پزشکان قابل حذف انتسابات");

                // دریافت لیست تمام پزشکان
                var searchFilter = new DoctorSearchViewModel
                {
                    PageNumber = 1,
                    PageSize = 1000, // دریافت همه پزشکان
                    SearchTerm = filters?.DoctorName,
                    DepartmentId = filters?.DepartmentId
                };
                var doctorsResult = await _doctorService.GetDoctorsAsync(searchFilter);
                if (!doctorsResult.Success)
                {
                    return ServiceResult<List<DoctorRemovalListItem>>.Failed(doctorsResult.Message);
                }

                var doctors = new List<DoctorRemovalListItem>();

                foreach (var doctor in doctorsResult.Data.Items)
                {
                    try
                    {
                        // دریافت تعداد انتسابات فعال
                        var activeAssignmentsResult = await _doctorAssignmentService.GetActiveAssignmentsCountAsync(doctor.Id);
                        var activeAssignmentsCount = activeAssignmentsResult.Success ? activeAssignmentsResult.Data : 0;

                        // دریافت وابستگی‌ها
                        var dependenciesResult = await _doctorAssignmentService.GetDoctorDependenciesAsync(doctor.DoctorId);
                        var dependencies = dependenciesResult.Success ? dependenciesResult.Data : new DoctorDependencyInfo();

                        // اعمال فیلترهای اضافی
                        if (filters != null)
                        {
                            // فیلتر حداقل تعداد انتسابات
                            if (filters.MinAssignmentsCount.HasValue && activeAssignmentsCount < filters.MinAssignmentsCount.Value)
                                continue;

                            // فیلتر فقط انتسابات فعال
                            if (filters.ShowOnlyActiveAssignments && activeAssignmentsCount == 0)
                                continue;

                            // فیلتر فقط بدون وابستگی
                            if (filters.ShowOnlyWithoutDependencies && (dependencies.HasActiveDepartmentAssignments || dependencies.TotalFutureAppointments > 0))
                                continue;
                        }

                        // ایجاد آیتم لیست
                        var listItem = new DoctorRemovalListItem
                        {
                            DoctorId = doctor.DoctorId,
                            DoctorName = doctor.FullName ?? $"{doctor.FirstName} {doctor.LastName}",
                            NationalCode = doctor.NationalCode,
                            Specialization = doctor.SpecializationNames?.FirstOrDefault() ?? "نامشخص",
                            ActiveAssignments = activeAssignmentsCount,
                            TotalAssignments = activeAssignmentsCount, // TODO: دریافت تعداد کل انتسابات
                            HasDependencies = dependencies.HasActiveDepartmentAssignments || dependencies.TotalFutureAppointments > 0,
                            CanBeRemoved = !dependencies.HasActiveDepartmentAssignments && dependencies.TotalFutureAppointments == 0,
                            RemovalBlockReason = dependencies.HasActiveDepartmentAssignments ? "دارای انتسابات فعال" : 
                                               dependencies.TotalFutureAppointments > 0 ? "دارای نوبت‌های آینده" : null
                        };

                        doctors.Add(listItem);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "خطا در دریافت اطلاعات پزشک {DoctorId}", doctor.Id);
                    }
                }

                _logger.Information("✅ لیست پزشکان قابل حذف با موفقیت دریافت شد. تعداد: {Count}", doctors.Count);
                return ServiceResult<List<DoctorRemovalListItem>>.Successful(doctors);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان قابل حذف");
                return ServiceResult<List<DoctorRemovalListItem>>.Failed("خطا در دریافت لیست پزشکان");
            }
        }

        /// <summary>
        /// جستجوی پزشکان بر اساس فیلترها (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> SearchDoctors(DoctorRemovalFiltersViewModel filters)
        {
            try
            {
                _logger.Information("🔍 درخواست جستجوی پزشکان با فیلترها: {@Filters}", filters);

                // دریافت لیست پزشکان با فیلترها
                var doctorsResult = await GetDoctorsForRemovalAsync(filters);
                var doctors = doctorsResult.Success ? doctorsResult.Data : new List<DoctorRemovalListItem>();

                // دریافت فیلترها
                var updatedFilters = await GetRemovalFiltersAsync();

                // ایجاد ViewModel
                var viewModel = DoctorRemovalIndexViewModel.CreateWithData(
                    new AssignmentStatsViewModel(), // آمار کلی
                    doctors,
                    updatedFilters
                );

                return PartialView("_DoctorsList", viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در جستجوی پزشکان");
                return Json(new { success = false, message = "خطا در جستجو" });
            }
        }

        /// <summary>
        /// دریافت جزئیات پزشک (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetDoctorDetails(int doctorId)
        {
            try
            {
                _logger.Information("🔍 درخواست دریافت جزئیات پزشک: {DoctorId}", doctorId);

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success)
                {
                    return PartialView("_DoctorDetailsError", "پزشک یافت نشد");
                }

                var doctor = doctorResult.Data;

                // دریافت انتسابات فعال
                var activeAssignmentsResult = await _doctorAssignmentService.GetActiveAssignmentsCountAsync(doctorId);
                var activeAssignmentsCount = activeAssignmentsResult.Success ? activeAssignmentsResult.Data : 0;

                // دریافت وابستگی‌ها
                var dependenciesResult = await _doctorAssignmentService.GetDoctorDependenciesAsync(doctorId);
                var dependencies = dependenciesResult.Success ? dependenciesResult.Data : new DoctorDependencyInfo();

                // دریافت انتسابات دپارتمان
                var departmentAssignmentsResult = await _doctorServiceCategoryService.GetDoctorDepartmentsAsync(doctorId);
                var departmentAssignments = departmentAssignmentsResult.Success ? departmentAssignmentsResult.Data : new List<LookupItemViewModel>();

                // دریافت انتسابات سرفصل‌های خدماتی (ساده شده)
                var serviceCategoryAssignments = new List<LookupItemViewModel>();

                var viewModel = new DoctorDetailsViewModel
                {
                    DoctorId = doctor.DoctorId,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    FullName = doctor.FullName,
                    NationalCode = doctor.NationalCode,
                    MedicalCouncilCode = doctor.MedicalCouncilCode,
                    SpecializationNames = doctor.SpecializationNames,
                    IsActive = doctor.IsActive,
                    ActiveAssignmentsCount = activeAssignmentsCount,
                    Dependencies = dependencies,
                    DepartmentAssignments = departmentAssignments,
                    ServiceCategoryAssignments = serviceCategoryAssignments
                };

                return PartialView("_DoctorDetails", viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت جزئیات پزشک");
                return PartialView("_DoctorDetailsError", "خطا در دریافت جزئیات");
            }
        }

        /// <summary>
        /// دریافت سرفصل‌های خدماتی بر اساس دپارتمان (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GetServiceCategoriesByDepartment(int departmentId)
        {
            try
            {
                _logger.Information("🔍 درخواست دریافت سرفصل‌های خدماتی برای دپارتمان: {DepartmentId}", departmentId);

                var serviceCategories = await _doctorServiceCategoryService.GetServiceCategoriesByDepartmentAsync(departmentId);
                
                if (serviceCategories.Success)
                {
                    var selectList = serviceCategories.Data.Select(sc => new SelectListItem
                    {
                        Value = sc.Id.ToString(),
                        Text = sc.Name
                    }).ToList();

                    return Json(new { success = true, data = selectList });
                }

                return Json(new { success = false, message = "خطا در دریافت سرفصل‌ها" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت سرفصل‌های خدماتی");
                return Json(new { success = false, message = "خطا در دریافت سرفصل‌ها" });
            }
        }

        /// <summary>
        /// دریافت فیلترهای حذف انتسابات
        /// </summary>
        private async Task<DoctorRemovalFiltersViewModel> GetRemovalFiltersAsync()
        {
            try
            {
                _logger.Information("🔍 دریافت فیلترهای حذف انتسابات");

                var filters = new DoctorRemovalFiltersViewModel();

                // دریافت لیست دپارتمان‌ها
                var departmentsResult = await GetDepartmentsSelectListAsync();
                filters.Departments = departmentsResult;

                // دریافت لیست سرفصل‌های خدماتی
                var serviceCategoriesResult = await GetServiceCategoriesSelectListAsync();
                filters.ServiceCategories = serviceCategoriesResult;

                _logger.Information("✅ فیلترهای حذف انتسابات با موفقیت دریافت شد");
                return filters;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت فیلترهای حذف انتسابات");
                return new DoctorRemovalFiltersViewModel();
            }
        }

        #endregion
    }
}
