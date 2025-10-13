using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// مدل نمایش پذیرش اورژانس
    /// </summary>
    public class EmergencyReceptionViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        [Required(ErrorMessage = "انتخاب بیمار الزامی است")]
        [Display(Name = "بیمار")]
        public int PatientId { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        /// <summary>
        /// نوع اورژانس
        /// </summary>
        [Required(ErrorMessage = "انتخاب نوع اورژانس الزامی است")]
        [Display(Name = "نوع اورژانس")]
        public string EmergencyType { get; set; }

        /// <summary>
        /// علائم انتخاب شده
        /// </summary>
        [Display(Name = "علائم")]
        public List<string> SelectedSymptoms { get; set; } = new List<string>();

        /// <summary>
        /// علائم اضافی
        /// </summary>
        [Display(Name = "علائم اضافی")]
        [StringLength(500, ErrorMessage = "علائم اضافی نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string AdditionalSymptoms { get; set; }

        /// <summary>
        /// یادداشت‌های اورژانس
        /// </summary>
        [Display(Name = "یادداشت‌های اورژانس")]
        [StringLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 1000 کاراکتر باشد")]
        public string EmergencyNotes { get; set; }

        /// <summary>
        /// اولویت
        /// </summary>
        [Display(Name = "اولویت")]
        public AppointmentPriority Priority { get; set; } = AppointmentPriority.Critical;

        /// <summary>
        /// آیا فوری است
        /// </summary>
        [Display(Name = "فوری")]
        public bool IsUrgent { get; set; } = true;

        /// <summary>
        /// زمان ایجاد
        /// </summary>
        [Display(Name = "زمان ایجاد")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// انواع اورژانس
        /// </summary>
        public List<string> EmergencyTypes { get; set; } = new List<string>();

        /// <summary>
        /// علائم رایج
        /// </summary>
        public List<string> Symptoms { get; set; } = new List<string>();

        /// <summary>
        /// اولویت به صورت متن
        /// </summary>
        public string PriorityText => Priority switch
        {
            AppointmentPriority.Critical => "بحرانی",
            AppointmentPriority.High => "بالا",
            AppointmentPriority.Medium => "متوسط",
            AppointmentPriority.Low => "پایین",
            _ => "نامشخص"
        };

        /// <summary>
        /// کلاس CSS برای نمایش اولویت
        /// </summary>
        public string PriorityCssClass => Priority switch
        {
            AppointmentPriority.Critical => "danger",
            AppointmentPriority.High => "warning",
            AppointmentPriority.Medium => "info",
            AppointmentPriority.Low => "success",
            _ => "secondary"
        };
    }

    /// <summary>
    /// مدل نمایش نتیجه پذیرش اورژانس
    /// </summary>
    public class EmergencyReceptionResultViewModel
    {
        /// <summary>
        /// شناسه پذیرش
        /// </summary>
        public int ReceptionId { get; set; }

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// نوع اورژانس
        /// </summary>
        public string EmergencyType { get; set; }

        /// <summary>
        /// سطح تریاژ
        /// </summary>
        public string TriageLevel { get; set; }

        /// <summary>
        /// اولویت
        /// </summary>
        public AppointmentPriority Priority { get; set; }

        /// <summary>
        /// زمان ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// وضعیت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// پیام نتیجه
        /// </summary>
        public string ResultMessage { get; set; }

        /// <summary>
        /// آیا موفق بوده
        /// </summary>
        public bool IsSuccess { get; set; }
    }
}
