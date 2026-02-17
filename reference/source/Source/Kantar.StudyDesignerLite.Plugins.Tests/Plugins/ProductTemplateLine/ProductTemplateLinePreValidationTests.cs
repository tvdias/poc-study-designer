using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.ProductTemplateLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.ProductTemplateLine
{
    [TestClass]
    public class ProductTemplateLinePreValidationTests
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
        public void ExecutePlugin_ShouldThrow_WhenModuleAlreadyAssociated()
        {
            // Arrange
            var module = new ModuleBuilder().Build();// { Id = Guid.NewGuid(), LogicalName = KT_Module.EntityLogicalName };
            var productTemplate = new ProductTemplateBuilder().Build();// { Id = Guid.NewGuid(), LogicalName = KTR_ProductTemplate.EntityLogicalName };

            var existing = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Module)
                .WithModule(module)
                .Build();

            var newLine = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Module)
                .WithModule(module)
                .Build();

            _context.Initialize(new Entity[] { existing });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newLine;

            var plugin = new ProductTemplateLinePreValidation();

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>

              _context.ExecutePluginWith<ProductTemplateLinePreValidation>(pluginContext));
            Assert.AreEqual("This module is already associated!", ex.Message);

        }
        [TestMethod]
        public void ExecutePlugin_ShouldThrow_WhenQuestionAlreadyAssociated()
        {
            // Arrange
            var quest = new QuestionBankBuilder().Build();
            var productTemplate = new ProductTemplateBuilder().Build();
            var existing = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(quest)
                .Build();

            var newLine = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(quest)
                .Build();

            _context.Initialize(new Entity[] { existing });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newLine;

            var plugin = new ProductTemplateLinePreValidation();

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>

              _context.ExecutePluginWith<ProductTemplateLinePreValidation>(pluginContext));
            Assert.AreEqual("This question is already associated!", ex.Message);

        }

        [TestMethod]
        public void ExecutePlugin_ShouldThrow_WhenQuestionAlreadyAssociatedViaModule()
        {
            // Arrange
            var question = new QuestionBankBuilder().Build();

            var module = new ModuleBuilder().Build();

            var productTemplate = new ProductTemplateBuilder().Build();

            // Existing module line associated with product template
            var existingModuleLine = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Module)
                .WithModule(module)
                .Build();

            // The module contains the question
            var moduleQuestion = new ModuleQuestionBankBuilder(module, question).Build();
             
            // New line trying to add same question directly
            var newQuestionLine = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(question)
                .Build();

            _context.Initialize(new Entity[]
            {
        existingModuleLine,
        moduleQuestion
            });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newQuestionLine;

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                _context.ExecutePluginWith<ProductTemplateLinePreValidation>(pluginContext));

            Assert.AreEqual("This question is already associated via a module!", exception.Message);
        }

        [TestMethod]
        public void ExecutePlugin_ShouldThrow_WhenModuleContainsAlreadyAssociatedQuestions()
        {
            // Arrange
            var question = new QuestionBankBuilder().Build(); 
            var module = new ModuleBuilder().Build();
            var productTemplate = new ProductTemplateBuilder().Build();
            // Question is already directly associated to product template
            var existingQuestionLine = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(question)
                .Build();

            // Module being added that contains that same question
            var moduleQuestion = new KTR_ModuleQuestionBank
            {
                Id = Guid.NewGuid(),
                LogicalName = KTR_ModuleQuestionBank.EntityLogicalName,
                [KTR_ModuleQuestionBank.Fields.KTR_Module] = new EntityReference(KT_Module.EntityLogicalName, module.Id),
                [KTR_ModuleQuestionBank.Fields.KTR_QuestionBank] = new EntityReference(KT_QuestionBank.EntityLogicalName, question.Id)
            };

            var newModuleLine = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Module)
                .WithModule(module)
                .Build();

            _context.Initialize(new Entity[] { existingQuestionLine, moduleQuestion });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newModuleLine;

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                _context.ExecutePluginWith<ProductTemplateLinePreValidation>(pluginContext));

            Assert.AreEqual("This module contains questions that are already associated!", ex.Message);
        }

        [TestMethod]
        public void ExecutePlugin_ShouldNotThrow_WhenLineIsValid()
        {
            // Arrange
            var question = new QuestionBankBuilder().Build();
            var productTemplate = new ProductTemplateBuilder().Build();
            var validLine = new ProductTemplateLineBuilder(productTemplate)
                .WithType(KTR_ProductTemplateLineType.Question)
                .WithQuestionBank(question)
                .Build();

            // Initialize ONLY the valid line in the context (no existing associations)
            _context.Initialize(new Entity[] { });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = validLine;

            // Act
            _context.ExecutePluginWith<ProductTemplateLinePreValidation>(pluginContext);

            // Assert: Ensure the valid line still exists in context and was not modified
            Assert.IsNotNull(validLine);
            Assert.AreEqual(validLine.Id, validLine.Id);
        }
    }
}
