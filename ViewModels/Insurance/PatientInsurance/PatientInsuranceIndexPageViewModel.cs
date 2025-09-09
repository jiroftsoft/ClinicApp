using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using ClinicApp.ViewModels.Insurance.InsurancePlan;

namespace ClinicApp.ViewModels.Insurance.PatientInsurance
{
    /// <summary>
    /// ViewModel برای صفحه Index بیمه‌های بیماران
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از جستجو و فیلتر
    /// 2. پشتیبانی از صفحه‌بندی
    /// 3. نمایش لیست بیمه‌های بیماران
    /// 4. نمایش آمار کلی
    /// 5. رعایت قراردادهای موجود
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class PatientInsuranceIndexPageViewModel
    {
        #region Search and Filter Properties

        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "ارائه‌دهنده بیمه")]
        public int? ProviderId { get; set; }

        [Display(Name = "طرح بیمه")]
        public int? PlanId { get; set; }

        [Display(Name = "بیمه اصلی")]
        public bool? IsPrimary { get; set; }

        [Display(Name = "وضعیت فعال")]
        public bool? IsActive { get; set; }

        [Display(Name = "تاریخ شروع")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? ToDate { get; set; }

        #endregion

        #region Pagination Properties

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10; // Default page size
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        #endregion

        #region Data Lists

        public List<PatientInsuranceIndexItemViewModel> PatientInsurances { get; set; } = new List<PatientInsuranceIndexItemViewModel>();

        #endregion

        #region Select Lists for Dropdowns

        public List<InsuranceProviderLookupViewModel> InsuranceProviders { get; set; } = new List<InsuranceProviderLookupViewModel>();
        public SelectList InsuranceProviderSelectList { get; set; }

        public List<InsurancePlanLookupViewModel> InsurancePlans { get; set; } = new List<InsurancePlanLookupViewModel>();
        public SelectList InsurancePlanSelectList { get; set; }

        public SelectList PrimaryInsuranceSelectList { get; set; }
        public SelectList ActiveStatusSelectList { get; set; }

        #endregion

        #region Helper Properties

        public bool HasData => PatientInsurances != null && PatientInsurances.Any();
        public bool HasSearchFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                       ProviderId.HasValue || 
                                       PlanId.HasValue || 
                                       IsPrimary.HasValue || 
                                       IsActive.HasValue ||
                                       FromDate.HasValue || 
                                       ToDate.HasValue;

        #endregion

        #region Helper Methods

        /// <summary>
        /// ایجاد SelectList برای وضعیت بیمه اصلی
        /// </summary>
        public static SelectList CreatePrimaryInsuranceSelectList(bool? selectedValue = null)
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه", Selected = !selectedValue.HasValue },
                new SelectListItem { Value = "true", Text = "بیمه اصلی", Selected = selectedValue == true },
                new SelectListItem { Value = "false", Text = "بیمه تکمیلی", Selected = selectedValue == false }
            };

            return new SelectList(items, "Value", "Text", selectedValue?.ToString());
        }

        /// <summary>
        /// ایجاد SelectList برای وضعیت فعال
        /// </summary>
        public static SelectList CreateActiveStatusSelectList(bool? selectedValue = null)
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه", Selected = !selectedValue.HasValue },
                new SelectListItem { Value = "true", Text = "فعال", Selected = selectedValue == true },
                new SelectListItem { Value = "false", Text = "غیرفعال", Selected = selectedValue == false }
            };

            return new SelectList(items, "Value", "Text", selectedValue?.ToString());
        }

        #endregion
    }
}
