using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// پیاده‌سازی Repository برای مدیریت تاریخچه انتسابات پزشکان
    /// این کلاس برای عملیات CRUD و جستجو در تاریخچه انتسابات طراحی شده است
    /// </summary>
    public class DoctorAssignmentHistoryRepository : IDoctorAssignmentHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DoctorAssignmentHistoryRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Core CRUD Operations (عملیات اصلی CRUD)

        /// <summary>
        /// دریافت تاریخچه انتساب بر اساس شناسه
        /// </summary>
        public async Task<DoctorAssignmentHistory> GetByIdAsync(int id)
        {
            try
            {
                return await _context.DoctorAssignmentHistories
                    .Where(h => h.Id == id && !h.IsDeleted)
                    .Include(h => h.Doctor)
                    .Include(h => h.Department)
                    .Include(h => h.PerformedByUser)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتساب با شناسه {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// دریافت تمام تاریخچه‌های انتساب
        /// </summary>
        public async Task<List<DoctorAssignmentHistory>> GetAllAsync()
        {
            try
            {
                return await _context.DoctorAssignmentHistories
                    .Where(h => !h.IsDeleted)
                    .Include(h => h.Doctor)
                    .Include(h => h.Department)
                    .Include(h => h.PerformedByUser)
                    .OrderByDescending(h => h.ActionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تمام تاریخچه‌های انتساب");
                throw;
            }
        }

        /// <summary>
        /// افزودن تاریخچه انتساب جدید
        /// </summary>
        public async Task<DoctorAssignmentHistory> AddAsync(DoctorAssignmentHistory history)
        {
            try
            {
                // تنظیم فیلدهای ردیابی
                history.ActionDate = DateTime.Now;
                
                // اطمینان از اینکه PerformedByUserId هرگز null نباشد
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    // اگر کاربر فعلی در دسترس نیست، اولین کاربر موجود را پیدا کنیم
                    var firstUser = await _context.Users.FirstOrDefaultAsync();
                    if (firstUser != null)
                    {
                        currentUserId = firstUser.Id;
                        Log.Warning("شناسه کاربر فعلی در دسترس نیست. استفاده از اولین کاربر موجود: {FirstUserId}", currentUserId);
                    }
                    else
                    {
                        // اگر هیچ کاربری وجود ندارد، از یک شناسه پیش‌فرض استفاده کنیم
                        currentUserId = "6f999f4d-24b8-4142-a97e-20077850278b"; // شناسه کاربر شما
                        Log.Error("هیچ کاربری در دیتابیس یافت نشد. استفاده از شناسه پیش‌فرض: {DefaultUserId}", currentUserId);
                    }
                }
                history.PerformedByUserId = history.PerformedByUserId ?? currentUserId;
                
                history.IsDeleted = false;
                history.DeletedAt = null;
                history.DeletedByUserId = null;

                _context.DoctorAssignmentHistories.Add(history);
                await _context.SaveChangesAsync();
                return history;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در افزودن تاریخچه انتساب جدید");
                throw;
            }
        }

        /// <summary>
        /// به‌روزرسانی تاریخچه انتساب
        /// </summary>
        public async Task<DoctorAssignmentHistory> UpdateAsync(DoctorAssignmentHistory history)
        {
            try
            {
                _context.Entry(history).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return history;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در به‌روزرسانی تاریخچه انتساب با شناسه {Id}", history.Id);
                throw;
            }
        }

        /// <summary>
        /// حذف نرم تاریخچه انتساب
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var history = await _context.DoctorAssignmentHistories.FindAsync(id);
                if (history == null) return false;

                history.IsDeleted = true;
                history.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در حذف تاریخچه انتساب با شناسه {Id}", id);
                throw;
            }
        }

        #endregion

        /// <summary>
        /// دریافت تاریخچه انتسابات یک پزشک خاص
        /// </summary>
        public async Task<List<DoctorAssignmentHistory>> GetDoctorHistoryAsync(int doctorId, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.DoctorAssignmentHistories
                    .Where(h => h.DoctorId == doctorId && !h.IsDeleted)
                    .Include(h => h.Doctor)
                    .Include(h => h.Department)
                    .Include(h => h.PerformedByUser)
                    .OrderByDescending(h => h.ActionDate);

                var skip = (page - 1) * pageSize;
                return await query.Skip(skip).Take(pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتسابات پزشک {DoctorId}", doctorId);
                throw;
            }
        }

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس نوع عملیات
        /// </summary>
        public async Task<List<DoctorAssignmentHistory>> GetHistoryByActionTypeAsync(string actionType, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.DoctorAssignmentHistories
                    .Where(h => h.ActionType == actionType && !h.IsDeleted)
                    .Include(h => h.Doctor)
                    .Include(h => h.Department)
                    .Include(h => h.PerformedByUser);

                if (startDate.HasValue)
                    query = query.Where(h => h.ActionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(h => h.ActionDate <= endDate.Value);

                var skip = (page - 1) * pageSize;
                return await query.OrderByDescending(h => h.ActionDate).Skip(skip).Take(pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتسابات بر اساس نوع عملیات {ActionType}", actionType);
                throw;
            }
        }

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس سطح اهمیت
        /// </summary>
        public async Task<List<DoctorAssignmentHistory>> GetHistoryByImportanceAsync(AssignmentHistoryImportance importance, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.DoctorAssignmentHistories
                    .Where(h => h.Importance == importance && !h.IsDeleted)
                    .Include(h => h.Doctor)
                    .Include(h => h.Department)
                    .Include(h => h.PerformedByUser);

                if (startDate.HasValue)
                    query = query.Where(h => h.ActionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(h => h.ActionDate <= endDate.Value);

                var skip = (page - 1) * pageSize;
                return await query.OrderByDescending(h => h.ActionDate).Skip(skip).Take(pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتسابات بر اساس سطح اهمیت {Importance}", importance);
                throw;
            }
        }

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس دپارتمان
        /// </summary>
        public async Task<List<DoctorAssignmentHistory>> GetHistoryByDepartmentAsync(int departmentId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.DoctorAssignmentHistories
                    .Where(h => h.DepartmentId == departmentId && !h.IsDeleted)
                    .Include(h => h.Doctor)
                    .Include(h => h.Department)
                    .Include(h => h.PerformedByUser);

                if (startDate.HasValue)
                    query = query.Where(h => h.ActionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(h => h.ActionDate <= endDate.Value);

                var skip = (page - 1) * pageSize;
                return await query.OrderByDescending(h => h.ActionDate).Skip(skip).Take(pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتسابات بر اساس دپارتمان {DepartmentId}", departmentId);
                throw;
            }
        }

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس کاربر انجام‌دهنده
        /// </summary>
        public async Task<List<DoctorAssignmentHistory>> GetHistoryByPerformedByAsync(string performedByUserId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.DoctorAssignmentHistories
                    .Where(h => h.PerformedByUserId == performedByUserId && !h.IsDeleted)
                    .Include(h => h.Doctor)
                    .Include(h => h.Department)
                    .Include(h => h.PerformedByUser);

                if (startDate.HasValue)
                    query = query.Where(h => h.ActionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(h => h.ActionDate <= endDate.Value);

                var skip = (page - 1) * pageSize;
                return await query.OrderByDescending(h => h.ActionDate).Skip(skip).Take(pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت تاریخچه انتسابات بر اساس کاربر انجام‌دهنده {PerformedByUserId}", performedByUserId);
                throw;
            }
        }

        /// <summary>
        /// جستجوی پیشرفته در تاریخچه انتسابات
        /// </summary>
        public async Task<List<DoctorAssignmentHistory>> SearchHistoryAsync(string searchTerm = null, string actionType = null, AssignmentHistoryImportance? importance = null, int? departmentId = null, string performedByUserId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.DoctorAssignmentHistories
                    .Where(h => !h.IsDeleted)
                    .Include(h => h.Doctor)
                    .Include(h => h.Department)
                    .Include(h => h.PerformedByUser);

                // اعمال فیلترهای جستجو
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(h => 
                        h.ActionTitle.Contains(searchTerm) ||
                        h.ActionDescription.Contains(searchTerm) ||
                        h.DepartmentName.Contains(searchTerm) ||
                        h.PerformedByUserName.Contains(searchTerm) ||
                        h.Notes.Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(actionType))
                    query = query.Where(h => h.ActionType == actionType);

                if (importance.HasValue)
                    query = query.Where(h => h.Importance == importance.Value);

                if (departmentId.HasValue)
                    query = query.Where(h => h.DepartmentId == departmentId.Value);

                if (!string.IsNullOrEmpty(performedByUserId))
                    query = query.Where(h => h.PerformedByUserId == performedByUserId);

                if (startDate.HasValue)
                    query = query.Where(h => h.ActionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(h => h.ActionDate <= endDate.Value);

                var skip = (page - 1) * pageSize;
                return await query.OrderByDescending(h => h.ActionDate).Skip(skip).Take(pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در جستجوی پیشرفته تاریخچه انتسابات");
                throw;
            }
        }

        /// <summary>
        /// دریافت آمار تاریخچه انتسابات
        /// </summary>
        public async Task<HistoryStats> GetHistoryStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.DoctorAssignmentHistories.Where(h => !h.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(h => h.ActionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(h => h.ActionDate <= endDate.Value);

                var stats = new HistoryStats
                {
                    TotalRecords = await query.CountAsync(),
                    CriticalRecords = await query.CountAsync(h => h.Importance == AssignmentHistoryImportance.Critical),
                    ImportantRecords = await query.CountAsync(h => h.Importance == AssignmentHistoryImportance.Important),
                    NormalRecords = await query.CountAsync(h => h.Importance == AssignmentHistoryImportance.Normal),
                    SecurityRecords = await query.CountAsync(h => h.Importance == AssignmentHistoryImportance.Security)
                };

                // آمار بر اساس نوع عملیات
                var actionTypeStats = await query
                    .GroupBy(h => h.ActionType)
                    .Select(g => new { ActionType = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var stat in actionTypeStats)
                {
                    stats.ActionTypeCounts[stat.ActionType] = stat.Count;
                }

                // آمار بر اساس دپارتمان
                var departmentStats = await query
                    .Where(h => h.DepartmentId.HasValue)
                    .GroupBy(h => h.DepartmentName)
                    .Select(g => new { DepartmentName = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var stat in departmentStats)
                {
                    stats.DepartmentCounts[stat.DepartmentName] = stat.Count;
                }

                // آمار بر اساس کاربر
                var userStats = await query
                    .GroupBy(h => h.PerformedByUserName)
                    .Select(g => new { UserName = g.Key, Count = g.Count() })
                    .ToListAsync();

                foreach (var stat in userStats)
                {
                    stats.UserCounts[stat.UserName] = stat.Count;
                }

                return stats;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت آمار تاریخچه انتسابات");
                throw;
            }
        }

        /// <summary>
        /// پاک‌سازی تاریخچه قدیمی
        /// </summary>
        public async Task<int> CleanupOldHistoryAsync(int olderThanDays = 365)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
                var oldRecords = await _context.DoctorAssignmentHistories
                    .Where(h => h.ActionDate < cutoffDate && !h.IsDeleted)
                    .ToListAsync();

                foreach (var record in oldRecords)
                {
                    record.IsDeleted = true;
                    record.DeletedAt = DateTime.Now;
                    record.DeletedByUserId = "System"; // سیستم
                }

                await _context.SaveChangesAsync();
                Log.Information("تاریخچه قدیمی پاک‌سازی شد. تعداد رکوردهای پاک شده: {Count}", oldRecords.Count);
                return oldRecords.Count;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در پاک‌سازی تاریخچه قدیمی");
                throw;
            }
        }
    }
}
