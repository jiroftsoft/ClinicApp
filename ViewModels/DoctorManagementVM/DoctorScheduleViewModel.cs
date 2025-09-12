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
        public string DayOfWeek { get; set; }

        /// <summary>
        /// زمان شروع (برای سازگاری با View)
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان شروع به صورت string برای JavaScript
        /// </summary>
        public string StartTimeString => StartTime.ToString(@"hh\:mm");

        /// <summary>
        /// زمان پایان (برای سازگاری با View)
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// زمان پایان به صورت string برای JavaScript
        /// </summary>
        public string EndTimeString => EndTime.ToString(@"hh\:mm");

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
            if (doctorSchedule == null) return null;
            
            // لاگ اطلاعات برای دیباگ
            var workDaysCount = doctorSchedule.WorkDays?.Count ?? 0;
            var timeRangesCount = doctorSchedule.WorkDays?.Sum(w => w.TimeRanges?.Count ?? 0) ?? 0;
            
            // لاگ به فایل (برای دیباگ)
            System.Diagnostics.Debug.WriteLine($"FromEntity: DoctorId={doctorSchedule.DoctorId}, WorkDays={workDaysCount}, TimeRanges={timeRangesCount}");
            
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
            if (workDay == null) return null;
            
            var dayNames = new[] { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };
            
            return new WorkDayViewModel
            {
                Id = workDay.WorkDayId, // استفاده از WorkDayId به جای Id
                DayOfWeek = workDay.DayOfWeek,
                DayName = workDay.DayOfWeek >= 0 && workDay.DayOfWeek < dayNames.Length ? dayNames[workDay.DayOfWeek] : "نامشخص",
                DayOfWeekForCalendar = workDay.DayOfWeek, // Populate for calendar
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
        /// زمان شروع به صورت string برای JavaScript
        /// </summary>
        public string StartTimeString => StartTime.ToString(@"hh\:mm");

        /// <summary>
        /// زمان پایان بازه
        /// </summary>
        [Required(ErrorMessage = "زمان پایان الزامی است.")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// زمان پایان به صورت string برای JavaScript
        /// </summary>
        public string EndTimeString => EndTime.ToString(@"hh\:mm");

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
                StartTime = timeRange.StartTime, // TimeSpan نمی‌تواند null باشد
                EndTime = timeRange.EndTime, // TimeSpan نمی‌تواند null باشد
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

            // اعتبارسنجی WorkDays (اصلی)
            RuleFor(x => x.WorkDays)
                .NotEmpty()
                .WithMessage("حداقل یک روز کاری باید تعیین شود.")
                .WithErrorCode("NO_WORK_DAYS");

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

            // اعتبارسنجی WorkDays فعال
            RuleFor(x => x.WorkDays)
                .Must(workDays => workDays == null || workDays.Any(w => w.IsActive))
                .WithMessage("حداقل یک روز کاری باید فعال باشد.")
                .WithErrorCode("NO_ACTIVE_WORK_DAYS");

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

            // اعتبارسنجی نام روز هفته
            RuleFor(x => x.DayName)
                .NotEmpty()
                .WithMessage("نام روز هفته الزامی است.")
                .WithErrorCode("EMPTY_DAY_NAME");

            // اعتبارسنجی TimeRanges
            RuleFor(x => x.TimeRanges)
                .NotEmpty()
                .When(x => x.IsActive)
                .WithMessage("برای روزهای فعال، حداقل یک بازه زمانی باید تعیین شود.")
                .WithErrorCode("NO_TIME_RANGES_FOR_ACTIVE_DAY");

            // اعتبارسنجی تعداد بازه‌های زمانی
            RuleFor(x => x.TimeRanges)
                .Must(timeRanges => timeRanges == null || timeRanges.Count <= 10)
                .WithMessage("حداکثر 10 بازه زمانی در روز مجاز است.")
                .WithErrorCode("TOO_MANY_TIME_RANGES");

            // اعتبارسنجی بازه‌های زمانی تکراری
            RuleFor(x => x.TimeRanges)
                .Must(timeRanges => timeRanges == null || !HasOverlappingTimeRanges(timeRanges))
                .WithMessage("بازه‌های زمانی نباید با هم تداخل داشته باشند.")
                .WithErrorCode("OVERLAPPING_TIME_RANGES");

            // اعتبارسنجی جزئیات TimeRanges
            When(x => x.TimeRanges != null && x.TimeRanges.Any(), () =>
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

            var sortedRanges = timeRanges.OrderBy(t => t.StartTime).ToList();
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
    /// </summary>
    public class TimeRangeViewModelValidator : AbstractValidator<TimeRangeViewModel>
    {
        public TimeRangeViewModelValidator()
        {
            // اعتبارسنجی زمان شروع
            RuleFor(x => x.StartTime)
                .NotEqual(TimeSpan.Zero)
                .WithMessage("زمان شروع الزامی است.")
                .WithErrorCode("EMPTY_START_TIME");

            // اعتبارسنجی زمان پایان
            RuleFor(x => x.EndTime)
                .NotEqual(TimeSpan.Zero)
                .WithMessage("زمان پایان الزامی است.")
                .WithErrorCode("EMPTY_END_TIME");

            // اعتبارسنجی محدوده زمانی
            RuleFor(x => x.StartTime)
                .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromHours(24))
                .WithMessage("زمان شروع باید بین 00:00 تا 23:59 باشد.")
                .WithErrorCode("INVALID_START_TIME_RANGE");

            RuleFor(x => x.EndTime)
                .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromHours(24))
                .WithMessage("زمان پایان باید بین 00:00 تا 23:59 باشد.")
                .WithErrorCode("INVALID_END_TIME_RANGE");

            // اعتبارسنجی منطقی بودن زمان
            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .WithMessage("زمان پایان باید بعد از زمان شروع باشد.")
                .WithErrorCode("INVALID_TIME_ORDER");

            // اعتبارسنجی حداقل مدت زمان
            RuleFor(x => x.EndTime)
                .Must((timeRange, endTime) => (endTime - timeRange.StartTime).TotalMinutes >= 15)
                .WithMessage("حداقل مدت زمان هر بازه باید 15 دقیقه باشد.")
                .WithErrorCode("TOO_SHORT_TIME_RANGE");

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
}
