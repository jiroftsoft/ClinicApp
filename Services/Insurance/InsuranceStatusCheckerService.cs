using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.DTOs.Insurance;
using ClinicApp.Models.Entities.Patient;
using Serilog;
using InsuranceStatusCheckResult = ClinicApp.Models.DTOs.Insurance.InsuranceStatusCheckResult;
using InsuranceStatusDetail = ClinicApp.Models.DTOs.Insurance.InsuranceStatusDetail;
using InsuranceStatusAlert = ClinicApp.Models.DTOs.Insurance.InsuranceStatusAlert;
using InsuranceStatusType = ClinicApp.Models.DTOs.Insurance.InsuranceStatusType;
using AlertType = ClinicApp.Models.DTOs.Insurance.AlertType;
using AlertSeverity = ClinicApp.Models.DTOs.Insurance.AlertSeverity;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس بررسی جامع وضعیت بیمه - قابل استفاده مجدد در تمام ماژول‌ها
    /// 
    /// این سرویس برای بررسی وضعیت بیمه بیمار و نمایش هشدارهای واضح به منشی‌ها طراحی شده است
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. بررسی جامع وضعیت بیمه (پایه و تکمیلی)
    /// 2. بررسی انقضای بیمه با هشدارهای واضح
    /// 3. پیام‌های کاربرپسند برای منشی‌ها
    /// 4. قابل استفاده در فرم پذیرش و سایر ماژول‌ها
    /// 5. بهینه‌سازی شده برای محیط Production
    /// </summary>
    public class InsuranceStatusCheckerService : IInsuranceStatusCheckerService
    {
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public InsuranceStatusCheckerService(
            IPatientInsuranceService patientInsuranceService,
            IPatientInsuranceRepository patientInsuranceRepository,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _patientInsuranceRepository = patientInsuranceRepository ?? throw new ArgumentNullException(nameof(patientInsuranceRepository));
            _logger = logger.ForContext<InsuranceStatusCheckerService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region بررسی جامع وضعیت بیمه

        /// <summary>
        /// بررسی جامع وضعیت بیمه بیمار
        /// </summary>
        public async Task<ServiceResult<InsuranceStatusCheckResult>> CheckInsuranceStatusAsync(int patientId, DateTime? checkDate = null)
        {
            try
            {
                _logger.Information("🔍 شروع بررسی جامع وضعیت بیمه. PatientId: {PatientId}, CheckDate: {CheckDate}, User: {UserName}",
                    patientId, checkDate, _currentUserService.UserName);

                var effectiveCheckDate = checkDate ?? DateTime.Now;
                var result = new InsuranceStatusCheckResult
                {
                    PatientId = patientId,
                    CheckedAt = DateTime.Now,
                    Status = InsuranceStatusType.Valid,
                    IsValid = true,
                    CanProceedWithReception = true
                };

                // ✅ دریافت بیمه‌های بیمار از Repository (مستقیم برای کارایی بهتر)
                var patientInsurances = await _patientInsuranceRepository.GetByPatientIdAsync(patientId);
                if (patientInsurances == null)
                {
                    patientInsurances = new List<PatientInsurance>();
                }

                // فیلتر کردن بیمه‌های حذف شده
                patientInsurances = patientInsurances.Where(pi => !pi.IsDeleted).ToList();

                // بررسی بیمه پایه
                var primaryInsurance = patientInsurances.FirstOrDefault(pi => pi.IsPrimary && !pi.IsDeleted);
                if (primaryInsurance != null)
                {
                    result.PrimaryInsurance = await BuildInsuranceStatusDetailAsync(primaryInsurance, effectiveCheckDate, "بیمه پایه");
                    
                    // بررسی انقضا
                    if (result.PrimaryInsurance.IsExpired)
                    {
                        result.Status = InsuranceStatusType.Expired;
                        result.IsValid = false;
                        result.IsExpired = true;
                        result.CanProceedWithReception = false; // ⚠️ بحرانی: نمی‌توان پذیرش را ادامه داد
                        result.PrimaryInsuranceExpiryDate = primaryInsurance.EndDate;
                        result.DaysUntilExpiry = result.PrimaryInsurance.DaysRemaining;

                        result.MainMessage = "⚠️ هشدار: بیمه پایه بیمار منقضی شده است!";
                        result.DetailedMessage = $"بیمه پایه بیمار در تاریخ {PersianDateHelper.ToPersianDate(primaryInsurance.EndDate.Value)} منقضی شده است. لطفاً بیمه را تمدید کنید.";

                        result.Alerts.Add(new InsuranceStatusAlert
                        {
                            Type = AlertType.Expired,
                            Severity = AlertSeverity.Critical,
                            Title = "بیمه پایه منقضی شده است",
                            Message = primaryInsurance.EndDate.HasValue
                                ? $"بیمه پایه ({primaryInsurance.InsurancePlan?.Name ?? "نامشخص"}) در تاریخ {PersianDateHelper.ToPersianDate(primaryInsurance.EndDate.Value)} منقضی شده است. نمی‌توان پذیرش را ادامه داد."
                                : $"بیمه پایه ({primaryInsurance.InsurancePlan?.Name ?? "نامشخص"}) منقضی شده است. نمی‌توان پذیرش را ادامه داد.",
                            ShowAsModal = true,
                            BlockReception = true,
                            Icon = "fas fa-exclamation-triangle",
                            CssClass = "alert-danger"
                        });

                        result.Recommendations.Add("لطفاً بیمه پایه بیمار را تمدید کنید");
                        result.Recommendations.Add("یا بیمه جدید برای بیمار ثبت کنید");
                    }
                    else if (result.PrimaryInsurance.DaysRemaining.HasValue && result.PrimaryInsurance.DaysRemaining.Value <= 30)
                    {
                        result.Status = InsuranceStatusType.ExpiringSoon;
                        result.HasExpiryWarning = true;
                        result.PrimaryInsuranceExpiryDate = primaryInsurance.EndDate;
                        result.DaysUntilExpiry = result.PrimaryInsurance.DaysRemaining;

                        result.MainMessage = $"⚠️ هشدار: بیمه پایه در {result.PrimaryInsurance.DaysRemaining} روز آینده منقضی می‌شود";
                        result.DetailedMessage = primaryInsurance.EndDate.HasValue 
                            ? $"بیمه پایه بیمار در تاریخ {PersianDateHelper.ToPersianDate(primaryInsurance.EndDate.Value)} منقضی می‌شود. برای تمدید اقدام کنید."
                            : "بیمه پایه بیمار در حال انقضا است. برای تمدید اقدام کنید.";

                        result.Alerts.Add(new InsuranceStatusAlert
                        {
                            Type = AlertType.ExpiringSoon,
                            Severity = AlertSeverity.Warning,
                            Title = "بیمه پایه در حال انقضا",
                            Message = primaryInsurance.EndDate.HasValue
                                ? $"بیمه پایه ({primaryInsurance.InsurancePlan?.Name ?? "نامشخص"}) در {result.PrimaryInsurance.DaysRemaining} روز آینده (تاریخ {PersianDateHelper.ToPersianDate(primaryInsurance.EndDate.Value)}) منقضی می‌شود."
                                : $"بیمه پایه ({primaryInsurance.InsurancePlan?.Name ?? "نامشخص"}) در {result.PrimaryInsurance.DaysRemaining} روز آینده منقضی می‌شود.",
                            ShowAsModal = true,
                            BlockReception = false, // می‌توان پذیرش را ادامه داد اما هشدار داده می‌شود
                            Icon = "fas fa-exclamation-circle",
                            CssClass = "alert-warning"
                        });

                        result.Recommendations.Add("برای تمدید بیمه پایه اقدام کنید");
                    }
                    else if (!primaryInsurance.IsActive)
                    {
                        result.Status = InsuranceStatusType.Inactive;
                        result.IsValid = false;
                        result.CanProceedWithReception = false;

                        result.MainMessage = "⚠️ هشدار: بیمه پایه غیرفعال است!";
                        result.DetailedMessage = "بیمه پایه بیمار غیرفعال است. لطفاً بیمه را فعال کنید یا بیمه جدید انتخاب کنید.";

                        result.Alerts.Add(new InsuranceStatusAlert
                        {
                            Type = AlertType.Inactive,
                            Severity = AlertSeverity.Critical,
                            Title = "بیمه پایه غیرفعال است",
                            Message = $"بیمه پایه ({primaryInsurance.InsurancePlan?.Name ?? "نامشخص"}) غیرفعال است. نمی‌توان پذیرش را ادامه داد.",
                            ShowAsModal = true,
                            BlockReception = true,
                            Icon = "fas fa-ban",
                            CssClass = "alert-danger"
                        });

                        result.Recommendations.Add("لطفاً بیمه پایه را فعال کنید");
                        result.Recommendations.Add("یا بیمه جدید برای بیمار ثبت کنید");
                    }
                }
                else
                {
                    // بیمه پایه وجود ندارد
                    result.Status = InsuranceStatusType.MissingPrimaryInsurance;
                    result.IsValid = false;
                    result.CanProceedWithReception = false;

                    result.MainMessage = "⚠️ هشدار: بیمه پایه برای این بیمار تعریف نشده است!";
                    result.DetailedMessage = "بیمه پایه برای این بیمار تعریف نشده است. لطفاً ابتدا بیمه پایه را ثبت کنید.";

                    result.Alerts.Add(new InsuranceStatusAlert
                    {
                        Type = AlertType.Missing,
                        Severity = AlertSeverity.Critical,
                        Title = "بیمه پایه وجود ندارد",
                        Message = "بیمه پایه برای این بیمار تعریف نشده است. نمی‌توان پذیرش را ادامه داد.",
                        ShowAsModal = true,
                        BlockReception = true,
                        Icon = "fas fa-times-circle",
                        CssClass = "alert-danger"
                    });

                    result.Recommendations.Add("لطفاً ابتدا بیمه پایه را برای بیمار ثبت کنید");
                }

                // بررسی بیمه تکمیلی (اختیاری)
                var supplementaryInsurance = patientInsurances.FirstOrDefault(pi => !pi.IsPrimary && !pi.IsDeleted);
                if (supplementaryInsurance != null)
                {
                    result.SupplementaryInsurance = await BuildInsuranceStatusDetailAsync(supplementaryInsurance, effectiveCheckDate, "بیمه تکمیلی");
                    
                    // بررسی انقضای بیمه تکمیلی (هشدار اما متوقف نمی‌کند)
                    if (result.SupplementaryInsurance.IsExpired)
                    {
                        result.SupplementaryInsuranceExpiryDate = supplementaryInsurance.EndDate;
                        result.HasExpiryWarning = true;

                        result.Alerts.Add(new InsuranceStatusAlert
                        {
                            Type = AlertType.Expired,
                            Severity = AlertSeverity.Warning,
                            Title = "بیمه تکمیلی منقضی شده است",
                            Message = supplementaryInsurance.EndDate.HasValue
                                ? $"بیمه تکمیلی ({supplementaryInsurance.InsurancePlan?.Name ?? "نامشخص"}) در تاریخ {PersianDateHelper.ToPersianDate(supplementaryInsurance.EndDate.Value)} منقضی شده است."
                                : $"بیمه تکمیلی ({supplementaryInsurance.InsurancePlan?.Name ?? "نامشخص"}) منقضی شده است.",
                            ShowAsModal = false, // فقط هشدار، modal نیست
                            BlockReception = false, // نمی‌تواند پذیرش را متوقف کند
                            Icon = "fas fa-info-circle",
                            CssClass = "alert-warning"
                        });

                        if (string.IsNullOrEmpty(result.DetailedMessage))
                        {
                            result.DetailedMessage = supplementaryInsurance.EndDate.HasValue
                                ? $"بیمه تکمیلی در تاریخ {PersianDateHelper.ToPersianDate(supplementaryInsurance.EndDate.Value)} منقضی شده است."
                                : "بیمه تکمیلی منقضی شده است.";
                        }
                        else
                        {
                            result.DetailedMessage += supplementaryInsurance.EndDate.HasValue
                                ? $" همچنین بیمه تکمیلی در تاریخ {PersianDateHelper.ToPersianDate(supplementaryInsurance.EndDate.Value)} منقضی شده است."
                                : " همچنین بیمه تکمیلی منقضی شده است.";
                        }

                        result.Recommendations.Add("برای تمدید بیمه تکمیلی اقدام کنید");
                    }
                    else if (result.SupplementaryInsurance.DaysRemaining.HasValue && result.SupplementaryInsurance.DaysRemaining.Value <= 30)
                    {
                        result.HasExpiryWarning = true;
                        result.SupplementaryInsuranceExpiryDate = supplementaryInsurance.EndDate;

                        result.Alerts.Add(new InsuranceStatusAlert
                        {
                            Type = AlertType.ExpiringSoon,
                            Severity = AlertSeverity.Info,
                            Title = "بیمه تکمیلی در حال انقضا",
                            Message = $"بیمه تکمیلی ({supplementaryInsurance.InsurancePlan?.Name ?? "نامشخص"}) در {result.SupplementaryInsurance.DaysRemaining} روز آینده منقضی می‌شود.",
                            ShowAsModal = false,
                            BlockReception = false,
                            Icon = "fas fa-info-circle",
                            CssClass = "alert-info"
                        });
                    }
                }

                // تنظیم پیام نهایی
                if (result.IsValid && !result.HasExpiryWarning)
                {
                    result.MainMessage = "✅ وضعیت بیمه معتبر است";
                    result.DetailedMessage = "بیمه پایه و تکمیلی بیمار معتبر و فعال هستند.";
                }

                _logger.Information("✅ بررسی وضعیت بیمه تکمیل شد. PatientId: {PatientId}, Status: {Status}, IsValid: {IsValid}, CanProceed: {CanProceed}",
                    patientId, result.Status, result.IsValid, result.CanProceedWithReception);

                return ServiceResult<InsuranceStatusCheckResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بررسی وضعیت بیمه. PatientId: {PatientId}", patientId);

                var errorResult = new InsuranceStatusCheckResult
                {
                    PatientId = patientId,
                    Status = InsuranceStatusType.Error,
                    IsValid = false,
                    CanProceedWithReception = false,
                    MainMessage = "خطا در بررسی وضعیت بیمه",
                    DetailedMessage = "خطای سیستمی رخ داد. لطفاً با پشتیبانی تماس بگیرید."
                };

                // در صورت خطا، errorResult را در Metadata قرار می‌دهیم
                var failedResult = ServiceResult<InsuranceStatusCheckResult>.Failed(
                    "خطا در بررسی وضعیت بیمه",
                    "INSURANCE_STATUS_CHECK_ERROR",
                    ErrorCategory.General);
                failedResult.Metadata["ErrorResult"] = errorResult;
                return failedResult;
            }
        }

        #endregion

        #region بررسی سریع

        /// <summary>
        /// بررسی سریع وضعیت بیمه
        /// </summary>
        public async Task<ServiceResult<bool>> IsInsuranceValidAsync(int patientId, DateTime? checkDate = null)
        {
            try
            {
                var result = await CheckInsuranceStatusAsync(patientId, checkDate);
                if (!result.Success)
                {
                    return ServiceResult<bool>.Failed(result.Message, result.Code);
                }

                return ServiceResult<bool>.Successful(result.Data.IsValid && result.Data.CanProceedWithReception);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بررسی سریع وضعیت بیمه. PatientId: {PatientId}", patientId);
                return ServiceResult<bool>.Failed("خطا در بررسی وضعیت بیمه");
            }
        }

        #endregion

        #region بررسی انقضا

        /// <summary>
        /// بررسی انقضای بیمه
        /// </summary>
        public async Task<ServiceResult<InsuranceExpiryCheckResult>> CheckInsuranceExpiryAsync(int patientId, DateTime? checkDate = null, int warningDays = 30)
        {
            try
            {
                var statusResult = await CheckInsuranceStatusAsync(patientId, checkDate);
                if (!statusResult.Success)
                {
                    return ServiceResult<InsuranceExpiryCheckResult>.Failed(statusResult.Message, statusResult.Code);
                }

                var status = statusResult.Data;
                var expiryResult = new InsuranceExpiryCheckResult
                {
                    PatientId = patientId,
                    IsExpired = status.IsExpired,
                    IsExpiringSoon = status.HasExpiryWarning && !status.IsExpired,
                    DaysUntilExpiry = status.DaysUntilExpiry,
                    PrimaryInsuranceExpiryDate = status.PrimaryInsuranceExpiryDate,
                    SupplementaryInsuranceExpiryDate = status.SupplementaryInsuranceExpiryDate
                };

                // ساخت پیام هشدار
                if (expiryResult.IsExpired)
                {
                    expiryResult.WarningMessage = "⚠️ بیمه بیمار منقضی شده است!";
                }
                else if (expiryResult.IsExpiringSoon && expiryResult.DaysUntilExpiry.HasValue)
                {
                    expiryResult.WarningMessage = $"⚠️ بیمه بیمار در {expiryResult.DaysUntilExpiry} روز آینده منقضی می‌شود";
                }
                else
                {
                    expiryResult.WarningMessage = null;
                }

                return ServiceResult<InsuranceExpiryCheckResult>.Successful(expiryResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بررسی انقضای بیمه. PatientId: {PatientId}", patientId);
                return ServiceResult<InsuranceExpiryCheckResult>.Failed("خطا در بررسی انقضای بیمه");
            }
        }

        #endregion

        #region بررسی برای پذیرش

        /// <summary>
        /// بررسی وضعیت بیمه برای پذیرش
        /// </summary>
        public async Task<ServiceResult<InsuranceStatusCheckResult>> CheckInsuranceForReceptionAsync(int patientId, DateTime receptionDate)
        {
            try
            {
                _logger.Information("🔍 بررسی وضعیت بیمه برای پذیرش. PatientId: {PatientId}, ReceptionDate: {ReceptionDate}, User: {UserName}",
                    patientId, receptionDate, _currentUserService.UserName);

                var result = await CheckInsuranceStatusAsync(patientId, receptionDate);

                if (result.Success && result.Data != null)
                {
                    // اگر بیمه منقضی شده یا غیرفعال است، نمی‌توان پذیرش را ادامه داد
                    if (result.Data.IsExpired || result.Data.Status == InsuranceStatusType.Inactive || result.Data.Status == InsuranceStatusType.MissingPrimaryInsurance)
                    {
                        result.Data.CanProceedWithReception = false;
                        _logger.Warning("⚠️ نمی‌توان پذیرش را ادامه داد. PatientId: {PatientId}, Status: {Status}",
                            patientId, result.Data.Status);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بررسی وضعیت بیمه برای پذیرش. PatientId: {PatientId}", patientId);
                return ServiceResult<InsuranceStatusCheckResult>.Failed("خطا در بررسی وضعیت بیمه برای پذیرش");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ساخت جزئیات وضعیت بیمه
        /// </summary>
        private async Task<InsuranceStatusDetail> BuildInsuranceStatusDetailAsync(
            PatientInsurance insurance, 
            DateTime checkDate, 
            string insuranceType)
        {
            var detail = new InsuranceStatusDetail
            {
                Exists = true,
                InsuranceId = insurance.PatientInsuranceId,
                InsuranceName = insurance.InsurancePlan?.Name ?? "نامشخص",
                PolicyNumber = insurance.PolicyNumber,
                StartDate = insurance.StartDate,
                EndDate = insurance.EndDate,
                IsActive = insurance.IsActive
            };

            // بررسی انقضا
            if (insurance.EndDate.HasValue)
            {
                var daysRemaining = (insurance.EndDate.Value.Date - checkDate.Date).Days;
                detail.DaysRemaining = daysRemaining;
                detail.IsExpired = daysRemaining < 0;

                if (detail.IsExpired)
                {
                    detail.StatusText = "منقضی شده";
                    detail.StatusClass = "danger";
                }
                else if (daysRemaining <= 30)
                {
                    detail.StatusText = $"در حال انقضا ({daysRemaining} روز باقی‌مانده)";
                    detail.StatusClass = "warning";
                }
                else
                {
                    detail.StatusText = "معتبر";
                    detail.StatusClass = "success";
                }
            }
            else
            {
                detail.StatusText = "بدون تاریخ انقضا";
                detail.StatusClass = "info";
            }

            // بررسی فعال بودن
            if (!insurance.IsActive)
            {
                detail.StatusText = "غیرفعال";
                detail.StatusClass = "danger";
            }

            return detail;
        }

        #endregion
    }
}

