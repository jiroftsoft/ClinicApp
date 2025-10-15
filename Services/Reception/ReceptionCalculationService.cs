using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.ViewModels.Reception;
using Serilog;
using ServiceCalculationResult = ClinicApp.ViewModels.Reception.ServiceCalculationResult;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس تخصصی محاسبات فرم پذیرش - طراحی شده برای محیط درمانی
    /// مسئولیت: محاسبات تخصصی پذیرش شامل بیمه، خدمات، و پرداخت
    /// </summary>
    public class ReceptionCalculationService : IReceptionCalculationService
    {
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly IServiceRepository _serviceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionCalculationService(
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            IServiceRepository serviceRepository,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger.ForContext<ReceptionCalculationService>();
        }

        #region Reception-Specific Calculations

        /// <summary>
        /// محاسبه کامل پذیرش برای فرم پذیرش
        /// </summary>
        public async Task<ServiceResult<ReceptionCalculationResult>> CalculateReceptionAsync(
            int patientId, 
            List<int> serviceIds, 
            DateTime receptionDate)
        {
            try
            {
                _logger.Information("🏥 RECEPTION: شروع محاسبه پذیرش - PatientId: {PatientId}, ServiceCount: {ServiceCount}, Date: {Date}. User: {UserName}",
                    patientId, serviceIds.Count, receptionDate, _currentUserService.UserName);

                var result = new ReceptionCalculationResult
                {
                    PatientId = patientId,
                    ReceptionDate = receptionDate,
                    ServiceCalculations = new List<ServiceCalculationResult>(),
                    TotalServiceAmount = 0,
                    TotalInsuranceCoverage = 0,
                    TotalPatientShare = 0,
                    HasInsurance = false,
                    CalculationDate = DateTime.Now
                };

                // محاسبه هر خدمت
                foreach (var serviceId in serviceIds)
                {
                    var serviceResult = await CalculateServiceForReceptionAsync(patientId, serviceId, receptionDate);
                    if (serviceResult.Success)
                    {
                        result.ServiceCalculations.Add(serviceResult.Data);
                        result.TotalServiceAmount += serviceResult.Data.ServiceAmount;
                        result.TotalInsuranceCoverage += serviceResult.Data.InsuranceCoverage;
                        result.TotalPatientShare += serviceResult.Data.PatientShare;
                        result.HasInsurance = true;
                    }
                }

                _logger.Information("🏥 RECEPTION: محاسبه پذیرش تکمیل شد - TotalAmount: {TotalAmount}, InsuranceCoverage: {InsuranceCoverage}, PatientShare: {PatientShare}. User: {UserName}",
                    result.TotalServiceAmount, result.TotalInsuranceCoverage, result.TotalPatientShare, _currentUserService.UserName);

                return ServiceResult<ReceptionCalculationResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 RECEPTION: خطا در محاسبه پذیرش - PatientId: {PatientId}. User: {UserName}",
                    patientId, _currentUserService.UserName);
                return ServiceResult<ReceptionCalculationResult>.Failed("خطا در محاسبه پذیرش");
            }
        }

        /// <summary>
        /// محاسبه یک خدمت برای پذیرش
        /// </summary>
        public async Task<ServiceResult<ViewModels.Reception.ServiceCalculationResult>> CalculateServiceForReceptionAsync(
            int patientId, 
            int serviceId, 
            DateTime receptionDate)
        {
            try
            {
                _logger.Information("🏥 RECEPTION: شروع محاسبه خدمت - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName}",
                    patientId, serviceId, _currentUserService.UserName);

                // دریافت اطلاعات خدمت
                var service = await _serviceRepository.GetServiceByIdAsync(serviceId);
                if (service == null)
                {
                    return ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Failed("خدمت یافت نشد");
                }
                var serviceAmount = service.Price;

                // محاسبه بیمه ترکیبی
                var insuranceResult = await _combinedInsuranceCalculationService.CalculateCombinedInsuranceAsync(
                    patientId, serviceId, serviceAmount, receptionDate);

                var result = new ViewModels.Reception.ServiceCalculationResult
                {
                    ServiceId = serviceId,
                    ServiceName = service.Title,
                    ServiceCode = service.ServiceCode,
                    ServiceAmount = serviceAmount,
                    InsuranceCoverage = insuranceResult.Success ? insuranceResult.Data.TotalInsuranceCoverage : 0,
                    PatientShare = insuranceResult.Success ? insuranceResult.Data.FinalPatientShare : serviceAmount,
                    HasInsurance = insuranceResult.Success,
                    InsuranceDetails = insuranceResult.Success ? insuranceResult.Data : null
                };

                _logger.Information("🏥 RECEPTION: محاسبه خدمت تکمیل شد - ServiceId: {ServiceId}, Amount: {Amount}, PatientShare: {PatientShare}. User: {UserName}",
                    serviceId, serviceAmount, result.PatientShare, _currentUserService.UserName);

                return ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 RECEPTION: خطا در محاسبه خدمت - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName}",
                    patientId, serviceId, _currentUserService.UserName);
                return ServiceResult<ViewModels.Reception.ServiceCalculationResult>.Failed("خطا در محاسبه خدمت");
            }
        }

        /// <summary>
        /// محاسبه سریع برای نمایش در فرم پذیرش
        /// </summary>
        public async Task<ServiceResult<QuickReceptionCalculation>> CalculateQuickReceptionAsync(
            int patientId, 
            int serviceId, 
            decimal? customAmount = null)
        {
            try
            {
                _logger.Information("🏥 RECEPTION: شروع محاسبه سریع - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName}",
                    patientId, serviceId, _currentUserService.UserName);

                // دریافت اطلاعات خدمت
                var service = await _serviceRepository.GetServiceByIdAsync(serviceId);
                if (service == null)
                {
                    return ServiceResult<QuickReceptionCalculation>.Failed("خدمت یافت نشد");
                }
                var serviceAmount = customAmount ?? service.Price;

                // محاسبه بیمه ترکیبی
                var insuranceResult = await _combinedInsuranceCalculationService.CalculateCombinedInsuranceAsync(
                    patientId, serviceId, serviceAmount, DateTime.Now);

                var result = new QuickReceptionCalculation
                {
                    ServiceId = serviceId,
                    ServiceName = service.Title,
                    ServiceAmount = serviceAmount,
                    InsuranceCoverage = insuranceResult.Success ? insuranceResult.Data.TotalInsuranceCoverage : 0,
                    PatientShare = insuranceResult.Success ? insuranceResult.Data.FinalPatientShare : serviceAmount,
                    HasInsurance = insuranceResult.Success,
                    CoveragePercent = insuranceResult.Success ? 
                        (serviceAmount > 0 ? (insuranceResult.Data.TotalInsuranceCoverage / serviceAmount) * 100 : 0) : 0
                };

                _logger.Information("🏥 RECEPTION: محاسبه سریع تکمیل شد - ServiceId: {ServiceId}, PatientShare: {PatientShare}, Coverage: {Coverage}%. User: {UserName}",
                    serviceId, result.PatientShare, result.CoveragePercent, _currentUserService.UserName);

                return ServiceResult<QuickReceptionCalculation>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 RECEPTION: خطا در محاسبه سریع - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName}",
                    patientId, serviceId, _currentUserService.UserName);
                return ServiceResult<QuickReceptionCalculation>.Failed("خطا در محاسبه سریع");
            }
        }

        #endregion
    }
}