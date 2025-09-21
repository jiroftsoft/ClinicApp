using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای جزئیات تعرفه بیمه تکمیلی - Production Optimized
    /// طراحی شده برای محیط درمانی کلینیک شفا
    /// 
    /// ویژگی‌های بهینه‌سازی:
    /// 1. Calculated Properties برای نمایش بهتر
    /// 2. Validation Attributes
    /// 3. Factory Methods برای تبدیل Entity
    /// 4. Performance Optimized Properties
    /// </summary>
    public class SupplementaryTariffDetailsViewModel
    {
        /// <summary>
        /// شناسه تعرفه بیمه تکمیلی
        /// </summary>
        public int InsuranceTariffId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        [Display(Name = "نام خدمت")]
        public string ServiceTitle { get; set; }

        /// <summary>
        /// کد خدمت
        /// </summary>
        [Display(Name = "کد خدمت")]
        public string ServiceCode { get; set; }

        /// <summary>
        /// توضیحات خدمت
        /// </summary>
        [Display(Name = "توضیحات خدمت")]
        public string ServiceDescription { get; set; }

        /// <summary>
        /// شناسه سرفصل خدمت
        /// </summary>
        public int ServiceCategoryId { get; set; }

        /// <summary>
        /// نام سرفصل خدمت
        /// </summary>
        [Display(Name = "سرفصل خدمت")]
        public string ServiceCategoryTitle { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        [Display(Name = "دپارتمان")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// شناسه طرح بیمه
        /// </summary>
        public int InsurancePlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه
        /// </summary>
        [Display(Name = "طرح بیمه")]
        public string InsurancePlanName { get; set; }

        /// <summary>
        /// کد طرح بیمه
        /// </summary>
        [Display(Name = "کد طرح بیمه")]
        public string InsurancePlanCode { get; set; }

        /// <summary>
        /// شناسه ارائه‌دهنده بیمه
        /// </summary>
        public int InsuranceProviderId { get; set; }

        /// <summary>
        /// نام ارائه‌دهنده بیمه
        /// </summary>
        [Display(Name = "ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        /// <summary>
        /// قیمت تعرفه
        /// </summary>
        [Display(Name = "قیمت تعرفه")]
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
        public decimal? TariffPrice { get; set; }

        /// <summary>
        /// سهم بیمار
        /// </summary>
        [Display(Name = "سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
        public decimal? PatientShare { get; set; }

        /// <summary>
        /// سهم بیمه
        /// </summary>
        [Display(Name = "سهم بیمه")]
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
        public decimal? InsurerShare { get; set; }

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "درصد پوشش تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal? SupplementaryCoveragePercent { get; set; }

        /// <summary>
        /// سقف پرداخت بیمه تکمیلی
        /// </summary>
        [Display(Name = "سقف پرداخت تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
        public decimal? SupplementaryMaxPayment { get; set; }

        /// <summary>
        /// تنظیمات خاص بیمه تکمیلی (JSON)
        /// </summary>
        [Display(Name = "تنظیمات خاص")]
        public string SupplementarySettings { get; set; }

        /// <summary>
        /// اولویت تعرفه
        /// </summary>
        [Display(Name = "اولویت")]
        public int? Priority { get; set; }

        /// <summary>
        /// تاریخ شروع اعتبار
        /// </summary>
        [Display(Name = "تاریخ شروع اعتبار")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار
        /// </summary>
        [Display(Name = "تاریخ پایان اعتبار")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        [Display(Name = "وضعیت فعال")]
        public bool IsActive { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// نام کاربر ایجادکننده
        /// </summary>
        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        [Display(Name = "آخرین به‌روزرسانی")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// نام کاربر آخرین به‌روزرسانی‌کننده
        /// </summary>
        [Display(Name = "به‌روزرسانی‌کننده")]
        public string UpdatedByUserName { get; set; }

        /// <summary>
        /// آیا تعرفه منقضی شده است؟
        /// </summary>
        public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.Now;

        /// <summary>
        /// آیا تعرفه در آینده فعال می‌شود؟
        /// </summary>
        public bool IsFuture => StartDate.HasValue && StartDate.Value > DateTime.Now;

        /// <summary>
        /// آیا تعرفه در حال حاضر معتبر است؟
        /// </summary>
        public bool IsCurrentlyValid => IsActive && !IsExpired && !IsFuture;

        /// <summary>
        /// وضعیت تعرفه (فعال/غیرفعال/منقضی/آینده)
        /// </summary>
        public string StatusText
        {
            get
            {
                if (!IsActive) return "غیرفعال";
                if (IsExpired) return "منقضی";
                if (IsFuture) return "آینده";
                return "فعال";
            }
        }

        /// <summary>
        /// کلاس CSS برای وضعیت
        /// </summary>
        public string StatusCssClass
        {
            get
            {
                if (!IsActive) return "badge-secondary";
                if (IsExpired) return "badge-danger";
                if (IsFuture) return "badge-warning";
                return "badge-success";
            }
        }

        /// <summary>
        /// درصد پوشش کل (اصلی + تکمیلی)
        /// </summary>
        [Display(Name = "درصد پوشش کل")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal TotalCoveragePercent
        {
            get
            {
                if (TariffPrice.HasValue && TariffPrice.Value > 0)
                {
                    var primaryPercent = InsurerShare.HasValue ? (InsurerShare.Value / TariffPrice.Value) * 100 : 0;
                    var supplementaryPercent = SupplementaryCoveragePercent ?? 0;
                    return Math.Min(primaryPercent + supplementaryPercent, 100);
                }
                return 0;
            }
        }

        /// <summary>
        /// مبلغ باقی‌مانده پس از بیمه اصلی
        /// </summary>
        [Display(Name = "مبلغ باقی‌مانده")]
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
        public decimal RemainingAmountAfterPrimary
        {
            get
            {
                if (TariffPrice.HasValue && InsurerShare.HasValue)
                {
                    return Math.Max(0, TariffPrice.Value - InsurerShare.Value);
                }
                return TariffPrice ?? 0;
            }
        }

        /// <summary>
        /// آیا بیمه تکمیلی اعمال شده است؟
        /// </summary>
        [Display(Name = "بیمه تکمیلی اعمال شده")]
        public bool IsSupplementaryApplied => SupplementaryCoveragePercent.HasValue && SupplementaryCoveragePercent.Value > 0;

        /// <summary>
        /// مبلغ پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "پوشش تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
        public decimal SupplementaryCoverageAmount
        {
            get
            {
                if (SupplementaryCoveragePercent.HasValue && SupplementaryCoveragePercent.Value > 0)
                {
                    var remainingAmount = RemainingAmountAfterPrimary;
                    return (remainingAmount * SupplementaryCoveragePercent.Value) / 100;
                }
                return 0;
            }
        }

        /// <summary>
        /// سهم نهایی بیمار
        /// </summary>
        [Display(Name = "سهم نهایی بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
        public decimal FinalPatientShare
        {
            get
            {
                var remainingAmount = RemainingAmountAfterPrimary;
                var supplementaryCoverage = SupplementaryCoverageAmount;
                return Math.Max(0, remainingAmount - supplementaryCoverage);
            }
        }

        #region Factory Methods

        /// <summary>
        /// ایجاد ViewModel از Entity - Production Optimized
        /// </summary>
        public static SupplementaryTariffDetailsViewModel FromEntity(Models.Entities.Insurance.InsuranceTariff entity)
        {
            if (entity == null) return null;

            return new SupplementaryTariffDetailsViewModel
            {
                InsuranceTariffId = entity.InsuranceTariffId,
                ServiceId = entity.ServiceId,
                ServiceTitle = entity.Service?.Title,
                ServiceCode = entity.Service?.ServiceCode,
                ServiceDescription = entity.Service?.Description,
                ServiceCategoryId = entity.Service?.ServiceCategoryId ?? 0,
                ServiceCategoryTitle = entity.Service?.ServiceCategory?.Title,
                InsurancePlanId = entity.InsurancePlanId ?? 0,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsurancePlanCode = entity.InsurancePlan?.PlanCode,
                InsuranceProviderId = entity.InsurancePlan?.InsuranceProviderId ?? 0,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name,
                TariffPrice = entity.TariffPrice,
                PatientShare = entity.PatientShare,
                InsurerShare = entity.InsurerShare,
                SupplementaryCoveragePercent = entity.SupplementaryCoveragePercent,
                SupplementaryMaxPayment = entity.SupplementaryMaxPayment,
                Priority = entity.Priority,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedByUserName = entity.CreatedByUser?.UserName,
                UpdatedAt = entity.UpdatedAt,
                UpdatedByUserName = entity.UpdatedByUser?.UserName
            };
        }

        /// <summary>
        /// ایجاد ViewModel از Entity با Service و Plan
        /// </summary>
        public static SupplementaryTariffDetailsViewModel FromEntityWithDetails(
            Models.Entities.Insurance.InsuranceTariff entity, 
            Models.Entities.Clinic.Service service, 
            Models.Entities.Insurance.InsurancePlan plan)
        {
            if (entity == null) return null;

            var viewModel = FromEntity(entity);
            
            if (service != null)
            {
                viewModel.ServiceTitle = service.Title;
                viewModel.ServiceCode = service.ServiceCode;
                viewModel.ServiceDescription = service.Description;
                viewModel.ServiceCategoryId = service.ServiceCategoryId;
                viewModel.ServiceCategoryTitle = service.ServiceCategory?.Title;
            }

            if (plan != null)
            {
                viewModel.InsurancePlanName = plan.Name;
                viewModel.InsurancePlanCode = plan.PlanCode;
                viewModel.InsuranceProviderId = plan.InsuranceProviderId;
                viewModel.InsuranceProviderName = plan.InsuranceProvider?.Name;
            }

            return viewModel;
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// اعتبارسنجی ViewModel
        /// </summary>
        public bool IsValid()
        {
            return InsuranceTariffId > 0 && 
                   ServiceId > 0 && 
                   !string.IsNullOrEmpty(ServiceTitle) &&
                   TariffPrice.HasValue && 
                   TariffPrice.Value > 0;
        }

        /// <summary>
        /// دریافت پیام‌های خطا
        /// </summary>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (InsuranceTariffId <= 0)
                errors.Add("شناسه تعرفه نامعتبر است");

            if (ServiceId <= 0)
                errors.Add("شناسه خدمت نامعتبر است");

            if (string.IsNullOrEmpty(ServiceTitle))
                errors.Add("نام خدمت الزامی است");

            if (!TariffPrice.HasValue || TariffPrice.Value <= 0)
                errors.Add("قیمت تعرفه باید بزرگتر از صفر باشد");

            return errors;
        }

        #endregion
    }
}
