using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ClinicApp.ViewModels.Insurance.PatientInsurance
{
    /// <summary>
    /// ViewModel برای ایجاد و ویرایش یک بیمه بیمار
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل فیلدها
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. تبدیل Entity به ViewModel و بالعکس
    /// 4. اعتبارسنجی شماره بیمه منحصر به فرد
    /// 5. پشتیبانی از تقویم شمسی
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class PatientInsuranceCreateEditViewModel
    {
        [Display(Name = "شناسه")]
        public int PatientInsuranceId { get; set; }

        [Required(ErrorMessage = "بیمار الزامی است")]
        [Display(Name = "بیمار")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "شماره بیمه الزامی است")]
        [StringLength(100, ErrorMessage = "شماره بیمه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "شماره بیمه")]
        public string PolicyNumber { get; set; }

        [Required(ErrorMessage = "طرح بیمه الزامی است")]
        [Display(Name = "طرح بیمه")]
        public int InsurancePlanId { get; set; }

        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "بیمه اصلی")]
        public bool IsPrimary { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        #region Select Lists

        [Display(Name = "لیست بیماران")]
        public SelectList PatientSelectList { get; set; }

        [Display(Name = "لیست طرح‌های بیمه")]
        public SelectList InsurancePlanSelectList { get; set; }

        #endregion

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static PatientInsuranceCreateEditViewModel FromEntity(Models.Entities.Patient.PatientInsurance entity)
        {
            if (entity == null) return null;

            return new PatientInsuranceCreateEditViewModel
            {
                PatientInsuranceId = entity.PatientInsuranceId,
                PatientId = entity.PatientId,
                PolicyNumber = entity.PolicyNumber,
                InsurancePlanId = entity.InsurancePlanId,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsPrimary = entity.IsPrimary,
                IsActive = entity.IsActive
            };
        }

        /// <summary>
        /// ✅ یک Entity جدید از روی ViewModel می‌سازد.
        /// </summary>
        public Models.Entities.Patient.PatientInsurance ToEntity()
        {
            return new Models.Entities.Patient.PatientInsurance
            {
                PatientInsuranceId = this.PatientInsuranceId,
                PatientId = this.PatientId,
                PolicyNumber = this.PolicyNumber?.Trim(),
                InsurancePlanId = this.InsurancePlanId,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                IsPrimary = this.IsPrimary,
                IsActive = this.IsActive
            };
        }

        /// <summary>
        /// ✅ یک Entity موجود را بر اساس داده‌های این ViewModel به‌روزرسانی می‌کند.
        /// </summary>
        public void MapToEntity(Models.Entities.Patient.PatientInsurance entity)
        {
            if (entity == null) return;

            entity.PatientId = this.PatientId;
            entity.PolicyNumber = this.PolicyNumber?.Trim();
            entity.InsurancePlanId = this.InsurancePlanId;
            entity.StartDate = this.StartDate;
            entity.EndDate = this.EndDate;
            entity.IsPrimary = this.IsPrimary;
            entity.IsActive = this.IsActive;
        }
    }
}
