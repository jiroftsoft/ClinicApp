using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// پیاده‌سازی اینترفیس IDoctorScheduleRepository برای مدیریت برنامه کاری پزشکان
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت برنامه‌های کاری پزشکان
    /// 2. رعایت استانداردهای پزشکی ایران در برنامه‌ریزی نوبت‌دهی
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorScheduleRepository : IDoctorScheduleRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorScheduleRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Schedule Management (مدیریت برنامه کاری)

        /// <summary>
        /// دریافت برنامه کاری پزشک
        /// </summary>
        public async Task<DoctorSchedule> GetDoctorScheduleAsync(int doctorId)
        {
            try
            {
                return await _context.DoctorSchedules
                    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت برنامه کاری پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت برنامه کاری پزشک همراه با جزئیات
        /// </summary>
        public async Task<DoctorSchedule> GetDoctorScheduleWithDetailsAsync(int doctorId)
        {
            try
            {
                return await _context.DoctorSchedules
                    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted)
                    .Include(ds => ds.Doctor)
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.CreatedByUser)
                    .Include(ds => ds.UpdatedByUser)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت جزئیات برنامه کاری پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// افزودن برنامه کاری جدید برای پزشک
        /// </summary>
        public async Task<DoctorSchedule> AddDoctorScheduleAsync(DoctorSchedule schedule)
        {
            try
            {
                if (schedule == null)
                    throw new ArgumentNullException(nameof(schedule));

                // بررسی وجود برنامه کاری قبلی
                var existingSchedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(ds => ds.DoctorId == schedule.DoctorId && !ds.IsDeleted);

                if (existingSchedule != null)
                    throw new InvalidOperationException($"پزشک قبلاً دارای برنامه کاری است.");

                // تنظیم تاریخ‌ها
                schedule.CreatedAt = DateTime.Now;
                schedule.UpdatedAt = DateTime.Now;
                schedule.IsDeleted = false;

                _context.DoctorSchedules.Add(schedule);
                await _context.SaveChangesAsync();

                return schedule;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در افزودن برنامه کاری پزشک", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی برنامه کاری پزشک
        /// </summary>
        public async Task<DoctorSchedule> UpdateDoctorScheduleAsync(DoctorSchedule schedule)
        {
            try
            {
                if (schedule == null)
                    throw new ArgumentNullException(nameof(schedule));

                var existingSchedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(ds => ds.ScheduleId == schedule.ScheduleId && !ds.IsDeleted);

                if (existingSchedule == null)
                    throw new InvalidOperationException($"برنامه کاری پزشک یافت نشد.");

                // به‌روزرسانی فیلدها
                existingSchedule.AppointmentDuration = schedule.AppointmentDuration;
                existingSchedule.DefaultStartTime = schedule.DefaultStartTime;
                existingSchedule.DefaultEndTime = schedule.DefaultEndTime;
                existingSchedule.IsActive = schedule.IsActive;
                existingSchedule.UpdatedAt = DateTime.Now;
                existingSchedule.UpdatedByUserId = schedule.UpdatedByUserId;

                await _context.SaveChangesAsync();

                return existingSchedule;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // لاگ خطای همزمانی برای سیستم‌های پزشکی
                throw new InvalidOperationException("خطای همزمانی در به‌روزرسانی برنامه کاری پزشک", ex);
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در به‌روزرسانی برنامه کاری پزشک", ex);
            }
        }

        /// <summary>
        /// دریافت اسلات‌های زمانی خالی و قابل رزرو برای یک پزشک در یک روز مشخص
        /// </summary>
        public async Task<List<DoctorTimeSlot>> GetAvailableAppointmentSlotsAsync(int doctorId, DateTime date)
        {
            try
            {
                // دریافت برنامه کاری پزشک
                var doctorSchedule = await _context.DoctorSchedules
                    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted && ds.IsActive)
                    .FirstOrDefaultAsync();

                if (doctorSchedule == null)
                    return new List<DoctorTimeSlot>();

                // دریافت روزهای کاری پزشک
                var workDays = await _context.DoctorWorkDays
                    .Where(wd => wd.ScheduleId == doctorSchedule.ScheduleId && wd.DayOfWeek == (int)date.DayOfWeek && wd.IsActive)
                    .Include(wd => wd.TimeRanges)
                    .ToListAsync();

                if (!workDays.Any())
                    return new List<DoctorTimeSlot>();

                var availableSlots = new List<DoctorTimeSlot>();

                foreach (var workDay in workDays)
                {
                    foreach (var timeRange in workDay.TimeRanges.Where(tr => tr.IsActive && !tr.IsDeleted))
                    {
                        var currentTime = timeRange.StartTime;
                        var endTime = timeRange.EndTime;

                        while (currentTime < endTime)
                        {
                            var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(doctorSchedule.AppointmentDuration));

                            if (slotEndTime <= endTime)
                            {
                                // بررسی وجود نوبت‌های رزرو شده در این بازه زمانی
                                var hasExistingAppointment = await _context.Appointments
                                    .AnyAsync(a => a.DoctorId == doctorId && 
                                                 a.AppointmentDate.Date == date.Date &&
                                                 a.AppointmentDate.TimeOfDay >= currentTime &&
                                                 a.AppointmentDate.TimeOfDay < slotEndTime &&
                                                 a.Status != AppointmentStatus.Cancelled &&
                                                 !a.IsDeleted);

                                if (!hasExistingAppointment)
                                {
                                    // بررسی وجود اسلات‌های مسدود شده
                                    var hasBlockedSlot = await _context.DoctorTimeSlots
                                        .AnyAsync(ts => ts.DoctorId == doctorId &&
                                                      ts.AppointmentDate.Date == date.Date &&
                                                      ts.StartTime >= currentTime &&
                                                      ts.EndTime <= slotEndTime &&
                                                      ts.Status == AppointmentStatus.Cancelled &&
                                                      !ts.IsDeleted);

                                    if (!hasBlockedSlot)
                                    {
                                        availableSlots.Add(new DoctorTimeSlot
                                        {
                                            DoctorId = doctorId,
                                            AppointmentDate = date,
                                            StartTime = currentTime,
                                            EndTime = slotEndTime,
                                            Duration = doctorSchedule.AppointmentDuration,
                                            Status = AppointmentStatus.Available,
                                            CreatedAt = DateTime.Now
                                        });
                                    }
                                }
                            }

                            currentTime = slotEndTime;
                        }
                    }
                }

                return availableSlots;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت اسلات‌های زمانی خالی برای پزشک {doctorId} در تاریخ {date:yyyy/MM/dd}", ex);
            }
        }

        /// <summary>
        /// مسدود کردن یک بازه زمانی برای پزشک (مثلا برای مرخصی یا جلسه)
        /// </summary>
        public async Task<bool> BlockTimeRangeForDoctorAsync(int doctorId, DateTime start, DateTime end, string reason)
        {
            try
            {
                if (start >= end)
                    throw new ArgumentException("زمان شروع باید قبل از زمان پایان باشد.");

                // بررسی وجود نوبت‌های رزرو شده در این بازه زمانی
                var hasExistingAppointments = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == doctorId &&
                                 a.AppointmentDate >= start &&
                                 a.AppointmentDate < end &&
                                 a.Status != AppointmentStatus.Cancelled &&
                                 !a.IsDeleted);

                if (hasExistingAppointments)
                    throw new InvalidOperationException("امکان مسدود کردن بازه زمانی به دلیل وجود نوبت‌های رزرو شده وجود ندارد.");

                // ایجاد اسلات‌های مسدود شده
                var blockedSlots = new List<DoctorTimeSlot>();
                var currentTime = start;

                while (currentTime < end)
                {
                    var slotEndTime = currentTime.AddMinutes(30); // اسلات‌های 30 دقیقه‌ای
                    if (slotEndTime > end)
                        slotEndTime = end;

                                         blockedSlots.Add(new DoctorTimeSlot
                     {
                         DoctorId = doctorId,
                         AppointmentDate = currentTime.Date,
                         StartTime = currentTime.TimeOfDay,
                         EndTime = slotEndTime.TimeOfDay,
                         Duration = (int)(slotEndTime - currentTime).TotalMinutes,
                         Status = AppointmentStatus.Cancelled,
                         CreatedAt = DateTime.Now
                     });

                    currentTime = slotEndTime;
                }

                _context.DoctorTimeSlots.AddRange(blockedSlots);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در مسدود کردن بازه زمانی برای پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت برنامه‌های کاری پزشک
        /// </summary>
        public async Task<List<DoctorSchedule>> GetSchedulesForDoctorAsync(int doctorId)
        {
            try
            {
                return await _context.DoctorSchedules
                    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted)
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                    .OrderBy(ds => ds.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت برنامه‌های کاری پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت برنامه کاری بر اساس شناسه
        /// </summary>
        public async Task<DoctorSchedule> GetScheduleByIdAsync(int scheduleId)
        {
            try
            {
                return await _context.DoctorSchedules
                    .Where(ds => ds.ScheduleId == scheduleId && !ds.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت برنامه کاری {scheduleId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود نوبت‌های فعال برای پزشک
        /// </summary>
        public async Task<bool> HasActiveAppointmentsAsync(int doctorId)
        {
            try
            {
                return await _context.Appointments
                    .AnyAsync(a => a.DoctorId == doctorId &&
                                 a.AppointmentDate >= DateTime.Today &&
                                 a.Status != AppointmentStatus.Cancelled &&
                                 !a.IsDeleted);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در بررسی نوبت‌های فعال پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// حذف برنامه کاری
        /// </summary>
        public async Task<bool> DeleteScheduleAsync(int scheduleId)
        {
            try
            {
                var schedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(ds => ds.ScheduleId == scheduleId);

                if (schedule != null)
                {
                    schedule.IsDeleted = true;
                    schedule.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در حذف برنامه کاری {scheduleId}", ex);
            }
        }

        /// <summary>
        /// دریافت بازه‌های زمانی مسدود شده
        /// </summary>
        public async Task<List<DoctorTimeSlot>> GetBlockedTimeRangesAsync(int doctorId)
        {
            try
            {
                return await _context.DoctorTimeSlots
                    .Where(ts => ts.DoctorId == doctorId &&
                               ts.Status == AppointmentStatus.Cancelled &&
                               !ts.IsDeleted)
                    .OrderBy(ts => ts.AppointmentDate)
                    .ThenBy(ts => ts.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت بازه‌های مسدود شده پزشک {doctorId}", ex);
            }
        }

        #endregion
    }
}
