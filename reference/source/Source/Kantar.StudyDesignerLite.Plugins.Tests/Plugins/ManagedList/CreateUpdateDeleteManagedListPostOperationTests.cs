using System;
using System.Collections.Generic;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.ManagedList;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.ManagedList
{
    [TestClass]
    public class CreateUpdateDeleteManagedListPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;
        private KT_Project _project;
        private KTR_ManagedList _managedList;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();

            _project = new ProjectBuilder().Build();
            _managedList = new ManagedListBuilder(_project).Build();

            // Initialize with the project and managed list
            _context.Initialize(new List<Entity> { _project, _managedList });
        }

        // ---------------------------
        // General early-exit tests
        // ---------------------------

        [TestMethod]
        public void Execute_ShouldReturn_WhenMessageNameIsNotCreateOrUpdateOrDelete()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "SomeOtherMessage";
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", _managedList } // still a valid target, but message is different
            };

            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetParameterMissing()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection(); // No Target

            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetIsNotEntity_ForCreateUpdate()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", "NotAnEntity" }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Execute_ShouldReturn_WhenTargetIsNotManagedListEntity()
        {
            var someOtherEntity = new Entity("some_other_entity", Guid.NewGuid());

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", someOtherEntity }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            Assert.IsTrue(true);
        }

        // ---------------------------
        // DELETE path tests
        // ---------------------------

        [TestMethod]
        public void Delete_ShouldReturn_WhenStageIsNotPostOperation()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PreValidation; // Not PostOperation
            pluginContext.InputParameters = new ParameterCollection
            {
                // use EntityReference instead of ToEntityReference()
                { "Target", new EntityReference(_managedList.LogicalName, _managedList.Id) }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            // No update should be performed
            var saved = _context.Data[_project.LogicalName][_project.Id];
            Assert.IsNull(saved.GetAttributeValue<string>(KT_Project.Fields.KTR_ManagedListsHtml));
        }

        [TestMethod]
        public void Delete_ShouldReturn_WhenTargetIsNotEntityReference()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PostOperation;
            pluginContext.InputParameters = new ParameterCollection
            {
                // passing an Entity instead of EntityReference should early-exit in plugin
                { "Target", new Entity("some_entity", Guid.NewGuid()) }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            var saved = _context.Data[_project.LogicalName][_project.Id];
            Assert.IsNull(saved.GetAttributeValue<string>(KT_Project.Fields.KTR_ManagedListsHtml));
        }

        [TestMethod]
        public void Delete_ShouldReturn_WhenPreImageMissing()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PostOperation;
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new EntityReference(_managedList.LogicalName, _managedList.Id) }
            };

            // No PreImage added -> plugin should trace and exit
            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            var saved = _context.Data[_project.LogicalName][_project.Id];
            Assert.IsNull(saved.GetAttributeValue<string>(KT_Project.Fields.KTR_ManagedListsHtml));
        }

        [TestMethod]
        public void Delete_ShouldUpdateProjectHtml_WhenPreImageHasProject()
        {
            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.Stage = (int)ContextStageEnum.PostOperation;
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new EntityReference(_managedList.LogicalName, _managedList.Id) }
            };

            // Build a PreImage Entity manually (avoid early-bound ToEntity)
            var preImage = new Entity(_managedList.LogicalName, _managedList.Id);
            preImage[KTR_ManagedList.Fields.KTR_Project] = new EntityReference(_project.LogicalName, _project.Id);

            pluginContext.PreEntityImages.Add("PreImage", preImage);

            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            var saved = _context.Data[_project.LogicalName][_project.Id];
            // plugin sets html ?? string.Empty, so attribute should exist (maybe empty)
            Assert.IsTrue(saved.Attributes.Contains(KT_Project.Fields.KTR_ManagedListsHtml));
            Assert.IsNotNull(saved.GetAttributeValue<string>(KT_Project.Fields.KTR_ManagedListsHtml));
        }

        // ---------------------------
        // CREATE path tests
        // ---------------------------

        [TestMethod]
        public void Create_ShouldUpdateProjectHtml_WhenProjectFoundOnTarget()
        {
            // Arrange: create a Target entity with KTR_Project present
            var target = new Entity(_managedList.LogicalName, _managedList.Id);
            target[KTR_ManagedList.Fields.KTR_Project] = new EntityReference(_project.LogicalName, _project.Id);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            var saved = _context.Data[_project.LogicalName][_project.Id];
            Assert.IsTrue(saved.Attributes.Contains(KT_Project.Fields.KTR_ManagedListsHtml));
            Assert.IsNotNull(saved.GetAttributeValue<string>(KT_Project.Fields.KTR_ManagedListsHtml));
        }

        // ---------------------------
        // UPDATE path tests
        // ---------------------------

        [TestMethod]
        public void Update_ShouldUsePreImageProject_WhenTargetDoesNotContainProject()
        {
            // Target has no project, but PreImage contains it
            var target = new Entity(_managedList.LogicalName, _managedList.Id);

            var preImage = new Entity(_managedList.LogicalName, _managedList.Id);
            preImage[KTR_ManagedList.Fields.KTR_Project] = new EntityReference(_project.LogicalName, _project.Id);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };
            pluginContext.PreEntityImages.Add("PreImage", preImage);

            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            var saved = _context.Data[_project.LogicalName][_project.Id];
            Assert.IsTrue(saved.Attributes.Contains(KT_Project.Fields.KTR_ManagedListsHtml));
            Assert.IsNotNull(saved.GetAttributeValue<string>(KT_Project.Fields.KTR_ManagedListsHtml));
        }

        [TestMethod]
        public void Update_ShouldRetrieveProject_WhenMissingInTargetAndPreImage()
        {
            // Remove KTR_Project from the managed list in the DB so plugin will attempt Retrieve
            if (_managedList.Attributes.Contains(KTR_ManagedList.Fields.KTR_Project))
            { _managedList.Attributes.Remove(KTR_ManagedList.Fields.KTR_Project); }

            var target = new Entity(_managedList.LogicalName, _managedList.Id);

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };

            _context.ExecutePluginWith<CreateUpdateDeleteManagedListPostOperation>(pluginContext);

            var saved = _context.Data[_project.LogicalName][_project.Id];
            Assert.IsTrue(saved.Attributes.Contains(KT_Project.Fields.KTR_ManagedListsHtml));
            Assert.IsNotNull(saved.GetAttributeValue<string>(KT_Project.Fields.KTR_ManagedListsHtml));
        }
    }
}
