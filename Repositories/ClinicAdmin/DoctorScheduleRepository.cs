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
using ClinicApp.Models.Entities.Appointment;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using EntityFramework.DynamicFilters;
using ClinicApp.Infrastructure; // ✅ برای ITimeProvider

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
        private readonly ITimeProvider _timeProvider; // ✅ ENTERPRISE-GRADE: برای مدیریت زمان ایران

        public DoctorScheduleRepository(ApplicationDbContext context, ITimeProvider timeProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        /// <summary>
        /// متد کمکی برای Rollback امن Transaction
        /// این متد از خطای "Value cannot be null. Parameter name: connection" جلوگیری می‌کند
        /// </summary>
        private void SafeRollback(System.Data.Entity.DbContextTransaction transaction, string methodName)
        {
            try
            {
                if (transaction != null)
                {
                    // ✅ بررسی اینکه Transaction هنوز معتبر است
                    // ✅ استفاده از try-catch برای بررسی امن Connection
                    bool canRollback = false;
                    try
                    {
                        var underlyingTransaction = transaction.UnderlyingTransaction;
                        if (underlyingTransaction != null)
                        {
                            var connection = underlyingTransaction.Connection;
                            if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                            {
                                canRollback = true;
                            }
                        }
                    }
                    catch
                    {
                        // ✅ اگر خطا در بررسی Connection رخ داد، نمی‌توانیم Rollback کنیم
                        canRollback = false;
                    }

                    if (canRollback)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"[{methodName}] ✅ Transaction با موفقیت Rollback شد");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[{methodName}] ⚠️ Transaction قبلاً Rollback شده یا Connection قطع شده");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[{methodName}] ⚠️ Transaction null است");
                }
            }
            catch (Exception rollbackEx)
            {
                // ✅ لاگ خطای Rollback اما جلوگیری از پرتاب Exception جدید
                System.Diagnostics.Debug.WriteLine($"[{methodName}] ❌ خطا در Rollback Transaction: {rollbackEx.Message}");
                System.Diagnostics.Debug.WriteLine($"[{methodName}] ❌ StackTrace: {rollbackEx.StackTrace}");
                if (rollbackEx.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[{methodName}] ❌ InnerException: {rollbackEx.InnerException.Message}");
                }
                // ✅ نادیده گرفتن خطای Rollback - Transaction احتمالاً قبلاً Rollback شده است
            }
        }

        #region Schedule Management (مدیریت برنامه کاری)

        /// <summary>
        /// دریافت برنامه کاری پزشک
        /// </summary>
        public async Task<DoctorSchedule> GetDoctorScheduleAsync(int doctorId)
        {
            try
            {
                // ✅ CRITICAL FIX: Include WorkDays برای بررسی DayOfWeek
                // ✅ استفاده از AsNoTracking() برای جلوگیری از lazy loading Navigation Properties
                // ✅ این کار از خطای SQL "Invalid column name 'Doctor_DoctorId'" جلوگیری می‌کند
                return await _context.DoctorSchedules
                    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted)
                    .Include(ds => ds.WorkDays) // ✅ CRITICAL FIX: Include WorkDays برای DayOfWeek validation
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges)) // ✅ Include TimeRanges برای validation
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
                    schedule.IsActive = schedule.IsActive; // حفظ مقدار موجود یا استفاده از پیش‌فرض (true)

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

                    // ✅ بررسی اینکه ScheduleId بعد از SaveChangesAsync مقداردهی شده است
                    if (schedule.ScheduleId <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ ScheduleId بعد از SaveChangesAsync مقداردهی نشد. ScheduleId: {schedule.ScheduleId}");
                        SafeRollback(transaction, "AddDoctorScheduleAsync");
                        throw new InvalidOperationException("خطا در ذخیره برنامه کاری: شناسه برنامه کاری تولید نشد. لطفاً دوباره تلاش کنید.");
                    }

                    System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ✅ Schedule با موفقیت ذخیره شد. ScheduleId: {schedule.ScheduleId}, DoctorId: {schedule.DoctorId}");

                    // ✅ تولید و ذخیره اسلات‌های زمانی در دیتابیس (قبل از Commit)
                    // ✅ این کار در همان Transaction انجام می‌شود تا در صورت خطا، همه چیز Rollback شود
                    System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] 🔄 شروع تولید اسلات‌های زمانی - ScheduleId: {schedule.ScheduleId}, DoctorId: {schedule.DoctorId}");
                    try
                    {
                        await GenerateAndSaveTimeSlotsAsync(schedule.DoctorId, schedule.ScheduleId);
                        System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ✅ تولید اسلات‌های زمانی با موفقیت انجام شد");
                    }
                    catch (Exception slotEx)
                    {
                        // ✅ اگر تولید اسلات‌ها با خطا مواجه شد، Transaction را Rollback می‌کنیم
                        System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ خطا در تولید اسلات‌های زمانی: {slotEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ ExceptionType: {slotEx.GetType().Name}");
                        System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ StackTrace: {slotEx.StackTrace}");
                        if (slotEx.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ InnerException: {slotEx.InnerException.GetType().Name} - {slotEx.InnerException.Message}");
                        }
                        SafeRollback(transaction, "AddDoctorScheduleAsync");
                        throw new InvalidOperationException($"خطا در تولید اسلات‌های زمانی برای برنامه کاری: {slotEx.Message}", slotEx);
                    }

                    // ✅ Commit Transaction در صورت موفقیت کامل (شامل تولید اسلات‌ها)
                    transaction.Commit();
                    System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ✅ Transaction با موفقیت Commit شد");

                    return schedule;
                }
                catch (DbUpdateException dbEx)
                {
                    // ✅ Rollback Transaction در صورت خطای دیتابیس
                    SafeRollback(transaction, "AddDoctorScheduleAsync");
                    
                    // ✅ بررسی InnerException برای جزئیات بیشتر
                    var innerEx = dbEx.InnerException;
                    var errorDetails = new System.Text.StringBuilder();
                    errorDetails.AppendLine($"خطا در افزودن برنامه کاری پزشک.");
                    
                    System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ خطای DbUpdateException - ExceptionType: {dbEx.GetType().Name}, Message: {dbEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ StackTrace: {dbEx.StackTrace}");
                    
                    while (innerEx != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ InnerException: {innerEx.GetType().Name}, Message: {innerEx.Message}");
                        if (innerEx is System.Data.SqlClient.SqlException sqlEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ SqlException - Number: {sqlEx.Number}, LineNumber: {sqlEx.LineNumber}, Procedure: {sqlEx.Procedure}");
                            
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
                    // ✅ Rollback Transaction در صورت خطا - با بررسی وضعیت Transaction
                    SafeRollback(transaction, "AddDoctorScheduleAsync");
                    
                    System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ خطای غیرمنتظره - ExceptionType: {ex.GetType().Name}, Message: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ StackTrace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AddDoctorScheduleAsync] ❌ InnerException: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                    }
                    
                    // لاگ خطا برای سیستم‌های پزشکی
                    throw new InvalidOperationException($"خطا در افزودن برنامه کاری پزشک. لطفاً دوباره تلاش کنید. اگر مشکل ادامه داشت، با بخش فنی تماس بگیرید. جزئیات خطا: {ex.Message}", ex);
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
                        SafeRollback(transaction, "UpdateDoctorScheduleAsync");
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
                    SafeRollback(transaction, "UpdateDoctorScheduleAsync");
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
                    SafeRollback(transaction, "UpdateDoctorScheduleAsync");
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
                    SafeRollback(transaction, "UpdateDoctorScheduleAsync");
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
                    SafeRollback(transaction, "UpdateDoctorScheduleAsync");
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

                // ✅ بررسی ScheduleExceptions (تعطیلات، مرخصی، و غیره)
                var doctorSchedule = await _context.DoctorSchedules
                    .Where(ds => ds.DoctorId == doctorId && !ds.IsDeleted && ds.IsActive)
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                    .FirstOrDefaultAsync();

                if (doctorSchedule == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ⚠️ برنامه کاری فعالی برای پزشک {doctorId} یافت نشد");
                    return new List<DoctorTimeSlot>();
                }

                var hasScheduleException = await HasScheduleExceptionAsync(doctorSchedule.ScheduleId, date);
                if (hasScheduleException)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ⚠️ ScheduleException برای تاریخ {date:yyyy/MM/dd} یافت شد");
                    return new List<DoctorTimeSlot>();
                }

                // ✅ CRITICAL FIX: خواندن همه اسلات‌های موجود از دیتابیس (نه فقط Available)
                // Service مسئولیت تعیین IsAvailable را دارد
                // ⚠️ NOTE: حذف فیلتر Status برای نمایش اسلات‌های booked در UI
                var existingSlots = await _context.DoctorTimeSlots
                    .Where(ts => ts.DoctorId == doctorId &&
                                DbFunctions.TruncateTime(ts.AppointmentDate) == DbFunctions.TruncateTime(date) &&
                                !ts.IsDeleted)
                    .OrderBy(ts => ts.StartTime)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ✅ {existingSlots.Count} اسلات از دیتابیس خوانده شد");

                // ✅ CRITICAL FIX: اگر هیچ اسلاتی در دیتابیس وجود ندارد، از Schedule تولید می‌کنیم
                if (!existingSlots.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ⚠️ هیچ اسلاتی در دیتابیس یافت نشد - تولید از Schedule...");
                    
                    // ✅ اطمینان از بارگذاری TimeRanges
                    if (doctorSchedule.WorkDays != null)
                    {
                        foreach (var workDay in doctorSchedule.WorkDays)
                        {
                            if (workDay != null && workDay.TimeRanges == null)
                            {
                                await _context.Entry(workDay)
                                    .Collection(wd => wd.TimeRanges)
                                    .LoadAsync();
                            }
                        }
                    }

                    // ✅ تولید اسلات‌ها از Schedule
                    // ✅ CRITICAL FIX: تبدیل C# DayOfWeek به دیتابیس DayOfWeek
                    // در C#: Sunday=0, Monday=1, Tuesday=2, Wednesday=3, Thursday=4, Friday=5, Saturday=6
                    // در دیتابیس: یکشنبه=0, دوشنبه=1, سه‌شنبه=2, چهارشنبه=3, پنج‌شنبه=4, جمعه=5, شنبه=6
                    // تبدیل: Sunday(0) → یکشنبه(0), Monday(1) → دوشنبه(1), ..., Saturday(6) → شنبه(6)
                    // پس: dayOfWeek در C# = dayOfWeek در دیتابیس (بدون تبدیل)
                    var cSharpDayOfWeek = (int)date.DayOfWeek;
                    var dbDayOfWeek = cSharpDayOfWeek; // ✅ بدون تبدیل - یکسان هستند
                    
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] 📅 تاریخ: {date:yyyy/MM/dd}, CSharpDayOfWeek: {cSharpDayOfWeek} ({(DayOfWeek)cSharpDayOfWeek}), DbDayOfWeek: {dbDayOfWeek}");
                    
                    var workDays = doctorSchedule.WorkDays?
                        .Where(wd => wd != null && wd.DayOfWeek == dbDayOfWeek && wd.IsActive && !wd.IsDeleted)
                        .ToList() ?? new List<DoctorWorkDay>();
                    
                    System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ✅ {workDays.Count} WorkDay برای DayOfWeek {dbDayOfWeek} یافت شد");

                    if (workDays.Any())
                    {
                        // ✅ دریافت ScheduleExceptions و booked appointments برای تولید اسلات
                        var allScheduleExceptions = await _context.ScheduleExceptions
                            .Where(se => se.ScheduleId == doctorSchedule.ScheduleId &&
                                        DbFunctions.TruncateTime(se.StartDate) <= DbFunctions.TruncateTime(date) &&
                                        (se.EndDate == null || DbFunctions.TruncateTime(se.EndDate.Value) >= DbFunctions.TruncateTime(date)) &&
                                        se.IsActive && !se.IsDeleted)
                            .ToListAsync();

                        var bookedAppointmentsInRange = await _context.Appointments
                            .Where(a => a.DoctorId == doctorId &&
                                       DbFunctions.TruncateTime(a.AppointmentDate) == DbFunctions.TruncateTime(date) &&
                                       a.Status != AppointmentStatus.Cancelled &&
                                       !a.IsDeleted)
                            .ToListAsync();

                        // ✅ تولید اسلات‌ها
                        var generatedSlots = await GenerateSlotsForDateAsync(
                            date,
                            workDays,
                            doctorSchedule,
                            doctorSchedule.ScheduleId,
                            doctorId,
                            allScheduleExceptions,
                            new List<DoctorTimeSlot>(), // existingSlotsInRange (خالی است)
                            bookedAppointmentsInRange);

                        System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ✅ {generatedSlots.Count} اسلات از Schedule تولید شد (فقط برای نمایش، بدون ذخیره در DB)");

                        // ✅ تولید اسلات فقط برای نمایش (Read-Only): ذخیره در دیتابیس انجام نمی‌شود.
                        // دلیل: جلوگیری از ایجاد ناخواسته اسلات‌ها هنگام بازدید بیمار از صفحه «نوبت‌های موجود» (Available).
                        // اسلات‌ها باید فقط از طریق ادمین (تولید برنامه) یا در زمان رزرو واقعی ایجاد شوند.
                        if (generatedSlots.Any())
                        {
                            existingSlots = generatedSlots;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ⚠️ هیچ WorkDay فعالی برای DayOfWeek {dbDayOfWeek} یافت نشد");
                    }
                }

                // ✅ CRITICAL FIX: Repository باید همه اسلات‌ها را برگرداند (نه فقط available)
                // Service مسئولیت تعیین IsAvailable را دارد
                // ⚠️ NOTE: این تغییر برای نمایش اسلات‌های booked در UI است
                // منطق دقیق Overlap با Duration در Service انجام می‌شود
                
                System.Diagnostics.Debug.WriteLine($"[GetAvailableAppointmentSlotsAsync] ✅ {existingSlots.Count} اسلات برگردانده می‌شود (همه اسلات‌ها، نه فقط available)");

                return existingSlots;
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
        /// تولید و ذخیره اسلات‌های زمانی در دیتابیس برای یک تاریخ خاص
        /// این متد هنگام ایجاد یا به‌روزرسانی برنامه کاری فراخوانی می‌شود
        /// 
        /// ✅ منطق: برنامه هفتگی است و منشی برای تاریخ‌های خاص برنامه تنظیم می‌کند
        /// - منشی می‌تواند برای هفته آینده یا تاریخ‌های خاص (مثلاً 25-26) برنامه تنظیم کند
        /// - اسلات‌ها فقط برای همان تاریخ خاص تولید می‌شوند (نه برای چند هفته آینده)
        /// - اگر targetDate مشخص نشده باشد، اولین روز کاری آینده (در 7 روز آینده) استفاده می‌شود
        /// 
        /// ✅ رعایت تقویم شمسی: شنبه = اولین روز هفته (مطابق time.ir)
        /// ✅ On-Demand Generation: برای تاریخ‌های دیگر، اسلات‌ها در `GetAvailableAppointmentSlotsAsync` تولید می‌شوند
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="scheduleId">شناسه برنامه کاری</param>
        /// <param name="targetDate">تاریخ هدف برای تولید اسلات (null = اولین روز کاری آینده)</param>
        public async Task GenerateAndSaveTimeSlotsAsync(int doctorId, int scheduleId, DateTime? targetDate = null)
        {
            // ✅ Transaction Management: بررسی اینکه آیا از قبل یک transaction وجود دارد یا نه
            // ✅ اگر از داخل یک transaction فراخوانی شده باشد (مثل AddDoctorScheduleAsync)، از همان استفاده می‌کنیم
            // ✅ در غیر این صورت، یک transaction جدید ایجاد می‌کنیم
            var existingTransaction = _context.Database.CurrentTransaction;
            var shouldCommitTransaction = existingTransaction == null;
            System.Data.Entity.DbContextTransaction transaction = null;

            if (shouldCommitTransaction)
            {
                transaction = _context.Database.BeginTransaction();
                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ Transaction جدید ایجاد شد");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ استفاده از Transaction موجود");
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 🔍 شروع - DoctorId: {doctorId}, ScheduleId: {scheduleId}, TargetDate: {targetDate?.ToString("yyyy/MM/dd") ?? "null (اولین روز کاری)"}");

                // دریافت برنامه کاری با جزئیات
                // ✅ حذف شرط ds.IsActive از query برای اجازه تولید اسلات حتی اگر IsActive = false باشد
                // ✅ این کار برای اجازه تولید اسلات در زمان ایجاد برنامه کاری است
                // ✅ مهم: باید TimeRanges را به درستی بارگذاری کنیم تا منطق حذف اسلات‌های قدیمی درست کار کند
                var doctorSchedule = await _context.DoctorSchedules
                    .Where(ds => ds.ScheduleId == scheduleId && ds.DoctorId == doctorId && !ds.IsDeleted)
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                    .FirstOrDefaultAsync();

                // ✅ اطمینان از بارگذاری TimeRanges - اگر null باشند، از دیتابیس بارگذاری می‌کنیم
                if (doctorSchedule != null && doctorSchedule.WorkDays != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 🔍 بررسی بارگذاری TimeRanges - تعداد WorkDays: {doctorSchedule.WorkDays.Count}");
                    foreach (var workDay in doctorSchedule.WorkDays)
                    {
                        if (workDay != null)
                        {
                            if (workDay.TimeRanges == null)
                            {
                                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ WorkDay {workDay.WorkDayId} (DayOfWeek: {workDay.DayOfWeek}) دارای TimeRanges null است - بارگذاری دستی...");
                                // ✅ بارگذاری دستی TimeRanges در صورت نیاز
                                await _context.Entry(workDay)
                                    .Collection(wd => wd.TimeRanges)
                                    .LoadAsync();
                                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ TimeRanges بارگذاری شد - تعداد: {workDay.TimeRanges?.Count ?? 0}");
                            }
                            else
                            {
                                var activeTimeRangesCount = workDay.TimeRanges?.Count(tr => tr != null && tr.IsActive && !tr.IsDeleted) ?? 0;
                                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ WorkDay {workDay.WorkDayId} (DayOfWeek: {workDay.DayOfWeek}) دارای {workDay.TimeRanges.Count} TimeRange (فعال: {activeTimeRangesCount})");
                                
                                // ✅ لاگ جزئیات TimeRanges
                                if (workDay.TimeRanges != null && workDay.TimeRanges.Any())
                                {
                                    foreach (var tr in workDay.TimeRanges.Where(t => t != null && t.IsActive && !t.IsDeleted))
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync]   ⏰ TimeRange {tr.TimeRangeId}: {tr.StartTime} - {tr.EndTime} (فعال: {tr.IsActive}, حذف نشده: {!tr.IsDeleted})");
                                    }
                                }
                            }
                        }
                    }
                }

                if (doctorSchedule == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ❌ برنامه کاری یافت نشد - ScheduleId: {scheduleId}, DoctorId: {doctorId}");
                    throw new InvalidOperationException($"برنامه کاری با شناسه {scheduleId} برای پزشک {doctorId} یافت نشد.");
                }

                // ✅ اگر برنامه کاری غیرفعال است، فقط هشدار می‌دهیم اما ادامه می‌دهیم
                if (!doctorSchedule.IsActive)
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ برنامه کاری غیرفعال است - ScheduleId: {scheduleId}, اما اسلات‌ها تولید می‌شوند");
                }

                // ✅ بررسی وجود WorkDays
                if (doctorSchedule.WorkDays == null || !doctorSchedule.WorkDays.Any(wd => wd.IsActive && !wd.IsDeleted))
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ هیچ WorkDay فعالی برای این برنامه کاری وجود ندارد");
                    // این یک هشدار است، نه خطا - ممکن است پزشک هنوز روزهای کاری را تنظیم نکرده باشد
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ برنامه کاری یافت شد - WorkDaysCount: {doctorSchedule.WorkDays?.Count(wd => wd.IsActive && !wd.IsDeleted) ?? 0}");

                // ✅ منطق: اگر targetDate مشخص شده باشد، از آن استفاده می‌کنیم (منشی تاریخ خاص را انتخاب کرده)
                // ✅ در غیر این صورت، اولین روز کاری آینده را پیدا می‌کنیم
                DateTime? dateToGenerate = targetDate;
                
                if (!dateToGenerate.HasValue)
                {
                    // ✅ پیدا کردن اولین روز کاری برای تولید اسلات
                    // ✅ اگر امروز روز کاری است، برای امروز اسلات تولید می‌شود
                    // ✅ اگر امروز روز کاری نیست، برای اولین روز کاری آینده (در 7 روز آینده) اسلات تولید می‌شود
                    dateToGenerate = await FindFirstWorkDayForScheduleAsync(doctorSchedule, _timeProvider.GetIranToday());
                    
                    if (!dateToGenerate.HasValue)
                    {
                        System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ هیچ روز کاری فعالی در 7 روز آینده یافت نشد - اسلات تولید نمی‌شود");
                        return;
                    }
                }

                var startDate = dateToGenerate.Value.Date; // ✅ فقط بخش تاریخ (بدون زمان)
                var generatedSlots = new List<DoctorTimeSlot>();

                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 📅 تولید اسلات‌ها برای تاریخ {startDate:yyyy/MM/dd} (روز کاری: {(DayOfWeek)startDate.DayOfWeek})");

                // ✅ بهینه‌سازی: دریافت تمام ScheduleExceptions برای همان روز خاص به صورت batch (جلوگیری از N+1 Query)
                // ✅ استفاده از DbFunctions.TruncateTime برای مقایسه تاریخ در LINQ to Entities
                var startDateOnly = startDate.Date; // برای استفاده در ToListAsync
                var allScheduleExceptions = await _context.ScheduleExceptions
                    .Where(se => se.ScheduleId == scheduleId &&
                                DbFunctions.TruncateTime(se.StartDate) <= DbFunctions.TruncateTime(startDate) &&
                                (se.EndDate == null || DbFunctions.TruncateTime(se.EndDate.Value) >= DbFunctions.TruncateTime(startDate)) &&
                                se.IsActive &&
                                !se.IsDeleted)
                    .ToListAsync();

                // ✅ بهینه‌سازی: دریافت تمام اسلات‌های موجود برای همان روز خاص به صورت batch
                // ✅ استفاده از DbFunctions.TruncateTime برای مقایسه تاریخ در LINQ to Entities
                var existingSlotsInRange = await _context.DoctorTimeSlots
                    .Where(ts => ts.DoctorId == doctorId &&
                               DbFunctions.TruncateTime(ts.AppointmentDate) == DbFunctions.TruncateTime(startDate) &&
                               !ts.IsDeleted)
                    .ToListAsync();

                // ✅ بهینه‌سازی: دریافت تمام نوبت‌های رزرو شده برای همان روز خاص به صورت batch
                // ✅ استفاده از DbFunctions.TruncateTime برای مقایسه تاریخ در LINQ to Entities
                var bookedAppointmentsInRange = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId &&
                               DbFunctions.TruncateTime(a.AppointmentDate) == DbFunctions.TruncateTime(startDate) &&
                               a.Status != AppointmentStatus.Cancelled &&
                               !a.IsDeleted)
                    .ToListAsync();

                // ✅ منطق جدید: فقط برای همان روز خاص اسلات تولید می‌شود
                var date = startDate;
                
                // ✅ بررسی تعطیلات رسمی
                if (IsPersianHoliday(date))
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ تاریخ {date:yyyy/MM/dd} تعطیل رسمی است - اسلات تولید نمی‌شود");
                    return;
                }

                // ✅ بررسی ScheduleExceptions (استفاده از لیست از پیش بارگذاری شده)
                // ✅ مقایسه تاریخ‌ها در حافظه (پس از ToListAsync)
                var dateOnly = date.Date;
                var hasScheduleException = allScheduleExceptions.Any(se =>
                    se.StartDate.Date <= dateOnly &&
                    (se.EndDate == null || se.EndDate.Value.Date >= dateOnly) &&
                    (!se.StartTime.HasValue || !se.EndTime.HasValue)); // استثنای تمام روز

                if (hasScheduleException)
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ تاریخ {date:yyyy/MM/dd} دارای ScheduleException است - اسلات تولید نمی‌شود");
                    return;
                }

                var dayOfWeek = (int)date.DayOfWeek;
                var workDays = doctorSchedule.WorkDays?
                    .Where(wd => wd != null && wd.DayOfWeek == dayOfWeek && wd.IsActive && !wd.IsDeleted)
                    .ToList() ?? new List<DoctorWorkDay>();

                // ✅ بررسی دقیق: اگر هیچ WorkDay فعالی برای این DayOfWeek وجود ندارد، اسلات تولید نکن
                if (!workDays.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ هیچ WorkDay فعالی برای DayOfWeek {dayOfWeek} ({(DayOfWeek)dayOfWeek}) در تاریخ {date:yyyy/MM/dd} یافت نشد - اسلات تولید نمی‌شود");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ {workDays.Count} WorkDay فعال برای DayOfWeek {dayOfWeek} ({(DayOfWeek)dayOfWeek}) در تاریخ {date:yyyy/MM/dd} یافت شد");

                // ✅ تولید اسلات‌ها برای این تاریخ (با استفاده از متد جداگانه برای رعایت SRP)
                var slotsForDate = await GenerateSlotsForDateAsync(
                    date, 
                    workDays, 
                    doctorSchedule, 
                    scheduleId, 
                    doctorId, 
                    allScheduleExceptions, 
                    existingSlotsInRange, 
                    bookedAppointmentsInRange);

                generatedSlots.AddRange(slotsForDate);

                // ✅ حذف اسلات‌های قدیمی که دیگر در برنامه کاری نیستند (فقط برای همان روز خاص)
                // ✅ مهم: باید تمام اسلات‌های موجود را بررسی کنیم (نه فقط Available) تا اسلات‌های Booked که دیگر معتبر نیستند هم حذف شوند
                // ✅ استفاده از DbFunctions.TruncateTime برای مقایسه تاریخ در LINQ to Entities
                var oldSlots = await _context.DoctorTimeSlots
                    .Where(ts => ts.DoctorId == doctorId &&
                               DbFunctions.TruncateTime(ts.AppointmentDate) == DbFunctions.TruncateTime(startDate) &&
                               !ts.IsDeleted)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 🔍 بررسی {oldSlots.Count} اسلات قدیمی برای حذف");

                // ✅ استفاده از همان لیست ScheduleExceptions که قبلاً بارگذاری شده (جلوگیری از query تکراری)
                // allScheduleExceptions قبلاً در خط 1205 بارگذاری شده است

                // ✅ فیلتر کردن اسلات‌های قدیمی که باید حذف شوند (با استفاده از منطق synchronous برای جلوگیری از async در Where)
                var slotsToDelete = new List<DoctorTimeSlot>();
                var slotsToKeep = new List<DoctorTimeSlot>();
                foreach (var oldSlot in oldSlots)
                {
                    if (ShouldDeleteOldSlot(oldSlot, doctorSchedule, allScheduleExceptions))
                    {
                        System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 🗑️ اسلات {oldSlot.TimeSlotId} برای تاریخ {oldSlot.AppointmentDate:yyyy/MM/dd} ساعت {oldSlot.StartTime}-{oldSlot.EndTime} حذف می‌شود");
                        slotsToDelete.Add(oldSlot);
                    }
                    else
                    {
                        slotsToKeep.Add(oldSlot);
                        System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ اسلات {oldSlot.TimeSlotId} برای تاریخ {oldSlot.AppointmentDate:yyyy/MM/dd} ساعت {oldSlot.StartTime}-{oldSlot.EndTime} معتبر است و نگه داشته می‌شود");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 📊 خلاصه: {slotsToDelete.Count} اسلات برای حذف، {slotsToKeep.Count} اسلات معتبر");

                if (slotsToDelete.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 🗑️ حذف {slotsToDelete.Count} اسلات قدیمی (Soft Delete)");
                    var deletedByUserId = doctorSchedule.UpdatedByUserId ?? doctorSchedule.CreatedByUserId;
                    var deletedAt = DateTime.Now;
                    
                    // ✅ بهینه‌سازی: Soft Delete برای تمام اسلات‌ها
                    // ✅ در EF6، entity های خوانده شده از دیتابیس به صورت خودکار tracked می‌شوند
                    // ✅ پس نیازی به Attach یا UpdateRange نیست - فقط تغییرات را اعمال می‌کنیم
                    foreach (var slot in slotsToDelete)
                    {
                        slot.IsDeleted = true;
                        slot.DeletedAt = deletedAt;
                        slot.DeletedByUserId = deletedByUserId;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ {slotsToDelete.Count} اسلات قدیمی با Soft Delete علامت‌گذاری شدند (ذخیره در SaveChanges)");
                }

                // ✅ CRITICAL FIX: حذف اسلات‌های قدیمی قبل از اضافه کردن اسلات‌های جدید
                // ✅ این کار برای جلوگیری از اسلات‌های تکراری و تضمین یکپارچگی داده‌ها ضروری است
                if (slotsToDelete.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] 🗑️ حذف {slotsToDelete.Count} اسلات قدیمی قبل از اضافه کردن اسلات جدید");
                    // ✅ ذخیره تغییرات حذف قبل از اضافه کردن اسلات جدید
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ {slotsToDelete.Count} اسلات قدیمی با موفقیت حذف شدند");
                }

                // ✅ CRITICAL FIX: فیلتر کردن اسلات‌های جدید برای جلوگیری از تکراری
                // ✅ بررسی اینکه آیا اسلات جدید با اسلات‌های موجود (که حذف نشده‌اند) تکراری است یا نه
                var slotsToAdd = new List<DoctorTimeSlot>();
                foreach (var newSlot in generatedSlots)
                {
                    // ✅ بررسی اینکه آیا این اسلات با اسلات‌های موجود (که حذف نشده‌اند) تکراری است
                    var isDuplicate = slotsToKeep.Any(ks =>
                        ks.DoctorId == newSlot.DoctorId &&
                        ks.AppointmentDate.Date == newSlot.AppointmentDate.Date &&
                        ks.StartTime == newSlot.StartTime &&
                        ks.EndTime == newSlot.EndTime &&
                        ks.Duration == newSlot.Duration &&
                        !ks.IsDeleted);
                    
                    if (!isDuplicate)
                    {
                        slotsToAdd.Add(newSlot);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ اسلات تکراری نادیده گرفته شد - StartTime: {newSlot.StartTime}, EndTime: {newSlot.EndTime}, Duration: {newSlot.Duration}");
                    }
                }

                // اضافه کردن اسلات‌های جدید (فقط اسلات‌های غیرتکراری)
                if (slotsToAdd.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ➕ اضافه کردن {slotsToAdd.Count} اسلات جدید (از {generatedSlots.Count} اسلات تولید شده، {generatedSlots.Count - slotsToAdd.Count} اسلات تکراری نادیده گرفته شد)");
                    _context.DoctorTimeSlots.AddRange(slotsToAdd);
                    
                    // ✅ ذخیره اسلات‌های جدید
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ {slotsToAdd.Count} اسلات جدید با موفقیت ذخیره شدند");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ⚠️ هیچ اسلات جدیدی برای اضافه کردن وجود ندارد (همه تکراری بودند یا قبلاً حذف شدند)");
                }

                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ فرآیند تولید اسلات‌های زمانی با موفقیت تکمیل شد");

                // ✅ Commit Transaction فقط در صورتی که خودمان آن را ایجاد کرده‌ایم
                if (shouldCommitTransaction && transaction != null)
                {
                    transaction.Commit();
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ✅ Transaction با موفقیت Commit شد");
                }
            }
            catch (InvalidOperationException)
            {
                // ✅ Rollback Transaction فقط در صورتی که خودمان آن را ایجاد کرده‌ایم
                // ✅ اگر از transaction موجود استفاده می‌کنیم، rollback را به caller واگذار می‌کنیم
                if (shouldCommitTransaction && transaction != null)
                {
                    SafeRollback(transaction, "GenerateAndSaveTimeSlotsAsync");
                }
                // ✅ پرتاب مجدد InvalidOperationException بدون تغییر
                throw;
            }
            catch (Exception ex)
            {
                // ✅ Rollback Transaction فقط در صورتی که خودمان آن را ایجاد کرده‌ایم
                // ✅ اگر از transaction موجود استفاده می‌کنیم، rollback را به caller واگذار می‌کنیم
                if (shouldCommitTransaction && transaction != null)
                {
                    SafeRollback(transaction, "GenerateAndSaveTimeSlotsAsync");
                }
                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ❌ خطا: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ❌ ExceptionType: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ❌ StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateAndSaveTimeSlotsAsync] ❌ InnerException: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                }
                throw new InvalidOperationException($"خطا در تولید اسلات‌های زمانی برای پزشک {doctorId} و برنامه کاری {scheduleId}: {ex.Message}", ex);
            }
            finally
            {
                // ✅ Dispose Transaction فقط در صورتی که خودمان آن را ایجاد کرده‌ایم
                if (shouldCommitTransaction && transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }

        /// <summary>
        /// بررسی اینکه آیا اسلات قدیمی باید حذف شود یا نه
        /// ✅ SRP: این متد فقط مسئولیت بررسی حذف اسلات را دارد
        /// </summary>
        /// <param name="oldSlot">اسلات قدیمی برای بررسی</param>
        /// <param name="doctorSchedule">برنامه کاری پزشک</param>
        /// <param name="scheduleExceptions">لیست ScheduleExceptions برای بازه زمانی (برای جلوگیری از N+1 Query)</param>
        /// <returns>true اگر باید حذف شود، false در غیر این صورت</returns>
        private bool ShouldDeleteOldSlot(DoctorTimeSlot oldSlot, DoctorSchedule doctorSchedule, List<ScheduleException> scheduleExceptions)
        {
            // ✅ Null Safety: بررسی null بودن ورودی‌ها
            if (oldSlot == null || doctorSchedule == null)
            {
                return false; // اگر داده‌ها null باشند، حذف نکن
            }

            // ✅ بررسی تعطیلات رسمی
            if (IsPersianHoliday(oldSlot.AppointmentDate))
            {
                return true; // حذف شود
            }

            // ✅ بررسی ScheduleExceptions (استفاده از لیست از پیش بارگذاری شده - در memory)
            // ✅ استانداردهای پزشکی: اطمینان از عدم تداخل با زمان‌های بلاک شده
            var slotDate = oldSlot.AppointmentDate.Date;
            var hasException = scheduleExceptions != null && scheduleExceptions.Any(se =>
                se != null &&
                !se.IsDeleted && // ✅ فقط استثناهای حذف نشده
                se.StartDate.Date <= slotDate &&
                (se.EndDate == null || se.EndDate.Value.Date >= slotDate) &&
                (!se.StartTime.HasValue || !se.EndTime.HasValue || // استثنای تمام روز
                 (se.StartTime.Value <= oldSlot.StartTime && se.EndTime.Value >= oldSlot.EndTime))); // استثنای جزئی - اسلات باید کاملاً درون استثنا باشد

            if (hasException)
            {
                System.Diagnostics.Debug.WriteLine($"[ShouldDeleteOldSlot] 🗑️ اسلات {oldSlot.TimeSlotId} حذف می‌شود - در زمان بلاک شده (ScheduleException) قرار دارد");
                return true; // حذف شود - در زمان بلاک شده قرار دارد
            }

            // ✅ بررسی DayOfWeek
            var dayOfWeek = (int)oldSlot.AppointmentDate.DayOfWeek;
            var workDays = doctorSchedule.WorkDays?
                .Where(wd => wd != null && wd.DayOfWeek == dayOfWeek && wd.IsActive && !wd.IsDeleted)
                .ToList() ?? new List<DoctorWorkDay>();

            // ✅ اگر هیچ WorkDay فعالی برای این DayOfWeek وجود ندارد، اسلات حذف شود
            if (!workDays.Any())
            {
                return true; // حذف شود - این روز دیگر روز کاری نیست
            }

            // ✅ بررسی TimeRange - بررسی دقیق‌تر برای اطمینان از حذف اسلات‌های خارج از بازه
            bool isSlotValid = false;
            foreach (var workDay in workDays)
            {
                if (workDay?.TimeRanges == null)
                    continue;

                var activeTimeRanges = workDay.TimeRanges
                    .Where(tr => tr != null && tr.IsActive && !tr.IsDeleted)
                    .ToList();

                foreach (var timeRange in activeTimeRanges)
                {
                    if (timeRange == null)
                        continue;

                    // ✅ بررسی دقیق: اسلات باید کاملاً درون TimeRange باشد
                    // ✅ StartTime اسلات باید >= StartTime TimeRange
                    // ✅ EndTime اسلات باید <= EndTime TimeRange
                    // ✅ CRITICAL FIX: Duration اسلات باید برابر با AppointmentDuration فعلی باشد
                    // ✅ اگر Duration تغییر کرده باشد، اسلات قدیمی باید حذف شود
                    if (oldSlot.StartTime >= timeRange.StartTime &&
                        oldSlot.EndTime <= timeRange.EndTime &&
                        oldSlot.Duration == doctorSchedule.AppointmentDuration) // ✅ بررسی Duration برای جلوگیری از اسلات‌های با Duration نادرست
                    {
                        // ✅ این اسلات در یک TimeRange معتبر قرار دارد
                        isSlotValid = true;
                        System.Diagnostics.Debug.WriteLine($"[ShouldDeleteOldSlot] ✅ اسلات {oldSlot.TimeSlotId} معتبر است - StartTime: {oldSlot.StartTime}, EndTime: {oldSlot.EndTime}, TimeRange: {timeRange.StartTime}-{timeRange.EndTime}");
                        break; // نیازی به بررسی بیشتر نیست
                    }
                    else
                    {
                        // ✅ CRITICAL FIX: اگر Duration متفاوت باشد، اسلات باید حذف شود
                        if (oldSlot.Duration != doctorSchedule.AppointmentDuration)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ShouldDeleteOldSlot] 🗑️ اسلات {oldSlot.TimeSlotId} حذف می‌شود - Duration متفاوت است: {oldSlot.Duration} (انتظار: {doctorSchedule.AppointmentDuration})");
                            return true; // حذف شود - Duration تغییر کرده است
                        }
                        System.Diagnostics.Debug.WriteLine($"[ShouldDeleteOldSlot] ⚠️ اسلات {oldSlot.TimeSlotId} در TimeRange {timeRange.StartTime}-{timeRange.EndTime} قرار ندارد - StartTime: {oldSlot.StartTime}, EndTime: {oldSlot.EndTime}, Duration: {oldSlot.Duration}, ExpectedDuration: {doctorSchedule.AppointmentDuration}");
                    }
                }

                if (isSlotValid)
                    break; // اگر اسلات معتبر است، نیازی به بررسی WorkDay های دیگر نیست
            }

            // ✅ اگر اسلات در هیچ TimeRange معتبری قرار نگرفت، باید حذف شود
            if (!isSlotValid)
            {
                System.Diagnostics.Debug.WriteLine($"[ShouldDeleteOldSlot] 🗑️ اسلات {oldSlot.TimeSlotId} حذف می‌شود - در هیچ TimeRange معتبری قرار ندارد");
                return true; // این اسلات دیگر معتبر نیست
            }

            return false; // این اسلات هنوز معتبر است
        }

        /// <summary>
        /// پیدا کردن اولین روز کاری برای برنامه کاری
        /// ✅ SRP: این متد فقط مسئولیت پیدا کردن اولین روز کاری را دارد
        /// 
        /// منطق:
        /// - اگر امروز روز کاری است، امروز را برمی‌گرداند
        /// - اگر امروز روز کاری نیست، اولین روز کاری آینده (در 7 روز آینده) را برمی‌گرداند
        /// - اگر هیچ روز کاری فعالی در 7 روز آینده وجود نداشته باشد، null برمی‌گرداند
        /// </summary>
        /// <param name="doctorSchedule">برنامه کاری پزشک</param>
        /// <param name="startDate">تاریخ شروع جستجو (معمولاً امروز)</param>
        /// <returns>اولین روز کاری یا null اگر یافت نشد</returns>
        private Task<DateTime?> FindFirstWorkDayForScheduleAsync(DoctorSchedule doctorSchedule, DateTime startDate)
        {
            if (doctorSchedule?.WorkDays == null)
                return Task.FromResult<DateTime?>(null);

            var activeWorkDays = doctorSchedule.WorkDays
                .Where(wd => wd != null && wd.IsActive && !wd.IsDeleted)
                .ToList();

            if (!activeWorkDays.Any())
                return Task.FromResult<DateTime?>(null);

            // ✅ بررسی 7 روز آینده برای پیدا کردن اولین روز کاری
            for (int i = 0; i < 7; i++)
            {
                var checkDate = startDate.AddDays(i);
                
                // ✅ بررسی تعطیلات رسمی
                if (IsPersianHoliday(checkDate))
                    continue;

                var dayOfWeek = (int)checkDate.DayOfWeek;
                var hasWorkDay = activeWorkDays.Any(wd => wd.DayOfWeek == dayOfWeek);

                if (hasWorkDay)
                {
                    System.Diagnostics.Debug.WriteLine($"[FindFirstWorkDayForScheduleAsync] ✅ اولین روز کاری یافت شد: {checkDate:yyyy/MM/dd} ({(DayOfWeek)dayOfWeek})");
                    return Task.FromResult<DateTime?>(checkDate);
                }
            }

            System.Diagnostics.Debug.WriteLine($"[FindFirstWorkDayForScheduleAsync] ⚠️ هیچ روز کاری فعالی در 7 روز آینده یافت نشد");
            return Task.FromResult<DateTime?>(null);
        }

        /// <summary>
        /// تولید اسلات‌های زمانی برای یک تاریخ خاص
        /// ✅ SRP: این متد فقط مسئولیت تولید اسلات‌ها برای یک تاریخ را دارد
        /// ✅ بهینه‌سازی: استفاده از لیست‌های از پیش بارگذاری شده برای جلوگیری از N+1 Query
        /// </summary>
        /// <param name="date">تاریخ برای تولید اسلات</param>
        /// <param name="workDays">روزهای کاری فعال برای این تاریخ</param>
        /// <param name="doctorSchedule">برنامه کاری پزشک</param>
        /// <param name="scheduleId">شناسه برنامه کاری</param>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="allScheduleExceptions">لیست تمام ScheduleExceptions برای بازه زمانی</param>
        /// <param name="existingSlotsInRange">لیست تمام اسلات‌های موجود در بازه زمانی</param>
        /// <param name="bookedAppointmentsInRange">لیست تمام نوبت‌های رزرو شده در بازه زمانی</param>
        /// <returns>لیست اسلات‌های تولید شده برای این تاریخ</returns>
        private Task<List<DoctorTimeSlot>> GenerateSlotsForDateAsync(
            DateTime date,
            List<DoctorWorkDay> workDays,
            DoctorSchedule doctorSchedule,
            int scheduleId,
            int doctorId,
            List<ScheduleException> allScheduleExceptions,
            List<DoctorTimeSlot> existingSlotsInRange,
            List<Models.Entities.Appointment.Appointment> bookedAppointmentsInRange)
        {
            var slotsForDate = new List<DoctorTimeSlot>();
            var dateOnly = date.Date;

            foreach (var workDay in workDays)
            {
                if (workDay?.TimeRanges == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateSlotsForDateAsync] ⚠️ WorkDay {workDay?.WorkDayId} برای DayOfWeek {workDay?.DayOfWeek} دارای TimeRanges null است - نادیده گرفته می‌شود");
                    continue;
                }

                var activeTimeRanges = workDay.TimeRanges
                    .Where(tr => tr != null && tr.IsActive && !tr.IsDeleted)
                    .ToList();

                if (!activeTimeRanges.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[GenerateSlotsForDateAsync] ⚠️ WorkDay {workDay.WorkDayId} برای DayOfWeek {workDay.DayOfWeek} هیچ TimeRange فعالی ندارد - نادیده گرفته می‌شود");
                    continue;
                }

                System.Diagnostics.Debug.WriteLine($"[GenerateSlotsForDateAsync] ✅ WorkDay {workDay.WorkDayId} برای DayOfWeek {workDay.DayOfWeek} دارای {activeTimeRanges.Count} TimeRange فعال است");

                foreach (var timeRange in activeTimeRanges)
                {
                    if (timeRange == null)
                        continue;

                    var currentTime = timeRange.StartTime;
                    var endTime = timeRange.EndTime;

                    System.Diagnostics.Debug.WriteLine($"[GenerateSlotsForDateAsync] 🔍 شروع تولید اسلات برای TimeRange {timeRange.TimeRangeId} - StartTime: {currentTime}, EndTime: {endTime}, AppointmentDuration: {doctorSchedule.AppointmentDuration} دقیقه");

                    var slotsCreatedForThisTimeRange = 0;
                    while (currentTime < endTime)
                    {
                        var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(doctorSchedule.AppointmentDuration));

                        // ✅ بررسی اولیه: اگر slotEndTime > endTime، حلقه را متوقف کن (جلوگیری از تولید اسلات خارج از بازه)
                        if (slotEndTime > endTime)
                        {
                            System.Diagnostics.Debug.WriteLine($"[GenerateSlotsForDateAsync] ⚠️ اسلات خارج از TimeRange است - StartTime: {currentTime}, SlotEndTime: {slotEndTime}, TimeRangeEnd: {endTime} - حلقه متوقف می‌شود");
                            break; // ✅ توقف حلقه به جای ادامه - این رفع باگ اصلی است
                        }

                        // ✅ بررسی دقیق: اسلات باید کاملاً درون TimeRange باشد
                        if (slotEndTime <= endTime)
                        {
                            // ✅ بررسی ScheduleExceptions جزئی (استفاده از لیست از پیش بارگذاری شده)
                            // ✅ استانداردهای پزشکی: اطمینان از عدم تداخل با زمان‌های بلاک شده
                            // ✅ allScheduleExceptions قبلاً با فیلتر !se.IsDeleted بارگذاری شده است
                            var hasPartialException = allScheduleExceptions != null && allScheduleExceptions.Any(se =>
                                se != null &&
                                se.StartDate.Date == dateOnly &&
                                (se.EndDate == null || se.EndDate.Value.Date == dateOnly) &&
                                se.StartTime.HasValue &&
                                se.EndTime.HasValue &&
                                se.StartTime.Value < slotEndTime &&
                                se.EndTime.Value > currentTime);

                            if (!hasPartialException)
                            {
                                // ✅ بررسی وجود اسلات در دیتابیس (استفاده از لیست از پیش بارگذاری شده)
                                // ✅ CRITICAL FIX: بررسی Duration نیز برای جلوگیری از اسلات‌های تکراری با Duration متفاوت
                                var existingSlot = existingSlotsInRange != null && existingSlotsInRange.Any(ts =>
                                    ts != null &&
                                    ts.DoctorId == doctorId &&
                                    ts.AppointmentDate.Date == dateOnly &&
                                    ts.StartTime == currentTime &&
                                    ts.EndTime == slotEndTime &&
                                    ts.Duration == doctorSchedule.AppointmentDuration && // ✅ بررسی Duration برای جلوگیری از تکراری
                                    !ts.IsDeleted);

                                if (!existingSlot)
                                {
                                    // ✅ بررسی وجود نوبت رزرو شده (استفاده از لیست از پیش بارگذاری شده)
                                    var slotStartDateTime = dateOnly.Add(currentTime);
                                    var slotEndDateTime = dateOnly.Add(slotEndTime);
                                    var hasExistingAppointment = bookedAppointmentsInRange != null && bookedAppointmentsInRange.Any(a =>
                                        a != null &&
                                        a.DoctorId == doctorId &&
                                        a.AppointmentDate >= slotStartDateTime &&
                                        a.AppointmentDate < slotEndDateTime &&
                                        a.Status != AppointmentStatus.Cancelled &&
                                        !a.IsDeleted);

                                    if (!hasExistingAppointment)
                                    {
                                        // ✅ بررسی نهایی: اطمینان از اینکه اسلات درون TimeRange است
                                        if (currentTime >= timeRange.StartTime && slotEndTime <= timeRange.EndTime)
                                        {
                                            // ✅ CRITICAL FIX: اطمینان از اینکه Duration از DoctorSchedule استفاده می‌شود
                                            // ✅ همچنین اطمینان از اینکه Status = Available است
                                            var newSlot = new DoctorTimeSlot
                                            {
                                                DoctorId = doctorId,
                                                AppointmentDate = dateOnly,
                                                StartTime = currentTime,
                                                EndTime = slotEndTime,
                                                Duration = doctorSchedule.AppointmentDuration, // ✅ استفاده از AppointmentDuration از DoctorSchedule
                                                Status = AppointmentStatus.Available, // ✅ همیشه Available برای اسلات‌های جدید
                                                CreatedAt = DateTime.Now,
                                                CreatedByUserId = doctorSchedule.UpdatedByUserId ?? doctorSchedule.CreatedByUserId
                                            };
                                            
                                            slotsForDate.Add(newSlot);
                                            slotsCreatedForThisTimeRange++;
                                            System.Diagnostics.Debug.WriteLine($"[GenerateSlotsForDateAsync] ✅ اسلات ایجاد شد - StartTime: {currentTime}, EndTime: {slotEndTime}, درون TimeRange: {timeRange.StartTime}-{timeRange.EndTime}");
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine($"[GenerateSlotsForDateAsync] ❌ خطا: اسلات خارج از TimeRange است! StartTime: {currentTime}, EndTime: {slotEndTime}, TimeRange: {timeRange.StartTime}-{timeRange.EndTime}");
                                        }
                                    }
                                }
                            }
                        }

                        currentTime = slotEndTime;
                    }

                    System.Diagnostics.Debug.WriteLine($"[GenerateSlotsForDateAsync] ✅ برای TimeRange {timeRange.TimeRangeId} تعداد {slotsCreatedForThisTimeRange} اسلات ایجاد شد");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[GenerateSlotsForDateAsync] ✅ برای تاریخ {dateOnly:yyyy/MM/dd} تعداد {slotsForDate.Count} اسلات تولید شد");

            return Task.FromResult(slotsForDate);
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
                // ✅ استفاده از DbFunctions.TruncateTime برای مقایسه تاریخ در LINQ to Entities
                return await _context.ScheduleExceptions
                    .AnyAsync(se => se.ScheduleId == scheduleId &&
                                   DbFunctions.TruncateTime(se.StartDate) <= DbFunctions.TruncateTime(date) &&
                                   (se.EndDate == null || DbFunctions.TruncateTime(se.EndDate.Value) >= DbFunctions.TruncateTime(date)) &&
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
                // ✅ استفاده از DbFunctions.TruncateTime برای مقایسه تاریخ در LINQ to Entities
                return await _context.ScheduleExceptions
                    .AnyAsync(se => se.ScheduleId == scheduleId &&
                                   DbFunctions.TruncateTime(se.StartDate) == DbFunctions.TruncateTime(date) &&
                                   (se.EndDate == null || DbFunctions.TruncateTime(se.EndDate.Value) == DbFunctions.TruncateTime(date)) &&
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
                                 a.AppointmentDate >= _timeProvider.GetIranToday() &&
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
        /// ✅ غیرفعال کردن فیلتر سراسری ActiveDoctorSchedules برای دریافت همه برنامه‌ها (فعال و غیرفعال)
        /// طبق: AI_ASSISTANT_MASTER_CONTRACT.md, DEVELOPMENT_CONTRACT.md
        /// </summary>
        public async Task<List<DoctorSchedule>> GetAllDoctorSchedulesAsync()
        {
            try
            {
                // ✅ غیرفعال کردن موقت فیلتر سراسری برای دریافت تمام برنامه‌ها (فعال و غیرفعال)
                // این کار برای امکان فیلتر کردن در لایه Service بر اساس IsActive انجام می‌شود
                _context.DisableFilter("ActiveDoctorSchedules");
                _context.DisableFilter("ActiveDoctorWorkDays");
                _context.DisableFilter("ActiveDoctorTimeRanges");

                // ✅ استفاده از AsNoTracking برای بهبود Performance (Read-Only Query)
                // توجه: فیلتر کردن IsActive در لایه Service انجام می‌شود
                var result = await _context.DoctorSchedules
                    .AsNoTracking() // ✅ بهبود Performance برای Read-Only Query
                    .Where(ds => !ds.IsDeleted) // ✅ فقط فیلتر IsDeleted (Soft Delete)
                    .Include(ds => ds.Doctor)
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                    .OrderBy(ds => ds.CreatedAt)
                    .ToListAsync();

                // ✅ فعال کردن مجدد فیلترهای سراسری
                _context.EnableFilter("ActiveDoctorSchedules");
                _context.EnableFilter("ActiveDoctorWorkDays");
                _context.EnableFilter("ActiveDoctorTimeRanges");

                return result;
            }
            catch (Exception ex)
            {
                // ✅ اطمینان از فعال شدن مجدد فیلترها در صورت خطا
                try
                {
                    _context.EnableFilter("ActiveDoctorSchedules");
                    _context.EnableFilter("ActiveDoctorWorkDays");
                    _context.EnableFilter("ActiveDoctorTimeRanges");
                }
                catch
                {
                    // Ignore errors in cleanup
                }

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
                // ✅ غیرفعال کردن موقت فیلتر سراسری برای دریافت برنامه کاری (فعال یا غیرفعال)
                // این کار برای امکان فعال/غیرفعال کردن برنامه‌های کاری انجام می‌شود
                _context.DisableFilter("ActiveDoctorSchedules");
                _context.DisableFilter("ActiveDoctorWorkDays");
                _context.DisableFilter("ActiveDoctorTimeRanges");
                
                // ✅ حذف .Include(ds => ds.Doctor) به دلیل خطای SQL: Invalid column name 'Doctor_DoctorId'
                // ✅ Navigation Property Doctor باید به صورت جداگانه در Service لود شود
                var result = await _context.DoctorSchedules
                    .Where(ds => ds.ScheduleId == scheduleId && !ds.IsDeleted)
                    // .Include(ds => ds.Doctor) // ❌ حذف شده: باعث خطای SQL می‌شود
                    .Include(ds => ds.WorkDays)
                    .Include(ds => ds.WorkDays.Select(wd => wd.TimeRanges))
                    .Include(ds => ds.CreatedByUser)
                    .Include(ds => ds.UpdatedByUser)
                    .AsNoTracking() // ✅ بهبود Performance برای read-only query
                    .FirstOrDefaultAsync();
                
                // ✅ فعال کردن مجدد فیلترهای سراسری
                _context.EnableFilter("ActiveDoctorSchedules");
                _context.EnableFilter("ActiveDoctorWorkDays");
                _context.EnableFilter("ActiveDoctorTimeRanges");
                
                return result;
            }
            catch (Exception ex)
            {
                // ✅ فعال کردن مجدد فیلترهای سراسری در صورت خطا
                try
                {
                    _context.EnableFilter("ActiveDoctorSchedules");
                    _context.EnableFilter("ActiveDoctorWorkDays");
                    _context.EnableFilter("ActiveDoctorTimeRanges");
                }
                catch { /* نادیده گرفتن خطاهای فعال‌سازی مجدد */ }
                
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
                    SafeRollback(transaction, "DeleteDoctorScheduleAsync");
                    throw; // پرتاب مجدد همان Exception
                }
                catch (Exception ex)
                {
                    // ✅ Rollback Transaction در صورت خطا
                    SafeRollback(transaction, "DeleteDoctorScheduleAsync");
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
                System.Diagnostics.Debug.WriteLine($"[DeactivateDoctorScheduleAsync] 🔍 شروع - ScheduleId: {scheduleId}");

                var schedule = await _context.DoctorSchedules
                    .Include(ds => ds.WorkDays) // ✅ بارگذاری WorkDays برای تغییر
                    .FirstOrDefaultAsync(ds => ds.ScheduleId == scheduleId && !ds.IsDeleted);

                if (schedule == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DeactivateDoctorScheduleAsync] ⚠️ برنامه کاری {scheduleId} یافت نشد");
                    return false;
                }

                // ✅ غیرفعال کردن خود برنامه کاری
                schedule.IsActive = false;
                schedule.UpdatedAt = DateTime.Now;
                System.Diagnostics.Debug.WriteLine($"[DeactivateDoctorScheduleAsync] ✅ IsActive برنامه کاری {scheduleId} به false تغییر یافت");

                // ✅ غیرفعال کردن تمام روزهای کاری
                if (schedule.WorkDays != null && schedule.WorkDays.Any())
                {
                    foreach (var workDay in schedule.WorkDays)
                    {
                        workDay.IsActive = false;
                        workDay.UpdatedAt = DateTime.Now;
                        // ✅ اطمینان از track شدن WorkDay
                        _context.Entry(workDay).State = EntityState.Modified;
                    }
                    System.Diagnostics.Debug.WriteLine($"[DeactivateDoctorScheduleAsync] ✅ {schedule.WorkDays.Count} WorkDay غیرفعال شد");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DeactivateDoctorScheduleAsync] ⚠️ هیچ WorkDay یافت نشد");
                }

                // ✅ اطمینان از track شدن Schedule
                _context.Entry(schedule).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"[DeactivateDoctorScheduleAsync] ✅ تغییرات در دیتابیس ذخیره شد");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeactivateDoctorScheduleAsync] ❌ خطا: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"[ActivateDoctorScheduleAsync] 🔍 شروع - ScheduleId: {scheduleId}");

                // ✅ غیرفعال کردن موقت فیلتر سراسری برای دریافت برنامه کاری غیرفعال
                _context.DisableFilter("ActiveDoctorSchedules");
                _context.DisableFilter("ActiveDoctorWorkDays");
                _context.DisableFilter("ActiveDoctorTimeRanges");

                var schedule = await _context.DoctorSchedules
                    .Include(ds => ds.WorkDays) // ✅ بارگذاری WorkDays برای تغییر
                    .FirstOrDefaultAsync(ds => ds.ScheduleId == scheduleId && !ds.IsDeleted);

                if (schedule == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ActivateDoctorScheduleAsync] ⚠️ برنامه کاری {scheduleId} یافت نشد");
                    return false;
                }

                // ✅ فعال کردن خود برنامه کاری
                schedule.IsActive = true;
                schedule.UpdatedAt = DateTime.Now;
                System.Diagnostics.Debug.WriteLine($"[ActivateDoctorScheduleAsync] ✅ IsActive برنامه کاری {scheduleId} به true تغییر یافت");

                // ✅ فعال کردن تمام روزهای کاری
                if (schedule.WorkDays != null && schedule.WorkDays.Any())
                {
                    foreach (var workDay in schedule.WorkDays)
                    {
                        workDay.IsActive = true;
                        workDay.UpdatedAt = DateTime.Now;
                        // ✅ اطمینان از track شدن WorkDay
                        _context.Entry(workDay).State = EntityState.Modified;
                    }
                    System.Diagnostics.Debug.WriteLine($"[ActivateDoctorScheduleAsync] ✅ {schedule.WorkDays.Count} WorkDay فعال شد");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ActivateDoctorScheduleAsync] ⚠️ هیچ WorkDay یافت نشد");
                }

                // ✅ اطمینان از track شدن Schedule
                _context.Entry(schedule).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                // ✅ فعال کردن مجدد فیلترهای سراسری
                _context.EnableFilter("ActiveDoctorSchedules");
                _context.EnableFilter("ActiveDoctorWorkDays");
                _context.EnableFilter("ActiveDoctorTimeRanges");
                
                System.Diagnostics.Debug.WriteLine($"[ActivateDoctorScheduleAsync] ✅ تغییرات در دیتابیس ذخیره شد");
                return true;
            }
            catch (Exception ex)
            {
                // ✅ فعال کردن مجدد فیلترهای سراسری در صورت خطا
                try
                {
                    _context.EnableFilter("ActiveDoctorSchedules");
                    _context.EnableFilter("ActiveDoctorWorkDays");
                    _context.EnableFilter("ActiveDoctorTimeRanges");
                }
                catch { /* نادیده گرفتن خطاهای فعال‌سازی مجدد */ }
                
                System.Diagnostics.Debug.WriteLine($"[ActivateDoctorScheduleAsync] ❌ خطا: {ex.Message}");
                throw new InvalidOperationException($"خطا در فعال کردن برنامه کاری {scheduleId}", ex);
            }
        }

        #endregion

    }
}

