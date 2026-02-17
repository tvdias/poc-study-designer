using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Project;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Project
{
    [TestClass]
    public class CopyProductConfigurationQuestionToProjectPostOperationTests
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
        public void ExecutePlugin_WhenProductAssigned_ShouldCopyQuestionsAndAnswers()
        {
            var product = new ProductBuilder().Build();
            var question = new ConfigurationQuestionBuilder().Build();
            var answer = new ConfigurationAnswerBuilder(question).Build();
            var productConfig = new ProductConfigQuestionBuilder(product, question).Build();
            var project = new ProjectBuilder().Build();

            _context.Initialize(new List<Entity> { product, question, answer, productConfig, project });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = new Entity(KT_Project.EntityLogicalName)
            {
                Id = project.Id,
                [KT_Project.Fields.KTR_Product] = product.ToEntityReference()
            };

            _context.ExecutePluginWith<CopyProductConfigurationQuestionToProjectPostOperation>(pluginContext);

            var copiedConfigs = _context.CreateQuery(KTR_ProjectProductConfig.EntityLogicalName).ToList();
            var copiedAnswers = _context.CreateQuery(KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName).ToList();

            Assert.AreEqual(1, copiedConfigs.Count);
            Assert.AreEqual(1, copiedAnswers.Count);
        }

        [TestMethod]
        public void ExecutePlugin_WhenProductRemoved_ShouldDeleteExistingConfigs()
        {
            var project = new ProjectBuilder().Build();
            var question = new ConfigurationQuestionBuilder().Build();
            var existingConfig = new ProjectProductConfigBuilder(question, project).Build();

            _context.Initialize(new List<Entity> { project, question, existingConfig });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = new Entity(KT_Project.EntityLogicalName)
            {
                Id = project.Id,
                [KT_Project.Fields.KTR_Product] = null
            };

            _context.ExecutePluginWith<CopyProductConfigurationQuestionToProjectPostOperation>(pluginContext);

            var configs = _context.CreateQuery(KTR_ProjectProductConfig.EntityLogicalName).ToList();
            Assert.AreEqual(0, configs.Count);
        }

        [TestMethod]
        public void ExecutePlugin_WhenNoProductField_ShouldDoNothing()
        {
            var project = new ProjectBuilder().Build();
            _context.Initialize(new List<Entity> { project });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = new Entity(KT_Project.EntityLogicalName) { Id = project.Id };

            _context.ExecutePluginWith<CopyProductConfigurationQuestionToProjectPostOperation>(pluginContext);

            var configs = _context.CreateQuery(KTR_ProjectProductConfig.EntityLogicalName).ToList();
            var answers = _context.CreateQuery(KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName).ToList();

            Assert.AreEqual(0, configs.Count);
            Assert.AreEqual(0, answers.Count);
        }

        [TestMethod]
        public void ExecutePlugin_WhenProductHasNoConfigQuestions_ShouldDoNothing()
        {
            var product = new ProductBuilder().Build();
            var project = new ProjectBuilder().Build();
            _context.Initialize(new List<Entity> { product, project });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Update";
            pluginContext.InputParameters["Target"] = new Entity(KT_Project.EntityLogicalName)
            {
                Id = project.Id,
                [KT_Project.Fields.KTR_Product] = product.ToEntityReference()
            };

            _context.ExecutePluginWith<CopyProductConfigurationQuestionToProjectPostOperation>(pluginContext);

            Assert.AreEqual(0, _context.CreateQuery(KTR_ProjectProductConfig.EntityLogicalName).Count());
            Assert.AreEqual(0, _context.CreateQuery(KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName).Count());
        }
    }
}
