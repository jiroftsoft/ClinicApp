using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل جستجوی پزشکان برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی کامل از فیلترهای جستجوی پیشرفته برای پزشکان
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت پزشکان
    /// 3. پشتیبانی از صفحه‌بندی و جستجو برای عملکرد بهینه
    /// 4. پشتیبانی از فیلتر بر اساس کلینیک و دپارتمان
    /// 5. پشتیبانی از جستجو بر اساس نام، تخصص و وضعیت فعال/غیرفعال
    /// </summary>
    public class DoctorSearchViewModel
    {
        /// <summary>
        /// شناسه کلینیک برای فیلتر (اختیاری)
        /// </summary>
        public int? ClinicId { get; set; }

        /// <summary>
        /// شناسه دپارتمان برای فیلتر (اختیاری)
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// عبارت جستجو برای نام، نام خانوادگی یا تخصص پزشک
        /// </summary>
        [StringLength(100, ErrorMessage = "عبارت جستجو نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string SearchTerm { get; set; }

        /// <summary>
        /// شماره صفحه برای صفحه‌بندی
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید مثبت باشد")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// تعداد آیتم‌ها در هر صفحه
        /// </summary>
        [Range(1, 100, ErrorMessage = "تعداد آیتم‌ها در هر صفحه باید بین 1 تا 100 باشد")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// فیلتر بر اساس وضعیت فعال/غیرفعال پزشک
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// فیلتر بر اساس وضعیت حذف شده/عدم حذف شده
        /// </summary>
        public bool? IsDeleted { get; set; }

        /// <summary>
        /// فیلتر بر اساس تخصص پزشک
        /// </summary>
        public string Specialization { get; set; }

        /// <summary>
        /// فیلد مرتب‌سازی
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// ترتیب مرتب‌سازی (asc/desc)
        /// </summary>
        public string SortOrder { get; set; } = "asc";
    }

    /// <summary>
    /// ولیدیتور برای مدل جستجوی پزشکان
    /// </summary>
    public class DoctorSearchViewModelValidator : AbstractValidator<DoctorSearchViewModel>
    {
        public DoctorSearchViewModelValidator()
        {
            RuleFor(x => x.ClinicId)
                .GreaterThan(0)
                .When(x => x.ClinicId.HasValue)
                .WithMessage("شناسه کلینیک نامعتبر است.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .When(x => x.DepartmentId.HasValue)
                .WithMessage("شناسه دپارتمان نامعتبر است.");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage("عبارت جستجو نمی‌تواند بیش از 100 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("شماره صفحه باید مثبت باشد.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("تعداد آیتم‌ها در هر صفحه باید بین 1 تا 100 باشد.");
        }
    }
}
