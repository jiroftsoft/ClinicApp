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

    /// <summary>
    /// ولیدیتور برای مدل عملیات انتقال پزشک
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل عملیات انتقال پزشک
    /// 2. بررسی تفاوت دپارتمان مبدا و مقصد
    /// 3. رعایت استانداردهای پزشکی ایران
    /// 4. اعتبارسنجی قوانین کسب‌وکار
    /// </summary>
    public class DoctorTransferViewModelValidator : AbstractValidator<DoctorTransferViewModel>
    {
        public DoctorTransferViewModelValidator()
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

            // اعتبارسنجی شناسه دپارتمان مبدا
            RuleFor(x => x.FromDepartmentId)
                .GreaterThan(0)
                .WithMessage("شناسه دپارتمان مبدا نامعتبر است.")
                .WithErrorCode("INVALID_FROM_DEPARTMENT_ID");

            // اعتبارسنجی نام دپارتمان مبدا
            RuleFor(x => x.FromDepartmentName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.FromDepartmentName))
                .WithMessage("نام دپارتمان مبدا نمی‌تواند بیشتر از 100 کاراکتر باشد.")
                .WithErrorCode("FROM_DEPARTMENT_NAME_TOO_LONG");

            // اعتبارسنجی شناسه دپارتمان مقصد
            RuleFor(x => x.ToDepartmentId)
                .GreaterThan(0)
                .WithMessage("شناسه دپارتمان مقصد نامعتبر است.")
                .WithErrorCode("INVALID_TO_DEPARTMENT_ID");

            // اعتبارسنجی نام دپارتمان مقصد
            RuleFor(x => x.ToDepartmentName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.ToDepartmentName))
                .WithMessage("نام دپارتمان مقصد نمی‌تواند بیشتر از 100 کاراکتر باشد.")
                .WithErrorCode("TO_DEPARTMENT_NAME_TOO_LONG");

            // اعتبارسنجی تفاوت دپارتمان مبدا و مقصد
            RuleFor(x => x)
                .Must(x => x.FromDepartmentId != x.ToDepartmentId)
                .WithMessage("دپارتمان مبدا و مقصد نمی‌توانند یکسان باشند.")
                .WithErrorCode("SAME_DEPARTMENTS");

            // اعتبارسنجی توضیحات انتقال
            RuleFor(x => x.TransferReason)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.TransferReason))
                .WithMessage("توضیحات انتقال نمی‌تواند بیشتر از 500 کاراکتر باشد.")
                .WithErrorCode("TRANSFER_REASON_TOO_LONG");

            // اعتبارسنجی اجباری بودن توضیحات انتقال
            RuleFor(x => x.TransferReason)
                .NotEmpty()
                .WithMessage("دلیل انتقال الزامی است.")
                .WithErrorCode("TRANSFER_REASON_REQUIRED");
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل عملیات حذف انتسابات پزشک
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل عملیات حذف انتسابات
    /// 2. بررسی وابستگی‌ها قبل از حذف
    /// 3. رعایت استانداردهای پزشکی ایران
    /// 4. اعتبارسنجی قوانین کسب‌وکار
    /// </summary>
    public class DoctorAssignmentRemovalViewModelValidator : AbstractValidator<DoctorAssignmentRemovalViewModel>
    {
        public DoctorAssignmentRemovalViewModelValidator()
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

            // اعتبارسنجی دلیل حذف
            RuleFor(x => x.RemovalReason)
                .NotEmpty()
                .WithMessage("دلیل حذف انتسابات الزامی است.")
                .WithErrorCode("REMOVAL_REASON_REQUIRED");

            // اعتبارسنجی طول دلیل حذف
            RuleFor(x => x.RemovalReason)
                .MaximumLength(500)
                .WithMessage("دلیل حذف نمی‌تواند بیشتر از 500 کاراکتر باشد.")
                .WithErrorCode("REMOVAL_REASON_TOO_LONG");

            // اعتبارسنجی بررسی وابستگی‌ها
            RuleFor(x => x.DependenciesChecked)
                .Equal(true)
                .WithMessage("وابستگی‌ها باید قبل از حذف بررسی شوند.")
                .WithErrorCode("DEPENDENCIES_NOT_CHECKED");

            // اعتبارسنجی وجود انتسابات فعال
            RuleFor(x => x.HasActiveAssignments)
                .Equal(true)
                .WithMessage("پزشک باید دارای انتسابات فعال باشد تا بتوان آن‌ها را حذف کرد.")
                .WithErrorCode("NO_ACTIVE_ASSIGNMENTS_TO_REMOVE");

            // اعتبارسنجی تعداد انتسابات فعال
            RuleFor(x => x.ActiveAssignmentsCount)
                .GreaterThan(0)
                .WithMessage("تعداد انتسابات فعال باید بیشتر از صفر باشد.")
                .WithErrorCode("INVALID_ACTIVE_ASSIGNMENTS_COUNT");
        }
    }
}
