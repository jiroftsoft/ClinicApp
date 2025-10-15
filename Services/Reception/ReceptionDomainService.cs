using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.DataSeeding;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس دامنه تخصصی پذیرش - طراحی شده برای محیط درمانی
    /// مسئولیت: مدیریت منطق کسب‌وکار و ایجاد Entity های معتبر
    /// </summary>
    public class ReceptionDomainService : IReceptionDomainService
    {
        private readonly IReceptionService _receptionService;
        private readonly IReceptionCalculationService _receptionCalculationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionDomainService(
            IReceptionService receptionService,
            IReceptionCalculationService receptionCalculationService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _receptionCalculationService = receptionCalculationService ?? throw new ArgumentNullException(nameof(receptionCalculationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger.ForContext<ReceptionDomainService>();
        }

        #region Domain Business Logic

        /// <summary>
        /// ایجاد پذیرش معتبر با اعتبارسنجی کامل
        /// </summary>
        public async Task<ServiceResult<Models.Entities.Reception.Reception>> CreateValidReceptionAsync(ReceptionFormViewModel model, ReceptionCalculationResult calculation)
        {
            try
            {
                _logger.Information("🏥 DOMAIN: شروع ایجاد پذیرش معتبر - PatientId: {PatientId}, Date: {Date}. User: {UserName}",
                    model.PatientId, model.ReceptionDate, _currentUserService.UserName);

                // اعتبارسنجی دامنه
                var validationResult = await ValidateReceptionDomainAsync(model, calculation);
                if (!validationResult.Success)
                {
                    _logger.Warning("⚠️ اعتبارسنجی دامنه ناموفق: {Message}. User: {UserName}", 
                        validationResult.Message, _currentUserService.UserName);
                    return ServiceResult<Models.Entities.Reception.Reception>.Failed(validationResult.Message);
                }

                // ایجاد Entity معتبر
                var reception = await BuildValidReceptionEntityAsync(model, calculation);

                _logger.Information("✅ پذیرش معتبر ایجاد شد - ReceptionId: {ReceptionId}, TotalAmount: {TotalAmount}. User: {UserName}",
                    reception.ReceptionId, reception.TotalAmount, _currentUserService.UserName);

                return ServiceResult<Models.Entities.Reception.Reception>.Successful(reception);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ایجاد پذیرش معتبر - PatientId: {PatientId}. User: {UserName}",
                    model.PatientId, _currentUserService.UserName);
                return ServiceResult<Models.Entities.Reception.Reception>.Failed("خطا در ایجاد پذیرش معتبر");
            }
        }

        /// <summary>
        /// ایجاد آیتم‌های پذیرش معتبر
        /// </summary>
        public async Task<ServiceResult<List<Models.Entities.Reception.ReceptionItem>>> CreateValidReceptionItemsAsync(
            List<ViewModels.Reception.ServiceCalculationResult> serviceCalculations, 
            int receptionId)
        {
            try
            {
                _logger.Information("🏥 DOMAIN: شروع ایجاد آیتم‌های پذیرش معتبر - ReceptionId: {ReceptionId}, ItemCount: {Count}. User: {UserName}",
                    receptionId, serviceCalculations.Count, _currentUserService.UserName);

                var receptionItems = new List<Models.Entities.Reception.ReceptionItem>();

                foreach (var serviceCalculation in serviceCalculations)
                {
                    // اعتبارسنجی هر آیتم
                    var itemValidation = ValidateReceptionItemDomain(serviceCalculation);
                    if (!itemValidation.Success)
                    {
                        _logger.Warning("⚠️ اعتبارسنجی آیتم ناموفق: {Message}. User: {UserName}", 
                            itemValidation.Message, _currentUserService.UserName);
                        return ServiceResult<List<Models.Entities.Reception.ReceptionItem>>.Failed(itemValidation.Message);
                    }

                    // ایجاد آیتم معتبر
                    var item = await BuildValidReceptionItemEntityAsync(serviceCalculation, receptionId);
                    receptionItems.Add(item);
                }

                _logger.Information("✅ {Count} آیتم پذیرش معتبر ایجاد شد. User: {UserName}", 
                    receptionItems.Count, _currentUserService.UserName);

                return ServiceResult<List<Models.Entities.Reception.ReceptionItem>>.Successful(receptionItems);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ایجاد آیتم‌های پذیرش معتبر - ReceptionId: {ReceptionId}. User: {UserName}",
                    receptionId, _currentUserService.UserName);
                return ServiceResult<List<Models.Entities.Reception.ReceptionItem>>.Failed("خطا در ایجاد آیتم‌های پذیرش معتبر");
            }
        }

        /// <summary>
        /// اعتبارسنجی کامل پذیرش
        /// </summary>
        public async Task<ServiceResult> ValidateReceptionCompletenessAsync(Models.Entities.Reception.Reception reception, List<Models.Entities.Reception.ReceptionItem> items)
        {
            try
            {
                _logger.Information("🏥 DOMAIN: شروع اعتبارسنجی کامل پذیرش - ReceptionId: {ReceptionId}. User: {UserName}",
                    reception.ReceptionId, _currentUserService.UserName);

                // اعتبارسنجی مبلغ کل
                var totalCalculated = items.Sum(i => i.PatientShareAmount + i.InsurerShareAmount);
                if (Math.Abs(reception.TotalAmount - totalCalculated) > ReceptionConstants.BusinessRules.ToleranceAmount)
                {
                    return ServiceResult.Failed("مبلغ کل پذیرش با مجموع آیتم‌ها مطابقت ندارد");
                }

                // اعتبارسنجی پوشش بیمه
                var insuranceCalculated = items.Sum(i => i.InsurerShareAmount);
                if (Math.Abs(reception.InsurerShareAmount - insuranceCalculated) > ReceptionConstants.BusinessRules.ToleranceAmount)
                {
                    return ServiceResult.Failed("پوشش بیمه با محاسبات مطابقت ندارد");
                }

                // اعتبارسنجی سهم بیمار
                var patientShareCalculated = items.Sum(i => i.PatientShareAmount);
                if (Math.Abs(reception.PatientCoPay - patientShareCalculated) > ReceptionConstants.BusinessRules.ToleranceAmount)
                {
                    return ServiceResult.Failed("سهم بیمار با محاسبات مطابقت ندارد");
                }

                _logger.Information("✅ اعتبارسنجی کامل پذیرش موفق. User: {UserName}", _currentUserService.UserName);
                return ServiceResult.Successful("اعتبارسنجی کامل موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در اعتبارسنجی کامل پذیرش - ReceptionId: {ReceptionId}. User: {UserName}",
                    reception.ReceptionId, _currentUserService.UserName);
                return ServiceResult.Failed("خطا در اعتبارسنجی کامل پذیرش");
            }
        }

        #endregion

        #region Private Domain Methods

        /// <summary>
        /// اعتبارسنجی دامنه پذیرش
        /// </summary>
        private async Task<ServiceResult> ValidateReceptionDomainAsync(ReceptionFormViewModel model, ReceptionCalculationResult calculation)
        {
            try
            {
                // اعتبارسنجی بیمار
                if (model.PatientId <= 0)
                {
                    return ServiceResult.Failed("شناسه بیمار نامعتبر است");
                }

                // اعتبارسنجی تاریخ
                if (model.ReceptionDate < DateTime.Today)
                {
                    return ServiceResult.Failed("تاریخ پذیرش نمی‌تواند در گذشته باشد");
                }

                // اعتبارسنجی مبلغ
                if (calculation.TotalServiceAmount <= 0)
                {
                    return ServiceResult.Failed("مبلغ کل پذیرش باید مثبت باشد");
                }

                // اعتبارسنجی منطق بیمه
                if (calculation.TotalInsuranceCoverage + calculation.TotalPatientShare != calculation.TotalServiceAmount)
                {
                    return ServiceResult.Failed("محاسبات بیمه و سهم بیمار صحیح نیست");
                }

                return ServiceResult.Successful("اعتبارسنجی دامنه موفق");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در اعتبارسنجی دامنه پذیرش");
                return ServiceResult.Failed("خطا در اعتبارسنجی دامنه پذیرش");
            }
        }

        /// <summary>
        /// ایجاد Entity پذیرش معتبر
        /// </summary>
        private async Task<Models.Entities.Reception.Reception> BuildValidReceptionEntityAsync(ReceptionFormViewModel model, ReceptionCalculationResult calculation)
        {
            var reception = new Models.Entities.Reception.Reception
            {
                PatientId = model.PatientId,
                ReceptionDate = model.ReceptionDate,
                TotalAmount = calculation.TotalServiceAmount,
                InsurerShareAmount = calculation.TotalInsuranceCoverage,
                PatientCoPay = calculation.TotalPatientShare,
                Notes = model.Notes,
                Status = Models.Enums.ReceptionStatus.Pending,
                CreatedByUserId = _currentUserService.UserId,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            return reception;
        }

        /// <summary>
        /// اعتبارسنجی آیتم پذیرش
        /// </summary>
        private ServiceResult ValidateReceptionItemDomain(ViewModels.Reception.ServiceCalculationResult serviceCalculation)
        {
            if (serviceCalculation.ServiceId <ReceptionConstants.ValidationLimits.MinPatientId)
            {
                return ServiceResult.Failed("شناسه خدمت نامعتبر است");
            }

            if (serviceCalculation.ServiceAmount <ReceptionConstants.ValidationLimits.MinAmount)
            {
                return ServiceResult.Failed("مبلغ خدمت باید مثبت باشد");
            }

            if (serviceCalculation.InsuranceCoverage < 0 || serviceCalculation.PatientShare < 0)
            {
                return ServiceResult.Failed("پوشش بیمه و سهم بیمار نمی‌تواند منفی باشد");
            }

            if (serviceCalculation.InsuranceCoverage + serviceCalculation.PatientShare != serviceCalculation.ServiceAmount)
            {
                return ServiceResult.Failed("محاسبات آیتم صحیح نیست");
            }

            return ServiceResult.Successful("اعتبارسنجی آیتم موفق");
        }

        /// <summary>
        /// ایجاد Entity آیتم پذیرش معتبر
        /// </summary>
        private async Task<ReceptionItem> BuildValidReceptionItemEntityAsync(ViewModels.Reception.ServiceCalculationResult serviceCalculation, int receptionId)
        {
            return new ReceptionItem
            {
                ReceptionId = receptionId,
                ServiceId = serviceCalculation.ServiceId,
                PatientShareAmount = serviceCalculation.PatientShare,
                InsurerShareAmount = serviceCalculation.InsuranceCoverage,
                Quantity = ReceptionConstants.BusinessRules.DefaultQuantity,
                CreatedByUserId = _currentUserService.UserId,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };
        }

        /// <summary>
        /// تولید شماره پذیرش منحصر به فرد
        /// </summary>
        private async Task<string> GenerateUniqueReceptionNumberAsync()
        {
            var datePrefix = DateTime.Now.ToString("yyyyMMdd");
            var randomSuffix = new Random().Next(
                (int)Math.Pow(10, ReceptionConstants.BusinessRules.ReceptionNumberLength - 1),
                (int)Math.Pow(10, ReceptionConstants.BusinessRules.ReceptionNumberLength) - 1);
            return $"{ReceptionConstants.DefaultValues.ReceptionNumberPrefix}-{datePrefix}-{randomSuffix}";
        }

        #endregion
    }
}
