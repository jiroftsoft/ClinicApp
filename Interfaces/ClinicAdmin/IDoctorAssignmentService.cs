using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.DoctorManagementVM;
using ClinicApp.Models.Entities;
using ClinicApp.Models;
using DoctorDependencyInfo = ClinicApp.Models.DoctorDependencyInfo;

namespace ClinicApp.Interfaces.ClinicAdmin;

/// <summary>
/// اینترفیس تخصصی برای مدیریت انتسابات پزشکان در سیستم کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. تمرکز صرف بر مدیریت کامل انتسابات پزشکان
/// 2. رعایت استانداردهای پزشکی ایران در مدیریت انتسابات
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
/// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
/// 6. عملیات ترکیبی و انتقال پزشکان بین دپارتمان‌ها
/// 
/// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
/// </summary>
public interface IDoctorAssignmentService
{
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

    /// <summary>
    /// انتساب همزمان پزشک به دپارتمان و سرفصل‌های خدماتی مرتبط.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="departmentId">شناسه دپارتمان.</param>
    /// <param name="serviceCategoryIds">لیست شناسه‌های سرفصل‌های خدماتی.</param>
    /// <returns>نتیجه عملیات انتساب.</returns>
    Task<ServiceResult> AssignDoctorToDepartmentWithServicesAsync(int doctorId, int departmentId, List<int> serviceCategoryIds);

    /// <summary>
    /// حذف کامل تمام انتسابات یک پزشک.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <returns>نتیجه عملیات حذف.</returns>
    Task<ServiceResult> RemoveAllDoctorAssignmentsAsync(int doctorId);

    /// <summary>
    /// به‌روزرسانی انتسابات پزشک از طریق EditViewModel
    /// این متد برای فرم ویرایش انتسابات طراحی شده است
    /// </summary>
    /// <param name="editModel">مدل ویرایش انتسابات</param>
    /// <returns>نتیجه عملیات به‌روزرسانی</returns>
    Task<ServiceResult> UpdateDoctorAssignmentsFromEditAsync(DoctorAssignmentEditViewModel editModel);

    #endregion

    #region Statistics and Reporting (آمار و گزارش‌گیری)

    /// <summary>
    /// دریافت آمار کلی انتسابات پزشکان.
    /// </summary>
    /// <returns>نتیجه حاوی آمار انتسابات.</returns>
    Task<ServiceResult<AssignmentStatsViewModel>> GetAssignmentStatisticsAsync();

    /// <summary>
    /// آماده‌سازی کامل ViewModel برای صفحه اصلی مدیریت انتسابات
    /// شامل آمار، فیلترها و تنظیمات اولیه
    /// </summary>
    /// <returns>ViewModel کاملاً آماده برای نمایش</returns>
    Task<ServiceResult<DoctorAssignmentIndexViewModel>> GetDoctorAssignmentIndexViewModelAsync();

    /// <summary>
    /// دریافت اطلاعات وابستگی‌های پزشک برای بررسی امکان حذف.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <returns>نتیجه حاوی اطلاعات وابستگی‌های پزشک.</returns>
    Task<ServiceResult<DoctorDependencyInfo>> GetDoctorDependenciesAsync(int doctorId);

    /// <summary>
    /// دریافت تعداد انتسابات فعال یک پزشک.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <returns>نتیجه حاوی تعداد انتسابات فعال.</returns>
    Task<ServiceResult<int>> GetActiveAssignmentsCountAsync(int doctorId);

    /// <summary>
    /// دریافت لیست انتسابات برای DataTables با pagination و filtering.
    /// </summary>
    /// <param name="request">درخواست DataTables شامل pagination و filtering.</param>
    /// <returns>نتیجه حاوی لیست انتسابات با pagination.</returns>
    Task<ServiceResult<DataTablesResponse>> GetAssignmentsForDataTablesAsync(DataTablesRequest request);

    /// <summary>
    /// دریافت انتسابات برای DataTables با فیلترهای پیشرفته
    /// </summary>
    /// <param name="start">شروع pagination</param>
    /// <param name="length">تعداد رکورد در هر صفحه</param>
    /// <param name="searchValue">مقدار جستجو</param>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <param name="serviceCategoryId">شناسه سرفصل خدماتی</param>
    /// <param name="dateFrom">تاریخ از</param>
    /// <param name="dateTo">تاریخ تا</param>
    /// <returns>نتیجه حاوی داده‌های DataTables</returns>
    Task<ServiceResult<DataTablesResponse>> GetAssignmentsForDataTableAsync(
        int start, 
        int length, 
        string searchValue, 
        string departmentId, 
        string serviceCategoryId, 
        string dateFrom, 
        string dateTo);

    /// <summary>
    /// دریافت انتسابات فیلتر شده
    /// </summary>
    /// <param name="filter">فیلترهای اعمال شده</param>
    /// <returns>نتیجه حاوی انتسابات فیلتر شده</returns>
    Task<ServiceResult<List<DoctorAssignmentListItem>>> GetFilteredAssignmentsAsync(AssignmentFilterViewModel filter);

    #endregion

    #region Assignment History (تاریخچه انتسابات)

    /// <summary>
    /// دریافت تاریخچه انتسابات یک پزشک.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="page">شماره صفحه.</param>
    /// <param name="pageSize">تعداد رکورد در هر صفحه.</param>
    /// <returns>نتیجه حاوی تاریخچه انتسابات پزشک.</returns>
    Task<ServiceResult<List<DoctorAssignmentHistory>>> GetDoctorAssignmentHistoryAsync(int doctorId, int page = 1, int pageSize = 20);

    /// <summary>
    /// دریافت آمار تاریخچه انتسابات.
    /// </summary>
    /// <param name="startDate">تاریخ شروع (اختیاری).</param>
    /// <param name="endDate">تاریخ پایان (اختیاری).</param>
    /// <returns>نتیجه حاوی آمار تاریخچه انتسابات.</returns>
    Task<ServiceResult<DashboardHistoryStats>> GetAssignmentHistoryStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

    #endregion
}