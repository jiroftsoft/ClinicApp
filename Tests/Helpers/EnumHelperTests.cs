using System;
using System.Linq;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClinicApp.Tests.Helpers
{
    /// <summary>
    /// ✅ Unit Tests for EnumHelper
    /// طبق قرارداد: DEVELOPMENT_CONTRACT.md - "Every change MUST include tests"
    /// </summary>
    [TestClass]
    public class EnumHelperTests
    {
        [TestMethod]
        public void GetSelectList_ShouldReturnAllGenderValues()
        {
            // Arrange & Act
            var result = EnumHelper.GetSelectList<Gender>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count()); // Unknown, Male, Female
        }

        [TestMethod]
        public void GetSelectList_ShouldUseDisplayAttributes()
        {
            // Arrange & Act
            var result = EnumHelper.GetSelectList<Gender>();
            var items = result.ToList();

            // Assert - Check Display attribute values
            var maleItem = items.First(x => x.Value == "1");
            Assert.AreEqual("مرد", maleItem.Text);

            var femaleItem = items.First(x => x.Value == "2");
            Assert.AreEqual("زن", femaleItem.Text);
        }

        [TestMethod]
        public void GetSelectList_ShouldPreSelectValue()
        {
            // Arrange
            var selectedGender = Gender.Female;

            // Act
            var result = EnumHelper.GetSelectList<Gender>(selectedGender);

            // Assert
            var selectedItem = result.FirstOrDefault(x => x.Selected);
            Assert.IsNotNull(selectedItem);
            Assert.AreEqual("2", selectedItem.Value);
        }

        [TestMethod]
        public void GetDisplayName_ShouldReturnDisplayAttributeName()
        {
            // Arrange
            var gender = Gender.Male;

            // Act
            var result = EnumHelper.GetDisplayName(gender);

            // Assert
            Assert.AreEqual("مرد", result);
        }

        [TestMethod]
        public void GetDisplayName_ShouldReturnEnumNameIfNoDisplayAttribute()
        {
            // Arrange
            // Create a test enum without Display attribute
            var testEnum = TestEnum.Value1;

            // Act
            var result = EnumHelper.GetDisplayName(testEnum);

            // Assert
            Assert.AreEqual("Value1", result);
        }

        [TestMethod]
        public void GetSelectListItems_ShouldReturnCorrectFormat()
        {
            // Act
            var result = EnumHelper.GetSelectListItems<Gender>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            
            // Verify structure
            foreach (var item in result)
            {
                Assert.IsNotNull(item.Text);
                Assert.IsNotNull(item.Value);
                Assert.IsTrue(int.TryParse(item.Value, out _)); // Value should be numeric string
            }
        }

        [TestMethod]
        public void GetDisplayNames_ShouldReturnDictionary()
        {
            // Act
            var result = EnumHelper.GetDisplayNames<Gender>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.ContainsKey(Gender.Male));
            Assert.AreEqual("مرد", result[Gender.Male]);
        }

        // Test enum for testing fallback behavior
        private enum TestEnum
        {
            Value1,
            Value2
        }
    }
}

