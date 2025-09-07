using FluentValidation;
using ClinicApp.ViewModels.DoctorManagementVM;
using System.Linq;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// ولیدیتور برای مدل عملیات انتساب ترکیبی پزشک
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل عملیات انتساب ترکیبی
    /// 2. بررسی سازگاری دپارتمان و سرفصل‌های خدماتی
    /// 3. رعایت استانداردهای پزشکی ایران
    /// 4. اعتبارسنجی قوانین کسب‌وکار
    /// </summary>
    public class DoctorAssignmentOperationViewModelValidator : AbstractValidator<DoctorAssignmentOperationViewModel>
    {
        public DoctorAssignmentOperationViewModelValidator()
        {
            // اعتبارسنجی شناسه پزشک
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.")
                .WithErrorCode("INVALID_DOCTOR_ID");

            // اعتبارسنجی نام پزشک
            RuleFor(x => x.DoctorName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.DoctorName))
                .WithMessage("نام پزشک نمی‌تواند بیشتر از 100 کاراکتر باشد.")
                .WithErrorCode("DOCTOR_NAME_TOO_LONG");

            // اعتبارسنجی شناسه دپارتمان
            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("شناسه دپارتمان نامعتبر است.")
                .WithErrorCode("INVALID_DEPARTMENT_ID");

            // اعتبارسنجی نام دپارتمان
            RuleFor(x => x.DepartmentName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.DepartmentName))
                .WithMessage("نام دپارتمان نمی‌تواند بیشتر از 100 کاراکتر باشد.")
                .WithErrorCode("DEPARTMENT_NAME_TOO_LONG");

            // اعتبارسنجی لیست سرفصل‌های خدماتی
            RuleFor(x => x.ServiceCategoryIds)
                .NotNull()
                .WithMessage("لیست سرفصل‌های خدماتی نمی‌تواند خالی باشد.")
                .WithErrorCode("SERVICE_CATEGORY_IDS_NULL");

            // اعتبارسنجی عدم تکرار سرفصل‌های خدماتی
            RuleFor(x => x.ServiceCategoryIds)
                .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
                .WithMessage("سرفصل‌های خدماتی تکراری مجاز نیستند.")
                .WithErrorCode("DUPLICATE_SERVICE_CATEGORY_IDS");

            // اعتبارسنجی حداکثر تعداد سرفصل‌های خدماتی
            RuleFor(x => x.ServiceCategoryIds)
                .Must(ids => ids == null || ids.Count <= 10)
                .WithMessage("حداکثر 10 سرفصل خدماتی می‌توان انتخاب کرد.")
                .WithErrorCode("TOO_MANY_SERVICE_CATEGORIES");

            // اعتبارسنجی توضیحات
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد.")
                .WithErrorCode("DESCRIPTION_TOO_LONG");
        }
    }

 
}
