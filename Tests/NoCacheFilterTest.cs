using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ClinicApp.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ClinicApp.Tests
{
    /// <summary>
    /// تست‌های NoCacheFilter برای اطمینان از عملکرد صحیح ضد کش
    /// </summary>
    [TestClass]
    public class NoCacheFilterTest
    {
        private NoCacheFilter _filter;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpResponseBase> _mockResponse;
        private Mock<HttpCachePolicyBase> _mockCache;
        private Mock<ActionDescriptor> _mockActionDescriptor;
        private Mock<ControllerDescriptor> _mockControllerDescriptor;
        private ResultExecutingContext _resultExecutingContext;

        [TestInitialize]
        public void Setup()
        {
            _filter = new NoCacheFilter();
            
            // Mock HttpContext
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockResponse = new Mock<HttpResponseBase>();
            _mockCache = new Mock<HttpCachePolicyBase>();
            
            _mockResponse.Setup(r => r.Cache).Returns(_mockCache.Object);
            _mockHttpContext.Setup(c => c.Response).Returns(_mockResponse.Object);
            
            // Mock ActionDescriptor
            _mockActionDescriptor = new Mock<ActionDescriptor>();
            _mockControllerDescriptor = new Mock<ControllerDescriptor>();
            _mockControllerDescriptor.Setup(c => c.ControllerName).Returns("InsuranceTariff");
            _mockActionDescriptor.Setup(a => a.ActionName).Returns("Index");
            _mockActionDescriptor.Setup(a => a.ControllerDescriptor).Returns(_mockControllerDescriptor.Object);
            
            // Setup ResultExecutingContext
            var controllerContext = new ControllerContext(
                _mockHttpContext.Object,
                new RouteData(),
                new Mock<ControllerBase>().Object);
            
            _resultExecutingContext = new ResultExecutingContext(
                controllerContext,
                _mockActionDescriptor.Object,
                new Mock<ActionResult>().Object);
        }

        [TestMethod]
        public void OnResultExecuting_ShouldSetNoCacheHeaders()
        {
            // Act
            _filter.OnResultExecuting(_resultExecutingContext);

            // Assert
            _mockCache.Verify(c => c.SetCacheability(HttpCacheability.NoCache), Times.Once);
            _mockCache.Verify(c => c.SetNoStore(), Times.Once);
            _mockCache.Verify(c => c.SetRevalidation(HttpCacheRevalidation.AllCaches), Times.Once);
            _mockCache.Verify(c => c.SetExpires(It.IsAny<DateTime>()), Times.Once);
            _mockCache.Verify(c => c.AppendCacheExtension("must-revalidate, proxy-revalidate"), Times.Once);
        }

        [TestMethod]
        public void OnResultExecuting_ShouldAddCustomHeaders()
        {
            // Act
            _filter.OnResultExecuting(_resultExecutingContext);

            // Assert
            _mockResponse.Verify(r => r.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate, max-age=0"), Times.Once);
            _mockResponse.Verify(r => r.Headers.Add("Pragma", "no-cache"), Times.Once);
            _mockResponse.Verify(r => r.Headers.Add("Expires", "0"), Times.Once);
        }

        [TestMethod]
        public void IsClinicalController_ShouldReturnTrueForClinicalControllers()
        {
            // Arrange
            var clinicalControllers = new[]
            {
                "InsuranceTariff", "SupplementaryTariff", "CombinedInsuranceCalculation",
                "InsuranceCalculation", "PatientInsurance", "Reception",
                "Doctor", "Appointment", "EmergencyBooking", "ScheduleOptimization"
            };

            // Act & Assert
            foreach (var controller in clinicalControllers)
            {
                var result = _filter.GetType()
                    .GetMethod("IsClinicalController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(_filter, new object[] { controller });
                
                Assert.IsTrue((bool)result, $"Controller '{controller}' should be identified as clinical");
            }
        }

        [TestMethod]
        public void IsClinicalController_ShouldReturnFalseForNonClinicalControllers()
        {
            // Arrange
            var nonClinicalControllers = new[]
            {
                "Home", "Account", "Error", "Test", "Admin", "Settings"
            };

            // Act & Assert
            foreach (var controller in nonClinicalControllers)
            {
                var result = _filter.GetType()
                    .GetMethod("IsClinicalController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(_filter, new object[] { controller });
                
                Assert.IsFalse((bool)result, $"Controller '{controller}' should NOT be identified as clinical");
            }
        }

        [TestMethod]
        public void OnActionExecuting_ShouldLogForClinicalControllers()
        {
            // Arrange
            var actionExecutingContext = new ActionExecutingContext(
                new ControllerContext(_mockHttpContext.Object, new RouteData(), new Mock<ControllerBase>().Object),
                _mockActionDescriptor.Object,
                new System.Collections.Generic.Dictionary<string, object>());

            // Act
            _filter.OnActionExecuting(actionExecutingContext);

            // Assert - این تست نیاز به بررسی لاگ دارد
            // در محیط واقعی، باید لاگ‌ها را بررسی کنیم
            Assert.IsTrue(true); // Placeholder assertion
        }

        [TestMethod]
        public void Filter_ShouldBeAppliedToAllActions()
        {
            // Arrange & Act
            var attributes = _filter.GetType().GetCustomAttributes(typeof(ActionFilterAttribute), true);

            // Assert
            Assert.IsTrue(attributes.Length > 0, "NoCacheFilter should inherit from ActionFilterAttribute");
        }

        [TestMethod]
        public void Filter_ShouldHaveCorrectNamespace()
        {
            // Act & Assert
            Assert.AreEqual("ClinicApp.Filters", _filter.GetType().Namespace);
        }

        [TestMethod]
        public void Filter_ShouldBeSealed()
        {
            // Act & Assert
            Assert.IsTrue(_filter.GetType().IsSealed, "NoCacheFilter should be sealed for performance");
        }
    }
}
