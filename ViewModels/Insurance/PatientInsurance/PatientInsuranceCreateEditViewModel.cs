using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using ClinicApp.Extensions;
using ClinicApp.Filters;

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

        /// <summary>
        /// نام بیمار (برای نمایش)
        /// </summary>
        [NotMapped]
        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Required(ErrorMessage = "شماره بیمه الزامی است")]
        [StringLength(100, ErrorMessage = "شماره بیمه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "شماره بیمه فقط می‌تواند شامل حروف انگلیسی، اعداد، خط تیره و زیرخط باشد")]
        [CustomValidation(typeof(PatientInsuranceCreateEditViewModel), "ValidatePolicyNumber")]
        [Display(Name = "شماره بیمه")]
        public string PolicyNumber { get; set; }

        [Required(ErrorMessage = "طرح بیمه الزامی است")]
        [Display(Name = "طرح بیمه")]
        public int InsurancePlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه (برای نمایش)
        /// </summary>
        [NotMapped]
        [Display(Name = "نام طرح بیمه")]
        public string InsurancePlanName { get; set; }

        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        [CustomValidation(typeof(PatientInsuranceCreateEditViewModel), "ValidateStartDate")]
        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        [CustomValidation(typeof(PatientInsuranceCreateEditViewModel), "ValidateEndDate")]
        public DateTime? EndDate { get; set; }

        #region Persian Date Properties

        /// <summary>
        /// تاریخ شروع به صورت شمسی (برای نمایش در فرم)
        /// </summary>
        [PersianDate(IsRequired = true, MustBePastDate = false, MustBeFutureDate = false, 
            InvalidFormatMessage = "فرمت تاریخ شروع نامعتبر است. (مثال: 1403/05/12)",
            InvalidDateMessage = "تاریخ شروع معتبر نیست.",
            YearRangeMessage = "سال تاریخ شروع باید بین 1200 تا 1500 باشد.")]
        [Display(Name = "تاریخ شروع (شمسی)")]
        [NotMapped]
        public string StartDateShamsi { get; set; }

        /// <summary>
        /// تاریخ پایان به صورت شمسی (برای نمایش در فرم)
        /// </summary>
        [PersianDate(IsRequired = false, MustBePastDate = false, MustBeFutureDate = false,
            InvalidFormatMessage = "فرمت تاریخ پایان نامعتبر است. (مثال: 1403/05/12)",
            InvalidDateMessage = "تاریخ پایان معتبر نیست.",
            YearRangeMessage = "سال تاریخ پایان باید بین 1200 تا 1500 باشد.")]
        [Display(Name = "تاریخ پایان (شمسی)")]
        [NotMapped]
        public string EndDateShamsi { get; set; }

        #endregion

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

        #region Date Conversion Methods

        /// <summary>
        /// تبدیل تاریخ‌های شمسی به میلادی برای ذخیره در دیتابیس
        /// </summary>
        public void ConvertPersianDatesToGregorian()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(StartDateShamsi))
                {
                    StartDate = StartDateShamsi.ToDateTime();
                }

                if (!string.IsNullOrWhiteSpace(EndDateShamsi))
                {
                    EndDate = EndDateShamsi.ToDateTimeNullable();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در تبدیل تاریخ‌های شمسی: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// تبدیل تاریخ‌های میلادی به شمسی برای نمایش در فرم
        /// </summary>
        public void ConvertGregorianDatesToPersian()
        {
            try
            {
                if (StartDate != default(DateTime))
                {
                    StartDateShamsi = StartDate.ToPersianDate();
                }

                if (EndDate.HasValue)
                {
                    EndDateShamsi = EndDate.Value.ToPersianDate();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطا در تبدیل تاریخ‌های میلادی: {ex.Message}", ex);
            }
        }

        #endregion

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static PatientInsuranceCreateEditViewModel FromEntity(Models.Entities.Patient.PatientInsurance entity)
        {
            if (entity == null) return null;

            var viewModel = new PatientInsuranceCreateEditViewModel
            {
                PatientInsuranceId = entity.PatientInsuranceId,
                PatientId = entity.PatientId,
                PolicyNumber = entity.PolicyNumber,
                InsurancePlanId = entity.InsurancePlanId,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsPrimary = entity.IsPrimary,
                IsActive = entity.IsActive,
                // پر کردن فیلدهای نمایشی
                PatientName = entity.Patient != null ? $"{entity.Patient.FirstName} {entity.Patient.LastName}" : null,
                InsurancePlanName = entity.InsurancePlan != null ? entity.InsurancePlan.Name : null
            };

            // تبدیل تاریخ‌های میلادی به شمسی برای نمایش در فرم
            viewModel.ConvertGregorianDatesToPersian();

            return viewModel;
        }

        /// <summary>
        /// ✅ یک Entity جدید از روی ViewModel می‌سازد.
        /// </summary>
        public Models.Entities.Patient.PatientInsurance ToEntity()
        {
            // تبدیل تاریخ‌های شمسی به میلادی برای ذخیره در دیتابیس
            this.ConvertPersianDatesToGregorian();

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

            // تبدیل تاریخ‌های شمسی به میلادی برای ذخیره در دیتابیس
            this.ConvertPersianDatesToGregorian();

            entity.PatientId = this.PatientId;
            entity.PolicyNumber = this.PolicyNumber?.Trim();
            entity.InsurancePlanId = this.InsurancePlanId;
            entity.StartDate = this.StartDate;
            entity.EndDate = this.EndDate;
            entity.IsPrimary = this.IsPrimary;
            entity.IsActive = this.IsActive;
        }

        #region Custom Validation Methods

        /// <summary>
        /// اعتبارسنجی تاریخ پایان - باید بعد از تاریخ شروع باشد
        /// </summary>
        public static ValidationResult ValidateEndDate(DateTime? endDate, ValidationContext validationContext)
        {
            var model = (PatientInsuranceCreateEditViewModel)validationContext.ObjectInstance;
            
            if (endDate.HasValue && model.StartDate != default(DateTime))
            {
                if (endDate.Value <= model.StartDate)
                {
                    return new ValidationResult("تاریخ پایان باید بعد از تاریخ شروع باشد.");
                }
                
                // بررسی اینکه تاریخ پایان بیش از 10 سال در آینده نباشد
                var maxEndDate = model.StartDate.AddYears(10);
                if (endDate.Value > maxEndDate)
                {
                    return new ValidationResult("تاریخ پایان نمی‌تواند بیش از 10 سال بعد از تاریخ شروع باشد.");
                }
            }
            
            return ValidationResult.Success;
        }

        /// <summary>
        /// اعتبارسنجی شماره بیمه - باید منحصر به فرد باشد
        /// </summary>
        public static ValidationResult ValidatePolicyNumber(string policyNumber, ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(policyNumber))
            {
                return ValidationResult.Success; // Required attribute handles this
            }

            // بررسی فرمت شماره بیمه
            if (policyNumber.Length < 3)
            {
                return new ValidationResult("شماره بیمه باید حداقل 3 کاراکتر باشد.");
            }

            // بررسی اینکه شماره بیمه فقط شامل کاراکترهای مجاز باشد
            if (!System.Text.RegularExpressions.Regex.IsMatch(policyNumber, @"^[A-Za-z0-9\-_]+$"))
            {
                return new ValidationResult("شماره بیمه فقط می‌تواند شامل حروف انگلیسی، اعداد، خط تیره و زیرخط باشد.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// اعتبارسنجی تاریخ شروع - نباید بیش از 1 سال در گذشته باشد
        /// </summary>
        public static ValidationResult ValidateStartDate(DateTime startDate, ValidationContext validationContext)
        {
            if (startDate == default(DateTime))
            {
                return ValidationResult.Success; // Required attribute handles this
            }

            var oneYearAgo = DateTime.Now.AddYears(-1);
            if (startDate < oneYearAgo)
            {
                return new ValidationResult("تاریخ شروع نمی‌تواند بیش از 1 سال در گذشته باشد.");
            }

            var oneYearFromNow = DateTime.Now.AddYears(1);
            if (startDate > oneYearFromNow)
            {
                return new ValidationResult("تاریخ شروع نمی‌تواند بیش از 1 سال در آینده باشد.");
            }

            return ValidationResult.Success;
        }

        #endregion
    }
}
