using System.Collections.Generic;
using ClinicApp.ViewModels.Insurance.PatientInsurance;

namespace ClinicApp.ViewModels.Insurance.PatientInsurance
{
    /// <summary>
    /// ViewModel برای Partial View لیست بیمه‌های بیماران
    /// </summary>
    public class PatientInsuranceListPartialViewModel
    {
        /// <summary>
        /// لیست بیمه‌های بیماران
        /// </summary>
        public List<PatientInsuranceIndexItemViewModel> Items { get; set; } = new List<PatientInsuranceIndexItemViewModel>();

        /// <summary>
        /// شماره صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// تعداد آیتم‌ها در هر صفحه
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// تعداد کل آیتم‌ها
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages => (int)System.Math.Ceiling((double)TotalItems / PageSize);

        /// <summary>
        /// آیا صفحه قبلی وجود دارد؟
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// آیا صفحه بعدی وجود دارد؟
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// شماره صفحه قبلی
        /// </summary>
        public int PreviousPage => CurrentPage - 1;

        /// <summary>
        /// شماره صفحه بعدی
        /// </summary>
        public int NextPage => CurrentPage + 1;
    }
}
