using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using EntityFramework.DynamicFilters;
using Serilog;

namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// پیاده‌سازی اینترفیس IDoctorTimeSlotRepository برای مدیریت اسلات‌های زمانی پزشکان
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت اسلات‌های زمانی
    /// 2. رعایت استانداردهای پزشکی ایران در برنامه‌ریزی نوبت‌دهی
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorTimeSlotRepository : IDoctorTimeSlotRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public DoctorTimeSlotRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger?.ForContext<DoctorTimeSlotRepository>() ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Query Operations

        /// <summary>
        /// دریافت اسلات‌های زمانی با فیلتر و صفحه‌بندی
        /// </summary>
        public async Task<(List<DoctorTimeSlot> Items, int TotalCount)> GetTimeSlotsAsync(
            int? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            AppointmentStatus? status = null,
            int pageNumber = 1,
            int pageSize = 20,
            string searchTerm = null)
        {
            try
            {
                _logger.Information("درخواست دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, StartDate: {StartDate}, EndDate: {EndDate}, Status: {Status}, Page: {PageNumber}, PageSize: {PageSize}",
                    doctorId, startDate, endDate, status, pageNumber, pageSize);

                // ✅ اعتبارسنجی پارامترها
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100; // محدودیت برای جلوگیری از بارگذاری بیش از حد

                // ✅ ساخت Query پایه با AsNoTracking برای Read-Only
                var query = _context.DoctorTimeSlots
                    .AsNoTracking()
                    .Include(ts => ts.Doctor)
                    .Include(ts => ts.Appointment)
                    .Where(ts => !ts.IsDeleted)
                    .AsQueryable();

                // ✅ اعمال فیلتر DoctorId
                if (doctorId.HasValue && doctorId.Value > 0)
                {
                    query = query.Where(ts => ts.DoctorId == doctorId.Value);
                }

                // ✅ اعمال فیلتر تاریخ
                if (startDate.HasValue)
                {
                    var startDateOnly = startDate.Value.Date;
                    query = query.Where(ts => ts.AppointmentDate >= startDateOnly);
                }

                if (endDate.HasValue)
                {
                    var endDateOnly = endDate.Value.Date.AddDays(1); // شامل همان روز
                    query = query.Where(ts => ts.AppointmentDate < endDateOnly);
                }

                // ✅ اعمال فیلتر وضعیت
                if (status.HasValue)
                {
                    query = query.Where(ts => ts.Status == status.Value);
                }

                // ✅ اعمال جستجو (اگر ارائه شده باشد)
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var search = searchTerm.Trim();
                    query = query.Where(ts =>
                        (ts.Doctor != null && (ts.Doctor.FirstName.Contains(search) || ts.Doctor.LastName.Contains(search))) ||
                        (ts.Appointment != null && ts.Appointment.Patient != null && 
                         (ts.Appointment.Patient.FirstName.Contains(search) || ts.Appointment.Patient.LastName.Contains(search))));
                }

                // ✅ محاسبه تعداد کل
                var totalCount = await query.CountAsync();

                // ✅ اعمال صفحه‌بندی و مرتب‌سازی
                var items = await query
                    .OrderByDescending(ts => ts.AppointmentDate)
                    .ThenBy(ts => ts.StartTime)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.Information("اسلات‌های زمانی با موفقیت دریافت شدند - TotalItems: {TotalItems}, PageItems: {PageItems}",
                    totalCount, items.Count);

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های زمانی");
                throw new InvalidOperationException($"خطا در دریافت اسلات‌های زمانی: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت اسلات زمانی بر اساس شناسه
        /// </summary>
        public async Task<DoctorTimeSlot> GetTimeSlotByIdAsync(int timeSlotId)
        {
            try
            {
                _logger.Information("درخواست دریافت اسلات زمانی - TimeSlotId: {TimeSlotId}", timeSlotId);

                var timeSlot = await _context.DoctorTimeSlots
                    .AsNoTracking()
                    .Include(ts => ts.Doctor)
                    .Include(ts => ts.Appointment)
                    .Where(ts => ts.TimeSlotId == timeSlotId && !ts.IsDeleted)
                    .FirstOrDefaultAsync();

                // ✅ بارگذاری Patient به صورت جداگانه اگر Appointment وجود داشته باشد
                if (timeSlot?.Appointment != null)
                {
                    await _context.Entry(timeSlot.Appointment)
                        .Reference(a => a.Patient)
                        .LoadAsync();
                }

                if (timeSlot == null)
                {
                    _logger.Warning("اسلات زمانی {TimeSlotId} یافت نشد", timeSlotId);
                }
                else
                {
                    _logger.Information("اسلات زمانی {TimeSlotId} با موفقیت دریافت شد", timeSlotId);
                }

                return timeSlot;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات زمانی {TimeSlotId}", timeSlotId);
                throw new InvalidOperationException($"خطا در دریافت اسلات زمانی {timeSlotId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت اسلات‌های زمانی یک پزشک در یک تاریخ خاص
        /// </summary>
        public async Task<List<DoctorTimeSlot>> GetTimeSlotsByDoctorAndDateAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("درخواست دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ استفاده از DbFunctions.TruncateTime برای مقایسه تاریخ در LINQ to Entities
                var slots = await _context.DoctorTimeSlots
                    .AsNoTracking()
                    .Include(ts => ts.Doctor)
                    .Include(ts => ts.Appointment)
                    .Where(ts => ts.DoctorId == doctorId &&
                               DbFunctions.TruncateTime(ts.AppointmentDate) == DbFunctions.TruncateTime(date) &&
                               !ts.IsDeleted)
                    .OrderBy(ts => ts.StartTime)
                    .ToListAsync();

                _logger.Information("اسلات‌های زمانی با موفقیت دریافت شدند - Count: {Count}", slots.Count);

                return slots;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));
                throw new InvalidOperationException($"خطا در دریافت اسلات‌های زمانی پزشک {doctorId} در تاریخ {date:yyyy/MM/dd}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// دریافت آمار اسلات‌های زمانی
        /// </summary>
        public async Task<TimeSlotStatistics> GetTimeSlotStatisticsAsync(
            int? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                _logger.Information("درخواست دریافت آمار اسلات‌های زمانی - DoctorId: {DoctorId}, StartDate: {StartDate}, EndDate: {EndDate}",
                    doctorId, startDate, endDate);

                var query = _context.DoctorTimeSlots
                    .AsNoTracking()
                    .Where(ts => !ts.IsDeleted)
                    .AsQueryable();

                // ✅ اعمال فیلتر DoctorId
                if (doctorId.HasValue && doctorId.Value > 0)
                {
                    query = query.Where(ts => ts.DoctorId == doctorId.Value);
                }

                // ✅ اعمال فیلتر تاریخ
                if (startDate.HasValue)
                {
                    var startDateOnly = startDate.Value.Date;
                    query = query.Where(ts => ts.AppointmentDate >= startDateOnly);
                }

                if (endDate.HasValue)
                {
                    var endDateOnly = endDate.Value.Date.AddDays(1);
                    query = query.Where(ts => ts.AppointmentDate < endDateOnly);
                }

                var statistics = new TimeSlotStatistics
                {
                    TotalSlots = await query.CountAsync(),
                    AvailableSlots = await query.CountAsync(ts => ts.Status == AppointmentStatus.Available),
                    BookedSlots = await query.CountAsync(ts => ts.Status == AppointmentStatus.Scheduled || ts.Status == AppointmentStatus.Pending),
                    CompletedSlots = await query.CountAsync(ts => ts.Status == AppointmentStatus.Completed),
                    CancelledSlots = await query.CountAsync(ts => ts.Status == AppointmentStatus.Cancelled),
                    NoShowSlots = await query.CountAsync(ts => ts.Status == AppointmentStatus.NoShow),
                    DeletedSlots = await _context.DoctorTimeSlots.CountAsync(ts => ts.IsDeleted)
                };

                _logger.Information("آمار اسلات‌های زمانی با موفقیت دریافت شد - Total: {Total}, Available: {Available}, Booked: {Booked}",
                    statistics.TotalSlots, statistics.AvailableSlots, statistics.BookedSlots);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار اسلات‌های زمانی");
                throw new InvalidOperationException($"خطا در دریافت آمار اسلات‌های زمانی: {ex.Message}", ex);
            }
        }

        #endregion

        #region Management Operations

        /// <summary>
        /// حذف نرم اسلات زمانی
        /// </summary>
        public async Task<bool> SoftDeleteTimeSlotAsync(int timeSlotId, string deletedByUserId)
        {
            try
            {
                _logger.Information("درخواست حذف اسلات زمانی - TimeSlotId: {TimeSlotId}, DeletedBy: {DeletedBy}",
                    timeSlotId, deletedByUserId);

                var timeSlot = await _context.DoctorTimeSlots
                    .Where(ts => ts.TimeSlotId == timeSlotId && !ts.IsDeleted)
                    .FirstOrDefaultAsync();

                if (timeSlot == null)
                {
                    _logger.Warning("اسلات زمانی {TimeSlotId} یافت نشد", timeSlotId);
                    return false;
                }

                // ✅ بررسی اینکه آیا اسلات رزرو شده است یا نه
                if (timeSlot.Status == AppointmentStatus.Scheduled || timeSlot.Status == AppointmentStatus.Pending)
                {
                    _logger.Warning("اسلات زمانی {TimeSlotId} رزرو شده است و نمی‌تواند حذف شود", timeSlotId);
                    throw new InvalidOperationException("نمی‌توان اسلات رزرو شده را حذف کرد. ابتدا نوبت را لغو کنید.");
                }

                // ✅ Soft Delete
                timeSlot.IsDeleted = true;
                timeSlot.DeletedAt = DateTime.Now;
                timeSlot.DeletedByUserId = deletedByUserId;

                await _context.SaveChangesAsync();

                _logger.Information("اسلات زمانی {TimeSlotId} با موفقیت حذف شد (Soft Delete)", timeSlotId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اسلات زمانی {TimeSlotId}", timeSlotId);
                throw new InvalidOperationException($"خطا در حذف اسلات زمانی {timeSlotId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// تغییر وضعیت اسلات زمانی
        /// </summary>
        public async Task<bool> UpdateTimeSlotStatusAsync(int timeSlotId, AppointmentStatus status, string updatedByUserId)
        {
            try
            {
                _logger.Information("درخواست تغییر وضعیت اسلات زمانی - TimeSlotId: {TimeSlotId}, Status: {Status}, UpdatedBy: {UpdatedBy}",
                    timeSlotId, status, updatedByUserId);

                var timeSlot = await _context.DoctorTimeSlots
                    .Where(ts => ts.TimeSlotId == timeSlotId && !ts.IsDeleted)
                    .FirstOrDefaultAsync();

                if (timeSlot == null)
                {
                    _logger.Warning("اسلات زمانی {TimeSlotId} یافت نشد", timeSlotId);
                    return false;
                }

                timeSlot.Status = status;
                timeSlot.UpdatedAt = DateTime.Now;
                timeSlot.UpdatedByUserId = updatedByUserId;

                await _context.SaveChangesAsync();

                _logger.Information("وضعیت اسلات زمانی {TimeSlotId} به {Status} تغییر یافت", timeSlotId, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تغییر وضعیت اسلات زمانی {TimeSlotId}", timeSlotId);
                throw new InvalidOperationException($"خطا در تغییر وضعیت اسلات زمانی {timeSlotId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// آزاد کردن اسلات رزرو شده (برای لغو نوبت)
        /// </summary>
        public async Task<bool> ReleaseTimeSlotAsync(int timeSlotId, string updatedByUserId)
        {
            try
            {
                _logger.Information("درخواست آزاد کردن اسلات زمانی - TimeSlotId: {TimeSlotId}, UpdatedBy: {UpdatedBy}",
                    timeSlotId, updatedByUserId);

                var timeSlot = await _context.DoctorTimeSlots
                    .Where(ts => ts.TimeSlotId == timeSlotId && !ts.IsDeleted)
                    .FirstOrDefaultAsync();

                if (timeSlot == null)
                {
                    _logger.Warning("اسلات زمانی {TimeSlotId} یافت نشد", timeSlotId);
                    return false;
                }

                // ✅ آزاد کردن اسلات
                timeSlot.Status = AppointmentStatus.Available;
                timeSlot.AppointmentId = null;
                timeSlot.UpdatedAt = DateTime.Now;
                timeSlot.UpdatedByUserId = updatedByUserId;

                await _context.SaveChangesAsync();

                _logger.Information("اسلات زمانی {TimeSlotId} با موفقیت آزاد شد", timeSlotId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در آزاد کردن اسلات زمانی {TimeSlotId}", timeSlotId);
                throw new InvalidOperationException($"خطا در آزاد کردن اسلات زمانی {timeSlotId}: {ex.Message}", ex);
            }
        }

        #endregion
    }
}

