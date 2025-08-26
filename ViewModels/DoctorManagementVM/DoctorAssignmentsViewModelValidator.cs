using FluentValidation;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// ولیدیتور برای مدل انتسابات پزشک
    /// </summary>
    public class DoctorAssignmentsViewModelValidator : AbstractValidator<DoctorAssignmentsViewModel>
    {
        public DoctorAssignmentsViewModelValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.");

            RuleFor(x => x.DoctorDepartments)
                .NotNull()
                .WithMessage("لیست انتسابات دپارتمان‌ها نمی‌تواند خالی باشد.");

            RuleFor(x => x.DoctorServiceCategories)
                .NotNull()
                .WithMessage("لیست انتسابات سرفصل‌های خدماتی نمی‌تواند خالی باشد.");

            // اعتبارسنجی انتسابات دپارتمان‌ها
            RuleForEach(x => x.DoctorDepartments)
                .SetValidator(new DoctorDepartmentViewModelValidator());

            // اعتبارسنجی انتسابات سرفصل‌های خدماتی
            RuleForEach(x => x.DoctorServiceCategories)
                .SetValidator(new DoctorServiceCategoryViewModelValidator());
        }
    }
}
