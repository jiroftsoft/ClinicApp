using System.Threading;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using FluentValidation;
using ClinicApp.ViewModels;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// FluentValidation Validator برای ServiceCategoryCreateEditViewModel
    /// رعایت استانداردهای محیط‌های پزشکی و Clean Architecture
    /// </summary>
    public class ServiceCategoryCreateEditViewModelValidator : AbstractValidator<ServiceCategoryCreateEditViewModel>
    {
        private readonly IServiceCategoryRepository _serviceCategoryRepository;

        public ServiceCategoryCreateEditViewModelValidator(IServiceCategoryRepository serviceCategoryRepository)
        {
            _serviceCategoryRepository = serviceCategoryRepository;

            // ✅ قوانین اعتبارسنجی عنوان دسته‌بندی
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان دسته‌بندی الزامی است.")
                .Length(2, 200).WithMessage("عنوان دسته‌بندی باید بین ۲ تا ۲۰۰ کاراکتر باشد.")
                .MustAsync(BeUniqueTitle).WithMessage("عنوان دسته‌بندی در این دپارتمان تکراری است.");

            // ✅ قوانین اعتبارسنجی دپارتمان
            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("انتخاب دپارتمان الزامی است.");

            // ✅ قوانین اعتبارسنجی توضیحات (اختیاری)
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("توضیحات نباید بیشتر از ۱۰۰۰ کاراکتر باشد.");
        }

        /// <summary>
        /// بررسی یکتا بودن عنوان دسته‌بندی در یک دپارتمان
        /// </summary>
        private async Task<bool> BeUniqueTitle(ServiceCategoryCreateEditViewModel model, string title, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(title))
                return true; // خطای Empty در rule دیگری چک می‌شود

            try
            {
                // بررسی تکراری نبودن عنوان در همان دپارتمان
                var exists = await _serviceCategoryRepository.DoesCategoryExistAsync(
                    model.DepartmentId, title.Trim(), model.ServiceCategoryId);
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
