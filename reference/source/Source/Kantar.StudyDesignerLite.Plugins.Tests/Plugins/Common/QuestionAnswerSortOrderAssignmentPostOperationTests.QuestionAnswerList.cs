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
        public void QuestionAnswerList_OnDeactivate_Shifts_Lower_Siblings_Up()
        {
            // Arrange
            var question = new QuestionBankBuilder()
                .Build();

            var a1 = new QuestionAnswerListBuilder(question)
                .WithSortOrder(0)
                .Build();

            var a2 = new QuestionAnswerListBuilder(question)
                .WithSortOrder(1)
                .Build();

            var a3 = new QuestionAnswerListBuilder(question)
                .WithSortOrder(2)
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

            // Assert: a1 stays, a2 deactivated, a3 shifts from 2 to 1
            var updated = _context.CreateQuery(KTR_QuestionAnswerList.EntityLogicalName).ToList();
            Assert.AreEqual(0, (int)updated.First(e => e.Id == a1.Id)[KTR_QuestionAnswerList.Fields.KTR_DisplayOrder]);
            Assert.AreEqual(1, (int)updated.First(e => e.Id == a3.Id)[KTR_QuestionAnswerList.Fields.KTR_DisplayOrder]);
        }

        [TestMethod]
        public void QuestionAnswerList_OnReactivation_Appends_To_End()
        {
            // Arrange
            var question = new QuestionBankBuilder()
                .Build();

            var a1 = new QuestionAnswerListBuilder(question)
                .WithSortOrder(0)
                .Build();

            var a2 = new QuestionAnswerListBuilder(question)
                .WithSortOrder(1)
                .Build();

            var aId = Guid.NewGuid();

            var reactivated = new QuestionAnswerListBuilder(question)
                .WithId(aId)
                .WithState(0)
                .Build();

            var preImage = new QuestionAnswerListBuilder(question)
                .WithId(aId)
                .WithState(1)
                .Build();

            _context.Initialize(new List<Entity> { a1,a2, reactivated });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = reactivated;
            pluginContext.PreEntityImages["PreImage"] = preImage;

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery(KTR_QuestionAnswerList.EntityLogicalName).ToList();
            var updatedReact = updated.First(e => e.Id == reactivated.Id);

            Assert.AreEqual(2, updatedReact[KTR_QuestionAnswerList.Fields.KTR_DisplayOrder]);
        }

        [TestMethod]
        public void QuestionAnswerList_Skips_When_Parent_Not_Set()
        {
            // Arrange
            var newQuestionnaireAnswerList = new QuestionAnswerListBuilder().Build(); 

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = newQuestionnaireAnswerList;

            _context.Initialize(new List<Entity> { newQuestionnaireAnswerList });

            // Act
            _context.ExecutePluginWith<QuestionAnswerSortOrderAssignmentPostOperation>(pluginContext);

            // Assert
            Assert.IsFalse(newQuestionnaireAnswerList.Attributes.Contains(KTR_QuestionAnswerList.Fields.KTR_DisplayOrder));
        }
    }
}
