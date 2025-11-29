using ClinicApp.ViewModels.ClinicAdmin;
using FluentValidation;
using System.Text.RegularExpressions;

namespace ClinicApp.ViewModels.Validators
{
    /// <summary>
    /// Validator برای ClinicBankAccountCreateEditViewModel
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. Validation کامل شماره شبا (IBAN)
    /// 2. Validation فیلدهای الزامی
    /// 3. Validation طول فیلدها
    /// 4. Validation فرمت شماره شبا
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط Validation
    /// ✅ Separation of Concerns: جدا از Service و Controller
    /// ✅ Reusability: قابل استفاده در Service و Controller
    /// </summary>
    public class ClinicBankAccountCreateEditViewModelValidator : AbstractValidator<ClinicBankAccountCreateEditViewModel>
    {
        public ClinicBankAccountCreateEditViewModelValidator()
        {
            // Validation برای ClinicId
            RuleFor(x => x.ClinicId)
                .GreaterThan(0)
                .WithMessage("کلینیک الزامی است.");

            // Validation برای IbanNumber
            RuleFor(x => x.IbanNumber)
                .NotEmpty()
                .WithMessage("شماره شبا الزامی است.")
                .Length(26)
                .WithMessage("شماره شبا باید 26 کاراکتر باشد.")
                .Must(BeValidIbanFormat)
                .WithMessage("فرمت شماره شبا نامعتبر است. باید با IR شروع شود و 24 رقم داشته باشد.")
                .Must(BeValidIbanChecksum)
                .WithMessage("شماره شبا نامعتبر است. لطفاً شماره شبا را بررسی کنید.");

            // Validation برای BankName
            RuleFor(x => x.BankName)
                .NotEmpty()
                .WithMessage("نام بانک الزامی است.")
                .MaximumLength(100)
                .WithMessage("نام بانک نمی‌تواند بیشتر از 100 کاراکتر باشد.");

            // Validation برای AccountNumber
            RuleFor(x => x.AccountNumber)
                .MaximumLength(50)
                .WithMessage("شماره حساب نمی‌تواند بیشتر از 50 کاراکتر باشد.")
                .When(x => !string.IsNullOrWhiteSpace(x.AccountNumber));

            // Validation برای AccountHolderName
            RuleFor(x => x.AccountHolderName)
                .NotEmpty()
                .WithMessage("نام صاحب حساب الزامی است.")
                .MaximumLength(200)
                .WithMessage("نام صاحب حساب نمی‌تواند بیشتر از 200 کاراکتر باشد.");

            // Validation برای Description
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }

        /// <summary>
        /// بررسی فرمت شماره شبا
        /// فرمت صحیح: IR + 24 رقم
        /// </summary>
        private bool BeValidIbanFormat(string ibanNumber)
        {
            if (string.IsNullOrWhiteSpace(ibanNumber))
                return false;

            // فرمت: IR + 24 رقم
            var pattern = @"^IR\d{24}$";
            return Regex.IsMatch(ibanNumber, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// بررسی Checksum شماره شبا (الگوریتم MOD-97-10 استاندارد IBAN)
        /// 
        /// الگوریتم صحیح:
        /// 1. 4 کاراکتر اول را به انتها منتقل می‌کنیم
        /// 2. حروف را به اعداد تبدیل می‌کنیم (A=10, B=11, ..., Z=35)
        /// 3. عدد بزرگ را MOD 97 محاسبه می‌کنیم
        /// 4. باقیمانده باید 1 باشد
        /// </summary>
        private bool BeValidIbanChecksum(string ibanNumber)
        {
            if (string.IsNullOrWhiteSpace(ibanNumber) || ibanNumber.Length != 26)
                return false;

            try
            {
                // تبدیل به uppercase برای اطمینان
                ibanNumber = ibanNumber.ToUpper();

                // مرحله 1: 4 کاراکتر اول را به انتها منتقل می‌کنیم
                var rearranged = ibanNumber.Substring(4) + ibanNumber.Substring(0, 4);

                // مرحله 2: تبدیل حروف به اعداد (A=10, B=11, ..., Z=35)
                var numericString = "";
                foreach (char c in rearranged)
                {
                    if (char.IsLetter(c))
                    {
                        // A=10, B=11, ..., Z=35
                        numericString += (c - 'A' + 10).ToString();
                    }
                    else if (char.IsDigit(c))
                    {
                        numericString += c;
                    }
                    else
                    {
                        return false; // کاراکتر نامعتبر
                    }
                }

                // مرحله 3: محاسبه MOD 97
                // برای اعداد بزرگ، از روش تقسیم مرحله‌ای استفاده می‌کنیم
                var remainder = 0;
                for (int i = 0; i < numericString.Length; i++)
                {
                    remainder = (remainder * 10 + int.Parse(numericString[i].ToString())) % 97;
                }

                // مرحله 4: باقیمانده باید 1 باشد
                return remainder == 1;
            }
            catch
            {
                return false;
            }
        }
    }
}

