using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Services.Insurance;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using Xunit;
using Moq;
using Serilog;

namespace ClinicApp.Tests.Insurance
{
    /// <summary>
    /// تست‌های محاسبه خدمات - طبق مصوبه 1404
    /// 
    /// Test Cases:
    /// 1. Hashed vs Non-hashed services
    /// 2. FactorSetting 1404 compliance
    /// 3. Rounding rules (IRR precision)
    /// 4. Rule Engine exceptions
    /// 5. Base vs Supplementary insurance
    /// 6. Edge cases and limits
    /// </summary>
    public class ServiceCalculationTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<ILogger> _mockLogger;
        private readonly ServiceCalculationEngine _calculationEngine;
        private readonly ComputeTotalsService _computeTotalsService;

        public ServiceCalculationTests()
        {
            _mockContext = new Mock<ApplicationDbContext>();
            _mockLogger = new Mock<ILogger>();
            _calculationEngine = new ServiceCalculationEngine(_mockContext.Object, _mockLogger.Object);
            _computeTotalsService = new ComputeTotalsService(_mockLogger.Object);
        }

        #region UnitPriceIRR Calculation Tests

        [Fact]
        public async Task CalculateUnitPriceIRR_HashedService_ShouldApplyCorrectFactors()
        {
            // Arrange
            var serviceId = 1;
            var financialYear = 1404;
            var service = CreateTestService(serviceId, isHashtagged: true, basePrice: 1000000m);
            var factors = CreateTestFactors(technicalFactor: 1.5m, professionalFactor: 2.0m);

            SetupMockContext(service, factors);

            // Act
            var result = await _calculationEngine.CalculateUnitPriceIRRAsync(serviceId, financialYear);

            // Assert
            // Expected: 1,000,000 × 1.5 × 2.0 = 3,000,000
            Assert.Equal(3000000m, result);
        }

        [Fact]
        public async Task CalculateUnitPriceIRR_NonHashedService_ShouldApplyCorrectFactors()
        {
            // Arrange
            var serviceId = 2;
            var financialYear = 1404;
            var service = CreateTestService(serviceId, isHashtagged: false, basePrice: 500000m);
            var factors = CreateTestFactors(technicalFactor: 1.2m, professionalFactor: 1.8m);

            SetupMockContext(service, factors);

            // Act
            var result = await _calculationEngine.CalculateUnitPriceIRRAsync(serviceId, financialYear);

            // Assert
            // Expected: 500,000 × 1.2 × 1.8 = 1,080,000
            Assert.Equal(1080000m, result);
        }

        [Fact]
        public async Task CalculateUnitPriceIRR_WithServiceComponents_ShouldCalculateCorrectly()
        {
            // Arrange
            var serviceId = 3;
            var financialYear = 1404;
            var service = CreateTestServiceWithComponents(serviceId, basePrice: 1000000m);
            var factors = CreateTestFactors(technicalFactor: 1.0m, professionalFactor: 1.0m);

            SetupMockContext(service, factors);

            // Act
            var result = await _calculationEngine.CalculateUnitPriceIRRAsync(serviceId, financialYear);

            // Assert
            // Expected: (2.0 × 1.0) + (3.0 × 1.0) = 5.0
            Assert.Equal(5000000m, result);
        }

        [Fact]
        public async Task CalculateUnitPriceIRR_ShouldRoundToIRR()
        {
            // Arrange
            var serviceId = 4;
            var financialYear = 1404;
            var service = CreateTestService(serviceId, isHashtagged: false, basePrice: 1000000m);
            var factors = CreateTestFactors(technicalFactor: 1.333m, professionalFactor: 1.666m);

            SetupMockContext(service, factors);

            // Act
            var result = await _calculationEngine.CalculateUnitPriceIRRAsync(serviceId, financialYear);

            // Assert
            // Expected: 1,000,000 × 1.333 × 1.666 = 2,220,778 → 2,220,778 (rounded)
            Assert.Equal(2220778m, result);
        }

        #endregion

        #region ComputeTotals Tests

        [Fact]
        public async Task ComputeBaseInsuranceTotals_ShouldCalculateCorrectly()
        {
            // Arrange
            var services = new List<ServiceCalculationItem>
            {
                new ServiceCalculationItem { ServiceId = 1, UnitPriceIRR = 1000000m, Quantity = 2 },
                new ServiceCalculationItem { ServiceId = 2, UnitPriceIRR = 500000m, Quantity = 1 }
            };

            var primaryInsurance = CreateTestInsurance(coveragePercentage: 70m);

            // Act
            var result = await _computeTotalsService.ComputeBaseInsuranceTotalsAsync(services, primaryInsurance);

            // Assert
            // Gross: (1,000,000 × 2) + (500,000 × 1) = 2,500,000
            // Insurer Share: 2,500,000 × 70% = 1,750,000
            // Patient Share: 2,500,000 - 1,750,000 = 750,000
            Assert.Equal(2500000m, result.GrossAmount);
            Assert.Equal(1750000m, result.InsurerShareAmount);
            Assert.Equal(750000m, result.PatientShareAmount);
        }

        [Fact]
        public async Task ComputeSupplementaryInsuranceTotals_ShouldCalculateCorrectly()
        {
            // Arrange
            var baseTotals = new BaseInsuranceTotals
            {
                GrossAmount = 2500000m,
                InsurerShareAmount = 1750000m,
                PatientShareAmount = 750000m
            };

            var supplementaryInsurance = CreateTestInsurance(coveragePercentage: 50m);

            // Act
            var result = await _computeTotalsService.ComputeSupplementaryInsuranceTotalsAsync(baseTotals, supplementaryInsurance);

            // Assert
            // Supplementary Share: 750,000 × 50% = 375,000
            // Final Patient Share: 750,000 - 375,000 = 375,000
            Assert.Equal(375000m, result.InsurerShareAmount);
            Assert.Equal(375000m, result.PatientShareAmount);
        }

