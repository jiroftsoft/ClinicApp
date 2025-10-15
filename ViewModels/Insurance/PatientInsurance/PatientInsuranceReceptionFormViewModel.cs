using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.PatientInsurance
{
    /// <summary>
    /// ViewModel برای مدیریت بیمه بیمار در فرم پذیرش
    /// </summary>
    public class PatientInsuranceReceptionFormViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        [Required(ErrorMessage = "شناسه بیمار الزامی است")]
        public int PatientId { get; set; }

        /// <summary>
        /// شناسه بیمه پایه انتخاب شده
        /// </summary>
        public int? PrimaryInsuranceId { get; set; }

        /// <summary>
        /// نام بیمه‌گذار پایه
        /// </summary>
        public string PrimaryInsuranceProviderName { get; set; }

        /// <summary>
        /// نام طرح بیمه پایه
        /// </summary>
        public string PrimaryInsurancePlanName { get; set; }

        /// <summary>
        /// شماره بیمه‌نامه پایه
        /// </summary>
        public string PrimaryPolicyNumber { get; set; }

        /// <summary>
        /// شماره کارت بیمه پایه
        /// </summary>
        public string PrimaryCardNumber { get; set; }

        /// <summary>
        /// شناسه بیمه تکمیلی انتخاب شده
        /// </summary>
        public int? SupplementaryInsuranceId { get; set; }

        /// <summary>
        /// نام بیمه‌گذار تکمیلی
        /// </summary>
        public string SupplementaryInsuranceProviderName { get; set; }

        /// <summary>
        /// نام طرح بیمه تکمیلی
        /// </summary>
        public string SupplementaryInsurancePlanName { get; set; }

        /// <summary>
        /// شماره بیمه‌نامه تکمیلی
        /// </summary>
        public string SupplementaryPolicyNumber { get; set; }

        /// <summary>
        /// شماره کارت بیمه تکمیلی
        /// </summary>
        public string SupplementaryCardNumber { get; set; }

        /// <summary>
        /// تاریخ شروع بیمه پایه
        /// </summary>
        public DateTime? PrimaryStartDate { get; set; }

        /// <summary>
        /// تاریخ پایان بیمه پایه
        /// </summary>
        public DateTime? PrimaryEndDate { get; set; }

        /// <summary>
        /// تاریخ شروع بیمه تکمیلی
        /// </summary>
        public DateTime? SupplementaryStartDate { get; set; }

        /// <summary>
        /// تاریخ پایان بیمه تکمیلی
        /// </summary>
        public DateTime? SupplementaryEndDate { get; set; }

        /// <summary>
        /// درصد پوشش بیمه پایه
        /// </summary>
        public decimal? PrimaryCoveragePercent { get; set; }

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        public decimal? SupplementaryCoveragePercent { get; set; }

        /// <summary>
        /// فرانشیز بیمه پایه
        /// </summary>
        public decimal? PrimaryDeductible { get; set; }

        /// <summary>
        /// فرانشیز بیمه تکمیلی
        /// </summary>
        public decimal? SupplementaryDeductible { get; set; }

        /// <summary>
        /// آیا بیمه پایه فعال است؟
        /// </summary>
        public bool IsPrimaryActive { get; set; } = true;

        /// <summary>
        /// آیا بیمه تکمیلی فعال است؟
        /// </summary>
        public bool IsSupplementaryActive { get; set; } = false;

        /// <summary>
        /// یادداشت‌های بیمه
        /// </summary>
        [StringLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string Notes { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        public string CreatedByUserId { get; set; }
    }
}
