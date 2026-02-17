using System;
using System.Collections.Generic;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.SubsetEntities;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.SubsetEntities
{
    [TestClass]
    public class CreateUpdateDeleteSubsetEntitiesPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        private Entity _project;
        private Entity _managedList;
        private Entity _managedListEntity;

        private Entity _study; // KT_Study
        private Entity _subsetDefinition; // KTR_SubsetDefinition
        private Entity _studySubsetDef; // KTR_StudySubsetDefinition (links subsetDef -> study)
        private Entity _subsetEntity; // KTR_SubsetEntities record
        private Entity _qls; // KTR_QuestionnaireLineSubset record (to indicate association)

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();

            // Minimal project / managed list scaffolding in case builders require it
            var _project = new ProjectBuilder().Build();
            var _managedList = new ManagedListBuilder(_project).Build();
            var _managedListEntity = new ManagedListEntityBuilder(_managedList).Build();

            // Create a Study
            var study = new StudyBuilder(_project)
                .WithName("Test Study")
                .Build();
            _study = study;

            // Create a SubsetDefinition
            var subsetDefinition = new SubsetDefinitionBuilder()
                .WithName("Test Subset Definition")
                .Build();
            _subsetDefinition = subsetDefinition;

            // Create a StudySubsetDefinition linking subset definition -> study
            var studySubsetDef = new StudySubsetDefinitionBuilder()
                .WithStudy(study)
                .WithSubsetDefinition(subsetDefinition)
                .Build();
            _studySubsetDef = studySubsetDef;

            // Create a SubsetEntities record belonging to subsetDefinition
            var subsetEntity = new SubsetEntitiesBuilder(_managedListEntity, subsetDefinition)
                .WithName("Entity 1")
                .Build();
            _subsetEntity = subsetEntity;

            // Create a QuestionnaireLineSubset to indicate there's an active association for that study & subset
            var qls = new QuestionnaireLineSubsetBuilder()
                .WithStudy(study)
                .WithSubsetDefinition(subsetDefinition)
                .Build();

            qls[KTR_QuestionnaireLineSubset.Fields.StateCode] = new OptionSetValue((int)KTR_QuestionnaireLineSubset_StateCode.Active);
            _qls = qls;

            // Initialize context with entities that repository and plugin will query
            _context.Initialize(new List<Entity>
            {
                _project,
                _managedList,
                _managedListEntity,
                _study,
                _subsetDefinition,
                _studySubsetDef,
                _subsetEntity,
                _qls
            });
        }

        // ---------------------------
        // EARLY-EXIT TESTS
        // ---------------------------

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetMissing()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "SomeOtherMessage"; // not Create/Update/Delete
            pluginContext.InputParameters = new ParameterCollection(); // no Target

            _context.ExecutePluginWith<CreateUpdateDeleteSubsetEntitiesPostOperation>(pluginContext);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Delete_ShouldReturn_WhenTargetWrongType()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PostOperation;
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new EntityReference("some_other_entity", Guid.NewGuid()) }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteSubsetEntitiesPostOperation>(pluginContext);

            // Ensure study was not updated
            var saved = _service.Retrieve(_study.LogicalName, _study.Id, new ColumnSet(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsFalse(saved.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
        }

        // ---------------------------
        // DELETE PATH TESTS
        // ---------------------------

        [TestMethod]
        public void Delete_ShouldReturn_WhenPreImageMissing()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PostOperation;
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new EntityReference(KTR_SubsetEntities.EntityLogicalName, Guid.NewGuid()) }
            };

            // No PreImage -> plugin exits
            _context.ExecutePluginWith<CreateUpdateDeleteSubsetEntitiesPostOperation>(pluginContext);

            var saved = _service.Retrieve(_study.LogicalName, _study.Id, new ColumnSet(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsFalse(saved.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
        }

        [TestMethod]
        public void Delete_ShouldRebuildStudies_WhenPreImageHasSubsetDefinitionAndImpactedStudiesExist()
        {
            // PreImage contains the subset definition lookup
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PostOperation;
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new EntityReference(KTR_SubsetEntities.EntityLogicalName, _subsetEntity.Id) }
            };

            var preImage = new Entity(KTR_SubsetEntities.EntityLogicalName)
            {
                Id = _subsetEntity.Id
            };
            preImage[KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion] = new EntityReference(_subsetDefinition.LogicalName, _subsetDefinition.Id);

            pluginContext.PreEntityImages.Add("PreImage", preImage);

            // Act
            _context.ExecutePluginWith<CreateUpdateDeleteSubsetEntitiesPostOperation>(pluginContext);

            // Assert - study record should have been updated with KTR_SubsetListsHtml (possibly empty but present)
            var saved = _service.Retrieve(_study.LogicalName, _study.Id, new ColumnSet(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsTrue(saved.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsNotNull(saved.GetAttributeValue<string>(KT_Study.Fields.KTR_SubsetListsHtml));
        }

        // ---------------------------
        // CREATE / UPDATE PATH TESTS
        // ---------------------------

        [TestMethod]
        public void Create_ShouldRebuildStudies_WhenTargetHasSubsetDefinition()
        {
            var target = new Entity(KTR_SubsetEntities.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            target[KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion] = new EntityReference(_subsetDefinition.LogicalName, _subsetDefinition.Id);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteSubsetEntitiesPostOperation>(pluginContext);

            var saved = _service.Retrieve(_study.LogicalName, _study.Id, new ColumnSet(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsTrue(saved.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsNotNull(saved.GetAttributeValue<string>(KT_Study.Fields.KTR_SubsetListsHtml));
        }

        [TestMethod]
        public void Update_ShouldUsePreImage_WhenTargetDoesNotContainSubsetDefinition()
        {
            var target = new Entity(KTR_SubsetEntities.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };

            var preImage = new Entity(KTR_SubsetEntities.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            preImage[KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion] = new EntityReference(_subsetDefinition.LogicalName, _subsetDefinition.Id);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };
            pluginContext.PreEntityImages.Add("PreImage", preImage);

            _context.ExecutePluginWith<CreateUpdateDeleteSubsetEntitiesPostOperation>(pluginContext);

            var saved = _service.Retrieve(_study.LogicalName, _study.Id, new ColumnSet(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsTrue(saved.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsNotNull(saved.GetAttributeValue<string>(KT_Study.Fields.KTR_SubsetListsHtml));
        }

        [TestMethod]
        public void Update_ShouldRetrieveSubsetDefinition_WhenMissingInTargetAndPreImage()
        {
            // Simulate stored subset entity in DB with subset definition lookup so plugin's Retrieve will return it
            var stored = new Entity(KTR_SubsetEntities.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            stored[KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion] = new EntityReference(_subsetDefinition.LogicalName, _subsetDefinition.Id);

            // Add to context WITHOUT reinitializing
            _context.Data[KTR_SubsetEntities.EntityLogicalName][stored.Id] = stored;

            var target = new Entity(KTR_SubsetEntities.EntityLogicalName)
            {
                Id = stored.Id // plugin will call Retrieve to get subset definition
            };

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteSubsetEntitiesPostOperation>(pluginContext);

            var saved = _service.Retrieve(_study.LogicalName, _study.Id, new ColumnSet(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsTrue(saved.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsNotNull(saved.GetAttributeValue<string>(KT_Study.Fields.KTR_SubsetListsHtml));
        }
    }
}
