using Kantar.StudyDesignerLite.Plugins.Common;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Common
{
    public partial class QuestionAnswerSortOrderAssignmentPostOperationTests
    {
        [TestMethod]
        public void ProductConfigQuestion_OnDeactivate_Shifts_Lower_Siblings_Up()
        {
            // Arrange
            var product = new ProductBuilder()
                    .WithName("Test Product")
                    .Build();

            var configQuestion1 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 1")
                    .Build();
            var q1 = new ProductConfigQuestionBuilder(product, configQuestion1)
                    .WithSortOrder(0)
                    .Build();

            var configQuestion2 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 2")
                    .Build();
            var q2 = new ProductConfigQuestionBuilder(product, configQuestion2)
                    .WithSortOrder(1)
                    .Build();

            var configQuestion3 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 3")
                    .Build();
            var q3 = new ProductConfigQuestionBuilder(product, configQuestion3)
                    .WithSortOrder(2)
                    .Build();

            var target = q2;
            target["statecode"] = new OptionSetValue(1); // Deactivating q2

            _context.Initialize(new List<Entity> { product, configQuestion1, configQuestion2, configQuestion3, q1, q2, q3 });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = target;
            pluginContext.PreEntityImages["PreImage"] = q2;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPostOperation>(pluginContext);

            // Assert: q1 stays, q2 deactivated, q3 shifts from 2 to 1
            var updated = _context.CreateQuery(KTR_ProductConfigQuestion.EntityLogicalName).ToList();
            Assert.AreEqual(0, (int)updated.First(e => e.Id == q1.Id)[KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder]);
            Assert.AreEqual(1, (int)updated.First(e => e.Id == q3.Id)[KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder]);
        }

        [TestMethod]
        public void ProductConfigQuestion_OnReactivation_Appends_To_End()
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

            var qId = Guid.NewGuid();

            var configQuestion3 = new ConfigurationQuestionBuilder()
                .WithName("Test Config Question 3")
                .Build();
            var reactivated = new ProductConfigQuestionBuilder(product, configQuestion3)
                .WithId(qId)
                .Build();

            var preImage = new ProductConfigQuestionBuilder(product, configQuestion3)
                .WithId(qId)
                .WithState(KTR_ProductConfigQuestion_StateCode.Inactive)
                .Build();

            _context.Initialize(new List<Entity> { product, configQuestion1, configQuestion2, configQuestion3, existing1, existing2, reactivated });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = reactivated;
            pluginContext.PreEntityImages["PreImage"] = preImage;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery(KTR_ProductConfigQuestion.EntityLogicalName).ToList();
            var updatedReact = updated.First(e => e.Id == reactivated.Id);

            Assert.AreEqual(2, updatedReact[KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder]);
        }

        [TestMethod]
        public void ProductConfigQuestion_Skips_When_Parent_Not_Set()
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
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = newProductConfigQuestion;

            _context.Initialize(new List<Entity> { configQuestion1, newProductConfigQuestion });

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPostOperation>(pluginContext);

            // Assert
            Assert.IsFalse(newProductConfigQuestion.Attributes.Contains(KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder));
        }

        [TestMethod]
        public void ProductConfigQuestion_OnDeactivate_ShiftsLowerSiblingsUp()
        {
            // Arrange
            var product = new ProductBuilder()
                    .WithName("Test Product")
                    .Build();

            var configQuestion1 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 1")
                    .Build();
            var q1 = new ProductConfigQuestionBuilder(product, configQuestion1)
                    .WithSortOrder(0)
                    .Build();

            var configQuestion2 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 2")
                    .Build();
            var q2 = new ProductConfigQuestionBuilder(product, configQuestion2)
                    .WithSortOrder(1)
                    .Build();

            var configQuestion3 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 3")
                    .Build();
            var q3 = new ProductConfigQuestionBuilder(product, configQuestion3)
                    .WithSortOrder(2)
                    .Build();

            var configQuestion4 = new ConfigurationQuestionBuilder()
                    .WithName("Test Config Question 4")
                    .Build();
            var q4 = new ProductConfigQuestionBuilder(product, configQuestion4)
                    .WithSortOrder(5)
                    .Build();

            var target = q2;
            target["statecode"] = new OptionSetValue(1); // Deactivating q2

            _context.Initialize(new List<Entity> { product, configQuestion1, configQuestion2, configQuestion3,
                configQuestion4, q1, q2, q3, q4 });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = target;
            pluginContext.PreEntityImages["PreImage"] = q2;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPostOperation>(pluginContext);

            // Assert: q1 stays, q2 deactivated, q3 shifts from 2 to 1
            var updated = _context.CreateQuery(KTR_ProductConfigQuestion.EntityLogicalName).ToList();
            Assert.AreEqual(0, (int)updated.First(e => e.Id == q1.Id)[KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder]);
            Assert.AreEqual(1, (int)updated.First(e => e.Id == q3.Id)[KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder]);
            Assert.AreEqual(2, (int)updated.First(e => e.Id == q4.Id)[KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder]);
        }
    }
}
