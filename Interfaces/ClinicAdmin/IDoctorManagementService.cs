using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.ViewModels.DoctorManagementVM;
using DoctorCreateEditViewModel = ClinicApp.ViewModels.DoctorCreateEditViewModel;
using DoctorDetailsViewModel = ClinicApp.ViewModels.DoctorDetailsViewModel;
using DoctorIndexViewModel = ClinicApp.ViewModels.DoctorIndexViewModel;
using LookupItemViewModel = ClinicApp.ViewModels.LookupItemViewModel;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// سرویس تخصصی برای مدیریت کامل پزشکان در سیستم کلینیک شفا
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
    /// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    public interface IDoctorManagementService
    {
        #region Core Profile Management (مدیریت پروفایل اصلی)

        /// <summary>
        /// دریافت لیست پزشکان با قابلیت صفحه‌بندی و جستجو.
        /// </summary>
        /// <param name="filter">فیلترهای جستجو شامل clinicId، departmentId، searchTerm، pageNumber، pageSize</param>
        /// <returns>یک نتیجه صفحه‌بندی شده از پزشکان.</returns>
        Task<ServiceResult<PagedResult<DoctorIndexViewModel>>> GetDoctorsAsync(DoctorSearchViewModel filter);

        /// <summary>
        /// دریافت جزئیات کامل یک پزشک برای نمایش.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <returns>نتیجه حاوی جزئیات پزشک.</returns>
        Task<ServiceResult<DoctorDetailsViewModel>> GetDoctorDetailsAsync(int doctorId);

        /// <summary>
        /// دریافت اطلاعات یک پزشک برای پر کردن فرم ویرایش.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <returns>نتیجه حاوی اطلاعات پزشک برای ویرایش.</returns>
        Task<ServiceResult<DoctorCreateEditViewModel>> GetDoctorForEditAsync(int doctorId);

        /// <summary>
        /// ایجاد یک پزشک جدید بر اساس اطلاعات ورودی.
        /// </summary>
        /// <param name="model">مدل حاوی اطلاعات پزشک جدید.</param>
        /// <returns>نتیجه عملیات ایجاد.</returns>
        Task<ServiceResult<Doctor>> CreateDoctorAsync(DoctorCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی اطلاعات یک پزشک موجود.
        /// </summary>
        /// <param name="model">مدل حاوی اطلاعات به‌روز شده پزشک.</param>
        /// <returns>نتیجه عملیات به‌روزرسانی.</returns>
        Task<ServiceResult<Doctor>> UpdateDoctorAsync(DoctorCreateEditViewModel model);

        /// <summary>
        /// حذف نرم یک پزشک با بررسی قوانین کسب‌وکار.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک برای حذف.</param>
        /// <returns>نتیجه عملیات حذف.</returns>
        Task<ServiceResult> SoftDeleteDoctorAsync(int doctorId);

        /// <summary>
        /// بازیابی یک پزشک حذف شده.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک برای بازیابی.</param>
        /// <returns>نتیجه عملیات بازیابی.</returns>
        Task<ServiceResult> RestoreDoctorAsync(int doctorId);

        #endregion

        #region Doctor-Department Management (مدیریت انتصاب پزشک به دپارتمان)

        /// <summary>
        /// دریافت لیست پزشکان فعال برای استفاده در لیست‌های کشویی (Dropdowns).
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری).</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری).</param>
        /// <returns>لیستی از پزشکان فعال.</returns>
        Task<ServiceResult<List<LookupItemViewModel>>> GetActiveDoctorsForLookupAsync(int? clinicId, int? departmentId);

        /// <summary>
        /// دریافت لیست دپارتمان‌های مرتبط با یک پزشک.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="searchTerm">عبارت جستجو.</param>
        /// <param name="pageNumber">شماره صفحه.</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه.</param>
        /// <returns>یک نتیجه صفحه‌بندی شده از دپارتمان‌های مرتبط با پزشک.</returns>
        Task<ServiceResult<PagedResult<DoctorDepartmentViewModel>>> GetDepartmentsForDoctorAsync(int doctorId, string searchTerm, int pageNumber, int pageSize);

        /// <summary>
        /// انتصاب یک پزشک به یک دپارتمان با مشخص کردن نقش و سایر جزئیات.
        /// </summary>
        /// <param name="model">مدل حاوی اطلاعات انتصاب پزشک به دپارتمان.</param>
        /// <returns>نتیجه عملیات انتصاب.</returns>
        Task<ServiceResult> AssignDoctorToDepartmentAsync(DoctorDepartmentViewModel model);

        /// <summary>
        /// لغو انتصاب یک پزشک از یک دپارتمان.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="departmentId">شناسه دپارتمان.</param>
        /// <returns>نتیجه عملیات لغو انتصاب.</returns>
        Task<ServiceResult> RevokeDoctorFromDepartmentAsync(int doctorId, int departmentId);

        /// <summary>
        /// به‌روزرسانی اطلاعات انتصاب پزشک به دپارتمان (نقش، وضعیت فعال/غیرفعال و ...).
        /// </summary>
        /// <param name="model">مدل حاوی اطلاعات به‌روز شده انتصاب.</param>
        /// <returns>نتیجه عملیات به‌روزرسانی.</returns>
        Task<ServiceResult> UpdateDoctorDepartmentAssignmentAsync(DoctorDepartmentViewModel model);

        #endregion

        #region Doctor-ServiceCategory Management (مدیریت انتصاب پزشک به سرفصل‌های خدماتی)

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات مجاز برای یک پزشک.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="searchTerm">عبارت جستجو.</param>
        /// <param name="pageNumber">شماره صفحه.</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه.</param>
        /// <returns>یک نتیجه صفحه‌بندی شده از دسته‌بندی‌های خدمات مجاز برای پزشک.</returns>
        Task<ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>> GetServiceCategoriesForDoctorAsync(int doctorId, string searchTerm, int pageNumber, int pageSize);

        /// <summary>
        /// اعطا کردن صلاحیت ارائه یک دسته‌بندی خدمات به یک پزشک.
        /// </summary>
        /// <param name="model">مدل حاوی اطلاعات صلاحیت.</param>
        /// <returns>نتیجه عملیات اعطا صلاحیت.</returns>
        Task<ServiceResult> GrantServiceCategoryToDoctorAsync(DoctorServiceCategoryViewModel model);

        /// <summary>
        /// لغو صلاحیت ارائه یک دسته‌بندی خدمات از یک پزشک.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات.</param>
        /// <returns>نتیجه عملیات لغو صلاحیت.</returns>
        Task<ServiceResult> RevokeServiceCategoryFromDoctorAsync(int doctorId, int serviceCategoryId);

        /// <summary>
        /// به‌روزرسانی اطلاعات صلاحیت پزشک در ارائه یک دسته‌بندی خدمات.
        /// </summary>
        /// <param name="model">مدل حاوی اطلاعات به‌روز شده صلاحیت.</param>
        /// <returns>نتیجه عملیات به‌روزرسانی.</returns>
        Task<ServiceResult> UpdateDoctorServiceCategoryPermissionAsync(DoctorServiceCategoryViewModel model);

        #endregion

        #region Assignment Management (مدیریت انتسابات)

        /// <summary>
        /// به‌روزرسانی کامل انتسابات یک پزشک (دپارتمان‌ها و سرفصل‌های خدماتی).
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="assignments">مدل حاوی تمام انتسابات.</param>
        /// <returns>نتیجه عملیات به‌روزرسانی.</returns>
        Task<ServiceResult> UpdateDoctorAssignmentsAsync(int doctorId, DoctorAssignmentsViewModel assignments);

        /// <summary>
        /// دریافت تمام انتسابات یک پزشک (دپارتمان‌ها و سرفصل‌های خدماتی).
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <returns>نتیجه حاوی تمام انتسابات پزشک.</returns>
        Task<ServiceResult<DoctorAssignmentsViewModel>> GetDoctorAssignmentsAsync(int doctorId);

        #endregion

        #region Scheduling & Availability (برنامه‌ریزی و زمان‌های در دسترس)

        /// <summary>
        /// تنظیم یا به‌روزرسانی برنامه کاری هفتگی یک پزشک.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="schedule">مدل برنامه کاری.</param>
        /// <returns>نتیجه عملیات تنظیم برنامه.</returns>
        Task<ServiceResult> SetDoctorScheduleAsync(int doctorId, DoctorScheduleViewModel schedule);

        /// <summary>
        /// دریافت برنامه کاری هفتگی یک پزشک.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <returns>نتیجه حاوی برنامه کاری پزشک.</returns>
        Task<ServiceResult<DoctorScheduleViewModel>> GetDoctorScheduleAsync(int doctorId);
        
        /// <summary>
        /// مسدود کردن یک بازه زمانی برای پزشک (مثلا برای مرخصی یا جلسه).
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="start">زمان شروع.</param>
        /// <param name="end">زمان پایان.</param>
        /// <param name="reason">دلیل مسدودیت.</param>
        /// <returns>نتیجه عملیات مسدودیت.</returns>
        Task<ServiceResult> BlockTimeRangeForDoctorAsync(int doctorId, DateTime start, DateTime end, string reason);

        /// <summary>
        /// محاسبه و بازگرداندن تمام اسلات‌های زمانی خالی و قابل رزرو برای یک پزشک در یک روز مشخص.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="date">تاریخ مورد نظر.</param>
        /// <returns>نتیجه حاوی اسلات‌های زمانی خالی.</returns>
        Task<ServiceResult<List<TimeSlotViewModel>>> GetAvailableAppointmentSlotsAsync(int doctorId, DateTime date);

        #endregion

        #region Specialized Queries (پرس‌وجوهای تخصصی)

        /// <summary>
        /// بررسی دسترسی پزشک به یک دسته‌بندی خدمات خاص.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات.</param>
        /// <returns>نتیجه حاوی اطلاعات دسترسی پزشک به دسته‌بندی خدمات.</returns>
        Task<ServiceResult<bool>> HasAccessToServiceCategoryAsync(int doctorId, int serviceCategoryId);

        /// <summary>
        /// بررسی دسترسی پزشک به یک خدمت خاص.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="serviceId">شناسه خدمت.</param>
        /// <returns>نتیجه حاوی اطلاعات دسترسی پزشک به خدمت.</returns>
        Task<ServiceResult<bool>> HasAccessToServiceAsync(int doctorId, int serviceId);

        /// <summary>
        /// دریافت لیست پزشکان فعال در یک بازه زمانی برای گزارش‌گیری.
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک.</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری).</param>
        /// <param name="startDate">تاریخ شروع.</param>
        /// <param name="endDate">تاریخ پایان.</param>
        /// <returns>نتیجه حاوی اطلاعات گزارش پزشکان فعال.</returns>
        Task<ServiceResult<ActiveDoctorsReportViewModel>> GetActiveDoctorsReportAsync(int clinicId, int? departmentId, DateTime startDate, DateTime endDate);

        #endregion
        
        #region Data & Reporting (داده و گزارش‌گیری)

        /// <summary>
        /// دریافت داده‌های کلیدی برای داشبورد یک پزشک (مانند تعداد نوبت‌های امروز).
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <returns>نتیجه حاوی داده‌های داشبورد پزشک.</returns>
        Task<ServiceResult<DoctorDashboardViewModel>> GetDoctorDashboardDataAsync(int doctorId);

        #endregion
    }
}
