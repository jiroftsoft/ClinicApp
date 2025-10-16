using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای بخش‌های آکاردئون
    /// </summary>
    public class AccordionSectionViewModel
    {
        /// <summary>
        /// شناسه بخش
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// عنوان بخش
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// آیکون بخش
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// آیا بخش باز است؟
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// آیا بخش معتبر است؟
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// وضعیت بخش
        /// </summary>
        public string Status { get; set; } = "Ready";

        /// <summary>
        /// پیام وضعیت
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// آیا بخش قابل ویرایش است؟
        /// </summary>
        public bool IsEditable { get; set; } = true;

        /// <summary>
        /// آیا بخش اجباری است؟
        /// </summary>
        public bool IsRequired { get; set; } = true;
    }

    /// <summary>
    /// ViewModel برای آیتم‌های دراپ‌دان
    /// </summary>
    public class DropdownItemViewModel
    {
        /// <summary>
        /// شناسه آیتم
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// نام آیتم
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// کد آیتم
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// آیا قابل انتخاب است؟
        /// </summary>
        public bool IsSelectable { get; set; } = true;

        /// <summary>
        /// نام نمایشی
        /// </summary>
        public string DisplayName => !string.IsNullOrEmpty(Code) ? $"{Code} - {Name}" : Name;
    }

    /// <summary>
    /// ViewModel برای جستجو
    /// </summary>
    public class SearchViewModel
    {
        /// <summary>
        /// عبارت جستجو
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// نوع جستجو
        /// </summary>
        public string SearchType { get; set; }

        /// <summary>
        /// فیلترهای اضافی
        /// </summary>
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// تعداد نتایج در هر صفحه
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// شماره صفحه
        /// </summary>
        public int PageNumber { get; set; } = 1;
    }

    /// <summary>
    /// ViewModel برای نتایج جستجو
    /// </summary>
    public class SearchResultViewModel<T>
    {
        /// <summary>
        /// نتایج جستجو
        /// </summary>
        public List<T> Results { get; set; } = new List<T>();

        /// <summary>
        /// تعداد کل نتایج
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد نتایج در صفحه فعلی
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// آیا نتایج بیشتری وجود دارد؟
        /// </summary>
        public bool HasMoreResults { get; set; }

        /// <summary>
        /// زمان جستجو (میلی‌ثانیه)
        /// </summary>
        public long SearchTime { get; set; }
    }

    /// <summary>
    /// ViewModel برای محاسبات
    /// </summary>
    public class CalculationViewModel
    {
        /// <summary>
        /// مبلغ پایه
        /// </summary>
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// درصد بیمه
        /// </summary>
        public decimal InsurancePercentage { get; set; }

        /// <summary>
        /// مبلغ بیمه
        /// </summary>
        public decimal InsuranceAmount { get; set; }

        /// <summary>
        /// مبلغ بیمار
        /// </summary>
        public decimal PatientAmount { get; set; }

        /// <summary>
        /// مبلغ کل
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// آیا محاسبه معتبر است؟
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// پیام محاسبه
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// ViewModel برای وضعیت‌ها
    /// </summary>
    public class StatusViewModel
    {
        /// <summary>
        /// نوع وضعیت
        /// </summary>
        public string Type { get; set; } = "Info";

        /// <summary>
        /// پیام وضعیت
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// آیکون وضعیت
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// رنگ وضعیت
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// آیا قابل بستن است؟
        /// </summary>
        public bool IsDismissible { get; set; } = true;

        /// <summary>
        /// زمان نمایش (میلی‌ثانیه)
        /// </summary>
        public int DisplayTime { get; set; } = 5000;
    }
}
