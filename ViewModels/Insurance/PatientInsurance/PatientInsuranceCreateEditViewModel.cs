using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using ClinicApp.Extensions;
using ClinicApp.Filters;
using ClinicApp.Models.Enums;

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

        /// <summary>
        /// کد ملی بیمار (برای نمایش)
        /// </summary>
        [NotMapped]
        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        /// <summary>
        /// شماره تلفن بیمار (برای نمایش)
        /// </summary>
        [NotMapped]
        [Display(Name = "شماره تلفن بیمار")]
        public string PatientPhone { get; set; }

        /// <summary>
        /// تاریخ تولد بیمار (برای نمایش)
        /// </summary>
        [NotMapped]
        [Display(Name = "تاریخ تولد بیمار")]
        public string PatientBirthDate { get; set; }

        [Required(ErrorMessage = "شماره بیمه الزامی است")]
        [StringLength(100, ErrorMessage = "شماره بیمه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "شماره بیمه فقط می‌تواند شامل حروف انگلیسی، اعداد، خط تیره و زیرخط باشد")]
        [CustomValidation(typeof(PatientInsuranceCreateEditViewModel), "ValidatePolicyNumber")]
        [Display(Name = "شماره بیمه")]
        public string PolicyNumber { get; set; }

        /// <summary>
        /// شماره معرفی نامه بیمه تکمیلی (اختیاری)
        /// </summary>
        [StringLength(100, ErrorMessage = "شماره معرفی نامه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [RegularExpression(@"^[A-Za-z0-9\-_]*$", ErrorMessage = "شماره معرفی نامه فقط می‌تواند شامل حروف انگلیسی، اعداد، خط تیره و زیرخط باشد")]
        [Display(Name = "شماره معرفی نامه")]
        public string SupplementaryPolicyNumber { get; set; }

        [Required(ErrorMessage = "بیمه‌گذار الزامی است")]
        [Display(Name = "بیمه‌گذار")]
        public int InsuranceProviderId { get; set; }

        /// <summary>
        /// نام بیمه‌گذار (برای نمایش)
        /// </summary>
        [NotMapped]
        [Display(Name = "نام بیمه‌گذار")]
        public string InsuranceProviderName { get; set; }

        [Required(ErrorMessage = "طرح بیمه الزامی است")]
        [Display(Name = "طرح بیمه")]
        public int InsurancePlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه (برای نمایش)
        /// </summary>
        [NotMapped]
        [Display(Name = "نام طرح بیمه")]
        public string InsurancePlanName { get; set; }

        /// <summary>
        /// بیمه‌گذار تکمیلی (اختیاری)
        /// </summary>
                [Display(Name = "بیمه‌گذار تکمیلی (اختیاری)")]
                public int? SupplementaryInsuranceProviderId { get; set; }

        /// <summary>
        /// نام بیمه‌گذار تکمیلی (برای نمایش)
        /// </summary>
        [NotMapped]
        [Display(Name = "نام بیمه‌گذار تکمیلی")]
        public string SupplementaryInsuranceProviderName { get; set; }

        /// <summary>
        /// طرح بیمه تکمیلی (اختیاری)
        /// </summary>
                [Display(Name = "طرح بیمه تکمیلی (اختیاری)")]
                public int? SupplementaryInsurancePlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه تکمیلی (برای نمایش)
        /// </summary>
        [NotMapped]
        [Display(Name = "نام طرح بیمه تکمیلی")]
        public string SupplementaryInsurancePlanName { get; set; }

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

        /// <summary>
        /// اولویت بیمه
        /// استفاده از enum برای مدیریت بهتر اولویت‌ها
        /// </summary>
        [Required(ErrorMessage = "اولویت بیمه الزامی است.")]
        [Display(Name = "اولویت بیمه")]
        public InsurancePriority Priority { get; set; } = InsurancePriority.Primary;

        #region Select Lists

        [Display(Name = "لیست بیماران")]
        public SelectList PatientSelectList { get; set; }

        [Display(Name = "لیست بیمه‌گذاران")]
        public SelectList InsuranceProviderSelectList { get; set; }

        [Display(Name = "لیست طرح‌های بیمه")]
        public SelectList InsurancePlanSelectList { get; set; }

        [Display(Name = "لیست بیمه‌گذاران تکمیلی")]
        public SelectList SupplementaryInsuranceProviderSelectList { get; set; }

        [Display(Name = "لیست طرح‌های بیمه تکمیلی")]
        public SelectList SupplementaryInsurancePlanSelectList { get; set; }

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
                    // Debug logging
                    System.Diagnostics.Debug.WriteLine($"🔍 Converting StartDate: {StartDate}");
                    
                    // همیشه تبدیل را انجام دهیم (مطابق با استاندارد InsurancePlan)
                    StartDateShamsi = StartDate.ToPersianDate();
                    System.Diagnostics.Debug.WriteLine($"🔍 Converted to: {StartDateShamsi}");
                }

                if (EndDate.HasValue)
                {
                    // Debug logging
                    System.Diagnostics.Debug.WriteLine($"🔍 Converting EndDate: {EndDate.Value}");
                    
                    // همیشه تبدیل را انجام دهیم (مطابق با استاندارد InsurancePlan)
                    EndDateShamsi = EndDate.Value.ToPersianDate();
                    System.Diagnostics.Debug.WriteLine($"🔍 Converted to: {EndDateShamsi}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in ConvertGregorianDatesToPersian: {ex.Message}");
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
                SupplementaryPolicyNumber = entity.SupplementaryPolicyNumber,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsurancePlanId = entity.InsurancePlanId,
                SupplementaryInsuranceProviderId = entity.SupplementaryInsuranceProviderId,
                SupplementaryInsurancePlanId = entity.SupplementaryInsurancePlanId,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsPrimary = entity.IsPrimary,
                IsActive = entity.IsActive,
                Priority = entity.Priority,
                // پر کردن فیلدهای نمایشی
                PatientName = entity.Patient != null ? $"{entity.Patient.FirstName} {entity.Patient.LastName}" : null,
                PatientNationalCode = entity.Patient?.NationalCode,
                PatientPhone = entity.Patient?.PhoneNumber,
                PatientBirthDate = entity.Patient?.BirthDate?.ToPersianDate(),
                InsuranceProviderName = entity.InsuranceProvider != null ? entity.InsuranceProvider.Name : null,
                InsurancePlanName = entity.InsurancePlan != null ? entity.InsurancePlan.Name : null,
                SupplementaryInsuranceProviderName = entity.SupplementaryInsuranceProvider != null ? entity.SupplementaryInsuranceProvider.Name : null,
                SupplementaryInsurancePlanName = entity.SupplementaryInsurancePlan != null ? entity.SupplementaryInsurancePlan.Name : null
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
                SupplementaryPolicyNumber = this.SupplementaryPolicyNumber?.Trim(),
                InsuranceProviderId = this.InsuranceProviderId,
                InsurancePlanId = this.InsurancePlanId,
                SupplementaryInsuranceProviderId = this.SupplementaryInsuranceProviderId,
                SupplementaryInsurancePlanId = this.SupplementaryInsurancePlanId,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                IsPrimary = this.IsPrimary,
                IsActive = this.IsActive,
                Priority = this.Priority
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
            entity.SupplementaryPolicyNumber = this.SupplementaryPolicyNumber?.Trim();
            entity.InsuranceProviderId = this.InsuranceProviderId;
            entity.InsurancePlanId = this.InsurancePlanId;
            entity.SupplementaryInsuranceProviderId = this.SupplementaryInsuranceProviderId;
            entity.SupplementaryInsurancePlanId = this.SupplementaryInsurancePlanId;
            entity.StartDate = this.StartDate;
            entity.EndDate = this.EndDate;
            entity.IsPrimary = this.IsPrimary;
            entity.IsActive = this.IsActive;
            entity.Priority = this.Priority;
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
                
                // بررسی اینکه تاریخ پایان در گذشته نباشد
                var now = DateTime.Now;
                if (endDate.Value < now)
                {
                    return new ValidationResult("تاریخ پایان نمی‌تواند در گذشته باشد.");
                }
                
                // محدودیت 10 سال آینده حذف شد - منشی می‌تواند اعتبار بیمه را تا هر زمان آینده تنظیم کند
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

            var now = DateTime.Now;
            var oneYearAgo = now.AddYears(-1);

            // Debug logging
            var log = Serilog.Log.ForContext(typeof(PatientInsuranceCreateEditViewModel));
            log.Information("🔍 StartDate Validation: StartDate={StartDate}, Now={Now}, OneYearAgo={OneYearAgo}", 
                startDate.ToString("yyyy/MM/dd"), now.ToString("yyyy/MM/dd"), oneYearAgo.ToString("yyyy/MM/dd"));

            // فقط بررسی می‌کنیم که تاریخ شروع بیش از 1 سال در گذشته نباشد
            // محدودیت 1 سال آینده حذف شد - منشی می‌تواند اعتبار بیمه را تا هر زمان آینده تنظیم کند
            if (startDate < oneYearAgo)
            {
                log.Warning("🔍 StartDate Validation Failed: StartDate {StartDate} is more than 1 year in the past", startDate.ToString("yyyy/MM/dd"));
                return new ValidationResult("تاریخ شروع نمی‌تواند بیش از 1 سال در گذشته باشد.");
            }

            log.Information("🔍 StartDate Validation Passed: StartDate {StartDate} is valid (no future limit)", startDate.ToString("yyyy/MM/dd"));
            return ValidationResult.Success;
        }

        #endregion
    }
}
