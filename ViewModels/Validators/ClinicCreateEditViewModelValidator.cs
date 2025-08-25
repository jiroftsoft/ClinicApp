using System.Threading;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using FluentValidation;

namespace ClinicApp.ViewModels.Validators;

public class ClinicCreateEditViewModelValidator : AbstractValidator<ClinicCreateEditViewModel>
{
    private readonly IClinicRepository _clinicRepository;

    public ClinicCreateEditViewModelValidator(IClinicRepository clinicRepository)
    {
        _clinicRepository = clinicRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام کلینیک الزامی است.")
            .MaximumLength(200).WithMessage("نام کلینیک نمی‌تواند بیش از ۲۰۰ کاراکتر باشد.")
            .MustAsync(BeUniqueName).WithMessage("نام کلینیک تکراری است.");

        RuleFor(x => x.PhoneNumber)
            .Must(PersianNumberHelper.IsValidGeneralPhoneNumber) // Changed from IsValidPhoneNumber
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("فرمت شماره تلفن نامعتبر است.");
    }

    private async Task<bool> BeUniqueName(ClinicCreateEditViewModel model, string name, CancellationToken token)
    {
        return !await _clinicRepository.DoesClinicExistAsync(name, model.ClinicId);
    }
}