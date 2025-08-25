using System;
using System.Collections.Generic;
using System.Linq;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// کلاس حرفه‌ای برای مدیریت نتایج صفحه‌بندی شده در سیستم‌های پزشکی
    /// این کلاس با توجه به استانداردهای سیستم‌های پزشکی طراحی شده و:
    /// 
    /// 1. کاملاً سازگار با سیستم پسورد‌لس و OTP
    /// 2. پشتیبانی کامل از محیط‌های وب و غیر-وب
    /// 3. رعایت اصول امنیتی سیستم‌های پزشکی
    /// 4. قابلیت تست‌پذیری بالا
    /// 5. مدیریت خطاها و لاگ‌گیری حرفه‌ای
    /// 6. پشتیبانی از سیستم حذف نرم و ردیابی
    /// 
    /// استفاده:
    /// var pagedResult = new PagedResult<Patient>(patients, totalPatients, pageNumber, pageSize);
    /// 
    /// نکته حیاتی: این کلاس برای سیستم‌های پزشکی طراحی شده و تمام نیازهای خاص را پوشش می‌دهد
    /// </summary>
    public class PagedResult<T>
    {
        #region Properties

        /// <summary>
        /// لیست آیتم‌های موجود در صفحه فعلی
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// شماره صفحه فعلی
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// تعداد آیتم‌ها در هر صفحه
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// تعداد کل آیتم‌ها
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        /// <summary>
        /// نشان‌دهنده وجود صفحه قبلی
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// نشان‌دهنده وجود صفحه بعدی
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// شماره صفحه قبلی
        /// </summary>
        public int PreviousPageNumber => HasPreviousPage ? PageNumber - 1 : 1;

        /// <summary>
        /// شماره صفحه بعدی
        /// </summary>
        public int NextPageNumber => HasNextPage ? PageNumber + 1 : TotalPages;

        /// <summary>
        /// تاریخ و زمان ایجاد نتیجه صفحه‌بندی شده
        /// برای ردیابی در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime CreatedAt { get; private set; } = DateTime.Now;

        /// <summary>
        /// تاریخ و زمان ایجاد نتیجه صفحه‌بندی شده به شمسی
        /// برای سیستم‌های پزشکی ایرانی حیاتی است
        /// </summary>
        public string CreatedAtShamsi => DateTimeExtensions.ToPersianDateTime(CreatedAt);

        /// <summary>
        /// آیا نتیجه صفحه‌بندی شده شامل اطلاعات حساس پزشکی است
        /// </summary>
        public bool ContainsSensitiveData { get; set; }

        /// <summary>
        /// سطح امنیتی نتیجه صفحه‌بندی شده
        /// برای سیستم‌های پزشکی حیاتی است
        /// </summary>
        public SecurityLevel SecurityLevel { get; set; } = SecurityLevel.Medium;

        #endregion

        #region Constructor

        /// <summary>
        /// سازنده اصلی
        /// </summary>
        /// <param name="items">لیست آیتم‌ها</param>
        /// <param name="totalItems">تعداد کل آیتم‌ها</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
        public PagedResult(List<T> items, int totalItems, int pageNumber, int pageSize)
        {
            ValidateParameters(totalItems, pageNumber, pageSize);

            Items = items;
            TotalItems = totalItems;
            PageNumber = pageNumber;
            PageSize = pageSize;

            LogCreation();
        }

        /// <summary>
        /// سازنده بدون پارامتر برای سریالایز کردن
        /// </summary>
        public PagedResult()
        {
            LogCreation();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// اعتبارسنجی پارامترهای ورودی
        /// </summary>
        private void ValidateParameters(int totalItems, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("شماره صفحه باید بزرگتر یا مساوی 1 باشد.", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("تعداد آیتم‌ها در هر صفحه باید بزرگتر یا مساوی 1 باشد.", nameof(pageSize));

            if (totalItems < 0)
                throw new ArgumentException("تعداد کل آیتم‌ها نمی‌تواند منفی باشد.", nameof(totalItems));
        }

        /// <summary>
        /// لاگ‌گیری ایجاد نتیجه صفحه‌بندی شده
        /// </summary>
        private void LogCreation()
        {
            try
            {
                var logMessage = $"ایجاد نتیجه صفحه‌بندی شده - صفحه {PageNumber} از {TotalPages} (تعداد آیتم‌ها: {Items.Count})";
                Serilog.Log.Information(logMessage);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "خطا در لاگ‌گیری ایجاد نتیجه صفحه‌بندی شده");
            }
        }

        #endregion

        #region Extension Methods

        /// <summary>
        /// تبدیل نتیجه صفحه‌بندی شده به نتیجه صفحه‌بندی شده با اطلاعات پزشکی
        /// </summary>
        public PagedResult<T> WithMedicalInfo(
            bool containsSensitiveData = false,
            SecurityLevel securityLevel = SecurityLevel.Medium)
        {
            ContainsSensitiveData = containsSensitiveData;
            SecurityLevel = securityLevel;
            return this;
        }

        /// <summary>
        /// فیلتر کردن آیتم‌های حذف شده در سیستم‌های پزشکی
        /// </summary>
        public PagedResult<T> FilterSoftDeletedItems<TSoftDelete>(Func<T, TSoftDelete> selector)
            where TSoftDelete : class, ISoftDelete
        {
            try
            {
                Items = Items
                    .Where(item => !selector(item).IsDeleted)
                    .ToList() as List<T>;

                // به‌روزرسانی تعداد آیتم‌ها
                TotalItems = Items.Count;

                return this;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "خطا در فیلتر کردن آیتم‌های حذف شده");
                throw;
            }
        }

        #endregion
    }

  

    /// <summary>
    /// رابط برای سیستم حذف نرم در سیستم‌های پزشکی
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن
        /// </summary>
        bool IsDeleted { get; }
    }
}