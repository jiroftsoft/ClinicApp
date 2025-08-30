using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.Repositories.ClinicAdmin;
using ClinicApp.Services;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// پیاده‌سازی سرویس برای مدیریت تاریخچه انتسابات پزشکان
    /// این کلاس برای عملیات تجاری و منطق کسب‌وکار طراحی شده است
    /// </summary>
    public class DoctorAssignmentHistoryService : IDoctorAssignmentHistoryService
    {
        private readonly IDoctorAssignmentHistoryRepository _historyRepository;
        private readonly ICurrentUserService _currentUserService;

        public DoctorAssignmentHistoryService(
            IDoctorAssignmentHistoryRepository historyRepository,
            ICurrentUserService currentUserService)
        {
            _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Core CRUD Operations (عملیات اصلی CRUD)

        /// <summary>
        /// دریافت تاریخچه انتساب بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<DoctorAssignmentHistory>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return ServiceResult<DoctorAssignmentHistory>.Failed("شناسه تاریخچه نامعتبر است.");

                var history = await _historyRepository.GetByIdAsync(id);
                if (history == null)
                    return ServiceResult<DoctorAssignmentHistory>.Failed("تاریخچه مورد نظر یافت نشد.");

                return ServiceResult<DoctorAssignmentHistory>.Successful(history);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتساب با شناسه {Id}", id);
                return ServiceResult<DoctorAssignmentHistory>.Failed("خطا در دریافت تاریخچه انتساب.");
            }
        }

        /// <summary>
        /// دریافت تمام تاریخچه‌های انتساب
        /// </summary>
        public async Task<ServiceResult<List<DoctorAssignmentHistory>>> GetAllAsync()
        {
            try
            {
                var histories = await _historyRepository.GetAllAsync();
                return ServiceResult<List<DoctorAssignmentHistory>>.Successful(histories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تمام تاریخچه‌های انتساب");
                return ServiceResult<List<DoctorAssignmentHistory>>.Failed("خطا در دریافت تاریخچه‌های انتساب.");
            }
        }

        /// <summary>
        /// افزودن تاریخچه انتساب جدید
        /// </summary>
        public async Task<ServiceResult<DoctorAssignmentHistory>> AddAsync(DoctorAssignmentHistory history)
        {
            try
            {
                if (history == null)
                    return ServiceResult<DoctorAssignmentHistory>.Failed("داده‌های تاریخچه خالی است.");

                // اعتبارسنجی داده‌ها
                var validationResult = ValidateHistoryData(history);
                if (!validationResult.Success)
                    return ServiceResult<DoctorAssignmentHistory>.Failed(validationResult.Message);

                // تنظیم مقادیر پیش‌فرض
                history.ActionDate = DateTime.Now;
                history.PerformedByUserId = _currentUserService.GetCurrentUserId();
                history.PerformedByUserName = _currentUserService.GetCurrentUserName();

                var addedHistory = await _historyRepository.AddAsync(history);
                return ServiceResult<DoctorAssignmentHistory>.Successful(addedHistory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در افزودن تاریخچه انتساب جدید");
                return ServiceResult<DoctorAssignmentHistory>.Failed("خطا در افزودن تاریخچه انتساب.");
            }
        }

        /// <summary>
        /// به‌روزرسانی تاریخچه انتساب
        /// </summary>
        public async Task<ServiceResult<DoctorAssignmentHistory>> UpdateAsync(DoctorAssignmentHistory history)
        {
            try
            {
                if (history == null)
                    return ServiceResult<DoctorAssignmentHistory>.Failed("داده‌های تاریخچه خالی است.");

                if (history.Id <= 0)
                    return ServiceResult<DoctorAssignmentHistory>.Failed("شناسه تاریخچه نامعتبر است.");

                // اعتبارسنجی داده‌ها
                var validationResult = ValidateHistoryData(history);
                if (!validationResult.Success)
                    return ServiceResult<DoctorAssignmentHistory>.Failed(validationResult.Message);

                var updatedHistory = await _historyRepository.UpdateAsync(history);
                return ServiceResult<DoctorAssignmentHistory>.Successful(updatedHistory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در به‌روزرسانی تاریخچه انتساب با شناسه {Id}", history?.Id);
                return ServiceResult<DoctorAssignmentHistory>.Failed("خطا در به‌روزرسانی تاریخچه انتساب.");
            }
        }

        /// <summary>
        /// حذف نرم تاریخچه انتساب
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return ServiceResult<bool>.Failed("شناسه تاریخچه نامعتبر است.");

                var result = await _historyRepository.DeleteAsync(id);
                if (!result)
                    return ServiceResult<bool>.Failed("تاریخچه مورد نظر یافت نشد.");

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در حذف تاریخچه انتساب با شناسه {Id}", id);
                return ServiceResult<bool>.Failed("خطا در حذف تاریخچه انتساب.");
            }
        }

        #endregion

        #region Specialized Operations (عملیات تخصصی)

        /// <summary>
        /// دریافت تاریخچه انتسابات یک پزشک خاص
        /// </summary>
        public async Task<ServiceResult<List<DoctorAssignmentHistory>>> GetDoctorHistoryAsync(int doctorId, int page = 1, int pageSize = 20)
        {
            try
            {
                if (doctorId <= 0)
                    return ServiceResult<List<DoctorAssignmentHistory>>.Failed("شناسه پزشک نامعتبر است.");

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var histories = await _historyRepository.GetDoctorHistoryAsync(doctorId, page, pageSize);
                return ServiceResult<List<DoctorAssignmentHistory>>.Successful(histories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتسابات پزشک {DoctorId}", doctorId);
                return ServiceResult<List<DoctorAssignmentHistory>>.Failed("خطا در دریافت تاریخچه انتسابات پزشک.");
            }
        }

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس نوع عملیات
        /// </summary>
        public async Task<ServiceResult<List<DoctorAssignmentHistory>>> GetHistoryByActionTypeAsync(string actionType, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrEmpty(actionType))
                    return ServiceResult<List<DoctorAssignmentHistory>>.Failed("نوع عملیات الزامی است.");

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var histories = await _historyRepository.GetHistoryByActionTypeAsync(actionType, startDate, endDate, page, pageSize);
                return ServiceResult<List<DoctorAssignmentHistory>>.Successful(histories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتسابات بر اساس نوع عملیات {ActionType}", actionType);
                return ServiceResult<List<DoctorAssignmentHistory>>.Failed("خطا در دریافت تاریخچه انتسابات.");
            }
        }

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس سطح اهمیت
        /// </summary>
        public async Task<ServiceResult<List<DoctorAssignmentHistory>>> GetHistoryByImportanceAsync(AssignmentHistoryImportance importance, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var histories = await _historyRepository.GetHistoryByImportanceAsync(importance, startDate, endDate, page, pageSize);
                return ServiceResult<List<DoctorAssignmentHistory>>.Successful(histories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتسابات بر اساس سطح اهمیت {Importance}", importance);
                return ServiceResult<List<DoctorAssignmentHistory>>.Failed("خطا در دریافت تاریخچه انتسابات.");
            }
        }

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس دپارتمان
        /// </summary>
        public async Task<ServiceResult<List<DoctorAssignmentHistory>>> GetHistoryByDepartmentAsync(int departmentId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                if (departmentId <= 0)
                    return ServiceResult<List<DoctorAssignmentHistory>>.Failed("شناسه دپارتمان نامعتبر است.");

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var histories = await _historyRepository.GetHistoryByDepartmentAsync(departmentId, startDate, endDate, page, pageSize);
                return ServiceResult<List<DoctorAssignmentHistory>>.Successful(histories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتسابات بر اساس دپارتمان {DepartmentId}", departmentId);
                return ServiceResult<List<DoctorAssignmentHistory>>.Failed("خطا در دریافت تاریخچه انتسابات.");
            }
        }

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس کاربر انجام‌دهنده
        /// </summary>
        public async Task<ServiceResult<List<DoctorAssignmentHistory>>> GetHistoryByPerformedByAsync(string performedByUserId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrEmpty(performedByUserId))
                    return ServiceResult<List<DoctorAssignmentHistory>>.Failed("شناسه کاربر انجام‌دهنده الزامی است.");

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var histories = await _historyRepository.GetHistoryByPerformedByAsync(performedByUserId, startDate, endDate, page, pageSize);
                return ServiceResult<List<DoctorAssignmentHistory>>.Successful(histories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتسابات بر اساس کاربر انجام‌دهنده {PerformedByUserId}", performedByUserId);
                return ServiceResult<List<DoctorAssignmentHistory>>.Failed("خطا در دریافت تاریخچه انتسابات.");
            }
        }

        /// <summary>
        /// جستجوی پیشرفته در تاریخچه انتسابات
        /// </summary>
        public async Task<ServiceResult<List<DoctorAssignmentHistory>>> SearchHistoryAsync(string searchTerm = null, string actionType = null, AssignmentHistoryImportance? importance = null, int? departmentId = null, string performedByUserId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var histories = await _historyRepository.SearchHistoryAsync(searchTerm, actionType, importance, departmentId, performedByUserId, startDate, endDate, page, pageSize);
                return ServiceResult<List<DoctorAssignmentHistory>>.Successful(histories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در جستجوی پیشرفته تاریخچه انتسابات");
                return ServiceResult<List<DoctorAssignmentHistory>>.Failed("خطا در جستجوی تاریخچه انتسابات.");
            }
        }

        #endregion

        #region Statistics & Reporting (آمار و گزارش‌گیری)

        /// <summary>
        /// دریافت آمار تاریخچه انتسابات
        /// </summary>
        public async Task<ServiceResult<HistoryStats>> GetHistoryStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var stats = await _historyRepository.GetHistoryStatsAsync(startDate, endDate);
                return ServiceResult<HistoryStats>.Successful(stats);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت آمار تاریخچه انتسابات");
                return ServiceResult<HistoryStats>.Failed("خطا در دریافت آمار تاریخچه انتسابات.");
            }
        }

        /// <summary>
        /// دریافت آمار تاریخچه برای داشبورد
        /// </summary>
        public async Task<ServiceResult<DashboardHistoryStats>> GetDashboardStatsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                var thisMonthStart = new DateTime(today.Year, today.Month, 1);

                var stats = await _historyRepository.GetHistoryStatsAsync();
                var todayStats = await _historyRepository.GetHistoryStatsAsync(today, today.AddDays(1));
                var thisWeekStats = await _historyRepository.GetHistoryStatsAsync(thisWeekStart, today.AddDays(1));
                var thisMonthStats = await _historyRepository.GetHistoryStatsAsync(thisMonthStart, today.AddDays(1));

                var dashboardStats = new DashboardHistoryStats
                {
                    TotalRecords = stats.TotalRecords,
                    TodayRecords = todayStats.TotalRecords,
                    ThisWeekRecords = thisWeekStats.TotalRecords,
                    ThisMonthRecords = thisMonthStats.TotalRecords,
                    CriticalRecords = stats.CriticalRecords,
                    ImportantRecords = stats.ImportantRecords,
                    ActionTypeCounts = stats.ActionTypeCounts,
                    DepartmentCounts = stats.DepartmentCounts
                };

                // دریافت فعالیت‌های اخیر
                var recentActivities = await _historyRepository.SearchHistoryAsync(pageSize: 10);
                dashboardStats.RecentActivities = recentActivities.Select(h => new RecentActivity
                {
                    Id = h.Id,
                    ActionTitle = h.ActionTitle,
                    DoctorName = h.Doctor?.FirstName + " " + h.Doctor?.LastName,
                    DepartmentName = h.DepartmentName,
                    PerformedByUserName = h.PerformedByUserName,
                    ActionDate = h.ActionDate,
                    Importance = h.Importance
                }).ToList();

                return ServiceResult<DashboardHistoryStats>.Successful(dashboardStats);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت آمار داشبورد تاریخچه انتسابات");
                return ServiceResult<DashboardHistoryStats>.Failed("خطا در دریافت آمار داشبورد.");
            }
        }

        /// <summary>
        /// دریافت گزارش تغییرات بحرانی
        /// </summary>
        public async Task<ServiceResult<List<DoctorAssignmentHistory>>> GetCriticalChangesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var criticalHistories = await _historyRepository.GetHistoryByImportanceAsync(AssignmentHistoryImportance.Critical, startDate, endDate, 1, 50);
                return ServiceResult<List<DoctorAssignmentHistory>>.Successful(criticalHistories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت گزارش تغییرات بحرانی");
                return ServiceResult<List<DoctorAssignmentHistory>>.Failed("خطا در دریافت گزارش تغییرات بحرانی.");
            }
        }

        #endregion

        #region Maintenance Operations (عملیات نگهداری)

        /// <summary>
        /// پاک‌سازی تاریخچه قدیمی
        /// </summary>
        public async Task<ServiceResult<int>> CleanupOldHistoryAsync(int olderThanDays = 365)
        {
            try
            {
                if (olderThanDays < 30)
                    return ServiceResult<int>.Failed("حداقل 30 روز برای پاک‌سازی تاریخچه قدیمی الزامی است.");

                var deletedCount = await _historyRepository.CleanupOldHistoryAsync(olderThanDays);
                return ServiceResult<int>.Successful(deletedCount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در پاک‌سازی تاریخچه قدیمی");
                return ServiceResult<int>.Failed("خطا در پاک‌سازی تاریخچه قدیمی.");
            }
        }

        /// <summary>
        /// آرشیو تاریخچه‌های قدیمی
        /// </summary>
        public async Task<ServiceResult<bool>> ArchiveOldHistoryAsync(DateTime cutoffDate)
        {
            try
            {
                // این متد در آینده پیاده‌سازی خواهد شد
                // فعلاً فقط یک پیام موفقیت برمی‌گرداند
                Log.Information("آرشیو تاریخچه‌های قدیمی از تاریخ {CutoffDate} درخواست شد", cutoffDate);
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در آرشیو تاریخچه‌های قدیمی");
                return ServiceResult<bool>.Failed("خطا در آرشیو تاریخچه‌های قدیمی.");
            }
        }

        #endregion

        #region Business Logic Operations (عملیات منطق کسب‌وکار)

        /// <summary>
        /// ثبت خودکار تاریخچه برای عملیات انتساب
        /// </summary>
        public async Task<ServiceResult<bool>> LogAssignmentOperationAsync(int doctorId, string actionType, string actionTitle, string actionDescription, int? departmentId = null, string serviceCategories = null, string notes = null, AssignmentHistoryImportance importance = AssignmentHistoryImportance.Normal)
        {
            try
            {
                var history = new DoctorAssignmentHistory
                {
                    DoctorId = doctorId,
                    ActionType = actionType,
                    ActionTitle = actionTitle,
                    ActionDescription = actionDescription,
                    DepartmentId = departmentId,
                    ServiceCategories = serviceCategories,
                    Notes = notes,
                    Importance = importance,
                    ActionDate = DateTime.Now,
                    PerformedByUserId = _currentUserService.GetCurrentUserId(),
                    PerformedByUserName = _currentUserService.GetCurrentUserName()
                };

                await _historyRepository.AddAsync(history);
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در ثبت خودکار تاریخچه عملیات انتساب");
                return ServiceResult<bool>.Failed("خطا در ثبت تاریخچه عملیات.");
            }
        }

        /// <summary>
        /// ثبت خودکار تاریخچه برای تغییرات پزشک
        /// </summary>
        public async Task<ServiceResult<bool>> LogDoctorChangeAsync(int doctorId, string actionType, string actionTitle, string previousData = null, string newData = null, string notes = null)
        {
            try
            {
                var history = new DoctorAssignmentHistory
                {
                    DoctorId = doctorId,
                    ActionType = actionType,
                    ActionTitle = actionTitle,
                    PreviousData = previousData,
                    NewData = newData,
                    Notes = notes,
                    Importance = AssignmentHistoryImportance.Important,
                    ActionDate = DateTime.Now,
                    PerformedByUserId = _currentUserService.GetCurrentUserId(),
                    PerformedByUserName = _currentUserService.GetCurrentUserName()
                };

                await _historyRepository.AddAsync(history);
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در ثبت خودکار تاریخچه تغییرات پزشک");
                return ServiceResult<bool>.Failed("خطا در ثبت تاریخچه تغییرات.");
            }
        }

        /// <summary>
        /// ثبت خودکار تاریخچه برای تغییرات دپارتمان
        /// </summary>
        public async Task<ServiceResult<bool>> LogDepartmentChangeAsync(int doctorId, int departmentId, string actionType, string actionTitle, string previousData = null, string newData = null, string notes = null)
        {
            try
            {
                var history = new DoctorAssignmentHistory
                {
                    DoctorId = doctorId,
                    DepartmentId = departmentId,
                    ActionType = actionType,
                    ActionTitle = actionTitle,
                    PreviousData = previousData,
                    NewData = newData,
                    Notes = notes,
                    Importance = AssignmentHistoryImportance.Important,
                    ActionDate = DateTime.Now,
                    PerformedByUserId = _currentUserService.GetCurrentUserId(),
                    PerformedByUserName = _currentUserService.GetCurrentUserName()
                };

                await _historyRepository.AddAsync(history);
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در ثبت خودکار تاریخچه تغییرات دپارتمان");
                return ServiceResult<bool>.Failed("خطا در ثبت تاریخچه تغییرات.");
            }
        }

        /// <summary>
        /// ثبت خودکار تاریخچه برای تغییرات صلاحیت‌های خدماتی
        /// </summary>
        public async Task<ServiceResult<bool>> LogServiceCategoryChangeAsync(int doctorId, string actionType, string actionTitle, string previousData = null, string newData = null, string notes = null)
        {
            try
            {
                var history = new DoctorAssignmentHistory
                {
                    DoctorId = doctorId,
                    ActionType = actionType,
                    ActionTitle = actionTitle,
                    PreviousData = previousData,
                    NewData = newData,
                    Notes = notes,
                    Importance = AssignmentHistoryImportance.Important,
                    ActionDate = DateTime.Now,
                    PerformedByUserId = _currentUserService.GetCurrentUserId(),
                    PerformedByUserName = _currentUserService.GetCurrentUserName()
                };

                await _historyRepository.AddAsync(history);
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در ثبت خودکار تاریخچه تغییرات صلاحیت‌های خدماتی");
                return ServiceResult<bool>.Failed("خطا در ثبت تاریخچه تغییرات.");
            }
        }

        #endregion

        #region Export Operations (عملیات خروجی)

        /// <summary>
        /// خروجی Excel تاریخچه انتسابات
        /// </summary>
        public async Task<ServiceResult<byte[]>> ExportToExcelAsync(string searchTerm = null, string actionType = null, AssignmentHistoryImportance? importance = null, int? departmentId = null, string performedByUserId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // این متد در آینده پیاده‌سازی خواهد شد
                // فعلاً فقط یک آرایه خالی برمی‌گرداند
                Log.Information("خروجی Excel تاریخچه انتسابات درخواست شد");
                return ServiceResult<byte[]>.Successful(new byte[0]);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در خروجی Excel تاریخچه انتسابات");
                return ServiceResult<byte[]>.Failed("خطا در خروجی Excel.");
            }
        }

        /// <summary>
        /// خروجی PDF تاریخچه انتسابات
        /// </summary>
        public async Task<ServiceResult<byte[]>> ExportToPdfAsync(string searchTerm = null, string actionType = null, AssignmentHistoryImportance? importance = null, int? departmentId = null, string performedByUserId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // این متد در آینده پیاده‌سازی خواهد شد
                // فعلاً فقط یک آرایه خالی برمی‌گرداند
                Log.Information("خروجی PDF تاریخچه انتسابات درخواست شد");
                return ServiceResult<byte[]>.Successful(new byte[0]);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در خروجی PDF تاریخچه انتسابات");
                return ServiceResult<byte[]>.Failed("خطا در خروجی PDF.");
            }
        }

        #endregion

        #region Private Helper Methods (متدهای کمکی خصوصی)

        /// <summary>
        /// اعتبارسنجی داده‌های تاریخچه
        /// </summary>
        private ServiceResult<bool> ValidateHistoryData(DoctorAssignmentHistory history)
        {
            if (history.DoctorId <= 0)
                return ServiceResult<bool>.Failed("شناسه پزشک الزامی است.");

            if (string.IsNullOrEmpty(history.ActionType))
                return ServiceResult<bool>.Failed("نوع عملیات الزامی است.");

            if (string.IsNullOrEmpty(history.ActionTitle))
                return ServiceResult<bool>.Failed("عنوان عملیات الزامی است.");

            if (history.ActionTitle.Length > 200)
                return ServiceResult<bool>.Failed("عنوان عملیات نمی‌تواند بیش از 200 کاراکتر باشد.");

            if (!string.IsNullOrEmpty(history.ActionDescription) && history.ActionDescription.Length > 1000)
                return ServiceResult<bool>.Failed("توضیحات عملیات نمی‌تواند بیش از 1000 کاراکتر باشد.");

            return ServiceResult<bool>.Successful(true);
        }

        #endregion
    }
}
