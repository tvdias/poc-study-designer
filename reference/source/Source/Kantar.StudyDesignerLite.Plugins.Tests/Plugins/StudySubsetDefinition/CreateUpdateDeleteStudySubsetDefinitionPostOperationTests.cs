using System;
using System.Collections.Generic;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.StudySubsetDefinition;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.StudySubsetDefinition
{
    [TestClass]
    public class CreateUpdateDeleteStudySubsetDefinitionPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        private Entity _study; // KT_Study record
        private Entity _subsetDefinition; // KTR_SubsetDefinition
        private Entity _studySubsetDef; // KTR_StudySubsetDefinition linking subsetDef -> study
        private Entity _subsetEntity; // KTR_SubsetEntities record

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();

            // Arrange Project
            var project = new ProjectBuilder().Build();
            var managedList = new ManagedListBuilder(project).Build();
            var managedListEntity = new ManagedListEntityBuilder(managedList).Build();

            // --- KEEPING YOUR var, just not shadowing ---

            var study = new StudyBuilder(project)
                .WithName("Test Study")
                .Build();
            _study = study;   // assign to field

            var subsetDefinition = new SubsetDefinitionBuilder()
                .WithName("Test Subset Definition")
                .Build();
            _subsetDefinition = subsetDefinition;

            var studySubsetDef = new StudySubsetDefinitionBuilder()
                .WithStudy(study)
                .WithSubsetDefinition(subsetDefinition)
                .Build();
            _studySubsetDef = studySubsetDef;

            var subsetEntity = new SubsetEntitiesBuilder(managedListEntity, subsetDefinition)
                .WithName("Entity 1")
                .Build();
            _subsetEntity = subsetEntity;

            // Initialize context with the actual field values
            _context.Initialize(new List<Entity>
            {
                _study,
                _subsetDefinition,
                _studySubsetDef,
                _subsetEntity
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

            _context.ExecutePluginWith<CreateUpdateDeleteStudySubsetDefinitionPostOperation>(pluginContext);

            // No exception means success
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetWrongType_Delete()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PostOperation;
            // Pass wrong entity reference logical name
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new EntityReference("some_other_entity", Guid.NewGuid()) }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteStudySubsetDefinitionPostOperation>(pluginContext);

            // Study should remain untouched (no subset html set)
            var savedStudy = _context.Data[_study.LogicalName][_study.Id];
            Assert.IsFalse(savedStudy.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
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
                { "Target", new EntityReference(KTR_StudySubsetDefinition.EntityLogicalName, Guid.NewGuid()) }
            };

            // No PreImage supplied -> plugin should exit early
            _context.ExecutePluginWith<CreateUpdateDeleteStudySubsetDefinitionPostOperation>(pluginContext);

            var savedStudy = _context.Data[_study.LogicalName][_study.Id];
            Assert.IsFalse(savedStudy.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
        }

        [TestMethod]
        public void Delete_ShouldClearHtml_WhenNoSubsetDefinitionsExistForStudy()
        {
            // Arrange: PreImage referencing study, but we remove studySubsetDef records to simulate none exist
            // Remove existing studySubsetDef from the in-memory DB
            _context.Data.Remove(KTR_StudySubsetDefinition.EntityLogicalName);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PostOperation;
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new EntityReference(KTR_StudySubsetDefinition.EntityLogicalName, Guid.NewGuid()) }
            };

            var preImage = new Entity(KTR_StudySubsetDefinition.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            preImage[KTR_StudySubsetDefinition.Fields.KTR_Study] = new EntityReference(_study.LogicalName, _study.Id);

            pluginContext.PreEntityImages.Add("PreImage", preImage);

            // Act
            _context.ExecutePluginWith<CreateUpdateDeleteStudySubsetDefinitionPostOperation>(pluginContext);

            // Assert: plugin should update study with empty string
            var savedStudy = _context.Data[_study.LogicalName][_study.Id];
            Assert.IsTrue(savedStudy.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.AreEqual(string.Empty, savedStudy.GetAttributeValue<string>(KT_Study.Fields.KTR_SubsetListsHtml));
        }

        [TestMethod]
        public void Delete_ShouldUpdateHtml_WhenSubsetDefinitionsExist()
        {
            // Arrange: ensure a studySubsetDef exists (created in TestInitialize)
            // Prepare PreImage referencing study
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PostOperation;
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new EntityReference(KTR_StudySubsetDefinition.EntityLogicalName, _studySubsetDef.Id) }
            };

            var preImage = new Entity(KTR_StudySubsetDefinition.EntityLogicalName)
            {
                Id = _studySubsetDef.Id
            };
            preImage[KTR_StudySubsetDefinition.Fields.KTR_Study] = new EntityReference(_study.LogicalName, _study.Id);
            preImage[KTR_StudySubsetDefinition.Fields.KTR_SubsetDefinition] =
                new EntityReference(_subsetDefinition.LogicalName, _subsetDefinition.Id);

            pluginContext.PreEntityImages.Add("PreImage", preImage);

            // Act
            _context.ExecutePluginWith<CreateUpdateDeleteStudySubsetDefinitionPostOperation>(pluginContext);

            // Assert: plugin should update study with some html (may be empty string if Build yields empty, but we expect attribute present)
            var savedStudy = _context.Data[_study.LogicalName][_study.Id];
            Assert.IsTrue(savedStudy.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
            // Allow either empty or non-empty string, but attribute must exist
            Assert.IsNotNull(savedStudy.GetAttributeValue<string>(KT_Study.Fields.KTR_SubsetListsHtml));
        }

        // ---------------------------
        // CREATE / UPDATE PATH TESTS
        // ---------------------------

        [TestMethod]
        public void Create_ShouldUpdateHtml_WhenTargetContainsStudyRef()
        {
            // Target Entity contains KTR_Study lookup
            var target = new Entity(KTR_StudySubsetDefinition.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            target[KTR_StudySubsetDefinition.Fields.KTR_Study] = new EntityReference(_study.LogicalName, _study.Id);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteStudySubsetDefinitionPostOperation>(pluginContext);

            var savedStudy = _context.Data[_study.LogicalName][_study.Id];
            Assert.IsTrue(savedStudy.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsNotNull(savedStudy.GetAttributeValue<string>(KT_Study.Fields.KTR_SubsetListsHtml));
        }

        [TestMethod]
        public void Update_ShouldUsePreImage_WhenTargetDoesNotContainStudyRef()
        {
            // Target has no study lookup but PreImage does
            var target = new Entity(KTR_StudySubsetDefinition.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };

            var preImage = new Entity(KTR_StudySubsetDefinition.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            preImage[KTR_StudySubsetDefinition.Fields.KTR_Study] = new EntityReference(_study.LogicalName, _study.Id);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };
            pluginContext.PreEntityImages.Add("PreImage", preImage);

            _context.ExecutePluginWith<CreateUpdateDeleteStudySubsetDefinitionPostOperation>(pluginContext);

            var savedStudy = _context.Data[_study.LogicalName][_study.Id];
            Assert.IsTrue(savedStudy.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsNotNull(savedStudy.GetAttributeValue<string>(KT_Study.Fields.KTR_SubsetListsHtml));
        }

        [TestMethod]
        public void Update_ShouldRetrieveStudy_WhenMissingInTargetAndPreImage()
        {
            // Stored definition exists in DB but Target does not contain the study ref
            var storedDefinition = new Entity(KTR_StudySubsetDefinition.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };
            storedDefinition[KTR_StudySubsetDefinition.Fields.KTR_Study] =
                new EntityReference(_study.LogicalName, _study.Id);

            // Add storedDefinition directly into context (NO reinitialize!)
            _context.Data[KTR_StudySubsetDefinition.EntityLogicalName][storedDefinition.Id] = storedDefinition;

            var target = new Entity(KTR_StudySubsetDefinition.EntityLogicalName)
            {
                Id = storedDefinition.Id // missing study → plugin must Retrieve()
            };

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };

            // Act
            _context.ExecutePluginWith<CreateUpdateDeleteStudySubsetDefinitionPostOperation>(pluginContext);

            // Assert: study must have been updated
            var savedStudy = _context.Data[_study.LogicalName][_study.Id];
            Assert.IsTrue(savedStudy.Attributes.Contains(KT_Study.Fields.KTR_SubsetListsHtml));
            Assert.IsNotNull(savedStudy.GetAttributeValue<string>(KT_Study.Fields.KTR_SubsetListsHtml));
        }
    }
}
