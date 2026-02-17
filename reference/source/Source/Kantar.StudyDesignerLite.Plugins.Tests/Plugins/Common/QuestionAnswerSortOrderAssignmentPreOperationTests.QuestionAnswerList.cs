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
        public void QuestionAnswerLine_Assigns_MaxPlusOne_SortOrder()
        {
            // Arrange
            var question = new QuestionBankBuilder()
                .Build();

            var existing1 = new QuestionAnswerListBuilder(question)
                .WithSortOrder(0)
                .Build();

            var existing2 = new QuestionAnswerListBuilder(question)
                .WithSortOrder(1)
                .Build();

            var newLine = new QuestionAnswerListBuilder(question)
                .Build();

            _context.Initialize(new List<Entity> { existing1, existing2 });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newLine;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPreOperation>(pluginContext);

            // Assert
            Assert.AreEqual(2, newLine.KTR_DisplayOrder);
        }

        [TestMethod]
        public void QuestionAnswerLine_FirstEntry_GetsSortOrderZero()
        {
            // Arrange
            var question = new QuestionBankBuilder()
                 .Build();

            var newLine = new QuestionAnswerListBuilder(question)
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
        public void QuestionAnswerLine_Skips_When_Parent_Not_Set()
        {
            // Arrange
            var newQuestionAnswerLine = new QuestionAnswerListBuilder().Build(); 

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newQuestionAnswerLine;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPreOperation>(pluginContext);

            // Assert
            Assert.IsFalse(newQuestionAnswerLine.Attributes.Contains(KTR_QuestionAnswerList.Fields.KTR_DisplayOrder));
        }

        [TestMethod]
        public void QuestionAnswerLine_Skips_When_SortOrder_Is_Already_Set()
        {
            // Arrange
            var expectedSortOrder = 3;
            var newQuestionAnswerLine = new QuestionAnswerListBuilder()
                .WithSortOrder(expectedSortOrder)
                .Build();

            _context.Initialize(new List<Entity> { newQuestionAnswerLine });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newQuestionAnswerLine;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPreOperation>(pluginContext);

            // Assert
            Assert.AreEqual(expectedSortOrder, newQuestionAnswerLine.KTR_DisplayOrder);
        }
    }
}
