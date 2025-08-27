using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.SpecializationManagementVM
{
    /// <summary>
    /// مدل جستجو و فیلتر برای تخصص‌ها
    /// </summary>
    public class SpecializationSearchViewModel
    {
        /// <summary>
        /// عبارت جستجو
        /// </summary>
        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        /// <summary>
        /// فیلتر بر اساس وضعیت فعال/غیرفعال
        /// </summary>
        [Display(Name = "وضعیت")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// شماره صفحه
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// تعداد آیتم در هر صفحه
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// فیلد مرتب‌سازی
        /// </summary>
        [Display(Name = "مرتب‌سازی بر اساس")]
        public string SortBy { get; set; } = "DisplayOrder";

        /// <summary>
        /// ترتیب مرتب‌سازی (asc/desc)
        /// </summary>
        [Display(Name = "ترتیب")]
        public string SortOrder { get; set; } = "asc";

        /// <summary>
        /// فیلتر بر اساس تعداد پزشکان
        /// </summary>
        [Display(Name = "حداقل تعداد پزشکان")]
        public int? MinDoctorCount { get; set; }

        /// <summary>
        /// فیلتر بر اساس تاریخ ایجاد
        /// </summary>
        [Display(Name = "از تاریخ")]
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// فیلتر بر اساس تاریخ ایجاد
        /// </summary>
        [Display(Name = "تا تاریخ")]
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// نمایش تخصص‌های حذف شده
        /// </summary>
        [Display(Name = "نمایش حذف شده‌ها")]
        public bool IncludeDeleted { get; set; } = false;

        /// <summary>
        /// تنظیم مقادیر پیش‌فرض
        /// </summary>
        public SpecializationSearchViewModel()
        {
            PageNumber = 1;
            PageSize = 10;
            SortBy = "DisplayOrder";
            SortOrder = "asc";
            IncludeDeleted = false;
        }
    }
}
