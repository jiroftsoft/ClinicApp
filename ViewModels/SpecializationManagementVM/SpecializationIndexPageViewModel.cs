using System.Collections.Generic;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels;
using ClinicApp.Helpers;

namespace ClinicApp.ViewModels.SpecializationManagementVM
{
    /// <summary>
    /// مدل صفحه Index تخصص‌ها
    /// </summary>
    public class SpecializationIndexPageViewModel
    {
        /// <summary>
        /// لیست تخصص‌ها
        /// </summary>
        public List<SpecializationIndexViewModel> Specializations { get; set; } = new List<SpecializationIndexViewModel>();

        /// <summary>
        /// مدل جستجو
        /// </summary>
        public SpecializationSearchViewModel SearchModel { get; set; } = new SpecializationSearchViewModel();

        /// <summary>
        /// آمار کلی
        /// </summary>
        public SpecializationStatisticsViewModel Statistics { get; set; } = new SpecializationStatisticsViewModel();

        /// <summary>
        /// اطلاعات صفحه‌بندی
        /// </summary>
        public PagedResult<SpecializationIndexViewModel> PagedResult { get; set; }
    }

    /// <summary>
    /// مدل آمار تخصص‌ها
    /// </summary>
    public class SpecializationStatisticsViewModel
    {
        /// <summary>
        /// تعداد کل تخصص‌ها
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد تخصص‌های فعال
        /// </summary>
        public int ActiveCount { get; set; }

        /// <summary>
        /// تعداد تخصص‌های غیرفعال
        /// </summary>
        public int InactiveCount { get; set; }

        /// <summary>
        /// تعداد تخصص‌های حذف شده
        /// </summary>
        public int DeletedCount { get; set; }

        /// <summary>
        /// تعداد تخصص‌های ایجاد شده امروز
        /// </summary>
        public int TodayCount { get; set; }
    }
}
