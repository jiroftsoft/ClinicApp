using System.Threading;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using FluentValidation;
using ClinicApp.ViewModels;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// FluentValidation Validator برای ServiceCreateEditViewModel
    /// رعایت استانداردهای محیط‌های پزشکی و Clean Architecture
    /// </summary>
    public class ServiceCreateEditViewModelValidator : AbstractValidator<ServiceCreateEditViewModel>
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceCreateEditViewModelValidator(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;

            // ✅ قوانین اعتبارسنجی عنوان خدمت
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان خدمت الزامی است.")
                .Length(2, 200).WithMessage("عنوان خدمت باید بین ۲ تا ۲۰۰ کاراکتر باشد.");

            // ✅ قوانین اعتبارسنجی کد خدمت - Medical Environment (فقط اعداد)
            RuleFor(x => x.ServiceCode)
                .NotEmpty().WithMessage("کد خدمت الزامی است.")
                .Length(3, 10).WithMessage("کد خدمت باید بین ۳ تا ۱۰ رقم باشد.")
                .Matches("^\\d+$").WithMessage("کد خدمت باید فقط شامل اعداد باشد.")
                .MustAsync(BeUniqueServiceCode).WithMessage("کد خدمت در این دسته‌بندی تکراری است.");

            // ✅ قوانین اعتبارسنجی قیمت
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("قیمت خدمت نمی‌تواند منفی باشد.")
                .LessThanOrEqualTo(999999999).WithMessage("قیمت خدمت بیش از حد مجاز است.");

            // ✅ قوانین اعتبارسنجی دسته‌بندی خدمات
            RuleFor(x => x.ServiceCategoryId)
                .GreaterThan(0).WithMessage("انتخاب دسته‌بندی خدمات الزامی است.");

            // ✅ قوانین اعتبارسنجی توضیحات (اختیاری)
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("توضیحات نباید بیشتر از ۱۰۰۰ کاراکتر باشد.");
        }

        /// <summary>
        /// بررسی یکتا بودن کد خدمت در یک دسته‌بندی
        /// </summary>
        private async Task<bool> BeUniqueServiceCode(ServiceCreateEditViewModel model, string serviceCode, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                return true; // خطای Empty در rule دیگری چک می‌شود

            try
            {
                // بررسی تکراری نبودن کد در همان دسته‌بندی
                var exists = await _serviceRepository.DoesServiceExistAsync(
                    model.ServiceCategoryId, serviceCode.Trim(), model.ServiceId);
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
