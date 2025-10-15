using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش cascade loading خدمات در فرم پذیرش
    /// </summary>
    public class ReceptionServiceCascadeViewModel
    {
        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// شناسه سرفصل انتخاب شده
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// شناسه خدمت انتخاب شده
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// لیست سرفصل‌های دپارتمان
        /// </summary>
        public List<ReceptionServiceCategoryViewModel> Categories { get; set; } = new List<ReceptionServiceCategoryViewModel>();

        /// <summary>
        /// لیست خدمات سرفصل
        /// </summary>
        public List<ReceptionServiceViewModel> Services { get; set; } = new List<ReceptionServiceViewModel>();

        /// <summary>
        /// خدمت انتخاب شده
        /// </summary>
        public ReceptionServiceViewModel SelectedService { get; set; }

        /// <summary>
        /// تاریخ بارگذاری
        /// </summary>
        public DateTime LoadDate { get; set; }

        /// <summary>
        /// آیا دپارتمان انتخاب شده؟
        /// </summary>
        public bool IsDepartmentSelected => DepartmentId > 0;

        /// <summary>
        /// آیا سرفصل انتخاب شده؟
        /// </summary>
        public bool IsCategorySelected => CategoryId.HasValue && CategoryId.Value > 0;

        /// <summary>
        /// آیا خدمت انتخاب شده؟
        /// </summary>
        public bool IsServiceSelected => ServiceId.HasValue && ServiceId.Value > 0;

        /// <summary>
        /// تعداد سرفصل‌ها
        /// </summary>
        public int CategoryCount => Categories?.Count ?? 0;

        /// <summary>
        /// تعداد خدمات
        /// </summary>
        public int ServiceCount => Services?.Count ?? 0;

        /// <summary>
        /// نمایش وضعیت انتخاب
        /// </summary>
        public string SelectionStatus
        {
            get
            {
                if (!IsDepartmentSelected) return "دپارتمان انتخاب نشده";
                if (!IsCategorySelected) return "سرفصل انتخاب نشده";
                if (!IsServiceSelected) return "خدمت انتخاب نشده";
                return "انتخاب کامل";
            }
        }

        /// <summary>
        /// نمایش اطلاعات cascade (فرمات شده)
        /// </summary>
        public string CascadeInfoDisplay
        {
            get
            {
                var parts = new List<string>();
                
                if (IsDepartmentSelected) parts.Add($"دپارتمان: {DepartmentId}");
                if (IsCategorySelected && Categories.Any(c => c.CategoryId == CategoryId))
                {
                    var category = Categories.First(c => c.CategoryId == CategoryId);
                    parts.Add($"سرفصل: {category.CategoryName}");
                }
                if (IsServiceSelected && SelectedService != null)
                {
                    parts.Add($"خدمت: {SelectedService.ServiceName}");
                }
                
                return string.Join(" | ", parts);
            }
        }

        /// <summary>
        /// آیا cascade کامل است؟
        /// </summary>
        public bool IsComplete => IsDepartmentSelected && IsCategorySelected && IsServiceSelected;

        /// <summary>
        /// درصد تکمیل cascade
        /// </summary>
        public int CompletionPercentage
        {
            get
            {
                var steps = 0;
                if (IsDepartmentSelected) steps++;
                if (IsCategorySelected) steps++;
                if (IsServiceSelected) steps++;
                return (steps * 100) / 3;
            }
        }
    }
}
