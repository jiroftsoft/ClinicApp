using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Extensions;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;

namespace ClinicApp.ViewModels.Insurance.InsurancePlan
{
    /// <summary>
    /// ViewModel برای صفحه Index طرح‌های بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از جستجو و فیلتر
    /// 2. پشتیبانی از صفحه‌بندی
    /// 3. نمایش لیست ارائه‌دهندگان بیمه
    /// 4. نمایش آمار کلی
    /// 5. رعایت قراردادهای موجود
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsurancePlanIndexPageViewModel
    {
        // Search and Filter Properties
        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "ارائه‌دهنده بیمه")]
        public int? ProviderId { get; set; }

        [Display(Name = "وضعیت")]
        public bool? IsActive { get; set; }

        // Pagination Properties
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);

        // Data Properties
        public List<InsurancePlanIndexViewModel> InsurancePlans { get; set; } = new List<InsurancePlanIndexViewModel>();
        public List<InsuranceProviderLookupViewModel> InsuranceProviders { get; set; } = new List<InsuranceProviderLookupViewModel>();

        // Statistics Properties
        public int ActivePlansCount { get; set; }
        public int InactivePlansCount { get; set; }
        public int TotalPlansCount { get; set; }

        // Select Lists for Dropdowns
        public SelectList InsuranceProviderSelectList { get; set; }
        public SelectList StatusSelectList { get; set; }

        /// <summary>
        /// ایجاد SelectList برای ارائه‌دهندگان بیمه
        /// </summary>
        public void CreateInsuranceProviderSelectList()
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه ارائه‌دهندگان", Selected = !ProviderId.HasValue }
            };

            foreach (var provider in InsuranceProviders)
            {
                items.Add(new SelectListItem
                {
                    Value = provider.InsuranceProviderId.ToString(),
                    Text = provider.Name,
                    Selected = ProviderId == provider.InsuranceProviderId
                });
            }

            InsuranceProviderSelectList = new SelectList(items, "Value", "Text", ProviderId?.ToString());
        }

        /// <summary>
        /// ایجاد SelectList برای وضعیت
        /// </summary>
        public void CreateStatusSelectList()
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه وضعیت‌ها", Selected = !IsActive.HasValue },
                new SelectListItem { Value = "true", Text = "فعال", Selected = IsActive == true },
                new SelectListItem { Value = "false", Text = "غیرفعال", Selected = IsActive == false }
            };

            StatusSelectList = new SelectList(items, "Value", "Text", IsActive?.ToString());
        }

        /// <summary>
        /// بررسی وجود نتایج جستجو
        /// </summary>
        public bool HasSearchResults => !string.IsNullOrEmpty(SearchTerm) || ProviderId.HasValue || IsActive.HasValue;

        /// <summary>
        /// بررسی وجود داده
        /// </summary>
        public bool HasData => InsurancePlans != null && InsurancePlans.Count > 0;
    }
}