        [Fact]
        public async Task ComputeFinalTotals_ShouldCalculateCorrectly()
        {
            // Arrange
            var baseTotals = new BaseInsuranceTotals
            {
                GrossAmount = 2500000m,
                InsurerShareAmount = 1750000m,
                PatientShareAmount = 750000m
            };

            var supplementaryTotals = new SupplementaryInsuranceTotals
            {
                InsurerShareAmount = 375000m,
                PatientShareAmount = 375000m
            };

            var discounts = new List<DiscountItem>
            {
                new DiscountItem { Amount = 50000m }
            };

            // Act
            var result = await _computeTotalsService.ComputeFinalTotalsAsync(baseTotals, supplementaryTotals, discounts);

            // Assert
            // Total Insurer Share: 1,750,000 + 375,000 = 2,125,000
            // Patient Share: 375,000 - 50,000 = 325,000
            // Total Amount: 2,125,000 + 325,000 = 2,450,000
            Assert.Equal(2500000m, result.GrossAmount);
            Assert.Equal(1750000m, result.BaseInsurerShare);
            Assert.Equal(375000m, result.SupplementaryInsurerShare);
            Assert.Equal(2125000m, result.TotalInsurerShare);
            Assert.Equal(325000m, result.PatientShare);
            Assert.Equal(50000m, result.DiscountAmount);
            Assert.Equal(2450000m, result.TotalAmount);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public async Task CalculateUnitPriceIRR_ServiceNotFound_ShouldReturnZero()
        {
            // Arrange
            var serviceId = 999;
            var financialYear = 1404;

            SetupMockContext(null, null);

            // Act
            var result = await _calculationEngine.CalculateUnitPriceIRRAsync(serviceId, financialYear);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task CalculateUnitPriceIRR_FactorsNotFound_ShouldReturnBasePrice()
        {
            // Arrange
            var serviceId = 1;
            var financialYear = 1404;
            var service = CreateTestService(serviceId, isHashtagged: false, basePrice: 1000000m);

            SetupMockContext(service, null);

            // Act
            var result = await _calculationEngine.CalculateUnitPriceIRRAsync(serviceId, financialYear);

            // Assert
            Assert.Equal(1000000m, result);
        }

        [Fact]
        public async Task CalculateUnitPriceIRR_ShouldApplyMinimumLimit()
        {
            // Arrange
            var serviceId = 1;
            var financialYear = 1404;
            var service = CreateTestService(serviceId, isHashtagged: false, basePrice: 100m);
            var factors = CreateTestFactors(technicalFactor: 0.1m, professionalFactor: 0.1m);

            SetupMockContext(service, factors);

            // Act
            var result = await _calculationEngine.CalculateUnitPriceIRRAsync(serviceId, financialYear);

            // Assert
            // Expected: 100 × 0.1 × 0.1 = 1, but minimum is 1000
            Assert.Equal(1000m, result);
        }

        [Fact]
        public async Task CalculateUnitPriceIRR_ShouldApplyMaximumLimit()
        {
            // Arrange
            var serviceId = 1;
            var financialYear = 1404;
            var service = CreateTestService(serviceId, isHashtagged: false, basePrice = 100000000m);
            var factors = CreateTestFactors(technicalFactor: 2.0m, professionalFactor = 2.0m);

            SetupMockContext(service, factors);

            // Act
            var result = await _calculationEngine.CalculateUnitPriceIRRAsync(serviceId, financialYear);

            // Assert
            // Expected: 100,000,000 × 2.0 × 2.0 = 400,000,000, but maximum is 100,000,000
            Assert.Equal(100000000m, result);
        }

        #endregion

        #region Test Data Helpers

        private Service CreateTestService(int serviceId, bool isHashtagged, decimal basePrice)
        {
            return new Service
            {
                ServiceId = serviceId,
                ServiceCode = $"SVC{serviceId:D3}",
                ServiceName = $"Test Service {serviceId}",
                Price = basePrice,
                IsHashtagged = isHashtagged,
                IsActive = true,
                IsDeleted = false
            };
        }

        private Service CreateTestServiceWithComponents(int serviceId, decimal basePrice)
        {
            var service = CreateTestService(serviceId, isHashtagged: false, basePrice);
            service.ServiceComponents = new List<ServiceComponent>
            {
                new ServiceComponent
                {
                    ComponentType = ServiceComponentType.Technical,
                    Coefficient = 2.0m
                },
                new ServiceComponent
                {
                    ComponentType = ServiceComponentType.Professional,
                    Coefficient = 3.0m
                }
            };
            return service;
        }

        private FactorPair CreateTestFactors(decimal technicalFactor, decimal professionalFactor)
        {
            return new FactorPair
            {
                TechnicalFactor = technicalFactor,
                ProfessionalFactor = professionalFactor,
                TechnicalFactorId = 1,
                ProfessionalFactorId = 2
            };
        }

        private PatientInsurance CreateTestInsurance(decimal coveragePercentage)
        {
            return new PatientInsurance
            {
                PatientInsuranceId = 1,
                InsurancePlan = new InsurancePlan
                {
                    CoveragePercentage = coveragePercentage,
                    FranchiseAmount = 100000m,
                    PaymentCeiling = 5000000m
                }
            };
        }

        private void SetupMockContext(Service service, FactorPair factors)
        {
            // TODO: Setup mock context with test data
            // This would require setting up the DbContext mocks properly
        }

        #endregion
    }
}
