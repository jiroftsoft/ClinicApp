using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// رابط سرویس مدیریت بیمه‌ها - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ایجاد، ویرایش و حذف نرم بیمه‌ها با رعایت استانداردهای قانونی سیستم‌های پزشکی ایران
    /// 2. مدیریت تعرفه‌های بیمه‌ای برای خدمات مختلف با قابلیت تعریف سهم‌های خاص برای هر خدمت
    /// 3. پیاده‌سازی کامل سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی مطابق با قوانین ایران
    /// 4. ارتباط دقیق با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی کامل عملیات‌های حساس
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط با استفاده از زمان UTC و تبدیل به شمسی
    /// 6. پشتیبانی از بیمه آزاد به عنوان پیش‌فرض برای بیماران بدون بیمه با مکانیزم‌های امنیتی
    /// 7. پشتیبانی کامل از محیط‌های پزشکی ایرانی با تاریخ شمسی و اعداد فارسی در تمام لایه‌ها
    /// 8. عدم استفاده از AutoMapper برای کنترل کامل بر روی داده‌ها و امنیت بالا
    /// 9. پیاده‌سازی مکانیزم‌های بهینه‌سازی عملکرد برای سیستم‌های پزشکی پراستفاده
    /// 10. رعایت کامل استانداردهای امنیتی و حفظ حریم خصوصی اطلاعات پزشکی
    /// </summary>
    public interface IInsuranceService
    {
        /// <summary>
        /// دریافت لیست بیمه‌های فعال
        /// </summary>
        Task<ServiceResult<List<InsuranceViewModel>>> GetActiveInsurancesAsync();

        /// <summary>
        /// دریافت جزئیات بیمه
        /// </summary>
        Task<ServiceResult<InsuranceDetailsViewModel>> GetInsuranceDetailsAsync(int insuranceId);

        /// <summary>
        /// دریافت لیست تعرفه‌های بیمه
        /// </summary>
        Task<ServiceResult<List<InsuranceTariffViewModel>>> GetInsuranceTariffsAsync(int insuranceId);

        /// <summary>
        /// دریافت اطلاعات تعرفه برای ویرایش
        /// </summary>
        Task<ServiceResult<EditInsuranceTariffViewModel>> GetInsuranceTariffForEditAsync(int tariffId);

        /// <summary>
        /// دریافت بیمه آزاد
        /// </summary>
        Task<ServiceResult<Insurance>> GetFreeInsuranceAsync();

        /// <summary>
        /// ایجاد بیمه جدید
        /// </summary>
        Task<ServiceResult> CreateInsuranceAsync(CreateInsuranceViewModel model);

        /// <summary>
        /// ویرایش بیمه
        /// </summary>
        Task<ServiceResult> UpdateInsuranceAsync(EditInsuranceViewModel model);

        /// <summary>
        /// حذف نرم بیمه
        /// </summary>
        Task<ServiceResult> DeleteInsuranceAsync(int insuranceId);

        /// <summary>
        /// ایجاد تعرفه بیمه
        /// </summary>
        Task<ServiceResult> CreateInsuranceTariffAsync(CreateInsuranceTariffViewModel model);

        /// <summary>
        /// ویرایش تعرفه بیمه
        /// </summary>
        Task<ServiceResult> UpdateInsuranceTariffAsync(EditInsuranceTariffViewModel model);

        /// <summary>
        /// حذف تعرفه بیمه
        /// </summary>
        Task<ServiceResult> DeleteInsuranceTariffAsync(int insuranceTariffId);
    }
}