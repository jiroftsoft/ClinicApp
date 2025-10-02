using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Models.Entities.Insurance
{
    /// <summary>
    /// قوانین کسب‌وکار بیمه
    /// </summary>
    public class BusinessRule : ISoftDelete, ITrackable
    {
        [Key]
        public int BusinessRuleId { get; set; }

        /// <summary>
        /// نام قانون
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string RuleName { get; set; }

        /// <summary>
        /// توضیحات قانون
        /// </summary>
        [MaxLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// نوع قانون
        /// </summary>
        [Required]
        public BusinessRuleType RuleType { get; set; }

        /// <summary>
        /// اولویت قانون (بالاتر = اولویت بیشتر)
        /// </summary>
        public int Priority { get; set; } = 1;

        /// <summary>
        /// شرایط اعمال قانون (JSON)
        /// </summary>
        public string Conditions { get; set; }

        /// <summary>
        /// عملیات قانون (JSON)
        /// </summary>
        public string Actions { get; set; }

        /// <summary>
        /// شناسه طرح بیمه (اختیاری - برای قوانین خاص)
        /// </summary>
        public int? InsurancePlanId { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی خدمت (اختیاری)
        /// </summary>
        public int? ServiceCategoryId { get; set; }

        // پیشنهاد افزوده برای پوشش‌دهی بهتر
        public int? ServiceId { get; set; }        // اختیاری: قانون برای یک خدمت خاص
        public bool? IsHashtagged { get; set; }    // اختیاری: قوانین برای خدمات هشتگ‌دار/غیره


        /// <summary>
        /// تاریخ شروع اعتبار
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive { get; set; } = true;

        #region Navigation Properties
        public virtual InsurancePlan InsurancePlan { get; set; }
        public virtual ServiceCategory ServiceCategory { get; set; }
        public virtual Service Service { get; set; } // اگر اضافه شد
        #endregion

        #region ISoftDelete Implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }
        public ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region ITrackable Implementation
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByUserId { get; set; }
        public ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public ApplicationUser UpdatedByUser { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
        #endregion
    }


    /// <summary>
    /// انواع قوانین کسب‌وکار
    /// </summary>
    public enum BusinessRuleType
    {
        /// <summary>
        /// قانون درصد پوشش
        /// </summary>
        CoveragePercent = 1,

        /// <summary>
        /// قانون فرانشیز
        /// </summary>
        Deductible = 2,

        /// <summary>
        /// قانون سقف پرداخت
        /// </summary>
        PaymentLimit = 3,

        /// <summary>
        /// قانون بیمه تکمیلی
        /// </summary>
        SupplementaryInsurance = 4,

        /// <summary>
        /// قانون اعتبارسنجی
        /// </summary>
        Validation = 5,

        /// <summary>
        /// قانون تخفیف
        /// </summary>
        Discount = 6,

        /// <summary>
        /// قانون جریمه
        /// </summary>
        Penalty = 7,

        /// <summary>
        /// قانون تخفیف سنی
        /// </summary>
        AgeBasedDiscount = 8,

        /// <summary>
        /// قانون تخفیف جنسیتی
        /// </summary>
        GenderBasedDiscount = 9,

        /// <summary>
        /// قانون تخفیف خدمتی
        /// </summary>
        ServiceBasedDiscount = 10,

        /// <summary>
        /// قانون تخفیف بیمه‌ای
        /// </summary>
        InsuranceBasedDiscount = 11,

        /// <summary>
        /// قانون سفارشی
        /// </summary>
        CustomRule = 12
    }
}
