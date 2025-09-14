using ClinicApp.Helpers;
using ClinicApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس مدیریت دسته‌بندی‌های خدمات پزشکی
    /// این اینترفیس تمام عملیات مربوط به دسته‌بندی‌های خدمات پزشکی را تعریف می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. رعایت کامل اصول Soft Delete برای حفظ اطلاعات پزشکی (مطابق استانداردهای قانونی ایران)
    /// 2. پیاده‌سازی سیستم ردیابی کامل (Audit Trail) با ذخیره اطلاعات کاربر انجام‌دهنده عملیات
    /// 3. استفاده از زمان UTC برای تمام تاریخ‌ها به منظور رعایت استانداردهای بین‌المللی
    /// 4. مدیریت تراکنش‌های پایگاه داده برای اطمینان از یکپارچگی داده‌ها
    /// 5. اعمال قوانین کسب‌وکار پزشکی در تمام سطوح
    /// 6. پشتیبانی کامل از Dependency Injection برای افزایش قابلیت تست و نگهداری
    /// 7. مدیریت خطاها با پیام‌های کاربرپسند و لاگ‌گیری حرفه‌ای
    /// </summary>
    public interface IServiceCategoryService
    {
        /// <summary>
        /// ایجاد یک دسته‌بندی خدمات جدید با رعایت تمام استانداردهای امنیتی و پزشکی
        /// </summary>
        /// <param name="model">مدل اطلاعات دسته‌بندی خدمات جدید</param>
        /// <returns>نتیجه عملیات با شناسه دسته‌بندی ایجاد شده</returns>
        Task<ServiceResult<int>> CreateServiceCategoryAsync(ServiceCategoryCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی دسته‌بندی خدمات موجود با رعایت تمام استانداردهای امنیتی و پزشکی
        /// </summary>
        /// <param name="model">مدل اطلاعات به‌روزرسانی شده دسته‌بندی خدمات</param>
        /// <returns>نتیجه عملیات به‌روزرسانی</returns>
        Task<ServiceResult> UpdateServiceCategoryAsync(ServiceCategoryCreateEditViewModel model);

        /// <summary>
        /// حذف نرم (Soft-delete) یک دسته‌بندی خدمات با رعایت تمام استانداردهای پزشکی و حفظ اطلاعات مالی
        /// این عملیات تمام خدمات مرتبط را نیز به صورت نرم حذف می‌کند
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات مورد نظر</param>
        /// <returns>نتیجه عملیات حذف</returns>
        Task<ServiceResult> DeleteServiceCategoryAsync(int serviceCategoryId);

        /// <summary>
        /// بازیابی داده‌های یک دسته‌بندی خدمات برای پر کردن فرم ویرایش
        /// این متد فقط دسته‌بندی‌های فعال (غیرحذف شده) را بازمی‌گرداند
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات مورد نظر</param>
        /// <returns>مدل داده‌های دسته‌بندی خدمات برای ویرایش</returns>
        Task<ServiceResult<ServiceCategoryCreateEditViewModel>> GetServiceCategoryForEditAsync(int serviceCategoryId);

        /// <summary>
        /// بازیابی جزئیات کامل یک دسته‌بندی خدمات برای نمایش اطلاعات
        /// این متد فقط دسته‌بندی‌های فعال (غیرحذف شده) را بازمی‌گرداند
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات مورد نظر</param>
        /// <returns>مدل جزئیات کامل دسته‌بندی خدمات</returns>
        Task<ServiceResult<ServiceCategoryDetailsViewModel>> GetServiceCategoryDetailsAsync(int serviceCategoryId);

        /// <summary>
        /// جستجو و صفحه‌بندی دسته‌بندی‌های خدمات با رعایت فیلترهای ضروری و عملکرد بهینه
        /// این متد فقط دسته‌بندی‌های فعال (غیرحذف شده) را بازمی‌گرداند
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="departmentId">شناسه دپارتمان برای فیلتر</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
        /// <returns>نتایج جستجو به همراه اطلاعات صفحه‌بندی</returns>
        Task<ServiceResult<PagedResult<ServiceCategoryIndexItemViewModel>>> SearchServiceCategoriesAsync(string searchTerm, int? departmentId, int pageNumber, int pageSize);


        /// <summary>
        /// بررسی وجود دسته‌بندی خدمات با عنوان ویژه
        /// </summary>
        /// <param name="title">عنوان دسته‌بندی خدمات</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات (برای ویرایش)</param>
        /// <returns>آیا دسته‌بندی خدمات تکراری است؟</returns>
        Task<bool> IsDuplicateServiceCategoryNameAsync(string title, int departmentId, int serviceCategoryId = 0);

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات فعال برای استفاده در کنترل‌های انتخاب
        /// </summary>
        /// <returns>لیست دسته‌بندی‌های خدمات فعال</returns>
        Task<IEnumerable<ServiceCategorySelectItem>> GetActiveServiceCategoriesAsync();

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات فعال برای یک دپارتمان خاص
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <returns>لیست دسته‌بندی‌های خدمات فعال برای دپارتمان مورد نظر</returns>
        Task<List<Models.Entities.Clinic.ServiceCategory>> GetActiveServiceCategoriesByDepartmentAsync(int departmentId);

        /// <summary>
        /// بررسی اینکه آیا دسته‌بندی خدمات قابل حذف است یا خیر
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>آیا دسته‌بندی خدمات قابل حذف است؟</returns>
        /// <summary>
        /// بررسی امکان حذف دسته‌بندی خدمات
        /// </summary>
        /// <param name="id">شناسه دسته‌بندی خدمات</param>
        /// <returns>نتیجه عملیات به همراه اطلاعات امکان حذف</returns>
        Task<ServiceResult<bool>> CanDeleteServiceCategoryAsync(int id);
        /// <summary>
        /// دریافت تعداد خدمات مرتبط با یک دسته‌بندی
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <returns>تعداد خدمات مرتبط</returns>
        Task<int> GetServiceCountForCategoryAsync(int categoryId);

        /// <summary>
        /// بازیابی تعداد خدمات مرتبط با یک دسته‌بندی
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>تعداد خدمات مرتبط</returns>
        Task<int> GetServiceCountAsync(int serviceCategoryId);

        /// <summary>
        /// بازیابی تعداد خدمات فعال مرتبط با یک دسته‌بندی
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>تعداد خدمات فعال مرتبط</returns>
        Task<int> GetActiveServiceCountAsync(int serviceCategoryId);

        /// <summary>
        /// بررسی وجود دسته‌بندی خدمات فعال با شناسه مشخص
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>آیا دسته‌بندی خدمات فعال وجود دارد؟</returns>
        Task<bool> IsActiveServiceCategoryExistsAsync(int serviceCategoryId);

    }

    /// <summary>
    /// مدل ساده برای انتخاب دسته‌بندی‌های خدمات در کنترل‌های انتخاب
    /// </summary>
    public class ServiceCategorySelectItem
    {
        public int ServiceCategoryId { get; set; }
        public string Title { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int ServiceCount { get; set; }
    }
}