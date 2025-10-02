using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Insurance.InsuranceCalculation
{
    /// <summary>
    /// ViewModel برای محاسبه بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ورودی‌های محاسبه بیمه
    /// 2. اعتبارسنجی کامل فیلدها
    /// 3. پشتیبانی از تقویم شمسی
    /// 4. مناسب برای فرم‌های محاسبه
    /// 5. پشتیبانی از محاسبه چندین خدمت
    /// 6. رعایت قرارداد Factory Method Pattern
    /// 
    /// نکته حیاتی: این ViewModel برای محاسبات بیمه طراحی شده است
    /// </summary>
    public class InsuranceCalculationViewModel
    {
        [Required(ErrorMessage = "بیمار الزامی است")]
        [Display(Name = "بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        [Required(ErrorMessage = "بیمه بیمار الزامی است")]
        [Display(Name = "بیمه بیمار")]
        public int PatientInsuranceId { get; set; }

        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

        [Required(ErrorMessage = "خدمت الزامی است")]
        [Display(Name = "خدمت")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "دسته‌بندی خدمت الزامی است")]
        [Display(Name = "دسته‌بندی خدمت")]
        public int ServiceCategoryId { get; set; }

        [Required(ErrorMessage = "مبلغ خدمت الزامی است")]
        [Display(Name = "مبلغ خدمت")]
        public decimal ServiceAmount { get; set; }

        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }

        [Display(Name = "دسته‌بندی خدمت")]
        public string ServiceCategoryName { get; set; }

        [Required(ErrorMessage = "تاریخ محاسبه الزامی است")]
        [Display(Name = "تاریخ محاسبه")]
        public DateTime CalculationDate { get; set; }

        [Display(Name = "تاریخ محاسبه (شمسی)")]
        public string CalculationDateShamsi { get; set; }

        [Display(Name = "هزینه کل خدمت")]
        public decimal TotalServiceCost { get; set; }

        [Display(Name = "درصد پوشش بیمه")]
        public decimal CoveragePercent { get; set; }

        [Display(Name = "فرانشیز")]
        public decimal Deductible { get; set; }

        [Display(Name = "سهم بیمه")]
        public decimal InsuranceShare { get; set; }

        [Display(Name = "سهم بیمار")]
        public decimal PatientShare { get; set; }

        [Display(Name = "Copay")]
        public decimal? Copay { get; set; }

        [Display(Name = "Coverage Override")]
        public decimal? CoverageOverride { get; set; }

        [Display(Name = "نوع محاسبه")]
        public InsuranceCalculationType CalculationType { get; set; }

        [Display(Name = "یادداشت‌ها")]
        public string Notes { get; set; }

        [Display(Name = "شناسه پذیرش")]
        public int? ReceptionId { get; set; }

        [Display(Name = "شناسه قرار ملاقات")]
        public int? AppointmentId { get; set; }

        #region SelectList Properties

        [Display(Name = "لیست بیماران")]
        public SelectList PatientSelectList { get; set; }

        [Display(Name = "لیست بیمه‌های بیمار")]
        public SelectList PatientInsuranceSelectList { get; set; }

        [Display(Name = "لیست خدمات")]
        public SelectList ServiceSelectList { get; set; }

        [Display(Name = "لیست دسته‌بندی‌های خدمت")]
        public SelectList ServiceCategorySelectList { get; set; }

        [Display(Name = "لیست انواع محاسبه")]
        public SelectList CalculationTypeSelectList { get; set; }

        #endregion

        #region Factory Methods

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsuranceCalculationViewModel FromEntity(Models.Entities.Insurance.InsuranceCalculation entity)
        {
            if (entity == null) return null;

            return new InsuranceCalculationViewModel
            {
                PatientId = entity.PatientId,
                PatientInsuranceId = entity.PatientInsuranceId,
                InsurancePlanId = entity.InsurancePlanId,
                ServiceId = entity.ServiceId,
                ServiceAmount = entity.ServiceAmount,
                CoveragePercent = entity.CoveragePercent,
                Deductible = entity.Deductible ?? 0,
                InsuranceShare = entity.InsuranceShare,
                PatientShare = entity.PatientShare,
                Copay = entity.Copay,
                CoverageOverride = entity.CoverageOverride,
                CalculationDate = entity.CalculationDate,
                CalculationType = entity.CalculationType,
                Notes = entity.Notes,
                ReceptionId = entity.ReceptionId,
                AppointmentId = entity.AppointmentId,
                PatientName = entity.Patient?.FullName,
                PatientNationalCode = entity.Patient?.NationalCode,
                ServiceName = entity.Service?.Title,
                ServiceCategoryId = entity.Service?.ServiceCategoryId ?? 0
            };
        }

        /// <summary>
        /// ✅ (Factory Method) یک Entity جدید از روی ViewModel می‌سازد.
        /// </summary>
        public Models.Entities.Insurance.InsuranceCalculation ToEntity()
        {
            return new Models.Entities.Insurance.InsuranceCalculation
            {
                PatientId = PatientId,
                PatientInsuranceId = PatientInsuranceId,
                InsurancePlanId = InsurancePlanId,
                ServiceId = ServiceId,
                ServiceAmount = ServiceAmount,
                CoveragePercent = CoveragePercent,
                Deductible = Deductible,
                InsuranceShare = InsuranceShare,
                PatientShare = PatientShare,
                Copay = Copay,
                CoverageOverride = CoverageOverride,
                CalculationDate = CalculationDate,
                CalculationType = CalculationType,
                Notes = Notes,
                ReceptionId = ReceptionId,
                AppointmentId = AppointmentId,
                IsValid = true
            };
        }

        /// <summary>
        /// ✅ (Factory Method) یک Entity موجود را با اطلاعات ViewModel به‌روزرسانی می‌کند.
        /// </summary>
        public void MapToEntity(Models.Entities.Insurance.InsuranceCalculation entity)
        {
            if (entity == null) return;

            entity.PatientId = PatientId;
            entity.PatientInsuranceId = PatientInsuranceId;
            entity.InsurancePlanId = InsurancePlanId;
            entity.ServiceId = ServiceId;
            entity.ServiceAmount = ServiceAmount;
            entity.CoveragePercent = CoveragePercent;
            entity.Deductible = Deductible;
            entity.InsuranceShare = InsuranceShare;
            entity.PatientShare = PatientShare;
            entity.Copay = Copay;
            entity.CoverageOverride = CoverageOverride;
            entity.CalculationDate = CalculationDate;
            entity.CalculationType = CalculationType;
            entity.Notes = Notes;
            entity.ReceptionId = ReceptionId;
            entity.AppointmentId = AppointmentId;
        }

        #endregion
    }
}
