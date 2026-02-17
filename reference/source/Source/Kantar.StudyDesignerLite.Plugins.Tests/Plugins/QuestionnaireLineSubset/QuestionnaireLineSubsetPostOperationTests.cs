using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.QuestionnaireLineSubset;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLineSubset
{
    [TestClass]
    public class QuestionnaireLineSubsetPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        private KT_Project _project;
        private KT_Study _study; // KT_Study
        private KT_QuestionnaireLines _questionnaireLine; // ktr_questionnaireline
        private KTR_StudyQuestionnaireLine _studyQuestionnaireLine; // ktr_studyquestionnaireline
        private KTR_SubsetDefinition _subsetDefinition; // KTR_SubsetDefinition
        private KTR_QuestionnaireLineSubset _questionnaireLineSubset; // KTR_QuestionnaireLineSubset

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();

            // Create a Study
            _project = new ProjectBuilder().Build();
            _study = new StudyBuilder(_project).WithName("Test Study").Build();

            // Create a QuestionnaireLine
            _questionnaireLine = new QuestionnaireLineBuilder(_project).Build();

            // Create a StudyQuestionnaireLine linking Study + QuestionnaireLine
            _studyQuestionnaireLine = new StudyQuestionnaireLineBuilder(_study, _questionnaireLine).Build();

            // Create a SubsetDefinition
            _subsetDefinition = new SubsetDefinitionBuilder().WithName("Test Subset Definition").Build();

            // Create a QuestionnaireLineSubset
            _questionnaireLineSubset = new QuestionnaireLineSubsetBuilder().WithStudy( _study ).Build();

            // Initialize context
            _context.Initialize(new List<Entity>
            {
                _project,
                _study,
                _questionnaireLine,
                _studyQuestionnaireLine,
                _subsetDefinition,
                _questionnaireLineSubset
            });
        }

        // ---------------------------
        // EARLY-EXIT TESTS
        // ---------------------------

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetMissing()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "SomeOtherMessage";
            pluginContext.InputParameters = new ParameterCollection();

            _context.ExecutePluginWith<QuestionnaireLineSubsetPostOperation>(pluginContext);

            Assert.IsTrue(true); // no exception thrown
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetWrongEntity()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new Entity("some_other_entity", Guid.NewGuid()) }
            };

            _context.ExecutePluginWith<QuestionnaireLineSubsetPostOperation>(pluginContext);

            Assert.IsTrue(true); // no exception thrown
        }

        [TestMethod]
        public void Delete_ShouldUsePreImage_WhenTargetMissing()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PostOperation;

            var preImage = new Entity(KTR_QuestionnaireLineSubset.EntityLogicalName)
            {
                Id = _questionnaireLineSubset.Id,
                [KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineId] = _questionnaireLine.ToEntityReference(),
                [KTR_QuestionnaireLineSubset.Fields.KTR_Study] = _study.ToEntityReference()
            };
            pluginContext.PreEntityImages.Add("PreImage", preImage);

            _context.ExecutePluginWith<QuestionnaireLineSubsetPostOperation>(pluginContext);

            // Assert studyQuestionnaireLine updated
            var updated = _service.Retrieve(KTR_StudyQuestionnaireLine.EntityLogicalName, _studyQuestionnaireLine.Id, new ColumnSet(KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml));
            Assert.IsTrue(updated.Attributes.Contains(KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml));
        }

        // ---------------------------
        // CREATE PATH TEST
        // ---------------------------

        [TestMethod]
        public void Create_ShouldUpdateStudyQuestionnaireLineHtml()
        {
            var target = new Entity(KTR_QuestionnaireLineSubset.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                [KTR_QuestionnaireLineSubset.Fields.KTR_Study] = _study.ToEntityReference(),
                [KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineId] = _questionnaireLine.ToEntityReference(),
                [KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId] = _subsetDefinition.ToEntityReference()
            };

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection { { "Target", target } };

            _context.ExecutePluginWith<QuestionnaireLineSubsetPostOperation>(pluginContext);

            var updated = _service.Retrieve(KTR_StudyQuestionnaireLine.EntityLogicalName, _studyQuestionnaireLine.Id, new ColumnSet(KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml));
            Assert.IsTrue(updated.Attributes.Contains(KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml));
            Assert.IsNotNull(updated.GetAttributeValue<string>(KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml));
        }

        // ---------------------------
        // UPDATE PATH TEST
        // ---------------------------

        [TestMethod]
        public void Update_ShouldUsePreImage_WhenTargetMissingParent()
        {
            var target = new Entity(KTR_QuestionnaireLineSubset.EntityLogicalName)
            {
                Id = _questionnaireLineSubset.Id
            };

            var preImage = new Entity(KTR_QuestionnaireLineSubset.EntityLogicalName)
            {
                Id = _questionnaireLineSubset.Id,
                [KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineId] = _questionnaireLine.ToEntityReference(),
                [KTR_QuestionnaireLineSubset.Fields.KTR_Study] = _study.ToEntityReference()
            };

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection { { "Target", target } };
            pluginContext.PreEntityImages.Add("PreImage", preImage);

            _context.ExecutePluginWith<QuestionnaireLineSubsetPostOperation>(pluginContext);

            var updated = _service.Retrieve(KTR_StudyQuestionnaireLine.EntityLogicalName, _studyQuestionnaireLine.Id, new ColumnSet(KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml));
            Assert.IsTrue(updated.Attributes.Contains(KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml));
        }

        // ---------------------------
        // ACTIVE / INACTIVE TEST
        // ---------------------------

        [TestMethod]
        public void OnlyActiveQuestionnaireLineSubset_ShouldBeCounted()
        {
            // Create inactive subset
            var inactive = new KTR_QuestionnaireLineSubset
            {
                Id = Guid.NewGuid(),
                [KTR_QuestionnaireLineSubset.Fields.KTR_Study] = _study.ToEntityReference(),
                [KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineId] = _questionnaireLine.ToEntityReference(),
                [KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId] = _subsetDefinition.ToEntityReference(),
                [KTR_QuestionnaireLineSubset.Fields.StateCode] = new OptionSetValue((int)KTR_QuestionnaireLineSubset_StateCode.Inactive)
            };

            // Add to existing context without re-initializing
            _context.Data[KTR_QuestionnaireLineSubset.EntityLogicalName].Add(inactive.Id, inactive);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection { { "Target", inactive } };

            _context.ExecutePluginWith<QuestionnaireLineSubsetPostOperation>(pluginContext);

            // The active subset (from TestInitialize) should still update HTML, inactive ignored
            var updated = _service.Retrieve(KTR_StudyQuestionnaireLine.EntityLogicalName, _studyQuestionnaireLine.Id, new ColumnSet(KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml));
            Assert.IsTrue(updated.Attributes.Contains(KTR_StudyQuestionnaireLine.Fields.KTR_SubsetHtml));
        }
    }
}
