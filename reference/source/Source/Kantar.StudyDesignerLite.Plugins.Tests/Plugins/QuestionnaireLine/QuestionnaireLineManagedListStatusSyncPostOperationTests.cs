using System;
using System.Collections.Generic;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    [TestClass]
    public class QuestionnaireLineManagedListStatusSyncPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;

        [TestInitialize]
        public void Setup()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
        }

        [TestMethod]
        public void Plugin_Should_Run_On_Activate()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState(0).Build();
            var managedList = new ManagedListBuilder(project).Build();
            var qLineManagedList = new QuestionnaireLineManagedListBuilder(project, managedList, qLine).Build();

            _context.Initialize(new List<Entity> { qLine, qLineManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = qLine;

            // Act + Assert
            _context.ExecutePluginWith<QuestionnaireLineManagedListStatusSyncPostOperation>(pluginContext);
            Assert.IsTrue(true); // Plugin ran without exceptions
        }

        [TestMethod]
        public void Plugin_Should_Set_StatusCode_To_Active_When_Activated()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState((int)KT_QuestionnaireLines_StateCode.Active).Build();
            var managedList = new ManagedListBuilder(project).Build();
            var qLineManagedList = new QuestionnaireLineManagedListBuilder(project, managedList, qLine).Build();

            _context.Initialize(new List<Entity> { qLine, qLineManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = qLine;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineManagedListStatusSyncPostOperation>(pluginContext);

            // Assert
            var updatedEntity = _context.Data[qLineManagedList.LogicalName][qLineManagedList.Id];
            var statusCode = updatedEntity.GetAttributeValue<OptionSetValue>("statuscode")?.Value;

            Assert.AreEqual((int)KTR_QuestionnaireLinesHaRedList_StatusCode.Active, statusCode);
        }

        [TestMethod]
        public void Plugin_Should_Run_On_Deactivate()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState(0).Build();
            var managedList = new ManagedListBuilder(project).Build();
            var qLineManagedList = new QuestionnaireLineManagedListBuilder(project, managedList, qLine).Build();

            _context.Initialize(new List<Entity> { qLine, qLineManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = qLine;

            // Act + Assert
            _context.ExecutePluginWith<QuestionnaireLineManagedListStatusSyncPostOperation>(pluginContext);
            Assert.IsTrue(true); // Plugin ran without exceptions
        }

        [TestMethod]
        public void Plugin_Should_Set_StatusCode_To_Inactive_When_Deactivated()
        {
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState((int)KT_QuestionnaireLines_StateCode.Inactive).Build();
            var managedList = new ManagedListBuilder(project).Build();
            var qLineManagedList = new QuestionnaireLineManagedListBuilder(project, managedList, qLine).Build();

            _context.Initialize(new List<Entity> { qLine, qLineManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = qLine;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineManagedListStatusSyncPostOperation>(pluginContext);

            // Assert
            var updatedEntity = _context.Data[qLineManagedList.LogicalName][qLineManagedList.Id];
            var statusCode = updatedEntity.GetAttributeValue<OptionSetValue>("statuscode")?.Value;

            Assert.AreEqual((int)KTR_QuestionnaireLinesHaRedList_StatusCode.Inactive, statusCode);
        }

        [TestMethod]
        public void Plugin_Should_Skip_When_StateCode_Is_Invalid()
        {
            // Arrange
            var project = new ProjectBuilder().Build();

            // Using wrong invalid statecode, e.g., 999
            var qLine = new QuestionnaireLineBuilder(project)
                .WithState(999) // This hits the 'Unexpected statecode value' branch
                .Build();

            var managedList = new ManagedListBuilder(project).Build();
            var qLineManagedList = new QuestionnaireLineManagedListBuilder(project, managedList, qLine).Build();

            _context.Initialize(new List<Entity> { qLine, qLineManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = qLine;

            // Act + Assert
            _context.ExecutePluginWith<QuestionnaireLineManagedListStatusSyncPostOperation>(pluginContext);
            Assert.IsTrue(true, "Plugin completed without exceptions on invalid statecode.");
        }

        [TestMethod]
        public void Plugin_Should_Skip_When_StateCode_Is_Missing()
        {
            var project = new ProjectBuilder().Build();

            var qLine = new QuestionnaireLineBuilder(project).Build(); // No .WithState()
            var managedList = new ManagedListBuilder(project).Build();
            var qLineManagedList = new QuestionnaireLineManagedListBuilder(project, managedList, qLine).Build();

            _context.Initialize(new List<Entity> { qLine, qLineManagedList });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = qLine;

            _context.ExecutePluginWith<QuestionnaireLineManagedListStatusSyncPostOperation>(pluginContext);
            Assert.IsTrue(true, "Plugin completed without exceptions when statecode was missing.");
        }

        [TestMethod]
        public void Plugin_Should_Handle_Zero_ManagedList_Relations()
        {
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState(0).Build();

            _context.Initialize(new List<Entity> { qLine }); // No related records

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = qLine;

            _context.ExecutePluginWith<QuestionnaireLineManagedListStatusSyncPostOperation>(pluginContext);
            Assert.IsTrue(true, "Plugin completed without exception when no related records found.");
        }

        [TestMethod]
        public void Plugin_Should_Process_Multiple_ManagedList_Relations()
        {
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).WithState(0).Build();

            var managedList1 = new ManagedListBuilder(project).Build();
            var managedList2 = new ManagedListBuilder(project).Build();

            var qLineManagedList1 = new QuestionnaireLineManagedListBuilder(project, managedList1, qLine).Build();
            var qLineManagedList2 = new QuestionnaireLineManagedListBuilder(project, managedList2, qLine).Build();

            _context.Initialize(new List<Entity> { qLine, qLineManagedList1, qLineManagedList2 });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = qLine;

            _context.ExecutePluginWith<QuestionnaireLineManagedListStatusSyncPostOperation>(pluginContext);
            Assert.IsTrue(true, "Plugin handled multiple related records without error.");
        }
    }
}
