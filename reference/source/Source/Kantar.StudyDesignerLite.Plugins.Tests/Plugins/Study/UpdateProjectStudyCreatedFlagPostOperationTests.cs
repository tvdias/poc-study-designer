using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Study;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System.Collections.Generic;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Study
{
    [TestClass]
    public class UpdateProjectStudyCreatedFlagPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
        }

        [TestMethod]
        public void CreateStudy_Should_SetStudyCreatedTrue_OnProject()
        {
            // Arrange
            var project = new ProjectBuilder()
                .Build();

            var study = new StudyBuilder(project)
                .Build();

            _context.Initialize(new List<Entity> { project, study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", study }
            };

            // Act
            _context.ExecutePluginWith<UpdateProjectStudyCreatedFlagPostOperation>(pluginContext);

            // Assert
            var updatedProject = _context.Data[project.LogicalName][project.Id];
            Assert.IsTrue(updatedProject.Contains(KT_Project.Fields.KTR_StudyCreated));
            Assert.AreEqual(true, updatedProject[KT_Project.Fields.KTR_StudyCreated]);
        }

        [TestMethod]
        public void DeactivateLastStudy_Should_KeepStudyCreatedTrue_OnProject()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var inactiveStudy = new StudyBuilder(project)
                .WithStateCode(KT_Study_StateCode.Inactive)
                .Build();

            _context.Initialize(new List<Entity> { project, inactiveStudy });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new Entity(inactiveStudy.LogicalName) { Id = inactiveStudy.Id, [KT_Study.Fields.StateCode] = new OptionSetValue(1) } }
            };
            pluginContext.PreEntityImages = new EntityImageCollection
            {
                { "PreImage", inactiveStudy }
            };

            // Act
            _context.ExecutePluginWith<UpdateProjectStudyCreatedFlagPostOperation>(pluginContext);

            // Assert
            var updatedProject = _context.Data[project.LogicalName][project.Id];
            Assert.IsTrue(updatedProject.Contains(KT_Project.Fields.KTR_StudyCreated));
            Assert.AreEqual(true, updatedProject[KT_Project.Fields.KTR_StudyCreated]);
        }

        [TestMethod]
        public void DeleteLastStudy_Should_SetStudyCreatedFalse_OnProject()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).Build();

            _context.Initialize(new List<Entity> { project, study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Delete);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new EntityReference(study.LogicalName, study.Id) }
            };
            pluginContext.PreEntityImages = new EntityImageCollection
            {
                { "PreImage", study }
            };

            // Remove study to simulate deletion
            _context.Data[study.LogicalName].Remove(study.Id);

            // Act
            _context.ExecutePluginWith<UpdateProjectStudyCreatedFlagPostOperation>(pluginContext);

            // Assert
            var updatedProject = _context.Data[project.LogicalName][project.Id];
            Assert.IsTrue(updatedProject.Contains(KT_Project.Fields.KTR_StudyCreated));
            Assert.AreEqual(false, updatedProject[KT_Project.Fields.KTR_StudyCreated]);
        }

        [TestMethod]
        public void StudyUpdate_IrrelevantField_Should_NotThrow()
        {
            // Arrange
            var project = new ProjectBuilder()
                .Build();

            var study = new StudyBuilder(project)
                .Build();

            _context.Initialize(new List<Entity> { project, study });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", new Entity(study.LogicalName) { Id = study.Id, [KT_Study.Fields.KT_Name] = "Updated Name" } }
            };
            pluginContext.PreEntityImages = new EntityImageCollection
            {
                { "PreImage", study }
            };

            // Act
            _context.ExecutePluginWith<UpdateProjectStudyCreatedFlagPostOperation>(pluginContext);

            // Assert: StudyCreated flag should remain unchanged (default is false unless set elsewhere)
            var updatedProject = _context.Data[project.LogicalName][project.Id];
            Assert.IsTrue(updatedProject.Contains(KT_Project.Fields.KTR_StudyCreated));
        }
    }
}
