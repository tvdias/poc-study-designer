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
        public void QuestionnaireLineAnswerList_OnDeactivate_Shifts_Lower_Siblings_Up()
        {
            // Arrange
            var project = new ProjectBuilder().Build();

            var questionnaireLine = new QuestionnaireLineBuilder(project)
                .WithSortOrder(0)
                .WithState(0)
                .Build();

            var a1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithDisplayOrder(0)
                .Build();

            var a2 = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithDisplayOrder(1)
                .Build();

            var a3 = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithDisplayOrder(2)
                .Build();

            var target = a2;
            target["statecode"] = new OptionSetValue(1); // Deactivating a2

            _context.Initialize(new List<Entity> { a1, a2, a3 });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = target;
            pluginContext.PreEntityImages["PreImage"] = a2;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery(KTR_QuestionnaireLinesAnswerList.EntityLogicalName).ToList();
            Assert.AreEqual(0, (int)updated.First(e => e.Id == a1.Id)[KTR_QuestionnaireLinesAnswerList.Fields.KTR_DisplayOrder]);
            Assert.AreEqual(1, (int)updated.First(e => e.Id == a3.Id)[KTR_QuestionnaireLinesAnswerList.Fields.KTR_DisplayOrder]);
        }

        [TestMethod]
        public void QuestionnaireLineAnswerList_OnReactivation_Appends_To_End()
        {
            // Arrange
            var project = new ProjectBuilder().Build();

            var questionnaireLine = new QuestionnaireLineBuilder(project)
                .WithSortOrder(0)
                .WithState(0)
                .Build();

            var existing1 = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithDisplayOrder(0)
                .Build();

            var existing2 = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithDisplayOrder(1)
                .Build();

            var aId = Guid.NewGuid();

            var reactivated = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithId(aId)
                .WithState(0)
                .Build();

            var preImage = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithId(aId)
                .WithState(1)
                .Build();

            _context.Initialize(new List<Entity> { existing1, existing2, reactivated });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = reactivated;
            pluginContext.PreEntityImages["PreImage"] = preImage;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery(KTR_QuestionnaireLinesAnswerList.EntityLogicalName).ToList();
            var updatedReact = updated.First(e => e.Id == reactivated.Id);
            Assert.AreEqual(2, updatedReact[KTR_QuestionnaireLinesAnswerList.Fields.KTR_DisplayOrder]);
        }


        [TestMethod]
        public void QuestionnaireLineAnswerList_Skips_When_Parent_Not_Set()
        {
            // Arrange
            var newAnswer = new QuestionnaireLinesAnswerListBuilder().Build(); // no questionnaireLine set

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = newAnswer;
            _context.Initialize(new List<Entity> { newAnswer });
            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPostOperation>(pluginContext);

            // Assert
            Assert.IsFalse(newAnswer.Attributes.Contains(KTR_QuestionnaireLinesAnswerList.Fields.KTR_DisplayOrder));
        }
    }
}
