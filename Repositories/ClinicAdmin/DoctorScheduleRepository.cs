using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using EntityFramework.DynamicFilters;

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
                // ✅ استفاده از AsNoTracking() برای جلوگیری از lazy loading Navigation Properties
                // ✅ این کار از خطای SQL "Invalid column name 'Doctor_DoctorId'" جلوگیری می‌کند
                return await _context.DoctorSchedules
                    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted)
                    .AsNoTracking() // ✅ جلوگیری از lazy loading
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت برنامه کاری پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت برنامه کاری پزشک همراه با جزئیات کامل (شامل داده‌های غیرفعال)
        /// این متد فیلترهای سراسری را دور می‌زند تا تمام داده‌ها را دریافت کند
        /// </summary>
        public async Task<DoctorSchedule> GetDoctorScheduleWithAllDetailsAsync(int doctorId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] 🔍 شروع دریافت برنامه کاری پزشک {doctorId}");
                
                // ✅ استفاده از query مستقیم برای دور زدن فیلترهای سراسری
                // ✅ توجه: در EF6 نمی‌توان از Where در Include استفاده کرد، بنابراین فیلتر در memory انجام می‌شود
                System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] 🔍 در حال اجرای Query برای پزشک {doctorId}");
                
                // ✅ حذف .Include(ds => ds.Doctor) به دلیل خطای SQL: Invalid column name 'Doctor_DoctorId'
                // ✅ Navigation Property Doctor در FromEntity استفاده می‌شود اما می‌تواند lazy load شود یا از DoctorId استفاده شود
                var result = await _context.DoctorSchedules
                    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted)
                    // .Include(ds => ds.Doctor) // ❌ حذف شده: باعث خطای SQL می‌شود
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                    .Include(ds => ds.CreatedByUser)
                    .Include(ds => ds.UpdatedByUser)
                    .AsNoTracking() // ✅ بهبود Performance برای read-only query
                    .FirstOrDefaultAsync();
                
                System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] ✅ Query اجرا شد. Result is null: {result == null}");
                
                // ✅ اطمینان از اینکه WorkDays و TimeRanges null نباشند و فیلتر شوند
                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] ✅ Result دریافت شد. ScheduleId: {result.ScheduleId}, WorkDaysCount (before filter): {result.WorkDays?.Count ?? 0}");
                    
                    // ✅ فیلتر کردن WorkDays و TimeRanges در memory (برای اطمینان کامل)
                    if (result.WorkDays != null)
                    {
                        // ✅ فیلتر WorkDays
                        var filteredWorkDays = result.WorkDays
                            .Where(wd => wd != null && !wd.IsDeleted)
                            .ToList();
                        
                        System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] 📅 WorkDays فیلتر شد. قبل: {result.WorkDays.Count}, بعد: {filteredWorkDays.Count}");
                        
                        // ✅ فیلتر TimeRanges برای هر WorkDay
                        foreach (var workDay in filteredWorkDays)
                        {
                            if (workDay.TimeRanges != null)
                            {
                                var beforeCount = workDay.TimeRanges.Count;
                                workDay.TimeRanges = workDay.TimeRanges
                                    .Where(tr => tr != null && !tr.IsDeleted)
                                    .ToList();
                                var afterCount = workDay.TimeRanges.Count;
                                
                                System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] ⏰ WorkDay {workDay.DayOfWeek}: TimeRanges فیلتر شد. قبل: {beforeCount}, بعد: {afterCount}");
                                
                                // ✅ لاگ جزئیات TimeRanges
                                foreach (var timeRange in workDay.TimeRanges)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] ⏰ TimeRange: StartTime={timeRange.StartTime}, EndTime={timeRange.EndTime}, IsDeleted={timeRange.IsDeleted}");
                                }
                            }
                            else
                            {
                                workDay.TimeRanges = new List<DoctorTimeRange>();
                                System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] ⏰ WorkDay {workDay.DayOfWeek}: TimeRanges null بود، لیست خالی ایجاد شد");
                            }
                        }
                        
                        result.WorkDays = filteredWorkDays;
                    }
                    else
                    {
                        result.WorkDays = new List<DoctorWorkDay>();
                        System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] 📅 WorkDays null بود، لیست خالی ایجاد شد");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] ❌ Result null است");
                }
                
                System.Diagnostics.Debug.WriteLine($"[GetDoctorScheduleWithAllDetailsAsync] ✅ بازگرداندن Result");
                return result;
            }
            catch (InvalidOperationException)
            {
                // ✅ Re-throw InvalidOperationException بدون تغییر
                throw;
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                // ✅ لاگ خطاهای Entity Framework با جزئیات بیشتر
                System.Diagnostics.Debug.WriteLine($"EntityException در GetDoctorScheduleWithAllDetailsAsync برای DoctorId={doctorId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                
                throw new InvalidOperationException($"خطا در اتصال به پایگاه داده برای دریافت برنامه کاری پزشک {doctorId}", ex);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                // ✅ لاگ خطاهای SQL با جزئیات بیشتر
                System.Diagnostics.Debug.WriteLine($"SqlException در GetDoctorScheduleWithAllDetailsAsync برای DoctorId={doctorId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ErrorNumber: {ex.Number}, LineNumber: {ex.LineNumber}");
                
                throw new InvalidOperationException($"خطا در اجرای درخواست پایگاه داده برای دریافت برنامه کاری پزشک {doctorId}", ex);
            }
            catch (Exception ex)
            {
                // ✅ لاگ خطا برای سیستم‌های پزشکی با جزئیات بیشتر
                System.Diagnostics.Debug.WriteLine($"خطا در GetDoctorScheduleWithAllDetailsAsync برای DoctorId={doctorId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ExceptionType: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"InnerExceptionType: {ex.InnerException.GetType().Name}");
                }
                
                throw new InvalidOperationException($"خطا در دریافت جزئیات کامل برنامه کاری پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت برنامه کاری پزشک همراه با جزئیات
        /// </summary>
        public async Task<DoctorSchedule> GetDoctorScheduleWithDetailsAsync(int doctorId)
        {
            try
            {
                // غیرفعال کردن موقت فیلترهای سراسری برای دریافت تمام داده‌ها
                _context.DisableFilter("ActiveDoctorSchedules");
                _context.DisableFilter("ActiveDoctorWorkDays");
                _context.DisableFilter("ActiveDoctorTimeRanges");
                
                // ✅ حذف .Include(ds => ds.Doctor) به دلیل خطای SQL: Invalid column name 'Doctor_DoctorId'
                var result = await _context.DoctorSchedules
                    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted)
                    // .Include(ds => ds.Doctor) // ❌ حذف شده: باعث خطای SQL می‌شود
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges)) // اضافه کردن TimeRanges
                    .Include(ds => ds.CreatedByUser)
                    .Include(ds => ds.UpdatedByUser)
                    .AsNoTracking() // ✅ بهبود Performance برای read-only query
                    .FirstOrDefaultAsync();
                
                // فعال کردن مجدد فیلترهای سراسری
                _context.EnableFilter("ActiveDoctorSchedules");
                _context.EnableFilter("ActiveDoctorWorkDays");
                _context.EnableFilter("ActiveDoctorTimeRanges");
                
                return result;
            }
            catch (Exception ex)
            {
                // فعال کردن مجدد فیلترهای سراسری در صورت بروز خطا
                try
                {
                    _context.EnableFilter("ActiveDoctorSchedules");
                    _context.EnableFilter("ActiveDoctorWorkDays");
                    _context.EnableFilter("ActiveDoctorTimeRanges");
                }
                catch { /* نادیده گرفتن خطاهای فعال‌سازی مجدد */ }
                
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت جزئیات برنامه کاری پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// افزودن برنامه کاری جدید برای پزشک
        /// ✅ با استفاده از Transaction برای اتمیک کردن عملیات
        /// </summary>
        public async Task<DoctorSchedule> AddDoctorScheduleAsync(DoctorSchedule schedule)
        {
            // ✅ استفاده از Transaction برای اتمیک کردن تمام عملیات
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (schedule == null)
                        throw new ArgumentNullException(nameof(schedule));

                    // بررسی وجود برنامه کاری قبلی
                    // ✅ استفاده از AsNoTracking() برای جلوگیری از lazy loading Navigation Properties
                    var existingSchedule = await _context.DoctorSchedules
                        .Where(ds => ds.DoctorId == schedule.DoctorId && !ds.IsDeleted)
                        .AsNoTracking() // ✅ جلوگیری از lazy loading
                        .FirstOrDefaultAsync();

                    if (existingSchedule != null)
                        throw new InvalidOperationException($"پزشک قبلاً دارای برنامه کاری است.");

                    // ✅ Navigation Properties را تنظیم نمی‌کنیم (null نمی‌کنیم)
                    // ✅ فقط Foreign Keys (DoctorId, CreatedByUserId, etc.) تنظیم می‌شوند
                    // ✅ تنظیم Navigation Properties به null باعث می‌شود EF6 Foreign Keys را null کند

                    // تنظیم تاریخ‌ها
                    schedule.CreatedAt = DateTime.Now;
                    schedule.UpdatedAt = DateTime.Now;
                    schedule.IsDeleted = false;

                    // ✅ تنظیم تاریخ‌ها برای WorkDays و TimeRanges
                    if (schedule.WorkDays != null)
                    {
                        foreach (var workDay in schedule.WorkDays)
                        {
                            workDay.CreatedAt = DateTime.Now;
                            workDay.UpdatedAt = DateTime.Now;
                            workDay.IsDeleted = false;

                            if (workDay.TimeRanges != null)
                            {
                                foreach (var timeRange in workDay.TimeRanges)
                                {
                                    timeRange.CreatedAt = DateTime.Now;
                                    timeRange.UpdatedAt = DateTime.Now;
                                    timeRange.IsDeleted = false;
                                }
                            }
                        }
                    }

                    // ✅ تنظیم Navigation Properties برای WorkDays و TimeRanges قبل از Add
                    // ✅ این کار باعث می‌شود EF6 بتواند Foreign Keys را به صورت خودکار استخراج کند
                    if (schedule.WorkDays != null)
                    {
                        foreach (var workDay in schedule.WorkDays)
                        {
                            // ✅ تنظیم Navigation Property Schedule برای WorkDay
                            workDay.Schedule = schedule;
                            
                            if (workDay.TimeRanges != null)
                            {
                                foreach (var timeRange in workDay.TimeRanges)
                                {
                                    // ✅ تنظیم Navigation Property WorkDay برای TimeRange
                                    timeRange.WorkDay = workDay;
                                }
                            }
                        }
                    }

                    // ✅ استفاده از Add() برای افزودن Entity به Context
                    _context.DoctorSchedules.Add(schedule);
                    
                    // ✅ تنظیم IsLoaded = false برای Navigation Properties اصلی (Doctor, CreatedByUser, etc.)
                    // ✅ این کار باعث می‌شود EF6 Navigation Properties اصلی را نادیده بگیرد
                    // ✅ اما Navigation Properties برای WorkDays و TimeRanges (Schedule, WorkDay) را استفاده می‌کند
                    var entry = _context.Entry(schedule);
                    entry.Reference(e => e.Doctor).IsLoaded = false;
                    entry.Reference(e => e.CreatedByUser).IsLoaded = false;
                    entry.Reference(e => e.UpdatedByUser).IsLoaded = false;
                    entry.Reference(e => e.DeletedByUser).IsLoaded = false;
                    
                    await _context.SaveChangesAsync();

                    // ✅ تولید و ذخیره اسلات‌های زمانی در دیتابیس (قبل از Commit)
                    // ✅ این کار در همان Transaction انجام می‌شود تا در صورت خطا، همه چیز Rollback شود
                    System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] 🔄 شروع تولید اسلات‌های زمانی");
                    try
                    {
                        await GenerateAndSaveTimeSlotsAsync(schedule.DoctorId, schedule.ScheduleId);
                        System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ✅ تولید اسلات‌های زمانی با موفقیت انجام شد");
                    }
                    catch (Exception slotEx)
                    {
                        // ✅ اگر تولید اسلات‌ها با خطا مواجه شد، Transaction را Rollback می‌کنیم
                        System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ خطا در تولید اسلات‌های زمانی: {slotEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ StackTrace: {slotEx.StackTrace}");
                        transaction.Rollback();
                        throw new InvalidOperationException($"خطا در تولید اسلات‌های زمانی برای برنامه کاری: {slotEx.Message}", slotEx);
                    }

                    // ✅ Commit Transaction در صورت موفقیت کامل (شامل تولید اسلات‌ها)
                    transaction.Commit();
                    System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ✅ Transaction با موفقیت Commit شد");

                    return schedule;
                }
                catch (Exception ex)
                {
                    // ✅ Rollback Transaction در صورت خطا
                    transaction.Rollback();
                    // لاگ خطا برای سیستم‌های پزشکی
                    throw new InvalidOperationException($"خطا در افزودن برنامه کاری پزشک", ex);
                }
            }
        }

        /// <summary>
        /// به‌روزرسانی برنامه کاری پزشک (شامل WorkDays و TimeRanges)
        /// ✅ با استفاده از Transaction برای اتمیک کردن عملیات
        /// </summary>
        public async Task<DoctorSchedule> UpdateDoctorScheduleAsync(DoctorSchedule schedule)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] 🔍 شروع به‌روزرسانی برنامه کاری - ScheduleId: {schedule?.ScheduleId}, DoctorId: {schedule?.DoctorId}");
            
            // ✅ استفاده از Transaction برای اتمیک کردن تمام عملیات
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (schedule == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[UpdateDoctorScheduleAsync] ❌ schedule is null");
                        throw new ArgumentNullException(nameof(schedule));
                    }

                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] 📋 جزئیات Schedule ورودی - ScheduleId: {schedule.ScheduleId}, AppointmentDuration: {schedule.AppointmentDuration}, DefaultStartTime: {schedule.DefaultStartTime}, DefaultEndTime: {schedule.DefaultEndTime}, IsActive: {schedule.IsActive}, WorkDaysCount: {schedule.WorkDays?.Count ?? 0}");

                    // ✅ دریافت برنامه موجود با Include برای WorkDays و TimeRanges
                    // ✅ حذف .Include(ds => ds.Doctor) به دلیل خطای SQL: Invalid column name 'Doctor_DoctorId'
                    var existingSchedule = await _context.DoctorSchedules
                        .Include(ds => ds.WorkDays)
                        .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                        .FirstOrDefaultAsync(ds => ds.ScheduleId == schedule.ScheduleId && !ds.IsDeleted);

                    if (existingSchedule == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ برنامه کاری با ScheduleId {schedule.ScheduleId} یافت نشد");
                        throw new InvalidOperationException($"برنامه کاری با شناسه {schedule.ScheduleId} یافت نشد. لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.");
                    }

                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ✅ برنامه موجود یافت شد - ScheduleId: {existingSchedule.ScheduleId}, DoctorId: {existingSchedule.DoctorId}, WorkDaysCount: {existingSchedule.WorkDays?.Count ?? 0}");

                    // ✅ تنظیم Navigation Properties به null در schedule ورودی برای جلوگیری از خطای SQL
                    // ✅ EF6 نباید Navigation Properties را در Update statement استفاده کند
                    schedule.Doctor = null;
                    schedule.CreatedByUser = null;
                    schedule.UpdatedByUser = null;
                    schedule.DeletedByUser = null;

                    // ✅ به‌روزرسانی فیلدهای اصلی
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] 🔄 به‌روزرسانی فیلدهای اصلی - AppointmentDuration: {schedule.AppointmentDuration} -> {existingSchedule.AppointmentDuration}, DefaultStartTime: {schedule.DefaultStartTime} -> {existingSchedule.DefaultStartTime}");
                    existingSchedule.AppointmentDuration = schedule.AppointmentDuration;
                    existingSchedule.DefaultStartTime = schedule.DefaultStartTime;
                    existingSchedule.DefaultEndTime = schedule.DefaultEndTime;
                    existingSchedule.IsActive = schedule.IsActive;
                    existingSchedule.UpdatedAt = DateTime.Now;
                    existingSchedule.UpdatedByUserId = schedule.UpdatedByUserId;

                    // ✅ به‌روزرسانی WorkDays و TimeRanges
                    if (schedule.WorkDays != null && schedule.WorkDays.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] 🔄 شروع به‌روزرسانی WorkDays - تعداد: {schedule.WorkDays.Count}");
                        await UpdateWorkDaysAsync(existingSchedule, schedule.WorkDays, schedule.UpdatedByUserId);
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ✅ به‌روزرسانی WorkDays با موفقیت انجام شد");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ⚠️ هیچ WorkDay برای به‌روزرسانی وجود ندارد");
                    }

                    // ✅ ذخیره تمام تغییرات
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] 💾 شروع ذخیره تغییرات در دیتابیس");
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ✅ تغییرات با موفقیت ذخیره شد");

                    // ✅ تولید و ذخیره اسلات‌های زمانی در دیتابیس (قبل از Commit)
                    // ✅ این کار در همان Transaction انجام می‌شود تا در صورت خطا، همه چیز Rollback شود
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] 🔄 شروع تولید اسلات‌های زمانی");
                    try
                    {
                        await GenerateAndSaveTimeSlotsAsync(existingSchedule.DoctorId, existingSchedule.ScheduleId);
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ✅ تولید اسلات‌های زمانی با موفقیت انجام شد");
                    }
                    catch (Exception slotEx)
                    {
                        // ✅ اگر تولید اسلات‌ها با خطا مواجه شد، Transaction را Rollback می‌کنیم
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ خطا در تولید اسلات‌های زمانی: {slotEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ StackTrace: {slotEx.StackTrace}");
                        transaction.Rollback();
                        throw new InvalidOperationException($"خطا در تولید اسلات‌های زمانی برای برنامه کاری: {slotEx.Message}", slotEx);
                    }

                    // ✅ Commit Transaction در صورت موفقیت کامل (شامل تولید اسلات‌ها)
                    transaction.Commit();
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ✅ Transaction با موفقیت Commit شد");

                    return existingSchedule;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // ✅ Rollback Transaction در صورت خطا
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ خطای همزمانی - Rollback انجام شد. ExceptionType: {ex.GetType().Name}, Message: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ InnerException: {ex.InnerException.GetType().Name}, Message: {ex.InnerException.Message}");
                    }
                    // لاگ خطای همزمانی برای سیستم‌های پزشکی
                    throw new InvalidOperationException($"خطای همزمانی در به‌روزرسانی برنامه کاری. ممکن است برنامه کاری در جای دیگری تغییر کرده باشد. لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.", ex);
                }
                catch (InvalidOperationException ex)
                {
                    // ✅ Rollback Transaction در صورت خطا
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ خطای عملیاتی - Rollback انجام شد. ExceptionType: {ex.GetType().Name}, Message: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ InnerException: {ex.InnerException.GetType().Name}, Message: {ex.InnerException.Message}, StackTrace: {ex.InnerException.StackTrace}");
                    }
                    // پرتاب مجدد همان Exception با پیام واضح‌تر
                    throw;
                }
                catch (DbUpdateException dbEx)
                {
                    // ✅ Rollback Transaction در صورت خطا
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ خطای DbUpdateException - Rollback انجام شد. ExceptionType: {dbEx.GetType().Name}, Message: {dbEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ StackTrace: {dbEx.StackTrace}");
                    
                    // ✅ بررسی InnerException برای جزئیات بیشتر
                    var innerEx = dbEx.InnerException;
                    var errorDetails = new System.Text.StringBuilder();
                    errorDetails.AppendLine($"خطا در به‌روزرسانی برنامه کاری پزشک.");
                    
                    while (innerEx != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ InnerException: {innerEx.GetType().Name}, Message: {innerEx.Message}");
                        if (innerEx is System.Data.SqlClient.SqlException sqlEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ SqlException - Number: {sqlEx.Number}, LineNumber: {sqlEx.LineNumber}, Procedure: {sqlEx.Procedure}");
                            System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ SqlException - Server: {sqlEx.Server}, Source: {sqlEx.Source}");
                            
                            // ✅ بررسی خطاهای خاص SQL
                            if (sqlEx.Number == 2601 || sqlEx.Number == 2627) // Unique Constraint Violation
                            {
                                errorDetails.AppendLine($"خطای محدودیت یکتایی: یک رکورد تکراری در دیتابیس وجود دارد.");
                                errorDetails.AppendLine($"لطفاً صفحه را نوسازی کنید و مجدداً تلاش کنید.");
                            }
                            else if (sqlEx.Number == 547) // Foreign Key Constraint Violation
                            {
                                errorDetails.AppendLine($"خطای محدودیت کلید خارجی: رکورد مرتبط یافت نشد.");
                            }
                            else
                            {
                                errorDetails.AppendLine($"خطای SQL: {sqlEx.Message}");
                            }
                        }
                        innerEx = innerEx.InnerException;
                    }
                    
                    // لاگ خطا برای سیستم‌های پزشکی
                    throw new InvalidOperationException(errorDetails.ToString(), dbEx);
                }
                catch (Exception ex)
                {
                    // ✅ Rollback Transaction در صورت خطا
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ خطای غیرمنتظره - Rollback انجام شد. ExceptionType: {ex.GetType().Name}, Message: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ StackTrace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateDoctorScheduleAsync] ❌ InnerException: {ex.InnerException.GetType().Name}, Message: {ex.InnerException.Message}, StackTrace: {ex.InnerException.StackTrace}");
                    }
                    // لاگ خطا برای سیستم‌های پزشکی
                    throw new InvalidOperationException($"خطا در به‌روزرسانی برنامه کاری پزشک. لطفاً دوباره تلاش کنید. اگر مشکل ادامه داشت، با بخش فنی تماس بگیرید. جزئیات خطا: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// به‌روزرسانی روزهای کاری و بازه‌های زمانی
        /// </summary>
        private async Task UpdateWorkDaysAsync(DoctorSchedule existingSchedule, ICollection<DoctorWorkDay> newWorkDays, string updatedByUserId)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] 🔍 شروع به‌روزرسانی WorkDays - ScheduleId: {existingSchedule.ScheduleId}, تعداد WorkDays جدید: {newWorkDays.Count}");
            
            // ✅ دریافت تمام WorkDays موجود (شامل IsDeleted = true) برای جلوگیری از تداخل با Unique Constraint
            // ✅ Unique Constraint: ScheduleId + DayOfWeek + IsDeleted
            // ✅ اگر WorkDay با IsDeleted = true وجود داشته باشد، باید آن را فعال کنیم نه اینکه یک WorkDay جدید اضافه کنیم
            var allExistingWorkDays = existingSchedule.WorkDays?.ToList() ?? new List<DoctorWorkDay>();
            var existingWorkDays = allExistingWorkDays.Where(wd => !wd.IsDeleted).ToList();
            System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] 📋 تعداد WorkDays موجود (غیرحذف شده): {existingWorkDays.Count}, تعداد کل: {allExistingWorkDays.Count}");

            // ✅ ایجاد Dictionary برای جستجوی سریع‌تر (شامل WorkDays با IsDeleted = true)
            // ✅ این کار برای جلوگیری از تداخل با Unique Constraint است
            // ✅ اگر چند WorkDay با همان DayOfWeek وجود دارد (یکی IsDeleted = false و یکی IsDeleted = true)،
            // ✅ اولویت با WorkDay با IsDeleted = false است
            var existingWorkDaysDict = new Dictionary<int, DoctorWorkDay>();
            foreach (var wd in allExistingWorkDays)
            {
                // ✅ اگر WorkDay با این DayOfWeek وجود ندارد یا WorkDay موجود IsDeleted = true است و WorkDay جدید IsDeleted = false است
                if (!existingWorkDaysDict.ContainsKey(wd.DayOfWeek) || 
                    (existingWorkDaysDict[wd.DayOfWeek].IsDeleted && !wd.IsDeleted))
                {
                    existingWorkDaysDict[wd.DayOfWeek] = wd;
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] 🔑 Dictionary ایجاد شد - تعداد WorkDays: {existingWorkDaysDict.Count}");

            foreach (var newWorkDay in newWorkDays)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] 🔄 پردازش WorkDay - DayOfWeek: {newWorkDay.DayOfWeek}, IsActive: {newWorkDay.IsActive}, TimeRangesCount: {newWorkDay.TimeRanges?.Count ?? 0}");
                
                // ✅ بررسی وجود WorkDay با همان DayOfWeek (شامل WorkDays با IsDeleted = true)
                if (existingWorkDaysDict.TryGetValue(newWorkDay.DayOfWeek, out var existingWorkDay))
                {
                    System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] ✅ WorkDay موجود یافت شد - WorkDayId: {existingWorkDay.WorkDayId}, DayOfWeek: {existingWorkDay.DayOfWeek}, IsDeleted: {existingWorkDay.IsDeleted}");
                    
                    // ✅ اگر WorkDay با IsDeleted = true است، آن را فعال می‌کنیم
                    if (existingWorkDay.IsDeleted)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] 🔄 فعال کردن مجدد WorkDay حذف شده - WorkDayId: {existingWorkDay.WorkDayId}");
                        existingWorkDay.IsDeleted = false;
                        existingWorkDay.DeletedAt = null;
                        existingWorkDay.DeletedByUserId = null;
                    }
                    
                    // ✅ به‌روزرسانی WorkDay موجود
                    existingWorkDay.IsActive = newWorkDay.IsActive;
                    existingWorkDay.UpdatedAt = DateTime.Now;
                    existingWorkDay.UpdatedByUserId = updatedByUserId;

                    // ✅ به‌روزرسانی TimeRanges
                    if (newWorkDay.TimeRanges != null && newWorkDay.TimeRanges.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] 🔄 شروع به‌روزرسانی TimeRanges برای WorkDay {existingWorkDay.WorkDayId} - تعداد TimeRanges جدید: {newWorkDay.TimeRanges.Count}");
                        await UpdateTimeRangesAsync(existingWorkDay, newWorkDay.TimeRanges, updatedByUserId);
                        System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] ✅ به‌روزرسانی TimeRanges برای WorkDay {existingWorkDay.WorkDayId} با موفقیت انجام شد");
                    }
                    else
                    {
                        // ✅ اگر TimeRanges جدیدی وجود ندارد، غیرفعال کردن TimeRanges موجود
                        if (existingWorkDay.TimeRanges != null)
                        {
                            foreach (var timeRange in existingWorkDay.TimeRanges.Where(tr => !tr.IsDeleted))
                            {
                                timeRange.IsActive = false;
                                timeRange.IsDeleted = true;
                                timeRange.DeletedAt = DateTime.Now;
                                timeRange.DeletedByUserId = updatedByUserId;
                                timeRange.UpdatedAt = DateTime.Now;
                                timeRange.UpdatedByUserId = updatedByUserId;
                            }
                        }
                    }
                }
                else
                {
                    // ✅ افزودن WorkDay جدید
                    // ✅ بررسی اینکه آیا WorkDay با IsDeleted = true وجود دارد یا نه
                    // ✅ اگر وجود دارد، آن را فعال می‌کنیم به جای افزودن WorkDay جدید
                    var deletedWorkDay = allExistingWorkDays.FirstOrDefault(wd => wd.DayOfWeek == newWorkDay.DayOfWeek && wd.IsDeleted);
                    
                    if (deletedWorkDay != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] 🔄 فعال کردن مجدد WorkDay حذف شده - WorkDayId: {deletedWorkDay.WorkDayId}, DayOfWeek: {deletedWorkDay.DayOfWeek}");
                        
                        // ✅ فعال کردن مجدد WorkDay حذف شده
                        deletedWorkDay.IsDeleted = false;
                        deletedWorkDay.DeletedAt = null;
                        deletedWorkDay.DeletedByUserId = null;
                        deletedWorkDay.IsActive = newWorkDay.IsActive;
                        deletedWorkDay.UpdatedAt = DateTime.Now;
                        deletedWorkDay.UpdatedByUserId = updatedByUserId;

                        // ✅ به‌روزرسانی TimeRanges
                        if (newWorkDay.TimeRanges != null && newWorkDay.TimeRanges.Any())
                        {
                            System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] 🔄 شروع به‌روزرسانی TimeRanges برای WorkDay {deletedWorkDay.WorkDayId} - تعداد TimeRanges جدید: {newWorkDay.TimeRanges.Count}");
                            await UpdateTimeRangesAsync(deletedWorkDay, newWorkDay.TimeRanges, updatedByUserId);
                            System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] ✅ به‌روزرسانی TimeRanges برای WorkDay {deletedWorkDay.WorkDayId} با موفقیت انجام شد");
                        }
                    }
                    else
                    {
                        // ✅ افزودن WorkDay جدید (فقط اگر WorkDay با IsDeleted = true وجود ندارد)
                        System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] ➕ افزودن WorkDay جدید - DayOfWeek: {newWorkDay.DayOfWeek}");
                        
                        newWorkDay.ScheduleId = existingSchedule.ScheduleId;
                        newWorkDay.CreatedAt = DateTime.Now;
                        newWorkDay.UpdatedAt = DateTime.Now;
                        newWorkDay.CreatedByUserId = updatedByUserId;
                        newWorkDay.UpdatedByUserId = updatedByUserId;
                        newWorkDay.IsDeleted = false;

                        // ✅ افزودن TimeRanges
                        if (newWorkDay.TimeRanges != null)
                        {
                            foreach (var timeRange in newWorkDay.TimeRanges)
                            {
                                timeRange.CreatedAt = DateTime.Now;
                                timeRange.UpdatedAt = DateTime.Now;
                                timeRange.CreatedByUserId = updatedByUserId;
                                timeRange.UpdatedByUserId = updatedByUserId;
                                timeRange.IsDeleted = false;
                            }
                        }

                        _context.DoctorWorkDays.Add(newWorkDay);
                    }
                }
            }

            // ✅ حذف نرم WorkDays که دیگر در لیست جدید نیستند (فقط WorkDays فعال)
            // ✅ فقط WorkDays فعال را در نظر می‌گیریم، چون ToEntity() فقط WorkDays فعال را ارسال می‌کند
            var newWorkDaysDayOfWeeks = newWorkDays
                .Where(wd => wd.IsActive) // ✅ فقط WorkDays فعال
                .Select(wd => wd.DayOfWeek)
                .ToHashSet();
            
            System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] 🔍 WorkDays فعال در لیست جدید: {string.Join(", ", newWorkDaysDayOfWeeks)}");
            
            foreach (var existingWorkDay in existingWorkDays)
            {
                // ✅ اگر WorkDay موجود فعال است و در لیست جدید نیست، آن را غیرفعال می‌کنیم
                if (existingWorkDay.IsActive && !newWorkDaysDayOfWeeks.Contains(existingWorkDay.DayOfWeek))
                {
                    System.Diagnostics.Debug.WriteLine($"[UpdateWorkDaysAsync] 🗑️ حذف نرم WorkDay - WorkDayId: {existingWorkDay.WorkDayId}, DayOfWeek: {existingWorkDay.DayOfWeek}");
                    
                    existingWorkDay.IsActive = false;
                    existingWorkDay.IsDeleted = true;
                    existingWorkDay.DeletedAt = DateTime.Now;
                    existingWorkDay.DeletedByUserId = updatedByUserId;
                    existingWorkDay.UpdatedAt = DateTime.Now;
                    existingWorkDay.UpdatedByUserId = updatedByUserId;

                    // ✅ حذف نرم TimeRanges مربوطه
                    if (existingWorkDay.TimeRanges != null)
                    {
                        foreach (var timeRange in existingWorkDay.TimeRanges.Where(tr => !tr.IsDeleted))
                        {
                            timeRange.IsActive = false;
                            timeRange.IsDeleted = true;
                            timeRange.DeletedAt = DateTime.Now;
                            timeRange.DeletedByUserId = updatedByUserId;
                            timeRange.UpdatedAt = DateTime.Now;
                            timeRange.UpdatedByUserId = updatedByUserId;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// به‌روزرسانی بازه‌های زمانی یک روز کاری
        /// ✅ با بررسی تداخل بازه‌های زمانی
        /// </summary>
        private async Task UpdateTimeRangesAsync(DoctorWorkDay existingWorkDay, ICollection<DoctorTimeRange> newTimeRanges, string updatedByUserId)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🔍 شروع به‌روزرسانی TimeRanges - WorkDayId: {existingWorkDay.WorkDayId}, DayOfWeek: {existingWorkDay.DayOfWeek}, تعداد TimeRanges جدید: {newTimeRanges.Count}");
            
            // ✅ Helper function برای ایجاد key از TimeSpan (24-hour format)
            Func<TimeSpan, string> getTimeKey = (ts) => $"{ts.Hours:D2}:{ts.Minutes:D2}";
            
            // ✅ لاگ TimeRanges جدید
            foreach (var tr in newTimeRanges)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 📋 TimeRange جدید - TimeRangeId: {tr.TimeRangeId}, StartTime: {getTimeKey(tr.StartTime)}, EndTime: {getTimeKey(tr.EndTime)}, IsActive: {tr.IsActive}");
            }
            
            // ✅ بررسی تداخل بازه‌های زمانی قبل از ذخیره
            if (HasOverlappingTimeRanges(newTimeRanges))
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ❌ TimeRange های جدید با هم تداخل دارند");
                throw new InvalidOperationException("❌ بازه‌های زمانی جدید با هم تداخل دارند. لطفاً بازه‌های زمانی را بررسی کنید و مطمئن شوید که هیچ دو بازه‌ای با هم تداخل ندارند.");
            }
            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ✅ TimeRange های جدید با هم تداخل ندارند");

            // ✅ دریافت TimeRanges موجود
            var existingTimeRanges = existingWorkDay.TimeRanges?.Where(tr => !tr.IsDeleted).ToList() ?? new List<DoctorTimeRange>();
            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 📋 تعداد TimeRanges موجود: {existingTimeRanges.Count}");
            
            // ✅ لاگ TimeRanges موجود
            foreach (var tr in existingTimeRanges)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 📋 TimeRange موجود - TimeRangeId: {tr.TimeRangeId}, StartTime: {getTimeKey(tr.StartTime)}, EndTime: {getTimeKey(tr.EndTime)}, IsActive: {tr.IsActive}, IsDeleted: {tr.IsDeleted}");
            }
            
            // ✅ ایجاد HashSet از keys برای TimeRange های جدید (بر اساس StartTime و EndTime)
            var newTimeRangesKeys = newTimeRanges
                .Select(tr => $"{getTimeKey(tr.StartTime)}_{getTimeKey(tr.EndTime)}")
                .ToHashSet();
            
            // ✅ ایجاد HashSet از TimeRangeId های جدید (برای شناسایی TimeRange های در حال ویرایش)
            var newTimeRangeIds = newTimeRanges
                .Where(tr => tr.TimeRangeId > 0) // فقط TimeRange هایی که TimeRangeId دارند (در حال ویرایش هستند)
                .Select(tr => tr.TimeRangeId)
                .ToHashSet();
            
            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🔑 Keys برای TimeRange های جدید: {string.Join(", ", newTimeRangesKeys)}");
            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🔑 TimeRangeId های جدید (در حال ویرایش): {string.Join(", ", newTimeRangeIds)}");
            
            // ✅ بررسی تداخل با TimeRanges موجود (فقط برای TimeRanges فعال)
            // ✅ حذف TimeRange های موجود که:
            // 1. دقیقاً با newTimeRanges یکسان هستند (با همان StartTime و EndTime)
            // 2. یا TimeRangeId آن‌ها در newTimeRanges است (در حال ویرایش هستند)
            var activeExistingTimeRanges = existingTimeRanges.Where(tr => tr.IsActive).ToList();
            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 📋 تعداد TimeRanges موجود فعال: {activeExistingTimeRanges.Count}");
            
            // ✅ فقط TimeRange های موجود که:
            // - دقیقاً با newTimeRanges یکسان نیستند (با همان StartTime و EndTime)
            // - و TimeRangeId آن‌ها در newTimeRanges نیست (در حال ویرایش نیستند)
            var remainingExistingTimeRanges = activeExistingTimeRanges
                .Where(tr => 
                {
                    var key = $"{getTimeKey(tr.StartTime)}_{getTimeKey(tr.EndTime)}";
                    var isExactMatch = newTimeRangesKeys.Contains(key);
                    var isBeingEdited = newTimeRangeIds.Contains(tr.TimeRangeId);
                    
                    // ✅ اگر TimeRange دقیقاً یکسان است یا در حال ویرایش است، از بررسی تداخل حذف می‌شود
                    if (isExactMatch)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🔄 TimeRange موجود {tr.TimeRangeId} ({key}) با TimeRange جدید یکسان است - حذف از بررسی تداخل");
                        return false;
                    }
                    
                    if (isBeingEdited)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🔄 TimeRange موجود {tr.TimeRangeId} ({key}) در حال ویرایش است - حذف از بررسی تداخل");
                        return false;
                    }
                    
                    return true;
                })
                .ToList();
            
            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 📋 تعداد TimeRanges موجود باقی‌مانده (بعد از حذف یکسان‌ها): {remainingExistingTimeRanges.Count}");
            
            // ✅ لاگ TimeRanges باقی‌مانده
            foreach (var tr in remainingExistingTimeRanges)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 📋 TimeRange باقی‌مانده - TimeRangeId: {tr.TimeRangeId}, StartTime: {getTimeKey(tr.StartTime)}, EndTime: {getTimeKey(tr.EndTime)}");
            }
            
            // ✅ بررسی تداخل بین TimeRange های جدید و TimeRange های موجود باقی‌مانده
            // ✅ اگر هیچ TimeRange موجودی باقی نمانده، فقط newTimeRanges را بررسی می‌کنیم (که قبلاً بررسی شده)
            if (remainingExistingTimeRanges.Any())
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🔍 شروع بررسی تداخل بین TimeRange های جدید و موجود");
                
                // ✅ بررسی تداخل: TimeRange های جدید نباید با TimeRange های موجود باقی‌مانده تداخل داشته باشند
                foreach (var newRange in newTimeRanges)
                {
                    // ✅ اگر TimeRange در حال ویرایش است (TimeRangeId > 0)، از بررسی تداخل با خودش صرف نظر می‌کنیم
                    var isBeingEdited = newRange.TimeRangeId > 0 && newTimeRangeIds.Contains(newRange.TimeRangeId);
                    
                    foreach (var existingRange in remainingExistingTimeRanges)
                    {
                        // ✅ اگر TimeRange جدید همان TimeRange موجود است (در حال ویرایش است)، از بررسی تداخل صرف نظر می‌کنیم
                        if (isBeingEdited && existingRange.TimeRangeId == newRange.TimeRangeId)
                        {
                            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ⏭️ TimeRange {newRange.TimeRangeId} در حال ویرایش است - از بررسی تداخل با خودش صرف نظر می‌شود");
                            continue;
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🔍 بررسی تداخل - جدید (TimeRangeId: {newRange.TimeRangeId}): {getTimeKey(newRange.StartTime)}-{getTimeKey(newRange.EndTime)}, موجود (TimeRangeId: {existingRange.TimeRangeId}): {getTimeKey(existingRange.StartTime)}-{getTimeKey(existingRange.EndTime)}");
                        
                        // ✅ بررسی تداخل: دو بازه زمانی تداخل دارند اگر:
                        // newRange.StartTime < existingRange.EndTime && newRange.EndTime > existingRange.StartTime
                        if (newRange.StartTime < existingRange.EndTime && newRange.EndTime > existingRange.StartTime)
                        {
                            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ❌ تداخل پیدا شد! جدید (TimeRangeId: {newRange.TimeRangeId}): {getTimeKey(newRange.StartTime)}-{getTimeKey(newRange.EndTime)}, موجود (TimeRangeId: {existingRange.TimeRangeId}): {getTimeKey(existingRange.StartTime)}-{getTimeKey(existingRange.EndTime)}");
                            throw new InvalidOperationException($"❌ بازه زمانی جدید ({getTimeKey(newRange.StartTime)}-{getTimeKey(newRange.EndTime)}) با بازه زمانی موجود ({getTimeKey(existingRange.StartTime)}-{getTimeKey(existingRange.EndTime)}) تداخل دارد. لطفاً بازه‌های زمانی را بررسی کنید و مطمئن شوید که هیچ دو بازه‌ای با هم تداخل ندارند.");
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ✅ هیچ تداخلی بین TimeRange های جدید و موجود یافت نشد");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ✅ هیچ TimeRange موجودی باقی نمانده - فقط TimeRange های جدید بررسی می‌شوند");
            }

            // ✅ ایجاد Dictionary برای جستجوی سریع‌تر
            // ✅ اول بر اساس TimeRangeId (برای TimeRange های در حال ویرایش)
            // ✅ سپس بر اساس StartTime و EndTime (برای TimeRange های یکسان)
            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🔑 ایجاد Dictionary برای TimeRanges موجود");
            var existingTimeRangesByIdDict = existingTimeRanges
                .Where(tr => tr.TimeRangeId > 0)
                .ToDictionary(tr => tr.TimeRangeId, tr => tr);
            
            var existingTimeRangesByKeyDict = existingTimeRanges.ToDictionary(
                tr => $"{getTimeKey(tr.StartTime)}_{getTimeKey(tr.EndTime)}",
                tr => tr
            );

            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🔄 شروع به‌روزرسانی/افزودن TimeRanges");
            foreach (var newTimeRange in newTimeRanges)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🔍 بررسی TimeRange - TimeRangeId: {newTimeRange.TimeRangeId}, StartTime: {getTimeKey(newTimeRange.StartTime)}, EndTime: {getTimeKey(newTimeRange.EndTime)}");

                // ✅ اول بررسی می‌کنیم که آیا TimeRangeId دارد (در حال ویرایش است)
                if (newTimeRange.TimeRangeId > 0 && existingTimeRangesByIdDict.TryGetValue(newTimeRange.TimeRangeId, out var existingTimeRangeById))
                {
                    System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ✅ TimeRange موجود یافت شد (بر اساس TimeRangeId) - TimeRangeId: {existingTimeRangeById.TimeRangeId}, به‌روزرسانی StartTime: {getTimeKey(existingTimeRangeById.StartTime)} -> {getTimeKey(newTimeRange.StartTime)}, EndTime: {getTimeKey(existingTimeRangeById.EndTime)} -> {getTimeKey(newTimeRange.EndTime)}, IsActive: {existingTimeRangeById.IsActive} -> {newTimeRange.IsActive}");
                    
                    // ✅ به‌روزرسانی TimeRange موجود (شامل StartTime و EndTime)
                    existingTimeRangeById.StartTime = newTimeRange.StartTime;
                    existingTimeRangeById.EndTime = newTimeRange.EndTime;
                    existingTimeRangeById.IsActive = newTimeRange.IsActive;
                    existingTimeRangeById.UpdatedAt = DateTime.Now;
                    existingTimeRangeById.UpdatedByUserId = updatedByUserId;
                }
                else
                {
                    // ✅ بررسی وجود TimeRange با همان StartTime و EndTime (برای TimeRange های جدید که دقیقاً یکسان هستند)
                    var key = $"{getTimeKey(newTimeRange.StartTime)}_{getTimeKey(newTimeRange.EndTime)}";
                    if (existingTimeRangesByKeyDict.TryGetValue(key, out var existingTimeRangeByKey))
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ✅ TimeRange موجود یافت شد (بر اساس Key) - TimeRangeId: {existingTimeRangeByKey.TimeRangeId}, به‌روزرسانی IsActive: {existingTimeRangeByKey.IsActive} -> {newTimeRange.IsActive}");
                        
                        // ✅ به‌روزرسانی TimeRange موجود (فقط IsActive)
                        existingTimeRangeByKey.IsActive = newTimeRange.IsActive;
                        existingTimeRangeByKey.UpdatedAt = DateTime.Now;
                        existingTimeRangeByKey.UpdatedByUserId = updatedByUserId;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ➕ TimeRange جدید - افزودن به دیتابیس - StartTime: {getTimeKey(newTimeRange.StartTime)}, EndTime: {getTimeKey(newTimeRange.EndTime)}");
                        
                        // ✅ افزودن TimeRange جدید
                        newTimeRange.WorkDayId = existingWorkDay.WorkDayId;
                        newTimeRange.CreatedAt = DateTime.Now;
                        newTimeRange.UpdatedAt = DateTime.Now;
                        newTimeRange.CreatedByUserId = updatedByUserId;
                        newTimeRange.UpdatedByUserId = updatedByUserId;
                        newTimeRange.IsDeleted = false;

                        _context.DoctorTimeRanges.Add(newTimeRange);
                    }
                }
            }

            // ✅ حذف نرم TimeRanges که دیگر در لیست جدید نیستند
            // ✅ TimeRange حذف می‌شود اگر:
            // 1. Key آن (StartTime-EndTime) در newTimeRangesKeys نیست
            // 2. و TimeRangeId آن در newTimeRangeIds نیست (در حال ویرایش نیست)
            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🗑️ شروع حذف نرم TimeRanges که دیگر در لیست جدید نیستند");
            var deletedCount = 0;
            foreach (var existingTimeRange in existingTimeRanges)
            {
                var key = $"{getTimeKey(existingTimeRange.StartTime)}_{getTimeKey(existingTimeRange.EndTime)}";
                var keyExists = newTimeRangesKeys.Contains(key);
                var idExists = existingTimeRange.TimeRangeId > 0 && newTimeRangeIds.Contains(existingTimeRange.TimeRangeId);
                
                // ✅ اگر Key وجود ندارد و Id هم وجود ندارد (یا TimeRangeId = 0 است)، TimeRange حذف می‌شود
                if (!keyExists && !idExists)
                {
                    System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] 🗑️ حذف نرم TimeRange - TimeRangeId: {existingTimeRange.TimeRangeId}, Key: {key} (KeyExists: {keyExists}, IdExists: {idExists})");
                    existingTimeRange.IsActive = false;
                    existingTimeRange.IsDeleted = true;
                    existingTimeRange.DeletedAt = DateTime.Now;
                    existingTimeRange.DeletedByUserId = updatedByUserId;
                    existingTimeRange.UpdatedAt = DateTime.Now;
                    existingTimeRange.UpdatedByUserId = updatedByUserId;
                    deletedCount++;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ✅ TimeRange نگه داشته شد - TimeRangeId: {existingTimeRange.TimeRangeId}, Key: {key} (KeyExists: {keyExists}, IdExists: {idExists})");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"[UpdateTimeRangesAsync] ✅ به‌روزرسانی TimeRanges با موفقیت انجام شد - تعداد حذف شده: {deletedCount}");
        }

        /// <summary>
        /// بررسی تداخل بازه‌های زمانی
        /// </summary>
        private bool HasOverlappingTimeRanges(ICollection<DoctorTimeRange> timeRanges)
        {
            if (timeRanges == null || timeRanges.Count <= 1) return false;

            // ✅ مرتب‌سازی بر اساس StartTime
            var sortedRanges = timeRanges.OrderBy(t => t.StartTime).ToList();
            
            // ✅ بررسی تداخل بین بازه‌های متوالی
            for (int i = 0; i < sortedRanges.Count - 1; i++)
            {
                var currentRange = sortedRanges[i];
                var nextRange = sortedRanges[i + 1];

                // ✅ بررسی اینکه آیا EndTime بازه فعلی بعد از StartTime بازه بعدی است
                if (currentRange.EndTime > nextRange.StartTime)
                {
                    return true; // تداخل پیدا شد
                }
            }

            return false; // هیچ تداخلی وجود ندارد
        }

        /// <summary>
        /// دریافت اسلات‌های زمانی خالی و قابل رزرو برای یک پزشک در یک روز مشخص
        /// ✅ با بررسی تعطیلات رسمی و ScheduleExceptions
        /// </summary>
        public async Task<List<DoctorTimeSlot>> GetAvailableAppointmentSlotsAsync(int doctorId, DateTime date)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] 🔍 شروع - DoctorId: {doctorId}, Date: {date:yyyy/MM/dd}");
                
                // ✅ بررسی تعطیلات رسمی ایران
                if (IsPersianHoliday(date))
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] 📅 تاریخ {date:yyyy/MM/dd} تعطیل رسمی است");
                    return new List<DoctorTimeSlot>(); // در تعطیلات رسمی هیچ اسلاتی در دسترس نیست
                }

                // دریافت برنامه کاری پزشک همراه با Exceptions و WorkDays
                var doctorSchedule = await _context.DoctorSchedules
                    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted && ds.IsActive)
                    .Include(ds => ds.Exceptions) // ✅ Include برای ScheduleExceptions
                    .Include(ds => ds.WorkDays) // ✅ Include برای WorkDays
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges)) // ✅ Include برای TimeRanges
                    .FirstOrDefaultAsync();

                if (doctorSchedule == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ❌ برنامه کاری برای پزشک {doctorId} یافت نشد");
                    return new List<DoctorTimeSlot>();
                }
                
                System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ✅ برنامه کاری یافت شد - ScheduleId: {doctorSchedule.ScheduleId}, WorkDaysCount: {doctorSchedule.WorkDays?.Count ?? 0}");

                // ✅ بررسی ScheduleExceptions (تعطیلات، مرخصی، و غیره)
                var hasScheduleException = await HasScheduleExceptionAsync(doctorSchedule.ScheduleId, date);
                if (hasScheduleException)
                {
                    return new List<DoctorTimeSlot>(); // در صورت وجود استثنا، هیچ اسلاتی در دسترس نیست
                }

                // دریافت روزهای کاری پزشک
                // استفاده از WorkDays که قبلاً Include شده‌اند
                var dayOfWeek = (int)date.DayOfWeek;
                System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] 📅 تاریخ: {date:yyyy/MM/dd}, DayOfWeek: {dayOfWeek} ({(DayOfWeek)dayOfWeek})");
                
                // ✅ لاگ تمام WorkDays موجود
                if (doctorSchedule.WorkDays != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] 📋 تمام WorkDays موجود:");
                    foreach (var wd in doctorSchedule.WorkDays)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - WorkDayId: {wd.WorkDayId}, DayOfWeek: {wd.DayOfWeek}, IsActive: {wd.IsActive}, IsDeleted: {wd.IsDeleted}");
                    }
                }
                
                var workDays = doctorSchedule.WorkDays?
                    .Where(wd => wd.DayOfWeek == dayOfWeek && wd.IsActive && !wd.IsDeleted)
                    .ToList() ?? new List<DoctorWorkDay>();

                System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] 📅 WorkDays برای DayOfWeek {dayOfWeek}: {workDays.Count}");
                
                if (!workDays.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ⚠️ هیچ WorkDay برای DayOfWeek {dayOfWeek} یافت نشد");
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] 💡 پیشنهاد: بررسی کنید که آیا تاریخ انتخاب شده با روزهای کاری پزشک مطابقت دارد");
                    return new List<DoctorTimeSlot>();
                }

                var availableSlots = new List<DoctorTimeSlot>();

                foreach (var workDay in workDays)
                {
                    var activeTimeRanges = workDay.TimeRanges?.Where(tr => tr.IsActive && !tr.IsDeleted).ToList() ?? new List<DoctorTimeRange>();
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ⏰ WorkDay {workDay.DayOfWeek}: {activeTimeRanges.Count} TimeRange فعال");
                    
                    foreach (var timeRange in activeTimeRanges)
                    {
                        var currentTime = timeRange.StartTime;
                        var endTime = timeRange.EndTime;

                        while (currentTime < endTime)
                        {
                            var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(doctorSchedule.AppointmentDuration));

                            if (slotEndTime <= endTime)
                            {
                                // ✅ بررسی ScheduleExceptions جزئی (برای بازه‌های زمانی خاص)
                                var hasPartialException = await HasPartialScheduleExceptionAsync(
                                    doctorSchedule.ScheduleId, date, currentTime, slotEndTime);
                                
                                if (hasPartialException)
                                {
                                    currentTime = slotEndTime;
                                    continue; // این اسلات به دلیل استثنا در دسترس نیست
                                }

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

                System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ✅ {availableSlots.Count} اسلات زمانی تولید شد");
                return availableSlots;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ❌ خطا: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ❌ StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ❌ InnerException: {ex.InnerException.Message}");
                }
                throw new InvalidOperationException($"خطا در دریافت اسلات‌های زمانی خالی برای پزشک {doctorId} در تاریخ {date:yyyy/MM/dd}", ex);
            }
        }

        /// <summary>
        /// تولید و ذخیره اسلات‌های زمانی در دیتابیس برای یک بازه زمانی مشخص
        /// این متد هنگام ایجاد یا به‌روزرسانی برنامه کاری فراخوانی می‌شود
        /// </summary>
        public async Task GenerateAndSaveTimeSlotsAsync(int doctorId, int scheduleId, int daysAhead = 90)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 🔍 شروع - DoctorId: {doctorId}, ScheduleId: {scheduleId}, DaysAhead: {daysAhead}");

                // دریافت برنامه کاری با جزئیات
                var doctorSchedule = await _context.DoctorSchedules
                    .Where(ds => ds.ScheduleId == scheduleId && ds.DoctorId == doctorId && !ds.IsDeleted && ds.IsActive)
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                    .FirstOrDefaultAsync();

                if (doctorSchedule == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ❌ برنامه کاری یافت نشد - ScheduleId: {scheduleId}, DoctorId: {doctorId}");
                    throw new InvalidOperationException($"برنامه کاری با شناسه {scheduleId} برای پزشک {doctorId} یافت نشد یا غیرفعال است.");
                }

                // ✅ بررسی وجود WorkDays
                if (doctorSchedule.WorkDays == null || !doctorSchedule.WorkDays.Any(wd => wd.IsActive && !wd.IsDeleted))
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ هیچ WorkDay فعالی برای این برنامه کاری وجود ندارد");
                    // این یک هشدار است، نه خطا - ممکن است پزشک هنوز روزهای کاری را تنظیم نکرده باشد
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ برنامه کاری یافت شد - WorkDaysCount: {doctorSchedule.WorkDays?.Count(wd => wd.IsActive && !wd.IsDeleted) ?? 0}");

                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(daysAhead);
                var generatedSlots = new List<DoctorTimeSlot>();

                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 📅 تولید اسلات‌ها از {startDate:yyyy/MM/dd} تا {endDate:yyyy/MM/dd}");

                // تولید اسلات‌ها برای هر روز در بازه زمانی
                for (var date = startDate; date < endDate; date = date.AddDays(1))
                {
                    // بررسی تعطیلات رسمی
                    if (IsPersianHoliday(date))
                        continue;

                    // بررسی ScheduleExceptions
                    var hasScheduleException = await HasScheduleExceptionAsync(scheduleId, date);
                    if (hasScheduleException)
                        continue;

                    var dayOfWeek = (int)date.DayOfWeek;
                    var workDays = doctorSchedule.WorkDays?
                        .Where(wd => wd.DayOfWeek == dayOfWeek && wd.IsActive && !wd.IsDeleted)
                        .ToList() ?? new List<DoctorWorkDay>();

                    foreach (var workDay in workDays)
                    {
                        var activeTimeRanges = workDay.TimeRanges?
                            .Where(tr => tr.IsActive && !tr.IsDeleted)
                            .ToList() ?? new List<DoctorTimeRange>();

                        foreach (var timeRange in activeTimeRanges)
                        {
                            var currentTime = timeRange.StartTime;
                            var endTime = timeRange.EndTime;

                            while (currentTime < endTime)
                            {
                                var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(doctorSchedule.AppointmentDuration));

                                if (slotEndTime <= endTime)
                                {
                                    // بررسی ScheduleExceptions جزئی
                                    var hasPartialException = await HasPartialScheduleExceptionAsync(
                                        scheduleId, date, currentTime, slotEndTime);

                                    if (!hasPartialException)
                                    {
                                        // بررسی وجود اسلات در دیتابیس
                                        var existingSlot = await _context.DoctorTimeSlots
                                            .FirstOrDefaultAsync(ts => ts.DoctorId == doctorId &&
                                                                      ts.AppointmentDate.Date == date.Date &&
                                                                      ts.StartTime == currentTime &&
                                                                      ts.EndTime == slotEndTime &&
                                                                      !ts.IsDeleted);

                                        if (existingSlot == null)
                                        {
                                            // بررسی وجود نوبت رزرو شده
                                            var hasExistingAppointment = await _context.Appointments
                                                .AnyAsync(a => a.DoctorId == doctorId &&
                                                             a.AppointmentDate.Date == date.Date &&
                                                             a.AppointmentDate.TimeOfDay >= currentTime &&
                                                             a.AppointmentDate.TimeOfDay < slotEndTime &&
                                                             a.Status != AppointmentStatus.Cancelled &&
                                                             !a.IsDeleted);

                                            if (!hasExistingAppointment)
                                            {
                                                generatedSlots.Add(new DoctorTimeSlot
                                                {
                                                    DoctorId = doctorId,
                                                    AppointmentDate = date,
                                                    StartTime = currentTime,
                                                    EndTime = slotEndTime,
                                                    Duration = doctorSchedule.AppointmentDuration,
                                                    Status = AppointmentStatus.Available,
                                                    CreatedAt = DateTime.Now,
                                                    CreatedByUserId = doctorSchedule.UpdatedByUserId ?? doctorSchedule.CreatedByUserId
                                                });
                                            }
                                        }
                                    }
                                }

                                currentTime = slotEndTime;
                            }
                        }
                    }
                }

                // حذف اسلات‌های قدیمی که دیگر در برنامه کاری نیستند
                var oldSlots = await _context.DoctorTimeSlots
                    .Where(ts => ts.DoctorId == doctorId &&
                               ts.AppointmentDate >= startDate &&
                               ts.AppointmentDate < endDate &&
                               ts.Status == AppointmentStatus.Available &&
                               !ts.IsDeleted)
                    .ToListAsync();

                var slotsToDelete = oldSlots.Where(oldSlot =>
                {
                    var dayOfWeek = (int)oldSlot.AppointmentDate.DayOfWeek;
                    var workDays = doctorSchedule.WorkDays?
                        .Where(wd => wd.DayOfWeek == dayOfWeek && wd.IsActive && !wd.IsDeleted)
                        .ToList() ?? new List<DoctorWorkDay>();

                    foreach (var workDay in workDays)
                    {
                        var activeTimeRanges = workDay.TimeRanges?
                            .Where(tr => tr.IsActive && !tr.IsDeleted)
                            .ToList() ?? new List<DoctorTimeRange>();

                        foreach (var timeRange in activeTimeRanges)
                        {
                            if (oldSlot.StartTime >= timeRange.StartTime &&
                                oldSlot.EndTime <= timeRange.EndTime &&
                                oldSlot.Duration == doctorSchedule.AppointmentDuration)
                            {
                                return false; // این اسلات هنوز معتبر است
                            }
                        }
                    }

                    return true; // این اسلات دیگر معتبر نیست
                }).ToList();

                if (slotsToDelete.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 🗑️ حذف {slotsToDelete.Count} اسلات قدیمی");
                    foreach (var slot in slotsToDelete)
                    {
                        slot.IsDeleted = true;
                        slot.DeletedAt = DateTime.Now;
                        slot.DeletedByUserId = doctorSchedule.UpdatedByUserId ?? doctorSchedule.CreatedByUserId;
                    }
                }

                // اضافه کردن اسلات‌های جدید
                if (generatedSlots.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ➕ اضافه کردن {generatedSlots.Count} اسلات جدید");
                    _context.DoctorTimeSlots.AddRange(generatedSlots);
                    
                    // ✅ ذخیره اسلات‌های جدید
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ {generatedSlots.Count} اسلات جدید با موفقیت ذخیره شدند");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ هیچ اسلات جدیدی برای تولید وجود ندارد");
                    // اگر اسلات‌های قدیمی حذف شده‌اند، باید SaveChanges را فراخوانی کنیم
                    if (slotsToDelete.Any())
                    {
                        await _context.SaveChangesAsync();
                        System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ حذف {slotsToDelete.Count} اسلات قدیمی با موفقیت انجام شد");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ فرآیند تولید اسلات‌های زمانی با موفقیت تکمیل شد");
            }
            catch (InvalidOperationException)
            {
                // ✅ پرتاب مجدد InvalidOperationException بدون تغییر
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ❌ خطا: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ❌ ExceptionType: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ❌ StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ❌ InnerException: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                }
                throw new InvalidOperationException($"خطا در تولید اسلات‌های زمانی برای پزشک {doctorId} و برنامه کاری {scheduleId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// بررسی تعطیلات رسمی ایران
        /// </summary>
        private bool IsPersianHoliday(DateTime date)
        {
            try
            {
                // استفاده از PersianCalendar برای تبدیل به تاریخ شمسی
                var persianCalendar = new System.Globalization.PersianCalendar();
                var year = persianCalendar.GetYear(date);
                var month = persianCalendar.GetMonth(date);
                var day = persianCalendar.GetDayOfMonth(date);

                // تعطیلات ثابت ایران (ماه/روز)
                var fixedHolidays = new[]
                {
                    (1, 1),   // نوروز
                    (1, 2),   // نوروز
                    (1, 3),   // نوروز
                    (1, 4),   // نوروز
                    (1, 12),  // روز جمهوری اسلامی
                    (1, 13),  // روز طبیعت
                    (3, 14),  // رحلت امام خمینی
                    (3, 15),  // قیام 15 خرداد
                    (11, 22), // پیروزی انقلاب اسلامی
                    (12, 29)  // ملی شدن صنعت نفت
                };

                return fixedHolidays.Contains((month, day));
            }
            catch
            {
                // در صورت خطا، فرض می‌کنیم تعطیل نیست
                return false;
            }
        }

        /// <summary>
        /// بررسی وجود ScheduleException برای یک تاریخ خاص
        /// </summary>
        private async Task<bool> HasScheduleExceptionAsync(int scheduleId, DateTime date)
        {
            try
            {
                return await _context.ScheduleExceptions
                    .AnyAsync(se => se.ScheduleId == scheduleId &&
                                   se.StartDate.Date <= date.Date &&
                                   (se.EndDate == null || se.EndDate.Value.Date >= date.Date) &&
                                   (se.Type == ExceptionType.PublicHoliday || 
                                    se.Type == ExceptionType.Holiday ||
                                    se.Type == ExceptionType.Vacation ||
                                    se.Type == ExceptionType.SickLeave) &&
                                   se.IsActive &&
                                   !se.IsDeleted);
            }
            catch
            {
                // در صورت خطا، فرض می‌کنیم استثنایی وجود ندارد
                return false;
            }
        }

        /// <summary>
        /// بررسی وجود ScheduleException جزئی برای یک بازه زمانی خاص
        /// </summary>
        private async Task<bool> HasPartialScheduleExceptionAsync(int scheduleId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                return await _context.ScheduleExceptions
                    .AnyAsync(se => se.ScheduleId == scheduleId &&
                                   se.StartDate.Date == date.Date &&
                                   (se.EndDate == null || se.EndDate.Value.Date == date.Date) &&
                                   se.StartTime.HasValue &&
                                   se.EndTime.HasValue &&
                                   // بررسی تداخل بازه زمانی
                                   se.StartTime.Value < endTime &&
                                   se.EndTime.Value > startTime &&
                                   se.IsActive &&
                                   !se.IsDeleted);
            }
            catch
            {
                // در صورت خطا، فرض می‌کنیم استثنایی وجود ندارد
                return false;
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

        #region List and Search Operations

        /// <summary>
        /// دریافت تمام برنامه‌های کاری پزشکان
        /// ✅ بهینه‌سازی شده برای Production: استفاده از AsNoTracking
        /// </summary>
        public async Task<List<DoctorSchedule>> GetAllDoctorSchedulesAsync()
        {
            try
            {
                // ✅ استفاده از AsNoTracking برای بهبود Performance (Read-Only Query)
                // توجه: فیلتر کردن WorkDays و TimeRanges در لایه Service انجام می‌شود
                return await _context.DoctorSchedules
                    .AsNoTracking() // ✅ بهبود Performance برای Read-Only Query
                    .Where(ds => !ds.IsDeleted)
                    .Include(ds => ds.Doctor)
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                    .OrderBy(ds => ds.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("خطا در دریافت تمام برنامه‌های کاری پزشکان", ex);
            }
        }

        #endregion

        #region Schedule CRUD Operations

        /// <summary>
        /// دریافت برنامه کاری بر اساس شناسه
        /// </summary>
        public async Task<DoctorSchedule> GetDoctorScheduleByIdAsync(int scheduleId)
        {
            try
            {
                // ✅ حذف .Include(ds => ds.Doctor) به دلیل خطای SQL: Invalid column name 'Doctor_DoctorId'
                // ✅ Navigation Property Doctor باید به صورت جداگانه در Service لود شود
                return await _context.DoctorSchedules
                    .Where(ds => ds.ScheduleId == scheduleId && !ds.IsDeleted)
                    // .Include(ds => ds.Doctor) // ❌ حذف شده: باعث خطای SQL می‌شود
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                    .Include(ds => ds.CreatedByUser)
                    .Include(ds => ds.UpdatedByUser)
                    .AsNoTracking() // ✅ بهبود Performance برای read-only query
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در دریافت برنامه کاری {scheduleId}", ex);
            }
        }

        /// <summary>
        /// حذف برنامه کاری
        /// </summary>
        /// <summary>
        /// حذف برنامه کاری پزشک
        /// ✅ با بررسی نوبت‌های فعال قبل از حذف
        /// </summary>
        public async Task<bool> DeleteDoctorScheduleAsync(int scheduleId)
        {
            // ✅ استفاده از Transaction برای اتمیک کردن عملیات
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var schedule = await _context.DoctorSchedules
                        .FirstOrDefaultAsync(ds => ds.ScheduleId == scheduleId && !ds.IsDeleted);

                    if (schedule == null)
                        return false;

                    // ✅ بررسی وجود نوبت‌های فعال برای این پزشک
                    var hasActiveAppointments = await HasActiveAppointmentsAsync(schedule.DoctorId);
                    if (hasActiveAppointments)
                    {
                        throw new InvalidOperationException(
                            "امکان حذف برنامه کاری به دلیل وجود نوبت‌های فعال وجود ندارد. " +
                            "لطفاً ابتدا نوبت‌های فعال را لغو یا تکمیل کنید.");
                    }

                    // ✅ حذف نرم برنامه کاری
                    schedule.IsDeleted = true;
                    schedule.DeletedAt = DateTime.Now;
                    schedule.UpdatedAt = DateTime.Now;

                    // ✅ حذف نرم WorkDays مربوطه
                    if (schedule.WorkDays != null)
                    {
                        foreach (var workDay in schedule.WorkDays.Where(wd => !wd.IsDeleted))
                        {
                            workDay.IsDeleted = true;
                            workDay.DeletedAt = DateTime.Now;
                            workDay.UpdatedAt = DateTime.Now;

                            // ✅ حذف نرم TimeRanges مربوطه
                            if (workDay.TimeRanges != null)
                            {
                                foreach (var timeRange in workDay.TimeRanges.Where(tr => !tr.IsDeleted))
                                {
                                    timeRange.IsDeleted = true;
                                    timeRange.DeletedAt = DateTime.Now;
                                    timeRange.UpdatedAt = DateTime.Now;
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    // ✅ Commit Transaction در صورت موفقیت
                    transaction.Commit();

                    return true;
                }
                catch (InvalidOperationException)
                {
                    // ✅ Rollback Transaction و پرتاب مجدد Exception
                    transaction.Rollback();
                    throw; // پرتاب مجدد همان Exception
                }
                catch (Exception ex)
                {
                    // ✅ Rollback Transaction در صورت خطا
                    transaction.Rollback();
                    throw new InvalidOperationException($"خطا در حذف برنامه کاری {scheduleId}", ex);
                }
            }
        }

        /// <summary>
        /// غیرفعال کردن برنامه کاری
        /// </summary>
        public async Task<bool> DeactivateDoctorScheduleAsync(int scheduleId)
        {
            try
            {
                var schedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(ds => ds.ScheduleId == scheduleId && !ds.IsDeleted);

                if (schedule == null)
                    return false;

                // غیرفعال کردن تمام روزهای کاری
                if (schedule.WorkDays != null)
                {
                    foreach (var workDay in schedule.WorkDays)
                    {
                        workDay.IsActive = false;
                        workDay.UpdatedAt = DateTime.Now;
                    }
                }

                schedule.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در غیرفعال کردن برنامه کاری {scheduleId}", ex);
            }
        }

        /// <summary>
        /// فعال کردن مجدد برنامه کاری
        /// </summary>
        public async Task<bool> ActivateDoctorScheduleAsync(int scheduleId)
        {
            try
            {
                var schedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(ds => ds.ScheduleId == scheduleId && !ds.IsDeleted);

                if (schedule == null)
                    return false;

                // فعال کردن تمام روزهای کاری
                if (schedule.WorkDays != null)
                {
                    foreach (var workDay in schedule.WorkDays)
                    {
                        workDay.IsActive = true;
                        workDay.UpdatedAt = DateTime.Now;
                    }
                }

                schedule.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در فعال کردن مجدد برنامه کاری {scheduleId}", ex);
            }
        }

        #endregion

    }
}

