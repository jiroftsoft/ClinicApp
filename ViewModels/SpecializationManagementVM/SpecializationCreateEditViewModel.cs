using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using FluentValidation;

namespace ClinicApp.ViewModels.SpecializationManagementVM
{
    /// <summary>
    /// مدل ایجاد و ویرایش تخصص‌ها
    /// </summary>
    public class SpecializationCreateEditViewModel
    {
        /// <summary>
        /// شناسه تخصص (در حالت ایجاد جدید، این مقدار صفر است)
        /// </summary>
        public int SpecializationId { get; set; }

        /// <summary>
        /// نام تخصص
        /// </summary>
        [Required(ErrorMessage = "نام تخصص الزامی است.")]
        [StringLength(100, ErrorMessage = "نام تخصص نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نام تخصص")]
        public string Name { get; set; }

        /// <summary>
        /// توضیحات تخصص
        /// </summary>
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن
        /// </summary>
        [Display(Name = "فعال است")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        [Range(0, 1000, ErrorMessage = "ترتیب نمایش باید بین 0 تا 1000 باشد.")]
        [Display(Name = "ترتیب نمایش")]
        public int DisplayOrder { get; set; } = 0;

        #region فیلدهای ردیابی (Audit Trail)

        /// <summary>
        /// تاریخ و زمان ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// نام کاربر ایجاد کننده
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// نام کاربر آخرین ویرایش کننده
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// تاریخ ایجاد به فرمت شمسی
        /// </summary>
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش به فرمت شمسی
        /// </summary>
        public string UpdatedAtShamsi { get; set; }

        #endregion

        /// <summary>
        /// Factory Method برای تبدیل Entity به ViewModel
        /// </summary>
        public static SpecializationCreateEditViewModel FromEntity(Specialization specialization)
        {
            if (specialization == null) return null;

            return new SpecializationCreateEditViewModel
            {
                SpecializationId = specialization.SpecializationId,
                Name = specialization.Name,
                Description = specialization.Description,
                IsActive = specialization.IsActive,
                DisplayOrder = specialization.DisplayOrder,
                CreatedAt = specialization.CreatedAt,
                CreatedBy = specialization.CreatedByUser?.FullName ?? specialization.CreatedByUserId,
                UpdatedAt = specialization.UpdatedAt,
                UpdatedBy = specialization.UpdatedByUser?.FullName ?? specialization.UpdatedByUserId,
                CreatedAtShamsi = specialization.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = specialization.UpdatedAt?.ToPersianDateTime()
            };
        }

        /// <summary>
        /// تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// </summary>
        public Specialization ToEntity()
        {
            return new Specialization
            {
                SpecializationId = this.SpecializationId,
                Name = this.Name,
                Description = this.Description,
                IsActive = this.IsActive,
                DisplayOrder = this.DisplayOrder,
                CreatedAt = this.CreatedAt,
                CreatedByUserId = this.CreatedBy,
                UpdatedAt = this.UpdatedAt,
                UpdatedByUserId = this.UpdatedBy
            };
        }
    }

    /// <summary>
    /// اعتبارسنج برای مدل ایجاد و ویرایش تخصص
    /// </summary>
    public class SpecializationCreateEditViewModelValidator : AbstractValidator<SpecializationCreateEditViewModel>
    {
        public SpecializationCreateEditViewModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("نام تخصص الزامی است.")
                .MaximumLength(100)
                .WithMessage("نام تخصص نمی‌تواند بیش از 100 کاراکتر باشد.")
                .Matches(@"^[\u0600-\u06FF\s]+$")
                .WithMessage("نام تخصص باید به فارسی وارد شود.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.");

            RuleFor(x => x.DisplayOrder)
                .InclusiveBetween(0, 1000)
                .WithMessage("ترتیب نمایش باید بین 0 تا 1000 باشد.");
        }
    }
}
