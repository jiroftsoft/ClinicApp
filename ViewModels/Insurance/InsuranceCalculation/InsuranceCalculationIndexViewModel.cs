using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Extensions;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Insurance.InsuranceCalculation
{
    /// <summary>
    /// ViewModel برای نمایش یک محاسبه بیمه در لیست
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات کلیدی محاسبه بیمه
    /// 2. مناسب برای نمایش در جدول Index
    /// 3. رعایت قرارداد Factory Method Pattern
    /// 4. پشتیبانی از تاریخ شمسی
    /// 5. نمایش وضعیت اعتبار محاسبه
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsuranceCalculationIndexViewModel
    {
        [Display(Name = "شناسه محاسبه")]
        public int InsuranceCalculationId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }

        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

        [Display(Name = "نام طرح بیمه")]
        public string InsurancePlanName { get; set; }

        [Display(Name = "مبلغ خدمت")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ServiceAmount { get; set; }

        [Display(Name = "سهم بیمه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal InsuranceShare { get; set; }

        [Display(Name = "سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PatientShare { get; set; }

        [Display(Name = "درصد پوشش")]
        [DisplayFormat(DataFormatString = "{0:N1}%")]
        public decimal CoveragePercent { get; set; }

        [Display(Name = "فرانشیز")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Deductible { get; set; }

        [Display(Name = "تاریخ محاسبه")]
        public DateTime CalculationDate { get; set; }

        [Display(Name = "تاریخ محاسبه (شمسی)")]
        public string CalculationDateShamsi { get; set; }

        [Display(Name = "نوع محاسبه")]
        public InsuranceCalculationType CalculationType { get; set; }

        [Display(Name = "وضعیت اعتبار")]
        public bool IsValid { get; set; }

        [Display(Name = "وضعیت اعتبار")]
        public string IsValidText => IsValid ? "معتبر" : "نامعتبر";

        [Display(Name = "وضعیت اعتبار")]
        public string IsValidCssClass => IsValid ? "text-success" : "text-danger";

        [Display(Name = "یادداشت‌ها")]
        public string Notes { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsuranceCalculationIndexViewModel FromEntity(Models.Entities.Insurance.InsuranceCalculation entity)
        {
            if (entity == null) return null;

            return new InsuranceCalculationIndexViewModel
            {
                InsuranceCalculationId = entity.InsuranceCalculationId,
                PatientId = entity.PatientId,
                PatientName = entity.Patient?.FullName,
                PatientNationalCode = entity.Patient?.NationalCode,
                ServiceId = entity.ServiceId,
                ServiceName = entity.Service?.Title,
                InsurancePlanId = entity.InsurancePlanId,
                InsurancePlanName = entity.InsurancePlan?.Name,
                ServiceAmount = entity.ServiceAmount,
                InsuranceShare = entity.InsuranceShare,
                PatientShare = entity.PatientShare,
                CoveragePercent = entity.CoveragePercent,
                Deductible = entity.Deductible ?? 0,
                CalculationDate = entity.CalculationDate,
                CalculationDateShamsi = entity.CalculationDate.ToPersianDateTime(),
                CalculationType = entity.CalculationType,
                IsValid = entity.IsValid,
                Notes = entity.Notes,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                CreatedByUserName = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}".Trim() : null
            };
        }
    }
}
