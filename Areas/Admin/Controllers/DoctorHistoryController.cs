using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.DoctorManagementVM;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using Serilog;
using SelectListItem = System.Web.WebPages.Html.SelectListItem;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت تاریخچه پزشکان
    /// این کنترلر برای نمایش و مدیریت تاریخچه کامل پزشکان طراحی شده است
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش تاریخچه کامل انتسابات پزشکان
    /// 2. جستجو و فیلتر پیشرفته در تاریخچه
    /// 3. گزارش‌گیری از تاریخچه
    /// 4. مدیریت و نگهداری تاریخچه
    /// 5. رعایت استانداردهای پزشکی و حسابرسی
    /// </summary>
    //[Authorize]
    [MedicalEnvironmentFilter]
    [CheckProfileCompletion]
    public class DoctorHistoryController : Controller
    {
        private readonly IDoctorAssignmentHistoryService _historyService;
        private readonly IDoctorCrudService _doctorService;
        private readonly IDepartmentManagementService _departmentService;
        private readonly ILogger _logger;

        public DoctorHistoryController(
            IDoctorAssignmentHistoryService historyService,
            IDoctorCrudService doctorService,
            IDepartmentManagementService departmentService)
        {
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _doctorService = doctorService ?? throw new ArgumentNullException(nameof(doctorService));
            _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
            _logger = Log.ForContext<DoctorHistoryController>();
        }

        #region Main Actions (اکشن‌های اصلی)

        /// <summary>
        /// صفحه اصلی تاریخچه پزشکان
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(int? doctorId = null, string actionType = null,
            DateTime? startDate = null, DateTime? endDate = null, int page = 1)
        {
            try
            {
                _logger.Information("درخواست نمایش صفحه اصلی تاریخچه پزشکان. DoctorId: {DoctorId}, ActionType: {ActionType}, Page: {Page}",
                    doctorId, actionType, page);

                // تنظیم پارامترهای پیش‌فرض
                if (page < 1) page = 1;
                const int pageSize = 20;

                // دریافت داده‌های مورد نیاز برای dropdown ها
                var (doctors, departments) = await LoadSearchDataAsync();

                // دریافت تاریخچه بر اساس فیلترها
                var historyResult = await GetFilteredHistoryAsync(doctorId, actionType, startDate, endDate, page, pageSize);
                if (!historyResult.Success)
                {
                    _logger.Warning("خطا در دریافت تاریخچه: {Message}", historyResult.Message);
                    TempData["ErrorMessage"] = historyResult.Message;
                    return View(new DoctorHistoryIndexViewModel());
                }

                // دریافت آمار کلی
                var statsResult = await _historyService.GetHistoryStatisticsAsync(startDate, endDate);
                var statistics = statsResult.Success ? statsResult.Data : new AssignmentHistoryStatisticsViewModel();

                // ایجاد ViewModel
                var viewModel = new DoctorHistoryIndexViewModel
                {
                    Histories = historyResult.Data.Select(h => new AssignmentHistoryViewModel
                    {
                        Id = h.Id,
                        ActionType = h.ActionType,
                        ActionTitle = h.ActionTitle,
                        ActionDescription = h.ActionDescription,
                        ActionDate = h.ActionDate,
                        DepartmentName = h.DepartmentName,
                        ServiceCategories = h.ServiceCategories,
                        PerformedBy = h.PerformedByUserName,
                        PerformedByUserName = h.PerformedByUserName, // اضافه کردن property جدید
                        Notes = h.Notes,
                        DoctorId = h.DoctorId,
                        DoctorName = h.Doctor?.FullName ?? "نامشخص",
                        DoctorNationalCode = h.Doctor?.NationalCode ?? "نامشخص",
                        DepartmentId = h.DepartmentId,
                        PerformedByUserId = h.PerformedByUserId,
                        IsActive = !h.IsDeleted,
                        CreatedAt = h.CreatedAt,
                        UpdatedAt = h.UpdatedAt,
                        // اضافه کردن properties جدید
                        Importance = h.Importance.ToString(),
                        DoctorSpecialization = h.Doctor?.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "نامشخص"
                    }).ToList(),

                    Statistics = statistics,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)statistics.TotalHistoryRecords / pageSize),

                    // فیلترها
                    FilterDoctorId = doctorId,
                    FilterActionType = actionType,
                    FilterStartDate = startDate,
                    FilterEndDate = endDate,

                    // گزینه‌های فیلتر
                    ActionTypes = new[]
                    {
                        new System.Web.Mvc.SelectListItem { Text = "همه عملیات", Value = "" },
                        new System.Web.Mvc.SelectListItem { Text = "انتساب به دپارتمان", Value = "AssignToDepartment" },
                        new System.Web.Mvc.SelectListItem { Text = "انتساب سرفصل‌های خدماتی", Value = "AssignServiceCategories" },
                        new System.Web.Mvc.SelectListItem { Text = "انتقال", Value = "Transfer" },
                        new System.Web.Mvc.SelectListItem { Text = "حذف", Value = "Removal" },
                        new System.Web.Mvc.SelectListItem { Text = "ویرایش", Value = "Update" }
                    },

                    // Dropdown data
                    Doctors = doctors,
                    Departments = departments
                };

                _logger.Information("صفحه اصلی تاریخچه پزشکان با موفقیت نمایش داده شد. تعداد رکوردها: {Count}",
                    viewModel.Histories.Count);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه اصلی تاریخچه پزشکان");
                TempData["ErrorMessage"] = "خطا در بارگذاری تاریخچه پزشکان";
                return View(new DoctorHistoryIndexViewModel());
            }
        }

        /// <summary>
        /// نمایش جزئیات یک رکورد تاریخچه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.Information("درخواست نمایش جزئیات تاریخچه {Id}", id);

                if (id <= 0)
                {
                    _logger.Warning("شناسه تاریخچه نامعتبر: {Id}", id);
                    TempData["ErrorMessage"] = "شناسه تاریخچه نامعتبر است";
                    return RedirectToAction("Index");
                }

                var result = await _historyService.GetByIdAsync(id);
                if (!result.Success)
                {
                    _logger.Warning("تاریخچه با شناسه {Id} یافت نشد: {Message}", id, result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                var history = result.Data;
                var viewModel = new AssignmentHistoryViewModel
                {
                    Id = history.Id,
                    ActionType = history.ActionType,
                    ActionTitle = history.ActionTitle,
                    ActionDescription = history.ActionDescription,
                    ActionDate = history.ActionDate,
                    DepartmentName = history.DepartmentName,
                    ServiceCategories = history.ServiceCategories,
                    PerformedBy = history.PerformedByUserName,
                    PerformedByUserName = history.PerformedByUserName,
                    Notes = history.Notes,
                    DoctorId = history.DoctorId,
                    DoctorName = history.Doctor?.FullName ?? "نامشخص",
                    DoctorNationalCode = history.Doctor?.NationalCode ?? "نامشخص",
                    DoctorSpecialization = history.Doctor?.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "نامشخص",
                    DepartmentId = history.DepartmentId,
                    PerformedByUserId = history.PerformedByUserId,
                    IsActive = !history.IsDeleted,
                    IsDeleted = history.IsDeleted,
                    CreatedAt = history.CreatedAt,
                    CreatedDate = history.CreatedAt,
                    UpdatedAt = history.UpdatedAt,
                    UpdatedDate = history.UpdatedAt,
                    Importance = history.Importance.ToString(),
                    ClinicName = history.Department?.Clinic?.Name ?? "نامشخص"
                };

                _logger.Information("جزئیات تاریخچه {Id} با موفقیت نمایش داده شد", id);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات تاریخچه {Id}", id);
                TempData["ErrorMessage"] = "خطا در بارگذاری جزئیات تاریخچه";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Search & Filter Actions (اکشن‌های جستجو و فیلتر)

        /// <summary>
        /// جستجوی پیشرفته در تاریخچه - بهینه‌سازی شده برای محیط عملیاتی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Search(string searchTerm = null, int? doctorId = null,
            string actionType = null, DateTime? startDate = null, DateTime? endDate = null,
            string performedBy = null, int? departmentId = null, string importance = null, int page = 1)
        {
            try
            {
                _logger.Information("درخواست جستجوی پیشرفته در تاریخچه. SearchTerm: {SearchTerm}, DoctorId: {DoctorId}, Page: {Page}",
                    searchTerm, doctorId, page);

                // تنظیم پارامترهای پیش‌فرض
                if (page < 1) page = 1;
                const int pageSize = 20;

                // دریافت داده‌های مورد نیاز برای dropdown ها
                var (doctors, departments) = await LoadSearchDataAsync();

                // اگر هیچ parameter ارسال نشده، صفحه خالی نمایش داده شود
                if (string.IsNullOrEmpty(searchTerm) && !doctorId.HasValue && string.IsNullOrEmpty(actionType) &&
                    !startDate.HasValue && !endDate.HasValue && string.IsNullOrEmpty(performedBy) &&
                    !departmentId.HasValue && string.IsNullOrEmpty(importance) && page == 1)
                {
                    var emptyViewModel = CreateEmptySearchViewModel(doctors, departments);
                    return View(emptyViewModel);
                }

                // جستجوی پیشرفته با فیلترهای اضافی
                AssignmentHistoryImportance? importanceEnum = null;
                if (!string.IsNullOrEmpty(importance))
                {
                    if (Enum.TryParse<AssignmentHistoryImportance>(importance, out var parsedImportance))
                    {
                        importanceEnum = parsedImportance;
                    }
                }

                var searchResult = await _historyService.SearchHistoryAsync(searchTerm, actionType, importanceEnum,
                    departmentId, performedBy, startDate, endDate, page, pageSize);

                if (!searchResult.Success)
                {
                    _logger.Warning("خطا در جستجوی تاریخچه: {Message}", searchResult.Message);
                    TempData["ErrorMessage"] = searchResult.Message;
                    return View(CreateErrorSearchViewModel(searchTerm, doctorId, actionType, startDate, endDate, performedBy, departmentId, importance, doctors, departments));
                }

                // فیلتر کردن نتایج بر اساس departmentId اگر ارائه شده
                var filteredData = searchResult.Data;
                if (departmentId.HasValue)
                {
                    filteredData = filteredData.Where(h => h.DepartmentId == departmentId.Value).ToList();
                }

                var viewModel = CreateSearchViewModel(filteredData, searchTerm, doctorId, actionType, startDate, endDate, performedBy, departmentId, importance, page, pageSize, doctors, departments);

                _logger.Information("جستجوی پیشرفته تاریخچه با موفقیت انجام شد. تعداد نتایج: {Count}",
                    viewModel.Histories.Count);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پیشرفته تاریخچه");
                TempData["ErrorMessage"] = "خطا در انجام جستجو";
                var (doctors, departments) = await LoadSearchDataAsync();
                return View(CreateErrorSearchViewModel(searchTerm, doctorId, actionType, startDate, endDate, performedBy, departmentId, importance, doctors, departments));
            }
        }

        #endregion

        #region Report Actions (اکشن‌های گزارش‌گیری)

        /// <summary>
        /// گزارش آماری تاریخچه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Statistics(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("درخواست گزارش آماری تاریخچه. StartDate: {StartDate}, EndDate: {EndDate}",
                    startDate, endDate);

                var result = await _historyService.GetHistoryStatisticsAsync(startDate, endDate);
                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت آمار تاریخچه: {Message}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return View(new DoctorHistoryAdvancedStatisticsViewModel());
                }

                var statistics = result.Data;

                // ایجاد ViewModel پیشرفته
                var advancedStatistics = new DoctorHistoryAdvancedStatisticsViewModel
                {
                    StartDate = startDate ?? DateTime.Now.AddMonths(-1),
                    EndDate = endDate ?? DateTime.Now,
                    GeneratedAt = DateTime.Now,

                    // آمار کلی
                    TotalRecords = statistics.TotalHistoryRecords,
                    CriticalRecords = statistics.CriticalRecords,
                    ImportantRecords = statistics.ImportantRecords,
                    NormalRecords = statistics.NormalRecords,
                    AssignmentCount = statistics.AssignmentsCount,
                    RemovalCount = statistics.RemovalsCount,
                    UpdateCount = statistics.UpdatesCount,

                    // آمار ماهانه (نمونه)
                    MonthlyStatistics = new List<MonthlyStatisticsViewModel>
                    {
                        new MonthlyStatisticsViewModel
                        {
                            Year = DateTime.Now.Year,
                            Month = DateTime.Now.Month,
                            MonthName = "فروردین",
                            TotalOperations = statistics.TotalHistoryRecords,
                            RecordCount = statistics.TotalHistoryRecords,
                            AssignmentsCount = statistics.AssignmentsCount,
                            TransfersCount = statistics.TransfersCount,
                            RemovalsCount = statistics.RemovalsCount,
                            UpdatesCount = statistics.UpdatesCount
                        }
                    },

                    // آمار پزشکان (نمونه)
                    DoctorActivity = new List<DoctorActivityStatisticsViewModel>
                    {
                        new DoctorActivityStatisticsViewModel
                        {
                            DoctorId = 1,
                            DoctorName = "دکتر نمونه",
                            DoctorNationalCode = "1234567890",
                            TotalOperations = 10,
                            RecordCount = 10,
                            LastOperation = DateTime.Now,
                            AverageOperationsPerMonth = 5.5m
                        }
                    },

                    // آمار کاربران (نمونه)
                    UserActivity = new List<UserActivityStatisticsViewModel>
                    {
                        new UserActivityStatisticsViewModel
                        {
                            UserId = "1",
                            UserName = "کاربر نمونه",
                            TotalOperations = 15,
                            RecordCount = 15,
                            LastOperation = DateTime.Now,
                            AverageOperationsPerDay = 2.5m
                        }
                    },

                    // آمار دپارتمان‌ها (نمونه)
                    DepartmentActivity = new List<DepartmentActivityStatisticsViewModel>
                    {
                        new DepartmentActivityStatisticsViewModel
                        {
                            DepartmentId = 1,
                            DepartmentName = "دپارتمان نمونه",
                            TotalOperations = 20,
                            TotalRecords = 20,
                            ActiveDoctorsCount = 5,
                            LastOperation = DateTime.Now,
                            CriticalRecords = statistics.CriticalRecords,
                            ImportantRecords = statistics.ImportantRecords,
                            NormalRecords = statistics.NormalRecords
                        }
                    }
                };

                _logger.Information("گزارش آماری تاریخچه با موفقیت تولید شد");
                return View(advancedStatistics);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید گزارش آماری تاریخچه");
                TempData["ErrorMessage"] = "خطا در تولید گزارش آماری";
                return View(new DoctorHistoryAdvancedStatisticsViewModel
                {
                    StartDate = startDate ?? DateTime.Now.AddMonths(-1),
                    EndDate = endDate ?? DateTime.Now,
                    GeneratedAt = DateTime.Now,
                    TotalRecords = 0,
                    CriticalRecords = 0,
                    ImportantRecords = 0,
                    NormalRecords = 0,
                    AssignmentCount = 0,
                    RemovalCount = 0,
                    UpdateCount = 0,
                    MonthlyStatistics = new List<MonthlyStatisticsViewModel>(),
                    DoctorActivity = new List<DoctorActivityStatisticsViewModel>(),
                    UserActivity = new List<UserActivityStatisticsViewModel>(),
                    DepartmentActivity = new List<DepartmentActivityStatisticsViewModel>()
                });
            }
        }

        /// <summary>
        /// گزارش تاریخچه پزشک خاص
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> DoctorReport(int doctorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("درخواست گزارش تاریخچه پزشک {DoctorId}. StartDate: {StartDate}, EndDate: {EndDate}",
                    doctorId, startDate, endDate);

                if (doctorId <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر: {DoctorId}", doctorId);
                    TempData["ErrorMessage"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index");
                }

                var result = await _historyService.GetDoctorHistoryAsync(doctorId, 1, 1000);
                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت تاریخچه پزشک {DoctorId}: {Message}", doctorId, result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                var histories = result.Data;
                if (startDate.HasValue)
                    histories = histories.Where(h => h.ActionDate >= startDate.Value).ToList();
                if (endDate.HasValue)
                    histories = histories.Where(h => h.ActionDate <= endDate.Value).ToList();

                var firstHistory = histories.FirstOrDefault();
                var viewModel = new DoctorHistoryReportViewModel
                {
                    DoctorId = doctorId,
                    DoctorName = firstHistory?.Doctor?.FullName ?? "نامشخص",
                    DoctorNationalCode = firstHistory?.Doctor?.NationalCode ?? "نامشخص",
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalOperations = histories.Count,
                    TotalRecords = histories.Count,
                    AssignmentsCount = histories.Count(h => h.ActionType == "AssignToDepartment" || h.ActionType == "AssignServiceCategories"),
                    AssignmentCount = histories.Count(h => h.ActionType == "AssignToDepartment" || h.ActionType == "AssignServiceCategories"),
                    TransfersCount = histories.Count(h => h.ActionType == "Transfer"),
                    RemovalsCount = histories.Count(h => h.ActionType == "Removal"),
                    UpdatesCount = histories.Count(h => h.ActionType == "Update"),
                    RemovalCount = histories.Count(h => h.ActionType == "Removal"),
                    UpdateCount = histories.Count(h => h.ActionType == "Update"),
                    CriticalRecords = histories.Count(h => h.Importance == AssignmentHistoryImportance.Critical),
                    ImportantRecords = histories.Count(h => h.Importance == AssignmentHistoryImportance.Important),
                    NormalRecords = histories.Count(h => h.Importance == AssignmentHistoryImportance.Normal),
                    HistoryItems = histories.Select(h => new AssignmentHistoryViewModel
                    {
                        Id = h.Id,
                        ActionType = h.ActionType,
                        ActionTitle = h.ActionTitle,
                        ActionDescription = h.ActionDescription,
                        ActionDate = h.ActionDate,
                        DepartmentName = h.DepartmentName,
                        ServiceCategories = h.ServiceCategories,
                        PerformedBy = h.PerformedByUserName,
                        PerformedByUserName = h.PerformedByUserName,
                        Notes = h.Notes,
                        DoctorId = h.DoctorId,
                        DoctorName = h.Doctor?.FullName ?? "نامشخص",
                        DoctorNationalCode = h.Doctor?.NationalCode ?? "نامشخص",
                        DoctorSpecialization = h.Doctor?.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "نامشخص",
                        DepartmentId = h.DepartmentId,
                        PerformedByUserId = h.PerformedByUserId,
                        IsActive = !h.IsDeleted,
                        IsDeleted = h.IsDeleted,
                        CreatedAt = h.CreatedAt,
                        CreatedDate = h.CreatedAt,
                        UpdatedAt = h.UpdatedAt,
                        UpdatedDate = h.UpdatedAt,
                        Importance = h.Importance.ToString(),
                        ClinicName = h.Department?.Clinic?.Name ?? "نامشخص"
                    }).ToList(),
                    Doctor = new DoctorInfoViewModel
                    {
                        Id = firstHistory?.DoctorId ?? doctorId,
                        FullName = firstHistory?.Doctor?.FullName ?? "نامشخص",
                        Specialization = firstHistory?.Doctor?.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "نامشخص",
                        ClinicName = firstHistory?.Department?.Clinic?.Name ?? "نامشخص",
                        IsActive = firstHistory?.Doctor?.IsActive ?? false
                    },
                    GeneratedAt = DateTime.Now,
                    GeneratedBy = "سیستم"
                };

                _logger.Information("گزارش تاریخچه پزشک {DoctorId} با موفقیت تولید شد", doctorId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید گزارش تاریخچه پزشک {DoctorId}", doctorId);
                TempData["ErrorMessage"] = "خطا در تولید گزارش تاریخچه پزشک";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Maintenance Actions (اکشن‌های نگهداری)

        /// <summary>
        /// حذف نرم رکورد تاریخچه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                _logger.Information("درخواست حذف تاریخچه {Id}", id);

                if (id <= 0)
                {
                    return Json(new { success = false, message = "شناسه تاریخچه نامعتبر است" });
                }

                var result = await _historyService.DeleteAsync(id);
                if (!result.Success)
                {
                    _logger.Warning("خطا در حذف تاریخچه {Id}: {Message}", id, result.Message);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("تاریخچه {Id} با موفقیت حذف شد", id);
                return Json(new { success = true, message = "تاریخچه با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تاریخچه {Id}", id);
                return Json(new { success = false, message = "خطا در حذف تاریخچه" });
            }
        }

        /// <summary>
        /// پاکسازی تاریخچه قدیمی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CleanupOldHistory(int daysToKeep = 365)
        {
            try
            {
                _logger.Information("درخواست پاکسازی تاریخچه قدیمی. DaysToKeep: {DaysToKeep}", daysToKeep);

                if (daysToKeep < 30)
                {
                    return Json(new { success = false, message = "حداقل 30 روز باید حفظ شود" });
                }

                var result = await _historyService.CleanupOldHistoryAsync(daysToKeep);
                if (!result.Success)
                {
                    _logger.Warning("خطا در پاکسازی تاریخچه قدیمی: {Message}", result.Message);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("پاکسازی تاریخچه قدیمی با موفقیت انجام شد. تعداد رکوردهای حذف شده: {Count}",
                    result.Data);
                return Json(new { success = true, message = $"تعداد {result.Data} رکورد قدیمی حذف شد" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاکسازی تاریخچه قدیمی");
                return Json(new { success = false, message = "خطا در پاکسازی تاریخچه قدیمی" });
            }
        }

        #endregion

        #region AJAX Actions (اکشن‌های AJAX)

        /// <summary>
        /// دریافت تاریخچه به صورت AJAX
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetHistoryAjax(int? doctorId = null, string actionType = null,
            DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت تاریخچه. DoctorId: {DoctorId}, Page: {Page}",
                    doctorId, page);

                var historyResult = await GetFilteredHistoryAsync(doctorId, actionType, startDate, endDate, page, pageSize);
                if (!historyResult.Success)
                {
                    return Json(new { success = false, message = historyResult.Message }, JsonRequestBehavior.AllowGet);
                }

                var historyData = historyResult.Data.Select(h => new
                {
                    Id = h.Id,
                    ActionType = h.ActionType,
                    ActionTitle = h.ActionTitle,
                    ActionDescription = h.ActionDescription,
                    ActionDate = h.ActionDate.ToString("yyyy/MM/dd HH:mm"),
                    DepartmentName = h.DepartmentName ?? "-",
                    PerformedBy = h.PerformedByUserName ?? "سیستم",
                    DoctorName = h.Doctor?.FullName ?? "نامشخص",
                    Notes = h.Notes ?? "-"
                }).ToList();

                return Json(new { success = true, data = historyData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه به صورت AJAX");
                return Json(new { success = false, message = "خطا در دریافت تاریخچه" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Private Helper Methods (متدهای کمکی خصوصی)

        /// <summary>
        /// <summary>
        /// بارگذاری داده‌های مورد نیاز برای dropdown های جستجو
        /// </summary>
        private async Task<(List<System.Web.Mvc.SelectListItem> doctors, List<System.Web.Mvc.SelectListItem> departments)> LoadSearchDataAsync()
        {
            try
            {
                var doctors = new List<System.Web.Mvc.SelectListItem>();
                var departments = new List<System.Web.Mvc.SelectListItem>();

                // دریافت لیست پزشکان فعال
                var doctorsResult = await _doctorService.GetDoctorsAsync(new DoctorSearchViewModel { PageSize = 1000 });
                if (doctorsResult.Success)
                {
                    doctors = doctorsResult.Data.Items
                        .Where(d => d.IsActive)
                        .Select(d => new System.Web.Mvc.SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = d.FullName
                        })
                        .ToList();
                }
                else
                {
                    _logger.Warning("خطا در دریافت لیست پزشکان: {Message}", doctorsResult.Message);
                }

                // دریافت لیست دپارتمان‌ها
                var departmentsResult = await _departmentService.GetActiveDepartmentsForLookupAsync(1); // clinicId = 1 for now
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

                return (doctors, departments);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری داده‌های جستجو");
                return (new List<System.Web.Mvc.SelectListItem>(), new List<System.Web.Mvc.SelectListItem>());
            }
        }

        /// <summary>
        /// ایجاد ViewModel خالی برای جستجو
        /// </summary>
        private DoctorHistorySearchViewModel CreateEmptySearchViewModel(List<System.Web.Mvc.SelectListItem> doctors, List<System.Web.Mvc.SelectListItem> departments)
        {
            return new DoctorHistorySearchViewModel
            {
                Filter = new DoctorHistoryFilterViewModel(),
                SearchResults = new List<AssignmentHistoryViewModel>(),
                Histories = new List<AssignmentHistoryViewModel>(),
                ActionTypes = GetActionTypeOptions(),
                Doctors = doctors,
                Departments = departments
            };
        }

        /// <summary>
        /// ایجاد ViewModel خطا برای جستجو
        /// </summary>
        private DoctorHistorySearchViewModel CreateErrorSearchViewModel(string searchTerm, int? doctorId, string actionType,
            DateTime? startDate, DateTime? endDate, string performedBy, int? departmentId, string importance,
            List<System.Web.Mvc.SelectListItem> doctors, List<System.Web.Mvc.SelectListItem> departments)
        {
            return new DoctorHistorySearchViewModel
            {
                Filter = new DoctorHistoryFilterViewModel
                {
                    SearchTerm = searchTerm,
                    DoctorId = doctorId,
                    ActionType = actionType,
                    StartDate = startDate,
                    EndDate = endDate,
                    PerformedBy = performedBy,
                    DepartmentId = departmentId,
                    Importance = importance
                },
                SearchResults = new List<AssignmentHistoryViewModel>(),
                Histories = new List<AssignmentHistoryViewModel>(),
                ActionTypes = GetActionTypeOptions(),
                Doctors = doctors,
                Departments = departments
            };
        }

        /// <summary>
        /// ایجاد ViewModel کامل برای جستجو
        /// </summary>
        private DoctorHistorySearchViewModel CreateSearchViewModel(List<DoctorAssignmentHistory> data,
            string searchTerm, int? doctorId, string actionType, DateTime? startDate, DateTime? endDate,
            string performedBy, int? departmentId, string importance, int page, int pageSize,
            List<System.Web.Mvc.SelectListItem> doctors, List<System.Web.Mvc.SelectListItem> departments)
        {
            var histories = data.Select(h => new AssignmentHistoryViewModel
            {
                Id = h.Id,
                ActionType = h.ActionType,
                ActionTitle = h.ActionTitle,
                ActionDescription = h.ActionDescription,
                ActionDate = h.ActionDate,
                DepartmentName = h.DepartmentName,
                ServiceCategories = h.ServiceCategories,
                PerformedBy = h.PerformedByUserName,
                PerformedByUserName = h.PerformedByUserName,
                Notes = h.Notes,
                DoctorId = h.DoctorId,
                DoctorName = h.Doctor?.FullName ?? "نامشخص",
                DoctorNationalCode = h.Doctor?.NationalCode ?? "نامشخص",
                DoctorSpecialization = h.Doctor?.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "نامشخص",
                DepartmentId = h.DepartmentId,
                PerformedByUserId = h.PerformedByUserId,
                IsActive = !h.IsDeleted,
                IsDeleted = h.IsDeleted,
                CreatedAt = h.CreatedAt,
                CreatedDate = h.CreatedAt,
                UpdatedAt = h.UpdatedAt,
                UpdatedDate = h.UpdatedAt,
                Importance = h.Importance.ToString(),
                ClinicName = h.Department?.Clinic?.Name ?? "نامشخص"
            }).ToList();

            return new DoctorHistorySearchViewModel
            {
                SearchTerm = searchTerm,
                DoctorId = doctorId,
                ActionType = actionType,
                StartDate = startDate,
                EndDate = endDate,
                PerformedBy = performedBy,
                CurrentPage = page,
                PageSize = pageSize,
                Histories = histories,
                SearchResults = histories,
                Filter = new DoctorHistoryFilterViewModel
                {
                    SearchTerm = searchTerm,
                    DoctorId = doctorId,
                    ActionType = actionType,
                    StartDate = startDate,
                    EndDate = endDate,
                    PerformedBy = performedBy,
                    DepartmentId = departmentId,
                    Importance = importance
                },
                ActionTypes = GetActionTypeOptions(),
                Doctors = doctors,
                Departments = departments
            };
        }

        /// <summary>
        /// دریافت گزینه‌های نوع عملیات
        /// </summary>
        private System.Web.Mvc.SelectListItem[] GetActionTypeOptions()
        {
            return new[]
            {
                new System.Web.Mvc.SelectListItem { Text = "همه عملیات", Value = "" },
                new System.Web.Mvc.SelectListItem { Text = "انتساب به دپارتمان", Value = "AssignToDepartment" },
                new System.Web.Mvc.SelectListItem { Text = "انتساب سرفصل‌های خدماتی", Value = "AssignServiceCategories" },
                new System.Web.Mvc.SelectListItem { Text = "انتقال", Value = "Transfer" },
                new System.Web.Mvc.SelectListItem { Text = "حذف", Value = "Removal" },
                new System.Web.Mvc.SelectListItem { Text = "ویرایش", Value = "Update" }
            };
        }

        /// <summary>
        /// دریافت تاریخچه فیلتر شده
        /// </summary>
        private async Task<ServiceResult<List<DoctorAssignmentHistory>>> GetFilteredHistoryAsync(
            int? doctorId, string actionType, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            if (doctorId.HasValue)
            {
                return await _historyService.GetDoctorHistoryAsync(doctorId.Value, page, pageSize);
            }
            else if (!string.IsNullOrEmpty(actionType))
            {
                return await _historyService.GetHistoryByActionTypeAsync(actionType, startDate, endDate, page, pageSize);
            }
            else
            {
                return await _historyService.GetAllAsync();
            }
        }

        #endregion
    }
}
