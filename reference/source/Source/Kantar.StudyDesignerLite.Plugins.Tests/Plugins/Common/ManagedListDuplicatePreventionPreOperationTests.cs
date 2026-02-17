using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Collections.Generic;
using Kantar.StudyDesignerLite.Plugins.Common;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Common
{
    [TestClass]
    public class ManagedListDuplicatePreventionPreOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;
        private KT_Project _project;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();

            // Arrange Project
            _project = new ProjectBuilder()
                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CreateManagedList_WithDuplicateName_ThrowsException()
        {
            // Arrange: Duplicate already exists
            var existing = new ManagedListBuilder(_project)
                .WithName("Duplicate List")
                .Build();

            // Arrange: Incoming duplicate
            var target = new ManagedListBuilder(_project)
                .WithName("Duplicate List")
                .Build();

            _context.Initialize(new List<Entity> { _project, existing });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };

            var plugin = new ManagedlistDuplicatePreventionPreOperation();

            // Act + Assert: Should throw InvalidPluginExecutionException due to duplication
            _context.ExecutePluginWith<ManagedlistDuplicatePreventionPreOperation>(pluginContext);
        }

        [TestMethod]
        public void CreateManagedList_WithUniqueName_Succeeds()
        {
            // Arrange: No existing Managed List with this name
            var target = new ManagedListBuilder(_project)
                .WithName("Unique List")
                .Build();

            _context.Initialize(new List<Entity> { _project });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };

            var plugin = new ManagedlistDuplicatePreventionPreOperation();

            // Act
            _context.ExecutePluginWith<ManagedlistDuplicatePreventionPreOperation>(pluginContext);

            // Assert: No exception, so we just verify no errors and name remains unchanged
            Assert.AreEqual("Unique List", target.GetAttributeValue<string>(KTR_ManagedList.Fields.KTR_Name));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void CreateQLineManagedList_WithDuplicateCombination_ThrowsException()
        {
            // Arrange
            var qline = new QuestionnaireLineBuilder(_project)
                .WithVariableName("Q1")
                .Build();

            var managedList = new ManagedListBuilder(_project)
                .WithName("ML 1")
                .Build();

            // Existing duplicate
            var existing = new QuestionnaireLineManagedListBuilder(_project, managedList, qline).Build();

            // Target (same combination)
            var target = new QuestionnaireLineManagedListBuilder(_project, managedList, qline).Build();

            _context.Initialize(new List<Entity> { _project, qline, managedList, existing });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };

            var plugin = new ManagedlistDuplicatePreventionPreOperation();

            // Act + Assert
            _context.ExecutePluginWith<ManagedlistDuplicatePreventionPreOperation>(pluginContext);
        }

        [TestMethod]
        public void CreateQLineManagedList_WithUniqueCombination_Succeeds()
        {
            // Arrange
            var qline1 = new QuestionnaireLineBuilder(_project)
                .WithVariableName("Q1")
                .Build();

            var qline2 = new QuestionnaireLineBuilder(_project)
                .WithVariableName("Q2")
                .Build();

            var managedList = new ManagedListBuilder(_project)
                .WithName("ML 1")
                .Build();

            // Existing with Q1
            var existing = new QuestionnaireLineManagedListBuilder(_project, managedList, qline1).Build();

            // Target with Q2 (unique combination)
            var target = new QuestionnaireLineManagedListBuilder(_project, managedList, qline2).Build();
            target["ktr_questionnaireline"] = qline2.ToEntityReference();
            target["ktr_managedlistid"] = managedList.ToEntityReference();

            _context.Initialize(new List<Entity> { _project, qline1, qline2, managedList, existing });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", target }
            };

            var plugin = new ManagedlistDuplicatePreventionPreOperation();

            // Act
            _context.ExecutePluginWith<ManagedlistDuplicatePreventionPreOperation>(pluginContext);

            // Assert
            Assert.AreEqual(qline2.Id, ((EntityReference)target["ktr_questionnaireline"]).Id);
            Assert.AreEqual(managedList.Id, ((EntityReference)target["ktr_managedlistid"]).Id);
        }

    }
}
