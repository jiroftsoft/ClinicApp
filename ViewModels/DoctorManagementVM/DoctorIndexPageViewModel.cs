using System.Collections.Generic;
using System.Web.Mvc;
using ClinicApp.Interfaces;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// ViewModel برای صفحه لیست پزشکان.
    /// این کلاس تمام داده‌های لازم برای نمایش صفحه، از جمله لیست پزشکان و فیلترها را نگهداری می‌کند.
    /// </summary>
    public class DoctorIndexPageViewModel
    {
        /// <summary>
        /// نتیجه صفحه‌بندی شده پزشکان برای نمایش در جدول.
        /// </summary>
        public PagedResult<DoctorIndexViewModel> Doctors { get; set; }

        /// <summary>
        /// مدل جستجو و فیلتر برای فرم‌های جستجو.
        /// </summary>
        public DoctorSearchViewModel SearchModel { get; set; }

        /// <summary>
        /// آمار کلی پزشکان
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد پزشکان فعال
        /// </summary>
        public int ActiveCount { get; set; }

        /// <summary>
        /// تعداد پزشکان غیرفعال
        /// </summary>
        public int InactiveCount { get; set; }

        /// <summary>
        /// تعداد پزشکان اضافه شده امروز
        /// </summary>
        public int TodayCount { get; set; }

        /// <summary>
        /// لیست کلینیک‌ها برای dropdown فیلتر
        /// </summary>
        public SelectList Clinics { get; set; }

        /// <summary>
        /// لیست دپارتمان‌ها برای dropdown فیلتر
        /// </summary>
        public SelectList Departments { get; set; }

        /// <summary>
        /// لیست تخصص‌ها برای dropdown فیلتر
        /// </summary>
        public SelectList Specializations { get; set; }

        public DoctorIndexPageViewModel()
        {
            Doctors = new PagedResult<DoctorIndexViewModel>();
            SearchModel = new DoctorSearchViewModel();
            Clinics = new SelectList(new List<SelectListItem>());
            Departments = new SelectList(new List<SelectListItem>());
            Specializations = new SelectList(new List<SelectListItem>());
        }
    }
}
