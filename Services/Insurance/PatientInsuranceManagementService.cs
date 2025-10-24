using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Helpers.Insurance;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.DTOs.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس مدیریت حرفه‌ای بیمه بیماران
    /// قابلیت استفاده مجدد در تمامی ماژول‌ها
    /// </summary>
    public class PatientInsuranceManagementService : IPatientInsuranceManagementService
    {
        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public PatientInsuranceManagementService(
            IPatientInsuranceRepository patientInsuranceRepository,
            IInsurancePlanService insurancePlanService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _patientInsuranceRepository = patientInsuranceRepository;
            _insurancePlanService = insurancePlanService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        #region انتخاب بیمه پایه

        /// <summary>
        /// انتخاب بیمه پایه برای بیمار
        /// </summary>
        public async Task<ServiceResult<PatientInsuranceSelectionResult>> SelectPrimaryInsuranceAsync(
            int patientId, int insurancePlanId, string policyNumber, DateTime startDate, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("🏥 شروع انتخاب بیمه پایه. PatientId: {PatientId}, PlanId: {PlanId}, PolicyNumber: {PolicyNumber}", 
                    patientId, insurancePlanId, policyNumber);

                // بررسی وجود بیمه پایه فعال
                var existingPrimary = await _patientInsuranceRepository.GetActivePrimaryInsuranceAsync(patientId);
                if (existingPrimary != null)
                {
                return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                    "بیمه پایه فعال برای این بیمار وجود دارد. ابتدا بیمه قبلی را غیرفعال کنید.");
                }

                // بررسی طرح بیمه
                var planResult = await _insurancePlanService.GetByIdAsync(insurancePlanId);
                if (!planResult.Success)
                {
                    return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                        "طرح بیمه انتخابی یافت نشد.");
                }

                var plan = planResult.Data;
                if (plan.InsuranceType != InsuranceType.Primary)
                {
                    return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                        "طرح انتخابی بیمه پایه نیست.");
                }

                // ایجاد بیمه پایه جدید
                var primaryInsurance = new PatientInsurance
                {
                    PatientId = patientId,
                    InsurancePlanId = insurancePlanId,
                    PolicyNumber = policyNumber,
                    IsPrimary = true,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsActive = true,
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedAt = DateTime.Now
                };

                var createResult = await _patientInsuranceRepository.CreateAsync(primaryInsurance);
                if (!createResult.Success)
                {
                    return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                        "خطا در ایجاد بیمه پایه: " + createResult.Message);
                }

                var result = new PatientInsuranceSelectionResult
                {
                    PatientId = patientId,
                    InsuranceId = createResult.Data.PatientInsuranceId,
                    InsuranceType = InsuranceTypeHelper.ToDisplayString(InsuranceType.Primary),
                    InsuranceName = plan.Name,
                    PolicyNumber = policyNumber,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsActive = true,
                    Message = "بیمه پایه با موفقیت انتخاب شد."
                };

               _logger.Information("✅ بیمه پایه با موفقیت انتخاب شد. PatientId: {PatientId}, InsuranceId: {InsuranceId}", 
                   patientId, createResult.Data.PatientInsuranceId);

                return ServiceResult<PatientInsuranceSelectionResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در انتخاب بیمه پایه. PatientId: {PatientId}", patientId);
                return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                    "خطای غیرمنتظره در انتخاب بیمه پایه: " + ex.Message);
            }
        }

        #endregion

        #region انتخاب بیمه تکمیلی

        /// <summary>
        /// انتخاب بیمه تکمیلی برای بیمار
        /// </summary>
        public async Task<ServiceResult<PatientInsuranceSelectionResult>> SelectSupplementaryInsuranceAsync(
            int patientId, int insurancePlanId, string policyNumber, DateTime startDate, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("🏥 شروع انتخاب بیمه تکمیلی. PatientId: {PatientId}, PlanId: {PlanId}, PolicyNumber: {PolicyNumber}", 
                    patientId, insurancePlanId, policyNumber);

                // بررسی وجود بیمه پایه فعال
                var primaryInsurance = await _patientInsuranceRepository.GetActivePrimaryInsuranceAsync(patientId);
                if (primaryInsurance == null)
                {
                    return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                        "ابتدا باید بیمه پایه برای این بیمار انتخاب شود.");
                }

                // بررسی طرح بیمه
                var planResult = await _insurancePlanService.GetByIdAsync(insurancePlanId);
                if (!planResult.Success)
                {
                    return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                        "طرح بیمه انتخابی یافت نشد.");
                }

                var plan = planResult.Data;
                if (plan.InsuranceType != InsuranceType.Supplementary)
                {
                    return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                        "طرح انتخابی بیمه تکمیلی نیست.");
                }

                // بررسی تداخل تاریخ
                if (primaryInsurance.EndDate.HasValue && startDate > primaryInsurance.EndDate.Value)
                {
                    return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                        "تاریخ شروع بیمه تکمیلی نمی‌تواند بعد از تاریخ پایان بیمه پایه باشد.");
                }

                // ایجاد بیمه تکمیلی جدید
                var supplementaryInsurance = new PatientInsurance
                {
                    PatientId = patientId,
                    InsurancePlanId = insurancePlanId,
                    PolicyNumber = policyNumber,
                    IsPrimary = false,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsActive = true,
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedAt = DateTime.Now
                };

                var createResult = await _patientInsuranceRepository.CreateAsync(supplementaryInsurance);
                if (!createResult.Success)
                {
                    return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                        "خطا در ایجاد بیمه تکمیلی: " + createResult.Message);
                }

                var result = new PatientInsuranceSelectionResult
                {
                    PatientId = patientId,
                    InsuranceId = createResult.Data.PatientInsuranceId,
                    InsuranceType = InsuranceTypeHelper.ToDisplayString(InsuranceType.Supplementary),
                    InsuranceName = plan.Name,
                    PolicyNumber = policyNumber,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsActive = true,
                    Message = "بیمه تکمیلی با موفقیت انتخاب شد."
                };

               _logger.Information("✅ بیمه تکمیلی با موفقیت انتخاب شد. PatientId: {PatientId}, InsuranceId: {InsuranceId}", 
                   patientId, createResult.Data.PatientInsuranceId);

                return ServiceResult<PatientInsuranceSelectionResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در انتخاب بیمه تکمیلی. PatientId: {PatientId}", patientId);
                return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                    "خطای غیرمنتظره در انتخاب بیمه تکمیلی: " + ex.Message);
            }
        }

        #endregion

        #region تغییر بیمه

        /// <summary>
        /// تغییر بیمه پایه بیمار
        /// </summary>
        public async Task<ServiceResult<PatientInsuranceSelectionResult>> ChangePrimaryInsuranceAsync(
            int patientId, int newInsurancePlanId, string newPolicyNumber, DateTime startDate, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("🔄 شروع تغییر بیمه پایه. PatientId: {PatientId}, NewPlanId: {NewPlanId}", 
                    patientId, newInsurancePlanId);

                // غیرفعال کردن بیمه پایه قبلی
                var existingPrimary = await _patientInsuranceRepository.GetActivePrimaryInsuranceAsync(patientId);
                if (existingPrimary != null)
                {
                    existingPrimary.IsActive = false;
                    existingPrimary.EndDate = DateTime.Now;
                    existingPrimary.UpdatedByUserId = _currentUserService.UserId;
                    existingPrimary.UpdatedAt = DateTime.Now;
                    
                    await _patientInsuranceRepository.UpdateAsync(existingPrimary);
                }

                // انتخاب بیمه پایه جدید
                return await SelectPrimaryInsuranceAsync(patientId, newInsurancePlanId, newPolicyNumber, startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تغییر بیمه پایه. PatientId: {PatientId}", patientId);
                return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                    "خطای غیرمنتظره در تغییر بیمه پایه: " + ex.Message);
            }
        }

        /// <summary>
        /// تغییر بیمه تکمیلی بیمار
        /// </summary>
        public async Task<ServiceResult<PatientInsuranceSelectionResult>> ChangeSupplementaryInsuranceAsync(
            int patientId, int newInsurancePlanId, string newPolicyNumber, DateTime startDate, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("🔄 شروع تغییر بیمه تکمیلی. PatientId: {PatientId}, NewPlanId: {NewPlanId}", 
                    patientId, newInsurancePlanId);

                // غیرفعال کردن بیمه تکمیلی قبلی
                var existingSupplementary = await _patientInsuranceRepository.GetActiveSupplementaryInsuranceAsync(patientId);
                if (existingSupplementary != null)
                {
                    existingSupplementary.IsActive = false;
                    existingSupplementary.EndDate = DateTime.Now;
                    existingSupplementary.UpdatedByUserId = _currentUserService.UserId;
                    existingSupplementary.UpdatedAt = DateTime.Now;
                    
                    await _patientInsuranceRepository.UpdateAsync(existingSupplementary);
                }

                // انتخاب بیمه تکمیلی جدید
                return await SelectSupplementaryInsuranceAsync(patientId, newInsurancePlanId, newPolicyNumber, startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تغییر بیمه تکمیلی. PatientId: {PatientId}", patientId);
                return ServiceResult<PatientInsuranceSelectionResult>.Failed(
                    "خطای غیرمنتظره در تغییر بیمه تکمیلی: " + ex.Message);
            }
        }

        #endregion

        #region دریافت وضعیت بیمه

        /// <summary>
        /// دریافت وضعیت کامل بیمه بیمار
        /// </summary>
        public async Task<ServiceResult<PatientInsuranceStatus>> GetPatientInsuranceStatusAsync(int patientId)
        {
            try
            {
                _logger.Information("📊 دریافت وضعیت بیمه بیمار. PatientId: {PatientId}", patientId);

                var primaryInsurance = await _patientInsuranceRepository.GetActivePrimaryInsuranceAsync(patientId);
                var supplementaryInsurance = await _patientInsuranceRepository.GetActiveSupplementaryInsuranceAsync(patientId);

                var status = new PatientInsuranceStatus
                {
                    PatientId = patientId,
                    HasPrimaryInsurance = primaryInsurance != null,
                    HasSupplementaryInsurance = supplementaryInsurance != null,
                    PrimaryInsurance = primaryInsurance != null ? new InsuranceInfo
                    {
                        Id = primaryInsurance.PatientInsuranceId,
                        Name = primaryInsurance.InsurancePlan?.Name ?? "نامشخص",
                        PolicyNumber = primaryInsurance.PolicyNumber,
                        StartDate = primaryInsurance.StartDate,
                        EndDate = primaryInsurance.EndDate,
                        IsActive = primaryInsurance.IsActive
                    } : null,
                    SupplementaryInsurance = supplementaryInsurance != null ? new InsuranceInfo
                    {
                        Id = supplementaryInsurance.PatientInsuranceId,
                        Name = supplementaryInsurance.InsurancePlan?.Name ?? "نامشخص",
                        PolicyNumber = supplementaryInsurance.PolicyNumber,
                        StartDate = supplementaryInsurance.StartDate,
                        EndDate = supplementaryInsurance.EndDate,
                        IsActive = supplementaryInsurance.IsActive
                    } : null
                };

                _logger.Information("✅ وضعیت بیمه دریافت شد. PatientId: {PatientId}, HasPrimary: {HasPrimary}, HasSupplementary: {HasSupplementary}", 
                    patientId, status.HasPrimaryInsurance, status.HasSupplementaryInsurance);

                return ServiceResult<PatientInsuranceStatus>.Successful(status);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت وضعیت بیمه. PatientId: {PatientId}", patientId);
                return ServiceResult<PatientInsuranceStatus>.Failed(
                    "خطای غیرمنتظره در دریافت وضعیت بیمه: " + ex.Message);
            }
        }

        #endregion

        #region غیرفعال کردن بیمه

        /// <summary>
        /// غیرفعال کردن بیمه پایه
        /// </summary>
        public async Task<ServiceResult<bool>> DeactivatePrimaryInsuranceAsync(int patientId)
        {
            try
            {
                _logger.Information("🚫 غیرفعال کردن بیمه پایه. PatientId: {PatientId}", patientId);

                var primaryInsurance = await _patientInsuranceRepository.GetActivePrimaryInsuranceAsync(patientId);
                if (primaryInsurance == null)
                {
                    return ServiceResult<bool>.Failed("بیمه پایه فعالی برای این بیمار یافت نشد.");
                }

                primaryInsurance.IsActive = false;
                primaryInsurance.EndDate = DateTime.Now;
                primaryInsurance.UpdatedByUserId = _currentUserService.UserId;
                primaryInsurance.UpdatedAt = DateTime.Now;

                var updateResult = await _patientInsuranceRepository.UpdateAsync(primaryInsurance);
                if (!updateResult.Success)
                {
                    return ServiceResult<bool>.Failed("خطا در غیرفعال کردن بیمه پایه: " + updateResult.Message);
                }

                _logger.Information("✅ بیمه پایه غیرفعال شد. PatientId: {PatientId}", patientId);
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در غیرفعال کردن بیمه پایه. PatientId: {PatientId}", patientId);
                return ServiceResult<bool>.Failed("خطای غیرمنتظره در غیرفعال کردن بیمه پایه: " + ex.Message);
            }
        }

        /// <summary>
        /// غیرفعال کردن بیمه تکمیلی
        /// </summary>
        public async Task<ServiceResult<bool>> DeactivateSupplementaryInsuranceAsync(int patientId)
        {
            try
            {
                _logger.Information("🚫 غیرفعال کردن بیمه تکمیلی. PatientId: {PatientId}", patientId);

                var supplementaryInsurance = await _patientInsuranceRepository.GetActiveSupplementaryInsuranceAsync(patientId);
                if (supplementaryInsurance == null)
                {
                    return ServiceResult<bool>.Failed("بیمه تکمیلی فعالی برای این بیمار یافت نشد.");
                }

                supplementaryInsurance.IsActive = false;
                supplementaryInsurance.EndDate = DateTime.Now;
                supplementaryInsurance.UpdatedByUserId = _currentUserService.UserId;
                supplementaryInsurance.UpdatedAt = DateTime.Now;

                var updateResult = await _patientInsuranceRepository.UpdateAsync(supplementaryInsurance);
                if (!updateResult.Success)
                {
                    return ServiceResult<bool>.Failed("خطا در غیرفعال کردن بیمه تکمیلی: " + updateResult.Message);
                }

                _logger.Information("✅ بیمه تکمیلی غیرفعال شد. PatientId: {PatientId}", patientId);
                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در غیرفعال کردن بیمه تکمیلی. PatientId: {PatientId}", patientId);
                return ServiceResult<bool>.Failed("خطای غیرمنتظره در غیرفعال کردن بیمه تکمیلی: " + ex.Message);
            }
        }

        #endregion
    }
}
