using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// ViewModel برای ویرایش ارزیابی تریاژ
    /// </summary>
    public class TriageEditViewModel
    {
        [Required(ErrorMessage = "شناسه ارزیابی الزامی است")]
        public int AssessmentId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientFullName { get; set; }

        [Required(ErrorMessage = "شکایت اصلی الزامی است")]
        [StringLength(200, ErrorMessage = "شکایت اصلی نمی‌تواند بیش از 200 کاراکتر باشد")]
        [Display(Name = "شکایت اصلی")]
        public string ChiefComplaint { get; set; }

        [StringLength(500, ErrorMessage = "یادداشت‌های ارزیابی نمی‌تواند بیش از 500 کاراکتر باشد")]
        [Display(Name = "یادداشت‌های ارزیابی")]
        public string AssessmentNotes { get; set; }

        [Display(Name = "سطح تریاژ")]
        public TriageLevel Level { get; set; }

        [Display(Name = "اولویت")]
        public int Priority { get; set; }

        [Display(Name = "وضعیت")]
        public TriageStatus Status { get; set; }

        [Display(Name = "زمان ورود")]
        public DateTime ArrivalAt { get; set; }

        [Display(Name = "زمان شروع تریاژ")]
        public DateTime TriageStartAt { get; set; }

        [Display(Name = "زمان پایان تریاژ")]
        public DateTime? TriageEndAt { get; set; }

        [Display(Name = "باز است")]
        public bool IsOpen { get; set; }

        [Display(Name = "نیاز به ایزوله")]
        public bool IsolationRequired { get; set; }

        [Display(Name = "نوع ایزوله")]
        public IsolationType? Isolation { get; set; }

        [Display(Name = "پرچم قرمز - سپسیس")]
        public bool RedFlag_Sepsis { get; set; }

        [Display(Name = "پرچم قرمز - سکته")]
        public bool RedFlag_Stroke { get; set; }

        [Display(Name = "پرچم قرمز - سندرم حاد کرونری")]
        public bool RedFlag_ACS { get; set; }

        [Display(Name = "پرچم قرمز - تروما")]
        public bool RedFlag_Trauma { get; set; }

        [Display(Name = "باردار")]
        public bool IsPregnant { get; set; }

        [Display(Name = "بخش پیشنهادی")]
        public int? RecommendedDepartmentId { get; set; }

        [Display(Name = "پزشک پیشنهادی")]
        public int? RecommendedDoctorId { get; set; }

        // علائم حیاتی - برای ویرایش
        [Display(Name = "فشار خون سیستولیک")]
        [Range(40, 260, ErrorMessage = "فشار خون سیستولیک باید بین 40 تا 260 باشد")]
        public int? SBP { get; set; }

        [Display(Name = "فشار خون دیاستولیک")]
        [Range(20, 150, ErrorMessage = "فشار خون دیاستولیک باید بین 20 تا 150 باشد")]
        public int? DBP { get; set; }

        [Display(Name = "ضربان قلب")]
        [Range(30, 220, ErrorMessage = "ضربان قلب باید بین 30 تا 220 باشد")]
        public int? HR { get; set; }

        [Display(Name = "میزان تنفس")]
        [Range(5, 60, ErrorMessage = "میزان تنفس باید بین 5 تا 60 باشد")]
        public int? RR { get; set; }

        [Display(Name = "دمای بدن (°C)")]
        [Range(30, 45, ErrorMessage = "دمای بدن باید بین 30 تا 45 درجه سانتی‌گراد باشد")]
        public double? TempC { get; set; }

        [Display(Name = "اشباع اکسیژن (%)")]
        [Range(50, 100, ErrorMessage = "اشباع اکسیژن باید بین 50 تا 100 باشد")]
        public int? SpO2 { get; set; }

        [Display(Name = "GCS Eye")]
        [Range(1, 4, ErrorMessage = "GCS Eye باید بین 1 تا 4 باشد")]
        public int? GcsE { get; set; }

        [Display(Name = "GCS Verbal")]
        [Range(1, 5, ErrorMessage = "GCS Verbal باید بین 1 تا 5 باشد")]
        public int? GcsV { get; set; }

        [Display(Name = "GCS Motor")]
        [Range(1, 6, ErrorMessage = "GCS Motor باید بین 1 تا 6 باشد")]
        public int? GcsM { get; set; }

        [Display(Name = "تحت اکسیژن")]
        public bool OnOxygen { get; set; }

        [Display(Name = "نوع دستگاه اکسیژن")]
        public OxygenDevice? OxygenDevice { get; set; }

        [Display(Name = "جریان اکسیژن (L/min)")]
        [Range(0, 50, ErrorMessage = "جریان اکسیژن باید بین 0 تا 50 لیتر در دقیقه باشد")]
        public decimal? O2FlowLpm { get; set; }

        [Display(Name = "یادداشت‌های علائم حیاتی")]
        [StringLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string VitalSignsNotes { get; set; }

        // محاسبه شده
        public int? GcsTotal => (GcsE ?? 0) + (GcsV ?? 0) + (GcsM ?? 0);
        
        public bool RequiresImmediateAttention => 
            (SpO2 < 90) || 
            (HR > 120 || HR < 50) || 
            (SBP < 90) || 
            (TempC > 39 || TempC < 35) || 
            (RR > 30 || RR < 10) ||
            (GcsTotal < 8);

        public string LevelDisplayName => GetLevelDisplayName(Level);
        public string StatusDisplayName => GetStatusDisplayName(Status);
        public TimeSpan? TotalAssessmentTime => TriageEndAt.HasValue ? TriageEndAt.Value - TriageStartAt : null;
        public bool CanEdit => IsOpen && Status == TriageStatus.Pending;
        public bool CanComplete => IsOpen && Status == TriageStatus.Pending;
        
        private string GetLevelDisplayName(TriageLevel level)
        {
            return level switch
            {
                TriageLevel.ESI1 => "بحرانی (ESI-1)",
                TriageLevel.ESI2 => "فوری (ESI-2)",
                TriageLevel.ESI3 => "عاجل (ESI-3)",
                TriageLevel.ESI4 => "کم‌عاجل (ESI-4)",
                TriageLevel.ESI5 => "غیرعاجل (ESI-5)",
                _ => "نامشخص"
            };
        }
        
        private string GetStatusDisplayName(TriageStatus status)
        {
            return status switch
            {
                TriageStatus.Pending => "در انتظار",
                TriageStatus.InProgress => "در حال انجام",
                TriageStatus.Completed => "تکمیل شده",
                TriageStatus.Cancelled => "لغو شده",
                _ => "نامشخص"
            };
        }
    }
}
