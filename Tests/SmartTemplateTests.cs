using System;
using System.Collections.Generic;
using ClinicApp.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClinicApp.Tests
{
    /// <summary>
    /// Test Cases برای Template هوشمند
    /// تست‌های حیاتی برای Production
    /// </summary>
    [TestClass]
    public class SmartTemplateTests
    {
        private readonly SmartTemplateService _service = new SmartTemplateService();

        #region Test Case 1: Nested If

        [TestMethod]
        public void TestNestedIf()
        {
            // Arrange
            var template = @"{{#if A}}
  {{#if B}} X {{/if}}
{{/if}}";

            var variables = new Dictionary<string, object>
            {
                { "A", true },
                { "B", true }
            };

            // Act
            var result = _service.Render(template, variables);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Output.Contains("X"));
        }

        [TestMethod]
        public void TestNestedIf_OuterFalse()
        {
            // Arrange
            var template = @"{{#if A}}
  {{#if B}} X {{/if}}
{{/if}}";

            var variables = new Dictionary<string, object>
            {
                { "A", false },
                { "B", true }
            };

            // Act
            var result = _service.Render(template, variables);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Output.Contains("X"));
        }

        [TestMethod]
        public void TestNestedIf_InnerFalse()
        {
            // Arrange
            var template = @"{{#if A}}
  {{#if B}} X {{/if}}
{{/if}}";

            var variables = new Dictionary<string, object>
            {
                { "A", true },
                { "B", false }
            };

            // Act
            var result = _service.Render(template, variables);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Output.Contains("X"));
        }

        #endregion

        #region Test Case 2: For + If

        [TestMethod]
        public void TestForWithIf()
        {
            // Arrange
            var template = @"{{#for Items}}
  {{#if IsActive}}
    {{ItemName}}
  {{/if}}
{{/for}}";

            var items = new List<TestItem>
            {
                new TestItem { ItemName = "Item1", IsActive = true },
                new TestItem { ItemName = "Item2", IsActive = false },
                new TestItem { ItemName = "Item3", IsActive = true }
            };

            var variables = new Dictionary<string, object>
            {
                { "Items", items }
            };

            // Act
            var result = _service.Render(template, variables);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Output.Contains("Item1"));
            Assert.IsFalse(result.Output.Contains("Item2"));
            Assert.IsTrue(result.Output.Contains("Item3"));
        }

        #endregion

        #region Test Case 3: Missing Variable

        [TestMethod]
        public void TestMissingVariable_ShouldReturnEmpty()
        {
            // Arrange
            var template = @"Hello {{UnknownVar}} World";

            var variables = new Dictionary<string, object>
            {
                { "FullName", "Test" }
            };

            // Act
            var result = _service.Render(template, variables);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.HasErrors);
            Assert.IsTrue(result.Output.Contains("Hello"));
            Assert.IsTrue(result.Output.Contains("World"));
            // {{UnknownVar}} باید به Empty تبدیل شود
            Assert.IsFalse(result.Output.Contains("UnknownVar"));
        }

        [TestMethod]
        public void TestMissingVariable_ShouldNotThrowException()
        {
            // Arrange
            var template = @"{{UnknownVar1}} {{UnknownVar2}} {{UnknownVar3}}";

            var variables = new Dictionary<string, object>();

            // Act & Assert - نباید Exception بدهد
            var result = _service.Render(template, variables);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.HasErrors);
        }

        #endregion

        #region Security Tests

        [TestMethod]
        public void TestInfiniteLoopProtection()
        {
            // Arrange
            var template = @"{{#for Items}}{{/for}}";

            // ایجاد یک Collection با بیش از 100 آیتم
            var items = new List<string>();
            for (int i = 0; i < 150; i++)
            {
                items.Add($"Item{i}");
            }

            var variables = new Dictionary<string, object>
            {
                { "Items", items }
            };

            // Act
            var result = _service.Render(template, variables);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.Errors.Any(e => e.Code == "SECURITY_LOOP_LIMIT"));
        }

        [TestMethod]
        public void TestHtmlEncode_Variables()
        {
            // Arrange
            var template = @"<p>{{FullName}}</p>";

            var variables = new Dictionary<string, object>
            {
                { "FullName", "<script>alert('XSS')</script>" }
            };

            // Act
            var result = _service.Render(template, variables);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            // متغیر باید Encode شود
            Assert.IsTrue(result.Output.Contains("&lt;script&gt;"));
            // اما HTML خود Template نباید Encode شود
            Assert.IsTrue(result.Output.Contains("<p>"));
            Assert.IsTrue(result.Output.Contains("</p>"));
        }

        [TestMethod]
        public void TestHtmlEncode_UnsubscribeUrl_ShouldNotEncode()
        {
            // Arrange
            var template = @"<a href=""{{UnsubscribeUrl}}"">لغو اشتراک</a>";

            var variables = new Dictionary<string, object>
            {
                { "UnsubscribeUrl", "https://example.com/unsubscribe?token=abc123" }
            };

            // Act
            var result = _service.Render(template, variables);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            // UnsubscribeUrl نباید Encode شود (چون URL است)
            Assert.IsTrue(result.Output.Contains("https://example.com/unsubscribe?token=abc123"));
        }

        #endregion

        #region Performance Tests

        [TestMethod]
        public void TestCache_ShouldUseCachedAST()
        {
            // Arrange
            var template = @"Hello {{FullName}}";
            var variables1 = new Dictionary<string, object> { { "FullName", "User1" } };
            var variables2 = new Dictionary<string, object> { "FullName", "User2" } };

            // Act - Render اول (Cache می‌شود)
            var result1 = _service.Render(template, variables1, "test-template");

            // Render دوم (باید از Cache استفاده کند)
            var result2 = _service.Render(template, variables2, "test-template");

            // Assert
            Assert.IsTrue(result1.IsSuccess);
            Assert.IsTrue(result2.IsSuccess);
            Assert.IsTrue(result1.Output.Contains("User1"));
            Assert.IsTrue(result2.Output.Contains("User2"));
        }

        #endregion

        #region Helper Classes

        private class TestItem
        {
            public string ItemName { get; set; }
            public bool IsActive { get; set; }
        }

        #endregion
    }
}

