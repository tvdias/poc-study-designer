using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.ProjectProductConfigQuestionAnswer;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.ProjectProductConfigQuestionAnswer
{
    [TestClass]
    public class ProjectProductConfigQuestionAnswerPostOperationTest
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
        public void ExecuteCdsPlugin_HideQuestionDisplayRule_RunsSuccessfully()
        {
            // Arrange Product
            var product = new ProductBuilder()
                .WithName("Test Product")
                .Build();

            // Arrange Project
            var project = new ProjectBuilder()
                .WithName("Test Project")
                .WithProduct(product)
                .Build();

            // Arrange ConfigQuestion
            var configQuestion = new ConfigurationQuestionBuilder()
                .WithName("Test Config Question")
                .Build();
            var impactedConfigQuestion = new ConfigurationQuestionBuilder()
                .WithName("Impacted Question")
                .Build();

            // Arrange ProductConfigQuestion
            var productConfigQuestion = new ProductConfigQuestionBuilder(product, configQuestion)
                .Build();

            // Arrange Configuration Answer
            var configAnswer = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("Test Config Answer")
                .Build();

            // Arrange projectProductConfigQuestion
            var projectProductConfig = new ProjectProductConfigBuilder(configQuestion, project)
                .Build();
            var impactedQuestion = new ProjectProductConfigBuilder(impactedConfigQuestion, project)
                .Build();
            // Arrange 3 related answers to the impacted question
            var impactedanswer1 = new ProjectProductConfigQuestionAnswerBuilder(impactedQuestion, impactedConfigQuestion, configAnswer)
                .WithIsSelected(false)
                .Build();

            var impactedanswer2 = new ProjectProductConfigQuestionAnswerBuilder(impactedQuestion, impactedConfigQuestion, configAnswer)
                .WithIsSelected(true)
                .Build();

            var impactedanswer3 = new ProjectProductConfigQuestionAnswerBuilder(impactedQuestion, impactedConfigQuestion, configAnswer)
                .WithIsSelected(true)
                .Build();

            // Arrange projectProductConfigQuestionAnswer
            var projectProductConfigQuestionAnswer = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestion, configAnswer)
                .Build();

            // Arrange projectProductConfigQuestionAnswerPreImage
            var projectProductConfigQuestionAnswerPreImage = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestion, configAnswer)
                .WithIsSelectedAsFalse()
                .Build();

            // Arrange projectProductConfigQuestionAnswerPostImage
            var projectProductConfigQuestionAnswerPostImage = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestion, configAnswer)
               .WithIsSelectedAsTrue(projectProductConfig)
               .Build();

            // Arrange Product Config Question Display Rule
            var productConfigQuestionDisplayRule = new ProductConfigQuestionDisplayRuleBuilder(productConfigQuestion, configQuestion, configAnswer, impactedConfigQuestion)
                .DisplayRuleWithHideQuestionSetting()
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = projectProductConfigQuestionAnswer;
            pluginContext.PreEntityImages["Image"] = projectProductConfigQuestionAnswerPreImage;
            pluginContext.PostEntityImages["Image"] = projectProductConfigQuestionAnswerPostImage;

            _context.Initialize(new Entity[] { project, configQuestion, productConfigQuestion, configAnswer, projectProductConfig, projectProductConfigQuestionAnswer, productConfigQuestionDisplayRule, impactedQuestion,impactedanswer1,impactedanswer2,impactedanswer3 });
            // Act
            var projectProductConfigQuestionPostOpt = _context.ExecutePluginWith<ProjectProductConfigQuestionAnswerPostOperation>(pluginContext);

            // Assert
            var updatedProjectProductConfig = _service.Retrieve(KTR_ProjectProductConfig.EntityLogicalName, impactedQuestion.Id, new ColumnSet(KTR_ProjectProductConfig.Fields.StateCode));
            // var updatedProjectProducConfigAnswer= _service.RetrieveMultiple(KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName, projectProductConfigQuestionAnswer.Id,new ColumnSet(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_IsSelected));
            Assert.AreEqual(1, ((OptionSetValue)updatedProjectProductConfig[KTR_ProjectProductConfig.Fields.StateCode]).Value); // Status is Inactive
            var relatedAnswers = _service.RetrieveMultiple(new QueryExpression
            {
                EntityName = KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName,
                ColumnSet = new ColumnSet(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_IsSelected),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                            new ConditionExpression(
                            KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ProjectProductConfigQuestion,
                            ConditionOperator.Equal,
                            impactedQuestion.Id)
                    }
                }
            });

            Assert.AreEqual(3, relatedAnswers.Entities.Count, "All 3 answers should exist.");

            foreach (var answer in relatedAnswers.Entities)
            {
                Assert.IsFalse(answer.GetAttributeValue<bool>(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_IsSelected),
                    $"Answer {answer.Id} should be unselected.");

            }
        }
        
        [TestMethod]
        public void ExecuteCdsPlugin_DisplayQuestionDisplayRule_RunsSuccessfully()
        {
            // Arrange Product
            var product = new ProductBuilder()
                .WithName("Test Product")
                .Build();

            // Arrange Project
            var project = new ProjectBuilder()
                .WithName("Test Project")
                .WithProduct(product)
                .Build();

            // Arrange ConfigQuestion
            var configQuestion = new ConfigurationQuestionBuilder()
                .WithName("Test Config Question")
                .Build();
            var impactedConfigQuestion = new ConfigurationQuestionBuilder()
               .WithName("Impacted Question")
               .Build();

            // Arrange ProductConfigQuestion
            var productConfigQuestion = new ProductConfigQuestionBuilder(product, configQuestion)
                .Build();

            // Arrange Configuration Answer
            var configAnswer = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("Test Config Answer")
                .Build();

            // Arrange projectProductConfigQuestion
            var projectProductConfig = new ProjectProductConfigBuilder(configQuestion, project)
                .Build();
            var impactedQuestion = new ProjectProductConfigBuilder(impactedConfigQuestion, project)
              .Build();
            // Arrange projectProductConfigQuestionAnswer
            var projectProductConfigQuestionAnswer = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestion, configAnswer)
                .Build();

            // Arrange projectProductConfigQuestionAnswerPreImage
            var projectProductConfigQuestionAnswerPreImage = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestion, configAnswer)
                .WithIsSelectedAsFalse()
                .Build();

            // Arrange projectProductConfigQuestionAnswerPostImage
            var projectProductConfigQuestionAnswerPostImage = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestion, configAnswer)
               .WithIsSelectedAsTrue(projectProductConfig)
               .Build();

            // Arrange Product Config Question Display Rule
            var productConfigQuestionDisplayRule = new ProductConfigQuestionDisplayRuleBuilder(productConfigQuestion, configQuestion, configAnswer, impactedConfigQuestion)
                .DisplayRuleWithDisplayQuestionSetting()
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = projectProductConfigQuestionAnswer;
            pluginContext.PreEntityImages["Image"] = projectProductConfigQuestionAnswerPreImage;
            pluginContext.PostEntityImages["Image"] = projectProductConfigQuestionAnswerPostImage;

            _context.Initialize(new Entity[] { project, configQuestion, productConfigQuestion, configAnswer, projectProductConfig, projectProductConfigQuestionAnswer, productConfigQuestionDisplayRule, impactedQuestion });
            // Act
            _context.ExecutePluginWith<ProjectProductConfigQuestionAnswerPostOperation>(pluginContext);

            // Assert
            var updatedProjectProductConfig = _service.Retrieve(KTR_ProjectProductConfig.EntityLogicalName, impactedQuestion.Id, new ColumnSet(KTR_ProjectProductConfig.Fields.StateCode));
            Assert.AreEqual(0, ((OptionSetValue)updatedProjectProductConfig[KTR_ProjectProductConfig.Fields.StateCode]).Value);
        }
        [TestMethod]
        public void ExecuteCdsPlugin_HideAnswerDisplayRule_RunsSuccessfully()
        {
            // Arrange Product
            var product = new ProductBuilder()
                .WithName("Test Product")
                .Build();

            // Arrange Project
            var project = new ProjectBuilder()
                .WithName("Test Project")
                .WithProduct(product)
                .Build();

            // Arrange ConfigQuestion
            var configQuestion = new ConfigurationQuestionBuilder()
                .WithName("Test Config Question")
                .Build();
            var impactedConfigQuestion = new ConfigurationQuestionBuilder()
              .WithName("Impacted Question")
              .Build();

            // Arrange ProductConfigQuestion
            var productConfigQuestion = new ProductConfigQuestionBuilder(product, configQuestion)
                .Build();

            // Arrange Configuration Answer
            var configAnswer = new ConfigurationAnswerBuilder(configQuestion)
                .WithName("Test Config Answer")
                .Build();
            var impactedConfigAnswer = new ConfigurationAnswerBuilder(impactedConfigQuestion)
             .WithName("Impacted Answer")
             .Build();

            // Arrange projectProductConfigQuestion
            var projectProductConfig = new ProjectProductConfigBuilder(configQuestion, project)
                .Build();
            var impactedQuestion = new ProjectProductConfigBuilder(impactedConfigQuestion, project)
             .Build();

            // Arrange projectProductConfigQuestionAnswer
            var projectProductConfigQuestionAnswer = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestion, configAnswer)
                .Build();
            var impactedAnswer = new ProjectProductConfigQuestionAnswerBuilder(impactedQuestion,impactedConfigQuestion, impactedConfigAnswer)
             .Build();

            // Arrange projectProductConfigQuestionAnswerPreImage
            var projectProductConfigQuestionAnswerPreImage = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestion, configAnswer)
                .WithIsSelectedAsFalse()
                .Build();

            // Arrange projectProductConfigQuestionAnswerPostImage
            var projectProductConfigQuestionAnswerPostImage = new ProjectProductConfigQuestionAnswerBuilder(projectProductConfig, configQuestion, configAnswer)
               .WithIsSelectedAsTrue(projectProductConfig)
               .Build();

            var productConfigQuestionDisplayRule = new ProductConfigQuestionDisplayRuleBuilder(productConfigQuestion, configQuestion, configAnswer, impactedConfigQuestion)
                .WithImpactedAnswer(impactedConfigAnswer)
                .DisplayRuleWithDisplayAnswerSetting()
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = projectProductConfigQuestionAnswer;
            pluginContext.PreEntityImages["Image"] = projectProductConfigQuestionAnswerPreImage;
            pluginContext.PostEntityImages["Image"] = projectProductConfigQuestionAnswerPostImage;

            _context.Initialize(new Entity[] {project, configQuestion, productConfigQuestion, configAnswer, projectProductConfig, projectProductConfigQuestionAnswer, productConfigQuestionDisplayRule, impactedQuestion, impactedAnswer});
            
            // Act
            _context.ExecutePluginWith<ProjectProductConfigQuestionAnswerPostOperation>(pluginContext);

            // Assert
            var updatedProjectProductConfigQuestionAnswer = _service.Retrieve(KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName, impactedAnswer.Id, new ColumnSet(KTR_ProjectProductConfigQuestionAnswer.Fields.StateCode));
            Assert.AreEqual(1, ((OptionSetValue)updatedProjectProductConfigQuestionAnswer[KTR_ProjectProductConfigQuestionAnswer.Fields.StateCode]).Value);
            Assert.AreNotEqual(0, ((OptionSetValue)updatedProjectProductConfigQuestionAnswer[KTR_ProjectProductConfigQuestionAnswer.Fields.StateCode]).Value);
        }
    }
}
