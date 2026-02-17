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
        public void QuestionnaireLineAnswerList_Assigns_MaxPlusOne_SortOrder()
        {
            // Arrange
            var project = new ProjectBuilder()
                .Build();

            var questionnaireline = new QuestionnaireLineBuilder(project)
                .WithSortOrder(0)
                .WithState(0)
                .Build();

            var existing1 = new QuestionnaireLinesAnswerListBuilder(questionnaireline)
                .WithDisplayOrder(0)
                .Build();

            var existing2 = new QuestionnaireLinesAnswerListBuilder(questionnaireline)
                .WithDisplayOrder(1)
                .Build();

            var newLine = new QuestionnaireLinesAnswerListBuilder(questionnaireline)
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
        public void QuestionnaireLineAnswerList_FirstEntry_GetsSortOrderZero()
        {
            // Arrange
            var project = new ProjectBuilder()
                .Build();

            var questionnaireline = new QuestionnaireLineBuilder(project)
                .WithSortOrder(0)
                .WithState(0)
                .Build();

            var newLine = new QuestionnaireLinesAnswerListBuilder(questionnaireline)
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
        public void QuestionnaireLineAnswerList_Skips_When_Parent_Not_Set()
        {
            // Arrange
            var newLine = new QuestionnaireLinesAnswerListBuilder().Build(); 

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newLine;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPreOperation>(pluginContext);

            // Assert
            Assert.IsFalse(newLine.Attributes.Contains(KTR_QuestionnaireLinesAnswerList.Fields.KTR_DisplayOrder));
        }

        [TestMethod]
        public void QuestionnaireLineAnswerList_Skips_When_Depth_Greater_Than_One()
        {
            // Arrange
            var project = new ProjectBuilder()
                .Build();

            var questionnaireline = new QuestionnaireLineBuilder(project)
                .WithSortOrder(0)
                .WithState(0)
                .Build();

            var newLine = new QuestionnaireLinesAnswerListBuilder(questionnaireline)
               .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.Depth = 2; // triggers depth check
            pluginContext.InputParameters["Target"] = newLine;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPreOperation>(pluginContext);

            // Assert
            Assert.IsFalse(newLine.Attributes.Contains(KTR_QuestionnaireLinesAnswerList.Fields.KTR_DisplayOrder));
        }

        [TestMethod]
        public void QuestionnaireLineAnswerList_Skips_When_SortOrder_Is_Already_Set()
        {
            // Arrange
            var expectedSortOrder = 3;
            var project = new ProjectBuilder()
                .Build();

            var questionnaireline = new QuestionnaireLineBuilder(project)
                .WithSortOrder(0)
                .WithState(0)
                .Build();

            var newQuesitonnaireLineAnswer = new QuestionnaireLinesAnswerListBuilder(questionnaireline)
                .WithDisplayOrder(expectedSortOrder)
               .Build();

            _context.Initialize(new List<Entity> { newQuesitonnaireLineAnswer });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newQuesitonnaireLineAnswer;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPreOperation>(pluginContext);

            // Assert
            Assert.AreEqual(expectedSortOrder, newQuesitonnaireLineAnswer.KTR_DisplayOrder);
        }
    }
}
