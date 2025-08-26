using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.DoctorManagementVM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// رپازیتوری تخصصی برای مدیریت کامل پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی کامل از ساختار سازمانی پزشکی (کلینیک → دپارتمان → پزشک)
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت پزشکان
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 6. رعایت استانداردهای امنیتی سیستم‌های پزشکی ایران
    /// 7. پشتیبانی از سیستم‌های Load Balanced و محیط‌های Production
    /// 8. پشتیبانی از محدودیت نرخ (Rate Limiting) برای جلوگیری از حملات امنیتی
    /// 9. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 10. پشتیبانی از سیستم‌های پزشکی با استانداردهای محلی ایران
    /// 
    /// نکته حیاتی: این رپازیتوری بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    public interface IDoctorRepository
    {
        #region Core CRUD Operations (عملیات اصلی CRUD)

        /// <summary>
        /// دریافت پزشکان با pagination و فیلتر
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <returns>لیست پزشکان با pagination</returns>
        Task<PagedResult<Doctor>> GetDoctorsAsync(int? clinicId, int? departmentId, string searchTerm, int pageNumber, int pageSize);

        /// <summary>
        /// دریافت پزشک بر اساس شناسه
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>پزشک مورد نظر</returns>
        Task<Doctor> GetByIdAsync(int doctorId);

        /// <summary>
        /// دریافت پزشک بر اساس شناسه همراه با تمام روابط برای نمایش جزئیات
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>پزشک مورد نظر همراه با تمام روابط</returns>
        Task<Doctor> GetByIdWithDetailsAsync(int doctorId);

        /// <summary>
        /// بررسی وجود پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>درست اگر پزشک وجود داشته باشد</returns>
        Task<bool> DoesDoctorExistAsync(int doctorId);

        /// <summary>
        /// بررسی وجود کد نظام پزشکی
        /// </summary>
        /// <param name="medicalCouncilCode">کد نظام پزشکی</param>
        /// <param name="excludeDoctorId">شناسه پزشک برای استثنا (در حالت ویرایش)</param>
        /// <returns>درست اگر کد نظام پزشکی وجود داشته باشد</returns>
        Task<bool> DoesMedicalCouncilCodeExistAsync(string medicalCouncilCode, int? excludeDoctorId = null);

        /// <summary>
        /// بررسی وجود کد ملی
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <param name="excludeDoctorId">شناسه پزشک برای استثنا (در حالت ویرایش)</param>
        /// <returns>درست اگر کد ملی وجود داشته باشد</returns>
        Task<bool> DoesNationalCodeExistAsync(string nationalCode, int? excludeDoctorId = null);

        /// <summary>
        /// افزودن پزشک جدید
        /// </summary>
        /// <param name="doctor">پزشک جدید</param>
        /// <returns>پزشک افزوده شده</returns>
        Task<Doctor> AddAsync(Doctor doctor);

        /// <summary>
        /// به‌روزرسانی پزشک موجود
        /// </summary>
        /// <param name="doctor">پزشک به‌روز شده</param>
        /// <returns>پزشک به‌روز شده</returns>
        Task<Doctor> UpdateAsync(Doctor doctor);

        /// <summary>
        /// حذف نرم پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="deletedByUserId">شناسه کاربر حذف کننده</param>
        /// <returns>درست اگر حذف موفقیت‌آمیز باشد</returns>
        Task<bool> SoftDeleteAsync(int doctorId, string deletedByUserId);

        /// <summary>
        /// بازیابی پزشک حذف شده
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="restoredByUserId">شناسه کاربر بازیابی کننده</param>
        /// <returns>درست اگر بازیابی موفقیت‌آمیز باشد</returns>
        Task<bool> RestoreAsync(int doctorId, string restoredByUserId);

        #endregion

        #region Doctor-Department Management (مدیریت انتصاب پزشک به دپارتمان)

        /// <summary>
        /// دریافت انتصاب پزشک به دپارتمان بر اساس شناسه‌ها
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>ارتباط پزشک-دپارتمان</returns>
        Task<DoctorDepartment> GetDoctorDepartmentAsync(int doctorId, int departmentId);

        /// <summary>
        /// دریافت انتصاب پزشک به دپارتمان همراه با جزئیات
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>ارتباط پزشک-دپارتمان همراه با جزئیات</returns>
        Task<DoctorDepartment> GetDoctorDepartmentWithDetailsAsync(int doctorId, int departmentId);

        /// <summary>
        /// دریافت لیست انتصابات پزشک به دپارتمان‌ها
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
        /// <returns>لیست انتصابات پزشک به دپارتمان‌ها</returns>
        Task<List<DoctorDepartment>> GetDoctorDepartmentsAsync(int doctorId, string searchTerm, int pageNumber, int pageSize);

        /// <summary>
        /// دریافت تعداد انتصابات پزشک به دپارتمان‌ها
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>تعداد انتصابات پزشک به دپارتمان‌ها</returns>
        Task<int> GetDoctorDepartmentsCountAsync(int doctorId, string searchTerm);

        /// <summary>
        /// افزودن انتصاب پزشک به دپارتمان
        /// </summary>
        /// <param name="doctorDepartment">ارتباط پزشک-دپارتمان</param>
        /// <returns>ارتباط پزشک-دپارتمان افزوده شده</returns>
        Task<DoctorDepartment> AddDoctorDepartmentAsync(DoctorDepartment doctorDepartment);

        /// <summary>
        /// به‌روزرسانی انتصاب پزشک به دپارتمان
        /// </summary>
        /// <param name="doctorDepartment">ارتباط پزشک-دپارتمان به‌روز شده</param>
        /// <returns>ارتباط پزشک-دپارتمان به‌روز شده</returns>
        Task<DoctorDepartment> UpdateDoctorDepartmentAsync(DoctorDepartment doctorDepartment);

        /// <summary>
        /// حذف انتصاب پزشک از دپارتمان
        /// </summary>
        /// <param name="doctorDepartment">ارتباط پزشک-دپارتمان برای حذف</param>
        /// <returns>درست اگر حذف موفقیت‌آمیز باشد</returns>
        Task<bool> DeleteDoctorDepartmentAsync(DoctorDepartment doctorDepartment);

        /// <summary>
        /// بررسی وجود انتصاب پزشک به دپارتمان
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="excludeId">شناسه برای استثنا (در حالت ویرایش)</param>
        /// <returns>درست اگر انتصاب وجود داشته باشد</returns>
        Task<bool> DoesDoctorDepartmentExistAsync(int doctorId, int departmentId, int? excludeId = null);

        /// <summary>
        /// دریافت لیست پزشکان فعال در یک دپارتمان برای استفاده در لیست‌های کشویی
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>لیست پزشکان فعال در دپارتمان</returns>
        Task<List<Doctor>> GetActiveDoctorsForDepartmentLookupAsync(int departmentId);

        #endregion

        #region Doctor-ServiceCategory Management (مدیریت انتصاب پزشک به سرفصل‌های خدماتی)

        /// <summary>
        /// دریافت انتصاب پزشک به سرفصل خدماتی بر اساس شناسه‌ها
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="serviceCategoryId">شناسه سرفصل خدماتی</param>
        /// <returns>ارتباط پزشک-سرفصل خدماتی</returns>
        Task<DoctorServiceCategory> GetDoctorServiceCategoryAsync(int doctorId, int serviceCategoryId);

        /// <summary>
        /// دریافت انتصاب پزشک به سرفصل خدماتی همراه با جزئیات
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="serviceCategoryId">شناسه سرفصل خدماتی</param>
        /// <returns>ارتباط پزشک-سرفصل خدماتی همراه با جزئیات</returns>
        Task<DoctorServiceCategory> GetDoctorServiceCategoryWithDetailsAsync(int doctorId, int serviceCategoryId);

        /// <summary>
        /// دریافت لیست انتصابات پزشک به سرفصل‌های خدماتی
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
        /// <returns>لیست انتصابات پزشک به سرفصل‌های خدماتی</returns>
        Task<List<DoctorServiceCategory>> GetDoctorServiceCategoriesAsync(int doctorId, string searchTerm, int pageNumber, int pageSize);

        /// <summary>
        /// دریافت تعداد انتصابات پزشک به سرفصل‌های خدماتی
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>تعداد انتصابات پزشک به سرفصل‌های خدماتی</returns>
        Task<int> GetDoctorServiceCategoriesCountAsync(int doctorId, string searchTerm);

        /// <summary>
        /// افزودن انتصاب پزشک به سرفصل خدماتی
        /// </summary>
        /// <param name="doctorServiceCategory">ارتباط پزشک-سرفصل خدماتی</param>
        /// <returns>ارتباط پزشک-سرفصل خدماتی افزوده شده</returns>
        Task<DoctorServiceCategory> AddDoctorServiceCategoryAsync(DoctorServiceCategory doctorServiceCategory);

        /// <summary>
        /// به‌روزرسانی انتصاب پزشک به سرفصل خدماتی
        /// </summary>
        /// <param name="doctorServiceCategory">ارتباط پزشک-سرفصل خدماتی به‌روز شده</param>
        /// <returns>ارتباط پزشک-سرفصل خدماتی به‌روز شده</returns>
        Task<DoctorServiceCategory> UpdateDoctorServiceCategoryAsync(DoctorServiceCategory doctorServiceCategory);

        /// <summary>
        /// حذف انتصاب پزشک از سرفصل خدماتی
        /// </summary>
        /// <param name="doctorServiceCategory">ارتباط پزشک-سرفصل خدماتی برای حذف</param>
        /// <returns>درست اگر حذف موفقیت‌آمیز باشد</returns>
        Task<bool> DeleteDoctorServiceCategoryAsync(DoctorServiceCategory doctorServiceCategory);

        /// <summary>
        /// بررسی وجود انتصاب پزشک به سرفصل خدماتی
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="serviceCategoryId">شناسه سرفصل خدماتی</param>
        /// <param name="excludeId">شناسه برای استثنا (در حالت ویرایش)</param>
        /// <returns>درست اگر انتصاب وجود داشته باشد</returns>
        Task<bool> DoesDoctorServiceCategoryExistAsync(int doctorId, int serviceCategoryId, int? excludeId = null);

        /// <summary>
        /// بررسی دسترسی پزشک به یک سرفصل خدماتی خاص
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="serviceCategoryId">شناسه سرفصل خدماتی</param>
        /// <returns>درست اگر پزشک به سرفصل خدماتی دسترسی داشته باشد</returns>
        Task<bool> HasAccessToServiceCategoryAsync(int doctorId, int serviceCategoryId);

        /// <summary>
        /// بررسی دسترسی پزشک به یک خدمت خاص
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>درست اگر پزشک به خدمت دسترسی داشته باشد</returns>
        Task<bool> HasAccessToServiceAsync(int doctorId, int serviceId);

        /// <summary>
        /// دریافت لیست پزشکان مجاز در یک سرفصل خدماتی برای استفاده در لیست‌های کشویی
        /// </summary>
        /// <param name="serviceCategoryId">شناسه سرفصل خدماتی</param>
        /// <returns>لیست پزشکان مجاز در سرفصل خدماتی</returns>
        Task<List<Doctor>> GetAuthorizedDoctorsForServiceCategoryLookupAsync(int serviceCategoryId);

        #endregion

        #region Schedule Management (مدیریت برنامه کاری)

        /// <summary>
        /// دریافت برنامه کاری پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>برنامه کاری پزشک</returns>
        Task<DoctorSchedule> GetDoctorScheduleAsync(int doctorId);

        /// <summary>
        /// دریافت برنامه کاری پزشک همراه با جزئیات
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>برنامه کاری پزشک همراه با جزئیات</returns>
        Task<DoctorSchedule> GetDoctorScheduleWithDetailsAsync(int doctorId);

        /// <summary>
        /// افزودن برنامه کاری جدید برای پزشک
        /// </summary>
        /// <param name="schedule">برنامه کاری جدید</param>
        /// <returns>برنامه کاری افزوده شده</returns>
        Task<DoctorSchedule> AddDoctorScheduleAsync(DoctorSchedule schedule);

        /// <summary>
        /// به‌روزرسانی برنامه کاری پزشک
        /// </summary>
        /// <param name="schedule">برنامه کاری به‌روز شده</param>
        /// <returns>برنامه کاری به‌روز شده</returns>
        Task<DoctorSchedule> UpdateDoctorScheduleAsync(DoctorSchedule schedule);

        /// <summary>
        /// دریافت اسلات‌های زمانی خالی و قابل رزرو برای یک پزشک در یک روز مشخص
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>لیست اسلات‌های زمانی خالی</returns>
        Task<List<DoctorTimeSlot>> GetAvailableAppointmentSlotsAsync(int doctorId, DateTime date);

        /// <summary>
        /// مسدود کردن یک بازه زمانی برای پزشک (مثلا برای مرخصی یا جلسه)
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="start">زمان شروع</param>
        /// <param name="end">زمان پایان</param>
        /// <param name="reason">دلیل مسدودیت</param>
        /// <returns>درست اگر مسدودیت با موفقیت انجام شد</returns>
        Task<bool> BlockTimeRangeForDoctorAsync(int doctorId, DateTime start, DateTime end, string reason);

        #endregion

        #region Lookup & Search (جستجو و لیست‌ها)

        /// <summary>
        /// دریافت لیست پزشکان فعال برای استفاده در لیست‌های کشویی
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <returns>لیست پزشکان فعال</returns>
        Task<List<Doctor>> GetActiveDoctorsForLookupAsync(int? clinicId, int? departmentId);

        /// <summary>
        /// جستجوی پزشکان بر اساس فیلترهای مختلف
        /// </summary>
        /// <param name="filter">فیلترهای جستجو</param>
        /// <returns>لیست پزشکان مطابق با فیلترها</returns>
        Task<List<Doctor>> SearchDoctorsAsync(DoctorSearchViewModel filter);

        /// <summary>
        /// دریافت تعداد پزشکان مطابق با فیلترهای جستجو
        /// </summary>
        /// <param name="filter">فیلترهای جستجو</param>
        /// <returns>تعداد پزشکان مطابق با فیلترها</returns>
        Task<int> GetDoctorsCountAsync(DoctorSearchViewModel filter);

        #endregion

        #region Dependency Management (مدیریت وابستگی‌ها)

        /// <summary>
        /// دریافت اطلاعات وابستگی‌های پزشک برای بررسی امکان حذف
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>اطلاعات وابستگی‌های پزشک</returns>
        Task<DoctorDependencyInfo> GetDoctorDependencyInfoAsync(int doctorId);

        /// <summary>
        /// بررسی امکان حذف پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>درست اگر پزشک قابل حذف باشد</returns>
        Task<bool> CanDeleteDoctorAsync(int doctorId);

        #endregion

        #region Reporting & Statistics (گزارش‌گیری و آمار)

        /// <summary>
        /// دریافت گزارش پزشکان فعال در یک بازه زمانی
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>لیست پزشکان فعال همراه با آمار</returns>
        Task<List<Doctor>> GetActiveDoctorsReportAsync(int clinicId, int? departmentId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت داده‌های داشبورد پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>داده‌های داشبورد پزشک</returns>
        Task<Doctor> GetDoctorDashboardDataAsync(int doctorId);

        #endregion
    }
}
