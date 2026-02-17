using Kantar.StudyDesignerLite.Plugins.Common;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Common
{
    public partial class QuestionAnswerSortOrderAssignmentPreOperationTests
    {
        [TestMethod]
        public void ProductConfigQuestion_Assigns_MaxPlusOne_SortOrder()
        {
            // Arrange
            var product = new ProductBuilder()
                    .WithName("Test Product")
                    .Build();

            var configQuestion1 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 1")
                    .Build();
            var existing1 = new ProductConfigQuestionBuilder(product, configQuestion1)
                .WithSortOrder(0)
                .Build();

            var configQuestion2 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 2")
                    .Build();
            var existing2 = new ProductConfigQuestionBuilder(product, configQuestion2)
                .WithSortOrder(1)
                .Build();

            var configQuestion3 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 3")
                    .Build();
            var newLine = new ProductConfigQuestionBuilder(product, configQuestion3)
                .Build();

            _context.Initialize(new List<Entity> { configQuestion1, configQuestion2, configQuestion3, existing1, existing2 });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newLine;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPreOperation>(pluginContext);

            // Assert
            Assert.AreEqual(2, newLine.KTR_DisplayOrder);
        }

        [TestMethod]
        public void ProductConfigQuestion_FirstEntry_GetsSortOrderZero()
        {
            // Arrange
            var product = new ProductBuilder()
                    .WithName("Test Product")
                    .Build();

            var configQuestion1 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 1")
                    .Build();
            var newLine = new ProductConfigQuestionBuilder(product, configQuestion1)
                .Build();

            _context.Initialize(new List<Entity>()); // no siblings

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newLine;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPreOperation>(pluginContext);

            // Assert
            Assert.AreEqual(0, newLine.KTR_DisplayOrder);
        }

        [TestMethod]
        public void ProductConfigQuestion_Skips_When_Depth_Greater_Than_One()
        {
            // Arrange
            var product = new ProductBuilder()
                    .WithName("Test Product")
                    .Build();

            var configQuestion1 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 1")
                    .Build();
            var newProductConfigQuestion = new ProductConfigQuestionBuilder(product, configQuestion1).Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.Depth = 2; // triggers depth check
            pluginContext.InputParameters["Target"] = newProductConfigQuestion;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPreOperation>(pluginContext);

            // Assert
            Assert.IsFalse(newProductConfigQuestion.Attributes.Contains(KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder));
        }

        [TestMethod]
        public void ProductConfigQuestion_Skips_When_SortOrder_Is_Already_Set()
        {
            // Arrange
            var expectedSortOrder = 3;
            var product = new ProductBuilder()
                    .WithName("Test Product")
                    .Build();

            var configQuestion1 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 1")
                    .Build();

            var productConfigQuestion = new ProductConfigQuestionBuilder(product, configQuestion1)
                .WithSortOrder(expectedSortOrder)
                .Build();

            _context.Initialize(new List<Entity> { product, configQuestion1, productConfigQuestion });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = productConfigQuestion;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPreOperation>(pluginContext);

            // Assert
            Assert.AreEqual(expectedSortOrder, productConfigQuestion.KTR_DisplayOrder);
        }

    }
}
