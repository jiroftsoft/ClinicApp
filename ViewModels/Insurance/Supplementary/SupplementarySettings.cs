using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای تنظیمات بیمه تکمیلی
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت تنظیمات پیچیده بیمه تکمیلی
    /// 2. پشتیبانی از سناریوهای مختلف
    /// 3. رعایت استانداردهای پزشکی ایران
    /// 4. سازگاری با سیستم‌های موجود
    /// </summary>
    public class SupplementarySettings
    {
        #region Basic Information

        /// <summary>
        /// شناسه طرح بیمه
        /// </summary>
        [Display(Name = "شناسه طرح بیمه")]
        public int PlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه
        /// </summary>
        [Display(Name = "نام طرح بیمه")]
        [Required(ErrorMessage = "نام طرح بیمه الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام طرح بیمه نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string PlanName { get; set; }

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        #endregion

        #region Coverage Settings

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "درصد پوشش بیمه تکمیلی")]
        [Range(0, 100, ErrorMessage = "درصد پوشش باید بین 0 تا 100 باشد.")]
        public decimal CoveragePercent { get; set; }

        /// <summary>
        /// حداکثر مبلغ پرداخت
        /// </summary>
        [Display(Name = "حداکثر مبلغ پرداخت")]
        [Range(0, double.MaxValue, ErrorMessage = "حداکثر مبلغ پرداخت نمی‌تواند منفی باشد.")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal MaxPayment { get; set; }

        /// <summary>
        /// فرانشیز (مبلغ کسر شده)
        /// </summary>
        [Display(Name = "فرانشیز")]
        [Range(0, double.MaxValue, ErrorMessage = "فرانشیز نمی‌تواند منفی باشد.")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Deductible { get; set; }

        #endregion

        #region Advanced Settings

        /// <summary>
        /// تنظیمات JSON (برای تنظیمات پیچیده)
        /// </summary>
        [Display(Name = "تنظیمات JSON")]
        [MaxLength(2000, ErrorMessage = "تنظیمات JSON نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string SettingsJson { get; set; }

        /// <summary>
        /// آیا از تنظیمات پیشرفته استفاده می‌شود؟
        /// </summary>
        [Display(Name = "تنظیمات پیشرفته")]
        public bool UseAdvancedSettings { get; set; }

        /// <summary>
        /// حداقل مبلغ برای اعمال بیمه تکمیلی
        /// </summary>
        [Display(Name = "حداقل مبلغ")]
        [Range(0, double.MaxValue, ErrorMessage = "حداقل مبلغ نمی‌تواند منفی باشد.")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal MinimumAmount { get; set; }

        /// <summary>
        /// حداکثر تعداد استفاده در ماه
        /// </summary>
        [Display(Name = "حداکثر استفاده ماهانه")]
        [Range(0, int.MaxValue, ErrorMessage = "حداکثر استفاده ماهانه نمی‌تواند منفی باشد.")]
        public int MaxMonthlyUsage { get; set; }

        #endregion

        #region Validation Rules

        /// <summary>
        /// آیا اعتبارسنجی خودکار انجام شود؟
        /// </summary>
        [Display(Name = "اعتبارسنجی خودکار")]
        public bool AutoValidation { get; set; } = true;

        /// <summary>
        /// آیا نیاز به تأیید دستی است؟
        /// </summary>
        [Display(Name = "نیاز به تأیید دستی")]
        public bool RequiresManualApproval { get; set; }

        /// <summary>
        /// حداقل سن برای استفاده
        /// </summary>
        [Display(Name = "حداقل سن")]
        [Range(0, 120, ErrorMessage = "حداقل سن باید بین 0 تا 120 باشد.")]
        public int MinimumAge { get; set; }

        /// <summary>
        /// حداکثر سن برای استفاده
        /// </summary>
        [Display(Name = "حداکثر سن")]
        [Range(0, 120, ErrorMessage = "حداکثر سن باید بین 0 تا 120 باشد.")]
        public int MaximumAge { get; set; }

        #endregion

        #region Timestamps

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// تاریخ به‌روزرسانی
        /// </summary>
        [Display(Name = "تاریخ به‌روزرسانی")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر ایجادکننده
        /// </summary>
        [Display(Name = "ایجادکننده")]
        public int? CreatedByUserId { get; set; }

        /// <summary>
        /// شناسه کاربر به‌روزرسانی‌کننده
        /// </summary>
        [Display(Name = "به‌روزرسانی‌کننده")]
        public int? UpdatedByUserId { get; set; }

        #endregion

        #region Factory Methods

        /// <summary>
        /// ایجاد تنظیمات پیش‌فرض
        /// </summary>
        public static SupplementarySettings CreateDefault(int planId, string planName)
        {
            return new SupplementarySettings
            {
                PlanId = planId,
                PlanName = planName,
                IsActive = true,
                CoveragePercent = 80, // 80% پوشش پیش‌فرض
                MaxPayment = 1000000, // 1 میلیون تومان سقف پیش‌فرض
                Deductible = 0,
                UseAdvancedSettings = false,
                MinimumAmount = 100000, // 100 هزار تومان حداقل
                MaxMonthlyUsage = 10,
                AutoValidation = true,
                RequiresManualApproval = false,
                MinimumAge = 0,
                MaximumAge = 120,
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// ایجاد تنظیمات پیشرفته
        /// </summary>
        public static SupplementarySettings CreateAdvanced(int planId, string planName)
        {
            var settings = CreateDefault(planId, planName);
            settings.UseAdvancedSettings = true;
            settings.SettingsJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                SpecialRules = new[] { "Emergency", "Surgery", "Diagnostic" },
                Exclusions = new[] { "Cosmetic", "Experimental" },
                PriorAuthorization = true,
                NetworkRestrictions = true
            });
            return settings;
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// اعتبارسنجی تنظیمات
        /// </summary>
        public bool IsValid()
        {
            return PlanId > 0 &&
                   !string.IsNullOrWhiteSpace(PlanName) &&
                   CoveragePercent >= 0 && CoveragePercent <= 100 &&
                   MaxPayment >= 0 &&
                   Deductible >= 0 &&
                   MinimumAmount >= 0 &&
                   MaxMonthlyUsage >= 0 &&
                   MinimumAge >= 0 && MinimumAge <= 120 &&
                   MaximumAge >= 0 && MaximumAge <= 120 &&
                   MinimumAge <= MaximumAge;
        }

        /// <summary>
        /// بررسی سازگاری تنظیمات
        /// </summary>
        public bool IsConsistent()
        {
            return MaxPayment >= MinimumAmount &&
                   MaxMonthlyUsage > 0 &&
                   MinimumAge <= MaximumAge;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت تنظیمات JSON به صورت Dictionary
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> GetSettingsDictionary()
        {
            if (string.IsNullOrEmpty(SettingsJson))
                return new System.Collections.Generic.Dictionary<string, object>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(SettingsJson);
            }
            catch
            {
                return new System.Collections.Generic.Dictionary<string, object>();
            }
        }

        /// <summary>
        /// تنظیم تنظیمات JSON از Dictionary
        /// </summary>
        public void SetSettingsDictionary(System.Collections.Generic.Dictionary<string, object> settings)
        {
            SettingsJson = System.Text.Json.JsonSerializer.Serialize(settings);
        }

        /// <summary>
        /// نمایش خلاصه تنظیمات
        /// </summary>
        public string GetSummary()
        {
            return $"طرح: {PlanName} - پوشش: {CoveragePercent}% - سقف: {MaxPayment:N0} - فرانشیز: {Deductible:N0}";
        }

        #endregion
    }
}
