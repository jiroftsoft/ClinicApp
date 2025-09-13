using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.[Module]
{
    /// <summary>
    /// ViewModel برای ایجاد و ویرایش [Module]
    /// استاندارد کامل برای فرم‌های پیچیده
    /// </summary>
    public class [Module]CreateEditViewModel
    {
        #region Basic Properties - ویژگی‌های اصلی

        /// <summary>
        /// شناسه [Module]
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        /// <summary>
        /// نام [Module]
        /// </summary>
        [Required(ErrorMessage = "نام الزامی است")]
        [StringLength(250, ErrorMessage = "نام نمی‌تواند بیش از 250 کاراکتر باشد")]
        [Display(Name = "نام")]
        public string Name { get; set; }

        /// <summary>
        /// کد [Module]
        /// </summary>
        [Required(ErrorMessage = "کد الزامی است")]
        [StringLength(100, ErrorMessage = "کد نمی‌تواند بیش از 100 کاراکتر باشد")]
        [Display(Name = "کد")]
        public string Code { get; set; }

        #endregion

        #region Persian Date Properties - ویژگی‌های تاریخ شمسی

        /// <summary>
        /// تاریخ شروع اعتبار (شمسی)
        /// </summary>
        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        [PersianDate(IsRequired = true, MustBeFutureDate = false, InvalidFormatMessage = "فرمت تاریخ شروع نامعتبر است. (مثال: 1404/06/23)")]
        [Display(Name = "تاریخ شروع")]
        public string ValidFromShamsi { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار (شمسی)
        /// </summary>
        [PersianDate(IsRequired = false, MustBeFutureDate = false, InvalidFormatMessage = "فرمت تاریخ پایان نامعتبر است. (مثال: 1404/06/23)")]
        [Display(Name = "تاریخ پایان")]
        public string ValidToShamsi { get; set; }

        #endregion

        #region Hidden DateTime Properties - ویژگی‌های مخفی تاریخ میلادی

        /// <summary>
        /// تاریخ شروع اعتبار (میلادی) - مخفی
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار (میلادی) - مخفی
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public DateTime ValidTo { get; set; }

        #endregion

        #region Financial Properties - ویژگی‌های مالی

        /// <summary>
        /// درصد پوشش بیمه
        /// </summary>
        [Required(ErrorMessage = "درصد پوشش بیمه الزامی است")]
        [Range(0, 100, ErrorMessage = "درصد پوشش بیمه باید بین 0 تا 100 باشد")]
        [Display(Name = "درصد پوشش بیمه")]
        public decimal CoveragePercent { get; set; }

        /// <summary>
        /// مبلغ فرانشیز
        /// </summary>
        [Required(ErrorMessage = "مبلغ فرانشیز الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ فرانشیز باید بزرگتر یا مساوی صفر باشد")]
        [Display(Name = "مبلغ فرانشیز")]
        public decimal Deductible { get; set; }

        #endregion

        #region Additional Properties - ویژگی‌های تکمیلی

        /// <summary>
        /// توضیحات
        /// </summary>
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        #endregion

        #region Lookup Properties - ویژگی‌های جستجو

        /// <summary>
        /// شناسه ارائه‌دهنده بیمه
        /// </summary>
        [Required(ErrorMessage = "ارائه‌دهنده بیمه الزامی است")]
        [Display(Name = "ارائه‌دهنده بیمه")]
        public int InsuranceProviderId { get; set; }

        /// <summary>
        /// نام ارائه‌دهنده بیمه (برای نمایش)
        /// </summary>
        [Display(Name = "نام ارائه‌دهنده")]
        public string InsuranceProviderName { get; set; }

        #endregion

        #region Conversion Methods - متدهای تبدیل

        /// <summary>
        /// تبدیل ViewModel به Entity
        /// </summary>
        public Models.Entities.[Module].[Module] ToEntity()
        {
            return new Models.Entities.[Module].[Module]
            {
                Id = this.Id,
                Name = this.Name?.Trim(),
                Code = this.Code?.Trim(),
                ValidFrom = ConvertPersianToDateTime(this.ValidFromShamsi),
                ValidTo = ConvertPersianToDateTime(this.ValidToShamsi),
                CoveragePercent = this.CoveragePercent,
                Deductible = this.Deductible,
                Description = this.Description?.Trim(),
                IsActive = this.IsActive,
                InsuranceProviderId = this.InsuranceProviderId
            };
        }

        /// <summary>
        /// تبدیل Entity به ViewModel
        /// </summary>
        public static [Module]CreateEditViewModel FromEntity(Models.Entities.[Module].[Module] entity)
        {
            if (entity == null)
                return new [Module]CreateEditViewModel();

            return new [Module]CreateEditViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                ValidFrom = entity.ValidFrom,
                ValidTo = entity.ValidTo,
                ValidFromShamsi = entity.ValidFrom.ToPersianDateString(),
                ValidToShamsi = entity.ValidTo?.ToPersianDateString(),
                CoveragePercent = entity.CoveragePercent,
                Deductible = entity.Deductible,
                Description = entity.Description,
                IsActive = entity.IsActive,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderName = entity.InsuranceProvider?.Name
            };
        }

        /// <summary>
        /// به‌روزرسانی Entity موجود
        /// </summary>
        public void MapToEntity(Models.Entities.[Module].[Module] entity)
        {
            entity.Name = this.Name?.Trim();
            entity.Code = this.Code?.Trim();
            entity.ValidFrom = ConvertPersianToDateTime(this.ValidFromShamsi);
            entity.ValidTo = ConvertPersianToDateTime(this.ValidToShamsi);
            entity.CoveragePercent = this.CoveragePercent;
            entity.Deductible = this.Deductible;
            entity.Description = this.Description?.Trim();
            entity.IsActive = this.IsActive;
            entity.InsuranceProviderId = this.InsuranceProviderId;
        }

        #endregion

        #region Helper Methods - متدهای کمکی

        /// <summary>
        /// تبدیل تاریخ شمسی به DateTime
        /// </summary>
        private DateTime ConvertPersianToDateTime(string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                return DateTime.Now.AddYears(1); // مقدار پیش‌فرض برای ValidTo

            try
            {
                return PersianDateHelper.ToGregorianDate(persianDate);
            }
            catch
            {
                // در صورت خطا، تاریخ فعلی + یک سال را برگردان
                return DateTime.Now.AddYears(1);
            }
        }

        /// <summary>
        /// تبدیل تاریخ شمسی به DateTime? (nullable)
        /// </summary>
        private DateTime? ConvertPersianToDateTimeNullable(string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                return null;

            try
            {
                return PersianDateHelper.ToGregorianDate(persianDate);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// محاسبه درصد پرداخت بیمار
        /// </summary>
        public decimal PatientSharePercent => 100 - CoveragePercent;

        /// <summary>
        /// بررسی معتبر بودن بازه زمانی
        /// </summary>
        public bool IsValidDateRange
        {
            get
            {
                try
                {
                    var fromDate = ConvertPersianToDateTime(ValidFromShamsi);
                    var toDate = ConvertPersianToDateTime(ValidToShamsi);
                    return fromDate < toDate;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// محاسبه تعداد روزهای اعتبار
        /// </summary>
        public int ValidityDays
        {
            get
            {
                try
                {
                    var fromDate = ConvertPersianToDateTime(ValidFromShamsi);
                    var toDate = ConvertPersianToDateTime(ValidToShamsi);
                    return (int)(toDate - fromDate).TotalDays;
                }
                catch
                {
                    return 0;
                }
            }
        }

        #endregion

        #region Validation Methods - متدهای اعتبارسنجی

        /// <summary>
        /// اعتبارسنجی سفارشی
        /// </summary>
        public ValidationResult Validate(ValidationContext validationContext)
        {
            var errors = new List<string>();

            // اعتبارسنجی بازه زمانی
            if (!IsValidDateRange)
            {
                errors.Add("تاریخ پایان اعتبار نمی‌تواند قبل از تاریخ شروع اعتبار باشد");
            }

            // اعتبارسنجی منطق مالی
            if (CoveragePercent + (Deductible > 0 ? 10 : 0) > 100)
            {
                errors.Add("مجموع درصد پوشش و فرانشیز نباید از 100% بیشتر باشد");
            }

            // اعتبارسنجی دوره اعتبار
            if (ValidityDays > 365 * 5) // حداکثر 5 سال
            {
                errors.Add("دوره اعتبار نمی‌تواند بیش از 5 سال باشد");
            }

            if (errors.Any())
            {
                return new ValidationResult(string.Join("; ", errors));
            }

            return ValidationResult.Success;
        }

        #endregion

        #region Display Methods - متدهای نمایش

        /// <summary>
        /// نمایش تاریخ شروع به صورت شمسی
        /// </summary>
        public string DisplayValidFrom => ValidFrom.ToPersianDateString();

        /// <summary>
        /// نمایش تاریخ پایان به صورت شمسی
        /// </summary>
        public string DisplayValidTo => ValidTo?.ToPersianDateString() ?? "نامحدود";

        /// <summary>
        /// نمایش مبلغ فرانشیز با فرمت
        /// </summary>
        public string DisplayDeductible => Deductible.ToString("N0") + " تومان";

        /// <summary>
        /// نمایش درصد پوشش با فرمت
        /// </summary>
        public string DisplayCoveragePercent => CoveragePercent.ToString("F1") + "%";

        /// <summary>
        /// نمایش وضعیت فعال بودن
        /// </summary>
        public string DisplayIsActive => IsActive ? "فعال" : "غیرفعال";

        #endregion

        #region Factory Methods - متدهای کارخانه

        /// <summary>
        /// ایجاد ViewModel جدید برای ایجاد
        /// </summary>
        public static [Module]CreateEditViewModel CreateNew(int? insuranceProviderId = null)
        {
            return new [Module]CreateEditViewModel
            {
                InsuranceProviderId = insuranceProviderId ?? 0,
                IsActive = true,
                ValidFromShamsi = DateTime.Now.ToPersianDateString(),
                CoveragePercent = 0,
                Deductible = 0
            };
        }

        /// <summary>
        /// ایجاد ViewModel برای ویرایش
        /// </summary>
        public static [Module]CreateEditViewModel CreateForEdit(Models.Entities.[Module].[Module] entity)
        {
            return FromEntity(entity);
        }

        #endregion
    }

    /// <summary>
    /// ViewModel برای جستجو و انتخاب [Module]
    /// </summary>
    public class [Module]LookupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string InsuranceProviderName { get; set; }
        public decimal CoveragePercent { get; set; }
        public decimal Deductible { get; set; }
        public bool IsActive { get; set; }
        public string DisplayText => $"{Name} ({Code}) - {InsuranceProviderName}";
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات [Module]
    /// </summary>
    public class [Module]DetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ValidFromShamsi { get; set; }
        public string ValidToShamsi { get; set; }
        public decimal CoveragePercent { get; set; }
        public decimal Deductible { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string InsuranceProviderName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }

    /// <summary>
    /// ViewModel برای فهرست [Module]
    /// </summary>
    public class [Module]ListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string InsuranceProviderName { get; set; }
        public decimal CoveragePercent { get; set; }
        public decimal Deductible { get; set; }
        public bool IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string DisplayValidFrom => ValidFrom.ToPersianDateString();
        public string DisplayValidTo => ValidTo?.ToPersianDateString() ?? "نامحدود";
        public string DisplayCoveragePercent => CoveragePercent.ToString("F1") + "%";
        public string DisplayDeductible => Deductible.ToString("N0") + " تومان";
        public string DisplayIsActive => IsActive ? "فعال" : "غیرفعال";
    }
}
