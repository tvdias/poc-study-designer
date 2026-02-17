using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Study;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Study
{
    [TestClass]
    public class CreateStudyPreOperationTests
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
        public void CreateStudy_WithoutMasterStudy_SetsVersionTo1()
        {

            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            // Arrange Study
            var study = new StudyBuilder(project)
                .WithName("Study 1")
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", study }
            };

            var plugin = new CreateStudyPreOperation();

            // Act
            _context.ExecutePluginWith<CreateStudyPreOperation>(pluginContext);

            // Assert
            Assert.IsTrue(study.Attributes.Contains(KT_Study.Fields.KTR_VersionNumber));
            Assert.AreEqual(1, study.GetAttributeValue<int>(KT_Study.Fields.KTR_VersionNumber));
        }

        [TestMethod]
        public void CreateStudy_WithMasterStudy_SetsVersionToHighestPlusOne()
        {
            // Arrange Project
            var project = new ProjectBuilder()
                .Build();

            // Arrange Master Study 
            var masterStudy = new StudyBuilder(project)
                .WithName("Master")
                .Build();
            // Arrange Study
            var study = new StudyBuilder(project)
                .WithName("Study v1")
                .WithMasterStudy(masterStudy)
                .Build();
            
            // Arrange Study
            var study1 = new StudyBuilder(project)
                .WithName("Study v2")
                .WithMasterStudy(masterStudy)
                .WithVersion(3)
                .Build();
            // Arrange Study
            var study2 = new StudyBuilder(project)
                .WithName("Study v2")
                .WithMasterStudy(masterStudy)
                .WithVersion(4)
                .Build();

            // 🛠️ Initialize context with parent
            _context.Initialize(new List<Entity> { masterStudy, study1, study2 });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters = new ParameterCollection
            {
                { "Target", study }
            };

            var plugin = new CreateStudyPreOperation();

            // Act
            _context.ExecutePluginWith<CreateStudyPreOperation>(pluginContext);

            // Assert
            Assert.AreEqual(5, study.GetAttributeValue<int>(KT_Study.Fields.KTR_VersionNumber));
        }
    }
}
