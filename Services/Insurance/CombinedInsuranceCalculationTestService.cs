using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس تست محاسبه بیمه ترکیبی
    /// طراحی شده برای محیط درمانی کلینیک شفا
    /// </summary>
    public class CombinedInsuranceCalculationTestService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICombinedInsuranceCalculationService _calculationService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public CombinedInsuranceCalculationTestService(
            ApplicationDbContext context,
            ICombinedInsuranceCalculationService calculationService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _log = logger.ForContext<CombinedInsuranceCalculationTestService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// تست محاسبه بیمه ترکیبی با سناریوهای مختلف
        /// </summary>
        public async Task<ServiceResult<TestResults>> RunComprehensiveTestsAsync()
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع تست‌های جامع محاسبه بیمه ترکیبی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var testResults = new TestResults
                {
                    TestStartTime = DateTime.UtcNow,
                    TestScenarios = new List<TestScenario>()
                };

                // سناریو 1: بیمه اصلی + بیمه تکمیلی (پوشش کامل)
                await RunScenario1(testResults);

                // سناریو 2: بیمه اصلی + بیمه تکمیلی (پوشش جزئی)
                await RunScenario2(testResults);

                // سناریو 3: فقط بیمه اصلی
                await RunScenario3(testResults);

                // سناریو 4: فقط بیمه تکمیلی
                await RunScenario4(testResults);

                // سناریو 5: بیمه آزاد + بیمه تکمیلی
                await RunScenario5(testResults);

                testResults.TestEndTime = DateTime.UtcNow;
                testResults.TotalDuration = testResults.TestEndTime - testResults.TestStartTime;
                testResults.SuccessRate = CalculateSuccessRate(testResults.TestScenarios);

                _log.Information("🏥 MEDICAL: تست‌های جامع محاسبه بیمه ترکیبی تکمیل شد - SuccessRate: {SuccessRate}%, TotalScenarios: {TotalScenarios}, Duration: {Duration}. User: {UserName} (Id: {UserId})",
                    testResults.SuccessRate, testResults.TestScenarios.Count, testResults.TotalDuration, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<TestResults>.Successful(testResults);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اجرای تست‌های جامع محاسبه بیمه ترکیبی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<TestResults>.Failed("خطا در اجرای تست‌های جامع محاسبه بیمه ترکیبی");
            }
        }

        /// <summary>
        /// سناریو 1: بیمه اصلی (70%) + بیمه تکمیلی (100% باقی‌مانده)
        /// </summary>
        private async Task RunScenario1(TestResults testResults)
        {
            try
            {
                _log.Information("🏥 MEDICAL: اجرای سناریو 1 - بیمه اصلی (70%) + بیمه تکمیلی (100% باقی‌مانده). User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var scenario = new TestScenario
                {
                    ScenarioName = "بیمه اصلی (70%) + بیمه تکمیلی (100% باقی‌مانده)",
                    ExpectedPatientShare = 0, // بیمار نباید چیزی پرداخت کند
                    ExpectedInsuranceCoverage = 100 // بیمه باید 100% پوشش دهد
                };

                // دریافت بیمار با بیمه اصلی و تکمیلی
                var patient = await GetTestPatientWithBothInsurances();
                if (patient == null)
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = "بیمار با بیمه اصلی و تکمیلی یافت نشد";
                    testResults.TestScenarios.Add(scenario);
                    return;
                }

                // دریافت خدمت
                var service = await GetTestService();
                if (service == null)
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = "خدمت تست یافت نشد";
                    testResults.TestScenarios.Add(scenario);
                    return;
                }

                // محاسبه بیمه ترکیبی
                var result = await _calculationService.CalculateCombinedInsuranceAsync(
                    patient.PatientId, 
                    service.ServiceId, 
                    service.Price, 
                    DateTime.Now);
                
                if (result.Success)
                {
                    scenario.ActualPatientShare = result.Data.FinalPatientShare;
                    scenario.ActualInsuranceCoverage = result.Data.TotalInsuranceCoverage;
                    scenario.IsSuccess = Math.Abs(scenario.ActualPatientShare - scenario.ExpectedPatientShare) < 0.01m;
                    
                    if (scenario.IsSuccess)
                    {
                        _log.Information("🏥 MEDICAL: سناریو 1 موفق - PatientShare: {PatientShare}, InsuranceCoverage: {InsuranceCoverage}. User: {UserName} (Id: {UserId})",
                            scenario.ActualPatientShare, scenario.ActualInsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: سناریو 1 ناموفق - ExpectedPatientShare: {ExpectedPatientShare}, ActualPatientShare: {ActualPatientShare}. User: {UserName} (Id: {UserId})",
                            scenario.ExpectedPatientShare, scenario.ActualPatientShare, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }
                else
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = result.Message;
                    _log.Warning("🏥 MEDICAL: سناریو 1 ناموفق - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                testResults.TestScenarios.Add(scenario);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اجرای سناریو 1. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
            }
        }

        /// <summary>
        /// سناریو 2: بیمه اصلی (70%) + بیمه تکمیلی (50% باقی‌مانده)
        /// </summary>
        private async Task RunScenario2(TestResults testResults)
        {
            try
            {
                _log.Information("🏥 MEDICAL: اجرای سناریو 2 - بیمه اصلی (70%) + بیمه تکمیلی (50% باقی‌مانده). User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var scenario = new TestScenario
                {
                    ScenarioName = "بیمه اصلی (70%) + بیمه تکمیلی (50% باقی‌مانده)",
                    ExpectedPatientShare = 15, // بیمار باید 15% پرداخت کند
                    ExpectedInsuranceCoverage = 85 // بیمه باید 85% پوشش دهد
                };

                // دریافت بیمار با بیمه اصلی و تکمیلی
                var patient = await GetTestPatientWithBothInsurances();
                if (patient == null)
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = "بیمار با بیمه اصلی و تکمیلی یافت نشد";
                    testResults.TestScenarios.Add(scenario);
                    return;
                }

                // دریافت خدمت
                var service = await GetTestService();
                if (service == null)
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = "خدمت تست یافت نشد";
                    testResults.TestScenarios.Add(scenario);
                    return;
                }

                // محاسبه بیمه ترکیبی
                var result = await _calculationService.CalculateCombinedInsuranceAsync(
                    patient.PatientId, 
                    service.ServiceId, 
                    service.Price, 
                    DateTime.Now);
                
                if (result.Success)
                {
                    scenario.ActualPatientShare = result.Data.FinalPatientShare;
                    scenario.ActualInsuranceCoverage = result.Data.TotalInsuranceCoverage;
                    scenario.IsSuccess = Math.Abs(scenario.ActualPatientShare - scenario.ExpectedPatientShare) < 0.01m;
                    
                    if (scenario.IsSuccess)
                    {
                        _log.Information("🏥 MEDICAL: سناریو 2 موفق - PatientShare: {PatientShare}, InsuranceCoverage: {InsuranceCoverage}. User: {UserName} (Id: {UserId})",
                            scenario.ActualPatientShare, scenario.ActualInsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: سناریو 2 ناموفق - ExpectedPatientShare: {ExpectedPatientShare}, ActualPatientShare: {ActualPatientShare}. User: {UserName} (Id: {UserId})",
                            scenario.ExpectedPatientShare, scenario.ActualPatientShare, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }
                else
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = result.Message;
                    _log.Warning("🏥 MEDICAL: سناریو 2 ناموفق - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                testResults.TestScenarios.Add(scenario);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اجرای سناریو 2. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
            }
        }

        /// <summary>
        /// سناریو 3: فقط بیمه اصلی
        /// </summary>
        private async Task RunScenario3(TestResults testResults)
        {
            try
            {
                _log.Information("🏥 MEDICAL: اجرای سناریو 3 - فقط بیمه اصلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var scenario = new TestScenario
                {
                    ScenarioName = "فقط بیمه اصلی (70%)",
                    ExpectedPatientShare = 30, // بیمار باید 30% پرداخت کند
                    ExpectedInsuranceCoverage = 70 // بیمه باید 70% پوشش دهد
                };

                // دریافت بیمار با فقط بیمه اصلی
                var patient = await GetTestPatientWithPrimaryInsuranceOnly();
                if (patient == null)
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = "بیمار با بیمه اصلی یافت نشد";
                    testResults.TestScenarios.Add(scenario);
                    return;
                }

                // دریافت خدمت
                var service = await GetTestService();
                if (service == null)
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = "خدمت تست یافت نشد";
                    testResults.TestScenarios.Add(scenario);
                    return;
                }

                // محاسبه بیمه ترکیبی
                var result = await _calculationService.CalculateCombinedInsuranceAsync(
                    patient.PatientId, 
                    service.ServiceId, 
                    service.Price, 
                    DateTime.Now);
                
                if (result.Success)
                {
                    scenario.ActualPatientShare = result.Data.FinalPatientShare;
                    scenario.ActualInsuranceCoverage = result.Data.TotalInsuranceCoverage;
                    scenario.IsSuccess = Math.Abs(scenario.ActualPatientShare - scenario.ExpectedPatientShare) < 0.01m;
                    
                    if (scenario.IsSuccess)
                    {
                        _log.Information("🏥 MEDICAL: سناریو 3 موفق - PatientShare: {PatientShare}, InsuranceCoverage: {InsuranceCoverage}. User: {UserName} (Id: {UserId})",
                            scenario.ActualPatientShare, scenario.ActualInsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: سناریو 3 ناموفق - ExpectedPatientShare: {ExpectedPatientShare}, ActualPatientShare: {ActualPatientShare}. User: {UserName} (Id: {UserId})",
                            scenario.ExpectedPatientShare, scenario.ActualPatientShare, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }
                else
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = result.Message;
                    _log.Warning("🏥 MEDICAL: سناریو 3 ناموفق - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                testResults.TestScenarios.Add(scenario);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اجرای سناریو 3. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
            }
        }

        /// <summary>
        /// سناریو 4: فقط بیمه تکمیلی
        /// </summary>
        private async Task RunScenario4(TestResults testResults)
        {
            try
            {
                _log.Information("🏥 MEDICAL: اجرای سناریو 4 - فقط بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var scenario = new TestScenario
                {
                    ScenarioName = "فقط بیمه تکمیلی (90%)",
                    ExpectedPatientShare = 10, // بیمار باید 10% پرداخت کند
                    ExpectedInsuranceCoverage = 90 // بیمه باید 90% پوشش دهد
                };

                // دریافت بیمار با فقط بیمه تکمیلی
                var patient = await GetTestPatientWithSupplementaryInsuranceOnly();
                if (patient == null)
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = "بیمار با بیمه تکمیلی یافت نشد";
                    testResults.TestScenarios.Add(scenario);
                    return;
                }

                // دریافت خدمت
                var service = await GetTestService();
                if (service == null)
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = "خدمت تست یافت نشد";
                    testResults.TestScenarios.Add(scenario);
                    return;
                }

                // محاسبه بیمه ترکیبی
                var result = await _calculationService.CalculateCombinedInsuranceAsync(
                    patient.PatientId, 
                    service.ServiceId, 
                    service.Price, 
                    DateTime.Now);
                
                if (result.Success)
                {
                    scenario.ActualPatientShare = result.Data.FinalPatientShare;
                    scenario.ActualInsuranceCoverage = result.Data.TotalInsuranceCoverage;
                    scenario.IsSuccess = Math.Abs(scenario.ActualPatientShare - scenario.ExpectedPatientShare) < 0.01m;
                    
                    if (scenario.IsSuccess)
                    {
                        _log.Information("🏥 MEDICAL: سناریو 4 موفق - PatientShare: {PatientShare}, InsuranceCoverage: {InsuranceCoverage}. User: {UserName} (Id: {UserId})",
                            scenario.ActualPatientShare, scenario.ActualInsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: سناریو 4 ناموفق - ExpectedPatientShare: {ExpectedPatientShare}, ActualPatientShare: {ActualPatientShare}. User: {UserName} (Id: {UserId})",
                            scenario.ExpectedPatientShare, scenario.ActualPatientShare, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }
                else
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = result.Message;
                    _log.Warning("🏥 MEDICAL: سناریو 4 ناموفق - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                testResults.TestScenarios.Add(scenario);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اجرای سناریو 4. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
            }
        }

        /// <summary>
        /// سناریو 5: بیمه آزاد + بیمه تکمیلی
        /// </summary>
        private async Task RunScenario5(TestResults testResults)
        {
            try
            {
                _log.Information("🏥 MEDICAL: اجرای سناریو 5 - بیمه آزاد + بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var scenario = new TestScenario
                {
                    ScenarioName = "بیمه آزاد (0%) + بیمه تکمیلی (90%)",
                    ExpectedPatientShare = 10, // بیمار باید 10% پرداخت کند
                    ExpectedInsuranceCoverage = 90 // بیمه باید 90% پوشش دهد
                };

                // دریافت بیمار با بیمه آزاد و تکمیلی
                var patient = await GetTestPatientWithFreeAndSupplementaryInsurance();
                if (patient == null)
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = "بیمار با بیمه آزاد و تکمیلی یافت نشد";
                    testResults.TestScenarios.Add(scenario);
                    return;
                }

                // دریافت خدمت
                var service = await GetTestService();
                if (service == null)
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = "خدمت تست یافت نشد";
                    testResults.TestScenarios.Add(scenario);
                    return;
                }

                // محاسبه بیمه ترکیبی
                var result = await _calculationService.CalculateCombinedInsuranceAsync(
                    patient.PatientId, 
                    service.ServiceId, 
                    service.Price, 
                    DateTime.Now);
                
                if (result.Success)
                {
                    scenario.ActualPatientShare = result.Data.FinalPatientShare;
                    scenario.ActualInsuranceCoverage = result.Data.TotalInsuranceCoverage;
                    scenario.IsSuccess = Math.Abs(scenario.ActualPatientShare - scenario.ExpectedPatientShare) < 0.01m;
                    
                    if (scenario.IsSuccess)
                    {
                        _log.Information("🏥 MEDICAL: سناریو 5 موفق - PatientShare: {PatientShare}, InsuranceCoverage: {InsuranceCoverage}. User: {UserName} (Id: {UserId})",
                            scenario.ActualPatientShare, scenario.ActualInsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: سناریو 5 ناموفق - ExpectedPatientShare: {ExpectedPatientShare}, ActualPatientShare: {ActualPatientShare}. User: {UserName} (Id: {UserId})",
                            scenario.ExpectedPatientShare, scenario.ActualPatientShare, _currentUserService.UserName, _currentUserService.UserId);
                    }
                }
                else
                {
                    scenario.IsSuccess = false;
                    scenario.ErrorMessage = result.Message;
                    _log.Warning("🏥 MEDICAL: سناریو 5 ناموفق - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                testResults.TestScenarios.Add(scenario);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اجرای سناریو 5. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
            }
        }

        #region Helper Methods

        private async Task<Patient> GetTestPatientWithBothInsurances()
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.PatientInsurances.Any(pi => pi.IsPrimary && !pi.IsDeleted));
        }

        private async Task<Patient> GetTestPatientWithPrimaryInsuranceOnly()
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.PatientInsurances.Any(pi => pi.IsPrimary && !pi.IsDeleted));
        }

        private async Task<Patient> GetTestPatientWithSupplementaryInsuranceOnly()
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.PatientInsurances.Any(pi => !pi.IsPrimary && !pi.IsDeleted));
        }

        private async Task<Patient> GetTestPatientWithFreeAndSupplementaryInsurance()
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.PatientInsurances.Any(pi => !pi.IsDeleted));
        }

        private async Task<ClinicApp.Models.Entities.Clinic.Service> GetTestService()
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => !s.IsDeleted && s.IsActive);
        }

        private decimal CalculateSuccessRate(List<TestScenario> scenarios)
        {
            if (!scenarios.Any()) return 0;
            
            var successfulScenarios = scenarios.Count(s => s.IsSuccess);
            return (decimal)successfulScenarios / scenarios.Count * 100;
        }

        #endregion
    }

    /// <summary>
    /// نتایج تست‌های محاسبه بیمه ترکیبی
    /// </summary>
    public class TestResults
    {
        public DateTime TestStartTime { get; set; }
        public DateTime TestEndTime { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public decimal SuccessRate { get; set; }
        public List<TestScenario> TestScenarios { get; set; } = new List<TestScenario>();
    }

    /// <summary>
    /// سناریو تست محاسبه بیمه ترکیبی
    /// </summary>
    public class TestScenario
    {
        public string ScenarioName { get; set; }
        public decimal ExpectedPatientShare { get; set; }
        public decimal ExpectedInsuranceCoverage { get; set; }
        public decimal ActualPatientShare { get; set; }
        public decimal ActualInsuranceCoverage { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}
