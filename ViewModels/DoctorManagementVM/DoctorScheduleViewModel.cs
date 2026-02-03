using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Doctor;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل برنامه کاری پزشک برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از برنامه‌ریزی هفتگی پزشکان
    /// 2. مدیریت روزهای کاری و ساعات کاری برای هر روز
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 4. رعایت استانداردهای پزشکی ایران در برنامه‌ریزی نوبت‌دهی
    /// 5. پشتیبانی از محاسبه خودکار زمان‌های در دسترس
    /// </summary>
    public class DoctorScheduleViewModel
    {
        /// <summary>
        /// شناسه برنامه کاری (برای سازگاری با Views)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک (برای نمایش در View)
        /// </summary>
        public string DoctorName { get; set; } // Populated from Doctor.FullName

        /// <summary>
        /// کد ملی پزشک (برای نمایش در View)
        /// </summary>
        public string NationalCode { get; set; }

        /// <summary>
        /// شماره نظام پزشکی (برای نمایش در View)
        /// </summary>
        public string MedicalCouncilCode { get; set; }

        /// <summary>
        /// نام‌های تخصص‌ها (برای نمایش در View)
        /// </summary>
        public List<string> SpecializationNames { get; set; } = new List<string>();

        /// <summary>
        /// لیست روزهای کاری هفتگی پزشک
        /// </summary>
        public List<WorkDayViewModel> WorkDays { get; set; } = new List<WorkDayViewModel>();

        /// <summary>
        /// لیست تاریخ‌های خاص برای تنظیم برنامه (نه هفتگی)
        /// </summary>
        public List<SpecificDateViewModel> SpecificDates { get; set; } = new List<SpecificDateViewModel>();

        /// <summary>
        /// مدت زمان هر نوبت (به دقیقه)
        /// </summary>
        [Range(5, 120, ErrorMessage = "مدت زمان نوبت باید بین 5 تا 120 دقیقه باشد.")]
        [Display(Name = "مدت زمان هر نوبت (دقیقه)")]
        public int AppointmentDuration { get; set; } = 30;

        /// <summary>
        /// زمان شروع روز کاری
        /// </summary>
        public TimeSpan? DefaultStartTime { get; set; }

        /// <summary>
        /// زمان پایان روز کاری
        /// </summary>
        public TimeSpan? DefaultEndTime { get; set; }

        /// <summary>
        /// هزینه ویزیت پایه (ریال)
        /// </summary>
        [Range(0, 10000000, ErrorMessage = "هزینه ویزیت باید بین 0 تا 10,000,000 ریال باشد.")]
        [Display(Name = "هزینه ویزیت (ریال)")]
        public decimal ConsultationFee { get; set; } = 0;

        /// <summary>
        /// تاریخ ایجاد برنامه کاری
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// نام کاربر ایجاد کننده
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش برنامه کاری
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// نام کاربر آخرین ویرایش کننده
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// روز هفته (برای سازگاری با View)
        /// </summary>
        public string DayOfWeek { get; set; }

        /// <summary>
        /// زمان شروع (برای سازگاری با View)
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان شروع به صورت string برای JavaScript (24-hour format)
        /// توجه: در TimeSpan، فرمت hh به معنای ساعت 24 ساعته است (00-23)
        /// </summary>
        public string StartTimeString => $"{StartTime.Hours:D2}:{StartTime.Minutes:D2}";

        /// <summary>
        /// زمان پایان (برای سازگاری با View)
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// زمان پایان به صورت string برای JavaScript (24-hour format)
        /// توجه: در TimeSpan، فرمت hh به معنای ساعت 24 ساعته است (00-23)
        /// </summary>
        public string EndTimeString => $"{EndTime.Hours:D2}:{EndTime.Minutes:D2}";

        /// <summary>
        /// وضعیت فعال (برای سازگاری با View)
        /// ✅ این مقدار باید از DoctorSchedule.IsActive گرفته شود، نه از WorkDays
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ شروع (برای سازگاری با View)
        /// </summary>
        public DateTime? StartDate => DateTime.Today;

        /// <summary>
        /// تاریخ پایان (برای سازگاری با View)
        /// </summary>
        public DateTime? EndDate => DateTime.Today.AddYears(1); // نامحدود

        /// <summary>
        /// روز هفته برای کالندر (برای سازگاری با View)
        /// </summary>
        public int DayOfWeekForCalendar { get; set; }

        /// <summary>
        /// آمار برنامه‌های کاری (برای نمایش در View)
        /// </summary>
        public int TotalSchedules => WorkDays?.Count ?? 0;

        /// <summary>
        /// تعداد برنامه‌های فعال
        /// </summary>
        public int ActiveSchedules => WorkDays?.Count(w => w.IsActive) ?? 0;

        /// <summary>
        /// تعداد کل زمان‌های کاری
        /// </summary>
        public int TotalTimeSlots => WorkDays?.Sum(w => w.TimeRanges?.Count ?? 0) ?? 0;

        /// <summary>
        /// ساعت‌های کاری هفتگی
        /// </summary>
        public int WeeklyHours
        {
            get
            {
                if (WorkDays == null) return 0;
                var totalMinutes = WorkDays
                    .Where(w => w.IsActive)
                    .Sum(w => w.TimeRanges?.Sum(t => (t.EndTime - t.StartTime).TotalMinutes) ?? 0);
                return (int)(totalMinutes / 60);
            }
        }

        /// <summary>
        /// لیست برنامه‌های کاری (برای سازگاری با View)
        /// </summary>
        public List<ScheduleItemViewModel> Schedules
        {
            get
            {
                if (WorkDays == null) return new List<ScheduleItemViewModel>();
                
                var schedules = new List<ScheduleItemViewModel>();
                foreach (var workDay in WorkDays.Where(w => w.IsActive))
                {
                    foreach (var timeRange in workDay.TimeRanges ?? new List<TimeRangeViewModel>())
                    {
                        schedules.Add(new ScheduleItemViewModel
                        {
                            Id = workDay.Id,
                            Title = $"{workDay.DayName} - {timeRange.StartTime:hh\\:mm} تا {timeRange.EndTime:hh\\:mm}",
                            DayOfWeek = workDay.DayName,
                            StartDate = DateTime.Today,
                            EndDate = DateTime.Today.AddYears(1), // تاریخ پایان معقول به جای null
                            IsActive = workDay.IsActive && timeRange.IsActive,
                            StartTime = timeRange.StartTime,
                            EndTime = timeRange.EndTime,
                            TimeSlots = new List<ScheduleTimeSlotViewModel>
                            {
                                new ScheduleTimeSlotViewModel
                                {
                                    StartTime = timeRange.StartTime,
                                    EndTime = timeRange.EndTime,
                                    Type = "مشاوره"
                                }
                            },
                            Notes = ""
                        });
                    }
                }
                return schedules;
            }
        }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DoctorScheduleViewModel FromEntity(DoctorSchedule doctorSchedule)
        {
            if (doctorSchedule == null)
            {
                System.Diagnostics.Debug.WriteLine("[FromEntity] ❌ doctorSchedule is null");
                return null;
            }
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"[FromEntity] 🔍 شروع تبدیل DoctorSchedule به ViewModel. ScheduleId: {doctorSchedule.ScheduleId}, DoctorId: {doctorSchedule.DoctorId}");
                
                // ✅ لاگ اطلاعات برای دیباگ
                var workDaysCount = doctorSchedule.WorkDays?.Count(w => w != null && !w.IsDeleted) ?? 0;
                var timeRangesCount = doctorSchedule.WorkDays?.Sum(w => w?.TimeRanges?.Count(tr => tr != null && !tr.IsDeleted) ?? 0) ?? 0;
                System.Diagnostics.Debug.WriteLine($"[FromEntity] 📊 DoctorId={doctorSchedule.DoctorId}, WorkDays={workDaysCount}, TimeRanges={timeRangesCount}");
                
                // ✅ فیلتر کردن WorkDays و TimeRanges فقط موارد فعال و غیرحذف شده
                // ✅ با Null Safety کامل برای جلوگیری از NullReferenceException
                var activeWorkDays = new List<WorkDayViewModel>();
                
                if (doctorSchedule.WorkDays != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[FromEntity] 📅 شروع پردازش {doctorSchedule.WorkDays.Count} WorkDay");
                    
                    foreach (var workDay in doctorSchedule.WorkDays)
                    {
                        if (workDay != null && !workDay.IsDeleted)
                        {
                            try
                            {
                                System.Diagnostics.Debug.WriteLine($"[FromEntity] 🔄 در حال تبدیل WorkDay {workDay.WorkDayId} (DayOfWeek: {workDay.DayOfWeek})");
                                
                                var workDayViewModel = WorkDayViewModel.FromEntity(workDay);
                                
                                if (workDayViewModel != null)
                                {
                                    activeWorkDays.Add(workDayViewModel);
                                    System.Diagnostics.Debug.WriteLine($"[FromEntity] ✅ WorkDay {workDay.WorkDayId} با موفقیت تبدیل شد. TimeRangesCount: {workDayViewModel.TimeRanges?.Count ?? 0}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[FromEntity] ⚠️ WorkDay {workDay.WorkDayId} به ViewModel تبدیل نشد (null)");
                                }
                            }
                            catch (Exception ex)
                            {
                                // ✅ Log کردن خطا در تبدیل WorkDay اما ادامه دادن
                                System.Diagnostics.Debug.WriteLine($"[FromEntity] ❌ خطا در تبدیل WorkDay {workDay.WorkDayId} به ViewModel: {ex.GetType().Name} - {ex.Message}");
                                System.Diagnostics.Debug.WriteLine($"[FromEntity] StackTrace: {ex.StackTrace}");
                                if (ex.InnerException != null)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[FromEntity] InnerException: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[FromEntity] ⏭️ WorkDay نادیده گرفته شد (null یا IsDeleted=true)");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[FromEntity] ⚠️ WorkDays null است");
                }
                
                System.Diagnostics.Debug.WriteLine($"[FromEntity] 📊 تعداد WorkDays فعال: {activeWorkDays.Count}");
                
                var viewModel = new DoctorScheduleViewModel
                {
                    Id = doctorSchedule.ScheduleId,
                    DoctorId = doctorSchedule.DoctorId,
                    AppointmentDuration = doctorSchedule.AppointmentDuration,
                    DefaultStartTime = doctorSchedule.DefaultStartTime,
                    DefaultEndTime = doctorSchedule.DefaultEndTime,
                    ConsultationFee = doctorSchedule.ConsultationFee, // ✅ اضافه شد
                    CreatedAt = doctorSchedule.CreatedAt,
                    CreatedBy = doctorSchedule.CreatedByUserId,
                    UpdatedAt = doctorSchedule.UpdatedAt,
                    UpdatedBy = doctorSchedule.UpdatedByUserId,
                    IsActive = doctorSchedule.IsActive, // ✅ استفاده از IsActive از DoctorSchedule
                    WorkDays = activeWorkDays, // ✅ استفاده از WorkDays فیلتر شده
                    // ✅ پر کردن اطلاعات پزشک با Null Safety
                    DoctorName = doctorSchedule.Doctor?.FullName ?? "نامشخص",
                    NationalCode = doctorSchedule.Doctor?.NationalCode,
                    MedicalCouncilCode = doctorSchedule.Doctor?.MedicalCouncilCode,
                    SpecializationNames = doctorSchedule.Doctor?.DoctorSpecializations?
                        .Where(ds => ds != null && ds.Specialization != null && !string.IsNullOrEmpty(ds.Specialization.Name))
                        .Select(ds => ds.Specialization.Name)
                        .ToList() ?? new List<string>()
                };
                
                System.Diagnostics.Debug.WriteLine($"[FromEntity] ✅ DoctorScheduleViewModel ایجاد شد. WorkDaysCount: {viewModel.WorkDays?.Count ?? 0}");
                
                return viewModel;
            }
            catch (Exception ex)
            {
                // ✅ Log کردن خطا با جزئیات کامل
                System.Diagnostics.Debug.WriteLine($"[FromEntity] ❌ خطا در FromEntity برای DoctorSchedule {doctorSchedule?.ScheduleId}: {ex.GetType().Name} - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[FromEntity] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[FromEntity] InnerException: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// ✅ تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// ✅ فیلتر کردن WorkDays و TimeRanges خالی یا نامعتبر
        /// ✅ فقط WorkDays فعال را ارسال می‌کنیم (برای جلوگیری از تداخل با Unique Constraint)
        /// </summary>
        public DoctorSchedule ToEntity()
        {
            // ✅ فیلتر کردن WorkDays معتبر و فعال (با TimeRanges معتبر)
            // ✅ فقط WorkDays فعال را ارسال می‌کنیم تا از تداخل با Unique Constraint جلوگیری کنیم
            var validWorkDays = this.WorkDays?
                .Where(wd => wd != null && wd.IsActive) // ✅ فقط WorkDays فعال
                .Select(wd => wd.ToEntity())
                .Where(wd => wd != null)
                .ToList() ?? new List<DoctorWorkDay>();
            
            System.Diagnostics.Debug.WriteLine($"[DoctorScheduleViewModel.ToEntity] ✅ تبدیل به Entity - WorkDaysCount: {validWorkDays.Count} (فقط فعال)");
            
            return new DoctorSchedule
            {
                ScheduleId = this.Id, // استفاده از ScheduleId به جای Id
                DoctorId = this.DoctorId,
                AppointmentDuration = this.AppointmentDuration,
                DefaultStartTime = this.DefaultStartTime,
                DefaultEndTime = this.DefaultEndTime,
                ConsultationFee = this.ConsultationFee, // ✅ اضافه شد
                CreatedAt = this.CreatedAt,
                CreatedByUserId = this.CreatedBy,
                UpdatedAt = this.UpdatedAt,
                UpdatedByUserId = this.UpdatedBy,
                WorkDays = validWorkDays
            };
        }
    }

    /// <summary>
    /// مدل روز کاری پزشک
    /// </summary>
    public class WorkDayViewModel
    {
        /// <summary>
        /// شناسه روز کاری (برای سازگاری با Views)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// شماره روز هفته (0 = یکشنبه، 1 = دوشنبه، ...، 6 = شنبه)
        /// </summary>
        [Range(0, 6, ErrorMessage = "شماره روز هفته باید بین 0 تا 6 باشد.")]
        public int DayOfWeek { get; set; }

        /// <summary>
        /// ترتیب روز هفته برای مرتب‌سازی (مطابق با DayOfWeek)
        /// </summary>
        public int DayOrder => DayOfWeek;

        /// <summary>
        /// نام روز هفته
        /// </summary>
        public string DayName { get; set; }

        /// <summary>
        /// روز هفته برای کالندر JavaScript (0=یکشنبه، 6=شنبه)
        /// </summary>
        public int DayOfWeekForCalendar { get; set; }

        /// <summary>
        /// نشان‌دهنده فعال بودن روز کاری
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// لیست بازه‌های زمانی کاری در این روز
        /// </summary>
        public List<TimeRangeViewModel> TimeRanges { get; set; } = new List<TimeRangeViewModel>();

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static WorkDayViewModel FromEntity(DoctorWorkDay workDay)
        {
            if (workDay == null)
            {
                System.Diagnostics.Debug.WriteLine("[WorkDayViewModel.FromEntity] ❌ workDay is null");
                return null;
            }
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] 🔍 شروع تبدیل WorkDay {workDay.WorkDayId} (DayOfWeek: {workDay.DayOfWeek}, IsActive: {workDay.IsActive})");
                
                var dayNames = new[] { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };
                
                // ✅ تبدیل TimeRanges با Null Safety کامل
                var timeRanges = new List<TimeRangeViewModel>();
                if (workDay.TimeRanges != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] ⏰ شروع پردازش {workDay.TimeRanges.Count} TimeRange");
                    
                    foreach (var timeRange in workDay.TimeRanges)
                    {
                        if (timeRange != null && !timeRange.IsDeleted)
                        {
                            try
                            {
                                System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] 🔄 در حال پردازش TimeRange {timeRange.TimeRangeId}: StartTime={timeRange.StartTime}, EndTime={timeRange.EndTime}");
                                
                                // ✅ فقط TimeRange های معتبر را اضافه می‌کنیم (با StartTime و EndTime معتبر)
                                if (timeRange.StartTime != default(TimeSpan) && timeRange.EndTime != default(TimeSpan))
                                {
                                    var timeRangeViewModel = TimeRangeViewModel.FromEntity(timeRange);
                                    if (timeRangeViewModel != null)
                                    {
                                        timeRanges.Add(timeRangeViewModel);
                                        System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] ✅ TimeRange {timeRange.TimeRangeId} با موفقیت تبدیل شد");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] ⚠️ TimeRange {timeRange.TimeRangeId} به ViewModel تبدیل نشد (null)");
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] ⚠️ TimeRange {timeRange.TimeRangeId} نادیده گرفته شد (StartTime یا EndTime پیش‌فرض)");
                                }
                            }
                            catch (Exception ex)
                            {
                                // ✅ Log کردن خطا در تبدیل TimeRange اما ادامه دادن
                                System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] ❌ خطا در تبدیل TimeRange {timeRange?.TimeRangeId} به ViewModel: {ex.GetType().Name} - {ex.Message}");
                                System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] TimeRange Details: StartTime={timeRange?.StartTime}, EndTime={timeRange?.EndTime}, IsDeleted={timeRange?.IsDeleted}");
                                System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] StackTrace: {ex.StackTrace}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] ⏭️ TimeRange نادیده گرفته شد (null یا IsDeleted=true)");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] ⚠️ TimeRanges null است");
                }
                
                System.Diagnostics.Debug.WriteLine($"[WorkDayViewModel.FromEntity] 📊 تعداد TimeRanges معتبر: {timeRanges.Count}");
                
                return new WorkDayViewModel
                {
                    Id = workDay.WorkDayId, // استفاده از WorkDayId به جای Id
                    DayOfWeek = workDay.DayOfWeek,
                    DayName = workDay.DayOfWeek >= 0 && workDay.DayOfWeek < dayNames.Length ? dayNames[workDay.DayOfWeek] : "نامشخص",
                    DayOfWeekForCalendar = workDay.DayOfWeek, // Populate for calendar
                    IsActive = workDay.IsActive,
                    TimeRanges = timeRanges
                };
            }
            catch (Exception ex)
            {
                // ✅ Log کردن خطا با جزئیات بیشتر
                System.Diagnostics.Debug.WriteLine($"خطا در FromEntity برای WorkDay {workDay?.WorkDayId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ExceptionType: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// ✅ تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// ✅ فیلتر کردن TimeRange های خالی یا نامعتبر
        /// </summary>
        public DoctorWorkDay ToEntity()
        {
            // ✅ فیلتر کردن TimeRange های معتبر (با StartTime و EndTime معتبر)
            var validTimeRanges = this.TimeRanges?
                .Where(tr => tr != null && 
                            tr.StartTime != TimeSpan.Zero && 
                            tr.EndTime != TimeSpan.Zero &&
                            tr.StartTime < tr.EndTime)
                .Select(tr => tr.ToEntity())
                .Where(tr => tr != null)
                .ToList() ?? new List<DoctorTimeRange>();
            
            return new DoctorWorkDay
            {
                WorkDayId = this.Id, // استفاده از WorkDayId به جای Id
                DayOfWeek = this.DayOfWeek,
                IsActive = this.IsActive,
                TimeRanges = validTimeRanges
            };
        }
    }

    /// <summary>
    /// مدل بازه زمانی کاری
    /// </summary>
    public class TimeRangeViewModel
    {
        /// <summary>
        /// شناسه بازه زمانی (برای سازگاری با Views)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// زمان شروع بازه
        /// </summary>
        [Required(ErrorMessage = "زمان شروع الزامی است.")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان شروع به صورت string برای JavaScript (24-hour format)
        /// توجه: در TimeSpan، فرمت hh به معنای ساعت 24 ساعته است (00-23)
        /// </summary>
        public string StartTimeString => $"{StartTime.Hours:D2}:{StartTime.Minutes:D2}";

        /// <summary>
        /// زمان پایان بازه
        /// </summary>
        [Required(ErrorMessage = "زمان پایان الزامی است.")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// زمان پایان به صورت string برای JavaScript (24-hour format)
        /// توجه: در TimeSpan، فرمت hh به معنای ساعت 24 ساعته است (00-23)
        /// </summary>
        public string EndTimeString => $"{EndTime.Hours:D2}:{EndTime.Minutes:D2}";

        /// <summary>
        /// نشان‌دهنده فعال بودن بازه زمانی
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static TimeRangeViewModel FromEntity(DoctorTimeRange timeRange)
        {
            if (timeRange == null) return null;
            
            try
            {
                // ✅ اعتبارسنجی StartTime و EndTime
                if (timeRange.StartTime == default(TimeSpan) || timeRange.EndTime == default(TimeSpan))
                {
                    System.Diagnostics.Debug.WriteLine($"TimeRange {timeRange.TimeRangeId} دارای StartTime یا EndTime پیش‌فرض است. StartTime={timeRange.StartTime}, EndTime={timeRange.EndTime}");
                    return null;
                }
                
                // ✅ اعتبارسنجی منطقی بودن بازه زمانی
                if (timeRange.StartTime >= timeRange.EndTime)
                {
                    System.Diagnostics.Debug.WriteLine($"TimeRange {timeRange.TimeRangeId} دارای StartTime >= EndTime است. StartTime={timeRange.StartTime}, EndTime={timeRange.EndTime}");
                    return null;
                }
                
                return new TimeRangeViewModel
                {
                    Id = timeRange.TimeRangeId,
                    StartTime = timeRange.StartTime,
                    EndTime = timeRange.EndTime,
                    IsActive = timeRange.IsActive
                };
            }
            catch (Exception ex)
            {
                // ✅ Log کردن خطا با جزئیات بیشتر
                System.Diagnostics.Debug.WriteLine($"خطا در FromEntity برای TimeRange {timeRange?.TimeRangeId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ExceptionType: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// ✅ تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// </summary>
        public DoctorTimeRange ToEntity()
        {
            return new DoctorTimeRange
            {
                TimeRangeId = this.Id, // Assuming DoctorTimeRange has an Id
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                IsActive = this.IsActive
            };
        }
    }

    /// <summary>
    /// ولیدیتور پیشرفته برای مدل برنامه کاری پزشک
    /// </summary>
    public class DoctorScheduleViewModelValidator : AbstractValidator<DoctorScheduleViewModel>
    {
        public DoctorScheduleViewModelValidator()
        {
            // اعتبارسنجی شناسه پزشک
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.")
                .WithErrorCode("INVALID_DOCTOR_ID");

            // اعتبارسنجی مدت زمان نوبت
            RuleFor(x => x.AppointmentDuration)
                .InclusiveBetween(5, 120)
                .WithMessage("مدت زمان نوبت باید بین 5 تا 120 دقیقه باشد.")
                .WithErrorCode("INVALID_APPOINTMENT_DURATION");

            // اعتبارسنجی هزینه ویزیت
            RuleFor(x => x.ConsultationFee)
                .GreaterThanOrEqualTo(0)
                .WithMessage("هزینه ویزیت نمی‌تواند منفی باشد.")
                .WithErrorCode("INVALID_CONSULTATION_FEE_NEGATIVE")
                .LessThanOrEqualTo(10000000)
                .WithMessage("هزینه ویزیت نمی‌تواند بیش از 10,000,000 ریال باشد.")
                .WithErrorCode("INVALID_CONSULTATION_FEE_MAX");

            // ✅ اعتبارسنجی WorkDays (اصلی) - فقط اگر تاریخ خاصی وجود نداشته باشد
            // ✅ اگر تاریخ خاصی وجود دارد، نیازی به WorkDays نیست
            RuleFor(x => x)
                .Must(model => 
                    (model.WorkDays != null && model.WorkDays.Any()) ||
                    (model.SpecificDates != null && model.SpecificDates.Any(sd => !string.IsNullOrWhiteSpace(sd.PersianDate)))
                )
                .WithMessage("حداقل یک روز کاری هفتگی باید تعیین شود یا یک تاریخ خاص اضافه شود.")
                .WithErrorCode("NO_WORK_DAYS_OR_SPECIFIC_DATES")
                .OverridePropertyName("WorkDays");

            // اعتبارسنجی تعداد روزهای کاری
            RuleFor(x => x.WorkDays)
                .Must(workDays => workDays == null || workDays.Count <= 7)
                .WithMessage("حداکثر 7 روز کاری می‌تواند تعیین شود.")
                .WithErrorCode("TOO_MANY_WORK_DAYS");

            // اعتبارسنجی روزهای تکراری
            RuleFor(x => x.WorkDays)
                .Must(workDays => workDays == null || workDays.Select(w => w.DayOfWeek).Distinct().Count() == workDays.Count)
                .WithMessage("روزهای کاری تکراری مجاز نیست.")
                .WithErrorCode("DUPLICATE_WORK_DAYS");

            // ✅ اعتبارسنجی WorkDays فعال یا تاریخ‌های خاص
            // ✅ اگر تاریخ خاصی وجود دارد، نیازی به WorkDay فعال نیست
            RuleFor(x => x)
                .Must(model => 
                    (model.WorkDays != null && model.WorkDays.Any(w => w.IsActive)) ||
                    (model.SpecificDates != null && model.SpecificDates.Any(sd => !string.IsNullOrWhiteSpace(sd.PersianDate)))
                )
                .WithMessage("حداقل یک روز کاری هفتگی باید فعال باشد یا یک تاریخ خاص تعیین شود.")
                .WithErrorCode("NO_ACTIVE_WORK_DAYS_OR_SPECIFIC_DATES")
                .OverridePropertyName("WorkDays");

            // اعتبارسنجی جزئیات WorkDays
            When(x => x.WorkDays != null && x.WorkDays.Any(), () =>
            {
                RuleForEach(x => x.WorkDays)
                    .SetValidator(new WorkDayViewModelValidator());
            });

            // اعتبارسنجی properties flat (برای سازگاری با View)
            When(x => !string.IsNullOrEmpty(x.DayOfWeek), () =>
            {
                RuleFor(x => x.DayOfWeek)
                    .Must(day => IsValidDayOfWeek(day))
                    .WithMessage("روز هفته نامعتبر است.")
                    .WithErrorCode("INVALID_DAY_OF_WEEK");
            });

            When(x => x.StartTime != TimeSpan.Zero, () =>
            {
                RuleFor(x => x.StartTime)
                    .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromHours(24))
                    .WithMessage("زمان شروع نامعتبر است.")
                    .WithErrorCode("INVALID_START_TIME");
            });

            When(x => x.EndTime != TimeSpan.Zero, () =>
            {
                RuleFor(x => x.EndTime)
                    .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromHours(24))
                    .WithMessage("زمان پایان نامعتبر است.")
                    .WithErrorCode("INVALID_END_TIME");
            });

            // اعتبارسنجی منطقی بودن زمان (برای properties flat)
            When(x => x.StartTime != TimeSpan.Zero && x.EndTime != TimeSpan.Zero, () =>
            {
                RuleFor(x => x.EndTime)
                    .GreaterThan(x => x.StartTime)
                    .WithMessage("زمان پایان باید بعد از زمان شروع باشد.")
                    .WithErrorCode("INVALID_TIME_RANGE");
            });
        }

        /// <summary>
        /// بررسی معتبر بودن روز هفته
        /// </summary>
        private bool IsValidDayOfWeek(string dayOfWeek)
        {
            var validDays = new[] { "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه" };
            return validDays.Contains(dayOfWeek);
        }
    }

    /// <summary>
    /// ولیدیتور پیشرفته برای مدل روز کاری
    /// </summary>
    public class WorkDayViewModelValidator : AbstractValidator<WorkDayViewModel>
    {
        public WorkDayViewModelValidator()
        {
            // اعتبارسنجی شماره روز هفته
            RuleFor(x => x.DayOfWeek)
                .InclusiveBetween(0, 6)
                .WithMessage("شماره روز هفته باید بین 0 تا 6 باشد.")
                .WithErrorCode("INVALID_DAY_OF_WEEK_NUMBER");

            // ✅ اعتبارسنجی نام روز هفته - فقط زمانی که DayOfWeek معتبر است
            // اگر DayOfWeek معتبر است، DayName باید از DayOfWeek محاسبه شود
            RuleFor(x => x.DayName)
                .NotEmpty()
                .WithMessage("نام روز هفته الزامی است.")
                .WithErrorCode("EMPTY_DAY_NAME")
                .When(x => x.DayOfWeek >= 0 && x.DayOfWeek <= 6); // فقط زمانی که DayOfWeek معتبر است
            
            // ✅ اعتبارسنجی همخوانی DayName با DayOfWeek
            RuleFor(x => x.DayName)
                .Must((workDay, dayName) =>
                {
                    if (string.IsNullOrEmpty(dayName) || workDay.DayOfWeek < 0 || workDay.DayOfWeek > 6)
                        return true; // اگر DayOfWeek نامعتبر است، validation دیگری آن را بررسی می‌کند
                    
                    var dayNames = new[] { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };
                    var expectedDayName = dayNames[workDay.DayOfWeek];
                    return dayName == expectedDayName;
                })
                .WithMessage("نام روز هفته با شماره روز هفته همخوانی ندارد.")
                .WithErrorCode("DAY_NAME_MISMATCH")
                .When(x => x.DayOfWeek >= 0 && x.DayOfWeek <= 6 && !string.IsNullOrEmpty(x.DayName));

            // ✅ اعتبارسنجی TimeRanges - با پیغام راهنما
            // این Validation فقط TimeRange های معتبر (با StartTime و EndTime غیر صفر) را در نظر می‌گیرد
            RuleFor(x => x.TimeRanges)
                .Must((workDay, timeRanges) => 
                {
                    if (!workDay.IsActive) return true; // اگر روز فعال نیست، نیازی به TimeRange نیست
                    
                    if (timeRanges == null || !timeRanges.Any()) return false;
                    
                    // ✅ فیلتر کردن TimeRange های معتبر (با StartTime و EndTime غیر صفر و منطقی)
                    var validTimeRanges = timeRanges
                        .Where(tr => tr != null && 
                                    tr.StartTime != TimeSpan.Zero && 
                                    tr.EndTime != TimeSpan.Zero &&
                                    tr.StartTime < tr.EndTime) // ✅ فقط TimeRange های معتبر
                        .ToList();
                    
                    // ✅ بررسی اینکه حداقل یک TimeRange معتبر وجود داشته باشد
                    return validTimeRanges.Any();
                })
                .When(x => x.IsActive)
                .WithMessage("⚠️ برای روزهای فعال، باید حداقل یک بازه زمانی کامل (با زمان شروع و پایان) تعیین شود. لطفاً روی دکمه ➕ کلیک کنید و بازه زمانی اضافه کنید.")
                .WithErrorCode("NO_TIME_RANGES_FOR_ACTIVE_DAY");

            // اعتبارسنجی تعداد بازه‌های زمانی (فقط TimeRange های معتبر)
            RuleFor(x => x.TimeRanges)
                .Must(timeRanges => 
                {
                    if (timeRanges == null || !timeRanges.Any()) return true;
                    
                    // ✅ شمارش فقط TimeRange های معتبر
                    var validCount = timeRanges
                        .Count(tr => tr != null && 
                                    tr.StartTime != TimeSpan.Zero && 
                                    tr.EndTime != TimeSpan.Zero &&
                                    tr.StartTime < tr.EndTime);
                    
                    return validCount <= 10;
                })
                .WithMessage("حداکثر 10 بازه زمانی معتبر در روز مجاز است.")
                .WithErrorCode("TOO_MANY_TIME_RANGES");

            // اعتبارسنجی بازه‌های زمانی تکراری
            RuleFor(x => x.TimeRanges)
                .Must(timeRanges => timeRanges == null || !HasOverlappingTimeRanges(timeRanges))
                .WithMessage("بازه‌های زمانی نباید با هم تداخل داشته باشند.")
                .WithErrorCode("OVERLAPPING_TIME_RANGES");

            // ✅ اعتبارسنجی جزئیات TimeRanges - روی همه TimeRange ها (حتی نامعتبر) برای نمایش پیغام خطای دقیق
            // این کار باعث می‌شود که اگر EndTime < StartTime باشد، پیغام خطای مناسب نمایش داده شود
            When(x => x.TimeRanges != null && x.TimeRanges.Any(tr => tr != null), () =>
            {
                RuleForEach(x => x.TimeRanges)
                    .SetValidator(new TimeRangeViewModelValidator());
            });
        }

        /// <summary>
        /// بررسی تداخل بازه‌های زمانی
        /// </summary>
        private bool HasOverlappingTimeRanges(List<TimeRangeViewModel> timeRanges)
        {
            if (timeRanges == null || timeRanges.Count <= 1) return false;

            // ✅ فیلتر کردن TimeRange های معتبر (با StartTime و EndTime معتبر)
            var validTimeRanges = timeRanges
                .Where(tr => tr != null && 
                            tr.StartTime != TimeSpan.Zero && 
                            tr.EndTime != TimeSpan.Zero &&
                            tr.StartTime < tr.EndTime)
                .ToList();

            if (validTimeRanges.Count <= 1) return false;

            var sortedRanges = validTimeRanges.OrderBy(t => t.StartTime).ToList();
            for (int i = 0; i < sortedRanges.Count - 1; i++)
            {
                if (sortedRanges[i].EndTime > sortedRanges[i + 1].StartTime)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// ولیدیتور پیشرفته برای مدل بازه زمانی
    /// ✅ با پیغام‌های واضح و راهنما برای کاربران عادی
    /// </summary>
    public class TimeRangeViewModelValidator : AbstractValidator<TimeRangeViewModel>
    {
        public TimeRangeViewModelValidator()
        {
            // ✅ اعتبارسنجی زمان شروع - فقط اگر زمان پایان هم پر شده باشد
            RuleFor(x => x.StartTime)
                .NotEqual(TimeSpan.Zero)
                .WithMessage("⚠️ لطفاً زمان شروع بازه کاری را وارد کنید.")
                .WithErrorCode("EMPTY_START_TIME")
                .When(x => x.EndTime != TimeSpan.Zero); // فقط اگر زمان پایان پر شده باشد

            // ✅ اعتبارسنجی زمان پایان - فقط اگر زمان شروع هم پر شده باشد
            RuleFor(x => x.EndTime)
                .NotEqual(TimeSpan.Zero)
                .WithMessage("⚠️ لطفاً زمان پایان بازه کاری را وارد کنید.")
                .WithErrorCode("EMPTY_END_TIME")
                .When(x => x.StartTime != TimeSpan.Zero); // فقط اگر زمان شروع پر شده باشد

            // ✅ اعتبارسنجی محدوده زمانی
            RuleFor(x => x.StartTime)
                .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromHours(24))
                .WithMessage("⚠️ زمان شروع باید بین 00:00 تا 23:59 باشد. لطفاً زمان صحیح را وارد کنید.")
                .WithErrorCode("INVALID_START_TIME_RANGE")
                .When(x => x.StartTime != TimeSpan.Zero);

            RuleFor(x => x.EndTime)
                .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromHours(24))
                .WithMessage("⚠️ زمان پایان باید بین 00:00 تا 23:59 باشد. لطفاً زمان صحیح را وارد کنید.")
                .WithErrorCode("INVALID_END_TIME_RANGE")
                .When(x => x.EndTime != TimeSpan.Zero);

            // ✅ اعتبارسنجی منطقی بودن زمان - با پیغام راهنما
            RuleFor(x => x.EndTime)
                .Must((timeRange, endTime) => 
                {
                    if (timeRange.StartTime == TimeSpan.Zero || endTime == TimeSpan.Zero)
                        return true; // اگر زمان‌ها خالی باشند، validation دیگری آن را بررسی می‌کند
                    
                    return endTime > timeRange.StartTime;
                })
                .WithMessage("❌ زمان پایان باید بعد از زمان شروع باشد. مثال: اگر شروع 07:00 است، پایان باید بعد از 07:00 باشد (مثلاً 17:00).")
                .WithErrorCode("INVALID_TIME_ORDER")
                .When(x => x.StartTime != TimeSpan.Zero && x.EndTime != TimeSpan.Zero);

            // ✅ اعتبارسنجی حداقل مدت زمان - با پیغام راهنما
            RuleFor(x => x.EndTime)
                .Must((timeRange, endTime) => 
                {
                    if (timeRange.StartTime == TimeSpan.Zero || endTime == TimeSpan.Zero)
                        return true; // اگر زمان‌ها خالی باشند، validation دیگری آن را بررسی می‌کند
                    
                    var duration = (endTime - timeRange.StartTime).TotalMinutes;
                    return duration >= 5; // حداقل 5 دقیقه (حداقل AppointmentDuration)
                })
                .WithMessage("❌ مدت زمان بازه کاری باید حداقل 5 دقیقه باشد. مثال: اگر شروع 07:00 است، پایان باید حداقل 07:05 باشد.")
                .WithErrorCode("TOO_SHORT_TIME_RANGE")
                .When(x => x.StartTime != TimeSpan.Zero && x.EndTime != TimeSpan.Zero);

            // اعتبارسنجی حداکثر مدت زمان
            RuleFor(x => x.EndTime)
                .Must((timeRange, endTime) => (endTime - timeRange.StartTime).TotalMinutes <= 480)
                .WithMessage("حداکثر مدت زمان هر بازه باید 8 ساعت باشد.")
                .WithErrorCode("TOO_LONG_TIME_RANGE");

            // اعتبارسنجی وضعیت فعال
            RuleFor(x => x.IsActive)
                .NotNull()
                .WithMessage("وضعیت فعال بودن بازه زمانی الزامی است.")
                .WithErrorCode("EMPTY_IS_ACTIVE");
        }
    }

    /// <summary>
    /// مدل آیتم برنامه کاری برای نمایش در View
    /// </summary>
    public class ScheduleItemViewModel
    {
        /// <summary>
        /// شناسه برنامه
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// عنوان برنامه
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// روز هفته
        /// </summary>
        public string DayOfWeek { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// زمان شروع
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// وضعیت فعال
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// زمان‌های کاری
        /// </summary>
        public List<ScheduleTimeSlotViewModel> TimeSlots { get; set; } = new List<ScheduleTimeSlotViewModel>();

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// مدل زمان کاری برای نمایش در View
    /// </summary>
    public class ScheduleTimeSlotViewModel
    {
        /// <summary>
        /// زمان شروع
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// نوع زمان
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// مدل نمای کلی برنامه‌های کاری
    /// </summary>
    public class ScheduleOverviewViewModel
    {
        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int? ClinicId { get; set; }

        /// <summary>
        /// شناسه بخش
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// تعداد کل پزشکان
        /// </summary>
        public int TotalDoctors { get; set; }

        /// <summary>
        /// تعداد برنامه‌های فعال
        /// </summary>
        public int ActiveSchedules { get; set; }

        /// <summary>
        /// تعداد کل نوبت‌ها
        /// </summary>
        public int TotalAppointments { get; set; }

        /// <summary>
        /// نام کلینیک
        /// </summary>
        public string ClinicName { get; set; }

        /// <summary>
        /// نام بخش
        /// </summary>
        public string DepartmentName { get; set; }
    }

    /// <summary>
    /// ViewModel برای تاریخ‌های خاص (نه هفتگی)
    /// </summary>
    public class SpecificDateViewModel
    {
        /// <summary>
        /// تاریخ شمسی (مثلاً: 1404/10/17)
        /// </summary>
        [Required(ErrorMessage = "تاریخ الزامی است.")]
        public string PersianDate { get; set; }

        /// <summary>
        /// زمان شروع
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// مدت زمان هر نوبت (به دقیقه)
        /// </summary>
        [Range(5, 120, ErrorMessage = "مدت زمان نوبت باید بین 5 تا 120 دقیقه باشد.")]
        public int AppointmentDuration { get; set; } = 30;
    }
}
