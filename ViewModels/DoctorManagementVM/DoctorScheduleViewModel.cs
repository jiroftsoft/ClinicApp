using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
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
        [Required(ErrorMessage = "حداقل یک روز کاری باید تعیین شود.")]
        public List<WorkDayViewModel> WorkDays { get; set; } = new List<WorkDayViewModel>();

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
        public string DayOfWeek => WorkDays?.FirstOrDefault(w => w.IsActive)?.DayName ?? "نامشخص";

        /// <summary>
        /// زمان شروع (برای سازگاری با View)
        /// </summary>
        public TimeSpan StartTime => WorkDays?.FirstOrDefault(w => w.IsActive)?.TimeRanges?.FirstOrDefault()?.StartTime ?? TimeSpan.Zero;

        /// <summary>
        /// زمان پایان (برای سازگاری با View)
        /// </summary>
        public TimeSpan EndTime => WorkDays?.FirstOrDefault(w => w.IsActive)?.TimeRanges?.FirstOrDefault()?.EndTime ?? TimeSpan.Zero;

        /// <summary>
        /// وضعیت فعال (برای سازگاری با View)
        /// </summary>
        public bool IsActive => WorkDays?.Any(w => w.IsActive) ?? false;

        /// <summary>
        /// تاریخ شروع (برای سازگاری با View)
        /// </summary>
        public DateTime? StartDate => DateTime.Today;

        /// <summary>
        /// تاریخ پایان (برای سازگاری با View)
        /// </summary>
        public DateTime? EndDate => null; // نامحدود

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
                            Title = $"{workDay.DayName} - {timeRange.StartTime:HH:mm} تا {timeRange.EndTime:HH:mm}",
                            DayOfWeek = workDay.DayName,
                            StartDate = DateTime.Today,
                            EndDate = null,
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
            if (doctorSchedule == null) return null;
            return new DoctorScheduleViewModel
            {
                Id = doctorSchedule.ScheduleId, // استفاده از ScheduleId به جای Id
                DoctorId = doctorSchedule.DoctorId,
                AppointmentDuration = doctorSchedule.AppointmentDuration,
                DefaultStartTime = doctorSchedule.DefaultStartTime,
                DefaultEndTime = doctorSchedule.DefaultEndTime,
                CreatedAt = doctorSchedule.CreatedAt,
                CreatedBy = doctorSchedule.CreatedByUserId,
                UpdatedAt = doctorSchedule.UpdatedAt,
                UpdatedBy = doctorSchedule.UpdatedByUserId,
                WorkDays = doctorSchedule.WorkDays?.Select(WorkDayViewModel.FromEntity).ToList() ?? new List<WorkDayViewModel>(),
                // پر کردن اطلاعات پزشک
                DoctorName = doctorSchedule.Doctor?.FullName,
                NationalCode = doctorSchedule.Doctor?.NationalCode,
                MedicalCouncilCode = doctorSchedule.Doctor?.MedicalCouncilCode,
                SpecializationNames = doctorSchedule.Doctor?.DoctorSpecializations?.Select(ds => ds.Specialization?.Name).Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>()
            };
        }

        /// <summary>
        /// ✅ تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// </summary>
        public DoctorSchedule ToEntity()
        {
            return new DoctorSchedule
            {
                ScheduleId = this.Id, // استفاده از ScheduleId به جای Id
                DoctorId = this.DoctorId,
                AppointmentDuration = this.AppointmentDuration,
                DefaultStartTime = this.DefaultStartTime,
                DefaultEndTime = this.DefaultEndTime,
                CreatedAt = this.CreatedAt,
                CreatedByUserId = this.CreatedBy,
                UpdatedAt = this.UpdatedAt,
                UpdatedByUserId = this.UpdatedBy,
                WorkDays = this.WorkDays?.Select(wd => wd.ToEntity()).ToList() ?? new List<DoctorWorkDay>()
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
            if (workDay == null) return null;
            return new WorkDayViewModel
            {
                Id = workDay.WorkDayId, // استفاده از WorkDayId به جای Id
                DayOfWeek = workDay.DayOfWeek,
                IsActive = workDay.IsActive,
                TimeRanges = workDay.TimeRanges?.Select(TimeRangeViewModel.FromEntity).ToList() ?? new List<TimeRangeViewModel>()
            };
        }

        /// <summary>
        /// ✅ تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// </summary>
        public DoctorWorkDay ToEntity()
        {
            return new DoctorWorkDay
            {
                WorkDayId = this.Id, // استفاده از WorkDayId به جای Id
                DayOfWeek = this.DayOfWeek,
                IsActive = this.IsActive,
                TimeRanges = this.TimeRanges?.Select(tr => tr.ToEntity()).ToList() ?? new List<DoctorTimeRange>()
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
        /// زمان پایان بازه
        /// </summary>
        [Required(ErrorMessage = "زمان پایان الزامی است.")]
        public TimeSpan EndTime { get; set; }

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
            return new TimeRangeViewModel
            {
                Id = timeRange.TimeRangeId, // Assuming DoctorTimeRange has an Id
                StartTime = timeRange.StartTime,
                EndTime = timeRange.EndTime,
                IsActive = timeRange.IsActive
            };
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
    /// ولیدیتور برای مدل برنامه کاری پزشک
    /// </summary>
    public class DoctorScheduleViewModelValidator : AbstractValidator<DoctorScheduleViewModel>
    {
        public DoctorScheduleViewModelValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.");

            RuleFor(x => x.AppointmentDuration)
                .InclusiveBetween(5, 120)
                .WithMessage("مدت زمان نوبت باید بین 5 تا 120 دقیقه باشد.");

            RuleFor(x => x.WorkDays)
                .NotEmpty()
                .WithMessage("حداقل یک روز کاری باید تعیین شود.");

            RuleForEach(x => x.WorkDays)
                .SetValidator(new WorkDayViewModelValidator());
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل روز کاری
    /// </summary>
    public class WorkDayViewModelValidator : AbstractValidator<WorkDayViewModel>
    {
        public WorkDayViewModelValidator()
        {
            RuleFor(x => x.DayOfWeek)
                .InclusiveBetween(0, 6)
                .WithMessage("شماره روز هفته باید بین 0 تا 6 باشد.");

            RuleForEach(x => x.TimeRanges)
                .SetValidator(new TimeRangeViewModelValidator());
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل بازه زمانی
    /// </summary>
    public class TimeRangeViewModelValidator : AbstractValidator<TimeRangeViewModel>
    {
        public TimeRangeViewModelValidator()
        {
            RuleFor(x => x.StartTime)
                .NotEqual(TimeSpan.Zero)
                .WithMessage("زمان شروع الزامی است.");

            RuleFor(x => x.EndTime)
                .NotEqual(TimeSpan.Zero)
                .WithMessage("زمان پایان الزامی است.");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .WithMessage("زمان پایان باید بعد از زمان شروع باشد.");
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
}
