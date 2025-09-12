using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Entities.Patient;

namespace ClinicApp.Interfaces;

/// <summary>
/// رابط حرفه‌ای برای دسترسی به اطلاعات کاربر جاری در تمام لایه‌های سیستم کلینیک شفا
/// این رابط با توجه به استانداردهای سیستم‌های پزشکی طراحی شده و:
/// 
/// 1. کاملاً سازگار با سیستم پسورد‌لس و OTP
/// 2. پشتیبانی کامل از محیط‌های وب و غیر-وب
/// 3. رعایت اصول امنیتی سیستم‌های پزشکی
/// 4. قابلیت تست‌پذیری بالا
/// 5. مدیریت خطاها و لاگ‌گیری حرفه‌ای
/// 6. پشتیبانی از سیستم حذف نرم و ردیابی
/// 
/// این رابط به همراه کلاس AppHelper کاملاً یکپارچه شده و برای سیستم‌های پزشکی طراحی شده است.
/// </summary>
public interface ICurrentUserService
{
    #region Core Properties (ویژگی‌های اصلی)

    /// <summary>
    /// شناسه کاربر جاری
    /// در سیستم‌های پزشکی، این شناسه برای ردیابی و امنیت حیاتی است
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// نام کاربری کاربر جاری (معمولاً کد ملی در سیستم‌های پزشکی ایرانی)
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// آیا کاربر جاری احراز هویت شده است؟
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// آیا کاربر جاری در نقش مدیر سیستم است؟
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// آیا کاربر جاری پزشک است؟
    /// </summary>
    bool IsDoctor { get; }

    /// <summary>
    /// آیا کاربر جاری منشی است؟
    /// </summary>
    bool IsReceptionist { get; }

    /// <summary>
    /// آیا کاربر جاری بیمار است؟
    /// </summary>
    bool IsPatient { get; }

    /// <summary>
    /// زمان فعلی سیستم به صورت UTC
    /// برای سیستم‌های پزشکی بسیار حیاتی است تا تمام تاریخ‌ها یکسان باشند
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// زمان فعلی سیستم به صورت محلی (با توجه به تنظیمات سیستم پزشکی)
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// ادعاها (Claims) کاربر جاری
    /// برای دسترسی به اطلاعات جزئی‌تر کاربر در سیستم‌های پزشکی
    /// </summary>
    ClaimsPrincipal ClaimsPrincipal { get; }

    /// <summary>
    /// بررسی اینکه آیا در محیط توسعه هستیم یا نه
    /// برای تشخیص خودکار محیط توسعه و استفاده از کاربر Admin
    /// </summary>
    bool IsDevelopmentEnvironment();

    #endregion

    #region Security Methods (روش‌های امنیتی)

    /// <summary>
    /// بررسی اینکه آیا کاربر جاری در نقش خاصی است؟
    /// </summary>
    /// <param name="role">نام نقش مورد نظر</param>
    /// <returns>آیا کاربر در این نقش است؟</returns>
    bool IsInRole(string role);

    /// <summary>
    /// بررسی دسترسی کاربر جاری به عملیات خاص
    /// </summary>
    /// <param name="permission">نام دسترسی مورد نظر</param>
    /// <returns>آیا کاربر به این عملیات دسترسی دارد؟</returns>
    bool HasPermission(string permission);

    /// <summary>
    /// بررسی دسترسی کاربر جاری به موجودیت خاص
    /// </summary>
    /// <typeparam name="TEntity">نوع موجودیت</typeparam>
    /// <param name="entity">موجودیت مورد نظر</param>
    /// <param name="permission">نام دسترسی مورد نظر</param>
    /// <returns>آیا کاربر به این موجودیت دسترسی دارد؟</returns>
    bool HasEntityAccess<TEntity>(TEntity entity, string permission) where TEntity : class;

