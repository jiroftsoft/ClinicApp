using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای پارامترهای جستجوی بیماران
    /// </summary>
    public class SearchParameterViewModel
    {
        /// <summary>
        /// کد ملی
        /// </summary>
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        /// <summary>
        /// نام
        /// </summary>
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// تاریخ تولد از
        /// </summary>
        [Display(Name = "تاریخ تولد از")]
        public DateTime? BirthDateFrom { get; set; }

        /// <summary>
        /// تاریخ تولد تا
        /// </summary>
        [Display(Name = "تاریخ تولد تا")]
        public DateTime? BirthDateTo { get; set; }

        /// <summary>
        /// جنسیت
        /// </summary>
        [Display(Name = "جنسیت")]
        public string Gender { get; set; }

        /// <summary>
        /// وضعیت فعال
        /// </summary>
        [Display(Name = "وضعیت")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// تعداد نتایج در هر صفحه
        /// </summary>
        [Display(Name = "تعداد در صفحه")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// شماره صفحه
        /// </summary>
        [Display(Name = "شماره صفحه")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// فیلد مرتب‌سازی
        /// </summary>
        [Display(Name = "مرتب‌سازی بر اساس")]
        public string SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// جهت مرتب‌سازی (asc/desc)
        /// </summary>
        [Display(Name = "جهت مرتب‌سازی")]
        public string SortDirection { get; set; } = "desc";
    }
}
