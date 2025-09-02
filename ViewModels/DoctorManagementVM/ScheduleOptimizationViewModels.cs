using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// نتیجه توزیع بار کاری
    /// </summary>
    public class WorkloadBalanceResult
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
        /// تعداد نوبت‌های فعلی
        /// </summary>
        [Display(Name = "تعداد نوبت‌های فعلی")]
        public int CurrentAppointments { get; set; }

        /// <summary>
        /// تعداد نوبت‌های پیشنهادی
        /// </summary>
        [Display(Name = "تعداد نوبت‌های پیشنهادی")]
        public int SuggestedAppointments { get; set; }

        /// <summary>
        /// درصد بار کاری
        /// </summary>
        [Display(Name = "درصد بار کاری")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal WorkloadPercentage { get; set; }

        /// <summary>
        /// وضعیت تعادل
        /// </summary>
        [Display(Name = "وضعیت تعادل")]
        public WorkloadBalanceStatus Status { get; set; }

        /// <summary>
        /// پیام وضعیت
        /// </summary>
        [Display(Name = "پیام وضعیت")]
        public string Message { get; set; }

        /// <summary>
        /// تعداد کل نوبت‌ها
        /// </summary>
        [Display(Name = "تعداد کل نوبت‌ها")]
        public int TotalAppointments { get; set; }

        /// <summary>
        /// کل زمان کار (دقیقه)
        /// </summary>
        [Display(Name = "کل زمان کار (دقیقه)")]
        public int TotalWorkMinutes { get; set; }

        /// <summary>
        /// زمان استراحت (دقیقه)
        /// </summary>
        [Display(Name = "زمان استراحت (دقیقه)")]
        public int BreakTimeMinutes { get; set; }

        /// <summary>
        /// اسلات‌های بهینه شده
        /// </summary>
        [Display(Name = "اسلات‌های بهینه شده")]
        public List<TimeSlotViewModel> OptimizedSlots { get; set; } = new List<TimeSlotViewModel>();

        /// <summary>
        /// پیشنهادات بهبود
        /// </summary>
        [Display(Name = "پیشنهادات بهبود")]
        public List<string> Recommendations { get; set; } = new List<string>();

      
    }



    /// <summary>
    /// زمان استراحت
    /// </summary>
    public class BreakTimeSlot
    {
        /// <summary>
        /// شناسه زمان استراحت
        /// </summary>
        public int BreakId { get; set; }

        /// <summary>
        /// تاریخ
        /// </summary>
        [Display(Name = "تاریخ")]
        public DateTime Date { get; set; }

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
        /// نوع استراحت
        /// </summary>
        [Display(Name = "نوع استراحت")]
        public BreakType Type { get; set; }

        /// <summary>
        /// اولویت
        /// </summary>
        [Display(Name = "اولویت")]
        public int Priority { get; set; }

        /// <summary>
        /// آیا اجباری است؟
        /// </summary>
        [Display(Name = "اجباری")]
        public bool IsMandatory { get; set; }

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
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string DateDisplay => Date.ToString("yyyy/MM/dd");

        /// <summary>
        /// آیا بهینه شده است؟
        /// </summary>
        [Display(Name = "بهینه شده")]
        public bool IsOptimized { get; set; }

      
    }



    /// <summary>
    /// نتیجه توزیع بیماران
    /// </summary>
    public class PatientDistributionResult
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        /// <summary>
        /// زمان نوبت پیشنهادی
        /// </summary>
        [Display(Name = "زمان نوبت پیشنهادی")]
        public DateTime SuggestedAppointmentTime { get; set; }

        /// <summary>
        /// اولویت
        /// </summary>
        [Display(Name = "اولویت")]
        public int Priority { get; set; }

        /// <summary>
        /// نوع نوبت
        /// </summary>
        [Display(Name = "نوع نوبت")]
        public string Type { get; set; }

        /// <summary>
        /// دلیل اولویت
        /// </summary>
        [Display(Name = "دلیل اولویت")]
        public string PriorityReason { get; set; }

        /// <summary>
        /// زمان نمایشی پیشنهادی (فارسی)
        /// </summary>
        [Display(Name = "زمان نوبت پیشنهادی")]
        public string SuggestedTimeDisplay => SuggestedAppointmentTime.ToString("yyyy/MM/dd HH:mm");

        /// <summary>
        /// تعداد کل بیماران
        /// </summary>
        [Display(Name = "تعداد کل بیماران")]
        public int TotalPatients { get; set; }

        /// <summary>
        /// توزیع بر اساس نوع
        /// </summary>
        [Display(Name = "توزیع بر اساس نوع")]
        public Dictionary<string, int> DistributionByType { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// پیشنهادات بهبود
        /// </summary>
        [Display(Name = "پیشنهادات بهبود")]
        public List<string> Recommendations { get; set; } = new List<string>();

        
    }



    /// <summary>
    /// زمان اورژانس
    /// </summary>
    public class EmergencyTimeSlot
    {
        /// <summary>
        /// شناسه زمان اورژانس
        /// </summary>
        public int EmergencyTimeId { get; set; }

        /// <summary>
        /// تاریخ
        /// </summary>
        [Display(Name = "تاریخ")]
        public DateTime Date { get; set; }

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
        /// اولویت اورژانس
        /// </summary>
        [Display(Name = "اولویت اورژانس")]
        public EmergencyPriority Priority { get; set; }

        /// <summary>
        /// نوع اورژانس
        /// </summary>
        [Display(Name = "نوع اورژانس")]
        public EmergencyType Type { get; set; }

        /// <summary>
        /// آیا در دسترس است؟
        /// </summary>
        [Display(Name = "در دسترس")]
        public bool IsAvailable { get; set; }

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
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ")]
        public string DateDisplay => Date.ToString("yyyy/MM/dd");
    }



    /// <summary>
    /// گزارش تعادل کار-زندگی
    /// </summary>
    public class WorkLifeBalanceReport
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
        /// ماه گزارش
        /// </summary>
        [Display(Name = "ماه گزارش")]
        public DateTime ReportMonth { get; set; }

        /// <summary>
        /// تعداد روزهای کاری
        /// </summary>
        [Display(Name = "تعداد روزهای کاری")]
        public int WorkingDays { get; set; }

        /// <summary>
        /// تعداد روزهای استراحت
        /// </summary>
        [Display(Name = "تعداد روزهای استراحت")]
        public int RestDays { get; set; }

        /// <summary>
        /// تعداد ساعات کاری
        /// </summary>
        [Display(Name = "تعداد ساعات کاری")]
        public int WorkingHours { get; set; }

        /// <summary>
        /// تعداد ساعات استراحت
        /// </summary>
        [Display(Name = "تعداد ساعات استراحت")]
        public int RestHours { get; set; }

        /// <summary>
        /// درصد تعادل
        /// </summary>
        [Display(Name = "درصد تعادل")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal BalancePercentage { get; set; }

        /// <summary>
        /// وضعیت تعادل
        /// </summary>
        [Display(Name = "وضعیت تعادل")]
        public WorkLifeBalanceStatus Status { get; set; }

        /// <summary>
        /// پیشنهادات بهبود
        /// </summary>
        [Display(Name = "پیشنهادات بهبود")]
        public List<string> ImprovementSuggestions { get; set; } = new List<string>();

        /// <summary>
        /// تعداد کل ساعات کاری
        /// </summary>
        [Display(Name = "تعداد کل ساعات کاری")]
        public int TotalWorkHours { get; set; }

        /// <summary>
        /// تعداد کل ساعات استراحت
        /// </summary>
        [Display(Name = "تعداد کل ساعات استراحت")]
        public int TotalBreakHours { get; set; }

        /// <summary>
        /// پیشنهادات بهبود
        /// </summary>
        [Display(Name = "پیشنهادات بهبود")]
        public List<string> Recommendations { get; set; } = new List<string>();

        /// <summary>
        /// تاریخ گزارش
        /// </summary>
        [Display(Name = "تاریخ گزارش")]
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ گزارش")]
        public string ReportDateDisplay => ReportDate.ToString("yyyy/MM/dd");

        /// <summary>
        /// ماه نمایشی (فارسی)
        /// </summary>
        [Display(Name = "ماه گزارش")]
        public string ReportMonthDisplay => ReportMonth.ToString("yyyy/MM");
    }



    /// <summary>
    /// گزارش بهینه‌سازی هزینه
    /// </summary>
    public class CostOptimizationReport
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
        /// هزینه‌های فعلی (ریال)
        /// </summary>
        [Display(Name = "هزینه‌های فعلی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal CurrentCosts { get; set; }

        /// <summary>
        /// هزینه‌های بهینه شده (ریال)
        /// </summary>
        [Display(Name = "هزینه‌های بهینه شده")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal OptimizedCosts { get; set; }

        /// <summary>
        /// صرفه‌جویی (ریال)
        /// </summary>
        [Display(Name = "صرفه‌جویی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Savings { get; set; }

        /// <summary>
        /// درصد صرفه‌جویی
        /// </summary>
        [Display(Name = "درصد صرفه‌جویی")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public decimal SavingsPercentage { get; set; }

        /// <summary>
        /// پیشنهادات بهینه‌سازی
        /// </summary>
        [Display(Name = "پیشنهادات بهینه‌سازی")]
        public List<CostOptimizationSuggestion> Suggestions { get; set; } = new List<CostOptimizationSuggestion>();

        /// <summary>
        /// درآمد کل (ریال)
        /// </summary>
        [Display(Name = "درآمد کل")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// هزینه‌های کل (ریال)
        /// </summary>
        [Display(Name = "هزینه‌های کل")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalCosts { get; set; }

        /// <summary>
        /// سود خالص (ریال)
        /// </summary>
        [Display(Name = "سود خالص")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal NetProfit { get; set; }

        /// <summary>
        /// تاریخ نمایشی (فارسی)
        /// </summary>
        [Display(Name = "تاریخ گزارش")]
        public string ReportDateDisplay => ReportDate.ToString("yyyy/MM/dd");
    }



    /// <summary>
    /// پیشنهاد بهینه‌سازی هزینه
    /// </summary>
    public class CostOptimizationSuggestion
    {
        /// <summary>
        /// شناسه پیشنهاد
        /// </summary>
        public int SuggestionId { get; set; }

        /// <summary>
        /// عنوان پیشنهاد
        /// </summary>
        [Display(Name = "عنوان پیشنهاد")]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// <summary>
        /// هزینه صرفه‌جویی شده (ریال)
        /// </summary>
        [Display(Name = "صرفه‌جویی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal CostSavings { get; set; }

        /// <summary>
        /// اولویت اجرا
        /// </summary>
        [Display(Name = "اولویت اجرا")]
        public int ImplementationPriority { get; set; }

        /// <summary>
        /// دشواری اجرا
        /// </summary>
        [Display(Name = "دشواری اجرا")]
        public string Difficulty { get; set; }

        /// <summary>
        /// زمان تخمینی اجرا (روز)
        /// </summary>
        [Display(Name = "زمان تخمینی اجرا")]
        public int EstimatedImplementationDays { get; set; }
    }


}
