using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Kantar.StudyDesignerLite.Plugins.ProductConfigQuestion;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.ProductConfigQuestion
{
    [TestClass]
    public class DeleteConfigQuestionfromProjectifDeletedfromProductPostOperationTests
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
        public void WhenConfigExistsInActiveProject_ShouldDeleteIt()
        {
            // Arrange
            var product = new ProductBuilder().Build();
            var question = new ConfigurationQuestionBuilder().WithName("Q1").Build();
            var project = new ProjectBuilder().WithProduct(product).Build();
            var config = new ProjectProductConfigBuilder(question, project).Build();
            var pcq = new ProductConfigQuestionBuilder(product, question).Build();

            _context.Initialize(new List<Entity> { product, question, project, config });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            pluginContext.PreEntityImages["PreImage"] = pcq;

            // Act
            _context.ExecutePluginWith<DeleteConfigQuestionfromProjectifDeletedfromProductPostOperation>(pluginContext);

            // Assert
            var deleted = _context.CreateQuery<KTR_ProjectProductConfig>().FirstOrDefault(x => x.Id == config.Id);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public void WhenNoMatchingConfigExists_ShouldNotThrow()
        {
            // Arrange
            var product = new ProductBuilder().Build();
            var question = new ConfigurationQuestionBuilder().Build();
            var project = new ProjectBuilder().WithProduct(product).Build();
            var pcq = new ProductConfigQuestionBuilder(product, question).Build(); // No config created

            _context.Initialize(new List<Entity> { product, question, project });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            pluginContext.PreEntityImages["PreImage"] = pcq;

            // Act + Assert
            _context.ExecutePluginWith<DeleteConfigQuestionfromProjectifDeletedfromProductPostOperation>(pluginContext);
            var configs = _context.CreateQuery<KTR_ProjectProductConfig>().ToList();
            Assert.AreEqual(0, configs.Count);
        }

        [TestMethod]
        public void WhenNoMatchingProjectExists_ShouldNotDeleteAnything()
        {
            // Arrange
            var product = new ProductBuilder().Build();
            var question = new ConfigurationQuestionBuilder().Build();
            var unrelatedProject = new ProjectBuilder().Build(); // No product assigned
            var config = new ProjectProductConfigBuilder(question, unrelatedProject).Build();
            var pcq = new ProductConfigQuestionBuilder(product, question).Build();

            _context.Initialize(new List<Entity> { product, question, unrelatedProject, config });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Delete";
            pluginContext.PreEntityImages["PreImage"] = pcq;

            // Act
            _context.ExecutePluginWith<DeleteConfigQuestionfromProjectifDeletedfromProductPostOperation>(pluginContext);

            // Assert
            var remaining = _context.CreateQuery<KTR_ProjectProductConfig>().FirstOrDefault(x => x.Id == config.Id);
            Assert.IsNotNull(remaining);
        }

    }
}
