using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Helpers;

public class PhoneNumberValidator : IIdentityValidator<string>
{
    private readonly ILogger _log = Log.ForContext<PhoneNumberValidator>();

    public Task<IdentityResult> ValidateAsync(string item)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return Task.FromResult(IdentityResult.Failed("شماره تلفن همراه نمی‌تواند خالی باشد."));
            }

            // نرمال‌سازی شماره تلفن
            var normalizedNumber = NormalizePhoneNumber(item);

            // بررسی معتبر بودن شماره تلفن
            if (!IsValidIranianPhoneNumber(normalizedNumber))
            {
                return Task.FromResult(IdentityResult.Failed("شماره تلفن همراه وارد شده معتبر نیست."));
            }

            return Task.FromResult(IdentityResult.Success);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در اعتبارسنجی شماره تلفن همراه: {PhoneNumber}", item);
            return Task.FromResult(IdentityResult.Failed("خطا در اعتبارسنجی شماره تلفن همراه."));
        }
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return phoneNumber;

        // تبدیل ارقام فارسی/عربی به انگلیسی
        var converted = new string(phoneNumber.Select(c =>
        {
            if (c >= '\u06F0' && c <= '\u06F9') // ارقام فارسی
                return (char)(c - '\u06F0' + '0');
            if (c >= '\u0660' && c <= '\u0669') // ارقام عربی
                return (char)(c - '\u0660' + '0');
            return c;
        }).ToArray());

        // حذف کاراکترهای غیرضروری
        var cleaned = new string(converted.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // تبدیل به فرمت بین‌المللی
        if (cleaned.StartsWith("0"))
        {
            cleaned = "+98" + cleaned.Substring(1);
        }
        else if (cleaned.StartsWith("98"))
        {
            cleaned = "+" + cleaned;
        }
        else if (cleaned.StartsWith("9") && cleaned.Length == 10)
        {
            cleaned = "+98" + cleaned;
        }

        return cleaned;
    }

    private bool IsValidIranianPhoneNumber(string phoneNumber)
    {
        // بررسی فرمت بین‌المللی
        if (!phoneNumber.StartsWith("+989") || phoneNumber.Length != 13)
            return false;

        // بررسی کدهای اپراتور
        var operatorCode = phoneNumber.Substring(4, 2);
        var validOperators = new[] { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99" };

        return validOperators.Contains(operatorCode);
    }
}