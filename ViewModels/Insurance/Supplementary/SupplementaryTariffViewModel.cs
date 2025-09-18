using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای تعرفه‌های بیمه تکمیلی
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش تعرفه‌های بیمه تکمیلی
    /// 2. پشتیبانی از تنظیمات پیچیده
    /// 3. رعایت استانداردهای پزشکی ایران
    /// 4. سازگاری با سیستم‌های موجود
    /// </summary>
    public class SupplementaryTariffViewModel
    {
        #region Basic Information

        /// <summary>
        /// شناسه تعرفه
        /// </summary>
        [Display(Name = "شناسه تعرفه")]
        public int TariffId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        [Display(Name = "نام خدمت")]
        [Required(ErrorMessage = "نام خدمت الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام خدمت نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string ServiceName { get; set; }

        /// <summary>
        /// شناسه طرح بیمه
        /// </summary>
        [Display(Name = "شناسه طرح بیمه")]
        public int PlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه
        /// </summary>
        [Display(Name = "نام طرح بیمه")]
        public string PlanName { get; set; }

        #endregion

        #region Coverage Settings

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "درصد پوشش بیمه تکمیلی")]
        [Range(0, 100, ErrorMessage = "درصد پوشش باید بین 0 تا 100 باشد.")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
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
        public string Settings { get; set; }

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

        #endregion

        #region Status Information

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// آیا حذف شده است؟
        /// </summary>
        [Display(Name = "حذف شده")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ به‌روزرسانی
        /// </summary>
        [Display(Name = "تاریخ به‌روزرسانی")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime? UpdatedAt { get; set; }

        #endregion

        #region Helper Properties

        /// <summary>
        /// آیا تنظیمات JSON دارد؟
        /// </summary>
        [Display(Name = "تنظیمات JSON")]
        public bool HasSettings => !string.IsNullOrEmpty(Settings);

        /// <summary>
        /// آیا حداکثر مبلغ تعریف شده است؟
        /// </summary>
        [Display(Name = "حداکثر مبلغ تعریف شده")]
        public bool HasMaxPayment => MaxPayment > 0;

        /// <summary>
        /// آیا فرانشیز تعریف شده است؟
        /// </summary>
        [Display(Name = "فرانشیز تعریف شده")]
        public bool HasDeductible => Deductible > 0;

        /// <summary>
        /// وضعیت تعرفه
        /// </summary>
        [Display(Name = "وضعیت")]
        public string Status
        {
            get
            {
                if (IsDeleted) return "حذف شده";
                if (!IsActive) return "غیرفعال";
                return "فعال";
            }
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// ایجاد تعرفه پیش‌فرض
        /// </summary>
        public static SupplementaryTariffViewModel CreateDefault(int serviceId, string serviceName, int planId, string planName)
        {
            return new SupplementaryTariffViewModel
            {
                ServiceId = serviceId,
                ServiceName = serviceName,
                PlanId = planId,
                PlanName = planName,
                CoveragePercent = 80, // 80% پوشش پیش‌فرض
                MaxPayment = 1000000, // 1 میلیون تومان سقف پیش‌فرض
                Deductible = 0,
                MinimumAmount = 100000, // 100 هزار تومان حداقل
                IsActive = true,
                UseAdvancedSettings = false,
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// ایجاد تعرفه پیشرفته
        /// </summary>
        public static SupplementaryTariffViewModel CreateAdvanced(int serviceId, string serviceName, int planId, string planName)
        {
            var tariff = CreateDefault(serviceId, serviceName, planId, planName);
            tariff.UseAdvancedSettings = true;
            tariff.Settings = System.Text.Json.JsonSerializer.Serialize(new
            {
                SpecialRules = new[] { "Emergency", "Surgery", "Diagnostic" },
                Exclusions = new[] { "Cosmetic", "Experimental" },
                PriorAuthorization = true,
                NetworkRestrictions = true,
                AgeRestrictions = new { Min = 0, Max = 120 },
                GenderRestrictions = new[] { "Male", "Female" }
            });
            return tariff;
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// اعتبارسنجی تعرفه
        /// </summary>
        public bool IsValid()
        {
            return ServiceId > 0 &&
                   !string.IsNullOrWhiteSpace(ServiceName) &&
                   PlanId > 0 &&
                   CoveragePercent >= 0 && CoveragePercent <= 100 &&
                   MaxPayment >= 0 &&
                   Deductible >= 0 &&
                   MinimumAmount >= 0;
        }

        /// <summary>
        /// بررسی سازگاری تعرفه
        /// </summary>
        public bool IsConsistent()
        {
            return MaxPayment >= MinimumAmount &&
                   CoveragePercent > 0;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت تنظیمات JSON به صورت Dictionary
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> GetSettingsDictionary()
        {
            if (string.IsNullOrEmpty(Settings))
                return new System.Collections.Generic.Dictionary<string, object>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(Settings);
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
            Settings = System.Text.Json.JsonSerializer.Serialize(settings);
        }

        /// <summary>
        /// نمایش خلاصه تعرفه
        /// </summary>
        public string GetSummary()
        {
            return $"خدمت: {ServiceName} - پوشش: {CoveragePercent}% - سقف: {MaxPayment:N0} - فرانشیز: {Deductible:N0}";
        }

        /// <summary>
        /// محاسبه مبلغ پوشش برای مبلغ مشخص
        /// </summary>
        public decimal CalculateCoverage(decimal serviceAmount)
        {
            var remainingAmount = serviceAmount - Deductible;
            if (remainingAmount <= 0) return 0;

            var coverage = remainingAmount * (CoveragePercent / 100);
            return MaxPayment > 0 ? Math.Min(coverage, MaxPayment) : coverage;
        }

        /// <summary>
        /// محاسبه سهم بیمار برای مبلغ مشخص
        /// </summary>
        public decimal CalculatePatientShare(decimal serviceAmount)
        {
            var coverage = CalculateCoverage(serviceAmount);
            return serviceAmount - coverage;
        }

        #endregion
    }
}
