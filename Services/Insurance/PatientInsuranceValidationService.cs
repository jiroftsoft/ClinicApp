using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.DTOs.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس اعتبارسنجی حرفه‌ای بیمه بیماران
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی تاریخ بیمه‌ها
    /// 2. بررسی وضعیت فعال/غیرفعال
    /// 3. اعتبارسنجی قوانین کسب‌وکار
    /// 4. قابلیت استفاده مجدد در تمام ماژول‌ها
    /// 5. گزارش‌دهی جامع وضعیت بیمه
    /// </summary>
    public class PatientInsuranceValidationService : IPatientInsuranceValidationService
    {
        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly ILogger _logger;

        public PatientInsuranceValidationService(
            IPatientInsuranceRepository patientInsuranceRepository,
            ILogger logger)
        {
            _patientInsuranceRepository = patientInsuranceRepository;
            _logger = logger;
        }

        #region اعتبارسنجی تاریخ بیمه

        /// <summary>
        /// اعتبارسنجی کامل بیمه بیمار
        /// </summary>
        public async Task<ServiceResult<PatientInsuranceValidationResult>> ValidatePatientInsuranceAsync(int patientId)
        {
            try
            {
                _logger.Information("🔍 شروع اعتبارسنجی بیمه بیمار. PatientId: {PatientId}", patientId);

                var validationResult = new PatientInsuranceValidationResult
                {
                    PatientId = patientId,
                    IsValid = true,
                    ValidationDate = DateTime.Now,
                    Issues = new List<InsuranceValidationIssue>(),
                    Recommendations = new List<string>()
                };

                // دریافت بیمه‌های بیمار
                var primaryInsurance = await _patientInsuranceRepository.GetActivePrimaryInsuranceAsync(patientId);
                var supplementaryInsurance = await _patientInsuranceRepository.GetActiveSupplementaryInsuranceAsync(patientId);

                // اعتبارسنجی بیمه پایه
                if (primaryInsurance != null)
                {
                    var primaryValidation = await ValidateInsuranceDatesAsync(primaryInsurance, "بیمه پایه");
                    validationResult.PrimaryInsuranceStatus = primaryValidation;
                    
                    if (!primaryValidation.IsValid)
                    {
                        validationResult.IsValid = false;
                        validationResult.Issues.AddRange(primaryValidation.Issues);
                    }
                }
                else
                {
                    // بیمه پایه الزامی است
                    validationResult.IsValid = false;
                    validationResult.Issues.Add(new InsuranceValidationIssue
                    {
                        Type = ValidationIssueType.MissingPrimaryInsurance,
                        Severity = ValidationSeverity.Critical,
                        Message = "بیمه پایه برای این بیمار تعریف نشده است",
                        Recommendation = "ابتدا بیمه پایه را انتخاب کنید"
                    });
                }

                // اعتبارسنجی بیمه تکمیلی (اختیاری)
                if (supplementaryInsurance != null)
                {
                    var supplementaryValidation = await ValidateInsuranceDatesAsync(supplementaryInsurance, "بیمه تکمیلی");
                    validationResult.SupplementaryInsuranceStatus = supplementaryValidation;
                    
                    if (!supplementaryValidation.IsValid)
                    {
                        validationResult.Issues.AddRange(supplementaryValidation.Issues);
                    }
                }

                // تولید توصیه‌ها
                GenerateRecommendations(validationResult);

                _logger.Information("✅ اعتبارسنجی بیمه بیمار تکمیل شد. PatientId: {PatientId}, IsValid: {IsValid}, IssuesCount: {IssuesCount}", 
                    patientId, validationResult.IsValid, validationResult.Issues.Count);

                return ServiceResult<PatientInsuranceValidationResult>.Successful(validationResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در اعتبارسنجی بیمه بیمار. PatientId: {PatientId}", patientId);
                return ServiceResult<PatientInsuranceValidationResult>.Failed(
                    "خطای غیرمنتظره در اعتبارسنجی بیمه بیمار: " + ex.Message);
            }
        }

        /// <summary>
        /// اعتبارسنجی تاریخ‌های بیمه
        /// </summary>
        private async Task<InsuranceValidationStatus> ValidateInsuranceDatesAsync(
            PatientInsurance insurance, string insuranceType)
        {
            var status = new InsuranceValidationStatus
            {
                InsuranceId = insurance.PatientInsuranceId,
                InsuranceType = insuranceType,
                IsValid = true,
                Issues = new List<InsuranceValidationIssue>()
            };

            var currentDate = DateTime.Now;

            // بررسی تاریخ شروع
            if (insurance.StartDate > currentDate)
            {
                status.IsValid = false;
                status.Issues.Add(new InsuranceValidationIssue
                {
                    Type = ValidationIssueType.FutureStartDate,
                    Severity = ValidationSeverity.Warning,
                    Message = $"{insuranceType} هنوز شروع نشده است (شروع: {insurance.StartDate:yyyy/MM/dd})",
                    Recommendation = "تا تاریخ شروع صبر کنید یا تاریخ را اصلاح کنید"
                });
            }

            // بررسی تاریخ پایان
            if (insurance.EndDate.HasValue && insurance.EndDate.Value < currentDate)
            {
                status.IsValid = false;
                status.Issues.Add(new InsuranceValidationIssue
                {
                    Type = ValidationIssueType.ExpiredInsurance,
                    Severity = ValidationSeverity.Critical,
                    Message = $"{insuranceType} منقضی شده است (پایان: {insurance.EndDate.Value:yyyy/MM/dd})",
                    Recommendation = "بیمه را تمدید کنید یا بیمه جدید انتخاب کنید"
                });
            }

            // بررسی وضعیت فعال
            if (!insurance.IsActive)
            {
                status.IsValid = false;
                status.Issues.Add(new InsuranceValidationIssue
                {
                    Type = ValidationIssueType.InactiveInsurance,
                    Severity = ValidationSeverity.Critical,
                    Message = $"{insuranceType} غیرفعال است",
                    Recommendation = "بیمه را فعال کنید یا بیمه جدید انتخاب کنید"
                });
            }

            // بررسی نزدیک بودن به انقضا (30 روز قبل)
            if (insurance.EndDate.HasValue)
            {
                var daysUntilExpiry = (insurance.EndDate.Value - currentDate).Days;
                if (daysUntilExpiry <= 30 && daysUntilExpiry > 0)
                {
                    status.Issues.Add(new InsuranceValidationIssue
                    {
                        Type = ValidationIssueType.ExpiringSoon,
                        Severity = ValidationSeverity.Info,
                        Message = $"{insuranceType} در {daysUntilExpiry} روز آینده منقضی می‌شود",
                        Recommendation = "برای تمدید بیمه اقدام کنید"
                    });
                }
            }

            return status;
        }

        /// <summary>
        /// تولید توصیه‌های هوشمند
        /// </summary>
        private void GenerateRecommendations(PatientInsuranceValidationResult result)
        {
            if (!result.IsValid)
            {
                result.Recommendations.Add("وضعیت بیمه بیمار نیاز به بررسی و اصلاح دارد");
            }

            if (result.PrimaryInsuranceStatus?.IsValid == true && result.SupplementaryInsuranceStatus == null)
            {
                result.Recommendations.Add("برای کاهش هزینه‌های درمان، بیمه تکمیلی را در نظر بگیرید");
            }

            if (result.Issues.Any(i => i.Type == ValidationIssueType.ExpiredInsurance))
            {
                result.Recommendations.Add("بیمه‌های منقضی را تمدید یا جایگزین کنید");
            }

            if (result.Issues.Any(i => i.Type == ValidationIssueType.ExpiringSoon))
            {
                result.Recommendations.Add("برای تمدید بیمه‌های در حال انقضا اقدام کنید");
            }
        }

        #endregion

        #region اعتبارسنجی سریع

        /// <summary>
        /// بررسی سریع اعتبار بیمه برای استفاده در ماژول پذیرش
        /// </summary>
        public async Task<ServiceResult<bool>> IsPatientInsuranceValidAsync(int patientId)
        {
            try
            {
                var primaryInsurance = await _patientInsuranceRepository.GetActivePrimaryInsuranceAsync(patientId);
                
                if (primaryInsurance == null)
                {
                    return ServiceResult<bool>.Failed("بیمه پایه یافت نشد");
                }

                var currentDate = DateTime.Now;
                
                // بررسی تاریخ‌ها
                if (primaryInsurance.StartDate > currentDate)
                {
                    return ServiceResult<bool>.Failed("بیمه هنوز شروع نشده است");
                }

                if (primaryInsurance.EndDate.HasValue && primaryInsurance.EndDate.Value < currentDate)
                {
                    return ServiceResult<bool>.Failed("بیمه منقضی شده است");
                }

                if (!primaryInsurance.IsActive)
                {
                    return ServiceResult<bool>.Failed("بیمه غیرفعال است");
                }

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بررسی سریع اعتبار بیمه. PatientId: {PatientId}", patientId);
                return ServiceResult<bool>.Failed("خطا در بررسی اعتبار بیمه");
            }
        }

        #endregion
    }


}
