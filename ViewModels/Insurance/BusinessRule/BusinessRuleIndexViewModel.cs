using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.ViewModels.Insurance.BusinessRule
{
    /// <summary>
    /// ViewModel برای صفحه اصلی مدیریت قوانین کسب‌وکار
    /// </summary>
    public class BusinessRuleIndexViewModel
    {
        /// <summary>
        /// لیست قوانین کسب‌وکار
        /// </summary>
        public List<BusinessRuleItemViewModel> BusinessRules { get; set; } = new List<BusinessRuleItemViewModel>();

        /// <summary>
        /// تعداد کل قوانین
        /// </summary>
        public int TotalCount => BusinessRules?.Count ?? 0;

        /// <summary>
        /// تعداد قوانین فعال
        /// </summary>
        public int ActiveCount => BusinessRules?.Where(r => r.IsActive).Count() ?? 0;

        /// <summary>
        /// تعداد قوانین غیرفعال
        /// </summary>
        public int InactiveCount => BusinessRules?.Where(r => !r.IsActive).Count() ?? 0;
    }

    /// <summary>
    /// ViewModel برای نمایش آیتم قانون در لیست
    /// </summary>
    public class BusinessRuleItemViewModel
    {
        /// <summary>
        /// شناسه قانون
        /// </summary>
        public int BusinessRuleId { get; set; }

        /// <summary>
        /// نام قانون
        /// </summary>
        [Display(Name = "نام قانون")]
        public string RuleName { get; set; }

        /// <summary>
        /// توضیحات قانون
        /// </summary>
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// <summary>
        /// نوع قانون
        /// </summary>
        [Display(Name = "نوع قانون")]
        public BusinessRuleType RuleType { get; set; }

        /// <summary>
        /// نام نوع قانون
        /// </summary>
        public string RuleTypeName => GetRuleTypeName(RuleType);

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        /// <summary>
        /// متن وضعیت
        /// </summary>
        public string StatusText => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// کلاس CSS برای وضعیت
        /// </summary>
        public string StatusClass => IsActive ? "badge-success" : "badge-danger";

        /// <summary>
        /// اولویت قانون
        /// </summary>
        [Display(Name = "اولویت")]
        public int Priority { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public System.DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        [Display(Name = "آخرین به‌روزرسانی")]
        public System.DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// دریافت نام نوع قانون
        /// </summary>
        private string GetRuleTypeName(BusinessRuleType ruleType)
        {
            return ruleType switch
            {
                BusinessRuleType.CoveragePercent => "درصد پوشش",
                BusinessRuleType.Deductible => "فرانشیز",
                BusinessRuleType.PaymentLimit => "سقف پرداخت",
                BusinessRuleType.AgeBasedDiscount => "تخفیف سنی",
                BusinessRuleType.GenderBasedDiscount => "تخفیف جنسیتی",
                BusinessRuleType.ServiceBasedDiscount => "تخفیف خدمتی",
                BusinessRuleType.InsuranceBasedDiscount => "تخفیف بیمه‌ای",
                BusinessRuleType.CustomRule => "قانون سفارشی",
                _ => "نامشخص"
            };
        }
    }
}
