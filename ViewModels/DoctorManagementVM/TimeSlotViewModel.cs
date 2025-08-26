using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل نمایش بازه زمانی در دسترس برای نوبت‌دهی پزشکی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش بازه‌های زمانی در دسترس برای نوبت‌دهی
    /// 2. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 3. رعایت استانداردهای پزشکی ایران در نوبت‌دهی
    /// 4. پشتیبانی از محاسبه خودکار زمان‌های در دسترس
    /// 5. مدیریت وضعیت نوبت‌ها (در دسترس، رزرو شده، تکمیل شده)
    /// </summary>
    public class TimeSlotViewModel
    {
        /// <summary>
        /// شناسه بازه زمانی
        /// </summary>
        public int TimeSlotId { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// تاریخ نوبت
        /// </summary>
        [Required(ErrorMessage = "تاریخ نوبت الزامی است.")]
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// تاریخ نوبت به شمسی
        /// </summary>
        public string AppointmentDateShamsi { get; set; }

        /// <summary>
        /// زمان شروع نوبت
        /// </summary>
        [Required(ErrorMessage = "زمان شروع نوبت الزامی است.")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان نوبت
        /// </summary>
        [Required(ErrorMessage = "زمان پایان نوبت الزامی است.")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// مدت زمان نوبت (به دقیقه)
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// وضعیت نوبت
        /// </summary>
        public AppointmentStatus Status { get; set; }

        /// <summary>
        /// توضیحات وضعیت نوبت
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// آیا نوبت در دسترس است
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// آیا نوبت قابل رزرو است
        /// </summary>
        public bool IsBookable { get; set; }

        /// <summary>
        /// شناسه نوبت (در صورت رزرو شده)
        /// </summary>
        public int? AppointmentId { get; set; }

        /// <summary>
        /// نام بیمار (در صورت رزرو شده)
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// شماره تلفن بیمار (در صورت رزرو شده)
        /// </summary>
        public string PatientPhone { get; set; }

        /// <summary>
        /// تاریخ ایجاد بازه زمانی
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// نام کاربر ایجاد کننده
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static TimeSlotViewModel FromEntity(DoctorTimeSlot timeSlot)
        {
            if (timeSlot == null) return null;
            return new TimeSlotViewModel
            {
                TimeSlotId = timeSlot.TimeSlotId,
                DoctorId = timeSlot.DoctorId,
                DoctorName = timeSlot.Doctor?.FullName ?? $"{timeSlot.Doctor?.FirstName} {timeSlot.Doctor?.LastName}",
                AppointmentDate = timeSlot.AppointmentDate,
                AppointmentDateShamsi = timeSlot.AppointmentDate.ToPersianDateTime(),
                StartTime = timeSlot.StartTime,
                EndTime = timeSlot.EndTime,
                Duration = timeSlot.Duration,
                Status = timeSlot.Status,
                StatusDescription = GetStatusDescription(timeSlot.Status),
                IsAvailable = timeSlot.Status == AppointmentStatus.Available,
                IsBookable = timeSlot.Status == AppointmentStatus.Available,
                AppointmentId = timeSlot.AppointmentId,
                PatientName = timeSlot.Appointment?.PatientName,
                PatientPhone = timeSlot.Appointment?.PatientPhone,
                CreatedAt = timeSlot.CreatedAt,
                CreatedBy = timeSlot.CreatedByUser?.FullName ?? timeSlot.CreatedByUserId
            };
        }

        /// <summary>
        /// ✅ تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// </summary>
        public DoctorTimeSlot ToEntity()
        {
            return new DoctorTimeSlot
            {
                TimeSlotId = this.TimeSlotId,
                DoctorId = this.DoctorId,
                AppointmentDate = this.AppointmentDate,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                Duration = this.Duration,
                Status = this.Status,
                AppointmentId = this.AppointmentId,
                CreatedAt = this.CreatedAt,
                CreatedByUserId = this.CreatedBy
            };
        }

        /// <summary>
        /// دریافت توضیحات وضعیت نوبت
        /// </summary>
        private static string GetStatusDescription(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Scheduled => "ثبت شده",
                AppointmentStatus.Completed => "انجام شده",
                AppointmentStatus.Cancelled => "لغو شده",
                AppointmentStatus.NoShow => "عدم حضور",
                AppointmentStatus.Available => "در دسترس",
                _ => "نامشخص"
            };
        }
    }



    /// <summary>
    /// ولیدیتور برای مدل بازه زمانی
    /// </summary>
    public class TimeSlotViewModelValidator : AbstractValidator<TimeSlotViewModel>
    {
        public TimeSlotViewModelValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.");

            RuleFor(x => x.AppointmentDate)
                .NotEmpty()
                .WithMessage("تاریخ نوبت الزامی است.")
                .GreaterThan(DateTime.Today.AddDays(-1))
                .WithMessage("تاریخ نوبت نمی‌تواند در گذشته باشد.");

            RuleFor(x => x.StartTime)
                .NotEqual(TimeSpan.Zero)
                .WithMessage("زمان شروع نوبت الزامی است.");

            RuleFor(x => x.EndTime)
                .NotEqual(TimeSpan.Zero)
                .WithMessage("زمان پایان نوبت الزامی است.");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .WithMessage("زمان پایان باید بعد از زمان شروع باشد.");

            RuleFor(x => x.Duration)
                .InclusiveBetween(5, 120)
                .WithMessage("مدت زمان نوبت باید بین 5 تا 120 دقیقه باشد.");

            RuleFor(x => x.DoctorName)
                .NotEmpty()
                .WithMessage("نام پزشک الزامی است.")
                .MaximumLength(200)
                .WithMessage("نام پزشک نمی‌تواند بیش از 200 کاراکتر باشد.");

            RuleFor(x => x.PatientName)
                .MaximumLength(200)
                .WithMessage("نام بیمار نمی‌تواند بیش از 200 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.PatientName));

            RuleFor(x => x.PatientPhone)
                .Must(PersianNumberHelper.IsValidPhoneNumber)
                .When(x => !string.IsNullOrEmpty(x.PatientPhone))
                .WithMessage("شماره تلفن بیمار نامعتبر است.");
        }
    }
}
