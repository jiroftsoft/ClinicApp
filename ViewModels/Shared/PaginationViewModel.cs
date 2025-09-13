using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Shared
{
    /// <summary>
    /// ViewModel برای مدیریت اطلاعات صفحه‌بندی
    /// طبق اصل DRY - قابل استفاده مجدد در کل پروژه
    /// </summary>
    public class PaginationViewModel
    {
        /// <summary>
        /// صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// تعداد کل آیتم‌ها
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد آیتم‌ها در هر صفحه
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// نام Action برای لینک‌های صفحه‌بندی
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// نام Controller برای لینک‌های صفحه‌بندی
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        /// پارامترهای اضافی برای لینک‌های صفحه‌بندی
        /// </summary>
        public Dictionary<string, object> RouteValues { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// آیا صفحه‌بندی نمایش داده شود
        /// </summary>
        public bool ShowPagination => TotalPages > 1;

        /// <summary>
        /// آیا صفحه قبلی وجود دارد
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// آیا صفحه بعدی وجود دارد
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// شماره صفحه قبلی
        /// </summary>
        public int PreviousPage => CurrentPage - 1;

        /// <summary>
        /// شماره صفحه بعدی
        /// </summary>
        public int NextPage => CurrentPage + 1;

        /// <summary>
        /// محدوده صفحات برای نمایش (شروع)
        /// </summary>
        public int StartPage => Math.Max(1, CurrentPage - 2);

        /// <summary>
        /// محدوده صفحات برای نمایش (پایان)
        /// </summary>
        public int EndPage => Math.Min(TotalPages, CurrentPage + 2);

        /// <summary>
        /// اضافه کردن پارامتر به RouteValues
        /// </summary>
        public void AddRouteValue(string key, object value)
        {
            if (RouteValues == null)
                RouteValues = new Dictionary<string, object>();
            
            RouteValues[key] = value;
        }

        /// <summary>
        /// ایجاد RouteValues برای صفحه خاص
        /// </summary>
        public Dictionary<string, object> GetRouteValuesForPage(int page)
        {
            var routeValues = new Dictionary<string, object>(RouteValues);
            routeValues["page"] = page;
            return routeValues;
        }
    }
}
