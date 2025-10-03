using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Extensions;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ViewModel برای نمایش تنظیمات کای
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات کامل کای‌ها
    /// 2. پشتیبانی از Scope جدید
    /// 3. نمایش تاریخ شمسی
    /// 4. نمایش اطلاعات Audit Trail
    /// 5. رعایت قرارداد Factory Method Pattern
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class FactorSettingViewModel
    {
        [Display(Name = "شناسه")]
        public int Id { get; set; }

        [Display(Name = "نام")]
        public string Name { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "نوع کای")]
        public ServiceComponentType FactorType { get; set; }

        [Display(Name = "دامنه کای")]
        public FactorScope Scope { get; set; }

        [Display(Name = "دامنه کای (نمایشی)")]
        public string ScopeDisplayName { get; set; }

        [Display(Name = "هشتگ‌دار")]
        public bool IsHashtagged { get; set; }

        [Display(Name = "مقدار کای (ریال)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Value { get; set; }

        [Display(Name = "سال مالی")]
        public int FinancialYear { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedBy { get; set; }

        [Display(Name = "تاریخ بروزرسانی")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ بروزرسانی (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "بروزرسانی‌کننده")]
        public string UpdatedBy { get; set; }

        // ویژگی‌های اضافی برای نمایش
        [Display(Name = "نوع کای (نمایشی)")]
        public string FactorTypeText => FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای";

        [Display(Name = "وضعیت هشتگ (نمایشی)")]
        public string IsHashtaggedText => IsHashtagged ? "هشتگ‌دار" : "عادی";

        [Display(Name = "وضعیت فعال (نمایشی)")]
        public string IsActiveText => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static FactorSettingViewModel FromEntity(Models.Entities.Clinic.FactorSetting entity)
        {
            if (entity == null) return null;

            return new FactorSettingViewModel
            {
                Id = entity.FactorSettingId,
                Name = $"کای {(entity.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای")} {entity.FinancialYear}",
                Description = entity.Description,
                FactorType = entity.FactorType,
                Scope = entity.Scope,
                IsHashtagged = entity.IsHashtagged,
                Value = entity.Value,
                FinancialYear = entity.FinancialYear,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateString(),
                CreatedBy = entity.CreatedByUserId,
                UpdatedAt = entity.UpdatedAt,
                UpdatedAtShamsi = entity.UpdatedAt?.ToPersianDateString(),
                UpdatedBy = entity.UpdatedByUserId
            };
        }
    }

    /// <summary>
    /// ViewModel برای ایجاد و ویرایش تنظیمات کای
    /// </summary>
    public class FactorSettingCreateEditViewModel
    {
        [Display(Name = "شناسه")]
        public int Id { get; set; }

        [Required(ErrorMessage = "نوع کای الزامی است")]
        [Display(Name = "نوع کای")]
        public ServiceComponentType FactorType { get; set; }

        [Required(ErrorMessage = "دامنه کای الزامی است")]
        [Display(Name = "دامنه کای")]
        public FactorScope Scope { get; set; }

        [Display(Name = "هشتگ‌دار")]
        public bool IsHashtagged { get; set; }

        [Required(ErrorMessage = "مقدار کای الزامی است")]
        [Range(1, 999999999, ErrorMessage = "مقدار کای باید بین 1 تا 999,999,999 ریال باشد")]
        [Display(Name = "مقدار کای (ریال)")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "سال مالی الزامی است")]
        [Range(1400, 1700, ErrorMessage = "سال مالی باید بین 1400 تا 1700 باشد")]
        [Display(Name = "سال مالی")]
        public int FinancialYear { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "تاریخ شروع اعتبار")]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; } = DateTime.Now;

        [Display(Name = "تاریخ پایان اعتبار")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        // ویژگی‌های اضافی برای نمایش
        [Display(Name = "نوع کای (نمایشی)")]
        public string FactorTypeText => FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای";

        [Display(Name = "دامنه کای (نمایشی)")]
        public string ScopeDisplayName => GetScopeDisplayName(Scope);

        [Display(Name = "وضعیت هشتگ (نمایشی)")]
        public string IsHashtaggedText => IsHashtagged ? "هشتگ‌دار" : "عادی";

        [Display(Name = "وضعیت فعال (نمایشی)")]
        public string IsActiveText => IsActive ? "فعال" : "غیرفعال";

      

        /// <summary>
        /// دریافت نام نمایشی Scope
        /// </summary>
        private string GetScopeDisplayName(FactorScope scope)
        {
            return scope switch
            {
                FactorScope.General_NoHash => "عمومی بدون هشتگ",
                FactorScope.Hash_1_7 => "هشتگ‌دار (کدهای ۱-۷)",
                FactorScope.Hash_8_9 => "هشتگ‌دار (کدهای ۸-۹)",
                FactorScope.Dent_Technical => "دندانپزشکی فنی",
                FactorScope.Dent_Consumables => "مواد دندانپزشکی",
                FactorScope.Prof_NoHash => "حرفه‌ای بدون هشتگ",
                FactorScope.Prof_Hash => "حرفه‌ای هشتگ‌دار",
                FactorScope.Prof_Dental => "حرفه‌ای دندانپزشکی",
                _ => scope.ToString()
            };
        }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static FactorSettingCreateEditViewModel FromEntity(Models.Entities.Clinic.FactorSetting entity)
        {
            if (entity == null) return null;

            return new FactorSettingCreateEditViewModel
            {
                Id = entity.FactorSettingId,
                FactorType = entity.FactorType,
                Scope = entity.Scope,
                IsHashtagged = entity.IsHashtagged,
                Value = entity.Value,
                FinancialYear = entity.FinancialYear,
                IsActive = entity.IsActive,
                Description = entity.Description,
                EffectiveFrom = entity.EffectiveFrom,
                EffectiveTo = entity.EffectiveTo
            };
        }

        /// <summary>
        /// ✅ (Factory Method) یک Entity جدید از روی ViewModel می‌سازد.
        /// </summary>
        public Models.Entities.Clinic.FactorSetting ToEntity()
        {
            return new Models.Entities.Clinic.FactorSetting
            {
                FactorSettingId = Id,
                FactorType = FactorType,
                Scope = Scope,
                IsHashtagged = IsHashtagged,
                Value = Value,
                FinancialYear = FinancialYear,
                IsActive = IsActive,
                Description = Description?.Trim(),
                EffectiveFrom = EffectiveFrom,
                EffectiveTo = EffectiveTo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// ViewModel برای فیلتر کردن کای‌ها
    /// </summary>
    public class FactorSettingFilterViewModel
    {
        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "نوع کای")]
        public ServiceComponentType? FactorType { get; set; }

        [Display(Name = "دامنه کای")]
        public FactorScope? Scope { get; set; }

        [Display(Name = "سال مالی")]
        public int? FinancialYear { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool? IsActive { get; set; }

        [Display(Name = "هشتگ‌دار")]
        public bool? IsHashtagged { get; set; }

        // ویژگی‌های Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // لیست‌های Dropdown
        public List<SelectListItem> FactorTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Scopes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> FinancialYears { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// ViewModel برای نمایش آمار کای‌ها
    /// </summary>
    public class FactorSettingStatisticsViewModel
    {
        [Display(Name = "تعداد کل کای‌ها")]
        public int TotalCount { get; set; }

        [Display(Name = "کای‌های فعال")]
        public int ActiveCount { get; set; }

        [Display(Name = "کای‌های غیرفعال")]
        public int InactiveCount { get; set; }

        [Display(Name = "کای‌های فنی")]
        public int TechnicalCount { get; set; }

        [Display(Name = "کای‌های حرفه‌ای")]
        public int ProfessionalCount { get; set; }

        [Display(Name = "کای‌های هشتگ‌دار")]
        public int HashtaggedCount { get; set; }

        [Display(Name = "کای‌های عادی")]
        public int NormalCount { get; set; }

        [Display(Name = "آخرین بروزرسانی")]
        public DateTime? LastUpdated { get; set; }

        [Display(Name = "آخرین بروزرسانی (شمسی)")]
        public string LastUpdatedShamsi => LastUpdated?.ToPersianDateString();
    }
}
