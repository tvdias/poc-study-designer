using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.ProductConfigQuestion;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.ProductConfigQuestion
{
    [TestClass]
    public class AddNewProductConfigQuestiontoProjectPostOperationTests
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
        public void WhenProjectUsesProductAndQuestionIsNew_ShouldCreateProjectProductConfig()
        {
            // Arrange
            var product = new ProductBuilder().Build();
            var question = new ConfigurationQuestionBuilder().WithName("Q1").Build();
            var project = new ProjectBuilder().WithProduct(product).Build();
            var pcq = new ProductConfigQuestionBuilder(product, question).Build();

            _context.Initialize(new List<Entity> { product, question, project });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = pcq;

            // Act
            _context.ExecutePluginWith<AddNewProductConfigQuestiontoProjectPostOperation>(pluginContext);

            // Assert
            var createdConfigs = _context.CreateQuery<KTR_ProjectProductConfig>().ToList();
            Assert.AreEqual(1, createdConfigs.Count);
            Assert.AreEqual(project.Id, ((EntityReference)createdConfigs[0][KTR_ProjectProductConfig.Fields.KTR_KT_Project]).Id);
            Assert.AreEqual(question.Id, ((EntityReference)createdConfigs[0][KTR_ProjectProductConfig.Fields.KTR_ConfigurationQuestion]).Id);
        }

        [TestMethod]
        public void WhenQuestionAlreadyExistsForProject_ShouldNotCreateDuplicate()
        {
            // Arrange
            var product = new ProductBuilder().Build();
            var question = new ConfigurationQuestionBuilder().WithName("Q1").Build();
            var project = new ProjectBuilder().WithProduct(product).Build();
            var config = new ProjectProductConfigBuilder(question, project).Build();
            var pcq = new ProductConfigQuestionBuilder(product, question).Build();

            _context.Initialize(new List<Entity> { product, question, project, config });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = pcq;

            // Act
            _context.ExecutePluginWith<AddNewProductConfigQuestiontoProjectPostOperation>(pluginContext);

            // Assert
            var configs = _context.CreateQuery<KTR_ProjectProductConfig>().ToList();
            Assert.AreEqual(1, configs.Count); // Should not create another
        }

        [TestMethod]
        public void WhenNoMatchingProjectExists_ShouldNotCreateAnything()
        {
            // Arrange
            var product = new ProductBuilder().Build();
            var question = new ConfigurationQuestionBuilder().WithName("Q1").Build();
            var unrelatedProject = new ProjectBuilder().Build(); // no product assigned
            var pcq = new ProductConfigQuestionBuilder(product, question).Build();

            _context.Initialize(new List<Entity> { product, question, unrelatedProject });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = pcq;

            // Act
            _context.ExecutePluginWith<AddNewProductConfigQuestiontoProjectPostOperation>(pluginContext);

            // Assert
            var configs = _context.CreateQuery<KTR_ProjectProductConfig>().ToList();
            Assert.AreEqual(0, configs.Count);
        }
    }
}
