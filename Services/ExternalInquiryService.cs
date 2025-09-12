using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.Reception;
using Serilog;
using System;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس استعلام کمکی اطلاعات بیمار - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. استعلام کمکی از سرویس‌های خارجی (ثبت احوال و بیمه)
    /// 2. Fallback Strategy برای ادامه فرآیند در صورت عدم دسترسی
    /// 3. استفاده از توکن امنیتی KeyA (TR127256)
    /// 4. مدیریت کامل خطاها و timeout
    /// 5. لاگ‌گیری جامع تمام عملیات
    /// 6. عدم وابستگی - پذیرش همیشه انجام می‌شود
    /// 
    /// نکته حیاتی: این سرویس کمکی است و نباید مانع از انجام پذیرش شود
    /// </summary>
    public class ExternalInquiryService : IExternalInquiryService
    {
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public ExternalInquiryService(
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _log = logger.ForContext<ExternalInquiryService>();
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// استعلام اطلاعات هویت بیمار از ثبت احوال
        /// </summary>
        public async Task<ServiceResult<PatientIdentityData>> InquiryIdentityAsync(string nationalCode, DateTime birthDate, string tokenId = "TR127256")
        {
            _log.Information(
                "شروع استعلام هویت بیمار. کد ملی: {NationalCode}, تاریخ تولد: {BirthDate}, توکن: {TokenId}. کاربر: {UserName} (شناسه: {UserId})",
                nationalCode, birthDate.ToString("yyyy-MM-dd"), tokenId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی کد ملی
                var validationResult = await ValidateNationalCodeAsync(nationalCode);
                if (!validationResult.Success)
                {
                    _log.Warning("کد ملی نامعتبر برای استعلام هویت: {NationalCode}", nationalCode);
                    return ServiceResult<PatientIdentityData>.Failed(
                        "کد ملی وارد شده معتبر نیست.",
                        "INVALID_NATIONAL_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // بررسی وضعیت توکن
                var tokenStatus = await CheckTokenStatusAsync(tokenId);
                if (!tokenStatus.Success)
                {
                    _log.Warning("توکن امنیتی در دسترس نیست: {TokenId}", tokenId);
                    return ServiceResult<PatientIdentityData>.Failed(
                        "توکن امنیتی در دسترس نیست. لطفاً اطلاعات را دستی وارد کنید.",
                        "TOKEN_NOT_AVAILABLE",
                        ErrorCategory.General,
                        SecurityLevel.Medium);
                }

                // شبیه‌سازی استعلام از ثبت احوال
                // در پیاده‌سازی واقعی، اینجا API call به سرویس ثبت احوال انجام می‌شود
                // پارامترهای ارسالی: کد ملی + تاریخ تولد + توکن امنیتی
                _log.Information("ارسال درخواست استعلام هویت به سرویس ثبت احوال. کد ملی: {NationalCode}, تاریخ تولد: {BirthDate}", 
                    nationalCode, birthDate.ToString("yyyy-MM-dd"));
                
                await Task.Delay(2000); // شبیه‌سازی تأخیر شبکه

                // شبیه‌سازی پاسخ موفق
                var identityData = new PatientIdentityData
                {
                    FirstName = "احمد",
                    LastName = "محمدی",
                    FatherName = "علی",
                    BirthDate = new DateTime(1985, 5, 15),
                    Gender = Gender.Male,
                    BirthPlace = "تهران",
                    Address = "تهران، خیابان ولیعصر، پلاک 123",
                    BirthCertificateNumber = "1234567890",
                    IsVerified = true,
                    VerificationDate = DateTime.Now
                };

                _log.Information(
                    "استعلام هویت بیمار با موفقیت انجام شد. کد ملی: {NationalCode}. کاربر: {UserName} (شناسه: {UserId})",
                    nationalCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientIdentityData>.Successful(
                    identityData,
                    "اطلاعات هویت بیمار با موفقیت دریافت شد.",
                    operationName: "InquiryIdentity",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در استعلام هویت بیمار. کد ملی: {NationalCode}. کاربر: {UserName} (شناسه: {UserId})",
                    nationalCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientIdentityData>.Failed(
                    "خطا در استعلام هویت بیمار. لطفاً اطلاعات را دستی وارد کنید.",
                    "IDENTITY_INQUIRY_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// استعلام اطلاعات بیمه بیمار از سرویس بیمه
        /// </summary>
        public async Task<ServiceResult<PatientInsuranceData>> InquiryInsuranceAsync(string nationalCode, DateTime birthDate, string tokenId = "TR127256")
        {
            _log.Information(
                "شروع استعلام بیمه بیمار. کد ملی: {NationalCode}, تاریخ تولد: {BirthDate}, توکن: {TokenId}. کاربر: {UserName} (شناسه: {UserId})",
                nationalCode, birthDate.ToString("yyyy-MM-dd"), tokenId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی کد ملی
                var validationResult = await ValidateNationalCodeAsync(nationalCode);
                if (!validationResult.Success)
                {
                    _log.Warning("کد ملی نامعتبر برای استعلام بیمه: {NationalCode}", nationalCode);
                    return ServiceResult<PatientInsuranceData>.Failed(
                        "کد ملی وارد شده معتبر نیست.",
                        "INVALID_NATIONAL_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // بررسی وضعیت توکن
                var tokenStatus = await CheckTokenStatusAsync(tokenId);
                if (!tokenStatus.Success)
                {
                    _log.Warning("توکن امنیتی در دسترس نیست: {TokenId}", tokenId);
                    return ServiceResult<PatientInsuranceData>.Failed(
                        "توکن امنیتی در دسترس نیست. لطفاً اطلاعات بیمه را دستی وارد کنید.",
                        "TOKEN_NOT_AVAILABLE",
                        ErrorCategory.General,
                        SecurityLevel.Medium);
                }

                // شبیه‌سازی استعلام از سرویس بیمه
                // در پیاده‌سازی واقعی، اینجا API call به سرویس بیمه انجام می‌شود
                // پارامترهای ارسالی: کد ملی + تاریخ تولد + توکن امنیتی
                _log.Information("ارسال درخواست استعلام بیمه به سرویس بیمه. کد ملی: {NationalCode}, تاریخ تولد: {BirthDate}", 
                    nationalCode, birthDate.ToString("yyyy-MM-dd"));
                
                await Task.Delay(1500); // شبیه‌سازی تأخیر شبکه

                // شبیه‌سازی پاسخ موفق
                var insuranceData = new PatientInsuranceData
                {
                    InsuranceName = "تأمین اجتماعی",
                    InsuranceNumber = "1234567890123456",
                    InsuranceStatus = "فعال",
                    StartDate = new DateTime(2020, 1, 1),
                    EndDate = new DateTime(2025, 12, 31),
                    CoveragePercentage = 70,
                    Deductible = 100000,
                    IsVerified = true,
                    VerificationDate = DateTime.Now
                };

                _log.Information(
                    "استعلام بیمه بیمار با موفقیت انجام شد. کد ملی: {NationalCode}. کاربر: {UserName} (شناسه: {UserId})",
                    nationalCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientInsuranceData>.Successful(
                    insuranceData,
                    "اطلاعات بیمه بیمار با موفقیت دریافت شد.",
                    operationName: "InquiryInsurance",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در استعلام بیمه بیمار. کد ملی: {NationalCode}. کاربر: {UserName} (شناسه: {UserId})",
                    nationalCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientInsuranceData>.Failed(
                    "خطا در استعلام بیمه بیمار. لطفاً اطلاعات بیمه را دستی وارد کنید.",
                    "INSURANCE_INQUIRY_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// استعلام کامل (هویت + بیمه) بیمار
        /// </summary>
        public async Task<ServiceResult<PatientInquiryViewModel>> InquiryCompleteAsync(string nationalCode, DateTime birthDate, InquiryType inquiryType, string tokenId = "TR127256")
        {
            _log.Information(
                "شروع استعلام کامل بیمار. کد ملی: {NationalCode}, تاریخ تولد: {BirthDate}, نوع: {InquiryType}, توکن: {TokenId}. کاربر: {UserName} (شناسه: {UserId})",
                nationalCode, birthDate.ToString("yyyy-MM-dd"), inquiryType, tokenId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var inquiryResult = new PatientInquiryViewModel
                {
                    NationalCode = nationalCode,
                    BirthDate = birthDate,
                    InquiryType = inquiryType,
                    Status = InquiryStatus.InProgress,
                    InquiryTime = DateTime.Now
                };

                // استعلام هویت
                if (inquiryType == InquiryType.Identity || inquiryType == InquiryType.Both)
                {
                    var identityResult = await InquiryIdentityAsync(nationalCode, birthDate, tokenId);
                    if (identityResult.Success)
                    {
                        inquiryResult.IdentityData = identityResult.Data;
                        inquiryResult.IsSuccessful = true;
                    }
                    else
                    {
                        _log.Warning("استعلام هویت ناموفق: {Message}", identityResult.Message);
                    }
                }

                // استعلام بیمه
                if (inquiryType == InquiryType.Insurance || inquiryType == InquiryType.Both)
                {
                    var insuranceResult = await InquiryInsuranceAsync(nationalCode, birthDate, tokenId);
                    if (insuranceResult.Success)
                    {
                        inquiryResult.InsuranceData = insuranceResult.Data;
                        inquiryResult.IsSuccessful = true;
                    }
                    else
                    {
                        _log.Warning("استعلام بیمه ناموفق: {Message}", insuranceResult.Message);
                    }
                }

                // تعیین وضعیت نهایی
                if (inquiryResult.IsSuccessful)
                {
                    inquiryResult.Status = InquiryStatus.Successful;
                    inquiryResult.Message = "استعلام با موفقیت انجام شد. اطلاعات به فرم bind شدند.";
                }
                else
                {
                    inquiryResult.Status = InquiryStatus.Failed;
                    inquiryResult.Message = "استعلام ناموفق بود. لطفاً اطلاعات را دستی وارد کنید.";
                }

                _log.Information(
                    "استعلام کامل بیمار تکمیل شد. کد ملی: {NationalCode}, وضعیت: {Status}. کاربر: {UserName} (شناسه: {UserId})",
                    nationalCode, inquiryResult.Status, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientInquiryViewModel>.Successful(
                    inquiryResult,
                    inquiryResult.Message,
                    operationName: "InquiryComplete",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در استعلام کامل بیمار. کد ملی: {NationalCode}. کاربر: {UserName} (شناسه: {UserId})",
                    nationalCode, _currentUserService.UserName, _currentUserService.UserId);

                var failedResult = new PatientInquiryViewModel
                {
                    NationalCode = nationalCode,
                    BirthDate = birthDate,
                    InquiryType = inquiryType,
                    Status = InquiryStatus.Failed,
                    Message = "خطا در استعلام. لطفاً اطلاعات را دستی وارد کنید.",
                    InquiryTime = DateTime.Now
                };

                return ServiceResult<PatientInquiryViewModel>.Successful(
                    failedResult,
                    "استعلام ناموفق بود. لطفاً اطلاعات را دستی وارد کنید.",
                    operationName: "InquiryComplete",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// بررسی وضعیت توکن امنیتی
        /// </summary>
        public async Task<ServiceResult<bool>> CheckTokenStatusAsync(string tokenId = "TR127256")
        {
            try
            {
                _log.Information("بررسی وضعیت توکن امنیتی: {TokenId}", tokenId);

                // شبیه‌سازی بررسی توکن
                await Task.Delay(500);

                // در پیاده‌سازی واقعی، اینجا بررسی فیزیکی توکن انجام می‌شود
                bool tokenAvailable = true; // شبیه‌سازی

                if (tokenAvailable)
                {
                    _log.Information("توکن امنیتی در دسترس است: {TokenId}", tokenId);
                    return ServiceResult<bool>.Successful(
                        true,
                        "توکن امنیتی در دسترس است.",
                        operationName: "CheckTokenStatus",
                        userId: _currentUserService.UserId,
                        userFullName: _currentUserService.UserName,
                        securityLevel: SecurityLevel.Low);
                }
                else
                {
                    _log.Warning("توکن امنیتی در دسترس نیست: {TokenId}", tokenId);
                    return ServiceResult<bool>.Failed(
                        "توکن امنیتی در دسترس نیست.",
                        "TOKEN_NOT_AVAILABLE",
                        ErrorCategory.General,
                        SecurityLevel.Medium);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وضعیت توکن: {TokenId}", tokenId);
                return ServiceResult<bool>.Failed(
                    "خطا در بررسی وضعیت توکن.",
                    "TOKEN_CHECK_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// بررسی دسترسی به سرویس‌های خارجی
        /// </summary>
        public async Task<ServiceResult<ExternalServiceStatus>> CheckServiceAvailabilityAsync()
        {
            try
            {
                _log.Information("بررسی دسترسی به سرویس‌های خارجی");

                await Task.Delay(1000); // شبیه‌سازی تأخیر

                var status = new ExternalServiceStatus
                {
                    CivilRegistrationAvailable = true, // شبیه‌سازی
                    InsuranceServiceAvailable = true, // شبیه‌سازی
                    TokenAvailable = true, // شبیه‌سازی
                    LastCheckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    OverallStatus = "همه سرویس‌ها در دسترس هستند"
                };

                _log.Information("بررسی دسترسی به سرویس‌های خارجی تکمیل شد: {Status}", status.OverallStatus);

                return ServiceResult<ExternalServiceStatus>.Successful(
                    status,
                    "بررسی دسترسی تکمیل شد.",
                    operationName: "CheckServiceAvailability",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی دسترسی به سرویس‌های خارجی");
                return ServiceResult<ExternalServiceStatus>.Failed(
                    "خطا در بررسی دسترسی به سرویس‌های خارجی.",
                    "SERVICE_AVAILABILITY_CHECK_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// اعتبارسنجی کد ملی قبل از استعلام
        /// </summary>
        public async Task<ServiceResult<bool>> ValidateNationalCodeAsync(string nationalCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    return ServiceResult<bool>.Failed(
                        "کد ملی نمی‌تواند خالی باشد.",
                        "EMPTY_NATIONAL_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                if (nationalCode.Length != 10)
                {
                    return ServiceResult<bool>.Failed(
                        "کد ملی باید 10 رقم باشد.",
                        "INVALID_NATIONAL_CODE_LENGTH",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // شبیه‌سازی اعتبارسنجی
                await Task.Delay(100);

                // در پیاده‌سازی واقعی، اینجا الگوریتم اعتبارسنجی کد ملی اجرا می‌شود
                bool isValid = true; // شبیه‌سازی

                if (isValid)
                {
                    return ServiceResult<bool>.Successful(
                        true,
                        "کد ملی معتبر است.",
                        operationName: "ValidateNationalCode",
                        userId: _currentUserService.UserId,
                        userFullName: _currentUserService.UserName,
                        securityLevel: SecurityLevel.Low);
                }
                else
                {
                    return ServiceResult<bool>.Failed(
                        "کد ملی وارد شده معتبر نیست.",
                        "INVALID_NATIONAL_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در اعتبارسنجی کد ملی: {NationalCode}", nationalCode);
                return ServiceResult<bool>.Failed(
                    "خطا در اعتبارسنجی کد ملی.",
                    "NATIONAL_CODE_VALIDATION_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
    }
}
