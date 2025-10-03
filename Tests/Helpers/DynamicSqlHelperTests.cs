using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Tests.Helpers
{
    /// <summary>
    /// Unit Tests برای DynamicSqlHelper
    /// </summary>
    [TestClass]
    public class DynamicSqlHelperTests
    {
        [TestMethod]
        public void GenerateSelectQuery_WithValidModel_ShouldReturnValidSql()
        {
            // Arrange
            var tableName = "InsuranceProviders";
            var whereClause = "IsActive = 1";
            var orderByClause = "Name ASC";

            // Act
            var result = DynamicSqlHelper.GenerateSelectQuery<InsuranceProvider>(
                tableName, whereClause, orderByClause);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("SELECT"));
            Assert.IsTrue(result.Contains("FROM InsuranceProviders"));
            Assert.IsTrue(result.Contains("WHERE IsActive = 1"));
            Assert.IsTrue(result.Contains("ORDER BY Name ASC"));
        }

        [TestMethod]
        public void GenerateSelectQuery_WithoutWhereClause_ShouldReturnValidSql()
        {
            // Arrange
            var tableName = "InsuranceProviders";

            // Act
            var result = DynamicSqlHelper.GenerateSelectQuery<InsuranceProvider>(tableName);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("SELECT"));
            Assert.IsTrue(result.Contains("FROM InsuranceProviders"));
            Assert.IsFalse(result.Contains("WHERE"));
        }

        [TestMethod]
        public void GenerateSelectQuery_WithoutOrderByClause_ShouldReturnValidSql()
        {
            // Arrange
            var tableName = "InsuranceProviders";
            var whereClause = "IsActive = 1";

            // Act
            var result = DynamicSqlHelper.GenerateSelectQuery<InsuranceProvider>(
                tableName, whereClause);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("SELECT"));
            Assert.IsTrue(result.Contains("FROM InsuranceProviders"));
            Assert.IsTrue(result.Contains("WHERE IsActive = 1"));
            Assert.IsFalse(result.Contains("ORDER BY"));
        }

        [TestMethod]
        public void GetActiveFilterWhereClause_ShouldReturnCorrectFilter()
        {
            // Act
            var result = DynamicSqlHelper.GetActiveFilterWhereClause();

            // Assert
            Assert.AreEqual("IsActive = 1 AND IsDeleted = 0", result);
        }

        [TestMethod]
        public void GetNotDeletedFilterWhereClause_ShouldReturnCorrectFilter()
        {
            // Act
            var result = DynamicSqlHelper.GetNotDeletedFilterWhereClause();

            // Assert
            Assert.AreEqual("IsDeleted = 0", result);
        }

        [TestMethod]
        public void GetNameOrderByClause_WithDefaultColumn_ShouldReturnCorrectOrderBy()
        {
            // Act
            var result = DynamicSqlHelper.GetNameOrderByClause();

            // Assert
            Assert.AreEqual("Name ASC", result);
        }

        [TestMethod]
        public void GetNameOrderByClause_WithCustomColumn_ShouldReturnCorrectOrderBy()
        {
            // Arrange
            var customColumn = "CreatedAt";

            // Act
            var result = DynamicSqlHelper.GetNameOrderByClause(customColumn);

            // Assert
            Assert.AreEqual("CreatedAt ASC", result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateSelectQuery_WithEmptyTableName_ShouldThrowException()
        {
            // Act
            DynamicSqlHelper.GenerateSelectQuery<InsuranceProvider>("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateSelectQuery_WithNullTableName_ShouldThrowException()
        {
            // Act
            DynamicSqlHelper.GenerateSelectQuery<InsuranceProvider>(null);
        }
    }
}
