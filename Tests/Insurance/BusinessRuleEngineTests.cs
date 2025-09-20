using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using ClinicApp.Services.Insurance;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Core;
using Serilog;

namespace ClinicApp.Tests.Insurance
{
    /// <summary>
    /// تست‌های Business Rule Engine
    /// </summary>
    public class BusinessRuleEngineTests
    {
        private readonly Mock<IBusinessRuleRepository> _mockRepository;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly BusinessRuleEngine _businessRuleEngine;

        public BusinessRuleEngineTests()
        {
            _mockRepository = new Mock<IBusinessRuleRepository>();
            _mockLogger = new Mock<ILogger>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _businessRuleEngine = new BusinessRuleEngine(_mockRepository.Object, _mockLogger.Object, _mockCurrentUserService.Object);
        }

        [Fact]
        public async Task CalculateCoveragePercentAsync_WithValidRules_ShouldReturnCorrectPercentage()
        {
            // Arrange
            var context = CreateTestContext();
            var rules = new List<BusinessRule>
            {
                new BusinessRule
                {
                    RuleName = "پوشش 70%",
                    RuleType = BusinessRuleType.CoveragePercent,
                    Priority = 1,
                    Conditions = "{}",
                    Actions = "{\"set_coverage_percent\": 70}",
                    IsActive = true
                }
            };

            _mockRepository.Setup(r => r.GetActiveRulesByTypeAsync(
                BusinessRuleType.CoveragePercent, 
                It.IsAny<int?>(), 
                It.IsAny<int?>()))
                .ReturnsAsync(rules);

            // Act
            var result = await _businessRuleEngine.CalculateCoveragePercentAsync(context);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(70, result.Data);
        }

        [Fact]
        public async Task CalculateCoveragePercentAsync_WithNoRules_ShouldReturnDefaultPercentage()
        {
            // Arrange
            var context = CreateTestContext();
            context.InsurancePlan.CoveragePercent = 80;

            _mockRepository.Setup(r => r.GetActiveRulesByTypeAsync(
                BusinessRuleType.CoveragePercent, 
                It.IsAny<int?>(), 
                It.IsAny<int?>()))
                .ReturnsAsync(new List<BusinessRule>());

            // Act
            var result = await _businessRuleEngine.CalculateCoveragePercentAsync(context);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(80, result.Data);
        }

        [Fact]
        public async Task ValidatePaymentLimitsAsync_WithExceededLimit_ShouldReturnFalse()
        {
            // Arrange
            var context = CreateTestContext();
            context.ServiceAmount = 15000000; // 15 میلیون

            var rules = new List<BusinessRule>
            {
                new BusinessRule
                {
                    RuleName = "سقف 10 میلیون",
                    RuleType = BusinessRuleType.PaymentLimit,
                    Priority = 1,
                    Conditions = "{}",
                    Actions = "{\"validate_payment_limit\": 10000000}",
                    IsActive = true
                }
            };

            _mockRepository.Setup(r => r.GetActiveRulesByTypeAsync(
                BusinessRuleType.PaymentLimit, 
                It.IsAny<int?>(), 
                It.IsAny<int?>()))
                .ReturnsAsync(rules);

            // Act
            var result = await _businessRuleEngine.ValidatePaymentLimitsAsync(context);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("سقف پرداخت تجاوز شده است", result.Message);
        }

        [Fact]
        public async Task ApplySupplementaryRulesAsync_WithValidRules_ShouldReturnCorrectRule()
        {
            // Arrange
            var context = CreateTestContext();
            var rules = new List<BusinessRule>
            {
                new BusinessRule
                {
                    RuleName = "بیمه تکمیلی 100%",
                    RuleType = BusinessRuleType.SupplementaryInsurance,
                    Priority = 1,
                    Conditions = "{}",
                    Actions = "{\"set_coverage_percent\": 100, \"set_max_payment\": 5000000, \"set_supplementary_applicable\": true}",
                    IsActive = true
                }
            };

            _mockRepository.Setup(r => r.GetActiveRulesByTypeAsync(
                BusinessRuleType.SupplementaryInsurance, 
                It.IsAny<int?>(), 
                It.IsAny<int?>()))
                .ReturnsAsync(rules);

            // Act
            var result = await _businessRuleEngine.ApplySupplementaryRulesAsync(context);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(100, result.Data.CoveragePercent);
            Assert.Equal(5000000, result.Data.MaxPayment);
            Assert.True(result.Data.IsApplicable);
        }

        private InsuranceCalculationContext CreateTestContext()
        {
            return new InsuranceCalculationContext
            {
                PatientId = 1,
                ServiceId = 1,
                InsurancePlanId = 1,
                ServiceAmount = 1000000,
                CalculationDate = DateTime.Now,
                InsurancePlan = new InsurancePlan
                {
                    InsurancePlanId = 1,
                    Name = "بیمه پایه",
                    CoveragePercent = 70,
                    Deductible = 50000,
                    IsActive = true
                },
                Patient = new Patient
                {
                    PatientId = 1,
                    FirstName = "علی",
                    LastName = "احمدی",
                    BirthDate = DateTime.Now.AddYears(-30),
                    Gender = Gender.Male
                },
                Service = new Service
                {
                    ServiceId = 1,
                    Title = "معاینه عمومی",
                    ServiceCategoryId = 1
                }
            };
        }
    }
}
