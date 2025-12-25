using System;

namespace ClinicApp.ViewModels.Shared
{
    /// <summary>
    /// ViewModel برای Breadcrumb Navigation
    /// مخصوص سیستم‌های درمانی با ساختارهای پیچیده
    /// </summary>
    public class BreadcrumbItem
    {
        /// <summary>
        /// عنوان نمایشی
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// URL برای لینک
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// آیکون FontAwesome (اختیاری)
        /// مثال: "fas fa-user-plus"
        /// </summary>
        public string Icon { get; set; }
        
        /// <summary>
        /// آیا این آیتم فعال است؟
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Tooltip یا توضیحات اضافی (اختیاری)
        /// </summary>
        public string Tooltip { get; set; }
    }
}

