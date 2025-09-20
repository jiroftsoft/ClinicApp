using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;

namespace ClinicApp.ViewModels.Insurance.InsuranceCalculation
{
    /// <summary>
    /// ViewModel برای صفحه Index محاسبات بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از جستجو و فیلتر
    /// 2. پشتیبانی از صفحه‌بندی
    /// 3. نمایش لیست بیماران، خدمات و طرح‌های بیمه
    /// 4. نمایش آمار کلی
    /// 5. رعایت قراردادهای موجود
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsuranceCalculationIndexPageViewModel
    {
        #region Search and Filter Properties

        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "بیمار")]
        public int? PatientId { get; set; }

        [Display(Name = "خدمت")]
        public int? ServiceId { get; set; }

        [Display(Name = "طرح بیمه")]
        public int? PlanId { get; set; }

        [Display(Name = "وضعیت اعتبار")]
        public bool? IsValid { get; set; }

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

        public List<InsuranceCalculationIndexViewModel> InsuranceCalculations { get; set; } = new List<InsuranceCalculationIndexViewModel>();

        #endregion

        #region Select Lists for Dropdowns

        public List<PatientLookupViewModel> Patients { get; set; } = new List<PatientLookupViewModel>();
        public SelectList PatientSelectList { get; set; }

        public List<ServiceLookupViewModel> Services { get; set; } = new List<ServiceLookupViewModel>();
        public SelectList ServiceSelectList { get; set; }

        public List<InsurancePlanLookupViewModel> InsurancePlans { get; set; } = new List<InsurancePlanLookupViewModel>();
        public SelectList InsurancePlanSelectList { get; set; }

        public SelectList ValiditySelectList { get; set; }

        #endregion

        #region Helper Properties

        public bool HasData => InsuranceCalculations != null && InsuranceCalculations.Any();
        public bool HasSearchFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                       PatientId.HasValue || 
                                       ServiceId.HasValue || 
                                       PlanId.HasValue || 
                                       IsValid.HasValue || 
                                       FromDate.HasValue || 
                                       ToDate.HasValue;

        #endregion

        #region Helper Methods

        /// <summary>
        /// ایجاد SelectList برای بیماران
        /// </summary>
        public void CreatePatientSelectList()
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه بیماران", Selected = !PatientId.HasValue }
            };

            foreach (var patient in Patients)
            {
                items.Add(new SelectListItem
                {
                    Value = patient.PatientId.ToString(),
                    Text = $"{patient.FullName} ({patient.NationalCode})",
                    Selected = PatientId == patient.PatientId
                });
            }

            PatientSelectList = new SelectList(items, "Value", "Text", PatientId?.ToString());
        }

        /// <summary>
        /// ایجاد SelectList برای خدمات
        /// </summary>
        public void CreateServiceSelectList()
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه خدمات", Selected = !ServiceId.HasValue }
            };

            foreach (var service in Services)
            {
                items.Add(new SelectListItem
                {
                    Value = service.ServiceId.ToString(),
                    Text = service.Title,
                    Selected = ServiceId == service.ServiceId
                });
            }

            ServiceSelectList = new SelectList(items, "Value", "Text", ServiceId?.ToString());
        }

        /// <summary>
        /// ایجاد SelectList برای طرح‌های بیمه
        /// </summary>
        public void CreateInsurancePlanSelectList()
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه طرح‌های بیمه", Selected = !PlanId.HasValue }
            };

            foreach (var plan in InsurancePlans)
            {
                items.Add(new SelectListItem
                {
                    Value = plan.InsurancePlanId.ToString(),
                    Text = $"{plan.Name} ({plan.PlanCode})",
                    Selected = PlanId == plan.InsurancePlanId
                });
            }

            InsurancePlanSelectList = new SelectList(items, "Value", "Text", PlanId?.ToString());
        }

        /// <summary>
        /// ایجاد SelectList برای وضعیت اعتبار
        /// </summary>
        public void CreateValiditySelectList()
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "همه وضعیت‌ها", Selected = !IsValid.HasValue },
                new SelectListItem { Value = "true", Text = "معتبر", Selected = IsValid == true },
                new SelectListItem { Value = "false", Text = "نامعتبر", Selected = IsValid == false }
            };
            ValiditySelectList = new SelectList(items, "Value", "Text", IsValid?.ToString().ToLower());
        }

        #endregion
    }

    #region Lookup ViewModels

    /// <summary>
    /// ViewModel برای نمایش اطلاعات بیمار در Dropdown
    /// </summary>
    public class PatientLookupViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public string NationalCode { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش اطلاعات خدمت در Dropdown
    /// </summary>
    public class ServiceLookupViewModel
    {
        public int ServiceId { get; set; }
        public string Title { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceCategoryName { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش اطلاعات طرح بیمه در Dropdown
    /// </summary>
    public class InsurancePlanLookupViewModel
    {
        public int InsurancePlanId { get; set; }
        public string Name { get; set; }
        public string PlanCode { get; set; }
        public string InsuranceProviderName { get; set; }
    }

    #endregion
}
