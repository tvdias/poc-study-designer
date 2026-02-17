using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    [TestClass]
    public class QuestionnaireLineCustomAnswerTogglePostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;

        [TestInitialize]
        public void Initialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
        }

        [TestMethod]
        public void WhenToggleUpdatedToTrue_ShouldUpdateRelatedAnswersToTrue()
        {
            // Arrange
            var qBank = new QuestionBankBuilder().Build();
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project)
                .WithEditCustomAnswerCode(false)
                .Build();

            var answer1 = new QuestionnaireLinesAnswerListBuilder(qLine)
                .WithCustomAnswerCodeEditToggle(false)
                .Build();

            var answer2 = new QuestionnaireLinesAnswerListBuilder(qLine)
                .WithCustomAnswerCodeEditToggle(false)
                .Build();

            // Excluded answer → has QuestionBank
            var excludedAnswer = new QuestionnaireLinesAnswerListBuilder(qLine)
                .WithQuestionBank(qBank)
                .WithCustomAnswerCodeEditToggle(false)
                .Build();

            _context.Initialize(new List<Entity> { project, qLine, answer1, answer2, excludedAnswer });

            // Simulate update to true
            var updatedQLine = new Entity(KT_QuestionnaireLines.EntityLogicalName) { Id = qLine.Id };
            updatedQLine[KT_QuestionnaireLines.Fields.KTR_EditCustomAnswerCode] = true;

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = updatedQLine;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineCustomAnswerTogglePostOperation>(pluginContext);

            // Assert
            var updatedAnswers = _context.CreateQuery<KTR_QuestionnaireLinesAnswerList>()
            .ToList()
            .Where(a =>
            {
                var er = a.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine);
                return er != null && er.Id == qLine.Id;
            })
            .ToList();

            // Should be updated → true
            Assert.IsTrue(updatedAnswers.First(x => x.Id == answer1.Id)
                .GetAttributeValue<bool>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_EnableCustomAnswerCodeEditing));

            Assert.IsTrue(updatedAnswers.First(x => x.Id == answer2.Id)
                .GetAttributeValue<bool>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_EnableCustomAnswerCodeEditing));

            // Excluded → should stay false
            Assert.IsFalse(updatedAnswers.First(x => x.Id == excludedAnswer.Id)
                .GetAttributeValue<bool>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_EnableCustomAnswerCodeEditing));
        }

        [TestMethod]
        public void WhenToggleUpdatedToFalse_ShouldUpdateRelatedAnswersToFalse()
        {
            // Arrange
            var qBank = new QuestionBankBuilder().Build();
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project)
                .WithEditCustomAnswerCode(true)
                .Build();

            // Eligible answers
            var answer1 = new QuestionnaireLinesAnswerListBuilder(qLine)
                .WithCustomAnswerCodeEditToggle(true)
                .Build();

            var answer2 = new QuestionnaireLinesAnswerListBuilder(qLine)
                .WithCustomAnswerCodeEditToggle(true)
                .Build();

            // Excluded answer → has QuestionBank
            var excludedAnswer = new QuestionnaireLinesAnswerListBuilder(qLine)
                .WithQuestionBank(qBank)
                .WithCustomAnswerCodeEditToggle(true)
                .Build();

            _context.Initialize(new List<Entity> { project, qLine, answer1, answer2, excludedAnswer });

            // Simulate update to false
            var updatedQLine = new Entity(KT_QuestionnaireLines.EntityLogicalName) { Id = qLine.Id };
            updatedQLine[KT_QuestionnaireLines.Fields.KTR_EditCustomAnswerCode] = false;

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = updatedQLine;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineCustomAnswerTogglePostOperation>(pluginContext);

            // Assert
            var updatedAnswers = _context.CreateQuery<KTR_QuestionnaireLinesAnswerList>()
            .ToList()
            .Where(a =>
            {
                var er = a.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine);
                return er != null && er.Id == qLine.Id;
            })
            .ToList();

            // Should be updated → false
            Assert.IsFalse(updatedAnswers.First(x => x.Id == answer1.Id)
                .GetAttributeValue<bool>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_EnableCustomAnswerCodeEditing));

            Assert.IsFalse(updatedAnswers.First(x => x.Id == answer2.Id)
                .GetAttributeValue<bool>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_EnableCustomAnswerCodeEditing));

            // Excluded → should stay true
            Assert.IsTrue(updatedAnswers.First(x => x.Id == excludedAnswer.Id)
                .GetAttributeValue<bool>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_EnableCustomAnswerCodeEditing));
        }
    }
}
