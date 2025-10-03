using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using ClinicApp.Filters;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.Insurance.InsurancePlan
{
    /// <summary>
    /// ViewModel برای ایجاد و ویرایش یک طرح بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل فیلدها
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. تبدیل Entity به ViewModel و بالعکس
    /// 4. اعتبارسنجی کد و نام منحصر به فرد
    /// 5. پشتیبانی از تقویم شمسی
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsurancePlanCreateEditViewModel
    {
        [Display(Name = "شناسه")]
        public int InsurancePlanId { get; set; }

        [Required(ErrorMessage = "نام طرح بیمه الزامی است")]
        [StringLength(200, ErrorMessage = "نام طرح بیمه نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "نام طرح بیمه")]
        public string Name { get; set; }

        [Required(ErrorMessage = "کد طرح بیمه الزامی است")]
        [StringLength(50, ErrorMessage = "کد طرح بیمه نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [Display(Name = "کد طرح بیمه")]
        public string PlanCode { get; set; }

        [Required(ErrorMessage = "ارائه‌دهنده بیمه الزامی است")]
        [Display(Name = "ارائه‌دهنده بیمه")]
        public int InsuranceProviderId { get; set; }

        [Required(ErrorMessage = "درصد پوشش الزامی است")]
        [Range(0, 100, ErrorMessage = "درصد پوشش باید بین 0 تا 100 باشد")]
        [Display(Name = "درصد پوشش")]
        public decimal CoveragePercent { get; set; }

        [Required(ErrorMessage = "فرانشیز الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "فرانشیز باید بزرگتر یا مساوی صفر باشد")]
        [Display(Name = "فرانشیز")]
        public decimal Deductible { get; set; }

        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        [PersianDate(IsRequired = true, MustBeFutureDate = false, MinYear = 1200, MaxYear = 1500,
            InvalidFormatMessage = "فرمت تاریخ شروع نامعتبر است. (مثال: 1404/06/23)",
            YearRangeMessage = "سال تاریخ شروع باید بین 1200 تا 1500 باشد.")]
        [Display(Name = "تاریخ شروع")]
        public string ValidFromShamsi { get; set; }

        [PersianDate(IsRequired = false, MustBeFutureDate = false, MinYear = 1200, MaxYear = 1500,
            InvalidFormatMessage = "فرمت تاریخ پایان نامعتبر است. (مثال: 1404/06/23)",
            YearRangeMessage = "سال تاریخ پایان باید بین 1200 تا 1500 باشد.")]
        [Display(Name = "تاریخ پایان")]
        public string ValidToShamsi { get; set; }

        [HiddenInput(DisplayValue = false)]
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public DateTime ValidFrom { get; set; }

        [HiddenInput(DisplayValue = false)]
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public DateTime? ValidTo { get; set; }

        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // نام ارائه‌دهنده بیمه (فقط برای نمایش)
        [Display(Name = "نام ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        // Select Lists for Dropdowns
        public List<InsuranceProviderLookupViewModel> InsuranceProviders { get; set; } = new List<InsuranceProviderLookupViewModel>();
        public SelectList InsuranceProviderSelectList { get; set; }

        /// <summary>
        /// ایجاد SelectList برای ارائه‌دهندگان بیمه
        /// </summary>
        public void CreateInsuranceProviderSelectList()
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "انتخاب ارائه‌دهنده بیمه", Selected = InsuranceProviderId == 0 }
            };

            foreach (var provider in InsuranceProviders)
            {
                items.Add(new SelectListItem
                {
                    Value = provider.InsuranceProviderId.ToString(),
                    Text = provider.Name,
                    Selected = InsuranceProviderId == provider.InsuranceProviderId
                });
            }

            InsuranceProviderSelectList = new SelectList(items, "Value", "Text", InsuranceProviderId.ToString());
        }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsurancePlanCreateEditViewModel FromEntity(Models.Entities.Insurance.InsurancePlan entity)
        {
            if (entity == null) return null;

            return new InsurancePlanCreateEditViewModel
            {
                InsurancePlanId = entity.InsurancePlanId,
                Name = entity.Name,
                PlanCode = entity.PlanCode,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderName = entity.InsuranceProvider?.Name, // بارگیری نام ارائه‌دهنده
                CoveragePercent = entity.CoveragePercent,
                Deductible = entity.Deductible,
                ValidFrom = entity.ValidFrom,
                ValidTo = entity.ValidTo,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        /// <summary>
        /// ✅ یک Entity جدید از روی ViewModel می‌سازد.
        /// </summary>
        public Models.Entities.Insurance.InsurancePlan ToEntity()
        {
            return new Models.Entities.Insurance.InsurancePlan
            {
                InsurancePlanId = this.InsurancePlanId,
                Name = this.Name?.Trim(),
                PlanCode = this.PlanCode?.Trim(),
                InsuranceProviderId = this.InsuranceProviderId,
                CoveragePercent = this.CoveragePercent,
                Deductible = this.Deductible,
                ValidFrom = this.ValidFrom,
                ValidTo = this.ValidTo ?? DateTime.Now.AddYears(1),
                Description = this.Description?.Trim(),
                IsActive = this.IsActive
            };
        }

        /// <summary>
        /// ✅ یک Entity موجود را بر اساس داده‌های این ViewModel به‌روزرسانی می‌کند.
        /// </summary>
        public void MapToEntity(Models.Entities.Insurance.InsurancePlan entity)
        {
            if (entity == null) return;

            entity.Name = this.Name?.Trim();
            entity.PlanCode = this.PlanCode?.Trim();
            entity.InsuranceProviderId = this.InsuranceProviderId;
            entity.CoveragePercent = this.CoveragePercent;
            entity.Deductible = this.Deductible;
            entity.ValidFrom = this.ValidFrom;
            entity.ValidTo = this.ValidTo ?? DateTime.Now.AddYears(1);
            entity.Description = this.Description?.Trim();
            entity.IsActive = this.IsActive;
        }

        /// <summary>
        /// تبدیل تاریخ‌های شمسی به میلادی برای ذخیره در دیتابیس
        /// استفاده از DateTimeExtensions موجود
        /// </summary>
        public void ConvertPersianDatesToGregorian()
        {
            if (!string.IsNullOrEmpty(ValidFromShamsi))
            {
                ValidFrom = ValidFromShamsi.ToDateTime();
            }

            if (!string.IsNullOrEmpty(ValidToShamsi))
            {
                ValidTo = ValidToShamsi.ToDateTime();
            }
        }

        /// <summary>
        /// تبدیل تاریخ‌های میلادی به شمسی برای نمایش به کاربر
        /// استفاده از DateTimeExtensions موجود
        /// </summary>
        public void ConvertGregorianDatesToPersian()
        {
            if (ValidFrom != DateTime.MinValue)
            {
                ValidFromShamsi = ValidFrom.ToPersianDate();
            }

            if (ValidTo.HasValue)
            {
                ValidToShamsi = ValidTo.Value.ToPersianDate();
            }
        }
    }
}
