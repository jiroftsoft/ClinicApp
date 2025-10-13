using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// DTO برای علائم حیاتی تریاژ - Strong, Validation-Ready
    /// </summary>
    public class TriageVitalSignsDto
    {
        [Range(40, 260, ErrorMessage = "فشار خون سیستولیک باید بین 40 تا 260 باشد")]
        public int? SBP { get; set; }

        [Range(20, 150, ErrorMessage = "فشار خون دیاستولیک باید بین 20 تا 150 باشد")]
        public int? DBP { get; set; }

        [Range(30, 220, ErrorMessage = "ضربان قلب باید بین 30 تا 220 باشد")]
        public int? HR { get; set; }

        [Range(5, 60, ErrorMessage = "میزان تنفس باید بین 5 تا 60 باشد")]
        public int? RR { get; set; }

        [Range(30, 45, ErrorMessage = "دمای بدن باید بین 30 تا 45 درجه سانتی‌گراد باشد")]
        public double? TempC { get; set; }

        [Range(50, 100, ErrorMessage = "اشباع اکسیژن باید بین 50 تا 100 باشد")]
        public int? SpO2 { get; set; }

        [Range(20, 1000, ErrorMessage = "قند خون باید بین 20 تا 1000 باشد")]
        public int? Glucose { get; set; }

        [Range(1, 4, ErrorMessage = "GCS Eye باید بین 1 تا 4 باشد")]
        public int? GcsE { get; set; }

        [Range(1, 5, ErrorMessage = "GCS Verbal باید بین 1 تا 5 باشد")]
        public int? GcsV { get; set; }

        [Range(1, 6, ErrorMessage = "GCS Motor باید بین 1 تا 6 باشد")]
        public int? GcsM { get; set; }

        public bool OnOxygen { get; set; }
        public OxygenDevice? OxygenDevice { get; set; }

        [Range(0, 50, ErrorMessage = "جریان اکسیژن باید بین 0 تا 50 لیتر در دقیقه باشد")]
        public decimal? O2FlowLpm { get; set; }

        [StringLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string Notes { get; set; }

        // محاسبه شده
        public int? GcsTotal => (GcsE ?? 0) + (GcsV ?? 0) + (GcsM ?? 0);
        public bool RequiresImmediateAttention => 
            (SpO2 < 90) || 
            (HR > 120 || HR < 50) || 
            (SBP < 90) || 
            (TempC > 39 || TempC < 35) || 
            (RR > 30 || RR < 10) ||
            (GcsTotal < 8);
    }
}