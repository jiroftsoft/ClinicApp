using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل نمایش صفحه اصلی مدیریت حذف انتسابات پزشکان
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش آمار کلی حذف انتسابات
    /// 2. فیلترهای جستجو و انتخاب
    /// 3. لیست پزشکان قابل حذف
    /// 4. گزینه‌های حذف دسته‌ای
    /// 
    /// طبق قرارداد APP_PRINCIPLES_CONTRACT:
    /// - استفاده از Strongly-Typed ViewModels
    /// - مقداردهی اولیه properties با default values
    /// - پیاده‌سازی Factory Method Pattern
    /// </summary>
    public class DoctorRemovalIndexViewModel
    {
        /// <summary>
        /// عنوان صفحه
        /// </summary>
        public string PageTitle { get; set; } = "مدیریت حذف انتسابات پزشکان";

        /// <summary>
        /// زیرعنوان صفحه
        /// </summary>
        public string PageSubtitle { get; set; } = "حذف انتسابات پزشکان به دپارتمان‌ها و سرفصل‌های خدماتی";

        /// <summary>
        /// آمار کلی حذف انتسابات
        /// </summary>
        public AssignmentStatsViewModel Statistics { get; set; } = new AssignmentStatsViewModel();

        /// <summary>
        /// زمان آخرین به‌روزرسانی
        /// </summary>
        public string LastRefreshTime { get; set; }

        /// <summary>
        /// آیا داده‌ها بارگذاری شده‌اند
        /// </summary>
        public bool IsDataLoaded { get; set; } = false;

        /// <summary>
        /// آیا در حال بارگذاری است
        /// </summary>
        public bool IsLoading { get; set; } = true;

        /// <summary>
        /// پیام بارگذاری
        /// </summary>
        public string LoadingMessage { get; set; } = "در حال بارگذاری داده‌ها...";

        /// <summary>
        /// فیلترهای جستجو
        /// </summary>
        public DoctorRemovalFiltersViewModel Filters { get; set; } = new DoctorRemovalFiltersViewModel();

        /// <summary>
        /// لیست پزشکان قابل حذف
        /// </summary>
        public List<DoctorRemovalListItem> Doctors { get; set; } = new List<DoctorRemovalListItem>();

        /// <summary>
        /// آیا حذف دسته‌ای فعال است
        /// </summary>
        public bool IsBulkRemovalEnabled { get; set; } = true;

        /// <summary>
        /// حداکثر تعداد پزشکان قابل انتخاب برای حذف دسته‌ای
        /// </summary>
        public int MaxBulkSelectionCount { get; set; } = 50;

        /// <summary>
        /// آیا تأیید حذف دسته‌ای لازم است
        /// </summary>
        public bool RequireBulkConfirmation { get; set; } = true;

        /// <summary>
        /// تعداد کل رکوردها
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهای فیلتر شده
        /// </summary>
        public int FilteredRecords { get; set; }

        /// <summary>
        /// آیا خطا وجود دارد
        /// </summary>
        public bool HasErrors { get; set; }

        /// <summary>
        /// لیست پیام‌های خطا
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new List<string>();

        /// <summary>
        /// Factory Method: ایجاد ViewModel خالی
        /// </summary>
        public static DoctorRemovalIndexViewModel CreateEmpty()
        {
            return new DoctorRemovalIndexViewModel
            {
                PageTitle = "مدیریت حذف انتسابات پزشکان",
                PageSubtitle = "حذف انتسابات پزشکان به دپارتمان‌ها و سرفصل‌های خدماتی",
                Statistics = new AssignmentStatsViewModel(),
                Filters = new DoctorRemovalFiltersViewModel(),
                Doctors = new List<DoctorRemovalListItem>(),
                IsDataLoaded = false,
                IsLoading = true,
                LoadingMessage = "در حال بارگذاری داده‌ها...",
                LastRefreshTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
            };
        }

        /// <summary>
        /// Factory Method: ایجاد ViewModel با داده‌های اولیه
        /// </summary>
        public static DoctorRemovalIndexViewModel CreateWithData(
            AssignmentStatsViewModel statistics,
            List<DoctorRemovalListItem> doctors,
            DoctorRemovalFiltersViewModel filters = null)
        {
            return new DoctorRemovalIndexViewModel
            {
                PageTitle = "مدیریت حذف انتسابات پزشکان",
                PageSubtitle = "حذف انتسابات پزشکان به دپارتمان‌ها و سرفصل‌های خدماتی",
                Statistics = statistics ?? new AssignmentStatsViewModel(),
                Filters = filters ?? new DoctorRemovalFiltersViewModel(),
                Doctors = doctors ?? new List<DoctorRemovalListItem>(),
                IsDataLoaded = true,
                IsLoading = false,
                LoadingMessage = "",
                LastRefreshTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                TotalRecords = doctors?.Count ?? 0,
                FilteredRecords = doctors?.Count ?? 0
            };
        }
    }

    /// <summary>
    /// مدل فیلترهای جستجو برای حذف انتسابات
    /// </summary>
    public class DoctorRemovalFiltersViewModel
    {
        /// <summary>
        /// جستجو بر اساس نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// جستجو بر اساس کد ملی
        /// </summary>
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        /// <summary>
        /// فیلتر بر اساس دپارتمان
        /// </summary>
        [Display(Name = "دپارتمان")]
        public int? DepartmentId { get; set; }

        /// <summary>
        /// لیست دپارتمان‌ها برای dropdown
        /// </summary>
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// فیلتر بر اساس سرفصل خدماتی
        /// </summary>
        [Display(Name = "سرفصل خدماتی")]
        public int? ServiceCategoryId { get; set; }

        /// <summary>
        /// لیست سرفصل‌های خدماتی برای dropdown
        /// </summary>
        public List<SelectListItem> ServiceCategories { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// فیلتر بر اساس تعداد انتسابات
        /// </summary>
        [Display(Name = "حداقل تعداد انتسابات")]
        public int? MinAssignmentsCount { get; set; }

        /// <summary>
        /// فیلتر بر اساس تاریخ آخرین انتساب (از)
        /// </summary>
        [Display(Name = "تاریخ انتساب از")]
        public DateTime? LastAssignmentDateFrom { get; set; }

        /// <summary>
        /// فیلتر بر اساس تاریخ آخرین انتساب (تا)
        /// </summary>
        [Display(Name = "تاریخ انتساب تا")]
        public DateTime? LastAssignmentDateTo { get; set; }

        /// <summary>
        /// آیا فقط پزشکان با انتسابات فعال نمایش داده شوند
        /// </summary>
        [Display(Name = "فقط انتسابات فعال")]
        public bool ShowOnlyActiveAssignments { get; set; } = true;

        /// <summary>
        /// آیا فقط پزشکان بدون وابستگی نمایش داده شوند
        /// </summary>
        [Display(Name = "فقط بدون وابستگی")]
        public bool ShowOnlyWithoutDependencies { get; set; } = false;
    }

    /// <summary>
    /// مدل آیتم لیست پزشکان قابل حذف
    /// </summary>
    public class DoctorRemovalListItem
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// کد ملی پزشک
        /// </summary>
        public string NationalCode { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        public string Specialization { get; set; }

        /// <summary>
        /// تعداد کل انتسابات
        /// </summary>
        public int TotalAssignments { get; set; }

        /// <summary>
        /// تعداد انتسابات فعال
        /// </summary>
        public int ActiveAssignments { get; set; }

        /// <summary>
        /// تعداد انتسابات غیرفعال
        /// </summary>
        public int InactiveAssignments { get; set; }

        /// <summary>
        /// لیست دپارتمان‌های انتساب شده
        /// </summary>
        public List<DepartmentAssignment> Departments { get; set; } = new List<DepartmentAssignment>();

        /// <summary>
        /// لیست سرفصل‌های خدماتی انتساب شده
        /// </summary>
        public List<ServiceCategoryAssignment> ServiceCategories { get; set; } = new List<ServiceCategoryAssignment>();

        /// <summary>
        /// تاریخ آخرین انتساب
        /// </summary>
        public string LastAssignmentDate { get; set; }

        /// <summary>
        /// آیا پزشک وابستگی دارد
        /// </summary>
        public bool HasDependencies { get; set; }

        /// <summary>
        /// لیست وابستگی‌ها
        /// </summary>
        public List<string> Dependencies { get; set; } = new List<string>();

        /// <summary>
        /// آیا قابل حذف است
        /// </summary>
        public bool CanBeRemoved { get; set; }

        /// <summary>
        /// دلیل عدم امکان حذف
        /// </summary>
        public string RemovalBlockReason { get; set; }

        /// <summary>
        /// آیا انتخاب شده برای حذف دسته‌ای
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// متن وضعیت
        /// </summary>
        public string StatusText => GetStatusText();

        /// <summary>
        /// کلاس CSS برای badge وضعیت
        /// </summary>
        public string StatusBadgeClass => GetStatusBadgeClass();

        /// <summary>
        /// تاریخ فرمت شده آخرین انتساب
        /// </summary>
        public string FormattedLastAssignmentDate => FormatDate(LastAssignmentDate);

        private string GetStatusText()
        {
            if (HasDependencies)
                return "وابستگی دارد";
            if (ActiveAssignments > 0)
                return "انتسابات فعال";
            if (InactiveAssignments > 0)
                return "انتسابات غیرفعال";
            return "بدون انتساب";
        }

        private string GetStatusBadgeClass()
        {
            if (HasDependencies)
                return "bg-danger";
            if (ActiveAssignments > 0)
                return "bg-success";
            if (InactiveAssignments > 0)
                return "bg-warning";
            return "bg-secondary";
        }

        private static string FormatDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return "-";
            
            if (DateTime.TryParse(dateString, out var date))
            {
                return date.ToString("yyyy/MM/dd HH:mm");
            }
            
            return dateString;
        }
    }

 

   
}