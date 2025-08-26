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
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

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
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DoctorScheduleViewModel FromEntity(DoctorSchedule doctorSchedule)
        {
            if (doctorSchedule == null) return null;
            return new DoctorScheduleViewModel
            {
                DoctorId = doctorSchedule.DoctorId,
                AppointmentDuration = doctorSchedule.AppointmentDuration,
                DefaultStartTime = doctorSchedule.DefaultStartTime,
                DefaultEndTime = doctorSchedule.DefaultEndTime,
                CreatedAt = doctorSchedule.CreatedAt,
                CreatedBy = doctorSchedule.CreatedByUser?.FullName ?? doctorSchedule.CreatedByUserId,
                UpdatedAt = doctorSchedule.UpdatedAt,
                UpdatedBy = doctorSchedule.UpdatedByUser?.FullName ?? doctorSchedule.UpdatedByUserId,
                WorkDays = doctorSchedule.WorkDays?.Select(WorkDayViewModel.FromEntity).ToList() ?? new List<WorkDayViewModel>()
            };
        }

        /// <summary>
        /// ✅ تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// </summary>
        public DoctorSchedule ToEntity()
        {
            return new DoctorSchedule
            {
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
        /// شماره روز هفته (0 = یکشنبه، 1 = دوشنبه، ...، 6 = شنبه)
        /// </summary>
        [Range(0, 6, ErrorMessage = "شماره روز هفته باید بین 0 تا 6 باشد.")]
        public int DayOfWeek { get; set; }

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
}
