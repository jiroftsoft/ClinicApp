using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس تخصصی مدیریت فرم پذیرش - طراحی شده برای محیط درمانی
    /// مسئولیت: مدیریت کامل فرم پذیرش شامل بیمار، بیمه، خدمات، و محاسبات
    /// </summary>
    public class ReceptionFormService : IReceptionFormService
    {
        private readonly IReceptionService _receptionService;
        private readonly IReceptionCalculationService _receptionCalculationService;
        private readonly IReceptionDomainService _receptionDomainService;
        private readonly IPatientService _patientService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionFormService(
            IReceptionService receptionService,
            IReceptionCalculationService receptionCalculationService,
            IReceptionDomainService receptionDomainService,
            IPatientService patientService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _receptionCalculationService = receptionCalculationService ?? throw new ArgumentNullException(nameof(receptionCalculationService));
            _receptionDomainService = receptionDomainService ?? throw new ArgumentNullException(nameof(receptionDomainService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger.ForContext<ReceptionFormService>();
        }

        #region Reception Form Management

        /// <summary>
        /// ایجاد پذیرش جدید از فرم پذیرش
        /// </summary>
        public async Task<ServiceResult<ReceptionFormResult>> CreateReceptionFromFormAsync(ReceptionFormViewModel model)
        {
            try
            {
                _logger.Information("🏥 RECEPTION_FORM: شروع ایجاد پذیرش از فرم - PatientId: {PatientId}, ServiceCount: {ServiceCount}. User: {UserName}",
                    model.PatientId, model.SelectedServices?.Count ?? 0, _currentUserService.UserName);

                // اعتبارسنجی فرم
                var validationResult = await ValidateReceptionFormAsync(model);
                if (!validationResult.Success)
                {
                    return ServiceResult<ReceptionFormResult>.Failed(validationResult.Message);
                }

                // محاسبه پذیرش
                var calculationResult = await _receptionCalculationService.CalculateReceptionAsync(
                    model.PatientId, 
                    model.SelectedServices.Select(s => s.ServiceId).ToList(), 
                    model.ReceptionDate);

                if (!calculationResult.Success)
                {
                    return ServiceResult<ReceptionFormResult>.Failed($"خطا در محاسبه پذیرش: {calculationResult.Message}");
                }

                // ایجاد پذیرش معتبر با استفاده از Domain Service
                var validReceptionResult = await _receptionDomainService.CreateValidReceptionAsync(model, calculationResult.Data);
                if (!validReceptionResult.Success)
                {
                    _logger.Warning("⚠️ خطا در ایجاد پذیرش معتبر: {Message}. User: {UserName}", 
                        validReceptionResult.Message, _currentUserService.UserName);
                    return ServiceResult<ReceptionFormResult>.Failed(validReceptionResult.Message);
                }

                // ایجاد آیتم‌های پذیرش معتبر
                var validItemsResult = await _receptionDomainService.CreateValidReceptionItemsAsync(
                    calculationResult.Data.ServiceCalculations, 
                    validReceptionResult.Data.ReceptionId);

                if (!validItemsResult.Success)
                {
                    _logger.Warning("⚠️ خطا در ایجاد آیتم‌های پذیرش: {Message}. User: {UserName}", 
                        validItemsResult.Message, _currentUserService.UserName);
                    return ServiceResult<ReceptionFormResult>.Failed(validItemsResult.Message);
                }

                // اعتبارسنجی کامل پذیرش
                var completenessValidationResult = await _receptionDomainService.ValidateReceptionCompletenessAsync(
                    validReceptionResult.Data, validItemsResult.Data);

                if (!completenessValidationResult.Success)
                {
                    _logger.Warning("⚠️ اعتبارسنجی کامل ناموفق: {Message}. User: {UserName}", 
                        completenessValidationResult.Message, _currentUserService.UserName);
                    return ServiceResult<ReceptionFormResult>.Failed(completenessValidationResult.Message);
                }

                // ذخیره در دیتابیس - باید از ReceptionCreateViewModel استفاده کنیم
                var createModel = new ReceptionCreateViewModel
                {
                    PatientId = validReceptionResult.Data.PatientId,
                    ReceptionDate = validReceptionResult.Data.ReceptionDate,
                    TotalAmount = validReceptionResult.Data.TotalAmount,
                    Notes = validReceptionResult.Data.Notes,
                    Status = validReceptionResult.Data.Status
                };
                
                var saveResult = await _receptionService.CreateReceptionAsync(createModel);

                if (saveResult.Success)
                {
                    var result = new ReceptionFormResult
                    {
                        ReceptionId = validReceptionResult.Data.ReceptionId,
                        PatientId = model.PatientId,
                        TotalAmount = calculationResult.Data.TotalServiceAmount,
                        PatientShare = calculationResult.Data.TotalPatientShare,
                        InsuranceCoverage = calculationResult.Data.TotalInsuranceCoverage,
                        ReceptionDate = model.ReceptionDate,
                        Status = "تکمیل شده",
                        ReceptionNumber = validReceptionResult.Data.ReceptionNumber
                    };

                    _logger.Information("✅ پذیرش با موفقیت ایجاد شد - ReceptionId: {ReceptionId}, TotalAmount: {TotalAmount}. User: {UserName}",
                        result.ReceptionId, result.TotalAmount, _currentUserService.UserName);

                    return ServiceResult<ReceptionFormResult>.Successful(result);
                }

                _logger.Warning("⚠️ خطا در ذخیره پذیرش: {Message}. User: {UserName}", saveResult.Message, _currentUserService.UserName);
                return ServiceResult<ReceptionFormResult>.Failed(saveResult.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ایجاد پذیرش از فرم - PatientId: {PatientId}. User: {UserName}",
                    model.PatientId, _currentUserService.UserName);
                return ServiceResult<ReceptionFormResult>.Failed("خطا در ایجاد پذیرش");
            }
        }

        /// <summary>
        /// محاسبه سریع فرم پذیرش
        /// </summary>
        public async Task<ServiceResult<ReceptionFormCalculation>> CalculateReceptionFormAsync(ReceptionFormCalculationRequest request)
        {
            try
            {
                _logger.Information("🏥 RECEPTION_FORM: شروع محاسبه فرم پذیرش - PatientId: {PatientId}, ServiceCount: {ServiceCount}. User: {UserName}",
                    request.PatientId, request.ServiceIds?.Count ?? 0, _currentUserService.UserName);

                var calculationResult = await _receptionCalculationService.CalculateReceptionAsync(
                    request.PatientId, 
                    request.ServiceIds, 
                    request.ReceptionDate);

                if (calculationResult.Success)
                {
                    var result = new ReceptionFormCalculation
                    {
                        PatientId = request.PatientId,
                        TotalServiceAmount = calculationResult.Data.TotalServiceAmount,
                        TotalInsuranceCoverage = calculationResult.Data.TotalInsuranceCoverage,
                        TotalPatientShare = calculationResult.Data.TotalPatientShare,
                        HasInsurance = calculationResult.Data.HasInsurance,
                        ServiceCalculations = calculationResult.Data.ServiceCalculations,
                        CalculationDate = DateTime.Now
                    };

                    _logger.Information("✅ محاسبه فرم پذیرش موفق - TotalAmount: {TotalAmount}, PatientShare: {PatientShare}. User: {UserName}",
                        result.TotalServiceAmount, result.TotalPatientShare, _currentUserService.UserName);

                    return ServiceResult<ReceptionFormCalculation>.Successful(result);
                }

                _logger.Warning("⚠️ خطا در محاسبه فرم پذیرش: {Message}. User: {UserName}", calculationResult.Message, _currentUserService.UserName);
                return ServiceResult<ReceptionFormCalculation>.Failed(calculationResult.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در محاسبه فرم پذیرش - PatientId: {PatientId}. User: {UserName}",
                    request.PatientId, _currentUserService.UserName);
                return ServiceResult<ReceptionFormCalculation>.Failed("خطا در محاسبه فرم پذیرش");
            }
        }

        /// <summary>
        /// دریافت اطلاعات فرم پذیرش
        /// </summary>
        public async Task<ServiceResult<ReceptionFormInfo>> GetReceptionFormInfoAsync(int patientId)
        {
            try
            {
                _logger.Information("🏥 RECEPTION_FORM: درخواست اطلاعات فرم پذیرش - PatientId: {PatientId}. User: {UserName}",
                    patientId, _currentUserService.UserName);

                // دریافت اطلاعات بیمار
                var patientResult = await _patientService.GetPatientDetailsAsync(patientId);
                if (!patientResult.Success)
                {
                    return ServiceResult<ReceptionFormInfo>.Failed("اطلاعات بیمار یافت نشد");
                }

                var result = new ReceptionFormInfo
                {
                    PatientId = patientId,
                    PatientName = patientResult.Data.FullName,
                    NationalCode = patientResult.Data.NationalCode,
                    PhoneNumber = patientResult.Data.PhoneNumber,
                    ReceptionDate = DateTime.Now,
                    AvailableServices = new List<ServiceOption>(),
                    InsuranceInfo = new InsuranceInfo()
                };

                _logger.Information("✅ اطلاعات فرم پذیرش دریافت شد - PatientName: {PatientName}. User: {UserName}",
                    result.PatientName, _currentUserService.UserName);

                return ServiceResult<ReceptionFormInfo>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت اطلاعات فرم پذیرش - PatientId: {PatientId}. User: {UserName}",
                    patientId, _currentUserService.UserName);
                return ServiceResult<ReceptionFormInfo>.Failed("خطا در دریافت اطلاعات فرم پذیرش");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// اعتبارسنجی فرم پذیرش
        /// </summary>
        private async Task<ServiceResult> ValidateReceptionFormAsync(ReceptionFormViewModel model)
        {
            try
            {
                // اعتبارسنجی بیمار
                if (model.PatientId <= 0)
                {
                    return ServiceResult.Failed("شناسه بیمار نامعتبر است");
                }

                // اعتبارسنجی خدمات
                if (model.SelectedServices == null || !model.SelectedServices.Any())
                {
                    return ServiceResult.Failed("حداقل یک خدمت باید انتخاب شود");
                }

                // اعتبارسنجی تاریخ
                if (model.ReceptionDate < DateTime.Today)
                {
                    return ServiceResult.Failed("تاریخ پذیرش نمی‌تواند در گذشته باشد");
                }

                return ServiceResult.Successful("اعتبارسنجی موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در اعتبارسنجی فرم پذیرش");
                return ServiceResult.Failed("خطا در اعتبارسنجی فرم پذیرش");
            }
        }

        #endregion
    }
}