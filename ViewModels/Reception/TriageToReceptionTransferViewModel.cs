using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// مدل نمایش انتقال بیماران از تریاژ به پذیرش
    /// </summary>
    public class TriageToReceptionTransferViewModel
    {
        /// <summary>
        /// لیست بیماران آماده برای انتقال
        /// </summary>
        public List<TriageToReceptionCandidate> ReadyPatients { get; set; } = new List<TriageToReceptionCandidate>();

        /// <summary>
        /// تعداد کل ارزیابی‌ها
        /// </summary>
        public int TotalAssessments { get; set; }

        /// <summary>
        /// تعداد بیماران آماده
        /// </summary>
        public int ReadyPatientsCount => ReadyPatients?.Count ?? 0;

        /// <summary>
        /// تعداد بیماران اورژانس
        /// </summary>
        public int EmergencyPatientsCount => ReadyPatients?.Where(p => p.IsEmergency).Count() ?? 0;

        /// <summary>
        /// تعداد بیماران عادی
        /// </summary>
        public int NormalPatientsCount => ReadyPatients?.Where(p => !p.IsEmergency).Count() ?? 0;
    }

    /// <summary>
    /// کاندیدای انتقال از تریاژ به پذیرش
    /// </summary>
    public class TriageToReceptionCandidate
    {
        /// <summary>
        /// شناسه ارزیابی تریاژ
        /// </summary>
        public int TriageAssessmentId { get; set; }

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// نام کامل بیمار
        /// </summary>
        public string PatientFullName { get; set; }

        /// <summary>
        /// سطح تریاژ
        /// </summary>
        public TriageLevel TriageLevel { get; set; }

        /// <summary>
        /// اولویت
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// شکایت اصلی
        /// </summary>
        public string ChiefComplaint { get; set; }

        /// <summary>
        /// زمان تریاژ
        /// </summary>
        public DateTime TriageDateTime { get; set; }

        /// <summary>
        /// زمان انتظار تخمینی (دقیقه)
        /// </summary>
        public int? EstimatedWaitTime { get; set; }

        /// <summary>
        /// آیا اورژانس است
        /// </summary>
        public bool IsEmergency { get; set; }

        /// <summary>
        /// بخش پیشنهادی
        /// </summary>
        public string RecommendedDepartment { get; set; }

        /// <summary>
        /// پزشک پیشنهادی
        /// </summary>
        public string RecommendedDoctor { get; set; }

        /// <summary>
        /// سطح تریاژ به صورت متن
        /// </summary>
        public string TriageLevelText => TriageLevel switch
        {
            TriageLevel.ESI1 => "اورژانس فوری",
            TriageLevel.ESI2 => "اورژانس",
            TriageLevel.ESI3 => "فوری",
            TriageLevel.ESI4 => "عادی",
            TriageLevel.ESI5 => "غیر فوری",
            _ => "نامشخص"
        };

        /// <summary>
        /// کلاس CSS برای نمایش سطح تریاژ
        /// </summary>
        public string TriageLevelCssClass => TriageLevel switch
        {
            TriageLevel.ESI1 => "danger",
            TriageLevel.ESI2 => "warning",
            TriageLevel.ESI3 => "info",
            TriageLevel.ESI4 => "success",
            TriageLevel.ESI5 => "secondary",
            _ => "secondary"
        };

        /// <summary>
        /// زمان انتظار به صورت متن
        /// </summary>
        public string EstimatedWaitTimeText => EstimatedWaitTime.HasValue 
            ? $"{EstimatedWaitTime.Value} دقیقه" 
            : "نامشخص";
    }

    /// <summary>
    /// داده‌های انتقال از تریاژ به پذیرش
    /// </summary>
    public class TriageToReceptionTransferData
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "انتخاب پزشک الزامی است")]
        [Display(Name = "پزشک")]
        public int DoctorId { get; set; }

        /// <summary>
        /// شناسه بخش
        /// </summary>
        [Required(ErrorMessage = "انتخاب بخش الزامی است")]
        [Display(Name = "بخش")]
        public int DepartmentId { get; set; }

        /// <summary>
        /// مبلغ کل
        /// </summary>
        [Required(ErrorMessage = "مبلغ کل الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ کل باید مثبت باشد")]
        [Display(Name = "مبلغ کل")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// سهم بیمار
        /// </summary>
        [Required(ErrorMessage = "سهم بیمار الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "سهم بیمار باید مثبت باشد")]
        [Display(Name = "سهم بیمار")]
        public decimal PatientCoPay { get; set; }

        /// <summary>
        /// سهم بیمه
        /// </summary>
        [Required(ErrorMessage = "سهم بیمه الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "سهم بیمه باید مثبت باشد")]
        [Display(Name = "سهم بیمه")]
        public decimal InsurerShareAmount { get; set; }

        /// <summary>
        /// شناسه‌های خدمات
        /// </summary>
        [Display(Name = "خدمات")]
        public List<int> ServiceIds { get; set; } = new List<int>();

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        [Display(Name = "یادداشت‌ها")]
        [StringLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string Notes { get; set; }
    }
}
