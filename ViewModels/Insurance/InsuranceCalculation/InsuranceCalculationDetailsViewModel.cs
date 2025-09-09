using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.Insurance.InsuranceCalculation
{
    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل یک محاسبه بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش تمام اطلاعات محاسبه بیمه
    /// 2. مناسب برای صفحه Details
    /// 3. رعایت قرارداد Factory Method Pattern
    /// 4. پشتیبانی از تاریخ شمسی
    /// 5. نمایش اطلاعات کامل بیمار، خدمت و طرح بیمه
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsuranceCalculationDetailsViewModel
    {
        [Display(Name = "شناسه محاسبه")]
        public int InsuranceCalculationId { get; set; }

        #region Patient Information

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "تاریخ تولد بیمار")]
        public DateTime? PatientBirthDate { get; set; }

        [Display(Name = "تاریخ تولد بیمار (شمسی)")]
        public string PatientBirthDateShamsi { get; set; }

        [Display(Name = "شماره تماس بیمار")]
        public string PatientPhoneNumber { get; set; }

        #endregion

        #region Service Information

        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }

        [Display(Name = "دسته‌بندی خدمت")]
        public string ServiceCategoryName { get; set; }

        [Display(Name = "مبلغ خدمت")]
        [DisplayFormat(DataFormatString = "{0:N0} ریال")]
        public decimal ServiceAmount { get; set; }

        #endregion

        #region Insurance Information

        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

        [Display(Name = "نام طرح بیمه")]
        public string InsurancePlanName { get; set; }

        [Display(Name = "کد طرح بیمه")]
        public string InsurancePlanCode { get; set; }

        [Display(Name = "نام ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        [Display(Name = "شناسه بیمه بیمار")]
        public int PatientInsuranceId { get; set; }

        [Display(Name = "نوع بیمه")]
        public string InsuranceType { get; set; }

        [Display(Name = "بیمه اصلی")]
        public bool IsPrimaryInsurance { get; set; }

        [Display(Name = "بیمه اصلی")]
        public string IsPrimaryInsuranceText => IsPrimaryInsurance ? "بله" : "خیر";

        #endregion

        #region Calculation Details

        [Display(Name = "درصد پوشش")]
        [DisplayFormat(DataFormatString = "{0:N1}%")]
        public decimal CoveragePercent { get; set; }

        [Display(Name = "فرانشیز")]
        [DisplayFormat(DataFormatString = "{0:N0} ریال")]
        public decimal Deductible { get; set; }

        [Display(Name = "سهم بیمه")]
        [DisplayFormat(DataFormatString = "{0:N0} ریال")]
        public decimal InsuranceShare { get; set; }

        [Display(Name = "سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0} ریال")]
        public decimal PatientShare { get; set; }

        [Display(Name = "سهم بیمار (کوپه)")]
        [DisplayFormat(DataFormatString = "{0:N0} ریال")]
        public decimal? Copay { get; set; }

        [Display(Name = "پوشش خاص")]
        [DisplayFormat(DataFormatString = "{0:N0} ریال")]
        public decimal? CoverageOverride { get; set; }

        #endregion

        #region Additional Information

        [Display(Name = "تاریخ محاسبه")]
        public DateTime CalculationDate { get; set; }

        [Display(Name = "تاریخ محاسبه (شمسی)")]
        public string CalculationDateShamsi { get; set; }

        [Display(Name = "نوع محاسبه")]
        public string CalculationType { get; set; }

        [Display(Name = "وضعیت اعتبار")]
        public bool IsValid { get; set; }

        [Display(Name = "وضعیت اعتبار")]
        public string IsValidText => IsValid ? "معتبر" : "نامعتبر";

        [Display(Name = "وضعیت اعتبار")]
        public string IsValidCssClass => IsValid ? "text-success" : "text-danger";

        [Display(Name = "یادداشت‌ها")]
        public string Notes { get; set; }

        #endregion

        #region Reception/Appointment Information

        [Display(Name = "شناسه پذیرش")]
        public int? ReceptionId { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        public DateTime? ReceptionDate { get; set; }

        [Display(Name = "تاریخ پذیرش (شمسی)")]
        public string ReceptionDateShamsi { get; set; }

        [Display(Name = "شناسه قرار ملاقات")]
        public int? AppointmentId { get; set; }

        [Display(Name = "تاریخ قرار ملاقات")]
        public DateTime? AppointmentDate { get; set; }

        [Display(Name = "تاریخ قرار ملاقات (شمسی)")]
        public string AppointmentDateShamsi { get; set; }

        #endregion

        #region Audit Information

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        [Display(Name = "تاریخ آخرین به‌روزرسانی")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ آخرین به‌روزرسانی (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "به‌روزرسانی‌کننده")]
        public string UpdatedByUserName { get; set; }

        #endregion

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsuranceCalculationDetailsViewModel FromEntity(Models.Entities.InsuranceCalculation entity)
        {
            if (entity == null) return null;

            return new InsuranceCalculationDetailsViewModel
            {
                InsuranceCalculationId = entity.InsuranceCalculationId,
                
                // Patient Information
                PatientId = entity.PatientId,
                PatientName = entity.Patient?.FullName,
                PatientNationalCode = entity.Patient?.NationalCode,
                PatientBirthDate = entity.Patient?.BirthDate,
                PatientBirthDateShamsi = entity.Patient?.BirthDate?.ToPersianDateTime(),
                PatientPhoneNumber = entity.Patient?.PhoneNumber,
                
                // Service Information
                ServiceId = entity.ServiceId,
                ServiceName = entity.Service?.Title,
                ServiceCategoryName = entity.Service?.ServiceCategory?.Title,
                ServiceAmount = entity.ServiceAmount,
                
                // Insurance Information
                InsurancePlanId = entity.InsurancePlanId,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsurancePlanCode = entity.InsurancePlan?.PlanCode,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name,
                PatientInsuranceId = entity.PatientInsuranceId,
                InsuranceType = entity.PatientInsurance?.InsurancePlan?.InsuranceProvider?.Name,
                IsPrimaryInsurance = entity.PatientInsurance?.IsPrimary ?? false,
                
                // Calculation Details
                CoveragePercent = entity.CoveragePercent,
                Deductible = entity.Deductible ?? 0,
                InsuranceShare = entity.InsuranceShare,
                PatientShare = entity.PatientShare,
                Copay = entity.Copay ?? 0,
                CoverageOverride = entity.CoverageOverride,
                
                // Additional Information
                CalculationDate = entity.CalculationDate,
                CalculationDateShamsi = entity.CalculationDate.ToPersianDateTime(),
                CalculationType = entity.CalculationType,
                IsValid = entity.IsValid,
                Notes = entity.Notes,
                
                // Reception/Appointment Information
                ReceptionId = entity.ReceptionId,
                ReceptionDate = entity.Reception?.ReceptionDate,
                ReceptionDateShamsi = entity.Reception?.ReceptionDate.ToPersianDateTime(),
                AppointmentId = entity.AppointmentId,
                AppointmentDate = entity.Appointment?.AppointmentDate,
                AppointmentDateShamsi = entity.Appointment?.AppointmentDate.ToPersianDateTime(),
                
                // Audit Information
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                CreatedByUserName = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}".Trim() : null,
                UpdatedAt = entity.UpdatedAt,
                UpdatedAtShamsi = entity.UpdatedAt?.ToPersianDateTime(),
                UpdatedByUserName = entity.UpdatedByUser != null ? $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}".Trim() : null
            };
        }
    }
}
