using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    [TestClass]
    public class QuestionnaireLineAnswerListPreOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        [TestInitialize]
        public void Initialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
        }

        [TestMethod]
        public void ExecutePlugin_OnCreateDuplicatedAnswerCode_ThrowsExeption()
        {
            // Arrange
            var questionnaireLineId = Guid.NewGuid();
            var duplicateAnswerCode = "DuplicateAnswerCode";

            var existingRecord = new Entity(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine] = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineId),
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_Name] = duplicateAnswerCode,
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerType] = new OptionSetValue((int)KTR_AnswerType.Column)
            };

            var targetEntity = new Entity(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine] = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineId),
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_Name] = duplicateAnswerCode,
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerType] = new OptionSetValue((int)KTR_AnswerType.Column)
            };

            _context.Initialize(new List<Entity> { existingRecord });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = targetEntity;

            try
            {
                _context.ExecutePluginWith<QuestionnaireLineAnswerListPreOperation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                // Assert: Exception message should mention duplicate
                StringAssert.Contains(ex.Message, duplicateAnswerCode);
            }
        }

        [TestMethod]
        public void ExecutePlugin_OnCreateDuplicatedAnswerCodeDifferentType_NotThrowsExeption()
        {
            // Arrange
            var questionnaireLineId = Guid.NewGuid();
            var duplicateAnswerCode = "DuplicateAnswerCode";

            var existingRecord = new Entity(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine] = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineId),
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_Name] = duplicateAnswerCode,
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerType] = new OptionSetValue((int)KTR_AnswerType.Column)
            };

            var targetEntity = new Entity(KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine] = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineId),
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_Name] = duplicateAnswerCode,
                [KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerType] = new OptionSetValue((int)KTR_AnswerType.Row)
            };

            var questionnaireLine = new Entity(KT_QuestionnaireLines.EntityLogicalName)
            {
                Id = questionnaireLineId,
                [KT_QuestionnaireLines.Fields.KTR_EditCustomAnswerCode] = false
            };

            _context.Initialize(new List<Entity> { existingRecord, questionnaireLine });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = targetEntity;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAnswerListPreOperation>(pluginContext);

            // Assert: Should not throw, and the target entity should have the expected answer type
            Assert.AreEqual((int)KTR_AnswerType.Row, ((OptionSetValue)targetEntity[KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerType]).Value);
        }

        [TestMethod]
        public void ExecutePlugin_OnCreateValidAnswerCode_NotThrowsException()
        {
            // Arrange
            var questionnaireLineId = Guid.NewGuid();
            var questionnaireLine = new KT_QuestionnaireLines { Id = questionnaireLineId };
            var uniqueAnswerCode = "UniqueCode";

            var targetEntity = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithAnswerCode(uniqueAnswerCode)
                .WithAnswerType(KTR_AnswerType.Column)
                .Build();

            _context.Initialize(new List<Entity> { questionnaireLine }); // No existing records

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = targetEntity;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAnswerListPreOperation>(pluginContext);

            // Assert: The target entity should have the expected answer code
            Assert.AreEqual(uniqueAnswerCode, targetEntity[KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerCode]);
        }

        [TestMethod]
        public void ExecutePlugin_OnUpdateDuplicateAnswerCode_ThrowsException()
        {
            // Arrange
            var questionnaireLineId = Guid.NewGuid();
            var questionnaireLine = new KT_QuestionnaireLines { Id = questionnaireLineId };
            var duplicateAnswerCode = "DuplicateCode";

            var existingRecord = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithAnswerName(duplicateAnswerCode)
                .WithAnswerType(KTR_AnswerType.Column)
                .Build();

            var targetEntity = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithAnswerName(duplicateAnswerCode)
                .WithAnswerType(KTR_AnswerType.Column)
                .Build();

            var preImage = targetEntity.Clone(); // Simulate PreImage

            _context.Initialize(new List<Entity> { existingRecord });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = targetEntity;
            pluginContext.PreEntityImages["AnswerIdValidate"] = preImage;

            try
            {
                _context.ExecutePluginWith<QuestionnaireLineAnswerListPreOperation>(pluginContext);
                Assert.Fail("Expected InvalidPluginExecutionException was not thrown.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                // Assert: Exception message should mention duplicate
                StringAssert.Contains(ex.Message, duplicateAnswerCode);
            }
        }

        [TestMethod]
        public void ExecutePlugin_OnUpdateValidAnswerCode_NotThrowsException()
        {
            // Arrange
            var questionnaireLineId = Guid.NewGuid();
            var questionnaireLine = new KT_QuestionnaireLines { Id = questionnaireLineId };
            var uniqueAnswerCode = "UniqueCode";

            var targetEntity = new QuestionnaireLinesAnswerListBuilder(questionnaireLine)
                .WithAnswerCode(uniqueAnswerCode)
                .WithAnswerType(KTR_AnswerType.Row)
                .Build();

            var preImage = targetEntity.Clone(); // Simulate PreImage

            _context.Initialize(new List<Entity> { questionnaireLine }); // No duplicates

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = targetEntity;
            pluginContext.PreEntityImages["AnswerIdValidate"] = preImage;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAnswerListPreOperation>(pluginContext);

            // Assert: The target entity should have the expected answer code and type
            Assert.AreEqual(uniqueAnswerCode, targetEntity[KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerCode]);
            Assert.AreEqual((int)KTR_AnswerType.Row, ((OptionSetValue)targetEntity[KTR_QuestionnaireLinesAnswerList.Fields.KTR_AnswerType]).Value);
        }
    }
}