    /// <summary>
    /// بررسی دسترسی کاربر جاری به سرویس پزشکی خاص
    /// </summary>
    /// <param name="serviceId">شناسه سرویس پزشکی</param>
    /// <returns>آیا کاربر به این سرویس دسترسی دارد؟</returns>
    Task<bool> HasAccessToServiceAsync(int serviceId);

    /// <summary>
    /// بررسی دسترسی کاربر جاری به بیمه خاص
    /// </summary>
    /// <param name="insuranceId">شناسه بیمه</param>
    /// <returns>آیا کاربر به این بیمه دسترسی دارد؟</returns>
    Task<bool> HasAccessToInsuranceAsync(int insuranceId);

    /// <summary>
    /// بررسی دسترسی کاربر جاری به دپارتمان خاص
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>آیا کاربر به این دپارتمان دسترسی دارد؟</returns>
    Task<bool> HasAccessToDepartmentAsync(int departmentId);

    #endregion

    #region Helper Methods (روش‌های کمکی)

    /// <summary>
    /// دریافت اطلاعات پزشک کاربر جاری (در صورتی که کاربر پزشک باشد)
    /// </summary>
    /// <returns>اطلاعات پزشک یا null</returns>
    Task<Doctor> GetDoctorInfoAsync();

    /// <summary>
    /// دریافت اطلاعات بیمار کاربر جاری (در صورتی که کاربر بیمار باشد)
    /// </summary>
    /// <returns>اطلاعات بیمار یا null</returns>
    Task<Patient> GetPatientInfoAsync();

    /// <summary>
    /// دریافت شناسه کاربر سیستم برای استفاده در محیط‌های پس‌زمینه
    /// </summary>
    /// <returns>شناسه کاربر سیستم</returns>
    string GetSystemUserId();

    /// <summary>
    /// دریافت لیست دپارتمان‌هایی که پزشک فعلی در آن‌ها فعال است
    /// </summary>
    /// <returns>لیست دپارتمان‌های فعال پزشک</returns>
    Task<List<Department>> GetDoctorActiveDepartmentsAsync();

    /// <summary>
    /// دریافت لیست دسته‌بندی خدماتی که پزشک فعلی مجاز به ارائه آن‌ها است
    /// </summary>
    /// <returns>لیست دسته‌بندی خدمات مجاز پزشک</returns>
    Task<List<ServiceCategory>> GetDoctorAuthorizedServiceCategoriesAsync();

    /// <summary>
    /// بررسی اینکه آیا پزشک فعلی در دپارتمان مشخص شده فعال است یا خیر
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>آیا پزشک در این دپارتمان فعال است؟</returns>
    Task<bool> IsDoctorActiveInDepartmentAsync(int departmentId);

    /// <summary>
    /// بررسی اینکه آیا پزشک فعلی مجاز به دسته‌بندی خدمات مشخص شده است یا خیر
    /// </summary>
    /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
    /// <returns>آیا پزشک مجاز به این دسته‌بندی خدمات است؟</returns>
    Task<bool> IsDoctorAuthorizedForServiceCategoryAsync(int serviceCategoryId);

    /// <summary>
    /// دریافت نقش پزشک در دپارتمان مشخص شده
    /// </summary>
    /// <param name="departmentId">شناسه دپارتمان</param>
    /// <returns>نقش پزشک در دپارتمان یا null</returns>
    Task<string> GetDoctorRoleInDepartmentAsync(int departmentId);

    /// <summary>
    /// دریافت آرایه نقش‌های کاربر جاری
    /// </summary>
    /// <returns>آرایه نقش‌های کاربر</returns>
    string[] GetUserRoles();

    /// <summary>
    /// دریافت شناسه کاربر جاری
    /// </summary>
    /// <returns>شناسه کاربر جاری</returns>
    string GetCurrentUserId();

    /// <summary>
    /// دریافت نام کاربر جاری
    /// </summary>
    /// <returns>نام کاربر جاری</returns>
    string GetCurrentUserName();

    #endregion
}