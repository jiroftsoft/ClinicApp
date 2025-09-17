using ClinicApp.Helpers;
using ClinicApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس مدیریت خدمات پزشکی با رعایت کامل استانداردهای سیستم‌های پزشکی و امنیت اطلاعات
    /// این اینترفیس تمام عملیات مربوط به خدمات پزشکی را تعریف می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. رعایت کامل اصول Soft Delete برای حفظ اطلاعات پزشکی (مطابق استانداردهای قانونی ایران)
    /// 2. پیاده‌سازی سیستم ردیابی کامل (Audit Trail) با ذخیره اطلاعات کاربر انجام‌دهنده عملیات
    /// 3. استفاده از زمان UTC برای تمام تاریخ‌ها به منظور رعایت استانداردهای بین‌المللی
    /// 4. مدیریت تراکنش‌های پایگاه داده برای اطمینان از یکپارچگی داده‌ها
    /// 5. اعمال قوانین کسب‌وکار پزشکی در تمام سطوح
    /// 6. پشتیبانی کامل از Dependency Injection برای افزایش قابلیت تست و نگهداری
    /// 7. مدیریت خطاها با پیام‌های کاربرپسند و لاگ‌گیری حرفه‌ای
    /// 8. پشتیبانی کامل از محیط‌های ایرانی با تبدیل تاریخ‌ها به شمسی
    /// 9. ارائه امکانات پیشرفته برای سیستم‌های پزشکی ایرانی
    /// </summary>
    public interface IServiceService
    {
        /// <summary>
        /// ایجاد یک خدمات جدید با رعایت تمام استانداردهای امنیتی و پزشکی
        /// </summary>
        /// <param name="model">مدل اطلاعات خدمات جدید</param>
        /// <returns>نتیجه عملیات با شناسه خدمات ایجاد شده</returns>
        Task<ServiceResult<int>> CreateServiceAsync(ServiceCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی خدمات موجود با رعایت تمام استانداردهای امنیتی و پزشکی
        /// </summary>
        /// <param name="model">مدل اطلاعات به‌روزرسانی شده خدمات</param>
        /// <returns>نتیجه عملیات به‌روزرسانی</returns>
        Task<ServiceResult> UpdateServiceAsync(ServiceCreateEditViewModel model);

        /// <summary>
        /// حذف نرم (Soft-delete) یک خدمات با رعایت تمام استانداردهای پزشکی و حفظ اطلاعات مالی
        /// این عملیات تمام رکوردهای مرتبط را نیز به صورت نرم حذف می‌کند
        /// </summary>
        /// <param name="serviceId">شناسه خدمات مورد نظر</param>
        /// <returns>نتیجه عملیات حذف</returns>
        Task<ServiceResult> DeleteServiceAsync(int serviceId);

        /// <summary>
        /// بازیابی داده‌های یک خدمات برای پر کردن فرم ویرایش
        /// این متد فقط خدمات‌های فعال (غیرحذف شده) را بازمی‌گرداند
        /// </summary>
        /// <param name="serviceId">شناسه خدمات مورد نظر</param>
        /// <returns>مدل داده‌های خدمات برای ویرایش</returns>
        Task<ServiceResult<ServiceCreateEditViewModel>> GetServiceForEditAsync(int serviceId);

        /// <summary>
        /// بازیابی جزئیات کامل یک خدمات برای نمایش اطلاعات
        /// این متد فقط خدمات‌های فعال (غیرحذف شده) را بازمی‌گرداند
        /// </summary>
        /// <param name="serviceId">شناسه خدمات مورد نظر</param>
        /// <returns>مدل جزئیات کامل خدمات</returns>
        Task<ServiceResult<ServiceDetailsViewModel>> GetServiceDetailsAsync(int serviceId);

        /// <summary>
        /// جستجو و صفحه‌بندی خدمات با رعایت فیلترهای ضروری و عملکرد بهینه
        /// این متد فقط خدمات‌های فعال (غیرحذف شده) را بازمی‌گرداند
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات برای فیلتر</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
        /// <returns>نتایج جستجو به همراه اطلاعات صفحه‌بندی</returns>
        Task<ServiceResult<PagedResult<ServiceIndexViewModel>>> SearchServicesAsync(
            string searchTerm,
            int? serviceCategoryId,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// بررسی وجود کد خدمات تکراری
        /// </summary>
        /// <param name="serviceCode">کد خدمات</param>
        /// <param name="serviceId">شناسه خدمات (برای ویرایش)</param>
        /// <returns>آیا کد خدمات تکراری است؟</returns>
        Task<bool> IsDuplicateServiceCodeAsync(string serviceCode, int serviceId = 0);

        /// <summary>
        /// بررسی اعتبار دسته‌بندی خدمات
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>آیا دسته‌بندی خدمات معتبر است؟</returns>
        Task<bool> IsServiceCategoryValidAsync(int serviceCategoryId);

        /// <summary>
        /// دریافت لیست خدمات فعال برای استفاده در کنترل‌های انتخاب
        /// </summary>
        /// <returns>لیست خدمات فعال</returns>
        Task<IEnumerable<ServiceSelectItem>> GetActiveServicesAsync();

        /// <summary>
        /// دریافت لیست خدمات فعال برای یک دسته‌بندی خاص
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>لیست خدمات فعال برای دسته‌بندی مورد نظر</returns>
        Task<IEnumerable<ServiceSelectItem>> GetActiveServicesByCategoryAsync(int serviceCategoryId);

        /// <summary>
        /// بررسی اینکه آیا خدمات قابل حذف است یا خیر
        /// </summary>
        /// <param name="serviceId">شناسه خدمات</param>
        /// <returns>آیا خدمات قابل حذف است؟</returns>
        Task<ServiceResult> CanDeleteServiceAsync(int serviceId);

        /// <summary>
        /// بازیابی تعداد استفاده‌های یک خدمات
        /// </summary>
        /// <param name="serviceId">شناسه خدمات</param>
        /// <returns>تعداد استفاده‌ها</returns>
        Task<int> GetUsageCountAsync(int serviceId);

        /// <summary>
        /// بازیابی درآمد کل ایجاد شده توسط یک خدمات
        /// </summary>
        /// <param name="serviceId">شناسه خدمات</param>
        /// <returns>درآمد کل</returns>
        Task<decimal> GetTotalRevenueAsync(int serviceId);

        /// <summary>
        /// بازیابی آمار استفاده از خدمات در بازه زمانی مشخص
        /// </summary>
        /// <param name="serviceId">شناسه خدمات</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار استفاده</returns>
        Task<ServiceUsageStatistics> GetUsageStatisticsAsync(int serviceId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// بررسی وجود خدمات فعال با شناسه مشخص
        /// </summary>
        /// <param name="serviceId">شناسه خدمات</param>
        /// <returns>آیا خدمات فعال وجود دارد؟</returns>
        Task<bool> IsActiveServiceExistsAsync(int serviceId);

        /// <summary>
        /// دریافت لیست خدمات پرطرفدار
        /// </summary>
        /// <param name="topCount">تعداد خدمات مورد نظر</param>
        /// <param name="daysBack">تعداد روزهای بررسی</param>
        /// <returns>لیست خدمات پرطرفدار</returns>
        Task<IEnumerable<TopServiceItem>> GetTopServicesAsync(int topCount = 10, int daysBack = 30);

        /// <summary>
        /// به‌روزرسانی قیمت خدمت بر اساس ServiceComponents
        /// این method بعد از ایجاد یا ویرایش ServiceComponents فراخوانی می‌شود
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult<decimal>> UpdateServicePriceAsync(int serviceId);
    }

    /// <summary>
    /// مدل ساده برای انتخاب خدمات در کنترل‌های انتخاب
    /// </summary>
    public class ServiceSelectItem
    {
        public int ServiceId { get; set; }
        public string Title { get; set; }
        public string ServiceCode { get; set; }
        public decimal Price { get; set; }
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryTitle { get; set; }
        public int UsageCount { get; set; }
        public string CategoryTitle { get; set; }
    }

   
    public class TopServiceItem
    {
        public int ServiceId { get; set; }
        public string Title { get; set; }
        public string ServiceCode { get; set; }
        public int UsageCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public string ServiceCategoryTitle { get; set; }
    }
}