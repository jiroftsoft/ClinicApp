using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// DTO برای ایجاد پایش مجدد تریاژ
    /// </summary>
    public class TriageReassessmentDto
    {
        [Required(ErrorMessage = "سطح جدید تریاژ الزامی است")]
        public TriageLevel NewLevel { get; set; }

        [Required(ErrorMessage = "دلیل پایش مجدد الزامی است")]
        [StringLength(500, ErrorMessage = "دلیل پایش مجدد نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string Reason { get; set; }

        [StringLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 1000 کاراکتر باشد")]
        public string Notes { get; set; }

        [Display(Name = "شناسه ارزیابی تریاژ")]
        public int TriageAssessmentId { get; set; }

        [Display(Name = "زمان ارزیابی مجدد")]
        public DateTime AtUtc { get; set; } = DateTime.UtcNow;

        [Display(Name = "تغییرات")]
        public string Changes { get; set; }

        [Display(Name = "اقدامات")]
        public string Actions { get; set; }

        [Display(Name = "علائم حیاتی")]
        public string Vitals { get; set; }
    }
}
