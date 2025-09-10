using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// درخواست بررسی تاریخ‌های در دسترس
    /// </summary>
    public class AvailableDatesRequest
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        [Required(ErrorMessage = "تاریخ شروع الزامی است.")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        [Required(ErrorMessage = "تاریخ پایان الزامی است.")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// نوع نوبت
        /// </summary>
        [Display(Name = "نوع نوبت")]
        public AppointmentType? AppointmentType { get; set; }

        /// <summary>
        /// مدت زمان نوبت (دقیقه)
        /// </summary>
        [Display(Name = "مدت زمان نوبت (دقیقه)")]
        [Range(15, 480, ErrorMessage = "مدت زمان نوبت باید بین ۱۵ تا ۴۸۰ دقیقه باشد.")]
        public int DurationMinutes { get; set; } = 30;

        /// <summary>
        /// تاریخ شروع نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        public string StartDateDisplay => StartDate.ToString("yyyy/MM/dd");

        /// <summary>
        /// تاریخ پایان نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public string EndDateDisplay => EndDate.ToString("yyyy/MM/dd");

        /// <summary>
        /// تعداد روزهای درخواستی
        /// </summary>
        [Display(Name = "تعداد روزها")]
        public int DaysCount => (EndDate - StartDate).Days + 1;
    }

    /// <summary>
    /// نتیجه بررسی تاریخ‌های در دسترس
    /// </summary>
    public class AvailableDatesResult
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// تاریخ‌های در دسترس
        /// </summary>
        [Display(Name = "تاریخ‌های در دسترس")]
        public List<AvailableDateInfo> AvailableDates { get; set; } = new List<AvailableDateInfo>();

        /// <summary>
        /// تعداد کل تاریخ‌های در دسترس
        /// </summary>
        [Display(Name = "تعداد کل تاریخ‌های در دسترس")]
        public int TotalAvailableDates => AvailableDates?.Count ?? 0;

        /// <summary>
        /// درصد دسترسی‌پذیری
        /// </summary>
        [Display(Name = "درصد دسترسی‌پذیری")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal AvailabilityPercentage
        {
            get
            {
                var totalDays = (EndDate - StartDate).Days + 1;
                return totalDays > 0 ? (decimal)TotalAvailableDates / totalDays : 0;
            }
        }

        /// <summary>
        /// تاریخ شروع نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        public string StartDateDisplay => StartDate.ToString("yyyy/MM/dd");

        /// <summary>
        /// تاریخ پایان نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public string EndDateDisplay => EndDate.ToString("yyyy/MM/dd");
    }

    /// <summary>
    /// اطلاعات تاریخ در دسترس
    /// </summary>
    public class AvailableDateInfo
    {
        /// <summary>
        /// تاریخ
        /// </summary>
        [Display(Name = "تاریخ")]
        public DateTime Date { get; set; }

        /// <summary>
        /// روز هفته
        /// </summary>
        [Display(Name = "روز هفته")]
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// آیا در دسترس است؟
        /// </summary>
        [Display(Name = "در دسترس")]
        public bool IsAvailable { get; set; }

        /// <summary>
        /// تعداد اسلات‌های در دسترس
        /// </summary>
        [Display(Name = "تعداد اسلات‌های در دسترس")]
        public int AvailableSlots { get; set; }

        /// <summary>
        /// تعداد کل اسلات‌ها
        /// </summary>
        [Display(Name = "تعداد کل اسلات‌ها")]
        public int TotalSlots { get; set; }

        /// <summary>
        /// درصد دسترسی‌پذیری روز
        /// </summary>
        [Display(Name = "درصد دسترسی‌پذیری")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal DayAvailabilityPercentage => TotalSlots > 0 ? (decimal)AvailableSlots / TotalSlots : 0;

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string DateDisplay => Date.ToString("yyyy/MM/dd");

        /// <summary>
        /// روز هفته نمایشی (فارسی)
        /// </summary>
        [Display(Name = "روز هفته")]
        public string DayOfWeekDisplay
        {
            get
            {
                return DayOfWeek switch
                {
                    DayOfWeek.Saturday => "شنبه",
                    DayOfWeek.Sunday => "یکشنبه",
                    DayOfWeek.Monday => "دوشنبه",
                    DayOfWeek.Tuesday => "سه‌شنبه",
                    DayOfWeek.Wednesday => "چهارشنبه",
                    DayOfWeek.Thursday => "پنج‌شنبه",
                    DayOfWeek.Friday => "جمعه",
                    _ => "نامشخص"
                };
            }
        }

        /// <summary>
        /// کلاس CSS برای نمایش وضعیت
        /// </summary>
        public string AvailabilityCssClass
        {
            get
            {
                if (!IsAvailable) return "table-danger";
                if (DayAvailabilityPercentage >= 0.8m) return "table-success";
                if (DayAvailabilityPercentage >= 0.5m) return "table-warning";
                return "table-danger";
            }
        }

        /// <summary>
        /// نمایش وضعیت (برای سازگاری با Views)
        /// </summary>
        public string StatusDisplay
        {
            get
            {
                if (!IsAvailable) return "غیرفعال";
                if (DayAvailabilityPercentage >= 0.8m) return "عالی";
                if (DayAvailabilityPercentage >= 0.5m) return "متوسط";
                return "ضعیف";
            }
        }
    }

    /// <summary>
    /// درخواست بررسی اسلات‌های زمانی در دسترس
    /// </summary>
    public class AvailableTimeSlotsRequest
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تاریخ مورد نظر
        /// </summary>
        [Display(Name = "تاریخ مورد نظر")]
        [Required(ErrorMessage = "تاریخ مورد نظر الزامی است.")]
        public DateTime Date { get; set; }

        /// <summary>
        /// نوع نوبت
        /// </summary>
        [Display(Name = "نوع نوبت")]
        public AppointmentType? AppointmentType { get; set; }

        /// <summary>
        /// مدت زمان نوبت (دقیقه)
        /// </summary>
        [Display(Name = "مدت زمان نوبت (دقیقه)")]
        [Range(15, 480, ErrorMessage = "مدت زمان نوبت باید بین ۱۵ تا ۴۸۰ دقیقه باشد.")]
        public int DurationMinutes { get; set; } = 30;

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string DateDisplay => Date.ToString("yyyy/MM/dd");

        /// <summary>
        /// روز هفته
        /// </summary>
        [Display(Name = "روز هفته")]
        public DayOfWeek DayOfWeek => Date.DayOfWeek;

        /// <summary>
        /// روز هفته نمایشی (فارسی)
        /// </summary>
        [Display(Name = "روز هفته")]
        public string DayOfWeekDisplay
        {
            get
            {
                return DayOfWeek switch
                {
                    DayOfWeek.Saturday => "شنبه",
                    DayOfWeek.Sunday => "یکشنبه",
                    DayOfWeek.Monday => "دوشنبه",
                    DayOfWeek.Tuesday => "سه‌شنبه",
                    DayOfWeek.Wednesday => "چهارشنبه",
                    DayOfWeek.Thursday => "پنج‌شنبه",
                    DayOfWeek.Friday => "جمعه",
                    _ => "نامشخص"
                };
            }
        }
    }

    /// <summary>
    /// نتیجه بررسی اسلات‌های زمانی در دسترس
    /// </summary>
    public class AvailableTimeSlotsResult
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تاریخ مورد نظر
        /// </summary>
        [Display(Name = "تاریخ")]
        public DateTime Date { get; set; }

        /// <summary>
        /// اسلات‌های زمانی در دسترس
        /// </summary>
        [Display(Name = "اسلات‌های زمانی در دسترس")]
        public List<TimeSlotViewModel> AvailableTimeSlots { get; set; } = new List<TimeSlotViewModel>();

        /// <summary>
        /// تعداد کل اسلات‌های در دسترس
        /// </summary>
        [Display(Name = "تعداد کل اسلات‌های در دسترس")]
        public int TotalAvailableSlots => AvailableTimeSlots?.Count ?? 0;

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string DateDisplay => Date.ToString("yyyy/MM/dd");

        /// <summary>
        /// روز هفته
        /// </summary>
        [Display(Name = "روز هفته")]
        public DayOfWeek DayOfWeek => Date.DayOfWeek;

        /// <summary>
        /// روز هفته نمایشی (فارسی)
        /// </summary>
        [Display(Name = "روز هفته")]
        public string DayOfWeekDisplay
        {
            get
            {
                return DayOfWeek switch
                {
                    DayOfWeek.Saturday => "شنبه",
                    DayOfWeek.Sunday => "یکشنبه",
                    DayOfWeek.Monday => "دوشنبه",
                    DayOfWeek.Tuesday => "سه‌شنبه",
                    DayOfWeek.Wednesday => "چهارشنبه",
                    DayOfWeek.Thursday => "پنج‌شنبه",
                    DayOfWeek.Friday => "جمعه",
                    _ => "نامشخص"
                };
            }
        }
    }

    /// <summary>
    /// اسلات زمانی
    /// </summary>
    public class TimeSlotViewModel
    {
        /// <summary>
        /// شناسه اسلات (برای سازگاری با Views)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// شناسه اسلات
        /// </summary>
        public int SlotId { get; set; }

        /// <summary>
        /// تاریخ اسلات
        /// </summary>
        [Display(Name = "تاریخ")]
        public DateTime SlotDate { get; set; }

        /// <summary>
        /// زمان شروع
        /// </summary>
        [Display(Name = "زمان شروع")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان
        /// </summary>
        [Display(Name = "زمان پایان")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// مدت زمان (دقیقه)
        /// </summary>
        [Display(Name = "مدت زمان")]
        public int Duration { get; set; }

        /// <summary>
        /// آیا در دسترس است؟
        /// </summary>
        [Display(Name = "در دسترس")]
        public bool IsAvailable { get; set; }

        /// <summary>
        /// نوع نوبت
        /// </summary>
        [Display(Name = "نوع نوبت")]
        public AppointmentType? AppointmentType { get; set; }

        /// <summary>
        /// نوع اسلات (برای سازگاری با Views)
        /// </summary>
        [Display(Name = "نوع اسلات")]
        public string Type
        {
            get
            {
                if (!AppointmentType.HasValue)
                    return "نامشخص";

                return AppointmentType.Value switch
                {
                    Models.Entities.AppointmentType.GeneralVisit => "عادی",
                    Models.Entities.AppointmentType.SpecialistVisit => "تخصصی",
                    Models.Entities.AppointmentType.SubSpecialistVisit => "فوق‌تخصصی",
                    Models.Entities.AppointmentType.InitialExamination => "اولیه",
                    Models.Entities.AppointmentType.FollowUp => "پیگیری",
                    Models.Entities.AppointmentType.Consultation => "مشاوره",
                    Models.Entities.AppointmentType.Emergency => "فوری",
                    Models.Entities.AppointmentType.Cancellation => "کنسلی",
                    // سایر موارد تشخیصی/درمانی
                    Models.Entities.AppointmentType.Laboratory or
                        Models.Entities.AppointmentType.Imaging or
                        Models.Entities.AppointmentType.Vaccination or
                        Models.Entities.AppointmentType.Injection or
                        Models.Entities.AppointmentType.MedicalProcedure => "خدمات",

                    _ => "نامشخص"
                };
            }
        }
        /// <summary>
        /// شناسه نوبت موجود (اگر رزرو شده)
        /// </summary>
        public int? ExistingAppointmentId { get; set; }

        /// <summary>
        /// نام بیمار (اگر رزرو شده)
        /// </summary>
        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        /// <summary>
        /// قیمت اسلات
        /// </summary>
        [Display(Name = "قیمت")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        /// <summary>
        /// وضعیت اسلات
        /// </summary>
        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        /// <summary>
        /// نمایش وضعیت اسلات (برای سازگاری با Views)
        /// </summary>
        [Display(Name = "نمایش وضعیت")]
        public string StatusDisplay 
        { 
            get 
            {
                if (IsAvailable)
                    return "در دسترس";
                else if (!string.IsNullOrEmpty(PatientName))
                    return "رزرو شده";
                else if (IsEmergencySlot)
                    return "اورژانس";
                else
                    return Status ?? "نامشخص";
            }
        }

        /// <summary>
        /// آیا اسلات اورژانس است؟
        /// </summary>
        [Display(Name = "اورژانس")]
        public bool IsEmergencySlot { get; set; }

        /// <summary>
        /// آیا ورود بدون نوبت مجاز است؟
        /// </summary>
        [Display(Name = "ورود بدون نوبت")]
        public bool IsWalkInAllowed { get; set; }

        /// <summary>
        /// اولویت اسلات
        /// </summary>
        [Display(Name = "اولویت")]
        public string Priority { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        [Display(Name = "تخصص")]
        public string Specialization { get; set; }

        /// <summary>
        /// نام کلینیک
        /// </summary>
        [Display(Name = "نام کلینیک")]
        public string ClinicName { get; set; }

        /// <summary>
        /// آدرس کلینیک
        /// </summary>
        [Display(Name = "آدرس کلینیک")]
        public string ClinicAddress { get; set; }

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string SlotDateDisplay => SlotDate.ToString("yyyy/MM/dd");

        /// <summary>
        /// تاریخ نمایشی (برای سازگاری با Views)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string DateDisplay => SlotDate.ToString("yyyy/MM/dd");

        /// <summary>
        /// زمان شروع نمایشی (فارسی)
        /// </summary>
        [Display(Name = "زمان شروع")]
        public string StartTimeDisplay => StartTime.ToString(@"hh\:mm");

        /// <summary>
        /// زمان پایان نمایشی (فارسی)
        /// </summary>
        [Display(Name = "زمان پایان")]
        public string EndTimeDisplay => EndTime.ToString(@"hh\:mm");

        /// <summary>
        /// کلاس CSS برای نمایش وضعیت
        /// </summary>
        public string StatusCssClass
        {
            get
            {
                if (!IsAvailable) return "table-danger";
                if (ExistingAppointmentId.HasValue) return "table-warning";
                return "table-success";
            }
        }

        /// <summary>
        /// متن وضعیت
        /// </summary>
        public string StatusText
        {
            get
            {
                if (!IsAvailable) return "غیرفعال";
                if (ExistingAppointmentId.HasValue) return "رزرو شده";
                return "در دسترس";
            }
        }
    }

    /// <summary>
    /// درخواست رزرو موقت اسلات
    /// </summary>
    public class ReserveSlotRequest
    {
        /// <summary>
        /// شناسه اسلات
        /// </summary>
        [Required(ErrorMessage = "شناسه اسلات الزامی است.")]
        public int SlotId { get; set; }

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        [Required(ErrorMessage = "بیمار الزامی است.")]
        public int PatientId { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        /// <summary>
        /// مدت زمان رزرو (دقیقه)
        /// </summary>
        [Display(Name = "مدت زمان رزرو (دقیقه)")]
        [Required(ErrorMessage = "مدت زمان رزرو الزامی است.")]
        [Range(15, 480, ErrorMessage = "مدت زمان رزرو باید بین ۱۵ تا ۴۸۰ دقیقه باشد.")]
        public int DurationMinutes { get; set; }

        /// <summary>
        /// نوع نوبت
        /// </summary>
        [Display(Name = "نوع نوبت")]
        public AppointmentType AppointmentType { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [Display(Name = "توضیحات")]
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از ۵۰۰ کاراکتر باشد.")]
        public string Notes { get; set; }
    }

    /// <summary>
    /// نتیجه رزرو موقت اسلات
    /// </summary>
    public class ReserveSlotResult
    {
        /// <summary>
        /// آیا رزرو موفق بود؟
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// پیام نتیجه
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// شناسه رزرو موقت
        /// </summary>
        public int? TemporaryReservationId { get; set; }

        /// <summary>
        /// زمان انقضای رزرو موقت
        /// </summary>
        [Display(Name = "زمان انقضا")]
        public DateTime? ExpiryTime { get; set; }

        /// <summary>
        /// زمان انقضای نمایشی (فارسی)
        /// </summary>
        [Display(Name = "زمان انقضا")]
        public string ExpiryTimeDisplay => ExpiryTime?.ToString("yyyy/MM/dd HH:mm") ?? "نامشخص";
    }

    /// <summary>
    /// درخواست تولید اسلات‌های هفتگی
    /// </summary>
    public class GenerateWeeklySlotsRequest
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تاریخ شروع هفته
        /// </summary>
        [Display(Name = "تاریخ شروع هفته")]
        [Required(ErrorMessage = "تاریخ شروع هفته الزامی است.")]
        public DateTime WeekStart { get; set; }

        /// <summary>
        /// مدت زمان هر اسلات (دقیقه)
        /// </summary>
        [Display(Name = "مدت زمان هر اسلات (دقیقه)")]
        [Required(ErrorMessage = "مدت زمان اسلات الزامی است.")]
        [Range(15, 480, ErrorMessage = "مدت زمان اسلات باید بین ۱۵ تا ۴۸۰ دقیقه باشد.")]
        public int SlotDurationMinutes { get; set; } = 30;

        /// <summary>
        /// آیا شامل آخر هفته باشد؟
        /// </summary>
        [Display(Name = "شامل آخر هفته")]
        public bool IncludeWeekend { get; set; } = false;

        /// <summary>
        /// تاریخ شروع هفته نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ شروع هفته")]
        public string WeekStartDisplay => WeekStart.ToString("yyyy/MM/dd");

        /// <summary>
        /// تاریخ پایان هفته
        /// </summary>
        [Display(Name = "تاریخ پایان هفته")]
        public DateTime WeekEnd => WeekStart.AddDays(6);

        /// <summary>
        /// تاریخ پایان هفته نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ پایان هفته")]
        public string WeekEndDisplay => WeekEnd.ToString("yyyy/MM/dd");
    }

    /// <summary>
    /// درخواست تولید اسلات‌های ماهانه
    /// </summary>
    public class GenerateMonthlySlotsRequest
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// ماه مورد نظر
        /// </summary>
        [Display(Name = "ماه مورد نظر")]
        [Required(ErrorMessage = "ماه مورد نظر الزامی است.")]
        public DateTime MonthStart { get; set; }

        /// <summary>
        /// مدت زمان هر اسلات (دقیقه)
        /// </summary>
        [Display(Name = "مدت زمان هر اسلات (دقیقه)")]
        [Required(ErrorMessage = "مدت زمان اسلات الزامی است.")]
        [Range(15, 480, ErrorMessage = "مدت زمان اسلات باید بین ۱۵ تا ۴۸۰ دقیقه باشد.")]
        public int SlotDurationMinutes { get; set; } = 30;

        /// <summary>
        /// آیا شامل آخر هفته باشد؟
        /// </summary>
        [Display(Name = "شامل آخر هفته")]
        public bool IncludeWeekend { get; set; } = false;

        /// <summary>
        /// ماه مورد نظر نمایشی (فارسی)
        /// </summary>
        [Display(Name = "ماه مورد نظر")]
        public string MonthStartDisplay => MonthStart.ToString("yyyy/MM");

        /// <summary>
        /// تعداد روزهای ماه
        /// </summary>
        [Display(Name = "تعداد روزهای ماه")]
        public int DaysInMonth => DateTime.DaysInMonth(MonthStart.Year, MonthStart.Month);

        /// <summary>
        /// تاریخ پایان ماه
        /// </summary>
        [Display(Name = "تاریخ پایان ماه")]
        public DateTime MonthEnd => MonthStart.AddMonths(1).AddDays(-1);

        /// <summary>
        /// تاریخ پایان ماه نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ پایان ماه")]
        public string MonthEndDisplay => MonthEnd.ToString("yyyy/MM/dd");
    }

    /// <summary>
    /// درخواست آزادسازی اسلات
    /// </summary>
    public class ReleaseSlotRequest
    {
        /// <summary>
        /// شناسه اسلات
        /// </summary>
        [Required(ErrorMessage = "شناسه اسلات الزامی است.")]
        public int SlotId { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// تاریخ رزرو
        /// </summary>
        [Display(Name = "تاریخ رزرو")]
        [Required(ErrorMessage = "تاریخ رزرو الزامی است.")]
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// دلیل آزادسازی
        /// </summary>
        [Display(Name = "دلیل آزادسازی")]
        [Required(ErrorMessage = "دلیل آزادسازی الزامی است.")]
        public string ReleaseReason { get; set; }

        /// <summary>
        /// نوع آزادسازی
        /// </summary>
        [Display(Name = "نوع آزادسازی")]
        public string ReleaseType { get; set; } = "scheduled";

        /// <summary>
        /// تاریخ آزادسازی
        /// </summary>
        [Display(Name = "تاریخ آزادسازی")]
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// زمان آزادسازی
        /// </summary>
        [Display(Name = "زمان آزادسازی")]
        public TimeSpan? ReleaseTime { get; set; }

        /// <summary>
        /// توضیحات تفصیلی
        /// </summary>
        [Display(Name = "توضیحات تفصیلی")]
        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از ۱۰۰۰ کاراکتر باشد.")]
        public string DetailedReason { get; set; }

        /// <summary>
        /// آیا به بیمار اطلاع داده شود؟
        /// </summary>
        [Display(Name = "اعلان به بیمار")]
        public bool NotifyPatient { get; set; } = true;

        /// <summary>
        /// آیا به پزشک اطلاع داده شود؟
        /// </summary>
        [Display(Name = "اعلان به پزشک")]
        public bool NotifyDoctor { get; set; } = true;

        /// <summary>
        /// آیا نیاز به بازپرداخت دارد؟
        /// </summary>
        [Display(Name = "نیاز به بازپرداخت")]
        public bool RefundRequired { get; set; } = false;

        /// <summary>
        /// آیا پیشنهاد تغییر زمان داده شود؟
        /// </summary>
        [Display(Name = "پیشنهاد تغییر زمان")]
        public bool RescheduleOffered { get; set; } = true;

        /// <summary>
        /// تاریخ رزرو نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ رزرو")]
        public string ReservationDateDisplay => ReservationDate.ToString("yyyy/MM/dd");

        /// <summary>
        /// تاریخ آزادسازی نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ آزادسازی")]
        public string ReleaseDateDisplay => ReleaseDate?.ToString("yyyy/MM/dd") ?? "فوری";

        /// <summary>
        /// زمان آزادسازی نمایشی (فارسی)
        /// </summary>
        [Display(Name = "زمان آزادسازی")]
        public string ReleaseTimeDisplay => ReleaseTime?.ToString(@"hh\:mm") ?? "فوری";
    }

    /// <summary>
    /// نتیجه آزادسازی اسلات
    /// </summary>
    public class ReleaseSlotResult
    {
        /// <summary>
        /// آیا آزادسازی موفق بود؟
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// پیام نتیجه
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// شناسه آزادسازی
        /// </summary>
        public int? ReleaseId { get; set; }

        /// <summary>
        /// زمان آزادسازی
        /// </summary>
        [Display(Name = "زمان آزادسازی")]
        public DateTime ReleaseTime { get; set; }

        /// <summary>
        /// زمان آزادسازی نمایشی (فارسی)
        /// </summary>
        [Display(Name = "زمان آزادسازی")]
        public string ReleaseTimeDisplay => ReleaseTime.ToString("yyyy/MM/dd HH:mm");
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات اسلات
    /// </summary>
    public class SlotDetailsViewModel
    {
        /// <summary>
        /// شناسه اسلات
        /// </summary>
        public int SlotId { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        [Display(Name = "تخصص")]
        public string DoctorSpecialty { get; set; }

        /// <summary>
        /// تاریخ اسلات
        /// </summary>
        [Display(Name = "تاریخ")]
        public DateTime SlotDate { get; set; }

        /// <summary>
        /// زمان شروع
        /// </summary>
        [Display(Name = "زمان شروع")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان
        /// </summary>
        [Display(Name = "زمان پایان")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// مدت زمان اسلات (دقیقه)
        /// </summary>
        [Display(Name = "مدت زمان")]
        public int Duration { get; set; }

        /// <summary>
        /// نوع اسلات
        /// </summary>
        [Display(Name = "نوع اسلات")]
        public string SlotType { get; set; }

        /// <summary>
        /// قیمت اسلات (ریال)
        /// </summary>
        [Display(Name = "قیمت")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        /// <summary>
        /// وضعیت اسلات
        /// </summary>
        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        /// <summary>
        /// آیا اسلات در دسترس است؟
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// آیا اسلات رزرو شده است؟
        /// </summary>
        public bool IsBooked { get; set; }

        /// <summary>
        /// اطلاعات بیمار (اگر رزرو شده)
        /// </summary>
        public PatientInfoViewModel PatientInfo { get; set; }

        /// <summary>
        /// تاریخ‌چه اسلات
        /// </summary>
        public List<SlotHistoryViewModel> SlotHistory { get; set; } = new List<SlotHistoryViewModel>();

        /// <summary>
        /// آمار اسلات
        /// </summary>
        public SlotStatisticsViewModel SlotStatistics { get; set; } = new SlotStatisticsViewModel();

        /// <summary>
        /// اسلات‌های مرتبط
        /// </summary>
        public List<RelatedSlotViewModel> RelatedSlots { get; set; } = new List<RelatedSlotViewModel>();

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string SlotDateDisplay => SlotDate.ToString("yyyy/MM/dd");

        /// <summary>
        /// زمان شروع نمایشی (فارسی)
        /// </summary>
        [Display(Name = "زمان شروع")]
        public string StartTimeDisplay => StartTime.ToString(@"hh\:mm");

        /// <summary>
        /// زمان پایان نمایشی (فارسی)
        /// </summary>
        [Display(Name = "زمان پایان")]
        public string EndTimeDisplay => EndTime.ToString(@"hh\:mm");

        /// <summary>
        /// روز هفته
        /// </summary>
        [Display(Name = "روز هفته")]
        public DayOfWeek DayOfWeek => SlotDate.DayOfWeek;

        /// <summary>
        /// روز هفته نمایشی (فارسی)
        /// </summary>
        [Display(Name = "روز هفته")]
        public string DayOfWeekDisplay
        {
            get
            {
                return DayOfWeek switch
                {
                    DayOfWeek.Saturday => "شنبه",
                    DayOfWeek.Sunday => "یکشنبه",
                    DayOfWeek.Monday => "دوشنبه",
                    DayOfWeek.Tuesday => "سه‌شنبه",
                    DayOfWeek.Wednesday => "چهارشنبه",
                    DayOfWeek.Thursday => "پنج‌شنبه",
                    DayOfWeek.Friday => "جمعه",
                    _ => "نامشخص"
                };
            }
        }

        /// <summary>
        /// کلاس CSS برای نمایش وضعیت
        /// </summary>
        public string StatusCssClass
        {
            get
            {
                if (!IsAvailable) return "table-danger";
                if (IsBooked) return "table-warning";
                return "table-success";
            }
        }

        /// <summary>
        /// متن وضعیت
        /// </summary>
        public string StatusText
        {
            get
            {
                if (!IsAvailable) return "غیرفعال";
                if (IsBooked) return "رزرو شده";
                return "در دسترس";
            }
        }
    }

    /// <summary>
    /// اطلاعات بیمار
    /// </summary>
    public class PatientInfoViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// نام کامل بیمار
        /// </summary>
        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// کد ملی
        /// </summary>
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        /// <summary>
        /// سن
        /// </summary>
        [Display(Name = "سن")]
        public int? Age { get; set; }

        /// <summary>
        /// نوع ویزیت
        /// </summary>
        [Display(Name = "نوع ویزیت")]
        public AppointmentType AppointmentType { get; set; }

        /// <summary>
        /// اولویت
        /// </summary>
        [Display(Name = "اولویت")]
        public string Priority { get; set; }

        /// <summary>
        /// علائم و شکایات
        /// </summary>
        [Display(Name = "علائم و شکایات")]
        public string Symptoms { get; set; }

        /// <summary>
        /// سن نمایشی
        /// </summary>
        [Display(Name = "سن")]
        public string AgeDisplay => Age?.ToString() + " سال" ?? "نامشخص";
    }

    /// <summary>
    /// تاریخ‌چه اسلات
    /// </summary>
    public class SlotHistoryViewModel
    {
        /// <summary>
        /// زمان رویداد
        /// </summary>
        [Display(Name = "زمان")]
        public TimeSpan EventTime { get; set; }

        /// <summary>
        /// عنوان رویداد
        /// </summary>
        [Display(Name = "رویداد")]
        public string EventTitle { get; set; }

        /// <summary>
        /// توضیحات رویداد
        /// </summary>
        [Display(Name = "توضیحات")]
        public string EventDescription { get; set; }

        /// <summary>
        /// زمان نمایشی (فارسی)
        /// </summary>
        [Display(Name = "زمان")]
        public string EventTimeDisplay => EventTime.ToString(@"hh\:mm");
    }

    /// <summary>
    /// آمار اسلات
    /// </summary>
    public class SlotStatisticsViewModel
    {
        /// <summary>
        /// تعداد کل رزروها
        /// </summary>
        [Display(Name = "کل رزروها")]
        public int TotalBookings { get; set; }

        /// <summary>
        /// تعداد ویزیت‌های تکمیل شده
        /// </summary>
        [Display(Name = "ویزیت‌های تکمیل شده")]
        public int CompletedAppointments { get; set; }

        /// <summary>
        /// تعداد ویزیت‌های لغو شده
        /// </summary>
        [Display(Name = "ویزیت‌های لغو شده")]
        public int CancelledAppointments { get; set; }

        /// <summary>
        /// میانگین امتیاز
        /// </summary>
        [Display(Name = "میانگین امتیاز")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public decimal AverageRating { get; set; }
    }

    /// <summary>
    /// اسلات مرتبط
    /// </summary>
    public class RelatedSlotViewModel
    {
        /// <summary>
        /// شناسه اسلات
        /// </summary>
        public int SlotId { get; set; }

        /// <summary>
        /// تاریخ
        /// </summary>
        [Display(Name = "تاریخ")]
        public DateTime Date { get; set; }

        /// <summary>
        /// زمان
        /// </summary>
        [Display(Name = "زمان")]
        public string TimeRange { get; set; }

        /// <summary>
        /// وضعیت
        /// </summary>
        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        [Display(Name = "بیمار")]
        public string PatientName { get; set; }

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string DateDisplay => Date.ToString("yyyy/MM/dd");

        /// <summary>
        /// کلاس CSS برای نمایش وضعیت
        /// </summary>
        public string StatusCssClass
        {
            get
            {
                return Status switch
                {
                    "تکمیل شده" => "table-success",
                    "رزرو شده" => "table-warning",
                    "لغو شده" => "table-danger",
                    _ => "table-secondary"
                };
            }
        }
    }
}
