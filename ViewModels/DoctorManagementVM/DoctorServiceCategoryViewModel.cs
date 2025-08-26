using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل داده‌های رابطه پزشک-دسته‌بندی خدمات برای سیستم‌های پزشکی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش جزئیات رابطه چند-به-چند بین پزشک و دسته‌بندی خدمات
    /// 2. مدیریت سطح صلاحیت‌ها و گواهی‌نامه‌ها برای پزشکان
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// </summary>
    public class DoctorServiceCategoryViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی خدمات
        /// </summary>
        public int ServiceCategoryId { get; set; }

        /// <summary>
        /// عنوان دسته‌بندی خدمات
        /// </summary>
        [Display(Name = "عنوان دسته‌بندی")]
        public string ServiceCategoryTitle { get; set; }

        /// <summary>
        /// شناسه دپارتمان مربوط به دسته‌بندی خدمات
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان مربوط به دسته‌بندی خدمات
        /// </summary>
        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// شناسه کلینیک مربوط به دپارتمان
        /// </summary>
        public int ClinicId { get; set; }

        /// <summary>
        /// نام کلینیک مربوط به دپارتمان
        /// </summary>
        [Display(Name = "نام کلینیک")]
        public string ClinicName { get; set; }

        /// <summary>
        /// سطح صلاحیت پزشک در این دسته خدمات
        /// مثال: "مبتدی"، "متوسط"، "پیشرفته"، "متخصص"
        /// </summary>
        [MaxLength(50, ErrorMessage = "سطح صلاحیت نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "سطح صلاحیت")]
        public string AuthorizationLevel { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن صلاحیت پزشک در این دسته خدمات
        /// </summary>
        [Display(Name = "فعال است")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ اعطای صلاحیت
        /// </summary>
        [Display(Name = "تاریخ اعطا")]
        public DateTime? GrantedDate { get; set; }

        /// <summary>
        /// تاریخ اعطای صلاحیت به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ اعطا (شمسی)")]
        public string GrantedDateShamsi { get; set; }

        /// <summary>
        /// تاریخ انقضای صلاحیت (در صورت وجود)
        /// </summary>
        [Display(Name = "تاریخ انقضا")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// تاریخ انقضای صلاحیت به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ انقضا (شمسی)")]
        public string ExpiryDateShamsi { get; set; }

        /// <summary>
        /// شماره گواهی نامه (در صورت وجود)
        /// </summary>
        [MaxLength(100, ErrorMessage = "شماره گواهی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "شماره گواهی")]
        public string CertificateNumber { get; set; }

        /// <summary>
        /// توضیحات اضافی درباره این صلاحیت
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Notes { get; set; }

        #region فیلدهای ردیابی (Audit Trail)

        /// <summary>
        /// تاریخ ایجاد رابطه پزشک-دسته‌بندی خدمات
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ ایجاد رابطه پزشک-دسته‌بندی خدمات به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر ایجاد کننده
        /// </summary>
        [Display(Name = "ایجاد شده توسط")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش رابطه پزشک-دسته‌بندی خدمات
        /// </summary>
        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش رابطه پزشک-دسته‌بندی خدمات به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ آخرین ویرایش (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر آخرین ویرایش کننده
        /// </summary>
        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedBy { get; set; }

        #endregion

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DoctorServiceCategoryViewModel FromEntity(DoctorServiceCategory doctorServiceCategory)
        {
            if (doctorServiceCategory == null) return null;
            return new DoctorServiceCategoryViewModel
            {
                DoctorId = doctorServiceCategory.DoctorId,
                DoctorName = doctorServiceCategory.Doctor?.FullName ?? $"{doctorServiceCategory.Doctor?.FirstName} {doctorServiceCategory.Doctor?.LastName}",
                ServiceCategoryId = doctorServiceCategory.ServiceCategoryId,
                ServiceCategoryTitle = doctorServiceCategory.ServiceCategory?.Title,
                DepartmentId = doctorServiceCategory.ServiceCategory?.DepartmentId ?? 0,
                DepartmentName = doctorServiceCategory.ServiceCategory?.Department?.Name,
                ClinicId = doctorServiceCategory.ServiceCategory?.Department?.ClinicId ?? 0,
                ClinicName = doctorServiceCategory.ServiceCategory?.Department?.Clinic?.Name,
                AuthorizationLevel = doctorServiceCategory.AuthorizationLevel,
                IsActive = doctorServiceCategory.IsActive,
                GrantedDate = doctorServiceCategory.GrantedDate,
                GrantedDateShamsi = doctorServiceCategory.GrantedDate?.ToPersianDateTime(),
                ExpiryDate = doctorServiceCategory.ExpiryDate,
                ExpiryDateShamsi = doctorServiceCategory.ExpiryDate?.ToPersianDateTime(),
                CertificateNumber = doctorServiceCategory.CertificateNumber,
                Notes = doctorServiceCategory.Notes,
                CreatedAt = doctorServiceCategory.CreatedAt,
                CreatedBy = doctorServiceCategory.CreatedByUser?.FullName ?? doctorServiceCategory.CreatedByUserId,
                CreatedAtShamsi = doctorServiceCategory.CreatedAt.ToPersianDateTime(),
                UpdatedAt = doctorServiceCategory.UpdatedAt,
                UpdatedBy = doctorServiceCategory.UpdatedByUser?.FullName ?? doctorServiceCategory.UpdatedByUserId,
                UpdatedAtShamsi = doctorServiceCategory.UpdatedAt?.ToPersianDateTime()
            };
        }

        /// <summary>
        /// ✅ تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// </summary>
        public DoctorServiceCategory ToEntity()
        {
            return new DoctorServiceCategory
            {
                DoctorId = this.DoctorId,
                ServiceCategoryId = this.ServiceCategoryId,
                AuthorizationLevel = this.AuthorizationLevel,
                IsActive = this.IsActive,
                GrantedDate = this.GrantedDate,
                ExpiryDate = this.ExpiryDate,
                CertificateNumber = this.CertificateNumber,
                Notes = this.Notes,
                CreatedAt = this.CreatedAt,
                CreatedByUserId = this.CreatedBy,
                UpdatedAt = this.UpdatedAt,
                UpdatedByUserId = this.UpdatedBy
            };
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل رابطه پزشک-دسته‌بندی خدمات
    /// </summary>
    public class DoctorServiceCategoryViewModelValidator : AbstractValidator<DoctorServiceCategoryViewModel>
    {
        public DoctorServiceCategoryViewModelValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.");

            RuleFor(x => x.ServiceCategoryId)
                .GreaterThan(0)
                .WithMessage("شناسه دسته‌بندی خدمات نامعتبر است.");

            RuleFor(x => x.AuthorizationLevel)
                .MaximumLength(50)
                .WithMessage("سطح صلاحیت نمی‌تواند بیش از 50 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.AuthorizationLevel));

            RuleFor(x => x.CertificateNumber)
                .MaximumLength(100)
                .WithMessage("شماره گواهی نمی‌تواند بیش از 100 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.CertificateNumber));

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleFor(x => x.GrantedDate)
                .LessThanOrEqualTo(x => x.ExpiryDate)
                .When(x => x.GrantedDate.HasValue && x.ExpiryDate.HasValue)
                .WithMessage("تاریخ اعطا نمی‌تواند بعد از تاریخ انقضا باشد.");

            RuleFor(x => x.ExpiryDate)
                .GreaterThanOrEqualTo(x => x.GrantedDate)
                .When(x => x.GrantedDate.HasValue && x.ExpiryDate.HasValue)
                .WithMessage("تاریخ انقضا نمی‌تواند قبل از تاریخ اعطا باشد.");
        }
    }
}
