using System.Collections.Generic;

namespace ClinicApp.Models
{
    /// <summary>
    /// مدل اطلاعات وابستگی‌های پزشک برای بررسی امکان حذف
    /// </summary>
    public class DoctorDependencyInfo
    {
        /// <summary>
        /// آیا پزشک قابل حذف است
        /// </summary>
        public bool CanBeDeleted { get; set; }

        /// <summary>
        /// پیام خطای حذف (در صورت عدم امکان حذف)
        /// </summary>
        public string DeletionErrorMessage { get; set; }

        /// <summary>
        /// تعداد کل انتصابات به دپارتمان‌ها
        /// </summary>
        public int TotalDepartmentAssignments { get; set; }

        /// <summary>
        /// تعداد انتصابات فعال به دپارتمان‌ها
        /// </summary>
        public int ActiveDepartmentAssignments { get; set; }

        /// <summary>
        /// تعداد کل انتصابات به سرفصل‌های خدماتی
        /// </summary>
        public int TotalServiceCategoryAssignments { get; set; }

        /// <summary>
        /// تعداد انتصابات فعال به سرفصل‌های خدماتی
        /// </summary>
        public int ActiveServiceCategoryAssignments { get; set; }

        /// <summary>
        /// تعداد کل نوبت‌های فعال
        /// </summary>
        public int TotalActiveAppointments { get; set; }

        /// <summary>
        /// تعداد کل نوبت‌های آینده
        /// </summary>
        public int TotalFutureAppointments { get; set; }

        /// <summary>
        /// تعداد کل نوبت‌های امروز
        /// </summary>
        public int TotalTodayAppointments { get; set; }

        /// <summary>
        /// آیا انتسابات فعال به دپارتمان دارد
        /// </summary>
        public bool HasActiveDepartmentAssignments { get; set; }

        /// <summary>
        /// آیا انتسابات فعال به دسته‌بندی خدمات دارد
        /// </summary>
        public bool HasActiveServiceCategoryAssignments { get; set; }

        /// <summary>
        /// آیا برنامه‌های کاری فعال دارد
        /// </summary>
        public bool HasActiveSchedules { get; set; }

        /// <summary>
        /// تعداد نوبت‌ها
        /// </summary>
        public int AppointmentCount { get; set; }

        /// <summary>
        /// تعداد انتسابات دپارتمان
        /// </summary>
        public int DepartmentAssignmentCount { get; set; }

        /// <summary>
        /// تعداد انتسابات دسته‌بندی خدمات
        /// </summary>
        public int ServiceCategoryAssignmentCount { get; set; }

        /// <summary>
        /// لیست دپارتمان‌های مرتبط
        /// </summary>
        public List<DepartmentSummaryInfo> DepartmentSummaries { get; set; }

        /// <summary>
        /// لیست سرفصل‌های خدماتی مرتبط
        /// </summary>
        public List<ServiceCategorySummaryInfo> ServiceCategorySummaries { get; set; }

        /// <summary>
        /// لیست نوبت‌های آینده
        /// </summary>
        public List<AppointmentSummaryInfo> FutureAppointments { get; set; }

        public DoctorDependencyInfo()
        {
            DepartmentSummaries = new List<DepartmentSummaryInfo>();
            ServiceCategorySummaries = new List<ServiceCategorySummaryInfo>();
            FutureAppointments = new List<AppointmentSummaryInfo>();
        }
    }

    /// <summary>
    /// اطلاعات خلاصه دپارتمان
    /// </summary>
    public class DepartmentSummaryInfo
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
        /// نام کلینیک
        /// </summary>
        public string ClinicName { get; set; }

        /// <summary>
        /// تاریخ انتصاب
        /// </summary>
        public string AssignedDateShamsi { get; set; }

        /// <summary>
        /// آیا انتصاب فعال است
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// نقش پزشک در دپارتمان
        /// </summary>
        public string Role { get; set; }
    }

    /// <summary>
    /// اطلاعات خلاصه سرفصل خدماتی
    /// </summary>
    public class ServiceCategorySummaryInfo
    {
        /// <summary>
        /// شناسه سرفصل خدماتی
        /// </summary>
        public int ServiceCategoryId { get; set; }

        /// <summary>
        /// نام سرفصل خدماتی
        /// </summary>
        public string ServiceCategoryName { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// نام کلینیک
        /// </summary>
        public string ClinicName { get; set; }

        /// <summary>
        /// تاریخ اعطا صلاحیت
        /// </summary>
        public string GrantedDateShamsi { get; set; }

        /// <summary>
        /// آیا صلاحیت فعال است
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تعداد خدمات در این سرفصل
        /// </summary>
        public int ServiceCount { get; set; }
    }

    /// <summary>
    /// اطلاعات خلاصه نوبت
    /// </summary>
    public class AppointmentSummaryInfo
    {
        /// <summary>
        /// شناسه نوبت
        /// </summary>
        public int AppointmentId { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// تاریخ نوبت (شمسی)
        /// </summary>
        public string AppointmentDateShamsi { get; set; }

        /// <summary>
        /// ساعت نوبت
        /// </summary>
        public string AppointmentTime { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// وضعیت نوبت
        /// </summary>
        public string Status { get; set; }
    }
}
