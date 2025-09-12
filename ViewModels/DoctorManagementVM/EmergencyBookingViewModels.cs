using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// درخواست رزرو اورژانس
    /// </summary>
    public class EmergencyBookingRequest
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
        /// شناسه بیمار
        /// </summary>
        [Required(ErrorMessage = "بیمار الزامی است.")]
        public int PatientId { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        [Display(Name = "نام بیمار")]
        [Required(ErrorMessage = "نام بیمار الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام بیمار نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string PatientName { get; set; }

        /// <summary>
        /// شماره تلفن بیمار
        /// </summary>
        [Display(Name = "شماره تلفن")]
        [Required(ErrorMessage = "شماره تلفن الزامی است.")]
        [MaxLength(20, ErrorMessage = "شماره تلفن نمی‌تواند بیش از 20 کاراکتر باشد.")]
        public string PatientPhone { get; set; }

        /// <summary>
        /// تاریخ نوبت
        /// </summary>
        [Display(Name = "تاریخ نوبت")]
        [Required(ErrorMessage = "تاریخ نوبت الزامی است.")]
        public DateTime Date { get; set; }

        /// <summary>
        /// زمان نوبت
        /// </summary>
        [Display(Name = "زمان نوبت")]
        [Required(ErrorMessage = "زمان نوبت الزامی است.")]
        public TimeSpan Time { get; set; }

        /// <summary>
        /// نوع اورژانس
        /// </summary>
        [Display(Name = "نوع اورژانس")]
        [Required(ErrorMessage = "نوع اورژانس الزامی است.")]
        public EmergencyType EmergencyType { get; set; }

        /// <summary>
        /// اولویت اورژانس
        /// </summary>
        [Display(Name = "اولویت اورژانس")]
        [Required(ErrorMessage = "اولویت اورژانس الزامی است.")]
        public EmergencyPriority Priority { get; set; }

        /// <summary>
        /// دلیل اورژانس
        /// </summary>
        [Display(Name = "دلیل اورژانس")]
        [Required(ErrorMessage = "دلیل اورژانس الزامی است.")]
        [MaxLength(500, ErrorMessage = "دلیل اورژانس نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string EmergencyReason { get; set; }

        /// <summary>
        /// علائم بالینی
        /// </summary>
        [Display(Name = "علائم بالینی")]
        [MaxLength(1000, ErrorMessage = "علائم بالینی نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string ClinicalSymptoms { get; set; }

        /// <summary>
        /// سابقه پزشکی
        /// </summary>
        [Display(Name = "سابقه پزشکی")]
        [MaxLength(1000, ErrorMessage = "سابقه پزشکی نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string MedicalHistory { get; set; }

        /// <summary>
        /// داروهای مصرفی
        /// </summary>
        [Display(Name = "داروهای مصرفی")]
        [MaxLength(500, ErrorMessage = "داروهای مصرفی نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string CurrentMedications { get; set; }

        /// <summary>
        /// آلرژی‌ها
        /// </summary>
        [Display(Name = "آلرژی‌ها")]
        [MaxLength(500, ErrorMessage = "آلرژی‌ها نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Allergies { get; set; }

        /// <summary>
        /// نام همراه
        /// </summary>
        [Display(Name = "نام همراه")]
        [MaxLength(200, ErrorMessage = "نام همراه نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string CompanionName { get; set; }

        /// <summary>
        /// شماره تلفن همراه
        /// </summary>
        [Display(Name = "شماره تلفن همراه")]
        [MaxLength(20, ErrorMessage = "شماره تلفن همراه نمی‌تواند بیش از 20 کاراکتر باشد.")]
        public string CompanionPhone { get; set; }

        /// <summary>
        /// رابطه با بیمار
        /// </summary>
        [Display(Name = "رابطه با بیمار")]
        [MaxLength(100, ErrorMessage = "رابطه با بیمار نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string CompanionRelationship { get; set; }

        /// <summary>
        /// توضیحات اضافی
        /// </summary>
        [Display(Name = "توضیحات اضافی")]
        [MaxLength(1000, ErrorMessage = "توضیحات اضافی نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string AdditionalNotes { get; set; }

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ نوبت")]
        public string AppointmentDateDisplay => Date.ToString("yyyy/MM/dd");

        /// <summary>
        /// زمان نمایشی (فارسی)
        /// </summary>
        [Display(Name = "زمان نوبت")]
        public string AppointmentTimeDisplay => Time.ToString(@"hh\:mm");

        /// <summary>
        /// روز هفته (فارسی)
        /// </summary>
        [Display(Name = "روز هفته")]
        public string DayOfWeekDisplay
        {
            get
            {
                return Date.DayOfWeek switch
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
    /// نتیجه رزرو اورژانس
    /// </summary>
    public class EmergencyBookingResult
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
        /// رزرو اورژانس
        /// </summary>
        public EmergencyBooking EmergencyBooking { get; set; }

        /// <summary>
        /// تعارضات موجود
        /// </summary>
        public List<EmergencyConflict> Conflicts { get; set; } = new List<EmergencyConflict>();
    }

    /// <summary>
    /// رزرو اورژانس
    /// </summary>
    public class EmergencyBooking
    {
        /// <summary>
        /// شناسه رزرو اورژانس
        /// </summary>
        public int EmergencyId { get; set; }

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
        /// نام بیمار
        /// </summary>
        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        /// <summary>
        /// شماره تلفن بیمار
        /// </summary>
        [Display(Name = "شماره تلفن")]
        public string PatientPhone { get; set; }

        /// <summary>
        /// کد ملی بیمار
        /// </summary>
        [Display(Name = "کد ملی")]
        public string PatientNationalCode { get; set; }

      
        /// <summary>
        /// نوع اورژانس
        /// </summary>
        [Display(Name = "نوع اورژانس")]
        public EmergencyType Type { get; set; }

        /// <summary>
        /// اولویت اورژانس
        /// </summary>
        [Display(Name = "اولویت اورژانس")]
        public EmergencyPriority Priority { get; set; }

        /// <summary>
        /// دلیل اورژانس
        /// </summary>
        [Display(Name = "دلیل اورژانس")]
        public string EmergencyReason { get; set; }

        /// <summary>
        /// تاریخ
        /// </summary>
        [Display(Name = "تاریخ")]
        public DateTime Date { get; set; }

        /// <summary>
        /// زمان
        /// </summary>
        [Display(Name = "زمان")]
        public TimeSpan Time { get; set; }

        /// <summary>
        /// وضعیت رزرو
        /// </summary>
        [Display(Name = "وضعیت رزرو")]
        public EmergencyBookingStatus Status { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }



        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ نوبت")]
        public string AppointmentDateDisplay => Date.ToString("yyyy/MM/dd");

        /// <summary>
        /// زمان نمایشی (فارسی)
        /// </summary>
        [Display(Name = "زمان نوبت")]
        public string AppointmentTimeDisplay => Time.ToString(@"hh\:mm");

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

        /// <summary>
        /// کلاس CSS برای نمایش اولویت
        /// </summary>
        public string PriorityCssClass
        {
            get
            {
                return Priority switch
                {
                    EmergencyPriority.Low => "badge-success",
                    EmergencyPriority.Medium => "badge-warning",
                    EmergencyPriority.High => "badge-danger",
                    EmergencyPriority.Critical => "badge-danger",
                    _ => "badge-secondary"
                };
            }
        }

        /// <summary>
        /// متن فارسی اولویت
        /// </summary>
        public string PriorityDisplay
        {
            get
            {
                return Priority switch
                {
                    EmergencyPriority.Low => "کم",
                    EmergencyPriority.Medium => "متوسط",
                    EmergencyPriority.High => "زیاد",
                    EmergencyPriority.Critical => "بحرانی",
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
                return Status switch
                {
                    EmergencyBookingStatus.Pending => "badge-warning",
                    EmergencyBookingStatus.Confirmed => "badge-success",
                    EmergencyBookingStatus.Canceled => "badge-danger",
                    EmergencyBookingStatus.Completed => "badge-info",
                    _ => "badge-secondary"
                };
            }
        }

        /// <summary>
        /// متن فارسی وضعیت
        /// </summary>
        public string StatusDisplay
        {
            get
            {
                return Status switch
                {
                    EmergencyBookingStatus.Pending => "در انتظار تأیید",
                    EmergencyBookingStatus.Confirmed => "تأیید شده",
                    EmergencyBookingStatus.Canceled => "لغو شده",
                    EmergencyBookingStatus.Completed => "تکمیل شده",
                    _ => "نامشخص"
                };
            }
        }
    }

    /// <summary>
    /// تعارض اورژانس
    /// </summary>
    public class EmergencyConflict
    {
        /// <summary>
        /// شناسه تعارض
        /// </summary>
        public int ConflictId { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// تاریخ تعارض
        /// </summary>
        [Display(Name = "تاریخ تعارض")]
        public DateTime Date { get; set; }

        /// <summary>
        /// زمان شروع تعارض
        /// </summary>
        [Display(Name = "زمان شروع تعارض")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان تعارض
        /// </summary>
        [Display(Name = "زمان پایان تعارض")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// نوع تعارض
        /// </summary>
        [Display(Name = "نوع تعارض")]
        public string Type { get; set; }

        /// <summary>
        /// شدت تعارض
        /// </summary>
        [Display(Name = "شدت تعارض")]
        public EmergencyConflictSeverity Severity { get; set; }

        /// <summary>
        /// توضیحات تعارض
        /// </summary>
        [Display(Name = "توضیحات تعارض")]
        public string Description { get; set; }

        /// <summary>
        /// راه حل پیشنهادی
        /// </summary>
        [Display(Name = "راه حل پیشنهادی")]
        public string SuggestedSolution { get; set; }

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string DateDisplay => Date.ToString("yyyy/MM/dd");

        /// <summary>
        /// زمان نمایشی شروع (فارسی)
        /// </summary>
        [Display(Name = "زمان شروع")]
        public string StartTimeDisplay => StartTime.ToString(@"hh\:mm");

        /// <summary>
        /// زمان نمایشی پایان (فارسی)
        /// </summary>
        [Display(Name = "زمان پایان")]
        public string EndTimeDisplay => EndTime.ToString(@"hh\:mm");
    }



    /// <summary>
    /// آمار رزروهای اورژانس
    /// </summary>
    public class EmergencyBookingStatistics
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
        public DateTime FromDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public DateTime ToDate { get; set; }

        /// <summary>
        /// تعداد کل رزروهای اورژانس
        /// </summary>
        [Display(Name = "تعداد کل رزروهای اورژانس")]
        public int TotalEmergencyBookings { get; set; }

        /// <summary>
        /// تعداد رزروهای تأیید شده
        /// </summary>
        [Display(Name = "تعداد رزروهای تأیید شده")]
        public int ConfirmedBookings { get; set; }

        /// <summary>
        /// تعداد رزروهای لغو شده
        /// </summary>
        [Display(Name = "تعداد رزروهای لغو شده")]
        public int CanceledBookings { get; set; }

        /// <summary>
        /// تعداد رزروهای تکمیل شده
        /// </summary>
        [Display(Name = "تعداد رزروهای تکمیل شده")]
        public int CompletedBookings { get; set; }

        /// <summary>
        /// درصد رزروهای تأیید شده
        /// </summary>
        [Display(Name = "درصد رزروهای تأیید شده")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal ConfirmationRate => TotalEmergencyBookings > 0 ? (decimal)ConfirmedBookings / TotalEmergencyBookings : 0;

        /// <summary>
        /// درصد رزروهای لغو شده
        /// </summary>
        [Display(Name = "درصد رزروهای لغو شده")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal CancellationRate => TotalEmergencyBookings > 0 ? (decimal)CanceledBookings / TotalEmergencyBookings : 0;

        /// <summary>
        /// درصد رزروهای تکمیل شده
        /// </summary>
        [Display(Name = "درصد رزروهای تکمیل شده")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal CompletionRate => TotalEmergencyBookings > 0 ? (decimal)CompletedBookings / TotalEmergencyBookings : 0;

        /// <summary>
        /// تاریخ شروع نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        public string FromDateDisplay => FromDate.ToString("yyyy/MM/dd");

        /// <summary>
        /// تاریخ پایان نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public string ToDateDisplay => ToDate.ToString("yyyy/MM/dd");
    }





    /// <summary>
    /// گزارش اورژانس
    /// </summary>
    public class EmergencyReport
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
        /// تاریخ گزارش
        /// </summary>
        [Display(Name = "تاریخ گزارش")]
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// تعداد رزروهای اورژانس
        /// </summary>
        [Display(Name = "تعداد رزروهای اورژانس")]
        public int EmergencyBookingsCount { get; set; }

        /// <summary>
        /// تعداد رزروهای معمولی
        /// </summary>
        [Display(Name = "تعداد رزروهای معمولی")]
        public int RegularBookingsCount { get; set; }

        /// <summary>
        /// درصد رزروهای اورژانس
        /// </summary>
        [Display(Name = "درصد رزروهای اورژانس")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal EmergencyPercentage => (EmergencyBookingsCount + RegularBookingsCount) > 0 ? 
            (decimal)EmergencyBookingsCount / (EmergencyBookingsCount + RegularBookingsCount) : 0;

        /// <summary>
        /// متوسط زمان پاسخ‌دهی (دقیقه)
        /// </summary>
        [Display(Name = "متوسط زمان پاسخ‌دهی")]
        public int AverageResponseTime { get; set; }

        /// <summary>
        /// تعداد تعارضات
        /// </summary>
        [Display(Name = "تعداد تعارضات")]
        public int ConflictsCount { get; set; }

        /// <summary>
        /// تعداد تعارضات حل شده
        /// </summary>
        [Display(Name = "تعداد تعارضات حل شده")]
        public int ResolvedConflictsCount { get; set; }

        /// <summary>
        /// درصد تعارضات حل شده
        /// </summary>
        [Display(Name = "درصد تعارضات حل شده")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal ConflictResolutionRate => ConflictsCount > 0 ? 
            (decimal)ResolvedConflictsCount / ConflictsCount : 0;

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ گزارش")]
        public string ReportDateDisplay => ReportDate.ToString("yyyy/MM/dd");
    }
}
