using System.Threading;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using FluentValidation;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// FluentValidation Validator برای DepartmentCreateEditViewModel
    /// </summary>
    public class DepartmentCreateEditViewModelValidator : AbstractValidator<DepartmentCreateEditViewModel>
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentCreateEditViewModelValidator(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;

            // ✅ قوانین اعتبارسنجی نام دپارتمان
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام دپارتمان الزامی است.")
                .Length(2, 200).WithMessage("نام دپارتمان باید بین ۲ تا ۲۰۰ کاراکتر باشد.")
                .MustAsync(BeUniqueName).WithMessage("نام دپارتمان در این کلینیک تکراری است.");

            // ✅ قوانین اعتبارسنجی کلینیک
            RuleFor(x => x.ClinicId)
                .GreaterThan(0).WithMessage("انتخاب کلینیک الزامی است.");
        }

        /// <summary>
        /// بررسی یکتا بودن نام دپارتمان در یک کلینیک
        /// </summary>
        private async Task<bool> BeUniqueName(DepartmentCreateEditViewModel model, string name, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true; // خطای Empty در rule دیگری چک می‌شود

            try
            {
                // بررسی تکراری نبودن نام در همان کلینیک
                var exists = await _departmentRepository.DoesDepartmentExistAsync(model.ClinicId, name.Trim(), model.DepartmentId);
                return !exists;
            }
            catch
            {
                // در صورت خطا، اجازه ادامه فرآیند را می‌دهیم
                return true;
            }
        }
    }
}
