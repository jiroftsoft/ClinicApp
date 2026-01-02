using ClinicApp.Helpers;
using ClinicApp.Models;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Models.Core;

namespace ClinicApp.Services.Patient
{
    /// <summary>
    /// سرویس ارسال خوش‌آمدگویی به بیماران Legacy که از طریق Migration، User دریافت کرده‌اند
    /// </summary>
    public class LegacyPatientWelcomeService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _log;

        public LegacyPatientWelcomeService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _log = Log.ForContext<LegacyPatientWelcomeService>();
        }

        /// <summary>
        /// ارسال پیامک/ایمیل خوش‌آمدگویی به بیماران Legacy
        /// </summary>
        public async Task<ServiceResult> SendWelcomeNotificationsAsync()
        {
            _log.Information("شروع ارسال پیامک خوش‌آمدگویی به بیماران Legacy...");

            try
            {
                // یافتن بیماران Legacy که User از طریق Migration گرفته‌اند
                // (شناسایی: CreatedByUserId = NULL && PasswordHash = NULL)
                var legacyPatients = await _context.Patients
                    .Where(p => !p.IsDeleted &&
                                p.ApplicationUser != null &&
                                p.ApplicationUser.CreatedByUserId == null &&
                                p.ApplicationUser.PasswordHash == null)
                    .Select(p => new
                    {
                        p.PatientId,
                        p.NationalCode,
                        p.FirstName,
                        p.LastName,
                        p.PhoneNumber,
                        p.Email,
                        UserId = p.ApplicationUser.Id,
                        UserName = p.ApplicationUser.UserName
                    })
                    .ToListAsync();

                if (!legacyPatients.Any())
                {
                    _log.Information("هیچ بیمار Legacy‌ای برای ارسال پیامک یافت نشد.");
                    return ServiceResult.Successful("هیچ بیمار Legacy‌ای یافت نشد.");
                }

                _log.Information($"تعداد {legacyPatients.Count} بیمار Legacy یافت شد.");

                int successCount = 0;
                int failCount = 0;

                foreach (var patient in legacyPatients)
                {
                    try
                    {
                        // تولید لینک فعال‌سازی (Reset Password Token)
                        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(patient.UserId);

                        // ساخت لینک فعال‌سازی
                        // توجه: باید با URL واقعی سایت جایگزین شود
                        var activationLink = $"https://clinicapp.ir/Account/SetPassword?userId={patient.UserId}&token={Uri.EscapeDataString(resetToken)}";

                        // متن پیامک
                        var smsText = $"عزیز {patient.FirstName} {patient.LastName}، به پورتال کلینیک شفا خوش آمدید!\n" +
                                     $"برای فعال‌سازی حساب کاربری و تنظیم رمز عبور، لینک زیر را باز کنید:\n" +
                                     $"{activationLink}\n" +
                                     $"کد ملی شما: {patient.NationalCode}";

                        // TODO: ارسال واقعی پیامک (به جای این خط، سرویس SMS واقعی را فراخوانی کنید)
                        // await _smsService.SendAsync(patient.PhoneNumber, smsText);

                        // فعلاً فقط لاگ می‌کنیم
                        _log.Information(
                            "پیامک خوش‌آمدگویی برای بیمار {PatientId} ({FullName}) آماده شد. شماره: {PhoneNumber}",
                            patient.PatientId,
                            $"{patient.FirstName} {patient.LastName}",
                            patient.PhoneNumber);

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex,
                            "خطا در ارسال پیامک خوش‌آمدگویی به بیمار {PatientId}",
                            patient.PatientId);
                        failCount++;
                    }
                }

                _log.Information(
                    "ارسال پیامک خوش‌آمدگویی به پایان رسید. موفق: {SuccessCount}، ناموفق: {FailCount}",
                    successCount,
                    failCount);

                return ServiceResult.Successful(
                    $"ارسال پیامک به {successCount} بیمار موفق و {failCount} ناموفق بود.",
                    "WELCOME_SMS_SENT");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای عمومی در ارسال پیامک خوش‌آمدگویی به بیماران Legacy");
                return ServiceResult.Failed(
                    "خطا در ارسال پیامک خوش‌آمدگویی.",
                    "WELCOME_SMS_ERROR");
            }
        }

        /// <summary>
        /// آمار بیماران Legacy
        /// </summary>
        public async Task<ServiceResult> GetLegacyPatientsStatisticsAsync()
        {
            try
            {
                var stats = await _context.Patients
                    .Where(p => !p.IsDeleted &&
                                p.ApplicationUser != null &&
                                p.ApplicationUser.CreatedByUserId == null &&
                                p.ApplicationUser.PasswordHash == null)
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        TotalLegacyPatients = g.Count(),
                        PatientsWithPhone = g.Count(p => p.PhoneNumber != null),
                        PatientsWithEmail = g.Count(p => p.Email != null),
                        PatientsWithoutContact = g.Count(p => p.PhoneNumber == null && p.Email == null)
                    })
                    .FirstOrDefaultAsync();

                if (stats == null)
                {
                    _log.Information("هیچ بیمار Legacy‌ای یافت نشد.");
                    return ServiceResult.Successful("هیچ بیمار Legacy‌ای یافت نشد.", "NO_LEGACY_PATIENTS");
                }

                _log.Information(
                    "آمار بیماران Legacy: کل={Total}, با شماره تلفن={WithPhone}, با ایمیل={WithEmail}, بدون تماس={WithoutContact}",
                    stats.TotalLegacyPatients,
                    stats.PatientsWithPhone,
                    stats.PatientsWithEmail,
                    stats.PatientsWithoutContact);

                return ServiceResult.Successful("آمار با موفقیت دریافت شد.", "STATS_SUCCESS");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت آمار بیماران Legacy");
                return ServiceResult.Failed(
                    "خطا در دریافت آمار.",
                    "STATS_ERROR");
            }
        }
    }
}

