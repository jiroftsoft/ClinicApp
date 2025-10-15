using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش اطلاعات بیمه در فرم پذیرش
    /// </summary>
    public class ReceptionInsuranceViewModel
    {
        /// <summary>
        /// شناسه بیمه بیمار
        /// </summary>
        public int PatientInsuranceId { get; set; }

        /// <summary>
        /// شناسه بیمه‌گذار
        /// </summary>
        public int InsuranceProviderId { get; set; }

        /// <summary>
        /// نام بیمه‌گذار
        /// </summary>
        public string InsuranceProviderName { get; set; }

        /// <summary>
        /// شناسه طرح بیمه
        /// </summary>
        public int InsurancePlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه
        /// </summary>
        public string InsurancePlanName { get; set; }

        /// <summary>
        /// شماره بیمه‌نامه
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// شماره کارت بیمه
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// تاریخ شروع بیمه
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان بیمه
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// آیا بیمه اصلی است؟
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// آیا بیمه فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// درصد پوشش بیمه
        /// </summary>
        public decimal? CoveragePercent { get; set; }

        /// <summary>
        /// فرانشیز بیمه
        /// </summary>
        public decimal? Deductible { get; set; }

        /// <summary>
        /// نوع بیمه (اصلی/تکمیلی)
        /// </summary>
        public string InsuranceType => IsPrimary ? "اصلی" : "تکمیلی";

        /// <summary>
        /// وضعیت بیمه
        /// </summary>
        public string Status => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// نمایش اطلاعات بیمه (فرمات شده)
        /// </summary>
        public string DisplayName => $"{InsuranceProviderName} - {InsurancePlanName}";

        /// <summary>
        /// نمایش درصد پوشش (فرمات شده)
        /// </summary>
        public string CoveragePercentDisplay => CoveragePercent.HasValue ? $"{CoveragePercent.Value}%" : "نامشخص";

        /// <summary>
        /// نمایش فرانشیز (فرمات شده)
        /// </summary>
        public string DeductibleDisplay => Deductible.HasValue ? $"{Deductible.Value:N0} ریال" : "نامشخص";

        /// <summary>
        /// آیا بیمه منقضی شده؟
        /// </summary>
        public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.Now;

        /// <summary>
        /// آیا بیمه معتبر است؟
        /// </summary>
        public bool IsValid => IsActive && !IsExpired;

        /// <summary>
        /// نمایش وضعیت اعتبار
        /// </summary>
        public string ValidityStatus
        {
            get
            {
                if (!IsActive) return "غیرفعال";
                if (IsExpired) return "منقضی شده";
                return "معتبر";
            }
        }
    }
}
