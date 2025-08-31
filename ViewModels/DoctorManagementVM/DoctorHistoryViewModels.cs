using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Web.Mvc;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// ViewModel اصلی برای صفحه Index تاریخچه پزشکان
    /// </summary>
    public class DoctorHistoryIndexViewModel
    {
        /// <summary>
        /// لیست تاریخچه‌ها
        /// </summary>
        public List<AssignmentHistoryViewModel> Histories { get; set; } = new List<AssignmentHistoryViewModel>();

        /// <summary>
        /// آمار کلی
        /// </summary>
        public AssignmentHistoryStatisticsViewModel Statistics { get; set; } = new AssignmentHistoryStatisticsViewModel();

        /// <summary>
        /// صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// اندازه صفحه
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages { get; set; } = 1;

        /// <summary>
        /// فیلتر شناسه پزشک
        /// </summary>
        public int? FilterDoctorId { get; set; }

        /// <summary>
        /// فیلتر نوع عملیات
        /// </summary>
        public string FilterActionType { get; set; }

        /// <summary>
        /// فیلتر تاریخ شروع
        /// </summary>
        public DateTime? FilterStartDate { get; set; }

        /// <summary>
        /// فیلتر تاریخ پایان
        /// </summary>
        public DateTime? FilterEndDate { get; set; }

        /// <summary>
        /// گزینه‌های نوع عملیات
        /// </summary>
        public System.Web.Mvc.SelectListItem[] ActionTypes { get; set; } = new System.Web.Mvc.SelectListItem[0];
        
        // Properties for views
        public DoctorHistoryFilterViewModel Filter { get; set; } = new DoctorHistoryFilterViewModel();
        public List<AssignmentHistoryViewModel> HistoryItems { get; set; } = new List<AssignmentHistoryViewModel>();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();

        /// <summary>
        /// لیست پزشکان برای dropdown
        /// </summary>
        public List<System.Web.Mvc.SelectListItem> Doctors { get; set; } = new List<System.Web.Mvc.SelectListItem>();

        /// <summary>
        /// لیست دپارتمان‌ها برای dropdown
        /// </summary>
        public List<System.Web.Mvc.SelectListItem> Departments { get; set; } = new List<System.Web.Mvc.SelectListItem>();
    }

    /// <summary>
    /// ViewModel برای جستجوی پیشرفته تاریخچه
    /// </summary>
    public class DoctorHistorySearchViewModel
    {
        /// <summary>
        /// عبارت جستجو
        /// </summary>
        [Display(Name = "عبارت جستجو")]
        public string SearchTerm { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Display(Name = "پزشک")]
        public int? DoctorId { get; set; }

        /// <summary>
        /// نوع عملیات
        /// </summary>
        [Display(Name = "نوع عملیات")]
        public string ActionType { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// انجام شده توسط
        /// </summary>
        [Display(Name = "انجام شده توسط")]
        public string PerformedBy { get; set; }

        /// <summary>
        /// صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// اندازه صفحه
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// لیست نتایج جستجو
        /// </summary>
        public List<AssignmentHistoryViewModel> Histories { get; set; } = new List<AssignmentHistoryViewModel>();

        /// <summary>
        /// گزینه‌های نوع عملیات
        /// </summary>
        public System.Web.Mvc.SelectListItem[] ActionTypes { get; set; } = new System.Web.Mvc.SelectListItem[0];

        /// <summary>
        /// نتایج جستجو (برای view)
        /// </summary>
        public List<AssignmentHistoryViewModel> SearchResults { get; set; } = new List<AssignmentHistoryViewModel>();

        /// <summary>
        /// فیلترهای جستجو (برای view)
        /// </summary>
        public DoctorHistoryFilterViewModel Filter { get; set; } = new DoctorHistoryFilterViewModel();

        /// <summary>
        /// لیست پزشکان برای dropdown
        /// </summary>
        public List<System.Web.Mvc.SelectListItem> Doctors { get; set; } = new List<System.Web.Mvc.SelectListItem>();

        /// <summary>
        /// لیست دپارتمان‌ها برای dropdown
        /// </summary>
        public List<System.Web.Mvc.SelectListItem> Departments { get; set; } = new List<System.Web.Mvc.SelectListItem>();
    }

    /// <summary>
    /// ViewModel برای فیلتر تاریخچه
    /// </summary>
    public class DoctorHistoryFilterViewModel
    {
        /// <summary>
        /// عبارت جستجو
        /// </summary>
        [Display(Name = "عبارت جستجو")]
        public string SearchTerm { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Display(Name = "پزشک")]
        public int? DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// نوع عملیات
        /// </summary>
        [Display(Name = "نوع عملیات")]
        public string ActionType { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        [Display(Name = "دپارتمان")]
        public int? DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// انجام شده توسط
        /// </summary>
        [Display(Name = "انجام شده توسط")]
        public string PerformedBy { get; set; }

        /// <summary>
        /// سطح اهمیت
        /// </summary>
        [Display(Name = "سطح اهمیت")]
        public string Importance { get; set; }
    }

    /// <summary>
    /// ViewModel برای گزارش تاریخچه پزشک
    /// </summary>
    public class DoctorHistoryReportViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// کد ملی پزشک
        /// </summary>
        public string DoctorNationalCode { get; set; }

        /// <summary>
        /// تاریخ شروع گزارش
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان گزارش
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// تعداد کل عملیات
        /// </summary>
        public int TotalOperations { get; set; }

        /// <summary>
        /// تعداد کل رکوردها
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// تعداد انتسابات
        /// </summary>
        public int AssignmentsCount { get; set; }

        /// <summary>
        /// تعداد انتسابات (برای view)
        /// </summary>
        public int AssignmentCount { get; set; }

        /// <summary>
        /// تعداد انتقالات
        /// </summary>
        public int TransfersCount { get; set; }

        /// <summary>
        /// تعداد حذف‌ها
        /// </summary>
        public int RemovalsCount { get; set; }

        /// <summary>
        /// تعداد ویرایش‌ها
        /// </summary>
        public int UpdatesCount { get; set; }

        /// <summary>
        /// تعداد رکوردهای بحرانی
        /// </summary>
        public int CriticalRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهای مهم
        /// </summary>
        public int ImportantRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهای عادی
        /// </summary>
        public int NormalRecords { get; set; }

        /// <summary>
        /// تعداد حذف انتساب
        /// </summary>
        public int RemovalCount { get; set; }

        /// <summary>
        /// تعداد بروزرسانی
        /// </summary>
        public int UpdateCount { get; set; }

        /// <summary>
        /// لیست عملیات
        /// </summary>
        public List<AssignmentHistoryViewModel> Operations { get; set; } = new List<AssignmentHistoryViewModel>();

        /// <summary>
        /// لیست آیتم‌های تاریخچه (برای view)
        /// </summary>
        public List<AssignmentHistoryViewModel> HistoryItems { get; set; } = new List<AssignmentHistoryViewModel>();

        /// <summary>
        /// اطلاعات پزشک (برای view)
        /// </summary>
        public DoctorInfoViewModel Doctor { get; set; } = new DoctorInfoViewModel();

        /// <summary>
        /// تاریخ تولید گزارش
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// تولید شده توسط
        /// </summary>
        public string GeneratedBy { get; set; }
    }

    /// <summary>
    /// ViewModel برای اطلاعات پزشک در گزارش
    /// </summary>
    public class DoctorInfoViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        public string Specialization { get; set; }

        /// <summary>
        /// نام کلینیک
        /// </summary>
        public string ClinicName { get; set; }

        /// <summary>
        /// آیا فعال است
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// ViewModel برای نگهداری تاریخچه
    /// </summary>
    public class DoctorHistoryMaintenanceViewModel
    {
        /// <summary>
        /// تعداد کل رکوردها
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهای قدیمی (بیش از 1 سال)
        /// </summary>
        public int OldRecordsCount { get; set; }

        /// <summary>
        /// تعداد رکوردهای خیلی قدیمی (بیش از 2 سال)
        /// </summary>
        public int VeryOldRecordsCount { get; set; }

        /// <summary>
        /// حجم تخمینی دیتابیس (MB)
        /// </summary>
        public decimal EstimatedDatabaseSize { get; set; }

        /// <summary>
        /// آخرین پاکسازی
        /// </summary>
        public DateTime? LastCleanup { get; set; }

        /// <summary>
        /// تعداد روزهای حفظ رکوردها
        /// </summary>
        [Display(Name = "تعداد روزهای حفظ رکوردها")]
        [Range(30, 3650, ErrorMessage = "تعداد روزها باید بین 30 تا 3650 باشد")]
        public int DaysToKeep { get; set; } = 365;

        /// <summary>
        /// آیا پاکسازی خودکار فعال است
        /// </summary>
        [Display(Name = "پاکسازی خودکار فعال")]
        public bool AutoCleanupEnabled { get; set; } = false;

        /// <summary>
        /// فاصله پاکسازی خودکار (روز)
        /// </summary>
        [Display(Name = "فاصله پاکسازی خودکار (روز)")]
        [Range(7, 90, ErrorMessage = "فاصله پاکسازی باید بین 7 تا 90 روز باشد")]
        public int AutoCleanupInterval { get; set; } = 30;
    }

    /// <summary>
    /// ViewModel برای آمار پیشرفته تاریخچه
    /// </summary>
    public class DoctorHistoryAdvancedStatisticsViewModel
    {
        /// <summary>
        /// آمار کلی
        /// </summary>
        public AssignmentHistoryStatisticsViewModel GeneralStatistics { get; set; } = new AssignmentHistoryStatisticsViewModel();

        /// <summary>
        /// آمار ماهانه
        /// </summary>
        public List<MonthlyStatisticsViewModel> MonthlyStatistics { get; set; } = new List<MonthlyStatisticsViewModel>();

        /// <summary>
        /// آمار پزشکان فعال (برای view)
        /// </summary>
        public List<DoctorActivityStatisticsViewModel> DoctorActivityStatistics { get; set; } = new List<DoctorActivityStatisticsViewModel>();

        /// <summary>
        /// آمار پزشکان فعال (برای view)
        /// </summary>
        public List<DoctorActivityStatisticsViewModel> DoctorActivity { get; set; } = new List<DoctorActivityStatisticsViewModel>();

        /// <summary>
        /// آمار دپارتمان‌ها (برای view)
        /// </summary>
        public List<DepartmentActivityStatisticsViewModel> DepartmentActivityStatistics { get; set; } = new List<DepartmentActivityStatisticsViewModel>();

        /// <summary>
        /// آمار دپارتمان‌ها (برای view)
        /// </summary>
        public List<DepartmentActivityStatisticsViewModel> DepartmentActivity { get; set; } = new List<DepartmentActivityStatisticsViewModel>();

        /// <summary>
        /// آمار کاربران (برای view)
        /// </summary>
        public List<UserActivityStatisticsViewModel> UserActivityStatistics { get; set; } = new List<UserActivityStatisticsViewModel>();

        /// <summary>
        /// آمار کاربران (برای view)
        /// </summary>
        public List<UserActivityStatisticsViewModel> UserActivity { get; set; } = new List<UserActivityStatisticsViewModel>();

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// تاریخ تولید
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        // Properties for view compatibility
        /// <summary>
        /// تعداد کل رکوردها
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهای بحرانی
        /// </summary>
        public int CriticalRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهای مهم
        /// </summary>
        public int ImportantRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهای عادی
        /// </summary>
        public int NormalRecords { get; set; }

        /// <summary>
        /// تعداد انتسابات
        /// </summary>
        public int AssignmentCount { get; set; }

        /// <summary>
        /// تعداد حذف انتساب
        /// </summary>
        public int RemovalCount { get; set; }

        /// <summary>
        /// تعداد بروزرسانی
        /// </summary>
        public int UpdateCount { get; set; }
    }

    /// <summary>
    /// ViewModel برای آمار ماهانه
    /// </summary>
    public class MonthlyStatisticsViewModel
    {
        /// <summary>
        /// سال
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// ماه
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// نام ماه
        /// </summary>
        public string MonthName { get; set; }

        /// <summary>
        /// تعداد کل عملیات
        /// </summary>
        public int TotalOperations { get; set; }

        /// <summary>
        /// تعداد رکوردها (برای view)
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// تعداد انتسابات
        /// </summary>
        public int AssignmentsCount { get; set; }

        /// <summary>
        /// تعداد انتقالات
        /// </summary>
        public int TransfersCount { get; set; }

        /// <summary>
        /// تعداد حذف‌ها
        /// </summary>
        public int RemovalsCount { get; set; }

        /// <summary>
        /// تعداد ویرایش‌ها
        /// </summary>
        public int UpdatesCount { get; set; }
    }

    /// <summary>
    /// ViewModel برای آمار فعالیت پزشکان
    /// </summary>
    public class DoctorActivityStatisticsViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// کد ملی پزشک
        /// </summary>
        public string DoctorNationalCode { get; set; }

        /// <summary>
        /// تعداد کل عملیات
        /// </summary>
        public int TotalOperations { get; set; }

        /// <summary>
        /// تعداد رکوردها (برای view)
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// آخرین عملیات
        /// </summary>
        public DateTime? LastOperation { get; set; }

        /// <summary>
        /// میانگین عملیات در ماه
        /// </summary>
        public decimal AverageOperationsPerMonth { get; set; }
    }

    /// <summary>
    /// ViewModel برای آمار فعالیت دپارتمان‌ها
    /// </summary>
    public class DepartmentActivityStatisticsViewModel
    {
        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// تعداد کل عملیات
        /// </summary>
        public int TotalOperations { get; set; }

        /// <summary>
        /// تعداد رکوردها (برای view)
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// تعداد پزشکان فعال
        /// </summary>
        public int ActiveDoctorsCount { get; set; }

        /// <summary>
        /// آخرین عملیات
        /// </summary>
        public DateTime? LastOperation { get; set; }

        /// <summary>
        /// تعداد رکوردهای بحرانی
        /// </summary>
        public int CriticalRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهای مهم
        /// </summary>
        public int ImportantRecords { get; set; }

        /// <summary>
        /// تعداد رکوردهای عادی
        /// </summary>
        public int NormalRecords { get; set; }
    }

    /// <summary>
    /// ViewModel برای آمار فعالیت کاربران
    /// </summary>
    public class UserActivityStatisticsViewModel
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// نام کاربر
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// تعداد کل عملیات
        /// </summary>
        public int TotalOperations { get; set; }

        /// <summary>
        /// تعداد رکوردها (برای view)
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// آخرین عملیات
        /// </summary>
        public DateTime? LastOperation { get; set; }

        /// <summary>
        /// میانگین عملیات در روز
        /// </summary>
        public decimal AverageOperationsPerDay { get; set; }
    }

    /// <summary>
    /// ViewModel برای صفحه‌بندی
    /// </summary>
    public class PaginationViewModel
    {
        /// <summary>
        /// صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// اندازه صفحه
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages { get; set; } = 1;

        /// <summary>
        /// تعداد کل آیتم‌ها
        /// </summary>
        public int TotalItems { get; set; } = 0;

        /// <summary>
        /// آیا صفحه بعدی وجود دارد
        /// </summary>
        public bool HasNextPage { get; set; } = false;

        /// <summary>
        /// آیا صفحه قبلی وجود دارد
        /// </summary>
        public bool HasPreviousPage { get; set; } = false;

        /// <summary>
        /// شماره صفحه شروع
        /// </summary>
        public int StartPage { get; set; } = 1;

        /// <summary>
        /// شماره صفحه پایان
        /// </summary>
        public int EndPage { get; set; } = 1;
    }
}
