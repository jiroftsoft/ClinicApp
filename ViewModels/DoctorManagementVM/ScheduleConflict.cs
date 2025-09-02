using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// ViewModel برای نمایش تعارضات برنامه‌ریزی
    /// </summary>
    public class ScheduleConflict
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
        /// نام پزشک
        /// </summary>
        [Display(Name = "پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تاریخ تعارض
        /// </summary>
        [Display(Name = "تاریخ")]
        public DateTime ConflictDate { get; set; }

        /// <summary>
        /// زمان شروع تعارض
        /// </summary>
        [Display(Name = "زمان شروع")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان تعارض
        /// </summary>
        [Display(Name = "زمان پایان")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// نوع تعارض
        /// </summary>
        [Display(Name = "نوع تعارض")]
        public ConflictType Type { get; set; }

        /// <summary>
        /// شدت تعارض
        /// </summary>
        [Display(Name = "شدت تعارض")]
        public ConflictSeverity Severity { get; set; }

        /// <summary>
        /// توضیحات تعارض
        /// </summary>
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// <summary>
        /// راه‌حل پیشنهادی
        /// </summary>
        [Display(Name = "راه‌حل پیشنهادی")]
        public string SuggestedSolution { get; set; }

        /// <summary>
        /// آیا تعارض حل شده است؟
        /// </summary>
        [Display(Name = "حل شده")]
        public bool IsResolved { get; set; }

        /// <summary>
        /// تاریخ حل تعارض
        /// </summary>
        [Display(Name = "تاریخ حل")]
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// کاربر حل کننده
        /// </summary>
        [Display(Name = "حل کننده")]
        public string ResolvedBy { get; set; }

        /// <summary>
        /// تاریخ ایجاد تعارض
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string DateDisplay => ConflictDate.ToString("yyyy/MM/dd");

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

        /// <summary>
        /// روز هفته (فارسی)
        /// </summary>
        [Display(Name = "روز هفته")]
        public string DayOfWeekDisplay
        {
            get
            {
                return ConflictDate.DayOfWeek switch
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
        /// کلاس CSS برای نمایش شدت تعارض
        /// </summary>
        public string SeverityCssClass
        {
            get
            {
                return Severity switch
                {
                    ConflictSeverity.Low => "badge-success",
                    ConflictSeverity.Medium => "badge-warning",
                    ConflictSeverity.High => "badge-danger",
                    ConflictSeverity.Critical => "badge-danger",
                    _ => "badge-secondary"
                };
            }
        }

        /// <summary>
        /// متن فارسی شدت تعارض
        /// </summary>
        public string SeverityDisplay
        {
            get
            {
                return Severity switch
                {
                    ConflictSeverity.Low => "کم",
                    ConflictSeverity.Medium => "متوسط",
                    ConflictSeverity.High => "زیاد",
                    ConflictSeverity.Critical => "بحرانی",
                    _ => "نامشخص"
                };
            }
        }

        /// <summary>
        /// کلاس CSS برای نمایش نوع تعارض
        /// </summary>
        public string TypeCssClass
        {
            get
            {
                return Type switch
                {
                    ConflictType.Overlap => "badge-warning",
                    ConflictType.DoubleBooking => "badge-danger",
                    ConflictType.InvalidTime => "badge-info",
                    ConflictType.ExceptionConflict => "badge-secondary",
                    ConflictType.TemplateConflict => "badge-primary",
                    _ => "badge-secondary"
                };
            }
        }

        /// <summary>
        /// متن فارسی نوع تعارض
        /// </summary>
        public string TypeDisplay
        {
            get
            {
                return Type switch
                {
                    ConflictType.Overlap => "تداخل زمانی",
                    ConflictType.DoubleBooking => "رزرو مضاعف",
                    ConflictType.InvalidTime => "زمان نامعتبر",
                    ConflictType.ExceptionConflict => "تعارض با استثنا",
                    ConflictType.TemplateConflict => "تعارض با قالب",
                    _ => "نامشخص"
                };
            }
        }
    }

    /// <summary>
    /// انواع مختلف تعارضات برنامه‌ریزی
    /// </summary>
    public enum ConflictType : byte
    {
        [Display(Name = "تداخل زمانی")]
        Overlap = 0,
        [Display(Name = "رزرو مضاعف")]
        DoubleBooking = 1,
        [Display(Name = "زمان نامعتبر")]
        InvalidTime = 2,
        [Display(Name = "تعارض با استثنا")]
        ExceptionConflict = 3,
        [Display(Name = "تعارض با قالب")]
        TemplateConflict = 4
    }

    /// <summary>
    /// سطوح مختلف شدت تعارض
    /// </summary>
    public enum ConflictSeverity : byte
    {
        [Display(Name = "کم")]
        Low = 0,
        [Display(Name = "متوسط")]
        Medium = 1,
        [Display(Name = "زیاد")]
        High = 2,
        [Display(Name = "بحرانی")]
        Critical = 3
    }
}
