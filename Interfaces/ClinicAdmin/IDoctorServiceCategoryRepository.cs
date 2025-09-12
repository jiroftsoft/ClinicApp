using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Doctor;

namespace ClinicApp.Interfaces.ClinicAdmin;

/// <summary>
/// اینترفیس تخصصی برای مدیریت صلاحیت‌های خدماتی پزشکان در سیستم کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. تمرکز صرف بر مدیریت رابطه چند-به-چند پزشک-دسته‌بندی خدمات
/// 2. رعایت استانداردهای پزشکی ایران در مدیریت صلاحیت‌ها
/// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
/// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
/// 
/// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
/// </summary>
public interface IDoctorServiceCategoryRepository
{
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

    /// <summary>
    /// دریافت لیست پزشکان فعال در یک دسته‌بندی خدماتی برای استفاده در لیست‌های کشویی
    /// </summary>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدماتی</param>
    /// <returns>لیست پزشکان فعال در دسته‌بندی خدماتی</returns>
    Task<List<Doctor>> GetActiveDoctorsForServiceCategoryLookupAsync(int serviceCategoryId);

    /// <summary>
    /// ذخیره تمام تغییرات در انتظار به پایگاه داده
    /// </summary>
    Task SaveChangesAsync();

    /// <summary>
    /// دریافت لیست همه انتصابات پزشکان به سرفصل‌های خدماتی (برای فیلتر "همه پزشکان")
    /// </summary>
    /// <param name="searchTerm">عبارت جستجو</param>
    /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
    /// <param name="serviceCategoryId">شناسه سرفصل خدماتی (اختیاری)</param>
    /// <param name="isActive">وضعیت فعال (اختیاری)</param>
    /// <param name="pageNumber">شماره صفحه</param>
    /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
    /// <returns>لیست همه انتصابات پزشکان به سرفصل‌های خدماتی</returns>
    Task<List<DoctorServiceCategory>> GetAllDoctorServiceCategoriesAsync(string searchTerm, int? doctorId, int? serviceCategoryId, bool? isActive, int pageNumber, int pageSize);

    /// <summary>
    /// دریافت تعداد همه انتصابات پزشکان به سرفصل‌های خدماتی
    /// </summary>
    /// <param name="searchTerm">عبارت جستجو</param>
    /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
    /// <param name="serviceCategoryId">شناسه سرفصل خدماتی (اختیاری)</param>
    /// <param name="isActive">وضعیت فعال (اختیاری)</param>
    /// <returns>تعداد همه انتصابات پزشکان به سرفصل‌های خدماتی</returns>
    Task<int> GetAllDoctorServiceCategoriesCountAsync(string searchTerm, int? doctorId, int? serviceCategoryId, bool? isActive);

    #endregion

    #region Department Management (مدیریت دپارتمان‌ها)

    /// <summary>
    /// دریافت دپارتمان‌های مرتبط با پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>لیست دپارتمان‌های مرتبط با پزشک</returns>
    Task<List<Department>> GetDoctorDepartmentsAsync(int doctorId);

    /// <summary>
    /// دریافت سرفصل‌های خدماتی مرتبط با دپارتمان
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>لیست سرفصل‌های خدماتی مرتبط با دپارتمان</returns>
    Task<List<ServiceCategory>> GetServiceCategoriesByDepartmentAsync(int departmentId);

    #endregion
}