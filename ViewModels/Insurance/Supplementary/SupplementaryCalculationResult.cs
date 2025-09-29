using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای نتیجه محاسبه بیمه تکمیلی
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش جزئیات محاسبه بیمه تکمیلی
    /// 2. پشتیبانی از سناریوهای مختلف
    /// 3. رعایت استانداردهای پزشکی ایران
    /// 4. سازگاری با سیستم‌های موجود
    /// </summary>
    public class SupplementaryCalculationResult
    {
        #region Basic Information

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        /// <summary>
        /// مبلغ خدمت
        /// </summary>
        [Display(Name = "مبلغ خدمت")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ServiceAmount { get; set; }

        /// <summary>
        /// تاریخ محاسبه
        /// </summary>
        [Display(Name = "تاریخ محاسبه")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CalculationDate { get; set; }

        #endregion

        #region Coverage Information

        /// <summary>
        /// پوشش بیمه اصلی
        /// </summary>
        [Display(Name = "پوشش بیمه اصلی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PrimaryCoverage { get; set; }

        /// <summary>
        /// پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "پوشش بیمه تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal SupplementaryCoverage { get; set; }

        /// <summary>
        /// کل پوشش (اصلی + تکمیلی)
        /// </summary>
        [Display(Name = "کل پوشش")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalCoverage { get; set; }

        /// <summary>
        /// سهم نهایی بیمار
        /// </summary>
        [Display(Name = "سهم نهایی بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal FinalPatientShare { get; set; }

        #endregion

        #region Additional Information

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "درصد پوشش بیمه تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal SupplementaryCoveragePercent { get; set; }

        /// <summary>
        /// درصد کل پوشش
        /// </summary>
        [Display(Name = "درصد کل پوشش")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal TotalCoveragePercent { get; set; }

        /// <summary>
        /// سهم بیمار از بیمه اصلی (قبل از اعمال بیمه تکمیلی)
        /// </summary>
        [Display(Name = "سهم بیمار از بیمه اصلی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PatientShareFromPrimary { get; set; }

        /// <summary>
        /// مبلغ باقی‌مانده پس از بیمه اصلی (برای سازگاری)
        /// </summary>
        [Display(Name = "مبلغ باقی‌مانده")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RemainingAmount 
        { 
            get => PatientShareFromPrimary; 
            set => PatientShareFromPrimary = value; 
        }

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        [Display(Name = "یادداشت‌ها")]
        [MaxLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Notes { get; set; }

        #endregion

        #region Helper Properties

        /// <summary>
        /// آیا بیمه تکمیلی اعمال شده است؟
        /// </summary>
        [Display(Name = "بیمه تکمیلی اعمال شده")]
        public bool HasSupplementaryCoverage => SupplementaryCoverage > 0;

        /// <summary>
        /// آیا بیمار سهمی دارد؟
        /// </summary>
        [Display(Name = "سهم بیمار")]
        public bool HasPatientShare => FinalPatientShare > 0;

        /// <summary>
        /// درصد سهم بیمار
        /// </summary>
        [Display(Name = "درصد سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal PatientSharePercent => ServiceAmount > 0 ? (FinalPatientShare / ServiceAmount) * 100 : 0;

        #endregion

        #region Factory Methods

        /// <summary>
        /// ایجاد نمونه خالی
        /// </summary>
        public static SupplementaryCalculationResult Empty()
        {
            return new SupplementaryCalculationResult
            {
                CalculationDate = DateTime.Now
            };
        }

        /// <summary>
        /// ایجاد نمونه با اطلاعات پایه
        /// </summary>
        public static SupplementaryCalculationResult Create(int patientId, int serviceId, decimal serviceAmount, decimal primaryCoverage)
        {
            return new SupplementaryCalculationResult
            {
                PatientId = patientId,
                ServiceId = serviceId,
                ServiceAmount = serviceAmount,
                PrimaryCoverage = primaryCoverage,
                PatientShareFromPrimary = serviceAmount - primaryCoverage,
                CalculationDate = DateTime.Now
            };
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// اعتبارسنجی نتیجه محاسبه
        /// </summary>
        public bool IsValid()
        {
            return PatientId > 0 && 
                   ServiceId > 0 && 
                   ServiceAmount >= 0 && 
                   PrimaryCoverage >= 0 && 
                   SupplementaryCoverage >= 0 && 
                   FinalPatientShare >= 0;
        }

        /// <summary>
        /// بررسی سازگاری محاسبات
        /// </summary>
        public bool IsCalculationConsistent()
        {
            return Math.Abs(ServiceAmount - (PrimaryCoverage + SupplementaryCoverage + FinalPatientShare)) < 0.01m;
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// نمایش خلاصه محاسبه
        /// </summary>
        public string GetSummary()
        {
            return $"مبلغ خدمت: {ServiceAmount:N0} - پوشش اصلی: {PrimaryCoverage:N0} - پوشش تکمیلی: {SupplementaryCoverage:N0} - سهم بیمار: {FinalPatientShare:N0}";
        }

        /// <summary>
        /// نمایش درصد پوشش
        /// </summary>
        public string GetCoveragePercentage()
        {
            var percentage = ServiceAmount > 0 ? (TotalCoverage / ServiceAmount) * 100 : 0;
            return $"{percentage:F2}%";
        }

        #endregion
    }
}
